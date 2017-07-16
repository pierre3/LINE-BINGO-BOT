namespace BingoWebApi.Models
{
    public class CardCell
    {
        public int CardId { get; set; }
        public int Index { get; set; }
        public int Number { get; set; }
        public bool IsOpen { get; set; } = false;
    }
}
