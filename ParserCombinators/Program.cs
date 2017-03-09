using System;

namespace ParserCombinators
{
    internal static class Program
    {
        private static void Main()
        {
            Console.WriteLine(Parse.Char('A', "ABC"));
            Console.WriteLine(Parse.Char('A', "ZBC"));
            Console.WriteLine(Parse.Char('A', string.Empty));
        }
    }

    internal static class Parse
    {
        internal static (string, string) Char(char toMatch, string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return ("No more input", string.Empty);
            }

            var first = str[0];
            if (first == toMatch)
            {
                return ($"Found '{toMatch}'", str.Substring(1));
            }

            return ($"Expecting '{toMatch}'. Got '{first}'.", str);
        }
    }
}