using LineBotFunctions.BingoApi;
using LineBotFunctions.Line.Messaging;
using LineBotFunctions.TableStrage;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LineBotFunctions
{
    public class TalkManager
    {
        private static readonly string ReplyMessage_Start
            = "ゲームに参加するために必要な\"合言葉\"を設定できます。好きな言葉(20文字まで)を入力して送ってね。" + Environment.NewLine +
                            "設定しない場合は「なし」と入力してください。";
        private static readonly string ReplyMessage_Join
            = "参加するゲームの\"ID番号\"と\"合言葉\"を入力してください。" +
                            "「123 あいことば」のように間に空白を入れて送ってね。" + Environment.NewLine +
                            "合言葉が決められていない場合は\"ID番号\"だけでいいよ。";
        private static readonly string ReplyMessage_Usage
            = "新しいゲームを始めるには「0」または「開始」を、" + Environment.NewLine +
                "ゲームに参加するには「1」または「参加」を返信してください。";
        private static readonly string ReplyMessage_GameEntry
            = "ゲームを開始しました(ゲームＩＤ:{0})。" + Environment.NewLine +
                "ゲームを進めるには、なにかメッセージを送ってね。メッセージを送る度に番号を1つ引くよ。" + Environment.NewLine +
                "ゲームを終了するには「終了」を送ってね。";

        private BingoBotTableStrage _tableStrage;
        private LineMessagingApi _messagingApi;
        private TraceWriter _log;

        public TalkManager(BingoBotTableStrage tableStrage, LineMessagingApi messagingApi, TraceWriter log)
        {
            _tableStrage = tableStrage;
            _messagingApi = messagingApi;
            _log = log;
        }

        public async Task TalkAsync(string replyToken, UserProfile user, string userMessage)
        {
            try
            {
                await TalkAsync_impl(replyToken, user, userMessage);
            }
            catch (NewEntryException ex)
            {
                _log.Error("NewEntryAsync method has failed.", ex);
                var replyMessage = "ごめんなさい。ゲームの開始または参加に失敗しました。";
                await _messagingApi.ReplyMessageAsync(replyToken, new[] { new TextMessage(replyMessage) });
            }
            catch (RegisterGameException ex)
            {
                _log.Error("RegisterGameAsync method has failed.", ex);
                var replyMessage = "ごめんなさい。ゲームの作成に失敗しました。";
                await _messagingApi.ReplyMessageAsync(replyToken, new[] { new TextMessage(replyMessage) });
            }
            catch (RegisterCardException ex)
            {
                _log.Error("RegisterCardAsync method has failed.", ex);
                var replyMessage = "ごめんなさい。カードの作成に失敗しました。";
                await _messagingApi.ReplyMessageAsync(replyToken, new[] { new TextMessage(replyMessage) });
            }
            catch (RunGameException ex)
            {
                _log.Error("RunGameAsync method has failed.", ex);
                var replyMessage = "ごめんなさい。ゲーム中にエラーが発生しました。";
                await _messagingApi.ReplyMessageAsync(replyToken, new[] { new TextMessage(replyMessage) });
            }
            catch (GetCardException ex)
            {
                _log.Error("GetCardAsync method has failed.", ex);
                var replyMessage = "ごめんなさい。カード情報の取得に失敗しました。";
                await _messagingApi.ReplyMessageAsync(replyToken, new[] { new TextMessage(replyMessage) });
            }
        }

        private async Task TalkAsync_impl(string replyToken, UserProfile user, string userMessage)
        {
            var gameEntry = await _tableStrage.FindGameEntryAsync(user.UserId);
            var cardEntry = await _tableStrage.FindCardEntryAsync(user.UserId);

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
                await DeleteBingoEntry(replyToken, gameEntry, cardEntry);
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
                    await _messagingApi.ReplyMessageAsync(replyToken, new[] { new TextMessage(ReplyMessage_Join) });
                }
                return;
            }

            if (gameRegisterd)
            {
                await RunGameAsync(replyToken, gameEntry);
                return;
            }

            if (cardRegisterd)
            {
                await GetCardAsync(replyToken, cardEntry.CardId);
                return;
            }
        }

        private async Task DeleteBingoEntry(string replyToken, BingoEntry gameEntry, BingoEntry cardEntry)
        {
            if (gameEntry != null)
            {
                await _tableStrage.DeleteGameEntryAsync(gameEntry);
                using (var bingo = new BingoApiClient())
                {
                    await bingo.DeleteGameAsync(gameEntry.GameId, gameEntry.AccessKey);
                }
                await _messagingApi.ReplyMessageAsync(replyToken,
                        new[] { new TextMessage($"ID:{gameEntry.GameId}のゲームと、ゲームの情報、参加者の情報を全て削除しました。") });
            }
            else if (cardEntry != null)
            {
                await _tableStrage.DeleteCardEntryAsync(cardEntry);
                await _tableStrage.DeleteCardUserAsync(cardEntry);

                await _messagingApi.ReplyMessageAsync(replyToken,
                    new[] { new TextMessage($"ID:{cardEntry.GameId}のゲームから抜けて、カードの情報を削除しました。") });
            }
        }

        private async Task RegisterCardAsync(string replyToken, UserProfile user, string keyword, int gameId)
        {
            try
            {
                using (var bingoApi = new BingoApiClient())
                {
                    var cardId = await bingoApi.AddCardAsync(gameId, keyword);
                    await _tableStrage.UpdateCardEntryAsync(user.UserId, cardId, gameId);
                    await _tableStrage.AddCardUserAsync(gameId, cardId, user.UserId, user.DisplayName);

                    var cardStatus = await bingoApi.GetCardStatusAsync(cardId);
                    string replyMessage = CreateCardString(cardStatus);
                    await _messagingApi.ReplyMessageAsync(replyToken, new[] {
                        new TextMessage("カードを作成しました。"),
                        new TextMessage(replyMessage)
                    });
                }
            }
            catch (Exception e)
            {
                throw new RegisterGameException(e.Message, e);
            }
        }

        private static string CreateCardString(CardStatus cardStatus)
        {
            var cells = cardStatus.CardCells.Select(c => c.IsOpen ? "●" : c.Number.ToString("d2"));
            var row1 = cells.Take(5);
            var row2 = cells.Skip(5).Take(5);
            var row3 = cells.Skip(10).Take(5);
            var row4 = cells.Skip(15).Take(5);
            var row5 = cells.Skip(20).Take(5);
            var replyMessage =
                string.Join(" ", row1) + Environment.NewLine +
                string.Join(" ", row2) + Environment.NewLine +
                string.Join(" ", row3) + Environment.NewLine +
                string.Join(" ", row4) + Environment.NewLine +
                string.Join(" ", row5);
            return replyMessage;
        }

        private async Task RunGameAsync(string replyToken, BingoEntry gameUser)
        {
            try
            {
                using (var bingoApi = new BingoApiClient())
                {
                    var status = await bingoApi.DrawNextNumber(gameUser.GameId, gameUser.AccessKey);
                    var drawNumber = status.DrawResults.Last();

                    var cardUsers = await _tableStrage.GetCardUsersAsync(gameUser.GameId);

                    var lizhiUser = status.LizhiCards
                        .Select(c => cardUsers.FirstOrDefault(cusr => cusr.RowKey == c.ToString())?.UserName + "さん リーチ！");
                    var bingoUser = status.BingoCards
                        .Select(c => cardUsers.FirstOrDefault(cusr => cusr.RowKey == c.ToString())?.UserName + "さん ビンゴ！");

                    var messages = new[]{
                        $"No. {drawNumber}!!"
                    }.Concat(lizhiUser).Concat(bingoUser).Select(msg => new TextMessage(msg)).OfType<IMessage>().ToList();

                    await _messagingApi.ReplyMessageAsync(replyToken, messages);
                    await _messagingApi.MultiCastMessageAsync(cardUsers.Select(cusr => cusr.UserId).ToList(), messages);
                }
            }
            catch (Exception e)
            {
                throw new RunGameException(e.Message, e);
            }
        }

        private async Task GetCardAsync(string replyToken, int cardId)
        {
            try
            {
                using (var bingoApi = new BingoApiClient())
                {
                    var cardStatus = await bingoApi.GetCardStatusAsync(cardId);
                    var cardString = CreateCardString(cardStatus);
                    await _messagingApi.ReplyMessageAsync(replyToken, new[] { new TextMessage(cardString) });

                }
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
                var keyword = (userMessage == "なし") ? "" : userMessage.Substring(0, Math.Min(userMessage.Length, 20));
                using (var bingoApi = new BingoApiClient())
                {
                    var result = await bingoApi.CreateGameAsync(keyword);
                    await _tableStrage.UpdateGameEntryAsync(userId, result.GameId, result.AccessKey);

                    var replyMessage = string.Format(ReplyMessage_GameEntry, result.GameId);
                    await _messagingApi.ReplyMessageAsync(replyToken, new[] { new TextMessage(replyMessage) });
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
                    case "0":
                    case "開始":
                        replyMessage = ReplyMessage_Start;
                        await _tableStrage.AddGameEntryAsync(userId);
                        break;
                    case "1":
                    case "参加":
                        replyMessage = ReplyMessage_Join;
                        await _tableStrage.AddCardEntryAsync(userId);
                        break;
                    default:
                        replyMessage = ReplyMessage_Usage;
                        break;
                }
                await _messagingApi.ReplyMessageAsync(replyToken, new[] { new TextMessage(replyMessage) });
            }
            catch (Exception e)
            {
                throw new NewEntryException(e.Message, e);
            }
        }
    }
}