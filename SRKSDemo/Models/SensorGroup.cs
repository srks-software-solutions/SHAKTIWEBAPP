using SRKSDemo.Server_Model;
using System.Collections.Generic;

namespace SRKSDemo.Models
{
    public class SensorGroup
    {
        public int MachineID { get; set; }
        public int sid { get; set; }
        public string machineName { get; set; }
        public string SensorName { get; set; }
        public string sensorDesc { get; set; }

        public List<SensorGroup> sensorGroupsList { get; set; }
            
    }

    public class sensormodel
    {
        public configuration_tblsensorgroup sensorgroup { get; set; }
        public List<configuration_tblsensorgroup> sensorList { get; set; }
    }
}