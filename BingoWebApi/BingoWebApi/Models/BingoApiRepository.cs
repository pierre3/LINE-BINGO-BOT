using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoWebApi.Models
{
    public class BingoApiRepository : IBingoApiRepository
    {
        private readonly BingoApiContext _context;

        public BingoApiRepository(BingoApiContext context)
        {
            _context = context;
        }

        public async Task<Game> CreateGameAsync(string keyword = "")
        {
            var newGame = await _context.Games.AddAsync(new Game() { Keyword = keyword });
            await _context.DrawSource.AddRangeAsync(BingoGenerator.GenerateDrawSource(newGame.Entity.Id));
            await _context.SaveChangesAsync();
            return newGame.Entity;
        }

        public async Task<Game> DrawAsync(int gameId, string accessKey)
        {
            var game = await FindGameAsync(gameId);
            if (game == null) { return null; }

            if (!game.ValidateAccessKey(accessKey))
            {
                throw new InvalidAccessKeyException();
            }

            var result = game.Draw();
            if (result == null)
            {
                return game;
            }
            _context.Games.Update(game);
            OpenCardCells(game, result.Number);
            await _context.SaveChangesAsync();
            return game;
        }

        public async Task<Game> FindGameAsync(int gameId)
        {
            var game = await _context?.Games?.FindAsync(gameId);
            await _context.Entry(game).Collection(e => e.DrawSource).LoadAsync();
            await _context.Entry(game).Collection(e => e.Cards).LoadAsync();
            foreach(var card in game.Cards)
            {
                await _context.Entry(card).Collection(e => e.CardCells).LoadAsync();
            }
            return game;
        }

        public async Task<int> AddCardAsync(Game game, string keyword)
        {
            if (game == null) { throw new ArgumentNullException(nameof(game)); }
            if (!string.IsNullOrEmpty(game.Keyword) && game.Keyword != keyword)
            {
                throw new InvalidKeywordException();
            }

            var card = await _context.Cards.AddAsync(new Card() { GameId = game.Id });
            var opendNumbers = game.DrawSource.Take(game.DrawCount).Select(d => d.Number);
            await _context.CardCells.AddRangeAsync(BingoGenerator.GenerateCardCells(card.Entity.Id, opendNumbers));
            await _context.SaveChangesAsync();

            return card.Entity.Id;
        }

        public async Task<Card> FindCardAsync(int cardId)
        {
            var card = await _context.Cards.FindAsync(cardId);
            await _context.Entry(card).Collection(e => e.CardCells).LoadAsync();
            return card;
        }

        public async Task<IEnumerable<BingoLineType>> GetBingoLines(int cardId)
        {
            var card = await FindCardAsync(cardId);
            if (card == null) { return Enumerable.Empty<BingoLineType>(); }

            return card.CardCells.GetBingoLines();
        }

        public async Task DeleteGameAsync(int gameId, string accessKey)
        {
            var game = await FindGameAsync(gameId);
            if (game == null) { return; }

            if (!game.ValidateAccessKey(accessKey))
            {
                throw new InvalidAccessKeyException();
            }
            
            _context.Games.Remove(game);
            _context.DrawSource.RemoveRange(game.DrawSource);
            _context.Cards.RemoveRange(game.Cards);
            _context.CardCells.RemoveRange(game.Cards.SelectMany(cards => cards.CardCells));

            await _context.SaveChangesAsync();

        }

        private void OpenCardCells(Game game, int number)
        {
            var openCells = game.Cards
                .SelectMany(card => card.CardCells)
                .Where(cell => cell.Number == number)
                .ToArray();
            if (openCells.Length > 0)
            {
                foreach (var cell in openCells)
                {
                    cell.IsOpen = true;
                }
                _context.CardCells.UpdateRange(openCells);
            }
        }
        
    }
}
