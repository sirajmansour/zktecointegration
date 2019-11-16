using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace ZkTecoIn2IntegrationService
{
    public class MachineController : ApiController
    {
        [Route("machine/{machineNumber}/users")]
        [HttpGet]
        public HttpResponseMessage GetAllUsersByMachine(int machineNumber)
        {
            if (SessionPool.All.TryGetValue(machineNumber, out Session s))
            {
                try
                {
                    var result = s.GetAllUsers();
                    if (result.Succeeded)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, result.Data);
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"Machine error code : {result.ErrorCode}");
                    }
                }
                catch (InvalidOperationException)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable, "Connection to machine could not be made");
                }
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "machine id not found, check config file");
            }
        }

        [Route("machine/{machineNumber}/attendance")]
        [HttpGet]
        public HttpResponseMessage GetAttendanceRecords(int machineNumber, DateTime from)
        {
            if (SessionPool.All.TryGetValue(machineNumber, out Session s))
            {
                try
                {
                    var result = s.GetAttendanceRecords(from, from.AddDays(1));
                    if (result.Succeeded)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, result.Data);
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"Machine error code : {result.ErrorCode}");
                    }
                }
                catch (InvalidOperationException)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable, "Connection to machine could not be made");
                }
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "machine id not found, check config file");
            }
        }
    }
}
