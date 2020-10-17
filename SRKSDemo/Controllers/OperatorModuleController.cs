using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using OfficeOpenXml;
using System.Data.Entity.Validation;
using System.Data;
using System.Data.OleDb;
using System.Xml;
using System.Data.Entity;
using SRKSDemo.Server_Model;
using i_facility.Models;

namespace i_facility.Controllers
{
    public class OperatorModuleController : Controller
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
        // GET: OperatorModule
        public ActionResult Index()
        {

            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();

            OperatorModule OPList = new OperatorModule();
            tbloperatordetail OPDet = new tbloperatordetail();
            List<tbloperatordetail> OPDetList = new List<tbloperatordetail>();
            OPDetList = db.tbloperatordetails.Where(m => m.isDeleted == 0).ToList();
            OPList.OPdetailsList = OPDetList;
            return View(OPList);
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
            ViewBag.Operator = new SelectList(db.tblroles.Where(m => m.IsDeleted == 0), "Role_ID", "RoleDesc");
            return View();
        }

        //[HttpPost]
        //public ActionResult Create(OperatorModule OPTDet)
        //{
        //    if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
        //    {
        //        return RedirectToAction("Login", "Login", null);
        //    }
        //    ViewBag.Logout = Session["Username"].ToString().ToUpper();
        //    ViewBag.roleid = Session["RoleID"];
        //    int UserID = Convert.ToInt32(Session["UserId"]);
        //    string Dept = OPTDet.OPDetails.Dept;
        //    string OperatorName = OPTDet.OPDetails.OperatorName;
        //    int OperatorID = OPTDet.OPDetails.OperatorID;
        //    bool check = ValidationCheckForInsertion(OperatorID);
        //    if (check == true)
        //    {
        //        OPTDet.OPDetails.CreatedOn = DateTime.Now;
        //        OPTDet.OPDetails.CorrectedDate = DateTime.Now.Date;
        //        OPTDet.OPDetails.CreatedBy = UserID;
        //        OPTDet.OPDetails.isDeleted = 0;
        //        db.tbloperatordetails.Add(OPTDet.OPDetails);
        //        db.SaveChanges();
        //    }
        //    return RedirectToAction("Index");
        //}

        public string CreateOperator(string dept, string operatorId, string operatorName, int OperatorDesc)
        {
            string res = "";
            int UserID = Convert.ToInt32(Session["UserId"]);
            bool check = ValidationCheckForInsertion(operatorId);
            if (check == true)
            {
                tbloperatordetail OPTDet = new tbloperatordetail();
                OPTDet.CreatedOn = DateTime.Now;
                OPTDet.CorrectedDate = DateTime.Now.Date;
                OPTDet.CreatedBy = UserID;
                OPTDet.isDeleted = 0;
                OPTDet.Dept = dept;
                OPTDet.OperatorID = Convert.ToString(operatorId);
                OPTDet.OperatorName = operatorName;
                OPTDet.OperatorDesc = OperatorDesc;
                db.tbloperatordetails.Add(OPTDet);
                db.SaveChanges();
                res = "Success";
            }
            else
            {
                res = "failure";
            }
            return res;
        }

        public bool ValidationCheckForInsertion(string OperatorID)
        {
            bool result = true;
            var OPTDet = db.tbloperatordetails.Where(m => m.isDeleted == 0 && m.OperatorID == OperatorID).FirstOrDefault();
            if (OPTDet != null)
            {
                result = false;
            }
            return result;
        }
        public bool ValidationCheckForUpdation(int OperatorID)
        {
            bool result = true;
            var OPTDet = db.tbloperatordetails.Where(m => m.isDeleted == 0 && m.OPID == OperatorID).FirstOrDefault();
            if (OPTDet != null)
            {
                result = false;
            }
            return result;
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            Session["Opid"] = id;
            tbloperatordetail tblmc = db.tbloperatordetails.Find(id);
            if (tblmc == null)
            {
                return HttpNotFound();
            }
            ViewBag.Operator = new SelectList(db.tblroles.Where(m => m.IsDeleted == 0), "Role_ID", "RoleDesc",tblmc.OperatorDesc);
            return View(tblmc);
        }

