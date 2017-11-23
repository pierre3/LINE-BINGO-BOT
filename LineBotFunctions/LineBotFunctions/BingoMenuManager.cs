using Line.Messaging;
using System.Linq;
using System.Threading.Tasks;

namespace LineBotFunctions
{
    public class BingoMenuManager
    {
        private LineMessagingClient _client;
        private static readonly string MENU_START = "start";
        private static readonly string MENU_GAME = "game";
        private static readonly string MENU_CARD = "card";
        private static readonly string CHAT_BAR_TEXT = "BINGO Menu";

        private static readonly int BUTTON_M_WIDTH = 843;
        private static readonly int BUTTON_L_WIDTH = 1400;
        private static readonly int BUTTON_S_WIDTH = (ImagemapSize.RichMenuShort.Width - BUTTON_L_WIDTH) / 2;
        private static readonly int BUTTON_HEIGHT = ImagemapSize.RichMenuShort.Height;

        public const string POSTBACK_DATA_START_MENU_HINT = "Start-MenuHint";
        public const string POSTBACK_DATA_GAME_MENU_HINT = "Game-MenuHint";
        public const string POSTBACK_DATA_CARD_MENU_HINT = "Card-MenuHint";

        public BingoMenuManager(LineMessagingClient client)
        {
            _client = client;
        }

        public async Task RegisterBingoMenuAsync()
        {
            var menus = await _client.GetRichMenuListAsync();
            foreach (var menu in menus)
            {
                await _client.DeleteRichMenuAsync(menu.RichMenuId);
            }

            await CreateMenuAsync(MENU_START, Properties.Resources.menu_start,
                new ActionArea()
                {
                    Action = new MessageTemplateAction("ゲームを開始する", "開始"),
                    Bounds = new ImagemapArea(0, 0, BUTTON_M_WIDTH, BUTTON_HEIGHT)
                },
                new ActionArea()
                {
                    Action = new MessageTemplateAction("ゲームに参加する", "参加"),
                    Bounds = new ImagemapArea(BUTTON_M_WIDTH, 0, BUTTON_M_WIDTH, BUTTON_HEIGHT)
                },
                new ActionArea()
                {
                    Action = new PostbackTemplateAction("ヒント", POSTBACK_DATA_START_MENU_HINT),
                    Bounds = new ImagemapArea(BUTTON_M_WIDTH * 2, 0, BUTTON_M_WIDTH, BUTTON_HEIGHT)
                });

            await CreateMenuAsync(MENU_GAME, Properties.Resources.menu_game,
                new ActionArea()
                {
                    Action = new MessageTemplateAction("番号を引く", "ドロー"),
                    Bounds = new ImagemapArea(0, 0, BUTTON_L_WIDTH, BUTTON_HEIGHT)
                },
                new ActionArea()
                {
                    Action = new MessageTemplateAction("ゲームを終了する", "終了"),
                    Bounds = new ImagemapArea(BUTTON_L_WIDTH, 0, BUTTON_S_WIDTH, BUTTON_HEIGHT)
                },
                new ActionArea()
                {
                    Action = new PostbackTemplateAction("ヒント", POSTBACK_DATA_GAME_MENU_HINT),
                    Bounds = new ImagemapArea(BUTTON_L_WIDTH + BUTTON_S_WIDTH, 0, BUTTON_S_WIDTH, BUTTON_HEIGHT)
                });


            await CreateMenuAsync(MENU_CARD, Properties.Resources.menu_card,
                new ActionArea()
                {
                    Action = new MessageTemplateAction("カードを更新する", "カード"),
                    Bounds = new ImagemapArea(0, 0, BUTTON_L_WIDTH, BUTTON_HEIGHT)
                },
                new ActionArea()
                {
                    Action = new MessageTemplateAction("ゲームから抜ける", "終了"),
                    Bounds = new ImagemapArea(BUTTON_L_WIDTH, 0, BUTTON_S_WIDTH, BUTTON_HEIGHT)
                },
                new ActionArea()
                {
                    Action = new PostbackTemplateAction("ヒント", POSTBACK_DATA_CARD_MENU_HINT),
                    Bounds = new ImagemapArea(BUTTON_L_WIDTH + BUTTON_S_WIDTH, 0, BUTTON_S_WIDTH, BUTTON_HEIGHT)
                });
        }

        private async Task CreateMenuAsync(string menuName, System.Drawing.Image image, params ActionArea[] areas)
        {
            var menu = new RichMenu()
            {
                Name = menuName,
                ChatBarText = CHAT_BAR_TEXT,
                Selected = true,
                Size = ImagemapSize.RichMenuShort,
                Areas = areas
            };
            var menuId = await _client.CreateRichMenuAsync(menu);
            using (var stream = new System.IO.MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;
                await _client.UploadRichMenuPngImageAsync(stream, menuId);
            }
        }

        public async Task SetRichMenuAsync(string userId, string menuName)
        {
            var menu = (await _client.GetRichMenuListAsync()).FirstOrDefault(m => m.Name == menuName);
            if (menu == null)
            {
                return;
            }

            var currentMenuId = "";
            try
            {
                currentMenuId = await _client.GetRichMenuIdOfUserAsync(userId);
            }
            catch (LineResponseException e)
            {
                if (e.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    throw;
                }
            }

            if (menu.RichMenuId == currentMenuId)
            {
                return;
            }
            await _client.LinkRichMenuToUserAsync(userId, menu.RichMenuId);
        }

        public Task SetStartMenuAsync(string userId)
        {
            return SetRichMenuAsync(userId, MENU_START);
        }

        public Task SetGameMenuAsync(string userId)
        {
            return SetRichMenuAsync(userId, MENU_GAME);
        }

        public Task SetCardMenuAsync(string userId)
        {
            return SetRichMenuAsync(userId, MENU_CARD);
        }
    }
}
