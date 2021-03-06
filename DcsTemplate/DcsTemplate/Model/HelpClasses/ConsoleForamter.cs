using System;

namespace DcsTemplate.Model.HelpClasses
{
    public class ConsoleForamter
    {
        /// <summary>
        /// Centers the console.
        /// </summary>
        /// <param name="centerString">The text.</param>
        public static void Center(string centerString)
        {
            try
            {
                Console.Write("██");
                Console.SetCursorPosition((Console.WindowWidth - centerString.Length) / 2, Console.CursorTop);
                Console.Write(centerString);
                Console.SetCursorPosition((Console.WindowWidth - 4), Console.CursorTop);
                Console.WriteLine("██");
            }
            catch
            {
                Console.Write("Console to small!");
                Console.SetCursorPosition((Console.WindowWidth - centerString.Length) / 2, Console.CursorTop);
                Console.Write(centerString);
                Console.SetCursorPosition((Console.WindowWidth - 4), Console.CursorTop);
                Console.WriteLine("██");
            }
        }
        public static void FillRow()
        {
            Console.WriteLine($"{"".PadRight(Console.WindowWidth - 2, '█')}");
        }
    }
}

