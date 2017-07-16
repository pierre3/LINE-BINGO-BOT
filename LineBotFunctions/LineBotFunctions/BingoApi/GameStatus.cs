using System.Collections.Generic;

namespace LineBotFunctions.BingoApi
{
    public class GameStatus
    {
        public int GameId { get; set; }
        public int DrawCount { get; set; }
        public IReadOnlyList<int> DrawResults { get; set; }
        public IReadOnlyList<int> Cards { get; set; }

        public IReadOnlyList<int> BingoCards { get; set; }

        public IReadOnlyList<int> LizhiCards { get; set; }
    }
}
