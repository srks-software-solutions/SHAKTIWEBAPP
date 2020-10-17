
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
    public class MailIDsController : Controller
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

            var mailData = db.tblmailids.Where(m => m.IsDeleted == 0).ToList();
            return View(mailData);
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

            return View();
        }
        [HttpPost]
        public ActionResult Create(tblmailid tmi)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            #region//ActiveLog Code
            int UserID = Convert.ToInt32(Session["UserId"]);
            //string CompleteModificationdetail = "New Creation";
            //Action = "Create";
            // ActiveLogStorage Obj = new ActiveLogStorage();
            // Obj.SaveActiveLog(Action, Controller, Username, UserID, CompleteModificationdetail);
            //End
            #endregion

            //MailID Details validation
            string name = tmi.EmployeeName.ToString();
            string emailid = tmi.EmailID.ToString();
            string contactNo = tmi.EmployeeContactNum.ToString();
            var doesTheseExists = db.tblmailids.Where(m => m.IsDeleted == 0 && (m.EmployeeName == name || m.EmailID == emailid || m.EmployeeContactNum == contactNo)).ToList();
            if (doesTheseExists.Count() == 0)
            {
                tmi.IsDeleted = 0;
                tmi.CreatedBy = UserID;
                tmi.CreatedOn = DateTime.Now;
                db.tblmailids.Add(tmi);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                Session["Error"] = null;
                var doesNameExists = db.tblmailids.Where(m => m.IsDeleted == 0 && m.EmployeeName == name).ToList();
                if (doesNameExists.Count() > 0)
                {
                    Session["Error"] += "Duplicate Name. ";
                }
                var doesMailIdExists = db.tblmailids.Where(m => m.EmailID == emailid).ToList();
                if (doesMailIdExists.Count() > 0)
                {
                    Session["Error"] += " Duplicate Email ID. ";
                }
                var doesCnoExists = db.tblmailids.Where(m => m.IsDeleted == 0 && m.EmployeeContactNum == contactNo).ToList();
                if (doesCnoExists.Count() > 0)
                {
                    Session["Error"] += " Duplicate Contact Number. ";
                }
                return View(tmi);
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
            String Username = Session["Username"].ToString();
           tblmailid tmi = db.tblmailids.Find(id);
            if (tmi == null)
            {
                return HttpNotFound();
            }
            return View(tmi);
        }
        [HttpPost]
        public ActionResult Edit(tblmailid tmi)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();
            int UserID = Convert.ToInt32(Session["UserID"]);

            //MailID details validation
            string name = tmi.EmployeeName.ToString();
            string emailid = tmi.EmailID.ToString();
            string contactNo = tmi.EmployeeContactNum.ToString();
            int MailIDDetailsID = tmi.MailIDsID;
            var doesTheseExists = db.tblmailids.Where(m => m.IsDeleted == 0 && (m.EmployeeName == name || m.EmailID == emailid || m.EmployeeContactNum == contactNo) && m.MailIDsID != MailIDDetailsID).ToList();
            if (doesTheseExists.Count() == 0)
            {
                #region Active Log Code
                //tblmachinecategory OldData = db.tblmachinecategories.Find(tblmc.ID);
                //IEnumerable<string> FullData = ActiveLog.EnumeratePropertyDifferences<tblmachinecategory>(OldData, tblmc);
                //ICollection<tblmachinecategory> c = FullData as ICollection<tblmachinecategory>;
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

                tmi.ModifiedBy = UserID;
                tmi.ModifiedOn = DateTime.Now;
                db.Entry(tmi).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                Session["Error"] = null;
                var doesNameExists = db.tblmailids.Where(m => m.IsDeleted == 0 && m.EmployeeName == name && m.MailIDsID != MailIDDetailsID).ToList();
                if (doesNameExists.Count > 0)
                {
                    Session["Error"] += "Duplicate Name. ";
                }
                var doesMailIdExists = db.tblmailids.Where(m => m.EmailID == emailid && m.MailIDsID != MailIDDetailsID).ToList();
                if (doesMailIdExists.Count > 0)
                {
                    Session["Error"] += " Duplicate Email ID. ";
                }
                var doesCnoExists = db.tblmailids.Where(m => m.IsDeleted == 0 && m.EmployeeContactNum == contactNo && m.MailIDsID != MailIDDetailsID).ToList();
                if (doesCnoExists.Count > 0)
                {
                    Session["Error"] += " Duplicate Contact Number. ";
                }
                return View(tmi);
            }

        }

        public ActionResult Delete(int id)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            //start Logging
            int UserID = Convert.ToInt32(Session["UserId"]);
            String Username = Session["Username"].ToString();
            //string CompleteModificationdetail = "Deleted MachineDetails";
            //Action = "Delete";
            //ActiveLogStorage Obj = new ActiveLogStorage();
            //Obj.SaveActiveLog(Action, Controller, Username, UserID, CompleteModificationdetail);
            //End

            tblmailid tmi = db.tblmailids.Find(id);
            tmi.IsDeleted = 1;
            tmi.ModifiedBy = UserID;
            tmi.ModifiedOn = System.DateTime.Now;
            db.Entry(tmi).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult ImportMailDetails()
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
        public ActionResult ImportMailDetails(HttpPostedFileBase file)
        {

            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            String Username = Session["Username"].ToString();

            ////start logging
            //String Username = Session["Username"].ToString();
            //int UserID = Convert.ToInt32(Session["UserId"]);
            //string CompleteModificationdetail = "Import PriorityAlarm";
            //Action = "ImportPriorityAlarm";
            //ActiveLogStorage Obj = new ActiveLogStorage();
            //Obj.SaveActiveLog(Action, Controller, Username, UserID, CompleteModificationdetail);
            ////End

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
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    Errors = null;
                    string Name = ds.Tables[0].Rows[i][0].ToString();
                    string EmailID = ds.Tables[0].Rows[i][1].ToString();
                    string ContactNo = ds.Tables[0].Rows[i][2].ToString();

                    if ((!string.IsNullOrEmpty(Name.Trim())) && (!string.IsNullOrEmpty(EmailID.Trim())) && (!string.IsNullOrEmpty(ContactNo.Trim())))
                    {
                        var doesNameExists = db.tblmailids.Where(m => m.IsDeleted == 0 && m.EmployeeName == Name).ToList();
                        if (doesNameExists.Count > 0)
                        {
                            Errors += " Duplicate Name : " + Name + "\n";
                        }
                        else if (Name.Length > 30)
                        {
                            Errors += "  Name : " + Name + "\n";
                        }
                        var doesMailIdExists = db.tblmailids.Where(m => m.EmailID == EmailID).ToList();
                        if (doesMailIdExists.Count > 0)
                        {
                            Errors += " Duplicate Email ID :" + EmailID + "\n";
                        }
                        else if ((!EmailID.EndsWith("@tasl.aero")) || EmailID.Length > 50)
                        {
                            Errors += " Invalid EmailID : " + EmailID + "\n";
                        }
                        var doesCnoExists = db.tblmailids.Where(m => m.IsDeleted == 0 && m.EmployeeContactNum == ContactNo).ToList();
                        if (doesCnoExists.Count > 0)
                        {
                            Errors += " Duplicate Contact Number " + ContactNo + "\n";
                        }
                        else if ((ContactNo.Contains("[a-zA-Z]+")) || ContactNo.Length > 10)
                        {
                            Errors += " No Alphabets are Allowed in ContactNo. : " + ContactNo + "\n";
                        }


                        if (Errors == null)
                        {
                            MsqlConnection mc1 = new MsqlConnection();
                            mc1.open();
                            string dat = DateTime.Now.ToString();
                            dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            SqlCommand cmd2 = null;
                            try
                            {
                                cmd2 = new SqlCommand("INSERT INTO " + MsqlConnection.DbName + ".tblmailids(Name,EmailID,ContactNo,IsDeleted,CreatedOn,CreatedBy) "
                                     + " VALUES('" + Name + "' , '" + EmailID + "' ,'" + ContactNo + "',0,'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' , '" + Session["UserId"] + "')", mc1.msqlConnection);
                                cmd2.ExecuteNonQuery();
                            }
                            catch (Exception e)
                            {
                                Errors = e.ToString();
                            }
                            finally
                            {
                                mc1.close();
                            }
                        }
                        else
                        {
                            Session["Error"] += Errors;
                        }
                    }
                }

            }


            return RedirectToAction("Index");
        }
    }
}