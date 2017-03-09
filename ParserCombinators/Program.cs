using System;

namespace ParserCombinators
{
    internal static class Program
    {
        private static void Main()
        {
            Console.WriteLine(Parse.A("ABC"));
            Console.WriteLine(Parse.A("ZBC"));
        }
    }

    internal static class Parse
    {
        internal static (bool, string) A(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return (false, string.Empty);
            }

            if (str[0] == 'A')
            {
                return (true, str.Substring(1));
            }

            return (false, str);
        }
    }
}