using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Configuration.Install;
using System.Reflection;

namespace Wev.Service
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the service.
		/// </summary>
		static void Main(string[] args)
		{
			if (System.Environment.UserInteractive)
			{
				try
				{
					// Arguments used by the installer
					ServiceController controller;
					string parameter = string.Concat(args);
					switch (parameter)
					{
						case "--install":
							ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
							controller = new ServiceController("Wev HTTP Listener");
							controller.Start();
							break;

						case "--uninstall":
							controller = new ServiceController("Wev HTTP Listener");
							controller.Stop();
							ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
							break;
					}
				}
				catch (Exception) { }
			}
			else
			{
				ServiceBase.Run(new WevService());
			}
		}
	}
}
