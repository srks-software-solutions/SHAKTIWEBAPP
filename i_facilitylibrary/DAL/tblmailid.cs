using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{

    public partial class tblmailid
    {
        public int MailIDsID { get; set; }
        public string Name { get; set; }
        public string EmailID { get; set; }
        public string ContactNo { get; set; }
        public int IsDeleted { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
    }
}
