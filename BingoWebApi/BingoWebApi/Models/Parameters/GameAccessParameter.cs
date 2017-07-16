using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BingoWebApi.Models
{
    public class GameAccessParameter
    {
        [Required]
        public string AccessKey { get; set; }
    }
}
