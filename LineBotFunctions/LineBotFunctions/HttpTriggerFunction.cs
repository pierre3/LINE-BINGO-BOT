using LineBotFunctions.Line.Messaging;
using LineBotFunctions.TableStrage;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace LineBotFunctions
{
    public static class HttpTriggerFunction
    {

        [FunctionName("LineBingoBot")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
            var lineMessagingApi = new LineMessagingApi(ConfigurationManager.AppSettings["ChannelSecret"], ConfigurationManager.AppSettings["ChannelAccessToken"]);
            var contentJson = await req.Content.ReadAsStringAsync();

            // Performs signature verification.
            var xLineSignature = req.Headers.GetValues("X-Line-Signature").FirstOrDefault();
            if (string.IsNullOrEmpty(xLineSignature) ||
                    !lineMessagingApi.VerifySignature(xLineSignature, contentJson))
            {
                var errorMessage = "Signature validation faild.";
                log.Error(errorMessage);
                return req.CreateResponse(HttpStatusCode.OK, new { Message = errorMessage });
            }

            // Get text messages from web-hook request body.
            var messages = lineMessagingApi.GetTextMessages(contentJson);
            foreach (var msg in messages)
            {
                var user = await lineMessagingApi.GetUserProfile(msg.UserId);

                var talkManager
                    = new TalkManager(await BingoBotTableStrage.CreateAsync(ConfigurationManager.AppSettings["AzureWebJobsStorage"]),
                    lineMessagingApi, log);

                await talkManager.TalkAsync(msg.ReplyToken, user, msg.Message);

            }

            return req.CreateResponse(HttpStatusCode.OK);
        }

    }

}