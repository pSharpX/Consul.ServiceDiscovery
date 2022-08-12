using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Consul.ServiceDiscovery
{
    public class ConsulConfig
    {
        public string Address { get; set; }

        public string ServiceId { get; set; }

        public string ServiceName { get; set; }

    }
}
