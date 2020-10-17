
using OfficeOpenXml;
using SRKSDemo;
using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace i_facility.Controllers
{
    public class HoldCodesController : Controller
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
        public ActionResult Index()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            var query = from c in db.tblholdcodes
                        where (!(from o in db.tblholdcodes
                                 where o.IsDeleted == 0
                                 select o.HoldCodesLevel1ID)
                        .Contains(c.HoldCodeID))
                        &&
                                (!(from p in db.tblholdcodes
                                   where p.IsDeleted == 0
                                   select p.HoldCodesLevel2ID)
                                   .Contains(c.HoldCodeID))
                        &&
                        c.IsDeleted == 0
                        orderby c.HoldCodesLevel1ID, c.HoldCodesLevel2ID
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
            ViewData["Level1"] = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 1), "HoldCodeID", "HoldCode");
            //keep level2 codes empty, based on level1 populate level2
            ViewData["Level2"] = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 4), "HoldCodeID", "HoldCode");
            //ViewData["Level2"] = new SelectList(null);
            return View();
        }
        [HttpPost]
        public ActionResult Create(tblholdcode tlc, int Level1 = 0, int Level2 = 0)
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

            ViewData["Level1"] = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 1), "HoldCodeID", "HoldCodeDesc");
            ViewData["Level2"] = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 2), "HoldCodeID", "HoldCodeDesc");

            if (Convert.ToInt16(tlc.HoldCodesLevel) == 1)
            {
                var duplosscode = db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCode == tlc.HoldCode).ToList();
                if (duplosscode.Count == 0)
                {
                    db.tblholdcodes.Add(tlc);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "Hold code already Exist.";
                    return View(tlc);
                }
            }
            if (Convert.ToInt16(tlc.HoldCodesLevel) == 2)
            {
                tlc.HoldCodesLevel1ID = Level1;
                var duplosscode = db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCode == tlc.HoldCode && m.HoldCodesLevel1ID == tlc.HoldCodesLevel1ID).ToList();
                if (duplosscode.Count == 0)
                {
                    //Here check if Level1 code is used in tbllossofentry, if so then create new copy of it with Different LossID.
                    var lossOfEntryData = db.tblmanuallossofentries.Where(m => m.MessageCodeID == Level1).ToList();
                    if (lossOfEntryData.Count > 0)
                    {
                        var lossPrvData = db.tblholdcodes.Where(m => m.HoldCodeID == Level1 && m.IsDeleted == 0).FirstOrDefault();

                        tblholdcode tlcNewPrvLevel = new tblholdcode();
                        tlcNewPrvLevel.ContributeTo = lossPrvData.ContributeTo;
                        tlcNewPrvLevel.CreatedBy = lossPrvData.CreatedBy;
                        tlcNewPrvLevel.CreatedOn = lossPrvData.CreatedOn;
                        tlcNewPrvLevel.IsDeleted = 0;
                        tlcNewPrvLevel.HoldCode = lossPrvData.HoldCode;
                        tlcNewPrvLevel.HoldCodeDesc = lossPrvData.HoldCodeDesc;
                        tlcNewPrvLevel.HoldCodesLevel = lossPrvData.HoldCodesLevel;
                        tlcNewPrvLevel.HoldCodesLevel1ID = lossPrvData.HoldCodesLevel1ID;
                        tlcNewPrvLevel.HoldCodesLevel2ID = lossPrvData.HoldCodesLevel2ID;
                        tlcNewPrvLevel.MessageType = lossPrvData.MessageType;
                        tlcNewPrvLevel.ModifiedBy = lossPrvData.ModifiedBy;
                        tlcNewPrvLevel.ModifiedOn = lossPrvData.ModifiedOn;

                        db.tblholdcodes.Add(tlcNewPrvLevel);
                        db.SaveChanges();

                        //Delete the Old one.
                        lossPrvData.IsDeleted = 1;
                        lossPrvData.ModifiedOn = DateTime.Now;
                        lossPrvData.ModifiedBy = UserID;
                        db.Entry(lossPrvData).State = EntityState.Modified;
                        db.SaveChanges();

                        //Give new Level1 HoldCodeID to 2nd level code.
                        int Level1LossCodeID = tlcNewPrvLevel.HoldCodeID;
                        tlc.HoldCodesLevel1ID = Level1LossCodeID;

                    }
                    db.tblholdcodes.Add(tlc);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "Hold code already Exist.";
                    return View(tlc);
                }
            }
            if (Convert.ToInt16(tlc.HoldCodesLevel) == 3)
            {
                tlc.HoldCodesLevel1ID = Level1;
                tlc.HoldCodesLevel2ID = Level2;
                var duplosscode = db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCode == tlc.HoldCode && m.HoldCodesLevel1ID == tlc.HoldCodesLevel1ID && m.HoldCodesLevel2ID == tlc.HoldCodesLevel2ID).ToList();
                if (duplosscode.Count == 0)
                {
                    //Here check if Level1 code is used in tbllossofentry, if so then create new copy of it with Different LossID.
                    var lossOfEntryData = db.tbllossofentries.Where(m => m.MessageCodeID == Level2).ToList();
                    if (lossOfEntryData.Count > 0)
                    {
                        var lossPrvData = db.tblholdcodes.Where(m => m.HoldCodeID == Level2 && m.IsDeleted == 0).FirstOrDefault();

                        tblholdcode tlcNewPrvLevel = new tblholdcode();
                        tlcNewPrvLevel.ContributeTo = lossPrvData.ContributeTo;
                        tlcNewPrvLevel.CreatedBy = lossPrvData.CreatedBy;
                        tlcNewPrvLevel.CreatedOn = lossPrvData.CreatedOn;
                        tlcNewPrvLevel.IsDeleted = 0;
                        tlcNewPrvLevel.HoldCode = lossPrvData.HoldCode;
                        tlcNewPrvLevel.HoldCodeDesc = lossPrvData.HoldCodeDesc;
                        tlcNewPrvLevel.HoldCodesLevel = lossPrvData.HoldCodesLevel;
                        tlcNewPrvLevel.HoldCodesLevel1ID = lossPrvData.HoldCodesLevel1ID;
                        tlcNewPrvLevel.HoldCodesLevel2ID = lossPrvData.HoldCodesLevel2ID;
                        tlcNewPrvLevel.MessageType = lossPrvData.MessageType;
                        tlcNewPrvLevel.ModifiedBy = lossPrvData.ModifiedBy;
                        tlcNewPrvLevel.ModifiedOn = lossPrvData.ModifiedOn;

                        db.tblholdcodes.Add(tlcNewPrvLevel);
                        db.SaveChanges();

                        //Delete the Old one.
                        lossPrvData.IsDeleted = 1;
                        lossPrvData.ModifiedOn = DateTime.Now;
                        lossPrvData.ModifiedBy = UserID;
                        db.Entry(lossPrvData).State = EntityState.Modified;
                        db.SaveChanges();

                        //Give new Level1 HoldCodeID to 2nd level code.
                        int Level2LossCodeID = tlcNewPrvLevel.HoldCodeID;
                        tlc.HoldCodesLevel2ID = Level2LossCodeID;

                    }

                    db.tblholdcodes.Add(tlc);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "Hold code already Exist.";
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
            tblholdcode lossdata = db.tblholdcodes.Find(id);

            ViewData["Level1"] = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 1 && m.HoldCode != "999" && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "HoldCodeID", "HoldCodeDesc", lossdata.HoldCodesLevel1ID);
            ViewData["Level2"] = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 2 && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "HoldCodeID", "HoldCodeDesc", lossdata.HoldCodesLevel2ID);
            ViewBag.radioselected = lossdata.HoldCodesLevel;
            ViewData["MessageType"] = lossdata.MessageType;
            ViewData["ContributeTo"] = lossdata.ContributeTo;
            return View(lossdata);
        }
        [HttpPost]
        public ActionResult Edit(tblholdcode tlc, int Level1 = 0, int Level2 = 0)
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

            if (Convert.ToInt16(tlc.HoldCodesLevel) == 1)
            {
                var duplosscode = db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCode == tlc.HoldCode && m.HoldCodeID != tlc.HoldCodeID).ToList();
                if (duplosscode.Count == 0)
                {
                    db.Entry(tlc).State = EntityState.Modified;
                    db.SaveChanges();
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "Hold code already Exist.";
                    ViewData["Level1"] = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 1 && m.HoldCode != "999" && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "HoldCodeID", "HoldCodeDesc", tlc.HoldCodesLevel1ID);
                    ViewData["Level2"] = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 2 && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "HoldCodeID", "HoldCodeDesc", tlc.HoldCodesLevel2ID);
                    ViewBag.radioselected = tlc.HoldCodesLevel;

                    return View(tlc);
                }
            }

            if (Convert.ToInt16(tlc.HoldCodesLevel) == 2)
            {
                tlc.HoldCodesLevel1ID = Level1;
                var duplosscode = db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCode == tlc.HoldCode && m.HoldCodeID != tlc.HoldCodeID && m.HoldCodesLevel1ID == tlc.HoldCodesLevel1ID).ToList();
                if (duplosscode.Count == 0)
                {
                    db.Entry(tlc).State = EntityState.Modified;
                    db.SaveChanges();
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "Hold code already Exist.";
                    ViewData["Level1"] = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 1 && m.HoldCode != "999" && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "HoldCodeID", "HoldCodeDesc", tlc.HoldCodesLevel1ID);
                    ViewData["Level2"] = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 2 && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "HoldCodeID", "HoldCodeDesc", tlc.HoldCodesLevel2ID);
                    ViewBag.radioselected = tlc.HoldCodesLevel;

                    return View(tlc);
                }
            }
            if (Convert.ToInt16(tlc.HoldCodesLevel) == 3)
            {
                tlc.HoldCodesLevel1ID = Level1;
                tlc.HoldCodesLevel2ID = Level2;
                var duplosscode = db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCode == tlc.HoldCode && m.HoldCodeID != tlc.HoldCodeID && m.HoldCodesLevel1ID == tlc.HoldCodesLevel1ID && m.HoldCodesLevel2ID == tlc.HoldCodesLevel2ID).ToList();
                if (duplosscode.Count == 0)
                {
                    db.Entry(tlc).State = EntityState.Modified;
                    db.SaveChanges();
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "Hold code already Exist.";
                    ViewData["Level1"] = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 1 && m.HoldCode != "999" && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "HoldCodeID", "HoldCodeDesc", tlc.HoldCodesLevel1ID);
                    ViewData["Level2"] = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 2 && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "HoldCodeID", "HoldCodeDesc", tlc.HoldCodesLevel2ID);
                    ViewBag.radioselected = tlc.HoldCodesLevel;

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
            int UserID1 = id;
            //ViewBag.IsConfigMenu = 0;
            tblholdcode tlc = db.tblholdcodes.Find(id);
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
        //public JsonResult GetLevel1(int Id)
        //{
        //    var selectedRow = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 1), "HoldCodeID", "HoldCodeDesc");
        //    return Json(selectedRow, JsonRequestBehavior.AllowGet);
        //}

        public JsonResult GetLevel2(int Id)
        {
            var selectedRow = new SelectList(db.tblholdcodes.Where(m => m.IsDeleted == 0 && m.HoldCodesLevel == 2 && m.HoldCodesLevel1ID == Id), "HoldCodeID", "HoldCode");
            return Json(selectedRow, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ImportHoldCodes()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            return View();
        }
        [HttpPost]
        //public ActionResult ImportMasterPartsstsw(HttpPostedFileBase file,string UploadType)
        public ActionResult ImportHoldCodes(HttpPostedFileBase file, string UploadType)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            int UserID = Convert.ToInt32(Session["UserId"]);
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            //Deleting Excel file
            #region
            string fileLocation1 = Server.MapPath("~/Content/");
            DirectoryInfo di = new DirectoryInfo(fileLocation1);
            FileInfo[] files = di.GetFiles("*.xlsx").Where(p => p.Extension == ".xlsx").ToArray();
            foreach (FileInfo file1 in files)
                try
                {
                    file1.Attributes = FileAttributes.Normal;
                    System.IO.File.Delete(file1.FullName);
                }
                catch { }
            #endregion

            DataSet ds = new DataSet();
            if (Request.Files["file"].ContentLength > 0)
            {
                string fileExtension = System.IO.Path.GetExtension(Request.Files["file"].FileName);
                if (fileExtension == ".xls" || fileExtension == ".xlsx")
                {
                    string fileLocation = Server.MapPath("~/Content/") + Request.Files["file"].FileName;
                    if (System.IO.File.Exists(fileLocation))
                    {
                        System.IO.File.Delete(fileLocation);
                    }
                    Request.Files["file"].SaveAs(fileLocation);
                    string excelConnectionString = string.Empty;
                    excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                    fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    //connection String for xls file format.
                    if (fileExtension == ".xls")
                    {
                        excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
                        fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                    }
                    //connection String for xlsx file format.
                    else if (fileExtension == ".xlsx")
                    {
                        excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                        fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    }
                    //Create Connection to Excel work book and add oledb namespace
                    OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                    excelConnection.Open();
                    DataTable dt = new DataTable();
                    dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    if (dt == null)
                    {
                        return null;
                    }
                    String[] excelSheets = new String[dt.Rows.Count];
                    int t = 0;
                    //excel data saves in temp file here.
                    foreach (DataRow row in dt.Rows)
                    {
                        excelSheets[t] = row["TABLE_NAME"].ToString();
                        t++;
                    }
                    OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);
                    string query = string.Format("Select * from [{0}]", excelSheets[0]);
                    using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                    {
                        dataAdapter.Fill(ds);
                    }
                    excelConnection1.Close();
                    excelConnection.Close();
                }
                if (fileExtension.ToString().ToLower().Equals(".xml"))
                {
                    string fileLocation = Server.MapPath("~/Content/") + Request.Files["FileUpload"].FileName;
                    if (System.IO.File.Exists(fileLocation))
                    {
                        System.IO.File.Delete(fileLocation);
                    }
                    Request.Files["FileUpload"].SaveAs(fileLocation);
                    XmlTextReader xmlreader = new XmlTextReader(fileLocation);
                    // DataSet ds = new DataSet();
                    ds.ReadXml(xmlreader);
                    xmlreader.Close();
                }

                string Errors = null;
                if (UploadType == "OverWrite") // Accept only New Codes
                {
                    #region
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        string a = ds.Tables[0].Rows[i][0].ToString();

                        bool IsValid = false;
                        //Validations
                        int level = 0;
                        int.TryParse(ds.Tables[0].Rows[i][3].ToString(), out level);
                        if (level == 1)
                        {
                            //1st Check for Empty
                            //Check if it already exits , else insert.
                            #region
                            string codename = Convert.ToString(ds.Tables[0].Rows[i][0]);
                            string codedesc = Convert.ToString(ds.Tables[0].Rows[i][4]);
                            if (!string.IsNullOrEmpty(codename) && !string.IsNullOrEmpty(codedesc))
                            {
                                var DupData = db.tblholdcodes.Where(m => m.HoldCode == codename && m.IsDeleted == 0).FirstOrDefault();
                                if (DupData != null)
                                {
                                    Errors += "Duplicate Code : " + DupData.HoldCode;
                                    continue;
                                }
                                else if (codename.Length > 40 || codedesc.Length > 60)
                                {
                                    Errors += "LossCodez (MaxLength:40) and LossDesc (MaxLength:60)  " + DupData.HoldCode;
                                    continue;
                                }
                                else
                                {
                                    string EndCode = Convert.ToString(ds.Tables[0].Rows[i][5]).Trim();
                                    if (EndCode.Length > 0 && !Regex.IsMatch(EndCode, @"^\d+$"))
                                    {
                                        Errors += "EndCode for " + codename + " must be Integer.";
                                        continue;
                                    }

                                    MsqlConnection mc1 = new MsqlConnection();
                                    mc1.open();
                                    string dat = DateTime.Now.ToString();
                                    dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    SqlCommand cmd2 = null;
                                    try
                                    {
                                        string HoldCodesLevel1ID = null;
                                        string GWCodesLevel2ID = null;
                                        string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                        if (Regex.IsMatch(EndCode, @"^\d+$"))
                                        {
                                            cmd2 = new SqlCommand("INSERT INTO tblholdcodes (`HoldCode`,`HoldCodeDesc`,`HoldCodesLevel`,`IsDeleted`,`CreatedOn`,`CreatedBy`,`EndCode`) VALUES( '" + codename + "' , '" + codedesc + "' , " + level + "  , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "', '" + EndCode + "')", mc1.msqlConnection);
                                        }
                                        else
                                        {
                                            cmd2 = new SqlCommand("INSERT INTO tblholdcodes (`HoldCode`,`HoldCodeDesc`,`HoldCodesLevel`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' ,  " + level + " , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
                                        }
                                        cmd2.ExecuteNonQuery();
                                    }
                                    catch (Exception e)
                                    {
                                        Errors += e.ToString();
                                    }
                                    mc1.close();
                                }
                            }
                            else
                            {
                                Errors += "Please Enter both Code Name and Description for" + codename + " " + codedesc;
                                continue;
                            }
                            #endregion
                        }
                        else if (level == 2)
                        {
                            //1st Check for Empty
                            //Check if it already exits , else insert.
                            #region
                            string codename = Convert.ToString(ds.Tables[0].Rows[i][1]);
                            string codedesc = Convert.ToString(ds.Tables[0].Rows[i][4]);

                            if (!string.IsNullOrEmpty(codename) && !string.IsNullOrEmpty(codedesc))
                            {
                                var DupData = db.tblholdcodes.Where(m => m.HoldCode == codename).Where(m => m.IsDeleted == 0).FirstOrDefault();
                                if (DupData != null)
                                {
                                    Errors += "Duplicate Code : " + DupData.HoldCode;
                                    continue;
                                }
                                else if (codename.Length > 40 || codedesc.Length > 60)
                                {
                                    Errors += "LossCodez (MaxLength:40) and LossDesc (MaxLength:60)  " + DupData.HoldCode;
                                    continue;
                                }
                                else
                                {
                                    string Level1Code = Convert.ToString(ds.Tables[0].Rows[i][0]);

                                    //Is Level1 code Valid.
                                    if (!string.IsNullOrEmpty(Level1Code))
                                    {
                                        var Level1Data = db.tblholdcodes.Where(m => m.HoldCode == Level1Code && m.IsDeleted == 0).FirstOrDefault();
                                        if (Level1Data != null)
                                        {
                                            int HoldCodesLevel1ID = Level1Data.HoldCodeID;
                                            if (Level1Data.HoldCodesLevel != 1)
                                            {
                                                Errors += "InValid Level1 Code for " + codename + " ";
                                                continue;
                                            }
                                            else
                                            {
                                                MsqlConnection mc1 = new MsqlConnection();
                                                mc1.open();
                                                string dat = DateTime.Now.ToString();
                                                dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                SqlCommand cmd2 = null;
                                                try
                                                {
                                                    string EndCode = Convert.ToString(ds.Tables[0].Rows[i][5]).Trim();
                                                    if (EndCode.Length > 0 && !Regex.IsMatch(EndCode, @"^\d+$"))
                                                    {
                                                        Errors += "EndCode for " + codename + " must be Integer.";
                                                        continue;
                                                    }

                                                    string HoldCodesLevel2ID = null;
                                                    string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                                    if (Regex.IsMatch(EndCode, @"^\d+$"))
                                                    {
                                                        cmd2 = new SqlCommand("INSERT INTO tblholdcodes (`HoldCode`,`HoldCodeDesc`,`HoldCodesLevel`,`HoldCodesLevel1ID`,`IsDeleted`,`CreatedOn`,`CreatedBy`,`EndCode`) VALUES( '" + codename + "' , '" + codedesc + "' , " + level + " ,'" + HoldCodesLevel1ID + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "', '" + EndCode + "')", mc1.msqlConnection);
                                                    }
                                                    else
                                                    {
                                                        cmd2 = new SqlCommand("INSERT INTO tblholdcodes (`HoldCode`,`HoldCodeDesc`,`HoldCodesLevel`,`HoldCodesLevel1ID`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , " + level + " ,'" + HoldCodesLevel1ID + "', 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
                                                    }

                                                    cmd2.ExecuteNonQuery();
                                                }
                                                catch (Exception e)
                                                {
                                                    Errors = e.ToString();
                                                }
                                                mc1.close();
                                            }
                                        }
                                        else
                                        {
                                            Errors += "Level1 Code for " + codename + " doesn't exist.";
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        Errors += "Level1 Code for " + codename + "cannot be null";
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                Errors += "Please Enter both Code Name and Description ";
                                continue;
                            }
                            #endregion
                        }
                        else if (level == 3)
                        {
                            //1st Check for Empty
                            //Check if it already exits , else insert.
                            #region
                            string codename = Convert.ToString(ds.Tables[0].Rows[i][2]);
                            string codedesc = Convert.ToString(ds.Tables[0].Rows[i][4]);

                            if (!string.IsNullOrEmpty(codename) && !string.IsNullOrEmpty(codedesc))
                            {
                                var DupData = db.tblholdcodes.Where(m => m.HoldCode == codename).Where(m => m.IsDeleted == 0).FirstOrDefault();
                                if (DupData != null)
                                {
                                    Errors += "Duplicate Code : " + DupData.HoldCode;
                                    continue;
                                }
                                else if (codename.Length > 40 || codedesc.Length > 60)
                                {
                                    Errors += "HoldCodez (MaxLength:40) and LossDesc (MaxLength:60)  " + DupData.HoldCode;
                                    continue;
                                }
                                else
                                {
                                    string Level1Code = Convert.ToString(ds.Tables[0].Rows[i][0]);
                                    string Level2Code = Convert.ToString(ds.Tables[0].Rows[i][1]);

                                    //Is Level1 code Valid.
                                    if (!string.IsNullOrEmpty(Level1Code) && !string.IsNullOrEmpty(Level2Code))
                                    {
                                        var Level1Data = db.tblholdcodes.Where(m => m.HoldCode == Level1Code && m.IsDeleted == 0).FirstOrDefault();
                                        var Level2Data = db.tblholdcodes.Where(m => m.HoldCode == Level2Code && m.IsDeleted == 0).FirstOrDefault();

                                        if (Level1Data != null && Level2Data != null)
                                        {
                                            if (Level1Data.HoldCodesLevel != 1)
                                            {
                                                Errors += "InValid Level1 Code for " + codename + " ";
                                                continue;
                                            }
                                            if (Level2Data.HoldCodesLevel != 2)
                                            {
                                                Errors += "InValid Level2 Code for " + codename + " ";
                                                continue;
                                            }

                                            //Level 1 && 2 are not relevant to each other.
                                            var Level2sLevel1 = Level2Data.HoldCodesLevel1ID;
                                            if (Level2sLevel1 == Level1Data.HoldCodeID)
                                            {
                                                int HoldCodesLevel1ID = Level1Data.HoldCodeID;
                                                int HoldCodesLevel2ID = Level2Data.HoldCodeID;
                                                MsqlConnection mc1 = new MsqlConnection();
                                                mc1.open();
                                                string dat = DateTime.Now.ToString();
                                                dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                SqlCommand cmd2 = null;
                                                try
                                                {
                                                    string EndCode = Convert.ToString(ds.Tables[0].Rows[i][5]).Trim();
                                                    if (EndCode.Length > 0 && !Regex.IsMatch(EndCode, @"^\d+$"))
                                                    {
                                                        Errors += "EndCode for " + codename + " must be Integer.";
                                                        continue;
                                                    }
                                                    string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                    if (Regex.IsMatch(EndCode, @"^\d+$"))
                                                    {
                                                        cmd2 = new SqlCommand("INSERT INTO tblholdcodes (`HoldCode`,`HoldCodeDesc`,`HoldCodesLevel`,`HoldCodesLevel1ID`,`HoldCodesLevel2ID`,`IsDeleted`,`CreatedOn`,`CreatedBy`,`EndCode`) VALUES( '" + codename + "' , '" + codedesc + "'  , " + level + " ,'" + HoldCodesLevel1ID + "','" + HoldCodesLevel2ID + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "', '" + EndCode + "')", mc1.msqlConnection);
                                                    }
                                                    else
                                                    {
                                                        cmd2 = new SqlCommand("INSERT INTO tblholdcodes (`HoldCode`,`HoldCodeDesc`,`HoldCodesLevel`,`HoldCodesLevel1ID`,`HoldCodesLevel2ID`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , " + level + " ,'" + HoldCodesLevel1ID + "' ,'" + HoldCodesLevel2ID + "', 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
                                                    }
                                                    cmd2.ExecuteNonQuery();
                                                }
                                                catch (Exception e)
                                                {
                                                    Errors = e.ToString();
                                                }
                                                mc1.close();
                                            }
                                            else
                                            {
                                                Errors += "Hierarchy of LossCodes for " + codename + " doesn't exist.";
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            Errors += "Level1Code or Level2Code for " + codename + " doesn't exist.";
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        Errors += "Level1Code and Level2Code " + codename + "cannot be null";
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                Errors += "Please Enter both Code Name and Description ";
                                continue;
                            }
                            #endregion
                        }
                        else
                        {
                            Errors += "Please enter Level ";
                            continue;
                        }

                    }
                    #endregion
                }
                else if (UploadType == "New") // Delete Duplicate and Insert New. // if not Duplicate insert that
                {
                    #region
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        string a = ds.Tables[0].Rows[i][0].ToString();

                        bool IsValid = false;
                        //Validations
                        int level = 0;
                        int.TryParse(ds.Tables[0].Rows[i][3].ToString(), out level);
                        if (level == 1)
                        {
                            //1st Check for Empty
                            //Check if it already exits , else insert.
                            #region
                            string codename = Convert.ToString(ds.Tables[0].Rows[i][0]);
                            string codedesc = Convert.ToString(ds.Tables[0].Rows[i][4]);
                            if (!string.IsNullOrEmpty(codename) && !string.IsNullOrEmpty(codedesc))
                            {
                                //var DupData = db.tblholdcodes.Where(m => m.HoldCode == codename && m.IsDeleted == 0).FirstOrDefault();
                                //if (DupData != null)
                                //{
                                //    Errors += "Duplicate Code : " + DupData.HoldCode;
                                //    continue;
                                //}
                                //else 
                                if (codename.Length > 40 || codedesc.Length > 60)
                                {
                                    Errors += "LossCodez (MaxLength:40) and LossDesc (MaxLength:60)  " + codename;
                                    continue;
                                }
                                else
                                {
                                    string EndCode = Convert.ToString(ds.Tables[0].Rows[i][5]).Trim();
                                    if (EndCode.Length > 0 && !Regex.IsMatch(EndCode, @"^\d+$"))
                                    {
                                        Errors += "EndCode for " + codename + " must be Integer.";
                                        continue;
                                    }

                                    var DupData = db.tblholdcodes.Where(m => m.HoldCode == codename && m.IsDeleted == 0).FirstOrDefault();
                                    if (DupData != null)
                                    {
                                        DupData.IsDeleted = 1;
                                        DupData.DeletedDate = DateTime.Now;
                                        db.Entry(DupData).State = EntityState.Modified;
                                        db.SaveChanges();
                                    }

                                    MsqlConnection mc1 = new MsqlConnection();
                                    mc1.open();
                                    string dat = DateTime.Now.ToString();
                                    dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    SqlCommand cmd2 = null;
                                    try
                                    {
                                        string HoldCodesLevel1ID = null;
                                        string GWCodesLevel2ID = null;
                                        string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                        if (Regex.IsMatch(EndCode, @"^\d+$"))
                                        {
                                            cmd2 = new SqlCommand("INSERT INTO tblholdcodes (`HoldCode`,`HoldCodeDesc`,`HoldCodesLevel`,`IsDeleted`,`CreatedOn`,`CreatedBy`,`EndCode`) VALUES( '" + codename + "' , '" + codedesc + "' , " + level + "  , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "', '" + EndCode + "')", mc1.msqlConnection);
                                        }
                                        else
                                        {
                                            cmd2 = new SqlCommand("INSERT INTO tblholdcodes (`HoldCode`,`HoldCodeDesc`,`HoldCodesLevel`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' ,  " + level + " , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
                                        }
                                        cmd2.ExecuteNonQuery();
                                    }
                                    catch (Exception e)
                                    {
                                        Errors += e.ToString();
                                    }
                                    mc1.close();
                                }
                            }
                            else
                            {
                                Errors += "Please Enter both Code Name and Description for" + codename + " " + codedesc;
                                continue;
                            }
                            #endregion
                        }
                        else if (level == 2)
                        {
                            //1st Check for Empty
                            //Check if it already exits , else insert.
                            #region
                            string codename = Convert.ToString(ds.Tables[0].Rows[i][1]);
                            string codedesc = Convert.ToString(ds.Tables[0].Rows[i][4]);

                            if (!string.IsNullOrEmpty(codename) && !string.IsNullOrEmpty(codedesc))
                            {
                                //var DupData = db.tblholdcodes.Where(m => m.HoldCode == codename).Where(m => m.IsDeleted == 0).FirstOrDefault();
                                //if (DupData != null)
                                //{
                                //    Errors += "Duplicate Code : " + DupData.HoldCode;
                                //    continue;
                                //}
                                //else 
                                if (codename.Length > 40 || codedesc.Length > 60)
                                {
                                    Errors += "LossCodez (MaxLength:40) and LossDesc (MaxLength:60)  " + codename;
                                    continue;
                                }
                                else
                                {
                                    string Level1Code = Convert.ToString(ds.Tables[0].Rows[i][0]);

                                    //Is Level1 code Valid.
                                    if (!string.IsNullOrEmpty(Level1Code))
                                    {
                                        var Level1Data = db.tblholdcodes.Where(m => m.HoldCode == Level1Code && m.IsDeleted == 0).FirstOrDefault();
                                        if (Level1Data != null)
                                        {
                                            int HoldCodesLevel1ID = Level1Data.HoldCodeID;
                                            if (Level1Data.HoldCodesLevel != 1)
                                            {
                                                Errors += "InValid Level1 Code for " + codename + " ";
                                                continue;
                                            }
                                            else
                                            {
                                                MsqlConnection mc1 = new MsqlConnection();
                                                mc1.open();
                                                string dat = DateTime.Now.ToString();
                                                dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                SqlCommand cmd2 = null;
                                                try
                                                {
                                                    string EndCode = Convert.ToString(ds.Tables[0].Rows[i][5]).Trim();
                                                    if (EndCode.Length > 0 && !Regex.IsMatch(EndCode, @"^\d+$"))
                                                    {
                                                        Errors += "EndCode for " + codename + " must be Integer.";
                                                        continue;
                                                    }

                                                    var DupData = db.tblholdcodes.Where(m => m.HoldCode == codename && m.IsDeleted == 0 && m.HoldCodesLevel1ID == HoldCodesLevel1ID).Where(m => m.IsDeleted == 0).FirstOrDefault();
                                                    if (DupData != null)
                                                    {
                                                        DupData.IsDeleted = 1;
                                                        DupData.DeletedDate = DateTime.Now;
                                                        db.Entry(DupData).State = EntityState.Modified;
                                                        db.SaveChanges();
                                                    }

                                                    string HoldCodesLevel2ID = null;
                                                    string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                                    if (Regex.IsMatch(EndCode, @"^\d+$"))
                                                    {
                                                        cmd2 = new SqlCommand("INSERT INTO tblholdcodes (`HoldCode`,`HoldCodeDesc`,`HoldCodesLevel`,`HoldCodesLevel1ID`,`IsDeleted`,`CreatedOn`,`CreatedBy`,`EndCode`) VALUES( '" + codename + "' , '" + codedesc + "' , " + level + " ,'" + HoldCodesLevel1ID + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "', '" + EndCode + "')", mc1.msqlConnection);
                                                    }
                                                    else
                                                    {
                                                        cmd2 = new SqlCommand("INSERT INTO tblholdcodes (`HoldCode`,`HoldCodeDesc`,`HoldCodesLevel`,`HoldCodesLevel1ID`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , " + level + " ,'" + HoldCodesLevel1ID + "', 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
                                                    }

                                                    cmd2.ExecuteNonQuery();
                                                }
                                                catch (Exception e)
                                                {
                                                    Errors = e.ToString();
                                                }
                                                mc1.close();
                                            }
                                        }
                                        else
                                        {
                                            Errors += "Level1 Code for " + codename + " doesn't exist.";
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        Errors += "Level1 Code for " + codename + "cannot be null";
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                Errors += "Please Enter both Code Name and Description ";
                                continue;
                            }
                            #endregion
                        }
                        else if (level == 3)
                        {
                            //1st Check for Empty
                            //Check if it already exits , else insert.
                            #region
                            string codename = Convert.ToString(ds.Tables[0].Rows[i][2]);
                            string codedesc = Convert.ToString(ds.Tables[0].Rows[i][4]);

                            if (!string.IsNullOrEmpty(codename) && !string.IsNullOrEmpty(codedesc))
                            {
                                //var DupData = db.tblholdcodes.Where(m => m.HoldCode == codename).Where(m => m.IsDeleted == 0).FirstOrDefault();
                                //if (DupData != null)
                                //{
                                //    Errors += "Duplicate Code : " + DupData.HoldCode;
                                //    continue;
                                //}
                                //else 
                                if (codename.Length > 40 || codedesc.Length > 60)
                                {
                                    Errors += "HoldCodez (MaxLength:40) and LossDesc (MaxLength:60)  " + codename;
                                    continue;
                                }
                                else
                                {
                                    string Level1Code = Convert.ToString(ds.Tables[0].Rows[i][0]);
                                    string Level2Code = Convert.ToString(ds.Tables[0].Rows[i][1]);

                                    //Is Level1 code Valid.
                                    if (!string.IsNullOrEmpty(Level1Code) && !string.IsNullOrEmpty(Level2Code))
                                    {
                                        var Level1Data = db.tblholdcodes.Where(m => m.HoldCode == Level1Code && m.IsDeleted == 0).FirstOrDefault();
                                        var Level2Data = db.tblholdcodes.Where(m => m.HoldCode == Level2Code && m.IsDeleted == 0).FirstOrDefault();

                                        if (Level1Data != null && Level2Data != null)
                                        {
                                            if (Level1Data.HoldCodesLevel != 1)
                                            {
                                                Errors += "InValid Level1 Code for " + codename + " ";
                                                continue;
                                            }
                                            if (Level2Data.HoldCodesLevel != 2)
                                            {
                                                Errors += "InValid Level2 Code for " + codename + " ";
                                                continue;
                                            }

                                            //Level 1 && 2 are not relevant to each other.
                                            var Level2sLevel1 = Level2Data.HoldCodesLevel1ID;
                                            if (Level2sLevel1 == Level1Data.HoldCodeID)
                                            {
                                                int HoldCodesLevel1ID = Level1Data.HoldCodeID;
                                                int HoldCodesLevel2ID = Level2Data.HoldCodeID;
                                                MsqlConnection mc1 = new MsqlConnection();
                                                mc1.open();
                                                string dat = DateTime.Now.ToString();
                                                dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                SqlCommand cmd2 = null;
                                                try
                                                {
                                                    string EndCode = Convert.ToString(ds.Tables[0].Rows[i][5]).Trim();
                                                    if (EndCode.Length > 0 && !Regex.IsMatch(EndCode, @"^\d+$"))
                                                    {
                                                        Errors += "EndCode for " + codename + " must be Integer.";
                                                        continue;
                                                    }
                                                    string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                                    var DupData = db.tblholdcodes.Where(m => m.HoldCode == codename && m.IsDeleted == 0 && m.HoldCodesLevel1ID == HoldCodesLevel1ID && m.HoldCodesLevel2ID == HoldCodesLevel2ID).FirstOrDefault();
                                                    if (DupData != null)
                                                    {
                                                        DupData.IsDeleted = 1;
                                                        DupData.DeletedDate = DateTime.Now;
                                                        db.Entry(DupData).State = EntityState.Modified;
                                                        db.SaveChanges();
                                                    }
                                                    if (Regex.IsMatch(EndCode, @"^\d+$"))
                                                    {
                                                        cmd2 = new SqlCommand("INSERT INTO tblholdcodes (`HoldCode`,`HoldCodeDesc`,`HoldCodesLevel`,`HoldCodesLevel1ID`,`HoldCodesLevel2ID`,`IsDeleted`,`CreatedOn`,`CreatedBy`,`EndCode`) VALUES( '" + codename + "' , '" + codedesc + "'  , " + level + " ,'" + HoldCodesLevel1ID + "','" + HoldCodesLevel2ID + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "', '" + EndCode + "')", mc1.msqlConnection);
                                                    }
                                                    else
                                                    {
                                                        cmd2 = new SqlCommand("INSERT INTO tblholdcodes (`HoldCode`,`HoldCodeDesc`,`HoldCodesLevel`,`HoldCodesLevel1ID`,`HoldCodesLevel2ID`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , " + level + " ,'" + HoldCodesLevel1ID + "' ,'" + HoldCodesLevel2ID + "', 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
                                                    }
                                                    cmd2.ExecuteNonQuery();
                                                }
                                                catch (Exception e)
                                                {
                                                    Errors = e.ToString();
                                                }
                                                mc1.close();
                                            }
                                            else
                                            {
                                                Errors += "Hierarchy of LossCodes for " + codename + " doesn't exist.";
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            Errors += "Level1Code or Level2Code for " + codename + " doesn't exist.";
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        Errors += "Level1Code and Level2Code " + codename + "cannot be null";
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                Errors += "Please Enter both Code Name and Description ";
                                continue;
                            }
                            #endregion
                        }
                        else
                        {
                            Errors += "Please enter Level ";
                            continue;
                        }

                    }
                    #endregion
                }
                else if (UploadType == "Update") // OverWrite Existing Values 
                {
                    #region
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        string a = ds.Tables[0].Rows[i][0].ToString();

                        bool IsValid = false;
                        //Validations
                        int level = 0;
                        int.TryParse(ds.Tables[0].Rows[i][3].ToString(), out level);
                        if (level == 1)
                        {
                            //1st Check for Empty
                            //Check if it already exits , else insert.
                            #region
                            string codename = Convert.ToString(ds.Tables[0].Rows[i][0]);
                            string codedesc = Convert.ToString(ds.Tables[0].Rows[i][4]);
                            if (!string.IsNullOrEmpty(codename) && !string.IsNullOrEmpty(codedesc))
                            {
                                //var DupData = db.tblholdcodes.Where(m => m.HoldCode == codename && m.IsDeleted == 0).FirstOrDefault();
                                //if (DupData != null)
                                //{
                                //    Errors += "Duplicate Code : " + DupData.HoldCode;
                                //    continue;
                                //}
                                //else 
                                if (codename.Length > 40 || codedesc.Length > 60)
                                {
                                    Errors += "LossCodez (MaxLength:40) and LossDesc (MaxLength:60)  " + codename;
                                    continue;
                                }
                                else
                                {
                                    string EndCode = Convert.ToString(ds.Tables[0].Rows[i][5]).Trim();
                                    if (EndCode.Length > 0 && !Regex.IsMatch(EndCode, @"^\d+$"))
                                    {
                                        Errors += "EndCode for " + codename + " must be Integer.";
                                        continue;
                                    }

                                    var DupData = db.tblholdcodes.Where(m => m.HoldCode == codename && m.IsDeleted == 0).FirstOrDefault();
                                    if (DupData == null)
                                    {
                                        MsqlConnection mc1 = new MsqlConnection();
                                        mc1.open();
                                        string dat = DateTime.Now.ToString();
                                        dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                        SqlCommand cmd2 = null;
                                        try
                                        {
                                            string HoldCodesLevel1ID = null;
                                            string GWCodesLevel2ID = null;
                                            string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                            if (Regex.IsMatch(EndCode, @"^\d+$"))
                                            {
                                                cmd2 = new SqlCommand("INSERT INTO tblholdcodes (`HoldCode`,`HoldCodeDesc`,`HoldCodesLevel`,`IsDeleted`,`CreatedOn`,`CreatedBy`,`EndCode`) VALUES( '" + codename + "' , '" + codedesc + "' , " + level + "  , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "', '" + EndCode + "')", mc1.msqlConnection);
                                            }
                                            else
                                            {
                                                cmd2 = new SqlCommand("INSERT INTO tblholdcodes (`HoldCode`,`HoldCodeDesc`,`HoldCodesLevel`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' ,  " + level + " , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
                                            }
                                            cmd2.ExecuteNonQuery();
                                        }
                                        catch (Exception e)
                                        {
                                            Errors += e.ToString();
                                        }
                                        mc1.close();
                                    }
                                    else //Update
                                    {
                                        MsqlConnection mc1 = new MsqlConnection();
                                        mc1.open();
                                        string dat = DateTime.Now.ToString();
                                        dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                        SqlCommand cmd2 = null;
                                        try
                                        {
                                            string HoldCodesLevel1ID = null;
                                            string GWCodesLevel2ID = null;
                                            string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                            if (Regex.IsMatch(EndCode, @"^\d+$"))
                                            {
                                                cmd2 = new SqlCommand("update tblholdcodes set HoldCodeDesc = '" + codedesc + "', ModifiedOn = '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', ModifiedBy = '" + Convert.ToInt32(Session["UserId"]) + "', EndCode = '" + EndCode + "' where HoldCodeID = '"+ DupData.HoldCodeID +"'", mc1.msqlConnection);
                                            }
                                            else
                                            {
                                                //cmd2 = new SqlCommand("INSERT INTO tblholdcodes (`HoldCode`,`HoldCodeDesc`,`HoldCodesLevel`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' ,  " + level + " , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
                                                cmd2 = new SqlCommand("update tblholdcodes set HoldCodeDesc = '" + codedesc + "', ModifiedOn = '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', ModifiedBy = '" + Convert.ToInt32(Session["UserId"]) + "' where HoldCodeID = '" + DupData.HoldCodeID + "'", mc1.msqlConnection);
                                            }
                                            cmd2.ExecuteNonQuery();
                                        }
                                        catch (Exception e)
                                        {
                                            Errors += e.ToString();
                                        }
                                        mc1.close();
                                    }
                                }
                            }
                            else
                            {
                                Errors += "Please Enter both Code Name and Description for" + codename + " " + codedesc;
                                continue;
                            }
                            #endregion
                        }
                        else if (level == 2)
                        {
                            //1st Check for Empty
                            //Check if it already exits , else insert.
                            #region
                            string codename = Convert.ToString(ds.Tables[0].Rows[i][1]);
                            string codedesc = Convert.ToString(ds.Tables[0].Rows[i][4]);

                            if (!string.IsNullOrEmpty(codename) && !string.IsNullOrEmpty(codedesc))
                            {
                                if (codename.Length > 40 || codedesc.Length > 60)
                                {
                                    Errors += "LossCodez (MaxLength:40) and LossDesc (MaxLength:60)  " + codename;
                                    continue;
                                }
                                else
                                {
                                    string Level1Code = Convert.ToString(ds.Tables[0].Rows[i][0]);

                                    //Is Level1 code Valid.
                                    if (!string.IsNullOrEmpty(Level1Code))
                                    {
                                        var Level1Data = db.tblholdcodes.Where(m => m.HoldCode == Level1Code && m.IsDeleted == 0).FirstOrDefault();
                                        if (Level1Data != null)
                                        {
                                            int HoldCodesLevel1ID = Level1Data.HoldCodeID;
                                            if (Level1Data.HoldCodesLevel != 1)
                                            {
                                                Errors += "InValid Level1 Code for " + codename + " ";
                                                continue;
                                            }
                                            else
                                            {
                                                string EndCode = Convert.ToString(ds.Tables[0].Rows[i][5]).Trim();
                                                if (EndCode.Length > 0 && !Regex.IsMatch(EndCode, @"^\d+$"))
                                                {
                                                    Errors += "EndCode for " + codename + " must be Integer.";
                                                    continue;
                                                }
                                                string dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                                MsqlConnection mc1 = new MsqlConnection();
                                                mc1.open();
                                                SqlCommand cmd2 = null;
                                                var DupData = db.tblholdcodes.Where(m => m.HoldCode == codename && m.IsDeleted == 0).FirstOrDefault();
                                                if (DupData == null)
                                                {
                                                    try
                                                    {
                                                        if (Regex.IsMatch(EndCode, @"^\d+$"))
                                                        {
                                                            cmd2 = new SqlCommand("INSERT INTO tblholdcodes (`HoldCode`,`HoldCodeDesc`,`HoldCodesLevel`,`HoldCodesLevel1ID`,`IsDeleted`,`CreatedOn`,`CreatedBy`,`EndCode`) VALUES( '" + codename + "' , '" + codedesc + "' , " + level + " ,'" + HoldCodesLevel1ID + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "', '" + EndCode + "')", mc1.msqlConnection);
                                                        }
                                                        else
                                                        {
                                                            cmd2 = new SqlCommand("INSERT INTO tblholdcodes (`HoldCode`,`HoldCodeDesc`,`HoldCodesLevel`,`HoldCodesLevel1ID`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , " + level + " ,'" + HoldCodesLevel1ID + "', 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
                                                        }

                                                        cmd2.ExecuteNonQuery();
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        Errors += e.ToString();
                                                    }
                                                }
                                                else //Update
                                                {
                                                    try
                                                    {
                                                        if (Regex.IsMatch(EndCode, @"^\d+$"))
                                                        {
                                                            cmd2 = new SqlCommand("update tblholdcodes set HoldCodeDesc = '" + codedesc + "', HoldCodesLevel1ID = '" + HoldCodesLevel1ID + "' , ModifiedOn = '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', ModifiedBy = '" + Convert.ToInt32(Session["UserId"]) + "', EndCode = '" + EndCode + "' where HoldCodeID = '" + DupData.HoldCodeID + "'", mc1.msqlConnection);
                                                        }
                                                        else
                                                        {
                                                            cmd2 = new SqlCommand("update tblholdcodes set HoldCodeDesc = '" + codedesc + "' , HoldCodesLevel1ID = '" + HoldCodesLevel1ID + "' , ModifiedOn = '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', ModifiedBy = '" + Convert.ToInt32(Session["UserId"]) + "' where HoldCodeID = '" + DupData.HoldCodeID + "'", mc1.msqlConnection);
                                                        }
                                                        cmd2.ExecuteNonQuery();
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        Errors += e.ToString();
                                                    }
                                                }
                                                mc1.close();
                                            }
                                        }
                                        else
                                        {
                                            Errors += "Level1 Code for " + codename + " doesn't exist.";
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        Errors += "Level1 Code for " + codename + "cannot be null";
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                Errors += "Please Enter both Code Name and Description ";
                                continue;
                            }
                            #endregion
                        }
                        else if (level == 3)
                        {
                            //1st Check for Empty
                            //Check if it already exits , else insert.
                            #region
                            string codename = Convert.ToString(ds.Tables[0].Rows[i][2]);
                            string codedesc = Convert.ToString(ds.Tables[0].Rows[i][4]);

                            if (!string.IsNullOrEmpty(codename) && !string.IsNullOrEmpty(codedesc))
                            {
                                if (codename.Length > 40 || codedesc.Length > 60)
                                {
                                    Errors += "HoldCodez (MaxLength:40) and LossDesc (MaxLength:60)  " + codename;
                                    continue;
                                }
                                else
                                {
                                    string Level1Code = Convert.ToString(ds.Tables[0].Rows[i][0]);
                                    string Level2Code = Convert.ToString(ds.Tables[0].Rows[i][1]);

                                    //Is Level1 code Valid.
                                    if (!string.IsNullOrEmpty(Level1Code) && !string.IsNullOrEmpty(Level2Code))
                                    {
                                        var Level1Data = db.tblholdcodes.Where(m => m.HoldCode == Level1Code && m.IsDeleted == 0).FirstOrDefault();
                                        var Level2Data = db.tblholdcodes.Where(m => m.HoldCode == Level2Code && m.IsDeleted == 0).FirstOrDefault();

                                        if (Level1Data != null && Level2Data != null)
                                        {
                                            if (Level1Data.HoldCodesLevel != 1)
                                            {
                                                Errors += "InValid Level1 Code for " + codename + " ";
                                                continue;
                                            }
                                            if (Level2Data.HoldCodesLevel != 2)
                                            {
                                                Errors += "InValid Level2 Code for " + codename + " ";
                                                continue;
                                            }

                                            //Level 1 && 2 are not relevant to each other.
                                            var Level2sLevel1 = Level2Data.HoldCodesLevel1ID;
                                            if (Level2sLevel1 == Level1Data.HoldCodeID)
                                            {
                                                int HoldCodesLevel1ID = Level1Data.HoldCodeID;
                                                int HoldCodesLevel2ID = Level2Data.HoldCodeID;

                                                string EndCode = Convert.ToString(ds.Tables[0].Rows[i][5]).Trim();
                                                    if (EndCode.Length > 0 && !Regex.IsMatch(EndCode, @"^\d+$"))
                                                    {
                                                        Errors += "EndCode for " + codename + " must be Integer.";
                                                        continue;
                                                    }

                                                MsqlConnection mc1 = new MsqlConnection();
                                                mc1.open();
                                                string dat = DateTime.Now.ToString();
                                                dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                SqlCommand cmd2 = null;
                                                string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                var DupData = db.tblholdcodes.Where(m => m.HoldCode == codename && m.IsDeleted == 0).FirstOrDefault();
                                                if (DupData == null)
                                                {
                                                    try
                                                    {
                                                        if (Regex.IsMatch(EndCode, @"^\d+$"))
                                                        {
                                                            cmd2 = new SqlCommand("INSERT INTO tblholdcodes (`HoldCode`,`HoldCodeDesc`,`HoldCodesLevel`,`HoldCodesLevel1ID`,`HoldCodesLevel2ID`,`IsDeleted`,`CreatedOn`,`CreatedBy`,`EndCode`) VALUES( '" + codename + "' , '" + codedesc + "'  , " + level + " ,'" + HoldCodesLevel1ID + "','" + HoldCodesLevel2ID + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "', '" + EndCode + "')", mc1.msqlConnection);
                                                        }
                                                        else
                                                        {
                                                            cmd2 = new SqlCommand("INSERT INTO tblholdcodes (`HoldCode`,`HoldCodeDesc`,`HoldCodesLevel`,`HoldCodesLevel1ID`,`HoldCodesLevel2ID`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , " + level + " ,'" + HoldCodesLevel1ID + "' ,'" + HoldCodesLevel2ID + "', 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
                                                        }
                                                        cmd2.ExecuteNonQuery();
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        Errors = e.ToString();
                                                    }
                                                }
                                                else
                                                {
                                                    try
                                                    {
                                                        if (Regex.IsMatch(EndCode, @"^\d+$"))
                                                        {
                                                            cmd2 = new SqlCommand("update tblholdcodes set HoldCodeDesc = '" + codedesc + "', HoldCodesLevel1ID = '" + HoldCodesLevel1ID + "' , HoldCodesLevel2ID = '" + HoldCodesLevel2ID + "' , ModifiedOn = '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', ModifiedBy = '" + Convert.ToInt32(Session["UserId"]) + "', EndCode = '" + EndCode + "' where HoldCodeID = '" + DupData.HoldCodeID + "'", mc1.msqlConnection);
                                                        }
                                                        else
                                                        {
                                                            cmd2 = new SqlCommand("update tblholdcodes set HoldCodeDesc = '" + codedesc + "' , HoldCodesLevel1ID = '" + HoldCodesLevel1ID + "' , HoldCodesLevel2ID = '" + HoldCodesLevel2ID + "' , ModifiedOn = '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', ModifiedBy = '" + Convert.ToInt32(Session["UserId"]) + "' where HoldCodeID = '" + DupData.HoldCodeID + "'", mc1.msqlConnection);
                                                        }
                                                        cmd2.ExecuteNonQuery();
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        Errors += e.ToString();
                                                    }
                                                }


                                                mc1.close();
                                            }
                                            else
                                            {
                                                Errors += "Hierarchy of LossCodes for " + codename + " doesn't exist.";
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            Errors += "Level1Code or Level2Code for " + codename + " doesn't exist.";
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        Errors += "Level1Code and Level2Code " + codename + "cannot be null";
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                Errors += "Please Enter both Code Name and Description ";
                                continue;
                            }
                            #endregion
                        }
                        else
                        {
                            Errors += "Please enter Level ";
                            continue;
                        }
                    }
                    #endregion
                }
                Session["Errors"] = Errors;
            }
            return View();
        }

        public ActionResult ExportHoldCodesDetails()
        {
            #region Excel and Stuff
            DateTime frda = DateTime.Now;
            String FileDir = @"C:\TataReport\ReportsList\" + System.DateTime.Now.ToString("yyyy-MM-dd");
            bool exists = System.IO.Directory.Exists(FileDir);
            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "HoldCodesList" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "HoldCodesList" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //" to " + toda.ToString("yyyy-MM-dd") + 
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
            worksheet.Cells["E" + 1].Value = "Code Desc";
            worksheet.Cells["F" + 1].Value = "EndCode";
            //worksheet.Cells["G" + 1].Value = "Loss Type";
            //worksheet.Cells["H" +1].Value = "MessageType";
            //worksheet.Cells["I" +1].Value = "Contributes To";
            worksheet.Cells["A1:I1"].Style.Font.Bold = true;

            #endregion

            var BreakDownLossesData = db.tblholdcodes.Where(m => m.IsDeleted == 0).OrderBy(m => m.HoldCodesLevel1ID).ThenBy(m => m.HoldCodesLevel2ID).ToList();
            int i = 2;
            foreach (var row in BreakDownLossesData)
            {
                int losscodelevel = row.HoldCodesLevel;
                if (losscodelevel == 1)
                {
                    worksheet.Cells["A" + i].Value = row.HoldCode;
                    worksheet.Cells["D" + i].Value = losscodelevel;
                    worksheet.Cells["E" + i].Value = row.HoldCodeDesc;
                    worksheet.Cells["F" + i].Value = row.EndCode;
                    //worksheet.Cells["G" + i].Value = row.MessageType;
                    //worksheet.Cells["H" + i].Value = row.MessageType;
                    //worksheet.Cells["I" + i].Value = row.ContributeTo;
                }
                if (losscodelevel == 2)
                {
                    int level1ID = Convert.ToInt32(row.HoldCodesLevel1ID);
                    string level1Code = Convert.ToString(db.tblholdcodes.Where(m => m.HoldCodeID == level1ID).Select(m => m.HoldCode).FirstOrDefault());

                    worksheet.Cells["A" + i].Value = level1Code;
                    worksheet.Cells["B" + i].Value = row.HoldCode;
                    //worksheet.Cells["C" + i].Value = row.LossCode;
                    worksheet.Cells["D" + i].Value = losscodelevel;
                    worksheet.Cells["E" + i].Value = row.HoldCodeDesc;
                    worksheet.Cells["F" + i].Value = row.EndCode;
                    // worksheet.Cells["G" + i].Value = row.MessageType;
                    //worksheet.Cells["H" + i].Value = row.MessageType;
                    //worksheet.Cells["I" + i].Value = row.ContributeTo;
                }
                if (losscodelevel == 3)
                {
                    int level1ID = Convert.ToInt32(row.HoldCodesLevel1ID);
                    string level1Code = Convert.ToString(db.tblholdcodes.Where(m => m.HoldCodeID == level1ID).Select(m => m.HoldCode).FirstOrDefault());
                    int level2ID = Convert.ToInt32(row.HoldCodesLevel2ID);
                    string level2Code = Convert.ToString(db.tblholdcodes.Where(m => m.HoldCodeID == level2ID).Select(m => m.HoldCode).FirstOrDefault());

                    worksheet.Cells["A" + i].Value = level1Code;
                    worksheet.Cells["B" + i].Value = level2Code;
                    worksheet.Cells["C" + i].Value = row.HoldCode;
                    worksheet.Cells["D" + i].Value = losscodelevel;
                    worksheet.Cells["E" + i].Value = row.HoldCodeDesc;
                    worksheet.Cells["F" + i].Value = row.EndCode;
                    //worksheet.Cells["G" + i].Value = row.MessageType;
                    //worksheet.Cells["H" + i].Value = row.MessageType;
                    //worksheet.Cells["I" + i].Value = row.ContributeTo;
                }
                i++;
            }
            #region Save and Download

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            p.Save();

            //Downloding Excel
            string path1 = System.IO.Path.Combine(FileDir, "HoldCodesList" + frda.ToString("yyyy-MM-dd") + ".xlsx");
            System.IO.FileInfo file1 = new System.IO.FileInfo(path1);
            string Outgoingfile = "HoldCodesList" + frda.ToString("yyyy-MM-dd") + ".xlsx";
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
