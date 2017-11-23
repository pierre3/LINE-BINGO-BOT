using Line.Messaging;
using Line.Messaging.Webhooks;
using LineBotFunctions.BingoApi;
using LineBotFunctions.CloudStorage;
using LineBotFunctions.Drawing;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace LineBotFunctions
{
    public class TalkManager : WebhookApplication
    {
        private static readonly string ReplyMessage_Start
            = "ゲームに参加するために必要な\"合言葉\"を設定できます。好きな言葉(20文字まで)を入力して送ってね。" + Environment.NewLine +
                            "設定しない場合は「なし」と入力してください。";

        private static readonly string ReplyMessage_Join
            = "参加するゲームの\"ID番号\"と\"合言葉\"を入力してください。" +
                "「123 あいことば」のように間に空白を入れて送ってね。" + Environment.NewLine +
                "合言葉が決められていない場合は\"ID番号\"だけでいいよ。";

        private static readonly string ReplyMessage_Usage
            = "新しいゲームを始めるには「ゲームを開始する」をタップしてね。" + Environment.NewLine +
                "ゲームに参加するには「ゲームに参加する」をタップだよ！";

        private static readonly string ReplyMessage_GameEntry
            = "ゲームを開始しました(ゲームＩＤ:{0})。" + Environment.NewLine + Help_GameEntry;
                
        private static readonly string ReplyMessage_CardCreated
            = "カードを作成したよ。" + Environment.NewLine + Help_CardCreated;

        private static readonly string Help_GameEntry
            = "ゲームを進めるには、「番号を引く」をタップしてね。 " + Environment.NewLine +
                "ゲームを終了するには「ゲームを終了する」をタップしてね。";

        private static readonly string Help_CardCreated
            = "最新のカードを取得するには 、「カードを更新」をタップしてね。";

        private BingoBotTableStorage _tableStorage;
        private BingoBotBlobStorage _blobStorage;
        private LineMessagingClient _messagingClient;
        private TraceWriter _log;

        private BingoApiClient _bingoClient = BingoApiClient.Default;

        public TalkManager(BingoBotTableStorage tableStorage, BingoBotBlobStorage blobStrorage, LineMessagingClient messagingClient, TraceWriter log)
        {
            _tableStorage = tableStorage;
            _blobStorage = blobStrorage;
            _messagingClient = messagingClient;
            _log = log;
        }

        protected override async Task OnMessageAsync(MessageEvent ev)
        {
            var textMessage = (ev.Message as TextEventMessage);
            if (textMessage == null)
            {
                return;
            }

            if (textMessage.Text == "RegisterMenu")
            {
                var debugUser = ConfigurationManager.AppSettings["DebugUser"];
                if (debugUser != null && debugUser == ev.Source.UserId)
                {
                    var menuManager = new BingoMenuManager(_messagingClient);
                    await menuManager.RegisterBingoMenuAsync();
                    await _messagingClient.ReplyMessageAsync(ev.ReplyToken, "Registerd Bingo Menus.");
                    return;
                }
            }
            var user = await _messagingClient.GetUserProfileAsync(ev.Source.UserId);
            await TalkAsync(ev.ReplyToken, user, textMessage.Text);
        }

        protected override async Task OnFollowAsync(FollowEvent ev)
        {
            if(ev.Source.Type != EventSourceType.User) { return; }
            await _messagingClient.ReplyMessageAsync(ev.ReplyToken, "友達登録ありがとう！", ReplyMessage_Usage);
            await new BingoMenuManager(_messagingClient).SetStartMenuAsync(ev.Source.Id);
        }

        protected override async Task OnPostbackAsync(PostbackEvent ev)
        {
            var userId = ev.Source.UserId;
            var gameEntry = await _tableStorage.FindGameEntryAsync(userId);
            var cardEntry = await _tableStorage.FindCardEntryAsync(userId);
            var replyToken = ev.ReplyToken;

            switch (ev.Postback.Data)
            {
                case "exit":
                    await DeleteBingoEntry(replyToken, gameEntry, cardEntry);
                    await new BingoMenuManager(_messagingClient).SetStartMenuAsync(userId);
                    break;
                case "cancel-exit":
                    await _messagingClient.ReplyMessageAsync(replyToken, "キャンセルしました。");
                    break;
                case BingoMenuManager.POSTBACK_DATA_START_MENU_HINT:
                    await _messagingClient.ReplyMessageAsync(replyToken, ReplyMessage_Usage);
                    break;
                case BingoMenuManager.POSTBACK_DATA_GAME_MENU_HINT:
                    if (gameEntry?.GameId < 0)
                    {
                        await _messagingClient.ReplyMessageAsync(replyToken, ReplyMessage_Start);
                    }
                    else
                    {
                        await _messagingClient.ReplyMessageAsync(replyToken, Help_GameEntry);
                    }
                    break;
                case BingoMenuManager.POSTBACK_DATA_CARD_MENU_HINT:
                    if (cardEntry?.CardId < 0)
                    {
                        await _messagingClient.ReplyMessageAsync(replyToken, ReplyMessage_Join);
                    }
                    else
                    {
                        await _messagingClient.ReplyMessageAsync(replyToken, Help_CardCreated);
                    }
                    break;
            }
        }

        private async Task TalkAsync(string replyToken, UserProfile user, string userMessage)
        {
            try
            {
                await TalkAsync_impl(replyToken, user, userMessage);
            }
            catch (NewEntryException ex)
            {
                _log.Error("NewEntryAsync method has failed.", ex);
                var replyMessage = "ごめんなさい。ゲームの開始または参加に失敗しました。";
                await _messagingClient.ReplyMessageAsync(replyToken, new[] { new TextMessage(replyMessage) });
            }
            catch (RegisterGameException ex)
            {
                _log.Error("RegisterGameAsync method has failed.", ex);
                var replyMessage = "ごめんなさい。ゲームの作成に失敗しました。";
                await _messagingClient.ReplyMessageAsync(replyToken, new[] { new TextMessage(replyMessage) });
            }
            catch (RegisterCardException ex)
            {
                _log.Error("RegisterCardAsync method has failed.", ex);
                var replyMessage = "ごめんなさい。カードの作成に失敗しました。";
                await _messagingClient.ReplyMessageAsync(replyToken, new[] { new TextMessage(replyMessage) });
            }
            catch (RunGameException ex)
            {
                _log.Error("RunGameAsync method has failed.", ex);
                var replyMessage = "ごめんなさい。ゲーム中にエラーが発生しました。";
                await _messagingClient.ReplyMessageAsync(replyToken, new[] { new TextMessage(replyMessage) });
            }
            catch (GetCardException ex)
            {
                _log.Error("GetCardAsync method has failed.", ex);
                var replyMessage = "ごめんなさい。カード情報の取得に失敗しました。";
                await _messagingClient.ReplyMessageAsync(replyToken, new[] { new TextMessage(replyMessage) });
            }
        }

        private async Task TalkAsync_impl(string replyToken, UserProfile user, string userMessage)
        {
            var gameEntry = await _tableStorage.FindGameEntryAsync(user.UserId);
            var cardEntry = await _tableStorage.FindCardEntryAsync(user.UserId);

            var noEntry = gameEntry == null && cardEntry == null;
            var gameUnregisterd = gameEntry != null && gameEntry.GameId < 0;
            var cardUnregisterd = cardEntry != null && cardEntry.CardId < 0;
            var gameRegisterd = gameEntry != null && gameEntry.GameId >= 0;
            var cardRegisterd = cardEntry != null && cardEntry.CardId >= 0;

            if (noEntry)
            {
                await NewEntryAsync(replyToken, user.UserId, userMessage);
                return;
            }

            if (userMessage == "終了")
            {
                await _messagingClient.ReplyMessageAsync(replyToken, new[]
                {
                    new TemplateMessage("confirm exit", new ConfirmTemplate("一度終了したゲームは再開できません。ゲームを終了しますか？",
                        new []{
                            new PostbackTemplateAction("はい","exit"),
                            new PostbackTemplateAction("キャンセル","cancel-exit")
                        }))
                });
                return;
            }

            if (gameUnregisterd)
            {
                await RegisterGameAsync(replyToken, user.UserId, userMessage);
                return;
            }

            if (cardUnregisterd)
            {
                var gameInfo = userMessage.Split(new[] { ' ', '　', '\t' });
                int gameId;
                if (gameInfo.Length > 0 && int.TryParse(gameInfo[0], out gameId))
                {
                    var keyword = (gameInfo.Length > 1) ? gameInfo[1] : "";
                    await RegisterCardAsync(replyToken, user, keyword, gameId);
                }
                else
                {
                    await _messagingClient.ReplyMessageAsync(replyToken, new[] { new TextMessage(ReplyMessage_Join) });
                }
                return;
            }

            if (gameRegisterd && userMessage == "ドロー")
            {
                await RunGameAsync(replyToken, gameEntry);
                return;
            }

            if (cardRegisterd && userMessage == "カード")
            {
                await GetCardAsync(replyToken, cardEntry.GameId, cardEntry.CardId);
                return;
            }

            if (cardRegisterd || gameRegisterd)
            {
                var replyMessage = "";
                switch (userMessage)
                {

                    case "開始":
                        replyMessage = (gameEntry != null) ?
                            "ゲーム進行中は新しいゲームを始められないよ！ ゲームを終了するには「終了」を入力して送ってね。" + Environment.NewLine
                                + "ただし、終了したゲームを再開することはできないので注意してね！"
                            : "ゲームに参加中は新しいゲームを始められないよ！ 現在のゲームから抜けるには「終了」を入力して送ってね。";
                        await _messagingClient.ReplyMessageAsync(replyToken, new[] { new TextMessage(replyMessage) });
                        return;
                    case "参加":
                        replyMessage = (gameEntry != null) ?
                            "ゲーム進行中は違うゲームに参加できないよ！ ゲームを終了するには「終了」を入力して送ってね。" + Environment.NewLine
                            + "ただし、終了したゲームを再開することはできないので注意してね！。"
                            : "ゲームに参加中は違うゲームに参加できないよ！ 現在のゲームから抜けるには「終了」を入力して送ってね。";
                        await _messagingClient.ReplyMessageAsync(replyToken, new[] { new TextMessage(replyMessage) });
                        return;
                    case "カード":
                        replyMessage = "すみません。ゲームの進行役にはカードは配られません。";
                        await _messagingClient.ReplyMessageAsync(replyToken, new[] { new TextMessage(replyMessage) });
                        return;
                    case "ドロー":
                        replyMessage = "番号を引けるのは、ゲームを作成した進行役だけだよ！";
                        await _messagingClient.ReplyMessageAsync(replyToken, new[] { new TextMessage(replyMessage) });
                        return;
                }
                await BloadcastUserMessageAsync(user, userMessage, gameEntry, cardEntry);
            }
        }

        private async Task BloadcastUserMessageAsync(UserProfile user, string userMessage, BingoEntry gameEntry, BingoEntry cardEntry)
        {
            BingoEntry bingoEntry;
            string gameUserId;
            if (cardEntry != null)
            {
                gameUserId = _tableStorage.FindGameEntry(cardEntry.GameId)?.RowKey;
                bingoEntry = cardEntry;
            }
            else
            {
                bingoEntry = gameEntry;
                gameUserId = gameEntry.RowKey;
            }

            var cardUsers = await _tableStorage.GetCardUsersAsync(bingoEntry.GameId);
            var to = cardUsers
                .Select(u => u.UserId)
                .Concat(new[] { gameUserId })
                .Where(u => u != user.UserId).ToArray();
            await _messagingClient.MultiCastMessageAsync(to,
                new[]
                {
                        new TextMessage("@"+ user.DisplayName + Environment.NewLine + userMessage)
                });
        }

        private async Task DeleteBingoEntry(string replyToken, BingoEntry gameEntry, BingoEntry cardEntry)
        {
            if (gameEntry != null)
            {
                await _tableStorage.DeleteGameEntryAsync(gameEntry);
                await DeleteCardsAsync(gameEntry);

                await _bingoClient.DeleteGameAsync(gameEntry.GameId, gameEntry.AccessKey);
                if (gameEntry.GameId < 0)
                {
                    await _messagingClient.ReplyMessageAsync(replyToken,
                         "ゲームを終了しました。");
                }
                else
                {
                    await _messagingClient.ReplyMessageAsync(replyToken,
                          $"ID:{gameEntry.GameId}のゲームと、ゲームの情報、参加者の情報を全て削除しました。");
                }
            }
            else if (cardEntry != null)
            {
                await DeleteCardStorageAsync(cardEntry);
                if (cardEntry.CardId < 0)
                {
                    await _messagingClient.ReplyMessageAsync(replyToken,
                        "ゲームへの参加をやめました。");
                }
                else
                {
                    await _messagingClient.ReplyMessageAsync(replyToken,
                        $"ID:{cardEntry.GameId}のゲームから抜けて、カードの情報を削除しました。");
                }
            }

        }

        private async Task DeleteCardsAsync(BingoEntry gameEntry)
        {
            var users = await _tableStorage.GetCardUsersAsync(gameEntry.GameId);
            var tasks = users.Select(async usr =>
            {
                var cardEntry = await _tableStorage.FindCardEntryAsync(usr.UserId);
                await DeleteCardStorageAsync(cardEntry);
            });
            await Task.WhenAll(tasks);
        }

        private async Task DeleteCardStorageAsync(BingoEntry cardEntry)
        {
            await _tableStorage.DeleteCardEntryAsync(cardEntry);
            await _tableStorage.DeleteCardUserAsync(cardEntry);
            await _blobStorage.DeleteDirectoryAsync(cardEntry.CardId.ToString());
        }

        private async Task RegisterCardAsync(string replyToken, UserProfile user, string keyword, int gameId)
        {
            try
            {

                var cardId = await _bingoClient.AddCardAsync(gameId, keyword);
                await _tableStorage.UpdateCardEntryAsync(user.UserId, cardId, gameId);
                await _tableStorage.AddCardUserAsync(gameId, cardId, user.UserId, user.DisplayName);

                var cardStatus = await _bingoClient.GetCardStatusAsync(cardId);

                //string replyMessage = CreateCardString(cardStatus);
                var imageMessage = await CreateImageMessageAsync(cardStatus, gameId, cardId);
                await _messagingClient.ReplyMessageAsync(replyToken, new ISendMessage[] {
                    new TextMessage(ReplyMessage_CardCreated),
                    imageMessage
                 });

                var cardUsers = await _tableStorage.GetCardUsersAsync(gameId);
                var gameUser = _tableStorage.FindGameEntry(gameId)?.RowKey;
                var to = cardUsers.Where(cusr => cusr.UserId != user.UserId).Select(cusr => cusr.UserId).Concat(new[] { gameUser }).ToArray();
                await _messagingClient.MultiCastMessageAsync(to,
                    new[] { new TextMessage($"{user.DisplayName} さんがエントリーしました！") });

            }
            catch (Exception e)
            {
                throw new RegisterCardException(e.Message, e);
            }
        }

        private async Task<ImageMessage> CreateImageMessageAsync(CardStatus cardStatus, int gameId, int cardId)
        {
            var cardUser = await _tableStorage.FindCardUserAsync(gameId, cardId);
            await UpdateImageNumberAsync(cardUser);

            var cardImage = new BingoCardImage((IList<CardCellStatus>)cardStatus.CardCells);
            var imageUri = await _blobStorage.UploadImageAsync(cardImage.Image, cardStatus.CardId.ToString(), cardUser.ImageNumber + ".jpg");
            var previewUri = await _blobStorage.UploadImageAsync(cardImage.PreviewImage, cardStatus.CardId.ToString(), cardUser.ImageNumber + "_preview.jpg");

            return new ImageMessage(imageUri.ToString(), previewUri.ToString());
        }

        private async Task RunGameAsync(string replyToken, BingoEntry gameUser)
        {
            try
            {
                var gameStatus = await _bingoClient.DrawNextNumber(gameUser.GameId, gameUser.AccessKey);
                var drawNumber = gameStatus.DrawResults.Last();

                //Get all card-statuses in this game from BINGO API.
                var tasks = gameStatus.Cards.Select(async cardId => await _bingoClient.GetCardStatusAsync(cardId)).ToArray();
                var cardStatuses = await Task.WhenAll(tasks);

                //Get all card-users in this game from table storage.
                var cardUsers = await _tableStorage.GetCardUsersAsync(gameUser.GameId);

                //Set bingo-line and lizhi-line counts from card-statuses to card-Users. 
                SetNextLineCount(cardStatuses, cardUsers);

                var lizhiMessages = cardUsers
                    .Where(cusr => cusr.NextBingoLineCount == cusr.BingoLineCount)
                    .Where(cusr => cusr.NextLizhiLineCount > cusr.LizhiLineCount)
                    .Select(cusr => $"{cusr.UserName}さん リーチ{((cusr.NextLizhiLineCount > 1) ? "×" + cusr.NextLizhiLineCount : "")}!");
                var bingoMessages = cardUsers
                    .Where(cusr => cusr.NextBingoLineCount > cusr.BingoLineCount)
                    .Select(cusr => $"{cusr.UserName}さん ビンゴ{((cusr.NextBingoLineCount > 1) ? "×" + cusr.NextBingoLineCount : "")}!!");

                var messages = new[] { $"No. {drawNumber}!!" }
                    .Concat(lizhiMessages)
                    .Concat(bingoMessages)
                    .Select(msg => new TextMessage(msg)).OfType<ISendMessage>().ToList();

                await _messagingClient.ReplyMessageAsync(replyToken, messages);
                await _messagingClient.MultiCastMessageAsync(cardUsers.Select(cusr => cusr.UserId).ToList(), messages);
                await UpdateCardUserLineCountAsync(cardUsers);
            }
            catch (Exception e)
            {
                throw new RunGameException(e.Message, e);
            }
        }

        private async Task UpdateImageNumberAsync(CardUser cardUser)
        {
            cardUser.ImageNumber += 1;
            if (cardUser.ImageNumber > 10)
            {
                await _blobStorage.DeleteImageAsync(cardUser.RowKey, (cardUser.ImageNumber - 10) + ".jpg");
                await _blobStorage.DeleteImageAsync(cardUser.RowKey, (cardUser.ImageNumber - 10) + "_preview.jpg");
            }
            await _tableStorage.UpdateCardUserAsync(cardUser);
        }

        private static void SetNextLineCount(IList<CardStatus> cardStatuses, IList<CardUser> cardUsers)
        {
            foreach (var cusr in cardUsers)
            {
                var cardState = cardStatuses.FirstOrDefault(cst => cst.CardId.ToString() == cusr.RowKey);
                if (cardState == null) { continue; }
                cusr.NextBingoLineCount = cardState.BingoLineCount;
                cusr.NextLizhiLineCount = cardState.LizhiLineCount;
            }
        }

        private async Task UpdateCardUserLineCountAsync(IList<CardUser> cardUsers)
        {
            foreach (var cardUser in cardUsers)
            {
                cardUser.BingoLineCount = cardUser.NextBingoLineCount;
                cardUser.LizhiLineCount = cardUser.NextLizhiLineCount;
                await _tableStorage.UpdateCardUserAsync(cardUser);
            }
        }

        private async Task GetCardAsync(string replyToken, int gameId, int cardId)
        {
            try
            {
                var cardStatus = await _bingoClient.GetCardStatusAsync(cardId);
                //var cardString = CreateCardString(cardStatus);
                var imageMessage = await CreateImageMessageAsync(cardStatus, gameId, cardId);
                await _messagingClient.ReplyMessageAsync(replyToken, new[] { imageMessage });
            }
            catch (Exception e)
            {
                throw new GetCardException(e.Message, e);
            }
        }

        private async Task RegisterGameAsync(string replyToken, string userId, string userMessage)
        {
            try
            {
                if (userMessage == "ドロー")
                {
                    await _messagingClient.ReplyMessageAsync(replyToken, "合言葉を入力してください。");
                    return;
                }
                var keyword = (userMessage == "なし") ? "" : userMessage.Substring(0, Math.Min(userMessage.Length, 20));

                var result = await _bingoClient.CreateGameAsync(keyword);
                await _tableStorage.UpdateGameEntryAsync(userId, result.GameId, result.AccessKey);

                var replyMessage = string.Format(ReplyMessage_GameEntry, result.GameId);
                if (!string.IsNullOrEmpty(keyword))
                {
                    await _messagingClient.ReplyMessageAsync(replyToken, $"合言葉を「{keyword}」に設定しました。", replyMessage);
                }
                else
                {
                    await _messagingClient.ReplyMessageAsync(replyToken, replyMessage);
                }
            }
            catch (Exception e)
            {
                throw new RegisterGameException(e.Message, e);
            }
        }

        private async Task NewEntryAsync(string replyToken, string userId, string userMessage)
        {
            var replyMessage = "";
            try
            {
                switch (userMessage)
                {
                    case "開始":
                        replyMessage = ReplyMessage_Start;
                        await _tableStorage.AddGameEntryAsync(userId);
                        await new BingoMenuManager(_messagingClient).SetGameMenuAsync(userId);
                        break;
                    case "参加":
                        replyMessage = ReplyMessage_Join;
                        await _tableStorage.AddCardEntryAsync(userId);
                        await new BingoMenuManager(_messagingClient).SetCardMenuAsync(userId);
                        break;
                    default:
                        replyMessage = ReplyMessage_Usage;
                        break;
                }
                await _messagingClient.ReplyMessageAsync(replyToken, new[] { new TextMessage(replyMessage) });
            }
            catch (Exception e)
            {
                throw new NewEntryException(e.Message, e);
            }
        }
    }
}