using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace sshmngr.Model
{
    internal class SSHConnection
    {
        public string Name { get; set; }

        public string Host { get; set; }

        public int Port { get; set; } = -1;

        public string Username { get; set; }

        public override string ToString()
        {
            var conStr = $"{Host}";

            if (!string.IsNullOrWhiteSpace(Username))
                conStr = $"{Username}@{conStr}";

            if (Port != -1)
                conStr += $":{Port}";

            return conStr;
        }
    }
}
