using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LineBotFunctions.Line.Messaging
{
    public class TextMessage : IMessage
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "type")]
        public MessagType Type { get; } = MessagType.text;

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        public TextMessage(string text)
        {
            Text = text;
        }
    }
}
