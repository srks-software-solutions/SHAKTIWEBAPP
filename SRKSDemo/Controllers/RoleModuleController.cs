using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SRKSDemo.Server_Model;

namespace SRKSDemo.Controllers
{
    public class RoleModuleController : Controller
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
        // GET: RoleModule
        public ActionResult Index()
        {
            ViewBag.UserName = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            RoleModuleModel ra = new RoleModuleModel();
            tblrolemodulelink re = new tblrolemodulelink();
            ra.RoleModule = re;
            ra.RoleModuleList = db.tblrolemodulelinks.Where(m => m.IsDeleted == 0).ToList();
            ViewBag.Role = new SelectList(db.tblroles.Where(m => m.IsDeleted == 0), "Role_ID", "RoleName");
            //var RoleplayModule = db.masterrolemodulelink_tbl.Where(e => e.IsDeleted == 0);
            return View(ra);
        }

        [HttpGet]
        public ActionResult CreateRoleModule()
        {
            ViewBag.UserName = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            var rolemodule = db.tblrolemodulelinks.Where(m => m.IsDeleted == 0);
            ViewBag.Role = new SelectList(db.tblroles.Where(m => m.IsDeleted == 0), "RoleID", "RoleName");
            return View(rolemodule.ToList());
        }

        [HttpPost]
        public ActionResult CreateRoleModule(List<tblrolemodulelink> rolemodule, int RoleId1)
        {
            ViewBag.UserName = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();

            int UseId = 1;
            int role = 0;
            int count = 0;

            string[,] module = new string[2, 11];

            if (rolemodule != null)
            {
                int i = 0;
                foreach (var bsl in rolemodule)
                {
                    tblrolemodulelink he = new tblrolemodulelink();
                    module[i, 0] = bsl.IsSuperAdmin.ToString();
                    module[i, 1] = bsl.IsVisible.ToString();
                    module[i, 2] = bsl.EnableAdd.ToString();
                    module[i, 3] = bsl.EnableEdit.ToString();
                    module[i, 4] = bsl.EnableDelete.ToString();
                    module[i, 5] = bsl.EnableReadOnly.ToString();
                    module[i, 6] = bsl.EnableReport.ToString();
                    module[i, 7] = Convert.ToString(RoleId1);
                    module[i, 8] = bsl.ModuleID.ToString();
                    setvalue(module);
                    count++;
                }
                return RedirectToAction("Index");
            }
            //var tblrolemodule = db.masterrolemodulehelpers.Where(m => m.IsDeleted == 0);
            ViewBag.Role = new SelectList(db.tblroles.Where(m => m.IsDeleted == 0), "RoleID", "RoleName");
            return View();

        }

        public void setvalue(string[,] set)
        {
            int i = 0;
            int ModuleID = Convert.ToInt32(set[0, 8]);
            int RlId = Convert.ToInt32(set[0, 7]);
            int Count = 0;
            try
            {
                var RoleModule = (from n in db.tblrolemodulelinks where (n.ModuleID == ModuleID && n.RoleID == RlId && n.IsDeleted == 0) select n).FirstOrDefault();
                if (RoleModule != null)
                {
                    Count = 1;
                }
            }
            catch { }
            if (Count == 0)
            {
                for (int j = 0; j < 1; j++)
                {
                    tblrolemodulelink rl = new tblrolemodulelink();
                    rl.IsVisible = Convert.ToInt16(set[j, 1]);
                    rl.IsSuperAdmin = Convert.ToInt16(set[j, 0]); ;
                    rl.IsDeleted = 0;
                    rl.EnableAdd = Convert.ToInt16(set[j, 2]); ;
                    rl.EnableDelete = Convert.ToInt16(set[j, 4]); ;
                    rl.EnableEdit = Convert.ToInt16(set[j, 3]);
                    rl.EnableReadOnly = Convert.ToInt16(set[j, 5]);
                    rl.EnableReport = Convert.ToInt16(set[j, 6]);
                    rl.RoleID = Convert.ToInt32(set[j, 7]);
                    rl.ModuleID = ModuleID;
                    rl.InsertedBy = 1;
                    rl.InsertedOn = System.DateTime.Now;
                    db.tblrolemodulelinks.Add(rl);
                    db.SaveChanges();
                }
            }
        }

        [HttpGet]
        public ActionResult EditRoleModule(int RoleId1 = 0)
        {
            ViewBag.UserName = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();

            ViewBag.Role = new SelectList(db.tblroles.Where(m => m.IsDeleted == 0), "RoleID", "RoleName");
            var RoleModule = db.tblrolemodulelinks.Where(d => d.IsDeleted == 0 && d.RoleID == RoleId1);
            return View(RoleModule.ToList());
        }

        [HttpPost]
        public ActionResult EditRoleModule(IEnumerable<tblrolemodulelink> rolemodule)
        {

            if (rolemodule != null)
            {
                if (ModelState.IsValid)
                {
                    foreach (var b in rolemodule)
                    {
                        b.ModifiedBy = 1;
                        b.ModifiedOn = DateTime.Now;
                        db.Entry(b).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                    return RedirectToAction("Index");
                }
            }
            return View(rolemodule);

        }

    }
}