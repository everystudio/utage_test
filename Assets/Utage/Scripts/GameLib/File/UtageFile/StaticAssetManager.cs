// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utage
{
	/// <summary>
	/// 動的にロードしないで、常に保持しつづけるアセットの管理
	/// 3Dモデルや、BGM（DLするとストリーム再生できない）など
	/// アセットバンドル化したくないオブジェクトを中心に
	/// </summary>
	[AddComponentMenu("Utage/Lib/File/StaticAssetManager")]
	public class StaticAssetManager : MonoBehaviour
	{
		[SerializeField]
		List<StaticAsset> assets = new List<StaticAsset>();
		List<StaticAsset> Assets { get { return assets; } }

		public AssetFileBase FindAssetFile(AssetFileManager mangager, AssetFileInfo fileInfo, IAssetFileSettingData settingData)
		{
			if (Assets == null) return null;
			string assetName = FilePathUtil.GetFileNameWithoutExtension(fileInfo.FileName);
			StaticAsset asset = Assets.Find((x) => (x.Asset.name == assetName));
			if (asset == null) return null;

			return new StaticAssetFile(asset, mangager, fileInfo, settingData);
		}

		public bool Contains(Object asset)
		{
			foreach( StaticAsset item in Assets )
			{
				if( item.Asset == asset ) return true;
			}
			return false;
		}
	}

	//動的にロードしないアセットの情報
	[System.Serializable]
	public class StaticAsset
	{
		[SerializeField]
		Object asset=null;
		public Object Asset
		{
			get { return asset; }
		}
	}

	//動的にロードしないアセットをロードファイルのように扱うためのクラス
	public class StaticAssetFile : AssetFileBase
	{
		public StaticAsset Asset { get; protected set; }

		public StaticAssetFile(StaticAsset asset, AssetFileManager mangager, AssetFileInfo fileInfo, IAssetFileSettingData settingData)
			: base(mangager, fileInfo, settingData)
		{
			this.Asset = asset;
			this.Text = Asset.Asset as TextAsset;
			this.Texture = Asset.Asset as Texture2D;
			this.Sound = Asset.Asset as AudioClip;
			this.UnityObject = Asset.Asset;
			this.IsLoadEnd = true;
			this.IgnoreUnload = true;

			if (Texture != null)
			{
				FileType = AssetFileType.Texture;
			}
			else if (Sound != null)
			{
				FileType = AssetFileType.Sound;
			}
			else if (UnityObject != null)
			{
				FileType = AssetFileType.UnityObject;
			}
		}

		public override bool CheckCacheOrLocal()
		{
			return true;
		}

		public override IEnumerator LoadAsync(System.Action onComplete, System.Action onFailed)
		{
			onComplete();
			yield break;
		}
		public override void Unload() { }
	}
}
