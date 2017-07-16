using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LineBotFunctions.BingoApi
{
    public class LizhiLine
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public BingoLineType LineType { get; set; }
        public int WaitingNumber { get; set; }
    }
}
