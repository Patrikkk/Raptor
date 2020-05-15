using System;
using JetBrains.Annotations;

namespace RaptorShock.CommandManager
{
    /// <summary>
    ///     Specifies that a method is a command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    [MeansImplicitUse]
    public sealed class CommandAttribute : Attribute
    {
        /// <summary>
        /// Gets the names of the command.
        /// </summary>
        public string[] Names { get; }

        /// <summary>
        /// Gets or sets whether to do logging of this command.
        /// </summary>
        public bool DoLog { get; set; } = true;


        public CommandAttribute(params string[] aliases)
        {
            Names = aliases;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public sealed class CommandHelpAttribute : Attribute
    {
        public CommandHelpAttribute(string helpText)
        {
            HelpText = helpText;
        }

        /// <summary>
        /// Gets the description of this command.
        /// </summary>
        public string HelpText { get; }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public sealed class CommandDescriptionAttribute : Attribute
    {
        public CommandDescriptionAttribute(params string[] helpDesc)
        {
            HelpDesc = helpDesc;
        }

        /// <summary>
        /// Gets the syntax of this command.
        /// </summary>
        public string[] HelpDesc { get; }
    }
}
