using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ZkTecoIn2IntegrationService
{
    public class PushEventSender
    {
        private Uri _pushEventsUri;
        private HttpClient _httpClient;

        public PushEventSender()
        {
            _httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };
            _pushEventsUri = new Uri(ConfigurationManager.AppSettings["pushEventsUri"], UriKind.Absolute);
        }

        public void Push<T>(T @event)
            where T : class
        {
            Console.WriteLine($"Received event => {Newtonsoft.Json.JsonConvert.SerializeObject(@event)}");
            string destPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");
            try
            {
                File.AppendAllLines(destPath, new[] { $"Received event => {Newtonsoft.Json.JsonConvert.SerializeObject(@event)}" });
            }
            catch(Exception fe)
            {
                Console.WriteLine(fe.Message);
            }

            try
            {
                var response = _httpClient.PostAsJsonAsync(_pushEventsUri, @event).Result;
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                try
                {
                    File.AppendAllLines(destPath, new[] { $"Failed pushing event to [{_pushEventsUri}] with exception [{e.GetType().Name}] and message => {e.Message}" });
                }
                catch(Exception fe)
                {
                    Console.WriteLine(fe.Message);
                }
                Console.WriteLine(e.Message);
            }
        }
    }
}
