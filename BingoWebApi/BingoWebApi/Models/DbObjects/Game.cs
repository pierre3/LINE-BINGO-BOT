using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BingoWebApi.Models
{
    public class Game
    {
        public int Id { get; set; }
        public string Keyword { get; set; }
        public int DrawCount { get; set; } = 0;
        public Guid AccessKey { get; set; } = Guid.NewGuid();

        [DatabaseGenerated( DatabaseGeneratedOption.Identity)]
        public DateTime CreatedTime { get; set; }

        public IList<DrawSource> DrawSource { get; set; } = new List<DrawSource>();

        public IList<Card> Cards { get; set; } = new List<Card>();
    }
}
