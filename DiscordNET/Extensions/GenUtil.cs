namespace DiscordNET.GeneralUtility
{
	public static class ParseUtility
	{
		public static string CaptFirst(this string phrase){
            if (phrase.Length == 0)
                return "";
            else if (phrase.Length == 1)
                return char.ToUpper(phrase[0]).ToString();
            else
                return char.ToUpper(phrase[0]) + phrase.Substring(1);
        }
	}
}
