using System;
using System.Collections.Generic;
using System.Linq;

namespace BingoWebApi.Models
{
    public static class CardCellExtensions
    {
        //   c1,c2,c3,c4,c5
        //--:---------------
        //r1: 0, 1, 2, 3, 4, 
        //r2: 5, 6, 7, 8, 9,
        //r3:10,11,12,13,14,
        //r4:15,16,17,18,19,
        //r5:20,21,22,23,24,

        private static IList<BingoLine> _bingoLines = new[] {
            new BingoLine( BingoLineType.Row1, new[] { 0, 1, 2, 3, 4 }),
            new BingoLine(BingoLineType.Row2,new[] { 5, 6, 7, 8, 9 }),
            new BingoLine(BingoLineType.Row3,new[] { 10, 11, 12, 13, 14 }),
            new BingoLine(BingoLineType.Row4,new[] { 15, 16, 17, 18, 19 }),
            new BingoLine(BingoLineType.Row5,new[] { 20, 21, 22, 23, 24 }),
            new BingoLine(BingoLineType.Col1,new[] { 0, 5, 10, 15, 20 }),
            new BingoLine(BingoLineType.Col2,new[] { 1, 6, 11, 16, 21 }),
            new BingoLine(BingoLineType.Col3,new[] { 2, 7, 12, 17, 22 }),
            new BingoLine(BingoLineType.Col4,new[] { 3, 8, 13, 18, 23 }),
            new BingoLine(BingoLineType.Col5,new[] { 4, 9, 14, 19, 24 }),
            new BingoLine(BingoLineType.Slash,new[] { 4, 8, 12, 16, 20 }),
            new BingoLine(BingoLineType.BackSlash,new[] { 0, 6, 12, 18, 24 }),
            };


        public static IList<BingoLineType> GetBingoLines(this IList<CardCell> cardCells)
        {
            return _bingoLines
                .Where(line => line.Indexes.All(index => cardCells.Any(cell => cell.IsOpen && cell.Index == index)))
                .Select(line => line.LineType)
                .ToList();
        }

        public static IList<LizhiLine> GetLizhiLines(this IList<CardCell> cardCells)
        {
            return _bingoLines
                .Where(line => line.Indexes.Count(index => cardCells.Any(cell => cell.IsOpen && cell.Index == index)) == 4)
                .Select(line => 
                {
                    var waitingIndex = line.Indexes.First(i => !cardCells[i].IsOpen);
                    var waitingNumber = cardCells[waitingIndex].Number;
                    return new LizhiLine(line.LineType, waitingNumber);
                })
                .ToList();
        }
        
        public static IList<CardCellStatus> ToStatus(this IList<CardCell> cardCells)
        {
            return cardCells
                .OrderBy(cell => cell.Index)
                .Select(cell => new CardCellStatus()
                {
                    Number = cell.Number,
                    IsOpen = cell.IsOpen
                }).ToList();
        }
    }
}
