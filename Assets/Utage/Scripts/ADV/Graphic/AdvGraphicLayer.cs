// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UtageExtensions;

namespace Utage
{

	/// <summary>
	/// グラフィックのレイヤー管理
	/// </summary>
	[AddComponentMenu("Utage/ADV/Internal/GraphicLayer")]
	public class AdvGraphicLayer : MonoBehaviour
	{
		public AdvEngine Engine { get { return Manager.Engine; } }

		public AdvGraphicManager Manager { get; private set; }

		public AdvLayerSettingData SettingData { get; private set; }

		public AdvGraphicObject DefaultObject { get; private set; }
		public Dictionary<string, AdvGraphicObject> CurrentGraphics
		{
			get { return currentGraphics; }
		}
		Dictionary<string, AdvGraphicObject> currentGraphics = new Dictionary<string, AdvGraphicObject>();

		public Camera Camera { get; private set; }
		public LetterBoxCamera LetterBoxCamera { get; private set; }

		public Canvas Canvas { get; private set; }

		//これ以下に描画オブジェクトが置かれる
		Transform rootObjects = null;
		public Vector2 GameScreenSize
		{
			get
			{
				return LetterBoxCamera.CurrentSize;
			}
		}

		//初期化
		public void Init(AdvGraphicManager manager, AdvLayerSettingData settingData)
		{
			this.Manager = manager;
			this.SettingData = settingData;

			//UI用のコード
			this.Canvas = this.GetComponent<Canvas>();
#if UNITY_5_6_OR_NEWER
			this.Canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;
#endif

			if (!string.IsNullOrEmpty(SettingData.LayerMask))
			{
#if UNITY_EDITOR
				if (!LayerMaskEditor.ContainsInLayerNames(SettingData.LayerMask))
				{
					Debug.LogWarning("Please add Layer name [ " + SettingData.LayerMask + " ]");
					this.Canvas.gameObject.layer = 8;
				}
				else
				{ 
					this.Canvas.gameObject.layer = LayerMask.NameToLayer(SettingData.LayerMask);
				}
#else
				this.Canvas.gameObject.layer = LayerMask.NameToLayer(SettingData.LayerMask);
#endif
			}
			this.Canvas.sortingOrder = SettingData.Order;

			//入力受け付ける可能性があるので、イベントカメラとRaycasterを設定
			this.Camera = Engine.CameraManager.FindCameraByLayer(this.Canvas.gameObject.layer);
			if (Camera == null)
			{
				Debug.LogError("Cant find camera");
				this.Camera = Engine.CameraManager.FindCameraByLayer(0);
			}
			this.LetterBoxCamera = Camera.gameObject.GetComponent<LetterBoxCamera> ();
			this.Canvas.worldCamera = Camera;
			GraphicRaycaster raycaster = this.Canvas.gameObject.AddComponent<GraphicRaycaster>();
			raycaster.enabled = false;

			this.rootObjects = this.Canvas.transform;
			ResetCanvasRectTransform();
			//ToDo
			//キャンバスのアニメーションの最中でリセットされると破綻するが・・・
			if (Manager.DebugAutoResetCanvasPosition)
			{
				this.LetterBoxCamera.OnGameScreenSizeChange.AddListener(x => ResetCanvasRectTransform());
			}
		}


		//　キャンバスのRectTransformをリセットして初期状態に
		internal void ResetCanvasRectTransform()
		{
			RectTransform rectTransform = this.Canvas.transform as RectTransform;

			//今のゲーム画面の大きさと、宴のLayerシートの設定データから
			//キャンバスのサイズと位置を取得
			float x, width;
			SettingData.Horizontal.GetBorderdPositionAndSize(GameScreenSize.x, out x, out width);
			float y, height;
			SettingData.Vertical.GetBorderdPositionAndSize(GameScreenSize.y, out y, out height);

			//テクスチャ書き込みが無効な場合、位置をそのまま設定
			rectTransform.localPosition = new Vector3(x, y, SettingData.Z) / Manager.PixelsToUnits;
			//サイズ設定
			rectTransform.SetSize(width, height);
			//スケーリング値の設定
			rectTransform.localScale = SettingData.Scale / Manager.PixelsToUnits;
		}


		/*
				//RectTransformにレイヤー情報を設定
				internal void SetToRectTransform(RectTransform rectTransform, float defaultWitdh, float defaultHeight, float pixelsToUnits)
				{
					float x, width;
					Horizontal.GetBorderdPositionAndSize(defaultWitdh, out x, out width);
					float y, height;
					Vertical.GetBorderdPositionAndSize(defaultHeight, out y, out height);

					rectTransform.localPosition = new Vector3(x, y, Z) / pixelsToUnits;
					rectTransform.SetSize(width, height);
					rectTransform.localScale = Scale / pixelsToUnits;
				}*/

		internal void Remove(AdvGraphicObject obj)
		{
			if (currentGraphics.ContainsValue(obj))
			{
				currentGraphics.Remove(obj.name);
			}
			if (DefaultObject == obj)
			{
				DefaultObject = null;
			}
		}

