using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tblholdcode
    {
        public int HoldCodeID { get; set; }
        public string HoldCode { get; set; }
        public string HoldCodeDesc { get; set; }
        public string MessageType { get; set; }
        public int HoldCodesLevel { get; set; }
        public Nullable<int> HoldCodesLevel1ID { get; set; }
        public Nullable<int> HoldCodesLevel2ID { get; set; }
        public string ContributeTo { get; set; }
        public int IsDeleted { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<int> EndCode { get; set; }
        public Nullable<System.DateTime> DeletedDate { get; set; }
    }
}
