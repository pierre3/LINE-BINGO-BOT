using Microsoft.WindowsAzure.Storage.Table;

namespace LineBotFunctions.CloudStorage
{
    public class CardUser : TableEntity
    {
        public string UserId { get; set; }
        public string UserName { get; set; }

        public int BingoLineCount { get; set; } = 0;
        public int LizhiLineCount { get; set; } = 0;

        [IgnoreProperty]
        public int NextBingoLineCount { get; set; } = 0;
        [IgnoreProperty]
        public int NextLizhiLineCount { get; set; } = 0;
        
        public CardUser(string gameId, string cardId, string userId, string userName)
        {
            PartitionKey = gameId;
            RowKey = cardId;
            UserId = userId;
            UserName = userName;
        }

        public CardUser() { }
    }
}
