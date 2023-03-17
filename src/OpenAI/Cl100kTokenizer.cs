using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenAI
{
    public class Cl100kTokenizer
    {
        private const string ENDOFTEXT = "<|endoftext|>";
        private const string FIM_PREFIX = "<|fim_prefix|>";
        private const string FIM_MIDDLE = "<|fim_middle|>";
        private const string FIM_SUFFIX = "<|fim_suffix|>";
        private const string ENDOFPROMPT = "<|endofprompt|>";

        private readonly char[] specialChars = new char[] { '\\', '.', '+', '*', '?', '(', ')', '|', '[', ']', '{', '}', '^', '$', '#' };

        private static readonly Regex _regex = new("""(?i:'s|'t|'re|'ve|'m|'ll|'d)|[^\r\n\p{L}\p{N}]?\p{L}+|\p{N}{1,3}| ?[^\s\p{L}\p{N}]+[\r\n]*|\s*[\r\n]+|\s+(?!\S)|\s+""");
        private static readonly Regex _specialRegex;

        private static Dictionary<byte[], int> _tokens = new();
        private static Dictionary<string, int> _specialTokens = new();

        static Cl100kTokenizer()
        {
            LoadCl100kFile();

            _specialTokens.Add(ENDOFTEXT, 100257);
            _specialTokens.Add(FIM_PREFIX, 100258);
            _specialTokens.Add(FIM_MIDDLE, 100259);
            _specialTokens.Add(FIM_SUFFIX, 100260);
            _specialTokens.Add(ENDOFPROMPT, 100276);

            var parts = _specialTokens.Keys
              .Select(s => Regex.Escape(s))
              .ToList();

            _specialRegex = new(string.Join("|", parts));

        }

        private static void LoadCl100kFile()
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
                var rank = int.Parse(splitted[1]);

                if (!_tokens.TryAdd(fromBase64, rank))
                {
                    Debug.Assert(false);
                }
            }
        }

        //[Obsolete("Use EncodeNative instead")]
        //public static List<Token> Tokenize(string input)
        //{
        //    List<Token> tokens = new();

        //    int currentIndex = 0;
        //    int currentEnd = 0;
        //    while (currentIndex < input.Length)
        //    {

        //        //Trace.WriteLine($"Searching token in range [{currentIndex}..^{currentEnd}]. Input length is {input.Length}");
        //        var sub = input[currentIndex..^currentEnd];
        //        //Trace.WriteLine($"Current substring is: '{sub}'");
        //        if (_tokens.TryGetValue(sub, out var val))
        //        {
        //            //Trace.WriteLine($"Found token: '{sub}'");

        //            tokens.Add(new Token { Value = sub, Id = val });
        //            currentIndex += sub.Length;
        //            currentEnd = 0;
        //        }
        //        else
        //        {
        //            currentEnd++;
        //        }
        //    }

        //    return tokens;
        //}

        public static List<int> EncodeNative(string input) => EncodeNative(input, new());

        public static List<int> EncodeNative(string input, HashSet<string> allowedSpecial)
        {
            var ret = new List<int>();
            var start = 0;

            while (true)
            {
                Match nsMatch;
                int? nextSpecial = null;
                var startFind = start;
                while (true)
                {
                    nsMatch = _specialRegex.Match(input, startFind);
                    if (!nsMatch.Success) break;

                    nextSpecial = nsMatch.Index;

                    var match = input.Substring(nextSpecial.Value, nsMatch.Length);
                    if (allowedSpecial.Contains(match))
                    {
                        break;
                    }

                    startFind = nextSpecial.Value + 1;
                }

                var end = nextSpecial ?? input.Length;
                foreach (Match match in _regex.Matches(input.Substring(start, end - start)))
                {
                    var matchBytes = Encoding.UTF8.GetBytes(match.Value);
                    if (_tokens.ContainsKey(matchBytes))
                    {
                        ret.Add(_tokens[matchBytes]);
                        continue;
                    }

                    var piece = matchBytes;
                    var tokens = BytePairEncode(piece, _tokens);
                    ret.AddRange(tokens);
                }

                if (nextSpecial != null)
                {
                    var piece = input.Substring(nextSpecial.Value, nsMatch.Length);
                    var token = _specialTokens[piece];
                    ret.Add(token);
                    start = nextSpecial.Value + nsMatch.Length;
                }
                else
                {
                    break;
                }
            }

            return ret;
        }

        private static List<int> BytePairEncode(byte[] piece, Dictionary<byte[], int> ranks)
        {
            if (piece.Length == 1)
            {
                return new() { ranks[piece] };
            }

            return BytePairMerge(piece, ranks);
        }

        private static List<int> BytePairMerge(byte[] piece, Dictionary<byte[], int> ranks)
        {

            var parts = Enumerable.Range(0, piece.Length + 1)
                                     .Select(i => new Ranked { Item1 = i, Item2 = int.MaxValue })
                                     .ToList();

            int? GetRank(int startIdx, int skip = 0)
            {
                if (startIdx + skip + 2 < parts.Count)
                {
                    var start = parts[startIdx].Item1;
                    var end = parts[startIdx + skip + 2].Item1;
                    var subPiece = piece[start..end];
                    if (ranks.TryGetValue(subPiece, out int rank))
                    {
                        return rank;
                    }
                }
                return null;
            }

            // We look up the ranks once in the beginning and iteratively update
            // them during each merge, which reduces the number of rank lookups.
            for (int i = 0; i < parts.Count - 2; i++)
            {
                var rank = GetRank(i);
                if (rank.HasValue)
                {
                    Debug.Assert(rank != int.MaxValue, "Sentinel value usize::MAX is not a valid rank.");

                    parts[i] = new Ranked { Item1 = parts[i].Item1, Item2 = rank.Value };
                }
            }

            while (parts.Count > 1)
            {
                var minRank = (int.MaxValue, 0);
                for (int i = 0; i < parts.Count - 1; i++)
                {
                    if (parts[i].Item2 < minRank.Item1)
                    {
                        minRank = (parts[i].Item2, i);
                    }
                }

                if (minRank.Item1 != int.MaxValue)
                {
                    var i = minRank.Item2;
                    parts[i].Item2 = GetRank(i, 1) ?? int.MaxValue;
                    if (i > 0)
                    {
                        parts[i - 1].Item2 = GetRank(i - 1, 1) ?? int.MaxValue;
                    }

                    parts.RemoveAt(i + 1);
                }
                else
                {
                    break;
                }
            }

            var outList = new List<int>(parts.Count - 1);
            for (int i = 0; i < parts.Count - 1; i++)
            {
                var start = parts[i].Item1;
                var end = parts[i + 1].Item1;
                var itm = ranks[piece[start..end]];
                outList.Add(itm);
            }
            return outList;
        }

        [DebuggerDisplay("Item1 = {Item1}, Item2 = {Item2}")]
        internal class Ranked
        {
            public int Item1 { get; set; }
            public int Item2 { get; set; }
        }

        public class Token
        {
            public int Id { get; set; }
            public string Value { get; set; } = string.Empty;
        }
    }
}
