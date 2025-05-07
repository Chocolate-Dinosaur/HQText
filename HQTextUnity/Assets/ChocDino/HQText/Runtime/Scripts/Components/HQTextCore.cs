//--------------------------------------------------------------------------//
// Copyright 2024-2024 Chocolate Dinosaur Ltd. All rights reserved.         //
// For full documentation visit https://www.chocolatedinosaur.com           //
//--------------------------------------------------------------------------//

using System;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
using ChocDino.HQText.Internal;

namespace ChocDino.HQText
{
	/// <summary>
	/// The core set of functions that all HQText components needs to have, and is composed into
	/// another class.
	/// </summary>
	[AddComponentMenu("UI/Chocolate Dinosaur UIFX/HQText Core")]
	[DisallowMultipleComponent]
	[ExecuteAlways]
	public class HQTextCore : MonoBehaviour
	{
		/// <summary>
		/// Contains all the Text properties, such as fonts, common to all HQText objects
		/// </summary>
		public HQTextProperties Properties;

		/// <summary>
		/// Command buffer for drawing to texture from native
		/// </summary>
		private CommandBuffer _command;

		public event Action<HQTextProperties> OnRenderedEvent;

		private bool _initialised = false;
		private bool _destroyed = false;
		public void OnEnable()
		{
			if (_initialised)
			{
				return;
			}
			_initialised = true;
			FontHelper.TryInitFontConfig();

			if (Properties == null)
			{
				return;
			}
			Properties.OnPropChanged += Draw;
			Draw();
		}

		public void OnDestroy()
		{
			_destroyed = true;
			Properties.OnPropChanged -= Draw;
			if (Properties.Texture != null)
			{
				if (!Application.isPlaying)
				{
					Object.DestroyImmediate(Properties.Texture);
				}
				else
				{
					Object.Destroy(Properties.Texture);
				}
			}
			if (Properties.GetNativeIndex() > 0)
			{
				NativePlugin.Teardown(Properties.GetNativeIndex());
				Properties.SetNativeIndex(0);
			}
		}

		public void MarkAsDestroyed()
		{
			_destroyed = true;
		}

		public void Draw() 
		{
			var prevRT = RenderTexture.active;

			if (Properties == null || !Properties.IsValid())
			{
				Debug.LogError("[HQTextCore] Invalid Properties");
				return;
			}

			if (Properties.GetNativeIndex() == 0 && !_destroyed)
			{
				Properties.SetNativeIndex(NativePlugin.Initialize());
			}

			if (Properties.GetNativeIndex() == 0 && _destroyed)
			{
				Debug.LogError("[HQTextCore] Object destroyed but init called");
				return;
			}

			if (Properties.GetNativeIndex() == 0)
			{
				Debug.LogError("[HQTextCore] No native index");
				return;
			}

			// ensure that null is converted to the empty string
			if (string.IsNullOrEmpty(Properties.Text))
			{
				Properties.Text = string.Empty;
			}

			if (_command == null)
			{
				_command = new CommandBuffer();
			}

			Profiler.BeginSample("Tags to controlCharacter");
			var text = ControlCharacters.TagsToControlCharacters(Properties.TextBuilder);
			Profiler.EndSample();

			Properties.TextInfo = NativePlugin.SetTextData(
				Properties.GetNativeIndex(), text.ToString(), $"{Properties.Font}", Properties.FontFace,
				Properties.FontSize, Properties.TextBoxWidth, Properties.TextBoxHeight,
				new ColorBlock(Properties.TextColor.r, Properties.TextColor.g, Properties.TextColor.b,
								Properties.TextColor.a),
				Properties.HorizontalAlignment, Properties.LineSpacingInPixels,
				Properties.Justify ? 1 : 0, Properties.AutoDirection ? 1 : 0, Properties.Direction,
				Properties.VerticalAlignment, Properties.FontBackend, Properties.HorizontalWrapping,
				Properties.VerticalWrapping, Properties.UseMarkup ? 1 : 0,
				Properties.ResolutionMultiplier, Properties.AutoPadding ? 1 : 0, Properties.Padding.left,
				Properties.Padding.right, Properties.Padding.top, Properties.Padding.bottom);

			Properties.CharacterRects = NativePlugin.GetCharacterRects(Properties.GetNativeIndex(), Properties.TextInfo.CharacterCount);

			RegenerateTexture();

			if (Properties.Texture == null)
			{
				Debug.LogError("[HQTexCore] No texture generated");
				return;
			}

			// TODO: cache NativePlugin.GetTextureUpdateCallback()
			_command.IssuePluginCustomTextureUpdateV2(NativePlugin.GetTextureUpdateCallback(), Properties.Texture, Properties.GetNativeIndex());
			Graphics.ExecuteCommandBuffer(_command);
			_command.Clear();
			OnRenderedEvent?.Invoke(Properties);

			RenderTexture.active = prevRT;
		}

