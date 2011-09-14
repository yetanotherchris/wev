using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace Wev.Core
{
	public class ServerElement : ConfigurationElement
	{
		/// <summary>
		/// This should be the download Url of the remote server that is running Wev, for example
		/// http://10.10.20.20/Wev/home/downloadxml
		/// </summary>
		[ConfigurationProperty("url", IsKey = true, IsRequired = true)]
		public string Url
		{
			get
			{
				return (string)this["url"];
			}
		}

		/// <summary>
		/// The computer name that will be displayed in the list for this server. If this property is blank,
		/// the event log MachineName property is used instead.
		/// </summary>
		[ConfigurationProperty("displayName", IsRequired = false)]
		public string DisplayName
		{
			get
			{
				return (string)this["displayName"];
			}
		}
	}
}