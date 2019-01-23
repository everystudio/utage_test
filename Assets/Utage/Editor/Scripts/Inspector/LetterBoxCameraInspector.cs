// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Utage
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(LetterBoxCamera))]
	public class LetterBoxCameraInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			DrawProperties();
			serializedObject.ApplyModifiedProperties();
		}

		void DrawProperties()
		{
			LetterBoxCamera obj = target as LetterBoxCamera;

			UtageEditorToolKit.PropertyField(serializedObject, "pixelsToUnits", "Pixels To Units");
			if (obj.PixelsToUnits < 1) obj.PixelsToUnits = 1;

			//基本画面サイズ
			UtageEditorToolKit.PropertyField(serializedObject, "width", "Game Screen Width" );
			if (obj.Width < 1) obj.Width = 1;
			UtageEditorToolKit.PropertyField(serializedObject, "height", "Game Screen Height" );
			if (obj.Height < 1) obj.Height = 1;

			//基本画面サイズ
			UtageEditorToolKit.BeginGroup ("Flexible",serializedObject, "isFlexible");
			if(obj.IsFlexible)
			{
				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Wide  ", GUILayout.Width(50));
				UtageEditorToolKit.PropertyField(serializedObject, "maxWidth", "", GUILayout.Width(50));
				if (obj.MaxWidth < obj.Width) obj.MaxWidth = obj.Width;
				EditorGUILayout.LabelField(" x " + obj.Height, GUILayout.Width(50));
				GUILayout.EndHorizontal();

				GUILayout.Space(4f);
				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Nallow  ", GUILayout.Width(50));
				EditorGUILayout.LabelField("" + obj.Width + " x ", GUILayout.Width(50));

				UtageEditorToolKit.PropertyField(serializedObject, "maxHeight", "", GUILayout.Width(50));
				if (obj.MaxHeight < obj.Height) obj.MaxHeight = obj.Height;
				GUILayout.EndHorizontal();
			}
			UtageEditorToolKit.EndGroup();

			EditorGUILayout.LabelField("Current Size = " +  obj.CurrentSize.x +" x " + obj.CurrentSize.y);

			UtageEditorToolKit.PropertyField(serializedObject, "anchor", "Anchor");

			UtageEditorToolKit.PropertyField(serializedObject, "zoom2D", "Zoom 2D");
			UtageEditorToolKit.PropertyField(serializedObject, "zoom2DCenter", "Zoom 2D Center");

			if (obj.Height < 1) obj.Height = 1;
		}
	}
}

 