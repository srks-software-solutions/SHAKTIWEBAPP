using OfficeOpenXml;
using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace SRKSDemo.Controllers
{
    public class MultiWorkOrderController : Controller
    {
        private i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();

        public ActionResult Index()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            var MulitWOList = db.tblmultipleworkorders.Where(m => m.IsDeleted == 0).ToList();
            return View(MulitWOList);
        }

        public ActionResult Create()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            @Session["Error"] = null;
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();

            ViewBag.PlantID = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewBag.ShopID = new SelectList(db.tblshops.Where(m => m.IsDeleted == 2), "ShopID", "ShopName");
            ViewBag.CellID = new SelectList(db.tblcells.Where(m => m.IsDeleted == 2), "CellID", "CellName");
            ViewBag.WCID = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 2), "MachineID", "MachineInvNo");

            return View();
        }
        [HttpPost]
        public ActionResult Create(tblmultipleworkorder tee)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            int UserID = Convert.ToInt32(Session["UserId"]);
            #region//ActiveLog Code
            //string CompleteModificationdetail = "New Creation";
            //Action = "Create";
            // ActiveLogStorage Obj = new ActiveLogStorage();
            // Obj.SaveActiveLog(Action, Controller, Username, UserID, CompleteModificationdetail);
            //End
            #endregion

            //MultiWo validation
            int mwoID = Convert.ToInt32(tee.MWOID);
            string Plantidstring = Convert.ToString(tee.PlantID);
            string Shopidstring = Convert.ToString(tee.ShopID);
            string Cellidstring = Convert.ToString(tee.CellID);
            string WorkCenteridstring = Convert.ToString(tee.WCID);
            string ValidEscalation = null;

            ValidEscalation = IsItValidMWO(Plantidstring, Shopidstring, Cellidstring, WorkCenteridstring);

            if (ValidEscalation == null)
            {
                tee.CreatedBy = UserID;
                tee.CreatedOn = DateTime.Now;
                tee.IsDeleted = 0;
                tee.IsEnabled = 1;
                //tee.MultipleWOName = db.tblmachinedetails.Where(m => m.MachineID == tee.WCID && m.IsDeleted == 0).Select(m => m.MachineDispName).SingleOrDefault();

                db.tblmultipleworkorders.Add(tee);
                db.SaveChanges();
            }
            else
            {
                Session["Error"] = ValidEscalation;

                ViewBag.PlantID = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", tee.PlantID);
                ViewBag.ShopID = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == tee.PlantID), "ShopID", "ShopName", tee.ShopID);
                ViewBag.CellID = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.ShopID == tee.ShopID), "CellID", "CellName", tee.CellID);
                //bool tick = doesThisMachineHasCELL(Convert.ToInt32(tee.WCID));
                if (tee.CellID != null || tee.CellID != 0)
                {
                    ViewBag.WCID = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == tee.CellID), "MachineID", "MachineInvNo", tee.WCID);
                }
                else
                {
                    ViewBag.WCID = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == tee.ShopID), "MachineID", "MachineInvNo", tee.WCID);
                }
                return View(tee);
            }

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            @Session["Error"] = null;
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            tblmultipleworkorder tee = db.tblmultipleworkorders.Find(id);

            ViewBag.Plant = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", tee.PlantID);
            ViewBag.Shop = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == tee.PlantID), "ShopID", "ShopName", tee.ShopID);
            ViewBag.Cell = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.ShopID == tee.ShopID), "CellID", "CellName", tee.CellID);
            if (tee.CellID != null || tee.CellID != 0)
            {
                ViewBag.WorkCenter = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == tee.CellID), "MachineID", "MachineInvNo", tee.WCID);
            }
            else
            {
                ViewBag.WorkCenter = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == tee.ShopID), "MachineID", "MachineInvNo", tee.WCID);
            }

            return View(tee);
        }
        [HttpPost]
        public ActionResult Edit(tblmultipleworkorder tee)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            #region//ActiveLog Code
            int UserID = Convert.ToInt32(Session["UserId"]);
            //string CompleteModificationdetail = "New Creation";
            //Action = "Create";
            // ActiveLogStorage Obj = new ActiveLogStorage();
            // Obj.SaveActiveLog(Action, Controller, Username, UserID, CompleteModificationdetail);
            //End
            #endregion

            //Email validation
            int MWOID = Convert.ToInt32(tee.MWOID);
            string Plantidstring = Convert.ToString(tee.PlantID);
            string Shopidstring = Convert.ToString(tee.ShopID);
            string Cellidstring = Convert.ToString(tee.CellID);
            string WorkCenteridstring = Convert.ToString(tee.WCID);
            string ValidEscalation = null;

            ValidEscalation = IsItValidMWO(Plantidstring, Shopidstring, Cellidstring, WorkCenteridstring, MWOID);

            if (ValidEscalation == null)
            {
                tee.ModifiedBy = UserID;
                tee.ModifiedOn = DateTime.Now;
                tee.IsDeleted = 0;
                tee.IsEnabled = 1;
                //tee.MultipleWOName = db.tblmachinedetails.Where(m => m.MachineID == tee.WCID && m.IsDeleted == 0).Select(m => m.MachineDispName).SingleOrDefault();

                db.Entry(tee).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                Session["Error"] = ValidEscalation;

                ViewBag.Plant = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", tee.PlantID);
                ViewBag.Shop = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == tee.PlantID), "ShopID", "ShopName", tee.ShopID);
                ViewBag.Cell = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.ShopID == tee.ShopID), "CellID", "CellName", tee.CellID);
                //bool tick = doesThisMachineHasCELL(Convert.ToInt32(tee.WCID));
                if (tee.CellID != null || tee.CellID != 0)
                {
                    ViewBag.WorkCenter = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == tee.CellID), "MachineID", "MachineInvNo", tee.WCID);
                }
                else
                {
                    ViewBag.WorkCenter = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == tee.ShopID), "MachineID", "MachineInvNo", tee.WCID);
                }
                return View(tee);
            }

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            int UserID = Convert.ToInt32(Session["UserId"]);

            //ViewBag.IsConfigMenu = 0;
            tblmultipleworkorder tee = db.tblmultipleworkorders.Find(id);
            tee.IsDeleted = 1;
            tee.IsEnabled = 0;
            tee.ModifiedBy = UserID;
            tee.ModifiedOn = System.DateTime.Now;
            //start Logging

            String Username = Session["Username"].ToString();
            //string CompleteModificationdetail = "Deleted Parts/Item";
            //ActiveLogStorage Obj = new ActiveLogStorage();
            //Obj.SaveActiveLog(Action, Controller, Username, UserID, CompleteModificationdetail);
            //End
            db.Entry(tee).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        bool doesThisMachineHasCELL(int macid)
        {
            bool result = false;
            var machdetails = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == macid).SingleOrDefault();

            if (Convert.ToInt32(machdetails.CellID) != 0)
            {
                result = true;
            }
            return result;
        }

        public string IsItValidMWO(string Plantid, string Shopid, string Cellid, string WorkCenterid, int mwoID = 0)
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
                msg = null;
            }
            else
            {
                var results = db.tblmultipleworkorders.Where(m => m.IsDeleted == 0).Where(x => DoesThisPlanOverlapAll.Contains(x.MWOID));
                string OLPD = "<div  style='font-size:.75vw'>";
                OLPD += "<p><span>This Multi Work Order conflicts with  </span></p>";
                foreach (var row in results)
                {
                    OLPD += "<p><span>Multi WO Name : " + row.MultipleWOName + " </span></p>";
                }
                OLPD += "</div>";
                msg = OLPD;
            }
            return msg;
        }

        public List<int> MWO_OverlapCheckerForPlantDownwards(int plantid, int mwoID = 0)
        {
            List<int> overlappingPlanId = new List<int>();
            int PlantID = plantid;
            DataTable dataHolder = new DataTable();

            //1st check if its shop has a Plan.
            //so get its shopid.
            var shopdetails = db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID).ToList();
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
                string sString = null;
                string cString = null;
                string wcString = null;
                if (mwoID != 0)
                {
                    sql = "SELECT MWOID FROM " + MsqlConnection.DbName + ".tblmultipleworkorder where PlantID = '" + PlantID + "' and ShopID is null and CellID is null and WCID is null and IsDeleted = 0 and MWOID != " + mwoID + " ;";
                }
                else
                {
                    sql = "SELECT MWOID FROM " + MsqlConnection.DbName + ".tblmultipleworkorder where PlantID = '" + PlantID + "' and ShopID is null and CellID is null and WCID is null and IsDeleted = 0 ;";
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
            var celldetails = db.tblcells.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID).ToList();
            foreach (var cellrow in celldetails)
            {
                int cellId = cellrow.CellID;
                overlappingPlanId = MWO_OverlapCheckerForCellDownwards(cellId, mwoID);
                if (overlappingPlanId.Count > 0)
                {
                    break;
                }
            }

            var machinedetails = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID).ToList();
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
                    sql = "SELECT MWOID FROM " + MsqlConnection.DbName + ".tblmultipleworkorder where ShopID = '" + ShopID + "'  and CellID is null and WCID is null  and IsDeleted = 0 and MWOID != " + mwoID + " ;";
                }
                else
                {
                    sql = "SELECT MWOID FROM " + MsqlConnection.DbName + ".tblmultipleworkorder where ShopID = '" + ShopID + "'  and CellID is null and WCID is null  and IsDeleted = 0 ;";
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
            var machinedetails = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == CellID).ToList();
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
                    sql = "SELECT MWOID FROM " + MsqlConnection.DbName + ".tblmultipleworkorder where CellID = '" + CellID + "' and WCID is null and IsDeleted = 0 and MWOID != " + mwoID + " ;";
                }
                else
                {
                    sql = "SELECT MWOID FROM " + MsqlConnection.DbName + ".tblmultipleworkorder where CellID = '" + CellID + "' and WCID is null  and IsDeleted = 0  ;";
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
                sql = "SELECT MWOID FROM " + MsqlConnection.DbName + ".tblmultipleworkorder where IsDeleted = 0 and MWOID != " + mwoID + " and WCID = '" + WorkCenterID + "' ;";
            }
            else
            {
                sql = "SELECT MWOID FROM " + MsqlConnection.DbName + ".tblmultipleworkorder where IsDeleted = 0 and WCID = '" + WorkCenterID + "' ;";
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
                sql = "SELECT MWOID FROM " + MsqlConnection.DbName + ".tblmultipleworkorder where PlantID = '" + PlantID + "' and ShopID  is null and  CellID  is null and IsDeleted = 0 and MWOID != " + mwoID + " and WCID  is null ;";
            }
            else
            {
                sql = "SELECT MWOID FROM " + MsqlConnection.DbName + ".tblmultipleworkorder where PlantID = '" + PlantID + "' and ShopID  is null and  CellID  is null and IsDeleted = 0 and WCID  is null ;";
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
            var plantdetails = db.tblshops.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID).FirstOrDefault();
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
                    sql = "SELECT MWOID FROM " + MsqlConnection.DbName + ".tblmultipleworkorder where ShopID = '" + ShopID + "' and  CellID is null and IsDeleted = 0 and MWOID != " + mwoID + " and WCID  is null ;";
                }
                else
                {
                    sql = "SELECT MWOID FROM " + MsqlConnection.DbName + ".tblmultipleworkorder where ShopID = '" + ShopID + "' and  CellID  is null and IsDeleted = 0 and WCID  is null ;";
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
            var Celldetails = db.tblcells.Where(m => m.IsDeleted == 0 && m.CellID == CellID).FirstOrDefault();
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
                    sql = "SELECT MWOID FROM " + MsqlConnection.DbName + ".tblmultipleworkorder where CellID = '" + CellID + "' and IsDeleted = 0 and MWOID != " + mwoID + " and WCID is null ;";
                }
                else
                {
                    sql = "SELECT MWOID FROM " + MsqlConnection.DbName + ".tblmultipleworkorder where CellID = '" + CellID + "' and IsDeleted = 0 and WCID is null ;";
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

            var machinedetails = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID).FirstOrDefault();
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
                    sql = "SELECT MWOID FROM " + MsqlConnection.DbName + ".tblmultipleworkorder where IsDeleted = 0 and MWOID != " + mwoID + " and WCID = '" + MachineID + "' ;";
                }
                else
                {
                    sql = "SELECT MWOID FROM " + MsqlConnection.DbName + ".tblmultipleworkorder where IsDeleted = 0 and WCID = '" + MachineID + "' ;";

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

        public JsonResult GetShop(int PlantID)
        {
            var ShopData = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID), "ShopID", "ShopName");
            return Json(ShopData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetCell(int ShopID)
        {
            var CellData = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID), "CellID", "CellName");
            return Json(CellData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetWC_Cell(int CellID)
        {
            var MachineData = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == CellID), "MachineID", "MachineInvNo");
            return Json(MachineData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetWC_Shop(int ShopID)
        {
            var MachineData = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID && m.CellID.Equals(null)), "MachineID", "MachineInvNo");
            return Json(MachineData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ImportEnabledMultiWODetails()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            return View();
        }
        [HttpPost]
        public ActionResult ImportEnabledMultiWODetails(HttpPostedFileBase file, string UploadType)
        {

            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            int UserID = Convert.ToInt32(Session["UserId"]);
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

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
                    excelConnection1.Close();
                    excelConnection.Close();
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

                string Errors = null;
                if (UploadType == "OverWrite") // Accept only New Codes
                {
                    #region
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        string MultiWOName = Convert.ToString(ds.Tables[0].Rows[i][0]);

                        if (MultiWOName != null)
                        {
                            string PlantName = Convert.ToString(ds.Tables[0].Rows[i][1]);
                            int PlantID = Convert.ToInt32(db.tblplants.Where(m => m.PlantName == PlantName).Select(m => m.PlantID).FirstOrDefault());
                            string ShopName = Convert.ToString(ds.Tables[0].Rows[i][2]);
                            int ShopID = Convert.ToInt32(db.tblshops.Where(m => m.ShopName == ShopName).Select(m => m.ShopID).FirstOrDefault());
                            string CellName = Convert.ToString(ds.Tables[0].Rows[i][3]);
                            int CellID = Convert.ToInt32(db.tblcells.Where(m => m.CellName == CellName).Select(m => m.CellID).FirstOrDefault());
                            string WCName = Convert.ToString(ds.Tables[0].Rows[i][4]);
                            int WCID = Convert.ToInt32(db.tblmachinedetails.Where(m => m.MachineDisplayName == WCName).Select(m => m.MachineID).FirstOrDefault());

                            string isEnable = Convert.ToString(ds.Tables[0].Rows[i][5]);
                            if (isEnable.Length > 0 && (isEnable.Trim() == "Y" || isEnable.Trim() == "N"))
                            {
                                isEnable = isEnable.Trim();
                            }
                            else
                            {
                                Errors += "Enable(Y/N) is Null or Improper for MultiWOName: " + MultiWOName + " \n";
                                continue;
                            }
                            if (isEnable == "N")
                            {
                                try
                                {
                                    using (i_facility_shaktiEntities1 dbmwo = new i_facility_shaktiEntities1())
                                    {
                                        var DuplicateData = dbmwo.tblmultipleworkorders.Where(m => m.MultipleWOName == MultiWOName && m.IsDeleted == 0).FirstOrDefault();
                                        if (DuplicateData != null)
                                        {
                                            DuplicateData.IsDeleted = 1;
                                            DuplicateData.ModifiedOn = DateTime.Now;
                                            dbmwo.Entry(DuplicateData).State = EntityState.Modified;
                                            dbmwo.SaveChanges();
                                            continue;
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                }
                            }

                            // Check for Duplicate Dont have to Check for Hierarchy ( Assuming Rational User )
                            if (PlantID != 0 && ShopID != 0 && CellID != 0 && WCID != 0)
                            {
                                var DupMultiWOPData = db.tblmultipleworkorders.Where(m => m.PlantID == PlantID && m.ShopID == null && m.CellID == null).FirstOrDefault();
                                var DupMultiWOSData = db.tblmultipleworkorders.Where(m => m.PlantID == PlantID && m.ShopID == ShopID && m.CellID == null).FirstOrDefault();
                                var DupMultiWOCData = db.tblmultipleworkorders.Where(m => m.PlantID == PlantID && m.ShopID == ShopID && m.CellID == CellID).FirstOrDefault();
                                var DupMultiWOWCData = db.tblmultipleworkorders.Where(m => m.PlantID == PlantID && m.ShopID == null && m.CellID == null && m.WCID == WCID).FirstOrDefault();

                                if (DupMultiWOPData != null || DupMultiWOSData != null || DupMultiWOCData != null || DupMultiWOWCData != null)
                                {
                                    Errors += "Duplicate Entry for MultiWOName: " + MultiWOName + " \n";
                                    continue;
                                }
                                else
                                {
                                    tblmultipleworkorder tmwo = new tblmultipleworkorder();
                                    tmwo.CellID = CellID;
                                    tmwo.CreatedBy = UserID;
                                    tmwo.CreatedOn = DateTime.Now;
                                    tmwo.IsDeleted = 0;
                                    tmwo.IsEnabled = 1;
                                    tmwo.MultipleWODesc = MultiWOName;
                                    tmwo.MultipleWOName = MultiWOName;
                                    tmwo.PlantID = PlantID;
                                    tmwo.ShopID = ShopID;
                                    tmwo.WCID = WCID;
                                    db.tblmultipleworkorders.Add(tmwo);
                                    db.SaveChanges();
                                }
                            }
                            else if (PlantID != 0 && ShopID != 0 && CellID != 0)
                            {
                                var DupMultiWOPData = db.tblmultipleworkorders.Where(m => m.PlantID == PlantID && m.ShopID == null && m.CellID == null).FirstOrDefault();
                                var DupMultiWOSData = db.tblmultipleworkorders.Where(m => m.PlantID == PlantID && m.ShopID == ShopID && m.CellID == null).FirstOrDefault();
                                var DupMultiWOCData = db.tblmultipleworkorders.Where(m => m.PlantID == PlantID && m.ShopID == ShopID && m.CellID == CellID).FirstOrDefault();
                                //var DupMultiWOWCData = db.tblmultipleworkorders.Where(m => m.PlantID == PlantID && m.ShopID == null && m.CellID == null && m.WCID == WCID).FirstOrDefault();

                                if (DupMultiWOPData != null || DupMultiWOSData != null || DupMultiWOCData != null)
                                {
                                    Errors += "Duplicate Entry for MultiWOName: " + MultiWOName + " \n";
                                    continue;
                                }
                                else
                                {
                                    tblmultipleworkorder tmwo = new tblmultipleworkorder();
                                    tmwo.CellID = CellID;
                                    tmwo.CreatedBy = UserID;
                                    tmwo.CreatedOn = DateTime.Now;
                                    tmwo.IsDeleted = 0;
                                    tmwo.IsEnabled = 1;
                                    tmwo.MultipleWODesc = MultiWOName;
                                    tmwo.MultipleWOName = MultiWOName;
                                    tmwo.PlantID = PlantID;
                                    tmwo.ShopID = ShopID;
                                    db.tblmultipleworkorders.Add(tmwo);
                                    db.SaveChanges();
                                }
                            }
                            else if (PlantID != 0 && ShopID != 0)
                            {
                                var DupMultiWOPData = db.tblmultipleworkorders.Where(m => m.PlantID == PlantID && m.ShopID == null && m.CellID == null).FirstOrDefault();
                                var DupMultiWOSData = db.tblmultipleworkorders.Where(m => m.PlantID == PlantID && m.ShopID == ShopID && m.CellID == null).FirstOrDefault();
                                //var DupMultiWOCData = db.tblmultipleworkorders.Where(m => m.PlantID == PlantID && m.ShopID == ShopID && m.CellID == CellID).FirstOrDefault();
                                //var DupMultiWOWCData = db.tblmultipleworkorders.Where(m => m.PlantID == PlantID && m.ShopID == null && m.CellID == null && m.WCID == WCID).FirstOrDefault();

                                if (DupMultiWOPData != null || DupMultiWOSData != null)
                                {
                                    Errors += "Duplicate Entry for MultiWOName: " + MultiWOName + " \n";
                                    continue;
                                }
                                else
                                {
                                    tblmultipleworkorder tmwo = new tblmultipleworkorder();
                                    //tmwo.CellID = CellID;
                                    tmwo.CreatedBy = UserID;
                                    tmwo.CreatedOn = DateTime.Now;
                                    tmwo.IsDeleted = 0;
                                    tmwo.IsEnabled = 1;
                                    tmwo.MultipleWODesc = MultiWOName;
                                    tmwo.MultipleWOName = MultiWOName;
                                    tmwo.PlantID = PlantID;
                                    tmwo.ShopID = ShopID;
                                    db.tblmultipleworkorders.Add(tmwo);
                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                Errors += "Please select Plant & Shop / Cell \n";
                                continue;
                            }
                        }
                        else
                        {
                            Errors += "MultiWO Name Cannot be Null \n";
                            continue;
                        }
                    }
                    #endregion
                }
                if (UploadType == "New") // Delete Duplicate and Insert New. // if not Duplicate insert that
                {
                    #region
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        string MultiWOName = Convert.ToString(ds.Tables[0].Rows[i][0]);

                        if (MultiWOName != null)
                        {
                            string PlantName = Convert.ToString(ds.Tables[0].Rows[i][1]);
                            int PlantID = Convert.ToInt32(db.tblplants.Where(m => m.PlantName == PlantName).Select(m => m.PlantID).FirstOrDefault());
                            string ShopName = Convert.ToString(ds.Tables[0].Rows[i][2]);
                            int ShopID = Convert.ToInt32(db.tblshops.Where(m => m.ShopName == ShopName).Select(m => m.ShopID).FirstOrDefault());
                            string CellName = Convert.ToString(ds.Tables[0].Rows[i][3]);
                            int CellID = Convert.ToInt32(db.tblcells.Where(m => m.CellName == CellName).Select(m => m.CellID).FirstOrDefault());
                            int WCID = Convert.ToInt32(ds.Tables[0].Rows[i][4]);
                            string WCName = Convert.ToString(db.tblmachinedetails.Where(m => m.MachineID == WCID).Select(m => m.MachineDisplayName).FirstOrDefault());

                            string isEnable = Convert.ToString(ds.Tables[0].Rows[i][5]);
                            if (isEnable.Length > 0 && (isEnable.Trim() == "Y" || isEnable.Trim() == "N"))
                            {
                                isEnable = isEnable.Trim();
                            }
                            else
                            {
                                Errors += "Enable(Y/N) is Null or Improper for MultiWOName: " + MultiWOName + " \n";
                                continue;
                            }

                            if (isEnable == "N")
                            {
                                try
                                {
                                    using (i_facility_shaktiEntities1 dbmwo = new i_facility_shaktiEntities1())
                                    {
                                        var DuplicateData = dbmwo.tblmultipleworkorders.Where(m => m.MultipleWOName == MultiWOName && m.IsDeleted == 0).FirstOrDefault();
                                        if (DuplicateData != null)
                                        {
                                            DuplicateData.IsDeleted = 1;
                                            DuplicateData.ModifiedOn = DateTime.Now;
                                            dbmwo.Entry(DuplicateData).State = EntityState.Modified;
                                            dbmwo.SaveChanges();
                                            continue;
                                        }
                                    }
                                }
                                catch (Exception e)
                                {

                                }
                            }

                            // Check for Duplicate Dont have to Check for Hierarchy ( Assuming Rational User )
                            if (PlantID != 0 && ShopID != 0 && CellID != 0 && WCID != 0)
                            {
                                var DupMultiWOPData = db.tblmultipleworkorders.Where(m => m.PlantID == PlantID && m.ShopID == null && m.CellID == null).FirstOrDefault();
                                var DupMultiWOSData = db.tblmultipleworkorders.Where(m => m.PlantID == PlantID && m.ShopID == ShopID && m.CellID == null).FirstOrDefault();
                                var DupMultiWOCData = db.tblmultipleworkorders.Where(m => m.PlantID == PlantID && m.ShopID == ShopID && m.CellID == CellID).FirstOrDefault();
                                var DupMultiWOWCData = db.tblmultipleworkorders.Where(m => m.PlantID == PlantID && m.ShopID == null && m.CellID == null && m.WCID == WCID).FirstOrDefault();

                                if (DupMultiWOPData != null || DupMultiWOSData != null || DupMultiWOCData != null || DupMultiWOWCData != null)
                                {
                                    DupMultiWOWCData.IsDeleted = 1;
                                    DupMultiWOWCData.IsEnabled = 0;
                                    db.Entry(DupMultiWOWCData).State = EntityState.Modified;
                                    db.SaveChanges();

                                    tblmultipleworkorder tmwo = new tblmultipleworkorder();
                                    tmwo.CellID = CellID;
                                    tmwo.CreatedBy = UserID;
                                    tmwo.CreatedOn = DateTime.Now;
                                    tmwo.IsDeleted = 0;
                                    tmwo.IsEnabled = 1;
                                    tmwo.MultipleWODesc = MultiWOName;
                                    tmwo.MultipleWOName = MultiWOName;
                                    tmwo.PlantID = PlantID;
                                    tmwo.ShopID = ShopID;
                                    tmwo.WCID = WCID;
                                    db.tblmultipleworkorders.Add(tmwo);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    tblmultipleworkorder tmwo = new tblmultipleworkorder();
                                    tmwo.CellID = CellID;
                                    tmwo.CreatedBy = UserID;
                                    tmwo.CreatedOn = DateTime.Now;
                                    tmwo.IsDeleted = 0;
                                    tmwo.IsEnabled = 1;
                                    tmwo.MultipleWODesc = MultiWOName;
                                    tmwo.MultipleWOName = MultiWOName;
                                    tmwo.PlantID = PlantID;
                                    tmwo.ShopID = ShopID;
                                    tmwo.WCID = WCID;
                                    db.tblmultipleworkorders.Add(tmwo);
                                    db.SaveChanges();
                                }
                            }
                            else if (PlantID != 0 && ShopID != 0 && CellID != 0)
                            {
                                var DupMultiWOPData = db.tblmultipleworkorders.Where(m => m.PlantID == PlantID && m.ShopID == null && m.CellID == null).FirstOrDefault();
                                var DupMultiWOSData = db.tblmultipleworkorders.Where(m => m.PlantID == PlantID && m.ShopID == ShopID && m.CellID == null).FirstOrDefault();
                                var DupMultiWOCData = db.tblmultipleworkorders.Where(m => m.PlantID == PlantID && m.ShopID == ShopID && m.CellID == CellID).FirstOrDefault();
                                //var DupMultiWOWCData = db.tblmultipleworkorders.Where(m => m.PlantID == PlantID && m.ShopID == null && m.CellID == null && m.WCID == WCID).FirstOrDefault();

                                if (DupMultiWOPData != null || DupMultiWOSData != null || DupMultiWOCData != null)
                                {
                                    DupMultiWOCData.IsDeleted = 1;
                                    DupMultiWOCData.IsEnabled = 0;
                                    db.Entry(DupMultiWOCData).State = EntityState.Modified;
                                    db.SaveChanges();

                                    tblmultipleworkorder tmwo = new tblmultipleworkorder();
                                    tmwo.CellID = CellID;
                                    tmwo.CreatedBy = UserID;
                                    tmwo.CreatedOn = DateTime.Now;
                                    tmwo.IsDeleted = 0;
                                    tmwo.IsEnabled = 1;
                                    tmwo.MultipleWODesc = MultiWOName;
                                    tmwo.MultipleWOName = MultiWOName;
                                    tmwo.PlantID = PlantID;
                                    tmwo.ShopID = ShopID;
                                    db.tblmultipleworkorders.Add(tmwo);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    tblmultipleworkorder tmwo = new tblmultipleworkorder();
                                    tmwo.CellID = CellID;
                                    tmwo.CreatedBy = UserID;
                                    tmwo.CreatedOn = DateTime.Now;
                                    tmwo.IsDeleted = 0;
                                    tmwo.IsEnabled = 1;
                                    tmwo.MultipleWODesc = MultiWOName;
                                    tmwo.MultipleWOName = MultiWOName;
                                    tmwo.PlantID = PlantID;
                                    tmwo.ShopID = ShopID;
                                    db.tblmultipleworkorders.Add(tmwo);
                                    db.SaveChanges();
                                }
                            }
                            else if (PlantID != 0 && ShopID != 0)
                            {
                                var DupMultiWOPData = db.tblmultipleworkorders.Where(m => m.PlantID == PlantID && m.ShopID == null && m.CellID == null).FirstOrDefault();
                                var DupMultiWOSData = db.tblmultipleworkorders.Where(m => m.PlantID == PlantID && m.ShopID == ShopID && m.CellID == null).FirstOrDefault();

                                if (DupMultiWOPData != null || DupMultiWOSData != null)
                                {
                                    DupMultiWOSData.IsDeleted = 1;
                                    DupMultiWOSData.IsEnabled = 0;
                                    db.Entry(DupMultiWOSData).State = EntityState.Modified;
                                    db.SaveChanges();

                                    tblmultipleworkorder tmwo = new tblmultipleworkorder();
                                    tmwo.CreatedBy = UserID;
                                    tmwo.CreatedOn = DateTime.Now;
                                    tmwo.IsDeleted = 0;
                                    tmwo.IsEnabled = 1;
                                    tmwo.MultipleWODesc = MultiWOName;
                                    tmwo.MultipleWOName = MultiWOName;
                                    tmwo.PlantID = PlantID;
                                    tmwo.ShopID = ShopID;
                                    db.tblmultipleworkorders.Add(tmwo);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    tblmultipleworkorder tmwo = new tblmultipleworkorder();
                                    tmwo.CreatedBy = UserID;
                                    tmwo.CreatedOn = DateTime.Now;
                                    tmwo.IsDeleted = 0;
                                    tmwo.IsEnabled = 1;
                                    tmwo.MultipleWODesc = MultiWOName;
                                    tmwo.MultipleWOName = MultiWOName;
                                    tmwo.PlantID = PlantID;
                                    tmwo.ShopID = ShopID;
                                    db.tblmultipleworkorders.Add(tmwo);
                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                Errors += "Please select Plant & Shop / Cell \n";
                                continue;
                            }
                        }
                        else
                        {
                            Errors += "MultiWO Name Cannot be Null \n";
                            continue;
                        }
                    }
                    #endregion
                }
                Session["Errors"] = Errors;
            }
            return View();
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
            String FileDir = @"C:\SRKS_ifacility\ReportsList\" + System.DateTime.Now.ToString("yyyy-MM-dd");
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
            var BreakDownLossesData = db.tblmultipleworkorders.Where(m => m.IsDeleted == 0 && m.IsEnabled == 1).OrderBy(m => m.PlantID).ThenBy(m => m.ShopID).ThenBy(m => m.CellID).ToList();
            int i = 2;
            foreach (var row in BreakDownLossesData)
            {
                int PlantID = Convert.ToInt32(row.PlantID);
                string PlantName = Convert.ToString(db.tblplants.Where(m => m.PlantID == PlantID).Select(m => m.PlantName).FirstOrDefault());
                int ShopID = Convert.ToInt32(row.ShopID);
                string ShopName = Convert.ToString(db.tblshops.Where(m => m.ShopID == ShopID).Select(m => m.ShopName).FirstOrDefault());
                int CellID = Convert.ToInt32(row.CellID);
                string CellName = Convert.ToString(db.tblcells.Where(m => m.CellID == CellID).Select(m => m.CellName).FirstOrDefault());
                int WCID = Convert.ToInt32(row.WCID);
                string WCName = Convert.ToString(db.tblmachinedetails.Where(m => m.MachineID == WCID).Select(m => m.MachineDisplayName).FirstOrDefault());

                worksheet.Cells["A" + i].Value = row.MultipleWOName;
                worksheet.Cells["B" + i].Value = PlantName;
                worksheet.Cells["C" + i].Value = ShopName;
                worksheet.Cells["D" + i].Value = CellName;
                worksheet.Cells["E" + i].Value = WCName;
                worksheet.Cells["F" + i].Value = "Y";
                i++;
            }



            //Now push Every Other possible combinations into Excel (  so that it will be easy for user to do settings)
            var PlantData = db.tblplants.Where(m => m.PlantID == 1).FirstOrDefault();
            var shopDetails = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0).Select(m => m.ShopID).Distinct().ToList();
            foreach (var shopId in shopDetails)
            {
                var MultiwoDetailsForShop = db.tblmultipleworkorders.Where(m => m.ShopID == shopId && m.CellID == null).FirstOrDefault();
                if (MultiwoDetailsForShop != null)
                {
                    continue;
                }
                else
                {
                    var shopData = db.tblshops.Where(m => m.ShopID == shopId).FirstOrDefault();
                    //Insert for this Shop and Check for its child cells
                    worksheet.Cells["A" + i].Value = PlantData.PlantName + "_" + shopData.ShopName;
                    worksheet.Cells["B" + i].Value = PlantData.PlantName;
                    worksheet.Cells["C" + i].Value = shopData.ShopName;
                    i++;

                    var CellDetails = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.ShopID == shopId).Select(m => m.CellID).Distinct().ToList();
                    foreach (var cellId in CellDetails)
                    {
                        var MultiwoDetailsForCell = db.tblmultipleworkorders.Where(m => m.ShopID == shopId && m.CellID == cellId && m.WCID == null).FirstOrDefault();
                        if (MultiwoDetailsForCell != null)
                        {
                            continue;
                        }
                        else
                        {
                            //Insert for this Cell and Check for its child Macs
                            var CellData = db.tblcells.Where(m => m.ShopID == shopId && m.CellID == cellId).FirstOrDefault();
                            //Insert for this Shop and Check for its child cells
                            worksheet.Cells["A" + i].Value = PlantData.PlantName + "_" + shopData.ShopName;
                            worksheet.Cells["B" + i].Value = PlantData.PlantName;
                            worksheet.Cells["C" + i].Value = shopData.ShopName;
                            worksheet.Cells["D" + i].Value = CellData.CellName;
                            i++;

                            var WCDetails = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.ShopID == shopId && m.CellID == cellId && m.MachineID == null).Select(m => m.CellID).Distinct().ToList();
                            foreach (var wcId in WCDetails)
                            {
                                var MultiwoDetailsForWC = db.tblmultipleworkorders.Where(m => m.ShopID == shopId && m.CellID == cellId && m.WCID == wcId).FirstOrDefault();
                                if (MultiwoDetailsForWC != null)
                                {
                                    continue;
                                }
                                else
                                {
                                    //Insert for this WorkCenter
                                    var WCData = db.tblmachinedetails.Where(m => m.ShopID == shopId && m.CellID == cellId && m.MachineID == wcId).FirstOrDefault();
                                    worksheet.Cells["A" + i].Value = PlantData.PlantName + "_" + shopData.ShopName;
                                    worksheet.Cells["B" + i].Value = PlantData.PlantName;
                                    worksheet.Cells["C" + i].Value = shopData.ShopName;
                                    worksheet.Cells["D" + i].Value = CellData.CellName;
                                    worksheet.Cells["E" + i].Value = WCData.MachineDisplayName;
                                    i++;
                                }
                            }
                        }
                    }
                }
            }



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
            #endregion
            return RedirectToAction("Index");
        }
    }
}