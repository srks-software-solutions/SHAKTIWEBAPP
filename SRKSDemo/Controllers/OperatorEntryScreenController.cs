//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using SRKSDemo.Models;
//using System.Data;
////using MySql.Data.MySqlClient;
//using System.Data.SqlClient;
//using SRKSDemo.ServerModel;

//namespace SRKSDemo.Controllers
//{
//    public class OperatorEntryScreenController : Controller
//    {
//        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
//        #region Index get
//        public ActionResult Index(int id = 0)
//        {
//            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
//            {
//                return RedirectToAction("Login", "Login", null);
//            }
//            var alsdkfj = Session["isWorkOrder"];

//            //Session["FromDDL"] = 0;
//            ViewBag.Logout = Session["Username"];
//            ViewBag.roleid = Session["RoleID"];
//            int opid = Convert.ToInt32(Session["UserId"]);
//            int ShiftID = 0;

//            //Testing Login Status
//            var aj = User.Identity.IsAuthenticated;
//            var jb = Request.IsAuthenticated;
//            bool val1 = System.Web.HttpContext.Current.User.Identity.IsAuthenticated‌;
//            var c = User.Identity.Name;
//            var d = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
//            var e = System.Web.HttpContext.Current.User;

//            TempData["saveORUpdate"] = null;

//            //Getting Shift Value
//            #region
//            DateTime Time = DateTime.Now;
//            TimeSpan Tm = new TimeSpan(Time.Hour, Time.Minute, Time.Second);
//            var ShiftDetails = db.tblshift_mstr.Where(m => m.StartTime <= Tm && m.EndTime >= Tm);
//            string Shift = null;
//            foreach (var a in ShiftDetails)
//            {
//                Shift = a.ShiftName;
//            }
//            ViewBag.date = System.DateTime.Now;
//            if (Shift != null)
//            {
//                ViewBag.shift = Shift;
//                Session["shift"] = Shift;
//            }
//            else
//            {
//                ViewBag.shift = "C";
//                Session["shift"] = "C";
//                Shift = "C";
//            }
//            Session["realshift"] = Shift;
//            if (Shift == "A")
//                ShiftID = 1;
//            else if (Shift == "B")
//                ShiftID = 2;
//            else
//                ShiftID = 3;
//            #endregion

//            //Code For Admin And Super Admin
//            int RoleID = Convert.ToInt32(Session["RoleID"]);
//            if (RoleID == 1 || RoleID == 2)
//            {
//                return RedirectToAction("SelectMachine", "HMIScree", null);
//            }
//            ViewBag.roleid = Session["RoleID"];

//            //code to get CorrectedDate
//            #region
//            string CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
//            if (DateTime.Now.Hour < 6 && DateTime.Now.Hour >= 0)
//            {
//                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
//            }

//            #endregion

//            int Machinid = Convert.ToInt32(Session["MachineID"]);

//            //Checking operator machine is allocated or not
//            //var machineallocation = db.tblmachineallocations.Where(m => m.IsDeleted == 0 && m.CorrectedDate == CorrectedDate && m.UserID == opid && m.ShiftID == ShiftID);
//            //if (machineallocation.Count() != 0)
//            //{
//            //    foreach (var a in machineallocation)
//            //    {
//            //        Machinid = Convert.ToInt32(a.MachineID);
//            //        Session["MachineID"] = Machinid;
//            //    }
//            //}

//            //insert a new row if there is no row for this machine for this date.
//            // tblhmiscreen HMI = db.tblhmiscreens.Where(m => m.CorrectedDate == CorrectedDate && m.OperatiorID == opid).Where(m => m.MachineID == Machinid && m.Status == 0).FirstOrDefault();
//            //tblhmiscreen HMI = db.tblhmiscreens.Where(m => m.MachineID == Machinid && m.Status == 0).OrderByDescending(m => m.HMIID).FirstOrDefault();

//            var HMI = db.tblworkorderentries.Where(m => m.MachineID == Machinid && m.Status == 0).OrderByDescending(m => m.HMIID).ToList();
//            if (HMI.Count == 0)
//            {
//                var HMIOpName = db.tblworkorderentries.Where(m => m.MachineID == Machinid && m.Status == 0).OrderByDescending(m => m.HMIID).FirstOrDefault();
//                if (HMIOpName != null)
//                {
//                    Session["OpName"] = HMIOpName.OperatorID;
//                }

//                tblworkorderentry tblhmiscreen = new tblworkorderentry();
//                tblhmiscreen.MachineID = Machinid;
//                tblhmiscreen.CorrectedDate = CorrectedDate;
//                tblhmiscreen.PEStartTime = DateTime.Now;
//                tblhmiscreen.OperatorID = Convert.ToString(Session["OpName"]);
//                tblhmiscreen.Shift = Convert.ToString(ViewBag.shift);
//                tblhmiscreen.Status = 0;
//                int ishideInt = 0;
//                int.TryParse(Convert.ToString(Session["isHide"]), out ishideInt);
//                if (ishideInt == 1)
//                {
//                    //tblhmiscreen.isUpdate = 1;
//                }
//                //tblhmiscreen.isWorkInProgress = 2;
//                //tblhmiscreen.isWorkOrder = Convert.ToInt32(Session["isWorkOrder"]);
//                //tblhmiscreen.OperatorID = Convert.ToInt32(Session["UserId"]);
//                db.tblworkorderentries.Add(tblhmiscreen);
//                db.SaveChanges();

//                Session["FromDDL"] = 0;
//            }
//            else
//            {
//                //If there is no nonsubmited row then insert new
//                var HMIOpName = db.tblworkorderentries.Where(m => m.MachineID == Machinid && m.Status == 0).OrderByDescending(m => m.HMIID).FirstOrDefault();
//                if (HMIOpName != null)
//                {
//                    Session["OpName"] = HMIOpName.OperatorID;
//                    //Shift will be stale one, dont take it from here.
//                }

//                bool isEmptyAvailable = false; //1
//                foreach (var row in HMI)
//                {
//                    //if (row.isUpdate == 0 && row.Date == null)
//                    if (row.WOStart == null)
//                    {
//                        isEmptyAvailable = true;//0
//                        break;
//                    }
//                }

//                if (!isEmptyAvailable)
//                {
//                    tblworkorderentry tblhmiscreen = new tblworkorderentry();
//                    tblhmiscreen.MachineID = Machinid;
//                    tblhmiscreen.CorrectedDate = CorrectedDate;
//                    tblhmiscreen.PEStartTime = DateTime.Now;
//                    tblhmiscreen.OperatorID = Convert.ToString(Session["OpName"]);
//                    tblhmiscreen.Shift = Convert.ToString(ViewBag.shift);
//                    tblhmiscreen.Status = 0;

//                    int ishideInt = 0;
//                    int.TryParse(Convert.ToString(Session["isHide"]), out ishideInt);
//                    if (ishideInt == 1)
//                    {
//                        //tblhmiscreen.isUpdate = 1;
//                    }

//                    //tblhmiscreen.isWorkInProgress = 2;
//                    //tblhmiscreen.isWorkOrder = Convert.ToInt32(Session["isWorkOrder"]);
//                    //tblhmiscreen.OperatiorID = Convert.ToInt32(Session["UserId"]);

//                    db.tblworkorderentries.Add(tblhmiscreen);
//                    db.SaveChanges();
//                }

//                //Below code may be required based on how we handle WO's 2016-12-28
//                foreach (var row in HMI)
//                {
//                    if (row.CorrectedDate != CorrectedDate && row.WOStart == null)
//                    {
//                        row.Shift = Convert.ToString(ViewBag.shift);
//                        row.CorrectedDate = CorrectedDate;
//                        row.PEStartTime = DateTime.Now;
//                        db.Entry(row).State = System.Data.Entity.EntityState.Modified;
//                        db.SaveChanges();
//                    }
//                }
//            }

//            ViewBag.DATE = DateTime.Now.Date;
//            List<SelectListItem> Prod_FAI = new List<SelectListItem>();

//            ViewBag.machineID = Convert.ToInt32(Session["MachineID"]);
//            int macid = Convert.ToInt32(Session["MachineID"]);

//            ViewBag.MacDispName = db.tblmachinedetails.Where(m => m.MachineID == macid).Select(m => m.MachineDisplayName).FirstOrDefault();
//            var MachineName = db.tblmachinedetails.Where(m => m.MachineID == macid).Select(m => m.MachineDisplayName).FirstOrDefault();

//            tbluser tbl = db.tblusers.Find(opid);
//            ViewBag.operatordisplay = tbl.DisplayName;
//            if (Session["VError"] != null)
//            {

//            }
//            if (TempData["Err"] != null)
//            {

//            }

//            //Code to resume particular workOrder . Probabily ...... Never 
//            //if (id != 0)
//            //{
//            //    var resumeWorkOrder = db.tblworkorderentries.Where(m => m.MachineID == Machinid && m.OperatiorID == opid && (m.HMIID == id)).Where(m => m.Status != 2).Where(m => m.CorrectedDate == CorrectedDate).Take(1).ToList();
//            //    //ViewBag.shiftshivu = new SelectList(db.tblhmiscreens.Where(m => m.MachineID == Machinid && m.OperatiorID == opid && (m.HMIID == id)).Where(m => m.Status != 2).Where(m => m.CorrectedDate == CorrectedDate), "Shift", "Shift");
//            //    #region commmented
//            //    //if (resumeWorkOrder.Count == 1)
//            //    //{
//            //    //    int extrarowid = 0;
//            //    //    var resumeworkExtraRow = db.tblhmiscreens.Where(m => m.MachineID == Machinid && m.OperatiorID == opid).OrderByDescending(m => m.HMIID).FirstOrDefault();
//            //    //    if (resumeworkExtraRow != null)
//            //    //    {
//            //    //        extrarowid = resumeworkExtraRow.HMIID;
//            //    //    }
//            //    //    resumeWorkOrder = db.tblhmiscreens.Where(m => m.MachineID == Machinid && m.OperatiorID == opid && (m.HMIID == id || m.HMIID == extrarowid)).Where(m => m.CorrectedDate == CorrectedDate).ToList();
//            //    //}
//            //    //else
//            //    //{
//            //    //    int extrarowid = 0;
//            //    //    var resumeworkExtraRow = db.tblhmiscreens.Where(m => m.MachineID == Machinid && m.OperatiorID == opid).OrderByDescending(m => m.HMIID).FirstOrDefault();
//            //    //    if (resumeworkExtraRow != null)
//            //    //    {
//            //    //        extrarowid = resumeworkExtraRow.HMIID;
//            //    //    }
//            //    //    resumeWorkOrder = db.tblhmiscreens.Where(m => m.MachineID == Machinid && m.OperatiorID == opid && (m.HMIID == id || m.HMIID == extrarowid)).Where(m => m.CorrectedDate == CorrectedDate).ToList();
//            //    //}
//            //    //if (resumeWorkOrder[0].Status == resumeWorkOrder[1].Status)
//            //    //{
//            //    //    resumeWorkOrder[0].Status = 0;

//            //    //}
//            //    #endregion

//            //    if (resumeWorkOrder.Count == 0)
//            //    {
//            //        resumeWorkOrder = db.tblworkorderentries.Where(m => m.MachineID == Machinid && m.OperatiorID == opid && (m.HMIID == id)).Where(m => m.CorrectedDate == CorrectedDate).ToList();
//            //        //ViewBag.shiftshivu = new SelectList(db.tblhmiscreens.Where(m => m.MachineID == Machinid && m.OperatiorID == opid && (m.HMIID == id)).Where(m => m.CorrectedDate == CorrectedDate), "Shift", "Shift");
//            //    }

//            //    var j = ViewBag.shiftshivu;
//            //    return View(resumeWorkOrder);
//            //}

//            #region If data not selected.

//            //var data = db.tblhmiscreens.Where(m => m.MachineID == Machinid && m.OperatiorID == opid).Where(m => m.Status != 2).Where(m => m.CorrectedDate == CorrectedDate).Where(m => m.Shift == gshift).ToList();

//            //I am removing the CorrectedDate from condition so 2016-12-30 becarefull about what it retrieves(stale data)
//            var data = db.tblworkorderentries.Where(m => m.MachineID == Machinid && m.OperatorID == Convert.ToString(opid)).Where(m => m.Status == 0).OrderByDescending(m => m.HMIID).ToList();
//            if (data.Count > 0)
//            {
//                int sneha = Convert.ToInt32(TempData["ForDDL2"]);
//                //Default to Handle Manual/ScanEntry
//                if (Convert.ToInt32(TempData["ForDDL2"]) == 2)
//                {
//                    Session["FromDDL"] = 2;
//                    Session["SubmitClicked"] = 0;
//                }

//                int fromDDLInt = 6; //i am fake initializing to 6 , because i haven't used 6 anywhere.
//                string blah = Convert.ToString(Session["FromDDL"]);
//                int.TryParse(Convert.ToString(Session["FromDDL"]), out fromDDLInt);

//                if (!string.IsNullOrEmpty(blah.Trim())) // Implies Session is Alive. Continous
//                {
//                    #region
//                    if (fromDDLInt == 4) //implies that its a MultiWO & From DDL
//                    {
//                        if (data[0].WOStart == null) //Before Submit
//                        {
//                            Session["FromDDL"] = 4;
//                            Session["SubmitClicked"] = 0;
//                        }
//                        else //After Submit
//                        {
//                            Session["FromDDL"] = 4;
//                            Session["SubmitClicked"] = 1;
//                        }
//                    }
//                    else if (fromDDLInt == 1)
//                    {
//                        if (data[0].WOStart == null) //Before Submit
//                        {
//                            Session["FromDDL"] = 1;
//                            Session["SubmitClicked"] = 0;
//                        }

//                        else //After Submit
//                        {
//                            Session["FromDDL"] = 1;
//                            Session["SubmitClicked"] = 1;
//                        }
//                    }
//                    else if (fromDDLInt == 2) // Manual/ScanEntry
//                    {
//                        if (data[0].WOStart == null) //Before Submit
//                        {
//                            Session["FromDDL"] = 2;
//                            Session["SubmitClicked"] = 0;
//                        }
//                        else //After Submit
//                        {
//                            Session["FromDDL"] = 0;
//                            Session["SubmitClicked"] = 1;
//                        }
//                    }
//                    else if (fromDDLInt == 0)
//                    {
//                        if (data[0].WOStart == null) //Before Submit
//                        {
//                            Session["FromDDL"] = 0;
//                            Session["SubmitClicked"] = 0;
//                        }
//                        else //After Submit
//                        {
//                            Session["FromDDL"] = 0;
//                            Session["SubmitClicked"] = 1;
//                        }
//                    }
//                    #endregion

//                }
//                else //After Auto Logout or Session out.
//                {
//                    #region
//                    if (data[0].WOStart == null) //Before Submit
//                    {
//                        if (data[0].IsMultiWO == 1) //Its a MultiWO //need "Enter Delivered Qty button"
//                        {
//                            Session["FromDDL"] = 4;
//                        }
//                        else //Its a single WO
//                        {
//                            //string P = data[0].Project; code commented By sneha
//                            string wo = data[0].Prod_Order_No;
//                            string pno = data[0].PartNo;
//                            string opno = data[0].OperationNo;

//                            //if (data[0] != null) //Its a single WO from DDL
//                            //{
//                            //    Session["FromDDL"] = 1;
//                            //    Session["SubmitClicked"] = 0;
//                            //}
//                            //else if ((!string.IsNullOrEmpty(data[0].PartNo)) || (!string.IsNullOrEmpty(data[0].OperationNo)) || (!string.IsNullOrEmpty(data[0].Work_Order_No)))//(!string.IsNullOrEmpty(data[0].Project)) || Sneha commented
//                            //{
//                            //    Session["FromDDL"] = 2;
//                            //}
//                            //else
//                            //{
//                            //    Session["FromDDL"] = 0;
//                            //}
//                        }
//                    }
//                    else//After Submit
//                    {
//                        if (data[0].IsMultiWO == 1) //Its a MultiWO //need "Enter Delivered Qty button"
//                        {
//                            Session["FromDDL"] = 4;
//                            Session["SubmitClicked"] = 1;
//                        }
//                        else //Its a single WO
//                        {
//                            Session["FromDDL"] = 1;
//                            Session["SubmitClicked"] = 1;
//                        }
//                    }
//                    #endregion

//                }
//                ViewBag.shift = data[0].Shift;
//                Session["realshift"] = data[0].Shift;

//                bool isHide = false; //make it true when one of the rows as isupdate = 1
//                foreach (var dat in data)
//                {
//                    //if (dat.isUpdate == 1)
//                    //{
//                    //    isHide = true;
//                    //    Session["isHide"] = 1;
//                    //    break;
//                    //}
//                }
//                if (isHide)
//                {
//                    ViewBag.hide = 1;
//                }
//                else
//                {
//                    ViewBag.hide = null;
//                }

//                string shifthere = data[0].Shift;
//                //I am removing the CorrectedDate from condition so 2016-12-30 becarefull about what it retrieves(stale data)
//                //var data2 = db.tblhmiscreens.Where(m => m.MachineID == Machinid && m.OperatiorID == opid).Where(m => m.Status == 0).OrderBy(u => u.Work_Order_No).ThenBy(u=>u.PartNo).ThenBy(u=>u.OperationNo).ToList();
//                var data2 = db.tblworkorderentries.Where(m => m.MachineID == Machinid && m.OperatorID == Convert.ToString(opid)).Where(m => m.Status == 0).OrderBy(m => m.WOStart).ToList();

//                //See if Previously they were doing ReworkOrder
//                if (data2.Count == 1)
//                {
//                    int a1 = 0;
//                    if (int.TryParse(Convert.ToString(Session["WorkOrderClicked"]), out a1))
//                    {
//                        if (a1 == 1) //1st time After reworkorder clicked
//                        {
//                            int a = 0;
//                            if (int.TryParse(Convert.ToString(Session["isWorkOrder"]), out a))
//                            {
//                                if (a == 1)
//                                {
//                                    Session["isWorkOrder"] = 1;
//                                }
//                            }
//                            else
//                            {
//                                Session["isWorkOrder"] = 0;
//                            }
//                        }
//                        else
//                        {
//                            Session["isWorkOrder"] = 0;
//                        }
//                    }
//                    else //When You login.
//                    {
//                        Session["isWorkOrder"] = 0;
//                    }
//                }
//                else
//                {
//                    Session["isWorkOrder"] = 0;
//                    foreach (var row in data2)
//                    {
//                        if (row.isWorkOrder == 1)
//                        {
//                            Session["isWorkOrder"] = 1;
//                            break;
//                        }
//                    }
//                }

//                //Loop to get WO's Details Counts
//                ViewBag.TotalWOCount = data2.Where(m => m.PartNo != null && m.OperationNo != null && m.Prod_Order_No != null).Count();
//                //ViewBag.WOStartedCount = data2.Where(m => m.WOStart != null && m.isWorkInProgress != 3).Count();
//                //ViewBag.WOOnHoldCount = data2.Where(m => m.IsHold == 1).Count();
//                ViewBag.WONotStartedCount = data2.Where(m => m.WOStart == null && (m.PartNo != null || m.OperationNo != null || m.Prod_Order_No != null)).Count();

