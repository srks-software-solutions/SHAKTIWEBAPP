
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SRKSDemo.Server_Model;
using SRKSDemo;
using SRKSDemo.Models;
using System.Configuration;

namespace SRKSDemo.Controllers
{
    public class MachineStatusController : Controller
    {
        // GET: /AllMachineStatus/
        private i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
        string databaseName = ConfigurationManager.AppSettings["dbName"];
        public ActionResult Index()
        {
            //if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            //{
            //    return RedirectToAction("Login", "Login", null);
            //}

            Session["CellId"] = 1;
            Session["colordata"] = null;
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            //calculating Corrected Date
            TimeSpan currentHourMint = new TimeSpan(06, 59, 59);
            TimeSpan RealCurrntHour = System.DateTime.Now.TimeOfDay;
            string CorrectedDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
            DateTime correctedDate = DateTime.Now.Date;
            string PrvCorrectedDate = correctedDate.AddDays(-1).Date.ToString("yyyy-MM-dd");
            if (RealCurrntHour < currentHourMint)
            {
                CorrectedDate = DateTime.Now.AddDays(-1).Date.ToString("yyyy-MM-dd");
                correctedDate = DateTime.Now.AddDays(-1).Date;
                PrvCorrectedDate = correctedDate.AddDays(-1).Date.ToString("yyyy-MM-dd");
            }

            // getting all machine details and their count.
            var macData = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0);
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
                SqlCommand cmd1 = new SqlCommand("SELECT MachineID,sum(MachineOffTime) as op,sum(OperatingTime)as o,sum(IdleTime) as it,sum(BreakdownTime)as bt FROM "+databaseName+".tblmimics where CorrectedDate='" + CorrectedDate + "'and MachineID IN (select distinct(MachineID) from "+databaseName+".tblmachinedetails where IsDeleted = 0 and IsNormalWC = 0) group by MachineID", mc1.msqlConnection);
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
                    var cellname = db.tblmachinedetails.Where(m => m.MachineID == macstatus.MachineID).Select(m => m.tblcell.CelldisplayName).FirstOrDefault();
                    //if (cellname.Contains("\r\n"))
                    //{
                    //    cellname = cellname.Replace("\r\n", "");
                    //}
                    var machinedet = db.tblmachinedetails.Where(m => m.MachineID == macstatus.MachineID).FirstOrDefault();
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

                SqlCommand Prvcmd1 = new SqlCommand("SELECT MachineID,sum(MachineOffTime) as op,sum(OperatingTime)as o,sum(IdleTime) as it,sum(BreakdownTime)as bt FROM "+databaseName+".tblmimics where CorrectedDate='" + PrvCorrectedDate + "'and MachineID IN (select distinct(MachineID) from "+databaseName+".tblmachinedetails where IsDeleted = 0 and IsNormalWC = 0) group by MachineID", mc1.msqlConnection);
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
            List<tbllivemode> tbllivemodeDT = db.tbllivemodes.Where(m => m.CorrectedDate == correctedDate && m.tblmachinedetail.IsDeleted == 0 && m.tblmachinedetail.IsNormalWC == 0 && m.IsCompleted == 1).OrderBy(m => m.MachineID).ThenBy(m => m.StartTime).ToList();
            List<tbllivemode> tbllivemodeDTCurr = db.tbllivemodes.Where(m => m.CorrectedDate == correctedDate && m.tblmachinedetail.IsDeleted == 0 && m.tblmachinedetail.IsNormalWC == 0 && m.IsCompleted == 0).OrderBy(m => m.MachineID).ThenByDescending(m => m.ModeID).ToList();
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
                    GetDur = 1;
                }
                MainRow.DurationInSec = GetDur;
            };
            List<string> ShopNames = db.tbllivemodes.Where(m => m.CorrectedDate == correctedDate && m.tblmachinedetail.IsDeleted == 0 && m.tblmachinedetail.IsNormalWC == 0).OrderBy(m => m.tblmachinedetail.tblcell.CelldisplayName).Select(m => m.tblmachinedetail.tblcell.CelldisplayName).Distinct().ToList();
            //  List<int> ShopID = db.tbllivemodes.Where(m => m.CorrectedDate == correctedDate && m.tblmachinedetail.IsDeleted == 0 && m.tblmachinedetail.IsNormalWC == 0).Select(m => m.tblmachinedetail.tblcell.CellID).Distinct().ToList();
            ViewBag.DistinctShops = ShopNames.OrderBy(m => m).ToList();
            // ViewBag.DistinctShopID = ShopID;

            //List<int> macCountshopwise = new List<int>();
            //foreach(int shop in ShopID)
            //{
            //    int MacCount = db.tblmachinedetails.Where(m => m.tblcell.CellID == shop && m.IsDeleted == 0 && m.IsNormalWC == 0).ToList().Count;
            //    macCountshopwise.Add(MacCount);
            //}
            // ViewBag.machinecountbyshop = macCountshopwise;
            //ViewBag.machinecountbyshopCount = macCountshopwise.Count;
            return View(tbllivemodeDT.ToList());
        }
    }
}