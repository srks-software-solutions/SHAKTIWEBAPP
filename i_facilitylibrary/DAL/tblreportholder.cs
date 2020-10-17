using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tblreportholder
    {
        public int RHID { get; set; }
        public Nullable<System.DateTime> FromDate { get; set; }
        public Nullable<System.DateTime> ToDate { get; set; }
        public string Shift { get; set; }
        public string ShopNo { get; set; }
        public string WorkCenter { get; set; }
    }
}
