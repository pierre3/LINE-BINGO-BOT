using System.Collections.Generic;

namespace BingoWebApi.Models
{
    public class Card
    {
        public int Id { get; set; }
        public int GameId { get; set; }

        public IList<CardCell> CardCells { get; set; } = new List<CardCell>();
    }
 }
