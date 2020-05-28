using System;
using System.ComponentModel;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Content;
using Raptor.Hooks.Events.Game;
using Terraria;
using Terraria.Graphics.Light;

namespace Raptor.Hooks
{
    /// <summary>
    ///     Holds the game hooks.
    /// </summary>
    [PublicAPI]
    public static class GameHooks
    {
        /// <summary>
        ///     Invoked when the game is initialized.
        /// </summary>
        public static event EventHandler Initialized;

#if DEBUG
		/// <summary>
		///     Invoked when lighting is occurring.
		/// </summary>
		public static event EventHandler<LightingEventArgs> Lighting;
#endif
        /// <summary>
        ///     Invoked when the game content is loaded.
        /// </summary>
        public static event EventHandler<LoadedContentEventArgs> LoadedContent;

        /// <summary>
        ///     Invoked when the game is updating.
        /// </summary>
        public static event EventHandler<HandledEventArgs> Update;

        /// <summary>
        ///     Invoked when the game is updated.
        /// </summary>
        public static event EventHandler Updated;

        internal static void InvokeInitialized()
        {
            Initialized?.Invoke(null, EventArgs.Empty);
        }

#if DEBUG
        internal static bool InvokeLighting(object globalBrightness)
        {
            var args = new LightingEventArgs((float)globalBrightness);
            Lighting?.Invoke(null, args);
            return args.Handled;
        }
#endif
		internal static void InvokeLoadedContent(ContentManager contentManager)
        {
            LoadedContent?.Invoke(null, new LoadedContentEventArgs(contentManager));
        }

        internal static bool InvokeUpdate()
        {
            var args = new HandledEventArgs();
            Update?.Invoke(null, args);
            return args.Handled;
        }

        internal static void InvokeUpdated()
        {
            Updated?.Invoke(null, EventArgs.Empty);
        }
    }
}
