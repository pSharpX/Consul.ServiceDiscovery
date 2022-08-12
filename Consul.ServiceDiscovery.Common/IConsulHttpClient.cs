using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Consul.ServiceDiscovery.Common
{
    public interface IConsulHttpClient
    {
        Task<T> GetAsync<T>(string requestUri);
    }
}
