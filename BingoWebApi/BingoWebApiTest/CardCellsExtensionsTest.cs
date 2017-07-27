using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BingoWebApi.Models;
using System.Linq;

namespace BingoWebApiTest
{
    [TestClass]
    public class CardCellsExtensionsTest
    {
        [TestMethod]
        public void GetLizhiLinesTest()
        {
            var cardCells = new CardCell[] {
                new CardCell(){ CardId=0, Index=0, IsOpen=true, Number=1 },
                new CardCell(){ CardId=0, Index=1, IsOpen=true, Number=2 },
                new CardCell(){ CardId=0, Index=2, IsOpen=true, Number=3 },
                new CardCell(){ CardId=0, Index=3, IsOpen=true, Number=4 },
                new CardCell(){ CardId=0, Index=4, IsOpen=false, Number=5 },

                new CardCell(){ CardId=0, Index=5, IsOpen=false, Number=6 },
                new CardCell(){ CardId=0, Index=6, IsOpen=true, Number=7 },
                new CardCell(){ CardId=0, Index=7, IsOpen=true, Number=8 },
                new CardCell(){ CardId=0, Index=8, IsOpen=false, Number=9 },
                new CardCell(){ CardId=0, Index=9, IsOpen=false, Number=10 },

                new CardCell(){ CardId=0, Index=10, IsOpen=false, Number=11 },
                new CardCell(){ CardId=0, Index=11, IsOpen=false, Number=12 },
                new CardCell(){ CardId=0, Index=12, IsOpen=true, Number=13 },
                new CardCell(){ CardId=0, Index=13, IsOpen=false, Number=14 },
                new CardCell(){ CardId=0, Index=14, IsOpen=false, Number=15 },

                new CardCell(){ CardId=0, Index=15, IsOpen=false, Number=16 },
                new CardCell(){ CardId=0, Index=16, IsOpen=false, Number=17 },
                new CardCell(){ CardId=0, Index=17, IsOpen=false, Number=18 },
                new CardCell(){ CardId=0, Index=18, IsOpen=true, Number=19 },
                new CardCell(){ CardId=0, Index=19, IsOpen=false, Number=20 },

                new CardCell(){ CardId=0, Index=20, IsOpen=false, Number=21 },
                new CardCell(){ CardId=0, Index=21, IsOpen=false, Number=22 },
                new CardCell(){ CardId=0, Index=22, IsOpen=true, Number=23 },
                new CardCell(){ CardId=0, Index=23, IsOpen=false, Number=24 },
                new CardCell(){ CardId=0, Index=24, IsOpen=false, Number=25 },
            };

            var lizhi = cardCells.GetLizhiLines();
            Assert.AreEqual(3, lizhi.Count);
            CollectionAssert.AreEquivalent(
                lizhi.Select(x => x.LineType).ToArray(),
                new[] { BingoLineType.Row1, BingoLineType.Col3, BingoLineType.BackSlash });
            CollectionAssert.AreEquivalent(
                lizhi.Select(x => x.WaitingNumber).ToArray(),
                new[] { 5, 18, 25 });
        }

        [TestMethod]
        public void GetBIngoLinesTest()
        {
            var cardCells = new CardCell[] {
                new CardCell(){ CardId=0, Index=0, IsOpen=true, Number=1 },
                new CardCell(){ CardId=0, Index=1, IsOpen=true, Number=2 },
                new CardCell(){ CardId=0, Index=2, IsOpen=false, Number=3 },
                new CardCell(){ CardId=0, Index=3, IsOpen=false, Number=4 },
                new CardCell(){ CardId=0, Index=4, IsOpen=true, Number=5 },

                new CardCell(){ CardId=0, Index=5, IsOpen=false, Number=6 },
                new CardCell(){ CardId=0, Index=6, IsOpen=true, Number=7 },
                new CardCell(){ CardId=0, Index=7, IsOpen=false, Number=8 },
                new CardCell(){ CardId=0, Index=8, IsOpen=true, Number=9 },
                new CardCell(){ CardId=0, Index=9, IsOpen=false, Number=10 },

                new CardCell(){ CardId=0, Index=10, IsOpen=false, Number=11 },
                new CardCell(){ CardId=0, Index=11, IsOpen=true, Number=12 },
                new CardCell(){ CardId=0, Index=12, IsOpen=true, Number=13 },
                new CardCell(){ CardId=0, Index=13, IsOpen=false, Number=14 },
                new CardCell(){ CardId=0, Index=14, IsOpen=false, Number=15 },

                new CardCell(){ CardId=0, Index=15, IsOpen=true, Number=16 },
                new CardCell(){ CardId=0, Index=16, IsOpen=true, Number=17 },
                new CardCell(){ CardId=0, Index=17, IsOpen=true, Number=18 },
                new CardCell(){ CardId=0, Index=18, IsOpen=true, Number=19 },
                new CardCell(){ CardId=0, Index=19, IsOpen=true, Number=20 },

                new CardCell(){ CardId=0, Index=20, IsOpen=true, Number=21 },
                new CardCell(){ CardId=0, Index=21, IsOpen=true, Number=22 },
                new CardCell(){ CardId=0, Index=22, IsOpen=false, Number=23 },
                new CardCell(){ CardId=0, Index=23, IsOpen=false, Number=24 },
                new CardCell(){ CardId=0, Index=24, IsOpen=false, Number=25 },
            };

            var bingo = cardCells.GetBingoLines();
            Assert.AreEqual(3, bingo.Count);
            CollectionAssert.AreEquivalent(
                bingo.ToArray(),
                new[] { BingoLineType.Row4, BingoLineType.Col2, BingoLineType.Slash });
        }
    }
}
