using System.Numerics;
using CrossTermLib;

namespace CrossTermTester
{
    internal class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            args =
            [
                //"c",
                "t"
            ];
#endif
            if (args.Length == 0)
                return;

            if (args.Contains("c"))
            {
                RunProgram(new ConsoleWrapper());
            }
            
            if (args.Contains("t"))
            {
                (string name, float padding)[] fonts =
                [
                    ("Asphalt", 1.05f),     //0
                    ("SDS_8x8", 1.125f),    //1
                    ("Ubuntu",  1.1f)       //2
                ];

                var choice = 0;

                using var term = new TerminalWrapper(
                    cols: 40,
                    rows: 20,
                    title: "My Test Window",
                    fontPath: Path.Combine("Content", "Fonts", $"{fonts[choice].name}.ttf"),
                    cursorSpeed: 0.4f,
                    backgroundColor: new Vector4(0f, 0f, 0f, 1f),
                    fontColor: new Vector4(1f, 1f, 1f, 1f),
                    fontSize: 22,
                    paddingPercentage: fonts[choice].padding);

                RunProgram(term);
            }

        }

        static void RunProgram(IConsole console)
        {
            console.WriteLine("Hello World...");
            console.WriteLine("--------------");

            var invalid = false;
            string? name;
            do
            {
                console.Clear();
                if (invalid)
                    console.WriteLine("! Invalid Input !");

                console.WriteLine("Enter Name...");
                name = console.ReadLine();

                if (string.IsNullOrWhiteSpace(name))
                {
                    invalid = true;
                    continue;
                }

                console.WriteLine($"You chose '{name}' as your name.");
                console.WriteLine($"Is this correct? y or n");
                var choice = console.ReadLine();

                if (choice == "y")
                    break;

                invalid = true;

            } while (true);

            console.WriteLine($"Welcome, {name}");
            console.WriteLine("Press enter to exit...");
            console.ReadLine();
        }

    }

    internal interface IConsole
    {        
        void WriteLine(string message);
        string? ReadLine();
        void Clear();
        bool IsClosing { get; }
    }

    internal class ConsoleWrapper : IConsole
    {
        public void WriteLine(string message) => Console.WriteLine(message);        
        public string? ReadLine() => Console.ReadLine();
        public void Clear() => Console.Clear();
        public bool IsClosing => false;
    }

    internal class TerminalWrapper : IConsole, IDisposable
    {
        private readonly Terminal _terminal;
        private bool disposedValue;
        public TerminalWrapper(
            int cols,
            int rows,
            string title,
            string fontPath,
            float cursorSpeed,
            Vector4 backgroundColor,
            Vector4 fontColor,
            int fontSize,
            float paddingPercentage)
        {
            _terminal = new(cols, rows, title, fontPath, cursorSpeed, backgroundColor, fontColor, fontSize, paddingPercentage);
        }

        public void WriteLine(string message) => _terminal.WriteLine(message);
        public string? ReadLine() => _terminal.ReadLine();
        public void Clear() => _terminal.Clear();
        public bool IsClosing => _terminal.IsClosing;
        public void Tick() => _terminal.Tick(true);
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _terminal.Dispose();
                }

                disposedValue = true;
            }
        }


        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}
