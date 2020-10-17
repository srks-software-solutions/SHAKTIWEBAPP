using System;
using System.Linq;
using System.Web.Mvc;
using SRKSDemo.Server_Model;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Data.Entity;
using SRKSDemo.Models;
using static SRKSDemo.GetMode;

namespace SRKSDemo.Controllers
{
    public class OperatorEntry1Controller : Controller
    {
        public i_facility_shaktiEntities1 _UWcontext = new i_facility_shaktiEntities1();
        //public CCSOEEServerEntities _ServerContext = new CCSOEEServerEntities();
        public i_facility_shaktiEntities1 _ServerContext = new i_facility_shaktiEntities1();

        // GET: OperatorEntry
        //public ActionResult SaveSetting(int LossSelect = 0)
        //{
        //    //request came from level 2 and was a last node .Level 3  code will come as parameter.
        //    if (LossSelect == 0)
        //    {
        //        if (Request.QueryString["selectLoss"] != null) // Ideally not null, if null go to hell :) 
        //            LossSelect = Convert.ToInt32(Request.QueryString["selectLoss"]);
        //    }

        //    #region Update  TblMode
        //    GetMode GM = new GetMode();
        //    String IPAddress = GM.GetIPAddressofTabSystem();
        //    var celliddet = Session["CellID"];
        //    int cellid = Convert.ToInt32(celliddet);
        //    //int machineID = _UWcontext.tblmachinedetails.Where(m => m.TabIPAddress == IPAddress && m.IsDeleted == 0).Select(m => m.MachineID).First();
        //    var machinedet= _UWcontext.tblmachinedetails.Where(m => m.CellID == cellid && m.IsDeleted == 0 && m.IsLastMachine==1).ToList();
        //    foreach (var item in machinedet)
        //    {
        //        int machineID = item.MachineID;

        //        Session["MachineID"] = machineID;
        //        DateTime correctedDate = DateTime.Now;
        //        I_Facility.ServerModel.tbldaytiming StartTime = _UWcontext.tbldaytimings.Where(m => m.IsDeleted == 0).SingleOrDefault();
        //        TimeSpan Start = StartTime.StartTime;
        //        if (Start.Hours <= DateTime.Now.Hour)
        //        {
        //            correctedDate = DateTime.Now.Date;
        //        }
        //        else
        //        {
        //            correctedDate = DateTime.Now.AddDays(-1).Date;
        //        }
        //        string colorCode = "YELLOW";
        //        //to check whether the macine is in setup mode where issetup will be in 1  
        //        var isSetUpormaint = _UWcontext.tblsetupmaints.Where(m => m.MachineID == machineID && m.IsStarted == 1 && m.IsSetup == 0 && m.IsCompleted == 0).ToList();
        //        if (isSetUpormaint.Count == 0)
        //        {
        //            var mode = _UWcontext.tblmodes.Where(m => m.MachineID == machineID && m.ColorCode == colorCode && m.IsCompleted == 0 && m.ModeType == "IDLE" && m.ModeTypeEnd == 0)
        //                    .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
        //            if (mode != null)
        //            {
        //                mode.LossCodeID = LossSelect;
        //                mode.ModeType = "SETUP";
        //                mode.StartIdle = 0;
        //                mode.ModifiedOn = DateTime.Now;
        //                mode.IsCompleted = 1;
        //                mode.LossCodeEnteredTime = DateTime.Now;
        //                _UWcontext.Entry(mode).State = EntityState.Modified;
        //                _UWcontext.SaveChanges();

        //                I_Facility.ServerModel.tblsetupmaint SM = new I_Facility.ServerModel.tblsetupmaint();
        //                SM.IsCompleted = 0;
        //                SM.IsSetup = 1;
        //                SM.IsStarted = 1;
        //                SM.LossCodeID = LossSelect;
        //                SM.MachineID = machineID;
        //                SM.ModeId = mode.ModeId;
        //                SM.StartTime = (DateTime)mode.StartTime;
        //                //SM.Sync = 0;
        //                _UWcontext.tblsetupmaints.Add(SM);
        //                _UWcontext.SaveChanges();

        //                I_Facility.ServerModel.tblmode tm = new I_Facility.ServerModel.tblmode();
        //                tm.BreakdownID = null;
        //                tm.ColorCode = "YELLOW";
        //                tm.CorrectedDate = correctedDate;
        //                tm.InsertedBy = Convert.ToInt32(Session["UserID"]);
        //                tm.InsertedOn = DateTime.Now;
        //                tm.IsCompleted = 0;
        //                tm.IsDeleted = 0;
        //                tm.LossCodeID = null;
        //                tm.MachineID = machineID;
        //                tm.MacMode = "IDLE";
        //                tm.ModeType = "IDLE";
        //                tm.ModeTypeEnd = 0;
        //                tm.StartIdle = 0;
        //                tm.StartTime = tm.InsertedOn;

        //                //tm.Sync = 0;

        //                _UWcontext.tblmodes.Add(tm);
        //                _UWcontext.SaveChanges();


        //                #region UnLock the Machine
        //                var MacDet = _UWcontext.tblmachinedetails.Find(machineID);
        //                if (MacDet.MachineModelType == 1)
        //                {
        //                    AddFanucMachineWithConn AC = new AddFanucMachineWithConn(MacDet.IPAddress);
        //                    AC.SetMachineUnlock((ushort)MacDet.MachineUnlockBit, (ushort)MacDet.MachineLockBit);
        //                }
        //                #endregion

        //                #endregion

        //                #region Insert to Operator Dashboard

        //                I_Facility.ServerModel.tbloperatordashboard operatorDashboard = new I_Facility.ServerModel.tbloperatordashboard();
        //                DateTime CorrectedDateToDate = Convert.ToDateTime(correctedDate);
        //                Random OperatorDashboardID = new Random();
        //                operatorDashboard.MachineID = Convert.ToInt32(Session["MachineID"]);
        //                operatorDashboard.CorrectedDate = CorrectedDateToDate;
        //                operatorDashboard.SlNo = _UWcontext.tbloperatordashboards.Where(m => m.CorrectedDate == CorrectedDateToDate).Where(m => m.MachineID == machineID).ToList().Count + 1; //  @Pavan , I'm not sure what you meant here..  --Count++ from the tbloperatordashboard for the Machine and CorrectedDate
        //                operatorDashboard.MessageCode = "Setting";
        //                operatorDashboard.MessageDescription = "Machine Started In Setting Mode";
        //                operatorDashboard.MessageStartTime = DateTime.Now;
        //                operatorDashboard.InsertedOn = DateTime.Now;
        //                operatorDashboard.InsertedBy = Convert.ToInt32(Session["UserID"]);
        //                operatorDashboard.IsDeleted = 0;
        //                _UWcontext.tbloperatordashboards.Add(operatorDashboard);
        //                _UWcontext.SaveChanges();

        //                #endregion

        //                #region Update tblOperatorHeader 

        //                var operatorHeader = _UWcontext.tbloperatorheaders.Where(m => m.MachineID == machineID).OrderByDescending(m => m.InsertedOn).FirstOrDefault();
        //                operatorHeader.MachineMode = "Setting";
        //                operatorHeader.ModifiedOn = DateTime.Now;
        //                operatorHeader.ModifiedBy = 1;
        //                _UWcontext.Entry(operatorHeader).State = EntityState.Modified;
        //                _UWcontext.SaveChanges();

        //                #endregion
        //                return RedirectToAction("DashboardProduction");
        //            }
        //            else
        //            {
        //                Session["setuperror"] = "Machine is not in IDLE state, Cannot Start the Setting on the Machine.";

        //                return Redirect("SettingWindow");
        //            }
        //        }
        //        else
        //        {
        //            Session["setuperror"] = "Machine is in Maintance State, Cannot Start the Setting on the Machine.";

        //            return Redirect("SettingWindow");
        //        }
        //    }

        //}

        public ActionResult DashboardProduction()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            // TempData["toaster_success"] = "pinging messages to server";
            GetMode GM = new GetMode();
            String IPAddress = GM.GetIPAddressofTabSystem();
            List<OperatorDashboard> opobj = new List<OperatorDashboard>();

            int MachineID = 0;
            DateTime ddate = DateTime.Now;
            // int MachineID = _UWcontext.tblmachinedetails.Where(m => m.TabIPAddress == IPAddress && m.IsDeleted == 0).Select(m => m.MachineID).First();
            var celliddet = Session["CellID"];
            int cellid = Convert.ToInt32(celliddet);
            var celldet = _UWcontext.tblcells.Find(cellid);
            if (celldet != null)
            {
                ViewBag.CellName = celldet.CellName;
            }
            //int machineID = _UWcontext.tblmachinedetails.Where(m => m.TabIPAddress == IPAddress && m.IsDeleted == 0).Select(m => m.MachineID).First();
            var machinedet = _UWcontext.tblmachinedetails.Where(m => m.CellID == cellid && m.IsDeleted == 0).ToList();
            foreach (var item in machinedet)
            {
                MachineID = item.MachineID;

                Session["MachineID"] = MachineID;
                //int MachineID = Convert.ToInt32(Session["MachineID"]);
                var HeaderDet = _UWcontext.tblOperatorHeaders.Where(m => m.MachineID == MachineID).OrderByDescending(m => m.InsertedOn).SingleOrDefault();
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
                SRKSDemo.Server_Model.tbldaytiming StartTime = _UWcontext.tbldaytimings.Where(m => m.IsDeleted == 0).SingleOrDefault();
                TimeSpan Start = StartTime.StartTime;
                if (Start.Hours <= DateTime.Now.Hour)
                {
                    correctedDate = DateTime.Now.Date;
                }
                else
                {
                    correctedDate = DateTime.Now.AddDays(-1).Date;
                }

                var prvmode = _UWcontext.tblmodes.Where(m => m.MachineID == MachineID && m.ModeType == "SETUP" && m.ModeTypeEnd == 0)
                        .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
                if (prvmode != null)
                {
                    ViewBag.SetUpStarted = "1";
                    ViewBag.MachineMode = "Setting";
                }

                var prvmodeMaint = _UWcontext.tblmodes.Where(m => m.MachineID == MachineID && m.ModeType == "MNT" && m.ModeTypeEnd == 0)
                        .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
                if (prvmodeMaint != null)
                {
                    ViewBag.MNTStarted = "1";
                    ViewBag.MachineMode = "MNT";
                }
                var GetDashboardData = _UWcontext.tblOperatorDashboards.Where(m => m.MachineID == MachineID && m.CorrectedDate == ddate.Date).OrderByDescending(m => m.InsertedOn).FirstOrDefault();
                if (GetDashboardData != null)
                {
                    var mach = _UWcontext.tblmachinedetails.Find(MachineID);
                    SRKSDemo.Server_Model.tblOperatorDashboard TOD = new SRKSDemo.Server_Model.tblOperatorDashboard();
                    OperatorDashboard OD = new OperatorDashboard();
                    //OD.OpDashboardList = GetDashboardData;
                    OD.OpDashboard = GetDashboardData;
                    //OD.machinename = mach.MachineName;
                    opobj.Add(OD);
                }
            }


            return View(opobj);
        }

        public ActionResult DashboardSetting()
        {
            return View();
        }
        //[HttpGet]
        public ActionResult EntryWindow(int machineId, string operatorId)
        {
            var opDet = _UWcontext.tblworkorderentries.Where(m => m.MachineID == machineId && m.OperatorID == operatorId && m.IsStarted == 1 && m.IsFinished == 0).OrderByDescending(m => m.HMIID).FirstOrDefault();
            return View(opDet);
        }
        //public ActionResult EntryWindow(I_Facility.ServerModel.tblworkorderentry WOE)

        //[HttpPost]

