using SRKSDemo.Server_Model;
using System.Collections.Generic;

namespace SRKSDemo.Models
{
    public class SensorDataLink
    {
        public configurationtblsensordatalink sensordatalink { get; set; }
        public IEnumerable<configurationtblsensordatalink> sensordataList { get; set; }
    }
}