		//オブジェクトを描画する
		internal AdvGraphicObject Draw(string name, AdvGraphicOperaitonArg arg )
		{
			AdvGraphicObject obj = GetObjectCreateIfMissing(name, arg.Graphic);
			obj.Loader.LoadGraphic(arg.Graphic, () =>
			{
				obj.Draw(arg, arg.GetSkippedFadeTime(Engine));
			});
			return obj;
		}

		//デフォルトオブジェクトとして描画する
		internal AdvGraphicObject DrawToDefault(string name, AdvGraphicOperaitonArg arg)
		{
			if (CheckChangeDafaultObject(name, arg))
			{
				//フェードアウトする
				if (SettingData.Type == AdvLayerSettingData.LayerType.Bg)
				{
					DelayOut(DefaultObject.name, arg.GetSkippedFadeTime(Engine));
				}
				else
				{
					FadeOut(DefaultObject.name, arg.GetSkippedFadeTime(Engine));
				}
			}
			DefaultObject = Draw(name,arg);
			return DefaultObject;
		}

		bool CheckChangeDafaultObject(string name, AdvGraphicOperaitonArg arg)
		{
			if (DefaultObject == null) return false;
			//デフォルトオブジェクトの名前が違うなら、そのオブジェクトは変更
			if (DefaultObject.name != name) return true;

			if (DefaultObject.LastResource == null) return false;
			if (arg.Graphic.FileType != DefaultObject.LastResource.FileType) return true;
			return DefaultObject.TargetObject.CheckFailedCrossFade(arg.Graphic);
		}

		//指定の名前のオブジェクトを取得、なければ作成
		internal AdvGraphicObject GetObjectCreateIfMissing(string name, AdvGraphicInfo grapic)
		{
			if (grapic == null) 
			{
				Debug.LogError ( name + " grapic is null");
				return null;
			}
			AdvGraphicObject obj;
			if (!currentGraphics.TryGetValue(name, out obj))
			{
				//まだ作成されてないから作る
				obj = CreateObject(name, grapic);
			}
			return obj;
		}

		//描画オブジェクトを作成
		AdvGraphicObject CreateObject(string name, AdvGraphicInfo grapic)
		{
			AdvGraphicObject obj;
			//IAdvGraphicObjectがAddComponentされたプレハブをリソースに持つかチェック
			GameObject prefab;
			if (grapic.TryGetAdvGraphicObjectPrefab(out prefab))
			{
				//プレハブからリソースオブジェクトを作成して返す
				GameObject go = GameObject.Instantiate(prefab);
				go.name = name;
				obj = go.GetComponent<AdvGraphicObject>();
				rootObjects.AddChild(obj.gameObject);
			}
			else
			{
				obj = rootObjects.AddChildGameObjectComponent<AdvGraphicObject>(name);
			}
			obj.Init(this, grapic);

			//最初の描画時は位置をリセットする
			if (currentGraphics.Count == 0)
			{
				this.ResetCanvasRectTransform();
			}

			currentGraphics.Add(obj.name, obj);
			return obj;
		}

		//フェードアウト
		internal void FadeOut(string name, float fadeTime)
		{
			AdvGraphicObject obj;
			if (currentGraphics.TryGetValue(name, out obj))
			{
				obj.FadeOut(fadeTime);
				Remove(obj);
			}
		}

		//一定時間後にフェードなしで消える
		internal void DelayOut(string name, float delay)
		{
			AdvGraphicObject obj;
			if (currentGraphics.TryGetValue(name, out obj))
			{
				Remove(obj);
				StartCoroutine(CoDelayOut(obj,delay));
			}
		}

		IEnumerator CoDelayOut(AdvGraphicObject obj, float delay)
		{
			yield return new WaitForSeconds(delay);
			if(obj!=null) obj.Clear();
		}


		internal void FadeOutAll(float fadeTime)
		{
			List<AdvGraphicObject> values = new List<AdvGraphicObject>(currentGraphics.Values);
			foreach (var obj in values)
			{
				obj.FadeOut(fadeTime);
			}
			currentGraphics.Clear();
			DefaultObject = null;
		}

		//指定名のパーティクルを非表示にする
		internal void FadeOutParticle(string name)
		{
			AdvGraphicObject obj;
			if (currentGraphics.TryGetValue(name, out obj))
			{
				if (obj.TargetObject is AdvGraphicObjectParticle)
				{
					obj.FadeOut(0);
					Remove(obj);
				}
			}
		}

