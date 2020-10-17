

using OfficeOpenXml;
using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace i_facility.Controllers
{
    public class BreakdownCodesController : Controller
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
            String Username = Session["Username"].ToString();

            var query = from c in db.tbllossescodes
                        where (!(from o in db.tbllossescodes
                                 where o.IsDeleted == 0 && o.MessageType == "BREAKDOWN"
                                 select o.LossCodesLevel1ID)
                        .Contains(c.LossCodeID))
                        &&
                                (!(from p in db.tbllossescodes
                                   where p.IsDeleted == 0 && p.MessageType == "BREAKDOWN"
                                   select p.LossCodesLevel2ID)
                                   .Contains(c.LossCodeID))
                        &&
                        c.IsDeleted == 0
                        && c.MessageType == "BREAKDOWN"
                        && c.LossCode != "9999"
                        orderby c.LossCodesLevel1ID, c.LossCodesLevel2ID
                        select c;
            return View(query.ToList());
        }

        public ActionResult Create()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            ViewData["Level1"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.MessageType == "BREAKDOWN" && m.LossCodeID != 9999), "LossCodeID", "LossCodeDesc");
            //keep level2 codes empty, based on level1 populate level2
            ViewData["Level2"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 4 && m.MessageType == "BREAKDOWN"), "LossCodeID", "LossCodeDesc");
            //ViewData["Level2"] = new SelectList(null);
            return View();
        }
        [HttpPost]
        public ActionResult Create(tbllossescode tlc, int Level1 = 0, int Level2 = 0)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            int UserID = Convert.ToInt32(Session["UserId"]);
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];


            tlc.CreatedBy = UserID;
            tlc.CreatedOn = DateTime.Now;
            tlc.IsDeleted = 0;
            tlc.MessageType = "BREAKDOWN";

            if (Convert.ToInt16(tlc.LossCodesLevel) == 2)
            {
                tlc.LossCodesLevel1ID = Level1;
                var duplosscode = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCode == tlc.LossCode && m.LossCodesLevel1ID == tlc.LossCodesLevel1ID).ToList();
                if (duplosscode.Count() == 0)
                {
                    //Here check if Level1 code is used in tbllossofentry, if so then create new copy of it with Different LossID.
                    var lossOfEntryData = db.tbllossofentries.Where(m => m.MessageCodeID == Level1).ToList();
                    if (lossOfEntryData.Count() > 0)
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
                        tlc.LossCodesLevel1ID = Level1LossCodeID;

                    }

                    db.tbllossescodes.Add(tlc);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "Breakdown code already Exist.";
                    ViewData["Level1"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.MessageType == "BREAKDOWN" && m.LossCodeID != 9999), "LossCodeID", "LossCode");
                    ViewData["Level2"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 2 && m.MessageType == "BREAKDOWN"), "LossCodeID", "LossCode");
                    return View(tlc);
                }
            }
            if (Convert.ToInt16(tlc.LossCodesLevel) == 3)
            {
                tlc.LossCodesLevel1ID = Level1;
                tlc.LossCodesLevel2ID = Level2;
                var duplosscode = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCode == tlc.LossCode && m.LossCodesLevel1ID == tlc.LossCodesLevel1ID && m.LossCodesLevel2ID == tlc.LossCodesLevel2ID).ToList();
                if (duplosscode.Count() == 0)
                {
                    //Here check if Level1 code is used in tbllossofentry, if so then create new copy of it with Different LossID.
                    var lossOfEntryData = db.tbllossofentries.Where(m => m.MessageCodeID == Level2).ToList();
                    if (lossOfEntryData.Count() > 0)
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
                        tlc.LossCodesLevel2ID = Level2LossCodeID;

                    }

                    db.tbllossescodes.Add(tlc);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "Breakdown code already Exist.";
                    ViewData["Level1"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.MessageType == "BREAKDOWN" && m.LossCodeID != 9999), "LossCodeID", "LossCode");
                    ViewData["Level2"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 2 && m.MessageType == "BREAKDOWN"), "LossCodeID", "LossCode");
                    return View(tlc);
                }
            }
            if (Convert.ToInt16(tlc.LossCodesLevel) == 1)
            {
                var duplosscode = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCode == tlc.LossCode).ToList();
                if (duplosscode.Count() == 0)
                {
                    db.tbllossescodes.Add(tlc);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "Breakdown code already Exist.";
                    ViewData["Level1"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.MessageType == "BREAKDOWN" && m.LossCodeID != 9999), "LossCodeID", "LossCode");
                    ViewData["Level2"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 2 && m.MessageType == "BREAKDOWN"), "LossCodeID", "LossCode");
                    return View(tlc);
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();

            tbllossescode lossdata = db.tbllossescodes.Find(id);

            ViewData["Level1"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.MessageType == "BREAKDOWN" && m.LossCodeID != 9999), "LossCodeID", "LossCode", lossdata.LossCodesLevel1ID);
            ViewData["Level2"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 2 && m.MessageType == "BREAKDOWN"), "LossCodeID", "LossCode", lossdata.LossCodesLevel2ID);
            ViewBag.radioselected = lossdata.LossCodesLevel;
            ViewData["ContributeTo"] = lossdata.ContributeTo;
            return View(lossdata);
        }
        [HttpPost]
        public ActionResult Edit(tbllossescode tlc, int Level1 = 0, int Level2 = 0)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            int UserID = Convert.ToInt32(Session["UserId"]);
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            tlc.ModifiedBy = UserID;
            tlc.ModifiedOn = DateTime.Now;

            if (Convert.ToInt16(tlc.LossCodesLevel) == 2)
            {
                tlc.LossCodesLevel1ID = Level1;
                var duplosscode = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCode == tlc.LossCode && m.LossCodeID != tlc.LossCodeID && m.LossCodesLevel1ID == tlc.LossCodesLevel1ID).ToList();
                if (duplosscode.Count() == 0)
                {
                    db.Entry(tlc).State =EntityState.Modified;
                    db.SaveChanges();
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "Loss code already Exist.";
                    ViewData["Level1"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.LossCodeID != 9999), "LossCodeID", "LossCode", tlc.LossCodesLevel1ID);
                    ViewData["Level2"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 2), "LossCodeID", "LossCode", tlc.LossCodesLevel2ID);
                    ViewBag.radioselected = tlc.LossCodesLevel;
                    return View(tlc);
                }
            }
            else if (Convert.ToInt16(tlc.LossCodesLevel) == 3)
            {
                tlc.LossCodesLevel1ID = Level1;
                tlc.LossCodesLevel2ID = Level2;
                var duplosscode = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCode == tlc.LossCode && m.LossCodeID != tlc.LossCodeID && m.LossCodesLevel1ID == tlc.LossCodesLevel1ID && m.LossCodesLevel2ID == tlc.LossCodesLevel2ID).ToList();
                if (duplosscode.Count == 0)
                {
                    db.Entry(tlc).State = EntityState.Modified;
                    db.SaveChanges();
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "Loss code already Exist.";
                    ViewData["Level1"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.LossCodeID != 9999), "LossCodeID", "LossCode", tlc.LossCodesLevel1ID);
                    ViewData["Level2"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 2), "LossCodeID", "LossCode", tlc.LossCodesLevel2ID);
                    ViewBag.radioselected = tlc.LossCodesLevel;

                    return View(tlc);
                }
            }
            else if (Convert.ToInt16(tlc.LossCodesLevel) == 1)
            {
                var duplosscode = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCode == tlc.LossCode && m.LossCodeID != tlc.LossCodeID).ToList();
                if (duplosscode.Count() == 0)
                {
                    db.Entry(tlc).State = EntityState.Modified;
                    db.SaveChanges();
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "Loss code already Exist.";
                    ViewData["Level1"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.LossCodeID != 9999), "LossCodeID", "LossCode", tlc.LossCodesLevel1ID);
                    ViewData["Level2"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 2), "LossCodeID", "LossCode", tlc.LossCodesLevel2ID);
                    ViewBag.radioselected = tlc.LossCodesLevel;
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
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            int UserID = Convert.ToInt32(Session["UserId"]);
            //ViewBag.IsConfigMenu = 0;
            tbllossescode tlc = db.tbllossescodes.Find(id);
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

        public JsonResult GetLevel2(int Id)
        {
            var selectedRow = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 2 && m.LossCodesLevel1ID == Id), "LossCodeID", "LossCode");
            return Json(selectedRow, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ModelData(int id)
        {
            //Format for each Button.
            //<input value="Edit" id="Update" name="Update" class="update btn orange btn-lg" style="width: 30%; display: inline-block; height: inherit; margin-top: 2%; background-color: orange; color: black; font-size: 95%" />

            string nothing = "An Error Occured";
            var tbdc = db.tbllossescodes.Find(id);
            // take this row's level and based on it prepare number of Buttons for Model Window.
            int myLevel = tbdc.LossCodesLevel;
            if (myLevel == 1)
            {
                nothing = "<a id= 'level1Click' href='/BreakdownCodes/Edit/" + id + "' class = 'blue btn-lg fa fa-edit'>level1</a>";
            }
            if (myLevel == 2)
            {
                int level1ID = Convert.ToInt32(tbdc.LossCodesLevel1ID);
                var tbdcLevel1Data = db.tbllossescodes.Find(level1ID);
                string code = tbdcLevel1Data.LossCodeID.ToString();
                nothing = "<a href='/BreakdownCodes/Edit/" + code + "' class = 'blue btn-lg fa fa-edit'>level1</a><div style='display:inline;width:2%'>.</div><a href='/BreakdownCodes/Edit/" + id + "' class = 'blue btn-lg fa fa-edit'>level2</a>";
            }
            if (myLevel == 3)
            {
                int level1ID = Convert.ToInt32(tbdc.LossCodesLevel1ID);
                var tbdcLevel1Data = db.tbllossescodes.Find(level1ID);
                string code1 = tbdcLevel1Data.LossCodeID.ToString();
                int level2ID = Convert.ToInt32(tbdc.LossCodesLevel2ID);
                var tbdcLevel2Data = db.tbllossescodes.Find(level2ID);
                string code2 = tbdcLevel2Data.LossCodeID.ToString();
                nothing = "<a href='/BreakdownCodes/Edit/" + code1 + "' class = 'blue btn-lg fa fa-edit'>level1</a><div style='display:inline;width:2%'>.</div><a href='/BreakdownCodes/Edit/" + code2 + "' class = 'blue btn-lg fa fa-edit'>level2</a><div style='display:inline;width:2%'>.</div><a href='/BreakdownCodes/Edit/" + id + "' class = 'blue btn-lg fa fa-edit'>level3</a>";
            }
            return Json(nothing, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportBreakdownCodesDetails()
        {

            #region Excel and Stuff

            DateTime frda = DateTime.Now;
            String FileDir = @"C:\TataReport\ReportsList\" + System.DateTime.Now.ToString("yyyy-MM-dd");
            bool exists = System.IO.Directory.Exists(FileDir);
            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "BreakdownList" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "BreakdownList" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //" to " + toda.ToString("yyyy-MM-dd") + 
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
            worksheet.Cells["A" + 1].Value = "Level1Code";
            worksheet.Cells["B" + 1].Value = "Level2Code";
            worksheet.Cells["C" + 1].Value = "Level3Code";
            worksheet.Cells["D" + 1].Value = "Level";
            worksheet.Cells["E" + 1].Value = "EndCode";
            worksheet.Cells["F" + 1].Value = "Code Desc";
            worksheet.Cells["G" + 1].Value = "Loss Type";
            //worksheet.Cells["H" +1].Value = "MessageType";
            //worksheet.Cells["I" +1].Value = "Contributes To";
            worksheet.Cells["A1:I1"].Style.Font.Bold = true;

            #endregion
            var BreakDownLossesData = db.tbllossescodes.Where(m => m.MessageType == "BREAKDOWN" && m.LossCodeID != 9999 && m.IsDeleted == 0).OrderBy(m => m.LossCodesLevel1ID).ThenBy(m => m.LossCodesLevel2ID).ToList();
            int i = 2;
            foreach (var row in BreakDownLossesData)
            {
                int losscodelevel = row.LossCodesLevel;
                if (losscodelevel == 1)
                {
                    worksheet.Cells["A" + i].Value = row.LossCode;
                    worksheet.Cells["D" + i].Value = losscodelevel;
                    worksheet.Cells["E" + i].Value = row.EndCode;
                    worksheet.Cells["F" + i].Value = row.LossCodeDesc;
                    worksheet.Cells["G" + i].Value = row.MessageType;
                    //worksheet.Cells["H" + i].Value = row.MessageType;
                    //worksheet.Cells["I" + i].Value = row.ContributeTo;
                }
                if (losscodelevel == 2)
                {
                    int level1ID = Convert.ToInt32(row.LossCodesLevel1ID);
                    string level1Code = Convert.ToString(db.tbllossescodes.Where(m => m.LossCodeID == level1ID).Select(m => m.LossCode).FirstOrDefault());

                    worksheet.Cells["A" + i].Value = level1Code;
                    worksheet.Cells["B" + i].Value = row.LossCode;
                    //worksheet.Cells["C" + i].Value = row.LossCode;
                    worksheet.Cells["D" + i].Value = losscodelevel;
                    worksheet.Cells["E" + i].Value = row.EndCode;
                    worksheet.Cells["F" + i].Value = row.LossCodeDesc;
                    worksheet.Cells["G" + i].Value = row.MessageType;
                    //worksheet.Cells["H" + i].Value = row.MessageType;
                    //worksheet.Cells["I" + i].Value = row.ContributeTo;
                }
                if (losscodelevel == 3)
                {
                    int level1ID = Convert.ToInt32(row.LossCodesLevel1ID);
                    string level1Code = Convert.ToString(db.tbllossescodes.Where(m => m.LossCodeID == level1ID).Select(m => m.LossCode).FirstOrDefault());
                    int level2ID = Convert.ToInt32(row.LossCodesLevel2ID);
                    string level2Code = Convert.ToString(db.tbllossescodes.Where(m => m.LossCodeID == level2ID).Select(m => m.LossCode).FirstOrDefault());

                    worksheet.Cells["A" + i].Value = level1Code;
                    worksheet.Cells["B" + i].Value = level2Code;
                    worksheet.Cells["C" + i].Value = row.LossCode;
                    worksheet.Cells["D" + i].Value = losscodelevel;
                    worksheet.Cells["E" + i].Value = row.EndCode;
                    worksheet.Cells["F" + i].Value = row.LossCodeDesc;
                    worksheet.Cells["G" + i].Value = row.MessageType;
                    //worksheet.Cells["H" + i].Value = row.MessageType;
                    //worksheet.Cells["I" + i].Value = row.ContributeTo;
                }
                i++;
            }

            #region Save and Download

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            p.Save();

            //Downloding Excel
            string path1 = System.IO.Path.Combine(FileDir, "BreakdownList" + frda.ToString("yyyy-MM-dd") + ".xlsx");
            System.IO.FileInfo file1 = new System.IO.FileInfo(path1);
            string Outgoingfile = "BreakdownList" + frda.ToString("yyyy-MM-dd") + ".xlsx";
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
