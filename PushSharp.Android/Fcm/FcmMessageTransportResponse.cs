using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace PushSharp.Android
{
	public class FcmMessageTransportResponse
	{
		public FcmMessageTransportResponse()
		{
			this.MulticastId = -1;
			this.NumberOfSuccesses = 0;
			this.NumberOfFailures = 0;
			this.NumberOfCanonicalIds = 0;
			this.Message = null;
			this.Results = new List<FcmMessageResult>();
			this.ResponseCode = FcmMessageTransportResponseCode.Ok;
		}

		[JsonProperty("multicast_id")]
		public long MulticastId { get; set; }

		[JsonProperty("success")]
		public long NumberOfSuccesses { get; set; }
		
		[JsonProperty("failure")]
		public long NumberOfFailures { get; set; }

		[JsonProperty("canonical_ids")]
		public long NumberOfCanonicalIds { get; set; }

		[JsonIgnore]
		public FcmNotification Message { get; set; }

		[JsonProperty("results")]
		public List<FcmMessageResult> Results { get; set; }

		[JsonIgnore]
		public FcmMessageTransportResponseCode ResponseCode { get; set; }
	}

    public enum FcmMessageTransportResponseCode
    {
        Ok,
        Error,
        BadRequest,
        ServiceUnavailable,
        InvalidAuthToken,
        InternalServiceError
    }

    public enum FcmMessageTransportResponseStatus
    {
        Ok,
        Error,
        QuotaExceeded,
        DeviceQuotaExceeded,
        InvalidRegistration,
        NotRegistered,
        MessageTooBig,
        MissingCollapseKey,
        MissingRegistrationId,
        Unavailable,
        MismatchSenderId,
        CanonicalRegistrationId,
        InvalidDataKey,
        InvalidTtl,
        InternalServerError
    }
}
