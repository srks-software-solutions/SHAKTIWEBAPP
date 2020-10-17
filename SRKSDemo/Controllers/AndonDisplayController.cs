using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Newtonsoft.Json;
using System.Data.SqlClient;

using System.IO;
using SRKSDemo;
using SRKSDemo.App_Start;
using SRKSDemo.Server_Model;
using System.Configuration;
using SRKSDemo.Models;

namespace SRKSDemo.Controllers
{
    public class AndonDisplayController : Controller
    {
        private i_facility_shaktiEntities1 db= new i_facility_shaktiEntities1();

       string databaseName=ConfigurationManager.AppSettings["dbName"];
        // GET: AndonDisplay
        public ActionResult MachineStatus(int CellID = 0)
        {
            if (CellID == 0)
            {
                //GetMode GM = new GetMode();
                ////String IPAddress = GM.GetIPAddressofTabSystem();
                //String IPAddress = GM.GetIPAddressofAndon();

                //CellID = Convert.ToInt32(db.tblAndonDispDets.Where(m => m.IPAddress == IPAddress && m.IsDeleted == 0).Select(m => m.CellID).FirstOrDefault());
            }
            Session["CellId"] = CellID;
            Session["MachineID"] = CellID;
            ViewBag.CellID = CellID;
            Session["colordata"] = null;
            //ViewBag.Logout = Session["Username"];
            //ViewBag.roleid = Session["RoleID"];

            //calculating Corrected Date
            TimeSpan currentHourMint = new TimeSpan(07, 00, 00);
            TimeSpan RealCurrntHour = System.DateTime.Now.TimeOfDay;
            string CorrectedDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
            DateTime correctedDate = DateTime.Now.Date;
            String PrvCorrectedDate = correctedDate.AddDays(-1).Date.ToString("yyyy-MM-dd");
            if (RealCurrntHour < currentHourMint)
            {
                CorrectedDate = DateTime.Now.AddDays(-1).Date.ToString("yyyy-MM-dd");
                correctedDate = DateTime.Now.AddDays(-1).Date;
                PrvCorrectedDate = correctedDate.AddDays(-1).Date.ToString("yyyy-MM-dd");
            }

            // getting all machine details and their count.
            var macData = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.CellID == CellID).OrderBy(m => m.CellOrderNo);
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
            DataTable dt = new DataTable();
            using (MsqlConnection mc1 = new MsqlConnection())
            {
                //string cmd1 = "SELECT MachineID, sum(MachineOffTime) as op,sum(OperatingTime) as o,sum(IdleTime) as it,sum(BreakdownTime) as bt FROM "+databaseName+ ".tblmimics where CorrectedDate = '" + CorrectedDate + "'and MachineID IN(select distinct(MachineID) from "+databaseName+ ".tblmachinedetails where IsDeleted = 0 and IsNormalWC = 0 and CellID = " + CellID + ") group by MachineID";
                string cmd1 = "SELECT distinct(tmi.MachineID),tmi.MachineOffTime,tmi.OperatingTime,tmi.IdleTime,tmi.BreakdownTime,tm.CellOrderNo,tmi.SetupTime FROM "+databaseName+ ".tblmimics tmi left outer join " + databaseName + ".tblmachinedetails tm on tmi.MachineID = tm.MachineID where tmi.CorrectedDate = '" + correctedDate.ToString("yyyy-MM-dd") + "' and tm.IsDeleted = 0 and tm.IsNormalWC = 0 and tm.CellID = " + CellID + " order by tm.CellOrderNo";
                SqlDataAdapter daGetUpdate = new SqlDataAdapter(cmd1, mc1.msqlConnection);
                mc1.open();
                daGetUpdate.Fill(dt);
                mc1.close();

            }

            List<Models.MachineStatus> objmachstat = new List<Models.MachineStatus>();
            int countUpdate = dt.Rows.Count;

            if (countUpdate > 0)
            {
                int maindatalooper1 = 0;
                for (int k = 0; k < countUpdate; k++)
                {
                    try
                    {
                        MachineStatus macstatus = new MachineStatus();
                        int maindatalooper2 = 0;
                        int MachineOffTime = Convert.ToInt32(dt.Rows[k][1]);
                        int OpertTime = Convert.ToInt32(dt.Rows[k][2]);
                        int IdleTime = Convert.ToInt32(dt.Rows[k][3]) + Convert.ToInt32(dt.Rows[k][6]);
                        int BDTime = Convert.ToInt32(dt.Rows[k][4]);
                        int TotalTime = MachineOffTime + OpertTime + IdleTime + BDTime;
                        if (TotalTime == 0)
                        {
                            TotalTime = 1;
                        }
                        int UtilPer = Convert.ToInt32(Convert.ToDouble(Convert.ToDouble(Convert.ToDouble(OpertTime) / Convert.ToDouble(TotalTime)) * 100));

                        macstatus.MachineID = Convert.ToInt32(dt.Rows[k][0]);
                        var cellname = db.tblmachinedetails.Where(m => m.MachineID == macstatus.MachineID).Select(m => m.tblcell.CelldisplayName).FirstOrDefault();
                        var machinedet = db.tblmachinedetails.Where(m => m.MachineID == macstatus.MachineID).FirstOrDefault();
                        macstatus.CellName = cellname;
                        macstatus.MachineOffTime = Convert.ToInt32(dt.Rows[k][1]);
                        macstatus.OperatingTime = Convert.ToInt32(dt.Rows[k][2]);
                        macstatus.IdleTime = IdleTime;
                        macstatus.BreakdownTime = BDTime;
                        macstatus.Utilization = UtilPer;
                        macstatus.TotalTime = TotalTime;
                        macstatus.machinedet = machinedet;
                        //maindata[maindatalooper1, maindatalooper2++] = Convert.ToInt32(dt.Rows[k][0]);
                        //maindata[maindatalooper1, maindatalooper2++] = Convert.ToInt32(dt.Rows[k][1]);
                        //maindata[maindatalooper1, maindatalooper2++] = Convert.ToInt32(dt.Rows[k][2]);
                        //maindata[maindatalooper1, maindatalooper2++] = Convert.ToInt32(dt.Rows[k][3]) + Convert.ToInt32(dt.Rows[k][6]);
                        //maindata[maindatalooper1, maindatalooper2++] = Convert.ToInt32(dt.Rows[k][4]);
                        //maindata[maindatalooper1, maindatalooper2++] = UtilPer;
                        //maindata[maindatalooper1, maindatalooper2++] = TotalTime;
                        //maindatalooper1++;

                        objmachstat.Add(macstatus);
                    }
                    catch (Exception e)
                    {
                    }
                }

            }