        [HttpPost]
        public string EntryWindowData(string[] Operator, int PartNo, int Shift)
        {
            string result = "success";
            GetMode GM = new GetMode();
            String IPAddress = GM.GetIPAddressofTabSystem();

            //int MachineID = _UWcontext.tblmachinedetails.Where(m => m.TabIPAddress == IPAddress && m.IsDeleted == 0).Select(m => m.MachineID).First();
            var celliddet = Session["CellID"];
            int cellid = Convert.ToInt32(celliddet);
            var celldet = _UWcontext.tblcells.Find(cellid);
            if (celldet != null)
            {
                ViewBag.CellName = celldet.CellName;
            }

            var machinedet = _UWcontext.tblmachinedetails.Where(m => m.CellID == cellid && m.IsDeleted == 0 && m.IsLastMachine == 1).ToList();
            foreach (var item1 in machinedet)
            {
                int MachineID = item1.MachineID;

                Session["MachineID"] = MachineID;
                var HeaderDet = _UWcontext.tblOperatorHeaders.Where(m => m.MachineID == MachineID).SingleOrDefault();
                if (HeaderDet != null)
                {
                    ViewBag.MachineMode = HeaderDet.MachineMode;
                    ViewBag.MachineName = HeaderDet.tblmachinedetail.MachineDisplayName;
                    ViewBag.TabStatus = HeaderDet.TabConnecStatus;
                    ViewBag.ServerStatus = HeaderDet.ServerConnecStatus;
                    ViewBag.PageName = "Operator Entry";
                    ViewBag.Shift = HeaderDet.Shift;
                }
                var prvmode = _UWcontext.tblmodes.Where(m => m.MachineID == MachineID && m.ModeType == "SETUP" && m.ModeTypeEnd == 0)
                        .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
                if (prvmode != null)
                {
                    ViewBag.SetUpStarted = "1";
                    ViewBag.MachineMode = "Setting";
                }

                var prvmodeMaint = _UWcontext.tblmodes.Where(m => m.MachineID == MachineID && m.ModeType == "MNT" && m.ModeTypeEnd == 0)
                        .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
                if (prvmodeMaint != null)
                {
                    ViewBag.MNTStarted = "1";
                    ViewBag.MachineMode = "MNT";
                }

                DateTime correctedDate = DateTime.Now;
                SRKSDemo.Server_Model.tbldaytiming StartTime = _UWcontext.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
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

                //var GetProcessQty = _UWcontext.tblworkorderentries.Where(m => m.Prod_Order_No == WOE.Prod_Order_No && m.OperationNo == WOE.OperationNo).OrderByDescending(m => m.HMIID).FirstOrDefault();
                //if (GetProcessQty != null)
                //{
                //    PrvTotalQty = GetProcessQty.Total_Qty;
                //    PrvProcessQty = GetProcessQty.ProcessQty;
                //}

                #region check split

                //var item = _ServerContext.tblprodplanmasters.Where(m => m.Prod_Order_No == WOE.Prod_Order_No && m.OperationNo == WOE.OperationNo).FirstOrDefault();
                //int stat = 0;// 1=SplitOrderQty 0=OrderQty
                //if (item != null)
                //{
                //    string status = item.Status;
                //    if (status == "SPLT")
                //    {
                //        stat = 1;
                //    }
                //}
                #endregion
                string OperatorID = "";
                int count = Operator.Count();
                for (int i = 0; i < count; i++)
                {
                    OperatorID = OperatorID + Operator[i] + ",";
                }
                var Partnumdet = _UWcontext.tblcellparts.Find(PartNo);


                SRKSDemo.Server_Model.tblworkorderentry WOEntry = new SRKSDemo.Server_Model.tblworkorderentry();
                WOEntry.MachineID = MachineID;
                //WOEntry.OperationNo = WOE.OperationNo;
                WOEntry.OperationNo = "0";
                WOEntry.OperatorID = OperatorID;
                //WOEntry.PartNo = Convert.ToString(PartNo);
                if (Partnumdet != null)
                {
                    WOEntry.PartNo = Partnumdet.partNo;
                }
                WOEntry.PEStartTime = DateTime.Now;
                WOEntry.Prod_Order_No = "0";
                WOEntry.ScrapQty = 0;
                WOEntry.ShiftID = Shift;
                WOEntry.Total_Qty = 0;
                WOEntry.WOStart = DateTime.Now;
                WOEntry.Yield_Qty = 0;
                WOEntry.IsStarted = 1;
                WOEntry.CorrectedDate = Convert.ToDateTime(correctedDate.ToString("yyyy-MM-dd"));
                WOEntry.ProcessQty = PrvProcessQty + PrvTotalQty;
                WOEntry.FGCode = Convert.ToString(PartNo);
                WOEntry.ProdOrderQty = 0;
                WOEntry.Status = 0;
                WOEntry.IsFinished = 0;
                WOEntry.isWorkOrder = 1;
                WOEntry.IsMultiWO = 0;
                WOEntry.IsFlag = 0;
                WOEntry.IsHold = 0;
                WOEntry.isSplit = false;
                WOEntry.CellID = cellid;

                //WOEntry.MachineID = MachineID;
                ////WOEntry.OperationNo = WOE.OperationNo;
                //WOEntry.OperationNo = "0";
                //WOEntry.OperatorID = WOE.OperatorID;
                //WOEntry.PartNo = PartNo;
                //WOEntry.PEStartTime = DateTime.Now;
                //WOEntry.Prod_Order_No = WOE.Prod_Order_No;
                //WOEntry.ScrapQty = 0;
                //WOEntry.ShiftID = WOE.ShiftID;
                //WOEntry.Total_Qty = 0;
                //WOEntry.WOStart = DateTime.Now;
                //WOEntry.Yield_Qty = 0;
                //WOEntry.IsStarted = 1;
                //WOEntry.CorrectedDate = correctedDate;
                //WOEntry.ProcessQty = PrvProcessQty + PrvTotalQty;
                //WOEntry.FGCode = WOE.FGCode;
                //WOEntry.ProdOrderQty = WOE.ProdOrderQty;
                //WOEntry.Status = 0;
                //WOEntry.IsFinished = 0;
                //WOEntry.isWorkOrder = 1;
                //WOEntry.IsMultiWO = 0;
                //WOEntry.IsFlag = 0;
                //WOEntry.IsHold = 0;
                // WOEntry.SyncInsert = 0;
                //if (stat == 1)
                //{
                //    WOEntry.isSplit = true;
                //}
                //else
                //    WOEntry.isSplit = false;
                _UWcontext.tblworkorderentries.Add(WOEntry);
                _UWcontext.SaveChanges();

                //int hmmid = WOEntry.HMIID;

                //string operationNo = WOE.OperationNo;
                //string Part_No = WOE.FGCode;

                //var toolObj = _ServerContext.tblstdtoollives.Where(m => m.Part_No == Part_No && m.OperationNo == operationNo && m.IsDeleted == false).ToList();

                //foreach (var itemTool in toolObj)
                //{
                //    int ToolLifeCount = 0;
                //    GetToolLifeCounter(Part_No, operationNo, itemTool.ToolNo, ref ToolLifeCount);
                //    //adding new row server
                //    tbltoollifeoperator ServertabTLO = new tbltoollifeoperator();
                //    ServertabTLO.MachineID = MachineID;
                //    ServertabTLO.ToolNo = itemTool.ToolNo;
                //    ServertabTLO.ToolName = itemTool.ToolName;
                //    //ServertabTLO.ToolPrtCode = itemTool.PrtCode;
                //    ServertabTLO.StandardToolLife = itemTool.StdToolLife;
                //    ServertabTLO.toollifecounter = ToolLifeCount;
                //    ServertabTLO.InsertedOn = DateTime.Now;
                //    ServertabTLO.InsertedBy = Convert.ToInt32(WOE.OperatorID);
                //    ServertabTLO.IsReset = 0;
                //    ServertabTLO.IsDeleted = 0;
                //    ServertabTLO.ResetCounter = 0;
                //    ServertabTLO.Sync = 1;
                //    ServertabTLO.IsCompleted = false;
                //    ServertabTLO.IsCycleStart = false;
                //    _ServerContext.tbltoollifeoperators.Add(ServertabTLO);
                //    _ServerContext.SaveChanges();

                //    int serverTLOp = ServertabTLO.ToolLifeID;
                //    //adding to tab
                //    tbltoollifeoperator tabTLO = new tbltoollifeoperator();
                //    tabTLO.MachineID = MachineID;
                //    tabTLO.ToolNo = itemTool.ToolNo;
                //    tabTLO.ToolName = itemTool.ToolName;
                //    // tabTLO.ToolPrtCode = itemTool.PrtCode;
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
                //    _UWcontext.tbltoollifeoperators.Add(tabTLO);
                //    _UWcontext.SaveChanges();
                //}
            }
            return result;
            // return RedirectToAction("EntryWindow");
        }

        //public JsonResult RejectReasonData(int Shift)
        //{
        //    int curr = DateTime.Now.Hour;
        //    var celliddet = Session["CellID"];
        //    int cellid = Convert.ToInt32(celliddet);
        //    List<Reject> RLObj = new List<Reject>();
        //    string CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
        //    var ShiftDet = _UWcontext.shift_master.Where(m => m.IsDeleted == 0 && m.ShiftID == Shift).FirstOrDefault();
        //    TimeSpan? Stime = ShiftDet.StartTime;
        //    TimeSpan? ETime = ShiftDet.EndTime;
        //    if (Shift != 3 && Shift != 2)
        //    {
        //        DateTime TempStartTime = Convert.ToDateTime(CorrectedDate + " " + Stime);
        //        DateTime TempEndTime = Convert.ToDateTime(CorrectedDate + " " + ETime);
        //        DateTime TempIncTime = TempStartTime;

        //        do
        //        {
        //            int EndHour = TempIncTime.AddHours(1).Hour;
        //            Reject Robj = new Reject();
        //            Robj.Shifttime = TempIncTime.ToString("HH:mm:ss") + "-" + TempIncTime.AddHours(1).ToString("HH:mm:ss");
        //            List<Rejectdata> RDList = new List<Rejectdata>();
        //            var RejectDet = _UWcontext.tblrejectreasons.Where(m => m.isDeleted == 0 && m.Cellid == cellid).ToList();
        //            foreach (var item in RejectDet)
        //            {
        //                Rejectdata RDobj = new Rejectdata();
        //                RDobj.RejectReasonID = item.RID;
        //                RDobj.RejectReason = item.RejectName;
        //                RDList.Add(RDobj);
        //            }
        //            Robj.RList = RDList;
        //            if (TempIncTime.Hour == curr)
        //            {
        //                Robj.isEnable = true;
        //            }
        //            else
        //            {
        //                Robj.isEnable = false;
        //            }
        //            RLObj.Add(Robj);
        //            TempIncTime = TempIncTime.AddHours(1);
        //        } while (TempIncTime <= TempEndTime);
        //    }
        //    else
        //    {
        //        DateTime TempStartTime = Convert.ToDateTime(CorrectedDate + " " + Stime);
        //        DateTime TempEndTime = Convert.ToDateTime(CorrectedDate + " " + ETime);
        //        TempEndTime = TempEndTime.AddDays(1);
        //        DateTime TempIncTime = TempStartTime;
        //        do
        //        {
        //            Reject Robj = new Reject();
        //            Robj.Shifttime = TempIncTime.ToString("HH:mm:ss") + "-" + TempIncTime.AddHours(1).ToString("HH:mm:ss");
        //            List<Rejectdata> RDList = new List<Rejectdata>();
        //            var RejectDet = _UWcontext.tblrejectreasons.Where(m => m.isDeleted == 0 && m.Cellid == cellid).ToList();
        //            foreach (var item in RejectDet)
        //            {
        //                Rejectdata RDobj = new Rejectdata();
        //                RDobj.RejectReasonID = item.RID;
        //                RDobj.RejectReason = item.RejectName;
        //                RDList.Add(RDobj);
        //            }
        //            Robj.RList = RDList;
        //            if (TempIncTime.Hour == curr)
        //            {
        //                Robj.isEnable = true;
        //            }
        //            else
        //            {
        //                Robj.isEnable = false;
        //            }
        //            RLObj.Add(Robj);
        //            TempIncTime = TempIncTime.AddHours(1);
        //        } while (TempIncTime <= TempEndTime);
        //    }
        //    RLObj.OrderBy(m => m.Shifttime);
        //    return Json(RLObj, JsonRequestBehavior.AllowGet);
        //}

        public JsonResult RejectReasonDataPrint(int Shift)
        {
            //Shift = 3;
            int curr = DateTime.Now.Hour;
            List<Reject> RLObj = new List<Reject>();
            string CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            //CorrectedDate = "2019-06-28";
            var ShiftDet = _UWcontext.shift_master.Where(m => m.IsDeleted == 0 && m.ShiftID == Shift).FirstOrDefault();
            TimeSpan? Stime = ShiftDet.StartTime;
            TimeSpan? ETime = ShiftDet.EndTime;
            if (Shift != 3 )
            {
                DateTime TempStartTime = Convert.ToDateTime(CorrectedDate + " " + Stime);
                DateTime TempEndTime = Convert.ToDateTime(CorrectedDate + " " + ETime);
                DateTime TempIncTime = TempStartTime;

                do
                {
                    int EndHour = TempIncTime.AddHours(1).Hour;
                    DateTime TempIncTimeEnd = TempIncTime.AddHours(1);
                    Reject Robj = new Reject();
                    Robj.Shifttime = TempIncTime.ToString("HH:mm:ss") + "-" + TempIncTime.AddHours(1).ToString("HH:mm:ss");
                    List<Rejectdata> RDList = new List<Rejectdata>();
                    var RejectDet = _UWcontext.tblrejectreasons.Where(m => m.isDeleted == 0 /*&& m.Cellid == cellid*/).ToList();
                    foreach (var item in RejectDet)
                    {

                        var RejQtyDet = _UWcontext.tblrejectqties.Where(m => m.ShiftID == Shift && m.CorrectedDate == CorrectedDate && m.StartTime == TempIncTime && m.EndTime == TempIncTimeEnd && m.RID == item.RID).FirstOrDefault();
                        int? rval = 0;
                        if (RejQtyDet != null)
                        {
                            rval = RejQtyDet.RejectQty;
                        }
                        Rejectdata RDobj = new Rejectdata();
                        RDobj.RejectReasonID = item.RID;
                        RDobj.RejectReason = item.RejectName;
                        RDobj.RejectVal = rval;
                        RDList.Add(RDobj);
                    }
                    Robj.RList = RDList;
                    if (TempIncTime.Hour == curr)
                    {
                        Robj.isEnable = true;
                    }
                    else
                    {
                        Robj.isEnable = true;
                    }
                    RLObj.Add(Robj);
                    TempIncTime = TempIncTime.AddHours(1);
                } while (TempIncTime < TempEndTime);
            }
            else
            {
                DateTime TempStartTime = Convert.ToDateTime(CorrectedDate + " " + Stime);
                DateTime TempEndTime = Convert.ToDateTime(CorrectedDate + " " + ETime);
                TempEndTime = TempEndTime.AddDays(1);
                DateTime TempIncTime = TempStartTime;
                do
                {
                    Reject Robj = new Reject();
                    Robj.Shifttime = TempIncTime.ToString("HH:mm:ss") + "-" + TempIncTime.AddHours(1).ToString("HH:mm:ss");
                    List<Rejectdata> RDList = new List<Rejectdata>();
                    var RejectDet = _UWcontext.tblrejectreasons.Where(m => m.isDeleted == 0/* && m.Cellid == cellid*/).ToList();
                    foreach (var item in RejectDet)
                    {
                        DateTime tempEndtime = TempIncTime.AddHours(1);
                        var RejQtyDet = _UWcontext.tblrejectqties.Where(m => m.ShiftID == Shift && m.CorrectedDate == CorrectedDate && m.StartTime == TempIncTime && m.EndTime == tempEndtime && m.RID == item.RID).FirstOrDefault();
                        int? rval = 0;
                        if (RejQtyDet != null)
                        {
                            rval = RejQtyDet.RejectQty;
                        }
                        Rejectdata RDobj = new Rejectdata();
                        RDobj.RejectReasonID = item.RID;
                        RDobj.RejectReason = item.RejectName;
                        RDobj.RejectVal = rval;
                        RDList.Add(RDobj);
                    }
                    Robj.RList = RDList;
                    if (TempIncTime.Hour == curr)
                    {
                        Robj.isEnable = true;
                    }
                    else
                    {
                        Robj.isEnable = true;
                    }
                    RLObj.Add(Robj);
                    TempIncTime = TempIncTime.AddHours(1);
                } while (TempIncTime < TempEndTime);
            }
            RLObj.OrderBy(m => m.Shifttime);
            return Json(RLObj, JsonRequestBehavior.AllowGet);
        }

