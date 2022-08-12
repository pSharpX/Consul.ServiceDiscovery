using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Consul.ServiceDiscovery.Common
{
    public interface IConsulServices
    {
        Task<AgentService> GetAsync(string name);
    }
}
