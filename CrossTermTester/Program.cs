using System.Drawing;
using CrossTerminal.NET;

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
                cols: 50,
                rows: 25,
                fontPath: Path.Combine("Content", "Fonts", "Ubuntu.ttf"),
                fontSize: 20,
                title: "Hollow World",
                cursorBlinkSpeed: 0.4f,
                paddingPercentage: 1.2f,
                defaultFontColor: Color.White,
                backgroundColor: Color.Black
                );

            terminal.Write("-----------", Color.White);
            terminal.Write("hollow world", (c, i) => RandCol(rand));
            terminal.WriteLine("-----------", RandCol(rand));
            
            var line = terminal.ReadLine();
            terminal.WriteLine("You typed: " + line, (c) => c is 'e' or 'o' or 'a' or 'i' or 'u' ? RandCol(rand) : Color.Yellow);

            terminal.SetWindowSize(26, 13);
            terminal.WriteLine("Testing...", (c, i) => i % 2 == 0 ? Color.Green : Color.Blue);
            terminal.ReadLine();
            terminal.SetWindowSize(50, 25);
            terminal.ReadLine();

            string name;
            do
            {
                terminal.Clear();
                var rand1 = RandCol(rand);
                terminal.WriteLine("Hi... please enter your name", (c, i) => i % 2 == 0 ? Color.Green : Color.Pink);
                name = terminal.ReadLine();

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                terminal.WriteLine($"You chose '{name}', is that correct? y or n");

                if (terminal.ReadLine() == "y")
                    break;

            } while (!terminal.IsClosing);

            terminal.WriteLine($"Welcome, {name}. Press enter to exit...");

            terminal.ReadLine();

        }

        private static Color RandCol(Random rand)
        {
            return Color.FromArgb(255, rand.Next(255), rand.Next(255), rand.Next(255));
        }
        private static readonly string charOptions = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()?><;':{}[]_+-=~`";

        private static char RandChar(Random rand) => charOptions[rand.Next(charOptions.Length)];

    }
}
