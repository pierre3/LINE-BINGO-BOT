using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BingoWebApi.Models
{
    public class AddCardParameter
    {
        [Required]
        public int GameId { get; set; }
        [Required]
        public string Keyword { get; set; }
    }
}
