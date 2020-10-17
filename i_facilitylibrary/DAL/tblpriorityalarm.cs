using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tblpriorityalarm
    {
        public int AlarmID { get; set; }
        public int AlarmNumber { get; set; }
        public string AlarmDesc { get; set; }
        public int AxisNo { get; set; }
        public string AlarmGroup { get; set; }
        public int PriorityNumber { get; set; }
        public int IsDeleted { get; set; }
        public string CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<int> MachineID { get; set; }
        public string CorrectedDate { get; set; }
        public Nullable<int> isMailSent { get; set; }
    }
}
