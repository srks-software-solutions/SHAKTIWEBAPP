using i_facilitylibrary.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tblroleplaymaster
    {
        public int RolePlayID { get; set; }
        public int ModuleID { get; set; }
        public bool IsAll { get; set; }
        public bool IsAdded { get; set; }
        public bool IsEdited { get; set; }
        public bool IsRemoved { get; set; }
        public bool IsHidden { get; set; }
        public bool IsReadOnly { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public int IsDeleted { get; set; }
        public int RoleID { get; set; }

        public virtual tblmodulemaster tblmodulemaster { get; set; }
        public virtual tblrole tblrole { get; set; }
    }
}
