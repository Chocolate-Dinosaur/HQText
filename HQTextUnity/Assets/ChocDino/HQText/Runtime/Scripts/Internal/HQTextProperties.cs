//--------------------------------------------------------------------------//
// Copyright 2024-2024 Chocolate Dinosaur Ltd. All rights reserved.         //
// For full documentation visit https://www.chocolatedinosaur.com           //
//--------------------------------------------------------------------------//

using System;
using UnityEngine;
using UnityEngine.Profiling;

namespace ChocDino.HQText.Internal
{
	/// <summary>
	/// Common Properties and methods shared between  Components
	/// </summary>
	[Serializable]
	public class HQTextProperties
	{
		// Called when property values change
		public event Action OnPropChanged = delegate {};

		/// <summary>
		/// Stores the texture containing the rendered text
		/// </summary>
		//[HideInInspector]
		internal Texture2D Texture;

		/// <summary>
		/// The text to be rendered
		/// </summary>
		[SerializeField, TextAreaAttribute(4, 10)]
		private string _text = string.Empty;

		public HorizontalAlignment InterpretedHorizontalAlignment
		{
			get
			{
				if (!AutoDirection)
				{
					return HorizontalAlignment;
				}
				else
				{
					if (_textInfo.Direction == Direction.RTL || _textInfo.Direction == Direction.WEAK_RTL)
					{
						switch (HorizontalAlignment)
						{
						case HorizontalAlignment.Center:
							return HorizontalAlignment.Center;
						case HorizontalAlignment.Left:
							return HorizontalAlignment.Right;
						case HorizontalAlignment.Right:
							return HorizontalAlignment.Left;
						default:
							return HorizontalAlignment.Left;
						}
					}
					else
					{
						return HorizontalAlignment;
					}
				}
			}
		}

		public HQTextProperties()
		{
			_textBuilder = null;
		}

		private IStringBuilder _textBuilder = new FastStringBuilder();
		public string Text
		{
			get => _text;
			set
			{
				_textBuilder = new FastStringBuilder(value ?? string.Empty);
				_text = value ?? string.Empty;
				// TextInfo = HQTextMethods.GetTextSize(this);
				OnTextChanged?.Invoke(_text);
			}
		}

		public IStringBuilder TextBuilder
		{
			get
			{
				if (_textBuilder == null)
				{
					_textBuilder = new FastStringBuilder(_text != null ? _text : string.Empty);
				}
				return _textBuilder;
			}
		}

		/// <summary>
		/// Used for updating UI when text is updated
		/// </summary>
		public event Action<string> OnTextChanged = delegate {};

		[SerializeField]
		private TextInfo _textInfo;
		public TextInfo TextInfo
		{
			get => _textInfo;
			set => _textInfo = value;
		}

		[SerializeField]
		private Rectangle[] _characterRects;

		public Rectangle[] CharacterRects
		{
			get => _characterRects;
			set => _characterRects = value;
		}

		/// <summary>
		/// If true the direction of the text will be calculated automatically based
		/// on the content. English, for instance, will be left-to-right, while Hebrew
		/// and Arabic characters will be right-to-left.
		/// </summary>
		[SerializeField]
		public bool AutoDirection = true;

		/// <summary>
		/// Manually pick the direction of the text, only used if AutoDirection is set to false.
		/// </summary>
		[SerializeField]
		public Direction Direction = Direction.LTR;

		/// <summary>
		/// The width of the text box in pixels
		/// </summary>
		[SerializeField]
		[MinAttribute(0)]
		public int TextBoxWidth = 100;
		
		/// <summary>
		/// The height of the text box in pixels
		/// </summary>
		[SerializeField]
		[MinAttribute(0)]
		public int TextBoxHeight = 100;

		/// <summary>
		/// optional key reference to an override style
		/// </summary>
		[SerializeField]
		public string StyleName;

		/// <summary>
		/// The color to render the text
		/// </summary>
		[SerializeField]
		public Color TextColor = Color.black;

		/// <summary>
		/// how big to make the font in point
		/// </summary>
		[SerializeField]
		[MinAttribute(1)]
		private int _fontSize = 25;
		public int FontSize
		{
			get => _fontSize;
			set
			{
				_fontSize = value;
				OnPropChanged();
			}
		}

		/// <summary>
		/// If true each line will will stretch to fill the whitespace
		/// </summary>
		[SerializeField]
		public bool Justify = false;

		/// <summary>
		/// Whether the padding should be calculated automatically.
		/// </summary>
		[SerializeField]
		public bool AutoPadding = true;

		/// <summary>
		/// How much space there should be between text area bounds and text.
		/// </summary>
		[SerializeField]
		public TextPadding Padding = new TextPadding();

