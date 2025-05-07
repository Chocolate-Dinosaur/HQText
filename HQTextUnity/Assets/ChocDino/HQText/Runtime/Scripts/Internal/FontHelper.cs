//--------------------------------------------------------------------------//
// Copyright 2024-2024 Chocolate Dinosaur Ltd. All rights reserved.         //
// For full documentation visit https://www.chocolatedinosaur.com           //
//--------------------------------------------------------------------------//

using System;
using System.IO;
using UnityEngine;

namespace ChocDino.HQText.Internal
{
	/// <summary>
	/// Contains a series of static functions related to working with fonts in HQText.
	/// </summary>
	public static class FontHelper 
	{
		/// <summary>
		/// Tries to initialize the Font Config for Freetype
		/// </summary>
	#if UNITY_EDITOR
		//[MenuItem("ChocDino/HQText - Refresh Fonts")]
		internal static void RefreshFonts() { TryInitFontConfig(true); }
	#endif

		// Process all files in the directory passed in, recurse on any directories
		// that are found, and process the files they contain.
		public static string FindFontConf(string targetDirectory)
		{
			// Process the list of files found in the directory.
			string[] fileEntries = Directory.GetFiles(targetDirectory);
			foreach (string fileName in fileEntries)
			{
				if (Path.GetFileName(fileName) == "font.conf")
				{
					return fileName;
				}
			}
			// Recurse into subdirectories of this directory.
			string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
			foreach (string subdirectory in subdirectoryEntries)
			{
				string s = FindFontConf(subdirectory);
				if (s != null)
				{
					return s;
				}
			}
			return null;
		}

		/// <summary>
		/// Tries to initialise the FreeType font library using the font files added in the
		/// StreamingAssets directory.
		/// </summary>
		/// <param name="force">if true, it will force a refresh, useful if you want to refresh after
		/// changing the xml settings file</param> <returns>true if successful false if not</returns>
		public static bool TryInitFontConfig(bool force = false)
		{
			if (NativePlugin.FontConfigInitialized() && !force)
			{
				return true;
			}

			string fontConf = null;

			// NOTE: Loading an external config file for fontConfig is supported, but for now we just
			// use a default built-in one.

			/*
			try
			{
				fontConf = FindFontConf(Application.streamingAssetsPath);
			}
			catch (DirectoryNotFoundException)
			{
				Debug.LogError("[HQText] Could not find Streaming Assets folder");
				// do nothing, in this case streaming assets doesn't exist, so fontConf will be null, we
				// will create a directory
			}

			if (fontConf == null)
			{
				Directory.CreateDirectory(Path.Combine(Application.streamingAssetsPath, "HQText"));
				File.WriteAllText(
					Path.Combine(Application.streamingAssetsPath, "HQText/font.conf"),
					"<?xml version=\"1.0\"?><!DOCTYPE fontconfig SYSTEM \"fonts.dtd\"><fontconfig></fontconfig>");

				fontConf = FindFontConf(Application.streamingAssetsPath);
			}

			if (fontConf == null)
			{
				Debug.LogError("[HQTExt] Could not find font.conf in streaming assets");
				return false;
			}*/

			if (!NativePlugin.InitializeFontConfig(fontConf))
			{
				Debug.LogError("[HQText] Unable to Initialize FontConfig");
				return false;
			}

			if (!NativePlugin.AddFontDir(Application.streamingAssetsPath))
			{
				Debug.LogError("[HQText] Unable to add streaming assets as a font watch folder for fontconfig");
				return false;
			}

			return true;
		}

		/// <summary>
		/// Returns a list of fonts a given font backend
		/// </summary>
		/// <param name="backend">The backend to choose from (freetype or win32)</param>
		/// <returns>A list of fonts</returns>
		private static string[] GetFontsInternal(FontBackend backend)
		{
			string[] results = null;
			int count = 0;
			IntPtr ptr = NativePlugin.GetAvailableFontFamilies(ref count, backend);

			results = new string[count];
			for (int i = 0; i < count; i++)
			{
				results[i] = NativePlugin.GetFontFamilyAtIndex(ptr, i);
				//Debug.Log(i + " " + results[i]);
			}
			NativePlugin.FreeFontFamilies(ptr);
			return results;
		}

		private static string[] GetFontsInternal(FontBackend backend, ref string[][] fontFaces)
		{
			string[] results = null;
			int count = 0;
			IntPtr ptr = NativePlugin.GetAvailableFontFamilies(ref count, backend);

			results = new string[count];

			fontFaces = new string[count][];
			for (int i = 0; i < count; i++)
			{
				results[i] = NativePlugin.GetFontFamilyAtIndex(ptr, i);
				//Debug.Log(i + " " + results[i]);
				int facesCount = 0;
				IntPtr p = NativePlugin.GetAvailableFontFacesAtIndex(ptr, i, ref facesCount);
				fontFaces[i] = new string[facesCount];
				for (int x = 0; x < facesCount; x++)
				{
					bool isSynthetic = false;
					fontFaces[i][x] = NativePlugin.GetFontFaceAtIndex(p, x) + (isSynthetic ? " (Synthetic)" : "");
				}
				NativePlugin.FreeFontFaces(p);
			}
			NativePlugin.FreeFontFamilies(ptr);
			return results;
		}

		/// <summary>
		/// Will get fonts for a given font backend. It will also initialize freetype if its not
		/// initialized.
		/// </summary>
		/// <param name="backend">The backend to choose from</param>
		/// <returns>a list of fonts</returns>
		public static string[] GetFonts(FontBackend backend)
		{
			if (backend == FontBackend.FreeType)
			{
				if (TryInitFontConfig())
				{
					return GetFontsInternal(backend);
				}
			} else if (backend == FontBackend.Win32)
			{
				return GetFontsInternal(backend);
			}
			throw new Exception("[HQText] Should not get here, unknown font backend type");
		}

		public static string[] GetFonts(FontBackend backend, ref string[][] fontFaces)
		{
			if (backend == FontBackend.FreeType)
			{
				if (TryInitFontConfig())
				{
					return GetFontsInternal(backend, ref fontFaces);
				}
			}
		#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			// Win32 backend only allowed on Windows
			else if (backend == FontBackend.Win32)
			{
				return GetFontsInternal(backend, ref fontFaces);
			}
		#endif
		/*#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			// Quartz backend only allowed on OSX
			else if (backend == FontBackend.Quartz) {
				return GetFontsInternal(backend, ref fontFaces);
			}
		#endif*/
			throw new Exception("[HQText] Invalid font backend for operating system.");
		}

		/// <returns>a description of a font based on the settings for a given style. This string is
		/// used by our plugin as input to render a given style</returns>
		public static string GenerateFontDescription(int fontsize, Weight weight, Style style, Stretch stretch, Variant variant, Gravity gravity)
		{
			return $"{weight.ToFriendlyName()} {style.ToFriendlyName()}  {stretch.ToFriendlyName()} {gravity.ToFriendlyName()} {fontsize} {variant.ToFriendlyName()}";
		}
	}
}