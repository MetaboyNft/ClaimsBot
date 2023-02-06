using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gaia
{
    public interface ILoopringService
    {
        Task<OffchainFee> GetOffChainFee(string apiKey, int accountId, int requestType, string tokenAddress);

        Task<StorageId> GetNextStorageId(string apiKey, int accountId, int sellTokenId);
        Task<OffchainFee> GetOffChainFeeNftTransfer(string apiKey, int accountId, int requestType, string amount);

        Task<NftBalance> GetNftBalance(string apiKey, int accountId, string nftData);

        Task<string> SubmitNftTransfer(
            string apiKey,
            string exchange,
            int fromAccountId,
            string fromAddress,
                 int toAccountId,
                 string toAddress,
                 int nftTokenId,
                 string nftAmount,
                 int maxFeeTokenId,
                 string maxFeeAmount,
                 int storageId,
                 long validUntil,
                 string eddsaSignature,
                 string ecdsaSignature,
                 string nftData
                 );

        Task<EnsResult> GetHexAddress(string apiKey, string ens);

        Task<EnsResult> GetENS(string apiKey, string hexAddress);
    }
}
