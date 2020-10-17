using System;
using System.Data;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Data.SqlClient;
using SRKSDemo.Server_Model;

namespace SRKSDemo
{
    public class GetMode
    {
        //Tab Connection
       // i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
        SRKSDemo.Server_Model.i_facility_shaktiEntities1 Serverdb = new SRKSDemo.Server_Model.i_facility_shaktiEntities1();

        //Server Connection
        //titandmgEntities1 dbServ = new titandmgEntities1();

        //Geting latest mode value
        public string[] Getmode(int MachineID)
        {
            string[] Modevalue = new string[3];
            Modevalue[0] = "Production";
            Modevalue[1] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:dd");
            string modefromtable = null;
            string ModestartTime = null;

            //Corrected date
            DateTime nowdate = DateTime.Now;
            string correctedDate = nowdate.ToString("yyyy-MM-dd");
            if (nowdate.Hour < 7 && nowdate.Hour > 0)
            {
                correctedDate = nowdate.AddDays(-1).ToString("yyyy-MM-dd");
            }

            //var Mode = db.tabtblmodes.Where(m => m.IsDeleted == 0 && m.CorrectedDate == correctedDate && m.MachineID == MachineID).OrderByDescending(m => m.ModeID);
            var Mode = Serverdb.tblmodes.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID).OrderByDescending(m => m.ModeID);
            foreach (var val in Mode)
            {
                modefromtable = val.MacMode;
                ModestartTime = val.InsertedOn.ToString("yyyy-MM-dd HH:mm:dd");
                break;
            }

            if (modefromtable != null)
            {
                if (modefromtable == "MNT")
                    Modevalue[0] = "Maintenance";
                else if (modefromtable == "OPT")
                    Modevalue[0] = "Production";
                else if (modefromtable == "SETUP")
                    Modevalue[0] = "Setting";
                else if (modefromtable == "IDLE")
                    Modevalue[0] = "Idle";

                Modevalue[1] = ModestartTime;
            }
            else
            {
                //If no data in mode table
                //inserting data in mode table related to production
                Modetable("OPT", MachineID);
            }

            //Getting Tab Connection Status
            string MachineConnectionStatus = GetConnection(MachineID);
            if (MachineConnectionStatus == "Disconnected" && Modevalue[0] != "Maintenance")
            {
                Modevalue[2] = Modevalue[0];
                Modevalue[0] = "M/C Off";
                Modevalue[1] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:dd");
            }
            else if (MachineConnectionStatus == "Disconnected" && Modevalue[0] == "Maintenance")
            {
                Modevalue[2] = "M/C Off";
            }
            return Modevalue;
        }

        //Pushing data in mode table
        public void Modetable(string Mode, int Machineid)
        {
            tblmode message = new tblmode();
            //Corrected date
            DateTime nowdate = DateTime.Now;
            DateTime correctedDate = nowdate;
            if (nowdate.Hour < 7 && nowdate.Hour > 0)
            {
                correctedDate = nowdate.AddDays(-1);
            }

            DateTime datetime = DateTime.Now;

            message.MacMode = Mode.ToString();
            message.MachineID = Machineid;

            message.InsertedBy = 1;
            message.InsertedOn = Convert.ToDateTime(datetime.ToString("yyyy-MM-dd HH:mm:ss"));
            message.CorrectedDate = correctedDate;

            Serverdb.tblmodes.Add(message);
            Serverdb.SaveChanges();
        }

        //Geting latest Connection value for PC
        public string GetConnection(int MachineID)
        {

            //string Status = "Disconnected";
            //int Statustable = 0;

            ////Corrected date
            //DateTime nowdate = DateTime.Now;
            //string correctedDate = nowdate.ToString("yyyy-MM-dd");
            //if (nowdate.Hour < 6 && nowdate.Hour > 0)
            //{
            //    correctedDate = nowdate.AddDays(-1).ToString("yyyy-MM-dd");
            //}

            //var State = db.tabtblmcstatus.Where(m => m.IsDeleted == 0 && m.CorrectedDate == correctedDate && m.MachineID == MachineID).OrderByDescending(m => m.StatusID);
            //foreach (var val in State)
            //{
            //    Statustable = val.Status;
            //    break;
            //}

            //if (Statustable != 0)
            //{
            //    Status = "Connected";
            //}

            //return Status;


            //getting machine ipaddress
            int status = 2;
            string IPAddress = null;
            if (MachineID == 0)
            {
            }
            else
            {
                //machine_master ipadd = db.tabmachine_master.Find(MachineID);
                //IPAddress = ipadd.IPAddress;
            }
            string Status = "Disconnected";
            int Statustable = 0;

            bool State = false;
            Ping ping = new Ping();
            try
            {
                PingReply pingresult = ping.Send(IPAddress);
                if (pingresult.Status.ToString() == "Success")
                {
                    State = true;
                    Status = "Connected";
                }
            }
            catch { }

            ////Pushing data in tblmcstatus
            //var Statevalue = db.tabtblmcstatus.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID).OrderByDescending(m => m.StatusID);

            //if (Statevalue.Count() > 0)
            //{
            //    foreach (var val in Statevalue)
            //    {
            //        status = val.Status;
            //        break;
            //    }

            //    if (status == 0 && Status != "Disconnected")//if old status got changed
            //    {
            //        tblmcstatu serv = new tblmcstatu();
            //        serv.MachineID = MachineID;
            //        serv.Status = 1;
            //        serv.InsertedOn = DateTime.Now;
            //        serv.InsertedBy = 1;
            //        serv.CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            //        serv.IsDeleted = 0;
            //        db.tabEntry(serv).State = System.Data.Entity.EntityState.Added;
            //        db.tabSaveChanges();
            //    }
            //    else if (status == 1 && Status == "Disconnected")//if old status got changed
            //    {
            //        tblmcstatu serv = new tblmcstatu();
            //        serv.MachineID = MachineID;
            //        serv.Status = 0;
            //        serv.InsertedOn = DateTime.Now;
            //        serv.InsertedBy = 1;
            //        serv.CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            //        serv.IsDeleted = 0;
            //        db.tabEntry(serv).State = System.Data.Entity.EntityState.Added;
            //        db.tabSaveChanges();
            //    }
            //}
            //else// if there is no status in tblmcstatus status for particuler machine
            //{
            //    if (Status == "Disconnected")
            //        status = 0;
            //    else
            //        status = 1;
            //    tblmcstatu serv = new tblmcstatu();
            //    serv.MachineID = MachineID;
            //    serv.Status = status;
            //    serv.InsertedOn = DateTime.Now;
            //    serv.InsertedBy = 1;
            //    serv.CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            //    serv.IsDeleted = 0;
            //    db.tabEntry(serv).State = System.Data.Entity.EntityState.Added;
            //    db.tabSaveChanges();
            //}

            return Status;
        }

