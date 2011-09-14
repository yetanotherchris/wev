using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wev.Core
{
	/// <summary>
	/// Represents an error that has occured within the <see cref="LogWorker"/> class.
	/// </summary>
	public class LogWorkerException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LogWorkerException"/> class.
		/// </summary>
		/// <param name="message">The error message.</param>
		/// <param name="innerException">The inner exception.</param>
		public LogWorkerException(string message,Exception innerException) : base(message,innerException)
		{
		}
	}
}
