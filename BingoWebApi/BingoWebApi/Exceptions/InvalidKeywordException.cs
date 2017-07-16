using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoWebApi.Models
{
    public class InvalidKeywordException : Exception
    {
        public InvalidKeywordException()
        {
        }

        public InvalidKeywordException(string message) : base(message)
        {
        }

        public InvalidKeywordException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
