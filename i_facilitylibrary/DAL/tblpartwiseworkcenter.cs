using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tblpartwiseworkcenter
    {
        public int PartWiseWcId { get; set; }
        public int WorkCenterId { get; set; }
        public int MeasuringUnitId { get; set; }
        public short IsDeleted { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }

        public virtual tblmachinedetail tblmachinedetail { get; set; }
    }
}
