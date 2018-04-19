using System;

namespace Slofth.Firebase.Database
{
    public class FirebaseDatabaseException : Exception
    {
        public FirebaseDatabaseException(string message = null) : base(message) { }
    }

    public class UnknownServerEventException : Exception
    {
        public UnknownServerEventException(string message = null) : base(message) { }
    }

    public class CouldNotParseAuthTokenException : Exception
    {
        public CouldNotParseAuthTokenException(string message = null) : base(message) { }
    }

    public class PremissionDeniedException : Exception
    {
        public PremissionDeniedException(string message = null) : base(message) { }
    }
}

