using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using Gaia.Helpers;

namespace Gaia
{
    public class GamestopService : IDisposable
    {
        const string _baseUrl = "https://api.nft.gamestop.com"; 
        readonly RestClient _client;
        public GamestopService()
        {
            _client = new RestClient(_baseUrl);
            _client.AddDefaultHeader("User-Agent", "Mozilla/5.0 Gecko/20100101 Firefox/106.0");
        }

        public async Task<GamestopNftData> GetNftData(string tokenId, string contractAddress)
        {
            var request = new RestRequest("nft-svc-marketplace/getNft");
            request.AddParameter("tokenIdAndContractAddress", $"{tokenId}_{contractAddress}");
            try
            {
                var response = await _client.GetAsync(request);
                var data = JsonConvert.DeserializeObject<GamestopNftData>(response.Content!);
                return data;
            }
            catch (HttpRequestException httpException)
            {
                ConsoleMessage.WriteMessage($"Error getting nft data from gamestop: {httpException.Message}");
                return null;
            }
            catch (Newtonsoft.Json.JsonReaderException jSex)
            {
                ConsoleMessage.WriteMessage($"Error deserialising json: {jSex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                ConsoleMessage.WriteMessage($"Error getting nft data from gamestop: {ex.Message}");
                return null;
            }
        }

        public async Task<List<GamestopNftOrder>> GetNftOrders(string nftId)
        {
            var request = new RestRequest("nft-svc-marketplace/getNftOrders");
            request.AddParameter("nftId", nftId);
            try
            {
                var response = await _client.GetAsync(request);
                var data = JsonConvert.DeserializeObject<List<GamestopNftOrder>>(response.Content!);
                return data;
            }
            catch (HttpRequestException httpException)
            {
                ConsoleMessage.WriteMessage($"Error getting nft order from gamestop: {httpException.Message}");
                return null;
            }
            catch (Newtonsoft.Json.JsonReaderException jSex)
            {
                ConsoleMessage.WriteMessage($"Error deserialising json: {jSex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                ConsoleMessage.WriteMessage($"Error getting nft order from gamestop: {ex.Message}");
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
