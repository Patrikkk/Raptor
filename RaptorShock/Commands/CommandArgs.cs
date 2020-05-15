using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace RaptorShock.CommandManager
{
    public class CommandArgs : EventArgs
    {
        public string Message { get; private set; }

        /// <summary>
        /// Parameters passed to the arguement. Does not include the command name.
        /// IE '/kick "jerk face"' will only have 1 argument
        /// </summary>
        public List<string> Parameters { get; private set; }

        public CommandArgs(string message, List<string> args)
        {
            Message = message;
            Parameters = args;
        }
    }
}
