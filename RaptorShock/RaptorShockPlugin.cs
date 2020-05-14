using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using Raptor.Api;
using Raptor.Hooks;
using Raptor.Hooks.Events.Game;
using Raptor.Hooks.Events.Player;
using RaptorShock.Commands;
using Terraria;
using Terraria.Chat;

namespace RaptorShock
{
    /// <summary>
    ///     Represents the RaptorShock plugin.
    /// </summary>
    [ApiVersion(1, 0)]
    public class RShockAPI : TerrariaPlugin
    {
        #region Info
        /// <inheritdoc />
        public override string Author => "MarioE";

        /// <inheritdoc />
        public override string Description => "Provides a command API.";

        /// <inheritdoc />
        public override string Name => "RaptorShock";

        /// <inheritdoc />
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        #endregion

        /// <summary>
        /// The initial commands.
        /// </summary>
		public static readonly RaptorShockCommands Commands = new RaptorShockCommands();

        /// <summary>
        /// This is where RaptorShock saves data.
        /// </summary>
        public static string SavePath = "rshock";

        internal static readonly string ConfigPath = Path.Combine(SavePath, "raptorshock.json");

        /// <summary>
        /// Config of RaptorShock
        /// </summary>
        public static Config Config = new Config();
        public static ILog Log = LogManager.GetLogger("RaptorShock");

        private static KeyboardState _keyboard;
        private static KeyboardState _lastKeyboard;

        /// <summary>
        /// Checks if given key is being held down.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsKeyDown(Keys key) => _keyboard.IsKeyDown(key);
        /// <summary>
        /// Checks if given key was pressed.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsKeyTapped(Keys key) => _keyboard.IsKeyDown(key) && !_lastKeyboard.IsKeyDown(key);

        /// <summary>
        /// True if the client is pressing left or right Alt.
        /// </summary>
        public static bool IsAltDown => IsKeyDown(Keys.LeftAlt) || IsKeyDown(Keys.RightAlt);

        /// <summary>
        ///     Gets the command manager.
        /// </summary>
        [NotNull]
        public static CommandManager CommandManager { get; } = new CommandManager();

        /// <summary>
        ///     Initializes the <see cref="RShockAPI" /> class.
        /// </summary>
        [CLSCompliant(false)]
        public RShockAPI()
        {
            Order = 0;

           
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            Directory.CreateDirectory(SavePath);
            if (File.Exists(ConfigPath))
            {
                Config = Config.Read(ConfigPath);
            }

            Log.Info("Initialized RaptorShock.");
            CommandManager.AddParser(typeof(byte), s => byte.TryParse(s, out var result) ? (object)result : null);
            CommandManager.AddParser(typeof(float), s => float.TryParse(s, out var result) ? (object)result : null);
            CommandManager.AddParser(typeof(int), s => int.TryParse(s, out var result) ? (object)result : null);
            CommandManager.AddParser(typeof(Item), s =>
            {
                var items = Utils.GetItemsByNameOrId(s);
                return items.Count == 1 ? items[0] : null;
            });
            CommandManager.AddParser(typeof(Projectile), s =>
            {
                var projectiles = Utils.GetProjectilesByNameOrId(s);
                return projectiles.Count == 1 ? projectiles[0] : null;
            });
            CommandManager.AddParser(typeof(string), s => s);
            CommandManager.Register(Commands);

            GameHooks.Initialized += OnGameInitialized;
            GameHooks.Lighting += OnGameLighting;
            GameHooks.Update += OnGameUpdate;
            PlayerHooks.Hurting += OnPlayerHurting;
            PlayerHooks.Kill += OnPlayerKill;
            PlayerHooks.Updated2 += OnPlayerUpdated2;
            PlayerHooks.Updated += OnPlayerUpdated;
        }


        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(Config, Formatting.Indented));

