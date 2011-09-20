using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wev.Service;
using Nancy.Hosting.Self;

namespace Wev.Console
{
	class Program
	{
		private static NancyHost _nancy;

		static void Main(string[] args)
		{
			int port = 8090;
			_nancy = new NancyHost(new Uri("http://localhost:" + port));
			_nancy.Start();

			while (true) { }
		}
	}
}
