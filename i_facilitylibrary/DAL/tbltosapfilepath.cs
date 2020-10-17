using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tbltosapfilepath
    {
        public int toSAPFilePathID { get; set; }
        public string PathName { get; set; }
        public string Path { get; set; }
        public int IsDeleted { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public string tbltosapfilepathcol { get; set; }
    }
}
