using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZkTecoIn2IntegrationService
{
    public enum InOutMode
    {
        CheckIn = 0,
        CheckOut = 1,
        BreakOut = 2,
        BreakIn = 3,
        OT_In = 4,
        OT_Out = 5
    }

    public enum VerifyMode
    {
        Password = 0,
        Fingerprint = 1,
        Card = 2
    }

    public enum Privilege
    {
        CommonUser = 0,
        Registrar = 1,
        Administrator = 2,
        SuperAdministrator = 3
    }

    public class AttendanceRecord
    {
        public AttendanceRecord()
        {
        }

        public string UserId { get; internal set; }
        public DateTimeOffset Timestamp { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public InOutMode InOutMode { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public VerifyMode VerifyMode { get; set; }

        public int WorkCode { get; set; }
    }

    public class UserInfo
    {
        public string EnrollmentNumber { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Privilege Privilege { get; set; }

        public bool Enabled { get; set; }
    }

    public class UserVerifiedEventArgs
    {
        public string UserEnrollmentNumber { get; set; }
        public bool IsRecordValid { get; set; } //???

        [JsonConverter(typeof(StringEnumConverter))]
        public VerifyMode VerifyMode { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public InOutMode InOutMode { get; set; }

        public DateTimeOffset Timestamp { get; set; }
        public int WorkCode { get; set; } //???
        public int MachineNumber { get; set; }
    }

    public class StatusReport
    {

        public bool Succeeded { get; set; }
        public int ErrorCode { get; set; }

        public static StatusReport Success()
        {
            return new StatusReport() { Succeeded = true };
        }

        public static StatusReport Fail(int errorCode)
        {
            return new StatusReport() { Succeeded = false, ErrorCode = errorCode };
        }

        public static StatusReport<T> Success<T>(T Data)
        {
            return new StatusReport<T>() { Succeeded = true, Data = Data };
        }

        public static StatusReport<T> Fail<T>(int errorCode)
        {
            return new StatusReport<T>() { Succeeded = false, ErrorCode = errorCode };
        }
    }

    public class StatusReport<T> : StatusReport
    {
        public T Data { get; set; }
    }

    public enum State
    {
        Connecting,
        Connected,
        Disconnected,
    }

    public enum Trigger
    {
        ConnectRequested,
        ConnectSucceeded,
        ConnectFailed,
        ConnectionDropped
    }
}
