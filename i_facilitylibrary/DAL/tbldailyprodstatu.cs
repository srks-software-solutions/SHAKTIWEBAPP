using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tbldailyprodstatu
    {
        public int ID { get; set; }
        public Nullable<int> MachineID { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public Nullable<int> Duration { get; set; }
        public string Status { get; set; }
        public Nullable<int> IsDeleted { get; set; }
        public Nullable<System.DateTime> InsertedOn { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public string CorrectedDate { get; set; }
        public string ColorCode { get; set; }

        public virtual tblmachinedetail tblmachinedetail { get; set; }
    }
}
