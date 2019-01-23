// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEditor;
using UnityEngine;
using System.IO;

namespace Utage
{
	//宴のトップメニュー　Tools>Utage以下の処理は全てここから呼び出す
	public class MenuTool : ScriptableObject
	{
		enum Index
		{
			hoge,
		};
		public const string MenuToolRoot = "Tools/Utage/";

		const int PriorityAdv = 0;
		/// <summary>
		/// 各種マネージャーを作成
		/// </summary>
		[MenuItem(MenuToolRoot + "New Project", priority = PriorityAdv+0)]
		static void CreateNewProject()
		{
			EditorWindow.GetWindow(typeof(CreateNewProjectWindow), false, "New Project");
		}

		/// <summary>
		/// シナリオデータビルダーを開く
		/// </summary>
		[MenuItem(MenuToolRoot + "Scenario Data Builder", priority = PriorityAdv + 1)]
		static public void AdvExcelEditorWindow()
		{
			EditorWindow.GetWindow(typeof(AdvScenarioDataBuilderWindow), false, "Scenario Data");
		}
	
		/// <summary>
		/// リソースコンバーターを開く
		/// </summary>
		[MenuItem(MenuToolRoot + "Resource Converter", priority = PriorityAdv + 2)]
		static public void AdvResourceConverterWindow()
		{
			EditorWindow.GetWindow(typeof(AdvResourcesConverter), false, "Resource Converter");
		}

		/// <summary>
		/// リソースコンバーターを開く
		/// </summary>
		[MenuItem(MenuToolRoot + "Dicing Converter", priority = PriorityAdv + 3)]
		static public void AdvDicingConverter()
		{
			EditorWindow.GetWindow(typeof(DicingConverter), false, "Dicing Converter");
		}


		/// <summary>
		/// シナリオビュワーを開く
		/// </summary>
		[MenuItem(MenuToolRoot + "Viewer/Scenario Viewer", priority = PriorityAdv + 10)]
		static void OpenAdvScenarioViewer()
		{
			EditorWindow.GetWindow(typeof(AdvScenarioViewer), false, "Scenario");
		}

		/// <summary>
		/// パラメータービュワーを開く
		/// </summary>
		[MenuItem(MenuToolRoot + "Viewer/Parameter Viewer", priority = PriorityAdv + 11)]
		static void OpenAdvParamViewer()
		{
			EditorWindow.GetWindow(typeof(AdvParamViewer), false, "Parameter");
		}

		/// <summary>
		/// ファイルマネージャービュワーを開く
		/// </summary>
		[MenuItem(MenuToolRoot + "Viewer/File Manager Viewer", priority = PriorityAdv + 12)]
		static void OpenFileViewer()
		{
			EditorWindow.GetWindow(typeof(AdvFileManagerViewer), false, "File Manager");
		}


		/// <summary>
		/// Adv関係のエディター設定
		/// </summary>
		[MenuItem(MenuToolRoot + "Editor Setting", priority = PriorityAdv + 21)]
		static void OpenEditorSettingWindow()
		{
			EditorWindow.GetWindow(typeof(AdvEditorSettingWindow), false, "Editor Setting");
		}


		//************************出力ファイル************************//

		const int PrioriyOutPut = 100;
		/// <summary>
		/// セーブデータフォルダを開く
		/// </summary>
		[MenuItem(MenuToolRoot + "Open Output Folder/SaveData", priority = PrioriyOutPut + 0)]
		static void OpenSaveDataFolder()
		{
			OpenFilePanelCreateIfMissing("Open utage save data folder", FileIOManager.SdkPersistentDataPath);
		}

		/// <summary>
		/// キャッシュデータフォルダを開く
		/// </summary>
		[MenuItem(MenuToolRoot + "Open Output Folder/Cache", priority = PrioriyOutPut + 1)]
		static void OpenCacheFolder()
		{
			OpenFilePanelCreateIfMissing("Open utage cache folder", FileIOManager.SdkTemporaryCachePath);
		}

		/// <summary>
		/// セーブデータを全て削除
		/// </summary>
		[MenuItem(MenuToolRoot + "Delete Output Files/SaveData", priority = PrioriyOutPut+2)]
		static void DeleteSaveDataFiles()
		{
			if (EditorUtility.DisplayDialog(
				LanguageSystemText.LocalizeText(SystemText.DeleteAllSaveDataFilesTitle),
				LanguageSystemText.LocalizeText(SystemText.DeleteAllSaveDataFilesMessage),
				LanguageSystemText.LocalizeText(SystemText.Ok),
				LanguageSystemText.LocalizeText(SystemText.Cancel)
				))
			{
				DeleteFolder(FileIOManager.SdkPersistentDataPath);
			}
		}

