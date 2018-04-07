using System.Threading;

namespace DeStoofApi.Chatsources
{
    public class PingSender
    {
        private readonly TwitchSource _irc;
        private Thread pingSender;

        private bool active;

        public PingSender(TwitchSource irc)
        {
            _irc = irc;
            pingSender = new Thread(new ThreadStart(Run));
        }

        public void Start()
        {
            pingSender.IsBackground = true;
            active = true;
            pingSender.Start();
        }

        public void Stop()
        {
            active = false;
        }

        public void Run()
        {
            while (active)
            {
                _irc.SendIrcMessage("PING irc.twitch.tv");
                Thread.Sleep(300000); // 5 minutes
            }
        }
    }
}