        public string StoreRejectVal(int MachineID, int Shift, string[] rejectval, string[] ReasonID,string value)
        {
            int i = 0;
            //int curr = DateTime.Now.Hour;
            string[] ids = value.Split('-');
            string correcteddate = DateTime.Now.ToString("yyyy-MM-dd");
            DateTime Stime = Convert.ToDateTime(correcteddate+" "+ids[0]);
            DateTime Etime = Convert.ToDateTime(correcteddate+" "+ids[1]);
           
            //string time = ":15:00";
            //if (Shift == 1)
            //{
            //    time = ":15:00";
            //}
            //else if (Shift == 2)
            //{
            //    time = ":45:00";
            //}
            //else if (Shift == 3)
            //    time = ":15:00";

            DateTime corrDate = Convert.ToDateTime(correcteddate);
            int wodet = _UWcontext.tblworkorderentries.Where(m => m.ShiftID == Shift && m.CorrectedDate == corrDate && m.MachineID == MachineID).Select(m => m.HMIID).FirstOrDefault();
            // var rejectdet = _UWcontext.tblrejectreasons.Where(m => m.isDeleted == 0 && m.Cellid == cellid).Select(m => m.RID).ToList();

            var rejectdet = ReasonID.ToList();
            int count = rejectdet.Count;
            foreach (string item in rejectdet)
            {
                int RID = Convert.ToInt32(item);
                var RejQtyDet = _UWcontext.tblrejectqties.Where(m => m.RID == RID && m.CorrectedDate == correcteddate && m.ShiftID == Shift && m.StartTime == Stime && m.EndTime == Etime).FirstOrDefault();
                if (RejQtyDet == null)
                {
                    tblrejectqty addrow = new tblrejectqty();
                    addrow.CorrectedDate = correcteddate;
                    addrow.isDeleted = 0;
                    addrow.RID = RID;
                    addrow.ShiftID = Shift;
                    addrow.RejectQty = Convert.ToInt32(rejectval[i]);
                    addrow.WOID = wodet;
                    addrow.StartTime = Stime;
                    addrow.EndTime = Etime;
                    addrow.isFinished = 0;
                    addrow.ModifiedOn = Stime;
                    addrow.CreatedOn = Stime;
                    addrow.CreatedBy = 1;
                    addrow.ModifiedBy = 1;
                    try
                    {
                        _UWcontext.tblrejectqties.Add(addrow);
                        _UWcontext.SaveChanges();
                    }
                    catch(Exception ex)
                    { }
                    i++;
                }
                else
                {
                    int? Rval = RejQtyDet.RejectQty;
                    int RQty = 0;
                    if (Rval != null)
                    {
                        RQty = Convert.ToInt32(Rval);
                    }

                    RejQtyDet.RejectQty = Convert.ToInt32(rejectval[i]) + RQty;
                    _UWcontext.Entry(RejQtyDet).State = EntityState.Modified;
                    _UWcontext.SaveChanges();
                    i++;
                }

            }
            return "pass";
        }

        //public JsonResult FinishProdOrder()
        //{
        //    int MachineID = 0;
        //    try
        //    {
        //        MachineID = Convert.ToInt32(Session["MachineID"]);
        //    }
        //    catch
        //    {
        //        GetMode GM = new GetMode();
        //        String IPAddress = GM.GetIPAddressofTabSystem();

        //        MachineID = _UWcontext.tblmachinedetails.Where(m => m.TabIPAddress == IPAddress && m.IsDeleted == 0).Select(m => m.MachineID).First();

        //        Session["MachineID"] = MachineID;
        //    }
        //    var Data = true;

        //    var WOE = _UWcontext.tblworkorderentries.Where(m => m.MachineID == MachineID && m.IsFinished == 0 && m.IsStarted == 1).OrderByDescending(m => m.HMIID).ToList().FirstOrDefault();
        //    int Sqty = 0;
        //    //WOE.Yield_Qty = Yqty;
        //    WOE.ScrapQty = Sqty;
        //    //WOE.Total_Qty = Tqty;
        //    WOE.IsFinished = 1;
        //    WOE.WOEnd = DateTime.Now;
        //    //WOE.SyncInsert = 0;
        //    _UWcontext.Entry(WOE).State = EntityState.Modified;
        //    _UWcontext.SaveChanges();
        //    updateTool(WOE.HMIID);
        //    return Json(Data, JsonRequestBehavior.AllowGet);
        //}

        //public JsonResult HoldProdOrder(int HoldReasonID)
        //{
        //    int MachineID = 0;
        //    try
        //    {
        //        MachineID = Convert.ToInt32(Session["MachineID"]);
        //    }
        //    catch
        //    {
        //        GetMode GM = new GetMode();
        //        String IPAddress = GM.GetIPAddressofTabSystem();

        //        MachineID = _UWcontext.tblmachinedetails.Where(m => m.TabIPAddress == IPAddress && m.IsDeleted == 0).Select(m => m.MachineID).First();

        //        Session["MachineID"] = MachineID;
        //    }
        //    var Data = true;

        //    var WOE = _UWcontext.tblworkorderentries.Where(m => m.MachineID == MachineID && m.IsFinished == 0 && m.IsStarted == 1).OrderByDescending(m => m.HMIID).ToList().FirstOrDefault();

        //    //WOE.Yield_Qty = Yqty;
        //    WOE.ScrapQty = 0;
        //    // WOE.Total_Qty = Tqty;
        //    WOE.HoldCodeID = HoldReasonID;
        //    WOE.IsFinished = 1;
        //    WOE.IsHold = 1;
        //    WOE.WOEnd = DateTime.Now;
        //    WOE.HoldTime = DateTime.Now;
        //    // WOE.SyncInsert = 0;
        //    _UWcontext.Entry(WOE).State = EntityState.Modified;
        //    _UWcontext.SaveChanges();
        //    updateTool(WOE.HMIID);
        //    return Json(Data, JsonRequestBehavior.AllowGet);
        //}

        //public void updateTool(int hmiid)
        //{
        //    #region//updation in tbltoollife operator in server and tab

        //    var list = _UWcontext.tbltoollifeoperators.Where(m => m.HMIID == hmiid).ToList();
        //    foreach (var item in list)
        //    {
        //        try
        //        {
        //            var updateTabTool = _UWcontext.tbltoollifeoperators.Find(item.ToolLifeID);
        //            updateTabTool.IsCompleted = true;
        //            _UWcontext.Entry(updateTabTool).State = EntityState.Modified;
        //            _UWcontext.SaveChanges();

        //            var updateServerTool = _ServerContext.tbltoollifeoperators.Find(item.ToolIDAdmin);
        //            updateServerTool.IsCompleted = true;
        //            _ServerContext.Entry(updateServerTool).State = EntityState.Modified;
        //            _ServerContext.SaveChanges();
        //        }
        //        catch
        //        {

        //        }
        //    }
        //    #endregion
        //}

        //public void GetToolLifeCounter(String Part_No, String OpNo, String ToolNum, ref int ToolLifeCount)
        //{
        //    ToolLifeCount = 0;
        //    var GetHMIID = _ServerContext.tblworkorderentries.Where(m => m.FGCode == Part_No && m.OperationNo == OpNo && m.IsFinished == 1).OrderByDescending(m => m.HMIID).FirstOrDefault();
        //    if (GetHMIID != null)
        //    {
        //        var GetToolCount = _ServerContext.tbltoollifeoperators.Where(m => m.HMIID == GetHMIID.HMIID && m.IsReset == 0 && m.ToolNo == ToolNum).OrderByDescending(m => m.ToolLifeID).FirstOrDefault();
        //        if (GetToolCount != null)
        //        {
        //            ToolLifeCount = GetToolCount.toollifecounter;
        //        }
        //    }
        //}

        //public ActionResult MaintenanceProductionWindow(int smValue = 0)
        //{
        //    GetMode GM = new GetMode();
        //    String IPAddress = GM.GetIPAddressofTabSystem();

        //    int MachineID = _UWcontext.tblmachinedetails.Where(m => m.TabIPAddress == IPAddress && m.IsDeleted == 0).Select(m => m.MachineID).First();

        //    Session["MachineID"] = MachineID;
        //    var HeaderDet = _UWcontext.tbloperatorheaders.Where(m => m.MachineID == MachineID).SingleOrDefault();
        //    if (HeaderDet != null)
        //    {
        //        ViewBag.MachineMode = HeaderDet.MachineMode;
        //        ViewBag.MachineName = HeaderDet.tblmachinedetail.MachineDisplayName;
        //        ViewBag.TabStatus = HeaderDet.TabConnecStatus;
        //        ViewBag.ServerStatus = HeaderDet.ServerConnecStatus;
        //        ViewBag.PageName = "MNT";
        //        ViewBag.Shift = HeaderDet.Shift;
        //    }

        //    var prvmode = _UWcontext.tblmodes.Where(m => m.MachineID == MachineID && m.ModeType == "SETUP" && m.ModeTypeEnd == 0)
        //            .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
        //    if (prvmode != null)
        //    {
        //        ViewBag.SetUpStarted = "1";
        //        ViewBag.MachineMode = "Setting";
        //    }

        //    var prvmodeMaint = _UWcontext.tblmodes.Where(m => m.MachineID == MachineID && m.ModeType == "MNT" && m.ModeTypeEnd == 0)
        //            .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
        //    if (prvmodeMaint != null)
        //    {
        //        ViewBag.MNTStarted = "1";
        //        ViewBag.MachineMode = "MNT";
        //    }

        //    var GetMaint = _UWcontext.tblsetupmaints.Where(m => m.MachineID == MachineID && m.IsStarted == 1 && m.IsSetup == 0 && m.IsCompleted == 0).FirstOrDefault();
        //    if (smValue != 1)
        //    {
        //        Session["message"] = "nullinsess";
        //    }
        //    return View(GetMaint);
        //}

        //public ActionResult EndMaintenance(int smValue = 0)
        //{
        //    GetMode GM = new GetMode();
        //    String IPAddress = GM.GetIPAddressofTabSystem();

        //    int MachineID = _UWcontext.tblmachinedetails.Where(m => m.TabIPAddress == IPAddress && m.IsDeleted == 0).Select(m => m.MachineID).First();

        //    Session["MachineID"] = MachineID;

        //    var mode = _UWcontext.tblmodes.Where(m => m.MachineID == MachineID && m.ModeType == "MNT" && m.IsCompleted == 1 && m.ModeTypeEnd == 0)
        //                .OrderByDescending(m => m.ModeId).FirstOrDefault();
        //    mode.IsCompleted = 1;
        //    mode.ModeTypeEnd = 1;
        //    mode.EndTime = DateTime.Now;
        //    mode.DurationInSec = Convert.ToInt32(DateTime.Now.Subtract(Convert.ToDateTime(mode.StartTime)).TotalSeconds);
        //    mode.StartIdle = 0;
        //    mode.ModifiedOn = DateTime.Now;
        //    _UWcontext.Entry(mode).State = EntityState.Modified;
        //    _UWcontext.SaveChanges();

        //    var GetSetting = _UWcontext.tblsetupmaints.Where(m => m.MachineID == MachineID && m.IsStarted == 1 && m.IsSetup == 0 && m.IsCompleted == 0).FirstOrDefault();
        //    GetSetting.IsCompleted = 1;
        //    GetSetting.EndTime = System.DateTime.Now;
        //    GetSetting.DurationInSec = Convert.ToInt32(DateTime.Now.Subtract(Convert.ToDateTime(GetSetting.StartTime)).TotalSeconds);

        //    _UWcontext.Entry(GetSetting).State = EntityState.Modified;
        //    _UWcontext.SaveChanges();


        //    var GetTOD = _UWcontext.tbloperatordashboards.Where(m => m.MachineID == MachineID && m.MessageCode == "MNT").OrderByDescending(m => m.InsertedOn).FirstOrDefault();
        //    GetTOD.MessageEndTime = System.DateTime.Now;
        //    GetTOD.TotalDurationinMin = Convert.ToInt32(System.DateTime.Now.Subtract(GetTOD.MessageStartTime).TotalMinutes);
        //    _UWcontext.Entry(GetTOD).State = EntityState.Modified;
        //    _UWcontext.SaveChanges();

