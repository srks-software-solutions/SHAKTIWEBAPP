using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Diagnostics;
using System.Data;
using SRKSDemo.Server_Model;

namespace SRKSDemo.Controllers
{
    public class ShiftMethodController : Controller
    {

        i_facility_shaktiEntities1 condb = new i_facility_shaktiEntities1();
        // GET: ShiftMethod                                                                                       c
        public ActionResult Index()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            using (i_facility_shaktiEntities1 condb = new i_facility_shaktiEntities1())
            {
                ShiftMethodModel pa = new ShiftMethodModel();
                tblshiftmethod mp = new tblshiftmethod();
                pa.ShiftMethod = mp;
                pa.ShiftMethodList = condb.tblshiftmethods.Where(m => m.IsDeleted == 0).ToList();
                return View(pa);
            }

        }


        [HttpGet]
        public ActionResult Create()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            return View();
        }

        [HttpPost]
        public ActionResult Create(ShiftMethodModel objtblshiftMethod)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            int UserID = Convert.ToInt32(Session["UserId"]);
            string var_shiftmethod = objtblshiftMethod.ShiftMethod.ShiftMethodName;
            using (i_facility_shaktiEntities1 condb = new i_facility_shaktiEntities1())
            {
                var does_shiftmethod_name_Exists = condb.tblshiftmethods.Where(m => m.IsDeleted == 0 && m.ShiftMethodName == var_shiftmethod).ToList();
                if (does_shiftmethod_name_Exists.Count == 0)
                {
                    objtblshiftMethod.ShiftMethod.CreatedBy = UserID;
                    objtblshiftMethod.ShiftMethod.IsDeleted = 0;
                    objtblshiftMethod.ShiftMethod.CreatedOn = DateTime.Now;
                    condb.tblshiftmethods.Add(objtblshiftMethod.ShiftMethod);
                    condb.SaveChanges();
                }
                else
                {
                    TempData["Error"] = "Shift Method Exists.";
                    return View(objtblshiftMethod);
                }
                TempData["toaster_success"] = "Data Saved successfully";
                return RedirectToAction("Index");
            }

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
            using (i_facility_shaktiEntities1 condb = new i_facility_shaktiEntities1())
            {
                tblshiftmethod objshiftmethod2 = condb.tblshiftmethods.Find(id);
                if (objshiftmethod2 == null)
                {
                    return HttpNotFound();
                }
                return View(objshiftmethod2);
            }
        }

        [HttpPost]
        public ActionResult Edit(ShiftMethodModel objshiftmethod)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            int UserID = Convert.ToInt32(Session["UserID"]);

            string shiftmethodname = objshiftmethod.ShiftMethod.ShiftMethodName;
            int shiftmethodId = objshiftmethod.ShiftMethod.ShiftMethodID;
            using (i_facility_shaktiEntities1 condb = new i_facility_shaktiEntities1())
            {
                var doesthisExist = condb.tblshiftmethods.Where(m => m.IsDeleted == 0 && m.ShiftMethodName == shiftmethodname && m.ShiftMethodID != shiftmethodId).ToList();
                if (doesthisExist.Count == 0)
                {

                    //check if shift method is in use or was used and now its being modified.
                    /* ShiftDetails sd = new ShiftDetails();*///Its a class created in seperate file Commented the class 
                    int shiftmethodid = Convert.ToInt32(objshiftmethod.ShiftMethod.ShiftMethodID);
                    bool tick = Convert.ToBoolean(0);//sd.IsThisShiftMethodIsInActionOrEnded(shiftmethodid);
                    if (tick)
                    {
                        //tblshiftmethod objsftmethod = new tblshiftmethod();
                        //objsftmethod.CreatedBy = UserID;
                        //objsftmethod.CreatedOn = DateTime.Now;
                        //objsftmethod.IsDeleted = 0;
                        //objsftmethod.NoOfShifts = objshiftmethod.ShiftMethod.NoOfShifts;
                        //objsftmethod.ShiftMethodDesc = objshiftmethod.ShiftMethod.ShiftMethodDesc;
                        //objsftmethod.ShiftMethodName = objshiftmethod.ShiftMethod.ShiftMethodName;
                        //condb.tblshiftmethod.Add(objsftmethod);
                        //db.SaveChanges();

                        objshiftmethod.ShiftMethod.ModifiedBy = UserID;
                        objshiftmethod.ShiftMethod.ModifiedOn = DateTime.Now;
                        condb.Entry(objshiftmethod.ShiftMethod).State = EntityState.Modified;
                        condb.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        objshiftmethod.ShiftMethod.ModifiedBy = UserID;
                        objshiftmethod.ShiftMethod.ModifiedOn = DateTime.Now;
                        tblshiftmethod sm = condb.tblshiftmethods.Find(objshiftmethod.ShiftMethod.ShiftMethodID);
                        //sm = objshiftmethod.ShiftMethod;
                        sm.ShiftMethodName = objshiftmethod.ShiftMethod.ShiftMethodName;
                        sm.ShiftMethodDesc = objshiftmethod.ShiftMethod.ShiftMethodDesc;
                        sm.NoOfShifts = objshiftmethod.ShiftMethod.NoOfShifts;
                        sm.ModifiedBy = UserID;
                        sm.ModifiedOn = DateTime.Now;
                        condb.Entry(sm).State = EntityState.Modified;
                        condb.SaveChanges();
                        TempData["toaster_success"] = "Data Updated successfully";
                        return RedirectToAction("Index");
                    }
                }

                else
                {
                    TempData["Error"] = "Shift Method Exists.";
                    return View(objshiftmethod.ShiftMethod);
                }
            }
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
            tblshiftmethod tblmc = condb.tblshiftmethods.Find(id);
            var shiftdetailsList = condb.tblshiftdetails.Where(m => m.IsDeleted == 0 && m.ShiftMethodID == id).ToList();
            foreach (var shiftdetailsrow in shiftdetailsList)
            {
                shiftdetailsrow.IsDeleted = 1;
                condb.Entry(shiftdetailsrow).State = EntityState.Modified;
                condb.SaveChanges();
            }

            tblmc.IsDeleted = 1;
            condb.Entry(tblmc).State = EntityState.Modified;
            condb.SaveChanges();
            TempData["toaster_success"] = "Data Deleted successfully";
            return RedirectToAction("Index");
        }

        // This code is to get shiftmethods by id and load in Edit page textboxes
        public JsonResult GetShiftMethodById(int Id)
        {
            var Data = condb.tblshiftmethods.Where(m => m.ShiftMethodID == Id).Select(m => new { shiftmethodname = m.ShiftMethodName, shiftmethoddesc = m.ShiftMethodDesc, numofshift = m.NoOfShifts, ShiftID = m.ShiftMethodID });
            return Json(Data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public string ShiftMethodNameDuplicateCheck(string shiftMethodName = "")
        {
            string status = "notok";
            var doesThisShiftMethodNameExist = condb.tblshiftmethods.Where(m => m.IsDeleted == 0 && m.ShiftMethodName == shiftMethodName).ToList();
            if (doesThisShiftMethodNameExist.Count == 0)
            {
                status = "ok";
            }
            else
            {
                status = "notok";
            }
            return status;
        }

        [HttpPost]
        public string ShiftMethodNameDuplicateCheckEdit(int ShiftMethodID = 0, string shiftMethodName = "")
        {
            string status = "notok";
            if (ShiftMethodID != 0)
            {
                var doesThisShiftMethodNameExist = condb.tblshiftmethods.Where(m => m.IsDeleted == 0 && m.ShiftMethodName == shiftMethodName).ToList();
                if (doesThisShiftMethodNameExist.Count == 0)
                {
                    status = "ok";
                }
                else
                {
                    var checkforId = condb.tblshiftmethods.Where(m => m.IsDeleted == 0 && m.ShiftMethodName == shiftMethodName && m.ShiftMethodID == ShiftMethodID).ToList();
                    if (checkforId.Count == 0)
                    {
                        status = "notok";
                    }
                    else
                    {
                        status = "ok";
                    }

                }
            }
            return status;
        }
    }
}