using i_facilitylibrary.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tblplannedbreak
    {
        public int BreakID { get; set; }
        public string BreakReason { get; set; }
        public Nullable<System.TimeSpan> StartTime { get; set; }
        public Nullable<System.TimeSpan> EndTime { get; set; }
        public Nullable<int> BreakDuration { get; set; }
        public Nullable<int> ShiftID { get; set; }
        public Nullable<int> IsDeleted { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }

        public virtual tblshift_mstr tblshift_mstr { get; set; }
    }
}
