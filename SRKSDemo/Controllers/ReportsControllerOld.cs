using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using System.IO;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using SRKS.ServerModel;

namespace SRKS.Controllers
{
    public class ReportsController : Controller
    {
        unitworksccsEntities Serverdb = new unitworksccsEntities();

        // GET: Reports
        public ActionResult MasterPartsReport()
        {
            return View();
        }

        public ActionResult Utilization_ABGraph()
        {

            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");
            return View();
        }

        [HttpPost]
        public ActionResult Utilization_ABGraph(int PlantID, String FromDate, String ToDate, int ShopID = 0, int CellID = 0,int MachineID = 0)
        {
            ReportsCalcClass.UtilizationReport UR = new ReportsCalcClass.UtilizationReport();
            UR.CalculateUtilization(PlantID, ShopID, CellID, MachineID, Convert.ToDateTime(FromDate), Convert.ToDateTime(ToDate));

            var getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0).ToList();

            if (MachineID != 0)
            {
                getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID).ToList();
            }
            else if (CellID != 0)
            {
                getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == CellID).ToList();
            }
            else if (ShopID != 0)
            {
                getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID).ToList();
            }
            

            int dateDifference = Convert.ToDateTime(ToDate).Subtract(Convert.ToDateTime(FromDate)).Days;

            FileInfo templateFile = new FileInfo(@"C:\UnitworksCCSReports\MainTemplate\UtilizationReport_ABGraph.xlsx");

            ExcelPackage templatep = new ExcelPackage(templateFile);
            ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];
            ExcelWorksheet TemplateGraph = templatep.Workbook.Worksheets[2];

            String FileDir = @"C:\UnitworksCCSReports\ReportsList\" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd");
            bool exists = System.IO.Directory.Exists(FileDir);
            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "UtilizationReport_ABGraph" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "UtilizationReport_ABGraph" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx"));
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
            ExcelWorksheet workSheetGraphData = null;

            //Creating the WorkSheet for populating
            try
            {
                worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy"), Templatews);
                worksheetGraph = p.Workbook.Worksheets.Add("Graphs", TemplateGraph);
                workSheetGraphData = p.Workbook.Worksheets.Add("GraphData", TemplateGraph);
            }
            catch { }

            if (worksheet == null)
            {
                worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy") + "1", Templatews);
                worksheetGraph = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy") + "Graph", TemplateGraph);
                workSheetGraphData = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy") + "GraphData", TemplateGraph);
            }
            int sheetcount = p.Workbook.Worksheets.Count;
            p.Workbook.Worksheets.MoveToStart(sheetcount);
            worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            int StartRow = 3;
            int SlNo = 1;
            for (int i = 0; i <= dateDifference; i++)
            {
                DateTime QueryDate = Convert.ToDateTime(FromDate).AddDays(i);
                foreach (var Machine in getMachineList)
                {
                    var GetUtilList = Serverdb.tbl_UtilReport.Where(m => m.MachineID == Machine.MachineID && m.CorrectedDate == QueryDate.Date).ToList();
                    foreach (var MacRow in GetUtilList)
                    {
                        worksheet.Cells["A" + StartRow].Value = SlNo++;
                        worksheet.Cells["B" + StartRow].Value = QueryDate.Date.ToString("dd-MM-yyyy");
                        worksheet.Cells["C" + StartRow].Value = MacRow.tblmachinedetail.tblplant.PlantDisplayName;
                        worksheet.Cells["D" + StartRow].Value = MacRow.tblmachinedetail.tblshop.Shopdisplayname;
                        worksheet.Cells["E" + StartRow].Value = MacRow.tblmachinedetail.tblcell.CelldisplayName;
                        worksheet.Cells["F" + StartRow].Value = MacRow.tblmachinedetail.MachineDisplayName;
                        worksheet.Cells["G" + StartRow].Value = MacRow.TotalTime;
                        worksheet.Cells["H" + StartRow].Value = MacRow.OperatingTime;
                        worksheet.Cells["I" + StartRow].Value = MacRow.SetupTime;
                        worksheet.Cells["J" + StartRow].Value = (MacRow.MinorLossTime - MacRow.SetupMinorTime);
                        worksheet.Cells["K" + StartRow].Value = MacRow.LossTime;
                        worksheet.Cells["L" + StartRow].Value = MacRow.BDTime;
                        worksheet.Cells["M" + StartRow].Value = MacRow.PowerOffTime;
                        worksheet.Cells["N" + StartRow].Value = MacRow.UtilPercent + " %";
                        StartRow++;
                    }
                }
            }
           
            int rowCount = 2+ dateDifference;
            //getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0).ToList();

            int oldcolumn = 2;
            int height = 20;
            foreach (var Machine1 in getMachineList)
            {
                int currentCOlumn = oldcolumn;
                string macName = Machine1.MachineDisplayName;
                int StartRow1 = 2;
                for (int i = 0; i <= dateDifference; i++)
                {
                    DateTime QueryDate1 = Convert.ToDateTime(FromDate).AddDays(i);
                    var GetUtilList = Serverdb.tbl_UtilReport.Where(m => m.MachineID == Machine1.MachineID && m.CorrectedDate == QueryDate1.Date).ToList();
                    foreach (var MacRow in GetUtilList)
                    {
                        string ColEntry = ExcelColumnFromNumber(currentCOlumn);
                        workSheetGraphData.Cells[ColEntry + "" + StartRow1].Value = QueryDate1.Date.ToString("dd-MM-yyyy");
                        ColEntry = ExcelColumnFromNumber(currentCOlumn + 1);
                        workSheetGraphData.Cells[ColEntry + "" + StartRow1].Value = MacRow.tblmachinedetail.MachineDisplayName;
                        ColEntry = ExcelColumnFromNumber(currentCOlumn + 2);
                        workSheetGraphData.Cells[ColEntry + "" + StartRow1].Value = MacRow.OperatingTime;
                        StartRow1++;
                    }
                    
                }
                if (StartRow1 > 2)
                {
                    oldcolumn = currentCOlumn + 3;
                    var chartIDAndUnID = (ExcelBarChart)worksheetGraph.Drawings.AddChart("AB Graph-" + macName, eChartType.ColumnStacked);

                    chartIDAndUnID.SetSize((40 * rowCount), 350);

                    chartIDAndUnID.SetPosition(height, 20);
                    height = height + 400;

                    chartIDAndUnID.Title.Text = "AB Graph - " + macName;
                    chartIDAndUnID.Style = eChartStyle.Style18;
                    chartIDAndUnID.Legend.Position = eLegendPosition.Bottom;
                    //chartIDAndUnID.Legend.Remove();
                    chartIDAndUnID.YAxis.MaxValue = 24;
                    chartIDAndUnID.YAxis.MinValue = 0;
                    chartIDAndUnID.YAxis.MajorUnit = 4;
                 
                    chartIDAndUnID.Locked = false;
                    chartIDAndUnID.PlotArea.Border.Width = 0;
                    chartIDAndUnID.YAxis.MinorTickMark = eAxisTickMark.None;
                    chartIDAndUnID.DataLabel.ShowValue = true;
                    chartIDAndUnID.DisplayBlanksAs = eDisplayBlanksAs.Gap;
                    string ColEntry1 = ExcelColumnFromNumber(currentCOlumn);
                    ExcelRange dateWork = workSheetGraphData.Cells[ColEntry1 + "2:" + ColEntry1 + rowCount];
                    ColEntry1 = ExcelColumnFromNumber(currentCOlumn + 2);
                    ExcelRange hoursWork = workSheetGraphData.Cells[ColEntry1 + "2:" + ColEntry1 + rowCount];
                    workSheetGraphData.Hidden = eWorkSheetHidden.Hidden;
                    var hours = (ExcelChartSerie)(chartIDAndUnID.Series.Add(hoursWork, dateWork));
                    hours.Header = "Operating Time (Hours)";
                    //Get reference to the worksheet xml for proper namespace
                    var chartXml = chartIDAndUnID.ChartXml;
                    var nsuri = chartXml.DocumentElement.NamespaceURI;
                    var nsm = new XmlNamespaceManager(chartXml.NameTable);
                    nsm.AddNamespace("c", nsuri);

                    //XY Scatter plots have 2 value axis and no category
                    var valAxisNodes = chartXml.SelectNodes("c:chartSpace/c:chart/c:plotArea/c:valAx", nsm);
                    if (valAxisNodes != null && valAxisNodes.Count > 0)
                        foreach (XmlNode valAxisNode in valAxisNodes)
                        {
                            var major = valAxisNode.SelectSingleNode("c:majorGridlines", nsm);
                            if (major != null)
                                valAxisNode.RemoveChild(major);

                            var minor = valAxisNode.SelectSingleNode("c:minorGridlines", nsm);
                            if (minor != null)
                                valAxisNode.RemoveChild(minor);
                        }

                    //Other charts can have a category axis
                    var catAxisNodes = chartXml.SelectNodes("c:chartSpace/c:chart/c:plotArea/c:catAx", nsm);
                    if (catAxisNodes != null && catAxisNodes.Count > 0)
                        foreach (XmlNode catAxisNode in catAxisNodes)
                        {
                            var major = catAxisNode.SelectSingleNode("c:majorGridlines", nsm);
                            if (major != null)
                                catAxisNode.RemoveChild(major);

                            var minor = catAxisNode.SelectSingleNode("c:minorGridlines", nsm);
                            if (minor != null)
                                catAxisNode.RemoveChild(minor);
                        }
                }
            }
            p.Workbook.Worksheets.MoveToStart(3);
            p.Save();

            //Downloding Excel
            string path1 = System.IO.Path.Combine(FileDir, "UtilizationReport_ABGraph" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx");
            DownloadUtilReport(path1, "UtilizationReport_ABGraph", ToDate);

            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", PlantID);
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName", ShopID);
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName", CellID);
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName", MachineID);

            return View();
        }

        public void DownloadUtilReport(String FilePath, String FileName, String ToDate)
        {
            System.IO.FileInfo file1 = new System.IO.FileInfo(FilePath);
            string Outgoingfile = FileName + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx";
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
                Response.End();
            }
        }

        public ActionResult ManMachineTicket()
        {
            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");
            return View();
        }

        [HttpPost]
        public ActionResult ManMachineTicket(int PlantID, String FromDate, String ToDate, int ShopID = 0, int CellID = 0, int MachineID = 0)
        {
            ReportsCalcClass.ProdDetAndon UR = new ReportsCalcClass.ProdDetAndon();

            var getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0).ToList();

            if (MachineID != 0)
            {
                getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID).ToList();
            }
            else if (CellID != 0)
            {
                getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == CellID).ToList();
            }
            else if (ShopID != 0)
            {
                getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID).ToList();
            }

            int dateDifference = Convert.ToDateTime(ToDate).Subtract(Convert.ToDateTime(FromDate)).Days;

            FileInfo templateFile = new FileInfo(@"C:\UnitworksCCSReports\MainTemplate\ManMachineTicket.xlsx");

            ExcelPackage templatep = new ExcelPackage(templateFile);
            ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];
            //ExcelWorksheet TemplateGraph = templatep.Workbook.Worksheets[2];

            String FileDir = @"C:\UnitworksCCSReports\ReportsList\" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd");
            bool exists = System.IO.Directory.Exists(FileDir);
            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "ManMachineTicket" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "ManMachineTicket" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx"));
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
            //ExcelWorksheet worksheetGraph = null;

            //Creating the WorkSheet for populating
            try
            {
                worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy"), Templatews);
                //worksheetGraph = p.Workbook.Worksheets.Add("Graphs", TemplateGraph);
            }
            catch { }

            if (worksheet == null)
            {
                worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy") + "1", Templatews);
                //worksheetGraph = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy") + "Graph", TemplateGraph);
            }
            int sheetcount = p.Workbook.Worksheets.Count;
            p.Workbook.Worksheets.MoveToStart(sheetcount);
            worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            int StartRow = 4;
            int SlNo = 1;

            int Startcolumn = 18;
            String ColNam = ExcelColumnFromNumber(Startcolumn);
            var GetMainLossList = Serverdb.tbllossescodes.Where(m => m.LossCodesLevel == 1 && m.IsDeleted == 0 && m.MessageType != "SETUP").OrderBy(m=>m.LossCodeID).ToList();
            foreach(var LossRow in GetMainLossList)
            {
                ColNam = ExcelColumnFromNumber(Startcolumn);
                worksheet.Cells[ColNam + "3"].Value = LossRow.LossCode;
                Startcolumn++;
            }
            
            for (int i = 0; i <= dateDifference; i++)
            {
                DateTime QueryDate = Convert.ToDateTime(FromDate).AddDays(i);
                foreach (var Machine in getMachineList)
                {
                    UR.insertManMacProd(Machine.MachineID, QueryDate.Date);
                    var GetUtilList = Serverdb.tbl_ProdManMachine.Where(m => m.MachineID == Machine.MachineID && m.CorrectedDate == QueryDate.Date).ToList();
                    foreach (var MacRow in GetUtilList)
                    {
                        int MacStartcolumn = 18;
                        worksheet.Cells["A" + StartRow].Value = SlNo++;
                        worksheet.Cells["B" + StartRow].Value = MacRow.tblmachinedetail.MachineDisplayName;
                        worksheet.Cells["C" + StartRow].Value = MacRow.tblmachinedetail.MachineDisplayName;
                        worksheet.Cells["D" + StartRow].Value = MacRow.tblworkorderentry.OperatorID;
                        worksheet.Cells["E" + StartRow].Value = MacRow.tblworkorderentry.Prod_Order_No;
                        worksheet.Cells["F" + StartRow].Value = MacRow.tblworkorderentry.OperationNo;
                        worksheet.Cells["G" + StartRow].Value = QueryDate.Date.ToString("dd-MM-yyyy");
                        worksheet.Cells["H" + StartRow].Value = MacRow.tblworkorderentry.ShiftID;
                        worksheet.Cells["I" + StartRow].Value = MacRow.tblworkorderentry.WOStart.ToString("hh:mm tt");
                        worksheet.Cells["J" + StartRow].Value = Convert.ToDateTime(MacRow.tblworkorderentry.WOEnd).ToString("hh:mm tt");
                        worksheet.Cells["K" + StartRow].Value = MacRow.tblworkorderentry.Yield_Qty;
                        worksheet.Cells["L" + StartRow].Value = MacRow.tblworkorderentry.ScrapQty;
                        worksheet.Cells["M" + StartRow].Value = MacRow.tblworkorderentry.Total_Qty;
                        worksheet.Cells["N" + StartRow].Value = MacRow.TotalSetup;
                        worksheet.Cells["O" + StartRow].Value = MacRow.TotalOperatingTime;
                        worksheet.Cells["P" + StartRow].Value = 0;
                        worksheet.Cells["Q" + StartRow].Value = MacRow.TotalMinorLoss - MacRow.TotalSetupMinorLoss;
                        //var getWoLossList = Serverdb.tbl_ProdOrderLosses.Where(m => m.WOID == MacRow.WOID).ToList();

                        foreach (var LossRow in GetMainLossList)
                        {
                            var getWoLossList1 = Serverdb.tbl_ProdOrderLosses.Where(m => m.WOID == MacRow.WOID && m.LossID == LossRow.LossCodeID).FirstOrDefault();
                            String ColEntry = ExcelColumnFromNumber(MacStartcolumn);
                            if(getWoLossList1 != null)
                            worksheet.Cells[ColEntry + "" + StartRow].Value = getWoLossList1.LossDuration;
                            else
                                worksheet.Cells[ColEntry + "" + StartRow].Value = 0;
                            MacStartcolumn++;
                        }

                        //foreach (var LossRow in getWoLossList)
                        //{
                        //    int LossIndex = GetMainLossList.IndexOf(Serverdb.tbllossescodes.Find(LossRow.LossID));
                        //    String ColEntry = ExcelColumnFromNumber(MacStartcolumn + LossIndex);
                        //    worksheet.Cells[ColEntry + "" + StartRow].Value = LossRow.LossDuration;
                        //}
                        StartRow++;
                    }
                }
            }

            //worksheet.View.ShowGridLines = false;
            //worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            p.Save();

            //Downloding Excel
            string path1 = System.IO.Path.Combine(FileDir, "ManMachineTicket" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx");
            DownloadUtilReport(path1, "ManMachineTicket", ToDate);

            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", PlantID);
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName", ShopID);
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName", CellID);
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName", MachineID);
            return View();
        }

        public static string ExcelColumnFromNumber(int column)
        {
            string columnString = "";
            decimal columnNumber = column;
            while (columnNumber > 0)
            {
                decimal currentLetterNumber = (columnNumber - 1) % 26;
                char currentLetter = (char)(currentLetterNumber + 65);
                columnString = currentLetter + columnString;
                columnNumber = (columnNumber - (currentLetterNumber + 1)) / 26;
            }
            return columnString;
        }

        public ActionResult OEEReport()
        {
            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");
            return View();
        }

        [HttpPost]
        public ActionResult OEEReport(int PlantID, String FromDate, String ToDate, int ShopID = 0, int CellID = 0, int MachineID = 0)
        {
            ReportsCalcClass.ProdDetAndon UR = new ReportsCalcClass.ProdDetAndon();

            var getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0).ToList();

            if (MachineID != 0)
            {
                getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID).ToList();
            }
            else if (CellID != 0)
            {
                getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == CellID).ToList();
            }
            else if (ShopID != 0)
            {
                getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID).ToList();
            }

            int dateDifference = Convert.ToDateTime(ToDate).Subtract(Convert.ToDateTime(FromDate)).Days;

            FileInfo templateFile = new FileInfo(@"C:\UnitworksCCSReports\MainTemplate\OEE_Report.xlsx");

            ExcelPackage templatep = new ExcelPackage(templateFile);
            ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];
            ExcelWorksheet TemplateGraph = templatep.Workbook.Worksheets[2];

            String FileDir = @"C:\UnitworksCCSReports\ReportsList\" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd");
            bool exists = System.IO.Directory.Exists(FileDir);
            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "OEE_Report" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "OEE_Report" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx"));
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
                worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy"), Templatews);
                worksheetGraph = p.Workbook.Worksheets.Add("Graphs", TemplateGraph);
            }
            catch { }

            if (worksheet == null)
            {
                worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy") + "1", Templatews);
                worksheetGraph = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy") + "Graph", TemplateGraph);
            }
            int sheetcount = p.Workbook.Worksheets.Count;
            p.Workbook.Worksheets.MoveToStart(sheetcount);
            worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            int StartRow = 2;
            int SlNo = 1;

            int Startcolumn = 12;
            String ColNam = ExcelColumnFromNumber(Startcolumn);
            var GetMainLossList = Serverdb.tbllossescodes.Where(m => m.LossCodesLevel == 1 && m.IsDeleted == 0 && m.MessageType != "SETUP").OrderBy(m => m.LossCodeID).ToList();
            foreach (var LossRow in GetMainLossList)
            {
                ColNam = ExcelColumnFromNumber(Startcolumn);
                worksheet.Cells[ColNam + "1"].Value = LossRow.LossCode;
                Startcolumn++;
            }

            //Tabular sheet Data Population
            for (int i = 0; i <= dateDifference; i++)
            {
                DateTime QueryDate = Convert.ToDateTime(FromDate).AddDays(i);
                foreach (var Machine in getMachineList)
                {
                    UR.insertManMacProd(Machine.MachineID, QueryDate.Date);
                    var GetUtilList = Serverdb.tbl_ProdManMachine.Where(m => m.MachineID == Machine.MachineID && m.CorrectedDate == QueryDate.Date).ToList();
                    foreach (var MacRow in GetUtilList)
                    {
                        int MacStartcolumn = 12;
                        worksheet.Cells["A" + StartRow].Value = MacRow.tblmachinedetail.MachineName;
                        worksheet.Cells["B" + StartRow].Value = MacRow.tblmachinedetail.MachineName;
                        worksheet.Cells["C" + StartRow].Value = MacRow.tblworkorderentry.Prod_Order_No;
                        worksheet.Cells["D" + StartRow].Value = MacRow.tblworkorderentry.FGCode;
                        worksheet.Cells["E" + StartRow].Value = MacRow.tblworkorderentry.ProdOrderQty;
                        worksheet.Cells["F" + StartRow].Value = MacRow.tblworkorderentry.OperationNo;
                        worksheet.Cells["G" + StartRow].Value = QueryDate.Date.ToString("dd-MM-yyyy");
                        worksheet.Cells["H" + StartRow].Value = MacRow.TotalOperatingTime;
                        worksheet.Cells["I" + StartRow].Value = MacRow.tblworkorderentry.Yield_Qty;
                        worksheet.Cells["J" + StartRow].Value = MacRow.tblworkorderentry.ScrapQty;
                        worksheet.Cells["K" + StartRow].Value = MacRow.TotalSetup;
                        int TotalQty = MacRow.tblworkorderentry.Yield_Qty + MacRow.tblworkorderentry.ScrapQty;
                        if (TotalQty == 0)
                            TotalQty = 1;
                        worksheet.Cells["K1"].Value = "Setup Time";
                        worksheet.Cells["L1"].Value = "Rejections";
                        worksheet.Cells["L" + StartRow].Value = (MacRow.TotalOperatingTime / TotalQty) * MacRow.tblworkorderentry.ScrapQty;
                        //worksheet.Cells["I" + StartRow].Value = MacRow.tblworkorderentry.WOStart.ToString("hh:mm tt");
                        //worksheet.Cells["J" + StartRow].Value = Convert.ToDateTime(MacRow.tblworkorderentry.WOEnd).ToString("hh:mm tt");
                        //worksheet.Cells["K" + StartRow].Value = MacRow.tblworkorderentry.Yield_Qty;
                        //worksheet.Cells["L" + StartRow].Value = MacRow.tblworkorderentry.ScrapQty;
                        //worksheet.Cells["M" + StartRow].Value = MacRow.tblworkorderentry.Total_Qty;
                        //worksheet.Cells["N" + StartRow].Value = MacRow.TotalSetup;
                        //worksheet.Cells["O" + StartRow].Value = MacRow.TotalOperatingTime;
                        //worksheet.Cells["P" + StartRow].Value = 0;
                        //worksheet.Cells["Q" + StartRow].Value = MacRow.TotalMinorLoss - MacRow.TotalSetupMinorLoss;
                        //var getWoLossList = Serverdb.tbl_ProdOrderLosses.Where(m => m.WOID == MacRow.WOID).ToList();
                        int MacTotalLoss = 0;
                        foreach (var LossRow in GetMainLossList)
                        {
                            var getWoLossList1 = Serverdb.tbl_ProdOrderLosses.Where(m => m.WOID == MacRow.WOID && m.LossID == LossRow.LossCodeID).FirstOrDefault();
                            String ColEntry = ExcelColumnFromNumber(MacStartcolumn);
                            if (getWoLossList1 != null)
                            {
                                worksheet.Cells[ColEntry + "" + StartRow].Value = getWoLossList1.LossDuration;
                                MacTotalLoss += getWoLossList1.LossDuration;
                            }
                            else
                                worksheet.Cells[ColEntry + "" + StartRow].Value = 0;
                            MacStartcolumn++;
                        }
                        String ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
                        worksheet.Cells[ColEntry1 + "1"].Value = "No Power";
                        worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.TotalPowerLoss;
                        MacStartcolumn++;

                        ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
                        worksheet.Cells[ColEntry1 + "1"].Value = "Total Part";
                        worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.tblworkorderentry.Total_Qty;
                        MacStartcolumn++;

                        ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
                        worksheet.Cells[ColEntry1 + "1"].Value = "Load / Unload";
                        worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.TotalMinorLoss - MacRow.TotalSetupMinorLoss;
                        MacStartcolumn++;

                        ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
                        worksheet.Cells[ColEntry1 + "1"].Value = "Shift";
                        worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.tblworkorderentry.ShiftID;
                        MacStartcolumn++;

                        ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
                        worksheet.Cells[ColEntry1 + "1"].Value = "Operator ID";
                        worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.tblworkorderentry.OperatorID;
                        MacStartcolumn++;

                        ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
                        worksheet.Cells[ColEntry1 + "1"].Value = "Total OEE Loss";
                        worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacTotalLoss;
                        MacStartcolumn++;

                        decimal OEEPercent =(decimal) Math.Round((double)(MacRow.UtilPercent/100) * (double)(MacRow.PerformancePerCent/100) * (double)(MacRow.QualityPercent/100) * 100,2);

                        ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
                        worksheet.Cells[ColEntry1 + "1"].Value = "% of OEE";
                        worksheet.Cells[ColEntry1 + "" + StartRow].Value = OEEPercent;
                        MacStartcolumn++;

                        //ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
                        //worksheet.Cells[ColEntry1 + "1"].Value = "Month Full Name";
                        //worksheet.Cells[ColEntry1 + "" + StartRow].Value = QueryDate.ToString("MMMM");
                        //MacStartcolumn++;

                        //ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
                        //worksheet.Cells[ColEntry1 + "1"].Value = "Week In Month";
                        //worksheet.Cells[ColEntry1 + "" + StartRow].Value = QueryDate;
                        //MacStartcolumn++;

                        //ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
                        //worksheet.Cells[ColEntry1 + "1"].Value = "Month";
                        //worksheet.Cells[ColEntry1 + "" + StartRow].Value = QueryDate.ToString("MM");
                        //MacStartcolumn++;

                        //ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
                        //worksheet.Cells[ColEntry1 + "1"].Value = "Year";
                        //worksheet.Cells[ColEntry1 + "" + StartRow].Value = QueryDate.ToString("yyyy");
                        //MacStartcolumn++;

                        //ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
                        //worksheet.Cells[ColEntry1 + "1"].Value = "Week No.";
                        //worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacTotalLoss;
                        //MacStartcolumn++;

                        //foreach (var LossRow in getWoLossList)
                        //{
                        //    int LossIndex = GetMainLossList.IndexOf(Serverdb.tbllossescodes.Find(LossRow.LossID));
                        //    String ColEntry = ExcelColumnFromNumber(MacStartcolumn + LossIndex);
                        //    worksheet.Cells[ColEntry + "" + StartRow].Value = LossRow.LossDuration;
                        //}
                        StartRow++;
                    }
                }
            }

            DataTable LossTbl = new DataTable();
            LossTbl.Columns.Add("LossID", typeof(int));
            LossTbl.Columns.Add("LossDuration", typeof(int));

            //Graph Sheet Population
            //Start Date and End Date
            worksheetGraph.Cells["B6"].Value = Convert.ToDateTime(FromDate).ToString("dd-MM-yyyy");
            worksheetGraph.Cells["D6"].Value = Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy");
            int GetHolidays = getsundays(Convert.ToDateTime(ToDate), Convert.ToDateTime(FromDate));
            int WorkingDays = dateDifference - GetHolidays;
            //Working Days
            worksheetGraph.Cells["D5"].Value = WorkingDays;
            //Planned Production Time
            worksheetGraph.Cells["E11"].Value = WorkingDays * 24;
            double TotalOperatingTime = 0;
            double TotalDownTime = 0;
            double TotalAcceptedQty = 0;
            double TotalRejectedQty = 0;
            double TotalPerformanceFactor = 0;
            int StartGrpah1 = 48;
            for (int i = 0; i <= dateDifference; i++)
            {
                double DayOperatingTime = 0;
                double DayDownTime = 0;
                double DayAcceptedQty = 0;
                double DayRejectedQty = 0;
                double DayPerformanceFactor = 0;
                DateTime QueryDate = Convert.ToDateTime(FromDate).AddDays(i);
                foreach (var MachRow in getMachineList)
                {
                    if (MachineID == 0)
                    {
                        worksheetGraph.Cells["B4"].Value = MachRow.tblcell.CelldisplayName;
                    }
                    else
                    {
                        worksheetGraph.Cells["B5"].Value = MachRow.MachineDisplayName;
                    }
                    var GetUtilList = Serverdb.tbl_ProdManMachine.Where(m => m.MachineID == MachRow.MachineID && m.CorrectedDate == QueryDate.Date).ToList();
                    foreach(var ProdRow in GetUtilList)
                    {
                        //Total Values
                        TotalOperatingTime += (double)ProdRow.TotalOperatingTime;
                        TotalDownTime += (double)ProdRow.TotalLoss + (double)ProdRow.TotalMinorLoss;
                        TotalAcceptedQty += ProdRow.tblworkorderentry.Yield_Qty;
                        TotalRejectedQty += ProdRow.tblworkorderentry.ScrapQty;
                        TotalPerformanceFactor += ProdRow.PerfromaceFactor;
                        //Day Values
                        DayOperatingTime += (double)ProdRow.TotalOperatingTime;
                        DayDownTime += (double)ProdRow.TotalLoss + (double)ProdRow.TotalMinorLoss;
                        DayAcceptedQty += ProdRow.tblworkorderentry.Yield_Qty;
                        DayRejectedQty += ProdRow.tblworkorderentry.ScrapQty;
                        DayPerformanceFactor += ProdRow.PerfromaceFactor;
                    }
                    var GetLossList = Serverdb.tbl_ProdOrderLosses.Where(m => m.MachineID == MachRow.MachineID && m.CorrectedDate == QueryDate.Date).ToList();
                    foreach(var LossRow in GetLossList)
                    {
                        var getrow = (from DataRow row in LossTbl.Rows where row.Field<int>("LossID") == LossRow.LossID select row["LossID"]).FirstOrDefault();
                        if(getrow == null)
                        {
                            LossTbl.Rows.Add(LossRow.LossID, LossRow.LossDuration);
                        }
                        else
                        {
                            foreach(DataRow GetRow in LossTbl.Rows)
                            {
                                if(Convert.ToInt32(GetRow["LossID"]) == LossRow.LossID)
                                {
                                    int LossDura = Convert.ToInt32(GetRow["LossDuration"]);
                                    LossDura += LossRow.LossDuration;
                                    GetRow["LossDuration"] = LossDura;
                                }
                            }
                        }
                    }
                }
                int TotQty =(int)(DayAcceptedQty + DayRejectedQty);
                if (TotQty == 0)
                    TotQty = 1;

                double DayOpTime = DayOperatingTime;
                if (DayOpTime == 0)
                    DayOpTime = 1;

                decimal DayAvailPercent = (decimal)Math.Round(DayOperatingTime / (24) * 100, 2);
                decimal DayPerformancePercent = (decimal)Math.Round(DayPerformanceFactor / DayOpTime * 100, 2);
                decimal DayQualityPercent = (decimal)Math.Round((DayAcceptedQty / (TotQty)) * 100, 2);
                decimal DayOEEPercent = (decimal)Math.Round((double)(DayAvailPercent / 100) * (double)(DayPerformancePercent / 100) * (double)(DayQualityPercent / 100) * 100, 2);

                worksheetGraph.Cells["B" + StartGrpah1].Value = QueryDate.ToString("dd-MM-yyyy");
                worksheetGraph.Cells["C" + StartGrpah1].Value = "85.00%";
                worksheetGraph.Cells["D" + StartGrpah1].Value = DayOEEPercent;

                StartGrpah1++;
            }
            worksheetGraph.Cells["E12"].Value = TotalOperatingTime;
            worksheetGraph.Cells["E13"].Value = TotalDownTime;
            worksheetGraph.Cells["E14"].Value = TotalAcceptedQty;
            worksheetGraph.Cells["E15"].Value = TotalRejectedQty;

            decimal TotalAvailPercent = (decimal)Math.Round(TotalOperatingTime / (WorkingDays * 24) * 100, 2);
            decimal TotalPerformancePercent = (decimal)Math.Round(TotalPerformanceFactor / TotalOperatingTime * 100, 2);
            decimal TotalQualityPercent = (decimal)Math.Round((TotalAcceptedQty / (TotalAcceptedQty + TotalRejectedQty)) * 100, 2);
            decimal TotalOEEPercent = (decimal)Math.Round((double)(TotalAvailPercent / 100) * (double)(TotalPerformancePercent / 100) * (double)(TotalQualityPercent / 100) * 100, 2);

            worksheetGraph.Cells["E21"].Value = TotalAvailPercent;
            worksheetGraph.Cells["E22"].Value = TotalPerformancePercent;
            worksheetGraph.Cells["E23"].Value = TotalQualityPercent;
            worksheetGraph.Cells["E24"].Value = TotalOEEPercent;
            //worksheetGraph.Cells["F5"].Value = OEE;

            //worksheet.View.ShowGridLines = false;
            //worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            p.Workbook.Worksheets.MoveToStart(2);
            p.Save();

            //Downloding Excel
            string path1 = System.IO.Path.Combine(FileDir, "OEE_Report" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx");
            DownloadUtilReport(path1, "OEE_Report", ToDate);

            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", PlantID);
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID), "ShopID", "ShopName", ShopID);
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID), "CellID", "CellName", CellID);
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID && m.CellID == CellID), "MachineID", "MachineDisplayName", MachineID);
            return View();
        }

        public int getsundays(DateTime fdate,DateTime sdate)
        {
            TimeSpan ts = fdate - sdate;
            var sundays = ((ts.TotalDays / 7) + (sdate.DayOfWeek == DayOfWeek.Sunday || fdate.DayOfWeek == DayOfWeek.Sunday || fdate.DayOfWeek > sdate.DayOfWeek ? 1 : 0));

            sundays = Math.Round(sundays - .5, MidpointRounding.AwayFromZero);

            return (int)sundays;
        }
    }
}