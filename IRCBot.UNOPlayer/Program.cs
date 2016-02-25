// -----------------------------------------------------------
// This program is private software, based on C# source code.
// To sell or change credits of this software is forbidden,
// except if someone approves it from the LeafyCoding INC. team.
// -----------------------------------------------------------
// Copyrights (c) 2016 IRCBot.UNOPlayer INC. All rights reserved.
// -----------------------------------------------------------

#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatSharp;
using ChatSharp.Events;

#endregion

namespace IRCBot.UNOPlayer
{
    internal static class Program
    {
        private const string IRC_BotName = "UNO_Rekt";
        private const string IRC_ChannelName = "#Seditio";
        private const string IRC_Server = "irc.rizon.net";
        private static IrcClient client;
        private static bool MenuEnabled;
        private static readonly string IRC_Password = string.Empty;
        private static readonly string IRC_NSPassword = string.Empty;
        private static string topNumber = string.Empty;
        private static string topColor = string.Empty;
        private static bool drawingCard;
        private static readonly List<string> CurrentCards = new List<string>();

        private static void Main()
        {
            Console.Title = $"{IRC_BotName} on {IRC_Server} in channel {IRC_ChannelName}.";
            ColoredWrite(ConsoleColor.Green, "*** Initialising " + Console.Title);

            SemiColoredWrite(ConsoleColor.Yellow, "[IRC] ", "Initialising IRC client.");
            client = new IrcClient(IRC_Server, new IrcUser(IRC_BotName, IRC_BotName, IRC_Password));

            client.ConnectAsync();
            SemiColoredWrite(ConsoleColor.Yellow, "[IRC] ", "Connected to IRC server.");

            SetupIRCClient();

            do
            {
                Thread.Sleep(100);
            } while (!MenuEnabled);

            Start();
        }

        private static void SetupIRCClient()
        {
            client.ConnectionComplete += (s, e) =>
            {
                if (!string.IsNullOrEmpty(IRC_NSPassword))
                {
                    SemiColoredWrite(ConsoleColor.Yellow, "[IRC] ", "Identifying to NickServ.");
                    client.SendMessage("identify " + IRC_NSPassword, "NickServ");
                    Thread.Sleep(200);
                    SemiColoredWrite(ConsoleColor.Yellow, "[IRC] ", "Enabling vHost.");
                    client.SendMessage("hs on", "HostServ");
                    Thread.Sleep(200);
                }

                SemiColoredWrite(ConsoleColor.Yellow, "[IRC] ", "Joining default channel.");
                client.JoinChannel(IRC_ChannelName);
                SemiColoredWrite(ConsoleColor.Yellow, "[IRC] ", "Joined channel, enabling menu.");
                Thread.Sleep(100);

                MenuEnabled = true;
            };

            client.ChannelMessageRecieved += (s, e) => { new Task(() => { HandleMSG(e); }).Start(); };

            client.NoticeRecieved += (s, e) => { new Task(() => { HandleNOTICE(e); }).Start(); };

            client.PrivateMessageRecieved += (s, e) =>
            {
                if (e.PrivateMessage.Message.Equals("\x01VERSION\x01"))
                {
                    var msg = $"\x01VERSION {IRC.NOCOLOR}💩 Leafy-IRCBot.UNO 💩\x01";
                    client.SendNotice(msg, e.PrivateMessage.User.Nick);
                    SemiColoredWrite(ConsoleColor.Magenta, "[CTCP:VERSION] ",
                        $"Responded to request from {e.PrivateMessage.User.Nick}");
                }
                if (e.PrivateMessage.Message.Equals("\x01TIME\x01"))
                {
                    var msg = $"\x01TIME {BuildCTCPTime()} \x01";
                    client.SendNotice(msg, e.PrivateMessage.User.Nick);
                    SemiColoredWrite(ConsoleColor.Magenta, "[CTCP:TIME] ",
                        $"Responded to request from {e.PrivateMessage.User.Nick}");
                }
            };
        }

        private static void HandleMSG(PrivateMessageEventArgs e)
        {
            var sender = e.PrivateMessage.User.Nick;
            var message = e.PrivateMessage.Message;
            var channel = e.PrivateMessage.Source;

            if (sender == "UNOBot")
            {
                if (message.EndsWith("- Type \".ujoin\" to join!"))
                {
                    client.SendMessage(".uj", channel);
                }
                if (message.StartsWith($"\u000f\u0002{IRC_BotName}'s\u0002 turn. Top Card:"))
                {
                    var top = message.Split(new[] {"Top Card: \u0002\u0003\u0003"}, 0)[1].Split(new[] {"\u0003"}, 0)[0];
                    top = top.Remove(0, 2);

                    var color = top.Split('(')[1].Split(')')[0];
                    var card = top.Split('[')[1].Split(']')[0];

                    Console.Title = $"TopCard is {color} {card}.";
                    topColor = color;
                    topNumber = card;
                }
                if (message.Contains("We have a winner!") && message.Contains(IRC_BotName))
                {
                    SemiColoredWrite(ConsoleColor.Green, "[UNO] ", "Bot won the game.");
                    Thread.Sleep(1000);
                    client.SendMessage($"{IRC.BOLD}You just got rekted. ( ͡° ͜ʖ ͡°)", channel);
                }
                if (message.Contains("We have a winner!") && !message.Contains(IRC_BotName))
                {
                    SemiColoredWrite(ConsoleColor.Green, "[UNO] ", "Bot lost the game.");
                    Thread.Sleep(1000);
                    client.SendMessage($"{IRC.BOLD}I just got rekted. :(", channel);
                }
            }
        }

