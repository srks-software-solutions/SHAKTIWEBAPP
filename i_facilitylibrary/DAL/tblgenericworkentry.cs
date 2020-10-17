using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tblgenericworkentry
    {
        public int GWEntryID { get; set; }
        public int GWCodeID { get; set; }
        public Nullable<System.DateTime> StartDateTime { get; set; }
        public Nullable<System.DateTime> EndDateTime { get; set; }
        public string CorrectedDate { get; set; }
        public int MachineID { get; set; }
        public string Shift { get; set; }
        public string GWCodeDesc { get; set; }
        public string GWCode { get; set; }
        public int DoneWithRow { get; set; }

        public virtual tblgenericworkcode tblgenericworkcode { get; set; }
    }
}
