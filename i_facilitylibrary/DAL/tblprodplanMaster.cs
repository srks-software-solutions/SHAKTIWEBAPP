using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
   public class tblprodplanMaster
    {
        public int ProdPlanID { get; set; }
        public string WorkCenter { get; set; }
        public string Prod_Order_No { get; set; }
        public string OperationNo { get; set; }
        public string Status { get; set; }
        public string FGCode { get; set; }
        public int OrderQty { get; set; }
        public Nullable<int> SplitOrderQty { get; set; }
        public string QtyUnit { get; set; }
        public System.DateTime InsertedOn { get; set; }
        public int IsCompleted { get; set; }
    }
}
