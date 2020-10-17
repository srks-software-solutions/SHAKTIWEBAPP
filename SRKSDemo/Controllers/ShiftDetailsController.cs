using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data;
using SRKSDemo.Server_Model;

namespace SRKSDemo.Controllers
{

    public class ShiftDetailsController : Controller
    {
        i_facility_shaktiEntities1 condb = new i_facility_shaktiEntities1();

        public ActionResult Index()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            var shiftdetails = condb.tblshiftdetails.Where(m => m.IsDeleted == 0).OrderBy(m => m.ShiftDetailsID).ToList();
            return View(shiftdetails.ToList());
            //return View();
        }

        public ActionResult Create()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            ViewBag.ShiftMethod1 = new SelectList(condb.tblshiftmethods.Where(m => m.IsDeleted == 0), "ShiftMethodID", "ShiftMethodName");
            return View();
        }

        [HttpPost]
        public ActionResult Create(IEnumerable<tblshiftdetail> tblp, string Nextday, int ShiftMethod1 = 0, int IsGShift = 0)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            string error = "";
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            #region//ActiveLog Code
            int UserID = Convert.ToInt32(Session["UserId"]);
            //check if there's a entry of this shiftMethod in tblshiftdetailss
            var shiftmethodCheck = condb.tblshiftdetails.Where(m => m.IsDeleted == 0 && m.ShiftMethodID == ShiftMethod1).ToList();
            if (shiftmethodCheck.Count > 0)
            {
                error = "ShiftDetails for this ShiftMethod Exists.";
                TempData["toaster_error"] = error;
                ViewBag.ShiftMethod1 = new SelectList(condb.tblshiftmethods.Where(m => m.IsDeleted == 0), "ShiftMethodID", "ShiftMethodName");
                return View();

            }

            var shiftmethodiddata = condb.tblshiftmethods.Where(m => m.IsDeleted == 0 && m.ShiftMethodID == ShiftMethod1).SingleOrDefault();
            int? noofshifts = shiftmethodiddata.NoOfShifts;
            int rowscount = 0;

            //to check if names are duplicate
            List<string> shiftdetailsnames = new List<string>();
            foreach (var shift in tblp)
            {
                if (shift.ShiftDetailsName != null)
                {
                    shiftdetailsnames.Add(shift.ShiftDetailsName);
                }
            }
            // for current shiftdetails.
            if (shiftdetailsnames.Distinct().Count() != shiftdetailsnames.Count())
            {
                //Console.WriteLine("List contains duplicate values.");
                error = "Shift Names Cannot be Same.";
                TempData["toaster_error"] = error;
                ViewBag.ShiftMethod1 = new SelectList(condb.tblshiftmethods.Where(m => m.IsDeleted == 0), "ShiftMethodID", "ShiftMethodName");
                return View();
            }

            try
            {
                foreach (var shift in tblp)
                {
                    if (rowscount < noofshifts)
                    {
                        // calculate duration
                        int duration = 0;
                        string starttimestring = "2016-06-02" + " " + shift.ShiftStartTime;
                        DateTime starttimedatetime = Convert.ToDateTime(starttimestring);
                        string endtimestring = null;

                        TimeSpan tsStart = (System.TimeSpan)shift.ShiftStartTime;
                        TimeSpan tsEnd = (System.TimeSpan)shift.ShiftEndTime;

                        int result = TimeSpan.Compare(tsStart, tsEnd);
                        if (result < 0)
                        {
                            endtimestring = "2016-06-02" + " " + shift.ShiftEndTime;
                        }
                        else if (result > 0)
                        {
                            endtimestring = "2016-06-03" + " " + shift.ShiftEndTime;
                            shift.NextDay = 1;
                        }
                        DateTime endtimedatetime = Convert.ToDateTime(endtimestring);
                        TimeSpan ts = endtimedatetime.Subtract(starttimedatetime);
                        duration = Convert.ToInt32(ts.TotalMinutes);
                        int UserId = (Convert.ToInt32(Session["UserId"]));
                        //create new object/row
                        tblshiftdetail tsd = new tblshiftdetail();
                        tsd.CreatedBy = UserId;
                        tsd.CreatedOn = DateTime.Now;
                        tsd.Duration = duration;
                        tsd.IsDeleted = 0;
                        tsd.IsGShift = 0;
                        tsd.NextDay = shift.NextDay;
                        tsd.ShiftMethodID = ShiftMethod1;
                        tsd.ShiftDetailsDesc = shift.ShiftDetailsDesc;
                        tsd.ShiftDetailsName = shift.ShiftDetailsName;
                        tsd.ShiftEndTime = shift.ShiftEndTime;
                        tsd.IsShiftDetailsEdited = 0;
                        tsd.ShiftStartTime = shift.ShiftStartTime;
                        condb.tblshiftdetails.Add(tsd);
                        condb.SaveChanges();
                    }
                    rowscount++;
                }
            }
            catch (Exception e)
            {
                error = "Shift Name already exists for this ShiftMethod.";
                TempData["toaster_error"] = error;

                using (i_facility_shaktiEntities1 condb = new i_facility_shaktiEntities1())
                {
                    var todeletedata = condb.tblshiftdetails.Where(m => m.IsDeleted == 0 && m.ShiftMethodID == ShiftMethod1).ToList();
                    foreach (var row in todeletedata)
                    {
                        row.IsDeleted = 1;
                        condb.Entry(row).State = EntityState.Modified;
                        condb.SaveChanges();
                    }
                }
                ViewBag.ShiftMethod1 = new SelectList(condb.tblshiftmethods.Where(m => m.IsDeleted == 0), "ShiftMethodID", "ShiftMethodName");
                return View();
            }

            ViewBag.ShiftMethod = new SelectList(condb.tblshiftdetails.Where(m => m.IsDeleted == 0), "ShiftMethodID", "ShiftMethodName");
            TempData["toaster_success"] = "Data Saved Successfully";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            ViewBag.ID = id;

            //get all the shiftsdetails in the shift method
            tblshiftdetail tblmc = condb.tblshiftdetails.Find(id);
            List<tblshiftdetail> tsd = null;
            if (tblmc == null)
            {
                return HttpNotFound();
            }
            else
            {
                int shiftmethodid = Convert.ToInt32(tblmc.ShiftMethodID);
                tsd = condb.tblshiftdetails.Where(m => m.IsDeleted == 0 && m.ShiftMethodID == shiftmethodid).ToList();
            }
            ViewBag.ShiftMethod = new SelectList(condb.tblshiftmethods.Where(m => m.IsDeleted == 0), "ShiftMethodID", "ShiftMethodName", tblmc.ShiftMethodID);
            ViewBag.NextDay = new SelectList(condb.tblshiftmethods.Where(m => m.IsDeleted == 0), "NextDay", "NextDay", tblmc.NextDay);
            ViewBag.id = id;

            //ViewBag.unit = new SelectList(db.tblunits.Where(m => m.IsDeleted == 0), "U_ID", "Unit", tblpart.UnitDesc);
            return View(tsd);
        }

        [HttpPost]
        public ActionResult Edit(IEnumerable<tblshiftdetail> tblp, int ShiftMethod = 0, int hdnishift = 0)
        {
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            int UserID = Convert.ToInt32(Session["UserID"]);

            var shiftmethodiddata = condb.tblshiftmethods.Where(m => m.IsDeleted == 0 && m.ShiftMethodID == ShiftMethod).SingleOrDefault();
            int? noofshifts = shiftmethodiddata.NoOfShifts;
            int rowscount = 0;

            //insert isedited and other details into old rows and insert the new rows.
            var shiftDetailsData = condb.tblshiftdetails.Where(m => m.IsDeleted == 0 && m.ShiftMethodID == ShiftMethod).ToList();
            //check if shift method is in use or was used and now its being modified.
            ShiftDetails sd = new ShiftDetails();
            int shiftmethodid = Convert.ToInt32(ShiftMethod);
            bool tick = Convert.ToBoolean(1);
            try
            {
                foreach (var shift in tblp)
                {
                    if (rowscount < noofshifts)
                    {
                        using (i_facility_shaktiEntities1 condb = new i_facility_shaktiEntities1())
                        {
                            // calculate duration
                            int duration = 0;
                            string starttimestring = "2016-06-02" + " " + shift.ShiftStartTime;
                            DateTime starttimedatetime = Convert.ToDateTime(starttimestring);
                            string endtimestring = null;

                            TimeSpan tsStart = (System.TimeSpan)shift.ShiftStartTime;
                            TimeSpan tsEnd = (System.TimeSpan)shift.ShiftEndTime;

                            int result = TimeSpan.Compare(tsStart, tsEnd);
                            if (result < 0)
                            {
                                endtimestring = "2016-06-02" + " " + shift.ShiftEndTime;
                            }
                            else if (result > 0)
                            {
                                endtimestring = "2016-06-03" + " " + shift.ShiftEndTime;
                                shift.NextDay = 1;
                            }
                            DateTime endtimedatetime = Convert.ToDateTime(endtimestring);
                            TimeSpan ts = endtimedatetime.Subtract(starttimedatetime);
                            duration = Convert.ToInt32(ts.TotalMinutes);

                            if (tick)
                            {
                                //create new object/row
                                int shiftid = shift.ShiftDetailsID;
                                int oldcreatedby = 0;
                                DateTime oldcreatedon = DateTime.Now;
                                using (i_facility_shaktiEntities1 condb1 = new i_facility_shaktiEntities1())
                                {
                                    var getShiftId = condb1.tblshiftdetails.Where(m => m.IsDeleted == 0 && m.ShiftDetailsID == shiftid).SingleOrDefault();
                                    getShiftId.IsShiftDetailsEdited = 1;
                                    getShiftId.IsDeleted = 1;
                                    getShiftId.ShiftMethodID = ShiftMethod;
                                    getShiftId.ShiftDetailsEditedDate = DateTime.Now;

                                    oldcreatedon = Convert.ToDateTime(getShiftId.CreatedOn);
                                    oldcreatedby = Convert.ToInt32(getShiftId.CreatedBy);
                                    ViewBag.ShiftMethod = new SelectList(condb.tblshiftdetails.Where(m => m.IsDeleted == 0), "ShiftMethodID", "ShiftDetailsName", shift.ShiftMethodID);

                                    condb1.Entry(getShiftId).State = EntityState.Modified;
                                    condb1.SaveChanges();
                                }
                                tblshiftdetail tsd = new tblshiftdetail();
                                tsd.Duration = duration;
                                tsd.IsDeleted = 0;
                                tsd.CreatedBy = oldcreatedby;
                                tsd.CreatedOn = oldcreatedon;
                                tsd.ModifiedBy = UserID;
                                tsd.ModifiedOn = DateTime.Now;
                                tsd.IsDeleted = 0;
                                tsd.NextDay = shift.NextDay;
                                tsd.ShiftMethodID = ShiftMethod;
                                tsd.ShiftDetailsDesc = shift.ShiftDetailsDesc;
                                tsd.ShiftDetailsName = shift.ShiftDetailsName;
                                tsd.ShiftEndTime = shift.ShiftEndTime;
                                tsd.ShiftStartTime = shift.ShiftStartTime;
                                condb.tblshiftdetails.Add(tsd);
                                condb.SaveChanges();
                            }
                            else
                            {
                                //create new object/row
                                shift.ModifiedBy = UserID;
                                shift.ModifiedOn = DateTime.Now;
                                shift.Duration = duration;
                                shift.IsDeleted = 0;
                                shift.ShiftMethodID = ShiftMethod;

                                //db3.Entry(shift).State = EntityState.Modified;
                                condb.SaveChanges();
                            }
                        }
                    }
                    rowscount++;
                }

            }
            catch (Exception e)
            {

                TempData["toaster_error"] = "Please check with the data";
                ViewBag.ShiftMethod = new SelectList(condb.tblshiftmethods.Where(m => m.IsDeleted == 0), "ShiftMethodID", "ShiftMethodName", ShiftMethod);
                return View(tblp);
            }
            TempData["toaster_success"] = "Data Updated Successfully";
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            int UserID1 = id;
            int UserID = Convert.ToInt32(Session["UserId"]);

            tblshiftdetail tblmc = condb.tblshiftdetails.Find(id);
            tblmc.IsDeleted = 1;
            tblmc.ModifiedBy = UserID;
            tblmc.ModifiedOn = DateTime.Now;
            condb.Entry(tblmc).State = EntityState.Modified;
            condb.SaveChanges();
            TempData["toaster_success"] = "Data Deleted Successfully";
            return RedirectToAction("Index");
        }

        public JsonResult GetShifts(int shiftsCount)
        {
            int shifts = 0;
            var NumberOfShifts = condb.tblshiftmethods.Where(m => m.IsDeleted == 0 && m.ShiftMethodID == shiftsCount).Take(1).ToList();
            shifts = Convert.ToInt32(NumberOfShifts[0].NoOfShifts);
            return Json(shifts, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}