using System.Collections.Generic;
using System.Threading.Tasks;

namespace BingoWebApi.Models
{
    public interface IBingoApiRepository
    {
        Task<Game> CreateGameAsync(string keyword = "");
        Task<Game> DrawAsync(int gameId, string accessKey);
        Task<Game> FindGameAsync(int gameId);
        Task<int> AddCardAsync(Game game, string keyword);
        Task<Card> FindCardAsync(int cardId);
        Task DeleteGameAsync(int gameId, string accessKey);
    }
}