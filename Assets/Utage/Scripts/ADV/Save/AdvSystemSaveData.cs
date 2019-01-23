// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// ゲームで共通して使うシステムセーブデータ
	/// </summary>
	[AddComponentMenu("Utage/ADV/Internal/SystemSaveData")]
	public class AdvSystemSaveData : MonoBehaviour
	{
		/// <summary>
		/// システムセーブデータを使わない
		/// </summary>
		public bool DontUseSystemSaveData { get { return dontUseSystemSaveData; } set { dontUseSystemSaveData = value; } }
		[SerializeField]
		bool dontUseSystemSaveData = false;

		//アプリ終了時やスリープ時にオートセーブするか
		public bool IsAutoSaveOnQuit { get { return isAutoSaveOnQuit; } set { isAutoSaveOnQuit = value; } }
		[SerializeField]
		bool isAutoSaveOnQuit = true;

		FileIOManager FileIOManager { get { return this.fileIOManager ?? (this.fileIOManager = FindObjectOfType<FileIOManager>() as FileIOManager); } }
		[SerializeField]
		FileIOManager fileIOManager;

		/// <summary>
		/// ディレクトリ名
		/// </summary>
		public string DirectoryName
		{
			get { return directoryName; }
			set { directoryName = value; }
		}
		[SerializeField]
		string directoryName = "Save";

		/// <summary>
		/// ファイル名
		/// </summary>
		public string FileName
		{
			get { return fileName; }
			set { fileName = value; }
		}
		[SerializeField]
		string fileName = "system";

		/// <summary>
		/// ファイルパス
		/// </summary>
		public string Path { get; private set; }

		/// <summary>
		/// 既読のデータ
		/// </summary>
		public AdvReadHistorySaveData ReadData { get { return this.readData; } }
		AdvReadHistorySaveData readData = new AdvReadHistorySaveData();

		/// <summary>
		/// 選択肢のデータ
		/// </summary>
		public AdvSelectedHistorySaveData SelectionData { get { return this.selectionData; } }
		AdvSelectedHistorySaveData selectionData = new AdvSelectedHistorySaveData();

		/// <summary>
		/// 回想モード用のデータ
		/// </summary>
		public AdvGallerySaveData GalleryData { get { return this.galleryData; } }
		AdvGallerySaveData galleryData = new AdvGallerySaveData();

		AdvEngine Engine { get { return this.engine; } }
		AdvEngine engine;
		
		/// <summary>
		/// 初期化フラグ
		/// </summary>
		bool isInit = false;

		/// <summary>
		/// 初期化
		/// </summary>
		public void Init(AdvEngine engine)
		{
			this.engine = engine;
			if (!TryReadSaveData())
			{
				InitDefault();
			}
			isInit = true;
		}

		/// <summary>
		/// デフォルト値で初期化(初回でセーブデータがない場合)
		/// </summary>
		void InitDefault()
		{
			this.engine.Config.InitDefault();
		}

		bool TryReadSaveData()
		{
			if (DontUseSystemSaveData) return false;

			string saveDir = FilePathUtil.Combine(FileIOManager.SdkPersistentDataPath, DirectoryName);
			//セーブデータのディレクトリがなければ作成
			FileIOManager.CreateDirectory(saveDir);

			Path = FilePathUtil.Combine(saveDir, FileName);
			if (!FileIOManager.Exists(Path)) return false;

			return FileIOManager.ReadBinaryDecode(Path, ReadBinary);			
		}

		/// <summary>
		/// 書き込み
		/// </summary>
		public void Write()
		{
			if (!DontUseSystemSaveData && isInit)
			{
				FileIOManager.WriteBinaryEncode(Path, WriteBinary);
			}
		}

		//セーブデータを消去して終了(SendMessageでコールバックされるので名前固定)
		void OnDeleteAllSaveDataAndQuit()
		{
			Delete();
			isAutoSaveOnQuit = false;
		}

		/// <summary>
		/// セーブデータを消去
		/// </summary>
		public void Delete()
		{
			FileIOManager.Delete(Path);
		}
		//ゲーム終了時
		void OnApplicationQuit()
		{
			AutoSave();
		}

		//アプリがポーズしたとき
		void OnApplicationPause(bool pauseStatus)
		{
			if (pauseStatus)
			{
				AutoSave();
			}
		}

		void AutoSave()
		{
			//オートセーブ
			if (IsAutoSaveOnQuit)
			{
				Write();
			}
		}


		static readonly int MagicID = FileIOManager.ToMagicID('S', 'y', 's', 't');  //識別ID
		const int Version = 4;  //最新ファイルバージョン

		List<IBinaryIO> DataList
		{
			get
			{
				List<IBinaryIO> list = new List<IBinaryIO>()
				{
					ReadData,					//既読データ
					SelectionData,				//選択肢の選択済みデータ
					Engine.Config,				//コンフィグのデータ
					GalleryData,				//ギャラリーデータ
					Engine.Param.SystemData,	//システムセーブデータにセーブするパラメーターのデータ
				};
				return list;
			}
		}

		//バイナリ読み込み
		void ReadBinary(BinaryReader reader)
		{
			int magicID = reader.ReadInt32();
			if (magicID != MagicID)
			{
				throw new System.Exception("Read File Id Error");
			}

			int fileVersion = reader.ReadInt32();
			if (fileVersion == Version)
			{
				BinaryBuffer.Read(reader, DataList);
			}
			else
			{
				throw new System.Exception(LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.UnknownVersion, fileVersion));
			}
		}

		//バイナリ書き込み
		void WriteBinary(BinaryWriter writer)
		{
			writer.Write(MagicID);                  //識別ID
			writer.Write(Version);                  //バージョン

			BinaryBuffer.Write(writer, DataList);
		}
	}
}