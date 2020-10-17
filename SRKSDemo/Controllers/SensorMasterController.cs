
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SRKSDemo.Models;
using SRKSDemo.Server_Model;
namespace SRKSDemo.Controllers
{
    public class SensorMasterController : Controller
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();

        // GET: SensorMaster
        public ActionResult Index()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            SensorMaster pa = new SensorMaster();
            configurationtblsensormaster mp = new configurationtblsensormaster();
            pa.sensormaster = mp;
            pa.sensormasterList = db.configurationtblsensormasters.Where(m => m.IsDeleted == 0).ToList();
            return View(pa);

        }
        [HttpGet]
        public ActionResult CreateSensorMaster()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            ViewBag.Unit = new SelectList(db.tblunits.Where(m => m.IsDeleted == 0), "U_id", "Unit").ToList();
            ViewBag.SensorGroupName = new SelectList(db.configuration_tblsensorgroup.Where(m => m.IsDeleted == 0), "SID", "SensorGroupName").ToList();
            ViewBag.SensorDataLink = new SelectList(db.configurationtblsensordatalinks.Where(m => m.IsDeleted == 0 && m.IsSensor == 1), "ParameterTypeID", "parametername").ToList();
            return View();
        }
        public string InsertDatawithio(int channelnum, string memoryadd, int sid, string sensordesc, int modeltype,
                decimal scalingfac, int uid, int sdlid)
        {
            string res = "";
            //var doesThisExist = db.configurationtblsensormasters.Where(m => m.IsDeleted == 0 && m.ChannelNo == channelnum && m.MemoryAddress == memoryadd && m.Sid == sid && m.SensorDesc == sensordesc && m.IsAnalog == modeltype && m.scalingFactor == scalingfac && m.Unitid == uid && m.parametertypeid == sdlid).ToList();
            //if (doesThisExist.Count == 0)
            //{

            configurationtblsensormaster tblpc = new configurationtblsensormaster();
            tblpc.CreatedBy = 1;
            tblpc.CreatedOn = DateTime.Now;
            tblpc.IsDeleted = 0;
            tblpc.IsAnalog = modeltype;
            tblpc.ChannelNo = channelnum;
            tblpc.MemoryAddress = Convert.ToInt32(memoryadd);
            tblpc.Sid = sid;
            tblpc.SensorDesc = sensordesc;
            tblpc.scalingFactor = scalingfac;
            tblpc.Unitid = uid;
            tblpc.parametertypeid = sdlid;
            db.configurationtblsensormasters.Add(tblpc);
            db.SaveChanges();
            res = "success";
            // }
            //else
            // {
            //     TempData["Message"] = "This Record is already Exists";
            //     return res;
            // }
            return res;
        }
        public string InsertData(int channelnum, string memoryadd, int sid, string sensordesc, int countlow, int counthigh, int sensorlimithigh, int sensorlimitlow, int uid, int sdlid, int modeltype)
        {
            string res = "";
            //var doesThisExist = db.configurationtblsensormasters.Where(m => m.IsDeleted == 0 && m.ChannelNo == channelnum && m.MemoryAddress == memoryadd && m.Sid == sid && m.SensorDesc == sensordesc && m.CountLow == countlow && m.CountHigh == counthigh  && m.sensorlimitLow == sensorlimitlow && m.sensorlimitHigh == sensorlimithigh && m.IsAnalog == modeltype).ToList();
            //if (doesThisExist.Count == 0)
            //{

            configurationtblsensormaster tblpc = new configurationtblsensormaster();
            tblpc.CreatedBy = 1;
            tblpc.CreatedOn = DateTime.Now;
            tblpc.IsDeleted = 0;
            tblpc.ChannelNo = channelnum;
            tblpc.MemoryAddress = Convert.ToInt32(memoryadd);
            tblpc.Sid = sid;
            tblpc.SensorDesc = sensordesc;
            tblpc.Unitid = uid;
            tblpc.CountLow = countlow;
            tblpc.IsAnalog = modeltype;
            tblpc.CountHigh = counthigh;
            tblpc.sensorlimitHigh = sensorlimithigh;
            tblpc.sensorlimitLow = sensorlimitlow;
            tblpc.parametertypeid = sdlid;
            db.configurationtblsensormasters.Add(tblpc);
            db.SaveChanges();
            res = "success";
            // }
            //else
            //{
            //    TempData["Message"] = "Sensor Name already Exists";
            //    res = "failure";
            //    return res;
            //}
            return res;
        }



        public ActionResult EditSensorMaster(int id)
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
                configurationtblsensormaster tblmc = db.configurationtblsensormasters.Find(id);
                if (tblmc == null)
                {
                    return HttpNotFound();
                }
                ViewBag.Unit = new SelectList(db.tblunits.Where(m => m.IsDeleted == 0), "U_id", "Unit", tblmc.Unitid).ToList();
                ViewBag.SensorGroupName = new SelectList(db.configuration_tblsensorgroup.ToList().Where(m => m.IsDeleted == 0), "SID", "SensorGroupName", tblmc.Sid).ToList();
                ViewBag.SensorDataLink = new SelectList(db.configurationtblsensordatalinks.Where(m => m.IsDeleted == 0 && m.IsSensor == 1), "ParameterTypeID", "parametername", tblmc.parametertypeid).ToList();
                SensorMaster sd = new SensorMaster();
                sd.sensormaster = tblmc;
                return View(sd);
            }
        }

        public string UpdateData(int smid, int channelnum, string memoryadd, int sid, string sensordesc, int modeltype, int countlow, int counthigh, int sensorlimithigh, int sensorlimitlow, int uid, int sdlid)
        {
            string res = "";
            //var doesThissensorExist = db.configurationtblsensormasters.Where(m => m.IsDeleted == 0 && m.ChannelNo == channelnum && m.CountHigh == counthigh && m.CountLow == countlow && m.MemoryAddress == memoryadd && m.IsAnalog == modeltype && m.SensorDesc == sensordesc && m.sensorlimitHigh == sensorlimithigh && m.sensorlimitLow == sensorlimitlow && m.SMID != smid).ToList();
            //if (doesThissensorExist.Count == 0)
            //{
            var sensor = db.configurationtblsensormasters.Find(smid);
            sensor.ChannelNo = channelnum;
            sensor.CountHigh = counthigh;
            sensor.CountLow = countlow;
            sensor.MemoryAddress = Convert.ToInt32(memoryadd);
            sensor.IsAnalog = modeltype;
            sensor.SensorDesc = sensordesc;
            sensor.sensorlimitHigh = sensorlimithigh;
            sensor.sensorlimitLow = sensorlimitlow;
            sensor.Sid = sid;
            sensor.parametertypeid = sdlid;
            sensor.Unitid = uid;
            sensor.ModifiedBy = ViewBag.roleid;
            sensor.ModifiedOn = DateTime.Now;
            db.Entry(sensor).State = EntityState.Modified;
            db.SaveChanges();
            res = "success";
            // }
            //else
            //{
            //    TempData["Message"] = "parameter Name already Exists";
            //    res = "failure";
            //    return res;
            //}
            return res;
            //return Json(tblpc.SdlID, JsonRequestBehavior.AllowGet);
        }

        public string UpDataDatawithio(int smid, int channelnum, string memoryadd, int sid, string sensordesc, int modeltype,
                decimal scalingfac, int uid, int sdlid)
        {
            string res = "";
            //var doesThisExist = db.configurationtblsensormasters.Where(m => m.IsDeleted == 0 && m.ChannelNo == channelnum && m.MemoryAddress == memoryadd && m.Sid == sid && m.SensorDesc == sensordesc && m.IsAnalog == modeltype && m.scalingFactor == scalingfac && m.Unitid == uid && m.parametertypeid == sdlid && m.SMID != smid).ToList();
            //if (doesThisExist.Count == 0)
            //{
            var sensor = db.configurationtblsensormasters.Find(smid);
            sensor.ChannelNo = channelnum;
            sensor.MemoryAddress = Convert.ToInt32(memoryadd);
            sensor.IsAnalog = modeltype;
            sensor.SensorDesc = sensordesc;
            sensor.scalingFactor = scalingfac;
            sensor.Sid = sid;
            sensor.parametertypeid = sdlid;
            sensor.Unitid = uid;
            sensor.ModifiedBy = ViewBag.roleid;
            sensor.ModifiedOn = DateTime.Now;
            db.Entry(sensor).State = EntityState.Modified;
            db.SaveChanges();
            res = "success";
            //}
            //else
            //{
            //    TempData["Message"] = "parameter Name already Exists";
            //    res = "failure";
            //    return res;
            //}
            return res;
            //return Json(tblpc.SdlID, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetSensorById(int Id)
        {

            var Data = db.configurationtblsensormasters.Where(m => m.SMID == Id).Select(m => new { Editchannelno = m.ChannelNo, scalingfac = m.scalingFactor, Editmemoryadd = m.MemoryAddress, EditSensorDesc = m.SensorDesc });
            return Json(Data, JsonRequestBehavior.AllowGet);

        }
        public JsonResult GetSensoranalogById(int Id)
        {

            var Data = db.configurationtblsensormasters.Where(m => m.SMID == Id).Select(m => new { Editchannelno = m.ChannelNo, counthigh = m.CountHigh, countlow = m.CountLow, Editmemoryadd = m.MemoryAddress, issensor = m.IsAnalog, EditSensorDesc = m.SensorDesc, sensorlimithigh = m.sensorlimitHigh, sensorlimitlow = m.sensorlimitLow });
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
            String Username = Session["Username"].ToString();
            int UserID1 = id;
            int UserID = Convert.ToInt32(Session["UserId"]);
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                var tblpm = db.configurationtblsensormasters.Where(m => m.SMID == id).FirstOrDefault();
                //tblpmchecklist tblpm = db.tblpmchecklists.Find(id);
                tblpm.IsDeleted = 1;
                tblpm.ModifiedBy = UserID;
                tblpm.ModifiedOn = DateTime.Now;
                db.Entry(tblpm).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

        }
    }
}