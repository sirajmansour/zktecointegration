using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZkTecoIn2IntegrationService
{
    public class SessionPool
    {
        public static Dictionary<int, Session> All = new Dictionary<int, Session>();

        public static void Build()
        {
            var machineConfigs = ConfigurationManager.AppSettings.AllKeys
                             .Where(key => int.TryParse(key,out _))
                             .ToDictionary(key => key,key => ConfigurationManager.AppSettings[key]);

            foreach (var machineConfig in machineConfigs)
            {
                var parts = machineConfig.Value.Split('/');

                var ipAndPort = parts[0].Split(':');

                string ip = ipAndPort[0];
                int port = int.Parse(ipAndPort[1]);
                int commKey = int.Parse(parts[1]);
                int machineNumber = int.Parse(machineConfig.Key);
                var s = new Session(ip, port, commKey, machineNumber);
                //s.Connect();
                All.Add(machineNumber, s);
            }
        }
    }
}