		/// <summary>
		/// How big to set the gap between lines. If it is 0, then it will ignore it.
		/// </summary>
		[SerializeField]
		public float LineSpacingInPixels = 0f;

		[SerializeField]
		public bool UseMarkup = false;

		[SerializeField]
		public HorizontalWrapping HorizontalWrapping = HorizontalWrapping.Wrap;

		[SerializeField]
		public VerticalWrapping VerticalWrapping = VerticalWrapping.Clip;

		/// <summary>
		/// Which font renderer to use (Freetype or Win32)
		/// </summary>
		[SerializeField]
		public FontBackend FontBackend = FontBackend.FreeType;

		/// <summary>
		/// Where to place the text vertically in the text box.
		/// </summary>
		[SerializeField]
		public VerticalAlignment VerticalAlignment = VerticalAlignment.Top;

		/// <summary>
		/// The name of the font-family to use.
		/// </summary>
		[SerializeField]
		private string _font = "Sans";
		public string Font
		{
			get => _font;
			set
			{
				_font = value;
				OnPropChanged();
			}
		}

		[SerializeField]
		private string _fontFace = "Regular";
		public string FontFace
		{
			get => _fontFace;
			set
			{
				_fontFace = value;
				OnPropChanged();
			}
		}

		/// <summary>
		/// Where to align the text horizontally in the text box
		/// </summary>
		[SerializeField]
		public HorizontalAlignment HorizontalAlignment;

		/// <summary>
		/// Multiply the resolution by this factor to super sample the text.
		/// </summary>
		[SerializeField, Range(1f, 1f)]
		public float ResolutionMultiplier = 1f;

		/// <summary>
		/// The reference to the native instance of the text renderer.
		/// </summary>
		private uint _nativeIndex = 0;

		/// <summary>
		/// Returns the native index
		/// </summary>
		/// <returns>0 if unset, otherwise the index</returns>
		public uint GetNativeIndex()
		{
			return _nativeIndex;
		}

		/// <summary>
		/// Set the native index that this component must reference
		/// </summary>
		/// <param name="i">A vlid index generated by NativePlugin.Initialize()</param>
		public void SetNativeIndex(uint i)
		{
			_nativeIndex = i;
		}

		/// <summary>
		/// Sets the font, font size, and font face all in one call and then calls redraw.
		/// It is recommended to use this method instead of setting the properties individually.
		/// </summary>
		/// <param name="fontSize"></param>
		/// <param name="font"></param>
		/// <param name="fontFace"></param>
		public void SetFontStyle(int fontSize, string font, string fontFace)
		{
			_fontSize = fontSize;
			_font = font;
			_fontFace = fontFace;
			OnPropChanged();
		}

		/// <summary>
		/// Sets the font, font size, and font face all in one call and then calls redraw.
		/// It is recommended to use this method instead of setting the properties individually.
		/// </summary>
		/// <param name="font"></param>
		/// <param name="fontFace"></param>
		public void SetFontStyle(string font, string fontFace)
		{
			_font = font;
			_fontFace = fontFace;
			OnPropChanged();
		}

		/// <summary>
		/// Validates all the settings to make sure that are sensible.
		///
		/// </summary>
		/// <returns>True if all tests pass, otherwise false</returns>
		public bool IsValid()
		{
			Profiler.BeginSample("[HQText] IsValid");
			bool valid = true;

			if (FontSize == 0)
			{
				Debug.LogError("[HQText][Properties Invalid]: font size is 0");
				valid = false;
			}

			if (string.IsNullOrEmpty(Font))
			{
				Debug.LogError("[HQText][Properties Invalid]: font unset");
				valid = false;
			}

			if (string.IsNullOrEmpty(FontFace))
			{
				Debug.LogError("[HQText][Properties Invalid]: font face unset");
				valid = false;
			}
			if (ResolutionMultiplier == 0)
			{
				Debug.LogError("[HQText][Properties Invalid]:resultion multiplier = 0");
				valid = false;
			}

			// Note to Don't use System.Enum.IsDefined to determine if enum is valid, its really slow

			if (!FontBackend.IsValid())
			{
				Debug.LogError("[HQText][Properties Invalid]:FontBackend enum undefined");

				valid = false;
			}
			if (!VerticalAlignment.IsValid())
			{
				Debug.LogError("[HQText][Properties Invalid]:VerticalAlignment enum undefined");

				valid = false;
			}

			if (!HorizontalAlignment.IsValid())
			{
				Debug.LogError("[HQText][Properties Invalid]:HorizontalAlignment enum undefined");

				valid = false;
			}
			Profiler.EndSample();
			return valid;
		}
	}
}