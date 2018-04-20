using System.Diagnostics.CodeAnalysis;

namespace DeStoofApi.Models
{
    public class Enums
    {
        public enum Platforms
        {
            Twitch,
            Discord
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum Numbers
        {
            zero,
            one,
            two,
            three,
            four,
            five,
            six,
            seven,
            eight,
            nine
        }
    }
}
