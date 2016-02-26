// -----------------------------------------------------------
// This program is private software, based on C# source code.
// To sell or change credits of this software is forbidden,
// except if someone approves it from the LeafyCoding INC. team.
// -----------------------------------------------------------
// Copyrights (c) 2016 IRCBot.UNOPlayer INC. All rights reserved.
// -----------------------------------------------------------

#region

using System;
using System.IO;
using Nini.Config;

#endregion

namespace IRCBot.UNOPlayer
{
    internal static class Config
    {
        private static IniConfigSource ConfigFile;

        public static string IRC_BotName = string.Empty;
        public static string IRC_ChannelName = string.Empty;
        public static string IRC_Server = string.Empty;
        public static string IRC_Password = string.Empty;
        public static string IRC_NSPassword = string.Empty;
        public static bool IRC_SSL;

        public static bool Init()
        {
            try
            {
                ConfigFile = new IniConfigSource("config.ini");
                Populate();
                return true;
            }
            catch (Exception ex)
            {
                Program.ColoredWrite(ConsoleColor.Red, $"{ex.GetType().Name}: {ex.Message}");

                if (CreateConfig())
                {
                    Program.ColoredWrite(ConsoleColor.Red,
                        "A new config file has been created, please edit it and re-run the program.");
                    Console.ReadKey();
                }
                return false;
            }
        }

        private static bool CreateConfig()
        {
            try
            {
                var NewConfig = ";config.ini" + Environment.NewLine +
                                "[IRC_Options]" + Environment.NewLine +
                                "BotName = " + Environment.NewLine +
                                "ChannelName = " + Environment.NewLine +
                                "Server = " + Environment.NewLine +
                                "Password = " + Environment.NewLine +
                                "SSL = " + Environment.NewLine +
                                "NSPassword = ";

                File.WriteAllText(@"config.ini", NewConfig);
                return true;
            }
            catch (Exception ex)
            {
                Program.ColoredWrite(ConsoleColor.DarkRed, $"{ex.GetType().Name}: {ex.Message}");
                Program.ColoredWrite(ConsoleColor.Red,
                    "A config file could not be written, check current directory permissions.");
                Console.ReadKey();
                return false;
            }
        }

        private static void Populate()
        {
            try
            {
                IRC_BotName = ConfigFile.Configs["IRC_Options"].Get("BotName");
                IRC_ChannelName = ConfigFile.Configs["IRC_Options"].Get("ChannelName");
                IRC_Server = ConfigFile.Configs["IRC_Options"].Get("Server");
                IRC_Password = ConfigFile.Configs["IRC_Options"].Get("Password");
                IRC_NSPassword = ConfigFile.Configs["IRC_Options"].Get("NSPassword");
                IRC_SSL = ConfigFile.Configs["IRC_Options"].GetBoolean("SSL");
            }
            catch (Exception ex)
            {
                Program.ColoredWrite(ConsoleColor.DarkRed, $"{ex.GetType().Name}: {ex.Message}");
                Program.ColoredWrite(ConsoleColor.Red, "An error was encountered while parsing the config file.");
                Console.ReadKey();
                Environment.Exit(1);
            }
        }
    }
}