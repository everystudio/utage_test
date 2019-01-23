// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Utage
{

	//「Utage」のシナリオデータ用のエクセルファイルの管理エディタウイドウ
	public class AdvScenarioDataBuilderWindow : EditorWindow
	{
		static public AdvScenarioDataProject ProjectData
		{
			get	{
				if (scenarioDataProject == null)
				{
					scenarioDataProject = UtageEditorPrefs.LoadAsset<AdvScenarioDataProject>(UtageEditorPrefs.Key.AdvScenarioProject);
				}
				return scenarioDataProject;
			}
			set
			{
				if (scenarioDataProject != value)
				{
					scenarioDataProject = value;
					UtageEditorPrefs.SaveAsset(UtageEditorPrefs.Key.AdvScenarioProject, scenarioDataProject);
				}
			}
		}
		//プロジェクトデータ
		static AdvScenarioDataProject scenarioDataProject;

		static public Object ProjectDirAsset
		{
			get
			{
				if (ProjectData == null) return null;

				string dir = AssetDatabase.GetAssetPath(AdvScenarioDataBuilderWindow.ProjectData);
				MainAssetInfo info = new MainAssetInfo(FilePathUtil.GetDirectoryPath(dir));
				return info.Asset;
			}
		}

		/// <summary>
		/// エクセルをコンバート
		/// </summary>
		public static void Convert()
		{
			if (ProjectData == null)
			{
				Debug.LogWarning("Scenario Data Excel project is no found");
				return;
			}
			if (string.IsNullOrEmpty(ProjectData.ConvertPath))
			{
				Debug.LogWarning("Convert Path is not defined");
				return;
			}
			AdvExcelCsvConverter converter = new AdvExcelCsvConverter();
			foreach( var item in ProjectData.ChapterDataList )
			{
				if (!converter.Convert(ProjectData.ConvertPath, item.ExcelPathList, item.chapterName, ProjectData.ConvertVersion))
				{
					Debug.LogWarning("Convert is failed");
					return;
				}
			}
			if (ProjectData.IsAutoUpdateVersionAfterConvert)
			{
				ProjectData.ConvertVersion = ProjectData.ConvertVersion + 1;
				EditorUtility.SetDirty(ProjectData);
			}
		}

		/// <summary>
		/// 管理対象のリソースを再インポート
		/// </summary>
		static public void ReImportResources()
		{
			if (ProjectData)
			{
				ProjectData.ReImportResources();
			}
		}

		/// <summary>
		/// インポートされたアセットが管理対象ならインポート
		/// </summary>
		public static void Import( string[] importedAssets = null)
		{
			if (ProjectData == null)
			{
				//シナリオが設定されてないのでインポートしない
				return;
			}
			AdvExcelImporter importer = new AdvExcelImporter();
			if (importedAssets != null)
			{
				if (!ProjectData.CheckAutoImportType())
				{
					//インポートが無効
					return;
				}
				if (!ProjectData.ContainsExcelPath(importedAssets))
				{
					//現在のプロジェクトのアセットがないのでインポートしない
					return;
				}
				if (ProjectData.QuickImport)
				{
					importer.Import(ProjectData, importedAssets);
				}
				else
				{
					importer.ImportAll(ProjectData);
				}
			}
			else
			{
				importer.ImportAll(ProjectData);
			}

			if (ProjectData.IsAutoConvertOnImport)
			{
				Convert();
			}
		}


		//		string newProjectName = "";

		protected Vector2 scrollPosition;
		void OnGUI()
		{
			//スクロール
			this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);

			GUILayout.Space(4f);
			EditorGUILayout.BeginHorizontal();
//			GUIStyle style = new GUIStyle();
//			style.richText = true;
			UtageEditorToolKit.BoldLabel("Project", GUILayout.Width(80f));
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(4f);

			AdvScenarioDataProject project = EditorGUILayout.ObjectField("", ProjectData, typeof(AdvScenarioDataProject), false) as AdvScenarioDataProject;
			if (project != ProjectData)
			{
				ProjectData = project;
			}

			if (ProjectData != null)
			{
				DrawProject();
			}

			EditorGUILayout.EndScrollView();
		}

		void DrawProject()
		{
			SerializedObject serializedObject = new SerializedObject(ProjectData);
			serializedObject.Update();

			/*********************************************************************/
			UtageEditorToolKit.BeginGroup("Import Scenario Files");
			UtageEditorToolKit.PropertyField(serializedObject, "autoImportType", "Auto Import Type");


			UtageEditorToolKit.PropertyFieldArray(serializedObject, "chapterDataList", "Chapters");

			GUILayout.Space(8f);

			EditorGUI.BeginDisabledGroup(!ProjectData.IsEnableImport);

			UtageEditorToolKit.PropertyField(serializedObject, "checkWaitType", "Check Wait Type");
			UtageEditorToolKit.PropertyField(serializedObject, "checkWhiteSpaceEndOfCell", "Check White Space EndOfCell");			
			UtageEditorToolKit.PropertyField(serializedObject, "checkTextCount", "Check Text Count");
			UtageEditorToolKit.PropertyField(serializedObject, "quickImport", "Quick Auto Import [Warining!]");
			UtageEditorToolKit.PropertyField(serializedObject, "parseFormula", "Parse Formula");
			UtageEditorToolKit.PropertyField(serializedObject, "parseNumreic", "Parse Numreic");
			if (GUILayout.Button("Import", GUILayout.Width(180)))
			{
				Import();
			}
			EditorGUI.EndDisabledGroup();
			UtageEditorToolKit.EndGroup();

			GUILayout.Space(8f);

			/*********************************************************************/
			UtageEditorToolKit.BeginGroup("Custom Import Folders");
			UtageEditorToolKit.PropertyFieldArray(serializedObject, "customInportSpriteFolders", "Sprite Folder List");
			UtageEditorToolKit.PropertyFieldArray(serializedObject, "customInportAudioFolders", "Audio Folder List");
			UtageEditorToolKit.PropertyFieldArray(serializedObject, "customInportMovieFolders", "Movie Folder List");

			bool isEnableResouces = ProjectData.CustomInportAudioFolders.Count <= 0 && 
				ProjectData.CustomInportSpriteFolders.Count <= 0 &&
				ProjectData.CustomInportMovieFolders.Count <= 0;

			EditorGUI.BeginDisabledGroup(isEnableResouces);
			if (GUILayout.Button("ReimportResources", GUILayout.Width(180)))
			{
				ReImportResources();
			}
			EditorGUI.EndDisabledGroup();

			UtageEditorToolKit.EndGroup();

			GUILayout.Space(8f);

			/*********************************************************************/
			UtageEditorToolKit.BeginGroup("Covert Setting");

			EditorGUILayout.BeginHorizontal();
			UtageEditorToolKit.PropertyField(serializedObject, "convertPath", "Output directory");
			if (GUILayout.Button("Select", GUILayout.Width(100)))
			{
				string convertPath = ProjectData.ConvertPath;
				string dir = string.IsNullOrEmpty(convertPath) ? "" : Path.GetDirectoryName(convertPath);
				string name = string.IsNullOrEmpty(convertPath) ? "" : Path.GetFileName(convertPath);
				string path = EditorUtility.SaveFolderPanel("Select folder to output", dir, name);
				Debug.Log(path);
				if (!string.IsNullOrEmpty(path))
				{
					ProjectData.ConvertPath = path;
				}
			}
			EditorGUILayout.EndHorizontal();

			UtageEditorToolKit.PropertyField(serializedObject, "convertVersion", "Version");

			UtageEditorToolKit.PropertyField(serializedObject, "isAutoUpdateVersionAfterConvert", "Auto Update Version");


			UtageEditorToolKit.EndGroup();
			GUILayout.Space(4f);

			/*********************************************************************/
			EditorGUI.BeginDisabledGroup(!ProjectData.IsEnableConvert);
			UtageEditorToolKit.PropertyField(serializedObject, "isAutoConvertOnImport", "Auto Convert OnImport");
			if (GUILayout.Button("Convert", GUILayout.Width(180)))
			{
				Convert();
			}
			EditorGUI.EndDisabledGroup();
			GUILayout.Space(8f);

			serializedObject.ApplyModifiedProperties();
		}
	}
}