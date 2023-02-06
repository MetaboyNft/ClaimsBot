using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gaia.Models
{
    public class Ranks
    {
        public string name { get; set; }
        public List<Ranking> rankings { get; set; }
    }

    public class Ranking
    {
        public int Id { get; set; }
        public int Rank { get; set; }
        public double RarityScore { get; set; }
        public string MarketplaceUrl { get; set; }
    }

}
