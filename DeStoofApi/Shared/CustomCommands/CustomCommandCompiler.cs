using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Core.Messages;
using Core.Settings;

namespace DeStoofBot.CustomCommands
{
    public class CustomCommandCompiler
    {
        private readonly CommandContext _context;
        private readonly Dictionary<string, Func<string>> _macros;

        public CustomCommandCompiler(CommandContext context)
        {
            _context = context;
            _macros = new Dictionary<string, Func<string>>
            {
                ["{time}"] = GetTime,
                ["{caller}"] = GetCaller,
                ["{channel}"] = GetChannel
            };
        }       

        public string CompileCustomCommand(CustomCommand command)
        {
            var regex = new Regex(@"\{(.*?)\}");

            string output = command.InputString;
            foreach (Match m in regex.Matches(output))
                if (_macros.ContainsKey(m.Value)) output = output.Replace(m.Value, _macros[m.Value]());

            return output;
        }

        private string GetTime()
        {
            return DateTime.Now.ToString();
        }

        private string GetCaller()
        {
            return _context.Message.Author;
        }

        private string GetChannel()
        {
            return _context.GuildSettings.TwitchSettings.ChannelName ?? "Unknown";
        }
    }
}