//                return View(data2);
//            }
//            else
//            {
//                bool tick = false;
//                //var data1 = db.tblhmiscreens.Where(m => m.MachineID == Machinid && m.Status == 0).OrderBy(u => u.Work_Order_No).ThenBy(u=>u.PartNo).ThenBy(u=>u.OperationNo).ToList();
//                var data1 = db.tblworkorderentries.Where(m => m.MachineID == Machinid && m.Status == 0).OrderBy(m => m.WOStart).ToList();
//                foreach (var a in data1)
//                {
//                    //if (a.isUpdate == 1)
//                    //{ tick = true; }
//                }
//                if (tick)
//                {
//                    TempData["saveORUpdate"] = 1;
//                }

//                bool isHide = false; //make it true when one of the rows as isupdate = 1
//                //2017-01-19
//                // foreach (var dat in data)
//                foreach (var dat in data1)
//                {
//                    //if (dat.isUpdate == 1)
//                    //{
//                    //    isHide = true;
//                    //    break;
//                    //}
//                }
//                if (isHide)
//                {
//                    ViewBag.hide = 1;
//                }
//                else
//                {
//                    ViewBag.hide = null;
//                }

//                //Default to Handle Manual/ScanEntry
//                if (Convert.ToInt32(TempData["ForDDL2"]) == 2)
//                {
//                    Session["FromDDL"] = 2;
//                    Session["SubmitClicked"] = 0;
//                }

//                int fromDDLInt = 6; //i am fake initializing to 6 , because i haven't used 6 anywhere.
//                string blah = Convert.ToString(Session["FromDDL"]);
//                int.TryParse(Convert.ToString(Session["FromDDL"]), out fromDDLInt);

//                if (!string.IsNullOrEmpty(blah.Trim())) // Implies Session is Alive. Continous
//                {
//                    #region
//                    if (fromDDLInt == 4) //implies that its a MultiWO & From DDL
//                    {
//                        if (data1[0].WOStart == null) //Before Submit
//                        {
//                            Session["FromDDL"] = 4;
//                            Session["SubmitClicked"] = 0;
//                        }
//                        else //After Submit
//                        {
//                            Session["FromDDL"] = 4;
//                            Session["SubmitClicked"] = 1;
//                        }
//                    }
//                    else if (fromDDLInt == 1)
//                    {
//                        if (data1[0].WOStart == null) //Before Submit
//                        {
//                            Session["FromDDL"] = 1;
//                            Session["SubmitClicked"] = 0;
//                        }
//                        else //After Submit
//                        {
//                            Session["FromDDL"] = 1;
//                            Session["SubmitClicked"] = 1;
//                        }
//                    }
//                    else if (fromDDLInt == 2) // Manual/ScanEntry
//                    {
//                        if (data1[0].WOStart == null) //Before Submit
//                        {
//                            Session["FromDDL"] = 2;
//                            Session["SubmitClicked"] = 0;
//                        }
//                        else //After Submit
//                        {
//                            Session["FromDDL"] = 0;
//                            Session["SubmitClicked"] = 1;
//                        }
//                    }
//                    else if (fromDDLInt == 0)
//                    {
//                        Session["FromDDL"] = 0;
//                        Session["SubmitClicked"] = 0;
//                    }
//                    #endregion
//                }
//                else //After Auto Logout or Session out.
//                {
//                    #region
//                    if (data1[0].WOStart == null) //Before Submit
//                    {
//                        if (data1[0].IsMultiWO == 1) //Its a MultiWO //need "Enter Delivered Qty button"
//                        {
//                            Session["FromDDL"] = 4;
//                        }
//                        else //Its a single WO
//                        {
//                            //if (data1[0].DDLWokrCentre != null) //Its a single WO from DDL
//                            //{
//                            //    Session["FromDDL"] = 1;
//                            //    Session["SubmitClicked"] = 0;
//                            //}
//                            //else if ((!string.IsNullOrEmpty(data1[0].Work_Order_No)) || (!string.IsNullOrEmpty(data1[0].OperationNo)) || (!string.IsNullOrEmpty(data1[0].PartNo)))//Its Manual Entry.//(!string.IsNullOrEmpty(data1[0].Project)) || Sneha
//                            //{
//                            //    Session["FromDDL"] = 2;
//                            //}
//                            //else
//                            //{
//                            //    Session["FromDDL"] = 0;
//                            //}
//                        }
//                    }
//                    else//After Submit
//                    {
//                        if (data1[0].IsMultiWO == 1) //Its a MultiWO //need "Enter Delivered Qty button"
//                        {
//                            Session["FromDDL"] = 4;
//                            Session["SubmitClicked"] = 1;
//                        }
//                        else //Its a single WO
//                        {
//                            //Its a single WO from DDL or Manual Entry(AfterSubmit) . They are same.
//                            Session["FromDDL"] = 1;
//                            Session["SubmitClicked"] = 1;
//                        }
//                    }
//                    #endregion
//                }

//                if (data1.Count > 1)
//                {
//                    int b = 0;
//                    if (int.TryParse(Convert.ToString(Session["isWorkOrder"]), out b))
//                    {
//                        if (b == 1)
//                        {
//                            Session["isWorkOrder"] = 1;
//                        }
//                    }
//                    else
//                    {
//                        Session["isWorkOrder"] = 0;
//                    }
//                }
//                else
//                {
//                    Session["isWorkOrder"] = 0;
//                    foreach (var row in data1)
//                    {
//                        if (row.isWorkOrder == 1)
//                        {
//                            Session["isWorkOrder"] = 1;
//                            break;
//                        }
//                    }
//                }

//                //Loop to get WO's Details Counts
//                ViewBag.TotalWOCount = data1.Where(m => m.PartNo != null && m.OperationNo != null && m.Prod_Order_No != null).Count();
//                //ViewBag.WOStartedCount = data1.Where(m => m.WOStart != null && m.isWorkInProgress != 3).Count();
//                //ViewBag.WOOnHoldCount = data1.Where(m => m.IsHold == 1).Count();
//                ViewBag.WONotStartedCount = data1.Where(m => m.WOStart == null && (m.PartNo != null || m.OperationNo != null || m.Prod_Order_No != null)).Count();

//                return View(data1);
//            }

//            #endregion

//        }

//        #endregion

//        #region Start All, Index Post
//        //Control comes here when StartAll button is clicked.
//        [HttpPost]
//        public ActionResult Index(IList<tblworkorderentry> tbldaily_plan)
//        {
//            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
//            {
//                return RedirectToAction("Login", "Login", null);
//            }

//            //if (IDLEorGenericWorkisON())//commented as Generic work order is not there in UnitworksCSS
//            //{
//            //    Session["Error"] = "Please End IDLE/GenericWork Before Selecting New Work Orders";
//            //    return RedirectToAction("Index");
//            //}

//            DateTime presentdate = System.DateTime.Now.Date;
//            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
//            {
//                return RedirectToAction("Login", "Login", null);
//            }
//            ViewBag.Logout = Session["Username"];
//            ViewBag.roleid = Session["RoleID"];
//            String Username = Session["Username"].ToString();
//            int UserID = Convert.ToInt32(Session["UserID"].ToString());
//            int machineID = Convert.ToInt32(Session["MachineID"]);
//            ViewBag.DPIsMenu = 0;
//            if (tbldaily_plan != null)
//            {
//                int count = 0;
//                int ExceptionHMIID = 0;
//                foreach (var plan in tbldaily_plan)
//                {
//                    //1) Check eligibility to Start (Even if 1 fails reject All)
//                    if (plan.IsHold == 1 && plan.WOStart == null)
//                    //if (plan.isWorkInProgress == 3)
//                    {
//                        Session["VError"] = "End HOLD Before StartAll is Clicked";
//                        return RedirectToAction("Index");
//                    }
//                    else if (plan.OperatorID != null && plan.Shift != null && plan.PartNo != null && plan.Prod_Order_No != null && plan.OperationNo != null && plan.Prod_Order_No != null && plan.Total_Qty.HasValue)//&& plan.Project != null && plan.Prod_FAI != null && Code removed from this condition 1 Aug 2017
//                    {
//                    }
//                    else if (plan.WOStart == null && plan.Prod_Order_No == null && plan.PartNo == null && plan.OperationNo == null)
//                    {// This condition is to check for empty row if empty row found that will not be started
//                        if (ExceptionHMIID == 0)
//                        {
//                            ExceptionHMIID = Convert.ToInt32(plan.HMIID);
//                        }
//                        else
//                        {
//                            Session["VError"] = "Please enter all Details Before StartAll is Clicked.";
//                            return RedirectToAction("Index");
//                        }
//                    }
//                    else
//                    {
//                        Session["VError"] = "Please enter all Details Before StartAll is Clicked.";
//                        return RedirectToAction("Index");
//                    }
//                }

//                //checking for duplicate WorkOrders
//                #region
//                foreach (var plan in tbldaily_plan)
//                {
//                    int hmiid = Convert.ToInt32(plan.HMIID);
//                    if (ExceptionHMIID != hmiid)
//                    {
//                        string woNo = Convert.ToString(plan.Prod_Order_No);
//                        string opNo = Convert.ToString(plan.OperationNo);
//                        string partNo = Convert.ToString(plan.PartNo);

//                        var InHMIData = db.tblworkorderentries.Where(m => m.Prod_Order_No == woNo && m.PartNo == partNo && m.OperationNo == opNo && m.Status == 0 && m.MachineID == machineID && m.HMIID != hmiid).FirstOrDefault();
//                        if (InHMIData != null)
//                        {
//                            Session["Error"] = "Duplicate WorkOrder:" + woNo + " OpNo: " + opNo + " PartNo:" + partNo;
//                            //db.tblhmiscreens.Remove(plan);
//                            db.Entry(plan).State = System.Data.Entity.EntityState.Deleted;
//                            db.SaveChanges();
//                            return RedirectToAction("Index");
//                        }

//                        //2017-06-22
//                        var HMICompletedData = db.tblworkorderentries.Where(m => m.Prod_Order_No == woNo && m.PartNo == partNo && m.OperationNo == opNo).FirstOrDefault();
//                        if (HMICompletedData != null)
//                        {
//                            Session["Error"] = "Job is Finished for WorkOrder:" + woNo + " OpNo: " + opNo + " PartNo:" + partNo;
//                            var hmirow = db.tblworkorderentries.Find(hmiid);
//                            db.tblworkorderentries.Remove(hmirow);
//                            db.SaveChanges();
//                            return RedirectToAction("Index");
//                        }

//                    }
//                }
//                #endregion

//                //Order Them Prior to checking them for " Sequence of Start "  Condition 2017-01-09
//                tbldaily_plan = tbldaily_plan.OrderBy(m => m.Prod_Order_No).ThenBy(m => m.PartNo).ThenBy(m => m.OperationNo).ToList();

//                foreach (var plan in tbldaily_plan)
//                {
//                    int hmiid = plan.HMIID;
//                    if (hmiid != ExceptionHMIID)
//                    {
//                        tblworkorderentry hmiidData = db.tblworkorderentries.Where(m => m.HMIID == hmiid).FirstOrDefault();
//                        string WONo = Convert.ToString(hmiidData.Prod_Order_No);
//                        string PNo = Convert.ToString(hmiidData.PartNo);
//                        int Opno = Convert.ToInt32(hmiidData.OperationNo);
//                        //var HoldData = db.tblmanuallossofentries.Where(m => m.PartNo == PNo && m.OpNo == Opno && m.WONo == WONo && m.EndDateTime == null).OrderByDescending(m => m.MLossID).SingleOrDefault();
//                        //if (HoldData != null)
//                        //{
//                        //    HoldData.EndDateTime = DateTime.Now;
//                        //    db.Entry(HoldData).State = System.Data.Entity.EntityState.Modified;
//                        //    db.SaveChanges();
//                        //}
//                        if (hmiidData.WOStart == null)
//                        {
//                            hmiidData.WOStart = DateTime.Now;
//                        }
//                        else
//                        {
//                            continue;
//                        }

//                        //Get Processed Qty
//                        int newProcessedQty = 0;
//                        int PrvProcessQty = 0, PrvDeliveredQty = 0;
//                        string woNo = Convert.ToString(hmiidData.Prod_Order_No);
//                        string opNo = Convert.ToString(hmiidData.OperationNo);
//                        string partNo = Convert.ToString(hmiidData.PartNo);
//                        int OperationNoInt = Convert.ToInt32(opNo);

//                        #region 2017-02-07

//                        //Logic to check sequence of Submit Based on WONo, PartNo and OpNo.
//                        //2017-01-21
//                        using (i_facility_shaktiEntities1 dbHMI = new i_facility_shaktiEntities1())
//                        {
//                            string WIPQuery = @"SELECT * from tblworkorderentry where  HMIID IN ( SELECT HMIID from tblworkorderentry where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + opNo + "' order by HMIID ) group by Work_Order_No,PartNo,OperationNo ";
//                            var WIP = dbHMI.tblworkorderentries.SqlQuery(WIPQuery).ToList();
//                            foreach (var row in WIP)
//                            {
//                                int InnerOpNo = Convert.ToInt32(row.OperationNo);
//                                if (OperationNoInt > InnerOpNo)
//                                {
//                                    if (row.WOStart == null) //=> lower OpNo is not submitted.
//                                    {
//                                        Session["VError"] = " Submit WONo: " + row.Prod_Order_No + " and PartNo: " + row.PartNo + " and OperationNo: " + InnerOpNo;
//                                        return RedirectToAction("Index");
//                                    }
//                                }
//                            }
//                        }

//                        ////New Logic to Check Sequential Submit 2017-01-21
//                        //bool IsInHMI = true;
//                        ////int OperationNoInt = Convert.ToInt32(opNo);
//                        //string WIPQuery1 = @"SELECT * from tblddl where WorkOrder = '" + woNo + "' and MaterialDesc = '" + partNo + "' and OperationNo != '" + opNo + "'  and IsCompleted = 0 order by WorkOrder,MaterialDesc,OperationNo  ";
//                        //var WIPDDL = db.tblddls.SqlQuery(WIPQuery1).ToList();
//                        //foreach (var row in WIPDDL)
//                        //{
//                        //    IsInHMI = true; //reinitialize
//                        //    int InnerOpNo = Convert.ToInt32(row.OperationNo);
//                        //    if (InnerOpNo < OperationNoInt)
//                        //    {
//                        //        bool IsItWrong = false;
//                        //        string WIPQueryHMI = @"SELECT * from tblhmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + InnerOpNo + "' order by HMIID limit 1 ";
//                        //        var WIP = db.tblworkorderentries.SqlQuery(WIPQueryHMI).ToList();

//                        //        if (WIP.Count == 0)
//                        //        {
//                        //            //Session["VError"] = " Select & Start WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
//                        //            //return RedirectToAction("Index");
//                        //            IsInHMI = false;
//                        //        }
//                        //        else
//                        //        {
//                        //            foreach (var rowHMI in WIP)
//                        //            {
//                        //                if (rowHMI.Date == null) //=> lower OpNo is not submitted.
//                        //                {
//                        //                    Session["VError"] = " Start WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;

//                        //                    //If its in current list then its not error.
//                        //                    bool inCurrentList = false;
//                        //                    foreach (var planrow in tbldaily_plan)
//                        //                    {
//                        //                        if (InnerOpNo == Convert.ToInt32(planrow.OperationNo))
//                        //                        {
//                        //                            inCurrentList = true;
//                        //                            break;
//                        //                        }
//                        //                    }
//                        //                    if (!inCurrentList)
//                        //                    {
//                        //                        return RedirectToAction("Index");
//                        //                    }
//                        //                }
//                        //            }
//                        //        }

//                        //        if (!IsInHMI)
//                        //        {
//                        //            //Strange , it might have been started in Normal WorkCenter as MultiWorkOrder.
//                        //            #region //also check in MultiWO table
//                        //            //string WIPQueryMultiWO = @"SELECT * from tbl_multiwoselection where WorkOrder = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + InnerOpNo + "' order by MultiWOID limit 1 ";
//                        //            //var WIPMWO = db.tbl_multiwoselection.SqlQuery(WIPQueryMultiWO).ToList();

//                        //            //if (WIPMWO.Count == 0)
//                        //            //{
//                        //            //    Session["VError"] = " Select  WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
//                        //            //    return RedirectToAction("Index");
//                        //            //    //IsInHMI = false;
//                        //            //    //break;
//                        //            //}

//                        //            //foreach (var rowHMI in WIPMWO)
//                        //            //{
//                        //            //    int hmiid1 = Convert.ToInt32(rowHMI.HMIID);
//                        //            //    var MWOHMIData = db.tblworkorderentries.Where(m => m.HMIID == hmiid1).FirstOrDefault();
//                        //            //    if (MWOHMIData != null) //obviously != 0
//                        //            //    {
//                        //            //        if (MWOHMIData.Date == null) //=> lower OpNo is not submitted.
//                        //            //        {
//                        //            //            Session["VError"] = " Start WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
//                        //            //            return RedirectToAction("Index");
//                        //            //            //break;
//                        //            //        }
//                        //            //        else
//                        //            //        {
//                        //            //            Session["VError"] = null;
//                        //            //            IsInHMI = true;
//                        //            //        }
//                        //            //    }
//                        //            //}
//                        //            #endregion
//                        //        }
//                        //    }
//                        //}

//                        //string WIPQuery1 = @"SELECT * from tblhmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo != '" + opNo + "' order by Work_Order_No,PartNo,OperationNo";

//                        //Commented on 2017-05-29
//                        /////to Catch those Manual WorkOrders 
//                        //string WIPQuery2 = @"SELECT * from tblhmiscreen where  HMIID IN ( SELECT Max(HMIID) from tblhmiscreen where  HMIID IN  ( SELECT HMIID from tblhmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo != '" + opNo + "' and  IsMultiWO = 0 and DDLWokrCentre is null order by HMIID ) group by Work_Order_No,PartNo,OperationNo ) order by OperationNo ;";
//                        //var WIPDDL1 = db.tblhmiscreens.SqlQuery(WIPQuery2).ToList();
//                        //foreach (var row in WIPDDL1)
//                        //{
//                        //    int InnerOpNo = Convert.ToInt32(row.OperationNo);
//                        //    if (InnerOpNo < OperationNoInt)
//                        //    {
//                        //        string WIPQueryHMI = @"SELECT * from tblhmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + InnerOpNo + "' order by HMIID limit 1 ";
//                        //        var WIP = db.tblhmiscreens.SqlQuery(WIPQueryHMI).ToList();
//                        //        if (WIP.Count == 0)
//                        //        {
//                        //            Session["VError"] = " Select & Start WONo: " + row.Work_Order_No + " and PartNo: " + row.PartNo + " and OperationNo: " + InnerOpNo;
//                        //            return RedirectToAction("Index");
//                        //        }
//                        //        foreach (var rowHMI in WIP)
//                        //        {
//                        //            if (rowHMI.Date == null) //=> lower OpNo is not submitted.
//                        //            {
//                        //                Session["VError"] = " Start WONo: " + row.Work_Order_No + " and PartNo: " + row.PartNo + " and OperationNo: " + InnerOpNo;
//                        //                return RedirectToAction("Index");
//                        //            }
//                        //        }
//                        //    }
//                        //}

