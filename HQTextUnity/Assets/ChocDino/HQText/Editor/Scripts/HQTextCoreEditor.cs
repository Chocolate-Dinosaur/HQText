//--------------------------------------------------------------------------//
// Copyright 2024-2024 Chocolate Dinosaur Ltd. All rights reserved.         //
// For full documentation visit https://www.chocolatedinosaur.com           //
//--------------------------------------------------------------------------//

using System.Linq;
using UnityEditor;
using UnityEngine;
using ChocDino.HQText.Internal;

namespace ChocDino.HQText.Editor
{
	/// <summary>
	/// HQTextCoreEditor is the primary script for visualizing the test editor controls.
	/// It is worth noting that it has some logic to resolve missing fonts, though it might be need
	/// some work.
	/// </summary>
	[CustomEditor(typeof(HQTextCore), true)]
	// TODO: Fix supports for multiple object editing
	//[CanEditMultipleObjects]	
	public class HQTextCoreEditor : UnityEditor.Editor
	{
		private SerializedProperty _text;
		private SerializedProperty _font;
		private SerializedProperty _fontFace;
		//private SerializedProperty _textBoxWidth;
		//private SerializedProperty _textBoxHeight;
		private SerializedProperty _autoPadding;
		private SerializedProperty _padding;
		private SerializedProperty _fontSize;
		private SerializedProperty _horizontalAlignment;
		private SerializedProperty _autoDirection;
		private SerializedProperty _justify;
		private SerializedProperty _lineSpacingMultiplier;
		private SerializedProperty _horizontalWrapping;
		private SerializedProperty _verticalWrapping;
		//private SerializedProperty _ellipsesOnClip;
		private SerializedProperty _fontBackend;
		private SerializedProperty _verticalAlignment;
		private SerializedProperty _textColor;
		private SerializedProperty _resolutionMultiplier;
		private SerializedProperty _useMarkup;

		private FontManager _fontManager;
		private HQTextCore _instance;
		//private bool _showAdvanced;
		private bool _showControlCharacters;
		//private bool _showHelp;

		private GUIContent _leftAlign, _centerAlign, _rightAlign;
		private GUIContent _topAlign, _middleAlign, _bottomAlign;

		private AnimCollapseSection _sectionFontSettings;
		private AnimCollapseSection _sectionParagraphSettings;
		private AnimCollapseSection _sectionTextBoxSettings;
		private AnimCollapseSection _sectionAdvancedSettings;
		private AnimCollapseSection _sectionHelpSettings;
		private AnimCollapseSection _sectionControlCharacters;

		public void OnEnable()
		{
			_instance = this.target as HQTextCore;
			if (_instance == null)
			{
				throw new System.Exception("[HQText] _instance should never be null!");
			}

			CreateSections();

			SerializedProperty properties = serializedObject.FindProperty("Properties");
			_text = properties.FindPropertyRelative("_text");
			_textColor = properties.FindPropertyRelative("TextColor");
			_font = properties.FindPropertyRelative("_font");
			_fontFace = properties.FindPropertyRelative("_fontFace");
			_fontSize = properties.FindPropertyRelative("_fontSize");
			_horizontalAlignment = properties.FindPropertyRelative("HorizontalAlignment");
			_verticalAlignment = properties.FindPropertyRelative("VerticalAlignment");
			_horizontalWrapping = properties.FindPropertyRelative("HorizontalWrapping");
			_verticalWrapping = properties.FindPropertyRelative("VerticalWrapping");
			_autoDirection = properties.FindPropertyRelative("AutoDirection");
			_justify = properties.FindPropertyRelative("Justify");
			_lineSpacingMultiplier = properties.FindPropertyRelative("LineSpacingInPixels");
			_fontBackend = properties.FindPropertyRelative("FontBackend");
			_resolutionMultiplier = properties.FindPropertyRelative("ResolutionMultiplier");
			//_textBoxWidth = properties.FindPropertyRelative("TextBoxWidth");
			//_textBoxHeight = properties.FindPropertyRelative("TextBoxHeight");
			_autoPadding = properties.FindPropertyRelative("AutoPadding");
			_padding = properties.FindPropertyRelative("Padding");
			_useMarkup = properties.FindPropertyRelative("UseMarkup");

			_leftAlign = EditorGUIUtility.IconContent("align_horizontally_left");
			_centerAlign = EditorGUIUtility.IconContent("align_horizontally_center");
			_rightAlign = EditorGUIUtility.IconContent("align_horizontally_right");
			_topAlign = EditorGUIUtility.IconContent("align_vertically_top");
			_middleAlign = EditorGUIUtility.IconContent("align_vertically_center");
			_bottomAlign = EditorGUIUtility.IconContent("align_vertically_bottom");

			_fontManager = new FontManager(_instance.Properties.FontBackend);
		}

