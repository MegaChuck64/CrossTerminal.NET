using CrossTermLib;
using System.Numerics;

namespace PackageTester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using var console = new Terminal(
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
                console.Clear();
                currentScene = scenes[currentScene].Run(console);
            } while (currentScene != "exit");

            console.WriteLine("Press enter to exit...");
            console.ReadLine();

        }
    }

    internal static class GameMemory
    {
        public static string Name { get; set; } = string.Empty;
    }
    internal class LoginScene : Scene
    {
        public override string Run(Terminal console)
        {
            string name;
            do
            {
                console.Clear();

                console.WriteLine("Hello world...");
                console.WriteLine("Please type your name...");

                name = console.ReadLine();

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                console.WriteLine($"You entered '{name}'. Keep? y or n");
                if (console.ReadLine() == "y")
                    break;

            } while (!console.IsClosing);

            console.WriteLine($"Welcome, {name}. Press enter to start...");

            GameMemory.Name = name;

            return console.IsClosing ? "exit" : "menu";
        }
    }

    internal class MenuScene : Scene
    {
        public override string Run(Terminal console)
        {
            string scene = "exit";
            do
            {
                console.Clear();

                console.Write("------- ", new Vector4(1f, 0f, 0.2f,1f));
                console.Write("MENU", new Vector4(0f, 0.2f, 1f, 1f));
                console.WriteLine(" -------", new Vector4(1f, 0f, 0.2f, 1f));
                console.WriteLine("1. play");
                console.WriteLine("2. login");
                console.WriteLine("3. exit");

                var choice = console.ReadLine();
                
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


            } while (!console.IsClosing);

            return scene;
        }
    }

    internal abstract class Scene
    {
        /// <summary>
        /// Runs until should run next scene or 'exit' to close game. Next scene's name is returned
        /// </summary>
        /// <returns></returns>
        public abstract string Run(Terminal console);
    }
}
