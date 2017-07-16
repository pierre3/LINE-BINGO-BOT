using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace LineBotFunctions.BingoApi
{
    public class CardStatus
    {
        public int CardId { get; set; }
        public IReadOnlyList<CardCellStatus> CardCells { get; set; }

        public int BingoLineCount { get; set; }
        public int LizhiLineCount { get; set; }

        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public IReadOnlyList<BingoLineType> BingoLines { get; set; }

        public IReadOnlyList<LizhiLine> LizhiLines { get; set; }
    }
}
