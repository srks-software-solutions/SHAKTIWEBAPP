using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


using System.Data.Entity;
using System.Data;
using System.IO;
using System.Xml;
using System.Configuration;
using System.Data.OleDb;
using SRKSDemo.Server_Model;
using OfficeOpenXml;
//using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using SRKSDemo;






namespace SRKSDemo.Controllers
{
    public class PMSMasterController : Controller
    {

        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
        string dbName = ConfigurationManager.AppSettings["dbName"];

        // GET: PMSMaster
        public ActionResult Index()
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
            var Escalationdata = db.TblPMSNotification_Master.Where(m => m.IsDeleted == 0).ToList();
            return View(Escalationdata);
        }

        public ActionResult Create()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();

            ViewBag.Plant = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewBag.Shop = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewBag.Cell = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewBag.WorkCenter = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineInvNo");
            return View();
        }
        [HttpPost]
        public ActionResult Create(TblPMSNotification_Master tee, int hdnSaveNContinue = 0)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
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
           
            string Plantidstring = Convert.ToString(tee.PlantID);
            string Shopidstring = Convert.ToString(tee.ShopID);
            string Cellidstring = Convert.ToString(tee.CellID);
            string WorkCenteridstring = Convert.ToString(tee.WorkCenterID);
            string days = tee.NoOfDaysPrior;
            string cclist =tee.CcList;
            string tolist = tee.ToList;
            string contactlist = tee.SMSContactList;
            int Frequency = Convert.ToInt32(tee.Frequency);
            string Unit = tee.Unit;
            if (hdnSaveNContinue == 0)
            {

                tee.CreatedBy = UserID;
                tee.CreatedOn = DateTime.Now;
                tee.IsDeleted = 0;

                db.TblPMSNotification_Master.Add(tee);
                db.SaveChanges();

            }
            return RedirectToAction("Index");
           
        }

        public ActionResult Edit(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            TblPMSNotification_Master tee = db.TblPMSNotification_Master.Find(id);
            int plantid = Convert.ToInt32(tee.PlantID);
            int shopid = Convert.ToInt32(tee.ShopID);
            int cellid = Convert.ToInt32(tee.CellID);
            int machineid = Convert.ToInt32(tee.WorkCenterID);

            ViewBag.Plant = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", tee.PlantID);
            ViewBag.Shop = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == plantid), "ShopID", "ShopName", tee.ShopID);
            ViewBag.Cell = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.ShopID == shopid), "CellID", "CellName", tee.CellID);
            //bool tick = doesThisMachineHasCELL(Convert.ToInt32(tee.WorkCenterID));
            if (tee.CellID != null )
            {
                ViewBag.WorkCenter = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == cellid), "MachineID", "MachineDisplayName", tee.WorkCenterID);
            }
            else
            {
                ViewBag.WorkCenter = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == shopid), "MachineID", "MachineDisplayName", tee.WorkCenterID);
            }

            return View(tee);
        }
        [HttpPost]
        public ActionResult Edit(TblPMSNotification_Master tee)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
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
          
            string Plantidstring = Convert.ToString(tee.PlantID);
            string Shopidstring = Convert.ToString(tee.ShopID);
            string Cellidstring = Convert.ToString(tee.CellID);
            string WorkCenteridstring = Convert.ToString(tee.WorkCenterID);
            string days= tee.NoOfDaysPrior;
            string cclist = tee.CcList;
            string tolist = tee.ToList;
            string contactlist = tee.SMSContactList;


               
                tee.ModifiedBy = UserID;
                tee.ModifiedOn =(DateTime) DateTime.Now;
                tee.IsDeleted = 0;

                db.Entry(tee).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
           
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

            int UserID1 = id;
            //ViewBag.IsConfigMenu = 0;
            TblPMSNotification_Master tee = db.TblPMSNotification_Master.Find(id);
            tee.DeletedDate = DateTime.Now;
            tee.IsDeleted = 1;
            tee.ModifiedBy = UserID1;
            tee.ModifiedOn = System.DateTime.Now;
            //start Logging
            int UserID = Convert.ToInt32(Session["UserId"]);
            String Username = Session["Username"].ToString();
            //string CompleteModificationdetail = "Deleted Parts/Item";
            //ActiveLogStorage Obj = new ActiveLogStorage();
            //Obj.SaveActiveLog(Action, Controller, Username, UserID, CompleteModificationdetail);
            //End
            db.Entry(tee).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
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
            var MachineData = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == CellID && m.IsNormalWC == 0), "MachineID", "MachineDisplayName");
            return Json(MachineData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetWC_Shop(int ShopID)
        {
            var MachineData = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID && m.CellID.Equals(null) && m.IsNormalWC == 0), "MachineID", "MachineInvNo");
            return Json(MachineData, JsonRequestBehavior.AllowGet);
        }

      

    }
}