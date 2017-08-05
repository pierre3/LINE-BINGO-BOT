using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LineBotFunctions.CloudStorage
{

    public class BingoBotTableStorage
    {
        private CloudTableClient _tableClient;
        private CloudTable _bingoEntries;
        private CloudTable _cardUsers;

        public BingoBotTableStorage(string connectionString)
        {
            var strageAccount = CloudStorageAccount.Parse(connectionString);
            _tableClient = strageAccount.CreateCloudTableClient();
        }

        public async Task InitializeAsync()
        {
            _bingoEntries = _tableClient.GetTableReference("BingoEnties");
            _cardUsers = _tableClient.GetTableReference("CardUsers");
            await _cardUsers.CreateIfNotExistsAsync();
            await _bingoEntries.CreateIfNotExistsAsync();
        }

        public static async Task<BingoBotTableStorage> CreateAsync(string connectionString)
        {
            var result = new BingoBotTableStorage(connectionString);
            await result.InitializeAsync();
            return result;
        }

        public async Task AddGameEntryAsync(string userId)
        {
            var newItem = new BingoEntry(BingoEntryType.Game, userId);
            await _bingoEntries.ExecuteAsync(TableOperation.Insert(newItem));
        }

        public async Task<BingoEntry> FindGameEntryAsync(string userId)
        {
            var ope = TableOperation.Retrieve<BingoEntry>(BingoEntryType.Game.ToString(), userId);
            var retrieveResult = await _bingoEntries.ExecuteAsync(ope);
            if (retrieveResult.Result == null) { return null; }
            return (BingoEntry)(retrieveResult.Result);
        }

        public BingoEntry FindGameEntry(int gameId)
        {
            var query = new TableQuery<BingoEntry>()
                .Where(TableQuery.GenerateFilterConditionForInt("GameId", QueryComparisons.Equal, gameId));
            var result = _bingoEntries.ExecuteQuery(query);
            return result.FirstOrDefault(entry => entry.PartitionKey == "Game");
        }

        public async Task UpdateGameEntryAsync(string userId, int gameId, string accessKey)
        {
            var gameEntry = await FindGameEntryAsync(userId);
            if (gameEntry == null) { return; }

            gameEntry.GameId = gameId;
            gameEntry.AccessKey = accessKey;
            var ope = TableOperation.Replace(gameEntry);
            await _bingoEntries.ExecuteAsync(ope);
        }

        public async Task UpdateCardEntryAsync(string userId, int cardId, int gameId)
        {
            var cardEntry = await FindCardEntryAsync(userId);
            if (cardEntry == null) { return; }

            cardEntry.CardId = cardId;
            cardEntry.GameId = gameId;
            var ope = TableOperation.Replace(cardEntry);
            await _bingoEntries.ExecuteAsync(ope);
        }

        public async Task AddCardEntryAsync(string userId)
        {
            var newItem = new BingoEntry(BingoEntryType.Card, userId);
            await _bingoEntries.ExecuteAsync(TableOperation.Insert(newItem));
        }

        public async Task<BingoEntry> FindCardEntryAsync(string userId)
        {
            var ope = TableOperation.Retrieve<BingoEntry>(BingoEntryType.Card.ToString(), userId);
            var retrieveResult = await _bingoEntries.ExecuteAsync(ope);
            if (retrieveResult.Result == null) { return null; }
            return (BingoEntry)(retrieveResult.Result);
        }

        public async Task AddCardUserAsync(int gameId, int cardId, string userId, string userName)
        {
            var cardUser = new CardUser(gameId.ToString(), cardId.ToString(), userId, userName);
            await _cardUsers.ExecuteAsync(TableOperation.Insert(cardUser));
        }

        public async Task UpdateCardUserAsync(CardUser cardUser)
        {
            await _cardUsers.ExecuteAsync(TableOperation.Replace(cardUser));
        }

        public async Task<CardUser> FindCardUserAsync(int gameId, int cardId)
        {
            var ope = TableOperation.Retrieve<CardUser>(gameId.ToString(), cardId.ToString());
            var retrieveResult = await _cardUsers.ExecuteAsync(ope);
            return (CardUser)(retrieveResult.Result);
        }

        public async Task<IList<CardUser>> GetCardUsersAsync(int gameId)
        {
            var query = new TableQuery<CardUser>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, gameId.ToString()));

            // Initialize the continuation token to null to start from the beginning of the table.
            TableContinuationToken continuationToken = null;
            var result = new List<CardUser>();
            do
            {
                // Retrieve a segment (up to 1,000 entities).
                TableQuerySegment<CardUser> tableQueryResult =
                    await _cardUsers.ExecuteQuerySegmentedAsync(query, continuationToken);

                // Assign the new continuation token to tell the service where to
                // continue on the next iteration (or null if it has reached the end).
                continuationToken = tableQueryResult.ContinuationToken;

                result.AddRange(tableQueryResult);

                // Loop until a null continuation token is received, indicating the end of the table.
            } while (continuationToken != null);
            return result;
        }

        public async Task DeleteCardEntryAsync(BingoEntry cardEntry)
        {
            if (cardEntry == null) { throw new ArgumentNullException(nameof(cardEntry)); }
            var ope = TableOperation.Delete(cardEntry);
            await _bingoEntries.ExecuteAsync(ope);
        }

        public async Task DeleteCardUserAsync(BingoEntry cardEntry)
        {
            var cardUser = await FindCardUserAsync(cardEntry.GameId, cardEntry.CardId);
            if (cardUser == null) { return; }
            var ope = TableOperation.Delete(cardUser);
            await _cardUsers.ExecuteAsync(ope);
        }

        public async Task DeleteGameEntryAsync(BingoEntry gameEntry)
        {
            if (gameEntry == null) { throw new ArgumentNullException(nameof(gameEntry)); }

            var ope = TableOperation.Delete(gameEntry);
            await _bingoEntries.ExecuteAsync(ope);
        }
    }

}

