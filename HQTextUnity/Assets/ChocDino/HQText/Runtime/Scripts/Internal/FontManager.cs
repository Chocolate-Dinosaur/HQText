//--------------------------------------------------------------------------//
// Copyright 2024-2024 Chocolate Dinosaur Ltd. All rights reserved.         //
// For full documentation visit https://www.chocolatedinosaur.com           //
//--------------------------------------------------------------------------//

using System.Linq;

namespace ChocDino.HQText.Internal
{
	public class FontManager
	{
		private string[] fonts = {};
		public string[] Fonts { get => fonts; }
		private string[][] faces = {};

		private FontBackend backend = FontBackend.FreeType;

		public FontBackend Backend
		{
			get => backend;
			set
			{
				if (backend != value)
				{
					backend = value;
					LoadFonts();
				}
			}
		}

		private FontManager() {}

		public FontManager(FontBackend backend)
		{
			this.backend = backend;
			LoadFonts();
		}

		private void LoadFonts()
		{
			fonts = FontHelper.GetFonts(backend, ref faces);
		}

		public int FontIndex(string font)
		{
			return Fonts.ToList().IndexOf(font);
		}

		public string[] Faces(string font)
		{
			if (string.IsNullOrEmpty(font))
			{
				return new string[] {};
			}
			var i = FontIndex(font);
			if (i < 0)
			{
				return new string[] {};
			}
			return faces[i];
		}
	}
}