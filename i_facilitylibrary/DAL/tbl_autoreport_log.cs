using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tbl_autoreport_log
    {
        public int AutoReportLogID { get; set; }
        public Nullable<System.DateTime> CorrectedDate { get; set; }
        public Nullable<int> AutoReportID { get; set; }
        public Nullable<System.DateTime> InsertedOn { get; set; }
        public Nullable<int> ExcelCreated { get; set; }
        public Nullable<int> MailSent { get; set; }
        public Nullable<System.DateTime> CompletedOn { get; set; }
        public Nullable<System.DateTime> ExcelCreatedTime { get; set; }

        public virtual tbl_autoreportsetting tbl_autoreportsetting { get; set; }
    }
}
