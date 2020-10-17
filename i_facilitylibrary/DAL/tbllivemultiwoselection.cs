using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tbllivemultiwoselection
    {
        public int MultiWOID { get; set; }
        public string WorkOrder { get; set; }
        public string PartNo { get; set; }
        public string OperationNo { get; set; }
        public Nullable<int> TargetQty { get; set; }
        public Nullable<int> DeliveredQty { get; set; }
        public Nullable<int> HMIID { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> IsCompleted { get; set; }
        public Nullable<int> ProcessQty { get; set; }
        public string DDLWorkCentre { get; set; }
        public int IsSubmit { get; set; }
        public int ScrapQty { get; set; }
        public string SplitWO { get; set; }

        public virtual tbllivehmiscreen tbllivehmiscreen { get; set; }
    }
}
