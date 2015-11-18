using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DowntimeCollection_Demo
{
    public class NodeLine
    {
        public int DeviceSetupId { get; set; }

        public string ServerName { get; set; }
        public string Client { get; set; }
        public string Line { get; set; }
        public int DowntimeThreshold { get; set; }
        public int UptimeThreshold { get; set; }
        public string IPAddress { get; set; }
        public int TrackingType { get; set; }
        public int AddressType { get; set; }
        public string TagName { get; set; }
        public int TagType { get; set; }
        public int DataType { get; set; }
        public bool TrackDowntime { get; set; }
    }
}