// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using UtageExtensions;

namespace Utage
{

    /// <summary>
    /// テクスチャに描きこんだものを描画する
    /// </summary>
    [AddComponentMenu("Utage/ADV/Internal/GraphicObject/RenderTextureImage")]
    public class AdvGraphicObjectRenderTextureImage : AdvGraphicObjectUguiBase
	{
		protected override Material Material { get { return RawImage.material; } set { RawImage.material = value; } }
		public AdvRenderTextureSpace RenderTextureSpace { get; private set; }

		//前フレームのテクスチャを使ってクロスフェード処理を行う
		RenderTexture copyTemporary;
		void ReleaseTemporary()
		{
			if (this.copyTemporary != null)
			{
				RenderTexture.ReleaseTemporary(this.copyTemporary);
				this.copyTemporary = null;
			}
		}

		RawImage RawImage { get; set; }


		void OnDestroy()
		{
			ReleaseTemporary();
		}

		//初期化処理
		protected override void AddGraphicComponentOnInit()
		{
		}

		//初期化
		internal void Init(AdvRenderTextureSpace renderTextureSpace)
		{
			this.RenderTextureSpace = renderTextureSpace;
			this.RawImage = this.gameObject.GetComponentCreateIfMissing<RawImage>();
			if (renderTextureSpace.RenderTextureType == AdvRenderTextureMode.Image)
			{
				this.Material = new Material(ShaderManager.DrawByRenderTexture);
			}
			this.RawImage.texture = RenderTextureSpace.RenderTexture;
			this.RawImage.SetNativeSize();
			this.RawImage.rectTransform.localScale = Vector3.one;

			//			this.fadeTimer = this.gameObject.AddComponent<Timer>();
			//			this.fadeTimer.AutoDestroy = false;
		}

		//********描画時にクロスフェードが失敗するであろうかのチェック********//
		internal override bool CheckFailedCrossFade(AdvGraphicInfo graphic)
		{
			return false;
		}

		//********描画時のリソース変更********//
		internal override void ChangeResourceOnDraw(AdvGraphicInfo graphic, float fadeTime)
		{
			//既に描画されている場合は、クロスフェード用のイメージを作成
			bool crossFade = TryCreateCrossFadeImage(fadeTime, graphic);
			if (!crossFade)
			{
				this.gameObject.RemoveComponent<UguiCrossFadeRawImage>();
				ReleaseTemporary();
			}
			//新しくリソースを設定
			RawImage.texture = RenderTextureSpace.RenderTexture;
			RawImage.SetNativeSize();
			if (!crossFade && LastResource == null)
			{
				ParentObject.FadeIn(fadeTime, () => { });
			}
		}

		//ルール画像つきのフェードイン
		public override void RuleFadeIn(AdvEngine engine, AdvTransitionArgs data, Action onComplete)
		{
			UguiTransition transition = this.gameObject.AddComponent<UguiTransition>();
			transition.RuleFadeIn(
				engine.EffectManager.FindRuleTexture(data.TextureName),
				data.Vague,
				RenderTextureSpace.RenderTextureType == AdvRenderTextureMode.Image,
				data.GetSkippedTime(engine),
				()=>
				{
					transition.RemoveComponentMySelf(false);
					if (onComplete != null) onComplete();
				});
		}

		//ルール画像つきのフェードアウト
		public override void RuleFadeOut(AdvEngine engine, AdvTransitionArgs data, Action onComplete)
		{
			UguiTransition transition = this.gameObject.AddComponent<UguiTransition>();
			transition.RuleFadeOut(
				engine.EffectManager.FindRuleTexture(data.TextureName),
				data.Vague,
				RenderTextureSpace.RenderTextureType == AdvRenderTextureMode.Image,
				data.GetSkippedTime(engine),
				() =>
				{
					transition.RemoveComponentMySelf(false);
					RawImage.SetAlpha(0);
					if (onComplete != null) onComplete();
				});
		}

		//クロスフェード用のイメージを作成
		protected bool TryCreateCrossFadeImage(float time, AdvGraphicInfo graphic)
		{
			if (LastResource == null) return false;

			if (RawImage.texture == null) return false;

			//前フレームのテクスチャを使ってクロスフェード処理を行う
			ReleaseTemporary();
			Material material = this.Material;
			this.copyTemporary = RenderTextureSpace.RenderTexture.CreateCopyTemporary(0);
			UguiCrossFadeRawImage crossFade = this.gameObject.AddComponent<UguiCrossFadeRawImage>();
			crossFade.Material = material;
			crossFade.CrossFade(
				copyTemporary,
				time,
				() =>
				{
					//テクスチャを破棄
					ReleaseTemporary();
					crossFade.RemoveComponentMySelf(false);
				});
			return true;
		}
	}
}