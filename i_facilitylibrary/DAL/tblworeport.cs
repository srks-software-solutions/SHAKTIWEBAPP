using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tblworeport
    {
        public int WOReportID { get; set; }
        public Nullable<int> MachineID { get; set; }
        public Nullable<int> HMIID { get; set; }
        public string OperatorName { get; set; }
        public string Shift { get; set; }
        public string CorrectedDate { get; set; }
        public string PartNo { get; set; }
        public string WorkOrderNo { get; set; }
        public string OpNo { get; set; }
        public Nullable<int> TargetQty { get; set; }
        public Nullable<int> DeliveredQty { get; set; }
        public Nullable<int> IsPF { get; set; }
        public Nullable<int> IsHold { get; set; }
        public Nullable<decimal> CuttingTime { get; set; }
        public Nullable<decimal> SettingTime { get; set; }
        public Nullable<decimal> SelfInspection { get; set; }
        public Nullable<decimal> Idle { get; set; }
        public Nullable<decimal> Breakdown { get; set; }
        public string Type { get; set; }
        public Nullable<decimal> NCCuttingTimePerPart { get; set; }
        public Nullable<decimal> TotalNCCuttingTime { get; set; }
        public Nullable<decimal> WOEfficiency { get; set; }
        public Nullable<int> RejectedQty { get; set; }
        public string RejectedReason { get; set; }
        public string Program { get; set; }
        public Nullable<decimal> MRWeight { get; set; }
        public Nullable<System.DateTime> InsertedOn { get; set; }
        public int IsMultiWO { get; set; }
        public Nullable<int> IsNormalWC { get; set; }
        public string HoldReason { get; set; }
        public Nullable<decimal> MinorLoss { get; set; }
        public string SplitWO { get; set; }
        public Nullable<decimal> Blue { get; set; }
        public Nullable<decimal> ScrapQtyTime { get; set; }
        public Nullable<decimal> ReWorkTime { get; set; }
        public Nullable<decimal> SummationOfSCTvsPP { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public Nullable<int> batchNo { get; set; }
    }
}
