//--------------------------------------------------------------------------//
// Copyright 2024-2024 Chocolate Dinosaur Ltd. All rights reserved.         //
// For full documentation visit https://www.chocolatedinosaur.com           //
//--------------------------------------------------------------------------//

using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace ChocDino.HQText.Editor
{
	public static class Helpers
	{
		[MenuItem("GameObject/UI/HQText")]
		public static void AddHQTextUIComponent(MenuCommand menuCommand) {
			GameObject selectedObject = menuCommand.context as GameObject;
			if (!selectedObject)
			{
				selectedObject = Selection.activeGameObject;
			}

			if (selectedObject != null)
			{
				var rectTransform = selectedObject.GetComponent<RectTransform>();
				if (rectTransform == null) 
				{
					selectedObject = FindOrCreateCanvas(selectedObject);
				}
			}
			else 
			{
				selectedObject = FindOrCreateCanvas(selectedObject);
			}

			GameObject hqTextUI = new GameObject("HQText");
			hqTextUI.transform.SetParent(selectedObject.transform, false);
			hqTextUI.AddComponent<CanvasRenderer>();
			hqTextUI.AddComponent<HQTextCore>();
			hqTextUI.AddComponent<HQTextUGUI>();
			Selection.activeGameObject = hqTextUI;
		}

		private static GameObject FindOrCreateCanvas(GameObject selectedObject) 
		{
			// If a GameObject is selected, use it's parent or itself has a Canvas.
			if (selectedObject != null)
			{
				if (selectedObject.GetComponent<Canvas>() != null) { return selectedObject; }
				if (selectedObject.GetComponentInParent<Canvas>() != null) { return selectedObject; }
			}

			// Find a canvas
			#if UNITY_2022_2_OR_NEWER
			Canvas canvas = Object.FindFirstObjectByType(typeof(Canvas)) as Canvas;
			#else
			Canvas canvas = Object.FindObjectOfType(typeof(Canvas)) as Canvas;
			#endif
			if (!canvas)
			{
				// Create a canvas
				GameObject canvasGo = new GameObject("Canvas");
				if (selectedObject != null)
				{
					canvasGo.transform.SetParent(selectedObject.transform);
				}
				canvas = canvasGo.AddComponent<Canvas>();
				canvas.renderMode = RenderMode.ScreenSpaceOverlay;
				canvasGo.AddComponent<CanvasScaler>();
				canvasGo.AddComponent<GraphicRaycaster>();
			}

			return canvas.gameObject;
		}
	}
}