        //Geting latest Connection value for server
        public string GetConnectionForServer(int MachineID)
        {
            //getting server ipaddress
            int status = 2;
            string IPAddress = null;
            if (MachineID == 0)
            {
            }
            else
            {
                //machine_master ipadd = db.tabmachine_master.Find(MachineID);
                //IPAddress = ipadd.ServerIPAddress;
            }
            string Status = "Disconnected";
            int Statustable = 0;

            bool State = false;
            Ping ping = new Ping();
            try
            {
                PingReply pingresult = ping.Send(IPAddress);
                if (pingresult.Status.ToString() == "Success")
                {
                    State = true;
                    Status = "Connected";
                }
            }
            catch { }


            //Pushing data in tblserverstatus
            //var Statevalue = db.tabtblserverstatus.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID).OrderByDescending(m => m.StatusID);

            //if (Statevalue.Count() > 0)
            //{
            //    foreach (var val in Statevalue)
            //    {
            //        status = val.Status;
            //        break;
            //    }

            //    if (status == 0 && Status != "Disconnected")//if old status got changed
            //    {
            //        tblserverstatu serv = new tblserverstatu();
            //        serv.MachineID = MachineID;
            //        serv.Status = 1;
            //        serv.InsertedOn = DateTime.Now;
            //        serv.InsertedBy = 1;
            //        serv.CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            //        serv.IsDeleted = 0;
            //        db.tabEntry(serv).State = System.Data.Entity.EntityState.Added;
            //        db.tabSaveChanges();
            //    }
            //    else if (status == 1 && Status == "Disconnected")//if old status got changed
            //    {
            //        tblserverstatu serv = new tblserverstatu();
            //        serv.MachineID = MachineID;
            //        serv.Status = 0;
            //        serv.InsertedOn = DateTime.Now;
            //        serv.InsertedBy = 1;
            //        serv.CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            //        serv.IsDeleted = 0;
            //        db.tabEntry(serv).State = System.Data.Entity.EntityState.Added;
            //        db.tabSaveChanges();
            //    }
            //}
            //else// if there is no status in tblserver status for particuler machine
            //{
            //    if (Status == "Disconnected")
            //        status = 0;
            //    else
            //        status = 1;
            //    tblserverstatu serv = new tblserverstatu();
            //    serv.MachineID = MachineID;
            //    serv.Status = status;
            //    serv.InsertedOn = DateTime.Now;
            //    serv.InsertedBy = 1;
            //    serv.CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            //    serv.IsDeleted = 0;
            //    db.tabEntry(serv).State = System.Data.Entity.EntityState.Added;
            //    db.tabSaveChanges();
            //}

            return Status;
        }

        //Get Shift 
        public string GetShift()
        {
            string Shift = null;
            DateTime time = DateTime.Now;
            TimeSpan CtTime = new TimeSpan(time.Hour, time.Minute, time.Second);

            var shiftdetails = Serverdb.tblshiftdetails.Where(m => m.ShiftStartTime <= CtTime && m.ShiftEndTime >= CtTime);
            if (shiftdetails.Count() != 0)
            {
                foreach (var shft in shiftdetails)
                {
                    Shift = shft.ShiftDetailsName.Trim();
                }
            }
            else
            {
                Shift = "Shift3";
            }

            if (Shift == "Shift1")
                Shift = "1";
            else if (Shift == "Shift2")
                Shift = "2";
            else if (Shift == "Shift3")
                Shift = "3";

            return Shift;
        }

        //Update Maintenance data in dashboard table
        public void updatemantrecord(int Machineid)
        {
            int listID = 0;
            string startdate = null;
            //var maintmoderecord = db.tabtbldashboarddatas.Where(m => m.MachineID == Machineid && m.IsDeleted == 0 && m.Mode == "Maintenance").OrderByDescending(m => m.listID);
            //foreach (var id in maintmoderecord)
            //{
            //    listID = id.listID;
            //    startdate = id.InsertedOn.ToString("yyyy-MM-dd");
            //    break;
            //}

            //if (listID != 0)
            //{
            //    tbldashboarddata dashboarddata = db.tabtbldashboarddatas.Find(listID);
            //    dashboarddata.EndTime = DateTime.Now.ToString("HH:mm.ss");

            //    dashboarddata.ModifiedOn = DateTime.Now;
            //    dashboarddata.ModifiedBy = 1;

            //    dashboarddata.Duration = Math.Round(DateTime.Now.Subtract(Convert.ToDateTime(startdate + " " + dashboarddata.StartTime)).TotalHours, 2).ToString();
            //    db.tabEntry(dashboarddata).State = System.Data.Entity.EntityState.Modified;
            //    db.tabSaveChanges();
            //}
        }

