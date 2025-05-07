//--------------------------------------------------------------------------//
// Copyright 2024-2024 Chocolate Dinosaur Ltd. All rights reserved.         //
// For full documentation visit https://www.chocolatedinosaur.com           //
//--------------------------------------------------------------------------//

using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using ChocDino.HQText.Internal;

namespace ChocDino.HQText.Editor
{
	[CustomPreview(typeof(HQTextCore))]
	public class HQTextCoreComponentPreview : ObjectPreview
	{
		private readonly static GUIContent _title = new GUIContent("Text Preview");
		public override string GetInfoString() { return "Text Preview"; }
		public override bool HasPreviewGUI() { return true; }
		public override GUIContent GetPreviewTitle() { return _title; }

		/// <summary>
		/// Gets an internal property that lets us determine the scaling factor of the editor.
		/// This is so that we can account for it when rendering the text.
		/// </summary>
		public float pixelsPerPoint
		{
			get
			{
				Type utilityType = typeof(GUIUtility);
				PropertyInfo[] allProps =
					utilityType.GetProperties(BindingFlags.Static | BindingFlags.NonPublic);
				PropertyInfo property = allProps.First(m => m.Name == "pixelsPerPoint");
				float pixelsPerPoint = (float)property.GetValue(null);
				return pixelsPerPoint;
			}
		}

		private bool _invertBackground = false;
		private bool _showCharacterBounds = false;
		private bool _borders = true;
		
		public override void OnPreviewGUI(Rect r, GUIStyle background)
		{
			HQTextCore hqText = (HQTextCore)target;
			var core = hqText;
			if (core.Properties.Texture == null)
			{
				return;
			}

			if (core.Properties.Texture != null)
			{
				Rect textBoxPadding = core.GetTextboxCoordinatesFromParentRect(r);
				Rect textureRect = core.GetTextureCoordinatesInTextBox(textBoxPadding.x, textBoxPadding.y);

				GUILayout.Label($"Texture Size:{core.Properties.Texture.name} {core.Properties.Texture.width}x{core.Properties.Texture.height}");
				GUILayout.Label($"Text Size Logical:{core.Properties.TextInfo.WidthLogical}x{core.Properties.TextInfo.HeightLogical}");
				GUILayout.Label($"Text Size Ink:{core.Properties.TextInfo.WidthInk}x{core.Properties.TextInfo.HeightInk}");
				GUILayout.Label($"Base Direction:{core.Properties.TextInfo.Direction}");
				GUILayout.Label($"Total Lines:{core.Properties.TextInfo.Lines}");
				GUILayout.Label($"Total Characters:{core.Properties.TextInfo.CharacterCount}");
				GUILayout.Label($"Ascent:{core.Properties.TextInfo.Ascent}");
				GUILayout.Label($"Descent:{core.Properties.TextInfo.Descent}");
				GUILayout.Label($"Line Height:{core.Properties.TextInfo.LineHeight}");

				GUI.color = _invertBackground ? new Color(0, 0, 0, 1) : Color.white;
				GUI.DrawTexture(new Rect(r.x, r.y, r.width, r.height), Texture2D.whiteTexture);
				GUI.color = Color.white;

				GUI.DrawTexture(textureRect, hqText.Properties.Texture);

				if (_borders)
				{
					DrawBox(Color.red, (textureRect));
					DrawBox(Color.blue, (textBoxPadding));
				}
				//  DrawBox(Color.yellow, new Rect(r.x + paddingLeft -
				//  hqText.Properties.TextInfo.WidthLogical/2 + hqText.Properties.TextBoxWidth/2,
				//  r.y + paddingTop, hqText.Properties.TextInfo.WidthLogical,
				//  hqText.Properties.TextInfo.HeightLogical));
				if (_showCharacterBounds)
				{
					DrawCharacterRects(hqText, textureRect.x, textureRect.y);
				}
				GUI.matrix = Matrix4x4.identity;
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Invert Background"))
				{
					_invertBackground = !_invertBackground;
				}
				if (GUILayout.Button("Toggle Character Bounds"))
				{
					_showCharacterBounds = !_showCharacterBounds;
				}
				if (GUILayout.Button("Show Borders"))
				{
					_borders = !_borders;
				}
				GUILayout.EndHorizontal();
			}
		}

		private void DrawCharacterRects(HQTextCore core, float paddingLeft, float paddingTop)
		{
			Color c = Color.green;
			Rectangle[] rects = core.Properties.CharacterRects;
			for (int i = 0; i < rects.Length; i++)
			{
				Rectangle r = rects[i];
				DrawBox(c, new Rect(r.X + paddingLeft, r.Y + paddingTop, r.Width, r.Height));
			}
		}

		private void DrawBox(Color color, Rect rect)
		{
			GUI.color = color;
			GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 1), Texture2D.whiteTexture);
			GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height, rect.width, 1), Texture2D.whiteTexture);
			GUI.DrawTexture(new Rect(rect.x, rect.y, 1, rect.height), Texture2D.whiteTexture);
			GUI.DrawTexture(new Rect(rect.x + rect.width, rect.y, 1, rect.height), Texture2D.whiteTexture);
			GUI.color = Color.white;
		}
	}
}