using Nethereum.Web3;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Nethereum.ENS;

namespace Gaia
{
    public class EthereumService
    {
        public async Task<string?> GetEnsFromAddessAsync(string? address)
        {
            var web3 = new Web3("https://mainnet.infura.io/v3/53173af3389645d18c3bcac2ee9a751c");
            var ensService = new ENSService(web3);

            try
            {
                return await ensService.ReverseResolveAsync(address);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.StackTrace + "\n" + e.Message);
                return null;
            }
        }
    }
}
