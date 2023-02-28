using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Gaia.Models;
using Gaia.Services;
using Gaia.Helpers;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;
using Nethereum.Util;
using Newtonsoft.Json;
using PoseidonSharp;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Gaia
{
    public class ClaimsSlashCommands : ApplicationCommandModule
    {
        public LoopringService LoopringService { private get; set; }
        public Random Random { private get; set; }

        public SqlService SqlService { private get; set; }

        public Settings Settings { private get; set; }

        public EthereumService EthereumService { private get; set; }

        public EtherscanService EtherscanService { private get; set; }

        public GamestopService GamestopService { private get; set; }

        public ClaimsApiService ClaimsApiService { private get; set; }

        [SlashCommand("claim", "Claim NFTs(if eligible)")]
        public async Task ClaimCommand(InteractionContext ctx, [Option("address", "The address in Hex Format for the claim, Example: 0x36Cd6b3b9329c04df55d55D41C257a5fdD387ACd")] string address)
        {
            string ethAddressRegexPattern = @"0x[a-fA-F0-9]{40}";
            address = address.Trim();

            var hexAddress = "";
            foreach (Match m in Regex.Matches(address, ethAddressRegexPattern))
            {
                hexAddress = m.Value.ToLower();
                break;
            }

            if (ctx.Channel.Id == Settings.ClaimsChannelId && !string.IsNullOrEmpty(hexAddress))
            {

                List<NftReciever> nftRecievers = new List<NftReciever>();
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
                // Attempting Claim
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Looking for a Soul..."));
                

                // Receives back CanClaimV3 as a list of Address, NftData, with amount greater than 0
                var redeemable = await ClaimsApiService.GetRedeemable(address);
                if (redeemable != null && redeemable.Count > 0)
                {
                    foreach (var redeem in redeemable)
                    {
                        // nftReceiver { string Address, string NftData }
                        nftRecievers.Add(new NftReciever() { Address = address, NftData = redeem.NftData });
                    }
                }

                // Valid claims found - send to Api
                if (nftRecievers.Count > 0 && redeemable != null)
                {
                    // Claim Found
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"There was a Soul associated with your wallet in the catacombs!"));
                    
                    // Gets passed instances of nftReceiver [{ string Address, string NftData }]
                    await ClaimsApiService.AddClaim(nftRecievers);
                    // Claim sent
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Hades might become furious but you are right, this Soul's haunting days are not over! I will honor your request this time. It should be there soon! Please check your hidden tab as your new Soul may be shy. If there are any problems, you may contact someone from the Soulies Team."));
                    
                    foreach (NftReciever nftReciever in nftRecievers)
                    {
                        ConsoleMessage.WriteMessage($"[INFO]  Submitted claim for Address: {nftReciever.Address} NftData: {nftReciever.NftData} ");
                    }
                    
                    return;

                }
                else
                {
                    // No claim found
                    ConsoleMessage.WriteMessage($"[INFO]  Unable to find any matching claims for Address: {address}  !");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"I apologize... I can't seem to find it at this moment in time. The underworld has far too many dark corners. Please check our recent announcements for the most up to date info!"));
                    return;
                }
            }
            else if (ctx.Channel.Id == Settings.ClaimsChannelId && string.IsNullOrEmpty(hexAddress))
            {
                // Invalid Address Format
                var builder = new DiscordInteractionResponseBuilder()
                .WithContent("Woah woah woah. You are clearly not speaking my language... Please try again in Hex format: Example: 0x6CB2111326Fa160B7A50F0Bf4158964CA9CF5DB4")
                .AsEphemeral(true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                return;
            }
            else
            {
                var builder = new DiscordInteractionResponseBuilder()
                .WithContent($"UNKNOWN COMMAND. For all claims please visit <#{Settings.ClaimsChannelId}>")
                .AsEphemeral(true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                return;
            }
        }

        // Shows all claimable NFTs with NftName and NftData
        [SlashCommand("claimable_show", "Show claimable NFTs")]
        public async Task ShowClaimableCommand(InteractionContext ctx)
        {

            if (ctx.Channel.Id == Settings.ClaimsAdminChannelId)
            {

                List<Claimable> claimableList = new List<Claimable>();
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
                var claimables = await SqlService.GetClaimable(Settings.SqlServerConnectionString);
                foreach (var claimable in claimables)
                {
                    claimableList.Add(claimable);
                }

                if (claimableList.Count > 0)
                {
                    string text = "\n";
                    var chunks = claimableList.Chunk(10);
                    foreach (var chunk in chunks)
                    {
                        foreach (var claimable in chunk)
                        {
                            text += $"NFT Name: {claimable.nftName}\nNFT Data: {claimable.nftData}\n\n";

                        }
                        try
                        {
                            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent(text));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        text = "";

                    }
                    return;
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"I'm sorry I can't see to find any claimable NFTs...You can add claimable nfts with the slash command: claimable_add"));
                    return;
                }
            }
            else
            {
                var builder = new DiscordInteractionResponseBuilder()
                .WithContent($"UNKNOWN COMMAND. For all claims please visit <#{Settings.ClaimsChannelId}>")
                .AsEphemeral(true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                return;
            }
        }

        // Should never have to increment the record, as there is no amount
        [SlashCommand("claimable_add", "Add a claimable NFT")]
        public async Task AddClaimableCommand(InteractionContext ctx, [Option("nftName", "The  NFT name")] string nftName, [Option("nftData", "The nftData of the claimable nft")] string nftData)
        {
            var isValid = false;
            nftName = nftName.Trim();
            nftData = nftData.Trim();

            if (nftName.Length > 0 && nftData.Length == 66 && nftData.StartsWith("0x"))
            {
                isValid = true;
            }

            if (ctx.Channel.Id == Settings.ClaimsAdminChannelId && isValid)
            {

                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Hold on adding the claimable NFT..."));

                var queryResult = await SqlService.AddClaimable(nftName, nftData, Settings.SqlServerConnectionString);

                if (queryResult == 1)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"I've added the claimable NFT!"));
                    return;
                }
                else if (queryResult == -1)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Sorry, the claimable NFT already exists!"));
                    return;
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Sorry, something went wrong! Please try again later..."));
                    return;
                }
            }
            else if (ctx.Channel.Id == Settings.ClaimsAdminChannelId && !isValid)
            {
                var builder = new DiscordInteractionResponseBuilder()
                .WithContent("Woah woah woah it's like you're speaking another language! My machines can't read that, please type the nftData in Hex Format : Example: 0x14e15ad24d034f0883e38bcf95a723244a9a22e17d47eb34aa2b91220be0adc4")
            .AsEphemeral(true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                return;
            }
            else
            {
                var builder = new DiscordInteractionResponseBuilder()
                .WithContent($"UNKNOWN COMMAND. For all claims please visit <#{Settings.ClaimsChannelId}>")
                .AsEphemeral(true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                return;
            }
        }

        // Deletes a record from Claimable
        [SlashCommand("claimable_remove", "Remove a claimable NFT")]
        public async Task RemoveClaimableCommand(InteractionContext ctx, [Option("nftData", "The nftData of the claimable nft")] string nftData)
        {
            var isValid = false;
            nftData = nftData.Trim();

            if (nftData.Length == 66 && nftData.StartsWith("0x"))
            {
                isValid = true;
            }

            if (ctx.Channel.Id == Settings.ClaimsAdminChannelId && isValid)
            {

                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Hold on removing the claimable NFT..."));

                var queryResult = await SqlService.RemoveClaimable(nftData, Settings.SqlServerConnectionString);

                if (queryResult > 0)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"I've removed the claimable NFT"));
                    return;
                }
                else if (queryResult == -1)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Sorry, the claimable NFT has already been removed!"));
                    return;
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Sorry, something went wrong! Please try again later..."));
                    return;
                }
            }
            else if (ctx.Channel.Id == Settings.ClaimsAdminChannelId && !isValid)
            {
                var builder = new DiscordInteractionResponseBuilder()
                .WithContent("Woah woah woah it's like you're speaking another language! My machines can't read that, please type the nftData in Hex Format : Example: 0x14e15ad24d034f0883e38bcf95a723244a9a22e17d47eb34aa2b91220be0adc4")
            .AsEphemeral(true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                return;
            }
            else
            {
                var builder = new DiscordInteractionResponseBuilder()
                .WithContent($"UNKNOWN COMMAND. For all claims please visit <#{Settings.ClaimsChannelId}>")
                .AsEphemeral(true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                return;
            }
        }
        
        // Will create a new AllowList record if one is not found
        // If an AllowList record is found, it will add the requested Amount to the existing record
        [SlashCommand("allowlist_add", "Add an address to the allow list")]
        public async Task AddToAllowlistCommand(InteractionContext ctx, [Option("address", "The address for the allowlist")] string address, [Option("nftData", "The nftData of the claimable nft")] string nftData, [Option("amount", "The amount for the claim")] long amount)
        {
            var isValid = false;
            nftData = nftData.Trim();

            string ethAddressRegexPattern = @"0x[a-fA-F0-9]{40}";
            address = address.Trim();

            var hexAddress = "";
            foreach (Match m in Regex.Matches(address, ethAddressRegexPattern))
            {
                hexAddress = m.Value.ToLower();
                break;
            }

            if (nftData.Length == 66 && nftData.StartsWith("0x") && !string.IsNullOrEmpty(hexAddress))
            {
                isValid = true;
            }

            if (ctx.Channel.Id == Settings.ClaimsAdminChannelId && isValid)
            {

                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Hold on adding this to the allow list..."));

                var queryResult = await SqlService.AddToAllowlist(hexAddress, nftData, amount.ToString(), Settings.SqlServerConnectionString);

                if (queryResult > 0)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"I've added this to the allow list"));
                    return;
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Sorry, something went wrong! Please try again later..."));
                    return;
                }
            }
            else if (ctx.Channel.Id == Settings.ClaimsAdminChannelId && !isValid)
            {
                string text = "Woah woah woah it's like you're speaking another language!\n";
                text += "My machines can't read that, please type the nftData in Hex Format : Example: 0x14e15ad24d034f0883e38bcf95a723244a9a22e17d47eb34aa2b91220be0adc4\n";
                text += "Addresses should be in Hex Format too : Example: 0x36cd6b3b9329c04df55d55d41c257a5fdd387acd\n";
                var builder = new DiscordInteractionResponseBuilder()
                .WithContent($"{text}")
            .AsEphemeral(true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                return;
            }
            else
            {
                var builder = new DiscordInteractionResponseBuilder()
                .WithContent($"UNKNOWN COMMAND. For all claims please visit <#{Settings.ClaimsChannelId}>")
                .AsEphemeral(true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                return;
            }
        }

        // Deletes an Allowlist record
        [SlashCommand("allowlist_remove", "Remove an address from the allow list")]
        public async Task RemoveFromAllowlistCommand(InteractionContext ctx, [Option("address", "The address for the allow list")] string address, [Option("nftData", "The nftData of the claimable nft")] string nftData)
        {
            var isValid = false;
            nftData = nftData.Trim();

            string ethAddressRegexPattern = @"0x[a-fA-F0-9]{40}";
            address = address.Trim();

            var hexAddress = "";
            foreach (Match m in Regex.Matches(address, ethAddressRegexPattern))
            {
                hexAddress = m.Value.ToLower();
                break;
            }

            if (nftData.Length == 66 && nftData.StartsWith("0x") && !string.IsNullOrEmpty(hexAddress))
            {
                isValid = true;
            }

            if (ctx.Channel.Id == Settings.ClaimsAdminChannelId && isValid)
            {

                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Hold on removing this from the allow list..."));

                var queryResult = await SqlService.RemoveFromAllowlist(hexAddress, nftData, Settings.SqlServerConnectionString);

                if (queryResult > 0)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"I've removed this from the allow list"));
                    return;
                }
                else if (queryResult == -1)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Sorry, this has already been removed from the allow list!"));
                    return;
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Sorry, something went wrong! Please try again later..."));
                    return;
                }
            }
            else if (ctx.Channel.Id == Settings.ClaimsAdminChannelId && !isValid)
            {
                string text = "Woah woah woah it's like you're speaking another language!\n";
                text += "My machines can't read that, please type the nftData in Hex Format : Example: 0x14e15ad24d034f0883e38bcf95a723244a9a22e17d47eb34aa2b91220be0adc4\n";
                text += "Addresses should be in Hex Format too : Example: 0x36cd6b3b9329c04df55d55d41c257a5fdd387acd\n";
                var builder = new DiscordInteractionResponseBuilder()
                .WithContent($"{text}")
            .AsEphemeral(true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                return;
            }
            else
            {
                var builder = new DiscordInteractionResponseBuilder()
                .WithContent($"UNKNOWN COMMAND. For all claims please visit <#{Settings.ClaimsChannelId}>")
                .AsEphemeral(true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                return;
            }
        }

        // Checks to see if a record matching Address and NftData exists in AllowList
        [SlashCommand("allowlist_check", "Check if an address is in the allow list")]
        public async Task CheckAllowlistCommand(InteractionContext ctx, [Option("address", "The address for the allow list")] string address, [Option("nftData", "The nftData of the claimable nft")] string nftData)
        {
            var isValid = false;
            nftData = nftData.Trim();

            string ethAddressRegexPattern = @"0x[a-fA-F0-9]{40}";
            address = address.Trim();

            var hexAddress = "";
            foreach (Match m in Regex.Matches(address, ethAddressRegexPattern))
            {
                hexAddress = m.Value.ToLower();
                break;
            }

            if (nftData.Length == 66 && nftData.StartsWith("0x") && !string.IsNullOrEmpty(hexAddress))
            {
                isValid = true;
            }

            if (ctx.Channel.Id == Settings.ClaimsAdminChannelId && isValid)
            {

                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Hold on checking the allow list..."));

                var queryResult = await SqlService.CheckAllowlist(hexAddress, nftData, Settings.SqlServerConnectionString);

                if (queryResult != null && queryResult.Address != "Error")
                {
                    string text = "I've found the address in the allow list with the following details:\n";
                    text += $"Address: {address}\nNftData: {nftData}\nAmount: {queryResult.Amount}\n";
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"{text}"));
                    return;
                }
                else if (queryResult != null && queryResult.Address == "Error")
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Sorry, something went wrong! Please try again later..."));
                    return;
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"I could not find that address and nftData in the allow list!"));
                    return;
                }
            }
            else if (ctx.Channel.Id == Settings.ClaimsAdminChannelId && !isValid)
            {
                string text = "Woah woah woah it's like you're speaking another language!\n";
                text += "My machines can't read that, please type the nftData in Hex Format : Example: 0x14e15ad24d034f0883e38bcf95a723244a9a22e17d47eb34aa2b91220be0adc4\n";
                text += "Addresses should be in Hex Format too : Example: 0x36cd6b3b9329c04df55d55d41c257a5fdd387acd\n";
                var builder = new DiscordInteractionResponseBuilder()
                .WithContent($"{text}")
            .AsEphemeral(true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                return;
            }
            else
            {
                var builder = new DiscordInteractionResponseBuilder()
                .WithContent($"UNKNOWN COMMAND. For all claims please visit <#{Settings.ClaimsChannelId}>")
                .AsEphemeral(true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                return;
            }
        }

        // Deletes a record from Claimed matching Address and NftData
        [SlashCommand("claimed_remove", "Remove an address from the claimed list")]
        public async Task RemoveFromClaimedCommand(InteractionContext ctx, [Option("address", "The address for the claimed list")] string address, [Option("nftData", "The nftData of the claimable nft")] string nftData)
        {
            var isValid = false;
            nftData = nftData.Trim();

            string ethAddressRegexPattern = @"0x[a-fA-F0-9]{40}";
            address = address.Trim();

            var hexAddress = "";
            foreach (Match m in Regex.Matches(address, ethAddressRegexPattern))
            {
                hexAddress = m.Value.ToLower();
                break;
            }

            if (nftData.Length == 66 && nftData.StartsWith("0x") && !string.IsNullOrEmpty(hexAddress))
            {
                isValid = true;
            }

            if (ctx.Channel.Id == Settings.ClaimsAdminChannelId && isValid)
            {

                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Hold on removing this from the claimed list..."));

                var queryResult = await SqlService.RemoveFromClaimed(hexAddress, nftData, Settings.SqlServerConnectionString);

                if (queryResult > 0)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"I've removed this from the claimed list"));
                    return;
                }
                else if (queryResult == -1)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Sorry, this has already been removed from the claimed list!"));
                    return;
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Sorry, something went wrong! Please try again later..."));
                    return;
                }
            }
            else if (ctx.Channel.Id == Settings.ClaimsAdminChannelId && !isValid)
            {
                string text = "Woah woah woah it's like you're speaking another language!\n";
                text += "My machines can't read that, please type the nftData in Hex Format : Example: 0x14e15ad24d034f0883e38bcf95a723244a9a22e17d47eb34aa2b91220be0adc4\n";
                text += "Addresses should be in Hex Format too : Example: 0x36cd6b3b9329c04df55d55d41c257a5fdd387acd\n";
                var builder = new DiscordInteractionResponseBuilder()
                .WithContent($"{text}")
            .AsEphemeral(true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                return;
            }
            else
            {
                var builder = new DiscordInteractionResponseBuilder()
                .WithContent($"UNKNOWN COMMAND. For all claims please visit <#{Settings.ClaimsChannelId}>")
                .AsEphemeral(true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                return;
            }
        }

        // Checks to see if a record matching Address and NftData exists in Claimed
        [SlashCommand("claimed_check", "Check if an address has claimed")]
        public async Task CheckClaimedCommand(InteractionContext ctx, [Option("address", "The address for the claim")] string address, [Option("nftData", "The nftData of the claimable nft")] string nftData)
        {
            var isValid = false;
            nftData = nftData.Trim();

            string ethAddressRegexPattern = @"0x[a-fA-F0-9]{40}";
            address = address.Trim();

            var hexAddress = "";
            foreach (Match m in Regex.Matches(address, ethAddressRegexPattern))
            {
                hexAddress = m.Value.ToLower();
                break;
            }

            if (nftData.Length == 66 && nftData.StartsWith("0x") && !string.IsNullOrEmpty(hexAddress))
            {
                isValid = true;
            }

            if (ctx.Channel.Id == Settings.ClaimsAdminChannelId && isValid)
            {

                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Hold on checking the claimed list..."));

                var queryResult = await SqlService.CheckClaimed(hexAddress, nftData, Settings.SqlServerConnectionString);

                if (queryResult != null && queryResult.Address != "Error")
                {
                    string text = "I've found the address in the claimed list with the following details:\n";
                    text += $"Address: {address}\nNftData: {nftData}\nAmount: {queryResult.Amount}\nClaimed Date: {queryResult.ClaimedDate}";
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"{text}"));
                    return;
                }
                else if (queryResult != null && queryResult.Address == "Error")
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Sorry, something went wrong! Please try again later..."));
                    return;
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"I could not find that address and nftData in the claimed list!"));
                    return;
                }
            }
            else if (ctx.Channel.Id == Settings.ClaimsAdminChannelId && !isValid)
            {
                string text = "Woah woah woah it's like you're speaking another language!\n";
                text += "My machines can't read that, please type the nftData in Hex Format : Example: 0x14e15ad24d034f0883e38bcf95a723244a9a22e17d47eb34aa2b91220be0adc4\n";
                text += "Addresses should be in Hex Format too : Example: 0x36cd6b3b9329c04df55d55d41c257a5fdd387acd\n";
                var builder = new DiscordInteractionResponseBuilder()
                .WithContent($"{text}")
            .AsEphemeral(true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                return;
            }
            else
            {
                var builder = new DiscordInteractionResponseBuilder()
                .WithContent($"UNKNOWN COMMAND. For all claims please visit <#{Settings.ClaimsChannelId}>")
                .AsEphemeral(true);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                return;
            }
        }
    }
}

