using Microsoft.WindowsAzure.Storage.Table;

namespace LineBotFunctions.TableStrage
{
    public class CardUser : TableEntity
    {
        public string UserId { get; set; }
        public string UserName { get; set; }

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
