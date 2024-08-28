using System;
using System.ComponentModel;
using System.ServiceProcess;
using System.Configuration.Install;

//used to install the service with InstallUtil.exe
namespace AltStoreAutomation
{
     [RunInstaller(true)]
     public partial class ProjectInstaller : Installer
     {
          private ServiceInstaller serviceInstaller;
          private ServiceProcessInstaller processInstaller;

          public ProjectInstaller()
          {
               // Initialize the service installer
               serviceInstaller = new ServiceInstaller
               {
                    ServiceName = "AltStoreAutomation",
                    DisplayName = "AltStore Automation",
                    StartType = ServiceStartMode.Automatic
               };

               // Initialize the process installer
               processInstaller = new ServiceProcessInstaller
               {
                    Account = ServiceAccount.LocalSystem
               };

               // Add installers to the installer collection
               Installers.Add(processInstaller);
               Installers.Add(serviceInstaller);
          }
     }
}
