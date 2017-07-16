using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace BingoWebApi.Models
{
    public class CardStatus
    {
        public int CardId { get; }
        public IReadOnlyList<CardCellStatus> CardCells { get; }

        public int BingoLineCount => BingoLines.Count;
        public int LizhiLineCount => LizhiLines.Count;

        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public IReadOnlyList<BingoLineType> BingoLines { get; }
        
        public IReadOnlyList<LizhiLine> LizhiLines { get; }
        
        public CardStatus(int cardId, IList<CardCellStatus> cardCells, IList<BingoLineType> bingoLines, IList<LizhiLine> lizhiLines)
        {
            CardId = cardId;
            CardCells = new ReadOnlyCollection<CardCellStatus>(cardCells);
            BingoLines = new ReadOnlyCollection<BingoLineType>(bingoLines);
            LizhiLines = new ReadOnlyCollection<LizhiLine>(lizhiLines);
        }
    }

}
