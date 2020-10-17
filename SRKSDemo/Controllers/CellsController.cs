using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SRKSDemo.Server_Model;
using System.Data.Entity;

namespace SRKSDemo.Controllers
{
    public class CellsController : Controller
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();

        //Gettimg Machine category list
        public ActionResult CellsList()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.plant = new SelectList(db.tblplants.Where(p => p.IsDeleted == 0), "PlantID", "PlantName").ToList();
            ViewBag.shop = new SelectList(db.tblshops.Where(d => d.IsDeleted == 0), "ShopId", "ShopName").ToList();
            CellsModel pa = new CellsModel();
            tblcell mp = new tblcell();
            pa.Cells = mp;
            pa.cellslist = db.tblcells.Where(m => m.IsDeleted == 0).ToList();
            return View(pa);
            //var MachineCategory = db.mastermachinecategory_tbl.Where(x => x.IsDeleted == 0).ToList();
            //return View(MachineCategory);

        }

        [HttpGet]
        public ActionResult CreateCells()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.plant = new SelectList(db.tblplants.Where(p => p.IsDeleted == 0), "PlantID", "PlantName").ToList();
           // ViewBag.shop = new SelectList(db.tblshops.Where(d => d.IsDeleted == 0), "ShopId", "ShopName").ToList();
            return View();

        }

        [HttpPost]
        public ActionResult CreateCells(CellsModel tblp, int shop = 0, int Plant = 0)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            int UserID = 1; //Convert.ToInt32(Session["UserId"]);
            //Cell name validation
            string cellname = tblp.Cells.ToString();
          // var shop= tblp.Cells.ShopID;
            var PlantId = tblp.Cells.PlantID;
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                var doesThisShopExists = db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == Plant && m.ShopID == shop && m.CellName == cellname).ToList();
                if (doesThisShopExists.Count == 0)
                {
                    tblp.Cells.CreatedBy = UserID;
                    tblp.Cells.CreatedOn = DateTime.Now;
                    tblp.Cells.ShopID = shop;
                    tblp.Cells.PlantID = PlantId;
                    tblp.Cells.IsDeleted = 0;
                    db.tblcells.Add(tblp.Cells);
                    db.SaveChanges();
                    return RedirectToAction("CellsList");

                }
                else
                {
                    ViewBag.Plant = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", Plant);
                    ViewBag.Shop = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0), "ShopId", "ShopName", shop);
                    Session["Error"] = "Machine Name" + " " + tblp.Cells + " " + " already exists for this Plant/Department.";
                    return View(tblp);
                }

            }
        }

        [HttpGet]
        public ActionResult EditCellDetails(int Id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            //ViewBag.Logout = Session["Username"].ToString().ToUpper();
            //ViewBag.roleid = Session["RoleID"];
            //String Username = Session["Username"].ToString();
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                tblcell tblmc = db.tblcells.Find(Id);
                if (tblmc == null)
                {
                    return HttpNotFound();
                }
                int plantid = Convert.ToInt32(tblmc.PlantID);
                ViewBag.Plant = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", tblmc.PlantID).ToList();
                ViewBag.DepartmentID = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == plantid), "ShopId", "ShopName", tblmc.ShopID).ToList();
                return View(tblmc);
            }
        }

        [HttpPost]
        public ActionResult EditCellDetails(CellsModel objcell, int shop = 0, int Plant = 0)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            int UserID = Convert.ToInt32(Session["UserID"]);
            //Cell name validation
            string cellname = objcell.Cells.CellName.ToString();
            int cellid = objcell.Cells.CellID;
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                var doesThisdeptExists = db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == Plant && m.ShopID == shop && m.CellName == cellname && m.CellID != cellid).ToList();
                if (doesThisdeptExists.Count == 0)
                {
                    var objCell = db.tblcells.Find(objcell.Cells.CellID);

                    objCell.PlantID = Plant;
                    objCell.ShopID = shop;
                    objCell.CellName = objcell.Cells.CellName;
                    objCell.CellDesc = objcell.Cells.CellDesc;
                    objCell.CelldisplayName = objcell.Cells.CelldisplayName;
                    db.Entry(objCell).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("CellsList");
                }
                else
                {
                    ViewBag.Plant = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", objcell.Cells.PlantID).ToList();
                    ViewBag.DepartmentID = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0), "DepartmentID", "DepartmentDisplayName", objcell.Cells.ShopID).ToList();
                    Session["Error"] = "Machine Name" + " " + objcell.Cells + " " + "already exists for this Department.";
                    return View(objcell);
                }
            }
        }
       

        //delete machine category
        public ActionResult DeleteCells(int id)
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

            //start Logging
            // int UserID = Convert.ToInt32(Session["UserId"]);
            // string CompleteModificationdetail = "Deleted Role";
            // Action = "Delete";
            // ActiveLogStorage Obj = new ActiveLogStorage();
            //Obj.SaveActiveLog(Action, Controller, Username, UserID, CompleteModificationdetail);
            //End
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                tblcell tblmc = db.tblcells.Find(id);
                int mcahineid = tblmc.CellID;
                tblmc.IsDeleted = 1;
                tblmc.ModifiedBy = 1;
                tblmc.ModifiedOn = DateTime.Now;
                db.Entry(tblmc).State =EntityState.Modified;
                db.SaveChanges();

                //delete corresponding machines
                var machinedata = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == mcahineid).ToList();
                foreach (var machinerow in machinedata)
                {
                    machinerow.IsDeleted = 1;
                    db.Entry(machinerow).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }


            return RedirectToAction("CellsList");
        }

        public ActionResult CellCategoryById(int Id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            var Data = db.tblcells.Where(m => m.CellID == Id).Select(m => new { PlantId = m.PlantID, DepartmentId = m.ShopID, machinecategory = m.CellName, catdesc = m.CellDesc, catdeisplay = m.CelldisplayName });
            return Json(Data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult FetchDept(int PID)
        {

            var DeptData = (from row in db.tblshops
                            where row.IsDeleted == 0 && row.PlantID == PID
                            select new { Value = row.ShopID, Text = row.Shopdisplayname }
                                ).ToList();
            return Json(DeptData, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public string CellNameDuplicateCheck(int plantID = 0,int shopId=0, string cellName = "")
        {
            string status = "notok";
            var doesThisCellExists = db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == plantID && m.ShopID == shopId && m.CellName == cellName).ToList();
            if (doesThisCellExists.Count == 0)
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
        public string CellNameDuplicateCheckEdit(int plantID = 0, int shopId = 0, string cellName = "", int editCellID = 0)
        {
            string status = "notok";
            var doesThisCellExists = db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == plantID && m.ShopID == shopId && m.CellName == cellName).ToList();
            if (doesThisCellExists.Count == 0)
            {
                status = "ok";
            }
            else
            {
                var checkforId = db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == plantID && m.ShopID == shopId && m.CellID==editCellID && m.CellName == cellName).ToList();//checks for that cellid
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
            var macChild = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == id).ToList();
            if (macChild.Count == 0)
            {
                status = "";
            }
            else
            {
                status = "The Cell is having dependent machines, Do you want to continue(If Yes every machine having this cell will be deleted)";
            }
            return status;
        }
    }
}