		public void OnDisable()
		{
			if (_sectionFontSettings != null) { _sectionFontSettings.Save(); }
			if (_sectionParagraphSettings != null) { _sectionParagraphSettings.Save(); }
			if (_sectionTextBoxSettings != null) { _sectionTextBoxSettings.Save(); } 
			if (_sectionAdvancedSettings != null) { _sectionAdvancedSettings.Save(); }
			if (_sectionHelpSettings != null) { _sectionHelpSettings.Save(); }
			if (_sectionControlCharacters != null) { _sectionControlCharacters.Save(); }
		}

		public override void OnInspectorGUI()
		{
			AnimCollapseSection.CreateStyles();
			serializedObject.Update();

			EditorGUI.BeginChangeCheck();
			GUI.SetNextControlName("TextArea");
			//_text.stringValue = EditorGUILayout.TextArea(_text.stringValue, GUILayout.Height(80));
			EditorGUILayout.PropertyField(_text);
			EditorGUILayout.Space();

			AnimCollapseSection.Show(_sectionFontSettings);
			AnimCollapseSection.Show(_sectionParagraphSettings);
			AnimCollapseSection.Show(_sectionTextBoxSettings);
			AnimCollapseSection.Show(_sectionControlCharacters);
			AnimCollapseSection.Show(_sectionAdvancedSettings);
			AnimCollapseSection.Show(_sectionHelpSettings);

			if (EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(_instance as MonoBehaviour);
			}

			serializedObject.ApplyModifiedProperties();

			// NOTE: SetTexture() must be called after ApplyModifiedProperties() otherwise the editor doesn't always show the texture right away.
			_instance.SetText(_text.stringValue);
		}

		private void CreateSections()
		{
			const float colorSaturation = 0.66f;
			Color platformColor = Color.HSVToRGB(0.0f, colorSaturation, 1f);
			if (EditorGUIUtility.isProSkin)
			{
				platformColor *= 0.66f;
			}

			//platformColor = Color.white;

			_sectionFontSettings = new AnimCollapseSection("Font Settings", false, true, OnInspectorGUI_FontSettings, this, platformColor);

			platformColor = Color.HSVToRGB(0.25f, colorSaturation, 1f);
			if (EditorGUIUtility.isProSkin)
			{
				platformColor *= 0.66f;
			}

			_sectionParagraphSettings =	new AnimCollapseSection("Paragraph Settings", false, true, OnInspectorGUI_ParagraphSettings, this, platformColor);

			platformColor = Color.HSVToRGB(0.5f, colorSaturation, 1f);
			if (EditorGUIUtility.isProSkin)
			{
				platformColor *= 0.66f;
			}

			_sectionTextBoxSettings = new AnimCollapseSection("Text Box Settings", false, true, OnInspectorGUI_TextBoxSettings, this, platformColor);
			_sectionControlCharacters = new AnimCollapseSection("Control Characters", false, false, OnInspectorGUI_ControlCharacters, this, Color.white);
			_sectionAdvancedSettings = new AnimCollapseSection("Advanced", false, false, OnInspectorGUI_AdvancedSettings, this, Color.white);
			_sectionHelpSettings = new AnimCollapseSection("Help", false, false, OnInspectorGUI_Help, this, Color.white);
		}

