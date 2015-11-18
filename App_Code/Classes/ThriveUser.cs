using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DowntimeCollection_Demo.Classes
{
    public class ThriveUser
    {
        public Guid GuidKey { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool hidePanel { get; set; }
        public bool hideHelper { get; set; }
    }
}