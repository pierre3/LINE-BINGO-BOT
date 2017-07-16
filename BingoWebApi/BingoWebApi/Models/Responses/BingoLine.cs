namespace BingoWebApi.Models
{
    public class BingoLine
    {
        public BingoLineType LineType { get; }
        public int[] Indexes { get; }
        public BingoLine(BingoLineType lineType, int[] indexes)
        {
            LineType = lineType;
            Indexes = indexes;
        }
    }

}
