using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tblmode
    {
        public int ModeID { get; set; }
        public int MachineID { get; set; }
        public string Mode { get; set; }
        public System.DateTime InsertedOn { get; set; }
        public int InsertedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public string CorrectedDate { get; set; }
        public int IsDeleted { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public string ColorCode { get; set; }
        public int IsCompleted { get; set; }
        public Nullable<int> DurationInSec { get; set; }
        public Nullable<int> ModeMonth { get; set; }
        public Nullable<int> ModeYear { get; set; }
        public Nullable<int> ModeWeekNumber { get; set; }
        public Nullable<int> ModeQuarter { get; set; }

        public virtual tblmachinedetail tblmachinedetail { get; set; }
    }
}
