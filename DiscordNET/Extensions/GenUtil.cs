namespace DiscordNET.GeneralUtility
{
	public static class ParseUtility
	{
		public static string CaptFirst(this string phrase) =>
            string.IsNullOrEmpty(phrase) ? null : char.ToUpper(phrase[0]) + phrase.Substring(1);
	}
}