        //Check shift end before 20 minute
        public bool CheckShiftEndForO8531Prgm(int Machineid)
        {
            bool tick = false;

            //get current time
            DateTime currentTIme = DateTime.Now;
            DateTime EndTime = DateTime.Now;
            DateTime datetimebefore10mnt = DateTime.Now;
            double duration = 0;

            //Get Shift end time and running shift
            string Shift = null;
            string Shiftendtime = null;


            TimeSpan CtTime = new TimeSpan(currentTIme.Hour, currentTIme.Minute, currentTIme.Second);

            var shiftdetails = Serverdb.tblshiftdetails.Where(m => m.ShiftStartTime <= CtTime && m.ShiftEndTime >= CtTime);
            if (shiftdetails.Count() != 0)
            {
                foreach (var shft in shiftdetails)
                {
                    Shift = shft.ShiftDetailsName.Trim();
                    Shiftendtime = shft.ShiftEndTime.ToString().Trim();
                }
            }
            else
            {
                Shift = "Shift3";
                Shiftendtime = "06:00";
            }

            //Checking 10 minutes scanerio
            if (Shift == "Shift3")
            {
                if (currentTIme.Hour < 6 && currentTIme.Hour > 0)
                {
                    Shiftendtime = currentTIme.ToString("yyyy-MM-dd") + " " + Shiftendtime;
                    EndTime = Convert.ToDateTime(Shiftendtime);

                    duration = EndTime.Subtract(currentTIme).TotalMinutes;
                }
                else
                {
                    Shiftendtime = currentTIme.ToString("yyyy-MM-dd") + " " + Shiftendtime;
                    EndTime = Convert.ToDateTime(Shiftendtime);

                    duration = EndTime.AddDays(1).Subtract(currentTIme).TotalMinutes;
                }
            }
            else
            {
                Shiftendtime = currentTIme.ToString("yyyy-MM-dd") + " " + Shiftendtime;
                EndTime = Convert.ToDateTime(Shiftendtime);

                duration = EndTime.Subtract(currentTIme).TotalMinutes;
            }

            if (duration <= 10)//in same shift 10 minite before this code will work
            {

                datetimebefore10mnt = EndTime.AddMinutes(-10);
                var O8531 = Serverdb.program_master.Where(m => m.MachineID == Machineid && m.InsertedOn >= datetimebefore10mnt && m.InsertedOn <= EndTime);
                int count = 0;
                foreach (var data in O8531)
                {
                    count = count + 1;
                }
                //checking data is present or not in program master
                if (count != 0)
                {

                }
                else
                {
                    tick = true;
                }
            }
            else//after shift 
            {
                //Taking last shift end time
                if (Shift == "Shift3")
                {
                    Shift = "Shift2";
                }
                else if (Shift == "Shift2")
                {
                    Shift = "Shift1";
                }
                else
                { Shift = "Shift3"; }

                var shiftdetails1 = Serverdb.tblshiftdetails.Where(m => m.ShiftDetailsName == Shift);
                if (shiftdetails1.Count() != 0)
                {
                    foreach (var shft in shiftdetails1)
                    {
                        Shift = shft.ShiftDetailsName.Trim();
                        Shiftendtime = shft.ShiftEndTime.ToString().Trim();
                    }
                }

                //If current running shift is 3 (3-1)
                if (Shift == "Shift2")
                {
                    if (currentTIme.Hour < 6 && currentTIme.Hour > 0)
                    {
                        Shiftendtime = currentTIme.ToString("yyyy-MM-dd") + " " + Shiftendtime;
                        EndTime = Convert.ToDateTime(Shiftendtime);
                        //end time of 2nd shift will be last date
                        EndTime = EndTime.AddDays(-1);
                    }
                    else
                    {
                        Shiftendtime = currentTIme.ToString("yyyy-MM-dd") + " " + Shiftendtime;
                        EndTime = Convert.ToDateTime(Shiftendtime);
                    }
                }
                else
                {
                    Shiftendtime = currentTIme.ToString("yyyy-MM-dd") + " " + Shiftendtime;
                    EndTime = Convert.ToDateTime(Shiftendtime);
                }

                //Current time
                DateTime Currenttime = DateTime.Now;
                datetimebefore10mnt = EndTime.AddMinutes(-10);
                var O8531 = Serverdb.program_master.Where(m => m.MachineID == Machineid && m.InsertedOn >= datetimebefore10mnt && m.InsertedOn <= Currenttime);
                int count = 0;
                foreach (var data in O8531)
                {
                    count = count + 1;
                }
                //checking data is present or not in program master
                if (count != 0)
                {

                }
                else
                {
                    tick = true;
                }
            }
            return tick;
        }

