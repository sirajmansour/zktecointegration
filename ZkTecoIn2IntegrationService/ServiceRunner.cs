using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZkTecoIn2IntegrationService
{
    public class ServiceRunner
    {
        private IDisposable _server;
        private PushEventSender _pushSender;

        public void Start()
        {
            string baseAddress = "http://*:9000/";
            
            string baseAddressConfig = System.Configuration.ConfigurationManager.AppSettings["baseAddress"];

            if(!string.IsNullOrWhiteSpace(baseAddressConfig))
            {
                baseAddress = baseAddressConfig;
            }

            _server = WebApp.Start<Startup>(url: baseAddress);

            SessionPool.Build();

            _pushSender = new PushEventSender();

            foreach (var session in SessionPool.All.Values)
            {
                session.UserVerified += (sender, args) =>
                {
                    _pushSender.Push(args);
                };

                session.Connect();
            }
        }

        public void Stop()
        {
            if(_server != null)
            {
                _server.Dispose();
            }
        }
    }
}