//                        #endregion

//                        int deliveredQty = 0;
//                        int TargetQtyNew = Convert.ToInt32(hmiidData.Total_Qty);
//                        int DeliveredNew = Convert.ToInt32(hmiidData.Yield_Qty);
//                        int ProcessedNew = Convert.ToInt32(hmiidData.ProcessQty);
//                        newProcessedQty = DeliveredNew + ProcessedNew;
//                        if (Convert.ToInt32(hmiidData.Status) == 1 || TargetQtyNew == newProcessedQty)
//                        {
//                            Session["Error"] = "Job is Finished for WorkOrder:" + woNo + " OpNo: " + opNo + " PartNo:" + partNo;
//                            db.tblworkorderentries.Remove(hmiidData);
//                            db.SaveChanges();
//                            return RedirectToAction("Index");

//                        }



//                        if (TargetQtyNew < newProcessedQty)
//                        {
//                            Session["Error"] = "Previous ProcessedQty :" + newProcessedQty + ". TargetQty Cannot be Less than Processed";
//                            hmiidData.ProcessQty = 0;
//                            hmiidData.WOStart = null;
//                            Session["FromDDL"] = 2;
//                            TempData["ForDDL2"] = 2;
//                            db.Entry(hmiidData).State = System.Data.Entity.EntityState.Modified;
//                            db.SaveChanges();
//                            return RedirectToAction("Index");
//                        }

//                        //////////////////////////////////hmiidData.ProcessQty = Convert.ToInt32(PrvProcessQty + PrvDeliveredQty);

//                        int ReworkOrder = 0;
//                        string ReworkOrderString = Convert.ToString(Session["isWorkOrder"]);
//                        if (int.TryParse(ReworkOrderString, out ReworkOrder))
//                        {
//                            if (ReworkOrderString == "1")
//                            {
//                                hmiidData.isWorkOrder = 1;
//                            }
//                            else
//                            {
//                                hmiidData.isWorkOrder = 0;
//                            }
//                        }

//                        Session["WorkOrderClicked"] = 0;

//                        //2017-03-14
//                        DateTime EndTime = DateTime.Now;
//                        int hmiiid = hmiidData.HMIID;
//                        //1) 1remove hold on current one 
//                        var hmiData = db.tblworkorderentries.Where(m => m.HMIID == hmiiid).FirstOrDefault();
//                        hmiData.IsHold = 0;
//                        db.Entry(hmiData).State = System.Data.Entity.EntityState.Modified;
//                        db.SaveChanges();

//                        int OldHMIID = 0;
//                        //update the ishold and end time in old one.
//                        var tblhmi = db.tblworkorderentries.Where(m => m.HMIID != hmiiid && m.Prod_Order_No == hmiData.Prod_Order_No && m.PartNo == hmiData.PartNo && m.OperationNo == hmiData.OperationNo).OrderByDescending(m => m.PEStartTime).FirstOrDefault();
//                        if (tblhmi != null)
//                        {
//                            OldHMIID = tblhmi.HMIID;
//                            tblhmi.IsHold = 2;
//                            db.Entry(tblhmi).State = System.Data.Entity.EntityState.Modified;
//                            db.SaveChanges();

//                            ////3) update the EndDateTime column in manuallossofentry table.
//                            //var tblmanualLossData = db.tblmanuallossofentries.Where(m => m.HMIID == OldHMIID).OrderByDescending(m => m.StartDateTime).FirstOrDefault();
//                            //if (tblmanualLossData != null)
//                            //{
//                            //    tblmanualLossData.EndHMIID = hmiiid;
//                            //    tblmanualLossData.EndDateTime = EndTime;
//                            //    db.Entry(tblmanualLossData).State = System.Data.Entity.EntityState.Modified;
//                            //    db.SaveChanges();
//                            //}
//                        }

//                        db.Entry(hmiidData).State = System.Data.Entity.EntityState.Modified;
//                        db.SaveChanges();
//                    }
//                }
//                Session["SubmitClicked"] = 1;
//                return RedirectToAction("Index");
//            }
//            return View();
//        }



//        #endregion

//        #region Set Opearator and Shift Details
//        public ActionResult changeShiftNorm(String Shift)
//        {
//            Session["Show"] = null;
//            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
//            {
//                return RedirectToAction("Login", "Login", null);
//            }
//            ViewBag.roleid = Session["RoleID"];
//            string CorrectedDate = null;
//            tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).SingleOrDefault();
//            TimeSpan Start = StartTime.StartTime;
//            if (Start <= DateTime.Now.TimeOfDay)
//            {
//                CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
//            }
//            else
//            {
//                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
//            }
//            int MachineID = Convert.ToInt32(Session["MachineID"]);


//            int opid = Convert.ToInt32(Session["Opid"]);  //Getting OpearatorID tblusers Pk is Operator Id
//            var HMIData = db.tblworkorderentries.Where(m => m.CorrectedDate == CorrectedDate && m.Status == 0 && m.MachineID == MachineID).ToList();
//            foreach (var HMIRow in HMIData)
//            {
//                //Row will be updated 
//                HMIRow.Shift = Shift;
//                HMIRow.Status = 1;
//                HMIRow.PEStartTime = DateTime.Now;
//                db.Entry(HMIRow).State = System.Data.Entity.EntityState.Modified;
//                db.SaveChanges();

//                //HMIScreenForAdmin(MachineID, opid, CorrectedDate); //not doing
//                Session["opid"] = opid;
//                Session["gopid"] = opid;
//                Session["gshift" + opid] = Shift;
//                ViewBag.hide = 1;//set button will be hidden so setting flag 
//            }
//            return RedirectToAction("Index");
//        }

//        #endregion


//        //#region Individual Start

//        //public ActionResult IndividualSubmit(int HMIID = 0)
//        //{
//        //    if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
//        //    {
//        //        return RedirectToAction("Login", "Login", null);
//        //    }

//        //    if (IDLEorGenericWorkisON())
//        //    {
//        //        Session["Error"] = "Please End IDLE/GenericWork Before Selecting New Work Orders";
//        //        return RedirectToAction("Index");
//        //    }

//        //    ViewBag.Logout = Session["Username"];
//        //    ViewBag.roleid = Session["RoleID"];
//        //    int MachineId = Convert.ToInt32(Session["MachineID"]);

//        //    //Check if similar WorkOrder is in View
//        //    tblworkorderentry hmiidDataDup = db.tblworkorderentries.Where(m => m.HMIID == HMIID).FirstOrDefault();
//        //    if (hmiidDataDup != null)
//        //    {
//        //        int hmiid = Convert.ToInt32(hmiidDataDup.HMIID);
//        //        string woNo = Convert.ToString(hmiidDataDup.Work_Order_No);
//        //        string opNo = Convert.ToString(hmiidDataDup.OperationNo);
//        //        string partNo = Convert.ToString(hmiidDataDup.PartNo);

               

//        //        //2017-06-22
//        //        var HMICompletedData = db.tblworkorderentries.Where(m => m.Work_Order_No == woNo && m.PartNo == partNo && m.OperationNo == opNo && m.isWorkInProgress == 1).FirstOrDefault();
//        //        if (HMICompletedData != null)
//        //        {
//        //            Session["Error"] = "Job is Finished for WorkOrder:" + woNo + " OpNo: " + opNo + " PartNo:" + partNo;

//        //            db.tblworkorderentries.Remove(hmiidDataDup);
//        //            db.SaveChanges();
//        //            return RedirectToAction("Index");
//        //        }

//        //        #region 2017-02-07
//        //        bool isValid = true, IsInHMI = true;
//        //        string IssueMsg = null;
//        //        if (isValid)
//        //        {
//        //            // This is to check whether same work order with same Opnum  job completed 
//        //            var Siblingddldata = db.tblworkorderentries.Where(m => m.isWorkInProgress == 0 && m.Work_Order_No == woNo && m.PartNo == partNo && m.OperationNo != opNo).OrderBy(m => new { m.Work_Order_No, m.PartNo, m.OperationNo }).ToList();
//        //            foreach (var row in Siblingddldata)
//        //            {
//        //                IsInHMI = true; //reinitialize
//        //                int localOPNo = Convert.ToInt32(row.OperationNo);
//        //                string localOPNoString = Convert.ToString(row.OperationNo);
//        //                if (localOPNo < Convert.ToInt32(opNo))
//        //                {
//        //                    #region //Here Check in HMIScreen Table. There are chances that this one is started prior to this round of ddl selection ,
//        //                    //which case is valid.
//        //                    var SiblingHMIdata = db.tblworkorderentries.Where(m => m.Work_Order_No == woNo && m.PartNo == partNo && m.OperationNo == localOPNoString).FirstOrDefault();
//        //                    if (SiblingHMIdata == null)
//        //                    {
//        //                        //IssueMsg = "Please Select Below WorkOrder, WONo: " + WONo + " PartNo: " + Part + " OperationNo: " + localOPNo;
//        //                        IsInHMI = false;
//        //                        //break;
//        //                    }
//        //                    else
//        //                    {
//        //                        if (SiblingHMIdata.Date == null) //=> lower OpNo is not submitted.
                               
//        //                            IssueMsg = " Start WONo: " + row.Work_Order_No + " and PartNo: " + row.PartNo+ " and OperationNo: " + localOPNoString;
//        //                            //return RedirectToAction("Index");
//        //                            IsInHMI = false;
//        //                            break;
//        //                        }
//        //                    }
//        //                    #endregion

//        //                    if (!IsInHMI)
//        //                    {
//        //                    //#region //also check in MultiWO table
//        //                    //string WIPQueryMultiWO = @"SELECT * from tbl_multiwoselection where WorkOrder = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + localOPNo + "' order by MultiWOID limit 1 ";
//        //                    //var WIPMWO = db.tbl_multiwoselection.SqlQuery(WIPQueryMultiWO).ToList();

//        //                    //if (WIPMWO.Count == 0)
//        //                    //{
//        //                    //    IssueMsg = " Select  WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + localOPNoString;
//        //                    //    //return RedirectToAction("Index");
//        //                    //    IsInHMI = false;
//        //                    //    break;
//        //                    //}

//        //                    //foreach (var rowHMI in WIPMWO)
//        //                    //{
//        //                    //    int hmiidInner = Convert.ToInt32(rowHMI.HMIID);
//        //                    //    var MWOHMIData = db.tblhmiscreens.Where(m => m.HMIID == hmiidInner).FirstOrDefault();
//        //                    //    if (MWOHMIData != null) //obviously != 0
//        //                    //    {
//        //                    //        if (MWOHMIData.Date == null) //=> lower OpNo is not submitted.
//        //                    //        {
//        //                    //            IssueMsg = " Start WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + localOPNoString;
//        //                    //            //return RedirectToAction("Index");
//        //                    //            break;
//        //                    //        }
//        //                    //        else
//        //                    //        {
//        //                    //            IsInHMI = true;
//        //                    //        }
//        //                    //    }
//        //                    //}
//        //                    #endregion
//        //                }
//        //            }
//        //            }

//        //            //commented on 2017-05-29
//        //            /////to Catch those Manual WorkOrders 
//        //            //string WIPQuery1 = @"SELECT * from tblhmiscreen where  HMIID IN ( SELECT Max(HMIID) from tblhmiscreen where  HMIID IN  ( SELECT HMIID from tblhmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo != '" + opNo + "' and  IsMultiWO = 0 and DDLWokrCentre is null order by HMIID ) group by Work_Order_No,PartNo,OperationNo ) order by OperationNo ;";
//        //            //var WIPDDL1 = db.tblhmiscreens.SqlQuery(WIPQuery1).ToList();
//        //            //foreach (var row in WIPDDL1)
//        //            //{
//        //            //    int InnerOpNo = Convert.ToInt32(row.OperationNo);
//        //            //    if (InnerOpNo < Convert.ToInt32(opNo))
//        //            //    {
//        //            //        string WIPQueryHMI = @"SELECT * from tblhmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + InnerOpNo + "' order by HMIID limit 1 ";
//        //            //        var WIP1 = db.tblhmiscreens.SqlQuery(WIPQueryHMI).ToList();
//        //            //        if (WIP1.Count == 0)
//        //            //        {
//        //            //            Session["VError"] = " Select & Start WONo: " + row.Work_Order_No + " and PartNo: " + row.PartNo + " and OperationNo: " + InnerOpNo;
//        //            //            return RedirectToAction("Index");
//        //            //        }
//        //            //        foreach (var rowHMI in WIP1)
//        //            //        {
//        //            //            if (rowHMI.Date == null) //=> lower OpNo is not submitted.
//        //            //            {
//        //            //                Session["VError"] = " Start WONo: " + row.Work_Order_No + " and PartNo: " + row.PartNo + " and OperationNo: " + InnerOpNo;
//        //            //                return RedirectToAction("Index");
//        //            //            }
//        //            //        }
//        //            //    }
//        //            //}
//        //        }
//        //        #endregion

//        //        if (!string.IsNullOrEmpty(woNo) && !string.IsNullOrEmpty(opNo) && !string.IsNullOrEmpty(partNo))
//        //        {
//        //        int hmiid = Convert.ToInt32(hmiidDataDup.HMIID);
//        //        var InHMIData = db.tblworkorderentries.Where(m => m.Work_Order_No == woNo && m.PartNo == partNo && m.OperationNo == opNo && m.Status == 0 && m.HMIID != hmiid && m.MachineID == MachineId).FirstOrDefault();
//        //            if (InHMIData != null)
//        //            {
//        //                Session["Error"] = "Duplicate WorkOrder:" + woNo + " OpNo: " + opNo + " PartNo:" + partNo;
//        //                //db.tblhmiscreens.Remove(hmiidDataDup);
//        //                db.Entry(hmiidDataDup).State = System.Data.Entity.EntityState.Deleted;
//        //                db.SaveChanges();
//        //                return RedirectToAction("Index");
//        //            }
//        //        }
//        //    }
//        //tblworkorderentry hmiidData = db.tblworkorderentries.Where(m => m.HMIID == ).FirstOrDefault();
//        //    if (hmiidData.IsHold == 0)
//        //    {
//        //        if (hmiidData.Date == null)
//        //        {
//        //            if (hmiidData.OperatorDet != null && hmiidData.Shift != null && hmiidData.PartNo != null && hmiidData.Work_Order_No != null && hmiidData.OperationNo != null && hmiidData.Target_Qty.HasValue)//&& hmiidData.Project != null && hmiidData.Prod_FAI != null  code removed
//        //            {
//        //                hmiidData.Date = DateTime.Now;

//        //                //Get Processed Qty
//        //                int newProcessedQty = 0;
//        //                int PrvProcessQty = 0, PrvDeliveredQty = 0;
//        //                string woNo = Convert.ToString(hmiidData.Work_Order_No);
//        //                string opNo = Convert.ToString(hmiidData.OperationNo);
//        //                string partNo = Convert.ToString(hmiidData.PartNo);
//        //                int deliveredQty = 0;
//        //                int TargetQtyNew = Convert.ToInt32(hmiidData.Target_Qty);
//        //                int DeliveredQty = Convert.ToInt32(hmiidData.Delivered_Qty);
//        //                int ProcessedQty = Convert.ToInt32(hmiidData.ProcessQty);
//        //                newProcessedQty = DeliveredQty + ProcessedQty;

//        //                if (Convert.ToInt32(hmiidData.isWorkInProgress) == 1 || TargetQtyNew == newProcessedQty)
//        //                {
//        //                    Session["Error"] = "Job is Finished for WorkOrder:" + woNo + " OpNo: " + opNo + " PartNo:" + partNo;
//        //                    db.tblhmiscreens.Remove(hmiidData);
//        //                    db.SaveChanges();
//        //                    return RedirectToAction("Index");
//        //                }


//        //                if (TargetQtyNew < newProcessedQty)
//        //                {
//        //                    Session["Error"] = "Previous ProcessedQty :" + newProcessedQty + ". TargetQty Cannot be Less than Processed";
//        //                    hmiidData.ProcessQty = 0;
//        //                    hmiidData.Date = null;
//        //                    Session["FromDDL"] = 2;
//        //                    TempData["ForDDL2"] = 2;
//        //                    db.Entry(hmiidData).State = System.Data.Entity.EntityState.Modified;
//        //                    db.SaveChanges();
//        //                    return RedirectToAction("Index");
//        //                }


//        //                #region 2017-02-07
//        //                //Check if Lower Operation No is submitted or JF  or PF 'ed Prior to this.
//        //                //1) Get Related WO's , order by Ascending.
//        //                //2) Check if current Opno is Allowed to be Submitted. 
//        //                //(CONDITION : All Opno's lesser than this has to be submitted ( => || PF or JF 'ed Atleast Once).)

//        //                int OperationNoInt = Convert.ToInt32(opNo);
//        //                //var hmiDataInAscendingOrder = db.tblhmiscreens.Where(m => m.Work_Order_No == woNo && m.PartNo == partNo && m.OperationNo != opNo && m.OperationNo.toInt < opNo)).OrderBy(m => m.OperationNo).ToList(); //&& m.OperationNo < opNo "< Cannot be applied to Strings"
//        //                string WIPQuery = @"SELECT * from tblhmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo != '" + opNo + "' and MachineID = '" + MachineId + "' order by HMIID   ";
//        //                var WIP = db.tblhmiscreens.SqlQuery(WIPQuery).ToList();
//        //                foreach (var row in WIP)
//        //                {
//        //                    int InnerOpNo = Convert.ToInt32(row.OperationNo);
//        //                    if (OperationNoInt > InnerOpNo)
//        //                    {
//        //                        if (row.Date == null) //=> lower OpNo is not submitted.
//        //                        {
//        //                            Session["VError"] = " Submit WONo: " + row.Work_Order_No + " and PartNo: " + row.PartNo + " and OperationNo: " + InnerOpNo;
//        //                            return RedirectToAction("Index");
//        //                        }
//        //                    }
//        //                }

