using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;
using FluentValidation;

namespace buildeR.Common.FluentValidators.Shared
{
    public static class SpecialCharactersRules
    {
        private static Regex SpecialCharsExceptRegExr(params char[] chars)
        {
            if (chars.Contains('-'))
            {
                chars = chars.Except(new []{'-'}).ToArray();
                return new Regex($@"[^a-zA-Z0-9-{string.Concat(chars.Select(ch => Regex.Escape(ch.ToString())))}]");
            }
            return new Regex($@"[^a-zA-Z0-9{string.Concat(chars.Select(ch => Regex.Escape(ch.ToString())))}]");
        }

        private static readonly Regex HyphenOnEdges = new Regex(@"^-|-$");
        private static readonly Regex SpacesOnEdges = new Regex(@"^ | $");
        private static readonly Regex DotsOnEdges = new Regex(@"^\.|\.$");
        private static readonly Regex NonLatinLetter = new Regex(@"[^\x00-\x7F]+");

        public static IRuleBuilderOptions<T, string> NoNonLatinLetters<T>(this IRuleBuilder<T, string> rule, params char[] chars)
        {
            return rule
                .Must(input => !NonLatinLetter.IsMatch(input));
        }

        public static IRuleBuilderOptions<T, string> NoSpecialCharsExcept<T>(this IRuleBuilder<T, string> rule, params char[] chars)
        {
            return rule
                .Must(input => !SpecialCharsExceptRegExr(chars).IsMatch(input));
        }

        public static IRuleBuilderOptions<T, string> NoHyphenOnEdges<T>(this IRuleBuilder<T, string> rule)
        {
            return rule
                .Must(input => !HyphenOnEdges.IsMatch(input));
        }

        public static IRuleBuilderOptions<T, string> NoSpacesOnEdges<T>(this IRuleBuilder<T, string> rule)
        {
            return rule
                .Must(input => !SpacesOnEdges.IsMatch(input));
        }

        public static IRuleBuilderOptions<T, string> NoDotsOnEdges<T>(this IRuleBuilder<T, string> rule)
        {
            return rule
                .Must(input => !DotsOnEdges.IsMatch(input));
        }
    }
}