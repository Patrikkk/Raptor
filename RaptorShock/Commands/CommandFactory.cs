using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Reflection;

namespace RaptorShock.CommandManager
{
    public static class CommandFactory
    {
        public static IEnumerable<Command> GetCommands(Assembly assembly)
        {
            var cmdMethods = assembly.GetTypes()
              .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
              .Where(m => m.GetCustomAttributes(typeof(CommandAttribute), false).Length > 0);


            var cmds = GetCommands(cmdMethods, true);
            var commandgroups = GetCommandGroups(assembly).Concat(cmds);

            return commandgroups;
        }
        public static IEnumerable<Command> GetCommands(IEnumerable<MethodInfo> methods, bool filterSubCommands)
        {
            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<CommandAttribute>();

                if (attr == null || method.DeclaringType == null) continue;

                var parentGroup = method.DeclaringType.GetCustomAttribute<CommandAttribute>();
                if (parentGroup != null &&
                    filterSubCommands)
                    continue;

                var cmdName = (attr.Names.Length == 0 ? new[] { method.Name } : attr.Names)
                  .Select(s => s.ToLowerInvariant()).ToArray();

                var help = method.GetCustomAttribute<CommandHelpAttribute>()?.HelpText;
                var desc = method.GetCustomAttribute<CommandDescriptionAttribute>()?.HelpDesc ?? new string[0];

#if DEBUG
                RShockAPI.Log.Info($"Discovered command {cmdName[0]} in {method.DeclaringType}");
#endif

                var dlg = (CommandDelegate)Delegate.CreateDelegate(
                 typeof(CommandDelegate), method.DeclaringType, method.Name, false, true);

                if (!filterSubCommands)
                {


                    var cmd = new Command(dlg, cmdName);

                    cmd.DoLog = attr.DoLog;
                    cmd.HelpText = help;
                    cmd.HelpDesc = desc;
                    yield return cmd;
                }
                else
                    yield return new CommandHandler(attr, new List<Command>(), help, desc, dlg).ToCommand();
            }
        }

        public static IEnumerable<Command> GetCommandGroups(Assembly assembly)
        {
            var cmdGroups = assembly.GetTypes()
              .Where(t => t.IsPublic && t.IsClass && t.GetCustomAttributes(typeof(CommandAttribute), false).Length > 0);

            foreach (var type in cmdGroups)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
                var groupAttr = type.GetCustomAttribute<CommandAttribute>();

                var help = type.GetCustomAttribute<CommandHelpAttribute>()?.HelpText;
                var desc = type.GetCustomAttribute<CommandDescriptionAttribute>()?.HelpDesc ?? new string[0];

#if DEBUG
                RShockAPI.Log.Info($"- Discovered command group {type.Name} in {type.Assembly.GetName().Name}");
#endif

                yield return new CommandHandler(groupAttr, GetCommands(methods, false), help, desc).ToCommand();
            }
        }

        public sealed class CommandHandler
        {
            internal CommandHandler(CommandAttribute baseAttribute, IEnumerable<Command> subCommands,
              string helpText, string[] helpDesc, CommandDelegate dlg = null)
            {
                Names = baseAttribute.Names;
                DoLog = baseAttribute.DoLog;
                HelpText = helpText ?? "No help available.";
                HelpDesc = helpDesc;

                Subcommands = subCommands.ToArray();
                commandDelegate = dlg;
            }

            public string[] Names { get; }

            public bool DoLog { get; }

            public string HelpText { get; }
            public string[] HelpDesc { get; }

            public Command[] Subcommands { get; }

            private CommandDelegate commandDelegate;

            public void GroupCommandHandler(CommandArgs args)
            {
                try
                {
                    if (args.Parameters.Count < 1)
                    {
                        SendHelpText(args);
                        return;
                    }

                    if (args.Parameters[0].Equals("help", StringComparison.OrdinalIgnoreCase))
                    {
                        SendHelpText(args);
                        return;
                    }

                    var subcmd = Subcommands.FirstOrDefault(s => s.Names.Contains(args.Parameters[0]));

                    if (subcmd == null)
                        throw new CommandException($"Invalid subcommand! Try {RShockAPI.Config.CommandPrefix}{(args.Message.Split(' ')[0])} help for more info.");

                    subcmd.CommandDelegate.Invoke(
                      new CommandArgs(args.Message, args.Parameters.Skip(1).ToList())
                    );
                }
                catch (CommandException ex)
                {
                    Utils.ShowErrorMessage(ex.Message);
                }
            }

            public void SingleCommandHandler(CommandArgs args)
            {
                try
                {
                    string commandName = args.Message.Split(' ')[0];
                    var cmd = Commands.ChatCommands.FirstOrDefault(s => s.Names.Contains(commandName));

                    if (cmd == null)
                        throw new CommandException($"Invalid command! Try {RShockAPI.Config.CommandPrefix}{commandName} help for more info.");

                    commandDelegate.Invoke(
                      new CommandArgs(args.Message, args.Parameters)
                    );
                }
                catch (CommandException ex)
                {
                    Utils.ShowErrorMessage(ex.Message);
                }
            }


            private static string GetCalledAlias(CommandArgs args) => args.Message.Split(' ')[0];

            public IEnumerable<string> GetHelpText(CommandArgs args)
              => Subcommands.Select(s => string.Format("{0}{1}: {2}",
                  RShockAPI.Config.CommandPrefix, GetCalledAlias(args) + " " + s.Name,
                  string.IsNullOrWhiteSpace(s.HelpText) ? "No help available." : s.HelpText));

            public void SendHelpText(CommandArgs args, string error = null)
            {
                if (error != null) Utils.ShowErrorMessage(error);

                List<string> helpText = new List<string>();
                int pageParamIndex = 1;
                int pageNumber;

                if (args.Parameters.Count > 1 && args.Parameters[0] == "help" && !int.TryParse(args.Parameters[1], out int num))
                {
                    pageParamIndex = 2;
                    string commandName = args.Parameters[1];
                    var subcmd = Subcommands.FirstOrDefault(s => s.Names.Contains(args.Parameters[1]));

                    if (subcmd == null)
                        throw new CommandException($"Invalid subcommand! Try {RShockAPI.Config.CommandPrefix}{commandName} help for more info.");

                    helpText = subcmd.HelpDesc.Count() == 0 ? new List<string>() { subcmd.HelpText } : subcmd.HelpDesc.ToList();
                }
                else
                {
                    helpText = GetHelpText(args).ToList();
                }

                if (helpText.Count == 0)
                {
                    Utils.ShowInfoMessage("The command does not have any help description!");
                    return;
                }

                if (!PaginationTools.TryParsePageNumber(args.Parameters, pageParamIndex, out pageNumber))
                    return;

                PaginationTools.SendPage(pageNumber, helpText,
                  new PaginationTools.Settings
                  {
                      HeaderFormat = "Commands ({0}/{1}):",
                      FooterFormat = $"Type /{Names.First()} help {{0}} for more. \nType /{Names.First()} help <command name> for command description.",
                      MaxLinesPerPage = 4
                  });
            }

            public Command ToCommand()
            {
                var command = new Command(GroupCommandHandler, Names);
                 
                if (Subcommands.Length == 0)
                {
                    command = new Command(SingleCommandHandler, Names);
                }

                command.DoLog = DoLog;
                command.HelpText = HelpText;
                command.HelpDesc = HelpDesc;
                return command;
            }
        }
    }
}
