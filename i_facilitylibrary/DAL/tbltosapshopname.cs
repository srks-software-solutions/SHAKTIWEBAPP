using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tbltosapshopname
    {
        public int toSAPShopNamesID { get; set; }
        public Nullable<int> ShopId { get; set; }
        public string ShopName { get; set; }
        public int IsDeleted { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<int> isSetupShown { get; set; }
    }
}
