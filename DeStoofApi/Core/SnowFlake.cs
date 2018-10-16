using System;

namespace Core
{
    public struct Snowflake
    {
        private const ulong DiscordEpoch = 1420070400000UL;
        private const ulong CreationMask = (1 << 12) - 1;
        private const ulong ProcessAndWorkerMask = (1 << 5) - 1;

        public ulong RawValue { get; }

        public Snowflake(ulong value) => RawValue = value;
        public Snowflake(DateTimeOffset dto) => RawValue = ((ulong)dto.ToUniversalTime().ToUnixTimeMilliseconds() - DiscordEpoch) << 22;
        public Snowflake(DateTime dt) : this(new DateTimeOffset(dt)) { }

        public DateTimeOffset DateTimeOffset => DateTimeOffset.FromUnixTimeMilliseconds((long)((RawValue >> 22) + DiscordEpoch));
        public DateTime DateTime => DateTimeOffset.UtcDateTime;
        public int CreationIndex => (int)(RawValue & CreationMask);
        public int ProcessId => (int)((RawValue >> 12) & ProcessAndWorkerMask);
        public int WorkersId => (int)((RawValue >> 17) & ProcessAndWorkerMask);

        public static implicit operator ulong(Snowflake snowflake) => snowflake.RawValue;
        public static implicit operator Snowflake(ulong value) => new Snowflake(value);

        public override string ToString() => RawValue.ToString();
    }
}