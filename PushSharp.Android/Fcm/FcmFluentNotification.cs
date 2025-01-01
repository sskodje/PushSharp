using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PushSharp.Android;

namespace PushSharp
{
    public static class FcmFluentNotification
    {
        public static FcmNotification ForDeviceRegistrationId(this FcmNotification n, string deviceRegistrationId)
        {
            n.To = deviceRegistrationId;
            return n;
        }

        public static FcmNotification WithCollapseKey(this FcmNotification n, string collapseKey)
        {
            n.CollapseKey = collapseKey;
            return n;
        }

        public static FcmNotification WithTimeToLive(this FcmNotification n, int ttlSeconds)
        {
            n.TimeToLive = ttlSeconds;
            return n;
        }

        public static FcmNotification WithJson(this FcmNotification n, string json)
        {
            try { Newtonsoft.Json.Linq.JObject.Parse(json); }
            catch { throw new InvalidCastException("Invalid JSON detected!"); }

            n.JsonData = json;
            return n;
        }
        public static FcmNotification WithHighPriority(this FcmNotification n)
        {
            n.Priority = "high";
            return n;
        }
        public static FcmNotification WithNormalPriority(this FcmNotification n)
        {
            n.Priority = "normal";
            return n;
        }
        public static FcmNotification WithData(this FcmNotification n, IDictionary<string, string> data)
        {
            if (data == null)
                return n;

            var json = new Newtonsoft.Json.Linq.JObject();

            try
            {
                if (!String.IsNullOrEmpty(n.JsonData))
                    json = Newtonsoft.Json.Linq.JObject.Parse(n.JsonData);
            }
            catch { }

            foreach (var pair in data)
                json.Add(pair.Key, new Newtonsoft.Json.Linq.JValue(pair.Value));

            n.JsonData = json.ToString(Newtonsoft.Json.Formatting.None);

            return n;
        }

        public static FcmNotification WithTag(this FcmNotification n, object tag)
        {
            n.Tag = tag;
            return n;
        }
    }
}
