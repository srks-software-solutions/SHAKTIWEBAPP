using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using SRKSDemo.Server_Model;

namespace SRKSDemo.Controllers
{
    public class RolesController : Controller
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
        // GET: Roles
        public ActionResult Index()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.UserName = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            RolesModel Ra = new RolesModel();
            tblrole ro = new tblrole();
            Ra.Role = ro;
            Ra.RoleList = db.tblroles.Where(m => m.IsDeleted == 0);
            return View(Ra);
                
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
            return View();
        }


        [HttpPost]
        public ActionResult Create(RolesModel tblrole)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            var DuplicateRole = db.tblroles.Where(m => m.IsDeleted == 0 && m.RoleName == tblrole.Role.RoleName).FirstOrDefault();
            if (DuplicateRole == null)
            {
                //  Update Role data with other required fields.
                int UserID = Convert.ToInt32(Session["UserId"]);
                tblrole.Role.CreatedBy = UserID;
                tblrole.Role.CreatedOn = System.DateTime.Now;
                tblrole.Role.IsDeleted = 0;

                using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
                {
                    db.tblroles.Add(tblrole.Role);
                    db.SaveChanges();
                }
            }
            else
            {
                Session["Error"] = "Duplicate Role : " + tblrole.Role.RoleName;
                return View(tblrole);
            }

            return RedirectToAction("Index");
        }

        //Edit Existing Role.
        [HandleError(View = "~/Views/Shared/Error.cshtml")]

        public ActionResult Edit(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            //id = -1;
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                tblrole tblrole = db.tblroles.Find(id);
                //if (tblrole == null)
                //{
                //    //return HttpNotFound();
                //}
                int a = tblrole.Role_ID;
                return View(tblrole);
            }
        }
        [HttpPost]
        public ActionResult Edit(RolesModel tblrole)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            int UserID = Convert.ToInt32(Session["UserID"]);

            // Update Role data with other required fields.
            //tblrole.ModifiedBy = UserID;
            //tblrole.ModifiedOn = System.DateTime.Now;
            var DuplicateRole = db.tblroles.Where(m => m.IsDeleted == 0 && m.RoleName == tblrole.Role.RoleName && m.Role_ID != tblrole.Role.Role_ID).FirstOrDefault();
            if (DuplicateRole == null)
            {
                if (ModelState.IsValid)
                {
                    using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
                    {
                        var RoleData = db.tblroles.Find(tblrole.Role.Role_ID);
                        RoleData.RoleName = tblrole.Role.RoleName;
                        RoleData.RoleDesc = tblrole.Role.RoleDesc;
                        RoleData.RoleDisplayName = tblrole.Role.RoleDisplayName;
                        RoleData.ModifiedBy = 1;
                        RoleData.ModifiedOn = DateTime.Now;
                        db.Entry(RoleData).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    return RedirectToAction("Index");
                }
            }
            else
            {
                Session["Error"] = "Duplicate Role : " + tblrole.Role.RoleName;
                return View(tblrole);
            }
            return View(tblrole);
        }

        //Update IsDeleted = 1 i.e If IsDeleted=1 then its deleted row
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

            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                tblrole tblrole = db.tblroles.Find(id);
                tblrole.IsDeleted = 1;
                tblrole.ModifiedBy = UserID1;
                tblrole.ModifiedOn = DateTime.Now;

                db.Entry(tblrole).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public JsonResult GetRoleById(int Id)
        {
            var Data = db.tblroles.Where(m => m.Role_ID == Id).Select(m => new { rolename = m.RoleName, roledesc = m.RoleDesc, roledisplay = m.RoleDisplayName });

            return Json(Data, JsonRequestBehavior.AllowGet);
        }




    }
}