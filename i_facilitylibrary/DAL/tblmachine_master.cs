using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tblmachine_master
    {
        public int MachineID { get; set; }
        public string MachineInvNo { get; set; }
        public string IPAddress { get; set; }
        public Nullable<int> MachineType { get; set; }
        public string ControllerType { get; set; }
        public Nullable<int> IsDeleted { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
    }
}
