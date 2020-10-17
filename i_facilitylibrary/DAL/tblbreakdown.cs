using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tblbreakdown
    {
        public int BreakdownID { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public Nullable<int> BreakDownCode { get; set; }
        public int MachineID { get; set; }
        public string CorrectedDate { get; set; }
        public string Shift { get; set; }
        public string MessageDesc { get; set; }
        public string MessageCode { get; set; }
        public Nullable<int> DoneWithRow { get; set; }

        public virtual tbllossescode tbllossescode { get; set; }
    }
}
