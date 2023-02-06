namespace Gaia
{
    public class Attribute
    {
        public string value { get; set; }
        public string trait_type { get; set; }
    }

    public class LoopringNftInfo
    {
        public List<string> nftData { get; set; }
    }

    public class MetadataJson
    {
        public string name { get; set; }
        public string image { get; set; }
        public List<Attribute> attributes { get; set; }
        public string description { get; set; }
        public string animation_url { get; set; }
        public int royalty_percentage { get; set; }
        public string collection_metadata { get; set; }
    }

    public class GamestopNftData
    {
        public string nftId { get; set; }
        public string tokenId { get; set; }
        public string contractAddress { get; set; }
        public string creatorEthAddress { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string amount { get; set; }
        public int royaltyFeeBips { get; set; }
        public object copyright { get; set; }
        public string nftType { get; set; }
        public bool mutable { get; set; }
        public string metadataUri { get; set; }
        public MetadataJson metadataJson { get; set; }
        public string mediaType { get; set; }
        public string mediaUri { get; set; }
        public string mediaThumbnailUri { get; set; }
        public object preRevealMediaType { get; set; }
        public object preRevealMediaUri { get; set; }
        public object preRevealMediaThumbnailUri { get; set; }
        public string collectionId { get; set; }
        public bool revealed { get; set; }
        public bool blocked { get; set; }
        public DateTime firstMintedAt { get; set; }
        public int likeCount { get; set; }
        public LoopringNftInfo loopringNftInfo { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }
}
