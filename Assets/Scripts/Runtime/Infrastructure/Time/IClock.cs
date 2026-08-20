using System;

namespace CurioClerk.Infrastructure.Time
{
    public interface IClock
    {
        DateTime LocalNow { get; }
    }

    public sealed class SystemClock : IClock
    {
        public DateTime LocalNow => DateTime.Now;
    }
}

