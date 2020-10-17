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
    public class OperatorLoginDetailsController : Controller
    {

        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
        // GET: OperatorLoginDetails

        public ActionResult Index()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            tblOperatorLoginDetail pa = new tblOperatorLoginDetail();
            OperatorLogin mp = new OperatorLogin();
            //ViewBag.Roles = new SelectList(db.tblroles.Where(m => m.IsDeleted == 0), "RoleId", "RoleName", pa.roleId).ToList();
            mp.operatorLogin = pa;
            mp.operatorLoginList = db.tblOperatorLoginDetails.Where(m => m.isDeleted == 0).ToList();
            //ViewBag.Role = new SelectList(db.tblroles.Where(m => m.IsDeleted == 0), "RoleId", "RoleName").ToList();
            return View(mp);
        }

        [HttpGet]
        public ActionResult CreateOperatorLogin()
        {

            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                ViewBag.Roles = new SelectList(db.tblroles.Where(m => m.IsDeleted == 0 && (m.Role_ID == 6 || m.Role_ID == 9)), "Role_ID", "RoleName").ToList();
                ViewBag.machineDetails = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0), "MachineID", "MachineName").ToList();
                return View();
            }
        }

        [HttpPost]
        public string CreateOperatorLogin(OperatorLoginDetails OperatorLogin)
        {
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();

            //shop name validation
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                var operatorCheck = db.tblOperatorLoginDetails.Where(m => m.operatorId == OperatorLogin.operatorId).FirstOrDefault();
                if (operatorCheck == null)
                {
                    string[] machineName = OperatorLogin.machineids.Split(',');
                    tblOperatorLoginDetail obj = new tblOperatorLoginDetail();
                    obj.operatorUserName = OperatorLogin.operatorUserName;
                    obj.operatorPwd = OperatorLogin.operatorPwd;
                    obj.operatorEmailId = OperatorLogin.operatorEmailId;
                    obj.operatorMobileNo = OperatorLogin.operatorMobileNo;
                    obj.roleId = OperatorLogin.roleId;
                    obj.operatorId = OperatorLogin.operatorId;
                    obj.operatorName = OperatorLogin.operatorName;
                    obj.NumOfMachines = OperatorLogin.NumOfMachines;
                    obj.createdOn = DateTime.Now;
                    obj.isDeleted = 0;
                    db.tblOperatorLoginDetails.Add(obj);
                    db.SaveChanges();

                    if (machineName != null)
                    {
                        foreach (var i in machineName)
                        {
                            tblOperatorMachineDetail obj1 = new tblOperatorMachineDetail();
                            var machineId = db.tblmachinedetails.Where(m => m.MachineName == i).Select(m => m.MachineID).FirstOrDefault();
                            obj1.machineId = machineId;
                            obj1.operatorLoginId = obj.operatorLoginId;
                            obj1.isDeleted = 0;
                            obj1.createdOn = DateTime.Now;
                            db.tblOperatorMachineDetails.Add(obj1);
                            db.SaveChanges();
                        }
                    }
                }
                else
                {
                    Session["Error"] = "Operator User Name: " + OperatorLogin.operatorUserName + " already exists ";
                    return "Operator Name already exists ";
                }
                return "Created";
            }
        }

        [HttpGet]
        public ActionResult EditOperatorLogin(int Id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                tblOperatorLoginDetail operatorLoginDetail = db.tblOperatorLoginDetails.Find(Id);
                if (operatorLoginDetail == null)
                {
                    return HttpNotFound();
                }

                ViewBag.Roles = new SelectList(db.tblroles.Where(m => m.IsDeleted == 0 && (m.Role_ID == 6 || m.Role_ID == 9)), "Role_ID", "RoleName", operatorLoginDetail.roleId).ToList();
                ViewBag.machineDetails = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0), "MachineID", "MachineName").ToList();
                ViewBag.mac = String.Join(",", db.tblOperatorMachineDetails.Where(m => m.isDeleted == 0 && m.operatorLoginId == operatorLoginDetail.operatorLoginId).Select(m => m.machineId).ToList());
                OperatorLogin op = new OperatorLogin();
                op.operatorLogin = operatorLoginDetail;
                return View(op);
            }
        }

        [HttpPost]
        public string EditOperator(OperatorLoginDetails OperatorLogin)
        {

            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            int UserID = Convert.ToInt32(Session["UserID"]);

            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                var operatorCheck = db.tblOperatorLoginDetails.Where(m => m.operatorId == OperatorLogin.operatorId).FirstOrDefault();
                if (operatorCheck != null)
                {
                    string[] machineName = OperatorLogin.machineids.Split(',');
                    operatorCheck.operatorUserName = OperatorLogin.operatorUserName;
                    operatorCheck.operatorMobileNo = OperatorLogin.operatorMobileNo;
                    operatorCheck.roleId = OperatorLogin.roleId;
                    operatorCheck.operatorId = OperatorLogin.operatorId;
                    operatorCheck.operatorEmailId = OperatorLogin.operatorEmailId;
                    operatorCheck.NumOfMachines = OperatorLogin.NumOfMachines;
                    operatorCheck.operatorName = OperatorLogin.operatorName;
                    operatorCheck.modifiedOn = DateTime.Now;
                    db.Entry(operatorCheck).State = EntityState.Modified;
                    db.SaveChanges();

                    if (machineName != null)
                    {
                        var machineDetails = db.tblOperatorMachineDetails.Where(m => m.operatorLoginId == operatorCheck.operatorLoginId && m.isDeleted == 0).ToList();
                        if (machineDetails.Count != 0)
                        {
                            foreach (var ec in machineDetails)
                            {
                                ec.isDeleted = 1;
                                ec.modifiedOn = DateTime.Now;
                                db.SaveChanges();
                            }

                            foreach (var i in machineName)
                            {
                                var machineId = db.tblmachinedetails.Where(m => m.MachineName == i).Select(m => m.MachineID).FirstOrDefault();
                                tblOperatorMachineDetail obj1 = new tblOperatorMachineDetail();
                                obj1.machineId = machineId;
                                obj1.operatorLoginId = operatorCheck.operatorLoginId;
                                obj1.isDeleted = 0;
                                obj1.createdOn = DateTime.Now;
                                db.tblOperatorMachineDetails.Add(obj1);
                                db.SaveChanges();
                            }
                        }
                    }
                    else
                    {

                    }
                }
                else
                {
                    Session["Error"] = "Operator Name already exists ";
                    return "Operator Name already exists ";
                }
            }
            return "Updated";
        }

        public string DeleteOperatorDetails(int id)
        {

            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            int UserID = Convert.ToInt32(Session["UserID"]);
            //ViewBag.IsConfigMenu = 0;


            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                tblOperatorLoginDetail obj = db.tblOperatorLoginDetails.Find(id);
                obj.isDeleted = 1;
                db.Entry(obj).State = EntityState.Modified;
                db.SaveChanges();
            }

            var mac = db.tblOperatorMachineDetails.Where(m => m.operatorLoginId == id).ToList();
            if (mac.Count != 0)
            {
                foreach (var ec in mac)
                {
                    ec.isDeleted = 1;
                    db.SaveChanges();
                }
            }
            return "Deleted";
        }
    }
}