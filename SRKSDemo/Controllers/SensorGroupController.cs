
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SRKSDemo.Server_Model;
using SRKSDemo.Models;

namespace SRKSDemo.Controllers
{
    public class SensorGroupController : Controller
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
        // GET: SensorGroup
        public ActionResult IndexSensorGroup()
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
                sensormodel pa = new sensormodel();
                configuration_tblsensorgroup mp = new configuration_tblsensorgroup();
                pa.sensorgroup = mp;
                pa.sensorList = db.configuration_tblsensorgroup.Where(m => m.IsDeleted == 0).ToList();
                return View(pa);
            }
        }
        [HttpGet]
        public ActionResult CreateSensorGroup()
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
        public ActionResult CreateSensorGroup(sensormodel tblp)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            string sensorName = tblp.sensorgroup.SensorGroupName.ToString();

            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                var doesThisExist = db.configuration_tblsensorgroup.Where(m => m.IsDeleted == 0 && m.SensorGroupName == sensorName).ToList();
                if (doesThisExist.Count == 0)
                {
                    tblp.sensorgroup.CreatedBy = ViewBag.roleid;
                    tblp.sensorgroup.CreatedOn = DateTime.Now;
                    tblp.sensorgroup.IsDeleted = 0;
                    db.configuration_tblsensorgroup.Add(tblp.sensorgroup);
                    db.SaveChanges();
                    TempData["toaster_success"] = "Data Saved successfully";
                    return RedirectToAction("IndexSensorGroup");
                }
                else
                {
                    TempData["toaster_warning"] = "This Record Already Exists";
                    return View(tblp);
                }
            }
        }
        [HttpGet]
        public ActionResult EditSensorGroup(int id)
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
                configuration_tblsensorgroup tblmc = db.configuration_tblsensorgroup.Find(id);
                if (tblmc == null)
                {
                    return HttpNotFound();
                }
                sensormodel sd = new sensormodel();
                sd.sensorgroup = tblmc;
                return View(sd);
            }
        }
        [HttpPost]
        public ActionResult EditSensorGroup(sensormodel tblmc)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            int UserID = Convert.ToInt32(Session["UserID"]);
            string sensorName = tblmc.sensorgroup.SensorGroupName.ToString();
            int sid = tblmc.sensorgroup.SID;
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                var doesThisPlantExist = db.configuration_tblsensorgroup.Where(m => m.IsDeleted == 0 && m.SensorGroupName == sensorName && m.SID != sid).ToList();
                if (doesThisPlantExist.Count == 0)
                {
                    var sensor = db.configuration_tblsensorgroup.Find(tblmc.sensorgroup.SID);
                    sensor.SensorDesc = tblmc.sensorgroup.SensorDesc;
                    sensor.SensorGroupName = sensorName;
                    sensor.ModifiedBy = ViewBag.roleid;
                    sensor.ModifiedOn = DateTime.Now;
                    db.Entry(sensor).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["toaster_success"] = "Data Updated successfully";
                    return RedirectToAction("IndexSensorGroup");
                }
                else
                {
                    TempData["toaster_warning"] = "This Record Already Exists";
                    return View(tblmc);
                }
            }
        }
        public JsonResult GetSensorById(int Id)
        {
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                var Data = db.configuration_tblsensorgroup.Where(m => m.SID == Id).Select(m => new { sensorname = m.SensorGroupName, sensordesc = m.SensorDesc });
                return Json(Data, JsonRequestBehavior.AllowGet);
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
            String Username = Session["Username"].ToString();
            int UserID1 = id;
            int UserID = Convert.ToInt32(Session["UserId"]);
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                var tblpm = db.configuration_tblsensorgroup.Where(m => m.SID == id).FirstOrDefault();
                //tblpmchecklist tblpm = db.tblpmchecklists.Find(id);
                tblpm.IsDeleted = 1;
                tblpm.ModifiedBy = UserID;
                tblpm.ModifiedOn = DateTime.Now;
                db.Entry(tblpm).State = EntityState.Modified;
                db.SaveChanges();
                TempData["toaster_success"] = "Data Deleted successfully";
                return RedirectToAction("IndexSensorGroup");
            }

        }
    }
}