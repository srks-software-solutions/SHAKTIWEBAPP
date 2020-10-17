using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class parameters_master
    {
        public int ParameterID { get; set; }
        public int SetupTime { get; set; }
        public int OperatingTime { get; set; }
        public int PowerOnTime { get; set; }
        public Nullable<int> PartsCount { get; set; }
        public Nullable<System.DateTime> InsertedOn { get; set; }
        public Nullable<int> MachineID { get; set; }
        public Nullable<int> Shift { get; set; }
        public Nullable<System.DateTime> CorrectedDate { get; set; }
        public string AutoCutTime { get; set; }
        public string Total_CutTime { get; set; }
    }
}
