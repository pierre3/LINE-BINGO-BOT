using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BingoWebApi.Models
{
    public class LizhiLine
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public BingoLineType LineType { get; }
        public int WaitingNumber { get; }
        public LizhiLine(BingoLineType lineType, int waitingNumber)
        {
            LineType = lineType;
            WaitingNumber = waitingNumber;
        }
    }

}
