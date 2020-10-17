using System.Collections.Generic;

namespace i_facilitylibrary.DAL
{
    public class HoldList
    {
        public List<tblmanuallossofentry> HoldListDetailsWO { get; set; }
        //public List<tbllossofentry> HoldListDetailsWC { get; set; }
        public List<tbllivelossofentry> HoldListDetailsWC { get; set; }
    }
}
