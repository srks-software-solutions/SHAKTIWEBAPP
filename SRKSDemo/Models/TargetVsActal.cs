using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SRKSDemo.Models
{
    public class TargetVsActal
    {
        public class ViewData
        {
            public string type { get; set; }
            public string showInLegend { get; set; }
            public int yValueFormatString { get; set; }
            public string name { get; set; }
            public List<dataPoints> dataPoints { get; set; }
           // public List<dataPoints> dataPointsTarget { get; set; }


        }

        public class dataPoints
        {
            public int? y { get; set; }
            public string label { get; set; }
            public string markerColor { get; set; }
            public string indexLabel { get; set; }
            public string markerType { get; set; }
        }

        public class ChartDataVal
        {
            public string type { get; set; }
            public string name { get; set; }
            public string showInLegend { get; set; }
            public string indexLabel { get; set; }
            public List<PivotalData> dataPoints { get; set; }
            public List<PivotalData> dataPointsTarget { get; set; }
        }
        public class PivotalData
        {
            public string label { get; set; }
            public int y { get; set; }
        }
    }
}