        //    var checkIsCompletedRow = _UWcontext.tblmodes.Where(m => m.IsCompleted == 0).ToList();
        //    if (checkIsCompletedRow.Count == 0)
        //    {
        //        I_Facility.ServerModel.tblmode TM = new I_Facility.ServerModel.tblmode();
        //        TM.ColorCode = "YELLOW";
        //        TM.CorrectedDate = mode.CorrectedDate;
        //        TM.IsCompleted = 0;
        //        TM.IsDeleted = 0;
        //        TM.InsertedOn = DateTime.Now;
        //        TM.InsertedBy = 1;
        //        TM.MachineID = mode.MachineID;
        //        TM.MacMode = "IDLE";
        //        TM.ModeType = "IDLE";
        //        TM.StartIdle = 0;
        //        TM.StartTime = mode.EndTime;
        //        //TM.Sync = 0;
        //        _UWcontext.tblmodes.Add(TM);
        //        _UWcontext.SaveChanges();
        //    }



        //    #region Update tblOperatorHeader 

        //    var operatorHeader = _UWcontext.tbloperatorheaders.Where(m => m.MachineID == MachineID).OrderByDescending(m => m.InsertedOn).FirstOrDefault();
        //    operatorHeader.MachineMode = "Production";
        //    operatorHeader.ModifiedOn = DateTime.Now;
        //    operatorHeader.ModifiedBy = 1;
        //    _UWcontext.Entry(operatorHeader).State = EntityState.Modified;
        //    _UWcontext.SaveChanges();

        //    #endregion

        //    #region UnLock the Machine
        //    var MacDet = _UWcontext.tblmachinedetails.Find(MachineID);
        //    if (MacDet.MachineModelType == 1)
        //    {
        //        AddFanucMachineWithConn AC = new AddFanucMachineWithConn(MacDet.IPAddress);
        //        AC.SetMachineUnlock((ushort)MacDet.MachineUnlockBit, (ushort)MacDet.MachineLockBit);
        //    }
        //    #endregion
        //    if (smValue == 1)
        //    {
        //        return Redirect("SettingWindow");
        //    }
        //    return RedirectToAction("DashboardProduction");
        //}

        //public ActionResult MaintenanceWindow(int LossSelect = 0)
        //{

        //    GetMode GM = new GetMode();
        //    String IPAddress = GM.GetIPAddressofTabSystem();
        //    try
        //    {
        //        string sessionvar = Session["message"].ToString();
        //        if (sessionvar == null || sessionvar == string.Empty)
        //        {
        //            Session["message"] = "";
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Session["message"] = "";
        //    }
        //    int MachineID = _UWcontext.tblmachinedetails.Where(m => m.TabIPAddress == IPAddress && m.IsDeleted == 0).Select(m => m.MachineID).First();

        //    Session["MachineID"] = MachineID;
        //    var isSetUpormaint = _UWcontext.tblsetupmaints.Where(m => m.MachineID == MachineID && m.IsStarted == 1 && m.IsSetup == 1 && m.IsCompleted == 0).ToList();
        //    if (isSetUpormaint.Count == 0)
        //    {
        //        var HeaderDet = _UWcontext.tbloperatorheaders.Where(m => m.MachineID == MachineID).SingleOrDefault();
        //        ViewBag.MachineMode = HeaderDet.MachineMode;
        //        ViewBag.MachineName = HeaderDet.tblmachinedetail.MachineDisplayName;
        //        ViewBag.TabStatus = HeaderDet.TabConnecStatus;
        //        ViewBag.ServerStatus = HeaderDet.ServerConnecStatus;
        //        ViewBag.PageName = "MNT";
        //        ViewBag.Shift = HeaderDet.Shift;

        //        var prvmode = _UWcontext.tblmodes.Where(m => m.MachineID == MachineID && m.ModeType == "SETUP" && m.ModeTypeEnd == 0)
        //                .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
        //        if (prvmode != null)
        //        {
        //            ViewBag.SetUpStarted = "1";
        //            ViewBag.MachineMode = "Setting";
        //        }

        //        var prvmodeMaint = _UWcontext.tblmodes.Where(m => m.MachineID == MachineID && m.ModeType == "MNT" && m.ModeTypeEnd == 0)
        //                .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
        //        if (prvmodeMaint != null)
        //        {
        //            ViewBag.MNTStarted = "1";
        //            ViewBag.MachineMode = "MNT";
        //        }

        //        if (HeaderDet.MachineMode == "MNT")
        //        {
        //            return RedirectToAction("MaintenanceProductionWindow");
        //        }

        //        var tblLossCodes = _UWcontext.tbllossescodes.Where(m => m.MessageType == "MNT").ToList();
        //        ViewBag.lossCodes = tblLossCodes;
        //        if (tblLossCodes.Count != 0)
        //        {
        //            Session["message"] = "";
        //            Session["messagevalue"] = "2";//for settings
        //            if (LossSelect == 0)//first time ,show level 2
        //            {
        //                int lossCodeID = tblLossCodes.Find(a => a.MessageType == "MNT" && a.LossCodesLevel == 1).LossCodeID;
        //                ViewBag.lossCodeID = lossCodeID;
        //                ViewBag.level = 2;
        //            }
        //            else // show level 3
        //            {
        //                int lossCodeID = LossSelect;
        //                ViewBag.lossCodeID = lossCodeID;
        //                ViewBag.level = 3;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        Session["message"] = "Machine is in Setting. Cannot start the Maintenance. Stop the Setting, and then start the Maintenance.";
        //        Session["messagevalue"] = "1";//for maintaince
        //        return Redirect("SettingProduction?smValue=2");
        //    }
        //    return View();
        //}

        //public ActionResult SaveMaintenance(int LossSelect = 0)
        //{
        //    //request came from level 2 and was a last node .Level 3  code will come as parameter.
        //    if (LossSelect == 0)
        //    {
        //        if (Request.QueryString["selectLoss"] != null) // Ideally not null, if null go to hell :) 
        //            LossSelect = Convert.ToInt32(Request.QueryString["selectLoss"]);
        //    }

        //    GetMode GM = new GetMode();
        //    String IPAddress = GM.GetIPAddressofTabSystem();

        //    int MachineID = _UWcontext.tblmachinedetails.Where(m => m.TabIPAddress == IPAddress && m.IsDeleted == 0).Select(m => m.MachineID).First();

        //    Session["MachineID"] = MachineID;
        //    string correctedDate = null;
        //    I_Facility.ServerModel.tbldaytiming StartTime = _UWcontext.tbldaytimings.Where(m => m.IsDeleted == 0).SingleOrDefault();
        //    TimeSpan Start = StartTime.StartTime;
        //    if (Start.Hours <= DateTime.Now.Hour)
        //    {
        //        correctedDate = DateTime.Now.ToString("yyyy-MM-dd");
        //    }
        //    else
        //    {
        //        correctedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        //    }
        //    try
        //    {
        //        string sessionvar = Session["message"].ToString();
        //        if (sessionvar == null || sessionvar == string.Empty)
        //        {
        //            Session["message"] = "";
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Session["message"] = "";
        //    }
        //    var prvmode = _UWcontext.tblmodes.Where(m => m.MachineID == MachineID && m.IsCompleted == 0)
        //            .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
        //    //to check whether the macine is in setup mode where issetup will be in 1  
        //    var isSetUpormaint = _UWcontext.tblsetupmaints.Where(m => m.MachineID == MachineID && m.IsStarted == 1 && m.IsSetup == 1 && m.IsCompleted == 0).ToList();
        //    if (isSetUpormaint.Count == 0)
        //    {
        //        int flag = 0;
        //        #region Update  TblMode
        //        if (prvmode.ColorCode == "YELLOW" || prvmode.ColorCode == "BLUE")
        //        {
        //            prvmode.LossCodeID = LossSelect;
        //            prvmode.ModeType = "MNT";
        //            prvmode.StartIdle = 0;
        //            prvmode.ColorCode = "RED";
        //            prvmode.MacMode = "MNT";
        //            prvmode.IsCompleted = 1;
        //            prvmode.LossCodeEnteredTime = DateTime.Now;
        //            prvmode.LossCodeEnteredBy = "";
        //            prvmode.ModifiedOn = DateTime.Now;
        //            prvmode.ModifiedBy = Convert.ToInt32(Session["UserID"]);
        //            _UWcontext.Entry(prvmode).State = EntityState.Modified;
        //            _UWcontext.SaveChanges();


        //            I_Facility.ServerModel.tblsetupmaint SM = new I_Facility.ServerModel.tblsetupmaint();

        //            SM.IsCompleted = 0;
        //            SM.IsSetup = 0;
        //            SM.IsStarted = 1;
        //            SM.LossCodeID = LossSelect;
        //            SM.MachineID = MachineID;
        //            SM.ModeId = prvmode.ModeId;
        //            SM.StartTime = (DateTime)prvmode.StartTime;

        //            SM.Sync = 0;

        //            _UWcontext.tblsetupmaints.Add(SM);
        //            _UWcontext.SaveChanges();
        //            flag = 1;//tocheck wheather the data is updated or inserted
        //        }
        //        else if (prvmode.ColorCode == "GREEN")
        //        {
        //            ViewBag.SetupError = "Machine is in Production. Cannot start the Maintenance. Stop the Prodcution, wait for 2 minutes and then start the Maintenance.";
        //            return RedirectToAction("DashboardProduction");
        //        }
        //        else if (prvmode.ModeType == "SETUP")
        //        {
        //            ViewBag.SetupError = "Machine is in Setting. Cannot start the Maintenance. Stop the Setting, wait for 2 minutes and then start the Maintenance.";
        //            return RedirectToAction("DashboardProduction");
        //        }

        //        #region UnLock the Machine
        //        var MacDet = _UWcontext.tblmachinedetails.Find(MachineID);
        //        if (MacDet.MachineModelType == 1)
        //        {
        //            AddFanucMachineWithConn AC = new AddFanucMachineWithConn(MacDet.IPAddress);
        //            AC.SetMachineUnlock((ushort)MacDet.MachineUnlockBit, (ushort)MacDet.MachineLockBit);
        //        }
        //        #endregion

        //        #endregion
        //        if (flag == 1)
        //        {
        //            #region Insert to Operator Dashboard

        //            I_Facility.ServerModel.tbloperatordashboard operatorDashboard = new I_Facility.ServerModel.tbloperatordashboard();
        //            DateTime CorrectedDateToDate = Convert.ToDateTime(correctedDate);
        //            Random OperatorDashboardID = new Random();
        //            //operatorDashboard.OperatorDashboardID = OperatorDashboardID.Next(1, 9999999);  //remove this line once Identity is setup
        //            operatorDashboard.MachineID = Convert.ToInt32(Session["MachineID"]);
        //            operatorDashboard.CorrectedDate = CorrectedDateToDate;
        //            operatorDashboard.SlNo = _UWcontext.tbloperatordashboards.Where(m => m.CorrectedDate == CorrectedDateToDate).Where(m => m.MachineID == MachineID).ToList().Count + 1;
        //            operatorDashboard.MessageCode = "MNT";
        //            operatorDashboard.MessageDescription = "Machine Started In Maintenance Mode";
        //            operatorDashboard.MessageStartTime = DateTime.Now;
        //            operatorDashboard.InsertedOn = DateTime.Now;
        //            operatorDashboard.InsertedBy = Convert.ToInt32(Session["UserID"]);
        //            operatorDashboard.IsDeleted = 0;
        //            _UWcontext.tbloperatordashboards.Add(operatorDashboard);
        //            _UWcontext.SaveChanges();

        //            #endregion

        //            #region Update tblOperatorHeader 

        //            var operatorHeader = _UWcontext.tbloperatorheaders.Where(m => m.MachineID == MachineID).OrderByDescending(m => m.InsertedOn).FirstOrDefault();
        //            operatorHeader.MachineMode = "MNT";
        //            operatorHeader.ModifiedOn = DateTime.Now;
        //            operatorHeader.ModifiedBy = 1;
        //            _UWcontext.Entry(operatorHeader).State = EntityState.Modified;
        //            _UWcontext.SaveChanges();

        //            #endregion
        //        }
        //    }
        //    else
        //    {
        //        Session["maintanceState"] = "Machine is in Setting. Cannot start the Maintenance. Stop the Setting, wait for 2 minutes and then start the Maintenance.";
        //        return Redirect("MaintenanceWindow");
        //    }


        //    return RedirectToAction("DashboardProduction");
        //}

        //public ActionResult SettingProduction(int smValue = 0)
        //{
        //    GetMode GM = new GetMode();
        //    String IPAddress = GM.GetIPAddressofTabSystem();

        //    int MachineID = _UWcontext.tblmachinedetails.Where(m => m.TabIPAddress == IPAddress && m.IsDeleted == 0).Select(m => m.MachineID).First();

        //    Session["MachineID"] = MachineID;
        //    var HeaderDet = _UWcontext.tbloperatorheaders.Where(m => m.MachineID == MachineID).SingleOrDefault();
        //    ViewBag.MachineMode = HeaderDet.MachineMode;
        //    ViewBag.MachineName = HeaderDet.tblmachinedetail.MachineDisplayName;
        //    ViewBag.TabStatus = HeaderDet.TabConnecStatus;
        //    ViewBag.ServerStatus = HeaderDet.ServerConnecStatus;
        //    ViewBag.PageName = "Setting";
        //    ViewBag.Shift = HeaderDet.Shift;

        //    var prvmode = _UWcontext.tblmodes.Where(m => m.MachineID == MachineID && m.ModeType == "SETUP" && m.ModeTypeEnd == 0)
        //            .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
        //    if (prvmode != null)
        //    {
        //        ViewBag.SetUpStarted = "1";
        //        ViewBag.MachineMode = "Setting";
        //    }

        //    var prvmodeMaint = _UWcontext.tblmodes.Where(m => m.MachineID == MachineID && m.ModeType == "MNT" && m.ModeTypeEnd == 0)
        //            .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
        //    if (prvmodeMaint != null)
        //    {
        //        ViewBag.MNTStarted = "1";
        //        ViewBag.MachineMode = "MNT";
        //    }

