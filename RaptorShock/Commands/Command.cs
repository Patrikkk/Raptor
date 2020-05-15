using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Terraria;

namespace RaptorShock.CommandManager
{
    public delegate void CommandDelegate(CommandArgs args);

    /// <summary>
    ///     Represents a command.
    /// </summary>
    public sealed class Command
    {
        /// <summary>
		/// Gets or sets whether to do logging of this command.
		/// </summary>
		public bool DoLog { get; set; }
        /// <summary>
        /// Gets or sets the help text of this command.
        /// </summary>
        public string HelpText { get; set; }
        /// <summary>
        /// Gets or sets an extended description of this command.
        /// </summary>
        public string[] HelpDesc { get; set; }
        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        public string Name { get { return Names[0]; } }
        /// <summary>
        /// Gets the names of the command.
        /// </summary>
        public List<string> Names { get; protected set; }
        /// <summary>
        /// Gets the permissions of the command.
        /// </summary>


        private CommandDelegate commandDelegate;
        public CommandDelegate CommandDelegate
        {
            get { return commandDelegate; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                commandDelegate = value;
            }
        }

        public Command(CommandDelegate cmd, params string[] names)
        {
            if (cmd == null)
                throw new ArgumentNullException("cmd");
            if (names == null || names.Length < 1)
                throw new ArgumentException("names");

            CommandDelegate = cmd;
            DoLog = true;
            HelpText = "No help available.";
            HelpDesc = null;
            Names = new List<string>(names);
        }

        public bool Run(string msg, List<string> parms)
        {
            try
            {
                CommandDelegate(new CommandArgs(msg, parms));
            }
            catch (Exception e)
            {
                Utils.ShowErrorMessage("Command failed, check logs for more details.");
                RShockAPI.Log.Error(e.ToString());
            }

            return true;
        }

        public bool HasAlias(string name)
        {
            return Names.Contains(name);
        }
    }
}
