using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace ZkTecoIn2IntegrationService
{
    public class TestController:ApiController
    {
        [HttpPost]
        [Route("test/push")]
        public Task Push(UserVerifiedEventArgs @event)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(@event));
            return Task.CompletedTask;
        }
    }
}