//        //                bool IsInHMI = true;
//        //                string WIPQuery2 = @"SELECT * from tblddl where WorkOrder = '" + woNo + "' and MaterialDesc = '" + partNo + "' and OperationNo != '" + opNo + "'  and IsCompleted = 0 order by WorkOrder,MaterialDesc,OperationNo ";
//        //                var WIPDDL = db.tblddls.SqlQuery(WIPQuery2).ToList();
//        //                foreach (var row in WIPDDL)
//        //                {
//        //                    Session["VError"] = null;
//        //                    IsInHMI = true; //reinitialize
//        //                    int InnerOpNo = Convert.ToInt32(row.OperationNo);
//        //                    if (InnerOpNo < OperationNoInt)
//        //                    {
//        //                        bool IsItWrong = false;
//        //                        string WIPQueryHMI = @"SELECT * from tblhmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + InnerOpNo + "' order by HMIID limit 1 ";
//        //                        var WIP1 = db.tblhmiscreens.SqlQuery(WIPQueryHMI).ToList();

//        //                        if (WIP1.Count == 0)
//        //                        {
//        //                            //Session["VError"] = " Select & Start WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
//        //                            IsItWrong = true;
//        //                            IsInHMI = false;
//        //                        }
//        //                        else
//        //                        {
//        //                            foreach (var rowHMI in WIP1)
//        //                            {
//        //                                if (rowHMI.Date == null) //=> lower OpNo is not submitted.
//        //                                {
//        //                                    Session["VError"] = " Start WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
//        //                                    return RedirectToAction("Index");
//        //                                }
//        //                            }
//        //                        }

//        //                        if (!IsInHMI)
//        //                        {
//        //                            #region //also check in MultiWO table
//        //                            string WIPQueryMultiWO = @"SELECT * from tbl_multiwoselection where WorkOrder = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + InnerOpNo + "' order by MultiWOID limit 1 ";
//        //                            var WIPMWO = db.tbl_multiwoselection.SqlQuery(WIPQueryMultiWO).ToList();

//        //                            if (WIPMWO.Count == 0)
//        //                            {
//        //                                Session["VError"] = " Select  WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
//        //                                return RedirectToAction("Index");
//        //                                //IsInHMI = false;
//        //                                break;
//        //                            }

//        //                            foreach (var rowHMI in WIPMWO)
//        //                            {
//        //                                int hmiid = Convert.ToInt32(rowHMI.HMIID);
//        //                                var MWOHMIData = db.tblhmiscreens.Where(m => m.HMIID == hmiid).FirstOrDefault();
//        //                                if (MWOHMIData != null) //obviously != 0
//        //                                {
//        //                                    if (MWOHMIData.Date == null) //=> lower OpNo is not submitted.
//        //                                    {
//        //                                        Session["VError"] = " Start WONo: " + row.WorkOrder + " and PartNo: " + row.MaterialDesc + " and OperationNo: " + InnerOpNo;
//        //                                        return RedirectToAction("Index");
//        //                                        //break;
//        //                                    }
//        //                                    else
//        //                                    {
//        //                                        IsInHMI = true;
//        //                                    }
//        //                                }
//        //                            }
//        //                            #endregion
//        //                        }

//        //                    }
//        //                }

//        //                //commmented On 2017-05-29
//        //                ////2017-01-21 I guess when its purely ManualEntry
//        //                ////string WIPQuery1 = @"SELECT * from tblhmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo != '" + opNo + "' order by Work_Order_No,PartNo,OperationNo";
//        //                //string WIPQuery1 = @"SELECT * from tblhmiscreen where  HMIID IN ( SELECT Max(HMIID) from tblhmiscreen where  HMIID IN  ( SELECT HMIID from tblhmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo != '" + opNo + "' and IsMultiWO = 0 and DDLWokrCentre is null order by HMIID ) group by Work_Order_No,PartNo,OperationNo  ) order by OperationNo;";
//        //                //var WIPDDL1 = db.tblhmiscreens.SqlQuery(WIPQuery1).ToList();
//        //                //foreach (var row in WIPDDL1)
//        //                //{
//        //                //    int InnerOpNo = Convert.ToInt32(row.OperationNo);
//        //                //    if (InnerOpNo < OperationNoInt)
//        //                //    {
//        //                //        string WIPQueryHMI = @"SELECT * from tblhmiscreen where Work_Order_No = '" + woNo + "' and PartNo = '" + partNo + "' and OperationNo = '" + InnerOpNo + "' order by HMIID limit 1 ";
//        //                //        var WIP2 = db.tblhmiscreens.SqlQuery(WIPQueryHMI).ToList();
//        //                //        foreach (var rowHMI in WIP2)
//        //                //        {
//        //                //            if (rowHMI.Date == null) //=> lower OpNo is not submitted.
//        //                //            {
//        //                //                Session["VError"] = " Start WONo: " + row.Work_Order_No + " and PartNo: " + row.PartNo + " and OperationNo: " + InnerOpNo;
//        //                //                return RedirectToAction("Index");
//        //                //            }
//        //                //        }
//        //                //        if (WIP2.Count == 0)
//        //                //        {
//        //                //            Session["VError"] = " Select & Start WONo: " + row.Work_Order_No + " and PartNo: " + row.PartNo + " and OperationNo: " + InnerOpNo;
//        //                //            return RedirectToAction("Index");
//        //                //        }
//        //                //    }
//        //                //}

//        //                #endregion

//        //                int ReworkOrder = 0;
//        //                string ReworkOrderString = Convert.ToString(Session["isWorkOrder"]);
//        //                if (int.TryParse(ReworkOrderString, out ReworkOrder))
//        //                {
//        //                    if (ReworkOrderString == "1")
//        //                    {
//        //                        hmiidData.isWorkOrder = 1;
//        //                    }
//        //                    else
//        //                    {
//        //                        hmiidData.isWorkOrder = 0;
//        //                    }
//        //                }

//        //                Session["WorkOrderClicked"] = 0;
//        //                hmiidData.Status = 0;
//        //                db.Entry(hmiidData).State = System.Data.Entity.EntityState.Modified;
//        //                db.SaveChanges();
//        //            }
//        //            else
//        //            {
//        //                Session["VError"] = "Please enter all Details Before StartAll is Clicked.";
//        //                return RedirectToAction("Index");
//        //            }
//        //        }
//        //        else
//        //        {
//        //            Session["VError"] = "WorkOrder is already Started.";
//        //            return RedirectToAction("Index");
//        //        }
//        //    }
//        //    else
//        //    {
//        //        Session["VError"] = "Please end HOLD before Start";
//        //        return RedirectToAction("Index");
//        //    }

//        //    Session["VError"] = "Job Started!!!";
//        //    return RedirectToAction("Index");
//        //}

//        //#endregion


//        List<string> GetMacHierarchyData(int MachineID)
//        {
//            List<string> HierarchyData = new List<string>();
//            //1st get PlantName or -
//            //2nd get ShopName or -
//            //3rd get CellName or -
//            //4th get MachineName.

//            using (i_facility_shaktiEntities1 dbMac = new i_facility_shaktiEntities1())
//            {
//                var machineData = dbMac.tblmachinedetails.Where(m => m.MachineID == MachineID).FirstOrDefault();
//                int PlantID = Convert.ToInt32(machineData.PlantID);
//                string name = "-";
//                name = dbMac.tblplants.Where(m => m.PlantID == PlantID).Select(m => m.PlantName).FirstOrDefault();
//                HierarchyData.Add(name);

//                string ShopIDString = Convert.ToString(machineData.ShopID);
//                int value;
//                if (int.TryParse(ShopIDString, out value))
//                {
//                    name = dbMac.tblshops.Where(m => m.ShopID == value).Select(m => m.ShopName).FirstOrDefault();
//                    HierarchyData.Add(name.ToString());
//                }
//                else
//                {
//                    HierarchyData.Add("-");
//                }

//                string CellIDString = Convert.ToString(machineData.CellID);
//                if (int.TryParse(CellIDString, out value))
//                {
//                    name = dbMac.tblcells.Where(m => m.CellID == value).Select(m => m.CellName).FirstOrDefault();
//                    HierarchyData.Add(name.ToString());
//                }
//                else
//                {
//                    HierarchyData.Add("-");
//                }
//                HierarchyData.Add(Convert.ToString(machineData.MachineDisplayName));
//                HierarchyData.Add(Convert.ToString(machineData.MachineDisplayName));
//            }
//            return HierarchyData;
//        }

//        string GetOrderedHMIIDs(string hmiids)
//        {
//            string retHMIIDs = null;
//            if (hmiids != null)
//            {

//                string WIPQueryOuter = @"SELECT * from tblworkorderentry where HMIID IN ( " + hmiids + " ) order by Work_Order_No,PartNo,OperationNo ; ";
//                var WIPOuter = db.tblworkorderentries.SqlQuery(WIPQueryOuter).ToList();

//                for (int id = 0; id < WIPOuter.Count; id++)
//                {
//                    if (retHMIIDs == null)
//                    {
//                        retHMIIDs = Convert.ToString(WIPOuter[id].HMIID);
//                    }
//                    else
//                    {
//                        retHMIIDs += "," + Convert.ToString(WIPOuter[id].HMIID);
//                    }
//                }
//            }

//            return retHMIIDs;
//        }


//        #region Overall Job finish
//        public ActionResult OverAllFinish(string HMIIDs)
//        {
//            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
//            {
//                return RedirectToAction("Login", "Login", null);
//            }
//            //1) extract hmiids from string
//            //2) Check if eligible for JobFinish (Eligible only if all are eligible)
//            //3) Now Job Finish all of them

//            int machineID = Convert.ToInt32(Session["MachineID"]);
//            if (IDLEorGenericWorkisON())
//            {
//                Session["Error"] = "Please End IDLE/GenericWork Before Selecting New Work Orders";
//                return RedirectToAction("Index");
//            }

//            //1)
//            string[] HMIIDArray = HMIIDs.Split(',');

//            //2)
//            int ExceptionHMIID = 0;
//            foreach (var hmiid in HMIIDArray)
//            {
//                int hmiid1 = Convert.ToInt32(hmiid);
//                var HMIData = db.tblworkorderentries.Where(m => m.HMIID == hmiid1).FirstOrDefault();

//                //if (HMIData.isWorkInProgress == 3)
//                if (HMIData.IsHold == 1)
//                {
//                    Session["VError"] = "End HOLD before Clicking Job Finish";
//                    return RedirectToAction("Index");
//                }
//                else if (HMIData.WOStart != null)
//                {
//                    int deliveredQty = 0, processQty = 0;
//                    int.TryParse(Convert.ToString(HMIData.ProcessQty), out processQty);
//                    if (int.TryParse(Convert.ToString(HMIData.Yield_Qty), out deliveredQty))
//                    {
//                        if ((deliveredQty + processQty) == Convert.ToInt32(HMIData.Total_Qty))
//                        {
//                        }
//                        else
//                        {
//                            Session["VError"] = "DeliveredQty Must be equal to Target Qty";
//                            return RedirectToAction("Index");
//                        }
//                    }
//                    else
//                    {
//                        Session["VError"] = "Enter Delivered Quantity";
//                        return RedirectToAction("Index");
//                    }
//                }
//                //else if (HMIData.Date == null && HMIData.Work_Order_No == null && HMIData.PartNo == null && HMIData.OperationNo == null)
//                else if (HMIData.WOStart == null)
//                {// Do Nothing. Just to Skip our Extra Empty Row
//                    if (ExceptionHMIID == 0)
//                    {
//                        ExceptionHMIID = Convert.ToInt32(HMIData.HMIID);
//                    }
//                    else
//                    {
//                        Session["VError"] = "Please enter all Details Before StartAll is Clicked.";
//                        return RedirectToAction("Index");
//                    }
//                }
//                else
//                {
//                    Session["VError"] = "Please Start All WorkOrders Before JobFinish";
//                    return RedirectToAction("Index");
//                }
//            }

//            //Check if its  Empty Screen and JobFinish is clicked.
//            if (HMIIDArray.Length == 1)
//            {
//                foreach (var hmiid in HMIIDArray)
//                {
//                    int hmiidi = Convert.ToInt32(hmiid);
//                    if (hmiidi == ExceptionHMIID)
//                    {
//                        Session["VError"] = "There are no WorkOrder to Finish";
//                        return RedirectToAction("Index");
//                    }
//                }
//            }


//            //3)
//            //foreach (var hmiid in HMIIDArray)
//            //{
//            //    int hmiid1 = Convert.ToInt32(hmiid);
//            //    if (hmiid1 != ExceptionHMIID)
//            //    {
//            //        var HMIData = db.tblhmiscreens.Where(m => m.HMIID == hmiid1).FirstOrDefault();

//            //        string wono = HMIData.Work_Order_No;
//            //        string partno = HMIData.PartNo;
//            //        string opno = HMIData.OperationNo;
//            //        var ddldata = db.tblddls.Where(m => m.WorkOrder == wono && m.MaterialDesc == partno && m.OperationNo == opno).FirstOrDefault();
//            //        if (ddldata != null)
//            //        {
//            //            ddldata.IsCompleted = 1;
//            //            db.Entry(ddldata).State = System.Data.Entity.EntityState.Modified;
//            //        }

//            //        HMIData.Status = 2;
//            //        HMIData.isWorkInProgress = 1;
//            //        HMIData.Time = DateTime.Now;
//            //        db.Entry(HMIData).State = System.Data.Entity.EntityState.Modified;
//            //        db.SaveChanges();
//            //    }
//            //}

//            //Gen , Seperated String from HMIIDArray
//            string hmiids = null;
//            for (int hmiid = 0; hmiid < HMIIDArray.Length; hmiid++)
//            {
//                if (hmiids == null)
//                {
//                    string localhmiidString = HMIIDArray[hmiid];
//                    if (ExceptionHMIID.ToString() != localhmiidString)
//                    {
//                        hmiids = HMIIDArray[hmiid].ToString();
//                    }
//                }
//                else
//                {
//                    if (ExceptionHMIID.ToString() != HMIIDArray[hmiid])
//                    {
//                        hmiids += "," + HMIIDArray[hmiid].ToString();
//                    }
//                }
//            }

//            //Get hmiids (as comma seperated string) in ascending order based on wono,partno,Opno.
//            hmiids = GetOrderedHMIIDs(hmiids);


//            // this and one below , both were there so 2017-02-03 //string WIPQueryOuter = @"SELECT * from tblhmiscreen where HMIID IN ( ' " + HMIIDArray + " ' ) order by  Work_Order_No,PartNo,OperationNo ; ";
//            string WIPQueryOuter = @"SELECT * from tblworkorderentry where HMIID IN ( " + hmiids + " ) order by  Work_Order_No,PartNo,OperationNo ; ";
//            var WIPOuter = db.tblworkorderentries.SqlQuery(WIPQueryOuter).ToList();

//            foreach (var rowOuter in WIPOuter)
//            {
//                using (i_facility_shaktiEntities1 dbsimilar = new i_facility_shaktiEntities1())
//                {
//                    int HMIId = rowOuter.HMIID;
//                    #region If its as SingleWO
//                    var SimilarWOData = dbsimilar.tblworkorderentries.Where(m => m.Prod_Order_No == rowOuter.Prod_Order_No && m.OperationNo == rowOuter.OperationNo && m.PartNo == rowOuter.PartNo && m.MachineID != machineID).FirstOrDefault();
//                    if (SimilarWOData != null)
//                    {
//                        int InnerMacID = Convert.ToInt32(dbsimilar.tblworkorderentries.Where(m => m.HMIID == SimilarWOData.HMIID).Select(m => m.MachineID).FirstOrDefault());
//                        var MacDispName = Convert.ToString(dbsimilar.tblmachinedetails.Where(m => m.MachineID == InnerMacID).Select(m => m.MachineDisplayName).FirstOrDefault());

//                        Session["Error"] = " Same WorkOrder in Machine: " + MacDispName + " , So you cannot JobFinish ";
//                        return RedirectToAction("Index");
//                    }
//                    #endregion

//                    //#region If its as MultiWO// code commented By Sneha
//                    //var SimilarWODataMulti = dbsimilar.tbl_multiwoselection.Where(m => m.WorkOrder == rowOuter.Work_Order_No && m.OperationNo == rowOuter.OperationNo && m.PartNo == rowOuter.PartNo && m.HMIID != HMIId && m.tblhmiscreen.isWorkInProgress == 2).FirstOrDefault();
//                    //if (SimilarWODataMulti != null)
//                    //{
//                    //    int InnerHMIID = (int)SimilarWODataMulti.HMIID;
//                    //    int InnerMacID = Convert.ToInt32(dbsimilar.tblhmiscreens.Where(m => m.HMIID == InnerHMIID).Select(m => m.MachineID).FirstOrDefault());
//                    //    var MacDispName = Convert.ToString(dbsimilar.tblmachinedetails.Where(m => m.MachineID == InnerMacID).Select(m => m.MachineDispName).FirstOrDefault());

//                    //    Session["Error"] = " Same WorkOrder in Machine: " + MacDispName + " , So you cannot JobFinish ";
//                    //    return RedirectToAction("Index");
//                    //}
//                    //#endregion
//                }
//            }

//            #region 2017-07-01
//            foreach (var rowOuter in WIPOuter)
//            {
//                int hmiid1 = Convert.ToInt32(rowOuter.HMIID);
//                string woNo = Convert.ToString(rowOuter.Prod_Order_No);
//                string opNo = Convert.ToString(rowOuter.OperationNo);
//                string partNo = Convert.ToString(rowOuter.PartNo);

//                //Logic to check sequence of JF Based on WONo, PartNo and OpNo.
//                int OperationNoInt = Convert.ToInt32(opNo);

//                using (i_facility_shaktiEntities1 dbHMI = new i_facility_shaktiEntities1())
//                {
//                    string WIPQuery1 = @"SELECT * from tblworkorderentry where Prod_Order_No = '" + woNo + "' and PartNo = '" + partNo + "'  and OperationNo != '" + opNo + "' and isWorkInProgress = 2 order by OperationNo ";
//                    var WIP1 = dbHMI.tblworkorderentries.SqlQuery(WIPQuery1).ToList();
//                    foreach (var row in WIP1)
//                    {
//                        int InnerOpNo = Convert.ToInt32(row.OperationNo);
//                        string ddlopno = row.OperationNo;
//                        string InnerOpNoString = Convert.ToString(row.OperationNo);
//                        if (hmiids.Contains(InnerOpNoString))
//                        { }
//                        else
//                        {
//                            if (OperationNoInt > InnerOpNo)
//                            {
//                                int PrvProcessQty = 0, PrvDeliveredQty = 0, TotalProcessQty = 0, ishold = 0;
//                                #region new code
//                                //here 1st get latest of delivered and processed among row in tblHMIScreen & tblmulitwoselection
//                                int isHMIFirst = 2; //default NO History for that wo,pn,on

//                                //var mulitwoData = db.tbl_multiwoselection.Where(m => m.WorkOrder == woNo && m.PartNo == partNo && m.OperationNo == ddlopno && m.tblhmiscreen.isWorkInProgress != 2).OrderByDescending(m => m.tblhmiscreen.Time).Take(1).ToList();
//                                var hmiData = db.tblworkorderentries.Where(m => m.Prod_Order_No == woNo && m.PartNo == partNo && m.OperationNo == ddlopno).OrderByDescending(m => m.WOEnd).Take(1).ToList();

