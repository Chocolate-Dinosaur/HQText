//--------------------------------------------------------------------------//
// Copyright 2024-2024 Chocolate Dinosaur Ltd. All rights reserved.         //
// For full documentation visit https://www.chocolatedinosaur.com           //
//--------------------------------------------------------------------------//

using System.Collections.Generic;
using System.Text;

namespace ChocDino.HQText.Internal
{
	/// <summary>
	/// Helper class to assist with handling control characters. Control characters are special
	/// characters that change how the text is rendered but aren't visible themselves
	/// </summary>
	public static class ControlCharacters
	{
		/// <summary>
		/// Mapping of control characters to tags
		/// </summary>
		public static List<KeyValuePair<string, char>> ControlCharacterLUT = new List<KeyValuePair<string, char>>()
		{
			new KeyValuePair<string, char>(
				"<LTR>",
				'\u200E'),  // Left to right mark goes at the front of the string to determine
							// directionality. You probably don't need this
			new KeyValuePair<string, char>(
				"<RTL>",
				'\u200F'),  // Right to left mark goes at the front of the string to determine
							// directionality. You probably don't need this
			new KeyValuePair<string, char>("<ZWNSB>",
											'\uFEFF'),  // Use to break arabic letters that should join
														// without using a space. Also Bite Order Mark
			new KeyValuePair<string, char>(
				"<ALM>",
				'\u061C'),  // An arabic letter mark to fool the system into thinking that there is an
							// Arabic letter - often placed before number sequences without Arabic
							// that should be Bidi'd to RTL
			new KeyValuePair<string, char>(
				"<LTRI>",
				'\u2066'),  // Left to right mark control character meant meant to distinguish a piece
							// of text from its surroundings
			new KeyValuePair<string, char>(
				"<RTLI>",
				'\u2068'),  // RightToLeft mark control character meant meant to distinguish a piece
							// of text from its surroundings
			new KeyValuePair<string, char>("<PDI>", '\u2069'),  // End Directional Isolate section
			new KeyValuePair<string, char>("<BR>", '\r')
		};

		/// <summary>
		/// Returns true if character unicode matches a known control character
		/// </summary>
		/// <param name="c">Character to check</param>
		/// <returns>Returns true if character unicode matches a known control character</returns>
		public static bool IsControlCharacter(char c)
		{
			return 	c == '\u200E' || c == '\u200F' || c == '\uFEFF' || c == '\u061C' || c == '\u2066' ||
					c == '\u2066' || c == '\u2068' || c == '\u2069' || c == '\r';
		}

		/// <summary>
		/// Convert tags e.g. <LTRI>11234</LTRI> to control characters eg
		/// \u2066 11234  \u2069
		/// </summary>
		/// <param name="input">string with tags</param>
		/// <returns>string with control characters</returns>
		public static IStringBuilder TagsToControlCharacters(IStringBuilder input)
		{
			IStringBuilder s = input;
			foreach (var kvp in ControlCharacterLUT)
			{
				s.Replace(kvp.Key, kvp.Value);
			}
			return s;
		}

		/// <summary>
		/// Removes known control character from input string.
		/// </summary>
		/// <param name="input">String to process</param>
		/// <returns>a string with stripped characters</returns>
		public static string StripControlCharacters(string input)
		{
			StringBuilder sb = new StringBuilder();
			foreach (char c in input)
			{
				if (!IsControlCharacter(c))
				{
					sb.Append(c);
				}
			}
			return sb.ToString();
		}
	}
}