﻿// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UtageExtensions;

namespace Utage
{

	public class AssetBundleInfo
	{
		public string Url { get; private set; }
		public Hash128 Hash { get; private set; }
		public int Version { get; private set; }
		internal AssetBundleInfo(string url, Hash128 hash)
		{
			this.Url = url;
			this.Hash = hash;
			this.Version = int.MinValue;
		}
		internal AssetBundleInfo(string url, int version)
		{
			this.Url = url;
			this.Version = version;
		}
	}

	//AssetBundleの情報管理クラス
	[AddComponentMenu("Utage/Lib/File/AssetBundleInfoManager")]
	public class AssetBundleInfoManager : MonoBehaviour
	{
		//ロード失敗したときのリトライ回数
		public int RetryCount
		{
			get { return retryCount; }
			set { retryCount = value; }
		}
		[SerializeField]
		int retryCount = 5;

		//ロードのタイムアウト時間
		public int TimeOut
		{
			get { return retryCount; }
			set { retryCount = value; }
		}
		[SerializeField]
		float timeOut = 5;

		//DLしたマニフェストをキャッシュ書き込みする
		public bool UseCacheManifest
		{
			get { return useCacheManifest; }
			set { useCacheManifest = value; }
		}
		[SerializeField]
		bool useCacheManifest = true;

		//DLしたマニフェストを書き込むフォルダ名
		public string CacheDirectoryName
		{
			get { return cacheDirectoryName; }
			set { cacheDirectoryName = value; }
		}
		[SerializeField]
		string cacheDirectoryName = "Cache";

		AssetFileManager AssetFileManager { get { return this.GetComponentCache<AssetFileManager>(ref assetFileManager); } }
		[SerializeField]
		AssetFileManager assetFileManager;

		FileIOManager FileIOManager { get { return AssetFileManager.FileIOManager; } }

		//大文字と小文字を無視するDictionary
		Dictionary<string, AssetBundleInfo> dictionary = new Dictionary<string, AssetBundleInfo>(StringComparer.OrdinalIgnoreCase);

		//アセットバンドルのマニフェスト名
		const string AssetBundleManifestName = "assetbundlemanifest";

		//アセットバンドルの情報を取得
		public AssetBundleInfo FindAssetBundleInfo(string path)
		{
			//ファイル情報を取得or作成
			AssetBundleInfo info;
			//大文字と小文字を無視するDictionaryでアセットバンドルの小文字化に対応している
			if (!dictionary.TryGetValue(path, out info))
			{
				string key = FilePathUtil.ChangeExtension(path, ".asset");
				if (!dictionary.TryGetValue(key, out info))
				{
					return null;
				}
			}
			return info;
		}

		//アセットバンドルの情報を追加(カスタムしたアセットバンドルの情報を設定する場合はここを使う)
		public void AddAssetBundleInfo(string resourcePath, string assetBunleUrl, int assetBunleVersion)
		{
			try
			{
				dictionary.Add(resourcePath, new AssetBundleInfo(assetBunleUrl, assetBunleVersion));
			}
			catch
			{
				Debug.LogError(resourcePath + "is already contains in assetbundleManger");
			}
		}

		//アセットバンドルマニフェストの情報を追加
		public void AddAssetBundleManifest(string rootUrl, AssetBundleManifest manifest)
		{
			foreach (string name in manifest.GetAllAssetBundles())
			{
				Hash128 assetBundleHash = manifest.GetAssetBundleHash(name);
				string path = FilePathUtil.Combine(rootUrl, name);
				try
				{
					dictionary.Add(path, new AssetBundleInfo(path, assetBundleHash));
				}
				catch
				{
					Debug.LogError(path + "is already contains in assetbundleManger");
				}
			}
		}

		//アセットバンドルマニフェストをDLして情報を追加
		public IEnumerator DownloadManifestAsync(string rootUrl, string relativeUrl, Action onComplete, Action onFailed)
		{
			string url = FilePathUtil.Combine(rootUrl, relativeUrl);
			url = FilePathUtil.ToCacheClearUrl(url);
			WWWEx wwwEx = new WWWEx(url);
			if (UseCacheManifest)
			{
				wwwEx.IoManager = FileIOManager;
				wwwEx.WriteLocal = true;
				wwwEx.WritePath = GetCachePath(relativeUrl);
			}
			wwwEx.OnUpdate = OnDownloadingManifest;
			wwwEx.RetryCount = retryCount;
			wwwEx.TimeOut = timeOut;
//			Debug.Log("Load Start " + url);
			return wwwEx.LoadAssetBundleByNameAsync<AssetBundleManifest>(
							AssetBundleManifestName,
							false,
							(manifest) =>
							{
								AddAssetBundleManifest(rootUrl, manifest);
								if (onComplete != null) onComplete();
							},
							() =>
							{
								if (onFailed != null) onFailed();
							}
							);
		}

		void OnDownloadingManifest(WWWEx wwwEx)
		{
//			Debug.Log(wwwEx.Progress);
		}

		//アセットバンドルマニフェストをキャッシュからロードして追加する
		public IEnumerator LoadCacheManifestAsync(string rootUrl, string relativeUrl, Action onComplete, Action onFailed)
		{
			string url = GetCachePath(relativeUrl);
			url = FilePathUtil.AddFileProtocol(url);
			WWWEx wwwEx = new WWWEx(url);
			wwwEx.OnUpdate = OnDownloadingManifest;
			wwwEx.RetryCount = 0;
			wwwEx.TimeOut = 0.1f;
			return wwwEx.LoadAssetBundleByNameAsync<AssetBundleManifest>(
							AssetBundleManifestName,
							false,
							(manifest) =>
							{
								AddAssetBundleManifest(rootUrl, manifest);
								if (onComplete != null) onComplete();
							},
							() =>
							{
								if (onFailed != null) onFailed();
							}
							);
		}


		//キャッシュのパスを取得
		string GetCachePath(string relativeUrl)
		{
			string path = FilePathUtil.Combine(FileIOManager.SdkTemporaryCachePath, cacheDirectoryName, relativeUrl);
			return path;
		}

		//キャッシュすべて削除
		public void DeleteAllCache()
		{
			FileIOManager.DeleteDirectory(FilePathUtil.Combine(FileIOManager.SdkTemporaryCachePath, cacheDirectoryName)+"/");
			WrapperUnityVersion.CleanCache();
		}
	}
}