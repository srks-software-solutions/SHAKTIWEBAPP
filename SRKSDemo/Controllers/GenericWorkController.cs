
using OfficeOpenXml;
using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace SRKSDemo.Controllers
{
    public class GenericWorkController : Controller
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
            var query = from c in db.tblgenericworkcodes
                        where (!(from o in db.tblgenericworkcodes
                                 where o.IsDeleted == 0
                                 select o.GWCodesLevel1ID)
                        .Contains(c.GenericWorkID))
                        &&
                                (!(from p in db.tblgenericworkcodes
                                   where p.IsDeleted == 0
                                   select p.GWCodesLevel2ID)
                                   .Contains(c.GenericWorkID))
                        &&
                        c.IsDeleted == 0
                        //&& c.GenericWorkCode != "999"
                        //&& c.MessageType != "BREAKDOWN"
                        //&& c.MessageType != "PM"
                        orderby c.GWCodesLevel1ID, c.GWCodesLevel1ID
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
            ViewData["Level1"] = new SelectList(db.tblgenericworkcodes.Where(m => m.IsDeleted == 0 && m.GWCodesLevel == 1), "GenericWorkID", "GenericWorkDesc");
            //keep level2 codes empty, based on level1 populate level2
            ViewData["Level2"] = new SelectList(db.tblgenericworkcodes.Where(m => m.IsDeleted == 0 && m.GWCodesLevel == 4), "GenericWorkID", "GenericWorkDesc");
            //ViewData["Level2"] = new SelectList(null);
            return View();
        }
        [HttpPost]
        public ActionResult Create(tblgenericworkcode tlc, int Level1 = 0, int Level2 = 0)
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

            ViewData["Level1"] = new SelectList(db.tblgenericworkcodes.Where(m => m.IsDeleted == 0 && m.GWCodesLevel == 1), "GenericWorkID", "GenericWorkDesc");
            ViewData["Level2"] = new SelectList(db.tblgenericworkcodes.Where(m => m.IsDeleted == 0 && m.GWCodesLevel == 2), "GenericWorkID", "GenericWorkDesc");

            if (Convert.ToInt16(tlc.GWCodesLevel) == 1)
            {
                var duplosscode = db.tblgenericworkcodes.Where(m => m.IsDeleted == 0 && m.GenericWorkCode == tlc.GenericWorkCode).ToList();
                if (duplosscode.Count == 0)
                {
                    db.tblgenericworkcodes.Add(tlc);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "GenericWork code already Exist.";
                    return View(tlc);
                }
            }
            if (Convert.ToInt16(tlc.GWCodesLevel) == 2)
            {
                tlc.GWCodesLevel1ID = Level1;

                var duplosscode = db.tblgenericworkcodes.Where(m => m.IsDeleted == 0 && m.GenericWorkCode == tlc.GenericWorkCode && m.GWCodesLevel1ID == tlc.GWCodesLevel1ID).ToList();
                if (duplosscode.Count == 0)
                {
                    //Here check if Level1 code is used in tbllossofentry, if so then create new copy of it with Different LossID.
                    var lossOfEntryData = db.tblgenericworkentries.Where(m => m.GWCodeID == Level1).ToList();
                    if (lossOfEntryData.Count > 0)
                    {
                        var lossPrvData = db.tblgenericworkcodes.Where(m => m.GenericWorkID == Level1 && m.IsDeleted == 0).FirstOrDefault();

                        tblgenericworkcode tlcNewPrvLevel = new tblgenericworkcode();
                        tlcNewPrvLevel.CreatedBy = lossPrvData.CreatedBy;
                        tlcNewPrvLevel.CreatedOn = lossPrvData.CreatedOn;
                        tlcNewPrvLevel.IsDeleted = 0;
                        tlcNewPrvLevel.GenericWorkCode = lossPrvData.GenericWorkCode;
                        tlcNewPrvLevel.GenericWorkDesc = lossPrvData.GenericWorkDesc;
                        tlcNewPrvLevel.GWCodesLevel = lossPrvData.GWCodesLevel;
                        tlcNewPrvLevel.GWCodesLevel1ID = lossPrvData.GWCodesLevel1ID;
                        tlcNewPrvLevel.GWCodesLevel2ID = lossPrvData.GWCodesLevel2ID;
                        tlcNewPrvLevel.MessageType = lossPrvData.MessageType;
                        tlcNewPrvLevel.ModifiedBy = lossPrvData.ModifiedBy;
                        tlcNewPrvLevel.ModifiedOn = lossPrvData.ModifiedOn;

                        db.tblgenericworkcodes.Add(tlcNewPrvLevel);
                        db.SaveChanges();

                        //Delete the Old one.
                        lossPrvData.IsDeleted = 1;
                        lossPrvData.ModifiedOn = DateTime.Now;
                        lossPrvData.ModifiedBy = UserID;
                        db.Entry(lossPrvData).State = EntityState.Modified;
                        db.SaveChanges();

                        //Give new Level1 GenericWorkID to 2nd level code.
                        int Level1LossCodeID = tlcNewPrvLevel.GenericWorkID;
                        tlc.GWCodesLevel1ID = Level1LossCodeID;

                    }
                    db.tblgenericworkcodes.Add(tlc);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "GenericWork code already Exist.";
                    return View(tlc);
                }
            }
            if (Convert.ToInt16(tlc.GWCodesLevel) == 3)
            {
                tlc.GWCodesLevel1ID = Level1;
                tlc.GWCodesLevel2ID = Level2;
                var duplosscode = db.tblgenericworkcodes.Where(m => m.IsDeleted == 0 && m.GenericWorkCode == tlc.GenericWorkCode && m.GWCodesLevel1ID == tlc.GWCodesLevel1ID && m.GWCodesLevel2ID == tlc.GWCodesLevel2ID).ToList();
                if (duplosscode.Count == 0)
                {
                    //Here check if Level1 code is used in tbllossofentry, if so then create new copy of it with Different LossID.
                    var lossOfEntryData = db.tblgenericworkentries.Where(m => m.GWCodeID == Level2).ToList();
                    if (lossOfEntryData.Count > 0)
                    {
                        var lossPrvData = db.tblgenericworkcodes.Where(m => m.GenericWorkID == Level2 && m.IsDeleted == 0).FirstOrDefault();

                        tblgenericworkcode tlcNewPrvLevel = new tblgenericworkcode();
                        tlcNewPrvLevel.CreatedBy = lossPrvData.CreatedBy;
                        tlcNewPrvLevel.CreatedOn = lossPrvData.CreatedOn;
                        tlcNewPrvLevel.IsDeleted = 0;
                        tlcNewPrvLevel.GenericWorkCode = lossPrvData.GenericWorkCode;
                        tlcNewPrvLevel.GenericWorkDesc = lossPrvData.GenericWorkDesc;
                        tlcNewPrvLevel.GWCodesLevel = lossPrvData.GWCodesLevel;
                        tlcNewPrvLevel.GWCodesLevel1ID = lossPrvData.GWCodesLevel1ID;
                        tlcNewPrvLevel.GWCodesLevel2ID = lossPrvData.GWCodesLevel2ID;
                        tlcNewPrvLevel.MessageType = lossPrvData.MessageType;
                        tlcNewPrvLevel.ModifiedBy = lossPrvData.ModifiedBy;
                        tlcNewPrvLevel.ModifiedOn = lossPrvData.ModifiedOn;

                        db.tblgenericworkcodes.Add(tlcNewPrvLevel);
                        db.SaveChanges();

                        //Delete the Old one.
                        lossPrvData.IsDeleted = 1;
                        lossPrvData.ModifiedOn = DateTime.Now;
                        lossPrvData.ModifiedBy = UserID;
                        db.Entry(lossPrvData).State = EntityState.Modified;
                        db.SaveChanges();

                        //Give new Level1 GenericWorkID to 2nd level code.
                        int Level2LossCodeID = tlcNewPrvLevel.GenericWorkID;
                        tlc.GWCodesLevel2ID = Level2LossCodeID;

                    }

                    db.tblgenericworkcodes.Add(tlc);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "GenericWork code already Exist.";
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
            tblgenericworkcode lossdata = db.tblgenericworkcodes.Find(id);

            ViewData["Level1"] = new SelectList(db.tblgenericworkcodes.Where(m => m.IsDeleted == 0 && m.GWCodesLevel == 1 && m.GenericWorkCode != "999" && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "GenericWorkID", "GenericWorkDesc", lossdata.GWCodesLevel1ID);
            ViewData["Level2"] = new SelectList(db.tblgenericworkcodes.Where(m => m.IsDeleted == 0 && m.GWCodesLevel == 2 && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "GenericWorkID", "GenericWorkDesc", lossdata.GWCodesLevel2ID);
            ViewBag.radioselected = lossdata.GWCodesLevel;
            ViewData["MessageType"] = lossdata.MessageType;
            return View(lossdata);
        }
        [HttpPost]
        public ActionResult Edit(tblgenericworkcode tlc, int Level1 = 0, int Level2 = 0)
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

            if (Convert.ToInt16(tlc.GWCodesLevel) == 1)
            {
                var duplosscode = db.tblgenericworkcodes.Where(m => m.IsDeleted == 0 && m.GenericWorkCode == tlc.GenericWorkCode && m.GenericWorkID != tlc.GenericWorkID).ToList();
                if (duplosscode.Count == 0)
                {
                    db.Entry(tlc).State = EntityState.Modified;
                    db.SaveChanges();
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "GenericWork code already Exist.";
                    ViewData["Level1"] = new SelectList(db.tblgenericworkcodes.Where(m => m.IsDeleted == 0 && m.GWCodesLevel == 1 && m.GenericWorkCode != "999" && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "GenericWorkID", "GenericWorkDesc", tlc.GWCodesLevel1ID);
                    ViewData["Level2"] = new SelectList(db.tblgenericworkcodes.Where(m => m.IsDeleted == 0 && m.GWCodesLevel == 2 && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "GenericWorkID", "GenericWorkDesc", tlc.GWCodesLevel2ID);
                    ViewBag.radioselected = tlc.GWCodesLevel;

                    return View(tlc);
                }
            }

            if (Convert.ToInt16(tlc.GWCodesLevel) == 2)
            {
                tlc.GWCodesLevel1ID = Level1;
                var duplosscode = db.tblgenericworkcodes.Where(m => m.IsDeleted == 0 && m.GenericWorkCode == tlc.GenericWorkCode && m.GenericWorkID != tlc.GenericWorkID && m.GWCodesLevel1ID == tlc.GWCodesLevel1ID).ToList();
                if (duplosscode.Count == 0)
                {
                    db.Entry(tlc).State = EntityState.Modified;
                    db.SaveChanges();
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "GenericWork code already Exist.";
                    ViewData["Level1"] = new SelectList(db.tblgenericworkcodes.Where(m => m.IsDeleted == 0 && m.GWCodesLevel == 1 && m.GenericWorkCode != "999" && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "GenericWorkID", "GenericWorkDesc", tlc.GWCodesLevel1ID);
                    ViewData["Level2"] = new SelectList(db.tblgenericworkcodes.Where(m => m.IsDeleted == 0 && m.GWCodesLevel == 2 && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "GenericWorkID", "GenericWorkDesc", tlc.GWCodesLevel2ID);
                    ViewBag.radioselected = tlc.GWCodesLevel;

                    return View(tlc);
                }
            }
            if (Convert.ToInt16(tlc.GWCodesLevel) == 3)
            {
                tlc.GWCodesLevel1ID = Level1;
                tlc.GWCodesLevel2ID = Level2;
                var duplosscode = db.tblgenericworkcodes.Where(m => m.IsDeleted == 0 && m.GenericWorkCode == tlc.GenericWorkCode && m.GenericWorkID != tlc.GenericWorkID && m.GWCodesLevel1ID == tlc.GWCodesLevel1ID && m.GWCodesLevel2ID == tlc.GWCodesLevel2ID).ToList();
                if (duplosscode.Count == 0)
                {
                    db.Entry(tlc).State = EntityState.Modified;
                    db.SaveChanges();
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    Session["Error"] = "GenericWork code already Exist.";
                    ViewData["Level1"] = new SelectList(db.tblgenericworkcodes.Where(m => m.IsDeleted == 0 && m.GWCodesLevel == 1 && m.GenericWorkCode != "999" && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "GenericWorkID", "GenericWorkDesc", tlc.GWCodesLevel1ID);
                    ViewData["Level2"] = new SelectList(db.tblgenericworkcodes.Where(m => m.IsDeleted == 0 && m.GWCodesLevel == 2 && m.MessageType != "PM" && m.MessageType != "BREAKDOWN"), "GenericWorkID", "GenericWorkDesc", tlc.GWCodesLevel2ID);
                    ViewBag.radioselected = tlc.GWCodesLevel;

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
            tblgenericworkcode tlc = db.tblgenericworkcodes.Find(id);
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
            var selectedRow = new SelectList(db.tblgenericworkcodes.Where(m => m.IsDeleted == 0 && m.GWCodesLevel == 2 && m.GWCodesLevel1ID == Id), "GenericWorkID", "GenericWorkDesc");
            return Json(selectedRow, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ImportGenericWorkCodeDetails()
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
        public ActionResult ImportGenericWorkCodeDetails(HttpPostedFileBase file, string UploadType)
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
                                var DupData = db.tblgenericworkcodes.Where(m => m.GenericWorkCode == codename && m.IsDeleted == 0).FirstOrDefault();
                                if (DupData != null)
                                {
                                    Errors += "Duplicate Code : " + DupData.GenericWorkCode;
                                    continue;
                                }
                                else if (codename.Length > 40 || codedesc.Length > 60)
                                {
                                    Errors += "LossCodez (MaxLength:40) and LossDesc (MaxLength:60)  " + DupData.GenericWorkCode;
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
                                        string GWCodesLevel1ID = null;
                                        string GWCodesLevel2ID = null;
                                        string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                        cmd2 = new SqlCommand("INSERT INTO tblgenericworkcodes (`GenericWorkCode`,`GenericWorkDesc`,`GWCodesLevel`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' ,  " + level + " , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
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
                            string codedesc = Convert.ToString(ds.Tables[0].Rows[i][4]);

                            if (!string.IsNullOrEmpty(codename) && !string.IsNullOrEmpty(codedesc))
                            {
                                var DupData = db.tblgenericworkcodes.Where(m => m.GenericWorkCode == codename).Where(m => m.IsDeleted == 0).FirstOrDefault();
                                if (DupData != null)
                                {
                                    Errors += "Duplicate Code : " + DupData.GenericWorkCode;
                                    continue;
                                }
                                else if (codename.Length > 40 || codedesc.Length > 60)
                                {
                                    Errors += "LossCodez (MaxLength:40) and LossDesc (MaxLength:60)  " + DupData.GenericWorkCode;
                                    continue;
                                }
                                else
                                {
                                    string Level1Code = Convert.ToString(ds.Tables[0].Rows[i][0]);

                                    //Is Level1 code Valid.
                                    if (!string.IsNullOrEmpty(Level1Code))
                                    {
                                        var Level1Data = db.tblgenericworkcodes.Where(m => m.GenericWorkCode == Level1Code && m.IsDeleted == 0).FirstOrDefault();
                                        if (Level1Data != null)
                                        {
                                            int GWCodesLevel1ID = Level1Data.GenericWorkID;
                                            if (Level1Data.GWCodesLevel != 1)
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

                                                    string GWCodesLevel2ID = null;
                                                    string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                                    //if (Regex.IsMatch(EndCode, @"^\d+$"))
                                                    //{
                                                    //    cmd2 = new SqlCommand("INSERT INTO tblgenericworkcodes (`GenericWorkCode`,`GenericWorkDesc`,`GWCodesLevel`,`GWCodesLevel1ID`,`IsDeleted`,`CreatedOn`,`CreatedBy`,`EndCode`) VALUES( '" + codename + "' , '" + codedesc + "' , '" + messageType + "' , " + level + " ,'" + GWCodesLevel1ID + "', '" + contributesTo + "' , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "', '" + EndCode + "')", mc1.msqlConnection);
                                                    //}
                                                    //else
                                                    //{
                                                    cmd2 = new SqlCommand("INSERT INTO tblgenericworkcodes (`GenericWorkCode`,`GenericWorkDesc`,`GWCodesLevel`,`GWCodesLevel1ID`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , " + level + " ,'" + GWCodesLevel1ID + "', 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
                                                    //}

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
                                var DupData = db.tblgenericworkcodes.Where(m => m.GenericWorkCode == codename).Where(m => m.IsDeleted == 0).FirstOrDefault();
                                if (DupData != null)
                                {
                                    Errors += "Duplicate Code : " + DupData.GenericWorkCode;
                                    continue;
                                }
                                else if (codename.Length > 40 || codedesc.Length > 60)
                                {
                                    Errors += "GenericWorkCodez (MaxLength:40) and LossDesc (MaxLength:60)  " + DupData.GenericWorkCode;
                                    continue;
                                }
                                else
                                {
                                    string Level1Code = Convert.ToString(ds.Tables[0].Rows[i][0]);
                                    string Level2Code = Convert.ToString(ds.Tables[0].Rows[i][1]);

                                    //Is Level1 code Valid.
                                    if (!string.IsNullOrEmpty(Level1Code) && !string.IsNullOrEmpty(Level2Code))
                                    {
                                        var Level1Data = db.tblgenericworkcodes.Where(m => m.GenericWorkCode == Level1Code && m.IsDeleted == 0).FirstOrDefault();
                                        var Level2Data = db.tblgenericworkcodes.Where(m => m.GenericWorkCode == Level2Code && m.IsDeleted == 0).FirstOrDefault();

                                        if (Level1Data != null && Level2Data != null)
                                        {
                                            if (Level1Data.GWCodesLevel != 1)
                                            {
                                                Errors += "InValid Level1 Code for " + codename + " ";
                                                continue;
                                            }
                                            if (Level2Data.GWCodesLevel != 2)
                                            {
                                                Errors += "InValid Level2 Code for " + codename + " ";
                                                continue;
                                            }

                                            //Level 1 && 2 are not relevant to each other.
                                            var Level2sLevel1 = Level2Data.GWCodesLevel1ID;
                                            if (Level2sLevel1 == Level1Data.GenericWorkID)
                                            {
                                                int GWCodesLevel1ID = Level1Data.GenericWorkID;
                                                int GWCodesLevel2ID = Level2Data.GenericWorkID;
                                                MsqlConnection mc1 = new MsqlConnection();
                                                mc1.open();
                                                string dat = DateTime.Now.ToString();
                                                dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                SqlCommand cmd2 = null;
                                                try
                                                {
                                                    string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                    cmd2 = new SqlCommand("INSERT INTO tblgenericworkcodes (`GenericWorkCode`,`GenericWorkDesc`,`GWCodesLevel`,`GWCodesLevel1ID`,`GWCodesLevel2ID`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , " + level + " ,'" + GWCodesLevel1ID + "' ,'" + GWCodesLevel2ID + "', 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
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
                                var DupData = db.tblgenericworkcodes.Where(m => m.GenericWorkCode == codename && m.IsDeleted == 0).FirstOrDefault();
                                if (DupData == null)
                                {
                                    if (codename.Length > 40 || codedesc.Length > 60)
                                    {
                                        Errors += "LossCodez (MaxLength:40) and LossDesc (MaxLength:60)  " + DupData.GenericWorkCode;
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

                                        DupData.IsDeleted = 1;
                                        DupData.DeletedDate = Convert.ToDateTime(datetimeNow);
                                        db.Entry(DupData).State = EntityState.Modified;
                                        db.SaveChanges();

                                        cmd2 = new SqlCommand("INSERT INTO tblgenericworkcodes (`GenericWorkCode`,`GenericWorkDesc`,`GWCodesLevel`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' ,  " + level + " , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
                                        cmd2.ExecuteNonQuery();
                                    }
                                    catch (Exception e)
                                    {
                                        Errors += e.ToString();
                                    }
                                    mc1.close();
                                }
                                else if (codename.Length > 40 || codedesc.Length > 60)
                                {
                                    Errors += "LossCodez (MaxLength:40) and LossDesc (MaxLength:60)  " + DupData.GenericWorkCode;
                                    continue;
                                }
                                else
                                {
                                    string GenericWorkCode = DupData.GenericWorkCode;
                                    string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                    MsqlConnection mc1 = new MsqlConnection();
                                    mc1.open();
                                    string dat = DateTime.Now.ToString();
                                    dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    SqlCommand cmd2 = null;
                                    try
                                    {
                                        DupData.IsDeleted = 1;
                                        DupData.DeletedDate = Convert.ToDateTime(datetimeNow);
                                        db.Entry(DupData).State = EntityState.Modified;
                                        db.SaveChanges();

                                        cmd2 = new SqlCommand("INSERT INTO tblgenericworkcodes (`GenericWorkCode`,`GenericWorkDesc`,`GWCodesLevel`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' ,  " + level + " , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
                                        cmd2.ExecuteNonQuery();
                                    }
                                    catch (Exception e)
                                    {
                                        Errors += e.ToString();
                                    }
                                    mc1.close();

                                    //update sublevels level1ID
                                    var NewGWData = db.tblgenericworkcodes.Where(m => m.GenericWorkCode == GenericWorkCode && m.IsDeleted == 0).FirstOrDefault();
                                    if (NewGWData != null)
                                    {
                                        int GenericWorkIDNew = NewGWData.GenericWorkID;
                                        var subLevelsData = db.tblgenericworkcodes.Where(m => m.GWCodesLevel1ID == GenericWorkIDNew && m.IsDeleted == 0).ToList();
                                        foreach (var row in subLevelsData)
                                        {
                                            row.GWCodesLevel1ID = GenericWorkIDNew;
                                            db.Entry(row).State = EntityState.Modified;
                                            db.SaveChanges();
                                        }
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
                            string codedesc = Convert.ToString(ds.Tables[0].Rows[i][4]);

                            if (!string.IsNullOrEmpty(codename) && !string.IsNullOrEmpty(codedesc))
                            {
                                var DupData = db.tblgenericworkcodes.Where(m => m.GenericWorkCode == codename).Where(m => m.IsDeleted == 0).FirstOrDefault();
                                if (DupData == null)
                                {
                                    if (codename.Length > 40 || codedesc.Length > 60)
                                    {
                                        Errors += "LossCodez (MaxLength:40) and LossDesc (MaxLength:60)  " + DupData.GenericWorkCode;
                                        continue;
                                    }

                                    //Insert new row if no duplicate
                                    string Level1Code = Convert.ToString(ds.Tables[0].Rows[i][0]);
                                    //Is Level1 code Valid.
                                    if (!string.IsNullOrEmpty(Level1Code))
                                    {
                                        var Level1Data = db.tblgenericworkcodes.Where(m => m.GenericWorkCode == Level1Code && m.IsDeleted == 0).FirstOrDefault();
                                        if (Level1Data != null)
                                        {
                                            int GWCodesLevel1ID = Level1Data.GenericWorkID;
                                            if (Level1Data.GWCodesLevel != 1)
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
                                                    string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                    cmd2 = new SqlCommand("INSERT INTO tblgenericworkcodes (`GenericWorkCode`,`GenericWorkDesc`,`GWCodesLevel`,`GWCodesLevel1ID`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , " + level + " ,'" + GWCodesLevel1ID + "', 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
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
                                else if (codename.Length > 40 || codedesc.Length > 60)
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
                                        var Level1Data = db.tblgenericworkcodes.Where(m => m.GenericWorkCode == Level1Code && m.IsDeleted == 0).FirstOrDefault();
                                        if (Level1Data != null)
                                        {
                                            int GWCodesLevel1ID = Level1Data.GenericWorkID;
                                            if (Level1Data.GWCodesLevel != 1)
                                            {
                                                Errors += "InValid Level1 Code for " + codename + " ";
                                                continue;
                                            }
                                            else
                                            {
                                                string GenericWorkCode = DupData.GenericWorkCode;
                                                string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                                MsqlConnection mc1 = new MsqlConnection();
                                                mc1.open();
                                                string dat = DateTime.Now.ToString();
                                                dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                SqlCommand cmd2 = null;
                                                try
                                                {
                                                    DupData.IsDeleted = 1;
                                                    DupData.DeletedDate = Convert.ToDateTime(datetimeNow);
                                                    db.Entry(DupData).State = EntityState.Modified;
                                                    db.SaveChanges();

                                                    cmd2 = new SqlCommand("INSERT INTO tblgenericworkcodes (`GenericWorkCode`,`GenericWorkDesc`,`GWCodesLevel`,`GWCodesLevel1ID`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , " + level + " ,'" + GWCodesLevel1ID + "', 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);

                                                    cmd2.ExecuteNonQuery();
                                                }
                                                catch (Exception e)
                                                {
                                                    Errors = e.ToString();
                                                }
                                                mc1.close();

                                                //update sublevels level1ID
                                                var NewGWData = db.tblgenericworkcodes.Where(m => m.GenericWorkCode == GenericWorkCode && m.IsDeleted == 0).FirstOrDefault();
                                                if (NewGWData != null)
                                                {
                                                    int GenericWorkIDNew = NewGWData.GenericWorkID;
                                                    var subLevelsData = db.tblgenericworkcodes.Where(m => m.GWCodesLevel2ID == GenericWorkIDNew && m.IsDeleted == 0).ToList();
                                                    foreach (var row in subLevelsData)
                                                    {
                                                        row.GWCodesLevel1ID = GenericWorkIDNew;
                                                        db.Entry(row).State = EntityState.Modified;
                                                        db.SaveChanges();
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
                            string codedesc = Convert.ToString(ds.Tables[0].Rows[i][4]);

                            if (!string.IsNullOrEmpty(codename) && !string.IsNullOrEmpty(codedesc))
                            {
                                var DupData = db.tblgenericworkcodes.Where(m => m.GenericWorkCode == codename).Where(m => m.IsDeleted == 0).FirstOrDefault();
                                if (DupData == null)
                                {
                                    //if no duplicate then insert new row
                                    if (codename.Length > 40 || codedesc.Length > 60)
                                    {
                                        Errors += "LossCodez (MaxLength:40) and LossDesc (MaxLength:60)  " + DupData.GenericWorkCode;
                                        continue;
                                    }

                                    string Level1Code = Convert.ToString(ds.Tables[0].Rows[i][0]);
                                    string Level2Code = Convert.ToString(ds.Tables[0].Rows[i][1]);

                                    //Is Level1 code Valid.
                                    if (!string.IsNullOrEmpty(Level1Code) && !string.IsNullOrEmpty(Level2Code))
                                    {
                                        var Level1Data = db.tblgenericworkcodes.Where(m => m.GenericWorkCode == Level1Code && m.IsDeleted == 0).FirstOrDefault();
                                        var Level2Data = db.tblgenericworkcodes.Where(m => m.GenericWorkCode == Level2Code && m.IsDeleted == 0).FirstOrDefault();

                                        if (Level1Data != null && Level2Data != null)
                                        {
                                            if (Level1Data.GWCodesLevel != 1)
                                            {
                                                Errors += "InValid Level1 Code for " + codename + " ";
                                                continue;
                                            }
                                            if (Level2Data.GWCodesLevel != 2)
                                            {
                                                Errors += "InValid Level2 Code for " + codename + " ";
                                                continue;
                                            }

                                            //Level 1 && 2 are not relevant to each other.
                                            var Level2sLevel1 = Level2Data.GWCodesLevel1ID;
                                            if (Level2sLevel1 == Level1Data.GenericWorkID)
                                            {
                                                int GWCodesLevel1ID = Level1Data.GenericWorkID;
                                                int GWCodesLevel2ID = Level2Data.GenericWorkID;
                                                MsqlConnection mc1 = new MsqlConnection();
                                                mc1.open();
                                                string dat = DateTime.Now.ToString();
                                                dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                SqlCommand cmd2 = null;
                                                try
                                                {
                                                    string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                    cmd2 = new SqlCommand("INSERT INTO tblgenericworkcodes (`GenericWorkCode`,`GenericWorkDesc`,`GWCodesLevel`,`GWCodesLevel1ID`,`GWCodesLevel2ID`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , " + level + " ,'" + GWCodesLevel1ID + "' ,'" + GWCodesLevel2ID + "', 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
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
                                else if (codename.Length > 40 || codedesc.Length > 60)
                                {
                                    Errors += "GenericWorkCodez (MaxLength:40) and LossDesc (MaxLength:60)  " + DupData.GenericWorkCode;
                                    continue;
                                }
                                else
                                {
                                    string Level1Code = Convert.ToString(ds.Tables[0].Rows[i][0]);
                                    string Level2Code = Convert.ToString(ds.Tables[0].Rows[i][1]);

                                    //Is Level1 code Valid.
                                    if (!string.IsNullOrEmpty(Level1Code) && !string.IsNullOrEmpty(Level2Code))
                                    {
                                        var Level1Data = db.tblgenericworkcodes.Where(m => m.GenericWorkCode == Level1Code && m.IsDeleted == 0).FirstOrDefault();
                                        var Level2Data = db.tblgenericworkcodes.Where(m => m.GenericWorkCode == Level2Code && m.IsDeleted == 0).FirstOrDefault();

                                        if (Level1Data != null && Level2Data != null)
                                        {
                                            if (Level1Data.GWCodesLevel != 1)
                                            {
                                                Errors += "InValid Level1 Code for " + codename + " ";
                                                continue;
                                            }
                                            if (Level2Data.GWCodesLevel != 2)
                                            {
                                                Errors += "InValid Level2 Code for " + codename + " ";
                                                continue;
                                            }

                                            //Level 1 && 2 are not relevant to each other.
                                            var Level2sLevel1 = Level2Data.GWCodesLevel1ID;
                                            if (Level2sLevel1 == Level1Data.GenericWorkID)
                                            {
                                                int GWCodesLevel1ID = Level1Data.GenericWorkID;
                                                int GWCodesLevel2ID = Level2Data.GenericWorkID;
                                                MsqlConnection mc1 = new MsqlConnection();
                                                mc1.open();
                                                string dat = DateTime.Now.ToString();
                                                dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                SqlCommand cmd2 = null;
                                                try
                                                {
                                                    string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                    DupData.IsDeleted = 1;
                                                    DupData.DeletedDate = Convert.ToDateTime(datetimeNow);
                                                    db.Entry(DupData).State = EntityState.Modified;
                                                    db.SaveChanges();

                                                    cmd2 = new SqlCommand("INSERT INTO tblgenericworkcodes (`GenericWorkCode`,`GenericWorkDesc`,`GWCodesLevel`,`GWCodesLevel1ID`,`GWCodesLevel2ID`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , " + level + " ,'" + GWCodesLevel1ID + "' ,'" + GWCodesLevel2ID + "', 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
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
                            //Check if it already exits , if so insert.
                            #region
                            string codename = Convert.ToString(ds.Tables[0].Rows[i][0]);
                            string codedesc = Convert.ToString(ds.Tables[0].Rows[i][4]);
                            if (!string.IsNullOrEmpty(codename) && !string.IsNullOrEmpty(codedesc))
                            {
                                //var DupData = db.tblgenericworkcodes.Where(m => m.GenericWorkCode == codename && m.IsDeleted == 0).FirstOrDefault();
                                //if (DupData == null)
                                //{
                                //    Errors += "No Such Code Exists: " + codename;
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
                                    var DupData = db.tblgenericworkcodes.Where(m => m.GenericWorkCode == codename && m.IsDeleted == 0).FirstOrDefault();
                                    if (DupData == null)
                                    {
                                        MsqlConnection mc1 = new MsqlConnection();
                                        mc1.open();
                                        string dat = DateTime.Now.ToString();
                                        dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                        SqlCommand cmd2 = null;
                                        try
                                        {
                                            string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                            cmd2 = new SqlCommand("UPDATE tblgenericworkcodes set GenericWorkDesc = '" + codedesc + "',ModifiedOn = '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',ModifiedBy = '" + Convert.ToInt32(Session["UserId"]) + "' where GenericWorkID = '" + DupData.GenericWorkID + "'", mc1.msqlConnection);
                                            cmd2.ExecuteNonQuery();
                                        }
                                        catch (Exception e)
                                        {
                                            Errors += e.ToString();
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
                                            cmd2 = new SqlCommand("INSERT INTO tblgenericworkcodes (`GenericWorkCode`,`GenericWorkDesc`,`GWCodesLevel`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' ,  " + level + " , 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
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
                            //Check if it already exits , if so insert.
                            #region
                            string codename = Convert.ToString(ds.Tables[0].Rows[i][1]);
                            string codedesc = Convert.ToString(ds.Tables[0].Rows[i][4]);

                            if (!string.IsNullOrEmpty(codename) && !string.IsNullOrEmpty(codedesc))
                            {
                                //var DupData = db.tblgenericworkcodes.Where(m => m.GenericWorkCode == codename).Where(m => m.IsDeleted == 0).FirstOrDefault();
                                //if (DupData == null)
                                //{
                                //    Errors += "Code Does not Exist : " + codename;
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
                                        var Level1Data = db.tblgenericworkcodes.Where(m => m.GenericWorkCode == Level1Code && m.IsDeleted == 0).FirstOrDefault();
                                        if (Level1Data != null)
                                        {
                                            int GWCodesLevel1ID = Level1Data.GenericWorkID;
                                            if (Level1Data.GWCodesLevel != 1)
                                            {
                                                Errors += "InValid Level1 Code for " + codename + " ";
                                                continue;
                                            }
                                            else
                                            {
                                                var DupData = db.tblgenericworkcodes.Where(m => m.GenericWorkCode == codename && m.GWCodesLevel == 2 && m.IsDeleted == 0).FirstOrDefault();
                                                if (DupData != null)
                                                {
                                                    MsqlConnection mc1 = new MsqlConnection();
                                                    mc1.open();
                                                    string dat = DateTime.Now.ToString();
                                                    dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                    SqlCommand cmd2 = null;
                                                    try
                                                    {
                                                        string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                        cmd2 = new SqlCommand("UPDATE tblgenericworkcodes set GenericWorkDesc = '" + codedesc + "', GWCodesLevel1ID = '" + GWCodesLevel1ID + "' ,ModifiedOn = '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',ModifiedBy = '" + Convert.ToInt32(Session["UserId"]) + "' where GenericWorkID = '" + DupData.GenericWorkID + "'", mc1.msqlConnection);
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
                                                        cmd2 = new SqlCommand("INSERT INTO tblgenericworkcodes (`GenericWorkCode`,`GenericWorkDesc`,`GWCodesLevel`,`GWCodesLevel1ID`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , " + level + " ,'" + GWCodesLevel1ID + "', 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
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
                            //Check if it already exits , if so insert.
                            #region
                            string codename = Convert.ToString(ds.Tables[0].Rows[i][2]);
                            string codedesc = Convert.ToString(ds.Tables[0].Rows[i][4]);

                            if (!string.IsNullOrEmpty(codename) && !string.IsNullOrEmpty(codedesc))
                            {
                                if (codename.Length > 40 || codedesc.Length > 60)
                                {
                                    Errors += "GenericWorkCodez (MaxLength:40) and LossDesc (MaxLength:60)  " + codename;
                                    continue;
                                }
                                else
                                {
                                    string Level1Code = Convert.ToString(ds.Tables[0].Rows[i][0]);
                                    string Level2Code = Convert.ToString(ds.Tables[0].Rows[i][1]);

                                    //Is Level1 code Valid.
                                    if (!string.IsNullOrEmpty(Level1Code) && !string.IsNullOrEmpty(Level2Code))
                                    {
                                        var Level1Data = db.tblgenericworkcodes.Where(m => m.GenericWorkCode == Level1Code && m.IsDeleted == 0).FirstOrDefault();
                                        var Level2Data = db.tblgenericworkcodes.Where(m => m.GenericWorkCode == Level2Code && m.IsDeleted == 0).FirstOrDefault();

                                        if (Level1Data != null && Level2Data != null)
                                        {
                                            if (Level1Data.GWCodesLevel != 1)
                                            {
                                                Errors += "InValid Level1 Code for " + codename + " ";
                                                continue;
                                            }
                                            if (Level2Data.GWCodesLevel != 2)
                                            {
                                                Errors += "InValid Level2 Code for " + codename + " ";
                                                continue;
                                            }

                                            //Level 1 && 2 are not relevant to each other.
                                            var Level2sLevel1 = Level2Data.GWCodesLevel1ID;
                                            if (Level2sLevel1 == Level1Data.GenericWorkID)
                                            {
                                                int GWCodesLevel1ID = Level1Data.GenericWorkID;
                                                int GWCodesLevel2ID = Level2Data.GenericWorkID;
                                                try
                                                {
                                                    string datetimeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                    var DupData = db.tblgenericworkcodes.Where(m => m.GenericWorkCode == codename && m.IsDeleted == 0 && m.GWCodesLevel == 3).FirstOrDefault();
                                                    if (DupData != null)
                                                    {
                                                        MsqlConnection mc1 = new MsqlConnection();
                                                        mc1.open();
                                                        string dat = DateTime.Now.ToString();
                                                        dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                        SqlCommand cmd2 = null;
                                                        try
                                                        {
                                                            cmd2 = new SqlCommand("UPDATE tblgenericworkcodes set GenericWorkDesc = '" + codedesc + "', GWCodesLevel1ID = '" + GWCodesLevel1ID + "',GWCodesLevel2ID = '" + GWCodesLevel2ID + "' ,ModifiedOn = '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',ModifiedBy = '" + Convert.ToInt32(Session["UserId"]) + "' where GenericWorkID = '" + DupData.GenericWorkID + "'", mc1.msqlConnection);
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
                                                            cmd2 = new SqlCommand("INSERT INTO tblgenericworkcodes (`GenericWorkCode`,`GenericWorkDesc`,`GWCodesLevel`,`GWCodesLevel1ID`,`GWCodesLevel2ID`,`IsDeleted`,`CreatedOn`,`CreatedBy`) VALUES( '" + codename + "' , '" + codedesc + "' , " + level + " ,'" + GWCodesLevel1ID + "','" + GWCodesLevel2ID + "', 0 , '" + @DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Convert.ToInt32(Session["UserId"]) + "')", mc1.msqlConnection);
                                                            cmd2.ExecuteNonQuery();
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            Errors = e.ToString();
                                                        }
                                                        mc1.close();
                                                    }

                                                }
                                                catch (Exception e)
                                                {
                                                    Errors = e.ToString();
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
            return View();
        }

        public ActionResult ExportGenericCodesDetails()
        {
            #region Excel and Stuff
            DateTime frda = DateTime.Now;
            String FileDir = @"C:\SRKS_ifacility\ReportsList\" + System.DateTime.Now.ToString("yyyy-MM-dd");
            bool exists = System.IO.Directory.Exists(FileDir);
            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "GenericWorkList" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "GenericWorkList" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //" to " + toda.ToString("yyyy-MM-dd") + 
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
            //worksheet.Cells["E" + 1].Value = "EndCode";
            worksheet.Cells["E" + 1].Value = "Code Desc";
            //worksheet.Cells["G" + 1].Value = "Loss Type";
            //worksheet.Cells["H" +1].Value = "MessageType";
            //worksheet.Cells["I" +1].Value = "Contributes To";
            worksheet.Cells["A1:I1"].Style.Font.Bold = true;

            #endregion
            var BreakDownLossesData = db.tblgenericworkcodes.Where(m => m.IsDeleted == 0).OrderBy(m => m.GWCodesLevel1ID).ThenBy(m => m.GWCodesLevel2ID).ToList();
            int i = 2;
            foreach (var row in BreakDownLossesData)
            {
                int losscodelevel = row.GWCodesLevel;
                if (losscodelevel == 1)
                {
                    worksheet.Cells["A" + i].Value = row.GenericWorkCode;
                    worksheet.Cells["D" + i].Value = losscodelevel;
                    //worksheet.Cells["E" + i].Value = row.EndCode;
                    worksheet.Cells["E" + i].Value = row.GenericWorkDesc;
                    //worksheet.Cells["G" + i].Value = row.MessageType;
                    //worksheet.Cells["H" + i].Value = row.MessageType;
                    //worksheet.Cells["I" + i].Value = row.ContributeTo;
                }
                if (losscodelevel == 2)
                {
                    int level1ID = Convert.ToInt32(row.GWCodesLevel1ID);
                    string level1Code = Convert.ToString(db.tblgenericworkcodes.Where(m => m.GenericWorkID == level1ID).Select(m => m.GenericWorkCode).FirstOrDefault());

                    worksheet.Cells["A" + i].Value = level1Code;
                    worksheet.Cells["B" + i].Value = row.GenericWorkCode;
                    //worksheet.Cells["C" + i].Value = row.LossCode;
                    worksheet.Cells["D" + i].Value = losscodelevel;
                    //worksheet.Cells["E" + i].Value = row.EndCode;
                    worksheet.Cells["E" + i].Value = row.GenericWorkDesc;
                    // worksheet.Cells["G" + i].Value = row.MessageType;
                    //worksheet.Cells["H" + i].Value = row.MessageType;
                    //worksheet.Cells["I" + i].Value = row.ContributeTo;
                }
                if (losscodelevel == 3)
                {
                    int level1ID = Convert.ToInt32(row.GWCodesLevel1ID);
                    string level1Code = Convert.ToString(db.tblgenericworkcodes.Where(m => m.GenericWorkID == level1ID).Select(m => m.GenericWorkCode).FirstOrDefault());
                    int level2ID = Convert.ToInt32(row.GWCodesLevel2ID);
                    string level2Code = Convert.ToString(db.tblgenericworkcodes.Where(m => m.GenericWorkID == level2ID).Select(m => m.GenericWorkCode).FirstOrDefault());

                    worksheet.Cells["A" + i].Value = level1Code;
                    worksheet.Cells["B" + i].Value = level2Code;
                    worksheet.Cells["C" + i].Value = row.GenericWorkCode;
                    worksheet.Cells["D" + i].Value = losscodelevel;
                    //worksheet.Cells["E" + i].Value = row.EndCode;
                    worksheet.Cells["E" + i].Value = row.GenericWorkDesc;
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
            string path1 = System.IO.Path.Combine(FileDir, "GenericWorkList" + frda.ToString("yyyy-MM-dd") + ".xlsx");
            System.IO.FileInfo file1 = new System.IO.FileInfo(path1);
            string Outgoingfile = "GenericWorkList" + frda.ToString("yyyy-MM-dd") + ".xlsx";
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