		private void OnInspectorGUI_FontSettings()
		{
			Debug.Assert(_fontManager.Fonts.Length > 0, "No font families available!");

			int fontIdx = _fontManager.FontIndex(_font.stringValue);

			if (fontIdx < 0)
			{
				EditorGUILayout.HelpBox("Font Family '" + _font.stringValue + "' not found", MessageType.Error);
			}

			fontIdx = EditorGUILayout.Popup("Font Family", fontIdx, _fontManager.Fonts);

			if (fontIdx >= 0)
			{
				_font.stringValue = _fontManager.Fonts[fontIdx];
				var fontFaces = _fontManager.Faces(_font.stringValue);
				// If the font doesn't contain the current face value, select the first available one.
				if (fontFaces.ToList().IndexOf(_fontFace.stringValue) < 0)
				{
					_fontFace.stringValue = fontFaces[0];
				}

				var faceIdx = EditorGUILayout.Popup("Font Face", fontFaces.ToList().IndexOf(_fontFace.stringValue), fontFaces);
				_fontFace.stringValue = fontFaces[faceIdx];
			}

			EditorGUILayout.PropertyField(_fontSize);
			EditorGUILayout.PropertyField(_useMarkup);
			EditorGUILayout.PropertyField(_textColor);

			EditorGUILayout.Space();
		}

		private void OnInspectorGUI_ControlCharacters()
		{
			//_showControlCharacters = EditorGUILayout.Foldout(_showControlCharacters, "Control Characters");
			//if (_showControlCharacters) {
			string[] buttonLabels = { 	"Left To Right",           "Right To Left",
										"Byte Order Mark",         "Arabic Letter Mark",
										"Left to Right Mark",      "Right To Left Mark",
										"End Directional Isolate", "New Line" };

			int buttonsPerRow = 2;
			int selectedButton = -1;

			GUILayout.BeginHorizontal();
			GUILayout.Space(10);
			GUI.SetNextControlName("ControlCharacterGrid");
			selectedButton = GUILayout.SelectionGrid(selectedButton, buttonLabels, buttonsPerRow);

			GUILayout.Space(10);
			GUILayout.EndHorizontal();

			string[] controlCodes = { "<LTR>",  "<RTL>",  "<ZWNSB>", "<ALM>", "<LTRI>", "<RTLI>", "<PDI>", "<BR>" };
			if (selectedButton >= 0)
			{
				// for some reason if this retains focus it does not update.
				if (GUI.GetNameOfFocusedControl() == "TextArea")
				{
					GUI.FocusControl("ControlCharacterGrid");
				}

				_text.stringValue += controlCodes[selectedButton];
				EditorUtility.SetDirty(_instance as MonoBehaviour);
				_instance.SetText(_text.stringValue);
				serializedObject.ApplyModifiedProperties();
			}
			
			EditorGUILayout.Space();
		}

		private readonly static GUIContent Content_HorizontalAlignment = new GUIContent("Horizontal Alignment");
		private readonly static GUIContent Content_VerticalAlignment = new GUIContent("Vertical Alignment");

