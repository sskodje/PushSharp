using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PushSharp.Core;

namespace PushSharp.Android
{
	public class FcmPushService : PushServiceBase
	{
		public FcmPushService(FcmPushChannelSettings channelSettings)
			: this(default(IPushChannelFactory), channelSettings, default(IPushServiceSettings))
		{
		}

		public FcmPushService(FcmPushChannelSettings channelSettings, IPushServiceSettings serviceSettings)
			: this(default(IPushChannelFactory), channelSettings, serviceSettings)
		{
		}

		public FcmPushService(IPushChannelFactory pushChannelFactory, FcmPushChannelSettings channelSettings)
			: this(pushChannelFactory, channelSettings, default(IPushServiceSettings))
		{
		}

		public FcmPushService(IPushChannelFactory pushChannelFactory, FcmPushChannelSettings channelSettings, IPushServiceSettings serviceSettings)
			: base(pushChannelFactory ?? new FcmPushChannelFactory(), channelSettings, serviceSettings)
		{
		}
	}

	public class FcmPushChannelFactory : IPushChannelFactory
	{
		public IPushChannel CreateChannel(IPushChannelSettings channelSettings)
		{
			if (!(channelSettings is FcmPushChannelSettings))
				throw new ArgumentException("channelSettings must be of type " + typeof(FcmPushChannelSettings).Name);

			return new FcmPushChannel(channelSettings as FcmPushChannelSettings);
		}
	}
}
