using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data;
using System.Data.Entity.Validation;
using SRKSDemo.Server_Model;

namespace SRKSDemo.Controllers
{
    public class PmCheckListController : Controller
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
            PmCheckList tblpm = new PmCheckList();
            tblpm.pmchecklistlist = condb.configuration_tblpmchecklist.Where(m => m.Isdeleted == 0).ToList();

            return View(tblpm);
        }

        [HttpGet]
        public ActionResult Create()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.plant = new SelectList(condb.tblplants.ToList().Where(p => p.IsDeleted == 0), "PlantID", "PlantName").ToList();
            ViewBag.shop = new SelectList(condb.tblshops.ToList().Where(d => d.IsDeleted == 0), "ShopId", "ShopName").ToList();
            ViewBag.cell = new SelectList(condb.tblcells.ToList().Where(d => d.IsDeleted == 0), "CellID", "CellName").ToList();
            ViewBag.TypeOfCheckPoint = new SelectList(condb.configuration_tblpmcheckpoint.ToList().Where(d => d.Isdeleted == 0), "pmcpID", "TypeOfCheckPoint").ToList();
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            return View();
        }


        public JsonResult InsertData(int Plant, int shop, int cell, string Value, string CheckList, string How, string Frequency, int TypeOfCheckPoint)
        {
            configuration_tblpmchecklist tblpc = new configuration_tblpmchecklist();
            tblpc.CreatedBy = 1;
            tblpc.CreatedOn = DateTime.Now;
            tblpc.Isdeleted = 0;
            tblpc.PlantID = Plant;
            tblpc.ShopID = shop;
            tblpc.CellID = cell;
            tblpc.How = How;
            tblpc.Frequency = Frequency;
            tblpc.Value = Value;
            tblpc.pmcpID = TypeOfCheckPoint;
            tblpc.CheckList = CheckList;
            condb.configuration_tblpmchecklist.Add(tblpc);
            condb.SaveChanges();

            return Json(tblpc.pmcid, JsonRequestBehavior.AllowGet);
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
                var pmd = condb.configuration_tblpmchecklist.Where(m => m.pmcid == id).Select(m => m.pmcpID).FirstOrDefault();
                var tblpm = condb.configuration_tblpmchecklist.Where(m => m.pmcid == id).FirstOrDefault();

                var tbl = condb.configuration_tblpmchecklist.Where(m => m.pmcid == id || m.pmcpID == pmd && m.Isdeleted == 0).ToList();
                if (tbl == null)
                {
                    return HttpNotFound();
                }
                List<PmCheckList> obj = new List<PmCheckList>();
                foreach (var item in tbl)
                {
                    PmCheckList pm1 = new PmCheckList();
                    ViewBag.plant = new SelectList(condb.tblplants.ToList().Where(p => p.IsDeleted == 0), "PlantID", "PlantName", item.PlantID).ToList();
                    ViewBag.shop = new SelectList(condb.tblshops.ToList().Where(d => d.IsDeleted == 0), "ShopId", "ShopName", item.ShopID).ToList();
                    ViewBag.cell = new SelectList(condb.tblcells.ToList().Where(d => d.IsDeleted == 0), "CellID", "CellName", item.CellID).ToList();
                    ViewBag.TypeOfCheckPoint = new SelectList(condb.configuration_tblpmcheckpoint.ToList().Where(d => d.Isdeleted == 0), "pmcpID", "TypeOfCheckPoint", item.pmcpID).ToList();
                    pm1.pmchecklist = item;
                    obj.Add(pm1);
                }
                return View(obj);
            }
        }
        public string update(int plant, int shop, int cell, string value, string frequency, int pmcpid, int pmcid, string checklist, string How)
        {
            string res = "";
            using (i_facility_shaktiEntities1 condb = new i_facility_shaktiEntities1())
            {
                var tblpc = condb.configuration_tblpmchecklist.Find(pmcid);

                tblpc.Isdeleted = 0;
                tblpc.PlantID = plant;
                tblpc.ShopID = shop;
                tblpc.CellID = cell;
                tblpc.pmcpID = pmcpid;
                tblpc.How = How;
                tblpc.Value = value;
                tblpc.Frequency = frequency;
                tblpc.CheckList = checklist;
                tblpc.ModifiedBy = 1;
                tblpc.ModifiedOn = DateTime.Now;
                condb.Entry(tblpc).State = EntityState.Modified;
                condb.SaveChanges();
                res = "Success";
                return res;
            }
        }

        public string update1(int plant, int shop, int cell, string value, string frequency, int pmcpid, int pmcid, string checklist, string How)
        {
            string res = "";
            using (i_facility_shaktiEntities1 condb = new i_facility_shaktiEntities1())
            {
                var tblpc = condb.configuration_tblpmchecklist.Find(pmcid);

                tblpc.Isdeleted = 0;
                tblpc.PlantID = plant;
                tblpc.ShopID = shop;
                tblpc.CellID = cell;
                tblpc.pmcpID = pmcpid;
                tblpc.Value = value;
                tblpc.How = How;
                tblpc.Frequency = frequency;
                tblpc.CheckList = checklist;
                tblpc.ModifiedBy = 1;
                tblpc.ModifiedOn = DateTime.Now;
                condb.Entry(tblpc).State = EntityState.Modified;
                condb.SaveChanges();
                res = "Success";
                return res;
            }
        }

        public JsonResult GetCheckListById(int Id)
        {
            var Data = condb.configuration_tblpmchecklist.Where(m => m.pmcid == Id).Select(m => new { cell = m.CellID, plant = m.PlantID, shop = m.ShopID });
            return Json(Data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Delete(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            int UserID = Convert.ToInt32(Session["UserId"]);
            String Username = Session["Username"].ToString();
            using (i_facility_shaktiEntities1 condb = new i_facility_shaktiEntities1())
            {
                var tblpm = condb.configuration_tblpmchecklist.Where(m => m.pmcid == id).FirstOrDefault();
                tblpm.Isdeleted = 1;
                tblpm.ModifiedBy = UserID;
                tblpm.ModifiedOn = DateTime.Now;
                condb.Entry(tblpm).State = EntityState.Modified;
                condb.SaveChanges();
                TempData["toaster_success"] = "Data Deleted successfully";
                return RedirectToAction("Index");
            }
        }

        public JsonResult DeleteData(int id = 0)
        {
            var tblpm = condb.configuration_tblpmchecklist.Where(m => m.pmcid == id).FirstOrDefault();
            tblpm.Isdeleted = 1;
            tblpm.ModifiedBy = 1;
            tblpm.ModifiedOn = DateTime.Now;
            condb.Entry(tblpm).State = EntityState.Modified;
            condb.SaveChanges();
            return Json(tblpm.pmcid, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FetchCheckPoint(int CID)
        {
            using (i_facility_shaktiEntities1 condb = new i_facility_shaktiEntities1())
            {
                var checkPointData = (from row in condb.configuration_tblpmcheckpoint
                                      where row.Isdeleted == 0 && row.CellID == CID
                                      select new { Value = row.pmcpID, Text = row.TypeofCheckpoint }).ToList();
                return Json(checkPointData, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult FetchCell(int SID)
        {
            using (i_facility_shaktiEntities1 condb = new i_facility_shaktiEntities1())
            {
                var checkPointData = (from row in condb.tblcells
                                      where row.IsDeleted == 0 && row.ShopID == SID
                                      select new { Value = row.CellID, Text = row.CellName }).ToList();
                return Json(checkPointData, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult FetchShop(int PID)
        {
            using (i_facility_shaktiEntities1 condb = new i_facility_shaktiEntities1())
            {
                var checkPointData = (from row in condb.tblshops
                                      where row.IsDeleted == 0 && row.PlantID == PID
                                      select new { Value = row.ShopID, Text = row.ShopName }).ToList();
                return Json(checkPointData, JsonRequestBehavior.AllowGet);
            }
        }
    }

}