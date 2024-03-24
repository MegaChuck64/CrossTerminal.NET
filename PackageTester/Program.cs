using CrossTermLib;
using System.Numerics;

namespace PackageTester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using var console = new ConsoleGL(
                 cols: 60,
                 rows: 30,
                 fontPath: "Ubuntu.ttf",
                 fontSize: 20,
                 title: "Console GL Test",
                 defaultFontColor: new Vector4(0f, 1f, 0.2f, 1f));


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

            console.WriteLine($"Welcome, {name}. Press enter to exit...");

            console.ReadLine();

        }
    }
}
