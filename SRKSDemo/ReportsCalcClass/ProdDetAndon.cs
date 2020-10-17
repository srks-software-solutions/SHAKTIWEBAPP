using SRKSDemo.Server_Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SRKSDemo.ReportsCalcClass
{
    public class ProdDetAndon
    {
        i_facility_shaktiEntities1 Serverdb = new i_facility_shaktiEntities1();

        public ProdDetAndon()
        {

        }

       // DateTime CorrectedDate = Convert.ToDateTime("2020-07-13");
        public void insertProdDet(int MachineID, DateTime CorrectedDate)
        {
            try
            {
                String correcteddate = CorrectedDate.Date.ToString("yyyy-MM-dd");
                var getProdList = Serverdb.tbllivehmiscreens.Where(m => m.MachineID == MachineID && m.CorrectedDate == correcteddate).OrderBy(m => m.Date).ToList();

                foreach (var ProdRow in getProdList)
                {
                    var GetEntry = Serverdb.tbl_ProdAndonDisp.Where(m => m.WOID == ProdRow.HMIID).FirstOrDefault();
                    if (GetEntry == null)
                    {
                        DateTime ProdStartTime = Convert.ToDateTime(ProdRow.Date);
                        DateTime ProdEndtime = DateTime.Now;
                        try
                        {
                            if (ProdRow.Time.HasValue)
                            {
                                ProdEndtime = Convert.ToDateTime(ProdRow.Time);
                            }
                        }
                        catch { }

                        int OperatingTime = 0;
                        int LossTime = 0;
                        int MinorLossTime = 0;
                        int MntTime = 0;
                        int SetupTime = 0;
                        int SetupMinorTime = 0;
                        int PowerOffTime = 0;

                        #region Logic to get the Mode Durations between a Production Order which are completed
                        var GetModeDurations = Serverdb.tbllivemodes.Where(m => m.MachineID == MachineID && m.StartTime >= ProdStartTime && m.StartTime < ProdEndtime && m.EndTime > ProdStartTime && m.EndTime < ProdEndtime && m.IsCompleted == 1 && m.ModeTypeEnd == 1).ToList();
                        foreach (var ModeRow in GetModeDurations)
                        {
                            if (ModeRow.ModeType == "PROD")
                            {
                                OperatingTime += (int)(ModeRow.DurationInSec / 60);
                            }
                            else if (ModeRow.ModeType == "IDLE" && ModeRow.DurationInSec > 600)
                            {
                                LossTime += (int)(ModeRow.DurationInSec / 60);
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
                                    SetupTime += (int)(Convert.ToDateTime(ModeRow.LossCodeEnteredTime).Subtract(Convert.ToDateTime(ModeRow.StartTime)).TotalSeconds / 60);
                                    SetupMinorTime += (int)(Serverdb.tblSetupMaints.Where(m => m.ModeID == ModeRow.ModeID).Select(m => m.MinorLossTime).First() / 60);
                                }
                                catch { }
                            }
                        }
                        #endregion

                        #region Logic to get the Mode Duration if the Production is still Running.
                        if (ProdRow.isWorkInProgress == 2)
                        {
                            var getRunningMode = Serverdb.tbllivemodes.Where(m => m.MachineID == MachineID && m.IsCompleted == 0).OrderByDescending(m => m.ModeID).First();
                            if (getRunningMode.ModeType == "PROD")
                            {
                                OperatingTime += (int)(DateTime.Now.Subtract(Convert.ToDateTime(getRunningMode.StartTime)).TotalSeconds / 60);
                            }
                            else if (getRunningMode.ModeType == "IDLE")
                            {
                                LossTime += (int)(DateTime.Now.Subtract(Convert.ToDateTime(getRunningMode.StartTime)).TotalSeconds / 60);
                            }
                            else if (getRunningMode.ModeType == "MNT")
                            {
                                MntTime += (int)(DateTime.Now.Subtract(Convert.ToDateTime(getRunningMode.StartTime)).TotalSeconds / 60);
                            }
                            else if (getRunningMode.ModeType == "POWEROFF")
                            {
                                PowerOffTime += (int)(DateTime.Now.Subtract(Convert.ToDateTime(getRunningMode.StartTime)).TotalSeconds / 60);
                            }
                            else if (getRunningMode.ModeType == "SETUP")
                            {
                                try
                                {
                                    SetupTime += (int)(Convert.ToDateTime(getRunningMode.LossCodeEnteredTime).Subtract(Convert.ToDateTime(getRunningMode.StartTime)).TotalSeconds / 60);
                                    SetupMinorTime += (int)(Serverdb.tblSetupMaints.Where(m => m.ModeID == getRunningMode.ModeID).Select(m => m.MinorLossTime).First() / 60);
                                }
                                catch { }
                            }
                        }
                        #endregion

                        #region Logic to get the Mode Duration Which Was started before this Production and Ended during this Production
                        var GetEndModeDuration = Serverdb.tbllivemodes.Where(m => m.MachineID == MachineID && m.StartTime < ProdStartTime && m.EndTime > ProdStartTime && m.EndTime < ProdEndtime && m.IsCompleted == 1 && m.ModeTypeEnd == 1).FirstOrDefault();
                        if (GetEndModeDuration != null)
                        {
                            if (GetEndModeDuration.ModeType == "PROD")
                            {
                                OperatingTime += (int)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                            }
                            else if (GetEndModeDuration.ModeType == "IDLE")
                            {
                                LossTime += (int)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                            }
                            else if (GetEndModeDuration.ModeType == "MNT")
                            {
                                MntTime += (int)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                            }
                            else if (GetEndModeDuration.ModeType == "POWEROFF")
                            {
                                PowerOffTime += (int)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                            }
                            else if (GetEndModeDuration.ModeType == "SETUP")
                            {
                                try
                                {
                                    SetupTime += (int)(Convert.ToDateTime(GetEndModeDuration.LossCodeEnteredTime).Subtract(Convert.ToDateTime(GetEndModeDuration.StartTime)).TotalSeconds / 60);
                                    SetupMinorTime += (int)(Serverdb.tblSetupMaints.Where(m => m.ModeID == GetEndModeDuration.ModeID).Select(m => m.MinorLossTime).First() / 60);
                                }
                                catch { }
                            }
                        }
                        #endregion

                        #region Logic to get the Mode Duration Which Was Started during the Production and Ended after the Production
                        var GetStartModeDuration = Serverdb.tbllivemodes.Where(m => m.MachineID == MachineID && m.StartTime >= ProdStartTime && m.EndTime >= ProdStartTime && m.StartTime < ProdEndtime && m.EndTime > ProdEndtime && m.IsCompleted == 1 && m.ModeTypeEnd == 1).FirstOrDefault();
                        if (GetStartModeDuration != null)
                        {
                            if (GetStartModeDuration.ModeType == "PROD")
                            {
                                OperatingTime += (int)(Convert.ToDateTime(GetStartModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                            }
                            else if (GetStartModeDuration.ModeType == "IDLE")
                            {
                                LossTime += (int)(Convert.ToDateTime(GetStartModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                            }
                            else if (GetStartModeDuration.ModeType == "MNT")
                            {
                                MntTime += (int)(Convert.ToDateTime(GetStartModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                            }
                            else if (GetStartModeDuration.ModeType == "POWEROFF")
                            {
                                PowerOffTime += (int)(Convert.ToDateTime(GetStartModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                            }
                            else if (GetStartModeDuration.ModeType == "SETUP")
                            {
                                try
                                {
                                    SetupTime += (int)(Convert.ToDateTime(GetStartModeDuration.LossCodeEnteredTime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60);
                                    SetupMinorTime += (int)(Serverdb.tblSetupMaints.Where(m => m.ModeID == GetStartModeDuration.ModeID).Select(m => m.MinorLossTime).First() / 60);
                                }
                                catch { }
                            }
                        }
                        #endregion

                        double TotalTime = ProdEndtime.Subtract(ProdStartTime).TotalMinutes;
                        decimal UtilPercent = (decimal)Math.Round(OperatingTime / TotalTime * 100, 2);
                        if (UtilPercent > 100)
                        {
                            UtilPercent = 100;
                        }

                        tbl_ProdAndonDisp PRA = new tbl_ProdAndonDisp();
                        PRA.MachineID = MachineID;
                        PRA.WOID = ProdRow.HMIID;
                        PRA.CorrectedDate = CorrectedDate.Date;
                        PRA.TotalLoss = LossTime + MinorLossTime - SetupMinorTime;
                        PRA.TotalOperatingTime = OperatingTime;
                        PRA.TotalSetup = SetupTime + SetupMinorTime;
                        PRA.UtilPercent = UtilPercent;
                        PRA.InsertedOn = DateTime.Now;
                        Serverdb.tbl_ProdAndonDisp.Add(PRA);
                        Serverdb.SaveChanges();
                    }
                    else if (GetEntry.tbllivehmiscreen.isWorkInProgress != 2)
                    {
                        DateTime ProdStartTime = Convert.ToDateTime(ProdRow.Date);
                        DateTime ProdEndtime = DateTime.Now;
                        try
                        {
                            if (ProdRow.Time.HasValue)
                            {
                                ProdEndtime = Convert.ToDateTime(ProdRow.Time);
                            }
                        }
                        catch { }

                        decimal OperatingTime = 0;
                        decimal LossTime = 0;
                        decimal MinorLossTime = 0;
                        decimal MntTime = 0;
                        decimal SetupTime = 0;
                        decimal SetupMinorTime = 0;
                        decimal PowerOffTime = 0;

                        #region Logic to get the Mode Durations between a Production Order which are completed
                        var GetModeDurations = Serverdb.tbllivemodes.Where(m => m.MachineID == MachineID && m.StartTime >= ProdStartTime && m.StartTime <= ProdEndtime && m.EndTime >= ProdStartTime && m.EndTime <= ProdEndtime && m.IsCompleted == 1 && m.ModeTypeEnd == 1).ToList();
                        foreach (var ModeRow in GetModeDurations)
                        {
                            if (ModeRow.ModeType == "PROD")
                            {
                                OperatingTime += (decimal)(ModeRow.DurationInSec / 60.00);
                            }
                            else if (ModeRow.ModeType == "IDLE" && ModeRow.DurationInSec > 600)
                            {
                                LossTime += (decimal)(ModeRow.DurationInSec / 60.00);
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
                                    SetupTime += (decimal)(Convert.ToDateTime(ModeRow.LossCodeEnteredTime).Subtract(Convert.ToDateTime(ModeRow.StartTime)).TotalSeconds / 60);
                                    SetupMinorTime += (decimal)(Serverdb.tblSetupMaints.Where(m => m.ModeID == ModeRow.ModeID).Select(m => m.MinorLossTime).First() / 60);
                                }
                                catch { }
                            }
                        }
                        #endregion

                        #region Logic to get the Mode Duration if the Production is still Running.
                        if (ProdRow.isWorkInProgress == 2)
                        {
                            var getRunningMode = Serverdb.tbllivemodes.Where(m => m.MachineID == MachineID && m.IsCompleted == 0).OrderByDescending(m => m.ModeID).First();
                            if (getRunningMode.ModeType == "PROD")
                            {
                                OperatingTime += (decimal)(DateTime.Now.Subtract(Convert.ToDateTime(getRunningMode.StartTime)).TotalSeconds / 60);
                            }
                            else if (getRunningMode.ModeType == "IDLE")
                            {
                                LossTime += (decimal)(DateTime.Now.Subtract(Convert.ToDateTime(getRunningMode.StartTime)).TotalSeconds / 60);
                            }
                            else if (getRunningMode.ModeType == "MNT")
                            {
                                MntTime += (decimal)(DateTime.Now.Subtract(Convert.ToDateTime(getRunningMode.StartTime)).TotalSeconds / 60);
                            }
                            else if (getRunningMode.ModeType == "POWEROFF")
                            {
                                PowerOffTime += (decimal)(DateTime.Now.Subtract(Convert.ToDateTime(getRunningMode.StartTime)).TotalSeconds / 60);
                            }
                        }
                        #endregion

                        #region Logic to get the Mode Duration Which Was started before this Production and Ended during this Production
                        var GetEndModeDuration = Serverdb.tbllivemodes.Where(m => m.MachineID == MachineID && m.StartTime < ProdStartTime && m.EndTime >= ProdStartTime && m.EndTime <= ProdEndtime && m.IsCompleted == 1 && m.ModeTypeEnd == 1).FirstOrDefault();
                        if (GetEndModeDuration != null)
                        {
                            if (GetEndModeDuration.ModeType == "PROD")
                            {
                                OperatingTime += (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                            }
                            else if (GetEndModeDuration.ModeType == "IDLE")
                            {
                                LossTime += (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                            }
                            else if (GetEndModeDuration.ModeType == "MNT")
                            {
                                MntTime += (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                            }
                            else if (GetEndModeDuration.ModeType == "POWEROFF")
                            {
                                PowerOffTime += (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                            }
                        }
                        #endregion

                        #region Logic to get the Mode Duration Which Was Started during the Production and Ended Before the Production End
                        var GetStartModeDuration = Serverdb.tbllivemodes.Where(m => m.MachineID == MachineID && m.StartTime >= ProdStartTime && m.EndTime >= ProdStartTime && m.StartTime <= ProdEndtime && m.EndTime > ProdEndtime && m.IsCompleted == 1 && m.ModeTypeEnd == 1).FirstOrDefault();
                        if (GetStartModeDuration != null)
                        {
                            if (GetStartModeDuration.ModeType == "PROD")
                            {
                                OperatingTime += (decimal)(Convert.ToDateTime(GetStartModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                            }
                            else if (GetStartModeDuration.ModeType == "IDLE")
                            {
                                LossTime += (decimal)(Convert.ToDateTime(GetStartModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                            }
                            else if (GetStartModeDuration.ModeType == "MNT")
                            {
                                MntTime += (decimal)(Convert.ToDateTime(GetStartModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                            }
                            else if (GetStartModeDuration.ModeType == "POWEROFF")
                            {
                                PowerOffTime += (decimal)(Convert.ToDateTime(GetStartModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60);
                            }
                        }
                        #endregion

                        double TotalTime = ProdEndtime.Subtract(ProdStartTime).TotalMinutes;
                        decimal UtilPercent = (Decimal)Math.Round((double)OperatingTime / TotalTime * 100, 2);
                        if (UtilPercent > 100)
                        {
                            UtilPercent = 100;
                        }

                        tbl_ProdAndonDisp PRA = Serverdb.tbl_ProdAndonDisp.Where(m => m.ProdDashboardID == GetEntry.ProdDashboardID).First();
                        //PRA.MachineID = MachineID;
                        //PRA.WOID = ProdRow.HMIID;
                        //PRA.CorrectedDate = CorrectedDate.Date;
                        PRA.TotalLoss = (Decimal)Math.Round(LossTime + MinorLossTime - SetupMinorTime, 2);
                        PRA.TotalOperatingTime = (Decimal)Math.Round(OperatingTime, 2);
                        PRA.TotalSetup = (Decimal)Math.Round(SetupTime + SetupMinorTime, 2);
                        PRA.UtilPercent = UtilPercent;
                        Serverdb.Entry(PRA).State = System.Data.Entity.EntityState.Modified;
                        Serverdb.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        // Get Production Data for Man Machine Ticket.
        //public void insertManMacProd(int MachineID, DateTime CorrectedDate)
        //{
        //    decimal OperatingTime = 0;
        //    decimal LossTime = 0;
        //    decimal MinorLossTime = 0;
        //    decimal MntTime = 0;
        //    decimal SetupTime = 0;
        //    decimal SetupMinorTime = 0;
        //    decimal PowerOffTime = 0;
        //    try
        //    {
        //        var getProdList = Serverdb.tblworkorderentries.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate.Date).OrderBy(m => m.WOStart).ToList();
        //        if (getProdList.Count != 0)
        //        {
        //            foreach (var ProdRow in getProdList)
        //            {
        //                var GetEntry = Serverdb.tbl_ProdManMachine.Where(m => m.WOID == ProdRow.HMIID).FirstOrDefault();
        //                if (GetEntry == null)
        //                {
        //                    DateTime ProdStartTime = ProdRow.WOStart;
        //                    DateTime ProdEndtime = DateTime.Now;
        //                    try
        //                    {
        //                        if (ProdRow.WOEnd.HasValue)
        //                        {
        //                            ProdEndtime = Convert.ToDateTime(ProdRow.WOEnd);
        //                        }
        //                    }
        //                    catch { }



        //                    #region Logic to get the Mode Durations between a Production Order which are completed
        //                    var GetModeDurations = Serverdb.tbllivemodes.Where(m => m.MachineID == MachineID && m.StartTime >= ProdStartTime && m.StartTime < ProdEndtime && m.EndTime > ProdStartTime && m.EndTime <= ProdEndtime && m.IsCompleted == 1 && m.ModeTypeEnd == 1).ToList();
        //                    foreach (var ModeRow in GetModeDurations)
        //                    {
        //                        if (ModeRow.ModeType == "PROD")
        //                        {
        //                            OperatingTime += (decimal)(ModeRow.DurationInSec / 60.00);
        //                        }
        //                        else if (ModeRow.ModeType == "IDLE" && ModeRow.DurationInSec > 600)
        //                        {
        //                            LossTime += (decimal)(ModeRow.DurationInSec / 60.00);
        //                            decimal LossDuration = (decimal)(ModeRow.DurationInSec / 60.00);
        //                            if (ModeRow.LossCodeID != null)
        //                                insertProdlosses(ProdRow.HMIID, (int)ModeRow.LossCodeID, LossDuration, CorrectedDate, MachineID);
        //                        }
        //                        else if (ModeRow.ModeType == "IDLE" && ModeRow.DurationInSec < 600)
        //                        {
        //                            MinorLossTime += (decimal)(ModeRow.DurationInSec / 60.00);
        //                        }
        //                        else if (ModeRow.ModeType == "MNT")
        //                        {
        //                            MntTime += (decimal)(ModeRow.DurationInSec / 60.00);
        //                        }
        //                        else if (ModeRow.ModeType == "POWEROFF")
        //                        {
        //                            PowerOffTime += (decimal)(ModeRow.DurationInSec / 60.00);
        //                        }
        //                        else if (ModeRow.ModeType == "SETUP")
        //                        {
        //                            try
        //                            {
        //                                SetupTime += (decimal)Convert.ToDateTime(ModeRow.LossCodeEnteredTime).Subtract(Convert.ToDateTime(ModeRow.StartTime)).TotalMinutes;
        //                                SetupMinorTime += (decimal)(Serverdb.tblSetupMaints.Where(m => m.ModeID == ModeRow.ModeID).Select(m => m.MinorLossTime).First() / 60.00);
        //                            }
        //                            catch { }
        //                        }
        //                    }
        //                    #endregion

        //                    #region Logic to get the Mode Duration Which Was started before this Production and Ended during this Production
        //                    var GetEndModeDuration = Serverdb.tbllivemodes.Where(m => m.MachineID == MachineID && m.StartTime < ProdStartTime && m.EndTime > ProdStartTime && m.EndTime <= ProdEndtime && m.IsCompleted == 1 && m.ModeTypeEnd == 1).FirstOrDefault();
        //                    if (GetEndModeDuration != null)
        //                    {
        //                        if (GetEndModeDuration.ModeType == "PROD")
        //                        {
        //                            OperatingTime += (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60.00);
        //                        }
        //                        else if (GetEndModeDuration.ModeType == "IDLE")
        //                        {
        //                            LossTime += (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60.00);
        //                            decimal LossDuration = (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60.00);
        //                            if (GetEndModeDuration.LossCodeID != null)
        //                                insertProdlosses(ProdRow.HMIID, (int)GetEndModeDuration.LossCodeID, LossDuration, CorrectedDate, MachineID);
        //                            //insertProdlosses(WOID, LossID, LossDuration, CorrectedDate);
        //                        }
        //                        else if (GetEndModeDuration.ModeType == "MNT")
        //                        {
        //                            MntTime += (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60.00);
        //                        }
        //                        else if (GetEndModeDuration.ModeType == "POWEROFF")
        //                        {
        //                            PowerOffTime += (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60.00);
        //                        }
        //                    }
        //                    #endregion

        //                    #region Logic to get the Mode Duration Which Was Started during the Production and Ended after the Production
        //                    var GetStartModeDuration = Serverdb.tbllivemodes.Where(m => m.MachineID == MachineID && m.StartTime >= ProdStartTime && m.EndTime >= ProdStartTime && m.StartTime < ProdEndtime && m.EndTime > ProdEndtime && m.IsCompleted == 1 && m.ModeTypeEnd == 1).FirstOrDefault();
        //                    if (GetStartModeDuration != null)
        //                    {
        //                        if (GetStartModeDuration.ModeType == "PROD")
        //                        {
        //                            OperatingTime += (decimal)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60.00);
        //                        }
        //                        else if (GetStartModeDuration.ModeType == "IDLE")
        //                        {
        //                            LossTime += (decimal)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60.00);
        //                            decimal LossDuration = (decimal)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60.00);
        //                            if (GetStartModeDuration.LossCodeID != null)
        //                                insertProdlosses(ProdRow.HMIID, (int)GetStartModeDuration.LossCodeID, LossDuration, CorrectedDate, MachineID);
        //                            //insertProdlosses(WOID, LossID, LossDuration, CorrectedDate);
        //                        }
        //                        else if (GetStartModeDuration.ModeType == "MNT")
        //                        {
        //                            MntTime += (decimal)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60.00);
        //                        }
        //                        else if (GetStartModeDuration.ModeType == "POWEROFF")
        //                        {
        //                            PowerOffTime += (decimal)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60.00);
        //                        }
        //                    }
        //                    #endregion

        //                    int TotlaQty = ProdRow.Total_Qty;
        //                    if (TotlaQty == 0)
        //                        TotlaQty = 1;
        //                    decimal GetOptime = OperatingTime;
        //                    if (GetOptime == 0)
        //                    {
        //                        GetOptime = 1;
        //                    }
        //                    decimal IdealCycleTimeVal = 0;
        //                    var IdealCycTime = Serverdb.tblparts.Where(m => m.FGCode == ProdRow.FGCode && m.OperationNo == ProdRow.OperationNo).FirstOrDefault();
        //                    if (IdealCycTime != null)
        //                        IdealCycleTimeVal = IdealCycTime.IdealCycleTime;
        //                    double TotalTime = ProdEndtime.Subtract(ProdStartTime).TotalMinutes;
        //                    decimal UtilPercent = (decimal)Math.Round((double)OperatingTime / TotalTime * 100, 2);
        //                    decimal Quality = (decimal)Math.Round((double)ProdRow.Yield_Qty / TotlaQty * 100, 2);
        //                    decimal Performance = (decimal)Math.Round((double)IdealCycleTimeVal * (double)ProdRow.Total_Qty / (double)GetOptime * 100, 2);
        //                    int PerformanceFactor = (int)IdealCycleTimeVal * ProdRow.Total_Qty;
        //                    tbl_ProdManMachine PRA = new tbl_ProdManMachine();
        //                    PRA.MachineID = MachineID;
        //                    PRA.WOID = ProdRow.HMIID;
        //                    PRA.CorrectedDate = CorrectedDate.Date;
        //                    PRA.TotalLoss = LossTime;
        //                    PRA.TotalOperatingTime = Math.Round(OperatingTime, 2);
        //                    PRA.TotalSetup = Math.Round(SetupTime + SetupMinorTime, 2);
        //                    PRA.TotalMinorLoss = Math.Round(MinorLossTime - SetupMinorTime, 2);
        //                    PRA.TotalSetupMinorLoss = Math.Round(SetupMinorTime, 2);
        //                    PRA.TotalPowerLoss = Math.Round(PowerOffTime, 2);
        //                    PRA.UtilPercent = Math.Round(UtilPercent, 2);
        //                    PRA.QualityPercent = Math.Round(Quality, 2);
        //                    PRA.PerformancePerCent = Math.Round(Performance, 2);
        //                    PRA.PerfromaceFactor = PerformanceFactor;
        //                    PRA.InsertedOn = DateTime.Now;
        //                    Serverdb.tbl_ProdManMachine.Add(PRA);
        //                    Serverdb.SaveChanges();
        //                }
        //            }
        //        }
        //        else
        //        {
        //            var prodman = Serverdb.tbl_ProdManMachine.Where(m => m.CorrectedDate == CorrectedDate && m.MachineID == MachineID).ToList();
        //            if (prodman.Count == 0)
        //            {
        //                var GetModeDurations = Serverdb.tbllivemodes.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate && m.IsCompleted == 1 && m.ModeTypeEnd == 1).ToList();
        //                foreach (var ModeRow in GetModeDurations)
        //                {
        //                    if (ModeRow.ModeType == "PROD")
        //                    {
        //                        OperatingTime += (decimal)(ModeRow.DurationInSec / 60.00);
        //                    }
        //                    else if (ModeRow.ModeType == "IDLE" && ModeRow.DurationInSec > 600)
        //                    {
        //                        LossTime += (decimal)(ModeRow.DurationInSec / 60.00);
        //                        decimal LossDuration = (decimal)(ModeRow.DurationInSec / 60.00);
        //                        //if (ModeRow.LossCodeID != null)
        //                        //    insertProdlosses(ProdRow.HMIID, (int)ModeRow.LossCodeID, LossDuration, CorrectedDate, MachineID);
        //                    }
        //                    else if (ModeRow.ModeType == "IDLE" && ModeRow.DurationInSec < 600)
        //                    {
        //                        MinorLossTime += (decimal)(ModeRow.DurationInSec / 60.00);
        //                    }
        //                    else if (ModeRow.ModeType == "MNT")
        //                    {
        //                        MntTime += (decimal)(ModeRow.DurationInSec / 60.00);
        //                    }
        //                    else if (ModeRow.ModeType == "POWEROFF")
        //                    {
        //                        PowerOffTime += (decimal)(ModeRow.DurationInSec / 60.00);
        //                    }
        //                    else if (ModeRow.ModeType == "SETUP")
        //                    {
        //                        try
        //                        {
        //                            SetupTime += (decimal)Convert.ToDateTime(ModeRow.LossCodeEnteredTime).Subtract(Convert.ToDateTime(ModeRow.StartTime)).TotalMinutes;
        //                            SetupMinorTime += (decimal)(Serverdb.tblSetupMaints.Where(m => m.ModeID == ModeRow.ModeID).Select(m => m.MinorLossTime).First() / 60.00);
        //                        }
        //                        catch { }
        //                    }
        //                }

        //                //#region Logic to get the Mode Duration Which Was started before this Production and Ended during this Production
        //                //var GetEndModeDuration = Serverdb.tbllivemodes.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate && m.IsCompleted == 1 && m.ModeTypeEnd == 1).FirstOrDefault();
        //                //if (GetEndModeDuration != null)
        //                //{
        //                //    if (GetEndModeDuration.ModeType == "PROD")
        //                //    {
        //                //        OperatingTime += (decimal)(ModeRow.DurationInSec / 60.00);
        //                //        // OperatingTime += (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60.00);
        //                //    }
        //                //    else if (GetEndModeDuration.ModeType == "IDLE")
        //                //    {
        //                //        LossTime += (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60.00);
        //                //        decimal LossDuration = (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60.00);
        //                //        if (GetEndModeDuration.LossCodeID != null)
        //                //            insertProdlosses(ProdRow.HMIID, (int)GetEndModeDuration.LossCodeID, LossDuration, CorrectedDate, MachineID);
        //                //        //insertProdlosses(WOID, LossID, LossDuration, CorrectedDate);
        //                //    }
        //                //    else if (GetEndModeDuration.ModeType == "MNT")
        //                //    {
        //                //        MntTime += (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60.00);
        //                //    }
        //                //    else if (GetEndModeDuration.ModeType == "POWEROFF")
        //                //    {
        //                //        PowerOffTime += (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60.00);
        //                //    }
        //                //}
        //                //#endregion

        //                //#region Logic to get the Mode Duration Which Was Started during the Production and Ended after the Production
        //                //var GetStartModeDuration = Serverdb.tbllivemodes.Where(m => m.MachineID == MachineID && m.StartTime >= ProdStartTime && m.EndTime >= ProdStartTime && m.StartTime < ProdEndtime && m.EndTime > ProdEndtime && m.IsCompleted == 1 && m.ModeTypeEnd == 1).FirstOrDefault();
        //                //if (GetStartModeDuration != null)
        //                //{
        //                //    if (GetStartModeDuration.ModeType == "PROD")
        //                //    {
        //                //        OperatingTime += (decimal)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60.00);
        //                //    }
        //                //    else if (GetStartModeDuration.ModeType == "IDLE")
        //                //    {
        //                //        LossTime += (decimal)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60.00);
        //                //        decimal LossDuration = (decimal)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60.00);
        //                //        if (GetStartModeDuration.LossCodeID != null)
        //                //            insertProdlosses(ProdRow.HMIID, (int)GetStartModeDuration.LossCodeID, LossDuration, CorrectedDate, MachineID);
        //                //        //insertProdlosses(WOID, LossID, LossDuration, CorrectedDate);
        //                //    }
        //                //    else if (GetStartModeDuration.ModeType == "MNT")
        //                //    {
        //                //        MntTime += (decimal)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60.00);
        //                //    }
        //                //    else if (GetStartModeDuration.ModeType == "POWEROFF")
        //                //    {
        //                //        PowerOffTime += (decimal)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60.00);
        //                //    }
        //                //}
        //                //#endregion
        //                int partscount = 0;
        //                var partcount = Serverdb.tblpartscountandcuttings.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate).ToList();
        //                if (partcount != null)
        //                {
        //                    foreach (var partcountdet in partcount)
        //                    {
        //                        partscount = partscount + partcountdet.PartCount;
        //                    }
        //                    double Total_Qty = partscount;
        //                    int TotlaQty = (int)Total_Qty;
        //                    if (TotlaQty == 0)
        //                        TotlaQty = 1;
        //                    decimal GetOptime = OperatingTime;
        //                    if (GetOptime == 0)
        //                    {
        //                        GetOptime = 1;
        //                    }
        //                    decimal IdealCycleTimeVal = 60;
        //                    double TotalTime = 24;
        //                    //double TotalTime = ProdEndtime.Subtract(ProdStartTime).TotalMinutes;
        //                    decimal UtilPercent = (decimal)Math.Round((double)OperatingTime / TotalTime * 100, 2) / 100;
        //                    //decimal Quality = (decimal)Math.Round((double)ProdRow.Yield_Qty / TotlaQty * 100, 2);
        //                    decimal Quality = 100;
        //                    decimal Performance = (decimal)Math.Round((double)IdealCycleTimeVal * Total_Qty / (double)GetOptime * 100, 2)/100;
        //                    int PerformanceFactor = (int)IdealCycleTimeVal * (int)Total_Qty;
        //                    tbl_ProdManMachine PRA = new tbl_ProdManMachine();
        //                    PRA.MachineID = MachineID;
        //                    //PRA.WOID = ProdRow.HMIID;
        //                    PRA.WOID = 8;
        //                    PRA.CorrectedDate = CorrectedDate.Date;
        //                    PRA.TotalLoss = LossTime;
        //                    PRA.TotalOperatingTime = OperatingTime;
        //                    PRA.TotalSetup = Math.Round(SetupTime + SetupMinorTime, 2);
        //                    PRA.TotalSetup = Math.Round(SetupTime + SetupMinorTime, 2);
        //                    PRA.TotalMinorLoss = Math.Round(MinorLossTime - SetupMinorTime, 2);
        //                    PRA.TotalSetupMinorLoss = Math.Round(SetupMinorTime, 2);
        //                    PRA.TotalPowerLoss = Math.Round(PowerOffTime, 2);
        //                    PRA.UtilPercent = Math.Round(UtilPercent, 2);
        //                    PRA.QualityPercent = Math.Round(Quality, 2);
        //                    PRA.PerformancePerCent = Math.Round(Performance, 2);
        //                    PRA.PerfromaceFactor = PerformanceFactor;
        //                    PRA.InsertedOn = DateTime.Now;
        //                    Serverdb.tbl_ProdManMachine.Add(PRA);
        //                    Serverdb.SaveChanges();
        //                }
        //                else { }
        //            }
        //            else { }
        //        }

        //    }

        //    catch (Exception e)
        //    {
        //    }
        //}


        public void insertManMacProd(int MachineID, DateTime CorrectedDate)
        {
            decimal OperatingTime = 0;
            decimal LossTime = 0;
            decimal MinorLossTime = 0;
            decimal MntTime = 0;
            decimal SetupTime = 0;
            decimal SetupMinorTime = 0;
            decimal PowerOffTime = 0;
            try
            {
                var getProdList = Serverdb.tblworkorderentries.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate.Date).OrderBy(m => m.WOStart).ToList();
                if (getProdList.Count != 0)
                {
                    foreach (var ProdRow in getProdList)
                    {
                        var GetEntry = Serverdb.tbl_ProdManMachine.Where(m => m.WOID == ProdRow.HMIID).FirstOrDefault();
                        if (GetEntry == null)
                        {
                            DateTime ProdStartTime = ProdRow.WOStart;
                            DateTime ProdEndtime = DateTime.Now;
                            try
                            {
                                if (ProdRow.WOEnd.HasValue)
                                {
                                    ProdEndtime = Convert.ToDateTime(ProdRow.WOEnd);
                                }
                            }
                            catch { }



                            #region Logic to get the Mode Durations between a Production Order which are completed
                            var GetModeDurations = Serverdb.tbllivemodes.Where(m => m.MachineID == MachineID && m.StartTime >= ProdStartTime && m.StartTime < ProdEndtime && m.EndTime > ProdStartTime && m.EndTime <= ProdEndtime && m.IsCompleted == 1 && m.ModeTypeEnd == 1).ToList();
                            foreach (var ModeRow in GetModeDurations)
                            {
                                if (ModeRow.ModeType == "PROD")
                                {
                                    OperatingTime += (decimal)(ModeRow.DurationInSec / 60.00);
                                }
                                else if (ModeRow.ModeType == "IDLE" && ModeRow.DurationInSec > 600)
                                {
                                    LossTime += (decimal)(ModeRow.DurationInSec / 60.00);
                                    decimal LossDuration = (decimal)(ModeRow.DurationInSec / 60.00);
                                    if (ModeRow.LossCodeID != null)
                                        insertProdlosses(ProdRow.HMIID, (int)ModeRow.LossCodeID, LossDuration, CorrectedDate, MachineID);
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
                                        SetupMinorTime += (decimal)(Serverdb.tblSetupMaints.Where(m => m.ModeID == ModeRow.ModeID).Select(m => m.MinorLossTime).First() / 60.00);
                                    }
                                    catch { }
                                }
                            }
                            #endregion

                            #region Logic to get the Mode Duration Which Was started before this Production and Ended during this Production
                            var GetEndModeDuration = Serverdb.tbllivemodes.Where(m => m.MachineID == MachineID && m.StartTime < ProdStartTime && m.EndTime > ProdStartTime && m.EndTime <= ProdEndtime && m.IsCompleted == 1 && m.ModeTypeEnd == 1).FirstOrDefault();
                            if (GetEndModeDuration != null)
                            {
                                if (GetEndModeDuration.ModeType == "PROD")
                                {
                                    OperatingTime += (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60.00);
                                }
                                else if (GetEndModeDuration.ModeType == "IDLE")
                                {
                                    LossTime += (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60.00);
                                    decimal LossDuration = (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60.00);
                                    if (GetEndModeDuration.LossCodeID != null)
                                        insertProdlosses(ProdRow.HMIID, (int)GetEndModeDuration.LossCodeID, LossDuration, CorrectedDate, MachineID);
                                    //insertProdlosses(WOID, LossID, LossDuration, CorrectedDate);
                                }
                                else if (GetEndModeDuration.ModeType == "MNT")
                                {
                                    MntTime += (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60.00);
                                }
                                else if (GetEndModeDuration.ModeType == "POWEROFF")
                                {
                                    PowerOffTime += (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60.00);
                                }
                            }
                            #endregion

                            #region Logic to get the Mode Duration Which Was Started during the Production and Ended after the Production
                            var GetStartModeDuration = Serverdb.tbllivemodes.Where(m => m.MachineID == MachineID && m.StartTime >= ProdStartTime && m.EndTime >= ProdStartTime && m.StartTime < ProdEndtime && m.EndTime > ProdEndtime && m.IsCompleted == 1 && m.ModeTypeEnd == 1).FirstOrDefault();
                            if (GetStartModeDuration != null)
                            {
                                if (GetStartModeDuration.ModeType == "PROD")
                                {
                                    OperatingTime += (decimal)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60.00);
                                }
                                else if (GetStartModeDuration.ModeType == "IDLE")
                                {
                                    LossTime += (decimal)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60.00);
                                    decimal LossDuration = (decimal)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60.00);
                                    if (GetStartModeDuration.LossCodeID != null)
                                        insertProdlosses(ProdRow.HMIID, (int)GetStartModeDuration.LossCodeID, LossDuration, CorrectedDate, MachineID);
                                    //insertProdlosses(WOID, LossID, LossDuration, CorrectedDate);
                                }
                                else if (GetStartModeDuration.ModeType == "MNT")
                                {
                                    MntTime += (decimal)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60.00);
                                }
                                else if (GetStartModeDuration.ModeType == "POWEROFF")
                                {
                                    PowerOffTime += (decimal)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60.00);
                                }
                            }
                            #endregion

                            int TotlaQty = ProdRow.Total_Qty;
                            if (TotlaQty == 0)
                                TotlaQty = 1;
                            decimal GetOptime = OperatingTime;
                            if (GetOptime == 0)
                            {
                                GetOptime = 1;
                            }
                            decimal IdealCycleTimeVal = 0;
                            var IdealCycTime = Serverdb.tblparts.Where(m => m.FGCode == ProdRow.FGCode && m.OperationNo == ProdRow.OperationNo).FirstOrDefault();
                            if (IdealCycTime != null)
                                IdealCycleTimeVal = IdealCycTime.IdealCycleTime;
                            double TotalTime = ProdEndtime.Subtract(ProdStartTime).TotalMinutes;
                            decimal UtilPercent = (decimal)Math.Round((double)OperatingTime / TotalTime * 100, 2);
                            decimal Quality = (decimal)Math.Round((double)ProdRow.Yield_Qty / TotlaQty * 100, 2);
                            decimal Performance = (decimal)Math.Round((double)IdealCycleTimeVal * (double)ProdRow.Total_Qty / (double)GetOptime * 100, 2);
                            int PerformanceFactor = (int)IdealCycleTimeVal * ProdRow.Total_Qty;
                            tbl_ProdManMachine PRA = new tbl_ProdManMachine();
                            PRA.MachineID = MachineID;
                            PRA.WOID = ProdRow.HMIID;
                            PRA.CorrectedDate = CorrectedDate.Date;
                            PRA.TotalLoss = LossTime;
                            PRA.TotalOperatingTime = Math.Round(OperatingTime, 2);
                            PRA.TotalSetup = Math.Round(SetupTime + SetupMinorTime, 2);
                            PRA.TotalMinorLoss = Math.Round(MinorLossTime - SetupMinorTime, 2);
                            PRA.TotalSetupMinorLoss = Math.Round(SetupMinorTime, 2);
                            PRA.TotalPowerLoss = Math.Round(PowerOffTime, 2);
                            PRA.UtilPercent = Math.Round(UtilPercent, 2);
                            PRA.QualityPercent = Math.Round(Quality, 2);
                            PRA.PerformancePerCent = Math.Round(Performance, 2);
                            PRA.PerfromaceFactor = PerformanceFactor;
                            PRA.InsertedOn = DateTime.Now;
                            Serverdb.tbl_ProdManMachine.Add(PRA);
                            Serverdb.SaveChanges();
                        }
                    }
                }
                else
                {
                    var prodman = Serverdb.tbl_ProdManMachine.Where(m => m.CorrectedDate == CorrectedDate && m.MachineID == MachineID).ToList();
                    if (prodman.Count == 0)
                    {
                        var GetModeDurations = Serverdb.tbllivemodes.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate && m.IsCompleted == 1 && m.ModeTypeEnd == 1).ToList();
                        foreach (var ModeRow in GetModeDurations)
                        {
                            if (ModeRow.ModeType == "PROD")
                            {
                                OperatingTime += (decimal)(ModeRow.DurationInSec / 60.00);
                            }
                            else if (ModeRow.ModeType == "IDLE" && ModeRow.DurationInSec > 600)
                            {
                                LossTime += (decimal)(ModeRow.DurationInSec / 60.00);
                                decimal LossDuration = (decimal)(ModeRow.DurationInSec / 60.00);
                                //if (ModeRow.LossCodeID != null)
                                //    insertProdlosses(ProdRow.HMIID, (int)ModeRow.LossCodeID, LossDuration, CorrectedDate, MachineID);
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
                                    SetupMinorTime += (decimal)(Serverdb.tblSetupMaints.Where(m => m.ModeID == ModeRow.ModeID).Select(m => m.MinorLossTime).First() / 60.00);
                                }
                                catch { }
                            }
                        }

                        //#region Logic to get the Mode Duration Which Was started before this Production and Ended during this Production
                        //var GetEndModeDuration = Serverdb.tbllivemodes.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate && m.IsCompleted == 1 && m.ModeTypeEnd == 1).FirstOrDefault();
                        //if (GetEndModeDuration != null)
                        //{
                        //    if (GetEndModeDuration.ModeType == "PROD")
                        //    {
                        //        OperatingTime += (decimal)(ModeRow.DurationInSec / 60.00);
                        //        // OperatingTime += (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60.00);
                        //    }
                        //    else if (GetEndModeDuration.ModeType == "IDLE")
                        //    {
                        //        LossTime += (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60.00);
                        //        decimal LossDuration = (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60.00);
                        //        if (GetEndModeDuration.LossCodeID != null)
                        //            insertProdlosses(ProdRow.HMIID, (int)GetEndModeDuration.LossCodeID, LossDuration, CorrectedDate, MachineID);
                        //        //insertProdlosses(WOID, LossID, LossDuration, CorrectedDate);
                        //    }
                        //    else if (GetEndModeDuration.ModeType == "MNT")
                        //    {
                        //        MntTime += (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60.00);
                        //    }
                        //    else if (GetEndModeDuration.ModeType == "POWEROFF")
                        //    {
                        //        PowerOffTime += (decimal)(Convert.ToDateTime(GetEndModeDuration.EndTime).Subtract(Convert.ToDateTime(ProdStartTime)).TotalSeconds / 60.00);
                        //    }
                        //}
                        //#endregion

                        //#region Logic to get the Mode Duration Which Was Started during the Production and Ended after the Production
                        //var GetStartModeDuration = Serverdb.tbllivemodes.Where(m => m.MachineID == MachineID && m.StartTime >= ProdStartTime && m.EndTime >= ProdStartTime && m.StartTime < ProdEndtime && m.EndTime > ProdEndtime && m.IsCompleted == 1 && m.ModeTypeEnd == 1).FirstOrDefault();
                        //if (GetStartModeDuration != null)
                        //{
                        //    if (GetStartModeDuration.ModeType == "PROD")
                        //    {
                        //        OperatingTime += (decimal)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60.00);
                        //    }
                        //    else if (GetStartModeDuration.ModeType == "IDLE")
                        //    {
                        //        LossTime += (decimal)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60.00);
                        //        decimal LossDuration = (decimal)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60.00);
                        //        if (GetStartModeDuration.LossCodeID != null)
                        //            insertProdlosses(ProdRow.HMIID, (int)GetStartModeDuration.LossCodeID, LossDuration, CorrectedDate, MachineID);
                        //        //insertProdlosses(WOID, LossID, LossDuration, CorrectedDate);
                        //    }
                        //    else if (GetStartModeDuration.ModeType == "MNT")
                        //    {
                        //        MntTime += (decimal)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60.00);
                        //    }
                        //    else if (GetStartModeDuration.ModeType == "POWEROFF")
                        //    {
                        //        PowerOffTime += (decimal)(Convert.ToDateTime(ProdEndtime).Subtract(Convert.ToDateTime(GetStartModeDuration.StartTime)).TotalSeconds / 60.00);
                        //    }
                        //}
                        //#endregion
                        int partscount = 0;
                        var partcount = Serverdb.tblpartscountandcuttings.Where(m => m.MachineID == MachineID && m.CorrectedDate == CorrectedDate).ToList();
                        if (partcount != null)
                        {
                            foreach (var partcountdet in partcount)
                            {
                                partscount = partscount + partcountdet.PartCount;
                            }
                            double Total_Qty = partscount;
                            int TotlaQty = (int)Total_Qty;
                            if (TotlaQty == 0)
                                TotlaQty = 1;
                            decimal GetOptime = OperatingTime;
                            if (GetOptime == 0)
                            {
                                GetOptime = 1;
                            }
                            decimal IdealCycleTimeVal = 60;
                            double TotalTime = 24;
                            //double TotalTime = ProdEndtime.Subtract(ProdStartTime).TotalMinutes;
                            decimal UtilPercent = (decimal)Math.Round((double)OperatingTime / TotalTime * 100, 2) / 100;
                            //decimal Quality = (decimal)Math.Round((double)ProdRow.Yield_Qty / TotlaQty * 100, 2);
                            decimal Quality = 100;
                            decimal Performance = (decimal)Math.Round((double)IdealCycleTimeVal * Total_Qty / (double)GetOptime * 100, 2) / 100;
                            int PerformanceFactor = (int)IdealCycleTimeVal * (int)Total_Qty;
                            tbl_ProdManMachine PRA = new tbl_ProdManMachine();
                            PRA.MachineID = MachineID;
                            //PRA.WOID = ProdRow.HMIID;
                            PRA.WOID = 8;
                            PRA.CorrectedDate = CorrectedDate.Date;
                            PRA.TotalLoss = LossTime;
                            PRA.TotalOperatingTime = OperatingTime;
                            PRA.TotalSetup = Math.Round(SetupTime + SetupMinorTime, 2);
                            PRA.TotalSetup = Math.Round(SetupTime + SetupMinorTime, 2);
                            PRA.TotalMinorLoss = Math.Round(MinorLossTime - SetupMinorTime, 2);
                            PRA.TotalSetupMinorLoss = Math.Round(SetupMinorTime, 2);
                            PRA.TotalPowerLoss = Math.Round(PowerOffTime, 2);
                            PRA.UtilPercent = Math.Round(UtilPercent, 2);
                            PRA.QualityPercent = Math.Round(Quality, 2);
                            PRA.PerformancePerCent = Math.Round(Performance, 2);
                            PRA.PerfromaceFactor = PerformanceFactor;
                            PRA.InsertedOn = DateTime.Now;
                            Serverdb.tbl_ProdManMachine.Add(PRA);
                            Serverdb.SaveChanges();
                        }
                        else { }
                    }
                    else { }
                }

            }

            catch (Exception e)
            {
            }
        }

        //Insert Losses Data for Man Machine Ticket.
        public void insertProdlosses(int WOID, int LossID, decimal LossDuration, DateTime CorrectedDate, int MachineID)
        {
            var Presentloss = Serverdb.tbl_ProdOrderLosses.Where(m => m.WOID == WOID && m.LossID == LossID).FirstOrDefault();
            if (Presentloss == null)
            {
                tbl_ProdOrderLosses PRA = new tbl_ProdOrderLosses();
                PRA.LossID = LossID;
                if (LossID != 0)
                {
                    var GetLossDet = Serverdb.tbllossescodes.Find(LossID);
                    if (GetLossDet.LossCodesLevel1ID == null)
                    {
                        PRA.LossCodeL1ID = LossID;
                    }
                    else
                    {
                        PRA.LossCodeL1ID = (int)GetLossDet.LossCodesLevel1ID;
                        PRA.LossID = (int)GetLossDet.LossCodesLevel1ID;
                    }
                    if (GetLossDet.LossCodesLevel2ID != null)
                        PRA.LossCodeL2ID = (int)GetLossDet.LossCodesLevel2ID;
                }
                //PRA.MachineID = MachineID;
                PRA.WOID = WOID;
                PRA.CorrectedDate = CorrectedDate.Date;
                PRA.LossDuration = (int)LossDuration;
                PRA.MachineID = MachineID;
                Serverdb.tbl_ProdOrderLosses.Add(PRA);
                Serverdb.SaveChanges();
            }
            else
            {
                Presentloss.LossDuration = (int)(Presentloss.LossDuration + LossDuration);
                Serverdb.Entry(Presentloss).State = System.Data.Entity.EntityState.Modified;
                Serverdb.SaveChanges();
            }
        }

        //Insert into Graph Data to calculate Duration from tblmode
        public void InsertGraphData(int machineID, DateTime correctedDate)
        {
            decimal OperatingTime = 0;
            decimal LossTime = 0;
            decimal MinorLossTime = 0;
            decimal MntTime = 0;
            decimal PowerOffTime = 0;
            string Corrected = correctedDate.ToString("yyyy-MM-dd");
            var GetModDet = new Graph_Data();
            using (i_facility_shaktiEntities1 Serverdb = new i_facility_shaktiEntities1())
            {
                GetModDet = Serverdb.Graph_Data.Where(m => m.MachineID == machineID && m.CorrectedDate == Corrected).FirstOrDefault();
            }
            if (GetModDet == null)
            {
                var GetModeDurations = new List<tblmode>();
                using (i_facility_shaktiEntities1 Serverdb = new i_facility_shaktiEntities1())
                {
                    GetModeDurations = Serverdb.tblmodes.Where(m => m.MachineID == machineID && m.CorrectedDate == correctedDate.Date && m.IsCompleted == 1).ToList();
                }
                OperatingTime = Convert.ToDecimal(GetModeDurations.Where(m => m.ModeType == "PROD").ToList().Sum(m => m.DurationInSec));
                PowerOffTime = Convert.ToDecimal(GetModeDurations.Where(m => m.ModeType == "POWEROFF").ToList().Sum(m => m.DurationInSec));
                MinorLossTime = Convert.ToDecimal(GetModeDurations.Where(m => m.ModeType == "IDLE" && m.DurationInSec < 600).ToList().Sum(m => m.DurationInSec));
                LossTime = Convert.ToDecimal(GetModeDurations.Where(m => m.ModeType == "IDLE" && m.DurationInSec > 600).ToList().Sum(m => m.DurationInSec));
                OperatingTime = Math.Round((OperatingTime / 60), 2);
                PowerOffTime = (PowerOffTime / 60);
                MntTime = (MntTime / 60);
                MinorLossTime = (MinorLossTime / 60);
                LossTime = (LossTime / 60);
                var Lossdet = GetModeDurations.Where(m => m.ModeType == "IDLE" && m.DurationInSec > 600).Select(m => new { m.LossCodeID, m.DurationInSec }).ToList();
                foreach (var row in Lossdet)
                {
                    if (row.LossCodeID != null)
                    {
                        double lossduration = Convert.ToDouble(row.DurationInSec) / 60.00;
                        LossDetail obj = new LossDetail();
                        obj.CorrectedDate = correctedDate.Date;
                        obj.MachineID = machineID;
                        obj.losscodeid = (int)row.LossCodeID;
                        obj.Duration = Convert.ToInt32(lossduration);
                        using (i_facility_shaktiEntities1 Serverdb = new i_facility_shaktiEntities1())
                        {
                            Serverdb.LossDetails.Add(obj);
                            Serverdb.SaveChanges();
                        }
                    }
                    else { }
                }
                try
                {
                    Graph_Data gdobj = new Graph_Data();
                    //gdobj.Gid = 1;
                    gdobj.LossTime = (int)LossTime;
                    gdobj.MinorLossTime = (int)MinorLossTime;
                    gdobj.OperatingTime = (int)OperatingTime;
                    gdobj.MinTime = null;
                    gdobj.PowerOFF = (int)PowerOffTime;
                    gdobj.CorrectedDate = Corrected;
                    gdobj.MachineID = machineID;
                    //gdobj.ScrapQty = ScrapQty;
                    //gdobj.YeildQty = Yield_Qty;
                    //gdobj.PerformanceFactor = PerformanceFactor;
                    using (i_facility_shaktiEntities1 Serverdb = new i_facility_shaktiEntities1())
                    {
                        Serverdb.Graph_Data.Add(gdobj);
                        Serverdb.SaveChanges();
                    }
                }
                catch (Exception ex)
                {

                }
            }
            else
            {

            }
        }
    }
}

