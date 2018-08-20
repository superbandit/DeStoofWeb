using System;

namespace Models
{
    public class Enums
    {
        [Flags]
        public enum ChatPlatforms
        {
            None = 0,
            Twitch = 1,
            Discord = 2,
            Undefined = 4
        }
    }
}
