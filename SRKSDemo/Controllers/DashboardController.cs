
using i_facilitylibrary;
using i_facilitylibrary.DAO;
using Newtonsoft.Json;
using SRKSDemo.Models;
using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Web.Mvc;
using static SRKSDemo.Models.TargetVsActal;

namespace SRKSDemo.Controllers
{
    public class DashboardController : Controller
    {
        private i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
        private IConnectionFactory _conn;
        private Dao obj1 = new Dao();
        private Dao1 obj2 = new Dao1();
        private string databaseName;

        // GET: Dashboard
        public ActionResult Dashboard()
        {
            Session["Errors"] = "";
            return View();
        }

        public ActionResult Index()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            return View();
        }




        private string GetCorrectedDate()
        {
            DateTime correctedDate = DateTime.Now;
            //string Corre = "2019-05-25";
            //DateTime correctedDate = Convert.ToDateTime(Corre);
            tbldaytiming daytimings = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
            if (daytimings != null)
            {
                DateTime Start = Convert.ToDateTime(correctedDate.ToString("yyyy-MM-dd") + " " + daytimings.StartTime);


                //DateTime Start = Convert.ToDateTime(dtMode.Rows[0][0].ToString());
                if (Start <= DateTime.Now)
                {
                    correctedDate = DateTime.Now.Date;
                }
                else
                {
                    correctedDate = DateTime.Now.AddDays(-1).Date;
                }
            }
            string correctedDateformat = correctedDate.ToString("yyyy-MM-dd");
            return correctedDateformat;
        }

