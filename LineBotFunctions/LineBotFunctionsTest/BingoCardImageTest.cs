using Microsoft.VisualStudio.TestTools.UnitTesting;
using LineBotFunctions.Drawing;
using LineBotFunctions.BingoApi;
using LineBotFunctions.CloudStorage;
namespace LineBotFunctionsTest
{
    [TestClass]
    public class BingoCardImageTest
    {
        [TestMethod]
        public void TestMethod1()
        {


            var cardCells = new CardCellStatus[]
            {
                new CardCellStatus(){ IsOpen=true, Number=11 }, new CardCellStatus(){ IsOpen=false, Number=12 },new CardCellStatus(){ IsOpen=false, Number=13 },new CardCellStatus(){ IsOpen=false, Number=14 },new CardCellStatus(){ IsOpen=false, Number=15 },
                new CardCellStatus(){ IsOpen=false, Number=21 },new CardCellStatus(){ IsOpen=true, Number=22 },new CardCellStatus(){ IsOpen=false, Number=23 },new CardCellStatus(){ IsOpen=false, Number=34 },new CardCellStatus(){ IsOpen=false, Number=25 },
                new CardCellStatus(){ IsOpen=false, Number=31 },new CardCellStatus(){ IsOpen=false, Number=32 },new CardCellStatus(){ IsOpen=true, Number=0 },new CardCellStatus(){ IsOpen=false, Number=34 },new CardCellStatus(){ IsOpen=false, Number=35 },
                new CardCellStatus(){ IsOpen=false, Number=41 },new CardCellStatus(){ IsOpen=false, Number=42 },new CardCellStatus(){ IsOpen=false, Number=43 },new CardCellStatus(){ IsOpen=true, Number=44 },new CardCellStatus(){ IsOpen=false, Number=45 },
                new CardCellStatus(){ IsOpen=false, Number=51 },new CardCellStatus(){ IsOpen=false, Number=52 },new CardCellStatus(){ IsOpen=false, Number=53 },new CardCellStatus(){ IsOpen=false, Number=54 },new CardCellStatus(){ IsOpen=true, Number=55 }
            };
            var cardImage = new BingoCardImage(cardCells);
            cardImage.Image.Save("testImage.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            cardImage.PreviewImage.Save("PreviewImage.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
        }
    }
}
