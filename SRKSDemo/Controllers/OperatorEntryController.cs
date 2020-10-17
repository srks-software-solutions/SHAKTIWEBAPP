using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web;
using System.Web.Mvc;
using SRKSDemo;
using SRKSDemo.Models;
using SRKSDemo.Server_Model;
namespace SRKSDemo.Controllers
{
    public class OperatorEntryController : Controller
    {
        //public i_facility_shaktiEntities1 _ServerContext = new i_facility_shaktiEntities1();
        //public CCSOEEServerEntities _ServerContext = new CCSOEEServerEntities();
        public i_facility_shaktiEntities1 _ServerContext = new i_facility_shaktiEntities1();

        // GET: OperatorEntry
        public ActionResult SaveSetting(int LossSelect = 0)
        {
            //request came from level 2 and was a last node .Level 3  code will come as parameter.
            if (LossSelect == 0)
            {
                if (Request.QueryString["selectLoss"] != null) // Ideally not null, if null go to hell :) 
                    LossSelect = Convert.ToInt32(Request.QueryString["selectLoss"]);
            }

            #region Update  tbllivemode
            GetMode GM = new GetMode();
            // String IPAddress = GM.GetIPAddressofTabSystem();
            int machineID = Convert.ToInt32(Session["MachineID"]);
            //int machineID = _ServerContext.tblmachinedetails.Where(m =>m.MachineID == 7&& m.IsDeleted == 0).Select(m => m.MachineID).FirstOrDefault();
            Session["MachineID"] = machineID;
            DateTime correctedDate = DateTime.Now;
            tbldaytiming StartTime = _ServerContext.tbldaytimings.Where(m => m.IsDeleted == 0).SingleOrDefault();
            TimeSpan Start = StartTime.StartTime;
            if (Start.Hours <= DateTime.Now.Hour)
            {
                correctedDate = DateTime.Now.Date;
            }
            else
            {
                correctedDate = DateTime.Now.AddDays(-1).Date;
            }
            string colorCode = "YELLOW";
            //to check whether the macine is in setup mode where issetup will be in 1  
            var isSetUpormaint = _ServerContext.tblSetupMaints.Where(m => m.MachineID == machineID && m.IsStarted == 1 && m.IsSetup == 0 && m.IsCompleted == 0).ToList();
            if (isSetUpormaint.Count == 0)
            {
                var mode = _ServerContext.tbllivemodes.Where(m => m.MachineID == machineID && m.ColorCode == colorCode && m.IsCompleted == 0 && m.ModeType == "IDLE" && m.ModeTypeEnd == 0)
                        .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
                if (mode != null)
                {
                    mode.LossCodeID = LossSelect;
                    mode.ModeType = "SETUP";
                    mode.StartIdle = 0;
                    mode.ModifiedOn = DateTime.Now;
                    mode.IsCompleted = 1;
                    mode.LossCodeEnteredTime = DateTime.Now;
                    //mode.IsPiWeb = 0;
                    _ServerContext.Entry(mode).State = System.Data.Entity.EntityState.Modified;
                    _ServerContext.SaveChanges();

                    tblSetupMaint SM = new tblSetupMaint();
                    SM.IsCompleted = 0;
                    SM.IsSetup = 1;
                    SM.IsStarted = 1;
                    SM.LossCodeID = LossSelect;
                    SM.MachineID = machineID;
                    SM.ModeID = mode.ModeID;
                    SM.StartTime = (DateTime)mode.StartTime;
                    //SM.Sync = 0;  Ashok
                    _ServerContext.tblSetupMaints.Add(SM);
                    _ServerContext.SaveChanges();

                    tbllivemode tm = new tbllivemode();
                    tm.BreakdownID = null;
                    tm.ColorCode = "YELLOW";
                    tm.CorrectedDate = correctedDate;
                    tm.InsertedBy = Convert.ToInt32(Session["UserID"]);
                    tm.InsertedOn = DateTime.Now;
                    tm.IsCompleted = 0;
                    tm.IsDeleted = 0;
                    tm.LossCodeID = null;
                    tm.MachineID = machineID;
                    tm.MacMode = "IDLE";
                    tm.ModeType = "IDLE";
                    tm.ModeTypeEnd = 0;
                    tm.StartIdle = 0;
                    tm.StartTime = tm.InsertedOn;
                    //tm.IsPiWeb = 0;
                    //tm.Sync = 0;         Ashok

                    _ServerContext.tbllivemodes.Add(tm);
                    _ServerContext.SaveChanges();


                    #region UnLock the Machine
                    var MacDet = _ServerContext.tblmachinedetails.Find(machineID);
                    //if (MacDet.MachineModelType == 1)
                    //{
                    //    AddFanucMachineWithConn AC = new AddFanucMachineWithConn(MacDet.IPAddress);
                    //    AC.SetMachineUnlock((ushort)MacDet.MachineUnlockBit, (ushort)MacDet.MachineLockBit);
                    //}
                    #endregion

                    #endregion

                    #region Insert to Operator Dashboard

                    tblOperatorDashboard operatorDashboard = new tblOperatorDashboard();
                    DateTime CorrectedDateToDate = Convert.ToDateTime(correctedDate);
                    Random OperatorDashboardID = new Random();
                    operatorDashboard.MachineID = Convert.ToInt32(Session["MachineID"]);
                    operatorDashboard.CorrectedDate = CorrectedDateToDate;
                    operatorDashboard.SlNo = _ServerContext.tblOperatorDashboards.Where(m => m.CorrectedDate == CorrectedDateToDate).Where(m => m.MachineID == machineID).ToList().Count + 1; //  @Pavan , I'm not sure what you meant here..  --Count++ from the tbloperatordashboard for the Machine and CorrectedDate
                    operatorDashboard.MessageCode = "Setting";
                    //operatorDashboard.MessageCode = "IDLE";
                    operatorDashboard.MessageDescription = "Machine Started In Setting Mode";
                    operatorDashboard.MessageStartTime = DateTime.Now;
                    operatorDashboard.InsertedOn = DateTime.Now;
                    operatorDashboard.InsertedBy = Convert.ToInt32(Session["UserID"]);
                    operatorDashboard.IsDeleted = 0;
                    _ServerContext.tblOperatorDashboards.Add(operatorDashboard);
                    _ServerContext.SaveChanges();

                    #endregion

                    #region Update tblOperatorHeader 

                    var operatorHeader = _ServerContext.tblOperatorHeaders.Where(m => m.MachineID == machineID).OrderByDescending(m => m.InsertedOn).FirstOrDefault();
                    operatorHeader.MachineMode = "Setting";
                    //operatorHeader.MachineMode = "IDLE";
                    operatorHeader.ModifiedOn = DateTime.Now;
                    operatorHeader.ModifiedBy = 1;
                    _ServerContext.Entry(operatorHeader).State = System.Data.Entity.EntityState.Modified;
                    _ServerContext.SaveChanges();

                    #endregion
                    return RedirectToAction("DashboardProduction");
                }
                else
                {
                    Session["setuperror"] = "Machine is not in IDLE state, Cannot Start the Setting on the Machine.";
                     ViewBag.SetupError = Session["setuperror"];
                    return Redirect("SettingWindow");
                }
            }
            else
            {
                Session["setuperror"] = "Machine is in Maintance State, Cannot Start the Setting on the Machine.";
                ViewBag.SetupError = Session["setuperror"];
                return Redirect("SettingWindow");
            }

        }

        public ActionResult DashboardProduction()
        {
            // TempData["toaster_success"] = "pinging messages to server";
            GetMode GM = new GetMode();
            //  String IPAddress = GM.GetIPAddressofTabSystem();
            //int macid = Session["MachineID"];
            //int MachineID = _ServerContext.tblmachinedetails.Where(m =>m.MachineID == 7&& m.IsDeleted == 0).Select(m => m.MachineID).FirstOrDefault();
            int MachineID = Convert.ToInt32( Session["MachineID"]);

            try
            {
                string sessionvar = Session["message"].ToString();
                if (sessionvar == null || sessionvar == string.Empty)
                {
                    Session["message"] = "";
                }
            }
            catch (Exception e)
            {
                Session["message"] = "";
            }

            //Session["MachineID"] = MachineID;
            //int MachineID = Convert.ToInt32(Session["MachineID"]);
            var HeaderDet = _ServerContext.tblOperatorHeaders.Where(m => m.MachineID == MachineID).FirstOrDefault();
            if (HeaderDet != null)
            {
                ViewBag.MachineMode = HeaderDet.MachineMode;
                ViewBag.MachineName = HeaderDet.tblmachinedetail.MachineDisplayName;
                ViewBag.TabStatus = HeaderDet.TabConnecStatus;
                ViewBag.ServerStatus = HeaderDet.ServerConnecStatus;
                ViewBag.PageName = "Dashboard";
                ViewBag.Shift = HeaderDet.Shift;
            }
            DateTime correctedDate = DateTime.Now;
            tbldaytiming StartTime = _ServerContext.tbldaytimings.Where(m => m.IsDeleted == 0).SingleOrDefault();
            TimeSpan Start = StartTime.StartTime;
            if (Start.Hours <= DateTime.Now.Hour)
            {
                correctedDate = DateTime.Now.Date;
            }
            else
            {
                correctedDate = DateTime.Now.AddDays(-1).Date;
            }

            var prvmode = _ServerContext.tbllivemodes.Where(m => m.MachineID == MachineID && m.ModeType == "SETUP" && m.ModeTypeEnd == 0)
                    .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
            if (prvmode != null)
            {
                ViewBag.SetUpStarted = "1";
                ViewBag.MachineMode = "Setting";
            }

            var prvmodeMaint = _ServerContext.tbllivemodes.Where(m => m.MachineID == MachineID && m.ModeType == "MNT" && m.ModeTypeEnd == 0)
                    .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
            if (prvmodeMaint != null)
            {
                ViewBag.MNTStarted = "1";
                ViewBag.MachineMode = "MNT";
            }
            if (Session["setuperror"] != null)
            {
                Session["message"] = Session["setuperror"];
                Session["setuperror"] = "";

            }
            var GetDashboardData = _ServerContext.tblOperatorDashboards.Where(m => m.MachineID == MachineID && m.CorrectedDate == correctedDate).OrderByDescending(m => m.InsertedOn).ToList();
            tblOperatorDashboard TOD = new tblOperatorDashboard();
            OperatorDashboard OD = new OperatorDashboard();
            OD.OpDashboardList = GetDashboardData;
            OD.OpDashboard = TOD;
            return View(OD);
        }

