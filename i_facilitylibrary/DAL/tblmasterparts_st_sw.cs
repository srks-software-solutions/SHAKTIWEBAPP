using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tblmasterparts_st_sw
    {
        public int PARTSSTSWID { get; set; }
        public string PartNo { get; set; }
        public string OpNo { get; set; }
        public Nullable<decimal> StdSetupTime { get; set; }
        public Nullable<decimal> StdCuttingTime { get; set; }
        public Nullable<decimal> StdChangeoverTime { get; set; }
        public Nullable<decimal> InputWeight { get; set; }
        public Nullable<decimal> OutputWeight { get; set; }
        public Nullable<decimal> MaterialRemovedQty { get; set; }
        public int IsDeleted { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public string StdSetupTimeUnit { get; set; }
        public string StdCuttingTimeUnit { get; set; }
        public string StdChangeoverTimeUnit { get; set; }
        public string InputWeightUnit { get; set; }
        public string OutputWeightUnit { get; set; }
        public string MaterialRemovedQtyUnit { get; set; }
        public Nullable<System.DateTime> DeletedDate { get; set; }
    }
}
