using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SRKSDemo;
using SRKSDemo.Server_Model;
namespace SRKSDemo.Controllers
{
    public class OperatorEntryAdminController : Controller
    {

      
        //public CCSOEEServerEntities _ServerContext = new CCSOEEServerEntities();
        public i_facility_shaktiEntities1 _ServerContext = new i_facility_shaktiEntities1();

        public ActionResult SelectMachineAdmin(int PlantID = 0, int ShopID = 0, int CellID = 0, int WorkCenterID = 0)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            Session["MachineID"] = 0;
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];

            ViewData["PlantID"] = new SelectList(_ServerContext.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(_ServerContext.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(_ServerContext.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["MachineID"] = new SelectList(_ServerContext.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");
            return View();
        }

        [HttpPost]
        public ActionResult SelectMachineAdmin(int MachineID)
        {
            Session["MachineID"] = MachineID;
            return Redirect("/OperatorEntryAdmin/EntryWindowAdmin?machineId="+MachineID+"");
        }

            // GET: OperatorEntryAdmin
        public ActionResult EntryWindowAdmin(int machineId,int ShiftID = 0)
        {

            int MachineID = machineId;

            Session["MachineID"] = MachineID;

            var HeaderDet = _ServerContext.tblOperatorHeaders.Where(m => m.MachineID == MachineID).SingleOrDefault();
            ViewBag.MachineMode = HeaderDet.MachineMode;
            ViewBag.MachineName = HeaderDet.tblmachinedetail.MachineDisplayName;
            ViewBag.TabStatus = HeaderDet.TabConnecStatus;
            ViewBag.ServerStatus = HeaderDet.ServerConnecStatus;
            ViewBag.PageName = "Operator Entry";
            ViewBag.Shift = HeaderDet.Shift;
            var prvmode = _ServerContext.tblmodes.Where(m => m.MachineID == MachineID && m.ModeType == "SETUP" && m.ModeTypeEnd == 0)
                    .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
            if (prvmode != null)
            {
                ViewBag.SetUpStarted = "1";
                ViewBag.MachineMode = "Setting";
            }

            var prvmodeMaint = _ServerContext.tblmodes.Where(m => m.MachineID == MachineID && m.ModeType == "MNT" && m.ModeTypeEnd == 0)
                    .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
            if (prvmodeMaint != null)
            {
                ViewBag.MNTStarted = "1";
                ViewBag.MachineMode = "MNT";
            }

            ViewBag.HoldReason = new SelectList(_ServerContext.tblholdcodes.Where(m => m.IsDeleted == 0), "HoldCodeID", "HoldCodeDesc");
            var WOE = _ServerContext.tblworkorderentries.Where(m => m.MachineID == MachineID && m.IsFinished == 0 && m.IsStarted == 1).OrderByDescending(m => m.HMIID).ToList().FirstOrDefault();

            if (WOE != null)
            {
                ShiftID = WOE.ShiftID;
            }
            var sItem = _ServerContext.tblshift_mstr.Where(m => m.IsDeleted == 0).ToList();
            ViewData["ShiftID"] = new SelectList(_ServerContext.tblshift_mstr.Where(m => m.IsDeleted == 0), "ShiftID", "ShiftName", ShiftID);
            return View(WOE);
        }

        //[HttpPost]
        //public ActionResult EntryWindowAdmin(SRKSDemo.Models.tabtblworkorderentry WOE,int machineId)
        //{
        //    int MachineID = machineId;

        //    ////Session["MachineID"] = MachineID;
        //    //var HeaderDet = _ServerContext.tblOperatorHeaders.Where(m => m.MachineID == MachineID).SingleOrDefault();
        //    //ViewBag.MachineMode = HeaderDet.MachineMode;
        //    //ViewBag.MachineName = HeaderDet.tblmachinedetail.MachineDisplayName;
        //    //ViewBag.TabStatus = HeaderDet.TabConnecStatus;
        //    //ViewBag.ServerStatus = HeaderDet.ServerConnecStatus;
        //    //ViewBag.PageName = "Operator Entry";
        //    //ViewBag.Shift = HeaderDet.Shift;
        //    //var prvmode = _ServerContext.tblmodes.Where(m => m.MachineID == MachineID && m.ModeType == "SETUP" && m.ModeTypeEnd == 0)
        //    //        .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
        //    //if (prvmode != null)
        //    //{
        //    //    ViewBag.SetUpStarted = "1";
        //    //    ViewBag.MachineMode = "Setting";
        //    //}

        //    //var prvmodeMaint = _UWcontext.tabtblmodes.Where(m => m.MachineID == MachineID && m.ModeType == "MNT" && m.ModeTypeEnd == 0)
        //    //        .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
        //    //if (prvmodeMaint != null)
        //    //{
        //    //    ViewBag.MNTStarted = "1";
        //    //    ViewBag.MachineMode = "MNT";
        //    //}

        //    //DateTime correctedDate = DateTime.Now;
        //    //SRKSDemo.Models.tabtbldaytiming StartTime = _UWcontext.tabtbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
        //    //TimeSpan Start = StartTime.StartTime;
        //    //if (Start.Hours < DateTime.Now.Hour)
        //    //{
        //    //    correctedDate = DateTime.Now;
        //    //}
        //    //else
        //    //{
        //    //    correctedDate = DateTime.Now.AddDays(-1);
        //    //}

        //    //int PrvTotalQty = 0;
        //    //int PrvProcessQty = 0;

        //    //var GetProcessQty = _UWcontext.tabtblworkorderentries.Where(m => m.Prod_Order_No == WOE.Prod_Order_No && m.OperationNo == WOE.OperationNo).OrderByDescending(m => m.HMIID).FirstOrDefault();
        //    //if (GetProcessQty != null)
        //    //{
        //    //    PrvTotalQty = GetProcessQty.Total_Qty;
        //    //    PrvProcessQty = GetProcessQty.ProcessQty;
        //    //}

        //    //#region check split

        //    //var item = _ServerContext.tblProdPlanMasters.Where(m => m.Prod_Order_No == WOE.Prod_Order_No && m.OperationNo == WOE.OperationNo).FirstOrDefault();
        //    //int stat = 0;// 1=SplitOrderQty 0=OrderQty
        //    //if (item != null)
        //    //{
        //    //    string status = item.Status;
        //    //    if (status == "SPLT")
        //    //    {
        //    //        stat = 1;
        //    //    }
        //    //}
        //    //#endregion

        //    //SRKSDemo.Models.tabtblworkorderentry WOEntry = new SRKSDemo.Models.tabtblworkorderentry();
        //    //WOEntry.MachineID = MachineID;
        //    //WOEntry.OperationNo = WOE.OperationNo;
        //    //WOEntry.OperatorID = WOE.OperatorID;
        //    //WOEntry.PartNo = WOE.PartNo;
        //    //WOEntry.PEStartTime = DateTime.Now;
        //    //WOEntry.Prod_Order_No = WOE.Prod_Order_No;
        //    //WOEntry.ScrapQty = 0;
        //    //WOEntry.ShiftID = WOE.ShiftID;
        //    //WOEntry.Total_Qty = 0;
        //    //WOEntry.WOStart = DateTime.Now;
        //    //WOEntry.Yield_Qty = 0;
        //    //WOEntry.IsStarted = 1;
        //    //WOEntry.CorrectedDate = correctedDate;
        //    //WOEntry.ProcessQty = PrvProcessQty + PrvTotalQty;
        //    //WOEntry.FGCode = WOE.FGCode;
        //    //WOEntry.ProdOrderQty = WOE.ProdOrderQty;
        //    //WOEntry.Status = 0;
        //    //WOEntry.IsFinished = 0;
        //    //WOEntry.isWorkOrder = 1;
        //    //WOEntry.IsMultiWO = 0;
        //    //WOEntry.IsFlag = 0;
        //    //WOEntry.IsHold = 0;
        //    //WOEntry.SyncInsert = 0;
        //    //if (stat == 1)
        //    //{
        //    //    WOEntry.isSplit = true;
        //    //}
        //    //else
        //    //{
        //    //    WOEntry.isSplit = false;
        //    //}
        //    //_UWcontext.tabtblworkorderentries.Add(WOEntry);
        //    //_UWcontext.SaveChanges();

        //    //int hmmid = WOEntry.HMIID;


        //    //string operationNo = WOE.OperationNo;
        //    //string FGCode = WOE.FGCode;

        //    //var toolObj = _ServerContext.tblStdToolLives.Where(m => m.FGCode == FGCode && m.OperationNo == operationNo && m.IsDeleted == false).ToList();

        //    //foreach (var itemTool in toolObj)
        //    //{
        //    //    int ToolLifeCount = 0;
        //    //    GetToolLifeCounter(FGCode, operationNo, itemTool.ToolNo, ref ToolLifeCount);
        //    //    //adding new row server
        //    //    tbltoollifeoperator ServertabTLO = new tbltoollifeoperator();
        //    //    ServertabTLO.MachineID = MachineID;
        //    //    ServertabTLO.ToolNo = itemTool.ToolNo;
        //    //    ServertabTLO.ToolName = itemTool.ToolName;
        //    //    ServertabTLO.ToolCTCode = itemTool.CTCode;
        //    //    ServertabTLO.StandardToolLife = itemTool.StdToolLife;
        //    //    ServertabTLO.toollifecounter = ToolLifeCount;
        //    //    ServertabTLO.InsertedOn = DateTime.Now;
        //    //    ServertabTLO.InsertedBy = Convert.ToInt32(WOE.OperatorID);
        //    //    ServertabTLO.IsReset = 0;
        //    //    ServertabTLO.IsDeleted = 0;
        //    //    ServertabTLO.ResetCounter = 0;
        //    //    ServertabTLO.Sync = 1;
        //    //    ServertabTLO.IsCompleted = false;
        //    //    ServertabTLO.IsCycleStart = false;
        //    //    _ServerContext.tbltoollifeoperators.Add(ServertabTLO);
        //    //    _ServerContext.SaveChanges();

        //    //    int serverTLOp = ServertabTLO.ToolLifeID;
        //    //    //adding to tab
        //    //    tabtbltoollifeoperator tabTLO = new tabtbltoollifeoperator();
        //    //    tabTLO.MachineID = MachineID;
        //    //    tabTLO.ToolNo = itemTool.ToolNo;
        //    //    tabTLO.ToolName = itemTool.ToolName;
        //    //    tabTLO.ToolCTCode = itemTool.CTCode;
        //    //    tabTLO.StandardToolLife = itemTool.StdToolLife;
        //    //    tabTLO.toollifecounter = ToolLifeCount;
        //    //    tabTLO.InsertedOn = DateTime.Now;
        //    //    tabTLO.InsertedBy = Convert.ToInt32(WOE.OperatorID);
        //    //    tabTLO.IsReset = 0;
        //    //    tabTLO.IsDeleted = 0;
        //    //    tabTLO.ResetCounter = 0;
        //    //    tabTLO.TabHMIID = hmmid;
        //    //    tabTLO.Sync = 1;
        //    //    tabTLO.ToolIDAdmin = serverTLOp;
        //    //    tabTLO.IsCompleted = false;
        //    //    _UWcontext.tabtbltoollifeoperators.Add(tabTLO);
        //    //    _UWcontext.SaveChanges();
        //    //}

        //    return RedirectToAction("EntryWindow");
        //}


        public ActionResult ToolLifeAdmin(int machineId)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            
            int MachineID = machineId;

            Session["MachineID"] = MachineID;
            var HeaderDet = _ServerContext.tblOperatorHeaders.Where(m => m.MachineID == MachineID).SingleOrDefault();
            ViewBag.MachineMode = HeaderDet.MachineMode;
            ViewBag.MachineName = HeaderDet.tblmachinedetail.MachineDisplayName;
            ViewBag.TabStatus = HeaderDet.TabConnecStatus;
            ViewBag.ServerStatus = HeaderDet.ServerConnecStatus;
            ViewBag.PageName = "Tool Life";
            ViewBag.Shift = HeaderDet.Shift;

            var prvmode = _ServerContext.tblmodes.Where(m => m.MachineID == MachineID && m.ModeType == "SETUP" && m.ModeTypeEnd == 0)
                    .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
            if (prvmode != null)
            {
                ViewBag.SetUpStarted = "1";
                ViewBag.MachineMode = "Setting";
            }

            var prvmodeMaint = _ServerContext.tblmodes.Where(m => m.MachineID == MachineID && m.ModeType == "MNT" && m.ModeTypeEnd == 0)
                    .OrderByDescending(m => m.InsertedOn).FirstOrDefault();
            if (prvmodeMaint != null)
            {
                ViewBag.MNTStarted = "1";
                ViewBag.MachineMode = "MNT";
            }
            var toolLifeOPerator = _ServerContext.tbltoollifeoperators.Where(m => m.IsDeleted == 0 && m.IsReset == 0/* && m.IsCompleted == false*/).OrderByDescending(m => m.toollifecounter).ToList();
            return View(toolLifeOPerator);
        }

        [OutputCache(NoStore = true, Location = System.Web.UI.OutputCacheLocation.Client, Duration = 5)] // every 3 sec
        public ActionResult ToolLifeDataAdmin()
        {
            var toolLifeOPerator = _ServerContext.tbltoollifeoperators.Where(m => m.IsDeleted == 0 && m.IsReset == 0 /*&& m.IsCompleted == false*/).OrderByDescending(m => m.toollifecounter).ToList();
            return PartialView( toolLifeOPerator);
        }

        //public ActionResult ResetToolLifeCounterAdmin(string id, string resetreason)
        //{
        //    int toolLifeopId = 0;
        //    if (id == "0" || id == "" || id == null)
        //    {
        //    }
        //    else
        //    {
        //        toolLifeopId = Convert.ToInt32(id);

        //        #region//updation in tbltoollife operator in server and tab

        //        var updateTabTool = _UWcontext.tabtbltoollifeoperators.Find(toolLifeopId);
        //        var updateServerTool = _ServerContext.tbltoollifeoperators.Find(updateTabTool.ToolIDAdmin);

        //        updateTabTool.IsReset = 1;
        //        _UWcontext.SaveChanges();
        //        updateServerTool.IsReset = 1;
        //        _ServerContext.SaveChanges();

        //        #endregion

        //        #region // insertion in tbltoollifeoperator in server and tab

        //        var itemTool = _UWcontext.tabtbltoollifeoperators.Find(toolLifeopId);
        //        var serVeritemTool = _ServerContext.tbltoollifeoperators.Find(itemTool.ToolIDAdmin);

        //        //addding in server
        //        tbltoollifeoperator ServertabTLO = new tbltoollifeoperator();

        //        ServertabTLO.MachineID = serVeritemTool.MachineID;
        //        ServertabTLO.ToolNo = serVeritemTool.ToolNo;
        //        ServertabTLO.ToolName = serVeritemTool.ToolName;
        //        ServertabTLO.ToolCTCode = serVeritemTool.ToolCTCode;
        //        ServertabTLO.StandardToolLife = serVeritemTool.StandardToolLife;
        //        ServertabTLO.toollifecounter = 0;
        //        ServertabTLO.InsertedOn = DateTime.Now;
        //        ServertabTLO.InsertedBy = serVeritemTool.InsertedBy;
        //        ServertabTLO.IsReset = 0;
        //        ServertabTLO.IsDeleted = 0;
        //        ServertabTLO.ResetCounter = 0;
        //        ServertabTLO.HMIID = serVeritemTool.HMIID;
        //        ServertabTLO.Sync = 1;
        //        ServertabTLO.ResetReason = resetreason;
        //        ServertabTLO.IsCompleted = false;
        //        _ServerContext.tbltoollifeoperators.Add(ServertabTLO);
        //        _ServerContext.SaveChanges();

        //        int serverTLOp = ServertabTLO.ToolLifeID;
        //        //adding to tab
        //        tabtbltoollifeoperator tabTLO = new tabtbltoollifeoperator();
        //        tabTLO.MachineID = itemTool.MachineID;
        //        tabTLO.ToolNo = itemTool.ToolNo;
        //        tabTLO.ToolName = itemTool.ToolName;
        //        tabTLO.ToolCTCode = itemTool.ToolCTCode;
        //        tabTLO.StandardToolLife = itemTool.StandardToolLife;
        //        tabTLO.toollifecounter = 0;
        //        tabTLO.InsertedOn = DateTime.Now;
        //        tabTLO.InsertedBy = itemTool.StandardToolLife;
        //        tabTLO.IsReset = 0;
        //        tabTLO.IsDeleted = 0;
        //        tabTLO.ResetCounter = 0;
        //        tabTLO.HMIID = itemTool.HMIID;
        //        tabTLO.TabHMIID = itemTool.TabHMIID;
        //        tabTLO.Sync = 1;
        //        tabTLO.ToolIDAdmin = serverTLOp;
        //        tabTLO.ResetReason = resetreason;
        //        tabTLO.IsCompleted = false;
        //        _UWcontext.tabtbltoollifeoperators.Add(tabTLO);
        //        _UWcontext.SaveChanges();
        //        #endregion
        //    }
        //    return RedirectToAction("ToolLife");
        //}

        public JsonResult RefreshData(int machineId)
        {
            List<string> retValList = new List<string>();
            GetMode GM = new GetMode();
            String IPAddress = GM.GetIPAddressofTabSystem();

            int MachineID = machineId;

            Session["MachineID"] = MachineID;
            GM.UpdateOperatorHeader(MachineID);
            retValList = new List<string>();

            var HeaderDet = _ServerContext.tblOperatorHeaders.Where(m => m.MachineID == MachineID).SingleOrDefault();
            retValList.Add(HeaderDet.tblmachinedetail.MachineDisplayName); //0
            retValList.Add(HeaderDet.TabConnecStatus); //1
            retValList.Add(HeaderDet.ServerConnecStatus); //2
            retValList.Add(HeaderDet.MachineMode); //3

            retValList.Add(HeaderDet.Shift); //4
            retValList.Add(MachineID.ToString());
            retValList.Add(IPAddress);
            //string errormessage = Session["setuperror"].ToString();

            var result = new { RetVal = retValList };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //public void GetToolLifeCounter(String FGCode, String OpNo, String ToolNum, ref int ToolLifeCount)
        //{
        //    ToolLifeCount = 0;
        //    var GetHMIID = _ServerContext.tblworkorderentries.Where(m => m.FGCode == FGCode && m.OperationNo == OpNo && m.IsFinished == 1).OrderByDescending(m => m.HMIID).FirstOrDefault();
        //    if (GetHMIID != null)
        //    {
        //        var GetToolCount = _ServerContext.tbltoollifeoperators.Where(m => m.HMIID == GetHMIID.HMIID && m.IsReset == 0 && m.ToolNo == ToolNum).OrderByDescending(m => m.ToolLifeID).FirstOrDefault();
        //        if (GetToolCount != null)
        //        {
        //            ToolLifeCount = GetToolCount.toollifecounter;
        //        }
        //    }
        //}
    }
}