using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
   public class tbl_PrevOperationCancel
    {
        public int OPCancelID { get; set; }
        public string ProductionOrder { get; set; }
        public string Operation { get; set; }
        public int IsCancelled { get; set; }
        public Nullable<int> Qty { get; set; }
        public string CorrectedDate { get; set; }
        public string WorkCenter { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public string PartNumber { get; set; }
    }
}
