using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using SRKSDemo.Server_Model;

namespace SRKSDemo.Controllers
{
    public class PlantController : Controller
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
            String Username = Session["Username"].ToString();
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                PlantModel pa = new PlantModel();
                tblplant mp = new tblplant();
                pa.Plant= mp;
                pa.PlantList = db.tblplants.Where(m => m.IsDeleted == 0).ToList();
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

        //Creating Plant
        [HttpPost]
        public ActionResult Create(PlantModel tblp)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            string plantName = tblp.Plant.PlantName.ToString();
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                var doesThisPlantExist = db.tblplants.Where(m => m.IsDeleted == 0 && m.PlantName == plantName).ToList();
                if (doesThisPlantExist.Count == 0)
                {
                    tblp.Plant.CreatedBy = 1;
                    tblp.Plant.CreatedOn = DateTime.Now;
                    tblp.Plant.IsDeleted = 0;
                    db.tblplants.Add(tblp.Plant);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "Plant Name" + " " + tblp.Plant.PlantName + " already Exists.";
                    return View(tblp);
                }
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
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                tblplant tblmc = db.tblplants.Find(id);
                if (tblmc == null)
                {
                    return HttpNotFound();
                }
                return View(tblmc);
            }
        }

        //Update Plant
        [HttpPost]
        public ActionResult Edit(PlantModel tblmc)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            int UserID = Convert.ToInt32(Session["UserID"]);
            string plantName = tblmc.Plant.PlantName.ToString();
            int plantid = tblmc.Plant.PlantID;
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                var doesThisPlantExist = db.tblplants.Where(m => m.IsDeleted == 0 && m.PlantName == plantName && m.PlantID != plantid).ToList();
                if (doesThisPlantExist.Count == 0)
                {
                    var Plant = db.tblplants.Find(tblmc.Plant.PlantID);
                    Plant.PlantName = tblmc.Plant.PlantName;
                    Plant.PlantDesc = tblmc.Plant.PlantDesc;
                    Plant.PlantDisplayName = tblmc.Plant.PlantDisplayName;
                    Plant.ModifiedBy = 1;
                    Plant.ModifiedOn = DateTime.Now;
                    db.Entry(Plant).State = EntityState.Modified;
                    db.SaveChanges();
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "Plant Name" + " " + tblmc.Plant.PlantName + " " + "already Exists.";
                    return View(tblmc);
                }
            }
        }

        public JsonResult GetPlantsById(int Id)
        {
            var Data = db.tblplants.Where(m => m.PlantID == Id).Select(m => new { PlantName = m.PlantName, PlantDes = m.PlantDesc, PlantDisplay = m.PlantDisplayName });
            return Json(Data, JsonRequestBehavior.AllowGet);
        }

        //Delete Plant
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
            ViewBag.IsConfigMenu = 0;
            tblplant tblmc = db.tblplants.Find(id);
            int plantid = tblmc.PlantID;
            tblmc.IsDeleted = 1;
            db.Entry(tblmc).State =EntityState.Modified;
            db.SaveChanges();


            //Delete corresponding shops cells & machines.

            var shopdata = db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == plantid).ToList();
            foreach (var shoprow in shopdata)
            {
                shoprow.IsDeleted = 1;
                db.Entry(shoprow).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                int shopid = shoprow.ShopID;

                var cellsdata = db.tblcells.Where(m => m.IsDeleted == 0 && m.ShopID == shopid).ToList();
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
            }

                var machinedata1 = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == id).ToList();
                foreach (var machinerow in machinedata1)
                {
                    machinerow.IsDeleted = 1;
                    db.Entry(machinerow).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

                    return RedirectToAction("Index");
        }

        [HttpPost]
        public string PlantNameDuplicateCheck(string plantName = "")
        {
            string status = "notok";
            var doesThisPlantExist = db.tblplants.Where(m => m.IsDeleted == 0 && m.PlantName == plantName).ToList();
            if (doesThisPlantExist.Count == 0)
            {
                status = "ok";
            }
            else {
                status = "notok";
            }
            return status;
        }

        [HttpPost]
        public string PlantNameDuplicateCheckEdit(string plantName = "",int EditPlantID=0)
        {
            string status = "notok";
            if (EditPlantID != 0)
            {
                var doesThisPlantExist = db.tblplants.Where(m => m.IsDeleted == 0 && m.PlantName == plantName ).ToList();
                if (doesThisPlantExist.Count == 0)
                {
                    status = "ok";
                }
                else
                {
                    var checkforId= db.tblplants.Where(m => m.IsDeleted == 0 && m.PlantName == plantName && m.PlantID == EditPlantID).ToList();
                    if (checkforId.Count == 0)
                    {
                        status = "notok";
                    }
                    else {
                        status = "ok";
                    }
                    
                }
            }
            return status;
        }

        [HttpPost]
        public string ChildNodeCheck(int id = 0)
        {
            string status = "";
            var shopChild = db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == id).ToList();
            var cellChild = db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == id).ToList();
            var macChild = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == id).ToList();
            if (shopChild.Count == 0 && cellChild.Count == 0 && macChild.Count == 0 )
            {
                status = "";
            }
            else
            {
                status = "The Plant is having dependent shop, cell and machines, Do you want to continue(If Yes every cell,shop and machine having this plant will be deleted)";
            }
            return status;
        }
    }
}