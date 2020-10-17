using i_facilitylibrary.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tblshift_mstr
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblshift_mstr()
        {
            this.tblmachineallocations = new HashSet<tblmachineallocation>();
            this.tblplannedbreaks = new HashSet<tblplannedbreak>();
        }

        public int ShiftID { get; set; }
        public string ShiftName { get; set; }
        public Nullable<System.TimeSpan> StartTime { get; set; }
        public Nullable<System.TimeSpan> EndTime { get; set; }
        public Nullable<int> IsDeleted { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<int> Duration { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblmachineallocation> tblmachineallocations { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblplannedbreak> tblplannedbreaks { get; set; }
    }
}
