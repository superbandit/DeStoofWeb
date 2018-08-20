namespace Models.Domain.Guilds
{
    public class CustomCommand
    {
        public CustomCommand(bool prefixAnywhere, string prefix, string inputString)
        {
            PrefixAnywhere = prefixAnywhere;
            Prefix = prefix;
            InputString = inputString;
        }

        public bool PrefixAnywhere { get; set; }
        public string Prefix { get; set; }
        public string InputString { get; set; }
    }
}
