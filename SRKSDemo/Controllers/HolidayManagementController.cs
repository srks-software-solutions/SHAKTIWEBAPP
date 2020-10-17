using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using SRKSDemo.Server_Model;
namespace SRKSDemo.Controllers
{
    public class HolidayManagementController : Controller
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
        public ActionResult Index()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            return View(db.tblholidays.Where(m => m.IsDeleted == 0).ToList());
        }



        public JsonResult CreateData(string HD, string RD)
        {
            tblholiday tblholiday = new tblholiday();
          DateTime Var  = Convert.ToDateTime(HD).Date;
           var duplicateEntry = db.tblholidays.Where(m => m.IsDeleted == 0 && m.HolidayDate== Var).ToList();
            if(duplicateEntry.Count>0)
            {
                TempData["Error"] = "Record already exists";
            }
            else
            {
                tblholiday.CreatedBy = 1;
                tblholiday.CreatedOn = DateTime.Now;
                tblholiday.IsDeleted = 0;
                tblholiday.HolidayDate = Convert.ToDateTime(HD).Date;
                tblholiday.Reason = RD;
                {
                    db.tblholidays.Add(tblholiday);
                    db.SaveChanges();
                }
            }
           
            return Json(tblholiday.HolidayId, JsonRequestBehavior.AllowGet);
        }
            


        public ActionResult Create()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            return View();
        }

        public JsonResult DeleteData(int id = 0)
        {
            tblholiday tblholiday = db.tblholidays.Find(id);
            tblholiday.IsDeleted = 1;
            tblholiday.ModifiedBy = 1;
            tblholiday.ModifiedOn = DateTime.Now;
            {
                db.Entry(tblholiday).State = EntityState.Modified;
                db.SaveChanges();
            }
            return Json(tblholiday.HolidayId, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(int id = 0)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            tblholiday tblholiday = db.tblholidays.Find(id);
            if (tblholiday == null)
            {
                return HttpNotFound();
            }
            tblholiday.IsDeleted = 1;
            tblholiday.ModifiedBy = 1;
            tblholiday.ModifiedOn = DateTime.Now;
            db.Entry(tblholiday).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpGet]

        public ActionResult Edit(int id = 0)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            tblholiday tblholiday = db.tblholidays.Find(id);
            ViewBag.EditDate = Convert.ToDateTime(tblholiday.HolidayDate).ToString("dd/MM/yyyy");
            if (tblholiday == null)
            {
                return HttpNotFound();
            }
            return View(tblholiday);
        }


        [HttpPost]
        public ActionResult Edit(tblholiday tblholiday)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            tblholiday.ModifiedBy = 1;
            tblholiday.ModifiedOn = DateTime.Now;
            tblholiday.IsDeleted = 0;
            db.Entry(tblholiday).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

    }
}