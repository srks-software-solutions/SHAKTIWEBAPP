using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using SRKSDemo;
using SRKSDemo.Server_Model;
namespace SRKSDemo.Controllers
{

    public class ProgramExecutionMonitorController : Controller
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();

        public JsonResult GetShop(int PlantID)
        {
            var ShopData = (from row in db.tblshops
                            where row.IsDeleted == 0 && row.PlantID == PlantID
                            select new { Value = row.ShopID, Text = row.Shopdisplayname });
            return Json(ShopData, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCell(int ShopID)
        {
            var CellData = (from row in db.tblcells
                            where row.IsDeleted == 0 && row.ShopID == ShopID
                            select new { Value = row.CellID, Text = row.CelldisplayName });

            return Json(CellData, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetWC_Cell(int CellID)
        {
            var MachineData = (from row in db.tblmachinedetails
                               where row.IsDeleted == 0 && row.CellID == CellID
                               select new { Value = row.MachineID, Text = row.MachineDisplayName });
            return Json(MachineData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetWC_Shop(int ShopID)
        {
            var MachineData = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID && m.CellID.Equals(null)), "MachineID", "MachineDisplayName");
            return Json(MachineData, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RefreshData(int PlantID = 0, int ShopID = 0, int CellID = 0, int WorkCenterID = 0)
        {
            //Don't Delete
            //retValList Order : OpTime,CutTime,PonTime,CycleTime,FeedRate,Spindal,Program,IP,MacInv,State,RunMode,Alarm,Emergency,ATCCounter.
            int AxisCount = 32;
            string IpAddress = db.tblmachinedetails.Where(m => m.MachineID == WorkCenterID && m.IsDeleted == 0).Select(m => m.IPAddress).FirstOrDefault();
            string MacInvNo = db.tblmachinedetails.Where(m => m.MachineID == WorkCenterID && m.IsDeleted == 0).Select(m => m.MachineDisplayName).FirstOrDefault();
            string AxisCountString = Convert.ToString(db.tblmachinedetails.Where(m => m.MachineID == WorkCenterID && m.IsDeleted == 0).Select(m => m.CurrentControlAxis).FirstOrDefault());
            int MacType = Convert.ToInt32(db.tblmachinedetails.Where(m => m.MachineID == WorkCenterID && m.IsDeleted == 0).Select(m => m.MacType).FirstOrDefault());
            if (AxisCountString != null)
            {
                AxisCount = Convert.ToInt32(AxisCountString);
            }

            VirtualHMI vhmi = new VirtualHMI(IpAddress, MacInvNo);  //Initializing the Class;
                                                                    // int NoOfAxis, out List<string> retValList, out List<AxisDetails> AxisDetailsList
            List<string> retValList = new List<string>();
            List<AxisDetails> AxisDetailsList = new List<AxisDetails>();
            vhmi.VirtualDispRefersh(AxisCount, out retValList, out AxisDetailsList);

            foreach (var row in AxisDetailsList)
            {
                int AxisID = row.AxisID;
                string AxisName = db.tbl_axisdet.Where(m => m.MachineID == WorkCenterID && m.AxisID == AxisID).Select(m => m.AxisName).FirstOrDefault();
                row.AxisName = AxisName;
            }

            ViewBag.RetValList = retValList;
            ViewBag.AxisDetailsList = AxisDetailsList;

            List<string> ModelData = new List<string>();
            var ATCCounterVal = db.tblatccounters.Where(m => m.MachineID == WorkCenterID).FirstOrDefault();
            if (ATCCounterVal != null)
            {
                ModelData.Add(ATCCounterVal.Counter.ToString());
            }
            else
            {
                ModelData.Add(0.ToString());
            }
            vhmi.ModalData(ref ModelData);
            ViewBag.ModalData = ModelData;

            var result = new { RetVal = retValList, AxisDetailsList = AxisDetailsList, ModalData = ModelData };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index(int PlantID = 0, int ShopID = 0, int CellID = 0, int WorkCenterID = 0)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];

            //int.TryParse( Convert.ToString( TempData["PlantID"]),out PlantID);
            //int.TryParse( Convert.ToString( TempData["ShopID"]),out ShopID);
            //int.TryParse( Convert.ToString( TempData["CellID"]),out CellID);
            //int.TryParse( Convert.ToString( TempData["WorkCenterID"]),out WorkCenterID);

            ViewBag.ModalData = null;
            ViewBag.RetValList = null;
            ViewBag.AxisDetailsList = null;

            if (WorkCenterID != 0)
            {
                //Don't Delete
                //retValList Order : OpTime,CutTime,PonTime,CycleTime,FeedRate,Spindal,Program,IP,MacInv,State,RunMode,Alarm,Emergency,ATCCounter.
                int AxisCount = 32;
                string IpAddress = db.tblmachinedetails.Where(m => m.MachineID == WorkCenterID && m.IsDeleted == 0).Select(m => m.IPAddress).FirstOrDefault();
                string MacInvNo = db.tblmachinedetails.Where(m => m.MachineID == WorkCenterID && m.IsDeleted == 0).Select(m => m.MachineDisplayName).FirstOrDefault();
                string AxisCountString = Convert.ToString(db.tblmachinedetails.Where(m => m.MachineID == WorkCenterID && m.IsDeleted == 0).Select(m => m.CurrentControlAxis).FirstOrDefault());
                int MacType = Convert.ToInt32(db.tblmachinedetails.Where(m => m.MachineID == WorkCenterID && m.IsDeleted == 0).Select(m => m.MacType).FirstOrDefault());
                if (AxisCountString != null)
                {
                    AxisCount = Convert.ToInt32(AxisCountString);
                }

                VirtualHMI vhmi = new VirtualHMI(IpAddress, MacInvNo);  //Initializing the Class;
                // int NoOfAxis, out List<string> retValList, out List<AxisDetails> AxisDetailsList
                List<string> retValList = new List<string>();
                List<AxisDetails> AxisDetailsList = new List<AxisDetails>();
                vhmi.VirtualDispRefersh(AxisCount, out retValList, out AxisDetailsList);

                foreach (var row in AxisDetailsList)
                {
                    int AxisID = row.AxisID;
                    string AxisName = db.tbl_axisdet.Where(m => m.MachineID == WorkCenterID && m.AxisID == AxisID).Select(m => m.AxisName).FirstOrDefault();
                    row.AxisName = AxisName;
                }

                ViewBag.RetValList = retValList;
                ViewBag.AxisDetailsList = AxisDetailsList;

                List<string> ModelData = new List<string>();
                var ATCCounterVal = db.tblatccounters.Where(m => m.MachineID == WorkCenterID).FirstOrDefault();
                if (ATCCounterVal != null)
                {
                    ModelData.Add(ATCCounterVal.Counter.ToString());
                }
                else
                {
                    ModelData.Add(0.ToString());
                }
                vhmi.ModalData(ref ModelData);
                ViewBag.ModalData = ModelData;
                ViewData["PlantID"] = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", PlantID);
                ViewData["ShopID"] = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID), "ShopID", "ShopName", ShopID);
                ViewData["CellID"] = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID), "CellID", "CellName", CellID);
                ViewData["WorkCenterID"] = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID && m.CellID == CellID && m.IsNormalWC == 0), "MachineID", "MachineDisplayName", WorkCenterID);
            }
            else
            {
                ViewData["PlantID"] = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", PlantID);
                ViewData["ShopID"] = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID), "ShopID", "ShopName", ShopID);
                ViewData["CellID"] = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID), "CellID", "CellName", CellID);
                ViewData["WorkCenterID"] = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID && m.CellID == CellID && m.IsNormalWC == 0), "MachineID", "MachineDisplayName", WorkCenterID);
            }

            return View();
        }


        //public JsonResult UpdateVirtualHMI(int WorkCenterID)
        //{
        //    if (WorkCenterID != 0)
        //    {
        //        VirtualHMI vhmiobj = new VirtualHMI();
        //        try
        //        {

        //        }
        //        catch (Exception e)
        //        {
        //            return Json("Error", JsonRequestBehavior.AllowGet);
        //        }

        //        return Json(vhmiobj,JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        return Json("Error", JsonRequestBehavior.AllowGet);
        //    }

        //}

        //public ActionResult Alarms()
        //{
        //    if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
        //    {
        //        return RedirectToAction("Login", "Login", null);
        //    }
        //    ViewBag.Logout = Session["Username"].ToString().ToUpper();
        //    ViewBag.roleid = Session["RoleID"];

        //    return View();
        //}
        #region
        //[HttpPost]
        //[MultipleButton(Name = "action", Argument = "Alarms")]
        //public ActionResult Alarms(MachineHierarchy mh)
        //{
        //    if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
        //    {
        //        return RedirectToAction("Login", "Login", null);
        //    }
        //    ViewBag.Logout = Session["Username"].ToString().ToUpper();
        //    ViewBag.roleid = Session["RoleID"];

        //    ViewData["PlantID"] = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
        //    ViewData["ShopID"] = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == mh.PlantID), "ShopID", "ShopName", mh.ShopID);
        //    ViewData["CellID"] = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == mh.PlantID && m.ShopID == mh.ShopID), "CellID", "CellName", mh.CellID);
        //    ViewData["WorkCenterID"] = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == mh.PlantID && m.ShopID == mh.ShopID && m.CellID == mh.CellID && m.IsNormalWC == 0), "MachineID", "MachineInvNo", mh.WorkCenterID);


        //    return View("Alarms");
        //}

        //[HttpPost]
        //[MultipleButton(Name = "action", Argument = "OpMsgHistory")]
        //public ActionResult OpMsgHistory(MachineHierarchy mh)
        //{
        //    if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
        //    {
        //        return RedirectToAction("Login", "Login", null);
        //    }
        //    ViewBag.Logout = Session["Username"].ToString().ToUpper();
        //    ViewBag.roleid = Session["RoleID"];

        //    ViewData["PlantID"] = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
        //    ViewData["ShopID"] = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == mh.PlantID), "ShopID", "ShopName", mh.ShopID);
        //    ViewData["CellID"] = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == mh.PlantID && m.ShopID == mh.ShopID), "CellID", "CellName", mh.CellID);
        //    ViewData["WorkCenterID"] = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == mh.PlantID && m.ShopID == mh.ShopID && m.CellID == mh.CellID && m.IsNormalWC == 0), "MachineID", "MachineInvNo", mh.WorkCenterID);

        //    return View("OpMsgHistory");
        //}

        #endregion

        public ActionResult Alarms(int PlantID = 0, int ShopID = 0, int CellID = 0, int WorkCenterID = 0)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];

            TempData["PlantID"] = PlantID;
            TempData["ShopID"] = ShopID;
            TempData["CellID"] = CellID;
            TempData["WorkCenterID"] = WorkCenterID;

            string IpAddress = db.tblmachinedetails.Where(m => m.MachineID == WorkCenterID && m.IsDeleted == 0).Select(m => m.IPAddress).FirstOrDefault();
            string MacInvNo = db.tblmachinedetails.Where(m => m.MachineID == WorkCenterID && m.IsDeleted == 0).Select(m => m.MachineDisplayName).FirstOrDefault();
            int MacType = Convert.ToInt32(db.tblmachinedetails.Where(m => m.MachineID == WorkCenterID && m.IsDeleted == 0).Select(m => m.MacType).FirstOrDefault());

           AlarmHMI AHMI = new AlarmHMI(IpAddress, MacInvNo, MacType);
            List<AlarmDetails> AlarmList = new List<AlarmDetails>();
            if (MacType == 1)
            {
                AHMI.formatinsertAlmHis(out AlarmList);
            }
            else if (MacType == 2)
            {
                AHMI.formatinsertAlmHis2(out AlarmList);
            }

            ViewData["PlantID"] = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", PlantID);
            ViewData["ShopID"] = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID), "ShopID", "ShopName", ShopID);
            ViewData["CellID"] = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID), "CellID", "CellName", CellID);
            ViewData["WorkCenterID"] = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID && m.CellID == CellID && m.IsNormalWC == 0), "MachineID", "MachineDisplayName", WorkCenterID);

            return View(AlarmList);
        }

        public ActionResult OpMsgHistory(int PlantID = 0, int ShopID = 0, int CellID = 0, int WorkCenterID = 0)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];

            TempData["PlantID"] = PlantID;
            TempData["ShopID"] = ShopID;
            TempData["CellID"] = CellID;
            TempData["WorkCenterID"] = WorkCenterID;

            string IpAddress = db.tblmachinedetails.Where(m => m.MachineID == WorkCenterID && m.IsDeleted == 0).Select(m => m.IPAddress).FirstOrDefault();
            string MacInvNo = db.tblmachinedetails.Where(m => m.MachineID == WorkCenterID && m.IsDeleted == 0).Select(m => m.MachineDisplayName).FirstOrDefault();
            int MacType = Convert.ToInt32(db.tblmachinedetails.Where(m => m.MachineID == WorkCenterID && m.IsDeleted == 0).Select(m => m.MacType).FirstOrDefault());

           OpMsgHMI AHMI = new OpMsgHMI(IpAddress, MacInvNo, MacType);
            List<OpMsgHisDetails> OpMsgList = new List<OpMsgHisDetails>();
            if (MacType == 1)
            {
                AHMI.formatinsertMsgHis(out OpMsgList);
            }
            else if (MacType == 2)
            {
                AHMI.formatinsertMsgHis2(out OpMsgList);
            }


            ViewData["PlantID"] = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", PlantID);
            ViewData["ShopID"] = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID), "ShopID", "ShopName", ShopID);
            ViewData["CellID"] = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID), "CellID", "CellName", CellID);
            ViewData["WorkCenterID"] = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID && m.CellID == CellID && m.IsNormalWC == 0), "MachineID", "MachineDisplayName", WorkCenterID);

            return View(OpMsgList);
        }

    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class MultipleButtonAttribute : ActionNameSelectorAttribute
    {
        public string Name { get; set; }
        public string Argument { get; set; }

        public override bool IsValidName(ControllerContext controllerContext, string actionName, MethodInfo methodInfo)
        {
            var isValidName = false;
            var keyValue = string.Format("{0}:{1}", Name, Argument);
            var value = controllerContext.Controller.ValueProvider.GetValue(keyValue);

            if (value != null)
            {
                controllerContext.Controller.ControllerContext.RouteData.Values[Name] = Argument;
                isValidName = true;
            }

            return isValidName;
        }

    }

    //public class VirtualHMI
    //{
    //    public double AbsX;
    //    public double AbsY;
    //    public double RelX;
    //    public double RelY;
    //    public double MacX;
    //    public double MacY;
    //    public double DistX;
    //    public double DistY;

    //    public double FRActual;//FeedRate
    //    public double SActual;//Spindle

    //    public double OpTime;//Operating Time
    //    public double CuttingTime;
    //    public double PONTime;

    //}

}
