using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SRKSDemo.Models;
using System.Data.Entity;
using System.Data;
using Newtonsoft.Json;
using SRKSDemo.Server_Model;

namespace SRKSDemo.Controllers
{
    public class PreventiveMaintainanceSchedulingController : Controller
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
            PreventiveMaintainanceScheduling pm = new PreventiveMaintainanceScheduling();

            pm.primitivemaintainancescheduling = condb.configuration_tblprimitivemaintainancescheduling.Where(m => m.IsDeleted == 0 && m.Month != null && m.Week != null).ToList();

            return View(pm);
        }

        // Get Plant Details
        public string GetPlant()
        {
            string res = "";

            var plant = condb.tblplants.Where(p => p.IsDeleted == 0).Select(m => new { m.PlantID, m.PlantName }).ToList();
            res = JsonConvert.SerializeObject(plant);
            return res;
        }

        // Get Shop Details
        public string GetShop(int PlantID)
        {
            string res = "";

            var shop = condb.tblshops.Where(p => p.IsDeleted == 0 && p.PlantID == PlantID).Select(m => new { m.ShopID, m.ShopName }).ToList();
            res = JsonConvert.SerializeObject(shop);
            return res;
        }

        //Get Cell Details
        public string GetCell(int ShopID)
        {
            string res = "";

            var cell = condb.tblcells.Where(p => p.IsDeleted == 0 && p.ShopID == ShopID).Select(m => new { m.CellID, m.CellName }).ToList();
            res = JsonConvert.SerializeObject(cell);
            return res;
        }

        public ActionResult Create()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.display = 0;
            return View();
        }

        // Get Month Details
        public string GetMonth()
        {
            string res = "";

            var Month = condb.monthdatas.Where(p => p.Isdeleted == 0).Select(m => new { m.MonthID, m.Text }).ToList();
            res = JsonConvert.SerializeObject(Month);
            return res;
        }
        //Get Week Details
        public string GetWeek()
        {
            string res = "";

            var Week = condb.weekdatas.Where(p => p.Isdeleted == 0).Select(m => new { m.WeekID, m.value }).ToList();
            res = JsonConvert.SerializeObject(Week);
            return res;
        }

        // Get PMS Details based on CELL
        public string GetDetails(int CellID)
        {
            string res = "";
            List<PMSData> pm = new List<PMSData>();
            var MachineDetails = condb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == CellID).ToList();
            int count = MachineDetails.Count();
            int i = 1;
            foreach (var item in MachineDetails)
            {

                int macId = Convert.ToInt32(item.MachineID);
                var macName = item.MachineDisplayName;
                List<PMSData> pmsList = new List<PMSData>();
                var doesmachineIDpresent = condb.configuration_tblprimitivemaintainancescheduling.Where(m => m.IsDeleted == 0 && m.MachineID == macId).ToList();
                if (doesmachineIDpresent.Count == 0)
                {
                    configuration_tblprimitivemaintainancescheduling cp = new configuration_tblprimitivemaintainancescheduling();
                    var CellDet = condb.tblcells.Where(m => m.CellID == CellID).FirstOrDefault();

                    cp.CellID = CellDet.CellID;
                    cp.plantName = CellDet.tblplant.PlantName;
                    cp.shopname = CellDet.tblshop.ShopName;
                    cp.MachineID = macId;
                    cp.PlantID = CellDet.tblplant.PlantID;
                    cp.ShopID = CellDet.tblshop.ShopID;
                    cp.MachineName = macName;
                    cp.CellName = CellDet.CellName;
                    cp.CreatedBy = 1;
                    cp.CreatedOn = DateTime.Now;
                    cp.IsDeleted = 0;
                    condb.configuration_tblprimitivemaintainancescheduling.Add(cp);
                    condb.SaveChanges();
                    res = "Saved";
                }
            }
            MachineDetails = condb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == CellID).ToList();
            count = MachineDetails.Count();
            ViewBag.item = count;
            i = 1;

            foreach (var item in MachineDetails)
            {
                PMSData obj = new PMSData();
                obj.MachineName = item.MachineDisplayName;
                obj.MachineID = item.MachineID;

                var data = condb.configuration_tblprimitivemaintainancescheduling.Where(m => m.MachineID == item.MachineID && m.IsDeleted == 0).ToList();

                List<PmsDetails> objpmsdet = new List<PmsDetails>();
                foreach (var row in data)
                {
                    PmsDetails pmsdet = new PmsDetails();
                    pmsdet.MachineID = row.MachineID;
                    pmsdet.pmid = row.pmid;
                    pmsdet.MachineName = row.MachineName;
                    pmsdet.month = Convert.ToString(row.Month);
                    pmsdet.monthID = Convert.ToInt32(row.MonthID);
                    //int month = Convert.ToInt32(row.Month);
                    //var monthName = condb.monthdatas.Where(m => m.Value == month).Select(m => m.Text).FirstOrDefault();
                    //pmsdet.month = Convert.ToInt32(monthName);
                    pmsdet.week = Convert.ToInt32(row.Week);
                    objpmsdet.Add(pmsdet);
                }
                obj.pmsdetailsList = objpmsdet;
                pm.Add(obj);
            }
            res = JsonConvert.SerializeObject(pm);
            return res;
        }



        //public ActionResult Create(int cell = 0, int id = 0)

        //{
        //    i_facilityEntities db = new i_facilityEntities();
        //    if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
        //    {
        //        return RedirectToAction("Login", "Login", null);
        //    }
        //    ViewBag.plant = new SelectList(condb.configuration_configuration_tblplant.Where(p => p.IsDeleted == 0), "PlantID", "PlantName").ToList();

        //    ViewBag.shop = new SelectList(condb.configuration_configuration_tblshop.Where(d => d.IsDeleted == 0), "ShopId", "ShopName").ToList();
        //    ViewBag.cell = new SelectList(condb.configurationtblcells.Where(p => p.IsDeleted == 0), "cellID", "cellName").ToList();
        //    ViewBag.Logout = Session["Username"].ToString().ToUpper();
        //    ViewBag.roleid = Session["RoleID"];
        //    String Username = Session["Username"].ToString();
        //    ViewBag.display = 0;
        //    var year = DateTime.Now.Year;
        //    ViewBag.Month = new SelectList(condb.configurationmonthdatas.Where(p => p.Isdeleted == 0), "MonthID", "Text").ToList();
        //    ViewBag.Week = new SelectList(condb.configuration_weekdata.Where(p => p.Isdeleted == 0), "WeekID", "Value").ToList();
        //    ViewBag.Year = year;
        //    List<PMSData> pm = new List<PMSData>();
        //    if (cell != 0)
        //    {

        //        using (i_facilityEntities db = new i_facilityEntities())
        //        {
        //            int plantid = Convert.ToInt32(condb.configurationtblcells.Where(m => m.CellID == cell).Select(m => m.PlantID).FirstOrDefault());
        //            string plantname = Convert.ToString(condb.configuration_configuration_tblplant.Where(m => m.PlantID == plantid).Select(m => m.PlantName).FirstOrDefault());
        //            int shopid = Convert.ToInt32(condb.configurationtblcells.Where(m => m.CellID == cell).Select(m => m.ShopID).FirstOrDefault());
        //            string shopname = Convert.ToString(condb.configuration_configuration_tblshop.Where(m => m.ShopID == shopid).Select(m => m.ShopName).FirstOrDefault());
        //            var cellname = (from s in condb.configurationtblcells where s.CellID == cell select s.CellName).FirstOrDefault();

        //            int machineid = (from s in condb.configurationtblmachinedetails where s.IsDeleted == 0 && s.CellID == cell select s.MachineID).FirstOrDefault();
        //            if (machineid == 0)
        //            {
        //                TempData["Message"] = "The selected Cell does not have any machines for this plant and shop,please select another cell";
        //                return View();
        //            }
        //            else
        //            {
        //                var machinename = (from s in condb.configurationtblmachinedetails where s.MachineID == machineid select s.MachineName).FirstOrDefault();
        //                var pmsmachinedetails = (from s in condb.configuration_tblprimitivemaintainancescheduling where s.IsDeleted == 0 && s.CellID == cell select s.MachineID).ToList();
        //                var MachineDetails = condb.configurationtblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == cell).ToList();
        //                int count = MachineDetails.Count();
        //                int i = 1;
        //                foreach (var item in MachineDetails)
        //                {

        //                    int macId = Convert.ToInt32(item.MachineID);
        //                    var macName = item.MachineName;
        //                    List<PMSData> pmsList = new List<PMSData>();
        //                    var doesmachineIDpresent = condb.configuration_tblprimitivemaintainancescheduling.Where(m => m.IsDeleted == 0 && m.MachineID == macId).ToList();
        //                    if (doesmachineIDpresent.Count == 0)
        //                    {
        //                        tblprimitivemaintainancescheduling cp = new tblprimitivemaintainancescheduling();

        //                        cp.CellID = cell;
        //                        cp.plantName = plantname;
        //                        cp.shopname = shopname;
        //                        cp.MachineID = macId;
        //                        cp.PlantID = plantid;
        //                        cp.ShopID = shopid;
        //                        cp.MachineName = macName;
        //                        cp.CellName = cellname;
        //                        cp.CreatedBy = 1;
        //                        cp.CreatedOn = DateTime.Now;
        //                        cp.IsDeleted = 0;
        //                        condb.configuration_tblprimitivemaintainancescheduling.Add(cp);
        //                        condb.SaveChanges();
        //                        if (i == count)
        //                        {
        //                            return View("Index");
        //                        }
        //                        i++;
        //                    }
        //                }

        //                MachineDetails = condb.configurationtblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == cell).ToList();
        //                count = MachineDetails.Count();
        //                ViewBag.item = count;
        //                i = 1;

        //                foreach (var item in MachineDetails)
        //                {

        //                    PMSData obj = new PMSData();
        //                    obj.MachineName = item.MachineName;
        //                    obj.MachineID = item.MachineID;

        //                    var data = condb.configuration_tblprimitivemaintainancescheduling.Where(m => m.MachineID == item.MachineID && m.IsDeleted == 0).ToList();

        //                    List<PmsDetails> objpmsdet = new List<PmsDetails>();
        //                    foreach (var row in data)
        //                    {
        //                        PmsDetails pmsdet = new PmsDetails();
        //                        pmsdet.MachineID = row.MachineID;
        //                        pmsdet.pmid = row.pmid;
        //                        pmsdet.MachineName = row.MachineName;
        //                        pmsdet.month = Convert.ToInt32(row.Month);
        //                        pmsdet.week = Convert.ToInt32(row.Week);
        //                        pmsdet.selectMonth = new SelectList(condb.configurationmonthdatas.Where(p => p.Isdeleted == 0), "MonthID", "Text", row.MachineID);
        //                        pmsdet.selectweek = new SelectList(condb.configuration_weekdata.Where(p => p.Isdeleted == 0), "WeekID", "Value");
        //                        objpmsdet.Add(pmsdet);
        //                    }
        //                    obj.pmsdetailsList = objpmsdet;
        //                    pm.Add(obj);
        //                }
        //                var details = (from s in condb.configuration_tblprimitivemaintainancescheduling where s.CellID == cell select s.plantName).FirstOrDefault();
        //                var dat = (from s in condb.configuration_tblprimitivemaintainancescheduling where s.CellID == cell select s.CellName).FirstOrDefault();
        //                var dat1 = (from s in condb.configuration_tblprimitivemaintainancescheduling where s.CellID == cell select s.shopname).FirstOrDefault();
        //                ViewBag.item = dat.ToString();
        //                ViewBag.item1 = dat1.ToString();
        //                ViewBag.item2 = details.ToString();
        //                ViewBag.display = 1;
        //            }
        //        }
        //        return View(pm);
        //    }
        //}

        //Inserting the Data into database when user click on + button
        public string InsertData(int id, int monthValue, int weekvalue)
        {
            string res = "";

            //var prevenid = condb.configuration_tblprimitivemaintainancescheduling.Where(m => m.IsDeleted == 0).Select(m => m.MachineID).ToList();
            //foreach (var items in prevenid)
            //{
            //    var mac1id = condb.configuration_tblprimitivemaintainancescheduling.Where(m => m.MachineID == items && m.IsDeleted == 0).ToList();

            //    foreach (var item in mac1id)
            //    {
            //        if (item.MachineID == id)
            //        {
            //            //if (mac1id.Count > 1)
            //            //{

            //            var tblpm = condb.configuration_tblprimitivemaintainancescheduling.Where(m => m.MachineID == id && m.IsDeleted == 0).FirstOrDefault();
            //            tblprimitivemaintainancescheduling cp1 = new tblprimitivemaintainancescheduling();
            //            cp1.CellID = tblpm.CellID;
            //            cp1.plantName = tblpm.plantName;
            //            cp1.shopname = tblpm.shopname;
            //            cp1.MachineID = tblpm.MachineID;
            //            cp1.PlantID = tblpm.PlantID;
            //            cp1.ShopID = tblpm.ShopID;
            //            cp1.MachineName = tblpm.MachineName;
            //            cp1.CellName = tblpm.CellName;
            //            cp1.CreatedBy = 1;
            //            cp1.CreatedOn = DateTime.Now;
            //            cp1.IsDeleted = 0;
            //            cp1.Month = monthValue;
            //            cp1.Week = weekvalue;
            //            condb.configuration_tblprimitivemaintainancescheduling.Add(cp1);
            //            condb.SaveChanges();
            //            break;
            //            res = "Success";
            //            return res;

            //        }
            //        else //if (item.pmid == id)
            //        {

            //        }
            //    }
            //}
            var macid = condb.configuration_tblprimitivemaintainancescheduling.Where(m => m.pmid == id && m.IsDeleted == 0).FirstOrDefault();
            var tblpm1 = condb.configuration_tblprimitivemaintainancescheduling.Where(m => m.MachineID == macid.MachineID && m.IsDeleted == 0).FirstOrDefault();
            var monthdata = condb.monthdatas.Where(m => m.MonthID == monthValue).Select(m => m.Text).FirstOrDefault();
            macid.Month = Convert.ToString(monthdata);
            string machid = Convert.ToString(macid.Month);
            var monthId = condb.monthdatas.Where(m => m.Text == machid).Select(m => m.MonthID).FirstOrDefault();
            macid.MonthID = monthId;
            macid.Week = weekvalue;
            condb.Entry(macid).State = EntityState.Modified;
            condb.SaveChanges();
            configuration_tblprimitivemaintainancescheduling cp = new configuration_tblprimitivemaintainancescheduling();
            cp.CellID = tblpm1.CellID;
            cp.plantName = tblpm1.plantName;
            cp.shopname = tblpm1.shopname;
            cp.MachineID = tblpm1.MachineID;
            cp.PlantID = tblpm1.PlantID;
            cp.ShopID = tblpm1.ShopID;
            cp.MachineName = tblpm1.MachineName;
            cp.CellName = tblpm1.CellName;
            cp.CreatedBy = 1;
            cp.CreatedOn = DateTime.Now;
            cp.IsDeleted = 0;
            cp.Month = null;
            cp.MonthID = 0;
            cp.Week = null;
            condb.configuration_tblprimitivemaintainancescheduling.Add(cp);
            condb.SaveChanges();
            TempData["toaster_success"] = "Data Saved successfully";
            res = "Success";
            return res;
        }

        public string InsertData1(/*int pmid,*/int macid, int monthValue, int weekvalue)
        {
            string res = "";
            //var macid1 = condb.configuration_tblprimitivemaintainancescheduling.Where(m => m.MachineID == macid && m.IsDeleted == 0 && m.pmid == pmid).FirstOrDefault();
            var tblpm1 = condb.configuration_tblprimitivemaintainancescheduling.Where(m => m.MachineID == macid && m.IsDeleted == 0).FirstOrDefault();
            //macid1.Month = monthValue;
            //macid1.Week = weekvalue;
            //db.Entry(macid1).State = EntityState.Modified;
            //db.SaveChanges();
            configuration_tblprimitivemaintainancescheduling cp = new configuration_tblprimitivemaintainancescheduling();
            cp.CellID = tblpm1.CellID;
            cp.plantName = tblpm1.plantName;
            cp.shopname = tblpm1.shopname;
            cp.MachineID = tblpm1.MachineID;
            cp.PlantID = tblpm1.PlantID;
            cp.ShopID = tblpm1.ShopID;
            cp.MachineName = tblpm1.MachineName;
            cp.CellName = tblpm1.CellName;
            cp.CreatedBy = 1;
            cp.CreatedOn = DateTime.Now;
            cp.IsDeleted = 0;
            var monthdata = condb.monthdatas.Where(m => m.MonthID == monthValue).Select(m => m.Text).FirstOrDefault();
            cp.Month = Convert.ToString(monthdata);

            cp.MonthID = monthValue;
            cp.Week = weekvalue;
            condb.configuration_tblprimitivemaintainancescheduling.Add(cp);
            condb.SaveChanges();
            TempData["toaster_success"] = "Data Saved successfully";
            res = "Success";
            return res;
        }

        //Saving all the Data into database without click on + button
        public string Save(int id, int mon, int wee)
        {
            string res = "";
            var macid = condb.tblmachinedetails.Where(m => m.IsDeleted == 0).Select(m => m.MachineID).ToList();
            foreach (var items in macid)
            {
                var mac1id = condb.configuration_tblprimitivemaintainancescheduling.Where(m => m.MachineID == items && m.IsDeleted == 0).ToList();

                foreach (var item in mac1id)
                {
                    if (item.MachineID == id)
                    {
                        var tblpm = condb.configuration_tblprimitivemaintainancescheduling.Where(m => m.MachineID == id && m.IsDeleted == 0).FirstOrDefault();
                        configuration_tblprimitivemaintainancescheduling cp = new configuration_tblprimitivemaintainancescheduling();
                        cp.CellID = tblpm.CellID;
                        cp.plantName = tblpm.plantName;
                        cp.shopname = tblpm.shopname;
                        cp.MachineID = tblpm.MachineID;
                        cp.PlantID = tblpm.PlantID;
                        cp.ShopID = tblpm.ShopID;
                        cp.MachineName = tblpm.MachineName;
                        cp.CellName = tblpm.CellName;
                        cp.CreatedBy = 1;
                        cp.CreatedOn = DateTime.Now;
                        cp.IsDeleted = 0;
                        var monthdata = condb.monthdatas.Where(m => m.MonthID == mon).Select(m => m.Text).FirstOrDefault();
                        cp.Month = Convert.ToString(monthdata);
                        cp.MonthID = mon;
                        cp.Week = wee;
                        condb.configuration_tblprimitivemaintainancescheduling.Add(cp);
                        condb.SaveChanges();
                        res = "Success";
                        return res;
                    }
                    else if (item.pmid == id)
                    {
                        var pmid = condb.configuration_tblprimitivemaintainancescheduling.Where(m => m.pmid == id && m.IsDeleted == 0).FirstOrDefault();
                        var tblpm = condb.configuration_tblprimitivemaintainancescheduling.Where(m => m.MachineID == pmid.MachineID && m.IsDeleted == 0).FirstOrDefault();
                        var monthdata = condb.monthdatas.Where(m => m.MonthID == mon).Select(m => m.Text).FirstOrDefault();
                        pmid.Month = Convert.ToString(monthdata);
                        pmid.MonthID = mon;
                        pmid.Week = wee;
                        condb.Entry(pmid).State = EntityState.Modified;
                        condb.SaveChanges();
                        configuration_tblprimitivemaintainancescheduling cp = new configuration_tblprimitivemaintainancescheduling();
                        cp.CellID = tblpm.CellID;
                        cp.plantName = tblpm.plantName;
                        cp.shopname = tblpm.shopname;
                        cp.MachineID = tblpm.MachineID;
                        cp.PlantID = tblpm.PlantID;
                        cp.ShopID = tblpm.ShopID;
                        cp.MachineName = tblpm.MachineName;
                        cp.CellName = tblpm.CellName;
                        cp.CreatedBy = 1;
                        cp.CreatedOn = DateTime.Now;
                        cp.IsDeleted = 0;
                        cp.Month = null;
                        cp.MonthID = 0;
                        cp.Week = null;
                        condb.configuration_tblprimitivemaintainancescheduling.Add(cp);
                        condb.SaveChanges();
                        res = "Success";
                    }
                }
            }
            return res;
        }
        //Deleting the record
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
                configuration_tblprimitivemaintainancescheduling tblpm = condb.configuration_tblprimitivemaintainancescheduling.Find(id);
                tblpm.IsDeleted = 1;
                tblpm.ModifiedBy = UserID;
                tblpm.ModifiedOn = DateTime.Now;
                condb.Entry(tblpm).State = EntityState.Modified;
                condb.SaveChanges();
                TempData["toaster_success"] = "Data Deleted successfully";
                return RedirectToAction("Index");
            }
        }

        //Deleting the record when user click on - button
        public string DeleteData(int Id)
        {
            string res = "";
            var tblpm = condb.configuration_tblprimitivemaintainancescheduling.Where(m => m.pmid == Id && m.IsDeleted == 0).FirstOrDefault();
            tblpm.IsDeleted = 1;
            tblpm.ModifiedBy = 1;
            tblpm.ModifiedOn = DateTime.Now;
            condb.Entry(tblpm).State = EntityState.Modified;
            condb.SaveChanges();
            res = "Success";
            return res;
        }

        //Updating the Month value
        public string UpdateData(int pmid, int monthvalue)
        {
            string res = "";
            var tblpm = condb.configuration_tblprimitivemaintainancescheduling.Where(m => m.pmid == pmid && m.IsDeleted == 0).FirstOrDefault();
            var monthdata = condb.monthdatas.Where(m => m.MonthID == monthvalue).Select(m => m.Text).FirstOrDefault();
            tblpm.Month = Convert.ToString(monthdata);
            tblpm.MonthID = monthvalue;
            condb.Entry(tblpm).State = EntityState.Modified;
            condb.SaveChanges();
            res = "Success";
            return res;
        }

        //Updating the Week Value
        public string UpdateData1(int pmid, int weekvalue)
        {
            string res = "";

            var tblpm = condb.configuration_tblprimitivemaintainancescheduling.Where(m => m.pmid == pmid && m.IsDeleted == 0).FirstOrDefault();
            tblpm.Week = weekvalue;
            condb.Entry(tblpm).State = EntityState.Modified;
            condb.SaveChanges();
            res = "Success";
            return res;
        }
        //Checking whether the cell Name is present in Machine Table or not
        [HttpPost]
        public string cellnameDuplicateCheck(int cellid)
        {
            string status = "notok";
            var doesThisExist = condb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == cellid).ToList();
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