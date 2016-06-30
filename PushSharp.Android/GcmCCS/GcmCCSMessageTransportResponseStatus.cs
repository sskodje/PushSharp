using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PushSharp.Android
{
    public enum GcmCCSMessageTransportResponseStatus
    {
        OK,
        ERROR,
        INVALID_JSON,
        BAD_REGISTRATION,
        DEVICE_UNREGISTERED,
        BAD_ACK,
        SERVICE_UNAVAILABLE,
        INTERNAL_SERVER_ERROR,
        DEVICE_MESSAGE_RATE_EXCEEDED,
        TOPICS_MESSAGE_RATE_EXCEEDED,
        CONNECTION_DRAINING
    }
}
