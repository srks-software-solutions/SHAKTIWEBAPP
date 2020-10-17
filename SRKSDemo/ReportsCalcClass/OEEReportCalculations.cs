
using SRKSDemo.Models;
using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SRKSDemo.ReportsCalcClass
{
    public class OEEReportCalculations
    {
        i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1();

        public OEEReportCalculations() { }

        public bool OEE(int machineID, string correctdate)
        {
            // string correctdate = DateTime.Now.ToString("yyyy-MM-dd");
            DateTime correctedDate = Convert.ToDateTime(correctdate);
            int GetHour = System.DateTime.Now.Hour;
            DateTime StartModeTime = Convert.ToDateTime(System.DateTime.Now.ToString("yyyy-MM-dd " + GetHour + ":00:00"));
            double AvailabilityPercentage = 0;
            double PerformancePercentage = 0;
            double QualityPercentage = 0;
            double OEEPercentage = 0;


            decimal Utilization = 0;
            decimal DayOEEPercent = 0;
            int PerformanceFactor = 0;
            decimal Quality = 0;
            int TotlaQty = 0;
            decimal IdealCycleTimeVal = 0;

            OEECalModel OEECal = OEEDetails(machineID, correctedDate);
            decimal OperatingTime = OEECal.OperatingTime;
            decimal LossTime = OEECal.LossTime;
            decimal MinorLossTime = OEECal.MinorLossTime;
            decimal MntTime = OEECal.MntTime;
            decimal SetupTime = OEECal.SetupTime;
            //var scrap = new tblworkorderentry();
            var scrap = new tblhmiscreen();
            var scrapqty1 = new List<tblrejectqty>();
            int YeildQty = 0;
            int rejQty = 0;
            int reject = 0;
            //decimal SetupMinorTime = 0;
            decimal PowerOffTime = OEECal.PowerOffTime;
            decimal PowerONTime = OEECal.PowerONTime;
            int cuttingTime = 0;
            #region Commented
            //var machinedet = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.MachineID == machineID).FirstOrDefault();

            //var GetModeDurations = db.tblmodes.Where(m => m.MachineID == machineID && m.CorrectedDate == correctedDate.Date && m.IsCompleted == 1 && m.ModeTypeEnd == 1).ToList();
            //foreach (var ModeRow in GetModeDurations)
            //{
            //    //GetCorrectedDate = ModeRow.CorrectedDate;
            //    if (ModeRow.ModeType == "PROD")
            //    {
            //        OperatingTime += (decimal)(ModeRow.DurationInSec / 60.00);
            //    }
            //    else if (ModeRow.ModeType == "IDLE" && ModeRow.DurationInSec > 600)
            //    {
            //        LossTime += (decimal)(ModeRow.DurationInSec / 60.00);
            //        decimal LossDuration = (decimal)(ModeRow.DurationInSec / 60.00);
            //        //if (ModeRow.LossCodeID != null)
            //        // insertProdlosses(ProdRow.HMIID, (int)ModeRow.LossCodeID, LossDuration, CorrectedDate, MachineID);
            //    }
            //    else if (ModeRow.ModeType == "IDLE" && ModeRow.DurationInSec < 600)
            //    {
            //        MinorLossTime += (decimal)(ModeRow.DurationInSec / 60.00);
            //    }
            //    else if (ModeRow.ModeType == "MNT")
            //    {
            //        MntTime += (decimal)(ModeRow.DurationInSec / 60.00);
            //    }
            //    else if (ModeRow.ModeType == "POWEROFF")
            //    {
            //        PowerOffTime += (decimal)(ModeRow.DurationInSec / 60.00);
            //    }
            //    else if (ModeRow.ModeType == "SETUP")
            //    {
            //        try
            //        {
            //            SetupTime += (decimal)Convert.ToDateTime(ModeRow.LossCodeEnteredTime).Subtract(Convert.ToDateTime(ModeRow.StartTime)).TotalMinutes;
            //            //SetupMinorTime += (decimal)(db.tblSetupMaints.Where(m => m.ModeId == ModeRow.ModeId).Select(m => m.MinorLossTime).First() / 60.00);
            //        }
            //        catch { }
            //    }
            //    else if (ModeRow.ModeType == "POWERON")
            //    {
            //        PowerONTime += (decimal)(ModeRow.DurationInSec / 60.00);
            //    }
            //}
            #endregion

            //scrap = db.tblworkorderentries.Where(m => m.MachineID == machineID && m.CorrectedDate == correctedDate).OrderByDescending(m => m.HMIID).FirstOrDefault();

            scrap = db.tblhmiscreens.Where(m => m.MachineID == machineID && m.CorrectedDate == correctdate).OrderByDescending(m => m.HMIID).FirstOrDefault();


            TotlaQty = GetQuantiy(machineID, out YeildQty, correctedDate.ToString("yyyy-MM-dd"), out cuttingTime);

            var partsdet = db.tblparts.Where(m => m.MachineID == machineID).FirstOrDefault();
            if (partsdet != null)
            {
                IdealCycleTimeVal += Math.Round((partsdet.IdealCycleTime / 60), 2);
                int qty = Convert.ToInt32(partsdet.PartsPerCycle);
                //TotlaQty = TotlaQty * qty;
            }


            decimal IdleTime = LossTime + MinorLossTime;
            decimal BDTime = MntTime;

            if (scrap != null)
            {
                using (i_facility_shaktiEntities1 db = new i_facility_shaktiEntities1())
                {
                    scrapqty1 = db.tblrejectqties.Where(m => m.WOID == scrap.HMIID && m.CorrectedDate == correctdate).ToList();
                }

                foreach (var r1 in scrapqty1)
                {
                    reject = reject + Convert.ToInt32(r1.RejectQty);
                }

            }

            int TotalTime = Convert.ToInt32(PowerONTime) + Convert.ToInt32(OperatingTime) + Convert.ToInt32(IdleTime) + Convert.ToInt32(BDTime) + Convert.ToInt32(PowerOffTime);
            if (TotalTime == 0)
            {
                TotalTime = 1;
            }
            if (TotlaQty == 0)
                TotlaQty = 1;


            Utilization = Convert.ToInt32(Convert.ToDouble(Convert.ToDouble(Convert.ToDouble(OperatingTime) / Convert.ToDouble(TotalTime)) * 100));
            if (OperatingTime == 0)
                OperatingTime = 1;
            else
                OperatingTime = Math.Round(OperatingTime, 2);

            //double TotalTime1 = Convert.ToDouble(PowerONTime) + Convert.ToDouble(OperatingTime) + Convert.ToDouble(IdleTime) + Convert.ToDouble(BDTime) + Convert.ToDouble(PowerOffTime);
            if (YeildQty == 0)
                YeildQty = 1;


            //Quality = 100;
             Quality = Math.Round((decimal)((YeildQty - reject) / YeildQty), 2) * 100;

            decimal Performance = (decimal)Math.Round((((double)IdealCycleTimeVal * (double)TotlaQty) / (double)OperatingTime) * 100, 2);
            PerformanceFactor = (int)IdealCycleTimeVal * TotlaQty;

            if (PerformanceFactor == 0)
            {
                //PerformanceFactor = 100;
            }
            if (Quality > 0)
            {
               Quality = 100;
            }
            DayOEEPercent = (decimal)Math.Round((double)(Utilization / 100) * (double)(Performance / 100) * (double)(Quality / 100), 2) * 100;

            AvailabilityPercentage = (double)Utilization;
            QualityPercentage = (double)Quality;
            PerformancePercentage = (double)Performance;
            OEEPercentage = (double)DayOEEPercent;

            bool Res = InsertOEEDetails(correctdate, machineID, (decimal)AvailabilityPercentage, (decimal)QualityPercentage, (decimal)PerformancePercentage, (decimal)OEEPercentage, TotlaQty, OperatingTime, IdleTime, (decimal)PerformanceFactor);
            //}
            return Res;
        }

        private bool InsertOEEDetails(string CorrectedDate, int MachineID, decimal AvailabilityPercentage, decimal QualityPercentage, decimal PerformancePercentage, decimal OEEPercentage,
            int TotalPartsCount, decimal OperatingTime, decimal TotalIdleTimeInMin, decimal PerformanceFactor)
        {
            bool res = false;
            var oeedet = db.tbl_OEEDetails.Where(m => m.CorrectedDate == CorrectedDate && m.MachineID == MachineID).FirstOrDefault();
            if (oeedet == null)
            {
                tbl_OEEDetails oee = new tbl_OEEDetails();
                oee.MachineID = MachineID;
                oee.Availability = AvailabilityPercentage;
                oee.IsDeleted = 0;
                oee.OEE = OEEPercentage;
                oee.Performance = PerformancePercentage;
                oee.Quality = QualityPercentage;
                oee.TotalPartsCount = TotalPartsCount;
                oee.CreatedOn = DateTime.Now;
                oee.CreatedBy = 1;
                oee.CorrectedDate = CorrectedDate;
                oee.OperatingTimeinMin = OperatingTime;
                oee.TotalIDLETimeinMin = TotalIdleTimeInMin;
                oee.PerformanceFactor = PerformanceFactor;
                db.tbl_OEEDetails.Add(oee);
                db.SaveChanges();
                res = true;
            }
            return res;
        }

        private int GetQuantiy(int MachineID,out int YeildQty, string Correcteddate, out int CuttingTime)
        {
            int TotalQty = 0;
            YeildQty = 0;
            CuttingTime = 0;
            var parametermasterlistAll = new List<parameters_master>();
            DateTime CorrectedDate = Convert.ToDateTime(Correcteddate);
            string NxtCorrecteddate = CorrectedDate.AddDays(1).ToString("yyyy-MM-dd");
            //DateTime CorrectedDate = Convert.ToDateTime(Correcteddate);
            //string NxtCorrecteddate = CorrectedDate.AddDays(1).ToString("yyyy-MM-dd");

            string StartTime = Correcteddate + " 06:00:00";
            string EndTime = NxtCorrecteddate + " 06:00:00";

            DateTime St = Convert.ToDateTime(StartTime);
            DateTime Et = Convert.ToDateTime(EndTime);

            parametermasterlistAll = db.parameters_master.Where(m => m.CorrectedDate == CorrectedDate.Date && m.InsertedOn >= St && m.InsertedOn <= Et).ToList();
            var parametermasterlistLast = parametermasterlistAll.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate.Date && m.InsertedOn >= St && m.InsertedOn <= Et).ToList();
            var TopRowLast = parametermasterlistLast.OrderByDescending(m => m.ParameterID).FirstOrDefault();
            var RowLast = parametermasterlistLast.OrderBy(m => m.ParameterID).FirstOrDefault();

            if (TopRowLast != null && RowLast != null)
                YeildQty = Convert.ToInt32(TopRowLast.PartsTotal - RowLast.PartsTotal);

            // Based on 1st Machine
            var parametermasterlist = db.parameters_master.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate.Date && m.InsertedOn >= St && m.InsertedOn <= Et).ToList();
            var TopRow = parametermasterlist.OrderByDescending(m => m.ParameterID).FirstOrDefault();
            var LastRow = parametermasterlist.OrderBy(m => m.ParameterID).FirstOrDefault();
            if (TopRow != null && LastRow != null)
            {
                TotalQty = Convert.ToInt32(TopRow.PartsTotal - LastRow.PartsTotal);
                CuttingTime = Convert.ToInt32(TopRow.CuttingTime) - Convert.ToInt32(LastRow.CuttingTime);
            }




            return TotalQty;

        }


        public void GETCYCLETIMEAnalysis(int machineID, string correctdate)
        {
            DateTime correctedDate = Convert.ToDateTime(correctdate);

            decimal IdealCycleTimeVal = 0;

            decimal LoadingTime = 0;
            decimal Losses = 0;
            decimal SCT = 0;
            CTAModel objCTA = new CTAModel();

            //Get Prerequisites of OEE Like OPT,IDLETIME,POWEROFF, POWERON 
            OEECalModel OEECal = OEEDetails(machineID, correctedDate);
            decimal OperatingTime = OEECal.OperatingTime;
            decimal LossTime = OEECal.LossTime;
            decimal MinorLossTime = OEECal.MinorLossTime;
            decimal MntTime = OEECal.MntTime;
            decimal SetupTime = OEECal.SetupTime;
            //decimal SetupMinorTime = 0;
            decimal PowerOffTime = OEECal.PowerOffTime;
            decimal PowerONTime = OEECal.PowerONTime;
            int CuttingTime = 0;
            int YeildQty = 0;

            #region Commented
            //var machinedet = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.MachineID == machineID).FirstOrDefault();

            //var GetModeDurations = db.tblmodes.Where(m => m.MachineID == machineID && m.CorrectedDate == correctedDate.Date && m.IsCompleted == 1 && m.ModeTypeEnd == 1).ToList();
            //foreach (var ModeRow in GetModeDurations)
            //{
            //    //GetCorrectedDate = ModeRow.CorrectedDate;
            //    if (ModeRow.ModeType == "PROD")
            //    {
            //        OperatingTime += (decimal)(ModeRow.DurationInSec / 60.00);
            //    }
            //    else if (ModeRow.ModeType == "IDLE" && ModeRow.DurationInSec > 600)
            //    {
            //        LossTime += (decimal)(ModeRow.DurationInSec / 60.00);
            //        decimal LossDuration = (decimal)(ModeRow.DurationInSec / 60.00);
            //        //if (ModeRow.LossCodeID != null)
            //        // insertProdlosses(ProdRow.HMIID, (int)ModeRow.LossCodeID, LossDuration, CorrectedDate, MachineID);
            //    }
            //    else if (ModeRow.ModeType == "IDLE" && ModeRow.DurationInSec < 600)
            //    {
            //        MinorLossTime += (decimal)(ModeRow.DurationInSec / 60.00);
            //    }
            //    else if (ModeRow.ModeType == "MNT")
            //    {
            //        MntTime += (decimal)(ModeRow.DurationInSec / 60.00);
            //    }
            //    else if (ModeRow.ModeType == "POWEROFF")
            //    {
            //        PowerOffTime += (decimal)(ModeRow.DurationInSec / 60.00);
            //    }
            //    else if (ModeRow.ModeType == "SETUP")
            //    {
            //        try
            //        {
            //            SetupTime += (decimal)Convert.ToDateTime(ModeRow.LossCodeEnteredTime).Subtract(Convert.ToDateTime(ModeRow.StartTime)).TotalMinutes;
            //            //SetupMinorTime += (decimal)(db.tblSetupMaints.Where(m => m.ModeId == ModeRow.ModeId).Select(m => m.MinorLossTime).First() / 60.00);
            //        }
            //        catch { }
            //    }
            //    else if (ModeRow.ModeType == "POWERON")
            //    {
            //        PowerONTime += (decimal)(ModeRow.DurationInSec / 60.00);
            //    }
            //}
            #endregion

            int TotlaQty = GetQuantiy(machineID,out YeildQty, correctedDate.ToString("yyyy-MM-dd"), out CuttingTime);

            var partsdet = db.tblparts.Where(m => m.MachineID == machineID).FirstOrDefault();
            if (partsdet != null)
            {
                objCTA.PartNum = partsdet.FGCode;
                IdealCycleTimeVal = Math.Round((partsdet.IdealCycleTime / 60), 2);
                int qty = Convert.ToInt32(partsdet.PartsPerCycle);
                TotlaQty = TotlaQty * qty;
                decimal loadtime = Math.Round((decimal)(partsdet.Std_Load_UnloadTime / 60), 2);
                objCTA.Std_LoadTime = loadtime;
                LoadingTime = loadtime * TotlaQty;

                objCTA.CuttingTime = CuttingTime;
            }
            OperatingTime = Math.Round(OperatingTime, 2);
            objCTA.OperatingTime = OperatingTime;
            objCTA.MachineID = machineID;
            objCTA.PartsCount = TotlaQty;
            objCTA.Std_CycleTime = IdealCycleTimeVal;
            objCTA.TotalLoadTime = LoadingTime;
            if (TotlaQty == 0)
                TotlaQty = 1;
            objCTA.AvgOperatingTime = (OperatingTime / TotlaQty);
            objCTA.AvgLoadTimeinMinutes = (LoadingTime / TotlaQty);
            objCTA.AvgCuttingTime = (CuttingTime / TotlaQty);
            decimal IdleTime = LossTime + MinorLossTime;
            Losses = IdleTime - LoadingTime;
            objCTA.LossTime = Math.Round(Losses, 2);
            InsertCTA(machineID, correctdate, objCTA);
        }

        //Inserting Cutting Time Analysis Data
        private void InsertCTA(int MachineID, string CorrectedDate, CTAModel ctadet)
        {
            var CTADet = db.tbl_CycleTimeAnalysis.Where(m => m.CorrectedDate == CorrectedDate && m.MachineID == MachineID && m.IsDeleted == 0).FirstOrDefault();
            string Unit = "MINUTES";
            if (CTADet == null)
            {

                tbl_CycleTimeAnalysis ctarow = new tbl_CycleTimeAnalysis();
                ctarow.MachineID = MachineID;
                ctarow.CorrectedDate = CorrectedDate;
                ctarow.Std_CycleTime = ctadet.Std_CycleTime;
                ctarow.Std_CycleTimeUnit = Unit;
                ctarow.PartsCount = ctadet.PartsCount;
                ctarow.Std_LoadTime = ctadet.Std_LoadTime;
                ctarow.Std_LoadTimeUnit = Unit;
                ctarow.TotalLoadTime = ctadet.TotalLoadTime;
                ctarow.TotalLoadTimeUnit = Unit;
                ctarow.LossTime = ctadet.LossTime;
                ctarow.LossTimeUnit = Unit;
                ctarow.IsDeleted = 0;
                ctarow.CreatedOn = DateTime.Now;
                ctarow.PartNum = ctadet.PartNum;
                ctarow.OperatingTime = ctadet.OperatingTime;
                ctarow.OperatingTimeUnit = Unit;

                ctarow.AvgOperatingTime = ctadet.AvgOperatingTime;
                ctarow.AvgOperatingTimeUnit = Unit;

                ctarow.AvgLoadTimeinMinutes = ctadet.AvgLoadTimeinMinutes;
                db.tbl_CycleTimeAnalysis.Add(ctarow);
                db.SaveChanges();


            }
        }

        private OEECalModel OEEDetails(int machineID, DateTime correctedDate)
        {
            OEECalModel objCal = new OEECalModel();
            decimal OperatingTime = 0;
            decimal LossTime = 0;
            decimal MinorLossTime = 0;
            decimal MntTime = 0;
            decimal SetupTime = 0;
            decimal SetupMinorTime = 0;
            decimal PowerOffTime = 0;
            decimal PowerONTime = 0;
            var machinedet = db.tblmachinedetails.Where(m => m.IsDeleted == 0 && m.IsNormalWC == 0 && m.MachineID == machineID).FirstOrDefault();

            var GetModeDurations = db.tbllivemodes.Where(m => m.MachineID == machineID && m.CorrectedDate == correctedDate.Date && m.IsCompleted == 1 && m.ModeTypeEnd == 1).ToList();
            foreach (var ModeRow in GetModeDurations)
            {
                //GetCorrectedDate = ModeRow.CorrectedDate;
                if (ModeRow.ModeType == "PROD")
                {

                    OperatingTime += (decimal)(ModeRow.DurationInSec / 60.00);
                }
                else if (ModeRow.ModeType == "IDLE" && ModeRow.DurationInSec > 600)
                {
                    LossTime += (decimal)(ModeRow.DurationInSec / 60.00);
                    decimal LossDuration = (decimal)(ModeRow.DurationInSec / 60.00);
                    //if (ModeRow.LossCodeID != null)
                    // insertProdlosses(ProdRow.HMIID, (int)ModeRow.LossCodeID, LossDuration, CorrectedDate, MachineID);
                }
                else if (ModeRow.ModeType == "IDLE" && ModeRow.DurationInSec < 600)
                {
                    MinorLossTime += (decimal)(ModeRow.DurationInSec / 60.00);
                }
                else if (ModeRow.ModeType == "MNT")
                {
                    MntTime += (decimal)(ModeRow.DurationInSec / 60.00);
                }
                else if (ModeRow.ModeType == "POWEROFF")
                {
                    PowerOffTime += (decimal)(ModeRow.DurationInSec / 60.00);
                }
                else if (ModeRow.ModeType == "SETUP")
                {
                    try
                    {
                        SetupTime += (decimal)Convert.ToDateTime(ModeRow.LossCodeEnteredTime).Subtract(Convert.ToDateTime(ModeRow.StartTime)).TotalMinutes;
                        //SetupMinorTime += (decimal)(db.tblSetupMaints.Where(m => m.ModeId == ModeRow.ModeId).Select(m => m.MinorLossTime).First() / 60.00);
                    }
                    catch { }
                }
                else if (ModeRow.ModeType == "POWERON")
                {
                    PowerONTime += (decimal)(ModeRow.DurationInSec / 60.00);
                }

            }
            objCal.OperatingTime = OperatingTime;
            objCal.LossTime = LossTime;
            objCal.MinorLossTime = MinorLossTime;
            objCal.MntTime = MntTime;
            objCal.PowerOffTime = PowerOffTime;
            objCal.PowerONTime = PowerONTime;
            objCal.SetupTime = SetupTime;
            return objCal;
        }  
 
    }
}