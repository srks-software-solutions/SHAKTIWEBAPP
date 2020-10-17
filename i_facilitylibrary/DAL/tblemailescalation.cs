using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tblemailescalation
    {
        public int EMailEscalationID { get; set; }
        public string EMailEscalationName { get; set; }
        public string MessageType { get; set; }
        public Nullable<int> ReasonLevel1 { get; set; }
        public Nullable<int> ReasonLevel2 { get; set; }
        public Nullable<int> ReasonLevel3 { get; set; }
        public Nullable<int> PlantID { get; set; }
        public Nullable<int> ShopID { get; set; }
        public Nullable<int> CellID { get; set; }
        public Nullable<int> WorkCenterID { get; set; }
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public string ToList { get; set; }
        public string CcList { get; set; }
        public int IsDeleted { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<System.DateTime> DeletedDate { get; set; }

        public virtual tblcell tblcell { get; set; }
        public virtual tblplant tblplant { get; set; }
        public virtual tbllossescode tbllossescode { get; set; }
        public virtual tbllossescode tbllossescode1 { get; set; }
        public virtual tbllossescode tbllossescode2 { get; set; }
        public virtual tblshop tblshop { get; set; }
        public virtual tblmachinedetail tblmachinedetail { get; set; }
    }
}
