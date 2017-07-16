using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace LineBotFunctions.BingoApi
{

    public class BingoApiClient : IDisposable
    {
        private HttpClient _client;
        public BingoApiClient()
        {
            _client = new HttpClient();
        }

        public async Task<NewGameResult> CreateGameAsync(string keyword)
        {
            var response = await _client.PostAsJsonAsync("http://bingowebapi.azurewebsites.net/api/game", new { keyword });
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<NewGameResult>(content);
        }

        public async Task<GameStatus> GetGameStatusAsync(int gameId)
        {
            var jsonString = await _client.GetStringAsync("http://bingowebapi.azurewebsites.net/api/game");
            return JsonConvert.DeserializeObject<GameStatus>(jsonString);
        }

        public async Task<GameStatus> DrawNextNumber(int gameId, string accessKey)
        {
            var response = await _client.PutAsJsonAsync($"http://bingowebapi.azurewebsites.net/api/game/{gameId}/draw", new { accessKey });
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GameStatus>(content);
        }

        public async Task<int> AddCardAsync(int gameId, string keyword)
        {

            var response = await _client.PostAsJsonAsync("http://bingowebapi.azurewebsites.net/api/card", new { gameId, keyword });
            var cardId = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<int>(cardId);
        }

        public async Task<CardStatus> GetCardStatusAsync(int cardId)
        {
            var jsonString = await _client.GetStringAsync($"http://bingowebapi.azurewebsites.net/api/card/{cardId}");
            return JsonConvert.DeserializeObject<CardStatus>(jsonString);
        }

        public async Task DeleteGameAsync(int gameId, string accessKey)
        {
            await _client.DeleteAsync($"http://bingowebapi.azurewebsites.net/api/game/{gameId}?accesskey={accessKey}");
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }


}
