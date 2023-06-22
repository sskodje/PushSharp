using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PushSharp.Core;

namespace PushSharp.Android
{
    public class FcmPushChannelSettings : IPushChannelSettings
    {
        private static String MESSAGING_SCOPE = "https://www.googleapis.com/auth/firebase.messaging";
        private static String[] SCOPES = { MESSAGING_SCOPE };
        GoogleCredential _credentials;
        public FcmPushChannelSettings(string credentialsFilePath, string optionalApplicationIdPackageName = "")
        {

            this.CredentialsFilePath = credentialsFilePath;
            this.ApplicationIdPackageName = optionalApplicationIdPackageName;

            using (var file = System.IO.File.OpenText(credentialsFilePath))
            using (var reader = new JsonTextReader(file))
            {
                var jsonData = (JObject)JToken.ReadFrom(reader);
                string projectId = jsonData["project_id"].ToString();
                this.FcmUrl = $"https://fcm.googleapis.com/v1/projects/{projectId}/messages:send";
                this.SenderID = projectId;
            }
            this.ValidateServerCertificate = true;

            _credentials = GoogleCredential.FromFile(CredentialsFilePath).CreateScoped(SCOPES);
        }

        public string SenderID { get; private set; }
        public string CredentialsFilePath { get; private set; }
        public string ApplicationIdPackageName { get; private set; }

        public bool ValidateServerCertificate { get; set; }

        public string FcmUrl { get; set; }

        public void OverrideUrl(string url)
        {
            FcmUrl = url;
        }
        internal string GetAccessToken()
        {
            string token = _credentials.UnderlyingCredential.GetAccessTokenForRequestAsync().Result;
            return token;
        }
    }
}
