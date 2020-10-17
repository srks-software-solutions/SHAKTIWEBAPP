using System;
using System.Linq;
using System.Web.Mvc;
using SRKSDemo.Server_Model;
namespace SRKSDemo.Controllers
{
    public class ToolLifeController : Controller
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();

        // GET: ToolLife
        public ActionResult Index(int PlantID = 0, int ShopID = 0, int CellID = 0, int WorkCenterID = 0)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];

            ViewData["PlantID"] = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", PlantID);
            ViewData["ShopID"] = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID), "ShopID", "ShopName", ShopID);
            ViewData["CellID"] = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID), "CellID", "CellName", CellID);
            ViewData["WorkCenterID"] = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID && m.CellID == CellID && m.IsNormalWC == 0), "MachineID", "MachineDisplayName", WorkCenterID);
            return View();
        }
    }
}