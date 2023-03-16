using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenAI
{
    public class Cl100kTokenizer
    {
        private const string ENDOFTEXT = "<|endoftext|>";
        private const string FIM_PREFIX = "<|fim_prefix|>";
        private const string FIM_MIDDLE = "<|fim_middle|>";
        private const string FIM_SUFFIX = "<|fim_suffix|>";
        private const string ENDOFPROMPT = "<|endofprompt|>";

        private readonly Regex _regex = new(""" "(?i:'s|'t|'re|'ve|'m|'ll|'d)|[^\r\n\p{L}\p{N}]?\p{L}+|\p{N}{1,3}| ?[^\s\p{L}\p{N}]+[\r\n]*|\s*[\r\n]+|\s+(?!\S)|\s+",""");

        private static Dictionary<string, uint> _tokens = new Dictionary<string, uint>();
        private static Dictionary<string, uint> _specialTokens = new Dictionary<string, uint>();

        static Cl100kTokenizer()
        {
            using var res = typeof(Cl100kTokenizer).Assembly.GetManifestResourceStream("OpenAI.cl100k_base.tiktoken")!;
            using var sr = new StreamReader(res);
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

            _specialTokens.Add(ENDOFTEXT, 100257);
            _specialTokens.Add(FIM_PREFIX, 100258);
            _specialTokens.Add(FIM_MIDDLE, 100259);
            _specialTokens.Add(FIM_SUFFIX, 100260);
            _specialTokens.Add(ENDOFPROMPT, 100276);
        }

        public static List<Token> Tokenize(string input)
        {
            List<Token> tokens = new();

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

                    tokens.Add(new Token { Value = sub, Id = val });
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

    public class Token
    {
        public uint Id { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}
