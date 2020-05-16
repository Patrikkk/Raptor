using JetBrains.Annotations;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Raptor.Hooks.Events.Player
{
    /// <summary>
    ///     Provides data for the <see cref="PlayerHooks.KeysPressed" /> event.
    /// </summary>
    [PublicAPI]
    public class KeyboardEventArgs : HandledEventArgs
    {
        /// <summary>
        /// State of they key that was pressed.
        /// </summary>
        public List<Keys> Keys { get; private set; }

        internal KeyboardEventArgs(List<Keys> keys)
        {
            Keys = keys;
        }
    }
}
