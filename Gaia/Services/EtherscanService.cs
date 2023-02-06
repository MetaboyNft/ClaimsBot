using Gaia.Models;
using Gaia.Helpers;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gaia
{
    public class EtherscanService : IEtherscanService, IDisposable
    {
        const string _baseUrl = "https://api.etherscan.io";
        readonly RestClient _client;

        public EtherscanService()
        {
            _client = new RestClient(_baseUrl);
        }

        public async Task<EtherscanGas> GetGas(string apiKey)
        {
            var request = new RestRequest($"/api?module=gastracker&action=gasoracle&apikey={apiKey}");
            try
            {
                var response = await _client.GetAsync(request);
                var data = JsonConvert.DeserializeObject<EtherscanGas>(response.Content!);
                return data;
            }
            catch (HttpRequestException httpException)
            {
                ConsoleMessage.WriteMessage($"Error getting gas: {httpException.Message}");
                return null;
            }
            catch (JsonReaderException sre)
            {
                ConsoleMessage.WriteMessage($"Error getting gas: {sre.Message}");
                return null;
            }
            catch (Exception ex)
            {
                ConsoleMessage.WriteMessage($"Error getting gas: {ex.Message}");
                return null;
            }
        }

        public void Dispose()
        {
            _client?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
