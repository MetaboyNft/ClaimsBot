using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gaia
{
    public class Settings
    {
        public string SqlServerConnectionString { get; set; }
        public ulong DiscordServerId { get; set; }
        public string DiscordToken { get; set; }
        public string ApiEndpointUrl { get; set; }
        public ulong ClaimsChannelId { get; set; }
        public ulong ClaimsAdminChannelId { get; set; }
        public ulong TeamChannelId { get; set; }
        public ulong GeneralChannelId { get; set; }
        public ulong ShowAndTellChannelId { get; set; }
        public ulong MarketChannelId { get; set; }
        public ulong MarketForumChannelId { get; set; }
        public string Description { get; set; }
        public string Environment { get; set; }

    }
}
