using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wev.Core
{
	/// <summary>
	/// A serializable form of the EventLogEntryType enum.
	/// </summary>
	public enum EntryType
	{
		None			= 0,
		Error			= 1,
		Warning			= 2,
		Information		= 4,
		SuccessAudit	= 8,
		FailureAudit	= 16
	}
}