using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
//using MySql.Data.MySqlClient;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Xml;
//using OfficeOpenXml;
using System.Data.SqlClient;
using OfficeOpenXml;
using System.Data.Entity.Validation;
using SRKSDemo;
using SRKSDemo.Server_Model;

namespace SRKSDemo.Controllers
{
    public class MultipleWorkOrderController : Controller
    {
        // GET: MultipleWorkOrder
        i_facility_shaktiEntities1 condb = new i_facility_shaktiEntities1();
        public ActionResult Index()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            ViewBag.Plant = new SelectList(condb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewBag.ShopID = new SelectList(condb.tblshops.Where(m => m.IsDeleted == 0), "ShopID", "ShopName");
            ViewBag.CellID = new SelectList(condb.tblcells.Where(m => m.IsDeleted == 0), "CellID", "CellName");
            ViewBag.MachineID = new SelectList(condb.tblmachinedetails.Where(m => m.IsDeleted == 0), "MachineID", "MachineDisplayName");
            MultiWorkOrderModel MW = new MultiWorkOrderModel();
            tblmultipleworkorder mp = new tblmultipleworkorder();
            List<tblmultipleworkorder> mplist = new List<tblmultipleworkorder>();
            MW.Multiworkorder = mp;
            mplist = condb.tblmultipleworkorders.Where(m => m.IsDeleted == 0).ToList();
            MW.MultiOwrkOrderList = mplist;
            return View(MW);
        }

        [HttpPost]
        public ActionResult Create(MultiWorkOrderModel tee, int Plant = 0, int CellID = 0, int ShopID = 0, int MachineID = 0)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            int UserID = Convert.ToInt32(Session["UserId"]);
            //MultiWo validation

            int mwoID = Convert.ToInt32(tee.Multiworkorder.MWOID);
            //string Plantidstring = Convert.ToString(tee.Multiworkorder.PlantID);
            //string Shopidstring = Convert.ToString(tee.Multiworkorder.ShopID);
            //string Cellidstring = Convert.ToString(tee.Multiworkorder.CellID);
            //string WorkCenteridstring = Convert.ToString(tee.Multiworkorder.WCID);
            tee.Multiworkorder.PlantID = Plant;
            tee.Multiworkorder.ShopID = ShopID;
            tee.Multiworkorder.CellID = CellID;
            tee.Multiworkorder.WCID = MachineID;
            tee.Multiworkorder.CreatedBy = UserID;
            tee.Multiworkorder.CreatedOn = DateTime.Now;
            tee.Multiworkorder.IsDeleted = 0;
            tee.Multiworkorder.IsEnabled = 1;
            //tee.MultipleWOName = condb.tblmachinedetails.Where(m => m.MachineID == tee.WCID && m.IsDeleted == 0).Select(m => m.MachineDispName).SingleOrDefault();
            condb.tblmultipleworkorders.Add(tee.Multiworkorder);
            condb.SaveChanges();
            TempData["toaster_success"] = "Data Saved successfully";
            return RedirectToAction("Index");
        }

