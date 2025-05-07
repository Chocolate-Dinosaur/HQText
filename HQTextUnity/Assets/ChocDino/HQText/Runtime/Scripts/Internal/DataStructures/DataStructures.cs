using System;
using System.Runtime.InteropServices;

namespace ChocDino.HQText.Internal
{
	// Needs to match PangoDirection
	public enum Direction
	{
		LTR = 0,
		RTL = 1,
		TTB_LTR = 2,  // deprecated
		TTB_RTL = 3,  // deprecated
		WEAK_LTR = 4,
		WEAK_RTL = 5,
		NEUTRAL = 6
	}
	[Serializable]
	public struct TextInfo
	{
		public int Width;
		public int Height;
		public int WidthLogical;
		public int HeightLogical;
		public int WidthInk;
		public int HeightInk;
		public Direction Direction;
		public int Lines;
		public int CharacterCount;
		public int Ascent;
		public int Descent;
		public int LineHeight;
	}
	[Serializable]
	public struct TextPadding
	{
		public int left;
		public int right;
		public int top;
		public int bottom;
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct Rectangle
	{
		public int X;
		public int Y;
		public int Width;
		public int Height;

		public override string ToString() { return $"Rectangle [x={X},y={Y},w={Width},h={Height}]"; }
	}

	/// <summary>
	/// Which way the text should run
	/// </summary>
	public enum Gravity
	{
		NOT_ROTATED,
		SOUTH,
		UPSIDE_DOWN,
		NORTH,
		ROTATED_LEFT,
		EAST,
		ROTATED_RIGHT,
		WEST

	}

	// The following words are understood as gravity values: "Not-Rotated", "South", "Upside-Down",
	// "North", "Rotated-Left", "East", "Rotated-Right", "West".

	public static class GravityExtensions
	{
		public static bool IsValid(this Gravity e)
		{
			switch (e)
			{
				case Gravity.NOT_ROTATED:
				case Gravity.SOUTH:
				case Gravity.UPSIDE_DOWN:
				case Gravity.NORTH:
				case Gravity.ROTATED_LEFT:
				case Gravity.EAST:
				case Gravity.ROTATED_RIGHT:
				case Gravity.WEST:
				return true;
				default:
				return false;
			}
		}

		public static string ToFriendlyName(this Gravity gravity)
		{
			switch (gravity)
			{
				case Gravity.NOT_ROTATED:
				return "Not-Rotated";
				case Gravity.SOUTH:
				return "South";

				case Gravity.UPSIDE_DOWN:
				return "Upside-Down";

				case Gravity.NORTH:
				return "North";

				case Gravity.ROTATED_LEFT:
				return "Rotated-Left";

				case Gravity.ROTATED_RIGHT:
				return "Rotated-Right";

				case Gravity.WEST:
				return "West";

				default:
				return string.Empty;
			}
		}
	}
	/// <summary>
	/// A RGBA colour struct to define a text colour
	/// </summary>
	public struct ColorBlock
	{
		public readonly double r;
		public readonly double g;
		public readonly double b;
		public readonly double a;

		public ColorBlock(double r, double g, double b, double a)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}
	}

	/// <summary>
	/// Vertically align text in a text block
	/// </summary>
	public enum VerticalAlignment { Top = 0, Middle = 1, Bottom = 2 }

	public static class VerticalAlignmentExtensions
	{
		public static bool IsValid(this VerticalAlignment e)
		{
			switch (e)
			{
				case VerticalAlignment.Top:
				case VerticalAlignment.Middle:
				case VerticalAlignment.Bottom:
				return true;
				default:
				return false;
			}
		}
	}
	/// <summary>
	/// Which font backend to use, Freetype is default.
	/// </summary>
	public enum FontBackend
	{

		FreeType = 1,
		Win32 = 2,
		//Quartz = 3,
	}

	public static class FontBackendExtensions
	{
		public static bool IsValid(this FontBackend e)
		{
			switch (e)
			{
				case FontBackend.FreeType:
		#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
				//case FontBackend.Quartz:
		#endif
		#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
				case FontBackend.Win32:
		#endif
				return true;
				default:
				return false;
			}
		}
	}

	/// <summary>
	/// Horizontal alignment
	/// </summary>
	public enum HorizontalAlignment { Left = 0, Center = 1, Right = 2 }

	public static class TextAlignmentExtensions
	{
		public static bool IsValid(this HorizontalAlignment e)
		{
			switch (e)
			{
				case HorizontalAlignment.Left:
				case HorizontalAlignment.Center:
				case HorizontalAlignment.Right:
				return true;
				default:
				return false;
			}
		}
	}
	/// <summary>
	/// The following words are understood as weights: "Thin", "Ultra-Light", "Extra-Light", "Light",
	/// "Semi-Light", "Demi-Light", "Book", "Regular", "Medium", "Semi-Bold", "Demi-Bold", "Bold",
	/// "Ultra-Bold", "Extra-Bold", "Heavy", "wBlack", "Ultra-Black", "Extra-Black".
	/// </summary>
	public enum Weight
	{
		THIN = 100,
		ULTRALIGHT = 200,
		LIGHT = 300,
		SEMILIGHT = 350,
		BOOK = 380,
		NORMAL = 400,
		MEDIUM = 500,
		SEMIBOLD = 600,
		BOLD = 700,
		ULTRABOLD = 800,
		HEAVY = 900,
		BLACK = 1000,
		ULTRABLACK = 1001,
		EXTRABLACK = 1002,
	}