        private static void HandleNOTICE(IrcNoticeEventArgs e)
        {
            var message = e.Notice;
            var sender = e.Source.Split('!')[0];

            if (sender == "UNOBot")
            {
                if (!message.Contains("Next:") && !message.Contains("Cards:"))
                {
                    var splitter = message.Split('\u0003');

                    foreach (var s in splitter)
                    {
                        if (!s.StartsWith(@"\") && !string.IsNullOrEmpty(s) && (s.Length <= 6 || s.Length >= 5) &&
                            !s.Contains("Your ") && !s.Equals("\u0002") && !s.Contains("Drawn"))
                        {
                            AddCard(s);
                        }
                        if ((s.Contains("[W]") || s.Contains("[WD4]")) && !s.Contains("Cards:"))
                        {
                            AddWildCard(s);
                        }
                    }

                    PlayCard();
                }
                if (message.Contains("Drawn") && drawingCard)
                {
                    AddDrawnCard(message);
                    Thread.Sleep(1000);
                }
            }
        }

        private static void AddCard(string s)
        {
            var color = s.Split('[')[0];
            var card = s.Split('[')[1].Split(']')[0];

            switch (color)
            {
                case "04":
                    CurrentCards.Add($"R {card}");
                    break;
                case "08":
                    CurrentCards.Add($"Y {card}");
                    break;
                case "09":
                    CurrentCards.Add($"G {card}");
                    break;
                case "12":
                    CurrentCards.Add($"B {card}");
                    break;
            }
        }

        private static void AddDrawnCard(string s)
        {
            drawingCard = false;

            if (s.Contains("[W]"))
            {
                var wildColor = WildColor();
                if (wildColor != null)
                {
                    Play("W", wildColor);
                }
                else
                {
                    client.SendMessage($"{IRC.BOLD}{IRC.RED}ERROR FETCHING COLOR.", IRC_ChannelName);
                }
                return;
            }

            if (s.Contains("[WD4]"))
            {
                var wildColor = WildColor();
                if (wildColor != null)
                {
                    Play("WD4", wildColor);
                }
                else
                {
                    client.SendMessage($"{IRC.BOLD}{IRC.RED}ERROR FETCHING COLOR.", IRC_ChannelName);
                }
                return;
            }

            var splitter = s.Split('\u0003');
            var played = false;

            foreach (var str in splitter)
            {
                if (!str.StartsWith(@"\") && !string.IsNullOrEmpty(s) && (s.Length <= 6 || s.Length >= 5))
                {
                    var color = s.Split('[')[0];
                    var card = s.Split('[')[1].Split(']')[0];

                    switch (color)
                    {
                        case "04":
                            if (isCardPlayable(card, "R"))
                            {
                                Play("R", card);
                                played = true;
                            }
                            break;
                        case "08":
                            if (isCardPlayable(card, "Y"))
                            {
                                Play("Y", card);
                                played = true;
                            }
                            break;
                        case "09":
                            if (isCardPlayable(card, "G"))
                            {
                                Play("G", card);
                                played = true;
                            }
                            break;
                        case "12":
                            if (isCardPlayable(card, "B"))
                            {
                                Play("B", card);
                                played = true;
                            }
                            break;
                    }
                }
            }

            if (!played)
            {
                client.SendMessage(".pa", IRC_ChannelName);
                SemiColoredWrite(ConsoleColor.Green, "[UNO] ", "Passed turn.");
            }
        }

        private static void AddWildCard(string s)
        {
            var card = s.Split('[')[1].Split(']')[0];
            CurrentCards.Add(card);
        }

        private static void PlayCard()
        {
            var canPlay = false;
            var played = false;
            var playCard = string.Empty;

            ShowCurrentCards();
            Thread.Sleep(100);

            foreach (var _card in CurrentCards)
            {
                if (_card.Equals("W") || _card.Equals("WD4"))
                {
                    canPlay = true;
                    playCard = _card;
                }
                else
                {
                    var color = _card.Split(' ')[0];
                    var card = _card.Split(' ')[1];

                    if (isCardPlayable(card, color))
                    {
                        canPlay = true;
                        played = true;
                        Play(color, card);
                        break;
                    }
                }
            }

            if (!canPlay && !drawingCard)
            {
                drawingCard = true;
                client.SendMessage(".d", IRC_ChannelName);
                SemiColoredWrite(ConsoleColor.Green, "[UNO] ", "Drew card");
            }
            else
            {
                if (!played && !drawingCard)
                {
                    var wildColor = WildColor();
                    if (wildColor != null)
                    {
                        Play(playCard, wildColor);
                    }
                    else
                    {
                        client.SendMessage($"{IRC.BOLD}{IRC.RED}ERROR FETCHING COLOR.", IRC_ChannelName);
                    }
                }
            }

            CurrentCards.Clear();
        }

        private static void Play(string color, string card)
        {
            if (color != "R" && color != "Y" && color != "G" && color != "B" && color != "W" && color != "WD4")
            {
                ColoredWrite(ConsoleColor.Red, $"Error playing card. (color: `{color}`, card: `{card}`)");
            }
            else
            {
                client.SendMessage($".p {color} {card}", IRC_ChannelName);
                SemiColoredWrite(ConsoleColor.Green, "[UNO] ", $"Played {color} {card}");
            }
        }

        private static void ShowCurrentCards()
        {
            ColoredWrite(ConsoleColor.Cyan, "Current cards: ");

            foreach (var currentCard in CurrentCards)
            {
                Console.Write($"{currentCard.Replace(" ", "")} ");
                Thread.Sleep(1);
            }
            Console.WriteLine();
        }

        private static bool isCardPlayable(string number, string color)
        {
            if (topNumber.Equals(number))
            {
                return true;
            }
            if (topColor.Equals(color))
            {
                return true;
            }

            return false;
        }

        private static string BuildCTCPTime()
        {
            var time = DateTime.Now;
            var message = $"{IRC.NOCOLOR}{time.ToString("HH:mm:ss tt")} ({time.ToString("dd/MM/yyyy")})";
            return message;
        }

        private static string WildColor()
        {
            var red = 0;
            var blue = 0;
            var yellow = 0;
            var green = 0;

            foreach (var card in CurrentCards)
            {
                if (card.StartsWith("R"))
                {
                    red++;
                }
                if (card.StartsWith("Y"))
                {
                    yellow++;
                }
                if (card.StartsWith("B"))
                {
                    blue++;
                }
                if (card.StartsWith("G"))
                {
                    green++;
                }
            }

            var colors = new List<int> {red, blue, yellow, green};
            var max = colors.Max();

            return max.Equals(red)
                ? "R"
                : (max.Equals(blue) ? "B" : (max.Equals(yellow) ? "Y" : (max.Equals(green) ? "G" : null)));
        }

        private static void Start()
        {
            ColoredWrite(ConsoleColor.Green, "--- READY ---");

            string command;
            do
            {
                Console.ReadKey(true);
                Console.Write("> ");
                command = Console.ReadLine();
                switch (command)
                {
                    case "exit":
                        break;
                    case "say":
                        ColoredWrite(ConsoleColor.DarkGray, "--- say");
                        ColoredWrite(ConsoleColor.Cyan, "Enter message to send:");
                        var msg = Console.ReadLine();
                        ColoredWrite(ConsoleColor.Cyan, "Enter channel to send to:");
                        var channel = Console.ReadLine();

                        try
                        {
                            client.Channels[channel].SendMessage(msg);
                        }
                        catch (Exception ex)
                        {
                            ColoredWrite(ConsoleColor.Red, $"{ex.GetType()}: {ex.Message}");
                        }

                        ColoredWrite(ConsoleColor.DarkGray, "--- end_say");
                        continue;
                    case "join":
                        ColoredWrite(ConsoleColor.DarkGray, "--- join");
                        ColoredWrite(ConsoleColor.Cyan, "Enter channel to join:");

                        try
                        {
                            client.JoinChannel(Console.ReadLine());
                        }
                        catch (Exception ex)
                        {
                            ColoredWrite(ConsoleColor.Red, $"{ex.GetType()}: {ex.Message}");
                        }

                        ColoredWrite(ConsoleColor.DarkGray, "--- end_join");
                        continue;
                    case "part":
                        ColoredWrite(ConsoleColor.DarkGray, "--- part");
                        ColoredWrite(ConsoleColor.Cyan, "Enter channel to part:");

                        try
                        {
                            client.PartChannel(Console.ReadLine());
                        }
                        catch (Exception ex)
                        {
                            ColoredWrite(ConsoleColor.Red, $"{ex.GetType()}: {ex.Message}");
                        }

                        ColoredWrite(ConsoleColor.DarkGray, "--- end_part");
                        continue;
                    default:
                        ColoredWrite(ConsoleColor.DarkGray, "--- err");
                        ColoredWrite(ConsoleColor.Red, "Invalid command.");
                        ColoredWrite(ConsoleColor.DarkGray, "--- end_err");
                        continue;
                }
            } while (command != "exit");
        }

        private static void ColoredWrite(ConsoleColor color, string text)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text + Environment.NewLine);
            Console.ForegroundColor = originalColor;
        }

        private static void SemiColoredWrite(ConsoleColor color, string coloredText, string noColorText)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(coloredText);
            Console.ForegroundColor = originalColor;
            Console.Write(noColorText + Environment.NewLine);
        }
    }
}