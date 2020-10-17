
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
    public class IdleCodesController : Controller
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
            var query = from c in db.tbllossescodes
                        where (!(from o in db.tbllossescodes
                                where o.IsDeleted == 0
                                select o.LossCodesLevel1ID)
                                .Contains(c.LossCodeID))
                        &&
                                (!(from p in db.tbllossescodes
                                where p.IsDeleted == 0
                                select p.LossCodesLevel2ID)
                                .Contains(c.LossCodeID))
                        &&
                        c.IsDeleted == 0
                        && c.LossCode != "999"
                        && c.MessageType != "BREAKDOWN"
                        && c.MessageType != "PM"
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
            ViewData["Level1"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.LossCode != "999" && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "LossCodeID", "LossCodeDesc");
            //keep level2 codes empty, based on level1 populate level2
            ViewData["Level2"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 4 && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "LossCodeID", "LossCodeDesc");
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

            ViewData["Level1"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.LossCode != "999" && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "LossCodeID", "LossCodeDesc");
            ViewData["Level2"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 2 && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "LossCodeID", "LossCodeDesc");

            if (Convert.ToInt16(tlc.LossCodesLevel) == 1)
            {
                var duplosscode = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCode == tlc.LossCode).ToList();
                if (duplosscode.Count == 0)
                {
                    db.tbllossescodes.Add(tlc);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "Loss code already Exist.";
                    return View(tlc);
                }
            }
            if (Convert.ToInt16(tlc.LossCodesLevel) == 2)
            {
                tlc.LossCodesLevel1ID = Level1;

                var duplosscode = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCode == tlc.LossCode && m.LossCodesLevel1ID == tlc.LossCodesLevel1ID).ToList();
                if (duplosscode.Count == 0)
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
                    Session["Error"] = "Loss code already Exist.";
                    return View(tlc);
                }
            }
            if (Convert.ToInt16(tlc.LossCodesLevel) == 3)
            {
                tlc.LossCodesLevel1ID = Level1;
                tlc.LossCodesLevel2ID = Level2;
                var duplosscode = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCode == tlc.LossCode && m.LossCodesLevel1ID == tlc.LossCodesLevel1ID && m.LossCodesLevel2ID == tlc.LossCodesLevel2ID).ToList();
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
                    Session["Error"] = "Loss code already Exist.";
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

            ViewData["Level1"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.LossCode != "999" && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "LossCodeID", "LossCodeDesc", lossdata.LossCodesLevel1ID);
            ViewData["Level2"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 2 && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "LossCodeID", "LossCodeDesc", lossdata.LossCodesLevel2ID);
            ViewBag.radioselected = lossdata.LossCodesLevel;
            ViewData["MessageType"] = lossdata.MessageType;
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

            if (Convert.ToInt16(tlc.LossCodesLevel) == 1)
            {
                var duplosscode = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCode == tlc.LossCode && m.LossCodeID != tlc.LossCodeID).ToList();
                if (duplosscode.Count == 0)
                {
                    db.Entry(tlc).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "Loss code already Exist.";
                    ViewData["Level1"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.LossCode != "999" && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "LossCodeID", "LossCodeDesc", tlc.LossCodesLevel1ID);
                    ViewData["Level2"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 2 && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "LossCodeID", "LossCodeDesc", tlc.LossCodesLevel2ID);
                    ViewBag.radioselected = tlc.LossCodesLevel;

                    return View(tlc);
                }
            }

            if (Convert.ToInt16(tlc.LossCodesLevel) == 2)
            {
                tlc.LossCodesLevel1ID = Level1;
                var duplosscode = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCode == tlc.LossCode && m.LossCodeID != tlc.LossCodeID && m.LossCodesLevel1ID == tlc.LossCodesLevel1ID).ToList();
                if (duplosscode.Count == 0)
                {
                    db.Entry(tlc).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "Loss code already Exist.";
                    ViewData["Level1"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.LossCode != "999" && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "LossCodeID", "LossCodeDesc", tlc.LossCodesLevel1ID);
                    ViewData["Level2"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 2 && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "LossCodeID", "LossCodeDesc", tlc.LossCodesLevel2ID);
                    ViewBag.radioselected = tlc.LossCodesLevel;

                    return View(tlc);
                }
            }
            if (Convert.ToInt16(tlc.LossCodesLevel) == 3)
            {
                tlc.LossCodesLevel1ID = Level1;
                tlc.LossCodesLevel2ID = Level2;
                var duplosscode = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCode == tlc.LossCode && m.LossCodeID != tlc.LossCodeID && m.LossCodesLevel1ID == tlc.LossCodesLevel1ID && m.LossCodesLevel2ID == tlc.LossCodesLevel2ID).ToList();
                if (duplosscode.Count == 0)
                {
                    db.Entry(tlc).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "Loss code already Exist.";
                    ViewData["Level1"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1 && m.LossCode != "999" && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "LossCodeID", "LossCodeDesc", tlc.LossCodesLevel1ID);
                    ViewData["Level2"] = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 2 && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "LossCodeID", "LossCodeDesc", tlc.LossCodesLevel2ID);
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
            int UserID1 = id;
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
        //public JsonResult GetLevel1(int Id)
        //{
        //    var selectedRow = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 1), "LossCodeID", "LossCodeDesc");
        //    return Json(selectedRow, JsonRequestBehavior.AllowGet);
        //}

        public JsonResult GetLevel2(int Id)
        {
            var selectedRow = new SelectList(db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodesLevel == 2 && m.LossCodesLevel1ID == Id), "LossCodeID", "LossCodeDesc");
            return Json(selectedRow, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ImportLossCodesDetails()
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
        public ActionResult ImportLossCodesDetails(HttpPostedFileBase file, string UploadType)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

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

                List<string> MessageType = new List<string>();
                MessageType.Add("Setup");
                MessageType.Add("IDLE");
                MessageType.Add("BREAKDOWN");

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
                            string codedesc = Convert.ToString(ds.Tables[0].Rows[i][5]);
                            if (!string.IsNullOrEmpty(codename) && !string.IsNullOrEmpty(codedesc))
                            {
                                //Validate MessageType
                                string messageType = Convert.ToString(ds.Tables[0].Rows[i][6]);
                                if (string.IsNullOrEmpty(messageType) || messageType.Length > 30)
                                {
                                    Errors += "MessageType for " + codename + " Cannot be Null & MaxLength less than 30 ";
                                    continue;
                                }
                                //else if ((String.Compare(messageType, "Setup", StringComparison.OrdinalIgnoreCase) != 0)
                                //           || (String.Compare(messageType, "IDLE", StringComparison.OrdinalIgnoreCase) != 0)
                                //    || (String.Compare(messageType, "BREAKDOWN", StringComparison.OrdinalIgnoreCase) == 0)
                                //          )

                                else if (!MessageType.Any(s => s.Equals(messageType, StringComparison.OrdinalIgnoreCase)))
                                {
                                    Errors += "Invalid MessageType for " + codename + "It has to be('Setup' or 'IDLE' or 'BREAKDOWN')";
                                    continue;
                                }

                                var DupData = db.tbllossescodes.Where(m => m.LossCode == codename && m.MessageType == messageType).Where(m => m.IsDeleted == 0).FirstOrDefault();
                                if (DupData != null)
                                {
                                    Errors += "Duplicate Code : " + DupData.LossCode;
                                    continue;
                                }
                                else if (codename.Length > 40 || codedesc.Length > 60)
                                {
                                    Errors += "LossCodez (MaxLength:40) and LossDesc (MaxLength:60)  " + DupData.LossCode;
                                    continue;
                                }
                                else
                                {
                                    //Just to make sure the case is similar for all losses.
                                    if (String.Compare(messageType, "Setup", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        messageType = "Setup";
                                    }
                                    else if (String.Compare(messageType, "IDLE", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        messageType = "IDLE";
                                    }
                                    if (String.Compare(messageType, "BREAKDOWN", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        messageType = "BREAKDOWN";
                                    }

                                    string contributesTo = Convert.ToString(ds.Tables[0].Rows[i][7]).ToUpper();
                                    //if (string.IsNullOrEmpty(contributesTo) && messageType != "BREAKDOWN")
                                    if (string.IsNullOrEmpty(contributesTo))
                                    {
                                        contributesTo = "ROA";
                                    }

                                    string EndCode = Convert.ToString(ds.Tables[0].Rows[i][4]).Trim();

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
                                        string LossCodesLevel1ID = null;
                                        string LossCodesLevel2ID = null;
                                        string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                        if (Regex.IsMatch(EndCode, @"^\d+$"))
                                        {
                                            cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`,`EndCode`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType + "' , " + level + " , '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "', '" + EndCode + "')", mc1.msqlConnection);
                                        }
                                        else
                                        {
                                            cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType + "' , " + level + " , '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
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
                                Errors += "Please Enter both Code Name and Description ";
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
                            string codedesc = Convert.ToString(ds.Tables[0].Rows[i][5]);

                            if (!string.IsNullOrEmpty(codename) && !string.IsNullOrEmpty(codedesc))
                            {
                                var DupData = db.tbllossescodes.Where(m => m.LossCode == codename).Where(m => m.IsDeleted == 0).FirstOrDefault();
                                if (DupData != null)
                                {
                                    Errors += "Duplicate Code : " + DupData.LossCode;
                                    continue;
                                }
                                else if (codename.Length > 40 || codedesc.Length > 60)
                                {
                                    Errors += "LossCodez (MaxLength:40) and LossDesc (MaxLength:60)  " + DupData.LossCode;
                                    continue;
                                }
                                else
                                {
                                    string Level1Code = Convert.ToString(ds.Tables[0].Rows[i][0]);
                                    //Its Level1 code Valid.
                                    if (!string.IsNullOrEmpty(Level1Code))
                                    {

                                        //Validate MessageType
                                        string messageType = Convert.ToString(ds.Tables[0].Rows[i][6]);
                                        if (string.IsNullOrEmpty(messageType) || messageType.Length > 30)
                                        {
                                            Errors += "MessageType for " + codename + " Cannot be Null or MaxLength(30)";
                                            continue;
                                        }
                                        //else if ((String.Compare(messageType, "Setup", StringComparison.OrdinalIgnoreCase) != 0)
                                        //   || (String.Compare(messageType, "IDLE", StringComparison.OrdinalIgnoreCase) != 0)
                                        //    || (String.Compare(messageType, "BREAKDOWN", StringComparison.OrdinalIgnoreCase) == 0)
                                        //  )
                                        else if (!MessageType.Any(s => s.Equals(messageType, StringComparison.OrdinalIgnoreCase)))
                                        {
                                            Errors += "Invalid MessageType for " + codename + "It has to be('Setup' or 'IDLE' or 'BREAKDOWN')";
                                            continue;
                                        }

                                        //Just to make sure the case is similar for all losses.
                                        if (String.Compare(messageType, "Setup", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            messageType = "Setup";
                                        }
                                        else if (String.Compare(messageType, "IDLE", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            messageType = "IDLE";
                                        }
                                        if (String.Compare(messageType, "BREAKDOWN", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            messageType = "BREAKDOWN";
                                        }

                                        var Level1Data = db.tbllossescodes.Where(m => m.LossCode == Level1Code && m.MessageType == messageType && m.IsDeleted == 0).FirstOrDefault();
                                        if (Level1Data != null)
                                        {
                                            int LossCodesLevel1ID = Level1Data.LossCodeID;
                                            if (Level1Data.LossCodesLevel != 1)
                                            {
                                                Errors += "InValid Level1 Code for " + codename + " ";
                                                continue;
                                            }
                                            else
                                            {
                                                string contributesTo = Convert.ToString(ds.Tables[0].Rows[i][7]).ToUpper();
                                                if (string.IsNullOrEmpty(contributesTo))
                                                {
                                                    contributesTo = "ROA";
                                                }
                                                string EndCode = Convert.ToString(ds.Tables[0].Rows[i][4]).Trim();
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
                                                    string LossCodesLevel2ID = null;
                                                    string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                    if (Regex.IsMatch(EndCode, @"^\d+$"))
                                                    {
                                                        cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`LossCodesLevel1ID`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`,`EndCode`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType + "' , " + level + " ,'" + LossCodesLevel1ID + "', '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "', '" + EndCode + "')", mc1.msqlConnection);
                                                    }
                                                    else
                                                    {
                                                        cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`LossCodesLevel1ID`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType + "' , " + level + " ,'" + LossCodesLevel1ID + "', '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
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
                            string codedesc = Convert.ToString(ds.Tables[0].Rows[i][5]);

                            if (!string.IsNullOrEmpty(codename) && !string.IsNullOrEmpty(codedesc))
                            {
                                var DupData = db.tbllossescodes.Where(m => m.LossCode == codename).Where(m => m.IsDeleted == 0).FirstOrDefault();
                                if (DupData != null)
                                {
                                    Errors += "Duplicate Code : " + DupData.LossCode;
                                    continue;
                                }
                                else if (codename.Length > 40 || codedesc.Length > 60)
                                {
                                    Errors += "LossCodez (MaxLength:40) and LossDesc (MaxLength:60)  " + DupData.LossCode;
                                    continue;
                                }
                                else
                                {
                                    string Level1Code = Convert.ToString(ds.Tables[0].Rows[i][0]);
                                    string Level2Code = Convert.ToString(ds.Tables[0].Rows[i][1]);

                                    //Is Level1 code Valid.
                                    if (!string.IsNullOrEmpty(Level1Code) && !string.IsNullOrEmpty(Level2Code))
                                    {

                                        //Validate MessageType
                                        string messageType = Convert.ToString(ds.Tables[0].Rows[i][6]);
                                        if (string.IsNullOrEmpty(messageType) || messageType.Length > 30)
                                        {
                                            Errors += "MessageType for " + codename + " Cannot be Null or MaxLength(30)";
                                            continue;
                                        }
                                        //else if ((String.Compare(messageType, "Setup", StringComparison.OrdinalIgnoreCase) != 0)
                                        //    || (String.Compare(messageType, "IDLE", StringComparison.OrdinalIgnoreCase) != 0)
                                        //    || (String.Compare(messageType, "BREAKDOWN", StringComparison.OrdinalIgnoreCase) == 0)
                                        //   )
                                        // || (String.Compare(messageType, "BREAKDOWN", StringComparison.OrdinalIgnoreCase) == 0)
                                        // messageType != "Setup" || messageType != "IDLE" || messageType != "BREAKDOWN")

                                        else if (!MessageType.Any(s => s.Equals(messageType, StringComparison.OrdinalIgnoreCase)))
                                        {
                                            Errors += "Invalid MessageType for " + codename + "It has to be('Setup' or 'IDLE' or 'BREAKDOWN')";
                                            continue;
                                        }

                                        //Just to make sure the case is similar for all losses.
                                        if (String.Compare(messageType, "Setup", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            messageType = "Setup";
                                        }
                                        else if (String.Compare(messageType, "IDLE", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            messageType = "IDLE";
                                        }
                                        if (String.Compare(messageType, "BREAKDOWN", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            messageType = "BREAKDOWN";
                                        }

                                        var Level1Data = db.tbllossescodes.Where(m => m.LossCode == Level1Code && m.MessageType == messageType && m.IsDeleted == 0).FirstOrDefault();
                                        var Level2Data = db.tbllossescodes.Where(m => m.LossCode == Level2Code && m.MessageType == messageType && m.IsDeleted == 0).FirstOrDefault();

                                        if (Level1Data != null && Level2Data != null)
                                        {
                                            if (Level1Data.LossCodesLevel != 1)
                                            {
                                                Errors += "InValid Level1 Code for " + codename + " ";
                                                continue;
                                            }
                                            if (Level2Data.LossCodesLevel != 2)
                                            {
                                                Errors += "InValid Level2 Code for " + codename + " ";
                                                continue;
                                            }

                                            //Level 1 && 2 are not relevant to each other.
                                            var Level2sLevel1 = Level2Data.LossCodesLevel1ID;
                                            if (Level2sLevel1 == Level1Data.LossCodeID)
                                            {
                                                int LossCodesLevel1ID = Level1Data.LossCodeID;
                                                int LossCodesLevel2ID = Level2Data.LossCodeID;

                                                string contributesTo = Convert.ToString(ds.Tables[0].Rows[i][7]).ToUpper();
                                                if (string.IsNullOrEmpty(contributesTo))
                                                {
                                                    contributesTo = "ROA";
                                                }
                                                string EndCode = Convert.ToString(ds.Tables[0].Rows[i][4]).Trim();

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
                                                    string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                                    if (Regex.IsMatch(EndCode, @"^\d+$"))
                                                    {
                                                        cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`LossCodesLevel1ID`,`LossCodesLevel2ID`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`,`EndCode`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType.ToUpper() + "' , " + level + " ,'" + LossCodesLevel1ID + "' ,'" + LossCodesLevel2ID + "', '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "', '" + EndCode + "')", mc1.msqlConnection);
                                                    }
                                                    else
                                                    {
                                                        cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`LossCodesLevel1ID`,`LossCodesLevel2ID`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType + "' , " + level + " ,'" + LossCodesLevel1ID + "' ,'" + LossCodesLevel2ID + "', '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
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

                        bool IsValid = false;
                        //Validations
                        int level = 0;
                        int.TryParse(ds.Tables[0].Rows[i][3].ToString(), out level);
                        if (level == 1)
                        {
                            //1st Check for Empty
                            //Check if it already exits if so delete that and insert new , else insert.
                            #region
                            string codename = Convert.ToString(ds.Tables[0].Rows[i][0]);
                            string codedesc = Convert.ToString(ds.Tables[0].Rows[i][5]);
                            if (!string.IsNullOrEmpty(codename) && !string.IsNullOrEmpty(codedesc))
                            {
                                //Validate MessageType
                                string messageType = Convert.ToString(ds.Tables[0].Rows[i][6]);
                                if (string.IsNullOrEmpty(messageType) || messageType.Length > 30)
                                {
                                    Errors += "MessageType for " + codename + " Cannot be Null & MaxLength less than 30 ";
                                    continue;
                                }
                                //else if ((String.Compare(messageType, "Setup", StringComparison.OrdinalIgnoreCase) != 0)
                                //           || (String.Compare(messageType, "IDLE", StringComparison.OrdinalIgnoreCase) != 0)
                                //    || (String.Compare(messageType, "BREAKDOWN", StringComparison.OrdinalIgnoreCase) == 0)
                                //          )

                                else if (!MessageType.Any(s => s.Equals(messageType, StringComparison.OrdinalIgnoreCase)))
                                {
                                    Errors += "Invalid MessageType for " + codename + "It has to be('Setup' or 'IDLE' or 'BREAKDOWN')";
                                    continue;
                                }

                                //var DupData = db.tbllossescodes.Where(m => m.LossCode == codename && m.MessageType == messageType).Where(m => m.IsDeleted == 0).FirstOrDefault();
                                //if (DupData != null)
                                //{
                                //    Errors += "Duplicate Code : " + DupData.LossCode;
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
                                    //Just to make sure the case is similar for all losses.
                                    if (String.Compare(messageType, "Setup", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        messageType = "Setup";
                                    }
                                    else if (String.Compare(messageType, "IDLE", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        messageType = "IDLE";
                                    }
                                    if (String.Compare(messageType, "BREAKDOWN", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        messageType = "BREAKDOWN";
                                    }

                                    string contributesTo = Convert.ToString(ds.Tables[0].Rows[i][7]).ToUpper();
                                    //if (string.IsNullOrEmpty(contributesTo) && messageType != "BREAKDOWN")
                                    if (string.IsNullOrEmpty(contributesTo))
                                    {
                                        contributesTo = "ROA";
                                    }

                                    string EndCode = Convert.ToString(ds.Tables[0].Rows[i][4]).Trim();

                                    if (EndCode.Length > 0 && !Regex.IsMatch(EndCode, @"^\d+$"))
                                    {
                                        Errors += "EndCode for " + codename + " must be Integer.";
                                        continue;
                                    }

                                    //Delete if loss exists with same codename and Details
                                    var LossDup = db.tbllossescodes.Where(m => m.LossCode == codename && m.LossCodesLevel == 1 && m.MessageType == messageType && m.IsDeleted == 0).FirstOrDefault();
                                    if (LossDup != null)
                                    {
                                        try
                                        {
                                            // if any loss is running then you are screwed.
                                            // well u can update its id or throw error about this update

                                            //insert new row only then u will get the Latest id.
                                            MsqlConnection mc1 = new MsqlConnection();
                                            mc1.open();
                                            string dat = DateTime.Now.ToString();
                                            dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                            SqlCommand cmd2 = null;
                                            try
                                            {
                                                string LossCodesLevel1ID = null;
                                                string LossCodesLevel2ID = null;
                                                string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                                if (Regex.IsMatch(EndCode, @"^\d+$"))
                                                {
                                                    cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`,`EndCode`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType + "' , " + level + " , '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "', '" + EndCode + "')", mc1.msqlConnection);
                                                }
                                                else
                                                {
                                                    cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType + "' , " + level + " , '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
                                                }

                                                cmd2.ExecuteNonQuery();
                                            }
                                            catch (Exception e)
                                            {
                                                Errors += e.ToString();
                                            }
                                            mc1.close();

                                            LossDup.IsDeleted = 1;
                                            LossDup.DeletedDate = DateTime.Now;
                                            db.Entry(LossDup).State = EntityState.Modified;
                                            db.SaveChanges();

                                            using (i_facility_shaktiEntities1 dbloss = new i_facility_shaktiEntities1())
                                            {
                                                // Change all sublevels level1 ID to the new one
                                                //now query for new row.

                                                var LossNew = dbloss.tbllossescodes.Where(m => m.LossCode == codename && m.LossCodesLevel == 1 && m.MessageType == messageType && m.IsDeleted == 0).FirstOrDefault();
                                                if (LossNew != null)
                                                {
                                                    int NewLossID = LossNew.LossCodeID;

                                                    var AllSubLevelLosses = dbloss.tbllossescodes.Where(m => m.LossCodesLevel1ID == NewLossID && m.IsDeleted == 0).ToList();
                                                    foreach (var sublosses in AllSubLevelLosses)
                                                    {
                                                        sublosses.LossCodesLevel1ID = NewLossID;
                                                        dbloss.Entry(sublosses).State = EntityState.Modified;
                                                        dbloss.SaveChanges();
                                                    }
                                                }
                                            }

                                        }
                                        catch (Exception e)
                                        {
                                        }
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
                                            string LossCodesLevel1ID = null;
                                            string LossCodesLevel2ID = null;
                                            string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                            if (Regex.IsMatch(EndCode, @"^\d+$"))
                                            {
                                                cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`,`EndCode`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType + "' , " + level + " , '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "', '" + EndCode + "')", mc1.msqlConnection);
                                            }
                                            else
                                            {
                                                cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType + "' , " + level + " , '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
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
                                Errors += "Please Enter both Code Name and Description ";
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
                            string codedesc = Convert.ToString(ds.Tables[0].Rows[i][5]);

                            if (!string.IsNullOrEmpty(codename) && !string.IsNullOrEmpty(codedesc))
                            {
                                //var DupData = db.tbllossescodes.Where(m => m.LossCode == codename).Where(m => m.IsDeleted == 0).FirstOrDefault();
                                //if (DupData != null)
                                //{
                                //    Errors += "Duplicate Code : " + DupData.LossCode;
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
                                    //Its Level1 code Valid.
                                    if (!string.IsNullOrEmpty(Level1Code))
                                    {

                                        //Validate MessageType
                                        string messageType = Convert.ToString(ds.Tables[0].Rows[i][6]);
                                        if (string.IsNullOrEmpty(messageType) || messageType.Length > 30)
                                        {
                                            Errors += "MessageType for " + codename + " Cannot be Null or MaxLength(30)";
                                            continue;
                                        }
                                        //else if ((String.Compare(messageType, "Setup", StringComparison.OrdinalIgnoreCase) != 0)
                                        //   || (String.Compare(messageType, "IDLE", StringComparison.OrdinalIgnoreCase) != 0)
                                        //    || (String.Compare(messageType, "BREAKDOWN", StringComparison.OrdinalIgnoreCase) == 0)
                                        //  )
                                        else if (!MessageType.Any(s => s.Equals(messageType, StringComparison.OrdinalIgnoreCase)))
                                        {
                                            Errors += "Invalid MessageType for " + codename + "It has to be('Setup' or 'IDLE' or 'BREAKDOWN')";
                                            continue;
                                        }

                                        //Just to make sure the case is similar for all losses.
                                        if (String.Compare(messageType, "Setup", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            messageType = "Setup";
                                        }
                                        else if (String.Compare(messageType, "IDLE", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            messageType = "IDLE";
                                        }
                                        if (String.Compare(messageType, "BREAKDOWN", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            messageType = "BREAKDOWN";
                                        }

                                        var Level1Data = db.tbllossescodes.Where(m => m.LossCode == Level1Code && m.MessageType == messageType && m.IsDeleted == 0).FirstOrDefault();
                                        if (Level1Data != null)
                                        {
                                            int LossCodesLevel1ID = Level1Data.LossCodeID;
                                            if (Level1Data.LossCodesLevel != 1)
                                            {
                                                Errors += "InValid Level1 Code for " + codename + " ";
                                                continue;
                                            }
                                            else
                                            {
                                                string contributesTo = Convert.ToString(ds.Tables[0].Rows[i][7]).ToUpper();
                                                if (string.IsNullOrEmpty(contributesTo))
                                                {
                                                    contributesTo = "ROA";
                                                }
                                                string EndCode = Convert.ToString(ds.Tables[0].Rows[i][4]).Trim();
                                                if (EndCode.Length > 0 && !Regex.IsMatch(EndCode, @"^\d+$"))
                                                {
                                                    Errors += "EndCode for " + codename + " must be Integer.";
                                                    continue;
                                                }

                                                using (i_facility_shaktiEntities1 dbloss = new i_facility_shaktiEntities1())
                                                {
                                                    //Delete if loss exists with same codename and Details
                                                    var LossDup = dbloss.tbllossescodes.Where(m => m.LossCode == codename && m.LossCodesLevel == 2 && m.MessageType == messageType && m.LossCodesLevel1ID == LossCodesLevel1ID && m.IsDeleted == 0).FirstOrDefault();
                                                    if (LossDup != null)
                                                    {
                                                        MsqlConnection mc1 = new MsqlConnection();
                                                        mc1.open();
                                                        string dat = DateTime.Now.ToString();
                                                        dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                        SqlCommand cmd2 = null;
                                                        try
                                                        {
                                                            string LossCodesLevel2ID = null;
                                                            string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                            if (Regex.IsMatch(EndCode, @"^\d+$"))
                                                            {
                                                                cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`LossCodesLevel1ID`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`,`EndCode`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType + "' , " + level + " ,'" + LossCodesLevel1ID + "', '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "', '" + EndCode + "')", mc1.msqlConnection);
                                                            }
                                                            else
                                                            {
                                                                cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`LossCodesLevel1ID`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType + "' , " + level + " ,'" + LossCodesLevel1ID + "', '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
                                                            }
                                                            cmd2.ExecuteNonQuery();
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            Errors = e.ToString();
                                                        }
                                                        mc1.close();

                                                        LossDup.IsDeleted = 1;
                                                        LossDup.DeletedDate = DateTime.Now;

                                                        dbloss.Entry(LossDup).State = EntityState.Modified;
                                                        dbloss.SaveChanges();

                                                        // Change all sublevels level1 ID to the new one
                                                        //now query for new row.

                                                        var LossNew = dbloss.tbllossescodes.Where(m => m.LossCode == codename && m.LossCodesLevel == 2 && m.MessageType == messageType && m.LossCodesLevel1ID == LossCodesLevel1ID && m.IsDeleted == 0).FirstOrDefault();
                                                        if (LossNew != null)
                                                        {
                                                            int NewLossID = LossNew.LossCodeID;

                                                            var AllSubLevelLosses = dbloss.tbllossescodes.Where(m => m.LossCodesLevel2ID == NewLossID && m.IsDeleted == 0).ToList();
                                                            foreach (var sublosses in AllSubLevelLosses)
                                                            {
                                                                sublosses.LossCodesLevel1ID = NewLossID;
                                                                dbloss.Entry(sublosses).State = EntityState.Modified;
                                                                dbloss.SaveChanges();
                                                            }
                                                        }


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
                                                            string LossCodesLevel2ID = null;
                                                            string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                            if (Regex.IsMatch(EndCode, @"^\d+$"))
                                                            {
                                                                cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`LossCodesLevel1ID`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`,`EndCode`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType + "' , " + level + " ,'" + LossCodesLevel1ID + "', '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "', '" + EndCode + "')", mc1.msqlConnection);
                                                            }
                                                            else
                                                            {
                                                                cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`LossCodesLevel1ID`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType + "' , " + level + " ,'" + LossCodesLevel1ID + "', '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
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
                            string codedesc = Convert.ToString(ds.Tables[0].Rows[i][5]);

                            if (!string.IsNullOrEmpty(codename) && !string.IsNullOrEmpty(codedesc))
                            {
                                //var DupData = db.tbllossescodes.Where(m => m.LossCode == codename).Where(m => m.IsDeleted == 0).FirstOrDefault();
                                //if (DupData != null)
                                //{
                                //    Errors += "Duplicate Code : " + DupData.LossCode;
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
                                    string Level2Code = Convert.ToString(ds.Tables[0].Rows[i][1]);

                                    //Is Level1 code Valid.
                                    if (!string.IsNullOrEmpty(Level1Code) && !string.IsNullOrEmpty(Level2Code))
                                    {

                                        //Validate MessageType
                                        string messageType = Convert.ToString(ds.Tables[0].Rows[i][6]);
                                        if (string.IsNullOrEmpty(messageType) || messageType.Length > 30)
                                        {
                                            Errors += "MessageType for " + codename + " Cannot be Null or MaxLength(30)";
                                            continue;
                                        }
                                        //else if ((String.Compare(messageType, "Setup", StringComparison.OrdinalIgnoreCase) != 0)
                                        //    || (String.Compare(messageType, "IDLE", StringComparison.OrdinalIgnoreCase) != 0)
                                        //    || (String.Compare(messageType, "BREAKDOWN", StringComparison.OrdinalIgnoreCase) == 0)
                                        //   )
                                        // || (String.Compare(messageType, "BREAKDOWN", StringComparison.OrdinalIgnoreCase) == 0)
                                        // messageType != "Setup" || messageType != "IDLE" || messageType != "BREAKDOWN")

                                        else if (!MessageType.Any(s => s.Equals(messageType, StringComparison.OrdinalIgnoreCase)))
                                        {
                                            Errors += "Invalid MessageType for " + codename + "It has to be('Setup' or 'IDLE' or 'BREAKDOWN')";
                                            continue;
                                        }

                                        //Just to make sure the case is similar for all losses.
                                        if (String.Compare(messageType, "Setup", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            messageType = "Setup";
                                        }
                                        else if (String.Compare(messageType, "IDLE", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            messageType = "IDLE";
                                        }
                                        if (String.Compare(messageType, "BREAKDOWN", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            messageType = "BREAKDOWN";
                                        }

                                        var Level1Data = db.tbllossescodes.Where(m => m.LossCode == Level1Code && m.MessageType == messageType && m.IsDeleted == 0).FirstOrDefault();
                                        var Level2Data = db.tbllossescodes.Where(m => m.LossCode == Level2Code && m.MessageType == messageType && m.IsDeleted == 0).FirstOrDefault();

                                        if (Level1Data != null && Level2Data != null)
                                        {
                                            if (Level1Data.LossCodesLevel != 1)
                                            {
                                                Errors += "InValid Level1 Code for " + codename + " ";
                                                continue;
                                            }
                                            if (Level2Data.LossCodesLevel != 2)
                                            {
                                                Errors += "InValid Level2 Code for " + codename + " ";
                                                continue;
                                            }

                                            //Level 1 && 2 are not relevant to each other.
                                            var Level2sLevel1 = Level2Data.LossCodesLevel1ID;
                                            if (Level2sLevel1 == Level1Data.LossCodeID)
                                            {
                                                int LossCodesLevel1ID = Level1Data.LossCodeID;
                                                int LossCodesLevel2ID = Level2Data.LossCodeID;

                                                string contributesTo = Convert.ToString(ds.Tables[0].Rows[i][7]).ToUpper();
                                                if (string.IsNullOrEmpty(contributesTo))
                                                {
                                                    contributesTo = "ROA";
                                                }
                                                string EndCode = Convert.ToString(ds.Tables[0].Rows[i][4]).Trim();

                                                if (EndCode.Length > 0 && !Regex.IsMatch(EndCode, @"^\d+$"))
                                                {
                                                    Errors += "EndCode for " + codename + " must be Integer.";
                                                    continue;
                                                }

                                                //Delete if loss exists with same codename and Details
                                                var LossDup = db.tbllossescodes.Where(m => m.LossCode == codename && m.LossCodesLevel == 3 && m.MessageType == messageType && m.LossCodesLevel1ID == LossCodesLevel1ID && m.LossCodesLevel2ID == LossCodesLevel2ID && m.IsDeleted == 0).FirstOrDefault();
                                                if (LossDup != null)
                                                {

                                                    MsqlConnection mc1 = new MsqlConnection();
                                                    mc1.open();
                                                    string dat = DateTime.Now.ToString();
                                                    dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                    SqlCommand cmd2 = null;
                                                    try
                                                    {
                                                        string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                                        if (Regex.IsMatch(EndCode, @"^\d+$"))
                                                        {
                                                            cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`LossCodesLevel1ID`,`LossCodesLevel2ID`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`,`EndCode`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType.ToUpper() + "' , " + level + " ,'" + LossCodesLevel1ID + "' ,'" + LossCodesLevel2ID + "', '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "', '" + EndCode + "')", mc1.msqlConnection);
                                                        }
                                                        else
                                                        {
                                                            cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`LossCodesLevel1ID`,`LossCodesLevel2ID`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType + "' , " + level + " ,'" + LossCodesLevel1ID + "' ,'" + LossCodesLevel2ID + "', '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
                                                        }

                                                        cmd2.ExecuteNonQuery();
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        Errors = e.ToString();
                                                    }
                                                    mc1.close();

                                                    LossDup.IsDeleted = 1;
                                                    LossDup.DeletedDate = DateTime.Now;
                                                   
                                                        db.Entry(LossDup).State = EntityState.Modified;
                                                        db.SaveChanges();

                                                        using (i_facility_shaktiEntities1 dbloss = new i_facility_shaktiEntities1())
                                                        {
                                                        // Change all sublevels level1 ID to the new one
                                                        //now query for new row.

                                                        var LossNew = dbloss.tbllossescodes.Where(m => m.LossCode == codename && m.LossCodesLevel == 3 && m.MessageType == messageType && m.LossCodesLevel1ID == LossCodesLevel1ID && m.LossCodesLevel2ID == LossCodesLevel2ID && m.IsDeleted == 0).FirstOrDefault();
                                                        if (LossNew != null)
                                                        {
                                                            int NewLossID = LossNew.LossCodeID;
                                                            var AllSubLevelLosses = dbloss.tbllossescodes.Where(m => m.LossCodesLevel2ID == NewLossID && m.IsDeleted == 0).ToList();
                                                            foreach (var sublosses in AllSubLevelLosses)
                                                            {
                                                                sublosses.LossCodesLevel1ID = NewLossID;
                                                                dbloss.Entry(sublosses).State = EntityState.Modified;
                                                                dbloss.SaveChanges();
                                                            }
                                                        }
                                                    }
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
                                                        string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                                        if (Regex.IsMatch(EndCode, @"^\d+$"))
                                                        {
                                                            cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`LossCodesLevel1ID`,`LossCodesLevel2ID`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`,`EndCode`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType.ToUpper() + "' , " + level + " ,'" + LossCodesLevel1ID + "' ,'" + LossCodesLevel2ID + "', '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "', '" + EndCode + "')", mc1.msqlConnection);
                                                        }
                                                        else
                                                        {
                                                            cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`LossCodesLevel1ID`,`LossCodesLevel2ID`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType + "' , " + level + " ,'" + LossCodesLevel1ID + "' ,'" + LossCodesLevel2ID + "', '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
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
                else if (UploadType == "Update")
                {
                    #region
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        bool IsValid = false;
                        //Validations
                        int level = 0;
                        int.TryParse(ds.Tables[0].Rows[i][3].ToString(), out level);
                        if (level == 1)
                        {
                            //1st Check for Empty
                            //Check if it already exits if so update that or insert new .
                            #region
                            string codename = Convert.ToString(ds.Tables[0].Rows[i][0]);
                            string codedesc = Convert.ToString(ds.Tables[0].Rows[i][5]);
                            if (!string.IsNullOrEmpty(codename) && !string.IsNullOrEmpty(codedesc))
                            {
                                //Validate MessageType
                                string messageType = Convert.ToString(ds.Tables[0].Rows[i][6]);
                                if (string.IsNullOrEmpty(messageType) || messageType.Length > 30)
                                {
                                    Errors += "MessageType for " + codename + " Cannot be Null & MaxLength less than 30 ";
                                    continue;
                                }
                                //else if ((String.Compare(messageType, "Setup", StringComparison.OrdinalIgnoreCase) != 0)
                                //           || (String.Compare(messageType, "IDLE", StringComparison.OrdinalIgnoreCase) != 0)
                                //    || (String.Compare(messageType, "BREAKDOWN", StringComparison.OrdinalIgnoreCase) == 0)
                                //          )

                                else if (!MessageType.Any(s => s.Equals(messageType, StringComparison.OrdinalIgnoreCase)))
                                {
                                    Errors += "Invalid MessageType for " + codename + "It has to be('Setup' or 'IDLE' or 'BREAKDOWN')";
                                    continue;
                                }

                                //var DupData = db.tbllossescodes.Where(m => m.LossCode == codename && m.MessageType == messageType).Where(m => m.IsDeleted == 0).FirstOrDefault();
                                //if (DupData != null)
                                //{
                                //    Errors += "Duplicate Code : " + DupData.LossCode;
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
                                    //Just to make sure the case is similar for all losses.
                                    if (String.Compare(messageType, "Setup", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        messageType = "Setup";
                                    }
                                    else if (String.Compare(messageType, "IDLE", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        messageType = "IDLE";
                                    }
                                    if (String.Compare(messageType, "BREAKDOWN", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        messageType = "BREAKDOWN";
                                    }

                                    string contributesTo = Convert.ToString(ds.Tables[0].Rows[i][7]).ToUpper();
                                    //if (string.IsNullOrEmpty(contributesTo) && messageType != "BREAKDOWN")
                                    if (string.IsNullOrEmpty(contributesTo))
                                    {
                                        contributesTo = "ROA";
                                    }

                                    string EndCode = Convert.ToString(ds.Tables[0].Rows[i][4]).Trim();

                                    if (EndCode.Length > 0 && !Regex.IsMatch(EndCode, @"^\d+$"))
                                    {
                                        Errors += "EndCode for " + codename + " must be Integer.";
                                        continue;
                                    }

                                    //Delete if loss exists with same codename and Details
                                    var LossDup = db.tbllossescodes.Where(m => m.LossCode == codename && m.LossCodesLevel == 1 && m.MessageType == messageType && m.IsDeleted == 0).FirstOrDefault();
                                    if (LossDup != null)
                                    {
                                        try
                                        {
                                            // if any loss is running then you are screwed.
                                            // well u can update its id or throw error about this update

                                            //insert new row only then u will get the Latest id.
                                            MsqlConnection mc1 = new MsqlConnection();
                                            mc1.open();
                                            string dat = DateTime.Now.ToString();
                                            dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                            SqlCommand cmd2 = null;
                                            try
                                            {
                                                string LossCodesLevel1ID = null;
                                                string LossCodesLevel2ID = null;
                                                string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                                if (Regex.IsMatch(EndCode, @"^\d+$"))
                                                {
                                                    //cmd2 = new SqlCommand("Update tbllossescodes set `LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`,`EndCode`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType + "' , " + level + " , '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "', '" + EndCode + "')", mc1.msqlConnection);
                                                    cmd2 = new SqlCommand("Update tbllossescodes set LossCodeDesc = '" + codedesc + "',MessageType = '" + messageType + "',LossCodesLevel = '" + level + "', ContributeTo ='" + contributesTo + "',ModifiedOn='" + datetimeNow + "',ModifiedBy = '" + Convert.ToInt32(Session["UserId"]) + "',EndCode ='" + EndCode + "'  where LossCodeID = '" + LossDup.LossCodeID + "'", mc1.msqlConnection);
                                                }
                                                else
                                                {
                                                    //cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType + "' , " + level + " , '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
                                                    cmd2 = new SqlCommand("Update tbllossescodes set LossCodeDesc = '" + codedesc + "',MessageType = '" + messageType + "',LossCodesLevel = '" + level + "', ContributeTo ='" + contributesTo + "',ModifiedOn='" + datetimeNow + "',ModifiedBy = '" + Convert.ToInt32(Session["UserId"]) + "'  where LossCodeID = '" + LossDup.LossCodeID + "'", mc1.msqlConnection);
                                                }

                                                cmd2.ExecuteNonQuery();
                                            }
                                            catch (Exception e)
                                            {
                                                Errors += e.ToString();
                                            }
                                            mc1.close();
                                        }
                                        catch (Exception e)
                                        {
                                        }
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
                                            string LossCodesLevel1ID = null;
                                            string LossCodesLevel2ID = null;
                                            string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                            if (Regex.IsMatch(EndCode, @"^\d+$"))
                                            {
                                                cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`,`EndCode`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType + "' , " + level + " , '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "', '" + EndCode + "')", mc1.msqlConnection);
                                            }
                                            else
                                            {
                                                cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType + "' , " + level + " , '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
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
                                Errors += "Please Enter both Code Name and Description ";
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
                            string codedesc = Convert.ToString(ds.Tables[0].Rows[i][5]);

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
                                    //Its Level1 code Valid.
                                    if (!string.IsNullOrEmpty(Level1Code))
                                    {

                                        //Validate MessageType
                                        string messageType = Convert.ToString(ds.Tables[0].Rows[i][6]);
                                        if (string.IsNullOrEmpty(messageType) || messageType.Length > 30)
                                        {
                                            Errors += "MessageType for " + codename + " Cannot be Null or MaxLength(30)";
                                            continue;
                                        }
                                        //else if ((String.Compare(messageType, "Setup", StringComparison.OrdinalIgnoreCase) != 0)
                                        //   || (String.Compare(messageType, "IDLE", StringComparison.OrdinalIgnoreCase) != 0)
                                        //    || (String.Compare(messageType, "BREAKDOWN", StringComparison.OrdinalIgnoreCase) == 0)
                                        //  )
                                        else if (!MessageType.Any(s => s.Equals(messageType, StringComparison.OrdinalIgnoreCase)))
                                        {
                                            Errors += "Invalid MessageType for " + codename + "It has to be('Setup' or 'IDLE' or 'BREAKDOWN')";
                                            continue;
                                        }

                                        //Just to make sure the case is similar for all losses.
                                        if (String.Compare(messageType, "Setup", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            messageType = "Setup";
                                        }
                                        else if (String.Compare(messageType, "IDLE", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            messageType = "IDLE";
                                        }
                                        if (String.Compare(messageType, "BREAKDOWN", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            messageType = "BREAKDOWN";
                                        }

                                        var Level1Data = db.tbllossescodes.Where(m => m.LossCode == Level1Code && m.MessageType == messageType && m.IsDeleted == 0).FirstOrDefault();
                                        if (Level1Data != null)
                                        {
                                            int LossCodesLevel1ID = Level1Data.LossCodeID;
                                            if (Level1Data.LossCodesLevel != 1)
                                            {
                                                Errors += "InValid Level1 Code for " + codename + " ";
                                                continue;
                                            }
                                            else
                                            {
                                                string contributesTo = Convert.ToString(ds.Tables[0].Rows[i][7]).ToUpper();
                                                if (string.IsNullOrEmpty(contributesTo))
                                                {
                                                    contributesTo = "ROA";
                                                }
                                                string EndCode = Convert.ToString(ds.Tables[0].Rows[i][4]).Trim();
                                                if (EndCode.Length > 0 && !Regex.IsMatch(EndCode, @"^\d+$"))
                                                {
                                                    Errors += "EndCode for " + codename + " must be Integer.";
                                                    continue;
                                                }

                                                //Delete if loss exists with same codename and Details
                                                var LossDup = db.tbllossescodes.Where(m => m.LossCode == codename && m.LossCodesLevel == 2 && m.MessageType == messageType && m.LossCodesLevel1ID == LossCodesLevel1ID && m.IsDeleted == 0).FirstOrDefault();
                                                if (LossDup != null)
                                                {
                                                    MsqlConnection mc1 = new MsqlConnection();
                                                    mc1.open();
                                                    string dat = DateTime.Now.ToString();
                                                    dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                    SqlCommand cmd2 = null;
                                                    try
                                                    {
                                                        string LossCodesLevel2ID = null;
                                                        string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                        if (Regex.IsMatch(EndCode, @"^\d+$"))
                                                        {
                                                            //cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`LossCodesLevel1ID`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`,`EndCode`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType + "' , " + level + " ,'" + LossCodesLevel1ID + "', '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "', '" + EndCode + "')", mc1.msqlConnection);
                                                            cmd2 = new SqlCommand("Update tbllossescodes set LossCodeDesc = '" + codedesc + "',MessageType = '" + messageType + "' ,LossCodesLevel1ID= '" + LossCodesLevel1ID + "' ,LossCodesLevel = '" + level + "', ContributeTo ='" + contributesTo + "',ModifiedOn='" + datetimeNow + "',ModifiedBy = '" + Convert.ToInt32(Session["UserId"]) + "',EndCode ='" + EndCode + "'  where LossCodeID = '" + LossDup.LossCodeID + "'", mc1.msqlConnection);
                                                        }
                                                        else
                                                        {
                                                            //cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`LossCodesLevel1ID`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType + "' , " + level + " ,'" + LossCodesLevel1ID + "', '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
                                                            cmd2 = new SqlCommand("Update tbllossescodes set LossCodeDesc = '" + codedesc + "',MessageType = '" + messageType + "' ,LossCodesLevel1ID= '" + LossCodesLevel1ID + "' ,LossCodesLevel = '" + level + "', ContributeTo ='" + contributesTo + "',ModifiedOn='" + datetimeNow + "',ModifiedBy = '" + Convert.ToInt32(Session["UserId"]) + "' where LossCodeID = '" + LossDup.LossCodeID + "'", mc1.msqlConnection);
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
                                                    MsqlConnection mc1 = new MsqlConnection();
                                                    mc1.open();
                                                    string dat = DateTime.Now.ToString();
                                                    dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                    SqlCommand cmd2 = null;
                                                    try
                                                    {
                                                        string LossCodesLevel2ID = null;
                                                        string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                        if (Regex.IsMatch(EndCode, @"^\d+$"))
                                                        {
                                                            cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`LossCodesLevel1ID`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`,`EndCode`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType + "' , " + level + " ,'" + LossCodesLevel1ID + "', '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "', '" + EndCode + "')", mc1.msqlConnection);
                                                        }
                                                        else
                                                        {
                                                            cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`LossCodesLevel1ID`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType + "' , " + level + " ,'" + LossCodesLevel1ID + "', '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
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
                            string codedesc = Convert.ToString(ds.Tables[0].Rows[i][5]);

                            if (!string.IsNullOrEmpty(codename) && !string.IsNullOrEmpty(codedesc))
                            {
                                //var DupData = db.tbllossescodes.Where(m => m.LossCode == codename).Where(m => m.IsDeleted == 0).FirstOrDefault();
                                //if (DupData != null)
                                //{
                                //    Errors += "Duplicate Code : " + DupData.LossCode;
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
                                    string Level2Code = Convert.ToString(ds.Tables[0].Rows[i][1]);

                                    //Is Level1 code Valid.
                                    if (!string.IsNullOrEmpty(Level1Code) && !string.IsNullOrEmpty(Level2Code))
                                    {

                                        //Validate MessageType
                                        string messageType = Convert.ToString(ds.Tables[0].Rows[i][6]);
                                        if (string.IsNullOrEmpty(messageType) || messageType.Length > 30)
                                        {
                                            Errors += "MessageType for " + codename + " Cannot be Null or MaxLength(30)";
                                            continue;
                                        }
                                        //else if ((String.Compare(messageType, "Setup", StringComparison.OrdinalIgnoreCase) != 0)
                                        //    || (String.Compare(messageType, "IDLE", StringComparison.OrdinalIgnoreCase) != 0)
                                        //    || (String.Compare(messageType, "BREAKDOWN", StringComparison.OrdinalIgnoreCase) == 0)
                                        //   )
                                        // || (String.Compare(messageType, "BREAKDOWN", StringComparison.OrdinalIgnoreCase) == 0)
                                        // messageType != "Setup" || messageType != "IDLE" || messageType != "BREAKDOWN")

                                        else if (!MessageType.Any(s => s.Equals(messageType, StringComparison.OrdinalIgnoreCase)))
                                        {
                                            Errors += "Invalid MessageType for " + codename + "It has to be('Setup' or 'IDLE' or 'BREAKDOWN')";
                                            continue;
                                        }

                                        //Just to make sure the case is similar for all losses.
                                        if (String.Compare(messageType, "Setup", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            messageType = "Setup";
                                        }
                                        else if (String.Compare(messageType, "IDLE", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            messageType = "IDLE";
                                        }
                                        if (String.Compare(messageType, "BREAKDOWN", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            messageType = "BREAKDOWN";
                                        }

                                        var Level1Data = db.tbllossescodes.Where(m => m.LossCode == Level1Code && m.MessageType == messageType && m.IsDeleted == 0).FirstOrDefault();
                                        var Level2Data = db.tbllossescodes.Where(m => m.LossCode == Level2Code && m.MessageType == messageType && m.IsDeleted == 0).FirstOrDefault();

                                        if (Level1Data != null && Level2Data != null)
                                        {
                                            if (Level1Data.LossCodesLevel != 1)
                                            {
                                                Errors += "InValid Level1 Code for " + codename + " ";
                                                continue;
                                            }
                                            if (Level2Data.LossCodesLevel != 2)
                                            {
                                                Errors += "InValid Level2 Code for " + codename + " ";
                                                continue;
                                            }

                                            //Level 1 && 2 are not relevant to each other.
                                            var Level2sLevel1 = Level2Data.LossCodesLevel1ID;
                                            if (Level2sLevel1 == Level1Data.LossCodeID)
                                            {
                                                int LossCodesLevel1ID = Level1Data.LossCodeID;
                                                int LossCodesLevel2ID = Level2Data.LossCodeID;

                                                string contributesTo = Convert.ToString(ds.Tables[0].Rows[i][7]).ToUpper();
                                                if (string.IsNullOrEmpty(contributesTo))
                                                {
                                                    contributesTo = "ROA";
                                                }
                                                string EndCode = Convert.ToString(ds.Tables[0].Rows[i][4]).Trim();

                                                if (EndCode.Length > 0 && !Regex.IsMatch(EndCode, @"^\d+$"))
                                                {
                                                    Errors += "EndCode for " + codename + " must be Integer.";
                                                    continue;
                                                }

                                                //Delete if loss exists with same codename and Details
                                                var LossDup = db.tbllossescodes.Where(m => m.LossCode == codename && m.LossCodesLevel == 3 && m.MessageType == messageType && m.LossCodesLevel1ID == LossCodesLevel1ID && m.LossCodesLevel2ID == LossCodesLevel2ID && m.IsDeleted == 0).FirstOrDefault();
                                                if (LossDup != null)
                                                {

                                                    MsqlConnection mc1 = new MsqlConnection();
                                                    mc1.open();
                                                    string dat = DateTime.Now.ToString();
                                                    dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                    SqlCommand cmd2 = null;
                                                    try
                                                    {
                                                        string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                                        if (Regex.IsMatch(EndCode, @"^\d+$"))
                                                        {
                                                            //cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`LossCodesLevel1ID`,`LossCodesLevel2ID`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`,`EndCode`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType.ToUpper() + "' , " + level + " ,'" + LossCodesLevel1ID + "' ,'" + LossCodesLevel2ID + "', '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "', '" + EndCode + "')", mc1.msqlConnection);
                                                            cmd2 = new SqlCommand("Update tbllossescodes set LossCodeDesc = '" + codedesc + "',MessageType = '" + messageType + "' ,LossCodesLevel1ID= '" + LossCodesLevel1ID + "', LossCodesLevel2ID= '" + LossCodesLevel2ID + "' ,LossCodesLevel = '" + level + "', ContributeTo ='" + contributesTo + "',ModifiedOn='" + datetimeNow + "',ModifiedBy = '" + Convert.ToInt32(Session["UserId"]) + "' ,EndCode = '" + EndCode + "'  where LossCodeID = '" + LossDup.LossCodeID + "'", mc1.msqlConnection);
                                                        }
                                                        else
                                                        {
                                                            //cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`LossCodesLevel1ID`,`LossCodesLevel2ID`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType + "' , " + level + " ,'" + LossCodesLevel1ID + "' ,'" + LossCodesLevel2ID + "', '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
                                                            cmd2 = new SqlCommand("Update tbllossescodes set LossCodeDesc = '" + codedesc + "',MessageType = '" + messageType + "' ,LossCodesLevel1ID= '" + LossCodesLevel1ID + "', LossCodesLevel2ID= '" + LossCodesLevel2ID + "' ,LossCodesLevel = '" + level + "', ContributeTo ='" + contributesTo + "',ModifiedOn='" + datetimeNow + "',ModifiedBy = '" + Convert.ToInt32(Session["UserId"]) + "' where LossCodeID = '" + LossDup.LossCodeID + "'", mc1.msqlConnection);
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
                                                    MsqlConnection mc1 = new MsqlConnection();
                                                    mc1.open();
                                                    string dat = DateTime.Now.ToString();
                                                    dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                    SqlCommand cmd2 = null;
                                                    try
                                                    {
                                                        string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                                        if (Regex.IsMatch(EndCode, @"^\d+$"))
                                                        {
                                                            cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`LossCodesLevel1ID`,`LossCodesLevel2ID`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`,`EndCode`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType.ToUpper() + "' , " + level + " ,'" + LossCodesLevel1ID + "' ,'" + LossCodesLevel2ID + "', '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "', '" + EndCode + "')", mc1.msqlConnection);
                                                        }
                                                        else
                                                        {
                                                            cmd2 = new SqlCommand("INSERT INTO tbllossescodes (`LossCode`,`LossCodeDesc`,`MessageType`,`LossCodesLevel`,`LossCodesLevel1ID`,`LossCodesLevel2ID`,`ContributeTo`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType + "' , " + level + " ,'" + LossCodesLevel1ID + "' ,'" + LossCodesLevel2ID + "', '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
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
            //return RedirectToAction("Index");
            return View();
        }

        public ActionResult ExportIdleCodesDetails()
        {

            #region Excel and Stuff

            DateTime frda = DateTime.Now;
            String FileDir = @"C:\TataReport\ReportsList\" + System.DateTime.Now.ToString("yyyy-MM-dd");
            bool exists = System.IO.Directory.Exists(FileDir);
            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "IdleList" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "IdleList" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //" to " + toda.ToString("yyyy-MM-dd") + 
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
            worksheet.Cells["H" + 1].Value = "Contributes To";
            worksheet.Cells["A1:I1"].Style.Font.Bold = true;

            #endregion
            var BreakDownLossesData = db.tbllossescodes.Where(m => m.MessageType != "BREAKDOWN" && m.LossCodeID != 999 && m.IsDeleted == 0).OrderBy(m => m.LossCodesLevel1ID).ThenBy(m => m.LossCodesLevel2ID).ToList();
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
                    worksheet.Cells["H" + i].Value = row.ContributeTo;
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
                    worksheet.Cells["H" + i].Value = row.ContributeTo;
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
                    worksheet.Cells["H" + i].Value = row.ContributeTo;
                    //worksheet.Cells["I" + i].Value = row.ContributeTo;
                }
                i++;
            }

            #region Save and Download

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            p.Save();

            //Downloding Excel
            string path1 = System.IO.Path.Combine(FileDir, "IdleList" + frda.ToString("yyyy-MM-dd") + ".xlsx");
            System.IO.FileInfo file1 = new System.IO.FileInfo(path1);
            string Outgoingfile = "IdleList" + frda.ToString("yyyy-MM-dd") + ".xlsx";
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
