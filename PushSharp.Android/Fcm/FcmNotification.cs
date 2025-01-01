using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;
using PushSharp.Core;

namespace PushSharp.Android
{
    public class FcmNotification : Notification
    {
        public static FcmNotification ForSingleResult(FcmMessageTransportResponse response, int resultIndex)
        {
            var result = new FcmNotification();
            result.Tag = response.Message.Tag;
            result.To = response.Message.To;
            result.CollapseKey = response.Message.CollapseKey;
            result.JsonData = response.Message.JsonData;
            return result;
        }

        public static FcmNotification ForSingleRegistrationId(FcmNotification msg, string registrationId)
        {
            var result = new FcmNotification();
            result.Tag = msg.Tag;
            result.To = registrationId;
            result.CollapseKey = msg.CollapseKey;
            result.JsonData = msg.JsonData;
            return result;
        }

        public FcmNotification()
        {
            this.CollapseKey = string.Empty;
            this.JsonData = string.Empty;
            this.Priority = "Normal";
        }


        /// <summary>
        /// Priority can be high or normal. High is always sendt, even to dozed clients.
        /// </summary>
        public string Priority
        {
            get;
            set;
        }

        /// <summary>
        /// Only the latest message with the same collapse key will be delivered
        /// </summary>
        public string CollapseKey
        {
            get;
            set;
        }

        /// <summary>
        /// JSON Payload to be sent in the message
        /// </summary>
        public string JsonData
        {
            get;
            set;
        }

        /// <summary>
        /// Time in seconds that a message should be kept on the server if the device is offline.  Default is 4 weeks.
        /// </summary>
        public int? TimeToLive
        {
            get;
            set;
        }

        public string To { get; set; }




        internal virtual string GetJson()
        {
            var messageJson = new JObject();

            messageJson["token"] = this.To;

            var androidJson = new JObject();
            androidJson["priority"] = Priority;
            if (!string.IsNullOrEmpty(this.CollapseKey))
                androidJson["collapse_key"] = this.CollapseKey;
            if (this.TimeToLive.HasValue)
                androidJson["ttl"] = $"{this.TimeToLive.Value}s";
            messageJson["android"] = androidJson;

            var webPushJson = new JObject();
            var headersJson = new JObject();
            headersJson["Urgency"] = Priority;
            if (this.TimeToLive.HasValue)
                headersJson["TTL"] = this.TimeToLive.Value.ToString();
            if (!string.IsNullOrEmpty(this.CollapseKey))
                headersJson["Token"] = this.CollapseKey;
            webPushJson["headers"] = headersJson;
            messageJson["webpush"] = webPushJson;
            if (!string.IsNullOrEmpty(this.JsonData))
            {
                var jsonData = JObject.Parse(this.JsonData);

                if (jsonData != null)
                    messageJson["data"] = jsonData;
            }

            JObject json = new JObject();
            json["message"] = messageJson;
            return json.ToString();
        }

        public override string ToString()
        {
            return GetJson();
        }
    }
}