            DataTable dt1 = new DataTable();
            using (MsqlConnection mc1 = new MsqlConnection())
            {

                string cmd1 = "SELECT distinct(tmi.MachineID),tmi.MachineOffTime,tmi.OperatingTime,tmi.IdleTime,tmi.BreakdownTime,tm.CellOrderNo,tmi.SetupTime FROM "+databaseName+ ".tblmimics tmi left outer join "+databaseName+ ".tblmachinedetails tm on tmi.MachineID = tm.MachineID where tmi.CorrectedDate = '" + PrvCorrectedDate + "' and tm.IsDeleted = 0 and tm.IsNormalWC = 0 and tm.CellID = " + CellID + " order by tm.CellOrderNo";
                SqlDataAdapter daGetUpdate = new SqlDataAdapter(cmd1, mc1.msqlConnection);
                mc1.open();
                daGetUpdate.Fill(dt1);
                mc1.close();
            }
            int countUpdatepre = dt1.Rows.Count;
            if (countUpdatepre > 0)
            {
                int Prvmaindatalooper1 = 0;
                for (int k = 0; k < countUpdate; k++)
                {
                    try
                    {
                        int MachineOffTime = Convert.ToInt32(dt1.Rows[k][1]);
                        int OpertTime = Convert.ToInt32(dt1.Rows[k][2]);
                        int IdleTime = Convert.ToInt32(dt1.Rows[k][3]) + Convert.ToInt32(dt1.Rows[k][6]);
                        int BDTime = Convert.ToInt32(dt1.Rows[k][4]);
                        int TotalTime = MachineOffTime + OpertTime + IdleTime + BDTime;
                        if (TotalTime == 0)
                        {
                            TotalTime = 1;
                        }
                        int UtilPer = Convert.ToInt32(Convert.ToDouble(Convert.ToDouble(Convert.ToDouble(OpertTime) / Convert.ToDouble(TotalTime)) * 100));
                        //maindata[Prvmaindatalooper1, 7] = UtilPer;
                        //maindata[Prvmaindatalooper1, 8] = OpertTime;
                        //maindata[Prvmaindatalooper1, 9] = TotalTime;
                        //Prvmaindatalooper1++;
                        int MachineID = Convert.ToInt32(dt1.Rows[k][0]);
                        foreach (var row in objmachstat)
                        {
                            if (row.MachineID == MachineID)
                            {
                                row.PrevOperatingTime = OpertTime;
                                row.PrevTotalTime = TotalTime;
                                row.PrevUtilization = UtilPer;
                                objmachstatUpdate.Add(row);

                            }

                        }
                    }
                    catch (Exception e)
                    {
                    }
                }

            }


            //Session["colordata"] = maindata;
            objmachstatUpdate = objmachstatUpdate.OrderBy(m => m.CellName).ToList();
            Session["MachineStauts"] = objmachstatUpdate.OrderBy(m => m.CellName).ToList();

            //Get Modes for All Machines for Today
            List<tbllivemode> tblModeDT = db.tbllivemodes.Where(m => m.CorrectedDate == correctedDate && m.tblmachinedetail.IsDeleted == 0 && m.tblmachinedetail.IsNormalWC == 0 && m.tblmachinedetail.CellID == CellID && m.IsCompleted == 1 && m.ModeTypeEnd == 1).OrderBy(m => m.tblmachinedetail.CellOrderNo).ThenBy(m => m.StartTime).ToList();
            List<tbllivemode> tblModeDTCurr = db.tbllivemodes.Where(m => m.CorrectedDate == correctedDate && m.tblmachinedetail.IsDeleted == 0 && m.tblmachinedetail.IsNormalWC == 0 && m.tblmachinedetail.CellID == CellID && (m.IsCompleted == 0 || (m.IsCompleted == 1 && m.ModeTypeEnd == 0))).OrderBy(m => m.tblmachinedetail.CellOrderNo).ThenByDescending(m => m.ModeID).ToList();
            //Get Latest Mode for each machine and Update the DurationInSec Column
            List<tbllivemode> CurrentModesOfAllMachines = (from row in tblModeDT
                                                       where row.IsCompleted == 0 || (row.IsCompleted == 1 && row.ModeTypeEnd == 0)
                                                       orderby row.ModeID descending
                                                       select row).ToList().OrderByDescending(m => m.ModeID).ToList();
            int PrvMachineID = 0;
            foreach (var row in tblModeDTCurr)
            {
                if (PrvMachineID != row.MachineID)
                {
                    DateTime startDateTime = Convert.ToDateTime(row.StartTime);
                    int DurInSec = Convert.ToInt32(DateTime.Now.Subtract(startDateTime).TotalSeconds);
                    int ModeID = row.ModeID;
                    row.DurationInSec = DurInSec;
                    tblModeDT.Add(row);
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
                //GetDur = (int)MainRow.DurationInSec / 60;
                if (GetDur < 1)
                {
                    GetDur = 0;
                }
                MainRow.DurationInSec = GetDur;
            };
            List<string> ShopNames = db.tblmachinedetails.Where(m => m.CellID == CellID && m.IsDeleted == 0).Select(m => m.tblcell.CelldisplayName).Distinct().ToList();
            ViewBag.DistinctShops = ShopNames;

            List<tbllivemode> MainTbl = tblModeDT.OrderBy(m => m.tblmachinedetail.CellOrderNo).ThenBy(m => m.StartTime).ToList();

            return View(MainTbl);
        }

