namespace Gaia
{
    public static class RarityTierConverter
    {
        public static string RarityTier(int rarity, int rarityTotal, string? rarityTier = null)
        {
            decimal rarityDec = rarity;
            decimal rarityDecTotal = rarityTotal;

            if(!String.IsNullOrEmpty(rarityTier))
            {
                return rarityTier;
            }

            if(rarityDec == 0 || rarityDecTotal == 0)
            {
                return "N/A";
            }
            decimal rarityPercentageDecimal = (rarityDec / rarityTotal) * 100;

            if (rarityPercentageDecimal >= 80)
            {
                return "Common";

            }
            else if (rarityPercentageDecimal >= 60)
            {
                return "Uncommon";
            }

            else if (rarityPercentageDecimal >= 40)
            {
                return "Rare";

            }
            else if (rarityPercentageDecimal >= 20)
            {
                return "Epic";

            }
            else if (rarityPercentageDecimal >= 10)
            {
                return "Legendary";
            }
            else if (rarityPercentageDecimal >= 5)
            {
                return "Mythical";
            }
            else if(rarityPercentageDecimal >= 1)
            {
                return "Transcendent";
            }
            else
            {
                return "Godlike";
            }

        }
    }
}
