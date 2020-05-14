using System;
using JetBrains.Annotations;

namespace Raptor.Api
{
    /// <summary>
    ///     Specifies a plugin.
    /// </summary>
    [PublicAPI]
    public abstract class TerrariaPlugin : IDisposable
    {

        /// <summary>
        ///     Gets the name.
        /// </summary>
        [NotNull]
        public virtual string Name => "";

        /// <summary>
        ///     Gets the author.
        /// </summary>
        [NotNull]
        public virtual string Author => "";

        /// <summary>
        ///     Gets the description.
        /// </summary>
        [NotNull]
        public virtual string Description => "";

        /// <summary>
        ///     Gets the version.
        /// </summary>
        [NotNull]
        public virtual Version Version => new Version(1, 0);

        ///<summary>
        /// The plugin's order. This represents load priority.
        /// Plugins are sorted first based on order, then on name where conflicts occur.
        /// A lower order represents a higher load priority - I.E., order 1 is loaded before order 5.
        /// This value may be negative.
        /// The default plugin constructor will set order to 1.
        ///</summary>
        public int Order
        {
            get;
            set;
        }

        /// <summary>
        /// Instantiates a new instance of the plugin.
        /// Sets <see cref="Order"/> to 1.
        /// </summary>
        protected TerrariaPlugin()
        {
            this.Order = 1;
        }

        /// <summary>
        ///     Destructs the plugin.
        /// </summary>
        ~TerrariaPlugin()
        {
            Dispose(false);
        }

        /// <summary>
        ///     Disposes the plugin.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Disposes the plugin.
        /// </summary>
        /// <param name="disposing"><c>true</c> to dispose managed resources; otherwise, <c>false</c>.</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        ///<summary>
        /// Invoked after the plugin is constructed. Initialization logic should occur here.
        ///</summary>
        public abstract void Initialize();
    }
}
