using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace ZkTecoIn2IntegrationService
{
    internal static class WindowsServiceConfigurator
    {
        internal static void Configure()
        {
            HostFactory.Run(configure =>
            {
                configure.Service<ServiceRunner>(service =>
                {
                    service.ConstructUsing(s => new ServiceRunner());
                    service.WhenStarted(s => s.Start());
                    service.WhenStopped(s => s.Stop());
                });
            
                //Setup Account that window service use to run.  
                configure.RunAsLocalSystem();

                configure.SetServiceName("ZkTecoIn2IntergrationService");
                configure.SetDisplayName("ZkTeco In2 Intergration Service");
                configure.SetDescription("ZkTeco In2 intergration service");
            });
        }
    }
}
