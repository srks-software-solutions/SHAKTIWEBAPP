using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
            public partial class alarm_history_master
        {
            public int AlarmID { get; set; }
            public string AlarmMessage { get; set; }
            public System.DateTime AlarmDate { get; set; }
            public System.TimeSpan AlarmTime { get; set; }
            public System.DateTime AlarmDateTime { get; set; }
            public Nullable<System.DateTime> InsertedOn { get; set; }
            public Nullable<int> MachineID { get; set; }
            public string Shift { get; set; }
            public Nullable<System.DateTime> CorrectedDate { get; set; }
            public string ErrorNum { get; set; }
            public string Status { get; set; }
            public string DetailCode1 { get; set; }
            public string DetailCode2 { get; set; }
            public string DetailCode3 { get; set; }
            public string InterferedPart { get; set; }
            public string SystemHead { get; set; }
            public string AlarmNo { get; set; }
            public string Axis_Num { get; set; }
        }
}