        //    var GetSetting = _UWcontext.tblsetupmaints.Where(m => m.MachineID == MachineID && m.IsStarted == 1 && m.IsSetup == 1 && m.IsCompleted == 0).FirstOrDefault();
        //    if (smValue != 2)
        //    {
        //        Session["message"] = "nullinsess";
        //    }

        //    return View(GetSetting);
        //}

        //public ActionResult EndSetting(int smValue = 0)
        //{
        //    GetMode GM = new GetMode();
        //    String IPAddress = GM.GetIPAddressofTabSystem();

        //    int MachineID = _UWcontext.tblmachinedetails.Where(m => m.TabIPAddress == IPAddress && m.IsDeleted == 0).Select(m => m.MachineID).First();

        //    Session["MachineID"] = MachineID;
        //    var GetSetting = _UWcontext.tblsetupmaints.Where(m => m.MachineID == MachineID && m.IsStarted == 1 && m.IsSetup == 1 && m.IsCompleted == 0).FirstOrDefault();

        //    if (GetSetting != null)
        //    {
        //        GetSetting.IsCompleted = 1;
        //        GetSetting.EndTime = System.DateTime.Now;
        //        DateTime ModeStartTime = DateTime.Now;
        //        try
        //        {
        //            ModeStartTime = Convert.ToDateTime(_UWcontext.tblmodes.Where(m => m.ModeId == GetSetting.ModeId).Select(m => m.LossCodeEnteredTime).First());
        //        }
        //        catch (Exception e)
        //        {
        //        }

        //        var getLossDuration = _UWcontext.tblmodes.Where(m => m.StartTime > ModeStartTime && m.StartTime < GetSetting.EndTime).ToList();

        //        double LossDuration = 0;
        //        double MinorlossDuration = 0;
        //        double OpDuration = 0;
        //        double PowerOffDuration = 0;
        //        foreach (var ModeRow in getLossDuration)
        //        {
        //            if (ModeRow.ModeType == "IDLE" && ModeRow.DurationInSec > 600)
        //                LossDuration += (int)ModeRow.DurationInSec;
        //            else if (ModeRow.ModeType == "IDLE" && ModeRow.DurationInSec < 600)
        //                MinorlossDuration += (int)ModeRow.DurationInSec;
        //            else if (ModeRow.ModeType == "POWEROFF")
        //                PowerOffDuration += (int)ModeRow.DurationInSec;
        //            else if (ModeRow.ModeType == "PROD")
        //                OpDuration += (int)ModeRow.DurationInSec;
        //        }
        //        GetSetting.DurationInSec = (int)(OpDuration + MinorlossDuration);
        //        GetSetting.MinorLossTime = (int)MinorlossDuration;

        //        //GetSetting.ServerSetMainID
        //        _UWcontext.Entry(GetSetting).State = EntityState.Modified;
        //        _UWcontext.SaveChanges();


        //        var GetTOD = _UWcontext.tbloperatordashboards.Where(m => m.MachineID == MachineID && m.MessageCode == "Setting").OrderByDescending(m => m.InsertedOn).FirstOrDefault();
        //        GetTOD.MessageEndTime = System.DateTime.Now;
        //        GetTOD.TotalDurationinMin = Convert.ToInt32(System.DateTime.Now.Subtract(GetTOD.MessageStartTime).TotalMinutes);
        //        _UWcontext.Entry(GetTOD).State = EntityState.Modified;
        //        _UWcontext.SaveChanges();

        //        var mode = _UWcontext.tblmodes.Where(m => m.MachineID == MachineID && m.ModeType == "SETUP" && m.ModeTypeEnd == 0)
        //                    .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
        //        mode.IsCompleted = 1;
        //        mode.ModeTypeEnd = 1;
        //        mode.EndTime = DateTime.Now;
        //        mode.DurationInSec = Convert.ToInt32(DateTime.Now.Subtract(Convert.ToDateTime(mode.StartTime)).TotalSeconds);
        //        mode.StartIdle = 0;
        //        mode.ModifiedOn = DateTime.Now;
        //        _UWcontext.Entry(mode).State = EntityState.Modified;
        //        _UWcontext.SaveChanges();



        //        #region Update tblOperatorHeader 

        //        //@Pavan as per mail ->  get the latest row according to the MachineID and CorrectedDate ->  there is no CorrectedDate column 
        //        var operatorHeader = _UWcontext.tbloperatorheaders.Where(m => m.MachineID == MachineID).OrderByDescending(m => m.InsertedOn).FirstOrDefault();

        //        operatorHeader.MachineMode = "Production";
        //        operatorHeader.ModifiedOn = DateTime.Now;
        //        operatorHeader.ModifiedBy = 1;// get from session once these screens are integrated....

        //        _UWcontext.Entry(operatorHeader).State = EntityState.Modified;
        //        _UWcontext.SaveChanges();

        //        #endregion

        //    }
        //    else
        //    {
        //        var GetSetupModeList = _UWcontext.tblmodes.Where(m => m.MachineID == MachineID && m.ModeType == "SETUP" && m.ModeTypeEnd == 0).ToList();

        //        foreach (var GetRow in GetSetupModeList)
        //        {
        //            GetRow.ModeTypeEnd = 1;
        //            GetRow.StartIdle = 0;
        //            GetRow.IsCompleted = 1;
        //            _UWcontext.Entry(GetRow).State = EntityState.Modified;
        //            _UWcontext.SaveChanges();
        //        }
        //    }


        //    #region UnLock the Machine
        //    var MacDet = _UWcontext.tblmachinedetails.Find(MachineID);
        //    if (MacDet.MachineModelType == 1)
        //    {
        //        AddFanucMachineWithConn AC = new AddFanucMachineWithConn(MacDet.IPAddress);
        //        AC.SetMachineUnlock((ushort)MacDet.MachineUnlockBit, (ushort)MacDet.MachineLockBit);
        //    }
        //    #endregion
        //    if (smValue == 2)
        //    {
        //        return Redirect("MaintenanceWindow");
        //    }
        //    return RedirectToAction("DashboardProduction");
        //}

        //public ActionResult SettingWindow(FormCollection form, int LossSelect = 0)
        //{
        //    GetMode GM = new GetMode();
        //    String IPAddress = GM.GetIPAddressofTabSystem();

        //    int MachineID = _UWcontext.tblmachinedetails.Where(m => m.TabIPAddress == IPAddress && m.IsDeleted == 0).Select(m => m.MachineID).First();
        //    try
        //    {
        //        string sessionvar = Session["message"].ToString();
        //        if (sessionvar == null || sessionvar == string.Empty)
        //        {
        //            Session["message"] = "";
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Session["message"] = "";
        //    }
        //    Session["MachineID"] = MachineID;
        //    var isSetUpormaint = _UWcontext.tblsetupmaints.Where(m => m.MachineID == MachineID && m.IsStarted == 1 && m.IsSetup == 0 && m.IsCompleted == 0).ToList();
        //    if (isSetUpormaint.Count == 0)
        //    {
        //        Session["message"] = "";
        //        Session["messagevalue"] = "1";//for maintance
        //        var HeaderDet = _UWcontext.tbloperatorheaders.Where(m => m.MachineID == MachineID).SingleOrDefault();
        //        ViewBag.MachineMode = HeaderDet.MachineMode;
        //        ViewBag.MachineName = HeaderDet.tblmachinedetail.MachineDisplayName;
        //        ViewBag.TabStatus = HeaderDet.TabConnecStatus;
        //        ViewBag.ServerStatus = HeaderDet.ServerConnecStatus;
        //        ViewBag.PageName = "Setting";
        //        ViewBag.Shift = HeaderDet.Shift;



        //        var prvmode = _UWcontext.tblmodes.Where(m => m.MachineID == MachineID && m.ModeType == "SETUP" && m.ModeTypeEnd == 0)
        //                .OrderByDescending(m => m.ModeId).FirstOrDefault();
        //        if (prvmode != null)
        //        {
        //            ViewBag.SetUpStarted = "1";
        //            ViewBag.MachineMode = "Setting";
        //            return RedirectToAction("IDLEPopup");
        //        }

        //        var prvmodeMaint = _UWcontext.tblmodes.Where(m => m.MachineID == MachineID && m.ModeType == "MNT" && m.ModeTypeEnd == 0)
        //                .OrderByDescending(m => m.ModeId).FirstOrDefault();
        //        if (prvmodeMaint != null)
        //        {
        //            ViewBag.MNTStarted = "1";
        //            ViewBag.MachineMode = "MNT";
        //        }

        //        if (HeaderDet.MachineMode == "Setting")
        //        {
        //            return RedirectToAction("SettingProduction");
        //        }

        //        var tblLossCodes = _UWcontext.tbllossescodes.Where(m => m.MessageType == "SETUP").ToList();
        //        ViewBag.lossCodes = tblLossCodes;

        //        if (LossSelect == 0)//first time ,show level 2
        //        {
        //            int lossCodeID = tblLossCodes.Find(a => a.MessageType == "SETUP" && a.LossCodesLevel == 1).LossCodeID;
        //            ViewBag.lossCodeID = lossCodeID;
        //            ViewBag.level = 2;
        //        }
        //        else // show level 3
        //        {
        //            int lossCodeID = LossSelect;
        //            ViewBag.lossCodeID = lossCodeID;
        //            ViewBag.level = 3;
        //        }
        //    }
        //    else
        //    {
        //        Session["message"] = "Machine is in Maintenance. Cannot start the Setting. Stop the Maintenance, wait for 2 minutes and then start the Maintenance.";
        //        Session["messagevalue"] = "0";//for settings
        //        return Redirect("MaintenanceProductionWindow?smValue=1");
        //    }
        //    return View();
        //}


        //    public JsonResult GetMachinePopup()
        //    {
        //        DateTime correcteddate = DateTime.Now.Date;
        //        List<IdlePopupMachine> IdelListObj = new List<IdlePopupMachine>();
        //        GetMode GM = new GetMode();
        //        int cellIdlecount = 0;
        //        var celliddet = Session["CellID"];
        //        int cellid = Convert.ToInt32(celliddet);
        //        var machinedet = _UWcontext.tblmachinedetails.Where(m => m.CellID == cellid && m.IsDeleted == 0).ToList();
        //        foreach (var item in machinedet)
        //        {
        //            int MachineID = item.MachineID;
        //            bool IdleStatus = GM.CheckIdleEntry(MachineID);
        //            if (IdleStatus)
        //            {
        //                List<LossCode> LoosObj = new List<LossCode>();
        //                var tblLossCodes = _UWcontext.tbllossescodes.Where(m => m.MessageType == "IDLE" && m.IsDeleted == 0).ToList();
        //                foreach (var data in tblLossCodes)
        //                {
        //                    LossCode Lobj = new LossCode();
        //                    Lobj.losscodeid = data.LossCodeID;
        //                    Lobj.losscode = data.LossCode;
        //                    Lobj.losslevel = Convert.ToInt32(data.LossCodesLevel);
        //                    LoosObj.Add(Lobj);
        //                }
        //                var IdleEntry = _UWcontext.tbllivemodes.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID && m.IsCompleted == 0 && m.StartIdle == 1 && m.ColorCode == "YELLOW").OrderByDescending(m => m.ModeId).FirstOrDefault();
        //                var optdet = _UWcontext.tbloperatordashboards.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID && m.CorrectedDate == correcteddate).ToList();
        //                int machinecount = optdet.Count();
        //                cellIdlecount = cellIdlecount + machinecount;
        //                IdlePopupMachine obj = new IdlePopupMachine();
        //                obj.MachineID = item.MachineID;
        //                obj.machinename = item.MachineDisplayName;
        //                obj.starttimeidle = Convert.ToString(IdleEntry.StartTime);
        //                obj.IdelNo = machinecount;
        //                obj.cellidleno = cellIdlecount;
        //                obj.LLoss = LoosObj;
        //                IdelListObj.Add(obj);
        //            }
        //        }
        //        IdelListObj.OrderBy(m => m.starttimeidle);
        //        return Json(IdelListObj, JsonRequestBehavior.AllowGet);
        //    }

        //    //public int CellIdelCount()
        //    //{
        //    //    int cellIdlecount = 0;
        //    //    DateTime correcteddate = DateTime.Now.Date;
        //    //    var celliddet = Session["CellID"];
        //    //    int cellid = Convert.ToInt32(celliddet);
        //    //    var machinedet = _UWcontext.tblmachinedetails.Where(m => m.CellID == cellid && m.IsDeleted == 0).ToList();
        //    //    foreach (var item in machinedet)
        //    //    {
        //    //        int MachineID = item.MachineID;               
        //    //        var optdet = _UWcontext.tbloperatordashboards.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID && m.CorrectedDate == correcteddate).ToList();
        //    //        int machinecount = optdet.Count();
        //    //        cellIdlecount = cellIdlecount + machinecount;
        //    //    }
        //    //    return cellIdlecount;
        //    //}


        //    public ActionResult IDLEPopup(FormCollection form, int LossSelect = 0)
        //    {
        //        GetMode GM = new GetMode();
        //        String IPAddress = GM.GetIPAddressofTabSystem();
        //        //int cellcount = CellIdelCount();
        //        //ViewData["CellIdle"] = cellcount;
        //        //List<IdlePopupMachine> IdelListObj = new List<IdlePopupMachine>();