        public ActionResult NewDashboard2911(int CellID = 0)
        {
            //if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            //{
            //    return RedirectToAction("Login", "Login", null);
            //}
            if (CellID == 0)
            {
                GetMode GM = new GetMode();
                //String IPAddress = GM.GetIPAddressofTabSystem();
                String IPAddress = GM.GetIPAddressofAndon();

                CellID = Convert.ToInt32(db.tblAndonDispDets.Where(m => m.IPAddress == IPAddress && m.IsDeleted == 0).Select(m => m.CellID).FirstOrDefault());
            }
            Session["CellId"] = CellID;
            return View();
        }

        public ActionResult ProdDisplay(int CellID = 0)
        {
            if (CellID == 0)
            {
                GetMode GM = new GetMode();
                //String IPAddress = GM.GetIPAddressofTabSystem();
                String IPAddress = GM.GetIPAddressofAndon();

                CellID = Convert.ToInt32(db.tblAndonDispDets.Where(m => m.IPAddress == IPAddress && m.IsDeleted == 0).Select(m => m.CellID).FirstOrDefault());
            }
            Session["CellId"] = CellID;
            Session["MachineID"] = CellID;
            ViewBag.CellID = CellID;

            var CellName = db.tblcells.Where(m => m.CellID == CellID && m.IsDeleted == 0).FirstOrDefault();

            //ViewBag.CellName = CellName.tblplant.PlantDisplayName + " --> " + CellName.tblshop.Shopdisplayname + " --> " + CellName.CelldisplayName;

            ViewBag.CellName = "Shakti";

            //calculating Corrected Date
            TimeSpan currentHourMint = new TimeSpan(07, 00, 00);
            TimeSpan RealCurrntHour = System.DateTime.Now.TimeOfDay;
            string CorrectedDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
            DateTime correctedDate = DateTime.Now.Date;
            //DateTime correctedDate = Convert.ToDateTime("2020-07-13");
            String PrvCorrectedDate = correctedDate.AddDays(-1).Date.ToString("yyyy-MM-dd");
            if (RealCurrntHour < currentHourMint)
            {
                CorrectedDate = DateTime.Now.AddDays(-1).Date.ToString("yyyy-MM-dd");
                correctedDate = DateTime.Now.AddDays(-1).Date;
                PrvCorrectedDate = correctedDate.AddDays(-1).Date.ToString("yyyy-MM-dd");
            }

            // getting all machine details and their count.
            var macData = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 ).OrderBy(m => m.CellOrderNo).ToList();
            //int mc = macData.Count();
            //int machineid;
            
            ReportsCalcClass.ProdDetAndon PD = new ReportsCalcClass.ProdDetAndon();
            foreach (var MacDet in macData)
            {
                try
                {
                    PD.insertProdDet(MacDet.MachineID, correctedDate);
                }
                catch (Exception e)
                {
                }
            }
            var mc = db.tbl_ProdAndonDisp.Where(m => m.CorrectedDate == correctedDate.Date).Select(m => m.MachineID).Distinct().ToList();
            ViewBag.macCount = mc.Count;
            int[] macid = new int[mc.Count];
            int macidlooper = 0;
            foreach (var v in mc)
            {
                macid[macidlooper++] = v;
            }
            Session["macid"] = macid;
            ViewBag.macCount = mc.Count;

            //var GetProdDetList = db.tbl_ProdAndonDisp.Where(m => m.tblmachinedetail.CellID == CellID && m.CorrectedDate == correctedDate.Date).OrderBy(m => m.tblworkorderentry.HMIID).ToList();
            var GetProdDetList = db.tbl_ProdAndonDisp.Where(m => m.CorrectedDate == correctedDate.Date).ToList();

            return View(GetProdDetList);
        }

        public ActionResult ImageDisplay(int CellId = 0)
        {
            Session["CellId"] = CellId;
            DateTime getCurrentDateToDisplay = DateTime.Now;
            if (CellId != 0)
            {
                var dbItemToSchedule = db.tblAndonImageTextScheduledDisplays.Where(m => m.CellID == CellId && m.FlagStart == 1 && m.FlagEnd == 0 && m.IsDeleted == 0).OrderByDescending(m => m.StartDateTime).FirstOrDefault();
                if (dbItemToSchedule != null)
                {

                    TempData["ImageToDisplay"] = dbItemToSchedule.ImageName;
                }
                else
                {
                    TempData["ImageToDisplay"] = "";
                }
                return View(dbItemToSchedule);
            }
            else
            {
                return View();
            }
        }

