// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.Events;
using UtageExtensions;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif


namespace Utage
{

	/// <summary>
	/// セーブデータ管理クラス
	/// </summary>
	[AddComponentMenu("Utage/ADV/Internal/SaveManager")]
	public class AdvSaveManager : MonoBehaviour
	{
		protected virtual FileIOManager FileIOManager { get { return this.fileIOManager ?? (this.fileIOManager = FindObjectOfType<FileIOManager>()); } }
		[SerializeField]
		protected FileIOManager fileIOManager;

		/// <summary>
		/// セーブのタイプ
		/// </summary>
		public enum SaveType
		{
			Default,		//全ページ
			SavePoint,		//セーブポイントのみ
			Disable,        //セーブをしない
		};
		public virtual SaveType Type { get { return type; } }
		[SerializeField]
		protected SaveType type = SaveType.Default;


		/// <summary>
		/// オートセーブが有効か
		/// </summary>
		public virtual bool IsAutoSave { get { return isAutoSave; } }
		[SerializeField]
		protected bool isAutoSave = true;
		
		/// <summary>
		/// ディレクトリ名
		/// </summary>
		public virtual string DirectoryName
		{
			get { return directoryName; }
			set { directoryName = value; }
		}
		[SerializeField]
		protected string directoryName = "Save";

		/// <summary>
		/// セーブファイル名(実際には連番のIDがさらに末尾につく)
		/// </summary>
		public virtual string FileName
		{
			get { return fileName; }
			set { fileName = value; }
		}
		[SerializeField]
		protected string fileName = "save";


		/// <summary>
		/// セーブデータの設定
		/// </summary>
		[System.Serializable]
		protected class SaveSetting
		{
			/// <summary>
			/// セーブ時のスクリーンショット解像度（幅）
			/// </summary>
			public int CaptureWidth { get { return this.captureWidth; } }
			[SerializeField]
			int captureWidth = 256;

			/// <summary>
			/// セーブ時のスクリーンショット解像度（高さ）
			/// </summary>
			public int CaptureHeight { get { return this.captureHeight; } }
			[SerializeField]
			int captureHeight = 256;

			/// <summary>
			/// セーブファイルの数
			/// </summary>
			public int SaveMax { get { return this.saveMax; } }
			[SerializeField]
			int saveMax = 9;
		}

		[SerializeField]
		protected SaveSetting defaultSetting = new SaveSetting();		//セーブデータの設定（デフォルト）
		[SerializeField]
		protected SaveSetting webPlayerSetting;       //セーブデータの設定（WebPlayer用）

#if UNITY_WEBPLAYER || UNITY_WEBGL
		public virtual int CaptureWidth {get {return webPlayerSetting.CaptureWidth;}}
		public virtual int CaptureHeight { get { return webPlayerSetting.CaptureHeight; } }
		protected virtual int SaveMax { get { return webPlayerSetting.SaveMax; } }
#else
		public int CaptureWidth { get { return defaultSetting.CaptureWidth; } }
		public int CaptureHeight { get { return defaultSetting.CaptureHeight; } }
		protected virtual int SaveMax { get { return defaultSetting.SaveMax; } }
#endif

		public List<GameObject> CustomSaveDataObjects;

		public virtual List<IBinaryIO> CustomSaveDataIOList
		{
			get
			{
				List<IBinaryIO> list = new List<IBinaryIO>();
				foreach (GameObject go in CustomSaveDataObjects)
				{
					IAdvSaveData io = go.GetComponent<IAdvSaveData>();
					if (io == null)
					{
						Debug.LogError(go.name + "is not contains IAdvCustomSaveDataIO ", go);
						continue;
					}
					else
					{
						list.Add(io);
					}
				}
				return list;
			}
		}

		//バージョンアップ用のセーブデータ
		public virtual List<IBinaryIO> GetSaveIoListCreateIfMissing(AdvEngine engine)
		{
			if (saveIoList == null)
			{
				saveIoList = new List<IBinaryIO>();
				saveIoList.AddRange(this.GetComponentsInChildren<IAdvSaveData>(true));
			}
			return saveIoList;
		}
		protected List<IBinaryIO> saveIoList;

		/// <summary>
		/// オートセーブ
		/// </summary>
		public virtual AdvSaveData AutoSaveData { get { return autoSaveData; } }
		protected AdvSaveData autoSaveData;

		/// <summary>
		/// オートセーブ用のデータ
		/// </summary>
		public virtual AdvSaveData CurrentAutoSaveData { get { return currentAutoSaveData; } }
		protected AdvSaveData currentAutoSaveData;


		/// <summary>
		/// クイックセーブ用のデータ
		/// </summary>
		public virtual AdvSaveData QuickSaveData { get { return quickSaveData; } }
		protected AdvSaveData quickSaveData;

		/// <summary>
		/// セーブデータのリスト
		/// </summary>
		public virtual List<AdvSaveData> SaveDataList{get{return saveDataList;}}
		protected List<AdvSaveData> saveDataList;

		/// <summary>
		/// キャプチャー画面
		/// </summary>
		public virtual Texture2D CaptureTexture
		{
			get
			{
				return captureTexture;
			}
			set
			{
				ClearCaptureTexture();
				captureTexture = value;
			}
		}
		protected Texture2D captureTexture;


		/// <summary>
		/// キャプチャ画像をクリア
		/// </summary>
		public void ClearCaptureTexture()
		{
			if (captureTexture != null)
			{
				UnityEngine.Object.Destroy(captureTexture);
				captureTexture = null;
			}			
		}

