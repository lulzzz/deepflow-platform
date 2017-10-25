using System;
using System.Collections.Generic;
using System.Text;

namespace Deepflow.Platform.Common.Data.Configuration
{
    public class CassandraConfiguration
    {
        public string Address { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Keyspace { get; set; }
        public int QueryTimeout { get; set; }
    }
}
