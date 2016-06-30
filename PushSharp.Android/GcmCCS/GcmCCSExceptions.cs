using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PushSharp.Android
{
    public class GcmCCSMessageTransportException : Exception
    {
        public GcmCCSMessageTransportException(string message, GcmCCSMessageTransportResponseStatus response)
            : base(message)
        {
            this.Response = response;
        }

        public GcmCCSMessageTransportResponseStatus Response
        {
            get;
            private set;
        }
    }
}