        public JsonResult FetchShop(int PID)
        {
            using (i_facility_shaktiEntities1 condb = new i_facility_shaktiEntities1())
            {
                var ShopData = (from row in condb.tblshops
                                where row.IsDeleted == 0 && row.PlantID == PID
                                select new { Value = row.ShopID, Text = row.Shopdisplayname }).ToList();
                return Json(ShopData, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult FetchCell(int PID, int SID)
        {
            using (i_facility_shaktiEntities1 condb = new i_facility_shaktiEntities1())
            {
                var CellData = (from row in condb.tblcells
                                where row.IsDeleted == 0 && row.PlantID == PID && row.ShopID == SID
                                select new { Value = row.CellID, Text = row.CelldisplayName }).ToList();
                return Json(CellData, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult FetchMachine(int PID, int SID, int CID)
        {
            using (i_facility_shaktiEntities1 condb = new i_facility_shaktiEntities1())
            {
                var MachineData = (from row in condb.tblmachinedetails
                                   where row.IsDeleted == 0 && row.PlantID == PID && row.ShopID == SID && row.CellID == CID
                                   select new { Value = row.MachineID, Text = row.MachineDisplayName }).ToList();
                return Json(MachineData, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetMultiPleworkOrderById(int Id)
        {
            var Data = condb.tblmultipleworkorders.Where(m => m.MWOID == Id).Select(m => new { Mname = m.MultipleWOName, Mdesc = m.MultipleWODesc, PlantID = m.PlantID, ShopID = m.ShopID, CellID = m.CellID, Macid = m.WCID }).ToList();
            return Json(Data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Edit(MultiWorkOrderModel tee, int Plant = 0, int CellID = 0, int ShopID = 0, int MachineID = 0)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            int UserID = Convert.ToInt32(Session["UserId"]);
            int MWOID = Convert.ToInt32(tee.Multiworkorder.MWOID);
            tee.Multiworkorder.PlantID = Plant;
            tee.Multiworkorder.ShopID = ShopID;
            tee.Multiworkorder.CellID = CellID;
            tee.Multiworkorder.WCID = MachineID;
            tee.Multiworkorder.ModifiedBy = UserID;
            tee.Multiworkorder.ModifiedOn = DateTime.Now;
            tee.Multiworkorder.IsDeleted = 0;
            tee.Multiworkorder.IsEnabled = 1;
            condb.Entry(tee.Multiworkorder).State = EntityState.Modified;
            condb.SaveChanges();
            TempData["toaster_success"] = "Data Updated successfully";
            return RedirectToAction("Index");
        }

        public JsonResult IsItValidMWO(string Plantid, string Shopid, string Cellid, string WorkCenterid, int mwoID = 0)
        {
            List<int> DoesThisPlanOverlapUpwards = new List<int>(), DoesThisPlanOverlapDownwards = new List<int>(), DoesThisPlanOverlapAll = new List<int>();
            string msg = null;

            if (!String.IsNullOrEmpty(Shopid))
            {
                if (!String.IsNullOrEmpty(Cellid))
                {
                    if (!String.IsNullOrEmpty(WorkCenterid))
                    {
                        int wcid = Convert.ToInt32(WorkCenterid);
                        DoesThisPlanOverlapUpwards = MWO_OverlapCheckerForMachine(wcid, mwoID);
                        DoesThisPlanOverlapDownwards = MWO_OverlapCheckerForMachineDownwards(wcid, mwoID);
                    }
                    else
                    {
                        int cellid = Convert.ToInt32(Cellid);
                        DoesThisPlanOverlapUpwards = MWO_OverlapCheckerForCell(cellid, mwoID);
                        DoesThisPlanOverlapDownwards = MWO_OverlapCheckerForCellDownwards(cellid, mwoID);
                    }
                }
                else
                {
                    int shopid = Convert.ToInt32(Shopid);
                    DoesThisPlanOverlapUpwards = MWO_OverlapCheckerForShop(shopid, mwoID);
                    DoesThisPlanOverlapDownwards = MWO_OverlapCheckerForShopDownwards(shopid, mwoID);
                }
            }
            else
            {
                int plantid = Convert.ToInt32(Plantid);
                DoesThisPlanOverlapUpwards = MWO_OverlapCheckerForPlant(plantid, mwoID);
                DoesThisPlanOverlapDownwards = MWO_OverlapCheckerForPlantDownwards(plantid, mwoID);
            }

            ////move all id's into one list and only takes distinct.
            DoesThisPlanOverlapAll.AddRange(DoesThisPlanOverlapUpwards);
            DoesThisPlanOverlapAll.AddRange(DoesThisPlanOverlapDownwards);

            DoesThisPlanOverlapAll = (from n in DoesThisPlanOverlapAll
                                      select n).Distinct().ToList();

            if (DoesThisPlanOverlapAll.Count == 0) //plan doesn't ovelap. So commit.
            {
                msg = "No";
            }
            else
            {
                var results = condb.tblmultipleworkorders.Where(m => m.IsDeleted == 0).Where(x => DoesThisPlanOverlapAll.Contains(x.MWOID));
                string OLPD = string.Empty;

                foreach (var row in results)
                {
                    OLPD += "This Multi Work Order conflicts with " + row.MultipleWOName + "";
                }

                msg = OLPD;
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        public List<int> MWO_OverlapCheckerForPlantDownwards(int plantid, int mwoID = 0)
        {
            List<int> overlappingPlanId = new List<int>();
            int PlantID = plantid;
            DataTable dataHolder = new DataTable();

            //1st check if its shop has a Plan.
            //so get its shopid.
            var shopdetails = condb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID).ToList();
            foreach (var shoprow in shopdetails)
            {
                int shopId = shoprow.ShopID;
                overlappingPlanId = MWO_OverlapCheckerForShopDownwards(shopId, mwoID);
                if (overlappingPlanId.Count > 0)
                {
                    break;
                }
            }

            if (overlappingPlanId.Count == 0)
            {
                MsqlConnection mc = new MsqlConnection();
                mc.open();
                String sql = null;
                //string sString = null;
                //string cString = null;
                //string wcString = null;
                if (mwoID != 0)
                {
                    sql = "SELECT MWOID FROM SRKSDemo.tblmultipleworkorder where PlantID = '" + PlantID + "' and ShopID is null and CellID is null and WCID is null and IsDeleted = 0 and MWOID != " + mwoID + " ;";
                }
                else
                {
                    sql = "SELECT MWOID FROM SRKSDemo.tblmultipleworkorder where PlantID = '" + PlantID + "' and ShopID is null and CellID is null and WCID is null and IsDeleted = 0 ;";
                }
                SqlDataAdapter da = new SqlDataAdapter(sql, mc.msqlConnection);
                da.Fill(dataHolder);
                mc.close();

                for (int i = 0; i < dataHolder.Rows.Count; i++)
                {
                    overlappingPlanId.Add(Convert.ToInt32(dataHolder.Rows[i][0]));
                }
            }
            return overlappingPlanId;
        }
        public List<int> MWO_OverlapCheckerForShopDownwards(int shopid, int mwoID = 0)
        {
            List<int> overlappingPlanId = new List<int>(), overlappingPlanId1 = new List<int>(), overlappingPlanId2 = new List<int>();
            int ShopID = shopid;

            //1st check if its Cells has a Plan.
            //so get its cellid.
            var celldetails = condb.tblcells.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID).ToList();
            foreach (var cellrow in celldetails)
            {
                int cellId = cellrow.CellID;
                overlappingPlanId = MWO_OverlapCheckerForCellDownwards(cellId, mwoID);
                if (overlappingPlanId.Count > 0)
                {
                    break;
                }
            }

            var machinedetails = condb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID).ToList();
            foreach (var machinerow in machinedetails)
            {
                int machineId = machinerow.MachineID;
                overlappingPlanId1 = MWO_OverlapCheckerForMachineDownwards(machineId, mwoID);
                if (overlappingPlanId1.Count > 0)
                {
                    break;
                }
            }

            //move all id's into one list.

            overlappingPlanId2.AddRange(overlappingPlanId);
            overlappingPlanId2.AddRange(overlappingPlanId1);

            if (overlappingPlanId2.Count == 0)
            {
                DataTable dataHolder = new DataTable();
                MsqlConnection mc = new MsqlConnection();
                mc.open();
                string sql = null;
                string cString = null;
                string wcString = null;
                if (mwoID != 0)
                {
                    sql = "SELECT MWOID FROM SRKSDemo.tblmultipleworkorder where ShopID = '" + ShopID + "'  and CellID is null and WCID is null  and IsDeleted = 0 and MWOID != " + mwoID + " ;";
                }
                else
                {
                    sql = "SELECT MWOID FROM SRKSDemo.tblmultipleworkorder where ShopID = '" + ShopID + "'  and CellID is null and WCID is null  and IsDeleted = 0 ;";
                }
                SqlDataAdapter da = new SqlDataAdapter(sql, mc.msqlConnection);
                da.Fill(dataHolder);
                mc.close();
                for (int i = 0; i < dataHolder.Rows.Count; i++)
                {
                    overlappingPlanId2.Add(Convert.ToInt32(dataHolder.Rows[i][0]));
                }
            }
            return overlappingPlanId2;
        }
        public List<int> MWO_OverlapCheckerForCellDownwards(int cellid, int mwoID = 0)
        {
            List<int> overlappingPlanId = new List<int>();
            int CellID = cellid;
            DataTable dataHolder = new DataTable();
            //1st check if its machines has a Plan.
            //so get its machineids.
            var machinedetails = condb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == CellID).ToList();
            foreach (var machinerow in machinedetails)
            {
                int machineId = machinerow.MachineID;
                overlappingPlanId = MWO_OverlapCheckerForMachineDownwards(machineId, mwoID);
                if (overlappingPlanId.Count > 0)
                {
                    break;
                }
            }

            if (overlappingPlanId.Count == 0)
            {
                MsqlConnection mc = new MsqlConnection();
                mc.open();
                string sql = null;
                string wcString = null;
                if (mwoID != 0)
                {
                    sql = "SELECT MWOID FROM SRKSDemo.tblmultipleworkorder where CellID = '" + CellID + "' and WCID is null and IsDeleted = 0 and MWOID != " + mwoID + " ;";
                }
                else
                {
                    sql = "SELECT MWOID FROM SRKSDemo.tblmultipleworkorder where CellID = '" + CellID + "' and WCID is null  and IsDeleted = 0  ;";
                }
                SqlDataAdapter da = new SqlDataAdapter(sql, mc.msqlConnection);
                da.Fill(dataHolder);
                mc.close();
                for (int i = 0; i < dataHolder.Rows.Count; i++)
                {
                    overlappingPlanId.Add(Convert.ToInt32(dataHolder.Rows[i][0]));
                }

            }
            return overlappingPlanId;
        }
        public List<int> MWO_OverlapCheckerForMachineDownwards(int wc, int mwoID = 0)
        {
            List<int> overlappingPlanId = new List<int>();
            int WorkCenterID = wc;
            DataTable dataHolder = new DataTable();

            MsqlConnection mc = new MsqlConnection();
            mc.open();

            string sql = null;
            if (mwoID != 0)
            {
                sql = "SELECT MWOID FROM SRKSDemo.tblmultipleworkorder where IsDeleted = 0 and MWOID != " + mwoID + " and WCID = '" + WorkCenterID + "' ;";
            }
            else
            {
                sql = "SELECT MWOID FROM SRKSDemo.tblmultipleworkorder where IsDeleted = 0 and WCID = '" + WorkCenterID + "' ;";
            }
            SqlDataAdapter da = new SqlDataAdapter(sql, mc.msqlConnection);
            da.Fill(dataHolder);
            mc.close();

            for (int i = 0; i < dataHolder.Rows.Count; i++)
            {
                overlappingPlanId.Add(Convert.ToInt32(dataHolder.Rows[i][0]));
            }
            return overlappingPlanId;
        }

        public List<int> MWO_OverlapCheckerForPlant(int plantid, int mwoID = 0)
        {
            List<int> overlappingPlanId = new List<int>();
            int PlantID = plantid;

            DataTable dataHolder = new DataTable();
            MsqlConnection mc = new MsqlConnection();
            mc.open();
            String sql = null;
            string wcString = null;
            string cString = null;
            string sString = null;
            if (mwoID != 0)
            {
                sql = "SELECT MWOID FROM SRKSDemo.tblmultipleworkorder where PlantID = '" + PlantID + "' and ShopID  is null and  CellID  is null and IsDeleted = 0 and MWOID != " + mwoID + " and WCID  is null ;";
            }
            else
            {
                sql = "SELECT MWOID FROM SRKSDemo.tblmultipleworkorder where PlantID = '" + PlantID + "' and ShopID  is null and  CellID  is null and IsDeleted = 0 and WCID  is null ;";
            }
            SqlDataAdapter da = new SqlDataAdapter(sql, mc.msqlConnection);
            da.Fill(dataHolder);
            mc.close();
            for (int i = 0; i < dataHolder.Rows.Count; i++)
            {
                overlappingPlanId.Add(Convert.ToInt32(dataHolder.Rows[i][0]));
            }
            return overlappingPlanId;
        }
        public List<int> MWO_OverlapCheckerForShop(int shopid, int mwoID = 0)
        {
            List<int> overlappingPlanId = new List<int>();
            int ShopID = shopid;

            //1st check if its Plant has a Plan.
            //so get its plantid.
            var plantdetails = condb.tblshops.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID).FirstOrDefault();
            int plantId = plantdetails.PlantID;
            overlappingPlanId = MWO_OverlapCheckerForPlant(plantId, mwoID);

            if (overlappingPlanId.Count == 0)
            {
                DataTable dataHolder = new DataTable();
                MsqlConnection mc = new MsqlConnection();
                mc.open();
                string sql = null;
                string wcString = null;
                string cString = null;
                if (mwoID != 0)
                {
                    sql = "SELECT MWOID FROM SRKSDemo.tblmultipleworkorder where ShopID = '" + ShopID + "' and  CellID is null and IsDeleted = 0 and MWOID != " + mwoID + " and WCID  is null ;";
                }
                else
                {
                    sql = "SELECT MWOID FROM SRKSDemo.tblmultipleworkorder where ShopID = '" + ShopID + "' and  CellID  is null and IsDeleted = 0 and WCID  is null ;";
                }
                SqlDataAdapter da = new SqlDataAdapter(sql, mc.msqlConnection);
                da.Fill(dataHolder);
                mc.close();

                for (int i = 0; i < dataHolder.Rows.Count; i++)
                {
                    overlappingPlanId.Add(Convert.ToInt32(dataHolder.Rows[i][0]));
                }
            }
            return overlappingPlanId;
        }
        public List<int> MWO_OverlapCheckerForCell(int cellid, int mwoID = 0)
        {
            List<int> overlappingPlanId = new List<int>();
            int CellID = cellid;
            DataTable dataHolder = new DataTable();

            //1st check if its Shop has a Plan.
            //so get its shopid.
            var Celldetails = condb.tblcells.Where(m => m.IsDeleted == 0 && m.CellID == CellID).FirstOrDefault();
            int shopId = Celldetails.ShopID;
            overlappingPlanId = MWO_OverlapCheckerForShop(shopId, mwoID);

            if (overlappingPlanId.Count == 0)
            {
                MsqlConnection mc = new MsqlConnection();
                mc.open();
                string sql = null;
                string wcString = null;
                if (mwoID != 0)
                {
                    sql = "SELECT MWOID FROM SRKSDemo.tblmultipleworkorder where CellID = '" + CellID + "' and IsDeleted = 0 and MWOID != " + mwoID + " and WCID is null ;";
                }
                else
                {
                    sql = "SELECT MWOID FROM SRKSDemo.tblmultipleworkorder where CellID = '" + CellID + "' and IsDeleted = 0 and WCID is null ;";
                }
                SqlDataAdapter da = new SqlDataAdapter(sql, mc.msqlConnection);
                da.Fill(dataHolder);
                mc.close();

                for (int i = 0; i < dataHolder.Rows.Count; i++)
                {
                    overlappingPlanId.Add(Convert.ToInt32(dataHolder.Rows[i][0]));
                }

            }
            return overlappingPlanId;
        }
        public List<int> MWO_OverlapCheckerForMachine(int wc, int mwoID = 0)
        {
            List<int> overlappingPlanId = new List<int>(), overlappingPlanId1 = new List<int>(), overlappingPlanId2 = new List<int>();
            int MachineID = wc;
            DataTable dataHolder = new DataTable();

            //1st check if it has a Cell else go for Shop

            var machinedetails = condb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID).FirstOrDefault();
            if (machinedetails.CellID.HasValue)
            {
                int cellId = Convert.ToInt32(machinedetails.CellID);
                overlappingPlanId = MWO_OverlapCheckerForCell(cellId, mwoID);
            }
            else
            {
                int shopId = Convert.ToInt32(machinedetails.ShopID);
                overlappingPlanId1 = MWO_OverlapCheckerForShop(shopId, mwoID);
            }

            overlappingPlanId2.AddRange(overlappingPlanId);
            overlappingPlanId2.AddRange(overlappingPlanId1);

            if (overlappingPlanId2.Count == 0)
            {
                MsqlConnection mc = new MsqlConnection();
                mc.open();
                string sql = null;
                if (mwoID != 0)
                {
                    sql = "SELECT MWOID FROM SRKSDemo.tblmultipleworkorder where IsDeleted = 0 and MWOID != " + mwoID + " and WCID = '" + MachineID + "' ;";
                }
                else
                {
                    sql = "SELECT MWOID FROM SRKSDemo.tblmultipleworkorder where IsDeleted = 0 and WCID = '" + MachineID + "' ;";

                }
                SqlDataAdapter da = new SqlDataAdapter(sql, mc.msqlConnection);
                da.Fill(dataHolder);
                mc.close();

                for (int i = 0; i < dataHolder.Rows.Count; i++)
                {
                    overlappingPlanId2.Add(Convert.ToInt32(dataHolder.Rows[i][0]));
                }
            }
            return overlappingPlanId2;
        }

        public ActionResult ExportEnabledMultiWODetails()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            int UserID = Convert.ToInt32(Session["UserId"]);

            #region Excel and Stuff
            DateTime frda = DateTime.Now;
            String FileDir = @"C:\I_ShopFloorReports\ReportsList\" + System.DateTime.Now.ToString("yyyy-MM-dd");
            bool exists = System.IO.Directory.Exists(FileDir);
            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "EnableMultiWO" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "EnableMultiWO" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //" to " + toda.ToString("yyyy-MM-dd") + 
                }
                catch
                {
                    TempData["Excelopen"] = "Excel with same date is already open, please close it and try to generate!!!!";
                    //return View();
                }
            }
            //Using the File for generation and populating it
            ExcelPackage p = null;
            p = new ExcelPackage(newFile);
            ExcelWorksheet worksheet = null;
            ExcelWorksheet worksheetGraph = null;

            //Creating the WorkSheet for populating
            try
            {
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"));
            }
            catch { }

            if (worksheet == null)
            {
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"));
            }
            int sheetcount = p.Workbook.Worksheets.Count;
            p.Workbook.Worksheets.MoveToStart(sheetcount);
            worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            //Header
            worksheet.Cells["A" + 1].Value = "MultiWO Name";
            worksheet.Cells["B" + 1].Value = "Plant";
            worksheet.Cells["C" + 1].Value = "Shop";
            worksheet.Cells["D" + 1].Value = "Cell";
            worksheet.Cells["E" + 1].Value = "WorkCenter";
            worksheet.Cells["F" + 1].Value = "Enable(Y/N)";
            //worksheet.Cells["G" + 1].Value = "Loss Type";
            //worksheet.Cells["H" +1].Value = "MessageType";
            //worksheet.Cells["I" +1].Value = "Contributes To";
            worksheet.Cells["A1:I1"].Style.Font.Bold = true;

            #endregion
            //var BreakDownLossesData = condb.tblmultipleworkorders.Where(m => m.IsDeleted == 0 && m.IsEnabled == 1).OrderBy(m => m.PlantID).ThenBy(m => m.ShopID).ThenBy(m => m.CellID).ToList();
            var BreakDownLossesData = condb.tblmultipleworkorders.Where(m => m.IsDeleted == 0).OrderBy(m => m.PlantID).ThenBy(m => m.ShopID).ThenBy(m => m.CellID).ToList();
            int i = 2;
            foreach (var row in BreakDownLossesData)
            {
                int PlantID = Convert.ToInt32(row.PlantID);
                string PlantName = Convert.ToString(condb.tblplants.Where(m => m.PlantID == PlantID).Select(m => m.PlantName).FirstOrDefault());
                int ShopID = Convert.ToInt32(row.ShopID);
                string ShopName = Convert.ToString(condb.tblshops.Where(m => m.ShopID == ShopID).Select(m => m.ShopName).FirstOrDefault());
                int CellID = Convert.ToInt32(row.CellID);
                string CellName = Convert.ToString(condb.tblcells.Where(m => m.CellID == CellID).Select(m => m.CellName).FirstOrDefault());
                int WCID = Convert.ToInt32(row.WCID);
                string WCName = Convert.ToString(condb.tblmachinedetails.Where(m => m.MachineID == WCID).Select(m => m.MachineDisplayName).FirstOrDefault());
                int IsEnable = Convert.ToInt32(row.IsEnabled);

                worksheet.Cells["A" + i].Value = row.MultipleWOName;
                worksheet.Cells["B" + i].Value = PlantName;
                worksheet.Cells["C" + i].Value = ShopName;
                worksheet.Cells["D" + i].Value = CellName;
                worksheet.Cells["E" + i].Value = WCName;
                if (IsEnable == 0)
                {
                    worksheet.Cells["F" + i].Value = "N";
                }
                else
                {
                    worksheet.Cells["F" + i].Value = "Y";
                }

                i++;
            }
            //Now push Every Other possible combinations into Excel (  so that it will be easy for user to do settings)
            //var PlantData = condb.tblplants.Where(m => m.PlantID == 1).FirstOrDefault();
            //var shopDetails = condb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0).Select(m => m.ShopID).Distinct().ToList();
            //foreach (var shopId in shopDetails)
            //{
            //    var MultiwoDetailsForShop = condb.tblmultipleworkorders.Where(m => m.ShopID == shopId && m.CellID == null).FirstOrDefault();
            //    if (MultiwoDetailsForShop != null)
            //    {
            //        continue;
            //    }
            //    else
            //    {
            //        var shopData = condb.tblshops.Where(m => m.ShopID == shopId).FirstOrDefault();
            //        //Insert for this Shop and Check for its child cells
            //        worksheet.Cells["A" + i].Value = PlantData.PlantName + "_" + shopData.ShopName;
            //        worksheet.Cells["B" + i].Value = PlantData.PlantName;
            //        worksheet.Cells["C" + i].Value = shopData.ShopName;
            //        i++;
            //        var CellDetails = condb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.ShopID == shopId).Select(m => m.CellID).Distinct().ToList();
            //        foreach (var cellId in CellDetails)
            //        {
            //            var MultiwoDetailsForCell = condb.tblmultipleworkorders.Where(m => m.ShopID == shopId && m.CellID == cellId && m.WCID == null).FirstOrDefault();
            //            if (MultiwoDetailsForCell != null)
            //            {
            //                continue;
            //            }
            //            else
            //            {
            //                //Insert for this Cell and Check for its child Macs
            //                var CellData = condb.tblcells.Where(m => m.ShopID == shopId && m.CellID == cellId).FirstOrDefault();
            //                //Insert for this Shop and Check for its child cells
            //                worksheet.Cells["A" + i].Value = PlantData.PlantName + "_" + shopData.ShopName;
            //                worksheet.Cells["B" + i].Value = PlantData.PlantName;
            //                worksheet.Cells["C" + i].Value = shopData.ShopName;
            //                //worksheet.Cells["D" + i].Value = CellData.CellName;
            //                i++;

            //                var WCDetails = condb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.ShopID == shopId && m.CellID == cellId && m.MachineID == null).Select(m => m.CellID).Distinct().ToList();
            //                foreach (var wcId in WCDetails)
            //                {
            //                    var MultiwoDetailsForWC = condb.tblmultipleworkorders.Where(m => m.ShopID == shopId && m.CellID == cellId && m.WCID == wcId).FirstOrDefault();
            //                    if (MultiwoDetailsForWC != null)
            //                    {
            //                        continue;
            //                    }
            //                    else
            //                    {
            //                        //Insert for this WorkCenter
            //                        var WCData = condb.tblmachinedetails.Where(m => m.ShopID == shopId && m.CellID == cellId && m.MachineID == wcId).FirstOrDefault();
            //                        worksheet.Cells["A" + i].Value = PlantData.PlantName + "_" + shopData.ShopName;
            //                        worksheet.Cells["B" + i].Value = PlantData.PlantName;
            //                        worksheet.Cells["C" + i].Value = shopData.ShopName;
            //                        worksheet.Cells["D" + i].Value = CellData.CellName;
            //                        worksheet.Cells["E" + i].Value = WCData.MachineDisplayName;
            //                        i++;
            //                    }
            //                }
            //             }
            //        }
            //    }
            //}

            if (BreakDownLossesData.Count != 0)
            {

                #region Save and Download

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                p.Save();

                //Downloding Excel
                string path1 = System.IO.Path.Combine(FileDir, "EnableMultiWO" + frda.ToString("yyyy-MM-dd") + ".xlsx");
                System.IO.FileInfo file1 = new System.IO.FileInfo(path1);
                string Outgoingfile = "EnableMultiWO" + frda.ToString("yyyy-MM-dd") + ".xlsx";
                if (file1.Exists)
                {
                    Response.Clear();
                    Response.ClearContent();
                    Response.ClearHeaders();
                    Response.AddHeader("Content-Disposition", "attachment; filename=" + Outgoingfile);
                    Response.AddHeader("Content-Length", file1.Length.ToString());
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.WriteFile(file1.FullName);
                    Response.Flush();
                    Response.Close();
                }
            }
            else
            {

            }
            #endregion
            TempData["toaster_success"] = "Export successfully";
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            //int UserID = Convert.ToInt32(Session["UserId"]);
            //String Username = Session["Username"].ToString();
            tblmultipleworkorder tblmwo = condb.tblmultipleworkorders.Find(id);
            if (tblmwo == null)
            {
                return HttpNotFound();
            }
            tblmwo.IsDeleted = 1;
            tblmwo.ModifiedBy = 1;
            tblmwo.ModifiedOn = System.DateTime.Now;
            condb.Entry(tblmwo).State = EntityState.Modified;
            condb.SaveChanges();
            TempData["toaster_success"] = "Data Deleted successfully";
            return RedirectToAction("Index");
        }

        public bool ValidationCheckForInsertion(int plantID, int shopID, int cellid, int Wcid, int mwoid)
        {
            bool count = true;
            int varcount = 0;
            if (mwoid == 0)
            {
                var countQuery = condb.tblmultipleworkorders.Where(m => m.PlantID == plantID && m.ShopID == shopID && m.CellID == cellid && m.WCID == Wcid && m.IsDeleted == 0 && m.MWOID == mwoid).ToList();
                varcount = countQuery.Count();
            }
            else
            {
                var countQuery = condb.tblmultipleworkorders.Where(m => m.MWOID == mwoid && m.PlantID == plantID && m.ShopID == shopID && m.CellID == cellid && m.WCID == Wcid && m.IsDeleted == 0).ToList();
                varcount = countQuery.Count();
            }

            if (varcount == 0)
            {
                count = true;
            }
            else
            {
                count = false;
            }
            return count;
        }

        [HttpPost]
        public ActionResult ImportMultipleWorkorder(HttpPostedFileBase file, string UploadType)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            //Deleting Excel file
            #region
            string fileLocation1 = Server.MapPath("~/Content/");
            DirectoryInfo di = new DirectoryInfo(fileLocation1);
            FileInfo[] files = di.GetFiles("*.xlsx").Where(p => p.Extension == ".xlsx").ToArray();
            foreach (FileInfo file1 in files)
                try
                {
                    file1.Attributes = FileAttributes.Normal;
                    System.IO.File.Delete(file1.FullName);
                }
                catch { }
            #endregion

            DataSet ds = new DataSet();
            if (Request.Files["file"].ContentLength > 0)
            {

                string fileExtension = System.IO.Path.GetExtension(Request.Files["file"].FileName);
                if (fileExtension == ".xls" || fileExtension == ".xlsx")
                {
                    string fileLocation = Server.MapPath("~/Content/") + Request.Files["file"].FileName;
                    if (System.IO.File.Exists(fileLocation))
                    {
                        System.IO.File.Delete(fileLocation);
                    }
                    Request.Files["file"].SaveAs(fileLocation);
                    string excelConnectionString = string.Empty;
                    excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                    fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    //connection String for xls file format.
                    if (fileExtension == ".xls")
                    {
                        excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
                        fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                    }
                    //connection String for xlsx file format.
                    else if (fileExtension == ".xlsx")
                    {
                        excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                        fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    }
                    //Create Connection to Excel work book and add oledb namespace
                    OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                    excelConnection.Open();
                    DataTable dt = new DataTable();
                    dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    if (dt == null)
                    {
                        return null;
                    }
                    String[] excelSheets = new String[dt.Rows.Count];
                    int t = 0;
                    //excel data saves in temp file here.
                    foreach (DataRow row in dt.Rows)
                    {
                        excelSheets[t] = row["TABLE_NAME"].ToString();
                        t++;
                    }
                    OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);
                    string query = string.Format("Select * from [{0}]", excelSheets[0]);
                    using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                    {
                        dataAdapter.Fill(ds);
                    }
                    excelConnection.Close();
                    excelConnection1.Close();
                }
                if (fileExtension.ToString().ToLower().Equals(".xml"))
                {
                    string fileLocation = Server.MapPath("~/Content/") + Request.Files["FileUpload"].FileName;
                    if (System.IO.File.Exists(fileLocation))
                    {
                        System.IO.File.Delete(fileLocation);
                    }
                    Request.Files["FileUpload"].SaveAs(fileLocation);
                    XmlTextReader xmlreader = new XmlTextReader(fileLocation);
                    // DataSet ds = new DataSet();
                    ds.ReadXml(xmlreader);
                    xmlreader.Close();
                }
                if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
                {
                    return RedirectToAction("Login", "Login", null);
                }
                ViewBag.Logout = Session["Username"].ToString().ToUpper();
                ViewBag.roleid = Session["RoleID"];

                var unitsItem = condb.tblmultipleworkorders.Where(m => m.IsDeleted == 0).ToList();

                List<string> TimeUnits = new List<string>();
                foreach (var item in unitsItem)
                {
                    TimeUnits.Add(item.MultipleWOName.ToString());
                    TimeUnits.Add(item.PlantID.ToString());
                }
                string text = "";
                string ErrorMsg = null;
                if (UploadType == "OverWrite") // Accept only New Codes
                {
                    #region
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {

                        tblmultipleworkorder tblmp = new tblmultipleworkorder();
                        String Username = Session["Username"].ToString();
                        tblmp.CreatedBy = Convert.ToInt32(Session["UserId"]);
                        tblmp.CreatedOn = DateTime.Now;
                        tblmp.IsDeleted = 0;

                        string MultiWOName = null, PlantName = null, ShopName = null, CellName = null, MachineName = null, IsEnable = null;
                        int PlantID = 0, ShopID = 0, CellID = 0, MachineID = 0, Enable = 0;
                        MultiWOName = Convert.ToString(ds.Tables[0].Rows[i][0]);

                        PlantName = Convert.ToString(ds.Tables[0].Rows[i][1]);
                        ShopName = Convert.ToString(ds.Tables[0].Rows[i][2]);
                        CellName = Convert.ToString(ds.Tables[0].Rows[i][3]);
                        MachineName = Convert.ToString(ds.Tables[0].Rows[i][4]);
                        IsEnable = Convert.ToString(ds.Tables[0].Rows[i][5]);


                        using (i_facility_shaktiEntities1 condb1 = new i_facility_shaktiEntities1())
                        {
                            var MasterStdPWTData = condb1.tblmultipleworkorders.Where(m => m.MultipleWOName != MultiWOName).FirstOrDefault();



                            if (MasterStdPWTData != null)
                            {
                                try
                                {

                                    try
                                    {
                                        MasterStdPWTData.MultipleWOName = Convert.ToString(ds.Tables[0].Rows[i][0]);

                                    }
                                    catch
                                    {
                                        text = text + htmlerrorMaker(MultiWOName, "Please check with Multiple Work Order Name");
                                        continue;
                                    }
                                    try
                                    {


                                        PlantID = Convert.ToInt32(condb.tblplants.Where(m => m.PlantName == PlantName && m.IsDeleted == 0).Select(m => m.PlantID).FirstOrDefault());

                                        MasterStdPWTData.PlantID = PlantID;


                                    }

                                    catch
                                    {
                                        text = text + htmlerrorMaker(MultiWOName, "Please check with Plant Name");
                                        continue;
                                    }
                                    try
                                    {


                                        ShopID = Convert.ToInt32(condb.tblshops.Where(m => m.ShopName == ShopName && m.IsDeleted == 0).Select(m => m.ShopID).FirstOrDefault());

                                        MasterStdPWTData.ShopID = ShopID;


                                    }

                                    catch
                                    {
                                        text = text + htmlerrorMaker(MultiWOName, "Please check with Plant Name");
                                        continue;
                                    }
                                    try
                                    {


                                        CellID = Convert.ToInt32(condb.tblcells.Where(m => m.CellName == CellName && m.IsDeleted == 0).Select(m => m.CellID).FirstOrDefault());

                                        MasterStdPWTData.CellID = CellID;


                                    }

                                    catch
                                    {
                                        text = text + htmlerrorMaker(MultiWOName, "Please check with Plant Name");
                                        continue;
                                    }
                                    try
                                    {


                                        MachineID = Convert.ToInt32(condb.tblmachinedetails.Where(m => m.MachineName == MachineName && m.IsDeleted == 0).Select(m => m.MachineID).FirstOrDefault());

                                        MasterStdPWTData.WCID = MachineID;


                                    }

                                    catch
                                    {
                                        text = text + htmlerrorMaker(MultiWOName, "Please check with Plant Name");
                                        continue;
                                    }
                                    try
                                    {
                                        if (IsEnable == "y" || IsEnable == "Y")
                                        {
                                            Enable = 1;
                                        }
                                        else
                                        {
                                            Enable = 0;
                                        }



                                        MasterStdPWTData.IsEnabled = Enable;


                                    }

                                    catch
                                    {
                                        text = text + htmlerrorMaker(MultiWOName, "Please check with Plant Name");
                                        continue;
                                    }

                                    MasterStdPWTData.ModifiedOn = DateTime.Now;
                                    MasterStdPWTData.ModifiedBy = Convert.ToInt32(Session["UserId"]);

                                    condb1.SaveChanges();
                                    continue;
                                }
                                catch (DbEntityValidationException e)
                                {
                                    foreach (var eve in e.EntityValidationErrors)
                                    {
                                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                                        foreach (var ve in eve.ValidationErrors)
                                        {
                                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                                ve.PropertyName, ve.ErrorMessage);
                                        }
                                    }

                                }
                            }
                            else if (string.IsNullOrEmpty(MultiWOName))
                            {
                                text = text + htmlerrorMaker(MultiWOName, "PartName or OperationNo cannot be empty");
                                //ErrorMsg += " PartName or OperationNo cannot be empty for " + PartName + " and " + OperationNo + " .\n";
                                continue;
                            }
                            else
                            {

                                tblmp.MultipleWOName = MultiWOName;
                                tblmp.PlantID = PlantID;
                                tblmp.ShopID = ShopID;
                                tblmp.CellID = CellID;
                                tblmp.MWOID = MachineID;
                                tblmp.IsEnabled = Enable;
                            }
                        }

                        condb.tblmultipleworkorders.Add(tblmp);
                        condb.SaveChanges();
                    }
                    #endregion
                }
                else if (UploadType == "Update") // OverWrite Existing Values 
                {
                    #region
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {

                        tblmultipleworkorder tblmp = new tblmultipleworkorder();
                        String Username = Session["Username"].ToString();
                        tblmp.CreatedBy = Convert.ToInt32(Session["UserId"]);
                        tblmp.CreatedOn = DateTime.Now;
                        tblmp.IsDeleted = 0;

                        string MultiWOName = null, PlantName = null, ShopName = null, CellName = null, MachineName = null, IsEnable = null;
                        int PlantID = 0, ShopID = 0, CellID = 0, MachineID = 0, Enable = 0;
                        MultiWOName = Convert.ToString(ds.Tables[0].Rows[i][0]);

                        PlantName = Convert.ToString(ds.Tables[0].Rows[i][1]);
                        ShopName = Convert.ToString(ds.Tables[0].Rows[i][2]);
                        CellName = Convert.ToString(ds.Tables[0].Rows[i][3]);
                        MachineName = Convert.ToString(ds.Tables[0].Rows[i][4]);
                        IsEnable = Convert.ToString(ds.Tables[0].Rows[i][5]);

                        DateTime createdOn = DateTime.Now;

                        if (string.IsNullOrEmpty(MultiWOName) || string.IsNullOrEmpty(PlantName) || string.IsNullOrEmpty(ShopName) || string.IsNullOrEmpty(CellName) || string.IsNullOrEmpty(MachineName))
                        {
                            text = text + htmlerrorMaker(MultiWOName, "MWOName or Other Data cannot be empty");
                            continue;
                        }
                        else
                        {
                            try
                            {
                                MultiWOName = Convert.ToString(ds.Tables[0].Rows[i][0]);
                                tblmp.MultipleWOName = MultiWOName;

                            }
                            catch
                            {
                                text = text + htmlerrorMaker(MultiWOName, "Please check with Multi Work OrderName");
                                continue;
                            }
                            try
                            {
                                PlantID = Convert.ToInt32(condb.tblplants.Where(m => m.PlantName == PlantName && m.IsDeleted == 0).Select(m => m.PlantID).FirstOrDefault());
                                tblmp.PlantID = PlantID;

                            }
                            catch
                            {
                                text = text + htmlerrorMaker(PlantName, "Please check with PlantName ");
                                continue;
                            }
                            try
                            {
                                ShopID = Convert.ToInt32(condb.tblshops.Where(m => m.ShopName == ShopName && m.IsDeleted == 0).Select(m => m.ShopID).FirstOrDefault());
                                tblmp.ShopID = ShopID;

                            }
                            catch
                            {
                                text = text + htmlerrorMaker(ShopName, "Please check with ShopName ");
                                continue;
                            }
                            try
                            {
                                CellID = Convert.ToInt32(condb.tblcells.Where(m => m.CellName == CellName && m.IsDeleted == 0).Select(m => m.CellID).FirstOrDefault());
                                tblmp.CellID = CellID;

                            }
                            catch
                            {
                                text = text + htmlerrorMaker(CellName, "Please check with CellName ");
                                continue;
                            }
                            try
                            {
                                MachineID = Convert.ToInt32(condb.tblmachinedetails.Where(m => m.MachineName == MachineName && m.IsDeleted == 0).Select(m => m.MachineID).FirstOrDefault());
                                tblmp.WCID = MachineID;

                            }
                            catch
                            {
                                text = text + htmlerrorMaker(MachineName, "Please check with MachineName ");
                                continue;
                            }
                            try
                            {
                                if (IsEnable == "y" || IsEnable == "Y")
                                {
                                    Enable = 1;
                                }
                                else
                                {
                                    Enable = 0;
                                }
                                tblmp.IsEnabled = Enable;

                            }
                            catch
                            {
                                text = text + htmlerrorMaker(MultiWOName, "Please check with Is Enable");
                                continue;
                            }


                            //var MasterStdPWTData = db.tblparts.Where(m => m.Part_No == PartName && m.OperationNo == OperationNo && m.IsDeleted == 0).FirstOrDefault();
                            var MasterStdPWTData = condb.tblmultipleworkorders.Where(m => m.MultipleWOName == MultiWOName).FirstOrDefault();

                            if (MasterStdPWTData == null)
                            {
                                condb.tblmultipleworkorders.Add(tblmp);
                                condb.SaveChanges();
                            }
                            else
                            {

                                MasterStdPWTData.ModifiedBy = Convert.ToInt32(Session["UserId"]);
                                MasterStdPWTData.ModifiedOn = DateTime.Now;
                                MasterStdPWTData.IsDeleted = 1;
                                //MasterStdPWTData = DateTime.Now;
                                condb.Entry(MasterStdPWTData).State = EntityState.Modified;
                                condb.SaveChanges();
                                condb.tblmultipleworkorders.Add(tblmp);
                                condb.SaveChanges();
                            }

                        }
                    }
                    #endregion
                }
                //else if (UploadType == "New") // Delete Duplicate and Insert New. // if not Duplicate insert that
                //{
                //    #region
                //    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                //    {

                //        tblmultipleworkorder tblmp = new tblmultipleworkorder();
                //        String Username = Session["Username"].ToString();
                //        tblmp.CreatedBy = Convert.ToInt32(Session["UserId"]);
                //        tblmp.CreatedOn = DateTime.Now;
                //        tblmp.IsDeleted = 0;

                //        string MultiWOName = null, PlantName = null, ShopName = null, CellName = null, MachineName = null, IsEnable = null;
                //        int PlantID = 0, ShopID = 0, CellID = 0, MachineID = 0, Enable = 0;
                //        MultiWOName = Convert.ToString(ds.Tables[0].Rows[i][0]);

                //        PlantName = Convert.ToString(ds.Tables[0].Rows[i][1]);
                //        ShopName = Convert.ToString(ds.Tables[0].Rows[i][2]);
                //        CellName = Convert.ToString(ds.Tables[0].Rows[i][3]);
                //        MachineName = Convert.ToString(ds.Tables[0].Rows[i][4]);
                //        IsEnable = Convert.ToString(ds.Tables[0].Rows[i][5]);


                //        bool check = ValidationCheckForInsertion(PlantID, ShopID, CellID, MachineID);
                //        //bool check = true;
                //        if (check == true)
                //        {
                //            using (i_facility_shaktiEntities1 db1 = new i_facility_shaktiEntities1())
                //            {
                //                try
                //                {
                //                    if (string.IsNullOrEmpty(MultiWOName) || string.IsNullOrEmpty(PlantName) || string.IsNullOrEmpty(ShopName) || string.IsNullOrEmpty(CellName) || string.IsNullOrEmpty(MachineName))
                //                    {
                //                        text = text + htmlerrorMaker(MultiWOName, "MWOName or Other Data cannot be empty");
                //                        continue;
                //                    }
                //                    else
                //                    {
                //                        try
                //                        {
                //                            MultiWOName = Convert.ToString(ds.Tables[0].Rows[i][0]);
                //                            tblmp.MultipleWOName = MultiWOName;

                //                        }
                //                        catch
                //                        {
                //                            text = text + htmlerrorMaker(MultiWOName, "Please check with Multi Work OrderName");
                //                            continue;
                //                        }
                //                        try
                //                        {
                //                            PlantID = Convert.ToInt32(condb.tblplants.Where(m => m.PlantName == PlantName && m.IsDeleted == 0).Select(m => m.PlantID).FirstOrDefault());
                //                            tblmp.PlantID = PlantID;

                //                        }
                //                        catch
                //                        {
                //                            text = text + htmlerrorMaker(PlantName, "Please check with PlantName ");
                //                            continue;
                //                        }
                //                        try
                //                        {
                //                            ShopID = Convert.ToInt32(condb.tblshops.Where(m => m.ShopName == ShopName && m.IsDeleted == 0).Select(m => m.ShopID).FirstOrDefault());
                //                            tblmp.ShopID = ShopID;

                //                        }
                //                        catch
                //                        {
                //                            text = text + htmlerrorMaker(ShopName, "Please check with ShopName ");
                //                            continue;
                //                        }
                //                        try
                //                        {
                //                            CellID = Convert.ToInt32(condb.tblcells.Where(m => m.CellName == CellName && m.IsDeleted == 0).Select(m => m.CellID).FirstOrDefault());
                //                            tblmp.CellID = CellID;

                //                        }
                //                        catch
                //                        {
                //                            text = text + htmlerrorMaker(CellName, "Please check with CellName ");
                //                            continue;
                //                        }
                //                        try
                //                        {
                //                            MachineID = Convert.ToInt32(condb.tblmachinedetails.Where(m => m.MachineName == MachineName && m.IsDeleted == 0).Select(m => m.MachineID).FirstOrDefault());
                //                            tblmp.WCID = MachineID;

                //                        }
                //                        catch
                //                        {
                //                            text = text + htmlerrorMaker(MachineName, "Please check with MachineName ");
                //                            continue;
                //                        }
                //                        try
                //                        {
                //                            if (IsEnable == "y" || IsEnable == "Y")
                //                            {
                //                                Enable = 1;
                //                            }
                //                            else
                //                            {
                //                                Enable = 0;
                //                            }
                //                            tblmp.IsEnabled = Enable;

                //                        }
                //                        catch
                //                        {
                //                            text = text + htmlerrorMaker(MultiWOName, "Please check with Is Enable");
                //                            continue;
                //                        }
                //                        //check for dup and delete previous one.
                //                        var Dupdata = condb.tblmultipleworkorders.Where(m => m.MultipleWOName == MultiWOName && m.IsDeleted == 0).FirstOrDefault();
                //                        if (Dupdata != null)
                //                        {
                //                            Dupdata.IsDeleted = 1;
                //                            //Dupdata.DeletedDate = DateTime.Now;
                //                            //db.Entry(Dupdata).State = EntityState.Modified;
                //                            db1.SaveChanges();
                //                        }

                //                        db1.tblmultipleworkorders.Add(tblmp);
                //                        db1.SaveChanges();
                //                    }
                //                }
                //                catch (DbEntityValidationException e)
                //                {
                //                    foreach (var eve in e.EntityValidationErrors)
                //                    {
                //                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                //                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                //                        foreach (var ve in eve.ValidationErrors)
                //                        {
                //                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                //                                ve.PropertyName, ve.ErrorMessage);
                //                        }
                //                    }
                //                    //throw;
                //                }
                //            }
                //        }
                //    }
                //    #endregion
                //}

                else if (UploadType == "New") // Delete Duplicate and Insert New. // if not Duplicate insert that
                {
                    #region
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {

                        tblmultipleworkorder tblmp = new tblmultipleworkorder();
                        String Username = Session["Username"].ToString();
                        tblmp.CreatedBy = Convert.ToInt32(Session["UserId"]);
                        tblmp.CreatedOn = DateTime.Now;
                        tblmp.IsDeleted = 0;

                        string MultiWOName = null, PlantName = null, ShopName = null, CellName = null, MachineName = null, IsEnable = null;
                        int PlantID = 0, ShopID = 0, CellID = 0, MachineID = 0, Enable = 0;
                        MultiWOName = Convert.ToString(ds.Tables[0].Rows[i][0]);

                        PlantName = Convert.ToString(ds.Tables[0].Rows[i][1]);
                        ShopName = Convert.ToString(ds.Tables[0].Rows[i][2]);
                        CellName = Convert.ToString(ds.Tables[0].Rows[i][3]);
                        MachineName = Convert.ToString(ds.Tables[0].Rows[i][4]);
                        IsEnable = Convert.ToString(ds.Tables[0].Rows[i][5]);



                        var mwoid = Convert.ToInt32(condb.tblmultipleworkorders.Where(m => m.MultipleWOName == MultiWOName && m.IsDeleted == 0).Select(m => m.MWOID).FirstOrDefault());
                        var plantID = Convert.ToInt32(condb.tblplants.Where(m => m.PlantName == PlantName && m.IsDeleted == 0).Select(m => m.PlantID).FirstOrDefault());
                        var shopID = Convert.ToInt32(condb.tblshops.Where(m => m.ShopName == ShopName && m.IsDeleted == 0).Select(m => m.ShopID).FirstOrDefault());
                        var cellid = Convert.ToInt32(condb.tblcells.Where(m => m.CellName == CellName && m.IsDeleted == 0).Select(m => m.CellID).FirstOrDefault());
                        var wcid = Convert.ToInt32(condb.tblmachinedetails.Where(m => m.MachineName == MachineName && m.IsDeleted == 0).Select(m => m.MachineID).FirstOrDefault());
                        //mwoid = Convert.ToInt32(ds.Tables[0].Rows[i][0]);

                        //PlantID = Convert.ToInt32(ds.Tables[0].Rows[i][1]);
                        //ShopID = Convert.ToInt32(ds.Tables[0].Rows[i][2]);
                        //CellID = Convert.ToInt32(ds.Tables[0].Rows[i][3]);
                        //MachineID = Convert.ToInt32(ds.Tables[0].Rows[i][4]);
                        // IsEnable = Convert.ToInt32(ds.Tables[0].Rows[i][5]);


                        bool check = ValidationCheckForInsertion(plantID, shopID, cellid, wcid, mwoid);
                        //bool check = true;
                        if (check == true)
                        {
                            using (i_facility_shaktiEntities1 condb1 = new i_facility_shaktiEntities1())
                            {

                                if (string.IsNullOrEmpty(MultiWOName) || string.IsNullOrEmpty(PlantName) || string.IsNullOrEmpty(ShopName) || string.IsNullOrEmpty(CellName) || string.IsNullOrEmpty(MachineName))
                                {
                                    text = text + htmlerrorMaker(MultiWOName, "MWOName or Other Data cannot be empty");
                                    continue;
                                }
                                else
                                {
                                    try
                                    {
                                        tblmp.PlantID = plantID;
                                        tblmp.ShopID = shopID;
                                        tblmp.CellID = cellid;
                                        tblmp.WCID = wcid;
                                        tblmp.MWOID = mwoid;
                                        tblmp.MultipleWOName = MultiWOName;
                                        if (IsEnable == "y" || IsEnable == "Y")
                                        {
                                            Enable = 1;
                                        }
                                        else
                                        {
                                            Enable = 0;
                                        }
                                        tblmp.IsEnabled = Enable;
                                        tblmp.CreatedBy = Convert.ToInt32(1);
                                        tblmp.CreatedOn = DateTime.Now;
                                    }
                                    catch
                                    {

                                    }
                                }
                                //check for dup and delete previous one.
                                var Dupdata = condb.tblmultipleworkorders.Where(m => m.PlantID == PlantID && m.IsDeleted == 0 && m.ShopID == ShopID && m.CellID == CellID && m.WCID == MachineID).FirstOrDefault();
                                if (Dupdata != null)
                                {
                                    Dupdata.IsDeleted = 1;
                                    Dupdata.ModifiedOn = DateTime.Now;
                                    Dupdata.ModifiedBy = Convert.ToInt32(1);
                                    condb.Entry(Dupdata).State = EntityState.Modified;
                                    condb.SaveChanges();
                                }

                                condb1.tblmultipleworkorders.Add(tblmp);
                                try
                                {
                                    condb1.SaveChanges();
                                }
                                catch (DbEntityValidationException e)
                                {
                                    foreach (var eve in e.EntityValidationErrors)
                                    {
                                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                                        foreach (var ve in eve.ValidationErrors)
                                        {
                                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                                ve.PropertyName, ve.ErrorMessage);
                                        }
                                    }
                                    //throw;
                                }
                            }
                        }
                    }
                    #endregion
                }
                TempData["txtShow"] = text;
                Session["Part_No"] = ErrorMsg;
            }
            TempData["toaster_success"] = "Import successfully";
            return RedirectToAction("Index");
        }

        public string htmlerrorMaker(string MWOName, string message)
        {
            string val = "";

            val = "<tr><td>" + MWOName + "</td><td>" + message + "</td></tr>";

            return val;
        }
    }
}