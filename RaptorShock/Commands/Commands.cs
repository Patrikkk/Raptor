using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Localization;

namespace RaptorShock.CommandManager
{
    public static class Commands
    {
        public static List<Command> ChatCommands = new List<Command>();

        public static string Specifier
        {
            get { return string.IsNullOrWhiteSpace(RShockAPI.Config.CommandPrefix) ? "/" : RShockAPI.Config.CommandPrefix; }
        }

        private delegate void AddChatCommand(CommandDelegate command, params string[] names);

        public static void InitCommands()
        {
            ChatCommands.AddRange(CommandFactory.GetCommands(typeof(RShockAPI).Assembly));
        }

        public static bool HandleCommand(string text)
        {
            string cmdText = text.Remove(0, 1);
            string cmdPrefix = text[0].ToString();

            int index = -1;
            for (int i = 0; i < cmdText.Length; i++)
            {
                if (IsWhiteSpace(cmdText[i]))
                {
                    index = i;
                    break;
                }
            }
            string cmdName;
            if (index == 0) // Space after the command specifier should not be supported
            {
                Utils.ShowErrorMessage($"Invalid command entered. Type {RShockAPI.Config.CommandPrefix}help for a list of valid commands.");
                return true;
            }
            else if (index < 0)
                cmdName = cmdText.ToLower();
            else
                cmdName = cmdText.Substring(0, index).ToLower();

            List<string> args;
            if (index < 0)
                args = new List<string>();
            else
                args = ParseParameters(cmdText.Substring(index));

            IEnumerable<Command> cmds = ChatCommands.FindAll(c => c.HasAlias(cmdName));

            if (cmds.Count() == 0)
            {
                /*if (player.AwaitingResponse.ContainsKey(cmdName))
                {
                    Action<CommandArgs> call = player.AwaitingResponse[cmdName];
                    player.AwaitingResponse.Remove(cmdName);
                    call(new CommandArgs(cmdText, player, args));
                    return true;
                }*/
                Utils.ShowErrorMessage($"Invalid command entered. Type {RShockAPI.Config.CommandPrefix}help for a list of valid commands.");
                return true;
            }
            foreach (Command cmd in cmds)
            {
                if (cmd.DoLog)
                    RShockAPI.Log.Info(string.Format("{0} executed: {1}.", Utils.LocalPlayer.name, cmdText));
                cmd.Run(cmdText, args);
            }
            return true;


        }

        /// <summary>
		/// Parses a string of parameters into a list. Handles quotes.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		private static List<String> ParseParameters(string str)
        {
            var ret = new List<string>();
            var sb = new StringBuilder();
            bool instr = false;
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];

                if (c == '\\' && ++i < str.Length)
                {
                    if (str[i] != '"' && str[i] != ' ' && str[i] != '\\')
                        sb.Append('\\');
                    sb.Append(str[i]);
                }
                else if (c == '"')
                {
                    instr = !instr;
                    if (!instr)
                    {
                        ret.Add(sb.ToString());
                        sb.Clear();
                    }
                    else if (sb.Length > 0)
                    {
                        ret.Add(sb.ToString());
                        sb.Clear();
                    }
                }
                else if (IsWhiteSpace(c) && !instr)
                {
                    if (sb.Length > 0)
                    {
                        ret.Add(sb.ToString());
                        sb.Clear();
                    }
                }
                else
                    sb.Append(c);
            }
            if (sb.Length > 0)
                ret.Add(sb.ToString());

