//--------------------------------------------------------------------------//
// Copyright 2024-2024 Chocolate Dinosaur Ltd. All rights reserved.         //
// For full documentation visit https://www.chocolatedinosaur.com           //
//--------------------------------------------------------------------------//

using System;
using UnityEngine;
using UnityEngine.UI;
using ChocDino.HQText.Internal;

namespace ChocDino.HQText
{
	/// <summary>
	/// This component renders HQText Textures to a UI component.
	/// Typically you will apply this component on an object that has an HQTextCore component on it.
	/// </summary>
	[RequireComponent(typeof(CanvasRenderer))]
	[RequireComponent(typeof(HQTextCore))]
	[DisallowMultipleComponent]
	[ExecuteAlways]
	[AddComponentMenu("UI/Chocolate Dinosaur UIFX/HQText (UGUI)")]
	public class HQTextUGUI : MaskableGraphic
	{
		[SerializeField, Range(0f, 1f)] float _revealLetters = 1f;

		[Header("Debug")]

		//[SerializeField]
		private Texture _referenceTexture = null;
		private HQTextCore _coreComponent = null;

		protected override void OnDisable()
		{
			_coreComponent.OnRenderedEvent -= OnRenderedEvent;
			SetVerticesDirty();
			SetMaterialDirty();
			base.OnDisable();
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			if (_coreComponent == null)
			{
				_coreComponent = GetComponent<HQTextCore>();
				// If it's still null, don't continue
				if (_coreComponent == null)
				{
					return;
				}
			}

			_coreComponent.OnRenderedEvent += OnRenderedEvent;
			_coreComponent.OnEnable();
		}

		void OnRenderedEvent(HQTextProperties p)
		{
			//_referenceTexture = p.Texture;
			texture = p.Texture;
			SetNativeSize();
			//SetVerticesDirty();
			//SetMaterialDirty();
		}

		public Texture texture
		{
			get { return _referenceTexture; }
			set
			{
				// NOTE: we can't use this simplistic check as the texture may be reused when dimensions don't change (eg when only changing alignment)
				//if (_referenceTexture == value)
				//	return;
	
				_referenceTexture = value;
				SetMaterialDirty();
				SetVerticesDirty();
			}
		}
		public override Texture mainTexture
		{
			get
			{
				if (_referenceTexture == null)
				{
					if (material != null && material.mainTexture != null)
					{
						return material.mainTexture;
					}
					return s_WhiteTexture;
				}
				return _referenceTexture;
			}
		}

		/// <summary>
		/// Public property to adjust the noramalized percentage of letter to be shown, used predominantly to create a typewriter effect
		/// </summary>
		public float RevealLetters
		{
			get => _revealLetters;
			set
			{
				_revealLetters = value;
				SetVerticesDirty();
			}
		}

		/// <summary>
		/// Adjust the scale of the Graphic to make it pixel-perfect.
		/// </summary>
		/// <remarks>
		/// This means setting the RawImage's RectTransform.sizeDelta  to be equal to the Texture
		/// dimensions.
		/// </remarks>
		public override void SetNativeSize()
		{
			Texture tex = mainTexture;
			if (tex != null)
			{
				int w = _coreComponent.Properties.TextBoxWidth;
				int h = _coreComponent.Properties.TextBoxHeight;
				rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
				rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
			}
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();

			if (_coreComponent == null)
			{
				Debug.LogError("[HQText] Unassigned core component");
				return;
			}

			var tex = _coreComponent.Properties.Texture;
			if (tex == null)
			{
				Debug.LogError("[HQText] Unassigned texture");
				return;
			}

			Rect rect = GetPixelAdjustedRect();
			Rect textRect = _coreComponent.GetTextTextureRect(rect);

			// Render the whole rectangle if all letters should be shown.
			// This shouldn't really be necessary, but the rectangles we currently get
			// from Pango don't cover the characters completely.
			if (_revealLetters >= 1.0f)
			{
				Func<Vector2, Vector2> posToUV = p =>
					new Vector2((p.x - textRect.x) / tex.width, (p.y - textRect.y) / tex.height);
				var left = textRect.x;
				var right = textRect.x + textRect.width;
				var bot = textRect.y;
				var top = textRect.y + textRect.height;
				var color32 = color;
				var v = new Vector3(left, bot);
				vh.AddVert(v, color32, posToUV(v));
				v = new Vector3(left, top);
				vh.AddVert(v, color32, posToUV(v));
				v = new Vector3(right, top);
				vh.AddVert(v, color32, posToUV(v));
				v = new Vector3(right, bot);
				vh.AddVert(v, color32, posToUV(v));
				vh.AddTriangle(0, 1, 2);
				vh.AddTriangle(2, 3, 0);
				return;
			}

			var numChars = _coreComponent.Properties.CharacterRects.Length;
			// By using Ceiling, only 0 percent will show no words.
			var lastCharIndex = Math.Min(numChars, (int)Math.Ceiling(numChars * _revealLetters));
			if (lastCharIndex <= 0)
			{
				return;
			}

			for (int charIdx = 0; charIdx < lastCharIndex; charIdx++)
			{
				// Rects are on the texture
				var charRect = _coreComponent.Properties.CharacterRects[charIdx];

				var color32 = color;
				// Values of the character rect's edges is Unity, clamped to stay within the bound of the text
				// rectangle.
				var left = Mathf.Clamp(textRect.x + charRect.X, textRect.x, textRect.x + textRect.width);
				var right = Mathf.Clamp(textRect.x + charRect.X + charRect.Width, textRect.x,
										textRect.x + textRect.width);
				var bot = Mathf.Clamp(textRect.y + textRect.height - charRect.Y - charRect.Height, textRect.y,
									textRect.y + textRect.height);
				var top = Mathf.Clamp(textRect.y + textRect.height - charRect.Y, textRect.y,
									textRect.y + textRect.height);

				Func<Vector2, Vector2> posToUV = p =>
					new Vector2((p.x - textRect.x) / _coreComponent.Properties.Texture.width,
								(p.y - textRect.y) / _coreComponent.Properties.Texture.height);
				var v = new Vector3(left, bot);
				vh.AddVert(v, color32, posToUV(v));
				v = new Vector3(left, top);
				vh.AddVert(v, color32, posToUV(v));
				v = new Vector3(right, top);
				vh.AddVert(v, color32, posToUV(v));
				v = new Vector3(right, bot);
				vh.AddVert(v, color32, posToUV(v));

				// This is the number of vertices already added to the helper from previous iterations.
				int numVerts = charIdx * 4;
				vh.AddTriangle(numVerts + 0, numVerts + 1, numVerts + 2);
				vh.AddTriangle(numVerts + 2, numVerts + 3, numVerts + 0);
			}
		}

		protected override void OnDidApplyAnimationProperties()
		{
			SetMaterialDirty();
			SetVerticesDirty();
		}
	}
}