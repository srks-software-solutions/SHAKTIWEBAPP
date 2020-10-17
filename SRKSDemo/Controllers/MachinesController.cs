using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Newtonsoft.Json;
using SRKSDemo.Server_Model;
using SRKSDemo;

namespace SRKSDemo.Controllers
{
    public class MachinesController : Controller
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
        //getting machine list
        [HttpGet]
        public ActionResult MachineList()
        {

            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
          
                ViewBag.plant = new SelectList(db.tblplants.Where(p => p.IsDeleted == 0), "PlantID", "PlantDisplayName");
                ViewBag.dept = new SelectList(db.tblshops.Where(d => d.IsDeleted == -1), "ShopId", "ShopDisplayName");
                ViewBag.cell = new SelectList(db.tblcells.Where(m => m.IsDeleted == -1), "CellId", "CellDisplayName");
                MachineModel ma = new MachineModel();
                tblmachinedetail me = new tblmachinedetail();
                ma.Machine = me;
                ma.MachineList = db.tblmachinedetails.Where(x => x.IsDeleted == 0).ToList();
                return View(ma);
          
            //var machine = db.mastermachine_tbl.Where(x => x.IsDeleted == 0);
            //return View(machine.ToList());
        }

        [HttpGet]
        public ActionResult CreateMachine()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            ViewBag.plant = new SelectList(db.tblplants.Where(p => p.IsDeleted == 0), "PlantID", "PlantName");
            ViewBag.dept = new SelectList(db.tblshops.Where(d => d.IsDeleted == 0), "ShopId", "ShopName");
            ViewBag.cell = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0), "CellId", "CellName");
            return View();
        }
        [HttpPost]
        public ActionResult CreateMachine(MachineModel tblmachine, int dept = 0, int Plant = 0, int cell=0, int Machnecategory = 0)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];

            string machinename = tblmachine.Machine.MachineName;
            var duplicateEntry = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == Plant && m.ShopID == dept && m.CellID == Machnecategory && m.MachineName == machinename).ToList();
            if (duplicateEntry.Count == 0)
            {
                tblmachine.Machine.InsertedBy = 1; //Convert.ToInt32(Session["UserId"]);
                var Datevar= Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                tblmachine.Machine.InsertedOn = Datevar.ToString();
                tblmachine.Machine.IsDeleted = 0;
                tblmachine.Machine.ServerTabFlagSync = 0;
                tblmachine.Machine.ServerTabCheck = 1;
                tblmachine.Machine.PlantID = Plant;
                tblmachine.Machine.ShopID = dept;
                tblmachine.Machine.CellID = cell;
                //var shopname = db.tblshops.Where(m => m.IsDeleted == 0 && m.ShopID == Shop).Select(m => m.ShopName).FirstOrDefault();
                //tblmachine.ShopNo = shopname.ToString();

                //if (Cell != 0)
                //{
                //    tblmachine.CellID = Cell;
                //}

                db.tblmachinedetails.Add(tblmachine.Machine);
                db.SaveChanges();
            }
            else
            {
                Session["Error"] = "Duplicate Machine Name";
                ViewBag.plant = new SelectList(db.tblplants.Where(p => p.IsDeleted == 0), "PlantID", "PlantName", tblmachine.Machine.PlantID);
                ViewBag.dept = new SelectList(db.tblshops.Where(d => d.IsDeleted == 0), "ShopId", "ShopName", tblmachine.Machine.ShopID);
                ViewBag.MachineCategoryID = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0), "CellID", "CellName", tblmachine.Machine.CellID);
                return View(tblmachine);
            }
            return RedirectToAction("MachineList");
        }

        [HttpGet]
        public ActionResult EditMachine(int Id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            tblmachinedetail machine = db.tblmachinedetails.Find(Id);
            if (machine == null)
            {
                return HttpNotFound();
            }
            int plantid = Convert.ToInt32(machine.PlantID);
            ViewBag.Plant = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", machine.PlantID);
            ViewBag.dept = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0), "ShopId", "ShopName", machine.ShopID);
            ViewBag.cell = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0), "CellID", "CellName", machine.CellID);
            return View(machine);
        }
        //Update Machine
        [HttpPost]
        public ActionResult EditMachine(tblmachinedetail tblmachine, int dept = 0, int plant = 0, int cell = 0, string Machine = "")
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            int UserID = Convert.ToInt32(Session["UserID"]);
            tblmachine.PlantID = plant;
            tblmachine.CellID = cell;
            tblmachine.ShopID = dept;
            // tblmachine.MachineCategoryID = machinecategory;
            tblmachine.ModifiedBy = UserID;
            tblmachine.ModifiedOn = System.DateTime.Now;
            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
            {
                var duplicateEntry = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == plant && m.ShopID == dept && m.CellID == tblmachine.CellID && m.MachineName == tblmachine.MachineName && m.MachineID != tblmachine.MachineID).ToList();
                if (duplicateEntry.Count == 0)
                {
                    var MachineDet = db.tblmachinedetails.Find(tblmachine.MachineID);

                    MachineDet.CellID = tblmachine.CellID;
                    MachineDet.ControllerType = tblmachine.ControllerType;
                    MachineDet.CurrentControlAxis = tblmachine.CurrentControlAxis;
                    MachineDet.IPAddress = tblmachine.IPAddress;
                    MachineDet.MachineDescription = tblmachine.MachineDescription;
                    MachineDet.MachineDisplayName = tblmachine.MachineDisplayName;
                    MachineDet.MachineMake = tblmachine.MachineMake;
                    MachineDet.MachineModel = tblmachine.MachineModel;
                    MachineDet.MachineName = tblmachine.MachineName;
                    MachineDet.MachineType = tblmachine.MachineType;
                    MachineDet.ModelType = tblmachine.ModelType;
                    MachineDet.NoOfAxis = tblmachine.NoOfAxis;
                    MachineDet.PlantID = tblmachine.PlantID;
                    MachineDet.ShopID = tblmachine.ShopID;
                    MachineDet.ServerTabFlagSync = 1;
                    MachineDet.ServerTabCheck = 2;
                    MachineDet.IsParameters = tblmachine.IsParameters;
                    MachineDet.IsPCB = tblmachine.IsPCB;
                    MachineDet.IsNormalWC = tblmachine.IsNormalWC;
                    MachineDet.MacConnName = tblmachine.MacConnName;
                    MachineDet.SpindleAxis = tblmachine.SpindleAxis;
                    MachineDet.TabIPAddress = tblmachine.TabIPAddress;
                    MachineDet.EnableLockLogic = tblmachine.EnableLockLogic;
                    MachineDet.ModifiedBy = tblmachine.ModifiedBy;
                    MachineDet.ModifiedOn = tblmachine.ModifiedOn;

                    db.Entry(MachineDet).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("MachineList","Machines");
                }
                else
                {
                    ViewBag.Plant = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", tblmachine.PlantID);
                    ViewBag.dept = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0), "ShopID", "ShopName", tblmachine.ShopID);
                    ViewBag.MachineCategoryID = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0), "CellID", "CellName", tblmachine.CellID);
                    return View(tblmachine);
                }
            }
        }

        //Delete Machine
        public ActionResult DeleteMachine(int id)
        {
            //ViewBag.Logout = Session["Username"].ToString().ToUpper();
            //ViewBag.roleid = Session["RoleID"];
            //String Username = Session["Username"].ToString();
            //int UserID1 = id;
            //ViewBag.IsConfigMenu = 0;

            //start Logging
            // int UserID = Convert.ToInt32(Session["UserId"]);
            // string CompleteModificationdetail = "Deleted Role";
            // Action = "Delete";
            // ActiveLogStorage Obj = new ActiveLogStorage();
            //Obj.SaveActiveLog(Action, Controller, Username, UserID, CompleteModificationdetail);
            //End
            tblmachinedetail tblmc = db.tblmachinedetails.Find(id);
            int mcahineid = tblmc.MachineID;
            tblmc.IsDeleted = 1;
            tblmc.ModifiedBy = 1;
            tblmc.ServerTabFlagSync = 1;
            tblmc.ServerTabCheck = 2;
            tblmc.ModifiedOn = DateTime.Now;
            db.Entry(tblmc).State = EntityState.Modified;
            db.SaveChanges();

            //delete corresponding machines
            var machinedata = db.tblcells.Where(m => m.IsDeleted == 0 && m.CellID == mcahineid).ToList();
            foreach (var machinerow in machinedata)
            {
                machinerow.IsDeleted = 1;
                db.Entry(machinerow).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("MachineList");
        }

        public JsonResult MachineById(int Id)
        {
            var Data = db.tblmachinedetails.Where(m => m.MachineID == Id).Select(m => new { pname = m.PlantID, dname = m.ShopID, maccatname = m.CellID, mname = m.MachineName, mdesc = m.MachineDescription, mdiispname = m.MachineDisplayName, ContType = m.ControllerType, MacModel = m.MachineModel, MacMake = m.MachineMake, ModType = m.ModelType, CurrAxis = m.CurrentControlAxis, MaxAxis = m.NoOfAxis, MacIPAdd = m.IPAddress });
            return Json(Data, JsonRequestBehavior.AllowGet);
        }
        //Machine Table End

        public JsonResult FetchDept(int PID)
        {
            var DeptData = (from row in db.tblshops
                            where row.IsDeleted == 0 && row.PlantID == PID
                            select new { Value = row.ShopID, Text = row.Shopdisplayname }
                                );
            return Json(DeptData, JsonRequestBehavior.AllowGet);
        }

        public JsonResult FetchCat(int DeptID)
        {
            var CatData = (from row in db.tblcells
                           where row.IsDeleted == 0 && row.ShopID == DeptID
                           select new { Value = row.CellID, Text = row.CelldisplayName }
                                );
            return Json(CatData, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMacDetail(String IPAdd)
        {
            int Connected = 0;
            String ControllerType = null;
            int NoOfAxis = 0;
            int CurrentControlAxis = 0;
            String ModelType = null;

            AddFanucMachineWithConn Conn = new AddFanucMachineWithConn(IPAdd);
            Conn.AddFanucMac(out Connected, out ControllerType, out NoOfAxis, out CurrentControlAxis, out ModelType);
            String[] JsonData = new String[4];
            if(Connected != 0)
            {
                JsonData[0] = ControllerType;
                JsonData[1] = ModelType;
                JsonData[2] = CurrentControlAxis.ToString();
                JsonData[3] = NoOfAxis.ToString();
            }
            else
            {
                JsonData[0] = ControllerType;
                JsonData[1] = ModelType;
                JsonData[2] = "0";
                JsonData[3] = "0";
            }

            //String SendJsonData = JsonConvert.SerializeObject(JsonData);

            return Json(JsonData, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public string MacNameDuplicateCheck(int plantID = 0, int shopId = 0,int cellId=0, string machineName = "")
        {
            string status = "notok";
            var duplicateEntry = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == plantID && m.ShopID == shopId && m.CellID == cellId && m.MachineName == machineName).ToList();
            if (duplicateEntry.Count == 0)
            {
                status = "ok";
            }
            else
            {
                status = "notok";
            }
            return status;
        }

        [HttpPost]
        public string MacNameDuplicateCheckEdit(int plantID = 0, int shopId = 0, int cellId = 0, string machineName = "", int editMachineID = 0)
        {
            string status = "notok";
            var duplicateEntry = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == plantID && m.ShopID == shopId && m.CellID == cellId && m.MachineName == machineName).ToList();
            if (duplicateEntry.Count == 0)
            {
                status = "ok";
            }
            else
            {
                var checkforId = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == plantID && m.ShopID == shopId && m.CellID == cellId && m.MachineName == machineName && m.MachineID == editMachineID).ToList();//checks for that machineId
                if (checkforId.Count != 0)
                {
                    status = "ok";
                }
                else
                {
                    status = "notok";
                }
            }
            return status;
        }
    }
}