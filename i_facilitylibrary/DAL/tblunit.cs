using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tblunit
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblunit()
        {
            this.tblparts = new HashSet<tblpart>();
        }

        public int U_ID { get; set; }
        public string Unit { get; set; }
        public string UnitDesc { get; set; }
        public int IsDeleted { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblpart> tblparts { get; set; }
    }
}
