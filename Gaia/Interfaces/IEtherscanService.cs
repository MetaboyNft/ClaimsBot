using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gaia.Models;

namespace Gaia
{
    public interface IEtherscanService
    {
        Task<EtherscanGas> GetGas(string apiKey);
    }
}
