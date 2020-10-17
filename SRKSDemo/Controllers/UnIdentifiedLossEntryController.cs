
using SRKSDemo.Models;
using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SRKSDemo.Controllers
{
    // Look for the class "UnIdentifiedLossCorrection" in Model folder
    public class UnIdentifiedLossEntryController : Controller
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
        public ActionResult Index(string PlantID = null, string CDate = null, string ShopID = null, string CellID = null, string WorkCenterID = null)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            //1: get list of all 999 which have duration greater than 120 sec 
            //2: Construct new list in form of a class(UnIdentifiedLossCorrection) and send to view .
            string CorrectedDate = DateTime.Now.ToString("yyyy-MM-dd");
            if (DateTime.Now.Date.Hour < 6)
            {
                CorrectedDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }
            if (!string.IsNullOrEmpty(CDate))
            {
                CorrectedDate = CDate;
            }

            //var noCodeList = db.tbllossofentries.Where(m => m.MessageCodeID == 999 && m.CorrectedDate == CorrectedDate).OrderByDescending(m => m.CorrectedDate).ToList();
            //if (!string.IsNullOrEmpty(WorkCenterID))
            //{
            //    noCodeList = db.tbllossofentries.Where(m => m.MessageCodeID == 999 && m.MachineID == WorkCenterID && m.CorrectedDate == CorrectedDate).OrderByDescending(m => m.CorrectedDate).ToList();
            //}

            int plantId = 0, shopId = 0, cellId = 0, wcId = 0;
            string Header = null, query = null;
            if (string.IsNullOrEmpty(WorkCenterID))
            {
                if (string.IsNullOrEmpty(CellID))
                {
                    if (string.IsNullOrEmpty(ShopID))
                    {
                        if (string.IsNullOrEmpty(PlantID))
                        {//donothing 
                        }
                        else
                        {
                            plantId = Convert.ToInt32(PlantID);
                            string Macs = null;
                            var SubWCData = db.tblmachinedetails.Where(m => m.PlantID == plantId && m.IsDeleted == 0 && m.IsNormalWC == 0).ToList();
                            foreach (var subMacs in SubWCData)
                            {
                                if (Macs == null)
                                {
                                    Macs = Convert.ToString(subMacs.MachineID);
                                }
                                else
                                {
                                    Macs += "," + Convert.ToString(subMacs.MachineID);
                                }
                            }
                            query = @" SELECT LossID,MachineID,MessageCodeID,StartDateTime,EndDateTime FROM tbllossofentry Where MachineID IN ( " + Macs + ") and CorrectedDate = '" + CorrectedDate + "' and MessageCodeID = 999  order by MachineID";
                        }
                    }
                    else
                    {
                        shopId = Convert.ToInt32(ShopID);
                        string Macs = null;
                        var SubWCData = db.tblmachinedetails.Where(m => m.ShopID == shopId && m.IsDeleted == 0 && m.IsNormalWC == 0).ToList();
                        foreach (var subMacs in SubWCData)
                        {
                            if (Macs == null)
                            {
                                Macs = Convert.ToString(subMacs.MachineID);
                            }
                            else
                            {
                                Macs += "," + Convert.ToString(subMacs.MachineID);
                            }
                        }
                        query = @" SELECT LossID,MachineID,MessageCodeID,StartDateTime,EndDateTime FROM tbllossofentry Where MachineID IN ( " + Macs + ") and CorrectedDate = '" + CorrectedDate + "' and MessageCodeID = 999   order by MachineID";
                    }
                }
                else
                {
                    cellId = Convert.ToInt32(CellID);
                    string Macs = null;
                    var SubWCData = db.tblmachinedetails.Where(m => m.CellID == cellId && m.IsDeleted == 0 && m.IsNormalWC == 0).ToList();
                    foreach (var subMacs in SubWCData)
                    {
                        if (Macs == null)
                        {
                            Macs = Convert.ToString(subMacs.MachineID);
                        }
                        else
                        {
                            Macs += "," + Convert.ToString(subMacs.MachineID);
                        }
                    }
                    query = @" SELECT LossID,MachineID,MessageCodeID,StartDateTime,EndDateTime FROM tbllossofentry Where MachineID IN ( " + Macs + ") and CorrectedDate = '" + CorrectedDate + "' and MessageCodeID = 999   order by MachineID";
                }
            }
            else
            {
                wcId = Convert.ToInt32(WorkCenterID);
                query = @" SELECT LossID,MachineID,MessageCodeID,StartDateTime,EndDateTime FROM tbllossofentry Where MachineID IN ( " + wcId + ") and CorrectedDate = '" + CorrectedDate + "' and MessageCodeID = 999 ";
            }

            DataTable LossCodesData = new DataTable();
            if (!string.IsNullOrEmpty(PlantID))
            {
                using (MsqlConnection mcLossCodes = new MsqlConnection())
                {
                    mcLossCodes.open();
                    SqlDataAdapter daLossCodesData = new SqlDataAdapter(query, mcLossCodes.msqlConnection);
                    daLossCodesData.Fill(LossCodesData);
                    mcLossCodes.close();
                }
            }

            List<UnIdentifiedLossCorrection> UnIdentified = new List<UnIdentifiedLossCorrection>();
            for (int i = 0; i < LossCodesData.Rows.Count; i++)
            {
                int macID = Convert.ToInt32(LossCodesData.Rows[i][1]);
                int LossID = Convert.ToInt32(LossCodesData.Rows[i][0]);
                int LossCodeID = Convert.ToInt32(LossCodesData.Rows[i][2]);
                string MacName = Convert.ToString(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == macID).Select(m => m.MachineName).FirstOrDefault());
                UnIdentified.Add(new UnIdentifiedLossCorrection { LossID = LossID, MachineName = MacName, Level1 = LossCodeID, SDateTime = Convert.ToDateTime(LossCodesData.Rows[i][3]), EDateTime = Convert.ToDateTime(LossCodesData.Rows[i][4]) });
            }

            ViewData["PlantID"] = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", PlantID);
            ViewData["ShopID"] = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 1), "ShopID", "ShopName", ShopID);
            ViewData["CellID"] = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName", CellID);
            ViewData["WorkCenterID"] = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineInvNo", WorkCenterID);

            ViewData["Level1"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.MessageType != "Breakdown" && m.MessageType != "PM"), "LossCodeID", "LossCodeDesc", 999);
            ViewData["Level2"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 2 && m.MessageType == "Nothing"), "LossCodeID", "LossCode"); //Just send an empty List
            ViewData["Level3"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 3 && m.MessageType == "Nothing"), "LossCodeID", "LossCode"); //Just send an empty List
            return View(UnIdentified.ToList());
        }

        [HttpPost]
        public ActionResult Index(IList<UnIdentifiedLossCorrection> UnIdentifiedL)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            string MacIDs = null;
            int i = 0;
            string CorrectedDate = null;
            if(UnIdentifiedL != null)
            foreach (var row in UnIdentifiedL)
            {
                int LossCodeID = 0, LossID = 0;
                if (row.Level3 != 0)
                {
                    LossCodeID = Convert.ToInt32(row.Level3);
                    LossID = Convert.ToInt32(row.LossID);
                }
                else if (row.Level2 != 0)
                {
                    LossCodeID = Convert.ToInt32(row.Level2);
                    LossID = Convert.ToInt32(row.LossID);
                }
                else if (row.Level1 != 0 && row.Level1 != 999)
                {
                    LossCodeID = Convert.ToInt32(row.Level1);
                    LossID = Convert.ToInt32(row.LossID);
                }

                var LossData = db.tbllossofentries.Find(LossID);
                if (LossData != null)
                {
                    //Get MacNumber if it doesn't exist in String(MacIDs)
                    string MacID = Convert.ToString(LossData.MachineID);
                    if (MacIDs == null)
                    {
                        MacIDs += MacID;
                    }
                    else
                    {
                        MacIDs += "," + MacID;
                    }

                    if (i == 0)
                    {
                        CorrectedDate = LossData.CorrectedDate;
                        i++;
                    }

                    LossData.MessageCodeID = LossCodeID;
                    string LossName = Convert.ToString(db.tbllossescodes.Where(m => m.LossCodeID == LossCodeID).Select(m => m.LossCode).FirstOrDefault());
                    LossData.MessageCode = LossName;
                    LossData.MessageDesc = LossName;
                    db.Entry(LossData).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }

            //Delete Data from tblworeport table and tbloeedashboardvariables
            string StartDate = Convert.ToString(CorrectedDate);
            MsqlConnection mcDeleteRows = new MsqlConnection();
            try
            {
                try
                {
                    DataTable LossCodesData = new DataTable();
                    string query = @"SELECT Distinct(HMIID) FROM "+ MsqlConnection.DbName + ".tblworeport WHERE MachineID IN ( " + MacIDs + ") and CorrectedDate = '" + CorrectedDate + "';";
                    using (MsqlConnection mcLossCodes = new MsqlConnection())
                    {
                        mcLossCodes.open();
                        SqlDataAdapter daLossCodesData = new SqlDataAdapter(query, mcLossCodes.msqlConnection);
                        daLossCodesData.Fill(LossCodesData);
                        mcLossCodes.close();
                    }

                    mcDeleteRows.open();
                    SqlCommand cmdDeleteWOData = new SqlCommand("DELETE FROM "+ MsqlConnection.DbName + ".tblworeport WHERE MachineID IN ( " + MacIDs + ") and CorrectedDate = '" + CorrectedDate + "';", mcDeleteRows.msqlConnection);
                    cmdDeleteWOData.ExecuteNonQuery();

                    for (int l = 0; l < LossCodesData.Rows.Count; l++)
                    {
                        SqlCommand cmdDeleteWOLosses = new SqlCommand("DELETE FROM "+ MsqlConnection.DbName + ".tblwolossess WHERE HMIID = '" + LossCodesData.Rows[l][0] + "' ", mcDeleteRows.msqlConnection);
                        cmdDeleteWOLosses.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                }

                try
                {
                    SqlCommand cmdDeleteOEEData = new SqlCommand("DELETE FROM "+ MsqlConnection.DbName + ".tbloeedashboardvariables WHERE WCID IN ( " + MacIDs + ") and StartDate = '" + StartDate + "';", mcDeleteRows.msqlConnection);
                    cmdDeleteOEEData.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                }
            }
            catch (Exception e)
            {

            }
            finally
            {
                mcDeleteRows.close();
            }

            //Insert New Data
            try
            {
                CalOEE_WODetails cwod = new CalOEE_WODetails();
                cwod.CalculateOEEForYesterday(Convert.ToDateTime(CorrectedDate), Convert.ToDateTime(CorrectedDate));
                cwod.CalWODataForYesterday(Convert.ToDateTime(CorrectedDate), Convert.ToDateTime(CorrectedDate));
            }
            catch (Exception e)
            {
            }
            return RedirectToAction("Index");
        }

        public JsonResult GetLevel2(int Id)
        {
            var selectedRow = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 2 && m.LossCodesLevel1ID == Id), "LossCodeID", "LossCodeDesc");
            return Json(selectedRow, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetLevel3(int Id)
        {
            var selectedRow = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 3 && m.LossCodesLevel2ID == Id), "LossCodeID", "LossCodeDesc");
            return Json(selectedRow, JsonRequestBehavior.AllowGet);
        }

    }
}
