using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tblwolossess
    {
        public int WOLossesID { get; set; }
        public Nullable<int> HMIID { get; set; }
        public Nullable<int> LossID { get; set; }
        public string LossName { get; set; }
        public Nullable<decimal> LossDuration { get; set; }
        public Nullable<int> Level { get; set; }
        public Nullable<int> LossCodeLevel1ID { get; set; }
        public string LossCodeLevel1Name { get; set; }
        public Nullable<int> LossCodeLevel2ID { get; set; }
        public string LossCodeLevel2Name { get; set; }
        public Nullable<System.DateTime> InsertedOn { get; set; }
        public int IsDeleted { get; set; }
    }
}