        public ActionResult DashboardSetting()
        {
            return View();
        }

        public ActionResult EntryWindow(int ShiftID = 0)
        {
            GetMode GM = new GetMode();
            //  String IPAddress = GM.GetIPAddressofTabSystem();

            //int MachineID = _ServerContext.tblmachinedetails.Where(m => m.MachineID ==  && m.IsDeleted == 0).Select(m => m.MachineID).FirstOrDefault();

            int MachineID = Convert.ToInt32(Session["MachineID"]);

            //int MachineID = _ServerContext.tblmachinedetails.Where(m => m.MachineID == 7 && m.IsDeleted == 0).Select(m => m.MachineID).FirstOrDefault();

            var HeaderDet = _ServerContext.tblOperatorHeaders.Where(m => m.MachineID == MachineID).SingleOrDefault();
            ViewBag.MachineMode = HeaderDet.MachineMode;
            ViewBag.MachineName = HeaderDet.tblmachinedetail.MachineDisplayName;
            ViewBag.TabStatus = HeaderDet.TabConnecStatus;
            ViewBag.ServerStatus = HeaderDet.ServerConnecStatus;
            ViewBag.PageName = "Operator Entry";
            ViewBag.Shift = HeaderDet.Shift;
            var prvmode = _ServerContext.tbllivemodes.Where(m => m.MachineID == MachineID && m.ModeType == "SETUP" && m.ModeTypeEnd == 0)
                    .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
            if (prvmode != null)
            {
                ViewBag.SetUpStarted = "1";
                ViewBag.MachineMode = "Setting";
            }

            var prvmodeMaint = _ServerContext.tbllivemodes.Where(m => m.MachineID == MachineID && m.ModeType == "MNT" && m.ModeTypeEnd == 0)
                    .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
            if (prvmodeMaint != null)
            {
                ViewBag.MNTStarted = "1";
                ViewBag.MachineMode = "MNT";
            }

            ViewBag.HoldReason = new SelectList(_ServerContext.tblholdcodes.Where(m => m.IsDeleted == 0), "HoldCodeID", "HoldCodeDesc");
            //ViewData["HoldReason"] = new SelectList(_ServerContext.tblholdcodes.Where(m => m.IsDeleted == 0), "HoldCodeID", "HoldCodeDesc");
            var WOE = _ServerContext.tblworkorderentries.Where(m => m.MachineID == MachineID && m.IsFinished == 0 && m.IsStarted == 1).OrderByDescending(m => m.HMIID).ToList().FirstOrDefault();

            if (WOE != null)
            {
                ShiftID = WOE.ShiftID;

                //    using (MsqlConnection mc = new MsqlConnection())
                //    {
                //        mc.open();

                //        String getparametersquery = "SELECT FGCode, ProdOrderQty FROM unitworksccs.[dbo].tblworkorderentry WHERE HMIID = " + WOE.HMIID + "";
                //        SqlDataAdapter da = new SqlDataAdapter(getparametersquery, mc.sqlConnection);
                //        DataTable dt = new DataTable();
                //        da.Fill(dt);
                //        mc.close();

                //        if (dt.Rows.Count != 0)
                //        {
                //            WOE.FGCode = dt.Rows[0][0].ToString();
                //            WOE.ProdOrderQty = Convert.ToInt32(dt.Rows[0][1]);
                //        }
                //    }
            }
            ViewData["ShiftID"] = new SelectList(_ServerContext.shift_master.Where(m => m.IsDeleted == 0), "ShiftID", "ShiftName", ShiftID);
            return View(WOE);
        }
        [HttpPost]
        public string GetOperation(string Prefix, string ProductionorderNo)
        {
            string res = "";
            var operationdet = _ServerContext.tblProdPlanMasters.Where(m => m.OperationNo.StartsWith(Prefix) && m.Prod_Order_No == ProductionorderNo).Select(m => m.OperationNo).ToList();
            res = Newtonsoft.Json.JsonConvert.SerializeObject(operationdet);
            return res;

        }

        // Get Operation Number based on Production as AutoSuggest
        [HttpGet]
        public string GetOperationDetails(string ProductionorderNo)
        {
            string res = "";
            var operationdet = _ServerContext.tblProdPlanMasters.Where(m => m.Prod_Order_No == ProductionorderNo).Select(m => m.OperationNo).ToList();
            res = Newtonsoft.Json.JsonConvert.SerializeObject(operationdet);
            return res;
        }

        [HttpPost]
        public ActionResult EntryWindow(tblworkorderentry WOE)
        {
            GetMode GM = new GetMode();
            // String IPAddress = GM.GetIPAddressofTabSystem();
            int MachineID = Convert.ToInt32(Session["MachineID"]);
            //int MachineID = _ServerContext.tblmachinedetails.Where(m => m.MachineID == 7 && m.IsDeleted == 0).Select(m => m.MachineID).First();

            Session["MachineID"] = MachineID;
            var HeaderDet = _ServerContext.tblOperatorHeaders.Where(m => m.MachineID == MachineID).SingleOrDefault();
            ViewBag.MachineMode = HeaderDet.MachineMode;
            ViewBag.MachineName = HeaderDet.tblmachinedetail.MachineDisplayName;
            ViewBag.TabStatus = HeaderDet.TabConnecStatus;
            ViewBag.ServerStatus = HeaderDet.ServerConnecStatus;
            ViewBag.PageName = "Operator Entry";
            ViewBag.Shift = HeaderDet.Shift;
            var prvmode = _ServerContext.tbllivemodes.Where(m => m.MachineID == MachineID && m.ModeType == "SETUP" && m.ModeTypeEnd == 0)
                    .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
            if (prvmode != null)
            {
                ViewBag.SetUpStarted = "1";
                ViewBag.MachineMode = "Setting";
            }

            var prvmodeMaint = _ServerContext.tbllivemodes.Where(m => m.MachineID == MachineID && m.ModeType == "MNT" && m.ModeTypeEnd == 0)
                    .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
            if (prvmodeMaint != null)
            {
                ViewBag.MNTStarted = "1";
                ViewBag.MachineMode = "MNT";
            }

            DateTime correctedDate = DateTime.Now;
            tbldaytiming StartTime = _ServerContext.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
            TimeSpan Start = StartTime.StartTime;
            if (Start.Hours <= DateTime.Now.Hour)
            {
                correctedDate = DateTime.Now;
            }
            else
            {
                correctedDate = DateTime.Now.AddDays(-1);
            }

            int PrvTotalQty = 0;
            int PrvProcessQty = 0;

            var GetProcessQty = _ServerContext.tblworkorderentries.Where(m => m.Prod_Order_No == WOE.Prod_Order_No && m.OperationNo == WOE.OperationNo).OrderByDescending(m => m.HMIID).FirstOrDefault();
            if (GetProcessQty != null)
            {
                PrvTotalQty = GetProcessQty.Total_Qty;
                PrvProcessQty = GetProcessQty.ProcessQty;
            }

            #region check split

            var item = _ServerContext.tblProdPlanMasters.Where(m => m.Prod_Order_No == WOE.Prod_Order_No && m.OperationNo == WOE.OperationNo).FirstOrDefault();
            int stat = 0;// 1=SplitOrderQty 0=OrderQty
            if (item != null)
            {
                string status = item.Status;
                if (status == "SPLT")
                {
                    stat = 1;
                }
            }
            #endregion

            tblworkorderentry WOEntry = new tblworkorderentry();
            WOEntry.MachineID = MachineID;
            WOEntry.OperationNo = WOE.OperationNo;
            WOEntry.OperatorID = WOE.OperatorID;
            WOEntry.PartNo =WOE.FGCode;
            WOEntry.PEStartTime = DateTime.Now;
            WOEntry.Prod_Order_No = WOE.Prod_Order_No;
            WOEntry.ScrapQty = 0;
            WOEntry.ShiftID = WOE.ShiftID;
            WOEntry.Total_Qty = 0;
            WOEntry.WOStart = DateTime.Now;
            WOEntry.Yield_Qty = 0;
            WOEntry.IsStarted = 1;
            WOEntry.CorrectedDate = correctedDate;
            WOEntry.ProcessQty = PrvProcessQty + PrvTotalQty;
            WOEntry.FGCode = WOE.FGCode;
            WOEntry.ProdOrderQty = WOE.ProdOrderQty;
            WOEntry.Status = 0;
            WOEntry.IsFinished = 0;
            WOEntry.isWorkOrder = 1;
            WOEntry.IsMultiWO = 0;
            WOEntry.IsFlag = 0;
            WOEntry.IsHold = 0;
            //WOEntry.SyncInsert = 0; //by Ashok
            if (stat == 1)
            {
                WOEntry.isSplit = true;
            }
            else
                WOEntry.isSplit = false;
            _ServerContext.tblworkorderentries.Add(WOEntry);
            _ServerContext.SaveChanges();

            int hmmid = WOEntry.HMIID;

            string operationNo = WOE.OperationNo;
            string FGCode = WOE.FGCode;

            var toolObj = _ServerContext.tblStdToolLives.Where(m => m.FGCode == FGCode && m.OperationNo == operationNo && m.IsDeleted == false && m.MachineID==MachineID).ToList();

            foreach (var itemTool in toolObj)
            {
                int ToolLifeCount = 0;
                GetToolLifeCounter(FGCode, operationNo, itemTool.ToolNo, MachineID,ref ToolLifeCount);
                //adding new row server
                tbltoollifeoperator ServertabTLO = new tbltoollifeoperator();
                ServertabTLO.MachineID = MachineID;
                ServertabTLO.ToolNo = itemTool.ToolNo;
                ServertabTLO.ToolName = itemTool.ToolName;
                ServertabTLO.ToolCTCode = itemTool.CTCode;
                ServertabTLO.StandardToolLife = itemTool.StdToolLife;
                ServertabTLO.toollifecounter = ToolLifeCount;
                ServertabTLO.InsertedOn = DateTime.Now;
                ServertabTLO.InsertedBy = Convert.ToInt32(WOE.OperatorID);
                ServertabTLO.IsReset = 0;
                ServertabTLO.IsDeleted = 0;
                ServertabTLO.ResetCounter = 0;
              
                ServertabTLO.IsCompleted = false;
                ServertabTLO.IsCycleStart = false;
                _ServerContext.tbltoollifeoperators.Add(ServertabTLO);
                _ServerContext.SaveChanges();

                //    int serverTLOp = ServertabTLO.ToolLifeID;
                //    //adding to tab
                //    tbltoollifeoperator tabTLO = new tbltoollifeoperator();
                //    tabTLO.MachineID = MachineID;
                //    tabTLO.ToolNo = itemTool.ToolNo;
                //    tabTLO.ToolName = itemTool.ToolName;
                //    tabTLO.ToolCTCode = itemTool.CTCode;
                //    tabTLO.StandardToolLife = itemTool.StdToolLife;
                //    tabTLO.toollifecounter = ToolLifeCount;
                //    tabTLO.InsertedOn = DateTime.Now;
                //    tabTLO.InsertedBy = Convert.ToInt32(WOE.OperatorID);
                //    tabTLO.IsReset = 0;
                //    tabTLO.IsDeleted = 0;
                //    tabTLO.ResetCounter = 0;
                //    tabTLO.HMIID = hmmid;
                //    tabTLO.Sync = 1;
                //    tabTLO.ToolIDAdmin = serverTLOp;
                //    tabTLO.IsCompleted = false;
                //    _ServerContext.tbltoollifeoperators.Add(tabTLO);
                //    _ServerContext.SaveChanges();
            }

            return RedirectToAction("EntryWindow");
        }

