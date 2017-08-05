using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LineBotFunctions.Line.Messaging
{
    public class LineMessagingApi
    {
        private string _channelSecret;
        private string _channelAccessToken;

        public LineMessagingApi(string channelSecret, string channelAccessToken)
        {
            _channelSecret = channelSecret;
            _channelAccessToken = channelAccessToken;
        }

        public bool VerifySignature(string xLineSignature, string requestBody)
        {
            try
            {
                var key = Encoding.UTF8.GetBytes(_channelSecret);
                var body = Encoding.UTF8.GetBytes(requestBody);

                using (HMACSHA256 hmac = new HMACSHA256(key))
                {
                    var hash = hmac.ComputeHash(body, 0, body.Length);
                    var hash64 = Convert.ToBase64String(hash);
                    return xLineSignature == hash64;
                }
            }
            catch
            {
                return false;
            }

        }

        public async Task ReplyMessageAsync(string replyToken, IList<IMessage> messages)
        {
            var content = JsonConvert.SerializeObject(new { replyToken, messages });
            
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _channelAccessToken);
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.line.me/v2/bot/message/reply");
                request.Content = new StringContent(content, Encoding.UTF8, "application/json");
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
            }

        }

        public async Task MultiCastMessageAsync(IList<string> to, IList<IMessage> messages)
        {
            var content = JsonConvert.SerializeObject(new { to, messages });

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _channelAccessToken);
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.line.me/v2/bot/message/multicast");
                request.Content = new StringContent(content, Encoding.UTF8, "application/json");
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
            }
        }

        public IEnumerable<UserTextMessage> GetTextMessages(string content)
        {
            dynamic dynamicObj = JsonConvert.DeserializeObject(content);
            if (dynamicObj == null) { yield break; }

            foreach (var ev in dynamicObj.events)
            {
                if (ev?.type != "message") { continue; }

                if (ev?.message?.type != "text") { continue; }
                string userId = ev?.source?.userId;
                string message = ev?.message?.text;
                string replyToken = ev?.replyToken;
                if (userId == null || message == null || replyToken == null)
                {
                    continue;
                }
                yield return new UserTextMessage()
                {
                    UserId = userId,
                    Message = message,
                    ReplyToken = replyToken
                };
            }
        }

        public async Task<UserProfile> GetUserProfile(string userId)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _channelAccessToken);
                var response = await client.GetStringAsync($"https://api.line.me/v2/bot/profile/{userId}");
              
                return JsonConvert.DeserializeObject<UserProfile>(response);
            }
        }
    }
}
