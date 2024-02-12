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
            result.DelayWhileIdle = response.Message.DelayWhileIdle;
            return result;
        }

        public static FcmNotification ForSingleRegistrationId(FcmNotification msg, string registrationId)
        {
            var result = new FcmNotification();
            result.Tag = msg.Tag;
            result.To = registrationId;
            result.CollapseKey = msg.CollapseKey;
            result.JsonData = msg.JsonData;
            result.DelayWhileIdle = msg.DelayWhileIdle;
            return result;
        }

        public FcmNotification()
        {
            this.CollapseKey = string.Empty;
            this.JsonData = string.Empty;
            this.DelayWhileIdle = null;
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
        /// If true, GCM will only be delivered once the device's screen is on
        /// </summary>
        public bool? DelayWhileIdle
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

        /// <summary>
        /// If true, dry_run attribute will be sent in payload causing the notification not to be actually sent, but the result returned simulating the message
        /// </summary>
        public bool? DryRun
        {
            get;
            set;
        }

        public string To { get; set; }

        /// <summary>
        /// A string that maps a single user to multiple registration IDs associated with that user. This allows a 3rd-party server to send a single message to multiple app instances (typically on multiple devices) owned by a single user.
        /// </summary>
        public string NotificationKey { get; set; }

        /// <summary>
        /// A string containing the package name of your application. When set, messages will only be sent to registration IDs that match the package name
        /// </summary>
        public string TargetPackageName { get; set; }


        internal string GetJson()
        {
            var messageJson = new JObject();

            //if (!string.IsNullOrEmpty(this.CollapseKey))
            //    messageJson["collapse_key"] = this.CollapseKey;

            //if (this.TimeToLive.HasValue)
            //    messageJson["time_to_live"] = this.TimeToLive.Value;

            //if (this.RequestDeliveryReceipt.HasValue)
            //    json["delivery_receipt_requested"] = this.RequestDeliveryReceipt.Value;

            //json["registration_ids"] = new JArray(this.RegistrationIds.ToArray());
            messageJson["token"] = this.To;

            //json["message_id"] = this.MessageID;
            var androidJson = new JObject();
            androidJson["priority"] = Priority;
            messageJson["android"] = androidJson;
            //if (DryRun.HasValue && DryRun.Value)
            //    messageJson["dry_run"] = true;

            if (!string.IsNullOrEmpty(this.JsonData))
            {
                var jsonData = JObject.Parse(this.JsonData);

                if (jsonData != null)
                    messageJson["data"] = jsonData;
            }

            //if (!string.IsNullOrWhiteSpace(this.NotificationKey))
            //    messageJson["notification_key"] = NotificationKey;

            //if (!string.IsNullOrWhiteSpace(this.TargetPackageName))
            //    messageJson["restricted_package_name"] = TargetPackageName;


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
