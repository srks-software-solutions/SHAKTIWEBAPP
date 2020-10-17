using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Globalization;
using SRKSDemo.Server_Model;

namespace SRKSDemo.Controllers
{
    public class AutomaticEmailReportController : Controller
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
        public ActionResult Index()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            Session["Error"] = null;
            var Escalationdata = db.tbl_autoreportsetting.Where(m => m.IsDeleted == 0).ToList();
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

            ViewBag.RL1 = new SelectList(db.tbl_reportmaster.Where(m => m.IsDeleted == 0), "ReportID", "ReportDispName");
            ViewBag.RL2 = new SelectList(db.tbl_autoreportbasedon.Where(m => m.IsDeleted == 0), "BasedOnID", "BasedOn");
            ViewBag.RL3 = new SelectList(db.tbl_autoreporttime.Where(m => m.IsDeleted == 0), "AutoReportTimeID", "AutoReportTime");
            ViewBag.Plant = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewBag.Shop = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 1), "ShopID", "ShopName");
            ViewBag.Cell = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewBag.WorkCenter = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.PlantID == 999), "MachineID", "MachineInvNo");

            return View();
        }
        [HttpPost]
        public ActionResult Create(tbl_autoreportsetting tee)
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
            string RL1 = Convert.ToString(tee.ReportID);
            string RL2 = Convert.ToString(tee.BasedOn);
            string RL3 = Convert.ToString(tee.AutoReportTimeID);
            string ValidEscalation = null;

            //ValidEscalation = IsItValidEscalation(RL1, RL2, RL3);


            if (tee.AutoReportTimeID == 1) //day
            {
                tee.NextRunDate = DateTime.Now.AddDays(1);
            }
            else if (tee.AutoReportTimeID == 2) //week
            {
                DateTime begining, end;
                GetWeek(DateTime.Now, new CultureInfo("fr-FR"), out begining, out end);
                tee.NextRunDate = end.AddDays(1);
            }
            else if (tee.AutoReportTimeID == 3) //month
            {
                DateTime Temp2 = new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(1).Month, 01, 00, 00, 01);
                tee.NextRunDate = Temp2;
            }
            else if (tee.AutoReportTimeID == 4) //year
            {
                DateTime Temp2 = new DateTime(DateTime.Now.AddYears(1).Year, 01, 01, 00, 00, 01);
                tee.NextRunDate = Temp2;
            }

            var DupData = db.tbl_autoreportsetting.Where(m =>m.IsDeleted==0 && m.ReportID == tee.ReportID && m.BasedOn == tee.BasedOn && m.AutoReportTimeID == tee.AutoReportTimeID && m.PlantID == tee.PlantID && m.ShopID == tee.ShopID && m.CellID == tee.CellID && m.MachineID == tee.MachineID).FirstOrDefault();
            if (DupData != null)
            {
                ValidEscalation += " Duplicate Entry ";
                TempData["toaster_error"] = ValidEscalation;
            }
            if (ValidEscalation == null)
            {
                tee.CreatedBy = UserID;
                tee.CreatedOn = DateTime.Now;
                tee.IsDeleted = 0;

                db.tbl_autoreportsetting.Add(tee);
                db.SaveChanges();
            }
            else
            {
                Session["Error"] = ValidEscalation;
                TempData["toaster_error"] = ValidEscalation;
                ViewBag.RL1 = new SelectList(db.tbl_reportmaster.Where(m => m.IsDeleted == 0), "ReportID", "ReportDispName", tee.ReportID);
                ViewBag.RL2 = new SelectList(db.tbl_autoreportbasedon.Where(m => m.IsDeleted == 0), "BasedOnID", "BasedOn", tee.BasedOn);
                ViewBag.RL3 = new SelectList(db.tbl_autoreporttime.Where(m => m.IsDeleted == 0), "AutoReportTimeID", "AutoReportTime", tee.AutoReportTimeID);

                ViewBag.Plant = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", tee.PlantID);
                ViewBag.Shop = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == tee.PlantID), "ShopID", "ShopName", tee.ShopID);
                ViewBag.Cell = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == tee.ShopID), "CellID", "CellName", tee.CellID);
                ViewBag.WorkCenter = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.PlantID == tee.PlantID && m.ShopID == tee.ShopID && m.CellID == tee.CellID), "MachineID", "MachineDisplayName", tee.MachineID);

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
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            tbl_autoreportsetting tee = db.tbl_autoreportsetting.Find(id);
            ViewBag.RL1 = new SelectList(db.tbl_reportmaster.Where(m => m.IsDeleted == 0), "ReportID", "ReportDispName", tee.ReportID);
            ViewBag.RL2 = new SelectList(db.tbl_autoreportbasedon.Where(m => m.IsDeleted == 0), "BasedOnID", "BasedOn", tee.BasedOn);
            ViewBag.RL3 = new SelectList(db.tbl_autoreporttime.Where(m => m.IsDeleted == 0), "AutoReportTimeID", "AutoReportTime", tee.AutoReportTimeID);
            ViewBag.Plant = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", tee.PlantID);
            ViewBag.Shop = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == tee.PlantID), "ShopID", "ShopName", tee.ShopID);
            ViewBag.Cell = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.ShopID == tee.ShopID), "CellID", "CellName", tee.CellID);
            ViewBag.WorkCenter = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == tee.PlantID && m.ShopID == tee.ShopID && m.CellID == tee.CellID), "MachineID", "MachineDisplayName", tee.MachineID);
            return View(tee);
        }
        [HttpPost]
        public ActionResult Edit(tbl_autoreportsetting tee)
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
            string RL1 = Convert.ToString(tee.ReportID);
            string RL2 = Convert.ToString(tee.BasedOn);
            string RL3 = Convert.ToString(tee.AutoReportTimeID);
            string ValidEscalation = null;

            //ValidEscalation = IsItValidEscalation(RL1, RL2, RL3);

            if (tee.AutoReportTimeID == 1) //day
            {
                tee.NextRunDate = DateTime.Now.AddDays(1);
            }
            else if (tee.AutoReportTimeID == 2) //week
            {
                DateTime begining, end;
                GetWeek(DateTime.Now, new CultureInfo("fr-FR"), out begining, out end);
                tee.NextRunDate = end.AddDays(1);
            }
            else if (tee.AutoReportTimeID == 3) //month
            {
                DateTime Temp2 = new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(1).Month, 01, 00, 00, 01);
                tee.NextRunDate = Temp2;
            }
            else if (tee.AutoReportTimeID == 4) //year
            {
                DateTime Temp2 = new DateTime(DateTime.Now.AddYears(1).Year, 01, 01, 00, 00, 01);
                tee.NextRunDate = Temp2;
            }

            var DupData = db.tbl_autoreportsetting.Where(m =>m.IsDeleted==0 && m.AutoReportID != tee.AutoReportID && m.ReportID == tee.ReportID && m.BasedOn == tee.BasedOn && m.AutoReportTimeID == tee.AutoReportTimeID && m.PlantID == tee.PlantID && m.ShopID == tee.ShopID && m.CellID == tee.CellID && m.MachineID == tee.MachineID).FirstOrDefault();
            if (DupData != null)
            {
                ValidEscalation += " Duplicate Entry ";
                TempData["toaster_error"] = ValidEscalation;
            }

            if (ValidEscalation == null)
            {
                tee.ModifiedBy = UserID;
                tee.ModifiedOn = DateTime.Now;
                tee.IsDeleted = 0;
                db.Entry(tee).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                Session["Error"] = ValidEscalation;
                TempData["toaster_error"] = ValidEscalation;
                ViewBag.RL1 = new SelectList(db.tbl_reportmaster.Where(m => m.IsDeleted == 0), "ReportID", "ReportDispName", tee.ReportID);
                ViewBag.RL2 = new SelectList(db.tbl_autoreportbasedon.Where(m => m.IsDeleted == 0), "BasedOnID", "BasedOn", tee.BasedOn);
                ViewBag.RL3 = new SelectList(db.tbl_autoreporttime.Where(m => m.IsDeleted == 0), "AutoReportTimeID", "AutoReportTime", tee.AutoReportTimeID);

                ViewBag.Plant = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", tee.PlantID);
                ViewBag.Shop = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == tee.PlantID), "ShopID", "ShopName", tee.ShopID);
                ViewBag.Cell = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == tee.ShopID), "CellID", "CellName", tee.CellID);
                ViewBag.WorkCenter = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.PlantID == tee.PlantID && m.ShopID == tee.ShopID && m.CellID == tee.CellID), "MachineID", "MachineDisplayName", tee.MachineID);

                return View(tee);
            }

            return RedirectToAction("Index");
        }

        public ActionResult Copy(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            tbl_autoreportsetting tee = db.tbl_autoreportsetting.Find(id);

            ViewBag.RL1 = new SelectList(db.tbl_reportmaster.Where(m => m.IsDeleted == 0), "ReportID", "ReportDispName", tee.ReportID);
            ViewBag.RL2 = new SelectList(db.tbl_autoreportbasedon.Where(m => m.IsDeleted == 0), "BasedOnID", "BasedOn", tee.BasedOn);
            ViewBag.RL3 = new SelectList(db.tbl_autoreporttime.Where(m => m.IsDeleted == 0), "AutoReportTimeID", "AutoReportTime", tee.AutoReportTimeID);

            ViewBag.Plant = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", tee.PlantID);
            ViewBag.Shop = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 1), "ShopID", "ShopName", tee.ShopID);
            ViewBag.Cell = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.ShopID == tee.ShopID), "CellID", "CellName", tee.CellID);
            ViewBag.WorkCenter = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == tee.PlantID && m.ShopID == tee.ShopID && m.CellID == tee.CellID), "MachineID", "MachineDisplayName", tee.MachineID);

            return View(tee);
        }
        [HttpPost]
        public ActionResult Copy(tbl_autoreportsetting tee)
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

            //Report validation

            string RL1 = Convert.ToString(tee.ReportID);
            string RL2 = Convert.ToString(tee.BasedOn);
            string RL3 = Convert.ToString(tee.AutoReportTimeID);
            string ValidEscalation = null;

            //ValidEscalation = IsItValidEscalation(RL1, RL2, RL3);

            if (tee.AutoReportTimeID == 1) //day
            {
                tee.NextRunDate = DateTime.Now.AddDays(1);
            }
            else if (tee.AutoReportTimeID == 2) //week
            {
                DateTime begining, end;
                GetWeek(DateTime.Now, new CultureInfo("fr-FR"), out begining, out end);
                tee.NextRunDate = end.AddDays(1);
            }
            else if (tee.AutoReportTimeID == 3) //month
            {
                DateTime Temp2 = new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(1).Month, 01, 00, 00, 01);
                tee.NextRunDate = Temp2;
            }
            else if (tee.AutoReportTimeID == 4) //year
            {
                DateTime Temp2 = new DateTime(DateTime.Now.AddYears(1).Year, 01, 01, 00, 00, 01);
                tee.NextRunDate = Temp2;
            }

            var DupData = db.tbl_autoreportsetting.Where(m =>m.IsDeleted==0 && m.ReportID == tee.ReportID && m.BasedOn == tee.BasedOn && m.AutoReportTimeID == tee.AutoReportTimeID && m.PlantID == tee.PlantID && m.ShopID == tee.ShopID && m.CellID == tee.CellID && m.MachineID == tee.MachineID).FirstOrDefault();
            if (DupData != null)
            {
                ValidEscalation += " Duplicate Entry ";
                TempData["toaster_error"] = ValidEscalation;
            }

            if (ValidEscalation == null)
            {
                tee.CreatedBy = UserID;
                tee.CreatedOn = DateTime.Now;
                tee.IsDeleted = 0;

                db.tbl_autoreportsetting.Add(tee);
                db.SaveChanges();
            }
            else
            {
                Session["Error"] = ValidEscalation;
                TempData["toaster_error"] = ValidEscalation;
                ViewBag.RL1 = new SelectList(db.tbl_reportmaster.Where(m => m.IsDeleted == 0), "ReportID", "ReportDispName", tee.ReportID);
                ViewBag.RL2 = new SelectList(db.tbl_autoreportbasedon.Where(m => m.IsDeleted == 0), "BasedOnID", "BasedOn", tee.BasedOn);
                ViewBag.RL3 = new SelectList(db.tbl_autoreporttime.Where(m => m.IsDeleted == 0), "AutoReportTimeID", "AutoReportTime", tee.AutoReportTimeID);

                ViewBag.Plant = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", tee.PlantID);
                ViewBag.Shop = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 1), "ShopID", "ShopName", tee.ShopID);
                ViewBag.Cell = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.ShopID == tee.ShopID), "CellID", "CellName", tee.CellID);
                ViewBag.WorkCenter = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == tee.PlantID && m.ShopID == tee.ShopID && m.CellID == tee.CellID), "MachineID", "MachineDisplayName", tee.MachineID);

                return View(tee);
            }
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
            tbl_autoreportsetting tee = db.tbl_autoreportsetting.Find(id);
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

        public string IsItValidEscalation(string ReportID, string BasedOn, string AutoReportTimeID)
        {
            String msg = null;
            int RepID = Convert.ToInt32(ReportID);
            int BasedOnID = Convert.ToInt32(BasedOn);
            int ReportTimeID = Convert.ToInt32(AutoReportTimeID);
            var GetExistReportSet = db.tbl_autoreportsetting.Where(m => m.IsDeleted == 0 && m.ReportID == RepID && m.BasedOn == BasedOnID && m.AutoReportTimeID == ReportTimeID).ToList();
            int count = GetExistReportSet.Count;
            if (count == 0) //plan doesn't ovelap. So commit.
            {
                msg = null;
            }
            else
            {
                string OLPD = "<div  style='font-size:.75vw'>";
                OLPD += "<p><span>This Automatic Report Setting has already been added. </span></p>";

                OLPD += "</div>";
                msg = OLPD;
            }
            return msg;

        }

        public JsonResult GetRL2(int RL1ID)
        {
            var RL2Data = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 2 && m.LossCodesLevel1ID == RL1ID), "LossCodeID", "LossCode");
            return Json(RL2Data, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetRL3(int RL2ID)
        {
            var RL2Data = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 3 && m.LossCodesLevel2ID == RL2ID), "LossCodeID", "LossCode");
            return Json(RL2Data, JsonRequestBehavior.AllowGet);
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
            var MachineData = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.CellID == CellID), "MachineID", "MachineInvNo");
            return Json(MachineData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetWC_Shop(int ShopID)
        {
            var MachineData = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.ShopID == ShopID && m.CellID.Equals(null)), "MachineID", "MachineInvNo");
            return Json(MachineData, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMailIDs(string Prefix, string AllVal)
        {
            //This format was working.

            List<string> existingMailIds = new List<string>();
            if (AllVal.Contains(','))
            {
                string[] mails = AllVal.Split(',');
                existingMailIds.AddRange(mails);
            }

            tblmailid[] MailData = (from c in db.tblmailids
                                    where !existingMailIds.Contains(c.EmailID) && c.IsDeleted == 0
                                    select c).ToArray();

            var MailDataToView = (from N in MailData
                                  where N.EmailID.StartsWith(Prefix)
                                  select new { N.EmailID });

            return Json(MailDataToView, JsonRequestBehavior.AllowGet);
        }

        public string GetReasonType(int reasonId)
        {
            string reason = null;
            var lossCodeData = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodeID == reasonId).FirstOrDefault();
            reason = lossCodeData.MessageType;
            if (reason == "PM")
            {
                reason = "BREAKDOWN";
            }
            if (reason == "Setup")
            {
                reason = "IDLE";
            }
            if (reason == "NoCode")
            {
                reason = "IDLE";
            }

            return reason;
        }

        private static void GetWeek(DateTime now, CultureInfo cultureInfo, out DateTime begining, out DateTime end)
        {
            if (now == null)
                throw new ArgumentNullException("now");
            if (cultureInfo == null)
                throw new ArgumentNullException("cultureInfo");

            var firstDayOfWeek = cultureInfo.DateTimeFormat.FirstDayOfWeek;
            int offset = firstDayOfWeek - now.DayOfWeek;
            if (offset != 1)
            {
                DateTime weekStart = now.AddDays(offset);
                DateTime endOfWeek = weekStart.AddDays(6);
                begining = weekStart;
                end = endOfWeek;
            }
            else
            {
                begining = now.AddDays(-6);
                end = now;
            }
        }


    }
}