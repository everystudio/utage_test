// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UtageExtensions;

namespace Utage
{

	/// <summary>
	/// フェード切り替え機能つきのスプライト表示
	/// </summary>
	[AddComponentMenu("Utage/ADV/Internal/GraphicObject/RawImage")]
	public class AdvGraphicObjectRawImage : AdvGraphicObjectUguiBase
	{
		protected override Material Material { get { return RawImage.material; } set { RawImage.material = value; } }
		RawImage RawImage { get; set; }

		//クロスフェード用のファイル参照
		AssetFileReference crossFadeReference;
		void ReleaseCrossFadeReference()
		{
			if( crossFadeReference != null)
			{
				DestroyImmediate(crossFadeReference);
				crossFadeReference = null;
			}
		}

		//初期化処理
		protected override void AddGraphicComponentOnInit()
		{
			RawImage = this.GetComponentCreateIfMissing<RawImage>();
		}

		//********描画時にクロスフェードが失敗するであろうかのチェック********//
		internal override bool CheckFailedCrossFade(AdvGraphicInfo graphic)
		{
			return !EnableCrossFade(graphic);
		}

		//********描画時のリソース変更********//
		internal override void ChangeResourceOnDraw(AdvGraphicInfo graphic, float fadeTime)
		{
			Material = graphic.RenderTextureSetting.GetRenderMaterialIfEnable(Material);

			//既に描画されている場合は、クロスフェード用のイメージを作成
			bool crossFade = TryCreateCrossFadeImage(fadeTime, graphic);
			if (!crossFade)
			{
				ReleaseCrossFadeReference();
				this.gameObject.RemoveComponent<UguiCrossFadeRawImage>();
			}
			//新しくリソースを設定
			RawImage.texture = graphic.File.Texture;
			RawImage.SetNativeSize();
			if (!crossFade)
			{
				ParentObject.FadeIn(fadeTime, () => { });
			}
		}

		//クロスフェード用のイメージを作成
		protected bool TryCreateCrossFadeImage(float fadeTime, AdvGraphicInfo graphic)
		{
			if (LastResource == null) return false;

			if (EnableCrossFade(graphic))
			{
				StartCrossFadeImage(fadeTime);
				return true;
			}
			else
			{
				return false;
			}
		}

		//今の表示状態と比較して、クロスフェード可能か
		protected bool EnableCrossFade(AdvGraphicInfo graphic)
		{
			Texture texture = graphic.File.Texture as Texture;
			if (texture == null) return false;
			if (RawImage.texture == null) return false;
			return RawImage.rectTransform.pivot == graphic.Pivot
				&& RawImage.texture.width == texture.width
				&& RawImage.texture.height == texture.height;
		}

		//前フレームのテクスチャを使ってクロスフェード処理を行う
		internal void StartCrossFadeImage(float time)
		{
			Texture lastTexture = LastResource.File.Texture;
			ReleaseCrossFadeReference();
			crossFadeReference = this.gameObject.AddComponent<AssetFileReference>();
			crossFadeReference.Init(LastResource.File);
			UguiCrossFadeRawImage crossFade = this.gameObject.GetComponentCreateIfMissing<UguiCrossFadeRawImage>();
			crossFade.CrossFade(
				lastTexture,
				time,
				() =>
				{
					ReleaseCrossFadeReference();
					crossFade.RemoveComponentMySelf();
				}
			);
		}

		//カメラのキャプチャ画像を、Imageとして設定
		internal void CaptureCamera(Camera camera)
		{
			RawImage.enabled = false;

			//カメラのキャプチャコンポーネントを有効に
			CaptureCamera captureCamera = camera.GetComponentCreateIfMissing<CaptureCamera>();
			captureCamera.enabled = true;
			captureCamera.OnCaptured.AddListener(OnCaptured);
		}

		void OnCaptured(CaptureCamera captureCamera)
		{
			RawImage.enabled = true;
			RawImage.texture = captureCamera.CaptureImage;
			LetterBoxCamera letterBoxCamera = captureCamera.GetComponent<LetterBoxCamera>();
			if (letterBoxCamera != null)
			{
				RawImage.rectTransform.SetSize(letterBoxCamera.CurrentSize);
				//ズームが1ではなく、このイメージを描画するカメラのキャプチャ画像かどうか
				if (letterBoxCamera.Zoom2D != 1)
				{
					int layerMask = 1 << this.gameObject.layer;
					if ((letterBoxCamera.CachedCamera.cullingMask & layerMask) != 0)
					{
						Vector2 pivot = letterBoxCamera.Zoom2DCenter;
						pivot.x /= letterBoxCamera.CurrentSize.x;
						pivot.y /= letterBoxCamera.CurrentSize.y;
						pivot = -pivot + Vector2.one * 0.5f;
						RawImage.rectTransform.pivot = pivot;
						RawImage.rectTransform.localScale = Vector3.one / letterBoxCamera.Zoom2D;
					}
				}
			}
			else
			{
				RawImage.rectTransform.SetSize(Screen.width,Screen.height);
			}

			//カメラのキャプチャコンポーネントを無効にする
			captureCamera.OnCaptured.RemoveListener(OnCaptured);
			captureCamera.enabled = false;
		}
	}
}
