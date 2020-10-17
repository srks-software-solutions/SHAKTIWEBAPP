using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tbllossofentry
    {
        public int LossID { get; set; }
        public int MessageCodeID { get; set; }
        public Nullable<System.DateTime> StartDateTime { get; set; }
        public Nullable<System.DateTime> EndDateTime { get; set; }
        public Nullable<System.DateTime> EntryTime { get; set; }
        public string CorrectedDate { get; set; }
        public int MachineID { get; set; }
        public string Shift { get; set; }
        public string MessageDesc { get; set; }
        public string MessageCode { get; set; }
        public int IsUpdate { get; set; }
        public int DoneWithRow { get; set; }
        public Nullable<int> IsStart { get; set; }
        public Nullable<int> IsScreen { get; set; }
        public int ForRefresh { get; set; }
        public Nullable<int> LossMonth { get; set; }
        public Nullable<int> LossYear { get; set; }
        public Nullable<int> LossWeekNumber { get; set; }
        public Nullable<int> LossQuarter { get; set; }

        public virtual tbllossescode tbllossescode { get; set; }
    }
}
