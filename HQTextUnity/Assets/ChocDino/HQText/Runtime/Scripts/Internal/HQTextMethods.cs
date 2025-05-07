//--------------------------------------------------------------------------//
// Copyright 2024-2024 Chocolate Dinosaur Ltd. All rights reserved.         //
// For full documentation visit https://www.chocolatedinosaur.com           //
//--------------------------------------------------------------------------//

using System;
using UnityEngine;

namespace ChocDino.HQText.Internal
{
	/// <summary>
	/// Helper methods common to the components, containing rendering business logic
	/// </summary>
	internal static class HQTextMethods 
	{
		private const int MaxTextureSize = 8192;
		
		public static Vector2Int CalculateTextureSize(HorizontalWrapping horizontalWrapping, VerticalWrapping verticalWrapping, Vector2 textSize, Vector2 uiSize)
		{
			Vector2Int textureSize = new Vector2Int(Mathf.CeilToInt(uiSize.x), Mathf.CeilToInt(uiSize.y));

			if (horizontalWrapping == HorizontalWrapping.Clip || horizontalWrapping == HorizontalWrapping.Wrap)
			{
				if (uiSize.x > textSize.x)
				{
					textureSize.x = Mathf.CeilToInt(textSize.x);
				}
			}

			if (horizontalWrapping == HorizontalWrapping.Expand)
			{
				textureSize.x = Mathf.CeilToInt(textSize.x);
			}
			if (verticalWrapping == VerticalWrapping.Clip)
			{
				if (uiSize.y > textSize.y)
				{
					textureSize.y = Mathf.CeilToInt(textSize.y);
				}
			}

			if (verticalWrapping == VerticalWrapping.Expand)
			{
				textureSize.y = Mathf.CeilToInt(textSize.y);
			}

			return new Vector2Int(Mathf.Max(8, textureSize.x), Mathf.Max(8, textureSize.y));
		}

		private static Texture2D CreateTexture(int width, int height)
		{
			Debug.Assert(width > 0 && height > 0);
			Debug.Assert(width <= MaxTextureSize && height <= MaxTextureSize);
			// TODO: Support an option for generating mip-maps
			width = Mathf.Clamp(width, 0, MaxTextureSize);
			height = Mathf.Clamp(height, 0, MaxTextureSize);
			var result = new Texture2D(width, height, TextureFormat.ARGB32, mipChain:false, linear:false
			#if UNITY_2022_1_OR_NEWER
			, createUninitialized:true
			#endif
			)
			{
				wrapMode = TextureWrapMode.Clamp,
				filterMode = FilterMode.Point,
			};
			return result;
		}

		/// <summary>
		/// Creates a texture in the properties object based on a new width and height. If the width and
		/// height input match the current texture size it does nothing
		/// </summary>
		/// <param name="properties">The input text properties</param>
		/// <param name="width">The width to test against</param>
		/// <param name="height">The height to test again</param>
		/// <returns>Return value indicates if the texture was regenerated</returns>
		public static bool RegenerateTexture(HQTextProperties properties, int width, int height)
		{
			// < 8 causes crash in native
			width = Mathf.Max(8, width);
			height = Mathf.Max(8, height);

			if (properties.Texture == null)
			{
				properties.Texture = CreateTexture(width, height);
				return true;
			}

			// TODO: Use texture pool
			if (width != properties.Texture.width || height != properties.Texture.height)
			{
				if (!Application.isPlaying)
				{
					GameObject.DestroyImmediate(properties.Texture);
				}
				else
				{
					GameObject.Destroy(properties.Texture);
				}
				properties.Texture = CreateTexture(width, height);
				return true;
			}
			return false;
		}
	}
}