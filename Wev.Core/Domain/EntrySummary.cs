using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;
using System.Xml;

namespace Wev.Core
{
	/// <summary>
	/// An serializable class that contains normalised data from an EventLogEntry instance, including a unique id.
	/// </summary>
	public class EntrySummary
	{
		/// <summary>
		/// The unique ID for the entry.
		/// </summary>
		public Guid Id
		{
			get;
			set;
		}

		/// <summary>
		/// The category of the event.
		/// </summary>
		public string Category
		{
			get;
			set;
		}

		/// <summary>
		/// The category number.
		/// </summary>
		public short CategoryNumber
		{
			get;
			set;
		}

		/// <summary>
		/// The <see cref="EntryType"/> for the event, e.g. information, warning.
		/// </summary>
		public EntryType EntryType
		{
			get;
			set;
		}

		/// <summary>
		/// The index for the event, which is relevant for the server the event came from.
		/// </summary>
		public int Index
		{
			get;
			set;
		}

		/// <summary>
		/// The id for the event, which is relevant for the server the event came from.
		/// </summary>
		public long InstanceId
		{
			get;
			set;
		}

		/// <summary>
		/// The machine name the event was logged on.
		/// </summary>
		public string MachineName
		{
			get;
			set;
		}

		/// <summary>
		/// The machine for the event, using the names assigned from the web.config configuration file.
		/// </summary>
		[XmlIgnore]
		public string MachineDisplayName
		{
			get;
			set;
		}

		/// <summary>
		/// The event message. This property is not serialized.
		/// </summary>
		/// <remarks>This property is serialized using CDataMessage to wrap it inside CDATA and remove bad characters.</remarks>
		[XmlIgnore]
		public string Message
		{
			get;
			set;
		}

		/// <summary>
		/// The XML-compliant form of <see cref="Message"/>
		/// </summary>
		[XmlElement("CDataMessage")]
		public XmlCDataSection CDataMessage
		{
			get
			{
				// This takes the message and makes it XML compliant.
				XmlDocument doc = new XmlDocument();
				return doc.CreateCDataSection(RemoveControlCharacters(Message));
			}
			set
			{
				this.Message = value.Value;
			}
		}

		/// <summary>
		/// The event source (e.g. ASP.NET 2.0)
		/// </summary>
		public string Source
		{
			get;
			set;
		}

		/// <summary>
		/// The date/time the event was generated.
		/// </summary>
		public DateTime TimeGenerated
		{
			get;
			set;
		}

		/// <summary>
		/// The date/time the event was written to the log.
		/// </summary>
		public DateTime TimeWritten
		{
			get;
			set;
		}

		/// <summary>
		/// Checks for control characters in a string, which are removed from the string.
		/// </summary>
		private string RemoveControlCharacters(string text)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] == '\t')
				{
					builder.Append("    ");
				}
				else
				{
					if (text[i] == '\r' || text[i] == '\n' || text[i] == ' ' || text[i] == '\u00a0')
					{
						builder.Append(text[i]);
					}
					else
					{
						if (char.GetUnicodeCategory(text[i]) != UnicodeCategory.Control)
						{
							builder.Append(text[i]);
						}
					}
				}
			}
			return builder.ToString();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EntrySummary"/> class. This constructor is required for serialization.
		/// </summary>
		public EntrySummary()
		{
			Id = Guid.NewGuid();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EntrySummary"/> class.
		/// </summary>
		/// <param name="entry"></param>
		public EntrySummary(EventLogEntry entry) : this()
		{
			Category = entry.Category;
			CategoryNumber = entry.CategoryNumber;
			EntryType = (EntryType)entry.EntryType; // Cast to a serializable type
			InstanceId = entry.InstanceId;
			Index = entry.Index;
			Message = entry.Message;
			Source = entry.Source;
			TimeGenerated = entry.TimeGenerated;
			TimeWritten = entry.TimeWritten;

			MachineName = entry.MachineName;
			MachineDisplayName = entry.MachineName;
		}
	}
}