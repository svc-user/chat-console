using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace chat_console.OpenAI
{
    internal class TokenHelper
    {
        private static Dictionary<string, uint> _tokens = new Dictionary<string, uint>();

        static TokenHelper()
        {
            using var sr = new StreamReader("./OpenAI/cl100k_base.tiktoken");
            string? line;
            while ((line = sr.ReadLine()) != null)
            {
                var splitted = line.Split(" ", StringSplitOptions.TrimEntries);
                if (splitted.Count() < 2) continue;

                byte[] fromBase64 = Convert.FromBase64String(splitted[0]);
                string token = Encoding.Latin1.GetString(fromBase64);
                uint rank = uint.Parse(splitted[1]);

                if (!_tokens.TryAdd(token, rank))
                {
                    Debugger.Break();
                }
            }
        }

        public static List<(uint, string)> Tokenize(string input)
        {
            List<(uint, string)> tokens = new();

            int currentIndex = 0;
            int currentEnd = 0;
            while (currentIndex < input.Length)
            {

                //Trace.WriteLine($"Searching token in range [{currentIndex}..^{currentEnd}]. Input length is {input.Length}");
                var sub = input[currentIndex..^currentEnd];
                //Trace.WriteLine($"Current substring is: '{sub}'");
                if (_tokens.TryGetValue(sub, out var val))
                {
                    //Trace.WriteLine($"Found token: '{sub}'");

                    tokens.Add((val, sub));
                    currentIndex += sub.Length;
                    currentEnd = 0;


                }
                else
                {
                    currentEnd++;
                }
            }

            return tokens;
        }
    }
}