            return ret;
        }
        private static bool IsWhiteSpace(char c)
        {
            return c == ' ' || c == '\t' || c == '\n';
        }


        [Command("anitime","at")]
        [CommandHelp(".anitime <animation-time>")]
        [CommandDescription("Sets your selected item's animation time.")]
        public static void AniTime(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
                throw new CommandException("Invalid syntax! Syntax: .anitime <animation-time>");
            if (!int.TryParse(args.Parameters[0], out int animationTime))
                throw new CommandException($"Invalid animation time '{args.Parameters[0]}'!");

            Utils.LocalPlayerItem.useAnimation = animationTime;
            Utils.ShowSuccessMessage($"Set animation time to '{animationTime}'.");
        }

        [Command("autoreuse", "ar")]
        [CommandHelp(".autoreuse")]
        [CommandDescription("Toggles your selected item's autoreuse.")]
        public static void AutoReuse(CommandArgs args)
        {
            Utils.LocalPlayerItem.autoReuse = !Utils.LocalPlayerItem.autoReuse;
            Utils.ShowSuccessMessage($"{(Utils.LocalPlayerItem.autoReuse ? "En" : "Dis")}abled autoreuse.");
        }

        [Command("createtile")]
        [CommandHelp(".createtile <createTile>")]
        [CommandDescription("Sets your selected item's createTile.")]
        public static void CreateTile(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
                throw new CommandException("Invalid syntax! Syntax: .createtile <createtile>");
            if (!int.TryParse(args.Parameters[0], out int createTile))
                throw new CommandException($"Invalid createtile '{args.Parameters[0]}'!");

            Utils.LocalPlayerItem.createTile = createTile;
            Utils.ShowSuccessMessage($"Set createTile to '{createTile}'.");
        }

        [Command("damage")]
        [CommandHelp(".damage <damage>")]
        [CommandDescription("Sets your selected item's damage.")]
        public static void Damage(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
                throw new CommandException("Invalid syntax! Syntax: .damage <damage>");
            if (!int.TryParse(args.Parameters[0], out int damage))
                throw new CommandException($"Invalid damage '{args.Parameters[0]}'!");

            if (damage <= 0)
                throw new CommandException($"Invalid defense  damage '{damage}'.");

            Utils.LocalPlayerItem.damage = damage;
            Utils.ShowSuccessMessage($"Set damage to '{damage}'.");
        }

        [Command("defense")]
        [CommandHelp(".defense <defense>")]
        [CommandDescription("Sets your defense.")]
        public static void Defense(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
                throw new CommandException("Invalid syntax! Syntax: .defense <defense>");
            if (!int.TryParse(args.Parameters[0], out int defense))
                throw new CommandException($"Invalid defense '{args.Parameters[0]}'!");

            if (defense <= 0)
                throw new CommandException($"Invalid defense '{defense}'.");


            PlayerExtension.DefenseValue = defense;
            Utils.ShowSuccessMessage($"Set defense to '{defense}'.");
        }

        [Command("fullbright", "fb")]
        [CommandHelp(".fullbright")]
        [CommandDescription("Toggles fullbright mode")]
        public static void FullBright(CommandArgs args)
        {
            PlayerExtension.IsFullBright = !PlayerExtension.IsFullBright;
            Utils.ShowSuccessMessage($"{(PlayerExtension.IsFullBright ? "En" : "Dis")}abled fullbright mode.");
        }

        [Command("godmode")]
        [CommandHelp(".godmode")]
        [CommandDescription("Toggles god mode.")]
        public static void GodMode(CommandArgs args)
        {
            PlayerExtension.IsGodMode = !PlayerExtension.IsGodMode;
            Utils.ShowSuccessMessage($"{(PlayerExtension.IsGodMode ? "En" : "Dis")}abled god mode.");
        }

        [Command("help")]
        [CommandHelp(".help [command-name]")]
        [CommandDescription("Provides help about a command.")]
        public static void Help(CommandArgs args)
        {
            var commands = CommandManager.Commands.ChatCommands;
            if (args.Parameters.Count < 1)
            {
                Utils.ShowSuccessMessage("Available commands:");
                Utils.ShowInfoMessage(string.Join(", ", commands.Select(c => c.Name)));
                return;
            }
            string commandName = args.Parameters[0];

            var command = commands.FirstOrDefault(c => c.Names.Contains(commandName));
            if (command == null)
                throw new CommandException($"Invalid command '{commandName}'.");

            Utils.ShowSuccessMessage($"{command.Name} help:");
            if (command.Names.Count > 1)
            {
                Utils.ShowInfoMessage($"Alias: {string.Join(", ", command.Names)}");
            }
            Utils.ShowInfoMessage($"Syntax: {command.HelpText}");
            Utils.ShowInfoMessage(command.HelpDesc.Length == 0 ? "No help text available." : string.Join("\n", command.HelpDesc));
        }

        [Command("reload")]
        [CommandHelp(".reload")]
        [CommandDescription("Reloads RaptorShock config.")]
        public static void Reload(CommandArgs args)
        {
            RShockAPI.Config = Config.Read(RShockAPI.ConfigPath);
            Utils.ShowSuccessMessage($"Reloaded RaptorShock config!");
        }

        [Command("infammo")]
        [CommandHelp(".infammo")]
        [CommandDescription("Toggles infinite ammo.")]
        public static void InfiniteAmmo(CommandArgs args)
        {
            PlayerExtension.IsInfiniteAmmo = !PlayerExtension.IsInfiniteAmmo;
            Utils.ShowSuccessMessage($"{(PlayerExtension.IsInfiniteAmmo ? "En" : "Dis")}abled infinite ammo.");
        }

        [Command("infbreath")]
        [CommandHelp(".infbreath")]
        [CommandDescription("Toggles infinite breath.")]
       
        public static void InfiniteBreath(CommandArgs args)
        {
            PlayerExtension.IsInfiniteBreath = !PlayerExtension.IsInfiniteBreath;
            Utils.ShowSuccessMessage($"{(PlayerExtension.IsInfiniteBreath ? "En" : "Dis")}abled infinite breath.");
        }

        [Command("infhealth")]
        [CommandHelp(".infhealth")]
        [CommandDescription("Toggles infinite health.")]
        public static void InfiniteHealth(CommandArgs args)
        {
            PlayerExtension.IsInfiniteHealth = !PlayerExtension.IsInfiniteHealth;
            Utils.ShowSuccessMessage($"{(PlayerExtension.IsInfiniteHealth ? "En" : "Dis")}abled infinite health.");
        }

        [Command("infmana")]
        [CommandHelp(".infmana")]
        [CommandDescription("Toggles infinite mana.")]
        public static void InfiniteMana(CommandArgs args)
        {
            PlayerExtension.IsInfiniteMana = !PlayerExtension.IsInfiniteMana;
            Utils.ShowSuccessMessage($"{(PlayerExtension.IsInfiniteMana ? "En" : "Dis")}abled infinite mana.");
        }
        [Command("infwings")]
        [CommandHelp(".infwings")]
        [CommandDescription("Toggles infinite wings.")]
        public static void InfiniteWings(CommandArgs args)
        {
            PlayerExtension.IsInfiniteWings = !PlayerExtension.IsInfiniteWings;
            Utils.ShowSuccessMessage($"{(PlayerExtension.IsInfiniteWings ? "En" : "Dis")}abled infinite wings.");
        }
        [Command("item", "i")]
        [CommandHelp(".item <item-name> [stack-size] [prefix]")]
        [CommandDescription("Spawns an item.")]
        public static void Item(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
                throw new CommandException("Invalid syntax! Syntax: .item <item-name> [stack-size] [prefix]");
            string itemName = args.Parameters[0];

            int amountParamIndex = -1;
            int itemAmount = 0;
            for (int i = 1; i < args.Parameters.Count; i++)
            {
                if (int.TryParse(args.Parameters[i], out itemAmount))
                {
                    amountParamIndex = i;
                    break;
                }
            }

            string itemNameOrId;
            if (amountParamIndex == -1)
                itemNameOrId = string.Join(" ", args.Parameters);
            else
                itemNameOrId = string.Join(" ", args.Parameters.Take(amountParamIndex));

            Item item;
            List<Item> matchedItems = Utils.GetItemsByNameOrId(itemNameOrId);
            if (matchedItems.Count == 0)
                throw new CommandException("Invalid item type!");
            else if (matchedItems.Count > 1)
                throw new CommandException($"Multiple matches found: {matchedItems.Select(i => $"{ i.Name }({ i.netID})")}");

            item = matchedItems[0];
            if (item.type < 1 && item.type >= Main.maxItemTypes)
                throw new CommandException($"The item type {itemNameOrId} is invalid.");

            int prefixId = 0;
            if (amountParamIndex != -1 && args.Parameters.Count > amountParamIndex + 1)
            {
                string prefixidOrName = args.Parameters[amountParamIndex + 1];
                int prefix = Utils.GetPrefixByNameOrId(prefixidOrName);

                if (prefix == -1)
                    throw new CommandException($"Did not find prefix \"{prefixidOrName}\"!");

                if (item.accessory && prefix == PrefixID.Quick)
                {
                    prefix = 76;
                }
                prefixId = prefix;
            }

            
            item.stack = itemAmount;
            item.position = Utils.LocalPlayer.Center;
            item.Prefix(prefixId);

            Utils.LocalPlayer.GetItem(Utils.LocalPlayer.whoAmI, item);
            Utils.ShowSuccessMessage($"Spawned {itemAmount} {item.Name}(s).");
        }
        [Command("noclip", "nc")]
        [CommandHelp(".noclip")]
        [CommandDescription("Toggles noclip.")]
        public static void Noclip(CommandArgs args)
        {
            PlayerExtension.IsNoclip = !PlayerExtension.IsNoclip;
            if (PlayerExtension.IsNoclip)
            {
                PlayerExtension.NoclipPosition = Utils.LocalPlayer.position;
            }
            Utils.ShowSuccessMessage($"{(PlayerExtension.IsNoclip ? "En" : "Dis")}abled noclip.");
        }
        [Command("projectile")]
        [CommandHelp(".projectile <projectile-name>")]
        [CommandDescription("Sets your selected item's projectile.")]
        public static void Projectile(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
                throw new CommandException("Syntax: .projectile <projectile-name>");
            string projectileName = args.Parameters[0];
            var matches = Utils.GetProjectilesByNameOrId(projectileName);

            if (matches.Count == 0)
                throw new CommandException("Invalid projectile type!");
            else if (matches.Count > 1)
                throw new CommandException($"Multiple matches found: {matches.Select(i => $"{ i.Name }({ i.type})")}");

            Utils.LocalPlayerItem.shoot = matches[0].type;
            Utils.ShowSuccessMessage($"Set projectile to {matches[0].Name}.");
        }
        [Command("range")]
        [CommandHelp(".range <range>")]
        [CommandDescription("Sets your range.")]
        public static void Range(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
                throw new CommandException("Syntax: .range <range>");

            if(!int.TryParse(args.Parameters[0], out int range) || range < 0)
                throw new CommandException($"Invalid range '{args.Parameters[0]}'.");

            PlayerExtension.RangeValue = range;
            Utils.ShowSuccessMessage($"Set range to '{range}'.");
        }

        [Command("reset")]
        [CommandHelp(".reset")]
        [CommandDescription("Resets all modified states.")]
        public static void Reset(CommandArgs args)
        {
            PlayerExtension.DefenseValue = null;
            PlayerExtension.IsFullBright = PlayerExtension.IsGodMode = PlayerExtension.IsInfiniteAmmo = 
                PlayerExtension.IsInfiniteBreath = PlayerExtension.IsInfiniteHealth = PlayerExtension.IsInfiniteMana =
                PlayerExtension.IsInfiniteWings = PlayerExtension.IsNoclip = false;
            PlayerExtension.RangeValue = null;
            PlayerExtension.SpeedValue = null;
            Utils.ShowSuccessMessage("Reset everything.");
        }
        [Command("shootspeed","ss" )]
        [CommandHelp(".shootspeed <speed>")]
        [CommandDescription("Sets your selected item's projectile speed.")]
        public static void ShootSpeed(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
                throw new CommandException("Syntax: .shootspeed <speed>");

            if (!float.TryParse(args.Parameters[0], out float shootSpeed) || shootSpeed < 0.0f)
                throw new CommandException($"Invalid shoot speed '{args.Parameters[0]}'.");

            Utils.LocalPlayerItem.shootSpeed = shootSpeed;
            Utils.ShowSuccessMessage($"Set shoot speed to '{shootSpeed}'.");
        }
        [Command("speed")]
        [CommandHelp(".speed <speed>")]
        [CommandDescription("Sets your speed value.")]
        public static void Speed(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
                throw new CommandException("Syntax: .speed <speed>");

            if (!float.TryParse(args.Parameters[0], out float speed) || speed < 0.0f)
                throw new CommandException($"Invalid speed '{args.Parameters[0]}'.");
            
            PlayerExtension.SpeedValue = speed;
            Utils.ShowSuccessMessage($"Set speed to '{speed}'.");
        }

        [Command("usetime", "ut")]
        [CommandHelp(".usetime <time>")]
        [CommandDescription("Sets your selected item's use time.")]
        public static void UseTime(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
                throw new CommandException("Syntax: .usetime <time>");

            if (!int.TryParse(args.Parameters[0], out int useTime) || useTime <= 0)
                throw new CommandException($"Invalid use time '{args.Parameters[0]}'.");

            Utils.LocalPlayerItem.useTime = useTime;
            Utils.ShowSuccessMessage($"Set use time to '{useTime}'.");
        }

        [Command("say")]
        [CommandHelp(".say <message>")]
        [CommandDescription("Sends chat message to the server.")]
        public static void Say(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
                throw new CommandException("Syntax: .say <message>");
            string message = string.Join(" ", args.Parameters);
            
            if (Main.netMode == 1)
            {
                NetMessage.SendData(25, -1, -1, NetworkText.FromLiteral(message), 0, 0f, 0f, 0f, 0, 0, 0);
                return;
            }
            Utils.ShowMessage(message, new byte[] { 255, 255, 255 });
        }
    }
}