		private void OnInspectorGUI_ParagraphSettings()
		{
			GUILayout.BeginHorizontal();

			EditorGUILayout.LabelField(Content_HorizontalAlignment);
			GUI.color = _horizontalAlignment.enumValueIndex == 0 ? Color.cyan : Color.white;
			if (GUILayout.Button(_leftAlign))
			{
				_horizontalAlignment.enumValueIndex = (int)HorizontalAlignment.Left;
			}
			GUI.color = _horizontalAlignment.enumValueIndex == 1 ? Color.cyan : Color.white;
			if (GUILayout.Button(_centerAlign))
			{
				_horizontalAlignment.enumValueIndex = (int)HorizontalAlignment.Center;
			}
			GUI.color = _horizontalAlignment.enumValueIndex == 2 ? Color.cyan : Color.white;
			if (GUILayout.Button(_rightAlign))
			{
				_horizontalAlignment.enumValueIndex = (int)HorizontalAlignment.Right;
			}
			GUI.color = Color.white;
			GUILayout.FlexibleSpace();

			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(Content_VerticalAlignment);
			GUI.color = _verticalAlignment.enumValueIndex == 0 ? Color.cyan : Color.white;
			if (GUILayout.Button(_topAlign))
			{
				_verticalAlignment.enumValueIndex = (int)VerticalAlignment.Top;
			}
			GUI.color = _verticalAlignment.enumValueIndex == 1 ? Color.cyan : Color.white;
			if (GUILayout.Button(_middleAlign))
			{
				_verticalAlignment.enumValueIndex = (int)VerticalAlignment.Middle;
			}
			GUI.color = _verticalAlignment.enumValueIndex == 2 ? Color.cyan : Color.white;
			if (GUILayout.Button(_bottomAlign))
			{
				_verticalAlignment.enumValueIndex = (int)VerticalAlignment.Bottom;
			}
			GUI.color = Color.white;
			GUILayout.FlexibleSpace();

			GUILayout.EndHorizontal();
			EditorGUILayout.PropertyField(_lineSpacingMultiplier);

			EditorGUILayout.Space();
		}

		private void OnInspectorGUI_TextBoxSettings()
		{
			// Update the TextBoxWidth and TextBoxHeight directly from RectTransform

			Vector2 tbSettings = new Vector2();

			AssessAnchors(ref tbSettings);

			_instance.Properties.TextBoxWidth = (int)Mathf.Abs(tbSettings.x);
			_instance.Properties.TextBoxHeight = (int)Mathf.Abs(tbSettings.y);

			EditorGUILayout.PropertyField(_autoPadding);
			if (!_autoPadding.boolValue)
			{
				EditorGUILayout.PropertyField(_padding);
			}

			EditorGUILayout.PropertyField(_horizontalWrapping);
			EditorGUILayout.PropertyField(_verticalWrapping);

			EditorGUILayout.Space();
		}

