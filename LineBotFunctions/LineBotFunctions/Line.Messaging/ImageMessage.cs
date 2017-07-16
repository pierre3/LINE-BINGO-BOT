using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LineBotFunctions.Line.Messaging
{
    public class ImageMessage : IMessage
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "type")]
        public MessagType Type { get; } = MessagType.image;

        [JsonProperty(PropertyName = "originalContentUrl")]
        public string OriginalContentUrl { get; set; }

        [JsonProperty(PropertyName = "previewImageUrl")]
        public string PreviewImageUrl { get; set; }

        public ImageMessage(string originalContentUrl, string previerImageUrl)
        {
            OriginalContentUrl = originalContentUrl;
            PreviewImageUrl = previerImageUrl;
        }
    }
}
