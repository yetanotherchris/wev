using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using System.Web;
using System.Net;

namespace Wev.Core
{
	/// <summary>
	/// Performs event log related tasks such as reading and serializing logs to <see cref="EventSummary"/> objects.
	/// This class is a singleton class and cannot be instantiated.
	/// </summary>
	public sealed class LogWorker
	{
		private IEnumerable<EntrySummary> _cachedEntries = null;
		private DateTime _lastUpdate;
		private object _lockObject = new object();

		/// <summary>
		/// Gets the file system path to the App_Data root, equivalent of Server.MapPath("~/App_Data/")
		/// </summary>
		public string AppDataPath { get; set; }

		private LogWorker()
		{
		}

		/// <summary>
		/// The current instance of the LogWorker as a Singleton to ensure thread safety if there's multiple requests.
		/// </summary>
		public static LogWorker Current
		{
			get
			{
				return Nested.instance;
			}
		}

		class Nested
		{
			// The famous Jon Skeet Singleton:
			// Explicit static constructor to tell C# compiler
			// not to mark type as beforefieldinit
			static Nested()
			{
			}

			internal static readonly LogWorker instance = new LogWorker();
		}

		/// <summary>
		/// All event log entries for all servers. This property is null until <see cref="ReadEventLogs"/> is called.
		/// </summary>
		public IEnumerable<EntrySummary> CachedEntries
		{
			get
			{
				return _cachedEntries;
			}
		}

		/// <summary>
		/// All date/time of the last update for the <see cref="CachedEntries"/>
		/// </summary>
		public DateTime LastUpdate
		{
			get
			{
				return _lastUpdate;
			}
		}

		/// <summary>
		/// Clears the cached list of event logs.
		/// </summary>
		public void ClearCache()
		{
			lock (_lockObject)
			{
				_cachedEntries = null;
			}
		}

		/// <summary>
		/// Reads all event logs from the list of servers in the web.config, which can include the local event log.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<EntrySummary> ReadEventLogs()
		{
			List<EntrySummary> list = new List<EntrySummary>();

			lock (_lockObject)
			{
				if (_cachedEntries != null)
					return _cachedEntries;

				foreach (ServerElement element in WevSection.Current.Servers)
				{
					string filename = string.Format(@"{0}\{1}.xml", AppDataPath, element.DisplayName);

					try
					{
						if (File.Exists(filename))
						{
							File.Delete(filename);
						}
					}
					catch (IOException e)
					{
						throw new LogWorkerException(string.Format("An IO exception occurred when deleting the serialized event log {0}", filename), e);
					}

					IEnumerable<EntrySummary> downloadList = new List<EntrySummary>();
					if (element.Url.ToLower().Contains("localhost") || element.Url.ToLower().Contains("127.0.0.1"))
					{
						downloadList = ReadLocalLog(element.DisplayName);
					}
					else
					{
						downloadList = ReadRemoteLogs(element.Url, element.DisplayName, filename);
					}

					list.AddRange(downloadList);
				}

				_cachedEntries = list;
				_lastUpdate = DateTime.UtcNow;
			}

			return list;
		}

		/// <summary>
		/// Reads all event log entries from the Application log.
		/// </summary>
		/// <returns>An IEnumerable of <see cref="EntrySummary"/> objects</returns>
		public IEnumerable<EntrySummary> ReadLocalLog(string machineDisplayName = "")
		{
			List<EntrySummary> list = new List<EntrySummary>();

			EventLog applicationLog = EventLog.GetEventLogs().First(e => e.Log == "Application");
			foreach (EventLogEntry entry in applicationLog.Entries.Cast<EventLogEntry>()) // cast as ometimes a System.Object is returned
			{
				list.Add(new EntrySummary(entry));
			}

			// Replace the machine name entries with the display name from the config.
			if (!string.IsNullOrEmpty(machineDisplayName))
			{
				foreach (EntrySummary summary in list)
				{
					summary.MachineDisplayName = machineDisplayName;
				}
			}

			return list;
		}

		/// <summary>
		/// Reads a remote log from a server hosting WEV.
		/// </summary>
		/// <param name="address">The server address to retrieve</param>
		/// <param name="machineDisplayName">The machine name for these entries, which is replaced.</param>
		/// <param name="saveAsfilename">The filename to save the requested serialized XML file to</param>
		private IEnumerable<EntrySummary> ReadRemoteLogs(string address, string machineDisplayName, string saveAsfilename)
		{
			List<EntrySummary> list = new List<EntrySummary>();

			try
			{
				WebClient client = new WebClient();
				client.Encoding = Encoding.Unicode;
				client.UseDefaultCredentials = true;
				client.DownloadFile(address, saveAsfilename);				

				using (FileStream stream = new FileStream(saveAsfilename, FileMode.Open, FileAccess.Read))
				{
					StreamReader reader = new StreamReader(stream, Encoding.Unicode);
					XmlSerializer serializer = new XmlSerializer(typeof(List<EntrySummary>));
					list = (List<EntrySummary>)serializer.Deserialize(reader);

					// Replace all machine name entries with the display name from the config.
					if (!string.IsNullOrEmpty(machineDisplayName))
					{
						foreach (EntrySummary summary in list)
						{
							summary.MachineDisplayName = machineDisplayName;
						}
					}
				}
			}
			catch (IOException e)
			{
				throw new LogWorkerException(string.Format("An IO exception occurred when de-serializing the remote event log from {0} ({1})", address, saveAsfilename), e);
			}
			catch (WebException e)
			{
				if (!WevSection.Current.IgnoreRemoteErrors)
				{
					throw new LogWorkerException(string.Format(@"A WebException occurred when downloading the remote event log from {0} ({1}). 
									If the error message is a 404 response this may indicate an exception has occured on the remote server.", address, saveAsfilename), e);
				}
			}

			return list;
		}
	}
}
