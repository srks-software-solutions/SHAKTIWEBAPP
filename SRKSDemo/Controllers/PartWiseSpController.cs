
using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace i_facility.Controllers
{
    public class PartWiseSpController : Controller
    {

        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();

        public ActionResult ImportPartWiseSP()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ImportPartWiseSP(HttpPostedFileBase file)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }

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
                    string fileLocation = @"C:\TataReport\ReportsList" + Request.Files["file"].FileName;
                    if (System.IO.File.Exists(fileLocation))
                    {
                        try
                        {
                            System.IO.File.Delete(fileLocation);
                        }
                        catch { }
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
                    //        //Create Connection to Excel work book and add oledb namespace
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
                ViewBag.Logout = Session["Username"];
                ViewBag.roleid = Session["RoleID"];

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    string result = "";
                    double SurfaceArea = 0;
                    double Perimeter = 0;
                    string PartName = "";
                    string a = ds.Tables[0].Rows[i][1].ToString();
                    if (string.IsNullOrEmpty(a) == false)
                    {
                        try
                        {
                            SurfaceArea = Math.Round(Convert.ToDouble(ds.Tables[0].Rows[i][1]),3);
                        }
                        catch
                        {
                            result = result + "surface area " + SurfaceArea + "should not be empty and should be number.\n";
                            continue;
                        }

                        try
                        {
                            Perimeter = Math.Round(Convert.ToDouble(ds.Tables[0].Rows[i][2]),3);
                        }
                        catch
                        {
                            result = result + "Perimeter " + Perimeter + "should not be empty and should be number.\n";
                            continue;
                        }
                        try
                        {
                            PartName = Convert.ToString(ds.Tables[0].Rows[i][0]);
                        }
                        catch
                        {
                            result = result + "PartName " + PartName + "should not be empty and should be string value.\n";
                            continue;
                        }

                        tblpartwisesp obj = new tblpartwisesp();
                        String Username = Session["Username"].ToString();
                        obj.CreatedBy = Convert.ToInt32(Session["UserId"]);
                        obj.CreatedOn = DateTime.Now;
                        obj.IsDeleted = 0;
                        obj.SurfaceArea = SurfaceArea;
                        obj.Perimeter = Perimeter;
                        obj.PartName = PartName;
                        if (result == "")
                        {
                            db.tblpartwisesps.Add(obj);
                            db.SaveChanges();
                        }
                    }
                    sb.Append(result);
                }
                if (sb.Length != 0)
                {
                    TempData["SurafaceArea"] = sb;
                }
                else
                {
                    TempData["SurafaceArea"] = "Please Upload a Valid Excel File for Parts";
                }
            }
            return RedirectToAction("Index", "Dashboard");    
        }

    }
}
