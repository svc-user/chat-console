using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatConsole
{
    internal class Console2
    {
        public static string ReadInput()
        {
            StringBuilder sb = new();
            string? line = string.Empty;
            while (line != null && !line.EndsWith((char)0x1a))
            {
                line = Console.ReadLine();
                sb.AppendLine(line);
            }

            return sb.ToString().TrimEnd('\r', '\n', (char)0x1a);
        }
    }
}
