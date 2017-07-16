using System;
using System.Runtime.Serialization;

namespace LineBotFunctions
{
    public class BingoBotTalkException : Exception
    { 
        public BingoBotTalkException(string message) : base(message)
        {
        }

        public BingoBotTalkException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BingoBotTalkException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
    public class NewEntryException : BingoBotTalkException
    {
        public NewEntryException(string message) : base(message)
        {
        }

        public NewEntryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NewEntryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class RegisterGameException : BingoBotTalkException
    {
        public RegisterGameException(string message) : base(message)
        {
        }

        public RegisterGameException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RegisterGameException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class GetCardException : BingoBotTalkException
    {
        public GetCardException(string message) : base(message)
        {
        }

        public GetCardException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected GetCardException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class RunGameException : BingoBotTalkException
    {
        public RunGameException(string message) : base(message)
        {
        }

        public RunGameException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RunGameException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class RegisterCardException : BingoBotTalkException
    {
        public RegisterCardException(string message) : base(message)
        {
        }

        public RegisterCardException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RegisterCardException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
