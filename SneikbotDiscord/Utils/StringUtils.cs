namespace SneikbotDiscord.Utils
{
    public static class StringUtils
    {
        public readonly static string[] SentenceEnds = { ".",";", "!", "?", "!?", "?!", "..." };
        public readonly static char[] SentenceEndsChar = { '.', ';', '!', '?' };

        public static bool ContainsEnds(this string value)
        {
            foreach (var end in SentenceEnds) 
            {
                if (value.EndsWith(end)) return true;
            }
            return false;
        }
        public static string ToFirstLetterUp(this string value) 
        {
            var result = $"{char.ToUpper(value[0])}{value.Substring(1)}";
            return result; 
        }
        public static bool isFirstLetterUp(this string value)
        {
            return char.IsUpper(value[0]);
        }
        public static string FormatSentence(this string value)
        {
            var result = value.ToFirstLetterUp();
            if(result.ContainsEnds() == false)
                result = $"{result}.";
            return result;
        }
        public static string RemoveSentenceEnds(this string value)
        {
            //var result = string.Join(null, value.SkipWhile(obj => SentenceEndsChar.Contains(obj)));
            var result = value.TrimEnd(SentenceEndsChar);
            return result;
        }
    }
}
