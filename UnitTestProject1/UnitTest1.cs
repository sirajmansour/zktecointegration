using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZkTecoIn2IntegrationService;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var x = Newtonsoft.Json.JsonConvert.SerializeObject(new AttendanceRecord() { Timestamp = DateTime.Now } );
            Console.WriteLine(x);
        }
    }
}
