using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using System.Xml.Serialization;

namespace Wev.Core
{
	public class HomeController : Controller
	{
		/// <summary>
		/// The default list view of events.
		/// </summary>
		public ActionResult Index(string server, string source, int? entryType, int? maxItems, string messageFilter, string startDate, string endDate)
		{
			IEnumerable<EntrySummary> eventList = LogWorker.Current.ReadEventLogs();

			maxItems = maxItems.GetValueOrDefault();
			if (maxItems < 1)
				maxItems = 100;

			//
			// Server filter
			//
			IEnumerable<EntrySummary> filteredList = eventList;
			if (!string.IsNullOrEmpty(server))
			{
				string loweredServer = server.ToLower();
				filteredList =
					from e in filteredList
					where e.MachineDisplayName.ToLower() == loweredServer || e.MachineName.ToLower() == loweredServer
					select e;
			}
			
			//
			// Source filter
			//
			if (!string.IsNullOrEmpty(source))
			{
				string loweredSource = source.ToLower();
				filteredList =
					from e in filteredList
					where e.Source.ToLower().Contains(loweredSource)
					select e;
			}

			//
			// Entry type filter
			//
			entryType = entryType.GetValueOrDefault();

			if (entryType > 0)
			{
				EntryType entryTypeEnum = (EntryType)entryType.Value;
				filteredList =
					from e in filteredList
					where e.EntryType == entryTypeEnum
					select e;
			}

			//
			// Start and end date filter
			//
			DateTime startDateTime = this.ParseFriendlyDate(startDate);
			DateTime endDateTime = this.ParseFriendlyDate(endDate);
			if (startDateTime > DateTime.MinValue)
			{
				filteredList =
					from e in filteredList
					where e.TimeGenerated >= startDateTime
					select e;
			}

			if (endDateTime > DateTime.MinValue)
			{
				filteredList =
					from e in filteredList
					where e.TimeGenerated <= endDateTime
					select e;
			}

			//
			// Message filter
			//
			bool filterMessages = !string.IsNullOrEmpty(messageFilter);
			List<EntrySummary> list = new List<EntrySummary>();
			foreach (EntrySummary filteredEntry in filteredList)
			{
				if (filterMessages)
				{
					string loweredMessageFilter = messageFilter.ToLower();
					string message = filteredEntry.Message;

					if (!string.IsNullOrEmpty(message))
					{ 
						message = message.ToLower();
						if (message.Contains(loweredMessageFilter))
							list.Add(filteredEntry);
					}
				}
				else
				{
					list.Add(filteredEntry);
				}
			}

			ViewData["source"] = source;
			ViewData["entryType"] = entryType.Value;
			ViewData["maxItems"] = maxItems.Value;
			ViewData["messageFilter"] = messageFilter;
			ViewData["startDate"] = startDate;
			ViewData["endDate"] = endDate;
			ViewData["lastUpdate"] = LogWorker.Current.LastUpdate;

			// Sort by the time generated and only take N items
			list = (from e in list orderby e.TimeGenerated descending select e).Take(maxItems.Value).ToList<EntrySummary>();

			return View(list);
		}

		/// <summary>
		/// Clears the current cache of events and gets all aggregated events.
		/// </summary>
		/// <returns></returns>
		public ActionResult Refresh()
		{
			LogWorker.Current.ClearCache();
			return RedirectToAction("Index");
		}

		/// <summary>
		/// Retrieves a message from the cached list of aggregated events, and returns a HTML string with the message text.
		/// </summary>
		/// <param name="id">The Guid of the event</param>
		/// <returns></returns>
		public ActionResult GetMessageText(string id)
		{
			StringBuilder builder = new StringBuilder();
			Guid entryId;
			Guid.TryParse(id, out entryId);
			EntrySummary entry = LogWorker.Current.CachedEntries.FirstOrDefault((EntrySummary e) => e.Id == entryId);
			if (entry != null)
			{
				builder.Append("<br />");
				builder.Append("<pre class=\"entrymessage\">" + entry.Message.Replace("\r", "") + "</pre>");
				builder.Append("<br style=\"clear:both;\" />");
			}
			return base.Content(builder.ToString());
		}

		/// <summary>
		/// Retrieves the event log entries for the machine this app is running on, and returns them in serialized format.
		/// This route is called used by the 'parent' site to aggregate all the events.
		/// </summary>
		public ActionResult DownloadXml()
		{
			IList<EntrySummary> list = LogWorker.Current.ReadLocalLog().ToList();
			
			// Do not dispose of the MemoryStream, as it breaks the File() method
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream, Encoding.Unicode);
			XmlSerializer serializer = new XmlSerializer(typeof(List<EntrySummary>));	
			
			try
			{
				serializer.Serialize(writer, list);
				stream.Position = 0;
			}
			catch (Exception e)
			{
				// Create an event log with the error
				list = new List<EntrySummary>();
				list.Add(new EntrySummary()
				{
					Message = "Unable to DownloadXML:" +e.ToString(),
					MachineName = Environment.MachineName,
					Source = "WEV ERROR",
					Category = "WEV",
					TimeGenerated = DateTime.Now,
					TimeWritten = DateTime.Now,
					EntryType = EntryType.Warning
				});

				stream = new MemoryStream();
				writer = new StreamWriter(stream, Encoding.Unicode);
				serializer.Serialize(writer, list);
				stream.Position = 0;
			}
			
			ActionResult result = File(stream, "text/xml", "events.xml");
			return result;
		}

		/// <summary>
		/// Turns constants such as 'today', 'yesterday', 'lastweek' into a DateTime.
		/// </summary>
		private DateTime ParseFriendlyDate(string startDate)
		{
			DateTime result = DateTime.MinValue;
			if (!string.IsNullOrEmpty(startDate))
			{
				startDate = startDate.ToLower();
				if (startDate == "today")
				{
					result = DateTime.Today;
				}
				else
				{
					if (startDate == "yesterday")
					{
						result = DateTime.UtcNow.Yesterday();
					}
					else
					{
						if (startDate == "thisweek")
						{
							result = DateTime.UtcNow.StartOfWeek();
						}
						else
						{
							if (startDate == "lastweek")
							{
								result = DateTime.UtcNow.StartOfWeek().AddDays(-7.0);
							}
							else
							{
								if (startDate == "thismonth")
								{
									result = DateTime.UtcNow.StartOfThisMonth();
								}
								else
								{
									if (startDate == "lastmonth")
									{
										result = DateTime.UtcNow.StartOfLastMonth();
									}
									else
									{
										result = DateTime.MinValue;
										DateTime.TryParse(startDate, out result);
									}
								}
							}
						}
					}
				}
			}

			return result;
		}
	}
}
