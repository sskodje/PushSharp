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
			assemblyVerison = System.Reflection.Assembly.GetExecutingAssembly ().GetName ().Version;
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
			webReq.UserAgent = "PushSharp (version: " + assemblyVerison.ToString () + ")";
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
				//Raise individual failures for each registration id for the notification
				foreach (var r in asyncParam.Message.RegistrationIds)
					asyncParam.Callback(this, new SendNotificationResult(FcmNotification.ForSingleRegistrationId(asyncParam.Message, r), false, ex));

				Interlocked.Decrement(ref waitCounter);
			}
		}

		void responseCallback(IAsyncResult result)
		{
			var asyncParam = result.AsyncState as FcmAsyncParameters;

			try
			{
				try
				{
					asyncParam.WebResponse = asyncParam.WebRequest.EndGetResponse(result) as HttpWebResponse;
					processResponseOk(asyncParam);
				}
				catch (WebException wex)
				{
					asyncParam.WebResponse = wex.Response as HttpWebResponse;
					processResponseError(asyncParam);
				}
			}
			catch (Exception ex)
			{
				//Raise individual failures for each registration id for the notification
				foreach (var r in asyncParam.Message.RegistrationIds)
					asyncParam.Callback(this, new SendNotificationResult(FcmNotification.ForSingleRegistrationId(asyncParam.Message, r), false, ex));

				Interlocked.Decrement(ref waitCounter);
			}
		}

		void processResponseOk(FcmAsyncParameters asyncParam)
		{
			var result = new FcmMessageTransportResponse()
			{
				ResponseCode = FcmMessageTransportResponseCode.Ok,
				Message = asyncParam.Message
			};

			//Get the response body
			var json = new JObject();

		    var str = string.Empty;

			try { str = (new StreamReader(asyncParam.WebResponse.GetResponseStream())).ReadToEnd(); }
			catch { }

		    try { json = JObject.Parse(str); }
		    catch { }

            //result.NumberOfCanonicalIds = json.Value<long>("canonical_ids");
            //result.NumberOfFailures = json.Value<long>("failure");
            //result.NumberOfSuccesses = json.Value<long>("success");

            //var jsonResults = json["results"] as JArray;

            //if (jsonResults == null)
            //	jsonResults = new JArray();

            //foreach (var r in jsonResults)
            //{
            //	var msgResult = new FcmMessageResult();

            //	msgResult.MessageId = r.Value<string>("message_id");
            //	msgResult.CanonicalRegistrationId = r.Value<string>("registration_id");
            //	msgResult.ResponseStatus = FcmMessageTransportResponseStatus.Ok;

            //	if (!string.IsNullOrEmpty(msgResult.CanonicalRegistrationId))
            //	{
            //		msgResult.ResponseStatus = FcmMessageTransportResponseStatus.CanonicalRegistrationId;
            //	}
            //	else if (r["error"] != null)
            //	{
            //		var err = r.Value<string>("error") ?? "";

            //		switch (err.ToLowerInvariant().Trim())
            //		{
            //			case "ok":
            //				msgResult.ResponseStatus = FcmMessageTransportResponseStatus.Ok;
            //				break;
            //			case "missingregistration":
            //				msgResult.ResponseStatus = FcmMessageTransportResponseStatus.MissingRegistrationId;
            //				break;
            //			case "unavailable":
            //				msgResult.ResponseStatus = FcmMessageTransportResponseStatus.Unavailable;
            //				break;
            //			case "notregistered":
            //				msgResult.ResponseStatus = FcmMessageTransportResponseStatus.NotRegistered;
            //				break;
            //			case "invalidregistration":
            //				msgResult.ResponseStatus = FcmMessageTransportResponseStatus.InvalidRegistration;
            //				break;
            //			case "mismatchsenderid":
            //				msgResult.ResponseStatus = FcmMessageTransportResponseStatus.MismatchSenderId;
            //				break;
            //			case "messagetoobig":
            //				msgResult.ResponseStatus = FcmMessageTransportResponseStatus.MessageTooBig;
            //				break;
            //                     case "invaliddatakey":
            //                         msgResult.ResponseStatus = FcmMessageTransportResponseStatus.InvalidDataKey;
            //                         break;
            //                     case "invalidttl":
            //                         msgResult.ResponseStatus = FcmMessageTransportResponseStatus.InvalidTtl;
            //                         break;
            //                     case "internalservererror":
            //                         msgResult.ResponseStatus = FcmMessageTransportResponseStatus.InternalServerError;
            //                         break;
            //			default:
            //				msgResult.ResponseStatus = FcmMessageTransportResponseStatus.Error;
            //				break;
            //		}
            //	}

            //	result.Results.Add(msgResult);

            //}
            result.Results.Add(new FcmMessageResult() {  ResponseStatus = FcmMessageTransportResponseStatus.Ok});

            asyncParam.WebResponse.Close();

			int index = 0;

			var response = result;

			//Loop through every result in the response
			// We will raise events for each individual result so that the consumer of the library
			// can deal with individual registrationid's for the notification
			foreach (var r in response.Results)
			{
				var singleResultNotification = FcmNotification.ForSingleResult(response, index);

				if (r.ResponseStatus == FcmMessageTransportResponseStatus.Ok)
				{
					//It worked! Raise success
					asyncParam.Callback(this, new SendNotificationResult(singleResultNotification));
				}
				else if (r.ResponseStatus == FcmMessageTransportResponseStatus.CanonicalRegistrationId)
				{
					//Swap Registrations Id's
					var newRegistrationId = r.CanonicalRegistrationId;
					var oldRegistrationId = string.Empty;

					if (singleResultNotification.RegistrationIds != null && singleResultNotification.RegistrationIds.Count > 0)
						oldRegistrationId = singleResultNotification.RegistrationIds[0];

					asyncParam.Callback(this, new SendNotificationResult(singleResultNotification, false, new DeviceSubscriptonExpiredException()) { OldSubscriptionId = oldRegistrationId, NewSubscriptionId = newRegistrationId, IsSubscriptionExpired = true });
				}
				else if (r.ResponseStatus == FcmMessageTransportResponseStatus.Unavailable)
				{
					asyncParam.Callback(this, new SendNotificationResult(singleResultNotification, true, new Exception("Unavailable Response Status")));
				}
				else if (r.ResponseStatus == FcmMessageTransportResponseStatus.NotRegistered)
				{
					var oldRegistrationId = string.Empty;
					
					if (singleResultNotification.RegistrationIds != null && singleResultNotification.RegistrationIds.Count > 0)
						oldRegistrationId = singleResultNotification.RegistrationIds[0];

					//Raise failure and device expired
					asyncParam.Callback(this, new SendNotificationResult(singleResultNotification, false, new DeviceSubscriptonExpiredException()) { OldSubscriptionId = oldRegistrationId, IsSubscriptionExpired = true, SubscriptionExpiryUtc = DateTime.UtcNow });
				}
				else
				{
					//Raise failure, for unknown reason
					asyncParam.Callback(this, new SendNotificationResult(singleResultNotification, false, new FcmMessageTransportException(r.ResponseStatus.ToString(), response)));
				}

				index++;
			}

			Interlocked.Decrement(ref waitCounter);
		}

		void processResponseError(FcmAsyncParameters asyncParam)
		{
			try
			{
				var result = new FcmMessageTransportResponse();
				result.ResponseCode = FcmMessageTransportResponseCode.Error;

				if (asyncParam == null || asyncParam.WebResponse == null)
					throw new FcmMessageTransportException("Unknown Transport Error", result);

                int statusCode = (int)asyncParam.WebResponse.StatusCode;

				if (asyncParam.WebResponse.StatusCode == HttpStatusCode.Unauthorized)
				{
					//401 bad auth token
					result.ResponseCode = FcmMessageTransportResponseCode.InvalidAuthToken;
					throw new FcmAuthenticationErrorTransportException(result);
				}
				else if (asyncParam.WebResponse.StatusCode == HttpStatusCode.BadRequest)
				{
					result.ResponseCode = FcmMessageTransportResponseCode.BadRequest;
					throw new FcmBadRequestTransportException(result);
				}
				else if (statusCode >= 500 && statusCode<600)
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

				throw new FcmMessageTransportException("Unknown Transport Error", result);
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

	public class FcmMessageResult
	{
		[JsonProperty("message_id", NullValueHandling = NullValueHandling.Ignore)]
		public string MessageId { get; set; }

		[JsonProperty("registration_id", NullValueHandling = NullValueHandling.Ignore)]
		public string CanonicalRegistrationId {	get; set; }

		[JsonIgnore]
		public FcmMessageTransportResponseStatus ResponseStatus { get; set; }

		[JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
		public string Error
		{
			get 
			{
				switch (ResponseStatus)
				{
					case FcmMessageTransportResponseStatus.Ok:
						return null;
					case FcmMessageTransportResponseStatus.Unavailable:
						return "Unavailable";
					case FcmMessageTransportResponseStatus.QuotaExceeded:
						return "QuotaExceeded";
					case FcmMessageTransportResponseStatus.NotRegistered:
						return "NotRegistered";
					case FcmMessageTransportResponseStatus.MissingRegistrationId:
						return "MissingRegistration";
					case FcmMessageTransportResponseStatus.MissingCollapseKey:
						return "MissingCollapseKey";
					case FcmMessageTransportResponseStatus.MismatchSenderId:
						return "MismatchSenderId";
					case FcmMessageTransportResponseStatus.MessageTooBig:
						return "MessageTooBig";
					case FcmMessageTransportResponseStatus.InvalidTtl:
						return "InvalidTtl";
					case FcmMessageTransportResponseStatus.InvalidRegistration:
						return "InvalidRegistration";
					case FcmMessageTransportResponseStatus.InvalidDataKey:
						return "InvalidDataKey";
					case FcmMessageTransportResponseStatus.InternalServerError:
						return "InternalServerError";
					case FcmMessageTransportResponseStatus.DeviceQuotaExceeded:
						return null;
					case FcmMessageTransportResponseStatus.CanonicalRegistrationId:
						return null;
					case FcmMessageTransportResponseStatus.Error:
						return "Error";
					default:
						return null;
				}
			}
		}
	}
}
