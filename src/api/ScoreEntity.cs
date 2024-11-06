using Azure;
using Azure.Data.Tables;

namespace api;

public class ScoreEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "ScorePartition";
    public string RowKey { get; set; } = "Score";
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public int Score { get; set; }
}