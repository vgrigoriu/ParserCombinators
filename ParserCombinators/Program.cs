using System;
using System.Collections.Generic;

namespace ParserCombinators
{
    public delegate Result<(T, string)> Parser<T>(string str);

    internal static class Program
    {
        private static void Main()
        {
            Parser<char> aParser = Parse.Char('A');
            Console.WriteLine(aParser("ABC"));
            Console.WriteLine(aParser("ZBC"));
            Console.WriteLine(aParser(string.Empty));

            Parser<char> bParser = Parse.Char('B');
            var aAndBParser = aParser.AndThen(bParser);
            Console.WriteLine(aAndBParser("ABC"));
            Console.WriteLine(aAndBParser("ZBC"));
            Console.WriteLine(aAndBParser("AZC"));

            var aOrBParser = aParser.OrElse(bParser);
            Console.WriteLine(aOrBParser("AZZ"));
            Console.WriteLine(aOrBParser("BZZ"));
            Console.WriteLine(aOrBParser("CZZ"));

            Parser<char> cParser = Parse.Char('C');
            var aAndBOrC = aParser.AndThen(bParser.OrElse(cParser));
            Console.WriteLine(aAndBOrC("ABZ"));
            Console.WriteLine(aAndBOrC("ACZ"));
            Console.WriteLine(aAndBOrC("QBZ"));
            Console.WriteLine(aAndBOrC("AQZ"));
        }
    }

    public static class Parse
    {
        public static Parser<char> Char(char toMatch)
        {
            return str =>
            {
                if (string.IsNullOrEmpty(str))
                {
                    return Failure<char>("No more input");
                }

                var first = str[0];
                if (first == toMatch)
                {
                    return Success.Of((toMatch, str.Substring(1)));
                }

                return Failure<char>($"Expecting '{toMatch}'. Got '{first}'.");
            };
        }

        public static Parser<IEnumerable<T>> AndThen<T>(this Parser<T> parser1, Parser<T> parser2)
        {
            return str =>
            {
                switch (parser1(str))
                {
                    case Failure<(T, string)> f:
                        return Failure<IEnumerable<T>>(f.Message);
                    case Success<(T, string)> s1:
                        (T value1, string remaining1) = s1.Value;
                        switch (parser2(remaining1))
                        {
                            case Failure<(T, string)> f:
                                return Failure<IEnumerable<T>>(f.Message);
                            case Success<(T, string)> s2:
                                (var value2, var remaining2) = s2.Value;
                                return Success.Of((Pair(value1, value2), remaining2));
                        }

                        break;
                }

                return Failure<IEnumerable<T>>("AndThen: switch was not exhaustive");
            };
        }

        public static Parser<T> OrElse<T>(this Parser<T> parser1, Parser<T> parser2)
        {
            return str =>
            {
                switch (parser1(str))
                {
                    case Success<(T, string)> s1:
                        return s1;
                    default:
                        return parser2(str);
                }
            };
        }

        private static IEnumerable<T> Pair<T>(T t1, T t2)
        {
            yield return t1;
            yield return t2;
        }

        private static Failure<(T, string)> Failure<T>(string message)
        {
            return new Failure<(T, string)>(message);
        }
    }

    public abstract class Result<T>
    {
        internal protected Result()
        {
        }
    }

    public sealed class Success<T> : Result<T>
    {
        public Success(T value)
        {
            Value = value;
        }

        public T Value { get; }

        public override string ToString()
        {
            return $"Success {Value}";
        }
    }

    public static class Success
    {
        public static Success<T> Of<T>(T value)
        {
            return new Success<T>(value);
        }
    }

    public sealed class Failure<T> : Result<T>
    {
        public Failure(string message)
        {
            Message = message;
        }

        public string Message { get; }

        public override string ToString()
        {
            return $"Failure \"{Message}\"";
        }
    }
}