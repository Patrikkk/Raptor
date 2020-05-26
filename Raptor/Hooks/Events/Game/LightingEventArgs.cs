using System;
using System.ComponentModel;
using JetBrains.Annotations;
using Terraria;
using Terraria.Graphics.Light;

namespace Raptor.Hooks.Events.Game
{
#if DEBUG
	/// <summary>
	///     Provides data for the <see cref="GameHooks.Lighting" /> event.
	/// </summary>
	[PublicAPI]
    public sealed class LightingEventArgs : HandledEventArgs
    {
        internal LightingEventArgs(ILightingEngine engine)
        {
            LightingEngine = engine;
        }

        /// <summary>
        ///     Gets the swipe data.
        /// </summary>
        [CLSCompliant(false)]
        [NotNull]
        public static ILightingEngine LightingEngine;
    }
#endif
}
