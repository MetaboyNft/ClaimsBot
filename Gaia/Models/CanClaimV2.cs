using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gaia.Models
{
    public class CanClaimV2
    {
        public string Redeemable { get; set; } = "False";

        public string NftData { get; set; } = "";
        public int Amount { get; set; } = 0;
    }
}