                GameHooks.Initialized -= OnGameInitialized;
                GameHooks.Lighting -= OnGameLighting;
                GameHooks.Update -= OnGameUpdate;
                PlayerHooks.Hurting -= OnPlayerHurting;
                PlayerHooks.Kill -= OnPlayerKill;
                PlayerHooks.Updated2 -= OnPlayerUpdated2;
                PlayerHooks.Updated -= OnPlayerUpdated;
            }

            base.Dispose(disposing);
        }

        private void OnGameInitialized(object sender, EventArgs e)
        {
            Main.showSplash = Config.ShowSplashScreen;
            Utils.InitializeNames();
        }

        private void OnGameLighting(object sender, LightingEventArgs e)
        {
            if (Commands.IsFullBright)
            {
                e.SwipeData.function = lsd =>
                {
                    foreach (var state in lsd.jaggedArray.SelectMany(s => s))
                    {
                        state.r = state.r2 = state.g = state.g2 = state.b = state.b2 = 1;
                    }
                };
            }
        }

        private void OnGameUpdate(object sender, HandledEventArgs e)
        {
            if (!Main.hasFocus)
            {
                return;
            }

            _lastKeyboard = _keyboard;
            _keyboard = Keyboard.GetState();
            Main.chatRelease = false;

            // Don't misinterpret key presses in menus.
            if (Main.gameMenu || Main.editChest || Main.editSign)
            {
                return;
            }

            if (IsKeyTapped(Keys.Enter) && !IsAltDown)
            {
                Main.drawingPlayerChat = !Main.drawingPlayerChat;
                if (Main.drawingPlayerChat)
                {
                    Main.PlaySound(10);
                }
                else
                {
                    var chatText = Main.chatText;
                    if (chatText.StartsWith(Config.CommandPrefix) && !chatText.StartsWith(Config.CommandPrefix + Config.CommandPrefix))
                    {
                        try
                        {
                            CommandManager.Run(chatText.Substring(1));
                            Log.Info($"Executed '{chatText}'.");
                        }
                        catch (FormatException ex)
                        {
                            Utils.ShowErrorMessage(ex.Message);
                        }
                        catch (Exception ex)
                        {
                            Utils.ShowErrorMessage("An exception occurred. Check the log for more details.");
                            Log.Error($"An exception occurred while running the command '{chatText}':");
                            Log.Error(ex);
                        }
                    }
                    else if (!string.IsNullOrEmpty(chatText))
                    {
                        if (chatText.StartsWith(Config.CommandPrefix))
                        {
                            chatText = chatText.Substring(1);
                        }

                        if (Main.netMode == 0)
                        {
                            Main.NewText($"<{Utils.LocalPlayer.name}> {chatText}");
                        }
                        else
                        {
                            NetMessage.SendChatMessageFromClient(new ChatMessage(chatText));
                        }
                    }

                    Main.chatText = "";
                    Main.PlaySound(11);
                }
            }
            if (IsKeyTapped(Keys.Escape) && Main.drawingPlayerChat)
            {
                Main.chatText = "";
                Main.PlaySound(11);
            }
        }

        private void OnPlayerHurting(object sender, HurtingEventArgs e)
        {
            if (e.IsLocal && Commands.IsGodMode)
            {
                e.Handled = true;
            }
        }

        private void OnPlayerKill(object sender, KillEventArgs e)
        {
            if (e.IsLocal && Commands.IsGodMode)
            {
                e.Handled = true;
            }
        }

        private void OnPlayerUpdated(object sender, UpdatedEventArgs e)
        {
            if (e.IsLocal)
            {
                var player = e.Player;
                if (Commands.IsNoclip)
                {
                    float movespeed = IsKeyDown(Keys.LeftShift) ? player.moveSpeed * Config.NoclipSpeedBoost : player.moveSpeed;
                    if (player.controlLeft)
                    {
                        Commands.NoclipPosition += new Vector2(-movespeed, 0);
                    }
                    if (player.controlRight)
                    {
                        Commands.NoclipPosition += new Vector2(movespeed, 0);
                    }
                    if (player.controlUp)
                    {
                        Commands.NoclipPosition += new Vector2(0, -movespeed);
                    }
                    if (player.controlDown)
                    {
                        Commands.NoclipPosition += new Vector2(0, movespeed);
                    }
                    player.gfxOffY = 0;
                    player.position = Commands.NoclipPosition;
                }
            }
        }

        private void OnPlayerUpdated2(object sender, UpdatedEventArgs e)
        {
            if (e.IsLocal)
            {
                var player = e.Player;
                if (Commands.DefenseValue != null)
                {
                    player.statDefense = Commands.DefenseValue.Value;
                }
                if (Commands.IsInfiniteAmmo)
                {
                    foreach (var item in player.inventory)
                    {
                        if (item.ammo != 0 && !item.notAmmo)
                        {
                            item.stack = item.maxStack;
                        }
                    }
                }
                if (Commands.IsInfiniteBreath || Commands.IsGodMode)
                {
                    player.breath = player.breathMax - 1;
                }
                if (Commands.IsInfiniteHealth || Commands.IsGodMode)
                {
                    player.statLife = player.statLifeMax2;
                }
                if (Commands.IsInfiniteMana || Commands.IsGodMode)
                {
                    player.statMana = player.statManaMax2;
                }
                if (Commands.IsInfiniteWings)
                {
                    player.wingTime = 2;
                }
                if (Commands.RangeValue != null)
                {
                    var range = Commands.RangeValue.Value;
                    Player.tileRangeX = range;
                    Player.tileRangeY = range;
                }
                if (Commands.SpeedValue != null)
                {
                    var speed = (float)Commands.SpeedValue;
                    player.maxRunSpeed = speed;
                    player.moveSpeed = speed;
                    player.runAcceleration = speed / 12.5f;
                }
            }
        }

       
    }
}