        public ActionResult TextDisplay(int CellId = 0)
        {
            Session["CellId"] = CellId;
            DateTime getCurrentDateToDisplay = DateTime.Now;
            if (CellId != 0)
            {
                var dbItemToSchedule = db.tblAndonImageTextScheduledDisplays.Where(m => m.CellID == CellId && m.FlagStart == 1 && m.FlagEnd == 0 && m.IsDeleted == 0).OrderByDescending(m => m.StartDateTime).FirstOrDefault();
                if (dbItemToSchedule != null)
                {

                    TempData["TextToDisplay"] = dbItemToSchedule.TextToDisplay;
                }
                else
                {
                    TempData["TextToDisplay"] = "";
                }
                return View(dbItemToSchedule);
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        //public string CookiePageRedirector(string pageStatus, string cellId)
        //{
        //    UpdateAndonStart(Convert.ToInt32(cellId));
        //    string nextURLAndPageStatus = "";

        //    //pageStatus = "1,1,0,0-1"; -- for hardcode testing
        //    string nextUrl = "", caseStatus = "", insideCaseStatus = "";
        //    if (pageStatus == null || pageStatus == "")
        //    {
        //        pageStatus = "1,1,0,0-1";
        //    }
        //    string[] arryStatus = pageStatus.Split('-');
        //    caseStatus = arryStatus[0];
        //    insideCaseStatus = arryStatus[1];

        //    string screen1Url = "/AndonDisplay/MachineStatus?CellID=" + cellId + "";
        //    string screen2Url = "/AndonDisplay/MachineStatus?CellID=" + cellId + "";
        //    string screen3Url = "/AndonDisplay/ImageDisplay?CellID=" + cellId + "";
        //    string screen4Url = "/AndonDisplay/TextDisplay?CellID=" + cellId + "";

        //    switch (caseStatus)
        //    {
        //        //order will be in s1,s2,s3,s4 - (in which screen it is)
        //        //case 1 
        //        case "0,0,0,1":
        //            switch (insideCaseStatus)
        //            {
        //                case "4":
        //                    nextUrl = screen4Url;
        //                    pageStatus = caseStatus + "-4";
        //                    break;
        //                default:
        //                    nextUrl = screen4Url;
        //                    pageStatus = caseStatus + "-4";
        //                    break;
        //            }
        //            break;

        //        case "0,0,1,0":
        //            switch (insideCaseStatus)
        //            {
        //                case "3":
        //                    nextUrl = screen3Url;
        //                    pageStatus = caseStatus + "-3";
        //                    break;
        //                default:
        //                    nextUrl = screen3Url;
        //                    pageStatus = caseStatus + "-3";
        //                    break;
        //            }
        //            break;

        //        case "0,0,1,1":
        //            switch (insideCaseStatus)
        //            {
        //                case "3":
        //                    nextUrl = screen4Url;
        //                    pageStatus = caseStatus + "-4";
        //                    break;
        //                case "4":
        //                    nextUrl = screen3Url;
        //                    pageStatus = caseStatus + "-3";
        //                    break;
        //                default:
        //                    nextUrl = screen3Url;
        //                    pageStatus = caseStatus + "-3";
        //                    break;
        //            }
        //            break;

        //        case "1,1,0,0":
        //            switch (insideCaseStatus)
        //            {
        //                case "1":
        //                    nextUrl = screen2Url;
        //                    pageStatus = caseStatus + "-2";
        //                    break;
        //                case "2":
        //                    nextUrl = screen1Url;
        //                    pageStatus = caseStatus + "-1";
        //                    break;
        //                default:
        //                    nextUrl = screen1Url;
        //                    pageStatus = caseStatus + "-1";
        //                    break;
        //            }
        //            break;

        //        case "1,1,0,1":
        //            switch (insideCaseStatus)
        //            {
        //                case "1":
        //                    nextUrl = screen2Url;
        //                    pageStatus = caseStatus + "-2";
        //                    break;
        //                case "2":
        //                    nextUrl = screen4Url;
        //                    pageStatus = caseStatus + "-4";
        //                    break;
        //                case "4":
        //                    nextUrl = screen1Url;
        //                    pageStatus = caseStatus + "-1";
        //                    break;
        //                default:
        //                    nextUrl = screen1Url;
        //                    pageStatus = caseStatus + "-1";
        //                    break;
        //            }
        //            break;

        //        case "1,1,1,0":
        //            switch (insideCaseStatus)
        //            {
        //                case "1":
        //                    nextUrl = screen2Url;
        //                    pageStatus = caseStatus + "-2";
        //                    break;
        //                case "2":
        //                    nextUrl = screen3Url;
        //                    pageStatus = caseStatus + "-3";
        //                    break;
        //                case "3":
        //                    nextUrl = screen1Url;
        //                    pageStatus = caseStatus + "-1";
        //                    break;
        //                default:
        //                    nextUrl = screen1Url;
        //                    pageStatus = caseStatus + "-1";
        //                    break;
        //            }
        //            break;

        //        case "1,1,1,1":
        //            switch (insideCaseStatus)
        //            {
        //                case "1":
        //                    nextUrl = screen2Url;
        //                    pageStatus = caseStatus + "-2";
        //                    break;
        //                case "2":
        //                    nextUrl = screen3Url;
        //                    pageStatus = caseStatus + "-3";
        //                    break;
        //                case "3":
        //                    nextUrl = screen4Url;
        //                    pageStatus = caseStatus + "-4";
        //                    break;
        //                case "4":
        //                    nextUrl = screen1Url;
        //                    pageStatus = caseStatus + "-1";
        //                    break;
        //                default:
        //                    nextUrl = screen1Url;
        //                    pageStatus = caseStatus + "-1";
        //                    break;
        //            }
        //            break;

        //        default:
        //            switch (insideCaseStatus)
        //            {
        //                case "1":
        //                    nextUrl = screen2Url;
        //                    pageStatus = caseStatus + "-2";
        //                    break;
        //                case "2":
        //                    nextUrl = screen4Url;
        //                    pageStatus = caseStatus + "-4";
        //                    break;
        //                case "4":
        //                    nextUrl = screen1Url;
        //                    pageStatus = caseStatus + "-1";
        //                    break;
        //                default:
        //                    nextUrl = screen1Url;
        //                    pageStatus = caseStatus + "-1";
        //                    break;
        //            }
        //            break;
        //    }

        //    nextURLAndPageStatus = nextUrl + "%" + pageStatus;

        //    return nextURLAndPageStatus;
        //}

        public ActionResult ImageText()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            var dbItem = db.tblAndonImageTextScheduledDisplays.Where(m => m.IsDeleted == 0).OrderByDescending(m => m.StartDateTime).ToList();
            return View(dbItem);

        }

        public ActionResult ImageTextMaster()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.plant = new SelectList(db.tblplants.Where(p => p.IsDeleted == 0), "PlantID", "PlantDisplayName");
            ViewBag.dept = new SelectList(db.tblshops.Where(d => d.IsDeleted == -1), "ShopId", "ShopDisplayName");
            ViewBag.cell = new SelectList(db.tblcells.Where(m => m.IsDeleted == -1), "CellId", "CellDisplayName");

            return View();
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult ImageTextMaster(tblAndonImageTextScheduledDisplay viewItem, HttpPostedFileBase[] imageFile)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.plant = new SelectList(db.tblplants.Where(p => p.IsDeleted == 0), "PlantID", "PlantDisplayName");
            ViewBag.dept = new SelectList(db.tblshops.Where(d => d.IsDeleted == -1), "ShopId", "ShopDisplayName");
            ViewBag.cell = new SelectList(db.tblcells.Where(m => m.IsDeleted == -1), "CellId", "CellDisplayName");

            GetMode GM = new GetMode();
            String IPAddress = GM.GetIPAddressofAndon();

            string screenType = "1,1,0,0-1";
            string imageNameFullName = "";
            if (viewItem.TextToDisplay != null && imageFile[0] != null && viewItem.DefaultScreenVisible == 1)
            {
                screenType = "1,1,1,1-1";
            }
            else if (viewItem.TextToDisplay != null && imageFile[0] != null && viewItem.DefaultScreenVisible == 0)
            {
                screenType = "0,0,1,1-1";
            }
            else if (viewItem.TextToDisplay == null && imageFile[0] != null && viewItem.DefaultScreenVisible == 0)
            {
                screenType = "0,0,1,0-1";
            }
            else if (viewItem.TextToDisplay != null && imageFile[0] == null && viewItem.DefaultScreenVisible == 0)
            {
                screenType = "0,0,0,1-1";
            }
            else if (viewItem.TextToDisplay != null && imageFile[0] == null && viewItem.DefaultScreenVisible == 1)
            {
                screenType = "1,1,0,1-1";
            }
            else if (viewItem.TextToDisplay == null && imageFile[0] != null && viewItem.DefaultScreenVisible == 1)
            {
                screenType = "1,1,1,0-1";
            }
            int i = 0;
            foreach (HttpPostedFileBase img in imageFile)
            {
                if (i == 3)//restricting to save only 3 images
                {
                    break;
                }
                string imageName = "";
                if (img != null)
                {
                    string fileExtension = Path.GetExtension(img.FileName);
                    Utility uObj = new Utility();
                    imageName = uObj.GUIDGenerator() + fileExtension;
                    bool upload = uObj.SaveImage(img, imageName);
                }
                else
                {
                    imageName = "";
                }
                if (imageNameFullName == "")
                {
                    imageNameFullName = imageName;
                }
                else
                {
                    imageNameFullName = imageNameFullName + "#" + imageName;
                }

                i++;
            }

            tblAndonImageTextScheduledDisplay dbItem = new tblAndonImageTextScheduledDisplay();

            dbItem.IPAddress = IPAddress;
            dbItem.PlantID = viewItem.PlantID;
            dbItem.ShopID = viewItem.ShopID;
            dbItem.CellID = viewItem.CellID;
            dbItem.ScreenType = screenType;
            dbItem.FlagEnd = 0;
            dbItem.FlagStart = 0;
            dbItem.StartDateTime = viewItem.StartDateTime;
            dbItem.EndDateTime = viewItem.EndDateTime;
            if (imageNameFullName != "")
            {
                dbItem.ImageName = imageNameFullName;
            }
            dbItem.TextToDisplay = viewItem.TextToDisplay;
            dbItem.DefaultScreenVisible = viewItem.DefaultScreenVisible;
            dbItem.IsDeleted = 0;
            dbItem.InsertedBy = 1;
            dbItem.InsertedOn = DateTime.Now;
            db.tblAndonImageTextScheduledDisplays.Add(dbItem);
            db.SaveChanges();

            return Redirect("/AndonDisplay/ImageText");
        }

