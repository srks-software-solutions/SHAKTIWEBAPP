using DNC;
using i_facilitylibrary;
using i_facilitylibrary.DAL;
using i_facilitylibrary.DAO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
//using i_facility.Models;
using System.Data.SqlClient;
//using i_facility;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;

namespace SRKSDemo.Controllers
{
    public class HMIScreeController : Controller
    {

        //PreviousOperationCancel objprev = new PreviousOperationCancel();
        private IConnectionFactory _conn;
        private Dao obj = new Dao();
        //GET: HMIScree

        public ActionResult SelectMachine()
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            int opid = Convert.ToInt32(Session["UserId"]);
            //ViewBag.RHID = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0), "MachineID", "MachineInvNo");

            List<tblmachinedetail> machinelist = obj.GetMachineDetails2();
            ViewBag.RHID = new SelectList(machinelist, "MachineID", "MachineName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SelectMachine(tblreportholder tbl)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.roleid = Session["RoleID"];

            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            string CorrectedDate = null;
            //tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
            tbldaytiming StartTime = obj.GetDaytimingDetails();
            TimeSpan End = StartTime.EndTime; // this is Shift End Time Specified
            TimeSpan EndTimeSpan = new TimeSpan(0, 0, 0); // 00:00:00 Normal day end time.
            TimeSpan TimeSpanNow = DateTime.Now.TimeOfDay;
            if (TimeSpanNow >= EndTimeSpan && TimeSpanNow <= End)
            {
                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }
            else
            {
                CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            }

            int MachineID = Convert.ToInt32(tbl.RHID);
            //Gatting UderID
            //var User = db.tblusers.Where(m => m.MachineID == MachineID && m.IsDeleted == 0).FirstOrDefault();
            tbluser User = obj.GetUserDetails(MachineID);
            int opid = 0;
            if (User != null)
            {
                opid = User.UserID;
            }
            //tbllivehmiscreen HMI = db.tbllivehmiscreens.Where(m => m.CorrectedDate == CorrectedDate && m.OperatiorID == opid).Where(m => m.MachineID == MachineID).FirstOrDefault();
            tbllivehmiscreen HMI = obj.GetLiveHMIDetails3(CorrectedDate, opid, MachineID);
            if (HMI == null)
            {

                //tbllivehmiscreen tblhmiscreen = new tbllivehmiscreen();
                //tblhmiscreen.MachineID = MachineID;
                //tblhmiscreen.CorrectedDate = CorrectedDate;
                ////tblhmiscreen.PEStartTime = DateTime.Now;
                ////tblhmiscreen.Date = DateTime.Now;
                ////tblhmiscreen.Time = DateTime.Now;
                //tblhmiscreen.Shift = Convert.ToString(Session["shift"]);
                //tblhmiscreen.Status = 0;
                //tblhmiscreen.OperatiorID = opid;
                //tblhmiscreen.isWorkInProgress = 2;
                ////tblhmiscreen.HMIID = (HMMID.HMIID + 1); // by Ashok
                //db.tbllivehmiscreens.Add(tblhmiscreen);
                //db.SaveChanges();

                obj.InsertLiveHMIDetails1(MachineID, CorrectedDate, Convert.ToString(Session["shift"]), 0, opid, 2);


                //tblhmiscreen tblhmiscreenSecondRow = new tblhmiscreen();
                //tblhmiscreenSecondRow.MachineID = MachineID;
                //tblhmiscreenSecondRow.CorrectedDate = CorrectedDate;
                //tblhmiscreenSecondRow.Date = DateTime.Now.Date;
                //tblhmiscreenSecondRow.Shift = Convert.ToString(Session["shift"]);
                //tblhmiscreenSecondRow.Status = 1;
                //tblhmiscreenSecondRow.OperatiorID = opid;
                //tblhmiscreenSecondRow.isWorkInProgress = 2;
                //tblhmiscreenSecondRow.Time = DateTime.Now.TimeOfDay;
                //db.tblhmiscreens.Add(tblhmiscreenSecondRow);
                //db.SaveChanges();
            }
            //HMIScreenForAdmin(MachineID, opid, CorrectedDate);
            Session["opid"] = opid;
            if (opid == 0)
            {
                Session["Error"] = "This machine is not Allocated to any Operator";
                return RedirectToAction("SelectMachine");
            }
            return RedirectToAction("HMIScreenForAdmin", "HMIScree", new { MachineID, opid, CorrectedDate });
            //return View(db.tblhmiscreens.Where(m => m.MachineID == MachineID && m.OperatiorID == opid).Where(m => m.Status != 2).Where(m => m.CorrectedDate == CorrectedDate).ToList());
        }

        //public ActionResult HMIScreenForAdmin(int MachineID, int opid, string CorrectedDate)
        //{
        //    if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
        //    {
        //        return RedirectToAction("Login", "Login", null);
        //    }
        //    TempData["saveORUpdate"] = null;
        //    //Getting Shift Value
        //    _conn = new ConnectionFactory();
        //    obj = new Dao(_conn);

        //    DateTime Time = DateTime.Now;
        //    TimeSpan Tm = new TimeSpan(Time.Hour, Time.Minute, Time.Second);
        //    //var ShiftDetails = db.tblshift_mstr.Where(m => m.StartTime <= Tm && m.EndTime >= Tm);
        //    var ShiftDetails = obj.GetShiftDetails(Tm);
        //    string Shift = null;
        //    if (ShiftDetails != null)
        //    {
        //        Shift = ShiftDetails.ShiftName;
        //    }
        //    ViewBag.date = System.DateTime.Now;
        //    if (Shift != null)
        //        ViewBag.shift = Shift;
        //    else
        //        ViewBag.shift = "C";
        //    Shift = "C";

        //    //bool tick = checkingIdle();
        //    bool tick = true;
        //    //if (tick == true)
        //    //{
        //    //    return RedirectToAction("DownCodeEntry");
        //    //}

        //    int handleidleReturnValue = HandleIdle();
        //    if (handleidleReturnValue == 0)
        //    {
        //        return RedirectToAction("DownCodeEntry");
        //    }

        //    //tbluser tbl = db.tblusers.Find(opid);
        //    tbluser tbl = obj.GetUserDetails1(opid);
        //    ViewBag.operatordisplay = tbl.DisplayName;
        //    ViewBag.machineID = Convert.ToInt32(MachineID);
        //    ViewBag.Logout = Session["Username"];
        //    ViewBag.roleid = Session["RoleID"];
        //    Session["MchnID"] = MachineID;
        //    Session["Opid"] = opid;
        //    Session["realshift"] = Shift;
        //    string gshift = null;
        //    if (Session["gshift" + opid] != null)
        //        gshift = Session["gshift" + opid].ToString();

        //    #region old code
        //    //var data = db.tblhmiscreens.Where(m => m.MachineID == MachineID && m.OperatiorID == opid).Where(m => m.CorrectedDate == CorrectedDate).Where(m => m.Shift == gshift).ToList();
        //    //if (data.Count() != 0)
        //    //{
        //    //    ViewBag.shift = gshift;
        //    //    Session["realshift"] = gshift;
        //    //    if (Session["Show"] == null)
        //    //    {
        //    //        ViewBag.hide = 1;
        //    //    }
        //    //    else
        //    //    {
        //    //        ViewBag.hide = null;
        //    //    }
        //    //    tick = false;
        //    //    var data1 = db.tblhmiscreens.Where(m => m.MachineID == MachineID && m.OperatiorID == opid).Where(m => m.CorrectedDate == CorrectedDate).Where(m => m.Shift == gshift).OrderByDescending(u => u.HMIID).Take(1).ToList();
        //    //    foreach (var a in data1)
        //    //    {
        //    //        //ViewBag.shift = a.Shift;
        //    //        if (a.isUpdate == 1)
        //    //            tick = true;
        //    //    }
        //    //    if (tick)
        //    //    {
        //    //        TempData["saveORUpdate"] = 1;
        //    //    }
        //    //    return View(data1);
        //    //}
        //    //else
        //    //{
        //    //    tick = false;
        //    //    var data2 = db.tblhmiscreens.Where(m => m.MachineID == MachineID && m.OperatiorID == opid).Where(m => m.Status != 2).Where(m => m.CorrectedDate == CorrectedDate).OrderByDescending(u => u.HMIID).Take(2).ToList();
        //    //    foreach (var a in data2)
        //    //    {
        //    //        //ViewBag.shift = a.Shift;
        //    //        if (a.isUpdate == 1)
        //    //            tick = true;
        //    //    }
        //    //    if (tick)
        //    //    {
        //    //        TempData["saveORUpdate"] = 1;
        //    //    }
        //    //    return View(data2);
        //    //}

        //    #endregion

        //    //var resumeWorkOrder = db.tbllivehmiscreens.Where(m => m.MachineID == MachineID && m.OperatiorID == opid).Where(m => m.CorrectedDate == CorrectedDate).OrderByDescending(m => m.HMIID).Take(1).ToList();
        //    var resumeWorkOrder = obj.GetLiveHMIScreenDetails(CorrectedDate, MachineID, opid);
        //    if (resumeWorkOrder != null)
        //    {
        //        ViewBag.hide = 1;
        //    }
        //    ViewBag.ProdFAI = resumeWorkOrder[0].Prod_FAI;
        //    return View(resumeWorkOrder);

        //}

        public ActionResult HMIScreenForAdmin(int MachineID, int opid, String CorrectedDate)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            TempData["saveORUpdate"] = null;
            //Getting Shift Value
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            DateTime Time = DateTime.Now;
            TimeSpan Tm = new TimeSpan(Time.Hour, Time.Minute, Time.Second);
            //var ShiftDetails = db.tblshift_mstr.Where(m => m.StartTime <= Tm && m.EndTime >= Tm);
            tblshift_mstr ShiftDetails = obj.GetShiftDetails(Tm);
            string Shift = null;
            if (ShiftDetails != null)
            {
                Shift = ShiftDetails.ShiftName;
            }
            ViewBag.date = System.DateTime.Now;
            if (Shift != null)
            {
                ViewBag.shift = Shift;
            }
            else
            {
                ViewBag.shift = "C";
            }

            Shift = "C";
            //if (tick == true)
            //{
            //    return RedirectToAction("DownCodeEntry");
            //}

            int handleidleReturnValue = HandleIdle();
            if (handleidleReturnValue == 0)
            {
                return RedirectToAction("DownCodeEntry");
            }

            ViewBag.machineID = Convert.ToInt32(Session["MachineID"]);
            int macid = Convert.ToInt32(Session["MachineID"]);
            //ViewBag.MacDispName = db.tblmachinedetails.Where(m => m.MachineID == macid).Select(m => m.MachineDispName).FirstOrDefault();
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);
            string test = obj.GetMachineDet1(macid);
            ViewBag.MacDispName = test;
            //tbluser tbl = db.tblusers.Find(opid);
            tbluser tbl = obj.GetUserDetails1(opid);
            ViewBag.operatordisplay = tbl.DisplayName;
            ViewBag.machineID = Convert.ToInt32(MachineID);
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            Session["MchnID"] = MachineID;
            Session["Opid"] = opid;
            Session["realshift"] = Shift;
            string gshift = null;
            if (Session["gshift" + opid] != null)
            {
                gshift = Session["gshift" + opid].ToString();
            }

            #region old code
            //var data = db.tblhmiscreens.Where(m => m.MachineID == MachineID && m.OperatiorID == opid).Where(m => m.CorrectedDate == CorrectedDate).Where(m => m.Shift == gshift).ToList();
            //if (data.Count() != 0)
            //{
            //    ViewBag.shift = gshift;
            //    Session["realshift"] = gshift;
            //    if (Session["Show"] == null)
            //    {
            //        ViewBag.hide = 1;
            //    }
            //    else
            //    {
            //        ViewBag.hide = null;
            //    }
            //    tick = false;
            //    var data1 = db.tblhmiscreens.Where(m => m.MachineID == MachineID && m.OperatiorID == opid).Where(m => m.CorrectedDate == CorrectedDate).Where(m => m.Shift == gshift).OrderByDescending(u => u.HMIID).Take(1).ToList();
            //    foreach (var a in data1)
            //    {
            //        //ViewBag.shift = a.Shift;
            //        if (a.isUpdate == 1)
            //            tick = true;
            //    }
            //    if (tick)
            //    {
            //        TempData["saveORUpdate"] = 1;
            //    }
            //    return View(data1);
            //}
            //else
            //{
            //    tick = false;
            //    var data2 = db.tblhmiscreens.Where(m => m.MachineID == MachineID && m.OperatiorID == opid).Where(m => m.Status != 2).Where(m => m.CorrectedDate == CorrectedDate).OrderByDescending(u => u.HMIID).Take(2).ToList();
            //    foreach (var a in data2)
            //    {
            //        //ViewBag.shift = a.Shift;
            //        if (a.isUpdate == 1)
            //            tick = true;
            //    }
            //    if (tick)
            //    {
            //        TempData["saveORUpdate"] = 1;
            //    }
            //    return View(data2);
            //}

            #endregion

            //var resumeWorkOrder = db.tbllivehmiscreens.Where(m => m.MachineID == MachineID && m.OperatiorID == opid).Where(m => m.CorrectedDate == CorrectedDate).OrderByDescending(m => m.HMIID).Take(1).ToList();
            List<tbllivehmiscreen> resumeWorkOrder = obj.GetLiveHMIScreenDetails(CorrectedDate, MachineID, opid);
            if (resumeWorkOrder != null)
            {
                ViewBag.hide = 1;
            }

            ViewBag.ProdFAI = resumeWorkOrder[0].Prod_FAI;
            return View(resumeWorkOrder);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //public ActionResult UpdateData(IEnumerable<tbllivehmiscreen> tbldaily_plan, int Line1 = 0)
        public ActionResult HMIScreenForAdmin(IEnumerable<tbllivehmiscreen> tbldaily_plan, int Line1 = 0, String Shift = null, string hiddentextbox = null)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            string CorrectedDate = null;
            //tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
            tbldaytiming StartTime = obj.GetDaytimingDetails();
            TimeSpan Start = StartTime.StartTime;
            if (Start <= DateTime.Now.TimeOfDay)
            {
                CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }

            DateTime presentdate = System.DateTime.Now.Date;
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            int UserID = Convert.ToInt32(Session["UserID"].ToString());
            ViewBag.DPIsMenu = 0;
            int opid = 0;
            int MachineID = 0;
            if (tbldaily_plan != null)
            {
                //if (ModelState.IsValid)
                {
                    int count = 0;
                    foreach (tbllivehmiscreen plan in tbldaily_plan)
                    {



                        if (plan.Project != null || plan.Prod_FAI != null || plan.PartNo != null || plan.Work_Order_No != null || plan.OperationNo != null || plan.Work_Order_No != null || plan.Target_Qty.HasValue || plan.Rej_Qty.HasValue || plan.Delivered_Qty.HasValue)
                        {
                            plan.isUpdate = 1;
                        }
                        else
                        {
                            plan.isUpdate = 0;
                        }

                        opid = plan.OperatiorID;
                        MachineID = plan.MachineID;


                        if (count == 1)
                        {
                            //when Record WIP is clicked. => work is in progress.
                            plan.isWorkInProgress = 1;

                        }
                        plan.Time = DateTime.Now;
                        //db.Entry(plan).State = System.Data.Entity.EntityState.Modified;
                        //db.SaveChanges();
                        obj.UpdateLiveHMIScreenDetails9(plan.HMIID, plan.isUpdate, plan.isWorkInProgress, Convert.ToDateTime(plan.Time));
                        count++;
                    }
                    return RedirectToAction("HMIScreenForAdmin", "HMIScree", new { MachineID, opid, CorrectedDate });
                }
            }
            return View();
        }

        // host Name: 10.30.10.57
        //port = 14

        public ActionResult Index(int id = 0, string selectedValues = "")
        {
            //Session["Mode"] = null;
            Session["Route"] = 0;
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            object alsdkfj = Session["isWorkOrder"];

            ViewBag.selectedValue = selectedValues;

            //Session["FromDDL"] = 0;
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            int opid = Convert.ToInt32(Session["UserId"]);
            int ShiftID = 0;

            ViewBag.item = Session["item"];

            if (ViewBag.item == null || ViewBag.item.Count == 0)
            {
                Session["empty"] = 1;
            }

            ViewBag.RedStart = Session["redStrat"];
            ViewBag.RedEnd = Session["redEnd"];

            if (ViewBag.RedStart == 1)
            {
                Session["BreakdownredStart"] = 1;
            }
            else if (ViewBag.RedEnd == 1)
            {
                Session["BreakdownredEnd"] = 1;
            }

            //ViewBag.sele = count;
            //Testing Login Status
            bool aj = User.Identity.IsAuthenticated;
            bool jb = Request.IsAuthenticated;
            bool val1 = System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            string c = User.Identity.Name;
            string d = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            System.Security.Principal.IPrincipal e = System.Web.HttpContext.Current.User;

            TempData["saveORUpdate"] = null;

            string breakdown = GetBreakdownstatus();
            Session["Breakdown_UAT"] = breakdown;
            //Getting Shift Value
            #region
            DateTime Time = DateTime.Now;
            TimeSpan Tm = new TimeSpan(Time.Hour, Time.Minute, Time.Second);
            //var ShiftDetails = db.tblshift_mstr.Where(m => m.StartTime <= Tm && m.EndTime >= Tm);
            tblshift_mstr ShiftDetails = obj.GetShiftDetails(Tm);
            string Shift = null;
            if (ShiftDetails != null)
            {
                Shift = ShiftDetails.ShiftName;
            }
            ViewBag.date = System.DateTime.Now;
            if (Shift != null)
            {
                ViewBag.shift = Shift;
                Session["shift"] = Shift;
            }
            else
            {
                ViewBag.shift = "C";
                Session["shift"] = "C";
                Shift = "C";
            }
            Session["realshift"] = Shift;
            if (Shift == "A")
            {
                ShiftID = 1;
            }
            else if (Shift == "B")
            {
                ShiftID = 2;
            }
            else
            {
                ShiftID = 3;
            }
            #endregion

            //Code For Admin And Super Admin
            int RoleID = Convert.ToInt32(Session["RoleID"]);
            if (RoleID == 1 || RoleID == 2)
            {
                return RedirectToAction("SelectMachine", "HMIScree", null);
            }
            ViewBag.roleid = Session["RoleID"];

            //code to get CorrectedDate
            #region
            string CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            if (DateTime.Now.Hour < 6 && DateTime.Now.Hour >= 0)
            {
                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }

            #endregion

            int Machinid = Convert.ToInt32(Session["MachineID"]);

            tblmachinedetail machinedet = obj.GetMachineDetails(Machinid);

            //if (machinedet.ShopID == 6 && machinedet.IsNormalWC == 0)
            //{
            //    Session["Route"] = 1;
            //}

            //if (machinedet.IsNestedSheetMachine == 1)
            //{
            //    Session["Route"] = 1;
            //}

            //Checking operator machine is allocated or not
            //var machineallocation = db.tblmachineallocations.Where(m => m.IsDeleted == 0 && m.CorrectedDate == CorrectedDate && m.UserID == opid && m.ShiftID == ShiftID);
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);
            List<tblmachineallocation> machineallocation = obj.GetMachineAllocationDetails(CorrectedDate, opid, ShiftID);
            if (machineallocation.Count() != 0)
            {
                foreach (tblmachineallocation a in machineallocation)
                {
                    Machinid = Convert.ToInt32(a.MachineID);
                    Session["MachineID"] = Machinid;
                }
            }

            if (Session["Mode"] != null)
            {
                Session["ErM"] = "Presently the Machine is Running You can not select Breakdown";
            }
            //insert a new row if there is no row for this machine for this date.
            // tblhmiscreen HMI = db.tblhmiscreens.Where(m => m.CorrectedDate == CorrectedDate && m.OperatiorID == opid).Where(m => m.MachineID == Machinid && m.Status == 0).FirstOrDefault();
            //tblhmiscreen HMI = db.tblhmiscreens.Where(m => m.MachineID == Machinid && m.Status == 0).OrderByDescending(m => m.HMIID).FirstOrDefault();

            //var HMI = db.tbllivehmiscreens.Where(m => m.MachineID == Machinid && m.Status == 0).OrderByDescending(m => m.HMIID).ToList();
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);
            List<tbllivehmiscreen> HMI = obj.GetLiveHMIDetails4(Machinid);
            if (HMI.Count == 0)
            {
                //var HMIOpName = db.tbllivehmiscreens.Where(m => m.MachineID == Machinid && m.Status == 0 && m.isUpdate == 1).OrderByDescending(m => m.HMIID).FirstOrDefault();
                tbllivehmiscreen HMIOpName = obj.GetLiveHMIScreen1Details(Machinid);
                if (HMIOpName != null)
                {
                    // Session["OpName"] = HMIOpName.OperatorDet; //commented on 2020-07-08
                }

                tbllivehmiscreen tblhmiscreen = new tbllivehmiscreen();

                //tblhmiscreen.MachineID = Machinid;
                //tblhmiscreen.CorrectedDate = CorrectedDate;
                //tblhmiscreen.PEStartTime = DateTime.Now;
                //tblhmiscreen.OperatorDet = Convert.ToString(Session["OpName"]);
                //tblhmiscreen.Shift = Convert.ToString(ViewBag.shift);
                //tblhmiscreen.Status = 0;
                // tblhmiscreen.HMIID = ; //by Ashok
                int ishideInt = 0;
                int.TryParse(Convert.ToString(Session["isHide"]), out ishideInt);
                if (ishideInt == 1)
                {
                    tblhmiscreen.isUpdate = 1;
                }
                //tblhmiscreen.isWorkInProgress = 2;
                //tblhmiscreen.isWorkOrder = Convert.ToInt32(Session["isWorkOrder"]);
                //tblhmiscreen.OperatiorID = Convert.ToInt32(Session["UserId"]);

                //db.tbllivehmiscreens.Add(tblhmiscreen);
                //db.SaveChanges();
                _conn = new ConnectionFactory();
                obj = new Dao(_conn);
                obj.InsertLiveHMIScreenDetails3(Machinid, Convert.ToInt32(Session["UserId"]), Convert.ToString(ViewBag.shift), 0, CorrectedDate, 2, DateTime.Now, tblhmiscreen.isUpdate);

                Session["FromDDL"] = 0;
                Session["partialclick"] = 1;
            }
            else
            {
                //If there is no nonsubmited row then insert new
                //var HMIOpName = db.tbllivehmiscreens.Where(m => m.MachineID == Machinid && m.Status == 0 && m.isUpdate == 1).OrderByDescending(m => m.HMIID).FirstOrDefault();
                _conn = new ConnectionFactory();
                obj = new Dao(_conn);
                tbllivehmiscreen HMIOpName = obj.GetLiveHMIScreen1Details(Machinid);
                if (HMIOpName != null)
                {
                    Session["OpName"] = HMIOpName.OperatorDet;
                    //Shift will be stale one, dont take it from here.
                }

                bool isEmptyAvailable = false;
                //if (HMI.Count > 1)
                //{
                //    foreach (var row in HMI)
                //    {
                //        //if (row.isUpdate == 0 && row.Date == null)
                //        if (row.Date == null)
                //        {
                //            isEmptyAvailable = true;
                //            break;
                //        }
                //    }
                //}

                if (HMI.Count != 0)
                {
                    isEmptyAvailable = true;
                }

                if (!isEmptyAvailable)
                {
                    tbllivehmiscreen tblhmiscreen = new tbllivehmiscreen();

                    //tblhmiscreen.MachineID = Machinid;
                    //tblhmiscreen.CorrectedDate = CorrectedDate;
                    //tblhmiscreen.PEStartTime = DateTime.Now;
                    //tblhmiscreen.OperatorDet = Convert.ToString(Session["OpName"]);
                    //tblhmiscreen.Shift = Convert.ToString(ViewBag.shift);
                    //tblhmiscreen.Status = 0;
                    // tblhmiscreen.HMIID = (HMMID.HMIID+1);
                    int ishideInt = 0;
                    int.TryParse(Convert.ToString(Session["isHide"]), out ishideInt);
                    if (ishideInt == 1)
                    {
                        tblhmiscreen.isUpdate = 1;
                    }

                    //tblhmiscreen.isWorkInProgress = 2;
                    //tblhmiscreen.isWorkOrder = Convert.ToInt32(Session["isWorkOrder"]);
                    //tblhmiscreen.OperatiorID = Convert.ToInt32(Session["UserId"]);
                    _conn = new ConnectionFactory();
                    obj = new Dao(_conn);
                    obj.InsertLiveHMIScreenDetails2(Machinid, Convert.ToInt32(Session["UserId"]), Convert.ToString(ViewBag.shift), 0, CorrectedDate, 2, Convert.ToString(Session["OpName"]), DateTime.Now, tblhmiscreen.isUpdate);
                    //db.tbllivehmiscreens.Add(tblhmiscreen);
                    //db.SaveChanges();
                }

                //Below code may be required based on how we handle WO's 2016-12-28
                foreach (tbllivehmiscreen row in HMI)
                {
                    if (row.CorrectedDate != CorrectedDate && row.Date == null)
                    {
                        //row.Shift = Convert.ToString(ViewBag.shift);
                        //row.CorrectedDate = CorrectedDate;
                        //row.PEStartTime = DateTime.Now;
                        //db.Entry(row).State = System.Data.Entity.EntityState.Modified;
                        //db.SaveChanges();
                        _conn = new ConnectionFactory();
                        obj = new Dao(_conn);
                        obj.UpdateLiveHMIScreenDetails10(row.HMIID, Convert.ToString(ViewBag.shift), CorrectedDate, DateTime.Now);
                    }
                }
            }

            ViewBag.DATE = DateTime.Now.Date;
            List<SelectListItem> Prod_FAI = new List<SelectListItem>();

            //Prod_FAI.Add(new SelectListItem
            //{
            //    Text = "FAI",
            //    Value = "FAI",
            //});
            //Prod_FAI.Add(new SelectListItem
            //{
            //    Text = "PRR",
            //    Value = "PRR"
            //});
            //Prod_FAI.Add(new SelectListItem
            //{
            //    Text = "Prod",
            //    Value = "Prod",
            //});
            //Prod_FAI.Add(new SelectListItem
            //{
            //    Text = "Trial",
            //    Value = "Trial"
            //});
            //Prod_FAI.Add(new SelectListItem
            //{
            //    Text = "RM Sizeing",
            //    Value = "RM Sizeing",
            //});

            Prod_FAI.Add(new SelectListItem
            {
                Text = "Production",
                Value = "Production",
            });
            Prod_FAI.Add(new SelectListItem
            {
                Text = "FAI",
                Value = "FAI",
            });
            Prod_FAI.Add(new SelectListItem
            {
                Text = "DeltaFAI",
                Value = "DeltaFAI",
            });
            Prod_FAI.Add(new SelectListItem
            {
                Text = "ProtoType",
                Value = "ProtoType",
            });
            Prod_FAI.Add(new SelectListItem
            {
                Text = "TASLRequirement",
                Value = "TASLRequirement",
            });
            Prod_FAI.Add(new SelectListItem
            {
                Text = "InWork",
                Value = "InWork",
            });
            Prod_FAI.Add(new SelectListItem
            {
                Text = "ReleasedLev1",
                Value = "ReleasedLev1",
            });
            Prod_FAI.Add(new SelectListItem
            {
                Text = "ProtoTypeLev1",
                Value = "ProtoTypeLev1"
            });
            Prod_FAI.Add(new SelectListItem
            {
                Text = "Trial",
                Value = "Trial"
            });
            Prod_FAI.Add(new SelectListItem
            {
                Text = "RM Sizeing",
                Value = "RM Sizeing",
            });
            Prod_FAI.Add(new SelectListItem
            {
                Text = "PRR",
                Value = "PRR"
            });

            ViewBag.Prod_FAI = Prod_FAI;
            //this is needed ...... like (now) no never.
            //int handleidleReturnValue = HandleIdleManualWC();
            //if (handleidleReturnValue == 0)
            //{
            //    return RedirectToAction("DownCodeEntry");
            //}
            string ja = Convert.ToString(Session["showIdlePopUp"]);

            int handleidleReturnValue = HandleIdle();
            if (handleidleReturnValue == 0)
            {
                return RedirectToAction("DownCodeEntry");
            }

            ViewBag.machineID = Convert.ToInt32(Session["MachineID"]);
            int macid = Convert.ToInt32(Session["MachineID"]);
            //ViewBag.MacDispName = db.tblmachinedetails.Where(m => m.MachineID == macid).Select(m => m.MachineDispName).FirstOrDefault();
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);
            string test = obj.GetMachineDet1(macid);
            ViewBag.MacDispName = test;
            //tbluser tbl = db.tblusers.Find(opid);
            tbluser tbl = obj.GetUserDetails1(opid);
            ViewBag.operatordisplay = tbl.DisplayName;

            if (Session["VError"] != null)
            {

            }
            if (TempData["Err"] != null)
            {

            }

            //Code to resume particular workOrder . Probabily ...... Never 
            if (id != 0)
            {
                _conn = new ConnectionFactory();
                obj = new Dao(_conn);
                //var resumeWorkOrder = db.tbllivehmiscreens.Where(m => m.MachineID == Machinid && m.OperatiorID == opid && (m.HMIID == id)).Where(m => m.Status != 2).Where(m => m.CorrectedDate == CorrectedDate).Take(1).ToList();
                List<tbllivehmiscreen> resumeWorkOrder = obj.GetLiveHMIDetails5(Machinid, opid, id, CorrectedDate);
                //ViewBag.shiftshivu = new SelectList(db.tblhmiscreens.Where(m => m.MachineID == Machinid && m.OperatiorID == opid && (m.HMIID == id)).Where(m => m.Status != 2).Where(m => m.CorrectedDate == CorrectedDate), "Shift", "Shift");
                #region commmented
                //if (resumeWorkOrder.Count == 1)
                //{
                //    int extrarowid = 0;
                //    var resumeworkExtraRow = db.tblhmiscreens.Where(m => m.MachineID == Machinid && m.OperatiorID == opid).OrderByDescending(m => m.HMIID).FirstOrDefault();
                //    if (resumeworkExtraRow != null)
                //    {
                //        extrarowid = resumeworkExtraRow.HMIID;
                //    }
                //    resumeWorkOrder = db.tblhmiscreens.Where(m => m.MachineID == Machinid && m.OperatiorID == opid && (m.HMIID == id || m.HMIID == extrarowid)).Where(m => m.CorrectedDate == CorrectedDate).ToList();
                //}
                //else
                //{
                //    int extrarowid = 0;
                //    var resumeworkExtraRow = db.tblhmiscreens.Where(m => m.MachineID == Machinid && m.OperatiorID == opid).OrderByDescending(m => m.HMIID).FirstOrDefault();
                //    if (resumeworkExtraRow != null)
                //    {
                //        extrarowid = resumeworkExtraRow.HMIID;
                //    }
                //    resumeWorkOrder = db.tblhmiscreens.Where(m => m.MachineID == Machinid && m.OperatiorID == opid && (m.HMIID == id || m.HMIID == extrarowid)).Where(m => m.CorrectedDate == CorrectedDate).ToList();
                //}
                //if (resumeWorkOrder[0].Status == resumeWorkOrder[1].Status)
                //{
                //    resumeWorkOrder[0].Status = 0;

                //}
                #endregion

                if (resumeWorkOrder.Count == 0)
                {
                    _conn = new ConnectionFactory();
                    obj = new Dao(_conn);
                    //resumeWorkOrder = db.tbllivehmiscreens.Where(m => m.MachineID == Machinid && m.OperatiorID == opid && (m.HMIID == id)).Where(m => m.CorrectedDate == CorrectedDate).ToList();
                    resumeWorkOrder = obj.GetLHMIDetails(Machinid, opid, id, CorrectedDate);
                    //ViewBag.shiftshivu = new SelectList(db.tblhmiscreens.Where(m => m.MachineID == Machinid && m.OperatiorID == opid && (m.HMIID == id)).Where(m => m.CorrectedDate == CorrectedDate), "Shift", "Shift");
                }
                ViewBag.ProdFAI = resumeWorkOrder[0].Prod_FAI;
                dynamic j = ViewBag.shiftshivu;
                return View(resumeWorkOrder);
            }

            #region . if data not selected.

            //var data = db.tblhmiscreens.Where(m => m.MachineID == Machinid && m.OperatiorID == opid).Where(m => m.Status != 2).Where(m => m.CorrectedDate == CorrectedDate).Where(m => m.Shift == gshift).ToList();

            //I am removing the CorrectedDate from condition so 2016-12-30 becarefull about what it retrieves(stale data)
            //var data = db.tbllivehmiscreens.Where(m => m.MachineID == Machinid && m.OperatiorID == opid).Where(m => m.Status == 0).OrderByDescending(m => m.HMIID).ToList();
            List<tbllivehmiscreen> data = obj.GetListHMIDetails(Machinid, opid);
            if (data.Count > 0)
            {
                //Default to Handle Manual/ScanEntry
                if (Convert.ToInt32(TempData["ForDDL2"]) == 2)
                {
                    Session["FromDDL"] = 2;
                    Session["SubmitClicked"] = 0;
                }

                int fromDDLInt = 6; //i am fake initializing to 6 , because i haven't used 6 anywhere.
                string blah = Convert.ToString(Session["FromDDL"]);
                int.TryParse(Convert.ToString(Session["FromDDL"]), out fromDDLInt);

                if (!string.IsNullOrEmpty(blah.Trim())) // Implies Session is Alive. Continous
                {
                    #region
                    if (fromDDLInt == 4) //implies that its a MultiWO & From DDL
                    {
                        if (data[0].Date == null) //Before Submit
                        {
                            Session["FromDDL"] = 4;
                            Session["SubmitClicked"] = 0;
                        }
                        else //After Submit
                        {
                            Session["FromDDL"] = 4;
                            Session["SubmitClicked"] = 1;
                        }
                    }
                    else if (fromDDLInt == 1)
                    {
                        if (data[0].Date == null) //Before Submit
                        {
                            Session["FromDDL"] = 1;
                            Session["SubmitClicked"] = 0;
                        }

                        else //After Submit
                        {
                            Session["FromDDL"] = 1;
                            Session["SubmitClicked"] = 1;
                        }
                    }
                    else if (fromDDLInt == 2) // Manual/ScanEntry
                    {
                        if (data[0].Date == null) //Before Submit
                        {
                            Session["FromDDL"] = 2;
                            Session["SubmitClicked"] = 0;
                        }
                        else //After Submit
                        {
                            Session["FromDDL"] = 1;
                            Session["SubmitClicked"] = 1;
                        }
                    }
                    else if (fromDDLInt == 0)
                    {
                        if (data[0].Date == null) //Before Submit
                        {
                            Session["FromDDL"] = 0;
                            Session["SubmitClicked"] = 0;
                        }
                        else //After Submit
                        {
                            Session["FromDDL"] = 1;
                            Session["SubmitClicked"] = 1;
                        }
                    }
                    #endregion

                }
                else //After Auto Logout or Session out.
                {
                    #region
                    if (data[0].Date == null) //Before Submit
                    {
                        if (data[0].IsMultiWO == 1) //Its a MultiWO //need "Enter Delivered Qty button"
                        {
                            Session["FromDDL"] = 4;
                        }
                        else //Its a single WO
                        {
                            string P = data[0].Project;
                            string wo = data[0].Work_Order_No;
                            string pno = data[0].PartNo;
                            string opno = data[0].OperationNo;

                            if (data[0].DDLWokrCentre != null) //Its a single WO from DDL
                            {
                                Session["FromDDL"] = 1;
                                Session["SubmitClicked"] = 0;
                            }
                            else if ((!string.IsNullOrEmpty(data[0].Project)) || (!string.IsNullOrEmpty(data[0].PartNo)) || (!string.IsNullOrEmpty(data[0].OperationNo)) || (!string.IsNullOrEmpty(data[0].Work_Order_No)))
                            {
                                Session["FromDDL"] = 2;
                            }
                            else
                            {
                                Session["FromDDL"] = 0;

                            }
                        }
                    }
                    else//After Submit
                    {
                        if (data[0].IsMultiWO == 1) //Its a MultiWO //need "Enter Delivered Qty button"
                        {
                            Session["FromDDL"] = 4;
                            Session["SubmitClicked"] = 1;
                        }
                        else //Its a single WO
                        {
                            Session["FromDDL"] = 1;
                            Session["SubmitClicked"] = 1;
                        }
                    }
                    #endregion

                }
                ViewBag.shift = data[0].Shift;
                Session["realshift"] = data[0].Shift;

                bool isHide = false; //make it true when one of the rows as isupdate = 1
                foreach (tbllivehmiscreen dat in data)
                {
                    if (dat.isUpdate == 1)
                    {
                        isHide = true;
                        Session["isHide"] = 1;
                        break;
                    }
                }
                if (isHide)
                {
                    ViewBag.hide = 1;
                }
                else
                {
                    ViewBag.hide = null;
                }

                string shifthere = data[0].Shift;
                //I am removing the CorrectedDate from condition so 2016-12-30 becarefull about what it retrieves(stale data)
                //var data2 = db.tblhmiscreens.Where(m => m.MachineID == Machinid && m.OperatiorID == opid).Where(m => m.Status == 0).OrderBy(u => u.Work_Order_No).ThenBy(u=>u.PartNo).ThenBy(u=>u.OperationNo).ToList();
                //var data2 = db.tbllivehmiscreens.Where(m => m.MachineID == Machinid && m.OperatiorID == opid).Where(m => m.Status == 0).OrderBy(m => m.Date).ToList();
                _conn = new ConnectionFactory();
                obj = new Dao(_conn);
                List<tbllivehmiscreen> data2 = obj.GetList1HMIDetails(Machinid, opid);
                if (data2.Count != 0)
                {
                    ViewBag.ProdFAI = data2[0].Prod_FAI;
                }
                else
                {

                }

                //See if Previously they were doing ReworkOrder
                if (data2.Count == 1)
                {
                    int a1 = 0;
                    if (int.TryParse(Convert.ToString(Session["WorkOrderClicked"]), out a1))
                    {
                        if (a1 == 1) //1st time After reworkorder clicked
                        {
                            int a = 0;
                            if (int.TryParse(Convert.ToString(Session["isWorkOrder"]), out a))
                            {
                                if (a == 1)
                                {
                                    Session["isWorkOrder"] = 1;
                                }
                            }
                            else
                            {
                                Session["isWorkOrder"] = 0;
                            }
                        }
                        else
                        {
                            Session["isWorkOrder"] = 0;
                        }
                    }
                    else //When You login.
                    {
                        Session["isWorkOrder"] = 0;
                    }
                }
                else
                {
                    Session["isWorkOrder"] = 0;
                    foreach (tbllivehmiscreen row in data2)
                    {
                        if (row.isWorkOrder == 1)
                        {
                            Session["isWorkOrder"] = 1;
                            break;
                        }
                    }
                }

                //Loop to get WO's Details Counts
                ViewBag.TotalWOCount = data2.Where(m => m.PartNo != null && m.OperationNo != null && m.Work_Order_No != null).Count();
                ViewBag.WOStartedCount = data2.Where(m => m.Date != null && m.isWorkInProgress != 3).Count();
                //ViewBag.WOOnHoldCount = data2.Where(m => m.IsHold == 1).Count();
                ViewBag.WONotStartedCount = data2.Where(m => m.Date == null && (m.PartNo != null || m.OperationNo != null || m.Work_Order_No != null)).Count();

                return View(data2);
            }
            else
            {
                bool tick = false;
                _conn = new ConnectionFactory();
                obj = new Dao(_conn);
                //var data1 = db.tblhmiscreens.Where(m => m.MachineID == Machinid && m.Status == 0).OrderBy(u => u.Work_Order_No).ThenBy(u=>u.PartNo).ThenBy(u=>u.OperationNo).ToList();
                //var data1 = db.tbllivehmiscreens.Where(m => m.MachineID == Machinid && m.Status == 0).OrderBy(m => m.Date).ToList();
                List<tbllivehmiscreen> data1 = obj.GetList2HMIDetails(Machinid);
                if (data1.Count != 0)
                {
                    foreach (tbllivehmiscreen a in data1)
                    {
                        if (a.isUpdate == 1)
                        { tick = true; }
                    }
                }
                else { }
                if (tick)
                {
                    TempData["saveORUpdate"] = 1;
                }

                bool isHide = false; //make it true when one of the rows as isupdate = 1
                //2017-01-19
                // foreach (var dat in data)
                foreach (tbllivehmiscreen dat in data1)
                {

                    if (dat.isUpdate == 1)
                    {
                        isHide = true;
                        break;
                    }
                }
                if (isHide)
                {
                    ViewBag.hide = 1;
                }
                else
                {
                    ViewBag.hide = null;
                }

                //Default to Handle Manual/ScanEntry
                if (Convert.ToInt32(TempData["ForDDL2"]) == 2)
                {
                    Session["FromDDL"] = 2;
                    Session["SubmitClicked"] = 0;
                }

                int fromDDLInt = 6; //i am fake initializing to 6 , because i haven't used 6 anywhere.
                string blah = Convert.ToString(Session["FromDDL"]);
                int.TryParse(Convert.ToString(Session["FromDDL"]), out fromDDLInt);

                if (!string.IsNullOrEmpty(blah.Trim())) // Implies Session is Alive. Continous
                {
                    #region
                    if (fromDDLInt == 4) //implies that its a MultiWO & From DDL
                    {
                        if (data1[0].Date == null) //Before Submit
                        {
                            Session["FromDDL"] = 4;
                            Session["SubmitClicked"] = 0;
                        }
                        else //After Submit
                        {
                            Session["FromDDL"] = 4;
                            Session["SubmitClicked"] = 1;
                        }
                    }
                    else if (fromDDLInt == 1)
                    {
                        if (data1[0].Date == null) //Before Submit
                        {
                            Session["FromDDL"] = 1;
                            Session["SubmitClicked"] = 0;
                        }
                        else //After Submit
                        {
                            Session["FromDDL"] = 1;
                            Session["SubmitClicked"] = 1;
                        }
                    }
                    else if (fromDDLInt == 2) // Manual/ScanEntry
                    {
                        if (data1[0].Date == null) //Before Submit
                        {
                            Session["FromDDL"] = 2;
                            Session["SubmitClicked"] = 0;
                        }
                        else //After Submit
                        {
                            Session["FromDDL"] = 0;
                            Session["SubmitClicked"] = 1;
                        }
                    }
                    else if (fromDDLInt == 0)
                    {
                        Session["FromDDL"] = 0;
                        Session["SubmitClicked"] = 0;
                    }
                    #endregion
                }
                else //After Auto Logout or Session out.
                {
                    #region
                    if (data1[0].Date == null) //Before Submit
                    {
                        if (data1[0].IsMultiWO == 1) //Its a MultiWO //need "Enter Delivered Qty button"
                        {
                            Session["FromDDL"] = 4;
                        }
                        else //Its a single WO
                        {
                            if (data1[0].DDLWokrCentre != null) //Its a single WO from DDL
                            {
                                Session["FromDDL"] = 1;
                                Session["SubmitClicked"] = 0;
                            }
                            else if ((!string.IsNullOrEmpty(data1[0].Project)) || (!string.IsNullOrEmpty(data1[0].Work_Order_No)) || (!string.IsNullOrEmpty(data1[0].OperationNo)) || (!string.IsNullOrEmpty(data1[0].PartNo)))//Its Manual Entry.
                            {
                                Session["FromDDL"] = 2;
                            }
                            else
                            {
                                Session["FromDDL"] = 0;
                            }
                        }
                    }
                    else//After Submit
                    {
                        if (data1[0].IsMultiWO == 1) //Its a MultiWO //need "Enter Delivered Qty button"
                        {
                            Session["FromDDL"] = 4;
                            Session["SubmitClicked"] = 1;
                        }
                        else //Its a single WO
                        {
                            //Its a single WO from DDL or Manual Entry(AfterSubmit) . They are same.
                            Session["FromDDL"] = 1;
                            Session["SubmitClicked"] = 1;
                        }
                    }
                    #endregion
                }
                if (data1.Count != 0)
                {
                    ViewBag.ProdFAI = data1[0].Prod_FAI;
                }
                if (data1.Count > 1)
                {
                    int b = 0;
                    if (int.TryParse(Convert.ToString(Session["isWorkOrder"]), out b))
                    {
                        if (b == 1)
                        {
                            Session["isWorkOrder"] = 1;
                        }
                    }
                    else
                    {
                        Session["isWorkOrder"] = 0;
                    }
                }
                else
                {
                    Session["isWorkOrder"] = 0;
                    foreach (tbllivehmiscreen row in data1)
                    {
                        if (row.isWorkOrder == 1)
                        {
                            Session["isWorkOrder"] = 1;
                            break;
                        }
                    }
                }

                //Loop to get WO's Details Counts
                ViewBag.TotalWOCount = data1.Where(m => m.PartNo != null && m.OperationNo != null && m.Work_Order_No != null).Count();
                ViewBag.WOStartedCount = data1.Where(m => m.Date != null && m.isWorkInProgress != 3).Count();
                //ViewBag.WOOnHoldCount = data1.Where(m => m.IsHold == 1).Count();
                ViewBag.WONotStartedCount = data1.Where(m => m.Date == null && (m.PartNo != null || m.OperationNo != null || m.Work_Order_No != null)).Count();

                return View(data1);
            }

            #endregion
        }

        //Control comes here when StartAll button is clicked.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public ActionResult UpdateData(IEnumerable<tblHMIScreen> tbldaily_plan, int Line1 = 0)
        public ActionResult Index(IList<tbllivehmiscreen> tbldaily_plan)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            //if (IDLEorGenericWorkisON())
            //{
            //    Session["Error"] = "Please End IDLE/GenericWork Before Selecting New Work Orders";
            //    return RedirectToAction("Index");
            //}

            DateTime presentdate = System.DateTime.Now.Date;
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            int UserID = Convert.ToInt32(Session["UserID"].ToString());
            int machineID = Convert.ToInt32(Session["MachineID"]);
            ViewBag.DPIsMenu = 0;
            if (tbldaily_plan != null)
            {
                int ExceptionHMIID = 0;
                foreach (tbllivehmiscreen plan in tbldaily_plan)
                {
                    //1) Check eligibility to Start (Even if 1 fails reject All)
                    if (plan.IsHold == 1 && plan.Date == null)
                    //if (plan.isWorkInProgress == 3)
                    {
                        Session["VError"] = "End HOLD Before StartAll is Clicked";
                        return RedirectToAction("Index");
                    }
                    else if (plan.OperatorDet != null && plan.Shift != null && plan.Project != null && plan.Prod_FAI != null && plan.PartNo != null && plan.Work_Order_No != null && plan.OperationNo != null && plan.Work_Order_No != null && plan.Target_Qty.HasValue)
                    {
                    }
                    else if (plan.Date == null && plan.Work_Order_No == null && plan.PartNo == null && plan.OperationNo == null)
                    {// Do Nothing. Just to Skip our Extra Empty Row
                        if (ExceptionHMIID == 0)
                        {
                            ExceptionHMIID = Convert.ToInt32(plan.HMIID);
                        }
                        else
                        {
                            Session["VError"] = "Please enter all Details Before StartAll is Clicked.";
                            return RedirectToAction("Index");
                        }
                    }
                    else
                    {
                        Session["VError"] = "Please enter all Details Before StartAll is Clicked.";
                        return RedirectToAction("Index");
                    }
                }

                //checking for duplicate WorkOrders
                #region
                foreach (tbllivehmiscreen plan in tbldaily_plan)
                {
                    int hmiid = Convert.ToInt32(plan.HMIID);
                    if (ExceptionHMIID != hmiid)
                    {
                        string woNo = Convert.ToString(plan.Work_Order_No);
                        string opNo = Convert.ToString(plan.OperationNo);
                        string partNo = Convert.ToString(plan.PartNo);

                        //var InHMIData = db.tbllivehmiscreens.Where(m => m.Work_Order_No == woNo && m.PartNo == partNo && m.OperationNo == opNo && m.Status == 0 && m.MachineID == machineID && m.HMIID != hmiid).FirstOrDefault();
                        tbllivehmiscreen InHMIData = obj.GetLiveHMI1Details(woNo, partNo, opNo, hmiid, machineID);

                        if (InHMIData != null)
                        {
                            Session["Error"] = "Duplicate WorkOrder:" + woNo + " OpNo: " + opNo + " PartNo:" + partNo;
                            //db.tblhmiscreens.Remove(plan);
                            //db.Entry(plan).State = System.Data.Entity.EntityState.Deleted;
                            //db.SaveChanges();
                            obj.DeleteHMIScreenDetails(plan.HMIID);
                            return RedirectToAction("Index");
                        }

                        //2017-06-22
                        //var HMICompletedData = db.tbllivehmiscreens.Where(m => m.Work_Order_No == woNo && m.PartNo == partNo && m.OperationNo == opNo && m.isWorkInProgress == 1).FirstOrDefault();
                        tbllivehmiscreen HMICompletedData = obj.GetLiveHMIDetails2(woNo, partNo, opNo);
                        if (HMICompletedData != null)
                        {
                            Session["Error"] = "Job is Finished for WorkOrder:" + woNo + " OpNo: " + opNo + " PartNo:" + partNo;
                            //var hmirow = db.tbllivehmiscreens.Find(hmiid);
                            //db.tbllivehmiscreens.Remove(hmirow);
                            //db.SaveChanges();
                            obj.DeleteHMIScreenDetails(hmiid);
                            return RedirectToAction("Index");
                        }

                    }
                }
                #endregion

                //Order Them Prior to checking them for " Sequence of Start "  Condition 2017-01-09
                tbldaily_plan = tbldaily_plan.OrderBy(m => m.Work_Order_No).ThenBy(m => m.PartNo).ThenBy(m => m.OperationNo).ToList();

                foreach (tbllivehmiscreen plan in tbldaily_plan)
                {
                    int hmiid = plan.HMIID;
                    if (hmiid != ExceptionHMIID)
                    {
                        //tbllivehmiscreen hmiidData = db.tbllivehmiscreens.Where(m => m.HMIID == hmiid).FirstOrDefault();

                        tbllivehmiscreen hmiidData = obj.GetLiveHMIDetails6(hmiid);
                        if (hmiidData != null)
                        {
                            string WONo = "";
                            string PNo = "";
                            int Opno = 0;
                            if (hmiidData.Work_Order_No != null)
                            {
                                WONo = Convert.ToString(hmiidData.Work_Order_No);
                                PNo = Convert.ToString(hmiidData.PartNo);
                                Opno = Convert.ToInt32(hmiidData.OperationNo);
                            }
                            //var HoldData = db.tblmanuallossofentries.Where(m => m.PartNo == PNo && m.OpNo == Opno && m.WONo == WONo && m.EndDateTime == null).OrderByDescending(m => m.MLossID).FirstOrDefault();
                            //if (HoldData != null)
                            //{
                            //    HoldData.EndDateTime = DateTime.Now;
                            //    db.Entry(HoldData).State = System.Data.Entity.EntityState.Modified;
                            //    db.SaveChanges();
                            //}
                            if (hmiidData.Date == null)
                            {
                                hmiidData.Date = DateTime.Now;
                            }
                            else
                            {
                                continue;
                            }

                            //Get Processed Qty
                            int newProcessedQty = 0;
                            string woNo = Convert.ToString(hmiidData.Work_Order_No);
                            string opNo = Convert.ToString(hmiidData.OperationNo);
                            string partNo = Convert.ToString(hmiidData.PartNo);
                            int OperationNoInt = Convert.ToInt32(opNo);

                            #region 2017-02-07

                            //Logic to check sequence of Submit Based on WONo, PartNo and OpNo.
                            //2017-01-21
                            //using (i_facility.Models.i_facility_tsalEntities dbHMI = new i_facility.Models.i_facility_tsalEntities())
                            //{
                            //string WIPQuery = @"SELECT * from tbllivehmiscreen where  HMIID IN ( SELECT HMIID from tbllivehmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + opNo + "' group by Work_Order_No,PartNo,OperationNo order by HMIID )";
                            //var WIP = dbHMI.tbllivehmiscreens.SqlQuery(WIPQuery).ToList();
                            List<tbllivehmiscreen> WIP = obj.GetList3HMIDetails(woNo, partNo, opNo);
                            //string WIPQueryHistorian = @"SELECT * from tblhmiscreen where  HMIID IN ( SELECT HMIID from tbllivehmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + opNo + "' group by Work_Order_No,PartNo,OperationNo order by HMIID )";
                            //var WIPHistorian = dbHMI.tblhmiscreens.SqlQuery(WIPQueryHistorian).ToList();
                            List<tblhmiscreen> WIPHistorian = obj.GetListH3HMIDetails(woNo, partNo, opNo);
                            foreach (tbllivehmiscreen row in WIP)
                            {
                                int InnerOpNo = Convert.ToInt32(row.OperationNo);
                                if (OperationNoInt > InnerOpNo)
                                {
                                    if (row.Date == null) //=> lower OpNo is not submitted.
                                    {
                                        Session["VError"] = " Submit WONo: " + row.Work_Order_No + " and PartNo: " + row.PartNo + " and OperationNo: " + InnerOpNo;
                                        return RedirectToAction("Index");
                                    }
                                }
                            }
                            if (WIPHistorian.Count > 0)
                            {
                                foreach (tblhmiscreen row in WIPHistorian)
                                {
                                    int InnerOpNo = Convert.ToInt32(row.OperationNo);
                                    if (OperationNoInt > InnerOpNo)
                                    {
                                        if (row.Date == null) //=> lower OpNo is not submitted.
                                        {
                                            Session["VError"] = " Submit WONo: " + row.Work_Order_No + " and PartNo: " + row.PartNo + " and OperationNo: " + InnerOpNo;
                                            return RedirectToAction("Index");
                                        }
                                    }
                                }
                            }
                            //}

                            //New Logic to Check Sequential Submit 2017-01-21
                            bool IsInHMI = true;
                            //int OperationNoInt = Convert.ToInt32(opNo);
                            //string WIPQuery1 = @"SELECT * from tblddl where WorkOrder = '" + woNo + "' and MaterialDesc = '" + partNo + "' and OperationNo != '" + opNo + "'  and IsCompleted = 0 order by WorkOrder,MaterialDesc,OperationNo  ";
                            //var WIPDDL = db.tblddls.SqlQuery(WIPQuery1).ToList();
                            List<tblddl> WIPDDL = obj.GetddlDetails(woNo, partNo, opNo);
                            foreach (tblddl row in WIPDDL)
                            {
                                IsInHMI = true; //reinitialize
                                int InnerOpNo = Convert.ToInt32(row.OperationNo);
                                if (InnerOpNo < OperationNoInt)
                                {
                                    //string WIPQueryHMI = @"SELECT * from tbllivehmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + InnerOpNo + "' order by HMIID limit 1 ";
                                    //var WIP = db.tbllivehmiscreens.SqlQuery(WIPQueryHMI).ToList();
                                    WIP = obj.GetLive1HMIScreenDetails(woNo, partNo, Convert.ToString(InnerOpNo));
                                    //string WIPQueryHMIHistorian = @"SELECT * from tblhmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + InnerOpNo + "' order by HMIID limit 1 ";
                                    //var WIPHistorian = db.tblhmiscreens.SqlQuery(WIPQueryHMIHistorian).ToList();
                                    WIPHistorian = obj.GetLive1HScreenDetails(woNo, partNo, Convert.ToString(InnerOpNo));
                                    if (WIP.Count == 0 || WIPHistorian.Count == 0)
                                    {
                                        //Session["VError"] = " Select & Start WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
                                        //return RedirectToAction("Index");
                                        IsInHMI = false;
                                    }
                                    else
                                    {
                                        foreach (tbllivehmiscreen rowHMI in WIP)
                                        {
                                            if (rowHMI.Date == null) //=> lower OpNo is not submitted.
                                            {
                                                Session["VError"] = " Start WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;

                                                //If its in current list then its not error.
                                                bool inCurrentList = false;
                                                foreach (tbllivehmiscreen planrow in tbldaily_plan)
                                                {
                                                    if (InnerOpNo == Convert.ToInt32(planrow.OperationNo))
                                                    {
                                                        inCurrentList = true;
                                                        break;
                                                    }
                                                }
                                                if (!inCurrentList)
                                                {
                                                    return RedirectToAction("Index");
                                                }
                                            }
                                        }
                                        if (WIPHistorian.Count > 0)
                                        {
                                            foreach (tblhmiscreen rowHMI in WIPHistorian)
                                            {
                                                if (rowHMI.Date == null) //=> lower OpNo is not submitted.
                                                {
                                                    Session["VError"] = " Start WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;

                                                    //If its in current list then its not error.
                                                    bool inCurrentList = false;
                                                    foreach (tbllivehmiscreen planrow in tbldaily_plan)
                                                    {
                                                        if (InnerOpNo == Convert.ToInt32(planrow.OperationNo))
                                                        {
                                                            inCurrentList = true;
                                                            break;
                                                        }
                                                    }
                                                    if (!inCurrentList)
                                                    {
                                                        return RedirectToAction("Index");
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (!IsInHMI)
                                    {
                                        //Strange , it might have been started in Normal WorkCenter as MultiWorkOrder.
                                        #region //also check in MultiWO table
                                        //string WIPQueryMultiWO = @"SELECT * from tbllivemultiwoselection where WorkOrder = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + InnerOpNo + "' order by MultiWOID limit 1 ";
                                        //var WIPMWO = db.tbllivemultiwoselections.SqlQuery(WIPQueryMultiWO).ToList();
                                        List<tbllivemultiwoselection> WIPMWO = obj.GetMultiWOtDetails(woNo, partNo, Convert.ToString(InnerOpNo));
                                        //string WIPQueryMultiWOHistorian = @"SELECT * from tbl_multiwoselection where WorkOrder = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + InnerOpNo + "' order by MultiWOID limit 1 ";
                                        //var WIPMWOHistorian = db.tbl_multiwoselection.SqlQuery(WIPQueryMultiWOHistorian).ToList();
                                        List<tbl_multiwoselection> WIPMWOHistorian = obj.GetMultiWorkSelectionDetails(woNo, partNo, Convert.ToString(InnerOpNo));
                                        if (WIPMWO.Count == 0 || WIPMWOHistorian.Count == 0)
                                        {
                                            Session["VError"] = " Select  WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
                                            return RedirectToAction("Index");
                                            //IsInHMI = false;
                                            //break;
                                        }

                                        foreach (tbllivemultiwoselection rowHMI in WIPMWO)
                                        {
                                            int hmiid1 = Convert.ToInt32(rowHMI.HMIID);
                                            //var MWOHMIData = db.tbllivehmiscreens.Where(m => m.HMIID == hmiid1).FirstOrDefault();
                                            tbllivehmiscreen MWOHMIData = obj.GetLiveHMIDetails7(hmiid1);
                                            if (MWOHMIData != null) //obviously != 0
                                            {
                                                if (MWOHMIData.Date == null) //=> lower OpNo is not submitted.
                                                {
                                                    Session["VError"] = " Start WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
                                                    return RedirectToAction("Index");
                                                    //break;
                                                }
                                                else
                                                {
                                                    Session["VError"] = null;
                                                    IsInHMI = true;
                                                }
                                            }
                                        }
                                        foreach (tbl_multiwoselection rowHMI in WIPMWOHistorian)
                                        {
                                            int hmiid1 = Convert.ToInt32(rowHMI.HMIID);
                                            //var MWOHMIData = db.tblhmiscreens.Where(m => m.HMIID == hmiid1).FirstOrDefault();
                                            tbllivehmiscreen MWOHMIData = obj.GetLiveHMIDetails7(hmiid1);
                                            if (MWOHMIData != null) //obviously != 0
                                            {
                                                if (MWOHMIData.Date == null) //=> lower OpNo is not submitted.
                                                {
                                                    Session["VError"] = " Start WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
                                                    return RedirectToAction("Index");
                                                    //break;
                                                }
                                                else
                                                {
                                                    Session["VError"] = null;
                                                    IsInHMI = true;
                                                }
                                            }
                                        }
                                        #endregion
                                    }
                                }
                            }

                            //string WIPQuery1 = @"SELECT * from tblhmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo != '" + opNo + "' order by Work_Order_No,PartNo,OperationNo";

                            //Commented on 2017-05-29
                            /////to Catch those Manual WorkOrders 
                            //string WIPQuery2 = @"SELECT * from tblhmiscreen where  HMIID IN ( SELECT Max(HMIID) from tblhmiscreen where  HMIID IN  ( SELECT HMIID from tblhmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo != '" + opNo + "' and  IsMultiWO = 0 and DDLWokrCentre is null order by HMIID ) group by Work_Order_No,PartNo,OperationNo ) order by OperationNo ;";
                            //var WIPDDL1 = db.tblhmiscreens.SqlQuery(WIPQuery2).ToList();
                            //foreach (var row in WIPDDL1)
                            //{
                            //    int InnerOpNo = Convert.ToInt32(row.OperationNo);
                            //    if (InnerOpNo < OperationNoInt)
                            //    {
                            //        string WIPQueryHMI = @"SELECT * from tblhmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + InnerOpNo + "' order by HMIID limit 1 ";
                            //        var WIP = db.tblhmiscreens.SqlQuery(WIPQueryHMI).ToList();
                            //        if (WIP.Count == 0)
                            //        {
                            //            Session["VError"] = " Select & Start WONo: " + row.Work_Order_No + " and PartNo: " + row.PartNo + " and OperationNo: " + InnerOpNo;
                            //            return RedirectToAction("Index");
                            //        }
                            //        foreach (var rowHMI in WIP)
                            //        {
                            //            if (rowHMI.Date == null) //=> lower OpNo is not submitted.
                            //            {
                            //                Session["VError"] = " Start WONo: " + row.Work_Order_No + " and PartNo: " + row.PartNo + " and OperationNo: " + InnerOpNo;
                            //                return RedirectToAction("Index");
                            //            }
                            //        }
                            //    }
                            //}

                            #endregion

                            int TargetQtyNew = Convert.ToInt32(hmiidData.Target_Qty);
                            int DeliveredNew = Convert.ToInt32(hmiidData.Delivered_Qty);
                            int ProcessedNew = Convert.ToInt32(hmiidData.ProcessQty);
                            newProcessedQty = DeliveredNew + ProcessedNew;
                            if (Convert.ToInt32(hmiidData.isWorkInProgress) == 1)
                            {
                                Session["Error"] = "Job is Finished for WorkOrder:" + woNo + " OpNo: " + opNo + " PartNo:" + partNo;
                                //db.tbllivehmiscreens.Remove(hmiidData);
                                //db.SaveChanges();
                                obj.DeleteHMIScreenDetails(hmiidData.HMIID);
                                return RedirectToAction("Index");

                            }

                            if (TargetQtyNew == newProcessedQty)
                            {
                                //hmiidData.Target_Qty = newProcessedQty;
                                //hmiidData.ProcessQty = newProcessedQty;
                                //hmiidData.SplitWO = "No";
                                //hmiidData.isWorkInProgress = 1;
                                //hmiidData.Status = 2;
                                //hmiidData.Time = hmiidData.Date;
                                //hmiidData.Delivered_Qty = 0;

                                //db.Entry(hmiidData).State = System.Data.Entity.EntityState.Modified;
                                //db.SaveChanges();

                                obj.UpdateLivHMIDets(hmiidData.HMIID, newProcessedQty, newProcessedQty, "No", 1, 2, Convert.ToDateTime(hmiidData.Date), 0);

                                //if it existing in DDLList Update 
                                //var DDLList = db.tblddls.Where(m => m.WorkOrder == hmiidData.Work_Order_No && m.MaterialDesc == hmiidData.PartNo && m.OperationNo == hmiidData.OperationNo && m.IsCompleted == 0).ToList();
                                List<tblddl> DDLList = obj.GetddlDets(hmiidData.Work_Order_No, hmiidData.PartNo, hmiidData.OperationNo);
                                foreach (tblddl row in DDLList)
                                {
                                    //row.IsCompleted = 1;
                                    //db.Entry(row).State = System.Data.Entity.EntityState.Modified;
                                    //db.SaveChanges();
                                    obj.UpdateddlDetails(row.DDLID);
                                }

                                Session["Error"] = "Job is Finished for WorkOrder:" + woNo + " OpNo: " + opNo + " PartNo:" + partNo;
                                return RedirectToAction("Index");

                            }

                            #region NOt Using 2017-06-03
                            //var getProcessQty = db.tblhmiscreens.Where(m => m.Work_Order_No == woNo && m.PartNo == partNo && m.OperationNo == opNo && m.isWorkInProgress != 2).OrderByDescending(m => m.Time).Take(2).ToList();
                            //if (getProcessQty.Count == 2)
                            //{
                            //    string delivString = Convert.ToString(getProcessQty[1].Delivered_Qty);
                            //    int.TryParse(delivString, out PrvDeliveredQty);

                            //    string processString = Convert.ToString(getProcessQty[1].ProcessQty);
                            //    int.TryParse(processString, out PrvProcessQty);

                            //    newProcessedQty = PrvProcessQty + PrvDeliveredQty;
                            //    if (Convert.ToInt32(getProcessQty[1].isWorkInProgress) == 1 || TargetQtyNew == newProcessedQty)
                            //    {
                            //        Session["Error"] = "Job is Finished for WorkOrder:" + woNo + " OpNo: " + opNo + " PartNo:" + partNo;

                            //        //2017-01-07
                            //        //hmiidData.Prod_FAI = null;
                            //        //hmiidData.Target_Qty = null;
                            //        //hmiidData.OperationNo = null;
                            //        //hmiidData.PartNo = null;
                            //        //hmiidData.Work_Order_No = null;
                            //        //hmiidData.Project = null;
                            //        //hmiidData.Date = null;
                            //        //hmiidData.DDLWokrCentre = null;
                            //        //hmiidData.isWorkOrder = 0;
                            //        //hmiidData.ProcessQty = 0;
                            //        //Session["FromDDL"] = 2;
                            //        //TempData["ForDDL2"] = 2;

                            //        //db.Entry(hmiidData).State = System.Data.Entity.EntityState.Modified;
                            //        //db.SaveChanges();

                            //        db.tblhmiscreens.Remove(hmiidData);
                            //        db.SaveChanges();
                            //        return RedirectToAction("Index");
                            //    }
                            //}
                            #endregion

                            if (TargetQtyNew < newProcessedQty)
                            {
                                Session["Error"] = "Previous ProcessedQty :" + newProcessedQty + ". TargetQty Cannot be Less than Processed";
                                hmiidData.ProcessQty = 0;
                                hmiidData.Date = null;
                                Session["FromDDL"] = 2;
                                TempData["ForDDL2"] = 2;
                                //db.Entry(hmiidData).State = System.Data.Entity.EntityState.Modified;
                                //db.SaveChanges();
                                obj.UpdateLivHMI1Dets(hmiidData.HMIID, hmiidData.ProcessQty, Convert.ToDateTime(hmiidData.Date));
                                return RedirectToAction("Index");
                            }

                            //////////////////////////////////hmiidData.ProcessQty = Convert.ToInt32(PrvProcessQty + PrvDeliveredQty);

                            int ReworkOrder = 0;
                            string ReworkOrderString = Convert.ToString(Session["isWorkOrder"]);
                            if (int.TryParse(ReworkOrderString, out ReworkOrder))
                            {
                                if (ReworkOrderString == "1")
                                {
                                    hmiidData.isWorkOrder = 1;
                                }
                                else
                                {
                                    hmiidData.isWorkOrder = 0;
                                }
                            }

                            Session["WorkOrderClicked"] = 0;

                            //2017-03-14
                            DateTime EndTime = DateTime.Now;
                            int hmiiid = hmiidData.HMIID;
                            //1) 1remove hold on current one 
                            //var hmiData = db.tbllivehmiscreens.Where(m => m.HMIID == hmiiid).FirstOrDefault();
                            tbllivehmiscreen hmiData = obj.GetLiveHMIDetails6(hmiid);
                            hmiData.IsHold = 0;
                            //db.Entry(hmiData).State = System.Data.Entity.EntityState.Modified;
                            //db.SaveChanges();

                            obj.UpdateLiveHMIScreen1Detail(hmiData.HMIID, hmiData.IsHold);

                            int OldHMIID = 0;
                            //update the ishold and end time in old one.
                            //var tblhmi = db.tbllivehmiscreens.Where(m => m.HMIID != hmiiid && m.Work_Order_No == hmiData.Work_Order_No && m.PartNo == hmiData.PartNo && m.OperationNo == hmiData.OperationNo).OrderByDescending(m => m.PEStartTime).FirstOrDefault();
                            tbllivehmiscreen tblhmi = obj.GetLive3HMIDetails(hmiid, hmiData.Work_Order_No, hmiData.PartNo, hmiData.OperationNo);
                            if (tblhmi != null)
                            {
                                OldHMIID = tblhmi.HMIID;
                                tblhmi.IsHold = 2;
                                //db.Entry(tblhmi).State = System.Data.Entity.EntityState.Modified;
                                //db.SaveChanges();
                                obj.UpdateLiveHMIScreen1Detail(tblhmi.HMIID, tblhmi.IsHold);
                                //3) update the EndDateTime column in manuallossofentry table.
                                //var tblmanualLossData = db.tblmanuallossofentries.Where(m => m.HMIID == OldHMIID).OrderByDescending(m => m.StartDateTime).FirstOrDefault();
                                tblmanuallossofentry tblmanualLossData = obj.GetManualLossofEntryDetails(OldHMIID);
                                if (tblmanualLossData != null)
                                {
                                    tblmanualLossData.EndHMIID = hmiiid;
                                    tblmanualLossData.EndDateTime = EndTime;
                                    //db.Entry(tblmanualLossData).State = System.Data.Entity.EntityState.Modified;
                                    //db.SaveChanges();
                                    obj.UpdateManualLossofEntryDetails(Convert.ToInt32(tblmanualLossData.HMIID), Convert.ToInt32(tblmanualLossData.EndHMIID), Convert.ToDateTime(tblmanualLossData.EndDateTime));
                                }
                            }

                            //db.Entry(hmiidData).State = System.Data.Entity.EntityState.Modified;
                            //db.SaveChanges();
                            obj.UpdateLivH2MIDetails(hmiidData.HMIID, Convert.ToDateTime(hmiidData.Date), hmiidData.isWorkOrder, hmiidData.ProcessQty);
                        }
                    }
                    Session["SubmitClicked"] = 1;
                    return RedirectToAction("Index");
                }
            }
            return View();
        }

        //Developer : 
        public int HandleIdle()
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            int status = -1;
            int doneWithRow = -1;//some default value
            int isUpdate = -1;
            int lossid = -1;
            int isStart = 0, isScreen = 0;
            int machineid = Convert.ToInt32(Session["MachineID"]);
            int userid = Convert.ToInt16(Session["UserID"]);
            DateTime endTime = DateTime.Now, startTime = DateTime.Now;
            string shift = null;
            string LCorrectedDate, todaysCorrectedDate;

            todaysCorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            LCorrectedDate = todaysCorrectedDate;//dummy initializaition;
            //correcteddate
            string correcteddate = null;
            //tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
            tbldaytiming StartTime = obj.GetDaytimingDetails();
            TimeSpan Start = StartTime.StartTime;
            if (Start <= DateTime.Now.TimeOfDay)
            {
                correcteddate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                correcteddate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }

            //shift
            DateTime Time = DateTime.Now;
            TimeSpan Tm = new TimeSpan(Time.Hour, Time.Minute, Time.Second);
            //var ShiftDetails = db.tblshift_mstr.Where(m => m.StartTime <= Tm && m.EndTime >= Tm);
            tblshift_mstr ShiftDetails = obj.GetShiftDetails(Tm);
            string Shift = null;
            if (ShiftDetails != null)
            {
                Shift = ShiftDetails.ShiftName;
            }

            //var lossStatusData = db.tbllivelossofentries.Where(m => m.MachineID == machineid).OrderByDescending(m => m.StartDateTime).FirstOrDefault();
            tbllivelossofentry lossStatusData = obj.GetLossOfEntryDetails1(machineid);
            if (lossStatusData != null)
            {
                lossid = lossStatusData.LossID;
                doneWithRow = lossStatusData.DoneWithRow;
                isUpdate = lossStatusData.IsUpdate;
                endTime = Convert.ToDateTime(lossStatusData.EndDateTime);
                startTime = Convert.ToDateTime(lossStatusData.StartDateTime);
                shift = lossStatusData.Shift;
                isStart = Convert.ToInt32(lossStatusData.IsStart);
                isScreen = Convert.ToInt32(lossStatusData.IsScreen);
                LCorrectedDate = lossStatusData.CorrectedDate;
            }

            if (doneWithRow == 0 && isUpdate == 0 && isStart == 1 && isScreen == 0)
            {
                string x = Convert.ToString(Session["showIdlePopUp"]);
                int value;
                if (int.TryParse(x, out value))
                {
                }
                else
                {
                    Session["showIdlePopUp"] = 0;
                }
                status = 0;
            }
            else if (doneWithRow == 0 && isUpdate == 1 && isStart == 1 && isScreen == 1)
            {
                //don't add code to show popup
                Session["showIdlePopUp"] = 2;
                status = 0;
            }

            else if (doneWithRow == 1 && isUpdate == 1 && isStart == 0 && isScreen == 0)
            {
                //RedirectToAction("Index");
                Session["showIdlePopUp"] = 0;
                status = 1;
            }
            else if (doneWithRow == 0 && isUpdate == 1 && isStart == 1 && isScreen == 0)
            {
                //RedirectToAction("Index");

                Session["showIdlePopUp"] = 0;
                //status = 1;
                status = 1;
            }

            //checking for shift change.
            //Getting Shift Value
            #region
            //string[] Shift1 = GetShift(machineid);
            //if (LCorrectedDate == todaysCorrectedDate)
            //{
            //    if (shift == Shift1[0])
            //    {
            //        //shift has not changed so do nothing.
            //    }
            //    else
            //    {
            //        if (isUpdate == 0 && doneWithRow == 0)
            //        {
            //            string colorcode = null;
            //            var dailyprodstatusdata1 = db.tbldailyprodstatus.Where(m => m.IsDeleted == 0 && m.CorrectedDate == correcteddate && m.MachineID == machineid && m.StartTime > startTime && (m.ColorCode == "green" || m.ColorCode == "blue" || m.ColorCode == "red")).OrderBy(m => m.StartTime).FirstOrDefault();
            //            if (dailyprodstatusdata1 != null)
            //            {
            //                colorcode = dailyprodstatusdata1.ColorCode;
            //                endTime = Convert.ToDateTime(dailyprodstatusdata1.StartTime);
            //            }
            //            DateTime starttimeGetShiftMethod = Convert.ToDateTime(todaysCorrectedDate + " " + Shift1[1]);

            //            if (colorcode == "green" || colorcode == "blue" || colorcode == "red")
            //            {
            //                var lossofentryrow = db.tbllossofentries.Where(m => m.MachineID == machineid && m.CorrectedDate == correcteddate).OrderByDescending(m => m.StartDateTime).FirstOrDefault();
            //                if (lossofentryrow != null)
            //                {
            //                    lossofentryrow.EndDateTime = endTime;
            //                    lossofentryrow.IsUpdate = 1;
            //                    lossofentryrow.DoneWithRow = 1;
            //                    db.Entry(lossofentryrow).State = System.Data.Entity.EntityState.Modified;
            //                    db.SaveChanges();
            //                }
            //            }
            //            else
            //            {
            //                var lossdata = db.tbllossofentries.Find(lossid);
            //                lossdata.EndDateTime = starttimeGetShiftMethod;
            //                lossdata.IsUpdate = 1;
            //                lossdata.DoneWithRow = 1;
            //                db.Entry(lossdata).State = System.Data.Entity.EntityState.Modified;
            //                db.SaveChanges();

            //            }

            //            //insert fresh row
            //            //Session["showIdlePopUp"] = 1;
            //            tbllossofentry lossentry = new tbllossofentry();
            //            lossentry.Shift = Shift1[0];
            //            lossentry.EntryTime = starttimeGetShiftMethod;
            //            lossentry.StartDateTime = starttimeGetShiftMethod;
            //            lossentry.EndDateTime = starttimeGetShiftMethod;
            //            lossentry.CorrectedDate = correcteddate;
            //            lossentry.IsUpdate = 0;
            //            lossentry.DoneWithRow = 0;
            //            lossentry.MessageCodeID = 999;
            //            int abc = Convert.ToInt32(lossentry.MessageCodeID);
            //            var a = db.message_code_master.Find(abc);
            //            lossentry.MessageDesc = a.MessageDescription.ToString();
            //            lossentry.MessageCode = a.MessageCode.ToString();
            //            lossentry.MachineID = machineid;

            //            if (ModelState.IsValid)
            //            {
            //                Session["showIdlePopUp"] = 0;
            //                db.tbllossofentries.Add(lossentry);
            //                db.SaveChanges();
            //            }
            //        }
            //        else if (isUpdate == 1 && doneWithRow == 0)
            //        {
            //            string colorcode = null;
            //            var dailyprodstatusdata1 = db.tbldailyprodstatus.Where(m => m.IsDeleted == 0 && m.CorrectedDate == correcteddate && m.MachineID == machineid && m.StartTime > startTime && (m.ColorCode == "green" || m.ColorCode == "blue" || m.ColorCode == "red")).OrderBy(m => m.StartTime).FirstOrDefault();
            //            if (dailyprodstatusdata1 != null)
            //            {
            //                colorcode = dailyprodstatusdata1.ColorCode;
            //                endTime = Convert.ToDateTime(dailyprodstatusdata1.StartTime);
            //            }
            //            DateTime starttimeGetShiftMethod = Convert.ToDateTime(todaysCorrectedDate + " " + Shift1[1]);

            //            if (colorcode == "green" || colorcode == "blue" || colorcode == "red")
            //            {
            //                var lossofentryrow = db.tbllossofentries.Where(m => m.MachineID == machineid && m.CorrectedDate == correcteddate).OrderByDescending(m => m.StartDateTime).FirstOrDefault();
            //                if (lossofentryrow != null)
            //                {
            //                    lossofentryrow.EndDateTime = endTime;
            //                    lossofentryrow.IsUpdate = 1;
            //                    lossofentryrow.DoneWithRow = 1;
            //                    db.Entry(lossofentryrow).State = System.Data.Entity.EntityState.Modified;
            //                    db.SaveChanges();
            //                }
            //            }
            //            else
            //            {
            //                var lossdata = db.tbllossofentries.Find(lossid);
            //                lossdata.EndDateTime = starttimeGetShiftMethod;
            //                lossdata.IsUpdate = 1;
            //                lossdata.DoneWithRow = 1;
            //                db.Entry(lossdata).State = System.Data.Entity.EntityState.Modified;
            //                db.SaveChanges();

            //            }

            //            var lossdata1 = db.tbllossofentries.Find(lossid);
            //            //insert fresh row
            //            //Session["showIdlePopUp"] = 1;
            //            tbllossofentry lossentry = new tbllossofentry();
            //            lossentry.Shift = Shift1[0];
            //            lossentry.EntryTime = starttimeGetShiftMethod;
            //            lossentry.StartDateTime = starttimeGetShiftMethod;
            //            lossentry.EndDateTime = starttimeGetShiftMethod;
            //            lossentry.CorrectedDate = correcteddate;
            //            lossentry.IsUpdate = 0;
            //            lossentry.DoneWithRow = 0;
            //            lossentry.MessageCodeID = lossdata1.MessageCodeID;
            //            int abc = Convert.ToInt32(lossdata1.MessageCodeID);
            //            var a = db.message_code_master.Find(abc);
            //            lossentry.MessageDesc = a.MessageDescription.ToString();
            //            lossentry.MessageCode = a.MessageCode.ToString();
            //            lossentry.MachineID = machineid;

            //            if (ModelState.IsValid)
            //            {
            //                Session["showIdlePopUp"] = 0;
            //                db.tbllossofentries.Add(lossentry);
            //                db.SaveChanges();
            //            }

            //        }
            //    }

            //}
            //else
            //{
            //    #region
            //    if (isUpdate == 0 && doneWithRow == 0)
            //    {
            //        string colorcode = null;
            //        var dailyprodstatusdata1 = db.tbldailyprodstatus.Where(m => m.IsDeleted == 0 && m.CorrectedDate == correcteddate && m.MachineID == machineid && m.StartTime > startTime && (m.ColorCode == "green" || m.ColorCode == "blue" || m.ColorCode == "red")).OrderBy(m => m.StartTime).FirstOrDefault();
            //        if (dailyprodstatusdata1 != null)
            //        {
            //            colorcode = dailyprodstatusdata1.ColorCode;
            //            endTime = Convert.ToDateTime(dailyprodstatusdata1.StartTime);
            //        }
            //        DateTime starttimeGetShiftMethod = Convert.ToDateTime(todaysCorrectedDate + " " + Shift1[1]);

            //        if (colorcode == "green" || colorcode == "blue" || colorcode == "red")
            //        {
            //            var lossofentryrow = db.tbllossofentries.Where(m => m.MachineID == machineid && m.CorrectedDate == correcteddate).OrderByDescending(m => m.StartDateTime).FirstOrDefault();
            //            if (lossofentryrow != null)
            //            {
            //                lossofentryrow.EndDateTime = endTime;
            //                lossofentryrow.IsUpdate = 1;
            //                lossofentryrow.DoneWithRow = 1;
            //                db.Entry(lossofentryrow).State = System.Data.Entity.EntityState.Modified;
            //                db.SaveChanges();
            //            }
            //        }
            //        else
            //        {
            //            var lossdata = db.tbllossofentries.Find(lossid);
            //            lossdata.EndDateTime = starttimeGetShiftMethod;
            //            lossdata.IsUpdate = 1;
            //            lossdata.DoneWithRow = 1;
            //            db.Entry(lossdata).State = System.Data.Entity.EntityState.Modified;
            //            db.SaveChanges();

            //        }


            //        //insert fresh row
            //        //Session["showIdlePopUp"] = 1;
            //        tbllossofentry lossentry = new tbllossofentry();
            //        lossentry.Shift = Shift1[0];
            //        lossentry.EntryTime = starttimeGetShiftMethod;
            //        lossentry.StartDateTime = starttimeGetShiftMethod;
            //        lossentry.EndDateTime = starttimeGetShiftMethod;
            //        lossentry.CorrectedDate = correcteddate;
            //        lossentry.IsUpdate = 0;
            //        lossentry.DoneWithRow = 0;
            //        lossentry.MessageCodeID = 999;
            //        int abc = Convert.ToInt32(lossentry.MessageCodeID);
            //        var a = db.message_code_master.Find(abc);
            //        lossentry.MessageDesc = a.MessageDescription.ToString();
            //        lossentry.MessageCode = a.MessageCode.ToString();
            //        lossentry.MachineID = machineid;

            //        if (ModelState.IsValid)
            //        {
            //            Session["showIdlePopUp"] = 0;
            //            db.tbllossofentries.Add(lossentry);
            //            db.SaveChanges();
            //        }
            //    }
            //    else if (isUpdate == 1 && doneWithRow == 0)
            //    {
            //        string colorcode = null;
            //        var dailyprodstatusdata1 = db.tbldailyprodstatus.Where(m => m.IsDeleted == 0 && m.CorrectedDate == correcteddate && m.MachineID == machineid && m.StartTime > startTime && (m.ColorCode == "green" || m.ColorCode == "blue" || m.ColorCode == "red")).OrderBy(m => m.StartTime).FirstOrDefault();
            //        if (dailyprodstatusdata1 != null)
            //        {
            //            colorcode = dailyprodstatusdata1.ColorCode;
            //            endTime = Convert.ToDateTime(dailyprodstatusdata1.StartTime);
            //        }
            //        DateTime starttimeGetShiftMethod = Convert.ToDateTime(todaysCorrectedDate + " " + Shift1[1]);

            //        if (colorcode == "green" || colorcode == "blue" || colorcode == "red")
            //        {
            //            var lossofentryrow = db.tbllossofentries.Where(m => m.MachineID == machineid && m.CorrectedDate == correcteddate).OrderByDescending(m => m.StartDateTime).FirstOrDefault();
            //            if (lossofentryrow != null)
            //            {
            //                lossofentryrow.EndDateTime = endTime;
            //                lossofentryrow.IsUpdate = 1;
            //                lossofentryrow.DoneWithRow = 1;
            //                db.Entry(lossofentryrow).State = System.Data.Entity.EntityState.Modified;
            //                db.SaveChanges();
            //            }
            //        }
            //        else
            //        {
            //            var lossdata = db.tbllossofentries.Find(lossid);
            //            lossdata.EndDateTime = starttimeGetShiftMethod;
            //            lossdata.IsUpdate = 1;
            //            lossdata.DoneWithRow = 1;
            //            db.Entry(lossdata).State = System.Data.Entity.EntityState.Modified;
            //            db.SaveChanges();

            //        }

            //        var lossdata1 = db.tbllossofentries.Find(lossid);
            //        //insert fresh row

            //        tbllossofentry lossentry = new tbllossofentry();
            //        lossentry.Shift = Shift1[0];
            //        lossentry.EntryTime = starttimeGetShiftMethod;
            //        lossentry.StartDateTime = starttimeGetShiftMethod;
            //        lossentry.EndDateTime = starttimeGetShiftMethod;
            //        lossentry.CorrectedDate = correcteddate;
            //        lossentry.IsUpdate = 0;
            //        lossentry.DoneWithRow = 0;
            //        lossentry.MessageCodeID = lossdata1.MessageCodeID;
            //        int abc = Convert.ToInt32(lossdata1.MessageCodeID);
            //        var a = db.message_code_master.Find(abc);
            //        lossentry.MessageDesc = a.MessageDescription.ToString();
            //        lossentry.MessageCode = a.MessageCode.ToString();
            //        lossentry.MachineID = machineid;

            //        if (ModelState.IsValid)
            //        {
            //            Session["showIdlePopUp"] = 0;
            //            db.tbllossofentries.Add(lossentry);
            //            db.SaveChanges();
            //        }

            //    }
            //    #endregion

            //}

            #endregion

            #region
            //if ((isUpdate == 1 && doneWithRow == 1) || lossStatusData == null)
            //{

            //    #region
            //    int yellowcount = 0;

            //    //var dailyprodstatusdata = db.tbldailyprodstatus.Where(m => m.IsDeleted == 0 && m.CorrectedDate == correcteddate && m.MachineID == machineid).OrderByDescending(m => m.StartTime);
            //    //int checkcount = 0;
            //    //foreach (var dailyrow in dailyprodstatusdata)
            //    //{
            //    //    if (dailyrow.ColorCode == "yellow")
            //    //    {
            //    //        yellowcount++;
            //    //    }
            //    //    if (checkcount >= 2)
            //    //    {
            //    //        break;
            //    //    }
            //    //}

            //    bool IdleStatus = false;
            //    int TotalMinute = 0;
            //    TotalMinute = System.DateTime.Now.Subtract(startTime).Minutes;
            //    if (TotalMinute >= 3)
            //    {
            //        #region DownColor
            //        int count = 0;
            //        int ContinuesChecking = 0;
            //        var productionstatus = db.tbldailyprodstatus.Where(m => m.CorrectedDate == correcteddate && m.MachineID == machineid && m.StartTime > startTime).OrderByDescending(m => m.StartTime);
            //        foreach (var check in productionstatus)
            //        {
            //            if (ContinuesChecking < 2)
            //            {
            //                if (check.ColorCode == "yellow")
            //                {
            //                    count++;
            //                    if (count == 2)
            //                    {
            //                        break;
            //                    }
            //                }
            //                else
            //                {
            //                    count = 0;
            //                }
            //                ContinuesChecking++;
            //            }
            //            else
            //                break;
            //        }
            //        if (count >= 2 && ContinuesChecking < 5)
            //        {
            //            IdleStatus = true;
            //        }
            //        #endregion
            //    }

            //    if (IdleStatus)
            //    {
            //        DateTime starttime = GetIdleStartTime(0, correcteddate);
            //        //insert fresh row
            //        tbllossofentry lossentry = new tbllossofentry();
            //        lossentry.Shift = Session["realshift"].ToString();
            //        lossentry.EntryTime = starttime;
            //        lossentry.StartDateTime = starttime;
            //        lossentry.EndDateTime = starttime;
            //        lossentry.CorrectedDate = correcteddate;
            //        lossentry.IsUpdate = 0;
            //        lossentry.DoneWithRow = 0;
            //        lossentry.MessageCodeID = 999;
            //        int abc = Convert.ToInt32(lossentry.MessageCodeID);
            //        var a = db.message_code_master.Find(abc);
            //        lossentry.MessageDesc = a.MessageDescription.ToString();
            //        lossentry.MessageCode = a.MessageCode.ToString();
            //        lossentry.MachineID = machineid;

            //        if (ModelState.IsValid)
            //        {
            //            Session["showIdlePopUp"] = 0;
            //            db.tbllossofentries.Add(lossentry);
            //            db.SaveChanges();
            //        }

            //        //RedirectToAction("DownCodeEntry");
            //        status = 0;
            //    }
            //    //else do nothing
            //    #endregion

            //}
            ////its a fresh row so, check color. if yellow update endtime if green end IDLE
            //else if (isUpdate == 0 && doneWithRow == 0)
            //{

            //    #region

            //    string colorcode = null;
            //    var dailyprodstatusdata1 = db.tbldailyprodstatus.Where(m => m.IsDeleted == 0 && m.CorrectedDate == correcteddate && m.MachineID == machineid && m.StartTime > startTime && (m.ColorCode == "green" || m.ColorCode == "blue" || m.ColorCode == "red")).OrderBy(m => m.StartTime).FirstOrDefault();
            //    if (dailyprodstatusdata1 != null)
            //    {
            //        colorcode = dailyprodstatusdata1.ColorCode;
            //        endTime = Convert.ToDateTime(dailyprodstatusdata1.StartTime);
            //    }

            //    bool IdleStatus = false;
            //    int TotalMinute = 0;
            //    TotalMinute = System.DateTime.Now.Subtract(startTime).Minutes;
            //    if (TotalMinute >= 2)
            //    {
            //        #region DownColor
            //        int count = 0;
            //        int ContinuesChecking = 0;
            //        var productionstatus = db.tbldailyprodstatus.Where(m => m.CorrectedDate == correcteddate && m.MachineID == machineid && m.StartTime > startTime).OrderByDescending(m => m.StartTime);
            //        foreach (var check in productionstatus)
            //        {
            //            if (ContinuesChecking < 2)
            //            {
            //                if (check.ColorCode == "yellow")
            //                {
            //                    count++;
            //                    if (count == 2)
            //                    {
            //                        break;
            //                    }
            //                }
            //                else
            //                {
            //                    count = 0;
            //                }
            //                ContinuesChecking++;
            //            }
            //            else
            //                break;
            //        }
            //        if (count >= 2 && ContinuesChecking < 5)
            //        {
            //            IdleStatus = true;
            //        }
            //        #endregion
            //    }
            //    #region commented
            //    //if (colorcode == "yellow") //update endtime 
            //    //{
            //    //    var lossofentryrow = db.tbllossofentries.Where(m => m.MachineID == machineid && m.CorrectedDate == correcteddate).OrderByDescending(m => m.StartDateTime).Take(1);
            //    //    foreach (var row in lossofentryrow)
            //    //    {
            //    //        row.EndDateTime = DateTime.Now;
            //    //        db.Entry(row).State = System.Data.Entity.EntityState.Modified;
            //    //        db.SaveChanges();
            //    //        break;
            //    //    }
            //    //    //if you have isUpdate & donewithrow then use its lossID . This fail's for the 1st time.
            //    //    //RedirectToAction("DownCodeEntry");
            //    //    //status = 0;
            //    //}
            //    //else 
            //    #endregion

            //    if (IdleStatus)
            //    {
            //        status = 0;
            //    }
            //    else if (colorcode == "green" || colorcode == "blue" || colorcode == "red")
            //    {
            //        var lossofentryrow = db.tbllossofentries.Where(m => m.MachineID == machineid && m.CorrectedDate == correcteddate).OrderByDescending(m => m.StartDateTime).FirstOrDefault();
            //        if(lossofentryrow != null)
            //        {
            //            lossofentryrow.EndDateTime = endTime;
            //            lossofentryrow.IsUpdate = 1;
            //            lossofentryrow.DoneWithRow = 1;
            //            db.Entry(lossofentryrow).State = System.Data.Entity.EntityState.Modified;
            //            db.SaveChanges();
            //        }
            //        //RedirectToAction("Index");
            //        status = 1;
            //    }

            //    #endregion

            //}

            // //its already updated row . update endtime every minute here.
            //// idleCode reentry will be handled in downcodeentry POST Method.
            //else if (isUpdate == 1 && doneWithRow == 0)
            //{

            //    #region
            //    string colorcode = null;
            //    var dailyprodstatusdata2 = db.tbldailyprodstatus.Where(m => m.IsDeleted == 0 && m.CorrectedDate == correcteddate && m.MachineID == machineid && m.StartTime > startTime && (m.ColorCode == "green" || m.ColorCode == "blue" || m.ColorCode == "red")).OrderBy(m => m.StartTime).FirstOrDefault();
            //    if (dailyprodstatusdata2 != null)
            //    {
            //        colorcode = dailyprodstatusdata2.ColorCode;
            //        endTime = Convert.ToDateTime(dailyprodstatusdata2.StartTime);
            //    }

            //    bool IdleStatus = false;
            //    int TotalMinute = 0;
            //    TotalMinute = System.DateTime.Now.Subtract(endTime).Minutes;
            //    if (TotalMinute >= 2)
            //    {
            //        #region DownColor
            //        int count = 0;
            //        int ContinuesChecking = 0;
            //        var productionstatus = db.tbldailyprodstatus.Where(m => m.CorrectedDate == correcteddate && m.MachineID == machineid && m.StartTime > endTime).OrderByDescending(m => m.StartTime);
            //        foreach (var check in productionstatus)
            //        {

            //            if (ContinuesChecking < 2)
            //            {
            //                if (check.ColorCode == "yellow")
            //                {
            //                    count++;
            //                    if (count == 2)
            //                    {
            //                        break;
            //                    }
            //                }
            //                else
            //                {
            //                    count = 0;
            //                }
            //                ContinuesChecking++;
            //            }
            //            else
            //                break;
            //        }
            //        if (count >= 2 && ContinuesChecking < 5)
            //        {
            //            IdleStatus = true;
            //        }
            //        #endregion
            //    }
            //    if (IdleStatus)
            //    {
            //        status = 0;
            //    }
            //    else if (colorcode == "green" || colorcode == "blue" || colorcode == "red")
            //    {
            //        var lossofentryrow = db.tbllossofentries.Where(m => m.MachineID == machineid && m.CorrectedDate == correcteddate).OrderByDescending(m => m.StartDateTime).FirstOrDefault();
            //        if (lossofentryrow != null)
            //        {
            //            lossofentryrow.EndDateTime = endTime;
            //            lossofentryrow.IsUpdate = 1;
            //            lossofentryrow.DoneWithRow = 1;
            //            db.Entry(lossofentryrow).State = System.Data.Entity.EntityState.Modified;
            //            db.SaveChanges();
            //        }
            //        //RedirectToAction("Index");
            //        status = 1;
            //    }
            //    #endregion
            //}
            #endregion
            //return RedirectToAction("Index");

            return status;
        }

        public string[] GetShift(int machineID)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            string[] shift = new string[4];
            shift[0] = "C";
            DateTime Time1 = DateTime.Now;
            TimeSpan Tm1 = new TimeSpan(Time1.Hour, Time1.Minute, Time1.Second);
            //var Shiftdetails = db.tblshift_mstr.Where(m => m.StartTime <= Tm1 && m.EndTime >= Tm1).FirstOrDefault();
            tblshift_mstr Shiftdetails = obj.GetShiftDetails(Tm1);
            if (Shiftdetails != null)
            {
                shift[0] = Shiftdetails.ShiftName;
                shift[1] = Shiftdetails.StartTime.ToString();
                shift[2] = Shiftdetails.EndTime.ToString();
            }

            return shift;
        }

        //1st yellow row in tbldailyprodstatus after endtime of donewithrow in tbllossofentry for that machine , or now
        public DateTime GetIdleStartTime(int status, string correcteddate)
        {
            IntoFile("GetIdleStartTime:" + status);
            IntoFile("GetIdleStartTime:" + correcteddate);
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            DateTime starttime = DateTime.Now;
            DateTime lastdonewithrowendtime = new DateTime(2012, 12, 12);
            DateTime duplicatedate = lastdonewithrowendtime;
            int machineid = Convert.ToInt32(Session["MachineID"]);

            //using (i_facility_tsalEntities db1 = new i_facility_tsalEntities())
            //{

            //var lastdownwithrowdata = db1.tbllivelossofentries.Where(m => m.MachineID == machineid && m.IsUpdate == 1 && m.DoneWithRow == 1).OrderByDescending(m => m.StartDateTime).FirstOrDefault();
            tbllivelossofentry lastdownwithrowdata = obj.GetLossOfEntryDetails2(machineid);
            if (lastdownwithrowdata != null)
            {
                IntoFile("GetIdleStartTime lastdownwithrowdata:" + lastdownwithrowdata.EndDateTime);
                lastdonewithrowendtime = Convert.ToDateTime(lastdownwithrowdata.EndDateTime);
                if (status == 4)
                {

                    starttime = lastdonewithrowendtime;
                    IntoFile("GetIdleStartTime starttime:" + starttime);
                }
                else
                {
                    IntoFile("GetIdleStartTime status !=4:");
                    //var dailyprodstatusdata = db1.tbldailyprodstatus.Where(m => m.IsDeleted == 0 && m.CorrectedDate == correcteddate && m.MachineID == machineid).OrderByDescending(m => m.StartTime);
                    //foreach (var dailyrow in dailyprodstatusdata)
                    //{
                    //    if (dailyrow.ColorCode == "yellow")
                    //    {
                    //        starttime = Convert.ToDateTime(dailyrow.StartTime);
                    //    }
                    //    else
                    //    {
                    //        break;
                    //    }
                    //}

                    //starttime = (DateTime)db1.tbllivemodedbs.Where(m => m.CorrectedDate == correcteddate && m.MachineID == machineid).OrderByDescending(m => m.StartTime).Select(m => m.StartTime).FirstOrDefault();
                    starttime = Convert.ToDateTime(obj.GetLiveModeDetails(correcteddate, machineid));
                    IntoFile("GetIdleStartTime status !=4 starttime:" + starttime);
                }
            }
            else
            {
                IntoFile("GetIdleStartTime IsUpdate == 1 && m.DoneWithRow == 1 data is not there in lossofentry:");
                //var dailyprodstatusdata = db1.tbldailyprodstatus.Where(m => m.IsDeleted == 0 && m.CorrectedDate == correcteddate && m.MachineID == machineid).OrderByDescending(m => m.StartTime);
                //foreach (var dailyrow in dailyprodstatusdata)
                //{
                //    if (dailyrow.ColorCode == "yellow")
                //    {
                //        starttime = Convert.ToDateTime(dailyrow.StartTime);
                //    }
                //    else
                //    {
                //        break;
                //    }
                //}
                //starttime = (DateTime)db1.tbllivemodedbs.Where(m => m.CorrectedDate == correcteddate && m.MachineID == machineid).OrderByDescending(m => m.StartTime).Select(m => m.StartTime).FirstOrDefault();
                starttime = Convert.ToDateTime(obj.GetLiveModeDetails(correcteddate, machineid));
                IntoFile("GetIdleStartTime IsUpdate == 1 && m.DoneWithRow == 1 data is not there in lossofentry starttime:" + starttime);
            }
            //}
            return starttime;
        }

        #region Commented by Monika
        //public JsonResult ReworkOrderClicked(string values)
        //{
        //    _conn = new ConnectionFactory();
        //    obj = new Dao(_conn);
        //    int nothing = 0;
        //    //Session["FromDDL"] = 2;
        //    //var thisrow = db.tbllivehmiscreens.Find(HMIID);
        //    //thisrow.isWorkOrder = 1;
        //    //db.Entry(thisrow).State = System.Data.Entity.EntityState.Modified;
        //    //db.SaveChanges();
        //    string[] ids = values.Split(',');
        //    if (ids.Length == 1)
        //    {
        //        int id = Convert.ToInt32(ids);
        //        var hmiidrecord = obj.GetLiveHMIDetails101(id);
        //        if (hmiidrecord.OperatorDet == null && hmiidrecord.OperationNo == null)
        //        {
        //            Session["error"] = "Please Select workorder Then select Reworkorder";
        //        }
        //    }
        //    foreach (var item in ids)
        //    {
        //        int id = Convert.ToInt32(item);
        //        var hmiidrecord = obj.GetLiveHMIDetails101(id);
        //        if (hmiidrecord.Date != null)
        //        {
        //            nothing = 1;
        //            Session["error"] = "The Workorder is already started so you Could not able to Select Reworkorder";
        //        }
        //        else if (hmiidrecord.Date == null)
        //        {
        //            obj.UpdateLiveHMIScreenDetails(id);
        //            nothing = 0;
        //        }
        //    }
        //    if (nothing == 0)
        //    {
        //        Session["isWorkOrder"] = 1;
        //        Session["WorkOrderClicked"] = 1;
        //        var a = Session["isWorkOrder"];
        //    }
        //    else { }

        //    return Json(nothing, JsonRequestBehavior.AllowGet);
        //}

        #endregion
        #region rework click functionality by Monika
        public JsonResult ReworkOrderClicked(string[] values)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);
            int nothing = 0;
            //Session["FromDDL"] = 2;
            //var thisrow = db.tbllivehmiscreens.Find(HMIID);
            //thisrow.isWorkOrder = 1;
            //db.Entry(thisrow).State = System.Data.Entity.EntityState.Modified;
            //db.SaveChanges();
            //string[] ids = values.Split(',');
            foreach (string item in values)
            {
                int id = Convert.ToInt32(item);
                obj.UpdateLiveHMIScreenDetails(id);
            }
            Session["isWorkOrder"] = 1;
            Session["WorkOrderClicked"] = 1;
            object a = Session["isWorkOrder"];
            return Json(nothing, JsonRequestBehavior.AllowGet);
        }
        #endregion


        public JsonResult AutoSave(int HMIID, string field, string value)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            //var selectedRow = new SelectList(db.tblmachinedetails.Where(m => m.ShopNo == countryId).Where(m => m.IsDeleted == 0), "MachineDispName", "MachineDispName");
            //int hmiid = HMIID - 1;
            //var thisrow = db.tbllivehmiscreens.Where(m => m.HMIID == HMIID).ToList();
            List<tbllivehmiscreen> thisrow = obj.GetLiveHMIDetailsList1(HMIID);
            int nothing = 1;
            switch (field)
            {
                case "cjtextboxshift":
                    {
                        //thisrow[0].Shift = value;
                        obj.UpdateLiveHMIScreenDetails1(thisrow[0].HMIID, value);
                        break;
                    }
                case "cjtextboxop":
                    {
                        if (value != "")
                        {
                            thisrow[0].OperatorDet = value;
                            thisrow[0].PEStartTime = DateTime.Now;

                        }
                        else
                        {
                            thisrow[0].OperatorDet = null;
                            thisrow[0].PEStartTime = DateTime.Now;
                        }
                        obj.UpdateLiveHMIScreenDetails2(thisrow[0].HMIID, thisrow[0].OperatorDet, Convert.ToDateTime(thisrow[0].PEStartTime));
                        break;
                    }
                case "cjtextbox1":
                    {
                        if (value != "")
                        {
                            //thisrow[0].PEStartTime = DateTime.Now;
                            thisrow[0].Project = value;

                        }
                        else
                        {
                            thisrow[0].Project = null;
                        }
                        obj.UpdateLiveHMIScreenDetails3(thisrow[0].HMIID, thisrow[0].Project);
                        break;
                    }
                case "Prod_FAI":
                    {
                        if (value != "")
                        {
                            //thisrow[0].Prod_FAI = value;
                            obj.UpdateLiveHMIScreenDetails4(thisrow[0].HMIID, value);
                        }
                        break;
                    }
                case "cjtextbox3":
                    {
                        if (value != "")
                        {
                            thisrow[0].PartNo = value;
                        }
                        else
                        {
                            thisrow[0].PartNo = null;
                        }
                        obj.UpdateLiveHMIScreenDetails5(thisrow[0].HMIID, thisrow[0].PartNo);
                        break;
                    }
                case "cjtextbox4":
                    {
                        if (value != "")
                        {
                            thisrow[0].Work_Order_No = value;
                        }
                        else
                        {
                            thisrow[0].Work_Order_No = null;
                        }
                        obj.UpdateLiveHMIScreenDetails6(thisrow[0].HMIID, thisrow[0].Work_Order_No);
                        break;
                    }
                case "cjtextbox5":
                    {
                        if (value != "")
                        {
                            thisrow[0].OperationNo = value;
                        }
                        else
                        {
                            thisrow[0].OperationNo = null;
                        }
                        obj.UpdateLiveHMIScreenDetails7(thisrow[0].HMIID, thisrow[0].OperationNo);
                        break;
                    }
                case "cjtextbox6":
                    {
                        if (value != "")
                        {
                            thisrow[0].Target_Qty = Convert.ToInt32(value);
                        }
                        else
                        {
                            thisrow[0].Target_Qty = null;
                        }
                        obj.UpdateLiveHMIScreenDetails8(thisrow[0].HMIID, Convert.ToString(thisrow[0].Target_Qty));
                        break;
                    }
                //case "cjtextbox7":
                //    {
                //        if (value != "")
                //            thisrow[0].Rej_Qty = Convert.ToInt32(value);
                //        break;
                //    }
                case "cjtextbox8":
                    {
                        if (value != "")
                        {
                            //bool retstatus = objprev.CalPrevQty(HMIID);
                            //if (retstatus == true)
                            //{
                            //    thisrow[0].prevQty = Convert.ToInt32(value);
                            //}
                            //else
                            //{
                            thisrow[0].Delivered_Qty = Convert.ToInt32(value);
                            //}
                        }
                        else
                        {
                            thisrow[0].Delivered_Qty = null;
                        }
                        obj.UpdateLiveHMIScreenDetailsD(thisrow[0].HMIID, Convert.ToString(thisrow[0].Delivered_Qty));
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            //db.Entry(thisrow[0]).State = System.Data.Entity.EntityState.Modified;
            //db.SaveChanges();

            return Json(nothing, JsonRequestBehavior.AllowGet);
        }

        public string AutoSaveOPName(string field, string value)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);
            string res = "";
            //var selectedRow = new SelectList(db.tblmachinedetails.Where(m => m.ShopNo == countryId).Where(m => m.IsDeleted == 0), "MachineDispName", "MachineDispName");
            //int hmiid = HMIID - 1;
            int macID = Convert.ToInt32(Session["MachineID"]);
            //obj.IntoFile(macID.ToString());
            //var thisrow = db.tbllivehmiscreens.Where(m => m.MachineID == macID && m.Status == 0).ToList();
            List<tbllivehmiscreen> thisrow = obj.GetListHMIScreeDetails(macID);

            foreach (tbllivehmiscreen row in thisrow)
            {
                switch (field)
                {
                    case "cjtextboxshift":
                        {
                            //row.Shift = value;
                            Session["OpName"] = value;
                            obj.UpdateLiveHMIScreenDetails1(row.HMIID, value);
                            res = "Success";
                            break;
                        }
                    case "cjtextboxop":
                        {
                            row.OperatorDet = value;
                            row.PEStartTime = DateTime.Now;
                            int a = obj.UpdateLiveHMIScreenDetails2(row.HMIID, value, DateTime.Now);
                            if (a == 1)
                            {
                                res = "Success";
                            }
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }

                //db.Entry(row).State = System.Data.Entity.EntityState.Modified;
                //db.SaveChanges();
            }
            return res;
        }

        //To select work order or rework order

        public ActionResult ChooseWorkOrder()
        {

            //Code For Admin And Super Admin
            int RoleID = Convert.ToInt32(Session["RoleID"]);
            if (RoleID == 1 || RoleID == 2)
            {
                return RedirectToAction("SelectMachine", "HMIScree", null);
            }
            return View();
        }
        [HttpPost]
        public ActionResult ChooseWorkOrder(string wo, string reworkwo)
        {

            Session["isWorkOrder"] = reworkwo == null ? 0 : 1;
            int data = Convert.ToInt32(Session["isWorkOrder"]);
            //if()
            return RedirectToAction("Index");
        }

        //IDLE codes
        public ActionResult DownCodeEntry(int Bid = 0)
        {
            Session["Mode"] = null;
            Session["split"] = null;
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            Session["starttime"] = DateTime.Now;
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            //ViewData["MessageCodeID"] = new SelectList(db.message_code_master.Where(m => m.IsDeleted == 0).Where(m => m.MessageType == "IDLE" || m.MessageType == "SETUP"), "MessageCodeID", "MessageDescription");
            //corrected date
            string CorrectedDate = null;
            //tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
            tbldaytiming StartTime = obj.GetDaytimingDetails();
            TimeSpan Start = StartTime.StartTime;
            if (Start <= DateTime.Now.TimeOfDay)
            {
                CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }

            int macid = Convert.ToInt32(Session["MachineID"]);
            string shift = Session["realshift"].ToString();



            //var machinedispname = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == macid).Select(m => m.MachineDispName).FirstOrDefault();
            string machinedispname = obj.GetMachineDet1(macid);
            ViewBag.macDispName = Convert.ToString(machinedispname);

            #region Old code
            //var check = db.tbllossofentries.Where(m => m.MachineID == macid && m.CorrectedDate == CorrectedDate).OrderByDescending(m => m.LossID).ToList();//m.IsUpdate == 0 && m.DoneWithRow == 0   && m.Shift == shift
            //int downcheck = 1;
            //if (check.Count > 0)
            //{
            //    downcheck = check[0].DoneWithRow;
            //}
            //if (downcheck == 1)
            //{

            //    DateTime starttime = DateTime.Now;
            //    //this variable has been declared to get the start time of the idle
            //    var productionstatus1 = db.tbldailyprodstatus.Where(m => m.CorrectedDate == CorrectedDate && m.MachineID == macid).OrderByDescending(m => m.StartTime);
            //    foreach (var check1 in productionstatus1)
            //    {
            //        if (check1.ColorCode == "yellow")
            //        {
            //            starttime = Convert.ToDateTime(check1.StartTime);
            //        }
            //        else
            //        {
            //            break;
            //        }
            //    }

            //    tbllossofentry lossentry = new tbllossofentry();

            //    lossentry.Shift = Session["realshift"].ToString();
            //    lossentry.EntryTime = starttime;
            //    lossentry.StartDateTime = starttime;
            //    lossentry.EndDateTime = starttime;
            //    lossentry.CorrectedDate = CorrectedDate;
            //    lossentry.DoneWithRow = 0;
            //    //lossentry.MessageCodeID = 4; narendra
            //    lossentry.MessageCodeID = 999;
            //    int abc = Convert.ToInt32(lossentry.MessageCodeID);
            //    string msgcode = null;
            //    var a = db.message_code_master.Find(abc);
            //    lossentry.MessageDesc = a.MessageDescription.ToString();
            //    lossentry.MessageCode = a.MessageCode.ToString();
            //    //lossentry.MessageCodeID = validatingCode.MessageCodeID;
            //    lossentry.MachineID = Convert.ToInt32(Session["MachineID"]);
            //    lossentry.IsUpdate = 0;
            //    //if (ModelState.IsValid)
            //    {
            //        db.tbllossofentries.Add(lossentry);
            //        db.SaveChanges();
            //    }
            //    return View(lossentry);
            //}
            //else
            //{
            //    int id = 0;
            //    foreach (var a in check)
            //    {
            //        id = a.LossID;
            //        break;
            //    }
            //    tbllossofentry lossentry = db.tbllossofentries.Find(id);
            //    // situation is: user doesn't enter code and production starts
            //    var check1 = db.tbldailyprodstatus.Where(m => m.CorrectedDate == CorrectedDate && m.MachineID == macid && m.ColorCode == "green" && m.EndTime >= lossentry.StartDateTime);
            //    if (check1.Count() == 0)
            //    {
            //        lossentry.EndDateTime = DateTime.Now;

            //        db.Entry(lossentry).State = System.Data.Entity.EntityState.Modified;
            //        db.SaveChanges();
            //    }
            //    //update the messagecode and message description.
            //    else
            //    {
            //        foreach (var j in check1)
            //        {
            //            lossentry.EndDateTime = j.EndTime;
            //            lossentry.EntryTime = j.EndTime;
            //            lossentry.MessageCodeID = 999;

            //            lossentry.DoneWithRow = 1;

            //            lossentry.MessageDesc = "IDLECODE NOT ENTERED";
            //            db.Entry(lossentry).State = System.Data.Entity.EntityState.Modified;
            //            db.SaveChanges();
            //            return RedirectToAction("Index");
            //        }
            //    }
            //    return View(lossentry);
            //}
            #endregion

            int handleidleReturnValue = HandleIdle();
            if (handleidleReturnValue == 1)
            {
                Session["showIdlePopUp"] = 0;
                return RedirectToAction("Index");
            }

            //Get Previous Loss to Display.
            //var PrevIdleToView = db.tbllivelossofentries.Where(m => m.MachineID == macid && m.DoneWithRow == 0).OrderByDescending(m => m.LossID).FirstOrDefault();
            tbllivelossofentry PrevIdleToView = obj.GetLossOfEntryDetails3(macid);
            if (PrevIdleToView != null)
            {
                int losscode = PrevIdleToView.MessageCodeID;
                ViewBag.PrevLossName = GetLossPath(losscode);
                ViewBag.PrevLossStartTime = PrevIdleToView.StartDateTime;
            }

            //stage 2. Idle is running and u need to send data to view regarding that.

            //var IdleToView = db.tbllivelossofentries.Where(m => m.MachineID == macid).OrderByDescending(m => m.LossID).FirstOrDefault();
            tbllivelossofentry IdleToView = obj.GetLossOfEntryDetails4(macid);
            if (IdleToView != null) //implies idle is running
            {
                if (IdleToView.DoneWithRow == 0 && IdleToView.MessageCodeID != 999)
                {
                    int idlecode = Convert.ToInt32(IdleToView.MessageCodeID);
                    //var DataToView = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodeID == idlecode).ToList();
                    List<tbllossescode> DataToView = obj.GetLossCodeDetail8(idlecode);
                    ViewBag.Level = DataToView[0].LossCodesLevel;
                    ViewBag.LossCode = DataToView[0].LossCode;
                    ViewBag.LossId = DataToView[0].LossCodeID;
                    ViewBag.IdleStartTime = IdleToView.StartDateTime;
                }
            }

            //stage 3. Operator is selecting the Idle by traversing down the Hierarchy of LossCodes.
            if (Bid != 0)
            {
                //var lossdata = db.tbllossescodes.Find(Bid);
                tbllossescode lossdata = obj.GetLossCodeDetails(Bid);
                int level = lossdata.LossCodesLevel;
                string losscode = lossdata.LossCode;
                if (level == 1)
                {
                    //var level2Data = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel1ID == Bid && m.LossCodesLevel == 2 && m.LossCodesLevel2ID == null && m.MessageType != "BREAKDOWN").ToList();
                    List<tbllossescode> level2Data = obj.GetLossCodeDetails1(Bid);
                    if (level2Data.Count == 0)
                    {
                        //var level1Data = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == level && m.LossCodesLevel1ID == null && m.LossCodesLevel2ID == null && m.MessageType != "NoCode" && m.MessageType != "BREAKDOWN" && m.MessageType != "PM").ToList();
                        List<tbllossescode> level1Data = obj.GetLossCodeDetails2(level);
                        ViewBag.ItsLastLevel = "No Further Levels . Do you want to set " + losscode + " as reason.";
                        ViewBag.LossID = Bid;
                        ViewBag.Level = level;
                        ViewBag.breadScrum = losscode + "-->  ";
                        return View(level1Data);
                    }
                    ViewBag.Level = level + 1;
                    ViewBag.LossID = Bid;
                    ViewBag.breadScrum = losscode + "-->  ";
                    return View(level2Data);
                }
                else if (level == 2)
                {
                    //var level3Data = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel2ID == Bid && m.LossCodesLevel == 3 && m.MessageType != "BREAKDOWN").ToList();
                    List<tbllossescode> level3Data = obj.GetLossCodeDetails3(Bid);
                    int prevLevelId = Convert.ToInt32(lossdata.LossCodesLevel1ID);
                    //var level1data = db.tbllossescodes.Where(m => m.LossCodeID == prevLevelId).Select(m => m.LossCode).FirstOrDefault();
                    string level1data = obj.GetLossCodeDetails4(prevLevelId);

                    if (level3Data.Count == 0)
                    {
                        //var level2Data = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel1ID == prevLevelId && m.LossCodesLevel2ID == null).ToList();
                        List<tbllossescode> level2Data = obj.GetLossCodeDetails5(prevLevelId);
                        ViewBag.ItsLastLevel = "No Further Levels . Do you want to set " + losscode + " as reason.";
                        ViewBag.LossID = Bid;
                        ViewBag.Level = level;
                        ViewBag.breadScrum = level1data + " --> " + losscode + " --> ";
                        return View(level2Data);
                    }
                    ViewBag.breadScrum = level1data + " --> " + losscode;
                    ViewBag.Level = level + 1;
                    ViewBag.LossID = Bid;

                    return View(level3Data);
                }
                else if (level == 3)
                {
                    int prevLevelId = Convert.ToInt32(lossdata.LossCodesLevel2ID);
                    int FirstLevelID = Convert.ToInt32(lossdata.LossCodesLevel1ID);
                    //var level2scrum = db.tbllossescodes.Where(m => m.LossCodeID == prevLevelId).Select(m => m.LossCode).FirstOrDefault();
                    string level2scrum = obj.GetLossCodeDetails4(prevLevelId);
                    //var level1scrum = db.tbllossescodes.Where(m => m.LossCodeID == FirstLevelID).Select(m => m.LossCode).FirstOrDefault();
                    string level1scrum = obj.GetLossCodeDetails4(FirstLevelID);
                    //var level2Data = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel2ID == prevLevelId && m.LossCodesLevel == 3).ToList();
                    List<tbllossescode> level2Data = obj.GetLossCodeDetails6(prevLevelId);
                    ViewBag.ItsLastLevel = "No Further Levels . Do you want to set " + losscode + " as reason.";
                    ViewBag.LossID = Bid;
                    ViewBag.Level = 3;
                    ViewBag.breadScrum = level1scrum + " --> " + level2scrum + " --> ";
                    return View(level2Data);
                }
            }
            else
            {
                //var level1Data = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.MessageType != "NoCode" && m.MessageType != "BREAKDOWN" && m.MessageType != "PM").ToList();
                List<tbllossescode> level1Data = obj.GetLossCodeDetails2(1);
                ViewBag.Level = 1;
                return View(level1Data);
            }

            //Fail Safe: if everything else fails send level1 codes.
            ViewBag.Level = 1;
            //var level10Data = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.MessageType != "NoCode" && m.MessageType != "BREAKDOWN" && m.MessageType != "PM").ToList();
            List<tbllossescode> level10Data = obj.GetLossCodeDetails2(1);
            return View(level10Data);

            //var lossentry = db.tbllossofentries.Where(m => m.MachineID == macid).OrderByDescending(m => m.StartDateTime).FirstOrDefault();
            //return View(lossentry);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DownCodeEntry(tbllossescode losscode, int HiddenID = 0)
        {
            Session["split"] = null;
            //IntoFile("DownCodeEntry Started:" + HiddenID);
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);
            //ViewData["MessageCodeID"] = new SelectList(db.message_code_master.Where(m => m.IsDeleted == 0).Where(m => m.MessageType == "IDLE" || m.MessageType == "SETUP"), "MessageCodeID", "MessageDescription");
            ViewData["MessageCodeID"] = new SelectList(obj.GetMessageCodeDetails(), "MessageCodeID", "MessageDescription");
            int RotationCount = Convert.ToInt32(Session["Rotation"]);
            if (RotationCount == 0)
            {
                Session["Rotation"] = 1;
            }
            if (RotationCount >= 3)
            {
                Session["Rotation"] = 1;
                //return RedirectToAction("Index", "MachineStatus", null);
            }

            //corrected date
            string CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            if (DateTime.Now.Hour < 6 && DateTime.Now.Hour >= 0)
            {
                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }

            //tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
            //TimeSpan Start = StartTime.StartTime;
            //if (Start <= DateTime.Now.TimeOfDay)
            //{
            //    CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            //}
            //else
            //{
            //    CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            //}



            //string MessageCode = lossentry.MessageCodeID.ToString();
            //var validatingCode = db.message_code_master.Where(m => m.MessageCode == MessageCode).Where(m => m.MessageType == "IDLE").FirstOrDefault();
            try
            {
                int machid = Convert.ToInt32(Session["MachineID"]);
                string shift = Session["realshift"].ToString();
                int isupdate = 0, donewithrow = 0;
                int lossid = 0;

                tblmachinedetail machinedetails = obj.GetMachineDet(machid);
                string IPAddress = "";
                int machineLockbit = 0;
                ushort h = 0;
                short ret = 1;
                if (machinedetails != null)
                {
                    IPAddress = machinedetails.IPAddress;
                    //MachineUnlockbit = Convert.ToInt32(machinedetails.MachineUnlockBit);
                    machineLockbit = Convert.ToInt32(machinedetails.MachineLockBit);

                }

                //here only 2 scenarios
                // 1 => isUpdate = 0 & doneWithRow = 0 :: we shall update the messageCode , isUpdate & stuff.
                // 2 => isUpdate = 1 & doneWithRow = 0 :: we shall update the old row & insert new one.
                //using (i_facility_tsalEntities db1 = new i_facility_tsalEntities())
                //{
                //var lossentryrow = db1.tbllivelossofentries.Where(m => m.MachineID == machid).OrderByDescending(m => m.LossID).Take(1).ToList();
                List<tbllivelossofentry> lossentryrow = obj.GetLossOfEntryDetails5(machid);
                foreach (tbllivelossofentry row in lossentryrow)
                {
                    isupdate = row.IsUpdate;
                    donewithrow = row.DoneWithRow;
                    lossid = row.LossID;
                    //IntoFile("Latest Record values:" + isupdate);
                    //IntoFile("Latest Record values:" + donewithrow);
                    //IntoFile("Latest Record values:" + lossid);
                    break;
                }
                //IDLE Popup has come, now we are updating the Losscode will be selected by Operator.
                if (isupdate == 0 && donewithrow == 0)
                {
                    //IntoFile("if (isupdate == 0 && donewithrow == 0):" + lossid);
                    tbllivelossofentry lossentry = obj.GetLossOfEntryDetails6(lossid);
                    if (HiddenID != 0)
                    {
                        //IntoFile("LossCodeId:" + HiddenID);
                        tbllossescode lossdata = obj.GetLossCodeDetails(HiddenID);
                        //obj.UpdateLossofEntryDetails(lossentry.LossID, (DateTime)lossentry.StartDateTime, DateTime.Now, DateTime.Now, machid, shift, HiddenID, lossdata.LossCodeDesc.ToString(), lossdata.LossCode.ToString(), 1, 0, 1, 0, 1);
                        obj.UpdateLossofEntryDetails(lossentry.LossID, DateTime.Now, DateTime.Now, machid, shift, HiddenID, lossdata.LossCodeDesc.ToString(), lossdata.LossCode.ToString(), 1, 0, 1, 0, 1);
                    }
                    else
                    {
                        //IntoFile("There is no losscode entered:");
                        int abc = Convert.ToInt32(lossentry.MessageCodeID);
                        message_code_master a = obj.GetMessageCodeDetails1(abc);

                        //obj.UpdateLossofEntryDetails(lossentry.LossID, GetIdleStartTime(0, CorrectedDate), DateTime.Now, DateTime.Now, machid, shift, Convert.ToInt32(lossentry.MessageCodeID), a.MessageDescription.ToString(), a.MessageCode.ToString(), 1, 0, 1, 0, 1);
                        obj.UpdateLossofEntryDetails(lossentry.LossID, DateTime.Now, DateTime.Now, machid, shift, Convert.ToInt32(lossentry.MessageCodeID), a.MessageDescription.ToString(), a.MessageCode.ToString(), 1, 0, 1, 0, 1);

                    }
                    if (machinedetails.EnableLockLogic == 1)
                    {
                        SRKSDemo.Server_Model.tbllivemode ModeDetList = new SRKSDemo.Server_Model.tbllivemode();
                        using (Server_Model.i_facility_shaktiEntities1 db1 = new Server_Model.i_facility_shaktiEntities1())
                        {
                            ModeDetList = db1.tbllivemodes.Where(m => m.MachineID == machid && m.IsCompleted == 0 && m.ColorCode == "YELLOW").OrderByDescending(m => m.ModeID).FirstOrDefault();
                        }

                        using (VirtualHMI vmi = new VirtualHMI(IPAddress, out h, out ret))
                        {
                            //Machine Unlocking
                            if (ret == 0)
                            {                              
                                setmachineUnlock(h, (ushort)machineLockbit, false);
                                UpdatetbllivemodeDetails(0, ModeDetList.ModeID);
                            }
                        }
                    }
                    return RedirectToAction("Index");
                }
                else if (isupdate == 1 && donewithrow == 0) // operator is entering new code for 2nd time and so on.
                {
                    //IntoFile("if (isupdate == 1 && donewithrow == 0):" + lossid);
                    tbllivelossofentry previousLoss = obj.GetLossOfEntryDetails6(lossid);
                    //IntoFile("if (isupdate == 0 && donewithrow == 0) Previous Lossid is:" + previousLoss.LossID);
                    obj.UpdateLossofEntryDetails1(previousLoss.LossID, DateTime.Now, 1, 0, 0, 0);
                    tbllivelossofentry loss = new tbllivelossofentry();

                    if (HiddenID != 0)
                    {
                        //IntoFile("LossCodeId:" + HiddenID);
                        tbllossescode lossdata = obj.GetLossCodeDetails(HiddenID);
                        previousLoss = obj.GetLossOfEntryDetails6(previousLoss.LossID);
                        DateTime EtTime = (DateTime)previousLoss.EndDateTime;
                        // IntoFile("StartTime coming from GetIdleStartTime method:" + EtTime);
                        if (previousLoss.EndDateTime <= EtTime)
                        {
                            //IntoFile(" if (previousLoss.EndDateTime <= stTime):");
                            //obj.InsertLossofEntryDetails(GetIdleStartTime(4, CorrectedDate), DateTime.Now, DateTime.Now, CorrectedDate, machid, shift, HiddenID, lossdata.LossCodeDesc.ToString(), lossdata.LossCode.ToString(), 1, 0, 1, 0, 1);
                            obj.InsertLossofEntryDetails(EtTime, DateTime.Now, DateTime.Now, CorrectedDate, machid, shift, HiddenID, lossdata.LossCodeDesc.ToString(), lossdata.LossCode.ToString(), 1, 0, 1, 0, 1);
                            if (machinedetails.EnableLockLogic == 1)
                                InsertModeafterLock(machid, CorrectedDate, EtTime);
                        }
                        else { }
                    }
                    else
                    {
                        //IntoFile("There is no losscode entered:");
                        int abc = 999;
                        message_code_master a = obj.GetMessageCodeDetails1(abc);
                        //DateTime stTime = GetIdleStartTime(4, CorrectedDate);
                        previousLoss = obj.GetLossOfEntryDetails6(previousLoss.LossID);
                        DateTime EtTime = (DateTime)previousLoss.EndDateTime;
                        //IntoFile("StartTime coming from GetIdleStartTime method:" + EtTime);
                        if (previousLoss.EndDateTime <= EtTime)
                        {
                            //IntoFile(" if (previousLoss.EndDateTime <= stTime):");
                            //obj.InsertLossofEntryDetails(GetIdleStartTime(4, CorrectedDate), DateTime.Now, DateTime.Now, CorrectedDate, machid, shift, Convert.ToInt32(loss.MessageCodeID), a.MessageDescription.ToString(), a.MessageCode.ToString(), 1, 0, 1, 0, 1);
                            obj.InsertLossofEntryDetails(EtTime, DateTime.Now, DateTime.Now, CorrectedDate, machid, shift, Convert.ToInt32(loss.MessageCodeID), a.MessageDescription.ToString(), a.MessageCode.ToString(), 1, 0, 1, 0, 1);
                            if (machinedetails.EnableLockLogic == 1)
                                InsertModeafterLock(machid, CorrectedDate, EtTime);
                        }
                        else { }
                    }
                    if (machinedetails.EnableLockLogic == 1)
                    {

                        using (VirtualHMI vmi = new VirtualHMI(IPAddress, out h, out ret))
                        {
                            //Machine Unlocking
                            if (ret == 0)
                            {
                                setmachineUnlock(h, (ushort)machineLockbit, false);
                            }
                        }
                    }
                    return RedirectToAction("Index");
                }

                #region old code
                ////if machine under setting( that is messageCodeID = 81 )
                //if (MessageCode != "81")
                //{
                //    //lossentry.Shift = Session["realshift"].ToString();
                //    //lossentry.EntryTime = DateTime.Now;
                //    //lossentry.StartDateTime = Convert.ToDateTime(Session["starttime"]);
                //    //lossentry.EndDateTime = DateTime.Now;
                //    //lossentry.CorrectedDate = CorrectedDate;
                //    //int abc = Convert.ToInt32(lossentry.MessageCodeID);
                //    //string msgcode = null;
                //    //var a = db.message_code_master.Find(abc);
                //    //lossentry.MessageDesc = a.MessageDescription.ToString();
                //    //lossentry.MessageCode = a.MessageCode.ToString();
                //    ////lossentry.MessageCodeID = validatingCode.MessageCodeID;
                //    //lossentry.MachineID = Convert.ToInt32(Session["MachineID"]);
                //    ////if (ModelState.IsValid)
                //    //{
                //    //    db.tbllossofentries.Add(lossentry);
                //    //    db.SaveChanges();
                //    //    return RedirectToAction("Index");
                //    //}

                //    int machid = Convert.ToInt32(Session["MachineID"]);
                //    string shift = Session["realshift"].ToString();
                //    var check = db.tbllossofentries.Where(m => m.MachineID == machid && m.CorrectedDate == CorrectedDate && m.IsUpdate == 0 && m.Shift == shift && m.DoneWithRow == 0).OrderByDescending(m => m.LossID);
                //    if (check.Count() != 0)
                //    {
                //        lossentry.EntryTime = DateTime.Now;
                //        lossentry.EndDateTime = DateTime.Now;
                //        int abc = Convert.ToInt32(lossentry.MessageCodeID);
                //        string msgcode = null;
                //        var a = db.message_code_master.Find(abc);
                //        lossentry.MessageDesc = a.MessageDescription.ToString();
                //        lossentry.MessageCode = a.MessageCode.ToString();
                //        lossentry.DoneWithRow = 0;
                //        lossentry.IsUpdate = 1;
                //        //lossentry.MessageCodeID = validatingCode.MessageCodeID;
                //        db.Entry(lossentry).State = System.Data.Entity.EntityState.Modified;
                //        db.SaveChanges();
                //        return RedirectToAction("Index");
                //    }
                //    else
                //    {
                //        var check1 = db.tbllossofentries.Where(m => m.MachineID == machid && m.CorrectedDate == CorrectedDate && m.IsUpdate == 1 && m.Shift == shift && m.DoneWithRow == 0).OrderByDescending(m => m.LossID);
                //        if (check1.Count() > 0)
                //        {
                //            foreach (var c in check1)
                //            {
                //                c.EndDateTime = DateTime.Now;
                //                c.DoneWithRow = 1;
                //                db.Entry(c).State = System.Data.Entity.EntityState.Modified;
                //                db.SaveChanges();
                //                break;
                //            }
                //            tbllossofentry tle = new tbllossofentry();

                //            tle.StartDateTime = DateTime.Now;
                //            tle.EntryTime = DateTime.Now;
                //            tle.EndDateTime = DateTime.Now;
                //            tle.CorrectedDate = CorrectedDate;
                //            tle.MachineID = machid;
                //            tle.Shift = shift;
                //            int abc = Convert.ToInt32(lossentry.MessageCodeID);
                //            string msgcode = null;
                //            tle.MessageCodeID = Convert.ToInt32(lossentry.MessageCodeID);
                //            var a = db.message_code_master.Find(abc);
                //            tle.MessageDesc = a.MessageDescription.ToString();
                //            tle.MessageCode = a.MessageCode.ToString();
                //            tle.IsUpdate = 1;
                //            tle.DoneWithRow = 0;
                //            //lossentry.MessageCodeID = validatingCode.MessageCodeID;
                //            db.tbllossofentries.Add(tle);
                //            //db.Entry(tle).State = System.Data.Entity.EntityState.Modified;
                //            db.SaveChanges();
                //            return RedirectToAction("Index");
                //        }
                //    }

                //}
                //else
                //{
                //    //lossentry.Shift = Session["realshift"].ToString();
                //    //lossentry.EntryTime = DateTime.Now;
                //    //lossentry.StartDateTime = DateTime.Now;
                //    //lossentry.CorrectedDate = CorrectedDate;
                //    //int abc = Convert.ToInt32(lossentry.MessageCodeID);
                //    //string msgcode = null;
                //    //var a = db.message_code_master.Find(abc);
                //    //lossentry.MessageDesc = a.MessageDescription.ToString();
                //    //lossentry.MessageCode = a.MessageCode.ToString();
                //    ////lossentry.MessageCodeID = validatingCode.MessageCodeID;
                //    //lossentry.MachineID = Convert.ToInt32(Session["MachineID"]);
                //    //if (ModelState.IsValid)
                //    //{
                //    //    db.tbllossofentries.Add(lossentry);
                //    //    db.SaveChanges();
                //    //    return RedirectToAction("Index");
                //    //}

                //    int machid = Convert.ToInt32(Session["MachineID"]);
                //    string shift = Session["realshift"].ToString();
                //    var check = db.tbllossofentries.Where(m => m.MachineID == machid && m.CorrectedDate == CorrectedDate && m.IsUpdate == 1 && m.Shift == shift && m.DoneWithRow == 0).OrderByDescending(m => m.LossID);
                //    if (check.Count() == 0)
                //    {
                //        lossentry.EntryTime = DateTime.Now;
                //        lossentry.EndDateTime = DateTime.Now;
                //        int abc = Convert.ToInt32(lossentry.MessageCodeID);
                //        string msgcode = null;
                //        var a = db.message_code_master.Find(abc);
                //        lossentry.MessageDesc = a.MessageDescription.ToString();
                //        lossentry.MessageCode = a.MessageCode.ToString();

                //        lossentry.DoneWithRow = 0;

                //        lossentry.IsUpdate = 1;
                //        //lossentry.MessageCodeID = validatingCode.MessageCodeID;
                //        db.Entry(lossentry).State = System.Data.Entity.EntityState.Modified;
                //        db.SaveChanges();
                //        return RedirectToAction("Index");
                //    }
                //    else
                //    {
                //        foreach (var b in check)
                //        {
                //            b.EndDateTime = DateTime.Now;
                //            //j 2016-06-15
                //            //b.DoneWithRow = 1;
                //            db.Entry(b).State = System.Data.Entity.EntityState.Modified;
                //            db.SaveChanges();
                //            break;
                //        }

                //        lossentry.StartDateTime = DateTime.Now;
                //        lossentry.EntryTime = DateTime.Now;
                //        lossentry.EndDateTime = DateTime.Now;
                //        int abc = Convert.ToInt32(lossentry.MessageCodeID);
                //        string msgcode = null;
                //        var a = db.message_code_master.Find(abc);
                //        lossentry.MessageDesc = a.MessageDescription.ToString();
                //        lossentry.MessageCode = a.MessageCode.ToString();
                //        lossentry.IsUpdate = 1;
                //        //lossentry.MessageCodeID = validatingCode.MessageCodeID;
                //        db.Entry(lossentry).State = System.Data.Entity.EntityState.Modified;
                //        db.SaveChanges();
                //        return RedirectToAction("Index");
                //    }

                //}
                #endregion

            }
            catch { }
            return RedirectToAction("Index");
        }

        #region Old Code
        //public ActionResult DownCodeEntry(tbllossescode losscode, int HiddenID = 0)
        //{
        //    _conn = new ConnectionFactory();
        //    obj = new Dao(_conn);
        //    //ViewData["MessageCodeID"] = new SelectList(db.message_code_master.Where(m => m.IsDeleted == 0).Where(m => m.MessageType == "IDLE" || m.MessageType == "SETUP"), "MessageCodeID", "MessageDescription");
        //    ViewData["MessageCodeID"] = new SelectList(obj.GetMessageCodeDetails(), "MessageCodeID", "MessageDescription");
        //    int RotationCount = Convert.ToInt32(Session["Rotation"]);
        //    if (RotationCount == 0)
        //    {
        //        Session["Rotation"] = 1;
        //    }
        //    if (RotationCount >= 3)
        //    {
        //        Session["Rotation"] = 1;
        //        //return RedirectToAction("Index", "MachineStatus", null);
        //    }

        //    //corrected date
        //    string CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
        //    if (DateTime.Now.Hour < 6 && DateTime.Now.Hour >= 0)
        //    {
        //        CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        //    }

        //    //tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
        //    //TimeSpan Start = StartTime.StartTime;
        //    //if (Start <= DateTime.Now.TimeOfDay)
        //    //{
        //    //    CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
        //    //}
        //    //else
        //    //{
        //    //    CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        //    //}



        //    //string MessageCode = lossentry.MessageCodeID.ToString();
        //    //var validatingCode = db.message_code_master.Where(m => m.MessageCode == MessageCode).Where(m => m.MessageType == "IDLE").FirstOrDefault();
        //    try
        //    {
        //        int machid = Convert.ToInt32(Session["MachineID"]);
        //        string shift = Session["realshift"].ToString();
        //        int isupdate = 0, donewithrow = 0;
        //        int lossid = 0;

        //        //here only 2 scenarios
        //        // 1 => isUpdate = 0 & doneWithRow = 0 :: we shall update the messageCode , isUpdate & stuff.
        //        // 2 => isUpdate = 1 & doneWithRow = 0 :: we shall update the old row & insert new one.
        //        //using (i_facility_tsalEntities db1 = new i_facility_tsalEntities())
        //        //{
        //        //var lossentryrow = db1.tbllivelossofentries.Where(m => m.MachineID == machid).OrderByDescending(m => m.LossID).Take(1).ToList();
        //        var lossentryrow = obj.GetLossOfEntryDetails5(machid);
        //        foreach (var row in lossentryrow)
        //        {
        //            isupdate = row.IsUpdate;
        //            donewithrow = row.DoneWithRow;
        //            lossid = row.LossID;
        //            break;
        //        }
        //        //IDLE Popup has come, now we are updating the Losscode will be selected by Operator.
        //        if (isupdate == 0 && donewithrow == 0)
        //        {
        //            tbllivelossofentry lossentry = obj.GetLossOfEntryDetails6(lossid);
        //            if (HiddenID != 0)
        //            {
        //                var lossdata = obj.GetLossCodeDetails(HiddenID);
        //                obj.UpdateLossofEntryDetails(lossentry.LossID, (DateTime)lossentry.StartDateTime, DateTime.Now, DateTime.Now, machid, shift, HiddenID, lossdata.LossCodeDesc.ToString(), lossdata.LossCode.ToString(), 1, 0, 1, 0, 1);
        //            }
        //            else
        //            {
        //                int abc = Convert.ToInt32(lossentry.MessageCodeID);
        //                var a = obj.GetMessageCodeDetails1(abc);
        //                obj.UpdateLossofEntryDetails(lossentry.LossID, GetIdleStartTime(0, CorrectedDate), DateTime.Now, DateTime.Now, machid, shift, Convert.ToInt32(lossentry.MessageCodeID), a.MessageDescription.ToString(), a.MessageCode.ToString(), 1, 0, 1, 0, 1);
        //            }

        //            return RedirectToAction("Index");
        //        }
        //        else if (isupdate == 1 && donewithrow == 0) // operator is entering new code for 2nd time and so on.
        //        {
        //            var previousLoss = obj.GetLossOfEntryDetails6(lossid);
        //            obj.UpdateLossofEntryDetails1(previousLoss.LossID, DateTime.Now, 1, 0, 0, 0);
        //            tbllivelossofentry loss = new tbllivelossofentry();

        //            if (HiddenID != 0)
        //            {
        //                var lossdata = obj.GetLossCodeDetails(HiddenID);
        //                obj.InsertLossofEntryDetails(GetIdleStartTime(4, CorrectedDate), DateTime.Now, DateTime.Now, CorrectedDate, machid, shift, HiddenID, lossdata.LossCodeDesc.ToString(), lossdata.LossCode.ToString(), 1, 0, 1, 0, 1);
        //            }
        //            else
        //            {
        //                int abc = 999;
        //                var a = obj.GetMessageCodeDetails1(abc);
        //                obj.InsertLossofEntryDetails(GetIdleStartTime(4, CorrectedDate), DateTime.Now, DateTime.Now, CorrectedDate, machid, shift, Convert.ToInt32(loss.MessageCodeID), a.MessageDescription.ToString(), a.MessageCode.ToString(), 1, 0, 1, 0, 1);
        //            }
        //            return RedirectToAction("Index");
        //        }

        //        #region old code
        //        ////if machine under setting( that is messageCodeID = 81 )
        //        //if (MessageCode != "81")
        //        //{
        //        //    //lossentry.Shift = Session["realshift"].ToString();
        //        //    //lossentry.EntryTime = DateTime.Now;
        //        //    //lossentry.StartDateTime = Convert.ToDateTime(Session["starttime"]);
        //        //    //lossentry.EndDateTime = DateTime.Now;
        //        //    //lossentry.CorrectedDate = CorrectedDate;
        //        //    //int abc = Convert.ToInt32(lossentry.MessageCodeID);
        //        //    //string msgcode = null;
        //        //    //var a = db.message_code_master.Find(abc);
        //        //    //lossentry.MessageDesc = a.MessageDescription.ToString();
        //        //    //lossentry.MessageCode = a.MessageCode.ToString();
        //        //    ////lossentry.MessageCodeID = validatingCode.MessageCodeID;
        //        //    //lossentry.MachineID = Convert.ToInt32(Session["MachineID"]);
        //        //    ////if (ModelState.IsValid)
        //        //    //{
        //        //    //    db.tbllossofentries.Add(lossentry);
        //        //    //    db.SaveChanges();
        //        //    //    return RedirectToAction("Index");
        //        //    //}

        //        //    int machid = Convert.ToInt32(Session["MachineID"]);
        //        //    string shift = Session["realshift"].ToString();
        //        //    var check = db.tbllossofentries.Where(m => m.MachineID == machid && m.CorrectedDate == CorrectedDate && m.IsUpdate == 0 && m.Shift == shift && m.DoneWithRow == 0).OrderByDescending(m => m.LossID);
        //        //    if (check.Count() != 0)
        //        //    {
        //        //        lossentry.EntryTime = DateTime.Now;
        //        //        lossentry.EndDateTime = DateTime.Now;
        //        //        int abc = Convert.ToInt32(lossentry.MessageCodeID);
        //        //        string msgcode = null;
        //        //        var a = db.message_code_master.Find(abc);
        //        //        lossentry.MessageDesc = a.MessageDescription.ToString();
        //        //        lossentry.MessageCode = a.MessageCode.ToString();
        //        //        lossentry.DoneWithRow = 0;
        //        //        lossentry.IsUpdate = 1;
        //        //        //lossentry.MessageCodeID = validatingCode.MessageCodeID;
        //        //        db.Entry(lossentry).State = System.Data.Entity.EntityState.Modified;
        //        //        db.SaveChanges();
        //        //        return RedirectToAction("Index");
        //        //    }
        //        //    else
        //        //    {
        //        //        var check1 = db.tbllossofentries.Where(m => m.MachineID == machid && m.CorrectedDate == CorrectedDate && m.IsUpdate == 1 && m.Shift == shift && m.DoneWithRow == 0).OrderByDescending(m => m.LossID);
        //        //        if (check1.Count() > 0)
        //        //        {
        //        //            foreach (var c in check1)
        //        //            {
        //        //                c.EndDateTime = DateTime.Now;
        //        //                c.DoneWithRow = 1;
        //        //                db.Entry(c).State = System.Data.Entity.EntityState.Modified;
        //        //                db.SaveChanges();
        //        //                break;
        //        //            }
        //        //            tbllossofentry tle = new tbllossofentry();

        //        //            tle.StartDateTime = DateTime.Now;
        //        //            tle.EntryTime = DateTime.Now;
        //        //            tle.EndDateTime = DateTime.Now;
        //        //            tle.CorrectedDate = CorrectedDate;
        //        //            tle.MachineID = machid;
        //        //            tle.Shift = shift;
        //        //            int abc = Convert.ToInt32(lossentry.MessageCodeID);
        //        //            string msgcode = null;
        //        //            tle.MessageCodeID = Convert.ToInt32(lossentry.MessageCodeID);
        //        //            var a = db.message_code_master.Find(abc);
        //        //            tle.MessageDesc = a.MessageDescription.ToString();
        //        //            tle.MessageCode = a.MessageCode.ToString();
        //        //            tle.IsUpdate = 1;
        //        //            tle.DoneWithRow = 0;
        //        //            //lossentry.MessageCodeID = validatingCode.MessageCodeID;
        //        //            db.tbllossofentries.Add(tle);
        //        //            //db.Entry(tle).State = System.Data.Entity.EntityState.Modified;
        //        //            db.SaveChanges();
        //        //            return RedirectToAction("Index");
        //        //        }
        //        //    }

        //        //}
        //        //else
        //        //{
        //        //    //lossentry.Shift = Session["realshift"].ToString();
        //        //    //lossentry.EntryTime = DateTime.Now;
        //        //    //lossentry.StartDateTime = DateTime.Now;
        //        //    //lossentry.CorrectedDate = CorrectedDate;
        //        //    //int abc = Convert.ToInt32(lossentry.MessageCodeID);
        //        //    //string msgcode = null;
        //        //    //var a = db.message_code_master.Find(abc);
        //        //    //lossentry.MessageDesc = a.MessageDescription.ToString();
        //        //    //lossentry.MessageCode = a.MessageCode.ToString();
        //        //    ////lossentry.MessageCodeID = validatingCode.MessageCodeID;
        //        //    //lossentry.MachineID = Convert.ToInt32(Session["MachineID"]);
        //        //    //if (ModelState.IsValid)
        //        //    //{
        //        //    //    db.tbllossofentries.Add(lossentry);
        //        //    //    db.SaveChanges();
        //        //    //    return RedirectToAction("Index");
        //        //    //}

        //        //    int machid = Convert.ToInt32(Session["MachineID"]);
        //        //    string shift = Session["realshift"].ToString();
        //        //    var check = db.tbllossofentries.Where(m => m.MachineID == machid && m.CorrectedDate == CorrectedDate && m.IsUpdate == 1 && m.Shift == shift && m.DoneWithRow == 0).OrderByDescending(m => m.LossID);
        //        //    if (check.Count() == 0)
        //        //    {
        //        //        lossentry.EntryTime = DateTime.Now;
        //        //        lossentry.EndDateTime = DateTime.Now;
        //        //        int abc = Convert.ToInt32(lossentry.MessageCodeID);
        //        //        string msgcode = null;
        //        //        var a = db.message_code_master.Find(abc);
        //        //        lossentry.MessageDesc = a.MessageDescription.ToString();
        //        //        lossentry.MessageCode = a.MessageCode.ToString();

        //        //        lossentry.DoneWithRow = 0;

        //        //        lossentry.IsUpdate = 1;
        //        //        //lossentry.MessageCodeID = validatingCode.MessageCodeID;
        //        //        db.Entry(lossentry).State = System.Data.Entity.EntityState.Modified;
        //        //        db.SaveChanges();
        //        //        return RedirectToAction("Index");
        //        //    }
        //        //    else
        //        //    {
        //        //        foreach (var b in check)
        //        //        {
        //        //            b.EndDateTime = DateTime.Now;
        //        //            //j 2016-06-15
        //        //            //b.DoneWithRow = 1;
        //        //            db.Entry(b).State = System.Data.Entity.EntityState.Modified;
        //        //            db.SaveChanges();
        //        //            break;
        //        //        }

        //        //        lossentry.StartDateTime = DateTime.Now;
        //        //        lossentry.EntryTime = DateTime.Now;
        //        //        lossentry.EndDateTime = DateTime.Now;
        //        //        int abc = Convert.ToInt32(lossentry.MessageCodeID);
        //        //        string msgcode = null;
        //        //        var a = db.message_code_master.Find(abc);
        //        //        lossentry.MessageDesc = a.MessageDescription.ToString();
        //        //        lossentry.MessageCode = a.MessageCode.ToString();
        //        //        lossentry.IsUpdate = 1;
        //        //        //lossentry.MessageCodeID = validatingCode.MessageCodeID;
        //        //        db.Entry(lossentry).State = System.Data.Entity.EntityState.Modified;
        //        //        db.SaveChanges();
        //        //        return RedirectToAction("Index");
        //        //    }

        //        //}
        //        #endregion

        //    }
        //    catch { }
        //    return RedirectToAction("Index");
        //}
        #endregion


        public string GetLossPath(int LossCode)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);
            string path = null;
            //var lossdata = db.tbllossescodes.Find(LossCode);
            tbllossescode lossdata = obj.GetLossCodeDetails(LossCode);
            int level = lossdata.LossCodesLevel;
            string losscode = lossdata.LossCode;
            if (level == 1)
            {
                path = losscode;
            }
            else if (level == 2)
            {
                int prevLevelId = Convert.ToInt32(lossdata.LossCodesLevel1ID);
                //var level1data = db.tbllossescodes.Where(m => m.LossCodeID == prevLevelId).Select(m => m.LossCode).FirstOrDefault();
                string level1data = obj.GetLossCodeDetails4(prevLevelId);
                path = level1data + " --> " + losscode;
            }
            else if (level == 3)
            {
                int prevLevelId = Convert.ToInt32(lossdata.LossCodesLevel2ID);
                int FirstLevelID = Convert.ToInt32(lossdata.LossCodesLevel1ID);
                //var level2scrum = db.tbllossescodes.Where(m => m.LossCodeID == prevLevelId).Select(m => m.LossCode).FirstOrDefault();
                //var level1scrum = db.tbllossescodes.Where(m => m.LossCodeID == FirstLevelID).Select(m => m.LossCode).FirstOrDefault();
                string level2scrum = obj.GetLossCodeDetails4(prevLevelId);
                string level1scrum = obj.GetLossCodeDetails4(FirstLevelID);
                path = level1scrum + " --> " + level2scrum + " --> " + losscode;
            }


            return path;
        }

        public ActionResult Details(int id = 0)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);
            //tbllivehmiscreen tblhmiscreen = db.tbllivehmiscreens.Find(id);
            tbllivehmiscreen tblhmiscreen = obj.GetLiveHMIDetails6(id);
            if (tblhmiscreen == null)
            {
                return HttpNotFound();
            }
            return View(tblhmiscreen);
        }

        public ActionResult Create()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            return View();
        }

        public ActionResult OverAllPartial(string HMIIDs)
        {
            Session["Mode"] = null;
            Session["split"] = null;
            Session["ddl"] = null;
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);
            //Session["Split"] = null;
            //1) extract hmiids from string
            //2) Check if eligible for  PartialFinish (Eligible only if all are eligible)
            //3) Now Partial Finish all of them
            int MachineID = Convert.ToInt32(Session["MachineID"]);
            //if (IDLEorGenericWorkisON())
            //{
            //    Session["Error"] = "Please End IDLE/GenericWork Before Selecting New Work Orders";
            //    return RedirectToAction("Index");
            //}

            //1)
            string[] HMIIDArray = HMIIDs.Split(',');
            List<string> HMIIDList = HMIIDs.Split(',').ToList();

            //2)
            int ExceptionHMIID = 0;
            foreach (string hmiid in HMIIDArray)
            {
                int hmiiid = Convert.ToInt32(hmiid);
                //var HMIData = db.tbllivehmiscreens.Where(m => m.HMIID == hmiiid).FirstOrDefault();
                tbllivehmiscreen HMIData = obj.GetLiveHMIDetails6(hmiiid);
                //if (HMIData.isWorkInProgress == 3)
                if (HMIData != null)
                {
                    if (HMIData.IsHold == 1)
                    {
                        Session["VError"] = "End HOLD before Partial Finish";
                        return RedirectToAction("Index");
                    }
                    else if (HMIData.Date != null)
                    {
                        int deliveredQty = 0;
                        if (int.TryParse(Convert.ToString(HMIData.Delivered_Qty), out deliveredQty))
                        {
                            int processed = 0;
                            int.TryParse(Convert.ToString(HMIData.ProcessQty), out processed);
                            if ((deliveredQty + processed) > Convert.ToInt32(HMIData.Target_Qty))
                            {
                                Session["VError"] = "DeliveredQty Must be less than Target for " + HMIData.Work_Order_No;
                                return RedirectToAction("Index");
                            }

                            #region 2017-02-07
                            else if ((deliveredQty + processed) == Convert.ToInt32(HMIData.Target_Qty))
                            {
                                //Check if highest of opno's for sibling wo's is eligible for JF, if Qty satisfies.
                                //1) if already lower op is jobfinished
                                //2) or  its in current HMIIDArray and valid for jf

                                string wono = HMIData.Work_Order_No;
                                string partno = HMIData.PartNo;
                                string opno = HMIData.OperationNo;
                                int opnoInt = Convert.ToInt32(HMIData.OperationNo);

                                //Gen , Seperated String from HMIIDArray
                                string hmiids = null;
                                for (int hmiidLooper = 0; hmiidLooper < HMIIDArray.Length; hmiidLooper++)
                                {
                                    if (hmiids == null)
                                    {
                                        string localhmiidString = HMIIDArray[hmiidLooper];
                                        if (ExceptionHMIID.ToString() != localhmiidString)
                                        {
                                            hmiids = HMIIDArray[hmiidLooper].ToString();
                                        }
                                    }
                                    else
                                    {
                                        if (ExceptionHMIID.ToString() != HMIIDArray[hmiidLooper])
                                        {
                                            hmiids += "," + HMIIDArray[hmiidLooper].ToString();
                                        }
                                    }
                                }
                                //Get hmiids (as comma seperated string) in ascending order based on wono,partno,Opno.
                                hmiids = GetOrderedHMIIDs(hmiids);
                                //string WIPQuery1 = @"SELECT * from tblddl where WorkOrder = '" + wono + "' and MaterialDesc = '" + partno + "'  and OperationNo != '" + opno + "' and IsCompleted = 0 order by OperationNo ";
                                //var WIP1 = dbHMI.tblddls.SqlQuery(WIPQuery1).ToList();
                                List<tblddl> WIP1 = obj.GettblddlList1(wono, partno, opno);
                                foreach (tblddl row in WIP1)
                                {
                                    int InnerOpNo = Convert.ToInt32(row.OperationNo);
                                    string InnerOpNoString = Convert.ToString(row.OperationNo);
                                    bool IsInHMI = true;
                                    if (opnoInt > InnerOpNo)
                                    {
                                        //string WIPQuery2 = @"SELECT * from tbllivehmiscreen where Work_Order_No = '" + wono + "' and PartNo = '" + partno + "'  and OperationNo = '" + InnerOpNo + "' and isWorkInProgress = 1 limit 1";
                                        //var WIP2 = dbHMI.tbllivehmiscreens.SqlQuery(WIPQuery2).FirstOrDefault();
                                        tbllivehmiscreen WIP2 = obj.GetLiveHMIDet2(wono, partno, InnerOpNo);
                                        if (WIP2 != null)
                                        {
                                            IsInHMI = true;
                                        }
                                        else
                                        {
                                            IsInHMI = false;
                                        }

                                        //We have to check for MultiWorkOrder.
                                        if (!IsInHMI)
                                        {
                                            //string WIPQuery3 = @"SELECT * from tbllivemultiwoselection where WorkOrder = '" + wono + "' and PartNo = '" + partno + "'  and OperationNo = '" + InnerOpNo + "' and IsCompleted = 1 limit 1";
                                            //var WIP3 = dbHMI.tbllivemultiwoselections.SqlQuery(WIPQuery3).FirstOrDefault();
                                            tbllivemultiwoselection WIP3 = obj.GetMultiWorkDetails(wono, InnerOpNo.ToString(), partno);
                                            if (WIP3 != null)
                                            {

                                            }
                                            else
                                            {
                                                Session["VError"] = "Finish WorkOrder: " + wono + ": OpNo: " + InnerOpNo;
                                                return RedirectToAction("Index");
                                            }
                                        }

                                    }
                                }
                            }
                            else if ((deliveredQty + processed) < Convert.ToInt32(HMIData.Target_Qty))
                            {
                                //Check if highest of opno's for sibling wo's is eligible for JF, if Qty satisfies.
                                //1) if already lower op is jobfinished
                                //2) or  its in current HMIIDArray and valid for jf

                                string wono = HMIData.Work_Order_No;
                                string partno = HMIData.PartNo;
                                string opno = HMIData.OperationNo;
                                int opnoInt = Convert.ToInt32(HMIData.OperationNo);

                                //Gen , Seperated String from HMIIDArray
                                string hmiids = null;
                                for (int hmiidLooper = 0; hmiidLooper < HMIIDArray.Length; hmiidLooper++)
                                {
                                    if (hmiids == null)
                                    {
                                        string localhmiidString = HMIIDArray[hmiidLooper];
                                        if (ExceptionHMIID.ToString() != localhmiidString)
                                        {
                                            hmiids = HMIIDArray[hmiidLooper].ToString();
                                        }
                                    }
                                    else
                                    {
                                        if (ExceptionHMIID.ToString() != HMIIDArray[hmiidLooper])
                                        {
                                            hmiids += "," + HMIIDArray[hmiidLooper].ToString();
                                        }
                                    }
                                }
                                //Get hmiids (as comma seperated string) in ascending order based on wono,partno,Opno.
                                hmiids = GetOrderedHMIIDs(hmiids);
                                //string WIPQuery1 = @"SELECT * from tblddl where WorkOrder = '" + wono + "' and MaterialDesc = '" + partno + "'  and OperationNo != '" + opno + "' and IsCompleted = 0 order by OperationNo ";
                                //var WIP1 = dbHMI.tblddls.SqlQuery(WIPQuery1).ToList();
                                List<tblddl> WIP1 = obj.GettblddlList1(wono, partno, opno);
                                foreach (tblddl row in WIP1)
                                {
                                    int InnerOpNo = Convert.ToInt32(row.OperationNo);
                                    string InnerOpNoString = Convert.ToString(row.OperationNo);
                                    bool IsInHMI = true;
                                    if (opnoInt > InnerOpNo)
                                    {
                                        //string WIPQuery2 = @"SELECT * from tbllivehmiscreen where Work_Order_No = '" + wono + "' and PartNo = '" + partno + "'  and OperationNo = '" + InnerOpNo + "' and isWorkInProgress = 1 limit 1";
                                        //var WIP2 = dbHMI.tbllivehmiscreens.SqlQuery(WIPQuery2).FirstOrDefault();
                                        tbllivehmiscreen WIP2 = obj.GetLiveHMIDet2(wono, partno, InnerOpNo);
                                        if (WIP2 != null)
                                        {
                                            IsInHMI = true;
                                        }
                                        //else
                                        //{
                                        //    IsInHMI = false;   //modified by monika 20190802
                                        //}

                                        //We have to check for MultiWorkOrder.
                                        if (!IsInHMI)
                                        {
                                            //string WIPQuery3 = @"SELECT * from tbllivemultiwoselection where WorkOrder = '" + wono + "' and PartNo = '" + partno + "'  and OperationNo = '" + InnerOpNo + "' and IsCompleted = 1 limit 1";
                                            //var WIP3 = dbHMI.tbllivemultiwoselections.SqlQuery(WIPQuery3).FirstOrDefault();
                                            tbllivemultiwoselection WIP3 = obj.GetMultiWorkDetails(wono, InnerOpNo.ToString(), partno);
                                            if (WIP3 != null)
                                            {

                                            }
                                            else
                                            {
                                                Session["VError"] = "Finish WorkOrder: " + wono + ": OpNo: " + InnerOpNo;
                                                return RedirectToAction("Index");
                                            }
                                        }
                                    }
                                }
                            }
                            //else
                            //{ //Do Nothing
                            //}
                            #endregion
                        }
                        else
                        {
                            Session["VError"] = "Enter Delivered Quantity";
                            return RedirectToAction("Index");
                        }
                    }

                    //else if (HMIData.Date == null && HMIData.Work_Order_No == null && HMIData.PartNo == null && HMIData.OperationNo == null)
                    else if (HMIData.Date == null)
                    {// Do Nothing. Just to Skip our Extra Empty Row
                        if (ExceptionHMIID == 0)
                        {
                            ExceptionHMIID = Convert.ToInt32(HMIData.HMIID);
                        }
                        else
                        {
                            //Session["VError"] = "Please enter all Details Before PartialFinish is Clicked.";
                            Session["VError"] = "Please Start All WorkOrders Before PartialFinish";
                            return RedirectToAction("Index");
                        }
                    }
                }
                else
                {
                    Session["VError"] = "Please Start All WorkOrders Before PartialFinish";
                    return RedirectToAction("Index");
                }
            }

            //Check if there are any row to partialFinish (Empty Screen)
            if (HMIIDList.Count == 1)
            {
                foreach (string hmiid in HMIIDArray)
                {
                    int hmiidi = Convert.ToInt32(hmiid);
                    if (hmiidi == ExceptionHMIID)
                    {
                        Session["VError"] = "There are no WorkOrder to Finish";
                        return RedirectToAction("Index");
                    }
                }
            }

            ////Check if there are any row to partialFinish (Empty Screen)
            //if (HMIIDList.Count == 1)
            //{
            //    foreach (var hmiid in HMIIDArray)
            //    {
            //        int hmiidi = Convert.ToInt32(hmiid);
            //        if (hmiidi == ExceptionHMIID)
            //        {
            //            Session["VError"] = "There are no WorkOrder to Finish";
            //            return RedirectToAction("Index");
            //        }
            //    }
            //}

            //Prevent JF is lower Opno is Not Elegible for JF
            //Check if all are eligable to JF, if not throw err.
            bool IsSequenceOK = true;
            foreach (string hmiid in HMIIDArray)
            {
                int hmiidi = Convert.ToInt32(hmiid);
                if (hmiidi != ExceptionHMIID)
                {
                    //var HMIData = db.tbllivehmiscreens.Where(m => m.HMIID == hmiidi).FirstOrDefault();
                    tbllivehmiscreen HMIData = obj.GetLiveHMIDetails6(hmiidi);
                    int DelivQty = 0, TargQty = 0, ProcessedQty = 0;

                    int.TryParse(HMIData.Target_Qty.ToString(), out TargQty);
                    int.TryParse(HMIData.Delivered_Qty.ToString(), out DelivQty);
                    int.TryParse(HMIData.ProcessQty.ToString(), out ProcessedQty);

                    if (TargQty >= (DelivQty + ProcessedQty))
                    {
                        string WoNo = HMIData.Work_Order_No;
                        string partNo = HMIData.PartNo;
                        int OpNo = Convert.ToInt32(HMIData.OperationNo);
                        foreach (string hmiid1 in HMIIDArray)
                        {
                            int hmiidiInner = Convert.ToInt32(hmiid1);
                            if (hmiidi != ExceptionHMIID)
                            {
                                tbllivehmiscreen HMIDataInner = obj.GetliveHMIScreenDetails1(hmiidiInner, WoNo, partNo, hmiidi);
                                //var HMIDataInner = dbhmi.tbllivehmiscreens.Where(m => m.HMIID == hmiidiInner && m.Work_Order_No == WoNo && m.PartNo == partNo && m.HMIID != hmiidi).FirstOrDefault();
                                if (HMIDataInner != null)
                                {
                                    int opNoInner = Convert.ToInt32(HMIDataInner.OperationNo);
                                    if (opNoInner < OpNo)
                                    {
                                        int DelivQty1 = 0, TargQty1 = 0, ProcessedQty1 = 0;
                                        int.TryParse(HMIDataInner.Target_Qty.ToString(), out TargQty1);
                                        int.TryParse(HMIDataInner.Delivered_Qty.ToString(), out DelivQty1);
                                        int.TryParse(HMIDataInner.ProcessQty.ToString(), out ProcessedQty1);
                                        if (TargQty1 < (DelivQty + ProcessedQty))
                                        {
                                            Session["VError"] = "Click JobFinish, All WorkOrders have (Delivered + Processed) = Target.";
                                            IsSequenceOK = false;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        if (!IsSequenceOK)
                        {
                            break;
                        }
                    }
                    else
                    {
                        IsSequenceOK = false;
                        break;
                    }
                }
            }

            if (!IsSequenceOK)
            {
                return RedirectToAction("Index");
            }

            //Check if all are eligable to JF, if so throw erro.
            bool AllREligableForJF = true;
            foreach (string hmiid in HMIIDArray)
            {
                int hmiidi = Convert.ToInt32(hmiid);
                if (hmiidi != ExceptionHMIID)
                {
                    //var HMIData = db.tbllivehmiscreens.Where(m => m.HMIID == hmiidi).FirstOrDefault();
                    tbllivehmiscreen HMIData = obj.GetLiveHMIDetails6(hmiidi);
                    int DelivQty = 0;
                    int TargQty = 0;
                    int ProcessedQty = 0;

                    int.TryParse(HMIData.Target_Qty.ToString(), out TargQty);
                    int.TryParse(HMIData.Delivered_Qty.ToString(), out DelivQty);
                    int.TryParse(HMIData.ProcessQty.ToString(), out ProcessedQty);



                    if (TargQty == (DelivQty + ProcessedQty))
                    {
                    }
                    else
                    {
                        AllREligableForJF = false;
                        break;
                    }
                }
            }
            if (AllREligableForJF)
            {
                Session["VError"] = "Click JobFinish, All WorkOrders have (Delivered + Processed) = Target.";
                return RedirectToAction("Index");
            }

            List<string> MacHierarchy = GetMacHierarchyData(MachineID);

            //Update ProcessedQty if WorkOrder Available in Different WorkCenter
            foreach (string hmiid in HMIIDArray)
            {
                int hmiidi = Convert.ToInt32(hmiid);
                if (hmiidi != ExceptionHMIID)
                {
                    //var HMIData = db.tbllivehmiscreens.Where(m => m.HMIID == hmiidi).FirstOrDefault();
                    tbllivehmiscreen HMIData = obj.GetLiveHMIDetails6(hmiidi);
                    int DelivQty = 0;
                    int.TryParse(HMIData.Delivered_Qty.ToString(), out DelivQty);
                    int HMIMacID = HMIData.MachineID;

                    #region If Its as SingleWO
                    List<tbllivehmiscreen> SimilarWOData = obj.GetLiHMIDetails(HMIData.Work_Order_No, HMIData.PartNo, HMIData.OperationNo, MachineID);
                    //var SimilarWOData = dbsimilar.tbllivehmiscreens.Where(m => m.Work_Order_No == HMIData.Work_Order_No && m.OperationNo == HMIData.OperationNo && m.PartNo == HMIData.PartNo && m.MachineID != MachineID && m.isWorkInProgress == 2).ToList();
                    foreach (tbllivehmiscreen row in SimilarWOData)
                    {
                        int InnerProcessed = row.ProcessQty;
                        int FinalProcessed = DelivQty + InnerProcessed;
                        if (FinalProcessed < row.Target_Qty)
                        {
                            if (row.isWorkInProgress == 2)
                            {
                                int ProcessQty = FinalProcessed;
                                int hmid = row.HMIID;
                                obj.GetHMIDet5(hmid, FinalProcessed);
                                //dbsimilar.Entry(row).State = System.Data.Entity.EntityState.Modified;
                                //dbsimilar.SaveChanges();
                            }
                        }
                        else
                        {
                            Session["Error"] = " Same WorkOrder in Machine: " + MacHierarchy[3] + "->" + MacHierarchy[4] + " has ProcessedQty :" + InnerProcessed;
                            return RedirectToAction("Index");
                        }
                    }
                    #endregion

                    #region If its as MultiWO
                    //var SimilarWODataMulti = dbsimilar.tbllivemultiwoselections.Where(m => m.WorkOrder == HMIData.Work_Order_No && m.OperationNo == HMIData.OperationNo && m.PartNo == HMIData.PartNo && m.HMIID != hmiidi && m.tbllivehmiscreen.isWorkInProgress == 2).ToList();
                    List<tbllivemultiwoselection> SimilarWODataMulti = obj.GetMultiWOListDetails(HMIData.Work_Order_No, HMIData.PartNo, HMIData.OperationNo, hmiidi);
                    foreach (tbllivemultiwoselection row in SimilarWODataMulti)
                    {
                        //update only if its still in hmiscreen
                        int RowHMIID = Convert.ToInt32(row.HMIID);
                        tbllivehmiscreen localhmiData = obj.GetLiveHMIDetails6(hmiidi);
                        //var localhmiData = dbsimilar.tbllivehmiscreens.Find(RowHMIID);
                        int DelivQtyLocal = Convert.ToInt32(row.DeliveredQty);
                        int InnerProcessed = Convert.ToInt32(row.ProcessQty);
                        int FinalProcessed = DelivQtyLocal + InnerProcessed;
                        if (FinalProcessed < row.TargetQty)
                        {
                            if (localhmiData.isWorkInProgress == 2)
                            {
                                int ProcessQty = FinalProcessed;
                                int hmid = localhmiData.HMIID;
                                obj.GetHMIDet5(hmid, FinalProcessed);
                                //row.ProcessQty = FinalProcessed;
                                //    dbsimilar.Entry(row).State = System.Data.Entity.EntityState.Modified;
                                //    dbsimilar.SaveChanges();

                                //Update tblhmiscreen table row.
                                if (localhmiData != null)
                                {
                                    //localhmiData.ProcessQty += DelivQtyLocal;
                                    int ProcessQty1 = +DelivQtyLocal;
                                    int hmid1 = localhmiData.HMIID;
                                    obj.GetHMIDet5(hmid1, ProcessQty1);
                                    //dbsimilar.Entry(localhmiData).State = System.Data.Entity.EntityState.Modified;
                                    //    dbsimilar.SaveChanges();
                                }
                            }
                        }
                        else
                        {
                            Session["Error"] = " Same WorkOrder in Machine: " + MacHierarchy[3] + "->" + MacHierarchy[4] + "has ProcessedQty :" + InnerProcessed;
                            return RedirectToAction("Index");
                        }

                        //int InnerHMIID = (int)row.HMIID;
                        //var InnerHMIDupData = dbsimilar.tblhmiscreens.Where(m => m.HMIID == InnerHMIID && m.HMIID != hmiidi).FirstOrDefault();
                        //if (InnerHMIDupData != null)
                        //{
                        //    if (InnerHMIDupData.isWorkInProgress == 2)
                        //    {
                        //        int InnerMacID = Convert.ToInt32(InnerHMIDupData.MachineID);
                        //        var MacDispName = Convert.ToString(dbsimilar.tblmachinedetails.Where(m => m.MachineID == InnerMacID).Select(m => m.MachineDispName).FirstOrDefault());
                        //        Session["Error"] = " Same WorkOrder in Machine: " + MacDispName + " , So you cannot JobFinish ";
                        //        return RedirectToAction("Index");
                        //    }
                        //}
                    }
                    #endregion

                }
            }

            //2017-01-09
            //3) 
            //I think you have to rearrange these HMIID's based on wono,partno,opno ascending.
            //Because they may select wo's 
            //var ddlDummy = db.tblddls.Where(m=> HMIIDList.Contains(m.))
            foreach (string hmiid in HMIIDArray)
            {
                int hmiidi = Convert.ToInt32(hmiid);
                if (hmiidi != ExceptionHMIID)
                {
                    //var HMIData = db.tbllivehmiscreens.Where(m => m.HMIID == hmiidi).FirstOrDefault();
                    tbllivehmiscreen HMIData = obj.GetLiveHMIDetails6(hmiidi);
                    int DelivQty = 0;
                    int TargQty = 0;
                    int ProcessedQty = 0;

                    int.TryParse(HMIData.Target_Qty.ToString(), out TargQty);
                    int.TryParse(HMIData.Delivered_Qty.ToString(), out DelivQty);
                    int.TryParse(HMIData.ProcessQty.ToString(), out ProcessedQty);

                    if (TargQty == (DelivQty + ProcessedQty))
                    {
                        string wono = HMIData.Work_Order_No;
                        string partno = HMIData.PartNo;
                        string opno = HMIData.OperationNo;
                        int opnoInt = Convert.ToInt32(HMIData.OperationNo);

                        //2017-01-19
                        //var ddldata1 = db.tblddls.Where(m => m.WorkOrder == wono && m.MaterialDesc == partno).FirstOrDefault();
                        //if (ddldata1 != null)
                        //{
                        //    int opLocal = 0;
                        //    int.TryParse(Convert.ToString(ddldata1.OperationNo), out opLocal);
                        //    if (opLocal < opnoInt)
                        //    {
                        //        if (ddldata1.IsCompleted == 0)
                        //        {
                        //            Session["VError"] = "First, JobFinish WONo :" + ddldata1.WorkOrder;
                        //            return RedirectToAction("Index");
                        //        }
                        //    }
                        //}

                        HMIData.Status = 2;
                        HMIData.isWorkInProgress = 1;
                        tblddl ddldata = obj.GetddlDetails3(wono, partno, opno);
                        //var ddldata = db.tblddls.Where(m => m.WorkOrder == wono && m.MaterialDesc == partno && m.OperationNo == opno).FirstOrDefault();
                        if (ddldata != null)
                        {
                            int ddlID = ddldata.DDLID;
                            obj.UpdatetblddlDetails2(ddlID);
                            //ddldata.IsCompleted = 1;
                            //db.Entry(ddldata).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                    else if (TargQty > (DelivQty + ProcessedQty))
                    {
                        HMIData.Status = 1;
                        HMIData.isWorkInProgress = 0;
                    }
                    else
                    {
                        Session["VError"] = "Delivered + Processed Cannot be Greater than Target for :" + HMIData.Work_Order_No;
                        return RedirectToAction("Index");
                    }
                    HMIData.Time = DateTime.Now;
                    int HMIID = HMIData.HMIID;
                    obj.UpdateLiveHMIScreenDetailsD1(HMIData.Time, Convert.ToInt32(HMIData.Status), HMIData.isWorkInProgress, HMIID);
                    //db.Entry(HMIData).State = System.Data.Entity.EntityState.Modified;
                    //db.SaveChanges();
                }
                Session["isWorkOrder"] = 0;
                Session["Started"] = 0;
                //Session["partialclick"] = 1;
                if (Convert.ToInt32(Session["isWorkOrder"]) == 0)
                {
                    obj.UpdateIsworkorder(hmiidi);
                }
            }

            //var HMIDataAll = from r in  ( from row in db.tblhmiscreens
            //                    where HMIIDList.Contains( row.HMIID.ToString() ) 
            //                    orderby row.Work_Order_No, row.PartNo , row.OperationNo
            //                    select row).ToList() );


            return RedirectToAction("Index");
        }
        public ActionResult OverAllFinish(string HMIIDs)
        {
            Session["split"] = null;
            Session["Mode"] = null;
            Session["ddl"] = null;
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);


            //1) extract hmiids from string
            //2) Check if eligible for JobFinish (Eligible only if all are eligible)
            //3) Now Job Finish all of them

            int machineID = Convert.ToInt32(Session["MachineID"]);
            //if (IDLEorGenericWorkisON())
            //{
            //    Session["Error"] = "Please End IDLE/GenericWork Before Selecting New Work Orders";
            //    return RedirectToAction("Index");
            //}

            //1)
            string[] HMIIDArray = HMIIDs.Split(',');

            //2)
            int ExceptionHMIID = 0;
            foreach (string hmiid in HMIIDArray)
            {
                int hmiid1 = Convert.ToInt32(hmiid);
                //var HMIData = db.tbllivehmiscreens.Where(m => m.HMIID == hmiid1).FirstOrDefault();
                tbllivehmiscreen HMIData = obj.GetLiveHMIDetails6(hmiid1);
                if (HMIData.SplitWO == "Yes")
                {
                    Session["VError"] = "while doing Job Finish, You can not able to split the Work Order ";
                    Session["split"] = 1;
                    return RedirectToAction("Index");
                }
                else { }
                //if (HMIData.isWorkInProgress == 3)
                if (HMIData.IsHold == 1)
                {
                    Session["VError"] = "End HOLD before Clicking Job Finish";
                    return RedirectToAction("Index");
                }
                else if (HMIData.Date != null)
                {
                    int deliveredQty = 0, processQty = 0;
                    int.TryParse(Convert.ToString(HMIData.ProcessQty), out processQty);
                    if (int.TryParse(Convert.ToString(HMIData.Delivered_Qty), out deliveredQty))
                    {
                        if ((deliveredQty + processQty) == Convert.ToInt32(HMIData.Target_Qty))
                        {
                        }
                        else
                        {
                            Session["VError"] = "DeliveredQty Must be equal to Target Qty";
                            return RedirectToAction("Index");
                        }
                    }
                    else
                    {
                        Session["VError"] = "Enter Delivered Quantity";
                        return RedirectToAction("Index");
                    }
                }
                //else if (HMIData.Date == null && HMIData.Work_Order_No == null && HMIData.PartNo == null && HMIData.OperationNo == null)
                else if (HMIData.Date == null)
                {// Do Nothing. Just to Skip our Extra Empty Row
                    if (ExceptionHMIID == 0)
                    {
                        ExceptionHMIID = Convert.ToInt32(HMIData.HMIID);
                    }
                    else
                    {
                        Session["VError"] = "Please enter all Details Before StartAll is Clicked.";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    Session["VError"] = "Please Start All WorkOrders Before JobFinish";
                    return RedirectToAction("Index");
                }
            }

            //Check if its  Empty Screen and JobFinish is clicked.
            if (HMIIDArray.Length == 1)
            {
                foreach (string hmiid in HMIIDArray)
                {
                    int hmiidi = Convert.ToInt32(hmiid);
                    if (hmiidi == ExceptionHMIID)
                    {
                        Session["VError"] = "There are no WorkOrder to Finish";
                        return RedirectToAction("Index");
                    }
                }
            }


            //3)
            //foreach (var hmiid in HMIIDArray)
            //{
            //    int hmiid1 = Convert.ToInt32(hmiid);
            //    if (hmiid1 != ExceptionHMIID)
            //    {
            //        var HMIData = db.tblhmiscreens.Where(m => m.HMIID == hmiid1).FirstOrDefault();

            //        string wono = HMIData.Work_Order_No;
            //        string partno = HMIData.PartNo;
            //        string opno = HMIData.OperationNo;
            //        var ddldata = db.tblddls.Where(m => m.WorkOrder == wono && m.MaterialDesc == partno && m.OperationNo == opno).FirstOrDefault();
            //        if (ddldata != null)
            //        {
            //            ddldata.IsCompleted = 1;
            //            db.Entry(ddldata).State = System.Data.Entity.EntityState.Modified;
            //        }

            //        HMIData.Status = 2;
            //        HMIData.isWorkInProgress = 1;
            //        HMIData.Time = DateTime.Now;
            //        db.Entry(HMIData).State = System.Data.Entity.EntityState.Modified;
            //        db.SaveChanges();
            //    }
            //}

            //Gen , Seperated String from HMIIDArray
            string hmiids = null;
            for (int hmiid = 0; hmiid < HMIIDArray.Length; hmiid++)
            {
                if (hmiids == null)
                {
                    string localhmiidString = HMIIDArray[hmiid];
                    if (ExceptionHMIID.ToString() != localhmiidString)
                    {
                        hmiids = HMIIDArray[hmiid].ToString();
                    }
                }
                else
                {
                    if (ExceptionHMIID.ToString() != HMIIDArray[hmiid])
                    {
                        hmiids += "," + HMIIDArray[hmiid].ToString();
                    }
                }
            }

            //Get hmiids (as comma seperated string) in ascending order based on wono,partno,Opno.
            hmiids = GetOrderedHMIIDs(hmiids);


            // this and one below , both were there so 2017-02-03 //string WIPQueryOuter = @"SELECT * from tblhmiscreen where HMIID IN ( ' " + HMIIDArray + " ' ) order by  Work_Order_No,PartNo,OperationNo ; ";
            //string WIPQueryOuter = @"SELECT * from tbllivehmiscreen where HMIID IN ( " + hmiids + " ) order by  Work_Order_No,PartNo,OperationNo ; ";
            //var WIPOuter = db.tbllivehmiscreens.SqlQuery(WIPQueryOuter).ToList();
            List<tbllivehmiscreen> WIPOuter = obj.GetHMIList5(hmiids);
            foreach (tbllivehmiscreen rowOuter in WIPOuter)
            {
                //using (i_facility_tsalEntities dbsimilar = new i_facility_tsalEntities())
                //{
                int HMIId = rowOuter.HMIID;
                #region If its as SingleWO
                tbllivehmiscreen SimilarWOData = obj.GettbllivehmiscreensDet1(rowOuter.Work_Order_No, rowOuter.PartNo, rowOuter.OperationNo, machineID);
                //var SimilarWOData = dbsimilar.tbllivehmiscreens.Where(m => m.Work_Order_No == rowOuter.Work_Order_No && m.OperationNo == rowOuter.OperationNo && m.PartNo == rowOuter.PartNo && m.MachineID != machineID && m.isWorkInProgress == 2).FirstOrDefault();
                if (SimilarWOData != null)
                {
                    int InnerMacID = Convert.ToInt32(obj.GetLiveHMIDetails10(SimilarWOData.HMIID));
                    // int InnerMacID = Convert.ToInt32(dbsimilar.tbllivehmiscreens.Where(m => m.HMIID == SimilarWOData.HMIID).Select(m => m.MachineID).FirstOrDefault());
                    //var MacDispName = Convert.ToString(dbsimilar.tblmachinedetails.Where(m => m.MachineID == InnerMacID).Select(m => m.MachineDispName).FirstOrDefault());
                    string MacDispName = obj.GetMachineDetails1(InnerMacID);

                    Session["Error"] = " This WorkOrder:'" + rowOuter.Work_Order_No + "' and OperationNo:'" + rowOuter.OperationNo + "' and PartNo:'" + rowOuter.PartNo + "' is already in Machine: " + MacDispName + " , So you cannot JobFinish ";
                    return RedirectToAction("Index");
                }
                #endregion

                #region If its as MultiWO
                //var SimilarWODataMulti = dbsimilar.tbllivemultiwoselections.Where(m => m.WorkOrder == rowOuter.Work_Order_No && m.OperationNo == rowOuter.OperationNo && m.PartNo == rowOuter.PartNo && m.HMIID != HMIId && m.tbllivehmiscreen.isWorkInProgress == 2).FirstOrDefault();
                tbllivemultiwoselection SimilarWODataMulti = obj.GetMultiWODetails4(rowOuter.Work_Order_No, rowOuter.PartNo, rowOuter.OperationNo, HMIId);

                if (SimilarWODataMulti != null)
                {
                    int InnerHMIID = (int)SimilarWODataMulti.HMIID;
                    int InnerMacID = Convert.ToInt32(obj.GetLiveHMIDetails10(InnerHMIID));
                    // int InnerMacID = Convert.ToInt32(dbsimilar.tbllivehmiscreens.Where(m => m.HMIID == InnerHMIID).Select(m => m.MachineID).FirstOrDefault());
                    //var MacDispName = Convert.ToString(dbsimilar.tblmachinedetails.Where(m => m.MachineID == InnerMacID).Select(m => m.MachineDispName).FirstOrDefault());
                    string MacDispName = obj.GetMachineDetails1(InnerMacID);

                    Session["Error"] = " Same WorkOrder is already in Machine: " + MacDispName + " , So you cannot JobFinish ";
                    return RedirectToAction("Index");
                }
                #endregion
                //}
            }

            #region 2017-07-01
            foreach (tbllivehmiscreen rowOuter in WIPOuter)
            {
                int hmiid1 = Convert.ToInt32(rowOuter.HMIID);
                string woNo = Convert.ToString(rowOuter.Work_Order_No);
                string opNo = Convert.ToString(rowOuter.OperationNo);
                string partNo = Convert.ToString(rowOuter.PartNo);

                //Logic to check sequence of JF Based on WONo, PartNo and OpNo.
                int OperationNoInt = Convert.ToInt32(opNo);

                //string WIPQuery1 = @"SELECT * from tblddl where WorkOrder = '" + woNo + "' and MaterialDesc = '" + partNo + "'  and OperationNo != '" + opNo + "' and IsCompleted = 0 order by OperationNo ";
                //var WIP1 = dbHMI.tblddls.SqlQuery(WIPQuery1).ToList();
                List<tblddl> WIP1 = obj.GettblddlList1(woNo, partNo, opNo);
                foreach (tblddl row in WIP1)
                {
                    int InnerOpNo = Convert.ToInt32(row.OperationNo);
                    string ddlopno = row.OperationNo;
                    string InnerOpNoString = Convert.ToString(row.OperationNo);
                    if (hmiids.Contains(InnerOpNoString))
                    { }
                    else
                    {
                        if (OperationNoInt > InnerOpNo)
                        {
                            int PrvProcessQty = 0, PrvDeliveredQty = 0, TotalProcessQty = 0, ishold = 0;
                            #region new code
                            //here 1st get latest of delivered and processed among row in tblHMIScreen & tblmulitwoselection
                            int isHMIFirst = 2; //default NO History for that wo,pn,on

                            //var mulitwoData = db.tbllivemultiwoselections.Where(m => m.WorkOrder == woNo && m.PartNo == partNo && m.OperationNo == ddlopno && m.tbllivehmiscreen.isWorkInProgress != 2).OrderByDescending(m => m.tbllivehmiscreen.Time).Take(1).ToList();
                            List<tbl_multiwoselection> mulitwoData = obj.GetMultiWorkOrderDetails(woNo, partNo, ddlopno);
                            //var hmiData = db.tbllivehmiscreens.Where(m => m.Work_Order_No == woNo && m.PartNo == partNo && m.OperationNo == ddlopno && m.isWorkInProgress != 2).OrderByDescending(m => m.Time).Take(1).ToList();
                            List<tbllivehmiscreen> hmiData = obj.GetLiveHMIDetails11(woNo, partNo, opNo);

                            if (hmiData.Count > 0 && mulitwoData.Count > 0) // now check for greatest amongst
                            {
                                DateTime multiwoDateTime = Convert.ToDateTime(mulitwoData[0].tblhmiscreen.Time);
                                DateTime hmiDateTime = Convert.ToDateTime(hmiData[0].Time);

                                if (Convert.ToInt32(multiwoDateTime.Subtract(hmiDateTime).TotalSeconds) > 0)
                                {
                                    isHMIFirst = 1;
                                }
                                else
                                {
                                    isHMIFirst = 0;
                                }

                            }
                            else if (mulitwoData.Count > 0)
                            {
                                isHMIFirst = 1;
                            }
                            else if (hmiData.Count > 0)
                            {
                                isHMIFirst = 0;
                            }

                            if (isHMIFirst == 1)
                            {
                                string delivString = Convert.ToString(mulitwoData[0].DeliveredQty);
                                int delivInt = 0;
                                int.TryParse(delivString, out delivInt);

                                string processString = Convert.ToString(mulitwoData[0].ProcessQty);
                                int procInt = 0;
                                int.TryParse(processString, out procInt);

                                PrvProcessQty += procInt;
                                PrvDeliveredQty += delivInt;

                                ishold = mulitwoData[0].tblhmiscreen.IsHold;
                                ishold = ishold == 2 ? 0 : ishold;

                            }
                            else if (isHMIFirst == 0)
                            {
                                string delivString = Convert.ToString(hmiData[0].Delivered_Qty);
                                int delivInt = 0;
                                int.TryParse(delivString, out delivInt);

                                string processString = Convert.ToString(hmiData[0].ProcessQty);
                                int procInt = 0;
                                int.TryParse(processString, out procInt);

                                PrvProcessQty += procInt;
                                PrvDeliveredQty += delivInt;

                                ishold = hmiData[0].IsHold;
                                ishold = ishold == 2 ? 0 : ishold;
                            }
                            else
                            {
                                //no previous delivered or processed qty so Do Nothing.
                            }
                            #endregion
                            TotalProcessQty = PrvProcessQty + PrvDeliveredQty;
                            //var hmiPFed = db.tblhmiscreens.Where(m => m.Work_Order_No == woNo && m.PartNo == partNo && m.OperationNo == opNo).OrderByDescending(m => m.Time).FirstOrDefault();

                            if (Convert.ToInt32(row.TargetQty) == TotalProcessQty)
                            {
                                #region
                                if (isHMIFirst == 1 && Convert.ToInt32(row.TargetQty) < Convert.ToInt32(mulitwoData[0].TargetQty))
                                {
                                    int hmiidmultitbl = Convert.ToInt32(mulitwoData[0].HMIID);
                                    //var hmiTomulittblData = db.tbllivehmiscreens.Find(hmiidmultitbl);
                                    tbllivehmiscreen hmiTomulittblData = obj.GetLiveHMIDetails6(hmiidmultitbl);
                                    if (hmiTomulittblData != null)
                                    {
                                        //tbllivehmiscreen tblh = new tbllivehmiscreen();

                                        //tblh.CorrectedDate = row.CorrectedDate;
                                        //tblh.Date = DateTime.Now;
                                        //tblh.Time = DateTime.Now;
                                        //tblh.PEStartTime = DateTime.Now;
                                        //tblh.DDLWokrCentre = hmiTomulittblData.DDLWokrCentre;
                                        //tblh.Delivered_Qty = 0;
                                        //tblh.DoneWithRow = 1;
                                        //tblh.IsHold = 0;
                                        //tblh.IsMultiWO = 0;
                                        //tblh.isUpdate = 1;
                                        //tblh.isWorkInProgress = 1;
                                        //tblh.isWorkOrder = hmiTomulittblData.isWorkOrder;
                                        //tblh.MachineID = hmiTomulittblData.MachineID;
                                        //tblh.OperationNo = mulitwoData[0].OperationNo;
                                        //tblh.OperatiorID = hmiTomulittblData.OperatiorID;
                                        //tblh.OperatorDet = hmiTomulittblData.OperatorDet;
                                        //tblh.PartNo = mulitwoData[0].PartNo;
                                        //tblh.ProcessQty = TotalProcessQty;
                                        //tblh.Prod_FAI = hmiTomulittblData.Prod_FAI;
                                        //tblh.Project = hmiTomulittblData.Project;
                                        //tblh.Rej_Qty = hmiTomulittblData.Rej_Qty;
                                        //tblh.Shift = hmiTomulittblData.Shift;
                                        //tblh.SplitWO = "No";
                                        //tblh.Status = hmiTomulittblData.Status;
                                        //tblh.Target_Qty = TotalProcessQty;
                                        //tblh.Work_Order_No = mulitwoData[0].WorkOrder;
                                        ////tblh.HMIID = ;
                                        //db.tbllivehmiscreens.Add(tblh);
                                        //db.SaveChanges();
                                        obj.InsertLiveHMIDetails1(hmiTomulittblData.MachineID, hmiTomulittblData.OperatiorID, hmiTomulittblData.Shift, DateTime.Now, DateTime.Now, hmiTomulittblData.Project, mulitwoData[0].PartNo, mulitwoData[0].OperationNo, hmiTomulittblData.Rej_Qty, mulitwoData[0].WorkOrder, TotalProcessQty, hmiTomulittblData.Status, row.CorrectedDate, hmiTomulittblData.Prod_FAI, hmiTomulittblData.isWorkOrder, hmiTomulittblData.OperatorDet, DateTime.Now, TotalProcessQty, hmiTomulittblData.DDLWokrCentre);
                                    }

                                }
                                else if (isHMIFirst == 0 && Convert.ToInt32(row.TargetQty) < Convert.ToInt32(hmiData[0].Target_Qty))
                                {
                                    obj.InsertLiveHMIDetails1(hmiData[0].MachineID, hmiData[0].OperatiorID, hmiData[0].Shift, DateTime.Now, DateTime.Now, hmiData[0].Project, hmiData[0].PartNo, hmiData[0].OperationNo, hmiData[0].Rej_Qty, hmiData[0].Work_Order_No, TotalProcessQty, hmiData[0].Status, row.CorrectedDate, hmiData[0].Prod_FAI, hmiData[0].isWorkOrder, hmiData[0].OperatorDet, DateTime.Now, TotalProcessQty, hmiData[0].DDLWokrCentre);


                                    //tbllivehmiscreen tblh = new tbllivehmiscreen();

                                    //tblh.CorrectedDate = row.CorrectedDate;
                                    //tblh.Date = DateTime.Now;
                                    //tblh.Time = DateTime.Now;
                                    //tblh.PEStartTime = DateTime.Now;
                                    //tblh.DDLWokrCentre = hmiData[0].DDLWokrCentre;
                                    //tblh.Delivered_Qty = 0;
                                    //tblh.DoneWithRow = 1;
                                    //tblh.IsHold = 0;
                                    //tblh.IsMultiWO = hmiData[0].IsMultiWO;
                                    //tblh.isUpdate = 1;
                                    //tblh.isWorkInProgress = 1;
                                    //tblh.isWorkOrder = hmiData[0].isWorkOrder;
                                    //tblh.MachineID = hmiData[0].MachineID;
                                    //tblh.OperationNo = hmiData[0].OperationNo;
                                    //tblh.OperatiorID = hmiData[0].OperatiorID;
                                    //tblh.OperatorDet = hmiData[0].OperatorDet;
                                    //tblh.PartNo = hmiData[0].PartNo;
                                    //tblh.ProcessQty = TotalProcessQty;
                                    //tblh.Prod_FAI = hmiData[0].Prod_FAI;
                                    //tblh.Project = hmiData[0].Project;
                                    //tblh.Rej_Qty = hmiData[0].Rej_Qty;
                                    //tblh.Shift = hmiData[0].Shift;
                                    //tblh.SplitWO = hmiData[0].SplitWO;
                                    //tblh.Status = hmiData[0].Status;
                                    //tblh.Target_Qty = TotalProcessQty;
                                    //tblh.Work_Order_No = hmiData[0].Work_Order_No;
                                    ////tblh.HMIID = ;
                                    //db.tbllivehmiscreens.Add(tblh);
                                    //db.SaveChanges();
                                }
                                #endregion
                                int ddlID = row.DDLID;
                                obj.UpdatetblddlDetails1(ddlID);
                                //row.IsCompleted = 1;
                                //db.Entry(row).State = System.Data.Entity.EntityState.Modified;
                                //db.SaveChanges();
                            }
                        }
                    }
                }
            }
            #endregion

            foreach (tbllivehmiscreen rowOuter in WIPOuter)
            {
                int hmiid1 = Convert.ToInt32(rowOuter.HMIID);
                #region 2017-02-07

                string woNo = Convert.ToString(rowOuter.Work_Order_No);
                string opNo = Convert.ToString(rowOuter.OperationNo);
                string partNo = Convert.ToString(rowOuter.PartNo);

                //Logic to check sequence of JF Based on WONo, PartNo and OpNo.
                int OperationNoInt = Convert.ToInt32(opNo);

                //using (i_facility.Models.i_facility_tsalEntities dbHMI = new i_facility.Models.i_facility_tsalEntities())
                //{

                //Commmented on 2017-06-01 . Just the DDL List will do.

                ////This may cause the problem due to multiple times PartialFinish not if use the new query 2017-02-03 . Helps with Manual Entry.
                ////string WIPQuery = @"SELECT * from tblhmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo != '" + opNo + "' order by OperationNo desc ";
                //string WIPQuery = @"SELECT * from tblhmiscreen where  HMIID IN ( SELECT Max(HMIID) from tblhmiscreen where  HMIID IN  ( SELECT HMIID from tblhmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo != '" + opNo + "' and IsMultiWO = 0 and DDLWokrCentre is null order by HMIID desc ) group by Work_Order_No,PartNo,OperationNo  ) order by OperationNo;";
                //var WIP = dbHMI.tblhmiscreens.SqlQuery(WIPQuery).ToList();
                //bool IsItWrong = false;
                //foreach (var row in WIP)
                //{
                //    int InnerOpNo = Convert.ToInt32(row.OperationNo);
                //    if (OperationNoInt > InnerOpNo)
                //    {
                //        if (row.isWorkInProgress != 1) //=> lower OpNo is not Finished.
                //        {
                //            Session["VError"] = " Finish WONo: " + row.Work_Order_No + " and PartNo: " + row.PartNo + " and OperationNo: " + InnerOpNo;
                //            //return RedirectToAction("Index");
                //        }
                //    }
                //}

                //?2017-01-19

                //if (rowOuter.IsMultiWO == 0) //Its a single WO
                //{

                //if its  finished in MultiWorkOrder then we can catch it here. (Above code can help with Manual Entry)
                //string WIPQuery1 = @"SELECT * from tblddl where WorkOrder = '" + woNo + "' and MaterialDesc = '" + partNo + "'  and OperationNo != '" + opNo + "' and IsCompleted = 0 order by OperationNo ";
                //var WIP1 = dbHMI.tblddls.SqlQuery(WIPQuery1).ToList();
                List<tblddl> WIP1 = obj.Getddl1Details(woNo, partNo, opNo);
                foreach (tblddl row in WIP1)
                {
                    int InnerOpNo = Convert.ToInt32(row.OperationNo);
                    string InnerOpNoString = Convert.ToString(row.OperationNo);
                    if (hmiids.Contains(InnerOpNoString))
                    { }
                    else
                    {
                        if (OperationNoInt > InnerOpNo)
                        {
                            if (row.IsCompleted != 1) //=> lower OpNo is not Finished.
                            {
                                string outop = "";
                                //bool retStatus = CheckWhetherWoStartedOrNot(woNo, opNo, partNo);
                                bool retStatus = CheckAlltheWOForFinish(woNo, opNo, partNo, out outop);
                                if (!retStatus)
                                {
                                    Session["VError"] = " Finish WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + outop;
                                    return RedirectToAction("Index");
                                }
                                else { }
                            }
                            else
                            {
                                Session["VError"] = null;
                            }
                        }
                    }
                }

                //}
                //there are no MultiWOs in Manual HMI Screen. // 2017-06-01 But they may be , because of Normal HMIScreen's MultiWO type

                //else  //Its a Multi-WorkOrder
                //{
                //    var MultiWorkOrderData = db.tbl_multiwoselection.Where(m => m.HMIID == hmiid1).FirstOrDefault();
                //    if (MultiWorkOrderData != null)
                //    {
                //        string multiWO = MultiWorkOrderData.WorkOrder;
                //        string multiPart = MultiWorkOrderData.PartNo;
                //        string multiOP = MultiWorkOrderData.OperationNo;

                //        string WIPQuery1 = @"SELECT * from tblddl where WorkOrder = '" + multiWO + "' and MaterialDesc = '" + multiPart + "'  and OperationNo != '" + multiOP + "' order by OperationNo ";
                //        var WIP1 = dbHMI.tblddls.SqlQuery(WIPQuery1).ToList();
                //        foreach (var row in WIP1)
                //        {
                //            int InnerOpNo = Convert.ToInt32(row.OperationNo);
                //            string InnerOpNoString = Convert.ToString(row.OperationNo);
                //            if (hmiids.Contains(InnerOpNoString))
                //            { }
                //            else
                //            {
                //                if (OperationNoInt > InnerOpNo)
                //                {
                //                    if (row.IsCompleted != 1) //=> lower OpNo is not Finished.
                //                    {
                //                        Session["VError"] = " Finish WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
                //                        return RedirectToAction("Index");
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}

                #endregion

                //var HMIData = db.tbllivehmiscreens.Where(m => m.HMIID == hmiid1).FirstOrDefault();
                tbllivehmiscreen HMIData = obj.GetLiveHMIDetails6(hmiid1);
                string wono = HMIData.Work_Order_No;
                string partno = HMIData.PartNo;
                string opno = HMIData.OperationNo;
                //var ddldata = db.tblddls.Where(m => m.WorkOrder == wono && m.MaterialDesc == partno && m.OperationNo == opno).FirstOrDefault();
                tblddl ddldata = obj.GetddlDetails3(wono, partno, opno);
                if (ddldata != null)
                {
                    int ddlID = ddldata.DDLID;
                    obj.UpdatetblddlDetails1(ddlID);
                    //ddldata.IsCompleted = 1;
                    //db.Entry(ddldata).State = System.Data.Entity.EntityState.Modified;
                }

                if (HMIData.isWorkOrder == 1)
                {
                    obj.UpdateIsworkorder(hmiid1);
                }
                //HMIData.Status = 2;
                //HMIData.isWorkInProgress = 1;
                //HMIData.SplitWO = "No";
                int hmiid = HMIData.HMIID;
                DateTime Time = DateTime.Now;
                obj.UpdateLossofentriesDetails1(hmiid, Time);
                //db.Entry(HMIData).State = System.Data.Entity.EntityState.Modified;
                //db.SaveChanges();
                // }
            }
            Session["isWorkOrder"] = 0;
            Session["Started"] = 0;
            return RedirectToAction("Index");
        }

        //Get Operator ID's based on prefix
        //public string OperatorDetails(string Operatorid)
        //{
        //    string res = "";

        //    using (SRKSDemo.Server_Model.i_facility_shaktiEntities1 db = new SRKSDemo.Server_Model.i_facility_shaktiEntities1)
        //    {
        //        List<string> OPDetList = new List<string>();
        //        OPDetList = db.tbloperatordetails.Where(m => m.isDeleted == 0 && m.OperatorID.StartsWith(Operatorid)).Select(m => m.OperatorID).ToList();
        //        res = JsonConvert.SerializeObject(OPDetList);
        //    }
        //    return res;
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(tbllivehmiscreen tblhmiscreen)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);
            if (ModelState.IsValid)
            {
                obj.InsertLiveHMIScreenDetails(tblhmiscreen.MachineID, tblhmiscreen.OperatiorID, tblhmiscreen.Shift, Convert.ToDateTime(tblhmiscreen.Date), Convert.ToDateTime(tblhmiscreen.Time), tblhmiscreen.Project, tblhmiscreen.PartNo, tblhmiscreen.OperationNo, Convert.ToInt32(tblhmiscreen.Rej_Qty), tblhmiscreen.Work_Order_No, Convert.ToInt32(tblhmiscreen.Target_Qty), Convert.ToInt32(tblhmiscreen.Delivered_Qty), Convert.ToInt32(tblhmiscreen.Status), tblhmiscreen.CorrectedDate, tblhmiscreen.Prod_FAI, tblhmiscreen.isUpdate, tblhmiscreen.DoneWithRow, tblhmiscreen.isWorkInProgress, tblhmiscreen.isWorkOrder, tblhmiscreen.OperatorDet, Convert.ToDateTime(tblhmiscreen.PEStartTime), tblhmiscreen.ProcessQty, tblhmiscreen.DDLWokrCentre, tblhmiscreen.IsMultiWO, tblhmiscreen.IsHold, tblhmiscreen.SplitWO, Convert.ToInt32(tblhmiscreen.batchNo));
                //db.tbllivehmiscreens.Add(tblhmiscreen);
                //db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tblhmiscreen);
        }

        //Control comes here when jobfinish is clicked.
        public ActionResult Edit(int id = 0, int reworkorderhidden = 0, int cjtextbox9 = 0, int cjtextbox8 = 0)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            int reworkyes = Convert.ToInt32(reworkorderhidden);
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            _conn = new ConnectionFactory();
            obj = new Dao(_conn);
            //Getting Shift Value
            DateTime Time = DateTime.Now;
            TimeSpan Tm = new TimeSpan(Time.Hour, Time.Minute, Time.Second);
            //var ShiftDetails = db.tblshift_mstr.Where(m => m.StartTime <= Tm && m.EndTime >= Tm);
            tblshift_mstr ShiftDetails = obj.GetShiftDetails(Tm);
            string Shift = null;
            if (ShiftDetails != null)
            {
                Shift = ShiftDetails.ShiftName;
            }
            ViewBag.date = System.DateTime.Now;
            if (Shift != null)
            {
                ViewBag.shift = Shift;
            }
            else
            {
                ViewBag.shift = "C";
            }

            int machineID = 0;
            //tbllivehmiscreen tblhmiscreen = db.tbllivehmiscreens.Find(id);
            tbllivehmiscreen tblhmiscreen = obj.GetLiveHMIDetails6(id);
            machineID = Convert.ToInt32(tblhmiscreen.MachineID);

            int Uid = tblhmiscreen.OperatiorID;

            int ID = id;
            //tbllivehmiscreen OldWork = db.tbllivehmiscreens.Find(ID);
            tbllivehmiscreen OldWork = obj.GetLiveHMIDetails6(id);
            OldWork.Status = 2;
            OldWork.ProcessQty = cjtextbox9;
            OldWork.Delivered_Qty = cjtextbox8;
            OldWork.Time = DateTime.Now;
            //update isWorkInProgress When WorkIs finished is clicked.

            //SplitWO
            OldWork.SplitWO = "No";

            OldWork.isWorkInProgress = 1;//job finished

            if (reworkorderhidden == 1)
            {
                OldWork.isWorkOrder = 1;
            }

            string Shiftgen = OldWork.Shift;
            string operatorName = OldWork.OperatorDet;
            int Opgid = OldWork.OperatiorID;

            List<string> MacHierarchy = GetHierarchyData(machineID);
            int IsWOMultiWO = OldWork.IsMultiWO;
            int HMIId = OldWork.HMIID;
            if (IsWOMultiWO == 0)
            {
                string woNo = Convert.ToString(OldWork.Work_Order_No);
                string opNo = Convert.ToString(OldWork.OperationNo);
                string partNo = Convert.ToString(OldWork.PartNo);
                int OperationNoInt = Convert.ToInt32(opNo);

                #region 2017-07-01

                //string WIPQuery6 = @"SELECT * from tblddl where WorkOrder = '" + woNo + "' and MaterialDesc = '" + partNo + "' and OperationNo != '" + opNo + "'  and IsCompleted = 0 order by WorkOrder,MaterialDesc,OperationNo ";
                //var WIPDDL6 = db.tblddls.SqlQuery(WIPQuery6).ToList();
                List<tblddl> WIPDDL6 = obj.GetddlDetails(woNo, partNo, opNo);
                foreach (tblddl row in WIPDDL6)
                {
                    int InnerOpNo = Convert.ToInt32(row.OperationNo);
                    string ddlopno = row.OperationNo;
                    if (InnerOpNo < OperationNoInt)
                    {
                        int PrvProcessQty = 0, PrvDeliveredQty = 0, TotalProcessQty = 0, ishold = 0;
                        #region new code
                        //here 1st get latest of delivered and processed among row in tblHMIScreen & tblmulitwoselection
                        int isHMIFirst = 2; //default NO History for that wo,pn,on

                        // Modified by Ashok
                        //var mulitwoData = db.tbllivemultiwoselections.Where(m => m.WorkOrder == woNo && m.PartNo == partNo && m.OperationNo == ddlopno && m.tbllivehmiscreen.isWorkInProgress != 2).OrderByDescending(m => m.tbllivehmiscreen.Time).Take(1).ToList();
                        List<tbllivemultiwoselection> mulitwoData = obj.GetMultiWODetails2(woNo, partNo, ddlopno);

                        //var hmiData = db.tbllivehmiscreens.Where(m => m.Work_Order_No == woNo && m.PartNo == partNo && m.OperationNo == ddlopno && m.isWorkInProgress != 2).OrderByDescending(m => m.Time).Take(1).ToList();

                        List<tbllivehmiscreen> hmiData = obj.GetLiveHMIDetails11(woNo, partNo, ddlopno);

                        if (hmiData.Count > 0 && mulitwoData.Count > 0) // now check for greatest amongst
                        {
                            DateTime multiwoDateTime = Convert.ToDateTime(mulitwoData[0].tbllivehmiscreen.Time);
                            DateTime hmiDateTime = Convert.ToDateTime(hmiData[0].Time);

                            if (Convert.ToInt32(multiwoDateTime.Subtract(hmiDateTime).TotalSeconds) > 0)
                            {
                                isHMIFirst = 1;
                            }
                            else
                            {
                                isHMIFirst = 0;
                            }

                        }
                        else if (mulitwoData.Count > 0)
                        {
                            isHMIFirst = 1;
                        }
                        else if (hmiData.Count > 0)
                        {
                            isHMIFirst = 0;
                        }

                        if (isHMIFirst == 1)
                        {
                            string delivString = Convert.ToString(mulitwoData[0].DeliveredQty);
                            int delivInt = 0;
                            int.TryParse(delivString, out delivInt);

                            string processString = Convert.ToString(mulitwoData[0].ProcessQty);
                            int procInt = 0;
                            int.TryParse(processString, out procInt);

                            PrvProcessQty += procInt;
                            PrvDeliveredQty += delivInt;

                            ishold = mulitwoData[0].tbllivehmiscreen.IsHold;
                            ishold = ishold == 2 ? 0 : ishold;

                        }
                        else if (isHMIFirst == 0)
                        {
                            string delivString = Convert.ToString(hmiData[0].Delivered_Qty);
                            int delivInt = 0;
                            int.TryParse(delivString, out delivInt);

                            string processString = Convert.ToString(hmiData[0].ProcessQty);
                            int procInt = 0;
                            int.TryParse(processString, out procInt);

                            PrvProcessQty += procInt;
                            PrvDeliveredQty += delivInt;

                            ishold = hmiData[0].IsHold;
                            ishold = ishold == 2 ? 0 : ishold;
                        }
                        else
                        {
                            //no previous delivered or processed qty so Do Nothing.
                        }
                        #endregion
                        TotalProcessQty = PrvProcessQty + PrvDeliveredQty;
                        //var hmiPFed = db.tblhmiscreens.Where(m => m.Work_Order_No == woNo && m.PartNo == partNo && m.OperationNo == opNo).OrderByDescending(m => m.Time).FirstOrDefault();

                        if (Convert.ToInt32(row.TargetQty) == TotalProcessQty)
                        {
                            #region
                            if (isHMIFirst == 1 && Convert.ToInt32(row.TargetQty) < Convert.ToInt32(mulitwoData[0].TargetQty))
                            {
                                int hmiidmultitbl = Convert.ToInt32(mulitwoData[0].HMIID);
                                //var hmiTomulittblData = db.tbllivehmiscreens.Find(hmiidmultitbl);
                                tbllivehmiscreen hmiTomulittblData = obj.GetLiveHMIDetails6(hmiidmultitbl);
                                if (hmiTomulittblData != null)
                                {
                                    //tbllivehmiscreen tblh = new tbllivehmiscreen();
                                    //tblh.CorrectedDate = row.CorrectedDate;
                                    //tblh.Date = DateTime.Now;
                                    //tblh.Time = DateTime.Now;
                                    //tblh.PEStartTime = DateTime.Now;
                                    //tblh.DDLWokrCentre = hmiTomulittblData.DDLWokrCentre;
                                    //tblh.Delivered_Qty = 0;
                                    //tblh.DoneWithRow = 1;
                                    //tblh.IsHold = 0;
                                    //tblh.IsMultiWO = 0;
                                    //tblh.isUpdate = 1;
                                    //tblh.isWorkInProgress = 1;
                                    //tblh.isWorkOrder = hmiTomulittblData.isWorkOrder;
                                    //tblh.MachineID = hmiTomulittblData.MachineID;
                                    //tblh.OperationNo = mulitwoData[0].OperationNo;
                                    //tblh.OperatiorID = hmiTomulittblData.OperatiorID;
                                    //tblh.OperatorDet = hmiTomulittblData.OperatorDet;
                                    //tblh.PartNo = mulitwoData[0].PartNo;
                                    //tblh.ProcessQty = TotalProcessQty;
                                    //tblh.Prod_FAI = hmiTomulittblData.Prod_FAI;
                                    //tblh.Project = hmiTomulittblData.Project;
                                    //tblh.Rej_Qty = hmiTomulittblData.Rej_Qty;
                                    //tblh.Shift = hmiTomulittblData.Shift;
                                    //tblh.SplitWO = "No";
                                    //tblh.Status = hmiTomulittblData.Status;
                                    //tblh.Target_Qty = TotalProcessQty;
                                    //tblh.Work_Order_No = mulitwoData[0].WorkOrder;

                                    //db.tbllivehmiscreens.Add(tblh);
                                    //db.SaveChanges();
                                    obj.InsertLiveHMIScreenDetails(hmiTomulittblData.MachineID, hmiTomulittblData.OperatiorID, hmiTomulittblData.Shift, DateTime.Now, DateTime.Now, hmiTomulittblData.Project, mulitwoData[0].PartNo, mulitwoData[0].OperationNo, Convert.ToInt32(hmiTomulittblData.Rej_Qty), mulitwoData[0].WorkOrder, TotalProcessQty, 0, Convert.ToInt32(hmiTomulittblData.Status), row.CorrectedDate, hmiTomulittblData.Prod_FAI, 1, 1, 1, hmiTomulittblData.isWorkOrder, hmiTomulittblData.OperatorDet, DateTime.Now, TotalProcessQty, hmiTomulittblData.DDLWokrCentre, 0, 0, "No", 0);
                                }

                            }
                            else if (isHMIFirst == 0 && Convert.ToInt32(row.TargetQty) < Convert.ToInt32(hmiData[0].Target_Qty))
                            {
                                //tbllivehmiscreen tblh = new tbllivehmiscreen();
                                //tblh.CorrectedDate = row.CorrectedDate;
                                //tblh.Date = DateTime.Now;
                                //tblh.Time = DateTime.Now;
                                //tblh.PEStartTime = DateTime.Now;
                                //tblh.DDLWokrCentre = hmiData[0].DDLWokrCentre;
                                //tblh.Delivered_Qty = 0;
                                //tblh.DoneWithRow = 1;
                                //tblh.IsHold = 0;
                                //tblh.IsMultiWO = hmiData[0].IsMultiWO;
                                //tblh.isUpdate = 1;
                                //tblh.isWorkInProgress = 1;
                                //tblh.isWorkOrder = hmiData[0].isWorkOrder;
                                //tblh.MachineID = hmiData[0].MachineID;
                                //tblh.OperationNo = hmiData[0].OperationNo;
                                //tblh.OperatiorID = hmiData[0].OperatiorID;
                                //tblh.OperatorDet = hmiData[0].OperatorDet;
                                //tblh.PartNo = hmiData[0].PartNo;
                                //tblh.ProcessQty = TotalProcessQty;
                                //tblh.Prod_FAI = hmiData[0].Prod_FAI;
                                //tblh.Project = hmiData[0].Project;
                                //tblh.Rej_Qty = hmiData[0].Rej_Qty;
                                //tblh.Shift = hmiData[0].Shift;
                                //tblh.SplitWO = hmiData[0].SplitWO;
                                //tblh.Status = hmiData[0].Status;
                                //tblh.Target_Qty = TotalProcessQty;
                                //tblh.Work_Order_No = hmiData[0].Work_Order_No;

                                //db.tbllivehmiscreens.Add(tblh);
                                //db.SaveChanges();
                                obj.InsertLiveHMIScreenDetails(hmiData[0].MachineID, hmiData[0].OperatiorID, hmiData[0].Shift, DateTime.Now, DateTime.Now, hmiData[0].Project, hmiData[0].PartNo, hmiData[0].OperationNo, Convert.ToInt32(hmiData[0].Rej_Qty), hmiData[0].Work_Order_No, TotalProcessQty, 0, Convert.ToInt32(hmiData[0].Status), row.CorrectedDate, hmiData[0].Prod_FAI, 1, 1, 1, hmiData[0].isWorkOrder, hmiData[0].OperatorDet, DateTime.Now, TotalProcessQty, hmiData[0].DDLWokrCentre, hmiData[0].IsMultiWO, 0, hmiData[0].SplitWO, 0);
                            }
                            #endregion

                            obj.UpdateddlDetails(row.DDLID);

                            //row.IsCompleted = 1;
                            //db.Entry(row).State = System.Data.Entity.EntityState.Modified;
                            //db.SaveChanges();
                        }
                    }
                }

                #endregion

                //OpNo sequence
                #region 2017-02-07
                //New Logic to Overcome WorkOrder Sequence Scenario 2017-02-03
                //string WIPQuery1 = @"SELECT * from tblddl where WorkOrder = '" + woNo + "' and MaterialDesc = '" + partNo + "' and OperationNo != '" + opNo + "'  and IsCompleted = 0 order by WorkOrder,MaterialDesc,OperationNo ";
                //var WIPDDL1 = db.tblddls.SqlQuery(WIPQuery1).ToList();
                List<tblddl> WIPDDL1 = obj.GetddlDetails(woNo, partNo, opNo);
                foreach (tblddl row in WIPDDL1)
                {
                    int InnerOpNo = Convert.ToInt32(row.OperationNo);
                    if (InnerOpNo < OperationNoInt)
                    {
                        if (row.IsCompleted == 0)
                        {
                            Session["Error"] = " Finish WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            Session["Error"] = null;
                        }

                        //bool IsItWrong = false;
                        //string WIPQueryHMI = @"SELECT * from tblhmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + InnerOpNo + "' order by HMIID desc limit 1 ";
                        //var WIP = db.tblhmiscreens.SqlQuery(WIPQueryHMI).ToList();

                        //if (WIP.Count == 0)
                        //{
                        //    Session["VError"] = " Finish WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
                        //    IsItWrong = true;
                        //}
                        //else
                        //{
                        //    foreach (var rowHMI in WIP)
                        //    {
                        //        if (rowHMI.isWorkInProgress != 1) //=> lower OpNo is in HMIScreen & not Finished.
                        //        {
                        //            Session["VError"] = " Finish WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
                        //            //return RedirectToAction("Index");
                        //            IsItWrong = true;
                        //        }
                        //    }
                        //}
                        //if (IsItWrong)
                        //{
                        //    //Strange , it might have been started in Normal WorkCenter as MultiWorkOrder.
                        //    string WIPQueryMultiWO = @"SELECT * from tbl_multiwoselection where WorkOrder = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + InnerOpNo + "' order by MultiWOID desc limit 1 ";
                        //    var WIPMWO = db.tbl_multiwoselection.SqlQuery(WIPQueryMultiWO).ToList();

                        //    if (WIPMWO.Count == 0)
                        //    {
                        //        Session["VError"] = " Finish WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
                        //        return RedirectToAction("Index");
                        //    }

                        //    foreach (var rowHMI in WIPMWO)
                        //    {
                        //        int hmiid = Convert.ToInt32(rowHMI.HMIID);
                        //        var MWOHMIData = db.tblhmiscreens.Where(m => m.HMIID == hmiid).FirstOrDefault();
                        //        if (MWOHMIData != null)
                        //        {
                        //            if (MWOHMIData.isWorkInProgress != 1) //=> lower OpNo is not Finished.
                        //            {
                        //                Session["VError"] = " Finish WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
                        //                return RedirectToAction("Index");
                        //            }
                        //        }
                        //    }
                        //}

                    }
                }

                //2017-06-01 No need check manual entries
                ////string WIPQuery = @"SELECT * from tblhmiscreen where  HMIID IN ( SELECT Max(HMIID) from tblhmiscreen where Work_Order_No = '" + wono + "' and PartNo = '" + partno + "' and OperationNo != '" + opno + "' order by HMIID desc ) group by Work_Order_No,PartNo,OperationNo ";
                //// 2017-01-21
                //string WIPQuery = @"SELECT * from tblhmiscreen where  HMIID IN ( SELECT Max(HMIID) from tblhmiscreen where  HMIID IN  ( SELECT HMIID from tblhmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo != '" + opNo + "' and IsMultiWO = 0 and DDLWokrCentre is null order by HMIID desc ) group by Work_Order_No,PartNo,OperationNo  ) order by OperationNo ;";
                //var WIPOuter = db.tblhmiscreens.SqlQuery(WIPQuery).ToList();
                //if (WIPOuter.Count == 0)
                //{
                //}
                //else
                //{
                //    foreach (var row in WIPOuter)
                //    {
                //        int InnerOpNo = Convert.ToInt32(row.OperationNo);
                //        if (InnerOpNo < OperationNoInt)
                //        {
                //            if (row.isWorkInProgress != 1) //=> lower OpNo is not JF 'ed.
                //            {
                //                Session["VError"] = " JobFinish WONo: " + row.Work_Order_No + " and PartNo: " + row.PartNo + " and OperationNo: " + InnerOpNo;
                //                return RedirectToAction("Index");
                //                //break;
                //            }
                //        }
                //    }
                //}
                #endregion

                //using (i_facility_tsalEntities dbsimilar = new i_facility_tsalEntities())
                //{
                #region If its as SingleWO
                //var SimilarWOData = dbsimilar.tbllivehmiscreens.Where(m => m.HMIID != OldWork.HMIID && m.Work_Order_No == OldWork.Work_Order_No && m.OperationNo == OldWork.OperationNo && m.PartNo == OldWork.PartNo && m.MachineID != machineID && m.isWorkInProgress == 2).FirstOrDefault();
                tbllivehmiscreen SimilarWOData = obj.GetLiveHMIDetails9(OldWork.Work_Order_No, OldWork.PartNo, OldWork.OperationNo, OldWork.HMIID, machineID);
                if (SimilarWOData != null)
                {
                    //int InnerMacID = Convert.ToInt32(dbsimilar.tbllivehmiscreens.Where(m => m.HMIID == SimilarWOData.HMIID).Select(m => m.MachineID).FirstOrDefault());
                    int InnerMacID = Convert.ToInt32(obj.GetLiveHMIDetails10(SimilarWOData.HMIID));

                    //var MacDispName = Convert.ToString(dbsimilar.tblmachinedetails.Where(m => m.MachineID == InnerMacID).Select(m => m.MachineDispName).FirstOrDefault());

                    string MacDispName = obj.GetMachineDetails1(InnerMacID);

                    Session["Error"] = " Same WorkOrder is already in Machine: " + MacDispName + " , So you cannot JobFinish  ";
                    return RedirectToAction("Index");
                }
                #endregion

                #region If its as MultiWO
                //var SimilarWODataMulti = dbsimilar.tbllivemultiwoselections.Where(m => m.WorkOrder == OldWork.Work_Order_No && m.OperationNo == OldWork.OperationNo && m.PartNo == OldWork.PartNo && m.HMIID != HMIId && m.IsCompleted == 0).FirstOrDefault();
                tbllivemultiwoselection SimilarWODataMulti = obj.GetMultiWODetails3(OldWork.Work_Order_No, OldWork.OperationNo, OldWork.PartNo, HMIId);
                if (SimilarWODataMulti != null)
                {
                    int InnerHMIID = (int)SimilarWODataMulti.HMIID;
                    //var InnerHMIDupData = dbsimilar.tbllivehmiscreens.Where(m => m.HMIID == InnerHMIID).FirstOrDefault();
                    tbllivehmiscreen InnerHMIDupData = obj.GetLiveHMIDetails101(InnerHMIID);
                    if (InnerHMIDupData != null)
                    {
                        if (InnerHMIDupData.isWorkInProgress == 2)
                        {
                            int InnerMacID = Convert.ToInt32(InnerHMIDupData.MachineID);
                            //var MacDispName = Convert.ToString(dbsimilar.tblmachinedetails.Where(m => m.MachineID == InnerMacID).Select(m => m.MachineDispName).FirstOrDefault());
                            string MacDispName = obj.GetMachineDetails1(InnerMacID);
                            Session["Error"] = "Same WorkOrder is already in Machine: " + MacDispName + " , So you cannot JobFinish  ";
                            return RedirectToAction("Index");
                        }
                    }
                }
                #endregion
                //}

                //var DDLData = db.tblddls.Where(m => m.MaterialDesc == partNo && m.OperationNo == opNo && m.WorkOrder == woNo).FirstOrDefault();
                tblddl DDLData = obj.GetddlDetails3(woNo, partNo, opNo);
                if (DDLData != null)
                {
                    //DDLData.IsCompleted = 1;
                    //db.Entry(DDLData).State = System.Data.Entity.EntityState.Modified;
                    //db.SaveChanges();
                    obj.UpdateddlDetails(DDLData.DDLID);
                }
            }
            else
            {

                //using (i_facility_tsalEntities dbsimilar = new i_facility_tsalEntities())
                //{
                //var multiWOSelectionDataInner = dbsimilar.tbllivemultiwoselections.Where(m => m.HMIID == HMIId).ToList();
                List<tbllivemultiwoselection> multiWOSelectionDataInner = obj.GetMWOtDetails(HMIId);

                //string DDLIDString = string.Join(",", data.Select(x => x.ToString()).ToArray());
                string OPString = string.Join(",", multiWOSelectionDataInner.Select(x => x.OperationNo).ToArray());


                #region 2017-07-01
                foreach (tbllivemultiwoselection rowMulti in multiWOSelectionDataInner)
                {
                    string woNo = Convert.ToString(rowMulti.WorkOrder);
                    string opNo = Convert.ToString(rowMulti.OperationNo);
                    string partNo = Convert.ToString(rowMulti.PartNo);
                    int OperationNoInt = Convert.ToInt32(opNo);

                    //New Logic to Overcome WorkOrder Sequence Scenario 2017-02-03
                    //string WIPQuery1 = @"SELECT * from tblddl where WorkOrder = '" + woNo + "' and MaterialDesc = '" + partNo + "' and OperationNo != '" + opNo + "' and IsCompleted = 0 order by WorkOrder,MaterialDesc,OperationNo ";
                    //var WIPDDL1 = db.tblddls.SqlQuery(WIPQuery1).ToList();
                    List<tblddl> WIPDDL1 = obj.GetddlDetails(woNo, partNo, opNo);
                    foreach (tblddl row in WIPDDL1)
                    {
                        int InnerOpNo = Convert.ToInt32(row.OperationNo);
                        string ddlopno = row.OperationNo;
                        if (InnerOpNo < OperationNoInt)
                        {
                            if (OPString.Contains(Convert.ToString(row.OperationNo)))
                            { }
                            else
                            {
                                int PrvProcessQty = 0, PrvDeliveredQty = 0, TotalProcessQty = 0, ishold = 0;
                                #region new code
                                //here 1st get latest of delivered and processed among row in tblHMIScreen & tblmulitwoselection
                                int isHMIFirst = 2; //default NO History for that wo,pn,on

                                /*var mulitwoData = db.tbllivemultiwoselections.Where(m => m.WorkOrder == woNo && m.PartNo == partNo && m.OperationNo == ddlopno && m.tbllivehmiscreen.isWorkInProgress != 2).OrderByDescending(m => m.tbllivehmiscreen.Time).Take(1).ToList();*/
                                List<tbllivemultiwoselection> mulitwoData = obj.GetMultiWODetails2(woNo, partNo, ddlopno);
                                //var hmiData = db.tbllivehmiscreens.Where(m => m.Work_Order_No == woNo && m.PartNo == partNo && m.OperationNo == ddlopno && m.isWorkInProgress != 2).OrderByDescending(m => m.Time).Take(1).ToList();
                                List<tbllivehmiscreen> hmiData = obj.GetLiveHMIDetails11(woNo, partNo, ddlopno);

                                if (hmiData.Count > 0 && mulitwoData.Count > 0) // now check for greatest amongst
                                {
                                    DateTime multiwoDateTime = Convert.ToDateTime(mulitwoData[0].tbllivehmiscreen.Time);
                                    DateTime hmiDateTime = Convert.ToDateTime(hmiData[0].Time);

                                    if (Convert.ToInt32(multiwoDateTime.Subtract(hmiDateTime).TotalSeconds) > 0)
                                    {
                                        isHMIFirst = 1;
                                    }
                                    else
                                    {
                                        isHMIFirst = 0;
                                    }

                                }
                                else if (mulitwoData.Count > 0)
                                {
                                    isHMIFirst = 1;
                                }
                                else if (hmiData.Count > 0)
                                {
                                    isHMIFirst = 0;
                                }

                                if (isHMIFirst == 1)
                                {
                                    string delivString = Convert.ToString(mulitwoData[0].DeliveredQty);
                                    int delivInt = 0;
                                    int.TryParse(delivString, out delivInt);

                                    string processString = Convert.ToString(mulitwoData[0].ProcessQty);
                                    int procInt = 0;
                                    int.TryParse(processString, out procInt);

                                    PrvProcessQty += procInt;
                                    PrvDeliveredQty += delivInt;

                                    ishold = mulitwoData[0].tbllivehmiscreen.IsHold;
                                    ishold = ishold == 2 ? 0 : ishold;

                                }
                                else if (isHMIFirst == 0)
                                {
                                    string delivString = Convert.ToString(hmiData[0].Delivered_Qty);
                                    int delivInt = 0;
                                    int.TryParse(delivString, out delivInt);

                                    string processString = Convert.ToString(hmiData[0].ProcessQty);
                                    int procInt = 0;
                                    int.TryParse(processString, out procInt);

                                    PrvProcessQty += procInt;
                                    PrvDeliveredQty += delivInt;

                                    ishold = hmiData[0].IsHold;
                                    ishold = ishold == 2 ? 0 : ishold;
                                }
                                else
                                {
                                    //no previous delivered or processed qty so Do Nothing.
                                }
                                #endregion
                                TotalProcessQty = PrvProcessQty + PrvDeliveredQty;

                                if (Convert.ToInt32(row.TargetQty) == TotalProcessQty)
                                {
                                    #region
                                    if (isHMIFirst == 1 && Convert.ToInt32(row.TargetQty) < Convert.ToInt32(mulitwoData[0].TargetQty))
                                    {
                                        int hmiidmultitbl = Convert.ToInt32(mulitwoData[0].HMIID);
                                        //var hmiTomulittblData = db.tbllivehmiscreens.Find(hmiidmultitbl);
                                        tbllivehmiscreen hmiTomulittblData = obj.GetLiveHMIDetails101(hmiidmultitbl);
                                        if (hmiTomulittblData != null)
                                        {
                                            //tbllivehmiscreen tblh = new tbllivehmiscreen();
                                            //tblh.CorrectedDate = row.CorrectedDate;
                                            //tblh.Date = DateTime.Now;
                                            //tblh.Time = DateTime.Now;
                                            //tblh.PEStartTime = DateTime.Now;
                                            //tblh.DDLWokrCentre = hmiTomulittblData.DDLWokrCentre;
                                            //tblh.Delivered_Qty = 0;
                                            //tblh.DoneWithRow = 1;
                                            //tblh.IsHold = 0;
                                            //tblh.IsMultiWO = 0;
                                            //tblh.isUpdate = 1;
                                            //tblh.isWorkInProgress = 1;
                                            //tblh.isWorkOrder = hmiTomulittblData.isWorkOrder;
                                            //tblh.MachineID = hmiTomulittblData.MachineID;
                                            //tblh.OperationNo = mulitwoData[0].OperationNo;
                                            //tblh.OperatiorID = hmiTomulittblData.OperatiorID;
                                            //tblh.OperatorDet = hmiTomulittblData.OperatorDet;
                                            //tblh.PartNo = mulitwoData[0].PartNo;
                                            //tblh.ProcessQty = TotalProcessQty;
                                            //tblh.Prod_FAI = hmiTomulittblData.Prod_FAI;
                                            //tblh.Project = hmiTomulittblData.Project;
                                            //tblh.Rej_Qty = hmiTomulittblData.Rej_Qty;
                                            //tblh.Shift = hmiTomulittblData.Shift;
                                            //tblh.SplitWO = "No";
                                            //tblh.Status = hmiTomulittblData.Status;
                                            //tblh.Target_Qty = TotalProcessQty;
                                            //tblh.Work_Order_No = mulitwoData[0].WorkOrder;

                                            //db.tbllivehmiscreens.Add(tblh);
                                            //db.SaveChanges();
                                            obj.InsertLiveHMIScreenDetails(hmiTomulittblData.MachineID, hmiTomulittblData.OperatiorID, hmiTomulittblData.Shift, DateTime.Now, DateTime.Now, hmiTomulittblData.Project, mulitwoData[0].PartNo, mulitwoData[0].OperationNo, Convert.ToInt32(hmiTomulittblData.Rej_Qty), mulitwoData[0].WorkOrder, TotalProcessQty, 0, Convert.ToInt32(hmiTomulittblData.Status), row.CorrectedDate, hmiTomulittblData.Prod_FAI, 1, 1, 1, hmiTomulittblData.isWorkOrder, hmiTomulittblData.OperatorDet, DateTime.Now, TotalProcessQty, hmiTomulittblData.DDLWokrCentre, 0, 0, "No", 0);
                                        }

                                    }
                                    else if (isHMIFirst == 0 && Convert.ToInt32(row.TargetQty) < Convert.ToInt32(hmiData[0].Target_Qty))
                                    {
                                        //tbllivehmiscreen tblh = new tbllivehmiscreen();
                                        //tblh.CorrectedDate = row.CorrectedDate;
                                        //tblh.Date = DateTime.Now;
                                        //tblh.Time = DateTime.Now;
                                        //tblh.PEStartTime = DateTime.Now;
                                        //tblh.DDLWokrCentre = hmiData[0].DDLWokrCentre;
                                        //tblh.Delivered_Qty = 0;
                                        //tblh.DoneWithRow = 1;
                                        //tblh.IsHold = 0;
                                        //tblh.IsMultiWO = hmiData[0].IsMultiWO;
                                        //tblh.isUpdate = 1;
                                        //tblh.isWorkInProgress = 1;
                                        //tblh.isWorkOrder = hmiData[0].isWorkOrder;
                                        //tblh.MachineID = hmiData[0].MachineID;
                                        //tblh.OperationNo = hmiData[0].OperationNo;
                                        //tblh.OperatiorID = hmiData[0].OperatiorID;
                                        //tblh.OperatorDet = hmiData[0].OperatorDet;
                                        //tblh.PartNo = hmiData[0].PartNo;
                                        //tblh.ProcessQty = TotalProcessQty;
                                        //tblh.Prod_FAI = hmiData[0].Prod_FAI;
                                        //tblh.Project = hmiData[0].Project;
                                        //tblh.Rej_Qty = hmiData[0].Rej_Qty;
                                        //tblh.Shift = hmiData[0].Shift;
                                        //tblh.SplitWO = hmiData[0].SplitWO;
                                        //tblh.Status = hmiData[0].Status;
                                        //tblh.Target_Qty = TotalProcessQty;
                                        //tblh.Work_Order_No = hmiData[0].Work_Order_No;

                                        //db.tbllivehmiscreens.Add(tblh);
                                        //db.SaveChanges();
                                        obj.InsertLiveHMIScreenDetails(hmiData[0].MachineID, hmiData[0].OperatiorID, hmiData[0].Shift, DateTime.Now, DateTime.Now, hmiData[0].Project, hmiData[0].PartNo, hmiData[0].OperationNo, Convert.ToInt32(hmiData[0].Rej_Qty), hmiData[0].Work_Order_No, TotalProcessQty, 0, Convert.ToInt32(hmiData[0].Status), row.CorrectedDate, hmiData[0].Prod_FAI, 1, 1, 1, hmiData[0].isWorkOrder, hmiData[0].OperatorDet, DateTime.Now, TotalProcessQty, hmiData[0].DDLWokrCentre, hmiData[0].IsMultiWO, 0, hmiData[0].SplitWO, 0);
                                    }
                                    #endregion

                                    //row.IsCompleted = 1;
                                    //db.Entry(row).State = System.Data.Entity.EntityState.Modified;
                                    //db.SaveChanges();
                                    obj.UpdateddlDetails(row.DDLID);
                                }

                            }
                        }
                    }
                }
                #endregion

                #region 2017-02-07
                foreach (tbllivemultiwoselection rowMulti in multiWOSelectionDataInner)
                {

                    string woNo = Convert.ToString(rowMulti.WorkOrder);
                    string opNo = Convert.ToString(rowMulti.OperationNo);
                    string partNo = Convert.ToString(rowMulti.PartNo);
                    int OperationNoInt = Convert.ToInt32(opNo);

                    //New Logic to Overcome WorkOrder Sequence Scenario 2017-02-03
                    //string WIPQuery1 = @"SELECT * from tblddl where WorkOrder = '" + woNo + "' and MaterialDesc = '" + partNo + "' and OperationNo != '" + opNo + "' and IsCompleted = 0 order by WorkOrder,MaterialDesc,OperationNo ";
                    //var WIPDDL1 = db.tblddls.SqlQuery(WIPQuery1).ToList();
                    List<tblddl> WIPDDL1 = obj.GetddlDetails(woNo, partNo, opNo);
                    foreach (tblddl row in WIPDDL1)
                    {
                        int InnerOpNo = Convert.ToInt32(row.OperationNo);
                        if (InnerOpNo < OperationNoInt)
                        {
                            if (OPString.Contains(Convert.ToString(row.OperationNo)))
                            { }
                            else
                            {
                                if (row.IsCompleted == 0)
                                {
                                    Session["Error"] = " Finish WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
                                    return RedirectToAction("Index");
                                }
                                else
                                {
                                    Session["Error"] = null;
                                }

                                //bool IsItWrong = false;
                                //string WIPQueryHMI = @"SELECT * from tblhmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + InnerOpNo + "' order by HMIID desc limit 1 ";
                                //var WIP = db.tblhmiscreens.SqlQuery(WIPQueryHMI).ToList();

                                //if (WIP.Count == 0)
                                //{
                                //    Session["VError"] = " Finish WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
                                //    IsItWrong = true;
                                //}
                                //else
                                //{
                                //    foreach (var rowHMI in WIP)
                                //    {
                                //        if (rowHMI.isWorkInProgress != 1) //=> lower OpNo is in HMIScreen & not Finished.
                                //        {
                                //            Session["VError"] = " Finish WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
                                //            //return RedirectToAction("Index");
                                //            IsItWrong = false;
                                //        }
                                //        else
                                //        {
                                //            Session["VError"] = null;
                                //            IsItWrong = false;
                                //        }
                                //    }
                                //}
                                //if (IsItWrong)
                                //{
                                //    //Strange , it might have been started in Normal WorkCenter as MultiWorkOrder.
                                //    string WIPQueryMultiWO = @"SELECT * from tbl_multiwoselection where WorkOrder = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + InnerOpNo + "' order by MultiWOID desc limit 1 ";
                                //    var WIPMWO = db.tbl_multiwoselection.SqlQuery(WIPQueryMultiWO).ToList();

                                //    if (WIPMWO.Count == 0)
                                //    {
                                //        Session["VError"] = " Finish WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
                                //        return RedirectToAction("Index");
                                //    }

                                //    foreach (var rowHMI in WIPMWO)
                                //    {
                                //        int hmiid = Convert.ToInt32(rowHMI.HMIID);
                                //        var MWOHMIData = db.tblhmiscreens.Where(m => m.HMIID == hmiid).FirstOrDefault();
                                //        if (MWOHMIData != null)
                                //        {
                                //            if (MWOHMIData.isWorkInProgress != 1) //=> lower OpNo is not Finished.
                                //            {
                                //                Session["VError"] = " Finish WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
                                //                return RedirectToAction("Index");
                                //            }
                                //            else
                                //            {
                                //                Session["VError"] = null;
                                //            }
                                //        }
                                //    }
                                //}

                            }
                        }
                    }

                    //string WIPQuery = @"SELECT * from tblhmiscreen where  HMIID IN ( SELECT Max(HMIID) from tblhmiscreen where  HMIID IN  ( SELECT HMIID from tblhmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo != '" + opNo + "' and IsMultiWO = 0 and DDLWokrCentre is null order by HMIID desc ) group by Work_Order_No,PartNo,OperationNo  ) order by OperationNo ;";
                    //var WIPOuter = db.tblhmiscreens.SqlQuery(WIPQuery).ToList();
                    //if (WIPOuter.Count == 0)
                    //{
                    //}
                    //else
                    //{
                    //    foreach (var row in WIPOuter)
                    //    {
                    //        int InnerOpNo = Convert.ToInt32(row.OperationNo);
                    //        if (InnerOpNo < OperationNoInt)
                    //        {
                    //            if (row.isWorkInProgress != 1) //=> lower OpNo is not JF 'ed.
                    //            {
                    //                Session["VError"] = " JobFinish WONo: " + row.Work_Order_No + " and PartNo: " + row.PartNo + " and OperationNo: " + InnerOpNo;
                    //                return RedirectToAction("Index");
                    //                break;
                    //            }
                    //        }
                    //    }
                    //}
                }
                #endregion





                //using (i_facility_tsalEntities dbsimilar = new i_facility_tsalEntities())
                //{
                //var multiWOSelectionDataInner = dbsimilar.tbllivemultiwoselections.Where(m => m.HMIID == HMIId).ToList();
                List<tbllivemultiwoselection> multiWOSelectionDataInner1 = obj.GetMWOtDetails(HMIId);
                foreach (tbllivemultiwoselection row in multiWOSelectionDataInner1)
                {
                    try
                    {
                        #region If its as SingleWO
                        //var SimilarWOData = dbsimilar.tbllivehmiscreens.Where(m => m.HMIID != HMIId && m.Work_Order_No == row.WorkOrder && m.OperationNo == row.OperationNo && m.PartNo == row.PartNo && m.MachineID != machineID && m.isWorkInProgress == 2).FirstOrDefault();
                        tbllivehmiscreen SimilarWOData = obj.GetLiveHMIDetails9(row.WorkOrder, row.PartNo, row.OperationNo, HMIId, machineID);
                        if (SimilarWOData != null)
                        {
                            //int InnerMacID = Convert.ToInt32(dbsimilar.tbllivehmiscreens.Where(m => m.HMIID == SimilarWOData.HMIID).Select(m => m.MachineID).FirstOrDefault());
                            int InnerMacID = Convert.ToInt32(obj.GetLiveHMIDetails10(SimilarWOData.HMIID));
                            //var MacDispName = Convert.ToString(dbsimilar.tblmachinedetails.Where(m => m.MachineID == InnerMacID).Select(m => m.MachineDispName).FirstOrDefault());
                            string MacDispName = obj.GetMachineDetails1(InnerMacID);

                            Session["Error"] = " Same WorkOrder is already in Machine: " + MacDispName + " , So you cannot JobFinish";
                            return RedirectToAction("Index");
                        }
                        #endregion

                        #region If its as MultiWO
                        //var SimilarWODataMulti = dbsimilar.tbllivemultiwoselections.Where(m => m.WorkOrder == row.WorkOrder && m.OperationNo == row.OperationNo && m.PartNo == row.PartNo && m.HMIID != HMIId && m.tbllivehmiscreen.isWorkInProgress == 2).FirstOrDefault();
                        tbllivemultiwoselection SimilarWODataMulti = obj.GetMultiWODetails4(row.WorkOrder, row.PartNo, row.OperationNo, HMIId);

                        if (SimilarWODataMulti != null)
                        {
                            int InnerHMIID = (int)SimilarWODataMulti.HMIID;

                            //Again check this hmiid in hmiscreen is finished or not
                            //if not thorw error
                            //var InnerHMIDupData = dbsimilar.tbllivehmiscreens.Where(m => m.HMIID == InnerHMIID).FirstOrDefault();
                            tbllivehmiscreen InnerHMIDupData = obj.GetLiveHMIDetails6(InnerHMIID);
                            if (InnerHMIDupData != null)
                            {
                                if (InnerHMIDupData.isWorkInProgress == 2)
                                {
                                    int InnerMacID = Convert.ToInt32(InnerHMIDupData.MachineID);
                                    //var MacDispName = Convert.ToString(dbsimilar.tblmachinedetails.Where(m => m.MachineID == InnerMacID).Select(m => m.MachineDispName).FirstOrDefault());
                                    string MacDispName = obj.GetMachineDetails1(InnerMacID);
                                    Session["Error"] = "Same WorkOrder is already in Machine: " + MacDispName + " , So you cannot JobFinish";
                                    return RedirectToAction("Index");
                                }
                            }
                        }
                        #endregion
                    }
                    catch (Exception)
                    {
                    }
                }

                //var multiWOSelectionData = db.tbllivemultiwoselections.Where(m => m.HMIID == HMIId).ToList();
                List<tbllivemultiwoselection> multiWOSelectionData = obj.GetMWOtDetails(HMIId);
                foreach (tbllivemultiwoselection row in multiWOSelectionData)
                {
                    string woNo = Convert.ToString(row.WorkOrder);
                    string opNo = Convert.ToString(row.OperationNo);
                    string partNo = Convert.ToString(row.PartNo);
                    int deliveredQty = Convert.ToInt32(row.DeliveredQty);
                    int targetqty = Convert.ToInt32(row.TargetQty);
                    //if (deliveredQty == targetqty)
                    {
                        //row.IsCompleted = 1;
                        //db.Entry(row).State = System.Data.Entity.EntityState.Modified;
                        //db.SaveChanges();
                        obj.UpdateMWoDetails(row.MultiWOID);
                    }

                    //var DDLData = db.tblddls.Where(m => m.MaterialDesc == partNo && m.OperationNo == opNo && m.WorkOrder == woNo).FirstOrDefault();
                    tblddl DDLData = obj.GetddlDetails3(woNo, partNo, opNo);
                    if (DDLData != null)
                    {
                        //DDLData.IsCompleted = 1;
                        //DDLData.DeliveredQty = deliveredQty;

                        //db.Entry(DDLData).State = System.Data.Entity.EntityState.Modified;
                        //db.SaveChanges();
                        obj.UpdateddlDetails1(DDLData.DDLID, deliveredQty);
                    }
                }
            }

            OldWork.SplitWO = "No";
            //db.Entry(OldWork).State = System.Data.Entity.EntityState.Modified;
            //db.SaveChanges();
            int HMIID = OldWork.HMIID;
            obj.UpdateLiveHMIDetails(HMIID, OldWork.SplitWO, Convert.ToInt32(OldWork.Status), OldWork.ProcessQty, Convert.ToInt32(OldWork.Delivered_Qty), Convert.ToDateTime(OldWork.Time), OldWork.isWorkInProgress, OldWork.isWorkOrder);

            string CorrectedDate = null;
            //tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
            tbldaytiming StartTime = obj.GetDaytimingDetails();
            TimeSpan Start = StartTime.StartTime;
            if (Start <= DateTime.Now.TimeOfDay)
            {
                CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }

            //insert a new row if there is no row for this machine for this shift.
            //tbllivehmiscreen HMI = db.tbllivehmiscreens.Where(m => m.CorrectedDate == CorrectedDate).Where(m => m.MachineID == machineID && m.Status == 0).FirstOrDefault();
            tbllivehmiscreen HMI = obj.GetLiveHMIDetails12(machineID, CorrectedDate);
            if (HMI == null)
            {

                // tbllivehmiscreen NewEntry = new tbllivehmiscreen();
                // NewEntry.MachineID = machineID;
                // NewEntry.CorrectedDate = CorrectedDate;
                // NewEntry.PEStartTime = DateTime.Now;
                // //NewEntry.Date = DateTime.Now;
                // //NewEntry.Time = DateTime.Now;
                // NewEntry.Shift = Convert.ToString(Shiftgen);
                // NewEntry.OperatorDet = operatorName;
                // NewEntry.Status = 0;
                // NewEntry.isWorkInProgress = 2;
                // NewEntry.OperatiorID = Opgid;
                //// NewEntry.HMIID = (HMMID.HMIID + 1); // by Ashok
                // db.tbllivehmiscreens.Add(NewEntry);
                // db.SaveChanges();

                obj.InsertLiveHMIScreenDetails1(machineID, Opgid, Convert.ToString(Shiftgen), 0, CorrectedDate, 2, operatorName, DateTime.Now);

                Session["FromDDL"] = 0;
                Session["SubmitClicked"] = 0;
            }
            return RedirectToAction("Index");
        }


        //control comes here when PartialFinish is pressed.
        public ActionResult EditWIP(string SplitWO = null, int id = 0, int reworkorderhidden = 0, int cjtextbox9 = 0, int cjtextbox8 = 0)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);
            //Getting Shift Value
            DateTime Time = DateTime.Now;
            TimeSpan Tm = new TimeSpan(Time.Hour, Time.Minute, Time.Second);
            //var ShiftDetails = db.tblshift_mstr.Where(m => m.StartTime <= Tm && m.EndTime >= Tm);
            tblshift_mstr ShiftDetails = obj.GetShiftDetails(Tm);
            string Shift = null;
            if (ShiftDetails != null)
            {
                Shift = ShiftDetails.ShiftName;
            }
            ViewBag.date = System.DateTime.Now;
            if (Shift != null)
            {
                ViewBag.shift = Shift;
            }
            else
            {
                ViewBag.shift = "C";
            }

            int machineID = 0;
            //tbllivehmiscreen tblhmiscreen = db.tbllivehmiscreens.Find(id);
            tbllivehmiscreen tblhmiscreen = obj.GetLiveHMIDetails6(id);
            machineID = Convert.ToInt32(tblhmiscreen.MachineID);
            int Uid = tblhmiscreen.OperatiorID;
            int ID = id;
            tbllivehmiscreen OldWork = obj.GetLiveHMIDetails6(ID);

            //2017-06-02 
            //OldWork.ProcessQty = cjtextbox9;
            OldWork.Delivered_Qty = cjtextbox8;
            OldWork.Status = 2;
            OldWork.Time = DateTime.Now;
            //else save workInProgress and continue
            OldWork.isWorkInProgress = 0;//work is in progress
            if (reworkorderhidden == 1)
            {
                OldWork.isWorkOrder = 1;
            }
            string Shiftgen = OldWork.Shift;
            string operatorName = OldWork.OperatorDet;
            int Opgid = OldWork.OperatiorID;

            int IsWOMultiWO = OldWork.IsMultiWO;
            //some how id is being passed to Index when redirected. so 
            id = 0;
            if (IsWOMultiWO == 0 && SplitWO.Length > 0 && !string.IsNullOrEmpty(SplitWO.Trim()))
            {
                OldWork.SplitWO = SplitWO;
            }
            int OldDeliveredQty = Convert.ToInt32(OldWork.Delivered_Qty);

            int Hmiid = OldWork.HMIID;
            List<string> MacHierarchy = GetHierarchyData(machineID);

            //Update ProcessedQty of Same WorkOrder , OpNo , PartNo In Other Machines IF(IsWorkInProgress == 2)
            //using (i_facility_tsalEntities dbsimilar = new i_facility_tsalEntities())
            //{
            if (IsWOMultiWO == 0)
            {

                if (Convert.ToInt32(OldWork.Target_Qty) < (Convert.ToInt32(OldWork.Delivered_Qty) + Convert.ToInt32(OldWork.ProcessQty)))
                {
                    Session["Error"] = " DeliveredQty + ProcessedQty should be equal to Target for WONo: " + OldWork.Work_Order_No + " OpNo: " + OldWork.OperationNo;
                    return RedirectToAction("Index");
                }

                #region If its as SingleWO
                //var SimilarWOData = dbsimilar.tbllivehmiscreens.Where(m => m.Work_Order_No == OldWork.Work_Order_No && m.OperationNo == OldWork.OperationNo && m.PartNo == OldWork.PartNo && m.MachineID != machineID && m.isWorkInProgress == 2).ToList();
                List<tbllivehmiscreen> SimilarWOData = obj.GetLiHMIDetails(OldWork.Work_Order_No, OldWork.PartNo, OldWork.OperationNo, machineID);
                foreach (tbllivehmiscreen row in SimilarWOData)
                {
                    int InnerDelivered = Convert.ToInt32(row.Delivered_Qty);
                    int InnerProcessed = Convert.ToInt32(row.ProcessQty);
                    int FinalProcessed = InnerDelivered + InnerProcessed;
                    if (FinalProcessed < row.Target_Qty)
                    {
                        if (row.isWorkInProgress == 2)
                        {
                            row.ProcessQty = FinalProcessed;
                            //dbsimilar.Entry(row).State = System.Data.Entity.EntityState.Modified;
                            //dbsimilar.SaveChanges();
                            obj.UpdateLivHMI2Dets(row.HMIID, row.ProcessQty);
                        }
                    }
                    else
                    {
                        Session["Error"] = " Same WorkOrder in Machine: " + MacHierarchy[3] + "->" + MacHierarchy[4] + "has ProcessedQty :" + InnerProcessed;
                        return RedirectToAction("Index");
                    }
                }
                #endregion

                #region If its as MultiWO
                //var SimilarWODataMulti = dbsimilar.tbllivemultiwoselections.Where(m => m.WorkOrder == OldWork.Work_Order_No && m.OperationNo == OldWork.OperationNo && m.PartNo == OldWork.PartNo && m.HMIID != Hmiid && m.tbllivehmiscreen.isWorkInProgress == 2).ToList();
                List<tbllivemultiwoselection> SimilarWODataMulti = obj.GetMultiWOListDetails(OldWork.Work_Order_No, OldWork.PartNo, OldWork.OperationNo, Hmiid);
                foreach (tbllivemultiwoselection row in SimilarWODataMulti)
                {
                    int RowHMIID = Convert.ToInt32(row.HMIID);
                    //var localhmiData = dbsimilar.tbllivehmiscreens.Find(RowHMIID);
                    tbllivehmiscreen localhmiData = obj.GetLiveHMIDetails6(RowHMIID);
                    //int InnerDelivered = Convert.ToInt32(row.DeliveredQty);
                    int InnerProcessed = Convert.ToInt32(row.ProcessQty);
                    int FinalProcessed = OldDeliveredQty + InnerProcessed;
                    if (FinalProcessed < row.TargetQty)
                    {
                        if (localhmiData.isWorkInProgress == 2)
                        {
                            row.ProcessQty = FinalProcessed;
                            //dbsimilar.Entry(row).State = System.Data.Entity.EntityState.Modified;
                            //dbsimilar.SaveChanges();
                            obj.UpdateMultiWork2Dets(row.MultiWOID, Convert.ToInt32(row.ProcessQty));
                            //Update tblhmiscreen table row.
                            if (localhmiData != null)
                            {
                                localhmiData.ProcessQty += OldDeliveredQty;
                                //dbsimilar.Entry(localhmiData).State = System.Data.Entity.EntityState.Modified;
                                //dbsimilar.SaveChanges();
                                obj.UpdateLivHMI2Dets(localhmiData.HMIID, localhmiData.ProcessQty);
                            }
                        }
                    }
                    else
                    {
                        Session["Error"] = " Same WorkOrder in Machine: " + MacHierarchy[3] + "->" + MacHierarchy[4] + "has ProcessedQty :" + InnerProcessed;
                        return RedirectToAction("Index");
                    }

                    //int InnerHMIID = (int)row.HMIID;
                    //var InnerHMIDupData = dbsimilar.tblhmiscreens.Where(m => m.HMIID == InnerHMIID && m.HMIID != Hmiid).FirstOrDefault();
                    //if (InnerHMIDupData != null)
                    //{
                    //    if (InnerHMIDupData.isWorkInProgress == 2)
                    //    {
                    //        int InnerMacID = Convert.ToInt32(InnerHMIDupData.MachineID);
                    //        var MacDispName = Convert.ToString(dbsimilar.tblmachinedetails.Where(m => m.MachineID == InnerMacID).Select(m => m.MachineDispName).FirstOrDefault());
                    //        Session["Error"] = " Same WorkOrder in Machine: " + MacDispName + " , So you cannot JobFinish ";
                    //        return RedirectToAction("Index");
                    //    }
                    //}
                }
                #endregion
            }
            else
            {
                #region
                //var multiWOSelectionData = db.tbllivemultiwoselections.Where(m => m.HMIID == Hmiid).ToList();
                List<tbllivemultiwoselection> multiWOSelectionData = obj.GetMWOtDetails(Hmiid);
                //during pf dont allow jf of higher opno
                List<tbllivemultiwoselection> multiWOSelectionData1 = multiWOSelectionData.OrderBy(m => m.PartNo).ThenBy(m => m.WorkOrder).ThenBy(m => m.OperationNo).ToList();

                string OPString = string.Join(",", multiWOSelectionData1.Select(x => x.OperationNo).ToArray());
                foreach (tbllivemultiwoselection row in multiWOSelectionData1)
                {
                    try
                    {
                        String WONo = row.WorkOrder;
                        String Part = row.PartNo;
                        String Operation = row.OperationNo;
                        int opInt = Convert.ToInt32(Operation);

                        int TargetQtyOut = Convert.ToInt32(row.TargetQty);
                        int ProcessedQtyOut = Convert.ToInt32(row.ProcessQty);
                        int DeliveredQtyOut = Convert.ToInt32(row.DeliveredQty);
                        //if (TargetQtyOut <= (ProcessedQtyOut + DeliveredQtyOut))
                        if (TargetQtyOut == (ProcessedQtyOut + DeliveredQtyOut))
                        {
                            foreach (tbllivemultiwoselection rowInner in multiWOSelectionData1)
                            {
                                String OperationInner = rowInner.OperationNo;
                                int opInnerInt = Convert.ToInt32(OperationInner);
                                if (opInnerInt < opInt)
                                {
                                    String WONoInner = rowInner.WorkOrder;
                                    String PartInner = rowInner.PartNo;
                                    //now check if they are about to JF, if so throw error.
                                    int TargetQty = Convert.ToInt32(rowInner.TargetQty);
                                    int ProcessedQty = Convert.ToInt32(rowInner.ProcessQty);
                                    int DeliveredQty = Convert.ToInt32(rowInner.DeliveredQty);
                                    if ((ProcessedQty + DeliveredQty) < TargetQty)
                                    {
                                        //if (OPString.Contains(OperationInner))
                                        //{ }
                                        //else
                                        //{
                                        TempData["VError"] = "Please finish this job first. WoNo: " + rowInner.WorkOrder + " OpNo: " + rowInner.OperationNo;
                                        return RedirectToAction("Index");
                                        //}
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                }

                //Check for Total Quantity compatibility
                foreach (tbllivemultiwoselection row in multiWOSelectionData)
                {

                    string LocalWONO = row.WorkOrder;
                    string LocalPartNo = row.PartNo;
                    string LocalOpNo = row.OperationNo;
                    int LocalDelivered = Convert.ToInt32(row.DeliveredQty);

                    if (Convert.ToInt32(row.TargetQty) < (Convert.ToInt32(row.DeliveredQty) + Convert.ToInt32(row.ProcessQty)))
                    {
                        Session["Error"] = " DeliveredQty + ProcessedQty should be equal to Target for WONo: " + LocalWONO + " OpNo: " + LocalOpNo;
                        return RedirectToAction("Index");
                    }

                    try
                    {
                        //var SimilarWOData = dbsimilar.tbllivehmiscreens.Where(m => m.Work_Order_No == LocalWONO && m.OperationNo == LocalOpNo && m.PartNo == LocalPartNo && m.MachineID != machineID && m.isWorkInProgress == 2).ToList();
                        List<tbllivehmiscreen> SimilarWOData = obj.GetLiHMIDetails(LocalWONO, LocalPartNo, LocalOpNo, machineID);
                        foreach (tbllivehmiscreen Innerrow in SimilarWOData)
                        {
                            int InnerProcessed = Innerrow.ProcessQty;
                            //int InnerDelivered = Convert.ToInt32(Innerrow.Delivered_Qty);
                            int FinalProcessed = LocalDelivered + InnerProcessed;
                            if (FinalProcessed < Innerrow.Target_Qty)
                            {
                                if (Innerrow.isWorkInProgress == 2)
                                {
                                    Innerrow.ProcessQty = FinalProcessed;
                                    //dbsimilar.Entry(Innerrow).State = System.Data.Entity.EntityState.Modified;
                                    //dbsimilar.SaveChanges();
                                    obj.UpdateLivHMI2Dets(Innerrow.HMIID, Innerrow.ProcessQty);
                                }
                            }
                            else
                            {
                                Session["Error"] = " Same WorkOrder in Machine: " + MacHierarchy[3] + "->" + MacHierarchy[4] + " , Target Qty Exceeds.";
                                return RedirectToAction("Index");
                            }
                        }

                        #region If its as MultiWO
                        //var SimilarWODataMulti = dbsimilar.tbllivemultiwoselections.Where(m => m.WorkOrder == LocalWONO && m.OperationNo == LocalOpNo && m.PartNo == LocalPartNo && m.HMIID != Hmiid && m.tbllivehmiscreen.isWorkInProgress == 2).ToList();
                        List<tbllivemultiwoselection> SimilarWODataMulti = obj.GetMultiWOListDetails(LocalWONO, LocalPartNo, LocalOpNo, Hmiid);
                        foreach (tbllivemultiwoselection Innerrow in SimilarWODataMulti)
                        {
                            //update only if its still in hmiscreen
                            int RowHMIID = Convert.ToInt32(row.HMIID);
                            //var localhmiData = dbsimilar.tbllivehmiscreens.Find(RowHMIID);
                            tbllivehmiscreen localhmiData = obj.GetLiveHMIDetails6(RowHMIID);
                            int DeliveredQtyLocal = Convert.ToInt32(Innerrow.DeliveredQty);
                            int InnerProcessed = Convert.ToInt32(Innerrow.ProcessQty);
                            int FinalProcessed = DeliveredQtyLocal + InnerProcessed;
                            if (FinalProcessed < Innerrow.TargetQty)
                            {
                                if (localhmiData.isWorkInProgress == 2)
                                {
                                    Innerrow.ProcessQty = FinalProcessed;
                                    //dbsimilar.Entry(Innerrow).State = System.Data.Entity.EntityState.Modified;
                                    //dbsimilar.SaveChanges();
                                    obj.UpdateMultiWork2Dets(Innerrow.MultiWOID, Convert.ToInt32(Innerrow.ProcessQty));

                                    //Update tblhmiscreen table row.
                                    if (localhmiData != null)
                                    {
                                        localhmiData.ProcessQty += DeliveredQtyLocal;
                                        //dbsimilar.Entry(localhmiData).State = System.Data.Entity.EntityState.Modified;
                                        //dbsimilar.SaveChanges();
                                        obj.UpdateLivHMI2Dets(localhmiData.HMIID, localhmiData.ProcessQty);
                                    }
                                }
                            }
                            else
                            {
                                Session["Error"] = " Same WorkOrder in Machine: " + MacHierarchy[3] + "->" + MacHierarchy[4] + "has ProcessedQty :" + InnerProcessed;
                                return RedirectToAction("Index");
                            }

                            int InnerHMIID = (int)Innerrow.HMIID;
                            //var InnerHMIDupData = dbsimilar.tbllivehmiscreens.Where(m => m.HMIID == InnerHMIID).FirstOrDefault();
                            tbllivehmiscreen InnerHMIDupData = obj.GetLiveHMIDetails101(InnerHMIID);
                            if (InnerHMIDupData != null)
                            {
                                if (InnerHMIDupData.isWorkInProgress == 2)
                                {
                                    int InnerMacID = Convert.ToInt32(InnerHMIDupData.MachineID);
                                    //var MacDispName = Convert.ToString(dbsimilar.tblmachinedetails.Where(m => m.MachineID == InnerMacID).Select(m => m.MachineDispName).FirstOrDefault());
                                    string MacDispName = obj.GetMachineDetails1(InnerMacID);
                                    Session["Error"] = " Same WorkOrder is already in Machine: " + MacDispName + " , So you cannot JobFinish  ";
                                    return RedirectToAction("Index");
                                }
                            }
                        }
                        #endregion
                    }
                    catch (Exception)
                    {
                    }
                }

                #endregion
            }
            //}

            //db.Entry(OldWork).State = System.Data.Entity.EntityState.Modified;
            //db.SaveChanges();
            int HMIID = OldWork.HMIID;
            obj.UpdateLiv1HMIDetails(HMIID, OldWork.SplitWO, Convert.ToInt32(OldWork.Status), Convert.ToInt32(OldWork.Delivered_Qty), Convert.ToDateTime(OldWork.Time), OldWork.isWorkInProgress, OldWork.isWorkOrder);

            if (IsWOMultiWO == 1)
            {
                int hmiid = OldWork.HMIID;
                //var multiWOSelectionData = db.tbllivemultiwoselections.Where(m => m.HMIID == hmiid).ToList();
                List<tbllivemultiwoselection> multiWOSelectionData = obj.GetMWOtDetails(hmiid);
                foreach (tbllivemultiwoselection row in multiWOSelectionData)
                {
                    string woNo = Convert.ToString(row.WorkOrder);
                    string opNo = Convert.ToString(row.OperationNo);
                    string partNo = Convert.ToString(row.PartNo);
                    int deliveredQty = Convert.ToInt32(row.DeliveredQty);
                    int targetqty = Convert.ToInt32(row.TargetQty);
                    int processedQty = Convert.ToInt32(row.ProcessQty);
                    if ((deliveredQty + processedQty) == targetqty)
                    {
                        try
                        {
                            //row.IsCompleted = 1;
                            //row.SplitWO = SplitWO;
                            //db.Entry(row).State = System.Data.Entity.EntityState.Modified;
                            //db.SaveChanges();
                            obj.UpdateMWoDetails(row.MultiWOID);
                        }
                        catch (Exception)
                        {
                        }
                        try
                        {
                            //var DDLData = db.tblddls.Where(m => m.MaterialDesc == partNo && m.OperationNo == opNo && m.WorkOrder == woNo).FirstOrDefault();
                            tblddl DDLData = obj.GetddlDetails3(woNo, partNo, opNo);
                            DDLData.DeliveredQty = deliveredQty + processedQty;
                            //DDLData.IsCompleted = 1;
                            //db.Entry(DDLData).State = System.Data.Entity.EntityState.Modified;
                            //db.SaveChanges();
                            obj.UpdateddlDetails1(DDLData.DDLID, DDLData.DeliveredQty);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }

            string CorrectedDate = null;
            //tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
            tbldaytiming StartTime = obj.GetDaytimingDetails();
            TimeSpan Start = StartTime.StartTime;
            if (Start <= DateTime.Now.TimeOfDay)
            {
                CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }

            //insert a new row if there is no row for this machine for this shift.
            //tbllivehmiscreen HMI = db.tbllivehmiscreens.Where(m => m.CorrectedDate == CorrectedDate).Where(m => m.MachineID == machineID && m.Status == 0).FirstOrDefault();
            tbllivehmiscreen HMI = obj.GetLiveHMIScreenDetails3(CorrectedDate, machineID);
            if (HMI == null)
            {

                //tbllivehmiscreen NewEntry = new tbllivehmiscreen();
                //NewEntry.MachineID = machineID;
                //NewEntry.CorrectedDate = CorrectedDate;
                //NewEntry.PEStartTime = DateTime.Now;
                ////NewEntry.Date = DateTime.Now;
                ////NewEntry.Time = DateTime.Now;
                //NewEntry.OperatorDet = operatorName;
                //NewEntry.Shift = Convert.ToString(Shiftgen);
                //NewEntry.Status = 0;
                //NewEntry.isWorkInProgress = 2;
                //NewEntry.OperatiorID = Opgid;
                // NewEntry.HMIID = (HMMID.HMIID + 1);  //by Ashok
                //db.tbllivehmiscreens.Add(NewEntry);
                //db.SaveChanges();
                obj.InsertLiveHMIScreenDetails1(machineID, Opgid, Convert.ToString(Shiftgen), 0, CorrectedDate, 2, operatorName, DateTime.Now);
                Session["FromDDL"] = 0;
                Session["SubmitClicked"] = 0;
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(tbllivehmiscreen tblhmiscreen)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);
            if (ModelState.IsValid)
            {
                tblhmiscreen.isWorkInProgress = 0;
                //db.Entry(tblhmiscreen).State = System.Data.Entity.EntityState.Modified;
                //db.SaveChanges();
                obj.UpdateLiveHMIDetails1(tblhmiscreen.HMIID, tblhmiscreen.MachineID, tblhmiscreen.OperatiorID, tblhmiscreen.Shift, Convert.ToDateTime(tblhmiscreen.Date), Convert.ToDateTime(tblhmiscreen.Time), tblhmiscreen.Project, tblhmiscreen.PartNo, tblhmiscreen.OperationNo, Convert.ToInt32(tblhmiscreen.Rej_Qty), tblhmiscreen.Work_Order_No, Convert.ToInt32(tblhmiscreen.Target_Qty), Convert.ToInt32(tblhmiscreen.Delivered_Qty), Convert.ToInt32(tblhmiscreen.Status), tblhmiscreen.CorrectedDate, tblhmiscreen.Prod_FAI, tblhmiscreen.isUpdate, tblhmiscreen.DoneWithRow, tblhmiscreen.isWorkInProgress, tblhmiscreen.isWorkOrder, tblhmiscreen.OperatorDet, Convert.ToDateTime(tblhmiscreen.PEStartTime), tblhmiscreen.ProcessQty, tblhmiscreen.DDLWokrCentre, tblhmiscreen.IsMultiWO, tblhmiscreen.IsHold, tblhmiscreen.SplitWO, Convert.ToInt32(tblhmiscreen.batchNo));
                return RedirectToAction("Index");
            }
            return View(tblhmiscreen);
        }

        public ActionResult Delete(int id = 0)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            //tbllivehmiscreen tblhmiscreen = db.tbllivehmiscreens.Find(id);
            tbllivehmiscreen tblhmiscreen = obj.GetLiveHMIDetails6(id);
            if (tblhmiscreen == null)
            {
                return HttpNotFound();
            }
            return View(tblhmiscreen);
        }

        public JsonResult AutoSaveListNo(int HMIID, string value)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);
            int nothing = 1;
            //Get one record from livehmiscreen screen of that particular HMIID
            List<tbllivehmiscreen> thisrow = obj.GetLiveHMIDetailsList1(HMIID);
            obj.UpdateLiveHMIScreenListNo(thisrow[0].HMIID, value);
            return Json(nothing, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            //using (i_facility_tsalEntities dbhmi = new i_facility_tsalEntities())
            //{
            try
            {
                //tbllivehmiscreen tblhmiscreen = dbhmi.tbllivehmiscreens.Find(id);
                tbllivehmiscreen tblhmiscreen = obj.GetLiveHMIDetails6(id);
                //db.tbllivehmiscreens.Remove(tblhmiscreen);
                //db.SaveChanges();
                obj.DeleteHMIScreenDetails(tblhmiscreen.HMIID);

                if (tblhmiscreen != null && tblhmiscreen.IsMultiWO == 1)
                {
                    //dbhmi.tbllivemultiwoselections.RemoveRange(dbhmi.tbllivemultiwoselections.Where(m => m.HMIID == tblhmiscreen.HMIID).ToList());
                    //dbhmi.SaveChanges();
                    obj.DeleteMWoDetails();
                }
            }
            catch (Exception)
            {

            }
            //}

            return RedirectToAction("Index");
        }

        //protected override void Dispose(bool disposing)
        //{
        //    db.Dispose();
        //    base.Dispose(disposing);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create1(List<tblhmiscreen> tblstat_prodcyctime)
        {
            return RedirectToAction("Index");
        }



        public ActionResult setupentry(int id = 0)
        {
            TempData["Enable"] = null;
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);
            string CorrectedDate = null;
            //tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
            tbldaytiming StartTime = obj.GetDaytimingDetails();
            TimeSpan Start = StartTime.StartTime;
            if (Start <= DateTime.Now.TimeOfDay)
            {
                CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }
            int MessageCodeID = 81;
            //var brkdown = db.tbllivelossofentries.Where(m => m.MachineID == id).Where(m => m.CorrectedDate == CorrectedDate && m.EndDateTime == null && m.MessageCodeID == 81);
            List<tbllivelossofentry> brkdown = obj.GetLossOfEntryDetails(id, CorrectedDate, MessageCodeID);

            int mdid = 0;
            foreach (tbllivelossofentry jd in brkdown)
            {
                mdid = jd.LossID;
            }
            //tbllivelossofentry loss = db.tbllivelossofentries.Find(mdid);
            tbllivelossofentry loss = obj.GetLossOfEntryDetails6(mdid);
            if (ModelState.IsValid)
            {
                loss.EndDateTime = DateTime.Now;
                //db.Entry(loss).State = System.Data.Entity.EntityState.Modified;
                //db.SaveChanges();
                obj.UpdateLossofEntryDetails(loss.LossID, loss.MessageCodeID, Convert.ToDateTime(loss.StartDateTime), Convert.ToDateTime(loss.EndDateTime), Convert.ToDateTime(loss.EntryTime), loss.CorrectedDate, loss.MachineID, loss.Shift, loss.MessageDesc, loss.MessageCode, loss.IsUpdate, loss.DoneWithRow, Convert.ToInt32(loss.IsStart), Convert.ToInt32(loss.IsScreen), loss.ForRefresh);
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        public ActionResult Dashboard(int id = 0)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            return RedirectToAction("Index", "Dashboard");
        }

        //breakdownlist
        public ActionResult BreakDownList(int id = 0)
        {
            Session["Mode"] = null;
            Session["split"] = null;
            Session["redStrat"] = 0;
            Session["redEnd"] = 0;
            //Session["Split"] = null;
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);
            string CorrectedDate = null;
            //tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
            tbldaytiming StartTime = obj.GetDaytimingDetails();
            TimeSpan Start = StartTime.StartTime;
            if (Start <= DateTime.Now.TimeOfDay)
            {
                CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }
            ViewBag.opid = Session["opid"];
            ViewBag.mcnid = id;
            ViewBag.coretddt = CorrectedDate;

            //bool tick = checkingIdle();
            //if (tick == true)
            //{
            //    return RedirectToAction("DownCodeEntry");
            //    //ViewBag.tick = 1;
            //}

            int handleidleReturnValue = HandleIdle();
            if (handleidleReturnValue == 0)
            {
                return RedirectToAction("DownCodeEntry");
            }

            //var machinedispname = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == id).Select(m => m.MachineDispName).FirstOrDefault();
            string machinedispname = obj.GetMachineDet1(id);
            ViewBag.macDispName = Convert.ToString(machinedispname);

            //var breakdown = db.tblbreakdowns.Include(t=>t.machine_master).Include(t=>t.message_code_master).Where(m=>m.MachineID==id && m.CorrectedDate==CorrectedDate).ToList();
            //var breakdown = db.tblbreakdowns.Include(t => t.tbllossescode).Where(m => m.MachineID == id && m.CorrectedDate == CorrectedDate && m.DoneWithRow == 1).OrderByDescending(m => m.StartTime).ToList();
            List<tblbreakdown> breakdown = obj.GetbreakdownDetails(id, CorrectedDate);
            return View(breakdown);
        }

        //IdleList
        //public ActionResult IdleList(int id = 0)
        //{
        //    _conn = new ConnectionFactory();
        //    obj = new Dao(_conn);

        //    if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
        //    {
        //        return RedirectToAction("Login", "Login", null);
        //    }
        //    ViewBag.Logout = Session["Username"];
        //    ViewBag.roleid = Session["RoleID"];
        //    string CorrectedDate = null, Shift = null;
        //    string[] DateShiftValues = GetDateShift();
        //    CorrectedDate = DateShiftValues[1];
        //    Shift = DateShiftValues[0];
        //    //int handleidleReturnValue = HandleIdleManualWC();
        //    //if (handleidleReturnValue == 0)
        //    //{
        //    //    return RedirectToAction("DownCodeEntry");
        //    //}

        //    //var machinedispname = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == id).Select(m => m.MachineDispName).FirstOrDefault();
        //    var machinedispname = obj.GetMachineDet1(id);
        //    ViewBag.macDispName = Convert.ToString(machinedispname);
        //    var vm = new HoldList();
        //    ViewBag.coretddt = CorrectedDate;
        //    //var idle = db.tbllossofentries.Include(t => t.machine_master).Include(t => t.message_code_master).Where(m => m.MachineID == id && m.CorrectedDate == CorrectedDate).ToList();
        //    //vm.HoldListDetailsWO = db.tblmanuallossofentries.Include(t => t.tblholdcode).Where(m => m.MachineID == id && m.CorrectedDate == CorrectedDate).ToList();
        //    //vm.HoldListDetailsWC = db.tbllivelossofentries.Include(t => t.tbllossescode).Where(m => m.MachineID == id && m.CorrectedDate == CorrectedDate && m.DoneWithRow == 1).OrderByDescending(m => m.StartDateTime).ToList();
        //    vm.HoldListDetailsWC = obj.GetLossofEntriesDetails(id, CorrectedDate);

        //    #region another technique. didnot finish
        //    ////    HoldCodesList : HoldCodeID  RL1  RL2  RL3  IsWOLevel 
        //    //List<HoldCodesList> HoldCodesToView = new List<HoldCodesList>();
        //    //foreach (var row in WOLossDetails)
        //    //{
        //    //    int LevelCurrentCodeInt = row.MessageCodeID;
        //    //    string Level1CodeString = row.MessageCode;
        //    //    string Level2CodeString = null, Level3CodeString = null;

        //    //    var LossCodeDetails = db.tbllossescodes.Where(m => m.LossCodeID == LevelCurrentCodeInt).FirstOrDefault();

        //    //    int Level1CodeInt = 0;
        //    //    if (int.TryParse(Convert.ToString(LossCodeDetails.LossCodesLevel1ID), out Level1CodeInt))
        //    //    {
        //    //        Level2CodeString = db.tbllossescodes.Where(m => m.LossCodeID == Level1CodeInt).Select(m => m.LossCode).FirstOrDefault();
        //    //    }

        //    //    int Level2CodeInt = 0;
        //    //    if (int.TryParse(Convert.ToString(LossCodeDetails.LossCodesLevel2ID), out Level2CodeInt))
        //    //    {
        //    //        Level2CodeString = db.tbllossescodes.Where(m => m.LossCodeID == Level2CodeInt).Select(m => m.LossCode).FirstOrDefault();
        //    //    }

        //    //    HoldCodesToView.Add(new HoldCodesList { HoldCodeID = LevelCurrentCodeInt , RL1 = Level1CodeString , RL2 = Level2CodeString , RL3 = Level3CodeString ,  IsWOLevel = 1 });
        //    //}

        //    //var LossDataDetails = db.tbllossofentries.Where(m => m.MachineID == id && m.CorrectedDate == CorrectedDate).ToList();
        //    //foreach (var row in LossDataDetails)
        //    //{
        //    //    int LevelCurrentCodeInt = row.MessageCodeID;
        //    //    string Level1CodeString = row.MessageCode;
        //    //    string Level2CodeString = null, Level3CodeString = null;

        //    //    var LossCodeDetails = db.tbllossescodes.Where(m => m.LossCodeID == LevelCurrentCodeInt).FirstOrDefault();

        //    //    int Level1CodeInt = 0;
        //    //    if (int.TryParse(Convert.ToString(LossCodeDetails.LossCodesLevel1ID), out Level1CodeInt))
        //    //    {
        //    //        Level2CodeString = db.tbllossescodes.Where(m => m.LossCodeID == Level1CodeInt).Select(m => m.LossCode).FirstOrDefault();
        //    //    }

        //    //    int Level2CodeInt = 0;
        //    //    if (int.TryParse(Convert.ToString(LossCodeDetails.LossCodesLevel2ID), out Level2CodeInt))
        //    //    {
        //    //        Level2CodeString = db.tbllossescodes.Where(m => m.LossCodeID == Level2CodeInt).Select(m => m.LossCode).FirstOrDefault();
        //    //    }
        //    //    HoldCodesToView.Add(new HoldCodesList { HoldCodeID = LevelCurrentCodeInt, RL1 = Level1CodeString, RL2 = Level2CodeString, RL3 = Level3CodeString, IsWOLevel = 0 });
        //    //}
        //    #endregion

        //    //var HoldCodesToView = new HoldList() { HoldListDetailsWO = WOLossDetails, HoldListDetailsWC = LossDataDetails };
        //    return View(vm);
        //}

        //IdleList
        //public ActionResult IdleList(int id = 0)
        //{
        //    if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
        //    {
        //        return RedirectToAction("Login", "Login", null);
        //    }
        //    ViewBag.Logout = Session["Username"];
        //    ViewBag.roleid = Session["RoleID"];

        //    _conn = new ConnectionFactory();
        //    obj = new Dao(_conn);
        //    string CorrectedDate = null;
        //    //tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
        //    tbldaytiming StartTime = obj.GetDaytimingDetails();
        //    TimeSpan Start = StartTime.StartTime;
        //    if (Start <= DateTime.Now.TimeOfDay)
        //    {
        //        CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
        //    }
        //    else
        //    {
        //        CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        //    }
        //    ViewBag.opid = Session["opid"];
        //    ViewBag.mcnid = id;

        //    string Shift = null;
        //    MsqlConnection mcp = new MsqlConnection();
        //    mcp.open();
        //    String queryshift = "SELECT ShiftName,StartTime,EndTime FROM tblshift_mstr WHERE IsDeleted = 0";
        //    SqlDataAdapter dashift = new SqlDataAdapter(queryshift, mcp.msqlConnection);
        //    DataTable dtshift = new DataTable();
        //    dashift.Fill(dtshift);
        //    String[] msgtime = System.DateTime.Now.TimeOfDay.ToString().Split(':');
        //    TimeSpan msgstime = System.DateTime.Now.TimeOfDay;
        //    //TimeSpan msgstime = new TimeSpan(Convert.ToInt32(msgtime[0]), Convert.ToInt32(msgtime[1]), Convert.ToInt32(msgtime[2]));
        //    TimeSpan s1t1 = new TimeSpan(0, 0, 0), s1t2 = new TimeSpan(0, 0, 0), s2t1 = new TimeSpan(0, 0, 0), s2t2 = new TimeSpan(0, 0, 0);
        //    TimeSpan s3t1 = new TimeSpan(0, 0, 0), s3t2 = new TimeSpan(0, 0, 0), s3t3 = new TimeSpan(0, 0, 0), s3t4 = new TimeSpan(23, 59, 59);
        //    for (int k = 0; k < dtshift.Rows.Count; k++)
        //    {
        //        if (dtshift.Rows[k][0].ToString().Contains("A"))
        //        {
        //            String[] s1 = dtshift.Rows[k][1].ToString().Split(':');
        //            s1t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
        //            String[] s11 = dtshift.Rows[k][2].ToString().Split(':');
        //            s1t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
        //        }
        //        else if (dtshift.Rows[k][0].ToString().Contains("B"))
        //        {
        //            String[] s1 = dtshift.Rows[k][1].ToString().Split(':');
        //            s2t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
        //            String[] s11 = dtshift.Rows[k][2].ToString().Split(':');
        //            s2t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
        //        }
        //        else if (dtshift.Rows[k][0].ToString().Contains("C"))
        //        {
        //            String[] s1 = dtshift.Rows[k][1].ToString().Split(':');
        //            s3t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
        //            String[] s11 = dtshift.Rows[k][2].ToString().Split(':');
        //            s3t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
        //        }
        //    }
        //    CorrectedDate = System.DateTime.Now.ToString("yyyy-MM-dd");
        //    if (msgstime >= s1t1 && msgstime < s1t2)
        //    {
        //        Shift = "A";
        //    }
        //    else if (msgstime >= s2t1 && msgstime < s2t2)
        //    {
        //        Shift = "B";
        //    }
        //    else if ((msgstime >= s3t1 && msgstime <= s3t4) || (msgstime >= s3t3 && msgstime < s3t2))
        //    {
        //        Shift = "C";
        //        if (msgstime >= s3t3 && msgstime < s3t2)
        //        {
        //            CorrectedDate = System.DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        //        }
        //    }
        //    mcp.close();



        //    //checking shift end.

        //    //string SEnd = checkShiftEnd();
        //    //if (SEnd == "yes")
        //    //{
        //    //    return View();
        //    //}

        //    //bool tick = checkingIdle();
        //    //if (tick == true)
        //    //{
        //    //    return RedirectToAction("DownCodeEntry");
        //    //}

        //    //int RotationCount = Convert.ToInt32(Session["Rotation"]);
        //    //if (RotationCount == 0)
        //    //{
        //    //    Session["Rotation"] = 1;
        //    //}
        //    //if (RotationCount == 6)
        //    //{
        //    //    Session["Rotation"] = 1;
        //    //    //return RedirectToAction("Index", "MachineStatus", null);
        //    //}

        //    int handleidleReturnValue = HandleIdle();
        //    if (handleidleReturnValue == 0)
        //    {
        //        return RedirectToAction("DownCodeEntry");
        //    }

        //    //var machinedispname = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == id).Select(m => m.MachineDispName).FirstOrDefault();
        //    var machinedispname = obj.GetMachineDetails1(id);
        //    ViewBag.macDispName = Convert.ToString(machinedispname);

        //    ViewBag.coretddt = CorrectedDate;
        //    //var idle = db.tbllossofentries.Include(t => t.machine_master).Include(t => t.message_code_master).Where(m => m.MachineID == id && m.CorrectedDate == CorrectedDate).ToList();
        //    //var idle = db.tbllivelossofentries.Include(t => t.tbllossescode).Where(m => m.MachineID == id && m.CorrectedDate == CorrectedDate && m.DoneWithRow == 1).OrderByDescending(m => m.StartDateTime).ToList();
        //    var idle = obj.GetLossofEntriesDetails(id, CorrectedDate);
        //    return View(idle);
        //}

        //IdleList
        public ActionResult IdleList(int id = 0)
        {
            Session["split"] = null;
            Session["Mode"] = null;
            //Session["Split"] = null;
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            string CorrectedDate = null, Shift = null;
            string[] DateShiftValues = GetDateShift();
            CorrectedDate = DateShiftValues[1];
            Shift = DateShiftValues[0];
            //int handleidleReturnValue = HandleIdleManualWC();
            //if (handleidleReturnValue == 0)
            //{
            //    return RedirectToAction("DownCodeEntry");
            //}

            //var machinedispname = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == id).Select(m => m.MachineDispName).FirstOrDefault();
            string machinedispname = obj.GetMachineDet1(id);
            ViewBag.macDispName = Convert.ToString(machinedispname);
            HoldList vm = new HoldList();
            ViewBag.mcnid = id;
            ViewBag.coretddt = CorrectedDate;
            //var idle = db.tbllossofentries.Include(t => t.machine_master).Include(t => t.message_code_master).Where(m => m.MachineID == id && m.CorrectedDate == CorrectedDate).ToList();
            //vm.HoldListDetailsWO = db.tblmanuallossofentries.Include(t => t.tblholdcode).Where(m => m.MachineID == id && m.CorrectedDate == CorrectedDate).ToList();
            //vm.HoldListDetailsWC = db.tbllivelossofentries.Include(t => t.tbllossescode).Where(m => m.MachineID == id && m.CorrectedDate == CorrectedDate && m.DoneWithRow == 1).OrderByDescending(m => m.StartDateTime).ToList();
            vm.HoldListDetailsWC = obj.GetLossofEntriesDetails(id, CorrectedDate);

            #region another technique. didnot finish
            ////    HoldCodesList : HoldCodeID  RL1  RL2  RL3  IsWOLevel 
            //List<HoldCodesList> HoldCodesToView = new List<HoldCodesList>();
            //foreach (var row in WOLossDetails)
            //{
            //    int LevelCurrentCodeInt = row.MessageCodeID;
            //    string Level1CodeString = row.MessageCode;
            //    string Level2CodeString = null, Level3CodeString = null;

            //    var LossCodeDetails = db.tbllossescodes.Where(m => m.LossCodeID == LevelCurrentCodeInt).FirstOrDefault();

            //    int Level1CodeInt = 0;
            //    if (int.TryParse(Convert.ToString(LossCodeDetails.LossCodesLevel1ID), out Level1CodeInt))
            //    {
            //        Level2CodeString = db.tbllossescodes.Where(m => m.LossCodeID == Level1CodeInt).Select(m => m.LossCode).FirstOrDefault();
            //    }

            //    int Level2CodeInt = 0;
            //    if (int.TryParse(Convert.ToString(LossCodeDetails.LossCodesLevel2ID), out Level2CodeInt))
            //    {
            //        Level2CodeString = db.tbllossescodes.Where(m => m.LossCodeID == Level2CodeInt).Select(m => m.LossCode).FirstOrDefault();
            //    }

            //    HoldCodesToView.Add(new HoldCodesList { HoldCodeID = LevelCurrentCodeInt , RL1 = Level1CodeString , RL2 = Level2CodeString , RL3 = Level3CodeString ,  IsWOLevel = 1 });
            //}

            //var LossDataDetails = db.tbllossofentries.Where(m => m.MachineID == id && m.CorrectedDate == CorrectedDate).ToList();
            //foreach (var row in LossDataDetails)
            //{
            //    int LevelCurrentCodeInt = row.MessageCodeID;
            //    string Level1CodeString = row.MessageCode;
            //    string Level2CodeString = null, Level3CodeString = null;

            //    var LossCodeDetails = db.tbllossescodes.Where(m => m.LossCodeID == LevelCurrentCodeInt).FirstOrDefault();

            //    int Level1CodeInt = 0;
            //    if (int.TryParse(Convert.ToString(LossCodeDetails.LossCodesLevel1ID), out Level1CodeInt))
            //    {
            //        Level2CodeString = db.tbllossescodes.Where(m => m.LossCodeID == Level1CodeInt).Select(m => m.LossCode).FirstOrDefault();
            //    }

            //    int Level2CodeInt = 0;
            //    if (int.TryParse(Convert.ToString(LossCodeDetails.LossCodesLevel2ID), out Level2CodeInt))
            //    {
            //        Level2CodeString = db.tbllossescodes.Where(m => m.LossCodeID == Level2CodeInt).Select(m => m.LossCode).FirstOrDefault();
            //    }
            //    HoldCodesToView.Add(new HoldCodesList { HoldCodeID = LevelCurrentCodeInt, RL1 = Level1CodeString, RL2 = Level2CodeString, RL3 = Level3CodeString, IsWOLevel = 0 });
            //}
            #endregion

            //var HoldCodesToView = new HoldList() { HoldListDetailsWO = WOLossDetails, HoldListDetailsWC = LossDataDetails };
            return View(vm);
        }

        public ActionResult BreakDownEntry(int id = 0, int Bid = 0)
        {
            Session["redStrat"] = 0;
            Session["redEnd"] = 0;
            Session["split"] = null;
            //Session["Split"] = null;
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);
            #region old
            ////if tblmode or tbldailyprodstatus has breakdown in them for today then update tblbreakdown now.
            ////will help in showing options on entry screen
            ////this happens because we are taking last available mode for that machine and updating it for today if we don't have mode for now.

            //var tblbreakdown = db.tblbreakdowns.Where(m => m.MachineID == id).OrderByDescending(m => m.StartTime);
            //foreach (var row in tblbreakdown)
            //{
            //    string date = row.CorrectedDate;
            //    string today = DateTime.Now.ToString("yyyy-MM-dd" );
            //    if (date != today)
            //    {
            //        var tblmode = db.tblmodes.Where(m => m.MachineID == id && m.IsDeleted == 0).OrderByDescending(m => m.InsertedOn);
            //        foreach (var rowIntblmode in tblmode)
            //        {
            //            if (rowIntblmode.Mode == "BREAKDOWN")
            //            {
            //                DateTime insertedOn = rowIntblmode.InsertedOn;

            //            }

            //            break;
            //        }

            //    }
            //    break;
            //}
            //doing this in daq is better
            #endregion

            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            Session["Mid"] = id;
            int machineid = Convert.ToInt32(Session["MachineID"]);
            string modedet = GetMode(machineid);
            if (modedet != "PowerOn")
            {
                string CorrectedDate = null;
                //tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
                tbldaytiming StartTime = obj.GetDaytimingDetails();
                TimeSpan Start = StartTime.StartTime;
                if (Start <= DateTime.Now.TimeOfDay)
                {
                    CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
                }
                else
                {
                    CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                }

                //var machinedispname = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == id).Select(m => m.MachineDispName).FirstOrDefault();
                string machinedispname = obj.GetMachineDet1(id);
                ViewBag.macDispName = Convert.ToString(machinedispname);

                //Stage 1: check if we r allowd to set this mode 
                //CODE to check the current mode is allowable or not , based on MODE Priority.
                //var curMode = db.tblbreakdowns.Where(m => m.MachineID == id && m.DoneWithRow == 0).OrderByDescending(m => m.BreakdownID).Take(1).ToList();
                List<tblbreakdown> curMode = obj.GetBreakDownDetails1(id);
                int currentId = 0;
                foreach (tblbreakdown j in curMode)
                {
                    currentId = j.BreakdownID;
                    tbllossescode loss = obj.GetLossDet(j.BreakDownCode);
                    string mode = loss.MessageType;

                    if (mode == "PM")
                    {
                        Session["ModeError"] = "Machine is in Maintenance , cannot change mode to Breakdown";
                        return RedirectToAction("Index");
                    }



                    //else if (mode == "BREAKDOWN")
                    //{
                    //    Session["ModeError"] = "Machine is in Breakdown Mode";
                    //    return RedirectToAction("Index");
                    //}
                    else if (mode != "BREAKDOWN")
                    {
                        //tblbreakdown tbd = db.tblbreakdowns.Find(currentId);
                        tblbreakdown tbd = obj.GetBreakDownDetails3(currentId);
                        tbd.EndTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        //db.Entry(tbd).State = System.Data.Entity.EntityState.Modified;
                        //db.SaveChanges();
                        obj.UpdateBreakDownDetails(tbd.BreakdownID, Convert.ToDateTime(tbd.StartTime), Convert.ToDateTime(tbd.EndTime), Convert.ToInt32(tbd.BreakDownCode), tbd.MachineID, tbd.CorrectedDate, tbd.Shift, tbd.MessageDesc, tbd.MessageCode, Convert.ToInt32(tbd.DoneWithRow));
                        //tbllossofentry tle = 
                        break;
                    }
                }


                //stage 2. Breakdown is running and u need to send data to view regarding that.

                //var breakdownToView = db.tblbreakdowns.Where(m => m.MachineID == machineid && m.DoneWithRow == 0).OrderByDescending(m => m.BreakdownID).FirstOrDefault();
                tblbreakdown breakdownToView = obj.GetBreakDownDetails2(machineid);
                if (breakdownToView != null) //implies brekdown is running
                {
                    if (breakdownToView.DoneWithRow == 0)
                    {
                        int breakdowncode = Convert.ToInt32(breakdownToView.BreakDownCode);
                        //var DataToView = db.tbllossescodes.Where(m => m.LossCodeID == breakdowncode).ToList();
                        List<tbllossescode> DataToView = obj.GetLossCodeDetail8(breakdowncode);
                        ViewBag.Level = DataToView[0].LossCodesLevel;
                        ViewBag.BreakdownCode = DataToView[0].LossCode;
                        ViewBag.BreakdownId = DataToView[0].LossCodeID;
                        ViewBag.BreakdownStartTime = breakdownToView.StartTime;
                        return View(DataToView);
                    }

                }


                //var brkdown = db.tblbreakdowns.Where(m => m.MachineID == id).Where(m => m.CorrectedDate == CorrectedDate && m.EndTime == null && m.message_code_master.MessageType == "BREAKDOWN");
                //if (brkdown.Count() != 0)
                //{
                //    Session["ItsBreakDown"] = "yes";
                //    int brekdnID = 0;
                //    foreach (var a in brkdown)
                //    {
                //        brekdnID = a.BreakdownID;
                //    }
                //    tblbreakdown brekdn = db.tblbreakdowns.Find(brekdnID);
                //    CheckLastOneHourDownTime(id);
                //    ViewBag.BreakDownCode = new SelectList(db.message_code_master.Where(m => m.IsDeleted == 0).Where(m => m.MessageType == "BREAKDOWN"), "MessageCodeID", "MessageDescription", brekdn.BreakDownCode);
                //    return View(brekdn);
                //}
                //else
                //{

                //}

                //This is needed but not now.
                //CheckLastOneHourDownTime(id);


                //stage 3. Operator is selecting the breakdown by traversing down the Hierarchy of BreakdownCodes.
                if (Bid != 0)
                {
                    //var breakdata = db.tbllossescodes.Find(Bid);
                    tbllossescode breakdata = obj.GetLossCodeDetails(Bid);
                    int level = breakdata.LossCodesLevel;
                    string breakdowncode = breakdata.LossCode;

                    if (level == 1)
                    {
                        //var level2Data = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel1ID == Bid && m.LossCodesLevel2ID == null && m.MessageType == "BREAKDOWN").ToList();
                        List<tbllossescode> level2Data = obj.GetLossCodeDetails7(Bid); //added breakdown cond
                        if (level2Data.Count == 0)
                        {
                            //var level1Data = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.LossCodesLevel1ID == null && m.LossCodesLevel2ID == null && m.MessageType == "BREAKDOWN").ToList();
                            List<tbllossescode> level1Data = obj.GetLossCodeDetails8();
                            ViewBag.ItsLastLevel = "No Further Levels . Do you want to set " + breakdowncode + " as reason.";
                            ViewBag.BreakDownID = Bid;
                            ViewBag.Level = level;
                            ViewBag.breadScrum = breakdowncode + "-->  ";
                            return View(level1Data);
                        }
                        ViewBag.Level = level + 1;
                        ViewBag.BreakDownID = Bid;
                        ViewBag.breadScrum = breakdowncode + "-->  ";
                        return View(level2Data);
                    }
                    else if (level == 2)
                    {
                        //var level3Data = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel2ID == Bid && m.MessageType == "BREAKDOWN").ToList();
                        List<tbllossescode> level3Data = obj.GetLossCodeDetails10(Bid); //added breakdown cond
                        int prevLevelId = Convert.ToInt32(breakdata.LossCodesLevel1ID);
                        //var level1data = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodeID == prevLevelId).Select(m => m.LossCode).FirstOrDefault();
                        string level1data = obj.GetLossCodeDetails4(prevLevelId);
                        if (level3Data.Count == 0)
                        {
                            //var level2Data = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel1ID == prevLevelId && m.MessageType == "BREAKDOWN" && m.LossCodesLevel2ID == null).ToList();
                            List<tbllossescode> level2Data = obj.GetLossCodeDetails7(prevLevelId); //added breakdown cond
                            ViewBag.ItsLastLevel = "No Further Levels . Do you want to set " + breakdowncode + " as reason.";
                            ViewBag.BreakDownID = Bid;
                            ViewBag.Level = level;
                            ViewBag.breadScrum = level1data + " --> " + breakdowncode + "-->  ";
                            return View(level2Data);
                        }
                        ViewBag.Level = level + 1;
                        ViewBag.BreakDownID = Bid;
                        ViewBag.breadScrum = level1data + " --> " + breakdowncode + "-->  ";
                        return View(level3Data);
                    }
                    else if (level == 3)
                    {
                        int prevLevelId = Convert.ToInt32(breakdata.LossCodesLevel2ID);
                        int FirstLevelID = Convert.ToInt32(breakdata.LossCodesLevel1ID);
                        //var level2scrum = db.tbllossescodes.Where(m => m.LossCodeID == prevLevelId).Select(m => m.LossCode).FirstOrDefault();
                        string level2scrum = obj.GetLossCodeDetails4(prevLevelId);
                        //var level1scrum = db.tbllossescodes.Where(m => m.LossCodeID == FirstLevelID).Select(m => m.LossCode).FirstOrDefault();
                        string level1scrum = obj.GetLossCodeDetails4(FirstLevelID);
                        //var level2Data = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel2ID == prevLevelId && m.MessageType == "BREAKDOWN").ToList();
                        List<tbllossescode> level2Data = obj.GetLossCodeDetails10(prevLevelId);
                        ViewBag.ItsLastLevel = "No Further Levels . Do you want to set " + breakdowncode + " as reason.";
                        ViewBag.BreakDownID = Bid;
                        ViewBag.Level = 3;
                        ViewBag.breadScrum = level1scrum + " --> " + level2scrum + "--> ";
                        return View(level2Data);
                    }
                }
                else
                {
                    //var level1Data = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.MessageType == "BREAKDOWN" && m.LossCode != "9999").ToList();
                    List<tbllossescode> level1Data = obj.GetLossCodeDetails9(); //added breakdown cond
                    ViewBag.Level = 1;
                    return View(level1Data);
                }

                //Fail Safe: if everything else fails send level1 codes.
                ViewBag.Level = 1;
                //var level10Data = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.MessageType == "BREAKDOWN" && m.LossCode != "9999").ToList();
                List<tbllossescode> level10Data = obj.GetLossCodeDetails9();
                return View(level10Data);
            }
            else
            {
                Session["Mode"] = 1;
                return RedirectToAction("Index");
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BreakDownEntry(tbllossescode tbdc, string EndBreakdown = null, int HiddenID = 0)
        {
            Session["split"] = null;
            Session["redStrat"] = null;
            Session["redEnd"] = null;
            //"EndBreakdown" is for insert new row or update old one. Basically speeking its like start and Stop of Breakdown.
            //"HiddenID" is the BreakdownID of row to be set as reason.
            int MachineID = Convert.ToInt32(Session["MachineID"]);
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            int RID = Convert.ToInt32(Session["RoleID"]);
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);
            string CorrectedDate = null;
            //tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
            tbldaytiming StartTime = obj.GetDaytimingDetails();
            TimeSpan Start = StartTime.StartTime;
            if (Start <= DateTime.Now.TimeOfDay)
            {
                CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }

            DateTime Time = DateTime.Now;
            TimeSpan Tm = new TimeSpan(Time.Hour, Time.Minute, Time.Second);
            //var ShiftDetails = db.tblshift_mstr.Where(m => m.StartTime <= Tm && m.EndTime >= Tm);
            tblshift_mstr ShiftDetails = obj.GetShiftDetails(Tm);
            string Shift = "C";
            int shiftid = 1;
            if (ShiftDetails != null)
            {
                Shift = ShiftDetails.ShiftName;
            }

            if (Shift == "A")
            {
                shiftid = 1;
            }
            else if (Shift == "B")
            {
                shiftid = 2;
            }
            else if (Shift == "C")
            {
                shiftid = 3;
            }

            if (HiddenID != 0 && string.IsNullOrEmpty(EndBreakdown) == true)
            {
                //var breakdata = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodeID == HiddenID).FirstOrDefault();
                tbllossescode breakdata = obj.GetLossCodeDetails43(HiddenID);
                string msgCode = breakdata.LossCode;
                string msgDesc = breakdata.LossCodeDesc;
                obj.InsertBreakDownDetails(HiddenID, CorrectedDate, 0, Convert.ToInt32(Session["MachineID"]), msgCode, msgDesc, Shift, DateTime.Now);
                //Code to End PreviousMode(Production Here) & save this event to tblmode table
                //var modedata = db.tbllivemodedbs.Where(m => m.MachineID == MachineID && m.IsCompleted == 0).OrderByDescending(m => m.StartTime).FirstOrDefault();
                tbllivemode modedata = obj.GetLiveModeDetails1(MachineID);
                if (modedata != null)
                {
                    double diff = DateTime.Now.Subtract(Convert.ToDateTime(modedata.StartTime)).TotalSeconds;
                    obj.UpdateLiveModeDetails(modedata.ModeID, DateTime.Now, diff);
                }
                obj.InsertLiveModeDetails(Convert.ToInt32(Session["MachineID"]), CorrectedDate, Convert.ToInt32(Session["UserId"]), DateTime.Now, "RED", shiftid, "BREAKDOWN", DateTime.Now, 0, "BREAKDOWN", 0);
                Session["redStrat"] = 1;
                Session["redEnd"] = 0;
            }
            else if (HiddenID != 0 && string.IsNullOrEmpty(EndBreakdown) == false)
            {
                //var tb = db.tblbreakdowns.Where(m => m.BreakDownCode == HiddenID && m.MachineID == MachineID && m.DoneWithRow == 0).OrderByDescending(m => m.BreakdownID).FirstOrDefault();
                tblbreakdown tb = obj.GetBreakDownDetails4(HiddenID, MachineID);
                obj.UpdateBreakDownDetails(tb.BreakdownID, DateTime.Now);

                //get the latest row and update it.
                //var modedata = db.tbllivemodedbs.Where(m => m.MachineID == MachineID && m.IsCompleted == 0).OrderByDescending(m => m.StartTime).FirstOrDefault();
                tbllivemode modedata = obj.GetLiveModeDetails1(MachineID);
                if (modedata != null)
                {
                    double diff = DateTime.Now.Subtract(Convert.ToDateTime(modedata.StartTime)).TotalSeconds;
                    obj.UpdateLiveModeDetails(modedata.ModeID, DateTime.Now, diff);
                }
                obj.InsertLiveModeDetails(MachineID, CorrectedDate, Convert.ToInt32(Session["UserId"]), DateTime.Now, "YELLOW", shiftid, "IDLE", DateTime.Now, 0, "IDLE", 0);
                Session["redEnd"] = 1;
                Session["redStrat"] = 0;
            }

            #region OLD
            //if (string.IsNullOrEmpty(submit) == false)
            //{
            //    lossentry.CorrectedDate = CorrectedDate;
            //    lossentry.StartTime = DateTime.Now;
            //    if (RID != 1 && RID != 2)
            //    {
            //        lossentry.MachineID = Convert.ToInt32(Session["MachineID"]);
            //        MachineID = Convert.ToInt32(Session["MachineID"]);
            //    }
            //    else
            //    {
            //        lossentry.MachineID = Convert.ToInt32(Session["Mid"]);
            //        MachineID = Convert.ToInt32(Session["Mid"]);
            //    }
            //    message_code_master downcode = db.message_code_master.Find(lossentry.BreakDownCode);
            //    //lossentry.BreakDownCode = Convert.ToInt32(downcode.MessageCode);
            //    lossentry.Shift = Session["realshift"].ToString();
            //    //lossentry.BreakDownCode =Convert.ToInt32(downcode.MessageCode);
            //    lossentry.MessageCode = (downcode.MessageCode).ToString();
            //    db.tblbreakdowns.Add(lossentry);
            //    db.SaveChanges();

            //    //update the endtime for the last mode of this machine 
            //    var tblmodedata = db.tblmodes.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID).OrderByDescending(m => m.StartTime).ToList();
            //    foreach (var row in tblmodedata)
            //    {
            //        row.EndTime = DateTime.Now;
            //        db.Entry(row).State = System.Data.Entity.EntityState.Modified;
            //        db.SaveChanges();
            //    }

            //    //Code to save this event to tblmode table
            //    tblmode tm = new tblmode();
            //    tm.MachineID = MachineID;
            //    tm.CorrectedDate = CorrectedDate;
            //    tm.InsertedBy = 1;
            //    tm.StartTime = DateTime.Now;
            //    tm.ColorCode = "red";
            //    tm.InsertedOn = DateTime.Now;

            //    tm.IsDeleted = 0;
            //    tm.Mode = "BREAKDOWN";

            //    db.tblmodes.Add(tm);
            //    db.SaveChanges();


            //    //SendMail(downcode.MessageCode, downcode.MessageDescription, MachineID);
            //    return RedirectToAction("Index");
            //}
            //else
            //{
            //    lossentry.CorrectedDate = CorrectedDate;
            //    lossentry.EndTime = DateTime.Now;
            //    MachineID = Convert.ToInt32(lossentry.MachineID);
            //    //lossentry.MachineID = Convert.ToInt32(Session["MachineID"]);
            //    db.Entry(lossentry).State = System.Data.Entity.EntityState.Modified;
            //    db.SaveChanges();
            //    UpdateRecordOfProduction(lossentry);
            //    int code = Convert.ToInt32(lossentry.BreakDownCode);
            //    message_code_master msg = db.message_code_master.Where(m => m.MessageCodeID == code).FirstOrDefault();
            //    //SendMailEnd(msg.MessageCode, msg.MessageDescription, MachineID);
            //    return RedirectToAction("Index");
            //}
            ////return View(lossentry);
            #endregion
            return RedirectToAction("Index");
        }

        public string GetMode(int MachineID)
        {
            string res = "";
            tbllivemode modedata = obj.GetLiveModeDetails1(MachineID);
            if (modedata != null)
            {
                string mode = modedata.Mode;
                res = mode;
            }

            return res;
        }

        public string GetModeCheck(int MachineID)
        {
            string res = "";
            string CorrectedDate = null;
            tbldaytiming StartTime = obj.GetDaytimingDetails();
            TimeSpan Start = StartTime.StartTime;
            if (Start >= DateTime.Now.TimeOfDay)
            {
                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");

                tbllivemode modedata = obj.GetLiveModeDetails1(MachineID);
                if (modedata != null)
                {
                    string mode = modedata.Mode;
                    res = mode;
                }
            }
            return res;
        }

        public void UpdateRecordOfProduction(tblbreakdown a)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            string CorrectedDate = null;
            tbldaytiming StartTime = obj.GetDaytimingDetails();
            //tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
            TimeSpan Start = StartTime.StartTime;
            if (Start <= DateTime.Now.TimeOfDay)
            {
                CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }
            List<tbldailyprodstatu> oldData = obj.GettbllivedailyprodstatusList2(CorrectedDate, a.StartTime, a.EndTime, a.MachineID);
            // var oldData = db.tbldailyprodstatus.Where(m => m.CorrectedDate == CorrectedDate).Where(m => m.StartTime >= a.StartTime).Where(m => m.EndTime <= a.EndTime).Where(m => m.MachineID == a.MachineID);
            if (oldData != null)
            {
                if (ModelState.IsValid)
                {
                    foreach (tbldailyprodstatu newdata in oldData)
                    {
                        int id = newdata.ID;
                        obj.UpdateprodsttausDetails(id);
                        //newdata.ColorCode = "yellow";
                        //db.Entry(newdata).State = System.Data.Entity.EntityState.Modified;
                        //db.SaveChanges();
                    }
                }
            }
        }

        public bool SendMail(string messagecode, string messagedescription, int MachineID)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            tblmachinedetail machin = obj.GetMachineDet(MachineID);
            // tblmachinedetail machin = db.tblmachinedetails.Find(MachineID);
            string MachineName = machin.MachineDisplayName;
            MailMessage mail = new MailMessage();

            //mail.To.Add(new MailAddress("pavan.v@srkssolutions.com"));
            //mail.To.Add(new MailAddress("deepak.Jojode@srkssolutions.com"));

            //mail.CC.Add(new MailAddress("srinidhi.kashyap@srkssolutions.com"));
            mail.To.Add(new MailAddress("janardhan.g@srkssolutions.com"));

            //mail.Bcc.Add(new MailAddress("narendra.kumar@srkssolutions.com"));
            //mail.To.Add(new MailAddress("pskumar@tataadvancedsystems.com"));
            //mail.CC.Add(new MailAddress("bpdesai@tataadvancedsystems.com"));
            //mail.CC.Add(new MailAddress("vkasinath@tataadvancedsystems.com"));
            //mail.CC.Add(new MailAddress("vsanghavi@i_facility.com"));
            //mail.CC.Add(new MailAddress("sgopu@tataadvancedsystems.com"));
            //mail.CC.Add(new MailAddress("soumyaagrawal@tataadvancedsystems.com"));
            //mail.CC.Add(new MailAddress("pkbhanja@tataadvancedsystems.com"));
            //mail.CC.Add(new MailAddress("tshabareesan@tataadvancedsystems.com"));
            //mail.CC.Add(new MailAddress("mmrafi@tataadvancedsystems.com"));
            //mail.Bcc.Add(new MailAddress("pavan.v@srkssolutions.com"));
            //mail.Bcc.Add(new MailAddress("narendramourya@live.com"));
            //mail.Bcc.Add(new MailAddress("srinidhi.kashyap@srkssolutions.com"));
            //mail.Bcc.Add(new MailAddress("deepak.Jojode@srkssolutions.com"));

            mail.From = new MailAddress("narendramourya37@gmail.com");
            mail.Subject = MachineName + " BreakDown Alert";
            mail.IsBodyHtml = true;
            mail.Body = "<p><b>Dear Concerned,</b></p>" +
                        "<b></b>" +
                        "<p><b>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; This is to inform you that machine " + MachineName + " has gone into Breakdown for " + messagecode + "  ," + messagedescription + "  &nbsp;<span>.</b></p>" +
                        "<p><b><br/><br/>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  Note: This Email has been sent for the demo purpose of Andon Display. &nbsp;<span>.</b></p>" +
                        "<p><b></b></p>";

            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new System.Net.NetworkCredential("narendramourya37@gmail.com", "8103097561");
            smtp.EnableSsl = true;
            smtp.Send(mail);
            return true;
        }

        public bool SendMailEnd(string messagecode, string messagedescription, int MachineID)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            tblmachinedetail machin = obj.GetMachineDet(MachineID);
            //tblmachinedetail machin = db.tblmachinedetails.Find(MachineID);
            string MachineName = machin.MachineDisplayName;
            MailMessage mail = new MailMessage();

            //mail.To.Add(new MailAddress("pavan.v@srkssolutions.com"));
            //mail.To.Add(new MailAddress("deepak.Jojode@srkssolutions.com"));

            //mail.CC.Add(new MailAddress("srinidhi.kashyap@srkssolutions.com"));
            mail.To.Add(new MailAddress("janardhan.g@srkssolutions.com"));

            //mail.Bcc.Add(new MailAddress("narendra.kumar@srkssolutions.com"));
            //mail.To.Add(new MailAddress("pskumar@tataadvancedsystems.com"));
            //mail.CC.Add(new MailAddress("bpdesai@tataadvancedsystems.com"));
            //mail.CC.Add(new MailAddress("vkasinath@tataadvancedsystems.com"));
            //mail.CC.Add(new MailAddress("vsanghavi@i_facility.com"));
            //mail.CC.Add(new MailAddress("sgopu@tataadvancedsystems.com"));
            //mail.CC.Add(new MailAddress("soumyaagrawal@tataadvancedsystems.com"));
            //mail.CC.Add(new MailAddress("pkbhanja@tataadvancedsystems.com"));
            //mail.CC.Add(new MailAddress("tshabareesan@tataadvancedsystems.com"));
            //mail.CC.Add(new MailAddress("mmrafi@tataadvancedsystems.com"));
            //mail.Bcc.Add(new MailAddress("pavan.v@srkssolutions.com"));
            //mail.Bcc.Add(new MailAddress("narendramourya@live.com"));
            //mail.Bcc.Add(new MailAddress("srinidhi.kashyap@srkssolutions.com"));
            //mail.Bcc.Add(new MailAddress("deepak.Jojode@srkssolutions.com"));

            mail.From = new MailAddress("narendramourya37@gmail.com");
            mail.Subject = MachineName + " BreakDown Alert";
            mail.IsBodyHtml = true;
            mail.Body = "<p><b>Dear Concerned,</b></p>" +
                        "<b></b>" +
                        "<p><b>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; This is to inform you that machine " + MachineName + " has been fixed for the Breakdown for " + messagecode + "  ," + messagedescription + " and is now available for production &nbsp;<span>.</b></p>" +
                        "<p><b><br/><br/>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  Note: This Email has been sent for the demo purpose of Andon Display. &nbsp;<span>.</b></p>" +
                        "<p><b></b></p>";
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new System.Net.NetworkCredential("narendramourya37@gmail.com", "8103097561");
            smtp.EnableSsl = true;
            smtp.Send(mail);
            return true;
        }

        public bool CheckLastOneHourDownTime(int MachineID)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            #region DownColor
            int count = 0;
            int ContinuesChecking = 0;
            List<tbldailyprodstatu> productionstatus = obj.GettbllivedailyprodstatusList1(MachineID);
            //var productionstatus = db.tbldailyprodstatus.Where(m => m.MachineID == MachineID).OrderByDescending(m => m.StartTime);
            foreach (tbldailyprodstatu check in productionstatus)
            {
                if (check.ColorCode == "yellow")
                {
                    count++;
                    if (count == 60)
                    {
                        break;
                    }
                }
                else
                {
                    count = 0;
                }
                ContinuesChecking++;
            }
            #endregion
            if (count >= 60 && ContinuesChecking < 61)
            {
                tblmachinedetail machin = obj.GetMachineDet(MachineID);
                //tblmachinedetail machin = db.tblmachinedetails.Find(MachineID);
                string MachineName = machin.MachineDisplayName;
                MailMessage mail = new MailMessage();

                //mail.To.Add(new MailAddress("pavan.v@srkssolutions.com"));


                //mail.CC.Add(new MailAddress("srinidhi.kashyap@srkssolutions.com"));
                mail.To.Add(new MailAddress("janardhan.g@srkssolutions.com"));

                //mail.Bcc.Add(new MailAddress("narendra.kumar@srkssolutions.com"));

                //mail.To.Add(new MailAddress("pskumar@tataadvancedsystems.com"));
                //mail.CC.Add(new MailAddress("bpdesai@tataadvancedsystems.com"));
                //mail.CC.Add(new MailAddress("vkasinath@tataadvancedsystems.com"));
                //mail.CC.Add(new MailAddress("vsanghavi@i_facility.com"));
                //mail.CC.Add(new MailAddress("sgopu@tataadvancedsystems.com"));
                //mail.CC.Add(new MailAddress("soumyaagrawal@tataadvancedsystems.com"));
                //mail.CC.Add(new MailAddress("pkbhanja@tataadvancedsystems.com"));
                //mail.CC.Add(new MailAddress("tshabareesan@tataadvancedsystems.com"));
                //mail.CC.Add(new MailAddress("mmrafi@tataadvancedsystems.com"));
                //mail.Bcc.Add(new MailAddress("pavan.v@srkssolutions.com"));
                //mail.Bcc.Add(new MailAddress("narendramourya@live.com"));
                //mail.Bcc.Add(new MailAddress("srinidhi.kashyap@srkssolutions.com"));
                //mail.Bcc.Add(new MailAddress("deepak.Jojode@srkssolutions.com"));


                mail.From = new MailAddress("narendramourya37@gmail.com");
                mail.Subject = MachineName + " Setup Mode";
                mail.IsBodyHtml = true;
                mail.Body = "<p><b>Dear Concerned,</b></p>" +
                            "<b></b>" +
                            "<p><b>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  This is to inform you that machine Nexus " + MachineName + " has crossed an Hour being under setup. &nbsp;<span>.</b></p>" +
                            "<p><b><br/><br/>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  Note: This Email has been sent for the demo purpose of Andon Display. &nbsp;<span>.</b></p>" +
                            "<p><b></b></p>";
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential("narendramourya37@gmail.com", "8103097561");
                smtp.EnableSsl = true;
                smtp.Send(mail);
            }
            return true;
        }

        //code to refresh checking idle list breakdownlist
        public bool checkingIdle()
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            Session["idlestarttime"] = null;
            bool tick = false;
            //getting CorrectedDate
            string CorrectedDate = null;
            tbldaytiming StartTime = obj.GetDaytimingDetails();
            //tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
            TimeSpan Start = StartTime.StartTime;
            if (Start <= DateTime.Now.TimeOfDay)
            {
                CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }

            int Machinid = Convert.ToInt32(Session["MachineID"]);

            tbllivelossofentry DowncodeEntryTime = obj.GettbllivelossofentriesDetails(CorrectedDate, Machinid);
            // tbllivelossofentry DowncodeEntryTime = db.tbllivelossofentries.Where(m => m.CorrectedDate == CorrectedDate && m.MachineID == Machinid).OrderByDescending(m => m.EntryTime).FirstOrDefault();
            DateTime EnteryTime = DateTime.Now;
            if (DowncodeEntryTime != null)
            {
                EnteryTime = Convert.ToDateTime(DowncodeEntryTime.EntryTime);
                int TotalMinute = 0;
                TotalMinute = System.DateTime.Now.Subtract(EnteryTime).Minutes;
                if (TotalMinute >= 3)
                {
                    #region DownColor
                    int count = 0;
                    int ContinuesChecking = 0;
                    List<tbllivedailyprodstatu> productionstatus = obj.GettbllivedailyprodstatusList(CorrectedDate, Machinid);
                    //var productionstatus = db.tbllivedailyprodstatus.Where(m => m.CorrectedDate == CorrectedDate && m.MachineID == Machinid).OrderByDescending(m => m.StartTime);
                    foreach (tbllivedailyprodstatu check in productionstatus)
                    {
                        if (check.ColorCode == "yellow")
                        {
                            count++;
                            if (count == 2)
                            {
                                break;
                            }
                        }
                        else
                        {
                            count = 0;
                        }
                        ContinuesChecking++;
                    }
                    if (count >= 2 && ContinuesChecking < 5)
                    {
                        tick = true;
                    }
                    #endregion
                }
            }
            else
            {
                #region DownColor
                int count = 0;
                int ContinuesChecking = 0;
                List<tbllivedailyprodstatu> productionstatus = obj.GettbllivedailyprodstatusList(CorrectedDate, Machinid);
                //var productionstatus = db.tbllivedailyprodstatus.Where(m => m.CorrectedDate == CorrectedDate && m.MachineID == Machinid).OrderByDescending(m => m.StartTime);
                foreach (tbllivedailyprodstatu check in productionstatus)
                {
                    if (check.ColorCode == "yellow")
                    {
                        count++;
                        if (count == 2)
                        {
                            break;
                        }
                    }
                    else
                    {
                        count = 0;
                    }
                    ContinuesChecking++;
                }
                if (count >= 2 && ContinuesChecking < 5)
                {
                    tick = true;
                }
                #endregion
            }
            return tick;
        }

        //Check for ShiftEnd.
        public JsonResult checkShiftEnd(string rep)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            string isShiftEnd = "no";

            DateTime dt = DateTime.Now;
            TimeSpan tm = new TimeSpan(dt.Hour, dt.Minute, dt.Second);
            List<shift_master> shiftstatus = obj.GetShiftList(tm);
            //var shiftstatus = db.shift_master.Where(m => m.EndTime >= tm && m.StartTime <= tm).OrderBy(m => m.StartTime);
            string shiftfor = "someshift";
            foreach (shift_master check in shiftstatus)
            {
                shiftfor = check.ShiftName;
            }

            if (shiftfor == "Shift1")
            {
                shiftfor = "A";
            }
            else if (shiftfor == "Shift2")
            {
                shiftfor = "B";
            }
            else if (shiftfor == "Shift3")
            {
                shiftfor = "C";
            }

            string shiftforpop = Session["shiftforpopup"].ToString();
            if (shiftforpop != shiftfor)
            {
                isShiftEnd = "yes";
                Session["shiftforpopup"] = shiftfor;
            }

            string json = JsonConvert.SerializeObject(isShiftEnd);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        //WorkInProgressList
        public ActionResult WorkInProgressList(int id = 0)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            string CorrectedDate = null;
            tbldaytiming StartTime = obj.GetDaytimingDetails();
            // tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
            TimeSpan Start = StartTime.StartTime;
            if (Start <= DateTime.Now.TimeOfDay)
            {
                CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }
            ViewBag.opid = Session["opid"];
            ViewBag.mcnid = id;
            ViewBag.coretddt = CorrectedDate;

            //bool tick = checkingIdle();
            //if (tick == true)
            //{
            //    return RedirectToAction("DownCodeEntry");
            //    //ViewBag.tick = 1;
            //}

            int handleidleReturnValue = HandleIdle();
            if (handleidleReturnValue == 0)
            {
                return RedirectToAction("DownCodeEntry");
            }
            string machinedispname = obj.GetMachineDet1(id);
            //var machinedispname = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == id).Select(m => m.MachineDispName).FirstOrDefault();
            ViewBag.macDispName = Convert.ToString(machinedispname);

            //var breakdown = db.tblbreakdowns.Include(t=>t.machine_master).Include(t=>t.message_code_master).Where(m=>m.MachineID==id && m.CorrectedDate==CorrectedDate).ToList();
            List<tbllivehmiscreen> WIP = obj.GettbllivehmiscreensList(id, CorrectedDate);
            //var WIP = db.tbllivehmiscreens.Include(t => t.tblmachinedetail).Where(m => m.MachineID == id && m.CorrectedDate == CorrectedDate && m.isWorkInProgress == 0).ToList();
            return View(WIP);
        }

        //public ActionResult Setting(int id = 0)
        //{
        //    _conn = new ConnectionFactory();
        //    obj = new Dao(_conn);

        //    if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
        //    {
        //        return RedirectToAction("Login", "Login", null);
        //    }

        //    ViewBag.Logout = Session["Username"];
        //    ViewBag.roleid = Session["RoleID"];
        //    Session["Mid"] = id;
        //    string CorrectedDate = null;
        //    tbldaytiming StartTime = obj.GetDaytimingDetails();
        //    //tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
        //    TimeSpan Start = StartTime.StartTime;
        //    if (Start <= DateTime.Now.TimeOfDay)
        //    {
        //        CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
        //    }
        //    else
        //    {
        //        CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        //    }

        //    //CODE to check the current mode is allowable or not , based on MODE Priority.
        //    var curMode = obj.GetBreakdownList(id, CorrectedDate);
        //    //var curMode = db.tblbreakdowns.Where(m => m.MachineID == id).Where(m => m.CorrectedDate == CorrectedDate && m.EndTime == null).OrderByDescending(m => m.BreakdownID);
        //    int currentId = 0;

        //    foreach (var j in curMode)
        //    {
        //        currentId = j.BreakdownID;
        //        string mode = j.tbllossescode.MessageType;

        //        if (mode == "PM" || mode == "BREAKDOWN")
        //        {
        //            Session["ModeError"] = "Machine is in " + mode + ", cannot change mode to Setting";
        //            return RedirectToAction("Index");
        //        }
        //        //else if (mode == "SETUP")
        //        //{
        //        //    Session["ModeError"] = "Machine is already in Setting Mode";
        //        //    return RedirectToAction("Index");
        //        //}
        //        else if (mode != "SETUP")
        //        {
        //            DateTime time = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        //            obj.UpdateBreakdownDetails(time, currentId);
        //            //tblbreakdown tbd = db.tblbreakdowns.Find(currentId);
        //            //tbd.EndTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        //            //db.Entry(tbd).State = System.Data.Entity.EntityState.Modified;
        //            //db.SaveChanges();
        //            break;
        //        }
        //    }

        //    string mcode = "70800";
        //    var brkdown = obj.GetbreakdownList1(id, mcode);
        //    //var brkdown = db.tblbreakdowns.Where(m => m.MachineID == id).Where(m => m.CorrectedDate == CorrectedDate && m.EndTime == null && m.MessageCode == mcode);

        //    if (brkdown.Count() != 0)
        //    {
        //        TempData["Enable"] = "Enable";
        //        int brekdnID = 0;
        //        foreach (var a in brkdown)
        //        {
        //            brekdnID = a.BreakdownID;
        //        }
        //        tblbreakdown brekdn = obj.GetbreakdownDet(brekdnID);
        //        //tblbreakdown brekdn = db.tblbreakdowns.Find(brekdnID);
        //        //CheckLastOneHourDownTime(id);
        //        ViewBag.BreakDownCode = new SelectList(obj.GettbllossescodeList1(), "MessageCodeID", "MessageDescription", brekdn.BreakDownCode);
        //        //ViewBag.BreakDownCode = new SelectList(db.message_code_master.Where(m => m.IsDeleted == 0).Where(m => m.MessageType == "SETUP"), "MessageCodeID", "MessageDescription", brekdn.BreakDownCode);
        //        return View(brekdn);
        //    }
        //    else
        //    {

        //    }
        //    //CheckLastOneHourDownTime(id);
        //    ViewBag.BreakDownCode = new SelectList(obj.GettbllossescodeList1(), "MessageCodeID", "MessageDescription");
        //    //ViewBag.BreakDownCode = new SelectList(db.message_code_master.Where(m => m.IsDeleted == 0).Where(m => m.MessageType == "SETUP"), "MessageCodeID", "MessageDescription");
        //    return View();
        //}
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Setting(tblbreakdown lossentry, string submit = "")
        //{
        //    _conn = new ConnectionFactory();
        //    obj = new Dao(_conn);

        //    int MachineID = 0;
        //    if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
        //    {
        //        return RedirectToAction("Login", "Login", null);
        //    }

        //    ViewBag.Logout = Session["Username"];
        //    ViewBag.roleid = Session["RoleID"];
        //    int RID = Convert.ToInt32(Session["RoleID"]);
        //    string CorrectedDate = null;
        //    int MachineID1 = 0;
        //    tbldaytiming StartTime = obj.GetDaytimingDetails();
        //    //tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
        //    TimeSpan Start = StartTime.StartTime;
        //    if (Start <= DateTime.Now.TimeOfDay)
        //    {
        //        CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
        //    }
        //    else
        //    {
        //        CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        //    }
        //    if (string.IsNullOrEmpty(submit) == false)
        //    {
        //        // lossentry.CorrectedDate = CorrectedDate;
        //        DateTime Time = DateTime.Now;
        //        //lossentry.StartTime = DateTime.Now;
        //        if (RID != 1 && RID != 2)
        //        {
        //            MachineID1 = Convert.ToInt32(Session["MachineID"]);
        //            //lossentry.MachineID = Convert.ToInt32(Session["MachineID"]);
        //            MachineID = Convert.ToInt32(Session["MachineID"]);
        //        }
        //        else
        //        {
        //            MachineID1 = Convert.ToInt32(Session["Mid"]);
        //            lossentry.MachineID = Convert.ToInt32(Session["Mid"]);
        //            MachineID = Convert.ToInt32(Session["Mid"]);
        //        }
        //        message_code_master downcode = obj.Getmessage_code_masterDet(lossentry.BreakDownCode);
        //        //message_code_master downcode = db.message_code_master.Find(lossentry.BreakDownCode);
        //        //lossentry.BreakDownCode = Convert.ToInt32(downcode.MessageCode);
        //        string Shift = Session["realshift"].ToString();
        //        //lossentry.Shift = Session["realshift"].ToString();
        //        //lossentry.BreakDownCode =Convert.ToInt32(downcode.MessageCode);
        //        string MessageCode = (downcode.MessageCode).ToString();
        //        //lossentry.MessageCode = (downcode.MessageCode).ToString();
        //        obj.InsertlossofentryDetails(CorrectedDate, Time, MachineID1, Shift, MessageCode)
        //        //db.tblbreakdowns.Add(lossentry);
        //        //db.SaveChanges();
        //        //SendMail(downcode.MessageCode, downcode.MessageDescription, MachineID);

        //        //update the endtime for the last mode of this machine
        //        var tblmodedata = obj.GettbllivemodedbsList1(MachineID);
        //        //var tblmodedata = db.tbllivemodedbs.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID).OrderByDescending(m => m.StartTime).ToList();
        //        foreach (var row in tblmodedata)
        //        {
        //            int id = row.ModeID;
        //            DateTime time = DateTime.Now;
        //            obj.UpdatetblmodedataDetails1(time, id);
        //            //row.EndTime = DateTime.Now;
        //            //db.Entry(row).State = System.Data.Entity.EntityState.Modified;
        //            //db.SaveChanges();
        //        }

        //        //Code to save this event to tblmode table
        //        DateTime InsertedOn = DateTime.Now;
        //        DateTime StartTime1 = DateTime.Now;
        //        obj.InserttbllivemodedbDetails1(MachineID, CorrectedDate, InsertedOn, StartTime1);
        //        //tbllivemodedb tm = new tbllivemodedb();
        //        //tm.MachineID = MachineID;
        //        //tm.CorrectedDate = CorrectedDate;
        //        //tm.InsertedBy = 1;
        //        //tm.InsertedOn = DateTime.Now;
        //        //tm.StartTime = DateTime.Now;
        //        //tm.ColorCode = "green";
        //        //tm.IsDeleted = 0;
        //        //tm.Mode = "SETUP";

        //        //db.tbllivemodedbs.Add(tm);
        //        //db.SaveChanges();

        //        return RedirectToAction("Index");
        //    }
        //    else
        //    {
        //        lossentry.CorrectedDate = CorrectedDate;
        //        lossentry.EndTime = DateTime.Now;
        //        MachineID = Convert.ToInt32(lossentry.MachineID);
        //        int id = lossentry.BreakdownID;
        //        //lossentry.MachineID = Convert.ToInt32(Session["MachineID"]);
        //        obj.UpdatetblbreakdownDetails1(lossentry.CorrectedDate, lossentry.EndTime, id, MachineID);
        //        //db.Entry(lossentry).State = System.Data.Entity.EntityState.Modified;
        //        //db.SaveChanges();
        //        //UpdateRecordOfProduction(lossentry);
        //        //int code = Convert.ToInt32(lossentry.BreakDownCode);
        //        //message_code_master msg = db.message_code_master.Where(m => m.MessageCodeID == code).FirstOrDefault();
        //        //SendMailEnd(msg.MessageCode, msg.MessageDescription, MachineID);
        //        return RedirectToAction("Index");
        //    }

        //    return View(lossentry);
        //}

        public ActionResult Maintenance(int id = 0)
        {
            Session["split"] = null;
            //Session["Split"] = null;
            Session["Mode"] = null;
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            Session["Mid"] = id;
            string CorrectedDate = null;
            tbldaytiming StartTime = obj.GetDaytimingDetails();
            //tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
            TimeSpan Start = StartTime.StartTime;
            if (Start <= DateTime.Now.TimeOfDay)
            {
                CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }
            string machinedispname = obj.GetMachineDet1(id);
            //var machinedispname = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == id).Select(m => m.MachineDispName).FirstOrDefault();
            ViewBag.macDispName = Convert.ToString(machinedispname);

            //CODE to check the current mode is allowable or not , based on MODE Priority.
            List<tblbreakdown> curMode = obj.GetBreakdownList(id, CorrectedDate);
            //var curMode = db.tblbreakdowns.Where(m => m.MachineID == id).Where(m => m.CorrectedDate == CorrectedDate && m.EndTime == null).OrderByDescending(m => m.BreakdownID);
            int currentId = 0;

            foreach (tblbreakdown j in curMode)
            {
                currentId = j.BreakdownID;
                string mode = j.MessageCode;
                if (mode != "PM")
                {
                    currentId = j.BreakdownID;
                    DateTime time = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    obj.UpdateBreakdownDetails(time, currentId);
                    //tblbreakdown tbd = db.tblbreakdowns.Find(currentId);

                    //db.Entry(tbd).State = System.Data.Entity.EntityState.Modified;
                    //db.SaveChanges();
                    break;
                }
                //else if (mode == "PM")
                //{
                //    Session["ModeError"] = "Machine is in Planned Maintenance Mode";
                //    return RedirectToAction("Index");
                //}

            }

            List<tblbreakdown> brkdown = obj.GetbreakdownList(id);
            //var brkdown = db.tblbreakdowns.Where(m => m.MachineID == id).Where(m => m.EndTime == null && m.MessageCode == "PM");
            if (brkdown.Count() != 0)
            {
                TempData["Enable"] = "Enable";
                int brekdnID = 0;
                foreach (tblbreakdown a in brkdown)
                {
                    brekdnID = a.BreakdownID;
                }
                tblbreakdown brekdn = obj.GetbreakdownDet(brekdnID);
                // tblbreakdown brekdn = db.tblbreakdowns.Find(brekdnID);
                //CheckLastOneHourDownTime(id);
                ViewBag.BreakDownCode = new SelectList(obj.GettbllossescodeList(), "LossCode", "LossCodeDesc", brekdn.BreakDownCode);
                //ViewBag.BreakDownCode = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0).Where(m => m.MessageType == "PM"), "LossCode", "LossCodeDesc", brekdn.BreakDownCode);
                return View(brekdn);
            }
            else
            {
            }
            //CheckLastOneHourDownTime(id);
            ViewBag.BreakDownCode = new SelectList(obj.GettbllossescodeList(), "LossCode", "LossCodeDesc");
            //ViewBag.BreakDownCode = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0).Where(m => m.MessageType == "PM"), "LossCode", "LossCodeDesc");
            return View();
        }

        #region Post Method planned maintance

        public void IntoFile(string Msg)
        {
            try
            {
                string appPath = @"D://LOG/logfile_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                using (StreamWriter writer = new StreamWriter(appPath, true)) //true => Append Text
                {
                    writer.WriteLine(System.DateTime.Now + ":  " + Msg + "\r \n");
                }
            }
            catch (Exception e7)
            {
                IntoFile("IntoFile" + e7.ToString());
            }

        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Maintenance(tblbreakdown lossentry, string submit = "", string BreakDownCode = null)
        //{
        //    Session["Mode"] = null;
        //    _conn = new ConnectionFactory();
        //    obj = new Dao(_conn);

        //    int MachineID = 0;
        //    if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
        //    {
        //        return RedirectToAction("Login", "Login", null);
        //    }
        //    ViewBag.Logout = Session["Username"];
        //    ViewBag.roleid = Session["RoleID"];
        //    int RID = Convert.ToInt32(Session["RoleID"]);
        //    string CorrectedDate = null;
        //    tbldaytiming StartTime = obj.GetDaytimingDetails();
        //    //tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
        //    TimeSpan Start = StartTime.StartTime;
        //    if (Start <= DateTime.Now.TimeOfDay)
        //    {
        //        CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
        //    }
        //    else
        //    {
        //        CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        //    }
        //    if (string.IsNullOrEmpty(submit) == false && submit == "Start")
        //    {
        //        string CorrectedDate1 = CorrectedDate;

        //        //lossentry.StartTime = DateTime.Now;
        //        if (RID != 1 && RID != 2)
        //        {
        //            //lossentry.MachineID = Convert.ToInt32(Session["MachineID"]);
        //            MachineID = Convert.ToInt32(Session["MachineID"]);
        //        }
        //        else
        //        {
        //            //lossentry.MachineID = Convert.ToInt32(Session["Mid"]);
        //            MachineID = Convert.ToInt32(Session["Mid"]);
        //        }
        //        DateTime Time = DateTime.Now;
        //        var LossData = obj.GettbllossDetails(BreakDownCode);
        //        //var LossData = db.tbllossescodes.Where(m => m.LossCode == BreakDownCode).FirstOrDefault();
        //        string Shift = Session["realshift"].ToString();
        //        string MessageCode = (LossData.LossCode).ToString();
        //        obj.InsertlossofentryDetails(CorrectedDate1, Time, MachineID, Shift, MessageCode);

        //        //int BreakDownCode1 = 120;
        //        //int DoneWithRow = 0;
        //        //string MessageDesc = "PM";
        //        //db.tblbreakdowns.Add(lossentry);
        //        //db.SaveChanges();

        //        //update the endtime for the last mode of this machine 
        //        var tblmodedata = obj.GettbllivemodedbsList(MachineID);
        //        //var tblmodedata = db.tbllivemodedbs.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID && m.IsCompleted == 0).OrderByDescending(m => m.StartTime).ToList();
        //        foreach (var row in tblmodedata)
        //        {
        //            int id = row.ModeID;
        //            //DateTime EndTime = DateTime.Now;
        //            //obj.UpdatetblmodedataDetails(EndTime, id);
        //            double diff = DateTime.Now.Subtract(Convert.ToDateTime(row.StartTime)).TotalSeconds;
        //            obj.UpdateLiveModeDetails(id, DateTime.Now, diff);

        //            //row.IsCompleted = 1;
        //            //db.Entry(row).State = System.Data.Entity.EntityState.Modified;
        //            //db.SaveChanges();


        //        }
        //        //Code to save this event to tblmode table
        //        int InsertedBy = Convert.ToInt32(Session["UserId"]);
        //        DateTime InsertedOn = DateTime.Now;
        //        DateTime StartTime1 = DateTime.Now;
        //        obj.InserttbllivemodedbDetails(MachineID, CorrectedDate, InsertedBy, InsertedOn, StartTime1);
        //        //tbllivemodedb tm = new tbllivemodedb();
        //        //tm.MachineID = MachineID;
        //        //tm.CorrectedDate = CorrectedDate;
        //        //tm.ColorCode = "red";
        //        //tm.IsCompleted = 0;
        //        //tm.IsDeleted = 0;
        //        //tm.Mode = "BREAKDOWN";
        //        //db.tbllivemodedbs.Add(tm);
        //        //db.SaveChanges();

        //        return RedirectToAction("Index");
        //    }
        //    else
        //    {
        //        DateTime EndTime1 = DateTime.Now;
        //        int id = lossentry.BreakdownID;
        //        obj.UpdatetblbreakdownDetails(CorrectedDate, EndTime1, id);
        //        lossentry.CorrectedDate = CorrectedDate;

        //        //lossentry.DoneWithRow = 1;
        //        MachineID = Convert.ToInt32(lossentry.MachineID);
        //        try
        //        {
        //            //db.Entry(lossentry).State = System.Data.Entity.EntityState.Modified;
        //            //db.SaveChanges();
        //            var tblmodedata = obj.GettbllivemodedbsList(MachineID);
        //            // var tblmodedata = db.tbllivemodedbs.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID && m.IsCompleted == 0).OrderByDescending(m => m.StartTime).ToList();
        //            foreach (var row in tblmodedata)
        //            {
        //                int id1 = row.ModeID;
        //                //DateTime EndTime = DateTime.Now;
        //                //obj.UpdatetblmodedataDetails(EndTime, id1);
        //                double diff = DateTime.Now.Subtract(Convert.ToDateTime(row.StartTime)).TotalSeconds;
        //                obj.UpdateLiveModeDetails(id, DateTime.Now, diff);

        //                //row.EndTime = DateTime.Now;
        //                //row.IsCompleted = 1;
        //                //db.Entry(row).State = System.Data.Entity.EntityState.Modified;
        //                //db.SaveChanges();
        //            }
        //            int InsertedBy = Convert.ToInt32(Session["UserId"]);
        //            DateTime InsertedOn = DateTime.Now;
        //            DateTime StartTime1 = DateTime.Now;
        //            string CorrectedDate1 = CorrectedDate;
        //            obj.InserttbllivemodedbDetails(CorrectedDate1, InsertedBy, InsertedOn, MachineID, StartTime1);
        //            //tbllivemodedb tmIDLE = new tbllivemodedb();
        //            //tmIDLE.ColorCode = "yellow";
        //            //
        //            //
        //            //
        //            //tmIDLE.IsCompleted = 0;
        //            //tmIDLE.IsDeleted = 0;
        //            //tmIDLE.MachineID = MachineID;
        //            //tmIDLE.Mode = "IDLE";
        //            //

        //            //db.tbllivemodedbs.Add(tmIDLE);
        //            //db.SaveChanges();

        //        }
        //        catch (Exception e)
        //        { }
        //        //UpdateRecordOfProduction(lossentry);
        //        //int code = Convert.ToInt32(lossentry.BreakDownCode);
        //        // message_code_master msg = db.message_code_master.Where(m => m.MessageCodeID == code).FirstOrDefault();
        //        //SendMailEnd(msg.MessageCode, msg.MessageDescription, MachineID);
        //        return RedirectToAction("Index");
        //    }
        //    return View(lossentry);
        //}
        #endregion


        //public ActionResult Maintenance(tblbreakdown lossentry, string submit = "", string BreakDownCode = null)
        //{
        //    Session["split"] = null;
        //    Session["Mode"] = null;
        //    _conn = new ConnectionFactory();
        //    obj = new Dao(_conn);

        //    int MachineID = 0;
        //    if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
        //    {
        //        return RedirectToAction("Login", "Login", null);
        //    }
        //    ViewBag.Logout = Session["Username"];
        //    ViewBag.roleid = Session["RoleID"];
        //    int RID = Convert.ToInt32(Session["RoleID"]);
        //    string CorrectedDate = null;
        //    tbldaytiming StartTime = obj.GetDaytimingDetails();
        //    //tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
        //    TimeSpan Start = StartTime.StartTime;
        //    if (Start <= DateTime.Now.TimeOfDay)
        //    {
        //        CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
        //    }
        //    else
        //    {
        //        CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        //    }
        //    DateTime Time = DateTime.Now;
        //    TimeSpan Tm = new TimeSpan(Time.Hour, Time.Minute, Time.Second);
        //    var ShiftDetails = obj.GetShiftDetails(Tm);
        //    string Shift = "C";
        //    int shiftid = 1;
        //    if (ShiftDetails != null)
        //    {
        //        Shift = ShiftDetails.ShiftName;
        //    }

        //    if (Shift == "A")
        //    {
        //        shiftid = 1;
        //    }
        //    else if (Shift == "B")
        //    {
        //        shiftid = 2;
        //    }
        //    else if (Shift == "C")
        //    {
        //        shiftid = 3;
        //    }
        //    if (string.IsNullOrEmpty(submit) == false && submit == "Start")
        //    {
        //        string CorrectedDate1 = CorrectedDate;

        //        //lossentry.StartTime = DateTime.Now;
        //        if (RID != 1 && RID != 2)
        //        {
        //            //lossentry.MachineID = Convert.ToInt32(Session["MachineID"]);
        //            MachineID = Convert.ToInt32(Session["MachineID"]);
        //        }
        //        else
        //        {
        //            //lossentry.MachineID = Convert.ToInt32(Session["Mid"]);
        //            MachineID = Convert.ToInt32(Session["Mid"]);
        //        }

        //        //var ShiftDetails = db.tblshift_mstr.Where(m => m.StartTime <= Tm && m.EndTime >= Tm);


        //        var LossData = obj.GettbllossDetails(BreakDownCode);
        //        //var LossData = db.tbllossescodes.Where(m => m.LossCode == BreakDownCode).FirstOrDefault();
        //        Shift = Session["realshift"].ToString();
        //        string MessageCode = (LossData.LossCode).ToString();
        //        obj.InsertlossofentryDetails(CorrectedDate1, Time, MachineID, Shift, MessageCode);

        //        //int BreakDownCode1 = 120;
        //        //int DoneWithRow = 0;
        //        //string MessageDesc = "PM";
        //        //db.tblbreakdowns.Add(lossentry);
        //        //db.SaveChanges();

        //        //update the endtime for the last mode of this machine 
        //        var tblmodedata = obj.GettbllivemodedbsList(MachineID);
        //        //var tblmodedata = db.tbllivemodedbs.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID && m.IsCompleted == 0).OrderByDescending(m => m.StartTime).ToList();
        //        foreach (var row in tblmodedata)
        //        {
        //            int id = row.ModeID;
        //            double diff = DateTime.Now.Subtract(Convert.ToDateTime(row.StartTime)).TotalSeconds;
        //            obj.UpdateLiveModeDetails(row.ModeID, DateTime.Now, diff);

        //            //row.IsCompleted = 1;
        //            //db.Entry(row).State = System.Data.Entity.EntityState.Modified;
        //            //db.SaveChanges();


        //        }
        //        //Code to save this event to tblmode table
        //        int InsertedBy = Convert.ToInt32(Session["UserId"]);
        //        DateTime InsertedOn = DateTime.Now;
        //        DateTime StartTime1 = DateTime.Now;
        //        obj.InserttbllivemodedbDetails( CorrectedDate, InsertedBy, InsertedOn, MachineID, StartTime1,shiftid);
        //        //tbllivemodedb tm = new tbllivemodedb();
        //        //tm.MachineID = MachineID;
        //        //tm.CorrectedDate = CorrectedDate;
        //        //tm.ColorCode = "red";
        //        //tm.IsCompleted = 0;
        //        //tm.IsDeleted = 0;
        //        //tm.Mode = "BREAKDOWN";
        //        //db.tbllivemodedbs.Add(tm);
        //        //db.SaveChanges();

        //        return RedirectToAction("Index");
        //    }
        //    else
        //    {
        //        DateTime EndTime1 = DateTime.Now;
        //        int id = lossentry.BreakdownID;
        //        obj.UpdatetblbreakdownDetails(CorrectedDate, EndTime1, id);
        //        lossentry.CorrectedDate = CorrectedDate;

        //        //lossentry.DoneWithRow = 1;
        //        MachineID = Convert.ToInt32(lossentry.MachineID);
        //        try
        //        {
        //            //db.Entry(lossentry).State = System.Data.Entity.EntityState.Modified;
        //            //db.SaveChanges();
        //            var tblmodedata = obj.GettbllivemodedbsList(MachineID);
        //            // var tblmodedata = db.tbllivemodedbs.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID && m.IsCompleted == 0).OrderByDescending(m => m.StartTime).ToList();
        //            foreach (var row in tblmodedata)
        //            {
        //                int id1 = row.ModeID;
        //                double diff = DateTime.Now.Subtract(Convert.ToDateTime(row.StartTime)).TotalSeconds;
        //                obj.UpdateLiveModeDetails(row.ModeID, DateTime.Now, diff);
        //                //row.EndTime = DateTime.Now;
        //                //row.IsCompleted = 1;
        //                //db.Entry(row).State = System.Data.Entity.EntityState.Modified;
        //                //db.SaveChanges();
        //            }
        //            int InsertedBy = Convert.ToInt32(Session["UserId"]);
        //            DateTime InsertedOn = DateTime.Now;
        //            DateTime StartTime1 = DateTime.Now;
        //            string CorrectedDate1 = CorrectedDate;
        //            obj.InserttbllivemodedbDetails(CorrectedDate1, InsertedBy, InsertedOn, MachineID, StartTime1,shiftid);
        //            //tbllivemodedb tmIDLE = new tbllivemodedb();
        //            //tmIDLE.ColorCode = "yellow";
        //            //
        //            //
        //            //
        //            //tmIDLE.IsCompleted = 0;
        //            //tmIDLE.IsDeleted = 0;
        //            //tmIDLE.MachineID = MachineID;
        //            //tmIDLE.Mode = "IDLE";
        //            //

        //            //db.tbllivemodedbs.Add(tmIDLE);
        //            //db.SaveChanges();

        //        }
        //        catch (Exception e)
        //        { }
        //        //UpdateRecordOfProduction(lossentry);
        //        //int code = Convert.ToInt32(lossentry.BreakDownCode);
        //        // message_code_master msg = db.message_code_master.Where(m => m.MessageCodeID == code).FirstOrDefault();
        //        //SendMailEnd(msg.MessageCode, msg.MessageDescription, MachineID);
        //        return RedirectToAction("Index");
        //    }
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Maintenance(tblbreakdown lossentry, string submit = "", string BreakDownCode = null)
        {
            Session["split"] = null;
            Session["Mode"] = null;
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            int MachineID = 0;
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            int RID = Convert.ToInt32(Session["RoleID"]);
            string CorrectedDate = null;
            tbldaytiming StartTime = obj.GetDaytimingDetails();
            //tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
            TimeSpan Start = StartTime.StartTime;
            if (Start <= DateTime.Now.TimeOfDay)
            {
                CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }

            DateTime Time = DateTime.Now;
            TimeSpan Tm = new TimeSpan(Time.Hour, Time.Minute, Time.Second);
            tblshift_mstr ShiftDetails = obj.GetShiftDetails(Tm);
            string Shift = "C";
            int shiftid = 1;
            if (ShiftDetails != null)
            {
                Shift = ShiftDetails.ShiftName;
            }

            if (Shift == "A")
            {
                shiftid = 1;
            }
            else if (Shift == "B")
            {
                shiftid = 2;
            }
            else if (Shift == "C")
            {
                shiftid = 3;
            }
            if (string.IsNullOrEmpty(submit) == false && submit == "Start")
            {
                string CorrectedDate1 = CorrectedDate;

                //lossentry.StartTime = DateTime.Now;
                if (RID != 1 && RID != 2)
                {
                    //lossentry.MachineID = Convert.ToInt32(Session["MachineID"]);
                    MachineID = Convert.ToInt32(Session["MachineID"]);
                }
                else
                {
                    //lossentry.MachineID = Convert.ToInt32(Session["Mid"]);
                    MachineID = Convert.ToInt32(Session["Mid"]);
                }
                tbllossescode LossData = obj.GettbllossDetails(BreakDownCode);
                //var LossData = db.tbllossescodes.Where(m => m.LossCode == BreakDownCode).FirstOrDefault();
                Shift = Session["realshift"].ToString();
                string MessageCode = (LossData.LossCode).ToString();
                obj.InsertlossofentryDetails(CorrectedDate1, Time, MachineID, Shift, MessageCode);

                //int BreakDownCode1 = 120;
                //int DoneWithRow = 0;
                //string MessageDesc = "PM";
                //db.tblbreakdowns.Add(lossentry);
                //db.SaveChanges();

                //update the endtime for the last mode of this machine 
                List<tbllivemode> tblmodedata = obj.GettbllivemodedbsList(MachineID);
                //var tblmodedata = db.tbllivemodedbs.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID && m.IsCompleted == 0).OrderByDescending(m => m.StartTime).ToList();
                foreach (tbllivemode row in tblmodedata)
                {
                    int id = row.ModeID;
                    double diff = DateTime.Now.Subtract(Convert.ToDateTime(row.StartTime)).TotalSeconds;
                    obj.UpdateLiveModeDetails(row.ModeID, DateTime.Now, diff);

                    //row.IsCompleted = 1;
                    //db.Entry(row).State = System.Data.Entity.EntityState.Modified;
                    //db.SaveChanges();


                }
                //Code to save this event to tblmode table
                int InsertedBy = Convert.ToInt32(Session["UserId"]);
                DateTime InsertedOn = DateTime.Now;
                DateTime StartTime1 = DateTime.Now;
                obj.InserttbllivemodedbDetails2(MachineID, CorrectedDate, InsertedBy, InsertedOn, StartTime1, shiftid);
                //tbllivemodedb tm = new tbllivemodedb();
                //tm.MachineID = MachineID;
                //tm.CorrectedDate = CorrectedDate;
                //tm.ColorCode = "red";
                //tm.IsCompleted = 0;
                //tm.IsDeleted = 0;
                //tm.Mode = "BREAKDOWN";
                //db.tbllivemodedbs.Add(tm);
                //db.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                DateTime EndTime1 = DateTime.Now;
                int id = lossentry.BreakdownID;
                obj.UpdatetblbreakdownDetails(CorrectedDate, EndTime1, id);
                lossentry.CorrectedDate = CorrectedDate;

                //lossentry.DoneWithRow = 1;
                MachineID = Convert.ToInt32(lossentry.MachineID);
                try
                {
                    //db.Entry(lossentry).State = System.Data.Entity.EntityState.Modified;
                    //db.SaveChanges();
                    List<tbllivemode> tblmodedata = obj.GettbllivemodedbsList(MachineID);
                    // var tblmodedata = db.tbllivemodedbs.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID && m.IsCompleted == 0).OrderByDescending(m => m.StartTime).ToList();
                    foreach (tbllivemode row in tblmodedata)
                    {
                        int id1 = row.ModeID;
                        double diff = DateTime.Now.Subtract(Convert.ToDateTime(row.StartTime)).TotalSeconds;
                        obj.UpdateLiveModeDetails(row.ModeID, DateTime.Now, diff);
                        //row.EndTime = DateTime.Now;
                        //row.IsCompleted = 1;
                        //db.Entry(row).State = System.Data.Entity.EntityState.Modified;
                        //db.SaveChanges();
                    }
                    int InsertedBy = Convert.ToInt32(Session["UserId"]);
                    DateTime InsertedOn = DateTime.Now;
                    DateTime StartTime1 = DateTime.Now;
                    string CorrectedDate1 = CorrectedDate;
                    obj.InserttbllivemodedbDetails(CorrectedDate1, InsertedBy, InsertedOn, MachineID, StartTime1, shiftid);
                    //tbllivemodedb tmIDLE = new tbllivemodedb();
                    //tmIDLE.ColorCode = "yellow";
                    //
                    //
                    //
                    //tmIDLE.IsCompleted = 0;
                    //tmIDLE.IsDeleted = 0;
                    //tmIDLE.MachineID = MachineID;
                    //tmIDLE.Mode = "IDLE";
                    //

                    //db.tbllivemodedbs.Add(tmIDLE);
                    //db.SaveChanges();

                }
                catch (Exception)
                { }
                //UpdateRecordOfProduction(lossentry);
                //int code = Convert.ToInt32(lossentry.BreakDownCode);
                // message_code_master msg = db.message_code_master.Where(m => m.MessageCodeID == code).FirstOrDefault();
                //SendMailEnd(msg.MessageCode, msg.MessageDescription, MachineID);
                return RedirectToAction("Index");
            }
            return View(lossentry);
        }
        #region set shift OLD
        ////code
        ////For changeVisibelity shift for normal user
        //public ActionResult changeVisibelityNorm()
        //{
        //    string CorrectedDate = null;
        //    tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
        //    TimeSpan Start = StartTime.StartTime;
        //    if (Start <= DateTime.Now.TimeOfDay)
        //    {
        //        CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
        //    }
        //    else
        //    {
        //        CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        //    }
        //    int MachineID = Convert.ToInt32(Session["MchnID"]);
        //    ViewBag.hide = null;
        //    //Gatting UderID
        //    int opid = Convert.ToInt32(Session["Opid"]);
        //    Session["Show"] = 1;
        //    return RedirectToAction("Index", "HMIScree");
        //}

        ////For general shift for normal user
        //public ActionResult changeShiftNorm(String Shift)
        //{
        //    Session["Show"] = null;
        //    if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
        //    {
        //        return RedirectToAction("Login", "Login", null);
        //    }
        //    ViewBag.roleid = Session["RoleID"];
        //    string CorrectedDate = null;
        //    tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
        //    TimeSpan Start = StartTime.StartTime;
        //    if (Start <= DateTime.Now.TimeOfDay)
        //    {
        //        CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
        //    }
        //    else
        //    {
        //        CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        //    }
        //    int MachineID = Convert.ToInt32(Session["MchnID"]);
        //    //Gatting UderID
        //    int opid = Convert.ToInt32(Session["Opid"]);
        //    tblhmiscreen HMI = db.tblhmiscreens.Where(m => m.CorrectedDate == CorrectedDate && m.OperatiorID == opid && m.Shift == Shift).Where(m => m.MachineID == MachineID).FirstOrDefault();
        //   // tblhmiscreen HMI = db.tblhmiscreens.Where(m => m.CorrectedDate == CorrectedDate && m.OperatiorID == opid).Where(m => m.MachineID == MachineID).FirstOrDefault();
        //    if (HMI == null)
        //    {
        //        //idea is to make the status of previous shift to 2 and insert new shift
        //        string operatorname = null;
        //        var oldshiftdata = db.tblhmiscreens.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate && m.OperatiorID == opid).OrderByDescending(m => m.HMIID).FirstOrDefault();
        //        if (oldshiftdata != null)
        //        {
        //            oldshiftdata.Status = 2;
        //            operatorname = oldshiftdata.OperatorDet;
        //            db.Entry(oldshiftdata).State = System.Data.Entity.EntityState.Modified;
        //            db.SaveChanges();
        //        }


        //        tblhmiscreen tblhmiscreen = new tblhmiscreen();
        //        tblhmiscreen.MachineID = MachineID;
        //        tblhmiscreen.CorrectedDate = CorrectedDate;
        //        tblhmiscreen.Date = DateTime.Now.Date;
        //        tblhmiscreen.Shift = Shift;
        //        tblhmiscreen.OperatorDet = operatorname;
        //        tblhmiscreen.Status = 0;
        //        tblhmiscreen.isWorkInProgress = 2;
        //        tblhmiscreen.OperatiorID = opid;
        //        tblhmiscreen.Time = DateTime.Now.TimeOfDay;
        //        db.tblhmiscreens.Add(tblhmiscreen);
        //        db.SaveChanges();

        //        //tblhmiscreen tblhmiscreenSecondRow = new tblhmiscreen();
        //        //tblhmiscreenSecondRow.MachineID = MachineID;
        //        //tblhmiscreenSecondRow.CorrectedDate = CorrectedDate;
        //        //tblhmiscreenSecondRow.Date = DateTime.Now.Date;
        //        //tblhmiscreenSecondRow.Shift = Shift;
        //        //tblhmiscreenSecondRow.Status = 1;
        //        //tblhmiscreenSecondRow.OperatiorID = opid;
        //        //tblhmiscreenSecondRow.Time = DateTime.Now.TimeOfDay;
        //        //db.tblhmiscreens.Add(tblhmiscreenSecondRow);
        //        //db.SaveChanges();
        //    }
        //    else
        //    { //do nothing.
        //        HMI.Shift = Shift;
        //        db.Entry(HMI).State = System.Data.Entity.EntityState.Modified;
        //        db.SaveChanges();
        //    }

        //    //HMIScreenForAdmin(MachineID, opid, CorrectedDate);
        //    Session["opid"] = opid;
        //    Session["gopid"] = opid;
        //    Session["gshift" + opid] = Shift;
        //    ViewBag.hide = 1;
        //    return RedirectToAction("Index", "HMIScree");
        //}

        ////For general shift for admin
        //public ActionResult changeShift(String Shift)
        //{
        //    Session["Show"] = null;
        //    if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
        //    {
        //        return RedirectToAction("Login", "Login", null);
        //    }
        //    ViewBag.roleid = Session["RoleID"];
        //    string CorrectedDate = null;
        //    tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
        //    TimeSpan Start = StartTime.StartTime;
        //    if (Start <= DateTime.Now.TimeOfDay)
        //    {
        //        CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
        //    }
        //    else
        //    {
        //        CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        //    }
        //    int MachineID = Convert.ToInt32(Session["MchnID"]);
        //    //Gatting UderID
        //    int opid = Convert.ToInt32(Session["Opid"]);
        //    //tblhmiscreen HMI = db.tblhmiscreens.Where(m => m.CorrectedDate == CorrectedDate && m.OperatiorID == opid && m.Shift == Shift).Where(m => m.MachineID == MachineID).FirstOrDefault();
        //    tblhmiscreen HMI = db.tblhmiscreens.Where(m => m.CorrectedDate == CorrectedDate && m.OperatiorID == opid ).Where(m => m.MachineID == MachineID).FirstOrDefault();
        //    if (HMI == null)
        //    {
        //        tblhmiscreen tblhmiscreen = new tblhmiscreen();
        //        tblhmiscreen.MachineID = MachineID;
        //        tblhmiscreen.CorrectedDate = CorrectedDate;
        //        tblhmiscreen.Date = DateTime.Now.Date;
        //        tblhmiscreen.Shift = Shift;
        //        tblhmiscreen.Status = 0;
        //        tblhmiscreen.OperatiorID = opid;
        //        tblhmiscreen.Time = DateTime.Now.TimeOfDay;
        //        db.tblhmiscreens.Add(tblhmiscreen);
        //        db.SaveChanges();

        //        //tblhmiscreen tblhmiscreenSecondRow = new tblhmiscreen();
        //        //tblhmiscreenSecondRow.MachineID = MachineID;
        //        //tblhmiscreenSecondRow.CorrectedDate = CorrectedDate;
        //        //tblhmiscreenSecondRow.Date = DateTime.Now.Date;
        //        //tblhmiscreenSecondRow.Shift = Shift;
        //        //tblhmiscreenSecondRow.Status = 1;
        //        //tblhmiscreenSecondRow.OperatiorID = opid;
        //        //tblhmiscreenSecondRow.Time = DateTime.Now.TimeOfDay;
        //        //db.tblhmiscreens.Add(tblhmiscreenSecondRow);
        //        //db.SaveChanges();
        //    }
        //    else
        //    {
        //        HMI.Shift = Shift;
        //        db.Entry(HMI).State = System.Data.Entity.EntityState.Modified;
        //        db.SaveChanges();
        //    }
        //    //HMIScreenForAdmin(MachineID, opid, CorrectedDate);
        //    Session["opid"] = opid;
        //    Session["gopid"] = opid;
        //    Session["gshift" + opid] = Shift;
        //    ViewBag.hide = 1;
        //    return RedirectToAction("HMIScreenForAdmin", "HMIScree", new { MachineID, opid, CorrectedDate });
        //}

        ////For changeVisibelity shift for admin
        //public ActionResult changeVisibelity()
        //{
        //    string CorrectedDate = null;
        //    tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
        //    TimeSpan Start = StartTime.StartTime;
        //    if (Start <= DateTime.Now.TimeOfDay)
        //    {
        //        CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
        //    }
        //    else
        //    {
        //        CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        //    }
        //    int MachineID = Convert.ToInt32(Session["MchnID"]);
        //    ViewBag.hide = null;
        //    //Gatting UderID
        //    int opid = Convert.ToInt32(Session["Opid"]);
        //    Session["Show"] = 1;
        //    return RedirectToAction("HMIScreenForAdmin", "HMIScree", new { MachineID, opid, CorrectedDate });
        //}
        #endregion

        public string GetReworkstatus(int MachineID)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);
            string res = "";
            string CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            if (DateTime.Now.Hour < 6 && DateTime.Now.Hour >= 0)
            {
                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }
            ViewBag.coretddt = CorrectedDate;
            List<tbllivehmiscreen> check = obj.GetLiveHMIDetails145(MachineID, CorrectedDate);
            if (check.Count == 1)
            {
                if (check[0].Work_Order_No != null)
                {
                    res = "WorkOrder";
                }
                else
                {
                    res = "Success";
                }
            }
            else if (check.Count > 1)
            {
                res = "fail";
            }
            return res;
        }

        public ActionResult changeShiftNorm(String Shift)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            Session["Show"] = null;
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.roleid = Session["RoleID"];
            string CorrectedDate = null;
            //tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
            tbldaytiming StartTime = obj.GetDaytimingDetails();
            TimeSpan Start = StartTime.StartTime;
            if (Start <= DateTime.Now.TimeOfDay)
            {
                CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }
            int MachineID = Convert.ToInt32(Session["MachineID"]);

            //Gatting UderID
            int opid = Convert.ToInt32(Session["Opid"]);
            List<tbllivehmiscreen> HMIData = obj.GetLiveHMIDetail(MachineID, CorrectedDate);
            //var HMIData = db.tbllivehmiscreens.Where(m => m.CorrectedDate == CorrectedDate && m.Status == 0 && m.MachineID == MachineID).ToList();
            // tblhmiscreen HMI = db.tblhmiscreens.Where(m => m.CorrectedDate == CorrectedDate && m.OperatiorID == opid).Where(m => m.MachineID == MachineID).FirstOrDefault();
            foreach (tbllivehmiscreen HMIRow in HMIData)
            {
                //idea is to update the row if set is clicked.
                string Shift1 = Shift;
                // HMIRow.isUpdate = 1;
                DateTime PEStartTime = DateTime.Now;
                string opdet = HMIRow.OperatorDet;
                int id = HMIRow.HMIID;
                obj.UpdateLiveHMIDetails12(Shift1, id, PEStartTime, opdet);
                //db.Entry(HMIRow).State = System.Data.Entity.EntityState.Modified;
                //db.SaveChanges();

                //HMIScreenForAdmin(MachineID, opid, CorrectedDate); //not doing
                Session["opid"] = opid;
                Session["gopid"] = opid;
                Session["gshift" + opid] = Shift;
                ViewBag.hide = 1;
            }
            return RedirectToAction("Index");
        }

        private List<string> GetMacHierarchyData(int MachineID)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            List<string> HierarchyData = new List<string>();
            //1st get PlantName or -
            //2nd get ShopName or -
            //3rd get CellName or -
            //4th get MachineName.

            tblmachinedetail machineData = obj.GetMachineDetails(MachineID);
            //var machineData = dbMac.tblmachinedetails.Where(m => m.MachineID == MachineID).FirstOrDefault();
            int PlantID = Convert.ToInt32(machineData.PlantID);
            string name = "-";
            name = Convert.ToString(obj.GetPlantDetails(PlantID));
            // name = dbMac.tblplants.Where(m => m.PlantID == PlantID).Select(m => m.PlantName).FirstOrDefault();
            HierarchyData.Add(name);
            int shopid = Convert.ToInt32(machineData.PlantID);
            //string ShopIDString = Convert.ToString(machineData.ShopID);
            //    int value;
            //    if (int.TryParse(ShopIDString, out value))
            //    {
            name = Convert.ToString(obj.GetShopDetails(shopid));
            //name = dbMac.tblshops.Where(m => m.ShopID == value).Select(m => m.ShopName).FirstOrDefault();
            HierarchyData.Add(name.ToString());
            //}
            // else
            // {
            //     HierarchyData.Add("-");
            // }
            int CellID = Convert.ToInt32(machineData.PlantID);
            //string CellIDString = Convert.ToString(machineData.CellID);
            //    if (int.TryParse(CellIDString, out value))
            //    {
            name = Convert.ToString(obj.GetCellDetails(CellID));
            // name = dbMac.tblcells.Where(m => m.CellID == value).Select(m => m.CellName).FirstOrDefault();
            HierarchyData.Add(name.ToString());
            //}
            //else
            //{
            //    HierarchyData.Add("-");
            //}
            HierarchyData.Add(Convert.ToString(machineData.MachineName));
            HierarchyData.Add(Convert.ToString(machineData.MachineDisplayName));
            return HierarchyData;
        }

        //public ActionResult changeVisibilityNorm(int id = 0, int reworkorderhidden = 0, int cjtextbox7 = 0, int cjtextbox8 = 0, int wd = 0)
        //{
        //    _conn = new ConnectionFactory();
        //    obj = new Dao(_conn);

        //    if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
        //    {
        //        return RedirectToAction("Login", "Login", null);
        //    }
        //    ViewBag.Logout = Session["Username"];
        //    ViewBag.roleid = Session["RoleID"];

        //    if (wd == 1)
        //    {
        //        //Getting Shift Value
        //        DateTime Time = DateTime.Now;
        //        TimeSpan Tm = new TimeSpan(Time.Hour, Time.Minute, Time.Second);
        //        var ShiftDetails = obj.GetShiftList(Tm);
        //        //var ShiftDetails = db.tblshift_mstr.Where(m => m.StartTime <= Tm && m.EndTime >= Tm);
        //        string Shift = null;
        //        foreach (var a in ShiftDetails)
        //        {
        //            Shift = a.ShiftName;
        //        }
        //        ViewBag.date = System.DateTime.Now;
        //        if (Shift != null)
        //            ViewBag.shift = Shift;
        //        else
        //            ViewBag.shift = "C";

        //        int machineID = 0;
        //        tbllivehmiscreen tblhmiscreen = obj.GetHMIDetails(id);
        //        //tbllivehmiscreen tblhmiscreen = db.tbllivehmiscreens.Find(id);
        //        machineID = Convert.ToInt32(tblhmiscreen.MachineID);

        //        int Uid = tblhmiscreen.OperatiorID;

        //        int ID = id;
        //        tbllivehmiscreen OldWork = obj.GetHMIDetails(id);
        //        obj.UpdateHMIDetails(cjtextbox7, cjtextbox8, ID);
        //        //tbllivehmiscreen OldWork = db.tbllivehmiscreens.Find(ID);

        //        //OldWork.Rej_Qty = cjtextbox7;
        //        //OldWork.Delivered_Qty = cjtextbox8;
        //        //OldWork.Status = 3;

        //        //update isWorkInProgress When WorkIs finished is clicked.
        //        //else save workInProgress and continue

        //        // OldWork.isWorkInProgress = 0;//work is in progress
        //        if (reworkorderhidden == 1)
        //        {
        //            obj.UpdateHMIDetails1(ID);
        //            // OldWork.isWorkOrder = 1;
        //        }

        //        string Shiftgen = OldWork.Shift;
        //        string operatorName = OldWork.OperatorDet;
        //        int Opgid = OldWork.OperatiorID;
        //        DateTime pestarttime = Convert.ToDateTime(OldWork.PEStartTime);
        //        //get all those data for new row.
        //        string project = OldWork.Project;
        //        string PorF = OldWork.Prod_FAI;
        //        string partno = OldWork.PartNo;
        //        string wono = OldWork.Work_Order_No;
        //        string opno = OldWork.OperationNo;
        //        int target = Convert.ToInt32(OldWork.Target_Qty);
        //        //db.Entry(OldWork).State = System.Data.Entity.EntityState.Modified;
        //        //db.SaveChanges();

        //        string CorrectedDate = null;
        //        tbldaytiming StartTime = obj.GetDaytimingDetails();
        //        // tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
        //        TimeSpan Start = StartTime.StartTime;
        //        if (Start <= DateTime.Now.TimeOfDay)
        //        {
        //            CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
        //        }
        //        else
        //        {
        //            CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        //        }
        //        obj.InsertLIVEHMIDetails(project, PorF, partno, wono, opno, target, machineID, CorrectedDate, pestarttime, Shiftgen, Opgid);           //tbllivehmiscreen NewEntry = new tbllivehmiscreen();
        //        //NewEntry.Project = project;
        //        //NewEntry.Prod_FAI = PorF;
        //        //NewEntry.PartNo = partno;
        //        //NewEntry.Work_Order_No = wono;
        //        //NewEntry.OperationNo = opno;
        //        //NewEntry.Target_Qty = target;
        //        //NewEntry.MachineID = machineID;
        //        //NewEntry.CorrectedDate = CorrectedDate;
        //        //NewEntry.PEStartTime = pestarttime;
        //        ////NewEntry.Date = DateTime.Now;
        //        ////NewEntry.OperatorDet = operatorName;
        //        //NewEntry.Shift = Convert.ToString(Shiftgen);
        //        //NewEntry.Status = 0;
        //        //NewEntry.isWorkInProgress = 2;
        //        //NewEntry.OperatiorID = Opgid;
        //        ////NewEntry.Time = DateTime.Now;
        //        //db.tbllivehmiscreens.Add(NewEntry);
        //        //db.SaveChanges();

        //    }
        //    else
        //    {
        //        obj.UpdateLiveHMIDetails4(id);
        //        //tbllivehmiscreen tblhmiscreen = db.tbllivehmiscreens.Find(id);
        //        //tblhmiscreen.isUpdate = 0;
        //        //db.Entry(tblhmiscreen).State = System.Data.Entity.EntityState.Modified;
        //        //db.SaveChanges();

        //    }
        //    Session["change"] = 1;
        //    return RedirectToAction("Index");
        //}

        public ActionResult changeVisibilityNorm(int wd = 0)
        {
            Session["Mode"] = null;
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            //Getting Shift Value
            string[] ShiftAndCorrectedDate = GetDateShift();
            ViewBag.shift = ShiftAndCorrectedDate[0];

            int machineID = Convert.ToInt32(Session["MachineID"]);

            //New Logic 2017-01-06 . Let them PF or JF Using respective Buttons.
            //var HMIData = db.tblhmiscreens.Where(m => m.MachineID == machineID && m.Status == 0).ToList();
            //if (HMIData.Count > 1)
            //{
            //    Session["VError"] = "Please Click JobFinish or PartialFinish Before Changing Shift";
            //    return RedirectToAction("Index");
            //}
            //else if (HMIData.Count == 1) //This one may be our empty row or not
            //{
            //    string project = Convert.ToString(HMIData[0].Project);
            //    string PartNo = Convert.ToString(HMIData[0].PartNo);
            //    string Work_Order_No = Convert.ToString(HMIData[0].Work_Order_No);
            //    if (HMIData[0].Date != null)
            //    {
            //        Session["VError"] = "Please Finish CurrentJob Before Changing Shift";
            //        return RedirectToAction("Index");
            //    }
            //}

            //2017-02-06 even if 1 of them is submitted then don't allow. Else Delete those rows and Allow.
            List<tbllivehmiscreen> HMIData = obj.GetListHMIScreeDetails(machineID);
            //var HMIData = db.tbllivehmiscreens.Where(m => m.MachineID == machineID && m.Status == 0).ToList();

            foreach (tbllivehmiscreen row in HMIData)
            {
                if (row.Date == null)
                {//Do Nothing

                }
                else //Implies one of them is submitted
                {
                    Session["VError"] = "Please Click JobFinish or PartialFinish Before Changing Shift";
                    return RedirectToAction("Index");
                }
            }

            if (HMIData.Count > 0)
            {
                foreach (tbllivehmiscreen row in HMIData)
                {
                    if (row.DDLWokrCentre == null)
                    {
                        int HMIID = row.HMIID;
                        obj.UpdateLiveHMIScreenDeta(HMIID);
                        //db.Entry(row).State = System.Data.Entity.EntityState.Modified;
                        //db.SaveChanges();
                    }
                    else
                    {
                        obj.DeleteHMIScreenDetails(row.HMIID);
                        //db.tbllivehmiscreens.Remove(row);
                        //db.SaveChanges();
                    }
                }
                return RedirectToAction("Index");
            }



            //1) Check if Basic Details Available for All WO's and are eligible for PF
            //2) else All fields are empty then Allow to change the shift and OpName.

            #region 2017-01-06
            //wd = 0;
            //var HMIData = db.tblhmiscreens.Where(m => m.Status == 0 && m.MachineID == machineID).ToList();
            //foreach (var hmiid in HMIData)
            //{
            //    string project = hmiid.Project;
            //    string WONo = hmiid.Work_Order_No;
            //    string partNo = hmiid.PartNo;
            //    string OpNo = hmiid.OperationNo;

            //    if (string.IsNullOrEmpty(project) && string.IsNullOrEmpty(WONo) && string.IsNullOrEmpty(partNo) && string.IsNullOrEmpty(OpNo))
            //    {
            //    }
            //    else { wd = 1; break; }
            //}

            //if (wd == 1)
            //{
            //    //1.1 check if all rows are elegible for PF (or null)
            //    //1.2 do PF

            //    #region Main Logic OLD Usefull
            //    //OldWork.Status = 3;
            //    //OldWork.isWorkInProgress = 0;//work is in progress
            //    //string Shiftgen = OldWork.Shift;
            //    //string operatorName = OldWork.OperatorDet;
            //    //int Opgid = OldWork.OperatiorID;
            //    //DateTime pestarttime = Convert.ToDateTime(OldWork.PEStartTime);

            //    ////get all those data for new row.
            //    //string project = OldWork.Project;
            //    //string PorF = OldWork.Prod_FAI;
            //    //string partno = OldWork.PartNo;
            //    //string wono = OldWork.Work_Order_No;
            //    //string opno = OldWork.OperationNo;
            //    //int target = Convert.ToInt32(OldWork.Target_Qty);
            //    //db.Entry(OldWork).State = System.Data.Entity.EntityState.Modified;
            //    //db.SaveChanges();

            //    //tblhmiscreen NewEntry = new tblhmiscreen();
            //    //NewEntry.Project = project;
            //    //NewEntry.Prod_FAI = PorF;
            //    //NewEntry.PartNo = partno;
            //    //NewEntry.Work_Order_No = wono;
            //    //NewEntry.OperationNo = opno;
            //    //NewEntry.Target_Qty = target;
            //    //NewEntry.MachineID = machineID;
            //    //NewEntry.CorrectedDate = ShiftAndCorrectedDate[1];
            //    //NewEntry.PEStartTime = pestarttime;
            //    ////NewEntry.Date = DateTime.Now;
            //    ////NewEntry.OperatorDet = operatorName;
            //    //NewEntry.Shift = Convert.ToString(Shiftgen);
            //    //NewEntry.Status = 0;
            //    //NewEntry.isWorkInProgress = 2;
            //    //NewEntry.OperatiorID = Opgid;
            //    ////NewEntry.Time = DateTime.Now;
            //    //db.tblhmiscreens.Add(NewEntry);
            //    //db.SaveChanges();
            //    #endregion

            //    //1.1
            //    foreach (var row in HMIData)
            //    {
            //        int hmiiid = Convert.ToInt32(row.HMIID);
            //        var HMIData1 = db.tblhmiscreens.Where(m => m.HMIID == hmiiid).FirstOrDefault();
            //        if (HMIData1.isWorkInProgress == 3)
            //        {
            //            Session["VError"] = " Please Release HOLD for WONo" + HMIData1.Work_Order_No;
            //            return RedirectToAction("Index");
            //        }
            //        if (HMIData1.Date != null)
            //        {
            //            int deliveredQty = 0;
            //            if (int.TryParse(Convert.ToString(HMIData1.Delivered_Qty), out deliveredQty))
            //            {
            //                if (deliveredQty < Convert.ToInt32(HMIData1.Target_Qty))
            //                {
            //                }
            //                else
            //                {
            //                    Session["VError"] = "DeliveredQty Must be less than Target";
            //                    return RedirectToAction("Index");
            //                }
            //            }
            //            else{
            //                Session["VError"] = " Please enter Delivered Qty for " + HMIData1.Work_Order_No;
            //                return RedirectToAction("Index");
            //            }
            //        }
            //        else
            //        {
            //            string project = HMIData1.Project;
            //            string WONo = HMIData1.Work_Order_No;
            //            string partNo = HMIData1.PartNo;
            //            string OpNo = HMIData1.OperationNo;

            //            if ((!string.IsNullOrEmpty(project)) || (!string.IsNullOrEmpty(WONo)) || (!string.IsNullOrEmpty(partNo)) || (!string.IsNullOrEmpty(OpNo)))
            //            {
            //                Session["VError"] = " Please Click Submit Before Clicking Change ";
            //                return RedirectToAction("Index");
            //            }

            //        }
            //    }
            //    //1.2
            //    foreach (var row in HMIData)
            //    {
            //        int hmiidi = Convert.ToInt32(row.HMIID);
            //        var HMIData1 = db.tblhmiscreens.Where(m => m.HMIID == hmiidi).FirstOrDefault();
            //        HMIData1.Status = 2;
            //        HMIData1.isWorkInProgress = 0;
            //        HMIData1.Time = DateTime.Now;
            //        db.Entry(HMIData1).State = System.Data.Entity.EntityState.Modified;
            //        db.SaveChanges();


            //        //Insert new into HMIScreen with same details for NON Null Rows (?For All)

            //        //tblhmiscreen NewEntry = new tblhmiscreen();
            //        //NewEntry.Project = HMIData1.Project;
            //        //NewEntry.Prod_FAI = HMIData1.Prod_FAI;
            //        //NewEntry.PartNo = HMIData1.PartNo;
            //        //NewEntry.Work_Order_No = HMIData1.Work_Order_No;
            //        //NewEntry.OperationNo = HMIData1.OperationNo;
            //        //NewEntry.Target_Qty = HMIData1.Target_Qty;
            //        //NewEntry.MachineID = machineID;
            //        //NewEntry.CorrectedDate = ShiftAndCorrectedDate[1]; 
            //        //NewEntry.PEStartTime = DateTime.Now;
            //        ////NewEntry.Date = DateTime.Now;
            //        ////NewEntry.OperatorDet = operatorName;
            //        //NewEntry.Shift = ShiftAndCorrectedDate[0];
            //        //NewEntry.Status = 0;
            //        //NewEntry.isWorkInProgress = 2;
            //        //NewEntry.OperatiorID = HMIData1.OperatiorID;
            //        ////NewEntry.Time = DateTime.Now;
            //        //db.tblhmiscreens.Add(NewEntry);
            //        //db.SaveChanges();

            //    }
            //}
            //else
            //{
            //foreach (var row in HMIData)
            //{
            //    row.isUpdate = 0;
            //    db.Entry(row).State = System.Data.Entity.EntityState.Modified;
            //    db.SaveChanges();
            //}

            //}
            #endregion

            foreach (tbllivehmiscreen row in HMIData)
            {
                // row.isUpdate = 0;
                int id = row.HMIID;
                obj.UpdateLiveHMIDetails4(id);
                //db.Entry(row).State = System.Data.Entity.EntityState.Modified;
                //db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public JsonResult IsItLastLevel(int id)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            string hasNextLevel = "no";
            tbllossescode levelData = obj.GetLossDet1(id);
            //var levelData = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodeID == id).FirstOrDefault();
            int level = levelData.LossCodesLevel;
            if (level == 1)
            {
                List<tbllossescode> NextlevelData = obj.GetLosscode1Det1(id);
                //var NextlevelData = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel1ID == id).ToList();
                if (NextlevelData.Count > 0)
                {
                    hasNextLevel = "yes";
                }
            }
            if (level == 2)
            {
                List<tbllossescode> NextlevelData = obj.GetLosscode2Det1(id);
                //var NextlevelData = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel2ID == id).ToList();
                if (NextlevelData.Count > 0)
                {
                    hasNextLevel = "yes";
                }
            }
            if (level == 3)
            {
                hasNextLevel = "no";
            }
            return Json(hasNextLevel, JsonRequestBehavior.AllowGet);
        }

        public JsonResult JsoncheckingIdle()
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            string IsIdle = "false";
            bool isidleMethodRetVal = checkingIdle();
            if (isidleMethodRetVal)
            {
                IsIdle = "true";
            }
            return Json(IsIdle, JsonRequestBehavior.AllowGet);
        }

        #region Commented by monika bcz shakthi team wants WO details based only on machine wise
        //public ActionResult DDLList(int DDLID = 0, string MacInvNo = null, int ToHMI = 0)
        //{
        //    Session["split"] = null;
        //    Session["Mode"] = null;
        //    Session["empty"] = 0;
        //    Session["item"] = null;
        //    Session["redStrat"] = 0;
        //    Session["redEnd"] = 0;
        //    //Session["Split"] = null;
        //    _conn = new ConnectionFactory();
        //    obj = new Dao(_conn);

        //    if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
        //    {
        //        return RedirectToAction("Login", "Login", null);
        //    }
        //    ViewBag.Logout = Session["Username"];
        //    ViewBag.roleid = Session["RoleID"];
        //    string CorrectedDate = null;
        //    tbldaytiming StartTime = obj.GetDaytimingDetails();
        //    //tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
        //    TimeSpan Start = StartTime.StartTime;
        //    if (Start <= DateTime.Now.TimeOfDay)
        //    {
        //        CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
        //    }
        //    else
        //    {
        //        CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        //    }
        //    int machineId = Convert.ToInt32(Session["MachineID"]);
        //    ViewBag.opid = Session["opid"];
        //    ViewBag.mcnid = machineId;
        //    ViewBag.coretddt = CorrectedDate;
        //   // objprev.PreviousHHIDDLUpdation(machineId, CorrectedDate);
        //    //int handleidleReturnValue = HandleIdle();
        //    //if (handleidleReturnValue == 0)
        //    //{
        //    //    return RedirectToAction("DownCodeEntry");
        //    //}
        //    var a = TempData["VError"];
        //    Session["VError"] = null;
        //    Session["VError"] = TempData["VError"];
        //    //Step 1: If DDLID is given then insert that data into HMIScreen table , take its HMIID and redirect to Index 

        //    #region doing this in post method.(2017-05-09)
        //    if (DDLID != 0)
        //    {
        //        int Hmiid = 0;
        //        var ddldata = obj.GetddlDetails(DDLID);
        //        // var ddldata = db.tblddls.Where(m => m.IsCompleted == 0 && m.DDLID == DDLID).FirstOrDefault();
        //        //String SplitWO = ddldata.SplitWO;

        //        String WONo = ddldata.WorkOrder;
        //        String Part = ddldata.MaterialDesc;
        //        String Operation = ddldata.OperationNo;
        //        string Type = ddldata.Type;
        //        #region 2017-02-07 doing this in post method.(2017-05-09)
        //        //bool IsInHMI = false;
        //        //var Siblingddldata = db.tblddls.Where(m => m.IsCompleted == 0 && m.WorkOrder == WONo && m.MaterialDesc == Part && m.OperationNo != Operation).OrderBy(m => new { m.WorkOrder, m.MaterialDesc, m.OperationNo }).ToList();
        //        //foreach (var row in Siblingddldata)
        //        //{
        //        //    IsInHMI = true; //reinitialize
        //        //    int localOPNo = Convert.ToInt32(row.OperationNo);
        //        //    string localOPNoString = Convert.ToString(row.OperationNo);
        //        //    if (localOPNo < Convert.ToInt32(Operation))
        //        //    {
        //        //        #region //Here Check in HMIScreen Table. There are chances that this one is started prior to this round of ddl selection ,
        //        //        //which case is valid.
        //        //        var SiblingHMIdata = db.tblhmiscreens.Where(m => m.Work_Order_No == WONo && m.PartNo == Part && m.OperationNo == localOPNoString).OrderByDescending(m => m.HMIID).FirstOrDefault();
        //        //        if (SiblingHMIdata == null)
        //        //        {
        //        //            Session["VError"] = "Please Select Below WorkOrder, WONo: " + WONo + " PartNo: " + Part + " OperationNo: " + localOPNo;
        //        //            IsInHMI = false;
        //        //            //break;
        //        //        }
        //        //        else
        //        //        {
        //        //            if (SiblingHMIdata.Date == null) //=> lower OpNo is not submitted.
        //        //            {
        //        //                Session["VError"] = " Start WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + localOPNoString;
        //        //                //return RedirectToAction("Index");
        //        //                IsInHMI = false;
        //        //                //break;
        //        //            }
        //        //            else
        //        //            {
        //        //                IsInHMI = true;
        //        //            }
        //        //        }
        //        //        #endregion

        //        //        if (!IsInHMI)
        //        //        {
        //        //            #region //also check in MultiWO table
        //        //            string WIPQueryMultiWO = @"SELECT * from tbl_multiwoselection where WorkOrder = '" + WONo + "' and PartNo = '" + Part + "' and OperationNo = '" + localOPNo + "' order by MultiWOID desc limit 1 ";
        //        //            var WIPMWO = db.tbl_multiwoselection.SqlQuery(WIPQueryMultiWO).ToList();

        //        //            if (WIPMWO.Count == 0)
        //        //            {
        //        //                Session["VError"] = " Select  WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + localOPNoString;
        //        //                //return RedirectToAction("Index");
        //        //                //IsInHMI = false;
        //        //                //break;
        //        //                return View();
        //        //            }

        //        //            foreach (var rowHMI in WIPMWO)
        //        //            {
        //        //                int hmiid = Convert.ToInt32(rowHMI.HMIID);
        //        //                var MWOHMIData = db.tblhmiscreens.Where(m => m.HMIID == hmiid).FirstOrDefault();
        //        //                if (MWOHMIData != null) //obviously != 0
        //        //                {
        //        //                    if (MWOHMIData.Date == null) //=> lower OpNo is not submitted.
        //        //                    {
        //        //                        Session["VError"] = " Start WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + localOPNoString;
        //        //                        //return RedirectToAction("Index");
        //        //                        return View();
        //        //                        //break;
        //        //                    }
        //        //                    else
        //        //                    {
        //        //                    }
        //        //                }
        //        //            }
        //        //            #endregion
        //        //        }
        //        //        else
        //        //        {
        //        //            //continue with other execution
        //        //        }
        //        //    }
        //        //}

        //        /////to Catch those Manual WorkOrders 
        //        //string WIPQuery1 = @"SELECT * from tblhmiscreen where  HMIID IN ( SELECT Max(HMIID) from tblhmiscreen where  HMIID IN  ( SELECT HMIID from tblhmiscreen where Work_Order_No = '" + WONo + "' and PartNo = '" + Part + "' and OperationNo != '" + Operation + "' and  IsMultiWO = 0 and DDLWokrCentre is null order by HMIID desc ) group by Work_Order_No,PartNo,OperationNo ) order by OperationNo ;";
        //        //var WIPDDL1 = db.tblhmiscreens.SqlQuery(WIPQuery1).ToList();
        //        //foreach (var row in WIPDDL1)
        //        //{
        //        //    int InnerOpNo = Convert.ToInt32(row.OperationNo);
        //        //    if (InnerOpNo < Convert.ToInt32(Operation))
        //        //    {
        //        //        string WIPQueryHMI = @"SELECT * from tblhmiscreen where Work_Order_No = '" + WONo + "' and PartNo = '" + Part + "' and OperationNo = '" + InnerOpNo + "' order by HMIID desc limit 1 ";
        //        //        var WIP1 = db.tblhmiscreens.SqlQuery(WIPQueryHMI).ToList();
        //        //        if (WIP1.Count == 0)
        //        //        {
        //        //            Session["VError"] = " Select & Start WONo: " + row.Work_Order_No + " and PartNo: " + row.PartNo + " and OperationNo: " + InnerOpNo;
        //        //            //return RedirectToAction("Index");
        //        //            return View();
        //        //        }
        //        //        foreach (var rowHMI in WIP1)
        //        //        {
        //        //            if (rowHMI.Date == null) //=> lower OpNo is not submitted.
        //        //            {
        //        //                Session["VError"] = " Start WONo: " + row.Work_Order_No + " and PartNo: " + row.PartNo + " and OperationNo: " + InnerOpNo;
        //        //                //return RedirectToAction("Index");
        //        //                return View();
        //        //            }
        //        //        }
        //        //    }
        //        //}
        //        #endregion

        //        //int PrvProcessQty = 0, PrvDeliveredQty = 0;
        //        //var getProcessQty = db.tblhmiscreens.Where(m => m.Work_Order_No == WONo && m.PartNo == Part && m.OperationNo == Operation && m.isWorkInProgress != 2).OrderByDescending(m => m.HMIID).Take(1).ToList();
        //        //if (getProcessQty.Count > 0)
        //        //{
        //        //    PrvProcessQty = Convert.ToInt32(getProcessQty[0].ProcessQty);
        //        //    PrvDeliveredQty = Convert.ToInt32(getProcessQty[0].Delivered_Qty);
        //        //}

        //        #region new code
        //        int PrvProcessQty = 0, PrvDeliveredQty = 0;
        //        //here 1st get latest of delivered and processed among row in tblHMIScreen & tblmulitwoselection
        //        int isHMIFirst = 2; //default NO History for that wo,pn,on
        //        var mulitwoData = obj.GetMultiWOSelectionList(WONo, Part, Operation);
        //        //var mulitwoData = db.tbllivemultiwoselections.Where(m => m.WorkOrder == WONo && m.PartNo == Part && m.OperationNo == Operation && m.tbllivehmiscreen.isWorkInProgress != 2).OrderByDescending(m => m.tbllivehmiscreen.Time).Take(1).ToList();
        //        var hmiData = obj.GetHMIList(WONo, Part, Operation);
        //        // var hmiData = db.tbllivehmiscreens.Where(m => m.Work_Order_No == WONo && m.PartNo == Part && m.OperationNo == Operation && m.isWorkInProgress != 2).OrderByDescending(m => m.Time).Take(1).ToList();

        //        if (hmiData.Count > 0 && mulitwoData.Count > 0) // now check for greatest amongst
        //        {
        //            //DateTime multiwoDateTime = Convert.ToDateTime(mulitwoData[0].CreatedOn); //2017-06-02
        //            //Based on hmiid of  multiwotable get  Time Column of tblhmiscreen 
        //            //int localhmiid = Convert.ToInt32(mulitwoData[0].HMIID);
        //            //var hmiiData = db.tblhmiscreens.Find(localhmiid);

        //            DateTime multiwoDateTime = Convert.ToDateTime(mulitwoData[0].tbllivehmiscreen.Time);
        //            DateTime hmiDateTime = Convert.ToDateTime(hmiData[0].Time);

        //            if (Convert.ToInt32(multiwoDateTime.Subtract(hmiDateTime).TotalSeconds) > 0)
        //            {
        //                isHMIFirst = 1;
        //            }
        //            else
        //            {
        //                isHMIFirst = 0;
        //            }

        //        }
        //        else if (mulitwoData.Count > 0)
        //        {
        //            isHMIFirst = 1;
        //        }
        //        else if (hmiData.Count > 0)
        //        {
        //            isHMIFirst = 0;
        //        }
        //        //bool retstatus = objprev.CalPrevQtyWithWO(WONo, Operation, Part);
        //        int delivInt = 0;
        //        if (isHMIFirst == 1)
        //        {
        //            string delivString = Convert.ToString(mulitwoData[0].DeliveredQty);
        //            delivInt = 0;
        //            int.TryParse(delivString, out delivInt);

        //            string processString = Convert.ToString(mulitwoData[0].ProcessQty);
        //            int procInt = 0;
        //            int.TryParse(processString, out procInt);

        //            PrvProcessQty += procInt;
        //            PrvDeliveredQty += delivInt;
        //        }
        //        else if (isHMIFirst == 0)
        //        {
        //            //if (retstatus == true)
        //            //{
        //            //    string delivString = Convert.ToString(hmiData[0].prevQty);
        //            //    delivInt = 0;
        //            //    int.TryParse(delivString, out delivInt);
        //            //}
        //            //else
        //            //{
        //                string delivString = Convert.ToString(hmiData[0].Delivered_Qty);
        //                delivInt = 0;
        //                int.TryParse(delivString, out delivInt);
        //            //}

        //            string processString = Convert.ToString(hmiData[0].ProcessQty);
        //            int procInt = 0;
        //            int.TryParse(processString, out procInt);

        //            PrvProcessQty += procInt;
        //            PrvDeliveredQty += delivInt;
        //        }
        //        else
        //        {
        //            //no previous delivered or processed qty so Do Nothing.
        //        }

        //        #endregion

        //        int ProcessQty = PrvProcessQty + PrvDeliveredQty;

        //        int TotalProcessQty = Convert.ToInt32(PrvProcessQty + PrvDeliveredQty);
        //        var hmidata = obj.GettbllivehmiscreensDet1(machineId);
        //        //var hmidata = db.tbllivehmiscreens.Where(m => m.MachineID == machineId && m.isWorkInProgress == 2).OrderByDescending(m => m.HMIID).FirstOrDefault();
        //        //hmidata.Date = DateTime.Now;

        //        int Hmiid1 = hmidata.HMIID;
        //        //delete if any IsSubmit = 0 for this hmiid.
        //        obj.deleteMultiWOSlectionDetails2(Hmiid1);
        //        //db.tbllivemultiwoselections.RemoveRange(db.tbllivemultiwoselections.Where(x => x.HMIID == Hmiid1 && x.IsSubmit == 0));
        //        //db.SaveChanges();
        //        var Target_Qty = Convert.ToInt32(ddldata.TargetQty);



        //        //hmidata.OperationNo = ddldata.OperationNo;
        //        //hmidata.PartNo = ddldata.MaterialDesc;
        //        //hmidata.Project = ddldata.Project;
        //        //hmidata.SplitWO = "0";
        //        //hmidata.Target_Qty = Convert.ToInt32(ddldata.TargetQty);
        //        //hmidata.Work_Order_No = ddldata.WorkOrder;
        //        //hmidata.ProcessQty = TotalProcessQty;
        //        //hmidata.Delivered_Qty = 0;
        //        //hmidata.DDLWokrCentre = ddldata.WorkCenter;
        //        //Hmiid = hmidata.HMIID;
        //        //hmidata.IsMultiWO = 0;
        //        //db.Entry(hmidata).State = System.Data.Entity.EntityState.Modified;
        //        //db.SaveChanges();
        //        Session["FromDDL"] = 1;
        //        Session["SubmitClicked"] = 0;
        //        return RedirectToAction("Index", Hmiid);
        //    }
        //    #endregion

        //    //Step2: If DDLID == 0 and ToHMI == 1 then go to HMIScreen "Index" With Normal HMI Flow
        //    // This means Operator opted for Manual Entry
        //    if (DDLID == 0 && ToHMI == 1)
        //    {
        //        var hmidata = obj.GettbllivehmiscreensDet1(machineId);
        //        //var hmidata = db.tbllivehmiscreens.Where(m => m.MachineID == machineId && m.isWorkInProgress == 2).OrderByDescending(m => m.HMIID).FirstOrDefault();

        //        int Hmiid = hmidata.HMIID;
        //        //delete if any IsSubmit = 0 for this hmiid.
        //        obj.deleteMultiWOSlectionDetails2(Hmiid);
        //        //db.tbllivemultiwoselections.RemoveRange(db.tbllivemultiwoselections.Where(x => x.HMIID == Hmiid && x.IsSubmit == 0));
        //        //db.SaveChanges();
        //        obj.UpdateLiveHMI3(Hmiid);
        //        //hmidata.OperationNo = null;
        //        //hmidata.PartNo = null;
        //        //hmidata.Project = null;
        //        //hmidata.Target_Qty = null;
        //        //hmidata.Work_Order_No = null;
        //        //hmidata.ProcessQty = 0;
        //        //hmidata.Delivered_Qty = 0;
        //        //hmidata.DDLWokrCentre = null;
        //        //hmidata.IsMultiWO = 0;
        //        //db.Entry(hmidata).State = System.Data.Entity.EntityState.Modified;
        //        //db.SaveChanges();
        //        Session["FromDDL"] = 2;
        //        return RedirectToAction("Index");
        //    }

        //    //Step 3: If DDLID == 0, then go to DDLList page.

        //    int MacId = Convert.ToInt32(Session["MachineID"]);
        //    ViewBag.machineData = obj.GetMachineList1(MacId);
        //    // ViewBag.machineData = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MacId).Select(m => m.MachineInvNo).ToList();
        //    var oneMacData = obj.GetOneMachineDet(MacId);
        //    //var oneMacData = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MacId).FirstOrDefault();
        //    string cellidstring = Convert.ToString(oneMacData.CellID);
        //    string shopidstring = Convert.ToString(oneMacData.ShopID);
        //    int shopid;
        //    int.TryParse(shopidstring, out shopid);
        //    int cellid;
        //    if (int.TryParse(cellidstring, out cellid) && int.TryParse(shopidstring, out shopid))
        //    {
        //        List<tblmachinedetail> macList = new List<tblmachinedetail>();
        //        var cellmachinedata = obj.GetCellMachineList1(cellid);
        //        macList.AddRange(cellmachinedata);
        //        // macList.AddRange(db.tblmachinedetails.Where(m => m.CellID == cellid && m.IsDeleted == 0 && !m.ManualWCID.HasValue).Select(m => m.MachineInvNo).ToList());
        //        macList.AddRange(obj.GetShopMachineList1(cellid, shopid));
        //        //macList.AddRange(db.tblmachinedetails.Where(m => m.ShopID == shopid && m.CellID != cellid && m.IsDeleted == 0 && !m.ManualWCID.HasValue).Select(m => m.MachineInvNo).ToList());

        //        //ViewBag.machineData = db.tblmachinedetails.Where(m => m.CellID == cellid && m.IsDeleted == 0).Select(m => m.MachineInvNo).ToList();
        //        //ViewBag.machineData += db.tblmachinedetails.Where(m => m.ShopID == shopid &&  m.CellID != cellid  && m.IsDeleted == 0).Select(m => m.MachineInvNo).ToList();
        //        ViewBag.machineData = macList;
        //    }
        //    else
        //    {
        //        if (int.TryParse(shopidstring, out shopid))
        //        {
        //            ViewBag.machineData = obj.GetShopList3(shopid);
        //            //ViewBag.machineData = from row in db.tblmachinedetails
        //            //                      where row.ShopID == shopid && row.IsDeleted == 0 && row.CellID.Equals(null) && !row.ManualWCID.HasValue
        //            //                      select row.MachineInvNo;
        //        }
        //        else
        //        {
        //            string plantidstring = Convert.ToString(oneMacData.PlantID);
        //            int plantid;
        //            if (int.TryParse(plantidstring, out plantid))
        //            {
        //                ViewBag.machineData = obj.GetPlantList3(plantid);
        //                //ViewBag.machineData = from row in db.tblmachinedetails
        //                //                      where row.PlantID == plantid && row.IsDeleted == 0 && row.ShopID.Equals(null) && row.CellID.Equals(null) && !row.ManualWCID.HasValue
        //                //                      select row.MachineInvNo;
        //            }
        //        }
        //    }
        //    string machineInvNo = null;
        //    if (MacInvNo == null)
        //    {
        //        var machinedata = obj.GetMachineDet2(machineId);
        //        //var machinedata = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == machineId && m.IsNormalWC == 0).FirstOrDefault();
        //        Session["macDispName"] = Convert.ToString(machinedata.MachineDisplayName);
        //        machineInvNo = machinedata.MachineName;
        //    }
        //    else
        //    {
        //        machineInvNo = MacInvNo;
        //    }
        //    ViewBag.error = Session["Error"];
        //    if (ViewBag.error == null)
        //    {
        //        Session["Error"] = TempData["Error"];
        //    }
        //    //ViewBag.MacInvNo = machineInvNo;
        //    List<tblddl> ddlDataList = new List<tblddl>();
        //    //string WIPQuery = @"SELECT * from tbllivehmiscreen where isWorkInProgress = 0 and HMIID IN ( SELECT HMIID from tbllivehmiscreen as h where h.MachineID = " + machineId + " order by h.Date)  ";

        //    //var WIP = db.tbllivehmiscreens.SqlQuery(WIPQuery).ToList();
        //    var WIP = obj.GetDDLList3(machineId);
        //    List<string> ExceptionDDLs = new List<string>();
        //    var ddldata1 = obj.GetDDLList2(machineInvNo);
        //    // var ddldata1 = db.tblddls.Where(m => m.WorkCenter == machineInvNo && m.IsCompleted == 0).ToList();
        //    if (ddldata1 != null)
        //    {

        //        foreach (var row in ddldata1)
        //        {
        //            string ddlid = row.DDLID.ToString();
        //            ExceptionDDLs.Add(ddlid);
        //        }

        //        ddlDataList = ddldata1;
        //    }

        //    foreach (var row in WIP)
        //    {
        //        string wono = row.Work_Order_No;
        //        string partno = row.PartNo;
        //        string opno = row.OperationNo;
        //        var ddldata = obj.GetDDLDet1(wono, partno, opno);
        //        //var ddldata = db.tblddls.Where(m => m.WorkOrder == wono && m.MaterialDesc == partno && m.OperationNo == opno && m.IsCompleted == 0).FirstOrDefault();
        //        if (ddldata != null)
        //        {
        //            string ddlid = ddldata.DDLID.ToString();
        //            ExceptionDDLs.Add(ddlid);
        //            //ddlDataList.Add(ddldata);
        //        }
        //    }
        //    string ExceptionDDLsArray = null;
        //    if (ExceptionDDLs.Count > 0)
        //    {
        //        ExceptionDDLsArray = String.Join(",", ExceptionDDLs);
        //    }
        //    else
        //    {
        //        ExceptionDDLsArray = "0";
        //    }


        //    //String Query = "select * " +
        //    //                "from tblddl WHERE WorkCenter = '" + machineInvNo + "' AND IsCompleted = 0  AND DDLID NOT IN (" + ExceptionDDLsArray + ")" +
        //    //                "order by DaysAgeing = '', Convert(DaysAgeing , SIGNED INTEGER) asc ,FlagRushInd = '',FlagRushInd = 0 ,Convert(FlagRushInd , SIGNED INTEGER) asc  , MADDateInd = '' , MADDateInd asc , MADDate asc";
        //    //ddlDataList.AddRange(db.tblddls.SqlQuery(Query).ToList());
        //    //  ddlDataList.AddRange(obj.GetDDLList1(machineInvNo, ExceptionDDLsArray));
        //    ViewBag.MacInvNo = machineInvNo;
        //    Session["MacInvNo"] = machineInvNo;
        //    // Used in Else Before 2017-01-07 // var WIP = db.tblddls.Where(m => m.WorkCenter == machineInvNo && m.IsCompleted == 0).ToList();
        //    if (ddlDataList.Count != 0)
        //    {
        //        return View(ddlDataList.ToList());
        //    }
        //    else
        //    {
        //        return View();
        //    }
        //}
        #endregion
        [HttpGet]
        public ActionResult DDLList(int DDLID = 0, string MacInvNo = null, int ToHMI = 0)
        {
            Session["split"] = null;
            Session["Mode"] = null;
            Session["empty"] = 0;
            Session["item"] = null;
            Session["redStrat"] = 0;
            Session["redEnd"] = 0;
            //Session["Split"] = null;
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            string CorrectedDate = null;
            tbldaytiming StartTime = obj.GetDaytimingDetails();
            //tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
            TimeSpan Start = StartTime.StartTime;
            if (Start <= DateTime.Now.TimeOfDay)
            {
                CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }
            int machineId = Convert.ToInt32(Session["MachineID"]);
            ViewBag.opid = Session["opid"];
            ViewBag.mcnid = machineId;
            ViewBag.coretddt = CorrectedDate;
            // objprev.PreviousHHIDDLUpdation(machineId, CorrectedDate);
            //int handleidleReturnValue = HandleIdle();
            //if (handleidleReturnValue == 0)
            //{
            //    return RedirectToAction("DownCodeEntry");
            //}
            object a = TempData["VError"];
            Session["VError"] = null;
            Session["VError"] = TempData["VError"];
            //Step 1: If DDLID is given then insert that data into HMIScreen table , take its HMIID and redirect to Index 

            tbllivemode poweroffdata = (obj.Getlivemodestatus(machineId, CorrectedDate));

            if (poweroffdata != null)
            {
                {
                    Session["VError"] = " Machine is in Poweroff, Operator can't able to start wos ";
                    return RedirectToAction("Index");
                }
            }

            #region doing this in post method.(2017-05-09)
            if (DDLID != 0)
            {
                int Hmiid = 0;
                tblddl ddldata = obj.GetddlDetails(DDLID);
                // var ddldata = db.tblddls.Where(m => m.IsCompleted == 0 && m.DDLID == DDLID).FirstOrDefault();
                //String SplitWO = ddldata.SplitWO;

                String WONo = ddldata.WorkOrder;
                String Part = ddldata.MaterialDesc;
                String Operation = ddldata.OperationNo;
                string Type = ddldata.Type;
                #region 2017-02-07 doing this in post method.(2017-05-09)
                //bool IsInHMI = false;
                //var Siblingddldata = db.tblddls.Where(m => m.IsCompleted == 0 && m.WorkOrder == WONo && m.MaterialDesc == Part && m.OperationNo != Operation).OrderBy(m => new { m.WorkOrder, m.MaterialDesc, m.OperationNo }).ToList();
                //foreach (var row in Siblingddldata)
                //{
                //    IsInHMI = true; //reinitialize
                //    int localOPNo = Convert.ToInt32(row.OperationNo);
                //    string localOPNoString = Convert.ToString(row.OperationNo);
                //    if (localOPNo < Convert.ToInt32(Operation))
                //    {
                //        #region //Here Check in HMIScreen Table. There are chances that this one is started prior to this round of ddl selection ,
                //        //which case is valid.
                //        var SiblingHMIdata = db.tblhmiscreens.Where(m => m.Work_Order_No == WONo && m.PartNo == Part && m.OperationNo == localOPNoString).OrderByDescending(m => m.HMIID).FirstOrDefault();
                //        if (SiblingHMIdata == null)
                //        {
                //            Session["VError"] = "Please Select Below WorkOrder, WONo: " + WONo + " PartNo: " + Part + " OperationNo: " + localOPNo;
                //            IsInHMI = false;
                //            //break;
                //        }
                //        else
                //        {
                //            if (SiblingHMIdata.Date == null) //=> lower OpNo is not submitted.
                //            {
                //                Session["VError"] = " Start WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + localOPNoString;
                //                //return RedirectToAction("Index");
                //                IsInHMI = false;
                //                //break;
                //            }
                //            else
                //            {
                //                IsInHMI = true;
                //            }
                //        }
                //        #endregion

                //        if (!IsInHMI)
                //        {
                //            #region //also check in MultiWO table
                //            string WIPQueryMultiWO = @"SELECT * from tbl_multiwoselection where WorkOrder = '" + WONo + "' and PartNo = '" + Part + "' and OperationNo = '" + localOPNo + "' order by MultiWOID desc limit 1 ";
                //            var WIPMWO = db.tbl_multiwoselection.SqlQuery(WIPQueryMultiWO).ToList();

                //            if (WIPMWO.Count == 0)
                //            {
                //                Session["VError"] = " Select  WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + localOPNoString;
                //                //return RedirectToAction("Index");
                //                //IsInHMI = false;
                //                //break;
                //                return View();
                //            }

                //            foreach (var rowHMI in WIPMWO)
                //            {
                //                int hmiid = Convert.ToInt32(rowHMI.HMIID);
                //                var MWOHMIData = db.tblhmiscreens.Where(m => m.HMIID == hmiid).FirstOrDefault();
                //                if (MWOHMIData != null) //obviously != 0
                //                {
                //                    if (MWOHMIData.Date == null) //=> lower OpNo is not submitted.
                //                    {
                //                        Session["VError"] = " Start WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + localOPNoString;
                //                        //return RedirectToAction("Index");
                //                        return View();
                //                        //break;
                //                    }
                //                    else
                //                    {
                //                    }
                //                }
                //            }
                //            #endregion
                //        }
                //        else
                //        {
                //            //continue with other execution
                //        }
                //    }
                //}

                /////to Catch those Manual WorkOrders 
                //string WIPQuery1 = @"SELECT * from tblhmiscreen where  HMIID IN ( SELECT Max(HMIID) from tblhmiscreen where  HMIID IN  ( SELECT HMIID from tblhmiscreen where Work_Order_No = '" + WONo + "' and PartNo = '" + Part + "' and OperationNo != '" + Operation + "' and  IsMultiWO = 0 and DDLWokrCentre is null order by HMIID desc ) group by Work_Order_No,PartNo,OperationNo ) order by OperationNo ;";
                //var WIPDDL1 = db.tblhmiscreens.SqlQuery(WIPQuery1).ToList();
                //foreach (var row in WIPDDL1)
                //{
                //    int InnerOpNo = Convert.ToInt32(row.OperationNo);
                //    if (InnerOpNo < Convert.ToInt32(Operation))
                //    {
                //        string WIPQueryHMI = @"SELECT * from tblhmiscreen where Work_Order_No = '" + WONo + "' and PartNo = '" + Part + "' and OperationNo = '" + InnerOpNo + "' order by HMIID desc limit 1 ";
                //        var WIP1 = db.tblhmiscreens.SqlQuery(WIPQueryHMI).ToList();
                //        if (WIP1.Count == 0)
                //        {
                //            Session["VError"] = " Select & Start WONo: " + row.Work_Order_No + " and PartNo: " + row.PartNo + " and OperationNo: " + InnerOpNo;
                //            //return RedirectToAction("Index");
                //            return View();
                //        }
                //        foreach (var rowHMI in WIP1)
                //        {
                //            if (rowHMI.Date == null) //=> lower OpNo is not submitted.
                //            {
                //                Session["VError"] = " Start WONo: " + row.Work_Order_No + " and PartNo: " + row.PartNo + " and OperationNo: " + InnerOpNo;
                //                //return RedirectToAction("Index");
                //                return View();
                //            }
                //        }
                //    }
                //}
                #endregion

                //int PrvProcessQty = 0, PrvDeliveredQty = 0;
                //var getProcessQty = db.tblhmiscreens.Where(m => m.Work_Order_No == WONo && m.PartNo == Part && m.OperationNo == Operation && m.isWorkInProgress != 2).OrderByDescending(m => m.HMIID).Take(1).ToList();
                //if (getProcessQty.Count > 0)
                //{
                //    PrvProcessQty = Convert.ToInt32(getProcessQty[0].ProcessQty);
                //    PrvDeliveredQty = Convert.ToInt32(getProcessQty[0].Delivered_Qty);
                //}

                #region new code
                int PrvProcessQty = 0, PrvDeliveredQty = 0;
                //here 1st get latest of delivered and processed among row in tblHMIScreen & tblmulitwoselection
                int isHMIFirst = 2; //default NO History for that wo,pn,on
                List<tbllivemultiwoselection> mulitwoData = obj.GetMultiWOSelectionList(WONo, Part, Operation);
                //var mulitwoData = db.tbllivemultiwoselections.Where(m => m.WorkOrder == WONo && m.PartNo == Part && m.OperationNo == Operation && m.tbllivehmiscreen.isWorkInProgress != 2).OrderByDescending(m => m.tbllivehmiscreen.Time).Take(1).ToList();
                List<tbllivehmiscreen> hmiData = obj.GetHMIList(WONo, Part, Operation);
                // var hmiData = db.tbllivehmiscreens.Where(m => m.Work_Order_No == WONo && m.PartNo == Part && m.OperationNo == Operation && m.isWorkInProgress != 2).OrderByDescending(m => m.Time).Take(1).ToList();

                if (hmiData.Count > 0 && mulitwoData.Count > 0) // now check for greatest amongst
                {
                    //DateTime multiwoDateTime = Convert.ToDateTime(mulitwoData[0].CreatedOn); //2017-06-02
                    //Based on hmiid of  multiwotable get  Time Column of tblhmiscreen 
                    //int localhmiid = Convert.ToInt32(mulitwoData[0].HMIID);
                    //var hmiiData = db.tblhmiscreens.Find(localhmiid);

                    DateTime multiwoDateTime = Convert.ToDateTime(mulitwoData[0].tbllivehmiscreen.Time);
                    DateTime hmiDateTime = Convert.ToDateTime(hmiData[0].Time);

                    if (Convert.ToInt32(multiwoDateTime.Subtract(hmiDateTime).TotalSeconds) > 0)
                    {
                        isHMIFirst = 1;
                    }
                    else
                    {
                        isHMIFirst = 0;
                    }

                }
                else if (mulitwoData.Count > 0)
                {
                    isHMIFirst = 1;
                }
                else if (hmiData.Count > 0)
                {
                    isHMIFirst = 0;
                }
                //bool retstatus = objprev.CalPrevQtyWithWO(WONo, Operation, Part);
                int delivInt = 0;
                if (isHMIFirst == 1)
                {
                    string delivString = Convert.ToString(mulitwoData[0].DeliveredQty);
                    delivInt = 0;
                    int.TryParse(delivString, out delivInt);

                    string processString = Convert.ToString(mulitwoData[0].ProcessQty);
                    int procInt = 0;
                    int.TryParse(processString, out procInt);

                    PrvProcessQty += procInt;
                    PrvDeliveredQty += delivInt;
                }
                else if (isHMIFirst == 0)
                {
                    //if (retstatus == true)
                    //{
                    //    string delivString = Convert.ToString(hmiData[0].prevQty);
                    //    delivInt = 0;
                    //    int.TryParse(delivString, out delivInt);
                    //}
                    //else
                    //{
                    string delivString = Convert.ToString(hmiData[0].Delivered_Qty);
                    delivInt = 0;
                    int.TryParse(delivString, out delivInt);
                    //}

                    string processString = Convert.ToString(hmiData[0].ProcessQty);
                    int procInt = 0;
                    int.TryParse(processString, out procInt);

                    PrvProcessQty += procInt;
                    PrvDeliveredQty += delivInt;
                }
                else
                {
                    //no previous delivered or processed qty so Do Nothing.
                }

                #endregion

                int ProcessQty = PrvProcessQty + PrvDeliveredQty;

                int TotalProcessQty = Convert.ToInt32(PrvProcessQty + PrvDeliveredQty);
                tbllivehmiscreen hmidata = obj.GettbllivehmiscreensDet1(machineId);
                //var hmidata = db.tbllivehmiscreens.Where(m => m.MachineID == machineId && m.isWorkInProgress == 2).OrderByDescending(m => m.HMIID).FirstOrDefault();
                //hmidata.Date = DateTime.Now;

                int Hmiid1 = hmidata.HMIID;
                //delete if any IsSubmit = 0 for this hmiid.
                obj.deleteMultiWOSlectionDetails2(Hmiid1);
                //db.tbllivemultiwoselections.RemoveRange(db.tbllivemultiwoselections.Where(x => x.HMIID == Hmiid1 && x.IsSubmit == 0));
                //db.SaveChanges();
                int Target_Qty = Convert.ToInt32(ddldata.TargetQty);



                //hmidata.OperationNo = ddldata.OperationNo;
                //hmidata.PartNo = ddldata.MaterialDesc;
                //hmidata.Project = ddldata.Project;
                //hmidata.SplitWO = "0";
                //hmidata.Target_Qty = Convert.ToInt32(ddldata.TargetQty);
                //hmidata.Work_Order_No = ddldata.WorkOrder;
                //hmidata.ProcessQty = TotalProcessQty;
                //hmidata.Delivered_Qty = 0;
                //hmidata.DDLWokrCentre = ddldata.WorkCenter;
                //Hmiid = hmidata.HMIID;
                //hmidata.IsMultiWO = 0;
                //db.Entry(hmidata).State = System.Data.Entity.EntityState.Modified;
                //db.SaveChanges();
                Session["FromDDL"] = 1;
                Session["SubmitClicked"] = 0;
                return RedirectToAction("Index", Hmiid);
            }
            #endregion

            //Step2: If DDLID == 0 and ToHMI == 1 then go to HMIScreen "Index" With Normal HMI Flow
            // This means Operator opted for Manual Entry
            if (DDLID == 0 && ToHMI == 1)
            {
                tbllivehmiscreen hmidata = obj.GettbllivehmiscreensDet1(machineId);
                //var hmidata = db.tbllivehmiscreens.Where(m => m.MachineID == machineId && m.isWorkInProgress == 2).OrderByDescending(m => m.HMIID).FirstOrDefault();

                int Hmiid = hmidata.HMIID;
                //delete if any IsSubmit = 0 for this hmiid.
                obj.deleteMultiWOSlectionDetails2(Hmiid);
                //db.tbllivemultiwoselections.RemoveRange(db.tbllivemultiwoselections.Where(x => x.HMIID == Hmiid && x.IsSubmit == 0));
                //db.SaveChanges();
                obj.UpdateLiveHMI3(Hmiid);
                //hmidata.OperationNo = null;
                //hmidata.PartNo = null;
                //hmidata.Project = null;
                //hmidata.Target_Qty = null;
                //hmidata.Work_Order_No = null;
                //hmidata.ProcessQty = 0;
                //hmidata.Delivered_Qty = 0;
                //hmidata.DDLWokrCentre = null;
                //hmidata.IsMultiWO = 0;
                //db.Entry(hmidata).State = System.Data.Entity.EntityState.Modified;
                //db.SaveChanges();
                Session["FromDDL"] = 2;
                return RedirectToAction("Index");
            }

            //Step 3: If DDLID == 0, then go to DDLList page.

            int MacId = Convert.ToInt32(Session["MachineID"]);
            ViewBag.machineData = obj.GetMachineList1(MacId);
            // ViewBag.machineData = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MacId).Select(m => m.MachineInvNo).ToList();
            tblmachinedetail oneMacData = obj.GetOneMachineDet(MacId);
            //var oneMacData = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MacId).FirstOrDefault();
            string cellidstring = Convert.ToString(oneMacData.CellID);
            string shopidstring = Convert.ToString(oneMacData.ShopID);
            int shopid;
            int.TryParse(shopidstring, out shopid);


            if (int.TryParse(shopidstring, out shopid))
            {
                ViewBag.machineData = obj.GetShopList3(shopid);
                //ViewBag.machineData = from row in db.tblmachinedetails
                //                      where row.ShopID == shopid && row.IsDeleted == 0 && row.CellID.Equals(null) && !row.ManualWCID.HasValue
                //                      select row.MachineInvNo;
            }
            else
            {
                string plantidstring = Convert.ToString(oneMacData.PlantID);
                int plantid;
                if (int.TryParse(plantidstring, out plantid))
                {
                    ViewBag.machineData = obj.GetPlantList3(plantid);
                    //ViewBag.machineData = from row in db.tblmachinedetails
                    //                      where row.PlantID == plantid && row.IsDeleted == 0 && row.ShopID.Equals(null) && row.CellID.Equals(null) && !row.ManualWCID.HasValue
                    //                      select row.MachineInvNo;
                }
            }

            string machineInvNo = null;
            if (MacInvNo == null)
            {
                tblmachinedetail machinedata = obj.GetMachineDet2(machineId);
                //var machinedata = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == machineId && m.IsNormalWC == 0).FirstOrDefault();
                Session["macDispName"] = Convert.ToString(machinedata.MachineDisplayName);
                machineInvNo = machinedata.MachineName;
                if (machineInvNo.Contains("\r\n"))
                {
                    machineInvNo = machineInvNo.Replace("\r\n", "");
                }
            }
            else
            {
                machineInvNo = MacInvNo;
            }
            ViewBag.error = Session["Error"];
            if (ViewBag.error == null)
            {
                Session["Error"] = TempData["Error"];
            }
            //ViewBag.MacInvNo = machineInvNo;
            List<tblddl> ddlDataList = new List<tblddl>();
            //string WIPQuery = @"SELECT * from tbllivehmiscreen where isWorkInProgress = 0 and HMIID IN ( SELECT HMIID from tbllivehmiscreen as h where h.MachineID = " + machineId + " order by h.Date)  ";

            //var WIP = db.tbllivehmiscreens.SqlQuery(WIPQuery).ToList();
            List<tbllivehmiscreen> WIP = obj.GetDDLList3(machineId);
            List<string> ExceptionDDLs = new List<string>();
            List<tblddl> ddldata1 = obj.GetDDLList2(machineInvNo);
            // var ddldata1 = db.tblddls.Where(m => m.WorkCenter == machineInvNo && m.IsCompleted == 0).ToList();
            if (ddldata1 != null)
            {

                foreach (tblddl row in ddldata1)
                {
                    string ddlid = row.DDLID.ToString();
                    ExceptionDDLs.Add(ddlid);
                }

                ddlDataList = ddldata1;
            }

            foreach (tbllivehmiscreen row in WIP)
            {
                string wono = row.Work_Order_No;
                string partno = row.PartNo;
                string opno = row.OperationNo;
                tblddl ddldata = obj.GetDDLDet1(wono, partno, opno);
                //var ddldata = db.tblddls.Where(m => m.WorkOrder == wono && m.MaterialDesc == partno && m.OperationNo == opno && m.IsCompleted == 0).FirstOrDefault();
                if (ddldata != null)
                {
                    string ddlid = ddldata.DDLID.ToString();
                    ExceptionDDLs.Add(ddlid);
                    //ddlDataList.Add(ddldata);
                }
            }
            string ExceptionDDLsArray = null;
            if (ExceptionDDLs.Count > 0)
            {
                ExceptionDDLsArray = String.Join(",", ExceptionDDLs);
            }
            else
            {
                ExceptionDDLsArray = "0";
            }


            //String Query = "select * " +
            //                "from tblddl WHERE WorkCenter = '" + machineInvNo + "' AND IsCompleted = 0  AND DDLID NOT IN (" + ExceptionDDLsArray + ")" +
            //                "order by DaysAgeing = '', Convert(DaysAgeing , SIGNED INTEGER) asc ,FlagRushInd = '',FlagRushInd = 0 ,Convert(FlagRushInd , SIGNED INTEGER) asc  , MADDateInd = '' , MADDateInd asc , MADDate asc";
            //ddlDataList.AddRange(db.tblddls.SqlQuery(Query).ToList());
            //  ddlDataList.AddRange(obj.GetDDLList1(machineInvNo, ExceptionDDLsArray));
            ViewBag.MacInvNo = machineInvNo;
            Session["MacInvNo"] = machineInvNo;
            // Used in Else Before 2017-01-07 // var WIP = db.tblddls.Where(m => m.WorkCenter == machineInvNo && m.IsCompleted == 0).ToList();
            if (ddlDataList.Count != 0)
            {
                return View(ddlDataList.ToList());
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult DDLList(string data1, string selectedValue = "")
        {
            Session["split"] = null;
            Session["Mode"] = null;
            Session["empty"] = 0;
            Session["item"] = null;

            Session["redStrat"] = 0;
            Session["redEnd"] = 0;
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            List<int> data = new List<int>();
            if (data1 != null)
            {
                data = JsonConvert.DeserializeObject<List<int>>(data1);
            }

            ViewBag.selectedValues = selectedValue;

            //if (IDLEorGenericWorkisON())
            //{
            //    Session["Error"] = "Please End IDLE/GenericWork Before Selecting New Work Orders";
            //    return RedirectToAction("Index", new { selectedValues = selectedValue });
            //}

            string[] DateShiftValues = GetDateShift();
            string CorrectedDate = DateShiftValues[1];
            int WOCount = data.Count;
            int machineId = Convert.ToInt32(Session["MachineID"]);
            string OpName = Convert.ToString(obj.GetLiveHMIScreenDetails43(machineId));

            string Shift = Convert.ToString(obj.GetLiveHMIScreenDetails44(machineId));
            //string OpName = db.tbllivehmiscreens.Where(m => m.MachineID == machineId && m.Status == 0).Select(m => m.OperatorDet).FirstOrDefault();
            //string Shift = db.tbllivehmiscreens.Where(m => m.MachineID == machineId && m.Status == 0).Select(m => m.Shift).FirstOrDefault();

            int invalidDDLID = 0;
            if (WOCount == 1)
            {
                int DDLID = data.First();

                //1st check for eligibility for wo,part,opno sequence condition.
                bool isValid = true;
                string IssueMsg = null;


                if (DDLID != 0)
                {
                    //var ddldataInner = db.tblddls.Where(m => m.IsCompleted == 0 && m.DDLID == DDLID).FirstOrDefault();
                    tblddl ddldataInner = obj.GetddlDetails1(DDLID);
                    String SplitWOInner = ddldataInner.SplitWO;
                    String WONoInner = ddldataInner.WorkOrder;
                    String PartInner = ddldataInner.MaterialDesc;
                    String OperationInner = ddldataInner.OperationNo;
                    string type = ddldataInner.Type;
                    if (type == null)
                    {
                        type = "Production";
                    }
                    //var DuplicateHMIdata = db.tblhmiscreens.Where(m => m.Work_Order_No == WONoInner && m.PartNo == PartInner && m.OperationNo == OperationInner && (m.isWorkInProgress == 2 || m.isWorkInProgress == 3)).FirstOrDefault();
                    tbllivehmiscreen DuplicateHMIdata = obj.GetListH1MIDetails(WONoInner, PartInner, OperationInner, machineId);
                    // var DuplicateHMIdata = db.tbllivehmiscreens.Where(m => m.Work_Order_No == WONoInner && m.PartNo == PartInner && m.OperationNo == OperationInner && m.isWorkInProgress == 2 && m.MachineID == machineId).FirstOrDefault();
                    if (DuplicateHMIdata != null)
                    {
                        isValid = false;
                        IssueMsg = "This Below WorkOrder, WONo: " + WONoInner + " PartNo: " + PartInner + " OperationNo: " + OperationInner + " Exists in PartEntry Screen";
                    }

                    #region 2017-02-07
                    if (isValid)
                    {
                        List<tblddl> Siblingddldata = obj.Getddl1Details1(WONoInner, PartInner, OperationInner);
                        // var Siblingddldata = db.tblddls.Where(m => m.IsCompleted == 0 && m.WorkOrder == WONoInner && m.MaterialDesc == PartInner && m.OperationNo != OperationInner && m.IsCompleted == 0).OrderBy(m => new { m.WorkOrder, m.MaterialDesc, m.OperationNo }).ToList();
                        foreach (tblddl row in Siblingddldata)
                        {
                            int localOPNo = Convert.ToInt32(row.OperationNo);
                            string localOPNoString = Convert.ToString(row.OperationNo);
                            if (localOPNo < Convert.ToInt32(OperationInner))
                            {
                                //Here Check in HMIScreen Table. There are chances that this one is started prior to this round of ddl selection ,
                                //which case is valid.
                                tbllivehmiscreen SiblingHMIdata = obj.GetLive1HMIDetails(WONoInner, PartInner, localOPNoString);
                                //var SiblingHMIdata = db.tbllivehmiscreens.Where(m => m.Work_Order_No == WONoInner && m.PartNo == PartInner && m.OperationNo == localOPNoString).FirstOrDefault();
                                //var SiblingHMIdatahistorian = db.tblhmiscreens.Where(m => m.Work_Order_No == WONoInner && m.PartNo == PartInner && m.OperationNo == localOPNoString).FirstOrDefault(); //added by Ashok
                                if (SiblingHMIdata == null)// || SiblingHMIdatahistorian==null) //its not in hmi Screen as Individual WO so Error
                                {
                                    invalidDDLID = ddldataInner.DDLID;
                                    //IssueMsg = "Please Select Below WorkOrder , WONo: " + WONoInner + " PartNo: " + PartInner + " OperationNo: " + localOPNo+" ";
                                    //IsInHMI = false;
                                    //isValid = false;
                                    string outOperation = "";
                                    // bool retstatus= CheckWhetherWoStartedOrNot(WONoInner, Opno.ToString(), PartInner);
                                    bool retstatus = CheckAlltheWO(WONoInner, PartInner, OperationInner, out outOperation);
                                    if (!retstatus)
                                    {
                                        IssueMsg = " Select  WONo: " + WONoInner + " and PartNo: " + PartInner + " and OperationNo: " + outOperation;

                                        //return RedirectToAction("Index");
                                        isValid = false;
                                        break;
                                    }
                                    //break;
                                }
                                else
                                {
                                    if (SiblingHMIdata.Date == null)//|| SiblingHMIdatahistorian.Date == null) //=> lower OpNo is not submitted.
                                    {
                                        IssueMsg = " Start WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + localOPNoString;
                                        isValid = false;
                                        break;
                                    }
                                    else
                                    {
                                        int Opno = Convert.ToInt32(row.OperationNo);
                                        tblddl LeastOperation = obj.GetLive1HMIDetails1(WONoInner, PartInner, Opno);
                                        if (LeastOperation != null)
                                        {
                                            string outOperation = "";
                                            // bool retstatus= CheckWhetherWoStartedOrNot(WONoInner, Opno.ToString(), PartInner);
                                            bool retstatus = CheckAlltheWO(WONoInner, PartInner, OperationInner, out outOperation);
                                            if (!retstatus)
                                            {
                                                IssueMsg = " Select  WONo: " + LeastOperation.WorkOrder + " and PartNo: " + LeastOperation.MaterialDesc + " and OperationNo: " + outOperation;

                                                //return RedirectToAction("Index");
                                                isValid = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                                //if (!IsInHMI)
                                //{
                                //    //also check in MultiWO table
                                //    //string WIPQueryMultiWO = @"SELECT * from tbllivemultiwoselection where WorkOrder = '" + WONoInner + "' and PartNo = '" + PartInner + "' and OperationNo = '" + localOPNo + "' order by MultiWOID limit 1 ";
                                //    //var WIPMWO = db.tbllivemultiwoselections.SqlQuery(WIPQueryMultiWO).ToList();
                                //    var WIPMWO = obj.GetMultiWOtDetails(WONoInner, PartInner, Convert.ToString(localOPNo));
                                //    if (WIPMWO.Count == 0)
                                //    {
                                //        IssueMsg = " Select  WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + localOPNoString;
                                //        //return RedirectToAction("Index");
                                //        isValid = false;
                                //        IsInHMI = false;
                                //        break;
                                //    }

                                //    foreach (var rowHMI in WIPMWO)
                                //    {
                                //        int hmiid = Convert.ToInt32(rowHMI.HMIID);
                                //        //var MWOHMIData = db.tbllivehmiscreens.Where(m => m.HMIID == hmiid).FirstOrDefault();
                                //        var MWOHMIData = obj.GetLiveHMIDetails7(hmiid);
                                //        if (MWOHMIData != null)
                                //        {
                                //            if (MWOHMIData.Date == null) //=> lower OpNo is not submitted.
                                //            {
                                //                IssueMsg = " Start WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + localOPNoString;
                                //                //return RedirectToAction("Index");
                                //                isValid = false;
                                //                break;
                                //            }
                                //            else
                                //            {
                                //                IsInHMI = true;
                                //            }
                                //        }
                                //    }
                                //}

                            }
                        }
                        //if is's a Manual Entry then u r allowing so directly check in HMIScreen. Do This Please.

                    }
                    #endregion

                }
                //if (isValid && IsInHMI)
                if (isValid)
                {
                    #region StartWO

                    //var ddldata = db.tblddls.Where(m => m.IsCompleted == 0 && m.DDLID == DDLID).FirstOrDefault();
                    tblddl ddldata = obj.GetddlDetails1(DDLID);
                    String SplitWO = ddldata.SplitWO;
                    String WONo = ddldata.WorkOrder;
                    String Part = ddldata.MaterialDesc;
                    String Operation = ddldata.OperationNo;
                    string type = ddldata.Type;

                    if (type == null)
                    {
                        type = "Production";
                    }

                    int PrvProcessQty = 0, PrvDeliveredQty = 0, TotalProcessQty = 0, ishold = 0;
                    //var getProcessQty = db.tblhmiscreens.Where(m => m.Work_Order_No == WONo && m.PartNo == Part && m.OperationNo == Operation && m.isWorkInProgress != 2).OrderByDescending(m => m.Time).Take(1).ToList();
                    //if (getProcessQty.Count > 0)
                    //{
                    //    string delivString = Convert.ToString(getProcessQty[0].Delivered_Qty);
                    //    int.TryParse(delivString, out PrvDeliveredQty);

                    //    string processString = Convert.ToString(getProcessQty[0].ProcessQty);
                    //    int.TryParse(processString, out PrvProcessQty);

                    //    TotalProcessQty = Convert.ToInt32(PrvProcessQty + PrvDeliveredQty);

                    //    ishold = getProcessQty[0].IsHold;
                    //    ishold = ishold == 2 ? 0 : ishold;
                    //}

                    #region new code

                    //here 1st get latest of delivered and processed among row in tblHMIScreen & tblmulitwoselection
                    int isHMIFirst = 2; //default NO History for that wo,pn,on
                    List<tbllivemultiwoselection> mulitwoData = obj.GetMultiWODetails2(WONo, Part, Operation);
                    // var mulitwoData = db.tbllivemultiwoselections.Where(m => m.WorkOrder == WONo && m.PartNo == Part && m.OperationNo == Operation && m.tbllivehmiscreen.isWorkInProgress != 2).OrderByDescending(m => m.tbllivehmiscreen.Time).Take(1).ToList();
                    List<tbllivehmiscreen> hmiData = obj.GetHMIList(WONo, Part, Operation);
                    //var hmiData = db.tbllivehmiscreens.Where(m => m.Work_Order_No == WONo && m.PartNo == Part && m.OperationNo == Operation && m.isWorkInProgress != 2).OrderByDescending(m => m.Time).Take(1).ToList();

                    if (hmiData.Count > 0 && mulitwoData.Count > 0) // now check for greatest amongst
                    {
                        //DateTime multiwoDateTime = Convert.ToDateTime(mulitwoData[0].CreatedOn); //2017-06-02
                        //Based on hmiid of  multiwotable get  Time Column of tblhmiscreen 
                        //int localhmiid = Convert.ToInt32(mulitwoData[0].HMIID);
                        //var hmiiData = db.tblhmiscreens.Find(localhmiid);
                        DateTime multiwoDateTime = Convert.ToDateTime(mulitwoData[0].tbllivehmiscreen.Time);
                        DateTime hmiDateTime = Convert.ToDateTime(hmiData[0].Time);

                        if (Convert.ToInt32(multiwoDateTime.Subtract(hmiDateTime).TotalSeconds) > 0)
                        {
                            isHMIFirst = 1;
                        }
                        else
                        {
                            isHMIFirst = 0;
                        }

                    }
                    else if (mulitwoData.Count > 0)
                    {
                        isHMIFirst = 1;
                    }
                    else if (hmiData.Count > 0)
                    {
                        isHMIFirst = 0;
                    }

                    if (isHMIFirst == 1)
                    {
                        string delivString = Convert.ToString(mulitwoData[0].DeliveredQty);
                        int delivInt = 0;
                        int.TryParse(delivString, out delivInt);

                        string processString = Convert.ToString(mulitwoData[0].ProcessQty);
                        int procInt = 0;
                        int.TryParse(processString, out procInt);

                        PrvProcessQty += procInt;
                        PrvDeliveredQty += delivInt;

                        ishold = mulitwoData[0].tbllivehmiscreen.IsHold;
                        ishold = ishold == 2 ? 0 : ishold;

                    }
                    else if (isHMIFirst == 0)
                    {
                        string delivString = Convert.ToString(hmiData[0].Delivered_Qty);
                        int delivInt = 0;
                        int.TryParse(delivString, out delivInt);

                        string processString = Convert.ToString(hmiData[0].ProcessQty);
                        int procInt = 0;
                        int.TryParse(processString, out procInt);

                        PrvProcessQty += procInt;
                        PrvDeliveredQty += delivInt;

                        ishold = hmiData[0].IsHold;
                        ishold = ishold == 2 ? 0 : ishold;
                    }
                    else
                    {
                        //no previous delivered or processed qty so Do Nothing.
                    }

                    #endregion
                    TotalProcessQty = PrvProcessQty + PrvDeliveredQty;

                    if (hmiData.Count != 0)
                    {
                        if (hmiData[0].SplitWO == "Yes")
                        {
                            TotalProcessQty = 0;
                        }
                    }
                    //var hmidata = db.tblhmiscreens.Where(m => m.MachineID == machineId && m.isWorkInProgress == 2).OrderByDescending(m => m.HMIID).FirstOrDefault();
                    //hmidata.Date = DateTime.Now;
                    //Hmiid = hmidata.HMIID;

                    ////delete if any IsSubmit = 0 for this hmiid.
                    //db.tbl_multiwoselection.RemoveRange(db.tbl_multiwoselection.Where(x => x.HMIID == Hmiid && x.IsSubmit == 0));
                    //db.SaveChanges();

                    tbllivehmiscreen hmidata = new tbllivehmiscreen();
                    //
                    //hmidata.PEStartTime = DateTime.Now;
                    //hmidata.OperationNo = ddldata.OperationNo;
                    //hmidata.PartNo = ddldata.MaterialDesc;
                    //hmidata.Project = ddldata.Project;
                    //hmidata.CorrectedDate = CorrectedDate;
                    //hmidata.Shift = Shift;
                    //hmidata.Status = 0;
                    //hmidata.OperatiorID = Convert.ToInt32(Session["UserID"]);
                    //hmidata.OperatorDet = OpName;
                    //hmidata.Target_Qty = Convert.ToInt32(ddldata.TargetQty);
                    //hmidata.Work_Order_No = ddldata.WorkOrder;
                    //hmidata.ProcessQty = TotalProcessQty;
                    //hmidata.Delivered_Qty = 0;
                    //hmidata.IsMultiWO = 0;
                    //hmidata.isWorkInProgress = 2;
                    //hmidata.IsHold = ishold;
                    //hmidata.DDLWokrCentre = ddldata.WorkCenter;
                    //hmidata.MachineID = machineId;
                    int ReworkOrder = 0;
                    string ReworkOrderString = Convert.ToString(Session["isWorkOrder"]);
                    if (int.TryParse(ReworkOrderString, out ReworkOrder))
                    {
                        if (ReworkOrderString == "1")
                        {
                            hmidata.isWorkOrder = 1;
                        }
                        else
                        {
                            hmidata.isWorkOrder = 0;
                        }
                    }
                    // hmidata.HMIID = ;
                    obj.InsertLiveHMIDetailsddl(DateTime.Now, ddldata.OperationNo, ddldata.MaterialDesc, ddldata.Project, CorrectedDate, Shift, ddldata.Type, Convert.ToInt32(Session["UserID"]), OpName, Convert.ToInt32(ddldata.TargetQty), ddldata.WorkOrder, hmidata.isWorkOrder, TotalProcessQty, ishold, ddldata.WorkCenter, machineId);
                    //db.tbllivehmiscreens.Add(hmidata);
                    //db.SaveChanges();
                    Session["ddl"] = 1;
                    return RedirectToAction("Index", new { selectedValues = selectedValue });

                    #endregion
                }
                else
                {
                    Session["Error"] = IssueMsg;
                    TempData["Err"] = IssueMsg;
                    ViewBag.Err = IssueMsg;
                    return RedirectToAction("DDLList", "HMIScree", new { DDLID = invalidDDLID });
                }

                Session["FromDDL"] = 1;


            }
            else if (WOCount > 1)
            {
                string DDLIDString = string.Join(",", data.Select(x => x.ToString()).ToArray());
                //var DDLData = db.tblddls.SqlQuery(DDLQuery).ToList();
                List<tblddl> DDLData = obj.GetLiveHMIDetails1(DDLIDString);
                bool isValid = true;
                List<HistoryHMI> HmiList = new List<HistoryHMI>();
                foreach (tblddl DDLRow in DDLData)
                {
                    isValid = true;
                    int DDLID = DDLRow.DDLID;
                    //var ddldataInner = db.tblddls.Where(m => m.IsCompleted == 0 && m.DDLID == DDLID).FirstOrDefault();
                    tblddl ddldataInner = obj.GetddlDetails1(DDLID);
                    String SplitWOInner = ddldataInner.SplitWO;
                    String WONoInner = ddldataInner.WorkOrder;
                    String PartInner = ddldataInner.MaterialDesc;
                    String OperationInner = ddldataInner.OperationNo;
                    string Type = ddldataInner.Type;
                    if (Type == null)
                    {
                        Type = "Production";
                    }
                    tbllivehmiscreen LiveHMIList = obj.HMIList(WONoInner, OperationInner);
                    if (LiveHMIList == null)
                    {
                        tblhmiscreen HistHMIDet = obj.OLDHMI(WONoInner, OperationInner);
                        if (HistHMIDet != null)
                        {
                            if (HistHMIDet.Date != null && HistHMIDet.Time != null && HistHMIDet.Status == 1 && HistHMIDet.isWorkInProgress == 0)
                            {
                                isValid = true;
                            }
                            else if (HistHMIDet.Date != null && HistHMIDet.Time == null)
                            {


                                List<tblhmiscreen> HistHMIList = obj.OldHistDet(WONoInner, OperationInner);
                                foreach (tblhmiscreen item in HistHMIList)
                                {
                                    HistoryHMI obj = new HistoryHMI();
                                    obj.WorkorderNo = item.Work_Order_No;
                                    obj.OperationNo = item.OperationNo;
                                    obj.Errmsg = "This WorkOrder is already Started";
                                    HmiList.Add(obj);
                                    isValid = false;
                                }
                            }
                            else if (HistHMIDet.Date == null)
                            {

                                List<tblhmiscreen> HistHMItabDet = obj.OldHistDet1(WONoInner, OperationInner);
                                foreach (tblhmiscreen item in HistHMItabDet)
                                {
                                    HistoryHMI obj = new HistoryHMI();
                                    obj.WorkorderNo = item.Work_Order_No;
                                    obj.OperationNo = item.OperationNo;
                                    obj.Errmsg = "This WorkOrder is already Selected";
                                    HmiList.Add(obj);
                                    isValid = false;
                                }

                            }
                            else
                            {
                                tblddl ddlList = obj.DDLList(WONoInner, OperationInner);
                                if (ddlList != null)
                                {
                                    List<tblhmiscreen> HMI = obj.OldHistDet2(WONoInner, ddlList.OperationNo);
                                    if (HMI != null)
                                    {
                                        if (HMI.Count == 0)
                                        {
                                            string HMI1 = obj.tblHistoryHMIDet(WONoInner);
                                            tblddl DDL1 = obj.DDLList1(WONoInner, HMI1);
                                            //var IssueMsg = " Select  WONo: " + DDL1.WorkOrder + " and PartNo: " + DDL1.MaterialDesc + " and              OperationNo: " + DDL1.OperationNo;
                                            ViewBag.toaster_error = " Select  WONo: " + DDL1.WorkOrder + " and PartNo: " + DDL1.MaterialDesc + " and OperationNo: " + DDL1.OperationNo;
                                            //Session["Error"] = IssueMsg;
                                            //Session.Timeout = 1;
                                            isValid = false;
                                        }

                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (LiveHMIList.Date != null && LiveHMIList.Time != null && LiveHMIList.Status == 1 && LiveHMIList.isWorkInProgress == 0)
                        {
                            isValid = true;
                        }
                        else if (LiveHMIList.Date != null && LiveHMIList.Time == null)
                        {


                            List<tbllivehmiscreen> HistHMIList = obj.LiveHMIDetails(WONoInner, OperationInner);
                            foreach (tbllivehmiscreen item in HistHMIList)
                            {
                                HistoryHMI obj = new HistoryHMI();
                                obj.WorkorderNo = item.Work_Order_No;
                                obj.OperationNo = item.OperationNo;
                                obj.Errmsg = "This WorkOrder is already Started";
                                HmiList.Add(obj);
                                isValid = false;
                            }
                        }
                        else if (LiveHMIList.Date == null)
                        {

                            List<tbllivehmiscreen> HistHMItabDet = obj.LiveHMIDetails1(WONoInner, OperationInner);
                            foreach (tbllivehmiscreen item in HistHMItabDet)
                            {
                                HistoryHMI obj = new HistoryHMI();
                                obj.WorkorderNo = item.Work_Order_No;
                                obj.OperationNo = item.OperationNo;
                                obj.Errmsg = "This WorkOrder is already Selected";
                                HmiList.Add(obj);
                                isValid = false;
                            }
                        }
                        else
                        {
                            tblddl ddlList = obj.DDLList(WONoInner, OperationInner);
                            if (ddlList != null)
                            {
                                List<tbllivehmiscreen> HMI = obj.LiveHMIDetails2(WONoInner, ddlList.OperationNo);
                                if (HMI != null)
                                {
                                    if (HMI.Count == 0)
                                    {
                                        HistoryHMI obj1 = new HistoryHMI();
                                        string HMI1 = obj.tblLiveHMIDetails(WONoInner);
                                        tblddl DDL1 = obj.DDLList1(WONoInner, HMI1);

                                        obj1.WorkorderNo = DDL1.WorkOrder;
                                        obj1.OperationNo = DDL1.OperationNo;
                                        obj1.PartNo = DDL1.MaterialDesc;
                                        obj1.Errmsg = "Select Work Order:" + DDL1.WorkOrder + " Operation No:" + DDL1.OperationNo + " Part No:" + DDL1.MaterialDesc + " First";
                                        HmiList.Add(obj1);
                                        isValid = false;
                                    }
                                }
                            }
                        }
                    }

                    if (isValid)
                    {
                        tblddl ddlList = obj.GetddlDetails1(DDLID);
                        int PrvProcessQty = 0, PrvDeliveredQty = 0, TotalProcessQty = 0, ishold = 0;

                        #region new code

                        //here 1st get latest of delivered and processed among row in tblHMIScreen & tblmulitwoselection
                        int isHMIFirst = 2; //default NO History for that wo,pn,on

                        List<tbllivemultiwoselection> mulitwoData = obj.GetMultiWODetails2(WONoInner, PartInner, OperationInner);
                        List<tbllivehmiscreen> hmiData = obj.GetHMIList(WONoInner, PartInner, OperationInner);

                        if (hmiData.Count > 0 && mulitwoData.Count > 0) // now check for greatest amongst
                        {
                            DateTime multiwoDateTime = Convert.ToDateTime(mulitwoData[0].tbllivehmiscreen.Time);
                            DateTime hmiDateTime = Convert.ToDateTime(hmiData[0].Time);

                            if (Convert.ToInt32(multiwoDateTime.Subtract(hmiDateTime).TotalSeconds) > 0)
                            {
                                isHMIFirst = 1;
                            }
                            else
                            {
                                isHMIFirst = 0;
                            }

                        }
                        else if (mulitwoData.Count > 0)
                        {
                            isHMIFirst = 1;
                        }
                        else if (hmiData.Count > 0)
                        {
                            isHMIFirst = 0;
                        }

                        if (isHMIFirst == 1)
                        {
                            string delivString = Convert.ToString(mulitwoData[0].DeliveredQty);
                            int delivInt = 0;
                            int.TryParse(delivString, out delivInt);

                            string processString = Convert.ToString(mulitwoData[0].ProcessQty);
                            int procInt = 0;
                            int.TryParse(processString, out procInt);

                            PrvProcessQty += procInt;
                            PrvDeliveredQty += delivInt;

                            ishold = mulitwoData[0].tbllivehmiscreen.IsHold;
                            ishold = ishold == 2 ? 0 : ishold;

                        }
                        else if (isHMIFirst == 0)
                        {
                            string delivString = Convert.ToString(hmiData[0].Delivered_Qty);
                            int delivInt = 0;
                            int.TryParse(delivString, out delivInt);

                            string processString = Convert.ToString(hmiData[0].ProcessQty);
                            int procInt = 0;
                            int.TryParse(processString, out procInt);

                            PrvProcessQty += procInt;
                            PrvDeliveredQty += delivInt;

                            ishold = hmiData[0].IsHold;
                            ishold = ishold == 2 ? 0 : ishold;
                        }
                        else
                        {
                            //no previous delivered or processed qty so Do Nothing.
                        }

                        #endregion
                        TotalProcessQty = PrvProcessQty + PrvDeliveredQty;
                        int ReworkOrder = 0;
                        string ReworkOrderString = Convert.ToString(Session["isWorkOrder"]);

                        tbllivehmiscreen hmidata = new tbllivehmiscreen();
                        if (int.TryParse(ReworkOrderString, out ReworkOrder))
                        {

                            if (ReworkOrderString == "1")
                            {
                                hmidata.isWorkOrder = 1;
                            }
                            else
                            {
                                hmidata.isWorkOrder = 0;
                            }
                        }
                        obj.InsertLiveHMIDetailsddl(DateTime.Now, ddlList.OperationNo, ddlList.MaterialDesc, ddlList.Project, CorrectedDate, Shift, ddlList.Type, Convert.ToInt32(Session["UserID"]), OpName, Convert.ToInt32(ddlList.TargetQty), ddlList.WorkOrder, hmidata.isWorkOrder, TotalProcessQty, 0, ddlList.WorkCenter, machineId);
                    }
                }

                Session["item"] = HmiList;
                Session["ddl"] = 1;
                return RedirectToAction("Index", new { selectedValues = selectedValue, message = ViewBag.toaster_error });
            }

            return RedirectToAction("Index", new { selectedValues = selectedValue });
            //return View();
        }




        //public JsonResult GetMacSiblings()
        //{
        //    string MacData = null;
        //    int MacId = Convert.ToInt32(Session["MachineID"]);
        //    var machineData = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MacId).ToList();
        //    var oneMacData = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MacId).FirstOrDefault();
        //    string cellidstring = Convert.ToString(oneMacData.CellID);
        //    if (cellidstring != null)
        //    {
        //        int cellid = Convert.ToInt32(oneMacData.CellID);
        //        machineData = db.tblmachinedetails.Where(m => m.CellID == cellid && m.IsDeleted == 0).ToList();
        //    }
        //    else
        //    {
        //        string shopidstring = Convert.ToString(oneMacData.ShopID);
        //        if (shopidstring != null)
        //        {
        //            int shopid = Convert.ToInt32(oneMacData.ShopID);
        //            machineData = db.tblmachinedetails.Where(m => m.ShopID == shopid && m.IsDeleted == 0).ToList();
        //        }
        //        else
        //        {
        //            string plantidstring = Convert.ToString(oneMacData.PlantID);
        //            if (plantidstring != null)
        //            {
        //                int plantid = Convert.ToInt32(oneMacData.PlantID);
        //                machineData = db.tblmachinedetails.Where(m => m.PlantID == plantid && m.IsDeleted == 0).ToList();
        //            }
        //        }
        //    }

        //    foreach (var row in machineData)
        //    {
        //        //MacData = "yes";
        //        MacData += @"<Button class='BringWo'> <span id='" + row.MachineInvNo + "' class='macInvNo'> " + row.MachineInvNo + " </span> </Button>";
        //    }

        //    return Json(MacData, JsonRequestBehavior.AllowGet);
        //}

        public JsonResult JsonIdleChecker()
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            string retStatus = "false";
            int machineID = Convert.ToInt32(Session["MachineID"]);
            List<tbllivelossofentry> CurrentStatusData = obj.GettbllivelossofentriesList4(machineID);
            // var CurrentStatusData = db.tbllivelossofentries.Where(m => m.MachineID == machineID && (m.IsScreen == 1 || m.IsStart == 1)).OrderByDescending(m => m.StartDateTime).Take(1).ToList();
            if (CurrentStatusData.Count > 0)
            {
                int LossID = CurrentStatusData[0].LossID;
                int IsStart = Convert.ToInt32(CurrentStatusData[0].IsStart);
                int IsScreen = Convert.ToInt32(CurrentStatusData[0].IsScreen);
                int forRefresh = Convert.ToInt32(CurrentStatusData[0].ForRefresh);
                if (IsStart == 1 && IsScreen == 0 && forRefresh == 0)
                {
                    retStatus = "true";
                    //obj.UpdateLossofentriesDetails(machineID);
                    obj.UpdateLossofentriesDetails(LossID);
                    //CurrentStatusData[0].ForRefresh = 1;
                    //db.Entry(CurrentStatusData[0]).State = System.Data.Entity.EntityState.Modified;
                    //db.SaveChanges();
                }
                if (IsStart == 1 && IsScreen == 1 && forRefresh == 1) //loss code has been repeadtly entered , so dont popup just show screen
                {
                    retStatus = "true";
                    //obj.UpdateLossofentriesDetails1(machineID);
                    obj.UpdateLossofentriesDetails(LossID);
                    //CurrentStatusData[0].ForRefresh = 2;
                    //db.Entry(CurrentStatusData[0]).State = System.Data.Entity.EntityState.Modified;
                    //db.SaveChanges();
                }
            }

            return Json(retStatus, JsonRequestBehavior.AllowGet);
        }

        public bool CheckAlltheWO(string woNo, string opNo, string partNo, out string OperationNum)
        {
            OperationNum = "";
            bool ret = false;
            List<Hmi> hmilist = new List<Hmi>();
            List<tblddl> ddldet = obj.GettblddlWOList(woNo, partNo, opNo);
            ddldet = ddldet.OrderBy(m => Convert.ToInt32(m.OperationNo)).ToList();


            foreach (tblddl ddlrow in ddldet)
            {
                Hmi hmiobj = new Hmi();
                tbllivehmiscreen livehmidet = obj.validatelivehmiscreensDet(ddlrow.WorkOrder, ddlrow.MaterialDesc, ddlrow.OperationNo);
                if (livehmidet != null)
                {
                    if (livehmidet.Date == null && livehmidet.Time == null)
                    {
                        hmiobj.OperationNo = livehmidet.OperationNo;
                        hmiobj.status = true;
                        hmilist.Add(hmiobj);
                    }
                    else if (livehmidet.Date != null && livehmidet.Time == null)
                    {
                        hmiobj.OperationNo = livehmidet.OperationNo;
                        hmiobj.status = true;
                        hmilist.Add(hmiobj);
                    }
                    else if (livehmidet.Date != null && livehmidet.Time != null)
                    {
                        hmiobj.OperationNo = livehmidet.OperationNo;
                        hmiobj.status = true;
                        hmilist.Add(hmiobj);
                    }
                }
                else
                {
                    tblhmiscreen histhmidet = obj.validateHisthmiscreensDet(ddlrow.WorkOrder, ddlrow.MaterialDesc, ddlrow.OperationNo);
                    if (histhmidet != null)
                    {
                        if (histhmidet.Date == null && histhmidet.Time == null)
                        {
                            hmiobj.OperationNo = histhmidet.OperationNo;
                            hmiobj.status = true;
                            hmilist.Add(hmiobj);
                        }
                        else if (histhmidet.Date != null && histhmidet.Time == null)
                        {
                            hmiobj.OperationNo = histhmidet.OperationNo;
                            hmiobj.status = true;
                            hmilist.Add(hmiobj);
                        }
                        else if (histhmidet.Date != null && histhmidet.Time != null)
                        {
                            hmiobj.OperationNo = histhmidet.OperationNo;
                            hmiobj.status = true;
                            hmilist.Add(hmiobj);
                        }
                    }
                    else
                    {
                        hmiobj.OperationNo = ddlrow.OperationNo;
                        hmiobj.status = false;
                        hmilist.Add(hmiobj);
                    }
                }
            }
            foreach (Hmi hmilistrow in hmilist)
            {
                if (hmilistrow.status == false)
                {
                    OperationNum = hmilistrow.OperationNo;
                    ret = false;
                    break;
                }
                else
                {
                    ret = true;
                }
            }

            return ret;
        }

        public bool CheckAlltheWOForFinish(string woNo, string opNo, string partNo, out string OperationNum)
        {
            OperationNum = "";
            bool ret = false;
            List<Hmi> hmilist = new List<Hmi>();
            List<tblddl> ddldet = obj.GettblddlWOList(woNo, opNo, partNo);
            ddldet = ddldet.OrderBy(m => Convert.ToInt32(m.OperationNo)).ToList();

            foreach (tblddl ddlrow in ddldet)
            {
                Hmi hmiobj = new Hmi();
                tbllivehmiscreen livehmidet = obj.validatelivehmiscreensDet(ddlrow.WorkOrder, ddlrow.MaterialDesc, ddlrow.OperationNo);
                if (livehmidet != null)
                {
                    //if (livehmidet.Date == null && livehmidet.Time == null)
                    //{
                    //    hmiobj.OperationNo = livehmidet.OperationNo;
                    //    hmiobj.status = true;
                    //    hmilist.Add(hmiobj);
                    //}
                    //else if (livehmidet.Date != null && livehmidet.Time == null)
                    //{
                    //    hmiobj.OperationNo = livehmidet.OperationNo;
                    //    hmiobj.status = true;
                    //    hmilist.Add(hmiobj);
                    //}
                    if ((livehmidet.Date != null && livehmidet.Time != null && livehmidet.isWorkInProgress == 1))
                    {
                        hmiobj.OperationNo = livehmidet.OperationNo;
                        hmiobj.status = true;
                        hmilist.Add(hmiobj);
                    }
                    else
                    {
                        hmiobj.OperationNo = livehmidet.OperationNo;
                        hmiobj.status = false;
                        hmilist.Add(hmiobj);
                    }
                }
                else
                {
                    tblhmiscreen histhmidet = obj.validateHisthmiscreensDet(ddlrow.WorkOrder, ddlrow.MaterialDesc, ddlrow.OperationNo);
                    if (histhmidet != null)
                    {
                        //if (histhmidet.Date == null && histhmidet.Time == null)
                        //{
                        //    hmiobj.OperationNo = histhmidet.OperationNo;
                        //    hmiobj.status = true;
                        //    hmilist.Add(hmiobj);
                        //}
                        //else if (histhmidet.Date != null && histhmidet.Time == null)
                        //{
                        //    hmiobj.OperationNo = histhmidet.OperationNo;
                        //    hmiobj.status = true;
                        //    hmilist.Add(hmiobj);
                        //}
                        if (histhmidet.Date != null && histhmidet.Time != null && histhmidet.isWorkInProgress == 1 && ddlrow.IsCompleted == 1)
                        {
                            hmiobj.OperationNo = histhmidet.OperationNo;
                            hmiobj.status = true;
                            hmilist.Add(hmiobj);
                        }
                        else
                        {
                            hmiobj.OperationNo = histhmidet.OperationNo;
                            hmiobj.status = false;
                            hmilist.Add(hmiobj);
                        }
                    }
                    else
                    {
                        hmiobj.OperationNo = ddlrow.OperationNo;
                        hmiobj.status = false;
                        hmilist.Add(hmiobj);
                    }
                }
            }
            foreach (Hmi hmilistrow in hmilist)
            {
                if (hmilistrow.status == false)
                {
                    OperationNum = hmilistrow.OperationNo;
                    ret = false;
                    break;
                }
                else
                {
                    ret = true;
                }
            }

            return ret;
        }

        public JsonResult JsonIdleEndChecker()
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            string retStatus = "false";
            int machineID = Convert.ToInt32(Session["MachineID"]);
            tbllivelossofentry CurrentStatusData = obj.GettbllivelossofentryDet(machineID);
            // var CurrentStatusData = db.tbllivelossofentries.Where(m => m.MachineID == machineID).OrderByDescending(m => m.StartDateTime).FirstOrDefault();
            if (CurrentStatusData != null)
            {
                int IsDone = Convert.ToInt32(CurrentStatusData.DoneWithRow);
                if (IsDone == 1)
                {
                    retStatus = "true";
                }
            }
            return Json(retStatus, JsonRequestBehavior.AllowGet);
        }

        public JsonResult JsonRemoveWO(int hmiid) // Remove WorkOrder if Its Not Started.
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            string retStatus = "false";

            tbllivehmiscreen CurrentStatusData = obj.GetLiveHMIDet1(hmiid);
            //var CurrentStatusData = dbhmi.tbllivehmiscreens.Where(m => m.HMIID == hmiid).FirstOrDefault();
            if (CurrentStatusData != null && CurrentStatusData.IsMultiWO == 1)
            {
                try
                {
                    obj.GettbllivemultiwoselectionsList(hmiid);
                    //dbhmi.tbllivemultiwoselections.RemoveRange(dbhmi.tbllivemultiwoselections.Where(m => m.HMIID == hmiid).ToList());
                    //dbhmi.SaveChanges();
                }
                catch (Exception)
                {

                }
            }
            if (CurrentStatusData != null && CurrentStatusData.Date == null)
            {
                obj.deleteMultiWOSlectionDetails3(hmiid);
                //dbhmi.tbllivehmiscreens.Remove(CurrentStatusData);
                //    dbhmi.SaveChanges();
                retStatus = "true";
            }
            return Json(retStatus, JsonRequestBehavior.AllowGet);
        }

        public JsonResult IsMultiWOAllowable(string id)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            string status = "no";
            int machineId = Convert.ToInt32(Session["MachineID"]);
            tblmachinedetail machineDATA = obj.GetMachineDet1(id);
            //var machineDATA = db.tblmachinedetails.Where(m => m.MachineInvNo == id).FirstOrDefault();
            string PlantID = Convert.ToString(machineDATA.PlantID);
            string ShopID = Convert.ToString(machineDATA.ShopID);
            string CellID = Convert.ToString(machineDATA.CellID);
            string WCID = Convert.ToString(machineDATA.MachineID);

            int value = 0;
            if (int.TryParse(WCID, out value))
            {
                List<tblmultipleworkorder> MultiWoWCData = obj.GettblmultipleworkordersList1(value);
                // var MultiWoWCData = db.tblmultipleworkorders.Where(m => m.IsDeleted == 0 && m.WCID == value).ToList();
                if (MultiWoWCData.Count > 0)
                {
                    status = "yes";
                }
            }
            if (int.TryParse(CellID, out value))
            {
                List<tblmultipleworkorder> MultiWoCellData = obj.GettblmultipleworkordersList2(value);
                //var MultiWoCellData = db.tblmultipleworkorders.Where(m => m.IsDeleted == 0 && m.CellID == value && m.WCID == null).ToList();
                if (MultiWoCellData.Count > 0)
                {
                    status = "yes";
                }
            }
            if (int.TryParse(ShopID, out value))
            {
                List<tblmultipleworkorder> MultiWoShopData = obj.GettblmultipleworkordersList3(value);
                //var MultiWoShopData = db.tblmultipleworkorders.Where(m => m.IsDeleted == 0 && m.ShopID == value && m.CellID == null && m.WCID == null).ToList();
                if (MultiWoShopData.Count > 0)
                {
                    status = "yes";
                }
            }
            if (int.TryParse(PlantID, out value))
            {
                List<tblmultipleworkorder> MultiWoPlantData = obj.GettblmultipleworkordersList4(value);
                //var MultiWoPlantData = db.tblmultipleworkorders.Where(m => m.IsDeleted == 0 && m.PlantID == value && m.ShopID == null && m.CellID == null && m.WCID == null).ToList();
                if (MultiWoPlantData.Count > 0)
                {
                    status = "yes";
                }
            }

            return Json(status, JsonRequestBehavior.AllowGet);
        }
        public string[] GetDateShift()
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            string[] dateShift = new string[2];
            //Get CorrectedDate & shift

            #region
            string Shift = null;
            MsqlConnection mcp = new MsqlConnection();
            mcp.open();
            String queryshift = "SELECT ShiftName,StartTime,EndTime FROM tblshift_mstr WHERE IsDeleted = 0";
            SqlDataAdapter dashift = new SqlDataAdapter(queryshift, mcp.msqlConnection);
            DataTable dtshift = new DataTable();
            dashift.Fill(dtshift);
            String[] msgtime = System.DateTime.Now.TimeOfDay.ToString().Split(':');
            TimeSpan msgstime = System.DateTime.Now.TimeOfDay;
            //TimeSpan msgstime = new TimeSpan(Convert.ToInt32(msgtime[0]), Convert.ToInt32(msgtime[1]), Convert.ToInt32(msgtime[2]));
            TimeSpan s1t1 = new TimeSpan(0, 0, 0), s1t2 = new TimeSpan(0, 0, 0), s2t1 = new TimeSpan(0, 0, 0), s2t2 = new TimeSpan(0, 0, 0);
            TimeSpan s3t1 = new TimeSpan(0, 0, 0), s3t2 = new TimeSpan(0, 0, 0), s3t3 = new TimeSpan(0, 0, 0), s3t4 = new TimeSpan(23, 59, 59);
            for (int k = 0; k < dtshift.Rows.Count; k++)
            {
                if (dtshift.Rows[k][0].ToString().Contains("A"))
                {
                    String[] s1 = dtshift.Rows[k][1].ToString().Split(':');
                    s1t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
                    String[] s11 = dtshift.Rows[k][2].ToString().Split(':');
                    s1t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
                }
                else if (dtshift.Rows[k][0].ToString().Contains("B"))
                {
                    String[] s1 = dtshift.Rows[k][1].ToString().Split(':');
                    s2t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
                    String[] s11 = dtshift.Rows[k][2].ToString().Split(':');
                    s2t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
                }
                else if (dtshift.Rows[k][0].ToString().Contains("C"))
                {
                    String[] s1 = dtshift.Rows[k][1].ToString().Split(':');
                    s3t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
                    String[] s11 = dtshift.Rows[k][2].ToString().Split(':');
                    s3t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
                }
            }
            String CorrectedDate = System.DateTime.Now.ToString("yyyy-MM-dd");
            if (msgstime >= s1t1 && msgstime < s1t2)
            {
                Shift = "A";
            }
            else if (msgstime >= s2t1 && msgstime < s2t2)
            {
                Shift = "B";
            }
            else if ((msgstime >= s3t1 && msgstime <= s3t4) || (msgstime >= s3t3 && msgstime < s3t2))
            {
                Shift = "C";
                if (msgstime >= s3t3 && msgstime < s3t2)
                {
                    CorrectedDate = System.DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                }
            }
            mcp.close();

            #endregion

            dateShift[0] = Shift;
            dateShift[1] = CorrectedDate;
            return dateShift;
        }

        public ActionResult MultiWOQtyEntry(int id)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            int macID = Convert.ToInt32(Session["MachineID"]);
            string machinedispname = obj.GetMachineDet1(macID);
            //var machinedispname = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == macID).Select(m => m.MachineDispName).FirstOrDefault();
            ViewBag.macDispName = Convert.ToString(machinedispname);
            List<tbllivemultiwoselection> MultiWOList = obj.GetmultiplewoselectionList1(id);
            // var MultiWOList = db.tbllivemultiwoselections.Where(m => m.HMIID == id).ToList();
            return View(MultiWOList);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MultiWOQtyEntry(List<tbllivemultiwoselection> MultiWO)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            int deliveredQtySummationValue = 0;
            int targetQtySummationValue = 0;
            int hmiID = 0;
            if (MultiWO != null)
            {
                foreach (tbllivemultiwoselection row in MultiWO)
                {
                    hmiID = Convert.ToInt32(row.HMIID);
                    int DelQty = (int)row.DeliveredQty;
                    deliveredQtySummationValue += DelQty;
                    int TarQty = (int)row.TargetQty;
                    targetQtySummationValue += TarQty;
                    tbllivemultiwoselection IndividualMultiWOIDData = obj.GettbllivemultiwoselectionDet(row.MultiWOID);
                    //var IndividualMultiWOIDData = dbMWO.tbllivemultiwoselections.Find(row.MultiWOID);
                    obj.UpdateMultiwoselectionDetails1(IndividualMultiWOIDData.MultiWOID, DelQty, row.SplitWO);
                    //IndividualMultiWOIDData.IsCompleted = 0;
                    //IndividualMultiWOIDData.DeliveredQty = DelQty;

                    //var splitWO = row.SplitWO;
                    //dbMWO.Entry(IndividualMultiWOIDData).State = System.Data.Entity.EntityState.Modified;
                    //dbMWO.SaveChanges();
                }
            }
            tbllivehmiscreen thmidata = obj.GetLiveHMIDet(hmiID);
            //tbllivehmiscreen thmidata = db.tbllivehmiscreens.Where(m => m.HMIID == hmiID).FirstOrDefault();
            if (thmidata != null)
            {
                obj.UpdateHMIDetails1(deliveredQtySummationValue, hmiID);
                //thmidata.Delivered_Qty = deliveredQtySummationValue;
                //db.Entry(thmidata).State = System.Data.Entity.EntityState.Modified;
                //db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public JsonResult AutoSaveMultiWOSplitWO(int multiwoID, string SplitWO)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            bool retStatus = false;
            int HMIID = 0;
            tbllivemultiwoselection thisrow = obj.GettbllivemultiwoselectionDet(multiwoID);
            // var thisrow = db.tbllivemultiwoselections.Where(m => m.MultiWOID == multiwoID).FirstOrDefault();
            if (thisrow != null)
            {
                if (SplitWO.Equals("Yes") || SplitWO.Equals("No"))
                {
                    //Update Spliwo in tbllivemultiwoselection table
                    obj.UpdateMultiwoselectionDetails(SplitWO, multiwoID);
                    //thisrow.SplitWO = SplitWO;
                    //db.Entry(thisrow).State = System.Data.Entity.EntityState.Modified;
                    //db.SaveChanges();
                }
                HMIID = (int)thisrow.HMIID;
            }
            if (HMIID != 0)
            {
                List<tbllivemultiwoselection> AllRows = obj.GetmultiplewoselectionList(multiwoID);
                //var AllRows = db.tbllivemultiwoselections.Where(m => m.MultiWOID == multiwoID).ToList();
                string tblhmiscreenRowSplitStatus = "No";
                foreach (tbllivemultiwoselection row in AllRows)
                {
                    if (row.SplitWO == "Yes")
                    {
                        tblhmiscreenRowSplitStatus = "Yes";
                        break;
                    }
                }
                if (AllRows.Count > 0)
                {
                    tbllivehmiscreen tblhmiscreenRow = obj.GetLiveHMIDet(HMIID);
                    //var tblhmiscreenRow = db.tbllivehmiscreens.Find(HMIID);
                    if (tblhmiscreenRow != null)
                    {

                        obj.UpdateLiveHMIDetails(tblhmiscreenRowSplitStatus, HMIID);
                        //tblhmiscreenRow.SplitWO = tblhmiscreenRowSplitStatus;
                        //db.Entry(tblhmiscreenRow).State = System.Data.Entity.EntityState.Modified;
                        //db.SaveChanges();
                    }
                }
            }
            return Json(retStatus, JsonRequestBehavior.AllowGet);
        }

        public JsonResult JsonBreakdownChecker()
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            string retStatus = null;
            int machineID = Convert.ToInt32(Session["MachineID"]);

            string correcteddate = null;
            tbldaytiming StartTime1 = obj.GetDaytimingDetails();
            //tbldaytiming StartTime1 = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
            TimeSpan Start = StartTime1.StartTime;
            if (Start <= DateTime.Now.TimeOfDay)
            {
                correcteddate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                correcteddate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }
            tblbreakdown CurrentStatusData = obj.GetBreakdownDet(machineID);
            // var CurrentStatusData = db.tblbreakdowns.Where(m => m.MachineID == machineID && m.DoneWithRow == 0).OrderByDescending(m => m.StartTime).FirstOrDefault();
            if (CurrentStatusData != null)
            {
                int value = 1;
                string doneWithRowString = Convert.ToString(CurrentStatusData.DoneWithRow);

                Int32.TryParse(doneWithRowString, out value);
                if (CurrentStatusData.MessageCode == "PM")
                {
                    if (value == 0)
                    {
                        retStatus = "PM";
                    }
                }
                else
                {
                    if (value == 0)
                    {
                        retStatus = "BREAKDOWN";
                    }
                }
            }
            return Json(retStatus, JsonRequestBehavior.AllowGet);
        }

        public string GetBreakdownstatus()
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            string retStatus = "";
            int machineID = Convert.ToInt32(Session["MachineID"]);

            string correcteddate = null;
            tbldaytiming StartTime1 = obj.GetDaytimingDetails();
            //tbldaytiming StartTime1 = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
            TimeSpan Start = StartTime1.StartTime;
            if (Start <= DateTime.Now.TimeOfDay)
            {
                correcteddate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                correcteddate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }
            tblbreakdown CurrentStatusData = obj.GetBreakdownDet(machineID);
            // var CurrentStatusData = db.tblbreakdowns.Where(m => m.MachineID == machineID && m.DoneWithRow == 0).OrderByDescending(m => m.StartTime).FirstOrDefault();
            if (CurrentStatusData != null)
            {
                int value = 1;
                string doneWithRowString = Convert.ToString(CurrentStatusData.DoneWithRow);

                Int32.TryParse(doneWithRowString, out value);
                if (CurrentStatusData.MessageCode == "PM")
                {
                    if (value == 0)
                    {
                        retStatus = "PM";
                    }
                }
                else
                {
                    if (value == 0)
                    {
                        retStatus = "BREAKDOWN";
                    }
                }
            }
            return retStatus;
        }

        public JsonResult JsonCheckerRemoveWO(string values)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);
            string retStatus = "false";
            if (values != null)
            {
                string[] ids = values.Split(',');

                //var hmidata = dbhmi.tbllivehmiscreens.Where(m => m.HMIID == Hmiid).FirstOrDefault();
                foreach (string item in ids)
                {
                    int item1 = Convert.ToInt32(item);
                    tbllivehmiscreen hmidata = obj.GetLiveHMIDetails6(item1);
                    if (hmidata != null)
                    {
                        if (string.IsNullOrEmpty(Convert.ToString(hmidata.Date)))
                        {
                            retStatus = "true";
                            obj.DeleteHMIScreenDetails(hmidata.HMIID);
                            //dbhmi.tbllivehmiscreens.Remove(hmidata);
                            //dbhmi.SaveChanges();

                            if (hmidata.IsMultiWO == 1)
                            {
                                try
                                {
                                    obj.GetMWOtDetails(hmidata.HMIID);
                                    //dbhmi.tbllivemultiwoselections.Remove(dbhmi.tbllivemultiwoselections.Where(m => m.HMIID == hmidata.HMIID).FirstOrDefault());
                                    //dbhmi.SaveChanges();
                                }
                                catch (Exception)
                                {

                                }
                            }

                        }
                        else
                        {
                            retStatus = "You cannnot remove WorkOrder Once its Started.";
                        }
                    }
                }
            }

            return Json(retStatus, JsonRequestBehavior.AllowGet);
        }

        //PartialFinished WO List 2017-01-07 Janardhan
        public ActionResult PartialFinishedList(int DDLID = 0, string MacInvNo = null, int ToHMI = 0)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            string CorrectedDate = null;
            tbldaytiming StartTime = obj.GetDaytimingDetails();
            //tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
            TimeSpan Start = StartTime.StartTime;
            if (Start <= DateTime.Now.TimeOfDay)
            {
                CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }
            int machineId = Convert.ToInt32(Session["MachineID"]);
            ViewBag.opid = Session["opid"];
            ViewBag.mcnid = machineId;
            ViewBag.coretddt = CorrectedDate;

            //int handleidleReturnValue = HandleIdle();
            //if (handleidleReturnValue == 0)
            //{
            //    return RedirectToAction("DownCodeEntry");
            //}

            //Step 1: If DDLID is given then insert that data into HMIScreen table , take its HMIID and redirect to Index 
            if (DDLID != 0)
            {
                int Hmiid = 0;
                tblddl ddldata = obj.GetddlDetails(DDLID);
                // var ddldata = db.tblddls.Where(m => m.IsCompleted == 0 && m.DDLID == DDLID).FirstOrDefault();
                // String SplitWO = ddldata.SplitWO;
                String WONo = ddldata.WorkOrder;
                String Part = ddldata.MaterialDesc;
                String Operation = ddldata.OperationNo;

                int PrvProcessQty = 0, PrvDeliveredQty = 0;
                List<tbllivehmiscreen> getProcessQty = obj.GetLiveHMIDetails1(WONo, Part, Operation);
                //var getProcessQty = db.tbllivehmiscreens.Where(m => m.Work_Order_No == WONo && m.PartNo == Part && m.OperationNo == Operation && m.isWorkInProgress != 2).OrderByDescending(m => m.HMIID).Take(1).ToList();
                if (getProcessQty.Count > 0)
                {
                    PrvProcessQty = Convert.ToInt32(getProcessQty[0].ProcessQty);
                    PrvDeliveredQty = Convert.ToInt32(getProcessQty[0].Delivered_Qty);
                }
                int TotalProcessQty = Convert.ToInt32(PrvProcessQty + PrvDeliveredQty);
                tbllivehmiscreen hmidata = obj.GettbllivehmiscreensDet1(machineId);
                //var hmidata = db.tbllivehmiscreens.Where(m => m.MachineID == machineId && m.isWorkInProgress == 2).OrderByDescending(m => m.HMIID).FirstOrDefault();
                //hmidata.Date = DateTime.Now;

                int Hmiid1 = hmidata.HMIID;
                //delete if any IsSubmit = 0 for this hmiid.
                obj.deleteMultiWOSlectionDetails2(Hmiid1);
                //db.tbllivemultiwoselections.RemoveRange(db.tbllivemultiwoselections.Where(x => x.HMIID == Hmiid1 && x.IsSubmit == 0));
                //db.SaveChanges();
                int Target_Qty = Convert.ToInt32(ddldata.TargetQty);
                obj.UpdateLiveHMIScreenDetails6(ddldata.OperationNo, ddldata.MaterialDesc, ddldata.Project, Target_Qty, ddldata.WorkOrder, TotalProcessQty, ddldata.WorkCenter, Hmiid);
                //hmidata.OperationNo = ddldata.OperationNo;
                //hmidata.PartNo = ddldata.MaterialDesc;
                //hmidata.Project = ddldata.Project;

                //hmidata.Work_Order_No = ddldata.WorkOrder;
                //hmidata.ProcessQty = TotalProcessQty;
                //hmidata.DDLWokrCentre = ddldata.WorkCenter;
                Hmiid = hmidata.HMIID;
                //hmidata.IsMultiWO = 0;
                //db.Entry(hmidata).State = System.Data.Entity.EntityState.Modified;
                //db.SaveChanges();
                Session["FromDDL"] = 1;
                Session["SubmitClicked"] = 0;
                return RedirectToAction("Index", Hmiid);
            }

            //Step2: If DDLID == 0 and ToHMI == 1 then go to HMIScreen "Index" With Normal HMI Flow
            // This means Operator opted for Manual Entry
            if (DDLID == 0 && ToHMI == 1)
            {
                tbllivehmiscreen hmidata = obj.GettbllivehmiscreensDet1(machineId);
                //var hmidata = db.tbllivehmiscreens.Where(m => m.MachineID == machineId && m.isWorkInProgress == 2).OrderByDescending(m => m.HMIID).FirstOrDefault();

                int Hmiid = hmidata.HMIID;
                //delete if any IsSubmit = 0 for this hmiid.
                obj.deleteMultiWOSlectionDetails2(Hmiid);
                //db.tbllivemultiwoselections.RemoveRange(db.tbllivemultiwoselections.Where(x => x.HMIID == Hmiid && x.IsSubmit == 0));
                //db.SaveChanges();
                obj.UpdateLiveHMI4(Hmiid);
                //hmidata.OperationNo = null;
                //hmidata.PartNo = null;
                //hmidata.Project = null;
                //hmidata.Target_Qty = null;
                //hmidata.Work_Order_No = null;
                //hmidata.ProcessQty = 0;
                //hmidata.DDLWokrCentre = null;
                //hmidata.IsMultiWO = 0;
                //db.Entry(hmidata).State = System.Data.Entity.EntityState.Modified;
                //db.SaveChanges();
                Session["FromDDL"] = 2;
                return RedirectToAction("Index");
            }

            //Step 3: If DDLID == 0, then go to DDLList page.

            int MacId = Convert.ToInt32(Session["MachineID"]);
            ViewBag.machineData = obj.GetMachineList2(MacId);
            // ViewBag.machineData = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MacId).Select(m => m.MachineInvNo).ToList();
            tblmachinedetail oneMacData = obj.GetOneMachineDet(MacId);
            //var oneMacData = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MacId).FirstOrDefault();
            string cellidstring = Convert.ToString(oneMacData.CellID);
            string shopidstring = Convert.ToString(oneMacData.ShopID);
            int shopid;
            int.TryParse(shopidstring, out shopid);
            int cellid;
            if (int.TryParse(cellidstring, out cellid) && int.TryParse(shopidstring, out shopid))
            {
                List<tblmachinedetail> macList = new List<tblmachinedetail>();
                macList.AddRange(obj.GetCellMachineList2(cellid));
                //macList.AddRange(db.tblmachinedetails.Where(m => m.CellID == cellid && m.IsDeleted == 0).Select(m => m.MachineInvNo).ToList());
                macList.AddRange(obj.GetShopMachineList2(cellid, shopid));
                //macList.AddRange(db.tblmachinedetails.Where(m => m.ShopID == shopid && m.CellID != cellid && m.IsDeleted == 0).Select(m => m.MachineInvNo).ToList());

                //ViewBag.machineData = db.tblmachinedetails.Where(m => m.CellID == cellid && m.IsDeleted == 0).Select(m => m.MachineInvNo).ToList();
                //ViewBag.machineData += db.tblmachinedetails.Where(m => m.ShopID == shopid &&  m.CellID != cellid  && m.IsDeleted == 0).Select(m => m.MachineInvNo).ToList();
                ViewBag.machineData = macList;
            }
            else
            {
                if (int.TryParse(shopidstring, out shopid))
                {
                    //ViewBag.machineData = db.tblmachinedetails.Where(m => m.ShopID == shopid && m.IsDeleted == 0).Select(m => m.MachineInvNo).ToList();
                    ViewBag.machineData = obj.GetShopList4(shopid);
                    //ViewBag.machineData = from row in db.tblmachinedetails
                    //                      where row.ShopID == shopid && row.IsDeleted == 0 && row.CellID.Equals(null)
                    //                      select row.MachineInvNo;
                }
                else
                {
                    string plantidstring = Convert.ToString(oneMacData.PlantID);
                    int plantid;
                    if (int.TryParse(plantidstring, out plantid))
                    {
                        //ViewBag.machineData = db.tblmachinedetails.Where(m => m.PlantID == plantid && m.IsDeleted == 0).Select(m => m.MachineInvNo).ToList();
                        ViewBag.machineData = obj.GetPlantList4(plantid);
                        //ViewBag.machineData = from row in db.tblmachinedetails
                        //                      where row.PlantID == plantid && row.IsDeleted == 0 && row.ShopID.Equals(null) && row.CellID.Equals(null)
                        //                      select row.MachineInvNo;
                    }
                }
            }
            string machineInvNo = null;
            if (MacInvNo == null)
            {
                tblmachinedetail machinedata = obj.GetmacDetails(machineId);
                // var machinedata = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == machineId).FirstOrDefault();
                Session["macDispName"] = Convert.ToString(machinedata.MachineDisplayName);
                machineInvNo = machinedata.MachineName;
            }
            else
            {
                machineInvNo = MacInvNo;
            }

            ////ViewBag.MacInvNo = machineInvNo;
            //String Query = "select * " +
            //                "from tblddl WHERE WorkCenter = '" + machineInvNo + "' AND IsCompleted = 0 " +
            //                "order by DaysAgeing = '', Convert(DaysAgeing , SIGNED INTEGER) asc ,FlagRushInd = '',FlagRushInd = 0 ,Convert(FlagRushInd , SIGNED INTEGER) asc  , MADDateInd = '' , MADDateInd asc , MADDate asc";
            ////"order by DaysAgeing = \"\", DaysAgeing asc ,FlagRushInd = \"\",FlagRushInd = 0 ,FlagRushInd asc  , MADDateInd = \"\" , MADDateInd asc , MADDate asc";
            //var data = db.tblddls.SqlQuery(Query).ToList();

            ViewBag.MacInvNo = machineInvNo;
            //New Logic 2017-01-07
            List<tblddl> ddlDataList = new List<tblddl>();
            int macID = Convert.ToInt32(Session["MachineID"]);
            //string WIPQuery = @"SELECT * from tbllivehmiscreen where isWorkInProgress = 0 and HMIID IN ( SELECT HMIID from tbllivehmiscreen where MachineID = " + macID + " order by HMIID desc ) ";
            //var WIP = db.tbllivehmiscreens.SqlQuery(WIPQuery).ToList();
            List<tbllivehmiscreen> WIP = obj.GetHMIList3(macID);
            foreach (tbllivehmiscreen row in WIP)
            {
                string wono = row.Work_Order_No;
                string partno = row.PartNo;
                string opno = row.OperationNo;
                tblddl ddldata = obj.GetDDLDet1(wono, partno, opno);
                //var ddldata = db.tblddls.Where(m => m.WorkOrder == wono && m.MaterialDesc == partno && m.OperationNo == opno && m.IsCompleted == 0).FirstOrDefault();
                if (ddldata != null)
                {
                    ddlDataList.Add(ddldata);
                }
            }
            if (ddlDataList.Count != 0)
            {
                return View(ddlDataList.ToList());
            }
            else
            {
                return View();
            }
        }
        [HttpPost]
        public ActionResult PartialFinishedList(List<int> data)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            int WOCount = data.Count;
            int machineId = Convert.ToInt32(Session["MachineID"]);
            if (WOCount == 1)
            {
                int DDLID = data.First();

                int Hmiid = 0;
                tblddl ddldata = obj.GetddlDetails(DDLID);
                // var ddldata = db.tblddls.Where(m => m.IsCompleted == 0 && m.DDLID == DDLID).FirstOrDefault();
                String WONo = ddldata.WorkOrder;
                String Part = ddldata.MaterialDesc;
                String Operation = ddldata.OperationNo;

                int PrvProcessQty = 0, PrvDeliveredQty = 0, TotalProcessQty = 0;
                List<tbllivehmiscreen> getProcessQty = obj.GethmiList(WONo, Part, Operation);
                //var getProcessQty = db.tbllivehmiscreens.Where(m => m.Work_Order_No == WONo && m.PartNo == Part && m.OperationNo == Operation && m.isWorkInProgress != 2).OrderByDescending(m => m.HMIID).Take(1).ToList();
                if (getProcessQty.Count > 0)
                {
                    string delivString = Convert.ToString(getProcessQty[0].Delivered_Qty);
                    int.TryParse(delivString, out PrvDeliveredQty);

                    string processString = Convert.ToString(getProcessQty[0].ProcessQty);
                    int.TryParse(processString, out PrvProcessQty);

                    TotalProcessQty = Convert.ToInt32(PrvProcessQty + PrvDeliveredQty);
                }
                tbllivehmiscreen hmidata = obj.GetLiveHMIScreenDet(machineId);
                //var hmidata = db.tbllivehmiscreens.Where(m => m.MachineID == machineId && m.isWorkInProgress == 2).OrderByDescending(m => m.HMIID).FirstOrDefault();
                Hmiid = hmidata.HMIID;

                //delete if any IsSubmit = 0 for this hmiid.
                obj.deleteMultiWOSlectionDetails2(Hmiid);
                //db.tbllivemultiwoselections.RemoveRange(db.tbllivemultiwoselections.Where(x => x.HMIID == Hmiid && x.IsSubmit == 0));
                //db.SaveChanges();
                int Target_Qty = Convert.ToInt32(ddldata.TargetQty);
                obj.UpdateLiveHMIScreenDetails3(Hmiid, ddldata.OperationNo, ddldata.MaterialDesc, ddldata.Project, Target_Qty, ddldata.WorkOrder);
                //hmidata.OperationNo = ddldata.OperationNo;
                //hmidata.PartNo = ddldata.MaterialDesc;
                //hmidata.Project = ddldata.Project;

                //hmidata.Work_Order_No = ddldata.WorkOrder;
                //hmidata.ProcessQty = TotalProcessQty;
                //hmidata.IsMultiWO = 0;
                //Hmiid = hmidata.HMIID;
                //db.Entry(hmidata).State = System.Data.Entity.EntityState.Modified;
                //db.SaveChanges();

                Session["FromDDL"] = 1;
                return RedirectToAction("Index", Hmiid);
            }
            else if (WOCount > 1)
            {
                int TotalTargetQty = 0;
                int TotalProcessQty = 0;
                tbllivehmiscreen hmidata = obj.GetLiveHMIScreenDet(machineId);
                //var hmidata = db.tbllivehmiscreens.Where(m => m.MachineID == machineId && m.isWorkInProgress == 2).OrderByDescending(m => m.HMIID).FirstOrDefault();
                int Hmiid = hmidata.HMIID;
                int i = 0;
                String MainOpearationNo = null;
                String MainWorkOrder = null;
                String MainPartNo = null;
                String MainProject = null;

                //delete if any IsSubmit = 0 for this hmiid.
                obj.deleteMultiWOSlectionDetails2(Hmiid);
                //db.tbllivemultiwoselections.RemoveRange(db.tbllivemultiwoselections.Where(x => x.HMIID == Hmiid && x.IsSubmit == 0));
                //db.SaveChanges();

                string ddlWorkCenter = null;
                foreach (int DDLID in data)
                {
                    int PrvProcessQty = 0, PrvDeliveredQty = 0;
                    tblddl ddldata = obj.GetddlDetails(DDLID);
                    //var ddldata = db.tblddls.Where(m => m.IsCompleted == 0 && m.DDLID == DDLID).FirstOrDefault();
                    String WONo = ddldata.WorkOrder;
                    String Part = ddldata.MaterialDesc;
                    String Operation = ddldata.OperationNo;
                    ddlWorkCenter = ddldata.WorkCenter;
                    int TargetQty = Convert.ToInt32(ddldata.TargetQty);
                    TotalTargetQty += TargetQty;
                    if (i == 0)
                    {
                        MainOpearationNo = Operation;
                        MainWorkOrder = WONo;
                        MainPartNo = Part;
                        MainProject = ddldata.Project;
                    }

                    ////var getMultiWoPorcessQty = db.tbl_multiwoselection.Where(m => m.WorkOrder == WONo && m.PartNo == Part && m.OperationNo == Operation).OrderByDescending(m => m.MultiWOID).FirstOrDefault();
                    ////int ProcessQty = Convert.ToInt32(getMultiWoPorcessQty.ProcessQty + getMultiWoPorcessQty.DeliveredQty);

                    #region OLD
                    //int ProcessQty = 0;
                    //var getMultiWoPorcessQty = db.tbl_multiwoselection.Where(m => m.WorkOrder == WONo && m.PartNo == Part && m.OperationNo == Operation).OrderByDescending(m => m.MultiWOID).Take(1).ToList();
                    //if (getMultiWoPorcessQty.Count > 0)
                    //{
                    //    string delivString = Convert.ToString(getMultiWoPorcessQty[0].DeliveredQty);
                    //    int delivInt = 0;
                    //    int.TryParse(delivString, out delivInt);

                    //    string processString = Convert.ToString(getMultiWoPorcessQty[0].ProcessQty);
                    //    int procInt = 0;
                    //    int.TryParse(processString, out procInt);
                    //    ProcessQty = Convert.ToInt32(procInt + delivInt);
                    //}
                    #endregion

                    #region new code

                    //here 1st get latest of delivered and processed among row in tblHMIScreen & tblmulitwoselection
                    int isHMIFirst = 2; //default NO History for that wo,pn,on
                    List<tbllivemultiwoselection> mulitwoData = obj.GetMultiWOSelectionList(WONo, Part, Operation);
                    // var mulitwoData = db.tbllivemultiwoselections.Where(m => m.WorkOrder == WONo && m.PartNo == Part && m.OperationNo == Operation && m.tbllivehmiscreen.isWorkInProgress != 2).OrderByDescending(m => m.tbllivehmiscreen.Time).Take(1).ToList();
                    List<tbllivehmiscreen> hmiData = obj.GetHMIList(WONo, Part, Operation);
                    //var hmiData = db.tbllivehmiscreens.Where(m => m.Work_Order_No == WONo && m.PartNo == Part && m.OperationNo == Operation && m.isWorkInProgress != 2).OrderByDescending(m => m.Time).Take(1).ToList();

                    if (hmiData.Count > 0 && mulitwoData.Count > 0) // now check for greatest amongst
                    {
                        //DateTime multiwoDateTime = Convert.ToDateTime(mulitwoData[0].CreatedOn); //2017-06-02
                        //Based on hmiid of  multiwotable get  Time Column of tblhmiscreen 
                        //int localhmiid = Convert.ToInt32(mulitwoData[0].HMIID);
                        //var hmiiData = db.tblhmiscreens.Find(localhmiid);
                        DateTime multiwoDateTime = Convert.ToDateTime(mulitwoData[0].tbllivehmiscreen.Time);
                        DateTime hmiDateTime = Convert.ToDateTime(hmiData[0].Time);

                        if (Convert.ToInt32(multiwoDateTime.Subtract(hmiDateTime).TotalSeconds) > 0)
                        {
                            isHMIFirst = 1;
                        }
                        else
                        {
                            isHMIFirst = 0;
                        }

                    }
                    else if (mulitwoData.Count > 0)
                    {
                        isHMIFirst = 1;
                    }
                    else if (hmiData.Count > 0)
                    {
                        isHMIFirst = 0;
                    }

                    if (isHMIFirst == 1)
                    {
                        string delivString = Convert.ToString(mulitwoData[0].DeliveredQty);
                        int delivInt = 0;
                        int.TryParse(delivString, out delivInt);

                        string processString = Convert.ToString(mulitwoData[0].ProcessQty);
                        int procInt = 0;
                        int.TryParse(processString, out procInt);

                        PrvProcessQty += procInt;
                        PrvDeliveredQty += delivInt;
                    }
                    else if (isHMIFirst == 0)
                    {
                        string delivString = Convert.ToString(hmiData[0].Delivered_Qty);
                        int delivInt = 0;
                        int.TryParse(delivString, out delivInt);

                        string processString = Convert.ToString(hmiData[0].ProcessQty);
                        int procInt = 0;
                        int.TryParse(processString, out procInt);

                        PrvProcessQty += procInt;
                        PrvDeliveredQty += delivInt;
                    }
                    else
                    {
                        //no previous delivered or processed qty so Do Nothing.
                    }

                    #endregion

                    int ProcessQty = PrvProcessQty + PrvDeliveredQty;

                    TotalProcessQty += ProcessQty;
                    //Insert into MultiWoSelection Table
                    DateTime date = System.DateTime.Now;
                    obj.InsertmultiwoselectionDetails(ddldata.WorkCenter, Operation, Part, TargetQty, WONo, ProcessQty, Hmiid, date);

                    // tbllivemultiwoselection MultiWORow = new tbllivemultiwoselection();
                    //MultiWORow.DDLWorkCentre = ddldata.WorkCenter;
                    //MultiWORow.OperationNo = Operation;
                    //MultiWORow.PartNo = Part;
                    //MultiWORow.SplitWO = "0";
                    //MultiWORow.TargetQty = TargetQty;
                    //MultiWORow.WorkOrder = WONo;
                    // MultiWORow.ProcessQty = ProcessQty;
                    //MultiWORow.HMIID = Hmiid;
                    //MultiWORow.IsCompleted = 0;
                    //MultiWORow.CreatedOn = System.DateTime.Now;
                    //db.tbllivemultiwoselections.Add(MultiWORow);
                    //db.SaveChanges();
                }

                //update data into tbllivehmiscreen table
                string opt = MainOpearationNo + " - " + WOCount;
                string part = MainPartNo + " - " + WOCount;
                obj.UpdateLiveHMIScreenDetails4(opt, part, MainProject, TotalTargetQty, MainWorkOrder, TotalProcessQty, hmidata.HMIID, ddlWorkCenter);
                //hmidata.OperationNo = MainOpearationNo + " - " + WOCount;
                //hmidata.PartNo = MainPartNo + " - " + WOCount;
                //hmidata.Project = MainProject;
                //hmidata.Target_Qty = TotalTargetQty;
                //hmidata.Work_Order_No = MainWorkOrder;
                //hmidata.ProcessQty = TotalProcessQty;
                //Hmiid = hmidata.HMIID;
                //hmidata.DDLWokrCentre = ddlWorkCenter;
                //hmidata.IsMultiWO = 1;
                //db.Entry(hmidata).State = System.Data.Entity.EntityState.Modified;
                //db.SaveChanges();
                Session["FromDDL"] = 4;
                return RedirectToAction("Index", Hmiid);
            }
            return RedirectToAction("Index");
        }

        public bool ContainsChar(string s)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            bool retVal = false;
            char[] a = s.ToCharArray();
            foreach (char alpha in a)
            {
                if (Convert.ToInt32(alpha) >= 48 && Convert.ToInt32(alpha) <= 57)
                {
                }
                else
                {
                    retVal = true;
                    break;
                }
            }

            //this also works
            bool result = !s.Any(x => char.IsLetter(x));

            return retVal;
        }

        private string GetOrderedHMIIDs(string hmiids)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            string retHMIIDs = null;
            if (hmiids != null)
            {
                List<tbllivehmiscreen> WIPOuter = obj.GetHMIList5(hmiids);
                //string WIPQueryOuter = @"SELECT * from tbllivehmiscreen where HMIID IN ( " + hmiids + " ) order by Work_Order_No,PartNo,OperationNo ; ";
                //var WIPOuter = db.tbllivehmiscreens.SqlQuery(WIPQueryOuter).ToList();

                for (int id = 0; id < WIPOuter.Count; id++)
                {
                    if (retHMIIDs == null)
                    {
                        retHMIIDs = Convert.ToString(WIPOuter[id].HMIID);
                    }
                    else
                    {
                        retHMIIDs += "," + Convert.ToString(WIPOuter[id].HMIID);
                    }
                }
            }

            return retHMIIDs;
        }

        public JsonResult AutoSaveSplitWO(int HMIID, string SplitWO)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            //Session["Split"] = 1;
            string retStatus = "";
            tbllivehmiscreen thisrow = obj.GetLiveHMIDetails6(HMIID);
            //var thisrow = db.tbllivehmiscreens.Where(m => m.HMIID == HMIID).FirstOrDefault();

            if (thisrow != null)
            {
                if (thisrow.Delivered_Qty == 0)
                {
                    retStatus = "failure";
                }
                else if (!string.IsNullOrEmpty(thisrow.PartNo) && !string.IsNullOrEmpty(thisrow.Work_Order_No))
                {
                    if (SplitWO.Equals("Yes") || SplitWO.Equals("No"))
                    {
                        retStatus = "Success";
                        string SplitWO1 = SplitWO;
                        obj.UpdateLiveHMIDetails(SplitWO, HMIID);
                        //db.Entry(thisrow).State = System.Data.Entity.EntityState.Modified;
                        //db.SaveChanges();
                    }
                }
            }
            return Json(retStatus, JsonRequestBehavior.AllowGet);
        }

        //public JsonResult PopulateWODetails(int hmiid, string WOData)
        //{
        //    string retStatus = "false";

        //    int MachineID = Convert.ToInt32(Session["MachineID"]);
        //    var hmiData = db.tblhmiscreens.Where(m => m.HMIID == hmiid).FirstOrDefault();
        //    string OpNo = null, WONo = null, SorM = null;

        //    if (ContainsChar(WOData))
        //    {
        //        OpNo = WOData.Substring(WOData.Length - 5 + 2, 2); //x-length of WONo , 4-length of OpNo (2-Zeros in Opno) , 1-length of SorM.
        //        int opNoInt = Convert.ToInt32(OpNo);
        //        OpNo = Convert.ToString(opNoInt);
        //        WONo = WOData.Substring(0, WOData.Length - 5); //4-length of opno and 1-length of SorM
        //        Int64 WONoInt = Convert.ToInt64(WONo);
        //        WONo = Convert.ToString(WONoInt);
        //    }
        //    else
        //    {
        //        OpNo = WOData.Substring(WOData.Length - 4 + 2, 2); //x-length of WONo , 4-length of OpNo (2-Zeros in Opno)
        //        int opNoInt = Convert.ToInt32(OpNo);
        //        OpNo = Convert.ToString(opNoInt);
        //        WONo = WOData.Substring(0, WOData.Length - 4); //4-length of opno 
        //        Int64 WONoInt = Convert.ToInt64(WONo);
        //        WONo = Convert.ToString(WONoInt);
        //    }

        //    if(OpNo != null && WONo != null)
        //    {
        //        var ddlData = db.tblddls.Where(m => m.WorkOrder == WONo && m.OperationNo == OpNo && m.IsCompleted == 0).FirstOrDefault();
        //        if (ddlData != null)
        //        {
        //            hmiData.DDLWokrCentre = ddlData.WorkCenter;
        //            hmiData.DoneWithRow = 0;
        //            hmiData.IsHold = 0;
        //            hmiData.IsMultiWO = 0;
        //            hmiData.isUpdate = 0;
        //            hmiData.isWorkInProgress = 2;
        //            hmiData.isWorkOrder = 0;
        //            hmiData.Target_Qty = Convert.ToInt32(ddlData.TargetQty);
        //            hmiData.Prod_FAI = ddlData.Type;
        //            hmiData.MachineID = MachineID;
        //            hmiData.OperationNo = OpNo;
        //            hmiData.PartNo = ddlData.MaterialDesc;
        //            hmiData.ProcessQty = 0;
        //            hmiData.Prod_FAI = ddlData.Type;
        //            hmiData.Project = ddlData.Project;
        //            hmiData.Status = 0;
        //            hmiData.Work_Order_No = WONo;
        //            hmiData.PEStartTime = DateTime.Now;

        //            db.Entry(hmiData).State = System.Data.Entity.EntityState.Modified;
        //            db.SaveChanges();

        //            retStatus = "true";
        //        }
        //        else
        //        {
        //            hmiData.PartNo = null;
        //            hmiData.OperationNo = OpNo;
        //            hmiData.Work_Order_No = WONo;
        //            db.Entry(hmiData).State = System.Data.Entity.EntityState.Modified;
        //            db.SaveChanges();

        //            retStatus = "true";
        //        }

        //    }
        //    return Json(retStatus,JsonRequestBehavior.AllowGet);
        //}

        public JsonResult PopulateWODetails(int hmiid, string WOData)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            string retStatus = "false";

            int MachineID = Convert.ToInt32(Session["MachineID"]);
            tbllivehmiscreen hmiData = obj.GetHMIDetails(hmiid);
            string OpNo = null, WONo = null;

            bool isValidWOData = false;
            System.Text.RegularExpressions.Regex regexItem = new System.Text.RegularExpressions.Regex("^[a-zA-Z0-9]*$");
            if (regexItem.IsMatch(WOData))
            {
                isValidWOData = true;
            }

            if (WOData.Length > 6 && isValidWOData)
            {
                if (ContainsChar(WOData))
                {
                    OpNo = WOData.Substring(WOData.Length - 5, 4); //x-length of WONo , 4-length of OpNo (2-Zeros in Opno) , 1-length of SorM.
                    int opNoInt = Convert.ToInt32(OpNo);
                    OpNo = Convert.ToString(opNoInt);
                    WONo = WOData.Substring(0, WOData.Length - 5); //4-length of opno and 1-length of SorM
                    Int64 WONoInt = Convert.ToInt64(WONo);
                    WONo = Convert.ToString(WONoInt);
                }
                else
                {
                    OpNo = WOData.Substring(WOData.Length - 4, 4); //x-length of WONo , 4-length of OpNo (2-Zeros in Opno)
                    int opNoInt = Convert.ToInt32(OpNo);
                    OpNo = Convert.ToString(opNoInt);
                    WONo = WOData.Substring(0, WOData.Length - 4); //4-length of opno 
                    Int64 WONoInt = Convert.ToInt64(WONo);
                    WONo = Convert.ToString(WONoInt);
                }
            }
            if (OpNo != null && WONo != null)
            {
                //Check if it's in screen
                bool isDuplicate = false;
                tbllivehmiscreen hmiDataDuplicate = obj.GetliveHMIScreenDetails(WONo, OpNo, MachineID);
                if (hmiDataDuplicate != null)
                {
                    if (hmiDataDuplicate.isWorkInProgress == 2)
                    {
                        isDuplicate = true;
                        retStatus = "Duplicate WorkOrder. WONo.= " + WONo + " , OperationNo. = " + OpNo;
                    }
                }
                if (!isDuplicate)
                {
                    //Check if its in Hold in HMIScreen
                    int isHold = 0;
                    tbllivehmiscreen hmiDataDup = obj.GetliveHMIScreenDetails(WONo, OpNo);
                    if (hmiDataDup != null)
                    {
                        isHold = hmiDataDup.IsHold;
                    }
                    tblddl ddlData = obj.GetddlDetails(WONo, OpNo);
                    if (ddlData != null)
                    {
                        string ddlWorkCenter = Convert.ToString(ddlData.WorkCenter);
                        if (ddlWorkCenter != null)
                        {
                            tblmachinedetail ddlWorkCenterMacDetails = obj.GetmacDetails(ddlWorkCenter);
                            if (ddlWorkCenterMacDetails != null)
                            {
                                tblmachinedetail thisMacDetails = obj.GetmacDetails(MachineID);
                                int tragetqty = Convert.ToInt32(ddlData.TargetQty);
                                if (ddlWorkCenterMacDetails.ShopID == thisMacDetails.ShopID)
                                {
                                    obj.UpdateLiveHMIScreenDetails(ddlData.WorkCenter, hmiData.HMIID, isHold, tragetqty, ddlData.Type, MachineID, OpNo, ddlData.MaterialDesc, ddlData.Project, WONo);

                                    //hmiData.DDLWokrCentre = ddlData.WorkCenter;
                                    //hmiData.DoneWithRow = 0;
                                    //hmiData.IsHold = isHold;
                                    //hmiData.IsMultiWO = 0;
                                    //hmiData.isUpdate = 0;
                                    //hmiData.isWorkInProgress = 2;
                                    //hmiData.isWorkOrder = 0;
                                    //
                                    //hmiData.Prod_FAI = ddlData.Type;
                                    //hmiData.MachineID = MachineID;
                                    //hmiData.OperationNo = OpNo;
                                    //hmiData.PartNo = ddlData.MaterialDesc;
                                    //hmiData.ProcessQty = 0;
                                    //hmiData.Prod_FAI = ddlData.Type;
                                    //hmiData.Project = ddlData.Project;
                                    //hmiData.Status = 0;
                                    //hmiData.Work_Order_No = WONo;
                                    //hmiData.PEStartTime = DateTime.Now;

                                    //db.Entry(hmiData).State = System.Data.Entity.EntityState.Modified;
                                    //db.SaveChanges();

                                    retStatus = "true";
                                }
                                else
                                {
                                    retStatus = "This WorkOrder doesnot belong to this Shop";
                                }
                            }

                        }
                    }
                    else
                    {
                        tbllivehmiscreen thisHMIDetails = obj.GetliveHMIScreenDetails(WONo, OpNo);
                        //var thisHMIDetails = db.tbllivehmiscreens.Where(m => m.Work_Order_No == WONo && m.OperationNo == OpNo).OrderByDescending(m => m.PEStartTime).FirstOrDefault();
                        tblddl DDLFinished = obj.GetddlDetails1(WONo, OpNo);
                        //var DDLFinished = db.tblddls.Where(m => m.WorkOrder == WONo && m.OperationNo == OpNo && m.IsCompleted == 1).FirstOrDefault();
                        if (DDLFinished == null)
                        {
                            if (thisHMIDetails != null)
                            {
                                int Target_Qty = Convert.ToInt32(thisHMIDetails.Target_Qty);
                                obj.UpdateLiveHMIScreenDetails1(thisHMIDetails.DDLWokrCentre, hmiData.HMIID, isHold, Target_Qty, thisHMIDetails.Prod_FAI, MachineID, OpNo, thisHMIDetails.PartNo, thisHMIDetails.Project, WONo);
                                //hmiData.DDLWokrCentre = thisHMIDetails.DDLWokrCentre;
                                //hmiData.DoneWithRow = 0;
                                //hmiData.IsHold = isHold;
                                //hmiData.IsMultiWO = 0;
                                //hmiData.isUpdate = 0;
                                //hmiData.isWorkInProgress = 2;
                                //hmiData.isWorkOrder = 0;
                                //
                                //hmiData.Prod_FAI = thisHMIDetails.Prod_FAI;
                                //hmiData.MachineID = MachineID;
                                //hmiData.OperationNo = OpNo;
                                //hmiData.PartNo = thisHMIDetails.PartNo;
                                //hmiData.ProcessQty = 0;
                                //hmiData.Project = thisHMIDetails.Project;
                                //hmiData.Status = 0;
                                //hmiData.Work_Order_No = WONo;
                                ////hmiData.PEStartTime = DateTime.Now;

                                //db.Entry(hmiData).State = System.Data.Entity.EntityState.Modified;
                                //db.SaveChanges();

                                retStatus = "true";
                            }
                            else
                            {
                                obj.UpdateLiveHMIScreenDetails2(hmiData.HMIID, OpNo, WONo);
                                //hmiData.PartNo = null;
                                //hmiData.OperationNo = OpNo;
                                //hmiData.Work_Order_No = WONo;
                                //db.Entry(hmiData).State = System.Data.Entity.EntityState.Modified;
                                //db.SaveChanges();
                                retStatus = "false";
                            }
                        }
                        else
                        {
                            retStatus = "This WorkOrder is Finished. WoNo: " + WONo + " OpNo.: " + OpNo;
                        }
                    }
                }
            }
            else
            {
                retStatus = "WorkOrder is Not Appropriate.";
            }
            return Json(retStatus, JsonRequestBehavior.AllowGet);
        }

        private List<string> GetHierarchyData(int MachineID)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            List<string> HierarchyData = new List<string>();
            //1st get PlantName or -
            //2nd get ShopName or -
            //3rd get CellName or -
            //4th get MachineName.

            //using (i_facility_tsalEntities dbMac = new i_facility_tsalEntities())
            //{
            tblmachinedetail machineData = obj.GetMachineDetails(MachineID);
            int PlantID = Convert.ToInt32(machineData.PlantID);
            string name = "-";
            name = Convert.ToString(obj.GetPlantDetails(PlantID));
            HierarchyData.Add(name);
            int shopid = Convert.ToInt32(machineData.ShopID);
            //int ShopIDString = Convert.ToInt32(machineData.ShopID);
            //int value;
            //if (int.TryParse(ShopIDString, out value))
            //{
            name = Convert.ToString(obj.GetShopDetails(shopid));
            HierarchyData.Add(name + "-".ToString());
            //}
            //else
            //{
            //    HierarchyData.Add("-");
            //}
            int CellID = Convert.ToInt32(machineData.CellID);
            //string CellIDString = Convert.ToString(machineData.CellID);
            //if (int.TryParse(CellIDString, out value))
            //{
            name = Convert.ToString(obj.GetCellDetails(CellID));
            HierarchyData.Add(name + "-".ToString());
            // }

            HierarchyData.Add(Convert.ToString(machineData.MachineName));
            HierarchyData.Add(Convert.ToString(machineData.MachineDisplayName));
            //}
            return HierarchyData;
        }

        public JsonResult IsMultiWOAllowable1(int id)
        {
            string status = "no";
            int machineId = Convert.ToInt32(Session["MachineID"]);
            tblmachinedetail machineDATA = obj.GetMacDetails(id);
            // var machineDATA = db.tblmachinedetails.Where(m => m.MachineID == id).FirstOrDefault();
            string PlantID = Convert.ToString(machineDATA.PlantID);
            string ShopID = Convert.ToString(machineDATA.ShopID);
            string CellID = Convert.ToString(machineDATA.CellID);
            string WCID = Convert.ToString(machineDATA.MachineID);

            int value = 0;
            if (int.TryParse(WCID, out value))
            {
                List<tblmultipleworkorder> MultiWoWCData = obj.GettblmultipleworkordersList1(value);
                // var MultiWoWCData = db.tblmultipleworkorders.Where(m => m.IsDeleted == 0 && m.WCID == value).ToList();
                if (MultiWoWCData.Count > 0)
                {
                    status = "yes";
                }
            }
            if (int.TryParse(CellID, out value))
            {
                List<tblmultipleworkorder> MultiWoCellData = obj.GettblmultipleworkordersList2(value);
                // var MultiWoCellData = db.tblmultipleworkorders.Where(m => m.IsDeleted == 0 && m.CellID == value && m.WCID == null).ToList();
                if (MultiWoCellData.Count > 0)
                {
                    status = "yes";
                }
            }
            if (int.TryParse(ShopID, out value))
            {
                List<tblmultipleworkorder> MultiWoShopData = obj.GettblmultipleworkordersList3(value);
                // var MultiWoShopData = db.tblmultipleworkorders.Where(m => m.IsDeleted == 0 && m.ShopID == value && m.CellID == null && m.WCID == null).ToList();
                if (MultiWoShopData.Count > 0)
                {
                    status = "yes";
                }
            }
            if (int.TryParse(PlantID, out value))
            {
                List<tblmultipleworkorder> MultiWoPlantData = obj.GettblmultipleworkordersList4(value);
                //var MultiWoPlantData = db.tblmultipleworkorders.Where(m => m.IsDeleted == 0 && m.PlantID == value && m.ShopID == null && m.CellID == null && m.WCID == null).ToList();
                if (MultiWoPlantData.Count > 0)
                {
                    status = "yes";
                }
            }

            return Json(status, JsonRequestBehavior.AllowGet);
        }

        #region  commented by monika
        public ActionResult IndividualSubmit(int HMIID = 0)
        {
            Session["Mode"] = null;
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);

            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            //if (IDLEorGenericWorkisON())
            //{
            //    Session["Error"] = "Please End IDLE/GenericWork Before Selecting New Work Orders";
            //    return RedirectToAction("Index");
            //}

            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            int MachineId = Convert.ToInt32(Session["MachineID"]);

            //Check if similar WorkOrder is in View
            //tbllivehmiscreen hmiidDataDup = db.tbllivehmiscreens.Where(m => m.HMIID == HMIID).FirstOrDefault();
            tbllivehmiscreen hmiidDataDup = obj.GetLiveHMIDetails6(HMIID);
            if (hmiidDataDup != null)
            {
                int hmiid = Convert.ToInt32(hmiidDataDup.HMIID);
                string woNo = Convert.ToString(hmiidDataDup.Work_Order_No);
                string opNo = Convert.ToString(hmiidDataDup.OperationNo);
                string partNo = Convert.ToString(hmiidDataDup.PartNo);

                //cheack in DDL if isCompleted = 1
                //var DDLCompletedData = db.tblddls.Where(m => m.WorkOrder == woNo && m.MaterialDesc == partNo && m.OperationNo == opNo && m.IsCompleted == 1).FirstOrDefault();
                tblddl DDLCompletedData = obj.GettblddlDetails(woNo, partNo, opNo);
                if (DDLCompletedData != null)
                {
                    Session["Error"] = "Job is Finished for WorkOrder:" + woNo + " OpNo: " + opNo + " PartNo:" + partNo;

                    //db.tbllivehmiscreens.Remove(hmiidDataDup);
                    //db.SaveChanges();
                    obj.DeleteHMIScreenDetails(hmiidDataDup.HMIID);
                    return RedirectToAction("Index");
                }

                //2017-06-22
                //var HMICompletedData = db.tbllivehmiscreens.Where(m => m.Work_Order_No == woNo && m.PartNo == partNo && m.OperationNo == opNo && m.isWorkInProgress == 1).FirstOrDefault();
                tbllivehmiscreen HMICompletedData = obj.GetLiveHMIDetails2(woNo, partNo, opNo);
                if (HMICompletedData != null)
                {
                    Session["Error"] = "Job is Finished for WorkOrder:" + woNo + " OpNo: " + opNo + " PartNo:" + partNo;

                    //db.tbllivehmiscreens.Remove(hmiidDataDup);
                    //db.SaveChanges();
                    obj.DeleteHMIScreenDetails(hmiidDataDup.HMIID);
                    return RedirectToAction("Index");
                }

                #region 2017-02-07
                bool isValid = true, IsInHMI = true;
                string IssueMsg = null;
                if (isValid)
                {
                    //var Siblingddldata = db.tblddls.Where(m => m.IsCompleted == 0 && m.WorkOrder == woNo && m.MaterialDesc == partNo && m.OperationNo != opNo && m.IsCompleted == 0).OrderBy(m => new { m.WorkOrder, m.MaterialDesc, m.OperationNo }).ToList();
                    List<tblddl> Siblingddldata = obj.GetddlDetails(woNo, partNo, opNo);
                    foreach (tblddl row in Siblingddldata)
                    {
                        IsInHMI = true; //reinitialize
                        int localOPNo = Convert.ToInt32(row.OperationNo);
                        string localOPNoString = Convert.ToString(row.OperationNo);
                        if (localOPNo < Convert.ToInt32(opNo))
                        {
                            #region //Here Check in HMIScreen Table. There are chances that this one is started prior to this round of ddl selection ,
                            //which case is valid.
                            //var SiblingHMIdata = db.tbllivehmiscreens.Where(m => m.Work_Order_No == woNo && m.PartNo == partNo && m.OperationNo == localOPNoString).FirstOrDefault();
                            tbllivehmiscreen SiblingHMIdata = obj.GetLive1HMIDetails(woNo, partNo, Convert.ToString(localOPNo));
                            //var SiblingHMIdatahistorian = db.tblhmiscreens.Where(m => m.Work_Order_No == woNo && m.PartNo == partNo && m.OperationNo == localOPNoString).FirstOrDefault(); //added by Ashok
                            tblhmiscreen SiblingHMIdatahistorian = obj.GetHMIDetails1(woNo, partNo, Convert.ToString(localOPNo));
                            if (SiblingHMIdata == null || SiblingHMIdatahistorian == null)
                            {
                                // IssueMsg = "Please Select Below WorkOrder, WONo: " + woNo + " PartNo: " + partNo + " OperationNo: " + localOPNo;
                                //IsInHMI = false;
                                //break;
                            }
                            else
                            {
                                if (SiblingHMIdata.Date == null)//|| SiblingHMIdatahistorian.Date==null) //=> lower OpNo is not submitted.
                                {
                                    IssueMsg = " Start WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + localOPNoString;
                                    //return RedirectToAction("Index");
                                    IsInHMI = false;
                                    break;
                                }
                                if (SiblingHMIdatahistorian != null)
                                {
                                    if (SiblingHMIdatahistorian.Date == null)//|| SiblingHMIdatahistorian.Date==null) //=> lower OpNo is not submitted.
                                    {
                                        IssueMsg = " Start WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + localOPNoString;
                                        //return RedirectToAction("Index");
                                        IsInHMI = false;
                                        break;
                                    }
                                }
                            }
                            #endregion

                            if (!IsInHMI)
                            {
                                #region //also check in MultiWO table
                                //string WIPQueryMultiWO = @"SELECT * from tbllivemultiwoselection where WorkOrder = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + localOPNo + "' order by MultiWOID limit 1 ";
                                //var WIPMWO = db.tbllivemultiwoselections.SqlQuery(WIPQueryMultiWO).ToList();
                                List<tbllivemultiwoselection> WIPMWO = obj.GetMultiWOtDetails(woNo, partNo, Convert.ToString(localOPNo));
                                //string WIPQueryMultiWOHistorian = @"SELECT * from tbl_multiwoselection where WorkOrder = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + localOPNo + "' order by MultiWOID limit 1 ";
                                //var WIPMWOHistorian = db.tbl_multiwoselection.SqlQuery(WIPQueryMultiWOHistorian).ToList();
                                List<tbl_multiwoselection> WIPMWOHistorian = obj.GetMultiWorkSelectionDetails(woNo, partNo, Convert.ToString(localOPNo));
                                if (WIPMWO.Count == 0 || WIPMWOHistorian.Count == 0)
                                {
                                    IssueMsg = " Select  WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + localOPNoString;
                                    //return RedirectToAction("Index");
                                    IsInHMI = false;
                                    break;
                                }

                                foreach (tbllivemultiwoselection rowHMI in WIPMWO)
                                {
                                    int hmiidInner = Convert.ToInt32(rowHMI.HMIID);
                                    //var MWOHMIData = db.tbllivehmiscreens.Where(m => m.HMIID == hmiidInner).FirstOrDefault();
                                    tbllivehmiscreen MWOHMIData = obj.GetLiveHMIDetails7(hmiidInner);
                                    if (MWOHMIData != null) //obviously != 0
                                    {
                                        if (MWOHMIData.Date == null) //=> lower OpNo is not submitted.
                                        {
                                            IssueMsg = " Start WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + localOPNoString;
                                            //return RedirectToAction("Index");
                                            break;
                                        }
                                        else
                                        {
                                            IsInHMI = true;
                                        }
                                    }
                                }

                                foreach (tbl_multiwoselection rowHMI in WIPMWOHistorian)
                                {
                                    int hmiidInner = Convert.ToInt32(rowHMI.HMIID);
                                    //var MWOHMIData = db.tblhmiscreens.Where(m => m.HMIID == hmiidInner).FirstOrDefault();
                                    tbllivehmiscreen MWOHMIData = obj.GetLiveHMIDetails7(hmiidInner);
                                    if (MWOHMIData != null) //obviously != 0
                                    {
                                        if (MWOHMIData.Date == null) //=> lower OpNo is not submitted.
                                        {
                                            IssueMsg = " Start WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + localOPNoString;
                                            //return RedirectToAction("Index");
                                            break;
                                        }
                                        else
                                        {
                                            IsInHMI = true;
                                        }
                                    }
                                }
                                #endregion
                            }
                        }
                    }

                    //commented on 2017-05-29
                    /////to Catch those Manual WorkOrders 
                    //string WIPQuery1 = @"SELECT * from tblhmiscreen where  HMIID IN ( SELECT Max(HMIID) from tblhmiscreen where  HMIID IN  ( SELECT HMIID from tblhmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo != '" + opNo + "' and  IsMultiWO = 0 and DDLWokrCentre is null order by HMIID ) group by Work_Order_No,PartNo,OperationNo ) order by OperationNo ;";
                    //var WIPDDL1 = db.tblhmiscreens.SqlQuery(WIPQuery1).ToList();
                    //foreach (var row in WIPDDL1)
                    //{
                    //    int InnerOpNo = Convert.ToInt32(row.OperationNo);
                    //    if (InnerOpNo < Convert.ToInt32(opNo))
                    //    {
                    //        string WIPQueryHMI = @"SELECT * from tblhmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + InnerOpNo + "' order by HMIID limit 1 ";
                    //        var WIP1 = db.tblhmiscreens.SqlQuery(WIPQueryHMI).ToList();
                    //        if (WIP1.Count == 0)
                    //        {
                    //            Session["VError"] = " Select & Start WONo: " + row.Work_Order_No + " and PartNo: " + row.PartNo + " and OperationNo: " + InnerOpNo;
                    //            return RedirectToAction("Index");
                    //        }
                    //        foreach (var rowHMI in WIP1)
                    //        {
                    //            if (rowHMI.Date == null) //=> lower OpNo is not submitted.
                    //            {
                    //                Session["VError"] = " Start WONo: " + row.Work_Order_No + " and PartNo: " + row.PartNo + " and OperationNo: " + InnerOpNo;
                    //                return RedirectToAction("Index");
                    //            }
                    //        }
                    //    }
                    //}
                }
                #endregion

                if (!string.IsNullOrEmpty(woNo) && !string.IsNullOrEmpty(opNo) && !string.IsNullOrEmpty(partNo))
                {
                    //var InHMIData = db.tbllivehmiscreens.Where(m => m.Work_Order_No == woNo && m.PartNo == partNo && m.OperationNo == opNo && m.Status == 0 && m.HMIID != hmiid && m.MachineID == MachineId).FirstOrDefault();
                    tbllivehmiscreen InHMIData = obj.GetLiveHMI1Details(woNo, partNo, opNo, hmiid, MachineId);
                    if (InHMIData != null)
                    {
                        Session["Error"] = "Duplicate WorkOrder:" + woNo + " OpNo: " + opNo + " PartNo:" + partNo;
                        //db.tblhmiscreens.Remove(hmiidDataDup);
                        //db.Entry(hmiidDataDup).State = System.Data.Entity.EntityState.Deleted;
                        //db.SaveChanges();
                        obj.DeleteHMIScreenDetails(hmiidDataDup.HMIID);
                        return RedirectToAction("Index");
                    }
                }

            }

            //tbllivehmiscreen hmiidData = db.tbllivehmiscreens.Where(m => m.HMIID == HMIID).FirstOrDefault();
            tbllivehmiscreen hmiidData = obj.GetLiveHMIDetails6(HMIID);
            if (hmiidData.IsHold == 0)
            {
                if (hmiidData.Date == null)
                {
                    if (hmiidData.OperatorDet != null && hmiidData.Shift != null && hmiidData.Project != null && hmiidData.Prod_FAI != null && hmiidData.PartNo != null && hmiidData.Work_Order_No != null && hmiidData.OperationNo != null && hmiidData.Target_Qty.HasValue)
                    {
                        hmiidData.Date = DateTime.Now;

                        //Get Processed Qty
                        int newProcessedQty = 0;
                        string woNo = Convert.ToString(hmiidData.Work_Order_No);
                        string opNo = Convert.ToString(hmiidData.OperationNo);
                        string partNo = Convert.ToString(hmiidData.PartNo);
                        int TargetQtyNew = Convert.ToInt32(hmiidData.Target_Qty);
                        int DeliveredQty = Convert.ToInt32(hmiidData.Delivered_Qty);
                        int ProcessedQty = Convert.ToInt32(hmiidData.ProcessQty);
                        newProcessedQty = DeliveredQty + ProcessedQty;

                        if (Convert.ToInt32(hmiidData.isWorkInProgress) == 1)
                        {
                            Session["Error"] = "Job is Finished for WorkOrder:" + woNo + " OpNo: " + opNo + " PartNo:" + partNo;
                            //db.tbllivehmiscreens.Remove(hmiidData);
                            //db.SaveChanges();
                            obj.DeleteHMIScreenDetails(hmiidData.HMIID);
                            return RedirectToAction("Index");
                        }

                        if (TargetQtyNew == newProcessedQty)
                        {
                            hmiidData.Target_Qty = newProcessedQty;
                            hmiidData.ProcessQty = newProcessedQty;
                            hmiidData.SplitWO = "No";
                            hmiidData.isWorkInProgress = 1;
                            hmiidData.Status = 2;
                            hmiidData.Time = hmiidData.Date;
                            hmiidData.Delivered_Qty = 0;

                            //db.Entry(hmiidData).State = System.Data.Entity.EntityState.Modified;
                            //db.SaveChanges();

                            // obj.UpdateLivHMIDets(hmiidData.HMIID, Convert.ToInt32(hmiidData.Target_Qty), hmiidData.ProcessQty, hmiidData.SplitWO, hmiidData.isWorkInProgress, Convert.ToInt32(hmiidData.Status), Convert.ToDateTime(hmiidData.Time), Convert.ToInt32(hmiidData.Delivered_Qty));

                            //if it existing in DDLList Update 
                            //var DDLList = db.tblddls.Where(m => m.WorkOrder == hmiidData.Work_Order_No && m.MaterialDesc == hmiidData.PartNo && m.OperationNo == hmiidData.OperationNo && m.IsCompleted == 0).ToList();
                            List<tblddl> DDLList = obj.GetddlDets(hmiidData.Work_Order_No, hmiidData.PartNo, hmiidData.OperationNo);
                            foreach (tblddl row in DDLList)
                            {
                                //row.IsCompleted = 1;
                                //db.Entry(row).State = System.Data.Entity.EntityState.Modified;
                                //db.SaveChanges();
                                // obj.UpdateddlDetails(row.DDLID);
                            }

                            Session["Error"] = "Job is Finished for WorkOrder:" + woNo + " OpNo: " + opNo + " PartNo:" + partNo;
                            return RedirectToAction("Index");

                        }

                        //2017-06-03
                        //var getProcessQty = db.tblhmiscreens.Where(m => m.Work_Order_No == woNo && m.PartNo == partNo && m.OperationNo == opNo).OrderByDescending(m => m.HMIID).Take(2).ToList();
                        //if (getProcessQty.Count == 2)
                        //{
                        //    string delivString = Convert.ToString(getProcessQty[1].Delivered_Qty);
                        //    int.TryParse(delivString, out PrvDeliveredQty);

                        //    string processString = Convert.ToString(getProcessQty[1].ProcessQty);
                        //    int.TryParse(processString, out PrvProcessQty);

                        //    newProcessedQty = PrvProcessQty + PrvDeliveredQty;
                        //    if (Convert.ToInt32(getProcessQty[1].isWorkInProgress) == 1 || TargetQtyNew == newProcessedQty)
                        //    {
                        //        Session["Error"] = "Job is Finished for WorkOrder:" + woNo + " OpNo: " + opNo + " PartNo:" + partNo;

                        //        //2017-01-07
                        //        //hmiidData.Prod_FAI = null;
                        //        //hmiidData.Target_Qty = null;
                        //        //hmiidData.OperationNo = null;
                        //        //hmiidData.PartNo = null;
                        //        //hmiidData.Work_Order_No = null;
                        //        //hmiidData.Project = null;
                        //        //hmiidData.Date = null;
                        //        //hmiidData.DDLWokrCentre = null;
                        //        //hmiidData.isWorkOrder = 0;
                        //        //hmiidData.ProcessQty = 0;
                        //        //Session["FromDDL"] = 2;
                        //        //TempData["ForDDL2"] = 2;

                        //        //db.Entry(hmiidData).State = System.Data.Entity.EntityState.Modified;
                        //        //db.SaveChanges();

                        //        db.tblhmiscreens.Remove(hmiidData);
                        //        db.SaveChanges();
                        //        return RedirectToAction("Index");
                        //    }
                        //}

                        if (TargetQtyNew < newProcessedQty)
                        {
                            Session["Error"] = "Previous ProcessedQty :" + newProcessedQty + ". TargetQty Cannot be Less than Processed";
                            hmiidData.ProcessQty = 0;
                            hmiidData.Date = null;
                            Session["FromDDL"] = 2;
                            TempData["ForDDL2"] = 2;
                            //db.Entry(hmiidData).State = System.Data.Entity.EntityState.Modified;
                            //db.SaveChanges();
                            // obj.UpdateLivHMI1Dets(hmiidData.HMIID, hmiidData.ProcessQty, Convert.ToDateTime(hmiidData.Date));
                            return RedirectToAction("Index");
                        }

                        //hmiidData.ProcessQty = Convert.ToInt32(PrvProcessQty + PrvDeliveredQty);


                        #region 2017-02-07
                        //Check if Lower Operation No is submitted or JF  or PF 'ed Prior to this.
                        //1) Get Related WO's , order by Ascending.
                        //2) Check if current Opno is Allowed to be Submitted. 
                        //(CONDITION : All Opno's lesser than this has to be submitted ( => || PF or JF 'ed Atleast Once).)

                        int OperationNoInt = Convert.ToInt32(opNo);
                        //var hmiDataInAscendingOrder = db.tblhmiscreens.Where(m => m.Work_Order_No == woNo && m.PartNo == partNo && m.OperationNo != opNo && m.OperationNo.toInt < opNo)).OrderBy(m => m.OperationNo).ToList(); //&& m.OperationNo < opNo "< Cannot be applied to Strings"
                        //string WIPQuery = @"SELECT * from tbllivehmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo != '" + opNo + "' and MachineID = '" + MachineId + "' order by HMIID   ";
                        //var WIP = db.tbllivehmiscreens.SqlQuery(WIPQuery).ToList();
                        List<tbllivehmiscreen> WIP = obj.GetLiveHMIDetailsList(woNo, partNo, opNo, MachineId);
                        //string WIPQueryHistorian = @"SELECT * from tblhmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo != '" + opNo + "' and MachineID = '" + MachineId + "' order by HMIID   ";
                        //var WIPHistorian = db.tblhmiscreens.SqlQuery(WIPQueryHistorian).ToList();
                        List<tblhmiscreen> WIPHistorian = obj.GetHisHMIDetailsList(woNo, partNo, opNo, MachineId);
                        foreach (tbllivehmiscreen row in WIP)
                        {
                            int InnerOpNo = Convert.ToInt32(row.OperationNo);
                            if (OperationNoInt > InnerOpNo)
                            {
                                if (row.Date == null) //=> lower OpNo is not submitted.
                                {
                                    Session["VError"] = " Submit WONo: " + row.Work_Order_No + " and PartNo: " + row.PartNo + " and OperationNo: " + InnerOpNo;
                                    return RedirectToAction("Index");
                                }
                            }
                        }
                        foreach (tblhmiscreen row in WIPHistorian)
                        {
                            int InnerOpNo = Convert.ToInt32(row.OperationNo);
                            if (OperationNoInt > InnerOpNo)
                            {
                                if (row.Date == null) //=> lower OpNo is not submitted.
                                {
                                    Session["VError"] = " Submit WONo: " + row.Work_Order_No + " and PartNo: " + row.PartNo + " and OperationNo: " + InnerOpNo;
                                    return RedirectToAction("Index");
                                }
                            }
                        }
                        bool IsInHMI = true;
                        //string WIPQuery2 = @"SELECT * from tblddl where WorkOrder = '" + woNo + "' and MaterialDesc = '" + partNo + "' and OperationNo != '" + opNo + "'  and IsCompleted = 0 order by WorkOrder,MaterialDesc,OperationNo ";
                        //var WIPDDL = db.tblddls.SqlQuery(WIPQuery2).ToList();
                        List<tblddl> WIPDDL = obj.GetddlDetails(woNo, partNo, opNo);
                        foreach (tblddl row in WIPDDL)
                        {
                            Session["VError"] = null;
                            IsInHMI = true; //reinitialize
                            int InnerOpNo = Convert.ToInt32(row.OperationNo);
                            if (InnerOpNo < OperationNoInt)
                            {
                                //string WIPQueryHMI = @"SELECT * from tbllivehmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + InnerOpNo + "' order by HMIID limit 1 ";
                                //var WIP1 = db.tbllivehmiscreens.SqlQuery(WIPQueryHMI).ToList();
                                List<tbllivehmiscreen> WIP1 = obj.GetLive1HMIScreenDetails(woNo, partNo, Convert.ToString(InnerOpNo));
                                //string WIPQueryHMIHistorian = @"SELECT * from tblhmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + InnerOpNo + "' order by HMIID limit 1 ";
                                //var WIP1Historian = db.tblhmiscreens.SqlQuery(WIPQueryHMIHistorian).ToList();
                                List<tblhmiscreen> WIP1Historian = obj.GetLive1HScreenDetails(woNo, partNo, Convert.ToString(InnerOpNo));
                                if (WIP1.Count == 0 || WIP1Historian.Count == 0)
                                {
                                    #region Commented by Ashok
                                    //Session["VError"] = " Select & Start WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
                                    //IsItWrong = true;
                                    //IsInHMI = false;
                                    ////IsInHMI = true;
                                    #endregion
                                }
                                else
                                {
                                    foreach (tbllivehmiscreen rowHMI in WIP1)
                                    {
                                        if (rowHMI.Date == null) //=> lower OpNo is not submitted.
                                        {
                                            Session["VError"] = " Start WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
                                            return RedirectToAction("Index");
                                        }
                                    }
                                    if (WIP1Historian != null)
                                    {
                                        foreach (tblhmiscreen rowHMI in WIP1Historian)
                                        {
                                            if (rowHMI.Date == null) //=> lower OpNo is not submitted.
                                            {
                                                Session["VError"] = " Start WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
                                                return RedirectToAction("Index");
                                            }
                                        }
                                    }
                                }

                                if (!IsInHMI)
                                {
                                    #region //also check in MultiWO table
                                    //string WIPQueryMultiWO = @"SELECT * from tbllivemultiwoselection where WorkOrder = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + InnerOpNo + "' order by MultiWOID limit 1 ";
                                    //var WIPMWO = db.tbllivemultiwoselections.SqlQuery(WIPQueryMultiWO).ToList();
                                    List<tbllivemultiwoselection> WIPMWO = obj.GetMultiWOtDetails(woNo, partNo, Convert.ToString(InnerOpNo));
                                    //string WIPQueryMultiWOHistroian = @"SELECT * from tbl_multiwoselection where WorkOrder = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + InnerOpNo + "' order by MultiWOID limit 1 ";
                                    //var WIPMWOHistorian = db.tbl_multiwoselection.SqlQuery(WIPQueryMultiWOHistroian).ToList();
                                    List<tbl_multiwoselection> WIPMWOHistorian = obj.GetMultiWorkSelectionDetails(woNo, partNo, Convert.ToString(InnerOpNo));
                                    if (WIPMWO.Count == 0 || WIPMWOHistorian.Count == 0)
                                    {
                                        Session["VError"] = " Select  WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
                                        return RedirectToAction("Index");
                                        //IsInHMI = false;
                                        break;
                                    }

                                    foreach (tbllivemultiwoselection rowHMI in WIPMWO)
                                    {
                                        int hmiid = Convert.ToInt32(rowHMI.HMIID);
                                        //var MWOHMIData = db.tbllivehmiscreens.Where(m => m.HMIID == hmiid).FirstOrDefault();
                                        tbllivehmiscreen MWOHMIData = obj.GetLiveHMIDetails7(hmiid);
                                        if (MWOHMIData != null) //obviously != 0
                                        {
                                            if (MWOHMIData.Date == null) //=> lower OpNo is not submitted.
                                            {
                                                Session["VError"] = " Start WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
                                                return RedirectToAction("Index");
                                                //break;
                                            }
                                            else
                                            {
                                                IsInHMI = true;
                                            }
                                        }
                                    }

                                    foreach (tbl_multiwoselection rowHMI in WIPMWOHistorian)
                                    {
                                        int hmiid = Convert.ToInt32(rowHMI.HMIID);
                                        //var MWOHMIData = db.tblhmiscreens.Where(m => m.HMIID == hmiid).FirstOrDefault();
                                        tbllivehmiscreen MWOHMIData = obj.GetLiveHMIDetails7(hmiid);
                                        if (MWOHMIData != null) //obviously != 0
                                        {
                                            if (MWOHMIData.Date == null) //=> lower OpNo is not submitted.
                                            {
                                                Session["VError"] = " Start WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
                                                return RedirectToAction("Index");
                                                //break;
                                            }
                                            else
                                            {
                                                IsInHMI = true;
                                            }
                                        }
                                    }
                                    #endregion
                                }

                            }
                        }

                        //commmented On 2017-05-29
                        ////2017-01-21 I guess when its purely ManualEntry
                        ////string WIPQuery1 = @"SELECT * from tblhmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo != '" + opNo + "' order by Work_Order_No,PartNo,OperationNo";
                        //string WIPQuery1 = @"SELECT * from tblhmiscreen where  HMIID IN ( SELECT Max(HMIID) from tblhmiscreen where  HMIID IN  ( SELECT HMIID from tblhmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo != '" + opNo + "' and IsMultiWO = 0 and DDLWokrCentre is null order by HMIID ) group by Work_Order_No,PartNo,OperationNo  ) order by OperationNo;";
                        //var WIPDDL1 = db.tblhmiscreens.SqlQuery(WIPQuery1).ToList();
                        //foreach (var row in WIPDDL1)
                        //{
                        //    int InnerOpNo = Convert.ToInt32(row.OperationNo);
                        //    if (InnerOpNo < OperationNoInt)
                        //    {
                        //        string WIPQueryHMI = @"SELECT * from tblhmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + InnerOpNo + "' order by HMIID limit 1 ";
                        //        var WIP2 = db.tblhmiscreens.SqlQuery(WIPQueryHMI).ToList();
                        //        foreach (var rowHMI in WIP2)
                        //        {
                        //            if (rowHMI.Date == null) //=> lower OpNo is not submitted.
                        //            {
                        //                Session["VError"] = " Start WONo: " + row.Work_Order_No + " and PartNo: " + row.PartNo + " and OperationNo: " + InnerOpNo;
                        //                return RedirectToAction("Index");
                        //            }
                        //        }
                        //        if (WIP2.Count == 0)
                        //        {
                        //            Session["VError"] = " Select & Start WONo: " + row.Work_Order_No + " and PartNo: " + row.PartNo + " and OperationNo: " + InnerOpNo;
                        //            return RedirectToAction("Index");
                        //        }
                        //    }
                        //}

                        #endregion

                        int ReworkOrder = 0;
                        string ReworkOrderString = Convert.ToString(Session["isWorkOrder"]);
                        if (int.TryParse(ReworkOrderString, out ReworkOrder))
                        {
                            if (ReworkOrderString == "1")
                            {
                                hmiidData.isWorkOrder = 1;
                            }
                            else
                            {
                                hmiidData.isWorkOrder = 0;
                            }
                        }

                        Session["WorkOrderClicked"] = 1;
                        hmiidData.Status = 0;
                        // obj.UpdateLiv2HMIDetails(hmiidData.HMIID, Convert.ToDateTime(hmiidData.Date), Convert.ToInt32(hmiidData.Status), hmiidData.isWorkOrder);
                        //db.Entry(hmiidData).State = System.Data.Entity.EntityState.Modified;
                        //db.SaveChanges();
                    }
                    else
                    {
                        Session["VError"] = "Please enter all Details Before StartAll is Clicked.";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    Session["VError"] = "WorkOrder is already Started.";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                Session["VError"] = "Please end HOLD before Start";
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
        #endregion
        public bool CheckWhetherWoStartedOrNot(string woNo, string opNo, string partNo)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);
            bool ret = false;
            List<tbllivehmiscreen> liveHmiDet = obj.ValidateLiveHMIData(woNo, opNo, partNo);
            if (liveHmiDet.Count != 0)
            {
                foreach (tbllivehmiscreen hmirow in liveHmiDet)
                {
                    if (hmirow.Date == null && hmirow.Time == null)
                    {
                        ret = false;
                        break;
                    }
                    else if (hmirow.Date != null && hmirow.Time == null)
                    {
                        ret = true;
                    }
                    else if (hmirow.Date != null && hmirow.Time != null)
                    {
                        ret = true;
                    }
                }
            }
            else
            {
                List<tbllivehmiscreen> HistHmiDet = obj.ValidateHistHMIData(woNo, opNo, partNo);
                if (HistHmiDet.Count != 0)
                {
                    foreach (tbllivehmiscreen hmirow in HistHmiDet)
                    {
                        if (hmirow.Date == null && hmirow.Time == null)
                        {
                            ret = false;
                            break;
                        }
                        else if (hmirow.Date != null && hmirow.Time == null)
                        {
                            ret = true;
                        }
                        else if (hmirow.Date != null && hmirow.Time != null)
                        {
                            ret = true;
                        }
                    }
                }
            }

            return ret;
        }

        public ActionResult IndividualHold(int HMIID = 0, int cjtextbox8 = 0)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);
            Session["split"] = null;
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            //if (IDLEorGenericWorkisON())
            //{
            //    Session["Error"] = "Please End IDLE/GenericWork Before Selecting New Work Orders";
            //    return RedirectToAction("Index");
            //}

            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            int machineId = Convert.ToInt32(Session["MachineID"]);

            if (string.IsNullOrEmpty(cjtextbox8.ToString().Trim()))
            {
                Session["VError"] = " Please Enter All Details Before Hold ";
                return RedirectToAction("Index");
            }

            //tbllivehmiscreen hmiidData = db.tbllivehmiscreens.Where(m => m.HMIID == HMIID).FirstOrDefault();
            tbllivehmiscreen hmiidData = obj.GetLiveHMIDetails6(HMIID);
            //Add server side validations.
            if (hmiidData != null)
            {
                if (Convert.ToInt32(hmiidData.IsHold) != 1)
                {
                    if (hmiidData.Date != null)
                    {
                        //hmiidData.Delivered_Qty = cjtextbox8;
                        ////Do it after entering the code.

                        //hmiidData.isWorkInProgress = 0;
                        //hmiidData.Time = DateTime.Now; 
                        //db.Entry(hmiidData).State = System.Data.Entity.EntityState.Modified;
                        //db.SaveChanges();
                        obj.UpdateLiveHMIDets(hmiidData.HMIID, cjtextbox8);

                        return RedirectToAction("HoldCodeEntry", "HMIScree", new { Hmiid = HMIID, Bid = 0 });

                    }
                    else
                    {
                        Session["VError"] = " Please Click Start Before Clicking Hold ";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    Session["VError"] = "WorkOrder is already in HOLD ";
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult IndividualFinish(int HMIID = 0, int cjtextbox8 = 0)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);
            Session["split"] = null;
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            //if (IDLEorGenericWorkisON())
            //{
            //    Session["Error"] = "Please End IDLE/GenericWork Before Selecting New Work Orders";
            //    return RedirectToAction("Index");
            //}

            int machineID = Convert.ToInt32(Session["MachineID"]);
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            int machineId = Convert.ToInt32(Session["MachineID"]);

            if (string.IsNullOrEmpty(cjtextbox8.ToString().Trim()))
            {
                Session["VError"] = " Please Enter All Details Before Job Finish ";
                return RedirectToAction("Index");
            }

            //tbllivehmiscreen hmiidData = db.tbllivehmiscreens.Where(m => m.HMIID == HMIID).FirstOrDefault();
            tbllivehmiscreen hmiidData = obj.GetLiveHMIDetails6(HMIID);
            //Add server side validations.
            if (hmiidData != null)
            {
                if (hmiidData.IsHold != 1)
                {
                    if (hmiidData.Date != null)
                    {
                        int ProcessQty = 0;
                        string processString1 = Convert.ToString(hmiidData.ProcessQty);
                        int.TryParse(processString1, out ProcessQty);

                        if ((cjtextbox8 + ProcessQty) == hmiidData.Target_Qty)
                        {
                            string wono = hmiidData.Work_Order_No;
                            string partno = hmiidData.PartNo;
                            string opno = hmiidData.OperationNo;
                            int OperationNoInt = Convert.ToInt32(opno);

                            #region 2017-07-01
                            //New Logic to Overcome WorkOrder Sequence Scenario 2017-02-03
                            //string WIPQuery6 = @"SELECT * from tblddl where WorkOrder = '" + wono + "' and MaterialDesc = '" + partno + "' and OperationNo != '" + opno + "' and IsCompleted = 0 order by OperationNo ";
                            //var WIPDDL6 = db.tblddls.SqlQuery(WIPQuery6).ToList();
                            List<tblddl> WIPDDL6 = obj.Getddl1Details(wono, partno, opno);
                            foreach (tblddl row in WIPDDL6)
                            {
                                int InnerOpNo = Convert.ToInt32(row.OperationNo);
                                string ddlOpno = row.OperationNo;
                                if (InnerOpNo < OperationNoInt)
                                {
                                    int PrvProcessQty = 0, PrvDeliveredQty = 0, TotalProcessQty = 0, ishold = 0;
                                    #region new code
                                    //here 1st get latest of delivered and processed among row in tblHMIScreen & tblmulitwoselection
                                    int isHMIFirst = 2; //default NO History for that wo,pn,on

                                    //var mulitwoData = db.tbllivemultiwoselections.Where(m => m.WorkOrder == wono && m.PartNo == partno && m.OperationNo == ddlOpno && m.tbllivehmiscreen.isWorkInProgress != 2).OrderByDescending(m => m.tbllivehmiscreen.Time).Take(1).ToList();
                                    List<tbllivemultiwoselection> mulitwoData = obj.GetMultiWODetails2(wono, partno, ddlOpno);
                                    //var hmiData = db.tbllivehmiscreens.Where(m => m.Work_Order_No == wono && m.PartNo == partno && m.OperationNo == ddlOpno && m.isWorkInProgress != 2).OrderByDescending(m => m.Time).Take(1).ToList();
                                    List<tbllivehmiscreen> hmiData = obj.GetLiveHMIDetails11(wono, partno, opno);
                                    if (hmiData.Count > 0 && mulitwoData.Count > 0) // now check for greatest amongst
                                    {
                                        DateTime multiwoDateTime = Convert.ToDateTime(mulitwoData[0].tbllivehmiscreen.Time);
                                        DateTime hmiDateTime = Convert.ToDateTime(hmiData[0].Time);

                                        if (Convert.ToInt32(multiwoDateTime.Subtract(hmiDateTime).TotalSeconds) > 0)
                                        {
                                            isHMIFirst = 1;
                                        }
                                        else
                                        {
                                            isHMIFirst = 0;
                                        }

                                    }
                                    else if (mulitwoData.Count > 0)
                                    {
                                        isHMIFirst = 1;
                                    }
                                    else if (hmiData.Count > 0)
                                    {
                                        isHMIFirst = 0;
                                    }

                                    if (isHMIFirst == 1)
                                    {
                                        string delivString = Convert.ToString(mulitwoData[0].DeliveredQty);
                                        int delivInt = 0;
                                        int.TryParse(delivString, out delivInt);

                                        string processString = Convert.ToString(mulitwoData[0].ProcessQty);
                                        int procInt = 0;
                                        int.TryParse(processString, out procInt);

                                        PrvProcessQty += procInt;
                                        PrvDeliveredQty += delivInt;

                                        ishold = mulitwoData[0].tbllivehmiscreen.IsHold;
                                        ishold = ishold == 2 ? 0 : ishold;

                                    }
                                    else if (isHMIFirst == 0)
                                    {
                                        string delivString = Convert.ToString(hmiData[0].Delivered_Qty);
                                        int delivInt = 0;
                                        int.TryParse(delivString, out delivInt);

                                        string processString = Convert.ToString(hmiData[0].ProcessQty);
                                        int procInt = 0;
                                        int.TryParse(processString, out procInt);

                                        PrvProcessQty += procInt;
                                        PrvDeliveredQty += delivInt;

                                        ishold = hmiData[0].IsHold;
                                        ishold = ishold == 2 ? 0 : ishold;
                                    }
                                    else
                                    {
                                        //no previous delivered or processed qty so Do Nothing.
                                    }
                                    #endregion
                                    TotalProcessQty = PrvProcessQty + PrvDeliveredQty;
                                    //var hmiPFed = db.tblhmiscreens.Where(m => m.Work_Order_No == woNo && m.PartNo == partNo && m.OperationNo == opNo).OrderByDescending(m => m.Time).FirstOrDefault();

                                    if (Convert.ToInt32(row.TargetQty) == TotalProcessQty)
                                    {
                                        #region
                                        if (isHMIFirst == 1 && Convert.ToInt32(row.TargetQty) < Convert.ToInt32(mulitwoData[0].TargetQty))
                                        {
                                            int hmiidmultitbl = Convert.ToInt32(mulitwoData[0].HMIID);
                                            //var hmiTomulittblData = db.tbllivehmiscreens.Find(hmiidmultitbl);
                                            tbllivehmiscreen hmiTomulittblData = obj.GetLiveHMIDetails6(hmiidmultitbl);
                                            if (hmiTomulittblData != null)
                                            {
                                                //tbllivehmiscreen tblh = new tbllivehmiscreen();

                                                //tblh.CorrectedDate = row.CorrectedDate;
                                                //tblh.Date = DateTime.Now;
                                                //tblh.Time = DateTime.Now;
                                                //tblh.PEStartTime = DateTime.Now;
                                                //tblh.DDLWokrCentre = hmiTomulittblData.DDLWokrCentre;
                                                //tblh.Delivered_Qty = 0;
                                                //tblh.DoneWithRow = 1;
                                                //tblh.IsHold = 0;
                                                //tblh.IsMultiWO = 0;
                                                //tblh.isUpdate = 1;
                                                //tblh.isWorkInProgress = 1;
                                                //tblh.isWorkOrder = hmiTomulittblData.isWorkOrder;
                                                //tblh.MachineID = hmiTomulittblData.MachineID;
                                                //tblh.OperationNo = mulitwoData[0].OperationNo;
                                                //tblh.OperatiorID = hmiTomulittblData.OperatiorID;
                                                //tblh.OperatorDet = hmiTomulittblData.OperatorDet;
                                                //tblh.PartNo = mulitwoData[0].PartNo;
                                                //tblh.ProcessQty = TotalProcessQty;
                                                //tblh.Prod_FAI = hmiTomulittblData.Prod_FAI;
                                                //tblh.Project = hmiTomulittblData.Project;
                                                //tblh.Rej_Qty = hmiTomulittblData.Rej_Qty;
                                                //tblh.Shift = hmiTomulittblData.Shift;
                                                //tblh.SplitWO = "No";
                                                //tblh.Status = hmiTomulittblData.Status;
                                                //tblh.Target_Qty = TotalProcessQty;
                                                //tblh.Work_Order_No = mulitwoData[0].WorkOrder;
                                                //tblh.HMIID = ;
                                                //db.tbllivehmiscreens.Add(tblh);
                                                //db.SaveChanges();

                                                obj.InsertLiveHMIScreenDetails(hmiTomulittblData.MachineID, hmiTomulittblData.OperatiorID, hmiTomulittblData.Shift, DateTime.Now, DateTime.Now, hmiTomulittblData.Project, mulitwoData[0].PartNo, mulitwoData[0].OperationNo, Convert.ToInt32(hmiTomulittblData.Rej_Qty), mulitwoData[0].WorkOrder, TotalProcessQty, 0, Convert.ToInt32(hmiTomulittblData.Status), row.CorrectedDate, hmiTomulittblData.Prod_FAI, 1, 1, 1, hmiTomulittblData.isWorkOrder, hmiTomulittblData.OperatorDet, DateTime.Now, TotalProcessQty, hmiTomulittblData.DDLWokrCentre, 0, 0, "No", 0);
                                            }

                                        }
                                        else if (isHMIFirst == 0 && Convert.ToInt32(row.TargetQty) < Convert.ToInt32(hmiData[0].Target_Qty))
                                        {
                                            //tbllivehmiscreen tblh = new tbllivehmiscreen();

                                            //tblh.CorrectedDate = row.CorrectedDate;
                                            //tblh.Date = DateTime.Now;
                                            //tblh.Time = DateTime.Now;
                                            //tblh.PEStartTime = DateTime.Now;
                                            //tblh.DDLWokrCentre = hmiData[0].DDLWokrCentre;
                                            //tblh.Delivered_Qty = 0;
                                            //tblh.DoneWithRow = 1;
                                            //tblh.IsHold = 0;
                                            //tblh.IsMultiWO = hmiData[0].IsMultiWO;
                                            //tblh.isUpdate = 1;
                                            //tblh.isWorkInProgress = 1;
                                            //tblh.isWorkOrder = hmiData[0].isWorkOrder;
                                            //tblh.MachineID = hmiData[0].MachineID;
                                            //tblh.OperationNo = hmiData[0].OperationNo;
                                            //tblh.OperatiorID = hmiData[0].OperatiorID;
                                            //tblh.OperatorDet = hmiData[0].OperatorDet;
                                            //tblh.PartNo = hmiData[0].PartNo;
                                            //tblh.ProcessQty = TotalProcessQty;
                                            //tblh.Prod_FAI = hmiData[0].Prod_FAI;
                                            //tblh.Project = hmiData[0].Project;
                                            //tblh.Rej_Qty = hmiData[0].Rej_Qty;
                                            //tblh.Shift = hmiData[0].Shift;
                                            //tblh.SplitWO = hmiData[0].SplitWO;
                                            //tblh.Status = hmiData[0].Status;
                                            //tblh.Target_Qty = TotalProcessQty;
                                            //tblh.Work_Order_No = hmiData[0].Work_Order_No;
                                            //// tblh.HMIID = ;
                                            //db.tbllivehmiscreens.Add(tblh);
                                            //db.SaveChanges();

                                            obj.InsertLiveHMIScreenDetails(hmiData[0].MachineID, hmiData[0].OperatiorID, hmiData[0].Shift, DateTime.Now, DateTime.Now, hmiData[0].Project, hmiData[0].PartNo, hmiData[0].OperationNo, Convert.ToInt32(hmiData[0].Rej_Qty), hmiData[0].Work_Order_No, TotalProcessQty, 0, Convert.ToInt32(hmiData[0].Status), row.CorrectedDate, hmiData[0].Prod_FAI, 1, 1, 1, hmiData[0].isWorkOrder, hmiData[0].OperatorDet, DateTime.Now, TotalProcessQty, hmiData[0].DDLWokrCentre, hmiData[0].IsMultiWO, 0, hmiData[0].SplitWO, 0);
                                        }
                                        #endregion

                                        //row.IsCompleted = 1;
                                        //db.Entry(row).State = System.Data.Entity.EntityState.Modified;
                                        //db.SaveChanges();
                                        obj.UpdateddlDetails(row.DDLID);
                                    }
                                }
                            }
                            #endregion

                            #region 2017-02-07
                            //New Logic to Overcome WorkOrder Sequence Scenario 2017-02-03
                            //string WIPQuery1 = @"SELECT * from tblddl where WorkOrder = '" + wono + "' and MaterialDesc = '" + partno + "' and OperationNo != '" + opno + "' and IsCompleted = 0 order by OperationNo ";
                            //var WIPDDL1 = db.tblddls.SqlQuery(WIPQuery1).ToList();
                            List<tblddl> WIPDDL1 = obj.Getddl1Details(wono, partno, opno);
                            foreach (tblddl row in WIPDDL1)
                            {
                                int InnerOpNo = Convert.ToInt32(row.OperationNo);
                                if (InnerOpNo < OperationNoInt)
                                {
                                    if (row.IsCompleted == 0)
                                    {
                                        Session["VError"] = " Finish WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
                                        return RedirectToAction("Index");
                                    }
                                    else
                                    {
                                        Session["VError"] = null;
                                    }

                                    //bool IsItWrong = false;
                                    //string WIPQueryHMI = @"SELECT * from tblhmiscreen where Work_Order_No = '" + wono + "' and PartNo = '" + partno + "' and OperationNo = '" + InnerOpNo + "' order by HMIID desc limit 1 ";
                                    //var WIP = db.tblhmiscreens.SqlQuery(WIPQueryHMI).ToList();

                                    //if (WIP.Count == 0)
                                    //{
                                    //    Session["VError"] = " Finish WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
                                    //    IsItWrong = true;
                                    //}
                                    //else
                                    //{
                                    //    foreach (var rowHMI in WIP)
                                    //    {
                                    //        if (rowHMI.isWorkInProgress != 1) //=> lower OpNo is in HMIScreen & not Finished.
                                    //        {
                                    //            Session["VError"] = " Finish WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
                                    //            //return RedirectToAction("Index");
                                    //            IsItWrong = true;
                                    //        }
                                    //    }
                                    //}
                                    //if (IsItWrong)
                                    //{
                                    //    //Strange , it might have been started in Normal WorkCenter as MultiWorkOrder.
                                    //    string WIPQueryMultiWO = @"SELECT * from tbl_multiwoselection where WorkOrder = '" + wono + "' and PartNo = '" + partno + "' and OperationNo = '" + InnerOpNo + "' order by MultiWOID desc limit 1 ";
                                    //    var WIPMWO = db.tbl_multiwoselection.SqlQuery(WIPQueryMultiWO).ToList();

                                    //    if (WIPMWO.Count == 0)
                                    //    {
                                    //        Session["VError"] = " Finish WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
                                    //        return RedirectToAction("Index");
                                    //    }

                                    //    foreach (var rowHMI in WIPMWO)
                                    //    {
                                    //        int hmiid = Convert.ToInt32(rowHMI.HMIID);
                                    //        var MWOHMIData = db.tblhmiscreens.Where(m => m.HMIID == hmiid).FirstOrDefault();
                                    //        if (MWOHMIData != null)
                                    //        {
                                    //            if (MWOHMIData.isWorkInProgress != 1) //=> lower OpNo is not Finished.
                                    //            {
                                    //                Session["VError"] = " Finish WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
                                    //                return RedirectToAction("Index");
                                    //            }
                                    //        }
                                    //    }
                                    //}


                                }
                            }

                            // Commented on 2017-05-29
                            //string WIPQuery = @"SELECT * from tblhmiscreen where  HMIID IN ( SELECT Max(HMIID) from tblhmiscreen where Work_Order_No = '" + wono + "' and PartNo = '" + partno + "' and OperationNo != '" + opno + "' order by HMIID desc ) group by Work_Order_No,PartNo,OperationNo ";
                            // 2017-01-21
                            //string WIPQuery = @"SELECT * from tblhmiscreen where  HMIID IN ( SELECT Max(HMIID) from tblhmiscreen where  HMIID IN  ( SELECT HMIID from tblhmiscreen where Work_Order_No = '" + wono + "' and PartNo = '" + partno + "' and OperationNo != '" + opno + "' and IsMultiWO = 0 and DDLWokrCentre is null order by HMIID desc ) group by Work_Order_No,PartNo,OperationNo  ) order by OperationNo ;";
                            //var WIPOuter = db.tblhmiscreens.SqlQuery(WIPQuery).ToList();
                            //if (WIPOuter.Count == 0)
                            //{
                            //}
                            //else
                            //{
                            //    foreach (var row in WIPOuter)
                            //    {
                            //        int InnerOpNo = Convert.ToInt32(row.OperationNo);
                            //        if (InnerOpNo < OperationNoInt)
                            //        {
                            //            if (row.isWorkInProgress != 1) //=> lower OpNo is not JF 'ed.
                            //            {
                            //                Session["VError"] = " JobFinish WONo: " + row.Work_Order_No + " and PartNo: " + row.PartNo + " and OperationNo: " + InnerOpNo;
                            //                return RedirectToAction("Index");
                            //                //break;
                            //            }
                            //        }
                            //    }
                            //}
                            #endregion

                            int target = 0, delivered = 0, processed = 0;
                            int.TryParse(hmiidData.Target_Qty.ToString(), out target);
                            int.TryParse(hmiidData.Delivered_Qty.ToString(), out delivered);
                            int.TryParse(hmiidData.ProcessQty.ToString(), out processed);

                            int HMIId = hmiidData.HMIID;
                            //using (i_facility_tsalEntities dbsimilar = new i_facility_tsalEntities())
                            //{
                            #region If its as SingleWO
                            //var SimilarWOData = dbsimilar.tbllivehmiscreens.Where(m => m.Work_Order_No == hmiidData.Work_Order_No && m.OperationNo == hmiidData.OperationNo && m.PartNo == hmiidData.PartNo && m.MachineID != machineID && m.isWorkInProgress == 2).FirstOrDefault();
                            tbllivehmiscreen SimilarWOData = obj.GetLiHMIDetailsfirst(hmiidData.Work_Order_No, hmiidData.PartNo, hmiidData.OperationNo, machineID);
                            if (SimilarWOData != null)
                            {
                                //int InnerMacID = Convert.ToInt32(dbsimilar.tbllivehmiscreens.Where(m => m.HMIID == SimilarWOData.HMIID).Select(m => m.MachineID).FirstOrDefault());
                                int InnerMacID = Convert.ToInt32(obj.GetLiveHMIDetails10(HMIID));
                                //var MacDispName = Convert.ToString(dbsimilar.tblmachinedetails.Where(m => m.MachineID == InnerMacID).Select(m => m.MachineDispName).FirstOrDefault());
                                string MacDispName = obj.GetMachineDetails1(InnerMacID);
                                Session["Error"] = "Same WorkOrder is already in Machine: " + MacDispName + " , So you cannot JobFinish  ";
                                return RedirectToAction("Index");
                            }
                            #endregion

                            #region If its as MultiWO
                            //var SimilarWODataMulti = dbsimilar.tbllivemultiwoselections.Where(m => m.WorkOrder == hmiidData.Work_Order_No && m.OperationNo == hmiidData.OperationNo && m.PartNo == hmiidData.PartNo && m.HMIID != HMIId && m.tbllivehmiscreen.isWorkInProgress == 2).FirstOrDefault();
                            tbllivemultiwoselection SimilarWODataMulti = obj.GetMultiWODetails4(hmiidData.Work_Order_No, hmiidData.PartNo, hmiidData.OperationNo, HMIId);
                            if (SimilarWODataMulti != null)
                            {
                                int InnerHMIID = (int)SimilarWODataMulti.HMIID;
                                //int InnerMacID = Convert.ToInt32(dbsimilar.tbllivehmiscreens.Where(m => m.HMIID == InnerHMIID).Select(m => m.MachineID).FirstOrDefault());
                                int InnerMacID = Convert.ToInt32(obj.GetLiveHMIDetails10(InnerHMIID));
                                //var MacDispName = Convert.ToString(dbsimilar.tblmachinedetails.Where(m => m.MachineID == InnerMacID).Select(m => m.MachineDispName).FirstOrDefault());
                                string MacDispName = obj.GetMachineDetails1(InnerMacID);

                                Session["Error"] = "Same WorkOrder is already in Machine: " + MacDispName + " , So you cannot JobFinish  ";
                                return RedirectToAction("Index");
                            }
                            #endregion
                            //}

                            //var ddldata = db.tblddls.Where(m => m.WorkOrder == wono && m.MaterialDesc == partno && m.OperationNo == opno).FirstOrDefault();
                            tblddl ddldata = obj.GetddlDetails3(wono, partno, opno);
                            if (ddldata != null)
                            {
                                //ddldata.IsCompleted = 1;
                                //db.Entry(ddldata).State = System.Data.Entity.EntityState.Modified;
                                obj.UpdateddlDetails(ddldata.DDLID);
                            }

                            //hmiidData.Delivered_Qty = cjtextbox8;
                            //hmiidData.SplitWO = "No";
                            //hmiidData.isWorkInProgress = 1;
                            //hmiidData.Status = 2;
                            //hmiidData.Time = DateTime.Now;
                            //db.Entry(hmiidData).State = System.Data.Entity.EntityState.Modified;
                            //db.SaveChanges();
                            obj.UpdateLiveHMIScreeDetails(hmiidData.HMIID, cjtextbox8, "No", 1, 2, DateTime.Now);
                        }
                        else
                        {
                            Session["VError"] = " Delivered + Processed Qty Must be Equal to Target Qty";
                            return RedirectToAction("Index");
                        }
                    }
                    else
                    {
                        Session["VError"] = " Please Click Start Before Clicking Job Finish ";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    Session["VError"] = " Please End HOLD Before JobFinish";
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult EndHoldIndividual(int hmiiid)
        {
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);
            Session["split"] = null;
            int MacID = Convert.ToInt32(Session["MachineID"]);
            DateTime EndTime = DateTime.Now;
            //1) 1remove hold on current one 
            //var hmiData = db.tbllivehmiscreens.Where(m => m.HMIID == hmiiid).FirstOrDefault();
            tbllivehmiscreen hmiData = obj.GetLiveHMIDetails6(hmiiid);
            hmiData.IsHold = 0;
            //db.Entry(hmiData).State = System.Data.Entity.EntityState.Modified;
            //db.SaveChanges();
            obj.UpdateLiveHMIScreen1Detail(hmiData.HMIID, hmiData.IsHold);

            int OldHMIID = 0;
            //update the ishold and end time in old one.
            //var tblhmi = db.tbllivehmiscreens.Where(m => m.HMIID != hmiiid && m.Work_Order_No == hmiData.Work_Order_No && m.PartNo == hmiData.PartNo && m.OperationNo == hmiData.OperationNo).OrderByDescending(m => m.PEStartTime).FirstOrDefault();
            tbllivehmiscreen tblhmi = obj.GetLive3HMIDetails(hmiiid, hmiData.Work_Order_No, hmiData.PartNo, hmiData.OperationNo);
            if (tblhmi != null)
            {
                OldHMIID = tblhmi.HMIID;
                tblhmi.IsHold = 2;
                //db.Entry(tblhmi).State = System.Data.Entity.EntityState.Modified;
                //db.SaveChanges();
                obj.UpdateLiveHMIScreen1Detail(tblhmi.HMIID, tblhmi.IsHold);
                //3) update the EndDateTime column in manuallossofentry table.
                //var tblmanualLossData = db.tbllivemanuallossofentries.Where(m => m.HMIID == OldHMIID).OrderByDescending(m => m.StartDateTime).FirstOrDefault();
                tblmanuallossofentry tblmanualLossData = obj.GetManualLossofEntryDetails(OldHMIID);
                if (tblmanualLossData != null)
                {
                    tblmanualLossData.EndHMIID = hmiiid;
                    tblmanualLossData.EndDateTime = EndTime;
                    //db.Entry(tblmanualLossData).State = System.Data.Entity.EntityState.Modified;
                    //db.SaveChanges();
                    obj.UpdateManualLossofEntryDetails(Convert.ToInt32(tblmanualLossData.HMIID), Convert.ToInt32(tblmanualLossData.EndHMIID), Convert.ToDateTime(tblmanualLossData.EndDateTime));
                }
            }
            return RedirectToAction("Index");
        }

        //public bool IDLEorGenericWorkisON()
        //{
        //    //Check if the Machine is in IDLE or GenericWork , We shouldn't allow them to do other activities if these are ON.
        //    //bool ItsOn = false;
        //    _conn = new ConnectionFactory();
        //    obj = new Dao(_conn);
        //    int MacID = Convert.ToInt32(Session["MachineID"]);
        //    var GWToView = obj.GetgenericworkDetails(MacID);
        //    //var GWToView = db.tblgenericworkentries.Where(m => m.MachineID == MacID).OrderByDescending(m => m.GWEntryID).FirstOrDefault();
        //    if (GWToView != null) //implies genericwork is running
        //    {
        //        if (GWToView.DoneWithRow == 0)
        //        {
        //            //ItsOn = true;
        //            return true;
        //        }
        //    }
        //    var IdleToView = obj.GetLossOfEntryDetails4(MacID);
        //    //var IdleToView = db.tbllivelossofentries.Where(m => m.MachineID == MacID).OrderByDescending(m => m.LossID).FirstOrDefault();
        //    if (IdleToView != null) //implies idle is running
        //    {
        //        if (IdleToView.DoneWithRow == 0)
        //        {
        //            //ItsOn = true;
        //            return true;
        //        }
        //    }

        //    return false;
        //}

        #region commented by monika
        //[HttpPost]
        //public int GetMaxbatchNo(string values)
        //{
        //    _conn = new ConnectionFactory();
        //    obj = new Dao(_conn);
        //    int? maxBatchNo = 0;

        //    string[] ids = values.Split(',');
        //    List<tbllivehmiscreen> batchcount = new List<tbllivehmiscreen>();
        //    foreach (var item in ids)
        //    {
        //        int hmid = Convert.ToInt32(item);
        //        var check = obj.GettbllivehmiscreensDet5(hmid);
        //        // var check = db.tbllivehmiscreens.Where(m => m.HMIID == hmid && m.Work_Order_No != null).FirstOrDefault();
        //        if (check != null)
        //        {
        //            batchcount.Add(check);
        //        }
        //    }
        //    maxBatchNo = Convert.ToInt32(obj.GetMaxbatchNoDet4());
        //    // maxBatchNo =Convert.ToInt32( db.tbllivehmiscreens.Max(p => p.batchNo));
        //    if (maxBatchNo == 0)
        //    {
        //        maxBatchNo = 1;
        //    }
        //    else
        //    {
        //        maxBatchNo += 1;
        //    }
        //    foreach (var item in ids)
        //    {
        //        int hmid = Convert.ToInt32(item);
        //        var check = obj.GettbllivehmiscreensDet5(hmid);
        //        if (check != null)
        //        {
        //            int batchCount = batchcount.Count();
        //            var hmIncrease = obj.GetHMIDet5(hmid, maxBatchNo);
        //            // var hmIncrease = db.tbllivehmiscreens.Where(m => m.HMIID == hmid).FirstOrDefault();
        //            //hmIncrease.batchNo = maxBatchNo;
        //            //db.SaveChanges();
        //        }
        //    }
        //    return Convert.ToInt32(maxBatchNo);

        //}

        #endregion

        #region Newocde for getbachno

        //public JsonResult GetMaxbatchNo(string values)
        //{
        //    string res = "";
        //    _conn = new ConnectionFactory();
        //    obj = new Dao(_conn);
        //    int? maxBatchNo = 0;

        //    string[] ids = values.Split(',');
        //    List<tbllivehmiscreen> batchcount = new List<tbllivehmiscreen>();
        //    foreach (var item in ids)
        //    {
        //        int hmid = Convert.ToInt32(item);
        //        var check = obj.GettbllivehmiscreensDet5(hmid);
        //        // var check = db.tbllivehmiscreens.Where(m => m.HMIID == hmid && m.Work_Order_No != null).FirstOrDefault();
        //        if (check != null)
        //        {
        //            batchcount.Add(check);
        //        }
        //    }
        //    maxBatchNo = Convert.ToInt32(obj.GetMaxbatchNoDet4());
        //    // maxBatchNo =Convert.ToInt32( db.tbllivehmiscreens.Max(p => p.batchNo));
        //    if (maxBatchNo == 0)
        //    {
        //        maxBatchNo = 1;
        //    }
        //    else
        //    {
        //        maxBatchNo += 1;
        //    }
        //    foreach (var item in ids)
        //    {
        //        int hmid = Convert.ToInt32(item);
        //        var check = obj.GettbllivehmiscreensDet5(hmid);
        //        if (check != null)
        //        {
        //            int batchCount = batchcount.Count();
        //            DateTime st = DateTime.Now;
        //            var hmIncrease = obj.GetHMIDet5(hmid, maxBatchNo, st, 1, 0, 0);
        //            // var hmIncrease = db.tbllivehmiscreens.Where(m => m.HMIID == hmid).FirstOrDefault();
        //            //hmIncrease.batchNo = maxBatchNo;
        //            //db.SaveChanges();
        //        }
        //        else
        //        {
        //            var hmIncrease = obj.GetHMIDetDel(hmid);
        //        }
        //    }
        //    Session["Started"] = 0;
        //    res = maxBatchNo.ToString();
        //    return Json(res, JsonRequestBehavior.AllowGet);

        //}

        public ActionResult GetMaxbatchNo(string values)
        {
            Session["Mode"] = null;
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);
            int? maxBatchNo = 0;

            string[] ids = values.Split(',');
            List<tbllivehmiscreen> batchcount = new List<tbllivehmiscreen>();
            foreach (string item in ids)
            {
                int hmid = Convert.ToInt32(item);
                tbllivehmiscreen check = obj.GettbllivehmiscreensDet5(hmid);
                // var check = db.tbllivehmiscreens.Where(m => m.HMIID == hmid && m.Work_Order_No != null).FirstOrDefault();
                if (check != null)
                {
                    batchcount.Add(check);
                }
            }
            maxBatchNo = Convert.ToInt32(obj.GetMaxbatchNoDet4());
            // maxBatchNo =Convert.ToInt32( db.tbllivehmiscreens.Max(p => p.batchNo));
            if (maxBatchNo == 0)
            {
                maxBatchNo = 1;
            }
            else
            {
                maxBatchNo += 1;
            }

            Session["ERRORMulti"] = null;
            foreach (string item in ids)
            {
                int hmid = Convert.ToInt32(item);
                tbllivehmiscreen check = obj.GettbllivehmiscreensDet5(hmid);

                if (check != null)
                {
                    IndividualSubmit(hmid);

                    if (Session["VError"] == null)
                    {
                        //while running the Multiple WO Getting validation error so we added these update functionality here on 2020-05-25
                        int batchCount = batchcount.Count();
                        DateTime st = DateTime.Now;
                        int hmIncrease = obj.GetHMIDet5(hmid, maxBatchNo, st, 1, 0, 0);
                    }

                    else
                    {
                        Session["ERRORMulti"] = Session["VError"];

                        Session["Error"] = Session["VError"];
                        Session["VError"] = "";
                    }

                    // var hmIncrease = db.tbllivehmiscreens.Where(m => m.HMIID == hmid).FirstOrDefault();
                    //hmIncrease.batchNo = maxBatchNo;
                    //db.SaveChanges();
                }
                else
                {
                    int hmIncrease = obj.GetHMIDetDel(hmid);
                }

            }

            foreach (string item in ids)
            {
                int hmid = Convert.ToInt32(item);
                tbllivehmiscreen check = obj.GettbllivehmiscreensDet5(hmid);

                string error = Convert.ToString(Session["ERRORMulti"]);
                if (check != null)
                {
                    if (Session["ERRORMulti"] == null)
                    {
                        int batchCount = batchcount.Count();
                        DateTime st = DateTime.Now;
                        int hmIncrease = obj.GetHMIDet5(hmid, maxBatchNo, st, 1, 0, 0);

                    }
                }
            }
            Session["Started"] = 1;
            Session["WorkOrderClicked"] = 1;
            return RedirectToAction("Index");

        }
        #endregion

        public ActionResult InsertNewEmptyRow(int id, string opno, string shift, int optId)
        {
            //tbllivehmiscreen obj = new tbllivehmiscreen();
            ////var check = db.tbllivehmiscreens.Where(m => m.MachineID == id && m.OperatorDet == opno && m.Shift == shift).ToList();
            ////if (check != null)

            //obj.MachineID = id;
            //obj.OperatiorID = optId;
            //obj.Shift = shift;
            //obj.Date = null;
            //obj.Time = null;
            //obj.Project = null;
            //obj.PartNo = null;
            //obj.OperationNo = null;
            //obj.Rej_Qty = null;
            //obj.Work_Order_No = null;
            //obj.Target_Qty = null;
            //obj.Delivered_Qty = null;
            //obj.Status = 0;
            //obj.CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            //obj.Prod_FAI = null;
            //obj.isUpdate = 0;
            //obj.DoneWithRow = 0;
            //obj.isWorkInProgress = 2;
            //obj.isWorkOrder = 0;
            //obj.OperatorDet = opno;
            //obj.PEStartTime = System.DateTime.Now;
            //obj.ProcessQty = 0;
            //obj.DDLWokrCentre = null;
            //obj.IsMultiWO = 0;
            //obj.IsHold = 0;
            //obj.SplitWO = null;
            //obj.batchCount = 0;
            //obj.batchNo = 0;
            //    db.tbllivehmiscreens.Add(obj);
            //    db.SaveChanges();
            obj.InsertHMI4(id, optId, shift, DateTime.Now.ToString("yyyy-MM-dd"), opno, System.DateTime.Now);
            return RedirectToAction("Index");
        }


        public JsonResult checkPassword(string OperatorID, string password)
        {
            string ValidOp = "false";
            _conn = new ConnectionFactory();
            obj = new Dao(_conn);
            using (SRKSDemo.Server_Model.i_facility_shaktiEntities1 db = new SRKSDemo.Server_Model.i_facility_shaktiEntities1())
            {
                Server_Model.tbluser OPDet = db.tblusers.Where(m => m.IsDeleted == 0 && m.UserName == OperatorID && m.Password == password).FirstOrDefault();
                if (OPDet != null)
                {
                    ValidOp = "true";
                    AutoSaveOPName("cjtextboxop", OperatorID);
                    ViewBag.hide = 1;
                    return Json(ValidOp, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(ValidOp, JsonRequestBehavior.AllowGet);
        }

        // Machine lock if there is no WorkOrder Entry by the operarator
        //private bool checkMachinelocking(int machid)
        //{
        //    bool res = false;
        //    tblmachinedetail machinedetails = obj.GetMachineDet(machid);
        //    string IPAddress = "";
        //    int MachineUnlockbit = 0;
        //    int machineLockbit = 0;
        //    ushort h = 0;
        //    short ret = 1;
        //    if (machinedetails != null && machinedetails.EnableLockLogic==1)
        //    {
        //        IPAddress = machinedetails.IPAddress;
        //        //MachineUnlockbit = Convert.ToInt32(machinedetails.MachineUnlockBit);
        //        machineLockbit = Convert.ToInt32(machinedetails.MachineLockBit);
        //        VirtualHMI vmi = new VirtualHMI(IPAddress, out h, out ret);
        //    }
        //    IntoFile("Machine connection ret" + ret + " IPADdress : " + IPAddress);
        //    if (ret == 0)
        //    {
        //        setmachinelock(h, (ushort)machineLockbit, true);
        //        IntoFile("MachineID Locked  " + machid);
        //    }
        //    return res;
        //}

        private string getcorrectedDate()
        {
            string CorrectedDate = null;
            //tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
            tbldaytiming StartTime = obj.GetDaytimingDetails();
            TimeSpan Start = StartTime.StartTime;
            if (Start <= DateTime.Now.TimeOfDay)
            {
                CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }
            return CorrectedDate;
        }

        //New UnLocking Logic for Shakti - 2020-08-30 Pavan V To be Used
        private void setmachineUnlock(ushort h, ushort LockDBit, bool LockStatus)
        {
            //if (!LockStatus)
            {
                //Locking D Bit Parameters
                Focas1.IODBPMC0 rdpmcdataLockBit = new Focas1.IODBPMC0();
                short adr_type = 9;
                short data_type = 0;
                ushort s_number = LockDBit; //8010
                ushort e_number = LockDBit; //8010
                ushort length = 9;
                short rdretLock = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_number, e_number, length, rdpmcdataLockBit);
                //Machine LOCK D Bit
                Focas1.IODBPMC0 wrpmcdata = rdpmcdataLockBit;
                wrpmcdata.cdata[0] = 0;
                for (int i = 0; i < 10; i++)
                {
                    short wrretLOCK = Focas1.pmc_wrpmcrng(h, length, wrpmcdata);
                }

                IntoFile("Lockbit: " + LockDBit + " \t handler :" + h);
            }

        }

        //New Locking Logic for Shakti - 2020-08-30 Pavan V To be Used
        private void setmachinelock(ushort h, ushort LockDBit, bool LockStatus)
        {
            if (LockStatus)
            {
                //Locking D Bit Parameters
                Focas1.IODBPMC0 rdpmcdataLockBit = new Focas1.IODBPMC0();
                short adr_type = 9;
                short data_type = 0;
                ushort s_number = LockDBit; //8010
                ushort e_number = LockDBit; //8010
                ushort length = 9;
                short rdretLock = Focas1.pmc_rdpmcrng(h, adr_type, data_type, s_number, e_number, length, rdpmcdataLockBit);
                //Machine LOCK D Bit
                Focas1.IODBPMC0 wrpmcdata = rdpmcdataLockBit;
                wrpmcdata.cdata[0] = 1;
                for (int i = 0; i < 10; i++)
                {
                    short wrretLOCK = Focas1.pmc_wrpmcrng(h, length, wrpmcdata);
                }
            }
        }


        public void INSERTMODE(string MacMode, int MacID, int InsertedBy, DateTime InsertedOn, DateTime correctedDate, DateTime StartTimeNext, string ColoCode, string ModeType,/*bool IsShift,*/int shift)
        {
            try
            {
                SRKSDemo.Server_Model.tbllivemode row = new SRKSDemo.Server_Model.tbllivemode();
                row.MacMode = MacMode;
                row.MachineID = MacID;
                row.InsertedBy = InsertedBy;
                row.InsertedOn = InsertedOn;
                row.CorrectedDate = correctedDate;
                row.StartTime = StartTimeNext;
                row.ColorCode = ColoCode;
                row.ModeType = ModeType;
                row.IsShiftEnd = shift;
                //if (shift == 1 && IsShift == true)
                //{
                //    row.IsShiftEnd = 1;
                //}
                using (Server_Model.i_facility_shaktiEntities1 db1 = new Server_Model.i_facility_shaktiEntities1())
                {
                    db1.tbllivemodes.Add(row);
                    db1.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
        }

        private void UpdatetbllivemodeDetails(DateTime nowdate, int durationinsec, int StartIDLE, int ModeID)
        {
            try
            {

                SRKSDemo.Server_Model.tbllivemode rowupdate = new SRKSDemo.Server_Model.tbllivemode();
                using (Server_Model.i_facility_shaktiEntities1 db = new Server_Model.i_facility_shaktiEntities1())
                {
                    rowupdate = db.tbllivemodes.Find(ModeID);
                }

                DateTime startDateTime = Convert.ToDateTime(rowupdate.StartTime);


                rowupdate.EndTime = nowdate;
                rowupdate.DurationInSec = durationinsec;
                rowupdate.IsCompleted = 1;
                rowupdate.ModeTypeEnd = 1;
                //rowupdate.CuttingDuration = cuttingTime;
                rowupdate.StartIdle = StartIDLE;

                using (Server_Model.i_facility_shaktiEntities1 db1 = new Server_Model.i_facility_shaktiEntities1())
                {
                    db1.Entry(rowupdate).State = System.Data.Entity.EntityState.Modified;
                    db1.SaveChanges();
                    IntoFile("Update Mode:" + ModeID);

                }
                try
                {
                    CalculateCuttingTime(startDateTime, nowdate, rowupdate.MachineID, ModeID);
                }
                catch (Exception ex)
                {
                    IntoFile(ex.ToString());
                }
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
        }

        private int CalculateCuttingTime(DateTime startDateTime, DateTime endDatetime, int machineId, int modeId)
        {
            int cuttingTime = 0;
            try
            {
                IntoFile("Before Calculating cutting");
                using (Server_Model.i_facility_shaktiEntities1 db = new Server_Model.i_facility_shaktiEntities1())
                {
                    SRKSDemo.Server_Model.parameters_master LastRow = db.parameters_master.Where(m => m.MachineID == machineId && m.InsertedOn >= startDateTime && m.InsertedOn <= endDatetime).OrderByDescending(m => m.ParameterID).FirstOrDefault();
                    SRKSDemo.Server_Model.parameters_master FirstRow = db.parameters_master.Where(m => m.MachineID == machineId && m.InsertedOn >= startDateTime && m.InsertedOn <= endDatetime).OrderBy(m => m.ParameterID).FirstOrDefault();
                    if (LastRow != null && FirstRow != null)
                    {
                        cuttingTime = Convert.ToInt32(LastRow.CuttingTime) - Convert.ToInt32(FirstRow.CuttingTime);
                    }
                }

                using (Server_Model.i_facility_shaktiEntities1 db = new Server_Model.i_facility_shaktiEntities1())
                {
                    SRKSDemo.Server_Model.tbllivemode updateRow = db.tbllivemodes.Find(modeId);
                    if (updateRow != null)
                    {
                        updateRow.CuttingDuration = cuttingTime;
                        db.SaveChanges();
                    }

                    IntoFile("after Calculating cutting");
                }
            }
            catch (Exception ex)
            {
                IntoFile("Cutting Time: " + ex.ToString());
            }


            return cuttingTime;
        }

        public int GetShift()
        {
            string CorrectedDate = getcorrectedDate();
            int shift = 0;
            List<SRKSDemo.Server_Model.shift_master> msgs2 = new List<SRKSDemo.Server_Model.shift_master>();
            using (Server_Model.i_facility_shaktiEntities1 db1 = new Server_Model.i_facility_shaktiEntities1())
            {
                msgs2 = db1.shift_master.Where(m => m.IsDeleted == 0).ToList();
            }
            String[] msgtime = DateTime.Now.ToString("HH:mm:ss").Split(':');
            TimeSpan msgstime = new TimeSpan(Convert.ToInt32(msgtime[0]), Convert.ToInt32(msgtime[1]), Convert.ToInt32(msgtime[2]));
            TimeSpan s1t1 = new TimeSpan(0, 0, 0), s1t2 = new TimeSpan(0, 0, 0);
            TimeSpan s2t1 = new TimeSpan(0, 0, 0), s2t2 = new TimeSpan(0, 0, 0);
            TimeSpan s3t1 = new TimeSpan(0, 0, 0), s3t2 = new TimeSpan(0, 0, 0), s3t3 = new TimeSpan(0, 0, 0), s3t4 = new TimeSpan(23, 59, 59);
            for (int j = 0; j < msgs2.Count; j++)
            {
                if (msgs2[j].ShiftName.ToString().Contains("1") || msgs2[j].ShiftName.ToString().Contains("A"))
                {
                    String[] s1 = msgs2[j].StartTime.ToString().Split(':');
                    s1t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
                    String[] s11 = msgs2[j].EndTime.ToString().Split(':');
                    s1t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
                }
                if (msgs2[j].ShiftName.ToString().Contains("2") || msgs2[j].ShiftName.ToString().Contains("B"))
                {
                    String[] s1 = msgs2[j].StartTime.ToString().Split(':');
                    s2t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
                    String[] s11 = msgs2[j].EndTime.ToString().Split(':');
                    s2t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
                }
                if (msgs2[j].ShiftName.ToString().Contains("3") || msgs2[j].ShiftName.ToString().Contains("C"))
                {
                    String[] s1 = msgs2[j].StartTime.ToString().Split(':');
                    s3t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
                    String[] s11 = msgs2[j].EndTime.ToString().Split(':');
                    s3t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
                }
            }

            if (msgstime >= s1t1 && msgstime < s1t2)
            {
                shift = 1;
            }
            else if (msgstime >= s2t1 && msgstime < s2t2)
            {
                shift = 2;
            }
            else if ((msgstime >= s3t1 && msgstime <= s3t4) || (msgstime >= s3t3 && msgstime < s3t2))
            {
                shift = 3;
                //if (msgstime >= s3t3 && msgstime < s3t2)
                //{
                //    CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                //}
            }


            return shift;

        }

        private void InsertModeafterLock(int machid, string CorrectedDate, DateTime LossStartTime)
        {
            try
            {
                SRKSDemo.Server_Model.tbllivemode ModeDetList = new SRKSDemo.Server_Model.tbllivemode();
                using (Server_Model.i_facility_shaktiEntities1 db1 = new Server_Model.i_facility_shaktiEntities1())
                {
                    ModeDetList = db1.tbllivemodes.Where(m => m.MachineID == machid && m.IsCompleted == 0 && m.ColorCode == "YELLOW").OrderByDescending(m => m.ModeID).FirstOrDefault();
                }

                if (ModeDetList != null)
                {
                    int ModeID = Convert.ToInt32(ModeDetList.ModeID.ToString());
                    String StartTime = LossStartTime.ToString();
                    DateTime nowdate = DateTime.Now;

                    int shiftid = GetShift();
                    int durationinsec = Convert.ToInt32(nowdate.Subtract(Convert.ToDateTime(ModeDetList.StartTime)).TotalSeconds);
                    UpdatetbllivemodeDetails(nowdate, durationinsec, 0, ModeID);
                    INSERTMODE("IDLE", machid, 3, System.DateTime.Now, Convert.ToDateTime(CorrectedDate), LossStartTime, "YELLOW", "IDLE", shiftid);
                }
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
        }

        private void UpdatetbllivemodeDetails(int StartIDLE, int ModeID)
        {
            try
            {

                SRKSDemo.Server_Model.tbllivemode rowupdate = new SRKSDemo.Server_Model.tbllivemode();
                using (Server_Model.i_facility_shaktiEntities1 db = new Server_Model.i_facility_shaktiEntities1())
                {
                    rowupdate = db.tbllivemodes.Find(ModeID);
                }

                rowupdate.StartIdle = StartIDLE;

                using (Server_Model.i_facility_shaktiEntities1 db1 = new Server_Model.i_facility_shaktiEntities1())
                {
                    db1.Entry(rowupdate).State = System.Data.Entity.EntityState.Modified;
                    db1.SaveChanges();
                    IntoFile("Update start IDLE as 0 Mode:" + ModeID);
                }
            }
            catch (Exception ex)
            {
                IntoFile(ex.ToString());
            }
        }

        public class Hmi
        {
            public string OperationNo { get; set; }
            public bool status { get; set; }
        }
    }
}