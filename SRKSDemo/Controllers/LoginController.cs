using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data;
using SRKSDemo.Server_Model;

namespace i_facility.Controllers
{
    public class LoginController : Controller
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();


        string Controller = "Login";
        string Action = null;

        public ActionResult Index()
        {
           
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            var tbllogin = db.tblusers.Include(t=>t.tblmachinedetail).Where(m => m.IsDeleted == 0);
            return View(tbllogin);
        }
        public ActionResult Create()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            int roleid = Convert.ToInt32(Session["RoleID"]);
            String Username = Session["Username"].ToString();
            ViewBag.PrimaryRole = new SelectList(db.tblroles.Where(m => m.IsDeleted == 0 && m.Role_ID >= roleid), "Role_ID", "RoleDesc");
            ViewBag.SecondaryRole = new SelectList(db.tblroles.Where(m => m.IsDeleted == 0 && m.Role_ID >= roleid), "Role_ID", "RoleDesc");
            ViewBag.MachineID = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0), "MachineID", "MachineDispName");
            return View();
        }
        [HttpPost]
        public ActionResult Create(tbluser tbluser)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            int roleid = Convert.ToInt32( Session["RoleID"]);
            String Username = Session["Username"].ToString();
            //ViewBag.PrimaryRole = new SelectList(db.tblroles.Where(m => m.IsDeleted == 0), "Role_ID", "RoleDesc");
            //ViewBag.SecondaryRole = new SelectList(db.tblroles.Where(m => m.IsDeleted == 0), "Role_ID", "RoleDesc");
            tbluser.CreatedBy = roleid;
            tbluser.CreatedOn = System.DateTime.Now;
            tbluser.IsDeleted = 0;
            ////ActiveLog Code
            //int UserID = Convert.ToInt32(Session["UserId"]);
            //string CompleteModificationdetail = "New Creation";
            //Action = "Create";
            //ActiveLogStorage Obj = new ActiveLogStorage();
            //Obj.SaveActiveLog(Action, Controller, Username, UserID, CompleteModificationdetail);
            ////End
            var dupUserData = db.tblusers.Where(m => m.IsDeleted == 0 && m.UserName == tbluser.UserName).ToList();
            if (dupUserData.Count == 0)
            {
                int primaryrole = Convert.ToInt32(tbluser.PrimaryRole);
                if (primaryrole != 3)
                {
                   // tbluser.MachineID =  ;
                }
                db.tblusers.Add(tbluser);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                Session["Error"] = "User Name Exists.";
                ViewBag.PrimaryRole = new SelectList(db.tblroles.Where(m => m.IsDeleted == 0 && m.Role_ID >= roleid), "Role_ID", "RoleDesc", tbluser.PrimaryRole);
                ViewBag.SecondaryRole = new SelectList(db.tblroles.Where(m => m.IsDeleted == 0 && m.Role_ID >= roleid), "Role_ID", "RoleDesc", tbluser.SecondaryRole);
                ViewBag.MachineID = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0), "MachineID", "MachineDispName", tbluser.MachineID);
                return View(tbluser);
            }
        }
        public ActionResult Edit(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            int roleid = Convert.ToInt32(Session["RoleID"]);
            String Username = Session["Username"].ToString();
            tbluser tbluser = db.tblusers.Find(id);
            if (tbluser == null)
            {
                return HttpNotFound();
            }
            ViewBag.PrimaryRole = new SelectList(db.tblroles.Where(m => m.IsDeleted == 0 && m.Role_ID >= roleid), "Role_ID", "RoleDesc", tbluser.PrimaryRole);
            ViewBag.SecondaryRole = new SelectList(db.tblroles.Where(m => m.IsDeleted == 0 && m.Role_ID >= roleid), "Role_ID", "RoleDesc", tbluser.SecondaryRole);
            ViewBag.MachineID = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0), "MachineID", "MachineDispName", tbluser.MachineID);
            return View(tbluser);
        }
        [HttpPost]
        public ActionResult Edit(tbluser tbluser)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            int UserID = Convert.ToInt32(Session["UserID"]);
            int roleid = Convert.ToInt32(Session["RoleID"]);
            tbluser.ModifiedBy = UserID;
            tbluser.ModifiedOn = System.DateTime.Now;
            var dupUserData = db.tblusers.Where(m => m.IsDeleted == 0 && m.UserName == tbluser.UserName && m.UserID != tbluser.UserID).ToList();
            if (dupUserData.Count == 0)
            {
                #region Active Log Code
                //tblUser OldData = db.tblUsers.Find(tbluser.UserID);
                //IEnumerable<string> FullData = ActiveLog.EnumeratePropertyDifferences<tblUser>(OldData, tbluser);
                //ICollection<tblUser> c = FullData as ICollection<tblUser>;
                //int Count = c.Count;
                //if (Count != 0)
                //{
                //    string CompleteModificationdetail = null;
                //    for (int i = 0; i < Count; i++)
                //    {
                //        CompleteModificationdetail = CompleteModificationdetail + "-" + FullData.Take(i).ToArray();
                //    }
                //    Action = "Edit";
                //    ActiveLogStorage Obj = new ActiveLogStorage();
                //    Obj.SaveActiveLog(Action, Controller, Username, UserID, CompleteModificationdetail);
                //}
                #endregion //End Active Log

                int primaryrole = Convert.ToInt32( tbluser.PrimaryRole);
                if (primaryrole != 3)
                {
                    tbluser.MachineID = Convert.ToInt32(System.DBNull.Value);
                }
                db.Entry(tbluser).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                Session["Error"] = "User Name Exists.";
                ViewBag.PrimaryRole = new SelectList(db.tblroles.Where(m => m.IsDeleted == 0 && m.Role_ID >= roleid), "Role_ID", "RoleDesc", tbluser.PrimaryRole);
                ViewBag.SecondaryRole = new SelectList(db.tblroles.Where(m => m.IsDeleted == 0 && m.Role_ID >= roleid), "Role_ID", "RoleDesc", tbluser.SecondaryRole);
                ViewBag.MachineID = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0), "MachineID", "MachineDispName", tbluser.MachineID);
                return View(tbluser);
            }

        }

        public string VerifyPassword(string currpass,string username)
        {
            string res = "";
            var verify = db.tblusers.Where(m => m.Password == currpass && m.UserName == username).FirstOrDefault();
            if(verify !=null)
            {
                res = "Success";
            }
            return res;
        }
        [HttpGet]
        public ActionResult ChangePassword(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            int roleid = Convert.ToInt32(Session["RoleID"]);
            String Username = Session["Username"].ToString();
            tbluser tbluser = db.tblusers.Find(id);
            if (tbluser == null)
            {
                return HttpNotFound();
            }
            return View(tbluser);
        }


        public string ChangePassword(string username, string password)
        {
            string res = "";
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            int UserID = Convert.ToInt32(Session["UserID"]);
            int roleid = Convert.ToInt32(Session["RoleID"]);
            var userid = db.tblusers.Where(m => m.UserName == username).Select(m => m.UserID).FirstOrDefault();
            var dupUserData = db.tblusers.Where(m => m.IsDeleted == 0 && m.UserName == username && m.UserID != userid && m.Password==password).ToList();
            if (dupUserData.Count == 0)
            {
                var userdet = db.tblusers.Where(m => m.UserID == userid).FirstOrDefault();
                userdet.Password = password;
                userdet.ModifiedBy = UserID;
                userdet.ModifiedOn = System.DateTime.Now;
                db.Entry(userdet).State = EntityState.Modified;
                db.SaveChanges();
                res = "Success";
            }
            return res;
        }
        //[HttpPost]
        //public ActionResult ChangePassword(string UserName, string cngPassword)
        //{
        //    ViewBag.Logout = Session["Username"];
        //    ViewBag.roleid = Session["RoleID"];
        //    String Username = Session["Username"].ToString();
        //    int UserID = Convert.ToInt32(Session["UserID"]);
        //    ViewBag.userid = UserID;
        //    int roleid = Convert.ToInt32(Session["RoleID"]);
        //    var dupUserData = db.tblusers.Where(m => m.IsDeleted == 0 && m.UserName == UserName && m.UserID != UserID).ToList();
        //    if (dupUserData.Count == 0)
        //    {
        //        var userdet = db.tblusers.Where(m => m.UserID == UserID).FirstOrDefault();
        //        userdet.Password = cngPassword;
        //        userdet.ModifiedBy = UserID;
        //        userdet.ModifiedOn = System.DateTime.Now;
        //        db.Entry(userdet).State = EntityState.Modified;
        //        db.SaveChanges();
        //        //res = "Success";
        //    }
        //    return RedirectToAction("Index");
        //}
        public ActionResult Delete(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            int UserID = Convert.ToInt32(Session["UserId"]);
            //ViewBag.IsConfigMenu = 0;
            tbluser tblusers = db.tblusers.Find(id);
            tblusers.IsDeleted = 1;
            tblusers.ModifiedBy = UserID;
            tblusers.ModifiedOn = System.DateTime.Now;
            //start Logging

            //string CompleteModificationdetail = "Deleted User";
            //Action = "Delete";
            //ActiveLogStorage Obj = new ActiveLogStorage();
            //Obj.SaveActiveLog(Action, Controller, Username, UserID, CompleteModificationdetail);
            //End
            db.Entry(tblusers).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {

            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(tbluser userlogin)
        {
            ViewBag.Logout = Session["Username"];
            if (userlogin.UserName != null && userlogin.Password != null)
            {
                var usercnt = db.tblusers.Where(m => m.UserName == userlogin.UserName && m.Password == userlogin.Password && m.IsDeleted == 0).Count();
                if (usercnt == 0)
                {
                    TempData["username"] = "Please enter a valid User Name & Password";
                }
               
                if (usercnt != 0)
                {
                    var log = db.tblusers.Where(m => m.UserName == userlogin.UserName && m.Password == userlogin.Password && m.IsDeleted == 0).Select(m => new { m.UserID, m.PrimaryRole, m.DisplayName, m.MachineID }).Single();
                    Session["UserID"] = log.UserID;
                    Session["Username"] = log.DisplayName;
                    Session["RoleID"] = log.PrimaryRole;
                    Session["FullName"] = log.DisplayName;
                    Session["MachineID"] = log.MachineID;
                    int opid = Convert.ToInt32(Session["UserID"]);
                    //Getting Shift Value
                    DateTime Time = DateTime.Now;
                    //TimeSpan Tm = new TimeSpan(Time.Hour, Time.Minute, Time.Second);
                    //var ShiftDetails = db.tblshift_mstr.Where(m => m.StartTime <= Tm && m.EndTime >= Tm);
                    //string Shift = "C";
                    int ShiftID = 0;
                    //foreach (var a in ShiftDetails)
                    //{
                    //    Shift = a.ShiftName;
                    //}
                    ViewBag.date = System.DateTime.Now;

                    //get shift new code only for Operator.
                    string Shift = null;
                    if (log.PrimaryRole == 3)
                    {
                        ViewBag.shift = "C";
                        Shift = "C";
                    }

                    if (Shift == "A")
                        ShiftID = 1;
                    else if (Shift == "B")
                        ShiftID = 2;
                    else
                        ShiftID = 3;

                    Session["shiftforpopup"] = Shift;
                    //Checking operator machine is allocated or not
                    int Machinid = Convert.ToInt32(Session["MachineID"]);
                    string CorrectedDate = null;
                    tbldaytiming StartTime = db.tbldaytimings.Where(m => m.IsDeleted == 0).SingleOrDefault();
                    TimeSpan Start = StartTime.StartTime;
                    if (Start <= DateTime.Now.TimeOfDay)
                    {
                        CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                    }

                    //var machineallocation = db.tblmachineallocations.Where(m => m.IsDeleted == 0 && m.CorrectedDate == CorrectedDate && m.UserID == opid && m.ShiftID == ShiftID);
                    //if (machineallocation.Count() != 0)
                    //{
                    //    foreach (var a in machineallocation)
                    //    {
                    //        Machinid = Convert.ToInt32(a.MachineID);
                    //        Session["MachineID"] = Machinid;
                    //    }
                    //}

                    ViewBag.roleid = log.PrimaryRole;
                    //if (log.PrimaryRole == 1 || log.PrimaryRole == 2)
                    //{
                    //    Response.Redirect("~/Dashboard/Index", false);
                    //}
                    if(log.PrimaryRole !=3)
                    {
                        Response.Redirect("~/Dashboard/Index", false);
                    }
                    else if (log.PrimaryRole == 3)
                    {
                        int MacID = Convert.ToInt32( Session["MachineID"] );
                        var MacDetails = db.tblmachinedetails.Where(m => m.MachineID == MacID).SingleOrDefault();
                        //Response.Redirect("~/HMIScree/Index", false);
                        if (MacDetails.IsNormalWC == 0)
                        {
                            // Response.Redirect("~/HMIScree/Index", false);
                            Response.Redirect("~/HMIScree/Index", false);
                        }
                        else if (MacDetails.IsNormalWC == 1)
                        {
                            Response.Redirect("~/ManualHMIScreen/Index", false);
                        }
                    }
                    else if (log.PrimaryRole == 4)
                    {
                        Response.Redirect("~/MachineStatus/Index", false);
                    }
                    else if (log.PrimaryRole == 5)
                    {
                        Response.Redirect("~/Dashboard/Index", false);
                    }
                    else if(log.PrimaryRole == 8)
                    {
                        int MacID = Convert.ToInt32(Session["MachineID"]);
                        ViewBag.MachineID = MacID;
                        var MacDetails = db.tblmachinedetails.Where(m => m.MachineID == MacID).FirstOrDefault();

                        //if (MacDetails.IsNormalWC == 2)// for testing purpose changed has 0 after that change to 2
                        //{
                        //    var hmidet = db.tblSplivehmiscreens.Where(m => m.MachineID == MacID && m.IsBatchStart == 1 || (m.isWorkInProgress == 0 && m.Status == 1)).FirstOrDefault();
                        //    if (hmidet != null)
                        //    {
                        //        Session["HMIStart"] = 1;
                        //        return View();
                        //    }
                        //    else
                        //    {
                        //        Session["HMIStart"] = 0;
                        //        return View();
                        //    }
                        //}
                    }
                }
            }
            return View(userlogin);
        }

        public ActionResult Logout()
        {
            Session.Abandon();
            return RedirectToAction("Login", "Login");
        }

    }
}
