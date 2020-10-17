using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tblmodulehelper
    {
        public int ID { get; set; }
        public string ModuleID { get; set; }
        public Nullable<int> RoleID { get; set; }
        public bool IsAll { get; set; }
        public bool IsAdded { get; set; }
        public bool IsEdited { get; set; }
        public bool IsRemoved { get; set; }
        public bool IsReadonly { get; set; }
        public bool IsHidden { get; set; }
        public short IsDeleted { get; set; }
    }
}
