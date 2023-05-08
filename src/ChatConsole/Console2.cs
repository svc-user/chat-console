using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace ChatConsole
{
    internal class Console2
    {
        public static string ReadInput()
        {


            int bufferIndex = 0;
            var (_, topOffset) = Console.GetCursorPosition();
            Func<(int, int)> GetCalculatedCursorPos = () =>
            {
                var (left, top) = Console.GetCursorPosition();
                if (bufferIndex == 0)
                {
                    left -= 9;
                }
                top -= topOffset;

                return (left, top);
            };

            Action<(int, int)> SetCalculatedCursorPos = (cpos) =>
            {
                var (left, top) = cpos;
                if (bufferIndex == 0)
                {
                    left += 9;
                }
                top += topOffset;

                Console.SetCursorPosition(Math.Min(left, Console.BufferWidth - 1), top);
            };

            ConsoleKeyInfo keyInfo;
            List<StringBuilder> lines = new() { new() };
            do
            {
                var (absLeft, _) = Console.GetCursorPosition();
                var (left, _) = GetCalculatedCursorPos();
                keyInfo = Console.ReadKey(true);
                if (keyInfo.Key != ConsoleKey.Enter && IsPrintable(keyInfo.KeyChar)) //keyInfo.KeyChar <= 126 && 32 <= keyInfo.KeyChar || (new char[] { 'æ', 'ø', 'å' }.Contains(char.ToLowerInvariant(keyInfo.KeyChar))))
                {
                    if (absLeft == Console.BufferWidth - 1)
                    {
                        bufferIndex++;
                        if (lines.Count - 1 < bufferIndex)
                        {
                            lines.Add(new());
                        }
                        left = 0;
                        SetCalculatedCursorPos((left, bufferIndex));
                    }
                    lines[bufferIndex].Insert(left, keyInfo.KeyChar);
                    Console.Write(("\r" + (bufferIndex == 0 ? "prompt > " : "") + lines[bufferIndex]).PadRight(Console.BufferWidth));
                    SetCalculatedCursorPos((left + 1, bufferIndex));
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    lines[bufferIndex].Insert(left, '\n');
                    if (left < lines[bufferIndex].Length - 1)
                    {
                        var rem = lines[bufferIndex].ToString()[(left + 1)..];
                        lines[bufferIndex] = lines[bufferIndex].Remove(left + 1, rem.Length);

                        lines.Insert(++bufferIndex, new());
                        lines[bufferIndex].Append(rem);
                    }
                    else
                    {
                        lines.Insert(++bufferIndex, new());
                    }

                    for (var i = bufferIndex - 1; i < lines.Count; i++)
                    {
                        SetCalculatedCursorPos((0, i));
                        PrintLine(lines[i], i);
                    }
                    SetCalculatedCursorPos((0, bufferIndex));

                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    bufferIndex++;
                    if (lines.Count - 1 < bufferIndex)
                    {
                        lines[bufferIndex - 1].Append("\n");
                        lines.Add(new());
                    }
                    SetCalculatedCursorPos((Math.Min(lines[bufferIndex].Length, left), bufferIndex));
                }
                else if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (lines[bufferIndex].Length > 0 && left > 0)
                    {
                        lines[bufferIndex] = lines[bufferIndex].Remove(left - 1, 1);
                        //Console.Write(("\r" + (bufferIndex == 0 ? "prompt > " : "") + lines[bufferIndex]).PadRight(Console.BufferWidth));
                        PrintLine(lines[bufferIndex], bufferIndex);
                        SetCalculatedCursorPos((left - 1, bufferIndex));

                    }
                }
                else if (keyInfo.Key == ConsoleKey.Delete)
                {
                    if (0 < lines[bufferIndex].Length && left < lines[bufferIndex].Length)
                    {
                        lines[bufferIndex] = lines[bufferIndex].Remove(left, 1);
                        Console.Write(("\r" + (bufferIndex == 0 ? "prompt > " : "") + lines[bufferIndex]).PadRight(Console.BufferWidth));
                        SetCalculatedCursorPos((left, bufferIndex));

                    }
                }
                else if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    if (bufferIndex > 0)
                    {
                        bufferIndex--;
                        SetCalculatedCursorPos((Math.Min(lines[bufferIndex].Length, left), bufferIndex));
                    }
                }
                else if (keyInfo.Key == ConsoleKey.LeftArrow)
                {
                    if (left > 0)
                    {
                        left--;
                        SetCalculatedCursorPos((Math.Max(0, left), bufferIndex));
                    }
                }
                else if (keyInfo.Key == ConsoleKey.RightArrow)
                {
                    if (left < lines[bufferIndex].Length)
                    {
                        left++;
                        SetCalculatedCursorPos((Math.Min(lines[bufferIndex].Length, left), bufferIndex));
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Home)
                {
                    SetCalculatedCursorPos((0, bufferIndex));
                }
                else if (keyInfo.Key == ConsoleKey.End)
                {
                    SetCalculatedCursorPos((lines[bufferIndex].Length, bufferIndex));
                }


            } while (!(keyInfo.Key == ConsoleKey.Enter && (keyInfo.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control));
            Console.SetCursorPosition(0, topOffset + lines.Count);

            //string? line = string.Empty;
            //while (line != null && !line.EndsWith((char)0x1a))
            //{
            //    line = Console.ReadLine();
            //    sb.AppendLine(line);
            //}

            var prompt = string.Join("", lines.Select(sb => sb.ToString())).TrimEnd('\n');
            Trace.WriteLine(prompt);
            return prompt;
        }

        private static void PrintLine(StringBuilder line, int index)
        {
            Console.Write("\r".PadRight(Console.BufferWidth));
            Console.Write("\r" + (index == 0 ? "prompt > " : "") + line);

        }

        private static bool IsPrintable(char c)
        {
            return char.IsLetter(c) ||
               char.IsLower(c) ||
               char.IsUpper(c) ||
               char.IsWhiteSpace(c) ||
               char.IsPunctuation(c) ||
               char.IsSymbol(c) ||
               char.IsDigit(c) ||
               char.IsNumber(c);
        }
    }
}
