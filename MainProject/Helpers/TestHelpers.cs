// #define TEST

using System;

namespace MainProject.Helpers
{
	/// <summary>
	/// Appi (preprocessor directive's komeyile) test eden zaman iwledilecek funksionalliqlari saxlayir.
	/// </summary>
	public struct TestHelpers
	{
		/// <summary>
		/// Konsolun gorunuwuyle elaqeli deyiwiklikleri eden metod.
		/// </summary>
		/// <param name="Title">Konsolun bawligi.</param>
		/// <param name="TextColor">Konsolda yazi rengi.</param>
		public static void CustomizeConsole(string Title = "Reconnecter", ConsoleColor TextColor = ConsoleColor.Magenta)
		{
#if TEST
			Console.Title = Title;
			Console.ForegroundColor = TextColor;
#endif
		}
	}
}