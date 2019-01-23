// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Utage
{
	//フォント別のアセットに変える
	public class FontChanger : CustomEditorWindow
	{
		Object projectDir;
		Font from;
		Font to;

		void OnEnable()
		{
			projectDir = AdvScenarioDataBuilderWindow.ProjectDirAsset;
			from = UtageEditorToolKit.LoadArialFont();
		}

		protected override void OnGUISub()
		{
			base.OnGUISub();
			projectDir = EditorGUILayout.ObjectField( "Project Dir", projectDir, typeof(Object), false) as Object;
			from = EditorGUILayout.ObjectField( "From", from, typeof(Font), false) as Font;
			to = EditorGUILayout.ObjectField("To", to, typeof(Font), false) as Font;

			bool isDisable = (from == null || to == null);

			EditorGUI.BeginDisabledGroup(isDisable);
			if (GUILayout.Button("Go!", GUILayout.Width(80f)))
			{
				Debug.Log( string.Format("Font Change {0} to {1} ", this.from.name, this.to.name ));
				
				string dir = (projectDir == null ) ? "" : AssetDatabase.GetAssetPath(projectDir);
				ReferenceAssetChanger.FindAndChangeAll(this.from, this.to, dir);
			}
			EditorGUI.EndDisabledGroup();
		}
	}
}