        //Get Shift end time
        public bool GetShiftEndTime()
        {
            bool tick = false;
            //get current time
            DateTime currentTIme = DateTime.Now;
            DateTime EndTime = DateTime.Now;
            double duration = 0;

            //Get Shift end time and running shift
            string Shift = null;
            string Shiftendtime = null;


            TimeSpan CtTime = new TimeSpan(currentTIme.Hour, currentTIme.Minute, currentTIme.Second);

            var shiftdetails = Serverdb.tblshiftdetails.Where(m => m.ShiftStartTime <= CtTime && m.ShiftEndTime >= CtTime);
            if (shiftdetails.Count() != 0)
            {
                foreach (var shft in shiftdetails)
                {
                    Shift = shft.ShiftDetailsName.Trim();
                    Shiftendtime = shft.ShiftEndTime.ToString().Trim();
                }
            }
            else
            {
                Shift = "Shift3";
                Shiftendtime = "06:00";
            }
            Shiftendtime = currentTIme.ToString("yyyy-MM-dd") + " " + Shiftendtime;
            EndTime = Convert.ToDateTime(Shiftendtime);
            EndTime = EndTime.AddMinutes(-10);
            if (EndTime.Hour == currentTIme.Hour && EndTime.Minute == currentTIme.Minute)
                tick = true;
            return tick;
        }

        //select which shift should display there
        public string[] SelectWhichShouldDisplay()
        {
            //get current time
            DateTime StartTime = DateTime.Now;
            DateTime EndTime = DateTime.Now;
            DateTime MiddleTime = DateTime.Now;

            //String Value
            string StartTimeString = null;
            string EndTimeString = null;

            DateTime currentTIme = DateTime.Now;
            int ShiftVal = 0;

            //Get Shift end time and running shift
            string[] Shift = new string[3];

            TimeSpan CtTime = new TimeSpan(currentTIme.Hour, currentTIme.Minute, currentTIme.Second);
            var shiftdetails = Serverdb.tblshiftdetails.Where(m => m.ShiftStartTime <= CtTime && m.ShiftEndTime >= CtTime);
            if (shiftdetails.Count() != 0)
            {
                foreach (var shft in shiftdetails)
                {
                    Shift[1] = shft.ShiftDetailsName.Trim();
                    ShiftVal = shft.ShiftDetailsID;
                    StartTimeString = shft.ShiftStartTime.ToString();
                    StartTimeString = StartTime.ToString("yyyy-MM-dd") + " " + StartTimeString;
                    StartTime = Convert.ToDateTime(StartTimeString);
                    EndTime = StartTime.AddMinutes(45);
                    Shift[2] = ShiftVal.ToString();
                }
            }
            else
            {
                Shift[1] = "Shift3";
                ShiftVal = 3;
                StartTimeString = StartTime.ToString("yyyy-MM-dd") + " 10:30:00 PM";
                StartTime = Convert.ToDateTime(StartTimeString);
                EndTime = StartTime.AddMinutes(45);
                Shift[2] = ShiftVal.ToString();
            }

            if (StartTime <= MiddleTime && MiddleTime <= EndTime)
            {
                //Taking previous shift
                if (ShiftVal == 1)
                {
                    Shift[0] = "Shift3";
                }
                else
                {
                    Shift[0] = "Shift" + (ShiftVal - 1);
                }

            }
            return Shift;
        }

        //Get Ip Address of Same System
        public string GetIPAddressofTabSystem()
        {
            //string IP_Address = null;
            //System.Web.HttpContext context = System.Web.HttpContext.Current;
            //string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            //if (!string.IsNullOrEmpty(ipAddress))
            //{
            //    string[] addresses = ipAddress.Split(',');
            //    if (addresses.Length != 0)
            //    {
            //        IP_Address = addresses[0];
            //    }
            //}
            ////Use this for client IP Address
            //IP_Address = context.Request.ServerVariables["HOST"];
            //string userIpAddress = context.Request.UserHostAddress;

            string ipAdd = "";
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
         //var AddressList=ipHostInfo.AddressList.OrderByDescending(address => address.AddressFamily).ToList();
            ipAdd = Convert.ToString(ipHostInfo.AddressList.LastOrDefault(address => address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork));
            //ipAdd = "192.68.1.128";

            return ipAdd;
        }

        public String GetIPAddressofAndon()
        {
            String IPAddress = null;
            string line;
            string curFile = @"c:\users\oeeuser\desktop\IPAddress.txt";
            // Read the file and display it line by line.
            if (System.IO.File.Exists(curFile))
            {
                System.IO.StreamReader file =
                    new System.IO.StreamReader(@"c:\users\oeeuser\desktop\IPAddress.txt");
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains("IP"))
                    {
                        string[] linesplit = line.Split(':');
                        IPAddress = linesplit[1];
                    }
                }

