using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tblshiftdetails_machinewise
    {
        public int ShiftDetailsMacID { get; set; }
        public int MachineID { get; set; }
        public string ShiftName { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public string CorrectedDate { get; set; }
        public Nullable<int> IsDeleted { get; set; }
        public string InsertedOn { get; set; }
        public Nullable<int> InsertedBy { get; set; }
        public Nullable<int> ShiftDetailsID { get; set; }

        public virtual tblmachinedetail tblmachinedetail { get; set; }
    }
}
