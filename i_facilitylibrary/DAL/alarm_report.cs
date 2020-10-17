using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class alarm_report
    {
        public int reportid { get; set; }
        public Nullable<int> slno { get; set; }
        public string alarmno { get; set; }
        public string alarmdescn { get; set; }
        public Nullable<System.DateTime> alarmdatetime { get; set; }
    }
}
