using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BingoWebApi.Models
{
    public class GameStatus
    {
        public int GameId { get; }
        public int DrawCount => DrawResults.Count;
        public IReadOnlyList<int> DrawResults { get; }
        public IReadOnlyList<int> Cards { get; }

        public IReadOnlyList<int> BingoCards { get; }

        public IReadOnlyList<int> LizhiCards { get; }

        public GameStatus(int gameId, IList<int> drawResults, IList<Card> cards)
        {
            GameId = gameId;
            DrawResults = new ReadOnlyCollection<int>(drawResults);
           
            Cards = new ReadOnlyCollection<int>(cards.Select(c=>c.Id).ToList());
            var statuses = cards.Select(c => c.ToStatus());
            BingoCards = new ReadOnlyCollection<int>(statuses.Where(st => st.BingoLines.Count > 0).Select(st => st.CardId).ToList());
            LizhiCards = new ReadOnlyCollection<int>(statuses.Where(st => st.LizhiLines.Count > 0).Select(st => st.CardId).ToList());
        }
    }
}