		/// <summary>
		/// Use to update the text at runtime
		/// </summary>
		/// <param name="text"></param>
		public void SetText(string text) 
		{
			//if  (text != Properties.Text)
			{
				Properties.Text = text;
				Draw();
			}
		}

		public string GetText()
		{
			return Properties.Text;
		}

		public Vector2Int GetTextureSize()
		{
			return new Vector2Int(Properties.TextInfo.Width, Properties.TextInfo.Height);
		}

		public void RegenerateTexture()
		{
			Vector2Int textureSize = GetTextureSize();
			HQTextMethods.RegenerateTexture(Properties, textureSize.x, textureSize.y);
		}

		/// <summary>
		/// Given a parent rect, align the textbox based on the alignment settings.
		/// </summary>
		/// <param name="parent"></param>
		/// <returns></returns>
		public Rect GetTextboxCoordinatesFromParentRect(Rect parent)
		{
			float paddingLeft = 0f;
			if (Properties.InterpretedHorizontalAlignment == HorizontalAlignment.Left)
			{
				paddingLeft = 0f;
			}
			else if (Properties.InterpretedHorizontalAlignment == HorizontalAlignment.Center)
			{
				paddingLeft = (parent.width - Properties.TextBoxWidth) / 2f;
			}
			else if (Properties.InterpretedHorizontalAlignment == HorizontalAlignment.Right)
			{
				paddingLeft = parent.width - Properties.TextBoxWidth;
			}

			float paddingTop = 0f;
			if (Properties.VerticalAlignment == VerticalAlignment.Top)
			{
				paddingTop = 0f;
			}
			else if (Properties.VerticalAlignment == VerticalAlignment.Middle)
			{
				paddingTop = (parent.height - Properties.TextBoxHeight) / 2f;
			}
			else if (Properties.VerticalAlignment == VerticalAlignment.Bottom)
			{
				paddingTop = parent.height - Properties.TextBoxHeight;
			}

			return new Rect(parent.x + paddingLeft, parent.y + paddingTop, Properties.TextBoxWidth, Properties.TextBoxHeight);
		}

		/// <summary>
		/// A method that returns the position of the texture within the textbox, based on the
		/// alignment.
		/// </summary>
		/// <param name="parent"></param>
		/// <returns></returns>
		public Rect GetTextureCoordinatesInTextBox(float offsetX, float offsetY, bool flipV = false)
		{
			if (Properties.Texture == null)
			{
				return default(Rect);
			}

			float halfWidth = Properties.TextBoxWidth / 2f;
			float halfTextureWidth = Properties.Texture.width / 2f;

			float halfHeight = Properties.TextBoxHeight / 2f;
			float halfTextureHeight = Properties.Texture.height / 2f;
			float paddingLeft = 0f;
			float paddingTop = 0f;

			VerticalAlignment v = Properties.VerticalAlignment;
			if (flipV)
			{
				if (v == VerticalAlignment.Top)
				{
					v = VerticalAlignment.Bottom;
				}
				else if (v == VerticalAlignment.Bottom)
				{
					v = VerticalAlignment.Top;
				}
			}
			if (Properties.InterpretedHorizontalAlignment == HorizontalAlignment.Center)
			{
				paddingLeft = halfWidth - halfTextureWidth;
			}
			if (Properties.InterpretedHorizontalAlignment == HorizontalAlignment.Left)
			{
				paddingLeft = 0f;
			}

			if (Properties.InterpretedHorizontalAlignment == HorizontalAlignment.Right)
			{
				paddingLeft = Properties.TextBoxWidth - Properties.Texture.width;
			}

			if (v == VerticalAlignment.Top)
			{
				paddingTop = 0f;
			}
			if (v == VerticalAlignment.Middle)
			{
				paddingTop = halfHeight - halfTextureHeight;
			}
			if (v == VerticalAlignment.Bottom)
			{
				paddingTop = Properties.TextBoxHeight - Properties.Texture.height;
			}

			return new Rect(paddingLeft + offsetX, paddingTop + offsetY, Properties.Texture.width, Properties.Texture.height);
		}

		public Rect GetTextTextureRect(Rect uiRect)
		{
			if (Properties.Texture == null)
			{
				return default;
			}

			int texWidth = Properties.Texture.width;
			int texHeight = Properties.Texture.height;
			float offsetX = 0f;
			if (Properties.InterpretedHorizontalAlignment == HorizontalAlignment.Center)
			{
				offsetX = (uiRect.width - texWidth) / 2f;
			}
			else if (Properties.InterpretedHorizontalAlignment == HorizontalAlignment.Right)
			{
				offsetX = uiRect.width - texWidth;
			}
			float offsetY = 0f;
			if (Properties.VerticalAlignment == VerticalAlignment.Top)
			{
				offsetY = uiRect.height - texHeight;
			}
			if (Properties.VerticalAlignment == VerticalAlignment.Middle)
			{
				offsetY = (uiRect.height - texHeight) / 2f;
			}

			return new Rect(uiRect.x + offsetX, uiRect.y + offsetY, texWidth, texHeight);
		}
	}
}
