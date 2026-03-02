namespace affolterNET.Data.Extensions
{
    public static class StringExtensions
    {
        public static string StripSquareBrackets(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            if (input.StartsWith("["))
            {
                input = input.Substring(1);
            }

            if (input.EndsWith("]"))
            {
                input = input.Substring(0, input.Length - 1);
            }

            return input;
        }

        public static string EnsureSquareBrackets(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            if (!input.StartsWith("["))
            {
                input = $"[{input}";
            }

            if (!input.EndsWith("]"))
            {
                input = $"{input}]";
            }

            return input;
        }

        public static string StripQuoting(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            // Handle [brackets]
            if (input.StartsWith("[") && input.EndsWith("]"))
            {
                return input.Substring(1, input.Length - 2);
            }

            // Handle "double-quotes"
            if (input.StartsWith("\"") && input.EndsWith("\""))
            {
                return input.Substring(1, input.Length - 2);
            }

            return input;
        }

        public static string EnsureQuoting(this string input, QuoteStyle style)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            // Strip any existing quoting first
            var stripped = input.StripQuoting();

            return style switch
            {
                QuoteStyle.Brackets => $"[{stripped}]",
                QuoteStyle.DoubleQuotes => $"\"{stripped}\"",
                _ => stripped
            };
        }
    }
}