		//パーティクルを全て非表示にする
		internal void FadeOutAllParticle()
		{
			List<AdvGraphicObject> values = new List<AdvGraphicObject>(currentGraphics.Values);
			foreach (var obj in values)
			{
				if (obj.TargetObject is AdvGraphicObjectParticle)
				{
					obj.FadeOut(0);
					Remove(obj);
				}
			}
		}
/*
		//フェードイン
		public void FadeIn(float time, Action onComplete)
		{
			if (!IsRenderTexture)
			{
				Debug.Log(this.gameObject.name + " is not support RuleFadeIn. Please set [RendetTexutre] in Layer sheet.");
				return;
			}
			RenderTextureImage.FadeIn(time, onComplete);
		}

		//フェードイン
		public void FadeOut(float time, Action onComplete)
		{
			if (!IsRenderTexture)
			{
				Debug.Log(this.gameObject.name + " is not support RuleFadeIn. Please set [RendetTexutre] in Layer sheet.");
				return;
			}
			RenderTextureImage.FadeOut(time, onComplete);
		}

		//ルール画像つきのフェードイン
		public void RuleFadeIn(AdvEngine engine, AdvTransitionArgs data, Action onComplete)
		{
			if (!IsRenderTexture)
			{
				Debug.Log(this.gameObject.name + " is not support RuleFadeIn. Please set [RendetTexutre] in Layer sheet.");
				return;
			}

			RenderTextureImage.RuleFadeIn(engine, data, onComplete);
		}

		//ルール画像つきのフェードアウト
		public void RuleFadeOut(AdvEngine engine, AdvTransitionArgs data, Action onComplete)
		{
			if (!IsRenderTexture)
			{
				Debug.Log(this.gameObject.name + " is not support RuleFadeOut. Please set [RendetTexutre] in Layer sheet.");
				return;
			}

			RenderTextureImage.RuleFadeOut(engine, data, 
				()=>
				{
					if (onComplete != null) onComplete();
					this.Clear();
				});
		}

		//前フレームのテクスチャを使ってクロスフェード処理を行う
		internal void StartCrossFadeImage(float fadeTime)
		{
			if (!IsRenderTexture)
			{
				Debug.Log(this.gameObject.name + " is not support CrossFadeImage. Please set [RendetTexutre] in Layer sheet.");
				return;
			}

			RenderTextureImage.StartCrossFadeImage(fadeTime);
		}
*/

		//クリア処理
		internal void Clear()
		{
			List<AdvGraphicObject> values = new List<AdvGraphicObject>(currentGraphics.Values);
			foreach (var obj in values)
			{
				obj.Clear();
			}
			currentGraphics.Clear();
			DefaultObject = null;
		}

		//デフォルトグラフィックオブジェクトの名前が指定名と同じかチェック
		internal bool IsEqualDefaultGraphicName(string name)
		{
			if (DefaultObject!=null)
			{
				return DefaultObject.name == name;
			}
			return false;
		}

		//指定名のオブジェクトがあるか
		internal bool Contains(string name)
		{
			return currentGraphics.ContainsKey(name);
		}

		//指定名のオブジェクトがあれば返す
		internal AdvGraphicObject Find(string name)
		{
			AdvGraphicObject obj;
			if(currentGraphics.TryGetValue(name,out obj))
			{
				return obj;
			}
			return null;
		}


		internal void AddAllGraphics(List<AdvGraphicObject> graphics)
		{
			graphics.AddRange(currentGraphics.Values);
		}

		//ロード中かチェック
		internal bool IsLoading
		{
			get
			{
				foreach (var keyValue in currentGraphics)
				{
					if (keyValue.Value.Loader.IsLoading) return true;
				}
				return false;
			}
		}

		const int Version = 0;
		//セーブデータ用のバイナリ書き込み
		public void Write(BinaryWriter writer)
		{
			writer.Write(Version);
			writer.WriteLocalTransform(this.transform);

			int count = 0;
			foreach (var keyValue in currentGraphics)
			{
				if (keyValue.Value.LastResource.DataType == AdvGraphicInfo.TypeCapture)
				{
					Debug.LogError("Caputure image not support on save");
					continue;
				}
				++count;
			}

			writer.Write(count);
			foreach (var keyValue in currentGraphics)
			{
				if (keyValue.Value.LastResource.DataType == AdvGraphicInfo.TypeCapture)
				{
					continue;
				}

				writer.Write(keyValue.Key);
				writer.WriteBuffer(keyValue.Value.LastResource.OnWrite);
				writer.WriteBuffer(keyValue.Value.Write);
			}
			writer.Write(DefaultObject == null ? "" : DefaultObject.name);
		}

		//セーブデータ用のバイナリ読み込み
		public void Read(BinaryReader reader)
		{
			int version = reader.ReadInt32();
			if (version < 0 || version > Version)
			{
				Debug.LogError(LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.UnknownVersion, version));
				return;
			}

			reader.ReadLocalTransform(this.transform);

			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = reader.ReadString();
				AdvGraphicInfo graphic = null;
				reader.ReadBuffer(x => graphic = AdvGraphicInfo.ReadGraphicInfo(Engine, x));
				byte[] buffer = reader.ReadBuffer();
				AdvGraphicObject obj = CreateObject(key, graphic);
				obj.Read(buffer,graphic);
			}
			string defaulObjectName = reader.ReadString();
			DefaultObject = Find(defaulObjectName);
		}
	}
}
