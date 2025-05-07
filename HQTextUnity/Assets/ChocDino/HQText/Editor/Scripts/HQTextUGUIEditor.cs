//--------------------------------------------------------------------------//
// Copyright 2024-2024 Chocolate Dinosaur Ltd. All rights reserved.         //
// For full documentation visit https://www.chocolatedinosaur.com           //
//--------------------------------------------------------------------------//

using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

namespace ChocDino.HQText.Editor
{
	/// <summary>
	/// Editor for HQ Text UGUI compeontn
	/// </summary>
	[CustomEditor(typeof(HQTextUGUI), true)]
	[CanEditMultipleObjects]
	public class HQTextUGUIEditor : GraphicEditor
	{
		//private SerializedProperty _propCoreComponent;
		private SerializedProperty _propRevealLetters;
		private SerializedProperty _propReferenceTexture;

		protected override void OnEnable()
		{
			base.OnEnable();
			_propRevealLetters = serializedObject.FindProperty("_revealLetters");
			//_propCoreComponent = serializedObject.FindProperty("_coreComponent");
			_propReferenceTexture = serializedObject.FindProperty("_referenceTexture");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			//EditorGUILayout.PropertyField(_propCoreComponent);
			EditorGUILayout.PropertyField(_propRevealLetters);

			EditorGUILayout.Space();

			AppearanceControlsGUI();
			RaycastControlsGUI();
			MaskableControlsGUI();

			//EditorGUILayout.PropertyField(_propReferenceTexture);

			serializedObject.ApplyModifiedProperties();
		}
	}
}