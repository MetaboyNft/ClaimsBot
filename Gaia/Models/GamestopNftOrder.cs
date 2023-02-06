namespace Gaia
{
    public class GamestopNftOrder
    {
        public string orderId { get; set; }
        public string nftId { get; set; }
        public string nftData { get; set; }
        public string ownerAddress { get; set; }
        public int sellTokenId { get; set; }
        public int buyTokenId { get; set; }
        public string amount { get; set; }
        public string fulfilledAmount { get; set; }
        public string pricePerNft { get; set; }
        public int storageId { get; set; }
        public int validUntil { get; set; }
        public int makerFeeBips { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }
}
