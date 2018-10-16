namespace Core.Settings
{
    public class CustomCommand
    {
        public string Prefix { get; }
        public string InputString { get; }

        public CustomCommand(string prefix, string inputString)
        {
            Prefix = prefix;
            InputString = inputString;
        }
    }
}
