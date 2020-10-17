using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using SRKSDemo.Server_Model;
using SRKSDemo.Models;

namespace SRKSDemo.Controllers
{

    public class MachineHealthController : Controller
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
        // GET: MachineHealth
        public ActionResult Index()
        {
            var sensormachine = db.configurationtblmachinesensors.Where(m => m.IsDeleted == 0).Select(m => m.MachineId).Distinct().ToList();
            foreach (var sensormachinedet in sensormachine)
            {
                var plant = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == sensormachinedet).Select(m => m.PlantID).ToList();
                foreach (var plantdet in plant)
                {
                    ViewData["Plant"] = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0 && m.PlantID == plantdet), "PlantID", "PlantName");
                }
                ViewData["Shop"] = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0), "ShopID", "ShopName");
                ViewData["Cell"] = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0), "CellID", "CellName");
                ViewData["Machine"] = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0), "MachineID", "MachineName");
                ViewData["Parameter"] = new SelectList(db.configurationtblsensormasters.Where(m => m.IsDeleted == 0), "SMID", "SensorDesc");
            }
            return View();

        }

        public string FetchShop(int PID)
        {
            string res = "";
            var sensormachine = db.configurationtblmachinesensors.Where(m => m.IsDeleted == 0).Select(m => m.MachineId).Distinct().ToList();
            foreach (var sensormachinedet in sensormachine)
            {
                var shop = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == sensormachinedet).Select(m => m.ShopID).ToList();
                foreach (var shopdet in shop)
                {
                    var shopdetails = (from s in db.tblshops where s.PlantID == PID && s.IsDeleted == 0 && s.ShopID == shopdet select new { Value = s.ShopID, Text = s.Shopdisplayname }).ToList();
                    res = JsonConvert.SerializeObject(shopdetails);
                }

            }
            return res;
        }

        public string Fetchcell(int SID)
        {
            string res = "";
            var sensormachine = db.configurationtblmachinesensors.Where(m => m.IsDeleted == 0).Select(m => m.MachineId).Distinct().ToList();
            foreach (var sensormachinedet in sensormachine)
            {
                var cell = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == sensormachinedet).Select(m => m.CellID).ToList();
                foreach (var celldet in cell)
                {
                    var celldetails = (from s in db.tblcells where s.ShopID == SID && s.IsDeleted == 0 && s.CellID == celldet select new { Value = s.CellID, Text = s.CelldisplayName }).ToList();
                    res = JsonConvert.SerializeObject(celldetails);
                }
            }
            return res;
        }

        public string FetchMachine(int CID)
        {
            string res = "";
            var sensormachine = db.configurationtblmachinesensors.Where(m => m.IsDeleted == 0).Select(m => m.MachineId).Distinct().ToList();
            foreach (var sensormachinedet in sensormachine)
            {
                tblmachinedetail obj = new tblmachinedetail();
                var machinedet = (from m in db.tblmachinedetails where m.IsDeleted == 0 && m.CellID == CID && m.MachineID == sensormachinedet select new { Value = m.MachineID, Text = m.MachineDisplayName }).ToList();
                res = JsonConvert.SerializeObject(machinedet);
            }
            return res;

        }

        public string Fetchsensor(int MID)
        {
            string res = "";
            List<cbmparametermodel> cbmlist = new List<cbmparametermodel>();
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                var sensordata = (from sensor in db.configurationtblmachinesensors
                                  where sensor.MachineId == MID
                                  select sensor.Sid).Distinct().ToList();
                foreach (var sensordatarow in sensordata)
                {

                    var sensormasterdata = (from sensormaster in db.configurationtblsensormasters
                                            where sensormaster.Sid == sensordatarow
                                            select new
                                            {
                                                Value = sensormaster.SMID,
                                                Text = sensormaster.SensorDesc
                                            }).ToList();

                    foreach (var item in sensormasterdata)
                    {
                        cbmparametermodel obj = new cbmparametermodel();
                        obj.Value = item.Value;
                        obj.Text = item.Text;
                        cbmlist.Add(obj);

                    }


                }
                res = JsonConvert.SerializeObject(cbmlist);
                return res;
            }
        }

        public string GetParameters(int id)
        {

            string res = " ";
            //string corrected = GetCorrectedDate();
            //var corrected = Convert.ToDateTime(corrected);
            string corrected = "2019-05-20";
           // var correcteddate = Convert.ToDateTime(corrected);
            List<MachineHealth> machineHealthList = new List<MachineHealth>();
            MachineHealth objmahchinehealth = new MachineHealth();
            var sensormasterrow = db.configurationtblsensormasters.Where(m => m.SMID == id).Select(m => new { m.Sid, m.MemoryAddress, m.parametertypeid, m.Unitid, m.sensorlimitHigh, m.sensorlimitLow }).FirstOrDefault();

            var unitdet = db.tblunits.Where(m => m.U_ID == sensormasterrow.Unitid).Select(m => m.Unit).FirstOrDefault();

            List<Health> cbmList = new List<Health>();

            var memadd = Convert.ToInt32(sensormasterrow.MemoryAddress);

            var sensordatalinkdata = db.configurationtblsensordatalinks.Where(m => m.ParameterTypeID == sensormasterrow.parametertypeid).FirstOrDefault();
            objmahchinehealth.LSL = sensordatalinkdata.LSL;
            objmahchinehealth.USL = sensordatalinkdata.USL;
            //objmahchinehealth.min = Convert.ToInt32(sensormasterrow.sensorlimitLow);
            //objmahchinehealth.max = Convert.ToInt32(sensormasterrow.sensorlimitHigh);
            objmahchinehealth.min = Convert.ToInt32(objmahchinehealth.LSL - ((30 * objmahchinehealth.LSL) / 100));
            objmahchinehealth.max = Convert.ToInt32(objmahchinehealth.USL + ((30 * objmahchinehealth.USL) / 100));
            objmahchinehealth.unit = unitdet;

            //var data1 = livedb.tbl_livetblsensorvalue.Where(m => m.SensorMasterID == id && m.CorrectedDate == correctedDate).OrderBy(m => m.sensorvalueid).Take(30).ToList();
            var data1 = db.tbl_livetblsensorvalue.Where(m => m.SensorMasterID == id && m.CorrectedDate == corrected).OrderByDescending(m => m.sensorvalueid).ToList();
            //var data1 = livedb.tbl_livecbmdetails.Where(m => m.SensorMasterID == id && m.CorrectedDate == correctedDate).OrderByDescending(m => m.cbmdetailsid).ToList().Take(30).ToList();
            //var data1 = livedb.tbl_livecbmparameters.Where(m => m.SensorGroupID == sensormasterrow.Sid && m.MemoryAddress == memadd && m.CorrectedDate == correctedDate).OrderByDescending(m => m.cbmpID).ToList().Take(30).ToList();
            foreach (var row in data1)
            {
                Health objvalue = new Health();
                objvalue.sensorvalueid = row.sensorvalueid;
                objvalue.value = Convert.ToInt32(row.sensorValues);
                objvalue.Time = row.CreatedOn?.ToString(@"hh\:mm\:ss");
                cbmList.Add(objvalue);
            }

            objmahchinehealth.MachineHealthdet = cbmList.OrderBy(m => m.Time).ToList();

            res = JsonConvert.SerializeObject(objmahchinehealth);

            return res;
        }

        private string GetCorrectedDate()
        {
            DateTime correctedDate = DateTime.Now;
            var daytimings = db.tbldaytimings.Where(m => m.IsDeleted == 0).FirstOrDefault();
            if (daytimings != null)
            {
                DateTime Start = Convert.ToDateTime(correctedDate.ToString("yyyy-MM-dd") + " " + daytimings.StartTime);
                if (Start <= DateTime.Now)
                {
                    correctedDate = DateTime.Now.Date;
                }
                else
                {
                    correctedDate = DateTime.Now.AddDays(-1).Date;
                }
            }
            string correctedDateformat = correctedDate.ToString("yyyy-MM-dd");
            return correctedDateformat;
        }
    }
}