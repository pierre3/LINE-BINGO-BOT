using Microsoft.WindowsAzure.Storage.Table;

namespace LineBotFunctions.TableStrage
{
    public class BingoEntry : TableEntity
    {
        public int GameId { get; set; } = -1;
        public string AccessKey { get; set; } = null;
        public int CardId { get; set; } = -1;
        public BingoEntry(BingoEntryType type, string userId)
        {
            PartitionKey = type.ToString();
            RowKey = userId;
        }
        public BingoEntry() { }
    }
}