        //[HttpPost]
        //public ActionResult Edit(OperatorModule OPDet, int hdnpid = 0)
        //{
        //    if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
        //    {
        //        return RedirectToAction("Login", "Login", null);
        //    }
        //    ViewBag.Logout = Session["Username"].ToString().ToUpper();
        //    ViewBag.roleid = Session["RoleID"];
        //    int UserID = Convert.ToInt32(Session["UserId"]);
        //    string Dept = OPDet.OPDetails.Dept;
        //    string OperatorName = OPDet.OPDetails.OperatorName;
        //    int OperatorID = OPDet.OPDetails.OperatorID;
        //    int OPID = hdnpid;
        //    bool check = ValidationCheckForInsertion(OperatorID);
        //    if (check == true)
        //    {
        //        //tbloperatordetail obj = db.tbloperatordetails.Find(OPID);
        //        tbloperatordetail obj = new tbloperatordetail();
        //        obj.Dept = OPDet.OPDetails.Dept;
        //        obj.OperatorName = OPDet.OPDetails.OperatorName;
        //        obj.OperatorID = OPDet.OPDetails.OperatorID;
        //        obj.CreatedOn = DateTime.Now;
        //        obj.CreatedBy = UserID;
        //        obj.CorrectedDate = DateTime.Now.Date;
        //        obj.isDeleted = 0;
        //        db.tbloperatordetails.Add(obj);
        //        db.SaveChanges();
        //    }
        //    else
        //    {
        //        tbloperatordetail obj = db.tbloperatordetails.Find(OPID);
        //        obj.Dept = OPDet.OPDetails.Dept;
        //        obj.OperatorName = OPDet.OPDetails.OperatorName;
        //        obj.ModifiedOn = DateTime.Now;
        //        obj.ModifiedBy = UserID;
        //        db.SaveChanges();
        //    }
        //    return RedirectToAction("Index");

        //}


        public string EditOperator(int id,string dept, int operatorId, string operatorName, int OperatorDesc)
        {
            string res = "";
            int UserID = Convert.ToInt32(Session["UserId"]);
            bool check = ValidationCheckForUpdation(id);
            if (check == true)
            {
                res = "failure";
            }
            else
            {
                tbloperatordetail obj = db.tbloperatordetails.Find(id);
                obj.Dept = dept;
                obj.OperatorName = operatorName;
                obj.OperatorDesc = OperatorDesc;
                obj.OperatorID = Convert.ToString(operatorId);
                obj.ModifiedOn = DateTime.Now;
                obj.ModifiedBy = UserID;
                db.SaveChanges();
                res = "Success";
            }
            return res;
        }

