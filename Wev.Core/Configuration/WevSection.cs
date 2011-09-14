using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace Wev.Core
{
	/// <summary>
	/// The default configuration section for WEV.
	/// </summary>
	public class WevSection : ConfigurationSection
	{
		private static WevSection _section;
		private static List<string> _serverDisplayNames;

		/// <summary>
		/// The current instance of the section. This is not a singleton as there is no requirement for this to be threadsafe.
		/// </summary>
		public static WevSection Current
		{
			get
			{
				if (_section == null)
					_section = ConfigurationManager.GetSection("wev") as WevSection;

				return _section;
			}
		}

		/// <summary>
		/// If true, this indicates that any problems downloading remote server logs should be ignored.
		/// </summary>
		[ConfigurationProperty("ignoreRemoteErrors")]
		public bool IgnoreRemoteErrors
		{
			get
			{
				return (bool)this["ignoreRemoteErrors"];
			}
		}

		/// <summary>
		/// The list of servers to retrieve the event logs from. These machines should have Wev running on them in
		/// order to retrieve the eventlogs, as the server address is a url.
		/// </summary>
		[ConfigurationProperty("servers")]
		public ServerElementCollection Servers
		{
			get
			{
				return (ServerElementCollection)this["servers"];
			}
		}
	}
}