using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ParserCombinators
{
    public delegate Result<(T value, string remaining)> Parser<T>(string str);

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

            var parseLowercase = Parse.AnyOf("abcdefghijklmnopqrstuvwxyz");
            Console.WriteLine(parseLowercase("aBC"));
            Console.WriteLine(parseLowercase("ABC"));

            var parseDigit = Parse.AnyOf("0123456789");
            var parseThreeDigits = parseDigit.AndThen(parseDigit).AndThen(parseDigit);
            Console.WriteLine(parseThreeDigits("135Z"));

            var threeDigitsStringParser = parseThreeDigits.Select(CharTupleToString);
            Console.WriteLine(threeDigitsStringParser("135Z"));

            var threeDigitsIntParser = threeDigitsStringParser.Select(int.Parse);
            Console.WriteLine(threeDigitsIntParser("135Z"));

            var parseThenAdd = Parse.Lift2((x, y) => x + y, threeDigitsIntParser, threeDigitsIntParser);
            Console.WriteLine(parseThenAdd("100456"));

            string CharTupleToString(((char c1, char c2) t1, char c3) p)
            {
                return new String(new[] { p.t1.c1, p.t1.c2, p.c3 });
            }
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

        public static Parser<(T1, T2)> AndThen<T1, T2>(this Parser<T1> parser1, Parser<T2> parser2)
        {
            return str =>
            {
                switch (parser1(str))
                {
                    case Failure<(T1, string)> f:
                        return Failure<(T1, T2)>(f.Message);
                    case Success<(T1, string)> s1:
                        (T1 value1, string remaining1) = s1.Value;
                        switch (parser2(remaining1))
                        {
                            case Failure<(T2, string)> f:
                                return Failure<(T1, T2)>(f.Message);
                            case Success<(T2, string)> s2:
                                (var value2, var remaining2) = s2.Value;
                                return Success.Of(((value1, value2), remaining2));
                        }

                        break;
                }

                return Failure<(T1, T2)>("AndThen: switch was not exhaustive");
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

        public static Parser<T> Choice<T>(this IEnumerable<Parser<T>> parsers)
        {
            return parsers.Aggregate(OrElse);
        }

        public static Parser<char> AnyOf(IEnumerable<char> chars)
        {
            return chars.Select(Char).Choice();
        }

        public static Parser<T2> Select<T1, T2>(this Parser<T1> parser, Func<T1, T2> f)
        {
            return str =>
            {
                switch (parser(str)) {
                    case Failure<(T1, string)> failure:
                        return Failure<T2>(failure.Message);
                    case Success<(T1, string)> success:
                        (T1 value, string remaining) = success.Value;
                        return Success.Of((f(value), remaining));
                    default:
                        return Failure<T2>("Map: switch was not exhaustive");
                }
            };
        }

        public static Parser<T> Return<T>(T value)
        {
            return str => Success.Of((value, str));
        }

        public static Parser<T2> Apply<T1, T2>(this Parser<T1> parserOfT1, Parser<Func<T1, T2>> parserOfFunc)
        {
            Parser<(Func<T1, T2>, T1)> parserOfFAndX = parserOfFunc.AndThen(parserOfT1);
            return parserOfFAndX.Select<(Func<T1, T2>, T1), T2>(((Func<T1, T2> f, T1 x) p) => p.f(p.x));
        }

        public static Parser<TReturn> Lift2<T1, T2, TReturn>(Func<T1, Func<T2, TReturn>> f, Parser<T1> x, Parser<T2> y)
        {
            return y.Apply(x.Apply(Return(f)));
        }

        public static Parser<TReturn> Lift2<T1, T2, TReturn>(Func<T1, T2, TReturn> f, Parser<T1> xP, Parser<T2> yP)
        {
            return Lift2<T1, T2, TReturn>(x => y => f(x, y), xP, yP);
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