        public JsonResult Operatordata(int Id)
        {
            ViewBag.id = Id;
            i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();
            var Data = db.tbloperatordetails.Where(m => m.isDeleted == 0 && m.OPID == Id).Select(m => new { OPID = m.OPID, Dept = m.Dept, OperatorName = m.OperatorName, OperatorID = m.OperatorID });
            return Json(Data, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Delete(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"].ToString().ToUpper();
            ViewBag.roleid = Session["RoleID"];
            int UserID = Convert.ToInt32(Session["UserId"]);
            String Username = Session["Username"].ToString();
            tbloperatordetail OpTDet = db.tbloperatordetails.Find(id);
            OpTDet.isDeleted = 1;
            OpTDet.ModifiedBy = UserID;
            OpTDet.ModifiedOn = DateTime.Now;
            db.Entry(OpTDet).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //[HttpGet]
        //public ActionResult ExportOperator()
        //{
        //    return View();
        //}

       // [HttpPost]
        public ActionResult ExportOperatorData()
        {
            #region Excel and Stuff
            DateTime frda = DateTime.Now;
            FileInfo templateFile = new FileInfo(@"C:\TataReport\NewTemplates\OperatorDetailsExport.xlsx");
            ExcelPackage templatep = new ExcelPackage(templateFile);
            ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];

            String FileDir = @"C:\TataReport\ReportsList\" + System.DateTime.Now.ToString("yyyy-MM-dd");
            bool exists = System.IO.Directory.Exists(FileDir);
            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "OperatorDetailsExport" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "OperatorDetailsExport" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //" to " + toda.ToString("yyyy-MM-dd") + 
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

            //Creating the WorkSheet for populating
            try
            {
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), Templatews);
            }
            catch { }

            if (worksheet == null)
            {
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy") + "1", Templatews);
            }

            int sheetcount = p.Workbook.Worksheets.Count;
            p.Workbook.Worksheets.MoveToStart(sheetcount);

            worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            #endregion

            var OperatorData = db.tbloperatordetails.Where(m => m.isDeleted == 0).ToList();

            int i = 2;
            for (i= 2;i < OperatorData.Count;i++)
            {

                worksheet.Cells["A" + i].Value = OperatorData[i].Dept;
                worksheet.Cells["B" + i].Value = OperatorData[i].OperatorName;
                worksheet.Cells["C" + i].Value = OperatorData[i].OperatorID;
                i++;
            }

            if (OperatorData.Count != 0)
            {
                #region Save and Download

                p.Save();
                //Downloding Excel
                string path1 = System.IO.Path.Combine(FileDir, "OperatorDetailsExport" + frda.ToString("yyyy-MM-dd") + ".xlsx");
                System.IO.FileInfo file1 = new System.IO.FileInfo(path1);
                string Outgoingfile = "OperatorDetailsExport" + frda.ToString("yyyy-MM-dd") + ".xlsx";
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
            }
            else
            {
                // return Content("<script language='javascript' type='text/javascript'>alert('Data Doesnt Exists');</script>");
            }
            return RedirectToAction("Index");

        }

        [HttpGet]
        public ActionResult ImportOperator()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ImportOperatorData(HttpPostedFileBase file, string UploadType)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            string UserId = Session["UserId"].ToString();
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
                    excelConnection.Close();
                    excelConnection1.Close();
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
                if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
                {
                    return RedirectToAction("Login", "Login", null);
                }
                ViewBag.Logout = Session["Username"].ToString().ToUpper();
                ViewBag.roleid = Session["RoleID"];


                string text = "";

                string ErrorMsg = null;

