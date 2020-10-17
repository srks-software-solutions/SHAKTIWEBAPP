using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace i_facilitylibrary.DAL
{
    public partial class tblshiftplanner
    {
        public int ShiftPlannerID { get; set; }
        public string ShiftPlannerName { get; set; }
        public string ShiftPlannerDesc { get; set; }
        public int ShiftMethodID { get; set; }
        public Nullable<int> PlantID { get; set; }
        public Nullable<int> ShopID { get; set; }
        public Nullable<int> CellID { get; set; }
        public Nullable<int> MachineID { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public Nullable<int> IsPlanStopped { get; set; }
        public Nullable<System.DateTime> PlanStoppedDate { get; set; }
        public Nullable<int> IsPlanRemoved { get; set; }
        public int IsDeleted { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }

        public virtual tblcell tblcell { get; set; }
        public virtual tblmachinedetail tblmachinedetail { get; set; }
        public virtual tblplant tblplant { get; set; }
        public virtual tblshop tblshop { get; set; }
    }
}
