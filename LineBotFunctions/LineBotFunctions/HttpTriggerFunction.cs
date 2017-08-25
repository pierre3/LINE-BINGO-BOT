using Line.Messaging;
using Line.Messaging.Webhooks;
using LineBotFunctions.CloudStorage;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace LineBotFunctions
{
    public static class HttpTriggerFunction
    {
        static LineMessagingClient LineMessagingClient { get; }
        static HttpTriggerFunction()
        {
            LineMessagingClient = new LineMessagingClient(ConfigurationManager.AppSettings["ChannelAccessToken"]);
            var sp = ServicePointManager.FindServicePoint(new Uri("https://api.line.me"));
            sp.ConnectionLeaseTimeout = 60 * 1000;
        }

        [FunctionName("LineBingoBot")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                var channelSecret = ConfigurationManager.AppSettings["ChannelSecret"];
                var connectionString = ConfigurationManager.AppSettings["AzureWebJobsStorage"];

                var events = await req.GetWebhookEventsAsync(channelSecret);
                foreach (var ev in events.OfType<MessageEvent>())
                {
                    if (ev.Message.Type != EventMessageType.Text)
                    {
                        continue;
                    }
                    var textMessage = (TextEventMessage)ev.Message;
                    var user = await LineMessagingClient.GetUserProfileAsync(ev.Source.UserId);
                    var talkManager = new TalkManager(
                            await BingoBotTableStorage.CreateAsync(connectionString),
                            await BingoBotBlobStorage.CreateAsync(connectionString),
                            LineMessagingClient, log);

                    await talkManager.TalkAsync(ev.ReplyToken, user, textMessage.Text);
                }
                return req.CreateResponse(HttpStatusCode.OK);
            }
            catch (InvalidSignatureException e)
            {
                return req.CreateResponse(HttpStatusCode.Forbidden, new { Message = e.Message });
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                return req.CreateResponse(HttpStatusCode.OK);
            }
        }

    }

}