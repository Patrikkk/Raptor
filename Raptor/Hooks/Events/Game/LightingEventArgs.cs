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
        internal LightingEventArgs(float globalBrightness)
        {
            GlobalBrightness = globalBrightness;
        }

        /// <summary>
        ///  Gets the global brightness value.
        /// </summary>
        [CLSCompliant(false)]
        [NotNull]
        public float GlobalBrightness { get; }
    }
#endif
}
