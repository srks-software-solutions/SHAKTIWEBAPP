using System.Collections.Generic;
using SRKSDemo.Server_Model;


namespace SRKSDemo.Models
{
    public class SensorMaster
    {
        public configurationtblsensormaster sensormaster { get; set; }
        public IEnumerable<configurationtblsensormaster> sensormasterList { get; set; }
    }
}