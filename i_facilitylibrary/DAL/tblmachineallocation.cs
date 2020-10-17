using i_facilitylibrary.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tblmachineallocation
    {
        public int ID { get; set; }
        public Nullable<int> UserID { get; set; }
        public Nullable<int> MachineID { get; set; }
        public Nullable<int> ShiftID { get; set; }
        public string CorrectedDate { get; set; }
        public Nullable<int> IsDeleted { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }

        public virtual tblmachinedetail tblmachinedetail { get; set; }
        public virtual tblshift_mstr tblshift_mstr { get; set; }
        public virtual tbluser tbluser { get; set; }
    }
}
