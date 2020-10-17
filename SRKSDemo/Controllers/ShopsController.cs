using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using SRKSDemo.Server_Model;

namespace SRKSDemo.Controllers
{
    public class ShopsController : Controller
    {

        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
       
        public ActionResult ShopList()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            ShopModel de = new ShopModel();
            tblshop da = new tblshop();
            ViewBag.PlantID = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", da.PlantID).ToList();
            de.Shops = da;
            de.Shopslist = db.tblshops.Where(m => m.IsDeleted == 0).ToList();
            ViewBag.Plant = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName").ToList();
            return View(de);

            //var dept = db.masterdepartment_tbl.Where(x => x.IsDeleted == 0);
            //return View(dept.ToList());

        }


        [HttpGet]
        public ActionResult CreateShops()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();

            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                ViewBag.Plant = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName").ToList();
                return View();
            }
        }


        [HttpPost]
        public ActionResult CreateShops(ShopModel shop)
        {

            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();

            //shop name validation
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                string shopname = shop.Shops.ShopName.ToString();
                var doesThisShopExists = db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == shop.Shops.PlantID && m.ShopName == shopname).ToList();
                if (doesThisShopExists.Count == 0)
                {
                    shop.Shops.CreatedBy = 1;
                    shop.Shops.CreatedOn = DateTime.Now;
                    shop.Shops.IsDeleted = 0;
                    db.tblshops.Add(shop.Shops);
                    db.SaveChanges();
                }
                else
                {
                    ViewBag.PlantID = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", shop.Shops.PlantID).ToList();
                    Session["Error"] = "Shop Name: " + shop.Shops.ShopName + " already exists for this Plant :" + db.tblplants.Where(m => m.PlantID == shop.Shops.PlantID && m.IsDeleted == 0).Select(m => m.PlantDisplayName).FirstOrDefault();
                    return View(shop);
                }
                return RedirectToAction("ShopList");
            }
        }

        [HttpGet]
        public ActionResult EditShops(int Id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                tblshop dept = db.tblshops.Find(Id);
                if (dept == null)
                {
                    return HttpNotFound();
                }

                ViewBag.PlantID = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", dept.PlantID).ToList();
                return View(dept);
            }
        }

        // Update Department
        [HttpPost]
        public ActionResult EditShops(ShopModel shop)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            int UserID = Convert.ToInt32(Session["UserID"]);
            //shop name validation
            string deptname = shop.Shops.ShopName.ToString();
            int shopid = shop.Shops.ShopID;
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                var doesThisdeptExists = db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == shop.Shops.PlantID && m.ShopName == deptname && m.ShopID != shopid).ToList();
                if (doesThisdeptExists.Count == 0)
                {
                    var Department = db.tblshops.Find(shop.Shops.ShopID);

                    Department.ShopName = shop.Shops.ShopName;
                    Department.ShopDesc = shop.Shops.ShopDesc;
                    Department.Shopdisplayname = shop.Shops.Shopdisplayname;
                    Department.ModifiedBy = UserID;
                    Department.ModifiedOn = DateTime.Now;
                    //dept.ModifiedBy = 1;
                    //dept.ModifiedOn = DateTime.Now;
                    db.Entry(Department).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("ShopList");
                }
                else
                {
                    Session["Error"] = "Department Name already exists for this Plant:" + shop.Shops.tblplant.PlantDisplayName;
                    ViewBag.PlantID = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", shop.Shops.PlantID);
                    return View(shop);
                }
            }
        }

        //Delete Department
        public ActionResult DeleteShops(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            int UserID = Convert.ToInt32(Session["UserID"]);
            //ViewBag.IsConfigMenu = 0;


            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                tblshop shop = db.tblshops.Find(id);
                shop.IsDeleted = 1;
                shop.ModifiedBy = UserID;
                int shopid = shop.ShopID;
                shop.ModifiedOn = DateTime.Now;
                db.Entry(shop).State = EntityState.Modified;
                db.SaveChanges();
            }

            //delete corresponding cells & machines also.
            var cellsdata = db.tblcells.Where(m => m.IsDeleted == 0 && m.ShopID == id).ToList();
            foreach (var cellrow in cellsdata)
            {
                cellrow.IsDeleted = 1;
                db.Entry(cellrow).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                int cellid = cellrow.CellID;

                var machinedata = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == cellid).ToList();
                foreach (var machinerow in machinedata)
                {
                    machinerow.IsDeleted = 1;
                    db.Entry(machinerow).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

             }

            var machinedata1 = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == id).ToList();
            foreach (var machinerow in machinedata1)
            {
                machinerow.IsDeleted = 1;
                db.Entry(machinerow).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("ShopList");
        }

        public JsonResult GetDepartmetsById(int Id)
        {
             var data = db.tblshops.Where(m => m.ShopID == Id).Select(m => new { PlantId = m.PlantID, shopname = m.ShopName, shopdesc = m.ShopDesc, shopdisplay = m.Shopdisplayname });
             return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public string ShopNameDuplicateCheck(int plantID=0,string shopName = "")
        {
            string status = "ok";
            var doesThisShopExists = db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == plantID && m.ShopName == shopName).ToList();
            if (doesThisShopExists.Count == 0)
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
        public string ShopNameDuplicateCheckEdit(int plantID = 0, string shopName = "",int EditShopID=0)
        {
            string status = "ok";
            var doesThisShopExists = db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == plantID && m.ShopName == shopName).ToList();
            if (doesThisShopExists.Count == 0)
            {
                status = "ok";
            }
            else
            {
                var checkforId = db.tblshops.Where(m => m.IsDeleted == 0 && m.ShopName == shopName && m.PlantID == plantID && m.ShopID == EditShopID).ToList();//checks for that shopid
                if (checkforId.Count != 0)
                {
                    status = "ok";
                }
                else
                {
                    status = "notok";
                }
            }
            return status;
        }

        [HttpPost]
        public string ChildNodeCheck(int id = 0)
        {
            string status = "";
            var cellChild = db.tblcells.Where(m => m.IsDeleted == 0 && m.ShopID == id).ToList();
            var macChild = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == id).ToList();
            if (cellChild.Count == 0 && macChild.Count == 0)
            {
                status = "";
            }
            else
            {
                status = "The Shop is having dependent  cell and machines, Do you want to continue(If Yes every cell and machine having this shop will be deleted)";
            }
            return status;
        }
    }
}