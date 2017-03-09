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

    public static class Parse
    {
        public static Result<(char, string)> Char(char toMatch, string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return Failure("No more input");
            }

            var first = str[0];
            if (first == toMatch)
            {
                return Success.Of((toMatch, str.Substring(1)));
            }

            return Failure($"Expecting '{toMatch}'. Got '{first}'.");
        }

        private static Failure<(char, string)> Failure(string message)
        {
            return new Failure<(char, string)>(message);
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