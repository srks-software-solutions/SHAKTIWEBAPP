using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class recipientmailid
    {
        public int RE_ID { get; set; }
        public string MailID { get; set; }
        public int AutoEmailType { get; set; }
        public string DisplayName { get; set; }
        public string RecipientType { get; set; }
        public int IsDeleted { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }

        public virtual tblemailreporttype tblemailreporttype { get; set; }
    }
}
