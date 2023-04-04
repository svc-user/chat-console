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
                var (left, top) = GetCalculatedCursorPos();
                keyInfo = Console.ReadKey(true);
                if (keyInfo.KeyChar <= 126 && 32 <= keyInfo.KeyChar)
                {
                    lines[bufferIndex].Insert(left, keyInfo.KeyChar);
                    Console.Write(("\r" + (top == 0 ? "prompt > " : "") + lines[bufferIndex]).PadRight(Console.BufferWidth));
                    SetCalculatedCursorPos((left + 1, top));
                }
                else if (keyInfo.Key == ConsoleKey.Enter || keyInfo.Key == ConsoleKey.DownArrow)
                {
                    bufferIndex++;
                    if (lines.Count - 1 < bufferIndex)
                    {
                        lines.Add(new());
                    }
                    SetCalculatedCursorPos((Math.Min(lines[bufferIndex].Length, left), bufferIndex));
                }
                else if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (lines[bufferIndex].Length > 0)
                    {
                        lines[bufferIndex] = lines[bufferIndex].Remove(left - 1, 1);
                        Console.Write(("\r" + (top == 0 ? "prompt > " : "") + lines[bufferIndex]).PadRight(Console.BufferWidth));
                        SetCalculatedCursorPos((left - 1, top));

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

            } while (!(keyInfo.Key == ConsoleKey.Enter && (keyInfo.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control));
            Console.SetCursorPosition(0, topOffset + lines.Count);

            //string? line = string.Empty;
            //while (line != null && !line.EndsWith((char)0x1a))
            //{
            //    line = Console.ReadLine();
            //    sb.AppendLine(line);
            //}

            var prompt = string.Join("\n", lines.Select(sb => sb.ToString())).TrimEnd('\n');
            Trace.WriteLine(prompt);
            return prompt;
        }
    }
}
