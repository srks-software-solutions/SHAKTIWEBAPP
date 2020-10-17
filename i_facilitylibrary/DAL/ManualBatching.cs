using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public class ManualBatching
    {
        public int BatchID { get; set; }
        public double Duration { get; set; }
        public double CuttingTime { get; set; }
        public string Operator { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool Iscompleted { get; set; }
    }
}
