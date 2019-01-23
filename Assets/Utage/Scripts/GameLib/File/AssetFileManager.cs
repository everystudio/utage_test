﻿// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtageExtensions;

namespace Utage
{

	/// <summary>
	/// ファイル管理
	/// </summary>
	[AddComponentMenu("Utage/Lib/File/AssetFileManager")]
	[RequireComponent(typeof(StaticAssetManager))]
//	[RequireComponent(typeof(ConvertFileListManager))]
	public partial class AssetFileManager : MonoBehaviour
	{
		public FileIOManager FileIOManager
		{
			get { return this.GetComponentCache<FileIOManager>(ref fileIOManager); }
			set { fileIOManager = value; }
		}
		[SerializeField,UnityEngine.Serialization.FormerlySerializedAs("fileIOManger")]
		FileIOManager fileIOManager;

		public bool EnableResourcesLoadAsync
		{
			get { return enableResourcesLoadAsync; }
			set { enableResourcesLoadAsync = value; }
		}
		[SerializeField]
		bool enableResourcesLoadAsync = true;

		//ダウンロードエラー時に、自動でリトライする回数
		public float TimeOutDownload
		{
			get { return timeOutDownload; }
			set { timeOutDownload = value; }
		}
		[SerializeField]
		float timeOutDownload = 10;                 //タイムアウト時間

		//ダウンロードエラー時に、自動でリトライする回数
		public int AutoRetryCountOnDonwloadError
		{
			get { return autoRetryCountOnDonwloadError; }
			set { autoRetryCountOnDonwloadError = value; }
		}
		[SerializeField]
		int autoRetryCountOnDonwloadError = 5;

		[SerializeField]
		int loadFileMax = 5;                    //同時にロードするファイルの最大数

		// ロード済みファイルの最大数（この数を超えたら、未使用ファイルをアンロードする）
		public int MaxFilesOnMemory { get { return rangeOfFilesOnMemory.Max; } }

		// 未使用ファイルをアンロードするときに、ロード済みファイル数がこの数以下になるようにする。
		// 使用中ファイルはアンロードしないので、アンロード後もロード済みファイル数がこの数以上になることはある
		public int MinFilesOnMemory { get { return rangeOfFilesOnMemory.Min; } }

		[SerializeField,MinMax(0,100)]
		MinMaxInt rangeOfFilesOnMemory = new MinMaxInt() { Min = 10, Max = 20 };


		//アンロード時の処理タイプ
		internal enum UnloadType
		{
			None,						//特に何もしない
			UnloadUnusedAsset,			//アセットバンドルがある場合はUnloadUnusedAsset
			UnloadUnusedAssetAlways,    //常にUnloadUnusedAsset
			NoneAndUnloadAssetBundleTrue,   //UnloadUnusedAssetはせず、AssetBundle.Unloade(true)を呼ぶ
		};
		[SerializeField]
		UnloadType unloadType = UnloadType.UnloadUnusedAsset;
		internal UnloadType UnloadUnusedType { get { return unloadType; } }

		[SerializeField]
		internal bool isOutPutDebugLog = false;								//ダウンロードのログをコンソールに出力する
		[SerializeField]
		internal bool isDebugCacheFileName = false;							//キャッシュファイルパスをデバッグモードにする（隠蔽せずに公開する）
		[SerializeField]
		internal bool isDebugBootDeleteChacheTextAndBinary = false;					//起動時に、テキストやバイナリのキャッシュを削除する
		[SerializeField]
		internal bool isDebugBootDeleteChacheAll = false;							//起動時に、キャッシュファイルを全て消す

		public AssetFileManagerSettings Settings
		{
			get { return settings; }
			set{ settings = value; }
		}
		[SerializeField]
		AssetFileManagerSettings settings;


		public AssetBundleInfoManager AssetBundleInfoManager
		{
			get { return this.GetComponentCacheCreateIfMissing<AssetBundleInfoManager>(ref assetBundleInfoManager); }
			set { assetBundleInfoManager = value; }
		}
		[SerializeField]
		AssetBundleInfoManager assetBundleInfoManager;

