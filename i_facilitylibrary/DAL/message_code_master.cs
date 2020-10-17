using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class message_code_master
    {
        public int MessageCodeID { get; set; }
        public string MessageCode { get; set; }
        public string MessageMCode { get; set; }
        public string MessageDescription { get; set; }
        public string MessageType { get; set; }
        public System.DateTime InsertedOn { get; set; }
        public string InsertedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public int IsDeleted { get; set; }
        public string ReportDispName { get; set; }
        public string ColourCode { get; set; }
    }
}
