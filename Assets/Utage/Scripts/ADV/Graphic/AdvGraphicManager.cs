// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UtageExtensions;

namespace Utage
{

	/// <summary>
	/// グラフィックの管理
	/// </summary>
	[AddComponentMenu("Utage/ADV/GraphicManager")]
	public class AdvGraphicManager : MonoBehaviour, IBinaryIO
	{
		/// <summary>
		/// スプライトを作成する際の、座標1.0単位辺りのピクセル数
		/// </summary>
		public float PixelsToUnits { get { return pixelsToUnits; } }

		[SerializeField]
		float pixelsToUnits = 100;

		/// <summary>
		/// Z座標1.0単位辺りのSortingOrderの数
		/// </summary>
		public float SortOderToZUnits { get { return sortOderToZUnits; } }

		[SerializeField]
		float sortOderToZUnits = 100;

		public string BgSpriteName
		{
			get { return bgSpriteName; }
		}
		[SerializeField]
		string bgSpriteName = "BG";

		//表示済みのキャラクターのレイヤー変更時に、ローカルのTransform値をリセットするか引き継ぐか
		public bool ResetCharacterTransformOnChangeLayer
		{
			get { return resetCharacterTransformOnChangeLayer; }
		}
		[SerializeField]
		bool resetCharacterTransformOnChangeLayer = true;

		public bool DebugAutoResetCanvasPosition
		{
			get
			{
#if UNITY_EDITOR
				return debugAutoResetCanvasPosition;
#else
				return false;				 

#endif
			}
		}
#if UNITY_EDITOR
		[SerializeField]
		bool debugAutoResetCanvasPosition = false;
#endif


		/// <summary>
		/// レンダーテクスチャー設定
		/// </summary>
		public AdvGraphicRenderTextureManager RenderTextureManager
		{
			get
			{
				if (renderTextureManager == null)
				{
					renderTextureManager = this.transform.parent.AddChildGameObjectComponent<AdvGraphicRenderTextureManager>("GraphicRenderTextureManager");
				}
				return renderTextureManager;
			}
		}

		[SerializeField]
		AdvGraphicRenderTextureManager renderTextureManager;

		/// <summary>
		/// ビデオ制御
		/// </summary>
		public AdvVideoManager VideoManager
		{
			get
			{
				if (videoManager == null)
				{
					videoManager = this.transform.parent.AddChildGameObjectComponent<AdvVideoManager>("VideoManager");
				}
				return videoManager;
			}
		}

		[SerializeField]
		AdvVideoManager videoManager;

		/// <summary>
		/// イベントモード（キャラクター立ち絵非表示）
		/// </summary>
		public bool IsEventMode { get { return this.isEventMode; } set { isEventMode = value; } }
		bool isEventMode;

		/// <summary>
		/// キャラクター管理
		/// </summary>
		public AdvGraphicGroup CharacterManager { get { return this.Groups[AdvLayerSettingData.LayerType.Character]; } }

		/// <summary>
		/// スプライト管理
		/// </summary>
		public AdvGraphicGroup SpriteManager { get { return this.Groups[AdvLayerSettingData.LayerType.Sprite]; } }

		/// <summary>
		/// スプライト管理
		/// </summary>
		public AdvGraphicGroup BgManager { get { return this.Groups[AdvLayerSettingData.LayerType.Bg]; } }

		/// <summary>
		/// 全てのグループ
		/// </summary>
		Dictionary<AdvLayerSettingData.LayerType, AdvGraphicGroup> Groups = new Dictionary<AdvLayerSettingData.LayerType, AdvGraphicGroup>();

		internal AdvEngine Engine { get { return engine; } }
		AdvEngine engine;

		/// <summary>
		/// 起動時初期化
		/// </summary>
		/// <param name="setting">レイヤー設定データ</param>
		public void BootInit(AdvEngine engine, AdvLayerSetting setting)
		{
			this.engine = engine;
			Groups.Clear();
			foreach( AdvLayerSettingData.LayerType type in Enum.GetValues(typeof(AdvLayerSettingData.LayerType) ))
			{
				AdvGraphicGroup group = new AdvGraphicGroup(type, setting, this);
				Groups.Add(type,group);
			}
		}

		/// <summary>
		/// 章追加時などリメイク
		/// </summary>
		public void Remake(AdvLayerSetting setting)
		{
			foreach (AdvGraphicGroup group in Groups.Values)
			{
				group.DestroyAll();
			}
			Groups.Clear();
			foreach (AdvLayerSettingData.LayerType type in Enum.GetValues(typeof(AdvLayerSettingData.LayerType)))
			{
				AdvGraphicGroup group = new AdvGraphicGroup(type, setting, this);
				Groups.Add(type, group);
			}
		}

		/// <summary>
		/// 全てクリア
		/// </summary>
		internal void Clear()
		{
			foreach (AdvGraphicGroup group in Groups.Values)
			{
				group.Clear();
			}
		}

		/// <summary>
		/// 指定のキーのレイヤーを探す
		/// </summary>
		internal AdvGraphicLayer FindLayer(string layerName)
		{
			foreach (var keyValue in Groups)
			{
				AdvGraphicLayer layer = keyValue.Value.FindLayer(layerName);
				if (layer != null) return layer;
			}
			return null;
		}

