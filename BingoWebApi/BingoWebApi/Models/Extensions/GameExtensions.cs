using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoWebApi.Models
{
    public static class GameExtensions
    {
        public static DrawSource Draw(this Game game)
        {
            if (game.DrawCount >= game.DrawSource.Count - 1)
            {
                return null;
            }
            game.DrawCount++;
            return game.DrawSource[game.DrawCount];
        }

        public static GameStatus ToStatus(this Game game)
        {
            var drawResult = game.DrawSource
                .OrderBy(a => a.Index)
                .Take(game.DrawCount)
                .Select(a => a.Number)
                .ToList();

            return new GameStatus(game.Id, drawResult, game.Cards);
        }

        public static bool ValidateAccessKey(this Game game, string accessKey)
        {
            if (accessKey == null) { return false; }
            return accessKey.ToUpper() == game.AccessKey.ToString().ToUpper();
        }
    }
}