		/// <summary>
		/// 初期化
		/// </summary>
		public virtual void Init()
		{
			//セーブデータのディレクトリがなければ作成
			FileIOManager.CreateDirectory(ToDirPath());

			//オートセーブデータ。読み込み用と書き込み用
			autoSaveData = new AdvSaveData( AdvSaveData.SaveDataType.Auto, ToFilePath("Auto")); ;
			currentAutoSaveData = new AdvSaveData(AdvSaveData.SaveDataType.Auto, ToFilePath("Auto")); ;

			quickSaveData = new AdvSaveData(AdvSaveData.SaveDataType.Quick, ToFilePath("Quick")); ;

			saveDataList = new List<AdvSaveData>();
			for (int i = 0; i < SaveMax; i++)
			{
				AdvSaveData data = new AdvSaveData(AdvSaveData.SaveDataType.Default, ToFilePath("" + (i + 1)));
				saveDataList.Add(data);
			}
		}

		protected virtual string ToFilePath(string id)
		{
			return FilePathUtil.Combine(ToDirPath(),FileName + id);
		}
		protected virtual string ToDirPath()
		{
			return FilePathUtil.Combine(FileIOManager.SdkPersistentDataPath, DirectoryName + "/");
		}

		/// <summary>
		/// オートセーブ用のデータを読み込み
		/// </summary>
		/// <returns></returns>
		public virtual bool ReadAutoSaveData()
		{
			if (!isAutoSave) return false;
			return ReadSaveData(AutoSaveData);
		}

		/// <summary>
		/// クイックセーブ用のデータを読み込み
		/// </summary>
		/// <returns></returns>
		public virtual bool ReadQuickSaveData()
		{
			return ReadSaveData(QuickSaveData);
		}

		/// <summary>
		/// セーブデータを全て読み込み
		/// </summary>
		public virtual void ReadAllSaveData()
		{
			Profiler.BeginSample("ReadAllSaveData");
			ReadAutoSaveData();
			ReadQuickSaveData();
			foreach (AdvSaveData item in SaveDataList)
			{
				ReadSaveData(item);
			}
			Profiler.EndSample();
		}

		/// <summary>
		/// セーブデータを読み込み
		/// </summary>
		/// <param name="saveData">読み込むセーブデータ</param>
		/// <returns>読み込めたかどうか</returns>
		protected virtual bool ReadSaveData(AdvSaveData saveData)
		{
			if (FileIOManager.Exists(saveData.Path))
			{
				return FileIOManager.ReadBinaryDecode(saveData.Path, saveData.Read);
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// オートセーブデータを更新（書き込みはしない）
		/// </summary>
		public virtual void UpdateAutoSaveData(AdvEngine engine)
		{
			Profiler.BeginSample("UpdateAutoSaveData");
			CurrentAutoSaveData.UpdateAutoSaveData(engine, null, CustomSaveDataIOList, GetSaveIoListCreateIfMissing(engine));
			Profiler.EndSample();
		}

		/// <summary>
		/// セーブデータを書き込み
		/// その場の状態を書き込まず、各ページ冒頭のオートセーブデータを利用する
		/// </summary>
		/// <param name="engine">ADVエンジン</param>
		/// <param name="saveData">書き込むセーブデータ</param>
		public virtual void WriteSaveData(AdvEngine engine, AdvSaveData saveData)
		{
			if (!CurrentAutoSaveData.IsSaved)
			{
				Debug.LogError("SaveData is Disabled");
				return;
			}

			//セーブ
			saveData.SaveGameData(CurrentAutoSaveData, engine, UtageToolKit.CreateResizeTexture(CaptureTexture, CaptureWidth, CaptureHeight));
			FileIOManager.WriteBinaryEncode(saveData.Path, saveData.Write);
		}

		//セーブデータを消去して終了(SendMessageでコールバックされるので名前固定)
		protected virtual void OnDeleteAllSaveDataAndQuit()
		{
			DeleteAllSaveData();
			isAutoSave = false;
		}

		/// <summary>
		/// セーブデータを全て消去
		/// </summary>
		public virtual void DeleteAllSaveData()
		{
			DeleteSaveData(AutoSaveData);
			DeleteSaveData(QuickSaveData);
			foreach (AdvSaveData item in SaveDataList)
			{
				DeleteSaveData(item);
			}
		}

		/// <summary>
		/// セーブデータを削除
		/// </summary>
		/// <param name="saveData">削除するセーブデータ</param>
		public virtual void DeleteSaveData(AdvSaveData saveData)
		{
			if (FileIOManager.Exists(saveData.Path))
			{
				FileIOManager.Delete(saveData.Path);
			}
			saveData.Clear();
		}
		//ゲーム終了時
		protected virtual void OnApplicationQuit()
		{
			AutoSave();
		}

		//アプリがポーズしたとき
		protected virtual void OnApplicationPause(bool pauseStatus)
		{
			if (pauseStatus)
			{
				AutoSave();
			}
		}

		protected virtual void AutoSave()
		{
			if (IsAutoSave && AutoSaveData != null)
			{
				if (CurrentAutoSaveData.IsSaved)
				{
					FileIOManager.WriteBinaryEncode(CurrentAutoSaveData.Path, CurrentAutoSaveData.Write);
				}
			}
		}
	}
}