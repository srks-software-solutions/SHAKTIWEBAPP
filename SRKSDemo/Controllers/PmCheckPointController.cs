using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data;
using SRKSDemo.Server_Model;

namespace SRKSDemo.Controllers
{
    public class PmCheckPointController : Controller
    {
        i_facility_shaktiEntities1 condb = new i_facility_shaktiEntities1();
        // GET: PmCheckPoint
        public ActionResult Index()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            PmCheckPoint tblpm = new PmCheckPoint();
            tblpm.pmCheckPointlist = condb.configuration_tblpmcheckpoint.Where(m => m.Isdeleted == 0).ToList();
            return View(tblpm);
        }
        public ActionResult Create()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.plant = new SelectList(condb.tblplants.ToList().Where(p => p.IsDeleted == 0), "PlantID", "PlantName").ToList();
            ViewBag.shop = new SelectList(condb.tblshops.ToList().Where(d => d.IsDeleted == 0), "ShopId", "ShopName").ToList();
            ViewBag.cell = new SelectList(condb.tblcells.ToList().Where(d => d.IsDeleted == 0), "CellID", "CellName").ToList();
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            return View();
        }

        public JsonResult InsertData(int Plant, int shop, int cell, string Value, string CheckList, string Frequency, string TypeOfCheckPoint, string How)
        {
            configuration_tblpmcheckpoint tblpc = new configuration_tblpmcheckpoint();
            tblpc.CreatedBy = 1;
            tblpc.CreatedOn = DateTime.Now;
            tblpc.Isdeleted = 0;
            tblpc.PlantID = Plant;
            tblpc.ShopID = shop;
            tblpc.How = How;
            tblpc.CellID = cell;
            tblpc.frequency = Frequency;
            tblpc.Value = Value;
            tblpc.CheckList = CheckList;
            tblpc.TypeofCheckpoint = TypeOfCheckPoint;
            condb.configuration_tblpmcheckpoint.Add(tblpc);
            condb.SaveChanges();

            return Json(tblpc.pmcpID, JsonRequestBehavior.AllowGet);
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
                var tblpm = condb.configuration_tblpmcheckpoint.Where(m => m.pmcpID == id && m.Isdeleted == 0).FirstOrDefault();
                if (tblpm == null)
                {
                    return HttpNotFound();
                }
                PmCheckPoint pm = new PmCheckPoint();
                pm.pmCheckPoint = tblpm;
                ViewBag.plant = new SelectList(condb.tblplants.ToList().Where(p => p.IsDeleted == 0), "PlantID", "PlantName", tblpm.PlantID).ToList();
                ViewBag.shop = new SelectList(condb.tblshops.ToList().Where(d => d.IsDeleted == 0), "ShopId", "ShopName", tblpm.ShopID).ToList();
                ViewBag.cell = new SelectList(condb.tblcells.ToList().Where(d => d.IsDeleted == 0), "CellID", "CellName", tblpm.CellID).ToList();
                return View(pm);
            }
        }
        public string update(int plant, int shop, int cell, string typeofcheckpoint, string value, string frequency, int pmcpid, string checklist, string How)
        {
            string res = "";
            var tblpc = condb.configuration_tblpmcheckpoint.Find(pmcpid);

            tblpc.Isdeleted = 0;
            tblpc.PlantID = plant;
            tblpc.ShopID = shop;
            tblpc.CellID = cell;
            tblpc.pmcpID = pmcpid;
            tblpc.frequency = frequency;
            tblpc.Value = value;
            tblpc.How = How;
            tblpc.CheckList = checklist;
            tblpc.TypeofCheckpoint = typeofcheckpoint;
            tblpc.ModifiedBy = ViewBag.roleid;
            tblpc.ModifiedOn = DateTime.Now;
            condb.Entry(tblpc).State = EntityState.Modified;
            condb.SaveChanges();
            res = "Success";
            return res;
        }

        public JsonResult GetcheckpointById(int Id)
        {
            using (i_facility_shaktiEntities1 condb = new i_facility_shaktiEntities1())
            {
                var Data = condb.configuration_tblpmcheckpoint.Where(m => m.CellID == Id).Select(m => new { pmcpid = m.pmcpID, plant = m.PlantID, shop = m.ShopID, cell = m.CellID, typeofcheckpoint = m.TypeofCheckpoint, frequency = m.frequency, value = m.Value, checklist = m.CheckList, how = m.How });
                return Json(Data, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult FetchCell(int SID)
        {
            using (i_facility_shaktiEntities1 condb = new i_facility_shaktiEntities1())
            {
                var CellData = (from row in condb.tblcells
                                where row.IsDeleted == 0 && row.ShopID == SID
                                select new { Value = row.CellID, Text = row.CelldisplayName }).ToList();
                return Json(CellData, JsonRequestBehavior.AllowGet);
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
            int UserID = Convert.ToInt32(Session["UserId"]);
            String Username = Session["Username"].ToString();
            //Getting all the details of that particular id in PMCheckPoint table
            configuration_tblpmcheckpoint tblmc = condb.configuration_tblpmcheckpoint.Find(id);

            using (i_facility_shaktiEntities1 condb = new i_facility_shaktiEntities1())
            {
                //Getting all the details of that particular id in PMCheckList table
                var data = condb.configuration_tblpmcheckpoint.Where(m => m.pmcpID == id && m.Isdeleted == 0).ToList();
                foreach (var row in data)
                {
                    //deleting the perticular row of that id in tblPMCheckList table
                    row.Isdeleted = 1;
                    condb.SaveChanges();
                }
                //And deleting the row present in tblPMCheckPoint table

            }
            tblmc.Isdeleted = 1;
            condb.SaveChanges();
            TempData["toaster_success"] = "Data Deleted successfully";
            return RedirectToAction("Index");

        }

        public JsonResult DeleteData(int id = 0)
        {
            var tblpm = condb.configuration_tblpmcheckpoint.Where(m => m.pmcpID == id).FirstOrDefault();
            tblpm.Isdeleted = 1;
            tblpm.ModifiedBy = 1;
            tblpm.ModifiedOn = DateTime.Now;
            condb.Entry(tblpm).State = EntityState.Modified;
            condb.SaveChanges();
            return Json(tblpm.pmcpID, JsonRequestBehavior.AllowGet);
        }

    }
}