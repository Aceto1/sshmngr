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
            using var stream = File.OpenWrite(jsonPath);

            JsonSerializer.Serialize(stream, Connections, new JsonSerializerOptions()
            {
                WriteIndented = true,
            });
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

            var procInfo = new ProcessStartInfo
            {
                UseShellExecute = false,

                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,

                CreateNoWindow = true,

                FileName = "ssh"
            };

            procInfo.Arguments = con.ToString();

            // Fires up a new process to run inside this one
            var process = Process.Start(procInfo);

            // Depending on your application you may either prioritize the IO or the exact opposite
            const ThreadPriority ioPriority = ThreadPriority.Highest;
            var outputThread = new Thread(outputReader) { Name = "ChildIO Output", Priority = ioPriority };
            var errorThread =  new Thread(errorReader)  { Name = "ChildIO Error", Priority = ioPriority };
            var inputThread =  new Thread(inputReader)  { Name = "ChildIO Input", Priority = ioPriority };

            // Set as background threads (will automatically stop when application ends)
            outputThread.IsBackground = errorThread.IsBackground
                = inputThread.IsBackground = true;

            // Start the IO threads
            outputThread.Start(process);
            errorThread.Start(process);
            inputThread.Start(process);

            // Signal to end the application
            ManualResetEvent stopApp = new ManualResetEvent(false);

            // Enables the exited event and set the stopApp signal on exited
            process.EnableRaisingEvents = true;
            process.Exited += (e, sender) => { stopApp.Set(); };

            // Wait for the child app to stop
            stopApp.WaitOne();

            // Write some nice output for now?
            Console.WriteLine();
            Console.WriteLine("SSH Session closed.");

            return true;
        }

        /// <summary>
        /// Continuously copies data from one stream to the other.
        /// </summary>
        /// <param name="instream">The input stream.</param>
        /// <param name="outstream">The output stream.</param>
        private static void PassThrough(Stream instream, Stream outstream)
        {
            byte[] buffer = new byte[4096];
            while (true)
            {
                int len;
                while ((len = instream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    outstream.Write(buffer, 0, len);
                    outstream.Flush();
                }
            }
        }

        private static void outputReader(object p)
        {
            var process = (Process)p;
            // Pass the standard output of the child to our standard output
            PassThrough(process.StandardOutput.BaseStream, Console.OpenStandardOutput());
        }

        private static void errorReader(object p)
        {
            var process = (Process)p;
            // Pass the standard error of the child to our standard error
            PassThrough(process.StandardError.BaseStream, Console.OpenStandardError());
        }

        private static void inputReader(object p)
        {
            var process = (Process)p;
            // Pass our standard input into the standard input of the child
            PassThrough(Console.OpenStandardInput(), process.StandardInput.BaseStream);
        }
    }
}
