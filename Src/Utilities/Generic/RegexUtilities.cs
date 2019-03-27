using System.Text.RegularExpressions;

namespace Ccf.Ck.Utilities.Generic
{

    public enum RegexReplaceType
    {
        Clear,
        Whitespace
    }

    public static class RegexUtilities
    {
        public static string ReplaceText(string fileStr, string pattern, bool multilineMode = true, RegexReplaceType replaceType = RegexReplaceType.Clear)
        {
            MatchEvaluator matchEvaluator;// = new MatchEvaluator(RegexUtilities.ClearText);

            //assign match evaluator
            switch (replaceType)
            {
                case RegexReplaceType.Whitespace:
                    matchEvaluator = new MatchEvaluator(m => { return " "; });
                    break;
                default:
                    matchEvaluator = new MatchEvaluator(m => { return ""; });
                    break;
            }

            //do replace
            return Regex.Replace(fileStr, pattern, matchEvaluator, RegexOptions.IgnorePatternWhitespace | ((multilineMode) ? RegexOptions.Multiline : RegexOptions.Singleline));
        }

        public static bool HasMatch(string text, string pattern)
        {
            Match match = RegexUtilities.Match(text, pattern);
            return match.Success;
        }

        public static Match Match(string text, string pattern)
        {
            return Regex.Match(text, pattern);
        }
    }
}
