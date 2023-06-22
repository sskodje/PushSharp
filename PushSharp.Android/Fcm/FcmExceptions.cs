using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PushSharp.Android
{
	public class FcmMessageTransportException : Exception
	{
		public FcmMessageTransportException(string message, FcmMessageTransportResponse response)
			: base(message)
		{
			this.Response = response;
		}

		public FcmMessageTransportResponse Response
		{
			get;
			private set;
		}
	}

	public class FcmBadRequestTransportException : FcmMessageTransportException
	{
		public FcmBadRequestTransportException(FcmMessageTransportResponse response)
			: base("Bad Request or Malformed JSON", response)
		{
		}
	}

	public class FcmAuthenticationErrorTransportException : FcmMessageTransportException
	{
		public FcmAuthenticationErrorTransportException(FcmMessageTransportResponse response)
			: base("Authentication Failed", response)
		{
		}
	}

	public class FcmServiceUnavailableTransportException : FcmMessageTransportException
	{
		public FcmServiceUnavailableTransportException(TimeSpan retryAfter, FcmMessageTransportResponse response)
			: base("Service Temporarily Unavailable.  Please wait the retryAfter amount and implement an Exponential Backoff", response)
		{
			this.RetryAfter = retryAfter;
		}

		public TimeSpan RetryAfter
		{
			get;
			private set;
		}
	}
}