//                                if (hmiData.Count > 0) // now check for greatest amongst
//                                {
//                                    //DateTime multiwoDateTime = Convert.ToDateTime(mulitwoData[0].tblhmiscreen.Time);
//                                    DateTime hmiDateTime = Convert.ToDateTime(hmiData[0].WOEnd);

//                                    if (Convert.ToInt32(hmiDateTime) > 0)
//                                    {
//                                        isHMIFirst = 1;
//                                    }
//                                    else
//                                    {
//                                        isHMIFirst = 0;
//                                    }

//                                }

//                                else if (hmiData.Count > 0)
//                                {
//                                    isHMIFirst = 0;
//                                }


//                                if (isHMIFirst == 0)
//                                {
//                                    string delivString = Convert.ToString(hmiData[0].Yield_Qty);
//                                    int delivInt = 0;
//                                    int.TryParse(delivString, out delivInt);

//                                    string processString = Convert.ToString(hmiData[0].ProcessQty);
//                                    int procInt = 0;
//                                    int.TryParse(processString, out procInt);

//                                    PrvProcessQty += procInt;
//                                    PrvDeliveredQty += delivInt;

//                                    ishold = hmiData[0].IsHold;
//                                    ishold = ishold == 2 ? 0 : ishold;
//                                }
//                                else
//                                {
//                                    //no previous delivered or processed qty so Do Nothing.
//                                }
//                                #endregion
//                                TotalProcessQty = PrvProcessQty + PrvDeliveredQty;
//                                //var hmiPFed = db.tblhmiscreens.Where(m => m.Work_Order_No == woNo && m.PartNo == partNo && m.OperationNo == opNo).OrderByDescending(m => m.Time).FirstOrDefault();

//                                if (Convert.ToInt32(row.Total_Qty) == TotalProcessQty)
//                                {
//                                    #region
//                                    //if (isHMIFirst == 1 && Convert.ToInt32(row.Target_Qty)>0 )
//                                    //{
//                                    //    // int hmiidmultitbl = Convert.ToInt32(mulitwoData[0].HMIID);
//                                    //    int hmiid = 0;
//                                    //    var hmiTomulittblData = db.tblworkorderentries.Find(hmiid);
//                                    //    if (hmiTomulittblData != null)
//                                    //    {
//                                    //        tblworkorderentry tblh = new tblworkorderentry();
//                                    //        tblh.CorrectedDate = row.CorrectedDate;
//                                    //        tblh.Date = DateTime.Now;
//                                    //        tblh.Time = DateTime.Now;
//                                    //        tblh.PEStartTime = DateTime.Now;
//                                    //        tblh.DDLWokrCentre = hmiTomulittblData.DDLWokrCentre;
//                                    //        tblh.Delivered_Qty = 0;
//                                    //        tblh.DoneWithRow = 1;
//                                    //        tblh.IsHold = 0;
//                                    //        tblh.IsMultiWO = 0;
//                                    //        tblh.isUpdate = 1;
//                                    //        tblh.isWorkInProgress = 1;
//                                    //        tblh.isWorkOrder = hmiTomulittblData.isWorkOrder;
//                                    //        tblh.MachineID = hmiTomulittblData.MachineID;
//                                    //        tblh.OperationNo = hmi.OperationNo;
//                                    //        tblh.OperatiorID = hmiTomulittblData.OperatiorID;
//                                    //        tblh.OperatorDet = hmiTomulittblData.OperatorDet;
//                                    //        tblh.PartNo = mulitwoData[0].PartNo;
//                                    //        tblh.ProcessQty = TotalProcessQty;
//                                    //        tblh.Prod_FAI = hmiTomulittblData.Prod_FAI;
//                                    //        tblh.Project = hmiTomulittblData.Project;
//                                    //        tblh.Rej_Qty = hmiTomulittblData.Rej_Qty;
//                                    //        tblh.Shift = hmiTomulittblData.Shift;
//                                    //        tblh.SplitWO = "No";
//                                    //        tblh.Status = hmiTomulittblData.Status;
//                                    //        tblh.Target_Qty = TotalProcessQty;
//                                    //        tblh.Work_Order_No = mulitwoData[0].WorkOrder;

//                                    //        db.tblhmiscreens.Add(tblh);
//                                    //        db.SaveChanges();
//                                    //    }

//                                    //}
//                                    if (isHMIFirst == 0 && Convert.ToInt32(row.Total_Qty) < Convert.ToInt32(hmiData[0].Total_Qty))
//                                    {
//                                        tblworkorderentry tblh = new tblworkorderentry();
//                                        tblh.CorrectedDate = row.CorrectedDate;
//                                        tblh.WOStart = DateTime.Now;
//                                        tblh.WOEnd = DateTime.Now;
//                                        tblh.PEStartTime = DateTime.Now;
//                                        //tblh.DDLWokrCentre = hmiData[0].DDLWokrCentre;
//                                        tblh.Yield_Qty = 0;
//                                        //tblh.DoneWithRow = 1;
//                                        tblh.IsHold = 0;
//                                        tblh.IsMultiWO = hmiData[0].IsMultiWO;
//                                        //tblh.isUpdate = 1;
//                                        //tblh.isWorkInProgress = 1;
//                                        tblh.isWorkOrder = hmiData[0].isWorkOrder;
//                                        tblh.MachineID = hmiData[0].MachineID;
//                                        tblh.OperationNo = hmiData[0].OperationNo;
//                                        tblh.OperatorID = hmiData[0].OperatorID;
//                                        //tblh.OperatorDet = hmiData[0].OperatorDet;
//                                        tblh.PartNo = hmiData[0].PartNo;
//                                        tblh.ProcessQty = TotalProcessQty;
//                                        // tblh.Prod_FAI = hmiData[0].Prod_FAI;
//                                        // tblh.Project = hmiData[0].Project;
//                                        tblh.ScrapQty = hmiData[0].ScrapQty;
//                                        tblh.Shift = hmiData[0].Shift;
//                                        //tblh.SplitWO = hmiData[0].SplitWO;
//                                        tblh.Status = hmiData[0].Status;
//                                        tblh.Total_Qty = TotalProcessQty;
//                                        tblh.Prod_Order_No = hmiData[0].Prod_Order_No;

//                                        db.tblworkorderentries.Add(tblh);
//                                        db.SaveChanges();
//                                    }
//                                    #endregion

//                                    // row.IsCompleted = 1;
//                                    db.Entry(row).State = System.Data.Entity.EntityState.Modified;
//                                    db.SaveChanges();
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//            #endregion

//            foreach (var rowOuter in WIPOuter)
//            {
//                int hmiid1 = Convert.ToInt32(rowOuter.HMIID);
//                #region 2017-02-07

//                string woNo = Convert.ToString(rowOuter.Prod_Order_No);
//                string opNo = Convert.ToString(rowOuter.OperationNo);
//                string partNo = Convert.ToString(rowOuter.PartNo);

//                //Logic to check sequence of JF Based on WONo, PartNo and OpNo.
//                int OperationNoInt = Convert.ToInt32(opNo);
//                var HMIData = db.tblworkorderentries.Where(m => m.HMIID == hmiid1).FirstOrDefault();
//                string wono = HMIData.Prod_Order_No;
//                string partno = HMIData.PartNo;
//                string opno = HMIData.OperationNo;
//                HMIData.Status = 2;
//                //HMIData.isWorkInProgress = 1;
//                //HMIData.SplitWO = "No";
//                HMIData.WOEnd = DateTime.Now;
//                db.Entry(HMIData).State = System.Data.Entity.EntityState.Modified;
//                db.SaveChanges();
//                // }
//            }
//            Session["isWorkOrder"] = 0;
//            return RedirectToAction("Index");
//        }

//        #endregion

//        #region Breakdown Get
//        public ActionResult BreakDownEntry(int id = 0, int Bid = 0)
//        {
//            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
//            {
//                return RedirectToAction("Login", "Login", null);
//            }

//            #region old
//            ////if tblmode or tbldailyprodstatus has breakdown in them for today then update tblbreakdown now.
//            ////will help in showing options on entry screen
//            ////this happens because we are taking last available mode for that machine and updating it for today if we don't have mode for now.

//            //var tblbreakdown = db.tblbreakdowns.Where(m => m.MachineID == id).OrderByDescending(m => m.StartTime);
//            //foreach (var row in tblbreakdown)
//            //{
//            //    string date = row.CorrectedDate;
//            //    string today = DateTime.Now.ToString("yyyy-MM-dd" );
//            //    if (date != today)
//            //    {
//            //        var tblmode = db.tblmodes.Where(m => m.MachineID == id && m.IsDeleted == 0).OrderByDescending(m => m.InsertedOn);
//            //        foreach (var rowIntblmode in tblmode)
//            //        {
//            //            if (rowIntblmode.Mode == "BREAKDOWN")
//            //            {
//            //                DateTime insertedOn = rowIntblmode.InsertedOn;

//            //            }

//            //            break;
//            //        }

//            //    }
//            //    break;
//            //}
//            //doing this in daq is better
//            #endregion

//            ViewBag.Logout = Session["Username"];
//            ViewBag.roleid = Session["RoleID"];
//            Session["Mid"] = id;
//            int machineid = Convert.ToInt32(Session["MachineID"]);
//            string CorrectedDate = null;
//            tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).SingleOrDefault();
//            TimeSpan Start = StartTime.StartTime;
//            if (Start <= DateTime.Now.TimeOfDay)
//            {
//                CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
//            }
//            else
//            {
//                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
//            }

//            var machinedispname = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == id).Select(m => m.MachineDisplayName).FirstOrDefault();
//            ViewBag.macDispName = Convert.ToString(machinedispname);

//            //Stage 1: check if we r allowd to set this mode 
//            //CODE to check the current mode is allowable or not , based on MODE Priority.
//            var curMode = db.tblbreakdowns.Where(m => m.MachineID == id && m.DoneWithRow == 0).OrderByDescending(m => m.BreakdownID).Take(1).ToList();
//            int currentId = 0;
//            foreach (var j in curMode)
//            {
//                currentId = j.BreakdownID;
//                string mode = j.tbllossescode.MessageType;

//                if (mode == "PM")
//                {
//                    Session["ModeError"] = "Machine is in Maintenance , cannot change mode to Breakdown";
//                    return RedirectToAction("Index");
//                }
//                //else if (mode == "BREAKDOWN")
//                //{
//                //    Session["ModeError"] = "Machine is in Breakdown Mode";
//                //    return RedirectToAction("Index");
//                //}
//                else if (mode != "BREAKDOWN")
//                {
//                    tblbreakdown tbd = db.tblbreakdowns.Find(currentId);
//                    tbd.EndTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
//                    db.Entry(tbd).State = System.Data.Entity.EntityState.Modified;
//                    db.SaveChanges();

//                    //tbllossofentry tle = 
//                    break;
//                }
//            }


//            //stage 2. Breakdown is running and u need to send data to view regarding that.

//            var breakdownToView = db.tblbreakdowns.Where(m => m.MachineID == machineid && m.DoneWithRow == 0).OrderByDescending(m => m.BreakdownID).FirstOrDefault();
//            if (breakdownToView != null) //implies brekdown is running
//            {
//                if (breakdownToView.DoneWithRow == 0)
//                {
//                    int breakdowncode = Convert.ToInt32(breakdownToView.BreakDownCode);
//                    var DataToView = db.tbllossescodes.Where(m => m.LossCodeID == breakdowncode).ToList();
//                    ViewBag.Level = DataToView[0].LossCodesLevel;
//                    ViewBag.BreakdownCode = DataToView[0].LossCode;
//                    ViewBag.BreakdownId = DataToView[0].LossCodeID;
//                    ViewBag.BreakdownStartTime = breakdownToView.StartTime;
//                    return View(DataToView);
//                }

//            }


//            //var brkdown = db.tblbreakdowns.Where(m => m.MachineID == id).Where(m => m.CorrectedDate == CorrectedDate && m.EndTime == null && m.message_code_master.MessageType == "BREAKDOWN");
//            //if (brkdown.Count() != 0)
//            //{
//            //    Session["ItsBreakDown"] = "yes";
//            //    int brekdnID = 0;
//            //    foreach (var a in brkdown)
//            //    {
//            //        brekdnID = a.BreakdownID;
//            //    }
//            //    tblbreakdown brekdn = db.tblbreakdowns.Find(brekdnID);
//            //    CheckLastOneHourDownTime(id);
//            //    ViewBag.BreakDownCode = new SelectList(db.message_code_master.Where(m => m.IsDeleted == 0).Where(m => m.MessageType == "BREAKDOWN"), "MessageCodeID", "MessageDescription", brekdn.BreakDownCode);
//            //    return View(brekdn);
//            //}
//            //else
//            //{

//            //}

//            //This is needed but not now.
//            //CheckLastOneHourDownTime(id);


//            //stage 3. Operator is selecting the breakdown by traversing down the Hierarchy of BreakdownCodes.
//            if (Bid != 0)
//            {
//                var breakdata = db.tbllossescodes.Find(Bid);
//                int level = breakdata.LossCodesLevel;
//                string breakdowncode = breakdata.LossCode;

//                if (level == 1)
//                {
//                    var level2Data = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel1ID == Bid && m.LossCodesLevel2ID == null && m.MessageType == "BREAKDOWN").ToList();
//                    if (level2Data.Count == 0)
//                    {
//                        var level1Data = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.LossCodesLevel1ID == null && m.LossCodesLevel2ID == null && m.MessageType == "BREAKDOWN").ToList();
//                        ViewBag.ItsLastLevel = "No Further Levels . Do you want to set " + breakdowncode + " as reason.";
//                        ViewBag.BreakDownID = Bid;
//                        ViewBag.Level = level;
//                        ViewBag.breadScrum = breakdowncode + "-->  ";
//                        return View(level1Data);
//                    }
//                    ViewBag.Level = level + 1;
//                    ViewBag.BreakDownID = Bid;
//                    ViewBag.breadScrum = breakdowncode + "-->  ";
//                    return View(level2Data);
//                }
//                else if (level == 2)
//                {
//                    var level3Data = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel2ID == Bid && m.MessageType == "BREAKDOWN").ToList();
//                    int prevLevelId = Convert.ToInt32(breakdata.LossCodesLevel1ID);
//                    var level1data = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodeID == prevLevelId).Select(m => m.LossCode).SingleOrDefault();
//                    if (level3Data.Count == 0)
//                    {
//                        var level2Data = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel1ID == prevLevelId && m.MessageType == "BREAKDOWN" && m.LossCodesLevel2ID == null).ToList();
//                        ViewBag.ItsLastLevel = "No Further Levels . Do you want to set " + breakdowncode + " as reason.";
//                        ViewBag.BreakDownID = Bid;
//                        ViewBag.Level = level;
//                        ViewBag.breadScrum = level1data + " --> " + breakdowncode + "-->  ";
//                        return View(level2Data);
//                    }
//                    ViewBag.Level = level + 1;
//                    ViewBag.BreakDownID = Bid;
//                    ViewBag.breadScrum = level1data + " --> " + breakdowncode + "-->  ";
//                    return View(level3Data);
//                }
//                else if (level == 3)
//                {
//                    int prevLevelId = Convert.ToInt32(breakdata.LossCodesLevel2ID);
//                    int FirstLevelID = Convert.ToInt32(breakdata.LossCodesLevel1ID);
//                    var level2scrum = db.tbllossescodes.Where(m => m.LossCodeID == prevLevelId).Select(m => m.LossCode).SingleOrDefault();
//                    var level1scrum = db.tbllossescodes.Where(m => m.LossCodeID == FirstLevelID).Select(m => m.LossCode).SingleOrDefault();
//                    var level2Data = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel2ID == prevLevelId && m.MessageType == "BREAKDOWN").ToList();
//                    ViewBag.ItsLastLevel = "No Further Levels . Do you want to set " + breakdowncode + " as reason.";
//                    ViewBag.BreakDownID = Bid;
//                    ViewBag.Level = 3;
//                    ViewBag.breadScrum = level1scrum + " --> " + level2scrum + "--> ";
//                    return View(level2Data);
//                }
//            }
//            else
//            {
//                var level1Data = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.MessageType == "BREAKDOWN" && m.LossCode != "9999").ToList();
//                ViewBag.Level = 1;
//                return View(level1Data);
//            }

//            //Fail Safe: if everything else fails send level1 codes.
//            ViewBag.Level = 1;
//            var level10Data = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.MessageType == "BREAKDOWN" && m.LossCode != "9999").ToList();
//            return View(level10Data);
//        }

//        #endregion

//        #region Breakdown Post
//        [HttpPost]
//        public ActionResult BreakDownEntry(tbllossescode tbdc, string EndBreakdown = null, int HiddenID = 0)
//        {
//            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
//            {
//                return RedirectToAction("Login", "Login", null);
//            }
//            //"EndBreakdown" is for insert new row or update old one. Basically speeking its like start and Stop of Breakdown.
//            //"HiddenID" is the BreakdownID of row to be set as reason.

//            int MachineID = Convert.ToInt32(Session["MachineID"]);
//            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
//            {
//                return RedirectToAction("Login", "Login", null);
//            }
//            ViewBag.Logout = Session["Username"];
//            ViewBag.roleid = Session["RoleID"];
//            int RID = Convert.ToInt32(Session["RoleID"]);
//            string CorrectedDate = null;
//            tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).SingleOrDefault();
//            TimeSpan Start = StartTime.StartTime;
//            if (Start <= DateTime.Now.TimeOfDay)
//            {
//                CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
//            }
//            else
//            {
//                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
//            }

//            DateTime Time = DateTime.Now;
//            TimeSpan Tm = new TimeSpan(Time.Hour, Time.Minute, Time.Second);
//            var ShiftDetails = db.tblshift_mstr.Where(m => m.StartTime <= Tm && m.EndTime >= Tm);
//            string Shift = "C";
//            foreach (var a in ShiftDetails)
//            {
//                Shift = a.ShiftName;
//            }


//            if (HiddenID != 0 && string.IsNullOrEmpty(EndBreakdown) == true)
//            {
//                var breakdata = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodeID == HiddenID).SingleOrDefault();
//                string msgCode = breakdata.LossCode;
//                string msgDesc = breakdata.LossCodeDesc;

//                tblbreakdown tb = new tblbreakdown();
//                tb.BreakDownCode = HiddenID;
//                tb.CorrectedDate = CorrectedDate;
//                tb.DoneWithRow = 0;
//                tb.MachineID = Convert.ToInt32(Session["MachineID"]);
//                tb.MessageCode = msgCode;
//                tb.MessageDesc = msgDesc;
//                tb.Shift = Shift;
//                tb.StartTime = DateTime.Now;
//                db.tblbreakdowns.Add(tb);
//                db.SaveChanges();

