using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoWebApi.Models
{
    public class InvalidAccessKeyException : Exception
    {
        public InvalidAccessKeyException()
        {
        }

        public InvalidAccessKeyException(string message) : base(message)
        {
        }

        public InvalidAccessKeyException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
