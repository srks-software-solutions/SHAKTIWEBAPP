using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tblnetworkdetailsforddl
    {
        public int NPFDDLID { get; set; }
        public string Path { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string DomainName { get; set; }
        public int IsDeleted { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
    }
}
