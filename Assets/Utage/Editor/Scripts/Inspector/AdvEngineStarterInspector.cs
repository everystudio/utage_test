// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
/*
namespace Utage
{
	[CustomEditor(typeof(AdvEngineStarter))]
	public class AdvEngineStarterInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			DrawProperties();
			serializedObject.ApplyModifiedProperties();
		}

		void DrawProperties()
		{
			AdvEngineStarter obj = target as AdvEngineStarter;

			UtageEditorToolKit.PropertyField(serializedObject, "engine", "Engine");
			UtageEditorToolKit.PropertyField(serializedObject, "isLoadOnAwake", "Is Load On Awake");
			UtageEditorToolKit.PropertyField(serializedObject, "isAutomaticPlay", "Is Automatic Play");
			UtageEditorToolKit.PropertyField(serializedObject, "startScenario", "Start Scenario Label");

			//シナリオデータ
			UtageEditorToolKit.BeginGroup("Scenario Data");
			UtageEditorToolKit.PropertyField(serializedObject, "scenarioDataLoadType", "LoadType");
			switch( obj.ScenarioDataLoadType )
			{
				case AdvEngineStarter.LoadType.Local:
					UtageEditorToolKit.PropertyField(serializedObject, "scenarios", "Scenarios");
					break;
				case AdvEngineStarter.LoadType.Server:
					UtageEditorToolKit.PropertyField(serializedObject, "urlScenarioData", "URL Scenario Data");
					UtageEditorToolKit.PropertyField(serializedObject, "scenarioVersion", "Boot File Version");
//					UtageEditorToolKit.PropertyFieldArray(serializedObject, "chapterUrlList", "Chapter URL List");
					break;
			}
			UtageEditorToolKit.EndGroup();


			//リソースデータ
			UtageEditorToolKit.BeginGroup("Resource Data");
			UtageEditorToolKit.PropertyField(serializedObject, "resourceLoadType", "LoadType");
			switch (obj.ResourceLoadType)
			{
				case AdvEngineStarter.LoadType.Local:
					UtageEditorToolKit.PropertyField(serializedObject, "rootResourceDir", "Root Dir");
					UtageEditorToolKit.PropertyField(serializedObject, "useConvertFileListOnLocal", "Convert File List");
					UtageEditorToolKit.PropertyField(serializedObject, "useAssetBundleListOnLocal", "Asset Bundle List");
					break;
				case AdvEngineStarter.LoadType.Server:
					UtageEditorToolKit.PropertyField(serializedObject, "urlResourceDir", "URL Resource Dir");
					UtageEditorToolKit.PropertyField(serializedObject, "useConvertFileListOnServer", "Convert File List");
					UtageEditorToolKit.PropertyField(serializedObject, "useAssetBundleListOnServer", "Asset Bundle List");
					break;
			}
			UtageEditorToolKit.EndGroup();

			//リソースデータ
			UtageEditorToolKit.BeginGroup("Load Setting");
			UtageEditorToolKit.PropertyField(serializedObject, "isAutomaticInitFileLoadSetting", "Is Automatic Init");

			UtageEditorToolKit.PropertyField(serializedObject, "localLoadSetting", "Local Load Setting");
			UtageEditorToolKit.PropertyField(serializedObject, "serverLoadSetting", "Server Load Setting");

			UtageEditorToolKit.EndGroup();


			//エディターのみ
			UtageEditorToolKit.BeginGroup("Editor");
			UtageEditorToolKit.PropertyField(serializedObject, "scenarioDataProject", "Scenario Data Project");
			if (!(obj.ScenarioDataProject is AdvScenarioDataProject))
			{
				obj.ScenarioDataProject = null;
			}
			UtageEditorToolKit.EndGroup();	
		}
	}
}

 */