		/// <summary>
		/// AssetBundleのキャッシュファイルを削除
		/// </summary>
		[MenuItem(MenuToolRoot + "Delete Output Files/Cache and AssetBundles", priority = PrioriyOutPut + 3)]
		static void DeleteCacheFilesAndAssetBundles()
		{
			if (EditorUtility.DisplayDialog(
				LanguageSystemText.LocalizeText(SystemText.DeleteAllCacheFilesTitle),
				LanguageSystemText.LocalizeText(SystemText.DeleteAllCacheFilesMessage),
				LanguageSystemText.LocalizeText(SystemText.Ok),
				LanguageSystemText.LocalizeText(SystemText.Cancel)
				))
			{
				DeleteFolder(FileIOManager.SdkTemporaryCachePath);
				WrapperUnityVersion.CleanCache();
			}
		}

		/// <summary>
		/// 全ファイルを全て削除
		/// </summary>
		[MenuItem(MenuToolRoot + "Delete Output Files/All Files", priority = PrioriyOutPut+4)]
		static void DeleteAllFiles()
		{
			if (EditorUtility.DisplayDialog(
				LanguageSystemText.LocalizeText(SystemText.DeleteAllOutputFilesTitle),
				LanguageSystemText.LocalizeText(SystemText.DeleteAllOutputFilesMessage),
				LanguageSystemText.LocalizeText(SystemText.Ok),
				LanguageSystemText.LocalizeText(SystemText.Cancel)
				))
			{
				DeleteSaveDataFiles();
			}
		}



		static void DeleteFolder(string path)
		{
			if (Directory.Exists(path))
			{
				Directory.Delete(path, true);
				Debug.Log("Delete " + path);
			}
			else
			{
				Debug.Log("Not found " + path);
			}
		}

		static void OpenFilePanelCreateIfMissing(string title, string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			EditorUtility.OpenFilePanel(title, path, "");
		}

		//************************ツール等************************//

		const int PriorityTools = 500;
		//プロジェクトのパッケージを全て出力する
		[MenuItem(MenuToolRoot + "Tools/Export Project Package", priority = PriorityTools+1)]
		static void OpenExportProjectPackage()
		{
			string path = EditorUtility.SaveFilePanel("Export Project Package...", "../", "", "unitypackage");
			if (!string.IsNullOrEmpty(path))
			{
				AssetDatabase.ExportPackage("Assets", path,
					ExportPackageOptions.Recurse | ExportPackageOptions.Interactive | ExportPackageOptions.IncludeLibraryAssets);
			}
		}

		/// <summary>
		/// フォントを変更
		/// </summary>
		[MenuItem(MenuToolRoot + "Tools/FontChanger", priority = PriorityTools + 2)]
		static void OpenFontChanger()
		{
			EditorWindow.GetWindow(typeof(FontChanger), false, "Font Changer");
		}


		/// <summary>
		/// スクリプトクリーナー
		/// </summary>
		[MenuItem(MenuToolRoot + "DeveloperTool/Script Cleaner", priority = PriorityTools + 10)]
		static void OpenScriptCleanerWindow()
		{
			EditorWindow.GetWindow(typeof(ScriptCleanerWindow), false, "Script Cleaner");
		}

		/// <summary>
		/// 色を変更
		/// </summary>
		[MenuItem(MenuToolRoot + "DeveloperTool/Selectable Color Changer", priority = PriorityTools + 11)]
		static void OpenSelectableColorChanger()
		{
			EditorWindow.GetWindow(typeof(SelectableColorChanger), false, "Selectable Color Changer");
		}

		/// <summary>
		/// アセット参照を変更
		/// </summary>
		[MenuItem(MenuToolRoot + "DeveloperTool/ReferenceAssetChanger", priority = PriorityTools + 12)]
		static void OpenReferenceAssetChanger()
		{
			UtageEditorToolKit.GetAllEditorWindow();
			EditorWindow.GetWindow(typeof(ReferenceAssetChanger), false, "Reference Asset Changer");
		}

		/// <summary>
		/// エクセルファイルのローカライズ用ファイルを作る
		/// </summary>
		[MenuItem(MenuToolRoot + "DeveloperTool/Excel Localize Converter", priority = PriorityTools + 13)]
		static void OpenExcelLocalizeConverter()
		{
			EditorWindow.GetWindow(typeof(AdvExcelLocalizeConverter), false, "Excel Localize Converter");
		}

		/// <summary>
		/// エクセルファイルのローカライズ用ファイルをシナリオファイルにマージする
		/// </summary>
		[MenuItem(MenuToolRoot + "DeveloperTool/Excel Localize Merger", priority = PriorityTools + 14)]
		static void OpenExcelLocalizeMerger()
		{
			EditorWindow.GetWindow(typeof(AdvExcelLocalizeMerger), false, "Excel Localize Merger");
		}



		//************************About************************//

		const int PriorityAbout = 900;

		/// <summary>
		/// 宴の情報を開く
		/// </summary>
		[MenuItem(MenuToolRoot + "About Utage...", priority = PriorityAbout+0)]
		static void OpenAboutUtage()
		{
			EditorWindow.GetWindow(typeof(AboutUtage), false, "About Utage");
		}
	}
}