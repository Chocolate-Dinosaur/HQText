//--------------------------------------------------------------------------//
// Copyright 2024-2024 Chocolate Dinosaur Ltd. All rights reserved.         //
// For full documentation visit https://www.chocolatedinosaur.com           //
//--------------------------------------------------------------------------//

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ChocDino.HQText.Internal;

namespace ChocDino.HQText.Editor
{
	internal static class Preferences
	{
		internal static readonly string SettingsPath = "Project/Chocolate Dinosaur/HQText";

		[SettingsProvider]
		static SettingsProvider CreateSettingsProvider()
		{
			return new UIFXSettingsProvider(SettingsPath, SettingsScope.Project);
		}

		private class UIFXSettingsProvider : SettingsProvider
		{
			public UIFXSettingsProvider(string path, SettingsScope scope) : base(path, scope)
			{
				this.keywords = new HashSet<string>(new[] { "HQText", "Chocolate", "Dinosaur", "ChocDino", "UI", "GUI", "UGUI", "Text" });
			}

			public override void OnGUI(string searchContext)
			{
				EditorGUILayout.Space();

				if (GUILayout.Button("Refresh Fonts"))
				{
					FontHelper.RefreshFonts();
				}

				EditorGUILayout.Space();

				Links();
			}

			private void Links()
			{
				const string DiscordCommunityUrl = "https://discord.gg/wKRzKAHVUE";
				const string DocumentationUrl = "http://www.chocdino.com/products/hqtext/about/";
				const string AssetStoreUrl = "https://assetstore.unity.com/publishers/80225?aid=1100lSvNe";

				GUILayout.Label("Chocolate Dinosaur Links:", EditorStyles.largeLabel);

				if (GUILayout.Button("HQText Documentation", EditorStyles.miniButton))
				{
					Application.OpenURL(DocumentationUrl);
				}
				if (GUILayout.Button("Discord Community", EditorStyles.miniButton))
				{
					Application.OpenURL(DiscordCommunityUrl);
				}
				if (GUILayout.Button("Our Assets", EditorStyles.miniButton))
				{
					Application.OpenURL(AssetStoreUrl);
				}
			}
		}
	}
}