        //        //int MachineID = _UWcontext.tblmachinedetails.Where(m => m.TabIPAddress == IPAddress && m.IsDeleted == 0).Select(m => m.MachineID).First();
        //        var celliddet = Session["CellID"];
        //        int cellid = Convert.ToInt32(celliddet);
        //        var celldet = _UWcontext.tblcells.Find(cellid);
        //        if (celldet != null)
        //        {
        //            ViewBag.CellName = celldet.CellName;
        //        }
        //        var machinedet = _UWcontext.tblmachinedetails.Where(m => m.CellID == cellid && m.IsDeleted == 0).ToList();
        //        foreach (var item in machinedet)
        //        {
        //            int MachineID = item.MachineID;

        //            Session["MachineID"] = MachineID;

        //            //bool IdleStatus = GM.CheckIdleEntry(MachineID);
        //            //if (IdleStatus)
        //            //{
        //            //    IdlePopupMachine obj = new IdlePopupMachine();
        //            //    obj.MachineID = item.MachineID;
        //            //    obj.machinename = item.MachineName;
        //            //    IdelListObj.Add(obj);
        //            //}

        //            var prvmode = _UWcontext.tblmodes.Where(m => m.MachineID == MachineID && m.ModeType == "SETUP" && m.ModeTypeEnd == 0)
        //                    .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
        //            if (prvmode != null)
        //            {
        //                ViewBag.SetUpStarted = "1";
        //                ViewBag.MachineMode = "Setting";
        //            }

        //            var prvmodeMaint = _UWcontext.tblmodes.Where(m => m.MachineID == MachineID && m.ModeType == "MNT" && m.ModeTypeEnd == 0)
        //                    .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
        //            if (prvmodeMaint != null)
        //            {
        //                ViewBag.MNTStarted = "1";
        //                ViewBag.MachineMode = "MNT";
        //            }

        //            var tblLossCodes = _UWcontext.tbllossescodes.Where(m => m.MessageType == "IDLE").ToList();
        //            ViewBag.lossCodes = tblLossCodes;

        //            if (LossSelect == 0)//first time ,show level 1
        //            {
        //                int lossCodeID = tblLossCodes.Find(a => a.MessageType == "IDLE").LossCodeID;
        //                ViewBag.lossCodeID = lossCodeID;
        //                ViewBag.level = 1;
        //            }
        //            else if (tblLossCodes.Where(m => m.LossCodesLevel1ID == LossSelect).ToList().Count > 0)// show level 2
        //            {
        //                int lossCodeID = LossSelect;
        //                ViewBag.lossCodeID = lossCodeID;
        //                ViewBag.level = 2;
        //            }
        //            else //show level 3
        //            {
        //                int lossCodeID = LossSelect;
        //                ViewBag.lossCodeID = lossCodeID;
        //                ViewBag.level = 3;
        //            }

        //            #region Update tblOperatorHeader 

        //            //get the latest row according to the MachineID and CorrectedDate ->  there is no CorrectedDate column 
        //            var operatorHeader = _UWcontext.tbloperatorheaders.Where(m => m.MachineID == MachineID).OrderByDescending(m => m.InsertedOn).FirstOrDefault();
        //            if (operatorHeader != null)
        //            {
        //                operatorHeader.MachineMode = "IDLE";
        //                operatorHeader.ModifiedOn = DateTime.Now;
        //                operatorHeader.ModifiedBy = 1;// get from session once these screens are integrated....

        //                _UWcontext.Entry(operatorHeader).State = EntityState.Modified;
        //                _UWcontext.SaveChanges();
        //            }

        //            #endregion

        //            #region Lock the Machine
        //            var MacDet = _UWcontext.tblmachinedetails.Find(MachineID);

        //            if (MacDet.MachineModelType == 1)
        //            {
        //                AddFanucMachineWithConn AC = new AddFanucMachineWithConn(MacDet.IPAddress);
        //                //AC.setmachinelock(true, (ushort)MacDet.MachineLockBit, (ushort)MacDet.MachineIdleBit, (ushort)MacDet.MachineUnlockBit);
        //            }

        //            #endregion

        //            var HeaderDet = _UWcontext.tbloperatorheaders.Where(m => m.MachineID == MachineID).SingleOrDefault();
        //            if (HeaderDet != null)
        //            {
        //                ViewBag.MachineMode = HeaderDet.MachineMode;
        //                ViewBag.MachineName = HeaderDet.tblmachinedetail.MachineDisplayName;
        //                ViewBag.TabStatus = HeaderDet.TabConnecStatus;
        //                ViewBag.ServerStatus = HeaderDet.ServerConnecStatus;
        //                ViewBag.PageName = "IDLE";
        //                ViewBag.Shift = HeaderDet.Shift;
        //            }
        //        }
        //        //ViewBag.IdelMachine = IdelListObj;
        //        return View();
        //    }

        //    public ActionResult SaveIdle(int LossSelect = 0, int machineid = 0, bool flage = false, int count = 0)
        //    {
        //        //request came from level 2 and was a last node .Level 3  code will come as parameter.
        //        if (LossSelect == 0)
        //        {
        //            if (Request.QueryString["selectLoss"] != null) // Ideally not null, if null go to hell :) 
        //                LossSelect = Convert.ToInt32(Request.QueryString["selectLoss"]);
        //        }
        //        if (machineid == 0)
        //        {
        //            if (Request.QueryString["machineid"] != null) // Ideally not null, if null go to hell :) 
        //                machineid = Convert.ToInt32(Request.QueryString["machineid"]);
        //        }

        //        #region Update TblMode

        //        GetMode GM = new GetMode();
        //        String IPAddress = GM.GetIPAddressofTabSystem();

        //        //int machineID = _UWcontext.tblmachinedetails.Where(m => m.TabIPAddress == IPAddress && m.IsDeleted == 0).Select(m => m.MachineID).First();
        //        //var celliddet = Session["CellID"];
        //        //int cellid = Convert.ToInt32(celliddet);
        //        //var machinedet = _UWcontext.tblmachinedetails.Where(m => m.CellID == cellid && m.IsDeleted == 0 && m.IsLastMachine == 1).ToList();
        //        if (flage == false)
        //        {
        //            var machinedet = _UWcontext.tblmachinedetails.Where(m => m.MachineID == machineid && m.IsDeleted == 0).ToList();
        //            foreach (var item in machinedet)
        //            {
        //                int machineID = item.MachineID;

        //                Session["MachineID"] = machineID;
        //                DateTime correctedDate = DateTime.Now;
        //                I_Facility.ServerModel.tbldaytiming StartTime = _UWcontext.tbldaytimings.Where(m => m.IsDeleted == 0).SingleOrDefault();
        //                TimeSpan Start = StartTime.StartTime;
        //                if (Start.Hours <= DateTime.Now.Hour)
        //                {
        //                    correctedDate = DateTime.Now;
        //                }
        //                else
        //                {
        //                    correctedDate = DateTime.Now.AddDays(-1);
        //                }
        //                int durationinsec = 0;
        //                //var correctedDate = "2017-11-17";   // Hard coding for time being
        //                string colorCode = "YELLOW";
        //                //Update TblMode with the Loss Code
        //                var mode = _UWcontext.tbllivemodes.Where(m => m.MachineID == machineID && m.ColorCode == colorCode && m.IsCompleted == 0 && m.StartIdle == 1)
        //                            .OrderByDescending(m => m.ModeId).FirstOrDefault();
        //                DateTime ModeStartTime = DateTime.Now;
        //                if (mode != null)
        //                {
        //                    ModeStartTime = (DateTime)mode.StartTime;
        //                    durationinsec = Convert.ToInt32(DateTime.Now.Subtract(ModeStartTime).TotalSeconds);
        //                    mode.LossCodeID = LossSelect;
        //                    mode.ModeType = "IDLE";
        //                    mode.LossCodeEnteredTime = DateTime.Now;
        //                    mode.LossCodeEnteredBy = "";
        //                    mode.ModeTypeEnd = 1;
        //                    mode.IsCompleted = 1;
        //                    mode.StartIdle = 0;
        //                    mode.EndTime = DateTime.Now;
        //                    mode.DurationInSec = durationinsec;
        //                    mode.ModifiedOn = DateTime.Now; // doing now for testing purpose
        //                    mode.ModifiedBy = Convert.ToInt32(Session["UserID"]);
        //                    _UWcontext.Entry(mode).State = EntityState.Modified;
        //                    _UWcontext.SaveChanges();


        //                    I_Facility.ServerModel.tblmode tm = new I_Facility.ServerModel.tblmode();
        //                    tm.BreakdownID = mode.BreakdownID;
        //                    tm.ColorCode = mode.ColorCode;
        //                    tm.CorrectedDate = correctedDate;
        //                    tm.InsertedBy = Convert.ToInt32(Session["UserID"]);
        //                    tm.InsertedOn = DateTime.Now;
        //                    tm.IsCompleted = 0;
        //                    tm.IsDeleted = 0;
        //                    tm.LossCodeID = null;
        //                    tm.MachineID = machineID;
        //                    tm.MacMode = mode.MacMode;
        //                    tm.ModeType = "IDLE";
        //                    tm.ModeTypeEnd = 0;
        //                    tm.StartIdle = 0;
        //                    tm.StartTime = tm.InsertedOn;
        //                    //tm.ServerModeId = ServerNewTM.ModeId;
        //                    //tm.Sync = 0;
        //                    _UWcontext.tblmodes.Add(tm);
        //                    _UWcontext.SaveChanges();

        //                    //mode = _UWcontext.tbllivemodes.Where(m => m.MachineID == machineID && m.ColorCode == colorCode && m.IsCompleted == 0)
        //                    //            .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
        //                }
        //                else
        //                {

        //                }
        //                #endregion

        //                //#region UnLock the Machine
        //                //var MacDet = _UWcontext.tblmachinedetails.Find(machineID);
        //                //if (MacDet.MachineModelType == 1)
        //                //{
        //                //    AddFanucMachineWithConn AC = new AddFanucMachineWithConn(MacDet.IPAddress);
        //                //    AC.SetMachineUnlock((ushort)MacDet.MachineUnlockBit, (ushort)MacDet.MachineLockBit);
        //                //}
        //                //#endregion

        //                #region Insert to Operator Dashboard

        //                //Pre requsite before insertion ???

        //                I_Facility.ServerModel.tbloperatordashboard operatorDashboard = new I_Facility.ServerModel.tbloperatordashboard();
        //                var lossdesc = _UWcontext.tbllossescodes.Find(LossSelect);
        //                DateTime CorrectedDateToDate = Convert.ToDateTime(correctedDate);
        //                Random OperatorDashboardID = new Random();
        //                int MachineID = Convert.ToInt32(Session["MachineID"]);
        //                //operatorDashboard.OperatorDashboardID = OperatorDashboardID.Next(1, 9999999);  //remove this line once Identity is setup
        //                operatorDashboard.MachineID = Convert.ToInt32(Session["MachineID"]);
        //                operatorDashboard.CorrectedDate = CorrectedDateToDate;
        //                operatorDashboard.SlNo = _UWcontext.tbloperatordashboards.Where(m => m.CorrectedDate == CorrectedDateToDate).Where(m => m.MachineID == MachineID).ToList().Count + 1; //  @Pavan , I'm not sure what you meant here..  --Count++ from the tbloperatordashboard for the Machine and CorrectedDate
        //                operatorDashboard.MessageCode = "IDLE";
        //                operatorDashboard.MessageDescription = "Machine in IDLE Mode" + "-" + lossdesc.LossCode;
        //                operatorDashboard.MessageStartTime = ModeStartTime;
        //                operatorDashboard.MessageEndTime = DateTime.Now;
        //                operatorDashboard.TotalDurationinMin = Convert.ToInt32(DateTime.Now.Subtract(ModeStartTime).TotalMinutes);
        //                operatorDashboard.InsertedOn = DateTime.Now;
        //                operatorDashboard.InsertedBy = Convert.ToInt32(Session["UserID"]);
        //                //operatorDashboard.ModifiedOn = DateTime.Now;
        //                //operatorDashboard.ModifiedBy = 1;  // Session["UserID"]
        //                operatorDashboard.IsDeleted = 0;

        //                _UWcontext.tbloperatordashboards.Add(operatorDashboard);
        //                _UWcontext.SaveChanges();

        //                #endregion

        //                #region Update tblOperatorHeader 

        //                //get the latest row according to the MachineID and CorrectedDate ->  there is no CorrectedDate column 
        //                var operatorHeader = _UWcontext.tbloperatorheaders.Where(m => m.MachineID == MachineID).OrderByDescending(m => m.InsertedOn).FirstOrDefault();
        //                if (operatorHeader != null)
        //                {
        //                    operatorHeader.MachineMode = "Production";
        //                    operatorHeader.ModifiedOn = DateTime.Now;
        //                    operatorHeader.ModifiedBy = 1;// get from session once these screens are integrated....

        //                    _UWcontext.Entry(operatorHeader).State = EntityState.Modified;
        //                    _UWcontext.SaveChanges();
        //                }

        //                #endregion
        //            }

