using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary.DAL
{
    public partial class user_master
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public Nullable<System.DateTime> InsertedOn { get; set; }
        public Nullable<int> InsertedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<System.DateTime> ModifiedBy { get; set; }
        public string EmailID { get; set; }
        public int IsDeleted { get; set; }
        public int RoleID { get; set; }
    }
}
