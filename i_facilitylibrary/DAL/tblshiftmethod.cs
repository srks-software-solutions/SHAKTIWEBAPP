using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tblshiftmethod
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblshiftmethod()
        {
            this.tblshiftdetails = new HashSet<tblshiftdetail>();
        }

        public int ShiftMethodID { get; set; }
        public string ShiftMethodName { get; set; }
        public string ShiftMethodDesc { get; set; }
        public int NoOfShifts { get; set; }
        public int IsDeleted { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<int> IsShiftMethodEdited { get; set; }
        public Nullable<System.DateTime> EditedDate { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblshiftdetail> tblshiftdetails { get; set; }
    }
}
