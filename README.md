# Gaia
This is Gaia, a discord bot for the MetaBoy NFT Discord Server. 

Gaia is intended to be used as part of the [Metaboy NFT Claims System](https://metaboynft.github.io/claims/guide-v1).

# Credit
Gaia was originally created by Fudgey. 
Please check out his awesome [repository](https://github.com/fudgebucket27/FroggieBot) !

# Setup
This is a .NET 6 Console App made with Visual Studio 2022. It uses DSharpPlus to interact with Discord.

You need to create a Discord Bot through the Discord Developer Portal with the following permissions and invite it to your Discord.

![discord](https://user-images.githubusercontent.com/5258063/174226244-5e9b4298-e569-4b6f-be02-e9fb07b961a6.png)

Create an appsettings.json file in the solution directory like below with the "Copy to Output Directory" option set to "Copy always". Keep these settings private.

```json
{
  "Settings": {
    "ApiEndpointUrl": "https://yourSiteName.azurewebsites.net",
    "SqlServerConnectionString": "",
    "ClaimsChannelId": 1044389196980310028,
    "ClaimsAdminChannelId": 1044389220715868160,
    "DiscordServerId": 933963129652674671, //Discord Server Id 
    "DiscordToken": "" //Discord Token
  }
}
```

# Deploy 
You can either run the bot locally at home on your PC,deploy it as a continous web job on Azure, or your preferred cloud hosting provider.

# Slash commands
## /claim
This command checks for claims with the address provided. If valid it will contact the API to process the claim into the message queue.

## /claimable_add
This command adds an NFT that can be claimed by addresses in the allow list. You must use the nftData attribute from the Loopring API. The nftData attribute is not the same as the nftId.

## /claimable_show
This command shows all NFTs that can be claimed.

## /claimable_remove
This command removes NFTs that can be claimed.

## /allowlist_add
This command adds addresses to the allow list that can claim the claimable nfts

## /allowlist_remove
This command removes addresses from the allow list.

## /allowlist_check
This command checks if addresses are in the allow list.

## /claimed_check
This command checks if an address has recieved a claim

## /claimed_remove
This command removes an address that has recieved a claim

# Bulk operations for claims
If you need to do operations in bulk, you can use raw SQL commands to add bulk addresses to the allow list table in the database.

1. Adding a claimable nft(should only need to add once):
```sql
insert into claimable (NftName,NftData) Values ('MetaBoy #9987','0x09c3a263e3cb7e893af1fe73b0cc373850f942842c92560bef6016c2711d5ca0');
```

2. Adding bulk addresses for allow list: 

```sql
insert into allowlist (Address,NftData,Amount) Values ('0x9Da766D34E5df44A1113ca3A38C4DBf400a37ceA','0x09c3a263e3cb7e893af1fe73b0cc373850f942842c92560bef6016c2711d5ca0','1');
insert into allowlist (Address,NftData,Amount) Values ('0x36Cd6b3b9329c04df55d55D41C257a5fdD387ACd','0x09c3a263e3cb7e893af1fe73b0cc373850f942842c92560bef6016c2711d5ca0','1');
```
