using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Raptor.Hooks.Events.Net
{
	/// <summary>
	/// Provides data for the <see cref="NetHooks.SentBytes" /> event.
	/// </summary>
	[PublicAPI]
	public class SentBytesEventArgs : HandledEventArgs
	{
		internal SentBytesEventArgs(byte[] buffer, int offset, int count)
		{
			Buffer = buffer;
			Offset = offset;
			Count = count;
		}

		/// <summary>
		/// The byte data array that is being sent.
		/// </summary>
		public byte[] Buffer
		{
			get;
			internal set;
		}
		/// <summary>
		/// The offset starting point of where the data should be read from.
		/// </summary>
		public int Offset
		{
			get;
			internal set;
		}
		/// <summary>
		/// The count of bytes the packet contains.
		/// </summary>
		public int Count
		{
			get;
			internal set;
		}
	}
}
