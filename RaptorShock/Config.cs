using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace RaptorShock
{
    /// <summary>
    ///     Represents a configuration.
    /// </summary>
    [PublicAPI]
    public class Config
    {
        /// <summary>
        ///     Gets or sets a value indicating whether to show the splash screen.
        /// </summary>
        public bool ShowSplashScreen { get; set; } = true;
        /// <summary>
        ///     Sets the command prefix that the API accepts.
        /// </summary>
        public string CommandPrefix { get; set; } = ".";
        /// <summary>
        /// Speed of noclip movement when holding shift.
        /// </summary>
        public int NoclipSpeedBoost { get; set; } = 2;

        /// <summary>
        /// List of hotkeys. 
        /// </summary>
        public Dictionary<string, List<string>> HotKeys { get; set; } = new Dictionary<string, List<string>>() 
        {
            { "F", new List<string>() { ".goto" } }
        };


        /// <summary>
        /// Reads a configuration file from a given path
        /// </summary>
        /// <param name="path">string path</param>
        /// <returns>ConfigFile object</returns>
        public static Config Read(string path)
        {
            if (!File.Exists(path))
                return new Config();
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Read(fs);
            }
        }

        /// <summary>
        /// Reads the configuration file from a stream
        /// </summary>
        /// <param name="stream">stream</param>
        /// <returns>ConfigFile object</returns>
        public static Config Read(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                var cf = JsonConvert.DeserializeObject<Config>(sr.ReadToEnd());
                if (ConfigRead != null)
                    ConfigRead(cf);
                return cf;
            }
        }

        /// <summary>
		/// Writes the configuration to a given path
		/// </summary>
		/// <param name="path">string path - Location to put the config file</param>
		public void Write(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                Write(fs);
            }
        }

        /// <summary>
        /// Writes the configuration to a stream
        /// </summary>
        /// <param name="stream">stream</param>
        public void Write(Stream stream)
        {
            var str = JsonConvert.SerializeObject(this, Formatting.Indented);
            using (var sw = new StreamWriter(stream))
            {
                sw.Write(str);
            }
        }

        /// <summary>
        /// On config read hook
        /// </summary>
        public static Action<Config> ConfigRead;
    }
}
