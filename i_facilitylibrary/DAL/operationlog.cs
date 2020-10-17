using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class operationlog
    {
        public int idoperationlog { get; set; }
        public string OpMsg { get; set; }
        public Nullable<System.DateTime> OpDate { get; set; }
        public Nullable<System.TimeSpan> OpTime { get; set; }
        public Nullable<System.DateTime> OpDateTime { get; set; }
        public string OpReason { get; set; }
        public Nullable<int> MachineID { get; set; }
    }
}