//                //Code to End PreviousMode(Production Here) & save this event to tblmode table
//                var modedata = db.tblmodes.Where(m => m.MachineID == MachineID && m.IsCompleted == 0).OrderByDescending(m => m.StartTime).FirstOrDefault();
//                if (modedata != null)
//                {
//                    modedata.IsCompleted = 1;
//                    modedata.EndTime = DateTime.Now;
//                    db.Entry(modedata).State = System.Data.Entity.EntityState.Modified;
//                    db.SaveChanges();
//                }

//                tblmode tm = new tblmode();
//                tm.MachineID = Convert.ToInt32(Session["MachineID"]);
//                tm.CorrectedDate = CorrectedDate;
//                tm.InsertedBy = Convert.ToInt32(Session["UserId"]);
//                tm.StartTime = DateTime.Now;
//                tm.ColorCode = "red";
//                tm.InsertedOn = DateTime.Now;
//                tm.IsDeleted = 0;
//                tm.MacMode = "BREAKDOWN";
//                tm.IsCompleted = 0;

//                db.tblmodes.Add(tm);
//                db.SaveChanges();

//            }
//            else if (HiddenID != 0 && string.IsNullOrEmpty(EndBreakdown) == false)
//            {
//                var tb = db.tblbreakdowns.Where(m => m.BreakDownCode == HiddenID && m.MachineID == MachineID && m.DoneWithRow == 0).OrderByDescending(m => m.BreakdownID).FirstOrDefault();
//                tb.EndTime = DateTime.Now;
//                tb.DoneWithRow = 1;

//                db.Entry(tb).State = System.Data.Entity.EntityState.Modified;
//                db.SaveChanges();

//                //get the latest row and update it.
//                var modedata = db.tblmodes.Where(m => m.MachineID == MachineID && m.IsCompleted == 0).OrderByDescending(m => m.StartTime).FirstOrDefault();
//                if (modedata != null)
//                {
//                    modedata.IsCompleted = 1;
//                    modedata.EndTime = DateTime.Now;
//                    db.Entry(modedata).State = System.Data.Entity.EntityState.Modified;
//                    db.SaveChanges();
//                }

//                tblmode tmIDLE = new tblmode();
//                tmIDLE.ColorCode = "yellow";
//                tmIDLE.CorrectedDate = CorrectedDate;
//                tmIDLE.InsertedBy = Convert.ToInt32(Session["UserId"]);
//                tmIDLE.InsertedOn = DateTime.Now;
//                tmIDLE.IsCompleted = 0;
//                tmIDLE.IsDeleted = 0;
//                tmIDLE.MachineID = MachineID;
//                tmIDLE.MacMode = "IDLE";
//                tmIDLE.StartTime = DateTime.Now;

//                db.tblmodes.Add(tmIDLE);
//                db.SaveChanges();
//            }

//            #region OLD
//            //if (string.IsNullOrEmpty(submit) == false)
//            //{
//            //    lossentry.CorrectedDate = CorrectedDate;
//            //    lossentry.StartTime = DateTime.Now;
//            //    if (RID != 1 && RID != 2)
//            //    {
//            //        lossentry.MachineID = Convert.ToInt32(Session["MachineID"]);
//            //        MachineID = Convert.ToInt32(Session["MachineID"]);
//            //    }
//            //    else
//            //    {
//            //        lossentry.MachineID = Convert.ToInt32(Session["Mid"]);
//            //        MachineID = Convert.ToInt32(Session["Mid"]);
//            //    }
//            //    message_code_master downcode = db.message_code_master.Find(lossentry.BreakDownCode);
//            //    //lossentry.BreakDownCode = Convert.ToInt32(downcode.MessageCode);
//            //    lossentry.Shift = Session["realshift"].ToString();
//            //    //lossentry.BreakDownCode =Convert.ToInt32(downcode.MessageCode);
//            //    lossentry.MessageCode = (downcode.MessageCode).ToString();
//            //    db.tblbreakdowns.Add(lossentry);
//            //    db.SaveChanges();

//            //    //update the endtime for the last mode of this machine 
//            //    var tblmodedata = db.tblmodes.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID).OrderByDescending(m => m.StartTime).ToList();
//            //    foreach (var row in tblmodedata)
//            //    {
//            //        row.EndTime = DateTime.Now;
//            //        db.Entry(row).State = System.Data.Entity.EntityState.Modified;
//            //        db.SaveChanges();
//            //    }

//            //    //Code to save this event to tblmode table
//            //    tblmode tm = new tblmode();
//            //    tm.MachineID = MachineID;
//            //    tm.CorrectedDate = CorrectedDate;
//            //    tm.InsertedBy = 1;
//            //    tm.StartTime = DateTime.Now;
//            //    tm.ColorCode = "red";
//            //    tm.InsertedOn = DateTime.Now;

//            //    tm.IsDeleted = 0;
//            //    tm.Mode = "BREAKDOWN";

//            //    db.tblmodes.Add(tm);
//            //    db.SaveChanges();


//            //    //SendMail(downcode.MessageCode, downcode.MessageDescription, MachineID);
//            //    return RedirectToAction("Index");
//            //}
//            //else
//            //{
//            //    lossentry.CorrectedDate = CorrectedDate;
//            //    lossentry.EndTime = DateTime.Now;
//            //    MachineID = Convert.ToInt32(lossentry.MachineID);
//            //    //lossentry.MachineID = Convert.ToInt32(Session["MachineID"]);
//            //    db.Entry(lossentry).State = System.Data.Entity.EntityState.Modified;
//            //    db.SaveChanges();
//            //    UpdateRecordOfProduction(lossentry);
//            //    int code = Convert.ToInt32(lossentry.BreakDownCode);
//            //    message_code_master msg = db.message_code_master.Where(m => m.MessageCodeID == code).SingleOrDefault();
//            //    //SendMailEnd(msg.MessageCode, msg.MessageDescription, MachineID);
//            //    return RedirectToAction("Index");
//            //}
//            ////return View(lossentry);
//            #endregion
//            return RedirectToAction("Index");
//        }
//        #endregion

//        #region Breakdownlist
//        public ActionResult BreakDownList(int id = 0)
//        {
//            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
//            {
//                return RedirectToAction("Login", "Login", null);
//            }
//            ViewBag.Logout = Session["Username"];
//            ViewBag.roleid = Session["RoleID"];
//            string CorrectedDate = null;
//            tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).SingleOrDefault();
//            TimeSpan Start = StartTime.StartTime;
//            if (Start <= DateTime.Now.TimeOfDay)
//            {
//                CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
//            }
//            else
//            {
//                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
//            }
//            ViewBag.opid = Session["opid"];
//            ViewBag.mcnid = id;
//            ViewBag.coretddt = CorrectedDate;

//            //bool tick = checkingIdle();
//            //if (tick == true)
//            //{
//            //    return RedirectToAction("DownCodeEntry");
//            //    //ViewBag.tick = 1;
//            //}

//            int handleidleReturnValue = HandleIdle();
//            if (handleidleReturnValue == 0)
//            {
//                return RedirectToAction("DownCodeEntry");
//            }

//            var machinedispname = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == id).Select(m => m.MachineDisplayName).FirstOrDefault();
//            ViewBag.macDispName = Convert.ToString(machinedispname);

//            //var breakdown = db.tblbreakdowns.Include(t=>t.machine_master).Include(t=>t.message_code_master).Where(m=>m.MachineID==id && m.CorrectedDate==CorrectedDate).ToList();
//            var breakdown = db.tblbreakdowns.Where(m => m.MachineID == id && m.CorrectedDate == CorrectedDate && m.DoneWithRow == 1).OrderByDescending(m => m.StartTime).ToList();
//            return View(breakdown);
//        }

//        #endregion

//        #region Maintainance Get

//        public ActionResult Maintenance(int id = 0)
//        {
//            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
//            {
//                return RedirectToAction("Login", "Login", null);
//            }
//            ViewBag.Logout = Session["Username"];
//            ViewBag.roleid = Session["RoleID"];
//            Session["Mid"] = id;
//            string CorrectedDate = null;
//            tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).SingleOrDefault();
//            TimeSpan Start = StartTime.StartTime;
//            if (Start <= DateTime.Now.TimeOfDay)
//            {
//                CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
//            }
//            else
//            {
//                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
//            }

//            var machinedispname = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == id).Select(m => m.MachineDisplayName).FirstOrDefault();
//            ViewBag.macDispName = Convert.ToString(machinedispname);

//            //CODE to check the current mode is allowable or not , based on MODE Priority.
//            var curMode = db.tblbreakdowns.Where(m => m.MachineID == id).Where(m => m.CorrectedDate == CorrectedDate && m.EndTime == null).OrderByDescending(m => m.BreakdownID);
//            int currentId = 0;

//            foreach (var j in curMode)
//            {
//                currentId = j.BreakdownID;
//                string mode = j.MessageCode;
//                if (mode != "PM")
//                {
//                    currentId = j.BreakdownID;
//                    tblbreakdown tbd = db.tblbreakdowns.Find(currentId);
//                    tbd.EndTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
//                    db.Entry(tbd).State = System.Data.Entity.EntityState.Modified;
//                    db.SaveChanges();
//                    break;
//                }
//                //else if (mode == "PM")
//                //{
//                //    Session["ModeError"] = "Machine is in Planned Maintenance Mode";
//                //    return RedirectToAction("Index");
//                //}

//            }

//            //var brkdown = db.tblbreakdowns.Where(m => m.MachineID == id).Where(m => m.CorrectedDate == CorrectedDate && m.EndTime == null && m.message_code_master.MessageType == "PM");

//            var brkdown = db.tblbreakdowns.Where(m => m.MachineID == id).Where(m => m.EndTime == null && m.MessageCode == "PM");
//            if (brkdown.Count() != 0)
//            {
//                TempData["Enable"] = "Enable";
//                int brekdnID = 0;
//                foreach (var a in brkdown)
//                {
//                    brekdnID = a.BreakdownID;
//                }
//                tblbreakdown brekdn = db.tblbreakdowns.Find(brekdnID);
//                //CheckLastOneHourDownTime(id);
//                ViewBag.BreakDownCode = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0).Where(m => m.MessageType == "PM"), "LossCode", "LossCodeDesc", brekdn.BreakDownCode);
//                return View(brekdn);
//            }
//            else
//            {
//            }
//            //CheckLastOneHourDownTime(id);
//            ViewBag.BreakDownCode = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0).Where(m => m.MessageType == "PM"), "LossCode", "LossCodeDesc");
//            return View();
//        }
//        #endregion

//        #region Maintainance Post
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult Maintenance(tblbreakdown lossentry, string submit = "", string BreakDownCode = null)
//        {
//            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
//            {
//                return RedirectToAction("Login", "Login", null);
//            }
//            int MachineID = 0;
            
//            ViewBag.Logout = Session["Username"];
//            ViewBag.roleid = Session["RoleID"];
//            int RID = Convert.ToInt32(Session["RoleID"]);
//            string CorrectedDate = null;
//            tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).SingleOrDefault();
//            TimeSpan Start = StartTime.StartTime;
//            if (Start <= DateTime.Now.TimeOfDay)
//            {
//                CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
//            }
//            else
//            {
//                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
//            }
//            if (string.IsNullOrEmpty(submit) == false && submit == "Start")
//            {
//                lossentry.CorrectedDate = CorrectedDate;
//                lossentry.StartTime = DateTime.Now;
//                if (RID != 1 && RID != 2)
//                {
//                    lossentry.MachineID = Convert.ToInt32(Session["MachineID"]);
//                    MachineID = Convert.ToInt32(Session["MachineID"]);
//                }
//                else
//                {
//                    lossentry.MachineID = Convert.ToInt32(Session["Mid"]);
//                    MachineID = Convert.ToInt32(Session["Mid"]);
//                }
//                //message_code_master downcode = db.message_code_master.Find(lossentry.BreakDownCode);
//                var LossData = db.tbllossescodes.Where(m => m.LossCode == BreakDownCode).FirstOrDefault();
//                lossentry.Shift = Session["realshift"].ToString();
//                lossentry.MessageCode = (LossData.LossCode).ToString();
//                lossentry.BreakDownCode = 120;
//                lossentry.DoneWithRow = 0;
//                lossentry.MessageDesc = "PM";
//                db.tblbreakdowns.Add(lossentry);
//                db.SaveChanges();
//                //SendMail(downcode.MessageCode, downcode.MessageDescription, MachineID);

//                //update the endtime for the last mode of this machine 
//                var tblmodedata = db.tblmodes.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID && m.IsCompleted == 0).OrderByDescending(m => m.StartTime).ToList();
//                foreach (var row in tblmodedata)
//                {
//                    row.EndTime = DateTime.Now;
//                    row.IsCompleted = 1;
//                    db.Entry(row).State = System.Data.Entity.EntityState.Modified;
//                    db.SaveChanges();


//                }
//                //Code to save this event to tblmode table
//                tblmode tm = new tblmode();
//                tm.MachineID = MachineID;
//                tm.CorrectedDate = CorrectedDate;
//                tm.InsertedBy = Convert.ToInt32(Session["UserId"]);
//                tm.InsertedOn = DateTime.Now;
//                tm.StartTime = DateTime.Now;
//                tm.ColorCode = "red";
//                tm.IsCompleted = 0;
//                tm.IsDeleted = 0;
//                tm.MacMode = "BREAKDOWN";

//                db.tblmodes.Add(tm);
//                db.SaveChanges();

//                return RedirectToAction("Index");
//            }
//            else
//            {
//                lossentry.CorrectedDate = CorrectedDate;
//                lossentry.EndTime = DateTime.Now;
//                lossentry.DoneWithRow = 1;
//                MachineID = Convert.ToInt32(lossentry.MachineID);
//                try
//                {
//                    db.Entry(lossentry).State = System.Data.Entity.EntityState.Modified;
//                    db.SaveChanges();

//                    var tblmodedata = db.tblmodes.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID && m.IsCompleted == 0).OrderByDescending(m => m.StartTime).ToList();
//                    foreach (var row in tblmodedata)
//                    {
//                        row.EndTime = DateTime.Now;
//                        row.IsCompleted = 1;
//                        db.Entry(row).State = System.Data.Entity.EntityState.Modified;
//                        db.SaveChanges();
//                    }

//                    tblmode tmIDLE = new tblmode();
//                    tmIDLE.ColorCode = "yellow";
//                    tmIDLE.CorrectedDate = CorrectedDate;
//                    tmIDLE.InsertedBy = Convert.ToInt32(Session["UserId"]);
//                    tmIDLE.InsertedOn = DateTime.Now;
//                    tmIDLE.IsCompleted = 0;
//                    tmIDLE.IsDeleted = 0;
//                    tmIDLE.MachineID = MachineID;
//                    tmIDLE.MacMode = "IDLE";
//                    tmIDLE.StartTime = DateTime.Now;

//                    db.tblmodes.Add(tmIDLE);
//                    db.SaveChanges();

//                }
//                catch (Exception e)
//                { }
//                //UpdateRecordOfProduction(lossentry);
//                //int code = Convert.ToInt32(lossentry.BreakDownCode);
//                // message_code_master msg = db.message_code_master.Where(m => m.MessageCodeID == code).SingleOrDefault();
//                //SendMailEnd(msg.MessageCode, msg.MessageDescription, MachineID);
//                return RedirectToAction("Index");
//            }
//            return View(lossentry);
//        }

//        #endregion

//        //IdleList   //Idle state in OperatorEntry will be calculated automatically once idle state will end it will be shown in idlelist
//        //public ActionResult IdleList(int id = 0)
//        //{
//        //    if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
//        //    {
//        //        return RedirectToAction("Login", "Login", null);
//        //    }
//        //    ViewBag.Logout = Session["Username"];
//        //    ViewBag.roleid = Session["RoleID"];
//        //    string CorrectedDate = null, Shift = null;
//        //    string[] DateShiftValues = GetDateShift();
//        //    CorrectedDate = DateShiftValues[1];
//        //    Shift = DateShiftValues[0];
//        //    //int handleidleReturnValue = HandleIdleManualWC();
//        //    //if (handleidleReturnValue == 0)
//        //    //{
//        //    //    return RedirectToAction("DownCodeEntry");
//        //    //}

//        //    var machinedispname = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == id).Select(m => m.MachineDisplayName).FirstOrDefault();
//        //    ViewBag.macDispName = Convert.ToString(machinedispname);
//        //    var vm = new HoldList();
//        //    ViewBag.coretddt = CorrectedDate;
//        //    //var idle = db.tbllossofentries.Include(t => t.machine_master).Include(t => t.message_code_master).Where(m => m.MachineID == id && m.CorrectedDate == CorrectedDate).ToList();
//        //    //vm.HoldListDetailsWO = db.tblmanuallossofentries.Include(t => t.tblholdcode).Where(m => m.MachineID == id && m.CorrectedDate == CorrectedDate).ToList();
//        //    vm.HoldListDetailsWC = db.tbllossofentries.Include(t => t.tbllossescode).Where(m => m.MachineID == id && m.CorrectedDate == CorrectedDate && m.DoneWithRow == 1).OrderByDescending(m => m.StartDateTime).ToList();

//        //    #region another technique. didnot finish
//        //    ////    HoldCodesList : HoldCodeID  RL1  RL2  RL3  IsWOLevel 
//        //    //List<HoldCodesList> HoldCodesToView = new List<HoldCodesList>();
//        //    //foreach (var row in WOLossDetails)
//        //    //{
//        //    //    int LevelCurrentCodeInt = row.MessageCodeID;
//        //    //    string Level1CodeString = row.MessageCode;
//        //    //    string Level2CodeString = null, Level3CodeString = null;

//        //    //    var LossCodeDetails = db.tbllossescodes.Where(m => m.LossCodeID == LevelCurrentCodeInt).FirstOrDefault();

//        //    //    int Level1CodeInt = 0;
//        //    //    if (int.TryParse(Convert.ToString(LossCodeDetails.LossCodesLevel1ID), out Level1CodeInt))
//        //    //    {
//        //    //        Level2CodeString = db.tbllossescodes.Where(m => m.LossCodeID == Level1CodeInt).Select(m => m.LossCode).FirstOrDefault();
//        //    //    }

//        //    //    int Level2CodeInt = 0;
//        //    //    if (int.TryParse(Convert.ToString(LossCodeDetails.LossCodesLevel2ID), out Level2CodeInt))
//        //    //    {
//        //    //        Level2CodeString = db.tbllossescodes.Where(m => m.LossCodeID == Level2CodeInt).Select(m => m.LossCode).FirstOrDefault();
//        //    //    }

//        //    //    HoldCodesToView.Add(new HoldCodesList { HoldCodeID = LevelCurrentCodeInt , RL1 = Level1CodeString , RL2 = Level2CodeString , RL3 = Level3CodeString ,  IsWOLevel = 1 });
//        //    //}

