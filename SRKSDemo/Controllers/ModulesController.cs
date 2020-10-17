using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using SRKSDemo.Server_Model;

namespace SRKSDemo.Controllers
{
    public class ModulesController : Controller
    {
        // Get All Roles to Display in View.
        public ActionResult Index()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.UserName = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
            ModulesModel ma = new ModulesModel();
            tblmodule mo = new tblmodule();
            ma.Modules = mo;
            ma.ModuleList = db.tblmodules.Where(m => m.IsDeleted == 0);
            return View(ma);
            //var ModulesData = db.mastermoduledet_tbl.Where(m => m.IsDeleted == 0).ToList();
            //return View(ModulesData);

        }

        //Create New Role.
        public ActionResult Create()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.UserName = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();

            return View();
        }
        [HttpPost]
        public ActionResult Create(ModulesModel tblModule)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            int UserID1 = Convert.ToInt32(Session["UserID"].ToString());
            tblModule.Modules.InsertedBy = UserID1;
            tblModule.Modules.InsertedOn = System.DateTime.Now;
            tblModule.Modules.IsDeleted = 0;

            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                var DupModule = db.tblmodules.Where(m => m.IsDeleted == 0 && m.Module == tblModule.Modules.Module).FirstOrDefault();
                if (DupModule == null)
                {
                    db.tblmodules.Add(tblModule.Modules);
                    db.SaveChanges();

                    //Insert new row into Module Helper table
                    tblrolemodulelink rl = new tblrolemodulelink();
                    rl.RoleID = Convert.ToInt32(Session["RoleID"]);
                    rl.ModuleID = tblModule.Modules.ModuleId;
                    rl.InsertedBy = UserID1;
                    rl.InsertedOn = System.DateTime.Now;
                    db.tblrolemodulelinks.Add(rl);
                    db.SaveChanges();
                }
                else
                {
                    Session["Error"] = "Duplicate ModuleName: " + tblModule.Modules.Module;
                    return View(tblModule);
                }
            }
            return RedirectToAction("Index");
        }

        //Edit Existing Role.
        public ActionResult Edit(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.UserName = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();

            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                tblmodule tblModule = db.tblmodules.Find(id);
                if (tblModule == null)
                {
                    return HttpNotFound();
                }
                Session["ID"] = id;
                return View(tblModule);
            }
        }
        [HttpPost]
        public ActionResult Edit(ModulesModel tblModule)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.UserName = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();

            int UserID1 = Convert.ToInt32(Session["UserID"].ToString());
            ViewBag.IsEmailEscalation = 0;


            if (ModelState.IsValid)
            {
                using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
                {
                    var DupModule = db.tblmodules.Where(m => m.IsDeleted == 0 && m.Module == tblModule.Modules.Module && m.ModuleId != tblModule.Modules.ModuleId).FirstOrDefault();
                    if (DupModule == null)
                    {
                        var ModuleData = db.tblmodules.Find(tblModule.Modules.ModuleId);
                        ModuleData.Module = tblModule.Modules.Module;
                        ModuleData.ModuleDesc = tblModule.Modules.ModuleDesc;
                        ModuleData.ModuleDispName = tblModule.Modules.ModuleDispName;
                        ModuleData.ModifiedBy = 1;
                        ModuleData.ModifiedOn = DateTime.Now;


                        db.Entry(ModuleData).State = EntityState.Modified;
                        db.SaveChanges();

                        ////Updating in module helper not necessary as we are not changing the ModuleID.
                        //int ID = Convert.ToInt32(Session["ID"]);
                        //masterrolemodulehelper module = db.masterrolemodulehelpers.Find(ID);
                        //module.ModuleID = tblModule.ModuleId;
                        //db.Entry(tblModule).State = EntityState.Modified;
                        //db.SaveChanges();

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        Session["Error"] = "Duplicate ModuleName: " + tblModule.Modules.Module;
                        return View(tblModule);
                    }
                }
            }
            else
            {
                return View(tblModule);
            }

        }

        //Update IsDeleted = 1 to mark it as Deleted Role.
        public ActionResult Delete(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            int UserID1 = Convert.ToInt32(Session["UserID"].ToString());

            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                tblmodule tblModule = db.tblmodules.Find(id);
                tblModule.IsDeleted = 1;
                tblModule.ModifiedBy = UserID1;
                tblModule.ModifiedOn = System.DateTime.Now;
                db.Entry(tblModule).State = EntityState.Modified;
                db.SaveChanges();

                //Updating in module helper
                int ID = Convert.ToInt32(tblModule.ModuleId);
                tblrolemodulelink module = db.tblrolemodulelinks.Where(m => m.ModuleID == ID).FirstOrDefault();
                module.IsDeleted = 1;
                module.ModifiedBy = UserID1;
                module.ModifiedOn = System.DateTime.Now;
                db.Entry(module).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
        }


        public JsonResult GetModuleById(int Id)
        {
            i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();

            var Data = db.tblmodules.Where(m => m.ModuleId == Id).Select(m => new { Module = m.Module, ModuleDesc = m.ModuleDesc, ModuleDisplay = m.ModuleDispName });

            return Json(Data, JsonRequestBehavior.AllowGet);
        }
    }
}