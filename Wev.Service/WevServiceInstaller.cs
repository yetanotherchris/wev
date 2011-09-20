using System.ComponentModel;
using System.ServiceProcess;
using System.Configuration.Install;

namespace Wev.Service
{
	[RunInstaller(true)]
	public partial class WevServiceInstaller : Installer
	{
		public WevServiceInstaller()
        {
            ServiceProcessInstaller serviceProcessInstaller = new ServiceProcessInstaller();
            ServiceInstaller serviceInstaller = new ServiceInstaller();

            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            serviceProcessInstaller.Username = null;
            serviceProcessInstaller.Password = null;

            serviceInstaller.DisplayName = "Wev HTTP Listener";
			serviceInstaller.Description = "Wev (Web Event Viewer) service that listens for incoming HTTP requests on the port specified in the wevservice.exe.config file, returning all event logs for the system as XML.";
            serviceInstaller.StartType = ServiceStartMode.Automatic;
			serviceInstaller.ServiceName = "Wev HTTP listener";

            Installers.Add(serviceProcessInstaller);
            Installers.Add(serviceInstaller);
        }
	}
}