	public static class WeightExtensions
	{
		public static bool IsValid(this Weight e)
		{
			switch (e)
			{
				case Weight.THIN:
				case Weight.ULTRALIGHT:
				case Weight.LIGHT:
				case Weight.SEMILIGHT:
				case Weight.BOOK:
				case Weight.NORMAL:
				case Weight.MEDIUM:
				case Weight.SEMIBOLD:
				case Weight.BOLD:
				case Weight.ULTRABOLD:
				case Weight.HEAVY:
				case Weight.BLACK:
				case Weight.ULTRABLACK:
				case Weight.EXTRABLACK:

				return true;
				default:
				return false;
			}
		}

		public static string ToFriendlyName(this Weight stretch)
		{
			switch (stretch)
			{
				case Weight.THIN:
				return "Thin";
				case Weight.ULTRALIGHT:
				return "Ultra-Light";
				case Weight.LIGHT:
				return "Light";
				case Weight.BOOK:
				return "Book";
				case Weight.NORMAL:
				return "Regular";
				case Weight.MEDIUM:
				return "Medium";
				case Weight.SEMIBOLD:
				return "Semi-Bold";
				case Weight.BOLD:
				return "Bold";
				case Weight.ULTRABOLD:
				return "Ultra-Bold";
				case Weight.HEAVY:
				return "Heavy";
				case Weight.BLACK:
				return "Black";
				case Weight.ULTRABLACK:
				return "Ultra-Black";
				case Weight.EXTRABLACK:
				return "Extra-Black";
				default:
				return string.Empty;
			}
		}
	}

	// The following words are understood as stretch values: "Ultra-Condensed", "Extra-Condensed",
	// "Condensed", "Semi-Condensed", "Semi-Expanded", "Expanded", "Extra-Expanded", "Ultra-Expanded".
	public enum Stretch
	{
		ULTRA_CONDENSED,
		EXTRA_CONDENSED,
		CONDENSED,
		SEMI_CONDENSED,
		NORMAL,
		SEMI_EXPANDED,
		EXPANDED,
		EXTRA_EXPANDED,
		ULTRA_EXPANDED
	}

	public static class StretchExtensions
	{
		public static bool IsValid(this Stretch e)
		{
			switch (e)
			{
				case Stretch.ULTRA_CONDENSED:
				case Stretch.EXTRA_CONDENSED:
				case Stretch.CONDENSED:
				case Stretch.NORMAL:
				case Stretch.SEMI_EXPANDED:
				case Stretch.EXPANDED:
				case Stretch.EXTRA_EXPANDED:
				case Stretch.ULTRA_EXPANDED:
				return true;
				default:
				return false;
			}
		}

		public static string ToFriendlyName(this Stretch stretch)
		{
			switch (stretch)
			{
				case Stretch.ULTRA_CONDENSED:
				return "Ultra-Condensed";
				case Stretch.EXTRA_CONDENSED:
				return "Extra-Rotated";
				case Stretch.CONDENSED:
				return "Condensed";
				case Stretch.SEMI_CONDENSED:
				return "Semi-Condensed";
				case Stretch.NORMAL:
				return string.Empty;
				case Stretch.SEMI_EXPANDED:
				return "Semi-Expanded";
				case Stretch.EXPANDED:
				return "Expanded";
				case Stretch.EXTRA_EXPANDED:
				return "Extra-Expanded";
				case Stretch.ULTRA_EXPANDED:
				return "Ultra-Expanded";
				default:
				return string.Empty;
			}
		}
	}

	// The following words are understood as variants: "Small-Caps".
	public enum Variant { NORMAL, SMALL_CAPS }
	public static class VariantExtensions
	{
		public static bool IsValid(this Variant e)
		{
			switch (e)
			{
				case Variant.SMALL_CAPS:
				case Variant.NORMAL:
				return true;
				default:
				return false;
			}
		}
		public static string ToFriendlyName(this Variant variant)
		{
			switch (variant)
			{
				case Variant.SMALL_CAPS:
				return "Small-Caps";
				default:
				return string.Empty;
			}
		}
	}

	// The following words are understood as styles: "Normal", "Roman", "Oblique", "Italic".
	public enum Style { NORMAL, ROMAN, OBLIQUE, ITALIC }

	public static class StyleExtensions
	{
		public static bool IsValid(this Style e)
		{
			switch (e)
			{
				case Style.NORMAL:
				case Style.ROMAN:
				case Style.OBLIQUE:
				case Style.ITALIC:
				return true;
				default:
				return false;
			}
		}
		public static string ToFriendlyName(this Style style)
		{
			switch (style)
			{
				case Style.NORMAL:
				return "Normal";
				case Style.ROMAN:
				return "Roman";
				case Style.OBLIQUE:
				return "Oblique";
				case Style.ITALIC:
				return "Italic";
				default:
				return string.Empty;
			}
		}
	}

	public enum HorizontalWrapping { Wrap, Clip, Expand }

	public enum VerticalWrapping { Clip, Expand }
}