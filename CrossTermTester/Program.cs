using CrossTermLib;

namespace CrossTermTester
{
    internal class Program
    {
        static void Main(string[] args)
        {

            using var terminal = new Terminal(800, 600, "Hollow World TESTER");

            terminal.WriteLine("Testing...");
            terminal.WriteLine("Is this thing on?");
            terminal.WriteLine(":) - - - (:");
            terminal.WriteLine("--------------");

            var invalid = false;
            string? name;
            do
            {
                terminal.Clear();
                if (invalid)
                    terminal.WriteLine("! Invalid Input !");

                terminal.WriteLine("Enter Name: ...");
                name = terminal.ReadLine();

                if (string.IsNullOrWhiteSpace(name))
                {
                    invalid = true;
                    continue;
                }

                terminal.WriteLine($"You chose '{name}' as your name.");
                terminal.WriteLine($"Is this correct? y or n");
                var choice = terminal.ReadLine();

                if (choice == "y")
                    break;

                invalid = true;

            } while (!terminal.IsClosing);

            terminal.WriteLine($"Welcome, {name}");
            terminal.WriteLine("Press enter to exit...");
            terminal.ReadLine();
        }

    }
}
