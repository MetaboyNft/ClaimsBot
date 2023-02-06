using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Gaia;
using Gaia.Models;
using Gaia.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();
Settings settings = config.GetRequiredSection("Settings").Get<Settings>();

var discord = new DiscordClient(new DiscordConfiguration()
{
    Token = settings.DiscordToken,
    TokenType = TokenType.Bot,
    Intents = DiscordIntents.AllUnprivileged,
    MinimumLogLevel = LogLevel.Debug
});

ClaimsApiService claims = new ClaimsApiService(settings.ApiEndpointUrl);

var slash = discord.UseSlashCommands(
new SlashCommandsConfiguration
{
    Services = new ServiceCollection()
    .AddSingleton<LoopringService>()
    .AddSingleton<EtherscanService>()
    .AddSingleton<Random>()
    .AddScoped<SqlService>()
    .AddSingleton<Settings>(settings)
    .AddSingleton<EthereumService>()
    .AddSingleton<GamestopService>()
    .AddSingleton<ClaimsApiService>(claims)
    .BuildServiceProvider()
}); ;

slash.RegisterCommands<ClaimsSlashCommands>(settings.DiscordServerId);

await discord.ConnectAsync();
await Task.Delay(-1);