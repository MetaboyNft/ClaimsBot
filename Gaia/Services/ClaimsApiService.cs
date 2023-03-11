using Gaia.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gaia.Helpers;

namespace Gaia.Services
{
    public class ClaimsApiService : IDisposable
    {
        readonly RestClient _client;

        public ClaimsApiService(string baseUrl)
        {
            _client = new RestClient(baseUrl);
        }

        public async Task<List<Claimable>> GetClaimable()
        {
            var request = new RestRequest("api/nft/claimable?api-version=3.0");
            try
            {
                var response = await _client.GetAsync(request);
                var data = JsonConvert.DeserializeObject<List<Claimable>>(response.Content!);
                return data;
            }
            catch (HttpRequestException httpException)
            {
                ConsoleMessage.WriteMessage($"Error getting claimable from metaboy api: {httpException.Message}");
                return null;
            }
            catch (Newtonsoft.Json.JsonReaderException jSex)
            {
                ConsoleMessage.WriteMessage($"Error deserialising claimable json: {jSex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                ConsoleMessage.WriteMessage($"Error getting claimable from metaboy api: {ex.Message}");
                return null;
            }
        }

        public async Task<List<CanClaimV3>> GetRedeemable(string address)
        {
            var request = new RestRequest("api/nft/redeemable?api-version=3.0");
            request.AddParameter("address", address);
            try
            {
                // Receives back a list of Address, NftData, Amount as CanClaimV3
                var response = await _client.GetAsync(request);
                var data = JsonConvert.DeserializeObject<List<CanClaimV3>>(response.Content!);
                return data;
            }
            catch (HttpRequestException httpException)
            {
                ConsoleMessage.WriteMessage($"Error getting redeemable from metaboy api: {httpException.Message}");
                return null;
            }
            catch (Newtonsoft.Json.JsonReaderException jSex)
            {
                ConsoleMessage.WriteMessage($"Error deserialising redeemable json: {jSex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                ConsoleMessage.WriteMessage($"Error getting redeemable from metaboy api: {ex.Message}");
                return null;
            }
        }

        public async Task<string> AddClaim(List<NftReciever> nftRecievers)
        {
            // NftReceiver { string Address, string NftData, string Amount}
            var request = new RestRequest("api/nft/claim?api-version=3.0");
            request.AddJsonBody(nftRecievers);
            try
            {
                var response = await _client.PostAsync(request);
                var data = response.Content!;
                foreach (NftReciever nftReceiver in nftRecievers)
                {
                    Console.WriteLine($"[CLAIM SUBMITTED] Address: {nftReceiver.Address} Amount {nftReceiver.NftData}");
                }
                
                return data;
            }
            catch (HttpRequestException httpException)
            {
                ConsoleMessage.WriteMessage($"Error adding claim from metaboy api: {httpException.Message}");
                return null;
            }
            catch (Newtonsoft.Json.JsonReaderException jSex)
            {
                ConsoleMessage.WriteMessage($"Error adding claim json: {jSex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                ConsoleMessage.WriteMessage($"Error adding claim from metaboy api: {ex.Message}");
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