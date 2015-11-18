using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DowntimeCollection_Demo
{
    public class CaseCountModel
    {
        public int Id { get; set; }
        public string EventStart { get; set; }
        public string EventStop  { get; set; }
        public string Line { get; set; }
        public int CaseCount { get; set; }
        public string Client { get; set; }
        public decimal Throughput { get; set; }

    }

    public class CCEfficiency
    {
        public int Id { get; set; }
        public DateTime? EventStart { get; set; }
        public DateTime? EventStop { get; set; }
        public string Line { get; set; }
        public int CaseCount { get; set; }
        public string Client { get; set; }

        public decimal EstCases { get; set; }
    }
}