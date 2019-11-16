using Stateless;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using zkemkeeper;

namespace ZkTecoIn2IntegrationService
{
    public class Session
    {
        private event EventHandler<UserVerifiedEventArgs> _userVerified;

        public event EventHandler<UserVerifiedEventArgs> UserVerified
        {
            add { lock (syncRoot) _userVerified += value; }
            remove { lock (syncRoot) _userVerified -= value; }
        }

        private readonly object syncRoot = new object();
        private CZKEM axCZKEM1;
        private StateMachine<State, Trigger> connectionState = new StateMachine<State, Trigger>(State.Disconnected);

        private string ip;
        private int port;
        private int commKey;
        private int dwMachineNumber = 1;


        public Session(string ip, int port, int commKey, int machineNumber)
        {
            this.ip = ip != null ? IPAddress.Parse(ip).ToString() : throw new ArgumentNullException(nameof(ip));
            this.port = (port > 0 && port <= 65535) ? port : throw new ArgumentOutOfRangeException(nameof(port), port, "port must be between 1 and 65535");
            this.commKey = (commKey >= 0 && commKey <= 999999) ? commKey : throw new ArgumentOutOfRangeException(nameof(commKey), commKey, "commKey must be between 0 and 999999");
            this.dwMachineNumber = machineNumber;

            axCZKEM1 = new zkemkeeper.CZKEM();
            axCZKEM1.OnConnected += Sdk_OnConnected;
            axCZKEM1.OnDisConnected += Sdk_OnDisconnected;
            configureConnectionStateMachine();
        }

        private void Sdk_OnDisconnected()
        {
            connectionState.Fire(Trigger.ConnectionDropped);
        }

        private void Sdk_OnConnected()
        {
            connectionState.Fire(Trigger.ConnectSucceeded);
        }

        private void configureConnectionStateMachine()
        {
            connectionState.Configure(State.Disconnected)
                .Permit(Trigger.ConnectRequested, State.Connecting)
                .OnEntryFrom(Trigger.ConnectionDropped, () =>
                {
                    lock (syncRoot)
                    {
                        axCZKEM1.Disconnect();
                    }
                });

            connectionState.Configure(State.Connecting)
                .Permit(Trigger.ConnectFailed, State.Disconnected)
                .Permit(Trigger.ConnectSucceeded, State.Connected);

            connectionState.Configure(State.Connected)
                .Permit(Trigger.ConnectionDropped, State.Disconnected)
                .OnEntry(() =>
                {
                    lock (syncRoot)
                    {
                        //Here you can register the realtime events that you want to be triggered
                        //(the parameters 65535 means registering all)
                        if (axCZKEM1.RegEvent(dwMachineNumber, 1 ^ 8))
                        {
                            Console.WriteLine("Events registered successfully");
                            //only for color device
                            axCZKEM1.OnAttTransactionEx += new _IZKEMEvents_OnAttTransactionExEventHandler(axCZKEM1_OnAttTransactionEx);
                            axCZKEM1.OnEnrollFingerEx += new _IZKEMEvents_OnEnrollFingerExEventHandler(axCZKEM1_OnEnrollFingerEx);
                        }
                    }

                });
        }

        private void axCZKEM1_OnEnrollFingerEx(string EnrollNumber, int FingerIndex, int ActionResult, int TemplateLength)
        {
            //throw new NotImplementedException();
        }

        private void axCZKEM1_OnAttTransactionEx(string EnrollNumber, int IsInValid, int AttState, int VerifyMethod, int Year, int Month, int Day, int Hour, int Minute, int Second, int WorkCode)
        {
            lock (syncRoot)
            {
                var args = new UserVerifiedEventArgs()
                {
                    UserEnrollmentNumber = EnrollNumber,
                    Timestamp = new DateTime(Year, Month, Day, Hour, Minute, Second, DateTimeKind.Local),
                    IsRecordValid = !Convert.ToBoolean(IsInValid),
                    VerifyMode = (VerifyMode)VerifyMethod,
                    InOutMode = (InOutMode)AttState,
                    WorkCode = WorkCode,
                    MachineNumber = dwMachineNumber
                };

                _userVerified?.Invoke(this, args);
            }
        }