		[SerializeField]
		AssetFileDummyOnLoadError dummyFiles = new AssetFileDummyOnLoadError();

		List<AssetFileBase> loadingFileList = new List<AssetFileBase>();		//ロード中ファイルリスト
		List<AssetFileBase> loadWaitFileList = new List<AssetFileBase>();		//ロード待ちファイルリスト
		List<AssetFileBase> usingFileList = new List<AssetFileBase>();			//使用中のファイルリスト
		Dictionary<string, AssetFileBase> fileTbl= new Dictionary<string, AssetFileBase>(); //管理中のファイルリスト

		//カスタムロードのマネージャー
		CustomLoadManager CustomLoadManager { get { return this.GetComponentCacheCreateIfMissing<CustomLoadManager>(ref customLoadManager); } }
		CustomLoadManager customLoadManager;

		//動的にロードしないリソースのリスト
		StaticAssetManager StaticAssetManager { get { return this.GetComponentCacheCreateIfMissing<StaticAssetManager>(ref staticAssetManager); } }
		StaticAssetManager staticAssetManager;

		Action<AssetFile> callbackError;

		public Action<AssetFile> CallbackError
		{
			get { return callbackError ??( callbackError = CallbackFileLoadError); }
			set { callbackError = value; }
		}

        bool isWaitingRetry;

        // ロードエラー時のデフォルトコールバック
        void CallbackFileLoadError(AssetFile file)
		{
            AssetFileBase errorFile =  file as AssetFileBase;
			string errorMsg = file.LoadErrorMsg + "\n" + file.FileName;
			Debug.LogError(errorMsg);

            if (SystemUi.GetInstance() != null)
            {
                if (isWaitingRetry)
                {
					StartCoroutine(CoWaitRetry(errorFile));
                }
                else
                {
                    isWaitingRetry = true;
                    //リロードを促すダイアログを表示
                    SystemUi.GetInstance().OpenDialog1Button(
                        errorMsg, LanguageSystemText.LocalizeText(SystemText.Retry),
                        () =>
                        {
                            isWaitingRetry = false;
                            ReloadFileSub(errorFile);
                        }
                        );
                }
            }
            else
            {
                ReloadFileSub(errorFile);
            }
        }

		//リトライダイアログからの応答を待つ
        IEnumerator CoWaitRetry(AssetFileBase file)
        {
            while (isWaitingRetry)
            {
                yield return null;
            }
            ReloadFileSub(file);
        }

        void Awake()
		{
			if (null == instance)
			{
				instance = this;
			}
		}

		///＊＊＊以下、ファイル追加・ロード処理＊＊＊///


		// 管理ファイルを追加
		AssetFileBase AddSub(string path, IAssetFileSettingData settingData)
		{
			AssetFileBase file;
			//管理テーブルにあるなら、そこから
			if (!fileTbl.TryGetValue(path, out file))
			{
				if (path.Contains(" "))
				{
					Debug.LogWarning("[" + path + "] contains white space");
				}
				AssetBundleInfo assetBundleInfo = AssetBundleInfoManager.FindAssetBundleInfo(path);
				AssetFileInfo fileInfo = new AssetFileInfo(path, settings, assetBundleInfo);

				//staticなアセットにあるなら、そこから
				file = StaticAssetManager.FindAssetFile(this, fileInfo, settingData);
				if (file == null)
				{
					//カスタムロードなアセットにあるなら、そこから
					file = CustomLoadManager.Find(this, fileInfo, settingData);
					if (file == null)
					{
						//宴形式の通常ファイルロード
						file = new AssetFileUtage(this, fileInfo, settingData);
					}
				}
				fileTbl.Add(path, file);
			}
			return file;
		}

		// ダウンロード
		void DownloadSub(AssetFileBase file)
		{
			if (file.CheckCacheOrLocal())
			{
				return;
			}
			file.ReadyToLoad(AssetFileLoadPriority.DownloadOnly, null);
			AddLoadFile(file);
		}

