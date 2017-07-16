using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoWebApi.Models
{
    public static class CardExtensions
    {
        public static CardStatus ToStatus(this Card card)
        {
            return new CardStatus(
                card.Id,
                card.CardCells.ToStatus(),
                card.CardCells.GetBingoLines(),
                card.CardCells.GetLizhiLines());
        }
    }
}
