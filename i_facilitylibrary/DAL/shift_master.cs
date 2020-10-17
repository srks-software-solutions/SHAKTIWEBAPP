using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class shift_master
    {
        public int ShiftID { get; set; }
        public string ShiftName { get; set; }
        public System.TimeSpan StartTime { get; set; }
        public System.TimeSpan EndTime { get; set; }
        public System.DateTime InsertedOn { get; set; }
        public string InsertedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public int IsDeleted { get; set; }
        public Nullable<int> Duration { get; set; }
    }
}