        public ActionResult NewDashboard2911()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            return View();
        }

        public List<MachineConnectivityStatusModel> MachineConnectivityDet()
        {
            List<MachineConnectivityStatusModel> model = new List<MachineConnectivityStatusModel>();
            string res = "";
            DateTime nowDate = DateTime.Now;
            string correctedDate = GetCorrectedDate();
            //string correctedDate = "2019-05-24";
            DateTime correctDate = Convert.ToDateTime(correctedDate);
            List<tblmachinedetail> machineDetailsList = new List<tblmachinedetail>();
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                machineDetailsList = db.tblmachinedetails.Where(m => m.IsDeleted == 0).OrderByDescending(m => m.InsertedOn).ToList();
            }
            foreach (tblmachinedetail machineDetails in machineDetailsList)
            {
                int MID = machineDetails.MachineID;
                List<tbllivemode> livemodeData = new List<tbllivemode>();
                using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
                {
                    livemodeData = db.tbllivemodes.Where(m => m.MachineID == MID && m.CorrectedDate == correctDate && m.IsDeleted == 0).ToList();
                }

                var cuttingAndPartsData = livemodeData.Select(m => new { m.ModeID, m.CuttingDuration, m.TotalPartsCount, m.ColorCode, m.MacMode }).OrderByDescending(m => m.ModeID).FirstOrDefault();

                int? PowerTime = livemodeData.Sum(m => m.DurationInSec);
                int Cuttingtime = 0, totalparts = 0;
                string ColorCode = "", MacMode = "";
                if (cuttingAndPartsData != null)
                {
                    Cuttingtime = Convert.ToInt32(cuttingAndPartsData.CuttingDuration);
                    totalparts = Convert.ToInt32(cuttingAndPartsData.TotalPartsCount);
                    ColorCode = cuttingAndPartsData.ColorCode;
                    MacMode = cuttingAndPartsData.MacMode;
                }

                Cuttingtime = GetParts_Cutting(MID, correctDate, out totalparts);
                var machinmodes = livemodeData.Select(m => new { m.MacMode, m.ColorCode, m.DurationInSec }).ToList();
                MachineConnectivityStatusModel databind = new MachineConnectivityStatusModel();
                databind.MachineName = machineDetails.MachineDisplayName;
                databind.MachineID = machineDetails.MachineID;
                double IdleTime = Convert.ToDouble(machinmodes.Where(m => m.ColorCode == "YELLOW").ToList().Sum(m => m.DurationInSec));

                double running = Convert.ToDouble(machinmodes.Where(m => m.ColorCode == "GREEN").ToList().Sum(m => m.DurationInSec));
                VirtualHMI objvirtual = new VirtualHMI(machineDetails.IPAddress, machineDetails.MachineName);
                double CycleTime = 0;
                short exeprogramnum = 0;
                ushort h;
                int AxisCount = 32;
                List<string> retValList = new List<string>();
                List<AxisDetails> AxisDetailsList = new List<AxisDetails>();
                objvirtual.VirtualDispRefersh(AxisCount, out retValList, out AxisDetailsList);
                string programnum = retValList[6].ToString();
                objvirtual.UTFValuesforMachine(out CycleTime, out exeprogramnum, out h);
                TimeSpan tmrunning = TimeSpan.FromSeconds(running);
                TimeSpan tmIdle = TimeSpan.FromSeconds(IdleTime);
                TimeSpan tm1 = TimeSpan.FromMinutes(CycleTime);
                TimeSpan tm2 = TimeSpan.FromSeconds(Convert.ToDouble(PowerTime));
                TimeSpan tm3 = TimeSpan.FromMinutes(Convert.ToDouble(Cuttingtime));
                databind.RunningTime = tmrunning.ToString(@"hh\:mm\:ss");
                databind.IdleTime = tmIdle.ToString(@"hh\:mm\:ss");
                databind.CycleTime = tm1.ToString(@"hh\:mm\:ss");
                //databind.ExeProgramName = programnum.ToString();
                databind.Color = ColorCode;
                databind.CurrentStatus = MacMode;
                databind.PowerOnTime = tm2.ToString(@"hh\:mm\:ss");
                databind.CuttingTime = tm3.ToString(@"hh\:mm\:ss");
                databind.PartsCount = totalparts;
                if (running == 0)
                {
                    running = 1;
                }

                running = (running / 60);
                databind.CuttingRatio = Math.Round(Convert.ToDecimal((Cuttingtime / running)) * 100, 2).ToString();
                model.Add(databind);
            }
            res = JsonConvert.SerializeObject(model);

            return model;
        }

        public string GetAllMachineDetails()
        {
            List<MachineConnectivityStatusModel> model = new List<MachineConnectivityStatusModel>();
            string res = "";
            DateTime nowDate = DateTime.Now;
            string correctedDate = GetCorrectedDate();
            DateTime correctDate = Convert.ToDateTime(correctedDate);
            List<tblmachinedetail> machineDetailsList = new List<tblmachinedetail>();
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                machineDetailsList = db.tblmachinedetails.Where(m => m.IsDeleted == 0).OrderByDescending(m => m.InsertedOn).ToList();
            }
            foreach (tblmachinedetail machineDetails in machineDetailsList)
            {
                int MID = machineDetails.MachineID;
                List<tbllivemode> livemodeData = new List<tbllivemode>();
                using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
                {
                    livemodeData = db.tbllivemodes.Where(m => m.MachineID == MID && m.CorrectedDate == correctDate && m.IsDeleted == 0).ToList();
                }

                var cuttingAndPartsData = livemodeData.Select(m => new { m.ModeID, m.CuttingDuration, m.TotalPartsCount, m.ColorCode, m.MacMode }).OrderByDescending(m => m.ModeID).FirstOrDefault();

                int? PowerTime = livemodeData.Sum(m => m.DurationInSec);
                int Cuttingtime = 0, totalparts = 0;
                string ColorCode = "", MacMode = "";
                if (cuttingAndPartsData != null)
                {
                    Cuttingtime = Convert.ToInt32(cuttingAndPartsData.CuttingDuration);
                    totalparts = Convert.ToInt32(cuttingAndPartsData.TotalPartsCount);
                    ColorCode = cuttingAndPartsData.ColorCode;
                    MacMode = cuttingAndPartsData.MacMode;
                }

                Cuttingtime = GetParts_Cutting(MID, correctDate, out totalparts);
                var machinmodes = livemodeData.Select(m => new { m.MacMode, m.ColorCode, m.DurationInSec }).ToList();
                MachineConnectivityStatusModel databind = new MachineConnectivityStatusModel();
                databind.MachineName = machineDetails.MachineDisplayName;
                databind.MachineID = machineDetails.MachineID;
                double IdleTime = Convert.ToDouble(machinmodes.Where(m => m.ColorCode == "YELLOW").ToList().Sum(m => m.DurationInSec));

                double running = Convert.ToDouble(machinmodes.Where(m => m.ColorCode == "GREEN").ToList().Sum(m => m.DurationInSec));
                VirtualHMI objvirtual = new VirtualHMI(machineDetails.IPAddress, machineDetails.MachineName);
                double CycleTime = 0;
                short exeprogramnum = 0;
                ushort h;
                int AxisCount = 32;
                List<string> retValList = new List<string>();
                List<AxisDetails> AxisDetailsList = new List<AxisDetails>();
                objvirtual.VirtualDispRefersh(AxisCount, out retValList, out AxisDetailsList);
                //string programnum = retValList[6].ToString();
                objvirtual.UTFValuesforMachine(out CycleTime, out exeprogramnum, out h);
                TimeSpan tmrunning = TimeSpan.FromSeconds(running);
                TimeSpan tmIdle = TimeSpan.FromSeconds(IdleTime);
                TimeSpan tm1 = TimeSpan.FromMinutes(CycleTime);
                TimeSpan tm2 = TimeSpan.FromSeconds(Convert.ToDouble(PowerTime));
                TimeSpan tm3 = TimeSpan.FromMinutes(Convert.ToDouble(Cuttingtime));
                databind.RunningTime = tmrunning.ToString(@"hh\:mm\:ss");
                databind.IdleTime = tmIdle.ToString(@"hh\:mm\:ss");
                databind.CycleTime = tm1.ToString(@"hh\:mm\:ss");
                //databind.ExeProgramName = programnum.ToString();
                databind.Color = ColorCode;
                databind.CurrentStatus = MacMode;
                databind.PowerOnTime = tm2.ToString(@"hh\:mm\:ss");
                databind.CuttingTime = tm3.ToString(@"hh\:mm\:ss");
                databind.PartsCount = totalparts;
                if (running == 0)
                {
                    running = 1;
                }

                running = (running / 60);
                databind.CuttingRatio = Math.Round(Convert.ToDecimal((Cuttingtime / running)) * 100, 2).ToString();
                model.Add(databind);
            }
            res = JsonConvert.SerializeObject(model);

            return res;
        }

        //public string MachineDashboard()
        //{

        //    string correctedDate = GetCorrectedDate();  // get CorrectedDate
        //    string res = "";                      // string correctedDate = "2018-08-23";

        //    DateTime correctedDate1 = Convert.ToDateTime(correctedDate);
        //    using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
        //    {

        //        int c = 0;
        //        List<GetMachines> AllMachinesList = new List<GetMachines>();
        //        List<MachineConnectivityStatusModel> machineModel = new List<MachineConnectivityStatusModel>();

        //        var machdet = db.tblmachinedetails.Where(m => m.IsDeleted == 0).OrderBy(m => m.MachineID).ToList();
        //        foreach (var row in machdet)
        //        {
        //            MachineConnectivityStatusModel machinedet = new MachineConnectivityStatusModel();
        //            GetMachines machinesdata = new GetMachines();

        //            List<Machines> machineList = new List<Machines>();
        //            machinesdata.cellName = row.tblcell.CelldisplayName;
        //            machinesdata.plantName = row.tblplant.PlantDisplayName;
        //            machinesdata.shopName = row.tblshop.Shopdisplayname;

        //            //Previous
        //            //machinedet.cellName = row.CellName;
        //            //machinedet.plantName = row.tblplant.PlantDisplayName;
        //            //machinedet.shopName = row.tblshop.Shopdisplayname;

        //            var machineslist = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.CellID == row.CellID).ToList();
        //            foreach (var machine in machineslist)
        //            {
        //                Machines machines = new Machines();
        //            int machineID = row.MachineID;
        //            var modetails = db.tbllivemodes.Where(m => m.MachineID == machineID && m.IsDeleted == 0 && m.IsCompleted == 0 && m.CorrectedDate == correctedDate1.Date).OrderByDescending(m => m.ModeID).FirstOrDefault();
        //            if (modetails != null)
        //            {
        //                machines.Color = modetails.ColorCode;
        //                machines.CurrentStatus = modetails.MacMode;
        //            }
        //            machines.MachineName = row.MachineDisplayName;
        //            machines.MachineID = machineID;
        //            machines.Time = DateTime.Now.ToShortTimeString();
        //            machineList.Add(machines);

        //            }
        //            // machinedet.machines = machineList;
        //            machinesdata.machines = machineList;
        //            if (c == 0)
        //            {
        //                machineModel = MachineConnectivityDet();
        //                machinesdata.machineModel = machineModel;
        //                c = c + 1;
        //            }



        //            AllMachinesList.Add(machinesdata);
        //            //machineModel.Add(machinedet);
        //        }
        //        AllMachinesList = AllMachinesList.OrderBy(m => m.cellName).ToList();
        //        res = JsonConvert.SerializeObject(AllMachinesList);
        //    }
        //    return res;
        //}

        public string MachineDashboard()
        {

            string correctedDate = GetCorrectedDate();  // get CorrectedDate
            string res = "";                      // string correctedDate = "2018-08-23";

            DateTime correctedDate1 = Convert.ToDateTime(correctedDate);
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {

                int c = 0;
                List<GetMachines> AllMachinesList = new List<GetMachines>();
                List<MachineConnectivityStatusModel> machineModel = new List<MachineConnectivityStatusModel>();

                List<tblcell> celldet = db.tblcells.Where(m => m.IsDeleted == 0).OrderBy(m => m.CellID).ToList();
                foreach (tblcell row in celldet)
                {
                    MachineConnectivityStatusModel machinedet = new MachineConnectivityStatusModel();
                    GetMachines machinesdata = new GetMachines();

                    List<Machines> machineList = new List<Machines>();
                    machinesdata.cellName = row.CelldisplayName;
                    machinesdata.plantName = row.tblplant.PlantDisplayName;
                    machinesdata.shopName = row.tblshop.Shopdisplayname;

                    //Previous
                    //machinedet.cellName = row.CellName;
                    //machinedet.plantName = row.tblplant.PlantDisplayName;
                    //machinedet.shopName = row.tblshop.Shopdisplayname;

                    List<tblmachinedetail> machineslist = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.CellID == row.CellID).ToList();
                    foreach (tblmachinedetail machine in machineslist)
                    {
                        Machines machines = new Machines();
                        int machineID = machine.MachineID;
                        tbllivemode modetails = db.tbllivemodes.Where(m => m.MachineID == machineID && m.IsDeleted == 0 && m.IsCompleted == 0 && m.CorrectedDate == correctedDate1.Date).OrderByDescending(m => m.ModeID).FirstOrDefault();
                        if (modetails != null)
                        {
                            machines.Color = modetails.ColorCode;
                            machines.CurrentStatus = modetails.MacMode;
                        }
                        machines.MachineName = machine.MachineName;
                        machines.MachineID = machine.MachineID;
                        machines.Time = DateTime.Now.ToShortTimeString();
                        machineList.Add(machines);

                    }
                    ///  machinedet.machines = machineList;
                    machinesdata.machines = machineList;
                    if (c == 0)
                    {
                        machineModel = MachineConnectivityDet();
                        machinesdata.machineModel = machineModel;
                        c = c + 1;
                    }



                    AllMachinesList.Add(machinesdata);
                    //machineModel.Add(machinedet);
                }
                AllMachinesList = AllMachinesList.OrderBy(m => m.cellName).ToList();
                res = JsonConvert.SerializeObject(AllMachinesList);
            }
            return res;
        }

        public ActionResult MConnectivity2()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            string correctedDate = DateTime.Now.ToString("yyyy-MM-dd");
            string correctdate = correctedDate;
            DateTime correctedDate1 = Convert.ToDateTime(correctdate);

            return View();
        }

        public List<MachineUtilizationModel> MachineUtilization()
        {
            List<MachineUtilizationModel> machineUtilizationList = new List<MachineUtilizationModel>();
            List<tblmachinedetail> machinedetails = new List<tblmachinedetail>();
            //var celldet = new List<tblcell>();
            //using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            //{
            //string correctedDate = "2017-11-18";
            string correctedDate = GetCorrectedDate();
            DateTime correctdate = Convert.ToDateTime(correctedDate);
            DateTime StartTime1 = Convert.ToDateTime(System.DateTime.Now.ToString("yyyy-MM-dd 06:00:00"));
            DateTime CurrentTime = System.DateTime.Now;
            double MinorLosses = 0; int ModeDuration = 0; int ModeDuration1 = 0;
            if (CurrentTime.Hour >= 0 && CurrentTime.Hour < 6)
            {
                StartTime1 = StartTime1.AddDays(-1);
                //fromdate = fromdate.AddDays(-1);
            }
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                //celldet = db.tblcells.Where(m => m.IsDeleted == 0).ToList();
                machinedetails = db.tblmachinedetails.Where(m => m.IsDeleted == 0).OrderByDescending(m => m.InsertedOn).ToList();
            }
            if (machinedetails != null)
            {
                machinedetails = machinedetails.OrderBy(m => m.MachineID).ToList();
                foreach (tblmachinedetail machine in machinedetails)
                {
                    MachineUtilizationModel mum = new MachineUtilizationModel();
                    int machineID = machine.MachineID;
                    string machineName = machine.MachineDisplayName;
                    //var cellName = celldet.Where(m => m.CellID == machine.CellID).FirstOrDefault();
                    tbllivemode tblmode = db.tbllivemodes.Where(m => m.IsDeleted == 0 && m.IsCompleted == 0 && m.MachineID == machineID && m.CorrectedDate == correctdate).FirstOrDefault();
                    //var machinmodes = db.tbllivemodes.Where(m => m.MachineID == machineID && m.CorrectedDate == correctdate && m.IsDeleted == 0).Select(m => new { m.MacMode, m.ColorCode, m.DurationInSec }).ToList();
                    //double RunningTimeinsec = Convert.ToDouble(machinmodes.Where(m => m.ColorCode == "GREEN").ToList().Sum(m => m.DurationInSec));
                    //double Runningtimeinmin = (RunningTimeinsec /60);

                    int OpeTime = db.tblmimics.Where(m => m.CorrectedDate == correctedDate && m.MachineID == machineID).Select(m => m.OperatingTime).FirstOrDefault();

                    // To calculate OPTIME for completed and running Modes.
                    using (MsqlConnection mc = new MsqlConnection())
                    {
                        DataTable OP = new DataTable();
                        DataTable RunningOP = new DataTable();
                        mc.open();
                        String GetDurationQuery = "SELECT SUM(DurationInSec) from " + databaseName + ".[tbllivemode] where MachineID = " + machineID + " and IsDeleted = 0 and CorrectedDate = '" + correctedDate + "' and ColorCode = 'GREEN' and IsCompleted = 1;";
                        SqlDataAdapter GetDurationDA = new SqlDataAdapter(GetDurationQuery, mc.msqlConnection);
                        GetDurationDA.Fill(OP);
                        mc.close();

                        mc.open();
                        String GetRunningDurationQuery = "SELECT StartTime from " + databaseName + ".[tbllivemode] where MachineID = " + machineID + " and IsDeleted = 0 and CorrectedDate = '" + correctedDate + "' and ColorCode = 'GREEN' and IsCompleted = 0;";
                        SqlDataAdapter GetRunningDurationDA = new SqlDataAdapter(GetRunningDurationQuery, mc.msqlConnection);
                        GetRunningDurationDA.Fill(RunningOP);
                        mc.close();

                        if (OP.Rows.Count != 0)
                        {
                            String Val = OP.Rows[0][0].ToString();
                            if (OP.Rows[0][0].ToString() != null && Val != "")
                            {
                                ModeDuration = Convert.ToInt32(OP.Rows[0][0]) / 60;

                            }

                        }
                        if (RunningOP.Rows.Count != 0)
                        {
                            DateTime StartTimeRunnning = Convert.ToDateTime(RunningOP.Rows[0][0]);
                            int DurationRunning = (int)DateTime.Now.Subtract(StartTimeRunnning).TotalSeconds / 60;
                            ModeDuration1 = DurationRunning;
                        }

                    }
                    TimeSpan StartTime = db.tbldaytimings.Where(d => d.IsDeleted == 0).Select(d => d.StartTime).FirstOrDefault();
                    TimeSpan correctedTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                    MinorLosses = GetMinorLossesToday(correctedDate, StartTime1.ToString("yyyy-MM-dd HH:mm:ss"), CurrentTime.ToString("yyyy-MM-dd HH:mm:ss"), machineID, "yellow");
                    if (MinorLosses < 0)
                    {
                        MinorLosses = 0;
                    }
                    String CorrectedStartTime = correctedDate + " " + StartTime;
                    double TotalTimeTaken = DateTime.Now.Subtract(Convert.ToDateTime(CorrectedStartTime)).TotalMinutes;
                    //GetParts_Cutting(machine.MachineID, correctdate, out PartsCount);
                    //double LoadtimewithProd = (double)(machine.StdLoadingTime + machine.StdUnLoadingTime) * PartsCount;
                    //double plannedBrkDurationinMin = 0;
                    //List<tblplannedbreak> plannedbrks = db.tblplannedbreaks.Where(m => m.IsDeleted == 0).ToList();
                    //foreach (tblplannedbreak row in plannedbrks)
                    //{
                    //    plannedBrkDurationinMin += Convert.ToDateTime(correctdate.Date.ToString("yyyy-MM-dd") + " " + row.EndTime).Subtract(Convert.ToDateTime(correctdate.Date.ToString("yyyy-MM-dd") + " " + row.StartTime)).TotalSeconds;
                    //}
                    mum.CellName = machine.MachineDisplayName;
                    double totaltimetaken = Convert.ToDouble(TotalTimeTaken);
                    double Availability = totaltimetaken /*- plannedBrkDurationinMin*/;
                    double Runningtimeinmin = ModeDuration + ModeDuration1;
                    double MachineUtilization = ((OpeTime) / Availability) * 100;
                    if (MachineUtilization > 100)
                    {
                        MachineUtilization = 100;
                    }

                    MachineUtilization = Math.Round(MachineUtilization, 2);
                    mum.MachineName = machine.MachineDisplayName;
                    mum.MachineUtiization = MachineUtilization;
                    mum.CurrentTime = correctedTime;
                    machineUtilizationList.Add(mum);
                }
            }

            return machineUtilizationList;
        }

        public string GetMachineUtilization()
        {
            string res = "";
            ViewModel model = new ViewModel();
            model.MachineUtilizationModels = MachineUtilization();
            //Alarms();
            model.AlarmLists = GetAlarms();
            res = JsonConvert.SerializeObject(model);
            return res;

        }

        public string OEEs()
        {
            List<OEEModel> model = new List<OEEModel>();
            string result = "";
            string OEEOP = "";
            string cellName = "";
            string[] backgroundcolr;
            string[] borderColor;
            //string correctedDate = "2017-11-18";
            string correctedDate = GetCorrectedDate();
            DateTime correctdate = Convert.ToDateTime(correctedDate);

            List<tblmachinedetail> machineDetails = new List<tblmachinedetail>();
            List<tblshop> shopdet = new List<tblshop>();
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                machineDetails = db.tblmachinedetails.Where(m => m.IsDeleted == 0).ToList();

                //shopdet = db.tblshops.Where(m => m.IsDeleted == 0).ToList();
            }
            int count = 0;
            if (machineDetails.Count > 0)
            {
                foreach (tblmachinedetail machine in machineDetails)
                {

                    Color color = GetRandomColour();
                    string val = "rgba(" + color.R.ToString() + "," + color.G.ToString() + "," + color.B.ToString() + "," + color.A.ToString() + ")";
                    count = count + 1;
                    borderColor = new string[] { val, val, val, val };
                    backgroundcolr = new string[] { val, val, val, val };
                    //var shop = shopdet.Where(m => m.ShopID == cell.ShopID).FirstOrDefault();

                    int machineId = machine.MachineID;
                    //cellName = shop.Shopdisplayname + " - " + cell.CelldisplayName;
                    cellName = machine.MachineDisplayName;
                    double AvailabilityPercentage = 0;
                    double PerformancePercentage = 0;
                    double QualityPercentage = 0;
                    double OEEPercentage = 0;


                    OEE(correctdate, machineId, out AvailabilityPercentage, out PerformancePercentage, out QualityPercentage, out OEEPercentage); // GET OEE

                    OEEModel OEEListData = new OEEModel();
                    OEEListData.CellName = cellName;
                    OEEListData.CellID = machineId;
                    //OEEListData.Target = Target;
                    //OEEListData.Actual = Actual;
                    double[] objdata = new double[] { AvailabilityPercentage, PerformancePercentage, QualityPercentage, OEEPercentage };

                    OEEListData.backgroundColor = backgroundcolr;
                    OEEListData.borderColor = borderColor;
                    OEEListData.data = objdata;
                    model.Add(OEEListData);

                }
                OEEOP = JsonConvert.SerializeObject(model);
                result = OEEOP;
            }
            return result;
        }

        private static readonly Random rand = new Random();

        private Color GetRandomColour()
        {
            return Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
        }

        //public void OEE(int machineId, out double AvailabilityPercentage, out double PerformancePercentage, out double QualityPercentage, out double OEEPercentage, out int Actual, out int Target)
        //{
        //    string correctdate = GetCorrectedDate();
        //    //DateTime correctedDate1 = Convert.ToDateTime(correctdate);
        //    string correctedDate1 = Convert.ToString(correctdate);
        //    DateTime correctedDate = Convert.ToDateTime(correctdate);
        //    decimal OperatingTime = 0;
        //    decimal LossTime = 0;
        //    decimal MinorLossTime = 0;
        //    decimal MntTime = 0;
        //    decimal SetupTime = 0;
        //    Actual = 0;
        //    Target = 0;
        //    //decimal SetupMinorTime = 0;
        //    decimal PowerOffTime = 0;
        //    decimal PowerONTime = 0;
        //    //decimal Utilization = 0;
        //    decimal DayOEEPercent = 0;
        //    //int PerformanceFactor = 0;
        //    //decimal Quality = 0;
        //    int TotlaQty = 0;
        //    int YieldQty = 0;
        //    int Del_Qty = 0;
        //    int BottleNeckYieldQty = 0;
        //    //decimal IdealCycleTimeVal = 2;
        //    decimal plannedCycleTime = 0;
        //    decimal LoadingTime = 0;
        //    decimal UnloadingTime = 0;

        //    double plannedBrkDurationinMin = 0;
        //    decimal LoadingUnloadingWithProd = 0;
        //    decimal LoadingUnloadingwithProdBottleNeck = 0;
        //    int minorstoppage = 0;
        //    //decimal TotalProductoin = 0;
        //    decimal Availability;
        //    decimal totalqty = 0;
        //    //  string plantName = row.tblplant.PlantName;
        //    List<tblmachinedetail> machineslist = new List<tblmachinedetail>();
        //    tblbottelneck bottleneckmachines = new tblbottelneck();
        //    // var scrap = new List<tbllivehmiscreen>();
        //    List<tblrejectqty> scrapqty1 = new List<tblrejectqty>();
        //    tblcellpart cellpartDet = new tblcellpart();
        //    tblpart partsDet = new tblpart();
        //    AvailabilityPercentage = 0;
        //    QualityPercentage = 0;
        //    PerformancePercentage = 0;
        //    OEEPercentage = 0;
        //    using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
        //    {
        //        List<tbllivehmiscreen> rawdata = new List<tbllivehmiscreen>();
        //        List<tbllivehmiscreen> scrap = db.tbllivehmiscreens.Where(m => m.MachineID == machineId && m.CorrectedDate == correctdate).OrderByDescending(m => m.HMIID).ToList();  //workorder entry
        //        foreach (tbllivehmiscreen row1 in scrap)
        //        {
        //            // Get Machines               
        //            machineslist = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.MachineID == machineId /* m.MachineID == bottleneckmachines.MachineID*/).OrderBy(m => m.MachineID).ToList();
        //            foreach (tblmachinedetail machine in machineslist)
        //            {
        //                Machines machines = new Machines();
        //                int machineID = machine.MachineID;
        //                // Mode details
        //                minorstoppage = Convert.ToInt32(machine.MachineIdleMin) * 60; // in sec
        //                List<tbllivemode> GetModeDurations = new List<tbllivemode>();
        //                //using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
        //                //{
        //                GetModeDurations = db.tbllivemodes.Where(m => m.MachineID == machineID && m.CorrectedDate == correctedDate.Date && m.IsCompleted == 1).ToList();
        //                // }
        //                //List<i_facilitylibrary.DAL.tbloeedashboardvariable> OEEDataToSummarize = obj1.GettbloeeListDet4(id, fromdate, todate);

        //                totalqty = ((decimal)row1.Delivered_Qty - (decimal)row1.Rej_Qty);
        //                Del_Qty = (Int32)row1.Delivered_Qty;
        //                OperatingTime = Convert.ToDecimal(GetModeDurations.Where(m => m.ModeType == "PROD").ToList().Sum(m => m.DurationInSec));
        //                PowerOffTime = Convert.ToDecimal(GetModeDurations.Where(m => m.ModeType == "POWEROFF").ToList().Sum(m => m.DurationInSec));
        //                MntTime = Convert.ToDecimal(GetModeDurations.Where(m => m.ModeType == "MNT").ToList().Sum(m => m.DurationInSec));
        //                MinorLossTime = Convert.ToDecimal(GetModeDurations.Where(m => m.ModeType == "IDLE" && m.DurationInSec < minorstoppage).ToList().Sum(m => m.DurationInSec));
        //                LossTime = Convert.ToDecimal(GetModeDurations.Where(m => m.ModeType == "IDLE" && m.DurationInSec > minorstoppage).ToList().Sum(m => m.DurationInSec));
        //                PowerONTime = Convert.ToDecimal(GetModeDurations.Where(m => m.ModeType == "POWERON").ToList().Sum(m => m.DurationInSec));
        //                OperatingTime = Math.Round((OperatingTime / 60), 2);
        //                PowerOffTime = (PowerOffTime / 60);
        //                MntTime = (MntTime / 60);
        //                MinorLossTime = (MinorLossTime / 60);
        //                LossTime = (LossTime / 60);
        //                PowerONTime = (PowerONTime / 60);
        //                List<tblplannedbreak> plannedbrks = db.tblplannedbreaks.Where(m => m.IsDeleted == 0).ToList();
        //                foreach (tblplannedbreak row in plannedbrks)
        //                {
        //                    plannedBrkDurationinMin += Convert.ToDateTime(correctedDate.Date.ToString("yyyy-MM-dd") + " " + row.EndTime).Subtract(Convert.ToDateTime(correctedDate.Date.ToString("yyyy-MM-dd") + " " + row.StartTime)).TotalMinutes;
        //                }
        //                foreach (tbllivemode ModeRow in GetModeDurations)
        //                {
        //                    if (ModeRow.ModeType == "SETUP")
        //                    {
        //                        try
        //                        {
        //                            SetupTime += (decimal)Convert.ToDateTime(ModeRow.LossCodeEnteredTime).Subtract(Convert.ToDateTime(ModeRow.StartTime)).TotalMinutes;
        //                            //SetupMinorTime += (decimal)(db.tblSetupMaints.Where(m => m.ModeID == ModeRow.ModeID).Select(m => m.MinorLossTime).First() / 60.00);
        //                        }
        //                        catch { }
        //                    }
        //                }
        //                List<tbllivemode> GetModeDurationsRunning = new List<tbllivemode>();
        //                //using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
        //                //{
        //                GetModeDurationsRunning = db.tbllivemodes.Where(m => m.MachineID == machineID && m.CorrectedDate == correctedDate.Date && m.IsCompleted == 0).ToList();
        //                // }
        //                foreach (tbllivemode ModeRow in GetModeDurationsRunning)
        //                {
        //                    String ColorCode = ModeRow.ColorCode;
        //                    DateTime StartTime = (DateTime)ModeRow.StartTime;
        //                    decimal Duration = (decimal)System.DateTime.Now.Subtract(StartTime).TotalMinutes;
        //                    if (ColorCode == "YELLOW")
        //                    {
        //                        LossTime += Duration;
        //                    }
        //                    else if (ColorCode == "GREEN")
        //                    {
        //                        OperatingTime += Duration;
        //                    }
        //                    else if (ColorCode == "RED")
        //                    {
        //                        MntTime += Duration;
        //                    }
        //                    else if (ColorCode == "BLUE")
        //                    {
        //                        PowerOffTime += Duration;
        //                    }
        //                }
        //                LoadingTime += Convert.ToDecimal(partsDet.StdLoadingTime);
        //                UnloadingTime += Convert.ToDecimal(partsDet.StdUnLoadingTime);
        //            }
        //        }
        //        //int bottleneckMachineID = bottleneckmachines.MachineID;
        //        int bottleneckMachineID = 0;
        //        TotlaQty = GetQuantiy(machineId, correctedDate, out YieldQty, out BottleNeckYieldQty, bottleneckMachineID);
        //        Actual = YieldQty;
        //        if (YieldQty == 0)
        //        {
        //            YieldQty = 1;
        //        }

        //        LoadingUnloadingWithProd = ((LoadingTime + UnloadingTime) * YieldQty) / 60;
        //        LoadingUnloadingwithProdBottleNeck = ((LoadingTime + UnloadingTime) * BottleNeckYieldQty) / 60;
        //        MinorLossTime = MinorLossTime - LoadingUnloadingWithProd;
        //        //decimal OPwithMinorStoppage = (OperatingTime + LoadingUnloadingWithProd + MinorLossTime);
        //        decimal OPwithMinorStoppage = (OperatingTime + MinorLossTime);
        //        decimal utilFactor = Math.Round((LoadingUnloadingWithProd + OperatingTime), 2);
        //        decimal IdleTime = LossTime;
        //        decimal BDTime = MntTime;
        //        int TotalTime = Convert.ToInt32(PowerONTime) + Convert.ToInt32(OperatingTime) + Convert.ToInt32(IdleTime) + Convert.ToInt32(BDTime) + Convert.ToInt32(PowerOffTime);
        //        //int TotalTime = 24 * 60;

        //        if (TotalTime == 0)
        //        {
        //            TotalTime = 1;
        //        }
        //        if (TotlaQty == 0)
        //        {
        //            TotlaQty = 1;
        //        }

        //        decimal plannedCycleTimeInMin = Math.Round((plannedCycleTime / 60), 2);
        //        decimal IdealCycleTimeinMin = Convert.ToDecimal(plannedCycleTimeInMin);
        //        int LoadunloadTimeinMin = ((int)LoadingTime + (int)UnloadingTime) / 60;
        //        if (IdealCycleTimeinMin < 1)
        //        {
        //            IdealCycleTimeinMin = 1;
        //        }

        //        decimal Targetdec = (TotalTime / (IdealCycleTimeinMin + LoadunloadTimeinMin));
        //        Target = Convert.ToInt32(Targetdec);
        //        if (TotalTime > (int)plannedBrkDurationinMin)
        //        {
        //            Availability = Math.Round((TotalTime - (decimal)plannedBrkDurationinMin), 2);
        //        }
        //        else
        //        {
        //            Availability = TotalTime;
        //        }

        //        if (OPwithMinorStoppage == 0)
        //        {
        //            OPwithMinorStoppage = 1;
        //        }

        //        decimal TotalTimeWithPlannedBrk = Availability;
        //        decimal AvailabilityPercent = Math.Round((OPwithMinorStoppage / TotalTimeWithPlannedBrk), 2) * 100;  // From BottleNeckMachine
        //        if (AvailabilityPercent > 100)
        //        {
        //            AvailabilityPercent = 100;
        //        }

        //        if (OperatingTime == 0)
        //        {
        //            OperatingTime = 1;
        //        }

        //        decimal PerformanceBottelNeck = Math.Round(((plannedCycleTimeInMin * Del_Qty) / OperatingTime), 2) * 100;
        //        decimal performanceFactor = (plannedCycleTime * YieldQty);
        //        if (PerformanceBottelNeck > 100)
        //        {
        //            PerformanceBottelNeck = 100;
        //        }
        //        //decimal QualityLastMachine = Math.Round((decimal)((YieldQty - reject) / YieldQty), 2) * 100;            // From LastMachine
        //        if (Del_Qty != 0)
        //        {
        //            decimal QualityLastMachine = Math.Round((totalqty) / Del_Qty, 2) * 100;
        //            if (QualityLastMachine > 100)
        //            {
        //                QualityLastMachine = 100;
        //            }

        //            DayOEEPercent = (decimal)Math.Round((double)(AvailabilityPercent / 100) * (double)(PerformanceBottelNeck / 100) * (double)(QualityLastMachine / 100), 2) * 100;
        //            if (AvailabilityPercent == 0)
        //            {
        //                QualityLastMachine = 0;
        //                PerformanceBottelNeck = 0;
        //                DayOEEPercent = 0;
        //            }

        //            AvailabilityPercentage = (double)AvailabilityPercent;
        //            QualityPercentage = (double)QualityLastMachine;
        //            PerformancePercentage = (double)PerformanceBottelNeck;
        //            OEEPercentage = (double)DayOEEPercent;
        //        }
        //        // }
        //    }
        //}

        public string GetIPAddressOf()
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj2 = new Dao1(_conn);
            string IP_Address = null;
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    IP_Address = addresses[0];
                }
            }
            //Use this for client IP Address
            IP_Address = context.Request.ServerVariables["REMOTE_ADDR"];

            return IP_Address;
        }

        #region OEE Calculation for TODAY  

        // Copied form OEEDashboard controller on 2020-07-28

        //For Today OEE Calculation
        //public void CalculateOEEToday(DateTime fromdate, DateTime todate, int MachineID, string TimeType, string SummarizeAs = null)
        public void OEE(DateTime fromdate, int machineId, out double AvailabilityPercentage, out double PerformancePercentage, out double QualityPercentage, out double OEEPercentage)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj2 = new Dao1(_conn); DateTime CurrentTime = System.DateTime.Now;
            DateTime StartTime = Convert.ToDateTime(System.DateTime.Now.ToString("yyyy-MM-dd 06:00:00"));
            AvailabilityPercentage = 0;
            QualityPercentage = 0;
            PerformancePercentage = 0;
            OEEPercentage = 0;

            double AvailableTime = 0;

            if (CurrentTime.Hour >= 0 && CurrentTime.Hour < 6)
            {
                StartTime = StartTime.AddDays(-1);
                //fromdate = fromdate.AddDays(-1);
            }

            DateTime UsedDateForExcel = Convert.ToDateTime(fromdate);
            DateTime correctedDate = DateTime.Now.Date;
            //double TotalDay = todate.Subtract(fromdate).TotalDays;
            #region

            string ipAddress = GetIPAddressOf();
            List<i_facilitylibrary.DAL.tbloeedashboardvariablestoday> OEEDataPresent = obj2.GettbloeeListDet3(machineId, ipAddress, Convert.ToDateTime(UsedDateForExcel));
            //List<i_facilitylibrary.DAL.tbloeedashboardvariablestoday> OEEDataPresent = obj2.GettbloeeListDet31(machineId, Convert.ToDateTime(Correcteddate));
            // var OEEDataPresent = db.tbloeedashboardvariablestodays.Where(m => m.WCID == MachineID && m.StartDate == UsedDateForExcel && m.IPAddress == ipAddress).ToList();
            if (OEEDataPresent.Count == 0 || OEEDataPresent.Count != 0)
            {
                double OperatingTime = 0;
                // int green = 0;
                int ModeDuration = 0; int ModeDuration1 = 0;
                double SummationOfSCTvsPP = 0, MinorLosses = 0;
                double ScrapQtyTime = 0, ReWOTime = 0;

                MinorLosses = GetMinorLossesToday(UsedDateForExcel.ToString("yyyy-MM-dd"), StartTime.ToString("yyyy-MM-dd HH:mm:ss"), CurrentTime.ToString("yyyy-MM-dd HH:mm:ss"), machineId, "yellow");
                if (MinorLosses < 0)
                {
                    MinorLosses = 0;
                }
                //blue = GetOPIDleBreakDownToday(UsedDateForExcel.ToString("yyyy-MM-dd"), StartTime.ToString("yyyy-MM-dd HH:mm:ss"), CurrentTime.ToString("yyyy-MM-dd HH:mm:ss"), machineId, "blue");
                // green = GetOPIDleBreakDownToday(UsedDateForExcel.ToString("yyyy-MM-dd"), StartTime.ToString("yyyy-MM-dd HH:mm:ss"), CurrentTime.ToString("yyyy-MM-dd HH:mm:ss"), machineId, "green");

                //green = ("SELECT SUM(DurationInSec) from " + databaseName + ".[tbllivemode] where MachineID = " + machineId + " and IsDeleted = 0 and CorrectedDate = '" + correctedDate.ToString("yyyy-MM-dd")+ "' and ColorCode = ' GREEN ' and IsCompleted = 1;");

                using (MsqlConnection mc = new MsqlConnection())
                {
                    DataTable OP = new DataTable();
                    DataTable RunningOP = new DataTable();
                    mc.open();
                    String GetDurationQuery = "SELECT SUM(DurationInSec) from " + databaseName + ".[tbllivemode] where MachineID = " + machineId + " and IsDeleted = 0 and CorrectedDate = '" + correctedDate + "' and ColorCode = 'GREEN' and IsCompleted = 1;";
                    SqlDataAdapter GetDurationDA = new SqlDataAdapter(GetDurationQuery, mc.msqlConnection);
                    GetDurationDA.Fill(OP);
                    mc.close();

                    mc.open();
                    String GetRunningDurationQuery = "SELECT StartTime from " + databaseName + ".[tbllivemode] where MachineID = " + machineId + " and IsDeleted = 0 and CorrectedDate = '" + correctedDate + "' and ColorCode = 'GREEN' and IsCompleted = 0;";
                    SqlDataAdapter GetRunningDurationDA = new SqlDataAdapter(GetRunningDurationQuery, mc.msqlConnection);
                    GetRunningDurationDA.Fill(RunningOP);
                    mc.close();

                    if (OP.Rows.Count != 0)
                    {
                        String Val = OP.Rows[0][0].ToString();
                        if (OP.Rows[0][0].ToString() != null && Val != "")
                        {
                            ModeDuration = Convert.ToInt32(OP.Rows[0][0]) / 60;

                        }

                    }
                    if (RunningOP.Rows.Count != 0)
                    {
                        DateTime StartTimeRunnning = Convert.ToDateTime(RunningOP.Rows[0][0]);
                        int DurationRunning = (int)DateTime.Now.Subtract(StartTimeRunnning).TotalSeconds / 60;
                        ModeDuration1 += DurationRunning;
                    }

                }

                //Quality
                ScrapQtyTime = QualityFactor_Piweb(machineId, UsedDateForExcel.ToString("yyyy-MM-dd"));
                if (ScrapQtyTime < 0)
                {
                    ScrapQtyTime = 0;
                }

                //Performance
                SummationOfSCTvsPP = GetSummationOfSCTvsPPToday(UsedDateForExcel.ToString("yyyy-MM-dd"), StartTime.ToString("yyyy-MM-dd HH:mm:ss"), CurrentTime.ToString("yyyy-MM-dd HH:mm:ss"), machineId);
                if (SummationOfSCTvsPP <= 0)
                {
                    SummationOfSCTvsPP = 0;
                }
                //ROPLosses = GetDownTimeLosses(UsedDateForExcel.ToString("yyyy-MM-dd"), MachineID, "ROP");


                ReWOTime = GetScrapQtyTimeOfRWOToday(UsedDateForExcel.ToString("yyyy-MM-dd"), StartTime.ToString("yyyy-MM-dd HH:mm:ss"), CurrentTime.ToString("yyyy-MM-dd HH:mm:ss"), machineId);
                if (ReWOTime < 0)
                {
                    ReWOTime = 0;
                }

                if (DateTime.Now.Hour > 6)
                {
                    AvailableTime = (DateTime.Now - Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 06:00:00"))).TotalMinutes;
                }
                else
                {
                    AvailableTime = (DateTime.Now - Convert.ToDateTime(DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd 06:00:00"))).TotalMinutes;
                }
                OperatingTime = ModeDuration + ModeDuration1;

                // OEE Calculations

                //Availability Factor
                double val = (OperatingTime + MinorLosses) / AvailableTime;
                AvailabilityPercentage = Math.Round(val * 100, 2);
                if (AvailabilityPercentage > 0 && AvailabilityPercentage < 100)
                {
                    AvailabilityPercentage = Math.Round(val * 100, 2);
                }
                else
                {
                    if (AvailabilityPercentage > 100)
                    {
                        AvailabilityPercentage = 100;
                    }
                    else if (AvailabilityPercentage < 0)
                    {
                        PerformancePercentage = 0;
                    }
                }

                //Performance(Efficiency) Factor
                if (OperatingTime == 0)
                {
                    PerformancePercentage = 0;
                }
                else
                {
                    PerformancePercentage = Math.Round((SummationOfSCTvsPP / (OperatingTime)) * 100, 2);
                    if (SummationOfSCTvsPP == -1 || SummationOfSCTvsPP == 0)
                    {
                        PerformancePercentage = 100;
                    }
                    else
                    {
                        PerformancePercentage = Math.Round((SummationOfSCTvsPP / (OperatingTime)) * 100, 2);
                        if (PerformancePercentage >= 0 && PerformancePercentage <= 100)
                        {
                        }
                        else if (PerformancePercentage > 100 || PerformancePercentage < 0)
                        {
                            PerformancePercentage = 100;
                        }
                    }
                }

                //QualityFactor
                if (OperatingTime == 0)
                {
                    QualityPercentage = 0;
                }
                else
                {
                    // QualityPercentage = (OperatingTime - ScrapQtyTime - ReWOTime) / OperatingTime;
                    QualityPercentage = ScrapQtyTime;
                    if (QualityPercentage >= 0 && QualityPercentage <= 100)
                    {
                        QualityPercentage = Math.Round(QualityPercentage);
                    }
                    else if (QualityPercentage > 100 || QualityPercentage < 0)
                    {
                        QualityPercentage = 100;
                    }
                }

                //OEE Factor
                if (AvailabilityPercentage <= 0 || PerformancePercentage <= 0 || QualityPercentage <= 0)
                {
                    OEEPercentage = 0;
                }
                else
                {
                    OEEPercentage = Math.Round((AvailabilityPercentage / 100) * (PerformancePercentage / 100) * (QualityPercentage / 100) * 100, 2);
                    if (OEEPercentage >= 0 && OEEPercentage <= 100)
                    {
                        OEEPercentage = Math.Round(OEEPercentage, 2);
                    }
                    else if (OEEPercentage > 100)
                    {
                        OEEPercentage = 100;
                    }
                    else if (OEEPercentage < 0)
                    {
                        OEEPercentage = 0;
                    }
                }

                //To get Top 5 Losses for this WC
                string todayAsCorrectedDate = UsedDateForExcel.ToString("yyyy-MM-dd");
                List<i_facilitylibrary.DAL.tbllossofentry> lossData = obj2.GettbllossofentryDet2(machineId, todayAsCorrectedDate);
                //var lossData = db.tbllossofentries.Where(m => m.CorrectedDate == todayAsCorrectedDate && m.MachineID == MachineID).ToList();

                DataTable DTLosses = new DataTable();
                DTLosses.Columns.Add("lossCodeID", typeof(int));
                DTLosses.Columns.Add("LossDuration", typeof(int));
                foreach (i_facilitylibrary.DAL.tbllossofentry row in lossData)
                {
                    int lossCodeID = Convert.ToInt32(row.MessageCodeID);
                    DateTime startDate = Convert.ToDateTime(row.StartDateTime);
                    DateTime endDate = Convert.ToDateTime(row.EndDateTime);
                    int duration = Convert.ToInt32(endDate.Subtract(startDate).TotalMinutes);

                    DataRow dr = DTLosses.Select("lossCodeID= '" + lossCodeID + "'").FirstOrDefault(); // finds all rows with id==2 and selects first or null if haven't found any
                    if (dr != null)
                    {
                        int LossDurationPrev = Convert.ToInt32(dr["LossDuration"]); //get lossduration and update it.
                        dr["LossDuration"] = (LossDurationPrev + duration);
                    }
                    //}
                    else
                    {
                        DTLosses.Rows.Add(lossCodeID, duration);
                    }
                }
                DataTable DTLossesTop5 = DTLosses.Clone();
                //get only the rows you want
                DataRow[] results = DTLosses.Select("", "LossDuration DESC");
                //populate new destination table
                if (DTLosses.Rows.Count > 0)
                {
                    int num = DTLosses.Rows.Count;
                    for (int iDT = 0; iDT < num; iDT++)
                    {
                        if (results[iDT] != null)
                        {
                            DTLossesTop5.ImportRow(results[iDT]);
                        }
                        else
                        {
                            DTLossesTop5.Rows.Add(0, 0);
                        }
                        if (iDT == 4)
                        {
                            break;
                        }
                    }
                    if (num < 5)
                    {
                        for (int iDT = num; iDT < 5; iDT++)
                        {
                            DTLossesTop5.Rows.Add(0, 0);
                        }
                    }
                }
                else
                {
                    for (int iDT = 0; iDT < 5; iDT++)
                    {
                        DTLossesTop5.Rows.Add(0, 0);
                    }
                }
                //Gather LossValues
                string lossCode1, lossCode2, lossCode3, lossCode4, lossCode5 = null;
                int lossCodeVal1, lossCodeVal2, lossCodeVal3, lossCodeVal4, lossCodeVal5 = 0;

                lossCode1 = Convert.ToString(DTLossesTop5.Rows[0][0]);
                lossCode2 = Convert.ToString(DTLossesTop5.Rows[1][0]);
                lossCode3 = Convert.ToString(DTLossesTop5.Rows[2][0]);
                lossCode4 = Convert.ToString(DTLossesTop5.Rows[3][0]);
                lossCode5 = Convert.ToString(DTLossesTop5.Rows[4][0]);
                lossCodeVal1 = Convert.ToInt32(DTLossesTop5.Rows[0][1]);
                lossCodeVal2 = Convert.ToInt32(DTLossesTop5.Rows[1][1]);
                lossCodeVal3 = Convert.ToInt32(DTLossesTop5.Rows[2][1]);
                lossCodeVal4 = Convert.ToInt32(DTLossesTop5.Rows[3][1]);
                lossCodeVal5 = Convert.ToInt32(DTLossesTop5.Rows[4][1]);
                string PlantIDS = null, ShopIDS = null, CellIDS = null;
                int value;
                i_facilitylibrary.DAL.tblmachinedetail WCData = obj1.GetmacDetails(machineId);
                //var WCData = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID).FirstOrDefault();
                string TempVal = WCData.PlantID.ToString();
                if (int.TryParse(TempVal, out value))
                {
                    PlantIDS = value.ToString();
                }

                TempVal = WCData.ShopID.ToString();
                if (int.TryParse(TempVal, out value))
                {
                    ShopIDS = value.ToString();
                }

                TempVal = WCData.CellID.ToString();
                if (int.TryParse(TempVal, out value))
                {
                    CellIDS = value.ToString();
                }

                //Now insert into table
                //using (MsqlConnection mcInsertRows = new MsqlConnection())
                //{
                //    try
                //    {
                //        mcInsertRows.open();
                //        SqlCommand cmdInsertRows = new SqlCommand("INSERT INTO tbloeedashboardvariablestoday(PlantID,ShopID,CellID,WCID,StartDate,EndDate,MinorLosses,Blue,Green,SettingTime,ROALossess,DownTimeBreakdown,SummationOfSCTvsPP,ScrapQtyTime,ReWOTime,Loss1Name,Loss1Value,Loss2Name,Loss2Value,Loss3Name,Loss3Value,Loss4Name,Loss4Value,Loss5Name,Loss5Value,CreatedOn,CreatedBy,IsDeleted,IPAddress)VALUES('" + PlantIDS + "','" + ShopIDS + "','" + CellIDS + "','" + MachineID + "','" + UsedDateForExcel.ToString("yyyy-MM-dd") + "','" + UsedDateForExcel.ToString("yyyy-MM-dd") + "','" + MinorLosses + "','" + blue + "','" + green + "','" + SettingTime + "','" + ROALossess + "','" + DownTimeBreakdown + "','" + SummationOfSCTvsPP + "','" + ScrapQtyTime + "','" + ReWOTime + "','" + lossCode1 + "','" + lossCodeVal1 + "','" + lossCode2 + "','" + lossCodeVal2 + "','" + lossCode3 + "','" + lossCodeVal3 + "','" + lossCode4 + "','" + lossCodeVal4 + "','" + lossCode5 + "','" + lossCodeVal5 + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + 1 + "','" + 0 + "','" + ipAddress + "' );", mcInsertRows.msqlConnection);
                //        cmdInsertRows.ExecuteNonQuery();
                //    }
                //    catch (Exception)
                //    {

                //    }
                //    finally
                //    {
                //        mcInsertRows.close();
                //    }
                //}
            }
            // UsedDateForExcel = UsedDateForExcel.AddDays(+1);
            #endregion

            //now push to tbloeedashboardFinalVariables.
        }

        //2017-04-05 //Get Minor Loss for Today OEE Calculation - Completed
        public int GetMinorLossesToday(string CorrectedDate, String StartTime, String EndTime, int MachineID, string Colour)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj2 = new Dao1(_conn); DateTime currentdate = Convert.ToDateTime(CorrectedDate);
            string datetime = currentdate.ToString("yyyy-MM-dd");
            int minorloss = 0;
            //int count = 0;
            DataTable GetMinorLossDT = new DataTable();
            using (MsqlConnection mc = new MsqlConnection())
            {
                //mc.open();
                //String GetMinorLoss = "SELECT ColorCode FROM tbllivedailyprodstatus WHERE MachineID = " + MachineID + " AND IsDeleted = 0 AND CorrectedDate = '" + CorrectedDate + "' AND StartTime BETWEEN '" + StartTime + "' AND '" + EndTime + "';";
                //SqlDataAdapter GetMinorLossDA = new SqlDataAdapter(GetMinorLoss, mc.msqlConnection);
                //GetMinorLossDA.Fill(GetMinorLossDT);
                //mc.close();
                mc.open();
                String GetMinorLossNew = "SELECT SUM(DurationInSec) from " + databaseName + ".[tbllivemode] where MachineID = " + MachineID + " and IsDeleted = 0 and CorrectedDate = '" + CorrectedDate + "' and ColorCode = '" + Colour + "' and DurationInSec < 120  and IsCompleted = 1;";
                SqlDataAdapter GetMinorLossNewDA = new SqlDataAdapter(GetMinorLossNew, mc.msqlConnection);
                GetMinorLossNewDA.Fill(GetMinorLossDT);
                mc.close();
            }
            int DataCount = GetMinorLossDT.Rows.Count;
            if (DataCount > 0)
            {
                String Val = GetMinorLossDT.Rows[0][0].ToString();
                if (GetMinorLossDT.Rows[0][0].ToString() != null && Val != "")
                {
                    minorloss = Convert.ToInt32(GetMinorLossDT.Rows[0][0]) / 60;
                }
            }

            ////var Data = db.tbldailyprodstatus.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID && m.CorrectedDate == CorrectedDate).OrderBy(m => m.StartTime).ToList();
            //for (int i = 0; i < DataCount; i++)
            //{
            //    //foreach (var row in Data)
            //    {
            //        if (GetMinorLossDT.Rows[i][0].ToString().ToUpper() == "YELLOW")
            //        {
            //            count++;
            //        }
            //        else
            //        {
            //            if (count > 0 && count < 2)
            //            {
            //                minorloss += count;
            //                count = 0;

            //            }
            //            count = 0;
            //        }
            //    }
            //}
            return minorloss;
        }

        //Get Idle/BD Loss for Today OEE Calculation - Completed
        //public int GetOPIDleBreakDownToday(string CorrectedDate, String StartTime, String EndTime, int MachineID, string Colour)
        //{
        //    int ModeDuration = 0;
        //    _conn = new ConnectionFactory();
        //    obj1 = new Dao(_conn);
        //    obj2 = new Dao1(_conn); DateTime currentdate = Convert.ToDateTime(CorrectedDate);
        //    string datetime = currentdate.ToString("yyyy-MM-dd");

        //    int[] count = new int[4];
        //    DataTable OP = new DataTable();
        //    DataTable RunningOP = new DataTable();
        //    using (MsqlConnection mc = new MsqlConnection())
        //    {
        //        ////operating
        //        //mc.open();
        //        //String query1 = "SELECT count(ID) From tbllivedailyprodstatus WHERE CorrectedDate='" + CorrectedDate + "' AND MachineID=" + MachineID + " AND ColorCode='" + Colour + "' AND StartTime BETWEEN '" + StartTime + "' AND '" + EndTime + "';";
        //        //SqlDataAdapter da1 = new SqlDataAdapter(query1, mc.msqlConnection);
        //        //da1.Fill(OP);
        //        //mc.close();

        //        mc.open();
        //        String GetDurationQuery = "SELECT SUM(DurationInSec) from " + databaseName + ".[tbllivemode] where MachineID = " + MachineID + " and IsDeleted = 0 and CorrectedDate = '" + CorrectedDate + "' and ColorCode = '" + Colour + "' and IsCompleted = 1;";
        //        SqlDataAdapter GetDurationDA = new SqlDataAdapter(GetDurationQuery, mc.msqlConnection);
        //        GetDurationDA.Fill(OP);
        //        mc.close();

        //        mc.open();
        //        String GetRunningDurationQuery = "SELECT StartTime from " + databaseName + ".[tbllivemode] where MachineID = " + MachineID + " and IsDeleted = 0 and CorrectedDate = '" + CorrectedDate + "' and ColorCode = '" + Colour + "' and IsCompleted = 0;";
        //        SqlDataAdapter GetRunningDurationDA = new SqlDataAdapter(GetRunningDurationQuery, mc.msqlConnection);
        //        GetRunningDurationDA.Fill(RunningOP);
        //        mc.close();
        //    }
        //    if (OP.Rows.Count != 0)
        //    {
        //        String Val = OP.Rows[0][0].ToString();
        //        if (OP.Rows[0][0].ToString() != null && Val != "")
        //        {
        //            ModeDuration = Convert.ToInt32(OP.Rows[0][0]) / 60;
        //        }
        //    }
        //    if (RunningOP.Rows.Count != 0)
        //    {
        //        DateTime StartTimeRunnning = Convert.ToDateTime(RunningOP.Rows[0][0]);
        //        int DurationRunning = (int)DateTime.Now.Subtract(StartTimeRunnning).TotalSeconds / 60;
        //        ModeDuration += DurationRunning;
        //    }
        //    return ModeDuration;
        //}
        ////Get Setting Time for Today OEE Calculation - Completed
        //public double GetSettingTimeToday(string UsedDateForExcel, String StartTime, String EndTime, int MachineID)
        //{
        //    _conn = new ConnectionFactory();
        //    obj1 = new Dao(_conn);
        //    obj2 = new Dao1(_conn);
        //    double settingTime = 0;
        //    int setupid = 0;
        //    string settingString = "Setup";
        //    i_facilitylibrary.DAL.tbllossescode setupiddata = obj2.GettbloeelossDet3(settingString);
        //    //var setupiddata = db.tbllossescodes.Where(m => m.MessageType.Contains(settingString)).FirstOrDefault();
        //    if (setupiddata != null)
        //    {
        //        setupid = setupiddata.LossCodeID;
        //    }
        //    else
        //    {
        //        Session["Error"] = "Unable to get Setup's ID";
        //        return -1;
        //    }
        //    //getting all setup's sublevels ids.
        //    List<string> SettingIDs = obj2.GettbllossescodeDet1(setupid);
        //    //var SettingIDs = dbloss.tbllossescodes.Where(m => (m.LossCodesLevel1ID == setupid || m.LossCodesLevel2ID == setupid)).Select(m => m.LossCodeID).ToList();

        //    //settingTime = (from row in db.tbllossofentries
        //    //               where  row.CorrectedDate == UsedDateForExcel && row.MachineID == MachineID );
        //    foreach (string Setting in SettingIDs)
        //    {
        //        DataTable GetSettingTimeDT = new DataTable();
        //        using (MsqlConnection mc = new MsqlConnection())
        //        {
        //            mc.open();
        //            String GetSettingTime = "SELECT * FROM " + databaseName + ".tbllivelossofentry WHERE MachineID = " + MachineID + " AND MessageCodeID = " + Setting + " AND CorrectedDate = '" + UsedDateForExcel + "' AND DoneWithRow = 1 AND StartDateTime BETWEEN '" + StartTime + "' AND '" + EndTime + "';";
        //            SqlDataAdapter GetSettingTimeDA = new SqlDataAdapter(GetSettingTime, mc.msqlConnection);
        //            GetSettingTimeDA.Fill(GetSettingTimeDT);
        //            mc.close();
        //        }
        //        int DataCount = GetSettingTimeDT.Rows.Count;
        //        //var SettingData = db.tbllossofentries.Where(m => SettingIDs.Contains(m.MessageCodeID) && m.MachineID == MachineID && m.CorrectedDate == UsedDateForExcel && m.DoneWithRow == 1).ToList();
        //        for (int i = 0; i < DataCount; i++)
        //        {
        //            DateTime startTime = Convert.ToDateTime(GetSettingTimeDT.Rows[i][2].ToString());
        //            DateTime endTime = Convert.ToDateTime(GetSettingTimeDT.Rows[i][3].ToString());
        //            settingTime += endTime.Subtract(startTime).TotalMinutes;
        //        }
        //    }
        //    return settingTime;
        //}
        ////Get Downtime Loss for Today OEE Calculation - Completed
        //public double GetDownTimeLossesToday(string UsedDateForExcel, String StartTime, String EndTime, int MachineID, string contribute)
        //{
        //    _conn = new ConnectionFactory();
        //    obj1 = new Dao(_conn);
        //    obj2 = new Dao1(_conn); double LossTime = 0;
        //    //string contribute = "ROA";
        //    //getting all ROA sublevels ids. Only those of IDLE.
        //    List<string> SettingIDs = obj2.GettbllossescodeDet2(contribute);
        //    // var SettingIDs = db.tbllossescodes.Where(m => m.ContributeTo == contribute && (m.MessageType != "PM" || m.MessageType != "BREAKDOWN" || m.MessageType != "Setup")).Select(m => m.LossCodeID).ToList();
        //    DataTable GetSettingTimeDT = new DataTable();
        //    foreach (string Setting in SettingIDs)
        //    {
        //        GetSettingTimeDT.Clear();
        //        using (MsqlConnection mc = new MsqlConnection())
        //        {
        //            mc.open();
        //            String GetSettingTime = "SELECT * FROM " + databaseName + ".tbllivelossofentry WHERE MachineID = " + MachineID + " AND MessageCodeID = " + Setting + " AND CorrectedDate = '" + UsedDateForExcel + "' AND DoneWithRow = 1 AND StartDateTime BETWEEN '" + StartTime + "' AND '" + EndTime + "';";
        //            SqlDataAdapter GetSettingTimeDA = new SqlDataAdapter(GetSettingTime, mc.msqlConnection);
        //            GetSettingTimeDA.Fill(GetSettingTimeDT);
        //            mc.close();
        //        }
        //        int DataCount = GetSettingTimeDT.Rows.Count;
        //        for (int i = 0; i < DataCount; i++)
        //        {
        //            //var SettingData = db.tbllossofentries.Where(m => SettingIDs.Contains(m.MessageCodeID) && m.MachineID == MachineID && m.CorrectedDate == UsedDateForExcel && m.DoneWithRow == 1).ToList();
        //            //foreach (var row in SettingData)
        //            {
        //                DateTime startTime = Convert.ToDateTime(GetSettingTimeDT.Rows[i][2].ToString());
        //                DateTime endTime = Convert.ToDateTime(GetSettingTimeDT.Rows[i][3].ToString());
        //                LossTime += endTime.Subtract(startTime).TotalMinutes;
        //            }
        //        }
        //    }
        //    return LossTime;
        //}
        ////Get BreakdownLoss for Today OEE Calculation - Completed
        //public double GetDownTimeBreakdownToday(string UsedDateForExcel, String StartTime, String EndTime, int MachineID)
        //{
        //    _conn = new ConnectionFactory();
        //    obj1 = new Dao(_conn);
        //    obj2 = new Dao1(_conn); double LossTime = 0;
        //    //var BreakdownData = db.tblbreakdowns.Where(m => m.MachineID == MachineID && m.CorrectedDate == UsedDateForExcel && m.DoneWithRow == 1).ToList();
        //    DataTable GetSettingTimeDT = new DataTable();
        //    using (MsqlConnection mc = new MsqlConnection())
        //    {
        //        mc.open();
        //        String GetSettingTime = "SELECT * FROM " + databaseName + ".tblbreakdown WHERE MachineID = " + MachineID + " AND CorrectedDate = '" + UsedDateForExcel + "' AND DoneWithRow = 1 AND StartTime BETWEEN '" + StartTime + "' AND '" + EndTime + "';";
        //        SqlDataAdapter GetSettingTimeDA = new SqlDataAdapter(GetSettingTime, mc.msqlConnection);
        //        GetSettingTimeDA.Fill(GetSettingTimeDT);
        //        mc.close();
        //    }
        //    int DataCount = GetSettingTimeDT.Rows.Count;
        //    for (int i = 0; i < DataCount; i++)
        //    {
        //        {
        //            if ((Convert.ToString(GetSettingTimeDT.Rows[i][2]) == null) || GetSettingTimeDT.Rows[i][2].ToString() == null)
        //            {
        //                //do nothing
        //            }
        //            else
        //            {
        //                DateTime startTime = Convert.ToDateTime(GetSettingTimeDT.Rows[i][1].ToString());
        //                DateTime endTime = Convert.ToDateTime(GetSettingTimeDT.Rows[i][2].ToString());
        //                LossTime += endTime.Subtract(startTime).TotalMinutes;
        //            }
        //        }
        //    }
        //    return LossTime;
        //}
        //Get SCTVsPP for Today OEE Calculation - Completed
        public double GetSummationOfSCTvsPPToday(string UsedDateForExcel, String StartTime, String EndTime, int MachineID)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj2 = new Dao1(_conn); double SummationofTime = 0;
            DataTable GetSettingTimeDT = new DataTable();
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                String GetSettingTime = "SELECT * FROM " + databaseName + ".tbllivehmiscreen WHERE MachineID = " + MachineID + " AND CorrectedDate = '" + UsedDateForExcel + "' AND isWorkOrder = 0 AND (isWorkInProgress = 1 OR isWorkInProgress = 0);";
                SqlDataAdapter GetSettingTimeDA = new SqlDataAdapter(GetSettingTime, mc.msqlConnection);
                GetSettingTimeDA.Fill(GetSettingTimeDT);
                mc.close();
            }
            int DataCount = GetSettingTimeDT.Rows.Count;
            //var PartsData = db.tblhmiscreens.Where(m => m.CorrectedDate == UsedDateForExcel && m.MachineID == MachineID && m.isWorkOrder == 0 && (m.isWorkInProgress == 1 || m.isWorkInProgress == 0)).ToList();
            if (DataCount == 0)
            {
                return -1;
            }
            for (int i = 0; i < DataCount; i++)
            //foreach (var row in PartsData)
            {
                string partno = GetSettingTimeDT.Rows[i][7].ToString();
                string operationno = GetSettingTimeDT.Rows[i][8].ToString();
                int rejectedQty = 0, deliveredQty = 0;
                string deliveredQtyString = GetSettingTimeDT.Rows[i][12].ToString();
                // string rejectedQtyString = GetSettingTimeDT.Rows[i][9].ToString();
                string rejectedQtyString = QualityFactor_PiwebReject(MachineID, UsedDateForExcel).ToString();
                rejectedQty = rejectedQtyString != "" ? Convert.ToInt32(rejectedQtyString) : 0;
                deliveredQty = deliveredQtyString != "" ? Convert.ToInt32(deliveredQtyString) : 0;
                int totalpartproduced = deliveredQty + rejectedQty;
                double stdCuttingTime = 0;
                i_facilitylibrary.DAL.tblmasterparts_st_sw stdcuttingTimeData = obj2.Gettblmasterparts_st_swDet3(operationno, partno);
                //var stdcuttingTimeData = db.tblmasterparts_st_sw.Where(m => m.IsDeleted == 0 && m.OpNo == operationno && m.PartNo == partno).FirstOrDefault();
                //foreach (var row1 in stdcuttingTimeData)
                if (stdcuttingTimeData != null)
                {
                    string stdcuttingvalString = Convert.ToString(stdcuttingTimeData.StdCuttingTime);
                    Double stdcuttingval = 0;
                    if (double.TryParse(stdcuttingvalString, out stdcuttingval))
                    {
                        stdcuttingval = stdcuttingval;
                    }
                    string Unit = Convert.ToString(stdcuttingTimeData.StdCuttingTimeUnit);
                    if (Unit == "Hrs")
                    {
                        stdCuttingTime = stdcuttingval * 60;
                    }
                    else //Unit is Minutes
                    {
                        stdCuttingTime = stdcuttingval;
                    }
                }
                SummationofTime += stdCuttingTime * totalpartproduced;
            }
            return SummationofTime;
        }
        ////Get Scrap Qty Operating Time for Today OEE Calculation - Completed
        //public double GetScrapQtyTimeOfWOToday(string UsedDateForExcel, String StartTime, String EndTime, int MachineID)
        //{
        //    _conn = new ConnectionFactory();
        //    obj1 = new Dao(_conn);
        //    obj2 = new Dao1(_conn); double SQT = 0;
        //    DataTable GetSettingTimeDT = new DataTable();
        //    using (MsqlConnection mc = new MsqlConnection())
        //    {
        //        mc.open();
        //        String GetSettingTime = "SELECT * FROM " + databaseName + ".tbllivehmiscreen WHERE MachineID = " + MachineID + " AND CorrectedDate = '" + UsedDateForExcel + "' AND isWorkOrder = 0 AND (isWorkInProgress = 1 OR isWorkInProgress = 0) ;";
        //        SqlDataAdapter GetSettingTimeDA = new SqlDataAdapter(GetSettingTime, mc.msqlConnection);
        //        GetSettingTimeDA.Fill(GetSettingTimeDT);
        //        mc.close();
        //    }
        //    int DataCount = GetSettingTimeDT.Rows.Count;

        //    //var PartsData = db.tblhmiscreens.Where(m => m.CorrectedDate == UsedDateForExcel && m.MachineID == MachineID && (m.isWorkInProgress == 1 || m.isWorkInProgress == 0) && m.isWorkOrder == 0).ToList();
        //    for (int i = 0; i < DataCount; i++)
        //    //foreach (var row in PartsData)
        //    {
        //        string partno = GetSettingTimeDT.Rows[i][7].ToString();
        //        string operationno = GetSettingTimeDT.Rows[i][8].ToString();
        //        //int scrapQty = Convert.ToInt32(GetSettingTimeDT.Rows[i][9].ToString());
        //        //int DeliveredQty = Convert.ToInt32(GetSettingTimeDT.Rows[i][12].ToString());

        //        int scrapQty = 0;
        //        int DeliveredQty = 0;
        //        string scrapQtyString = Convert.ToString(GetSettingTimeDT.Rows[i][9]);
        //        string DeliveredQtyString = Convert.ToString(GetSettingTimeDT.Rows[i][12]);
        //        string x = scrapQtyString;
        //        int value;
        //        if (int.TryParse(x, out value))
        //        {
        //            scrapQty = value;
        //        }
        //        x = DeliveredQtyString;
        //        if (int.TryParse(x, out value))
        //        {
        //            DeliveredQty = value;
        //        }

        //        if (scrapQty != 0)
        //        {
        //            DateTime startTime = Convert.ToDateTime(GetSettingTimeDT.Rows[i][4].ToString());
        //            DateTime endTime = Convert.ToDateTime(GetSettingTimeDT.Rows[i][5].ToString());
        //            //Double WODuration = endTimeTemp.Subtract(startTime).TotalMinutes;
        //            Double WODuration = GetGreenToday(UsedDateForExcel, startTime, endTime, MachineID);

        //            if ((scrapQty + DeliveredQty) == 0)
        //            {
        //                SQT += 0;
        //            }
        //            else
        //            {
        //                SQT += (WODuration / (scrapQty + DeliveredQty)) * scrapQty;
        //            }
        //        }
        //        else
        //        {
        //            //do nothing
        //        }
        //    }
        //    return SQT;
        //}
        //Get ReWork Order Time for Today OEE Calculation - Completed

        /*GOD*/
        public double GetScrapQtyTimeOfRWOToday(string UsedDateForExcel, String StartTime, String EndTime, int MachineID)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj2 = new Dao1(_conn); double SQT = 0;
            DataTable GetSettingTimeDT = new DataTable();
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                String GetSettingTime = "SELECT * FROM " + databaseName + ".tbllivehmiscreen WHERE MachineID = " + MachineID + " AND CorrectedDate = '" + UsedDateForExcel + "' AND isWorkOrder = 1 AND (isWorkInProgress = 1 OR isWorkInProgress = 0) ;";
                SqlDataAdapter GetSettingTimeDA = new SqlDataAdapter(GetSettingTime, mc.msqlConnection);
                GetSettingTimeDA.Fill(GetSettingTimeDT);
                mc.close();
            }
            int DataCount = GetSettingTimeDT.Rows.Count;
            //var PartsData = db.tblhmiscreens.Where(m => m.CorrectedDate == UsedDateForExcel && m.MachineID == MachineID && (m.isWorkInProgress == 1 || m.isWorkInProgress == 0) && m.isWorkOrder == 1).ToList();
            for (int i = 0; i < DataCount; i++)
            //foreach (var row in PartsData)
            {
                string partno = GetSettingTimeDT.Rows[i][7].ToString();
                string operationno = GetSettingTimeDT.Rows[i][8].ToString();
                //int scrapQty = Convert.ToInt32(GetSettingTimeDT.Rows[i][9].ToString());
                //int DeliveredQty = Convert.ToInt32(GetSettingTimeDT.Rows[i][12].ToString());

                int scrapQty = 0;
                int DeliveredQty = 0;
                string scrapQtyString = Convert.ToString(GetSettingTimeDT.Rows[i][9]);
                string DeliveredQtyString = Convert.ToString(GetSettingTimeDT.Rows[i][12]);
                string x = scrapQtyString;
                int value;
                if (int.TryParse(x, out value))
                {
                    scrapQty = value;
                }
                x = DeliveredQtyString;
                if (int.TryParse(x, out value))
                {
                    DeliveredQty = value;
                }

                DateTime startTime = Convert.ToDateTime(GetSettingTimeDT.Rows[i][4].ToString());
                DateTime endTime = Convert.ToDateTime(GetSettingTimeDT.Rows[i][5].ToString());
                Double WODuration = GetGreenToday(UsedDateForExcel, startTime, endTime, MachineID);

                //Double WODuration = endTime.Subtract(startTime).TotalMinutes;
                //For Availability Loss
                //double Settingtime = GetSetupForReworkLoss(UsedDateForExcel, startTime, endTime, MachineID);
                //double green = GetOT(UsedDateForExcel, startTime, endTime, MachineID);
                //double DownTime = GetDownTimeForReworkLoss(UsedDateForExcel, startTime, endTime, MachineID, "ROA");
                //double BreakdownTime = GetBreakDownTimeForReworkLoss(UsedDateForExcel, startTime, endTime, MachineID);
                //double AL = DownTime + BreakdownTime + Settingtime;

                //For Performance Loss
                //double downtimeROP = GetDownTimeForReworkLoss(UsedDateForExcel, startTime, endTime, MachineID, "ROP");
                //double minorlossWO = GetMinorLossForReworkLoss(UsedDateForExcel, startTime, endTime, MachineID, "yellow");
                //double PL = downtimeROP + minorlossWO;

                SQT += WODuration;
            }
            return SQT;
        }
        public double GetGreenToday(string UsedDateForExcel, DateTime TSstartTime, DateTime TSendTime, int MachineID)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj2 = new Dao1(_conn);
            double settingTime = 0;
            DateTime WOstarttimeDate = Convert.ToDateTime(TSstartTime);
            DateTime WOendtimeDate = Convert.ToDateTime(TSendTime);

            DataTable lossesData = new DataTable();

            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                String query1 = "SELECT Count(ID) From " + databaseName + ".tbllivedailyprodstatus WHERE MachineID = '" + MachineID + "' and CorrectedDate = '" + UsedDateForExcel + "' and ColorCode = 'green'"
                    + " and ( StartTime >= '" + WOstarttimeDate + "' and EndTime <= '" + WOendtimeDate + "' )";
                SqlDataAdapter da1 = new SqlDataAdapter(query1, mc.msqlConnection);
                da1.Fill(lossesData);
                mc.close();
            }
            if (lossesData.Rows.Count > 0)
            {
                settingTime = Convert.ToDouble(lossesData.Rows[0][0]);
            }
            return settingTime;
        }

        #endregion

        private int GetQuantiy(int machineId, DateTime CorrectedDate, out int YieldQty, out int BottleNeckYieldQty, int bottlneckMachineID/*, out int BottleNeckTotalQty*/)
        {
            int TotalQty = 0;
            List<tblmachinedetail> machineDet = new List<tblmachinedetail>();
            tbldaytiming starttime = new tbldaytiming();
            List<parameters_master> parametermasterlistAll = new List<parameters_master>();
            List<parameters_master> parametermasterlist = new List<parameters_master>();
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                machineDet = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.MachineID == machineId).ToList();
            }
            YieldQty = 0;
            //BottleNeckTotalQty = 0;
            BottleNeckYieldQty = 0;
            string Correcteddate = CorrectedDate.ToString("yyyy-MM-dd");
            string NxtCorrecteddate = CorrectedDate.AddDays(1).ToString("yyyy-MM-dd");
            tblmachinedetail bottleneckmachine = machineDet.Where(m => m.IsBottelNeck == 1).OrderBy(m => m.MachineID).FirstOrDefault();
            tblmachinedetail lastmachine = machineDet.Where(m => m.IsLastMachine == 1).OrderBy(m => m.MachineID).FirstOrDefault();
            tblmachinedetail firtstmachine = machineDet.Where(m => m.IsFirstMachine == 1).OrderBy(m => m.MachineID).FirstOrDefault();
            int firstmachineId = 0;
            int lstmachineId = 0;
            int bottleneckMachineID = 0;
            if (firtstmachine != null)
            {
                firstmachineId = firtstmachine.MachineID;
            }

            if (lastmachine != null)
            {
                lstmachineId = lastmachine.MachineID;
            }

            if (bottleneckmachine != null)
            {
                bottleneckMachineID = bottleneckmachine.MachineID;
            }

            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                starttime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault(); //.Select(m => m.StartTime)
            }

            string StartTime = Correcteddate + " 06:00:00";
            string EndTime = NxtCorrecteddate + " 06:00:00";

            DateTime St = Convert.ToDateTime(StartTime);
            DateTime Et = Convert.ToDateTime(EndTime);

            // Based on 1st Machine
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                parametermasterlistAll = db.parameters_master.Where(m => m.CorrectedDate == CorrectedDate.Date && m.InsertedOn >= St && m.InsertedOn <= Et).ToList();
            }
            parametermasterlist = parametermasterlistAll.Where(m => m.MachineID == firstmachineId && m.CorrectedDate == CorrectedDate.Date && m.InsertedOn >= St && m.InsertedOn <= Et).ToList();
            parameters_master TopRow = parametermasterlist.OrderByDescending(m => m.ParameterID).FirstOrDefault();
            parameters_master LastRow = parametermasterlist.OrderBy(m => m.ParameterID).FirstOrDefault();


            // Based on Last Machine
            List<parameters_master> parametermasterlistLast = parametermasterlistAll.Where(m => m.MachineID == lstmachineId && m.CorrectedDate == CorrectedDate.Date && m.InsertedOn >= St && m.InsertedOn <= Et).ToList();
            parameters_master TopRowLast = parametermasterlistLast.OrderByDescending(m => m.ParameterID).FirstOrDefault();
            parameters_master RowLast = parametermasterlistLast.OrderBy(m => m.ParameterID).FirstOrDefault();

            // Based on Last Machine
            List<parameters_master> parametermasterlistBottleNeck = parametermasterlistAll.Where(m => m.MachineID == bottlneckMachineID && m.CorrectedDate == CorrectedDate.Date && m.InsertedOn >= St && m.InsertedOn <= Et).ToList();
            parameters_master TopRowBottleNeck = parametermasterlistBottleNeck.OrderByDescending(m => m.ParameterID).FirstOrDefault();
            parameters_master RowLastBottleNeck = parametermasterlistBottleNeck.OrderBy(m => m.ParameterID).FirstOrDefault();


            if (TopRowLast != null && RowLast != null)
            {
                YieldQty = Convert.ToInt32(TopRowLast.PartsTotal - RowLast.PartsTotal);
            }

            if (TopRow != null && LastRow != null)
            {
                TotalQty = Convert.ToInt32(TopRow.PartsTotal - LastRow.PartsTotal);
            }

            if (TopRowBottleNeck != null && RowLastBottleNeck != null)
            {
                BottleNeckYieldQty = Convert.ToInt32(TopRowBottleNeck.PartsTotal - RowLastBottleNeck.PartsTotal);
            }
            //}
            return TotalQty;

        }

        private int GetParts_Cutting(int MachineID, DateTime CorrectedDate, out int TotalPartsCount)
        {
            int CuttingTime = 0;
            TotalPartsCount = 0;
            string Correcteddate = CorrectedDate.ToString("yyyy-MM-dd");
            string NxtCorrecteddate = CorrectedDate.AddDays(1).ToString("yyyy-MM-dd");
            string StartTime = Correcteddate + " 07:15:00";
            string EndTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            DateTime St = Convert.ToDateTime(StartTime);
            DateTime Et = Convert.ToDateTime(EndTime);
            List<parameters_master> parametermasterlist = new List<parameters_master>();
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                parametermasterlist = db.parameters_master.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate.Date && m.InsertedOn >= St && m.InsertedOn <= Et).ToList();
            }
            parameters_master TopRow = parametermasterlist.OrderByDescending(m => m.ParameterID).FirstOrDefault();
            parameters_master LastRow = parametermasterlist.OrderBy(m => m.ParameterID).FirstOrDefault();
            if (TopRow != null && LastRow != null)
            {
                CuttingTime = Convert.ToInt32(TopRow.CuttingTime) - Convert.ToInt32(LastRow.CuttingTime);
                TotalPartsCount = Convert.ToInt32(TopRow.PartsTotal - LastRow.PartsTotal);
            }
            return CuttingTime;
        }

        public List<AlarmList> GetAlarms()
        {
            string res = "";
            List<AlarmList> AlarmList = new List<AlarmList>();
            //using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            //{
            //string correctedDate = "2018-08-23";
            string correctedDate = GetCorrectedDate();

            string correctdate = correctedDate;
            DateTime CorrectedDate = Convert.ToDateTime(correctedDate);
            List<tblmachinedetail> machdet = new List<tblmachinedetail>();
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                machdet = db.tblmachinedetails.Where(m => m.IsDeleted == 0).OrderBy(m => m.MachineID).ToList();
            }
            foreach (tblmachinedetail row in machdet)
            {
                List<tblmachinedetail> machineslist = new List<tblmachinedetail>();
                List<alarm_history_master> alaramhistory = new List<alarm_history_master>();
                //using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
                //{
                //    //machineslist = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.CellID == row.CellID).ToList();
                //}
                //foreach (var machine in machineslist)
                //{
                int machineId = row.MachineID;
                string machineName = row.MachineDisplayName;
                List<tblalarmdetail> alarmsdetails = db.tblalarmdetails.Where(a => a.IsDeleted == 0).OrderBy(m => m.AlarmDesc).ToList();
                String MainCorrectedDate = CorrectedDate.ToString("yyyy-MM-dd");
                using (i_facility_shaktiEntities1 db1 = new i_facility_shaktiEntities1())
                {
                    //var alaramhistory = obj2.GetAlaramDetails(machineId, MainCorrectedDate);
                    alaramhistory = db1.alarm_history_master.Where(m => m.CorrectedDate == MainCorrectedDate && m.MachineID == machineId).Distinct().OrderBy(m => m.CorrectedDate).ToList();
                    //alaramhistory = "SELECT * From " + databaseName + ".alarm_history_master WHERE CorrectedDate = '" + MainCorrectedDate + "' AND MachineID = " + machineId + " AND AlarmMessage = '" + AlarmMessage + "' and AlarmNo = '" + AlarmNo + "' and Axis_No = '" + Axis_No + "' ";
                    //alarmsdetails = db1.tblalarmdetails.Where(m => m.IsDeleted == 0).ToList();

                    List<string> Alaramnumberlist = alarmsdetails.Select(m => m.AlarmDesc).ToList();
                    //IntoFile("Alaramnumberlist count" + Alaramnumberlist.Count);
                    List<alarm_history_master> alaramdet = alaramhistory.Where(m => Alaramnumberlist.Contains(m.AlarmMessage.ToString().Trim())).ToList();
                    //var MainAlarmList = (from dr in alaramdet
                    //                     orderby dr.AlarmDateTime descending
                    //                     select dr.AlarmMessage).Distinct();

                    //IntoFile("Alarm count" + alaramdet.Count);


                    //foreach (tblalarmdetail alarm in alarmsdetails)
                    //{
                    //    using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
                    //    {
                    //        String MainCorrectedDate = CorrectedDate.ToString("yyyy-MM-dd");
                    //        string alarmNo = @alarm.AlarmDesc.ToString();
                    //        alaramhistory = db.alarm_history_master.Where(m => m.AlarmMessage == @alarmNo && m.MachineID == machineId && m.CorrectedDate == MainCorrectedDate).ToList();
                    //    }
                    foreach (alarm_history_master alh in alaramdet)
                    {
                        AlarmList al = new AlarmList();
                        string MachineDispName = db.tblmachinedetails.Where(m => m.MachineID == alh.MachineID).Select(m => m.MachineName).FirstOrDefault();
                        al.MachineID = MachineDispName;
                        //al.MachineName = dt.Rows[p][machineName].ToString();
                        al.AlarmNumber = alh.AlarmNo;
                        al.AlarmMessage = alh.AlarmMessage.ToString();
                        al.AxisNumber = alh.Axis_Num;
                        al.AlarmDateTime = Convert.ToDateTime(alh.AlarmDateTime);
                        AlarmList.Add(al);
                    }
                    //}
                }
                res = JsonConvert.SerializeObject(AlarmList);
            }
            //res = JsonConvert.SerializeObject(AlarmList);
            //}
            return AlarmList;
        }

        public string GetAlarmsById(int cellId)
        {
            string res = "";
            List<AlarmList> AlarmList = new List<AlarmList>();
            string correctedDate = GetCorrectedDate();
            string correctdate = correctedDate;
            DateTime CorrectedDate = Convert.ToDateTime(correctedDate);
            //var machdet = new List<tblmachinedetail>();
            //using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            //{
            tblmachinedetail machdet = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == cellId).OrderBy(m => m.MachineID).FirstOrDefault();
            //}
            //foreach (var row in machdet)
            //{
            List<tblmachinedetail> machineslist = new List<tblmachinedetail>();
            List<alarm_history_master> alaramhistory = new List<alarm_history_master>();

            int machineId = cellId;
            string machineName = machdet.MachineDisplayName;
            List<tblpriorityalarm> alarmsdetails = db.tblpriorityalarms.Where(a => a.IsDeleted == 0).OrderBy(m => m.AlarmID).ToList();
            foreach (tblpriorityalarm alarm in alarmsdetails)
            {
                using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
                {
                    string alarmNo = alarm.AlarmNumber.ToString();
                    alaramhistory = db.alarm_history_master.Where(m => m.AlarmNo == alarmNo && m.MachineID == machineId && m.CorrectedDate == CorrectedDate.ToString("yyyy-MM-dd")).OrderByDescending(m => m.AlarmNo).ToList();
                }
                foreach (alarm_history_master alh in alaramhistory)
                {
                    string MachineDispName = db.tblmachinedetails.Where(m => m.MachineID == alh.MachineID).Select(m => m.MachineName).FirstOrDefault();

                    AlarmList al = new AlarmList();
                    al.MachineID = MachineDispName;
                    al.AlarmNumber = alh.AlarmNo.ToString();
                    al.AlarmMessage = alh.AlarmMessage.ToString();
                    al.AxisNumber = alh.Axis_Num.ToString();
                    al.AlarmDateTime = Convert.ToDateTime(alh.AlarmDateTime);
                    AlarmList.Add(al);
                }
            }

            //}
            res = JsonConvert.SerializeObject(AlarmList);
            return res;
        }

        //public string TargetAcualsDet(int cellid)
        //{
        //    string res = "";
        //    res = GetTarget_Actual(cellid);
        //    return res;
        //}

        //public string GetTarget_Actual(int cellid)
        //{
        //    string res = "";
        //    //using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
        //    //{
        //    string[] backgroundcolr;
        //    string[] borderColor;

        //    string correctedDate = GetCorrectedDate();

        //    string correctdate = correctedDate;
        //    DateTime CorrectedDate = Convert.ToDateTime(correctedDate);
        //    List<TargetActualListDet> TargetList = new List<TargetActualListDet>();
        //    List<TargetActualList> TargetActualList = new List<TargetActualList>();
        //    tblmachinedetail machdet = new tblmachinedetail();
        //    tblmachinedetail machineDet = new tblmachinedetail();
        //    List<tblpartscountandcutting> partDetails = new List<tblpartscountandcutting>();
        //    using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
        //    {
        //        machdet = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == cellid).OrderBy(m => m.MachineID).FirstOrDefault();
        //    }
        //    int count = 0;
        //    if (machdet != null)
        //    {
        //        count = count + 1;
        //        backgroundcolr = new string[] { "rgba(254, 99, 131, 1)", "rgba(54, 162, 235, 1)", "rgba(255, 206, 87, 1)", "rgba(75, 192, 192, 1)" };
        //        borderColor = new string[] { "rgba(254, 99, 131, 1)", "rgba(75, 192, 192, 1)", "rgba(75, 192, 192, 1)", "rgba(75, 192, 192, 1)" };

        //        if (count == 1)
        //        {
        //            backgroundcolr = new string[] { "rgba(254, 99, 131, 1)", "rgba(254, 99, 131, 1)", "rgba(254, 99, 131, 1)", "rgba(254, 99, 131, 1)" };
        //            borderColor = new string[] { "rgba(254, 99, 131, 1)", "rgba(254, 99, 131, 1)", "rgba(254, 99, 131, 1)", "rgba(254, 99, 131, 1)" };

        //        }
        //        else if (count > 1)
        //        {
        //            backgroundcolr = new string[] { "rgba(54, 162, 235, 1)", "rgba(54, 162, 235, 1)", "rgba(54, 162, 235, 1)", "rgba(54, 162, 235, 1)" };
        //            borderColor = new string[] { "rgba(75, 192, 192, 1)", "rgba(75, 192, 192, 1)", "rgba(75, 192, 192, 1)", "rgba(75, 192, 192, 1)" };
        //        }

        //        TargetActualListDet TAL = new TargetActualListDet();
        //        TAL.CellName = machineDet.MachineDisplayName;

        //        string StartTime = CorrectedDate.ToString("yyyy-MM-dd") + " 07:00:00";
        //        string EndTime = CorrectedDate.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss");
        //        DateTime St = Convert.ToDateTime(StartTime);
        //        DateTime Et = Convert.ToDateTime(EndTime);
        //        using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
        //        {
        //            machineDet = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.MachineID == machineDet.MachineID && m.IsBottelNeck == 1).FirstOrDefault();
        //        }
        //        if (machineDet != null)
        //        {
        //            int MachineID = machineDet.MachineID;
        //            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
        //            {
        //                partDetails = db.tblpartscountandcuttings.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate.Date && m.Isdeleted == 0 && m.StartTime >= St && m.EndTime <= Et).OrderBy(m => m.StartTime).ToList(); //.Select(m => new { m.PartCount, m.TargetQuantity, m.StartTime, m.EndTime })
        //            }
        //            int[] data = new int[partDetails.Count];
        //            List<int> Target = new List<int>();
        //            List<int> Actual = new List<int>();
        //            string[] Lables = new string[partDetails.Count];
        //            for (int i = 0; i < partDetails.Count; i++)
        //            {
        //                Target.Add(partDetails[i].TargetQuantity);
        //                Actual.Add(partDetails[i].PartCount);
        //                Lables[i] = partDetails[i].StartTime.ToString("HH:mm") + " - " + partDetails[i].EndTime.ToString("HH:mm");
        //            }
        //            TAL.backgroundColor = backgroundcolr;
        //            TAL.borderColor = borderColor;
        //            TAL.Timings = Lables;
        //            TAL.Target = Target;
        //            TAL.Actual = Actual;
        //            TargetList.Add(TAL);
        //        }
        //    }
        //    res = JsonConvert.SerializeObject(TargetList);
        //    //}
        //    return res;
        //}

        //public string GetTarget_Actual_Line(int cellid)
        //{
        //    string res = "";
        //    //using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
        //    //{
        //    string[] backgroundcolr;
        //    string[] borderColor;

        //    string correctedDate = GetCorrectedDate();

        //    string correctdate = correctedDate;
        //    DateTime CorrectedDate = Convert.ToDateTime(correctedDate);
        //    List<TargetActualListDet> TargetList = new List<TargetActualListDet>();
        //    List<TargetActualList> TargetActualList = new List<TargetActualList>();
        //    tblmachinedetail machdet = new tblmachinedetail();
        //    tblmachinedetail machineDet = new tblmachinedetail();
        //    List<tblpartscountandcutting> partDetails = new List<tblpartscountandcutting>();
        //    List<ViewData> finalData = new List<ViewData>();
        //    using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
        //    {
        //        machdet = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == cellid).OrderBy(m => m.MachineID).FirstOrDefault();
        //    }
        //    int count = 0;
        //    if (machdet != null)
        //    {
        //        count = count + 1;
        //        backgroundcolr = new string[] { "rgba(254, 99, 131, 1)", "rgba(54, 162, 235, 1)", "rgba(255, 206, 87, 1)", "rgba(75, 192, 192, 1)" };
        //        borderColor = new string[] { "rgba(254, 99, 131, 1)", "rgba(75, 192, 192, 1)", "rgba(75, 192, 192, 1)", "rgba(75, 192, 192, 1)" };

        //        if (count == 1)
        //        {
        //            backgroundcolr = new string[] { "rgba(254, 99, 131, 1)", "rgba(254, 99, 131, 1)", "rgba(254, 99, 131, 1)", "rgba(254, 99, 131, 1)" };
        //            borderColor = new string[] { "rgba(254, 99, 131, 1)", "rgba(254, 99, 131, 1)", "rgba(254, 99, 131, 1)", "rgba(254, 99, 131, 1)" };

        //        }
        //        else if (count > 1)
        //        {
        //            backgroundcolr = new string[] { "rgba(54, 162, 235, 1)", "rgba(54, 162, 235, 1)", "rgba(54, 162, 235, 1)", "rgba(54, 162, 235, 1)" };
        //            borderColor = new string[] { "rgba(75, 192, 192, 1)", "rgba(75, 192, 192, 1)", "rgba(75, 192, 192, 1)", "rgba(75, 192, 192, 1)" };
        //        }
        //        ViewData obj = new ViewData();
        //        TargetActualListDet TAL = new TargetActualListDet();
        //        TAL.CellName = machdet.MachineDisplayName;
        //        obj.name = machdet.MachineDisplayName;
        //        obj.type = "line";
        //        obj.showInLegend = "true";
        //        string StartTime = CorrectedDate.ToString("yyyy-MM-dd") + " 07:00:00";
        //        string EndTime = CorrectedDate.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss");
        //        DateTime St = Convert.ToDateTime(StartTime);
        //        DateTime Et = Convert.ToDateTime(EndTime);
        //        using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
        //        {
        //            machineDet = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.MachineID == machineDet.MachineID && m.IsBottelNeck == 1).FirstOrDefault();
        //        }
        //        if (machineDet != null)
        //        {
        //            int MachineID = machineDet.MachineID;
        //            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
        //            {
        //                partDetails = db.tblpartscountandcuttings.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate.Date && m.Isdeleted == 0 && m.StartTime >= St && m.EndTime <= Et).OrderBy(m => m.StartTime).ToList(); //.Select(m => new { m.PartCount, m.TargetQuantity, m.StartTime, m.EndTime })
        //            }
        //            int[] data = new int[partDetails.Count];
        //            List<int> Target = new List<int>();
        //            List<int> Actual = new List<int>();
        //            List<dataPoints> ActualData = new List<dataPoints>();
        //            List<dataPoints> TargetData = new List<dataPoints>();
        //            string[] Lables = new string[partDetails.Count];
        //            for (int i = 0; i < partDetails.Count; i++)
        //            {
        //                dataPoints obj1 = new dataPoints();

        //                if (i == 0)
        //                {
        //                    dataPoints obj2 = new dataPoints();
        //                    obj2.label = "Target";
        //                    obj2.markerColor = "red";
        //                    obj2.markerType = "triangle";
        //                    obj2.indexLabel = "Target";
        //                    obj2.y = partDetails[i].TargetQuantity;
        //                    ActualData.Add(obj2);

        //                }
        //                obj1.label = partDetails[i].StartTime.ToString("HH:mm") + " - " + partDetails[i].EndTime.ToString("HH:mm");
        //                obj1.y = partDetails[i].PartCount;
        //                obj1.indexLabel = "Actual";
        //                //obj2.label = partDetails[i].StartTime.ToString("HH:mm") + " - " + partDetails[i].EndTime.ToString("HH:mm");
        //                //obj2.y = partDetails[i].TargetQuantity;
        //                ActualData.Add(obj1);
        //                //TargetData.Add(obj2);
        //                //Target.Add(partDetails[i].TargetQuantity);
        //                Actual.Add(partDetails[i].PartCount);
        //                Lables[i] = partDetails[i].StartTime.ToString("HH:mm") + " - " + partDetails[i].EndTime.ToString("HH:mm");
        //            }
        //            obj.dataPoints = ActualData;
        //            // obj.dataPointsTarget = TargetData;
        //            TAL.backgroundColor = backgroundcolr;
        //            TAL.borderColor = borderColor;
        //            TAL.Timings = Lables;
        //            TAL.Target = Target;
        //            TAL.Actual = Actual;
        //            TargetList.Add(TAL);
        //            finalData.Add(obj);
        //        }
        //    }
        //    res = JsonConvert.SerializeObject(finalData);
        //    //}
        //    return res;
        //}

        //public string GetTarget_Actual_Data(int cellid)
        //{
        //    string res = "";
        //    List<ChartDataVal> ListData = new List<ChartDataVal>();
        //    ChartDataVal objListData = new ChartDataVal();
        //    objListData.type = "column";
        //    objListData.name = "Target";
        //    objListData.showInLegend = "true";
        //    objListData.indexLabel = "{y}";
        //    List<PivotalData> objList = new List<PivotalData>();
        //    List<PivotalData> objListTarget = new List<PivotalData>();
        //    string correctedDate = GetCorrectedDate();
        //    string correctdate = correctedDate;
        //    DateTime CorrectedDate = Convert.ToDateTime(correctedDate);
        //    tblmachinedetail machdet = new tblmachinedetail();
        //    tblmachinedetail machineDet = new tblmachinedetail();
        //    List<tblpartscountandcutting> partDetails = new List<tblpartscountandcutting>();
        //    List<ViewData> finalData = new List<ViewData>();
        //    using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
        //    {
        //        machdet = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == cellid).OrderBy(m => m.MachineID).FirstOrDefault();
        //    }
        //    if (machdet != null)
        //    {
        //        string StartTime = CorrectedDate.ToString("yyyy-MM-dd") + " 06:00:00";
        //        string EndTime = CorrectedDate.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss");
        //        DateTime St = Convert.ToDateTime(StartTime);
        //        DateTime Et = Convert.ToDateTime(EndTime);
        //        using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
        //        {
        //            machineDet = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.MachineID == machdet.MachineID/* && m.IsLastMachine == 1*/).FirstOrDefault(); //m.IsBottelNeck == 1
        //        }
        //        if (machineDet != null)
        //        {
        //            int MachineID = machineDet.MachineID;
        //            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
        //            {
        //                partDetails = db.tblpartscountandcuttings.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate.Date && m.Isdeleted == 0 && m.StartTime >= St && m.EndTime <= Et).OrderBy(m => m.StartTime).ToList(); //.Select(m => new { m.PartCount, m.TargetQuantity, m.StartTime, m.EndTime })
        //            }
        //            for (int i = 0; i < partDetails.Count; i++)
        //            {
        //                PivotalData obj = new PivotalData();
        //                PivotalData Tobj = new PivotalData();
        //                obj.label = partDetails[i].StartTime.ToString("HH:mm") + " - " + partDetails[i].EndTime.ToString("HH:mm");
        //                obj.y = partDetails[i].TargetQuantity;
        //                objList.Add(obj);
        //                Tobj.label = partDetails[i].StartTime.ToString("HH:mm") + " - " + partDetails[i].EndTime.ToString("HH:mm");
        //                Tobj.y = partDetails[i].PartCount;
        //                objListTarget.Add(Tobj);
        //            }
        //        }
        //    }
        //    objListData.dataPoints = objList;
        //    objListData.dataPointsTarget = objListTarget;
        //    ListData.Add(objListData);
        //    res = JsonConvert.SerializeObject(ListData);
        //    return res;
        //}

        //public string ContributingFactorLosses()
        //{
        //    List<TopContributingFactors> contfacList = new List<TopContributingFactors>();
        //    string res = "";
        //    //using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
        //    //{
        //    List<ContributingFactors> ContributingFactorsList = new List<ContributingFactors>();
        //    string[] backgroundcolr;
        //    string[] borderColor;
        //    List<ContributingFactors> ContributingFactorsListDist = new List<ContributingFactors>();
        //    List<LossDetails> objLossDistinct = new List<LossDetails>();
        //    List<LossDetails> objLoss = new List<LossDetails>();

        //    string correctedDate = GetCorrectedDate();

        //    DateTime correctedDate1 = Convert.ToDateTime(correctedDate);
        //    List<tblmachinedetail> machdet = new List<tblmachinedetail>();
        //    using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
        //    {
        //        machdet = db.tblmachinedetails.Where(m => m.IsDeleted == 0).OrderBy(m => m.MachineID).ToList();
        //    }
        //    int count = 0;
        //    foreach (tblmachinedetail machine in machdet)
        //    {
        //        Color color = GetRandomColour();
        //        string val = "rgba(" + color.R.ToString() + "," + color.G.ToString() + "," + color.B.ToString() + "," + color.A.ToString() + ")";
        //        count = count + 1;
        //        borderColor = new string[] { val, val, val, val };
        //        backgroundcolr = new string[] { val, val, val, val };
        //        List<tbllivemode> getmodes = new List<tbllivemode>();
        //        //using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
        //        //{
        //        getmodes = db.tbllivemodes.Where(m => m.tblmachinedetail.MachineID == machine.MachineID && m.tblmachinedetail.IsLastMachine == 1 && m.CorrectedDate == correctedDate1.Date && m.IsCompleted == 1 && m.ModeTypeEnd == 1 && (m.LossCodeID != null || m.BreakdownID != null)).OrderBy(m => new { m.ModeID, m.StartTime }).ToList();

        //        if (getmodes.Count == 0)
        //        {
        //            getmodes = getmodes = db.tbllivemodes.Where(m => m.tblmachinedetail.MachineID == machine.MachineID && m.tblmachinedetail.IsLastMachine == 1 && m.CorrectedDate == correctedDate1.Date && m.IsCompleted == 1 && m.ModeTypeEnd == 1).OrderBy(m => new { m.ModeID, m.StartTime }).ToList(); //&& (m.LossCodeID != null || m.BreakdownID != null)
        //        }
        //        //}
        //        string TotalLossDuration = getmodes.Where(m => m.ModeType == "IDLE").Sum(m => m.DurationInSec).ToString();
        //        double TotalLossDura = Convert.ToDouble(TotalLossDuration);
        //        TopContributingFactors contf = new TopContributingFactors();
        //        foreach (tbllivemode row in getmodes)
        //        {
        //            if ((row.LossCodeID != null && row.LossCodeID != 0) || (row.BreakdownID != null && row.BreakdownID != 0))
        //            {
        //                LossDetails loss = new LossDetails();
        //                if ((row.LossCodeID != null && row.LossCodeID != 0))
        //                {
        //                    loss.LossID = row.LossCodeID;
        //                    loss.LossCodeDescription = row.tbllossescode.LossCode;

        //                }
        //                else if (row.BreakdownID != null && row.BreakdownID != 0)
        //                {
        //                    loss.LossID = row.BreakdownID;
        //                    loss.LossCodeDescription = row.tblbreakdown.tbllossescode.LossCode.ToString();
        //                }
        //                loss.LossStartTime = Convert.ToDateTime(row.StartTime);
        //                loss.LossEndTime = Convert.ToDateTime(row.EndTime);
        //                double diff = loss.LossEndTime.Subtract(loss.LossStartTime).TotalMinutes;
        //                loss.DurationinMin = diff;
        //                loss.CellID = machine.MachineID;
        //                objLoss.Add(loss);
        //            }
        //            else
        //            {
        //                LossDetails loss = new LossDetails();
        //                loss.LossID = 0;
        //                loss.LossCodeDescription = "NO CODE";
        //                loss.DurationinMin = TimeSpan.FromSeconds(TotalLossDura).TotalMinutes;
        //                loss.CellID = machine.MachineID;
        //                objLoss.Add(loss);
        //                TotalLossDuration = getmodes.Sum(m => m.DurationInSec).ToString();
        //                TotalLossDura = Convert.ToDouble(TotalLossDuration);
        //                break;
        //            }
        //        }
        //        var idledistinct = objLoss.Where(m => m.CellID == machine.MachineID).Select(m => new { m.LossCodeDescription, m.LossID }).Distinct().ToList();
        //        foreach (var row2 in idledistinct)
        //        {
        //            ContributingFactors conf = new ContributingFactors();
        //            LossDetails det = new LossDetails();
        //            double Totalduration = 0;
        //            List<LossDetails> lossrow = objLoss.Where(m => m.LossCodeDescription == row2.LossCodeDescription && m.CellID == machine.MachineID).OrderByDescending(m => m.DurationinMin).ToList();
        //            foreach (LossDetails loss in lossrow)
        //            {
        //                if (row2.LossID == loss.LossID)
        //                {
        //                    Totalduration += loss.DurationinMin;
        //                    det = loss;
        //                    conf.LossCodeDescription = det.LossCodeDescription;
        //                }
        //            }
        //            det.DurationinMin = Totalduration;
        //            Double TotalTimeTaken = TimeSpan.FromMinutes(TotalLossDura).TotalHours;
        //            //double totaltimetaken = Convert.ToDouble(TotalTimeTaken);
        //            double lossduratin = TimeSpan.FromMinutes(Totalduration).TotalHours;
        //            // var lossPercentage = (Totalduration / TotalTimeTaken) * 100;
        //            int lossPercentage = Convert.ToInt32(lossduratin);
        //            if (TotalTimeTaken == 0)
        //            {
        //                lossPercentage = 0;
        //            }

        //            if (lossPercentage > 24)
        //            {
        //                lossPercentage = 24;
        //            }

        //            conf.cellid = machine.MachineID;
        //            contf.CellName = /*cell.tblshop.Shopdisplayname + " - " +*/ machine.MachineDisplayName;
        //            conf.LossPercent = Convert.ToDecimal(lossPercentage);
        //            conf.LossDurationInHours = Convert.ToDecimal(lossduratin);
        //            ContributingFactorsList.Add(conf);
        //            objLossDistinct.Add(det);
        //        }
        //        var contributingdistinct = ContributingFactorsList.Where(m => m.cellid == machine.MachineID).Select(m => new { m.LossCodeDescription }).Distinct().ToList();
        //        foreach (var con in contributingdistinct)
        //        {
        //            ContributingFactors row = ContributingFactorsList.Where(m => m.LossCodeDescription == con.LossCodeDescription && m.cellid == machine.MachineID).OrderByDescending(m => m.LossDurationInHours).FirstOrDefault();
        //            if (con.LossCodeDescription == row.LossCodeDescription)
        //            {
        //                ContributingFactorsListDist.Add(row);
        //            }

        //        }
        //        ContributingFactorsListDist = ContributingFactorsListDist.Where(m => m.cellid == machine.MachineID).OrderByDescending(m => m.LossPercent).Take(3).ToList();
        //        double[] data = new double[3];
        //        string[] LossNames = new string[3];
        //        ContributingFactorsListDist = ContributingFactorsListDist.Where(m => m.cellid == machine.MachineID).OrderByDescending(m => m.LossPercent).Take(3).ToList();
        //        int namecount = 0;
        //        namecount = ContributingFactorsListDist.Where(m => m.LossCodeDescription != null).ToList().Count;
        //        int j = 0;
        //        if (ContributingFactorsListDist.Count > 0 && ContributingFactorsListDist.Count == 3)
        //        {
        //            for (int i = 0; i < ContributingFactorsListDist.Count; i++)
        //            {
        //                LossNames[i] = ContributingFactorsListDist[i].LossCodeDescription;
        //                data[i] = Convert.ToDouble(ContributingFactorsListDist[i].LossPercent);
        //            }
        //        }
        //        else
        //        {
        //            for (int i = 0; i < ContributingFactorsListDist.Count; i++)
        //            {

        //                LossNames[i] = ContributingFactorsListDist[i].LossCodeDescription;
        //                data[i] = Convert.ToDouble(ContributingFactorsListDist[i].LossPercent);
        //                j = i + 1;
        //            }
        //            for (int i = j; i < (3 - namecount); i++)
        //            {
        //                LossNames[i] = "";
        //                data[i] = 0;
        //            }
        //        }
        //        contf.backgroundColor = backgroundcolr;
        //        contf.borderColor = borderColor;
        //        contf.data = data;
        //        contf.LossName = LossNames;
        //        contf.indexLabel = "{y}";
        //        contfacList.Add(contf);
        //    }

        //    //}
        //    res = JsonConvert.SerializeObject(contfacList);
        //    //}
        //    return res;
        //}

        public string ContributingFactorLosses()
        {
            List<TopContributingFactors> contfacList = new List<TopContributingFactors>();
            string res = "";
            //using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            //{
            List<ContributingFactors> ContributingFactorsList = new List<ContributingFactors>();
            string[] backgroundcolr;
            string[] borderColor;
            List<ContributingFactors> ContributingFactorsListDist = new List<ContributingFactors>();
            List<LossDetails> objLossDistinct = new List<LossDetails>();
            List<LossDetails> objLoss = new List<LossDetails>();

            string correctedDate = GetCorrectedDate();

            DateTime correctedDate1 = Convert.ToDateTime(correctedDate);
            List<tblmachinedetail> machdet = new List<tblmachinedetail>();
            List<int> DistinctLoss = new List<int>();
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                machdet = db.tblmachinedetails.Where(m => m.IsDeleted == 0).OrderBy(m => m.MachineID).ToList();
                DistinctLoss = db.tbllivelossofentries.Where(m => m.CorrectedDate == correctedDate && m.DoneWithRow == 1).Select(m => m.MessageCodeID).Distinct().ToList();
            }
            int count = 0;

            List<tbllivemode> getmodes = new List<tbllivemode>();
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                getmodes = db.tbllivemodes.Where(m => m.CorrectedDate == correctedDate1.Date && m.IsCompleted == 1 && m.ModeTypeEnd == 1).OrderBy(m => new { m.ModeID, m.StartTime }).ToList();
            }

            string TotalLossDuration = getmodes.Where(m => m.ModeType == "IDLE").Sum(m => m.DurationInSec).ToString();
            double TotalLossDura = Convert.ToDouble(TotalLossDuration);
            TopContributingFactors contf = new TopContributingFactors();
            
            Color color = GetRandomColour();
            string val = "rgba(" + color.R.ToString() + "," + color.G.ToString() + "," + color.B.ToString() + "," + color.A.ToString() + ")";
            count = count + 1;
            borderColor = new string[] { val, val, val, val };
            backgroundcolr = new string[] { val, val, val, val };
            foreach (int LossCode in DistinctLoss)
            {
                ContributingFactors conf = new ContributingFactors();
                String LossDesc = "";
                double LossDuration = 0;
                List<tbllivelossofentry> LossRecords = new List<tbllivelossofentry>();
                using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
                {
                    LossRecords = db.tbllivelossofentries.Where(m => m.CorrectedDate == correctedDate && m.DoneWithRow == 1 && m.MessageCodeID == LossCode).ToList();
                }
                foreach (tbllivelossofentry Record in LossRecords)
                {
                    DateTime EndTime = Convert.ToDateTime(Record.EndDateTime);
                    DateTime StartTime = Convert.ToDateTime(Record.StartDateTime);
                    LossDesc = Record.MessageDesc;
                    LossDuration += EndTime.Subtract(StartTime).TotalSeconds;
                }
                //conf.cellid = machine.MachineID;
                //contf.CellName = /*cell.tblshop.Shopdisplayname + " - " +*/ machine.MachineDisplayName;
                conf.LossCodeDescription = LossDesc;
                conf.LossPercent = Convert.ToDecimal(LossDuration / TotalLossDura * 100);
                conf.LossDurationInHours = Convert.ToDecimal(LossDuration / 3600);
                ContributingFactorsList.Add(conf);
            }
            double[] data = new double[3];
            string[] LossNames = new string[3];
            ContributingFactorsListDist = ContributingFactorsList.OrderByDescending(m => m.LossPercent).Take(3).ToList();
            int namecount = 0;
            namecount = ContributingFactorsListDist.Where(m => m.LossCodeDescription != null).ToList().Count;
            int j = 0;
            if (ContributingFactorsListDist.Count > 0 && ContributingFactorsListDist.Count == 3)
            {
                for (int i = 0; i < ContributingFactorsListDist.Count; i++)
                {
                    LossNames[i] = ContributingFactorsListDist[i].LossCodeDescription;
                    data[i] = Convert.ToDouble(ContributingFactorsListDist[i].LossPercent);
                }
            }
            else
            {
                for (int i = 0; i < ContributingFactorsListDist.Count; i++)
                {

                    LossNames[i] = ContributingFactorsListDist[i].LossCodeDescription;
                    data[i] = Convert.ToDouble(ContributingFactorsListDist[i].LossPercent);
                    j = i + 1;
                }
                for (int i = j; i <= (3 - namecount); i++)
                {
                    LossNames[i] = "";
                    data[i] = 0;
                }
            }
            contf.backgroundColor = backgroundcolr;
            contf.borderColor = borderColor;
            contf.data = data;
            contf.LossName = LossNames;
            contf.indexLabel = "{y}";
            contfacList.Add(contf);


            //}
            res = JsonConvert.SerializeObject(contfacList);
            //}
            return res;
        }

        public string ContributingFactorLossesByCell(int cellid)
        {
            List<TopContributingFactors> contfacList = new List<TopContributingFactors>();
            string res = "";
            //using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            //{
            List<ContributingFactors> ContributingFactorsList = new List<ContributingFactors>();
            string[] backgroundcolr;
            string[] borderColor;
            List<ContributingFactors> ContributingFactorsListDist = new List<ContributingFactors>();
            List<LossDetails> objLossDistinct = new List<LossDetails>();
            List<LossDetails> objLoss = new List<LossDetails>();
            List<ChartDataVal> ListData = new List<ChartDataVal>();
            List<PivotalData> objList = new List<PivotalData>();
            ChartDataVal objListData = new ChartDataVal();
            objListData.type = "column";

            objListData.showInLegend = "true";
            objListData.indexLabel = "{y}";
            string correctedDate = GetCorrectedDate();

            DateTime correctedDate1 = Convert.ToDateTime(correctedDate);
            List<tblmachinedetail> machdet = new List<tblmachinedetail>();
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                machdet = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == cellid).OrderBy(m => m.MachineID).ToList();
            }
            int count = 0;
            foreach (tblmachinedetail machine in machdet)
            {
                Color color = GetRandomColour();
                string val = "rgba(" + color.R.ToString() + "," + color.G.ToString() + "," + color.B.ToString() + "," + color.A.ToString() + ")";
                count = count + 1;
                borderColor = new string[] { val, val, val, val };
                backgroundcolr = new string[] { val, val, val, val };
                List<tbllivemode> getmodes = new List<tbllivemode>();
                //using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
                //{
                getmodes = db.tbllivemodes.Where(m => m.tblmachinedetail.MachineID == machine.MachineID && m.tblmachinedetail.IsLastMachine == 1 && m.CorrectedDate == correctedDate1.Date && m.IsCompleted == 1 && m.ModeTypeEnd == 1 && (m.LossCodeID != null || m.BreakdownID != null)).OrderBy(m => new { m.ModeID, m.StartTime }).ToList();
                if (getmodes.Count == 0)
                {
                    getmodes = getmodes = db.tbllivemodes.Where(m => m.tblmachinedetail.MachineID == machine.MachineID && m.tblmachinedetail.IsLastMachine == 1 && m.CorrectedDate == correctedDate1.Date && m.IsCompleted == 1 && m.ModeTypeEnd == 1).OrderBy(m => new { m.ModeID, m.StartTime }).ToList(); //&& (m.LossCodeID != null || m.BreakdownID != null)
                }
                //string lc = getmodes.tbllossescode.LossCode;
                //}
                string TotalLossDuration = getmodes.Where(m => m.ModeType == "IDLE").Sum(m => m.DurationInSec).ToString();
                double TotalLossDura = Convert.ToDouble(TotalLossDuration);
                TopContributingFactors contf = new TopContributingFactors();
                foreach (tbllivemode row in getmodes)
                {
                    if ((row.LossCodeID != null && row.LossCodeID != 0) || (row.BreakdownID != null && row.BreakdownID != 0))
                    {
                        LossDetails loss = new LossDetails();
                        if ((row.LossCodeID != null && row.LossCodeID != 0))
                        {
                            loss.LossID = row.LossCodeID;
                            loss.LossCodeDescription = row.tbllossescode.LossCode;

                        }
                        else if (row.BreakdownID != null && row.BreakdownID != 0)
                        {
                            loss.LossID = row.BreakdownID;
                            loss.LossCodeDescription = row.tblbreakdown.tbllossescode.LossCode.ToString();
                        }
                        loss.LossStartTime = Convert.ToDateTime(row.StartTime);
                        loss.LossEndTime = Convert.ToDateTime(row.EndTime);
                        double diff = loss.LossEndTime.Subtract(loss.LossStartTime).TotalMinutes;
                        loss.DurationinMin = diff;
                        loss.CellID = machine.MachineID;
                        objLoss.Add(loss);
                    }
                    else
                    {
                        LossDetails loss = new LossDetails();
                        loss.LossID = 0;
                        loss.LossCodeDescription = "NO CODE";
                        loss.DurationinMin = TimeSpan.FromSeconds(TotalLossDura).TotalMinutes;
                        loss.CellID = machine.MachineID;
                        objLoss.Add(loss);
                        TotalLossDuration = getmodes.Sum(m => m.DurationInSec).ToString();
                        TotalLossDura = Convert.ToDouble(TotalLossDuration);
                        break;
                    }
                }
                var idledistinct = objLoss.Where(m => m.CellID == machine.MachineID).Select(m => new { m.LossCodeDescription, m.LossID }).Distinct().ToList();
                foreach (var row2 in idledistinct)
                {
                    ContributingFactors conf = new ContributingFactors();
                    LossDetails det = new LossDetails();
                    double Totalduration = 0;
                    List<LossDetails> lossrow = objLoss.Where(m => m.LossCodeDescription == row2.LossCodeDescription && m.CellID == machine.MachineID).OrderByDescending(m => m.DurationinMin).ToList();
                    foreach (LossDetails loss in lossrow)
                    {
                        if (row2.LossID == loss.LossID)
                        {
                            Totalduration += loss.DurationinMin;
                            det = loss;
                            conf.LossCodeDescription = det.LossCodeDescription;
                        }
                    }
                    det.DurationinMin = Totalduration;
                    Double TotalTimeTaken = TimeSpan.FromMinutes(TotalLossDura).TotalHours;
                    //double totaltimetaken = Convert.ToDouble(TotalTimeTaken);
                    double lossduratin = TimeSpan.FromMinutes(Totalduration).TotalHours;
                    // var lossPercentage = (Totalduration / TotalTimeTaken) * 100;
                    int lossPercentage = Convert.ToInt32(lossduratin);
                    if (TotalTimeTaken == 0)
                    {
                        lossPercentage = 0;
                    }

                    if (lossPercentage > 24)
                    {
                        lossPercentage = 24;
                    }

                    conf.cellid = machine.MachineID;
                    contf.CellName = /*cell.tblshop.Shopdisplayname + " - " +*/ machine.MachineDisplayName;
                    conf.LossPercent = Convert.ToDecimal(lossPercentage);
                    conf.LossDurationInHours = Convert.ToDecimal(lossduratin);
                    ContributingFactorsList.Add(conf);
                    objLossDistinct.Add(det);
                }
                var contributingdistinct = ContributingFactorsList.Where(m => m.cellid == machine.MachineID).Select(m => new { m.LossCodeDescription }).Distinct().ToList();
                foreach (var con in contributingdistinct)
                {

                    ContributingFactors row = ContributingFactorsList.Where(m => m.LossCodeDescription == con.LossCodeDescription && m.cellid == machine.MachineID).OrderByDescending(m => m.LossDurationInHours).FirstOrDefault();
                    if (con.LossCodeDescription == row.LossCodeDescription)
                    {
                        ContributingFactorsListDist.Add(row);
                    }

                }
                ContributingFactorsListDist = ContributingFactorsListDist.Where(m => m.cellid == machine.MachineID).OrderByDescending(m => m.LossPercent).Take(3).ToList();
                double[] data = new double[3];
                string[] LossNames = new string[3];
                ContributingFactorsListDist = ContributingFactorsListDist.Where(m => m.cellid == machine.MachineID).OrderByDescending(m => m.LossPercent).Take(3).ToList();
                int namecount = 0;
                namecount = ContributingFactorsListDist.Where(m => m.LossCodeDescription != null).ToList().Count;
                int j = 0;
                if (ContributingFactorsListDist.Count > 0 && ContributingFactorsListDist.Count == 3)
                {
                    for (int i = 0; i < ContributingFactorsListDist.Count; i++)
                    {
                        PivotalData obj = new PivotalData();
                        obj.label = ContributingFactorsListDist[i].LossCodeDescription;
                        obj.y = Convert.ToInt32(ContributingFactorsListDist[i].LossPercent);
                        objList.Add(obj);
                        LossNames[i] = ContributingFactorsListDist[i].LossCodeDescription;
                        data[i] = Convert.ToDouble(ContributingFactorsListDist[i].LossPercent);
                    }
                }
                else
                {

                    for (int i = 0; i < ContributingFactorsListDist.Count; i++)
                    {
                        PivotalData obj = new PivotalData();
                        obj.label = ContributingFactorsListDist[i].LossCodeDescription;
                        decimal lossduration = Math.Round(Convert.ToDecimal(ContributingFactorsListDist[i].LossPercent), 2);
                        obj.y = Convert.ToInt32(lossduration);
                        objListData.name = ContributingFactorsListDist[i].LossCodeDescription;
                        objList.Add(obj);
                        LossNames[i] = ContributingFactorsListDist[i].LossCodeDescription;
                        data[i] = Convert.ToDouble(ContributingFactorsListDist[i].LossPercent);
                        j = i + 1;
                    }
                    for (int i = j; i < (3 - namecount); i++)
                    {
                        LossNames[i] = "";
                        data[i] = 0;
                    }
                }
                contf.backgroundColor = backgroundcolr;
                contf.borderColor = borderColor;
                contf.data = data;
                contf.LossName = LossNames;
                contf.indexLabel = "{y}";
                contfacList.Add(contf);
                objListData.dataPoints = objList;

                //objListData.dataPointsTarget = objListTarget;
                ListData.Add(objListData);

            }

            res = JsonConvert.SerializeObject(ListData);
            //}
            //res = JsonConvert.SerializeObject(contfacList);
            //}
            return res;
        }

        public int QualityFactor_Piweb(int MachineID, string UsedDateForExcel)
        {
            double qualitydata = 0;

            using (i_facility_shaktiEntities1 dbhmi = new i_facility_shaktiEntities1())
            {
                List<tblhmiscreen> PartsData = dbhmi.tblhmiscreens.Where(m => m.CorrectedDate == UsedDateForExcel && m.MachineID == MachineID && (m.isWorkInProgress == 1 || m.isWorkInProgress == 0) && m.isWorkOrder == 0).ToList();
                //if (scpid == 3)
                //{
                //    PartsData = dbhmi.tblhmiscreens.Where(m => m.CorrectedDate == UsedDateForExcel && m.tblmachinedetail.CellID == MachineID && (m.isWorkInProgress == 1 || m.isWorkInProgress == 0) && m.isWorkOrder == 0).ToList();
                //}
                //if (scpid == 5)
                //{
                //    PartsData = dbhmi.tblhmiscreens.Where(m => m.CorrectedDate == UsedDateForExcel && m.tblmachinedetail.ShopID == MachineID && (m.isWorkInProgress == 1 || m.isWorkInProgress == 0) && m.isWorkOrder == 0).ToList();
                //}
                //if (scpid == 1)
                //{
                //    PartsData = dbhmi.tblhmiscreens.Where(m => m.CorrectedDate == UsedDateForExcel && m.tblmachinedetail.PlantID == MachineID && (m.isWorkInProgress == 1 || m.isWorkInProgress == 0) && m.isWorkOrder == 0).ToList();
                //}
                double totaldelquantity = 0;
                double totalrejqty = 0;
                double totalreworkqty = 0;
                foreach (tblhmiscreen row in PartsData)
                {
                    // var machinedata = row.MachineID;
                    string partno = row.PartNo;
                    int operationno = Convert.ToInt32(row.OperationNo);
                    int scrapQty = 0;
                    int DeliveredQty = 0;

                    tblQuality_Piweb qulaity_row = new tblQuality_Piweb();
                    using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
                    {
                        qulaity_row = db.tblQuality_Piweb.Where(m => m.PartNumber == partno && m.OperationNum == operationno && m.MachineID == MachineID && m.CorrectedDate == UsedDateForExcel).FirstOrDefault();
                    }
                    int rejectQty = 0;
                    int totalQty = 0;
                    if (qulaity_row != null)
                    {
                        //totalQty = Convert.ToInt32(qulaity_row.TotalQty); //total qty
                        totalQty = Convert.ToInt32(qulaity_row.ApprovedQty) + Convert.ToInt32(qulaity_row.RejectedQty);    //Approvedqty + Rejectionqty = Delivered Qty
                        rejectQty = Convert.ToInt32(qulaity_row.RejectedQty); //difference of total qty and approved qty
                    }
                    string scrapQtyString = Convert.ToString(row.Rej_Qty);
                    string DeliveredQtyString = Convert.ToString(row.Delivered_Qty);
                    string x = scrapQtyString;
                    double rework = 0;
                    double deliveredQty = Convert.ToInt32(DeliveredQtyString);

                    scrapQty = rejectQty;
                    DeliveredQty = totalQty;


                    //qualitydata = Convert.ToDouble(((decimal)rejectQty /(decimal) totalQty));   //this condition was commented for verification purpose on 2020-05-17
                    int? reworkTime = dbhmi.tbllivehmiscreens.Where(m => m.MachineID == MachineID && m.CorrectedDate == UsedDateForExcel && m.isWorkOrder == 1).Sum(m => m.Delivered_Qty);
                    if (reworkTime != null)
                    {
                        rework = Convert.ToInt32(reworkTime);

                    }

                    totaldelquantity += DeliveredQty;
                    totalrejqty += scrapQty;
                    totalreworkqty += rework;
                    //qualitydata = ((deliveredQty - scrapQty - rework) / (deliveredQty) * 100);

                }
                if (totaldelquantity == 0)
                {
                    totaldelquantity = 1;
                }

                qualitydata = ((totaldelquantity - totalrejqty - totalreworkqty) / (totaldelquantity) * 100);

            }
            int quality = Convert.ToInt32(qualitydata);
            return quality;
        }

        public int QualityFactor_PiwebReject(int MachineID, string UsedDateForExcel)
        {
            int qualitydata = 0;


            using (i_facility_shaktiEntities1 dbhmi = new i_facility_shaktiEntities1())
            {
                List<tbllivehmiscreen> PartsData = dbhmi.tbllivehmiscreens.Where(m => m.CorrectedDate == UsedDateForExcel && m.MachineID == MachineID && (m.isWorkInProgress == 1 || m.isWorkInProgress == 0) && m.isWorkOrder == 0).ToList();
                foreach (tbllivehmiscreen row in PartsData)
                {
                    string partno = row.PartNo;
                    int operationno = Convert.ToInt32(row.OperationNo);
                    int scrapQty = 0;
                    int DeliveredQty = 0;

                    tblQuality_Piweb qulaity_row = new tblQuality_Piweb();
                    using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
                    {
                        qulaity_row = db.tblQuality_Piweb.Where(m => m.PartNumber == partno && m.OperationNum == operationno && m.MachineID == MachineID && m.CorrectedDate == UsedDateForExcel).FirstOrDefault();
                    }
                    int rejectQty = 0;
                    int totalQty = 0;
                    if (qulaity_row != null)
                    {
                        rejectQty = Convert.ToInt32(qulaity_row.RejectedQty);
                        totalQty = Convert.ToInt32(qulaity_row.TotalQty);

                    }
                    string scrapQtyString = Convert.ToString(row.Rej_Qty);
                    string DeliveredQtyString = Convert.ToString(row.Delivered_Qty);
                    string x = scrapQtyString;

                    scrapQty = rejectQty;
                    DeliveredQty = totalQty;

                    if (totalQty == 0)
                    {
                        totalQty = 1;
                    }

                    qualitydata = (rejectQty / totalQty);

                    qualitydata = rejectQty;
                }
            }
            return qualitydata;
        }

        public int get_PlanBreakdownduration()
        {

            int duration = 0;
            //var msgs2 = new List<tblplannedbreak>();

            using (i_facility_shaktiEntities1 db1 = new i_facility_shaktiEntities1())
            {
                int? msgs2 = db1.tblplannedbreaks.Where(m => m.IsDeleted == 0).Sum(m => m.BreakDuration);
                duration = Convert.ToInt32(msgs2);
            }

            //String[] msgtime = DateTime.Now.ToString("HH:mm:00").Split(':');
            //TimeSpan msgstime = new TimeSpan(Convert.ToInt32(msgtime[0]), Convert.ToInt32(msgtime[1]), Convert.ToInt32(msgtime[2]));
            //TimeSpan s1t1 = new TimeSpan(0, 0, 0), s1t2 = new TimeSpan(0, 0, 0);
            //TimeSpan s2t1 = new TimeSpan(0, 0, 0), s2t2 = new TimeSpan(0, 0, 0);
            //TimeSpan s3t1 = new TimeSpan(0, 0, 0), s3t2 = new TimeSpan(0, 0, 0), s3t3 = new TimeSpan(0, 0, 0), s3t4 = new TimeSpan(23, 59, 59);


            //for (int j = 0; j < msgs2.Count; j++)
            //{
            //    if (msgs2[j].ShiftID.ToString().Contains("1") || msgs2[j].ShiftID.ToString().Contains("A"))
            //    {
            //        String[] s1 = msgs2[j].StartTime.ToString().Split(':');
            //        s1t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
            //        String[] s11 = msgs2[j].EndTime.ToString().Split(':');
            //        s1t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));

            //        if (msgstime >= s1t1 || msgstime < s1t2)
            //        {
            //            duration += Convert.ToInt32(msgs2[j].BreakDuration);
            //        }
            //    }
            //    if (msgs2[j].ShiftID.ToString().Contains("2") || msgs2[j].ShiftID.ToString().Contains("B"))
            //    {
            //        String[] s1 = msgs2[j].StartTime.ToString().Split(':');
            //        s2t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
            //        String[] s11 = msgs2[j].EndTime.ToString().Split(':');
            //        s2t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));

            //        if (msgstime >= s2t1 && msgstime < s2t2)
            //        {
            //            duration += Convert.ToInt32(msgs2[j].BreakDuration);
            //        }
            //    }
            //    if (msgs2[j].ShiftID.ToString().Contains("3") || msgs2[j].ShiftID.ToString().Contains("C"))
            //    {
            //        String[] s1 = msgs2[j].StartTime.ToString().Split(':');
            //        s3t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
            //        String[] s11 = msgs2[j].EndTime.ToString().Split(':');
            //        s3t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));


            //        if (msgstime >= s3t1 && msgstime < s3t2)
            //        {
            //            duration += Convert.ToInt32(msgs2[j].BreakDuration);
            //        }
            //    }


            //}

            return duration;

        }
    }
}

public class LosslIstforDB
{

    public string LossMessage { get; set; }
    public Double LossDuration { get; set; }
}