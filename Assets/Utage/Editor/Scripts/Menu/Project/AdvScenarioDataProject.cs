// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Utage
{

	//「Utage」のシナリオデータ用のエクセルファイルのプロジェクトデータ
	public class AdvScenarioDataProject : ScriptableObject
	{
		public string ProjectName
		{
			get
			{
				return FilePathUtil.GetFileNameWithoutDoubleExtension(this.name);
			}
		}

		//古いデータ
		[SerializeField,HideInInspector]
		List<Object> excelList = new List<Object>();

		//章別に分けたエクセルのリスト
		[System.Serializable]
		public class ChapterData
		{
			public string chapterName = "";
			public List<Object> excelList = new List<Object>();
			public Object excelDir;

			public List<string> ExcelPathList
			{
				get
				{
					//エクセル直接指定
					List<string> list = UtageEditorToolKit.AssetsToPathList(excelList);

					//指定ディレクトリ以下のアセットを全て取得
					if (excelDir != null)
					{
						MainAssetInfo inputDirAsset = new MainAssetInfo(excelDir);
						foreach (MainAssetInfo asset in inputDirAsset.GetAllChildren())
						{
							if (asset.IsDirectory) continue;

							if (!ExcelParser.IsExcelFile (asset.AssetPath))
								continue;
							list.Add(asset.AssetPath);
						}
					}
					return list;
				}
			}

			public void InitDefault(List<Object> excelList)
			{
				this.chapterName = "Boot";
				this.excelList.Clear();
				this.excelList.AddRange( excelList);
			}
			public void InitDefault(string excelPath)
			{
				this.chapterName = "Boot";
				this.excelList.Clear();
				this.excelList.Add( UtageEditorToolKit.LoadAssetAtPath<Object>(excelPath));
			}
			public bool Contains(string[] list)
			{
				List<string> pathList = ExcelPathList;
				foreach (var item in list)
				{
					if (pathList.Contains(item)) return true;
				}
				return false;
			}
		}
		[SerializeField]
		List<ChapterData> chapterDataList = new List<ChapterData>();

		//章分けしたデータを取得
		public List<ChapterData> ChapterDataList
		{
			get
			{
				UpdateOldVersion();
				return chapterDataList;
			}
		}


		/// <summary>
		/// インポート時にWaitTypeをチェックするか
		/// </summary>
		public bool CheckWaitType
		{
			get { return checkWaitType; }
			set { checkWaitType = value; }
		}
		[SerializeField]
		bool checkWaitType = false;

		/// <summary>
		/// インポート時にセルの終端の空白をチェックする
		/// </summary>
		public bool CheckWhiteSpaceEndOfCell
		{
			get { return checkWhiteSpaceEndOfCell; }
			set { checkWhiteSpaceEndOfCell = value; }
		}
		[SerializeField]
		bool checkWhiteSpaceEndOfCell = true;

		/// <summary>
		/// インポート時にテキストの文字溢れをチェックするか
		/// </summary>
		public bool CheckTextCount
		{
			get { return checkTextCount; }
			set { checkTextCount = value; }
		}
		[SerializeField]
		bool checkTextCount = true;

		//インポート対象のシナリオアセット
		public AdvImportScenarios Scenarios
		{
			get
			{
				return scenarios;
			}
		}
		[SerializeField]
		AdvImportScenarios scenarios;

		public void CreateScenariosIfMissing()
		{
			if (this.scenarios != null) return;

			string path = AssetDatabase.GetAssetPath(this);
			path = FilePathUtil.Combine( FilePathUtil.GetDirectoryPath(path) , ProjectName + ".scenarios.asset" );
			//設定データのアセットをロードまたは作成
			this.scenarios = UtageEditorToolKit.GetImportedAssetCreateIfMissing<AdvImportScenarios>(path);
			this.scenarios.hideFlags = HideFlags.NotEditable;
			EditorUtility.SetDirty(this);
		}

		internal void InitDefault(string excelPath)
		{
			CreateScenariosIfMissing();
			ChapterData chapter = new ChapterData();
			chapter.InitDefault(excelPath);
			chapterDataList.Insert(0,chapter);
			EditorUtility.SetDirty(this);
		}


		void UpdateOldVersion()
		{
			if (excelList.Count>0)
			{
				ChapterData chapter = new ChapterData();
				chapter.InitDefault(this.excelList);
				this.chapterDataList.Insert(0,chapter);
				this.excelList.Clear();
				EditorUtility.SetDirty(this);
				Debug.Log("Update Old Project");
			}
		}


		//全てのエクセルパスを取得
		public List<string> AllExcelPathList
		{
			get
			{
				List<string> list = new List<string>();
				foreach (var item in ChapterDataList)
				{
					list.AddRange(item.ExcelPathList);
				}
				return list;
			}
		}

		//全てのエクセルパスを取得
		internal bool ContainsExcelPath(string[] importedAssets)
		{
			List<string> allpath = AllExcelPathList;
			foreach( string path in importedAssets )
			{
				if (allpath.Contains (path)) {
					return true;
				}
			}
			return false;
		}

		//自動インポートタイプ
		public enum AutoImportType
		{
			Always,				//常に
			OnUtageScene,		//宴のシーンのみ
			None,				//行わない
		};
		/// <summary>
		/// インポートタイプ
		/// </summary>
		[SerializeField]
		AutoImportType autoImportType = AutoImportType.Always;


		/// <summary>
		/// コンバート先のパス
		/// </summary>
		[SerializeField]
		string convertPath;
		public string ConvertPath
		{
			get { return convertPath; }
			set { convertPath = value; }
		}

		/// <summary>
		/// コンバートファイルのバージョン
		/// </summary>
		[SerializeField]
		int convertVersion = 0;
		public int ConvertVersion
		{
			get { return convertVersion; }
			set { convertVersion = value; }
		}

		/// <summary>
		/// コンバート後にバージョンを自動で更新するか
		/// </summary>
		[SerializeField]
		bool isAutoUpdateVersionAfterConvert = false;
		public bool IsAutoUpdateVersionAfterConvert
		{
			get { return isAutoUpdateVersionAfterConvert; }
			set { isAutoUpdateVersionAfterConvert = value; }
		}


		/// <summary>
		/// インポート時に自動でコンバートをするか
		/// </summary>
		[SerializeField]
		bool isAutoConvertOnImport = false;
		public bool IsAutoConvertOnImport
		{
			get { return isAutoConvertOnImport; }
			set { isAutoConvertOnImport = value; }
		}

		public bool IsEnableImport
		{
			get
			{
				bool isEnableImport = false;
				foreach (ChapterData chapter in ChapterDataList)
				{
					foreach (string path in chapter.ExcelPathList)
					{
						if (null != path)
						{
							isEnableImport = true;
							break;
						}
					}
				}
				return isEnableImport;
			}
		}

		public bool IsEnableConvert
		{
			get { return IsEnableImport && !string.IsNullOrEmpty(ConvertPath); }
		}

		/// <summary>
		/// 宴用のカスタムインポート設定を強制するSpriteフォルダassetのリスト
		/// </summary>
		[SerializeField]
		List<Object> customInportSpriteFolders = new List<Object>();
		public List<Object> CustomInportSpriteFolders
		{
			get { return customInportSpriteFolders; }
		}

		///宴用のカスタムインポート設定を強制するSpriteフォルダassetのリストを追加
		public void AddCustomImportSpriteFolders(List<Object> assetList)
		{
			CustomInportSpriteFolders.AddRange(assetList);
		}

		/// <summary>
		/// 宴用のカスタムインポート設定を強制するAudioフォルダassetのリスト
		/// </summary>
		[SerializeField]
		List<Object> customInportAudioFolders = new List<Object>();
		public List<Object> CustomInportAudioFolders
		{
			get { return customInportAudioFolders; }
		}
		///宴用のカスタムインポート設定を強制するSpriteフォルダassetのリストを追加
		public void AddCustomImportAudioFolders( List<Object> assetList )
		{
			CustomInportAudioFolders.AddRange(assetList);
		}

		/// <summary>
		/// 宴用のカスタムインポート設定を強制するMovieフォルダassetのリスト
		/// </summary>
		[SerializeField]
		List<Object> customInportMovieFolders = new List<Object>();
		public List<Object> CustomInportMovieFolders
		{
			get { return customInportMovieFolders; }
		}

		/// 簡易インポートをするか
		[SerializeField]
		bool quickImport = false;
		public bool QuickImport { get { return quickImport; } }

		/// エクセルの数式解析するか
		[SerializeField]
		bool parseFormula = false;
		public bool ParseFormula { get { return parseFormula; } }

		/// エクセルの数字解析（桁区切り対策など）
		[SerializeField]
		bool parseNumreic = false;
		public bool ParseNumreic { get { return parseNumreic; } }

		///宴用のカスタムインポート設定を強制するMovieフォルダassetのリストを追加
		public void AddCustomImportMovieFolders(List<Object> assetList)
		{
			CustomInportMovieFolders.AddRange(assetList);
		}

		/// <summary>
		/// 宴用のカスタムインポート設定を強制するAudioアセットかチェック
		/// </summary>
		public bool IsCustomImportAudio(AssetImporter importer)
		{
			string assetPath = importer.assetPath;
			foreach( Object folderAsset in CustomInportAudioFolders)
			{
				if (assetPath.StartsWith(AssetDatabase.GetAssetPath(folderAsset)))
				{
					return true;
				}
			}
			return false;
		}

		public enum TextureType
		{
			Unknown,
			Character,
			Sprite,
			BG,
			Event,
			Thumbnail,
		};
		/// <summary>
		/// 宴用のカスタムインポート設定を強制するSpriteアセットかチェック
		/// </summary>
		public TextureType ParseCustomImportTextureType(AssetImporter importer)
		{
			string assetPath = importer.assetPath;
			foreach (Object folderAsset in CustomInportSpriteFolders)
			{
				string floderPath = AssetDatabase.GetAssetPath(folderAsset);
				if (assetPath.StartsWith(floderPath))
				{
					string name = System.IO.Path.GetFileName( floderPath );
					TextureType type;
					if (ParserUtil.TryParaseEnum<TextureType>(name, out type))
					{
						return type;
					}
					return TextureType.Unknown;
				}
			}
			return TextureType.Unknown;
		}

		/// <summary>
		/// 宴用のカスタムインポート設定を強制するMovieアセットかチェック
		/// </summary>
		public bool IsCustomImportMovie(AssetImporter importer)
		{
			string assetPath = importer.assetPath;
			foreach (Object folderAsset in CustomInportMovieFolders)
			{
				if (assetPath.StartsWith(AssetDatabase.GetAssetPath(folderAsset)))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 管理対象のリソースを再インポート
		/// </summary>
		public void ReImportResources()
		{
			ImportAssetOptions options = ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive;
			foreach( Object folder in CustomInportSpriteFolders )
			{
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(folder), options);
			}
			foreach (Object folder in CustomInportAudioFolders)
			{
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(folder), options);
			}
			AssetDatabase.Refresh();
		}


		internal bool CheckAutoImportType()
		{
			switch (autoImportType)
			{
				case AutoImportType.None:
					return false;
				case AutoImportType.OnUtageScene:
					if (UtageEditorToolKit.FindComponentAllInTheScene<AdvEngine>() == null )
					{
						return false;
					}
					return true;
				case AutoImportType.Always:
				default:
					return true;
			}
		}
	}
}