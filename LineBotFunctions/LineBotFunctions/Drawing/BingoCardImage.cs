using LineBotFunctions.BingoApi;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace LineBotFunctions.Drawing
{
    public class BingoCardImage
    {
        private const int CELL_SIZE = 100;
        private const int PADDING = 10;
        private const int IMAGE_WIDTH = CELL_SIZE * 5 + PADDING * 2;
        private const int IMAGE_HEIGHT = CELL_SIZE * 6 + PADDING * 2;

        public Image Image { get; }
        public Image PreviewImage { get; }

        public BingoCardImage(IList<CardCellStatus> cardCells)
        {
            Image = new Bitmap(IMAGE_WIDTH, IMAGE_HEIGHT, PixelFormat.Format24bppRgb);
            CreateCardImage(Graphics.FromImage(Image), cardCells);
            PreviewImage = Image.GetThumbnailImage((int)(IMAGE_WIDTH * 0.3), (int)(IMAGE_HEIGHT * 0.3), () => false, IntPtr.Zero);
        }

        private void CreateCardImage(Graphics g, IList<CardCellStatus> cardCells)
        {
            g.FillRectangle(Brushes.DarkRed, 0, 0, IMAGE_WIDTH, IMAGE_HEIGHT);

            using (var font = new Font("Comic Sans MS", 46, FontStyle.Bold | FontStyle.Italic))
            {
                var format = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("B", font, Brushes.Gold, new RectangleF(PADDING + CELL_SIZE * 0, PADDING, CELL_SIZE, CELL_SIZE), format);
                g.DrawString("I", font, Brushes.Gold, new RectangleF(PADDING + CELL_SIZE * 1, PADDING, CELL_SIZE, CELL_SIZE), format);
                g.DrawString("N", font, Brushes.Gold, new RectangleF(PADDING + CELL_SIZE * 2, PADDING, CELL_SIZE, CELL_SIZE), format);
                g.DrawString("G", font, Brushes.Gold, new RectangleF(PADDING + CELL_SIZE * 3, PADDING, CELL_SIZE, CELL_SIZE), format);
                g.DrawString("O", font, Brushes.Gold, new RectangleF(PADDING + CELL_SIZE * 4, PADDING, CELL_SIZE, CELL_SIZE), format);
            }

            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    var cardCell = cardCells[y * 5 + x];
                    var left = PADDING + CELL_SIZE * x;
                    var top = PADDING + CELL_SIZE * (y + 1);

                    g.FillRectangle(Brushes.White, left + 2, top + 2, CELL_SIZE - 4, CELL_SIZE - 4);

                    if (cardCell.IsOpen)
                    {
                        g.FillEllipse(Brushes.Red, left + 4, top + 4, CELL_SIZE - 8, CELL_SIZE - 8);
                    }

                    using (var font1 = new Font("sans-serif", 34, FontStyle.Regular))
                    using (var font2 = new Font("Comic Sans MS", 24, FontStyle.Regular))
                    {
                        var brush = cardCell.IsOpen ? Brushes.White : Brushes.Black;
                        var text = cardCell.Number == 0 ? "Free" : cardCell.Number.ToString();
                        var font = cardCell.Number == 0 ? font2 : font1;
                        g.DrawString(text, font, brush, new RectangleF(left, top, CELL_SIZE, CELL_SIZE),
                            new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

                    }
                }
            }
        }
    }
}
