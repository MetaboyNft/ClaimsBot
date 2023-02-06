using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gaia.Models
{
    public class Result
    {
        public string LastBlock { get; set; }
        public string SafeGasPrice { get; set; }
        public string ProposeGasPrice { get; set; }
        public string FastGasPrice { get; set; }
        public string suggestBaseFee { get; set; }
        public string gasUsedRatio { get; set; }
    }

    public class EtherscanGas
    {
        public string status { get; set; }
        public string message { get; set; }
        public Result result { get; set; }
    }
}
