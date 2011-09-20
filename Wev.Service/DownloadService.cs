using System;
using System.Collections.Generic;
using System.Text;
using Nancy;
using System.IO;
using System.Xml.Serialization;
using Wev.Core;
using System.Linq;

namespace Wev.Service
{
	public class DownloadModule : NancyModule
	{
		public DownloadModule()
		{
			Get["/"]  = x=> DownloadXml(x);
		}

		/// <summary>
		/// Retrieves the event log entries for the machine this app is running on, and returns them in serialized format.
		/// This route is called used by the 'parent' site to aggregate all the events.
		/// </summary>
		public Response DownloadXml(dynamic p)
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
					Message = "Unable to DownloadXML:" + e.ToString(),
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

			FileStreamResponse result = new FileStreamResponse(stream, "text/xml", "events.xml");
			return result;
		}
	}
}
