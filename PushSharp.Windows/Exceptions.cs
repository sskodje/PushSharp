using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushSharp.Windows
{
	public class WindowsNotificationSendFailureException : Exception
	{
		public WindowsNotificationSendFailureException(WindowsNotificationStatus status)
			: base()
		{
			this.NotificationStatus = status;
		}

		public WindowsNotificationStatus NotificationStatus
		{
			get;
			set;
		}
        public override string ToString()
        {
            return String.Format("WindowsNotificationSendFailureException: {0} - http status {1}",NotificationStatus?.ErrorDescription,NotificationStatus?.HttpStatus);
        }
    }
}
