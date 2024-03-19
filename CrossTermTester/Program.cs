using System.Numerics;
using CrossTermLib;

namespace CrossTermTester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using var terminal = new Terminal(
                cols: 80,
                rows: 40,
                title: "Terminal Game",
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
                Task.Delay((int)((1f/60f)*1000f));

            } while (!terminal.IsClosing);

            terminal.WriteLine($"Goodbye {name}. Press Enter to Exit...");

            terminal.ReadLine();

        }

    }
}