		// プリロード
		void PreloadSub(AssetFileBase file, System.Object referenceObj)
		{
			AddUseList(file);
			file.ReadyToLoad(AssetFileLoadPriority.Preload, referenceObj);
			AddLoadFile(file);
		}
		//	ファイルのバックグランドロード
		AssetFile BackGroundLoadSub(AssetFileBase file, System.Object referenceObj)
		{
			AddUseList(file);
			file.ReadyToLoad(AssetFileLoadPriority.BackGround, referenceObj);
			AddLoadFile(file);
			return file;
		}
		
		// 通常のロード
		AssetFile LoadSub(AssetFileBase file, System.Object referenceObj)
		{
			AddUseList(file);
			file.ReadyToLoad(AssetFileLoadPriority.Default, referenceObj);
			AddLoadFile(file);
			return file;
		}

		//使用中のファイルリストとして設定
		void AddUseList(AssetFileBase file)
		{
			if (!usingFileList.Contains (file)) 
			{
				usingFileList.Add (file);
			}
		}

		// コールバックつきのロード
		void LoadSub(AssetFileBase file, System.Action<AssetFile> onComplete)
		{
			StartCoroutine(CoLoadWait(file, onComplete));
		}

		// コールバックつきのロードコルーチン
		IEnumerator CoLoadWait(AssetFileBase file, System.Action<AssetFile> onComplete)
		{
			if (file.IsLoadEnd)
			{
				onComplete(file);
			}
			else
			{
				LoadSub(file, this);
				while (!file.IsLoadEnd) yield return null;
				onComplete(file);
			}
		}

		//ロード待ちリストに追加
		void AddLoadFile(AssetFileBase file)
		{
			TryAddLoadingFileList(file);
		}

		//ロード待ちリストを追加
		bool TryAddLoadingFileList(AssetFileBase file)
		{
			if (file.IsLoadEnd) return false;
			if (loadingFileList.Contains(file))
			{
				return false;
			}

			if (loadingFileList.Count < loadFileMax)
			{
				loadingFileList.Add(file);
				if (isOutPutDebugLog) Debug.Log("Load Start :" + file.FileName);
				StartCoroutine(LoadAsync(file));
				return true;
			}
			else if (!loadWaitFileList.Contains(file))
			{
				loadWaitFileList.Add(file);
				return false;
			}
			else
			{
				return false;
			}
		}

		IEnumerator LoadAsync(AssetFileBase file)
		{
			yield return file.LoadAsync(
					//ロード成功
					() =>
					{
						if (isOutPutDebugLog) Debug.Log("Load End :" + file.FileName);
						loadingFileList.Remove(file);
						LoadNextFile();
					},
					//ロード失敗
					() =>
					{
						//ロード失敗
						if (dummyFiles.isEnable)
						{
							//ダミーファイルをロード
							if (dummyFiles.outputErrorLog)
							{
								Debug.LogError("Load Failed. Dummy file loaded:" + file.FileName + "\n" + file.LoadErrorMsg);
							}
							file.LoadDummy(dummyFiles);
							loadingFileList.Remove(file);
							LoadNextFile();
						}
						else
						{
							Debug.LogError("Load Failed :" + file.FileName + "\n" + file.LoadErrorMsg);
							//ロード失敗処理
							if (CallbackError != null)
							{
								CallbackError(file);
							}
						}
					}
				);
		}


		//ファイルリロード
		void ReloadFileSub(AssetFileBase file)
		{
			StartCoroutine(ReloadFileSubAsync(file));
		}

		//ファイルリロード(無限ループよけに1フレーム遅らせる)
		IEnumerator ReloadFileSubAsync(AssetFileBase file)
		{
			yield return null;
			yield return StartCoroutine(LoadAsync(file));
		}

		//ロード待ちのファイルをロードする
		void LoadNextFile()
		{
			AssetFileBase next = null;
			foreach (AssetFileBase file in loadWaitFileList)
			{
				if (next == null)
				{
					next = file;
				}
				else
				{
					if (file.Priority < next.Priority)
					{
						next = file;
					}
				}
			}
			if (next != null)
			{
				if (next.IsLoadEnd)
				{
					//ダミーファイルなどによって既にロード済み
					loadWaitFileList.Remove(next);
				}
				else if (TryAddLoadingFileList(next))
				{
					loadWaitFileList.Remove(next);
				}
				else
				{
					Debug.LogError("Failed To Load file " + next.FileName);
				}
			}
		}

