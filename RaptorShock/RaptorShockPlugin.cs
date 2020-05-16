using System;
using System.Collections.Generic;
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
        /// This is where RaptorShock saves data.
        /// </summary>
        public static string SavePath = "rshock";

        /// <summary>
        /// Direct path to the RaptorShock config.
        /// </summary>
        internal static readonly string ConfigPath = Path.Combine(SavePath, "raptorshock.json");

        /// <summary>
        /// Config of RaptorShock
        /// </summary>
        public static Config Config = new Config();
        public static ILog Log = LogManager.GetLogger("RaptorShock");

        private static List<Keys> keyCombination = new List<Keys>();

        /// <summary>
        /// Checks if given key is being held down.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsKeyDown(Keys key) => Main.keyState.IsKeyDown(key);
        /// <summary>
        /// Checks if given key was pressed.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsKeyTapped(Keys key) => Main.keyState.IsKeyDown(key) && !Main.oldKeyState.IsKeyDown(key);

        /// <summary>
        /// True if the client is pressing left or right Alt.
        /// </summary>
        public static bool IsAltDown => IsKeyDown(Keys.LeftAlt) || IsKeyDown(Keys.RightAlt);

        /// <summary>
        /// Temporary fix.
        /// Storing our own bool because Main.drawingPlayerChat is updated before OnUpdate.
        /// Thus when Escape is pressed, Terraria already sets it to false.
        /// </summary>
        public static bool IsChatOpen { get; set; }

        #region Initialize
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
            if (!File.Exists(ConfigPath))
            {
                Config.Write(ConfigPath);
            }
            Config = Config.Read(ConfigPath);
            Log.Info("Initialized RaptorShock.");

            CommandManager.Commands.InitCommands();

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
                Config.Write(ConfigPath);

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
        #endregion

        #region Hooks
        private void OnGameInitialized(object sender, EventArgs e)
        {
            Main.showSplash = Config.ShowSplashScreen;
            Utils.InitializeNames();
            PlayerHooks.KeysPressed += PlayerHooks_KeyPressed;
            PlayerHooks.KeysTapped += ProcessHotkeys;
        }

        private void ProcessHotkeys(object sender, KeyboardEventArgs e)
        {
            //Utils.ShowInfoMessage($"Tapped {string.Join(", ", e.Keys.Select(k => k.ToString()))}");

            if (e.Handled)
                return;

            if (!IsChatOpen)
            {
                string symbol = "";
                string keys = string.Join(" ", e.Keys);
                if (e.Keys.Count > 1 && Utils.ModifierSymbols.ContainsKey(e.Keys.Last()))
                {
                    symbol = Utils.ModifierSymbols[e.Keys.Last()];
                    keys = string.Join(" ", e.Keys.Remove(e.Keys.Last()));
                }

                if (Config.HotKeys.ContainsKey(symbol + keys))
                {
                    List<string> lines = Config.HotKeys[symbol + keys];
                    foreach (string line in lines)
                        CommandManager.Commands.HandleCommand(line);
                    e.Handled = true;
                }
            }
        }

        private void PlayerHooks_KeyPressed(object sender, KeyboardEventArgs e)
        {
            //Utils.ShowInfoMessage($"Pressing {string.Join(", ", e.Keys.Select(k => k.ToString()))}");

            // Release
            if (e.Keys.Count == 0)
            {
                if (keyCombination.Count > 1)
                {
                    PlayerHooks.InvokeKeysTapped(keyCombination);
                    keyCombination = new List<Keys>();
                }
                else
                {
                    Keys[] oldPressedKeys = Main.oldKeyState.GetPressedKeys();
                    if (oldPressedKeys.Length == 1)
                    {
                        PlayerHooks.InvokeKeysTapped(oldPressedKeys.ToList());
                    }
                }
            }
            else if (e.Keys.Count > 1 && e.Keys.Count > keyCombination.Count)
            {
                keyCombination = e.Keys;
            }
        }

        private void OnGameLighting(object sender, LightingEventArgs e)
        {
            if (PlayerExtension.IsFullBright)
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

            Main.chatRelease = false;

            // Don't misinterpret key presses in menus.
            if (Main.gameMenu || Main.editChest || Main.editSign)
            {
                return;
            }

            if (Main.oldKeyState != Main.keyState)
                PlayerHooks.InvokeKeysPressed(Main.keyState.GetPressedKeys().ToList());

            if (IsKeyTapped(Keys.Enter) && !IsAltDown)
            {
                if (!IsChatOpen)
                {
                    Main.drawingPlayerChat = true;
                    IsChatOpen = true;
                    Main.PlaySound(10);
                }
                else
                {
                    var chatText = Main.chatText;
                    if (chatText.StartsWith(Config.CommandPrefix) && !chatText.StartsWith(Config.CommandPrefix + Config.CommandPrefix))
                    {
                        try
                        {
                            CommandManager.Commands.HandleCommand(chatText);
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
                    IsChatOpen = false;
                    Main.drawingPlayerChat = false;
                    Main.PlaySound(11);
                }
            }
            else if (IsKeyTapped(Keys.Escape))
            {
                if (IsChatOpen)
                {
                    Main.drawingPlayerChat = false;
                    IsChatOpen = false;
                    Main.chatText = "";
                    Main.PlaySound(11);
                }
            }
        }

        private void OnPlayerHurting(object sender, HurtingEventArgs e)
        {
            if (e.IsLocal && PlayerExtension.IsGodMode)
            {
                e.Handled = true;
            }
        }

        private void OnPlayerKill(object sender, KillEventArgs e)
        {
            if (e.IsLocal && PlayerExtension.IsGodMode)
            {
                e.Handled = true;
            }
        }

        private void OnPlayerUpdated(object sender, UpdatedEventArgs e)
        {
            if (e.IsLocal)
            {
                var player = e.Player;
                if (PlayerExtension.IsNoclip)
                {
                    float movespeed = IsKeyDown(Keys.LeftShift) ? player.moveSpeed * Config.NoclipBoostSpeed : player.moveSpeed * 5;
                    if (player.controlLeft)
                    {
                        PlayerExtension.NoclipPosition += new Vector2(-movespeed, 0);
                    }
                    if (player.controlRight)
                    {
                        PlayerExtension.NoclipPosition += new Vector2(movespeed, 0);
                    }
                    if (player.controlUp)
                    {
                        PlayerExtension.NoclipPosition += new Vector2(0, -movespeed);
                    }
                    if (player.controlDown)
                    {
                        PlayerExtension.NoclipPosition += new Vector2(0, movespeed);
                    }
                    player.gfxOffY = 0;
                    player.position = PlayerExtension.NoclipPosition;
                }
            }
        }

        private void OnPlayerUpdated2(object sender, UpdatedEventArgs e)
        {
            if (e.IsLocal)
            {
                var player = e.Player;
                if (PlayerExtension.DefenseValue != null)
                {
                    player.statDefense = PlayerExtension.DefenseValue.Value;
                }
                if (PlayerExtension.IsInfiniteAmmo)
                {
                    foreach (var item in player.inventory)
                    {
                        if (item.ammo != 0 && !item.notAmmo)
                        {
                            item.stack = item.maxStack;
                        }
                    }
                }
                if (PlayerExtension.IsInfiniteBreath || PlayerExtension.IsGodMode)
                {
                    player.breath = player.breathMax - 1;
                }
                if (PlayerExtension.IsInfiniteHealth || PlayerExtension.IsGodMode)
                {
                    player.statLife = player.statLifeMax2;
                }
                if (PlayerExtension.IsInfiniteMana || PlayerExtension.IsGodMode)
                {
                    player.statMana = player.statManaMax2;
                }
                if (PlayerExtension.IsInfiniteWings)
                {
                    player.wingTime = 2;
                }
                if (PlayerExtension.RangeValue != null)
                {
                    var range = PlayerExtension.RangeValue;
                    Player.tileRangeX = range.Value;
                    Player.tileRangeY = range.Value;
                }
                if (PlayerExtension.SpeedValue != null)
                {
                    var speed = (float)PlayerExtension.SpeedValue;
                    player.maxRunSpeed = speed;
                    player.moveSpeed = speed;
                    player.runAcceleration = speed / 12.5f;
                }
            }
        }
        #endregion

    }
}
