
using Newtonsoft.Json;
using SRKSDemo.Models;
using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SRKSDemo.Controllers
{
    public class PMSEntryScreenController : Controller
    {
         i_facility_shaktiEntities1 condb = new i_facility_shaktiEntities1();
        // GET: PMSEntryScreen
        public ActionResult Index()
        {
            ViewData["plant"] = new SelectList(condb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["shop"] = new SelectList(condb.tblshops.Where(m => m.IsDeleted == 0), "ShopID", "ShopName");
            ViewData["Cell"] = new SelectList(condb.tblcells.Where(m => m.IsDeleted == 0), "CellID", "CellName");
            ViewData["Machine"] = new SelectList(condb.tblmachinedetails.Where(m => m.IsDeleted == 0), "MachineID", "MachineName");
            return View();
        }

        public ActionResult Index1()
        {
            try
            {
                ViewBag.Logout = Session["Username"].ToString().ToUpper();
                ViewBag.roleid = Session["RoleID"];
            }
            catch
            {
                return Redirect("/Login/Login");
            }

            TempData["Error"] = null;
            var PMSdetails = condb.tblpmsdetails.Where(m => m.IsDeleted == 0).ToList();
            return View(PMSdetails);
        }

        public string Getdata(int cellid, int machineid)
        {
            List<PMSEntry> pmslist = new List<PMSEntry>();
            string res = "";
            var pmsdet = condb.tblpmsdetails.Where(m => m.IsDeleted == 0 && m.IsCompleted == 0 && m.MachineID == machineid).ToList();
            int pmscount = pmsdet.Count;
            if (pmscount == 0)
            {
                var checkpointDetails = condb.configuration_tblpmcheckpoint.Where(m => m.Isdeleted == 0 && m.CellID == cellid).ToList();
                foreach (var checkpointdet in checkpointDetails)
                {
                    PMSEntry obj = new PMSEntry();
                    obj.checkpointid = checkpointdet.pmcpID;
                    obj.checkpointname = checkpointdet.TypeofCheckpoint;
                    List<pmchecklistdata> pmclist = new List<pmchecklistdata>();
                    var checklistdet = condb.configuration_tblpmchecklist.Where(m => m.Isdeleted == 0 && m.pmcpID == checkpointdet.pmcpID).ToList();
                    foreach (var checklistdet2 in checklistdet)
                    {
                        pmchecklistdata obj1 = new pmchecklistdata();
                        obj1.checklistname = checklistdet2.CheckList;
                        obj1.Frequency = checklistdet2.Frequency;
                        obj1.How = checklistdet2.How;
                        obj1.Value = checklistdet2.Value;
                        pmclist.Add(obj1);
                    }
                    obj.checklistdata = pmclist;
                    pmslist.Add(obj);
                }
                res = JsonConvert.SerializeObject(pmslist);
            }
            else if (pmscount > 0)
            {
                var checkpointDetails = condb.configuration_tblpmcheckpoint.Where(m => m.Isdeleted == 0 && m.CellID == cellid).ToList();
                foreach (var checkpointdet in checkpointDetails)
                {
                    PMSEntry obj = new PMSEntry();
                    obj.checkpointid = checkpointdet.pmcpID;
                    obj.checkpointname = checkpointdet.TypeofCheckpoint;
                    List<pmchecklistdata> pmclist = new List<pmchecklistdata>();
                    var checklistdet = condb.configuration_tblpmchecklist.Where(m => m.Isdeleted == 0 && m.pmcpID == checkpointdet.pmcpID).ToList();
                    foreach (var checklistdet2 in checklistdet)
                    {
                        pmchecklistdata obj1 = new pmchecklistdata();
                        obj1.checklistname = checklistdet2.CheckList;
                        obj1.Frequency = checklistdet2.Frequency;
                        obj1.How = checklistdet2.How;
                        obj1.Value = checklistdet2.Value;
                        pmclist.Add(obj1);
                    }
                    obj.checklistdata = pmclist;
                    pmslist.Add(obj);
                }
                res = JsonConvert.SerializeObject(pmslist);
            }


            else
            {
                var pmsdetails = condb.tblpmsdetails.Where(m => m.IsCompleted == 1).ToList();
                foreach (var item in pmsdetails)
                {
                    var pmsdata = condb.tblhistpms.Where(m => m.pmsid != item.pmsid).Select(m => m.Pmcheckpointid).Distinct().ToList();
                    foreach (var pms in pmsdata)
                    {
                        var checkpointDetails1 = condb.configuration_tblpmcheckpoint.Where(m => m.Isdeleted == 0 && m.pmcpID == pms).FirstOrDefault();
                        PMSEntry obj = new PMSEntry();
                        obj.checkpointid = checkpointDetails1.pmcpID;
                        obj.checkpointname = checkpointDetails1.TypeofCheckpoint;
                        List<pmchecklistdata> pmclist = new List<pmchecklistdata>();
                        var checklistdet1 = condb.configuration_tblpmchecklist.Where(m => m.Isdeleted == 0 && m.pmcpID == checkpointDetails1.pmcpID).ToList();
                        foreach (var checklistdet2 in checklistdet1)
                        {
                            pmchecklistdata obj1 = new pmchecklistdata();
                            obj1.checklistname = checklistdet2.CheckList;
                            obj1.Frequency = checklistdet2.Frequency;
                            obj1.How = checklistdet2.How;
                            obj1.Value = checklistdet2.Value;
                            pmclist.Add(obj1);
                        }
                        obj.checklistdata = pmclist;
                        pmslist.Add(obj);
                        res = JsonConvert.SerializeObject(pmslist);
                    }
                }
            }
            return res;
        }
        //public string Getdata(int cellid, int machineid)
        //{
        //    List<PMSEntry> pmslist = new List<PMSEntry>();
        //    string res = "";
        //    var checkpointdata = condb.configuration_tblpmcheckpoint.Where(m => m.Isdeleted == 0 && m.CellID == cellid).ToList();
        //    var pmsdet = condb.tblpmsdetails.Where(m => m.IsDeleted == 0 && m.IsCompleted == 0 && m.MachineID == machineid).ToList();
        //    int count = pmsdet.Count;
        //    if (count == 0)
        //    {

        //        foreach (var checkpointdet in checkpointdata)
        //        {
        //            PMSEntry obj = new PMSEntry();
        //            obj.checkpointid = checkpointdet.pmcpID;
        //            obj.checkpointname = checkpointdet.TypeofCheckpoint;
        //            var checklistdata = condb.configuration_tblpmchecklist.Where(m => m.Isdeleted == 0 && m.pmcpID == checkpointdet.pmcpID).ToList();
        //            List<pmchecklistdata> pmclist = new List<pmchecklistdata>();
        //            foreach (var checklistdet in checklistdata)
        //            {
        //                pmchecklistdata obj1 = new pmchecklistdata();
        //                obj1.checklistname = checklistdet.CheckList;
        //                obj1.Frequency = checklistdet.Frequency;
        //                obj1.Value = checklistdet.Value;
        //                obj1.How = checklistdet.How;
        //                pmclist.Add(obj1);
        //            }
        //            obj.checklistdata = pmclist;
        //            pmslist.Add(obj);
        //        }
        //        res = JsonConvert.SerializeObject(pmslist);
        //    }
        //    else
        //    {
        //        foreach (var checkpointdet in checkpointdata)
        //        {
        //            var histpms = condb.tblhistpms.Where(m => m.Pmcheckpointid == checkpointdet.pmcpID).ToList();
        //            foreach (var histpmsdet in histpms)
        //            {
        //                if (checkpointdet.pmcpID == histpmsdet.Pmcheckpointid)
        //                {
        //                    PMSEntry obj = new PMSEntry();
        //                    obj.checkpointid = histpmsdet.Pmcheckpointid;
        //                    var checkpointname = condb.configuration_tblpmcheckpoint.Where(m => m.pmcpID == histpmsdet.Pmcheckpointid).Select(m => m.TypeofCheckpoint).FirstOrDefault();
        //                    obj.checkpointname = checkpointname;
        //                    var checklistdata = condb.configuration_tblpmchecklist.Where(m => m.Isdeleted == 0 && m.pmcpID == histpmsdet.Pmcheckpointid).ToList();
        //                    List<pmchecklistdata> pmclist = new List<pmchecklistdata>();
        //                    foreach (var checklistdet in checklistdata)
        //                    {
        //                        pmchecklistdata obj1 = new pmchecklistdata();
        //                        obj1.checklistname = checklistdet.CheckList;
        //                        obj1.Frequency = checklistdet.Frequency;
        //                        obj1.Value = checklistdet.Value;
        //                        obj1.How = checklistdet.How;
        //                        pmclist.Add(obj1);
        //                    }
        //                    obj.checklistdata = pmclist;
        //                    pmslist.Add(obj);
        //                    res = JsonConvert.SerializeObject(pmslist);
        //                }
        //                else
        //                {
        //                    PMSEntry obj = new PMSEntry();
        //                    obj.checkpointid = checkpointdet.pmcpID;
        //                    obj.checkpointname = checkpointdet.TypeofCheckpoint;
        //                    var checklistdata = condb.configuration_tblpmchecklist.Where(m => m.Isdeleted == 0 && m.pmcpID == histpmsdet.Pmcheckpointid).ToList();
        //                    List<pmchecklistdata> pmclist = new List<pmchecklistdata>();
        //                    foreach (var checklistdet in checklistdata)
        //                    {
        //                        pmchecklistdata obj1 = new pmchecklistdata();
        //                        obj1.checklistname = checklistdet.CheckList;
        //                        obj1.Frequency = checklistdet.Frequency;
        //                        obj1.Value = checklistdet.Value;
        //                        obj1.How = checklistdet.How;
        //                        pmclist.Add(obj1);
        //                    }
        //                    obj.checklistdata = pmclist;
        //                    pmslist.Add(obj);
        //                    res = JsonConvert.SerializeObject(pmslist);
        //                }
        //            }
        //        }
        //    }
        //    return res;
        //}
        public string insertdata( int machineid, string Startdate)
        {
            string res = "";
            var pmsdet = condb.tblpmsdetails.Where(m => m.IsDeleted == 0  && m.IsCompleted == 0 && m.MachineID == machineid && m.PMStartDate == Startdate).ToList();
            int count = pmsdet.Count;
            if (count == 0)
            {
                tblpmsdetail obj1 = new tblpmsdetail();
                obj1.MachineID = machineid;
                //obj1.CellID = cellid;
                obj1.PMEndDate = null;
                obj1.PMStartDate = Startdate;
                obj1.IsCompleted = 0;
                obj1.IsSubmitted = 1;
                obj1.CreatedOn = System.DateTime.Now;
                obj1.CreatedBy = 1;
                obj1.IsDeleted = 0;
                condb.tblpmsdetails.Add(obj1);
                condb.SaveChanges();
                res = JsonConvert.SerializeObject(obj1.pmsid);
            }
            else { }
            return res;
        }
        public string inserthistdb(string remarks, int machineid, int work, int pmcpid, string Startdate, string j, int cellid)
        {
            string res = "";
            var pmsdet = condb.tblpmsdetails.Where(m => m.IsDeleted == 0 && m.IsCompleted == 0 && m.MachineID == machineid).Select(m => m.pmsid).FirstOrDefault();
            tblhistpm cp = new tblhistpm();
            cp.Machineid = machineid;
            cp.Pmcheckpointid = pmcpid;
            cp.Cellid = cellid;
            cp.CorrectedDate = Startdate;
            cp.Pmchecklistname = j;
            cp.Remarks = remarks;
            cp.workdone = work;
            cp.pmsid = pmsdet;
            condb.tblhistpms.Add(cp);
            condb.SaveChanges();
            res = "Success";
            return res;
        }
        public JsonResult FetchMachine(int CID)
        {
            using (i_facility_shaktiEntities1 condb = new i_facility_shaktiEntities1())
            {
                var MachineData = (from row in condb.tblmachinedetails
                                   where row.IsDeleted == 0 && row.CellID == CID
                                   select new { Value = row.MachineID, cell = row.CellID, Text = row.MachineDisplayName }).ToList();
                return Json(MachineData, JsonRequestBehavior.AllowGet);
            }
        }
        public string Getprev(int pmcpid)
        {
            string res = "";
            List<pmshistdata> obj = new List<pmshistdata>();
            var pmsdata = condb.tblhistpms.Where(m => m.Pmcheckpointid == pmcpid).Select(m => m.pmsid).Distinct().FirstOrDefault();
            if (pmsdata != 0)
            {
                var pmsdet = condb.tblpmsdetails.Where(m => m.pmsid == pmsdata).FirstOrDefault();
                if (pmsdet.IsCompleted == 1)
                {
                    res = "failure";
                    //var pmsdata1 = condb.tblhistpms.Where(m => m.pmsid == pmsdet.pmsid).Select(m => m.Pmcheckpointid).Distinct().ToList();
                    //foreach (var pms in pmsdata1)
                    //{
                    //    if (pms == pmcpid)
                    //    {
                    //        pmshistdata obj1 = new pmshistdata();
                    //        obj1.work = -1;
                    //        obj1.remarks = null;
                    //        obj.Add(obj1);
                    //    }
                    //}
                    //res = JsonConvert.SerializeObject(obj);
                }
                else
                {
                    var checklistdata = condb.configuration_tblpmchecklist.Where(m => m.pmcpID == pmcpid && m.Isdeleted == 0).Select(m => m.CheckList).ToList();
                    foreach (var items in checklistdata)
                    {
                        pmshistdata obj1 = new pmshistdata();
                        var Data = condb.tblhistpms.Where(m => m.Pmchecklistname == items && m.Pmcheckpointid == pmcpid).ToList();
                        foreach (var histpms in Data)
                        {
                            obj1.work = Convert.ToInt32(histpms.workdone);
                            obj1.remarks = histpms.Remarks;
                            obj.Add(obj1);
                        }
                        res = JsonConvert.SerializeObject(obj);
                    }

                }
            }
            else { }
            return res;
        }
        public string Setfalg(string Enddate)
        {
            string res = "";
            var pmsdetails = condb.tblpmsdetails.Where(m => m.IsDeleted == 0 && m.IsSubmitted == 1).ToList();
            foreach (var pmsdet1 in pmsdetails)
            {
                pmsdet1.IsCompleted = 1;
                pmsdet1.PMEndDate = Enddate;
                condb.Entry(pmsdet1).State = EntityState.Modified;
                condb.SaveChanges();
            }
            res = "Success";
            return res;
        }

        public JsonResult GetShop(int PlantID)
        {
            var ShopData = new SelectList(condb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID), "ShopID", "ShopName");
            return Json(ShopData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetCell(int ShopID)
        {
            var CellData = new SelectList(condb.tblcells.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID), "CellID", "CellName");
            return Json(CellData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetWC_Cell(int CellID)
        {
            var MachineData = new SelectList(condb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == CellID && m.IsNormalWC == 0), "MachineID", "MachineDisplayName");
            return Json(MachineData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetWC_Shop(int ShopID)
        {
            var MachineData = new SelectList(condb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID && m.CellID.Equals(null) && m.IsNormalWC == 0), "MachineID", "MachineInvNo");
            return Json(MachineData, JsonRequestBehavior.AllowGet);
        }
    }
}