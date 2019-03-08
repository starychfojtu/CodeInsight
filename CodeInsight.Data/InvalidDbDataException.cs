using System;

namespace CodeInsight.Data
{
    public class InvalidDbDataException : Exception
    {
        public InvalidDbDataException(string message) : base(message)
        {
        }
    }
}