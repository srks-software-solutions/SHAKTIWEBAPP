using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SRKSDemo;
using SRKSDemo.Server_Model;

namespace UnitWorksCCS.Controllers
{
    public class DailyProdStatusController : Controller
    {
        // GET: DailyProdStatus
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
        string databaseName = ConfigurationManager.AppSettings["dbName"];
        public ActionResult Index()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            Session["CellId"] = 1;
            Session["colordata"] = null;
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            //calculating Corrected Date
            //TimeSpan currentHourMint = new TimeSpan(06, 59, 59);
            TimeSpan currentHourMint = new TimeSpan(05, 59, 59);
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
            //correctedDate = Convert.ToDateTime("2019-05-21");
            //CorrectedDate = "2019-05-21";
            //PrvCorrectedDate = "2019-05-20";
            // getting all machine details and their count.
            var macData = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0);
            int mc = macData.Count();
            ViewBag.macCount = mc;

            int[] macid = new int[mc];
            int macidlooper = 0;
            foreach (var v in macData)
            {
                macid[macidlooper++] = v.MachineID;
            }
            Session["macid"] = macid;
            ViewBag.macCount = mc;

            int[,] maindata = new int[mc, 16];
            //int[,] maindata = new int[mc, 6];
            // write a raw query to get sum of powerOff, Operating, Idle, BreakDown, PlannedMaintenance. 

            using (MsqlConnection mc1 = new MsqlConnection())
            {
                mc1.open();
                SqlCommand cmd1 = new SqlCommand("SELECT MachineID,sum(OperatingTime)as ot,sum(Delay) as dl,sum(Alarm)as al,sum(MDI) as mdi,sum(JOG) as jog,sum(HND) as hnd,sum(EDIT) as edit,sum(RTN) as rtn,sum(ETC) as etc,sum(PowerOff) as po FROM "+databaseName+".tblworkmodemimics where CorrectedDate='" + CorrectedDate + "'and MachineID IN (select distinct(MachineID) from "+databaseName+".tblmachinedetails where IsDeleted = 0 and IsNormalWC = 0) group by MachineID", mc1.msqlConnection);
                SqlDataReader datareader = cmd1.ExecuteReader();
                int maindatalooper1 = 0;

                while (datareader.Read())
                {
                    int maindatalooper2 = 0;
                    int OperatingTime = datareader.GetInt32(1);
                    int Delay = datareader.GetInt32(2);
                    int Alarm = datareader.GetInt32(3);
                    int MDI = datareader.GetInt32(4);
                    int JOG = datareader.GetInt32(5);
                    int HND = datareader.GetInt32(6);
                    int EDIT = datareader.GetInt32(7);
                    int RTN = datareader.GetInt32(8);
                    int ETC = datareader.GetInt32(9);
                    int PowerOff = datareader.GetInt32(10);
                    int TotalTime = OperatingTime + Delay + Alarm + MDI+JOG+HND+EDIT+RTN+ETC+PowerOff;
                    if (TotalTime == 0)
                    {
                        TotalTime = 1;
                    }
                    int UtilPer = Convert.ToInt32(Convert.ToDouble(Convert.ToDouble(Convert.ToDouble(OperatingTime) / Convert.ToDouble(TotalTime)) * 100));
                    maindata[maindatalooper1, maindatalooper2++] = datareader.GetInt32(0);
                    maindata[maindatalooper1, maindatalooper2++] = datareader.GetInt32(1);
                    maindata[maindatalooper1, maindatalooper2++] = datareader.GetInt32(2);
                    maindata[maindatalooper1, maindatalooper2++] = datareader.GetInt32(3);
                    maindata[maindatalooper1, maindatalooper2++] = datareader.GetInt32(4);
                    maindata[maindatalooper1, maindatalooper2++] = datareader.GetInt32(5);
                    maindata[maindatalooper1, maindatalooper2++] = datareader.GetInt32(6);
                    maindata[maindatalooper1, maindatalooper2++] = datareader.GetInt32(7);
                    maindata[maindatalooper1, maindatalooper2++] = datareader.GetInt32(8);
                    maindata[maindatalooper1, maindatalooper2++] = datareader.GetInt32(9);
                    maindata[maindatalooper1, maindatalooper2++] = datareader.GetInt32(10);
                    maindata[maindatalooper1, maindatalooper2++] = UtilPer;
                    maindata[maindatalooper1, maindatalooper2++] = TotalTime;
                    maindatalooper1++;
                }
                datareader.Close();
                SqlCommand Prvcmd1 = new SqlCommand("SELECT MachineID,sum(OperatingTime)as ot,sum(Delay) as dl,sum(Alarm)as al,sum(MDI) as mdi,sum(JOG) as jog,sum(HND) as hnd,sum(EDIT) as edit,sum(RTN) as rtn,sum(ETC) as etc,sum(PowerOff) as po FROM "+databaseName+".tblworkmodemimics where CorrectedDate='" + PrvCorrectedDate + "'and MachineID IN (select distinct(MachineID) from "+databaseName+".tblmachinedetails where IsDeleted = 0 and IsNormalWC = 0) group by MachineID", mc1.msqlConnection);
                SqlDataReader Prvdatareader = Prvcmd1.ExecuteReader();
                int Prvmaindatalooper1 = 0;
                while (Prvdatareader.Read())
                {
                    int OperatingTime = Prvdatareader.GetInt32(1);
                    int Delay = Prvdatareader.GetInt32(2);
                    int Alarm = Prvdatareader.GetInt32(3);
                    int MDI = Prvdatareader.GetInt32(4);
                    int JOG = Prvdatareader.GetInt32(5);
                    int HND = Prvdatareader.GetInt32(6);
                    int EDIT = Prvdatareader.GetInt32(7);
                    int RTN = Prvdatareader.GetInt32(8);
                    int ETC = Prvdatareader.GetInt32(9);
                    int PowerOff = Prvdatareader.GetInt32(10);
                    int TotalTime = OperatingTime + Delay + Alarm + MDI + JOG + HND + EDIT + RTN + ETC + PowerOff;
                    if (TotalTime == 0)
                    {
                        TotalTime = 1;
                    }
                    int UtilPer = Convert.ToInt32(Convert.ToDouble(Convert.ToDouble(Convert.ToDouble(OperatingTime) / Convert.ToDouble(TotalTime)) * 100));
                    maindata[Prvmaindatalooper1, 13] = UtilPer;
                    maindata[Prvmaindatalooper1, 14] = OperatingTime;
                    maindata[Prvmaindatalooper1, 15] = TotalTime;
                    Prvmaindatalooper1++;
                }
                Prvdatareader.Close();
                mc1.close();
            }
            Session["colordata"] = maindata;

