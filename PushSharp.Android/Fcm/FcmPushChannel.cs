using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security;
using System.Net.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PushSharp.Core;
using System.Reflection;

namespace PushSharp.Android
{
    public class FcmPushChannel : IPushChannel
    {
        FcmPushChannelSettings gcmSettings = null;
        long waitCounter = 0;
        static Version assemblyVerison;

        public FcmPushChannel(FcmPushChannelSettings channelSettings)
        {
            gcmSettings = channelSettings;

            if (gcmSettings != null && gcmSettings.ValidateServerCertificate)
            {
                ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            }
            else
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, policyErrs) => true; //Don't validate remote cert
            }
        }


        static FcmPushChannel()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, policyErrs) => { return true; };
            assemblyVerison = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        }

        public void SendNotification(INotification notification, SendNotificationCallbackDelegate callback)
        {
            var msg = notification as FcmNotification;

            var result = new FcmMessageTransportResponse();
            result.Message = msg;

            //var postData = msg.GetJson();

            string accessToken = gcmSettings.GetAccessToken();

            var webReq = (HttpWebRequest)WebRequest.Create(gcmSettings.FcmUrl);
            //webReq.ContentLength = postData.Length;
            webReq.Method = "POST";
            webReq.ContentType = "application/json";
            //webReq.ContentType = "application/x-www-form-urlencoded;charset=UTF-8   can be used for plaintext bodies
            webReq.UserAgent = "PushSharp (version: " + assemblyVerison.ToString() + ")";
            webReq.Headers.Add("Authorization: Bearer " + accessToken);

            webReq.BeginGetRequestStream(new AsyncCallback(requestStreamCallback), new FcmAsyncParameters()
            {
                Callback = callback,
                WebRequest = webReq,
                WebResponse = null,
                Message = msg,
                SenderAuthToken = accessToken,
                SenderId = gcmSettings.SenderID,
                ApplicationId = gcmSettings.ApplicationIdPackageName
            });
        }

        void requestStreamCallback(IAsyncResult result)
        {
            var asyncParam = result.AsyncState as FcmAsyncParameters;

            try
            {
                if (asyncParam != null)
                {
                    var wrStream = asyncParam.WebRequest.EndGetRequestStream(result);

                    using (var webReqStream = new StreamWriter(wrStream))
                    {
                        var data = asyncParam.Message.GetJson();
                        webReqStream.Write(data);
                        webReqStream.Close();
                    }

                    try
                    {
                        asyncParam.WebRequest.BeginGetResponse(new AsyncCallback(responseCallback), asyncParam);
                    }
                    catch (WebException wex)
                    {
                        asyncParam.WebResponse = wex.Response as HttpWebResponse;
                        processResponseError(asyncParam);
                    }
                }
            }
            catch (Exception ex)
            {
                asyncParam.Callback(this, new SendNotificationResult(FcmNotification.ForSingleRegistrationId(asyncParam.Message, asyncParam.Message.To), false, ex));

                Interlocked.Decrement(ref waitCounter);
            }
        }

        void responseCallback(IAsyncResult result)
        {
            var asyncParam = result.AsyncState as FcmAsyncParameters;

            try
            {
                SendNotificationResult sendResult;
                try
                {
                    asyncParam.WebResponse = asyncParam.WebRequest.EndGetResponse(result) as HttpWebResponse;
                    sendResult = processResponseOk(asyncParam);
                }
                catch (WebException wex)
                {
                    asyncParam.WebResponse = wex.Response as HttpWebResponse;
                    sendResult = processResponseError(asyncParam);
                }
                asyncParam.Callback(this, sendResult);
            }
            catch (Exception ex)
            {
                asyncParam.Callback(this, new SendNotificationResult(FcmNotification.ForSingleRegistrationId(asyncParam.Message, asyncParam.Message.To), false, ex));

                Interlocked.Decrement(ref waitCounter);
            }
        }

        SendNotificationResult processResponseOk(FcmAsyncParameters asyncParam)
        {
            asyncParam.WebResponse.Close();

            Interlocked.Decrement(ref waitCounter);
            var singleResultNotification = FcmNotification.ForSingleRegistrationId(asyncParam.Message, asyncParam.Message.To);
            return new SendNotificationResult(singleResultNotification);
        }

        SendNotificationResult processResponseError(FcmAsyncParameters asyncParam)
        {
            try
            {
                if (asyncParam == null || asyncParam.WebResponse == null)
                    throw new FcmMessageTransportException("Unknown Transport Error", new FcmMessageTransportResponse { ResponseCode = FcmMessageTransportResponseCode.Error });

                int statusCode = (int)asyncParam.WebResponse.StatusCode;
                var singleResultNotification = FcmNotification.ForSingleRegistrationId(asyncParam.Message, asyncParam.Message.To);

                if (asyncParam.WebResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    return new SendNotificationResult(singleResultNotification, false, new DeviceSubscriptonExpiredException())
                    {
                        OldSubscriptionId = asyncParam.Message.To,
                        IsSubscriptionExpired = true,
                        SubscriptionExpiryUtc = DateTime.UtcNow
                    };
                }
                else if (asyncParam.WebResponse.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    return new SendNotificationResult(singleResultNotification, true, new Exception("Unavailable Response Status"));
                }
                else if (asyncParam.WebResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new FcmAuthenticationErrorTransportException(new FcmMessageTransportResponse { ResponseCode = FcmMessageTransportResponseCode.InvalidAuthToken });
                }
                else if (asyncParam.WebResponse.StatusCode == (HttpStatusCode)429)
                {
                    throw new FcmServiceUnavailableTransportException(TimeSpan.FromMinutes(1), new FcmMessageTransportResponse
                    {
                        ResponseCode = FcmMessageTransportResponseCode.ServiceUnavailable,
                    });
                }
                else if (asyncParam.WebResponse.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new FcmBadRequestTransportException(new FcmMessageTransportResponse
                    {
                        ResponseCode = FcmMessageTransportResponseCode.BadRequest
                    });
                }
                else if (statusCode >= 500 && statusCode < 600)
                {
                    //First try grabbing the retry-after header and parsing it.
                    TimeSpan retryAfter = new TimeSpan(0, 0, 120);

                    var wrRetryAfter = asyncParam.WebResponse.GetResponseHeader("Retry-After");

                    if (!string.IsNullOrEmpty(wrRetryAfter))
                    {
                        DateTime wrRetryAfterDate = DateTime.UtcNow;

                        if (DateTime.TryParse(wrRetryAfter, out wrRetryAfterDate))
                            retryAfter = wrRetryAfterDate - DateTime.UtcNow;
                        else
                        {
                            int wrRetryAfterSeconds = 120;
                            if (int.TryParse(wrRetryAfter, out wrRetryAfterSeconds))
                                retryAfter = new TimeSpan(0, 0, wrRetryAfterSeconds);
                        }
                    }
                    var result = new FcmMessageTransportResponse();
                    //Compatability for apps written with previous versions of PushSharp. 
                    if (asyncParam.WebResponse.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        result.ResponseCode = FcmMessageTransportResponseCode.InternalServiceError;
                    }
                    else
                    {
                        //503 exponential backoff, get retry-after header
                        result.ResponseCode = FcmMessageTransportResponseCode.ServiceUnavailable;
                    }

                    throw new FcmServiceUnavailableTransportException(retryAfter, result);
                }

                throw new FcmMessageTransportException("Unknown Transport Error", new FcmMessageTransportResponse { ResponseCode = FcmMessageTransportResponseCode.Error });
            }
            finally
            {
                if (asyncParam != null && asyncParam.WebResponse != null)
                    asyncParam.WebResponse.Close();
            }
        }

        public void Dispose()
        {
            var slept = 0;
            while (Interlocked.Read(ref waitCounter) > 0 && slept <= 5000)
            {
                slept += 100;
                Thread.Sleep(100);
            }
        }

        private static bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            return policyErrors == SslPolicyErrors.None;
        }

        class FcmAsyncParameters
        {
            public SendNotificationCallbackDelegate Callback { get; set; }

            public FcmNotification Message { get; set; }

            public HttpWebRequest WebRequest { get; set; }

            public HttpWebResponse WebResponse { get; set; }

            public string SenderAuthToken { get; set; }

            public string SenderId { get; set; }

            public string ApplicationId { get; set; }
        }
    }
}