		/// <summary>
		/// Because the Text Box size is driven by the width and height of the rect transform, we need to
		/// check the rect transforms configuration if it is set to stretch across the canvas we need to
		/// manually set width and height
		/// </summary>
		/// <param name="tbSettings"></param>
		private void AssessAnchors(ref Vector2 tbSettings)
		{
			RectTransform rectTransform = _instance.GetComponent<RectTransform>();
			Rect parentRect = rectTransform.gameObject.GetComponentInParent<Canvas>().pixelRect;

			if (rectTransform == null)
			{
				Debug.LogError("No RectTransform found. Not able to configure text box");
				return;
			}

			tbSettings = rectTransform.sizeDelta;

			if (rectTransform.anchorMin == Vector2.zero && rectTransform.anchorMax == Vector2.one)
			{
				tbSettings = new Vector2(parentRect.width, parentRect.height);
			}
			else if (rectTransform.anchorMin == new Vector2(0f, 0.5f) && rectTransform.anchorMax == new Vector2(1f, 0.5f))
			{
				// stretch horizontal centre
				tbSettings = new Vector2(parentRect.width, rectTransform.rect.height);
			}
			else if (rectTransform.anchorMin == new Vector2(0f, 1f) && rectTransform.anchorMax == Vector2.one)
			{
				// stretch horizontal top
				tbSettings = new Vector2(parentRect.width, rectTransform.rect.height);
			}
			else if (rectTransform.anchorMin == Vector2.zero && rectTransform.anchorMax == new Vector2(1f, 0f))
			{
				// stretch horizontal bottom
				tbSettings = new Vector2(parentRect.width, rectTransform.rect.height);
			}
			else if (rectTransform.anchorMin == new Vector2(0.5f, 0f) && rectTransform.anchorMax == new Vector2(0.5f, 1f))
			{
				// stretch vertical centre
				tbSettings = new Vector2(rectTransform.rect.width, parentRect.height);
			}
			else if (rectTransform.anchorMin == Vector2.zero && rectTransform.anchorMax == new Vector2(0f, 1f))
			{
				// stretch vertical Left
				tbSettings = new Vector2(rectTransform.rect.width, parentRect.height);
			}
			else if (rectTransform.anchorMin == new Vector2(1f, 0f) && rectTransform.anchorMax == Vector2.one)
			{
				// stretch vertical Right
				tbSettings = new Vector2(rectTransform.rect.width, parentRect.height);
			}
			else if (rectTransform.anchorMin == Vector2.zero && rectTransform.anchorMax.x < 1f &&  rectTransform.anchorMax.y == 1f)
			{
				// stretch vertical
				tbSettings.x = rectTransform.rect.width;
			}
			else if (rectTransform.anchorMin == Vector2.zero && rectTransform.anchorMax.x == 1f && rectTransform.anchorMax.y < 1f)
			{
				// stretch horizontal
				tbSettings.y = rectTransform.rect.height;
			}
			else if (rectTransform.anchorMin.x > 0f && rectTransform.anchorMin.y == 0f && rectTransform.anchorMax == Vector2.one)
			{
				// stretch vertical
				tbSettings.x = rectTransform.rect.width;
			}
			else if (rectTransform.anchorMin.x == 0f && rectTransform.anchorMin.y > 0f && rectTransform.anchorMax == Vector2.one)
			{
				// stretch horizontal
				tbSettings.y = rectTransform.rect.height;
			}
		}

		private void OnInspectorGUI_AdvancedSettings()
		{
			EditorGUI.indentLevel = 1;
			EditorGUILayout.PropertyField(_fontBackend);
			if (_instance.Properties.FontBackend == FontBackend.Win32)
			{
				EditorGUILayout.HelpBox(" Currently Win32 backend can only render fonts that are installed, so the machine this build is deployed onto will also need the font installed", MessageType.Info);
			}

			EditorGUILayout.PropertyField(_autoDirection);
			EditorGUILayout.PropertyField(_justify);
			EditorGUILayout.PropertyField(_resolutionMultiplier);

			EditorGUI.indentLevel = 0;

			if (_fontBackend.intValue != (int)_instance.Properties.FontBackend)
			{
				_fontManager.Backend = (FontBackend)_fontBackend.intValue;
				// Ensure we have valid font family and face values after the change.
				if (_fontManager.FontIndex(_font.stringValue) < 0)
				{
					_font.stringValue = _fontManager.Fonts[0];
				}

				var ff = _fontManager.Faces(_font.stringValue).ToList();
				if (ff.IndexOf(_fontFace.stringValue) < 0)
				{
					_fontFace.stringValue = ff[0];
				}
			}

			EditorGUILayout.Space();
		}

		private void OnInspectorGUI_Help()
		{
			if (GUILayout.Button("Website & Documentation"))
			{
				Application.OpenURL("http://www.chocdino.com/products/hqtext/about/");
			}
			if (GUILayout.Button("Markup / Rich Text Documentation"))
			{
				Application.OpenURL("https://docs.gtk.org/Pango/pango_markup.html");
			}
			if (GUILayout.Button("Discord Community"))
			{
				Application.OpenURL("https://discord.gg/wKRzKAHVUE");
			}
			if (GUILayout.Button("Email Us"))
			{
				Application.OpenURL("mailto:support@chocdino.com");
			}
			EditorGUILayout.Space();
		}
	}
}
