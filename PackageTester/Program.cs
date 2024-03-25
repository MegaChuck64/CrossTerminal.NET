using CrossTermLib;
using System.Numerics;

namespace PackageTester
{

    internal class Program
    {
        static void Main(string[] args)
        {
            using var terminal = new Terminal(
                 cols: 60,
                 rows: 30,
                 fontPath: "Ubuntu.ttf",
                 fontSize: 20,
                 title: "Console GL Test",
                 defaultFontColor: new Vector4(0f, 1f, 0.2f, 1f));


            var scenes = new Dictionary<string, Scene>
            {
                { "login", new LoginScene() },
                { "menu", new MenuScene() }
            };

            var currentScene = "login";

            do
            {
                terminal.Clear();
                currentScene = scenes[currentScene].Run(terminal);
            } while (currentScene != "exit");

            terminal.WriteLine("Press enter to exit...");

            terminal.ReadLine();

            terminal.Clear();

            var rand = new Random();
            var size = terminal.GetWindowSize();

            for (int i = 0; i < 5; i++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        terminal.SetCursorPosition(x, y);
                        terminal.Write(RandChar(rand), RandColor(rand));
                    }
                }

            }

            terminal.ReadLine();

        }

        private static Vector4 RandColor(Random rand)
        {
            return new Vector4((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1f);
        }

        private static readonly string charOptions = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()?><;':{}[]_+-=~`";

        private static char RandChar(Random rand) => charOptions[rand.Next(charOptions.Length)];
    }

    internal static class GameMemory
    {
        public static string Name { get; set; } = string.Empty;
    }
    internal class LoginScene : Scene
    {
        public override string Run(Terminal terminal)
        {
            string name;
            do
            {
                terminal.Clear();

                terminal.WriteLine("Hello world...");
                terminal.WriteLine("Please type your name...");

                name = terminal.ReadLine();

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                terminal.WriteLine($"You entered '{name}'. Keep? y or n");
                if (terminal.ReadLine() == "y")
                    break;

            } while (!terminal.IsClosing);

            terminal.WriteLine($"Welcome, {name}. Press enter to start...");

            GameMemory.Name = name;

            return terminal.IsClosing ? "exit" : "menu";
        }
    }

    internal class MenuScene : Scene
    {
        public override string Run(Terminal terminal)
        {
            string scene = "exit";
            do
            {
                terminal.Clear();

                terminal.Write("------- ", new Vector4(1f, 0f, 0.2f,1f));
                terminal.Write("MENU", new Vector4(0f, 0.2f, 1f, 1f));
                terminal.WriteLine(" -------", new Vector4(1f, 0f, 0.2f, 1f));
                terminal.WriteLine("1. play");
                terminal.WriteLine("2. login");
                terminal.WriteLine("3. exit");

                var choice = terminal.ReadLine();
                
                if (choice == "1")
                {
                    scene = "play";
                }
                else if (choice == "2")
                {
                    scene = "login";
                    break;
                }
                else if (choice == "3")
                {
                    scene = "exit";
                    break;
                }


            } while (!terminal.IsClosing);

            return scene;
        }


    }

    internal abstract class Scene
    {
        /// <summary>
        /// Runs until should run next scene or 'exit' to close game. Next scene's name is returned
        /// </summary>
        /// <returns></returns>
        public abstract string Run(Terminal terminal);
    }
}
