using System;

namespace CurioClerk.Core.Shifts
{
    public static class DailySeedProvider
    {
        public static int ForDate(DateTime localDate, int contentVersion)
        {
            unchecked
            {
                const uint offset = 2166136261;
                const uint prime = 16777619;
                var value = offset;
                var key = $"{localDate:yyyyMMdd}:{contentVersion}";
                for (var index = 0; index < key.Length; index++)
                {
                    value ^= key[index];
                    value *= prime;
                }

                return (int)(value & 0x7fffffff);
            }
        }
    }
}

