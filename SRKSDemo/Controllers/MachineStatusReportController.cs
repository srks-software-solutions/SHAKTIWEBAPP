using SRKSDemo.Models;
using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SRKSDemo.Controllers
{
    public class MachineStatusReportController : Controller
    {
        i_facility_shaktiEntities1 Serverdb = new i_facility_shaktiEntities1();
        string databaseName = ConfigurationManager.AppSettings["dbName"];
        // GET: MachineStatusReport
        [HttpGet]
        public ActionResult DashboardStatus()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["WorkCenterID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineInvNo");


            return View();
        }

        [HttpPost]
        public ActionResult DashboardStatus(string FromDate)
        {
            string from = Convert.ToDateTime(FromDate).ToString("yyyy-MM-dd");
            string TodayDate = DateTime.Now.ToString("yyyy-MM-dd");
            DateTime Stdate = Convert.ToDateTime(from);
            DateTime temp = Stdate.AddDays(-1);
            //DateTime endate = Convert.ToDateTime(ToDate);
            string PrvCorrectedDate = temp.ToString("yyyy-MM-dd");
            return RedirectToAction("Index", new { CorrectedDate = from, PrvCorrectedDate = PrvCorrectedDate });
        }


        //public ActionResult Index(string CorrectedDate, string PrvCorrectedDate)
        //{
        //    if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
        //    {
        //        return RedirectToAction("Login", "Login", null);
        //    }

        //    Session["colordata"] = null;
        //    ViewBag.Logout = Session["Username"];
        //    ViewBag.roleid = Session["RoleID"];
        //    ViewBag.StartDate = CorrectedDate;
        //    DateTime correcteDate = Convert.ToDateTime(CorrectedDate);
        //    calculating Corrected Date
        //    TimeSpan currentHourMint = new TimeSpan(05, 59, 59);
        //    TimeSpan RealCurrntHour = System.DateTime.Now.TimeOfDay;
        //    string CorrectedDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
        //    if (RealCurrntHour < currentHourMint)
        //    {
        //        CorrectedDate = DateTime.Now.AddDays(-1).Date.ToString("yyyy-MM-dd");
        //    }

        //    getting all machine details and their count.
        //   var macData = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0);
        //    int mc = macData.Count();
        //    ViewBag.macCount = mc;

        //    int[] macid = new int[mc];
        //    int macidlooper = 0;
        //    foreach (var v in macData)
        //    {
        //        macid[macidlooper++] = v.MachineID;
        //    }
        //    Session["macid"] = macid;
        //    ViewBag.macCount = mc;

        //    int[,] maindata = new int[mc, 5];
        //    int[,] maindata = new int[mc, 6];
        //    write a raw query to get sum of powerOff, Operating, Idle, BreakDown, PlannedMaintenance.

        //    using (MsqlConnection mc1 = new MsqlConnection())
        //    {
        //        mc1.open();
        //        SqlCommand cmd1 = new SqlCommand("SELECT MachineID,sum(MachineOffTime) as op,sum(OperatingTime)as o,sum(IdleTime) as it,sum(BreakdownTime)as bt FROM i_facility_tsal.dbo.tblmimics where CorrectedDate='" + CorrectedDate + "'and MachineID IN (select distinct(MachineID) from tblmachinedetails where IsDeleted = 0 and IsNormalWC = 0) group by MachineID", mc1.msqlConnection);
        //        SqlCommand cmd1 = " SELECT MachineID, sum(convert(int, MachineOffTime) )as op,sum(convert(int, OperatingTime)) as o,sum(convert(int, IdleTime)) as it,sum(convert(int, BreakdownTime)) as bt FROM i_facility_tsal.dbo.tblmimics where CorrectedDate = '2019-06-21'and MachineID IN(select distinct(MachineID) from tblmachinedetails where IsDeleted = 0 and IsNormalWC = 0) group by MachineID",
        //        SqlCommand cmd1 = new SqlCommand("SELECT MachineID,sum(convert (int,MachineOffTime) )as op,sum(convert (int,OperatingTime))as o,sum(convert (int,IdleTime)) as it,sum(convert (int,BreakdownTime))as bt FROM i_facility_tsal.dbo.tblmimics where CorrectedDate='" + CorrectedDate + "' and MachineID IN (select distinct(MachineID) from tblmachinedetails where IsDeleted = 0 and IsNormalWC = 0) group by MachineID", mc1.msqlConnection);

        //        SqlCommand cmd1 = new SqlCommand("SELECT MachineID, sum(convert(int, MachineOffTime) )as op,sum(convert(int, OperatingTime)) as o,sum(convert(int, IdleTime)) as it,sum(convert(int, BreakdownTime)) as bt FROM [i_facility_WeirMinerals].[dbo].tblmimics where CorrectedDate = '" + CorrectedDate + "' and MachineID IN(select distinct(MachineID) from [i_facility_WeirMinerals].[dbo].tblmachinedetails where IsDeleted = 0 and IsNormalWC = 0) group by MachineID", mc1.msqlConnection);

        //        SqlDataReader datareader = cmd1.ExecuteReader();
        //        int maindatalooper1 = 0;

        //        while (datareader.Read())
        //        {
        //            int maindatalooper2 = 0;
        //            maindata[maindatalooper1, maindatalooper2++] = datareader.GetInt32(0);
        //            maindata[maindatalooper1, maindatalooper2++] = datareader.GetInt32(1);
        //            maindata[maindatalooper1, maindatalooper2++] = datareader.GetInt32(2);
        //            maindata[maindatalooper1, maindatalooper2++] = datareader.GetInt32(3);
        //            maindata[maindatalooper1, maindatalooper2++] = datareader.GetInt32(4);
        //            maindatalooper1++;
        //        }
        //        mc1.close();
        //    }
        //    Session["colordata"] = maindata;
        //    var tblMainDT = Serverdb.tbllivedailyprodstatus.Include(t => t.tblmachinedetail).Where(m => m.CorrectedDate == CorrectedDate).OrderBy(m => m.StartTime);
        //    return View(tblMainDT.ToList());

        //    Get Modes for All Machines for Today
        //    List < tbllivemodedb > tblModeDT = Serverdb.tblmodes.Where(m => m.CorrectedDate == CorrectedDate && m.tblmachinedetail.IsDeleted == 0 && m.tblmachinedetail.IsNormalWC == 0).OrderBy(m => m.MachineID).ThenBy(m => m.StartTime).ToList();

        //    List < tblmode > tblModeDT = Serverdb.tblmodes.Where(m => m.CorrectedDate == correcteDate && m.tblmachinedetail.IsDeleted == 0 && m.tblmachinedetail.IsNormalWC == 0 && m.IsCompleted == 1).OrderBy(m => m.MachineID).ThenBy(m => m.StartTime).ToList();
        //    List<tblmode> tblModeDTCurr = Serverdb.tblmodes.Where(m => m.CorrectedDate == correcteDate && m.tblmachinedetail.IsDeleted == 0 && m.tblmachinedetail.IsNormalWC == 0 && m.IsCompleted == 0).OrderBy(m => m.MachineID).ThenByDescending(m => m.ModeID).ToList();

        //    Get Latest Mode for each machine and Update the DurationInSec Column
        //    List < tblmode > CurrentModesOfAllMachines = (from row in tblModeDT
        //                                                  where row.IsCompleted == 0
        //                                                  select row).ToList();
        //    int PrvMachineID = 0;
        //    foreach (var row in tblModeDTCurr)
        //    {
        //        DateTime startDateTime = Convert.ToDateTime(row.StartTime);
        //        int DurInSec = Convert.ToInt32(DateTime.Now.Subtract(startDateTime).TotalSeconds);
        //        //row.DurationInSec = Convert.ToInt32( DateTime.Now.Subtract(startDateTime).TotalSeconds );
        //        int ModeID = row.ModeID;
        //        foreach (var tom in tblModeDT.Where(w => w.ModeID == ModeID))
        //        {
        //            tom.DurationInSec = DurInSec;
        //        }

        //        if (PrvMachineID != row.MachineID)
        //        {
        //            DateTime startDateTime = Convert.ToDateTime(row.StartTime);
        //            int DurInSec = Convert.ToInt32(DateTime.Now.Subtract(startDateTime).TotalSeconds);
        //            row.DurationInSec = Convert.ToInt32(DateTime.Now.Subtract(startDateTime).TotalSeconds);
        //            int ModeID = row.ModeID;
        //            row.DurationInSec = DurInSec;
        //            tblModeDT.Add(row);
        //            foreach (var tom in tblModeDT.Where(w => w.ModeID == ModeID))
        //            {

        //            }
        //            PrvMachineID = row.MachineID;
        //        }
        //    }
        //    List<DBMode> ShowMode = new List<DBMode>();
        //    Update DurationInSec to Minutes
        //    foreach (var MainRow in tblModeDT.Where(m => m.DurationInSec > 0))
        //    {
        //        DBMode ShowModeItem = new DBMode();
        //        ShowModeItem.ColorCode = MainRow.ColorCode;
        //        ShowModeItem.CorrectedDate = Convert.ToString(MainRow.CorrectedDate);
        //        ShowModeItem.DurationInSec = MainRow.DurationInSec / 60.00;
        //        ShowModeItem.EndTime = MainRow.EndTime;
        //        ShowModeItem.InsertedBy = MainRow.InsertedBy;
        //        ShowModeItem.InsertedOn = MainRow.InsertedOn;
        //        ShowModeItem.IsCompleted = MainRow.IsCompleted;
        //        ShowModeItem.IsDeleted = MainRow.IsDeleted;
        //        ShowModeItem.MachineID = MainRow.MachineID;
        //        ShowModeItem.Mode = MainRow.MacMode;
        //        ShowModeItem.ModeID = MainRow.ModeID;
        //        ShowModeItem.ModifiedBy = MainRow.ModifiedBy;
        //        ShowModeItem.ModifiedOn = MainRow.ModifiedOn;
        //        ShowModeItem.StartTime = MainRow.StartTime;
        //        ShowModeItem.tblmachinedetail = MainRow.tblmachinedetail;
        //        ShowMode.Add(ShowModeItem);
        //        MainRow.DurationInSec = Convert.ToInt32(MainRow.DurationInSec / 60);
        //    };

        //    List<string> ShopNames = Serverdb.tblmodes.Where(m => m.CorrectedDate == correcteDate && m.tblmachinedetail.IsDeleted == 0 && m.tblmachinedetail.IsNormalWC == 0).Select(m => m.tblmachinedetail.ShopNo).Distinct().ToList();
        //    ViewBag.DistinctShops = ShopNames;

        //    return View(tblModeDT);
        //    return View(ShowMode);
        //}

        public ActionResult Index(string CorrectedDate,string PrvCorrectedDate)
        {
            //if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            //{
            //    return RedirectToAction("Login", "Login", null);
            //}

            Session["CellId"] = 1;
            Session["colordata"] = null;
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            DateTime correcteDate = Convert.ToDateTime(CorrectedDate);
            //    //calculating Corrected Date
            //    TimeSpan currentHourMint = new TimeSpan(05, 59, 59);
            //    TimeSpan RealCurrntHour = System.DateTime.Now.TimeOfDay;
            //    //string CorrectedDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
            //    //if (RealCurrntHour < currentHourMint)
            //    //{
            //    //    CorrectedDate = DateTime.Now.AddDays(-1).Date.ToString("yyyy-MM-dd");
            //    //}

            //calculating Corrected Date
            TimeSpan currentHourMint = new TimeSpan(05, 59, 59);
            TimeSpan RealCurrntHour = System.DateTime.Now.TimeOfDay;
            //string CorrectedDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
            //DateTime correctedDate = DateTime.Now.Date;
            //string PrvCorrectedDate = correctedDate.AddDays(-1).Date.ToString("yyyy-MM-dd");
            //if (RealCurrntHour < currentHourMint)
            //{
            //    CorrectedDate = DateTime.Now.AddDays(-1).Date.ToString("yyyy-MM-dd");
            //    correctedDate = DateTime.Now.AddDays(-1).Date;
            //    PrvCorrectedDate = correctedDate.AddDays(-1).Date.ToString("yyyy-MM-dd");
            //}

            // getting all machine details and their count.
            var macData = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0);
            int mc = macData.Count();
            ViewBag.macCount = mc;

            //int[] macid = new int[mc];
            //int macidlooper = 0;
            //foreach (var v in macData)
            //{
            //    macid[macidlooper++] = v.MachineID;
            //}
            //Session["macid"] = macid;
            //ViewBag.macCount = mc;

            //int[,] maindata = new int[mc, 10];
            //int[,] maindata = new int[mc, 6];
            // write a raw query to get sum of powerOff, Operating, Idle, BreakDown, PlannedMaintenance. 
            List<Models.MachineStatus> objmachstatUpdate = new List<Models.MachineStatus>();
            using (MsqlConnection mc1 = new MsqlConnection())
            {
                mc1.open();
                SqlCommand cmd1 = new SqlCommand("SELECT MachineID,sum(MachineOffTime) as op,sum(OperatingTime)as o,sum(IdleTime) as it,sum(BreakdownTime)as bt FROM " + databaseName + ".tblmimics where CorrectedDate='" + CorrectedDate + "'and MachineID IN (select distinct(MachineID) from " + databaseName + ".tblmachinedetails where IsDeleted = 0 and IsNormalWC = 0) group by MachineID", mc1.msqlConnection);
                SqlDataReader datareader = cmd1.ExecuteReader();
                // int maindatalooper1 = 0;
                List<Models.MachineStatus> objmachstat = new List<Models.MachineStatus>();

                while (datareader.Read())
                {
                    MachineStatus macstatus = new MachineStatus();
                    //int maindatalooper2 = 0;                   
                    int MachineOffTime = datareader.GetInt32(1);
                    int OpertTime = datareader.GetInt32(2);
                    int IdleTime = datareader.GetInt32(3);
                    int BDTime = datareader.GetInt32(4);
                    int TotalTime = MachineOffTime + OpertTime + IdleTime + BDTime;
                    if (TotalTime == 0)
                    {
                        TotalTime = 1;
                    }
                    int UtilPer = Convert.ToInt32(Convert.ToDouble(Convert.ToDouble(Convert.ToDouble(OpertTime) / Convert.ToDouble(TotalTime)) * 100));
                    macstatus.MachineID = datareader.GetInt32(0);
                    var cellname = Serverdb.tblmachinedetails.Where(m => m.MachineID == macstatus.MachineID).Select(m => m.tblcell.CelldisplayName).FirstOrDefault();
                    var machinedet = Serverdb.tblmachinedetails.Where(m => m.MachineID == macstatus.MachineID).FirstOrDefault();
                    macstatus.CellName = cellname;
                    macstatus.MachineOffTime = datareader.GetInt32(1);
                    macstatus.OperatingTime = datareader.GetInt32(2);
                    macstatus.IdleTime = datareader.GetInt32(3);
                    macstatus.BreakdownTime = datareader.GetInt32(4); ;
                    macstatus.Utilization = UtilPer;
                    macstatus.TotalTime = TotalTime;
                    macstatus.machinedet = machinedet;
                    //maindata[maindatalooper1, maindatalooper2++] = datareader.GetInt32(0);
                    //maindata[maindatalooper1, maindatalooper2++] = datareader.GetInt32(1);
                    //maindata[maindatalooper1, maindatalooper2++] = datareader.GetInt32(2);
                    //maindata[maindatalooper1, maindatalooper2++] = datareader.GetInt32(3);
                    //maindata[maindatalooper1, maindatalooper2++] = datareader.GetInt32(4);
                    //maindata[maindatalooper1, maindatalooper2++] = UtilPer;
                    //maindata[maindatalooper1, maindatalooper2++] = TotalTime;
                    //maindatalooper1++;
                    objmachstat.Add(macstatus);
                }
                datareader.Close();

                SqlCommand Prvcmd1 = new SqlCommand("SELECT MachineID,sum(MachineOffTime) as op,sum(OperatingTime)as o,sum(IdleTime) as it,sum(BreakdownTime)as bt FROM " + databaseName + ".tblmimics where CorrectedDate='" + PrvCorrectedDate + "'and MachineID IN (select distinct(MachineID) from " + databaseName + ".tblmachinedetails where IsDeleted = 0 and IsNormalWC = 0) group by MachineID", mc1.msqlConnection);
                SqlDataReader Prvdatareader = Prvcmd1.ExecuteReader();
                //  int Prvmaindatalooper1 = 0;
                while (Prvdatareader.Read())
                {
                    int MachineOffTime = Prvdatareader.GetInt32(1);
                    int OpertTime = Prvdatareader.GetInt32(2);
                    int IdleTime = Prvdatareader.GetInt32(3);
                    int BDTime = Prvdatareader.GetInt32(4);
                    int TotalTime = MachineOffTime + OpertTime + IdleTime + BDTime;
                    if (TotalTime == 0)
                    {
                        TotalTime = 1;
                    }
                    int UtilPer = Convert.ToInt32(Convert.ToDouble(Convert.ToDouble(Convert.ToDouble(OpertTime) / Convert.ToDouble(TotalTime)) * 100));
                    foreach (var row in objmachstat)
                    {
                        if (row.MachineID == Prvdatareader.GetInt32(0))
                        {
                            row.PrevOperatingTime = OpertTime;
                            row.PrevTotalTime = TotalTime;
                            row.PrevUtilization = UtilPer;
                            objmachstatUpdate.Add(row);

                        }
                    }

                    //maindata[Prvmaindatalooper1, 7] = UtilPer;
                    //maindata[Prvmaindatalooper1, 8] = OpertTime;
                    //maindata[Prvmaindatalooper1, 9] = TotalTime;
                    //Prvmaindatalooper1++;
                }
                Prvdatareader.Close();
                mc1.close();
            }
            // Session["colordata"] = maindata;
            objmachstatUpdate = objmachstatUpdate.OrderBy(m => m.CellName).ToList();
            Session["MachineStauts"] = objmachstatUpdate.OrderBy(m => m.CellName).ToList();

            //Get Modes for All Machines for Today
            List<tbllivemode> tbllivemodeDT = Serverdb.tbllivemodes.Where(m => m.CorrectedDate == correcteDate && m.tblmachinedetail.IsDeleted == 0 && m.tblmachinedetail.IsNormalWC == 0 && m.IsCompleted == 1).OrderBy(m => m.MachineID).ThenBy(m => m.StartTime).ToList();
            List<tbllivemode> tbllivemodeDTCurr = Serverdb.tbllivemodes.Where(m => m.CorrectedDate == correcteDate && m.tblmachinedetail.IsDeleted == 0 && m.tblmachinedetail.IsNormalWC == 0 && m.IsCompleted == 0).OrderBy(m => m.MachineID).ThenByDescending(m => m.ModeID).ToList();
            //Get Latest Mode for each machine and Update the DurationInSec Column
            List<tbllivemode> CurrentModesOfAllMachines = (from row in tbllivemodeDT
                                                           where row.IsCompleted == 0
                                                           orderby row.ModeID descending
                                                           select row).ToList().OrderByDescending(m => m.ModeID).ToList();
            int PrvMachineID = 0;
            foreach (var row in tbllivemodeDTCurr)
            {
                if (PrvMachineID != row.MachineID)
                {
                    DateTime startDateTime = Convert.ToDateTime(row.StartTime);
                    int DurInSec = Convert.ToInt32(DateTime.Now.Subtract(startDateTime).TotalSeconds);
                    //row.DurationInSec = Convert.ToInt32( DateTime.Now.Subtract(startDateTime).TotalSeconds );
                    int ModeID = row.ModeID;
                    row.DurationInSec = DurInSec;
                    tbllivemodeDT.Add(row);
                    //foreach (var tom in tbllivemodeDT.Where(w => w.ModeID == ModeID))
                    //{

                    //}
                    PrvMachineID = row.MachineID;
                }
                if (row.ModeType == "SETUP")
                {
                    DateTime StartTime = Convert.ToDateTime(row.StartTime);
                    DateTime EndTime = DateTime.Now;
                    try
                    {
                        EndTime = Convert.ToDateTime(row.LossCodeEnteredTime);
                    }
                    catch { }
                    int DurInSec = Convert.ToInt32(EndTime.Subtract(StartTime).TotalSeconds);
                    int ModeID = row.ModeID;
                    row.DurationInSec = DurInSec;
                    tbllivemodeDT.Add(row);
                }
            }
            //Update DurationInSec to Minutes
            foreach (var MainRow in tbllivemodeDT.Where(m => m.DurationInSec > 0))
            {
                int GetDur = (int)MainRow.DurationInSec / 60;
                if (MainRow.ModeType == "SETUP")
                {
                    GetDur = (int)Convert.ToDateTime(MainRow.LossCodeEnteredTime).Subtract(Convert.ToDateTime(MainRow.StartTime)).TotalSeconds / 60;
                }
                if (GetDur < 1)
                {
                    GetDur = 0;
                }
                MainRow.DurationInSec = GetDur;
            };
            List<string> ShopNames = Serverdb.tbllivemodes.Where(m => m.CorrectedDate == correcteDate && m.tblmachinedetail.IsDeleted == 0 && m.tblmachinedetail.IsNormalWC == 0).OrderBy(m => m.tblmachinedetail.tblcell.CelldisplayName).Select(m => m.tblmachinedetail.tblcell.CelldisplayName).Distinct().ToList();
            //  List<int> ShopID = Serverdb.tbllivemodes.Where(m => m.CorrectedDate == correctedDate && m.tblmachinedetail.IsDeleted == 0 && m.tblmachinedetail.IsNormalWC == 0).Select(m => m.tblmachinedetail.tblcell.CellID).Distinct().ToList();
            ViewBag.DistinctShops = ShopNames.OrderBy(m => m).ToList();
            // ViewBag.DistinctShopID = ShopID;

            //List<int> macCountshopwise = new List<int>();
            //foreach(int shop in ShopID)
            //{
            //    int MacCount = Serverdb.tblmachinedetails.Where(m => m.tblcell.CellID == shop && m.IsDeleted == 0 && m.IsNormalWC == 0).ToList().Count;
            //    macCountshopwise.Add(MacCount);
            //}
            // ViewBag.machinecountbyshop = macCountshopwise;
            //ViewBag.machinecountbyshopCount = macCountshopwise.Count;
            return View(tbllivemodeDT.ToList());
        }
    }
}