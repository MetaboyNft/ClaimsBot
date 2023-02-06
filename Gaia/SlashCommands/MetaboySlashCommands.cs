using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus;
using Gaia.Models;
using Gaia.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gaia.Services;
using System.Diagnostics;

namespace Gaia.SlashCommands
{
    public class MetaboySlashCommands : ApplicationCommandModule
    {
        public LoopringService LoopringService { private get; set; }
        public Random Random { private get; set; }

        public SqlService SqlService { private get; set; }

        public Settings Settings { private get; set; }

        public EthereumService EthereumService { private get; set; }

        public EtherscanService EtherscanService { private get; set; }

        public GamestopService GamestopService { private get; set; }

        public ClaimsApiService ClaimsApiService { private get; set; }

        public static Ranks Ranks { private get; set; }


        // Show Trade
        // Team and Market
        [SlashCommand("trade", "Show marketplace info on a MetaBoy")]
        public async Task MetaboyCommand(InteractionContext ctx, [Option("id", "The MetaBoy ID to Lookup, example: 420")] string id)
        {
            if (ctx.Channel.Id == Settings.TeamChannelId
                || ctx.Channel.Id == Settings.MarketChannelId
                || ctx.Channel.Id == Settings.MarketForumChannelId
                )
            {
                int metaboyId;
                bool canBeParsed = Int32.TryParse(id, out metaboyId);
                if (canBeParsed)
                {

                    var ranking = Ranks.rankings.FirstOrDefault(x => x.Id == metaboyId);
                    if (ranking == null)
                    {
                        var builder = new DiscordInteractionResponseBuilder()
                       .WithContent("Not a valid MetaBoy id!")
                       .AsEphemeral(true);
                        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                        return;
                    }
                    else
                    {
                        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
                        string contractAddress = ranking.MarketplaceUrl.Replace("https://nft.gamestop.com/token/", "").Split('/')[0];
                        string tokenId = ranking.MarketplaceUrl.Replace("https://nft.gamestop.com/token/", "").Split('/')[1];

                        var gamestopNFTData = await GamestopService.GetNftData(tokenId, contractAddress);
                        var gamestopNFTOrders = await GamestopService.GetNftOrders(gamestopNFTData.nftId);

                        var rarityTier = RarityTierConverter.RarityTier(ranking.Rank, 8661);

                        var embedColour = "";

                        switch (rarityTier)
                        {
                            case "Common":
                                embedColour = "#FFFFFF"; //white
                                break;
                            case "Uncommon":
                                embedColour = "#1EFF00"; //green
                                break;
                            case "Rare":
                                embedColour = "#0070DD"; //blue
                                break;
                            case "Epic":
                                embedColour = "#A335EE"; //purple
                                break;
                            case "Legendary":
                                embedColour = "#FF8000"; //orange
                                break;
                            case "Mythical":
                                embedColour = "#E6CC80"; //light gold
                                break;
                            case "Transcendent":
                                embedColour = "#00CCFF"; //cyan
                                break;
                            case "Godlike":
                                embedColour = "#FD0000"; //gme red
                                break;
                        }

                        var imageUrl = $"https://looprarecdn.azureedge.net/images/metaboys/full/{metaboyId}.gif";
                        var embed = new DiscordEmbedBuilder()
                        {
                            Title = $"Metaboy #{metaboyId}",
                            Url = ranking.MarketplaceUrl,
                            Color = new DiscordColor(embedColour)
                        };
                        embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() { Url = imageUrl, Height = 256, Width = 256 };
                        embed.AddField("Rank", ranking.Rank.ToString());

                        if (gamestopNFTOrders != null && gamestopNFTOrders.Count > 0)
                        {
                            var gamestopNFTOrder = gamestopNFTOrders.OrderByDescending(x => x.createdAt).ToList().FirstOrDefault();
                            var salePriceText = "";
                            if (gamestopNFTOrder.buyTokenId == 0)
                            {
                                salePriceText = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder.pricePerNft), 18)} ETH";
                            }
                            else if (gamestopNFTOrder.buyTokenId == 1)
                            {
                                salePriceText = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder.pricePerNft), 18)} LRC";
                            }
                            embed.AddField("List Price", salePriceText);
                        }
                        else if (gamestopNFTOrders != null && gamestopNFTOrders.Count == 0)
                        {
                            embed.AddField("List Price", "Not Listed!");
                        }

                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                        return;

                    }
                }
                else
                {
                    var builder = new DiscordInteractionResponseBuilder()
                      .WithContent("Not a valid MetaBoy id!")
                      .AsEphemeral(true);
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                    return;
                }
            }
            else
            {
                var builder = new DiscordInteractionResponseBuilder()
                      .WithContent("This command is not enabled in this channel")
                      .AsEphemeral(true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                return;
            }


        }

        // Show MetaBoy
        // Team, General, ShowAndTell
        [SlashCommand("show", "Show a MetaBoy")]
        public async Task ShowCommand(InteractionContext ctx, [Option("id", "The MetaBoy ID to Lookup, example: 420")] string id)
        {

            if (ctx.Channel.Id == Settings.TeamChannelId
                || ctx.Channel.Id == Settings.GeneralChannelId
                || ctx.Channel.Id == Settings.ShowAndTellChannelId
                )
            {
                int metaboyId;
                bool canBeParsed = Int32.TryParse(id, out metaboyId);
                if (canBeParsed)
                {
                    var ranking = Ranks.rankings.FirstOrDefault(x => x.Id == metaboyId);
                    if (ranking == null)
                    {
                        var builder = new DiscordInteractionResponseBuilder()
                       .WithContent("Not a valid MetaBoy id!")
                       .AsEphemeral(true);
                        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                        return;
                    }
                    else
                    {
                        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
                        var rarityTier = RarityTierConverter.RarityTier(ranking.Rank, 8661);

                        var embedColour = "";

                        switch (rarityTier)
                        {
                            case "Common":
                                embedColour = "#FFFFFF"; //white
                                break;
                            case "Uncommon":
                                embedColour = "#1EFF00"; //green
                                break;
                            case "Rare":
                                embedColour = "#0070DD"; //blue
                                break;
                            case "Epic":
                                embedColour = "#A335EE"; //purple
                                break;
                            case "Legendary":
                                embedColour = "#FF8000"; //orange
                                break;
                            case "Mythical":
                                embedColour = "#E6CC80"; //light gold
                                break;
                            case "Transcendent":
                                embedColour = "#00CCFF"; //cyan
                                break;
                            case "Godlike":
                                embedColour = "#FD0000"; //gme red
                                break;
                        }
                        var imageUrl = $"https://looprarecdn.azureedge.net/images/metaboys/full/{metaboyId}.gif";
                        var embed = new DiscordEmbedBuilder()
                        {
                            Title = $"Metaboy #{metaboyId}, Rank {ranking.Rank}",
                            Url = ranking.MarketplaceUrl,
                            ImageUrl = imageUrl,
                            Color = new DiscordColor(embedColour)
                        };
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                        return;
                    }
                }
                else
                {
                    var builder = new DiscordInteractionResponseBuilder()
                    .WithContent("Not a valid MetaBoy id!")
                    .AsEphemeral(true);
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                    return;
                }
            }
            else
            {
                var builder = new DiscordInteractionResponseBuilder()
                .WithContent("This command is not enabled in this channel")
                .AsEphemeral(true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                return;
            }
        }

        // Show Transparent
        // Team, General, ShowAndTell
        [SlashCommand("show_transparent", "Show a transparent MetaBoy")]
        public async Task ShowTransparentCommand(InteractionContext ctx, [Option("id", "The MetaBoy ID to Lookup, example: 420")] string id)
        {
            if (ctx.Channel.Id == Settings.TeamChannelId
                || ctx.Channel.Id == Settings.GeneralChannelId
                || ctx.Channel.Id == Settings.ShowAndTellChannelId
                )
            {
                int metaboyId;
                bool canBeParsed = Int32.TryParse(id, out metaboyId);
                if (canBeParsed)
                {
                    var ranking = Ranks.rankings.FirstOrDefault(x => x.Id == metaboyId);
                    if (ranking == null)
                    {
                        var builder = new DiscordInteractionResponseBuilder()
                       .WithContent("Not a valid MetaBoy id!")
                       .AsEphemeral(true);
                        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                        return;
                    }
                    else
                    {
                        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
                        var rarityTier = RarityTierConverter.RarityTier(ranking.Rank, 8661);

                        var embedColour = "";

                        switch (rarityTier)
                        {
                            case "Common":
                                embedColour = "#FFFFFF"; //white
                                break;
                            case "Uncommon":
                                embedColour = "#1EFF00"; //green
                                break;
                            case "Rare":
                                embedColour = "#0070DD"; //blue
                                break;
                            case "Epic":
                                embedColour = "#A335EE"; //purple
                                break;
                            case "Legendary":
                                embedColour = "#FF8000"; //orange
                                break;
                            case "Mythical":
                                embedColour = "#E6CC80"; //light gold
                                break;
                            case "Transcendent":
                                embedColour = "#00CCFF"; //cyan
                                break;
                            case "Godlike":
                                embedColour = "#FD0000"; //gme red
                                break;
                        }
                        var imageUrl = $"https://looprarecdn.azureedge.net/images/metaboy/transparent/{metaboyId}_cropped.gif";
                        var thumbnailUrl = $"https://looprarecdn.azureedge.net/images/metaboy/transparent/{metaboyId}_tiny.gif";
                        var embed = new DiscordEmbedBuilder()
                        {
                            Title = $"Metaboy #{metaboyId}, Rank {ranking.Rank}",
                            Url = ranking.MarketplaceUrl,
                            ImageUrl = imageUrl,
                            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() { Url = thumbnailUrl },
                            Color = new DiscordColor(embedColour)
                        };
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                        return;
                    }
                }
                else
                {
                    var builder = new DiscordInteractionResponseBuilder()
                    .WithContent("Not a valid MetaBoy id!")
                    .AsEphemeral(true);
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                    return;
                }
            }
            else
            {
                var builder = new DiscordInteractionResponseBuilder()
                .WithContent("This command is not enabled in this channel")
                .AsEphemeral(true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                return;
            }
        }

        // Honorary
        // Team, Market, ShowAndTell
        [SlashCommand("honorary", "Show information on Honorary Collection")]
        public async Task AstroCommand(InteractionContext ctx)
        {
            if (ctx.Channel.Id == Settings.TeamChannelId
                || ctx.Channel.Id == Settings.MarketChannelId
                || ctx.Channel.Id == Settings.ShowAndTellChannelId
                || ctx.Channel.Id == Settings.MarketForumChannelId
                )
            {
                try
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

                    //astro
                    var gamestopNFTData = await GamestopService.GetNftData("0xd8ada153c760d4acce89d9e612939ea7cc4f0cfc43707e423eb16476e293ff95", "0x1d006a27bd82e10f9194d30158d91201e9930420");
                    var gamestopNFTOrders = await GamestopService.GetNftOrders(gamestopNFTData.nftId);

                    //monkey
                    var gamestopNFTData2 = await GamestopService.GetNftData("0x930ff4e66577c22563dc8060e0a48ab4b6f0fcebdffa42a03e8a579c5d6b1503", "0x1d006a27bd82e10f9194d30158d91201e9930420");
                    var gamestopNFTOrders2 = await GamestopService.GetNftOrders(gamestopNFTData2.nftId);

                    //froggi
                    var gamestopNFTData3 = await GamestopService.GetNftData("0x8ed8173e66a07c49391b5aee318777258f6a96eafbb0daaf3f7f884a52a33ba4", "0x1d006a27bd82e10f9194d30158d91201e9930420");
                    var gamestopNFTOrders3 = await GamestopService.GetNftOrders(gamestopNFTData3.nftId);

                    //ordinary adam
                    var gamestopNFTData4 = await GamestopService.GetNftData("0x5573521b417e6757c77099578e791f889f0b7495eaf5b29093b2902bfe6813cf", "0x1d006a27bd82e10f9194d30158d91201e9930420");
                    var gamestopNFTOrders4 = await GamestopService.GetNftOrders(gamestopNFTData4.nftId);

                    //british
                    var gamestopNFTData5 = await GamestopService.GetNftData("0x445671e5df1feac09afb4a92f5b2cb9337dbd94abe2c6b55a0855e03cbc4653b", "0x1d006a27bd82e10f9194d30158d91201e9930420");
                    var gamestopNFTOrders5 = await GamestopService.GetNftOrders(gamestopNFTData5.nftId);

                    //reporter
                    var gamestopNFTData6 = await GamestopService.GetNftData("0xb34b96e2294f7b79b6af3f576758febcee688977054438328fcf3e76e9fb9742", "0x1d006a27bd82e10f9194d30158d91201e9930420");
                    var gamestopNFTOrders6 = await GamestopService.GetNftOrders(gamestopNFTData6.nftId);

                    //rockstar
                    var gamestopNFTData7 = await GamestopService.GetNftData("0xb3ca435a05ac1f67102258a14d1ac4687e4a00ca8fd838d7b0faf8ac994eb839", "0x1d006a27bd82e10f9194d30158d91201e9930420");
                    var gamestopNFTOrders7 = await GamestopService.GetNftOrders(gamestopNFTData7.nftId);

                    var imageUrl = $"https://looprarecdn.azureedge.net/images/metaboyhonorary/full/astroboy.gif";
                    var embed = new DiscordEmbedBuilder()
                    {
                        Title = $"Honorary",
                        Color = new DiscordColor("#FD0000"),
                        Url = "https://nft.gamestop.com/collection/MetaBoyHonorary"
                    };
                    embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() { Url = imageUrl, Height = 256, Width = 256 };

                    var gamestopNFTOrder = gamestopNFTOrders.OrderBy(x => Double.Parse(x.pricePerNft)).ToList()[0];
                    var salePriceText = "";
                    if (gamestopNFTOrder.buyTokenId == 0)
                    {
                        salePriceText = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder.pricePerNft), 18)} ETH";
                    }
                    else if (gamestopNFTOrder.buyTokenId == 1)
                    {
                        salePriceText = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder.pricePerNft), 18)} LRC";
                    }
                    embed.AddField("AstroBoy List Price", $"[{salePriceText}](https://nft.gamestop.com/token/0x1d006a27bd82e10f9194d30158d91201e9930420/0xd8ada153c760d4acce89d9e612939ea7cc4f0cfc43707e423eb16476e293ff95)", true);

                    var gamestopNFTOrder5 = gamestopNFTOrders5.OrderBy(x => Double.Parse(x.pricePerNft)).ToList()[0];
                    var salePriceText5 = "";
                    if (gamestopNFTOrder5.buyTokenId == 0)
                    {
                        salePriceText5 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder5.pricePerNft), 18)} ETH";
                    }
                    else if (gamestopNFTOrder5.buyTokenId == 1)
                    {
                        salePriceText5 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder5.pricePerNft), 18)} LRC";
                    }
                    embed.AddField("BritishBoy List Price", $"[{salePriceText5}](https://nft.gamestop.com/token/0x1d006a27bd82e10f9194d30158d91201e9930420/0x445671e5df1feac09afb4a92f5b2cb9337dbd94abe2c6b55a0855e03cbc4653b)");

                    var gamestopNFTOrder3 = gamestopNFTOrders3.OrderBy(x => Double.Parse(x.pricePerNft)).ToList()[0];
                    var salePriceText3 = "";
                    if (gamestopNFTOrder3.buyTokenId == 0)
                    {
                        salePriceText3 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder3.pricePerNft), 18)} ETH";
                    }
                    else if (gamestopNFTOrder3.buyTokenId == 1)
                    {
                        salePriceText3 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder3.pricePerNft), 18)} LRC";
                    }
                    embed.AddField("FroggiBoy List Price", $"[{salePriceText3}](https://nft.gamestop.com/token/0x1d006a27bd82e10f9194d30158d91201e9930420/0x8ed8173e66a07c49391b5aee318777258f6a96eafbb0daaf3f7f884a52a33ba4)");

                    var gamestopNFTOrder2 = gamestopNFTOrders2.OrderBy(x => Double.Parse(x.pricePerNft)).ToList()[0];
                    var salePriceText2 = "";
                    if (gamestopNFTOrder2.buyTokenId == 0)
                    {
                        salePriceText2 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder2.pricePerNft), 18)} ETH";
                    }
                    else if (gamestopNFTOrder2.buyTokenId == 1)
                    {
                        salePriceText2 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder2.pricePerNft), 18)} LRC";
                    }
                    embed.AddField("MonkeyBoy List Price", $"[{salePriceText2}](https://nft.gamestop.com/token/0x1d006a27bd82e10f9194d30158d91201e9930420/0x930ff4e66577c22563dc8060e0a48ab4b6f0fcebdffa42a03e8a579c5d6b1503)");

                    var gamestopNFTOrder4 = gamestopNFTOrders4.OrderBy(x => Double.Parse(x.pricePerNft)).ToList()[0];
                    var salePriceText4 = "";
                    if (gamestopNFTOrder4.buyTokenId == 0)
                    {
                        salePriceText4 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder4.pricePerNft), 18)} ETH";
                    }
                    else if (gamestopNFTOrder4.buyTokenId == 1)
                    {
                        salePriceText4 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder4.pricePerNft), 18)} LRC";
                    }
                    embed.AddField("OrdinaryAdamBoy List Price", $"[{salePriceText4}](https://nft.gamestop.com/token/0x1d006a27bd82e10f9194d30158d91201e9930420/0x5573521b417e6757c77099578e791f889f0b7495eaf5b29093b2902bfe6813cf)");

                    var gamestopNFTOrder6 = gamestopNFTOrders6.OrderBy(x => Double.Parse(x.pricePerNft)).ToList()[0];
                    var salePriceText6 = "";
                    if (gamestopNFTOrder6.buyTokenId == 0)
                    {
                        salePriceText6 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder6.pricePerNft), 18)} ETH";
                    }
                    else if (gamestopNFTOrder6.buyTokenId == 1)
                    {
                        salePriceText6 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder6.pricePerNft), 18)} LRC";
                    }
                    embed.AddField("ReporterBoy List Price", $"[{salePriceText6}](https://nft.gamestop.com/token/0x1d006a27bd82e10f9194d30158d91201e9930420/0xb34b96e2294f7b79b6af3f576758febcee688977054438328fcf3e76e9fb9742)");

                    var gamestopNFTOrder7 = gamestopNFTOrders7.OrderBy(x => Double.Parse(x.pricePerNft)).ToList()[0];
                    var salePriceText7 = "";
                    if (gamestopNFTOrder7.buyTokenId == 0)
                    {
                        salePriceText7 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder7.pricePerNft), 18)} ETH";
                    }
                    else if (gamestopNFTOrder7.buyTokenId == 1)
                    {
                        salePriceText7 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder7.pricePerNft), 18)} LRC";
                    }
                    embed.AddField("RockstarBoy List Price", $"[{salePriceText7}](https://nft.gamestop.com/token/0x1d006a27bd82e10f9194d30158d91201e9930420/0xb3ca435a05ac1f67102258a14d1ac4687e4a00ca8fd838d7b0faf8ac994eb839)");

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    return;
                }
                catch (Exception ex)
                {
                    var embed = new DiscordEmbedBuilder()
                    {
                        Title = $"Honorary",
                        Color = new DiscordColor("#FD0000"),
                        Url = "https://nft.gamestop.com/collection/MetaBoyHonorary"
                    };

                    var imageUrl = $"https://looprarecdn.azureedge.net/images/metaboyhonorary/full/astroboy.gif";
                    embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() { Url = imageUrl, Height = 256, Width = 256 };
                    embed.AddField("Oops!", "Something went wrong!");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    return;
                }
            }
            else
            {
                var builder = new DiscordInteractionResponseBuilder()
                .WithContent("This command is not enabled in this channel!")
                .AsEphemeral(true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                return;
            }
        }

        // GMerica
        // Team, Market, ShowAndTell
        [SlashCommand("gmerica", "Show information on GMErica Collection")]
        public async Task GMEricaCommand(InteractionContext ctx)
        {
            if (ctx.Channel.Id == Settings.TeamChannelId
                || ctx.Channel.Id == Settings.MarketChannelId
                || ctx.Channel.Id == Settings.ShowAndTellChannelId
                || ctx.Channel.Id == Settings.MarketForumChannelId
                )
            {
                try
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

                    //boston
                    var gamestopNFTData = await GamestopService.GetNftData("0x8c34436aa426f27077a6c9df5c314bd6013c85f3201d6ec3520a6cc0a706e271", "0x0c589fcd20f99a4a1fe031f50079cfc630015184");
                    var gamestopNFTOrders = await GamestopService.GetNftOrders(gamestopNFTData.nftId);

                    //canaveral
                    var gamestopNFTData2 = await GamestopService.GetNftData("0x5fa20ebf94a504c98b09b4c9e14ad96644effc7e05c8009e7f1eacfe4d194ed2", "0x0c589fcd20f99a4a1fe031f50079cfc630015184");
                    var gamestopNFTOrders2 = await GamestopService.GetNftOrders(gamestopNFTData2.nftId);

                    //hollywood
                    var gamestopNFTData3 = await GamestopService.GetNftData("0xe83cbd0ce56c8d986b31075e3bcd9f5974b76d06d48c877a4e5faaac70264636", "0x0c589fcd20f99a4a1fe031f50079cfc630015184");
                    var gamestopNFTOrders3 = await GamestopService.GetNftOrders(gamestopNFTData3.nftId);

                    //nyc
                    var gamestopNFTData4 = await GamestopService.GetNftData("0x8d3e53420e7f15a1ac5b54aed3eaa429b5e75046abb1af99d5b5040ed1beea0a", "0x0c589fcd20f99a4a1fe031f50079cfc630015184");
                    var gamestopNFTOrders4 = await GamestopService.GetNftOrders(gamestopNFTData4.nftId);

                    //seattle
                    var gamestopNFTData5 = await GamestopService.GetNftData("0x9bf31ca7985ac20239c026ae15b4c9241aaf06cab6365d92f98c99b34b409d60", "0x0c589fcd20f99a4a1fe031f50079cfc630015184");
                    var gamestopNFTOrders5 = await GamestopService.GetNftOrders(gamestopNFTData5.nftId);


                    var imageUrl = $"https://looprarecdn.azureedge.net/images/metaboygmerica/gmerica.gif";
                    var embed = new DiscordEmbedBuilder()
                    {
                        Title = $"GMErica",
                        Color = new DiscordColor("#FD0000"),
                        Url = "https://nft.gamestop.com/collection/Gmricaxmetaboy"
                    };
                    embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() { Url = imageUrl, Height = 256, Width = 256 };

                    var gamestopNFTOrder = gamestopNFTOrders.OrderBy(x => Double.Parse(x.pricePerNft)).ToList()[0];
                    var salePriceText = "";
                    if (gamestopNFTOrder.buyTokenId == 0)
                    {
                        salePriceText = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder.pricePerNft), 18)} ETH";
                    }
                    else if (gamestopNFTOrder.buyTokenId == 1)
                    {
                        salePriceText = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder.pricePerNft), 18)} LRC";
                    }
                    embed.AddField("Boston List Price", $"[{salePriceText}](https://nft.gamestop.com/token/0x0c589fcd20f99a4a1fe031f50079cfc630015184/0x8c34436aa426f27077a6c9df5c314bd6013c85f3201d6ec3520a6cc0a706e271)", true);

                    var gamestopNFTOrder2 = gamestopNFTOrders2.OrderBy(x => Double.Parse(x.pricePerNft)).ToList()[0];
                    var salePriceText2 = "";
                    if (gamestopNFTOrder2.buyTokenId == 0)
                    {
                        salePriceText2 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder2.pricePerNft), 18)} ETH";
                    }
                    else if (gamestopNFTOrder2.buyTokenId == 1)
                    {
                        salePriceText2 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder2.pricePerNft), 18)} LRC";
                    }
                    embed.AddField("Cape Canaveral List Price", $"[{salePriceText2}](https://nft.gamestop.com/token/0x0c589fcd20f99a4a1fe031f50079cfc630015184/0x5fa20ebf94a504c98b09b4c9e14ad96644effc7e05c8009e7f1eacfe4d194ed2)");

                    var gamestopNFTOrder3 = gamestopNFTOrders3.OrderBy(x => Double.Parse(x.pricePerNft)).ToList()[0];
                    var salePriceText3 = "";
                    if (gamestopNFTOrder3.buyTokenId == 0)
                    {
                        salePriceText3 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder3.pricePerNft), 18)} ETH";
                    }
                    else if (gamestopNFTOrder3.buyTokenId == 1)
                    {
                        salePriceText3 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder3.pricePerNft), 18)} LRC";
                    }
                    embed.AddField("Hollywood List Price", $"[{salePriceText3}](https://nft.gamestop.com/token/0x0c589fcd20f99a4a1fe031f50079cfc630015184/0xe83cbd0ce56c8d986b31075e3bcd9f5974b76d06d48c877a4e5faaac70264636)");

                    var gamestopNFTOrder4 = gamestopNFTOrders4.OrderBy(x => Double.Parse(x.pricePerNft)).ToList()[0];
                    var salePriceText4 = "";
                    if (gamestopNFTOrder4.buyTokenId == 0)
                    {
                        salePriceText4 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder4.pricePerNft), 18)} ETH";
                    }
                    else if (gamestopNFTOrder4.buyTokenId == 1)
                    {
                        salePriceText4 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder4.pricePerNft), 18)} LRC";
                    }
                    embed.AddField("NYC List Price", $"[{salePriceText4}](https://nft.gamestop.com/token/0x0c589fcd20f99a4a1fe031f50079cfc630015184/0x8d3e53420e7f15a1ac5b54aed3eaa429b5e75046abb1af99d5b5040ed1beea0a)");


                    var gamestopNFTOrder5 = gamestopNFTOrders5.OrderBy(x => Double.Parse(x.pricePerNft)).ToList()[0];
                    var salePriceText5 = "";
                    if (gamestopNFTOrder5.buyTokenId == 0)
                    {
                        salePriceText5 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder5.pricePerNft), 18)} ETH";
                    }
                    else if (gamestopNFTOrder5.buyTokenId == 1)
                    {
                        salePriceText5 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder5.pricePerNft), 18)} LRC";
                    }
                    embed.AddField("Seattle List Price", $"[{salePriceText5}](https://nft.gamestop.com/token/0x0c589fcd20f99a4a1fe031f50079cfc630015184/0x9bf31ca7985ac20239c026ae15b4c9241aaf06cab6365d92f98c99b34b409d60)");



                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    return;
                }
                catch (Exception ex)
                {
                    var embed = new DiscordEmbedBuilder()
                    {
                        Title = $"GMErica",
                        Color = new DiscordColor("#FD0000"),
                        Url = "https://nft.gamestop.com/collection/Gmricaxmetaboy"
                    };

                    var imageUrl = $"https://looprarecdn.azureedge.net/images/metaboygmerica/gmerica.gif";
                    embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() { Url = imageUrl, Height = 256, Width = 256 };
                    embed.AddField("Oops!", "Something went wrong!");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    return;
                }
            }
            else
            {
                var builder = new DiscordInteractionResponseBuilder()
                .WithContent("This command is not enabled in this channel!")
                .AsEphemeral(true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                return;
            }
        }

        // Airdrop
        // Team, Market, ShowAndTell
        [SlashCommand("airdrop", "Show information on Airdrop Collection")]
        public async Task AirdropCommand(InteractionContext ctx)
        {
            if (ctx.Channel.Id == Settings.TeamChannelId
                || ctx.Channel.Id == Settings.MarketChannelId
                || ctx.Channel.Id == Settings.ShowAndTellChannelId
                || ctx.Channel.Id == Settings.MarketForumChannelId
                )
            {
                try
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

                    //dalmatian metadog
                    var gamestopNFTData = await GamestopService.GetNftData("0xbbcbb61afe23eeadc4a6ca0d8b8379c45e4f846797bf79cb01a23811b87b38ce", "0x1d006a27bd82e10f9194d30158d91201e9930420");
                    var gamestopNFTOrders = await GamestopService.GetNftOrders(gamestopNFTData.nftId);

                    //mountain metadog
                    var gamestopNFTData2 = await GamestopService.GetNftData("0x004dec4dc078179e624487c2380394a8874a256ce75f781a6e07da3c209c8235", "0x1d006a27bd82e10f9194d30158d91201e9930420");
                    var gamestopNFTOrders2 = await GamestopService.GetNftOrders(gamestopNFTData2.nftId);

                    //chihuahua metadog
                    var gamestopNFTData3 = await GamestopService.GetNftData("0x3615d66402275f0276cd66961e4ff81a828ff51fbecc5b27fa064868231e94dd", "0x1d006a27bd82e10f9194d30158d91201e9930420");
                    var gamestopNFTOrders3 = await GamestopService.GetNftOrders(gamestopNFTData3.nftId);

                    //bordercollie metadog
                    var gamestopNFTData4 = await GamestopService.GetNftData("0x0076ecba9f6f87b3e97ad987a6aaec1ec40af97a0c1a6daf1b6ffd0a012ee620", "0x1d006a27bd82e10f9194d30158d91201e9930420");
                    var gamestopNFTOrders4 = await GamestopService.GetNftOrders(gamestopNFTData4.nftId);

                    //retriever metadog
                    var gamestopNFTData5 = await GamestopService.GetNftData("0xbf024aa2ebf7c4b137124c46f64a50a2c8e3cf733a7af5aac79f56bc61c6165f", "0x1d006a27bd82e10f9194d30158d91201e9930420");
                    var gamestopNFTOrders5 = await GamestopService.GetNftOrders(gamestopNFTData5.nftId);

                    //bedroom metacat
                    var gamestopNFTData6 = await GamestopService.GetNftData("0x80f1525cb6cea164781a2de003564c323bfcafc6d7dbb5c111a370bae95cda73", "0x1d006a27bd82e10f9194d30158d91201e9930420");
                    var gamestopNFTOrders6 = await GamestopService.GetNftOrders(gamestopNFTData6.nftId);


                    var imageUrl = $"https://looprarecdn.azureedge.net/images/metaboyairdrop/airdrop.gif";
                    var embed = new DiscordEmbedBuilder()
                    {
                        Title = $"Airdrop",
                        Color = new DiscordColor("#FD0000"),
                        Url = "https://nft.gamestop.com/collection/MetaBoyAirdrop"
                    };
                    embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() { Url = imageUrl, Height = 256, Width = 256 };

                    var gamestopNFTOrder = gamestopNFTOrders.OrderBy(x => Double.Parse(x.pricePerNft)).ToList()[0];
                    var salePriceText = "";
                    if (gamestopNFTOrder.buyTokenId == 0)
                    {
                        salePriceText = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder.pricePerNft), 18)} ETH";
                    }
                    else if (gamestopNFTOrder.buyTokenId == 1)
                    {
                        salePriceText = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder.pricePerNft), 18)} LRC";
                    }
                    embed.AddField("DalmatianMetaDog List Price", $"[{salePriceText}](https://nft.gamestop.com/token/0x1d006a27bd82e10f9194d30158d91201e9930420/0xbbcbb61afe23eeadc4a6ca0d8b8379c45e4f846797bf79cb01a23811b87b38ce)", true);

                    var gamestopNFTOrder2 = gamestopNFTOrders2.OrderBy(x => Double.Parse(x.pricePerNft)).ToList()[0];
                    var salePriceText2 = "";
                    if (gamestopNFTOrder2.buyTokenId == 0)
                    {
                        salePriceText2 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder2.pricePerNft), 18)} ETH";
                    }
                    else if (gamestopNFTOrder2.buyTokenId == 1)
                    {
                        salePriceText2 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder2.pricePerNft), 18)} LRC";
                    }
                    embed.AddField("MountainMetaDog List Price", $"[{salePriceText2}](https://nft.gamestop.com/token/0x1d006a27bd82e10f9194d30158d91201e9930420/0x004dec4dc078179e624487c2380394a8874a256ce75f781a6e07da3c209c8235)");

                    var gamestopNFTOrder3 = gamestopNFTOrders3.OrderBy(x => Double.Parse(x.pricePerNft)).ToList()[0];
                    var salePriceText3 = "";
                    if (gamestopNFTOrder3.buyTokenId == 0)
                    {
                        salePriceText3 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder3.pricePerNft), 18)} ETH";
                    }
                    else if (gamestopNFTOrder3.buyTokenId == 1)
                    {
                        salePriceText3 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder3.pricePerNft), 18)} LRC";
                    }
                    embed.AddField("ChihuahuaMetaDog List Price", $"[{salePriceText3}](https://nft.gamestop.com/token/0x1d006a27bd82e10f9194d30158d91201e9930420/0x3615d66402275f0276cd66961e4ff81a828ff51fbecc5b27fa064868231e94dd)");

                    var gamestopNFTOrder4 = gamestopNFTOrders4.OrderBy(x => Double.Parse(x.pricePerNft)).ToList()[0];
                    var salePriceText4 = "";
                    if (gamestopNFTOrder4.buyTokenId == 0)
                    {
                        salePriceText4 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder4.pricePerNft), 18)} ETH";
                    }
                    else if (gamestopNFTOrder4.buyTokenId == 1)
                    {
                        salePriceText4 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder4.pricePerNft), 18)} LRC";
                    }
                    embed.AddField("BorderCollieMetaDog List Price", $"[{salePriceText4}](https://nft.gamestop.com/token/0x1d006a27bd82e10f9194d30158d91201e9930420/0x0076ecba9f6f87b3e97ad987a6aaec1ec40af97a0c1a6daf1b6ffd0a012ee620)");


                    var gamestopNFTOrder5 = gamestopNFTOrders5.OrderBy(x => Double.Parse(x.pricePerNft)).ToList()[0];
                    var salePriceText5 = "";
                    if (gamestopNFTOrder5.buyTokenId == 0)
                    {
                        salePriceText5 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder5.pricePerNft), 18)} ETH";
                    }
                    else if (gamestopNFTOrder5.buyTokenId == 1)
                    {
                        salePriceText5 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder5.pricePerNft), 18)} LRC";
                    }
                    embed.AddField("RetrieverMetaDog List Price", $"[{salePriceText5}](https://nft.gamestop.com/token/0x1d006a27bd82e10f9194d30158d91201e9930420/0xbf024aa2ebf7c4b137124c46f64a50a2c8e3cf733a7af5aac79f56bc61c6165f)");

                    var gamestopNFTOrder6 = gamestopNFTOrders6.OrderBy(x => Double.Parse(x.pricePerNft)).ToList()[0];
                    var salePriceText6 = "";
                    if (gamestopNFTOrder6.buyTokenId == 0)
                    {
                        salePriceText6 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder6.pricePerNft), 18)} ETH";
                    }
                    else if (gamestopNFTOrder6.buyTokenId == 1)
                    {
                        salePriceText6 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder6.pricePerNft), 18)} LRC";
                    }
                    embed.AddField("BedroomMetaCat List Price", $"[{salePriceText6}](https://nft.gamestop.com/token/0x1d006a27bd82e10f9194d30158d91201e9930420/0x80f1525cb6cea164781a2de003564c323bfcafc6d7dbb5c111a370bae95cda73)");

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    return;
                }
                catch (Exception ex)
                {
                    var embed = new DiscordEmbedBuilder()
                    {
                        Title = $"Airdrop",
                        Color = new DiscordColor("#FD0000"),
                        Url = "https://nft.gamestop.com/collection/MetaBoyAirdrop"
                    };

                    var imageUrl = $"https://looprarecdn.azureedge.net/images/metaboyhairdrop/airdrop.gif";
                    embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() { Url = imageUrl, Height = 256, Width = 256 };
                    embed.AddField("Oops!", "Something went wrong!");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    return;
                }
            }
            else
            {
                var builder = new DiscordInteractionResponseBuilder()
                .WithContent("This command is not enabled in this channel!")
                .AsEphemeral(true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                return;
            }
        }

        // Celebratory
        // Team, Market, ShowAndTell
        [SlashCommand("Celebratory", "Show information on Celebratory Collection")]
        public async Task CelebratoryCommand(InteractionContext ctx)
        {
            if (ctx.Channel.Id == Settings.TeamChannelId
                || ctx.Channel.Id == Settings.MarketChannelId
                || ctx.Channel.Id == Settings.ShowAndTellChannelId
                || ctx.Channel.Id == Settings.MarketForumChannelId
                )
            {
                try
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

                    // Ethboy
                    var gamestopNFTData = await GamestopService.GetNftData("0x2a669f944bb80efdcdd1c86ad1fc340a4803210dce371d03d00f450a33ec11c6", "0x1d006a27bd82e10f9194d30158d91201e9930420");
                    var gamestopNFTOrders = await GamestopService.GetNftOrders(gamestopNFTData.nftId);
                    // Fighterboy
                    var gamestopNFTData2 = await GamestopService.GetNftData("0xd79e1ff7615e2826b2e4e29bbfd6cfa1d4109da4cbabf726b4690e1c9d1b411e", "0x1d006a27bd82e10f9194d30158d91201e9930420");
                    var gamestopNFTOrders2 = await GamestopService.GetNftOrders(gamestopNFTData2.nftId);

                    var imageUrl = $"https://looprarecdn.azureedge.net/images/metaboycelebratory/ethboy.gif";
                    var embed = new DiscordEmbedBuilder()
                    {
                        Title = $"Celebratory",
                        Color = new DiscordColor("#FD0000"),
                        Url = "https://nft.gamestop.com/collection/celebratorymetaboy"
                    };
                    embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() { Url = imageUrl, Height = 256, Width = 256 };

                    var gamestopNFTOrder = gamestopNFTOrders.OrderBy(x => Double.Parse(x.pricePerNft)).ToList()[0];
                    var salePriceText = "";
                    if (gamestopNFTOrder.buyTokenId == 0)
                    {
                        salePriceText = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder.pricePerNft), 18)} ETH";
                    }
                    else if (gamestopNFTOrder.buyTokenId == 1)
                    {
                        salePriceText = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder.pricePerNft), 18)} LRC";
                    }
                    embed.AddField("EthBoy List Price", $"[{salePriceText}](https://nft.gamestop.com/token/0x1d006a27bd82e10f9194d30158d91201e9930420/0x2a669f944bb80efdcdd1c86ad1fc340a4803210dce371d03d00f450a33ec11c6)");

                    var gamestopNFTOrder2 = gamestopNFTOrders2.OrderBy(x => Double.Parse(x.pricePerNft)).ToList()[0];
                    var salePriceText2 = "";
                    if (gamestopNFTOrder2.buyTokenId == 0)
                    {
                        salePriceText2 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder2.pricePerNft), 18)} ETH";
                    }
                    else if (gamestopNFTOrder2.buyTokenId == 1)
                    {
                        salePriceText2 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder2.pricePerNft), 18)} LRC";
                    }
                    embed.AddField("FighterBoy List Price", $"[{salePriceText2}](https://nft.gamestop.com/token/0x1d006a27bd82e10f9194d30158d91201e9930420/0xd79e1ff7615e2826b2e4e29bbfd6cfa1d4109da4cbabf726b4690e1c9d1b411e)");


                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    return;
                }
                catch (Exception ex)
                {
                    var embed = new DiscordEmbedBuilder()
                    {
                        Title = $"Celebratory",
                        Color = new DiscordColor("#FD0000"),
                        Url = "https://nft.gamestop.com/collection/celebratorymetaboy"
                    };

                    var imageUrl = $"https://looprarecdn.azureedge.net/images/metaboycelebratory/ethboy.gif";
                    embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() { Url = imageUrl, Height = 256, Width = 256 };
                    embed.AddField("Oops!", "Something went wrong!");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    return;
                }
            }
            else
            {
                var builder = new DiscordInteractionResponseBuilder()
                .WithContent("This command is not enabled in this channel!")
                .AsEphemeral(true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                return;
            }
        }

        // Metafam
        // Team, Market, ShowAndTell
        [SlashCommand("Metafam", "Show information on MetaFam Collection")]
        public async Task MetaFamCommand(InteractionContext ctx)
        {
            if (ctx.Channel.Id == Settings.TeamChannelId
                || ctx.Channel.Id == Settings.MarketChannelId
                || ctx.Channel.Id == Settings.ShowAndTellChannelId
                || ctx.Channel.Id == Settings.MarketForumChannelId
                )
            {
                try
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

                    // Betty Boop x MetaBoy - Timeless Edition S0.1 List Price
                    var gamestopNFTData = await GamestopService.GetNftData("0x9c3cbd40ff8ea91559311037844f640aa462b64c0c43d4e33cc25478ce91d0a5", "0x1d006a27bd82e10f9194d30158d91201e9930420");
                    var gamestopNFTOrders = await GamestopService.GetNftOrders(gamestopNFTData.nftId);
                    // SloppyPencil X MetaBoy - SloppyBoy S0.2 List Price
                    var gamestopNFTData2 = await GamestopService.GetNftData("0x7245664ea25745d780812e55b078d9058f1b4170f84c006f0a2e48ac5b20e3f2", "0x1d006a27bd82e10f9194d30158d91201e9930420");
                    var gamestopNFTOrders2 = await GamestopService.GetNftOrders(gamestopNFTData2.nftId);

                    // Image thumbnail
                    var imageUrl = $"https://metafamstorage.azureedge.net/metafam-collection/MetaFam.gif";
                    var embed = new DiscordEmbedBuilder()
                    {
                        Title = $"MetaFam",
                        Color = new DiscordColor("#FD0000"),
                        Url = "https://nft.gamestop.com/collection/MetaFam"
                    };
                    embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() { Url = imageUrl, Height = 256, Width = 256 };

                    var gamestopNFTOrder = gamestopNFTOrders.OrderBy(x => Double.Parse(x.pricePerNft)).ToList()[0];
                    var salePriceText = "";
                    if (gamestopNFTOrder.buyTokenId == 0)
                    {
                        salePriceText = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder.pricePerNft), 18)} ETH";
                    }
                    else if (gamestopNFTOrder.buyTokenId == 1)
                    {
                        salePriceText = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder.pricePerNft), 18)} LRC";
                    }
                    embed.AddField("Betty Boop x MetaBoy - Timeless Edition S0.1 List Price", $"[{salePriceText}](https://nft.gamestop.com/token/0x1d006a27bd82e10f9194d30158d91201e9930420/0x9c3cbd40ff8ea91559311037844f640aa462b64c0c43d4e33cc25478ce91d0a5)");

                    var gamestopNFTOrder2 = gamestopNFTOrders2.OrderBy(x => Double.Parse(x.pricePerNft)).ToList()[0];
                    var salePriceText2 = "";
                    if (gamestopNFTOrder2.buyTokenId == 0)
                    {
                        salePriceText2 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder2.pricePerNft), 18)} ETH";
                    }
                    else if (gamestopNFTOrder2.buyTokenId == 1)
                    {
                        salePriceText2 = $"{TokenAmountConverter.ToString(Double.Parse(gamestopNFTOrder2.pricePerNft), 18)} LRC";
                    }
                    embed.AddField("SloppyPencil X MetaBoy - SloppyBoy S0.2 List Price", $"[{salePriceText2}](https://nft.gamestop.com/token/0x1d006a27bd82e10f9194d30158d91201e9930420/0x7245664ea25745d780812e55b078d9058f1b4170f84c006f0a2e48ac5b20e3f2\r\n)");


                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    return;
                }
                catch (Exception ex)
                {
                    var embed = new DiscordEmbedBuilder()
                    {
                        Title = $"MetaFam",
                        Color = new DiscordColor("#FD0000"),
                        Url = "https://nft.gamestop.com/collection/MetaFam"
                    };

                    var imageUrl = $"https://metafamstorage.azureedge.net/metafam-collection/MetaFam.gif";
                    embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() { Url = imageUrl, Height = 256, Width = 256 };
                    embed.AddField("Oops!", "Something went wrong!");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    return;
                }
            }
            else
            {
                var builder = new DiscordInteractionResponseBuilder()
                .WithContent("This command is not enabled in this channel!")
                .AsEphemeral(true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                return;
            }
        }

        // Show Gaia
        // Team, Market, ShowAndTell
        [SlashCommand("Gaia", "Show a Gaia (Gilded, Infernal)")]
        public async Task GaiaCommand(InteractionContext ctx, [Option("name", "The Gaia name to lookup, example: Inferno, Gilded")] string gaiaName)
        {

            if (ctx.Channel.Id == Settings.TeamChannelId
                || ctx.Channel.Id == Settings.MarketChannelId
                || ctx.Channel.Id == Settings.ShowAndTellChannelId
                || ctx.Channel.Id == Settings.MarketForumChannelId
                )
            {
                List<string> GaiaNames = new() {
                    "Inferno",
                    "Gilded"
                };
                
                bool isNotNullOrEmpty = (!String.IsNullOrEmpty(gaiaName));
                bool inNameList = GaiaNames.Contains(gaiaName);

                if(isNotNullOrEmpty)
                {
                    // Ensure correctly named Gaia is used
                    if (inNameList == false)
                    {
                        var builder = new DiscordInteractionResponseBuilder()
                       .WithContent("Not a valid Gaia name! Examples: Gilded, Inferno")
                       .AsEphemeral(true);
                        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                        return;
                    }
                    else
                    {
                        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
                        
                        var embedColor = "#d4af37";
                        var imageUrl = $"https://metafamstorage.azureedge.net/images/gaia/full/{gaiaName}.gif";
                        
                        var embed = new DiscordEmbedBuilder()
                        {
                            Title = $"Gaia {gaiaName} Edition",
                            Url = $"https://nft.gamestop.com/collection/MetaBoyAirdrop",
                            ImageUrl = imageUrl,
                            Color = new DiscordColor(embedColor)
                        };
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                        return;
                        
                    }
                }
                else
                {
                    var builder = new DiscordInteractionResponseBuilder()
                    .WithContent("Not a valid Gaia name! Examples: Gilded, Inferno")
                    .AsEphemeral(true);
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                    return;
                }
            }
            else
            {
                var builder = new DiscordInteractionResponseBuilder()
                .WithContent("This command is not enabled in this channel")
                .AsEphemeral(true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                return;
            }
        }

    }
}
