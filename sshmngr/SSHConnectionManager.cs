using sshmngr.Helper;
using sshmngr.Model;
using System.Diagnostics;
using System.Text.Json;

namespace sshmngr
{
    internal class SSHConnectionManager
    {
        public List<SSHConnection> Connections { get; set; } = new();

        private readonly string jsonPath;

        public SSHConnectionManager()
        {
            var userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var jsonDir = Path.Combine(userFolder, ".sshmngr");

            Directory.CreateDirectory(jsonDir);

            jsonPath = Path.Combine(jsonDir, "connections.json");

            if (!File.Exists(jsonPath))
                return;

            using var stream = File.OpenRead(jsonPath);
            Connections.AddRange(JsonSerializer.Deserialize<List<SSHConnection>>(stream) ?? new());
        }

        public void Save()
        {
            var serializedCons = JsonSerializer.Serialize(Connections, new JsonSerializerOptions()
            {
                WriteIndented = true,
            });

            File.WriteAllText(jsonPath, serializedCons);
        }

        public void AddNewConnection()
        {
            var con = new SSHConnection();
            Console.Clear();

            Console.WriteLine("Add a new connection");
            Console.WriteLine();

            // Name
            while (true)
            {
                Console.Write("Specifiy a name: ");
                var name = Console.ReadLine();

                if (String.IsNullOrWhiteSpace(name))
                {
                    ConsoleHelper.ClearConsoleLine(-1);
                    continue;
                }

                con.Name = name;
                break;
            }

            while (true)
            {
                Console.Write("Specifiy a host: ");
                var host = Console.ReadLine();

                if (String.IsNullOrWhiteSpace(host))
                {
                    ConsoleHelper.ClearConsoleLine(-1);
                    continue;
                }

                con.Host = host;
                break;
            }

            while (true)
            {
                Console.Write("Specifiy a port (empty for default): ");
                var port = Console.ReadLine();

                if (String.IsNullOrWhiteSpace(port))
                {
                    con.Port = -1;
                    break;
                }
                else
                {
                    if (!Int32.TryParse(port, out int intPort))
                    {
                        ConsoleHelper.ClearConsoleLine(-1);
                        continue;
                    }

                    con.Port = intPort;
                    break;
                }
            }

            while (true)
            {
                Console.Write("Specifiy a user (empty for none): ");
                var username = Console.ReadLine();

                con.Username = username;
                break;
            }

            Connections.Add(con);
            Save();
        }

        public bool DeleteConnection(int i)
        {
            if (i > Connections.Count || i <= 0)
                return false;

            Connections.RemoveAt(i - 1);
            Save();
            return true;
        }

        public bool StartSSHSession(int i)
        {
            if (i > Connections.Count || i <= 0)
                return false;

            var con = Connections[i - 1];

            Console.Clear();
            Console.WriteLine($"Connection: [{con.Name}] - {con}");
            Console.WriteLine();

            var procInfo = new ProcessStartInfo
            {
                UseShellExecute = false,

                CreateNoWindow = false,

                FileName = "ssh",
                Arguments = $"{con}"
            };

            // Fires up a new process to run inside this one
            var process = Process.Start(procInfo);

            // Signal to end the application
            var stopApp = new ManualResetEvent(false);

            // Enables the exited event and set the stopApp signal on exited
            process.EnableRaisingEvents = true;
            process.Exited += (e, sender) => { stopApp.Set(); };

            // Wait for the child app to stop
            stopApp.WaitOne();

            return true;
        }
    }
}