//        //    //var LossDataDetails = db.tbllossofentries.Where(m => m.MachineID == id && m.CorrectedDate == CorrectedDate).ToList();
//        //    //foreach (var row in LossDataDetails)
//        //    //{
//        //    //    int LevelCurrentCodeInt = row.MessageCodeID;
//        //    //    string Level1CodeString = row.MessageCode;
//        //    //    string Level2CodeString = null, Level3CodeString = null;

//        //    //    var LossCodeDetails = db.tbllossescodes.Where(m => m.LossCodeID == LevelCurrentCodeInt).FirstOrDefault();

//        //    //    int Level1CodeInt = 0;
//        //    //    if (int.TryParse(Convert.ToString(LossCodeDetails.LossCodesLevel1ID), out Level1CodeInt))
//        //    //    {
//        //    //        Level2CodeString = db.tbllossescodes.Where(m => m.LossCodeID == Level1CodeInt).Select(m => m.LossCode).FirstOrDefault();
//        //    //    }

//        //    //    int Level2CodeInt = 0;
//        //    //    if (int.TryParse(Convert.ToString(LossCodeDetails.LossCodesLevel2ID), out Level2CodeInt))
//        //    //    {
//        //    //        Level2CodeString = db.tbllossescodes.Where(m => m.LossCodeID == Level2CodeInt).Select(m => m.LossCode).FirstOrDefault();
//        //    //    }
//        //    //    HoldCodesToView.Add(new HoldCodesList { HoldCodeID = LevelCurrentCodeInt, RL1 = Level1CodeString, RL2 = Level2CodeString, RL3 = Level3CodeString, IsWOLevel = 0 });
//        //    //}
//        //    #endregion

//        //    //var HoldCodesToView = new HoldList() { HoldListDetailsWO = WOLossDetails, HoldListDetailsWC = LossDataDetails };
//        //    return View(vm);
//        //}

//        #region Rework Order

//        public JsonResult ReworkOrderClicked(int HMIID)
//        {
//            var nothing = 1;
//            //Session["FromDDL"] = 2;
//            var thisrow = db.tblworkorderentries.Find(HMIID);
//            thisrow.isWorkOrder = 1;
//            db.Entry(thisrow).State = System.Data.Entity.EntityState.Modified;
//            db.SaveChanges();
//            return Json(nothing, JsonRequestBehavior.AllowGet);
//        }
//        #endregion

//        #region AutoSave All textbox
//        public JsonResult AutoSave(int HMIID, string field, string value)
//        {
//            //var selectedRow = new SelectList(db.tblmachinedetails.Where(m => m.ShopNo == countryId).Where(m => m.IsDeleted == 0), "MachineDispName", "MachineDispName");
//            //int hmiid = HMIID - 1;
//            int NotGoodPart = 0, Sum = 0, GoodPart = 0;
//            var thisrow = db.tblworkorderentries.Where(m => m.HMIID == HMIID).ToList();
//            var nothing = 1;
//            switch (field)
//            {
//                case "cjtextboxshift":
//                    {
//                        thisrow[0].Shift = value;
//                        break;
//                    }
//                case "cjtextboxop":
//                    {
//                        if (value != "")
//                        {
//                            thisrow[0].OperatorID = value;
//                            thisrow[0].PEStartTime = DateTime.Now;
//                        }
//                        else
//                        {
//                            thisrow[0].OperatorID = null;
//                            thisrow[0].PEStartTime = DateTime.Now;
//                        }
//                        break;
//                    }

//                case "cjtextbox3":
//                    {
//                        if (value != "")
//                        {
//                            thisrow[0].PartNo = value;
//                        }
//                        else
//                        {
//                            thisrow[0].PartNo = null;
//                        }
//                        break;
//                    }
//                case "cjtextbox4":
//                    {
//                        if (value != "")
//                        {
//                            thisrow[0].Prod_Order_No = value;
//                        }
//                        else
//                        {
//                            thisrow[0].Prod_Order_No = null;
//                        }
//                        break;
//                    }
//                case "cjtextbox5":
//                    {
//                        if (value != "")
//                        {
//                            thisrow[0].OperationNo = value;
//                        }
//                        else
//                        {
//                            thisrow[0].OperationNo = null;
//                        }
//                        break;
//                    }
//                case "cjtextbox6":
//                    {
//                        if (value != "")
//                        {
//                            thisrow[0].Total_Qty = Convert.ToInt32(value);
//                        }
//                        else
//                        {
//                            thisrow[0].Total_Qty = null;
//                        }
//                        break;
//                    }

//                case "txtGoodpart":
//                    {
//                        if (value != "")
//                        {
//                            thisrow[0].Yield_Qty = Convert.ToInt32(value);
//                            Session["GoodPart"] = Convert.ToInt32(value);
//                            GoodPart = Convert.ToInt32(Session["GoodPart"]);
//                            NotGoodPart = Convert.ToInt32(Session["NotGoodPart"]);
//                            Sum = GoodPart + NotGoodPart;
//                            thisrow[0].Total_Qty = Sum;

//                        }
//                        else
//                        {
//                            thisrow[0].Yield_Qty = null;
//                        }
//                        break;
//                    }
//                case "txtnotGoodpart":
//                    {
//                        if (value != "")
//                        {
//                            thisrow[0].ScrapQty = Convert.ToInt32(value);
//                            Session["NotGoodPart"] = Convert.ToInt32(value);
//                            GoodPart = Convert.ToInt32(Session["GoodPart"]);
//                            NotGoodPart = Convert.ToInt32(Session["NotGoodPart"]);
//                            Sum = GoodPart + NotGoodPart;
//                            thisrow[0].Total_Qty = Sum;
//                        }
//                        else
//                        {
//                            thisrow[0].ScrapQty = null;
//                        }
//                        break;
//                    }

//                default:
//                    {
//                        break;
//                    }
//            }

//            db.Entry(thisrow[0]).State = System.Data.Entity.EntityState.Modified;
//            db.SaveChanges();

//            return Json(nothing, JsonRequestBehavior.AllowGet);
//        }
//        #endregion

//        #region AutoSave Operator Details
//        public JsonResult AutoSaveOPName(string field, string value)
//        {
//            //var selectedRow = new SelectList(db.tblmachinedetails.Where(m => m.ShopNo == countryId).Where(m => m.IsDeleted == 0), "MachineDispName", "MachineDispName");
//            //int hmiid = HMIID - 1;
//            int macID = Convert.ToInt32(Session["MachineID"]);
//            var thisrow = db.tblworkorderentries.Where(m => m.MachineID == macID && m.Status == 0).ToList();
//            var nothing = 1;

//            foreach (var row in thisrow)
//            {
//                switch (field)
//                {
//                    case "cjtextboxshift":
//                        {
//                            row.Shift = value;
//                            Session["OpName"] = value;
//                            break;
//                        }
//                    case "cjtextboxop":
//                        {
//                            row.OperatorID = value;
//                            row.PEStartTime = DateTime.Now;
//                            break;
//                        }
//                    default:
//                        {
//                            break;
//                        }
//                }

//                db.Entry(row).State = System.Data.Entity.EntityState.Modified;
//                db.SaveChanges();
//            }
//            return Json(nothing, JsonRequestBehavior.AllowGet);
//        }
//        #endregion

//        #region Autosave split work order

//        public JsonResult AutoSaveSplitWO(int HMIID, string SplitWO)
//        {
//            bool retStatus = false;
//            var thisrow = db.tblworkorderentries.Where(m => m.HMIID == HMIID).FirstOrDefault();
//            if (thisrow != null)
//            {
//                if (!string.IsNullOrEmpty(thisrow.PartNo) && !string.IsNullOrEmpty(thisrow.Prod_Order_No))
//                {
//                    if (SplitWO.Equals("Yes") || SplitWO.Equals("No"))
//                    {
//                        retStatus = true;
//                        //thisrow.SplitWO = SplitWO;
//                        //db.Entry(thisrow).State = System.Data.Entity.EntityState.Modified;
//                        //db.SaveChanges();
//                    }
//                }
//            }
//            return Json(retStatus, JsonRequestBehavior.AllowGet);
//        }
//        #endregion

//        public int HandleIdle()
//        {
//            int status = -1;
//            int doneWithRow = -1;//some default value
//            int isUpdate = -1;
//            int lossid = -1;
//            int isStart = 0, isScreen = 0;
//            int machineid = Convert.ToInt32(Session["MachineID"]);
//            int userid = Convert.ToInt16(Session["UserID"]);
//            DateTime endTime = DateTime.Now, startTime = DateTime.Now;
//            string shift = null;
//            string LCorrectedDate, todaysCorrectedDate;

//            todaysCorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
//            LCorrectedDate = todaysCorrectedDate;//dummy initializaition;
//            //correcteddate
//            string correcteddate = null;
//            tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).SingleOrDefault();
//            TimeSpan Start = StartTime.StartTime;
//            if (Start <= DateTime.Now.TimeOfDay)
//            {
//                correcteddate = DateTime.Now.ToString("yyyy-MM-dd");
//            }
//            else
//            {
//                correcteddate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
//            }

//            //shift
//            DateTime Time = DateTime.Now;
//            TimeSpan Tm = new TimeSpan(Time.Hour, Time.Minute, Time.Second);
//            var ShiftDetails = db.tblshift_mstr.Where(m => m.StartTime <= Tm && m.EndTime >= Tm);
//            string Shift = null;
//            foreach (var a in ShiftDetails)
//            {
//                Shift = a.ShiftName;
//            }

//            var lossStatusData = db.tbllossofentries.Where(m => m.MachineID == machineid).OrderByDescending(m => m.StartDateTime).FirstOrDefault();
//            if (lossStatusData != null)
//            {
//                lossid = lossStatusData.LossID;
//                doneWithRow = lossStatusData.DoneWithRow;
//                isUpdate = lossStatusData.IsUpdate;
//                endTime = Convert.ToDateTime(lossStatusData.EndDateTime);
//                startTime = Convert.ToDateTime(lossStatusData.StartDateTime);
//                shift = lossStatusData.Shift;
//                isStart = Convert.ToInt32(lossStatusData.IsStart);
//                isScreen = Convert.ToInt32(lossStatusData.IsScreen);
//                LCorrectedDate = lossStatusData.CorrectedDate;
//            }

//            if (doneWithRow == 0 && isUpdate == 0 && isStart == 1 && isScreen == 0)
//            {
//                string x = Convert.ToString(Session["showIdlePopUp"]);
//                int value;
//                if (int.TryParse(x, out value))
//                {
//                }
//                else
//                {
//                    Session["showIdlePopUp"] = 0;
//                }
//                status = 0;
//            }
//            else if (doneWithRow == 0 && isUpdate == 1 && isStart == 1 && isScreen == 1)
//            {
//                //don't add code to show popup
//                Session["showIdlePopUp"] = 2;
//                status = 0;
//            }

//            else if (doneWithRow == 1 && isUpdate == 1 && isStart == 0 && isScreen == 0)
//            {
//                RedirectToAction("Index");
//                Session["showIdlePopUp"] = 0;
//                status = 1;
//            }
//            else if (doneWithRow == 0 && isUpdate == 1 && isStart == 1 && isScreen == 0)
//            {
//                RedirectToAction("Index");

//                Session["showIdlePopUp"] = 0;
//                status = 1;
//            }

//            return status;
//        }

//        public bool IDLEorGenericWorkisON()
//        {
//            //Check if the Machine is in IDLE or GenericWork , We shouldn't allow them to do other activities if these are ON.
//            bool ItsOn = false;
//            int MacID = Convert.ToInt32(Session["MachineID"]);
//            var GWToView = db.tblgenericworkentries.Where(m => m.MachineID == MacID).OrderByDescending(m => m.GWEntryID).FirstOrDefault();
//            if (GWToView != null) //implies genericwork is running
//            {
//                if (GWToView.DoneWithRow == 0)
//                {
//                    ItsOn = true;
//                    return true;
//                }
//            }

//            var IdleToView = db.tbllossofentries.Where(m => m.MachineID == MacID).OrderByDescending(m => m.LossID).FirstOrDefault();
//            if (IdleToView != null) //implies idle is running
//            {
//                if (IdleToView.DoneWithRow == 0)
//                {
//                    ItsOn = true;
//                    return true;
//                }
//            }

//            return false;
//        }

//        public string[] GetDateShift()
//        {

//            string[] dateShift = new string[2];
//            //Get CorrectedDate &shift

//            #region
//            string Shift = null;
//            MsqlConnection mcp = new MsqlConnection();
//            mcp.open();
//            String queryshift = "SELECT ShiftName,StartTime,EndTime FROM tblshift_mstr WHERE IsDeleted = 0";
//            SqlDataAdapter dashift = new SqlDataAdapter(queryshift, mcp.sqlConnection);
//            DataTable dtshift = new DataTable();
//            dashift.Fill(dtshift);
//            String[] msgtime = System.DateTime.Now.TimeOfDay.ToString().Split(':');
//            //TimeSpan msgstime = System.DateTime.Now.TimeOfDay;
//            TimeSpan msgstime = new TimeSpan(Convert.ToInt32(msgtime[0]), Convert.ToInt32(msgtime[1]), Convert.ToInt32(msgtime[2]));
//            TimeSpan s1t1 = new TimeSpan(0, 0, 0), s1t2 = new TimeSpan(0, 0, 0), s2t1 = new TimeSpan(0, 0, 0), s2t2 = new TimeSpan(0, 0, 0);
//            TimeSpan s3t1 = new TimeSpan(0, 0, 0), s3t2 = new TimeSpan(0, 0, 0), s3t3 = new TimeSpan(0, 0, 0), s3t4 = new TimeSpan(23, 59, 59);
//            for (int k = 0; k < dtshift.Rows.Count; k++)
//            {
//                if (dtshift.Rows[k][0].ToString().Contains("A"))
//                {
//                    String[] s1 = dtshift.Rows[k][1].ToString().Split(':');
//                    s1t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
//                    String[] s11 = dtshift.Rows[k][2].ToString().Split(':');
//                    s1t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
//                }
//                else if (dtshift.Rows[k][0].ToString().Contains("B"))
//                {
//                    String[] s1 = dtshift.Rows[k][1].ToString().Split(':');
//                    s2t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
//                    String[] s11 = dtshift.Rows[k][2].ToString().Split(':');
//                    s2t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
//                }
//                else if (dtshift.Rows[k][0].ToString().Contains("C"))
//                {
//                    String[] s1 = dtshift.Rows[k][1].ToString().Split(':');
//                    s3t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
//                    String[] s11 = dtshift.Rows[k][2].ToString().Split(':');
//                    s3t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));
//                }
//            }
//            String CorrectedDate = System.DateTime.Now.ToString("yyyy-MM-dd");
//            if (msgstime >= s1t1 && msgstime < s1t2)
//            {
//                Shift = "A";
//            }
//            else if (msgstime >= s2t1 && msgstime < s2t2)
//            {
//                Shift = "B";
//            }
//            else if ((msgstime >= s3t1 && msgstime <= s3t4) || (msgstime >= s3t3 && msgstime < s3t2))
//            {
//                Shift = "C";
//                if (msgstime >= s3t3 && msgstime < s3t2)
//                {
//                    CorrectedDate = System.DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
//                }
//            }
//            mcp.close();

//            #endregion

//            dateShift[0] = Shift;
//            dateShift[1] = CorrectedDate;
//            return dateShift;
//        }

//        #region Json Methods

//        public JsonResult JsonBreakdownChecker()
//        {
//            string retStatus = null;
//            var machineID = Convert.ToInt32(Session["MachineID"]);

//            string correcteddate = null;
//            tbldaytiming StartTime1 = db.tbldaytimings.Where(m => m.IsDeleted == 0).SingleOrDefault();
//            TimeSpan Start = StartTime1.StartTime;
//            if (Start <= DateTime.Now.TimeOfDay)
//            {
//                correcteddate = DateTime.Now.ToString("yyyy-MM-dd");
//            }
//            else
//            {
//                correcteddate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
//            }
//            var CurrentStatusData = db.tblbreakdowns.Where(m => m.MachineID == machineID && m.DoneWithRow == 0).OrderByDescending(m => m.StartTime).FirstOrDefault();
//            if (CurrentStatusData != null)
//            {
//                int value = 1;
//                string doneWithRowString = Convert.ToString(CurrentStatusData.DoneWithRow);

//                Int32.TryParse(doneWithRowString, out value);
//                if (CurrentStatusData.MessageCode == "PM")
//                {
//                    if (value == 0)
//                    {
//                        retStatus = "PM";
//                    }
//                }
//                else
//                {
//                    if (value == 0)
//                    {
//                        retStatus = "BREAKDOWN";
//                    }
//                }
//            }
//            return Json(retStatus, JsonRequestBehavior.AllowGet);
//        }

//        public JsonResult JsonIdleChecker()
//        {
//            string retStatus = "false";
//            var machineID = Convert.ToInt32(Session["MachineID"]);
//            var CurrentStatusData = db.tbllossofentries.Where(m => m.MachineID == machineID && (m.IsScreen == 1 || m.IsStart == 1)).OrderByDescending(m => m.StartDateTime).Take(1).ToList();
//            if (CurrentStatusData.Count > 0)
//            {
//                int IsStart = Convert.ToInt32(CurrentStatusData[0].IsStart);
//                int IsScreen = Convert.ToInt32(CurrentStatusData[0].IsScreen);
//                int forRefresh = Convert.ToInt32(CurrentStatusData[0].ForRefresh);
//                if (IsStart == 1 && IsScreen == 0 && forRefresh == 0)
//                {
//                    retStatus = "true";
//                    CurrentStatusData[0].ForRefresh = 1;
//                    db.Entry(CurrentStatusData[0]).State = System.Data.Entity.EntityState.Modified;
//                    db.SaveChanges();
//                }
//                if (IsStart == 1 && IsScreen == 1 && forRefresh == 1) //loss code has been repeadtly entered , so dont popup just show screen
//                {
//                    retStatus = "true";
//                    CurrentStatusData[0].ForRefresh = 2;
//                    db.Entry(CurrentStatusData[0]).State = System.Data.Entity.EntityState.Modified;
//                    db.SaveChanges();
//                }
//            }

//            return Json(retStatus, JsonRequestBehavior.AllowGet);
//        }

