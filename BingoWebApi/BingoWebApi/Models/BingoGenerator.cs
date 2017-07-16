using System;
using System.Collections.Generic;
using System.Linq;

namespace BingoWebApi.Models
{
    public static class BingoGenerator
    {
        private static readonly Random _random = new Random(Guid.NewGuid().GetHashCode());
        private static readonly IEnumerable<int> _drawSource = Enumerable.Range(1, 75);
        private static readonly IEnumerable<int> _cardSource_B = Enumerable.Range(1, 15);
        private static readonly IEnumerable<int> _cardSource_I = Enumerable.Range(16, 15);
        private static readonly IEnumerable<int> _cardSource_N = Enumerable.Range(31, 15);
        private static readonly IEnumerable<int> _cardSource_G = Enumerable.Range(46, 15);
        private static readonly IEnumerable<int> _cardSource_O = Enumerable.Range(61, 15);

        public static IEnumerable<DrawSource> GenerateDrawSource(int gameId)
        {
            return _drawSource.OrderBy(_ => _random.Next())
                .Select((num, i) => new DrawSource()
                {
                    GameId = gameId,
                    Index = i,
                    Number = num
                });
        }

        public static IList<CardCell> GenerateCardCells(int cardId, IEnumerable<int> openedNumbers)
        {
            var B = _cardSource_B.OrderBy(_ => _random.Next()).Take(5).Select(n => new { Num = n, IsOpen = openedNumbers.Any(on => on == n) }).ToArray();
            var I = _cardSource_I.OrderBy(_ => _random.Next()).Take(5).Select(n => new { Num = n, IsOpen = openedNumbers.Any(on => on == n) }).ToArray();
            var N = _cardSource_N.OrderBy(_ => _random.Next()).Take(5).Select(n => new { Num = n, IsOpen = openedNumbers.Any(on => on == n) }).ToArray();
            var G = _cardSource_G.OrderBy(_ => _random.Next()).Take(5).Select(n => new { Num = n, IsOpen = openedNumbers.Any(on => on == n) }).ToArray();
            var O = _cardSource_O.OrderBy(_ => _random.Next()).Take(5).Select(n => new { Num = n, IsOpen = openedNumbers.Any(on => on == n) }).ToArray();

            return new CardCell[] {
                new CardCell(){ CardId = cardId, Index = 0, Number = B[0].Num, IsOpen = B[0].IsOpen },
                new CardCell(){ CardId = cardId, Index = 1, Number = I[0].Num, IsOpen = I[0].IsOpen },
                new CardCell(){ CardId = cardId, Index = 2, Number = N[0].Num, IsOpen = N[0].IsOpen },
                new CardCell(){ CardId = cardId, Index = 3, Number = G[0].Num, IsOpen = G[0].IsOpen },
                new CardCell(){ CardId = cardId, Index = 4, Number = O[0].Num, IsOpen = O[0].IsOpen },

                new CardCell(){ CardId = cardId, Index = 5, Number = B[1].Num, IsOpen = B[1].IsOpen },
                new CardCell(){ CardId = cardId, Index = 6, Number = I[1].Num, IsOpen = I[1].IsOpen },
                new CardCell(){ CardId = cardId, Index = 7, Number = N[1].Num, IsOpen = N[1].IsOpen },
                new CardCell(){ CardId = cardId, Index = 8, Number = G[1].Num, IsOpen = G[1].IsOpen },
                new CardCell(){ CardId = cardId, Index = 9, Number = O[1].Num, IsOpen = O[1].IsOpen },

                new CardCell(){ CardId = cardId, Index = 10, Number = B[2].Num, IsOpen = B[2].IsOpen },
                new CardCell(){ CardId = cardId, Index = 11, Number = I[2].Num, IsOpen = I[2].IsOpen },
                new CardCell(){ CardId = cardId, Index = 12, Number = 0,        IsOpen = true },
                new CardCell(){ CardId = cardId, Index = 13, Number = G[2].Num, IsOpen = G[2].IsOpen },
                new CardCell(){ CardId = cardId, Index = 14, Number = O[2].Num, IsOpen = O[2].IsOpen },

                new CardCell(){ CardId = cardId, Index = 15, Number = B[3].Num, IsOpen = B[3].IsOpen },
                new CardCell(){ CardId = cardId, Index = 16, Number = I[3].Num, IsOpen = I[3].IsOpen },
                new CardCell(){ CardId = cardId, Index = 17, Number = N[3].Num, IsOpen = N[3].IsOpen },
                new CardCell(){ CardId = cardId, Index = 18, Number = G[3].Num, IsOpen = G[3].IsOpen },
                new CardCell(){ CardId = cardId, Index = 19, Number = O[3].Num, IsOpen = O[3].IsOpen },

                new CardCell(){ CardId = cardId, Index = 20, Number = B[4].Num, IsOpen = B[4].IsOpen },
                new CardCell(){ CardId = cardId, Index = 21, Number = I[4].Num, IsOpen = I[4].IsOpen },
                new CardCell(){ CardId = cardId, Index = 22, Number = N[4].Num, IsOpen = N[4].IsOpen },
                new CardCell(){ CardId = cardId, Index = 23, Number = G[4].Num, IsOpen = G[4].IsOpen },
                new CardCell(){ CardId = cardId, Index = 24, Number = O[4].Num, IsOpen = O[4].IsOpen },
            };
        }
    }
}
