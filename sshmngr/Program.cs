using sshmngr.Helper;

namespace sshmngr
{
    internal class Program
    {
        private static SSHConnectionManager mngr = new SSHConnectionManager();

        static void PrintHome()
        {
            Console.Clear();

            Console.WriteLine("sshmngr - Welcome");
            Console.WriteLine();

            Console.WriteLine("Actions:");
            Console.WriteLine("add       - Add SSH connection");
            Console.WriteLine("del {num} - Deletes the SSH connection with the specified number");
            Console.WriteLine("con {num} - Connects to the SSH connection with the specified number");
            Console.WriteLine();

            Console.WriteLine("Available connections:");
            Console.WriteLine();

            for (int i = 0; i < mngr.Connections.Count; i++)
            {
                var numberString = $"[{ i + 1}]".PadRight(2 + mngr.Connections.Count.ToString().Length, ' ');

                Console.WriteLine($"{numberString} - {mngr.Connections[i].Name}");
            }

            if (mngr.Connections.Count == 0)
                Console.WriteLine("--- None ---");

            Console.WriteLine();
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            PrintHome();

            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine() ?? "";
                if (String.IsNullOrWhiteSpace(input))
                {
                    ConsoleHelper.ClearConsoleLine(-1);
                    continue;
                }

                if (input == "add")
                {
                    mngr.AddNewConnection();
                    PrintHome();
                }
                else if (input.StartsWith("del "))
                {
                    if (Int32.TryParse(input[4..], out int i))
                    {
                        if(mngr.DeleteConnection(i))
                            PrintHome();
                        else
                        {
                            ConsoleHelper.WriteToConsoleLine(-2, $"Connection [{i}] does not exist.");
                            ConsoleHelper.ClearConsoleLine(-1);
                        }
                    }
                    else
                    {
                        ConsoleHelper.ClearConsoleLine(-1);
                    }
                }
                else if (input.StartsWith("con "))
                {
                    if (Int32.TryParse(input[4..], out int i))
                    {
                        if (!mngr.StartSSHSession(i))
                        {
                            ConsoleHelper.WriteToConsoleLine(-2, $"Connection [{i}] does not exist.");
                            ConsoleHelper.ClearConsoleLine(-1);
                        }
                    }
                    else
                    {
                        ConsoleHelper.ClearConsoleLine(-1);
                    }
                }
            }
        }
    }
}