		/// <summary>
		/// 指定のオブジェクト名のレイヤーを探す
		/// </summary>
		internal AdvGraphicLayer FindLayerByObjectName(string name)
		{
			foreach (var keyValue in Groups)
			{
				AdvGraphicLayer layer = keyValue.Value.FindLayerFromObjectName(name);
				if (layer != null) return layer;
			}
			return null;
		}

		/// <summary>
		/// 指定の名前のグラフィックオブジェクトを検索
		/// </summary>
		internal AdvGraphicObject FindObject(string name)
		{
			foreach (var keyValue in Groups)
			{
				AdvGraphicObject obj = keyValue.Value.FindObject(name);
				if (obj != null) return obj;
			}
			return null;
		}

		/// <summary>
		/// 指定の名前のレイヤーかグラフィックオブジェクトを検索
		/// </summary>
		internal GameObject FindObjectOrLayer(string name)
		{
			AdvGraphicObject obj = FindObject(name);
			if (obj != null)
			{
				return obj.gameObject;
			}
			AdvGraphicLayer layer = FindLayer(name);
			if (layer != null)
			{
				return layer.gameObject;
			}
			return null;
		}
		//全てのグラフィックオブジェクトを取得
		internal List<AdvGraphicObject> AllGraphics()
		{
			List<AdvGraphicObject> allGraphics = new List<AdvGraphicObject>();
			foreach (var keyValue in Groups)
			{
				keyValue.Value.AddAllGraphics(allGraphics);
			}
			return allGraphics;
		}

		//ロード中かチェック
		internal bool IsLoading
		{
			get
			{
				foreach (var keyValue in Groups)
				{
					if (keyValue.Value.IsLoading) return true;
				}
				return false;
			}
		}

		//表示する
		internal void DrawObject(string layerName, string label, AdvGraphicOperaitonArg graphicOperaitonArg)
		{
			FindLayer(layerName).Draw(label, graphicOperaitonArg);
		}

		//指定名のパーティクルを非表示にする
		internal void FadeOutParticle(string name)
		{
			foreach (var keyValue in Groups)
			{
				keyValue.Value.FadeOutParticle(name);
			}
		}

		//パーティクルを全て非表示にする
		internal void FadeOutAllParticle()
		{
			foreach (var keyValue in Groups)
			{
				keyValue.Value.FadeOutAllParticle();
			}
		}



		//指定のカメラのキャプチャ画像を撮って、それを表示するオブジェクトを作成
		internal void CreateCaptureImageObject(string name, string cameraName, string layerName)
		{
			AdvGraphicLayer layer = FindLayer(layerName);
			if (layer == null)
			{
				Debug.LogError(layerName + " is not layer name");
				return;
			}

			CameraRoot cameraRoot = Engine.CameraManager.FindCameraRoot(cameraName);
			if (cameraRoot==null)
			{
				Debug.LogError(cameraName + " is not camera name");
				return;
			}

			AdvGraphicInfo grapic = new AdvGraphicInfo(AdvGraphicInfo.TypeCapture, name, AdvGraphicInfo.FileType2D);
			AdvGraphicObject obj = layer.GetObjectCreateIfMissing(name, grapic);
			obj.InitCaptureImage(grapic, cameraRoot.LetterBoxCamera.CachedCamera);
		}

		/// <summary>
		/// クリックイベントを削除
		/// </summary>
		internal void RemoveClickEvent(string name)
		{
			AdvGraphicObject obj = FindObject(name);
			if (obj == null) return;

			IAdvClickEvent clickEvent = obj.gameObject.GetComponentInChildren<IAdvClickEvent>();
			if (clickEvent == null) return;

			clickEvent.RemoveClickEvent();
		}

		/// <summary>
		/// 指定の名前のスプライトにクリックイベントを設定
		/// </summary>
		/// <param name="name"></param>
		internal void AddClickEvent(string name, bool isPolygon, StringGridRow row, UnityAction<BaseEventData> action)
		{
			AdvGraphicObject obj = FindObject(name);
			if (obj == null)
			{
				Debug.LogError("can't find Graphic object" + name);
				return;
			}

			IAdvClickEvent clickEvent = obj.gameObject.GetComponentInChildren<IAdvClickEvent>();
			if (clickEvent == null)
			{
				Debug.LogError("can't find IAdvClickEvent Interface in " + name);
				return;
			}

			clickEvent.AddClickEvent(isPolygon, row, action);
		}

		public string SaveKey { get { return "AdvGraphicManager"; } }

		const int Version = 0;
		//セーブデータ用のバイナリ書き込み
		public void OnWrite(BinaryWriter writer)
		{
			writer.Write(Version);
			writer.Write(isEventMode);
			writer.Write(Groups.Count);
			foreach (var keyValue in Groups)
			{
				writer.Write((int)keyValue.Key);
				writer.WriteBuffer(keyValue.Value.Write);
			}
		}

		//セーブデータ用のバイナリ読み込み
		public void OnRead(BinaryReader reader)
		{
			int version = reader.ReadInt32();
			if (version < 0 || version > Version)
			{
				Debug.LogError(LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.UnknownVersion, version));
				return;
			}

			this.isEventMode = reader.ReadBoolean();
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				AdvLayerSettingData.LayerType type = (AdvLayerSettingData.LayerType)reader.ReadInt32();
				reader.ReadBuffer(Groups[type].Read);
			}
		}
	}
}