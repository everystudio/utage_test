// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Utage
{

	//「Utage」のカスタムエディタウィンドウのテスト
	public class TestCustomEditorWindow : CustomEditorWindow
	{
		[System.Serializable]
		public class MyClassSub
		{
			public bool isOpenSub = true;
		};

		[System.Serializable]
		public class MyClass
		{
			public MyClassSub sub;
		};

		public MyClass myclass;
		public string text = "test";
		public int count = 10;
		public bool[] flgas = new bool[4];

		[System.Flags]
		enum ResourcesLoadType
		{
			Text = 0x1 << 0,				//テキスト
			Binary = 0x1 << 1,				//バイナリ
			Texture = 0x1 << 2,				//テクスチャ
			Sound = 0x1 << 3,				//サウンド
			Csv = 0x1 << 4,					//CSV
			UnityObject = 0x1 << 5,			//UnityObject（プレハブ等）
		};
		//キャッシュするファイルの暗号化の仕方
		[SerializeField]
		[EnumFlagsAttribute]
		ResourcesLoadType resourcesLoadType = 0;
		ResourcesLoadType ResourcesLoad
		{
			get { return resourcesLoadType; }
			set { resourcesLoadType = value; }
		}

		/// <summary>
		/// リソースのパス
		/// </summary>
		[SerializeField]
		Object resourcesRoot;
		public Object ResourcesRoot
		{
			get { return resourcesRoot; }
			set { resourcesRoot = value; }
		}

		/// <summary>
		/// リソースの出力先のパス
		/// </summary>
		[SerializeField]
		string outputResourcesPath;
		public string OutputResourcesPath
		{
			get { return outputResourcesPath; }
			set { outputResourcesPath = value; }
		}

		/// <summary>
		/// リソースを暗号化して出力するか
		/// </summary>
		[SerializeField]
		bool isResoucesCopyNewerOnly;
		public bool IsResoucesCopyNewerOnly
		{
			get { return isResoucesCopyNewerOnly; }
			set { isResoucesCopyNewerOnly = value; }
		}
		
	
		/*********************************************************************/
		/*
					UtageEditorToolKit.BeginGroup("Output Resources Data Setting");

					UtageEditorToolKit.PropertyField(serializedObject, "resourcesRoot", "Resources Root");
					EditorGUILayout.BeginHorizontal();

					UtageEditorToolKit.PropertyField(serializedObject, "outputResourcesPath", "Output Resources Directory");
					if (GUILayout.Button("Select", GUILayout.Width(100)))
					{
						string path = ProjectData.OutputResourcesPath;
						string dir = string.IsNullOrEmpty(path) ? "" : Path.GetDirectoryName(path);
						string name = string.IsNullOrEmpty(path) ? "" : Path.GetFileName(path);
						path = EditorUtility.SaveFolderPanel("Select folder to output", dir, name);
						Debug.Log(path);
						if (!string.IsNullOrEmpty(path))
						{
							ProjectData.OutputResourcesPath = path;
						}
					}
					EditorGUILayout.EndHorizontal();

					fileManager = EditorGUILayout.ObjectField(fileManager, typeof(AssetFileManager), true) as AssetFileManager;

					bool isEnableOutputResources =
						!string.IsNullOrEmpty(ProjectData.OutputResourcesPath)
						&& (fileManager != null);

					EditorGUI.BeginDisabledGroup(!isEnableOutputResources);
					UtageEditorToolKit.PropertyField(serializedObject, "isResoucesCopyNewerOnly", "Copy new and newer files only");
					if (GUILayout.Button("Output Resources", GUILayout.Width(180)))
					{
						OutputResources();
					}
					EditorGUI.EndDisabledGroup();
					GUILayout.Space(8f);

					UtageEditorToolKit.EndGroup();
					GUILayout.Space(8f);
		*/
/*
		//リソースをアウトプット
		void OutputResources()
		{
			string dir = AssetDatabase.GetAssetPath(ResourcesRoot);
			OutputResources(dir, OutputResourcesPath);
		}

		//リソースをアウトプット
		void OutputResources(string srcDir, string destDir)
		{
			int count = 0;
			string[] assets = AssetDatabase.FindAssets("", new[] { srcDir });
			foreach (string assetId in assets)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(assetId);
				AssetFileType fileType = fileManager.PraseFileType(assetPath);
				switch (fileType)
				{
					case AssetFileType.UnityObject:
						MakeAssetBundle(assetPath, destDir);
						break;
					default:
						if (CopyFile(assetPath, assetPath.Replace(srcDir, destDir)))
						{
							++count;
						}
						break;
				}
			}
			Debug.Log("" + count + " files copied");
			if (count > 0)
			{
				AssetDatabase.Refresh();
			}
		}

		void MakeAssetBundle(string assetPath, string destDir)
		{
		}

		bool CopyFile(string srcFileName, string destFileName)
		{
			//ディレクトリがなければ作る
			string dir = Path.GetDirectoryName(destFileName);
			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}

			//新しいファイルのみコピー
			if (IsResoucesCopyNewerOnly)
			{
				if (File.Exists(destFileName))
				{
					if (File.GetLastWriteTime(srcFileName) <= File.GetLastWriteTime(destFileName))
					{
						return false;
					}
				}
			}

			if (fileManager.IsAlreadyEncodedFieType(srcFileName))
			{
				//エンコードが必要なタイプはエンコードする
				return fileManager.WriteEncode(destFileName, srcFileName, File.ReadAllBytes(srcFileName));
			}
			else
			{
				//通常ファイルコピー
				File.Copy(srcFileName, destFileName, true);
				return true;
			}
		}
		*/
	}
}