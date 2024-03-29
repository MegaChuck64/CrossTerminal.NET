﻿using System.Numerics;
using CrossTermLib;

namespace CrossTermTester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RunTerminal();
        }


        private static void RunTerminal()
        {
            var rand = new Random();

            using var terminal = new Terminal(
                cols: 60,
                rows: 30,
                fontPath: Path.Combine("Content", "Fonts", "Ubuntu.ttf"),
                fontSize: 18,
                title: "Hollow World",
                defaultFontColor: new Vector4(0f, 1f, 0f, 1f));

            terminal.Write("-------------", RandColor(rand));
            terminal.Write("hollow world", RandColor(rand));
            terminal.Write("-------------", RandColor(rand));
            terminal.ReadLine();
            string name;
            do
            {
                terminal.Clear();
                terminal.WriteLine("Hi... please enter your name");
                name = terminal.ReadLine();

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                terminal.WriteLine($"You chose '{name}', is that correct? y or n");

                if (terminal.ReadLine() == "y")
                    break;

            } while (!terminal.IsClosing);

            terminal.WriteLine($"Welcome, {name}. Press enter to exit...");

            terminal.ReadLine();

            (var cols, var rows) = terminal.GetWindowSize();
            var message = "? ? ? ? hello world ? ? ? ?";

            terminal.SetCursorPosition((cols / 2) - (message.Length / 2), (rows / 2));

            foreach (var ch in message)
            {
                terminal.Write(ch, RandColor(rand));
            }

            string scene;
            do
            {
                terminal.Clear();

                terminal.Write("------- ", new Vector4(1f, 0f, 0.2f, 1f));
                terminal.Write("MENU", new Vector4(0f, 0.2f, 1f, 1f));
                terminal.WriteLine(" -------", new Vector4(1f, 0f, 0.2f, 1f));
                terminal.WriteLine("1. play");
                terminal.WriteLine("2. leaderboard");
                terminal.WriteLine("3. settings");

                var choice = terminal.ReadLine();

                if (choice == "1")
                {
                    scene = "play";
                }
                else if (choice == "2")
                {
                    scene = "leaderboard";
                }
                else if (choice == "3")
                {
                    scene = "settings";
                }

                terminal.ReadLine();

                terminal.Clear();

                var size = terminal.GetWindowSize();

                for (int i = 0; i < 10; i++)
                {
                    for (int x = 0; x < size.x; x++)
                    {
                        for (int y = 0; y < size.y; y++)
                        {
                            terminal.SetCursorPosition(x, y);
                            terminal.Write(RandChar(rand), RandColor(rand));
                        }
                    }

                    terminal.Refresh();

                    if (i % 2 == 0)
                        terminal.SetWindowSize(40, 20);
                    else
                        terminal.SetWindowSize(60, 30);

                    Task.Delay(250).Wait();
                }

                break;

            } while (!terminal.IsClosing);

            terminal.ReadLine();

            terminal.SetWindowSize(40, 20);

            terminal.ReadLine();

            terminal.SetWindowSize(60, 30);

            terminal.ReadLine();
        }

        private static Vector4 RandColor(Random rand)
        {
            return new Vector4((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1f);
        }

        private static readonly string charOptions = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()?><;':{}[]_+-=~`";

        private static char RandChar(Random rand) => charOptions[rand.Next(charOptions.Length)];

    }
}
