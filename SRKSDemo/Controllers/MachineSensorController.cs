
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SRKSDemo.Server_Model;
using SRKSDemo.Models;

namespace SRKSDemo.Controllers
{
    public class MachineSensorController : Controller
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
        // GET: MachineSensor
        public ActionResult IndexMachineSensor()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            //using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            //{
            sensormachinemodel pa = new sensormachinemodel();
            configurationtblmachinesensor mp = new configurationtblmachinesensor();
            pa.machinesensor = mp;
            pa.machinesensorList = db.configurationtblmachinesensors.Where(m => m.IsDeleted == 0).ToList();
            return View(pa);
            //}
        }
        [HttpGet]
        public ActionResult CreateMachineSensor()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            ViewBag.MachineName = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0), "MachineId", "MachineName").ToList();
            ViewBag.SensorGroupName = new SelectList(db.configuration_tblsensorgroup.Where(m => m.IsDeleted == 0), "SID", "SensorGroupName").ToList();
            return View();
        }
        public string InsertData(string ip, int machineid, int sid, int portno)
        {
            string res = "";

            //var doesThisdataExist = db.configurationtblmachinesensors.Where(m => m.IsDeleted == 0 && m.Sid == sid && m.MachineId == machineid).ToList();
            //if (doesThisdataExist.Count == 0)
            //{
            configurationtblmachinesensor tblpc = new configurationtblmachinesensor();
            tblpc.CreatedBy = 1;
            tblpc.CreatedOn = DateTime.Now;
            tblpc.IsDeleted = 0;
            tblpc.IPAddress = ip;
            tblpc.MachineId = machineid;
            tblpc.Sid = sid;
            tblpc.PortNo = portno;
            db.configurationtblmachinesensors.Add(tblpc);
            db.SaveChanges();
            TempData["toaster_success"] = "Data Saved successfully";
            res = "success";
            return res;
            //}
            //else
            //{
            //    TempData["toaster_error"] = "Sensor Group Name already Exists";
            //    res = "success";

            //}
            //return Json(tblpc.SdlID, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult EditMachineSensor(int id)
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
                configurationtblmachinesensor tblmc = db.configurationtblmachinesensors.Find(id);
                if (tblmc == null)
                {
                    return HttpNotFound();
                }
                ViewBag.MachineName = new SelectList(db.tblmachinedetails.ToList().Where(m => m.IsDeleted == 0), "MachineId", "MachineName", tblmc.MachineId).ToList();
                ViewBag.SensorGroupName = new SelectList(db.configuration_tblsensorgroup.ToList().Where(m => m.IsDeleted == 0), "SID", "SensorGroupName", tblmc.Sid).ToList();
                sensormachinemodel sd = new sensormachinemodel();
                sd.machinesensor = tblmc;
                return View(sd);
            }
        }
        [HttpPost]
        public ActionResult EditMachineSensor(sensormachinemodel tblmc)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            int UserID = Convert.ToInt32(Session["UserID"]);
            string sensorName = db.configuration_tblsensorgroup.Where(m => m.SID == tblmc.machinesensor.Sid).Select(m => m.SensorGroupName).FirstOrDefault();
            string macname = db.tblmachinedetails.Where(m => m.MachineID == tblmc.machinesensor.MachineId).Select(m => m.MachineName).FirstOrDefault();
            int msid = tblmc.machinesensor.MSID;
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                var doesThisExist = db.configurationtblmachinesensors.Where(m => m.IsDeleted == 0 && m.MSID != msid && m.Sid == tblmc.machinesensor.Sid && m.MachineId == tblmc.machinesensor.MachineId && m.Sid == tblmc.machinesensor.Sid).ToList();
                if (doesThisExist.Count == 0)
                {
                    var sensor = db.configurationtblmachinesensors.Find(tblmc.machinesensor.MSID);
                    sensor.MachineId = tblmc.machinesensor.MachineId;
                    sensor.Sid = tblmc.machinesensor.Sid;
                    sensor.IPAddress = tblmc.machinesensor.IPAddress;
                    if (sensor.IPAddress == null)
                    {
                        sensor.IPAddress = "0";
                    }
                    sensor.PortNo = tblmc.machinesensor.PortNo;
                    sensor.MachineId = tblmc.machinesensor.MachineId;
                    sensor.ModifiedBy = ViewBag.roleid;
                    sensor.ModifiedOn = DateTime.Now;
                    db.Entry(sensor).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["toaster_success"] = "Data Updated successfully";
                    return RedirectToAction("IndexMachineSensor");
                }
                else
                {
                    TempData["toaster_error"] = "Sensor Group Name already exists";
                    return View(tblmc);
                }
            }
        }

        //public JsonResult FetchMachine(int SID)
        //{
        //    using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
        //    {
        //        var SensorData = (from row in db.configuration_tblSensorgroup
        //                        where row.IsDeleted == 0 && row.SID == SID
        //                        select new { Value = row.SID, Text = row.SensorGroupName }).ToList();
        //        return Json(SensorData, JsonRequestBehavior.AllowGet);
        //    }
        //}
        public JsonResult GetmachinesensorById(int Id)
        {
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                var Data = db.configurationtblmachinesensors.Where(m => m.MSID == Id).Select(m => new { ipaddress = m.IPAddress, portno = m.PortNo });
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
                var tblpm = db.configurationtblmachinesensors.Where(m => m.MSID == id).FirstOrDefault();
                //tblpmchecklist tblpm = db.tblpmchecklists.Find(id);
                tblpm.IsDeleted = 1;
                tblpm.ModifiedBy = UserID;
                tblpm.ModifiedOn = DateTime.Now;
                db.Entry(tblpm).State = EntityState.Modified;
                db.SaveChanges();
                TempData["toaster_success"] = "Data Deleted successfully";
                return RedirectToAction("IndexMachineSensor");
            }

        }

        [HttpPost]
        public string sensornameDuplicateCheck(int sensor, int machine)
        {
            string status = "notok";
            var doesThisExist = db.configurationtblmachinesensors.Where(m => m.IsDeleted == 0 && m.Sid == sensor && m.MachineId == machine).ToList();
            if (doesThisExist.Count == 0)
            {
                status = "ok";
            }
            else
            {
                status = "notok";
            }
            return status;

        }
    }
}