                file.Close();
            }
            return IPAddress;
        }
        //GetCorrectedDate(out correctedDate);
        public void GetCorrectedDate(out String correctedDate)
        {
            correctedDate = null;
            //MsqlConnection MC = new MsqlConnection();
            //MC.open();
            //String GetDayStartQuery = "SELECT StartTime from unitworksccs.[dbo].tbldaytiming where IsDeleted = 0 Limit 1";
            //SqlDataAdapter daGDS = new SqlDataAdapter(GetDayStartQuery, MC.msqlConnection);
            //DataTable dtGDS = new DataTable();
            //daGDS.Fill(dtGDS);
            //MC.close();
            var getDay = Serverdb.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
            TimeSpan Start = Convert.ToDateTime(getDay.StartTime).TimeOfDay;
            if (Start <= DateTime.Now.TimeOfDay)
            {
                correctedDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                correctedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }
        }

        //checking dual idle entry
        public bool CheckIdleEntry(int machnid)
        {
            bool tick = false;
            string mode = null;
            int IdleLock = 0;
            var IdleEntry = Serverdb.tbllivemodes.Where(m => m.IsDeleted == 0 && m.MachineID == machnid && m.IsCompleted == 0).OrderByDescending(m => m.ModeID).FirstOrDefault();
            if(IdleEntry != null)
            {
                mode = IdleEntry.MacMode;
                IdleLock = IdleEntry.StartIdle;
                if (mode == "IDLE" && IdleLock == 1)
                {
                    tick = true;
                }
            }
            //true if last entry is idle
            return tick;
        }

        public void UpdateOperatorHeader(int MacID)
        {
            var MacDet = Serverdb.tblmachinedetails.Where(m => m.MachineID == MacID).FirstOrDefault();

            String ServerConnected = "NOT CONNECTED";
            String MachineConnected = "NOT CONNECTED";
            Ping ServerPing = new Ping();
            PingReply ServerReply;
            //String TabIPAddress = GetIPAddressofTabSystem();
            String TabIPAddress = MacDet.IPAddress;
            var MachineDetails = Serverdb.tblmachinedetails.Where(m => m.TabIPAddress == TabIPAddress && m.IsDeleted == 0).FirstOrDefault();
            try
            {
                ServerReply = ServerPing.Send(MachineDetails.IPAddress, 500);
                if (ServerReply.Status == IPStatus.Success)
                {
                    ServerConnected = "CONNECTED";
                }
            }
            catch (Exception e)
            {

            }
            
            String MacIP = MacDet.IPAddress;
            Ping MacPing = new Ping();
            PingReply MacReply;
            MacReply = MacPing.Send(MacIP, 500);

            if (MacReply.Status == IPStatus.Success)
            {
                MachineConnected = "CONNECTED";
            }
            #region Update tblOperatorHeader 
            var operatorHeader = Serverdb.tblOperatorHeaders.Where(m => m.MachineID == MacID).OrderByDescending(m => m.InsertedOn).FirstOrDefault();

            operatorHeader.TabConnecStatus = MachineConnected;
            operatorHeader.ServerConnecStatus = ServerConnected;
            operatorHeader.ModifiedOn = DateTime.Now;
            operatorHeader.ModifiedBy = 1;// get from session once these screens are integrated....


            Serverdb.Entry(operatorHeader).State = System.Data.Entity.EntityState.Modified;
            Serverdb.SaveChanges();

            #endregion
        }

        //Deleting last day record after 8 am(data will delete yesterday 6am to today 6am)
        //public void MovingLastDayRecord(int MachineID)
        //{
        //    //Taking Current Time
        //    DateTime CurrentTime = DateTime.Now;
        //    string DateWithFormet = CurrentTime.ToString("yyyy-MM-dd HH:mm:ss");
        //    CurrentTime = Convert.ToDateTime(DateWithFormet);
        //    int CurrentHour = CurrentTime.Hour;

        //    DateTime LastDayStartTime = Convert.ToDateTime(CurrentTime.AddDays(-1).ToString("yyyy-MM-dd 06:00:00"));
        //    DateTime LastDayEndTime = Convert.ToDateTime(CurrentTime.ToString("yyyy-MM-dd 06:00:00"));

        //    DateTime correctedDate = new DateTime();
        //    correctedDate = Convert.ToDateTime(CurrentTime.AddDays(-1).ToString("yyyy-MM-dd"));
        //    string correcteddatestring = correctedDate.ToString("yyyy-MM-dd");

        //    //checking whether the data is already moved or not
        //    var datamovestatus = db.tabtbldatamovestatus.Where(m => m.IsDeleted == 0 && m.CorrectedDate == correctedDate && m.MoveStatus == 0);

        //    if (datamovestatus.Count() == 0)
        //    {
        //        #region //Checking if current time is greater than 8 than i will move the record from Tab Pc to server pc
        //        if (CurrentHour >= 8)
        //        {
        //            //GetMachine Status from Server
        //            string StatusFromServer = GetConnectionForServer(MachineID);

        //            if (StatusFromServer == "Connected")
        //            {
        //                int[,] Id = new int[4, 4];
        //                int i = 0;
        //                //moving data from alarm-history-master

        //                var alarmhistorymasterdata = db.tabalarm_history_master.Where(m => m.CorrectedDate == correctedDate);
        //                Id = new int[alarmhistorymasterdata.Count(), 2];
        //                if (alarmhistorymasterdata.Count() > 0)
        //                {
        //                    foreach (var data in alarmhistorymasterdata)
        //                    {
        //                        Id[i, 0] = data.AlarmID;
        //                        i = i + 1;
        //                    }

        //                    for (int j = 0; j < Id.Length / 2; j++)
        //                    {
        //                        //Taking data from PC
        //                        alarm_history_master maintable = new alarm_history_master();
        //                        using (titandmgEntities2 Context = new titandmgEntities2())
        //                        {
        //                            maintable = Context.alarm_history_master.Find(Id[j, 0]);
        //                        }

        //                        ////movingData to server
        //                        using (titandmgEntities1 Context1 = new titandmgEntities1())
        //                        {
        //                            Context1.Entry(maintable).State = System.Data.Entity.EntityState.Added;
        //                            Context1.SaveChanges();
        //                        }

        //                        Id[j, 1] = 1;

        //                        //deleting data from pc
        //                        using (titandmgEntities2 Context = new titandmgEntities2())
        //                        {
        //                            alarm_history_master alrm = Context.alarm_history_master.Find(Id[j, 0]);
        //                            Context.alarm_history_master.Remove(alrm);
        //                            Context.SaveChanges();
        //                        }
        //                    }
        //                }

        //                //moving data from Message-history-master
        //                //if isread =0 means data not yet transfered to server so this code will transfer tha data and delete the data from server
        //                var messagehistorymaster = db.tabmessage_history_master.Where(m => m.CorrectedDate == correctedDate && m.IsRead == 0);
        //                Id = new int[messagehistorymaster.Count(), 2];
        //                i = 0;
        //                if (messagehistorymaster.Count() > 0)
        //                {
        //                    foreach (var data in messagehistorymaster)
        //                    {
        //                        Id[i, 0] = data.MessageID;
        //                        i = i + 1;
        //                    }

        //                    for (int j = 0; j < Id.Length / 2; j++)
        //                    {

        //                        //Taking data from PC
        //                        message_history_master maintable = new message_history_master();
        //                        using (titandmgEntities2 Context = new titandmgEntities2())
        //                        {
        //                            maintable = Context.message_history_master.Find(Id[j, 0]);
        //                        }

        //                        ////movingData to server
        //                        using (titandmgEntities1 Context1 = new titandmgEntities1())
        //                        {
        //                            Context1.Entry(maintable).State = System.Data.Entity.EntityState.Added;
        //                            Context1.SaveChanges();
        //                        }

        //                        Id[j, 1] = 1;

        //                        //deleting data from pc
        //                        using (titandmgEntities2 Context = new titandmgEntities2())
        //                        {
        //                            message_history_master msg = Context.message_history_master.Find(Id[j, 0]);
        //                            Context.message_history_master.Remove(msg);
        //                            Context.SaveChanges();
        //                        }
        //                    }

        //                }
        //                //Deleting moved data from pc related to progrma master
        //                messagehistorymaster = db.tabmessage_history_master.Where(m => m.CorrectedDate == correctedDate && m.IsRead == 1);
        //                Id = new int[messagehistorymaster.Count(), 2];
        //                i = 0;
        //                if (messagehistorymaster.Count() > 0)
        //                {
        //                    foreach (var data in messagehistorymaster)
        //                    {
        //                        Id[i, 0] = data.MessageID;
        //                        i = i + 1;
        //                    }

        //                    for (int j = 0; j < Id.Length / 2; j++)
        //                    {

        //                        //deleting data from pc
        //                        using (titandmgEntities2 Context = new titandmgEntities2())
        //                        {
        //                            message_history_master msg = Context.message_history_master.Find(Id[j, 0]);
        //                            Context.message_history_master.Remove(msg);
        //                            Context.SaveChanges();
        //                        }
        //                    }
        //                }

        //                //moving data from Operationlog

        //                var opnlog = db.taboperationlogs.Where(m => m.OpDateTime >= LastDayStartTime && m.OpDateTime < LastDayEndTime);
        //                Id = new int[opnlog.Count(), 2];
        //                i = 0;
        //                if (opnlog.Count() > 0)
        //                {
        //                    foreach (var data in opnlog)
        //                    {
        //                        Id[i, 0] = data.idoperationlog;
        //                        i = i + 1;
        //                    }

        //                    for (int j = 0; j < Id.Length / 2; j++)
        //                    {
        //                        //Taking data from PC
        //                        operationlog maintable = new operationlog();
        //                        using (titandmgEntities2 Context = new titandmgEntities2())
        //                        {
        //                            maintable = Context.operationlogs.Find(Id[j, 0]);
        //                        }

        //                        ////movingData to server
        //                        using (titandmgEntities1 Context1 = new titandmgEntities1())
        //                        {
        //                            Context1.Entry(maintable).State = System.Data.Entity.EntityState.Added;
        //                            Context1.SaveChanges();
        //                        }

        //                        Id[j, 1] = 1;

        //                        //deleting data from pc
        //                        using (titandmgEntities2 Context = new titandmgEntities2())
        //                        {
        //                            operationlog opn = Context.operationlogs.Find(Id[j, 0]);
        //                            Context.operationlogs.Remove(opn);
        //                            Context.SaveChanges();
        //                        }
        //                    }
        //                }
        //                //moving data from Parameters-master

        //                var parametermaster = db.tabparameters_master.Where(m => m.CorrectedDate == correctedDate);
        //                Id = new int[parametermaster.Count(), 2];
        //                i = 0;

        //                if (parametermaster.Count() > 0)
        //                {
        //                    foreach (var data in parametermaster)
        //                    {
        //                        Id[i, 0] = data.ParameterID;
        //                        i = i + 1;
        //                    }

        //                    for (int j = 0; j < Id.Length / 2; j++)
        //                    {
        //                        //Taking data from PC
        //                        parameters_master maintable = new parameters_master();
        //                        using (titandmgEntities2 Context = new titandmgEntities2())
        //                        {
        //                            maintable = Context.parameters_master.Find(Id[j, 0]);
        //                        }

        //                        ////movingData to server
        //                        using (titandmgEntities1 Context1 = new titandmgEntities1())
        //                        {
        //                            Context1.Entry(maintable).State = System.Data.Entity.EntityState.Added;
        //                            Context1.SaveChanges();
        //                        }

        //                        Id[j, 1] = 1;

        //                        //deleting data from pc
        //                        using (titandmgEntities2 Context = new titandmgEntities2())
        //                        {
        //                            parameters_master pmmaster = Context.parameters_master.Find(Id[j, 0]);
        //                            Context.parameters_master.Remove(pmmaster);
        //                            Context.SaveChanges();
        //                        }
        //                    }
        //                }



        //                //moving data from program-master
        //                //if isread =0 means data not yet transfered to server so this code will transfer tha data and delete the data from server
        //                var programmaster = db.tabprogram_master.Where(m => m.CorrectedDate == correctedDate && m.IsRead == 0);
        //                Id = new int[programmaster.Count(), 2];
        //                i = 0;
        //                if (programmaster.Count() > 0)
        //                {
        //                    foreach (var data in programmaster)
        //                    {
        //                        Id[i, 0] = data.ProgramID;
        //                        i = i + 1;
        //                    }

        //                    for (int j = 0; j < Id.Length / 2; j++)
        //                    {
        //                        //Taking data from PC
        //                        program_master maintable = new program_master();
        //                        using (titandmgEntities2 Context = new titandmgEntities2())
        //                        {
        //                            maintable = Context.program_master.Find(Id[j, 0]);
        //                        }

        //                        ////movingData to server
        //                        using (titandmgEntities1 Context1 = new titandmgEntities1())
        //                        {
        //                            Context1.Entry(maintable).State = System.Data.Entity.EntityState.Added;
        //                            Context1.SaveChanges();
        //                        }

        //                        Id[j, 1] = 1;

        //                        //deleting data from pc
        //                        using (titandmgEntities2 Context = new titandmgEntities2())
        //                        {
        //                            program_master pgmaster = Context.program_master.Find(Id[j, 0]);
        //                            Context.program_master.Remove(pgmaster);
        //                            Context.SaveChanges();
        //                        }
        //                    }
        //                }

        //                //Deleting moved data from pc related to progrma master
        //                programmaster = db.tabprogram_master.Where(m => m.CorrectedDate == correctedDate && m.IsRead == 1);
        //                Id = new int[programmaster.Count(), 2];
        //                i = 0;
        //                if (programmaster.Count() > 0)
        //                {
        //                    foreach (var data in programmaster)
        //                    {
        //                        Id[i, 0] = data.ProgramID;
        //                        i = i + 1;
        //                    }

        //                    for (int j = 0; j < Id.Length / 2; j++)
        //                    {

        //                        //deleting data from pc
        //                        using (titandmgEntities2 Context = new titandmgEntities2())
        //                        {
        //                            program_master pgmaster = Context.program_master.Find(Id[j, 0]);
        //                            Context.program_master.Remove(pgmaster);
        //                            Context.SaveChanges();
        //                        }
        //                    }
        //                }

        //                //moving data from dashboarddatas

        //                var dashboarddata = db.tabtbldashboarddatas.Where(m => m.CorrectedDate == correctedDate);
        //                Id = new int[dashboarddata.Count(), 2];
        //                i = 0;
        //                if (dashboarddata.Count() > 0)
        //                {
        //                    foreach (var data in dashboarddata)
        //                    {
        //                        Id[i, 0] = data.listID;
        //                        i = i + 1;
        //                    }

        //                    for (int j = 0; j < Id.Length / 2; j++)
        //                    {
        //                        tbldashboarddata maintable = new tbldashboarddata();

        //                        //Taking data from PC
        //                        using (titandmgEntities2 Context = new titandmgEntities2())
        //                        {
        //                            maintable = Context.tbldashboarddatas.Find(Id[j, 0]);
        //                        }

        //                        ////movingData
        //                        using (titandmgEntities1 Context1 = new titandmgEntities1())
        //                        {
        //                            Context1.Entry(maintable).State = System.Data.Entity.EntityState.Added;
        //                            Context1.SaveChanges();
        //                        }

        //                        Id[j, 1] = 1;

        //                        //deleting data from pc
        //                        using (titandmgEntities2 Context = new titandmgEntities2())
        //                        {
        //                            tbldashboarddata dashboard = Context.tbldashboarddatas.Find(Id[j, 0]);
        //                            Context.tbldashboarddatas.Remove(dashboard);
        //                            Context.SaveChanges();
        //                        }
        //                    }
        //                }

        //                //moving data from tblmcstatus

        //                var mcstatus = db.tabtblmcstatus.Where(m => m.CorrectedDate == correcteddatestring);
        //                Id = new int[mcstatus.Count(), 2];
        //                i = 0;
        //                if (mcstatus.Count() > 0)
        //                {
        //                    foreach (var data in mcstatus)
        //                    {
        //                        Id[i, 0] = data.StatusID;
        //                        i = i + 1;
        //                    }

        //                    for (int j = 0; j < Id.Length / 2; j++)
        //                    {

        //                        tblmcstatu maintable = new tblmcstatu();

        //                        //Taking data from PC
        //                        using (titandmgEntities2 Context = new titandmgEntities2())
        //                        {
        //                            maintable = Context.tblmcstatus.Find(Id[j, 0]);
        //                        }

        //                        ////movingData
        //                        using (titandmgEntities1 Context1 = new titandmgEntities1())
        //                        {
        //                            Context1.Entry(maintable).State = System.Data.Entity.EntityState.Added;
        //                            Context1.SaveChanges();
        //                        }

        //                        Id[j, 1] = 1;

        //                        //deleting data from pc
        //                        using (titandmgEntities2 Context = new titandmgEntities2())
        //                        {
        //                            tblmcstatu mcstatusdata = Context.tblmcstatus.Find(Id[j, 0]);
        //                            Context.tblmcstatus.Remove(mcstatusdata);
        //                            Context.SaveChanges();
        //                        }

        //                    }
        //                }

        //                //moving data from tblmode

        //                var mode = db.tabtblmodes.Where(m => m.CorrectedDate == correcteddatestring);
        //                Id = new int[mode.Count(), 2];
        //                i = 0;
        //                if (mode.Count() > 0)
        //                {
        //                    foreach (var data in mode)
        //                    {
        //                        Id[i, 0] = data.ModeID;
        //                        i = i + 1;
        //                    }

        //                    for (int j = 0; j < Id.Length / 2; j++)
        //                    {
        //                        tblmode maintable = new tblmode();

        //                        //Taking data from PC
        //                        using (titandmgEntities2 Context = new titandmgEntities2())
        //                        {
        //                            maintable = Context.tblmodes.Find(Id[j, 0]);
        //                        }

        //                        ////movingData
        //                        using (titandmgEntities1 Context1 = new titandmgEntities1())
        //                        {
        //                            Context1.Entry(maintable).State = System.Data.Entity.EntityState.Added;
        //                            Context1.SaveChanges();
        //                        }

        //                        Id[j, 1] = 1;

        //                        //deleting data from pc
        //                        using (titandmgEntities2 Context = new titandmgEntities2())
        //                        {
        //                            tblmode tblmode = Context.tblmodes.Find(Id[j, 0]);
        //                            Context.tblmodes.Remove(tblmode);
        //                            Context.SaveChanges();
        //                        }
        //                    }
        //                }

        //                //moving data from tblotpdetails

        //                var otp = db.tabtblotpdetails.Where(m => m.InsertedOn >= LastDayStartTime && m.InsertedOn < LastDayEndTime);
        //                Id = new int[otp.Count(), 2];
        //                i = 0;
        //                if (otp.Count() > 0)
        //                {
        //                    foreach (var data in otp)
        //                    {
        //                        Id[i, 0] = data.OTPId;
        //                        i = i + 1;
        //                    }

        //                    for (int j = 0; j < Id.Length / 2; j++)
        //                    {
        //                        tblotpdetail maintable = new tblotpdetail();

        //                        //Taking data from PC
        //                        using (titandmgEntities2 Context = new titandmgEntities2())
        //                        {
        //                            maintable = Context.tblotpdetails.Find(Id[j, 0]);
        //                        }

        //                        ////movingData
        //                        using (titandmgEntities1 Context1 = new titandmgEntities1())
        //                        {
        //                            Context1.Entry(maintable).State = System.Data.Entity.EntityState.Added;
        //                            Context1.SaveChanges();
        //                        }

        //                        Id[j, 1] = 1;

        //                        //deleting data from pc
        //                        using (titandmgEntities2 Context = new titandmgEntities2())
        //                        {
        //                            tblotpdetail tblotpdetail = Context.tblotpdetails.Find(Id[j, 0]);
        //                            Context.tblotpdetails.Remove(tblotpdetail);
        //                            Context.SaveChanges();
        //                        }
        //                    }
        //                }

        //                //moving data from tblserverstatus

        //                var serverstatus = db.tabtblserverstatus.Where(m => m.CorrectedDate == correcteddatestring);
        //                Id = new int[serverstatus.Count(), 2];
        //                i = 0;
        //                if (serverstatus.Count() > 0)
        //                {
        //                    foreach (var data in serverstatus)
        //                    {
        //                        Id[i, 0] = data.StatusID;
        //                        i = i + 1;
        //                    }

        //                    for (int j = 0; j < Id.Length / 2; j++)
        //                    {
        //                        tblserverstatu maintable = new tblserverstatu();

        //                        //Taking data from PC
        //                        using (titandmgEntities2 Context = new titandmgEntities2())
        //                        {
        //                            maintable = Context.tblserverstatus.Find(Id[j, 0]);
        //                        }

        //                        ////movingData
        //                        using (titandmgEntities1 Context1 = new titandmgEntities1())
        //                        {
        //                            Context1.Entry(maintable).State = System.Data.Entity.EntityState.Added;
        //                            Context1.SaveChanges();
        //                        }

        //                        Id[j, 1] = 1;

        //                        //deleting data from pc
        //                        using (titandmgEntities2 Context = new titandmgEntities2())
        //                        {
        //                            tblserverstatu tblserverstatu = Context.tblserverstatus.Find(Id[j, 0]);
        //                            Context.tblserverstatus.Remove(tblserverstatu);
        //                            Context.SaveChanges();
        //                        }
        //                    }
        //                }

        //                //Modifing the status of moving data table
        //                tbldatamovestatu Movestatusdata = new tbldatamovestatu();
        //                Movestatusdata.CorrectedDate = correctedDate;
        //                Movestatusdata.CreatedBy = 1;
        //                Movestatusdata.CreatedOn = CurrentTime;
        //                Movestatusdata.IsDeleted = 0;
        //                Movestatusdata.MachineID = MachineID;
        //                Movestatusdata.MoveStatus = 1;

        //                db.tabtbldatamovestatus.Add(Movestatusdata);
        //                db.tabSaveChanges();
        //            }

        //        }
        //        #endregion



        //    }

        //}

        #region
        ////Thread for checking idle condition continuasely
        //public void SeprateThreadforIdleCondition(int MachineId)
        //{
        //    //Thread t = new Thread(NewThread);
        //    //t.Start();

        //    var timer = new System.Threading.Timer(e => NewThread(MachineId), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        //}

        //public void NewThread(int MachineID)
        //{
        //    //code goes here
        //    bool TickIdlecheck = CheackIdleEntry(MachineID);
        //    if (TickIdlecheck)
        //    {
        //        //Program entry not happend within given time Redirect to program window
        //        Session["PutAlert"] = "OpenAlert";
        //        return RedirectToAction("IDLE", "IDLE");
        //    }
        //}
        #endregion
    }
}