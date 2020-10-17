using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tbl_reportmaster
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_reportmaster()
        {
            this.tbl_autoreportsetting = new HashSet<tbl_autoreportsetting>();
        }

        public int ReportID { get; set; }
        public string ReportName { get; set; }
        public string ReportDescription { get; set; }
        public string ReportDispName { get; set; }
        public string ReportTemplatePath { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<int> IsDeleted { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_autoreportsetting> tbl_autoreportsetting { get; set; }
    }
}