        //            if (count > 1)
        //            {
        //                return RedirectToAction("IDLEPopup");
        //            }
        //            else
        //            {
        //                return RedirectToAction("EntryWindow");
        //            }
        //        }
        //        else
        //        {
        //            //var machinedet = _UWcontext.tblmachinedetails.Where(m => m.MachineID == machineid && m.IsDeleted == 0).ToList();
        //            var celliddet = Session["CellID"];
        //            int cellid = Convert.ToInt32(celliddet);
        //            var machinedet = _UWcontext.tblmachinedetails.Where(m => m.CellID == cellid && m.IsDeleted == 0).ToList();
        //            foreach (var item in machinedet)
        //            {
        //                int machineID = item.MachineID;
        //                Session["MachineID"] = machineID;
        //                DateTime correctedDate = DateTime.Now;
        //                I_Facility.ServerModel.tbldaytiming StartTime = _UWcontext.tbldaytimings.Where(m => m.IsDeleted == 0).SingleOrDefault();
        //                TimeSpan Start = StartTime.StartTime;
        //                if (Start.Hours <= DateTime.Now.Hour)
        //                {
        //                    correctedDate = DateTime.Now;
        //                }
        //                else
        //                {
        //                    correctedDate = DateTime.Now.AddDays(-1);
        //                }
        //                int durationinsec = 0;
        //                //var correctedDate = "2017-11-17";   // Hard coding for time being
        //                string colorCode = "YELLOW";
        //                //Update TblMode with the Loss Code
        //                var mode = _UWcontext.tbllivemodes.Where(m => m.MachineID == machineID && m.ColorCode == colorCode && m.IsCompleted == 0 && m.StartIdle == 1)
        //                            .OrderByDescending(m => m.ModeId).FirstOrDefault();
        //                DateTime ModeStartTime = DateTime.Now;
        //                if (mode != null)
        //                {
        //                    ModeStartTime = (DateTime)mode.StartTime;
        //                    durationinsec = Convert.ToInt32(DateTime.Now.Subtract(ModeStartTime).TotalSeconds);
        //                    mode.LossCodeID = LossSelect;
        //                    mode.ModeType = "IDLE";
        //                    mode.LossCodeEnteredTime = DateTime.Now;
        //                    mode.LossCodeEnteredBy = "";
        //                    mode.ModeTypeEnd = 1;
        //                    mode.IsCompleted = 1;
        //                    mode.StartIdle = 0;
        //                    mode.EndTime = DateTime.Now;
        //                    mode.DurationInSec = durationinsec;
        //                    mode.ModifiedOn = DateTime.Now; // doing now for testing purpose
        //                    mode.ModifiedBy = Convert.ToInt32(Session["UserID"]);
        //                    _UWcontext.Entry(mode).State = EntityState.Modified;
        //                    _UWcontext.SaveChanges();


        //                    I_Facility.ServerModel.tblmode tm = new I_Facility.ServerModel.tblmode();
        //                    tm.BreakdownID = mode.BreakdownID;
        //                    tm.ColorCode = mode.ColorCode;
        //                    tm.CorrectedDate = correctedDate;
        //                    tm.InsertedBy = Convert.ToInt32(Session["UserID"]);
        //                    tm.InsertedOn = DateTime.Now;
        //                    tm.IsCompleted = 0;
        //                    tm.IsDeleted = 0;
        //                    tm.LossCodeID = null;
        //                    tm.MachineID = machineID;
        //                    tm.MacMode = mode.MacMode;
        //                    tm.ModeType = "IDLE";
        //                    tm.ModeTypeEnd = 0;
        //                    tm.StartIdle = 0;
        //                    tm.StartTime = tm.InsertedOn;
        //                    //tm.ServerModeId = ServerNewTM.ModeId;
        //                    //tm.Sync = 0;
        //                    _UWcontext.tblmodes.Add(tm);
        //                    _UWcontext.SaveChanges();

        //                    //mode = _UWcontext.tbllivemodes.Where(m => m.MachineID == machineID && m.ColorCode == colorCode && m.IsCompleted == 0)
        //                    //            .OrderByDescending(m => m.InsertedOn).FirstOrDefault();

        //                    #region Insert to Operator Dashboard

        //                    //Pre requsite before insertion ???

        //                    I_Facility.ServerModel.tbloperatordashboard operatorDashboard = new I_Facility.ServerModel.tbloperatordashboard();
        //                    var lossdesc = _UWcontext.tbllossescodes.Find(LossSelect);
        //                    DateTime CorrectedDateToDate = Convert.ToDateTime(correctedDate);
        //                    Random OperatorDashboardID = new Random();
        //                    machineID = item.MachineID;// Convert.ToInt32(Session["MachineID"]);
        //                                               //operatorDashboard.OperatorDashboardID = OperatorDashboardID.Next(1, 9999999);  //remove this line once Identity is setup
        //                    operatorDashboard.MachineID = item.MachineID;// Convert.ToInt32(Session["MachineID"]);
        //                    operatorDashboard.CorrectedDate = CorrectedDateToDate;
        //                    operatorDashboard.SlNo = _UWcontext.tbloperatordashboards.Where(m => m.CorrectedDate == CorrectedDateToDate).Where(m => m.MachineID == machineID).ToList().Count + 1; //  @Pavan , I'm not sure what you meant here..  --Count++ from the tbloperatordashboard for the Machine and CorrectedDate
        //                    operatorDashboard.MessageCode = "IDLE";
        //                    operatorDashboard.MessageDescription = "Machine in IDLE Mode" + "-" + lossdesc.LossCode;
        //                    operatorDashboard.MessageStartTime = ModeStartTime;
        //                    operatorDashboard.MessageEndTime = DateTime.Now;
        //                    operatorDashboard.TotalDurationinMin = Convert.ToInt32(DateTime.Now.Subtract(ModeStartTime).TotalMinutes);
        //                    operatorDashboard.InsertedOn = DateTime.Now;
        //                    operatorDashboard.InsertedBy = Convert.ToInt32(Session["UserID"]);
        //                    //operatorDashboard.ModifiedOn = DateTime.Now;
        //                    //operatorDashboard.ModifiedBy = 1;  // Session["UserID"]
        //                    operatorDashboard.IsDeleted = 0;

        //                    _UWcontext.tbloperatordashboards.Add(operatorDashboard);
        //                    _UWcontext.SaveChanges();

        //                    #endregion



        //                }
        //                else
        //                {

        //                }

        //                //#region UnLock the Machine
        //                //var MacDet = _UWcontext.tblmachinedetails.Find(machineID);
        //                //if (MacDet.MachineModelType == 1)
        //                //{
        //                //    AddFanucMachineWithConn AC = new AddFanucMachineWithConn(MacDet.IPAddress);
        //                //    AC.SetMachineUnlock((ushort)MacDet.MachineUnlockBit, (ushort)MacDet.MachineLockBit);
        //                //}
        //                //#endregion



        //                #region Update tblOperatorHeader 
        //                int MachineID = item.MachineID;
        //                //get the latest row according to the MachineID and CorrectedDate ->  there is no CorrectedDate column 
        //                var operatorHeader = _UWcontext.tbloperatorheaders.Where(m => m.MachineID == MachineID).OrderByDescending(m => m.InsertedOn).FirstOrDefault();
        //                if (operatorHeader != null)
        //                {
        //                    operatorHeader.MachineMode = "Production";
        //                    operatorHeader.ModifiedOn = DateTime.Now;
        //                    operatorHeader.ModifiedBy = 1;// get from session once these screens are integrated....

        //                    _UWcontext.Entry(operatorHeader).State = EntityState.Modified;
        //                    _UWcontext.SaveChanges();
        //                }

        //                #endregion
        //            }

        //        }

        //        //return RedirectToAction("DashboardProduction");
        //        return RedirectToAction("EntryWindow");
        //    }

        //    //public ActionResult ToolLife()
        //    //{
        //    //    GetMode GM = new GetMode();
        //    //    String IPAddress = GM.GetIPAddressofTabSystem();

        //    //    int MachineID = _UWcontext.tblmachinedetails.Where(m => m.TabIPAddress == IPAddress && m.IsDeleted == 0).Select(m => m.MachineID).First();

        //    //    Session["MachineID"] = MachineID;
        //    //    var HeaderDet = _UWcontext.tbloperatorheaders.Where(m => m.MachineID == MachineID).SingleOrDefault();
        //    //    ViewBag.MachineMode = HeaderDet.MachineMode;
        //    //    ViewBag.MachineName = HeaderDet.tblmachinedetail.MachineDisplayName;
        //    //    ViewBag.TabStatus = HeaderDet.TabConnecStatus;
        //    //    ViewBag.ServerStatus = HeaderDet.ServerConnecStatus;
        //    //    ViewBag.PageName = "Tool Life";
        //    //    ViewBag.Shift = HeaderDet.Shift;

        //    //    var prvmode = _UWcontext.tblmodes.Where(m => m.MachineID == MachineID && m.ModeType == "SETUP" && m.ModeTypeEnd == 0)
        //    //            .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
        //    //    if (prvmode != null)
        //    //    {
        //    //        ViewBag.SetUpStarted = "1";
        //    //        ViewBag.MachineMode = "Setting";
        //    //    }

        //    //    var prvmodeMaint = _UWcontext.tblmodes.Where(m => m.MachineID == MachineID && m.ModeType == "MNT" && m.ModeTypeEnd == 0)
        //    //            .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
        //    //    if (prvmodeMaint != null)
        //    //    {
        //    //        ViewBag.MNTStarted = "1";
        //    //        ViewBag.MachineMode = "MNT";
        //    //    }
        //    //    var toolLifeOPerator = _UWcontext.tbltoollifeoperators.Where(m => m.IsDeleted == 0 && m.IsReset == 0 && m.IsCompleted == false).OrderByDescending(m => m.toollifecounter).ToList();
        //    //    return View(toolLifeOPerator);
        //    //}

        //    [OutputCache(NoStore = true, Location = System.Web.UI.OutputCacheLocation.Client, Duration = 5)] // every 3 sec
        //    public ActionResult ToolLifeData()
        //    {
        //        var toolLifeOPerator = _UWcontext.tbltoollifeoperators.Where(m => m.IsDeleted == 0 && m.IsReset == 0 && m.IsCompleted == false).OrderByDescending(m => m.toollifecounter).ToList();
        //        return PartialView("ToolLifeData", toolLifeOPerator);
        //    }

        //    //public ActionResult ResetToolLifeCounter(string id, string resetreason)
        //    //{
        //    //    int toolLifeopId = 0;
        //    //    if (id == "0" || id == "" || id == null)
        //    //    {
        //    //    }
        //    //    else
        //    //    {
        //    //        toolLifeopId = Convert.ToInt32(id);

        //    //        #region//updation in tbltoollife operator in server and tab

        //    //        var updateTabTool = _UWcontext.tbltoollifeoperators.Find(toolLifeopId);
        //    //        var updateServerTool = _ServerContext.tbltoollifeoperators.Find(updateTabTool.ToolIDAdmin);

        //    //        updateTabTool.IsReset = 1;
        //    //        _UWcontext.SaveChanges();
        //    //        updateServerTool.IsReset = 1;
        //    //        _ServerContext.SaveChanges();

        //    //        #endregion

        //    //        #region // insertion in tbltoollifeoperator in server and tab

        //    //        var itemTool = _UWcontext.tbltoollifeoperators.Find(toolLifeopId);
        //    //        var serVeritemTool = _ServerContext.tbltoollifeoperators.Find(itemTool.ToolIDAdmin);

        //    //        //addding in server
        //    //        tbltoollifeoperator ServertabTLO = new tbltoollifeoperator();

        //    //        ServertabTLO.MachineID = serVeritemTool.MachineID;
        //    //        ServertabTLO.ToolNo = serVeritemTool.ToolNo;
        //    //        ServertabTLO.ToolName = serVeritemTool.ToolName;
        //    //        //ServertabTLO.ToolPrtCode = serVeritemTool.ToolPrtCode;
        //    //        ServertabTLO.StandardToolLife = serVeritemTool.StandardToolLife;
        //    //        ServertabTLO.toollifecounter = 0;
        //    //        ServertabTLO.InsertedOn = DateTime.Now;
        //    //        ServertabTLO.InsertedBy = serVeritemTool.InsertedBy;
        //    //        ServertabTLO.IsReset = 0;
        //    //        ServertabTLO.IsDeleted = 0;
        //    //        ServertabTLO.ResetCounter = 0;
        //    //        ServertabTLO.HMIID = serVeritemTool.HMIID;
        //    //        ServertabTLO.Sync = 1;
        //    //        ServertabTLO.ResetReason = resetreason;
        //    //        ServertabTLO.IsCompleted = false;
        //    //        _ServerContext.tbltoollifeoperators.Add(ServertabTLO);
        //    //        _ServerContext.SaveChanges();

        //    //        int serverTLOp = ServertabTLO.ToolLifeID;
        //    //        //adding to tab
        //    //        //tbltoollifeoperator tabTLO = new tbltoollifeoperator();
        //    //        //tabTLO.MachineID = itemTool.MachineID;
        //    //        //tabTLO.ToolNo = itemTool.ToolNo;
        //    //        //tabTLO.ToolName = itemTool.ToolName;
        //    //        ////tabTLO.ToolPrtCode = itemTool.ToolPrtCode;
        //    //        //tabTLO.StandardToolLife = itemTool.StandardToolLife;
        //    //        //tabTLO.toollifecounter = 0;
        //    //        //tabTLO.InsertedOn = DateTime.Now;
        //    //        //tabTLO.InsertedBy = itemTool.StandardToolLife;
        //    //        //tabTLO.IsReset = 0;
        //    //        //tabTLO.IsDeleted = 0;
        //    //        //tabTLO.ResetCounter = 0;
        //    //        //tabTLO.HMIID = itemTool.HMIID;
        //    //        //tabTLO.HMIID = itemTool.HMIID;
        //    //        //tabTLO.Sync = 1;
        //    //        //tabTLO.ToolIDAdmin = serverTLOp;
        //    //        //tabTLO.ResetReason = resetreason;
        //    //        //tabTLO.IsCompleted = false;
        //    //        //_UWcontext.tbltoollifeoperators.Add(tabTLO);
        //    //        //_UWcontext.SaveChanges();
        //    //        #endregion
        //    //    }
        //    //    return RedirectToAction("ToolLife");
        //    //}

        //    public ContentResult lastNodeCheck(int id)
        //    {
        //        var tblLossCodes = _UWcontext.tbllossescodes.ToList();

        //        if (tblLossCodes.Find(level => level.LossCodesLevel == 3 && level.LossCodesLevel2ID == id) == null) { return Content("true/" + id); }

