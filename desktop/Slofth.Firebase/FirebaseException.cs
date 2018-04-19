using System;

namespace Slofth.Firebase
{
    public class FirebaseException : Exception
    {
        public FirebaseException(string message = null) : base(message) { }
    }
}