            //Get Modes for All Machines for Today
            List<tbllivemode> tblModeDT = db.tbllivemodes.Where(m => m.CorrectedDate == correctedDate && m.tblmachinedetail.IsDeleted == 0 && m.tblmachinedetail.IsNormalWC == 0 && m.IsCompleted == 1 && m.IsDeleted==0).OrderBy(m => m.MachineID).ThenBy(m => m.StartTime).ToList();
            List<tbllivemode> tblModeDTCurr = db.tbllivemodes.Where(m => m.CorrectedDate == correctedDate && m.tblmachinedetail.IsDeleted == 0 && m.tblmachinedetail.IsNormalWC == 0 && m.IsCompleted == 0 && m.IsDeleted==0).OrderBy(m => m.MachineID).ThenByDescending(m => m.ModeID).ToList();
            //Get Latest Mode for each machine and Update the DurationInSec Column
            List<tbllivemode> CurrentModesOfAllMachines = (from row in tblModeDT
                                                       where row.IsCompleted == 0
                                                       orderby row.ModeID descending
                                                       select row).ToList().OrderByDescending(m => m.ModeID).ToList();
            int PrvMachineID = 0;
            foreach (var row in tblModeDTCurr)
            {
                if (PrvMachineID != row.MachineID)
                {
                    DateTime startDateTime = Convert.ToDateTime(row.StartTime);
                    int DurInSec = Convert.ToInt32(DateTime.Now.Subtract(startDateTime).TotalSeconds);
                    //row.DurationInSec = Convert.ToInt32( DateTime.Now.Subtract(startDateTime).TotalSeconds );
                    int ModeID = row.ModeID;
                    row.DurationInSec = DurInSec;
                    tblModeDT.Add(row);
                    //foreach (var tom in tblModeDT.Where(w => w.ModeID == ModeID))
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
                    tblModeDT.Add(row);
                }
            }
            //Update DurationInSec to Minutes
            foreach (var MainRow in tblModeDT.Where(m => m.DurationInSec > 0))
            {
                int GetDur = (int)MainRow.DurationInSec / 60;
                if (MainRow.ModeType == "SETUP")
                {
                    GetDur = (int)Convert.ToDateTime(MainRow.LossCodeEnteredTime).Subtract(Convert.ToDateTime(MainRow.StartTime)).TotalSeconds / 60;
                }
                if (GetDur < 1)
                {
                    // if the duration is sec is greatethan the 50 then consider as 1 minutes
                    int LessThan1min = (int)MainRow.DurationInSec;
                    if (LessThan1min > 50)
                    {
                        GetDur = 1;
                    }
                    else
                    {
                        GetDur = 0;
                    }
                }
                MainRow.DurationInSec = GetDur;
            };
            List<string> ShopNames = db.tbllivemodes.Where(m => m.CorrectedDate == correctedDate && m.tblmachinedetail.IsDeleted == 0 && m.tblmachinedetail.IsNormalWC == 0).Select(m => m.tblmachinedetail.tblcell.CelldisplayName).Distinct().ToList();
            ViewBag.DistinctShops = ShopNames;
            return View(tblModeDT.OrderBy(m => m.MachineID).ThenBy(m => m.StartTime).ToList());
        }
    }
}