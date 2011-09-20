using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using System.IO;

namespace Wev.Service
{
	public class FileStreamResponse : Response
	{
		public FileStreamResponse(Stream fileStream, string contentType, string fileDownloadName)
		{
			this.Headers["Last-Modified"] = DateTime.UtcNow.ToString("R");
			this.Headers["Content-Disposition"] = "attachment; filename=" + fileDownloadName;
			this.ContentType = contentType;
			this.StatusCode = HttpStatusCode.OK;
			this.Contents = GetFileContent(fileStream);
		}

		private static Action<Stream> GetFileContent(Stream fileStream)
		{
			return stream => fileStream.CopyTo(stream);
		}
	}
}
