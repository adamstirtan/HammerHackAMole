using Azure;
using Azure.Data.Tables;

using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;

using SignalRSwaggerGen.Attributes;

namespace api;

[SignalRHub]
[EnableCors(policyName: "AllowFrontend")]
public class GameHub : Hub
{
    private static TableClient? _tableClient;

    public GameHub(IConfiguration configuration)
    {
        string connectionString = configuration["AzureTableStorage:ConnectionString"] ??
            throw new Exception("Connection string is missing.");

        string tableName = configuration["AzureTableStorage:TableName"] ??
            throw new Exception("Table name is missing.");
        
        TableServiceClient serviceClient = new(connectionString);

        _tableClient = serviceClient.GetTableClient(tableName);
    }

    public override async Task OnConnectedAsync()
    {
        ScoreEntity scoreEntity = await GetScoreAsync();
        
        await Clients.Caller.SendAsync("UpdateScore", scoreEntity.Score);
    }

    public async Task WhackMole()
    {
        if (_tableClient == null)
        {
            throw new Exception("Table client is not initialized.");
        }
        
        ScoreEntity scoreEntity = await GetScoreAsync();
        scoreEntity.Score++;

        await _tableClient.UpsertEntityAsync(scoreEntity);
        
        await Clients.All.SendAsync("UpdateScore", scoreEntity.Score);
    }

    private static async Task<ScoreEntity> GetScoreAsync()
    {
        if (_tableClient == null)
        {
            throw new Exception("Table client is not initialized.");
        }
        
        try
        {
            var response = await _tableClient.GetEntityAsync<ScoreEntity>("ScorePartition", "Score");
            
            return response.Value;
        }
        catch (RequestFailedException exception) when (exception.Status == 404)
        {
            ScoreEntity scoreEntity = new()
            {
                Score = 0
            };
            
            await _tableClient.AddEntityAsync(scoreEntity);
            
            return scoreEntity;
        }
    }
}