		///＊＊＊以下、未使用ファイルを管理してロードとメモリ効率を高める処理＊＊＊///

		void LateUpdate()
		{
			int totalLoaded = GetTotalOnMemoryFileCount ();
			if (totalLoaded > MaxFilesOnMemory)
			{
				UnloadUnusedFileList(totalLoaded - MinFilesOnMemory);
			}
		}


		void OnDestroy()
		{
			UnloadUnusedFileList(Int32.MaxValue);
		}

		//メモリを消費しているファイルの数
		int GetTotalOnMemoryFileCount()
		{
			int count = loadingFileList.Count;
			foreach( var file in usingFileList )
			{
				if (file.IgnoreUnload || !file.IsLoadEnd)
					continue;

				++count;
			}
			return count;
		}

		//指定の数のシステムメモリにプールされてる未使用ファイルをアンロードして、メモリを解放
		void UnloadUnusedFileList(int count)
		{
			if (usingFileList.Count <= 0 || count <= 0)
			{
				return;
			}

			//指定個数だけアンロード
			int unloadUnusedCount = 0;
			List<AssetFileBase> newList = new List<AssetFileBase>();
			foreach (AssetFileBase file in usingFileList)
			{
				//ロード中だったり、参照がのこっているのは対象としない
				if (count <= 0 || file.IgnoreUnload || !file.IsLoadEnd || file.ReferenceCount > 0)
				{
					newList.Add(file);
				}
				else
				{
					if (isOutPutDebugLog) Debug.Log("Unload " + file.FileName);
					file.Unload();
					--count;
					if (file.FileType == AssetFileType.UnityObject)
					{
						//UnloadUnusedAssetsが必要かカウント
						++unloadUnusedCount;
					}
				}
			}
			UnloadUnusedAssets(unloadUnusedCount);
			usingFileList = newList;
		}

		//未使用リソースをすべて解放するUnloadUnusedAssetsを呼ぶ
		//重いのでなるべく呼ばないために本当に必要か色々チェック
		bool unloadingUnusedAssets;
		void UnloadUnusedAssets(int count)
		{
			switch (unloadType)
			{
				//必要な数が0以下ならしない
				case UnloadType.UnloadUnusedAsset:
					if (count <= 0) return;
					break;
				//問答無用で毎回
				case UnloadType.UnloadUnusedAssetAlways:
					break;
				//アンロードしない
				case UnloadType.None:
				case UnloadType.NoneAndUnloadAssetBundleTrue:
				default:
					return;
			}
			//解放中なら二重解放はしない
			if (unloadingUnusedAssets) return;
			//動いてないならコルーチン回せない
			if (!this.gameObject.activeInHierarchy) return;

			StartCoroutine(UnloadUnusedAssetsAsync());
		}
		IEnumerator UnloadUnusedAssetsAsync()
		{
			if (isOutPutDebugLog) Debug.Log("UnloadUnusedAssets");
			unloadingUnusedAssets = true;
			yield return Resources.UnloadUnusedAssets();
			unloadingUnusedAssets = false;
		}


		///＊＊＊以下、外部から値を取得するための処理＊＊＊///

		//ロード終了チェック
		bool IsLoadEnd( AssetFileLoadPriority priority )
		{
			foreach (var file in loadingFileList)
			{
				if( file.Priority <= priority )
				{
					if (!file.IsLoadEnd) return false;
				}
			}
			foreach (var file in loadWaitFileList)
			{
				if (file.Priority <= priority)
				{
					if (!file.IsLoadEnd) return false;
				}
			}
			return true;
		}

		//ロード中のファイル数
		int CountLoading(AssetFileLoadPriority priority)
		{
			int count = 0;
			foreach (var file in loadingFileList)
			{
				if (file.Priority <= priority)
				{
					if (!file.IsLoadEnd)
					{
						++count;
					}
				}
			}
			foreach (var file in loadWaitFileList)
			{
				if (file.Priority <= priority)
				{
					if (!file.IsLoadEnd)
					{
						++count;
					}
				}
			}
			return count;
		}
	}
}