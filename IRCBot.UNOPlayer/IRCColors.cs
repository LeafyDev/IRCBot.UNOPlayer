﻿// -----------------------------------------------------------
// This program is private software, based on C# source code.
// To sell or change credits of this software is forbidden,
// except if someone approves it from the LeafyCoding INC. team.
// -----------------------------------------------------------
// Copyrights (c) 2016 IRCBot.UNOPlayer INC. All rights reserved.
// -----------------------------------------------------------

namespace IRCBot.UNOPlayer
{
    internal static class IRC
    {
        // ReSharper disable UnusedMember.Global

        public const char NORMAL = (char) 15;
        public const char BOLD = (char) 2;
        public const char UNDERLINE = (char) 31;
        public const char REVERSE = (char) 22;
        public const char ITALIC = (char) 29;
        public static readonly string NOCOLOR = ((char) 3).ToString();
        public static readonly string WHITE = (char) 3 + "00";
        public static readonly string BLACK = (char) 3 + "01";
        public static readonly string DARK_BLUE = (char) 3 + "02";
        public static readonly string DARK_GREEN = (char) 3 + "03";
        public static readonly string RED = (char) 3 + "04";
        public static readonly string BROWN = (char) 3 + "05";
        public static readonly string PURPLE = (char) 3 + "06";
        public static readonly string OLIVE = (char) 3 + "07";
        public static readonly string YELLOW = (char) 3 + "08";
        public static readonly string GREEN = (char) 3 + "09";
        public static readonly string TEAL = (char) 3 + "10";
        public static readonly string CYAN = (char) 3 + "11";
        public static readonly string BLUE = (char) 3 + "12";
        public static readonly string MAGENTA = (char) 3 + "13";
        public static readonly string DARK_GRAY = (char) 3 + "14";
        public static readonly string LIGHT_GRAY = (char) 3 + "15";

        // ReSharper restore UnusedMember.Global
    }
}