//        public JsonResult JsonIdleEndChecker()
//        {
//            string retStatus = "false";
//            var machineID = Convert.ToInt32(Session["MachineID"]);
//            using (i_facility_shaktiEntities1 dbloss = new i_facility_shaktiEntities1())
//            {
//                var CurrentStatusData = db.tbllossofentries.Where(m => m.MachineID == machineID).OrderByDescending(m => m.StartDateTime).FirstOrDefault();
//                if (CurrentStatusData != null)
//                {
//                    int IsDone = Convert.ToInt32(CurrentStatusData.DoneWithRow);
//                    if (IsDone == 1)
//                    {
//                        retStatus = "true";
//                    }
//                }
//            }
//            return Json(retStatus, JsonRequestBehavior.AllowGet);
//        }
//        public JsonResult JsonRemoveWO(int hmiid) // Remove WorkOrder if Its Not Started.
//        {
//            string retStatus = "false";
//            using (i_facility_shaktiEntities1 dbhmi = new i_facility_shaktiEntities1())
//            {
//                var CurrentStatusData = dbhmi.tblworkorderentries.Where(m => m.HMIID == hmiid).FirstOrDefault();
//                if (CurrentStatusData != null && CurrentStatusData.WOStart == null)
//                {
//                    dbhmi.tblworkorderentries.Remove(CurrentStatusData);
//                    dbhmi.SaveChanges();
//                    retStatus = "true";
//                }
//                if (CurrentStatusData != null && CurrentStatusData.IsMultiWO == 1)
//                {
//                    try
//                    {
//                        //dbhmi.tbl_multiwoselection.RemoveRange(dbhmi.tbl_multiwoselection.Where(m => m.HMIID == CurrentStatusData.HMIID).ToList());
//                        dbhmi.SaveChanges();
//                    }
//                    catch (Exception e)
//                    {

//                    }
//                }
//            }
//            return Json(retStatus, JsonRequestBehavior.AllowGet);
//        }

//        #endregion

//        public JsonResult IsMultiWOAllowable(string id)
//        {
//            string status = "no";
//            int machineId = Convert.ToInt32(Session["MachineID"]);
//            var machineDATA = db.tblmachinedetails.Where(m => m.MachineDisplayName == id).FirstOrDefault();
//            string PlantID = Convert.ToString(machineDATA.PlantID);
//            string ShopID = Convert.ToString(machineDATA.ShopID);
//            string CellID = Convert.ToString(machineDATA.CellID);
//            string WCID = Convert.ToString(machineDATA.MachineID);
//            bool tick = false;

//            int value = 0;
//            if (int.TryParse(WCID, out value))
//            {
//                var MultiWoWCData = db.tblmultipleworkorders.Where(m => m.IsDeleted == 0 && m.WCID == value).ToList();
//                if (MultiWoWCData.Count > 0)
//                {
//                    status = "yes";
//                }
//            }
//            if (int.TryParse(CellID, out value))
//            {
//                var MultiWoCellData = db.tblmultipleworkorders.Where(m => m.IsDeleted == 0 && m.CellID == value && m.WCID == null).ToList();
//                if (MultiWoCellData.Count > 0)
//                {
//                    status = "yes";
//                }
//            }
//            if (int.TryParse(ShopID, out value))
//            {
//                var MultiWoShopData = db.tblmultipleworkorders.Where(m => m.IsDeleted == 0 && m.ShopID == value && m.CellID == null && m.WCID == null).ToList();
//                if (MultiWoShopData.Count > 0)
//                {
//                    status = "yes";
//                }
//            }
//            if (int.TryParse(PlantID, out value))
//            {
//                var MultiWoPlantData = db.tblmultipleworkorders.Where(m => m.IsDeleted == 0 && m.PlantID == value && m.ShopID == null && m.CellID == null && m.WCID == null).ToList();
//                if (MultiWoPlantData.Count > 0)
//                {
//                    status = "yes";
//                }
//            }

//            return Json(status, JsonRequestBehavior.AllowGet);
//        }
//        #endregion



//        #region Overall Partial Finish

//        public ActionResult OverAllPartial(string HMIIDs)
//        {
//            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
//            {
//                return RedirectToAction("Login", "Login", null);
//            }
//            //1) extract hmiids from string
//            //2) Check if eligible for  PartialFinish (Eligible only if all are eligible)
//            //3) Now Partial Finish all of them
//            int MachineID = Convert.ToInt32(Session["MachineID"]);
//            if (IDLEorGenericWorkisON())
//            {
//                Session["Error"] = "Please End IDLE/GenericWork Before Selecting New Work Orders";
//                return RedirectToAction("Index");
//            }

//            //1)
//            string[] HMIIDArray = HMIIDs.Split(',');
//            List<string> HMIIDList = HMIIDs.Split(',').ToList();

//            //2)
//            int ExceptionHMIID = 0;
//            foreach (var hmiid in HMIIDArray)
//            {
//                int hmiiid = Convert.ToInt32(hmiid);
//                var HMIData = db.tblworkorderentries.Where(m => m.HMIID == hmiiid).FirstOrDefault();
//                //if (HMIData.isWorkInProgress == 3)
//                if (HMIData.IsHold == 1)
//                {
//                    Session["VError"] = "End HOLD before Partial Finish";
//                    return RedirectToAction("Index");
//                }
//                else if (HMIData.WOStart != null)
//                {
//                    int deliveredQty = 0;
//                    if (int.TryParse(Convert.ToString(HMIData.Total_Qty), out deliveredQty))
//                    {
//                        int processed = 0;
//                        int.TryParse(Convert.ToString(HMIData.ProcessQty), out processed);
//                        if ((deliveredQty + processed) > Convert.ToInt32(HMIData.Total_Qty))
//                        {
//                            Session["VError"] = "DeliveredQty Must be less than Target for " + HMIData.Prod_Order_No;
//                            return RedirectToAction("Index");
//                        }

//                        #region 2017-02-07
//                        else if ((deliveredQty + processed) == Convert.ToInt32(HMIData.Total_Qty))
//                        {
//                            //Check if highest of opno's for sibling wo's is eligible for JF, if Qty satisfies.
//                            //1) if already lower op is jobfinished
//                            //2) or  its in current HMIIDArray and valid for jf

//                            string wono = HMIData.Prod_Order_No;
//                            string partno = HMIData.PartNo;
//                            string opno = HMIData.OperationNo;
//                            int opnoInt = Convert.ToInt32(HMIData.OperationNo);

//                            //Gen , Seperated String from HMIIDArray
//                            string hmiids = null;
//                            for (int hmiidLooper = 0; hmiidLooper < HMIIDArray.Length; hmiidLooper++)
//                            {
//                                if (hmiids == null)
//                                {
//                                    string localhmiidString = HMIIDArray[hmiidLooper];
//                                    if (ExceptionHMIID.ToString() != localhmiidString)
//                                    {
//                                        hmiids = HMIIDArray[hmiidLooper].ToString();
//                                    }
//                                }
//                                else
//                                {
//                                    if (ExceptionHMIID.ToString() != HMIIDArray[hmiidLooper])
//                                    {
//                                        hmiids += "," + HMIIDArray[hmiidLooper].ToString();
//                                    }
//                                }
//                            }
//                            //Get hmiids (as comma seperated string) in ascending order based on wono,partno,Opno.
//                            hmiids = GetOrderedHMIIDs(hmiids);

//                            using (i_facility_shaktiEntities1 dbHMI = new i_facility_shaktiEntities1())
//                            {
//                                //string WIPQuery1 = @"SELECT * from tblddl where WorkOrder = '" + wono + "' and MaterialDesc = '" + partno + "'  and OperationNo != '" + opno + "' and IsCompleted = 0 order by OperationNo ";
//                                var WIP1 = 0;//dbHMI.tblddls.SqlQuery(WIPQuery1).ToList();


//                                bool IsInHMI = true;
//                                int InnerOpNo = 0;
//                                if (opnoInt > InnerOpNo)
//                                {
//                                    string WIPQuery2 = @"SELECT * from tblhmiscreen where Work_Order_No = '" + wono + "' and PartNo = '" + partno + "'  and OperationNo = '" + InnerOpNo + "' and isWorkInProgress = 1 limit 1";
//                                    var WIP2 = dbHMI.tblworkorderentries.SqlQuery(WIPQuery2).FirstOrDefault();
//                                    if (WIP2 != null)
//                                    {
//                                        IsInHMI = true;
//                                    }
//                                    else
//                                    {
//                                        IsInHMI = false;
//                                    }
//                                    if (!IsInHMI)

//                                        Session["VError"] = "Finish WorkOrder: " + wono + ": OpNo: " + InnerOpNo;
//                                    return RedirectToAction("Index");
//                                }

//                            }
//                        }
//                    }

//                    #endregion
//                }
//                //else if (HMIData.Date == null)//Sneha testing
//                //{
//                //    Session["VError"] = "Enter Delivered Quantity";
//                //    return RedirectToAction("Index");
//                //}
         
//                else if (HMIData.WOStart == null)
//                {// Do Nothing. Just to Skip our Extra Empty Row
//                    if (ExceptionHMIID == 0)
//                    {
//                        ExceptionHMIID = Convert.ToInt32(HMIData.HMIID);
//                    }
//                    else
//                    {
//                        //Session["VError"] = "Please enter all Details Before PartialFinish is Clicked.";
//                        Session["VError"] = "Please Start All WorkOrders Before PartialFinish";
//                        return RedirectToAction("Index");
//                    }
//                }
//                else
//                {
//                    Session["VError"] = "Please Start All WorkOrders Before PartialFinish";
//                    return RedirectToAction("Index");
//                }
//            }

//            //Check if there are any row to partialFinish (Empty Screen)
//            if (HMIIDList.Count == 1)
//            {
//                foreach (var hmiid in HMIIDArray)
//                {
//                    int hmiidi = Convert.ToInt32(hmiid);
//                    if (hmiidi == ExceptionHMIID)
//                    {
//                        Session["VError"] = "There are no WorkOrder to Finish";
//                        return RedirectToAction("Index");
//                    }
//                }
//            }

//            ////Check if there are any row to partialFinish (Empty Screen)
//            //if (HMIIDList.Count == 1)
//            //{
//            //    foreach (var hmiid in HMIIDArray)
//            //    {
//            //        int hmiidi = Convert.ToInt32(hmiid);
//            //        if (hmiidi == ExceptionHMIID)
//            //        {
//            //            Session["VError"] = "There are no WorkOrder to Finish";
//            //            return RedirectToAction("Index");
//            //        }
//            //    }
//            //}

//            //Prevent JF is lower Opno is Not Elegible for JF
//            //Check if all are eligable to JF, if not throw err.
//            bool IsSequenceOK = true;
//            foreach (var hmiid in HMIIDArray)
//            {
//                int hmiidi = Convert.ToInt32(hmiid);
//                if (hmiidi != ExceptionHMIID)
//                {
//                    var HMIData = db.tblworkorderentries.Where(m => m.HMIID == hmiidi).FirstOrDefault();
//                    int DelivQty = 0, TargQty = 0, ProcessedQty = 0;

//                    int.TryParse(HMIData.Total_Qty.ToString(), out TargQty);
//                    int.TryParse(HMIData.Total_Qty.ToString(), out DelivQty);
//                    int.TryParse(HMIData.ProcessQty.ToString(), out ProcessedQty);

//                    if (TargQty >= (DelivQty + ProcessedQty))
//                    {
//                        string WoNo = HMIData.Prod_Order_No;
//                        string partNo = HMIData.PartNo;
//                        int OpNo = Convert.ToInt32(HMIData.OperationNo);
//                        foreach (var hmiid1 in HMIIDArray)
//                        {
//                            int hmiidiInner = Convert.ToInt32(hmiid1);
//                            if (hmiidi != ExceptionHMIID)
//                            {
//                                using (i_facility_shaktiEntities1 dbhmi = new i_facility_shaktiEntities1())
//                                {
//                                    var HMIDataInner = dbhmi.tblworkorderentries.Where(m => m.HMIID == hmiidiInner && m.Prod_Order_No == WoNo && m.PartNo == partNo && m.HMIID != hmiidi).FirstOrDefault();
//                                    if (HMIDataInner != null)
//                                    {
//                                        int opNoInner = Convert.ToInt32(HMIDataInner.OperationNo);
//                                        if (opNoInner < OpNo)
//                                        {
//                                            int DelivQty1 = 0, TargQty1 = 0, ProcessedQty1 = 0;
//                                            int.TryParse(HMIDataInner.Total_Qty.ToString(), out TargQty1);
//                                            int.TryParse(HMIDataInner.Yield_Qty.ToString(), out DelivQty1);
//                                            int.TryParse(HMIDataInner.ProcessQty.ToString(), out ProcessedQty1);
//                                            if (TargQty1 < (DelivQty + ProcessedQty))
//                                            {
//                                                Session["VError"] = "Click JobFinish, All WorkOrders have (Delivered + Processed) = Target.";
//                                                IsSequenceOK = false;
//                                                break;
//                                            }
//                                        }
//                                    }
//                                }
//                            }
//                        }
//                        if (!IsSequenceOK)
//                        {
//                            break;
//                        }
//                    }
//                    else
//                    {
//                        IsSequenceOK = false;
//                        break;
//                    }
//                }
//            }

//            if (!IsSequenceOK)
//            {
//                return RedirectToAction("Index");
//            }

//            //Check if all are eligable to JF, if so throw erro.
//            bool AllREligableForJF = true;
//            foreach (var hmiid in HMIIDArray)
//            {
//                int hmiidi = Convert.ToInt32(hmiid);
//                if (hmiidi != ExceptionHMIID)
//                {
//                    var HMIData = db.tblworkorderentries.Where(m => m.HMIID == hmiidi).FirstOrDefault();
//                    int DelivQty = 0;
//                    int TargQty = 0;
//                    int ProcessedQty = 0;

//                    int.TryParse(HMIData.Total_Qty.ToString(), out TargQty);
//                    int.TryParse(HMIData.Yield_Qty.ToString(), out DelivQty);
//                    int.TryParse(HMIData.ProcessQty.ToString(), out ProcessedQty);

//                    if (TargQty == (DelivQty + ProcessedQty))
//                    {
//                    }
//                    else
//                    {
//                        AllREligableForJF = false;
//                        break;
//                    }
//                }
//            }
//            if (AllREligableForJF)
//            {
//                Session["VError"] = "Click JobFinish, All WorkOrders have (Delivered + Processed) = Target.";
//                return RedirectToAction("Index");
//            }

//            List<string> MacHierarchy = GetMacHierarchyData(MachineID);

//            //Update ProcessedQty if WorkOrder Available in Different WorkCenter
//            foreach (var hmiid in HMIIDArray)
//            {
//                int hmiidi = Convert.ToInt32(hmiid);
//                if (hmiidi != ExceptionHMIID)
//                {
//                    var HMIData = db.tblworkorderentries.Where(m => m.HMIID == hmiidi).FirstOrDefault();
//                    int DelivQty = 0, TargQty = 0, ProcessedQty = 0;
//                    int.TryParse(HMIData.Yield_Qty.ToString(), out DelivQty);
//                    int HMIMacID = HMIData.MachineID;

//                    using (i_facility_shaktiEntities1 dbsimilar = new i_facility_shaktiEntities1())
//                    {
//                        #region If Its as SingleWO
//                        var SimilarWOData = dbsimilar.tblworkorderentries.Where(m => m.Prod_Order_No == HMIData.Prod_Order_No && m.OperationNo == HMIData.OperationNo && m.PartNo == HMIData.PartNo && m.MachineID != MachineID).ToList();
//                        foreach (var row in SimilarWOData)
//                        {
//                            int InnerProcessed = row.ProcessQty;
//                            int FinalProcessed = DelivQty + InnerProcessed;
//                            if (FinalProcessed < row.Total_Qty)
//                            {
//                                //if (row.isWorkInProgress == 2)
//                                {
//                                    row.ProcessQty = FinalProcessed;
//                                    dbsimilar.Entry(row).State = System.Data.Entity.EntityState.Modified;
//                                    dbsimilar.SaveChanges();
//                                }
//                            }
//                            else
//                            {
//                                Session["Error"] = " Same WorkOrder in Machine: " + MacHierarchy[3] + "->" + MacHierarchy[4] + " has ProcessedQty :" + InnerProcessed;
//                                return RedirectToAction("Index");
//                            }
//                        }
//                        #endregion

                        

//                    }

//                }
//            }

//            //2017-01-09
//            //3) 
//            //I think you have to rearrange these HMIID's based on wono,partno,opno ascending.
//            //Because they may select wo's 
//            //var ddlDummy = db.tblddls.Where(m=> HMIIDList.Contains(m.))
//            foreach (var hmiid in HMIIDArray)
//            {
//                int hmiidi = Convert.ToInt32(hmiid);
//                if (hmiidi != ExceptionHMIID)
//                {
//                    var HMIData = db.tblworkorderentries.Where(m => m.HMIID == hmiidi).FirstOrDefault();
//                    int DelivQty = 0;
//                    int TargQty = 0;
//                    int ProcessedQty = 0;

//                    int.TryParse(HMIData.Total_Qty.ToString(), out TargQty);
//                    int.TryParse(HMIData.Yield_Qty.ToString(), out DelivQty);
//                    int.TryParse(HMIData.ProcessQty.ToString(), out ProcessedQty);

//                    if (TargQty == (DelivQty + ProcessedQty))
//                    {
//                        string wono = HMIData.Prod_Order_No;
//                        string partno = HMIData.PartNo;
//                        string opno = HMIData.OperationNo;
//                        int opnoInt = Convert.ToInt32(HMIData.OperationNo);

//                        //2017-01-19
//                        //var ddldata1 = db.tblddls.Where(m => m.WorkOrder == wono && m.MaterialDesc == partno).FirstOrDefault();
//                        //if (ddldata1 != null)
//                        //{
//                        //    int opLocal = 0;
//                        //    int.TryParse(Convert.ToString(ddldata1.OperationNo), out opLocal);
//                        //    if (opLocal < opnoInt)
//                        //    {
//                        //        if (ddldata1.IsCompleted == 0)
//                        //        {
//                        //            Session["VError"] = "First, JobFinish WONo :" + ddldata1.WorkOrder;
//                        //            return RedirectToAction("Index");
//                        //        }
//                        //    }
//                        //}

//                        HMIData.Status = 2;
//                        //HMIData.isWorkInProgress = 1;

//                        //var ddldata = db.tblddls.Where(m => m.WorkOrder == wono && m.MaterialDesc == partno && m.OperationNo == opno).FirstOrDefault();
//                        //if (ddldata != null)
//                        //{
//                        //    ddldata.IsCompleted = 1;
//                        //    db.Entry(ddldata).State = System.Data.Entity.EntityState.Modified;
//                        //}
//                    }
//                    else if (TargQty > (DelivQty + ProcessedQty))
//                    {
//                        HMIData.Status = 2;
//                        //HMIData.isWorkInProgress = 0;
//                    }
//                    else
//                    {
//                        Session["VError"] = "Delivered + Processed Cannot be Greater than Target for :" + HMIData.Prod_Order_No;
//                        return RedirectToAction("Index");
//                    }
//                    HMIData.WOEnd = DateTime.Now;
//                    db.Entry(HMIData).State = System.Data.Entity.EntityState.Modified;
//                    db.SaveChanges();
//                }
//                Session["isWorkOrder"] = 0;
//            }

//            //var HMIDataAll = from r in  ( from row in db.tblhmiscreens
//            //                    where HMIIDList.Contains( row.HMIID.ToString() ) 
//            //                    orderby row.Work_Order_No, row.PartNo , row.OperationNo
//            //                    select row).ToList() );


//            return RedirectToAction("Index");
//        }

       

    

//        #endregion








//    }
//}