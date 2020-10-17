using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class message_history_master
    {
        public int MessageID { get; set; }
        public string Meassage { get; set; }
        public System.DateTime MessageDate { get; set; }
        public System.TimeSpan MessageTime { get; set; }
        public System.DateTime MessageDateTime { get; set; }
        public string MessageNo { get; set; }
        public string MessageCode { get; set; }
        public string MessageType { get; set; }
        public string MessageShift { get; set; }
        public Nullable<int> MachineID { get; set; }
        public Nullable<System.DateTime> InsertedOn { get; set; }
        public Nullable<System.DateTime> CorrectedDate { get; set; }
    }
}
