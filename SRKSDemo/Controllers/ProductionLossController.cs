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
    public class ProductionLossController : Controller
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();

        public ActionResult Index()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.UserName = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            ViewData["Level1"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.MessageType != "PM" ), "LossCodeID", "LossCodeDesc");/*&& m.MessageType != "BREAKDOWN"*/
            ViewData["Level2"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 29 && m.MessageType != "PM"), "LossCodeID", "LossCodeDesc");
            ViewData["Level3"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 9 && m.MessageType != "PM"), "LossCodeID", "LossCodeDesc");
            ProductionLossModel pa = new ProductionLossModel();
            tbllossescode pe = new tbllossescode();
            pa.ProductionLoss = pe;
            pa.ProductionLossList = db.tbllossescodes.Where(m => m.IsDeleted == 0).ToList();//Dont take Any Loss to view, as we will display data based on Department and Category.
            var query = db.tbllossescodes.Where(M => M.IsDeleted == 0).ToList();
                       
            pa.ProductionLossList = query.ToList();

            return View(pa);
        }

     
        [HttpPost]
        public ActionResult Create(ProductionLossModel objLossCode, int Level1 = 0, int Level2 = 0)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            int UserID = Convert.ToInt32(Session["UserId"]);
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            objLossCode.ProductionLoss.CreatedBy = UserID;
            objLossCode.ProductionLoss.CreatedOn = DateTime.Now;
            objLossCode.ProductionLoss.IsDeleted = 0;
            //objLossCode.ProductionLoss.ServerTabCheck = 1;
            //objLossCode.ProductionLoss.ServerTabFlagSync = 0;
            //Check duplicate entry
            var losscodelevel = objLossCode.ProductionLoss.LossCodesLevel;
            var losscode = objLossCode.ProductionLoss.LossCode;
            if(losscodelevel == 1)
            {
                var DuplicateEntry = db.tbllossescodes.Where(m => m.LossCode == losscode && m.IsDeleted == 0).ToList();
                if (DuplicateEntry != null)

                {
                    db.tbllossescodes.Add(objLossCode.ProductionLoss);
                    db.SaveChanges();
                    RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "Losscode already exists";
                }
            }
            else if(losscodelevel == 2)
            {
               
                objLossCode.ProductionLoss.LossCodesLevel1ID = Level1;

                var duplosscode = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCode == objLossCode.ProductionLoss.LossCode && m.LossCodesLevel1ID == objLossCode.ProductionLoss.LossCodesLevel1ID).ToList();
                if (duplosscode.Count==0)
                {
                    //Here check if Level1 code is used in tbllossofentry, if so then create new copy of it with Different LossID.
                    var lossOfEntryData = db.tbllossofentries.Where(m => m.MessageCodeID == Level1).ToList();
                    if (lossOfEntryData.Count > 0)
                    {
                        var lossPrvData = db.tbllossescodes.Where(m => m.LossCodeID == Level1 && m.IsDeleted == 0).FirstOrDefault();

                        tbllossescode tlcNewPrvLevel = new tbllossescode();
                        tlcNewPrvLevel.ContributeTo = lossPrvData.ContributeTo;
                        tlcNewPrvLevel.CreatedBy = lossPrvData.CreatedBy;
                        tlcNewPrvLevel.CreatedOn = lossPrvData.CreatedOn;
                        tlcNewPrvLevel.IsDeleted = 0;
                        tlcNewPrvLevel.LossCode = lossPrvData.LossCode;
                        tlcNewPrvLevel.LossCodeDesc = lossPrvData.LossCodeDesc;
                        tlcNewPrvLevel.LossCodesLevel = lossPrvData.LossCodesLevel;
                        tlcNewPrvLevel.LossCodesLevel1ID = lossPrvData.LossCodesLevel1ID;
                        tlcNewPrvLevel.LossCodesLevel2ID = lossPrvData.LossCodesLevel2ID;
                        tlcNewPrvLevel.MessageType = lossPrvData.MessageType;
                        tlcNewPrvLevel.ModifiedBy = lossPrvData.ModifiedBy;
                        tlcNewPrvLevel.ModifiedOn = lossPrvData.ModifiedOn;
                        //tlcNewPrvLevel.ServerTabCheck = 1;
                        //tlcNewPrvLevel.ServerTabFlagSync = 0;

                        db.tbllossescodes.Add(tlcNewPrvLevel);
                        db.SaveChanges();

                        //Delete the Old one.
                        lossPrvData.IsDeleted = 1;
                        lossPrvData.ModifiedOn = DateTime.Now;
                        lossPrvData.ModifiedBy = UserID;
                        db.Entry(lossPrvData).State = EntityState.Modified;
                        db.SaveChanges();

                        //Give new Level1 LossCodeID to 2nd level code.
                        int Level1LossCodeID = tlcNewPrvLevel.LossCodeID;
                        objLossCode.ProductionLoss.LossCodesLevel1ID = Level1LossCodeID;

                    }
                    db.tbllossescodes.Add(objLossCode.ProductionLoss);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "Loss code already Exist.";
                    return View(objLossCode);
                }
            }
            else if(losscodelevel == 3)
            {
                objLossCode.ProductionLoss.LossCodesLevel1ID = Level1;
                objLossCode.ProductionLoss.LossCodesLevel2ID = Level2;
                var duplosscode = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCode == objLossCode.ProductionLoss.LossCode && m.LossCodesLevel1ID == objLossCode.ProductionLoss.LossCodesLevel1ID && m.LossCodesLevel2ID == objLossCode.ProductionLoss.LossCodesLevel2ID).ToList();
                if (duplosscode.Count == 0)
                {
                    //Here check if Level1 code is used in tbllossofentry, if so then create new copy of it with Different LossID.
                    var lossOfEntryData = db.tbllossofentries.Where(m => m.MessageCodeID == Level2).ToList();
                    if (lossOfEntryData.Count > 0)
                    {
                        var lossPrvData = db.tbllossescodes.Where(m => m.LossCodeID == Level2 && m.IsDeleted == 0).FirstOrDefault();

                        tbllossescode tlcNewPrvLevel = new tbllossescode();
                        tlcNewPrvLevel.ContributeTo = lossPrvData.ContributeTo;
                        tlcNewPrvLevel.CreatedBy = lossPrvData.CreatedBy;
                        tlcNewPrvLevel.CreatedOn = lossPrvData.CreatedOn;
                        tlcNewPrvLevel.IsDeleted = 0;
                        tlcNewPrvLevel.LossCode = lossPrvData.LossCode;
                        tlcNewPrvLevel.LossCodeDesc = lossPrvData.LossCodeDesc;
                        tlcNewPrvLevel.LossCodesLevel = lossPrvData.LossCodesLevel;
                        tlcNewPrvLevel.LossCodesLevel1ID = lossPrvData.LossCodesLevel1ID;
                        tlcNewPrvLevel.LossCodesLevel2ID = lossPrvData.LossCodesLevel2ID;
                        tlcNewPrvLevel.MessageType = lossPrvData.MessageType;
                        tlcNewPrvLevel.ModifiedBy = lossPrvData.ModifiedBy;
                        tlcNewPrvLevel.ModifiedOn = lossPrvData.ModifiedOn;
                        //tlcNewPrvLevel.ServerTabCheck = 1;
                        //tlcNewPrvLevel.ServerTabFlagSync = 0;

                        db.tbllossescodes.Add(tlcNewPrvLevel);
                        db.SaveChanges();

                        //Delete the Old one.
                        lossPrvData.IsDeleted = 1;
                        lossPrvData.ModifiedOn = DateTime.Now;
                        lossPrvData.ModifiedBy = UserID;
                        db.Entry(lossPrvData).State = EntityState.Modified;
                        db.SaveChanges();

                        //Give new Level1 LossCodeID to 2nd level code.
                        int Level2LossCodeID = tlcNewPrvLevel.LossCodeID;
                        objLossCode.ProductionLoss.LossCodesLevel2ID = Level2LossCodeID;

                    }

                    db.tbllossescodes.Add(objLossCode.ProductionLoss);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "Loss code already Exist.";
                    return View(objLossCode);
                }
            }

            return RedirectToAction("Index");
        }



        public JsonResult FetchLevel2Losscodes(int LevelId)
        {
            var results = (from row in db.tbllossescodes
                           where row.IsDeleted == 0 && row.LossCodesLevel1ID == LevelId
                           select new { Value = row.LossCodeID, Text = row.LossCode });
            return Json(results, JsonRequestBehavior.AllowGet);
        }


        public JsonResult FetchLevel2LosscodesEdit(int LevelId)
        {
            var results = (from row in db.tbllossescodes
                           where row.IsDeleted == 0 && row.LossCodeID == LevelId
                           select new { Value = row.LossCodeID, Text = row.LossCode });
            return Json(results, JsonRequestBehavior.AllowGet);
        }

        public JsonResult FetchLevel1CodesForedit(int PID)
        {
            var LevelResult = (from row in db.tbllossescodes
                               where row.IsDeleted == 0 && row.LossCodeID == PID
                               select new { Value = row.LossCodeID, Text = row.LossCode });
            return Json(LevelResult, JsonRequestBehavior.AllowGet);
        }
       

        public JsonResult GetProductionLossById(int PRDRLOID)
        {
            var Data = db.tbllossescodes.Where(m => m.LossCodeID == PRDRLOID).Select(m => new { Lcode = m.LossCode, Lcodedesc = m.LossCodeDesc,  messagetype= m.MessageType,LocodeLevel1id=m.LossCodesLevel1ID,LocdeLevel2Id=m.LossCodesLevel2ID, Lcodelevel = m.LossCodesLevel, Contributesto = m.ContributeTo , LossCodeID = m.LossCodeID});
            return Json(Data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBreakdowns(string  BREAKDOWN)
        {
            var ShopData = (from row in db.tbllossescodes
                            where row.IsDeleted == 0 && row.MessageType == "BREAKDOWN" && row.LossCodesLevel==1
                            select new { Value = row.LossCodeID, Text = row.LossCode });
            return Json(ShopData, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBreakdownsLevel2(string BREAKDOWN)
        {
            var ShopData = (from row in db.tbllossescodes
                            where row.IsDeleted == 0 && row.MessageType == "BREAKDOWN" && row.LossCodesLevel == 2
                            select new { Value = row.LossCodeID, Text = row.LossCode });
            return Json(ShopData, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Edit(ProductionLossModel tlc,int Id=0, int Level1 = 0, int Level2 = 0)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            int UserID = Convert.ToInt32(Session["UserId"]);
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            if (Level1 == 0 && Level2==0)
            {
                tlc.ProductionLoss.LossCodesLevel = 1;
            }
            else if (Level1 != 0 && Level2 == 0)
            {
                tlc.ProductionLoss.LossCodesLevel = 2;
                tlc.ProductionLoss.LossCodesLevel1ID = Level1;
            }
            else if (Level1 != 0 && Level2 != 0)
            {
                tlc.ProductionLoss.LossCodesLevel = 3;
                tlc.ProductionLoss.LossCodesLevel1ID = Level1;
                tlc.ProductionLoss.LossCodesLevel2ID = Level2;
            }
            

            tlc.ProductionLoss.ModifiedBy = UserID;
            tlc.ProductionLoss.ModifiedOn = DateTime.Now;
            //tlc.ProductionLoss.ServerTabFlagSync = 1;
            //tlc.ProductionLoss.ServerTabCheck = 2;

            if (Convert.ToInt16(tlc.ProductionLoss.LossCodesLevel) == 1)
            {
                var duplosscode = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCode == tlc.ProductionLoss.LossCode && m.LossCodeID != tlc.ProductionLoss.LossCodeID).ToList();
                if (duplosscode.Count == 0)
                {
                    
                    db.Entry(tlc.ProductionLoss).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "Loss code already Exist.";
                    ViewData["Level1"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.LossCode != "999" && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "LossCodeID", "LossCodeDesc", tlc.ProductionLoss.LossCodesLevel1ID);
                    ViewData["Level2"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 2 && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "LossCodeID", "LossCodeDesc", tlc.ProductionLoss.LossCodesLevel2ID);
                    ViewBag.radioselected = tlc.ProductionLoss.LossCodesLevel;

                    return View(tlc);
                }
            }

            if (Convert.ToInt16(tlc.ProductionLoss.LossCodesLevel) == 2)
            {
                tlc.ProductionLoss.LossCodesLevel1ID = Level1;
                var duplosscode = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCode == tlc.ProductionLoss.LossCode && m.LossCodeID != tlc.ProductionLoss.LossCodeID && m.LossCodesLevel1ID == tlc.ProductionLoss.LossCodesLevel1ID).ToList();
                if (duplosscode.Count == 0)
                {
                    db.Entry(tlc.ProductionLoss).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "Loss code already Exist.";
                    ViewData["Level1"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.LossCode != "999" && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "LossCodeID", "LossCodeDesc", tlc.ProductionLoss.LossCodesLevel1ID);
                    ViewData["Level2"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 2 && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "LossCodeID", "LossCodeDesc", tlc.ProductionLoss.LossCodesLevel2ID);
                    ViewBag.radioselected = tlc.ProductionLoss.LossCodesLevel;

                    return View(tlc);
                }
            }
            if (Convert.ToInt16(tlc.ProductionLoss.LossCodesLevel) == 3)
            {
                tlc.ProductionLoss.LossCodesLevel1ID = Level1;
                tlc.ProductionLoss.LossCodesLevel2ID = Level2;
                var duplosscode = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCode == tlc.ProductionLoss.LossCode && m.LossCodeID != tlc.ProductionLoss.LossCodeID && m.LossCodesLevel1ID == tlc.ProductionLoss.LossCodesLevel1ID && m.LossCodesLevel2ID == tlc.ProductionLoss.LossCodesLevel2ID).ToList();
                if (duplosscode.Count == 0)
                {
                    db.Entry(tlc.ProductionLoss).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "Loss code already Exist.";
                    ViewData["Level1"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.LossCode != "999" && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "LossCodeID", "LossCodeDesc", tlc.ProductionLoss.LossCodesLevel1ID);
                    ViewData["Level2"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 2 && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "LossCodeID", "LossCodeDesc", tlc.ProductionLoss.LossCodesLevel2ID);
                    ViewBag.radioselected = tlc.ProductionLoss.LossCodesLevel;

                    return View(tlc);
                }
            }
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

            var details = db.tbllossescodes.Find(id);
            int lossLevel = details.LossCodesLevel;
            int UserID = Convert.ToInt32(Session["UserId"]);
            if (lossLevel == 1)
            {
                var item = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel1ID == id).ToList();
                foreach (var it in item)
                {
                    //it.ServerTabFlagSync = 1;
                    //it.ServerTabCheck = 2;
                    it.IsDeleted = 1;
                    it.ModifiedBy = UserID;
                    it.ModifiedOn = System.DateTime.Now;
                    db.SaveChanges();
                }
            }
            else if (lossLevel == 2)
            {
                var item = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel2ID == id).ToList();
                foreach (var it in item)
                {
                    //it.ServerTabFlagSync = 1;
                    //it.ServerTabCheck = 2;
                    it.IsDeleted = 1;
                    it.ModifiedBy = UserID;
                    it.ModifiedOn = System.DateTime.Now;
                    db.SaveChanges();
                }
            }

            
            int UserID1 = id;
            //ViewBag.IsConfigMenu = 0;
            tbllossescode tlc = db.tbllossescodes.Find(id);
            //tlc.ServerTabFlagSync = 1;
            //tlc.ServerTabCheck = 2;
            tlc.IsDeleted = 1;
            tlc.ModifiedBy = UserID;
            tlc.ModifiedOn = System.DateTime.Now;
            //start Logging

            String Username = Session["Username"].ToString();
            //string CompleteModificationdetail = "Deleted Parts/Item";
            //ActiveLogStorage Obj = new ActiveLogStorage();
            //Obj.SaveActiveLog(Action, Controller, Username, UserID, CompleteModificationdetail);
            //End
            db.Entry(tlc).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

    }
}