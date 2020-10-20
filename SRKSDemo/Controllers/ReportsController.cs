using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using SRKSDemo;
using SRKSDemo.OperatorEntryModelClass;
using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Xml;
using System.Text.RegularExpressions;
using System.Reflection;
using SRKSDemo.Models;
using i_facilitylibrary;
using i_facilitylibrary.DAO;
using System.Globalization;
using i_facilitylibrary.DAL;
using System.Data.Entity;

namespace SRKSDemo.Controllers
{
    public class ReportsController : Controller
    {
        i_facility_shaktiEntities1 Serverdb = new i_facility_shaktiEntities1();
        private IConnectionFactory _conn;
        private Dao obj1 = new Dao();
        private Dao1 obj2 = new Dao1();
        ReportsDao obj = new ReportsDao();
        //Dao obj1 = new Dao();
        //Dao1 obj2 = new Dao1();
        string dbName = ConfigurationManager.AppSettings["dbName"];
        

        #region V changes Utilization report for with new structer
        public ActionResult Utilization()
        {

            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");
            return View();
        }

        [HttpPost]
        public ActionResult Utilization(int PlantID, String FromDate, String ToDate, int ShopID = 0, int CellID = 0, int MachineID = 0)
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

            FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\UtilixationReportMaini.xlsx");

            ExcelPackage templatep = new ExcelPackage(templateFile);
            ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];
            ExcelWorksheet Templatews1 = templatep.Workbook.Worksheets[2];

            String FileDir = @"C:\SRKS_ifacility\ReportsList\" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd");
            bool exists = System.IO.Directory.Exists(FileDir);
            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "UtilizationReport" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "UtilixationReportMaini" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx"));
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
            ExcelWorksheet worksheetSum = null;

            //Creating the WorkSheet for populating
            try
            {
                worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy"), Templatews);
                worksheetSum = p.Workbook.Worksheets.Add("Summerized", Templatews1);
            }
            catch { }

            if (worksheet == null)
            {
                worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy") + "1", Templatews);
                worksheetSum = p.Workbook.Worksheets.Add("Summerized" + "1", Templatews1);
            }
            int sheetcount = p.Workbook.Worksheets.Count;
            p.Workbook.Worksheets.MoveToStart(sheetcount);
            worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            worksheetSum.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheetSum.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            int StartRow = 4;
            int SlNo = 1;

            // day wise data display
            List<UtilSummerized> UtilSummerizedList = new List<UtilSummerized>();
            for (int i = 0; i <= dateDifference; i++)
            {
                DateTime QueryDate = Convert.ToDateTime(FromDate).AddDays(i);
                foreach (var Machine in getMachineList)
                {
                    UtilSummerized UtilSummerizedObj = new UtilSummerized();
                    string CorrDate = QueryDate.ToString("yyyy-MM-dd");
                    int MachID = Machine.MachineID;

                    var MachineDet = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MachID).FirstOrDefault();
                    string MachineName = MachineDet.MachineDisplayName;
                    int Cellid = Convert.ToInt32(MachineDet.CellID);
                    var CellDet = Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.CellID == Cellid).FirstOrDefault();
                    string Cellname = CellDet.CelldisplayName;
                    int Shopid = CellDet.ShopID;
                    var ShopDet = Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.ShopID == Shopid).FirstOrDefault();
                    string ShopName = ShopDet.Shopdisplayname;
                    int Plantid = ShopDet.PlantID;
                    string PlantName = Serverdb.tblplants.Where(m => m.IsDeleted == 0 && m.PlantID == Plantid).Select(m => m.PlantDisplayName).FirstOrDefault();

                    UtilSummerizedObj.PlantName = PlantName;
                    UtilSummerizedObj.ShopName = ShopName;
                    UtilSummerizedObj.CellName = Cellname;
                    UtilSummerizedObj.MachineName = MachineName;
                    UtilSummerizedObj.DateTime = FromDate + " TO " + ToDate;


                    worksheet.Cells["A" + StartRow].Value = SlNo++;
                    worksheet.Cells["B" + StartRow].Value = PlantName;
                    worksheet.Cells["C" + StartRow].Value = ShopName;
                    worksheet.Cells["D" + StartRow].Value = Cellname;
                    worksheet.Cells["E" + StartRow].Value = MachineName;
                    worksheet.Cells["F" + StartRow].Value = CorrDate;

                    int Col = 7;
                    var ShiftDet = Serverdb.tblshift_mstr.Where(m => m.IsDeleted == 0).ToList();
                    List<ShiftValue> ShiftList = new List<ShiftValue>();
                    foreach (var ShiftRow in ShiftDet)
                    {
                        string ColumnNumber = ExcelColumnFromNumber(Col);
                        int TotalTime = Convert.ToInt32(ShiftRow.Duration);
                        int ShiftID = ShiftRow.ShiftID;
                        string ShiftName = ShiftRow.ShiftName;
                        double SumCuttingtime = 0, SumOperatingtime = 0, SumPowerOntime = 0;
                        double CuttinTimeT = 0, ModeOPTimeT = 0, ModePOTimeT = 0, FinalCuttinTimeT = 0, FinalModeOPTimeT = 0, FinalModePOTimeT = 0;
                        DateTime StartTime = Convert.ToDateTime(CorrDate + " " + ShiftRow.StartTime);
                        DateTime EndTime = Convert.ToDateTime(CorrDate + " " + ShiftRow.EndTime);
                        var ModePOTime = Serverdb.tbllivemodes.Where(m => m.IsDeleted == 0 && m.IsShiftEnd == ShiftID && m.MachineID == MachID && m.CorrectedDate == QueryDate.Date).ToList();
                        var ModeOPTime = Serverdb.tbllivemodes.Where(m => m.IsDeleted == 0 && m.IsShiftEnd == ShiftID && m.MachineID == MachID && m.ColorCode == "GREEN" && m.CorrectedDate == QueryDate.Date).ToList();
                        var CuttingTime = Serverdb.tblpartscountandcuttings.Where(m => m.Isdeleted == 0 && m.ShiftName == ShiftName && m.MachineID == MachID && m.CorrectedDate == QueryDate.Date).ToList();
                        if (CuttingTime.Count != 0)
                        {
                            //CuttinTimeT = Serverdb.tblpartscountandcuttings.Where(m => m.Isdeleted == 0 && m.StartTime >= StartTime && m.EndTime <= EndTime && m.CorrectedDate == QueryDate.Date).Sum(m=>m.CuttingTime);
                            CuttinTimeT = Serverdb.tblpartscountandcuttings.Where(m => m.Isdeleted == 0 && m.ShiftName == ShiftName && m.MachineID == MachID && m.CorrectedDate == QueryDate.Date).Sum(m => m.CuttingTime);
                            FinalCuttinTimeT = (CuttinTimeT / TotalTime) * 100;
                            SumCuttingtime = SumCuttingtime + FinalCuttinTimeT;
                        }
                        if (ModeOPTime.Count != 0)
                        {
                            ModeOPTimeT = Convert.ToInt32(Serverdb.tbllivemodes.Where(m => m.IsDeleted == 0 && m.IsShiftEnd == ShiftID && m.MachineID == MachID && m.ColorCode == "GREEN" && m.CorrectedDate == QueryDate.Date).Sum(m => m.DurationInSec));
                            FinalModeOPTimeT = ((ModeOPTimeT / 60) / TotalTime) * 100;
                            SumOperatingtime = SumOperatingtime + FinalModeOPTimeT;
                        }
                        if (ModePOTime.Count != 0)
                        {
                            ModePOTimeT = Convert.ToInt32(Serverdb.tbllivemodes.Where(m => m.IsDeleted == 0 && m.IsShiftEnd == ShiftID && m.MachineID == MachID && m.CorrectedDate == QueryDate.Date).Sum(m => m.DurationInSec));
                            FinalModePOTimeT = ((ModePOTimeT / 60) / TotalTime) * 100;
                            SumPowerOntime = SumPowerOntime + FinalModePOTimeT;
                        }

                        var PrecentColourDet = Serverdb.tblPrecentColours.Where(m => m.IsDeleted == 0).ToList();
                        foreach (var ColourRow in PrecentColourDet)
                        {
                            double MinVal = Convert.ToDouble(ColourRow.Min);
                            double MaxVal = Convert.ToDouble(ColourRow.Max);
                            string Colour = ColourRow.Colour;

                            if (FinalModePOTimeT >= MinVal && FinalModePOTimeT < MaxVal)
                            {
                                worksheet.Cells[ColumnNumber + StartRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[ColumnNumber + StartRow].Style.Fill.BackgroundColor.SetColor(ReturnColour(Colour));
                                worksheet.Cells[ColumnNumber + StartRow].Value = Math.Round(FinalModePOTimeT, 2);
                                Col++;
                                ColumnNumber = ExcelColumnFromNumber(Col);

                            }
                            if (FinalModeOPTimeT >= MinVal && FinalModeOPTimeT < MaxVal)
                            {
                                worksheet.Cells[ColumnNumber + StartRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[ColumnNumber + StartRow].Style.Fill.BackgroundColor.SetColor(ReturnColour(Colour));
                                worksheet.Cells[ColumnNumber + StartRow].Value = Math.Round(FinalModeOPTimeT, 2);
                                Col++;
                                ColumnNumber = ExcelColumnFromNumber(Col);
                            }
                            if (FinalCuttinTimeT >= MinVal && FinalCuttinTimeT < MaxVal)
                            {
                                worksheet.Cells[ColumnNumber + StartRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[ColumnNumber + StartRow].Style.Fill.BackgroundColor.SetColor(ReturnColour(Colour));
                                worksheet.Cells[ColumnNumber + StartRow].Value = Math.Round(FinalCuttinTimeT, 2);
                            }

                        }



                        ShiftValue Obj = new ShiftValue();
                        Obj.CTTime = SumCuttingtime;
                        Obj.OPTime = SumOperatingtime;
                        Obj.POTime = SumPowerOntime;
                        Obj.MachineID = MachID;
                        ShiftList.Add(Obj);

                        Col++;
                    }



                    UtilSummerizedObj.ShiftDoubleVal = ShiftList;
                    UtilSummerizedList.Add(UtilSummerizedObj);
                    StartRow++;
                }
            }

            SlNo = 1;
            StartRow = 4;
            int Cols = 7;
            int dayDiff = dateDifference + 1;

            //summerized data display
            //var MachDet = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0).ToList();
            foreach (var MachID in getMachineList)
            {
                int Machineid = MachID.MachineID;
                if (UtilSummerizedList != null)
                {
                    foreach (var row in UtilSummerizedList)
                    {
                        worksheetSum.Cells["A" + StartRow].Value = SlNo++;
                        worksheetSum.Cells["B" + StartRow].Value = row.PlantName;
                        worksheetSum.Cells["C" + StartRow].Value = row.ShopName;
                        worksheetSum.Cells["D" + StartRow].Value = row.CellName;
                        worksheetSum.Cells["E" + StartRow].Value = row.MachineName;
                        worksheetSum.Cells["F" + StartRow].Value = row.DateTime;

                        var ListShiftVal = row.ShiftDoubleVal;
                        foreach (var ShiftRow in ListShiftVal)
                        {
                            int MID = ShiftRow.MachineID;
                            if (MID == Machineid)
                            {
                                string ColumnNumber = ExcelColumnFromNumber(Cols);

                                var PrecentColourDet = Serverdb.tblPrecentColours.Where(m => m.IsDeleted == 0).ToList();
                                foreach (var ColourRow in PrecentColourDet)
                                {
                                    double MinVal = Convert.ToDouble(ColourRow.Min);
                                    double MaxVal = Convert.ToDouble(ColourRow.Max);
                                    string Colour = ColourRow.Colour;

                                    if (ShiftRow.POTime >= MinVal && ShiftRow.POTime < MaxVal)
                                    {
                                        try
                                        {
                                            double POTIME = 0;
                                            var POTIMEFinal = worksheetSum.Cells[ColumnNumber + StartRow].Value;
                                            if (POTIMEFinal.ToString() == "")
                                            {
                                                POTIME = POTIME + ShiftRow.POTime;
                                            }
                                            else
                                            {
                                                POTIME = Convert.ToDouble(POTIMEFinal);
                                                POTIME = POTIME + ShiftRow.POTime;
                                            }
                                            worksheetSum.Cells[ColumnNumber + StartRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                            worksheetSum.Cells[ColumnNumber + StartRow].Style.Fill.BackgroundColor.SetColor(ReturnColour(Colour));
                                            worksheetSum.Cells[ColumnNumber + StartRow].Value = Math.Round(POTIME, 2);
                                            //worksheetSum.Cells[ColumnNumber + StartRow].Value = Math.Round((ShiftRow.POTime) / dayDiff, 2);
                                            Cols++;
                                            ColumnNumber = ExcelColumnFromNumber(Cols);
                                        }
                                        catch (Exception ex)
                                        {
                                            worksheetSum.Cells[ColumnNumber + StartRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                            worksheetSum.Cells[ColumnNumber + StartRow].Style.Fill.BackgroundColor.SetColor(ReturnColour(Colour));
                                            worksheetSum.Cells[ColumnNumber + StartRow].Value = Math.Round(ShiftRow.POTime, 2);
                                            Cols++;
                                            ColumnNumber = ExcelColumnFromNumber(Cols);
                                        }


                                    }
                                    if (ShiftRow.OPTime >= MinVal && ShiftRow.OPTime < MaxVal)
                                    {
                                        try
                                        {
                                            double OPTIME = 0;
                                            var OPTIMEFinal = worksheetSum.Cells[ColumnNumber + StartRow].Value;
                                            if (OPTIMEFinal.ToString() == "")
                                            {
                                                OPTIME = OPTIME + ShiftRow.POTime;
                                            }
                                            else
                                            {
                                                OPTIME = Convert.ToDouble(OPTIMEFinal);
                                                OPTIME = OPTIME + ShiftRow.POTime;
                                            }
                                            worksheetSum.Cells[ColumnNumber + StartRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                            worksheetSum.Cells[ColumnNumber + StartRow].Style.Fill.BackgroundColor.SetColor(ReturnColour(Colour));
                                            worksheetSum.Cells[ColumnNumber + StartRow].Value = Math.Round(OPTIME, 2);
                                            //worksheetSum.Cells[ColumnNumber + StartRow].Value = Math.Round((ShiftRow.OPTime) / dayDiff, 2);
                                            Cols++;
                                            ColumnNumber = ExcelColumnFromNumber(Cols);
                                        }
                                        catch (Exception ex)
                                        {
                                            worksheetSum.Cells[ColumnNumber + StartRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                            worksheetSum.Cells[ColumnNumber + StartRow].Style.Fill.BackgroundColor.SetColor(ReturnColour(Colour));
                                            worksheetSum.Cells[ColumnNumber + StartRow].Value = Math.Round(ShiftRow.OPTime, 2);
                                            Cols++;
                                            ColumnNumber = ExcelColumnFromNumber(Cols);
                                        }
                                    }
                                    if (ShiftRow.CTTime >= MinVal && ShiftRow.CTTime < MaxVal)
                                    {
                                        try
                                        {
                                            double CTTIME = 0;
                                            var CTTIMEFinal = worksheetSum.Cells[ColumnNumber + StartRow].Value;
                                            if (CTTIMEFinal.ToString() == "")
                                            {
                                                CTTIME = CTTIME + ShiftRow.POTime;
                                            }
                                            else
                                            {
                                                CTTIME = Convert.ToDouble(CTTIMEFinal);
                                                CTTIME = CTTIME + ShiftRow.POTime;
                                            }
                                            worksheetSum.Cells[ColumnNumber + StartRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                            worksheetSum.Cells[ColumnNumber + StartRow].Style.Fill.BackgroundColor.SetColor(ReturnColour(Colour));
                                            worksheetSum.Cells[ColumnNumber + StartRow].Value = Math.Round(CTTIME, 2);
                                            //worksheetSum.Cells[ColumnNumber + StartRow].Value = Math.Round((ShiftRow.CTTime) / dayDiff, 2);
                                            Cols++;
                                        }
                                        catch (Exception ex)
                                        {
                                            worksheetSum.Cells[ColumnNumber + StartRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                            worksheetSum.Cells[ColumnNumber + StartRow].Style.Fill.BackgroundColor.SetColor(ReturnColour(Colour));
                                            worksheetSum.Cells[ColumnNumber + StartRow].Value = Math.Round(ShiftRow.CTTime, 2);
                                            Cols++;
                                        }
                                    }

                                }
                                //worksheetSum.Cells[ColumnNumber + StartRow].Value = Math.Round(ShiftRow.POTime,2);
                                //Cols++;
                                //ColumnNumber = ExcelColumnFromNumber(Cols);
                                //worksheetSum.Cells[ColumnNumber + StartRow].Value = Math.Round(ShiftRow.OPTime,2);
                                //Cols++;
                                //ColumnNumber = ExcelColumnFromNumber(Cols);
                                //worksheetSum.Cells[ColumnNumber + StartRow].Value = Math.Round(ShiftRow.CTTime,2);
                                //Cols++;
                            }
                        }
                        Cols = 7;
                    }
                    Cols = 7;
                    StartRow++;
                }
            }

            // after storing the data in excel divide by difference day
            int SR = 4;
            int Coln = 7;
            var MaDet = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0).ToList();
            foreach (var r in MaDet)
            {
                int ShiftCount = Serverdb.tblshift_mstr.Where(m => m.IsDeleted == 0).Count();
                ShiftCount = ShiftCount * 3;
                string ColumnNameSt = ExcelColumnFromNumber(Coln);
                for (int i = 0; i < ShiftCount; i++)
                {
                    ColumnNameSt = ExcelColumnFromNumber(Coln);
                    var CTTIMEFinal = worksheetSum.Cells[ColumnNameSt + SR].Value;
                    if (CTTIMEFinal != null)
                    {
                        double Values = Convert.ToDouble(CTTIMEFinal);
                        Values = Values / dayDiff;
                        worksheetSum.Cells[ColumnNameSt + SR].Value = Math.Round(Values, 2);
                        Coln++;
                    }
                }
                Coln = 7;
                SR++;
            }



            int Srow = StartRow - 1;
            // last row disaply in summerized
            worksheetSum.SelectedRange["A" + StartRow + ":F" + StartRow + ""].Merge = true;
            worksheetSum.Cells["A" + StartRow].Value = "Total";
            worksheetSum.Cells["G" + StartRow].Formula = "SUM(G4:G" + Srow + ")";
            worksheetSum.Cells["H" + StartRow].Formula = "SUM(H4:H" + Srow + ")";
            worksheetSum.Cells["I" + StartRow].Formula = "SUM(I4:I" + Srow + ")";
            worksheetSum.Cells["J" + StartRow].Formula = "SUM(J4:J" + Srow + ")";
            worksheetSum.Cells["K" + StartRow].Formula = "SUM(K4:K" + Srow + ")";
            worksheetSum.Cells["L" + StartRow].Formula = "SUM(L4:L" + Srow + ")";
            worksheetSum.Cells["M" + StartRow].Formula = "SUM(M4:M" + Srow + ")";
            worksheetSum.Cells["N" + StartRow].Formula = "SUM(N4:N" + Srow + ")";
            worksheetSum.Cells["O" + StartRow].Formula = "SUM(O4:O" + Srow + ")";

            //  precentage and colour display
            int srt = 2;
            int cno = 17;
            var PrecentColourDet1 = Serverdb.tblPrecentColours.Where(m => m.IsDeleted == 0).ToList();
            string ColumnNumberS = "";
            ColumnNumberS = ExcelColumnFromNumber(cno);
            worksheet.Cells[ColumnNumberS + srt].Value = "Values are in Precentage";
            ColumnNumberS = ExcelColumnFromNumber(cno);
            worksheetSum.Cells[ColumnNumberS + srt].Value = "Values are in Precentage";
            cno++;
            srt++;
            foreach (var ColourRow in PrecentColourDet1)
            {
                ColumnNumberS = ExcelColumnFromNumber(cno);
                double MinVal = Convert.ToDouble(ColourRow.Min);
                double MaxVal = Convert.ToDouble(ColourRow.Max);
                string Colour = ColourRow.Colour;
                worksheet.Cells[ColumnNumberS + srt].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[ColumnNumberS + srt].Style.Fill.BackgroundColor.SetColor(ReturnColour(Colour));
                worksheet.Cells[ColumnNumberS + srt].Value = ">=" + MinVal + "%" + "<" + MaxVal + "%" + Colour;
                worksheetSum.Cells[ColumnNumberS + srt].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheetSum.Cells[ColumnNumberS + srt].Style.Fill.BackgroundColor.SetColor(ReturnColour(Colour));
                worksheetSum.Cells[ColumnNumberS + srt].Value = ">=" + MinVal + "%" + "<" + MaxVal + "%" + Colour;
                cno++;
            }




            //p.Workbook.Worksheets.MoveToStart(3);
            p.Save();

            //Downloding Excel
            string path1 = System.IO.Path.Combine(FileDir, "UtilizationReport" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx");
            DownloadUtilReport(path1, "UtilizationReport", ToDate);

            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", PlantID);
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName", ShopID);
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName", CellID);
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName", MachineID);

            return View();
        }


        public System.Drawing.Color ReturnColour(string Colour)
        {
            switch (Colour)
            {
                case "Red": return Color.Red;
                case "Yellow": return Color.Yellow;
                case "Green": return Color.Green;

            }
            return Color.White;
        }


        public string GetShift()
        {
            string ShiftValue = "";
            DateTime DateNow = DateTime.Now;
            var ShiftDetails = Serverdb.tblshift_mstr.Where(m => m.IsDeleted == 0).ToList();
            foreach (var row in ShiftDetails)
            {
                int ShiftStartHour = row.StartTime.Value.Hours;
                int ShiftEndHour = row.EndTime.Value.Hours;
                int CurrentHour = DateNow.Hour;
                if (CurrentHour >= ShiftStartHour && CurrentHour <= ShiftEndHour)
                {
                    ShiftValue = row.ShiftName;
                    break;
                }
            }

            return ShiftValue;
        }


        #endregion

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
        public ActionResult Utilization_ABGraph(int PlantID, String FromDate, String ToDate, int ShopID = 0, int CellID = 0, int MachineID = 0)
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

            FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\UtilizationReport_ABGraph.xlsx");

            ExcelPackage templatep = new ExcelPackage(templateFile);
            ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];
            ExcelWorksheet TemplateGraph = templatep.Workbook.Worksheets[2];

            String FileDir = @"C:\SRKS_ifacility\ReportsList\" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd");
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

            int rowCount = 2 + dateDifference;
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

                    chartIDAndUnID.Title.Text = "Graph - " + macName;
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
            ViewData["WorkCenterID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");
            return View();
        }

        public ActionResult JobReport()
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj2 = new Dao1(_conn);
            obj = new ReportsDao(_conn);
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            //var result = db.tblmachinedetails.Select(m=>m.ShopNo).Distinct();
            ViewData["PlantID"] = new SelectList(obj2.GetPlantList(), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(obj2.GetShopList1(), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(obj2.GetCellList(), "CellID", "CellName");
            ViewData["WorkCenterID"] = new SelectList(obj2.GetmachineList1(), "MachineID", "MachineInvNo");
            //ViewData["PlantID"] = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            //ViewData["ShopID"] = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            //ViewData["CellID"] = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            //ViewData["WorkCenterID"] = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineInvNo");


            return View();
        }

        [HttpPost]
        public ActionResult JobReport(string PlantID, DateTime StartDate, DateTime EndDate, string ProdFAI, string ShopID = null, string CellID = null, string WorkCenterID = null, string OperatorName = null)
        {
           _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj2 = new Dao1(_conn);
            obj = new ReportsDao(_conn);
            int PlantIDInt = Convert.ToInt32(PlantID);
            ViewData["PlantID"] = new SelectList(obj2.GetPlantList(), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(obj2.GetShop1List(PlantIDInt), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(obj2.GetCellList(), "CellID", "CellName");
            ViewData["WorkCenterID"] = new SelectList(obj2.GetmachineList1(), "MachineID", "MachineDisplayName");
            //ViewData["PlantID"] = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            //ViewData["ShopID"] = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == PlantIDInt), "ShopID", "ShopName");
            //ViewData["CellID"] = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            //ViewData["WorkCenterID"] = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineInvNo");

            JOBReportExcel(Convert.ToString(StartDate), Convert.ToString(EndDate), ProdFAI, PlantID, ShopID, CellID, WorkCenterID, OperatorName);
            return View();
        }

        #region Old 
        //[HttpPost]
        //public ActionResult ManMachineTicket(int PlantID, String FromDate, String ToDate, int ShopID = 0, int CellID = 0, int MachineID = 0)
        //{
        //    ReportsCalcClass.ProdDetAndon UR = new ReportsCalcClass.ProdDetAndon();

        //    var getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0).ToList();

        //    if (MachineID != 0)
        //    {
        //        getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID).ToList();
        //    }
        //    else if (CellID != 0)
        //    {
        //        getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == CellID).ToList();
        //    }
        //    else if (ShopID != 0)
        //    {
        //        getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID).ToList();
        //    }

        //    int dateDifference = Convert.ToDateTime(ToDate).Subtract(Convert.ToDateTime(FromDate)).Days;

        //    FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\ManMachineTicket.xlsx");

        //    ExcelPackage templatep = new ExcelPackage(templateFile);
        //    ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];
        //    //ExcelWorksheet TemplateGraph = templatep.Workbook.Worksheets[2];

        //    String FileDir = @"C:\SRKS_ifacility\ReportsList\" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd");
        //    bool exists = System.IO.Directory.Exists(FileDir);
        //    if (!exists)
        //        System.IO.Directory.CreateDirectory(FileDir);

        //    FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "ManMachineTicket" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
        //    if (newFile.Exists)
        //    {
        //        try
        //        {
        //            newFile.Delete();  // ensures we create a new workbook
        //            newFile = new FileInfo(System.IO.Path.Combine(FileDir, "ManMachineTicket" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx"));
        //        }
        //        catch
        //        {
        //            TempData["Excelopen"] = "Excel with same date is already open, please close it and try to generate!!!!";
        //            //return View();
        //        }
        //    }
        //    //Using the File for generation and populating it
        //    ExcelPackage p = null;
        //    p = new ExcelPackage(newFile);
        //    ExcelWorksheet worksheet = null;
        //    //ExcelWorksheet worksheetGraph = null;

        //    //Creating the WorkSheet for populating
        //    try
        //    {
        //        worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy"), Templatews);
        //        //worksheetGraph = p.Workbook.Worksheets.Add("Graphs", TemplateGraph);
        //    }
        //    catch { }

        //    if (worksheet == null)
        //    {
        //        worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy") + "1", Templatews);
        //        //worksheetGraph = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy") + "Graph", TemplateGraph);
        //    }
        //    int sheetcount = p.Workbook.Worksheets.Count;
        //    p.Workbook.Worksheets.MoveToStart(sheetcount);
        //    worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        //    worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
        //    int StartRow = 4;
        //    int SlNo = 1;

        //    int Startcolumn = 18;
        //    String ColNam = ExcelColumnFromNumber(Startcolumn);
        //    var GetMainLossList = Serverdb.tbllossescodes.Where(m => m.LossCodesLevel == 1 && m.IsDeleted == 0 && m.MessageType != "SETUP").OrderBy(m => m.LossCodeID).ToList();
        //    foreach (var LossRow in GetMainLossList)
        //    {
        //        ColNam = ExcelColumnFromNumber(Startcolumn);
        //        worksheet.Cells[ColNam + "3"].Value = LossRow.LossCodeDesc;
        //        Startcolumn++;
        //    }

        //    for (int i = 0; i <= dateDifference; i++)
        //    {
        //        DateTime QueryDate = Convert.ToDateTime(FromDate).AddDays(i);
        //        foreach (var Machine in getMachineList)
        //        {
        //            UR.insertManMacProd(Machine.MachineID, QueryDate.Date);
        //            var GetUtilList = Serverdb.tbl_ProdManMachine.Where(m => m.MachineID == Machine.MachineID && m.CorrectedDate == QueryDate.Date).ToList();
        //            foreach (var MacRow in GetUtilList)
        //            {
        //                int MacStartcolumn = 18;
        //                worksheet.Cells["A" + StartRow].Value = SlNo++;
        //                worksheet.Cells["B" + StartRow].Value = MacRow.tblmachinedetail.MachineDisplayName;
        //                worksheet.Cells["C" + StartRow].Value = MacRow.tblmachinedetail.MachineDisplayName;
        //                if (MacRow.tblworkorderentry != null)
        //                {
        //                    worksheet.Cells["D" + StartRow].Value = MacRow.tblworkorderentry.OperatorID;
        //                    worksheet.Cells["E" + StartRow].Value = MacRow.tblworkorderentry.Prod_Order_No;
        //                    worksheet.Cells["F" + StartRow].Value = MacRow.tblworkorderentry.OperationNo;
        //                    worksheet.Cells["H" + StartRow].Value = MacRow.tblworkorderentry.ShiftID;
        //                    worksheet.Cells["I" + StartRow].Value = MacRow.tblworkorderentry.WOStart.ToString("hh:mm tt");
        //                    worksheet.Cells["J" + StartRow].Value = Convert.ToDateTime(MacRow.tblworkorderentry.WOEnd).ToString("hh:mm tt");
        //                    worksheet.Cells["K" + StartRow].Value = MacRow.tblworkorderentry.Yield_Qty;
        //                    worksheet.Cells["L" + StartRow].Value = MacRow.tblworkorderentry.ScrapQty;
        //                    worksheet.Cells["M" + StartRow].Value = MacRow.tblworkorderentry.Total_Qty;
        //                }
        //                worksheet.Cells["G" + StartRow].Value = QueryDate.Date.ToString("dd-MM-yyyy");

        //                worksheet.Cells["N" + StartRow].Value = MacRow.TotalSetup;
        //                worksheet.Cells["O" + StartRow].Value = MacRow.TotalOperatingTime;
        //                worksheet.Cells["P" + StartRow].Value = 0;
        //                worksheet.Cells["Q" + StartRow].Value = MacRow.TotalMinorLoss - MacRow.TotalSetupMinorLoss;
        //                //var getWoLossList = Serverdb.tbl_ProdOrderLosses.Where(m => m.WOID == MacRow.WOID).ToList();

        //                foreach (var LossRow in GetMainLossList)
        //                {
        //                    var getWoLossList1 = Serverdb.tbl_ProdOrderLosses.Where(m => m.WOID == MacRow.WOID && m.LossID == LossRow.LossCodeID).FirstOrDefault();
        //                    String ColEntry = ExcelColumnFromNumber(MacStartcolumn);
        //                    if (getWoLossList1 != null)
        //                        worksheet.Cells[ColEntry + "" + StartRow].Value = getWoLossList1.LossDuration;
        //                    else
        //                        worksheet.Cells[ColEntry + "" + StartRow].Value = 0;
        //                    MacStartcolumn++;
        //                }

        //                //foreach (var LossRow in getWoLossList)
        //                //{
        //                //    int LossIndex = GetMainLossList.IndexOf(Serverdb.tbllossescodes.Find(LossRow.LossID));
        //                //    String ColEntry = ExcelColumnFromNumber(MacStartcolumn + LossIndex);
        //                //    worksheet.Cells[ColEntry + "" + StartRow].Value = LossRow.LossDuration;
        //                //}
        //                StartRow++;
        //            }
        //        }
        //    }

        //    //worksheet.View.ShowGridLines = false;
        //    //worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        //    p.Save();

        //    //Downloding Excel
        //    string path1 = System.IO.Path.Combine(FileDir, "ManMachineTicket" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx");
        //    DownloadUtilReport(path1, "ManMachineTicket", ToDate);

        //    ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", PlantID);
        //    ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName", ShopID);
        //    ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName", CellID);
        //    ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName", MachineID);
        //    return View();
        //} 
        #endregion

        #region TATA Job Report
        public void JOBReportExcel(string StartDate, string EndDate, string ProdFAI, string PlantID, string ShopID = null, string CellID = null, string WorkCenterID = null, string Operator = null, string TabularType = "Day")
        {
            //IntoFile("JobReportExcel");
            //IntoFile("JobReportExcelStartDate:" + StartDate);
            //IntoFile("JobReportExcelEndDate:" + EndDate);
            ReportsDao obj = new ReportsDao();
            Dao obj1 = new Dao();
            Dao1 obj2 = new Dao1();
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj2 = new Dao1(_conn);
            obj = new ReportsDao(_conn);
            #region Excel and Stuff

            DateTime frda = DateTime.Now;
            if (string.IsNullOrEmpty(StartDate) == true)
            {
                StartDate = DateTime.Now.Date.ToString();
            }
            if (string.IsNullOrEmpty(EndDate) == true)
            {
                EndDate = StartDate;
            }

            DateTime frmDate = Convert.ToDateTime(StartDate);
            DateTime toDate = Convert.ToDateTime(EndDate);

            //IntoFile("JobReportExcelfrmDate:" + frmDate);
            //IntoFile("JobReportExceltoDate:" + toDate);

            double TotalDay = toDate.Subtract(frmDate).TotalDays;

            FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\JobReportNew.xlsx");
            ExcelPackage templatep = new ExcelPackage(templateFile);
            ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];
            ExcelWorksheet TemplateGraph = templatep.Workbook.Worksheets[2];

            String FileDir = @"C:\SRKS_ifacility\ReportsList\" + System.DateTime.Now.ToString("yyyy-MM-dd");
            bool exists = System.IO.Directory.Exists(FileDir);
            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "JobReportNew" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "JobReportNew" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //" to " + toda.ToString("yyyy-MM-dd") + 
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
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), Templatews);
                worksheetGraph = p.Workbook.Worksheets.Add("Summarized", TemplateGraph);
            }
            catch { }

            if (worksheet == null)
            {
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy") + "-1", Templatews);
                worksheetGraph = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), TemplateGraph);
            }
            int sheetcount = p.Workbook.Worksheets.Count;
            p.Workbook.Worksheets.MoveToStart(sheetcount);
            worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            #endregion

            #region MacCount & LowestLevel
            string lowestLevel = null;
            int MacCount = 0;
            int plantId = 0, shopId = 0, cellId = 0, wcId = 0;
            if (string.IsNullOrEmpty(WorkCenterID))
            {
                if (string.IsNullOrEmpty(CellID))
                {
                    if (string.IsNullOrEmpty(ShopID))
                    {
                        if (string.IsNullOrEmpty(PlantID))
                        {
                            //donothing
                        }
                        else
                        {
                            lowestLevel = "Plant";
                            plantId = Convert.ToInt32(PlantID);
                            var plantName = obj2.GettbplantDet(plantId);
                            MacCount = obj2.GettbplantDet(plantId).ToList().Count();
                        }
                    }
                    else
                    {
                        lowestLevel = "Shop";
                        shopId = Convert.ToInt32(ShopID);
                        MacCount = obj.GettbMachineDetails1(shopId).ToList().Count();
                    }
                }
                else
                {
                    lowestLevel = "Cell";
                    cellId = Convert.ToInt32(CellID);
                    MacCount = obj.GettbMachineDetails2(cellId).ToList().Count();
                }
            }
            else
            {
                lowestLevel = "WorkCentre";
                wcId = Convert.ToInt32(WorkCenterID);
                MacCount = 1;
            }

            #endregion

            #region Get Machines List
            DataTable machin = new DataTable();
            DateTime endDateTime1 = Convert.ToDateTime(toDate.AddDays(1).ToString("yyyy-MM-dd") + " " + new TimeSpan(6, 0, 0));
            //IntoFile("JobReportExcelendDateTime" + endDateTime1);
            string endDateTime = toDate.AddDays(1).ToString("yyyy-MM-dd") + " " + new TimeSpan(6, 0, 0);
            //IntoFile("JobReportExcelendDateTime" + endDateTime);
            string startDateTime = frmDate.ToString("yyyy-MM-dd");
            //IntoFile("JobReportExcelendDateTime" + startDateTime);
            MsqlConnection mc = new MsqlConnection();
            mc.open();
            String query1 = null;
            if (lowestLevel == "Plant")
            {
                //query1 = " SELECT  distinct MachineID FROM ["+ConnectionFactory.DB+"].["+ConnectionFactory.Schema+"]tblmachinedetails WHERE PlantID = " + PlantID + " and ManualWCID IS NULL  AND((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( case when (IsDeleted = 1) then ( InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + startDateTime + "' ) end) ) ;";
                //query1 = " SELECT  distinct MachineID FROM ["+ConnectionFactory.DB+"].["+ConnectionFactory.Schema+"]tblmachinedetails WHERE PlantID = " + PlantID + " and ManualWCID IS NULL  AND((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( (IsDeleted = 1) and ( InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + startDateTime + "' )) ) ;";
                query1 = " SELECT  distinct MachineID FROM " + ConnectionFactory.DbName + ".tblmachinedetails WHERE PlantID = " + PlantID + " and ManualWCID IS NULL  AND((InsertedOn <= '" + endDateTime + "' and IsDeleted = 0) or   ( (IsDeleted = 1) and ( InsertedOn <= '" + endDateTime + "' and DeletedDate >= '" + startDateTime + "' )) ) ;";
                // IntoFile("JobReportExcelquery" + query1);
            }
            else if (lowestLevel == "Shop")
            {
                //query1 = " SELECT * FROM ["+ConnectionFactory.DB+"].["+ConnectionFactory.Schema+"]tblmachinedetails WHERE ShopID = " + ShopID + " and ManualWCID IS NULL and  ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( case when (IsDeleted = 1) then ( InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + startDateTime + "' ) end) ) ;";
                //query1 = " SELECT * FROM ["+ConnectionFactory.DB+"].["+ConnectionFactory.Schema+"]tblmachinedetails WHERE ShopID = " + ShopID + " and ManualWCID IS NULL and  ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ((IsDeleted = 1) and ( InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + startDateTime + "' )) ) ;";
                query1 = " SELECT * FROM " + ConnectionFactory.DbName + ".tblmachinedetails WHERE ShopID = " + ShopID + " and ManualWCID IS NULL and  ((InsertedOn <= '" + endDateTime + "' and IsDeleted = 0) or   ((IsDeleted = 1) and ( InsertedOn <= '" + endDateTime + "' and DeletedDate >= '" + startDateTime + "' )) ) ;";
                //IntoFile("JobReportExcelquery" + query1);
            }
            else if (lowestLevel == "Cell")
            {
                //query1 = " SELECT * FROM ["+ConnectionFactory.DB+"].["+ConnectionFactory.Schema+"]tblmachinedetails WHERE CellID = " + CellID + " and ManualWCID IS NULL and   ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( case when (IsDeleted = 1) then ( InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + startDateTime + "' ) end) ) ;";
                //query1 = " SELECT * FROM ["+ConnectionFactory.DB+"].["+ConnectionFactory.Schema+"]tblmachinedetails WHERE CellID = " + CellID + " and ManualWCID IS NULL and   ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ((IsDeleted = 1) and ( InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + startDateTime + "' )) ) ;";
                query1 = " SELECT * FROM " + ConnectionFactory.DbName + ".tblmachinedetails WHERE CellID = " + CellID + " and ManualWCID IS NULL and   ((InsertedOn <= '" + endDateTime + "' and IsDeleted = 0) or   ((IsDeleted = 1) and ( InsertedOn <= '" + endDateTime + "' and DeletedDate >= '" + startDateTime + "' )) ) ;";
                //IntoFile("JobReportExcelquery" + query1);
            }
            else if (lowestLevel == "WorkCentre")
            {
                //query1 = " SELECT * FROM ["+ConnectionFactory.DB+"].["+ConnectionFactory.Schema+"]tblmachinedetails WHERE MachineID = " + WorkCenterID + " and ManualWCID IS NULL and  ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( case when (IsDeleted = 1) then ( InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + startDateTime + "' ) end) ) ;";
                //query1 = " SELECT * FROM ["+ConnectionFactory.DB+"].["+ConnectionFactory.Schema+"]tblmachinedetails WHERE MachineID = " + WorkCenterID + " and ManualWCID IS NULL and   ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ((IsDeleted = 1) and ( InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + startDateTime + "' )) ) ;";
                query1 = " SELECT * FROM " + ConnectionFactory.DbName + ".tblmachinedetails WHERE MachineID = " + WorkCenterID + " and ManualWCID IS NULL and   ((InsertedOn <= '" + endDateTime + "' and IsDeleted = 0) or   ((IsDeleted = 1) and ( InsertedOn <= '" + endDateTime + "' and DeletedDate >= '" + startDateTime + "' )) ) ;";
                //IntoFile("JobReportExcelquery" + query1);
            }
            SqlDataAdapter da1 = new SqlDataAdapter(query1, mc.msqlConnection);
            da1.Fill(machin);
            mc.close();
            #endregion

            //DataTable for Consolidated Data 
            DataTable DTConsolidatedLosses = new DataTable();
            DTConsolidatedLosses.Columns.Add("Plant", typeof(string));
            DTConsolidatedLosses.Columns.Add("Shop", typeof(string));
            DTConsolidatedLosses.Columns.Add("Cell", typeof(string));
            DTConsolidatedLosses.Columns.Add("WCInvNo", typeof(string));
            DTConsolidatedLosses.Columns.Add("WCName", typeof(string));
            DTConsolidatedLosses.Columns.Add("CorrectedDate", typeof(string));
            //DTConsolidatedLosses.Columns.Add("HMIID", typeof(string));
            DTConsolidatedLosses.Columns.Add("OpName", typeof(string));
            DTConsolidatedLosses.Columns.Add("WOPF", typeof(int));
            DTConsolidatedLosses.Columns.Add("WOProcessed", typeof(int));
            DTConsolidatedLosses.Columns.Add("TotalWOQty", typeof(int));
            DTConsolidatedLosses.Columns.Add("TotalTarget", typeof(int));
            DTConsolidatedLosses.Columns.Add("TotalDelivered", typeof(int));
            DTConsolidatedLosses.Columns.Add("TargetNC", typeof(double));
            DTConsolidatedLosses.Columns.Add("TotalValueAdding", typeof(double));
            DTConsolidatedLosses.Columns.Add("TotalLosses", typeof(double));
            DTConsolidatedLosses.Columns.Add("TotalSetUp", typeof(double));
            DTConsolidatedLosses.Columns.Add("Breakdown  Loss", typeof(double));
            DTConsolidatedLosses.Columns.Add("Minor Loss", typeof(double));
            //Get All Losses and Insert into DataTable
            DataTable LossCodesData = new DataTable();
            using (MsqlConnection mcLossCodes = new MsqlConnection())
            {
                mcLossCodes.open();
                string startDateTime1 = frmDate.ToString("yyyy-MM-dd") + " " + new TimeSpan(6, 0, 0);
                //string query = @"select LossCodeID,LossCode from ["+ConnectionFactory.DB+"].["+ConnectionFactory.Schema+"]tbllossescodes  where MessageType != 'BREAKDOWN' and ((CreatedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or "
                //            + "( case when (IsDeleted = 1) then ( CreatedOn <=  '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ModifiedOn >= '" + startDateTime1 + "') end) ) and LossCodeID NOT IN (  "
                //            + "SELECT DISTINCT LossCodeID FROM (  "
                //            + "SELECT DISTINCT LossCodesLevel1ID AS LossCodeID FROM ["+ConnectionFactory.DB+"].["+ConnectionFactory.Schema+"]tbllossescodes where MessageType != 'BREAKDOWN' and LossCodesLevel1ID is not null  and ((CreatedOn <=  '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or  "
                //            + "( case when (IsDeleted = 1) then ( CreatedOn <=  '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ModifiedOn >= '" + startDateTime1 + "' ) end) ) "
                //            + "UNION  "
                //            + "SELECT DISTINCT LossCodesLevel2ID AS LossCodeID FROM ["+ConnectionFactory.DB+"].["+ConnectionFactory.Schema+"]tbllossescodes where MessageType != 'BREAKDOWN' and LossCodesLevel2ID is not null  and ((CreatedOn <=  '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or  "
                //            + "( case when (IsDeleted = 1) then ( CreatedOn <=  '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ModifiedOn >= '" + startDateTime1 + "' ) end) )  "
                //            + ") AS derived ) order by LossCodesLevel1ID;";

                //string query = @"select LossCodeID,LossCode from ["+ConnectionFactory.DB+"].["+ConnectionFactory.Schema+"]tbllossescodes  where MessageType != 'BREAKDOWN' and ((CreatedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or "
                //            + "( (IsDeleted = 1) and ( CreatedOn <=  '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ModifiedOn >= '" + startDateTime1 + "')) ) and LossCodeID NOT IN (  "
                //            + "SELECT DISTINCT LossCodeID FROM (  "
                //            + "SELECT DISTINCT LossCodesLevel1ID AS LossCodeID FROM ["+ConnectionFactory.DB+"].["+ConnectionFactory.Schema+"]tbllossescodes where MessageType != 'BREAKDOWN' and LossCodesLevel1ID is not null  and ((CreatedOn <=  '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or  "
                //            + "( (IsDeleted = 1) and ( CreatedOn <=  '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ModifiedOn >= '" + startDateTime1 + "' )) ) "
                //            + "UNION  "
                //            + "SELECT DISTINCT LossCodesLevel2ID AS LossCodeID FROM ["+ConnectionFactory.DB+"].["+ConnectionFactory.Schema+"]tbllossescodes where MessageType != 'BREAKDOWN' and LossCodesLevel2ID is not null  and ((CreatedOn <=  '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or  "
                //            + "( (IsDeleted = 1) and ( CreatedOn <=  '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ModifiedOn >= '" + startDateTime1 + "' )) )  "
                //            + ") AS derived ) order by LossCodesLevel1ID;";
                string query = @"select LossCodeID,LossCode from " + ConnectionFactory.DbName + ".tbllossescodes  where MessageType != 'BREAKDOWN' and ((CreatedOn <= '" + endDateTime + "' and IsDeleted = 0) or "
                    + "( (IsDeleted = 1) and ( CreatedOn <=  '" + endDateTime + "' and ModifiedOn >= '" + startDateTime1 + "')) ) and LossCodeID NOT IN (  "
                    + "SELECT DISTINCT LossCodeID FROM (  "
                    + "SELECT DISTINCT LossCodesLevel1ID AS LossCodeID FROM " + ConnectionFactory.DbName + ".tbllossescodes where MessageType != 'BREAKDOWN' and LossCodesLevel1ID is not null  and ((CreatedOn <=  '" + endDateTime + "' and IsDeleted = 0) or  "
                    + "( (IsDeleted = 1) and ( CreatedOn <=  '" + endDateTime + "' and ModifiedOn >= '" + startDateTime1 + "' )) ) "
                    + "UNION  "
                    + "SELECT DISTINCT LossCodesLevel2ID AS LossCodeID FROM " + ConnectionFactory.DbName + ".tbllossescodes where MessageType != 'BREAKDOWN' and LossCodesLevel2ID is not null  and ((CreatedOn <=  '" + endDateTime + "' and IsDeleted = 0) or  "
                    + "( (IsDeleted = 1) and ( CreatedOn <=  '" + endDateTime + "' and ModifiedOn >= '" + startDateTime1 + "' )) )  "
                    + ") AS derived ) order by LossCodesLevel1ID;";

                //IntoFile("JobReportExcelquery" + query1);
                SqlDataAdapter daLossCodesData = new SqlDataAdapter(query, mcLossCodes.msqlConnection);
                daLossCodesData.Fill(LossCodesData);
                mcLossCodes.close();
            }
            int LossesStartsATCol = 30;
            var LossesList = new List<KeyValuePair<int, string>>();

            #region LossCodes Into LossList
            for (int i = 0; i < LossCodesData.Rows.Count; i++)
            {
                int losscode = Convert.ToInt32(LossCodesData.Rows[i][0]);
                if (losscode == 999)
                { }
                string losscodeName = Convert.ToString(LossCodesData.Rows[i][1]);

                var lossdata = obj1.GetLossDet2(losscode);
                int level = lossdata.LossCodesLevel;
                if (level == 3)
                {
                    int lossLevel1ID = Convert.ToInt32(lossdata.LossCodesLevel1ID);
                    int lossLevel2ID = Convert.ToInt32(lossdata.LossCodesLevel2ID);
                    var lossdata1 = obj1.GetLossDet2(lossLevel1ID);
                    var lossdata2 = obj1.GetLossDet2(lossLevel2ID);
                    losscodeName = lossdata1.LossCode + " :: " + lossdata2.LossCode + " : " + lossdata.LossCode;
                }

                else if (level == 2)
                {
                    int lossLevel1ID = Convert.ToInt32(lossdata.LossCodesLevel1ID);
                    var lossdata1 = obj1.GetLossDet2(lossLevel1ID);

                    losscodeName = lossdata1.LossCode + ":" + lossdata.LossCode;
                }
                else if (level == 1)
                {
                    if (losscode == 999)
                    {
                        losscodeName = "NoCode Entered";
                    }
                    else if (losscode == 9999)
                    {
                        losscodeName = "UnIdentified BreakDown";
                    }
                    else
                    {
                        losscodeName = lossdata.LossCode;
                    }
                }
                //losscodeName = LossHierarchy3rdLevel(losscode);
                DTConsolidatedLosses.Columns.Add(losscodeName, typeof(double));
                DTConsolidatedLosses.Columns[losscodeName].DefaultValue = "0";
                //Code to write LossesNames to Excel.
                string columnAlphabet = ExcelColumnFromNumber(LossesStartsATCol);

                worksheet.Cells[columnAlphabet + 4].Value = losscodeName;
                //worksheet.Cells[columnAlphabet + 5].Value = "AF";

                LossesStartsATCol++;
                //Add the LossesToList
                LossesList.Add(new KeyValuePair<int, string>(losscode, losscodeName));
            }
            #endregion

            #region Push Headers that r supposed to be after Losses

            int ColIndex = LossCodesData.Rows.Count + 26 + 1; //+1 For Gap (& Testing) //26 is Previous DATA(Plant,Shop......)
            string ColAfterLosses = ExcelColumnFromNumber(ColIndex);
            worksheet.Cells[ColAfterLosses + "4"].Value = "Rejected Qty";
            ColIndex = LossCodesData.Rows.Count + 26 + 2;
            ColAfterLosses = ExcelColumnFromNumber(ColIndex);
            worksheet.Cells[ColAfterLosses + "4"].Value = "Rejected Reason";
            ColIndex = LossCodesData.Rows.Count + 26 + 3;
            ColAfterLosses = ExcelColumnFromNumber(ColIndex);
            worksheet.Cells[ColAfterLosses + "4"].Value = "Operator Name";
            ColIndex = LossCodesData.Rows.Count + 26 + 4;
            ColAfterLosses = ExcelColumnFromNumber(ColIndex);
            worksheet.Cells[ColAfterLosses + "4"].Value = "Type";
            //To skip a Column Just Increment the ColIndex extra +1
            ColIndex = LossCodesData.Rows.Count + 26 + 6;
            ColAfterLosses = ExcelColumnFromNumber(ColIndex);
            worksheet.Cells[ColAfterLosses + "4"].Value = "PartNo & OpNo";
            ColIndex = LossCodesData.Rows.Count + 26 + 7;
            ColAfterLosses = ExcelColumnFromNumber(ColIndex);
            worksheet.Cells[ColAfterLosses + "4"].Value = "NC Cutting Time Per Part";
            ColIndex = LossCodesData.Rows.Count + 26 + 8;
            ColAfterLosses = ExcelColumnFromNumber(ColIndex);
            worksheet.Cells[ColAfterLosses + "4"].Value = "Total NC Cutting Time";
            ColIndex = LossCodesData.Rows.Count + 26 + 9;
            ColAfterLosses = ExcelColumnFromNumber(ColIndex);
            worksheet.Cells[ColAfterLosses + "4"].Value = "%";
            ColIndex = LossCodesData.Rows.Count + 26 + 10;
            ColAfterLosses = ExcelColumnFromNumber(ColIndex);
            worksheet.Cells[ColAfterLosses + "4"].Value = "SplitWO";
            ColIndex = LossCodesData.Rows.Count + 26 + 11;
            ColAfterLosses = ExcelColumnFromNumber(ColIndex);
            worksheet.Cells[ColAfterLosses + "4"].Value = "Start Time";
            //worksheet.Column(ColIndex).Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss"; 

            ColIndex = LossCodesData.Rows.Count + 26 + 12;
            ColAfterLosses = ExcelColumnFromNumber(ColIndex);
            worksheet.Cells[ColAfterLosses + "4"].Value = "End Time";
            //worksheet.Column(ColIndex).Style.Numberformat.ToString();

            //ColIndex = LossCodesData.Rows.Count + 26 + 13;
            //ColAfterLosses = ExcelColumnFromNumber(ColIndex);
            //worksheet.Cells[ColAfterLosses + "4"].Value = "Operation Cancellation";

            //ColIndex = LossCodesData.Rows.Count + 26 + 14;
            //ColAfterLosses = ExcelColumnFromNumber(ColIndex);
            //worksheet.Cells[ColAfterLosses + "4"].Value = "Operation Cancellation Qty";

            //ColIndex = LossCodesData.Rows.Count + 26 + 15;
            //ColAfterLosses = ExcelColumnFromNumber(ColIndex);
            //worksheet.Cells[ColAfterLosses + "4"].Value = "Nesting Sheet No";
            #endregion

            DateTime UsedDateForExcel = Convert.ToDateTime(frmDate);
            //For each Date ...... for all Machines.
            var Col = 'B';
            int Row = 5; // Gap to Insert OverAll data. DataStartRow + MachinesCount + 2(1 for HighestLevel & another for Gap).
            int ROWStart = 5;
            int Sno = 1;
            int numberofRows = Row;
            int numberOFRowsBatch = 0;
            int numberofcount = 0;
            string PrevOpeatorName = "";
            string CurrOpeatorName = "";
            DateTime BatchST = DateTime.Now;
            int k = 0;
            int jk = 0;
            List<ManualBatching> BatchList = new List<ManualBatching>();
            string finalLossCol = null;

            for (int i = 0; i < TotalDay + 1; i++)
            {
                int StartingRowForToday = Row;
                jk = 0;
                ROWStart = 5;
                string dateforMachine = UsedDateForExcel.ToString("yyyy-MM-dd");

                int NumMacsToExcel = 0;
                for (int n = 0; n < machin.Rows.Count; n++)
                {
                    NumMacsToExcel++;
                    double CummulativeOfAllLosses = 0;
                    if (n == 0 && i != 0)
                    {
                        Row++;
                        StartingRowForToday = Row;
                    }

                    int MachineID = Convert.ToInt32(machin.Rows[n][0]);
                    List<string> HierarchyData = GetHierarchyData(MachineID);
                    List<int> MacList = new List<int>();
                    int IsNormalWC = 0;
                    IsNormalWC = Convert.ToInt32(obj.GettblmachineDet3(MachineID));
                    //IsNormalWC = Convert.ToInt32(obj.GettblmachineDet3(MachineID));
                    if (IsNormalWC == 1)
                    {
                        var SubWCData = obj.GettbMachineDetails3(MachineID);
                        //var SubWCData = obj.GettbMachineDetails3(MachineID);
                        foreach (var subMacs in SubWCData)
                        {
                            MacList.Add(Convert.ToInt32(subMacs.MachineID));
                        }
                    }
                    else
                    {
                        MacList.Add(MachineID);
                    }

                    string MacIDsString = null;
                    //MacIDsString = "0";
                    IsNormalWC = Convert.ToInt32(obj.GettblmachineDet3(MachineID));
                    if (IsNormalWC == 1)
                    {
                        var SubWCData = obj.GettbMachineDetails3(MachineID);
                        foreach (var subMacs in SubWCData)
                        {
                            if (MacIDsString == null)
                            {
                                MacIDsString = Convert.ToString(subMacs.MachineID);
                            }
                            else
                            {
                                MacIDsString += "," + Convert.ToString(subMacs.MachineID);
                            }
                        }
                    }
                    else
                    {
                        if (MacIDsString == null)
                        {
                            MacIDsString = Convert.ToString(MachineID);
                        }
                        else
                        {
                            MacIDsString += "," + Convert.ToString(MachineID);
                        }
                    }


                    //Added this machineDetails into Datatable
                    string WCInvNoString = HierarchyData[3];
                    string correctedDate = UsedDateForExcel.ToString("yyyy-MM-dd");
                    //IntoFile("JobReportExcelcorrectedDate" + correctedDate);

                    #region Get HMI DATA and Push For this machine and Date.

                    //Get general hmi data, later get prodfai && operator Combination based data inside loop
                    //var HMIDATA = db.tblworeports.Where(m => m.MachineID == MachineID && m.CorrectedDate == correctedDate).GroupBy(m => m.HMIID).Select(m => m.FirstOrDefault()).ToList();
                    //var HMIDATA = (from r in db.tblworeports
                    //               where MacList.Contains((int)r.MachineID) && r.CorrectedDate == correctedDate
                    //               select r).ToList().GroupBy(a => a.HMIID).ToList();
                    //scraped on 2017-04-04 
                    //var HMIDATA = db.tblworeports.Where(m => MacList.Contains((int)m.MachineID))
                    //    .Where(m => m.CorrectedDate == correctedDate).OrderByDescending(m => m.HMIID).GroupBy(m => m.HMIID).Select(m => m).ToList();
                    //if ((!string.IsNullOrEmpty(Operator.Trim())) && ProdFAI == "OverAll")
                    //{
                    //    //HMIDATA = db.tblworeports.Where(m => m.MachineID == MachineID && m.CorrectedDate == correctedDate && m.OperatorName == Operator).GroupBy(m => m.HMIID).Select(m => m.FirstOrDefault()).ToList();
                    //    HMIDATA = db.tblworeports.Where(m => MacList.Contains((int)m.MachineID) && m.CorrectedDate == correctedDate && m.OperatorName == Operator)
                    //    .OrderByDescending(m => m.HMIID).GroupBy(m => m.HMIID).Select(m => m).ToList();
                    //}
                    //else if ((!string.IsNullOrEmpty(Operator.Trim())) && ProdFAI != "OverAll")
                    //{
                    //    //HMIDATA = db.tblworeports.Where(m => m.MachineID == MachineID && m.CorrectedDate == correctedDate && m.OperatorName == Operator && m.Type == ProdFAI).GroupBy(m => m.HMIID).Select(m => m.FirstOrDefault()).ToList();
                    //    if (ProdFAI != "Others")
                    //    {
                    //        HMIDATA = db.tblworeports.Where(m => MacList.Contains((int)m.MachineID) && m.CorrectedDate == correctedDate && m.OperatorName == Operator && m.Type == ProdFAI)
                    //       .OrderByDescending(m => m.HMIID).GroupBy(m => m.HMIID).Select(m => m).ToList();
                    //    }
                    //    else
                    //    {
                    //        HMIDATA = db.tblworeports.Where(m => MacList.Contains((int)m.MachineID) && m.CorrectedDate == correctedDate && m.OperatorName == Operator && m.Type != "Production" && m.Type != "FAI")
                    //       .OrderByDescending(m => m.HMIID).GroupBy(m => m.HMIID).Select(m => m).ToList();
                    //    }
                    //}
                    //else if ((string.IsNullOrEmpty(Operator.Trim())) && ProdFAI != "OverAll")
                    //{
                    //    //HMIDATA = db.tblworeports.Where(m => m.MachineID == MachineID && m.CorrectedDate == correctedDate && m.Type == ProdFAI).GroupBy(m => m.HMIID).Select(m => m.FirstOrDefault()).ToList();
                    //    if (ProdFAI != "Others")
                    //    {
                    //        HMIDATA = db.tblworeports.Where(m => MacList.Contains((int)m.MachineID) && m.CorrectedDate == correctedDate && m.Type == ProdFAI)
                    //     .OrderByDescending(m => m.HMIID).GroupBy(m => m.HMIID).Select(m => m).ToList();
                    //        //.OrderByDescending(m => m.HMIID).Take(1).ToList(); //Wrong on 2017-03-24
                    //    }
                    //    else
                    //    {
                    //        HMIDATA = db.tblworeports.Where(m => MacList.Contains((int)m.MachineID) && m.CorrectedDate == correctedDate && m.Type != "Production" && m.Type != "FAI")
                    //    .OrderByDescending(m => m.HMIID).GroupBy(m => m.HMIID).Select(m => m).ToList();
                    //    }
                    //}


                    DataTable HMIDATA = new DataTable();
                    using (MsqlConnection mcHMI = new MsqlConnection())
                    {
                        mcHMI.open();
                        //string query = @"select * from tblworeport where MachineID in ( " + MacIDsString + ") and CorrectedDate = '" + correctedDate + "' group by HMIID order by HMIID desc ;";
                        string query = @"select * from " + ConnectionFactory.DbName + ".tblworeport where MachineID in ( " + MacIDsString + ") and CorrectedDate = '" + correctedDate + "' order by HMIID desc ;";
                        if (ProdFAI == "OverAll")
                        {
                            //query = @"select * from tblworeport where MachineID in ( " + MacIDsString + ") and CorrectedDate = '" + correctedDate + "' and OperatorName = '" + Operator + "'  group by HMIID order by HMIID desc ;";
                            query = @"select * from " + ConnectionFactory.DbName + ".tblworeport where MachineID in ( " + MacIDsString + ") and CorrectedDate = '" + correctedDate + "' order by HMIID desc ;";
                            //IntoFile("JobReportquery" + query);
                        }
                        else if (ProdFAI != "OverAll")
                        {
                            if (ProdFAI != "Others")
                            {
                                //query = @"select * from tblworeport where MachineID in ( " + MacIDsString + ") and CorrectedDate = '" + correctedDate + "' and OperatorName = '" + Operator + "' and Type = '" + ProdFAI + "'  group by HMIID order by HMIID desc ;";
                                query = @"select * from " + ConnectionFactory.DbName + ".tblworeport where MachineID in ( " + MacIDsString + ") and CorrectedDate = '" + correctedDate + "' and Type = '" + ProdFAI + "' order by HMIID desc ;";
                                //IntoFile("JobReportquery" + query);
                            }
                            else
                            {
                                //query = @"select * from tblworeport where MachineID in ( " + MacIDsString + ") and CorrectedDate = '" + correctedDate + "' and OperatorName = '" + Operator + "' and Type != 'Production' and Type != 'FAI' group by HMIID order by HMIID desc ;";
                                query = @"select * from " + ConnectionFactory.DbName + ".tblworeport where MachineID in ( " + MacIDsString + ") and CorrectedDate = '" + correctedDate + "' and Type != 'Production' and Type != 'FAI' order by HMIID desc ;";
                                //IntoFile("JobReportquery" + query);
                            }
                        }
                        else if (ProdFAI != "OverAll")
                        {
                            if (ProdFAI != "Others")
                            {
                                //query = @"select * from tblworeport where MachineID in ( " + MacIDsString + ") and CorrectedDate = '" + correctedDate + "' and Type = '" + ProdFAI + "' group by HMIID order by HMIID desc ;";
                                query = @"select * from " + ConnectionFactory.DbName + ".tblworeport where MachineID in ( " + MacIDsString + ") and CorrectedDate = '" + correctedDate + "' and Type = '" + ProdFAI + "' order by HMIID desc ;";
                                //IntoFile("JobReportquery" + query);
                            }
                            else
                            {
                                //query = @"select * from tblworeport where MachineID in ( " + MacIDsString + ") and CorrectedDate = '" + correctedDate + "' and Type != 'Production' and Type != 'FAI' group by HMIID order by HMIID desc ;";
                                query = @"select * from " + ConnectionFactory.DbName + ".tblworeport where MachineID in ( " + MacIDsString + ") and CorrectedDate = '" + correctedDate + "' and Type != 'Production' and Type != 'FAI' order by HMIID desc ;";
                                //IntoFile("JobReportquery" + query);
                            }
                        }
                        try
                        {
                            SqlDataAdapter daLossCodesData = new SqlDataAdapter(query, mcHMI.msqlConnection);
                            daLossCodesData.Fill(HMIDATA);
                            mcHMI.close();
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    //if (Operator == null & ProdFAI == "OverAll") . This purpose is served by 1st time HMIDATA is assigned.
                    if (HMIDATA.Rows.Count > 0)
                    {
                        int StartRows = 0;
                        int EndRows = 0;
                        double BatchGreen = 0;

                        //Now Loop through and Push Data into Excel
                        for (int ji = 0; ji < HMIDATA.Rows.Count; ji++)
                        {

                            //DateTime woStartTime = Convert.ToDateTime(row.Date);
                            //DateTime woEndTime = Convert.ToDateTime(row.Time);
                            if (Convert.ToInt32(HMIDATA.Rows[ji][27]) == 1) //Its a MultiWorkOrder
                            {
                                #region
                                double dimensionsResult = 0;
                                int MachineIDRow = Convert.ToInt32(HMIDATA.Rows[ji][1]);

                                //Getting MeasuringUnitId from tblpartwiseworkcenter 
                                var MeasuringUnitId = obj.GettblpartwiseworkcenterDet3(MachineIDRow);
                                // var MeasuringUnitId = obj.GettblpartwiseworkcenterDet3(MachineIDRow);
                                string PartNo = (HMIDATA.Rows[ji][6]).ToString();

                                if (MeasuringUnitId != null)
                                {
                                    dimensionsResult = CalculateSurfaceAreaAndPerimeter(PartNo, (int)MeasuringUnitId.MeasuringUnitId);
                                }

                                int HmiID = Convert.ToInt32(HMIDATA.Rows[ji][2]);
                                var MulitWOData = obj.Gettblworeportfor1(HmiID);
                                //var MulitWOData = db.tblworeports.Where(m => m.HMIID == HmiID).ToList();
                                if (worksheet.Cells["B" + Row + ":B" + (Row + MulitWOData.Count)].Merge != true)
                                {
                                    worksheet.Cells["B" + Row + ":B" + (Row + MulitWOData.Count)].Merge = true;
                                    worksheet.Cells["C" + Row + ":C" + (Row + MulitWOData.Count)].Merge = true;
                                    worksheet.Cells["D" + Row + ":D" + (Row + MulitWOData.Count)].Merge = true;
                                    worksheet.Cells["E" + Row + ":E" + (Row + MulitWOData.Count)].Merge = true;
                                    worksheet.Cells["F" + Row + ":F" + (Row + MulitWOData.Count)].Merge = true;
                                    worksheet.Cells["G" + Row + ":G" + (Row + MulitWOData.Count)].Merge = true;
                                    worksheet.Cells["H" + Row + ":H" + (Row + MulitWOData.Count)].Merge = true;
                                }
                                worksheet.Cells["B" + Row].Value = Sno++;
                                worksheet.Cells["C" + Row].Value = HierarchyData[0];
                                worksheet.Cells["D" + Row].Value = HierarchyData[1];
                                worksheet.Cells["E" + Row].Value = HierarchyData[2];
                                worksheet.Cells["F" + Row].Value = HierarchyData[3];//Display Name
                                worksheet.Cells["G" + Row].Value = HierarchyData[3];//Mac INV
                                worksheet.Cells["H" + Row].Value = UsedDateForExcel.ToString("yyyy-MM-dd");

                                worksheet.Cells["I" + Row].Value = HMIDATA.Rows[ji][4];// Shift;
                                worksheet.Cells["J" + Row].Value = HMIDATA.Rows[ji][6] + "-" + MulitWOData.Count;// PartNo + "-" + MulitWOData.Count;
                                worksheet.Cells["K" + Row].Value = HMIDATA.Rows[ji][7] + "-" + MulitWOData.Count; //WONo
                                worksheet.Cells["L" + Row].Value = HMIDATA.Rows[ji][8] + "-" + MulitWOData.Count; //OpNo
                                worksheet.Cells["M" + Row].Value = MulitWOData.Sum(m => m.TargetQty);
                                double delQty = 0;
                                //double.TryParse(Convert.ToString(row.Delivered_Qty), out delQty);
                                worksheet.Cells["N" + Row].Value = MulitWOData.Sum(m => m.DeliveredQty);
                                if (Convert.ToInt32(HMIDATA.Rows[ji][11]) == 0) //isPF
                                {
                                    worksheet.Cells["O" + Row].Value = "Yes";
                                    //worksheet.Cells["P" + Row].Value = "";
                                }
                                else
                                {
                                    //worksheet.Cells["O" + Row].Value = "";
                                    worksheet.Cells["P" + Row].Value = "Yes";
                                }
                                worksheet.Cells["Q" + Row].Value = "";//isHold
                                worksheet.Cells["R" + Row].Value = "";//Hold Reason

                                double SettingTime = Convert.ToDouble(HMIDATA.Rows[ji][14]); //Setting
                                worksheet.Cells["S" + Row].Value = Math.Round(SettingTime, 1);

                                if (dimensionsResult == 0)
                                {
                                    worksheet.Cells["T" + Row].Value = "";
                                }
                                else
                                {
                                    worksheet.Cells["T" + Row].Value = Math.Round(dimensionsResult, 2);
                                }

                                //double deliveredQuantity = (double)MulitWOData.Sum(m => m.DeliveredQty);
                                //worksheet.Cells["U" + Row].Value = Math.Round((deliveredQuantity * dimensionsResult), 2);

                                double Green = Convert.ToDouble(HMIDATA.Rows[ji][13]); //cutting

                                //double Changeover = GetChangeoverTimeForWO(correctedDate, MachineID, woStartTime, woEndTime);
                                //worksheet.Cells["S" + Row].Value = Math.Round(Changeover / 60, 1);

                                double idleTime = Convert.ToDouble(HMIDATA.Rows[ji][16]) - Convert.ToDouble(HMIDATA.Rows[ji][14]); //IDLE - Setting
                                worksheet.Cells["X" + Row].Value = Math.Round(idleTime, 2);

                                worksheet.Cells["Y" + Row].Formula = "=SUM(S" + Row + ",V" + Row + ",W" + Row + ",X" + Row + ",AB" + Row + ")";

                                worksheet.Cells["Z" + Row].Value = "";//Empty finalLossCol
                                int column = 26 + LossCodesData.Rows.Count; // StartCol in Excel + TotalLosses - 1
                                finalLossCol = ExcelColumnFromNumber(column);

                                worksheet.Cells["AA" + Row].Formula = "=SUM(AB" + Row + ":" + finalLossCol + Row + ")";

                                double BreakdownDuration = Convert.ToDouble(HMIDATA.Rows[ji][17]); //Breakdown
                                worksheet.Cells["AB" + Row].Value = Math.Round(BreakdownDuration, 1);

                                double MinorLoss = Convert.ToDouble(HMIDATA.Rows[ji][30]); //MinorLoss
                                worksheet.Cells["AC" + Row].Value = Math.Round(MinorLoss, 1);

                                //Push Data that is supposed to be after Losses.
                                ColIndex = LossCodesData.Rows.Count + 26 + 1; //+1 For Gap (& Testing) //26 is Previous DATA(Plant,Shop......)
                                ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                worksheet.Cells[ColAfterLosses + Row].Value = 0;
                                ColIndex = LossCodesData.Rows.Count + 26 + 2;
                                ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                worksheet.Cells[ColAfterLosses + Row].Value = "0";
                                ColIndex = LossCodesData.Rows.Count + 26 + 3;
                                ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                worksheet.Cells[ColAfterLosses + Row].Value = HMIDATA.Rows[ji][3];// OperatorName; 
                                ColIndex = LossCodesData.Rows.Count + 26 + 4;
                                ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                worksheet.Cells[ColAfterLosses + Row].Value = HMIDATA.Rows[ji][18];// Type;
                                //To skip a Column Just Increment the ColIndex extra +1
                                ColIndex = LossCodesData.Rows.Count + 26 + 6;
                                ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                worksheet.Cells[ColAfterLosses + Row].Value = HMIDATA.Rows[ji][6] + "#" + HMIDATA.Rows[ji][8]; // PartNo # OpNo
                                ColIndex = LossCodesData.Rows.Count + 26 + 7;
                                ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                string ColForNCCuttingTime = ColAfterLosses;
                                string partNo = HMIDATA.Rows[ji][6].ToString();// PartNo;
                                string opNo = HMIDATA.Rows[ji][8].ToString();// OpNo;
                                double stdCuttingTime = 0; double ProdOfstdCuttingTimeDelivQty = 0;
                                int PF = 0, JF = 0;
                                foreach (var MulitWOrow in MulitWOData)
                                {
                                    double stdCuttingTimeLocal = 0;
                                    int DelivQty = 0;
                                    stdCuttingTimeLocal = Convert.ToDouble(MulitWOrow.CuttingTime);
                                    DelivQty = Convert.ToInt32(MulitWOrow.DeliveredQty);
                                    stdCuttingTime += stdCuttingTimeLocal;
                                    ProdOfstdCuttingTimeDelivQty += DelivQty * stdCuttingTimeLocal;
                                    if (MulitWOrow.IsPF == 0)
                                    {
                                        PF += 1;
                                    }
                                    else
                                    {
                                        JF += 1;
                                    }
                                }
                                worksheet.Cells[ColAfterLosses + Row].Value = stdCuttingTime;
                                ColIndex = LossCodesData.Rows.Count + 26 + 8;
                                ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                //worksheet.Cells[ColAfterLosses + Row].Formula = "=PRODUCT( N" + Row + "," + ColForNCCuttingTime + Row + ")";
                                //double.TryParse(Convert.ToString(row.Delivered_Qty), out delQty);
                                worksheet.Cells[ColAfterLosses + Row].Value = ProdOfstdCuttingTimeDelivQty;
                                string ColForTotalNCCuttingTime = ColAfterLosses;
                                ColIndex = LossCodesData.Rows.Count + 26 + 9;
                                ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                //worksheet.Cells[ColAfterLosses + Row].Formula = "=IF((OR((R" + Row + "<=0),(" + ColForTotalNCCuttingTime + Row + " <=0))), 0, R" + Row + "/" + ColForTotalNCCuttingTime + Row + ")";
                                //worksheet.Cells[ColAfterLosses + Row].Formula = "=IF((OR((R" + Row + "<=0),(" + ColForTotalNCCuttingTime + Row + " <=0))), 0," + ColForTotalNCCuttingTime + Row + "/ R" + Row + ")";
                                // var TotalNCCutTimeDIVCuttingTime = worksheet.Calculate("=IF((OR((R" + Row + "<=0),(" + ColForTotalNCCuttingTime + Row + " <=0))), 0, R" + Row + "/" + ColForTotalNCCuttingTime + Row + ")");
                                var TotalNCCutTimeDIVCuttingTime = worksheet.Calculate("=IF((OR((R" + Row + "<=0),(" + ColForTotalNCCuttingTime + Row + " <=0))), 0," + ColForTotalNCCuttingTime + Row + "/ R" + Row + ")");
                                double Percentage = Math.Round(Convert.ToDouble(TotalNCCutTimeDIVCuttingTime) * 100, 0);
                                if (Percentage < 100)
                                {
                                    worksheet.Cells[ColAfterLosses + Row].Value = Percentage;
                                }
                                else
                                {
                                    worksheet.Cells[ColAfterLosses + Row].Value = 100;
                                }
                                //Start Time
                                ColIndex = LossCodesData.Rows.Count + 26 + 11;
                                ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                worksheet.Cells[ColAfterLosses + Row].Value = HMIDATA.Rows[ji][36].ToString(); //Start Time
                                                                                                               //End Time
                                ColIndex = LossCodesData.Rows.Count + 26 + 12;
                                ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                worksheet.Cells[ColAfterLosses + Row].Value = HMIDATA.Rows[ji][37].ToString(); //End Time;

                                // Push Loss Value into  DataTable & Excel
                                DataRow dr = DTConsolidatedLosses.AsEnumerable().FirstOrDefault(r => r.Field<string>("WCInvNo") == @WCInvNoString && r.Field<string>("CorrectedDate") == dateforMachine);
                                if (dr != null)
                                {
                                    //plant, shop, cell, macINV, WcName, CorrectedDate,WOPF,WOProcessed,
                                    //TotalWOQty,TotalTarget,TotalDelivered,TargetNC,TotalValueAdding,TotalLosses,TotalSetUp
                                    DTConsolidatedLosses.Rows.Add(HierarchyData[0], HierarchyData[1], HierarchyData[2], HierarchyData[3],
                                    HierarchyData[3], dateforMachine, Convert.ToString(HMIDATA.Rows[ji][3]), PF, JF, PF + JF, Convert.ToInt32(HMIDATA.Rows[ji][9]), delQty, ProdOfstdCuttingTimeDelivQty, Green, idleTime, SettingTime);
                                }
                                else
                                {
                                    //plant, shop, cell, macINV, WcName, CorrectedDate,WOPF,WOProcessed,
                                    //TotalWOQty,TotalTarget,TotalDelivered,TargetNC,TotalValueAdding,TotalLosses,TotalSetUp
                                    DTConsolidatedLosses.Rows.Add(HierarchyData[0], HierarchyData[1], HierarchyData[2], HierarchyData[3],
                                    HierarchyData[3], dateforMachine, Convert.ToString(HMIDATA.Rows[ji][3]), PF, JF, PF + JF, Convert.ToInt32(HMIDATA.Rows[ji][9]), delQty, ProdOfstdCuttingTimeDelivQty, Green, idleTime, SettingTime);
                                }

                                #region Capture and Push Losses

                                //now push 0 for every other loss into excel
                                worksheet.Cells["AD" + Row + ":" + finalLossCol + Row].Value = Convert.ToDouble(0.0);

                                //to Capture and Push , Losses that occured.
                                //List<KeyValuePair<int, double>> LossesdurationList = GetAllLossesDurationSecondsForWO(MachineID, correctedDate, woStartTime, woEndTime);
                                //DataRow dr1 = DTConsolidatedLosses.AsEnumerable().FirstOrDefault(r => r.Field<string>("WCInvNo") == WCInvNoString && r.Field<string>("CorrectedDate") == @dateforMachine);
                                //if (dr1 != null)
                                //{
                                //    foreach (var loss in LossesdurationList)
                                //    {

                                //        int LossID = loss.Key;
                                //        double Duration = loss.Value;
                                //        var lossdata = db.tbllossescodes.Where(m => m.LossCodeID == LossID).FirstOrDefault();
                                //        int level = lossdata.LossCodesLevel;
                                //        string losscodeName = null;

                                #region To Get LossCode Hierarchy
                                //        if (level == 3)
                                //        {
                                //            int lossLevel1ID = Convert.ToInt32(lossdata.LossCodesLevel1ID);
                                //            int lossLevel2ID = Convert.ToInt32(lossdata.LossCodesLevel2ID);
                                //           var lossdata1 = obj1.GetLossDet2(lossLevel1ID);
                                //             var lossdata2 = obj1.GetLossDet2(lossLevel2ID);
                                //            losscodeName = lossdata1.LossCode + " :: " + lossdata2.LossCode + " : " + lossdata.LossCode;
                                //        }
                                //        else if (level == 2)
                                //        {
                                //            int lossLevel1ID = Convert.ToInt32(lossdata.LossCodesLevel1ID);
                                //           var lossdata1 = obj1.GetLossDet2(lossLevel1ID);

                                //            losscodeName = lossdata1.LossCode + ":" + lossdata.LossCode;
                                //        }
                                //        else if (level == 1)
                                //        {
                                //            if (LossID == 999)
                                //            {
                                //                losscodeName = "NoCode Entered";
                                //            }
                                //            else
                                //            {
                                //                losscodeName = lossdata.LossCode;
                                //            }
                                //        }
                                #endregion

                                //        int ColumnIndex = DTConsolidatedLosses.Columns[losscodeName].Ordinal;
                                //        string ColumnForThisLoss = ExcelColumnFromNumber(ColumnIndex + 9);
                                //        double DurInMinutes = Convert.ToDouble(Math.Round((Duration / (60)), 1)); //To Minutes:: 1 Decimal Place
                                //        worksheet.Cells[ColumnForThisLoss + "" + Row].Value = DurInMinutes;
                                //        if (DurInMinutes > 0)
                                //        {
                                //        }
                                //        dr1[losscodeName] = DurInMinutes;
                                //        CummulativeOfAllLosses += DurInMinutes;
                                //    }
                                //}

                                //to Capture and Push , Losses that occured.
                                // List<KeyValuePair<int, double>> LossesdurationList = GetAllLossesDurationSecondsForWO(MachineID, correctedDate, woStartTime, woEndTime);
                                var LossesdurationList = obj.Gettblwolossessfor1(HmiID);
                                // var LossesdurationList = obj.Gettblwolossessfor1(HmiID);
                                DataRow dr1 = DTConsolidatedLosses.AsEnumerable().FirstOrDefault(r => r.Field<string>("WCInvNo") == WCInvNoString && r.Field<string>("CorrectedDate") == @dateforMachine);
                                if (dr1 != null)
                                {
                                    foreach (var loss in LossesdurationList)
                                    {
                                        int LossID = Convert.ToInt32(loss.LossID);
                                        double Duration = Convert.ToDouble(loss.LossDuration);
                                        int level = Convert.ToInt32(loss.Level);
                                        string losscodeName = loss.LossName;

                                        #region To Get LossCode Hierarchy
                                        if (level == 3)
                                        {
                                            losscodeName = loss.LossCodeLevel1Name + " :: " + loss.LossCodeLevel2Name + " : " + losscodeName;
                                        }
                                        else if (level == 2)
                                        {
                                            losscodeName = loss.LossCodeLevel1Name + ":" + losscodeName;
                                        }
                                        else if (level == 1)
                                        {
                                            if (LossID == 999)
                                            {
                                                losscodeName = "NoCode Entered";
                                            }
                                            else
                                            {
                                                losscodeName = losscodeName;
                                            }
                                        }
                                        #endregion

                                        int ColumnIndex = 0;
                                        try
                                        {
                                            ColumnIndex = DTConsolidatedLosses.Columns[losscodeName].Ordinal;
                                        }
                                        catch (Exception ex)
                                        {
                                            ColumnIndex = 0;
                                        }

                                        if (ColumnIndex != 0)
                                        {
                                            string ColumnForThisLoss = ExcelColumnFromNumber(ColumnIndex + 9);
                                            double DurInMinutes = Convert.ToDouble(Math.Round((Duration), 1)); //To Minutes:: 1 Decimal Place
                                            worksheet.Cells[ColumnForThisLoss + "" + Row].Value = DurInMinutes;
                                            if (DurInMinutes > 0)
                                            {
                                            }
                                            dr1[losscodeName] = DurInMinutes;
                                            CummulativeOfAllLosses += DurInMinutes;
                                        }
                                        //    int ColumnIndex = DTConsolidatedLosses.Columns[losscodeName].Ordinal;
                                        //string ColumnForThisLoss = ExcelColumnFromNumber(ColumnIndex + 9);
                                        //double DurInMinutes = Convert.ToDouble(Math.Round((Duration), 1)); //To Minutes:: 1 Decimal Place
                                        //worksheet.Cells[ColumnForThisLoss + "" + Row].Value = DurInMinutes;
                                        //if (DurInMinutes > 0)
                                        //{
                                        //}
                                        //dr1[losscodeName] = DurInMinutes;
                                        //CummulativeOfAllLosses += DurInMinutes;
                                    }
                                }

                                Row++;
                                #endregion

                                //individual WO's
                                int hmiID = Convert.ToInt32(HMIDATA.Rows[ji][2]);
                                var HMIDATA1 = obj.Gettbl_multiwoselectionfor1(HmiID);
                                //var HMIDATA1 = db.tbl_multiwoselection.Where(m => m.HMIID == hmiID).ToList();
                                int StartRow = Row;
                                int EndRow = Row;
                                foreach (var row1 in HMIDATA1)
                                {
                                    #region To push to excel. Multi WO.
                                    //worksheet.Cells["H" + Row].Value = UsedDateForExcel.ToString("yyyy-MM-dd");
                                    //worksheet.Cells["I" + Row].Value = row.Shift;
                                    double dimensionResults = 0;
                                    worksheet.Cells["J" + Row].Value = row1.PartNo;
                                    worksheet.Cells["K" + Row].Value = row1.WorkOrder;
                                    worksheet.Cells["L" + Row].Value = row1.OperationNo;
                                    worksheet.Cells["M" + Row].Value = row1.TargetQty;
                                    var MeasuringUnitId1 = obj.GettblpartwiseworkcenterDet3(MachineIDRow);
                                    // var MeasuringUnitId1 = db.tblpartwiseworkcenters.Where(m => m.WorkCenterId == MachineIDRow).FirstOrDefault();

                                    if (MeasuringUnitId != null)
                                    {
                                        dimensionResults = CalculateSurfaceAreaAndPerimeter(row1.PartNo, (int)MeasuringUnitId1.MeasuringUnitId);
                                    }
                                    if (dimensionResults == 0)
                                    {
                                        worksheet.Cells["T" + Row].Value = "";
                                    }
                                    else
                                    {
                                        worksheet.Cells["T" + Row].Value = Math.Round(dimensionResults, 2);
                                    }

                                    delQty = 0;
                                    double.TryParse(Convert.ToString(row1.DeliveredQty), out delQty);
                                    worksheet.Cells["N" + Row].Value = delQty;
                                    worksheet.Cells["U" + Row].Value = Math.Round((delQty * dimensionResults), 2); // total surface Area
                                    if (row1.IsCompleted == 0)
                                    {
                                        worksheet.Cells["O" + Row].Value = "Yes";
                                    }
                                    else
                                    {
                                        worksheet.Cells["P" + Row].Value = "Yes";
                                    }

                                    ColIndex = LossCodesData.Rows.Count + 26 + 3;
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    worksheet.Cells[ColAfterLosses + Row].Value = Convert.ToString(HMIDATA.Rows[ji][3]); //OpName
                                    ColIndex = LossCodesData.Rows.Count + 26 + 4;
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    worksheet.Cells[ColAfterLosses + Row].Value = Convert.ToString(HMIDATA.Rows[ji][18]); //Type
                                    //To skip a Column Just Increment the ColIndex extra +1
                                    ColIndex = LossCodesData.Rows.Count + 26 + 6;
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    worksheet.Cells[ColAfterLosses + Row].Value = row1.PartNo + "#" + row1.OperationNo;
                                    ColIndex = LossCodesData.Rows.Count + 26 + 7;
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    ColForNCCuttingTime = ColAfterLosses;
                                    partNo = row1.PartNo;
                                    opNo = row1.OperationNo;
                                    stdCuttingTime = 0;
                                    string stdCuttingTimeString = Convert.ToString(obj.Gettblmasterparts_st_swDet1(opNo, partNo));
                                    // string stdCuttingTimeString = Convert.ToString(db.tblmasterparts_st_sw.Where(m => m.PartNo == partNo && m.OpNo == opNo).Select(m => m.StdCuttingTime).FirstOrDefault());
                                    double.TryParse(stdCuttingTimeString, out stdCuttingTime);
                                    worksheet.Cells[ColAfterLosses + Row].Value = stdCuttingTime;
                                    ColIndex = LossCodesData.Rows.Count + 26 + 8;
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    //worksheet.Cells[ColAfterLosses + Row].Formula = "=PRODUCT( N" + Row + "," + ColForNCCuttingTime + Row + ")";
                                    //double.TryParse(Convert.ToString(row.Delivered_Qty), out delQty);
                                    worksheet.Cells[ColAfterLosses + Row].Value = delQty * stdCuttingTime;
                                    ColForTotalNCCuttingTime = ColAfterLosses;
                                    ColIndex = LossCodesData.Rows.Count + 26 + 9;
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    //worksheet.Cells[ColAfterLosses + Row].Formula = "=IF((OR((R" + Row + "<=0),(" + ColForTotalNCCuttingTime + Row + " <=0))), 0, R" + Row + "/" + ColForTotalNCCuttingTime + Row + ")";
                                    //TotalNCCutTimeDIVCuttingTime = worksheet.Calculate("=IF((OR((R" + Row + "<=0),(" + ColForTotalNCCuttingTime + Row + " <=0))), 0, R" + Row + "/" + ColForTotalNCCuttingTime + Row + ")");
                                    //worksheet.Cells[ColAfterLosses + Row].Value = Math.Round(Convert.ToDouble(TotalNCCutTimeDIVCuttingTime), 1);

                                    TotalNCCutTimeDIVCuttingTime = worksheet.Calculate("=IF((OR((R" + Row + "<=0),(" + ColForTotalNCCuttingTime + Row + " <=0))), 0," + ColForTotalNCCuttingTime + Row + "/ R" + Row + ")");
                                    Percentage = Math.Round(Convert.ToDouble(TotalNCCutTimeDIVCuttingTime) * 100, 0);
                                    if (Percentage < 100)
                                    {
                                        worksheet.Cells[ColAfterLosses + Row].Value = Percentage;
                                    }
                                    else
                                    {
                                        worksheet.Cells[ColAfterLosses + Row].Value = 100;
                                    }
                                    ColIndex = LossCodesData.Rows.Count + 26 + 10;
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    if (Convert.ToString(row1.SplitWO) == "Yes")
                                    {
                                        worksheet.Cells[ColAfterLosses + Row].Value = "Yes";
                                    }
                                    else
                                    {
                                        worksheet.Cells[ColAfterLosses + Row].Value = "No";
                                    }
                                    Row++;
                                    EndRow = Row;
                                    #endregion
                                }

                                for (int j = StartRow; j < EndRow; j++)
                                {
                                    worksheet.Cells["V" + j].Formula = "=IF(U" + j + " = 0,0,(" + Green + "/SUM(U" + StartRow + ":U" + (EndRow - 1) + "))*U" + j + ")";
                                }
                                #endregion
                            }
                            else
                            {
                                if (Convert.ToInt32(HMIDATA.Rows[ji][28]) == 0) //Its a NormalWC
                                {
                                    #region To push to excel. Single WO. NormalWorkCenter

                                    int MachineIDRow = Convert.ToInt32(HMIDATA.Rows[ji][1]);
                                    double dimensionsResult = 0;
                                    //Getting MeasuringUnitId from tblpartwiseworkcenter 
                                    var MeasuringUnitId = obj.GettblpartwiseworkcenterDet3(MachineIDRow);
                                    string PartNo = (HMIDATA.Rows[ji][6]).ToString();

                                    worksheet.Cells["B" + Row].Value = Sno++;
                                    //worksheet.Cells["C" + Row].Value = UsedDateForExcel.ToString("yyyy-MM-dd");
                                    worksheet.Cells["C" + Row].Value = HierarchyData[0];
                                    worksheet.Cells["D" + Row].Value = HierarchyData[1];
                                    worksheet.Cells["E" + Row].Value = HierarchyData[2];
                                    worksheet.Cells["F" + Row].Value = HierarchyData[3];//Display Name
                                    worksheet.Cells["G" + Row].Value = HierarchyData[3];//Mac INV

                                    worksheet.Cells["H" + Row].Value = UsedDateForExcel.ToString("yyyy-MM-dd");
                                    //worksheet.Cells["I" + Row].Value = UsedDateForExcel.ToString("yyyy-MM-dd");

                                    worksheet.Cells["I" + Row].Value = Convert.ToString(HMIDATA.Rows[ji][4]);// Shift;
                                    worksheet.Cells["J" + Row].Value = Convert.ToString(HMIDATA.Rows[ji][6]);// PartNo;
                                    worksheet.Cells["K" + Row].Value = Convert.ToString(HMIDATA.Rows[ji][7]);// WorkOrderNo;
                                    worksheet.Cells["L" + Row].Value = Convert.ToString(HMIDATA.Rows[ji][8]);// OpNo;
                                    worksheet.Cells["M" + Row].Value = Convert.ToString(HMIDATA.Rows[ji][9]);// TargetQty;
                                    double delQty = Convert.ToDouble(Convert.ToString(HMIDATA.Rows[ji][10])); //DeliveredQty
                                    worksheet.Cells["N" + Row].Value = delQty;
                                    int PF = 0, JF = 0;//PartialFinished
                                    if (Convert.ToInt32(HMIDATA.Rows[ji][11]) == 0) //PF
                                    {
                                        //PF = 1;
                                        worksheet.Cells["O" + Row].Value = "Yes";
                                    }
                                    else
                                    {
                                        //JF = 1;
                                        worksheet.Cells["P" + Row].Value = "Yes";
                                    }

                                    worksheet.Cells["Q" + Row].Value = ""; //IsHold
                                    worksheet.Cells["R" + Row].Value = ""; //Hold Reason
                                    double SettingTime = Convert.ToDouble(HMIDATA.Rows[ji][14]); //SettingTime
                                    worksheet.Cells["S" + Row].Value = Math.Round(SettingTime, 1);

                                    if (dimensionsResult == 0)
                                    {
                                        worksheet.Cells["T" + Row].Value = "";
                                    }
                                    else
                                    {
                                        worksheet.Cells["T" + Row].Value = Math.Round(dimensionsResult, 2);
                                    }
                                    worksheet.Cells["U" + Row].Value = Math.Round((delQty * dimensionsResult), 2); // total surface area

                                    double Green = Convert.ToDouble(HMIDATA.Rows[ji][13]); //cutting
                                    worksheet.Cells["V" + Row].Value = Green;

                                    if (MeasuringUnitId != null)
                                    {
                                        dimensionsResult = CalculateSurfaceAreaAndPerimeter(PartNo, (int)MeasuringUnitId.MeasuringUnitId);

                                        worksheet.Cells["T" + Row].Value = dimensionsResult;

                                        worksheet.Cells["U" + Row].Value = Math.Round((delQty * dimensionsResult), 2); // total surface area

                                        int StartRow = Row;
                                        int EndRow = Row;

                                        for (int j = StartRow; j < EndRow; j++)
                                        {

                                            worksheet.Cells["V" + j].Formula = "=IF(U" + j + " = 0,0,(" + Green + "/SUM(U" + StartRow + ":U" + (EndRow - 1) + "))*U" + j + ")";
                                        }
                                    }
                                    //double Changeover = GetChangeoverTimeForWO(correctedDate, MachineID, woStartTime, woEndTime);
                                    //worksheet.Cells["S" + Row].Value = Math.Round(Changeover / 60, 1); 

                                    double idleTime = Convert.ToDouble(HMIDATA.Rows[ji][16]) - Convert.ToDouble(HMIDATA.Rows[ji][14]); //IDLE - Setting
                                    worksheet.Cells["X" + Row].Value = Math.Round(idleTime, 2);

                                    worksheet.Cells["Y" + Row].Formula = "=SUM(S" + Row + ",V" + Row + ",W" + Row + ",X" + Row + ",AB" + Row + ")";

                                    worksheet.Cells["Z" + Row].Value = "";//Empty finalLossCol 
                                    int column = 26 + LossCodesData.Rows.Count; // StartCol in Excel + TotalLosses - 1
                                    finalLossCol = ExcelColumnFromNumber(column);

                                    worksheet.Cells["AA" + Row].Formula = "=SUM(AB" + Row + ":" + finalLossCol + Row + ")";

                                    double BreakdownDuration = Convert.ToDouble(HMIDATA.Rows[ji][17]); //breakdown
                                    worksheet.Cells["AB" + Row].Value = Math.Round(BreakdownDuration, 1);

                                    double MinorLoss = Convert.ToDouble(HMIDATA.Rows[ji][30]); //minorLoss
                                    worksheet.Cells["AC" + Row].Value = Math.Round(MinorLoss, 1);

                                    //Push Data that is supposed to be after Losses.
                                    ColIndex = LossCodesData.Rows.Count + 26 + 1; //+1 For Gap (& Testing) //26 is Previous DATA(Plant,Shop......)
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    worksheet.Cells[ColAfterLosses + Row].Value = 0;
                                    ColIndex = LossCodesData.Rows.Count + 26 + 2;
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    worksheet.Cells[ColAfterLosses + Row].Value = "0";
                                    ColIndex = LossCodesData.Rows.Count + 26 + 3;
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    worksheet.Cells[ColAfterLosses + Row].Value = HMIDATA.Rows[ji][3];// OperatorName;
                                    ColIndex = LossCodesData.Rows.Count + 26 + 4;
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    worksheet.Cells[ColAfterLosses + Row].Value = HMIDATA.Rows[ji][18];// Type;
                                    //To skip a Column Just Increment the ColIndex extra +1
                                    ColIndex = LossCodesData.Rows.Count + 26 + 6;
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    worksheet.Cells[ColAfterLosses + Row].Value = HMIDATA.Rows[ji][6] + "#" + HMIDATA.Rows[ji][8]; //PartNo OpNo
                                    ColIndex = LossCodesData.Rows.Count + 26 + 7;
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    string ColForNCCuttingTime = ColAfterLosses;
                                    string partNo = Convert.ToString(HMIDATA.Rows[ji][6]);
                                    string opNo = Convert.ToString(HMIDATA.Rows[ji][8]); //OpNo;
                                    double stdCuttingTime = 0;
                                    double ProdOfstdCuttingTimeDelivQty = 0;
                                    int totalWOs = 1;
                                    double TargetNC = 0;

                                    //string stdCuttingTimeString = Convert.ToString(db.tblmasterparts_st_sw.Where(m => m.PartNo == partNo && m.OpNo == opNo).Select(m => m.StdCuttingTime).FirstOrDefault());
                                    //double.TryParse(stdCuttingTimeString, out stdCuttingTime);

                                    stdCuttingTime = Convert.ToDouble(HMIDATA.Rows[ji][19]); //nccutting time per part
                                    ProdOfstdCuttingTimeDelivQty = delQty * stdCuttingTime;

                                    if (Convert.ToInt32(HMIDATA.Rows[ji][11]) == 0) //pf
                                    {
                                        PF = 1;
                                    }
                                    else
                                    {
                                        JF = 1;
                                    }

                                    worksheet.Cells[ColAfterLosses + Row].Value = stdCuttingTime;
                                    ColIndex = LossCodesData.Rows.Count + 26 + 8;
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    //worksheet.Cells[ColAfterLosses + Row].Formula = "=PRODUCT( N" + Row + "," + ColForNCCuttingTime + Row + ")";
                                    //double.TryParse(Convert.ToString(row.Delivered_Qty), out delQty);
                                    worksheet.Cells[ColAfterLosses + Row].Value = ProdOfstdCuttingTimeDelivQty;
                                    string ColForTotalNCCuttingTime = ColAfterLosses;
                                    ColIndex = LossCodesData.Rows.Count + 26 + 9;
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    //worksheet.Cells[ColAfterLosses + Row].Formula = "=IF((OR((R" + Row + "<=0),(" + ColForTotalNCCuttingTime + Row + " <=0))), 0, R" + Row + "/" + ColForTotalNCCuttingTime + Row + ")";
                                    //var TotalNCCutTimeDIVCuttingTime = worksheet.Calculate("=IF((OR((R" + Row + "<=0),(" + ColForTotalNCCuttingTime + Row + " <=0))), 0, R" + Row + "/" + ColForTotalNCCuttingTime + Row + ")");
                                    worksheet.Cells[ColAfterLosses + Row].Formula = "=IF((OR((T" + Row + "<=0),(" + ColForTotalNCCuttingTime + Row + " <=0))), 0," + ColForTotalNCCuttingTime + Row + "/ T" + Row + ")";
                                    var TotalNCCutTimeDIVCuttingTime = worksheet.Calculate("=IF((OR((T" + Row + "<=0),(" + ColForTotalNCCuttingTime + Row + " <=0))), 0," + ColForTotalNCCuttingTime + Row + "/ T" + Row + ")");

                                    double Percentage = Math.Round(Convert.ToDouble(TotalNCCutTimeDIVCuttingTime) * 100, 0);
                                    if (Percentage < 100)
                                    {
                                        worksheet.Cells[ColAfterLosses + Row].Value = Percentage;
                                    }
                                    else
                                    {
                                        worksheet.Cells[ColAfterLosses + Row].Value = 100;
                                    }
                                    ColIndex = LossCodesData.Rows.Count + 26 + 10;
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    if (Convert.ToString(HMIDATA.Rows[ji][31]) == "Yes") //is Split WO
                                    {
                                        worksheet.Cells[ColAfterLosses + Row].Value = "Yes";
                                    }
                                    else
                                    {
                                        worksheet.Cells[ColAfterLosses + Row].Value = "No";
                                    }

                                    //Start Time
                                    ColIndex = LossCodesData.Rows.Count + 26 + 11;
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    worksheet.Cells[ColAfterLosses + Row].Value = HMIDATA.Rows[ji][36].ToString(); //Start Time;
                                                                                                                   //End Time
                                    ColIndex = LossCodesData.Rows.Count + 26 + 12;
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    worksheet.Cells[ColAfterLosses + Row].Value = HMIDATA.Rows[ji][37].ToString(); //End Time;

                                    //bool checkOpCan = obj1.checkOpCancel(Convert.ToString(HMIDATA.Rows[ji][7]), Convert.ToString(HMIDATA.Rows[ji][8]));
                                    //if (checkOpCan)
                                    //{
                                    //    ColIndex = LossCodesData.Rows.Count + 26 + 13;
                                    //    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    //    worksheet.Cells[ColAfterLosses + Row].Value = "YES"; // Operation Cancalation Data Display yes

                                    //}
                                    //else
                                    //{
                                    //    ColIndex = LossCodesData.Rows.Count + 26 + 13;
                                    //    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    //    worksheet.Cells[ColAfterLosses + Row].Value = "NO"; // Operation Cancalation Data Display no
                                    //}

                                    //int qty = obj.Gettblhmidet(Convert.ToInt32(HMIDATA.Rows[ji][2]));
                                    //ColIndex = LossCodesData.Rows.Count + 26 + 14;
                                    //ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    //worksheet.Cells[ColAfterLosses + Row].Value = qty; //Operation cancellation Qty

                                    //int NestingSheetNo = obj.GettblhmidetFornestingmachine(Convert.ToInt32(HMIDATA.Rows[ji][2]));
                                    //ColIndex = LossCodesData.Rows.Count + 26 + 15;
                                    //ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    //worksheet.Cells[ColAfterLosses + Row].Value = NestingSheetNo; //Nesting Sheet No

                                    //worksheet.Cells[ColAfterLosses + Row].Value = Math.Round(Convert.ToDouble(TotalNCCutTimeDIVCuttingTime) * 100, 0);

                                    //Now get & put Losses
                                    // Push Loss Value into  DataTable & Excel
                                    DataRow dr = DTConsolidatedLosses.AsEnumerable().FirstOrDefault(r => r.Field<string>("WCInvNo") == @WCInvNoString && r.Field<string>("CorrectedDate") == dateforMachine);
                                    if (dr != null)
                                    {
                                        //plant, shop, cell, macINV, WcName, CorrectedDate,WOPF,WOProcessed,
                                        //TotalWOQty,TotalTarget,TotalDelivered,TargetNC,TotalValueAdding,TotalLosses,TotalSetUp
                                        DTConsolidatedLosses.Rows.Add(HierarchyData[0], HierarchyData[1], HierarchyData[2], HierarchyData[3],
                                        HierarchyData[3], dateforMachine, Convert.ToString(HMIDATA.Rows[ji][3]), PF, JF, totalWOs, Convert.ToInt32(HMIDATA.Rows[ji][9]), delQty, ProdOfstdCuttingTimeDelivQty, Green, idleTime, SettingTime);
                                    }
                                    else
                                    {
                                        //plant, shop, cell, macINV, WcName, CorrectedDate,WOPF,WOProcessed,
                                        //TotalWOQty,TotalTarget,TotalDelivered,TargetNC,TotalValueAdding,TotalLosses,TotalSetUp
                                        DTConsolidatedLosses.Rows.Add(HierarchyData[0], HierarchyData[1], HierarchyData[2], HierarchyData[3],
                                        HierarchyData[3], dateforMachine, Convert.ToString(HMIDATA.Rows[ji][3]), PF, JF, totalWOs, Convert.ToInt32(HMIDATA.Rows[ji][9]), delQty, ProdOfstdCuttingTimeDelivQty, Green, idleTime, SettingTime);
                                    }

                                    #region Capture and Push Losses

                                    //now push 0 for every other loss into excel
                                    worksheet.Cells["AA" + Row + ":" + finalLossCol + Row].Value = Convert.ToDouble(0.0);

                                    //to Capture and Push , Losses that occured.
                                    //List<KeyValuePair<int, double>> LossesdurationList = GetAllLossesDurationSecondsForWO(MachineID, correctedDate, woStartTime, woEndTime);
                                    int HmiID = Convert.ToInt32(HMIDATA.Rows[ji][2]);
                                    var LossesdurationList = obj.Gettblwolossessfor1(HmiID);
                                    DataRow dr1 = DTConsolidatedLosses.AsEnumerable().FirstOrDefault(r => r.Field<string>("WCInvNo") == WCInvNoString && r.Field<string>("CorrectedDate") == @dateforMachine);
                                    if (dr1 != null)
                                    {
                                        foreach (var loss in LossesdurationList)
                                        {
                                            int LossID = Convert.ToInt32(loss.LossID);
                                            if (LossID == 112)
                                            {
                                            }
                                            double Duration = Convert.ToDouble(loss.LossDuration);
                                            int level = Convert.ToInt32(loss.Level);
                                            string losscodeName = loss.LossName;

                                            #region To Get LossCode Hierarchy
                                            if (level == 3)
                                            {
                                                losscodeName = loss.LossCodeLevel1Name + " :: " + loss.LossCodeLevel2Name + " : " + losscodeName;
                                            }
                                            else if (level == 2)
                                            {
                                                losscodeName = loss.LossCodeLevel1Name + ":" + losscodeName;
                                            }
                                            else if (level == 1)
                                            {
                                                if (LossID == 999)
                                                {
                                                    losscodeName = "NoCode Entered";
                                                }
                                                else
                                                {
                                                    losscodeName = losscodeName;
                                                }
                                            }
                                            #endregion

                                            //int ColumnIndex = DTConsolidatedLosses.Columns[losscodeName].Ordinal;
                                            ///*string ColumnForThisLoss = ExcelColumnFromNumber(ColumnIndex + 11);*/// 5 is the Difference between position of Excel and DataTable Structure  for Losses Inserting column.   //Commented by monika because No Code value was updated in to PM
                                            //string ColumnForThisLoss = ExcelColumnFromNumber(ColumnIndex + 12);
                                            //double DurInMinutes = Convert.ToDouble(Math.Round((Duration), 1)); //To Minutes:: 1 Decimal Place
                                            //worksheet.Cells[ColumnForThisLoss + "" + Row].Value = DurInMinutes;
                                            //dr1[losscodeName] = DurInMinutes;
                                            //CummulativeOfAllLosses += DurInMinutes;

                                            int ColumnIndex = 0;
                                            try
                                            {
                                                ColumnIndex = DTConsolidatedLosses.Columns[losscodeName].Ordinal;
                                            }
                                            catch (Exception ex)
                                            {
                                                ColumnIndex = 0;
                                            }

                                            if (ColumnIndex != 0)
                                            {
                                                string ColumnForThisLoss = ExcelColumnFromNumber(ColumnIndex + 12);
                                                double DurInMinutes = Convert.ToDouble(Math.Round((Duration), 1)); //To Minutes:: 1 Decimal Place
                                                worksheet.Cells[ColumnForThisLoss + "" + Row].Value = DurInMinutes;
                                                dr1[losscodeName] = DurInMinutes;
                                                CummulativeOfAllLosses += DurInMinutes;
                                            }
                                        }
                                    }
                                    Row++;
                                    #endregion

                                    #endregion
                                }
                                else if (Convert.ToInt32(HMIDATA.Rows[ji][28]) == 1) //Its a ManualWC
                                {
                                    #region To push to excel. Single WO. NormalWorkCenter

                                    int MachineIDRow = Convert.ToInt32(HMIDATA.Rows[ji][1]);
                                    double dimensionsResult = 0;
                                    //Getting MeasuringUnitId from tblpartwiseworkcenter 
                                    var MeasuringUnitId = obj.GettblpartwiseworkcenterDet3(MachineIDRow);
                                    string PartNo = (HMIDATA.Rows[ji][6]).ToString();

                                    worksheet.Cells["B" + Row].Value = Sno++;
                                    //worksheet.Cells["C" + Row].Value = UsedDateForExcel.ToString("yyyy-MM-dd");
                                    worksheet.Cells["C" + Row].Value = HierarchyData[0];
                                    worksheet.Cells["D" + Row].Value = HierarchyData[1];
                                    worksheet.Cells["E" + Row].Value = HierarchyData[2];
                                    worksheet.Cells["G" + Row].Value = HierarchyData[3];//Mac INV
                                    worksheet.Cells["F" + Row].Value = HierarchyData[3];//Display Name
                                    // worksheet.Cells["G" + Row].Value = HierarchyData[3];//Mac INV

                                    worksheet.Cells["H" + Row].Value = UsedDateForExcel.ToString("yyyy-MM-dd");
                                    //worksheet.Cells["I" + Row].Value = UsedDateForExcel.ToString("yyyy-MM-dd");

                                    worksheet.Cells["I" + Row].Value = Convert.ToString(HMIDATA.Rows[ji][4]);// Shift;
                                    worksheet.Cells["J" + Row].Value = Convert.ToString(HMIDATA.Rows[ji][6]);// PartNo;
                                    worksheet.Cells["K" + Row].Value = Convert.ToString(HMIDATA.Rows[ji][7]);// WorkOrderNo;
                                    worksheet.Cells["L" + Row].Value = Convert.ToString(HMIDATA.Rows[ji][8]);// OpNo;
                                    worksheet.Cells["M" + Row].Value = Convert.ToString(HMIDATA.Rows[ji][9]);// TargetQty;
                                    double delQty = Convert.ToDouble(Convert.ToString(HMIDATA.Rows[ji][10])); //DeliveredQty
                                    worksheet.Cells["N" + Row].Value = delQty;
                                    int PF = 0, JF = 0;//PartialFinished
                                    if (Convert.ToInt32(HMIDATA.Rows[ji][11]) == 0) //pf
                                    {
                                        //PF = 1;
                                        worksheet.Cells["O" + Row].Value = "Yes";
                                    }
                                    else
                                    {
                                        //JF = 1;
                                        worksheet.Cells["P" + Row].Value = "Yes";
                                    }

                                    int HoldCodeID = 0;
                                    string HoldcodeIDString = Convert.ToString(HMIDATA.Rows[ji][29]);// HoldReason;
                                    int.TryParse(HoldcodeIDString, out HoldCodeID);
                                    if (HoldCodeID != 0)
                                    {
                                        worksheet.Cells["Q" + Row].Value = "Yes"; //IsHold
                                        worksheet.Cells["R" + Row].Value = GetHoldHierarchy(HoldCodeID); //Hold Reason
                                    }
                                    double SettingTime = Convert.ToDouble(HMIDATA.Rows[ji][14]); //setting
                                    worksheet.Cells["S" + Row].Value = Math.Round(SettingTime, 1);

                                    double Green = Convert.ToDouble(HMIDATA.Rows[ji][13]);
                                    worksheet.Cells["V" + Row].Value = Green;
                                    if (MeasuringUnitId != null)
                                    {
                                        dimensionsResult = CalculateSurfaceAreaAndPerimeter(PartNo, (int)MeasuringUnitId.MeasuringUnitId);
                                        worksheet.Cells["T" + Row].Value = dimensionsResult;
                                        worksheet.Cells["U" + Row].Value = Math.Round((delQty * dimensionsResult), 2); // total surface area

                                        #region cuttingTime
                                        int numrow = 0;


                                        //if (ji == (numberofRows - 5))
                                        //{
                                        //    k = 1;
                                        //}
                                        // if(ji==0)
                                        //{
                                        //    numberofRows++;
                                        //}

                                        DateTime PreviousRowStartDate = DateTime.Now;
                                        DateTime PreviousRowEndDate = DateTime.Now;
                                        DateTime CurrentRowStartDate = DateTime.Now;
                                        DateTime CurrentRowEndDate = DateTime.Now;

                                        if (ji != 0)
                                        {

                                            PreviousRowStartDate = Convert.ToDateTime(Convert.ToDateTime(HMIDATA.Rows[ji - 1][36]).ToString("yyyy-MM-dd HH:mm:00"));
                                            PreviousRowEndDate = Convert.ToDateTime(Convert.ToDateTime(HMIDATA.Rows[ji - 1][37]).ToString("yyyy-MM-dd HH:mm:00"));
                                            CurrentRowStartDate = Convert.ToDateTime(Convert.ToDateTime(HMIDATA.Rows[ji][36]).ToString("yyyy-MM-dd HH:mm:00"));
                                            CurrentRowEndDate = Convert.ToDateTime(Convert.ToDateTime(HMIDATA.Rows[ji][37]).ToString("yyyy-MM-dd HH:mm:00"));

                                            // PrevOpeatorName = HMIDATA.Rows[ji-1][3].ToString();
                                            CurrOpeatorName = HMIDATA.Rows[ji][3].ToString();

                                            var HMIDATAList = HMIDATA.AsEnumerable().Where(m => m["OperatorName"].ToString().Trim() == CurrOpeatorName).ToList();

                                            if (CurrOpeatorName == HMIDATA.Rows[ji - 1][3].ToString() && jk == 0)
                                                BatchList = GetBatchForManual(HMIDATAList, CurrOpeatorName);
                                            else if (CurrOpeatorName != HMIDATA.Rows[ji - 1][3].ToString())
                                            {
                                                BatchList = GetBatchForManual(HMIDATAList, CurrOpeatorName);
                                                jk += 1;
                                            }
                                            //foreach (var batch in BatchList)
                                            //{
                                            if (BatchList.Any(m => m.StartTime == Convert.ToDateTime(HMIDATA.Rows[ji][36])))   // if (batch.StartTime == Convert.ToDateTime(HMIDATA.Rows[ji][36]))
                                            {
                                                var DistinctBatchIDs = BatchList.Select(m => m.BatchID).Distinct().ToList();
                                                int l = 0;
                                                foreach (int Batchid in DistinctBatchIDs)
                                                {
                                                    var BatchDet = BatchList.Where(m => m.BatchID == Batchid && m.Iscompleted == false).ToList();
                                                    DateTime MinST = BatchDet.Min(m => m.StartTime);
                                                    DateTime MaxET = BatchDet.Max(m => m.EndTime);
                                                    BatchGreen = MaxET.Subtract(MinST).TotalMinutes;
                                                    if (k == 0)
                                                    {
                                                        numberofcount = BatchDet.Count;
                                                        k += 1;
                                                        jk += 1;
                                                    }
                                                    if (ROWStart == 5)
                                                    {
                                                        StartRows = Row - 1;

                                                    }
                                                    else
                                                    {
                                                        StartRows = Row;
                                                        ROWStart = 0;

                                                    }
                                                    //numberOFRowsBatch = BatchDet.Count;


                                                    if (StartRows != 0 && (StartRows != 0 || ji == HMIDATA.Rows.Count))
                                                    {
                                                        EndRows = StartRows + (numberofcount);
                                                        //EndRows = numrow - 1;
                                                        //BatchGreen += Convert.ToDouble(HMIDATA.Rows[ji - 1][13]);
                                                    }

                                                    if (StartRows != 0 && EndRows != 0)
                                                    {
                                                        for (int j = StartRows; j <= EndRows; j++)
                                                        {
                                                            worksheet.Cells["V" + j].Formula = "=IF(U" + j + " = 0,0,(" + BatchGreen + "/SUM(U" + StartRows + ":U" + EndRows + "))*U" + j + ")";
                                                        }

                                                        numberofcount -= 1;
                                                        if (numberofcount != 0)
                                                        {
                                                            StartRows = 0;
                                                            EndRows = 0;
                                                            BatchGreen = 0;
                                                        }


                                                    }
                                                    if (numberofcount == 0)
                                                    {
                                                        for (int j = EndRows; j <= EndRows; j++)
                                                        {
                                                            worksheet.Cells["V" + j].Formula = "=IF(U" + j + " = 0,0,(" + BatchGreen + "/SUM(U" + j + ":U" + EndRows + "))*U" + j + ")";
                                                        }
                                                        foreach (var row in BatchDet)
                                                        {
                                                            //row.Iscompleted = true;
                                                            BatchList.Remove(row);
                                                        }
                                                        l += 1;
                                                        k = 0;

                                                        // continue;
                                                    }
                                                    if (l == 1)
                                                    {
                                                        break;
                                                        //  continue;
                                                    }
                                                    else
                                                    {
                                                        l = 0;
                                                        break;

                                                    }
                                                }
                                            }
                                            else
                                            {
                                                ROWStart = Row;
                                                ROWStart += 1;
                                            }
                                            //else
                                            //    break;
                                            //}

                                            //if (StartRows != 0 && (StartRows != 0 || ji == HMIDATA.Rows.Count))
                                            //{
                                            //    EndRows = StartRows + (numberOFRowsBatch - 1);
                                            //    //EndRows = numrow - 1;
                                            //    //BatchGreen += Convert.ToDouble(HMIDATA.Rows[ji - 1][13]);
                                            //}

                                            //if (StartRows != 0 && EndRows != 0)
                                            //{
                                            //    for (int j = StartRows; j <= EndRows; j++)
                                            //    {
                                            //        worksheet.Cells["V" + j].Formula = "=IF(U" + j + " = 0,0,(" + BatchGreen + "/SUM(U" + StartRows + ":U" + EndRows + "))*U" + j + ")";
                                            //    }
                                            //    StartRows = 0;
                                            //    EndRows = 0;
                                            //    BatchGreen = 0;
                                            //}

                                            //if (k == 0)
                                            //{
                                            //    numberOFRowsBatch = HMIDATA.AsEnumerable().Where(m => Convert.ToDateTime(m["StartTime"]).ToString("yyyy-MM-dd HH:mm:00") == CurrentRowStartDate.ToString()).ToList().Count;
                                            //}
                                            //numberofRows=HMIDATA.AsEnumerable().Sum(m=>((m.Table.Rows[ji-1]["StartTime"])

                                            //    if (k == 1)
                                            //    {
                                            //        numberofRows = HMIDATA.AsEnumerable().Where(m => Convert.ToDateTime(m["StartTime"]).ToString("yyyy-MM-dd HH:mm:00") == CurrentRowStartDate.ToString()).ToList().Count;
                                            //        k = Row-1;
                                            //        numrow = numberofRows;
                                            //        numberofRows++;
                                            //    }

                                            //    if (CurrentRowStartDate == PreviousRowStartDate) //&& CurrentRowEndDate == PreviousRowEndDate
                                            //    {
                                            //        if (StartRows == 0)
                                            //        {
                                            //            StartRows = Row - 1;
                                            //        }
                                            //        BatchGreen += Convert.ToDouble(HMIDATA.Rows[ji - 1][13]);
                                            //    }
                                            //if (StartRows != 0 && (StartRows != 0 || ji == HMIDATA.Rows.Count))
                                            //{
                                            //    numrow += (Row - 1);
                                            //    EndRows = numrow - 1;
                                            //    //BatchGreen += Convert.ToDouble(HMIDATA.Rows[ji - 1][13]);
                                            //}

                                            //if (StartRows != 0 && EndRows != 0)
                                            //{
                                            //    for (int j = StartRows; j <= EndRows; j++)
                                            //    {
                                            //        worksheet.Cells["V" + j].Formula = "=IF(U" + j + " = 0,0,(" + BatchGreen + "/SUM(U" + StartRows + ":U" + EndRows + "))*U" + j + ")";
                                            //    }
                                            //    StartRows = 0;
                                            //    EndRows = 0;
                                            //    BatchGreen = 0;
                                            //}
                                        }

                                    }
                                    #endregion
                                    //double Changeover = GetChangeoverTimeForWO(correctedDate, MachineID, woStartTime, woEndTime);
                                    //worksheet.Cells["S" + Row].Value = Math.Round(Changeover / 60, 1);

                                    double idleTime = Convert.ToDouble(HMIDATA.Rows[ji][16]) - Convert.ToDouble(HMIDATA.Rows[ji][14]); //idle-setting
                                    worksheet.Cells["X" + Row].Value = Math.Round(idleTime, 2);

                                    worksheet.Cells["Y" + Row].Formula = "=SUM(S" + Row + ",V" + Row + ",W" + Row + ",X" + Row + ",AB" + Row + ")";

                                    worksheet.Cells["Z" + Row].Value = "";//Empty finalLossCol
                                    int column = 26 + LossCodesData.Rows.Count; // StartCol in Excel + TotalLosses - 1
                                    finalLossCol = ExcelColumnFromNumber(column);

                                    worksheet.Cells["AA" + Row].Formula = "=SUM(AB" + Row + ":" + finalLossCol + Row + ")";

                                    double BreakdownDuration = Convert.ToDouble(HMIDATA.Rows[ji][17]); //breakdown
                                    worksheet.Cells["AB" + Row].Value = Math.Round(BreakdownDuration, 1);

                                    double MinorLoss = Convert.ToDouble(HMIDATA.Rows[ji][30]); //minorLoss
                                    worksheet.Cells["AC" + Row].Value = Math.Round(MinorLoss, 1);

                                    //Push Data that is supposed to be after Losses.
                                    ColIndex = LossCodesData.Rows.Count + 26 + 1; //+1 For Gap (& Testing) //26 is Previous DATA(Plant,Shop......)
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    worksheet.Cells[ColAfterLosses + Row].Value = 0;
                                    ColIndex = LossCodesData.Rows.Count + 26 + 2;
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    worksheet.Cells[ColAfterLosses + Row].Value = "0";
                                    ColIndex = LossCodesData.Rows.Count + 26 + 3;
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    worksheet.Cells[ColAfterLosses + Row].Value = HMIDATA.Rows[ji][3];// OperatorName;
                                    ColIndex = LossCodesData.Rows.Count + 26 + 4;
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    worksheet.Cells[ColAfterLosses + Row].Value = HMIDATA.Rows[ji][18]; //Type
                                    //To skip a Column Just Increment the ColIndex extra +1
                                    ColIndex = LossCodesData.Rows.Count + 26 + 6;
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    worksheet.Cells[ColAfterLosses + Row].Value = HMIDATA.Rows[ji][6] + "#" + HMIDATA.Rows[ji][8];
                                    ColIndex = LossCodesData.Rows.Count + 26 + 7;
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    string ColForNCCuttingTime = ColAfterLosses;
                                    string partNo = Convert.ToString(HMIDATA.Rows[ji][6]);
                                    string opNo = Convert.ToString(HMIDATA.Rows[ji][8]);
                                    double stdCuttingTime = 0;
                                    double ProdOfstdCuttingTimeDelivQty = 0;
                                    int totalWOs = 1;
                                    double TargetNC = 0;

                                    //string stdCuttingTimeString = Convert.ToString(db.tblmasterparts_st_sw.Where(m => m.PartNo == partNo && m.OpNo == opNo).Select(m => m.StdCuttingTime).FirstOrDefault());
                                    //double.TryParse(stdCuttingTimeString, out stdCuttingTime);

                                    stdCuttingTime = Convert.ToDouble(HMIDATA.Rows[ji][19]); //std cutting time per part
                                    ProdOfstdCuttingTimeDelivQty = delQty * stdCuttingTime;

                                    if (Convert.ToInt32(HMIDATA.Rows[ji][11]) == 0) //PF
                                    {
                                        PF = 1;
                                    }
                                    else
                                    {
                                        JF = 1;
                                    }

                                    worksheet.Cells[ColAfterLosses + Row].Value = stdCuttingTime;
                                    ColIndex = LossCodesData.Rows.Count + 26 + 8;
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    //worksheet.Cells[ColAfterLosses + Row].Formula = "=PRODUCT( N" + Row + "," + ColForNCCuttingTime + Row + ")";
                                    //double.TryParse(Convert.ToString(row.Delivered_Qty), out delQty);
                                    worksheet.Cells[ColAfterLosses + Row].Value = ProdOfstdCuttingTimeDelivQty;
                                    string ColForTotalNCCuttingTime = ColAfterLosses;
                                    ColIndex = LossCodesData.Rows.Count + 26 + 9;
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    //worksheet.Cells[ColAfterLosses + Row].Formula = "=IF((OR((R" + Row + "<=0),(" + ColForTotalNCCuttingTime + Row + " <=0))), 0, R" + Row + "/" + ColForTotalNCCuttingTime + Row + ")";
                                    //var TotalNCCutTimeDIVCuttingTime = worksheet.Calculate("=IF((OR((R" + Row + "<=0),(" + ColForTotalNCCuttingTime + Row + " <=0))), 0, R" + Row + "/" + ColForTotalNCCuttingTime + Row + ")");
                                    worksheet.Cells[ColAfterLosses + Row].Formula = "=IF((OR((T" + Row + "<=0),(" + ColForTotalNCCuttingTime + Row + " <=0))), 0," + ColForTotalNCCuttingTime + Row + "/ T" + Row + ")";
                                    var TotalNCCutTimeDIVCuttingTime = worksheet.Calculate("=IF((OR((T" + Row + "<=0),(" + ColForTotalNCCuttingTime + Row + " <=0))), 0," + ColForTotalNCCuttingTime + Row + "/ T" + Row + ")");

                                    double Percentage = Math.Round(Convert.ToDouble(TotalNCCutTimeDIVCuttingTime) * 100, 0);
                                    if (Percentage < 100)
                                    {
                                        worksheet.Cells[ColAfterLosses + Row].Value = Percentage;
                                    }
                                    else
                                    {
                                        worksheet.Cells[ColAfterLosses + Row].Value = 100;
                                    }

                                    ColIndex = LossCodesData.Rows.Count + 26 + 10;
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    if (Convert.ToString(HMIDATA.Rows[ji][31]) == "Yes") //splitwo
                                    {
                                        worksheet.Cells[ColAfterLosses + Row].Value = "Yes";
                                    }
                                    else
                                    {
                                        worksheet.Cells[ColAfterLosses + Row].Value = "No";
                                    }

                                    //Start Time
                                    ColIndex = LossCodesData.Rows.Count + 26 + 11;
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    worksheet.Cells[ColAfterLosses + Row].Value = HMIDATA.Rows[ji][36].ToString(); //Start Time;
                                                                                                                   //End Time
                                    ColIndex = LossCodesData.Rows.Count + 26 + 12;
                                    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    worksheet.Cells[ColAfterLosses + Row].Value = HMIDATA.Rows[ji][37].ToString(); //End Time

                                    //bool checkOpCan = obj1.checkOpCancel(Convert.ToString(HMIDATA.Rows[ji][7]), Convert.ToString(HMIDATA.Rows[ji][8]));
                                    //if (checkOpCan)
                                    //{
                                    //    ColIndex = LossCodesData.Rows.Count + 26 + 13;
                                    //    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    //    worksheet.Cells[ColAfterLosses + Row].Value = "YES"; // Operation Cancalation Data Display yes

                                    //}
                                    //else
                                    //{
                                    //    ColIndex = LossCodesData.Rows.Count + 26 + 13;
                                    //    ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    //    worksheet.Cells[ColAfterLosses + Row].Value = "NO"; // Operation Cancalation Data Display no
                                    //}

                                    //int qty = obj.Gettblhmidet(Convert.ToInt32(HMIDATA.Rows[ji][2]));
                                    //ColIndex = LossCodesData.Rows.Count + 26 + 14;
                                    //ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    //worksheet.Cells[ColAfterLosses + Row].Value = qty; //Operation cancellation Qty


                                    //int NestingSheetNo = obj.GettblhmidetFornestingmachine(Convert.ToInt32(HMIDATA.Rows[ji][2]));
                                    //ColIndex = LossCodesData.Rows.Count + 26 + 15;
                                    //ColAfterLosses = ExcelColumnFromNumber(ColIndex);
                                    //worksheet.Cells[ColAfterLosses + Row].Value = NestingSheetNo; //Nesting Sheet No

                                    //worksheet.Cells[ColAfterLosses + Row].Value = Math.Round(Convert.ToDouble(TotalNCCutTimeDIVCuttingTime) * 100, 0);

                                    //Now get & put Losses
                                    // Push Loss Value into  DataTable & Excel
                                    DataRow dr = DTConsolidatedLosses.AsEnumerable().FirstOrDefault(r => r.Field<string>("WCInvNo") == @WCInvNoString && r.Field<string>("CorrectedDate") == dateforMachine);
                                    if (dr != null)
                                    {
                                        //plant, shop, cell, macINV, WcName, CorrectedDate,WOPF,WOProcessed,
                                        //TotalWOQty,TotalTarget,TotalDelivered,TargetNC,TotalValueAdding,TotalLosses,TotalSetUp
                                        DTConsolidatedLosses.Rows.Add(HierarchyData[0], HierarchyData[1], HierarchyData[2], HierarchyData[3],
                                        HierarchyData[3], dateforMachine, HMIDATA.Rows[ji][3], PF, JF, totalWOs, Convert.ToInt32(HMIDATA.Rows[ji][9]), delQty, ProdOfstdCuttingTimeDelivQty, Green, idleTime, SettingTime);
                                    }
                                    else
                                    {
                                        //plant, shop, cell, macINV, WcName, CorrectedDate,WOPF,WOProcessed,
                                        //TotalWOQty,TotalTarget,TotalDelivered,TargetNC,TotalValueAdding,TotalLosses,TotalSetUp
                                        DTConsolidatedLosses.Rows.Add(HierarchyData[0], HierarchyData[1], HierarchyData[2], HierarchyData[3],
                                        HierarchyData[3], dateforMachine, HMIDATA.Rows[ji][3], PF, JF, totalWOs, Convert.ToInt32(HMIDATA.Rows[ji][9]), delQty, ProdOfstdCuttingTimeDelivQty, Green, idleTime, SettingTime);
                                    }

                                    #region Capture and Push Losses

                                    //now push 0 for every other loss into excel
                                    worksheet.Cells["AA" + Row + ":" + finalLossCol + Row].Value = Convert.ToDouble(0.0);

                                    //to Capture and Push , Losses that occured.
                                    //List<KeyValuePair<int, double>> LossesdurationList = GetAllLossesDurationSecondsForWO(MachineID, correctedDate, woStartTime, woEndTime);
                                    int HmiID = Convert.ToInt32(HMIDATA.Rows[ji][2]);
                                    var LossesdurationList = obj.Gettblwolossessfor1(HmiID);
                                    DataRow dr1 = DTConsolidatedLosses.AsEnumerable().FirstOrDefault(r => r.Field<string>("WCInvNo") == WCInvNoString && r.Field<string>("CorrectedDate") == @dateforMachine);
                                    if (dr1 != null)
                                    {
                                        foreach (var loss in LossesdurationList)
                                        {
                                            int LossID = Convert.ToInt32(loss.LossID);
                                            if (LossID == 112)
                                            {
                                            }
                                            double Duration = Convert.ToDouble(loss.LossDuration);
                                            int level = Convert.ToInt32(loss.Level);
                                            string losscodeName = loss.LossName;

                                            #region To Get LossCode Hierarchy
                                            if (level == 3)
                                            {
                                                if (losscodeName == "Preset")
                                                    losscodeName = "Offset/Preset";
                                                losscodeName = loss.LossCodeLevel1Name + " :: " + loss.LossCodeLevel2Name + " : " + losscodeName;
                                            }
                                            else if (level == 2)
                                            {
                                                losscodeName = loss.LossCodeLevel1Name + ":" + losscodeName;
                                            }
                                            else if (level == 1)
                                            {
                                                if (LossID == 999)
                                                {
                                                    losscodeName = "NoCode Entered";
                                                }
                                                else
                                                {
                                                    losscodeName = losscodeName;
                                                }
                                            }
                                            #endregion
                                            int ColumnIndex = 0;
                                            try
                                            {
                                                ColumnIndex = DTConsolidatedLosses.Columns[losscodeName].Ordinal;
                                            }
                                            catch (Exception ex)
                                            {
                                                ColumnIndex = 0;
                                            }

                                            if (ColumnIndex != 0)
                                            {
                                                string ColumnForThisLoss = ExcelColumnFromNumber(ColumnIndex + 12);// 5 is the Difference between position of Excel and DataTable Structure  for Losses Inserting column.
                                                double DurInMinutes = Convert.ToDouble(Math.Round((Duration), 1)); //To Minutes:: 1 Decimal Place
                                                worksheet.Cells[ColumnForThisLoss + "" + Row].Value = DurInMinutes;
                                                dr1[losscodeName] = DurInMinutes;
                                                CummulativeOfAllLosses += DurInMinutes;
                                            }
                                            //    int ColumnIndex = DTConsolidatedLosses.Columns[losscodeName].Ordinal;
                                            //string ColumnForThisLoss = ExcelColumnFromNumber(ColumnIndex + 12);// 5 is the Difference between position of Excel and DataTable Structure  for Losses Inserting column.
                                            //double DurInMinutes = Convert.ToDouble(Math.Round((Duration), 1)); //To Minutes:: 1 Decimal Place
                                            //worksheet.Cells[ColumnForThisLoss + "" + Row].Value = DurInMinutes;
                                            //dr1[losscodeName] = DurInMinutes;
                                            //CummulativeOfAllLosses += DurInMinutes;
                                        }
                                    }
                                    Row++;

                                    #endregion

                                    #endregion
                                }
                            }

                        }//end of 1 HMIDATA Row

                    }
                    #endregion
                    //worksheet.Cells["W" + Row].Value = Convert.ToDouble(Math.Round((CummulativeOfAllLosses), 1));
                    // Row++;
                }//End of For Each Machine Loop

                if (StartingRowForToday != Row)
                {
                    //Stuff for entire day (of all WC's) Into DT
                    DTConsolidatedLosses.Rows.Add("Summarized", "Summarized", "Summarized", "Summarized", "Summarized", dateforMachine);

                    //Push each Date Cummulative. Loop through ExcelAddress and insert formula
                    var rangeIndividualSummarized = worksheet.Cells["AA4:" + finalLossCol + "4"];
                    foreach (var rangeBase in rangeIndividualSummarized)
                    {
                        string str = Convert.ToString(rangeBase);
                        string ExcelColAlphabet = Regex.Replace(str, "[^A-Z _]", "");
                        worksheet.Cells[ExcelColAlphabet + Row].Formula = "=SUM(" + ExcelColAlphabet + StartingRowForToday + ":" + ExcelColAlphabet + "" + (Row - 1) + ")";
                        //var a = worksheet.Cells[rangeBase.Address].Value;
                        var blah1 = worksheet.Calculate("=SUM(" + ExcelColAlphabet + StartingRowForToday + ":" + ExcelColAlphabet + "" + (Row - 1) + ")");
                        double LossVal = 0;
                        double.TryParse(Convert.ToString(blah1), out LossVal);
                        if (LossVal != 0.0)
                        {
                            string LossName = Convert.ToString(worksheet.Cells[ExcelColAlphabet + 4].Value);
                            DataRow dr = DTConsolidatedLosses.AsEnumerable().LastOrDefault(r => r.Field<string>("Plant") == "Summarized" && r.Field<string>("CorrectedDate") == dateforMachine);
                            if (dr != null)
                            {
                                dr[LossName] = LossVal;
                            }
                        }
                    }

                    //Total of Today into 
                    //Insert Cummulative for today + 9 cols extra
                    if (Row > StartingRowForToday)
                    {
                        int col = 26 + LossCodesData.Rows.Count + 15; // StartCol of Losses + AllLosses + 13 AfterLossColumns
                        finalLossCol = ExcelColumnFromNumber(col);

                        worksheet.Cells["C" + Row + ":G" + Row].Merge = true;
                        worksheet.Cells["C" + Row].Value = "Summarized For";
                        worksheet.Cells["H" + Row].Value = UsedDateForExcel.ToString("yyyy-MM-dd"); //For Date
                        worksheet.Cells["B" + Row + ":" + finalLossCol + Row].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
                        worksheet.Cells["B" + Row + ":" + finalLossCol + Row].Style.Font.Bold = true;

                        worksheet.Cells["S" + Row].Formula = "=SUM(S" + StartingRowForToday + ":S" + (Row - 1) + ")";
                        worksheet.Cells["T" + Row].Formula = "=SUM(T" + StartingRowForToday + ":T" + (Row - 1) + ")";
                        worksheet.Cells["V" + Row].Formula = "=SUM(V" + StartingRowForToday + ":V" + (Row - 1) + ")";
                        worksheet.Cells["W" + Row].Formula = "=SUM(W" + StartingRowForToday + ":W" + (Row - 1) + ")";
                        worksheet.Cells["Y" + Row].Formula = "=SUM(Y" + StartingRowForToday + ":Y" + (Row - 1) + ")";
                        worksheet.Cells["Z" + Row].Formula = "=SUM(Z" + StartingRowForToday + ":Z" + (Row - 1) + ")";

                        //Cellwise Border for Today
                        worksheet.Cells["B" + StartingRowForToday + ":" + finalLossCol + "" + Row].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells["B" + StartingRowForToday + ":" + finalLossCol + "" + Row].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells["B" + StartingRowForToday + ":" + finalLossCol + "" + Row].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells["B" + StartingRowForToday + ":" + finalLossCol + "" + Row].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        //Excel:: Border Around Cells.
                        worksheet.Cells["B" + StartingRowForToday + ":" + finalLossCol + "" + Row].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
                        worksheet.Cells["B" + Row + ":" + finalLossCol + "" + Row].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
                    }
                    Row++;
                }

                UsedDateForExcel = UsedDateForExcel.AddDays(+1);
                //Row++;

            } //End of All day's Loop
            //Summarized Data (You have it all in DataTable: "Plant = 'Non-Summarized'" rows)
            Row = 5;
            Sno = 1;

            for (int n = 0; n < machin.Rows.Count; n++)
            {
                int MachineID = Convert.ToInt32(machin.Rows[n][0]);
                List<string> HierarchyData = GetHierarchyData(MachineID);

                //get distinct opratornames for this mac & Date

                //List<string> distinctOpNames = (from row in DTConsolidatedLosses.AsEnumerable()
                //                           where row.Field<string>("WCInvNo") == HierarchyData[3][3] && row.Field<string>("CorrectedDate") == UsedDateForExcel.ToString("yyyy-MM-dd")
                //                           select(row.Field<string>("OpName")).Distinct()).ToList();

                var MacInv = HierarchyData[3];
                var idColumn = "OpName";
                // var distinctOpNames1 = DTConsolidatedLosses.DefaultView.ToTable(true, new String[] { "OpName" });
                //var distinctOpNames1 = DTConsolidatedLosses.DefaultView.ToTable(true, idColumn)
                //    .Rows
                //    .Cast<DataRow>()
                //    //.Where(row.WCInvNo == HierarchyData[3])
                //    .Select(row => row[idColumn]) //row => row[MacInv] == HierarchyData[3]
                //    .Distinct()
                //    .ToList()
                //    ;

                //var result = (from row in DTConsolidatedLosses.AsEnumerable()
                //              where row.Field<string>("WCInvNo") == HierarchyData[3]
                //              select row);

                var distinctValues = DTConsolidatedLosses.AsEnumerable()
                    .Where(row => row.Field<string>("WCInvNo") == HierarchyData[3])
                        .Select(row => new
                        {
                            opname = row.Field<string>("OpName"),
                        })
                        .Distinct();


                foreach (var row in distinctValues)
                {
                    //if (row != "{}")
                    //if(( Regex.Replace(Convert.ToString(row),"[^a-zA-Z0-9]+","")).Trim().Length > 0)
                    if (true)
                    {

                        worksheetGraph.Cells["B" + Row].Value = Sno++;
                        worksheetGraph.Cells["C" + Row].Value = HierarchyData[0];
                        worksheetGraph.Cells["D" + Row].Value = HierarchyData[1];
                        worksheetGraph.Cells["E" + Row].Value = HierarchyData[2];
                        worksheetGraph.Cells["F" + Row].Value = HierarchyData[3];//Display Name
                        worksheetGraph.Cells["G" + Row].Value = HierarchyData[3];//Mac INV

                        worksheetGraph.Cells["H" + Row].Value = frmDate.ToString("yyyy-MM-dd") + " - " + toDate.ToString("yyyy-MM-dd");

                        worksheetGraph.Cells["I" + Row].Value = row.opname;
                        worksheetGraph.Cells["J" + Row].Value = DTConsolidatedLosses.AsEnumerable().Where(x => x.Field<string>("WCInvNo") == @HierarchyData[3] && x.Field<string>("OpName") == row.opname.ToString()).Sum(x => x.Field<int>("WOPF"));
                        worksheetGraph.Cells["K" + Row].Value = DTConsolidatedLosses.AsEnumerable().Where(x => x.Field<string>("WCInvNo") == @HierarchyData[3] && x.Field<string>("OpName") == row.opname.ToString()).Sum(x => x.Field<int>("WOProcessed"));
                        worksheetGraph.Cells["L" + Row].Value = DTConsolidatedLosses.AsEnumerable().Where(x => x.Field<string>("WCInvNo") == @HierarchyData[3] && x.Field<string>("OpName") == row.opname.ToString()).Sum(x => x.Field<int>("TotalWOQty"));

                        worksheetGraph.Cells["M" + Row].Value = DTConsolidatedLosses.AsEnumerable().Where(x => x.Field<string>("WCInvNo") == @HierarchyData[3] && x.Field<string>("OpName") == row.opname.ToString()).Sum(x => x.Field<int>("TotalTarget"));
                        worksheetGraph.Cells["N" + Row].Value = DTConsolidatedLosses.AsEnumerable().Where(x => x.Field<string>("WCInvNo") == @HierarchyData[3] && x.Field<string>("OpName") == row.opname.ToString()).Sum(x => x.Field<int>("TotalDelivered"));
                        double summarizedCuttingTime = 0;
                        double.TryParse((DTConsolidatedLosses.AsEnumerable().Where(x => x.Field<string>("WCInvNo") == @HierarchyData[3] && x.Field<string>("OpName") == row.opname.ToString()).Sum(x => x.Field<double>("TargetNC")).ToString()), out summarizedCuttingTime);
                        worksheetGraph.Cells["O" + Row].Value = summarizedCuttingTime;

                        var ValueAdding = Math.Round((DTConsolidatedLosses.AsEnumerable().Where(x => x.Field<string>("WCInvNo") == @HierarchyData[3] && x.Field<string>("OpName") == row.opname.ToString()).Sum(x => x.Field<double>("TotalValueAdding"))), 0);
                        worksheetGraph.Cells["P" + Row].Value = Math.Round(ValueAdding, 0);
                        worksheetGraph.Cells["Q" + Row].Value = Math.Round((DTConsolidatedLosses.AsEnumerable().Where(x => x.Field<string>("WCInvNo") == @HierarchyData[3] && x.Field<string>("OpName") == row.opname.ToString()).Sum(x => x.Field<double>("TotalLosses")) / 60), 0);
                        worksheetGraph.Cells["R" + Row].Value = Math.Round((DTConsolidatedLosses.AsEnumerable().Where(x => x.Field<string>("WCInvNo") == @HierarchyData[3] && x.Field<string>("OpName") == row.opname.ToString()).Sum(x => x.Field<double>("TotalSetUp")) / 60), 0);

                        ////Efficiency
                        ////worksheetGraph.Cells["S" + Row].Formula = ;
                        ////1) select rows for this date machine operator 
                        ////2) multiply individual delivered , cuttingTime
                        ////3) write to Target NC column && Calculate Efficiency

                        //double summarizedCuttingTime = 0;
                        //DataRow[] ForMacOp = DTConsolidatedLosses.Select("WCInvNo = '" + @HierarchyData[3] + "'  AND OpName = '" + row.opname.ToString() + "' ");
                        //foreach (var dr in ForMacOp)
                        //{
                        //    double delQty = 0;
                        //    double.TryParse(dr["TotalDelivered"].ToString(), out delQty);
                        //    double NCCuttingTime = 0;
                        //    double.TryParse(dr["TargetNC"].ToString(), out NCCuttingTime);
                        //    summarizedCuttingTime += delQty * (NCCuttingTime/60); 
                        //}
                        //worksheetGraph.Cells["O" + Row].Value = summarizedCuttingTime;

                        double Efficiency = Math.Round((summarizedCuttingTime / ValueAdding) * 100, 0);
                        if (Efficiency > 100)
                        {
                            worksheetGraph.Cells["S" + Row].Value = 100;
                        }
                        else
                        {
                            worksheetGraph.Cells["S" + Row].Value = Efficiency;
                        }

                        Row++;
                    }
                }
            }
            if (Row != 5)
            {
                //Cellwise Border for Summarized
                worksheetGraph.Cells["B5:S" + (Row - 1)].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheetGraph.Cells["B5:S" + (Row - 1)].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheetGraph.Cells["B5:S" + (Row - 1)].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheetGraph.Cells["B5:S" + (Row - 1)].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }
            //Apply style to Losses header
            int colLast = 26 + LossCodesData.Rows.Count; // StartCol of Losses + AllLosses // not coloring these 12 AfterLossColumns
            finalLossCol = ExcelColumnFromNumber(colLast);

            Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#32CD32");//#32CD32:lightgreen //B8C9E9
            worksheet.Cells["X4:" + finalLossCol + "" + 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["X4:" + finalLossCol + "" + 4].Style.Fill.BackgroundColor.SetColor(colFromHex);

            colLast = 26 + LossCodesData.Rows.Count + 15; // StartCol of Losses + AllLosses + 14 AfterLossColumns
            finalLossCol = ExcelColumnFromNumber(colLast);
            worksheet.Cells["X4:" + finalLossCol + "" + 4].Style.Border.Top.Style = ExcelBorderStyle.Medium;
            worksheet.Cells["X4:" + finalLossCol + "" + 4].Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
            worksheet.Cells["X4:" + finalLossCol + "" + 4].Style.Border.Left.Style = ExcelBorderStyle.Medium;
            worksheet.Cells["X4:" + finalLossCol + "" + 4].Style.Border.Right.Style = ExcelBorderStyle.Medium;

            worksheet.Cells["X4:" + finalLossCol + "" + 4].Style.WrapText = true;
            worksheet.Row(4).Height = 70;
            worksheet.View.ShowGridLines = false;
            worksheetGraph.View.ShowGridLines = false;
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            worksheetGraph.Cells[worksheet.Dimension.Address].AutoFitColumns();
            //worksheetGraph.Cells["A3:R100"].Style.Font.Color.SetColor(Color.White);

            #region Save and Download
            p.Save();

            //Downloding Excel
            string path1 = System.IO.Path.Combine(FileDir, "JobReportNew" + frda.ToString("yyyy-MM-dd") + ".xlsx");
            System.IO.FileInfo file1 = new System.IO.FileInfo(path1);
            string Outgoingfile = "JobReportNew" + frda.ToString("yyyy-MM-dd") + ".xlsx";
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

        #endregion

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
            ViewData["WorkCenterID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");
            return View();
        }

        [HttpPost]
        public ActionResult OEEReport(string PlantID, string TimeType, DateTime StartDate, DateTime EndDate, string ProdFAI, string ShopID = null, string CellID = null, string WorkCenterID = null, string TabularType = null)
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj2 = new Dao1(_conn);
            obj = new ReportsDao(_conn);

            int PlantIDInt = Convert.ToInt32(PlantID);
            ViewData["PlantID"] = new SelectList(obj2.GetPlantList(), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(obj2.GetShop1List(PlantIDInt), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(obj2.GetCellList(), "CellID", "CellName");
            ViewData["WorkCenterID"] = new SelectList(obj2.GetmachineList1(), "MachineID", "MachineDisplayName");
            // ViewData["PlantID"] = new SelectList(db.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            // ViewData["ShopID"] = new SelectList(db.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == PlantIDInt), "ShopID", "ShopName");
            //ViewData["CellID"] = new SelectList(db.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            //ViewData["WorkCenterID"] = new SelectList(db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineInvNo");

            OEEReportExcel(StartDate.ToString(), EndDate.ToString(), TimeType, ProdFAI, PlantID.ToString(), ShopID, CellID, WorkCenterID, TabularType);
            return View();
        }

        #region Old Method
        //[HttpPost]
        //public ActionResult OEEReport(int PlantID, string FromDate, string ToDate, int ShopID = 0, int CellID = 0, int MachineID = 0)
        //{
        //    ReportsCalcClass.ProdDetAndon UR = new ReportsCalcClass.ProdDetAndon();

        //    var getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0).ToList();

        //    if (MachineID != 0)
        //    {
        //        getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID).ToList();
        //    }
        //    else if (CellID != 0)
        //    {
        //        getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == CellID).ToList();
        //    }
        //    else if (ShopID != 0)
        //    {
        //        getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID).ToList();
        //    }

        //    int dateDifference = Convert.ToDateTime(ToDate).Subtract(Convert.ToDateTime(FromDate)).Days;

        //    FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\OEE_Report.xlsx");

        //    ExcelPackage templatep = new ExcelPackage(templateFile);
        //    ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];
        //    ExcelWorksheet TemplateGraph = templatep.Workbook.Worksheets[2];

        //    String FileDir = @"C:\SRKS_ifacility\ReportsList\" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd");
        //    bool exists = System.IO.Directory.Exists(FileDir);
        //    if (!exists)
        //        Directory.CreateDirectory(FileDir);

        //    FileInfo newFile = new FileInfo(Path.Combine(FileDir, "OEE_Report" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
        //    if (newFile.Exists)
        //    {
        //        try
        //        {
        //            newFile.Delete();  // ensures we create a new workbook
        //            newFile = new FileInfo(Path.Combine(FileDir, "OEE_Report" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx"));
        //        }
        //        catch
        //        {
        //            TempData["Excelopen"] = "Excel with same date is already open, please close it and try to generate!!!!";
        //            //return View();
        //        }
        //    }

        //    //Using the File for generation and populating it
        //    ExcelPackage p = null;
        //    p = new ExcelPackage(newFile);
        //    ExcelWorksheet worksheet = null;
        //    ExcelWorksheet worksheetGraph = null;

        //    //Creating the WorkSheet for populating
        //    try
        //    {
        //        worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy"), Templatews);
        //        worksheetGraph = p.Workbook.Worksheets.Add("Graphs", TemplateGraph);
        //    }
        //    catch { }

        //    if (worksheet == null)
        //    {
        //        worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy") + "1", Templatews);
        //        worksheetGraph = p.Workbook.Worksheets.Add(DateTime.Now.ToString("dd-MM-yyyy") + "Graph", TemplateGraph);
        //    }
        //    else if (worksheetGraph == null)
        //    {
        //        worksheetGraph = p.Workbook.Worksheets.Add(DateTime.Now.ToString("dd-MM-yyyy") + "Graph", TemplateGraph);
        //    }
        //    int sheetcount = p.Workbook.Worksheets.Count;
        //    p.Workbook.Worksheets.MoveToStart(sheetcount);
        //    worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //    worksheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //    decimal TotalQualityPercent = 0, TotalOEEPercent = 0, TotalAvailPercent = 0, TotalPerformancePercent = 0;

        //    int StartRow = 2;
        //    int SlNo = 1;
        //    int MachineCount = getMachineList.Count;
        //    int Startcolumn = 13;
        //    string ColNam = ExcelColumnFromNumber(Startcolumn);
        //    var GetMainLossList = Serverdb.tbllossescodes.Where(m => m.LossCodesLevel == 1 && m.IsDeleted == 0 && m.MessageType != "SETUP").OrderBy(m => m.LossCodeID).ToList();
        //    foreach (var LossRow in GetMainLossList)
        //    {
        //        ColNam = ExcelColumnFromNumber(Startcolumn);
        //        worksheet.Cells[ColNam + "1"].Value = LossRow.LossCodeDesc;
        //        Startcolumn++;
        //    }

        //    //Tabular sheet Data Population
        //    for (int i = 0; i <= dateDifference; i++)
        //    {
        //        int partscount = 0;

        //        DateTime QueryDate = Convert.ToDateTime(FromDate).AddDays(i);
        //        var partcount = Serverdb.tblpartscountandcuttings.Where(m => m.MachineID == MachineID && m.CorrectedDate == QueryDate.Date).ToList();
        //        foreach (var partcountdet in partcount)
        //        {
        //            partscount = partscount + partcountdet.PartCount;
        //        }
        //        foreach (var Machine in getMachineList)
        //        {
        //            UR.insertManMacProd(Machine.MachineID, QueryDate.Date);
        //            var GetUtilList = Serverdb.tbl_ProdManMachine.Where(m => m.MachineID == Machine.MachineID && m.CorrectedDate == QueryDate.Date).ToList();
        //            foreach (var MacRow in GetUtilList)
        //            {
        //                int MacStartcolumn = 13;
        //                worksheet.Cells["A" + StartRow].Value = SlNo;
        //                worksheet.Cells["B" + StartRow].Value = MacRow.tblmachinedetail.MachineName;
        //                worksheet.Cells["C" + StartRow].Value = MacRow.tblworkorderentry.Prod_Order_No;
        //                worksheet.Cells["D" + StartRow].Value = MacRow.tblworkorderentry.FGCode;
        //                worksheet.Cells["E" + StartRow].Value = MacRow.tblworkorderentry.ProdOrderQty;
        //                worksheet.Cells["F" + StartRow].Value = QueryDate.Date.ToString("dd-MM-yyyy");
        //                //worksheet.Cells["F" + StartRow].Value = MacRow.UtilPercent;
        //                //worksheet.Cells["H" + StartRow].Value = MacRow.QualityPercent;
        //                //worksheet.Cells["G" + StartRow].Value = MacRow.PerformancePerCent;
        //                //double oee = Convert.ToDouble(((MacRow.UtilPercent / 100) * (100 / 100) * (MacRow.PerformancePerCent / 100)) * 100);
        //                //worksheet.Cells["I" + StartRow].Value = Math.Round(oee, 2);
        //                //worksheet.Cells["J" + StartRow].Value = partscount;
        //                worksheet.Cells["G" + StartRow].Value = MacRow.TotalOperatingTime;
        //                worksheet.Cells["H" + StartRow].Value = MacRow.tblworkorderentry.Yield_Qty;
        //                worksheet.Cells["I" + StartRow].Value = MacRow.tblworkorderentry.ScrapQty;
        //                //worksheet.Cells["K" + StartRow].Value = MacRow.TotalSetup;
        //                int TotalQty = MacRow.tblworkorderentry.Yield_Qty + MacRow.tblworkorderentry.ScrapQty;
        //                TotalQualityPercent += MacRow.QualityPercent;
        //                //TotalOEEPercent += (decimal)oee;
        //                TotalAvailPercent += (decimal)MacRow.UtilPercent;
        //                TotalPerformancePercent += MacRow.PerformancePerCent;

        //                if (TotalQty == 0)
        //                    TotalQty = 1;
        //                worksheet.Cells["J1"].Value = "Setup Time";
        //                //worksheet.Cells["K" + StartRow].Value = 0;
        //                worksheet.Cells["K1"].Value = "Rejections";
        //                worksheet.Cells["K" + StartRow].Value = (MacRow.TotalOperatingTime / TotalQty) * MacRow.tblworkorderentry.ScrapQty;
        //                long MacTotalLoss = 0;
        //                foreach (var LossRow in GetMainLossList)
        //                {
        //                    var getWoLossList1 = Serverdb.tbl_ProdOrderLosses.Where(m => m.WOID == MacRow.WOID && m.LossID == LossRow.LossCodeID).FirstOrDefault();
        //                    String ColEntry = ExcelColumnFromNumber(MacStartcolumn);
        //                    if (getWoLossList1 != null)
        //                    {
        //                        worksheet.Cells[ColEntry + "" + StartRow].Value = getWoLossList1.LossDuration;
        //                        MacTotalLoss += getWoLossList1.LossDuration;
        //                    }
        //                    else
        //                        worksheet.Cells[ColEntry + "" + StartRow].Value = 0;
        //                    MacStartcolumn++;
        //                }
        //                string ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                worksheet.Cells[ColEntry1 + "1"].Value = "No Power";
        //                worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.TotalPowerLoss;
        //                MacStartcolumn++;

        //                //ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                //worksheet.Cells[ColEntry1 + "1"].Value = "Total Part";
        //                //worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.tblworkorderentry.Total_Qty;
        //                //MacStartcolumn++;

        //                ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                worksheet.Cells[ColEntry1 + "1"].Value = "Load / Unload (in hr)";
        //                worksheet.Cells[ColEntry1 + "" + StartRow].Value = Math.Round((MacRow.TotalMinorLoss - MacRow.TotalSetupMinorLoss) / 60, 2);
        //                MacStartcolumn++;

        //                //ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                //worksheet.Cells[ColEntry1 + "1"].Value = "Shift";
        //                ////if (MacRow.tblworkorderentry.ShiftID == 1)
        //                ////    worksheet.Cells[ColEntry1 + StartRow].Value = "First Shift";
        //                ////else
        //                ////    worksheet.Cells[ColEntry1 + StartRow].Value = "Second Shift";
        //                //worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.tblworkorderentry.ShiftID;
        //                //MacStartcolumn++;

        //                ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                worksheet.Cells[ColEntry1 + "1"].Value = "Operator ID";
        //                worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.tblworkorderentry.OperatorID;
        //                MacStartcolumn++;

        //                //ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                //worksheet.Cells[ColEntry1 + "1"].Value = "Total OEE Loss";
        //                //worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacTotalLoss;
        //                //MacStartcolumn++;

        //                //decimal OEEPercent = (decimal)Math.Round((double)(MacRow.UtilPercent / 100) * (double)(MacRow.PerformancePerCent / 100) * (double)(MacRow.QualityPercent / 100) * 100, 2);

        //                //ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                //worksheet.Cells[ColEntry1 + "1"].Value = "% of OEE";
        //                //worksheet.Cells[ColEntry1 + "" + StartRow].Value = OEEPercent;
        //                //MacStartcolumn++;
        //                StartRow++;
        //                SlNo++;
        //            }
        //        }
        //    }
        //    StartRow = 2;

        //    DataTable LossTbl = new DataTable();
        //    LossTbl.Columns.Add("LossID", typeof(int));
        //    LossTbl.Columns.Add("LossDuration", typeof(int));
        //    LossTbl.Columns.Add("LossTarget", typeof(string));
        //    LossTbl.Columns.Add("LossName", typeof(string));
        //    LossTbl.Columns.Add("LossActual", typeof(string));

        //    //Graph Sheet Population
        //    //Start Date and End Date
        //    worksheetGraph.Cells["C6"].Value = Convert.ToDateTime(FromDate).ToString("dd-MM-yyyy");
        //    worksheetGraph.Cells["E6"].Value = Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy");
        //    int GetHolidays = getsundays(Convert.ToDateTime(ToDate), Convert.ToDateTime(FromDate));
        //    int WorkingDays = dateDifference - GetHolidays + 1;
        //    //Working Days
        //    worksheetGraph.Cells["E5"].Value = WorkingDays;
        //    //Planned Production Time
        //    worksheetGraph.Cells["E10"].Value = WorkingDays * 24 * MachineCount;
        //    double TotalOperatingTime = 0;
        //    double TotalDownTime = 0;
        //    double TotalAcceptedQty = 0;
        //    double TotalRejectedQty = 0;
        //    double TotalPerformanceFactor = 0;
        //    int StartGrpah1 = 48;
        //    for (int i = 0; i <= dateDifference; i++)
        //    {
        //        double DayOperatingTime = 0;
        //        double DayDownTime = 0;
        //        double DayAcceptedQty = 0;
        //        double DayRejectedQty = 0;
        //        double DayPerformanceFactor = 0;
        //        DateTime QueryDate = Convert.ToDateTime(FromDate).AddDays(i);

        //        var plantName = Serverdb.tblplants.Where(m => m.PlantID == PlantID).Select(m => m.PlantName).FirstOrDefault();
        //        worksheetGraph.Cells["C3"].Value = plantName;
        //        foreach (var MachRow in getMachineList)
        //        {
        //            if (MachineID == 0)
        //            {
        //                worksheetGraph.Cells["C4"].Value = MachRow.tblcell.CelldisplayName;
        //                worksheetGraph.Cells["C5"].Value = "AS DIVISION";
        //            }
        //            else
        //            {
        //                worksheetGraph.Cells["C4"].Value = MachRow.tblcell.CelldisplayName;
        //                worksheetGraph.Cells["C5"].Value = MachRow.MachineDisplayName;
        //            }
        //            var GetUtilList = Serverdb.tbl_ProdManMachine.Where(m => m.MachineID == MachRow.MachineID && m.CorrectedDate == QueryDate.Date).ToList();
        //            foreach (var ProdRow in GetUtilList)
        //            {
        //                //Total Values
        //                TotalOperatingTime += (double)ProdRow.TotalOperatingTime;
        //                TotalDownTime += (double)ProdRow.TotalLoss + (double)ProdRow.TotalMinorLoss + (double)ProdRow.TotalSetup;
        //                TotalAcceptedQty += ProdRow.tblworkorderentry.Yield_Qty;
        //                TotalRejectedQty += ProdRow.tblworkorderentry.ScrapQty;
        //                TotalPerformanceFactor += ProdRow.PerfromaceFactor;
        //                //Day Values
        //                DayOperatingTime += (double)ProdRow.TotalOperatingTime;
        //                DayDownTime += (double)ProdRow.TotalLoss + (double)ProdRow.TotalMinorLoss;
        //                DayAcceptedQty += ProdRow.tblworkorderentry.Yield_Qty;
        //                DayRejectedQty += ProdRow.tblworkorderentry.ScrapQty;
        //                DayPerformanceFactor += ProdRow.PerfromaceFactor;
        //            }
        //            var GetLossList = Serverdb.tbl_ProdOrderLosses.Where(m => m.MachineID == MachRow.MachineID && m.CorrectedDate == QueryDate.Date).ToList();

        //            foreach (var LossRow in GetLossList)
        //            {
        //                var getrow = (from DataRow row in LossTbl.Rows where row.Field<int>("LossID") == LossRow.LossID select row["LossID"]).FirstOrDefault();
        //                if (getrow == null)
        //                {
        //                    var GetLossTargetPercent = "1%";
        //                    String GetLossName = null;
        //                    var GetLossTarget = Serverdb.tbllossescodes.Where(m => m.LossCodeID == LossRow.LossID).FirstOrDefault();
        //                    if (GetLossTarget != null)
        //                    {
        //                        GetLossTargetPercent = GetLossTarget.TargetPercent.ToString();
        //                        GetLossName = GetLossTarget.LossCodeDesc;
        //                    }

        //                    LossTbl.Rows.Add(LossRow.LossID, LossRow.LossDuration, GetLossTargetPercent, GetLossName);
        //                }
        //                else
        //                {
        //                    foreach (DataRow GetRow in LossTbl.Rows)
        //                    {
        //                        if (Convert.ToInt32(GetRow["LossID"]) == LossRow.LossID)
        //                        {
        //                            long LossDura = Convert.ToInt32(GetRow["LossDuration"]);
        //                            LossDura += LossRow.LossDuration;
        //                            GetRow["LossDuration"] = LossDura;
        //                        }
        //                    }

        //                }
        //            }
        //        }
        //        int TotQty = (int)(DayAcceptedQty + DayRejectedQty);
        //        if (TotQty == 0)
        //            TotQty = 1;

        //        double DayOpTime = DayOperatingTime;
        //        if (DayOpTime == 0)
        //            DayOpTime = 1;

        //        decimal DayAvailPercent = (decimal)Math.Round(DayOperatingTime / (24 * MachineCount), 2);
        //        decimal DayPerformancePercent = (decimal)Math.Round(DayPerformanceFactor / DayOpTime, 2);
        //        decimal DayQualityPercent = (decimal)Math.Round((DayAcceptedQty / (TotQty)), 2);
        //        decimal DayOEEPercent = (decimal)Math.Round((double)(DayAvailPercent) * (double)(DayPerformancePercent) * (double)(DayQualityPercent), 2);

        //        worksheetGraph.Cells["B" + StartGrpah1].Value = QueryDate.ToString("dd-MM-yyyy");
        //        worksheetGraph.Cells["C" + StartGrpah1].Value = 0.88;
        //        worksheetGraph.Cells["D" + StartGrpah1].Value = (DayOEEPercent / 100) / 100;

        //        StartGrpah1++;
        //    }
        //    worksheetGraph.Cells["E11"].Value = (double)Math.Round(TotalOperatingTime / 60, 2);
        //    worksheetGraph.Cells["E12"].Value = (double)Math.Round(TotalDownTime / 60, 2);
        //    worksheetGraph.Cells["E13"].Value = TotalAcceptedQty;
        //    worksheetGraph.Cells["E14"].Value = TotalRejectedQty;

        //    decimal TotalQualityPercent1 = 0, TotalOEEPercent1 = 0, TotalAvailPercent1 = 0, TotalPerformancePercent1 = 0;

        //    if (TotalAcceptedQty != 0 && TotalRejectedQty != 0)
        //    {
        //        TotalAvailPercent1 = (decimal)Math.Round(TotalOperatingTime / (WorkingDays * 24 * 60 * MachineCount), 2);
        //        TotalPerformancePercent1 = (decimal)Math.Round(TotalPerformanceFactor / TotalOperatingTime, 2);
        //        TotalQualityPercent1 = (decimal)Math.Round((TotalAcceptedQty / (TotalAcceptedQty + TotalRejectedQty)), 2);
        //        TotalOEEPercent1 = (decimal)Math.Round((double)(TotalAvailPercent) * (double)(TotalPerformancePercent) * (double)(TotalQualityPercent), 2);
        //    }


        //    if (TotalAcceptedQty != 0 && TotalRejectedQty != 0)
        //    {
        //        worksheetGraph.Cells["E20"].Value = TotalAvailPercent1;
        //        worksheetGraph.Cells["E21"].Value = TotalPerformancePercent1;
        //        worksheetGraph.Cells["E22"].Value = TotalQualityPercent1;
        //        worksheetGraph.Cells["E23"].Value = TotalOEEPercent1;
        //        worksheetGraph.Cells["G5"].Value = TotalOEEPercent1;
        //        worksheetGraph.View.ShowGridLines = false;
        //    }
        //    else
        //    {
        //        int diff = dateDifference + 1;
        //        worksheetGraph.Cells["E20"].Value = (TotalAvailPercent / 100) / diff;
        //        worksheetGraph.Cells["E21"].Value = (TotalPerformancePercent / 100) / diff;
        //        worksheetGraph.Cells["E22"].Value = (TotalQualityPercent / 100) / diff;
        //        worksheetGraph.Cells["E23"].Value = (TotalOEEPercent / 100) / diff;
        //        worksheetGraph.Cells["G5"].Value = (TotalOEEPercent / 100) / diff;
        //        worksheetGraph.View.ShowGridLines = false;
        //    }


        //    DateTime fromDate = Convert.ToDateTime(FromDate);
        //    DateTime toDate = Convert.ToDateTime(ToDate);
        //    var top3ContrubutingFactors = (from dbItem in Serverdb.tbl_ProdOrderLosses
        //                                   where dbItem.CorrectedDate >= fromDate.Date && dbItem.CorrectedDate <= toDate.Date
        //                                   group dbItem by dbItem.LossID into x
        //                                   select new
        //                                   {
        //                                       LossId = x.Key,
        //                                       LossDuration = Serverdb.tbl_ProdOrderLosses.Where(m => m.LossID == x.Key).Select(m => m.LossDuration).Sum()
        //                                   }).ToList();
        //    var item = top3ContrubutingFactors.OrderByDescending(m => m.LossDuration).Take(3).ToList();
        //    int lossXccelNo = 29;
        //    foreach (var GetRow in item)
        //    {
        //        string lossCode = Serverdb.tbllossescodes.Where(m => m.LossCodeID == GetRow.LossId).Select(m => m.LossCodeDesc).FirstOrDefault();
        //        decimal lossPercentage = (decimal)Math.Round(((GetRow.LossDuration) / TotalDownTime), 2);
        //        decimal lossDurationInHours = (decimal)Math.Round((GetRow.LossDuration / 60.00), 2);
        //        worksheetGraph.Cells["L" + lossXccelNo].Value = lossCode;
        //        worksheetGraph.Cells["N" + lossXccelNo].Value = lossPercentage;
        //        worksheetGraph.Cells["O" + lossXccelNo].Value = lossDurationInHours;

        //        lossXccelNo++;
        //    }

        //    int grphData = 5;
        //    decimal CumulativePercentage = 0;
        //    foreach (var data in top3ContrubutingFactors)
        //    {
        //        var dbLoss = Serverdb.tbllossescodes.Where(m => m.LossCodeID == data.LossId).FirstOrDefault();
        //        string lossCode = dbLoss.LossCodeDesc;
        //        decimal Target = dbLoss.TargetPercent;
        //        decimal actualPercentage = (decimal)Math.Round(((data.LossDuration) / TotalDownTime), 2);
        //        CumulativePercentage = CumulativePercentage + actualPercentage;
        //        worksheetGraph.Cells["K" + grphData].Value = lossCode;
        //        worksheetGraph.Cells["L" + grphData].Value = Target;
        //        worksheetGraph.Cells["M" + grphData].Value = actualPercentage;
        //        worksheetGraph.Cells["N" + grphData].Value = CumulativePercentage;
        //        grphData++;
        //    }

        //    //Code written on 05-10-2018
        //    int col = 12, col1 = 12, col2 = 13, col3 = 14;

        //    foreach (var GetRow in item)
        //    {
        //        string lossCode = Serverdb.tbllossescodes.Where(m => m.LossCodeID == GetRow.LossId).Select(m => m.LossCodeDesc).FirstOrDefault();

        //        string columnNumber = ExcelColumnFromNumber(col);
        //        worksheetGraph.Cells[columnNumber + 36].Value = lossCode;

        //        int macLossNo = 38;

        //        DataTable dt = new DataTable();
        //        MsqlConnection mc = new MsqlConnection();
        //        mc.open();
        //        string query = "SELECT TOP 3 SUM(tpol.LossDuration),tblma.MachineName,tbllo.LossCodeDesc,tpol.CorrectedDate from " + dbName + ".tbl_ProdOrderLosses tpol inner join " + dbName + ".tbllossescodes tbllo on tbllo.LossCodeID = tpol.LossID inner join " + dbName + ".tblmachinedetails tblma on tblma.MachineID = tpol.MachineID where tpol.CorrectedDate >= '" + fromDate.Date + "' and tpol.CorrectedDate <= '" + toDate.Date + "' AND tbllo.LossCodeDesc = '" + lossCode + "' " +
        //            "group by tbllo.LossCodeDesc,tblma.MachineName,tpol.LossDuration, tpol.CorrectedDate order by tpol.LossDuration DESC";
        //        SqlDataAdapter sdt = new SqlDataAdapter(query, mc.msqlConnection);
        //        sdt.Fill(dt);
        //        mc.close();

        //        for (int i = 0; i < dt.Rows.Count; i++)
        //        {
        //            double value = Convert.ToDouble(dt.Rows[i][0]);
        //            string macName = Convert.ToString(dt.Rows[i][1]);

        //            decimal lossPercentage = (decimal)Math.Round(value / TotalDownTime, 2);
        //            decimal lossDurationInHours = (decimal)Math.Round(value / 60.00, 2);

        //            string colNum1 = ExcelColumnFromNumber(col1);
        //            worksheetGraph.Cells[colNum1 + macLossNo].Value = macName;
        //            string colNum2 = ExcelColumnFromNumber(col2);
        //            worksheetGraph.Cells[colNum2 + macLossNo].Value = lossPercentage;
        //            string colNum3 = ExcelColumnFromNumber(col3);
        //            worksheetGraph.Cells[colNum3 + macLossNo].Value = lossDurationInHours;

        //            macLossNo++;
        //        }

        //        col += 4; col1 += 4; col2 += 4; col3 += 4;
        //    }
        //    //Code ended on 05-10-2018

        //    //Code Written on 09-10-2018
        //    for (int i = 0; i < dateDifference; i++)
        //    {
        //        DateTime QueryDate = Convert.ToDateTime(FromDate).AddDays(i);
        //        foreach (var Machine in getMachineList)
        //        {
        //            DataTable dt = new DataTable();
        //            try
        //            {
        //                using (MsqlConnection mc = new MsqlConnection())
        //                {
        //                    using (SqlCommand cmd = new SqlCommand("InsertOEEReportDivision", mc.msqlConnection))
        //                    {
        //                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
        //                        {
        //                            mc.open();
        //                            cmd.CommandType = CommandType.StoredProcedure;
        //                            cmd.Parameters.AddWithValue("@MachineID", Machine.MachineID);
        //                            cmd.Parameters.AddWithValue("@CorrectedDate", QueryDate.Date);
        //                            sda.Fill(dt);
        //                            mc.close();
        //                        }
        //                    }
        //                }
        //            }
        //            catch (Exception ex) { }
        //        }
        //    }

        //    var topContributingFactors = (from dbItem in Serverdb.tbl_oeereportasdivision
        //                                  where dbItem.CorrectedDate >= fromDate.Date && dbItem.CorrectedDate <= toDate.Date
        //                                  group dbItem by new { dbItem.LossID, dbItem.FGCode } into x
        //                                  select new
        //                                  {
        //                                      x.Key.LossID,
        //                                      x.Key.FGCode,
        //                                      LossDuration = x.Select(m => m.LossDuration).Sum(),
        //                                  }).ToList();


        //    if (CellID != 0)
        //    {
        //        var getCellName = Serverdb.tblcells.Where(m => m.CellID == CellID).Select(m => m.CellName).FirstOrDefault();
        //        // worksheetGraph.Cells["K46"].Value = getCellName;
        //    }


        //    var getValues = topContributingFactors.OrderByDescending(m => m.LossDuration).ThenBy(m => m.LossID).ToList();
        //    var distinctLoss = getValues.Select(m => m.LossID).Distinct().Take(10).ToList();
        //    int colNum = 48;
        //    for (int i = 0; i < distinctLoss.Count; i++)
        //    {
        //        int colVal1 = 12, colVal2 = 13;
        //        var getLossId = distinctLoss[i];
        //        string losscode = Serverdb.tbllossescodes.Where(m => m.LossCodeID == getLossId).Select(m => m.LossCodeDesc).FirstOrDefault();
        //        worksheetGraph.Cells["K" + colNum].Value = losscode;

        //        var top3AccToLoss = getValues.Where(m => m.LossID == getLossId).OrderByDescending(m => m.LossDuration).Take(3).ToList();
        //        foreach (var data in top3AccToLoss)
        //        {
        //            var FGcode = data.FGCode;
        //            decimal LossDurationinHours = (decimal)Math.Round((data.LossDuration) / 60.00, 2);

        //            string colName1 = ExcelColumnFromNumber(colVal1);
        //            worksheetGraph.Cells[colName1 + colNum].Value = FGcode;
        //            string colName2 = ExcelColumnFromNumber(colVal2);
        //            worksheetGraph.Cells[colName2 + colNum].Value = LossDurationinHours;
        //            colVal1 += 2; colVal2 += 2;
        //        }
        //        colNum++;
        //    }
        //    //Code written on 09-10-2018

        //    #region
        //    //var chartIDAndUnID = (ExcelBarChart)worksheetGraph.Drawings.AddChart("Testing", eChartType.ColumnClustered);

        //    //chartIDAndUnID.SetSize((350), 550);

        //    //chartIDAndUnID.SetPosition(50, 60);

        //    //chartIDAndUnID.Title.Text = "AB Graph ";
        //    //chartIDAndUnID.Style = eChartStyle.Style18;
        //    //chartIDAndUnID.Legend.Position = eLegendPosition.Bottom;
        //    ////chartIDAndUnID.Legend.Remove();
        //    //chartIDAndUnID.YAxis.MaxValue = 100;
        //    //chartIDAndUnID.YAxis.MinValue = 0;
        //    //chartIDAndUnID.YAxis.MajorUnit = 5;

        //    //chartIDAndUnID.Locked = false;
        //    //chartIDAndUnID.PlotArea.Border.Width = 0;
        //    //chartIDAndUnID.YAxis.MinorTickMark = eAxisTickMark.None;
        //    //chartIDAndUnID.DataLabel.ShowValue = true;
        //    //chartIDAndUnID.DisplayBlanksAs = eDisplayBlanksAs.Gap;


        //    //ExcelRange dateWork = worksheetGraph.Cells["K33:" + lossXccelNo];
        //    //ExcelRange hoursWork = worksheetGraph.Cells["N33:" + lossXccelNo];
        //    //var hours = (ExcelChartSerie)(chartIDAndUnID.Series.Add(hoursWork, dateWork));
        //    //hours.Header = "Operating Time (Hours)";
        //    ////Get reference to the worksheet xml for proper namespace
        //    //var chartXml = chartIDAndUnID.ChartXml;
        //    //var nsuri = chartXml.DocumentElement.NamespaceURI;
        //    //var nsm = new XmlNamespaceManager(chartXml.NameTable);
        //    //nsm.AddNamespace("c", nsuri);

        //    ////XY Scatter plots have 2 value axis and no category
        //    //var valAxisNodes = chartXml.SelectNodes("c:chartSpace/c:chart/c:plotArea/c:valAx", nsm);
        //    //if (valAxisNodes != null && valAxisNodes.Count > 0)
        //    //    foreach (XmlNode valAxisNode in valAxisNodes)
        //    //    {
        //    //        var major = valAxisNode.SelectSingleNode("c:majorGridlines", nsm);
        //    //        if (major != null)
        //    //            valAxisNode.RemoveChild(major);

        //    //        var minor = valAxisNode.SelectSingleNode("c:minorGridlines", nsm);
        //    //        if (minor != null)
        //    //            valAxisNode.RemoveChild(minor);
        //    //    }

        //    ////Other charts can have a category axis
        //    //var catAxisNodes = chartXml.SelectNodes("c:chartSpace/c:chart/c:plotArea/c:catAx", nsm);
        //    //if (catAxisNodes != null && catAxisNodes.Count > 0)
        //    //    foreach (XmlNode catAxisNode in catAxisNodes)
        //    //    {
        //    //        var major = catAxisNode.SelectSingleNode("c:majorGridlines", nsm);
        //    //        if (major != null)
        //    //            catAxisNode.RemoveChild(major);

        //    //        var minor = catAxisNode.SelectSingleNode("c:minorGridlines", nsm);
        //    //        if (minor != null)
        //    //            catAxisNode.RemoveChild(minor);
        //    //    }
        //    //worksheetGraph.View["L29"]
        //    //worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        //    #endregion
        //    //worksheet.Column(29).Width = 12;
        //    p.Save();

        //    //Downloding Excel
        //    string path1 = System.IO.Path.Combine(FileDir, "OEE_Report" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx");
        //    DownloadUtilReport(path1, "OEE_Report", ToDate);

        //    ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", PlantID);
        //    ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID), "ShopID", "ShopName", ShopID);
        //    ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID), "CellID", "CellName", CellID);
        //    ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID && m.CellID == CellID), "MachineID", "MachineDisplayName", MachineID);
        //    return View();
        //}
        #endregion

        #region New
        //[HttpPost]
        //[HttpPost]
        //public ActionResult OEEReport(int PlantID, string FromDate, string ToDate, int ShopID = 0, int CellID = 0, int MachineID = 0)
        //{
        //    ReportsCalcClass.ProdDetAndon UR = new ReportsCalcClass.ProdDetAndon();

        //    var getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0).ToList();

        //    if (MachineID != 0)
        //    {
        //        getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID).ToList();
        //    }
        //    else if (CellID != 0)
        //    {
        //        getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == CellID).ToList();
        //    }
        //    else if (ShopID != 0)
        //    {
        //        getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID).ToList();
        //    }

        //    int dateDifference = Convert.ToDateTime(ToDate).Subtract(Convert.ToDateTime(FromDate)).Days;

        //    FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\OEE_Report.xlsx");

        //    ExcelPackage templatep = new ExcelPackage(templateFile);
        //    ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];
        //    ExcelWorksheet TemplateGraph = templatep.Workbook.Worksheets[2];

        //    String FileDir = @"C:\SRKS_ifacility\ReportsList\" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd");
        //    bool exists = System.IO.Directory.Exists(FileDir);
        //    if (!exists)
        //        Directory.CreateDirectory(FileDir);

        //    FileInfo newFile = new FileInfo(Path.Combine(FileDir, "OEE_Report" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
        //    if (newFile.Exists)
        //    {
        //        try
        //        {
        //            newFile.Delete();  // ensures we create a new workbook
        //            newFile = new FileInfo(Path.Combine(FileDir, "OEE_Report" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx"));
        //        }
        //        catch
        //        {
        //            TempData["Excelopen"] = "Excel with same date is already open, please close it and try to generate!!!!";
        //            //return View();
        //        }
        //    }

        //    //Using the File for generation and populating it
        //    ExcelPackage p = null;
        //    p = new ExcelPackage(newFile);
        //    ExcelWorksheet worksheet = null;
        //    ExcelWorksheet worksheetGraph = null;

        //    //Creating the WorkSheet for populating
        //    try
        //    {
        //        worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy"), Templatews);
        //        worksheetGraph = p.Workbook.Worksheets.Add("Graphs", TemplateGraph);
        //    }
        //    catch { }

        //    if (worksheet == null)
        //    {
        //        worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy") + "1", Templatews);
        //        worksheetGraph = p.Workbook.Worksheets.Add(DateTime.Now.ToString("dd-MM-yyyy") + "Graph", TemplateGraph);
        //    }
        //    else if (worksheetGraph == null)
        //    {
        //        worksheetGraph = p.Workbook.Worksheets.Add(DateTime.Now.ToString("dd-MM-yyyy") + "Graph", TemplateGraph);
        //    }
        //    int sheetcount = p.Workbook.Worksheets.Count;
        //    p.Workbook.Worksheets.MoveToStart(sheetcount);
        //    worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //    worksheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //    decimal TotalQualityPercent = 0, TotalOEEPercent = 0, TotalAvailPercent = 0, TotalPerformancePercent = 0;

        //    int StartRow = 2;
        //    int SlNo = 1;
        //    int MachineCount = getMachineList.Count;
        //    int Startcolumn = 13;
        //    string ColNam = ExcelColumnFromNumber(Startcolumn);
        //    var GetMainLossList = Serverdb.tbllossescodes.Where(m => m.LossCodesLevel == 1 && m.IsDeleted == 0 && m.MessageType != "SETUP").OrderBy(m => m.LossCodeID).ToList();
        //    foreach (var LossRow in GetMainLossList)
        //    {
        //        ColNam = ExcelColumnFromNumber(Startcolumn);
        //        worksheet.Cells[ColNam + "1"].Value = LossRow.LossCodeDesc;
        //        Startcolumn++;
        //    }

        //    //Tabular sheet Data Population
        //    for (int i = 0; i <= dateDifference; i++)
        //    {
        //        int partscount = 0;

        //        DateTime QueryDate = Convert.ToDateTime(FromDate).AddDays(i);
        //        var partcount = Serverdb.tblpartscountandcuttings.Where(m => m.MachineID == MachineID && m.CorrectedDate == QueryDate.Date).ToList();
        //        foreach (var partcountdet in partcount)
        //        {
        //            partscount = partscount + partcountdet.PartCount;
        //        }
        //        foreach (var Machine in getMachineList)
        //        {
        //            UR.insertManMacProd(Machine.MachineID, QueryDate.Date);
        //            var GetUtilList = Serverdb.tbl_ProdManMachine.Where(m => m.MachineID == Machine.MachineID && m.CorrectedDate == QueryDate.Date).ToList();
        //            foreach (var MacRow in GetUtilList)
        //            {
        //                int MacStartcolumn = 13;
        //                if (MacRow.tblworkorderentry != null)
        //                {
        //                    worksheet.Cells["A" + StartRow].Value = SlNo;
        //                    worksheet.Cells["B" + StartRow].Value = MacRow.tblmachinedetail.MachineName;
        //                    worksheet.Cells["C" + StartRow].Value = MacRow.tblworkorderentry.Prod_Order_No;
        //                    worksheet.Cells["D" + StartRow].Value = MacRow.tblworkorderentry.FGCode;
        //                    worksheet.Cells["E" + StartRow].Value = MacRow.tblworkorderentry.ProdOrderQty;
        //                    worksheet.Cells["F" + StartRow].Value = QueryDate.Date.ToString("dd-MM-yyyy");
        //                    //worksheet.Cells["F" + StartRow].Value = MacRow.UtilPercent;
        //                    //worksheet.Cells["H" + StartRow].Value = MacRow.QualityPercent;
        //                    //worksheet.Cells["G" + StartRow].Value = MacRow.PerformancePerCent;
        //                    //double oee = Convert.ToDouble(((MacRow.UtilPercent / 100) * (100 / 100) * (MacRow.PerformancePerCent / 100)) * 100);
        //                    //worksheet.Cells["I" + StartRow].Value = Math.Round(oee, 2);
        //                    //worksheet.Cells["J" + StartRow].Value = partscount;
        //                    worksheet.Cells["G" + StartRow].Value = MacRow.TotalOperatingTime;
        //                    worksheet.Cells["H" + StartRow].Value = MacRow.tblworkorderentry.Yield_Qty;
        //                    worksheet.Cells["I" + StartRow].Value = MacRow.tblworkorderentry.ScrapQty;
        //                    //worksheet.Cells["K" + StartRow].Value = MacRow.TotalSetup;
        //                    int TotalQty = MacRow.tblworkorderentry.Yield_Qty + MacRow.tblworkorderentry.ScrapQty;
        //                    TotalQualityPercent += MacRow.QualityPercent;
        //                    //TotalOEEPercent += (decimal)oee;
        //                    TotalAvailPercent += (decimal)MacRow.UtilPercent;
        //                    TotalPerformancePercent += MacRow.PerformancePerCent;

        //                    if (TotalQty == 0)
        //                        TotalQty = 1;
        //                    worksheet.Cells["K1"].Value = "Setup Time";
        //                    worksheet.Cells["K" + StartRow].Value = 0;
        //                    worksheet.Cells["L1"].Value = "Rejections";
        //                    worksheet.Cells["L" + StartRow].Value = (MacRow.TotalOperatingTime / TotalQty) * MacRow.tblworkorderentry.ScrapQty;
        //                    long MacTotalLoss = 0;
        //                    foreach (var LossRow in GetMainLossList)
        //                    {
        //                        var getWoLossList1 = Serverdb.tbl_ProdOrderLosses.Where(m => m.WOID == MacRow.WOID && m.LossID == LossRow.LossCodeID).FirstOrDefault();
        //                        String ColEntry = ExcelColumnFromNumber(MacStartcolumn);
        //                        if (getWoLossList1 != null)
        //                        {
        //                            worksheet.Cells[ColEntry + "" + StartRow].Value = getWoLossList1.LossDuration;
        //                            MacTotalLoss += getWoLossList1.LossDuration;
        //                        }
        //                        else
        //                            worksheet.Cells[ColEntry + "" + StartRow].Value = 0;
        //                        MacStartcolumn++;
        //                    }
        //                    string ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                    worksheet.Cells[ColEntry1 + "1"].Value = "No Power";
        //                    worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.TotalPowerLoss;
        //                    MacStartcolumn++;

        //                    //ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                    //worksheet.Cells[ColEntry1 + "1"].Value = "Total Part";
        //                    //worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.tblworkorderentry.Total_Qty;
        //                    //MacStartcolumn++;

        //                    ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                    worksheet.Cells[ColEntry1 + "1"].Value = "Load / Unload (in hr)";
        //                    worksheet.Cells[ColEntry1 + "" + StartRow].Value = Math.Round((MacRow.TotalMinorLoss - MacRow.TotalSetupMinorLoss) / 60, 2);
        //                    MacStartcolumn++;

        //                    //ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                    //worksheet.Cells[ColEntry1 + "1"].Value = "Shift";
        //                    ////if (MacRow.tblworkorderentry.ShiftID == 1)
        //                    ////    worksheet.Cells[ColEntry1 + StartRow].Value = "First Shift";
        //                    ////else
        //                    ////    worksheet.Cells[ColEntry1 + StartRow].Value = "Second Shift";
        //                    //worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.tblworkorderentry.ShiftID;
        //                    //MacStartcolumn++;

        //                    ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                    worksheet.Cells[ColEntry1 + "1"].Value = "Operator ID";
        //                    worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.tblworkorderentry.OperatorID;
        //                    MacStartcolumn++;

        //                    //ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                    //worksheet.Cells[ColEntry1 + "1"].Value = "Total OEE Loss";
        //                    //worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacTotalLoss;
        //                    //MacStartcolumn++;

        //                    //decimal OEEPercent = (decimal)Math.Round((double)(MacRow.UtilPercent / 100) * (double)(MacRow.PerformancePerCent / 100) * (double)(MacRow.QualityPercent / 100) * 100, 2);

        //                    //ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                    //worksheet.Cells[ColEntry1 + "1"].Value = "% of OEE";
        //                    //worksheet.Cells[ColEntry1 + "" + StartRow].Value = OEEPercent;
        //                    //MacStartcolumn++;
        //                    StartRow++;
        //                    SlNo++;

        //                }
        //                else { }
        //            }

        //        }
        //    }
        //    StartRow = 2;

        //    DataTable LossTbl = new DataTable();
        //    LossTbl.Columns.Add("LossID", typeof(int));
        //    LossTbl.Columns.Add("LossDuration", typeof(int));
        //    LossTbl.Columns.Add("LossTarget", typeof(string));
        //    LossTbl.Columns.Add("LossName", typeof(string));
        //    LossTbl.Columns.Add("LossActual", typeof(string));

        //    //Graph Sheet Population
        //    //Start Date and End Date
        //    worksheetGraph.Cells["C6"].Value = Convert.ToDateTime(FromDate).ToString("dd-MM-yyyy");
        //    worksheetGraph.Cells["E6"].Value = Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy");
        //    int GetHolidays = getsundays(Convert.ToDateTime(ToDate), Convert.ToDateTime(FromDate));
        //    int WorkingDays = dateDifference - GetHolidays + 1;
        //    //Working Days
        //    worksheetGraph.Cells["E5"].Value = WorkingDays;
        //    //Planned Production Time
        //    worksheetGraph.Cells["E10"].Value = WorkingDays * 24 * MachineCount;
        //    double TotalOperatingTime = 0;
        //    double TotalDownTime = 0;
        //    double TotalAcceptedQty = 0;
        //    double TotalRejectedQty = 0;
        //    double TotalPerformanceFactor = 0;
        //    int StartGrpah1 = 48;
        //    for (int i = 0; i <= dateDifference; i++)
        //    {
        //        double DayOperatingTime = 0;
        //        double DayDownTime = 0;
        //        double DayAcceptedQty = 0;
        //        double DayRejectedQty = 0;
        //        double DayPerformanceFactor = 0;
        //        DateTime QueryDate = Convert.ToDateTime(FromDate).AddDays(i);

        //        var plantName = Serverdb.tblplants.Where(m => m.PlantID == PlantID).Select(m => m.PlantName).FirstOrDefault();
        //        worksheetGraph.Cells["C3"].Value = plantName;
        //        foreach (var MachRow in getMachineList)
        //        {
        //            if (MachineID == 0)
        //            {
        //                worksheetGraph.Cells["C4"].Value = MachRow.tblcell.CelldisplayName;
        //                worksheetGraph.Cells["C5"].Value = "AS DIVISION";
        //            }
        //            else
        //            {
        //                worksheetGraph.Cells["C4"].Value = MachRow.tblcell.CelldisplayName;
        //                worksheetGraph.Cells["C5"].Value = MachRow.MachineDisplayName;
        //            }
        //            var GetUtilList = Serverdb.tbl_ProdManMachine.Where(m => m.MachineID == MachRow.MachineID && m.CorrectedDate == QueryDate.Date).ToList();
        //            foreach (var ProdRow in GetUtilList)
        //            {
        //                //Total Values
        //                if (ProdRow.tblworkorderentry != null)
        //                {
        //                    TotalOperatingTime += (double)ProdRow.TotalOperatingTime;
        //                    TotalDownTime += (double)ProdRow.TotalLoss + (double)ProdRow.TotalMinorLoss + (double)ProdRow.TotalSetup;
        //                    TotalAcceptedQty += ProdRow.tblworkorderentry.Yield_Qty;
        //                    TotalRejectedQty += ProdRow.tblworkorderentry.ScrapQty;
        //                    TotalPerformanceFactor += ProdRow.PerfromaceFactor;
        //                    //Day Values
        //                    DayOperatingTime += (double)ProdRow.TotalOperatingTime;
        //                    DayDownTime += (double)ProdRow.TotalLoss + (double)ProdRow.TotalMinorLoss;
        //                    DayAcceptedQty += ProdRow.tblworkorderentry.Yield_Qty;
        //                    DayRejectedQty += ProdRow.tblworkorderentry.ScrapQty;
        //                    DayPerformanceFactor += ProdRow.PerfromaceFactor;
        //                }
        //                else { }
        //            }
        //            var GetLossList = Serverdb.tbl_ProdOrderLosses.Where(m => m.MachineID == MachRow.MachineID && m.CorrectedDate == QueryDate.Date).ToList();

        //            foreach (var LossRow in GetLossList)
        //            {
        //                var getrow = (from DataRow row in LossTbl.Rows where row.Field<int>("LossID") == LossRow.LossID select row["LossID"]).FirstOrDefault();
        //                if (getrow == null)
        //                {
        //                    var GetLossTargetPercent = "1%";
        //                    String GetLossName = null;
        //                    var GetLossTarget = Serverdb.tbllossescodes.Where(m => m.LossCodeID == LossRow.LossID).FirstOrDefault();
        //                    if (GetLossTarget != null)
        //                    {
        //                        GetLossTargetPercent = GetLossTarget.TargetPercent.ToString();
        //                        GetLossName = GetLossTarget.LossCode;
        //                    }

        //                    LossTbl.Rows.Add(LossRow.LossID, LossRow.LossDuration, GetLossTargetPercent, GetLossName);
        //                }
        //                else
        //                {
        //                    foreach (DataRow GetRow in LossTbl.Rows)
        //                    {
        //                        if (Convert.ToInt32(GetRow["LossID"]) == LossRow.LossID)
        //                        {
        //                            long LossDura = Convert.ToInt32(GetRow["LossDuration"]);
        //                            LossDura += LossRow.LossDuration;
        //                            GetRow["LossDuration"] = LossDura;
        //                        }
        //                    }

        //                }
        //            }

        //        }
        //        int TotQty = (int)(DayAcceptedQty + DayRejectedQty);
        //        if (TotQty == 0)
        //            TotQty = 1;

        //        double DayOpTime = DayOperatingTime;
        //        if (DayOpTime == 0)
        //            DayOpTime = 1;

        //        decimal DayAvailPercent = (decimal)Math.Round(DayOperatingTime / (24 * MachineCount), 2);
        //        decimal DayPerformancePercent = (decimal)Math.Round(DayPerformanceFactor / DayOpTime, 2);
        //        decimal DayQualityPercent = (decimal)Math.Round((DayAcceptedQty / (TotQty)), 2);
        //        decimal DayOEEPercent = (decimal)Math.Round((double)(DayAvailPercent) * (double)(DayPerformancePercent) * (double)(DayQualityPercent), 2);

        //        worksheetGraph.Cells["B" + StartGrpah1].Value = QueryDate.ToString("dd-MM-yyyy");
        //        worksheetGraph.Cells["C" + StartGrpah1].Value = 0.88;
        //        worksheetGraph.Cells["D" + StartGrpah1].Value = (DayOEEPercent / 100) / 100;

        //        StartGrpah1++;
        //    }
        //    worksheetGraph.Cells["E11"].Value = (double)Math.Round(TotalOperatingTime / 60, 2);
        //    worksheetGraph.Cells["E12"].Value = (double)Math.Round(TotalDownTime / 60, 2);
        //    worksheetGraph.Cells["E13"].Value = TotalAcceptedQty;
        //    worksheetGraph.Cells["E14"].Value = TotalRejectedQty;

        //    decimal TotalQualityPercent1 = 0, TotalOEEPercent1 = 0, TotalAvailPercent1 = 0, TotalPerformancePercent1 = 0;

        //    if (TotalAcceptedQty != 0 && TotalRejectedQty != 0)
        //    {
        //        TotalAvailPercent1 = (decimal)Math.Round(TotalOperatingTime / (WorkingDays * 24 * 60 * MachineCount), 2);
        //        TotalPerformancePercent1 = (decimal)Math.Round(TotalPerformanceFactor / TotalOperatingTime, 2);
        //        TotalQualityPercent1 = (decimal)Math.Round((TotalAcceptedQty / (TotalAcceptedQty + TotalRejectedQty)), 2);
        //        TotalOEEPercent1 = (decimal)Math.Round((double)(TotalAvailPercent) * (double)(TotalPerformancePercent) * (double)(TotalQualityPercent), 2);
        //    }


        //    if (TotalAcceptedQty != 0 && TotalRejectedQty != 0)
        //    {
        //        worksheetGraph.Cells["E20"].Value = TotalAvailPercent1;
        //        worksheetGraph.Cells["E21"].Value = TotalPerformancePercent1;
        //        worksheetGraph.Cells["E22"].Value = TotalQualityPercent1;
        //        worksheetGraph.Cells["E23"].Value = TotalOEEPercent1;
        //        worksheetGraph.Cells["G5"].Value = TotalOEEPercent1;
        //        worksheetGraph.View.ShowGridLines = false;
        //    }
        //    else
        //    {
        //        int diff = dateDifference + 1;
        //        worksheetGraph.Cells["E20"].Value = (TotalAvailPercent / 100) / diff;
        //        worksheetGraph.Cells["E21"].Value = (TotalPerformancePercent / 100) / diff;
        //        worksheetGraph.Cells["E22"].Value = (TotalQualityPercent / 100) / diff;
        //        worksheetGraph.Cells["E23"].Value = (TotalOEEPercent / 100) / diff;
        //        worksheetGraph.Cells["G5"].Value = (TotalOEEPercent / 100) / diff;
        //        worksheetGraph.View.ShowGridLines = false;
        //    }


        //    DateTime fromDate = Convert.ToDateTime(FromDate);
        //    DateTime toDate = Convert.ToDateTime(ToDate);
        //    var top3ContrubutingFactors = (from dbItem in Serverdb.tbl_ProdOrderLosses
        //                                   where dbItem.CorrectedDate >= fromDate.Date && dbItem.CorrectedDate <= toDate.Date
        //                                   group dbItem by dbItem.LossID into x
        //                                   select new
        //                                   {
        //                                       LossId = x.Key,
        //                                       LossDuration = Serverdb.tbl_ProdOrderLosses.Where(m => m.LossID == x.Key).Select(m => m.LossDuration).Sum()
        //                                   }).ToList();
        //    var item = top3ContrubutingFactors.OrderByDescending(m => m.LossDuration).Take(3).ToList();
        //    int lossXccelNo = 29;
        //    foreach (var GetRow in item)
        //    {
        //        string lossCode = Serverdb.tbllossescodes.Where(m => m.LossCodeID == GetRow.LossId).Select(m => m.LossCodeDesc).FirstOrDefault();
        //        decimal lossPercentage = (decimal)Math.Round(((GetRow.LossDuration) / TotalDownTime), 2);
        //        decimal lossDurationInHours = (decimal)Math.Round((GetRow.LossDuration / 60.00), 2);
        //        worksheetGraph.Cells["L" + lossXccelNo].Value = lossCode;
        //        worksheetGraph.Cells["N" + lossXccelNo].Value = lossPercentage;
        //        worksheetGraph.Cells["O" + lossXccelNo].Value = lossDurationInHours;

        //        lossXccelNo++;
        //    }

        //    int grphData = 5;
        //    decimal CumulativePercentage = 0;
        //    foreach (var data in top3ContrubutingFactors)
        //    {
        //        var dbLoss = Serverdb.tbllossescodes.Where(m => m.LossCodeID == data.LossId).FirstOrDefault();
        //        string lossCode = dbLoss.LossCodeDesc;
        //        decimal Target = dbLoss.TargetPercent;
        //        decimal actualPercentage = (decimal)Math.Round(((data.LossDuration) / TotalDownTime), 2);
        //        CumulativePercentage = CumulativePercentage + actualPercentage;
        //        worksheetGraph.Cells["K" + grphData].Value = lossCode;
        //        worksheetGraph.Cells["L" + grphData].Value = Target;
        //        worksheetGraph.Cells["M" + grphData].Value = actualPercentage;
        //        worksheetGraph.Cells["N" + grphData].Value = CumulativePercentage;
        //        grphData++;
        //    }

        //    //Code written on 05-10-2018
        //    int col = 12, col1 = 12, col2 = 13, col3 = 14;

        //    foreach (var GetRow in item)
        //    {
        //        string lossCode = Serverdb.tbllossescodes.Where(m => m.LossCodeID == GetRow.LossId).Select(m => m.LossCode).FirstOrDefault();

        //        string columnNumber = ExcelColumnFromNumber(col);
        //        worksheetGraph.Cells[columnNumber + 36].Value = lossCode;

        //        int macLossNo = 38;

        //        DataTable dt = new DataTable();
        //        MsqlConnection mc = new MsqlConnection();
        //        mc.open();
        //        string query = "SELECT TOP 3 SUM(tpol.LossDuration),tblma.MachineName,tbllo.LossCode,tpol.CorrectedDate from unitworksccs.unitworkccs.tbl_ProdOrderLosses tpol inner join unitworkccs.tbllossescodes tbllo on tbllo.LossCodeID = tpol.LossID inner join unitworkccs.tblmachinedetails tblma on tblma.MachineID = tpol.MachineID where tpol.CorrectedDate >= '" + fromDate.Date + "' and tpol.CorrectedDate <= '" + toDate.Date + "' AND tbllo.LossCode = '" + lossCode + "' " +
        //            "group by tbllo.LossCode,tblma.MachineName,tpol.LossDuration, tpol.CorrectedDate order by tpol.LossDuration DESC";
        //        SqlDataAdapter sdt = new SqlDataAdapter(query, mc.msqlConnection);
        //        sdt.Fill(dt);
        //        mc.close();

        //        for (int i = 0; i < dt.Rows.Count; i++)
        //        {
        //            double value = Convert.ToDouble(dt.Rows[i][0]);
        //            string macName = Convert.ToString(dt.Rows[i][1]);

        //            decimal lossPercentage = (decimal)Math.Round(value / TotalDownTime, 2);
        //            decimal lossDurationInHours = (decimal)Math.Round(value / 60.00, 2);

        //            string colNum1 = ExcelColumnFromNumber(col1);
        //            worksheetGraph.Cells[colNum1 + macLossNo].Value = macName;
        //            string colNum2 = ExcelColumnFromNumber(col2);
        //            worksheetGraph.Cells[colNum2 + macLossNo].Value = lossPercentage;
        //            string colNum3 = ExcelColumnFromNumber(col3);
        //            worksheetGraph.Cells[colNum3 + macLossNo].Value = lossDurationInHours;

        //            macLossNo++;
        //        }

        //        col += 4; col1 += 4; col2 += 4; col3 += 4;
        //    }
        //    //Code ended on 05-10-2018

        //    //Code Written on 09-10-2018
        //    for (int i = 0; i < dateDifference; i++)
        //    {
        //        DateTime QueryDate = Convert.ToDateTime(FromDate).AddDays(i);
        //        foreach (var Machine in getMachineList)
        //        {
        //            DataTable dt = new DataTable();
        //            try
        //            {
        //                using (MsqlConnection mc = new MsqlConnection())
        //                {
        //                    using (SqlCommand cmd = new SqlCommand("InsertOEEReportDivision", mc.msqlConnection))
        //                    {
        //                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
        //                        {
        //                            mc.open();
        //                            cmd.CommandType = CommandType.StoredProcedure;
        //                            cmd.Parameters.AddWithValue("@MachineID", Machine.MachineID);
        //                            cmd.Parameters.AddWithValue("@CorrectedDate", QueryDate.Date);
        //                            sda.Fill(dt);
        //                            mc.close();
        //                        }
        //                    }
        //                }
        //            }
        //            catch (Exception ex) { }
        //        }
        //    }

        //    var topContributingFactors = (from dbItem in Serverdb.tbl_oeereportasdivision
        //                                  where dbItem.CorrectedDate >= fromDate.Date && dbItem.CorrectedDate <= toDate.Date
        //                                  group dbItem by new { dbItem.LossID, dbItem.FGCode } into x
        //                                  select new
        //                                  {
        //                                      x.Key.LossID,
        //                                      x.Key.FGCode,
        //                                      LossDuration = x.Select(m => m.LossDuration).Sum(),
        //                                  }).ToList();


        //    if (CellID != 0)
        //    {
        //        var getCellName = Serverdb.tblcells.Where(m => m.CellID == CellID).Select(m => m.CellName).FirstOrDefault();
        //        // worksheetGraph.Cells["K46"].Value = getCellName;
        //    }


        //    var getValues = topContributingFactors.OrderByDescending(m => m.LossDuration).ThenBy(m => m.LossID).ToList();
        //    var distinctLoss = getValues.Select(m => m.LossID).Distinct().Take(10).ToList();
        //    int colNum = 48;
        //    for (int i = 0; i < distinctLoss.Count; i++)
        //    {
        //        int colVal1 = 12, colVal2 = 13;
        //        var getLossId = distinctLoss[i];
        //        string losscode = Serverdb.tbllossescodes.Where(m => m.LossCodeID == getLossId).Select(m => m.LossCodeDesc).FirstOrDefault();
        //        worksheetGraph.Cells["K" + colNum].Value = losscode;

        //        var top3AccToLoss = getValues.Where(m => m.LossID == getLossId).OrderByDescending(m => m.LossDuration).Take(3).ToList();
        //        foreach (var data in top3AccToLoss)
        //        {
        //            var FGcode = data.FGCode;
        //            decimal LossDurationinHours = (decimal)Math.Round((data.LossDuration) / 60.00, 2);

        //            string colName1 = ExcelColumnFromNumber(colVal1);
        //            worksheetGraph.Cells[colName1 + colNum].Value = FGcode;
        //            string colName2 = ExcelColumnFromNumber(colVal2);
        //            worksheetGraph.Cells[colName2 + colNum].Value = LossDurationinHours;
        //            colVal1 += 2; colVal2 += 2;
        //        }
        //        colNum++;
        //    }
        //    //Code written on 09-10-2018

        //    #region
        //    //var chartIDAndUnID = (ExcelBarChart)worksheetGraph.Drawings.AddChart("Testing", eChartType.ColumnClustered);

        //    //chartIDAndUnID.SetSize((350), 550);

        //    //chartIDAndUnID.SetPosition(50, 60);

        //    //chartIDAndUnID.Title.Text = "AB Graph ";
        //    //chartIDAndUnID.Style = eChartStyle.Style18;
        //    //chartIDAndUnID.Legend.Position = eLegendPosition.Bottom;
        //    ////chartIDAndUnID.Legend.Remove();
        //    //chartIDAndUnID.YAxis.MaxValue = 100;
        //    //chartIDAndUnID.YAxis.MinValue = 0;
        //    //chartIDAndUnID.YAxis.MajorUnit = 5;

        //    //chartIDAndUnID.Locked = false;
        //    //chartIDAndUnID.PlotArea.Border.Width = 0;
        //    //chartIDAndUnID.YAxis.MinorTickMark = eAxisTickMark.None;
        //    //chartIDAndUnID.DataLabel.ShowValue = true;
        //    //chartIDAndUnID.DisplayBlanksAs = eDisplayBlanksAs.Gap;


        //    //ExcelRange dateWork = worksheetGraph.Cells["K33:" + lossXccelNo];
        //    //ExcelRange hoursWork = worksheetGraph.Cells["N33:" + lossXccelNo];
        //    //var hours = (ExcelChartSerie)(chartIDAndUnID.Series.Add(hoursWork, dateWork));
        //    //hours.Header = "Operating Time (Hours)";
        //    ////Get reference to the worksheet xml for proper namespace
        //    //var chartXml = chartIDAndUnID.ChartXml;
        //    //var nsuri = chartXml.DocumentElement.NamespaceURI;
        //    //var nsm = new XmlNamespaceManager(chartXml.NameTable);
        //    //nsm.AddNamespace("c", nsuri);

        //    ////XY Scatter plots have 2 value axis and no category
        //    //var valAxisNodes = chartXml.SelectNodes("c:chartSpace/c:chart/c:plotArea/c:valAx", nsm);
        //    //if (valAxisNodes != null && valAxisNodes.Count > 0)
        //    //    foreach (XmlNode valAxisNode in valAxisNodes)
        //    //    {
        //    //        var major = valAxisNode.SelectSingleNode("c:majorGridlines", nsm);
        //    //        if (major != null)
        //    //            valAxisNode.RemoveChild(major);

        //    //        var minor = valAxisNode.SelectSingleNode("c:minorGridlines", nsm);
        //    //        if (minor != null)
        //    //            valAxisNode.RemoveChild(minor);
        //    //    }

        //    ////Other charts can have a category axis
        //    //var catAxisNodes = chartXml.SelectNodes("c:chartSpace/c:chart/c:plotArea/c:catAx", nsm);
        //    //if (catAxisNodes != null && catAxisNodes.Count > 0)
        //    //    foreach (XmlNode catAxisNode in catAxisNodes)
        //    //    {
        //    //        var major = catAxisNode.SelectSingleNode("c:majorGridlines", nsm);
        //    //        if (major != null)
        //    //            catAxisNode.RemoveChild(major);

        //    //        var minor = catAxisNode.SelectSingleNode("c:minorGridlines", nsm);
        //    //        if (minor != null)
        //    //            catAxisNode.RemoveChild(minor);
        //    //    }
        //    //worksheetGraph.View["L29"]
        //    //worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        //    #endregion
        //    //worksheet.Column(29).Width = 12;
        //    p.Save();

        //    //Downloding Excel
        //    string path1 = System.IO.Path.Combine(FileDir, "OEE_Report" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx");
        //    DownloadUtilReport(path1, "OEE_Report", ToDate);

        //    ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", PlantID);
        //    ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID), "ShopID", "ShopName", ShopID);
        //    ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID), "CellID", "CellName", CellID);
        //    ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID && m.CellID == CellID), "MachineID", "MachineDisplayName", MachineID);
        //    return View();
        //}

        #endregion
        #region TVS
        //[HttpPost]
        //public ActionResult OEEReport(int PlantID, String FromDate, String ToDate, int ShopID = 0, int CellID = 0, int MachineID = 0)
        //{
        //    //ReportsCalcClass.ProdDetAndon UR = new ReportsCalcClass.ProdDetAndon();
        //    ReportsCalcClass.OEEReportCalculations OEC = new ReportsCalcClass.OEEReportCalculations();
        //    double AvailabilityPercentage = 0;
        //    double PerformancePercentage = 0;
        //    double QualityPercentage = 0;
        //    double OEEPercentage = 0;
        //    // OEC.GETCYCLETIMEAnalysis(MachineID, FromDate);


        //    var getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0).ToList();

        //    if (MachineID != 0)
        //    {
        //        getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID).ToList();
        //    }
        //    else if (CellID != 0)
        //    {
        //        getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == CellID).ToList();
        //    }
        //    else if (ShopID != 0)
        //    {
        //        getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID).ToList();
        //    }

        //    int dateDifference = Convert.ToDateTime(ToDate).Subtract(Convert.ToDateTime(FromDate)).Days;

        //    FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\OEE_Report.xlsx");

        //    ExcelPackage templatep = new ExcelPackage(templateFile);
        //    ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];
        //    ExcelWorksheet TemplateGraph = templatep.Workbook.Worksheets[2];

        //    String FileDir = @"C:\SRKS_ifacility\ReportsList\" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd");
        //    bool exists = System.IO.Directory.Exists(FileDir);
        //    if (!exists)
        //        System.IO.Directory.CreateDirectory(FileDir);

        //    FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "OEE_Report" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
        //    if (newFile.Exists)
        //    {
        //        try
        //        {
        //            newFile.Delete();  // ensures we create a new workbook
        //            newFile = new FileInfo(System.IO.Path.Combine(FileDir, "OEE_Report" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx"));
        //        }
        //        catch
        //        {
        //            TempData["Excelopen"] = "Excel with same date is already open, please close it and try to generate!!!!";
        //            //return View();
        //        }
        //    }
        //    //Using the File for generation and populating it
        //    ExcelPackage p = null;
        //    p = new ExcelPackage(newFile);
        //    ExcelWorksheet worksheet = null;
        //    ExcelWorksheet worksheetGraph = null;

        //    //Creating the WorkSheet for populating
        //    try
        //    {
        //        worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy"), Templatews);
        //        worksheetGraph = p.Workbook.Worksheets.Add("Graphs", TemplateGraph);
        //    }
        //    catch { }

        //    if (worksheet == null)
        //    {
        //        worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy") + "1", Templatews);
        //        worksheetGraph = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy") + "Graph", TemplateGraph);
        //    }
        //    else if (worksheetGraph == null)
        //    {
        //        worksheetGraph = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy") + "Graph", TemplateGraph);
        //    }
        //    int sheetcount = p.Workbook.Worksheets.Count;
        //    p.Workbook.Worksheets.MoveToStart(sheetcount);
        //    worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        //    worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
        //    int StartRow = 2;
        //    //int SlNo = 1;
        //    int MachineCount = getMachineList.Count;
        //    int Startcolumn = 11;
        //    String ColNam = ExcelColumnFromNumber(Startcolumn);
        //    var GetMainLossList = Serverdb.tbllossescodes.Where(m => m.LossCodesLevel == 1 && m.IsDeleted == 0 && m.MessageType != "SETUP").OrderBy(m => m.LossCodeID).ToList();

        //    foreach (var LossRow in GetMainLossList)
        //    {
        //        ColNam = ExcelColumnFromNumber(Startcolumn);
        //        worksheet.Cells[ColNam + "1"].Value = LossRow.LossCodeDesc;
        //        Startcolumn++;
        //    }

        //    //Tabular sheet Data Population
        //    for (int i = 0; i <= dateDifference; i++)
        //    {
        //        DateTime QueryDate = Convert.ToDateTime(FromDate).AddDays(i);
        //        string CorrectedDate = QueryDate.ToString("yyyy-MM-dd");
        //        foreach (var Machine in getMachineList)
        //        {
        //            OEC.OEE(Machine.MachineID, CorrectedDate);
        //            int MacStartcolumn = 11;
        //            var GetUtilList = Serverdb.tbl_OEEDetails.Where(m => m.MachineID == Machine.MachineID && m.CorrectedDate == CorrectedDate).ToList();
        //            foreach (var MacRow in GetUtilList)
        //            {
        //                var partdet = Serverdb.tblparts.Where(m => m.MachineID == MacRow.MachineID).FirstOrDefault();
        //                worksheet.Cells["A" + StartRow].Value = MacRow.tblmachinedetail.MachineName;
        //                if (partdet != null)
        //                {
        //                    worksheet.Cells["B" + StartRow].Value = partdet.OperationNo;

        //                    worksheet.Cells["C" + StartRow].Value = partdet.FGCode;
        //                }
        //                worksheet.Cells["D" + StartRow].Value = CorrectedDate;
        //                worksheet.Cells["E" + StartRow].Value = MacRow.OperatingTimeinMin;
        //                worksheet.Cells["F" + StartRow].Value = MacRow.Availability;
        //                worksheet.Cells["G" + StartRow].Value = MacRow.Quality;
        //                if (MacRow.Performance > 100)
        //                    MacRow.Performance = 100;
        //                worksheet.Cells["H" + StartRow].Value = MacRow.Performance;
        //                worksheet.Cells["I" + StartRow].Value = MacRow.OEE;
        //                worksheet.Cells["J" + StartRow].Value = MacRow.TotalPartsCount;
        //                //worksheet.Cells["L" + StartRow].Value = "";
        //                // worksheet.Cells["K" + StartRow].Value = MacRow.TotalSetup;
        //                //        int TotalQty = MacRow.tblworkorderentry.Yield_Qty + MacRow.tblworkorderentry.ScrapQty;
        //                //        if (TotalQty == 0)
        //                //            TotalQty = 1;
        //                //        worksheet.Cells["K1"].Value = "Setup Time";
        //                //        worksheet.Cells["L1"].Value = "Rejections";
        //                //        worksheet.Cells["L" + StartRow].Value = (MacRow.OperatingTimeinMin / TotalQty) * MacRow.tblworkorderentry.ScrapQty;
        //                //long MacTotalLoss = 0;
        //                //foreach (var LossRow in GetMainLossList)
        //                //{
        //                //    var getWoLossList1 = Serverdb.tbl_ProdOrderLosses.Where(m => m.WOID == MacRow.WOID && m.LossID == LossRow.LossCodeID).FirstOrDefault();
        //                //    String ColEntry = ExcelColumnFromNumber(MacStartcolumn);
        //                //    if (getWoLossList1 != null)
        //                //    {
        //                //        worksheet.Cells[ColEntry + "" + StartRow].Value = getWoLossList1.LossDuration;
        //                //        MacTotalLoss += getWoLossList1.LossDuration;
        //                //    }
        //                //    else
        //                //        worksheet.Cells[ColEntry + "" + StartRow].Value = 0;
        //                //    MacStartcolumn++;
        //                //}
        //                //        String ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                //        worksheet.Cells[ColEntry1 + "1"].Value = "No Power";
        //                //        worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.TotalPowerLoss;
        //                //        MacStartcolumn++;

        //                //        ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                //        worksheet.Cells[ColEntry1 + "1"].Value = "Total Part";
        //                //        worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.tblworkorderentry.Total_Qty;
        //                //        MacStartcolumn++;

        //                //        ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                //        worksheet.Cells[ColEntry1 + "1"].Value = "Load / Unload";
        //                //        worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.TotalMinorLoss - MacRow.TotalSetupMinorLoss;
        //                //        MacStartcolumn++;

        //                //        ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                //        worksheet.Cells[ColEntry1 + "1"].Value = "Shift";
        //                //        worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.tblworkorderentry.ShiftID;
        //                //        MacStartcolumn++;

        //                //        ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                //        worksheet.Cells[ColEntry1 + "1"].Value = "Operator ID";
        //                //        worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.tblworkorderentry.OperatorID;
        //                //        MacStartcolumn++;

        //                //        ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                //        worksheet.Cells[ColEntry1 + "1"].Value = "Total OEE Loss";
        //                //        worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacTotalLoss;
        //                //        MacStartcolumn++;

        //                //        decimal OEEPercent = (decimal)Math.Round((double)(MacRow.UtilPercent / 100) * (double)(MacRow.PerformancePerCent / 100) * (double)(MacRow.QualityPercent / 100) * 100, 2);

        //                //        ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                //        worksheet.Cells[ColEntry1 + "1"].Value = "% of OEE";
        //                //        worksheet.Cells[ColEntry1 + "" + StartRow].Value = OEEPercent;
        //                //        MacStartcolumn++;
        //                StartRow++;
        //            }
        //        }
        //    }

        //    DataTable LossTbl = new DataTable();
        //    LossTbl.Columns.Add("LossID", typeof(int));
        //    LossTbl.Columns.Add("LossDuration", typeof(int));
        //    LossTbl.Columns.Add("LossTarget", typeof(string));
        //    LossTbl.Columns.Add("LossName", typeof(string));
        //    LossTbl.Columns.Add("LossActual", typeof(string));

        //    //Graph Sheet Population
        //    //Start Date and End Date
        //    worksheetGraph.Cells["C6"].Value = Convert.ToDateTime(FromDate).ToString("dd-MM-yyyy");
        //    worksheetGraph.Cells["E6"].Value = Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy");
        //    int GetHolidays = getsundays(Convert.ToDateTime(ToDate), Convert.ToDateTime(FromDate));
        //    int WorkingDays = dateDifference - GetHolidays + 1;
        //    //Working Days
        //    worksheetGraph.Cells["E5"].Value = WorkingDays;
        //    //Planned Production Time
        //    worksheetGraph.Cells["E10"].Value = WorkingDays * 24 * MachineCount;
        //    double TotalOperatingTime = 0;
        //    double TotalDownTime = 0;
        //    double TotalAcceptedQty = 0;
        //    double TotalRejectedQty = 0;
        //    double TotalPerformanceFactor = 0;
        //    int StartGrpah1 = 48;
        //    for (int i = 0; i <= dateDifference; i++)
        //    {
        //        double DayOperatingTime = 0;
        //        double DayDownTime = 0;
        //        double DayAcceptedQty = 0;
        //        double DayRejectedQty = 0;
        //        double DayPerformanceFactor = 0;
        //        DateTime QueryDate = Convert.ToDateTime(FromDate).AddDays(i);
        //        string CorrectedDate = QueryDate.ToString("yyyy-MM-dd");
        //        var plantName = Serverdb.tblplants.Where(m => m.PlantID == PlantID).Select(m => m.PlantName).FirstOrDefault();
        //        worksheetGraph.Cells["C3"].Value = plantName;
        //        foreach (var MachRow in getMachineList)
        //        {
        //            if (MachineID == 0)
        //            {
        //                worksheetGraph.Cells["C4"].Value = MachRow.tblcell.CelldisplayName;
        //                worksheetGraph.Cells["C5"].Value = "AS DIVISION";
        //            }
        //            else
        //            {
        //                worksheetGraph.Cells["C4"].Value = MachRow.tblcell.CelldisplayName;
        //                worksheetGraph.Cells["C5"].Value = MachRow.MachineDisplayName;
        //            }
        //            var GetUtilList = Serverdb.tbl_OEEDetails.Where(m => m.MachineID == MachRow.MachineID && m.CorrectedDate == CorrectedDate).ToList();
        //            foreach (var ProdRow in GetUtilList)
        //            {
        //                //Total Values
        //                TotalOperatingTime += (double)ProdRow.OperatingTimeinMin;
        //                TotalDownTime += (double)ProdRow.TotalIDLETimeinMin;
        //                TotalAcceptedQty += Convert.ToInt32(ProdRow.TotalPartsCount);
        //                // TotalRejectedQty += ProdRow.tblworkorderentry.ScrapQty;
        //                TotalPerformanceFactor += (double)ProdRow.PerformanceFactor;
        //                //Day Values
        //                DayOperatingTime += (double)ProdRow.OperatingTimeinMin;
        //                DayDownTime += (double)ProdRow.TotalIDLETimeinMin;
        //                DayAcceptedQty += Convert.ToInt32(ProdRow.TotalPartsCount);
        //                // DayRejectedQty += ProdRow.tblworkorderentry.ScrapQty;
        //                DayPerformanceFactor += (double)ProdRow.PerformanceFactor;
        //            }
        //            var GetLossList = Serverdb.tbl_ProdOrderLosses.Where(m => m.MachineID == MachRow.MachineID && m.CorrectedDate == QueryDate.Date).ToList();

        //            foreach (var LossRow in GetLossList)
        //            {
        //                var getrow = (from DataRow row in LossTbl.Rows where row.Field<int>("LossID") == LossRow.LossID select row["LossID"]).FirstOrDefault();
        //                if (getrow == null)
        //                {
        //                    var GetLossTargetPercent = "1%";
        //                    String GetLossName = null;
        //                    var GetLossTarget = Serverdb.tbllossescodes.Where(m => m.LossCodeID == LossRow.LossID).FirstOrDefault();
        //                    if (GetLossTarget != null)
        //                    {
        //                        GetLossTargetPercent = GetLossTarget.TargetPercent.ToString();
        //                        GetLossName = GetLossTarget.LossCodeDesc;
        //                    }

        //                    LossTbl.Rows.Add(LossRow.LossID, LossRow.LossDuration, GetLossTargetPercent, GetLossName);
        //                }
        //                else
        //                {
        //                    foreach (DataRow GetRow in LossTbl.Rows)
        //                    {
        //                        if (Convert.ToInt32(GetRow["LossID"]) == LossRow.LossID)
        //                        {
        //                            long LossDura = Convert.ToInt32(GetRow["LossDuration"]);
        //                            LossDura += LossRow.LossDuration;
        //                            GetRow["LossDuration"] = LossDura;
        //                        }
        //                    }

        //                }
        //            }
        //        }
        //        int TotQty = (int)(DayAcceptedQty + DayRejectedQty);
        //        if (TotQty == 0)
        //            TotQty = 1;

        //        double DayOpTime = DayOperatingTime;
        //        if (DayOpTime == 0)
        //            DayOpTime = 1;

        //        decimal DayAvailPercent = (decimal)Math.Round(DayOperatingTime / (24 * MachineCount), 2);
        //        decimal DayPerformancePercent = (decimal)Math.Round(DayPerformanceFactor / DayOpTime, 2);
        //        decimal DayQualityPercent = (decimal)Math.Round((DayAcceptedQty / (TotQty)), 2);
        //        decimal DayOEEPercent = (decimal)Math.Round((double)(DayAvailPercent) * (double)(DayPerformancePercent) * (double)(DayQualityPercent), 2);

        //        worksheetGraph.Cells["B" + StartGrpah1].Value = QueryDate.ToString("dd-MM-yyyy");
        //        worksheetGraph.Cells["C" + StartGrpah1].Value = 0.85;
        //        worksheetGraph.Cells["D" + StartGrpah1].Value = DayOEEPercent;

        //        StartGrpah1++;
        //    }
        //    worksheetGraph.Cells["E11"].Value = (double)Math.Round(TotalOperatingTime / 60, 2);
        //    worksheetGraph.Cells["E12"].Value = (double)Math.Round(TotalDownTime / 60, 2);
        //    worksheetGraph.Cells["E13"].Value = TotalAcceptedQty;
        //    worksheetGraph.Cells["E14"].Value = TotalRejectedQty;

        //    if (TotalOperatingTime == 0)
        //        TotalOperatingTime = 1;
        //    if (TotalAcceptedQty == 0)
        //        TotalAcceptedQty = 1;
        //    decimal TotalAvailPercent = (decimal)Math.Round(TotalOperatingTime / (WorkingDays * 24 * 60 * MachineCount), 2);
        //    decimal TotalPerformancePercent = (decimal)Math.Round(TotalPerformanceFactor / TotalOperatingTime, 2);
        //    decimal TotalQualityPercent = (decimal)Math.Round((TotalAcceptedQty / (TotalAcceptedQty + TotalRejectedQty)), 2);
        //    decimal TotalOEEPercent = (decimal)Math.Round((double)(TotalAvailPercent) * (double)(TotalPerformancePercent) * (double)(TotalQualityPercent), 2);

        //    if (TotalPerformancePercent > 100)
        //        TotalPerformancePercent = 100;
        //    worksheetGraph.Cells["E20"].Value = TotalAvailPercent;
        //    worksheetGraph.Cells["E21"].Value = TotalPerformancePercent;
        //    worksheetGraph.Cells["E22"].Value = TotalQualityPercent;
        //    worksheetGraph.Cells["E23"].Value = TotalOEEPercent;
        //    worksheetGraph.Cells["G5"].Value = TotalOEEPercent;
        //    worksheetGraph.View.ShowGridLines = false;

        //    DateTime fromDate = Convert.ToDateTime(FromDate);
        //    DateTime toDate = Convert.ToDateTime(ToDate);
        //    var top3ContrubutingFactors = (from dbItem in Serverdb.tbl_ProdOrderLosses
        //                                   where dbItem.CorrectedDate >= fromDate.Date && dbItem.CorrectedDate <= toDate.Date
        //                                   group dbItem by dbItem.LossID into x
        //                                   select new
        //                                   {
        //                                       LossId = x.Key,
        //                                       LossDuration = Serverdb.tbl_ProdOrderLosses.Where(m => m.LossID == x.Key).Select(m => m.LossDuration).Sum()
        //                                   }).ToList();
        //    var item = top3ContrubutingFactors.OrderByDescending(m => m.LossDuration).Take(3).ToList();
        //    int lossXccelNo = 29;
        //    decimal lossPercentage = 0;
        //    foreach (var GetRow in item)
        //    {
        //        string lossCode = Serverdb.tbllossescodes.Where(m => m.LossCodeID == GetRow.LossId).Select(m => m.LossCodeDesc).FirstOrDefault();
        //        if (TotalDownTime != 0)
        //            lossPercentage = (decimal)Math.Round(((GetRow.LossDuration) / TotalDownTime), 2);
        //        decimal lossDurationInHours = (decimal)Math.Round((GetRow.LossDuration / 60.00), 2);
        //        worksheetGraph.Cells["L" + lossXccelNo].Value = lossCode;
        //        worksheetGraph.Cells["N" + lossXccelNo].Value = lossPercentage;
        //        worksheetGraph.Cells["O" + lossXccelNo].Value = lossDurationInHours;
        //        lossXccelNo++;
        //    }

        //    int grphData = 5;
        //    decimal CumulativePercentage = 0;
        //    foreach (var data in top3ContrubutingFactors)
        //    {
        //        var dbLoss = Serverdb.tbllossescodes.Where(m => m.LossCodeID == data.LossId).FirstOrDefault();
        //        string lossCode = dbLoss.LossCodeDesc;
        //        decimal Target = dbLoss.TargetPercent;
        //        decimal actualPercentage = (decimal)Math.Round(((data.LossDuration) / TotalDownTime), 2);
        //        CumulativePercentage = CumulativePercentage + actualPercentage;
        //        worksheetGraph.Cells["K" + grphData].Value = lossCode;
        //        worksheetGraph.Cells["L" + grphData].Value = Target;
        //        worksheetGraph.Cells["M" + grphData].Value = actualPercentage;
        //        worksheetGraph.Cells["N" + grphData].Value = CumulativePercentage;
        //        grphData++;

        //    }

        //    //var chartIDAndUnID = (ExcelBarChart)worksheetGraph.Drawings.AddChart("Testing", eChartType.ColumnClustered);

        //    //chartIDAndUnID.SetSize((350), 550);

        //    //chartIDAndUnID.SetPosition(50, 60);

        //    //chartIDAndUnID.Title.Text = "AB Graph ";
        //    //chartIDAndUnID.Style = eChartStyle.Style18;
        //    //chartIDAndUnID.Legend.Position = eLegendPosition.Bottom;
        //    ////chartIDAndUnID.Legend.Remove();
        //    //chartIDAndUnID.YAxis.MaxValue = 100;
        //    //chartIDAndUnID.YAxis.MinValue = 0;
        //    //chartIDAndUnID.YAxis.MajorUnit = 5;

        //    //chartIDAndUnID.Locked = false;
        //    //chartIDAndUnID.PlotArea.Border.Width = 0;
        //    //chartIDAndUnID.YAxis.MinorTickMark = eAxisTickMark.None;
        //    //chartIDAndUnID.DataLabel.ShowValue = true;
        //    //chartIDAndUnID.DisplayBlanksAs = eDisplayBlanksAs.Gap;


        //    //ExcelRange dateWork = worksheetGraph.Cells["K33:" + lossXccelNo];
        //    //ExcelRange hoursWork = worksheetGraph.Cells["N33:" + lossXccelNo];
        //    //var hours = (ExcelChartSerie)(chartIDAndUnID.Series.Add(hoursWork, dateWork));
        //    //hours.Header = "Operating Time (Hours)";
        //    ////Get reference to the worksheet xml for proper namespace
        //    //var chartXml = chartIDAndUnID.ChartXml;
        //    //var nsuri = chartXml.DocumentElement.NamespaceURI;
        //    //var nsm = new XmlNamespaceManager(chartXml.NameTable);
        //    //nsm.AddNamespace("c", nsuri);

        //    ////XY Scatter plots have 2 value axis and no category
        //    //var valAxisNodes = chartXml.SelectNodes("c:chartSpace/c:chart/c:plotArea/c:valAx", nsm);
        //    //if (valAxisNodes != null && valAxisNodes.Count > 0)
        //    //    foreach (XmlNode valAxisNode in valAxisNodes)
        //    //    {
        //    //        var major = valAxisNode.SelectSingleNode("c:majorGridlines", nsm);
        //    //        if (major != null)
        //    //            valAxisNode.RemoveChild(major);

        //    //        var minor = valAxisNode.SelectSingleNode("c:minorGridlines", nsm);
        //    //        if (minor != null)
        //    //            valAxisNode.RemoveChild(minor);
        //    //    }

        //    ////Other charts can have a category axis
        //    //var catAxisNodes = chartXml.SelectNodes("c:chartSpace/c:chart/c:plotArea/c:catAx", nsm);
        //    //if (catAxisNodes != null && catAxisNodes.Count > 0)
        //    //    foreach (XmlNode catAxisNode in catAxisNodes)
        //    //    {
        //    //        var major = catAxisNode.SelectSingleNode("c:majorGridlines", nsm);
        //    //        if (major != null)
        //    //            catAxisNode.RemoveChild(major);

        //    //        var minor = catAxisNode.SelectSingleNode("c:minorGridlines", nsm);
        //    //        if (minor != null)
        //    //            catAxisNode.RemoveChild(minor);
        //    //    }
        //    //worksheetGraph.View["L29"]
        //    //worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        //    p.Save();

        //    //Downloding Excel
        //    string path1 = System.IO.Path.Combine(FileDir, "OEE_Report" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx");
        //    DownloadUtilReport(path1, "OEE_Report", ToDate);

        //    ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", PlantID);
        //    ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID), "ShopID", "ShopName", ShopID);
        //    ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID), "CellID", "CellName", CellID);
        //    ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID && m.CellID == CellID), "MachineID", "MachineDisplayName", MachineID);
        //    return View();
        //}
        #endregion

        #region  TATA OEE Report
        public void OEEReportExcel(string StartDate, string EndDate, string TimeFactor, string ProdFAI, string PlantID, string ShopID = null, string CellID = null, string WorkCenterID = null, string TabularType = "Day")
        {
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj2 = new Dao1(_conn);
            obj = new ReportsDao(_conn);
            DateTime frda = DateTime.Now;
            if (string.IsNullOrEmpty(StartDate) == true)
            {
                StartDate = DateTime.Now.Date.ToString();
            }
            if (string.IsNullOrEmpty(EndDate) == true)
            {
                EndDate = StartDate;
            }

            DateTime frmDate = Convert.ToDateTime(StartDate);
            DateTime toDate = Convert.ToDateTime(EndDate);

            double TotalDay = toDate.Subtract(frmDate).TotalDays;

            FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\OEEReportGodHours.xlsx");
            if (TimeFactor == "GH")
            {
                templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\OEEReportGodHours.xlsx");
            }
            else if (TimeFactor == "NoBlue")
            {
                templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\OEEReportAdjusted.xlsx");
            }

            ExcelPackage templatep = new ExcelPackage(templateFile);
            ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];
            ExcelWorksheet TemplateGraph = templatep.Workbook.Worksheets[2];

            String FileDir = @"C:\SRKS_ifacility\ReportsList\" + System.DateTime.Now.ToString("yyyy-MM-dd");
            bool exists = System.IO.Directory.Exists(FileDir);
            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "OEEReportGodHours" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (TimeFactor == "GH")
            {
                newFile = new FileInfo(System.IO.Path.Combine(FileDir, "OEEReportGodHours" + frda.ToString("yyyy-MM-dd") + ".xlsx"));
            }
            else if (TimeFactor == "NoBlue")
            {
                newFile = new FileInfo(System.IO.Path.Combine(FileDir, "OEEReportAdjusted" + frda.ToString("yyyy-MM-dd") + ".xlsx"));
            }
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "OEEReportGodHours" + frda.ToString("yyyy-MM-dd") + ".xlsx"));
                    if (TimeFactor == "GH")
                    {
                        newFile = new FileInfo(System.IO.Path.Combine(FileDir, "OEEReportGodHours" + frda.ToString("yyyy-MM-dd") + ".xlsx"));
                    }
                    else if (TimeFactor == "NoBlue")
                    {
                        newFile = new FileInfo(System.IO.Path.Combine(FileDir, "OEEReportAdjusted" + frda.ToString("yyyy-MM-dd") + ".xlsx"));
                    }
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
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), Templatews);
                worksheetGraph = p.Workbook.Worksheets.Add("Graphs", TemplateGraph);
            }
            catch { }

            if (worksheet == null)
            {
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy") + "1", Templatews);
                worksheetGraph = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy") + "Graph", TemplateGraph);
            }
            int sheetcount = p.Workbook.Worksheets.Count;
            p.Workbook.Worksheets.MoveToStart(sheetcount);
            worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            #region MacCount & LowestLevel
            string lowestLevel = null;
            int MacCount = 0;
            int plantId = 0, shopId = 0, cellId = 0, wcId = 0;
            string Header = null;
            if (string.IsNullOrEmpty(WorkCenterID))
            {
                if (string.IsNullOrEmpty(CellID))
                {
                    if (string.IsNullOrEmpty(ShopID))
                    {
                        if (string.IsNullOrEmpty(PlantID))
                        {
                            //donothing
                        }
                        else
                        {
                            lowestLevel = "Plant";
                            plantId = Convert.ToInt32(PlantID);
                            MacCount = obj2.GetmachineListforoee(plantId).ToList().Count();
                            // MacCount = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == plantId && m.IsNormalWC == 0).ToList().Count();
                            var plantName = obj2.GettbplantDet(plantId);
                            //var plantName = (from plant in db.tblplants
                            //                 where plant.PlantID == plantId
                            //                 select new { plantname = plant.PlantName }).FirstOrDefault();

                            Header = plantName;
                        }
                    }
                    else
                    {
                        lowestLevel = "Shop";
                        shopId = Convert.ToInt32(ShopID);
                        MacCount = obj2.GetShopList(shopId).ToList().Count();
                        //MacCount = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == shopId && m.IsNormalWC == 0).ToList().Count();
                        var shopName = obj2.GettbShopIDDet(shopId);
                        // var shopName = (from shop in db.tblshops
                        //where shop.ShopID == shopId
                        //select new { shopname = shop.ShopName }).FirstOrDefault();

                        Header = shopName;
                    }
                }
                else
                {
                    lowestLevel = "Cell";
                    cellId = Convert.ToInt32(CellID);
                    MacCount = obj2.GetmachinecellList(cellId).ToList().Count();
                    // MacCount = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == cellId && m.IsNormalWC == 0).ToList().Count();
                    var cellName = obj2.GettbcellDet(cellId);
                    //var cellName = (from cell in db.tblcells
                    //                where cell.CellID == cellId
                    //                select new { wcname = cell.CellName }).FirstOrDefault();

                    Header = cellName;
                }
            }
            else
            {
                lowestLevel = "WorkCentre";
                wcId = Convert.ToInt32(WorkCenterID);
                MacCount = 1;
                var WCName = obj2.GettbMachineDe1t(wcId);
                //var WCName = (from wc in db.tblmachinedetails
                //              where wc.MachineID == wcId
                //              select new { wcname = wc.MachineDispName }).FirstOrDefault();
                Header = WCName;
            }

            #endregion

            #region Get Machines List
            DataTable machin = new DataTable();
            DateTime endDateTime = Convert.ToDateTime(toDate.AddDays(1).ToString("yyyy-MM-dd") + " " + new TimeSpan(6, 0, 0));
            string startDateTime = frmDate.ToString("yyyy-MM-dd");
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                String query1 = null;
                if (lowestLevel == "Plant")
                {
                    //query1 = " SELECT  distinct MachineID FROM "+ MsqlConnection.DbName + ".tblmachinedetails WHERE PlantID = " + PlantID + " and IsNormalWC = 0 and ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( case when (IsDeleted = 1) then ( InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + startDateTime + "' ) end) ) ;";
                    query1 = " SELECT  distinct MachineID FROM " + ConnectionFactory.DbName + ".tblmachinedetails WHERE PlantID = " + PlantID + " and IsNormalWC = 0 and ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( (IsDeleted = 1) and ( InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + startDateTime + "' )) ) ;";
                }
                else if (lowestLevel == "Shop")
                {
                    //query1 = " SELECT * FROM "+ MsqlConnection.DbName + ".tblmachinedetails WHERE ShopID = " + ShopID + "  and IsNormalWC = 0   and  ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( case when (IsDeleted = 1) then ( InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + startDateTime + "' ) end) ) ;";
                    query1 = " SELECT * FROM " + ConnectionFactory.DbName + ".tblmachinedetails WHERE ShopID = " + ShopID + "  and IsNormalWC = 0   and  ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( (IsDeleted = 1) and ( InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + startDateTime + "' ) ) ) ;";
                }
                else if (lowestLevel == "Cell")
                {
                    //query1 = " SELECT * FROM "+ MsqlConnection.DbName + ".tblmachinedetails WHERE CellID = " + CellID + "  and IsNormalWC = 0  and   ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( case when (IsDeleted = 1) then ( InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + startDateTime + "' ) end) ) ;";
                    query1 = " SELECT * FROM " + ConnectionFactory.DbName + ".tblmachinedetails WHERE CellID = " + CellID + "  and IsNormalWC = 0  and   ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( (IsDeleted = 1) and ( InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + startDateTime + "' ) ) ) ;";
                }
                else if (lowestLevel == "WorkCentre")
                {
                    //query1 = " SELECT * FROM "+ MsqlConnection.DbName + ".tblmachinedetails WHERE MachineID = " + WorkCenterID + "  and IsNormalWC = 0  and  ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( case when(IsDeleted = 1) then ( InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + startDateTime + "' )end)) ;";
                    query1 = " SELECT * FROM " + ConnectionFactory.DbName + ".tblmachinedetails WHERE MachineID = " + WorkCenterID + "  and IsNormalWC = 0  and  ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ((IsDeleted = 1) and ( InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + startDateTime + "' ))) ;";
                }
                SqlDataAdapter da1 = new SqlDataAdapter(query1, mc.msqlConnection);
                da1.Fill(machin);
                mc.close();
            }
            #endregion

            //DataTable for Consolidated Data 
            DataTable DTConsolidatedLosses = new DataTable();
            DTConsolidatedLosses.Columns.Add("Plant", typeof(string));
            DTConsolidatedLosses.Columns.Add("Shop", typeof(string));
            DTConsolidatedLosses.Columns.Add("Cell", typeof(string));
            DTConsolidatedLosses.Columns.Add("WCInvNo", typeof(string));
            DTConsolidatedLosses.Columns.Add("WCName", typeof(string));
            DTConsolidatedLosses.Columns.Add("CorrectedDate", typeof(string));

            //Add Other Cols of Excel into DataTable
            DTConsolidatedLosses.Columns.Add("OpTime", typeof(double));
            DTConsolidatedLosses.Columns["OpTime"].DefaultValue = 0.0;
            DTConsolidatedLosses.Columns.Add("AvailableTime", typeof(double));
            DTConsolidatedLosses.Columns["AvailableTime"].DefaultValue = 0.0;
            DTConsolidatedLosses.Columns.Add("SCTvsPP", typeof(double));
            DTConsolidatedLosses.Columns["SCTvsPP"].DefaultValue = 0.0;
            DTConsolidatedLosses.Columns.Add("ScrapQtyTime", typeof(double));
            DTConsolidatedLosses.Columns["ScrapQtyTime"].DefaultValue = 0.0;
            DTConsolidatedLosses.Columns.Add("ReworkTime", typeof(double));
            DTConsolidatedLosses.Columns["ReworkTime"].DefaultValue = 0.0;
            DTConsolidatedLosses.Columns.Add("CuttingTime", typeof(double));
            DTConsolidatedLosses.Columns["CuttingTime"].DefaultValue = 0.0;
            DTConsolidatedLosses.Columns.Add("DaysWorking", typeof(double));
            DTConsolidatedLosses.Columns["DaysWorking"].DefaultValue = 0.0;
            DTConsolidatedLosses.Columns.Add("GodHours", typeof(double));
            DTConsolidatedLosses.Columns["GodHours"].DefaultValue = 0.0;
            DTConsolidatedLosses.Columns.Add("TotalSTDHours", typeof(double));
            DTConsolidatedLosses.Columns["TotalSTDHours"].DefaultValue = 0.0;
            DTConsolidatedLosses.Columns.Add("RejectionHours", typeof(double));
            DTConsolidatedLosses.Columns["RejectionHours"].DefaultValue = 0.0;
            DTConsolidatedLosses.Columns.Add("MinorLoss", typeof(double));
            DTConsolidatedLosses.Columns["MinorLoss"].DefaultValue = 0.0;
            DTConsolidatedLosses.Columns.Add("Breakdown", typeof(double));
            DTConsolidatedLosses.Columns["Breakdown"].DefaultValue = 0.0;
            DTConsolidatedLosses.Columns.Add("PowerOff", typeof(double));
            DTConsolidatedLosses.Columns["PowerOff"].DefaultValue = 0.0;

            //Add Cols for A,P,Q and OEE for Individual Dates.
            DTConsolidatedLosses.Columns.Add("Avail", typeof(string));
            DTConsolidatedLosses.Columns.Add("Perf", typeof(string));
            DTConsolidatedLosses.Columns.Add("Qual", typeof(string));
            DTConsolidatedLosses.Columns.Add("OEE", typeof(string));

            //Get All Losses and Insert into DataTable

            DataTable LossCodesData = new DataTable();
            using (MsqlConnection mcLossCodes = new MsqlConnection())
            {
                mcLossCodes.open();
                startDateTime = frmDate.ToString("yyyy-MM-dd") + " " + new TimeSpan(6, 0, 0);
                //string query = @"select LossCodeID,LossCode from "+ MsqlConnection.DbName + ".tbllossescodes  where MessageType != 'BREAKDOWN' and ((CreatedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or "
                //            + "( case when (IsDeleted = 1) then ( CreatedOn <=  '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ModifiedOn >= '" + startDateTime + "') end) ) and LossCodeID NOT IN (  "
                //            + "SELECT DISTINCT LossCodeID FROM (  "
                //            + "SELECT DISTINCT LossCodesLevel1ID AS LossCodeID FROM "+ MsqlConnection.DbName + ".tbllossescodes where  MessageType != 'BREAKDOWN' and  LossCodesLevel1ID is not null  and ((CreatedOn <=  '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or  "
                //            + "( case when (IsDeleted = 1) then ( CreatedOn <=  '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ModifiedOn >= '" + startDateTime + "' ) end) ) "
                //            + "UNION  "
                //            + "SELECT DISTINCT LossCodesLevel2ID AS LossCodeID FROM "+ MsqlConnection.DbName + ".tbllossescodes where  MessageType != 'BREAKDOWN' and  LossCodesLevel2ID is not null  and ((CreatedOn <=  '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or  "
                //            + "( case when (IsDeleted = 1) then ( CreatedOn <=  '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ModifiedOn >= '" + startDateTime + "' ) end) )  "
                //            + ") AS derived ) order by LossCodesLevel1ID  ;";

                string query = @"select LossCodeID,LossCode from " + ConnectionFactory.DbName + ".tbllossescodes  where MessageType != 'BREAKDOWN' and ((CreatedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or "
                            + "( (IsDeleted = 1) and ( CreatedOn <=  '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ModifiedOn >= '" + startDateTime + "') ) ) and LossCodeID NOT IN (  "
                            + "SELECT DISTINCT LossCodeID FROM (  "
                            + "SELECT DISTINCT LossCodesLevel1ID AS LossCodeID FROM " + ConnectionFactory.DbName + ".tbllossescodes where  MessageType != 'BREAKDOWN' and  LossCodesLevel1ID is not null  and ((CreatedOn <=  '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or  "
                            + "( (IsDeleted = 1) and ( CreatedOn <=  '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ModifiedOn >= '" + startDateTime + "' ) ) ) "
                            + "UNION  "
                            + "SELECT DISTINCT LossCodesLevel2ID AS LossCodeID FROM " + ConnectionFactory.DbName + ".tbllossescodes where  MessageType != 'BREAKDOWN' and  LossCodesLevel2ID is not null  and ((CreatedOn <=  '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or  "
                            + "( (IsDeleted = 1) and ( CreatedOn <=  '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ModifiedOn >= '" + startDateTime + "' ) ) )  "
                            + ") AS derived ) order by LossCodesLevel1ID  ;";

                SqlDataAdapter daLossCodesData = new SqlDataAdapter(query, mcLossCodes.msqlConnection);
                daLossCodesData.Fill(LossCodesData);
                mcLossCodes.close();
            }

            //int LossesStartsATCol = 21;
            int LossesStartsATCol = 24;
            var LossesList = new List<KeyValuePair<int, string>>();

            #region LossCodes Into LossList
            for (int i = 0; i < LossCodesData.Rows.Count; i++)
            {
                int losscode = Convert.ToInt32(LossCodesData.Rows[i][0]);
                string losscodeName = Convert.ToString(LossCodesData.Rows[i][1]);
                var lossdata = obj1.GetLossDet2(losscode);
                //  var lossdata = obj1.GetLossDet2(losscode);
                int level = lossdata.LossCodesLevel;
                if (level == 3)
                {
                    int lossLevel1ID = Convert.ToInt32(lossdata.LossCodesLevel1ID);
                    int lossLevel2ID = Convert.ToInt32(lossdata.LossCodesLevel2ID);
                    var lossdata1 = obj1.GetLossDet2(lossLevel1ID);
                    var lossdata2 = obj1.GetLossDet2(lossLevel2ID);
                    //var lossdata1 = db.tbllossescodes.Where(m => m.LossCodeID == lossLevel1ID).FirstOrDefault();
                    // var lossdata2 = obj1.GetLossDet2(lossLevel2ID);
                    losscodeName = lossdata1.LossCode + " :: " + lossdata2.LossCode + " : " + lossdata.LossCode;
                }

                else if (level == 2)
                {
                    int lossLevel1ID = Convert.ToInt32(lossdata.LossCodesLevel1ID);
                    var lossdata1 = obj1.GetLossDet2(lossLevel1ID);
                    // var lossdata1 = obj1.GetLossDet2(lossLevel1ID);

                    losscodeName = lossdata1.LossCode + ":" + lossdata.LossCode;
                }
                else if (level == 1)
                {
                    if (losscode == 999)
                    {
                        losscodeName = "NoCode Entered";
                    }
                    else if (losscode == 9999)
                    {
                        losscodeName = "UnIdentified BreakDown";
                    }
                    else
                    {
                        losscodeName = lossdata.LossCode;
                    }
                }
                //losscodeName = LossHierarchy3rdLevel(losscode);
                DTConsolidatedLosses.Columns.Add(losscodeName, typeof(double));
                DTConsolidatedLosses.Columns[losscodeName].DefaultValue = "0";

                //Code to write LossesNames to Excel.
                string columnAlphabet = ExcelColumnFromNumber(LossesStartsATCol);

                worksheet.Cells[columnAlphabet + 3].Value = losscodeName;
                worksheet.Cells[columnAlphabet + 4].Value = "AF";

                LossesStartsATCol++;
                //Add the LossesToList
                LossesList.Add(new KeyValuePair<int, string>(losscode, losscodeName));
            }

            #endregion

            DateTime UsedDateForExcel = Convert.ToDateTime(frmDate);
            //For each Date ...... for all Machines.
            var Col = 'B';
            int Row = 5 + machin.Rows.Count + 3; // Gap to Insert OverAll data. DataStartRow + MachinesCount + 2(1 for HighestLevel & another for Gap).
            int Sno = 1;
            string finalLossCol = null;

            //for (int i = 0; i < TotalDay + 1; i++)
            int l = 0;
            do
            {
                DateTime begining = UsedDateForExcel, end = UsedDateForExcel;
                var testDate = UsedDateForExcel;

                double DaysInCurrentPeriod = 0;
                if (TabularType == "Day")
                {
                    DaysInCurrentPeriod = 1;
                    begining = end = UsedDateForExcel;
                }
                else if (TabularType == "Week")
                {
                    GetWeek(testDate, new CultureInfo("fr-FR"), out begining, out end); //en-US(Sunday - Monday) //fr-FR(Monday - Sunday)
                    if (end.Subtract(toDate).TotalSeconds > 0)
                    {
                        end = toDate;
                    }
                    if (begining.Subtract(UsedDateForExcel).TotalSeconds < 0)
                    {
                        begining = UsedDateForExcel;
                    }
                    DaysInCurrentPeriod = end.Subtract(UsedDateForExcel).TotalDays + 1;
                }
                else if (TabularType == "Month")
                {
                    DateTime itempDate = UsedDateForExcel.AddMonths(1);
                    DateTime Temp2 = new DateTime(itempDate.Year, itempDate.Month, 01, 00, 00, 01);
                    end = Temp2.AddDays(-1);
                    if (end.Subtract(toDate).TotalSeconds > 0)
                    {
                        end = toDate;
                    }
                    DaysInCurrentPeriod = end.Subtract(UsedDateForExcel).TotalDays + 1;
                }
                else if (TabularType == "Year")
                {
                    DateTime Temp2 = new DateTime(UsedDateForExcel.AddYears(1).Year, 01, 01, 00, 00, 01);
                    end = Temp2.AddDays(-1);
                    if (end.Subtract(toDate).TotalSeconds > 0)
                    {
                        end = toDate;
                    }
                    DaysInCurrentPeriod = end.Subtract(UsedDateForExcel).TotalDays + 1;
                }

                int StartingRowForToday = Row;
                double IndividualDateOpTime = 0, IndividualDateSCTvsPP = 0, IndividualDateScrapQtyTime = 0, IndividualDateReWorkTime = 0;
                double IndividualDateSetting = 0, IndividualDateIdle = 0, IndividualDateMinorLoss = 0, IndividualDateBreakdown = 0, IndividualDateNoPlan = 0;
                string dateforMachine = UsedDateForExcel.ToString("yyyy-MM-dd");

                double AvailableTimePerDay = 0;
                int NumMacsToExcel = 0;
                for (int n = 0; n < machin.Rows.Count; n++)
                {
                    NumMacsToExcel++;
                    double CummulativeOfAllLosses = 0;
                    if (n == 0 && l != 0)
                    {
                        Row++;
                        StartingRowForToday = Row;
                    }

                    int MachineID = Convert.ToInt32(machin.Rows[n][0]);
                    List<string> HierarchyData = GetHierarchyData(MachineID);

                    double AvaillabilityFactor = 0, EfficiencyFactor = 0, QualityFactor = 0;
                    double green, red, yellow, blue = 0, MinorLoss = 0, setup = 0, scrap = 0, NOP = 0, OperatingTime = 0, DownTimeBreakdown = 0, ROALossess = 0, AvailableTime = 0, SettingTime = 0, PlannedDownTime = 0, UnPlannedDownTime = 0;
                    double SummationOfSCTvsPP = 0, MinorLosses = 0, ROPLosses = 0;
                    double ScrapQtyTime = 0, ReWOTime = 0, ROQLosses = 0;
                    double selfInspection = 0, Idle = 0;

                    //New Logic . Take values from 
                    string correctedDateS = UsedDateForExcel.ToString("yyyy-MM-dd");
                    string correctedDateE = UsedDateForExcel.ToString("yyyy-MM-dd");
                    if (TabularType != "Day")
                    {
                        if (end.Subtract(toDate).TotalSeconds < 0)
                        {
                            correctedDateE = end.ToString("yyyy-MM-dd");
                        }
                        else
                        {
                            correctedDateE = toDate.ToString("yyyy-MM-dd");
                        }
                    }

                    //if (TabularType == "Day")
                    //{
                    //    //Default same as correctedDateE.
                    //}
                    //else if (TabularType == "Week")
                    //{
                    //    correctedDateE = end.ToString("yyyy-MM-dd");
                    //}
                    //else if (TabularType == "Month")
                    //{
                    //    correctedDateE = end.ToString("yyyy-MM-dd");
                    //}
                    //else if (TabularType == "Year")
                    //{
                    //    correctedDateE = end.ToString("yyyy-MM-dd");
                    //}

                    DateTime DateTimeValue = Convert.ToDateTime(UsedDateForExcel.ToString("yyyy-MM-dd") + " " + "00:00:00");
                    #region OLD 2017-03-30
                    //if (ProdFAI == "OverAll")
                    //{
                    //    var OEEData = dboee.tbloeedashboardvariables.Where(m => m.StartDate == DateTimeValue && m.WCID == MachineID).FirstOrDefault();
                    //    if (TabularType == "Day")
                    //    {
                    //        OEEData = dboee.tbloeedashboardvariables.Where(m => m.StartDate == DateTimeValue && m.WCID == MachineID).FirstOrDefault();
                    //    }
                    //    else if (TabularType == "Week")
                    //    {
                    //        OEEData = dboee.tbloeedashboardvariables.Where(m => m.StartDate == DateTimeValue && m.WCID == MachineID).FirstOrDefault();
                    //    }
                    //    else if (TabularType == "Month")
                    //    {
                    //        OEEData = dboee.tbloeedashboardvariables.Where(m => m.StartDate == DateTimeValue && m.WCID == MachineID).FirstOrDefault();
                    //    }
                    //    else if (TabularType == "Year")
                    //    {
                    //        OEEData = dboee.tbloeedashboardvariables.Where(m => m.StartDate == DateTimeValue && m.WCID == MachineID).FirstOrDefault();
                    //    }
                    //    if (OEEData != null)
                    //    {
                    //        MinorLosses = Convert.ToDouble(OEEData.MinorLosses);
                    //        blue = Convert.ToDouble(OEEData.Blue);
                    //        SettingTime = Convert.ToDouble(OEEData.SettingTime);
                    //        ROALossess = Convert.ToDouble(OEEData.ROALossess);
                    //        DownTimeBreakdown = Convert.ToDouble(OEEData.DownTimeBreakdown);
                    //        SummationOfSCTvsPP = Convert.ToDouble(OEEData.SummationOfSCTvsPP);
                    //        green = Convert.ToDouble(OEEData.Green);
                    //        OperatingTime = green;
                    //        ScrapQtyTime = Convert.ToDouble(OEEData.ScrapQtyTime);
                    //        ReWOTime = Convert.ToDouble(OEEData.ReWOTime);
                    //    }
                    //    else
                    //    {
                    //        #region Trying to Run .exe file with Params
                    //        try
                    //        {
                    //            String cPath = dboee.tblapp_paths.Where(m => m.IsDeleted == 0 && m.AppName == "CalOEEDaily").Select(m => m.AppPath).FirstOrDefault();
                    //            string filename = Path.Combine(cPath, "CalOEEDaily.exe");
                    //            var proc = System.Diagnostics.Process.Start(Server.MapPath(@filename), DateTimeValue.ToString());
                    //            proc.WaitForExit();
                    //            //proc.Kill();
                    //        }
                    //        catch (Exception e)
                    //        {
                    //        }
                    //        var OEEDataInner = dboee.tbloeedashboardvariables.Where(m => m.StartDate == DateTimeValue && m.WCID == MachineID).FirstOrDefault();
                    //        if (OEEDataInner != null)
                    //        {
                    //            MinorLosses = Convert.ToDouble(OEEDataInner.MinorLosses);
                    //            blue = Convert.ToDouble(OEEDataInner.Blue);
                    //            SettingTime = Convert.ToDouble(OEEDataInner.SettingTime);
                    //            ROALossess = Convert.ToDouble(OEEDataInner.ROALossess);
                    //            DownTimeBreakdown = Convert.ToDouble(OEEDataInner.DownTimeBreakdown);
                    //            SummationOfSCTvsPP = Convert.ToDouble(OEEData.SummationOfSCTvsPP);
                    //            green = Convert.ToDouble(OEEDataInner.Green);
                    //            OperatingTime = green;
                    //            ScrapQtyTime = Convert.ToDouble(OEEDataInner.ScrapQtyTime);
                    //            ReWOTime = Convert.ToDouble(OEEDataInner.ReWOTime);
                    //        }
                    //        else
                    //        {
                    //            continue;
                    //        }
                    //        #endregion
                    //    }
                    //}
                    #endregion

                    if (ProdFAI == "OverAll")
                    {
                        DataTable OEEData = new DataTable();
                        using (MsqlConnection mcOEE = new MsqlConnection())
                        {
                            mcOEE.open();
                            string query = null;
                            query = @"select WCID,sum(Green), sum(SummationOfSCTvsPP),sum(SettingTime),sum(ROALossess),SUM(MinorLosses), sum(Blue), sum(DownTimeBreakdown), "
                                    + " sum(ScrapQtyTime),sum(ReWOTime) from " + ConnectionFactory.DbName + ".tbloeedashboardvariables where StartDate >= '" + correctedDateS + "' and StartDate <= '" + correctedDateE + "' and WCID = '" + MachineID + "' Group by WCID ;";
                            SqlDataAdapter daLossCodesData = new SqlDataAdapter(query, mcOEE.msqlConnection);
                            daLossCodesData.Fill(OEEData);
                            mcOEE.close();
                        }
                        if (OEEData.Rows.Count > 0)
                        {
                            int wcidInt = 0;
                            int.TryParse(Convert.ToString(OEEData.Rows[0][0]), out wcidInt);
                            if (wcidInt != 0)
                            {
                                green = Convert.ToDouble(OEEData.Rows[0][1]);
                                SummationOfSCTvsPP = Convert.ToDouble(OEEData.Rows[0][2]);
                                SettingTime = Convert.ToDouble(OEEData.Rows[0][3]);
                                //selfInspection = Convert.ToDouble(OEEData.Rows[0][4]); ;
                                Idle = Convert.ToDouble(OEEData.Rows[0][4]); ;
                                MinorLosses = Convert.ToDouble(OEEData.Rows[0][5]);
                                ROALossess = selfInspection + Idle;
                                blue = Convert.ToDouble(OEEData.Rows[0][6]);
                                DownTimeBreakdown = Convert.ToDouble(OEEData.Rows[0][7]);
                                ScrapQtyTime = Convert.ToDouble(OEEData.Rows[0][8]);
                                ReWOTime = Convert.ToDouble(OEEData.Rows[0][9]);
                                OperatingTime = green;
                            }
                        }
                    }
                    else
                    {
                        //var OEEData = dboee.tblworeports.Where(m => m.CorrectedDate == correctedDate && m.MachineID == MachineID && m.Type == ProdFAI).GroupBy(m=>m.MachineID).Select new { MachineID = };
                        DataTable OEEData = new DataTable();
                        using (MsqlConnection mcOEE = new MsqlConnection())
                        {
                            mcOEE.open();
                            string query = null;
                            if (TabularType == "Day")
                            {
                                if (ProdFAI != "Others")
                                {
                                    query = @"select MachineID,sum(CuttingTime), sum(SummationOfSCTvsPP),sum(SettingTime),sum(SelfInspection),sum(Idle),SUM(MinorLoss), sum(Blue), sum(Breakdown),sum(ScrapQtyTime),sum(ReWorkTime) "
                                                 + " from " + ConnectionFactory.DbName + ".tblworeport where CorrectedDate >= '" + correctedDateS + "' and CorrectedDate <= '" + correctedDateE + "' and MachineID = '" + MachineID + "' and Type = '" + ProdFAI + "' group by MachineID;";
                                }
                                else
                                {
                                    query = @"select MachineID,sum(CuttingTime), sum(SummationOfSCTvsPP),sum(SettingTime),sum(SelfInspection),sum(Idle),SUM(MinorLoss), sum(Blue), sum(Breakdown),sum(ScrapQtyTime),sum(ReWorkTime) "
                                                 + " from " + ConnectionFactory.DbName + ".tblworeport where CorrectedDate >= '" + correctedDateS + "' and CorrectedDate <= '" + correctedDateE + "' and MachineID = '" + MachineID + "'  and  Type != 'FAI' and Type != 'Production' group by MachineID;";
                                }
                            }
                            else if (TabularType == "Week")
                            {
                                if (ProdFAI != "Others")
                                {
                                    query = @"select MachineID,sum(CuttingTime), sum(SummationOfSCTvsPP),sum(SettingTime),sum(SelfInspection),sum(Idle),SUM(MinorLoss), sum(Blue), sum(Breakdown),sum(ScrapQtyTime),sum(ReWorkTime) "
                                                 + " from " + ConnectionFactory.DbName + ".tblworeport where CorrectedDate >= '" + correctedDateS + "' and CorrectedDate <= '" + correctedDateE + "' and MachineID = '" + MachineID + "' and Type = '" + ProdFAI + "' group by MachineID;";
                                }
                                else
                                {
                                    query = @"select MachineID,sum(CuttingTime), sum(SummationOfSCTvsPP),sum(SettingTime),sum(SelfInspection),sum(Idle),SUM(MinorLoss), sum(Blue), sum(Breakdown),sum(ScrapQtyTime),sum(ReWorkTime) "
                                                 + " from " + ConnectionFactory.DbName + ".tblworeport where CorrectedDate >= '" + correctedDateS + "' and CorrectedDate <= '" + correctedDateE + "' and MachineID = '" + MachineID + "'  and  Type != 'FAI' and Type != 'Production' group by MachineID;";
                                }
                            }
                            else if (TabularType == "Month")
                            {
                                if (ProdFAI != "Others")
                                {
                                    query = @"select MachineID,sum(CuttingTime), sum(SummationOfSCTvsPP),sum(SettingTime),sum(SelfInspection),sum(Idle),SUM(MinorLoss), sum(Blue), sum(Breakdown),sum(ScrapQtyTime),sum(ReWorkTime) "
                                                 + " from " + ConnectionFactory.DbName + ".tblworeport where CorrectedDate >= '" + correctedDateS + "' and CorrectedDate <= '" + correctedDateE + "' and MachineID = '" + MachineID + "' and Type = '" + ProdFAI + "'  group by MachineID;";
                                }
                                else
                                {
                                    query = @"select MachineID,sum(CuttingTime), sum(SummationOfSCTvsPP),sum(SettingTime),sum(SelfInspection),sum(Idle),SUM(MinorLoss), sum(Blue), sum(Breakdown),sum(ScrapQtyTime),sum(ReWorkTime) "
                                                 + " from " + ConnectionFactory.DbName + ".tblworeport where CorrectedDate >= '" + correctedDateS + "' and CorrectedDate <= '" + correctedDateE + "' and MachineID = '" + MachineID + "'  and  Type != 'FAI' and Type != 'Production'   group by MachineID;";
                                }
                            }
                            else if (TabularType == "Year")
                            {
                                if (ProdFAI != "Others")
                                {
                                    query = @"select MachineID,sum(CuttingTime), sum(SummationOfSCTvsPP),sum(SettingTime),sum(SelfInspection),sum(Idle),SUM(MinorLoss), sum(Blue), sum(Breakdown),sum(ScrapQtyTime),sum(ReWorkTime) "
                                                 + " from " + ConnectionFactory.DbName + ".tblworeport where CorrectedDate >= '" + correctedDateS + "' and CorrectedDate <= '" + correctedDateE + "' and MachineID = '" + MachineID + "' and Type = '" + ProdFAI + "'  group by MachineID;";
                                }
                                else
                                {
                                    query = @"select MachineID,sum(CuttingTime), sum(SummationOfSCTvsPP),sum(SettingTime),sum(SelfInspection),sum(Idle),SUM(MinorLoss), sum(Blue), sum(Breakdown),sum(ScrapQtyTime),sum(ReWorkTime) "
                                                 + " from " + ConnectionFactory.DbName + ".tblworeport where CorrectedDate >= '" + correctedDateS + "' and CorrectedDate <= '" + correctedDateE + "' and MachineID = '" + MachineID + "'  and  Type != 'FAI' and Type != 'Production'   group by MachineID;";
                                }
                            }

                            SqlDataAdapter daLossCodesData = new SqlDataAdapter(query, mcOEE.msqlConnection);
                            daLossCodesData.Fill(OEEData);
                            mcOEE.close();
                        }
                        if (OEEData.Rows.Count > 0)
                        {
                            int wcidInt = 0;
                            int.TryParse(Convert.ToString(OEEData.Rows[0][0]), out wcidInt);
                            if (wcidInt != 0)
                            {
                                green = Convert.ToDouble(OEEData.Rows[0][1]);
                                SummationOfSCTvsPP = Convert.ToDouble(OEEData.Rows[0][2]);
                                SettingTime = Convert.ToDouble(OEEData.Rows[0][3]);
                                selfInspection = Convert.ToDouble(OEEData.Rows[0][4]); ;
                                Idle = Convert.ToDouble(OEEData.Rows[0][5]); ;
                                MinorLosses = Convert.ToDouble(OEEData.Rows[0][6]);
                                ROALossess = selfInspection + Idle;
                                blue = Convert.ToDouble(OEEData.Rows[0][7]);
                                DownTimeBreakdown = Convert.ToDouble(OEEData.Rows[0][8]);
                                ScrapQtyTime = Convert.ToDouble(OEEData.Rows[0][9]);
                                ReWOTime = Convert.ToDouble(OEEData.Rows[0][10]);
                                OperatingTime = green;
                            }
                        }
                    }

                    // OperatingTime = AvailableTime - (ROALossess + DownTimeBreakdown + blue + MinorLosses + ROPLosses + ROQLosses);
                    // OperatingTime = AvailableTime - (ROALossess + DownTimeBreakdown + blue + MinorLosses);

                    setup = SettingTime;
                    //double TotalMinutes = green + setup + (yellow - setup) + red + blue;
                    double TotalMinutes = OperatingTime + SettingTime + (ROALossess - SettingTime) + MinorLosses + DownTimeBreakdown + blue;
                    //double Diff = 1440 - TotalMinutes;

                    //for 3 shifts so 
                    //double Diff = (8 * 3 * 60) - TotalMinutes;
                    Double ActualTotalMinutes = (8 * 3 * 60) * DaysInCurrentPeriod;
                    double Diff = ActualTotalMinutes - TotalMinutes;
                    if (Diff > 0)
                    {
                        blue += Diff;
                    }
                    //else
                    //{
                    //    ROALossess += Diff;
                    //}

                    if (TimeFactor == "GH")
                    {
                        AvailableTime = ActualTotalMinutes; //24Hours to Minutes
                    }
                    else if (TimeFactor == "NoBlue")
                    {
                        AvailableTime = ActualTotalMinutes - blue;
                        double planned_Duration = get_PlanBreakdownduration();
                        if (AvailableTime > planned_Duration)
                        {
                            AvailableTime -= planned_Duration;
                        }
                    }
                    worksheet.Cells["Q" + Row].Value = Math.Round(AvailableTime / 60, 2);

                    int IsWorking = 1;
                    AvailableTimePerDay += AvailableTime;

                    worksheet.Cells["B" + Row].Value = Sno++;
                    //worksheet.Cells["C" + Row].Value = UsedDateForExcel.ToString("yyyy-MM-dd");
                    worksheet.Cells["C" + Row].Value = HierarchyData[0];
                    worksheet.Cells["D" + Row].Value = HierarchyData[1];
                    worksheet.Cells["E" + Row].Value = HierarchyData[2];
                    worksheet.Cells["F" + Row].Value = HierarchyData[3];
                    worksheet.Cells["G" + Row].Value = HierarchyData[3];

                    worksheet.Cells["H" + Row].Value = UsedDateForExcel.ToString("yyyy-MM-dd");
                    worksheet.Cells["I" + Row].Value = end.ToString("yyyy-MM-dd");

                    #region Calculate and push A,P,Q and OEE

                    //Utilisation // Whole day duration is 24*60 = 1440 minutes
                    double valA = 0;
                    valA = OperatingTime / AvailableTime;

                    //Availablity
                    if (OperatingTime != 0)
                    {
                        if (valA > 0 && valA < 100)
                        {
                            worksheet.Cells["J" + Row].Value = Math.Round(valA * 100, 0);
                            AvaillabilityFactor = Math.Round(valA * 100, 0);
                        }

                        else if (AvaillabilityFactor > 100)
                        {
                            AvaillabilityFactor = 100;
                        }
                        else
                        {
                            worksheet.Cells["J" + Row].Value = 0;
                            AvaillabilityFactor = 0;
                        }

                    }
                    if (AvaillabilityFactor != 0)
                    {
                        //Performance
                        if (SummationOfSCTvsPP == -1 || SummationOfSCTvsPP == 0)
                        {
                            EfficiencyFactor = 100;
                            worksheet.Cells["K" + Row].Value = Math.Round(EfficiencyFactor, 0);
                        }
                        else
                        {
                            EfficiencyFactor = Math.Round((SummationOfSCTvsPP / (OperatingTime)) * 100, 0);
                            if (EfficiencyFactor >= 0 && EfficiencyFactor <= 100)
                                worksheet.Cells["K" + Row].Value = EfficiencyFactor;
                            else if (EfficiencyFactor > 100)
                            {
                                EfficiencyFactor = 100;
                                worksheet.Cells["K" + Row].Value = 100;
                            }
                            else if (EfficiencyFactor < 0)
                            {
                                EfficiencyFactor = 0;
                                worksheet.Cells["K" + Row].Value = 0;
                            }
                        }
                        //Quality
                        if (OperatingTime != 0)
                        {
                            QualityFactor = ((OperatingTime - ScrapQtyTime - ReWOTime) / OperatingTime) * 100;
                            if (QualityFactor >= 0 && QualityFactor <= 100)
                            {
                                worksheet.Cells["L" + Row].Value = Math.Round(QualityFactor, 0);
                            }
                            else if (QualityFactor > 100)
                            {
                                QualityFactor = 100;
                                worksheet.Cells["L" + Row].Value = 100;
                            }
                            else if (QualityFactor < 0)
                            {
                                QualityFactor = 0;
                                worksheet.Cells["L" + Row].Value = 0;
                            }
                        }
                        else
                        {
                            QualityFactor = 0;
                            worksheet.Cells["L" + Row].Value = 0;
                        }
                    }
                    else
                    {
                        worksheet.Cells["K" + Row].Value = 0;//Performance
                        worksheet.Cells["L" + Row].Value = 0;//Quality
                    }

                    //OEE
                    if (AvaillabilityFactor <= 0 || EfficiencyFactor <= 0 || QualityFactor <= 0)
                    {
                        worksheet.Cells["M" + Row].Value = 0;
                    }
                    else
                    {
                        valA = Math.Round((AvaillabilityFactor / 100) * (EfficiencyFactor / 100) * (QualityFactor / 100) * 100, 0);
                        if (valA >= 0 && valA <= 100)
                        {
                            worksheet.Cells["M" + Row].Value = Math.Round(valA, 0);
                        }
                        else if (valA > 100)
                        {
                            worksheet.Cells["M" + Row].Value = 100;
                        }
                        else if (valA < 0)
                        {
                            worksheet.Cells["M" + Row].Value = 0;
                        }
                    }

                    #endregion

                    //if (TimeFactor == "GH")
                    //{
                    //    worksheet.Cells["N" + Row].Value = 24;
                    //}

                    worksheet.Cells["N" + Row].Value = Math.Round((SummationOfSCTvsPP / 60), 2);

                    //worksheet.Cells["R" + Row].Value = 24 * 60;
                    //worksheet.Cells["S" + Row].Formula = "=SUM(R" + Row + ",T" + Row + ",U" + Row + ",V" + Row + ")";
                    worksheet.Cells["S" + Row].Formula = "=SUM(R" + Row + ",T" + Row + ")";
                    worksheet.Cells["R" + Row].Value = Math.Round(OperatingTime / 60, 2);

                    worksheet.Cells["P" + Row].Value = 1;

                    //To push Formula for Total,a column @ index(20) in Template
                    //string LossesEndsAtCol = ExcelColumnFromNumber(20 + LossesList.Count);
                    //worksheet.Cells["T" + Row].Formula = "=SUM(U" + Row + ":" + LossesEndsAtCol + "" + Row + ")";

                    double ValueAddingTime = Math.Round(OperatingTime, 2);
                    double setTime = Math.Round(SettingTime, 2);
                    double idleTime = Math.Round((ROALossess - SettingTime), 2);
                    double minorLossTime = Math.Round(MinorLosses, 2);
                    double BreakdownTime = Math.Round(DownTimeBreakdown, 2);
                    double blueTime = Math.Round(blue, 2);

                    //For Individual Date Cummulative
                    IndividualDateOpTime += ValueAddingTime;
                    IndividualDateSCTvsPP += SummationOfSCTvsPP;
                    IndividualDateScrapQtyTime += ScrapQtyTime;
                    IndividualDateReWorkTime += ReWOTime;

                    IndividualDateSetting += setTime;
                    IndividualDateIdle += idleTime;
                    IndividualDateMinorLoss += minorLossTime;
                    IndividualDateBreakdown += BreakdownTime;
                    IndividualDateNoPlan += blueTime;

                    //Added this machineDetails into Datatable
                    string WCInvNoString = HierarchyData[3];
                    //DataRow dr = DTLosses.Select("LossName= " + lossname).FirstOrDefault(); 
                    //DataRow dr = DTConsolidatedLosses.Select("WCInvNo = '" + @WCInvNoString + "'", " CorrectedDate= '" + @dateforMachine + "'").FirstOrDefault(); // finds all rows with id==2 and selects first or null if haven't found any
                    DataRow dr = DTConsolidatedLosses.AsEnumerable().FirstOrDefault(r => r.Field<string>("WCInvNo") == @WCInvNoString && r.Field<string>("CorrectedDate") == dateforMachine);
                    if (dr != null)
                    {
                        //do nothing
                    }
                    else
                    {
                        //plant, shop, cell, macINV, WcName, CorrectedDate, ValueAdding(Green/Operating), AvailableTime, SummationofSCTvsPP, Scrap,Rework,CuttingTime,DaysWorking, GodHours, TotalSTDHours, RejectionHours.
                        DTConsolidatedLosses.Rows.Add(HierarchyData[0], HierarchyData[1], HierarchyData[2], HierarchyData[3], HierarchyData[3], dateforMachine, ValueAddingTime, AvailableTime, SummationOfSCTvsPP, ScrapQtyTime, ReWOTime, ValueAddingTime, IsWorking, AvailableTime, 24, 0, minorLossTime, BreakdownTime, blueTime);
                    }
                    //Now get & put Losses
                    // Push Loss Value into  DataTable & Excel

                    //1st push 0 for every loss into excel
                    int column = 23 + LossCodesData.Rows.Count; // StartCol in Excel + TotalLosses
                    finalLossCol = ExcelColumnFromNumber(column);
                    worksheet.Cells["X" + Row + ":" + finalLossCol + "" + Row].Value = 0;

                    #region
                    if (ProdFAI == "OverAll")
                    {
                        //to Capture and Push , Losses that occured.
                        List<KeyValuePair<int, double>> LossesdurationList = GetAllLossesDurationSecondsForDateRange(MachineID, correctedDateS, correctedDateE);
                        DataRow dr1 = DTConsolidatedLosses.AsEnumerable().FirstOrDefault(r => r.Field<string>("WCInvNo") == WCInvNoString && r.Field<string>("CorrectedDate") == @dateforMachine);
                        if (dr1 != null)
                        {
                            foreach (var loss in LossesdurationList)
                            {
                                int LossID = loss.Key;
                                double Duration = loss.Value;
                                var lossdata = obj1.GetLossDet2(LossID);
                                //var lossdata = db.tbllossescodes.Where(m => m.LossCodeID == LossID).FirstOrDefault();
                                int level = lossdata.LossCodesLevel;
                                string losscodeName = null;

                                #region To Get LossCode Hierarchy
                                if (level == 3)
                                {
                                    int lossLevel1ID = Convert.ToInt32(lossdata.LossCodesLevel1ID);
                                    int lossLevel2ID = Convert.ToInt32(lossdata.LossCodesLevel2ID);
                                    var lossdata1 = obj1.GetLossDet2(lossLevel1ID);
                                    var lossdata2 = obj1.GetLossDet2(lossLevel2ID);
                                    //var lossdata1 = db.tbllossescodes.Where(m => m.LossCodeID == lossLevel1ID).FirstOrDefault();
                                    // var lossdata2 = obj1.GetLossDet2(lossLevel2ID);
                                    losscodeName = lossdata1.LossCode + " :: " + lossdata2.LossCode + " : " + lossdata.LossCode;
                                }
                                else if (level == 2)
                                {
                                    int lossLevel1ID = Convert.ToInt32(lossdata.LossCodesLevel1ID);
                                    var lossdata1 = obj1.GetLossDet2(lossLevel1ID);
                                    // var lossdata1 = obj1.GetLossDet2(lossLevel1ID);

                                    losscodeName = lossdata1.LossCode + ":" + lossdata.LossCode;
                                }
                                else if (level == 1)
                                {
                                    if (LossID == 999)
                                    {
                                        losscodeName = "NoCode Entered";
                                    }
                                    else
                                    {
                                        losscodeName = lossdata.LossCode;
                                    }
                                }
                                #endregion

                                int ColumnIndex = DTConsolidatedLosses.Columns[losscodeName].Ordinal;
                                string ColumnForThisLoss = ExcelColumnFromNumber(ColumnIndex + 1);
                                double DurInHours = Convert.ToDouble(Math.Round((Duration / (60 * 60)), 2)); //To Hours:: 1 Decimal Place
                                worksheet.Cells[ColumnForThisLoss + "" + Row].Value = DurInHours;
                                dr1[losscodeName] = DurInHours;
                                CummulativeOfAllLosses += DurInHours;
                            }
                        }
                    }
                    else
                    {
                        DataRow dr1 = DTConsolidatedLosses.AsEnumerable().FirstOrDefault(r => r.Field<string>("WCInvNo") == WCInvNoString && r.Field<string>("CorrectedDate") == @dateforMachine);
                        if (dr1 != null)
                        {
                            DataTable LossesDurationData = new DataTable();
                            using (MsqlConnection mcLossCodes = new MsqlConnection())
                            {
                                mcLossCodes.open();
                                string query = null;
                                if (ProdFAI != "Others")
                                {
                                    query = @"select sum(LossDuration),Level,LossName,LossCodeLevel1Name,LossCodeLevel2Name from " + ConnectionFactory.DbName + ".tblwolossess Where HMIID in ( SELECT HMIID FROM " + ConnectionFactory.DbName + ".tblworeport where CorrectedDate >= '" + correctedDateS + "' and CorrectedDate <= '" + correctedDateE + "' and MachineID = '" + MachineID + "' and Type = '" + ProdFAI + "' ) group by LossName,Level,LossCodeLevel1Name,LossCodeLevel2Name;";
                                }
                                else
                                {
                                    query = @"select sum(LossDuration),Level,LossName,LossCodeLevel1Name,LossCodeLevel2Name from " + ConnectionFactory.DbName + ".tblwolossess Where HMIID in ( SELECT HMIID FROM " + ConnectionFactory.DbName + ".tblworeport where CorrectedDate >= '" + correctedDateS + "' and CorrectedDate <= '" + correctedDateE + "' and MachineID = '" + MachineID + "' and  Type != 'FAI' and Type != 'Production'  ) group by LossName,Level,LossCodeLevel1Name,LossCodeLevel2Name;";

                                }
                                SqlDataAdapter daLossCodesData = new SqlDataAdapter(query, mcLossCodes.msqlConnection);
                                daLossCodesData.Fill(LossesDurationData);
                                mcLossCodes.close();
                            }

                            for (int Lossloop = 0; Lossloop < LossesDurationData.Rows.Count; Lossloop++)
                            {
                                double Duration = Convert.ToDouble(LossesDurationData.Rows[Lossloop][0]);
                                int level = Convert.ToInt32(LossesDurationData.Rows[Lossloop][1]);
                                string losscodeName = null;

                                #region To Get LossCode Hierarchy
                                if (level == 3)
                                {
                                    string Level1Name = Convert.ToString(LossesDurationData.Rows[Lossloop][3]);
                                    string Level2Name = Convert.ToString(LossesDurationData.Rows[Lossloop][4]);
                                    string Level3Name = Convert.ToString(LossesDurationData.Rows[Lossloop][2]);
                                    losscodeName = Level1Name + " :: " + Level2Name + " : " + Level3Name;
                                }
                                else if (level == 2)
                                {
                                    string Level1Name = Convert.ToString(LossesDurationData.Rows[Lossloop][3]);
                                    string Level2Name = Convert.ToString(LossesDurationData.Rows[Lossloop][2]);
                                    losscodeName = Level1Name + ":" + Level2Name;
                                }
                                else if (level == 1)
                                {
                                    string Level1Name = Convert.ToString(LossesDurationData.Rows[Lossloop][2]);
                                    if (Level1Name == "999")
                                    {
                                        losscodeName = "NoCode Entered";
                                    }
                                    else
                                    {
                                        losscodeName = Level1Name;
                                    }
                                }
                                #endregion

                                int ColumnIndex = DTConsolidatedLosses.Columns[losscodeName].Ordinal;
                                string ColumnForThisLoss = ExcelColumnFromNumber(ColumnIndex + 1);
                                double DurInHours = Convert.ToDouble(Math.Round((Duration / (60 * 60)), 2)); //To Hours:: 1 Decimal Place
                                worksheet.Cells[ColumnForThisLoss + "" + Row].Value = DurInHours;
                                dr1[losscodeName] = DurInHours;
                                CummulativeOfAllLosses += DurInHours;
                            }
                        }
                    }
                    #endregion
                    //worksheet.Cells["T" + Row].Value = Convert.ToDouble(Math.Round((ROALossess / 60), 2));

                    worksheet.Cells["T" + Row].Formula = "=SUM(U" + Row + ",V" + Row + ",(X" + Row + ":" + finalLossCol + "" + Row + "))";
                    //worksheet.Cells["T" + Row].Value = Convert.ToDouble(Math.Round((ROALossess / 60), 2)) + Math.Round(minorLossTime / 60, 2) + Math.Round(BreakdownTime / 60, 2);  // Added by Ashok
                    worksheet.Cells["U" + Row].Value = Math.Round(minorLossTime / 60, 2);
                    worksheet.Cells["V" + Row].Value = Math.Round(BreakdownTime / 60, 2);
                    //20180924:Pavan-- Adding the Power Off value into the report.
                    worksheet.Cells["W" + Row].Value = Math.Round(blueTime / 60, 2);

                    Row++;
                }

                #region //Daywise OEE and Stuff
                //1) Availability
                double AFactor = 0;
                AFactor = IndividualDateOpTime / AvailableTimePerDay;
                if (AFactor > 0)
                {
                    worksheet.Cells["J" + Row].Value = Math.Round(AFactor * 100, 0);
                    AFactor = Math.Round(AFactor * 100, 0);
                }
                else
                {
                    worksheet.Cells["J" + Row].Value = 0;
                    AFactor = 0;
                }
                double QFactor = 0; double EFactor = 0;
                if (AFactor != 0)
                {

                    //2)Performance
                    if (IndividualDateSCTvsPP <= 0 || IndividualDateOpTime <= 0)
                    {
                        EFactor = 100;
                        worksheet.Cells["K" + Row].Value = Math.Round(EFactor, 0);
                    }
                    else
                    {
                        EFactor = Math.Round((IndividualDateSCTvsPP / (IndividualDateOpTime)) * 100, 0);
                        if (EFactor > 0 && EFactor <= 100)
                        {
                            worksheet.Cells["K" + Row].Value = EFactor;
                        }
                        else if (EFactor > 100)
                        {
                            EFactor = 100;
                            worksheet.Cells["K" + Row].Value = 100;
                        }
                        else if (EFactor <= 0)
                        {
                            EFactor = 0;
                            worksheet.Cells["K" + Row].Value = 100;
                        }
                    }
                    //3) Quality
                    if (IndividualDateOpTime != 0)
                    {
                        QFactor = ((IndividualDateOpTime - IndividualDateScrapQtyTime - IndividualDateReWorkTime) / IndividualDateOpTime) * 100;
                        if (QFactor > 0 && QFactor <= 100)
                        {
                            worksheet.Cells["L" + Row].Value = Math.Round(QFactor, 0);
                            QFactor = Math.Round(QFactor, 2);
                        }
                        else if (QFactor > 100)
                        {
                            QFactor = 100;
                            worksheet.Cells["L" + Row].Value = 100;
                        }
                        else if (QFactor <= 0)
                        {
                            QFactor = 0;
                            worksheet.Cells["L" + Row].Value = 100;
                        }
                    }
                    else
                    {
                        QFactor = 100;
                        worksheet.Cells["L" + Row].Value = 100;
                    }

                }
                else
                {
                    worksheet.Cells["K" + Row].Value = 0;//Performance
                    worksheet.Cells["L" + Row].Value = 0;//Quality
                }


                //4) OEE
                double ValOEE = 0;
                double OEEFactor = 0;
                if (AFactor <= 0 || EFactor <= 0 || QFactor <= 0)
                {
                    worksheet.Cells["M" + Row].Value = 0;
                }
                else
                {
                    ValOEE = Math.Round(((AFactor / 100) * (EFactor / 100) * (QFactor / 100)) * 100, 0);
                    if (ValOEE >= 0 && ValOEE <= 100)
                    {
                        OEEFactor = Math.Round(ValOEE, 2);
                        worksheet.Cells["M" + Row].Value = Math.Round(ValOEE, 0);
                    }
                    else if (ValOEE > 100)
                    {
                        OEEFactor = 100;
                        worksheet.Cells["M" + Row].Value = 100;
                    }
                    else if (ValOEE < 0)
                    {
                        worksheet.Cells["M" + Row].Value = 0;
                    }
                }

                //OEE and  Stuff for entire day (of all WC's) Into DT
                DTConsolidatedLosses.Rows.Add("Summarized", "Summarized", "Summarized", "Summarized", "Summarized", dateforMachine, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, "J" + Row, "K" + Row, "L" + Row, "M" + Row);

                //Cellwise Border for Today
                worksheet.Cells["B" + StartingRowForToday + ":" + finalLossCol + "" + Row].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells["B" + StartingRowForToday + ":" + finalLossCol + "" + Row].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells["B" + StartingRowForToday + ":" + finalLossCol + "" + Row].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Cells["B" + StartingRowForToday + ":" + finalLossCol + "" + Row].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                //Insert Cummulative for today
                worksheet.Cells["C" + Row + ":G" + Row].Merge = true;
                worksheet.Cells["C" + Row].Value = "Summarized For";
                worksheet.Cells["H" + Row].Value = worksheet.Cells["H" + (Row - 1)].Value;
                worksheet.Cells["B" + Row + ":" + finalLossCol + Row].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
                worksheet.Cells["B" + Row + ":" + finalLossCol + Row].Style.Font.Bold = true;
                //worksheet.Cells["I" + Row].Value = worksheet.Cells["I" + (Row - 1)].Value;

                // 1) Total of AllLosses( Col : T )
                worksheet.Cells["T" + Row].Formula = "=SUM(T" + StartingRowForToday + ":T" + (Row - 1) + ")";
                // 2) Total of OperatingTime( Col : S )
                worksheet.Cells["S" + Row].Formula = "=SUM(S" + StartingRowForToday + ":S" + (Row - 1) + ")";
                // 2) Total of CuttingTime( Col : R )
                worksheet.Cells["R" + Row].Formula = "=SUM(R" + StartingRowForToday + ":R" + (Row - 1) + ")";
                // 3) God Hours( Col : Q )
                worksheet.Cells["Q" + Row].Formula = "=SUM(Q" + StartingRowForToday + ":Q" + (Row - 1) + ")";
                // 4) Days Working( Col : P )
                worksheet.Cells["P" + Row].Formula = "=SUM(P" + StartingRowForToday + ":P" + (Row - 1) + ")";
                // 5) Days Working( Col : N )
                worksheet.Cells["N" + Row].Formula = "=SUM(N" + StartingRowForToday + ":N" + (Row - 1) + ")";
                // 6) MinorLoss( Col : U )
                worksheet.Cells["U" + Row].Formula = "=SUM(U" + StartingRowForToday + ":U" + (Row - 1) + ")";
                // 7) Breakdown( Col : V )
                worksheet.Cells["V" + Row].Formula = "=SUM(V" + StartingRowForToday + ":V" + (Row - 1) + ")";
                // 8) PowerOff (Col : W )
                worksheet.Cells["W" + Row].Formula = "=SUM(W" + StartingRowForToday + ":W" + (Row - 1) + ")";

                //5) Border for Above 4 & Around them.
                //worksheet.Cells["P" + StartingRowForToday + ":S" + "" + Row].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);

                //Push each Date Cummulative. Loop through ExcelAddress and insert formula
                var rangeIndividualSummarized = worksheet.Cells["X" + Row + ":" + finalLossCol + "" + Row];
                foreach (var rangeBase in rangeIndividualSummarized)
                {
                    string str = Convert.ToString(rangeBase);
                    string ExcelColAlphabet = Regex.Replace(str, "[^A-Z _]", "");
                    worksheet.Cells[rangeBase.Address].Formula = "=SUM(" + ExcelColAlphabet + StartingRowForToday + ":" + ExcelColAlphabet + "" + (Row - 1) + ")";
                    //var a = worksheet.Cells[rangeBase.Address].Value;
                    var blah1 = worksheet.Calculate("=SUM(" + ExcelColAlphabet + StartingRowForToday + ":" + ExcelColAlphabet + "" + (Row - 1) + ")");

                    double LossVal = 0;
                    double.TryParse(Convert.ToString(blah1), out LossVal);
                    if (LossVal != 0.0)
                    {
                        string LossName = Convert.ToString(worksheet.Cells[ExcelColAlphabet + 3].Value);
                        DataRow dr = DTConsolidatedLosses.AsEnumerable().LastOrDefault(r => r.Field<string>("Plant") == "Summarized" && r.Field<string>("CorrectedDate") == dateforMachine);
                        if (dr != null)
                        {
                            dr[LossName] = LossVal;
                        }
                    }
                }

                //Excel:: Border Around Cells.
                worksheet.Cells["B" + StartingRowForToday + ":" + finalLossCol + "" + Row].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);

                #endregion

                //UsedDateForExcel = UsedDateForExcel.AddDays(+1);
                double offsetE = 1;
                if (TabularType == "Day")
                {
                    offsetE = 1;
                }
                else if (TabularType == "Week")
                {
                    offsetE = end.Subtract(begining).TotalDays + 1;
                }
                else if (TabularType == "Month")
                {
                    offsetE = end.Subtract(begining).TotalDays + 1;
                }
                else if (TabularType == "Year")
                {
                    offsetE = end.Subtract(begining).TotalDays + 1;
                }

                UsedDateForExcel = UsedDateForExcel.AddDays(offsetE);
                Row++;
                l++;
            } while (UsedDateForExcel <= toDate);

            #region OverAll OEE and Stuff

            Row = 5;
            Sno = 1;
            var WCInvNoList = (from DataRow row in DTConsolidatedLosses.Rows
                               where row["WCInvNo"] != "Summarized"
                               select row["WCInvNo"]).Distinct();

            double OverAllOpTime = 0, OverAllAvailableTime = 0, OverAllSCTvsPP = 0, OverAllScrapQtyTime = 0, OverAllReworkTime = 0, OverAllMinorLoss = 0, OverAllBreakdown = 0, OverAllPowerOff = 0;
            foreach (var MacINV in WCInvNoList)
            {
                string WCInvNoStringOverAll = Convert.ToString(MacINV).Trim();
                DataRow drOverAll = DTConsolidatedLosses.AsEnumerable().FirstOrDefault(r => r.Field<string>("WCInvNo") == @WCInvNoStringOverAll);

                if (drOverAll != null)
                {
                    int MachineID = obj2.GetmachineID(WCInvNoStringOverAll);// V changes
                    //int MachineID = Convert.ToInt32(obj2.GetmachineList1(WCInvNoStringOverAll));

                    //int MachineID = db.tblmachinedetails.Where(m => m.MachineInvNo == WCInvNoStringOverAll).Select(m => m.MachineID).FirstOrDefault();
                    //string macDispName = Convert.ToString(obj2.GetmachineList2(WCInvNoStringOverAll));
                    string macDispName = obj2.GetmachineName(WCInvNoStringOverAll); // v Chnages machine name
                    // string macDispName = db.tblmachinedetails.Where(m => m.MachineInvNo == WCInvNoStringOverAll).Select(m => m.MachineDispName).FirstOrDefault();
                    List<string> HierarchyData = GetHierarchyData(MachineID);
                    worksheet.Cells["B" + Row].Value = Sno++;
                    worksheet.Cells["C" + Row].Value = HierarchyData[0];
                    worksheet.Cells["D" + Row].Value = HierarchyData[1];
                    worksheet.Cells["E" + Row].Value = HierarchyData[2];
                    worksheet.Cells["F" + Row].Value = macDispName;
                    worksheet.Cells["G" + Row].Value = HierarchyData[3];

                    worksheet.Cells["H" + Row].Value = (frmDate).ToString("yyyy-MM-dd");
                    worksheet.Cells["I" + Row].Value = (toDate).ToString("yyyy-MM-dd");

                    //OEE and Stuff
                    double OpTime = DTConsolidatedLosses.AsEnumerable().Where(x => x.Field<string>("WCInvNo") == @WCInvNoStringOverAll).Sum(x => x.Field<double>("OpTime"));
                    double AvailableTime = DTConsolidatedLosses.AsEnumerable().Where(x => x.Field<string>("WCInvNo") == @WCInvNoStringOverAll).Sum(x => x.Field<double>("AvailableTime"));
                    double SCTvsPP = DTConsolidatedLosses.AsEnumerable().Where(x => x.Field<string>("WCInvNo") == @WCInvNoStringOverAll).Sum(x => x.Field<double>("SCTvsPP"));
                    double ScrapQtyTime = DTConsolidatedLosses.AsEnumerable().Where(x => x.Field<string>("WCInvNo") == @WCInvNoStringOverAll).Sum(x => x.Field<double>("ScrapQtyTime"));
                    double ReworkTime = DTConsolidatedLosses.AsEnumerable().Where(x => x.Field<string>("WCInvNo") == @WCInvNoStringOverAll).Sum(x => x.Field<double>("ReworkTime"));
                    double CuttingTime = DTConsolidatedLosses.AsEnumerable().Where(x => x.Field<string>("WCInvNo") == @WCInvNoStringOverAll).Sum(x => x.Field<double>("CuttingTime"));
                    int DaysWorking = (int)DTConsolidatedLosses.AsEnumerable().Where(x => x.Field<string>("WCInvNo") == @WCInvNoStringOverAll).Sum(x => x.Field<double>("DaysWorking"));
                    int GodHours = (int)DTConsolidatedLosses.AsEnumerable().Where(x => x.Field<string>("WCInvNo") == @WCInvNoStringOverAll).Sum(x => x.Field<double>("GodHours"));
                    double TotalSTDHours = DTConsolidatedLosses.AsEnumerable().Where(x => x.Field<string>("WCInvNo") == @WCInvNoStringOverAll).Sum(x => x.Field<double>("TotalSTDHours"));
                    double RejectionHours = DTConsolidatedLosses.AsEnumerable().Where(x => x.Field<string>("WCInvNo") == @WCInvNoStringOverAll).Sum(x => x.Field<double>("RejectionHours"));
                    double MinorLoss = DTConsolidatedLosses.AsEnumerable().Where(x => x.Field<string>("WCInvNo") == @WCInvNoStringOverAll).Sum(x => x.Field<double>("MinorLoss"));
                    double Breakdown = DTConsolidatedLosses.AsEnumerable().Where(x => x.Field<string>("WCInvNo") == @WCInvNoStringOverAll).Sum(x => x.Field<double>("Breakdown"));
                    double PowerOff = DTConsolidatedLosses.AsEnumerable().Where(x => x.Field<string>("WCInvNo") == @WCInvNoStringOverAll).Sum(x => x.Field<double>("PowerOff"));

                    OverAllOpTime += OpTime;
                    OverAllAvailableTime += AvailableTime;
                    OverAllReworkTime += ReworkTime;
                    OverAllScrapQtyTime += ScrapQtyTime;
                    OverAllSCTvsPP += SCTvsPP;
                    OverAllMinorLoss += MinorLoss;
                    OverAllBreakdown += Breakdown;
                    OverAllPowerOff += PowerOff;

                    //Value InTo 
                    //worksheet.Cells["S" + Row].Value = Math.Round( OpTime, 2);
                    worksheet.Cells["R" + Row].Value = Math.Round((OpTime / 60), 2);
                    //worksheet.Cells["S" + Row].Formula = "=SUM(R" + Row + ",T" + Row + ",U" + Row + ",V" + Row + ")";
                    worksheet.Cells["S" + Row].Formula = "=SUM(R" + Row + ",T" + Row + ")";
                    worksheet.Cells["Q" + Row].Value = Math.Round(Convert.ToDouble(GodHours / 60),2);
                    worksheet.Cells["P" + Row].Value = (DaysWorking);
                    //worksheet.Cells["O" + Row].Value = Math.Round(RejectionHours, 2);
                    worksheet.Cells["O" + Row].Value = Math.Round(ScrapQtyTime, 2);
                    worksheet.Cells["N" + Row].Value = Math.Round((SCTvsPP / 60), 2);
                    worksheet.Cells["U" + Row].Value = Math.Round(MinorLoss / 60, 2);
                    worksheet.Cells["V" + Row].Value = Math.Round((Breakdown / 60), 2);
                    worksheet.Cells["W" + Row].Value = Math.Round((PowerOff / 60), 2);

                    #region A,E,Q & OEE
                    double AvaillabilityFactor = 0, EfficiencyFactor = 0, QualityFactor = 0;

                    double valA = 0;
                    valA = OpTime / AvailableTime;

                    //Availablity
                    if (valA > 0 && valA < 100)
                    {
                        worksheet.Cells["J" + Row].Value = Math.Round(valA * 100, 0);
                        AvaillabilityFactor = Math.Round(valA * 100, 0);
                        if (AvaillabilityFactor > 100)
                            AvaillabilityFactor = 100;
                        worksheet.Cells["J" + Row].Value = AvaillabilityFactor;
                    }
                    else
                    {
                        worksheet.Cells["J" + Row].Value = 0;
                        AvaillabilityFactor = 0;
                    }
                    if (AvaillabilityFactor != 0)
                    {
                        //Performance
                        if (SCTvsPP == -1 || SCTvsPP == 0)
                        {
                            EfficiencyFactor = 100;
                            worksheet.Cells["K" + Row].Value = Math.Round(EfficiencyFactor, 0);
                        }
                        else
                        {
                            EfficiencyFactor = Math.Round((SCTvsPP / (OpTime)) * 100, 0);
                            if (EfficiencyFactor > 0 && EfficiencyFactor <= 100)
                                worksheet.Cells["K" + Row].Value = EfficiencyFactor;
                            else if (EfficiencyFactor > 100)
                            {
                                EfficiencyFactor = 100;
                                worksheet.Cells["K" + Row].Value = 100;
                            }
                            else if (EfficiencyFactor <= 0)
                            {
                                EfficiencyFactor = 0;
                                worksheet.Cells["K" + Row].Value = 100;
                            }
                        }

                        //Quality
                        if (OpTime != 0)
                        {
                            QualityFactor = Math.Round(((OpTime - ScrapQtyTime - ReworkTime) / OpTime) * 100, 0);
                            if (QualityFactor > 0 && QualityFactor <= 100)
                            {
                                worksheet.Cells["L" + Row].Value = QualityFactor;
                            }
                            else if (QualityFactor > 100)
                            {
                                QualityFactor = 100;
                                worksheet.Cells["L" + Row].Value = 100;
                            }
                            else if (QualityFactor <= 0)
                            {
                                QualityFactor = 0;
                                worksheet.Cells["L" + Row].Value = 100;
                            }
                        }
                        else
                        {
                            QualityFactor = 0;
                            worksheet.Cells["L" + Row].Value = 0;
                        }

                    }
                    //###################
                    else
                    {
                        worksheet.Cells["K" + Row].Value = 0;//Performance
                        worksheet.Cells["L" + Row].Value = 0;//Quality
                    }


                    //OEE
                    if (AvaillabilityFactor <= 0 || EfficiencyFactor <= 0 || QualityFactor <= 0)
                    {
                        worksheet.Cells["M" + Row].Value = 0;
                    }
                    else
                    {
                        valA = Math.Round((AvaillabilityFactor / 100) * (EfficiencyFactor / 100) * (QualityFactor / 100) * 100, 2);
                        if (valA >= 0 && valA <= 100)
                        {
                            worksheet.Cells["M" + Row].Value = Math.Round(valA, 0);
                        }
                        else if (valA > 100)
                        {
                            worksheet.Cells["M" + Row].Value = 100;
                        }
                        else if (valA < 0)
                        {
                            worksheet.Cells["M" + Row].Value = 0;
                        }
                    }
                    #endregion

                    //Total of Losses
                    //worksheet.Cells["T" + Row].Formula = "=SUM(X" + Row + ":" + finalLossCol + "" + Row + ")";

                    worksheet.Cells["T" + Row].Formula = "=SUM(U" + Row + ",V" + Row + ",(X" + Row + ":" + finalLossCol + "" + Row + "))"; //Added by Ashok


                    //OverAll Losses 
                    var range = worksheet.Cells["X" + 3 + ":" + finalLossCol + "" + 3];
                    int i = 24;

                    foreach (var rangeBase in range)
                    {
                        string LossNameVal = Convert.ToString(rangeBase.Value);
                        string LossName = Convert.ToString(worksheet.Cells[rangeBase.Address].Value);
                        double LossValToExcel = DTConsolidatedLosses.AsEnumerable().Where(x => x.Field<string>("WCInvNo") == @WCInvNoStringOverAll).Sum(x => x.Field<double>(@LossNameVal));
                        string ColumnForThisLoss = ExcelColumnFromNumber(i++);
                        worksheet.Cells[ColumnForThisLoss + "" + Row].Value = Math.Round(LossValToExcel, 2);
                    }
                }
                Row++;
            }

            //Now Calculate OEE and Stuff with OverAll Variables calculated above "OverAll is the keywork in variable name"

            #region A,E,Q & OEE
            double OverAllAvaillabilityFactor = 0, OverAllEfficiencyFactor = 0, OverAllQualityFactor = 0;

            double OverAllvalA = 0;
            OverAllvalA = OverAllOpTime / OverAllAvailableTime;

            //Availablity
            if (OverAllvalA > 0)
            {
                worksheet.Cells["J" + Row].Value = Math.Round(OverAllvalA * 100, 0);
                OverAllAvaillabilityFactor = Math.Round(OverAllvalA * 100, 0);
            }
            else
            {
                worksheet.Cells["J" + Row].Value = 0;
                OverAllAvaillabilityFactor = 0;
            }
            if (OverAllAvaillabilityFactor != 0)
            {
                //Performance
                if (OverAllSCTvsPP == -1 || OverAllSCTvsPP == 0)
                {
                    OverAllEfficiencyFactor = 100;
                    worksheet.Cells["K" + Row].Value = Math.Round(OverAllEfficiencyFactor, 0);
                }
                else
                {
                    OverAllEfficiencyFactor = Math.Round((OverAllSCTvsPP / (OverAllOpTime)) * 100, 0);
                    if (OverAllEfficiencyFactor > 0 && OverAllEfficiencyFactor <= 100)
                        worksheet.Cells["K" + Row].Value = OverAllEfficiencyFactor;
                    else if (OverAllEfficiencyFactor > 100)
                    {
                        OverAllEfficiencyFactor = 100;
                        worksheet.Cells["K" + Row].Value = 100;
                    }
                    else if (OverAllEfficiencyFactor <= 0)
                    {
                        OverAllEfficiencyFactor = 0;
                        worksheet.Cells["K" + Row].Value = 100;
                    }
                }

                //Quality
                if (OverAllOpTime != 0)
                {
                    OverAllQualityFactor = Math.Round(((OverAllOpTime - OverAllScrapQtyTime - OverAllReworkTime) / OverAllOpTime) * 100, 0);
                    if (OverAllQualityFactor > 0 && OverAllQualityFactor <= 100)
                    {
                        worksheet.Cells["L" + Row].Value = OverAllQualityFactor;
                    }
                    else if (OverAllQualityFactor > 100)
                    {
                        OverAllQualityFactor = 100;
                        worksheet.Cells["L" + Row].Value = 100;
                    }
                    else if (OverAllQualityFactor <= 0)
                    {
                        OverAllQualityFactor = 0;
                        worksheet.Cells["L" + Row].Value = 100;
                    }
                }
                else
                {
                    OverAllQualityFactor = 0;
                    worksheet.Cells["L" + Row].Value = 0;
                }

            }
            //###################
            else
            {
                worksheet.Cells["K" + Row].Value = 0;//Performance
                worksheet.Cells["L" + Row].Value = 0;//Quality
            }


            //OEE
            if (OverAllAvaillabilityFactor <= 0 || OverAllEfficiencyFactor <= 0 || OverAllQualityFactor <= 0)
            {
                worksheet.Cells["M" + Row].Value = 0;
            }
            else
            {
                OverAllvalA = Math.Round((OverAllAvaillabilityFactor / 100) * (OverAllEfficiencyFactor / 100) * (OverAllQualityFactor / 100) * 100, 2);
                if (OverAllvalA >= 0 && OverAllvalA <= 100)
                {
                    worksheet.Cells["M" + Row].Value = Math.Round(OverAllvalA, 0);
                }
                else if (OverAllvalA > 100)
                {
                    worksheet.Cells["M" + Row].Value = 100;
                }
                else if (OverAllvalA < 0)
                {
                    worksheet.Cells["M" + Row].Value = 0;
                }
            }
            #endregion

            //Formulas to Calculate OverAll Summarized for Column N-T.
            int firstRowOfSummarizedOverAll = 5;
            int lastRowOfSummarizedOverAll = (4 + machin.Rows.Count);
            var rangeSummarizedOverall = worksheet.Cells["N" + Row + ":T" + Row];
            foreach (var rangeBase in rangeSummarizedOverall)
            {
                string column = Regex.Replace(rangeBase.Address, @"[\d-]", string.Empty);
                worksheet.Cells[rangeBase.Address].Formula = "=SUM(" + column + 5 + ":" + column + "" + (Row - 1) + ")"; ;
            }

            // Borders and Stuff for Cummulative Data.
            //Cellwise Border for Today
            worksheet.Cells["B3:" + finalLossCol + "" + Row].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            worksheet.Cells["B3:" + finalLossCol + "" + Row].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            worksheet.Cells["B3:" + finalLossCol + "" + Row].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            worksheet.Cells["B3:" + finalLossCol + "" + Row].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            //Excel:: Border Around Cells.
            worksheet.Cells["B5:" + finalLossCol + "" + (Row)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);
            worksheet.Cells["B" + Row + ":" + finalLossCol + "" + Row].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);


            ////Formula to Cummulate Losses
            //var rangeFinalLosses = worksheet.Cells["T" + Row + ":" + finalLossCol + "" + Row];
            //int j = 20;
            //foreach (var rangeBase in rangeFinalLosses)
            //{
            //    string ColumnForThisLoss = ExcelColumnFromNumber(j++);
            //    worksheet.Cells[ColumnForThisLoss + "" + Row].Formula = "=SUM(" + ColumnForThisLoss + 5 + ":" + ColumnForThisLoss + "" + (Row - 1) + ")";
            //}

            //Cummulative Losses into DT and Occured Losses into List and Identified and UnIdentified Losses
            //var rangeFinalLosses = worksheet.Cells["U" + Row + ":" + finalLossCol + "" + Row];
            var rangeFinalLosses = worksheet.Cells["X3:" + finalLossCol + "3"];
            List<KeyValuePair<string, double>> AllOccuredLosses = new List<KeyValuePair<string, double>>();
            int j = 21;
            double IdentifiedLoss = 0;
            double UnIdentifiedLoss = 0;
            foreach (var rangeBase in rangeFinalLosses)
            {
                string LossName = Convert.ToString(rangeBase.Value);
                string LossNameAddress = Convert.ToString(rangeBase.Address);

                double thisLossValue = DTConsolidatedLosses.AsEnumerable().Where(x => x.Field<string>("Plant") != "Summarized").Sum(x => x.Field<double>(@LossName));
                //string thisLossValueString = TimeFromSeconds(thisLossValue) == "0D, 00:00:00" ? "0" : TimeFromSeconds(thisLossValue);
                //TimeSpan time = TimeSpan.FromSeconds(thisLossValue);
                //string str = time.ToString(@"hh\:mm\:ss");
                string ColumnForThisLoss = ExcelColumnFromNumber(j++);
                worksheet.Cells[ColumnForThisLoss + "" + Row].Formula = "=SUM(" + ColumnForThisLoss + 5 + ":" + ColumnForThisLoss + "" + (Row - 1) + ")";
                // Double ValVal = (double) worksheet.Calculate("=SUM(" + ColumnForThisLoss + 5 + ":" + ColumnForThisLoss + "" + (Row - 1) + ")");
                // worksheet.Cells[ColumnForThisLoss + "" + Row].Value = Math.Round(thisLossValue,1);
                //worksheet.Cells[ColumnForThisLoss + "" + (Row + 1)].Value = Convert.ToString(thisLossValueString);
                if (thisLossValue > 0)
                {
                    if (LossName == "NoCode Entered" || LossName == "Unidentified Breakdown")
                    {
                        UnIdentifiedLoss += thisLossValue;
                    }
                    else
                    {
                        IdentifiedLoss += thisLossValue;
                    }
                    AllOccuredLosses.Add(new KeyValuePair<string, double>(LossNameAddress, Math.Round(thisLossValue, 1)));
                }
            }

            #endregion

            #region GRAPHS
            //Create the chart

            if (machin.Rows.Count > 0)
            {
                #region OEE and Stuff
                int TotalSummarizedRows = 0;
                TotalSummarizedRows = (machin.Rows.Count - 1); //-1 as data starts @ 5.

                //Its Not MacINV its MacDescription
                ExcelRange erLossesRangeMacInv = worksheet.Cells["F5:F" + (5 + TotalSummarizedRows)];
                //OEE
                ExcelChart chartOEE1 = (ExcelBarChart)worksheetGraph.Drawings.AddChart("barChartOEE", eChartType.ColumnClustered);
                var chartOEE = (ExcelBarChart)chartOEE1;
                chartOEE.SetSize(300, 390);
                chartOEE.SetPosition(60, 10);
                chartOEE.Title.Text = "OEE";
                //chart.Direction = eDirection.Column; // error: Property or indexer 'OfficeOpenXml.Drawing.Chart.ExcelBarChart.Direction' cannot be assigned to -- it is read only

                ExcelRange erLossesRangeOEE = worksheet.Cells["M5:M" + (5 + TotalSummarizedRows)];
                //chartOEE.YAxis.Fill.Color = System.Drawing.Color.Blue; //Working : 
                //erLossesRangeOEE.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;//(Black)Color on DATA    //r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                //erLossesRangeOEE.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);
                //chartOEE.SetDataPointStyle(chartOEE, erLossesRangeOEE, Color.Red);
                //chartOEE.Fill.Color = System.Drawing.Color.LightYellow; //Working :Charts Inside Background Color ~
                //chartOEE.PlotArea.Fill.Color = System.Drawing.Color.LightYellow; //Charts Inside Background Color ~
                chartOEE.Style = eChartStyle.None;
                chartOEE.Legend.Remove();
                chartOEE.DataLabel.ShowValue = true;
                chartOEE.YAxis.MaxValue = 100;
                chartOEE.YAxis.MinValue = 0;
                chartOEE.XAxis.Font.Size = 8;
                ////chartOEE.AxisX.IsMarginVisible = false;
                ////chartOEE.AxisY.IsMarginVisible = false;
                ////chartOEE.AxisX.LabelStyle.Format = "dd";
                ////chartOEE.AxisX.MajorGrid.LineWidth = 0;
                ////chartOEE.AxisY.MajorGrid.LineWidth = 0;
                ////chartOEE.AxisY.LabelStyle.Font = new Font("Consolas", 8);
                ////chartOEE.AxisX.LabelStyle.Font = new Font("Consolas", 8);
                //chartOEE.ChartAreas[0].AxisX.LineDashStyle.Dot;
                //chartOEE.ChartXml["barChartOEE"].x.MajorGrid.Enabled = false;
                //Chart1.ChartAreas["ChartArea1"].AxisY.MajorGrid.Enabled = false;
                chartOEE.YAxis.MinorTickMark = eAxisTickMark.None;
                chartOEE.XAxis.MajorTickMark = eAxisTickMark.None;
                ExcelChartSerie cSeries = chartOEE.Series.Add(erLossesRangeOEE, erLossesRangeMacInv);
                //chartOEE.Legend.Border.Fill.Color = Color.Yellow;
                //chartOEE1.VaryColors = true; //This Works
                ////Its readonly
                //chartOEE1.Series.Chart.Legend.Fill = System.Drawing.Color.Red;
                RemoveGridLines(ref chartOEE1);

                //Availability
                ExcelChart chartAvail1 = (ExcelBarChart)worksheetGraph.Drawings.AddChart("barChartAvail", eChartType.ColumnClustered);
                var chartAvail = (ExcelBarChart)chartAvail1;
                chartAvail.SetSize(300, 390);
                chartAvail.SetPosition(60, 320);
                ExcelRange erLossesRangechartAvail = worksheet.Cells["J5:J" + (5 + TotalSummarizedRows)];
                //erLossesRangechartAvail.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                //chartAvail.YAxis.Fill.Color = System.Drawing.Color.Orange;
                chartAvail.Title.Text = "Availability  ";
                chartAvail.Style = eChartStyle.Style12;
                chartAvail.Legend.Remove();
                chartAvail.YAxis.MaxValue = 100;
                chartAvail.YAxis.MinValue = 0;
                chartAvail.XAxis.Font.Size = 8;
                chartAvail.DataLabel.ShowValue = true;
                chartAvail.XAxis.MajorTickMark = eAxisTickMark.None;
                chartAvail.YAxis.MinorTickMark = eAxisTickMark.None;
                chartAvail.Series.Add(erLossesRangechartAvail, erLossesRangeMacInv);
                RemoveGridLines(ref chartAvail1);

                //Performance
                ExcelChart chartPerf1 = (ExcelBarChart)worksheetGraph.Drawings.AddChart("barChartPerf", eChartType.ColumnClustered);
                var chartPerf = (ExcelBarChart)chartPerf1;
                chartPerf.SetSize(300, 390);
                chartPerf.SetPosition(60, 630);
                chartPerf.XAxis.Font.Size = 8;
                chartPerf.Title.Text = "Performance ";
                ExcelRange erLossesRangechartPerf = worksheet.Cells["K5:K" + (5 + TotalSummarizedRows)];
                //erLossesRangechartPerf.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                //chartPerf.YAxis.Fill.Color = System.Drawing.Color.Yellow;
                chartPerf.Style = eChartStyle.Style11;
                chartPerf.Legend.Remove();
                chartPerf.YAxis.MaxValue = 100;
                chartPerf.YAxis.MinValue = 0;
                chartPerf.DataLabel.ShowValue = true;
                chartPerf.XAxis.MajorTickMark = eAxisTickMark.None;
                chartPerf.YAxis.MinorTickMark = eAxisTickMark.None;
                chartPerf.Series.Add(erLossesRangechartPerf, erLossesRangeMacInv);
                RemoveGridLines(ref chartPerf1);

                //Quality
                ExcelChart chartQual1 = (ExcelBarChart)worksheetGraph.Drawings.AddChart("barChartQual", eChartType.ColumnClustered);
                var chartQual = (ExcelBarChart)chartQual1;
                chartQual.SetSize(300, 390);
                chartQual.SetPosition(60, 940);
                ExcelRange erLossesRangechartQual = worksheet.Cells["L5:L" + (5 + TotalSummarizedRows)];
                chartQual.Title.Text = "Quality  ";
                chartQual.Style = eChartStyle.Style9;
                chartQual.Legend.Remove();
                chartQual.YAxis.MaxValue = 100;
                chartQual.YAxis.MinValue = 0;
                chartQual.XAxis.Font.Size = 8;
                chartQual.DataLabel.ShowValue = true;
                chartQual.XAxis.MajorTickMark = eAxisTickMark.None;
                chartQual.YAxis.MinorTickMark = eAxisTickMark.None;
                chartQual.Series.Add(erLossesRangechartQual, erLossesRangeMacInv);
                RemoveGridLines(ref chartQual1);

                #endregion

                #region Trend of OEE Over Selected DateRange
                List<double> ForAvg = new List<double>();
                UsedDateForExcel = Convert.ToDateTime(frmDate);
                string CellsOfOEEYAxis = null;
                string CellsOfOEEXAxis = null;
                for (int i = 0; i < TotalDay + 1; i++)
                {
                    string CorrectedDateString = UsedDateForExcel.ToString("yyyy-MM-dd");
                    var drs = from r in DTConsolidatedLosses.AsEnumerable()
                              where (r.Field<string>("CorrectedDate") == CorrectedDateString && r.Field<string>("Plant") == "Summarized")
                              select r;
                    int skipFirstRow = 0;
                    //if (drs != null)
                    //{
                    foreach (var cell in drs)
                    {
                        string CellRowString = cell["OEE"].ToString();
                        string CellRowDateString = cell["CorrectedDate"].ToString();

                        //Regex.Replace(str, "[^0-9 _]", "");
                        string extractedRowString = Regex.Replace(CellRowString, "[0-9 _]", string.Empty);
                        string extractedRowNumber = Regex.Replace(CellRowString, "[^0-9 _]", string.Empty);
                        if (CellsOfOEEXAxis == null)
                        {
                            CellsOfOEEXAxis = "H" + (Convert.ToInt32(extractedRowNumber) - 1);
                        }
                        else
                        {
                            CellsOfOEEXAxis += ",H" + (Convert.ToInt32(extractedRowNumber) - 1);
                        }

                        if (CellsOfOEEYAxis == null)
                        {
                            CellsOfOEEYAxis = CellRowString;
                            double CellVal = 0;
                            if (double.TryParse(Convert.ToString(worksheet.Cells[CellRowString].Value), out CellVal))
                            {
                                ForAvg.Add(CellVal);
                            }
                        }
                        else
                        {
                            CellsOfOEEYAxis += "," + CellRowString;
                            double CellVal = 0;
                            if (double.TryParse(Convert.ToString(worksheet.Cells[CellRowString].Value), out CellVal))
                            {
                                ForAvg.Add(CellVal);
                            }
                        }
                    }
                    //}
                    //else
                    //{}
                    UsedDateForExcel = UsedDateForExcel.AddDays(+1);
                }


                ExcelRange erLossesRangechartTop5LossesvALUE = worksheet.Cells[CellsOfOEEYAxis];
                ExcelRange erLossesRangechartTop5LossesNAMES = worksheet.Cells[CellsOfOEEXAxis];
                ExcelChart chartOEETrend1 = (ExcelLineChart)worksheetGraph.Drawings.AddChart("TrendChartOEE", eChartType.LineMarkers);

                #region Experiment for Hybrid Graph Success
                ////Now for the second chart type we use the chart.PlotArea.ChartTypes collection...
                //worksheetGraph.Cells["B9:B10"].Value = "Avg";
                //worksheetGraph.Cells["A9:A10"].Value = 30;
                //var chartType2 = chartOEETrend1.PlotArea.ChartTypes.Add(eChartType.Line);
                //var serie2 = chartType2.Series.Add(worksheetGraph.Cells["A9:A10"], worksheetGraph.Cells["B9:B10"]);

                #endregion

                for (int i = 0; i < ForAvg.Count; i++)
                {
                    worksheetGraph.Cells["A" + (12 + i)].Value = "Avg";
                    worksheetGraph.Cells["B" + (12 + i)].Value = ForAvg.Sum() / ForAvg.Count;
                }

                ////Based on TotalDays :: insert AvgValue into Cells
                // var AvgVal = worksheetGraph.Calculate(" =AVERAGE(erLossesRangechartTop5LossesvALUE)");
                // var AvgVal1 = worksheetGraph.Calculate(" =AVERAGE('" + erLossesRangechartTop5LossesvALUE + "')");
                //worksheetGraph.Cells["F9"].Formula = " =AVERAGE(" + erLossesRangechartTop5LossesvALUE + ")";
                //worksheetGraph.Cells["B9:B" + (9 + ForAvg.Count)].Value = "Avg";
                //worksheetGraph.Cells["A9:A" + (9 + OEEsForAvg.Count)].Value = AvgVal;
                //string blah = AvgVal.ToString();

                //worksheetGraph.Cells["C9:C" + (9 + TotalDay + 1)].Value = AvgVal;
                var chartType2 = chartOEETrend1.PlotArea.ChartTypes.Add(eChartType.Line);
                var serie2 = chartType2.Series.Add(worksheetGraph.Cells["B12:B" + (12 + ForAvg.Count - 1)], worksheetGraph.Cells["B12:B" + (12 + ForAvg.Count - 1)]);

                var chartOEETrend = (ExcelLineChart)chartOEETrend1;
                chartOEETrend.SetSize(300, 300);
                chartOEETrend.SetPosition(450, 10);
                chartOEETrend.Title.Text = "OEE ";
                chartOEETrend.Style = eChartStyle.Style4;
                chartOEETrend.Legend.Remove();
                chartOEETrend.YAxis.MaxValue = 100;
                chartOEETrend.DataLabel.ShowValue = true;
                chartOEETrend.XAxis.MajorTickMark = eAxisTickMark.None;
                chartOEETrend.YAxis.MinorTickMark = eAxisTickMark.None;
                chartOEETrend.Series.Add(erLossesRangechartTop5LossesvALUE, erLossesRangechartTop5LossesNAMES);
                RemoveGridLines(ref chartOEETrend1);
                #endregion

                #region Trend of Availability Over Selected DateRange
                UsedDateForExcel = Convert.ToDateTime(frmDate);
                string CellsOfAvailYAxis = null;
                string CellsOfAvailXAxis = null;
                //ForAvg.RemoveAll(m=>m > -1);
                ForAvg.Clear();
                for (int i = 0; i < TotalDay + 1; i++)
                {
                    string CorrectedDateString = UsedDateForExcel.ToString("yyyy-MM-dd");
                    var drs = from r in DTConsolidatedLosses.AsEnumerable()
                              where (r.Field<string>("CorrectedDate") == CorrectedDateString && r.Field<string>("Plant") == "Summarized")
                              select r;
                    int skipFirstRow = 0;
                    //if (drs != null)
                    //{
                    foreach (var cell in drs)
                    {
                        string CellRowString = cell["Avail"].ToString();
                        string CellRowDateString = cell["CorrectedDate"].ToString();

                        //Regex.Replace(str, "[^0-9 _]", "");
                        string extractedRowString = Regex.Replace(CellRowString, "[0-9 _]", string.Empty);
                        string extractedRowNumber = Regex.Replace(CellRowString, "[^0-9 _]", string.Empty);
                        if (CellsOfAvailXAxis == null)
                        {
                            CellsOfAvailXAxis = "H" + (Convert.ToInt32(extractedRowNumber) - 1);
                        }
                        else
                        {
                            CellsOfAvailXAxis += ",H" + (Convert.ToInt32(extractedRowNumber) - 1);
                        }

                        if (CellsOfAvailYAxis == null)
                        {
                            CellsOfAvailYAxis = CellRowString;
                            double CellVal = 0;
                            if (double.TryParse(Convert.ToString(worksheet.Cells[CellRowString].Value), out CellVal))
                            {
                                ForAvg.Add(CellVal);
                            }
                        }
                        else
                        {
                            CellsOfAvailYAxis += "," + CellRowString;
                            double CellVal = 0;
                            if (double.TryParse(Convert.ToString(worksheet.Cells[CellRowString].Value), out CellVal))
                            {
                                ForAvg.Add(CellVal);
                            }
                        }

                    }
                    //}
                    //else
                    //{}
                    UsedDateForExcel = UsedDateForExcel.AddDays(+1);
                }

                for (int i = 0; i < ForAvg.Count; i++)
                {
                    worksheetGraph.Cells["C" + (12 + i)].Value = "Avg";
                    worksheetGraph.Cells["D" + (12 + i)].Value = ForAvg.Sum() / ForAvg.Count;
                }

                erLossesRangechartTop5LossesvALUE = worksheet.Cells[CellsOfAvailYAxis];
                erLossesRangechartTop5LossesNAMES = worksheet.Cells[CellsOfAvailXAxis];
                ExcelChart chartAvailTrend1 = (ExcelLineChart)worksheetGraph.Drawings.AddChart("TrendChartAvail", eChartType.LineMarkers);

                var chartAvailType2 = chartAvailTrend1.PlotArea.ChartTypes.Add(eChartType.Line);
                var Availserie2 = chartAvailType2.Series.Add(worksheetGraph.Cells["D12:D" + (12 + ForAvg.Count - 1)], worksheetGraph.Cells["C12:C" + (12 + ForAvg.Count - 1)]);

                var chartAvailTrend = (ExcelLineChart)chartAvailTrend1;
                chartAvailTrend.SetSize(300, 300);
                chartAvailTrend.SetPosition(450, 330);
                chartAvailTrend.Title.Text = "Availability ";
                chartAvailTrend.Style = eChartStyle.Style3;
                chartAvailTrend.Legend.Remove();
                chartAvailTrend.YAxis.MaxValue = 100;
                chartAvailTrend.DataLabel.ShowValue = true;
                chartAvailTrend.YAxis.MinorTickMark = eAxisTickMark.None;
                chartAvailTrend.XAxis.MajorTickMark = eAxisTickMark.None;
                //chartAvailTrend.DataLabel.Position = eLabelPosition.InBase;
                chartAvailTrend.Series.Add(erLossesRangechartTop5LossesvALUE, erLossesRangechartTop5LossesNAMES);
                RemoveGridLines(ref chartAvailTrend1);
                #endregion

                #region Trend of Performance Over Selected DateRange
                UsedDateForExcel = Convert.ToDateTime(frmDate);
                string CellsOfPerfYAxis = null;
                string CellsOfPerfXAxis = null;
                ForAvg.Clear();
                for (int i = 0; i < TotalDay + 1; i++)
                {
                    string CorrectedDateString = UsedDateForExcel.ToString("yyyy-MM-dd");
                    var drs = from r in DTConsolidatedLosses.AsEnumerable()
                              where (r.Field<string>("CorrectedDate") == CorrectedDateString && r.Field<string>("Plant") == "Summarized")
                              select r;
                    int skipFirstRow = 0;
                    //if (drs != null)
                    //{
                    foreach (var cell in drs)
                    {
                        string CellRowString = cell["Perf"].ToString();
                        string CellRowDateString = cell["CorrectedDate"].ToString();

                        //Regex.Replace(str, "[^0-9 _]", "");
                        string extractedRowString = Regex.Replace(CellRowString, "[0-9 _]", string.Empty);
                        string extractedRowNumber = Regex.Replace(CellRowString, "[^0-9 _]", string.Empty);
                        if (CellsOfPerfXAxis == null)
                        {
                            CellsOfPerfXAxis = "H" + (Convert.ToInt32(extractedRowNumber) - 1);
                        }
                        else
                        {
                            CellsOfPerfXAxis += ",H" + (Convert.ToInt32(extractedRowNumber) - 1);
                        }

                        if (CellsOfPerfYAxis == null)
                        {
                            CellsOfPerfYAxis = CellRowString;
                            double CellVal = 0;
                            if (double.TryParse(Convert.ToString(worksheet.Cells[CellRowString].Value), out CellVal))
                            {
                                ForAvg.Add(CellVal);
                            }
                        }
                        else
                        {
                            CellsOfPerfYAxis += "," + CellRowString;
                            double CellVal = 0;
                            if (double.TryParse(Convert.ToString(worksheet.Cells[CellRowString].Value), out CellVal))
                            {
                                ForAvg.Add(CellVal);
                            }
                        }

                    }
                    //}
                    //else
                    //{}
                    UsedDateForExcel = UsedDateForExcel.AddDays(+1);
                }

                for (int i = 0; i < ForAvg.Count; i++)
                {
                    worksheetGraph.Cells["E" + (12 + i)].Value = "Avg";
                    worksheetGraph.Cells["F" + (12 + i)].Value = ForAvg.Sum() / ForAvg.Count;
                }

                erLossesRangechartTop5LossesvALUE = worksheet.Cells[CellsOfPerfYAxis];
                erLossesRangechartTop5LossesNAMES = worksheet.Cells[CellsOfPerfXAxis];
                ExcelChart chartPerfTrend1 = (ExcelLineChart)worksheetGraph.Drawings.AddChart("TrendChartPerf", eChartType.LineMarkers);

                var chartPerfType2 = chartPerfTrend1.PlotArea.ChartTypes.Add(eChartType.Line);
                var Perfserie2 = chartPerfType2.Series.Add(worksheetGraph.Cells["F12:F" + (12 + ForAvg.Count - 1)], worksheetGraph.Cells["E12:E" + (12 + ForAvg.Count - 1)]);

                var chartPerfTrend = (ExcelLineChart)chartPerfTrend1;
                //if (TotalDay < 7)
                //{
                chartPerfTrend.SetSize(300, 300);
                chartPerfTrend.SetPosition(450, 640);
                //}

                chartPerfTrend.Title.Text = "Performance ";
                chartPerfTrend.Style = eChartStyle.Style2;
                chartPerfTrend.Legend.Remove();
                chartPerfTrend.DataLabel.ShowValue = true;
                chartPerfTrend.YAxis.MaxValue = 100;
                chartPerfTrend.XAxis.MajorTickMark = eAxisTickMark.None;
                chartPerfTrend.YAxis.MinorTickMark = eAxisTickMark.None;
                chartPerfTrend.Series.Add(erLossesRangechartTop5LossesvALUE, erLossesRangechartTop5LossesNAMES);
                RemoveGridLines(ref chartPerfTrend1);

                #endregion

                #region Trend of Quality Over Selected DateRange
                UsedDateForExcel = Convert.ToDateTime(frmDate);
                string CellsOfQualYAxis = null;
                string CellsOfQualXAxis = null;
                ForAvg.Clear();
                for (int i = 0; i < TotalDay + 1; i++)
                {
                    string CorrectedDateString = UsedDateForExcel.ToString("yyyy-MM-dd");
                    var drs = from r in DTConsolidatedLosses.AsEnumerable()
                              where (r.Field<string>("CorrectedDate") == CorrectedDateString && r.Field<string>("Plant") == "Summarized")
                              select r;
                    int skipFirstRow = 0;
                    //if (drs != null)
                    //{
                    foreach (var cell in drs)
                    {
                        string CellRowString = cell["Qual"].ToString();
                        string CellRowDateString = cell["CorrectedDate"].ToString();

                        //Regex.Replace(str, "[^0-9 _]", "");
                        string extractedRowString = Regex.Replace(CellRowString, "[0-9 _]", string.Empty);
                        string extractedRowNumber = Regex.Replace(CellRowString, "[^0-9 _]", string.Empty);
                        if (CellsOfQualXAxis == null)
                        {
                            CellsOfQualXAxis = "H" + (Convert.ToInt32(extractedRowNumber) - 1);
                        }
                        else
                        {
                            CellsOfQualXAxis += ",H" + (Convert.ToInt32(extractedRowNumber) - 1);
                        }

                        if (CellsOfQualYAxis == null)
                        {
                            CellsOfQualYAxis = CellRowString;
                            double CellVal = 0;
                            if (double.TryParse(Convert.ToString(worksheet.Cells[CellRowString].Value), out CellVal))
                            {
                                ForAvg.Add(CellVal);
                            }
                        }
                        else
                        {
                            CellsOfQualYAxis += "," + CellRowString;
                            double CellVal = 0;
                            if (double.TryParse(Convert.ToString(worksheet.Cells[CellRowString].Value), out CellVal))
                            {
                                ForAvg.Add(CellVal);
                            }
                        }

                    }
                    //}
                    //else
                    //{}
                    UsedDateForExcel = UsedDateForExcel.AddDays(+1);
                }

                for (int i = 0; i < ForAvg.Count; i++)
                {
                    worksheetGraph.Cells["G" + (12 + i)].Value = "Avg";
                    worksheetGraph.Cells["H" + (12 + i)].Value = ForAvg.Sum() / ForAvg.Count;
                }


                erLossesRangechartTop5LossesvALUE = worksheet.Cells[CellsOfQualYAxis];
                erLossesRangechartTop5LossesNAMES = worksheet.Cells[CellsOfQualXAxis];
                ExcelChart chartQualTrend1 = (ExcelLineChart)worksheetGraph.Drawings.AddChart("TrendChartQual", eChartType.LineMarkers);

                var chartQualType2 = chartQualTrend1.PlotArea.ChartTypes.Add(eChartType.Line);
                var Qualserie2 = chartQualType2.Series.Add(worksheetGraph.Cells["H12:H" + (12 + ForAvg.Count - 1)], worksheetGraph.Cells["G12:G" + (12 + ForAvg.Count - 1)]);
                // var Perfserie2 = chartPerfType2.Series.Add(worksheetGraph.Cells["F12:F" + (12 + ForAvg.Count - 1)], worksheetGraph.Cells["E12:E" + (12 + ForAvg.Count - 1)]);

                var chartQualTrend = (ExcelLineChart)chartQualTrend1;
                chartQualTrend.SetSize(300, 300);
                chartQualTrend.SetPosition(450, 950);
                chartQualTrend.Title.Text = "Quality ";
                chartQualTrend.Style = eChartStyle.Style1;
                chartQualTrend.Legend.Remove();
                chartQualTrend.YAxis.MaxValue = 100;
                chartQualTrend.YAxis.MinValue = 0;
                chartQualTrend.ShowDataLabelsOverMaximum = true;
                chartQualTrend.DataLabel.ShowValue = true;
                chartQualTrend.DataLabel.ShowBubbleSize = true;
                chartQualTrend.XAxis.MajorTickMark = eAxisTickMark.None;
                chartQualTrend.YAxis.MinorTickMark = eAxisTickMark.None;
                chartQualTrend.Series.Add(erLossesRangechartTop5LossesvALUE, erLossesRangechartTop5LossesNAMES);
                RemoveGridLines(ref chartQualTrend1);

                #endregion

                #region lOSSES TOP 5 GRAPH
                //1. Get Top 5 Losses ColName in excel.
                //2. Generate the comma seperated String format .
                //sort the list
                AllOccuredLosses.Sort(Compare2);
                AllOccuredLosses = AllOccuredLosses.OrderByDescending(x => x.Value).ToList();

                //Now construct string from top 5 losses.
                int LooperTop5 = 0;
                string CellsOfTop5LossColNames = null;
                string CellsOfTop5LossColValues = null;
                foreach (KeyValuePair<string, double> loss in AllOccuredLosses)
                {
                    if (LooperTop5 < 5)
                    {
                        string a = loss.Key;
                        double b = loss.Value;

                        var outputJustColName = Regex.Replace(a, @"[\d-]", string.Empty);
                        //string LossCol = Convert.ToString(outputJustColName);
                        string LossCol = a;
                        if (LooperTop5 == 0)
                        {
                            CellsOfTop5LossColNames = LossCol;
                            CellsOfTop5LossColValues = outputJustColName + Row;
                        }
                        else
                        {
                            CellsOfTop5LossColNames += "," + LossCol;
                            CellsOfTop5LossColValues += "," + outputJustColName + Row;
                        }
                    }
                    else
                    {
                        break;
                    }
                    LooperTop5++;
                }

                //To make sure it doesn't through error when there's no data.
                if (AllOccuredLosses.Count > 0)
                {
                    ExcelChart chartTop5Losses1 = (ExcelBarChart)worksheetGraph.Drawings.AddChart("barChartTop5Losses", eChartType.ColumnClustered);
                    var chartTop5Losses = (ExcelBarChart)chartTop5Losses1;
                    chartTop5Losses.SetSize(500, 400);
                    chartTop5Losses.SetPosition(760, 10);
                    //string blah = "CY11,CZ11,DA11,DC11,DD11"; //This Works 
                    //ExcelRange erLossesRangechartTop5LossesvALUE = worksheet.Cells[blah];
                    //ExcelRange erLossesRangechartTop5LossesvALUE = worksheet.Cells["CY11,CZ11,DA11,DC11,DD11"];
                    //ExcelRange erLossesRangechartTop5LossesNAMES = worksheet.Cells["CY3,CZ3,DA3,DC3,DD3"];

                    erLossesRangechartTop5LossesvALUE = worksheet.Cells[CellsOfTop5LossColValues];
                    erLossesRangechartTop5LossesNAMES = worksheet.Cells[CellsOfTop5LossColNames];
                    chartTop5Losses.Title.Text = "LOSSES (Hrs)";
                    chartTop5Losses.Style = eChartStyle.Style19;
                    chartTop5Losses.Legend.Remove();
                    chartTop5Losses.DataLabel.ShowValue = true;
                    //chartTop5Losses.DataLabel.Font.Size = 8;
                    //chartTop5Losses.Legend.Font.Size = 8;
                    chartTop5Losses.XAxis.Font.Size = 8;
                    chartTop5Losses.YAxis.MinorTickMark = eAxisTickMark.None;
                    chartTop5Losses.XAxis.MajorTickMark = eAxisTickMark.None;
                    chartTop5Losses.Series.Add(erLossesRangechartTop5LossesvALUE, erLossesRangechartTop5LossesNAMES);
                    RemoveGridLines(ref chartTop5Losses1);

                }
                #endregion

                #region Identified & UnIdentified Losses "scary"
                worksheetGraph.Cells["A1"].Value = "Ratio of Losses";
                //worksheetGraph.Cells["A2"].Value = "UnIdentifiedLoss";

                double IdentifiedLossPercentage = (IdentifiedLoss / (IdentifiedLoss + UnIdentifiedLoss)) * 100;
                double UnIdentifiedLossPercentage = (UnIdentifiedLoss / (IdentifiedLoss + UnIdentifiedLoss)) * 100;
                worksheetGraph.Cells["B1"].Value = Math.Round(IdentifiedLossPercentage, 0);
                worksheetGraph.Cells["B2"].Value = Math.Round(UnIdentifiedLossPercentage, 0);

                erLossesRangechartTop5LossesvALUE = worksheetGraph.Cells["B1"];
                ExcelRange erLossesRangechartTop5LossesvALUE1 = worksheetGraph.Cells["B2"];
                erLossesRangechartTop5LossesNAMES = worksheetGraph.Cells["A1"];

                ExcelChart chartIDAndUnID1 = (ExcelBarChart)worksheetGraph.Drawings.AddChart("TypesOfLosses", eChartType.ColumnStacked);
                var chartIDAndUnID = (ExcelBarChart)chartIDAndUnID1;
                chartIDAndUnID.SetSize(500, 350);
                chartIDAndUnID.SetPosition(760, 520);

                chartIDAndUnID.Title.Text = "Identified Losses  ";
                chartIDAndUnID.Style = eChartStyle.Style18;
                chartIDAndUnID.Legend.Position = eLegendPosition.Bottom;
                //chartIDAndUnID.Legend.Remove();
                chartIDAndUnID.YAxis.MaxValue = 100;
                chartIDAndUnID.YAxis.MinValue = 0;
                chartIDAndUnID.Locked = false;
                chartIDAndUnID.PlotArea.Border.Width = 0;
                chartIDAndUnID.YAxis.MinorTickMark = eAxisTickMark.None;
                chartIDAndUnID.DataLabel.ShowValue = true;
                //chartAllLosses.DataLabel.ShowValue = true;
                var thisYearSeries = (ExcelChartSerie)(chartIDAndUnID.Series.Add(erLossesRangechartTop5LossesvALUE, erLossesRangechartTop5LossesNAMES));
                thisYearSeries.Header = "Identified Losses";
                var lastYearSeries = (ExcelChartSerie)(chartIDAndUnID.Series.Add(erLossesRangechartTop5LossesvALUE1, erLossesRangechartTop5LossesNAMES));
                lastYearSeries.Header = "UnIdentified Losses";
                RemoveGridLines(ref chartIDAndUnID1);

                #region OLD
                ////////////////////
                //have to remove cat nodes from each series so excel autonums 1 and 2 in xaxis
                //var chartXml = chartIDAndUnID.ChartXml;
                //var nsm = new XmlNamespaceManager(chartXml.NameTable);

                //var nsuri = chartXml.DocumentElement.NamespaceURI;
                //nsm.AddNamespace("c", nsuri);

                ////Get the Series ref and its cat
                //var serNodes = chartXml.SelectNodes("c:chartSpace/c:chart/c:plotArea/c:bar3DChart/c:ser", nsm);
                //foreach (XmlNode serNode in serNodes)
                //{
                //    //Cell any cell reference and replace it with a string literal list
                //    var catNode = serNode.SelectSingleNode("c:cat", nsm);
                //    catNode.RemoveAll();

                //    //Create the string list elements
                //    var ptCountNode = chartXml.CreateElement("c:ptCount", nsuri);
                //    ptCountNode.Attributes.Append(chartXml.CreateAttribute("val", nsuri));
                //    ptCountNode.Attributes[0].Value = "2";

                //    var v0Node = chartXml.CreateElement("c:v", nsuri);
                //    v0Node.InnerText = "opening";
                //    var pt0Node = chartXml.CreateElement("c:pt", nsuri);
                //    pt0Node.AppendChild(v0Node);
                //    pt0Node.Attributes.Append(chartXml.CreateAttribute("idx", nsuri));
                //    pt0Node.Attributes[0].Value = "0";

                //    var v1Node = chartXml.CreateElement("c:v", nsuri);
                //    v1Node.InnerText = "closing";
                //    var pt1Node = chartXml.CreateElement("c:pt", nsuri);
                //    pt1Node.AppendChild(v1Node);
                //    pt1Node.Attributes.Append(chartXml.CreateAttribute("idx", nsuri));
                //    pt1Node.Attributes[0].Value = "1";

                //    //Create the string list node
                //    var strLitNode = chartXml.CreateElement("c:strLit", nsuri);
                //    strLitNode.AppendChild(ptCountNode);
                //    strLitNode.AppendChild(pt0Node);
                //    strLitNode.AppendChild(pt1Node);
                //    catNode.AppendChild(strLitNode);
                //}
                //pck.Save();
                #endregion
                #region Experiment to Send Data to Excel Chart in Template
                //OfficeOpenXml.FormulaParsing.Excel.Application xlApp;
                //Excel.Workbook xlWorkBook;
                //Excel.Worksheet xlWorkSheet;
                //object misValue = System.Reflection.Missing.Value;

                //Excel.ChartObjects xlCharts = (Excel.ChartObjects)xlWorkSheet.ChartObjects(Type.Missing);
                //Excel.ChartObject myChart = (Excel.ChartObject)xlCharts.Add(10, 80, 300, 250);
                //Excel.Chart chartPage = myChart.Chart;

                //Excel.Range chartRange;
                //chartRange = xlWorkSheet.get_Range("A1", "d5");
                //chartPage.SetSourceData(chartRange, misValue);
                //chartPage.ChartType = Excel.XlChartType.xlColumnClustered;

                //xlWorkBook.SaveAs("csharp.net-informations.xls", Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
                //xlWorkBook.Close(true, misValue, misValue);
                //xlApp.Quit();

                //var piechart = worksheet.Drawings["A"] as ExcelBarChart;
                //piechart.Style = eChartStyle.Style26;
                //chartIDAndUnID.Style = eChartStyle.Style26;

                //////////////////////////////


                #endregion

                #endregion End of Identified & UnIdentified Losses

                #region All Losses Chart

                CellsOfTop5LossColNames = null;
                CellsOfTop5LossColValues = null;
                int Looper = 0;
                foreach (KeyValuePair<string, double> loss in AllOccuredLosses)
                {
                    string a = loss.Key;
                    double b = loss.Value;

                    var outputJustColName = Regex.Replace(a, @"[\d-]", string.Empty);
                    //string LossCol = Convert.ToString(outputJustColName);
                    string LossCol = a;
                    if (a == "0")
                    {
                    }
                    if (Looper == 0)
                    {
                        CellsOfTop5LossColNames = LossCol;
                        CellsOfTop5LossColValues = outputJustColName + Row;
                    }
                    else
                    {
                        CellsOfTop5LossColNames += "," + LossCol;
                        CellsOfTop5LossColValues += "," + outputJustColName + Row;
                    }
                    Looper++;
                }

                if (AllOccuredLosses.Count > 0)
                {
                    ExcelChart chartAllLosses1 = (ExcelBarChart)worksheetGraph.Drawings.AddChart("barChartAllLosses", eChartType.ColumnClustered);
                    var chartAllLosses = (ExcelBarChart)chartAllLosses1;
                    chartAllLosses.SetSize(1200, 500);
                    chartAllLosses.SetPosition(1170, 10);
                    erLossesRangechartTop5LossesvALUE = worksheet.Cells[CellsOfTop5LossColValues];
                    erLossesRangechartTop5LossesNAMES = worksheet.Cells[CellsOfTop5LossColNames];
                    chartAllLosses.Title.Text = "All LOSSES" + "(Hrs)";
                    chartAllLosses.Style = eChartStyle.Style25;
                    chartAllLosses.Legend.Remove();
                    chartAllLosses.DataLabel.ShowValue = true;
                    //chartAllLosses.DataLabel.Font.Size = 8;
                    //chartAllLosses.Legend.Font.Size = 8;
                    chartAllLosses.YAxis.MinorTickMark = eAxisTickMark.None;
                    chartAllLosses.XAxis.MajorTickMark = eAxisTickMark.None;
                    chartAllLosses.XAxis.Font.Size = 8;
                    chartAllLosses.Series.Add(erLossesRangechartTop5LossesvALUE, erLossesRangechartTop5LossesNAMES);

                    //Get reference of Graph to Remove GridLines
                    RemoveGridLines(ref chartAllLosses1);

                }

                #endregion

                #region Experiment Linq on Excel . IT's a Success

                //foreach (var MacINV in WCInvNoList)
                //{
                //    string WCInvNoString = Convert.ToString(MacINV);

                //    //Select all cells in column G with this MacINV
                //    var queryLinq = (from cell in worksheet.Cells["G:G"]
                //                     where cell.Value is string && (string)cell.Value == @WCInvNoString 
                //                     select cell);
                //    foreach (var cell in queryLinq)
                //    {
                //        string CellRowString = cell.Address;
                //    }

                //}
                #endregion

                #region  Losses Trend :: All 5 Topper's
                var queryLinq = (from cell in worksheet.Cells["C:C"]
                                 where cell.Value is string && (string)cell.Value == "Summarized For"
                                 select cell);
                int LossesLooper = 1;
                int PositionY = 1690;
                int PositionX = 10;

                int GraphNo = 0, maxYVal = 0;
                foreach (var Loss in AllOccuredLosses)
                {
                    maxYVal = Convert.ToInt32(Loss.Value);
                    break;
                }
                foreach (var Loss in AllOccuredLosses)
                {
                    if (LossesLooper <= 5)
                    {
                        ForAvg.Clear();
                        CellsOfOEEYAxis = null;
                        CellsOfOEEXAxis = null;
                        string CellColString = Convert.ToString(Loss.Key);
                        string LossName = Convert.ToString(worksheet.Cells[CellColString].Value);
                        foreach (var cell in queryLinq)
                        {
                            string CellRowString = cell.Address;
                            string RowNum = Regex.Replace(CellRowString, "[^0-9 _]", string.Empty);
                            string ColName = Regex.Replace(CellColString, "[0-9 _]", string.Empty);

                            if (CellsOfOEEXAxis == null)
                            {
                                CellsOfOEEXAxis = "H" + (Convert.ToInt32(RowNum) - 1);
                            }
                            else
                            {
                                CellsOfOEEXAxis += ",H" + (Convert.ToInt32(RowNum) - 1);
                            }

                            if (CellsOfOEEYAxis == null)
                            {
                                string colPlusrow = ColName + RowNum;
                                CellsOfOEEYAxis = colPlusrow;
                                double CellVal = 0;
                                string CellValString = worksheet.Calculate(worksheet.Cells[colPlusrow].Formula).ToString();
                                if (double.TryParse(CellValString, out CellVal))
                                {
                                    ForAvg.Add(CellVal);
                                }
                                //if (double.TryParse(Convert.ToString(worksheet.Cells[colPlusrow].Value), out CellVal))
                                //{
                                //    ForAvg.Add(CellVal);
                                //}
                            }
                            else
                            {
                                string colPlusrow = ColName + RowNum;
                                CellsOfOEEYAxis += "," + colPlusrow;
                                double CellVal = 0;

                                string CellValString = worksheet.Calculate(worksheet.Cells[colPlusrow].Formula).ToString();
                                if (double.TryParse(CellValString, out CellVal))
                                {
                                    ForAvg.Add(CellVal);
                                }
                            }
                        }

                        ExcelRange erTopLossesTrendYValues = worksheet.Cells[CellsOfOEEYAxis];
                        ExcelRange erTopLossesTrendXNames = worksheet.Cells[CellsOfOEEXAxis];
                        ExcelChart chartLossses1Trend1 = (ExcelLineChart)worksheetGraph.Drawings.AddChart("TrendChartTop" + LossesLooper, eChartType.LineMarkers);

                        GraphNo++;
                        if (GraphNo == 1)
                        {
                            for (int i = 0; i < ForAvg.Count; i++)
                            {
                                worksheetGraph.Cells["A" + (50 + i)].Value = "Avg";
                                worksheetGraph.Cells["B" + (50 + i)].Value = ForAvg.Sum() / ForAvg.Count;
                            }
                            var chartTypeL1 = chartLossses1Trend1.PlotArea.ChartTypes.Add(eChartType.Line);
                            var serieL2 = chartTypeL1.Series.Add(worksheetGraph.Cells["B50:B" + (50 + ForAvg.Count - 1)], worksheetGraph.Cells["A50:A" + (50 + ForAvg.Count - 1)]);
                        }
                        if (GraphNo == 2)
                        {
                            for (int i = 0; i < ForAvg.Count; i++)
                            {
                                worksheetGraph.Cells["C" + (50 + i)].Value = "Avg";
                                worksheetGraph.Cells["D" + (50 + i)].Value = ForAvg.Sum() / ForAvg.Count;
                            }
                            var chartTypeL1 = chartLossses1Trend1.PlotArea.ChartTypes.Add(eChartType.Line);
                            var serieL2 = chartTypeL1.Series.Add(worksheetGraph.Cells["D50:D" + (50 + ForAvg.Count - 1)], worksheetGraph.Cells["C50:C" + (50 + ForAvg.Count - 1)]);

                        }
                        if (GraphNo == 3)
                        {
                            for (int i = 0; i < ForAvg.Count; i++)
                            {
                                worksheetGraph.Cells["E" + (50 + i)].Value = "Avg";
                                worksheetGraph.Cells["F" + (50 + i)].Value = ForAvg.Sum() / ForAvg.Count;
                            }
                            var chartTypeL1 = chartLossses1Trend1.PlotArea.ChartTypes.Add(eChartType.Line);
                            var serieL2 = chartTypeL1.Series.Add(worksheetGraph.Cells["F50:F" + (50 + ForAvg.Count - 1)], worksheetGraph.Cells["E50:E" + (50 + ForAvg.Count - 1)]);

                        }
                        if (GraphNo == 4)
                        {
                            for (int i = 0; i < ForAvg.Count; i++)
                            {
                                worksheetGraph.Cells["G" + (50 + i)].Value = "Avg";
                                worksheetGraph.Cells["H" + (50 + i)].Value = ForAvg.Sum() / ForAvg.Count;
                            }
                            var chartTypeL1 = chartLossses1Trend1.PlotArea.ChartTypes.Add(eChartType.Line);
                            var serieL2 = chartTypeL1.Series.Add(worksheetGraph.Cells["H50:H" + (50 + ForAvg.Count - 1)], worksheetGraph.Cells["G50:G" + (50 + ForAvg.Count - 1)]);

                        }
                        if (GraphNo == 5)
                        {
                            for (int i = 0; i < ForAvg.Count; i++)
                            {
                                worksheetGraph.Cells["I" + (50 + i)].Value = "Avg";
                                worksheetGraph.Cells["J" + (50 + i)].Value = ForAvg.Sum() / ForAvg.Count;
                            }
                            var chartTypeL1 = chartLossses1Trend1.PlotArea.ChartTypes.Add(eChartType.Line);
                            var serieL2 = chartTypeL1.Series.Add(worksheetGraph.Cells["J50:J" + (50 + ForAvg.Count - 1)], worksheetGraph.Cells["I50:I" + (50 + ForAvg.Count - 1)]);

                        }

                        var chartLossses1Trend = (ExcelLineChart)chartLossses1Trend1;
                        chartLossses1Trend.SetSize(300, 230);
                        chartLossses1Trend.SetPosition(PositionY, 10 + (((LossesLooper - 1)) * 300));
                        //chartLossses1Trend.Title.Text = "Top" + LossesLooper + " Trend Chart " + "(Hrs)";
                        chartLossses1Trend.Title.Text = LossName;
                        chartLossses1Trend.Style = eChartStyle.Style8;
                        chartLossses1Trend.Legend.Remove();
                        chartLossses1Trend.XAxis.MinValue = 0;
                        chartLossses1Trend.YAxis.MinValue = 0;
                        chartLossses1Trend.YAxis.MaxValue = (maxYVal + 10);
                        //chartLossses1Trend.YAxis.MaxValue = 100;
                        chartLossses1Trend.DataLabel.ShowValue = true;
                        //chartLossses1Trend.DataLabel.Font.Size = 6.0F;
                        chartLossses1Trend.PlotArea.Border.Width = 0;
                        chartLossses1Trend.YAxis.MinorTickMark = eAxisTickMark.None;
                        //chartLossses1Trend.YAxis.MajorTickMark = eAxisTickMark.None;
                        //chartLossses1Trend.XAxis.MinorTickMark = eAxisTickMark.None;
                        chartLossses1Trend.XAxis.MajorTickMark = eAxisTickMark.None;
                        //chartLossses1Trend.XAxis.MinorTickMark = eAxisTickMark.None;
                        chartLossses1Trend.Series.Add(erTopLossesTrendYValues, erTopLossesTrendXNames);

                        //Get reference of Graph to Remove GridLines
                        RemoveGridLines(ref chartLossses1Trend1);

                        #region OLD
                        //chartXml = chartLossses1Trend.ChartXml;
                        // nsuri = chartXml.DocumentElement.NamespaceURI;
                        // nsm = new XmlNamespaceManager(chartXml.NameTable);
                        //nsm.AddNamespace("c", nsuri);

                        ////XY Scatter plots have 2 value axis and no category
                        //valAxisNodes = chartXml.SelectNodes("c:chartSpace/c:chart/c:plotArea/c:valAx", nsm);
                        //if (valAxisNodes != null && valAxisNodes.Count > 0)
                        //    foreach (XmlNode valAxisNode in valAxisNodes)
                        //    {
                        //        var major = valAxisNode.SelectSingleNode("c:majorGridlines", nsm);
                        //        if (major != null)
                        //            valAxisNode.RemoveChild(major);

                        //        var minor = valAxisNode.SelectSingleNode("c:minorGridlines", nsm);
                        //        if (minor != null)
                        //            valAxisNode.RemoveChild(minor);
                        //    }

                        ////Other charts can have a category axis
                        //catAxisNodes = chartXml.SelectNodes("c:chartSpace/c:chart/c:plotArea/c:catAx", nsm);
                        //if (catAxisNodes != null && catAxisNodes.Count > 0)
                        //    foreach (XmlNode catAxisNode in catAxisNodes)
                        //    {
                        //        var major = catAxisNode.SelectSingleNode("c:majorGridlines", nsm);
                        //        if (major != null)
                        //            catAxisNode.RemoveChild(major);

                        //        var minor = catAxisNode.SelectSingleNode("c:minorGridlines", nsm);
                        //        if (minor != null)
                        //            catAxisNode.RemoveChild(minor);
                        //    }
                        #endregion

                        LossesLooper++;
                    }
                    else
                    {
                        break;
                    }
                }

                #endregion
            }
            #endregion //End of Graphs

            //To Set Colors
            //http://stackoverflow.com/questions/36520427/legend-color-is-incorrect-in-excel-chart-created-using-epplus/36532733#36532733

            //Hide Column R(CuttingTime)
            worksheet.Column(18).Width = 0;
            worksheetGraph.Row(Row).Height = 0;

            //Hide Identified and UnIdentified Losses Values
            //Color ColorHexWhite = System.Drawing.Color.White;
            //worksheetGraph.Cells["A1:B2"].Style.Font.Color.SetColor(ColorHexWhite);
            //worksheetGraph.Cells["A3:Z100"].Style.Font.Color.SetColor(ColorHexWhite);

            //autofit
            //Apply style to Losses header
            Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#32CD32");//#32CD32:lightgreen //B8C9E9
            worksheet.Cells["X" + 3 + ":" + finalLossCol + "" + 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["X" + 3 + ":" + finalLossCol + "" + 4].Style.Fill.BackgroundColor.SetColor(colFromHex);
            worksheet.Cells["X" + 3 + ":" + finalLossCol + "" + 4].Style.Border.Top.Style = ExcelBorderStyle.Medium;
            worksheet.Cells["X" + 3 + ":" + finalLossCol + "" + 4].Style.WrapText = true;

            //For Header
            worksheet.Cells["F2:L2"].Merge = true;
            worksheet.Cells["F2:I2"].Style.Font.Bold = true;
            worksheet.Cells["F2:I2"].Style.Font.Size = 16;
            worksheet.Cells["F2"].Value = Header.ToUpper() + " OEE Analysis";
            worksheetGraph.Cells["F2:L2"].Merge = true;
            worksheetGraph.Cells["F2"].Style.Font.Bold = true;
            worksheetGraph.Cells["F2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheetGraph.Cells["F2"].Style.Font.Size = 16;
            worksheetGraph.Cells["F2"].Value = Header.ToUpper() + " OEE Analysis";
            worksheetGraph.Cells["F2:M2"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
            //worksheetGraph.Cells["A4:R100"].Style.Font.Color.SetColor(Color.White);

            worksheet.View.ShowGridLines = false;
            worksheetGraph.View.ShowGridLines = false;
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            p.Save();

            //Downloding Excel
            string path1 = System.IO.Path.Combine(FileDir, "OEEReportGodHours" + frda.ToString("yyyy-MM-dd") + ".xlsx");
            if (TimeFactor == "GH")
            {
                path1 = System.IO.Path.Combine(FileDir, "OEEReportGodHours" + frda.ToString("yyyy-MM-dd") + ".xlsx"); //OEEReportAdjusted
            }
            else if (TimeFactor == "NoBlue")
            {
                path1 = System.IO.Path.Combine(FileDir, "OEEReportAdjusted" + frda.ToString("yyyy-MM-dd") + ".xlsx");
            }
            System.IO.FileInfo file1 = new System.IO.FileInfo(path1);
            string Outgoingfile = "OEEReportGodHours" + frda.ToString("yyyy-MM-dd") + ".xlsx";
            if (TimeFactor == "GH")
            {
                Outgoingfile = "OEEReportGodHours" + frda.ToString("yyyy-MM-dd") + ".xlsx"; //OEEReportAdjusted
            }
            else if (TimeFactor == "NoBlue")
            {
                Outgoingfile = "OEEReportAdjusted" + frda.ToString("yyyy-MM-dd") + ".xlsx";
            }
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
        }

        #endregion

        private static void GetWeek(DateTime now, CultureInfo cultureInfo, out DateTime begining, out DateTime end)
        {
            ReportsDao obj = new ReportsDao();
            Dao obj1 = new Dao();
            Dao1 obj2 = new Dao1();
            if (now == null)
                throw new ArgumentNullException("now");
            if (cultureInfo == null)
                throw new ArgumentNullException("cultureInfo");

            var firstDayOfWeek = cultureInfo.DateTimeFormat.FirstDayOfWeek;
            int offset = firstDayOfWeek - now.DayOfWeek;
            if (offset != 1)
            {
                DateTime weekStart = now.AddDays(offset);
                DateTime endOfWeek = weekStart.AddDays(6);
                begining = weekStart;
                end = endOfWeek;
            }
            else
            {
                begining = now.AddDays(-6);
                end = now;
            }
        }

        #region OLDCode of OEEREPORT
        //[HttpPost]
        //public ActionResult OEEReport(int PlantID, String FromDate, String ToDate, int ShopID = 0, int CellID = 0, int MachineID = 0)
        //{
        //    ReportsCalcClass.ProdDetAndon UR = new ReportsCalcClass.ProdDetAndon();
        //    ReportsCalcClass.OEEReportCalculations OEC = new ReportsCalcClass.OEEReportCalculations();
        //   double AvailabilityPercentage = 0;
        //    double PerformancePercentage = 0;
        //    double QualityPercentage = 0;
        //    double OEEPercentage = 0;
        //    OEC.GETCYCLETIMEAnalysis(MachineID, FromDate);
        //    OEC.OEE(MachineID, FromDate);

        //    var getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0).ToList();

        //    if (MachineID != 0)
        //    {
        //        getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.MachineID == MachineID).ToList();
        //    }
        //    else if (CellID != 0)
        //    {
        //        getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == CellID).ToList();
        //    }
        //    else if (ShopID != 0)
        //    {
        //        getMachineList = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID).ToList();
        //    }

        //    int dateDifference = Convert.ToDateTime(ToDate).Subtract(Convert.ToDateTime(FromDate)).Days;

        //    FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\OEE_Report.xlsx");

        //    ExcelPackage templatep = new ExcelPackage(templateFile);
        //    ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];
        //    ExcelWorksheet TemplateGraph = templatep.Workbook.Worksheets[2];

        //    String FileDir = @"C:\SRKS_ifacility\ReportsList\" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd");
        //    bool exists = System.IO.Directory.Exists(FileDir);
        //    if (!exists)
        //        System.IO.Directory.CreateDirectory(FileDir);

        //    FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "OEE_Report" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
        //    if (newFile.Exists)
        //    {
        //        try
        //        {
        //            newFile.Delete();  // ensures we create a new workbook
        //            newFile = new FileInfo(System.IO.Path.Combine(FileDir, "OEE_Report" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx"));
        //        }
        //        catch
        //        {
        //            TempData["Excelopen"] = "Excel with same date is already open, please close it and try to generate!!!!";
        //            //return View();
        //        }
        //    }
        //    //Using the File for generation and populating it
        //    ExcelPackage p = null;
        //    p = new ExcelPackage(newFile);
        //    ExcelWorksheet worksheet = null;
        //    ExcelWorksheet worksheetGraph = null;

        //    //Creating the WorkSheet for populating
        //    try
        //    {
        //        worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy"), Templatews);
        //        worksheetGraph = p.Workbook.Worksheets.Add("Graphs", TemplateGraph);
        //    }
        //    catch { }

        //    if (worksheet == null)
        //    {
        //        worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy") + "1", Templatews);
        //        worksheetGraph = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy") + "Graph", TemplateGraph);
        //    }
        //    else if (worksheetGraph == null)
        //    {
        //        worksheetGraph = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy") + "Graph", TemplateGraph);
        //    }
        //    int sheetcount = p.Workbook.Worksheets.Count;
        //    p.Workbook.Worksheets.MoveToStart(sheetcount);
        //    worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        //    worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
        //    int StartRow = 2;
        //    int SlNo = 1;
        //    int MachineCount = getMachineList.Count;
        //    int Startcolumn = 12;
        //    String ColNam = ExcelColumnFromNumber(Startcolumn);
        //    var GetMainLossList = Serverdb.tbllossescodes.Where(m => m.LossCodesLevel == 1 && m.IsDeleted == 0 && m.MessageType != "SETUP").OrderBy(m => m.LossCodeID).ToList();
        //    foreach (var LossRow in GetMainLossList)
        //    {
        //        ColNam = ExcelColumnFromNumber(Startcolumn);
        //        worksheet.Cells[ColNam + "1"].Value = LossRow.LossCode;
        //        Startcolumn++;
        //    }

        //    //Tabular sheet Data Population
        //    for (int i = 0; i <= dateDifference; i++)
        //    {
        //        DateTime QueryDate = Convert.ToDateTime(FromDate).AddDays(i);
        //        foreach (var Machine in getMachineList)
        //        {
        //            UR.insertManMacProd(Machine.MachineID, QueryDate.Date);
        //            var GetUtilList = Serverdb.tbl_ProdManMachine.Where(m => m.MachineID == Machine.MachineID && m.CorrectedDate == QueryDate.Date).ToList();
        //            foreach (var MacRow in GetUtilList)
        //            {
        //                int MacStartcolumn = 12;
        //                worksheet.Cells["A" + StartRow].Value = MacRow.tblmachinedetail.MachineName;
        //                worksheet.Cells["B" + StartRow].Value = MacRow.tblmachinedetail.MachineName;
        //                worksheet.Cells["C" + StartRow].Value = MacRow.tblworkorderentry.Prod_Order_No;
        //                worksheet.Cells["D" + StartRow].Value = MacRow.tblworkorderentry.FGCode;
        //                worksheet.Cells["E" + StartRow].Value = MacRow.tblworkorderentry.ProdOrderQty;
        //                worksheet.Cells["F" + StartRow].Value = MacRow.tblworkorderentry.OperationNo;
        //                worksheet.Cells["G" + StartRow].Value = QueryDate.Date.ToString("dd-MM-yyyy");
        //                worksheet.Cells["H" + StartRow].Value = MacRow.TotalOperatingTime;
        //                worksheet.Cells["I" + StartRow].Value = MacRow.tblworkorderentry.Yield_Qty;
        //                worksheet.Cells["J" + StartRow].Value = MacRow.tblworkorderentry.ScrapQty;
        //                worksheet.Cells["K" + StartRow].Value = MacRow.TotalSetup;
        //                int TotalQty = MacRow.tblworkorderentry.Yield_Qty + MacRow.tblworkorderentry.ScrapQty;
        //                if (TotalQty == 0)
        //                    TotalQty = 1;
        //                worksheet.Cells["K1"].Value = "Setup Time";
        //                worksheet.Cells["L1"].Value = "Rejections";
        //                worksheet.Cells["L" + StartRow].Value = (MacRow.TotalOperatingTime / TotalQty) * MacRow.tblworkorderentry.ScrapQty;
        //                long MacTotalLoss = 0;
        //                foreach (var LossRow in GetMainLossList)
        //                {
        //                    var getWoLossList1 = Serverdb.tbl_ProdOrderLosses.Where(m => m.WOID == MacRow.WOID && m.LossID == LossRow.LossCodeID).FirstOrDefault();
        //                    String ColEntry = ExcelColumnFromNumber(MacStartcolumn);
        //                    if (getWoLossList1 != null)
        //                    {
        //                        worksheet.Cells[ColEntry + "" + StartRow].Value = getWoLossList1.LossDuration;
        //                        MacTotalLoss += getWoLossList1.LossDuration;
        //                    }
        //                    else
        //                        worksheet.Cells[ColEntry + "" + StartRow].Value = 0;
        //                    MacStartcolumn++;
        //                }
        //                String ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                worksheet.Cells[ColEntry1 + "1"].Value = "No Power";
        //                worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.TotalPowerLoss;
        //                MacStartcolumn++;

        //                ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                worksheet.Cells[ColEntry1 + "1"].Value = "Total Part";
        //                worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.tblworkorderentry.Total_Qty;
        //                MacStartcolumn++;

        //                ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                worksheet.Cells[ColEntry1 + "1"].Value = "Load / Unload";
        //                worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.TotalMinorLoss - MacRow.TotalSetupMinorLoss;
        //                MacStartcolumn++;

        //                ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                worksheet.Cells[ColEntry1 + "1"].Value = "Shift";
        //                worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.tblworkorderentry.ShiftID;
        //                MacStartcolumn++;

        //                ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                worksheet.Cells[ColEntry1 + "1"].Value = "Operator ID";
        //                worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacRow.tblworkorderentry.OperatorID;
        //                MacStartcolumn++;

        //                ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                worksheet.Cells[ColEntry1 + "1"].Value = "Total OEE Loss";
        //                worksheet.Cells[ColEntry1 + "" + StartRow].Value = MacTotalLoss;
        //                MacStartcolumn++;

        //                decimal OEEPercent = (decimal)Math.Round((double)(MacRow.UtilPercent / 100) * (double)(MacRow.PerformancePerCent / 100) * (double)(MacRow.QualityPercent / 100) * 100, 2);

        //                ColEntry1 = ExcelColumnFromNumber(MacStartcolumn);
        //                worksheet.Cells[ColEntry1 + "1"].Value = "% of OEE";
        //                worksheet.Cells[ColEntry1 + "" + StartRow].Value = OEEPercent;
        //                MacStartcolumn++;
        //                StartRow++;
        //            }
        //        }
        //    }

        //    DataTable LossTbl = new DataTable();
        //    LossTbl.Columns.Add("LossID", typeof(int));
        //    LossTbl.Columns.Add("LossDuration", typeof(int));
        //    LossTbl.Columns.Add("LossTarget", typeof(string));
        //    LossTbl.Columns.Add("LossName", typeof(string));
        //    LossTbl.Columns.Add("LossActual", typeof(string));

        //    //Graph Sheet Population
        //    //Start Date and End Date
        //    worksheetGraph.Cells["C6"].Value = Convert.ToDateTime(FromDate).ToString("dd-MM-yyyy");
        //    worksheetGraph.Cells["E6"].Value = Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy");
        //    int GetHolidays = getsundays(Convert.ToDateTime(ToDate), Convert.ToDateTime(FromDate));
        //    int WorkingDays = dateDifference - GetHolidays + 1;
        //    //Working Days
        //    worksheetGraph.Cells["E5"].Value = WorkingDays;
        //    //Planned Production Time
        //    worksheetGraph.Cells["E10"].Value = WorkingDays * 24 * MachineCount;
        //    double TotalOperatingTime = 0;
        //    double TotalDownTime = 0;
        //    double TotalAcceptedQty = 0;
        //    double TotalRejectedQty = 0;
        //    double TotalPerformanceFactor = 0;
        //    int StartGrpah1 = 48;
        //    for (int i = 0; i <= dateDifference; i++)
        //    {
        //        double DayOperatingTime = 0;
        //        double DayDownTime = 0;
        //        double DayAcceptedQty = 0;
        //        double DayRejectedQty = 0;
        //        double DayPerformanceFactor = 0;
        //        DateTime QueryDate = Convert.ToDateTime(FromDate).AddDays(i);

        //        foreach (var MachRow in getMachineList)
        //        {
        //            if (MachineID == 0)
        //            {
        //                worksheetGraph.Cells["C4"].Value = MachRow.tblcell.CelldisplayName;
        //                worksheetGraph.Cells["C5"].Value = "AS DIVISION";
        //            }
        //            else
        //            {
        //                worksheetGraph.Cells["C4"].Value = MachRow.tblcell.CelldisplayName;
        //                worksheetGraph.Cells["C5"].Value = MachRow.MachineDisplayName;
        //            }
        //            var GetUtilList = Serverdb.tbl_ProdManMachine.Where(m => m.MachineID == MachRow.MachineID && m.CorrectedDate == QueryDate.Date).ToList();
        //            foreach (var ProdRow in GetUtilList)
        //            {
        //                //Total Values
        //                TotalOperatingTime += (double)ProdRow.TotalOperatingTime;
        //                TotalDownTime += (double)ProdRow.TotalLoss + (double)ProdRow.TotalMinorLoss + (double)ProdRow.TotalSetup;
        //                TotalAcceptedQty += ProdRow.tblworkorderentry.Yield_Qty;
        //                TotalRejectedQty += ProdRow.tblworkorderentry.ScrapQty;
        //                TotalPerformanceFactor += ProdRow.PerfromaceFactor;
        //                //Day Values
        //                DayOperatingTime += (double)ProdRow.TotalOperatingTime;
        //                DayDownTime += (double)ProdRow.TotalLoss + (double)ProdRow.TotalMinorLoss;
        //                DayAcceptedQty += ProdRow.tblworkorderentry.Yield_Qty;
        //                DayRejectedQty += ProdRow.tblworkorderentry.ScrapQty;
        //                DayPerformanceFactor += ProdRow.PerfromaceFactor;
        //            }
        //            var GetLossList = Serverdb.tbl_ProdOrderLosses.Where(m => m.MachineID == MachRow.MachineID && m.CorrectedDate == QueryDate.Date).ToList();

        //            foreach (var LossRow in GetLossList)
        //            {
        //                var getrow = (from DataRow row in LossTbl.Rows where row.Field<int>("LossID") == LossRow.LossID select row["LossID"]).FirstOrDefault();
        //                if (getrow == null)
        //                {
        //                    var GetLossTargetPercent = "1%";
        //                    String GetLossName = null;
        //                    var GetLossTarget = Serverdb.tbllossescodes.Where(m => m.LossCodeID == LossRow.LossID).FirstOrDefault();
        //                    if (GetLossTarget != null)
        //                    {
        //                        GetLossTargetPercent = GetLossTarget.TargetPercent.ToString();
        //                        GetLossName = GetLossTarget.LossCode;
        //                    }

        //                    LossTbl.Rows.Add(LossRow.LossID, LossRow.LossDuration, GetLossTargetPercent, GetLossName);
        //                }
        //                else
        //                {
        //                    foreach (DataRow GetRow in LossTbl.Rows)
        //                    {
        //                        if (Convert.ToInt32(GetRow["LossID"]) == LossRow.LossID)
        //                        {
        //                            long LossDura = Convert.ToInt32(GetRow["LossDuration"]);
        //                            LossDura += LossRow.LossDuration;
        //                            GetRow["LossDuration"] = LossDura;
        //                        }
        //                    }

        //                }
        //            }
        //        }
        //        int TotQty = (int)(DayAcceptedQty + DayRejectedQty);
        //        if (TotQty == 0)
        //            TotQty = 1;

        //        double DayOpTime = DayOperatingTime;
        //        if (DayOpTime == 0)
        //            DayOpTime = 1;

        //        decimal DayAvailPercent = (decimal)Math.Round(DayOperatingTime / (24 * MachineCount), 2);
        //        decimal DayPerformancePercent = (decimal)Math.Round(DayPerformanceFactor / DayOpTime, 2);
        //        decimal DayQualityPercent = (decimal)Math.Round((DayAcceptedQty / (TotQty)), 2);
        //        decimal DayOEEPercent = (decimal)Math.Round((double)(DayAvailPercent) * (double)(DayPerformancePercent) * (double)(DayQualityPercent), 2);

        //        worksheetGraph.Cells["B" + StartGrpah1].Value = QueryDate.ToString("dd-MM-yyyy");
        //        worksheetGraph.Cells["C" + StartGrpah1].Value = 0.85;
        //        worksheetGraph.Cells["D" + StartGrpah1].Value = DayOEEPercent;

        //        StartGrpah1++;
        //    }
        //    worksheetGraph.Cells["E11"].Value = (double)Math.Round(TotalOperatingTime / 60, 2);
        //    worksheetGraph.Cells["E12"].Value = (double)Math.Round(TotalDownTime / 60, 2);
        //    worksheetGraph.Cells["E13"].Value = TotalAcceptedQty;
        //    worksheetGraph.Cells["E14"].Value = TotalRejectedQty;

        //    decimal TotalAvailPercent = (decimal)Math.Round(TotalOperatingTime / (WorkingDays * 24 * 60 * MachineCount), 2);
        //    decimal TotalPerformancePercent = (decimal)Math.Round(TotalPerformanceFactor / TotalOperatingTime, 2);
        //    decimal TotalQualityPercent = (decimal)Math.Round((TotalAcceptedQty / (TotalAcceptedQty + TotalRejectedQty)), 2);
        //    decimal TotalOEEPercent = (decimal)Math.Round((double)(TotalAvailPercent) * (double)(TotalPerformancePercent) * (double)(TotalQualityPercent), 2);

        //    worksheetGraph.Cells["E20"].Value = TotalAvailPercent;
        //    worksheetGraph.Cells["E21"].Value = TotalPerformancePercent;
        //    worksheetGraph.Cells["E22"].Value = TotalQualityPercent;
        //    worksheetGraph.Cells["E23"].Value = TotalOEEPercent;
        //    worksheetGraph.Cells["G5"].Value = TotalOEEPercent;
        //    worksheetGraph.View.ShowGridLines = false;

        //    DateTime fromDate = Convert.ToDateTime(FromDate);
        //    DateTime toDate = Convert.ToDateTime(ToDate);
        //    var top3ContrubutingFactors = (from dbItem in Serverdb.tbl_ProdOrderLosses
        //                                   where dbItem.CorrectedDate >= fromDate.Date && dbItem.CorrectedDate <= toDate.Date
        //                                   group dbItem by dbItem.LossID into x
        //                                   select new
        //                                   {
        //                                       LossId = x.Key,
        //                                       LossDuration = Serverdb.tbl_ProdOrderLosses.Where(m => m.LossID == x.Key).Select(m => m.LossDuration).Sum()
        //                                   }).ToList();
        //    var item = top3ContrubutingFactors.OrderByDescending(m => m.LossDuration).Take(3).ToList();
        //    int lossXccelNo = 29;
        //    foreach (var GetRow in item)
        //    {
        //        string lossCode = Serverdb.tbllossescodes.Where(m => m.LossCodeID == GetRow.LossId).Select(m => m.LossCode).FirstOrDefault();
        //        decimal lossPercentage = (decimal)Math.Round(((GetRow.LossDuration) / TotalDownTime), 2);
        //        decimal lossDurationInHours = (decimal)Math.Round((GetRow.LossDuration / 60.00), 2);
        //        worksheetGraph.Cells["L" + lossXccelNo].Value = lossCode;
        //        worksheetGraph.Cells["N" + lossXccelNo].Value = lossPercentage;
        //        worksheetGraph.Cells["O" + lossXccelNo].Value = lossDurationInHours;
        //        lossXccelNo++;
        //    }

        //    int grphData = 5;
        //    decimal CumulativePercentage = 0;
        //    foreach (var data in top3ContrubutingFactors)
        //    {
        //        var dbLoss = Serverdb.tbllossescodes.Where(m => m.LossCodeID == data.LossId).FirstOrDefault();
        //        string lossCode = dbLoss.LossCode;
        //        decimal Target = dbLoss.TargetPercent;
        //        decimal actualPercentage = (decimal)Math.Round(((data.LossDuration) / TotalDownTime), 2);
        //        CumulativePercentage = CumulativePercentage + actualPercentage;
        //        worksheetGraph.Cells["K" + grphData].Value = lossCode;
        //        worksheetGraph.Cells["L" + grphData].Value = Target;
        //        worksheetGraph.Cells["M" + grphData].Value = actualPercentage;
        //        worksheetGraph.Cells["N" + grphData].Value = CumulativePercentage;
        //        grphData++;

        //    }

        //    //var chartIDAndUnID = (ExcelBarChart)worksheetGraph.Drawings.AddChart("Testing", eChartType.ColumnClustered);

        //    //chartIDAndUnID.SetSize((350), 550);

        //    //chartIDAndUnID.SetPosition(50, 60);

        //    //chartIDAndUnID.Title.Text = "AB Graph ";
        //    //chartIDAndUnID.Style = eChartStyle.Style18;
        //    //chartIDAndUnID.Legend.Position = eLegendPosition.Bottom;
        //    ////chartIDAndUnID.Legend.Remove();
        //    //chartIDAndUnID.YAxis.MaxValue = 100;
        //    //chartIDAndUnID.YAxis.MinValue = 0;
        //    //chartIDAndUnID.YAxis.MajorUnit = 5;

        //    //chartIDAndUnID.Locked = false;
        //    //chartIDAndUnID.PlotArea.Border.Width = 0;
        //    //chartIDAndUnID.YAxis.MinorTickMark = eAxisTickMark.None;
        //    //chartIDAndUnID.DataLabel.ShowValue = true;
        //    //chartIDAndUnID.DisplayBlanksAs = eDisplayBlanksAs.Gap;


        //    //ExcelRange dateWork = worksheetGraph.Cells["K33:" + lossXccelNo];
        //    //ExcelRange hoursWork = worksheetGraph.Cells["N33:" + lossXccelNo];
        //    //var hours = (ExcelChartSerie)(chartIDAndUnID.Series.Add(hoursWork, dateWork));
        //    //hours.Header = "Operating Time (Hours)";
        //    ////Get reference to the worksheet xml for proper namespace
        //    //var chartXml = chartIDAndUnID.ChartXml;
        //    //var nsuri = chartXml.DocumentElement.NamespaceURI;
        //    //var nsm = new XmlNamespaceManager(chartXml.NameTable);
        //    //nsm.AddNamespace("c", nsuri);

        //    ////XY Scatter plots have 2 value axis and no category
        //    //var valAxisNodes = chartXml.SelectNodes("c:chartSpace/c:chart/c:plotArea/c:valAx", nsm);
        //    //if (valAxisNodes != null && valAxisNodes.Count > 0)
        //    //    foreach (XmlNode valAxisNode in valAxisNodes)
        //    //    {
        //    //        var major = valAxisNode.SelectSingleNode("c:majorGridlines", nsm);
        //    //        if (major != null)
        //    //            valAxisNode.RemoveChild(major);

        //    //        var minor = valAxisNode.SelectSingleNode("c:minorGridlines", nsm);
        //    //        if (minor != null)
        //    //            valAxisNode.RemoveChild(minor);
        //    //    }

        //    ////Other charts can have a category axis
        //    //var catAxisNodes = chartXml.SelectNodes("c:chartSpace/c:chart/c:plotArea/c:catAx", nsm);
        //    //if (catAxisNodes != null && catAxisNodes.Count > 0)
        //    //    foreach (XmlNode catAxisNode in catAxisNodes)
        //    //    {
        //    //        var major = catAxisNode.SelectSingleNode("c:majorGridlines", nsm);
        //    //        if (major != null)
        //    //            catAxisNode.RemoveChild(major);

        //    //        var minor = catAxisNode.SelectSingleNode("c:minorGridlines", nsm);
        //    //        if (minor != null)
        //    //            catAxisNode.RemoveChild(minor);
        //    //    }
        //    //worksheetGraph.View["L29"]
        //    //worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        //    p.Save();

        //    //Downloding Excel
        //    string path1 = System.IO.Path.Combine(FileDir, "OEE_Report" + Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd") + ".xlsx");
        //    DownloadUtilReport(path1, "OEE_Report", ToDate);

        //    ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", PlantID);
        //    ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID), "ShopID", "ShopName", ShopID);
        //    ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID), "CellID", "CellName", CellID);
        //    ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID && m.ShopID == ShopID && m.CellID == CellID), "MachineID", "MachineDisplayName", MachineID);
        //    return View();
        //}
        #endregion

        public int getsundays(DateTime fdate, DateTime sdate)
        {
            //TimeSpan ts = fdate - sdate;
            //var sundays = ((ts.TotalDays / 7) + (sdate.DayOfWeek == DayOfWeek.Sunday || fdate.DayOfWeek == DayOfWeek.Sunday || fdate.DayOfWeek > sdate.DayOfWeek ? 1 : 0));

            //sundays = Math.Round(sundays - .5, MidpointRounding.AwayFromZero);

            //return (int)sundays;
            int sundayCount = 0;

            for (DateTime dt = sdate; dt < fdate; dt = dt.AddDays(1.0))
            {
                if (dt.DayOfWeek == DayOfWeek.Sunday)
                {
                    sundayCount++;
                }
            }

            return sundayCount;
        }

        public List<KeyValuePair<int, double>> GetAllLossesDurationSecondsForDateRange(int machineID, string CorrectedDateS, string CorrectedDateE)
        {
            List<KeyValuePair<int, double>> durationList = new List<KeyValuePair<int, double>>();
            DataTable lossesData = new DataTable();
            DataTable lossesIDData = new DataTable();
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                String query1 = "SELECT distinct( MessageCodeID ) From tbllossofentry WHERE MachineID = '" + machineID + "' and CorrectedDate >= '" + CorrectedDateS + "' and CorrectedDate <= '" + CorrectedDateE + "' and DoneWithRow = 1 ";
                SqlDataAdapter da1 = new SqlDataAdapter(query1, mc.msqlConnection);
                da1.Fill(lossesIDData);
                mc.close();
            }
            //var LossesIDs = db.tbllossofentries.Where(m => m.MachineID == machineID && m.CorrectedDate >= CorrectedDate && m.DoneWithRow == 1).Select(m => m.MessageCodeID).Distinct().ToList();
            for (int j = 0; j < lossesIDData.Rows.Count; j++)
            {
                lossesData.Clear();
                double duration = 0;
                int lossID = Convert.ToInt32(lossesIDData.Rows[j][0]);

                using (MsqlConnection mc = new MsqlConnection())
                {
                    mc.open();
                    String query1 = "SELECT StartDateTime,EndDateTime,LossID From tbllossofentry WHERE MachineID = '" + machineID + "' and CorrectedDate >= '" + CorrectedDateS + "' and CorrectedDate <= '" + CorrectedDateE + "' and MessageCodeID = '" + lossID + "' and DoneWithRow = 1 ";
                    SqlDataAdapter da1 = new SqlDataAdapter(query1, mc.msqlConnection);
                    da1.Fill(lossesData);
                    mc.close();
                }
                for (int i = 0; i < lossesData.Rows.Count; i++)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(lossesData.Rows[i][0])) && !string.IsNullOrEmpty(Convert.ToString(lossesData.Rows[i][1])))
                    {
                        DateTime StartDate = Convert.ToDateTime(lossesData.Rows[i][0]);
                        DateTime EndDate = Convert.ToDateTime(lossesData.Rows[i][1]);
                        duration += EndDate.Subtract(StartDate).TotalSeconds;
                    }
                }
                durationList.Add(new KeyValuePair<int, double>(lossID, duration));
            }
            return durationList;
        }

        public ActionResult CycleTime()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");

            return View();
        }

        [HttpPost]
        public ActionResult CycleTime(string PlantID, string TimeType, DateTime FromDate, DateTime ToDate, string PartsList, string ShopID = null, string CellID = null, string WorkCenterID = null)
        {
            #region old
            //if (report.Shift == "--Select Shift--")
            //{
            //    report.Shift = "No Use";
            //}
            //if (report.ShopNo == null)
            //{
            //    report.ShopNo = "No Use";
            //}
            //if (report.WorkCenter == null)
            //{
            //    report.WorkCenter = "No Use";
            //}
            #endregion
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            CycleTimeReportExcel(FromDate.ToString("yyyy-MM-dd"), ToDate.ToString("yyyy-MM-dd"), PartsList, PlantID.ToString(), Convert.ToString(ShopID), Convert.ToString(CellID), Convert.ToString(WorkCenterID));
            //UtilizationReportExcel(report.FromDate.ToString(), report.ToDate.ToString(), report.ShopNo.ToString(), report.WorkCenter.ToString(), TimeType);
            int p = Convert.ToInt32(PlantID);
            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");

            return View();
        }


        #region Kaizen
        public void CycleTimeReportExcel(string StartDate, string EndDate, string PartsList, string PlantID, string ShopID = null, string CellID = null, string WorkCenterID = null)
        {
            ReportsCalcClass.OEEReportCalculations OEC = new ReportsCalcClass.OEEReportCalculations();

            #region Excel and Stuff

            DateTime frda = DateTime.Now;
            if (string.IsNullOrEmpty(StartDate) == true)
            {
                StartDate = DateTime.Now.Date.ToString();
            }
            if (string.IsNullOrEmpty(EndDate) == true)
            {
                EndDate = StartDate;
            }

            DateTime frmDate = Convert.ToDateTime(StartDate);
            DateTime toDate = Convert.ToDateTime(EndDate);

            double TotalDay = toDate.Subtract(frmDate).TotalDays;

            FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\CycleTime_Report.xlsx");
            ExcelPackage templatep = new ExcelPackage(templateFile);
            ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];

            String FileDir = @"C:\SRKS_ifacility\ReportsList\" + System.DateTime.Now.ToString("yyyyMMddHHmmss");
            bool exists = System.IO.Directory.Exists(FileDir);
            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "CycleTime_Report" + frda.ToString("yyyyMMddHHmmss") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "CycleTime_Report" + frda.ToString("yyyyMMddHHmmss") + ".xlsx")); //" to " + toda.ToString("yyyy-MM-dd") + 
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
                //worksheetGraph = p.Workbook.Worksheets.Add("Graphs", TemplateGraph);
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), Templatews);
                // workSheetGraphData = p.Workbook.Worksheets.Add("GraphData", workSheetGraphData);
            }
            catch { }

            if (worksheet == null)
            {
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), Templatews);
                // worksheetGraph = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), TemplateGraph);
                // workSheetGraphData = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy") + "GraphData", workSheetGraphData);

            }

            int sheetcount = p.Workbook.Worksheets.Count;
            p.Workbook.Worksheets.MoveToStart(sheetcount);
            worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            #endregion

            #region MacCount & LowestLevel
            string lowestLevel = null;
            int MacCount = 0;
            int plantId = 0, shopId = 0, cellId = 0, wcId = 0;
            if (string.IsNullOrEmpty(WorkCenterID))
            {
                if (string.IsNullOrEmpty(CellID))
                {
                    if (string.IsNullOrEmpty(ShopID))
                    {
                        if (string.IsNullOrEmpty(PlantID))
                        {
                            //donothing
                        }
                        else
                        {
                            lowestLevel = "Plant";
                            plantId = Convert.ToInt32(PlantID);
                            MacCount = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == plantId).ToList().Count();
                        }
                    }
                    else
                    {
                        lowestLevel = "Shop";
                        shopId = Convert.ToInt32(ShopID);
                        MacCount = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == shopId).ToList().Count();
                    }
                }
                else
                {
                    lowestLevel = "Cell";
                    cellId = Convert.ToInt32(CellID);
                    MacCount = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == cellId).ToList().Count();
                }
            }
            else
            {
                lowestLevel = "WorkCentre";
                wcId = Convert.ToInt32(WorkCenterID);
                MacCount = 1;
            }

            #endregion

            #region Get Machines List
            DataTable machin = new DataTable();
            DateTime endDateTime = Convert.ToDateTime(toDate.AddDays(1).ToString("yyyy-MM-dd") + " " + new TimeSpan(6, 0, 0));
            string startDateTime = frmDate.ToString("yyyy-MM-dd");
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                String query1 = null;
                if (lowestLevel == "Plant")
                {
                    query1 = " SELECT  distinct MachineID FROM  "+dbName+".tblmachinedetails WHERE PlantID = " + PlantID + "  and IsNormalWC = 0  and ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or (CASE IsDeleted WHEN 1 THEN  CASE WHEN((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "') and  (DeletedDate >= '" + startDateTime + "'))  THEN 1 ELSE 0 END END = 1)); ";
                }
                else if (lowestLevel == "Shop")
                {
                    query1 = " SELECT * FROM  "+dbName+".tblmachinedetails WHERE ShopID = " + ShopID + "  and IsNormalWC = 0   and  ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or (CASE IsDeleted WHEN 1 THEN  CASE WHEN((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "') and  (DeletedDate >= '" + startDateTime + "'))  THEN 1 ELSE 0 END END = 1)); ";
                }
                else if (lowestLevel == "Cell")
                {
                    query1 = " SELECT * FROM  "+dbName+".tblmachinedetails WHERE CellID = " + CellID + "  and IsNormalWC = 0  and   ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or (CASE IsDeleted WHEN 1 THEN  CASE WHEN((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "') and  (DeletedDate >= '" + startDateTime + "'))  THEN 1 ELSE 0 END END = 1)); ";
                }
                else if (lowestLevel == "WorkCentre")
                {
                    query1 = "SELECT * FROM  "+dbName+".tblmachinedetails WHERE MachineID = " + WorkCenterID + "  and IsNormalWC = 0 and((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or (CASE IsDeleted WHEN 1 THEN  CASE WHEN((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "') and  (DeletedDate >= '" + startDateTime + "'))  THEN 1 ELSE 0 END END = 1)); ";
                }
                SqlDataAdapter da1 = new SqlDataAdapter(query1, mc.msqlConnection);
                da1.Fill(machin);
                mc.close();
            }
            #endregion
            List<int> MachineIdList = new List<int>();
            foreach (DataRow intItem in machin.Rows)
            {
                MachineIdList.Add(Convert.ToInt32(intItem["MachineID"].ToString()));
            }
            DateTime UsedDateForExcel = Convert.ToDateTime(frmDate);
            //For each Date ...... for all Machines.
            var Col = 'B';
            int Row = 5; // Gap to Insert OverAll data. DataStartRow + MachinesCount + 2(1 for HighestLevel & another for Gap).
            int Sno = 1;
            string finalLossCol = null;
            string existingPartNo = PartsList;

            //DataTable for Consolidated Data 


            string correctedDate = UsedDateForExcel.ToString("yyyy-MM-dd");
            PartSearchCreate obj = new PartSearchCreate();
            obj.StartTime = Convert.ToDateTime(Convert.ToDateTime(StartDate).ToString("yyyy-MM-dd 07:00:00"));
            obj.EndTime = Convert.ToDateTime(Convert.ToDateTime(EndDate).AddDays(1).ToString("yyyy-MM-dd 07:00:00"));
            obj.MachineId = MachineIdList;
            obj.FG_code = existingPartNo;
            obj.correctedDate = correctedDate;
            PushDataToTblPartLearingReport(obj);
            List<CycleTimeAnalysis> cycletimeList = new List<CycleTimeAnalysis>();
            int count = 0;
            // List<CycleTiemDataGraph> cycleTimeList = new List<CycleTiemDataGraph>();
            for (int i = 0; i < TotalDay + 1; i++)
            {
                int StartingRowForToday = Row;
                //string dateforMachine = UsedDateForExcel.ToString("yyyy-MM-dd");
                DateTime QueryDate = frmDate.AddDays(i);
                string QueryDatestring = QueryDate.ToString("yyyy-MM-dd");

                foreach (var macId in MachineIdList)
                {
                    OEC.GETCYCLETIMEAnalysis(macId, QueryDatestring);
                    var CTADet = Serverdb.tbl_CycleTimeAnalysis.Where(m => m.CorrectedDate == QueryDatestring && m.MachineID == macId && m.IsDeleted == 0).FirstOrDefault();
                    string Unit = "MINUTES";

                    if (CTADet != null)
                    {
                        count = count + 1;
                        CycleTimeAnalysis cta = new CycleTimeAnalysis();
                        cta.Machine_Name = CTADet.tblmachinedetail.MachineDisplayName;
                        cta.Sl_No = count;
                        cta.Std_CycleTime_In_Minutes = Convert.ToDecimal(CTADet.Std_CycleTime);
                        // cta.Std_CycleTime_Unit = CTADet.Std_CycleTimeUnit;
                        cta.Parts_Count = Convert.ToInt32(CTADet.PartsCount);
                        cta.Part_Number = CTADet.PartNum;
                        cta.Operating_Time_In_Minutes = Convert.ToDecimal(CTADet.OperatingTime);
                        //  cta.Operating_Time_Unit = CTADet.OperatingTimeUnit;
                        cta.Avg_Operating_Time_In_Minutes = Convert.ToDecimal(CTADet.AvgOperatingTime);
                        // cta.Avg_Operating_Time_Unit = CTADet.AvgOperatingTimeUnit;
                        cta.std_LoadTime_In_Minutes = Convert.ToDecimal(CTADet.Std_LoadTime);
                        // cta.Std_LoadTime_Unit = CTADet.Std_LoadTimeUnit;
                        cta.Total_LoadTime_In_Minutes = Convert.ToDecimal(CTADet.TotalLoadTime);
                        // cta.Total_LoadTime_Unit = CTADet.TotalLoadTimeUnit;
                        cta.Avg_LoadTime_In_Minutes = Convert.ToDecimal(CTADet.AvgLoadTimeinMinutes);
                        // cta.Avg_Operating_Time_Unit = cta.Avg_LoadTime_Unit;
                        cta.Date = QueryDatestring;
                        // cta.Avg_Operating_Time_Unit = CTADet.AvgOperatingTimeUnit;
                        //cta.Avg_LoadTime_Unit = Unit;
                        cycletimeList.Add(cta);
                    }


                }

                UsedDateForExcel = UsedDateForExcel.AddDays(+1);
            }


            DataTable dt = ToDataTable(cycletimeList);
            worksheet.Cells["A2"].LoadFromDataTable(dt, true);

            #region Save and Download

            //Hide Values
            //Color ColorHexWhite = System.Drawing.Color.White;
            //worksheetGraph.Cells["A1:Z50"].Style.Font.Color.SetColor(ColorHexWhite);

            //worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            //worksheetGraph.View.ShowGridLines = false;
            p.Save();

            //Downloding Excel
            string path1 = System.IO.Path.Combine(FileDir, "CycleTime_Report" + frda.ToString("yyyyMMddHHmmss") + ".xlsx");
            System.IO.FileInfo file1 = new System.IO.FileInfo(path1);
            string Outgoingfile = "CycleTime_Report" + frda.ToString("yyyyMMddHHmmss") + ".xlsx";
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
        #endregion
        // Part Learning  
        //public void CycleTimeReportExcel(string StartDate, string EndDate, string PartsList, string PlantID, string ShopID = null, string CellID = null, string WorkCenterID = null)
        //{
        //    #region Excel and Stuff

        //    DateTime frda = DateTime.Now;
        //    if (string.IsNullOrEmpty(StartDate) == true)
        //    {
        //        StartDate = DateTime.Now.Date.ToString();
        //    }
        //    if (string.IsNullOrEmpty(EndDate) == true)
        //    {
        //        EndDate = StartDate;
        //    }

        //    DateTime frmDate = Convert.ToDateTime(StartDate);
        //    DateTime toDate = Convert.ToDateTime(EndDate);

        //    double TotalDay = toDate.Subtract(frmDate).TotalDays;

        //    FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\PartLearning.xlsx");
        //    ExcelPackage templatep = new ExcelPackage(templateFile);
        //    ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];
        //    ExcelWorksheet TemplateGraph = templatep.Workbook.Worksheets[2];
        //    ExcelWorksheet workSheetGraphData = templatep.Workbook.Worksheets[3];

        //    String FileDir = @"C:\SRKS_ifacility\ReportsList\" + System.DateTime.Now.ToString("yyyy-MM-dd");
        //    bool exists = System.IO.Directory.Exists(FileDir);
        //    if (!exists)
        //        System.IO.Directory.CreateDirectory(FileDir);

        //    FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "CycleTime" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
        //    if (newFile.Exists)
        //    {
        //        try
        //        {
        //            newFile.Delete();  // ensures we create a new workbook
        //            newFile = new FileInfo(System.IO.Path.Combine(FileDir, "CycleTime" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //" to " + toda.ToString("yyyy-MM-dd") + 
        //        }
        //        catch
        //        {
        //            TempData["Excelopen"] = "Excel with same date is already open, please close it and try to generate!!!!";
        //            //return View();
        //        }
        //    }
        //    //Using the File for generation and populating it
        //    ExcelPackage p = null;
        //    p = new ExcelPackage(newFile);
        //    ExcelWorksheet worksheet = null;
        //    ExcelWorksheet worksheetGraph = null;

        //    //Creating the WorkSheet for populating
        //    try
        //    {
        //        worksheetGraph = p.Workbook.Worksheets.Add("Graphs", TemplateGraph);
        //        worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), Templatews);
        //        workSheetGraphData = p.Workbook.Worksheets.Add("GraphData", workSheetGraphData);
        //    }
        //    catch { }

        //    if (worksheet == null)
        //    {
        //        worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), Templatews);
        //        worksheetGraph = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), TemplateGraph);
        //        workSheetGraphData = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy") + "GraphData", workSheetGraphData);

        //    }

        //    int sheetcount = p.Workbook.Worksheets.Count;
        //    p.Workbook.Worksheets.MoveToStart(sheetcount);
        //    worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        //    worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

        //    #endregion

        //    #region MacCount & LowestLevel
        //    string lowestLevel = null;
        //    int MacCount = 0;
        //    int plantId = 0, shopId = 0, cellId = 0, wcId = 0;
        //    if (string.IsNullOrEmpty(WorkCenterID))
        //    {
        //        if (string.IsNullOrEmpty(CellID))
        //        {
        //            if (string.IsNullOrEmpty(ShopID))
        //            {
        //                if (string.IsNullOrEmpty(PlantID))
        //                {
        //                    //donothing
        //                }
        //                else
        //                {
        //                    lowestLevel = "Plant";
        //                    plantId = Convert.ToInt32(PlantID);
        //                    MacCount = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == plantId).ToList().Count();
        //                }
        //            }
        //            else
        //            {
        //                lowestLevel = "Shop";
        //                shopId = Convert.ToInt32(ShopID);
        //                MacCount = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == shopId).ToList().Count();
        //            }
        //        }
        //        else
        //        {
        //            lowestLevel = "Cell";
        //            cellId = Convert.ToInt32(CellID);
        //            MacCount = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == cellId).ToList().Count();
        //        }
        //    }
        //    else
        //    {
        //        lowestLevel = "WorkCentre";
        //        wcId = Convert.ToInt32(WorkCenterID);
        //        MacCount = 1;
        //    }

        //    #endregion

        //    #region Get Machines List
        //    DataTable machin = new DataTable();
        //    DateTime endDateTime = Convert.ToDateTime(toDate.AddDays(1).ToString("yyyy-MM-dd") + " " + new TimeSpan(6, 0, 0));
        //    string startDateTime = frmDate.ToString("yyyy-MM-dd");
        //    using (MsqlConnection mc = new MsqlConnection())
        //    {
        //        mc.open();
        //        String query1 = null;
        //        if (lowestLevel == "Plant")
        //        {
        //            query1 = " SELECT  distinct MachineID FROM  " + dbName + ".tblmachinedetails WHERE PlantID = " + PlantID + "  and IsNormalWC = 0  and ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or (CASE IsDeleted WHEN 1 THEN  CASE WHEN((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "') and  (DeletedDate >= '" + startDateTime + "'))  THEN 1 ELSE 0 END END = 1)); ";
        //        }
        //        else if (lowestLevel == "Shop")
        //        {
        //            query1 = " SELECT * FROM  " + dbName + ".tblmachinedetails WHERE ShopID = " + ShopID + "  and IsNormalWC = 0   and  ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or (CASE IsDeleted WHEN 1 THEN  CASE WHEN((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "') and  (DeletedDate >= '" + startDateTime + "'))  THEN 1 ELSE 0 END END = 1)); ";
        //        }
        //        else if (lowestLevel == "Cell")
        //        {
        //            query1 = " SELECT * FROM  " + dbName + ".tblmachinedetails WHERE CellID = " + CellID + "  and IsNormalWC = 0  and   ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or (CASE IsDeleted WHEN 1 THEN  CASE WHEN((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "') and  (DeletedDate >= '" + startDateTime + "'))  THEN 1 ELSE 0 END END = 1)); ";
        //        }
        //        else if (lowestLevel == "WorkCentre")
        //        {
        //            query1 = "SELECT * FROM  " + dbName + ".tblmachinedetails WHERE MachineID = " + WorkCenterID + "  and IsNormalWC = 0 and((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or (CASE IsDeleted WHEN 1 THEN  CASE WHEN((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "') and  (DeletedDate >= '" + startDateTime + "'))  THEN 1 ELSE 0 END END = 1)); ";
        //        }
        //        SqlDataAdapter da1 = new SqlDataAdapter(query1, mc.msqlConnection);
        //        da1.Fill(machin);
        //        mc.close();
        //    }
        //    #endregion
        //    List<int> MachineIdList = new List<int>();
        //    foreach (DataRow intItem in machin.Rows)
        //    {
        //        MachineIdList.Add(Convert.ToInt32(intItem["MachineID"].ToString()));
        //    }
        //    DateTime UsedDateForExcel = Convert.ToDateTime(frmDate);
        //    //For each Date ...... for all Machines.
        //    var Col = 'B';
        //    int Row = 5; // Gap to Insert OverAll data. DataStartRow + MachinesCount + 2(1 for HighestLevel & another for Gap).
        //    int Sno = 1;
        //    string finalLossCol = null;
        //    string existingPartNo = PartsList;

        //    //DataTable for Consolidated Data 


        //    string correctedDate = UsedDateForExcel.ToString("yyyy-MM-dd");
        //    PartSearchCreate obj = new PartSearchCreate();
        //    obj.StartTime = Convert.ToDateTime(Convert.ToDateTime(StartDate).ToString("yyyy-MM-dd 07:00:00"));
        //    obj.EndTime = Convert.ToDateTime(Convert.ToDateTime(EndDate).AddDays(1).ToString("yyyy-MM-dd 07:00:00"));
        //    obj.MachineId = MachineIdList;
        //    obj.FG_code = existingPartNo;
        //    obj.correctedDate = correctedDate;
        //    PushDataToTblPartLearingReport(obj);

        //    List<CycleTiemDataGraph> cycleTimeList = new List<CycleTiemDataGraph>();
        //    for (int i = 0; i < TotalDay + 1; i++)
        //    {
        //        int StartingRowForToday = Row;
        //        //string dateforMachine = UsedDateForExcel.ToString("yyyy-MM-dd");
        //        DateTime QueryDate = frmDate.AddDays(i);
        //        foreach (var macId in MachineIdList)
        //        {
        //            //1) Get distinct partno,WoNo,Opno which are JF
        //            //2) Get sum of green, settingTime, etc and push into excel
        //            DataTable PartData = new DataTable();
        //            using (MsqlConnection mc = new MsqlConnection())
        //            {
        //                mc.open();
        //                //String query = "  select * from "+dbName+".tblpartlearningreport where HMIID in  (SELECT HMIID FROM"+dbName+".tblpartlearningreport where FGCode = '" + existingPartNo + "' and CorrectedDate = '" + QueryDate.ToString("yyyy-MM-dd") + "'); ";

        //                String query = "  select * from " + dbName + ".[tblpartlearningreport] where HMIID in  (SELECT HMIID FROM " + dbName + ".[tblworkorderentry] where CorrectedDate = '" + QueryDate.ToString("yyyy-MM-dd") + "' and MachineID = " + macId + " ); ";

        //                if (obj.FG_code != null && obj.FG_code != "")
        //                {
        //                    query = "  select * from " + dbName + ".[tblpartlearningreport] where HMIID in  (SELECT HMIID FROM " + dbName + ".[tblworkorderentry] where FGCode = '" + existingPartNo + "' and CorrectedDate = '" + QueryDate.ToString("yyyy-MM-dd") + "' and MachineID = " + macId + " ); ";
        //                }

        //                SqlDataAdapter da = new SqlDataAdapter(query, mc.msqlConnection);
        //                da.Fill(PartData);
        //                mc.close();
        //            }
        //            for (int j = 0; j < PartData.Rows.Count; j++)
        //            {
        //                int MachineID = Convert.ToInt32(PartData.Rows[j][1]); //MachineID
        //                List<string> HierarchyData = GetHierarchyData(MachineID);

        //                worksheet.Cells["B" + Row].Value = Sno++;
        //                //worksheet.Cells["C" + Row].Value = HierarchyData[0];//Plant
        //                //worksheet.Cells["D" + Row].Value = HierarchyData[1];//Shop
        //                //worksheet.Cells["E" + Row].Value = HierarchyData[2];//Cell
        //                worksheet.Cells["C" + Row].Value = HierarchyData[3];//Mac Display Name
        //                string WorkOrderNo = Convert.ToString(PartData.Rows[j][4]);//WorkOrderNo
        //                worksheet.Cells["D" + Row].Value = Convert.ToDateTime(Convert.ToString(PartData.Rows[j][3])).ToString("dd-MM-yyyy");//completed Date
        //                worksheet.Cells["E" + Row].Value = WorkOrderNo;
        //                worksheet.Cells["F" + Row].Value = PartData.Rows[j][5];//FG Code
        //                string OpNo = Convert.ToString(PartData.Rows[j][6]);//OpNo
        //                worksheet.Cells["G" + Row].Value = OpNo;
        //                string TargetQty = Convert.ToString(PartData.Rows[j][7]);//TargetQty
        //                int TargetQtyCalc = Convert.ToInt32(PartData.Rows[j][9]) + Convert.ToInt32(PartData.Rows[j][10]);//Yield Qty
        //                if (TargetQtyCalc == 0)
        //                {
        //                    TargetQtyCalc = 1;
        //                }
        //                worksheet.Cells["H" + Row].Value = TargetQty;
        //                worksheet.Cells["I" + Row].Value = Convert.ToString(PartData.Rows[j][9]);//Yield Qty
        //                worksheet.Cells["J" + Row].Value = Convert.ToInt32(PartData.Rows[j][10]); //Scrap Qty
        //                double StdCycTime = Convert.ToDouble(PartData.Rows[j][18]);
        //                double StdMinorLoss = Convert.ToDouble(PartData.Rows[j][21]);
        //                worksheet.Cells["K" + Row].Value = StdCycTime;//Std Cycle Time
        //                worksheet.Cells["L" + Row].Value = StdMinorLoss; //Std Minor Loss
        //                worksheet.Cells["M" + Row].Value = StdCycTime + StdMinorLoss; //Total Std Time

        //                //worksheet.Cells["N" + Row].Value = Convert.ToInt32(PartData.Rows[j][22]); //Total Std Minor Loss
        //                //worksheet.Cells["N" + Row].Value = Convert.ToInt32(PartData.Rows[j][11]); //Setting Time
        //                //worksheet.Cells["O" + Row].Value = Convert.ToInt32(PartData.Rows[j][12]);//Idle

        //                //worksheet.Cells["Q" + Row].Value = Convert.ToInt32(PartData.Rows[j][14]); //Blue
        //                int HMIID = Convert.ToInt32(PartData.Rows[j][2]);//Hmmid
        //                DataTable dt1 = new DataTable();
        //                using (MsqlConnection mc = new MsqlConnection())
        //                {
        //                    mc.open();
        //                    String qry = "SELECT WOStart,WOEnd FROM " + dbName + ".[tblworkorderentry] where HMIID = '" + HMIID + "'";
        //                    SqlDataAdapter da = new SqlDataAdapter(qry, mc.msqlConnection);
        //                    da.Fill(dt1);
        //                    mc.close();
        //                }
        //                int tbCount = dt1.Rows.Count;
        //                int ActualCuttingTime = 0;
        //                if (tbCount > 0)
        //                {
        //                    string startDate = (dt1.Rows[0][0]).ToString();
        //                    string endDate = (dt1.Rows[0][1]).ToString();

        //                    DataTable dt2 = new DataTable();
        //                    using (MsqlConnection mc = new MsqlConnection())
        //                    {
        //                        mc.open();
        //                        String qry = "SELECT SUM(DATEDiff(MINUTE,StartTime,EndTime)) as diff FROM" + dbName + ".[tblmode] where MachineID = " + MachineID + "  and StartTime>= '" + startDate + "' and EndTime<= '" + endDate + "' and MacMode = 'PROD'";
        //                        SqlDataAdapter da = new SqlDataAdapter(qry, mc.msqlConnection);
        //                        da.Fill(dt2);
        //                        mc.close();
        //                    }
        //                    try
        //                    {
        //                        ActualCuttingTime = Convert.ToInt32(dt2.Rows[0][0]);
        //                    }
        //                    catch
        //                    {
        //                        ActualCuttingTime = 0;
        //                    }
        //                }
        //                worksheet.Cells["N" + Row].Value = ActualCuttingTime;
        //                worksheet.Cells["O" + Row].Value = Convert.ToInt32(PartData.Rows[j][13]);//Minor Loss
        //                worksheet.Cells["P" + Row].Value = ActualCuttingTime + Convert.ToInt32(PartData.Rows[j][13]);//Actual Total Operating Time
        //                worksheet.Cells["Q" + Row].Value = Convert.ToInt32(PartData.Rows[j][17]);//Average Cuttng Time
        //                worksheet.Cells["R" + Row].Value = Convert.ToInt32(PartData.Rows[j][13]) / TargetQtyCalc;//Average Minor Loss
        //                worksheet.Cells["S" + Row].Value = Convert.ToInt32(PartData.Rows[j][17]) + (Convert.ToInt32(PartData.Rows[j][13]) / TargetQtyCalc);//Average Total Operating Time
        //                worksheet.Cells["T" + Row].Value = StdCycTime + StdMinorLoss - (Convert.ToInt32(PartData.Rows[j][17]) + (Convert.ToInt32(PartData.Rows[j][13]) / TargetQtyCalc));//Cycle Time Delta
        //                int CyCtimeDelta = (int)(StdCycTime + StdMinorLoss - (Convert.ToInt32(PartData.Rows[j][17]) + (Convert.ToInt32(PartData.Rows[j][13]) / TargetQtyCalc)));
        //                setcellcolor(worksheet, CyCtimeDelta, "T" + Row.ToString());
        //                worksheet.Cells["U" + Row].Value = Math.Round(((Convert.ToInt32(PartData.Rows[j][17]) + (Convert.ToInt32(PartData.Rows[j][13]) / TargetQtyCalc)) / ((StdCycTime + StdMinorLoss))) * 100, 0) - 100;//Cycle Time Delta %
        //                double CycDel = Math.Round(((Convert.ToInt32(PartData.Rows[j][17]) + (Convert.ToInt32(PartData.Rows[j][13]) / TargetQtyCalc)) / ((StdCycTime + StdMinorLoss))) * 100, 0) - 100;
        //                //settextcolor(worksheet, CycDel, "U" + Row.ToString());

        //                string modelRange = "B" + Row + ":U" + Row + "";
        //                var modelTable = worksheet.Cells[modelRange];
        //                modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        //                modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
        //                modelTable.Style.Border.Right.Style = ExcelBorderStyle.Thin;
        //                modelTable.Style.Border.Left.Style = ExcelBorderStyle.Thin;
        //                CycleTiemDataGraph itemCycleTime = new CycleTiemDataGraph();
        //                string fgcodOpno = PartData.Rows[j][5] + "-" + OpNo;
        //                itemCycleTime.fgcodOpno = fgcodOpno;
        //                itemCycleTime.YieldQty = Convert.ToInt32(PartData.Rows[j][9]);
        //                itemCycleTime.ScrapQty = Convert.ToInt32(PartData.Rows[j][10]);
        //                itemCycleTime.TotalStdTime = StdCycTime + StdMinorLoss;
        //                itemCycleTime.ActualTotalOperatingTime = ActualCuttingTime + Convert.ToInt32(PartData.Rows[j][13]);
        //                cycleTimeList.Add(itemCycleTime);
        //                Row++;
        //            }
        //        }

        //        UsedDateForExcel = UsedDateForExcel.AddDays(+1);
        //    }


        //    #region//graph data
        //    int RowGraph = 5;

        //    int intalColumn = 2;
        //    var iListItem = cycleTimeList.OrderBy(m => m.fgcodOpno);
        //    var uniqFGCodeOpNo = cycleTimeList.Select(m => m.fgcodOpno).Distinct();

        //    foreach (var fgItem in uniqFGCodeOpNo)
        //    {
        //        int ActRow = 1;
        //        decimal diff = 0;
        //        string fgCodeOverAll = "";
        //        int totalYelidAndScrapQty = 0, TotalActualTotalOperatingTime = 0;
        //        double TotalTotalStdTime = 0;
        //        foreach (var item in iListItem)
        //        {
        //            if (fgItem == item.fgcodOpno)
        //            {
        //                fgCodeOverAll = item.fgcodOpno;
        //                totalYelidAndScrapQty = totalYelidAndScrapQty + item.YieldQty + item.ScrapQty;
        //                TotalTotalStdTime = item.TotalStdTime;
        //                TotalActualTotalOperatingTime = TotalActualTotalOperatingTime + item.ActualTotalOperatingTime;

        //            }

        //        }
        //        if (totalYelidAndScrapQty != 0)
        //            diff = (Convert.ToDecimal(TotalActualTotalOperatingTime) / Convert.ToDecimal(totalYelidAndScrapQty));
        //        // string dcdff = diff.ToString("0.##");
        //        int dfrnc = Convert.ToInt32(Math.Round(diff));
        //        workSheetGraphData.Cells["A" + RowGraph].Value = fgCodeOverAll;//FG Code
        //        workSheetGraphData.Cells["B" + RowGraph].Value = totalYelidAndScrapQty;//Yield Qty+Scrap Qty
        //        workSheetGraphData.Cells["C" + RowGraph].Value = TotalTotalStdTime; //Total Std Time
        //        workSheetGraphData.Cells["D" + RowGraph].Value = TotalActualTotalOperatingTime;//Actual Total Operating Time
        //        workSheetGraphData.Cells["E" + RowGraph].Value = Convert.ToDecimal(dfrnc);//Actual Total Op Time = Cum of Actual Total Operating Time/Cum(yeild qty+scrap qty)


        //        var coluName = ExcelColumnFromNumber(intalColumn);
        //        workSheetGraphData.Cells[coluName + ActRow].Value = fgCodeOverAll;//FG Code
        //        ActRow++;
        //        workSheetGraphData.Cells[coluName + ActRow].Value = TotalTotalStdTime; //Total Std Time
        //        ActRow++;
        //        workSheetGraphData.Cells[coluName + ActRow].Value = Convert.ToDecimal(dfrnc);//Actual Total Op Time = Cum of Actual Total Operating Time/Cum(yeild qty+scrap qty)

        //        RowGraph++;
        //        intalColumn++;
        //    }

        //    for (int i = intalColumn; i <= 104; i++)
        //    {
        //        workSheetGraphData.Column(i).Hidden = true;
        //    }

        //    workSheetGraphData.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;

        //    #endregion

        //    #region Save and Download

        //    //Hide Values
        //    //Color ColorHexWhite = System.Drawing.Color.White;
        //    //worksheetGraph.Cells["A1:Z50"].Style.Font.Color.SetColor(ColorHexWhite);

        //    //worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        //    //worksheetGraph.View.ShowGridLines = false;
        //    p.Save();

        //    //Downloding Excel
        //    string path1 = System.IO.Path.Combine(FileDir, "CycleTime" + frda.ToString("yyyy-MM-dd") + ".xlsx");
        //    System.IO.FileInfo file1 = new System.IO.FileInfo(path1);
        //    string Outgoingfile = "CycleTime" + frda.ToString("yyyy-MM-dd") + ".xlsx";
        //    if (file1.Exists)
        //    {
        //        Response.Clear();
        //        Response.ClearContent();
        //        Response.ClearHeaders();
        //        Response.AddHeader("Content-Disposition", "attachment; filename=" + Outgoingfile);
        //        Response.AddHeader("Content-Length", file1.Length.ToString());
        //        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //        Response.WriteFile(file1.FullName);
        //        Response.Flush();
        //        Response.Close();
        //    }
        //    #endregion
        //}


        [HttpGet]
        public ActionResult PMSReport()
        {
            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");
            return View();
        }

        [HttpPost]
        public ActionResult PMSReport(int plantid, int shopid, int cellid, int machineid, int Year, int radiobtn = 0)
        {
            string startMonth = null;
            string endMonth = null;
            int nextyear = Year + 1;
            if (radiobtn == 1)
            {
                startMonth = ("01-01-" + Year);
                endMonth = ("01-12-" + nextyear);
            }

            DateTime ToDate = DateTime.Now;
            FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\PMS_Report.xlsx");
            ExcelPackage templatep = new ExcelPackage(templateFile);
            ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];

            String FileDir = @"C:\SRKS_ifacility\ReportsList\" + ToDate.ToString("yyyy-MM-dd");
            bool exists = System.IO.Directory.Exists(FileDir);
            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "PMS_Report_" + ToDate.ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "PMS_Report_" + ToDate.ToString("yyyy-MM-dd") + ".xlsx"));
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
                worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy"), Templatews);
            }
            catch { }

            if (worksheet == null)
            {
                worksheet = p.Workbook.Worksheets.Add(Convert.ToDateTime(ToDate).ToString("dd-MM-yyyy") + "1", Templatews);
            }
            int sheetcount = p.Workbook.Worksheets.Count;
            p.Workbook.Worksheets.MoveToStart(sheetcount);
            worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            int StartRow = 5;
            int SlNo = 1;
            int i = 0;
            DataTable dt = new DataTable();
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                var pmsdet = "Select pmsid from " + dbName + ".[tblpmsdetails] where MachineID =" + machineid + " and PMStartDate >= '" + startMonth + "' and PMEndDate >= '" + endMonth + "';";
                SqlDataAdapter sda = new SqlDataAdapter(pmsdet, mc.msqlConnection);
                sda.Fill(dt);
                mc.close();
            }
            int count1 = dt.Rows.Count;
            if (count1 == 1)
            {
                for (i = 0; i < count1; i++)
                {
                    int pmsid = Convert.ToInt32(dt.Rows[i][0]);
                    var pmsdata = Serverdb.tblhistpms.Where(m => m.pmsid == pmsid).Distinct().ToList();
                    foreach (var row in pmsdata)
                    {
                        string cellname = Serverdb.tblcells.Where(m => m.CellID == cellid).Select(m => m.CellName).FirstOrDefault();
                        string machinename = Serverdb.tblmachinedetails.Where(m => m.MachineID == machineid).Select(m => m.MachineDisplayName).FirstOrDefault();
                        worksheet.Cells["C2"].Value = cellname;
                        worksheet.Cells["E2"].Value = machinename;
                        var pmcpid = Serverdb.tblhistpms.Where(m => m.pmsid == row.pmsid).Select(m => m.Pmcheckpointid).Distinct().ToList();
                        foreach (var item in pmcpid)
                        {
                            SlNo = 1;
                            var checkpointdata = Serverdb.configuration_tblpmcheckpoint.Where(m => m.pmcpID == item && m.CellID == cellid && m.Isdeleted == 0).FirstOrDefault();
                            worksheet.Cells["A" + StartRow].Value = "";
                            worksheet.Cells["B" + StartRow].Value = checkpointdata.TypeofCheckpoint;
                            StartRow++;
                            var checklistdata = Serverdb.configuration_tblpmchecklist.Where(m => m.pmcpID == checkpointdata.pmcpID).ToList();
                            int count = checklistdata.Count;
                            foreach (var row1 in checklistdata)
                            {
                                worksheet.Cells["A" + StartRow].Value = SlNo++;
                                worksheet.Cells["B" + StartRow].Value = row1.CheckList;
                                worksheet.Cells["C" + StartRow].Value = row1.Frequency;
                                worksheet.Cells["D" + StartRow].Value = row1.How;
                                worksheet.Cells["E" + StartRow].Value = row1.Value;
                                //worksheet.Cells["G3"].Value = row.CorrectedDate;


                                var work = Serverdb.tblhistpms.Where(m => m.Pmcheckpointid == checkpointdata.pmcpID && m.Pmchecklistname == row1.CheckList && m.CorrectedDate == row.CorrectedDate).FirstOrDefault();
                                if (work != null)
                                    if (work.workdone == 1)
                                    {
                                        worksheet.Cells["F" + StartRow].Value = row.CorrectedDate;
                                        worksheet.Cells["G" + StartRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                        worksheet.Cells["G" + StartRow].Style.Fill.BackgroundColor.SetColor(Color.Green);
                                        worksheet.Cells["G" + StartRow].Style.Font.Color.SetColor(Color.White);
                                        worksheet.Cells["G" + StartRow].Value = "YES";
                                    }
                                    else
                                    {
                                        worksheet.Cells["F" + StartRow].Value = row.CorrectedDate;
                                        worksheet.Cells["G" + StartRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                        worksheet.Cells["G" + StartRow].Style.Fill.BackgroundColor.SetColor(Color.Red);
                                        worksheet.Cells["G" + StartRow].Style.Font.Color.SetColor(Color.White);
                                        worksheet.Cells["G" + StartRow].Value = "NO";
                                    }
                                StartRow++;
                            }
                        }
                        StartRow++;
                        break;
                    }
                    StartRow++;
                }
            }
            else if (count1 == 2)
            {
                int cou = 0;
                for (i = 0; i < count1; i++)
                {
                    int pmsid = Convert.ToInt32(dt.Rows[i][0]);
                    var pmsdata = Serverdb.tblhistpms.Where(m => m.pmsid == pmsid).ToList();

                    foreach (var row in pmsdata)
                    {
                        if (cou != 1)
                        {
                            string cellname = Serverdb.tblcells.Where(m => m.CellID == cellid).Select(m => m.CellName).FirstOrDefault();
                            string machinename = Serverdb.tblmachinedetails.Where(m => m.MachineID == machineid).Select(m => m.MachineDisplayName).FirstOrDefault();
                            worksheet.Cells["C2"].Value = cellname;
                            worksheet.Cells["E2"].Value = machinename;
                            var pmcpid1 = Serverdb.tblhistpms.Where(m => m.pmsid == row.pmsid).Select(m => m.Pmcheckpointid).ToList();
                            foreach (var item in pmcpid1)
                            {
                                SlNo = 1;
                                var checkpointdata = Serverdb.configuration_tblpmcheckpoint.Where(m => m.pmcpID == item && m.CellID == cellid && m.Isdeleted == 0).FirstOrDefault();
                                worksheet.Cells["A" + StartRow].Value = "";
                                worksheet.Cells["B" + StartRow].Value = checkpointdata.TypeofCheckpoint;
                                StartRow++;
                                var checklistdata = Serverdb.configuration_tblpmchecklist.Where(m => m.pmcpID == checkpointdata.pmcpID).ToList();
                                foreach (var row1 in checklistdata)
                                {
                                    worksheet.Cells["A" + StartRow].Value = SlNo++;
                                    worksheet.Cells["B" + StartRow].Value = row1.CheckList;
                                    worksheet.Cells["C" + StartRow].Value = row1.Frequency;
                                    worksheet.Cells["D" + StartRow].Value = row1.How;
                                    worksheet.Cells["E" + StartRow].Value = row1.Value;
                                    //worksheet.Cells["G3"].Value = row.CorrectedDate;

                                    var work = Serverdb.tblhistpms.Where(m => m.Pmcheckpointid == checkpointdata.pmcpID && m.Pmchecklistname == row1.CheckList && m.CorrectedDate == row.CorrectedDate).FirstOrDefault();
                                    if (work.workdone == 1)
                                    {
                                        worksheet.Cells["F" + StartRow].Value = row.CorrectedDate;
                                        worksheet.Cells["G" + StartRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                        worksheet.Cells["G" + StartRow].Style.Fill.BackgroundColor.SetColor(Color.Green);
                                        worksheet.Cells["G" + StartRow].Style.Font.Color.SetColor(Color.White);
                                        worksheet.Cells["G" + StartRow].Value = "YES";
                                    }
                                    else
                                    {
                                        worksheet.Cells["F" + StartRow].Value = row.CorrectedDate;
                                        worksheet.Cells["G" + StartRow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                        worksheet.Cells["G" + StartRow].Style.Fill.BackgroundColor.SetColor(Color.Red);
                                        worksheet.Cells["G" + StartRow].Style.Font.Color.SetColor(Color.White);
                                        worksheet.Cells["G" + StartRow].Value = "NO";
                                    }
                                    cou++;
                                    StartRow++;
                                }
                                StartRow++;
                            }
                            StartRow++;
                            break;
                        }
                        int startrow = 6;
                        worksheet.Cells["I3"].Value = "Date:";
                        worksheet.Cells["I" + startrow].Value = row.CorrectedDate;
                        worksheet.Cells["J3"].Value = "Work Done";
                        var pmcpid = Serverdb.tblhistpms.Where(m => m.pmsid == row.pmsid).Select(m => m.Pmcheckpointid).ToList();
                        foreach (var item in pmcpid)
                        {
                            var checkpointdata1 = Serverdb.configuration_tblpmcheckpoint.Where(m => m.pmcpID == item && m.CellID == cellid && m.Isdeleted == 0).Select(m => m.pmcpID).FirstOrDefault();
                            var checklistdata1 = Serverdb.configuration_tblpmchecklist.Where(m => m.pmcpID == checkpointdata1).ToList();
                            foreach (var row1 in checklistdata1)
                            {
                                var work = Serverdb.tblhistpms.Where(m => m.Pmcheckpointid == checkpointdata1 && m.Pmchecklistname == row1.CheckList && m.CorrectedDate == row.CorrectedDate).FirstOrDefault();
                                if (work.workdone == 1)
                                {
                                    worksheet.Cells["I" + startrow].Value = row.CorrectedDate;
                                    worksheet.Cells["J" + startrow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    worksheet.Cells["J" + startrow].Style.Fill.BackgroundColor.SetColor(Color.Green);
                                    worksheet.Cells["J" + startrow].Style.Font.Color.SetColor(Color.White);
                                    worksheet.Cells["J" + startrow].Value = "YES";
                                }
                                else
                                {
                                    worksheet.Cells["I" + startrow].Value = row.CorrectedDate;
                                    worksheet.Cells["J" + startrow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    worksheet.Cells["J" + startrow].Style.Fill.BackgroundColor.SetColor(Color.Red);
                                    worksheet.Cells["J" + startrow].Style.Font.Color.SetColor(Color.White);
                                    worksheet.Cells["J" + startrow].Value = "NO";
                                }
                                startrow++;

                            }
                        }
                        startrow++;
                        break;
                    }
                }
            }
            else if (count1 == 3)
            {
                int StartRow1 = 6;
                int count2 = 0;
                for (i = 0; i < count1; i++)
                {
                    int pmsid = Convert.ToInt32(dt.Rows[i][0]);
                    var pmsdata = Serverdb.tblhistpms.Where(m => m.pmsid == pmsid).ToList();

                    foreach (var row in pmsdata)
                    {
                        if (count2 == 0)
                        {
                            string cellname = Serverdb.tblcells.Where(m => m.CellID == cellid).Select(m => m.CellName).FirstOrDefault();
                            string machinename = Serverdb.tblmachinedetails.Where(m => m.MachineID == machineid).Select(m => m.MachineDisplayName).FirstOrDefault();
                            worksheet.Cells["C2"].Value = cellname;
                            worksheet.Cells["E2"].Value = machinename;
                            var pmcpid = Serverdb.tblhistpms.Where(m => m.pmsid == row.pmsid).Select(m => m.Pmcheckpointid).Distinct().ToList();
                            foreach (var item in pmcpid)
                            {
                                SlNo = 1;
                                var checkpointdata = Serverdb.configuration_tblpmcheckpoint.Where(m => m.pmcpID == item && m.CellID == cellid && m.Isdeleted == 0).FirstOrDefault();
                                worksheet.Cells["A" + StartRow].Value = "";
                                worksheet.Cells["B" + StartRow].Value = checkpointdata.TypeofCheckpoint;
                                //StartRow++;
                                var checklistdata = Serverdb.configuration_tblpmchecklist.Where(m => m.pmcpID == checkpointdata.pmcpID).ToList();
                                foreach (var row1 in checklistdata)
                                {
                                    var histrecord = Serverdb.tblhistpms.Where(m => m.Pmchecklistname == row1.CheckList && m.CorrectedDate == row.CorrectedDate && m.Pmcheckpointid == checkpointdata.pmcpID).FirstOrDefault();
                                    if (histrecord != null)
                                    {

                                        worksheet.Cells["A" + StartRow1].Value = SlNo++;
                                        worksheet.Cells["B" + StartRow1].Value = row1.CheckList;
                                        worksheet.Cells["C" + StartRow1].Value = row1.Frequency;
                                        worksheet.Cells["D" + StartRow1].Value = row1.How;
                                        worksheet.Cells["E" + StartRow1].Value = row1.Value;
                                        //worksheet.Cells["G3"].Value = row.CorrectedDate;
                                        //worksheet.Cells["F" + StartRow1].Value = row.CorrectedDate;

                                        var work = Serverdb.tblhistpms.Where(m => m.Pmcheckpointid == checkpointdata.pmcpID && m.Pmchecklistname == row1.CheckList && m.CorrectedDate == row.CorrectedDate).FirstOrDefault();
                                        if (work != null)
                                            if (work.workdone == 1)
                                            {
                                                worksheet.Cells["F" + StartRow1].Value = row.CorrectedDate;
                                                worksheet.Cells["G" + StartRow1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                                worksheet.Cells["G" + StartRow1].Style.Fill.BackgroundColor.SetColor(Color.Green);
                                                worksheet.Cells["G" + StartRow1].Style.Font.Color.SetColor(Color.White);
                                                worksheet.Cells["G" + StartRow1].Value = "YES";
                                            }
                                            else
                                            {
                                                worksheet.Cells["F" + StartRow1].Value = row.CorrectedDate;
                                                worksheet.Cells["G" + StartRow1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                                worksheet.Cells["G" + StartRow1].Style.Fill.BackgroundColor.SetColor(Color.Red);
                                                worksheet.Cells["G" + StartRow1].Style.Font.Color.SetColor(Color.White);
                                                worksheet.Cells["G" + StartRow1].Value = "NO";
                                            }
                                    }
                                    StartRow1++;
                                }
                                StartRow++;
                            }
                            StartRow++;
                            count2++;
                            break;
                        }
                        else if (count2 == 1)
                        {
                            int startrow = 6;
                            worksheet.Cells["I3"].Value = "Date:";
                            worksheet.Cells["J" + startrow].Value = row.CorrectedDate;
                            worksheet.Cells["J3"].Value = "Work Done";

                            var pmcpid = Serverdb.tblhistpms.Where(m => m.pmsid == row.pmsid).Select(m => m.Pmcheckpointid).Distinct().ToList();
                            foreach (var item in pmcpid)
                            {
                                var checkpointdata1 = Serverdb.configuration_tblpmcheckpoint.Where(m => m.pmcpID == item && m.CellID == cellid && m.Isdeleted == 0).Select(m => m.pmcpID).FirstOrDefault();
                                var checklistdata1 = Serverdb.configuration_tblpmchecklist.Where(m => m.pmcpID == checkpointdata1).ToList();
                                foreach (var row1 in checklistdata1)
                                {
                                    var work = Serverdb.tblhistpms.Where(m => m.Pmcheckpointid == checkpointdata1 && m.Pmchecklistname == row1.CheckList && m.CorrectedDate == row.CorrectedDate).FirstOrDefault();
                                    if (work != null)
                                        if (work.workdone == 1)
                                        {
                                            worksheet.Cells["I" + startrow].Value = row.CorrectedDate;
                                            worksheet.Cells["J" + startrow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                            worksheet.Cells["J" + startrow].Style.Fill.BackgroundColor.SetColor(Color.Green);
                                            worksheet.Cells["J" + startrow].Style.Font.Color.SetColor(Color.White);
                                            worksheet.Cells["J" + startrow].Value = "YES";
                                        }
                                        else
                                        {
                                            worksheet.Cells["I" + startrow].Value = row.CorrectedDate;
                                            worksheet.Cells["J" + startrow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                            worksheet.Cells["J" + startrow].Style.Fill.BackgroundColor.SetColor(Color.Red);
                                            worksheet.Cells["J" + startrow].Style.Font.Color.SetColor(Color.White);
                                            worksheet.Cells["J" + startrow].Value = "NO";
                                        }
                                    startrow++;
                                }
                            }
                            startrow++;
                            count2++;
                            break;
                        }
                        else if (count2 == 2)
                        {
                            int startrow = 6;
                            worksheet.Cells["L3"].Value = "Date:";
                            worksheet.Cells["L" + startrow].Value = row.CorrectedDate;
                            worksheet.Cells["M3"].Value = "Work Done";
                            var pmcpid = Serverdb.tblhistpms.Where(m => m.pmsid == row.pmsid).Select(m => m.Pmcheckpointid).Distinct().ToList();
                            foreach (var item in pmcpid)
                            {
                                var checkpointdata1 = Serverdb.configuration_tblpmcheckpoint.Where(m => m.pmcpID == item && m.CellID == cellid && m.Isdeleted == 0).Select(m => m.pmcpID).FirstOrDefault();
                                var checklistdata1 = Serverdb.configuration_tblpmchecklist.Where(m => m.pmcpID == checkpointdata1).ToList();
                                foreach (var row1 in checklistdata1)
                                {
                                    var work = Serverdb.tblhistpms.Where(m => m.Pmcheckpointid == checkpointdata1 && m.Pmchecklistname == row1.CheckList && m.CorrectedDate == row.CorrectedDate).FirstOrDefault();
                                    if (work != null)
                                        if (work.workdone == 1)
                                        {
                                            worksheet.Cells["L" + startrow].Value = row.CorrectedDate;
                                            worksheet.Cells["M" + startrow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                            worksheet.Cells["M" + startrow].Style.Fill.BackgroundColor.SetColor(Color.Green);
                                            worksheet.Cells["M" + startrow].Style.Font.Color.SetColor(Color.White);
                                            worksheet.Cells["M" + startrow].Value = "YES";
                                        }
                                        else
                                        {
                                            worksheet.Cells["L" + startrow].Value = row.CorrectedDate;
                                            worksheet.Cells["M" + startrow].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                            worksheet.Cells["M" + startrow].Style.Fill.BackgroundColor.SetColor(Color.Red);
                                            worksheet.Cells["M" + startrow].Style.Font.Color.SetColor(Color.White);
                                            worksheet.Cells["M" + startrow].Value = "NO";
                                        }
                                    count2++;
                                    startrow++;
                                }
                            }
                            startrow++;
                            break;
                        }
                    }
                    //StartRow++;
                    //SlNo = 1;
                }

            }
            p.Save();

            string path1 = System.IO.Path.Combine(FileDir, "PMS_Report_" + ToDate.ToString("yyyy-MM-dd") + ".xlsx");
            System.IO.FileInfo file1 = new System.IO.FileInfo(path1);
            string Outgoingfile = "PMS_Report_" + ToDate.ToString("yyyy-MM-dd") + ".xlsx";
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
            using (i_facility_shaktiEntities1 Serverdb = new i_facility_shaktiEntities1())
            {


                ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName", plantid);
                ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName", shopid);
                ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName", cellid);
                ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName", machineid);
            }
            return View();
        }

        public ActionResult AlaramReport()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];


            //var result = db.tblmachinedetails.Select(m=>m.ShopNo).Distinct();

            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            //ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");
            ViewData["ByMethod"] = "0";
            return View();

        }
        [HttpPost]
        public ActionResult AlaramReport(int PlantID, String FromDate, int ShopID = 0, int CellID = 0)
        {

            AlramReportExcel(PlantID, FromDate, ShopID, CellID);
            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            //ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");
            ViewData["ByMethod"] = "0";
            return View();
        }

        public ActionResult IdleList()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");

            return View();
        }

        [HttpPost]
        public ActionResult IdleList(i_facilitylibrary.DAL.tblreportholder report, string PlantID, String FromDate, String ToDate, string ShopID = null, string CellID = null, string MachineID = null)
        {
            //generateIdleReportExcel(FromDate, ToDate, PlantID, ShopID, CellID, MachineID);
            generateIdleReportExcel(report.FromDate.ToString(), report.ToDate.ToString(), PlantID, ShopID, CellID, MachineID);
            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");

            return View();
        }

        //Idle Down Report
        public ActionResult generateIdleReportExcel_old(string startDate, string EndtDate, string PlantID, string ShopID = null, string CellID = null, string WorkCenterID = null)
        {
            ReportsDao obj = new ReportsDao();
            Dao obj1 = new Dao();
            Dao1 obj2 = new Dao1();
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj2 = new Dao1(_conn);
            obj = new ReportsDao(_conn);
            DateTime frda = DateTime.Now;
            

            DateTime frmDate = Convert.ToDateTime(startDate);
            DateTime toDate = Convert.ToDateTime(EndtDate);

            FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\IDLE_Report.xlsx");
            ExcelPackage templatep = new ExcelPackage(templateFile);
            ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];

            String FileDir = @"C:\SRKS_ifacility\ReportsList\" + System.DateTime.Now.ToString("yyyy-MM-dd");
            //String FileDir = @"C:\inetpub\ContiAndonWebApp\Reports\" + System.DateTime.Now.ToString("yyyy");

            bool exists = System.IO.Directory.Exists(FileDir);

            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "IDLE_Report" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "IDLE_Report" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //" to " + toda.ToString("yyyy-MM-dd") + 
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
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), Templatews);
            }

            int sheetcount = p.Workbook.Worksheets.Count;
            p.Workbook.Worksheets.MoveToStart(sheetcount);

            worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            worksheet.Cells["C6"].Value = frmDate.ToString("dd-MM-yyyy");
            worksheet.Cells["E6"].Value = toDate.ToString("dd-MM-yyyy");

            string FDate = frmDate.ToString("yyyy-MM-dd");
            string TDate = toDate.ToString("yyyy-MM-dd");

            string lowestLevel = null;
            int MacCount = 0;
            int plantId = 0, shopId = 0, cellId = 0, wcId = 0;
            if (string.IsNullOrEmpty(WorkCenterID))
            {
                if (string.IsNullOrEmpty(CellID))
                {
                    if (string.IsNullOrEmpty(ShopID))
                    {
                        if (string.IsNullOrEmpty(PlantID))
                        { //donothing 
                        }
                        else
                        {
                            lowestLevel = "Plant";
                            plantId = Convert.ToInt32(PlantID);
                        }
                    }
                    else
                    {
                        lowestLevel = "Shop";
                        shopId = Convert.ToInt32(ShopID);
                    }
                }
                else
                {
                    lowestLevel = "Cell";
                    cellId = Convert.ToInt32(CellID);
                }
            }
            else
            {
                lowestLevel = "WorkCentre";
                wcId = Convert.ToInt32(WorkCenterID);
            }

            DataTable dataHolder = new DataTable();
            DataTable dataHolder1 = new DataTable();
            DataTable dataHolder2 = new DataTable();
            MsqlConnection mc = new MsqlConnection();
            mc.open();
            string sql1 = null;
            string sql2 = null;
            if (lowestLevel == "Plant")
            {
                sql1 = "SELECT MachineID,StartDateTime,EndDateTime,MessageCodeID,CorrectedDate,EntryTime,Shift FROM tbllossofentry WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where PlantID = " + plantId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( case when (IsDeleted = 1) then ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' ) end) ))  ORDER BY LossID ASC";
                //sql1 = "SELECT MachineID,StartDateTime,EndDateTime,MessageCodeID,CorrectedDate,EntryTime,Shift FROM tbllossofentry WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where PlantID = " + plantId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ((IsDeleted = 1) and ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' )) ))  ORDER BY LossID ASC";
                //sql2 = "SELECT MachineID, StartTime, EndTime, BreakDownCode, CorrectedDate, Shift FROM tblbreakdown WHERE CorrectedDate>= '" + FDate + "' AND CorrectedDate<= '" + TDate + "' AND DoneWithRow = 1 and MachineID in (select MachineID from tblmachinedetails where PlantID = " + plantId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ((IsDeleted = 1) and ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' )) )) ORDER BY BreakdownID ASC";
                //sql1 = "select loss.MachineID,loss.StartDateTime,loss.EndDateTime,loss.MessageCodeID,loss.CorrectedDate,loss.EntryTime,breakdown.BreakdownID FROM tbllossofentry as loss left join tblbreakdown as breakdown on loss.MachineID = breakdown.MachineID where (loss.DoneWithRow = 1 or breakdown.DoneWithRow = 1)  AND ((loss.CorrectedDate>='" + FDate + "' AND loss.CorrectedDate<='" + TDate + "') or  (breakdown.CorrectedDate>='" + FDate + "' AND breakdown.CorrectedDate<='" + TDate + "')) and loss.MachineID in (select MachineID from tblmachinedetails where PlantID = " + plantId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ((IsDeleted = 1) and ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' )) )) order by MachineID,loss.StartDateTime";
            }
            else if (lowestLevel == "Shop")
            {
                sql1 = "SELECT MachineID,StartDateTime,EndDateTime,MessageCodeID,CorrectedDate,EntryTime,Shift FROM tbllossofentry WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where ShopID = " + shopId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( case when (IsDeleted = 1) then ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' ) end) ))  ORDER BY LossID ASC";
                //sql1 = "SELECT MachineID,StartDateTime,EndDateTime,MessageCodeID,CorrectedDate,EntryTime,Shift FROM tbllossofentry WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where ShopID = " + shopId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ((IsDeleted = 1) and ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' ) ) ))  ORDER BY LossID ASC";
                //sql2 = "SELECT MachineID, StartTime, EndTime, BreakDownCode, CorrectedDate, Shift FROM tblbreakdown WHERE CorrectedDate>= '" + FDate + "' AND CorrectedDate<= '" + TDate + "' AND DoneWithRow = 1 and MachineID in (select MachineID from tblmachinedetails where ShopID = " + shopId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ((IsDeleted = 1) and ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' )) )) ORDER BY BreakdownID ASC";
                //sql1 = "select loss.MachineID,loss.StartDateTime,loss.EndDateTime,loss.MessageCodeID,loss.CorrectedDate,loss.EntryTime,breakdown.BreakdownID FROM tbllossofentry as loss left join tblbreakdown as breakdown on loss.MachineID = breakdown.MachineID where (loss.DoneWithRow = 1 or breakdown.DoneWithRow = 1)  AND ((loss.CorrectedDate>='" + FDate + "' AND loss.CorrectedDate<='" + TDate + "') or  (breakdown.CorrectedDate>='" + FDate + "' AND breakdown.CorrectedDate<='" + TDate + "')) and loss.MachineID in (select MachineID from tblmachinedetails  where ShopID = " + shopId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ((IsDeleted = 1) and ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' ) ) )) order by MachineID,loss.StartDateTime";
            }
            else if (lowestLevel == "Cell")
            {
                sql1 = "SELECT MachineID,StartDateTime,EndDateTime,MessageCodeID,CorrectedDate,EntryTime,Shift FROM tbllossofentry WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where CellID = " + cellId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( case when (IsDeleted = 1) then ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' ) end) ))  ORDER BY LossID ASC";
                //sql1 = "SELECT MachineID,StartDateTime,EndDateTime,MessageCodeID,CorrectedDate,EntryTime,Shift FROM tbllossofentry WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where CellID = " + cellId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ((IsDeleted = 1) and ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' )) ))  ORDER BY LossID ASC";
                //sql2 = "SELECT MachineID, StartTime, EndTime, BreakDownCode, CorrectedDate, Shift FROM tblbreakdown WHERE CorrectedDate>= '" + FDate + "' AND CorrectedDate<= '" + TDate + "' AND DoneWithRow = 1 and MachineID in (select MachineID from tblmachinedetails where CellID = " + cellId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ((IsDeleted = 1) and ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' )) )) ORDER BY BreakdownID ASC";
                //sql1 = "select loss.MachineID,loss.StartDateTime,loss.EndDateTime,loss.MessageCodeID,loss.CorrectedDate,loss.EntryTime,breakdown.BreakdownID FROM tbllossofentry as loss left join tblbreakdown as breakdown on loss.MachineID = breakdown.MachineID where (loss.DoneWithRow = 1 or breakdown.DoneWithRow = 1)  AND ((loss.CorrectedDate>='" + FDate + "' AND loss.CorrectedDate<='" + TDate + "') or  (breakdown.CorrectedDate>='" + FDate + "' AND breakdown.CorrectedDate<='" + TDate + "')) and loss.MachineID in (select MachineID from tblmachinedetails where CellID = " + cellId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ((IsDeleted = 1) and ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' )) )) order by MachineID,loss.StartDateTime ";
            }
            else if (lowestLevel == "WorkCentre")
            {
                sql1 = "SELECT MachineID,StartDateTime,EndDateTime,MessageCodeID,CorrectedDate,EntryTime,Shift FROM tbllossofentry WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where MachineID = " + wcId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( case when (IsDeleted = 1) then ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' ) end) ))  ORDER BY LossID ASC";
                //sql1 = "SELECT MachineID,StartDateTime,EndDateTime,MessageCodeID,CorrectedDate,EntryTime,Shift FROM tbllossofentry WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where MachineID = " + wcId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( (IsDeleted = 1) and ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' )) ))  ORDER BY LossID ASC";
                //sql2 = "SELECT MachineID, StartTime, EndTime, BreakDownCode, CorrectedDate, Shift FROM tblbreakdown WHERE CorrectedDate>= '" + FDate + "' AND CorrectedDate<= '" + TDate + "' AND DoneWithRow = 1 and MachineID in (select MachineID from tblmachinedetails where MachineID = " + wcId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ((IsDeleted = 1) and ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' )) )) ORDER BY BreakdownID ASC";
                //sql1 = "select loss.MachineID,loss.StartDateTime,loss.EndDateTime,loss.MessageCodeID,loss.CorrectedDate,loss.EntryTime,breakdown.BreakdownID FROM tbllossofentry as loss left join tblbreakdown as breakdown on loss.MachineID = breakdown.MachineID where (loss.DoneWithRow = 1 or breakdown.DoneWithRow = 1)  AND ((loss.CorrectedDate>='" + FDate + "' AND loss.CorrectedDate<='" + TDate + "') or  (breakdown.CorrectedDate>='" + FDate + "' AND breakdown.CorrectedDate<='" + TDate + "')) and loss.MachineID in (select MachineID from tblmachinedetails where MachineID = " + wcId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( (IsDeleted = 1) and ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' )) )) order by MachineID,loss.StartDateTime ";
            }

            SqlDataAdapter da1 = new SqlDataAdapter(sql1, mc.msqlConnection);
            da1.Fill(dataHolder);
            SqlDataAdapter da2 = new SqlDataAdapter(sql2, mc.msqlConnection);
            da2.Fill(dataHolder1);
            dataHolder2.Merge(dataHolder);
            dataHolder2.Merge(dataHolder1);
            mc.close();
            if (dataHolder2.Rows.Count != 0)
            {
                using (SqlCommand cmd = new SqlCommand("[dbo].[LossAndBreakDown]", mc.msqlConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    //cmd.Parameters.AddWithValue("@breakDownId", QueryDate.Date);
                    //cmd.Parameters.AddWithValue("@MachineID", Machine.MachineID);
                    mc.open();
                    cmd.ExecuteNonQuery();
                    mc.close();
                }
                var Col = 'B';
                int Row = 8;
                int Sno = 1;
                for (int i = 0; i < dataHolder2.Rows.Count; i++)
                {
                    int MachineID = Convert.ToInt32(dataHolder2.Rows[i][0]);
                    List<string> HierarchyData = GetHierarchyData(MachineID);

                    worksheet.Cells["B" + Row].Value = Sno;
                    worksheet.Cells["C" + Row].Value = HierarchyData[0]; //Plant Name
                    worksheet.Cells["D" + Row].Value = HierarchyData[1]; // Shop Name
                    worksheet.Cells["E" + Row].Value = HierarchyData[2]; //Cell Name
                    worksheet.Cells["F" + Row].Value = HierarchyData[3]; //Machine Name
                    //worksheet.Cells["G" + Row].Value = HierarchyData[4]; //WC Name

                    if (string.IsNullOrEmpty(dataHolder2.Rows[i][1].ToString()) == false)
                    {
                        DateTime startdate = Convert.ToDateTime(dataHolder2.Rows[i][1]);
                        worksheet.Cells["K" + Row].Value = startdate.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    if (string.IsNullOrEmpty(dataHolder2.Rows[i][2].ToString()) == false)
                    {
                        DateTime Enddate = Convert.ToDateTime(dataHolder2.Rows[i][2]);
                        worksheet.Cells["L" + Row].Value = Enddate.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    //if (string.IsNullOrEmpty(dataHolder.Rows[i][5].ToString()) == false)
                    //{
                    //    DateTime EntryTime = Convert.ToDateTime(dataHolder.Rows[i][5]);
                    //    worksheet.Cells["F" + Row].Value = EntryTime.ToString("yyyy-MM-dd HH:mm:ss");
                    //}
                    if (string.IsNullOrEmpty(dataHolder2.Rows[i][1].ToString()) == false && string.IsNullOrEmpty(dataHolder2.Rows[i][2].ToString()) == false)
                    {
                        DateTime StartTime = DateTime.Now;
                        StartTime = Convert.ToDateTime(dataHolder2.Rows[i][1]);
                        DateTime EndTime = DateTime.Now;
                        EndTime = Convert.ToDateTime(dataHolder2.Rows[i][2]);

                        TimeSpan ts = EndTime.Subtract(StartTime);
                        int H = ts.Hours;
                        int M = ts.Minutes;
                        int S = ts.Seconds;
                        string Hs = null, Ms = null, Ss = null;
                        if (H < 10)
                        {
                            Hs = "0" + H;
                        }
                        else
                        {
                            Hs = H.ToString();
                        }
                        if (M < 10)
                        {
                            Ms = "0" + M;
                        }
                        else
                        {
                            Ms = M.ToString();
                        }
                        if (S < 10)
                        {
                            Ss = "0" + S;
                        }
                        else
                        {
                            Ss = S.ToString();
                        }

                        string time = Hs + " : " + Ms + " : " + Ss;
                        //double Duration = EndTime.Subtract(StartTime).TotalMinutes;
                        //worksheet.Cells["I" + Row].Value = Math.Round(Duration, 2);
                        worksheet.Cells["M" + Row].Value = time;


                        //double Duration = EndTime.Subtract(StartTime).TotalMinutes;
                        //worksheet.Cells["I" + Row].Value = Math.Round(Duration, 2);
                    }
                    if (string.IsNullOrEmpty(dataHolder2.Rows[i][3].ToString()) == false)
                    {
                        int msgcd = Convert.ToInt32(dataHolder2.Rows[i][3]);
                        var a = obj1.GetLossDet1(msgcd);
                        //var a = db.tbllossescodes.Where(m => m.LossCodeID == msgcd).FirstOrDefault();

                        if (a.LossCodesLevel == 1)
                        {
                            if (a.LossCode == "999")
                            {
                                worksheet.Cells["H" + Row].Value = a.MessageType;
                            }
                            else
                            {
                                worksheet.Cells["H" + Row].Value = a.LossCode;
                            }
                        }
                        else if (a.LossCodesLevel == 2)
                        {
                            int lossid = Convert.ToInt32(a.LossCodesLevel1ID);
                            var level1data = obj1.GetLossDet1(lossid);
                            //var level1data = db.tbllossescodes.Where(m => m.LossCodeID == lossid).FirstOrDefault();
                            worksheet.Cells["H" + Row].Value = level1data.LossCode;
                            worksheet.Cells["I" + Row].Value = a.LossCode;
                        }
                        else if (a.LossCodesLevel == 3)
                        {
                            int lossid2 = Convert.ToInt32(a.LossCodesLevel1ID);
                            int lossid3 = Convert.ToInt32(a.LossCodesLevel2ID);
                            var level1data = obj1.GetLossDet1(lossid2);
                            var level2data = obj1.GetLossDet1(lossid3);
                            //var level1data = db.tbllossescodes.Where(m => m.LossCodeID == lossid2).FirstOrDefault();
                            //var level2data = db.tbllossescodes.Where(m => m.LossCodeID == lossid3).FirstOrDefault();
                            worksheet.Cells["H" + Row].Value = level1data.LossCode;
                            worksheet.Cells["I" + Row].Value = level2data.LossCode;
                            worksheet.Cells["J" + Row].Value = a.LossCode;
                        }
                    }
                    if (string.IsNullOrEmpty(dataHolder.Rows[i][6].ToString()) == false)
                    {
                        worksheet.Cells["N" + Row].Value = dataHolder.Rows[i][6].ToString();
                    }
                    else
                        worksheet.Cells["N" + Row].Value = "-";
                    Row++;
                    Sno++;
                }
            }

            int noOfRows = 8 + dataHolder.Rows.Count + 2;
            worksheet.Cells["B8:B" + noOfRows].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["C8:C" + noOfRows].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["D8:E" + noOfRows].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["F8:H" + noOfRows].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["I8:J" + noOfRows].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["C6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            ExcelRange r1, r2, r3;
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            p.Save();
            //Downloding Excel
            string path1 = System.IO.Path.Combine(FileDir, "IDLE_Report" + frda.ToString("yyyy-MM-dd") + ".xlsx");
            System.IO.FileInfo file1 = new System.IO.FileInfo(path1);
            string Outgoingfile = "Loss_Register" + frda.ToString("yyyy-MM-dd") + ".xlsx";
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
            return View();
        }

        public ActionResult generateIdleReportExcel(string startDate, string EndtDate, string PlantID, string ShopID = null, string CellID = null, string WorkCenterID = null)
        {
            ReportsDao obj = new ReportsDao();
            Dao obj1 = new Dao();
            Dao1 obj2 = new Dao1();
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj2 = new Dao1(_conn);
            obj = new ReportsDao(_conn);
            DateTime frda = DateTime.Now;

            DateTime frmDate = Convert.ToDateTime(startDate);
            DateTime toDate = Convert.ToDateTime(EndtDate);

            FileInfo templateFile = new FileInfo(@"C:\TataReport\NewTemplates\IDLE_Report.xlsx");
            ExcelPackage templatep = new ExcelPackage(templateFile);
            ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];

            String FileDir = @"C:\TataReport\ReportsList\" + System.DateTime.Now.ToString("yyyy-MM-dd");
            //String FileDir = @"C:\inetpub\ContiAndonWebApp\Reports\" + System.DateTime.Now.ToString("yyyy");

            bool exists = System.IO.Directory.Exists(FileDir);

            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "IDLE_Report" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "IDLE_Report" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //" to " + toda.ToString("yyyy-MM-dd") + 
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
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), Templatews);
            }

            int sheetcount = p.Workbook.Worksheets.Count;
            p.Workbook.Worksheets.MoveToStart(sheetcount);

            worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            worksheet.Cells["C6"].Value = frmDate.ToString("dd-MM-yyyy");
            worksheet.Cells["E6"].Value = toDate.ToString("dd-MM-yyyy");

            string FDate = frmDate.ToString("yyyy-MM-dd");
            string TDate = toDate.ToString("yyyy-MM-dd");

            string lowestLevel = null;
            int MacCount = 0;
            int plantId = 0, shopId = 0, cellId = 0, wcId = 0;
            if (string.IsNullOrEmpty(WorkCenterID))
            {
                if (string.IsNullOrEmpty(CellID))
                {
                    if (string.IsNullOrEmpty(ShopID))
                    {
                        if (string.IsNullOrEmpty(PlantID))
                        { //donothing 
                        }
                        else
                        {
                            lowestLevel = "Plant";
                            plantId = Convert.ToInt32(PlantID);
                        }
                    }
                    else
                    {
                        lowestLevel = "Shop";
                        shopId = Convert.ToInt32(ShopID);
                    }
                }
                else
                {
                    lowestLevel = "Cell";
                    cellId = Convert.ToInt32(CellID);
                }
            }
            else
            {
                lowestLevel = "WorkCentre";
                wcId = Convert.ToInt32(WorkCenterID);
            }

            DataTable dataHolder = new DataTable();
            MsqlConnection mc = new MsqlConnection();
            mc.open();
            string sql1 = null;
            if (lowestLevel == "Plant")
            {
                //sql1 = "SELECT MachineID,StartDateTime,EndDateTime,MessageCodeID,CorrectedDate,EntryTime,Shift FROM tbllossofentry WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where PlantID = " + plantId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( case when (IsDeleted = 1) then ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' ) end) ))  ORDER BY LossID ASC";
                sql1 = "SELECT MachineID,StartDateTime,EndDateTime,MessageCodeID,CorrectedDate,EntryTime,Shift FROM tbllivelossofentry WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where PlantID = " + plantId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ((IsDeleted = 1) and ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' )) ))  ORDER BY LossID ASC";
            }
            else if (lowestLevel == "Shop")
            {
                //sql1 = "SELECT MachineID,StartDateTime,EndDateTime,MessageCodeID,CorrectedDate,EntryTime,Shift FROM tbllossofentry WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where ShopID = " + shopId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( case when (IsDeleted = 1) then ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' ) end) ))  ORDER BY LossID ASC";
                sql1 = "SELECT MachineID,StartDateTime,EndDateTime,MessageCodeID,CorrectedDate,EntryTime,Shift FROM tbllivelossofentry WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where ShopID = " + shopId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ((IsDeleted = 1) and ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' ) ) ))  ORDER BY LossID ASC";
            }
            else if (lowestLevel == "Cell")
            {
                //sql1 = "SELECT MachineID,StartDateTime,EndDateTime,MessageCodeID,CorrectedDate,EntryTime,Shift FROM tbllossofentry WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where CellID = " + cellId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( case when (IsDeleted = 1) then ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' ) end) ))  ORDER BY LossID ASC";
                sql1 = "SELECT MachineID,StartDateTime,EndDateTime,MessageCodeID,CorrectedDate,EntryTime,Shift FROM tbllivelossofentry WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where CellID = " + cellId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ((IsDeleted = 1) and ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' )) ))  ORDER BY LossID ASC";
            }
            else if (lowestLevel == "WorkCentre")
            {
                //sql1 = "SELECT MachineID,StartDateTime,EndDateTime,MessageCodeID,CorrectedDate,EntryTime,Shift FROM tbllossofentry WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where MachineID = " + wcId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( case when (IsDeleted = 1) then ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' ) end) ))  ORDER BY LossID ASC";
                sql1 = "SELECT MachineID,StartDateTime,EndDateTime,MessageCodeID,CorrectedDate,EntryTime,Shift FROM tbllivelossofentry WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where MachineID = " + wcId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( (IsDeleted = 1) and ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' )) ))  ORDER BY LossID ASC";
            }

            SqlDataAdapter da1 = new SqlDataAdapter(sql1, mc.msqlConnection);
            da1.Fill(dataHolder);
            mc.close();
            if (dataHolder.Rows.Count != 0)
            {
                var Col = 'B';
                int Row = 8;
                int Sno = 1;
                for (int i = 0; i < dataHolder.Rows.Count; i++)
                {
                    int MachineID = Convert.ToInt32(dataHolder.Rows[i][0]);
                    List<string> HierarchyData = GetHierarchyData(MachineID);

                    worksheet.Cells["B" + Row].Value = Sno;
                    worksheet.Cells["C" + Row].Value = HierarchyData[0]; //Plant Name
                    worksheet.Cells["D" + Row].Value = HierarchyData[1]; // Shop Name
                    worksheet.Cells["E" + Row].Value = HierarchyData[2]; //Cell Name
                    worksheet.Cells["F" + Row].Value = HierarchyData[3]; //Cell Name

                    // worksheet.Cells["F" + Row].Value = MachineID; //wc no
                    //worksheet.Cells["G" + Row].Value = HierarchyData[3]; //WC Name

                    if (string.IsNullOrEmpty(dataHolder.Rows[i][1].ToString()) == false)
                    {
                        DateTime startdate = Convert.ToDateTime(dataHolder.Rows[i][1]);
                        worksheet.Cells["K" + Row].Value = startdate.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    if (string.IsNullOrEmpty(dataHolder.Rows[i][2].ToString()) == false)
                    {
                        DateTime Enddate = Convert.ToDateTime(dataHolder.Rows[i][2]);
                        worksheet.Cells["L" + Row].Value = Enddate.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    //if (string.IsNullOrEmpty(dataHolder.Rows[i][5].ToString()) == false)
                    //{
                    //    DateTime EntryTime = Convert.ToDateTime(dataHolder.Rows[i][5]);
                    //    worksheet.Cells["F" + Row].Value = EntryTime.ToString("yyyy-MM-dd HH:mm:ss");
                    //}
                    if (string.IsNullOrEmpty(dataHolder.Rows[i][1].ToString()) == false && string.IsNullOrEmpty(dataHolder.Rows[i][2].ToString()) == false)
                    {
                        DateTime StartTime = DateTime.Now;
                        StartTime = Convert.ToDateTime(dataHolder.Rows[i][1]);
                        DateTime EndTime = DateTime.Now;
                        EndTime = Convert.ToDateTime(dataHolder.Rows[i][2]);

                        TimeSpan ts = EndTime.Subtract(StartTime);
                        int H = ts.Hours;
                        int M = ts.Minutes;
                        int S = ts.Seconds;
                        string Hs = null, Ms = null, Ss = null;
                        if (H < 10)
                        {
                            Hs = "0" + H;
                        }
                        else
                        {
                            Hs = H.ToString();
                        }
                        if (M < 10)
                        {
                            Ms = "0" + M;
                        }
                        else
                        {
                            Ms = M.ToString();
                        }
                        if (S < 10)
                        {
                            Ss = "0" + S;
                        }
                        else
                        {
                            Ss = S.ToString();
                        }

                        string time = Hs + " : " + Ms + " : " + Ss;
                        //double Duration = EndTime.Subtract(StartTime).TotalMinutes;
                        //worksheet.Cells["I" + Row].Value = Math.Round(Duration, 2);
                        worksheet.Cells["M" + Row].Value = time;


                        //double Duration = EndTime.Subtract(StartTime).TotalMinutes;
                        //worksheet.Cells["I" + Row].Value = Math.Round(Duration, 2);
                    }
                    if (string.IsNullOrEmpty(dataHolder.Rows[i][3].ToString()) == false)
                    {
                        int msgcd = Convert.ToInt32(dataHolder.Rows[i][3]);
                        var a = obj1.GetLossDet1(msgcd);
                        //var a = db.tbllossescodes.Where(m => m.LossCodeID == msgcd).FirstOrDefault();

                        if (a.LossCodesLevel == 1)
                        {
                            if (a.LossCode == "999")
                            {
                                worksheet.Cells["H" + Row].Value = a.MessageType;
                            }
                            else
                            {
                                worksheet.Cells["H" + Row].Value = a.LossCode;
                            }
                        }
                        else if (a.LossCodesLevel == 2)
                        {
                            int lossid = Convert.ToInt32(a.LossCodesLevel1ID);
                            var level1data = obj1.GetLossDet1(lossid);
                            //var level1data = db.tbllossescodes.Where(m => m.LossCodeID == lossid).FirstOrDefault();
                            worksheet.Cells["H" + Row].Value = level1data.LossCode;
                            worksheet.Cells["I" + Row].Value = a.LossCode;
                        }
                        else if (a.LossCodesLevel == 3)
                        {
                            int lossid2 = Convert.ToInt32(a.LossCodesLevel1ID);
                            int lossid3 = Convert.ToInt32(a.LossCodesLevel2ID);
                            var level1data = obj1.GetLossDet1(lossid2);
                            var level2data = obj1.GetLossDet1(lossid3);
                            //var level1data = db.tbllossescodes.Where(m => m.LossCodeID == lossid2).FirstOrDefault();
                            //var level2data = db.tbllossescodes.Where(m => m.LossCodeID == lossid3).FirstOrDefault();
                            worksheet.Cells["H" + Row].Value = level1data.LossCode;
                            worksheet.Cells["I" + Row].Value = level2data.LossCode;
                            worksheet.Cells["J" + Row].Value = a.LossCode;
                        }
                    }
                    if (string.IsNullOrEmpty(dataHolder.Rows[i][6].ToString()) == false)
                    {
                        worksheet.Cells["N" + Row].Value = dataHolder.Rows[i][6].ToString();
                    }
                    else
                        worksheet.Cells["N" + Row].Value = "-";
                    Row++;
                    Sno++;
                }
            }
            int noOfRows = 8 + dataHolder.Rows.Count + 2;
            worksheet.Cells["B8:B" + noOfRows].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["C8:C" + noOfRows].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["D8:E" + noOfRows].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["F8:H" + noOfRows].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["I8:J" + noOfRows].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["C6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            ExcelRange r1, r2, r3;
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            p.Save();
            //Downloding Excel
            string path1 = System.IO.Path.Combine(FileDir, "IDLE_Report" + frda.ToString("yyyy-MM-dd") + ".xlsx");
            System.IO.FileInfo file1 = new System.IO.FileInfo(path1);
            string Outgoingfile = "Loss_Register" + frda.ToString("yyyy-MM-dd") + ".xlsx";
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
            return View();
        }

        public ActionResult BreakdownList()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");
            return View();
        }
        [HttpPost]
        public ActionResult BreakdownList(i_facilitylibrary.DAL.tblreportholder report, string PlantID, String FromDate, String ToDate, string ShopID = null, string CellID = null, string MachineID = null)
        {
            generateBreakDownReportExcel(report.FromDate.ToString(), report.ToDate.ToString(), PlantID, ShopID, CellID, MachineID);
            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");

            return View();
        }

        //Break Down Report
        public ActionResult generateBreakDownReportExcel_old(string startDate, string EndtDate)
        {
            ReportsDao obj = new ReportsDao();
            Dao obj1 = new Dao();
            Dao1 obj2 = new Dao1();
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj2 = new Dao1(_conn);
            obj = new ReportsDao(_conn);
            DateTime frda = DateTime.Now;

            DateTime frmDate = Convert.ToDateTime(startDate);
            DateTime toDate = Convert.ToDateTime(EndtDate);

            FileInfo templateFile = new FileInfo(@"C:\TataReport\Templet\BreakDownReport.xlsx");
            ExcelPackage templatep = new ExcelPackage(templateFile);
            ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];

            String FileDir = @"C:\TataReport\ReportsList\" + System.DateTime.Now.ToString("yyyy-MM-dd");
            //String FileDir = @"C:\inetpub\ContiAndonWebApp\Reports\" + System.DateTime.Now.ToString("yyyy");

            bool exists = System.IO.Directory.Exists(FileDir);

            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "BreakDownReport" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "BreakDownReport" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //" to " + toda.ToString("yyyy-MM-dd") + 
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
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), Templatews);
            }

            int sheetcount = p.Workbook.Worksheets.Count;
            p.Workbook.Worksheets.MoveToStart(sheetcount);

            worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            worksheet.Cells["C6"].Value = frmDate.ToString("dd-MM-yyyy");
            worksheet.Cells["E6"].Value = toDate.ToString("dd-MM-yyyy");

            string FDate = frmDate.ToString("yyyy-MM-dd");
            string TDate = toDate.ToString("yyyy-MM-dd");
            //var data = db.tblbreakdowns.Where(m => m.CorrectedDate >= FDate && m.CorrectedDate <= TDate);

            MsqlConnection mc = new MsqlConnection();
            mc.open();
            String sql1 = "SELECT MachineID,StartTime,EndTime,BreakDownCode,CorrectedDate,Shift FROM tblbreakdown WHERE CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' AND DoneWithRow = 1 ORDER BY BreakdownID ASC";
            SqlDataAdapter da1 = new SqlDataAdapter(sql1, mc.msqlConnection);
            DataTable dataHolder = new DataTable();
            da1.Fill(dataHolder);
            mc.close();
            int bdid = 0;

            if (dataHolder.Rows.Count != 0)
            {
                var Col = 'B';
                int Row = 8;
                int Sno = 1;
                for (int i = 0; i < dataHolder.Rows.Count; i++)
                {
                    int MachineID = Convert.ToInt32(dataHolder.Rows[i][0]);
                    //tblmachinedetail machineDetails = obj1.GetMachineDetails(MachineID);
                    //tblmachinedetail machineDetails = obj1.GetMachineDetails(MachineID);
                    var machineDetails = Serverdb.tblmachinedetails.Where(m => m.MachineID == MachineID).Select(m => m.MachineDisplayName).FirstOrDefault();
                    worksheet.Cells["B" + Row].Value = Sno;
                    worksheet.Cells["C" + Row].Value = machineDetails;
                    if (string.IsNullOrEmpty(dataHolder.Rows[i][1].ToString()) == false)
                    {
                        DateTime startdate = Convert.ToDateTime(dataHolder.Rows[i][1]);
                        worksheet.Cells["D" + Row].Value = startdate.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    if (string.IsNullOrEmpty(dataHolder.Rows[i][2].ToString()) == false)
                    {
                        DateTime Enddate = Convert.ToDateTime(dataHolder.Rows[i][2]);
                        worksheet.Cells["E" + Row].Value = Enddate.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    if (string.IsNullOrEmpty(dataHolder.Rows[i][3].ToString()) == false)
                        bdid = Convert.ToInt32(dataHolder.Rows[i][3]);
                    mc.open();
                    String sql2 = "SELECT * FROM tblbreakdowncodes WHERE BreakDownCodeID= " + bdid + "";
                    SqlDataAdapter da2 = new SqlDataAdapter(sql2, mc.msqlConnection);
                    DataTable dataHolder2 = new DataTable();
                    da2.Fill(dataHolder2);
                    mc.close();

                    int level = Convert.ToInt32(dataHolder2.Rows[0][4]);
                    if (level == 1)
                    {
                        worksheet.Cells["F" + Row].Value = dataHolder2.Rows[0][1].ToString();
                    }
                    else if (level == 2)
                    {
                        int id = Convert.ToInt32(dataHolder2.Rows[0][5]);
                        var data = obj1.GetLossDet1(id);
                        // var data = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodeID == id).SingleOrDefault();
                        worksheet.Cells["G" + Row].Value = dataHolder2.Rows[0][1].ToString();
                        worksheet.Cells["F" + Row].Value = data.LossCode;
                    }
                    else if (level == 3)
                    {
                        int id = Convert.ToInt32(dataHolder2.Rows[0][5]);
                        var data = obj1.GetLossDet1(id);
                        //var data = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodeID == id).SingleOrDefault();

                        int id1 = Convert.ToInt32(dataHolder2.Rows[0][6]);
                        var data1 = obj1.GetLossDet1(id);
                        // var data1 = db.tbllossescodes.Where(m => m.IsDeleted == 0 && m.LossCodeID == id).SingleOrDefault();

                        worksheet.Cells["H" + Row].Value = dataHolder2.Rows[0][1].ToString();
                        worksheet.Cells["F" + Row].Value = data.LossCode;
                        worksheet.Cells["G" + Row].Value = data1.LossCode;
                    }
                    if (string.IsNullOrEmpty(dataHolder.Rows[i][1].ToString()) == false && string.IsNullOrEmpty(dataHolder.Rows[i][2].ToString()) == false)
                    {
                        DateTime StartTime = DateTime.Now;
                        StartTime = Convert.ToDateTime(dataHolder.Rows[i][1]);
                        DateTime EndTime = DateTime.Now;
                        EndTime = Convert.ToDateTime(dataHolder.Rows[i][2]);
                        TimeSpan ts = EndTime.Subtract(StartTime);
                        int H = ts.Hours;
                        int M = ts.Minutes;
                        int S = ts.Seconds;
                        string Hs = null, Ms = null, Ss = null;
                        if (H < 10)
                        {
                            Hs = "0" + H;
                        }
                        else
                        {
                            Hs = H.ToString();
                        }
                        if (M < 10)
                        {
                            Ms = "0" + M;
                        }
                        else
                        {
                            Ms = M.ToString();
                        }
                        if (S < 10)
                        {
                            Ss = "0" + S;
                        }
                        else
                        {
                            Ss = S.ToString();
                        }

                        string time = Hs + " : " + Ms + " : " + Ss;
                        //double Duration = EndTime.Subtract(StartTime).TotalMinutes;
                        //worksheet.Cells["I" + Row].Value = Math.Round(Duration, 2);
                        worksheet.Cells["I" + Row].Value = time;
                    }
                    if (string.IsNullOrEmpty(dataHolder.Rows[i][5].ToString()) == false)
                        worksheet.Cells["J" + Row].Value = dataHolder.Rows[i][5].ToString();
                    else
                        worksheet.Cells["J" + Row].Value = "-";
                    Row++;
                    Sno++;
                }
            }

            ExcelRange r1, r2, r3;

            int noOfRows = 8 + dataHolder.Rows.Count + 2;
            worksheet.Cells["B8:B" + noOfRows].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["C8:C" + noOfRows].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["D8:E" + noOfRows].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["F8:H" + noOfRows].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["I8:J" + noOfRows].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["C6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            p.Save();

            //Downloding Excel
            string path1 = System.IO.Path.Combine(FileDir, "BreakDownReport" + frda.ToString("yyyy-MM-dd") + ".xlsx");
            System.IO.FileInfo file1 = new System.IO.FileInfo(path1);
            string Outgoingfile = "BreakDownReport" + frda.ToString("yyyy-MM-dd") + ".xlsx";
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
            return View();
        }

        public ActionResult generateBreakDownReportExcel(string startDate, string EndtDate, string PlantID, string ShopID = null, string CellID = null, string WorkCenterID = null)
        {
            ReportsDao obj = new ReportsDao();
            Dao obj1 = new Dao();
            Dao1 obj2 = new Dao1();
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj2 = new Dao1(_conn);
            obj = new ReportsDao(_conn);
            DateTime frda = DateTime.Now;

            DateTime frmDate = Convert.ToDateTime(startDate);
            DateTime toDate = Convert.ToDateTime(EndtDate);

            FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\BreakDownReport.xlsx");
            ExcelPackage templatep = new ExcelPackage(templateFile);
            ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];

            String FileDir = @"C:\SRKS_ifacility\ReportsList\" + System.DateTime.Now.ToString("yyyy-MM-dd");
            //String FileDir = @"C:\inetpub\ContiAndonWebApp\Reports\" + System.DateTime.Now.ToString("yyyy");

            bool exists = System.IO.Directory.Exists(FileDir);

            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "BreakDownReport" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "BreakDownReport" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //" to " + toda.ToString("yyyy-MM-dd") + 
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
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), Templatews);
            }

            int sheetcount = p.Workbook.Worksheets.Count;
            p.Workbook.Worksheets.MoveToStart(sheetcount);

            worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            worksheet.Cells["C6"].Value = frmDate.ToString("dd-MM-yyyy");
            worksheet.Cells["E6"].Value = toDate.ToString("dd-MM-yyyy");

            string FDate = frmDate.ToString("yyyy-MM-dd");
            string TDate = toDate.ToString("yyyy-MM-dd");

            string lowestLevel = null;
            int MacCount = 0;
            int plantId = 0, shopId = 0, cellId = 0, wcId = 0;
            if (string.IsNullOrEmpty(WorkCenterID))
            {
                if (string.IsNullOrEmpty(CellID))
                {
                    if (string.IsNullOrEmpty(ShopID))
                    {
                        if (string.IsNullOrEmpty(PlantID))
                        { //donothing 
                        }
                        else
                        {
                            lowestLevel = "Plant";
                            plantId = Convert.ToInt32(PlantID);
                        }
                    }
                    else
                    {
                        lowestLevel = "Shop";
                        shopId = Convert.ToInt32(ShopID);
                    }
                }
                else
                {
                    lowestLevel = "Cell";
                    cellId = Convert.ToInt32(CellID);
                }
            }
            else
            {
                lowestLevel = "WorkCentre";
                wcId = Convert.ToInt32(WorkCenterID);
            }

            DataTable dataHolder = new DataTable();
            MsqlConnection mc = new MsqlConnection();
            mc.open();
            string sql1 = null;
            if (lowestLevel == "Plant")
            {
                //sql1 = "SELECT MachineID,StartDateTime,EndDateTime,MessageCodeID,CorrectedDate,EntryTime,Shift FROM tbllossofentry WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where PlantID = " + plantId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( case when (IsDeleted = 1) then ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' ) end) ))  ORDER BY LossID ASC";
                sql1 = "SELECT MachineID,StartTime,EndTime,BreakDownCode,CorrectedDate,Shift FROM tblbreakdown WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where PlantID = " + plantId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ((IsDeleted = 1) and ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' )) ))  ORDER BY BreakdownID ASC";
            }
            else if (lowestLevel == "Shop")
            {
                //sql1 = "SELECT MachineID,StartDateTime,EndDateTime,MessageCodeID,CorrectedDate,EntryTime,Shift FROM tbllossofentry WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where ShopID = " + shopId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( case when (IsDeleted = 1) then ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' ) end) ))  ORDER BY LossID ASC";
                sql1 = "SELECT MachineID,StartTime,EndTime,BreakDownCode,CorrectedDate,Shift FROM tblbreakdown WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where ShopID = " + shopId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ((IsDeleted = 1) and ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' ) ) ))  ORDER BY BreakdownID ASC";
            }
            else if (lowestLevel == "Cell")
            {
                //sql1 = "SELECT MachineID,StartDateTime,EndDateTime,MessageCodeID,CorrectedDate,EntryTime,Shift FROM tbllossofentry WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where CellID = " + cellId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( case when (IsDeleted = 1) then ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' ) end) ))  ORDER BY LossID ASC";
                sql1 = "SELECT MachineID,StartTime,EndTime,BreakDownCode,CorrectedDate,Shift FROM tblbreakdown WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where CellID = " + cellId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ((IsDeleted = 1) and ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' )) ))  ORDER BY BreakdownID ASC";
            }
            else if (lowestLevel == "WorkCentre")
            {
                //sql1 = "SELECT MachineID,StartDateTime,EndDateTime,MessageCodeID,CorrectedDate,EntryTime,Shift FROM tbllossofentry WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where MachineID = " + wcId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( case when (IsDeleted = 1) then ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' ) end) ))  ORDER BY LossID ASC";
                sql1 = "SELECT MachineID,StartTime,EndTime,BreakDownCode,CorrectedDate,Shift FROM tblbreakdown WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where MachineID = " + wcId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( (IsDeleted = 1) and ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' )) ))  ORDER BY BreakdownID ASC";
            }

            SqlDataAdapter da1 = new SqlDataAdapter(sql1, mc.msqlConnection);
            da1.Fill(dataHolder);
            mc.close();
            if (dataHolder.Rows.Count != 0)
            {
                var Col = 'B';
                int Row = 8;
                int Sno = 1;
                for (int i = 0; i < dataHolder.Rows.Count; i++)
                {
                    int MachineID = Convert.ToInt32(dataHolder.Rows[i][0]);
                    List<string> HierarchyData = GetHierarchyData(MachineID);

                    worksheet.Cells["B" + Row].Value = Sno;
                    worksheet.Cells["C" + Row].Value = HierarchyData[0]; //Plant Name
                    worksheet.Cells["D" + Row].Value = HierarchyData[1]; // Shop Name
                    worksheet.Cells["E" + Row].Value = HierarchyData[2]; //Cell Name
                    worksheet.Cells["F" + Row].Value = HierarchyData[3]; //WC Name
                    //var machinedisplayname = Serverdb.tblmachinedetails.Where(m => m.MachineID == MachineID).Select(m=>m.MachineDisplayName);
                   // worksheet.Cells["G" + Row].Value = machinedisplayname; //WC Name

                    if (string.IsNullOrEmpty(dataHolder.Rows[i][1].ToString()) == false)
                    {
                        DateTime startdate = Convert.ToDateTime(dataHolder.Rows[i][1]);
                        worksheet.Cells["K" + Row].Value = startdate.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    if (string.IsNullOrEmpty(dataHolder.Rows[i][2].ToString()) == false)
                    {
                        DateTime Enddate = Convert.ToDateTime(dataHolder.Rows[i][2]);
                        worksheet.Cells["L" + Row].Value = Enddate.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    //if (string.IsNullOrEmpty(dataHolder.Rows[i][5].ToString()) == false)
                    //{
                    //    DateTime EntryTime = Convert.ToDateTime(dataHolder.Rows[i][5]);
                    //    worksheet.Cells["F" + Row].Value = EntryTime.ToString("yyyy-MM-dd HH:mm:ss");
                    //}
                    if (string.IsNullOrEmpty(dataHolder.Rows[i][1].ToString()) == false && string.IsNullOrEmpty(dataHolder.Rows[i][2].ToString()) == false)
                    {
                        DateTime StartTime = DateTime.Now;
                        StartTime = Convert.ToDateTime(dataHolder.Rows[i][1]);
                        DateTime EndTime = DateTime.Now;
                        EndTime = Convert.ToDateTime(dataHolder.Rows[i][2]);

                        TimeSpan ts = EndTime.Subtract(StartTime);
                        int H = ts.Hours;
                        int M = ts.Minutes;
                        int S = ts.Seconds;
                        string Hs = null, Ms = null, Ss = null;
                        if (H < 10)
                        {
                            Hs = "0" + H;
                        }
                        else
                        {
                            Hs = H.ToString();
                        }
                        if (M < 10)
                        {
                            Ms = "0" + M;
                        }
                        else
                        {
                            Ms = M.ToString();
                        }
                        if (S < 10)
                        {
                            Ss = "0" + S;
                        }
                        else
                        {
                            Ss = S.ToString();
                        }

                        string time = Hs + " : " + Ms + " : " + Ss;
                        //double Duration = EndTime.Subtract(StartTime).TotalMinutes;
                        //worksheet.Cells["I" + Row].Value = Math.Round(Duration, 2);
                        worksheet.Cells["M" + Row].Value = time;


                        //double Duration = EndTime.Subtract(StartTime).TotalMinutes;
                        //worksheet.Cells["I" + Row].Value = Math.Round(Duration, 2);
                    }
                    if (string.IsNullOrEmpty(dataHolder.Rows[i][3].ToString()) == false)
                    {
                        int msgcd = Convert.ToInt32(dataHolder.Rows[i][3]);
                        var a = obj1.GetLossDet1(msgcd);
                        //var a = db.tbllossescodes.Where(m => m.LossCodeID == msgcd).FirstOrDefault();

                        if (a.LossCodesLevel == 1)
                        {
                            if (a.LossCode == "999")
                            {
                                worksheet.Cells["H" + Row].Value = a.MessageType;
                            }
                            else
                            {
                                worksheet.Cells["H" + Row].Value = a.LossCode;
                            }
                        }
                        else if (a.LossCodesLevel == 2)
                        {
                            int lossid = Convert.ToInt32(a.LossCodesLevel1ID);
                            var level1data = obj1.GetLossDet1(lossid);
                            //var level1data = db.tbllossescodes.Where(m => m.LossCodeID == lossid).FirstOrDefault();
                            worksheet.Cells["H" + Row].Value = level1data.LossCode;
                            worksheet.Cells["I" + Row].Value = a.LossCode;
                        }
                        else if (a.LossCodesLevel == 3)
                        {
                            int lossid2 = Convert.ToInt32(a.LossCodesLevel1ID);
                            int lossid3 = Convert.ToInt32(a.LossCodesLevel2ID);
                            var level1data = obj1.GetLossDet1(lossid2);
                            var level2data = obj1.GetLossDet1(lossid3);
                            //var level1data = db.tbllossescodes.Where(m => m.LossCodeID == lossid2).FirstOrDefault();
                            //var level2data = db.tbllossescodes.Where(m => m.LossCodeID == lossid3).FirstOrDefault();
                            worksheet.Cells["H" + Row].Value = level1data.LossCode;
                            worksheet.Cells["I" + Row].Value = level2data.LossCode;
                            worksheet.Cells["J" + Row].Value = a.LossCode;
                        }
                    }
                    if (string.IsNullOrEmpty(dataHolder.Rows[i][5].ToString()) == false)
                    {
                        worksheet.Cells["N" + Row].Value = dataHolder.Rows[i][5].ToString();
                    }
                    else
                        worksheet.Cells["N" + Row].Value = "-";
                    Row++;
                    Sno++;
                }
            }
            int noOfRows = 8 + dataHolder.Rows.Count + 2;
            worksheet.Cells["B8:B" + noOfRows].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["C8:C" + noOfRows].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["D8:E" + noOfRows].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["F8:H" + noOfRows].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells["I8:J" + noOfRows].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["C6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            ExcelRange r1, r2, r3;
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            p.Save();
            //Downloding Excel
            string path1 = System.IO.Path.Combine(FileDir, "BreakDownReport" + frda.ToString("yyyy-MM-dd") + ".xlsx");
            System.IO.FileInfo file1 = new System.IO.FileInfo(path1);
            string Outgoingfile = "BreakDownReport" + frda.ToString("yyyy-MM-dd") + ".xlsx";
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
            return View();
        }

        //public ActionResult generateIdleReportExcel(string startDate, string EndtDate, int PlantID, int ShopID, int CellID, int MachineID)
        //{
        //    DateTime frda = DateTime.Now;

        //    DateTime frmDate = Convert.ToDateTime(startDate);
        //    DateTime toDate = Convert.ToDateTime(EndtDate);

        //    FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\IDLE_Report.xlsx");
        //    ExcelPackage templatep = new ExcelPackage(templateFile);
        //    ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];

        //    String FileDir = @"C:\SRKS_ifacility\ReportsList\" + System.DateTime.Now.ToString("yyyy-MM-dd");
        //    //String FileDir = @"C:\inetpub\ContiAndonWebApp\Reports\" + System.DateTime.Now.ToString("yyyy");

        //    bool exists = System.IO.Directory.Exists(FileDir);

        //    if (!exists)
        //        System.IO.Directory.CreateDirectory(FileDir);

        //    FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "IDLE_Report" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
        //    if (newFile.Exists)
        //    {
        //        try
        //        {
        //            newFile.Delete();  // ensures we create a new workbook
        //            newFile = new FileInfo(System.IO.Path.Combine(FileDir, "IDLE_Report" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //" to " + toda.ToString("yyyy-MM-dd") + 
        //        }
        //        catch
        //        {
        //            TempData["Excelopen"] = "Excel with same date is already open, please close it and try to generate!!!!";
        //            //return View();
        //        }
        //    }
        //    //Using the File for generation and populating it
        //    ExcelPackage p = null;
        //    p = new ExcelPackage(newFile);
        //    ExcelWorksheet worksheet = null;

        //    //Creating the WorkSheet for populating
        //    try
        //    {
        //        worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), Templatews);
        //    }
        //    catch { }

        //    if (worksheet == null)
        //    {
        //        worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), Templatews);
        //    }

        //    int sheetcount = p.Workbook.Worksheets.Count;
        //    p.Workbook.Worksheets.MoveToStart(sheetcount);

        //    worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        //    worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

        //    worksheet.Cells["C6"].Value = frmDate.ToString("dd-MM-yyyy");
        //    worksheet.Cells["E6"].Value = toDate.ToString("dd-MM-yyyy");

        //    string FDate = frmDate.ToString("yyyy-MM-dd");
        //    string TDate = toDate.ToString("yyyy-MM-dd");

        //    string lowestLevel = null;
        //    int MacCount = 0;
        //    int plantId = 0, shopId = 0, cellId = 0, wcId = 0;
        //    if (MachineID == 0)
        //    {
        //        if (CellID == 0)
        //        {
        //            if (ShopID == 0)
        //            {
        //                if (PlantID == 0)
        //                {
        //                    //donothing
        //                }
        //                else
        //                {
        //                    lowestLevel = "Plant";
        //                    plantId = Convert.ToInt32(PlantID);
        //                }
        //            }
        //            else
        //            {
        //                lowestLevel = "Shop";
        //                shopId = Convert.ToInt32(ShopID);
        //            }
        //        }
        //        else
        //        {
        //            lowestLevel = "Cell";
        //            cellId = Convert.ToInt32(CellID);
        //        }
        //    }
        //    else
        //    {
        //        lowestLevel = "WorkCentre";
        //        wcId = Convert.ToInt32(MachineID);
        //    }

        //    DataTable dataHolder = new DataTable();
        //    MsqlConnection mc = new MsqlConnection();
        //    mc.open();
        //    string sql1 = null;
        //    if (lowestLevel == "Plant")
        //    {
        //        sql1 = "SELECT MachineID,StartDateTime,EndDateTime,MessageCodeID,CorrectedDate,EntryTime,Shift FROM tbllossofentry WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where PlantID = " + plantId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ((IsDeleted = 1) and ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' )) ))  ORDER BY LossID ASC";
        //        //sql1 = "SELECT StartTime,EndTime,DurationInSec FROM " + dbName + ".tbllivemode WHERE IsCompleted = 1 AND CorrectedDate='" + FDate + "' and ColorCode='Yellow' and MachineID in (select MachineID from " + dbName + ".tblmachinedetails where PlantID = " + plantId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ((IsDeleted = 1) and ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' )) ))  ORDER BY ModeID ASC";
        //    }
        //    else if (lowestLevel == "Shop")
        //    {
        //        //sql1 = "SELECT MachineID,StartDateTime,EndDateTime,MessageCodeID,CorrectedDate,EntryTime,Shift FROM tbllossofentry WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where ShopID = " + shopId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( case when (IsDeleted = 1) then ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' ) end) ))  ORDER BY LossID ASC";
        //        sql1 = "SELECT MachineID,StartDateTime,EndDateTime,MessageCodeID,CorrectedDate,EntryTime,Shift FROM tbllossofentry WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where ShopID = " + shopId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ((IsDeleted = 1) and ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' ) ) ))  ORDER BY LossID ASC";
        //    }
        //    else if (lowestLevel == "Cell")
        //    {
        //        //sql1 = "SELECT MachineID,StartDateTime,EndDateTime,MessageCodeID,CorrectedDate,EntryTime,Shift FROM tbllossofentry WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where CellID = " + cellId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( case when (IsDeleted = 1) then ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' ) end) ))  ORDER BY LossID ASC";
        //        sql1 = "SELECT MachineID,StartDateTime,EndDateTime,MessageCodeID,CorrectedDate,EntryTime,Shift FROM tbllossofentry WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where CellID = " + cellId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ((IsDeleted = 1) and ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' )) ))  ORDER BY LossID ASC";
        //    }
        //    else if (lowestLevel == "WorkCentre")
        //    {
        //        //sql1 = "SELECT MachineID,StartDateTime,EndDateTime,MessageCodeID,CorrectedDate,EntryTime,Shift FROM tbllossofentry WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where MachineID = " + wcId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( case when (IsDeleted = 1) then ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' ) end) ))  ORDER BY LossID ASC";
        //        sql1 = "SELECT MachineID,StartDateTime,EndDateTime,MessageCodeID,CorrectedDate,EntryTime,Shift FROM tbllossofentry WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where MachineID = " + wcId + "  and ((InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( (IsDeleted = 1) and ( InsertedOn <= '" + toDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + frmDate + "' )) ))  ORDER BY LossID ASC";
        //    }

        //    SqlDataAdapter da1 = new SqlDataAdapter(sql1, mc.msqlConnection);
        //    da1.Fill(dataHolder);
        //    mc.close();
        //    if (dataHolder.Rows.Count != 0)
        //    {
        //        var Col = 'B';
        //        int Row = 8;
        //        int Sno = 1;
        //        for (int i = 0; i < dataHolder.Rows.Count; i++)
        //        {
        //            int MachineID1 = wcId;
        //            List<string> HierarchyData = GetHierarchyData(MachineID1);

        //            worksheet.Cells["B" + Row].Value = Sno;
        //            worksheet.Cells["C" + Row].Value = HierarchyData[0]; //Plant Name
        //            worksheet.Cells["D" + Row].Value = HierarchyData[1]; // Shop Name
        //            worksheet.Cells["E" + Row].Value = HierarchyData[2]; //Cell Name
        //            worksheet.Cells["F" + Row].Value = HierarchyData[3]; //WC Name

        //            if (string.IsNullOrEmpty(dataHolder.Rows[i][0].ToString()) == false)
        //            {
        //                DateTime startdate = Convert.ToDateTime(dataHolder.Rows[i][0]);
        //                worksheet.Cells["G" + Row].Value = startdate.ToString("yyyy-MM-dd HH:mm:ss");
        //            }
        //            if (string.IsNullOrEmpty(dataHolder.Rows[i][1].ToString()) == false)
        //            {
        //                DateTime Enddate = Convert.ToDateTime(dataHolder.Rows[i][1]);
        //                worksheet.Cells["H" + Row].Value = Enddate.ToString("yyyy-MM-dd HH:mm:ss");
        //            }
        //            //if (string.IsNullOrEmpty(dataHolder.Rows[i][5].ToString()) == false)
        //            //{
        //            //    DateTime EntryTime = Convert.ToDateTime(dataHolder.Rows[i][5]);
        //            //    worksheet.Cells["F" + Row].Value = EntryTime.ToString("yyyy-MM-dd HH:mm:ss");
        //            //}
        //            if (string.IsNullOrEmpty(dataHolder.Rows[i][0].ToString()) == false && string.IsNullOrEmpty(dataHolder.Rows[i][1].ToString()) == false)
        //            {
        //                DateTime StartTime = DateTime.Now;
        //                StartTime = Convert.ToDateTime(dataHolder.Rows[i][0]);
        //                DateTime EndTime = DateTime.Now;
        //                EndTime = Convert.ToDateTime(dataHolder.Rows[i][1]);

        //                TimeSpan ts = EndTime.Subtract(StartTime);
        //                int H = ts.Hours;
        //                int M = ts.Minutes;
        //                int S = ts.Seconds;
        //                string Hs = null, Ms = null, Ss = null;
        //                if (H < 10)
        //                {
        //                    Hs = "0" + H;
        //                }
        //                else
        //                {
        //                    Hs = H.ToString();
        //                }
        //                if (M < 10)
        //                {
        //                    Ms = "0" + M;
        //                }
        //                else
        //                {
        //                    Ms = M.ToString();
        //                }
        //                if (S < 10)
        //                {
        //                    Ss = "0" + S;
        //                }
        //                else
        //                {
        //                    Ss = S.ToString();
        //                }

        //                string time = Hs + " : " + Ms + " : " + Ss;
        //                //double Duration = EndTime.Subtract(StartTime).TotalMinutes;
        //                //worksheet.Cells["I" + Row].Value = Math.Round(Duration, 2);
        //                worksheet.Cells["I" + Row].Value = time;


        //                //double Duration = EndTime.Subtract(StartTime).TotalMinutes;
        //                //worksheet.Cells["I" + Row].Value = Math.Round(Duration, 2);
        //            }
        //            #region LossCodes Details
        //            //if (string.IsNullOrEmpty(dataHolder.Rows[i][3].ToString()) == false)
        //            //{
        //            //    int msgcd = Convert.ToInt32(dataHolder.Rows[i][3]);
        //            //    var a = Serverdb.tbllossescodes.Where(m => m.LossCodeID == msgcd).FirstOrDefault();

        //            //    if (a.LossCodesLevel == 1)
        //            //    {
        //            //        if (a.LossCode == "999")
        //            //        {
        //            //            worksheet.Cells["H" + Row].Value = a.MessageType;
        //            //        }
        //            //        else
        //            //        {
        //            //            worksheet.Cells["H" + Row].Value = a.LossCode;
        //            //        }
        //            //    }
        //            //    else if (a.LossCodesLevel == 2)
        //            //    {
        //            //        int lossid = Convert.ToInt32(a.LossCodesLevel1ID);
        //            //        //r level1data = obj1.GetLossDet1(lossid);
        //            //        var level1data = Serverdb.tbllossescodes.Where(m => m.LossCodeID == lossid).FirstOrDefault();
        //            //        worksheet.Cells["H" + Row].Value = level1data.LossCode;
        //            //        worksheet.Cells["I" + Row].Value = a.LossCode;
        //            //    }
        //            //    else if (a.LossCodesLevel == 3)
        //            //    {
        //            //        int lossid2 = Convert.ToInt32(a.LossCodesLevel1ID);
        //            //        int lossid3 = Convert.ToInt32(a.LossCodesLevel2ID);
        //            //        var level1data = Serverdb.tbllossescodes.Where(m => m.LossCodeID == lossid2).FirstOrDefault();
        //            //        var level2data = Serverdb.tbllossescodes.Where(m => m.LossCodeID == lossid3).FirstOrDefault();
        //            //        worksheet.Cells["H" + Row].Value = level1data.LossCode;
        //            //        worksheet.Cells["I" + Row].Value = level2data.LossCode;
        //            //        worksheet.Cells["J" + Row].Value = a.LossCode;
        //            //    }
        //            //}
        //            #endregion
        //            //if (string.IsNullOrEmpty(dataHolder.Rows[i][6].ToString()) == false)
        //            //{
        //            //    worksheet.Cells["N" + Row].Value = dataHolder.Rows[i][6].ToString();
        //            //}
        //            //else
        //            //    worksheet.Cells["N" + Row].Value = "-";
        //            Row++;
        //            Sno++;
        //        }
        //    }
        //    int noOfRows = 8 + dataHolder.Rows.Count + 2;
        //    worksheet.Cells["B8:B" + noOfRows].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //    worksheet.Cells["C8:C" + noOfRows].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        //    worksheet.Cells["D8:E" + noOfRows].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //    worksheet.Cells["F8:H" + noOfRows].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        //    worksheet.Cells["I8:J" + noOfRows].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //    worksheet.Cells["C6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        //    ExcelRange r1, r2, r3;
        //    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        //    p.Save();
        //    //Downloding Excel
        //    string path1 = System.IO.Path.Combine(FileDir, "IDLE_Report" + frda.ToString("yyyy-MM-dd") + ".xlsx");
        //    System.IO.FileInfo file1 = new System.IO.FileInfo(path1);
        //    string Outgoingfile = "Loss_Register" + frda.ToString("yyyy-MM-dd") + ".xlsx";
        //    if (file1.Exists)
        //    {
        //        Response.Clear();
        //        Response.ClearContent();
        //        Response.ClearHeaders();
        //        Response.AddHeader("Content-Disposition", "attachment; filename=" + Outgoingfile);
        //        Response.AddHeader("Content-Length", file1.Length.ToString());
        //        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //        Response.WriteFile(file1.FullName);
        //        Response.Flush();
        //        Response.Close();
        //    }
        //    return View();
        //}

        public ActionResult LossAnalysis()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];


            //var result = db.tblmachinedetails.Select(m=>m.ShopNo).Distinct();

            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");
            ViewData["ByMethod"] = "0";
            return View();

        }

        [HttpPost]
        public ActionResult LossAnalysis(int PlantID, String FromDate, String ToDate, int ShopID = 0, int CellID = 0, int MachineID = 0)
        {
            int Plantid = Convert.ToInt32(PlantID);

            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");

            ViewData["ByMethod"] = "1";
            LossAnalysisReportExcel(FromDate, ToDate, PlantID, ShopID, CellID, MachineID);
            return View();
            //return RedirectToAction("LossAnalysis");
        }

        public void LossAnalysisReportExcel(string StartDate, string EndDate, int PlantID, int ShopID, int CellID, int MachineID)
        {
            #region Excel and Stuff

            DateTime frda = DateTime.Now;
            if (string.IsNullOrEmpty(StartDate) == true)
            {
                StartDate = DateTime.Now.Date.ToString();
            }
            if (string.IsNullOrEmpty(EndDate) == true)
            {
                EndDate = StartDate;
            }

            DateTime frmDate = Convert.ToDateTime(StartDate);
            DateTime toDate = Convert.ToDateTime(EndDate);

            double TotalDay = toDate.Subtract(frmDate).TotalDays;

            FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\LossDetailsReport.xlsx");


          //  FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\IDLE_Report.xlsx");
            ExcelPackage templatep = new ExcelPackage(templateFile);
            ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];
            ExcelWorksheet TemplateGraph = templatep.Workbook.Worksheets[2];

            String FileDir = @"C:\SRKS_ifacility\ReportsList\" + System.DateTime.Now.ToString("yyyy-MM-dd");
            bool exists = System.IO.Directory.Exists(FileDir);
            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "LossDetailsReport" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "LossDetailsReport" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //" to " + toda.ToString("yyyy-MM-dd") + 
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
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), Templatews);
                worksheetGraph = p.Workbook.Worksheets.Add("Graphs", TemplateGraph);
            }
            catch { }

            if (worksheet == null)
            {
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), Templatews);
                worksheetGraph = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), TemplateGraph);
            }
            int sheetcount = p.Workbook.Worksheets.Count;
            p.Workbook.Worksheets.MoveToStart(sheetcount);
            worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            #endregion

            #region MacCount & LowestLevel
            string lowestLevel = null;
            int MacCount = 0;
            int plantId = 0, shopId = 0, cellId = 0, wcId = 0;
            if (MachineID == 0)
            {
                if (CellID == 0)
                {
                    if (ShopID == 0)
                    {
                        if (PlantID == 0)
                        {
                            //donothing
                        }
                        else
                        {
                            lowestLevel = "Plant";
                            plantId = Convert.ToInt32(PlantID);
                            MacCount = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == plantId).ToList().Count();
                        }
                    }
                    else
                    {
                        lowestLevel = "Shop";
                        shopId = Convert.ToInt32(ShopID);
                        MacCount = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == shopId).ToList().Count();
                    }
                }
                else
                {
                    lowestLevel = "Cell";
                    cellId = Convert.ToInt32(CellID);
                    MacCount = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == cellId).ToList().Count();
                }
            }
            else
            {
                lowestLevel = "WorkCentre";
                wcId = MachineID;
                MacCount = 1;
            }

            #endregion

            #region Get Machines List
            DataTable machin = new DataTable();
            DateTime endDateTime = Convert.ToDateTime(toDate.AddDays(1).ToString("yyyy-MM-dd") + " " + new TimeSpan(6, 0, 0));
            string startDateTime = frmDate.ToString("yyyy-MM-dd");
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                String query1 = null;
                if (lowestLevel == "Plant")
                {
                    //query1 = " SELECT  distinct MachineID FROM " + dbName + ".tblmachinedetails WHERE PlantID = " + PlantID + "  and IsNormalWC = 0  and ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( case when (IsDeleted = 1) then ( InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + startDateTime + "' ) end) ) ;";
                    query1 = " SELECT  distinct MachineID FROM " + dbName + ".tblmachinedetails WHERE PlantID = " + PlantID + "  and IsNormalWC = 0  and IsDeleted = 0) ;";
                }
                else if (lowestLevel == "Shop")
                {
                    //query1 = " SELECT * FROM " + dbName + ".tblmachinedetails WHERE ShopID = " + ShopID + "  and IsNormalWC = 0   and  ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( case when (IsDeleted = 1) then ( InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + startDateTime + "' ) end) ) ;";
                    query1 = " SELECT * FROM " + dbName + ".tblmachinedetails WHERE ShopID = " + ShopID + "  and IsNormalWC = 0 and IsDeleted = 0 ;";
                }
                else if (lowestLevel == "Cell")
                {
                    //query1 = " SELECT * FROM " + dbName + ".tblmachinedetails WHERE CellID = " + CellID + "  and IsNormalWC = 0  and   ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( case when (IsDeleted = 1) then ( InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + startDateTime + "' ) end) ) ;";
                    query1 = " SELECT * FROM " + dbName + ".tblmachinedetails WHERE CellID = " + CellID + "  and IsNormalWC = 0  and IsDeleted = 0;";
                }
                else if (lowestLevel == "WorkCentre")
                {
                    //query1 = " SELECT * FROM " + dbName + ".tblmachinedetails WHERE MachineID = " + MachineID + "  and IsNormalWC = 0  and  ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or   ( case when (IsDeleted = 1) then ( InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and DeletedDate >= '" + startDateTime + "' ) end) ) ;";
                    query1 = " SELECT * FROM " + dbName + ".tblmachinedetails WHERE MachineID = " + MachineID + "  and IsNormalWC = 0  and  IsDeleted = 0 ;";
                }
                SqlDataAdapter da1 = new SqlDataAdapter(query1, mc.msqlConnection);
                da1.Fill(machin);
                mc.close();
            }
            #endregion

            //DataTable for Consolidated Data 
            DataTable DTConsolidatedLosses = new DataTable();
            DTConsolidatedLosses.Columns.Add("Plant", typeof(string));
            DTConsolidatedLosses.Columns.Add("Shop", typeof(string));
            DTConsolidatedLosses.Columns.Add("Cell", typeof(string));
            DTConsolidatedLosses.Columns.Add("WCInvNo", typeof(string));
            DTConsolidatedLosses.Columns.Add("WCName", typeof(string));
            DTConsolidatedLosses.Columns.Add("CorrectedDate", typeof(string));

            //Get All Losses and Insert into DataTable
            DataTable LossCodesData = new DataTable();
            using (MsqlConnection mcLossCodes = new MsqlConnection())
            {
                mcLossCodes.open();
                string startDateTime1 = frmDate.ToString("yyyy-MM-dd") + " " + new TimeSpan(6, 0, 0);
                //string query = @"select LossCodeID,LossCode from " + dbName + ".tbllossescodes  where ((CreatedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or "
                //            + "( case when (IsDeleted = 1) then ( CreatedOn <=  '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ModifiedOn >= '" + startDateTime1 + "') end) ) and LossCodeID NOT IN (  "
                //            + "SELECT DISTINCT LossCodeID FROM (  "
                //            + "SELECT DISTINCT LossCodesLevel1ID AS LossCodeID FROM " + dbName + ".tbllossescodes where LossCodesLevel1ID is not null and ((CreatedOn <=  '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or  "
                //            + "( case when (IsDeleted = 1) then ( CreatedOn <=  '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ModifiedOn >= '" + startDateTime1 + "' ) end) ) "
                //            + "UNION  "
                //            + "SELECT DISTINCT LossCodesLevel2ID AS LossCodeID FROM " + dbName + ".tbllossescodes where LossCodesLevel2ID is not null  and ((CreatedOn <=  '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or  "
                //            + "( case when (IsDeleted = 1) then ( CreatedOn <=  '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and ModifiedOn >= '" + startDateTime1 + "' ) end) )  "
                //            + ") AS derived ) order by LossCodesLevel1ID;";

                string query = @"select LossCodeID,LossCode from " + dbName + ".tbllossescodes  where IsDeleted = 0 order by LossCodesLevel1ID;";

                SqlDataAdapter daLossCodesData = new SqlDataAdapter(query, mcLossCodes.msqlConnection);
                daLossCodesData.Fill(LossCodesData);
                mcLossCodes.close();
            }

            DTConsolidatedLosses.Columns.Add("MinorLoss", typeof(double));
            DTConsolidatedLosses.Columns["MinorLoss"].DefaultValue = "0";

            int LossesStartsATCol = 11;
            var LossesList = new List<KeyValuePair<int, string>>();

            #region LossCodes Into LossList
            for (int i = 0; i < LossCodesData.Rows.Count; i++)
            {
                int losscode = Convert.ToInt32(LossCodesData.Rows[i][0]);
                string losscodeName = Convert.ToString(LossCodesData.Rows[i][1]);

                var lossdata = Serverdb.tbllossescodes.Where(m => m.LossCodeID == losscode).FirstOrDefault();
                int level = lossdata.LossCodesLevel;
                if (level == 3)
                {
                    int lossLevel1ID = Convert.ToInt32(lossdata.LossCodesLevel1ID);
                    int lossLevel2ID = Convert.ToInt32(lossdata.LossCodesLevel2ID);
                    var lossdata1 = Serverdb.tbllossescodes.Where(m => m.LossCodeID == lossLevel1ID).FirstOrDefault();
                    var lossdata2 = Serverdb.tbllossescodes.Where(m => m.LossCodeID == lossLevel2ID).FirstOrDefault();
                    losscodeName = lossdata1.LossCodeDesc + " :: " + lossdata2.LossCodeDesc + " : " + lossdata.LossCodeDesc;
                }

                else if (level == 2)
                {
                    int lossLevel1ID = Convert.ToInt32(lossdata.LossCodesLevel1ID);
                    var lossdata1 = Serverdb.tbllossescodes.Where(m => m.LossCodeID == lossLevel1ID).FirstOrDefault();

                    losscodeName = lossdata1.LossCodeDesc + ":" + lossdata.LossCodeDesc;
                }
                else if (level == 1)
                {
                    if (losscode == 999)
                    {
                        losscodeName = "NoCode Entered";
                    }
                    else if (losscode == 9999)
                    {
                        losscodeName = "UnIdentified BreakDown";
                    }
                    else
                    {
                        losscodeName = lossdata.LossCodeDesc;
                    }
                }
                //losscodeName = LossHierarchy3rdLevel(losscode);
                DTConsolidatedLosses.Columns.Add(losscodeName, typeof(double));
                DTConsolidatedLosses.Columns[losscodeName].DefaultValue = "0";

                //Code to write LossesNames to Excel.
                string columnAlphabet = ExcelColumnFromNumber(LossesStartsATCol);

                worksheet.Cells[columnAlphabet + 4].Value = losscodeName;
                worksheet.Cells[columnAlphabet + 5].Value = "AF";

                LossesStartsATCol++;
                //Add the LossesToList
                LossesList.Add(new KeyValuePair<int, string>(losscode, losscodeName));
            }
            #endregion

            DateTime UsedDateForExcel = Convert.ToDateTime(frmDate);
            //For each Date ...... for all Machines.
            var Col = 'B';
            int Row = 5 + machin.Rows.Count + 2; // Gap to Insert OverAll data. DataStartRow + MachinesCount + 2(1 for HighestLevel & another for Gap).
            int Sno = 1;
            string finalLossCol = null;


            for (int i = 0; i < TotalDay + 1; i++)
            {
                int StartingRowForToday = Row;
                string dateforMachine = UsedDateForExcel.ToString("yyyy-MM-dd");

                int NumMacsToExcel = 0;
                for (int n = 0; n < machin.Rows.Count; n++)
                {
                    NumMacsToExcel++;
                    double CummulativeOfAllLosses = 0;
                    if (n == 0 && i != 0)
                    {
                        Row++;
                        StartingRowForToday = Row;
                    }

                    int MachineIDS = Convert.ToInt32(machin.Rows[n][0]);
                    List<string> HierarchyData = GetHierarchyData1(MachineIDS);

                    worksheet.Cells["B" + Row].Value = Sno++;
                    //worksheet.Cells["C" + Row].Value = UsedDateForExcel.ToString("yyyy-MM-dd");
                    worksheet.Cells["C" + Row].Value = HierarchyData[0];
                    worksheet.Cells["D" + Row].Value = HierarchyData[1];
                    worksheet.Cells["E" + Row].Value = HierarchyData[2];
                    worksheet.Cells["F" + Row].Value = HierarchyData[4];
                    // worksheet.Cells["G" + Row].Value = HierarchyData[3];

                    worksheet.Cells["G" + Row].Value = UsedDateForExcel.ToString("yyyy-MM-dd");
                    worksheet.Cells["H" + Row].Value = UsedDateForExcel.ToString("yyyy-MM-dd");
                    string CorrectedDateFormated = UsedDateForExcel.ToString("yyyy-MM-dd") + " 00:00:00";
                    DateTime StartDateFormatted = Convert.ToDateTime(UsedDateForExcel.ToString("yyyy-MM-dd") + " 00:00:00");

                    //Added this machineDetails into Datatable
                    string WCInvNoString = HierarchyData[3];
                    DataRow dr = DTConsolidatedLosses.AsEnumerable().FirstOrDefault(r => r.Field<string>("WCInvNo") == @WCInvNoString && r.Field<string>("CorrectedDate") == dateforMachine);
                    if (dr != null)
                    {
                        //do nothing
                    }
                    else
                    {
                        //plant, shop, cell, macINV, WcName, CorrectedDate, ValueAdding(Green/Operating), AvailableTime, SummationofSCTvsPP, Scrap,Rework,CuttingTime,DaysWorking, GodHours, TotalSTDHours, RejectionHours.
                        DTConsolidatedLosses.Rows.Add(HierarchyData[0], HierarchyData[1], HierarchyData[2], HierarchyData[3], HierarchyData[4], dateforMachine);
                    }

                    //Now get & put Losses
                    // Push Loss Value into  DataTable & Excel
                    string correctedDate = UsedDateForExcel.ToString("yyyy-MM-dd");

                    #region Capture and Push Losses
                    int column = 11 + LossCodesData.Rows.Count - 1; // StartCol in Excel + TotalLosses
                    finalLossCol = ExcelColumnFromNumber(column);

                    //now push 0 for every other loss into excel
                    worksheet.Cells["J" + Row + ":" + finalLossCol + Row].Value = Convert.ToDouble(0.0);


                    double MinorLoss = 0;
                    //string MLossString = Convert.ToString(Serverdb.tbloeedashboardvariables.Where(m => m.WCID == MachineIDS && m.StartDate == StartDateFormatted).Select(m => m.MinorLosses).FirstOrDefault());
                    //double.TryParse(MLossString, out MinorLoss);
                    int MachineIdleMinData = Convert.ToInt32(Serverdb.tblmachinedetails.Where(m => m.MachineID == MachineIDS && m.IsDeleted == 0).Select(m => m.MachineIdleMin).FirstOrDefault());
                    MachineIdleMinData = MachineIdleMinData * 60;
                    DateTime st = Convert.ToDateTime(startDateTime);
                    var MinorLossList = Serverdb.tblmodes.Where(m => m.IsDeleted == 0 && m.MachineID == MachineIDS && m.CorrectedDate == st && m.IsCompleted == 1 && m.DurationInSec < MachineIdleMinData && m.LossCodeID != null).Select(m => m.DurationInSec).ToList();
                    foreach (int LossMin in MinorLossList)
                    {
                        MinorLoss = MinorLoss + LossMin;
                    }
                    worksheet.Cells["J4"].Value = "MinorLoss";
                    worksheet.Cells["J" + Row].Value = Math.Round(MinorLoss / (60 * 60), 2);

                    //to Capture and Push , Losses that occured.
                    List<KeyValuePair<int, double>> LossesdurationList = GetAllLossesDurationSeconds(MachineIDS, correctedDate);
                    List<KeyValuePair<int, double>> BreakdowndurationList = GetAllBreakdownDurationSeconds(MachineIDS, correctedDate);
                    DataRow dr1 = DTConsolidatedLosses.AsEnumerable().FirstOrDefault(r => r.Field<string>("WCInvNo") == WCInvNoString && r.Field<string>("CorrectedDate") == @dateforMachine);
                    if (dr1 != null)
                    {
                        dr1["MinorLoss"] = Math.Round(MinorLoss / (60 * 60), 2);
                        CummulativeOfAllLosses += Math.Round(MinorLoss / (60 * 60), 2);

                        foreach (var loss in LossesdurationList)
                        {
                            int LossID = loss.Key;
                            double Duration = loss.Value;
                            var lossdata = Serverdb.tbllossescodes.Where(m => m.LossCodeID == LossID).FirstOrDefault();
                            int level = lossdata.LossCodesLevel;
                            string losscodeName = null;

                            #region To Get LossCode Hierarchy
                            if (level == 3)
                            {
                                int lossLevel1ID = Convert.ToInt32(lossdata.LossCodesLevel1ID);
                                int lossLevel2ID = Convert.ToInt32(lossdata.LossCodesLevel2ID);
                                var lossdata1 = Serverdb.tbllossescodes.Where(m => m.LossCodeID == lossLevel1ID).FirstOrDefault();
                                var lossdata2 = Serverdb.tbllossescodes.Where(m => m.LossCodeID == lossLevel2ID).FirstOrDefault();
                                losscodeName = lossdata1.LossCodeDesc + " :: " + lossdata2.LossCodeDesc + " : " + lossdata.LossCodeDesc;
                            }
                            else if (level == 2)
                            {
                                int lossLevel1ID = Convert.ToInt32(lossdata.LossCodesLevel1ID);
                                var lossdata1 = Serverdb.tbllossescodes.Where(m => m.LossCodeID == lossLevel1ID).FirstOrDefault();

                                losscodeName = lossdata1.LossCodeDesc + ":" + lossdata.LossCodeDesc;
                            }
                            else if (level == 1)
                            {
                                if (LossID == 999)
                                {
                                    losscodeName = "NoCode Entered";
                                }
                                else
                                {
                                    losscodeName = lossdata.LossCodeDesc;
                                }
                            }
                            #endregion

                            int ColumnIndex = DTConsolidatedLosses.Columns[losscodeName].Ordinal;
                            string ColumnForThisLoss = ExcelColumnFromNumber(ColumnIndex + 4);// 5 is the Difference between position of Excel and DataTable Structure  for Losses Inserting column.
                            double DurInHours = Convert.ToDouble(Math.Round((Duration / (60 * 60)), 1)); //To Hours:: 1 Decimal Place
                            worksheet.Cells[ColumnForThisLoss + "" + Row].Value = DurInHours;
                            dr1[losscodeName] = DurInHours;
                            CummulativeOfAllLosses += DurInHours;
                        }

                        foreach (var Breakdown in BreakdowndurationList)
                        {
                            int LossID = Breakdown.Key;
                            double Duration = Breakdown.Value;
                            var lossdata = Serverdb.tbllossescodes.Where(m => m.LossCodeID == LossID).FirstOrDefault();
                            int level = lossdata.LossCodesLevel;
                            string losscodeName = null;

                            #region To Get LossCode Hierarchy
                            if (level == 3)
                            {
                                int lossLevel1ID = Convert.ToInt32(lossdata.LossCodesLevel1ID);
                                int lossLevel2ID = Convert.ToInt32(lossdata.LossCodesLevel2ID);
                                var lossdata1 = Serverdb.tbllossescodes.Where(m => m.LossCodeID == lossLevel1ID).FirstOrDefault();
                                var lossdata2 = Serverdb.tbllossescodes.Where(m => m.LossCodeID == lossLevel2ID).FirstOrDefault();
                                losscodeName = lossdata1.LossCodeDesc + " :: " + lossdata2.LossCodeDesc + " : " + lossdata.LossCodeDesc;
                            }
                            else if (level == 2)
                            {
                                int lossLevel1ID = Convert.ToInt32(lossdata.LossCodesLevel1ID);
                                var lossdata1 = Serverdb.tbllossescodes.Where(m => m.LossCodeID == lossLevel1ID).FirstOrDefault();

                                losscodeName = lossdata1.LossCodeDesc + ":" + lossdata.LossCodeDesc;
                            }
                            else if (level == 1)
                            {
                                if (LossID == 999)
                                {
                                    losscodeName = "NoCode Entered";
                                }
                                else
                                {
                                    losscodeName = lossdata.LossCodeDesc;
                                }
                            }
                            #endregion

                            int ColumnIndex = DTConsolidatedLosses.Columns[losscodeName].Ordinal;

                            string ColumnForThisLoss = ExcelColumnFromNumber(ColumnIndex + 4);// 5 is the Difference between position of Excel and DataTable Structure  for Losses Inserting column.
                            double DurInHours = Convert.ToDouble(Math.Round((Duration / (60 * 60)), 1)); //To Hours:: 1 Decimal Place
                            worksheet.Cells[ColumnForThisLoss + "" + Row].Value = DurInHours;
                            dr1[losscodeName] = DurInHours;
                            CummulativeOfAllLosses += DurInHours;
                        }
                    }
                    #endregion

                    worksheet.Cells["I" + Row].Value = Convert.ToDouble(Math.Round((CummulativeOfAllLosses), 1));
                    Row++;

                }//End of For Each Machine Loop

                //Stuff for entire day (of all WC's) Into DT
                DTConsolidatedLosses.Rows.Add("Summarized", "Summarized", "Summarized", "Summarized", "Summarized", dateforMachine);

                //Push each Date Cummulative. Loop through ExcelAddress and insert formula
                if (finalLossCol != null)
                {
                    var rangeIndividualSummarized = worksheet.Cells["J4:" + finalLossCol + "4"];
                    //rangeIndividualSummarized = worksheet.Cells["K4:CW4"];
                    foreach (var rangeBase in rangeIndividualSummarized)
                    {
                        string str = Convert.ToString(rangeBase);
                        string ExcelColAlphabet = Regex.Replace(str, "[^A-Z _]", "");
                        worksheet.Cells[ExcelColAlphabet + Row].Formula = "=SUM(" + ExcelColAlphabet + StartingRowForToday + ":" + ExcelColAlphabet + "" + (Row - 1) + ")";
                        //var a = worksheet.Cells[rangeBase.Address].Value;
                        var blah1 = worksheet.Calculate("=SUM(" + ExcelColAlphabet + StartingRowForToday + ":" + ExcelColAlphabet + "" + (Row - 1) + ")");

                        double LossVal = 0;
                        double.TryParse(Convert.ToString(blah1), out LossVal);
                        if (LossVal != 0.0)
                        {
                            string LossName = Convert.ToString(worksheet.Cells[ExcelColAlphabet + 4].Value);
                            DataRow dr = DTConsolidatedLosses.AsEnumerable().LastOrDefault(r => r.Field<string>("Plant") == "Summarized" && r.Field<string>("CorrectedDate") == dateforMachine);
                            if (dr != null)
                            {
                                if (!string.IsNullOrEmpty(LossName))
                                    dr[LossName] = LossVal;
                            }
                        }
                    }
                }

                //Total of Today into 
                //Insert Cummulative for today
                worksheet.Cells["C" + Row + ":G" + Row].Merge = true;
                worksheet.Cells["C" + Row].Value = "Summarized For";
                worksheet.Cells["G" + Row].Value = worksheet.Cells["H" + (Row - 1)].Value;
                worksheet.Cells["B" + Row + ":" + finalLossCol + Row].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
                worksheet.Cells["B" + Row + ":" + finalLossCol + Row].Style.Font.Bold = true;
                worksheet.Cells["I" + Row].Formula = "=SUM(I" + StartingRowForToday + ":I" + (Row - 1) + ")";

                //Cellwise Border for Today
                worksheet.Cells["B" + StartingRowForToday + ":" + finalLossCol + "" + Row].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells["B" + StartingRowForToday + ":" + finalLossCol + "" + Row].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells["B" + StartingRowForToday + ":" + finalLossCol + "" + Row].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Cells["B" + StartingRowForToday + ":" + finalLossCol + "" + Row].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                //Excel:: Border Around Cells.
                worksheet.Cells["B" + StartingRowForToday + ":" + finalLossCol + "" + Row].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);
                worksheet.Cells["B" + Row + ":" + finalLossCol + "" + Row].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Medium);

                UsedDateForExcel = UsedDateForExcel.AddDays(+1);
                Row++;

            } //End of All day's Loop

            #region OverAll Losses and Stuff
            Row = 5;
            Sno = 1;
            var WCInvNoList = (from DataRow row in DTConsolidatedLosses.Rows
                               where row["WCInvNo"] != "Summarized"
                               select row["WCInvNo"]).Distinct();

            foreach (var MacINV in WCInvNoList)
            {
                string WCInvNoStringOverAll = Convert.ToString(MacINV);
                int machineid = Convert.ToInt32(MacINV);
                DataRow drOverAll = DTConsolidatedLosses.AsEnumerable().FirstOrDefault(r => r.Field<string>("WCInvNo") == @WCInvNoStringOverAll);

                if (drOverAll != null)
                {
                    int MachineIDS = Serverdb.tblmachinedetails.Where(m => m.MachineID == machineid).Select(m => m.MachineID).FirstOrDefault();
                    string macDispName = Serverdb.tblmachinedetails.Where(m => m.MachineID == machineid).Select(m => m.MachineDisplayName).FirstOrDefault();
                    List<string> HierarchyData = GetHierarchyData1(MachineIDS);
                    worksheet.Cells["B" + Row].Value = Sno++;
                    worksheet.Cells["C" + Row].Value = HierarchyData[0];
                    worksheet.Cells["D" + Row].Value = HierarchyData[1];
                    worksheet.Cells["E" + Row].Value = HierarchyData[2];
                    worksheet.Cells["F" + Row].Value = macDispName;
                    //worksheet.Cells["G" + Row].Value = HierarchyData[3];

                    worksheet.Cells["G" + Row].Value = (frmDate).ToString("yyyy-MM-dd");
                    worksheet.Cells["H" + Row].Value = (toDate).ToString("yyyy-MM-dd");

                    //Total of Losses
                    worksheet.Cells["I" + Row].Formula = "=SUM(J" + Row + ":" + finalLossCol + "" + Row + ")";

                    //OverAll Losses 
                    var range = worksheet.Cells["J4:" + finalLossCol + "" + 4];
                    int i = 10;

                    foreach (var rangeBase in range)
                    {
                        double LossValToExcel = 0;
                        string LossNameVal = Convert.ToString(rangeBase.Value);
                        if (LossNameVal == "MinorLoss")
                        {
                        }
                        string LossName = Convert.ToString(worksheet.Cells[rangeBase.Address].Value);
                        if (!string.IsNullOrEmpty(LossNameVal))
                        {
                            LossValToExcel = DTConsolidatedLosses.AsEnumerable().Where(x => x.Field<string>("WCInvNo") == @WCInvNoStringOverAll).Sum(x => x.Field<double>(@LossNameVal));
                            string ColumnForThisLoss = ExcelColumnFromNumber(i++);
                            worksheet.Cells[ColumnForThisLoss + "" + Row].Value = Math.Round(LossValToExcel, 2);
                        }
                        else
                        {
                        }

                    }
                } //End of if(drOverAll != null)
                Row++;
            }

            // Borders and Stuff for Cummulative Data.
            //Cellwise Border for Today
            List<KeyValuePair<string, double>> AllOccuredLosses = new List<KeyValuePair<string, double>>();
            int j = 11;
            double IdentifiedLoss = 0;
            double UnIdentifiedLoss = 0;
            if (finalLossCol != null)
            {
                worksheet.Cells["B4:" + finalLossCol + "" + Row].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells["B4:" + finalLossCol + "" + Row].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells["B4:" + finalLossCol + "" + Row].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Cells["B4:" + finalLossCol + "" + Row].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                //Excel:: Border Around Cells.
                worksheet.Cells["B5:" + finalLossCol + "" + (Row)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);
                worksheet.Cells["B" + Row + ":" + finalLossCol + "" + Row].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);


                //Total of Losses OverAll, Summarized of Total per Day
                worksheet.Cells["I" + Row].Formula = "=SUM(I5:I" + (Row - 1) + ")";

                //Cummulative Losses into DT and Occured Losses into List and Identified and UnIdentified Losses
                var rangeFinalLosses = worksheet.Cells["J4:" + finalLossCol + "4"];



                foreach (var rangeBase in rangeFinalLosses)
                {
                    string LossName = Convert.ToString(rangeBase.Value);
                    string LossNameAddress = Convert.ToString(rangeBase.Address);
                    if (LossName == "MinorLoss")
                    {
                        double thisLossValue = DTConsolidatedLosses.AsEnumerable().Where(x => x.Field<string>("Plant") != "Summarized").Sum(x => x.Field<double>(@LossName));
                        string ColumnForThisLoss = ExcelColumnFromNumber(--j);
                        worksheet.Cells[ColumnForThisLoss + "" + Row].Formula = "=SUM(" + ColumnForThisLoss + 6 + ":" + ColumnForThisLoss + "" + (Row) + ")";
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(LossName))
                        {
                            double thisLossValue = DTConsolidatedLosses.AsEnumerable().Where(x => x.Field<string>("Plant") != "Summarized").Sum(x => x.Field<double>(@LossName));
                            string ColumnForThisLoss = ExcelColumnFromNumber(j++);
                            worksheet.Cells[ColumnForThisLoss + "" + Row].Formula = "=SUM(" + ColumnForThisLoss + 5 + ":" + ColumnForThisLoss + "" + (Row - 1) + ")";
                            if (thisLossValue > 0)
                            {
                                if (LossName == "NoCode Entered" || LossName == "Unidentified Breakdown")
                                {
                                    UnIdentifiedLoss += thisLossValue;
                                }
                                else
                                {
                                    IdentifiedLoss += thisLossValue;
                                }
                                AllOccuredLosses.Add(new KeyValuePair<string, double>(LossNameAddress, Math.Round(thisLossValue, 1)));
                            }
                        }
                    }
                }
            }
            #endregion

            #region GRAPHS
            //Create the chart
            List<double> ForAvg = new List<double>();
            if (finalLossCol != null)
            {
                if (machin.Rows.Count > 0)
                {

                    #region lOSSES TOP 5 GRAPH
                    //1. Get Top 5 Losses ColName in excel.
                    //2. Generate the comma seperated String format .
                    //sort the list
                    AllOccuredLosses.Sort(Compare2);
                    AllOccuredLosses = AllOccuredLosses.OrderByDescending(x => x.Value).ToList();

                    #region Percentage Data into Graph sheet.

                    var SumOfAllLosses = AllOccuredLosses.Sum(x => x.Value);
                    j = 3;
                    int CellRow = 5;
                    foreach (var item in AllOccuredLosses)
                    {
                        string LossNameCell = item.Key;
                        double LossValue = item.Value;
                        string ColumnForThisLoss = ExcelColumnFromNumber(j++);
                        worksheetGraph.Cells[ColumnForThisLoss + CellRow].Value = worksheet.Cells[LossNameCell].Value;
                        worksheetGraph.Cells[ColumnForThisLoss + (CellRow + 1)].Value = LossValue;
                        double InPercentage = Math.Round(((LossValue / SumOfAllLosses) * 100), 2);
                        worksheetGraph.Cells[ColumnForThisLoss + (CellRow + 2)].Value = InPercentage + "%";
                    }

                    //Cellwise Border
                    string ColumnForEndOfLoss = ExcelColumnFromNumber(AllOccuredLosses.Count + 2);
                    worksheetGraph.Cells["B5:" + ColumnForEndOfLoss + "7"].Style.Border.Top.Style = ExcelBorderStyle.Medium;
                    worksheetGraph.Cells["B5:" + ColumnForEndOfLoss + "7"].Style.Border.Left.Style = ExcelBorderStyle.Medium;
                    worksheetGraph.Cells["B5:" + ColumnForEndOfLoss + "7"].Style.Border.Right.Style = ExcelBorderStyle.Medium;
                    worksheetGraph.Cells["B5:" + ColumnForEndOfLoss + "7"].Style.Border.Bottom.Style = ExcelBorderStyle.Medium;

                    #endregion

                    //Now construct string from top 5 losses.

                    int LooperTop5 = 0;
                    string CellsOfTop5LossColNames = null;
                    string CellsOfTop5LossColValues = null;
                    bool isNoCodeATopper = false;
                    double NonOthersTotalInHours = 0;
                    foreach (KeyValuePair<string, double> loss in AllOccuredLosses)
                    {
                        if (LooperTop5 < 5)
                        {
                            string a = loss.Key;
                            double b = loss.Value;
                            var outputJustColName = Regex.Replace(a, @"[\d-]", string.Empty);
                            //string LossCol = Convert.ToString(outputJustColName);
                            string LossCol = a;
                            string lossName = Convert.ToString(worksheet.Cells[a].Value);
                            if (lossName != "NoCode Entered")
                            {
                                if (LooperTop5 == 0)
                                {
                                    CellsOfTop5LossColNames = LossCol;
                                    CellsOfTop5LossColValues = outputJustColName + Row;
                                }
                                else
                                {
                                    CellsOfTop5LossColNames += "," + LossCol;
                                    CellsOfTop5LossColValues += "," + outputJustColName + Row;
                                }
                                NonOthersTotalInHours += b;
                                LooperTop5++;
                            }
                            else
                            {
                                isNoCodeATopper = true;
                                NonOthersTotalInHours += b;
                            }
                        }
                        else
                        {
                            break;
                        }

                    }

                    //Calculate Others Time && if necessary remove NoCode Time
                    double OthersTotalInHours = 0;
                    if (isNoCodeATopper)
                    {
                        OthersTotalInHours = SumOfAllLosses - NonOthersTotalInHours;
                    }
                    else
                    {
                        foreach (KeyValuePair<string, double> loss in AllOccuredLosses)
                        {
                            string a = loss.Key;
                            string LossCol = a;
                            double b = loss.Value;
                            string lossName = Convert.ToString(worksheet.Cells[a].Value);
                            if (lossName == "NoCode Entered")
                            {
                                OthersTotalInHours = SumOfAllLosses - NonOthersTotalInHours - b;
                            }
                        }
                    }

                    //Now Append "Others" to graph
                    int column = 20 + LossCodesData.Rows.Count; // StartCol in Excel + TotalLosses
                    finalLossCol = ExcelColumnFromNumber(column);

                    worksheet.Cells[finalLossCol + "1"].Value = "Others";
                    worksheet.Cells[finalLossCol + "2"].Value = Math.Round(OthersTotalInHours, 1);

                    if (CellsOfTop5LossColNames == null)
                    {
                        var akl = worksheetGraph.Cells[10, 3].Address;
                        CellsOfTop5LossColNames = worksheet.Cells[finalLossCol + "1"].Address;
                        CellsOfTop5LossColValues = worksheet.Cells[finalLossCol + "2"].Address;
                    }
                    else
                    {
                        var akl = worksheet.Cells[finalLossCol + "1"].Address;
                        CellsOfTop5LossColNames += "," + worksheet.Cells[finalLossCol + "1"].Address;
                        CellsOfTop5LossColValues += "," + worksheet.Cells[finalLossCol + "2"].Address;
                    }

                    ExcelChart chartTop5Losses1 = (ExcelBarChart)worksheetGraph.Drawings.AddChart("barChartTop5Losses", eChartType.ColumnClustered);
                    var chartTop5Losses = (ExcelBarChart)chartTop5Losses1;
                    chartTop5Losses.SetSize(500, 400); //Width,Height
                    chartTop5Losses.SetPosition(140, 10); //PixelTop,Pixelleft
                    //string blah = "CY11,CZ11,DA11,DC11,DD11"; //This Works 
                    //ExcelRange erLossesRangechartTop5LossesvALUE = worksheet.Cells[blah];
                    //ExcelRange erLossesRangechartTop5LossesvALUE = worksheet.Cells["CY11,CZ11,DA11,DC11,DD11"];
                    //ExcelRange erLossesRangechartTop5LossesNAMES = worksheet.Cells["CY3,CZ3,DA3,DC3,DD3"];

                    ExcelRange erLossesRangechartTop5LossesvALUE = worksheet.Cells[CellsOfTop5LossColValues];
                    ExcelRange erLossesRangechartTop5LossesNAMES = worksheet.Cells[CellsOfTop5LossColNames];
                    chartTop5Losses.Title.Text = "LOSSES  ";
                    chartTop5Losses.Style = eChartStyle.Style19;
                    chartTop5Losses.Legend.Remove();
                    chartTop5Losses.DataLabel.ShowValue = true;
                    //chartTop5Losses.DataLabel.Font.Size = 8;
                    //chartTop5Losses.Legend.Font.Size = 8;
                    chartTop5Losses.XAxis.Font.Size = 8;
                    chartTop5Losses.YAxis.MinorTickMark = eAxisTickMark.None;
                    chartTop5Losses.XAxis.MajorTickMark = eAxisTickMark.None;
                    chartTop5Losses.Series.Add(erLossesRangechartTop5LossesvALUE, erLossesRangechartTop5LossesNAMES);
                    RemoveGridLines(ref chartTop5Losses1);

                    #endregion

                    #region Identified & UnIdentified Losses "scary"
                    worksheetGraph.Cells["ZL1"].Value = "Ratio of Losses";
                    //worksheetGraph.Cells["A2"].Value = "UnIdentifiedLoss";

                    double IdentifiedLossPercentage = (IdentifiedLoss / (IdentifiedLoss + UnIdentifiedLoss)) * 100;
                    double UnIdentifiedLossPercentage = (UnIdentifiedLoss / (IdentifiedLoss + UnIdentifiedLoss)) * 100;
                    worksheetGraph.Cells["ZM1"].Value = Math.Round(IdentifiedLossPercentage, 0);
                    worksheetGraph.Cells["ZM2"].Value = Math.Round(UnIdentifiedLossPercentage, 0);

                    erLossesRangechartTop5LossesvALUE = worksheetGraph.Cells["ZM1"];
                    ExcelRange erLossesRangechartTop5LossesvALUE1 = worksheetGraph.Cells["ZM2"];
                    erLossesRangechartTop5LossesNAMES = worksheetGraph.Cells["ZL1"];

                    ExcelChart chartIDAndUnID1 = (ExcelBarChart)worksheetGraph.Drawings.AddChart("TypesOfLosses", eChartType.ColumnStacked);
                    var chartIDAndUnID = (ExcelBarChart)chartIDAndUnID1;
                    chartIDAndUnID.SetSize(500, 400);
                    chartIDAndUnID.SetPosition(140, 520);

                    chartIDAndUnID.Title.Text = "Identified Losses  ";
                    chartIDAndUnID.Style = eChartStyle.Style18;
                    chartIDAndUnID.Legend.Position = eLegendPosition.Bottom;
                    //chartIDAndUnID.Legend.Remove();
                    chartIDAndUnID.YAxis.MaxValue = 100;
                    chartIDAndUnID.YAxis.MinValue = 0;
                    chartIDAndUnID.Locked = false;
                    chartIDAndUnID.PlotArea.Border.Width = 0;
                    chartIDAndUnID.YAxis.MinorTickMark = eAxisTickMark.None;
                    chartIDAndUnID.DataLabel.ShowValue = true;
                    //chartAllLosses.DataLabel.ShowValue = true;
                    var thisYearSeries = (ExcelChartSerie)(chartIDAndUnID.Series.Add(erLossesRangechartTop5LossesvALUE, erLossesRangechartTop5LossesNAMES));
                    thisYearSeries.Header = "Identified Losses";
                    var lastYearSeries = (ExcelChartSerie)(chartIDAndUnID.Series.Add(erLossesRangechartTop5LossesvALUE1, erLossesRangechartTop5LossesNAMES));
                    lastYearSeries.Header = "UnIdentified Losses";
                    RemoveGridLines(ref chartIDAndUnID1);

                    #region OLD
                    ////////////////////
                    //have to remove cat nodes from each series so excel autonums 1 and 2 in xaxis
                    //var chartXml = chartIDAndUnID.ChartXml;
                    //var nsm = new XmlNamespaceManager(chartXml.NameTable);

                    //var nsuri = chartXml.DocumentElement.NamespaceURI;
                    //nsm.AddNamespace("c", nsuri);

                    ////Get the Series ref and its cat
                    //var serNodes = chartXml.SelectNodes("c:chartSpace/c:chart/c:plotArea/c:bar3DChart/c:ser", nsm);
                    //foreach (XmlNode serNode in serNodes)
                    //{
                    //    //Cell any cell reference and replace it with a string literal list
                    //    var catNode = serNode.SelectSingleNode("c:cat", nsm);
                    //    catNode.RemoveAll();

                    //    //Create the string list elements
                    //    var ptCountNode = chartXml.CreateElement("c:ptCount", nsuri);
                    //    ptCountNode.Attributes.Append(chartXml.CreateAttribute("val", nsuri));
                    //    ptCountNode.Attributes[0].Value = "2";

                    //    var v0Node = chartXml.CreateElement("c:v", nsuri);
                    //    v0Node.InnerText = "opening";
                    //    var pt0Node = chartXml.CreateElement("c:pt", nsuri);
                    //    pt0Node.AppendChild(v0Node);
                    //    pt0Node.Attributes.Append(chartXml.CreateAttribute("idx", nsuri));
                    //    pt0Node.Attributes[0].Value = "0";

                    //    var v1Node = chartXml.CreateElement("c:v", nsuri);
                    //    v1Node.InnerText = "closing";
                    //    var pt1Node = chartXml.CreateElement("c:pt", nsuri);
                    //    pt1Node.AppendChild(v1Node);
                    //    pt1Node.Attributes.Append(chartXml.CreateAttribute("idx", nsuri));
                    //    pt1Node.Attributes[0].Value = "1";

                    //    //Create the string list node
                    //    var strLitNode = chartXml.CreateElement("c:strLit", nsuri);
                    //    strLitNode.AppendChild(ptCountNode);
                    //    strLitNode.AppendChild(pt0Node);
                    //    strLitNode.AppendChild(pt1Node);
                    //    catNode.AppendChild(strLitNode);
                    //}
                    //pck.Save();
                    #endregion
                    #region Experiment to Send Data to Excel Chart in Template
                    //OfficeOpenXml.FormulaParsing.Excel.Application xlApp;
                    //Excel.Workbook xlWorkBook;
                    //Excel.Worksheet xlWorkSheet;
                    //object misValue = System.Reflection.Missing.Value;

                    //Excel.ChartObjects xlCharts = (Excel.ChartObjects)xlWorkSheet.ChartObjects(Type.Missing);
                    //Excel.ChartObject myChart = (Excel.ChartObject)xlCharts.Add(10, 80, 300, 250);
                    //Excel.Chart chartPage = myChart.Chart;

                    //Excel.Range chartRange;
                    //chartRange = xlWorkSheet.get_Range("A1", "d5");
                    //chartPage.SetSourceData(chartRange, misValue);
                    //chartPage.ChartType = Excel.XlChartType.xlColumnClustered;

                    //xlWorkBook.SaveAs("csharp.net-informations.xls", Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
                    //xlWorkBook.Close(true, misValue, misValue);
                    //xlApp.Quit();

                    //var piechart = worksheet.Drawings["A"] as ExcelBarChart;
                    //piechart.Style = eChartStyle.Style26;
                    //chartIDAndUnID.Style = eChartStyle.Style26;

                    //////////////////////////////


                    #endregion

                    #endregion End of Identified & UnIdentified Losses

                    #region All Losses Chart

                    CellsOfTop5LossColNames = null;
                    CellsOfTop5LossColValues = null;
                    int Looper = 0;
                    foreach (KeyValuePair<string, double> loss in AllOccuredLosses)
                    {
                        string a = loss.Key;
                        double b = loss.Value;

                        var outputJustColName = Regex.Replace(a, @"[\d-]", string.Empty);
                        //string LossCol = Convert.ToString(outputJustColName);
                        string LossCol = a;
                        if (a == "0")
                        {
                        }
                        if (Looper == 0)
                        {
                            CellsOfTop5LossColNames = LossCol;
                            CellsOfTop5LossColValues = outputJustColName + Row;
                        }
                        else
                        {
                            CellsOfTop5LossColNames += "," + LossCol;
                            CellsOfTop5LossColValues += "," + outputJustColName + Row;
                        }
                        Looper++;
                    }

                    if (CellsOfTop5LossColValues != null && CellsOfTop5LossColNames != null)
                    {
                        ExcelChart chartAllLosses1 = (ExcelBarChart)worksheetGraph.Drawings.AddChart("barChartAllLosses", eChartType.ColumnClustered);
                        var chartAllLosses = (ExcelBarChart)chartAllLosses1;
                        chartAllLosses.SetSize(1200, 500);
                        chartAllLosses.SetPosition(550, 10);
                        erLossesRangechartTop5LossesvALUE = worksheet.Cells[CellsOfTop5LossColValues];
                        erLossesRangechartTop5LossesNAMES = worksheet.Cells[CellsOfTop5LossColNames];
                        chartAllLosses.Title.Text = "All LOSSES ";
                        chartAllLosses.Style = eChartStyle.Style25;
                        chartAllLosses.Legend.Remove();
                        chartAllLosses.DataLabel.ShowValue = true;
                        //chartAllLosses.DataLabel.Font.Size = 8;
                        //chartAllLosses.Legend.Font.Size = 8;
                        chartAllLosses.YAxis.MinorTickMark = eAxisTickMark.None;
                        chartAllLosses.XAxis.MajorTickMark = eAxisTickMark.None;
                        chartAllLosses.XAxis.Font.Size = 8;
                        chartAllLosses.Series.Add(erLossesRangechartTop5LossesvALUE, erLossesRangechartTop5LossesNAMES);

                        //Get reference of Graph to Remove GridLines
                        RemoveGridLines(ref chartAllLosses1);
                    }

                    #endregion

                    #region  Losses Trend :: All 5 Topper's
                    var queryLinq = (from cell in worksheet.Cells["C:C"]
                                     where cell.Value is string && (string)cell.Value == "Summarized For"
                                     select cell);
                    int LossesLooper = 1;
                    int PositionY = 1060;
                    int PositionX = 10;



                    int GraphNo = 0, maxYVal = 0;
                    foreach (var Loss in AllOccuredLosses)
                    {
                        maxYVal = Convert.ToInt32(Loss.Value);
                        break;
                    }
                    foreach (var Loss in AllOccuredLosses)
                    {
                        if (LossesLooper <= 5)
                        {
                            ForAvg.Clear();
                            string a = Loss.Key;
                            double b = Loss.Value;
                            var outputJustColName = Regex.Replace(a, @"[\d-]", string.Empty);
                            //string LossCol = Convert.ToString(outputJustColName);
                            string LossCol = a;
                            string lossName = Convert.ToString(worksheet.Cells[a].Value);
                            if (lossName != "NoCode Entered")
                            {
                                string CellsOfOEEYAxis = null;
                                string CellsOfOEEXAxis = null;
                                string CellColString = Convert.ToString(Loss.Key);
                                string LossName = Convert.ToString(worksheet.Cells[CellColString].Value);
                                foreach (var cell in queryLinq)
                                {
                                    string CellRowString = cell.Address;
                                    string RowNum = Regex.Replace(CellRowString, "[^0-9 _]", string.Empty);
                                    string ColName = Regex.Replace(CellColString, "[0-9 _]", string.Empty);

                                    if (CellsOfOEEXAxis == null)
                                    {
                                        CellsOfOEEXAxis = "H" + (Convert.ToInt32(RowNum) - 1);
                                    }
                                    else
                                    {
                                        CellsOfOEEXAxis += ",H" + (Convert.ToInt32(RowNum) - 1);
                                    }

                                    if (CellsOfOEEYAxis == null)
                                    {
                                        string colPlusrow = ColName + RowNum;
                                        CellsOfOEEYAxis = colPlusrow;
                                        double CellVal = 0;
                                        string CellValString = worksheet.Calculate(worksheet.Cells[colPlusrow].Formula).ToString();
                                        if (double.TryParse(CellValString, out CellVal))
                                        {
                                            ForAvg.Add(CellVal);
                                        }
                                    }
                                    else
                                    {
                                        string colPlusrow = ColName + RowNum;
                                        CellsOfOEEYAxis += "," + colPlusrow;
                                        double CellVal = 0;

                                        string CellValString = worksheet.Calculate(worksheet.Cells[colPlusrow].Formula).ToString();
                                        if (double.TryParse(CellValString, out CellVal))
                                        {
                                            ForAvg.Add(CellVal);
                                        }
                                    }
                                }

                                ExcelRange erTopLossesTrendYValues = worksheet.Cells[CellsOfOEEYAxis];
                                ExcelRange erTopLossesTrendXNames = worksheet.Cells[CellsOfOEEXAxis];
                                ExcelChart chartLossses1Trend1 = (ExcelLineChart)worksheetGraph.Drawings.AddChart("TrendChartTop" + LossesLooper, eChartType.LineMarkers);

                                GraphNo++;
                                if (GraphNo == 1)
                                {
                                    for (int i = 0; i < ForAvg.Count; i++)
                                    {
                                        worksheetGraph.Cells["ZA" + (50 + i)].Value = "Avg";
                                        worksheetGraph.Cells["ZB" + (50 + i)].Value = ForAvg.Sum() / ForAvg.Count;
                                    }
                                    var chartTypeL1 = chartLossses1Trend1.PlotArea.ChartTypes.Add(eChartType.Line);
                                    var serieL2 = chartTypeL1.Series.Add(worksheetGraph.Cells["ZB50:ZB" + (50 + ForAvg.Count - 1)], worksheetGraph.Cells["A50:A" + (50 + ForAvg.Count - 1)]);
                                }
                                if (GraphNo == 2)
                                {
                                    for (int i = 0; i < ForAvg.Count; i++)
                                    {
                                        worksheetGraph.Cells["ZC" + (50 + i)].Value = "Avg";
                                        worksheetGraph.Cells["ZD" + (50 + i)].Value = ForAvg.Sum() / ForAvg.Count;
                                    }
                                    var chartTypeL1 = chartLossses1Trend1.PlotArea.ChartTypes.Add(eChartType.Line);
                                    var serieL2 = chartTypeL1.Series.Add(worksheetGraph.Cells["ZD50:ZD" + (50 + ForAvg.Count - 1)], worksheetGraph.Cells["C50:C" + (50 + ForAvg.Count - 1)]);

                                }
                                if (GraphNo == 3)
                                {
                                    for (int i = 0; i < ForAvg.Count; i++)
                                    {
                                        worksheetGraph.Cells["ZE" + (50 + i)].Value = "Avg";
                                        worksheetGraph.Cells["ZF" + (50 + i)].Value = ForAvg.Sum() / ForAvg.Count;
                                    }
                                    var chartTypeL1 = chartLossses1Trend1.PlotArea.ChartTypes.Add(eChartType.Line);
                                    var serieL2 = chartTypeL1.Series.Add(worksheetGraph.Cells["ZF50:ZF" + (50 + ForAvg.Count - 1)], worksheetGraph.Cells["E50:E" + (50 + ForAvg.Count - 1)]);

                                }
                                if (GraphNo == 4)
                                {
                                    for (int i = 0; i < ForAvg.Count; i++)
                                    {
                                        worksheetGraph.Cells["ZG" + (50 + i)].Value = "Avg";
                                        worksheetGraph.Cells["ZH" + (50 + i)].Value = ForAvg.Sum() / ForAvg.Count;
                                    }
                                    var chartTypeL1 = chartLossses1Trend1.PlotArea.ChartTypes.Add(eChartType.Line);
                                    var serieL2 = chartTypeL1.Series.Add(worksheetGraph.Cells["ZH50:ZH" + (50 + ForAvg.Count - 1)], worksheetGraph.Cells["G50:G" + (50 + ForAvg.Count - 1)]);

                                }
                                if (GraphNo == 5)
                                {
                                    for (int i = 0; i < ForAvg.Count; i++)
                                    {
                                        worksheetGraph.Cells["ZI" + (50 + i)].Value = "Avg";
                                        worksheetGraph.Cells["ZJ" + (50 + i)].Value = ForAvg.Sum() / ForAvg.Count;
                                    }
                                    var chartTypeL1 = chartLossses1Trend1.PlotArea.ChartTypes.Add(eChartType.Line);
                                    var serieL2 = chartTypeL1.Series.Add(worksheetGraph.Cells["ZJ50:ZJ" + (50 + ForAvg.Count - 1)], worksheetGraph.Cells["I50:I" + (50 + ForAvg.Count - 1)]);

                                }

                                if (erTopLossesTrendXNames != null && erTopLossesTrendYValues != null)
                                {
                                    var chartLossses1Trend = (ExcelLineChart)chartLossses1Trend1;
                                    chartLossses1Trend.SetSize(300, 300);
                                    chartLossses1Trend.SetPosition(PositionY, 10 + (((LossesLooper - 1)) * 300));
                                    //chartLossses1Trend.Title.Text = "Top" + LossesLooper + " Trend Chart ";
                                    chartLossses1Trend.Title.Text = LossName;
                                    chartLossses1Trend.Style = eChartStyle.Style8;
                                    chartLossses1Trend.Legend.Remove();
                                    chartLossses1Trend.YAxis.MinValue = 0;
                                    chartLossses1Trend.YAxis.MaxValue = maxYVal + 10;
                                    chartLossses1Trend.DataLabel.ShowValue = true;
                                    //chartLossses1Trend.DataLabel.Font.Size = 6.0F;
                                    chartLossses1Trend.PlotArea.Border.Width = 0;
                                    chartLossses1Trend.YAxis.MinorTickMark = eAxisTickMark.None;
                                    //chartLossses1Trend.YAxis.MajorTickMark = eAxisTickMark.None;
                                    //chartLossses1Trend.XAxis.MinorTickMark = eAxisTickMark.None;
                                    chartLossses1Trend.XAxis.MajorTickMark = eAxisTickMark.None;
                                    //chartLossses1Trend.XAxis.MinorTickMark = eAxisTickMark.None;
                                    chartLossses1Trend.Series.Add(erTopLossesTrendYValues, erTopLossesTrendXNames);

                                    //Get reference of Graph to Remove GridLines
                                    RemoveGridLines(ref chartLossses1Trend1);
                                }
                                #region OLD
                                //chartXml = chartLossses1Trend.ChartXml;
                                // nsuri = chartXml.DocumentElement.NamespaceURI;
                                // nsm = new XmlNamespaceManager(chartXml.NameTable);
                                //nsm.AddNamespace("c", nsuri);

                                ////XY Scatter plots have 2 value axis and no category
                                //valAxisNodes = chartXml.SelectNodes("c:chartSpace/c:chart/c:plotArea/c:valAx", nsm);
                                //if (valAxisNodes != null && valAxisNodes.Count > 0)
                                //    foreach (XmlNode valAxisNode in valAxisNodes)
                                //    {
                                //        var major = valAxisNode.SelectSingleNode("c:majorGridlines", nsm);
                                //        if (major != null)
                                //            valAxisNode.RemoveChild(major);

                                //        var minor = valAxisNode.SelectSingleNode("c:minorGridlines", nsm);
                                //        if (minor != null)
                                //            valAxisNode.RemoveChild(minor);
                                //    }

                                ////Other charts can have a category axis
                                //catAxisNodes = chartXml.SelectNodes("c:chartSpace/c:chart/c:plotArea/c:catAx", nsm);
                                //if (catAxisNodes != null && catAxisNodes.Count > 0)
                                //    foreach (XmlNode catAxisNode in catAxisNodes)
                                //    {
                                //        var major = catAxisNode.SelectSingleNode("c:majorGridlines", nsm);
                                //        if (major != null)
                                //            catAxisNode.RemoveChild(major);

                                //        var minor = catAxisNode.SelectSingleNode("c:minorGridlines", nsm);
                                //        if (minor != null)
                                //            catAxisNode.RemoveChild(minor);
                                //    }
                                #endregion

                                LossesLooper++;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    #endregion

                }
            }
            #endregion

            //Apply style to Losses header
            int col = 11 + LossCodesData.Rows.Count - 1; // StartCol in Excel + TotalLosses
            finalLossCol = ExcelColumnFromNumber(col);
            //worksheetGraph.Cells[worksheet.Dimension.Address].AutoFitColumns();
            if (finalLossCol != null)
            {
                Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#32CD32");//#32CD32:lightgreen //B8C9E9
                worksheet.Cells["J4:" + finalLossCol + "" + 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["J4:" + finalLossCol + "" + 4].Style.Fill.BackgroundColor.SetColor(colFromHex);
                worksheet.Cells["J4:" + finalLossCol + "" + 4].Style.Border.Top.Style = ExcelBorderStyle.Medium;
                worksheet.Cells["J4:" + finalLossCol + "" + 4].Style.WrapText = true;
                worksheetGraph.Cells["A5:" + finalLossCol + "" + 5].Style.WrapText = true;
            }
            //worksheetGraph.Cells["A1:B2"].Style.Font.Color.SetColor(Color.White);
            worksheet.Row(4).Height = 70;
            worksheetGraph.Row(5).Height = 90;
            worksheet.View.ShowGridLines = false;
            worksheetGraph.View.ShowGridLines = false;
            //worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            worksheetGraph.Cells[worksheet.Dimension.Address].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            //worksheetGraph.Cells[worksheet.Dimension.Address].AutoFitColumns();

            #region Save and Download
            p.Save();

            string path1 = System.IO.Path.Combine(FileDir, "LossDetailsReport" + frda.ToString("yyyy-MM-dd") + ".xlsx");
            System.IO.FileInfo file1 = new System.IO.FileInfo(path1);
            string Outgoingfile = "LossDetailsReport" + frda.ToString("yyyy-MM-dd") + ".xlsx";
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

        static int Compare2(KeyValuePair<string, double> a, KeyValuePair<string, double> b)
        {
            return a.Value.CompareTo(b.Value);
        }

        List<string> GetHierarchyData1(int MachineID)
        {
            List<string> HierarchyData = new List<string>();
            //1st get PlantName or -
            //2nd get ShopName or -
            //3rd get CellName or -
            //4th get MachineName.

            using (i_facility_shaktiEntities1 dbMac = new i_facility_shaktiEntities1())
            {
                var machineData = dbMac.tblmachinedetails.Where(m => m.MachineID == MachineID).FirstOrDefault();
                int PlantID = Convert.ToInt32(machineData.PlantID);
                string name = "-";
                name = dbMac.tblplants.Where(m => m.PlantID == PlantID).Select(m => m.PlantName).FirstOrDefault();
                HierarchyData.Add(name);

                string ShopIDString = Convert.ToString(machineData.ShopID);
                int value;
                if (int.TryParse(ShopIDString, out value))
                {
                    name = dbMac.tblshops.Where(m => m.ShopID == value).Select(m => m.ShopName).FirstOrDefault();
                    HierarchyData.Add(name.ToString());
                }
                else
                {
                    HierarchyData.Add("-");
                }

                string CellIDString = Convert.ToString(machineData.CellID);
                if (int.TryParse(CellIDString, out value))
                {
                    name = dbMac.tblcells.Where(m => m.CellID == value).Select(m => m.CellName).FirstOrDefault();
                    HierarchyData.Add(name.ToString());
                }
                else
                {
                    HierarchyData.Add("-");
                }
                // HierarchyData.Add(Convert.ToString(machineData.MachineName));
                HierarchyData.Add(Convert.ToString(machineData.MachineID));
                HierarchyData.Add(Convert.ToString(machineData.MachineDisplayName));
            }
            return HierarchyData;
        }

        public List<KeyValuePair<int, double>> GetAllLossesDurationSeconds(int machineID, string CorrectedDate)
        {
            List<KeyValuePair<int, double>> durationList = new List<KeyValuePair<int, double>>();
            DataTable lossesData = new DataTable();
            int MachineIdleMinData = Convert.ToInt32(Serverdb.tblmachinedetails.Where(m => m.MachineID == machineID && m.IsDeleted == 0).Select(m => m.MachineIdleMin).FirstOrDefault());
            MachineIdleMinData = MachineIdleMinData * 60;
            DateTime corrcteddate = Convert.ToDateTime(CorrectedDate);
            var LossID = Serverdb.tbllossescodes.Where(m => m.IsDeleted == 0).Select(m => m.LossCodeID).ToList();
            //var LossesIDs = Serverdb.tbllossofentries.Where(m => m.MachineID == machineID && m.CorrectedDate == CorrectedDate && m.DoneWithRow == 1).Select(m => m.MessageCodeID).Distinct().ToList();

            foreach (var loss in LossID)
            {
                lossesData.Clear();
                double duration = 0;
                int lossID = Convert.ToInt32(loss);

                using (MsqlConnection mc = new MsqlConnection())
                {
                    mc.open();
                    String query1 = "SELECT DurationInSec,LossCodeID From " + dbName + ".[tblmode] WHERE MachineID = '" + machineID + "' and CorrectedDate = '" + CorrectedDate + "' and LossCodeID = '" + lossID + "' and DurationInSec >='" + MachineIdleMinData + "' and IsCompleted = 1;";
                    SqlDataAdapter da1 = new SqlDataAdapter(query1, mc.msqlConnection);
                    da1.Fill(lossesData);
                    mc.close();
                }

                for (int i = 0; i < lossesData.Rows.Count; i++)
                {
                    if (Convert.ToInt32(lossesData.Rows[i][0]) > 0 && !string.IsNullOrEmpty(Convert.ToString(lossesData.Rows[i][1])))
                    {
                        duration = duration + Convert.ToInt32(lossesData.Rows[i][0]);
                    }
                }

                durationList.Add(new KeyValuePair<int, double>(lossID, duration));
            }
            return durationList;
        }


        public List<KeyValuePair<int, double>> GetAllBreakdownDurationSeconds(int machineID, string CorrectedDate)
        {
            List<KeyValuePair<int, double>> durationList = new List<KeyValuePair<int, double>>();
            //var LossesIDs = Serverdb.tblbreakdowns.Where(m => m.MachineID == machineID && m.CorrectedDate == CorrectedDate && m.DoneWithRow == 1).Select(m => m.BreakDownCode).Distinct().ToList();
            int MachineIdleMinData = Convert.ToInt32(Serverdb.tblmachinedetails.Where(m => m.MachineID == machineID && m.IsDeleted == 0).Select(m => m.MachineIdleMin).FirstOrDefault());
            MachineIdleMinData = MachineIdleMinData * 60;
            DateTime corrcteddate = Convert.ToDateTime(CorrectedDate);
            var BreakID = Serverdb.tbllossescodes.Where(m => m.IsDeleted == 0 && m.MessageType == "MNT").Select(m => m.LossCodeID).ToList();

            foreach (var BreakIDS in BreakID)
            {
                DataTable lossesData = new DataTable();
                double duration = 0;
                int BreakDownID = Convert.ToInt32(BreakIDS);
                using (MsqlConnection mc = new MsqlConnection())
                {
                    mc.open();
                    String query1 = "SELECT DurationInSec,BreakdownID From " + dbName + ".[tblmode] WHERE MachineID = '" + machineID + "' and CorrectedDate = '" + CorrectedDate + "' and BreakdownID = '" + BreakIDS + "' and DurationInSec >='" + MachineIdleMinData + "' and IsCompleted = 1;";
                    SqlDataAdapter da1 = new SqlDataAdapter(query1, mc.msqlConnection);
                    da1.Fill(lossesData);
                    mc.close();
                }

                for (int i = 0; i < lossesData.Rows.Count; i++)
                {
                    if (Convert.ToInt32(lossesData.Rows[i][0]) > 0 && !string.IsNullOrEmpty(Convert.ToString(lossesData.Rows[i][1])))
                    {
                        duration = duration + Convert.ToInt32(lossesData.Rows[i][0]);
                    }
                }
                duration = duration;
                durationList.Add(new KeyValuePair<int, double>(BreakDownID, duration));
            }
            return durationList;
        }

        public void AlramReportExcel(int PlantID, String FromDate, int ShopID = 0, int CellID = 0)
        {
            //Using the template of the header
            FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\AlarmReport.xlsx");
            //FileInfo templateFile = new FileInfo(@"C:\Users\Pavan Kumar\Desktop\TitanAlarmReportPDF.xlsx");
            ExcelPackage templatep = new ExcelPackage(templateFile);
            ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];
            //Getting the exact file

            String FileDir = @"C:\SRKS_ifacility\ReportsList\" + System.DateTime.Now.ToString("yyyy-MM-dd");
            //String FileDir = @"E:\ReportsList\Alarm\" + System.DateTime.Now.AddDays(-1).ToString("yyyy");
            //String FileDir = @"C:\Users\Pavan Kumar\Desktop\" + System.DateTime.Now.AddDays(-1).ToString("yyyy");

            bool exists = System.IO.Directory.Exists(FileDir);

            if (!exists)
            {
                System.IO.Directory.CreateDirectory(FileDir);
            }

            string pathString = System.IO.Path.Combine(FileDir, "Alarm " + System.DateTime.Now.ToString("MMMMyyyy") + ".xls");

            String sourceFile = System.IO.Path.Combine(FileDir, "Alarm " + System.DateTime.Now.ToString("MMMMyyyy") + ".xls");
            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "Alarm " + System.DateTime.Now.AddDays(1).ToString("MMMMyyyy") + ".xls"));
            FileInfo nextFile = null;
            if (newFile.Exists)
            {
                newFile.Delete();
                nextFile = new FileInfo(System.IO.Path.Combine(FileDir, "Alarm " + System.DateTime.Now.ToString("MMMMyyyy") + "1.xls"));
            }

            //Using the File for generation and populating it
            ExcelPackage p = null;
            if (nextFile != null)
            {
                //p = new ExcelPackage(nextFile, newFile);
                p = new ExcelPackage(newFile);
            }
            else
            {
                p = new ExcelPackage(newFile);
            }
            ExcelWorksheet worksheet = null;

            //Creating the WorkSheet for populating
            try
            {
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), Templatews);
            }
            catch { }

            if (worksheet == null)
            {
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), Templatews);
            }

            if (System.DateTime.Now.AddDays(1).ToString("ddd").ToUpper() == "SUN")
            {
                worksheet.TabColor = Color.MediumVioletRed;
            }

            //worksheet.Protection.SetPassword("titan" + System.DateTime.Now.AddDays(-1).ToString("MMyyyy"));
            //worksheet.Protection.AllowSelectLockedCells = true;
            int sheetcount = p.Workbook.Worksheets.Count;
            p.Workbook.Worksheets.MoveToStart(sheetcount);
            worksheet.Cells.Style.Font.Name = "Times New Roman";
            worksheet.Cells.Style.Font.Bold = true;

            worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            //Creating  the mymsqlConnection and opening the connection
            MsqlConnection mc = new MsqlConnection();
            mc.open();
            worksheet.Cells["B2"].Value = FromDate;
            worksheet.Cells["B2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            worksheet.Cells["B2"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Bottom;
            worksheet.Cells["A2"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Bottom;

            //The general Details of the excel file needed to populate the data into the excel
            int slno = 1;
            String TCMacName = null, TMMacName = null, VMCMacName = null, HMCMacName = null;
            int row = 5;
            Char Startcol = 'A';
            Char EndCol = 'H';
            String StartCell = (Startcol.ToString() + row).ToString();
            String EndCell = (EndCol.ToString() + row).ToString();
            String NextCell = StartCell;
            //Populating the Turning Centre data
            #region
            //string TCSQLQuery = "Select AlarmMessage,AlarmTime,v.MachineID,MachineModel,MachineMake,MachineDispName,AlarmNo,Axis_No From alarm_history_master v,machine_master m " +
            //                    "Where v.MachineID = m.MachineID  and v.CorrectedDate = '" + System.DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd") +
            //                    "' ORDER BY v.MachineID,v.AlarmDateTime ASC";
            //MySqlDataAdapter daTC = new MySqlDataAdapter(TCSQLQuery, mc.msqlConnection);
            //System.Data.DataTable dtTC = new System.Data.DataTable();
            //daTC.Fill(dtTC);
            //worksheet.Cells["A" + row + ":H" + row].Merge = true;
            //worksheet.Cells[StartCell].Value = "TURNING CENTER - ALARMS LIST";
            //worksheet.Cells["A" + row + ":H" + row].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            //worksheet.Cells["A" + row + ":H" + row].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(204, 255, 51));
            //worksheet.Cells["A" + row + ":H" + row].Style.Border.Top.Style = ExcelBorderStyle.Double;
            //worksheet.Cells["A" + row + ":H" + row].Style.Border.Bottom.Style = ExcelBorderStyle.Double;
            //worksheet.Cells["A" + row + ":H" + row].Style.Border.Left.Style = ExcelBorderStyle.Double;
            //worksheet.Cells["A" + row + ":H" + row].Style.Border.Right.Style = ExcelBorderStyle.Double;
            int startrow = row + 1;
            //Insert Value into the excel cell for each n every machine
            List<Server_Model.tblmachinedetail> MachDet = new List<Server_Model.tblmachinedetail>();
            if (CellID != 0)
            {
                MachDet = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == CellID).ToList();
            }
            else if (ShopID != 0)
            {
                MachDet = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == ShopID).ToList();
            }
            else
            {
                MachDet = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == PlantID).ToList();
            }

            foreach (var Machrow in MachDet)
            {
                int MachId = Machrow.MachineID;
                //DateTime frmDate = Convert.ToDateTime(FromDate);
                string frmDate = Convert.ToString(FromDate);
                var AlarmDet = Serverdb.alarm_history_master.Where(m => m.CorrectedDate == frmDate && m.MachineID == MachId).OrderBy(m => new { m.MachineID, m.AlarmDateTime }).ToList();

                //for (int i = 0; i < dtTC.Rows.Count; i++)
                //    {
                foreach (var AlarmRow in AlarmDet)
                {
                    row++;
                    worksheet.Cells["A" + row + ":D" + row].Style.Border.Top.Style = ExcelBorderStyle.None;
                    worksheet.Cells["A" + row + ":D" + row].Style.Border.Bottom.Style = ExcelBorderStyle.None;
                    worksheet.Cells["A" + row + ":D" + row].Style.Border.Left.Style = ExcelBorderStyle.None;
                    worksheet.Cells["A" + row + ":D" + row].Style.Border.Right.Style = ExcelBorderStyle.None;
                    worksheet.Cells["E" + row + ":H" + row].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells["E" + row + ":H" + row].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells["E" + row + ":H" + row].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells["E" + row + ":H" + row].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells["G" + row].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    char colnam = Startcol;
                    //String TCMacNameRep = dtTC.Rows[i][2].ToString();
                    //if (TCMacName != TCMacNameRep)
                    //{
                    worksheet.Cells["A" + row + ":D" + row].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells["A" + row + ":D" + row].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells["A" + row + ":D" + row].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells["A" + row + ":D" + row].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    StartCell = (Startcol.ToString() + row).ToString();
                    EndCell = (EndCol.ToString() + row).ToString();
                    worksheet.Cells[StartCell].Value = slno;
                    slno++;
                    colnam++;
                    NextCell = (colnam.ToString() + row).ToString();
                    //worksheet.Cells[NextCell].Value = dtTC.Rows[i][5].ToString();
                    worksheet.Cells[NextCell].Value = Machrow.MachineDisplayName;
                    worksheet.Cells[NextCell].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    colnam++;
                    NextCell = (colnam.ToString() + row).ToString();
                    //worksheet.Cells[NextCell].Value = dtTC.Rows[i][4].ToString();
                    worksheet.Cells[NextCell].Value = Machrow.MachineMake;
                    worksheet.Cells[NextCell].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    colnam++;
                    NextCell = (colnam.ToString() + row).ToString();
                    //worksheet.Cells[NextCell].Value = dtTC.Rows[i][3].ToString();
                    worksheet.Cells[NextCell].Value = Machrow.MachineModel;
                    worksheet.Cells[NextCell].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    colnam++;
                    NextCell = (colnam.ToString() + row).ToString();
                    //}
                    //String AxisNo = dtTC.Rows[i][7].ToString();
                    String AxisNo = AlarmRow.Axis_No;
                    if (AxisNo == "0")
                    {
                        AxisNo = "-";
                    }
                    //worksheet.Cells["E" + row].Value = dtTC.Rows[i][6].ToString();
                    worksheet.Cells["E" + row].Value = AlarmRow.AlarmNo;
                    worksheet.Cells["F" + row].Value = AxisNo;
                    //worksheet.Cells["G" + row].Value = dtTC.Rows[i][0].ToString();
                    worksheet.Cells["G" + row].Value = AlarmRow.AlarmMessage;
                    //worksheet.Cells["H" + row].Value = Convert.ToDateTime(dtTC.Rows[i][1].ToString()).ToString("hh:mm tt");
                    worksheet.Cells["H" + row].Value = Convert.ToDateTime(AlarmRow.AlarmDateTime).ToString("hh:mm tt");
                    //TCMacName = TCMacNameRep;
                }
                #endregion
                mc.close();
            }

            p.Save();
            //if (nextFile != null)
            //{
            //    newFile.Delete();
            //    nextFile.MoveTo(sourceFile);
            //}

            string path1 = System.IO.Path.Combine(FileDir, "Alarm " + DateTime.Now.ToString("MMMMyyyy") + ".xls");
            System.IO.FileInfo file1 = new System.IO.FileInfo(path1);
            //string Outgoingfile = "LossDetailsReport" + frda.ToString("yyyy-MM-dd") + ".xlsx";
            string Outgoingfile = "Alarm " + DateTime.Now.ToString("MMMMyyyy") + ".xls";
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
        }

        public void PushDataToTblPartLearingReport(PartSearchCreate obj)
        {
            //(obj.FG_code != null || obj.FG_code != "") && 
            if ((obj.StartTime != null) && (obj.EndTime != null) && (obj.MachineId != null))
            {
                foreach (var macId in obj.MachineId)
                {
                    var getWorkOrderIds = Serverdb.tblworkorderentries.Where(m => m.MachineID == macId && m.IsFinished == 1).Where(m => m.WOStart >= obj.StartTime && m.WOEnd <= obj.EndTime).ToList();

                    //String query = "  select * from "+dbName+".[tblpartlearningreport] where HMIID in  (SELECT HMIID FROM "+dbName+".[tblworkorderentry] where CorrectedDate >= '" + obj.StartTime.ToString("yyyy-MM-dd") + "'  and CorrectedDate <= '" + obj.EndTime.ToString("yyyy-MM-dd") + "' and MachineID = " + macId + " ); ";

                    if (obj.FG_code != null && obj.FG_code != "")
                    {
                        getWorkOrderIds = Serverdb.tblworkorderentries.Where(m => m.MachineID == macId && m.FGCode == obj.FG_code && m.IsFinished == 1).Where(m => m.WOStart >= obj.StartTime && m.WOEnd <= obj.EndTime).ToList();

                        //query = "  select * from "+dbName+".[tblpartlearningreport] where HMIID in  (SELECT HMIID FROM "+dbName+".[tblworkorderentry] where FGCode = '" + obj.FG_code + "' and CorrectedDate >= '" + obj.StartTime.ToString("yyyy-MM-dd") + "'  and CorrectedDate <= '" + obj.EndTime.ToString("yyyy-MM-dd") + "' and MachineID = " + macId + " ); ";
                    }
                    int count = getWorkOrderIds.Count();
                    if (count > 0)
                    {
                        //DataTable PartData = new DataTable();
                        //using (MsqlConnection mc = new MsqlConnection())
                        //{
                        //    mc.open();
                        //    SqlDataAdapter da = new SqlDataAdapter(query, mc.msqlConnection);
                        //    da.Fill(PartData);
                        //    mc.close();
                        //}
                        //int countPartData = PartData.Rows.Count;
                        //if (countPartData == 0)
                        {
                            foreach (var item in getWorkOrderIds)
                            {
                                var GetDataPre = Serverdb.tblpartlearningreports.Where(m => m.HMIID == item.HMIID).ToList();
                                if (GetDataPre.Count == 0)
                                {
                                    int OperatingTime = 0;
                                    int LossTime = 0;
                                    int MinorLossTime = 0;
                                    int MntTime = 0;
                                    int SetupTime = 0;
                                    int SetupMinorTime = 0;
                                    int PowerOffTime = 0;
                                    long idle = 0;
                                    decimal loadAndUnload = 0;
                                    int rejections = 0;
                                    DateTime ProdStartTime = item.WOStart;
                                    DateTime ProdEndtime = DateTime.Now;
                                    try
                                    {
                                        if (item.WOEnd.HasValue)
                                        {
                                            ProdEndtime = Convert.ToDateTime(item.WOEnd);
                                        }
                                    }
                                    catch { }

                                    #region Logic to get the Mode Durations between a Production Order which are completed
                                    var GetModeDurations = Serverdb.tblmodes.Where(m => m.MachineID == macId && m.StartTime >= ProdStartTime && m.StartTime < ProdEndtime && m.EndTime > ProdStartTime && m.EndTime < ProdEndtime && m.IsCompleted == 1 && m.ModeTypeEnd == 1).ToList();
                                    foreach (var ModeRow in GetModeDurations)
                                    {
                                        if (ModeRow.ModeType == "PROD")
                                        {
                                            OperatingTime += (int)(ModeRow.DurationInSec / 60);
                                        }
                                        else if (ModeRow.ModeType == "IDLE" && ModeRow.DurationInSec > 600)
                                        {
                                            LossTime += (int)(ModeRow.DurationInSec / 60);
                                            int LossDuration = (int)(ModeRow.DurationInSec / 60);
                                        }
                                        else if (ModeRow.ModeType == "IDLE" && ModeRow.DurationInSec < 600)
                                        {
                                            MinorLossTime += (int)(ModeRow.DurationInSec / 60);
                                        }
                                        else if (ModeRow.ModeType == "MNT")
                                        {
                                            MntTime += (int)(ModeRow.DurationInSec / 60);
                                        }
                                        else if (ModeRow.ModeType == "POWEROFF")
                                        {
                                            PowerOffTime += (int)(ModeRow.DurationInSec / 60);
                                        }
                                        else if (ModeRow.ModeType == "SETUP")
                                        {
                                            try
                                            {
                                                SetupTime += (int)(Serverdb.tblSetupMaints.Where(m => m.ModeID == ModeRow.ModeID).Select(m => m.DurationInSec).First() / 60);
                                                SetupMinorTime += (int)(Serverdb.tblSetupMaints.Where(m => m.ModeID == ModeRow.ModeID).Select(m => m.MinorLossTime).First() / 60);
                                            }
                                            catch { }
                                        }
                                    }
                                    #endregion

                                    #region Logic to get the Mode Duration Which Was started before this Production and Ended during this Production
                                    var GetEndModeDuration = Serverdb.tblmodes.Where(m => m.MachineID == macId && m.StartTime < ProdStartTime && m.EndTime > ProdStartTime && m.EndTime < ProdEndtime && m.IsCompleted == 1 && m.ModeTypeEnd == 1).FirstOrDefault();
                                    if (GetEndModeDuration != null)
                                    {
                                        if (GetEndModeDuration.ModeType == "PROD")
                                        {
                                            OperatingTime += (int)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                                        }
                                        else if (GetEndModeDuration.ModeType == "IDLE")
                                        {
                                            LossTime += (int)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                                            int LossDuration = (int)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                                            //insertProdlosses(WOID, LossID, LossDuration, CorrectedDate);
                                        }
                                        else if (GetEndModeDuration.ModeType == "MNT")
                                        {
                                            MntTime += (int)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                                        }
                                        else if (GetEndModeDuration.ModeType == "POWEROFF")
                                        {
                                            PowerOffTime += (int)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                                        }
                                    }
                                    #endregion

                                    #region Logic to get the Mode Duration Which Was Started during the Production and Ended after the Production
                                    var GetStartModeDuration = Serverdb.tblmodes.Where(m => m.MachineID == macId && m.StartTime >= ProdStartTime && m.EndTime >= ProdStartTime && m.StartTime < ProdEndtime && m.EndTime > ProdEndtime && m.IsCompleted == 1 && m.ModeTypeEnd == 1).FirstOrDefault();
                                    if (GetStartModeDuration != null)
                                    {
                                        if (GetStartModeDuration.ModeType == "PROD")
                                        {
                                            OperatingTime += (int)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60);
                                        }
                                        else if (GetStartModeDuration.ModeType == "IDLE")
                                        {
                                            LossTime += (int)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60);
                                            int LossDuration = (int)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60);
                                            //insertProdlosses(WOID, LossID, LossDuration, CorrectedDate);
                                        }
                                        else if (GetStartModeDuration.ModeType == "MNT")
                                        {
                                            MntTime += (int)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60);
                                        }
                                        else if (GetStartModeDuration.ModeType == "POWEROFF")
                                        {
                                            PowerOffTime += (int)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60);
                                        }
                                    }
                                    #endregion

                                    int TotlaQty = item.Total_Qty;
                                    if (TotlaQty == 0)
                                        TotlaQty = 1;
                                    int GetOptime = OperatingTime;
                                    if (GetOptime == 0)
                                    {
                                        GetOptime = 1;
                                    }
                                    decimal IdealCycleTimeVal = 0;
                                    decimal UtilPercent = 0;
                                    var IdealCycTime = Serverdb.tblparts.Where(m => m.FGCode == item.FGCode && m.OperationNo == item.OperationNo).FirstOrDefault();
                                    if (IdealCycTime != null)
                                        IdealCycleTimeVal = IdealCycTime.IdealCycleTime;
                                    double TotalTime = ProdEndtime.Subtract(ProdStartTime).TotalMinutes;
                                    if (TotalTime != 0)
                                        UtilPercent = (decimal)Math.Round(OperatingTime / TotalTime * 100, 2);
                                    decimal Quality = (decimal)Math.Round((double)item.Yield_Qty / TotlaQty * 100, 2);
                                    decimal Performance = (decimal)Math.Round((double)IdealCycleTimeVal * (double)item.Total_Qty / GetOptime * 100, 2);
                                    int PerformanceFactor = (int)IdealCycleTimeVal * item.Total_Qty;
                                    //tbl_ProdManMachine PRA = new tbl_ProdManMachine();
                                    //PRA.MachineID = macId;
                                    //PRA.WOID = item.HMIID;
                                    ////PRA.CorrectedDate = CorrectedDate.Date;
                                    //PRA.TotalLoss = LossTime;
                                    //PRA.TotalOperatingTime = OperatingTime;
                                    //PRA.TotalSetup = SetupTime + SetupMinorTime;
                                    //PRA.TotalMinorLoss = MinorLossTime - SetupMinorTime;
                                    //PRA.TotalSetupMinorLoss = SetupMinorTime;
                                    //PRA.TotalPowerLoss = PowerOffTime;
                                    //PRA.UtilPercent = UtilPercent;
                                    //PRA.QualityPercent = Quality;
                                    //PRA.PerformancePerCent = Performance;
                                    //PRA.PerfromaceFactor = PerformanceFactor;
                                    //PRA.InsertedOn = DateTime.Now;
                                    loadAndUnload = MinorLossTime;
                                    int TotalQty = item.Yield_Qty + item.ScrapQty;
                                    if (TotalQty == 0)
                                        TotalQty = 1;
                                    rejections = Convert.ToInt32((OperatingTime / TotalQty) * item.ScrapQty);

                                    var GetMainLossList = Serverdb.tbllossescodes.Where(m => m.LossCodesLevel == 1 && m.IsDeleted == 0 && m.MessageType != "SETUP").OrderBy(m => m.LossCodeID).ToList();
                                    foreach (var LossRow in GetMainLossList)
                                    {
                                        var getWoLossList1 = Serverdb.tbl_ProdOrderLosses.Where(m => m.WOID == item.HMIID && m.LossID == LossRow.LossCodeID).FirstOrDefault();
                                        if (getWoLossList1 == null)
                                        {
                                            idle = idle + 0;
                                        }
                                        else
                                        {
                                            idle = idle + getWoLossList1.LossDuration;
                                        }
                                        if (LossRow.LossCode == "LOAD / UNLOAD")
                                        {
                                            if (getWoLossList1 == null)
                                            {
                                                loadAndUnload = loadAndUnload + 0;
                                            }
                                            else
                                            {
                                                loadAndUnload = loadAndUnload + getWoLossList1.LossDuration;
                                            }
                                        }
                                    }
                                    var dbParts = Serverdb.tblparts.Where(m => m.FGCode == item.FGCode && m.OperationNo == item.OperationNo).FirstOrDefault();
                                    decimal idealctime = 0;
                                    decimal? stdmloss = 0;
                                    if (dbParts != null)
                                    {
                                        idealctime = dbParts.IdealCycleTime;
                                        if (dbParts.StdMinorLoss != null)
                                        {
                                            //  stdmloss = (decimal)dbParts.StdMinorLoss;
                                        }
                                        else
                                        {
                                            stdmloss = 0;
                                        }
                                    }
                                    tblpartlearningreport partLearning = new tblpartlearningreport();
                                    partLearning.MachineID = macId;
                                    partLearning.HMIID = item.HMIID;
                                    partLearning.CorrectedDate = item.CorrectedDate.ToString("yyyy-MM-dd");
                                    partLearning.WorkOrderNo = item.Prod_Order_No;
                                    partLearning.FGCode = item.FGCode;
                                    partLearning.OpNo = item.OperationNo;
                                    partLearning.TargetQty = item.ProdOrderQty;
                                    partLearning.TotalQty = item.Total_Qty;
                                    partLearning.YieldQty = item.Yield_Qty;
                                    partLearning.ScrapQty = item.ScrapQty;
                                    partLearning.SettingTime = SetupTime + SetupMinorTime;
                                    partLearning.Idle = idle;
                                    partLearning.MinorLoss = loadAndUnload;
                                    partLearning.PowerOff = PowerOffTime;
                                    partLearning.TotalNCCuttingTime = OperatingTime;
                                    try
                                    {
                                        partLearning.AvgCuttingTime = OperatingTime / item.Total_Qty;
                                    }
                                    catch
                                    {
                                        partLearning.AvgCuttingTime = 0;
                                    }

                                    //partLearning.IdleCycleTime = idealctime;
                                    //partLearning.TotalIdleCycleTime = idealctime * item.Total_Qty;
                                    partLearning.StdCycleTime = idealctime;
                                    partLearning.TotalStdCycleTime = idealctime * item.Total_Qty;
                                    partLearning.StdMinorLoss = stdmloss;
                                    partLearning.TotalStdMinorLoss = stdmloss * item.Total_Qty;
                                    partLearning.InsertedOn = DateTime.Now;
                                    partLearning.StartTime = obj.StartTime;
                                    partLearning.EndTime = obj.EndTime;
                                    Serverdb.tblpartlearningreports.Add(partLearning);
                                    Serverdb.SaveChanges();
                                }
                            }
                        }
                    }
                }
            }

        }

        public void setcellcolor(ExcelWorksheet ws, int value, String cell)
        {
            try
            {
                ws.Cells[cell].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                if (value < 0)
                {
                    ws.Cells[cell].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                }
                else if (value >= 0)
                {
                    ws.Cells[cell].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                }
            }
            catch { }
        }

        public void settextcolor(ExcelWorksheet ws, double value, String cell)
        {
            try
            {
                ws.Cells[cell].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                if (value > 0)
                {
                    ws.Cells[cell].Style.Font.Color.SetColor(Color.DarkRed);
                }
                else if (value <= 0)
                {
                    ws.Cells[cell].Style.Font.Color.SetColor(Color.Green);
                }
            }
            catch { }
        }

        List<string> GetHierarchyData(int MachineID)
        {
            List<string> HierarchyData = new List<string>();
            //1st get PlantName or -
            //2nd get ShopName or -
            //3rd get CellName or -
            //4th get MachineName.

            using (i_facility_shaktiEntities1 dbMac = new i_facility_shaktiEntities1())
            {
                var machineData = dbMac.tblmachinedetails.Where(m => m.MachineID == MachineID).FirstOrDefault();
                int PlantID = Convert.ToInt32(machineData.PlantID);
                string name = "-";
                name = dbMac.tblplants.Where(m => m.PlantID == PlantID).Select(m => m.PlantName).FirstOrDefault();
                HierarchyData.Add(name);

                string ShopIDString = Convert.ToString(machineData.ShopID);
                int value;
                if (int.TryParse(ShopIDString, out value))
                {
                    name = dbMac.tblshops.Where(m => m.ShopID == value).Select(m => m.ShopName).FirstOrDefault();
                    HierarchyData.Add(name.ToString());
                }
                else
                {
                    HierarchyData.Add("-");
                }

                string CellIDString = Convert.ToString(machineData.CellID);
                if (int.TryParse(CellIDString, out value))
                {
                    name = dbMac.tblcells.Where(m => m.CellID == value).Select(m => m.CellName).FirstOrDefault();
                    HierarchyData.Add(name.ToString());
                }
                else
                {
                    HierarchyData.Add("-");
                }
                // HierarchyData.Add(Convert.ToString(machineData.MachineName));
                HierarchyData.Add(Convert.ToString(machineData.MachineDisplayName));
            }
            return HierarchyData;
        }

        //code to remove major GridLines
        public void RemoveGridLines(ref ExcelChart chartName)
        {
            var chartXml = chartName.ChartXml;
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

        public ActionResult ToolLife()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");

            return View();
        }

        [HttpPost]
        public ActionResult ToolLife(string PlantID, string ShopID, string CellID, string WorkCenterID, string OpNo, DateTime FromDate, DateTime ToDate, string ProdNo = null, string CTCode = null)
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            String RetStatus = "";
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];
            ToolLifeReportExcel(FromDate.ToString("yyyy-MM-dd"), ToDate.ToString("yyyy-MM-dd"), PlantID.ToString(), Convert.ToString(ShopID), Convert.ToString(CellID), Convert.ToString(WorkCenterID), CTCode, OpNo, ProdNo, CTCode);
            int p = Convert.ToInt32(PlantID);
            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");
            TempData["ToolLifeStatus"] = RetStatus;
            return View();
        }

        public void ToolLifeReportExcel(string StartDate, string EndDate, string PlantID, string ShopID, string CellID, string WorkCenterID, string PartsList, string opNo, string ProdNo = null, string CTCode = null)
        {
            string RetStatus = "";

            #region Excel and Stuff

            DateTime frda = DateTime.Now;
            if (string.IsNullOrEmpty(StartDate) == true)
            {
                StartDate = DateTime.Now.Date.ToString();
            }
            if (string.IsNullOrEmpty(EndDate) == true)
            {
                EndDate = StartDate;
            }

            DateTime frmDate = Convert.ToDateTime(StartDate);
            DateTime toDate = Convert.ToDateTime(EndDate);

            double TotalDay = toDate.Subtract(frmDate).TotalDays;

            FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\ToolLifeMonitoringSheet.xlsx");
            ExcelPackage templatep = new ExcelPackage(templateFile);
            ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];
            //ExcelWorksheet TemplateGraph = templatep.Workbook.Worksheets[2];

            String FileDir = @"C:\SRKS_ifacility\ReportsList\" + System.DateTime.Now.ToString("yyyy-MM-dd");
            bool exists = System.IO.Directory.Exists(FileDir);
            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "ToolLifeMonitoringSheet" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "ToolLifeMonitoringSheet" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //" to " + toda.ToString("yyyy-MM-dd") + 
                }
                catch
                {
                    RetStatus = "Excel with same date is already open, please close it and try to generate!!!!";
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
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), Templatews);
                //worksheetGraph = p.Workbook.Worksheets.Add("Graphs", TemplateGraph);
            }
            catch { }

            if (worksheet == null)
            {
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), Templatews);
                //worksheetGraph = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), TemplateGraph);
            }
            int sheetcount = p.Workbook.Worksheets.Count;
            p.Workbook.Worksheets.MoveToStart(sheetcount);
            worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            #endregion

            #region MacCount & LowestLevel
            string lowestLevel = null;
            int MacCount = 0;
            int plantId = 0, shopId = 0, cellId = 0, wcId = 0;
            if (string.IsNullOrEmpty(WorkCenterID))
            {
                if (string.IsNullOrEmpty(CellID))
                {
                    if (string.IsNullOrEmpty(ShopID))
                    {
                        if (string.IsNullOrEmpty(PlantID))
                        {
                            //donothing
                        }
                        else
                        {
                            lowestLevel = "Plant";
                            plantId = Convert.ToInt32(PlantID);
                            MacCount = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == plantId).ToList().Count();
                        }
                    }
                    else
                    {
                        lowestLevel = "Shop";
                        shopId = Convert.ToInt32(ShopID);
                        MacCount = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.ShopID == shopId).ToList().Count();
                    }
                }
                else
                {
                    lowestLevel = "Cell";
                    cellId = Convert.ToInt32(CellID);
                    MacCount = Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.CellID == cellId).ToList().Count();
                }
            }
            else
            {
                lowestLevel = "WorkCentre";
                wcId = Convert.ToInt32(WorkCenterID);
                MacCount = 1;
            }

            #endregion

            #region Get Machines List
            DataTable machin = new DataTable();
            DateTime endDateTime = Convert.ToDateTime(toDate.AddDays(1).ToString("yyyy-MM-dd") + " " + new TimeSpan(6, 0, 0));
            string startDateTime = frmDate.ToString("yyyy-MM-dd");
            using (MsqlConnection mc = new MsqlConnection())
            {
                mc.open();
                String query1 = null;
                if (lowestLevel == "Plant")
                {
                    query1 = " SELECT  distinct MachineID FROM  " + dbName + ".tblmachinedetails WHERE PlantID = " + PlantID + "  and IsNormalWC = 0  and ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or (CASE IsDeleted WHEN 1 THEN  CASE WHEN((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "') and  (DeletedDate >= '" + startDateTime + "'))  THEN 1 ELSE 0 END END = 1)); ";
                }
                else if (lowestLevel == "Shop")
                {
                    query1 = " SELECT * FROM  " + dbName + ".tblmachinedetails WHERE ShopID = " + ShopID + "  and IsNormalWC = 0   and  ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or (CASE IsDeleted WHEN 1 THEN  CASE WHEN((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "') and  (DeletedDate >= '" + startDateTime + "'))  THEN 1 ELSE 0 END END = 1)); ";
                }
                else if (lowestLevel == "Cell")
                {
                    query1 = " SELECT * FROM  " + dbName + ".tblmachinedetails WHERE CellID = " + CellID + "  and IsNormalWC = 0  and   ((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or (CASE IsDeleted WHEN 1 THEN  CASE WHEN((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "') and  (DeletedDate >= '" + startDateTime + "'))  THEN 1 ELSE 0 END END = 1)); ";
                }
                else if (lowestLevel == "WorkCentre")
                {
                    query1 = "SELECT * FROM  " + dbName + ".tblmachinedetails WHERE MachineID = " + WorkCenterID + "  and IsNormalWC = 0 and((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "' and IsDeleted = 0) or (CASE IsDeleted WHEN 1 THEN  CASE WHEN((InsertedOn <= '" + endDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "') and  (DeletedDate >= '" + startDateTime + "'))  THEN 1 ELSE 0 END END = 1)); ";
                }
                SqlDataAdapter da1 = new SqlDataAdapter(query1, mc.msqlConnection);
                da1.Fill(machin);
                mc.close();
            }
            #endregion
            List<int> MachineIdList = new List<int>();
            foreach (DataRow intItem in machin.Rows)
            {
                MachineIdList.Add(Convert.ToInt32(intItem["MachineID"].ToString()));
            }
            DateTime UsedDateForExcel = Convert.ToDateTime(frmDate);
            int Row = 9; // Gap to Insert OverAll data. DataStartRow + MachinesCount + 2(1 for HighestLevel & another for Gap).

            var FGCodeDet = Serverdb.tblworkorderentries.Where(m => m.FGCode.Trim() == ProdNo && m.OperationNo == opNo).FirstOrDefault();
            string drawingNo = Serverdb.tblparts.Where(m => m.FGCode == ProdNo.Trim() && m.OperationNo == opNo).Select(m => m.DrawingNo).FirstOrDefault();
            int? macId = Convert.ToInt32(WorkCenterID);
            string macName = Serverdb.tblmachinedetails.Where(m => m.MachineID == macId).Select(m => m.MachineDisplayName).FirstOrDefault();
            int? stdToolLife = Serverdb.tblStdToolLives.Where(m => m.FGCode == ProdNo.Trim() && m.OperationNo == opNo && m.CTCode == CTCode).Select(m => m.StdToolLife).FirstOrDefault();
            worksheet.Cells["C4"].Value = CTCode;
            worksheet.Cells["C6"].Value = ProdNo;
            worksheet.Cells["H4"].Value = stdToolLife;
            worksheet.Cells["L4"].Value = opNo;
            worksheet.Cells["L6"].Value = macName;


            string correctedDate = UsedDateForExcel.ToString("yyyy-MM-dd");

            for (int i = 0; i < TotalDay + 1; i++)
            {
                DateTime QueryDate = frmDate.AddDays(i);
                DataTable toolData = new DataTable();
                using (MsqlConnection mc = new MsqlConnection())
                {
                    mc.open();
                    String query = "SELECT wrk.*,tblop.HMIID,tblop.IsReset,tblop.ResetReason,tblop.toollifecounter FROM" + dbName + ".[tbltoollifeoperator] tblop " +
                        "left outer join" + dbName + ".[tblworkorderentry] wrk on  tblop.HMIID=wrk.HMIID where wrk.FGCode='" + ProdNo +
                        "' and wrk.OperationNo='" + opNo + "' and wrk.MachineID= " + WorkCenterID + " and tblop.ToolCTCode = '" + CTCode + "'";

                    SqlDataAdapter da = new SqlDataAdapter(query, mc.msqlConnection);
                    da.Fill(toolData);
                    mc.close();
                }

                int CumulativeValue = 0;
                for (int j = 0; j < toolData.Rows.Count; j++)
                {
                    int MachineID = Convert.ToInt32(toolData.Rows[j][1]); //MachineID

                    string CorrectedDate = Convert.ToString(toolData.Rows[j][14]);//CorrectedDate
                    DateTime CorrectedDate1 = Convert.ToDateTime(CorrectedDate);
                    correctedDate = CorrectedDate1.Date.ToString("dd-MM-yyyy");
                    string shift = Convert.ToString(toolData.Rows[j][5]);//shift

                    int isreset = Convert.ToInt32(toolData.Rows[j][29]);

                    string ResetReason = Convert.ToString(toolData.Rows[j][31]);//ResetReason
                    CumulativeValue += Convert.ToInt32(toolData.Rows[j][32]);
                    worksheet.Cells["B" + Row].Value = QueryDate;
                    worksheet.Cells["B" + Row].Style.Numberformat.Format = "yyyy-MM-dd";
                    worksheet.Cells["C" + Row].Value = toolData.Rows[j][7].ToString();
                    worksheet.Cells["D" + Row].Value = shift;
                    worksheet.Cells["E" + Row].Value = Convert.ToInt32(toolData.Rows[j][32]);
                    worksheet.Cells["H" + Row].Value = CumulativeValue;
                    if (isreset == 0)
                    {
                        worksheet.Cells["K" + Row].Value = "NA";
                    }
                    else
                    {
                        worksheet.Cells["K" + Row].Value = ResetReason;
                    }

                    //string modelRange = "B" + Row + ":M" + Row + "";
                    worksheet.Cells["E" + Row + ":G" + Row + ""].Merge = true;
                    worksheet.Cells["H" + Row + ":J" + Row + ""].Merge = true;
                    worksheet.Cells["K" + Row + ":N" + Row + ""].Merge = true;
                    //var modelTable = worksheet.Cells[modelRange];
                    //modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    //modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    //modelTable.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    //modelTable.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    Row++;
                }
            }

            #region Save and Download

            p.Save();

            //Downloding Excel
            string path1 = System.IO.Path.Combine(FileDir, "ToolLifeMonitoringSheet" + frda.ToString("yyyy-MM-dd") + ".xlsx");
            System.IO.FileInfo file1 = new System.IO.FileInfo(path1);
            string Outgoingfile = "ToolLifeMonitoringSheet" + frda.ToString("yyyy-MM-dd") + ".xlsx";
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

        private List<ManualBatching> GetBatchForManual(List<DataRow> HMIDATAList, string CurrOpeatorName)
        {
            ReportsDao obj = new ReportsDao();
            Dao obj1 = new Dao();
            Dao1 obj2 = new Dao1();
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj2 = new Dao1(_conn);
            obj = new ReportsDao(_conn);
            List<ManualBatching> BatchningList = new List<ManualBatching>();
            //HMIDATAList = HMIDATA.AsEnumerable().Where(m => m["OperatorName"] == CurrOpeatorName).ToList().ToList();
            int l = 0;
            int k = 1;
            foreach (var row in HMIDATAList)
            {

                l = l + 1;
                if (l < HMIDATAList.Count)
                {
                    DateTime ST = Convert.ToDateTime(HMIDATAList[l - 1]["StartTime"]);
                    DateTime ST1 = Convert.ToDateTime(HMIDATAList[l]["StartTime"]);
                    Double duration = ST.Subtract(ST1).TotalMinutes;
                    if (duration < 1)
                    {
                        ManualBatching batch = new ManualBatching();
                        batch.BatchID = k;
                        batch.StartTime = ST;
                        batch.EndTime = Convert.ToDateTime(HMIDATAList[l - 1]["EndTime"]);
                        batch.Duration = duration;
                        BatchningList.Add(batch);
                    }
                    else
                        k = k + 1;
                }
            }
            return BatchningList;
        }

        private double CalculateSurfaceAreaAndPerimeter(string PartNumber, int MeasuringUnit)
        {
            ReportsDao obj = new ReportsDao();
            Dao obj1 = new Dao();
            Dao1 obj2 = new Dao1();
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj2 = new Dao1(_conn);
            obj = new ReportsDao(_conn);
            double Result = 0;
            if (MeasuringUnit == 1)
            {
                Result = Convert.ToDouble(obj.GettblpartwisespDet3(PartNumber));
                //Result = db.tblpartwisesps.Where(pw => pw.PartName == PartNumber).Select(pw => pw.SurfaceArea).FirstOrDefault();
            }
            else
            {
                Result = Convert.ToDouble(obj.GettblpartwisespDet1(PartNumber));
                //Result = db.tblpartwisesps.Where(pw => pw.PartName == PartNumber).Select(pw => pw.Perimeter).FirstOrDefault();
            }
            return Result;
        }

        string GetHoldHierarchy(int holdId)
        {
            string HoldHierarchy = null;
            //var holdData = db.tblholdcodes.Where(m => m.HoldCodeID == holdId).FirstOrDefault();
            var holdData = obj.GetHoldCodeDetails(holdId);

            int level = holdData.HoldCodesLevel;
            if (level == 1)
            {

                HoldHierarchy += holdData.HoldCode;
            }
            else if (level == 2)
            {
                int lossLevel1ID = Convert.ToInt32(holdData.HoldCodesLevel1ID);
                //var lossdata1 = db.tblholdcodes.Where(m => m.HoldCodeID == lossLevel1ID).FirstOrDefault();
                var lossdata1 = obj.GetHoldCodeDetails(lossLevel1ID);

                HoldHierarchy += lossdata1.HoldCode + "->" + holdData.HoldCode;
            }
            else if (level == 3)
            {
                int lossLevel1ID = Convert.ToInt32(holdData.HoldCodesLevel1ID);
                int lossLevel2ID = Convert.ToInt32(holdData.HoldCodesLevel2ID);
                //var lossdata1 = db.tblholdcodes.Where(m => m.HoldCodeID == lossLevel1ID).FirstOrDefault();
                var lossdata1 = obj.GetHoldCodeDetails(lossLevel1ID);
                //var lossdata2 = db.tblholdcodes.Where(m => m.HoldCodeID == lossLevel2ID).FirstOrDefault();
                var lossdata2 = obj.GetHoldCodeDetails(lossLevel2ID);
                HoldHierarchy += lossdata1.HoldCode + "->" + lossdata2.HoldCode + "->" + holdData.HoldCode;
            }
            return HoldHierarchy;
        }

        // Convert List TO DataTable
        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        public int get_PlanBreakdownduration()
        {

            int duration = 0;
            //var msgs2 = new List<tblplannedbreak>();

            using (i_facility_shaktiEntities1 db1 = new i_facility_shaktiEntities1())
            {
                int? msgs2 = db1.tblplannedbreaks.Where(m => m.IsDeleted == 0).Sum(m => m.BreakDuration);
                duration = Convert.ToInt32(msgs2);
            }

            //String[] msgtime = DateTime.Now.ToString("HH:mm:00").Split(':');
            //TimeSpan msgstime = new TimeSpan(Convert.ToInt32(msgtime[0]), Convert.ToInt32(msgtime[1]), Convert.ToInt32(msgtime[2]));
            //TimeSpan s1t1 = new TimeSpan(0, 0, 0), s1t2 = new TimeSpan(0, 0, 0);
            //TimeSpan s2t1 = new TimeSpan(0, 0, 0), s2t2 = new TimeSpan(0, 0, 0);
            //TimeSpan s3t1 = new TimeSpan(0, 0, 0), s3t2 = new TimeSpan(0, 0, 0), s3t3 = new TimeSpan(0, 0, 0), s3t4 = new TimeSpan(23, 59, 59);


            //for (int j = 0; j < msgs2.Count; j++)
            //{
            //    if (msgs2[j].ShiftID.ToString().Contains("1") || msgs2[j].ShiftID.ToString().Contains("A"))
            //    {
            //        String[] s1 = msgs2[j].StartTime.ToString().Split(':');
            //        s1t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
            //        String[] s11 = msgs2[j].EndTime.ToString().Split(':');
            //        s1t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));

            //        if (msgstime >= s1t1 || msgstime < s1t2)
            //        {
            //            duration += Convert.ToInt32(msgs2[j].BreakDuration);
            //        }
            //    }
            //    if (msgs2[j].ShiftID.ToString().Contains("2") || msgs2[j].ShiftID.ToString().Contains("B"))
            //    {
            //        String[] s1 = msgs2[j].StartTime.ToString().Split(':');
            //        s2t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
            //        String[] s11 = msgs2[j].EndTime.ToString().Split(':');
            //        s2t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));

            //        if (msgstime >= s2t1 && msgstime < s2t2)
            //        {
            //            duration += Convert.ToInt32(msgs2[j].BreakDuration);
            //        }
            //    }
            //    if (msgs2[j].ShiftID.ToString().Contains("3") || msgs2[j].ShiftID.ToString().Contains("C"))
            //    {
            //        String[] s1 = msgs2[j].StartTime.ToString().Split(':');
            //        s3t1 = new TimeSpan(Convert.ToInt32(s1[0]), Convert.ToInt32(s1[1]), Convert.ToInt32(s1[2]));
            //        String[] s11 = msgs2[j].EndTime.ToString().Split(':');
            //        s3t2 = new TimeSpan(Convert.ToInt32(s11[0]), Convert.ToInt32(s11[1]), Convert.ToInt32(s11[2]));


            //        if (msgstime >= s3t1 && msgstime < s3t2)
            //        {
            //            duration += Convert.ToInt32(msgs2[j].BreakDuration);
            //        }
            //    }


            //}

            return duration;

        }




        // Shakti_MTTR_MTBF
        public ActionResult MTTRandMTBFReport()
        {
            if ((Session["UserId"] == null) || (Session["UserId"].ToString() == String.Empty))
            {
                return RedirectToAction("Login", "Login", null);
            }
            ViewBag.Logout = Session["Username"];
            ViewBag.roleid = Session["RoleID"];

            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");
            return View();
        }
        [HttpPost]
        public ActionResult MTTRandMTBFReport(i_facilitylibrary.DAL.tblreportholder report, string PlantID, String FromDate, String ToDate, string ShopID = null, string CellID = null, string MachineID = null)
        {
            generateMTTRandMTBFReportExcel(report.FromDate.ToString(), report.ToDate.ToString(), PlantID, ShopID, CellID, MachineID);
            ViewData["PlantID"] = new SelectList(Serverdb.tblplants.Where(m => m.IsDeleted == 0), "PlantID", "PlantName");
            ViewData["ShopID"] = new SelectList(Serverdb.tblshops.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "ShopID", "ShopName");
            ViewData["CellID"] = new SelectList(Serverdb.tblcells.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "CellID", "CellName");
            ViewData["MachineID"] = new SelectList(Serverdb.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.PlantID == 999), "MachineID", "MachineDisplayName");

            return View();
        }

        public ActionResult generateMTTRandMTBFReportExcel(string startDate, string EndtDate, string PlantID, string ShopID = null, string CellID = null, string WorkCenterID = null)
        {
            ReportsDao obj = new ReportsDao();
            Dao obj1 = new Dao();
            Dao1 obj2 = new Dao1();
            _conn = new ConnectionFactory();
            obj1 = new Dao(_conn);
            obj2 = new Dao1(_conn);
            obj = new ReportsDao(_conn);
            DateTime frda = DateTime.Now;

            DateTime frmDate = Convert.ToDateTime(startDate);
            DateTime toDate = Convert.ToDateTime(EndtDate);

            FileInfo templateFile = new FileInfo(@"C:\SRKS_ifacility\MainTemplate\Shakti_MTTR_MTBF_Templete.xlsx");
            ExcelPackage templatep = new ExcelPackage(templateFile);
            ExcelWorksheet Templatews = templatep.Workbook.Worksheets[1];

            String FileDir = @"C:\SRKS_ifacility\ReportsList\" + System.DateTime.Now.ToString("yyyy-MM-dd");
            //String FileDir = @"C:\inetpub\ContiAndonWebApp\Reports\" + System.DateTime.Now.ToString("yyyy");

            bool exists = System.IO.Directory.Exists(FileDir);

            if (!exists)
                System.IO.Directory.CreateDirectory(FileDir);

            FileInfo newFile = new FileInfo(System.IO.Path.Combine(FileDir, "Shakti_MTTR_MTBF_Report" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //+ " to " + toda.ToString("yyyy-MM-dd") 
            if (newFile.Exists)
            {
                try
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(System.IO.Path.Combine(FileDir, "Shakti_MTTR_MTBF_Report" + frda.ToString("yyyy-MM-dd") + ".xlsx")); //" to " + toda.ToString("yyyy-MM-dd") + 
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
                worksheet = p.Workbook.Worksheets.Add(System.DateTime.Now.ToString("dd-MM-yyyy"), Templatews);
            }

            int sheetcount = p.Workbook.Worksheets.Count;
            p.Workbook.Worksheets.MoveToStart(sheetcount);

            worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            worksheet.Cells["D4"].Value = frmDate.ToString("dd-MM-yyyy");
            worksheet.Cells["H4"].Value = toDate.ToString("dd-MM-yyyy");

            string FDate = frmDate.ToString("yyyy-MM-dd");
            string TDate = toDate.ToString("yyyy-MM-dd");

           
            var machineids = Serverdb.tblmachinedetails.Where(m => m.IsDeleted==0).Select(m => m.MachineID).Distinct().ToList();

            if (ShopID == "" && CellID == "" && WorkCenterID == "" && PlantID != "")
            {
                string plantidd = PlantID;
                int plant11 = Convert.ToInt32(plantidd);
                machineids = Serverdb.tblmachinedetails.Where(m => m.PlantID == plant11 && m.IsDeleted == 0).Select(m => m.MachineID).Distinct().ToList();

            }
            else if (ShopID != "" && CellID == "" && WorkCenterID == "" && PlantID != "")
            {

                string shopidd = ShopID;
                int shopidd11 = Convert.ToInt32(shopidd);
                machineids = Serverdb.tblmachinedetails.Where(m => m.ShopID == shopidd11 && m.IsDeleted == 0).Select(m => m.MachineID).Distinct().ToList();

            }
            else if (ShopID != "" && CellID != "" && WorkCenterID == "" && PlantID != "")
            {
                string cellidd = CellID;
                int cellidd11 = Convert.ToInt32(cellidd);

                machineids = Serverdb.tblmachinedetails.Where(m => m.CellID == cellidd11 && m.IsDeleted == 0).Select(m => m.MachineID).Distinct().ToList();
            }

            else
            {
                string machineid = WorkCenterID;
                int machineidd = Convert.ToInt32(machineid);

                machineids = Serverdb.tblmachinedetails.Where(m => m.MachineID == machineidd && m.IsDeleted == 0).Select(m => m.MachineID).Distinct().ToList();
            }

            int Row = 6;
            int Sno = 1;

            foreach (var mid in machineids)
            {
                DataTable dataHolder = new DataTable();
                MsqlConnection mc = new MsqlConnection();
                mc.open();
                string sql1 = null;

                sql1 = "SELECT MachineID,StartTime,EndTime,BreakDownCode,CorrectedDate,Shift FROM tblbreakdown WHERE DoneWithRow = 1 AND CorrectedDate>='" + FDate + "' AND CorrectedDate<='" + TDate + "' and MachineID in (select MachineID from tblmachinedetails where MachineID = " + mid + " and IsDeleted = 0)  ORDER BY BreakdownID ASC";
               
                SqlDataAdapter da1 = new SqlDataAdapter(sql1, mc.msqlConnection);
                    da1.Fill(dataHolder);
                    mc.close();


                if (dataHolder.Rows.Count != 0)
                {
                    
                    decimal totaltime = 0;
                    decimal totaltime1 = 0;

                    int MachineID = Convert.ToInt32(dataHolder.Rows[0][0]);
                    List<string> HierarchyData = GetHierarchyData(MachineID);

                    worksheet.Cells["A" + Row].Value = Sno;
                    worksheet.Cells["B" + Row].Value = HierarchyData[0]; //Plant Name
                    worksheet.Cells["C" + Row].Value = HierarchyData[1]; // Shop Name
                    worksheet.Cells["D" + Row].Value = HierarchyData[2]; //Cell Name
                    worksheet.Cells["E" + Row].Value = HierarchyData[3]; //machine Name

                    for (int i = 0; i < dataHolder.Rows.Count; i++)
                    {

                        if (string.IsNullOrEmpty(dataHolder.Rows[i][1].ToString()) == false && string.IsNullOrEmpty(dataHolder.Rows[i][2].ToString()) == false)
                        {
                            DateTime StartTime = DateTime.Now;
                            StartTime = Convert.ToDateTime(dataHolder.Rows[i][1]);
                            DateTime EndTime = DateTime.Now;
                            EndTime = Convert.ToDateTime(dataHolder.Rows[i][2]);

                            TimeSpan ts = EndTime.Subtract(StartTime);

                            double tss = EndTime.Subtract(StartTime).TotalMinutes;
                            totaltime += Convert.ToDecimal(tss);

                            DateTime StartTime1 = DateTime.Now;

                            if (i < dataHolder.Rows.Count - 1)
                            {
                                StartTime1 = Convert.ToDateTime(dataHolder.Rows[i][2]);// 1st row end time
                                DateTime EndTime1 = DateTime.Now;
                                EndTime1 = Convert.ToDateTime(dataHolder.Rows[i+1][1]);  //2nd row start time

                                double tss1 = EndTime1.Subtract(StartTime1).TotalMinutes;
                                totaltime1 += Convert.ToDecimal(tss1);

                            }

                        }

                    }

                    int bkId = Convert.ToInt32(dataHolder.Rows[0][3]);
                   var  breakdownReason= Serverdb.tbllossescodes.Where(m => m.LossCodeID == bkId && m.IsDeleted == 0).Select(m => m.LossCode).FirstOrDefault();
                    worksheet.Cells["F" + Row].Value = breakdownReason;     //breakdown code reason



                    worksheet.Cells["G" + Row].Value = totaltime;                //total time
                    worksheet.Cells["H" + Row].Value = dataHolder.Rows.Count;    //total rows

                    decimal MTTRcal = totaltime / dataHolder.Rows.Count;        //MTTR calculation

                    worksheet.Cells["I" + Row].Value = MTTRcal;

                    decimal MTBFcal = totaltime1 / dataHolder.Rows.Count;      //MTBF calculation
                    worksheet.Cells["J" + Row].Value = MTBFcal;

                    Row++;
                    Sno++;

                }

            }


            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            p.Save();

            //Downloding Excel
            string path1 = System.IO.Path.Combine(FileDir, "Shakti_MTTR_MTBF_Report" + frda.ToString("yyyy-MM-dd") + ".xlsx");
            System.IO.FileInfo file1 = new System.IO.FileInfo(path1);
            string Outgoingfile = "Shakti_MTTR_MTBF_Report" + frda.ToString("yyyy-MM-dd") + ".xlsx";
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
            return View();
        }

    }

    public class PartSearchCreate
    {
        public List<int> MachineId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string FG_code { get; set; }
        public string correctedDate { get; set; }
    }

    public class CycleTiemDataGraph
    {
        public string fgcodOpno { get; set; }
        public int YieldQty { get; set; }
        public int ScrapQty { get; set; }
        public double TotalStdTime { get; set; }
        public int ActualTotalOperatingTime { get; set; }
    }

    
}