        public ActionResult EditImageTextMaster(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            var dataItem = db.tblAndonImageTextScheduledDisplays.Where(m => m.TextImageAndonId == id).FirstOrDefault();
            ViewBag.plant = new SelectList(db.tblplants.Where(p => p.IsDeleted == 0), "PlantID", "PlantDisplayName", dataItem.PlantID);
            ViewBag.dept = new SelectList(db.tblshops.Where(d => d.IsDeleted == 0), "ShopId", "ShopDisplayName", dataItem.ShopID);
            ViewBag.cell = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0), "CellId", "CellDisplayName", dataItem.CellID);
            ViewBag.AndonImages = dataItem.ImageName;
            ViewBag.AndonText = dataItem.TextToDisplay;
            ViewBag.FlagStart = dataItem.FlagStart.ToString();
            return View(dataItem);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditImageTextMaster(tblAndonImageTextScheduledDisplay viewItem, HttpPostedFileBase[] imageFile)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.plant = new SelectList(db.tblplants.Where(p => p.IsDeleted == 0), "PlantID", "PlantDisplayName");
            ViewBag.dept = new SelectList(db.tblshops.Where(d => d.IsDeleted == -1), "ShopId", "ShopDisplayName");
            ViewBag.cell = new SelectList(db.tblcells.Where(m => m.IsDeleted == -1), "CellId", "CellDisplayName");

            GetMode GM = new GetMode();
            String IPAddress = GM.GetIPAddressofAndon();
            if (viewItem.TextImageAndonId != 0)
            {
                var dbItem = db.tblAndonImageTextScheduledDisplays.Where(m => m.TextImageAndonId == viewItem.TextImageAndonId).FirstOrDefault();
                string screenType = "1,1,0,0-1";
                string imageNameFullName = "";
                string imageName = dbItem.ImageName;
                if (viewItem.IPAddress == "1")
                {
                    if (viewItem.TextToDisplay != null && imageFile[0] != null && viewItem.DefaultScreenVisible == 1)
                    {
                        screenType = "1,1,1,1-1";
                    }
                    else if (viewItem.TextToDisplay != null && imageFile[0] != null && viewItem.DefaultScreenVisible == 0)
                    {
                        screenType = "0,0,1,1-1";
                    }
                    else if (viewItem.TextToDisplay == null && imageFile[0] != null && viewItem.DefaultScreenVisible == 0)
                    {
                        screenType = "0,0,1,0-1";
                    }
                    else if (viewItem.TextToDisplay != null && imageFile[0] == null && viewItem.DefaultScreenVisible == 0)
                    {
                        screenType = "0,0,0,1-1";
                    }
                    else if (viewItem.TextToDisplay != null && imageFile[0] == null && viewItem.DefaultScreenVisible == 1)
                    {
                        screenType = "1,1,0,1-1";
                    }
                    else if (viewItem.TextToDisplay == null && imageFile[0] != null && viewItem.DefaultScreenVisible == 1)
                    {
                        screenType = "1,1,1,0-1";
                    }



                    Utility uObj = new Utility();
                    //delete old image
                    if (imageName != null)
                    {

                        string[] arryName = imageName.Split('#');
                        if (arryName.Count() != 0)
                        {
                            foreach (var item in arryName)
                            {
                                if (item != null || item != "")
                                {
                                    uObj.deleteOldImage(item);
                                }
                            }
                        }
                    }
                    int imgNo = 0;
                    //add new image
                    foreach (HttpPostedFileBase img in imageFile)
                    {
                        if (img != null)
                        {


                            if (imgNo == 3)//restricting to save only 3 images
                            {
                                break;
                            }
                            imageName = "";
                            if (img != null)
                            {
                                string fileExtension = Path.GetExtension(img.FileName);
                                imageName = uObj.GUIDGenerator() + fileExtension;
                                bool upload = uObj.SaveImage(img, imageName);
                            }
                            else
                            {
                                imageName = "";
                            }
                            if (imageNameFullName == "")
                            {
                                imageNameFullName = imageName;
                            }
                            else
                            {
                                imageNameFullName = imageNameFullName + "#" + imageName;
                            }
                            imgNo++;
                        }
                    }

                }
                else
                {
                    if (viewItem.TextToDisplay != null && imageName != null && viewItem.DefaultScreenVisible == 1)
                    {
                        screenType = "1,1,1,1-1";
                    }
                    else if (viewItem.TextToDisplay != null && imageName != null && viewItem.DefaultScreenVisible == 0)
                    {
                        screenType = "0,0,1,1-1";
                    }
                    else if (viewItem.TextToDisplay == null && imageName != null && viewItem.DefaultScreenVisible == 0)
                    {
                        screenType = "0,0,1,0-1";
                    }
                    else if (viewItem.TextToDisplay != null && imageName == null && viewItem.DefaultScreenVisible == 0)
                    {
                        screenType = "0,0,0,1-1";
                    }
                    else if (viewItem.TextToDisplay != null && imageName == null && viewItem.DefaultScreenVisible == 1)
                    {
                        screenType = "1,1,0,1-1";
                    }
                    else if (viewItem.TextToDisplay == null && imageName != null && viewItem.DefaultScreenVisible == 1)
                    {
                        screenType = "1,1,1,0-1";
                    }
                }

                dbItem.IPAddress = IPAddress;
                dbItem.PlantID = viewItem.PlantID;
                dbItem.ShopID = viewItem.ShopID;
                dbItem.CellID = viewItem.CellID;
                dbItem.ScreenType = screenType;
                //dbItem.FlagEnd = 0;
                //dbItem.FlagStart = 0;
                dbItem.StartDateTime = viewItem.StartDateTime;
                dbItem.EndDateTime = viewItem.EndDateTime;
                if (imageNameFullName != "")
                {
                    dbItem.ImageName = imageNameFullName;
                }
                dbItem.TextToDisplay = viewItem.TextToDisplay;
                dbItem.DefaultScreenVisible = viewItem.DefaultScreenVisible;
                dbItem.IsDeleted = 0;
                dbItem.ModifiedBy = 1;
                dbItem.ModifiedOn = DateTime.Now;
                db.Entry(dbItem).State = EntityState.Modified;
                db.SaveChanges();
                //db.SaveChanges();
                TempData["toaster_success"] = "Item Updated successfully";
            }
            else
            {
                TempData["toaster_error"] = "Something error occured";
            }

            return Redirect("/AndonDisplay/ImageText");
        }

        public ActionResult DeleteImageText(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            try
            {
                var deleteItem = db.tblAndonImageTextScheduledDisplays.Where(m => m.TextImageAndonId == id).FirstOrDefault();
                deleteItem.FlagStart = 1;
                deleteItem.FlagEnd = 1;
                deleteItem.IsDeleted = 1;
                db.SaveChanges();
                Utility uObj = new Utility();
                string imageName = deleteItem.ImageName;
                if (imageName != null && imageName!="")
                {
                    string[] arryName = imageName.Split('#');
                    if (arryName.Count() != 0)
                    {
                        foreach (var item in arryName)
                        {
                            if (item != null || item != "")
                            {
                                uObj.deleteOldImage(item);
                            }
                        }
                    }
               
                }
                TempData["toaster_success"] = "Item deleted successfully";
            }
            catch (Exception e)
            {
                TempData["toaster_error"] = "Something error occured";
            }
            return Redirect("/AndonDisplay/ImageText");
        }

        public ActionResult EndFlag(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            try
            {
                var updateItem = db.tblAndonImageTextScheduledDisplays.Where(m => m.TextImageAndonId == id).FirstOrDefault();
                updateItem.FlagStart = 1;
                updateItem.FlagEnd = 1;
                updateItem.ModifiedOn = System.DateTime.Now;
                db.SaveChanges();

                Utility uObj = new Utility();
                string imageName = updateItem.ImageName;
                string[] arryName = imageName.Split('#');
                if (arryName.Count() != 0)
                {
                    foreach (var item in arryName)
                    {
                        if (item != null || item != "")
                        {
                            uObj.deleteOldImage(item);
                        }
                    }
                }

                TempData["toaster_success"] = "Schedule stoped successfully";
            }
            catch (Exception e)
            {
                TempData["toaster_error"] = "Something error occured";
            }
            return Redirect("/AndonDisplay/ImageText");
        }

        [HttpPost]
        public string GetStatus(int CellId = 0, int Page = 0)
        {

            string pg = "1";
            switch (Page)
            {
                case 1:
                    pg = "1";
                    break;
                case 2:
                    pg = "2";
                    break;
                case 3:
                    pg = "3";
                    break;
                case 4:
                    pg = "4";
                    break;
                default:
                    pg = "1";
                    break;
            }
            string screenType = "1,1,0,0-" + pg;
            DateTime getCurrentDateToDisplay = DateTime.Now;
            string imageName = null, txtToDisplay = null;
            int defaultScreen = 0;
            if (CellId != 0)
            {
                var dbItemToSchedule = db.tblAndonImageTextScheduledDisplays.Where(m => m.CellID == CellId && m.FlagStart == 1 && m.FlagEnd == 0 && m.IsDeleted == 0).OrderByDescending(m => m.StartDateTime).FirstOrDefault();
                if (dbItemToSchedule != null)
                {
                    #region //get data from data base
                    if (dbItemToSchedule.ImageName != null)
                    {
                        if (dbItemToSchedule.ImageName != "")
                        {
                            imageName = dbItemToSchedule.ImageName;
                        }
                    }

                    if (dbItemToSchedule.TextToDisplay != null)
                    {
                        if (dbItemToSchedule.TextToDisplay != "")
                        {
                            txtToDisplay = dbItemToSchedule.TextToDisplay;
                        }
                    }
                    if (dbItemToSchedule.DefaultScreenVisible != null)
                    {
                        defaultScreen = Convert.ToInt32(dbItemToSchedule.DefaultScreenVisible);
                    }
                    #endregion
                }


                #region//screen type decider based on data for first time load
                if (txtToDisplay != null && imageName != null && defaultScreen == 1)
                {
                    screenType = "1,1,1,1-" + pg;
                }
                else if (txtToDisplay != null && imageName != null && defaultScreen == 0)
                {
                    screenType = "0,0,1,1-" + pg;
                }
                else if (txtToDisplay == null && imageName != null && defaultScreen == 0)
                {
                    screenType = "0,0,1,0-" + pg;
                }
                else if (txtToDisplay != null && imageName == null && defaultScreen == 0)
                {
                    screenType = "0,0,0,1-" + pg;
                }
                else if (txtToDisplay != null && imageName == null && defaultScreen == 1)
                {
                    screenType = "1,1,0,1-" + pg;
                }
                else if (txtToDisplay == null && imageName != null && defaultScreen == 1)
                {
                    screenType = "1,1,1,0-" + pg;
                }
                #endregion
            }

            return screenType;
        }

        public void clearAllCookie()
        {
            HttpCookie aCookie;
            string cookieName;
            int limit = Request.Cookies.Count;
            for (int i = 0; i < limit; i++)
            {
                cookieName = Request.Cookies[i].Name;
                aCookie = new HttpCookie(cookieName);
                aCookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(aCookie);
            }
        }

        public void UpdateAndonStart(int CellID)
        {
            DateTime CurrentTime = System.DateTime.Now;
            var GetStartDate = db.tblAndonImageTextScheduledDisplays.Where(m => m.CellID == CellID && m.FlagEnd == 0 && m.FlagStart == 0 && m.IsDeleted == 0).FirstOrDefault();
            if (GetStartDate != null)
            {
                DateTime GetScheduleStart = (DateTime)GetStartDate.StartDateTime;
                var Diff = CurrentTime.Subtract(GetScheduleStart).TotalSeconds;
                if (Diff > 0)
                {
                    GetStartDate.FlagStart = 1;
                    db.Entry(GetStartDate).State = EntityState.Modified;
                    db.SaveChanges();
                }

            }
            var GetEndDate = db.tblAndonImageTextScheduledDisplays.Where(m => m.CellID == CellID && m.FlagEnd == 0 && m.FlagStart == 1 && m.IsDeleted == 0).FirstOrDefault();
            if (GetEndDate != null)
            {
                try
                {
                    DateTime GetScheduleEnd = (DateTime)GetEndDate.EndDateTime;
                    var Diff = CurrentTime.Subtract(GetScheduleEnd).TotalSeconds;
                    if (Diff > 0)
                    {
                        GetEndDate.FlagEnd = 1;
                        db.Entry(GetEndDate).State = EntityState.Modified;
                        db.SaveChanges();
                        Utility uObj = new Utility();
                        string imageName = GetEndDate.ImageName;
                        string[] arryName = imageName.Split('#');
                        if (arryName.Count() != 0)
                        {
                            foreach (var item in arryName)
                            {
                                if (item != null || item != "")
                                {
                                    uObj.deleteOldImage(item);
                                }
                            }
                        }
                    }
                }
                catch { }

            }
        }

        public string MachineDashboard(int cellID)
        {

            string correctedDate = GetCorrectedDate();  // get CorrectedDate
            string res = "";                      // string correctedDate = "2018-08-23";

            DateTime correctedDate1 = Convert.ToDateTime(correctedDate);
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {

                int c = 0;
                List<GetMachines> AllMachinesList = new List<GetMachines>();
                List<MachineConnectivityStatusModel> machineModel = new List<MachineConnectivityStatusModel>();

                var celldet = db.tblcells.Where(m => m.IsDeleted == 0 && m.CellID==cellID).OrderBy(m => m.CellID).ToList();
                foreach (var row in celldet)
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

                    var machineslist = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.CellID == row.CellID).ToList();
                    foreach (var machine in machineslist)
                    {
                        Machines machines = new Machines();
                        int machineID = machine.MachineID;
                        var modetails = db.tbllivemodes.Where(m => m.MachineID == machineID && m.IsDeleted == 0 && m.IsCompleted == 0 && m.CorrectedDate == correctedDate1.Date).OrderByDescending(m => m.ModeID).FirstOrDefault();
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

        public List<MachineConnectivityStatusModel> MachineConnectivityDet()
        {
            List<MachineConnectivityStatusModel> model = new List<MachineConnectivityStatusModel>();
            string res = "";
            DateTime nowDate = DateTime.Now;
            string correctedDate = GetCorrectedDate();
            //string correctedDate = "2019-05-24";
            DateTime correctDate = Convert.ToDateTime(correctedDate);
            var machineDetailsList = new List<tblmachinedetail>();
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                machineDetailsList = db.tblmachinedetails.Where(m => m.IsDeleted == 0).OrderByDescending(m => m.InsertedOn).ToList();
            }
            foreach (var machineDetails in machineDetailsList)
            {
                int MID = machineDetails.MachineID;
                var livemodeData = new List<tbllivemode>();
                using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
                {
                    livemodeData = db.tbllivemodes.Where(m => m.MachineID == MID && m.CorrectedDate == correctDate && m.IsDeleted == 0).ToList();
                }

                var cuttingAndPartsData = livemodeData.Select(m => new { m.ModeID, m.CuttingDuration, m.TotalPartsCount, m.ColorCode, m.MacMode }).OrderByDescending(m => m.ModeID).FirstOrDefault();

                var PowerTime = livemodeData.Sum(m => m.DurationInSec);
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
                    running = 1;
                running = (running / 60);
                databind.CuttingRatio = Math.Round(Convert.ToDecimal(((double)Cuttingtime / running)) * 100, 2).ToString();
                model.Add(databind);
            }
            res = JsonConvert.SerializeObject(model);

            return model;
        }

        private int GetParts_Cutting(int MachineID, DateTime CorrectedDate, out int TotalPartsCount)
        {
            int CuttingTime = 0;
            TotalPartsCount = 0;
            string Correcteddate = CorrectedDate.ToString("yyyy-MM-dd");
            string NxtCorrecteddate = CorrectedDate.AddDays(1).ToString("yyyy-MM-dd");
            string StartTime = Correcteddate + " 06:00:00";
            string EndTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            DateTime St = Convert.ToDateTime(StartTime);
            DateTime Et = Convert.ToDateTime(EndTime);
            var parametermasterlist = new List<parameters_master>();
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                parametermasterlist = db.parameters_master.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate.Date && m.InsertedOn >= St && m.InsertedOn <= Et).ToList();
            }
            var TopRow = parametermasterlist.OrderByDescending(m => m.ParameterID).FirstOrDefault();
            var LastRow = parametermasterlist.OrderBy(m => m.ParameterID).FirstOrDefault();
            if (TopRow != null && LastRow != null)
            {
                CuttingTime = Convert.ToInt32(TopRow.CuttingTime) - Convert.ToInt32(LastRow.CuttingTime);
                TotalPartsCount = Convert.ToInt32(TopRow.PartsTotal - LastRow.PartsTotal);
            }
            return CuttingTime;
        }

        public string GetAllMachineDetails()
        {
            List<MachineConnectivityStatusModel> model = new List<MachineConnectivityStatusModel>();
            string res = "";
            DateTime nowDate = DateTime.Now;
            string correctedDate = GetCorrectedDate();
            DateTime correctDate = Convert.ToDateTime(correctedDate);
            var machineDetailsList = new List<tblmachinedetail>();
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                machineDetailsList = db.tblmachinedetails.Where(m => m.IsDeleted == 0).OrderByDescending(m => m.InsertedOn).ToList();
            }
            foreach (var machineDetails in machineDetailsList)
            {
                int MID = machineDetails.MachineID;
                var livemodeData = new List<tbllivemode>();
                using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
                {
                    livemodeData = db.tbllivemodes.Where(m => m.MachineID == MID && m.CorrectedDate == correctDate && m.IsDeleted == 0).ToList();
                }

                var cuttingAndPartsData = livemodeData.Select(m => new { m.ModeID, m.CuttingDuration, m.TotalPartsCount, m.ColorCode, m.MacMode }).OrderByDescending(m => m.ModeID).FirstOrDefault();

                var PowerTime = livemodeData.Sum(m => m.DurationInSec);
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
                    running = 1;
                running = (running / 60);
                databind.CuttingRatio = Math.Round(Convert.ToDecimal(((double)Cuttingtime / running)) * 100, 2).ToString();
                model.Add(databind);
            }
            res = JsonConvert.SerializeObject(model);

            return res;
        }

        private string GetCorrectedDate()
        {
            DateTime correctedDate = DateTime.Now;
            //string Corre = "2019-05-25";
            //DateTime correctedDate = Convert.ToDateTime(Corre);
            var daytimings = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
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
    }
}