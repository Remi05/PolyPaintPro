using System;

namespace PolyPaint.Services.Database
{
    public class DatabaseException : Exception
    {
        public DatabaseException(Exception innerException)
            : base("An unknown error occured.", innerException) { }
    }

    public class PermissionDeniedException : Exception
    {
        public PermissionDeniedException(Exception innerException)
            : base("Permission denied.", innerException) { }
    }
}