using System;

namespace GeoProfs.Domain.Exceptions
{
    public class InsufficientLeaveBalanceException : Exception
    {
        public InsufficientLeaveBalanceException(string message) : base(message) { }
    }
}
