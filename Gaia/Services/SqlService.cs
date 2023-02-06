using Dapper;
using DSharpPlus.Entities;
using Gaia.Models;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Diagnostics;
using Gaia.Helpers;

namespace Gaia
{
    public class SqlService
    {
        // Returns all records from Claimable
        public async Task<List<Claimable>> GetClaimable(string connectionString)
        {
            try
            {
                using (SqlConnection db = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    await db.OpenAsync();
                    var result = await db
                        .QueryAsync<Claimable>
                        ($"SELECT * FROM Claimable");
                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                return new List<Claimable>();
            }
        }

        // Adds a new Claimable Nft with NftName and NftData
        public async Task<int> AddClaimable(string nftName, string nftData, string connectionString)
        {
            int result = 0;
            try
            {
                using (SqlConnection db = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    await db.OpenAsync();
                    var doesClaimableExistParameters = new { NftData = nftData };
                    var doesClaimableExist = await db.QueryAsync<Claimable>($"SELECT * FROM Claimable WHERE NftData = @NftData", doesClaimableExistParameters);
                    if (doesClaimableExist.ToList().Count == 0)
                    {
                        var insertParameters = new
                        {
                            NftName = nftName,
                            NftData = nftData
                        };
                        result = await db.ExecuteAsync("INSERT INTO Claimable (NftName,NftData) VALUES (@NftName, @NftData)", insertParameters); // 1 when inserted
                    }
                    else
                    {
                        result = -1; // -1 We only ever need to have 1 Claimable record
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return result; //0 if something goes wrong
        }

        // Deletes a record from Claimable matching NftData
        public async Task<int> RemoveClaimable(string nftData, string connectionString)
        {
            int result = 0;
            try
            {
                using (SqlConnection db = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    await db.OpenAsync();
                    var doesClaimableExistParameters = new { NftData = nftData };
                    var doesClaimableExist = await db.QueryAsync<Claimable>($"SELECT * FROM Claimable WHERE NftData = @NftData", doesClaimableExistParameters);
                    if (doesClaimableExist.ToList().Count > 0)
                    {
                        var deleteParameters = new
                        {
                            NftData = nftData
                        };
                        result = await db.ExecuteAsync("DELETE FROM Claimable WHERE NftData = @NftData", deleteParameters); // > 0 when removed
                    }
                    else
                    {
                        result = -1; // -1 when already removed
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return result; //0 if something goes wrong
        }

        // Will create a new AllowList record matching Address and NftData for Amount
        // If an existing record is found, it will increment existing Amount with requested Amount
        public async Task<int> AddToAllowlist(string address, string nftData, string amount, string connectionString)
        {
            int result = 0;
            try
            {
                using (SqlConnection db = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    await db.OpenAsync();
                    var doesAllowListExistParameters = new { Address = address, NftData = nftData };
                    var doesAllowListExist = await db.QueryAsync<AllowList>($"SELECT * FROM AllowList WHERE NftData = @NftData and Address = @Address", doesAllowListExistParameters);
                    
                    if (doesAllowListExist.ToList().Count == 0) // New record must be created
                    {
                        var addToAllowListParameters = new
                        {
                            Address = address,
                            NftData = nftData,
                            Amount = amount
                        };
                        result = await db.ExecuteAsync("INSERT INTO AllowList (Address,NftData,Amount) Values (@Address,@NftData,@Amount)", addToAllowListParameters); // > 0 when added
                    }
                    else if (doesAllowListExist.ToList().Count == 1) // We need to update the Amount here on the existing record
                    {
                        string updatedAmount = (int.Parse(doesAllowListExist.First().Amount) + int.Parse(amount)).ToString();
                        var updateAllowListParameters = new
                        {
                            Address = address,
                            NftData = nftData,
                            Amount = updatedAmount
                        };
                        result = await db.ExecuteAsync("UPDATE AllowList SET Amount = @Amount WHERE nftdata = @NftData and address = @Address", updateAllowListParameters);
                    }
                    else
                    { 
                        
                        result = -1; // There should only 1 AllowList record retrieved. Something is wrong here!
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return result; //0 if something goes wrong
        }

        // Deletes record from AllowList matching Address and NftData
        public async Task<int> RemoveFromAllowlist(string address, string nftData, string connectionString)
        {
            int result = 0;
            try
            {
                using (SqlConnection db = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    await db.OpenAsync();
                    var doesAllowListExistParameters = new { Address = address, NftData = nftData };
                    var doesAllowListExist = await db.QueryAsync<AllowList>($"SELECT * FROM AllowList WHERE NftData = @NftData AND Address = @Address", doesAllowListExistParameters);
                    if (doesAllowListExist.ToList().Count > 0)
                    {
                        var removeFromAllowListParameters = new
                        {
                            Address = address,
                            NftData = nftData
                        };
                        result = await db.ExecuteAsync("DELETE FROM AllowList WHERE Address = @Address and NftData = @NftData", removeFromAllowListParameters); 
                    }
                    else
                    {
                        result = -1; // -1 when already removed
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return result; //0 if something goes wrong
        }

        // Check AllowList to see if Address & NftData are listed
        public async Task<AllowList?> CheckAllowlist(string address, string nftData, string connectionString)
        {
            AllowList? result = new AllowList();
            try
            {
                using (SqlConnection db = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    await db.OpenAsync();
                    var doesAllowListExistParameters = new { Address = address, NftData = nftData };
                    var doesAllowListExist = await db.QueryAsync<AllowList>($"SELECT * FROM AllowList WHERE NftData = @NftData and Address = @Address", doesAllowListExistParameters);
                    if (doesAllowListExist.ToList().Count > 0)
                    {
                        result = doesAllowListExist.First();
                    }
                    else
                    {
                        result = null; // -1 No records found matching Address & NftData
                    }
                }
            }
            catch (Exception ex)
            {
                result.Address = "Error";
            }
            return result; //0 if something goes wrong
        }

        // Deletes a record from Claimed matching Address and NftData
        public async Task<int> RemoveFromClaimed(string address, string nftData, string connectionString)
        {
            int result = 0;
            try
            {
                using (SqlConnection db = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    await db.OpenAsync();
                    var doesClaimExistParameters = new { Address = address, NftData = nftData };
                    var doesClaimExist = await db.QueryAsync<Claimed>($"SELECT * FROM Claimed WHERE NftData = @NftData and Address = @Address", doesClaimExistParameters);
                    if (doesClaimExist.ToList().Count > 0)
                    {
                        var removeFromAllowListParameters = new
                        {
                            Address = address,
                            NftData = nftData
                        };
                        result = await db.ExecuteAsync("DELETE FROM Claimed WHERE Address = @Address and NftData = @NftData", removeFromAllowListParameters); // > 0 when added
                    }
                    else
                    {
                        result = -1; // -1 when already removed
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return result; //0 if something goes wrong
        }

        // Checks to see if Address has previously claimed NftData
        public async Task<Claimed?> CheckClaimed(string address, string nftData, string connectionString)
        {
            Claimed? result = new Claimed();
            try
            {
                using (SqlConnection db = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    await db.OpenAsync();
                    var doesClaimExistParameters = new { Address = address, NftData = nftData };
                    var doesClaimExist = await db.QueryAsync<Claimed>($"SELECT * FROM Claimed WHERE NftData = @NftData and Address = @Address", doesClaimExistParameters);
                    if (doesClaimExist.ToList().Count > 0)
                    {
                        result = doesClaimExist.First();
                    }
                    else
                    {
                        result = null; // -1 when already exists
                    }
                }
            }
            catch (Exception ex)
            {
                result.Address = "Error";
            }
            return result; //0 if something goes wrong
        }
    }
}