        public JsonResult FinishProdOrder(int Yqty, int Sqty, int Tqty)
        {
            int MachineID = 0;
            try
            {
                MachineID = Convert.ToInt32(Session["MachineID"]);
            }
            catch
            {
                GetMode GM = new GetMode();
                //  String IPAddress = GM.GetIPAddressofTabSystem();
                 MachineID = Convert.ToInt32(Session["MachineID"]);
                //MachineID = _ServerContext.tblmachinedetails.Where(m => m.MachineID == 7 && m.IsDeleted == 0).Select(m => m.MachineID).First();

                Session["MachineID"] = MachineID;
            }
            var Data = true;

            var WOE = _ServerContext.tblworkorderentries.Where(m => m.MachineID == MachineID && m.IsFinished == 0 && m.IsStarted == 1).OrderByDescending(m => m.HMIID).ToList().FirstOrDefault();

            WOE.Yield_Qty = Yqty;
            WOE.ScrapQty = Sqty;
            WOE.Total_Qty = Tqty;
            WOE.IsFinished = 1;
            WOE.WOEnd = DateTime.Now;
            // WOE.SyncInsert = 0;            Ashok
            _ServerContext.Entry(WOE).State = System.Data.Entity.EntityState.Modified;
            _ServerContext.SaveChanges();
            // updateTool(WOE.HMIID);
            return Json(Data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult HoldProdOrder(int Yqty, int Sqty, int Tqty, int HoldReasonID)
        {
            int MachineID = 0;
            try
            {
                MachineID = Convert.ToInt32(Session["MachineID"]);
            }
            catch
            {
                GetMode GM = new GetMode();
                //  String IPAddress = GM.GetIPAddressofTabSystem();
                 MachineID = Convert.ToInt32(Session["MachineID"]);
                //MachineID = _ServerContext.tblmachinedetails.Where(m => m.MachineID == 7 && m.IsDeleted == 0).Select(m => m.MachineID).First();

                Session["MachineID"] = MachineID;
            }
            var Data = true;

            var WOE = _ServerContext.tblworkorderentries.Where(m => m.MachineID == MachineID && m.IsFinished == 0 && m.IsStarted == 1).OrderByDescending(m => m.HMIID).ToList().FirstOrDefault();

            WOE.Yield_Qty = Yqty;
            WOE.ScrapQty = Sqty;
            WOE.Total_Qty = Tqty;
            WOE.HoldCodeID = HoldReasonID;
            WOE.IsFinished = 1;
            WOE.IsHold = 1;
            WOE.WOEnd = DateTime.Now;
            WOE.HoldTime = DateTime.Now;
            //WOE.SyncInsert = 0;             //by ashok
            _ServerContext.Entry(WOE).State = System.Data.Entity.EntityState.Modified;
            _ServerContext.SaveChanges();
            //updateTool(WOE.HMIID);
            return Json(Data, JsonRequestBehavior.AllowGet);
        }

            //public void updateTool(int hmiid)
            //{
            //    #region//updation in tbltoollife operator in server and tab

            //    var list = _ServerContext.tbltoollifeoperators.Where(m => m.HMIID == hmiid).ToList();
            //    foreach (var item in list)
            //    {
            //        try
            //        {
            //            var updateTabTool = _ServerContext.tbltoollifeoperators.Find(item.ToolLifeID);
            //            updateTabTool.IsCompleted = true;
            //            _ServerContext.Entry(updateTabTool).State = System.Data.Entity.EntityState.Modified;
            //            _ServerContext.SaveChanges();

            //            var updateServerTool = _ServerContext.tbltoollifeoperators.Find(item.ToolIDAdmin);
            //            updateServerTool.IsCompleted = true;
            //            _ServerContext.Entry(updateServerTool).State = System.Data.Entity.EntityState.Modified;
            //            _ServerContext.SaveChanges();
            //        }
            //        catch
            //        {

            //        }
            //    }
            //    #endregion
            //}

            public void GetToolLifeCounter(String FGCode, String OpNo, String ToolNum,int MachineID, ref int ToolLifeCount)
            {
                ToolLifeCount = 0;
                var GetHMIID = _ServerContext.tblworkorderentries.Where(m => m.FGCode == FGCode && m.OperationNo == OpNo && m.IsFinished == 1 && m.MachineID==MachineID).OrderByDescending(m => m.HMIID).FirstOrDefault();
                if (GetHMIID != null)
                {
                    var GetToolCount = _ServerContext.tbltoollifeoperators.Where(m => m.HMIID == GetHMIID.HMIID && m.IsReset == 0 && m.ToolNo == ToolNum).OrderByDescending(m => m.ToolLifeID).FirstOrDefault();
                    if (GetToolCount != null)
                    {
                        ToolLifeCount = GetToolCount.toollifecounter;
                    }
                }
            }

            public ActionResult MaintenanceProductionWindow(int smValue = 0)
        {
            GetMode GM = new GetMode();
            //  String IPAddress = GM.GetIPAddressofTabSystem();
          int  MachineID = Convert.ToInt32(Session["MachineID"]);
            //int MachineID = _ServerContext.tblmachinedetails.Where(m =>m.MachineID == 7&& m.IsDeleted == 0).Select(m => m.MachineID).First();

            Session["MachineID"] = MachineID;
            var HeaderDet = _ServerContext.tblOperatorHeaders.Where(m => m.MachineID == MachineID).SingleOrDefault();
            if (HeaderDet != null)
            {
                ViewBag.MachineMode = HeaderDet.MachineMode;
                ViewBag.MachineName = HeaderDet.tblmachinedetail.MachineDisplayName;
                ViewBag.TabStatus = HeaderDet.TabConnecStatus;
                ViewBag.ServerStatus = HeaderDet.ServerConnecStatus;
                ViewBag.PageName = "MNT";
                ViewBag.Shift = HeaderDet.Shift;
            }

            var prvmode = _ServerContext.tbllivemodes.Where(m => m.MachineID == MachineID && m.ModeType == "SETUP" && m.ModeTypeEnd == 0)
                    .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
            if (prvmode != null)
            {
                ViewBag.SetUpStarted = "1";
                ViewBag.MachineMode = "Setting";
            }

            var prvmodeMaint = _ServerContext.tbllivemodes.Where(m => m.MachineID == MachineID && m.ModeType == "MNT" && m.ModeTypeEnd == 0)
                    .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
            if (prvmodeMaint != null)
            {
                ViewBag.MNTStarted = "1";
                ViewBag.MachineMode = "MNT";
            }

            var GetMaint = _ServerContext.tblSetupMaints.Where(m => m.MachineID == MachineID && m.IsStarted == 1 && m.IsSetup == 0 && m.IsCompleted == 0).FirstOrDefault();
            if (smValue != 1)
            {
                Session["message"] = "nullinsess";
            }
            return View(GetMaint);
        }

        public ActionResult EndMaintenance(int smValue = 0)
        {
            GetMode GM = new GetMode();
            //  String IPAddress = GM.GetIPAddressofTabSystem();
           int MachineID = Convert.ToInt32(Session["MachineID"]);
            //int MachineID = _ServerContext.tblmachinedetails.Where(m =>m.MachineID == 7&& m.IsDeleted == 0).Select(m => m.MachineID).First();

            Session["MachineID"] = MachineID;

            var mode = _ServerContext.tbllivemodes.Where(m => m.MachineID == MachineID && m.ModeType == "MNT" && m.IsCompleted == 1 && m.ModeTypeEnd == 0)
                        .OrderByDescending(m => m.ModeID).FirstOrDefault();
            if (mode != null)
            {
                mode.IsCompleted = 1;
                mode.ModeTypeEnd = 1;
                mode.EndTime = DateTime.Now;
                mode.DurationInSec = Convert.ToInt32(DateTime.Now.Subtract(Convert.ToDateTime(mode.StartTime)).TotalSeconds);
                mode.StartIdle = 0;
                mode.ModifiedOn = DateTime.Now;
               // mode.IsPiWeb = 0;
                _ServerContext.Entry(mode).State = System.Data.Entity.EntityState.Modified;
                _ServerContext.SaveChanges();
            }
            var GetSetting = _ServerContext.tblSetupMaints.Where(m => m.MachineID == MachineID && m.IsStarted == 1 && m.IsSetup == 0 && m.IsCompleted == 0).FirstOrDefault();
            GetSetting.IsCompleted = 1;
            GetSetting.EndTime = System.DateTime.Now;
            GetSetting.DurationInSec = Convert.ToInt32(DateTime.Now.Subtract(Convert.ToDateTime(GetSetting.StartTime)).TotalSeconds);

            _ServerContext.Entry(GetSetting).State = System.Data.Entity.EntityState.Modified;
            _ServerContext.SaveChanges();


            var GetTOD = _ServerContext.tblOperatorDashboards.Where(m => m.MachineID == MachineID && m.MessageCode == "MNT").OrderByDescending(m => m.InsertedOn).FirstOrDefault();
            GetTOD.MessageEndTime = System.DateTime.Now;
            GetTOD.TotalDurationinMin = Convert.ToInt32(System.DateTime.Now.Subtract(GetTOD.MessageStartTime).TotalMinutes);
            _ServerContext.Entry(GetTOD).State = System.Data.Entity.EntityState.Modified;
            _ServerContext.SaveChanges();

            var checkIsCompletedRow = _ServerContext.tbllivemodes.Where(m => m.IsCompleted == 0).ToList();
            if (checkIsCompletedRow.Count == 0)
            {
                tbllivemode TM = new tbllivemode();
                TM.ColorCode = "YELLOW";
                TM.CorrectedDate = mode.CorrectedDate;
                TM.IsCompleted = 0;
                TM.IsDeleted = 0;
                TM.InsertedOn = DateTime.Now;
                TM.InsertedBy = 1;
                TM.MachineID = mode.MachineID;
                TM.MacMode = "IDLE";
                TM.ModeType = "IDLE";
                TM.StartIdle = 0;
                TM.StartTime = mode.EndTime;
                //TM.IsPiWeb = 0;
                //TM.Sync = 0;     //By Ashok
                _ServerContext.tbllivemodes.Add(TM);
                _ServerContext.SaveChanges();
            }



            #region Update tblOperatorHeader 

            var operatorHeader = _ServerContext.tblOperatorHeaders.Where(m => m.MachineID == MachineID).OrderByDescending(m => m.InsertedOn).FirstOrDefault();
            operatorHeader.MachineMode = "Production";
            //operatorHeader.MachineMode = "IDLE";
            operatorHeader.ModifiedOn = DateTime.Now;
            operatorHeader.ModifiedBy = 1;
            _ServerContext.Entry(operatorHeader).State = System.Data.Entity.EntityState.Modified;
            _ServerContext.SaveChanges();

            #endregion

            #region UnLock the Machine
            var MacDet = _ServerContext.tblmachinedetails.Find(MachineID);
            //if (MacDet.MachineModelType == 1)
            //{
            //    AddFanucMachineWithConn AC = new AddFanucMachineWithConn(MacDet.IPAddress);
            //    AC.SetMachineUnlock((ushort)MacDet.MachineUnlockBit, (ushort)MacDet.MachineLockBit);
            //}
            #endregion
            if (smValue == 1)
            {
                return Redirect("SettingWindow");
            }
            return RedirectToAction("DashboardProduction");
        }

        public ActionResult MaintenanceWindow(int LossSelect = 0)
        {

            GetMode GM = new GetMode();
            //  String IPAddress = GM.GetIPAddressofTabSystem();
            try
            {
                string sessionvar = Session["message"].ToString();
                if (sessionvar == null || sessionvar == string.Empty)
                {
                    Session["message"] = "";
                }
            }
            catch (Exception e)
            {
                Session["message"] = "";
            }
          int  MachineID = Convert.ToInt32(Session["MachineID"]);
            //int MachineID = _ServerContext.tblmachinedetails.Where(m =>m.MachineID == 7&& m.IsDeleted == 0).Select(m => m.MachineID).First();

            Session["MachineID"] = MachineID;
            var isSetUpormaint = _ServerContext.tblSetupMaints.Where(m => m.MachineID == MachineID && m.IsStarted == 1 && m.IsSetup == 1 && m.IsCompleted == 0).ToList();
            if (isSetUpormaint.Count == 0)
            {
                var HeaderDet = _ServerContext.tblOperatorHeaders.Where(m => m.MachineID == MachineID).SingleOrDefault();
                ViewBag.MachineMode = HeaderDet.MachineMode;
                ViewBag.MachineName = HeaderDet.tblmachinedetail.MachineDisplayName;
                ViewBag.TabStatus = HeaderDet.TabConnecStatus;
                ViewBag.ServerStatus = HeaderDet.ServerConnecStatus;
                ViewBag.PageName = "MNT";
                ViewBag.Shift = HeaderDet.Shift;

                var prvmode = _ServerContext.tbllivemodes.Where(m => m.MachineID == MachineID && m.ModeType == "SETUP" && m.ModeTypeEnd == 0)
                        .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
                if (prvmode != null)
                {
                    ViewBag.SetUpStarted = "1";
                    ViewBag.MachineMode = "Setting";
                }

                var prvmodeMaint = _ServerContext.tbllivemodes.Where(m => m.MachineID == MachineID && m.ModeType == "MNT" && m.ModeTypeEnd == 0)
                        .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
                if (prvmodeMaint != null)
                {
                    ViewBag.MNTStarted = "1";
                    ViewBag.MachineMode = "MNT";
                }

                if (HeaderDet.MachineMode == "MNT")
                {
                    return RedirectToAction("MaintenanceProductionWindow");
                }

                var tblLossCodes = _ServerContext.tbllossescodes.Where(m => m.MessageType == "MNT").ToList();
                ViewBag.lossCodes = tblLossCodes;
                if (tblLossCodes.Count != 0)
                {
                    Session["message"] = "";
                    Session["messagevalue"] = "2";//for settings
                    if (LossSelect == 0)//first time ,show level 2
                    {
                        int lossCodeID = tblLossCodes.Find(a => a.MessageType == "MNT" && a.LossCodesLevel == 1).LossCodeID;
                        ViewBag.lossCodeID = lossCodeID;
                        ViewBag.level = 2;
                    }
                    else // show level 3
                    {
                        int lossCodeID = LossSelect;
                        ViewBag.lossCodeID = lossCodeID;
                        ViewBag.level = 3;
                    }
                }
            }
            else
            {
                Session["message"] = "Machine is in Setting. Cannot start the Maintenance. Stop the Setting, and then start the Maintenance.";
                Session["messagevalue"] = "1";//for maintaince
                return Redirect("SettingProduction?smValue=2");
            }
            return View();
        }

        public ActionResult SaveMaintenance(int LossSelect = 0)
        {
            //request came from level 2 and was a last node .Level 3  code will come as parameter.
            if (LossSelect == 0)
            {
                if (Request.QueryString["selectLoss"] != null) // Ideally not null, if null go to hell :) 
                    LossSelect = Convert.ToInt32(Request.QueryString["selectLoss"]);
            }

            GetMode GM = new GetMode();
            //  String IPAddress = GM.GetIPAddressofTabSystem();
            int MachineID = Convert.ToInt32(Session["MachineID"]);
            //int MachineID = _ServerContext.tblmachinedetails.Where(m =>m.MachineID == 7&& m.IsDeleted == 0).Select(m => m.MachineID).First();

            Session["MachineID"] = MachineID;
            string correctedDate = null;
            tbldaytiming StartTime = _ServerContext.tbldaytimings.Where(m => m.IsDeleted == 0).SingleOrDefault();
            TimeSpan Start = StartTime.StartTime;
            if (Start.Hours <= DateTime.Now.Hour)
            {
                correctedDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                correctedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }
            try
            {
                string sessionvar = Session["message"].ToString();
                if (sessionvar == null || sessionvar == string.Empty)
                {
                    Session["message"] = "";
                }
            }
            catch (Exception e)
            {
                Session["message"] = "";
            }
            var prvmode = _ServerContext.tbllivemodes.Where(m => m.MachineID == MachineID && m.IsCompleted == 0)
                    .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
            //to check whether the macine is in setup mode where issetup will be in 1  
            var isSetUpormaint = _ServerContext.tblSetupMaints.Where(m => m.MachineID == MachineID && m.IsStarted == 1 && m.IsSetup == 1 && m.IsCompleted == 0).ToList();
            if (isSetUpormaint.Count == 0)
            {
                int flag = 0;
                #region Update  tbllivemode
                if (prvmode.ColorCode == "YELLOW" || prvmode.ColorCode == "BLUE")
                {
                    //if (prvmode.ColorCode == "YELLOW")  // added and commented by Ashok
                    //{
                        prvmode.LossCodeID = LossSelect;
                        prvmode.ModeType = "MNT";
                        prvmode.StartIdle = 0;
                        prvmode.ColorCode = "RED";
                        prvmode.MacMode = "MNT";
                        prvmode.IsCompleted = 1;
                        prvmode.LossCodeEnteredTime = DateTime.Now;
                        prvmode.LossCodeEnteredBy = "";
                        prvmode.ModifiedOn = DateTime.Now;
                        prvmode.ModifiedBy = Convert.ToInt32(Session["UserID"]);
                        // prvmode.IsPiWeb = 0;
                        _ServerContext.Entry(prvmode).State = System.Data.Entity.EntityState.Modified;
                        _ServerContext.SaveChanges();

                        tblSetupMaint SM = new tblSetupMaint();

                        SM.IsCompleted = 0;
                        SM.IsSetup = 0;
                        SM.IsStarted = 1;
                        SM.LossCodeID = LossSelect;
                        SM.MachineID = MachineID;
                        SM.ModeID = prvmode.ModeID;
                        SM.StartTime = (DateTime)prvmode.StartTime;

                        // SM.Sync = 0;    // Ashok

                        _ServerContext.tblSetupMaints.Add(SM);
                        _ServerContext.SaveChanges();

                    //}
                    //else
                    //{
                    //    //prvmode.LossCodeID = LossSelect;
                    //    //prvmode.ModeType = "MNT";
                    //    prvmode.StartIdle = 0;
                    //    //prvmode.ColorCode = "RED";
                    //    //prvmode.MacMode = "MNT";
                    //    prvmode.IsCompleted = 1;
                    //    prvmode.ModeTypeEnd=1;
                    //    prvmode.EndTime = DateTime.Now;
                    //    prvmode.DurationInSec = Convert.ToInt32(DateTime.Now.Subtract(Convert.ToDateTime(prvmode.StartTime)).TotalSeconds);
                    //    //prvmode.LossCodeEnteredTime = DateTime.Now;
                    //    //prvmode.LossCodeEnteredBy = "";
                    //    prvmode.ModifiedOn = DateTime.Now;
                    //    prvmode.ModifiedBy = Convert.ToInt32(Session["UserID"]);
                    //    // prvmode.IsPiWeb = 0;
                    //    _ServerContext.Entry(prvmode).State = System.Data.Entity.EntityState.Modified;
                    //    _ServerContext.SaveChanges();

                    //    tbllivemode modedet = new tbllivemode();
                    //    modedet.MacMode = "MNT";
                    //    modedet.StartIdle = 0;
                    //    modedet.IsCompleted = 0;
                    //    modedet.CorrectedDate = DateTime.Now;
                    //    modedet.ColorCode = "RED";
                    //    modedet.LossCodeEnteredBy = "";
                    //    modedet.LossCodeID = LossSelect;
                    //    modedet.LossCodeEnteredTime = DateTime.Now;
                    //    modedet.StartTime = DateTime.Now;
                    //    modedet.ModeTypeEnd = 0;
                    //    modedet.MachineID = MachineID;
                    //    modedet.InsertedBy= Convert.ToInt32(Session["UserID"]);
                    //    _ServerContext.tbllivemodes.Add(modedet);
                    //    _ServerContext.SaveChanges();



                    //    //\\\
                    //    // prvmode.LossCodeID = LossSelect;
                    //    //prvmode.ModeType = "SETUP";
                    //    //prvmode.StartIdle = 0;
                    //    //prvmode.ModifiedOn = DateTime.Now;
                    //    //prvmode.IsCompleted = 1;
                    //    //prvmode.LossCodeEnteredTime = DateTime.Now;
                    //    ////mode.IsPiWeb = 0;
                    //    //_ServerContext.Entry(prvmode).State = System.Data.Entity.EntityState.Modified;
                    //    //_ServerContext.SaveChanges();

                    //    tblSetupMaint SM = new tblSetupMaint();
                    //    SM.IsCompleted = 0;
                    //    SM.IsSetup = 1;
                    //    SM.IsStarted = 1;
                    //    SM.LossCodeID = LossSelect;
                    //    SM.MachineID = MachineID;
                    //    SM.ModeID = prvmode.ModeID;
                    //    SM.StartTime = (DateTime)prvmode.StartTime;
                    //    //SM.Sync = 0;  Ashok
                    //    _ServerContext.tblSetupMaints.Add(SM);
                    //    _ServerContext.SaveChanges();

                    //    tbllivemode tm = new tbllivemode();
                    //    tm.BreakdownID = null;
                    //    tm.ColorCode = "RED";
                    //    tm.CorrectedDate = prvmode.CorrectedDate;
                    //    tm.InsertedBy = Convert.ToInt32(Session["UserID"]);
                    //    tm.InsertedOn = DateTime.Now;
                    //    tm.IsCompleted = 0;
                    //    tm.IsDeleted = 0;
                    //    tm.LossCodeID = null;
                    //    tm.MachineID = MachineID;
                    //    tm.MacMode = "MNT";
                    //    tm.ModeType = "MNT";
                    //    tm.ModeTypeEnd = 0;
                    //    tm.StartIdle = 0;
                    //    tm.StartTime = tm.InsertedOn;
                    //    //tm.IsPiWeb = 0;
                    //    //tm.Sync = 0;         Ashok

                    //    _ServerContext.tbllivemodes.Add(tm);
                    //    _ServerContext.SaveChanges();





                    //}



                    flag = 1;//tocheck wheather the data is updated or inserted
                }
                else if (prvmode.ColorCode == "GREEN")
                {
                    Session["setuperror"] = "Machine is in Production. Cannot start the Maintenance. Stop the Prodcution, wait for 2 minutes and then start the Maintenance.";
                    ViewBag.SetupError = Session["setuperror"];
                    return RedirectToAction("DashboardProduction");
                }
                else if (prvmode.ModeType == "SETUP")
                {
                    Session["setuperror"] = "Machine is in Setting. Cannot start the Maintenance. Stop the Setting, wait for 2 minutes and then start the Maintenance.";
                    ViewBag.SetupError = Session["setuperror"];
                    return RedirectToAction("DashboardProduction");
                }

                #region UnLock the Machine
                var MacDet = _ServerContext.tblmachinedetails.Find(MachineID);
                //if (MacDet.MachineModelType == 1)
                //{
                //    AddFanucMachineWithConn AC = new AddFanucMachineWithConn(MacDet.IPAddress);
                //    AC.SetMachineUnlock((ushort)MacDet.MachineUnlockBit, (ushort)MacDet.MachineLockBit);
                //}
                #endregion

                #endregion
                if (flag == 1)
                {
                    #region Insert to Operator Dashboard

                    tblOperatorDashboard operatorDashboard = new tblOperatorDashboard();
                    DateTime CorrectedDateToDate = Convert.ToDateTime(correctedDate);
                    Random OperatorDashboardID = new Random();
                    //operatorDashboard.OperatorDashboardID = OperatorDashboardID.Next(1, 9999999);  //remove this line once Identity is setup
                    operatorDashboard.MachineID = Convert.ToInt32(Session["MachineID"]);
                    operatorDashboard.CorrectedDate = CorrectedDateToDate;
                    operatorDashboard.SlNo = _ServerContext.tblOperatorDashboards.Where(m => m.CorrectedDate == CorrectedDateToDate).Where(m => m.MachineID == MachineID).ToList().Count + 1;
                    operatorDashboard.MessageCode = "MNT";
                    //operatorDashboard.MessageCode = "IDLE";
                    operatorDashboard.MessageDescription = "Machine Started In Maintenance Mode";
                    operatorDashboard.MessageStartTime = DateTime.Now;
                    operatorDashboard.InsertedOn = DateTime.Now;
                    operatorDashboard.InsertedBy = Convert.ToInt32(Session["UserID"]);
                    operatorDashboard.IsDeleted = 0;
                    _ServerContext.tblOperatorDashboards.Add(operatorDashboard);
                    _ServerContext.SaveChanges();

                    #endregion

                    #region Update tblOperatorHeader ,

                    var operatorHeader = _ServerContext.tblOperatorHeaders.Where(m => m.MachineID == MachineID).OrderByDescending(m => m.InsertedOn).FirstOrDefault();
                    operatorHeader.MachineMode = "MNT";
                    //operatorHeader.MachineMode = "IDLE";
                    operatorHeader.ModifiedOn = DateTime.Now;
                    operatorHeader.ModifiedBy = 1;
                    _ServerContext.Entry(operatorHeader).State = System.Data.Entity.EntityState.Modified;
                    _ServerContext.SaveChanges();

                    #endregion
                }
            }
            else
            {
                Session["maintanceState"] = "Machine is in Setting. Cannot start the Maintenance. Stop the Setting, wait for 2 minutes and then start the Maintenance.";
                ViewBag.SetupError = Session["maintanceState"];
                return Redirect("MaintenanceWindow");
            }


            return RedirectToAction("DashboardProduction");
        }

        public ActionResult SettingProduction(int smValue = 0)
        {
            GetMode GM = new GetMode();
            //  String IPAddress = GM.GetIPAddressofTabSystem();
            int MachineID = Convert.ToInt32(Session["MachineID"]);
            //int MachineID = _ServerContext.tblmachinedetails.Where(m =>m.MachineID == 7&& m.IsDeleted == 0).Select(m => m.MachineID).First();

            Session["MachineID"] = MachineID;
            var HeaderDet = _ServerContext.tblOperatorHeaders.Where(m => m.MachineID == MachineID).SingleOrDefault();
            ViewBag.MachineMode = HeaderDet.MachineMode;
            ViewBag.MachineName = HeaderDet.tblmachinedetail.MachineDisplayName;
            ViewBag.TabStatus = HeaderDet.TabConnecStatus;
            ViewBag.ServerStatus = HeaderDet.ServerConnecStatus;
            ViewBag.PageName = "Setting";
            ViewBag.Shift = HeaderDet.Shift;

            var prvmode = _ServerContext.tbllivemodes.Where(m => m.MachineID == MachineID && m.ModeType == "SETUP" && m.ModeTypeEnd == 0)
                    .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
            if (prvmode != null)
            {
                ViewBag.SetUpStarted = "1";
                ViewBag.MachineMode = "Setting";
            }

            var prvmodeMaint = _ServerContext.tbllivemodes.Where(m => m.MachineID == MachineID && m.ModeType == "MNT" && m.ModeTypeEnd == 0)
                    .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
            if (prvmodeMaint != null)
            {
                ViewBag.MNTStarted = "1";
                ViewBag.MachineMode = "MNT";
            }

            var GetSetting = _ServerContext.tblSetupMaints.Where(m => m.MachineID == MachineID && m.IsStarted == 1 && m.IsSetup == 1 && m.IsCompleted == 0).FirstOrDefault();
            if (smValue != 2)
            {
                Session["message"] = "nullinsess";
            }

            return View(GetSetting);
        }

        public ActionResult EndSetting(int smValue = 0)
        {
            GetMode GM = new GetMode();
            //  String IPAddress = GM.GetIPAddressofTabSystem();
            int MachineID = Convert.ToInt32(Session["MachineID"]);
            //int MachineID = _ServerContext.tblmachinedetails.Where(m =>m.MachineID == 7&& m.IsDeleted == 0).Select(m => m.MachineID).First();

            Session["MachineID"] = MachineID;
            var GetSetting = _ServerContext.tblSetupMaints.Where(m => m.MachineID == MachineID && m.IsStarted == 1 && m.IsSetup == 1 && m.IsCompleted == 0).FirstOrDefault();

            if (GetSetting != null)
            {
                GetSetting.IsCompleted = 1;
                GetSetting.EndTime = System.DateTime.Now;
                DateTime ModeStartTime = DateTime.Now;
                try
                {
                    ModeStartTime = Convert.ToDateTime(_ServerContext.tbllivemodes.Where(m => m.ModeID == GetSetting.ModeID).Select(m => m.LossCodeEnteredTime).First());
                }
                catch (Exception e)
                {
                }

                var getLossDuration = _ServerContext.tbllivemodes.Where(m => m.StartTime > ModeStartTime && m.StartTime < GetSetting.EndTime).ToList();

                double LossDuration = 0;
                double MinorlossDuration = 0;
                double OpDuration = 0;
                double PowerOffDuration = 0;
                foreach (var ModeRow in getLossDuration)
                {
                    if (ModeRow.ModeType == "IDLE" && ModeRow.DurationInSec > 600)
                        LossDuration += (int)ModeRow.DurationInSec;
                    else if (ModeRow.ModeType == "IDLE" && ModeRow.DurationInSec < 600)
                        MinorlossDuration += (int)ModeRow.DurationInSec;
                    else if (ModeRow.ModeType == "POWEROFF")
                        PowerOffDuration += (int)ModeRow.DurationInSec;
                    else if (ModeRow.ModeType == "OPT")
                        OpDuration += (int)ModeRow.DurationInSec;
                }
                GetSetting.DurationInSec = (int)(OpDuration + MinorlossDuration);
                GetSetting.MinorLossTime = (int)MinorlossDuration;

                //GetSetting.ServerSetMainID
                _ServerContext.Entry(GetSetting).State = System.Data.Entity.EntityState.Modified;
                _ServerContext.SaveChanges();


                var GetTOD = _ServerContext.tblOperatorDashboards.Where(m => m.MachineID == MachineID && m.MessageCode == "Setting").OrderByDescending(m => m.InsertedOn).FirstOrDefault();
                GetTOD.MessageEndTime = System.DateTime.Now;
                GetTOD.TotalDurationinMin = Convert.ToInt32(System.DateTime.Now.Subtract(GetTOD.MessageStartTime).TotalMinutes);
                _ServerContext.Entry(GetTOD).State = System.Data.Entity.EntityState.Modified;
                _ServerContext.SaveChanges();

                var mode = _ServerContext.tbllivemodes.Where(m => m.MachineID == MachineID && m.ModeType == "SETUP" && m.ModeTypeEnd == 0)
                            .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
                if (mode != null)
                {
                    mode.IsCompleted = 1;
                    mode.ModeTypeEnd = 1;
                    mode.EndTime = DateTime.Now;
                    mode.DurationInSec = Convert.ToInt32(DateTime.Now.Subtract(Convert.ToDateTime(mode.StartTime)).TotalSeconds);
                    mode.StartIdle = 0;
                    mode.ModifiedOn = DateTime.Now;
                    _ServerContext.Entry(mode).State = System.Data.Entity.EntityState.Modified;
                    _ServerContext.SaveChanges();
                }


                #region Update tblOperatorHeader 

                //@Pavan as per mail ->  get the latest row according to the MachineID and CorrectedDate ->  there is no CorrectedDate column 
                var operatorHeader = _ServerContext.tblOperatorHeaders.Where(m => m.MachineID == MachineID).OrderByDescending(m => m.InsertedOn).FirstOrDefault();

                operatorHeader.MachineMode = "Production";
                //operatorHeader.MachineMode = "IDLE";
                operatorHeader.ModifiedOn = DateTime.Now;
                operatorHeader.ModifiedBy = 1;// get from session once these screens are integrated....

                _ServerContext.Entry(operatorHeader).State = System.Data.Entity.EntityState.Modified;
                _ServerContext.SaveChanges();

                #endregion

            }
            else
            {
                var GetSetupModeList = _ServerContext.tbllivemodes.Where(m => m.MachineID == MachineID && m.ModeType == "SETUP" && m.ModeTypeEnd == 0).ToList();

                foreach (var GetRow in GetSetupModeList)
                {
                    GetRow.ModeTypeEnd = 1;
                    GetRow.StartIdle = 0;
                    GetRow.IsCompleted = 1;
                    //GetRow.IsPiWeb = 0;
                    _ServerContext.Entry(GetRow).State = System.Data.Entity.EntityState.Modified;
                    _ServerContext.SaveChanges();
                }
            }


            #region UnLock the Machine
            var MacDet = _ServerContext.tblmachinedetails.Find(MachineID);
            //if (MacDet.MachineModelType == 1)
            //{
            //    AddFanucMachineWithConn AC = new AddFanucMachineWithConn(MacDet.IPAddress);
            //    AC.SetMachineUnlock((ushort)MacDet.MachineUnlockBit, (ushort)MacDet.MachineLockBit);
            //}
            #endregion
            if (smValue == 2)
            {
                return Redirect("MaintenanceWindow");
            }
            return RedirectToAction("DashboardProduction");
        }

        public ActionResult SettingWindow(FormCollection form, int LossSelect = 0)
        {
            GetMode GM = new GetMode();
            //  String IPAddress = GM.GetIPAddressofTabSystem();
            int MachineID = Convert.ToInt32(Session["MachineID"]);
            //int MachineID = _ServerContext.tblmachinedetails.Where(m =>m.MachineID == 7&& m.IsDeleted == 0).Select(m => m.MachineID).First();
            try
            {
                string sessionvar = Session["message"].ToString();
                if (sessionvar == null || sessionvar == string.Empty)
                {
                    Session["message"] = "";
                }
            }
            catch (Exception e)
            {
                Session["message"] = "";
            }
            Session["MachineID"] = MachineID;
            var isSetUpormaint = _ServerContext.tblSetupMaints.Where(m => m.MachineID == MachineID && m.IsStarted == 1 && m.IsSetup == 0 && m.IsCompleted == 0).ToList();
            if (isSetUpormaint.Count == 0)
            {
                Session["message"] = "";
                Session["messagevalue"] = "1";//for maintance
                var HeaderDet = _ServerContext.tblOperatorHeaders.Where(m => m.MachineID == MachineID).SingleOrDefault();
                ViewBag.MachineMode = HeaderDet.MachineMode;
                ViewBag.MachineName = HeaderDet.tblmachinedetail.MachineDisplayName;
                ViewBag.TabStatus = HeaderDet.TabConnecStatus;
                ViewBag.ServerStatus = HeaderDet.ServerConnecStatus;
                ViewBag.PageName = "Setting";
                ViewBag.Shift = HeaderDet.Shift;



                var prvmode = _ServerContext.tbllivemodes.Where(m => m.MachineID == MachineID && m.ModeType == "SETUP" && m.ModeTypeEnd == 0)
                        .OrderByDescending(m => m.ModeID).FirstOrDefault();
                if (prvmode != null)
                {
                    ViewBag.SetUpStarted = "1";
                    ViewBag.MachineMode = "Setting";
                    return RedirectToAction("IDLEPopup");
                }

                var prvmodeMaint = _ServerContext.tbllivemodes.Where(m => m.MachineID == MachineID && m.ModeType == "MNT" && m.ModeTypeEnd == 0)
                        .OrderByDescending(m => m.ModeID).FirstOrDefault();
                if (prvmodeMaint != null)
                {
                    ViewBag.MNTStarted = "1";
                    ViewBag.MachineMode = "MNT";
                }

                if (HeaderDet.MachineMode == "Setting")
                {
                    return RedirectToAction("SettingProduction");

                }
                else if (HeaderDet.MachineMode == "OPT")
                {
                    Session["message"] = "Machine is in Operating. Cannot start the Setting. ";
                    //return RedirectToAction("DashboardProduction");
                }
                if (Session["setuperror"] != null)
                {
                    Session["message"] = Session["setuperror"];
                    Session["setuperror"] = "";

                }
                var tblLossCodes = _ServerContext.tbllossescodes.Where(m => m.MessageType == "SETUP").ToList();
                ViewBag.lossCodes = tblLossCodes;

                if (LossSelect == 0)//first time ,show level 2
                {
                    int lossCodeID = tblLossCodes.Find(a => a.MessageType == "SETUP" && a.LossCodesLevel == 1).LossCodeID;
                    ViewBag.lossCodeID = lossCodeID;
                    ViewBag.level = 2;
                }
                else // show level 3
                {
                    int lossCodeID = LossSelect;
                    ViewBag.lossCodeID = lossCodeID;
                    ViewBag.level = 3;
                }

            }
            else
            {
                Session["message"] = "Machine is in Maintenance. Cannot start the Setting. Stop the Maintenance, wait for 2 minutes and then start the Maintenance.";
                Session["messagevalue"] = "0";//for settings
                return Redirect("MaintenanceProductionWindow?smValue=1");
            }
            return View();
        }

        public ActionResult IDLEPopup(FormCollection form, int LossSelect = 0)
        {
            GetMode GM = new GetMode();
            //  String IPAddress = GM.GetIPAddressofTabSystem();
            int MachineID = Convert.ToInt32(Session["MachineID"]);
            //int MachineID = _ServerContext.tblmachinedetails.Where(m =>m.MachineID == 7&& m.IsDeleted == 0).Select(m => m.MachineID).First();

            Session["MachineID"] = MachineID;


            var prvmode = _ServerContext.tbllivemodes.Where(m => m.MachineID == MachineID && m.ModeType == "SETUP" && m.ModeTypeEnd == 0)
                    .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
            if (prvmode != null)
            {
                ViewBag.SetUpStarted = "1";
                ViewBag.MachineMode = "Setting";
            }

            var prvmodeMaint = _ServerContext.tbllivemodes.Where(m => m.MachineID == MachineID && m.ModeType == "MNT" && m.ModeTypeEnd == 0)
                    .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
            if (prvmodeMaint != null)
            {
                ViewBag.MNTStarted = "1";
                ViewBag.MachineMode = "MNT";
            }

            var tblLossCodes = _ServerContext.tbllossescodes.Where(m => m.MessageType == "IDLE").ToList();
            ViewBag.lossCodes = tblLossCodes;

            if (LossSelect == 0)//first time ,show level 1
            {
                int lossCodeID = tblLossCodes.Find(a => a.MessageType == "IDLE").LossCodeID;
                ViewBag.lossCodeID = lossCodeID;
                ViewBag.level = 1;
            }
            else if (tblLossCodes.Where(m => m.LossCodesLevel1ID == LossSelect).ToList().Count > 0)// show level 2
            {
                int lossCodeID = LossSelect;
                ViewBag.lossCodeID = lossCodeID;
                ViewBag.level = 2;
            }
            else //show level 3
            {
                int lossCodeID = LossSelect;
                ViewBag.lossCodeID = lossCodeID;
                ViewBag.level = 3;
            }

            #region Update tblOperatorHeader 

            //get the latest row according to the MachineID and CorrectedDate ->  there is no CorrectedDate column 
            var operatorHeader = _ServerContext.tblOperatorHeaders.Where(m => m.MachineID == MachineID).OrderByDescending(m => m.InsertedOn).FirstOrDefault();

            operatorHeader.MachineMode = "IDLE";
            operatorHeader.ModifiedOn = DateTime.Now;
            operatorHeader.ModifiedBy = 1;// get from session once these screens are integrated....

            _ServerContext.Entry(operatorHeader).State = System.Data.Entity.EntityState.Modified;
            _ServerContext.SaveChanges();

            #endregion

            #region Lock the Machine
            var MacDet = _ServerContext.tblmachinedetails.Find(MachineID);

            //if (MacDet.MachineModelType == 1)
            //{
            //    AddFanucMachineWithConn AC = new AddFanucMachineWithConn(MacDet.IPAddress);
            //    //AC.setmachinelock(true, (ushort)MacDet.MachineLockBit, (ushort)MacDet.MachineIdleBit, (ushort)MacDet.MachineUnlockBit);
            //}

            #endregion

            var HeaderDet = _ServerContext.tblOperatorHeaders.Where(m => m.MachineID == MachineID).SingleOrDefault();
            ViewBag.MachineMode = HeaderDet.MachineMode;
            ViewBag.MachineName = HeaderDet.tblmachinedetail.MachineDisplayName;
            ViewBag.TabStatus = HeaderDet.TabConnecStatus;
            ViewBag.ServerStatus = HeaderDet.ServerConnecStatus;
            ViewBag.PageName = "IDLE";
            ViewBag.Shift = HeaderDet.Shift;

            return View();
        }

        public ActionResult SaveIdle(int LossSelect = 0)
        {

            //request came from level 2 and was a last node .Level 3  code will come as parameter.
            if (LossSelect == 0)
            {
                if (Request.QueryString["selectLoss"] != null) // Ideally not null, if null go to hell :) 
                    LossSelect = Convert.ToInt32(Request.QueryString["selectLoss"]);
            }

            #region Update tbllivemode

            GetMode GM = new GetMode();
            //  String IPAddress = GM.GetIPAddressofTabSystem();
            int machineID = Convert.ToInt32(Session["MachineID"]);
            //int machineID = _ServerContext.tblmachinedetails.Where(m =>m.MachineID == 7&& m.IsDeleted == 0).Select(m => m.MachineID).First();

            Session["MachineID"] = machineID;
            DateTime correctedDate = DateTime.Now;
            tbldaytiming StartTime = _ServerContext.tbldaytimings.Where(m => m.IsDeleted == 0).SingleOrDefault();
            TimeSpan Start = StartTime.StartTime;
            if (Start.Hours <= DateTime.Now.Hour)
            {
                correctedDate = DateTime.Now;
            }
            else
            {
                correctedDate = DateTime.Now.AddDays(-1);
            }
            int durationinsec = 0;
            //var correctedDate = "2017-11-17";   // Hard coding for time being
            string colorCode = "YELLOW";
            //Update tbllivemode with the Loss Code
            var mode = _ServerContext.tbllivemodes.Where(m => m.MachineID == machineID && m.ColorCode == colorCode && m.IsCompleted == 0)
                        .OrderByDescending(m => m.ModeID).FirstOrDefault();
            DateTime ModeStartTime = DateTime.Now;
            if (mode != null)
            {
                ModeStartTime = (DateTime)mode.StartTime;
                durationinsec = Convert.ToInt32(DateTime.Now.Subtract(ModeStartTime).TotalSeconds);
                mode.LossCodeID = LossSelect;
                mode.ModeType = "IDLE";
                mode.LossCodeEnteredTime = DateTime.Now;
                mode.LossCodeEnteredBy = "";
                mode.ModeTypeEnd = 1;
                mode.IsCompleted = 1;
                mode.StartIdle = 0;
                mode.EndTime = DateTime.Now;
                mode.DurationInSec = durationinsec;
                mode.ModifiedOn = DateTime.Now; // doing now for testing purpose
                mode.ModifiedBy = Convert.ToInt32(Session["UserID"]);
                //mode.IsPiWeb = 0;
                _ServerContext.Entry(mode).State = System.Data.Entity.EntityState.Modified;
                _ServerContext.SaveChanges();


                tbllivemode tm = new tbllivemode();
                tm.BreakdownID = mode.BreakdownID;
                tm.ColorCode = mode.ColorCode;
                tm.CorrectedDate = correctedDate;
                tm.InsertedBy = Convert.ToInt32(Session["UserID"]);
                tm.InsertedOn = DateTime.Now;
                tm.IsCompleted = 0;
                tm.IsDeleted = 0;
                tm.LossCodeID = null;
                tm.MachineID = machineID;
                tm.MacMode = mode.MacMode;
                tm.ModeType = "IDLE";
                tm.ModeTypeEnd = 0;
                tm.StartIdle = 0;
                tm.StartTime = tm.InsertedOn;
               // tm.IsPiWeb = 0;
                //tm.ServerModeID = ServerNewTM.ModeID;
                // tm.Sync = 0;  Ashok
                _ServerContext.tbllivemodes.Add(tm);
                _ServerContext.SaveChanges();

                //mode = _ServerContext.tbllivemodes.Where(m => m.MachineID == machineID && m.ColorCode == colorCode && m.IsCompleted == 0)
                //            .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
            }
            else
            {

            }
            #endregion

            #region UnLock the Machine
            var MacDet = _ServerContext.tblmachinedetails.Find(machineID);
            //if (MacDet.MachineModelType == 1)
            //{
            //    AddFanucMachineWithConn AC = new AddFanucMachineWithConn(MacDet.IPAddress);
            //    AC.SetMachineUnlock((ushort)MacDet.MachineUnlockBit, (ushort)MacDet.MachineLockBit);
            //}
            #endregion

            #region Insert to Operator Dashboard

            //Pre requsite before insertion ???

            tblOperatorDashboard operatorDashboard = new tblOperatorDashboard();
            DateTime CorrectedDateToDate = Convert.ToDateTime(correctedDate);
            Random OperatorDashboardID = new Random();
            int MachineID = Convert.ToInt32(Session["MachineID"]);
            //operatorDashboard.OperatorDashboardID = OperatorDashboardID.Next(1, 9999999);  //remove this line once Identity is setup
            operatorDashboard.MachineID = Convert.ToInt32(Session["MachineID"]);
            operatorDashboard.CorrectedDate = CorrectedDateToDate;
            operatorDashboard.SlNo = _ServerContext.tblOperatorDashboards.Where(m => m.CorrectedDate == CorrectedDateToDate).Where(m => m.MachineID == MachineID).ToList().Count + 1; //  @Pavan , I'm not sure what you meant here..  --Count++ from the tbloperatordashboard for the Machine and CorrectedDate
            operatorDashboard.MessageCode = "IDLE";
            operatorDashboard.MessageDescription = "Machine in IDLE Mode";
            operatorDashboard.MessageStartTime = ModeStartTime;
            operatorDashboard.MessageEndTime = DateTime.Now;
            operatorDashboard.TotalDurationinMin = Convert.ToInt32(DateTime.Now.Subtract(ModeStartTime).TotalMinutes);
            operatorDashboard.InsertedOn = DateTime.Now;
            operatorDashboard.InsertedBy = Convert.ToInt32(Session["UserID"]);
            //operatorDashboard.ModifiedOn = DateTime.Now;
            //operatorDashboard.ModifiedBy = 1;  // Session["UserID"]
            operatorDashboard.IsDeleted = 0;

            _ServerContext.tblOperatorDashboards.Add(operatorDashboard);
            _ServerContext.SaveChanges();

            #endregion

            #region Update tblOperatorHeader 

            //get the latest row according to the MachineID and CorrectedDate ->  there is no CorrectedDate column 
            var operatorHeader = _ServerContext.tblOperatorHeaders.Where(m => m.MachineID == MachineID).OrderByDescending(m => m.InsertedOn).FirstOrDefault();

            //operatorHeader.MachineMode = "Production";
            operatorHeader.MachineMode = "IDLE";
            operatorHeader.ModifiedOn = DateTime.Now;
            operatorHeader.ModifiedBy = 1;// get from session once these screens are integrated....

            _ServerContext.Entry(operatorHeader).State = System.Data.Entity.EntityState.Modified;
            _ServerContext.SaveChanges();


            #endregion

            return RedirectToAction("DashboardProduction");
        }

        public ActionResult ToolLife(int smlength = 0)
        {
            GetMode GM = new GetMode();
            //  String IPAddress = GM.GetIPAddressofTabSystem();
            int MachineID = Convert.ToInt32(Session["MachineID"]);
            //int MachineID = _ServerContext.tblmachinedetails.Where(m => m.MachineID == 7 && m.IsDeleted == 0).Select(m => m.MachineID).First();

            Session["MachineID"] = MachineID;
            var HeaderDet = _ServerContext.tblOperatorHeaders.Where(m => m.MachineID == MachineID).SingleOrDefault();
            ViewBag.MachineMode = HeaderDet.MachineMode;
            ViewBag.MachineName = HeaderDet.tblmachinedetail.MachineDisplayName;
            ViewBag.TabStatus = HeaderDet.TabConnecStatus;
            ViewBag.ServerStatus = HeaderDet.ServerConnecStatus;
            ViewBag.PageName = "Tool Life";
            ViewBag.Shift = HeaderDet.Shift;

            var prvmode = _ServerContext.tbllivemodes.Where(m => m.MachineID == MachineID && m.ModeType == "SETUP" && m.ModeTypeEnd == 0)
                    .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
            if (prvmode != null)
            {
                ViewBag.SetUpStarted = "1";
                ViewBag.MachineMode = "Setting";
            }

            var prvmodeMaint = _ServerContext.tbllivemodes.Where(m => m.MachineID == MachineID && m.ModeType == "MNT" && m.ModeTypeEnd == 0)
                    .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
            if (prvmodeMaint != null)
            {
                ViewBag.MNTStarted = "1";
                ViewBag.MachineMode = "MNT";
            }
            var toolLifeOPerator = _ServerContext.tbltoollifeoperators.Where(m => m.IsDeleted == 0 && m.IsReset == 0 && m.IsCompleted == false).OrderByDescending(m => m.toollifecounter).ToList();
            return View(toolLifeOPerator);
        }

        [OutputCache(NoStore = true, Location = System.Web.UI.OutputCacheLocation.Client, Duration = 5)] // every 3 sec
        public ActionResult ToolLifeData()
        {
            var toolLifeOPerator = _ServerContext.tbltoollifeoperators.Where(m => m.IsDeleted == 0 && m.IsReset == 0 && m.IsCompleted == false).OrderByDescending(m => m.toollifecounter).ToList();
            return PartialView("ToolLifeData", toolLifeOPerator);
        }

        public ActionResult ResetToolLifeCounter(string id, string resetreason)
        {
            int toolLifeopId = 0;
            if (id == "0" || id == "" || id == null)
            {
            }
            else
            {
                toolLifeopId = Convert.ToInt32(id);

                #region//updation in tbltoollife operator in server and tab

                var updateTabTool = _ServerContext.tbltoollifeoperators.Find(toolLifeopId);
                var updateServerTool = _ServerContext.tbltoollifeoperators.Find(updateTabTool.ToolIDAdmin);

                updateTabTool.IsReset = 1;
                _ServerContext.SaveChanges();
                updateServerTool.IsReset = 1;
                _ServerContext.SaveChanges();

                #endregion

                #region // insertion in tbltoollifeoperator in server and tab

                var itemTool = _ServerContext.tbltoollifeoperators.Find(toolLifeopId);
                var serVeritemTool = _ServerContext.tbltoollifeoperators.Find(itemTool.ToolIDAdmin);

                //addding in server
                tbltoollifeoperator ServertabTLO = new tbltoollifeoperator();

                ServertabTLO.MachineID = serVeritemTool.MachineID;
                ServertabTLO.ToolNo = serVeritemTool.ToolNo;
                ServertabTLO.ToolName = serVeritemTool.ToolName;
                ServertabTLO.ToolCTCode = serVeritemTool.ToolCTCode;
                ServertabTLO.StandardToolLife = serVeritemTool.StandardToolLife;
                ServertabTLO.toollifecounter = 0;
                ServertabTLO.InsertedOn = DateTime.Now;
                ServertabTLO.InsertedBy = serVeritemTool.InsertedBy;
                ServertabTLO.IsReset = 0;
                ServertabTLO.IsDeleted = 0;
                ServertabTLO.ResetCounter = 0;
                ServertabTLO.HMIID = serVeritemTool.HMIID;
                ServertabTLO.Sync = 1;
                ServertabTLO.ResetReason = resetreason;
                ServertabTLO.IsCompleted = false;
                _ServerContext.tbltoollifeoperators.Add(ServertabTLO);
                _ServerContext.SaveChanges();

                int serverTLOp = ServertabTLO.ToolLifeID;
                //adding to tab
                tbltoollifeoperator tabTLO = new tbltoollifeoperator();
                tabTLO.MachineID = itemTool.MachineID;
                tabTLO.ToolNo = itemTool.ToolNo;
                tabTLO.ToolName = itemTool.ToolName;
                tabTLO.ToolCTCode = itemTool.ToolCTCode;
                tabTLO.StandardToolLife = itemTool.StandardToolLife;
                tabTLO.toollifecounter = 0;
                tabTLO.InsertedOn = DateTime.Now;
                tabTLO.InsertedBy = itemTool.StandardToolLife;
                tabTLO.IsReset = 0;
                tabTLO.IsDeleted = 0;
                tabTLO.ResetCounter = 0;
                tabTLO.HMIID = itemTool.HMIID;
                tabTLO.HMIID = itemTool.HMIID;
                tabTLO.Sync = 1;
                tabTLO.ToolIDAdmin = serverTLOp;
                tabTLO.ResetReason = resetreason;
                tabTLO.IsCompleted = false;
                _ServerContext.tbltoollifeoperators.Add(tabTLO);
                _ServerContext.SaveChanges();
                #endregion
            }
            return RedirectToAction("ToolLife");
        }

        public ContentResult lastNodeCheck(int id)
        {
            var tblLossCodes = _ServerContext.tbllossescodes.ToList();

            if (tblLossCodes.Find(level => level.LossCodesLevel == 3 && level.LossCodesLevel2ID == id) == null) { return Content("true/" + id); }

            return Content("false/" + id);
        }

        public ContentResult lastNodeIdleCheck(int id, int lev)
        {
            var tblLossCodes = _ServerContext.tbllossescodes.ToList();

            if (lev == 1)
            {
                if (tblLossCodes.Find(level => level.LossCodesLevel == 2 && level.LossCodesLevel1ID == id) == null) { return Content("true/" + id); }
                else
                {
                    return Content("false/" + id);
                }
            }
            else
            {
                if (tblLossCodes.Find(level => level.LossCodesLevel == 3 && level.LossCodesLevel2ID == id) == null) { return Content("true/" + id); }

                return Content("false/" + id);
            }
        }

        public JsonResult CheckIdle()
        {
            _ServerContext.Database.CommandTimeout = 180;
            GetMode GM = new GetMode();
            int Data = 0;

            //  String IPAddress = GM.GetIPAddressofTabSystem();
            int MachineID = Convert.ToInt32(Session["MachineID"]);
            //int MachineID = _ServerContext.tblmachinedetails.Where(m =>m.MachineID == 7&& m.IsDeleted == 0).Select(m => m.MachineID).FirstOrDefault();

            Session["MachineID"] = MachineID;

            // GM.UpdateOperatorHeader(MachineID);
            // var toolCounter = _ServerContext.tbltoollifeoperators.Where(m => m.toollifecounter == m.StandardToolLife).Where(m => m.IsCompleted == false && m.IsReset == 0 && m.IsDeleted == 0).ToList();

            bool IdleStatus = GM.CheckIdleEntry(MachineID);
            if (IdleStatus)
                Data = 1;
            //int toolcount = toolCounter.Count();
            if (Data == 1)
            {
                Data = 1;
            }
            //else if (Data == 1 && toolcount > 0)
            //{
            //    Data = 1;
            //}
            //else if (Data != 1 && toolcount > 0)
            //{
            //    Data = 2;
            //}
            else
            {
                Data = 0;
            }
            return Json(Data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RefreshData()
        {
            List<string> retValList = new List<string>();
            GetMode GM = new GetMode();
            //  String IPAddress = GM.GetIPAddressofTabSystem();
            int MachineID = Convert.ToInt32(Session["MachineID"]);
            //int MachineID = _ServerContext.tblmachinedetails.Where(m =>m.MachineID == 7&& m.IsDeleted == 0).Select(m => m.MachineID).FirstOrDefault();

            Session["MachineID"] = MachineID;
            //GM.UpdateOperatorHeader(MachineID);
            retValList = new List<string>();

            var HeaderDet = _ServerContext.tblOperatorHeaders.Where(m => m.MachineID == MachineID).SingleOrDefault();
            if (HeaderDet != null)
            {
                retValList.Add(HeaderDet.tblmachinedetail.MachineDisplayName); //0
                retValList.Add(HeaderDet.TabConnecStatus); //1
                retValList.Add(HeaderDet.ServerConnecStatus); //2
                retValList.Add(HeaderDet.MachineMode); //3
                retValList.Add(HeaderDet.Shift); //4
                retValList.Add(MachineID.ToString()); //5
                                                      // retValList.Add(IPAddress); //6
                                                      //string errormessage = Session["setuperror"].ToString();
            }

            var result = new { RetVal = retValList };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SelectMachineAdmin(int PlantID = 0, int ShopID = 0, int CellID = 0, int WorkCenterID = 0)
        {
            ViewData["PlantID"] = new SelectList(_ServerContext.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", PlantID);
            ViewData["ShopID"] = new SelectList(_ServerContext.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID), "ShopID", "ShopName", ShopID);
            ViewData["CellID"] = new SelectList(_ServerContext.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID), "CellID", "CellName", CellID);
            ViewData["WorkCenterID"] = new SelectList(_ServerContext.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID && m.CellID == CellID && m.IsNormalWC == 0), "MachineID", "MachineDisplayName", WorkCenterID);
            return View();
        }

        public string autoPopulateOperatorEntry(string Prod_Order_No, string OperationNo)
        {
            string fgCodeOrderQty = "";
            string fgCode = "", ProductionOrderQty = "";
            var item = _ServerContext.tblProdPlanMasters.Where(m => m.Prod_Order_No == Prod_Order_No && m.OperationNo == OperationNo).FirstOrDefault();
            string status = "";
            if (item != null)
                status = item.Status;
            int stat = 0;// 1=SplitOrderQty 0=OrderQty
            if (status == "SPLT")
            {
                stat = 1;
            }
            else
            {
                int count = _ServerContext.tblworkorderentries.Where(m => m.Prod_Order_No == Prod_Order_No && m.isSplit == true).Count();

                if (count > 0)
                {
                    stat = 1;
                }
                else
                {
                    stat = 0;
                }
            }

            if (stat == 1)
            {
                ProductionOrderQty = item.SplitOrderQty.ToString();
            }
            else
            {
                if (item != null)
                    ProductionOrderQty = item.OrderQty.ToString();
            }
            if (item != null)
                fgCode = item.FGCode;
            fgCodeOrderQty = fgCode + '#' + ProductionOrderQty;

            return fgCodeOrderQty;
        }

        //[HttpPost]
        //public string ServerPing()
        //{
        //    string Status = "Disconnected";
        //    GetMode GM = new GetMode();
        //    Ping ping = new Ping();
        //    //String TabIPAddress = GM.GetIPAddressofTabSystem();
        //    var MachineDetails = _ServerContext.tblmachinedetails.Where(m =>m.MachineID == 7&& m.IsDeleted == 0).FirstOrDefault();

        //    try
        //    {
        //        PingReply pingresult = ping.Send(MachineDetails.IPAddress);
        //        if (pingresult.Status.ToString() == "Success")
        //        {
        //            Status = "Connected";
        //        }
        //    }
        //    catch
        //    {
        //        Status = "Disconnected";
        //    }
        //    return Status;
        //}
    }

    public class FromCollection
    {
        public void GetOperatorHeader()
        {

        }
    }
}
