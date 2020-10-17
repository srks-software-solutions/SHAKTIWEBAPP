using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tbllivemanuallossofentryrep
    {
        public int MLossID { get; set; }
        public int MessageCodeID { get; set; }
        public Nullable<System.DateTime> StartDateTime { get; set; }
        public string CorrectedDate { get; set; }
        public int MachineID { get; set; }
        public string Shift { get; set; }
        public string MessageDesc { get; set; }
        public string MessageCode { get; set; }
        public Nullable<int> HMIID { get; set; }
        public string PartNo { get; set; }
        public Nullable<int> OpNo { get; set; }
        public string WONo { get; set; }
        public Nullable<System.DateTime> EndDateTime { get; set; }
        public Nullable<int> EndHMIID { get; set; }
        public Nullable<System.DateTime> DeletedDate { get; set; }

        public virtual tbllossescode tbllossescode { get; set; }
    }
}
