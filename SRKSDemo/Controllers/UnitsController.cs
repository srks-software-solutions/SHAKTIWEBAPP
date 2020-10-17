using i_facility.Models;
using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace i_facility.Controllers
{
    public class UnitsController : Controller
    {
        //
        // GET: /Units/
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
        string Controller = "Units";
        string Action = null;
        public ActionResult Index()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            var units = db.tblunits.Where(m => m.IsDeleted == 0);
            return View(units.ToList());
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

        //
        // POST: /Unit/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(tblunit tblunit)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            tblunit.CreatedBy = Convert.ToInt32(Session["UserId"]); ;
            tblunit.CreatedOn = System.DateTime.Now;
            tblunit.IsDeleted = 0;
            int UserID = Convert.ToInt32(Session["UserId"]);
            string CompleteModificationdetail = "New Creation";
            Action = "Create";
            //ActiveLogStorage Obj = new ActiveLogStorage();
            //Obj.SaveActiveLog(Action, Controller, Username, UserID, CompleteModificationdetail);
            //End
            db.tblunits.Add(tblunit);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            tblunit tblunit = db.tblunits.Find(id);
            if (tblunit == null)
            {
                return HttpNotFound();
            }
            return View(tblunit);
        }
        [HttpPost]
        public ActionResult Edit(tblunit tblunit)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            int UserID = Convert.ToInt32(Session["UserId"]);
            tblunit.ModifiedBy = UserID;
            tblunit.ModifiedOn = System.DateTime.Now;
            {
                //Section related to storing data in ActiveLog
                //#region Active Log Code
                //tblunit OldData = db.tblunits.Find(tblunit.U_ID);
                //IEnumerable<string> FullData = ActiveLog.EnumeratePropertyDifferences<tblunit>(OldData, tblunit);
                //ICollection<tblunit> c = FullData as ICollection<tblunit>;
                //int Count = c.Count;
                //if (Count != 0)
                //{
                //    string CompleteModificationdetail = null;
                //    for (int i = 0; i < Count; i++)
                //    {
                //        CompleteModificationdetail = CompleteModificationdetail + "-" + FullData.Take(i).ToArray();
                //    }
                //    Action = "Edit";
                //    ActiveLogStorage Obj = new ActiveLogStorage();
                //    Obj.SaveActiveLog(Action, Controller, Username, UserID, CompleteModificationdetail);
                //}
                //#endregion //End Active Log
                if (ModelState.IsValid)
                {
                    db.Entry(tblunit).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return View(tblunit);
        }
        public ActionResult Delete(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            int UserID1 = id;
            //ViewBag.IsConfigMenu = 0;
            tblunit tblunit = db.tblunits.Find(id);
            tblunit.IsDeleted = 1;
            tblunit.ModifiedBy = UserID1;
            tblunit.ModifiedOn = System.DateTime.Now;
            //start Logging
            int UserID = Convert.ToInt32(Session["UserId"]);
            string CompleteModificationdetail = "Deleted Units";
            Action = "Delete";
            //ActiveLogStorage Obj = new ActiveLogStorage();
            //Obj.SaveActiveLog(Action, Controller, Username, UserID, CompleteModificationdetail);
            //End
            db.Entry(tblunit).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        public ActionResult extra()
        { 
        return View();
        }

    }
}
