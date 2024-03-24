using System.Numerics;
using CrossTermLib;

namespace CrossTermTester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //RunTerminalGame();
            //RunTerminal();
            RunConsoleGL();
        }


        private static void RunConsoleGL()
        {
            using var console = new ConsoleGL(
                cols: 60,
                rows: 30,
                fontPath: Path.Combine("Content", "Fonts", "Ubuntu.ttf"),
                fontSize: 18,
                title: "Hollow World",
                defaultFontColor: new Vector4(0f, 1f, 0f, 1f));

            console.Write("-------------");
            console.Write("hollow world");
            console.Write("-------------");
            console.ReadLine();
            string name;
            do
            {
                console.Clear();
                console.WriteLine("Hi... please enter your name");
                name = console.ReadLine();

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                console.WriteLine($"You chose '{name}', is that correct? y or n");

                if (console.ReadLine() == "y")
                    break;

            } while (!console.IsClosing);

            console.WriteLine($"Welcome, {name}. Press enter to exit...");

            console.ReadLine();

            (var cols, var rows) = console.GetWindowSize();
            var rand = new Random();
            var message = "? ? ? ? hello world ? ? ? ?";

            console.SetCursorPosition((cols / 2) - (message.Length / 2), (rows / 2));

            foreach (var ch in message)
            {
                console.Write(ch, RandColor(rand));
            }

            string scene;
            do
            {
                console.Clear();

                console.Write("------- ", new Vector4(1f, 0f, 0.2f, 1f));
                console.Write("MENU", new Vector4(0f, 0.2f, 1f, 1f));
                console.WriteLine(" -------", new Vector4(1f, 0f, 0.2f, 1f));
                console.WriteLine("1. play");
                console.WriteLine("2. leaderboard");
                console.WriteLine("3. settings");

                var choice = console.ReadLine();

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

            } while (!console.IsClosing);

            console.ReadLine();
        }

        private static void RunTerminalGame()
        {
            using var game = new TerminalGame(
                cols: 80,
                rows: 40,
                title: "Terminal Game Test",
                fontPath: Path.Combine("Content", "Fonts", "Ubuntu.ttf"),
                backgroundColor: new Vector4(0f, 0f, 0.5f, 1f),
                defaultFontColor: new Vector4(0.5f, 0.5f, 0f, 1f),
                fontSize: 20,
                paddingPercentage: 1.2f);

            var timer = 0f;
            var updatePS = 50f;
            int x = 0;
            int y = 0;
            var rand = new Random();
            game.OnUpdate += (dt) =>
            {
                timer += (float)dt;

                if (timer > 1f/ updatePS)
                {
                    if (y < game.Cols - 1)
                    {
                        timer = 0f;
                        x++;
                        if (x > game.Cols - 1)
                        {
                            x = 0;
                            y++;
                        }
                        game.PutChar('?', x, y, RandColor(rand));
                    }
                }
            };

            game.Start();
        }

        private static Vector4 RandColor(Random rand)
        {
            return new Vector4((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1f);
        }

        private static void RunTerminal()
        {
            using var terminal = new Terminal(
                cols: 80,
                rows: 40,
                title: "Terminal Test",
                fontPath: Path.Combine("Content", "Fonts", "Ubuntu.ttf"),
                cursorBlinkSpeed: 0.4f,
                backgroundColor: new Vector4(0f, 0f, 0f, 1f),
                defaultFontColor: new Vector4(1f, 1f, 1f, 1f),
                fontSize: 20,
                paddingPercentage: 1.2f);


            var invalid = false;
            string name;
            do
            {
                terminal.Clear();

                terminal.WriteLine("Hello World");

                terminal.WriteLine("Enter Name...");

                if (invalid)
                    terminal.WriteLine("Invalid Input");

                name = terminal.ReadLine();
                if (!string.IsNullOrEmpty(name))
                {
                    terminal.WriteLine($"You chose '{name}'. Correct? y or n");

                    var choice = terminal.ReadLine();

                    if (choice == "y")
                        break;
                }
                else
                {
                    invalid = true;
                }

            } while (!terminal.IsClosing);

            terminal.Clear();

            terminal.PutChar('H', 0, 0);
            terminal.PutChar('H', terminal.Cols - 1, 0);
            terminal.PutChar('H', terminal.Cols - 1, terminal.Rows - 1);
            terminal.PutChar('H', 0, terminal.Rows - 1);

            var cnt = 0;
            var updateTimer = 0f;
            var updatePerSecond = 2f;
            var rand = new Random();
            do
            {
                terminal.Tick(true);
                updateTimer += 0.0170f;
                if (updateTimer > 1f / updatePerSecond)
                {
                    updateTimer = 0f;
                    cnt++;
                    for (int i = 0; i < terminal.Cols; i++)
                    {
                        for (int j = 0; j < terminal.Rows; j++)
                        {
                            var col = new Vector4((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1f);
                            var num = rand.Next(0, 26); // Zero to 25
                            var c = (char)('A' + num);
                            terminal.PutChar(c, i, j, col);
                        }
                    }
                }
                Task.Delay(170);

                if (cnt > 10)
                    break;

            } while (!terminal.IsClosing);

            var y = 0;
            updatePerSecond = 3f;
            do
            {
                terminal.Tick(true);
                updateTimer += (1f / 60f);
                if (updateTimer > 1f / updatePerSecond)
                {
                    updateTimer = 0f;
                    for (int i = 0; i < terminal.Cols; i++)
                    {
                        var col = new Vector4((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1f);
                        var num = rand.Next(0, 26); // Zero to 25
                        var c = (char)('A' + num);
                        terminal.PutChar(c, i, y, col);
                    }
                    y++;
                    if (y > terminal.Rows - 1)
                        break;
                }
                Task.Delay((int)((1f / 60f) * 1000f));

            } while (!terminal.IsClosing);

            terminal.WriteLine($"Goodbye {name}. Press Enter to Exit...");

            terminal.ReadLine();

        }

    }
}
