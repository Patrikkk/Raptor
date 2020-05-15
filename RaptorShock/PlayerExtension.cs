using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaptorShock
{
    public static class PlayerExtension
    {
        public static int? RangeValue { get; set; }
        public static float? SpeedValue { get; set; }
        public static int? DefenseValue { get; set; }
        public static bool IsFullBright { get; set; }
        public static bool IsGodMode { get; set; }
        public static bool IsInfiniteAmmo { get; set; }
        public static bool IsInfiniteBreath { get; set; }
        public static bool IsInfiniteHealth { get; set; }
        public static bool IsInfiniteMana { get; set; }
        public static bool IsInfiniteWings { get; set; }
        public static bool IsNoclip { get; set; }
        public static Vector2 NoclipPosition { get; set; }
    }
}
