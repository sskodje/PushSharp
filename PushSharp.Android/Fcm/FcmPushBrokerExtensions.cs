using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PushSharp.Android;
using PushSharp.Core;

namespace PushSharp
{
	public static class FcmPushBrokerExtensions
	{
		public static void RegisterFcmService(this PushBroker broker, FcmPushChannelSettings channelSettings, IPushServiceSettings serviceSettings = null)
		{
			RegisterFcmService (broker, channelSettings, null, serviceSettings);
		}

		public static void RegisterFcmService(this PushBroker broker, FcmPushChannelSettings channelSettings, string applicationId, IPushServiceSettings serviceSettings = null, Action<string> ret=null)
		{
			broker.RegisterService<FcmNotification>(new FcmPushService(new FcmPushChannelFactory(), channelSettings, serviceSettings), applicationId);
		}

		public static FcmNotification FcmNotification(this PushBroker broker)
		{
			return new FcmNotification();
		}
	}
}
