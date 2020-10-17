using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tblpart
    {
        public int PartID { get; set; }
        public int PartNo { get; set; }
        public string PartDesc { get; set; }
        public string PartName { get; set; }
        public int IdleCycleTime { get; set; }
        public int UnitDesc { get; set; }
        public int IsDeleted { get; set; }
        public string CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }

        public virtual tblunit tblunit { get; set; }
    }
}