        //        return Content("false/" + id);
        //    }

        //    public ContentResult lastNodeIdleCheck(int id, int lev)
        //    {
        //        var tblLossCodes = _UWcontext.tbllossescodes.ToList();

        //        if (lev == 1)
        //        {
        //            if (tblLossCodes.Find(level => level.LossCodesLevel == 2 && level.LossCodesLevel1ID == id && level.IsDeleted == 0) == null) { return Content("true/" + id); }
        //            else
        //            {
        //                return Content("false/" + id);
        //            }
        //        }
        //        else
        //        {
        //            if (tblLossCodes.Find(level => level.LossCodesLevel == 3 && level.LossCodesLevel2ID == id && level.IsDeleted == 0) == null) { return Content("true/" + id); }

        //            return Content("false/" + id);
        //        }
        //    }

        //    public JsonResult CheckIdle()
        //    {
        //        _ServerContext.Database.CommandTimeout = 180;
        //        GetMode GM = new GetMode();
        //        int Data = 0;

        //        String IPAddress = GM.GetIPAddressofTabSystem();

        //        //int MachineID = _UWcontext.tblmachinedetails.Where(m => m.TabIPAddress == IPAddress && m.IsDeleted == 0).Select(m => m.MachineID).First();
        //        var celliddet = Session["CellID"];
        //        int cellid = Convert.ToInt32(celliddet);
        //        var machinedet = _UWcontext.tblmachinedetails.Where(m => m.CellID == cellid && m.IsDeleted == 0).ToList();
        //        foreach (var item in machinedet)
        //        {
        //            int MachineID = item.MachineID;

        //            Session["MachineID"] = MachineID;

        //            GM.UpdateOperatorHeader(MachineID);
        //            var toolCounter = _UWcontext.tbltoollifeoperators.Where(m => m.toollifecounter == m.StandardToolLife).Where(m => m.IsCompleted == false && m.IsReset == 0 && m.IsDeleted == 0).ToList();

        //            bool IdleStatus = GM.CheckIdleEntry(MachineID);
        //            if (IdleStatus)
        //                Data = 1;
        //            int toolcount = toolCounter.Count();
        //            if (Data == 1 && toolcount == 0)
        //            {
        //                Data = 1;
        //            }
        //            else if (Data == 1 && toolcount > 0)
        //            {
        //                Data = 1;
        //            }
        //            else if (Data != 1 && toolcount > 0)
        //            {
        //                Data = 2;
        //            }
        //            else
        //            {
        //                Data = 0;
        //            }
        //            if (Data == 1)
        //            {
        //                return Json(Data, JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //        return Json(Data, JsonRequestBehavior.AllowGet);
        //    }

        //    public JsonResult RefreshData()
        //    {
        //        List<string> retValList = new List<string>();
        //        GetMode GM = new GetMode();
        //        String IPAddress = GM.GetIPAddressofTabSystem();

        //        //int MachineID = _UWcontext.tblmachinedetails.Where(m => m.TabIPAddress == IPAddress && m.IsDeleted == 0).Select(m => m.MachineID).First();

        //        var celliddet = Session["CellID"];
        //        int cellid = Convert.ToInt32(celliddet);
        //        var machinedet = _UWcontext.tblmachinedetails.Where(m => m.CellID == cellid && m.IsDeleted == 0 && m.IsLastMachine == 1).ToList();
        //        foreach (var item in machinedet)
        //        {
        //            int MachineID = item.MachineID;
        //            Session["MachineID"] = MachineID;
        //            GM.UpdateOperatorHeader(MachineID);
        //            retValList = new List<string>();

        //            var HeaderDet = _UWcontext.tbloperatorheaders.Where(m => m.MachineID == MachineID).SingleOrDefault();
        //            if (HeaderDet != null)
        //            {
        //                retValList.Add(HeaderDet.tblmachinedetail.MachineDisplayName); //0
        //                retValList.Add(HeaderDet.TabConnecStatus); //1
        //                retValList.Add(HeaderDet.ServerConnecStatus); //2
        //                retValList.Add(HeaderDet.MachineMode); //3
        //                retValList.Add(HeaderDet.Shift); //4
        //                retValList.Add(MachineID.ToString()); //5
        //                retValList.Add(IPAddress); //6
        //                                           //string errormessage = Session["setuperror"].ToString();
        //            }

        //        }
        //        var result = new { RetVal = retValList };
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }

        //    //public int GetYieldQty(string Operator, int PartNo, string Shift)
        //    //{
        //    //    var celliddet = Session["CellID"];
        //    //    int cellid = 0;
        //    //    DateTime correcteddate1 = DateTime.Now.Date;
        //    //    string correcteddate = correcteddate1.ToString("yyyy-mm-dd");
        //    //    int yieldval = 0, fpart = 0, lpart = 0, fval = 0, lval = 0;
        //    //    string OperatorID = "";
        //    //    if (Operator != null)
        //    //    {
        //    //        int count = Operator.Count();

        //    //        for (int i = 0; i < count; i++)
        //    //        {
        //    //            OperatorID = OperatorID + Operator[i] + ",";
        //    //        }
        //    //    }
        //    //    var operatorentrydet = _UWcontext.tbloperatorentries.Where(m => m.isDeleted == 0 && m.OPID == OperatorID && m.Shift == Shift && m.CorrectedDate == correcteddate1.Date).Select(m => m.CreatedOn).FirstOrDefault();
        //    //    //var celldet = db.tblcells.Where(m => m.IsDeleted == 0).Select(m => m.CellID).ToList();
        //    //    //foreach (var cellid in celldet)
        //    //    //{
        //    //    //    int cell = cellid;
        //    //    if (celliddet != null)
        //    //    {
        //    //        cellid = Convert.ToInt32(celliddet);
        //    //        var machinedet = _UWcontext.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsLastMachine == 1 && m.CellID == cellid).ToList();
        //    //        foreach (var item in machinedet)
        //    //        {
        //    //            DateTime value = DateTime.Now;
        //    //            if (operatorentrydet != null)
        //    //                value = Convert.ToDateTime(operatorentrydet.Value);
        //    //            DateTime stime = value;
        //    //            DateTime etime = DateTime.Now;
        //    //            int machineid = item.MachineID;
        //    //            var parama = _UWcontext.parameters_master.Where(m => m.MachineID == machineid && m.InsertedOn >= stime && m.InsertedOn <= etime).OrderBy(m => m.InsertedOn).Select(m => m.PartsTotal).FirstOrDefault();
        //    //            var paramd = _UWcontext.parameters_master.Where(m => m.MachineID == machineid && m.InsertedOn >= stime && m.InsertedOn <= etime).OrderByDescending(m => m.InsertedOn).Select(m => m.PartsTotal).FirstOrDefault();
        //    //            if (parama != null)
        //    //            {
        //    //                fval = parama.Value;
        //    //            }
        //    //            if (paramd != null)
        //    //            {
        //    //                lval = paramd.Value;
        //    //            }
        //    //            fpart = fval;
        //    //            lpart = lval;
        //    //            yieldval = lpart - fpart + 1;
        //    //            //getting yield value update to table
        //    //            tblworkorderentry uptabl = _UWcontext.tblworkorderentries.FirstOrDefault(x => x.OperatorID == OperatorID);
        //    //            if (uptabl != null)
        //    //            {
        //    //                uptabl.Yield_Qty = yieldval;
        //    //                _UWcontext.SaveChanges();
        //    //            }
        //    //            //break;
        //    //        }
        //    //    }
        //    //    //}
        //    //    return yieldval;
        //    //}

        //    //public string GetYieldQty(int PartNo, int Shift, string[] Operatorname)
        //    public string GetYieldQty(operatorgetqty getqty)
        //    {
        //        int PartNo = getqty.PartNumber;
        //        int Shift = getqty.ShiftID;
        //        string[] Operatorname = getqty.Operatorname;

        //        string[] Operator = Operatorname;
        //        var celliddet = Session["CellID"];
        //        int cellid = Convert.ToInt32(celliddet);
        //        DateTime correcteddate1 = DateTime.Now.Date;
        //        string correcteddate = correcteddate1.ToString("yyyy-MM-dd");
        //        int yieldval = 0, fpart = 0, lpart = 0, fval = 0, lval = 0;
        //        string OperatorID = "";
        //        if (Operator != null)
        //        {
        //            int count = Operator.Count();

        //            for (int i = 0; i < count; i++)
        //            {
        //                OperatorID = OperatorID + Operator[i] + ",";
        //            }
        //        }
        //        var operatorentrydet = _UWcontext.tblworkorderentries.Where(m => m.OperatorID == OperatorID && m.ShiftID == Shift && m.CorrectedDate == correcteddate && m.CellID == cellid).Select(m => m.WOStart).FirstOrDefault();
        //        //var celldet = db.tblcells.Where(m => m.IsDeleted == 0).Select(m => m.CellID).ToList();
        //        //foreach (var cellid in celldet)
        //        //{
        //        //    int cell = cellid;

        //        if (celliddet != null)
        //        {
        //            cellid = Convert.ToInt32(celliddet);
        //            var machinedet = _UWcontext.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsLastMachine == 1 && m.CellID == cellid).ToList();
        //            foreach (var item in machinedet)
        //            {
        //                DateTime value = DateTime.Now;
        //                if (operatorentrydet != null)
        //                    value = Convert.ToDateTime(operatorentrydet);
        //                DateTime stime = value;
        //                DateTime etime = DateTime.Now;
        //                int machineid = item.MachineID;
        //                var parama = _UWcontext.parameters_master.Where(m => m.MachineID == machineid && m.InsertedOn >= stime && m.InsertedOn <= etime).OrderBy(m => m.InsertedOn).Select(m => m.PartsTotal).FirstOrDefault();
        //                var paramd = _UWcontext.parameters_master.Where(m => m.MachineID == machineid && m.InsertedOn >= stime && m.InsertedOn <= etime).OrderByDescending(m => m.InsertedOn).Select(m => m.PartsTotal).FirstOrDefault();
        //                if (parama != null)
        //                {
        //                    fval = parama.Value;
        //                }
        //                if (paramd != null)
        //                {
        //                    lval = paramd.Value;
        //                }
        //                fpart = fval;
        //                lpart = lval;
        //                yieldval = lpart - fpart + 1;
        //                //getting yield value update to table
        //                tblworkorderentry uptabl = _UWcontext.tblworkorderentries.Where(x => x.OperatorID == OperatorID && x.CorrectedDate == correcteddate && x.CellID == cellid).FirstOrDefault();
        //                if (uptabl != null)
        //                {
        //                    uptabl.Yield_Qty = yieldval;
        //                    _UWcontext.SaveChanges();
        //                }
        //                //break;
        //            }
        //        }
        //        //}
        //        return yieldval.ToString();
        //    }

        //    public ActionResult SelectMachineAdmin(int PlantID = 0, int ShopID = 0, int CellID = 0, int WorkCenterID = 0)
        //    {
        //        ViewData["PlantID"] = new SelectList(_UWcontext.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", PlantID);
        //        ViewData["ShopID"] = new SelectList(_UWcontext.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID), "ShopID", "ShopName", ShopID);
        //        ViewData["CellID"] = new SelectList(_UWcontext.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID), "CellID", "CellName", CellID);
        //        ViewData["WorkCenterID"] = new SelectList(_UWcontext.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID && m.CellID == CellID && m.IsNormalWC == 0), "MachineID", "MachineDisplayName", WorkCenterID);
        //        return View();
        //    }

        //    public string autoPopulateOperatorEntry(string Prod_Order_No, string OperationNo)
        //    {
        //        string Part_NoOrderQty = "";
        //        string Part_No = "", ProductionOrderQty = "";
        //        var item = _ServerContext.tblprodplanmasters.Where(m => m.Prod_Order_No == Prod_Order_No && m.OperationNo == OperationNo).FirstOrDefault();

        //        string status = item.Status;
        //        int stat = 0;// 1=SplitOrderQty 0=OrderQty
        //        if (status == "SPLT")
        //        {
        //            stat = 1;
        //        }
        //        else
        //        {
        //            int count = _ServerContext.tblworkorderentries.Where(m => m.Prod_Order_No == Prod_Order_No && m.isSplit == true).Count();

        //            if (count > 0)
        //            {
        //                stat = 1;
        //            }
        //            else
        //            {
        //                stat = 0;
        //            }
        //        }

        //        if (stat == 1)
        //        {
        //            ProductionOrderQty = item.SplitOrderQty.ToString();
        //        }
        //        else
        //        {
        //            ProductionOrderQty = item.OrderQty.ToString();
        //        }

        //        Part_No = item.FGCode;
        //        Part_NoOrderQty = Part_No + '#' + ProductionOrderQty;

        //        return Part_NoOrderQty;
        //    }

        //    [HttpPost]
        //    public string ServerPing()
        //    {
        //        string Status = "Connected";
        //        GetMode GM = new GetMode();
        //        Ping ping = new Ping();
        //        String TabIPAddress = GM.GetIPAddressofTabSystem();
        //        var MachineDetails = _UWcontext.tblmachinedetails.Where(m => m.TabIPAddress == TabIPAddress && m.IsDeleted == 0).FirstOrDefault();

        //        //try
        //        //{
        //        //    PingReply pingresult = ping.Send(MachineDetails.ServerIPAddress);
        //        //    if (pingresult.Status.ToString() == "Success")
        //        //    {
        //        //        Status = "Connected";
        //        //    }
        //        //}
        //        //catch
        //        //{
        //        //    Status = "Disconnected";
        //        //}
        //        return Status;
        //    }
        //}

        //public class FromCollection
        //{
        //    public void GetOperatorHeader()
        //    {

        //    }
    }
}