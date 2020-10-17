using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class tblgenericworkcode
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblgenericworkcode()
        {
            this.tblgenericworkentries = new HashSet<tblgenericworkentry>();
        }

        public int GenericWorkID { get; set; }
        public string GenericWorkCode { get; set; }
        public string GenericWorkDesc { get; set; }
        public string MessageType { get; set; }
        public int GWCodesLevel { get; set; }
        public Nullable<int> GWCodesLevel1ID { get; set; }
        public Nullable<int> GWCodesLevel2ID { get; set; }
        public int IsDeleted { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<int> EndCode { get; set; }
        public Nullable<System.DateTime> DeletedDate { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblgenericworkentry> tblgenericworkentries { get; set; }
    }
}
