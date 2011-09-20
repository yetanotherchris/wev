using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using Nancy.Hosting.Self;
using System.Configuration;

namespace Wev.Service
{
	public partial class WevService : ServiceBase
	{
		private NancyHost _nancy;

		public WevService()
		{
			InitializeComponent();
			EventLog.Log = "Application";
		}

		protected override void OnStart(string[] args)
		{
			int port = int.Parse(ConfigurationManager.AppSettings["port"]);
			_nancy = new NancyHost(new Uri("http://localhost:" + port));
			_nancy.Start();
		}

		protected override void OnStop()
		{
			_nancy.Stop();
		}	
	}
}
