using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using SRKSDemo.App_Start;
using SRKSDemo.Server_Model;
namespace SRKSDemo.Controllers
{
    public class PlannedBreaksManagementController : Controller
    {
        // GET: PlannerBreaksManagement
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
        public ActionResult Index()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            return View(db.tblplannedbreaks.Where(m => m.IsDeleted == 0).Include(m => m.tblshift_mstr).ToList());
        }
        
        public ActionResult Create()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            return View();
        }

        public JsonResult CreateData(string st, string br, string et)
        {
            tblplannedbreak tblplannedbreak = new tblplannedbreak();
            tblplannedbreak.CreatedBy = 1;
            tblplannedbreak.CreatedOn = DateTime.Now;
            tblplannedbreak.IsDeleted = 0;
            tblplannedbreak.StartTime = Convert.ToDateTime(st).TimeOfDay;
            tblplannedbreak.EndTime = Convert.ToDateTime(et).TimeOfDay;
            tblplannedbreak.BreakReason = br;
            {
                db.tblplannedbreaks.Add(tblplannedbreak);
                db.SaveChanges();
            }
            return Json(tblplannedbreak.BreakID, JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult InsertData(string st,string br,string et )
        {
            tblplannedbreak tblplannedbreak = new tblplannedbreak();
            tblplannedbreak.CreatedBy = 1;
            tblplannedbreak.CreatedOn = DateTime.Now;
            tblplannedbreak.IsDeleted = 0;
            tblplannedbreak.StartTime = Convert.ToDateTime(st).TimeOfDay;
            tblplannedbreak.EndTime = Convert.ToDateTime(et).TimeOfDay;
            tblplannedbreak.BreakReason = br;
            {
                db.tblplannedbreaks.Add(tblplannedbreak);
                db.SaveChanges();
            }
            return Json(tblplannedbreak.BreakID, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult Edit(int id = 0)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            tblplannedbreak tblplannedbreak = db.tblplannedbreaks.Find(id);
            if (tblplannedbreak == null)
            {
                return HttpNotFound();
            }
            return View(tblplannedbreak);
        }

        [HttpPost]
        public ActionResult Edit(tblplannedbreak tblplannedbreak)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            tblplannedbreak.ModifiedBy = 1;
            tblplannedbreak.ModifiedOn = DateTime.Now;
            tblplannedbreak.IsDeleted = 0;
            db.Entry(tblplannedbreak).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id = 0)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            tblplannedbreak tblplannedbreak = db.tblplannedbreaks.Find(id);
            if (tblplannedbreak == null)
            {
                return HttpNotFound();
            }
            tblplannedbreak.IsDeleted = 1;
            tblplannedbreak.ModifiedBy = 1;
            tblplannedbreak.ModifiedOn = System.DateTime.Now;
            db.Entry(tblplannedbreak).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        
        public JsonResult DeleteData(int id =0)
        {
            tblplannedbreak tblplannedbreak = db.tblplannedbreaks.Find(id);
            tblplannedbreak.IsDeleted = 1;
            tblplannedbreak.ModifiedBy = 1;
            tblplannedbreak.ModifiedOn =DateTime.Now;
            db.Entry(tblplannedbreak).State = EntityState.Modified;
            db.SaveChanges();
            return Json(tblplannedbreak.BreakID, JsonRequestBehavior.AllowGet);
        }

        public string startandendtimeduplicatecheck(string st, string et)
        {
            string status = "ok";
            int flag = 0;
            if (st != null && st != "" && et != null && et != "")
            {
                TimeSpan starttime = TimeSpan.Parse(st);
                TimeSpan endtime = TimeSpan.Parse(et);

                var tpb = db.tblplannedbreaks.Where(m => m.IsDeleted == 0).ToList();
                foreach (tblplannedbreak item in tpb)
                {
                    TimeSpan ExistingStartTime = (TimeSpan)item.StartTime;
                    TimeSpan ExistingEndTime = (TimeSpan)item.EndTime;

                    if((starttime < ExistingEndTime) && (endtime > ExistingStartTime))
                    {
                        flag = flag + 1;
                        break;
                    }
                }
            }
            else
            {
                flag = 0;
            }
            if (flag > 0)
            {
                status = "notok";
            }
            else
            {
                status = "ok";
            }
            return status;
        }


        public string timeRangeChecker(string st, string et)
        {
            string status = "ok";
            Utility ul = new Utility();
            if (st != null && st != "" && et != null && et != "")
            {
                TimeSpan starttime = TimeSpan.Parse(st); 
                TimeSpan endtime = TimeSpan.Parse(et);
                TimeSpan newStartTime = ul.timeConerterToCCSSETime(starttime);
                TimeSpan newEndTime = ul.timeConerterToCCSSETime(endtime);

                if (newStartTime > newEndTime)
                {
                    status = "notok";
                }
                else {

                    status = "ok";
                }
            }
            return status;
        }

        public string BreakReasonDuplicatecheck(string breakreason = "")
        {
            string status = "ok";
            var doesreasonexist = db.tblplannedbreaks.Where(m => m.IsDeleted == 0 && m.BreakReason == breakreason).ToList();
            if (doesreasonexist.Count() == 0)
            {
                status = "ok";
            }
            else
            {
                status = "notok";
            }
            return status;
        }

        public string startandendtimeduplicatecheckedit(string st, string et, int editid = 0)
        {
            string status = "ok";
            int flag = 0;
            if (st != null && st != "" && et != null && et != "")
            {
                TimeSpan starttime = TimeSpan.Parse(st);
                TimeSpan endtime = TimeSpan.Parse(et);

                var tpb = db.tblplannedbreaks.Where(m => m.IsDeleted == 0 && m.BreakID!=editid).ToList();
                foreach (tblplannedbreak item in tpb)
                {
                    TimeSpan ExistingStartTime = (TimeSpan)item.StartTime;
                    TimeSpan ExistingEndTime = (TimeSpan)item.EndTime;
                    if (starttime == ExistingStartTime && endtime == ExistingEndTime)
                    {
                        flag = flag + 1;
                        break;
                    }
                    else if ((starttime < ExistingStartTime && starttime < ExistingEndTime) && (endtime > ExistingStartTime && endtime < ExistingEndTime))
                    {
                        flag = flag + 1;
                        break;
                    }
                    else if ((starttime > ExistingStartTime && starttime < ExistingEndTime) && (endtime > starttime && endtime > ExistingEndTime))
                    {
                        flag = flag + 1;
                        break;
                    }
                    else if ((starttime > ExistingStartTime && starttime < ExistingEndTime) && (endtime > starttime && endtime < ExistingEndTime))
                    {
                        flag = flag + 1;
                        break;
                    }
                    else if ((starttime == ExistingStartTime && starttime > ExistingEndTime) && (endtime > ExistingStartTime && endtime > ExistingEndTime))
                    {
                        flag = flag + 1;
                        break;
                    }
                    else if ((starttime > ExistingStartTime && starttime > ExistingEndTime) && (endtime > ExistingStartTime && endtime == ExistingEndTime))
                    {
                        flag = flag + 1;
                        break;
                    }
                    else if ((starttime < ExistingStartTime && starttime < endtime) && (endtime > ExistingStartTime && endtime > ExistingEndTime))
                    {
                        flag = flag + 1;
                    }
                }
            }
            else
            {
                flag = 0;
            }
            if (flag > 0)
            {
                status = "notok";
            }
            else
            {
                status = "ok";
            }
            return status;
        }

        public string BreakReasonDuplicatecheckedit(int editid = 0, string breakreason = "")
        {
            string status = "ok";
            var doesreasonexist = db.tblplannedbreaks.Where(m => m.IsDeleted == 0 && m.BreakReason == breakreason && m.BreakID!=editid).ToList();
            if (doesreasonexist.Count() == 0)
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