        public StatusReport Connect()
        {
            lock (syncRoot)
            {
                connectionState.Fire(Trigger.ConnectRequested);

                axCZKEM1.SetCommPassword(commKey);

                if (axCZKEM1.Connect_Net(ip, port))
                {
                    return StatusReport.Success();
                }
                else
                {
                    connectionState.Fire(Trigger.ConnectFailed);

                    int errorCode = 0;
                    axCZKEM1.GetLastError(ref errorCode);
                    return StatusReport.Fail(errorCode);
                }
            }
        }

        public StatusReport<List<UserInfo>> GetAllUsers()
        {
            lock (syncRoot)
            {
                EnsureConnected();
                try
                {
                    List<UserInfo> users = new List<UserInfo>();

                    axCZKEM1.EnableDevice(dwMachineNumber, false);
                    if (axCZKEM1.ReadAllUserID(dwMachineNumber)) //read all user info to memory
                    {
                        while (axCZKEM1.SSR_GetAllUserInfo(dwMachineNumber, out string enrollNumber, out string name, out string password, out int privilege, out bool enabled))
                        {
                            users.Add(new UserInfo() { Enabled = enabled, EnrollmentNumber = enrollNumber, Password = password, Name = name, Privilege = (Privilege)privilege });
                        }
                    }
                    else
                    {
                        int idwErrorCode = 0;
                        axCZKEM1.GetLastError(ref idwErrorCode);

                        if (idwErrorCode != 0)
                        {
                            return StatusReport.Fail<List<UserInfo>>(idwErrorCode);
                        }
                    }

                    return StatusReport.Success(users);
                }
                finally
                {
                    axCZKEM1.EnableDevice(dwMachineNumber, true);
                }
            }
        }

        public StatusReport<List<AttendanceRecord>> GetAttendanceRecords(DateTimeOffset from, DateTimeOffset to)
        {
            lock (syncRoot)
            {
                EnsureConnected();
                try
                {
                    axCZKEM1.EnableDevice(dwMachineNumber, false);//disable the device

                    List<AttendanceRecord> results = new List<AttendanceRecord>();
                    if (axCZKEM1.ReadGeneralLogData(dwMachineNumber))
                    {
                        string sdwEnrollNumber = "";
                        int idwVerifyMode = 0;
                        int idwInOutMode = 0;
                        int idwYear = 0;
                        int idwMonth = 0;
                        int idwDay = 0;
                        int idwHour = 0;
                        int idwMinute = 0;
                        int idwSecond = 0;
                        int idwWorkcode = 0;
                        while (axCZKEM1.SSR_GetGeneralLogData(dwMachineNumber, out sdwEnrollNumber, out idwVerifyMode,
                                    out idwInOutMode, out idwYear, out idwMonth, out idwDay, out idwHour, out idwMinute, out idwSecond, ref idwWorkcode))//get records from the memory
                        {
                            var ts = new DateTime(idwYear, idwMonth, idwDay, idwHour, idwMinute, idwSecond, DateTimeKind.Local);
                            if (ts >= from && ts <= to)
                            {
                                var r = new AttendanceRecord();
                                r.Timestamp = ts;
                                r.UserId = sdwEnrollNumber;
                                r.InOutMode = (InOutMode)idwInOutMode;
                                r.VerifyMode = (VerifyMode)idwVerifyMode;
                                r.WorkCode = idwWorkcode;
                                results.Add(r);
                            }
                        }
                    }
                    else
                    {
                        int idwErrorCode = 0;
                        axCZKEM1.GetLastError(ref idwErrorCode);

                        if (idwErrorCode != 0)
                        {
                            return StatusReport.Fail<List<AttendanceRecord>>(idwErrorCode);
                        }
                    }

                    return StatusReport.Success(results);
                }
                finally
                {
                    axCZKEM1.EnableDevice(dwMachineNumber, true);
                }
            }
        }

        private void EnsureConnected()
        {
            if (connectionState.State == State.Connected)
            {
                connectionState.Fire(Trigger.ConnectionDropped);                
            }

            if(connectionState.State == State.Disconnected)
            {
                var report = Connect();
                if(!report.Succeeded)
                {
                    throw new InvalidOperationException("Connect failed");
                }
            }
        }
    }
}
