
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
	/// グラフィックオブジェクトのデータ
	/// </summary>
	[AddComponentMenu("Utage/ADV/Internal/GraphicObject")]
	[RequireComponent(typeof(RectTransform))]
	public class AdvGraphicObject : MonoBehaviour, IAdvFade
	{
		//ローダー
		public AdvGraphicLoader Loader { get { return this.GetComponentCacheCreateIfMissing<AdvGraphicLoader>(ref loader); } }
		AdvGraphicLoader loader;

		public AdvGraphicLayer Layer { get { return layer; } }
		protected AdvGraphicLayer layer;

		public AdvEngine Engine { get { return Layer.Manager.Engine; } }
		public AdvGraphicInfo LastResource { get; private set; }

		public float PixelsToUnits { get { return Layer.Manager.PixelsToUnits; } }

		//テクスチャ描き込みが有効か
		public bool EnableRenderTexture { get { return LastResource !=null && LastResource.RenderTextureSetting.EnableRenderTexture; } }

		//ターゲットとなるオブジェクト（グラッフィックの本体）
		public AdvGraphicBase TargetObject { get; private set; }

		//実際に描画するオブジェクト（RenderTexture使用時は、RenderTextureImageのほう）
		public AdvGraphicBase RenderObject { get; private set; }

		//RenderTexture使用時の描画空間
		public AdvRenderTextureSpace RenderTextureSpace { get; private set; }

		//フェード用のタイマー
		Timer FadeTimer { get; set; }

		public AdvEffectColor EffectColor { get { return this.GetComponentCacheCreateIfMissing<AdvEffectColor>(ref effectColor); } }
		AdvEffectColor effectColor;

		//実際に描画するメインオブジェクト
		public RectTransform rectTransform { get; private set; }

		//********初期化********//
		public virtual void Init(AdvGraphicLayer layer, AdvGraphicInfo graphic)
		{
			this.layer = layer;
			this.rectTransform = this.transform as RectTransform;
			this.rectTransform.SetStretch();

			if (graphic.RenderTextureSetting.EnableRenderTexture)
			{
				InitRenderTextureImage(graphic);
			}
			else
			{
				GameObject child = this.transform.AddChildGameObject(graphic.Key);
				this.TargetObject = this.RenderObject = child.AddComponent(graphic.GetComponentType()) as AdvGraphicBase;
				this.TargetObject.Init(this);
			}

			//リップシンクのキャラクターラベルを設定
			LipSynchBase lipSync = TargetObject.GetComponentInChildren<LipSynchBase>();
			if (lipSync != null)
			{
				lipSync.CharacterLabel = this.gameObject.name;
				lipSync.OnCheckTextLipSync.AddListener(
					(x) =>
					{
						x.EnableTextLipSync = (x.CharacterLabel == Engine.Page.CharacterLabel && Engine.Page.IsSendChar);
					});
			}

			this.FadeTimer = this.gameObject.AddComponent<Timer>();
			this.effectColor = this.GetComponentCreateIfMissing<AdvEffectColor>();
			this.effectColor.OnValueChanged.AddListener(RenderObject.OnEffectColorsChange);
		}

		void InitRenderTextureImage(AdvGraphicInfo graphic)
		{
			AdvGraphicManager graphicManager = this.Layer.Manager;
			this.RenderTextureSpace = graphicManager.RenderTextureManager.CreateSpace();
			this.RenderTextureSpace.Init(graphic, graphicManager.PixelsToUnits);

			GameObject child = this.transform.AddChildGameObject(graphic.Key);
			AdvGraphicObjectRenderTextureImage renderTextureImage = child.AddComponent<AdvGraphicObjectRenderTextureImage>();
			this.RenderObject = renderTextureImage;
			renderTextureImage.Init(RenderTextureSpace);
			this.RenderObject.Init(this);

			this.TargetObject = RenderTextureSpace.RenderRoot.transform.AddChildGameObject(graphic.Key).AddComponent(graphic.GetComponentType()) as AdvGraphicBase;
			this.TargetObject.Init(this);
		}

		//********描画開始********//
		public virtual void Draw(AdvGraphicOperaitonArg arg, float fadeTime)
		{
			DrawSub(arg.Graphic, fadeTime);
		}
		void DrawSub(AdvGraphicInfo graphic, float fadeTime)
		{
			TargetObject.name = graphic.File.FileName;
/*			if (LastResource != graphic)
			{
				TargetObject.ChangeResourceOnDraw(graphic, fadeTime);
			}*/
			TargetObject.ChangeResourceOnDraw(graphic, fadeTime);
			if (RenderObject != TargetObject)
			{
				//テクスチャ書き込みをしている
				RenderObject.ChangeResourceOnDraw(graphic, fadeTime);
				if (graphic.IsUguiComponentType)
				{
					//UGUI系は、描画するImageにスケール値を適用
					RenderObject.Scale(graphic);
				}
			}
			else
			{
				TargetObject.Scale(graphic);
			}
			RenderObject.Alignment(Layer.SettingData.Alignment, graphic);
			RenderObject.Flip(Layer.SettingData.FlipX, Layer.SettingData.FlipY);
			this.LastResource = graphic;
		}


		//コマンドによる位置設定を適用
		internal virtual void SetCommandPostion(AdvCommand command)
		{
			//位置情報を反映
			bool parsed = false;
			Vector3 pos = transform.localPosition;
			float x;
			if (command.TryParseCell<float>(AdvColumnName.Arg4, out x))
			{
				pos.x = x;
				parsed = true;
			}
			float y;
			if (command.TryParseCell<float>(AdvColumnName.Arg5, out y))
			{
				pos.y = y;
				parsed = true;
			}

			if (parsed)
			{
				transform.localPosition = pos;
			}
		}

		//********描画終了********//
		public virtual bool TryFadeIn(float time)
		{
			if (TargetObject != null )
			{
				FadeIn(time,null);
				return true;
			}
			else
			{
				return false;
			}
		}

		//文字列指定でのパターンチェンジ（キーフレームアニメーションに使う）
		public virtual void ChangePattern(string pattern)
		{
			if (TargetObject != null)
			{
				TargetObject.ChangePattern(pattern);
			}
		}

		//フェードイン処理
		public void FadeIn(float fadeTime, Action onComplete)
		{
			float begin = 0;
			float end = 1;
			FadeTimer.StartTimer(
				fadeTime,
				x =>
				{
					this.EffectColor.FadeAlpha = x.GetCurve(begin, end);
				},
				x =>
				{
					if (onComplete != null) onComplete();
				}
				);
		}

		public virtual void FadeOut(float time)
		{
			FadeOut(time, Clear);
		}

		//フェードアウト処理
		public void FadeOut(float time, Action onComplete)
		{
			if (TargetObject == null)
			{
				if (onComplete != null) onComplete();
				return;
			}

			float begin = this.EffectColor.FadeAlpha;
			float end = 0;
			FadeTimer.StartTimer(
				time,
				x =>
				{
					this.EffectColor.FadeAlpha = x.GetCurve(begin, end);
				},
				x =>
				{
					if (onComplete != null) onComplete();
				}
				);
		}

		//ルール画像つきのフェードイン
		public void RuleFadeIn(AdvEngine engine, AdvTransitionArgs data, Action onComplete)
		{
			if ( TargetObject == null)
			{
				if (onComplete != null) onComplete();
				return;
			}

			RenderObject.RuleFadeIn(engine, data, onComplete);
		}

		//ルール画像つきのフェードアウト
		public void RuleFadeOut(AdvEngine engine, AdvTransitionArgs data, Action onComplete)
		{
			if (TargetObject == null)
			{
				if (onComplete != null) onComplete();
				Clear();
				return;
			}

			RenderObject.RuleFadeOut(
				engine,
				data,
				() =>
				{
					if (onComplete != null) onComplete();
					Clear();
				});
		}


		//********クリア********//
		public virtual void Clear()
		{
			RemoveFromLayer();
			//パーティクルのDestory対策
			this.gameObject.SetActive (false);
			GameObject.Destroy(gameObject);
		}

		protected virtual void OnDestroy()
		{
			RemoveFromLayer();
			if (RenderTextureSpace)
			{
				GameObject.Destroy(RenderTextureSpace.gameObject);
			}
		}
		public virtual void RemoveFromLayer()
		{
			if (this.Layer)
			{
				this.Layer.Remove(this);
			}
		}


		const int Version = 1;
		const int Version0 = 0;
		//セーブデータ用のバイナリ書き込み
		public void Write(BinaryWriter writer)
		{
			writer.Write(Version);
			writer.WriteLocalTransform(this.transform);
			writer.WriteBuffer(this.EffectColor.Write);
			writer.WriteBuffer((x)=>AdvITweenPlayer.WriteSaveData (x,this.gameObject));
			writer.WriteBuffer((x) => AdvAnimationPlayer.WriteSaveData(x, this.gameObject));
			writer.WriteBuffer((x) => this.TargetObject.Write(x));
		}

		//セーブデータ用のバイナリ読み込み
		public void Read(byte[] buffer, AdvGraphicInfo graphic)
		{
			this.TargetObject.gameObject.SetActive(false);
			Loader.LoadGraphic(
				graphic,
				() =>
				{
					this.TargetObject.gameObject.SetActive(true);
					SetGraphicOnSaveDataRead(graphic);
					BinaryUtil.BinaryRead(buffer, Read);
				}
			);
		}
		//セーブデータ用のバイナリ読み込み
		void Read(BinaryReader reader)
		{
			int version = reader.ReadInt32();
			if (version < 0 || version > Version)
			{
				Debug.LogError(LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.UnknownVersion, version));
				return;
			}
			reader.ReadLocalTransform(this.transform);
			reader.ReadBuffer(this.EffectColor.Read);
			reader.ReadBuffer(
				(x) => 
				{
					AdvITweenPlayer.ReadSaveData(x,this.gameObject,true, this.PixelsToUnits);
				});
			reader.ReadBuffer(
				(x) =>
				{
					AdvAnimationPlayer.ReadSaveData(x, this.gameObject, Engine);
				});

			if (version <= Version0) return;

			reader.ReadBuffer(
				(x) =>
				{
					this.TargetObject.Read(x);
				});
		}


		//キャプチャーイメージとして初期化
		internal void InitCaptureImage(AdvGraphicInfo grapic, Camera cachedCamera)
		{
			this.LastResource = grapic;
			AdvGraphicObjectRawImage captueImage = this.gameObject.GetComponentInChildren<AdvGraphicObjectRawImage>();
			captueImage.CaptureCamera(cachedCamera);
		}

		void SetGraphicOnSaveDataRead(AdvGraphicInfo graphic)
		{
			this.DrawSub(graphic, 0);
		}
	}
}