                if (UploadType == "New") // Delete Duplicate and Insert New. // if not Duplicate insert that
                {
                    #region
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {

                        tbloperatordetail tblmp = new tbloperatordetail();
                        String Username = Session["Username"].ToString();
                        tblmp.CreatedBy = Convert.ToInt32(Session["UserId"]);
                        tblmp.CreatedOn = DateTime.Now;
                        tblmp.isDeleted = 0;

                        string Dept = null, OperatorName = null;
                        string OperatorID = null;
                        Dept = Convert.ToString(ds.Tables[0].Rows[i][0]);
                        OperatorName = Convert.ToString(ds.Tables[0].Rows[i][1]);
                        OperatorID = Convert.ToString(ds.Tables[0].Rows[i][2]);

                        bool check = ValidationCheckForInsertion(OperatorID);
                        if (check == true)
                        {
                            using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
                            {

                                if (string.IsNullOrEmpty(Dept) || string.IsNullOrEmpty(OperatorName) || OperatorID == null)
                                {
                                    text = text + htmlerrorMaker(Dept, OperatorName, OperatorID, "Dept, OperatorName or OperatorID cannot be empty/Check the format");
                                    continue;
                                }
                                else
                                {
                                    try
                                    {
                                        tblmp.Dept = Dept.Trim();
                                        tblmp.OperatorName = OperatorName.Trim();
                                        tblmp.OperatorID = OperatorID;
                                        tblmp.CreatedBy = Convert.ToInt32(UserId);
                                        tblmp.CreatedOn = DateTime.Now;
                                    }
                                    catch
                                    {

                                    }
                                }
                            }

                            db.tbloperatordetails.Add(tblmp);
                            try
                            {
                                db.SaveChanges();
                            }
                            catch (DbEntityValidationException e)
                            {
                                foreach (var eve in e.EntityValidationErrors)
                                {
                                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                                    foreach (var ve in eve.ValidationErrors)
                                    {
                                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                            ve.PropertyName, ve.ErrorMessage);
                                    }
                                }
                                throw;
                            }
                        }
                    }
                    #endregion
                }
                else if (UploadType == "Update") // Delete Duplicate and Insert New. // if not Duplicate insert that
                {
                    #region
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {

                        tbloperatordetail tblmp = new tbloperatordetail();
                        String Username = Session["Username"].ToString();
                        tblmp.CreatedBy = Convert.ToInt32(Session["UserId"]);
                        tblmp.CreatedOn = DateTime.Now;
                        tblmp.isDeleted = 0;

                        string Dept = null, OperatorName = null;
                        string OperatorID = null;
                        Dept = Convert.ToString(ds.Tables[0].Rows[i][0]);
                        OperatorName = Convert.ToString(ds.Tables[0].Rows[i][1]);
                        OperatorID = Convert.ToString(ds.Tables[0].Rows[i][2]);

                        bool check = ValidationCheckForInsertion(OperatorID);
                        if (check == true)
                        {
                            using (i_facility_shaktiEntities1 db1 = new i_facility_shaktiEntities1())
                            {

                                if (string.IsNullOrEmpty(Dept) || string.IsNullOrEmpty(OperatorName) || OperatorID == null)
                                {
                                    text = text + htmlerrorMaker(Dept, OperatorName, OperatorID, "Dept, OperatorName or OperatorID cannot be empty/Check the format");
                                    continue;
                                }
                                else
                                {
                                    try
                                    {
                                        tblmp.Dept = Dept.Trim();
                                        tblmp.OperatorName = OperatorName.Trim();
                                        tblmp.OperatorID = OperatorID;
                                        tblmp.CreatedBy = Convert.ToInt32(UserId);
                                        tblmp.CreatedOn = DateTime.Now;
                                    }
                                    catch
                                    {

                                    }
                                }
                            }

                            db.tbloperatordetails.Add(tblmp);
                            try
                            {
                                db.SaveChanges();
                            }
                            catch (DbEntityValidationException e)
                            {
                                foreach (var eve in e.EntityValidationErrors)
                                {
                                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                                    foreach (var ve in eve.ValidationErrors)
                                    {
                                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                            ve.PropertyName, ve.ErrorMessage);
                                    }
                                }
                                throw;
                            }
                        }
                        else
                        {
                            using (i_facility_shaktiEntities1 db1 = new i_facility_shaktiEntities1())
                            {

                                if (string.IsNullOrEmpty(Dept) || string.IsNullOrEmpty(OperatorName) || OperatorID == null)
                                {
                                    text = text + htmlerrorMaker(Dept, OperatorName, OperatorID, "Dept, OperatorName or OperatorID cannot be empty/Check the format");
                                    continue;
                                }
                                else
                                {
                                    var OPTDet = db.tbloperatordetails.Where(m => m.isDeleted == 0 && m.OperatorID == OperatorID).FirstOrDefault();
                                    OPTDet.isDeleted = 1;
                                    db.Entry(OPTDet).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    try
                                    {
                                        tblmp.Dept = Dept.Trim();
                                        tblmp.OperatorName = OperatorName.Trim();
                                        tblmp.OperatorID = OperatorID;
                                        tblmp.CreatedBy = Convert.ToInt32(UserId);
                                        tblmp.CreatedOn = DateTime.Now;
                                    }
                                    catch
                                    {

                                    }
                                }
                            }

                            db.tbloperatordetails.Add(tblmp);
                            try
                            {
                                db.SaveChanges();
                            }
                            catch (DbEntityValidationException e)
                            {
                                foreach (var eve in e.EntityValidationErrors)
                                {
                                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                                    foreach (var ve in eve.ValidationErrors)
                                    {
                                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                            ve.PropertyName, ve.ErrorMessage);
                                    }
                                }
                                throw;
                            }
                        }
                    }
                    #endregion
                }
                else if (UploadType == "OverWrite") // Delete Duplicate and Insert New. // if not Duplicate insert that
                {
                    #region
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {

                        tbloperatordetail tblmp = new tbloperatordetail();
                        String Username = Session["Username"].ToString();
                        tblmp.CreatedBy = Convert.ToInt32(Session["UserId"]);
                        tblmp.CreatedOn = DateTime.Now;
                        tblmp.isDeleted = 0;

                        string Dept = null, OperatorName = null;
                        string OperatorID = null;
                        Dept = Convert.ToString(ds.Tables[0].Rows[i][0]);
                        OperatorName = Convert.ToString(ds.Tables[0].Rows[i][1]);
                        OperatorID = Convert.ToString(ds.Tables[0].Rows[i][2]);

                        bool check = ValidationCheckForInsertion(OperatorID);
                        if (check == true)
                        {
                            using (i_facility_shaktiEntities1 db1 = new i_facility_shaktiEntities1())
                            {

                                if (string.IsNullOrEmpty(Dept) || string.IsNullOrEmpty(OperatorName) || OperatorID == null)
                                {
                                    text = text + htmlerrorMaker(Dept, OperatorName, OperatorID, "Dept, OperatorName or OperatorID cannot be empty/Check the format");
                                    continue;
                                }
                                else
                                {
                                    try
                                    {
                                        tblmp.Dept = Dept.Trim();
                                        tblmp.OperatorName = OperatorName.Trim();
                                        tblmp.OperatorID = OperatorID;
                                        tblmp.CreatedBy = Convert.ToInt32(UserId);
                                        tblmp.CreatedOn = DateTime.Now;

                                    }
                                    catch
                                    {

                                    }
                                }
                            }

                            db.tbloperatordetails.Add(tblmp);
                            try
                            {
                                db.SaveChanges();
                            }
                            catch (DbEntityValidationException e)
                            {
                                foreach (var eve in e.EntityValidationErrors)
                                {
                                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                                    foreach (var ve in eve.ValidationErrors)
                                    {
                                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                            ve.PropertyName, ve.ErrorMessage);
                                    }
                                }
                                throw;
                            }
                        }
                        else
                        {
                            var OPTDet = db.tbloperatordetails.Where(m => m.isDeleted == 0 && m.OperatorID == OperatorID).FirstOrDefault();
                            using (i_facility_shaktiEntities1 db1 = new i_facility_shaktiEntities1())
                            {

                                if (string.IsNullOrEmpty(Dept) || string.IsNullOrEmpty(OperatorName) || OperatorID == null)
                                {
                                    text = text + htmlerrorMaker(Dept, OperatorName, OperatorID, "Dept, OperatorName or OperatorID cannot be empty/Check the format");
                                    continue;
                                }
                                else
                                {
                                    try
                                    {
                                        OPTDet.Dept = Dept.Trim();
                                        OPTDet.OperatorName = OperatorName.Trim();
                                        OPTDet.ModifiedBy = Convert.ToInt32(UserId);
                                        OPTDet.ModifiedOn = DateTime.Now;
                                        db.Entry(OPTDet).State = System.Data.Entity.EntityState.Modified;
                                    }
                                    catch
                                    {

                                    }
                                }
                            }

                            //db.tbloperatordetails.Add(OPTDet);
                            try
                            {
                                db.SaveChanges();
                            }
                            catch (DbEntityValidationException e)
                            {
                                foreach (var eve in e.EntityValidationErrors)
                                {
                                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                                    foreach (var ve in eve.ValidationErrors)
                                    {
                                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                            ve.PropertyName, ve.ErrorMessage);
                                    }
                                }
                                throw;
                            }
                        }
                    }
                    #endregion
                }

                //TempData["txtShow"] = text;
                //// ViewBag.text = text;
                //Session["PartNo"] = ErrorMsg;
            }
            //return RedirectToAction("Index", "MasterParts");
            return RedirectToAction("Index", "OperatorModule");
        }

        public string htmlerrorMaker(string Dept, string OperatorName, string OperatorID, string message)
        {
            string val = "";

            val = "<tr><td>" + Dept + "</td><td>" + OperatorName + "</td><td>" + OperatorID + "</td></tr>";

            return val;
        }       

        public JsonResult GetOperatorData()
        {
            var operatordet = db.tbloperatordetails.Where(m => m.isDeleted == 0).Select(m=>m.OperatorID).ToList();            
            return Json(operatordet, JsonRequestBehavior.AllowGet);
        }
    }
}