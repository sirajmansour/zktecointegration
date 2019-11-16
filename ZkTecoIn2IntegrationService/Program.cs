using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ZkTecoIn2IntegrationService
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Started");
#if DEBUG
            ServiceRunner server = null;
            try
            {
                server = new ServiceRunner();
                server.Start();
                Console.ReadLine();
            }
            finally
            {
                server.Stop();
            }

#else
            WindowsServiceConfigurator.Configure();
#endif
        }
    }
}
