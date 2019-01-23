// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System;
using System.IO;
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
	[AddComponentMenu("Utage/ADV/Internal/GraphicObject/Dicing")]
	public class AdvGraphicObjectDicing : AdvGraphicObjectUguiBase
	{
		//ダイシング描画コンポーネント
		protected DicingImage Dicing { get; set; }

		//目パチ
		protected EyeBlinkDicing EyeBlink { get; set; }
		//口パク
		protected LipSynchDicing LipSynch { get; set; }
		//アニメーション
		protected AdvAnimationPlayer Animation { get; set; }

		//クロスフェード用のファイル参照
		AssetFileReference crossFadeReference;
		void ReleaseCrossFadeReference()
		{
			if (crossFadeReference != null)
			{
				crossFadeReference.RemoveComponentMySelf();
				crossFadeReference = null;
			}
		}

		//初期化処理
		protected override void AddGraphicComponentOnInit()
		{
			Dicing = this.gameObject.AddComponent<DicingImage>();
			this.EyeBlink = this.gameObject.AddComponent<EyeBlinkDicing>();
			this.LipSynch = this.gameObject.AddComponent<LipSynchDicing>();
			this.Animation = this.gameObject.AddComponent<AdvAnimationPlayer>();
		}

		protected override Material Material { get { return Dicing.material; } set { Dicing.material = value; } }

		//********描画時にクロスフェードが失敗するであろうかのチェック********//
		internal override bool CheckFailedCrossFade(AdvGraphicInfo graphic)
		{
			return !EnableCrossFade(graphic);
		}

		//********描画時のリソース変更********//
		internal override void ChangeResourceOnDraw(AdvGraphicInfo graphic, float fadeTime)
		{
			Dicing.material = graphic.RenderTextureSetting.GetRenderMaterialIfEnable(Dicing.material);
			//既に描画されている場合は、クロスフェード用のイメージを作成
			bool crossFade = TryCreateCrossFadeImage(fadeTime, graphic);
			if (!crossFade)
			{
				ReleaseCrossFadeReference();
				this.gameObject.RemoveComponent<UguiCrossFadeDicing>();
			}
			DicingTextures dicingTexture = graphic.File.UnityObject as DicingTextures;
			string pattern = graphic.SubFileName;
			Dicing.DicingData = dicingTexture;
			Dicing.ChangePattern(pattern);
			Dicing.SetNativeSize();

			//目パチを設定
			SetEyeBlinkSync(graphic.EyeBlinkData);
			//口パクを設定
			SetLipSynch(graphic.LipSynchData);
			//アニメーションを設定
			SetAnimation(graphic.AnimationData);
			if (!crossFade)
			{
				ParentObject.FadeIn(fadeTime, () => { });
			}
		}

		//文字列指定でのパターンチェンジ（キーフレームアニメーションに使う）
		public override void ChangePattern(string pattern)
		{
			this.Dicing.ChangePattern(pattern);
		}


		//アニメーションを設定
		protected void SetAnimation(AdvAnimationData data)
		{
			Animation.Cancel();
			if (data != null)
			{
				Animation.Play(data.Clip, Engine.Page.SkippedSpeed);
			}
		}

		//目パチを設定
		protected void SetEyeBlinkSync(AdvEyeBlinkData data)
		{
			if (data == null)
			{
				EyeBlink.AnimationData = new MiniAnimationData();
			}
			else
			{
				EyeBlink.IntervalTime.Min = data.IntervalMin;
				EyeBlink.IntervalTime.Max = data.IntervalMax;
				EyeBlink.RandomDoubleEyeBlink = data.RandomDoubleEyeBlink;
				EyeBlink.EyeTag = data.Tag;
				EyeBlink.AnimationData = data.AnimationData;
			}
		}

		//口パクを設定
		protected void SetLipSynch(AdvLipSynchData data)
		{
			LipSynch.Cancel();
			if (data == null)
			{
				LipSynch.AnimationData = new MiniAnimationData();
			}
			else
			{
				LipSynch.Type = data.Type;
				LipSynch.Interval = data.Interval;
				LipSynch.ScaleVoiceVolume = data.ScaleVoiceVolume;
				LipSynch.LipTag = data.Tag;
				LipSynch.AnimationData = data.AnimationData;
				LipSynch.Play();
			}
		}


		//クロスフェード用のイメージを作成
		protected bool TryCreateCrossFadeImage(float time, AdvGraphicInfo graphic)
		{
			if (LastResource == null) return false;

			if(EnableCrossFade(graphic))
			{
				ReleaseCrossFadeReference();
				crossFadeReference = this.gameObject.AddComponent<AssetFileReference>();
				crossFadeReference.Init(LastResource.File);
				UguiCrossFadeDicing crossFade = this.gameObject.GetComponentCreateIfMissing<UguiCrossFadeDicing>();
				crossFade.CrossFade(
					Dicing.PatternData,
					Dicing.mainTexture,
					time,
					() =>
					{
						crossFade.RemoveComponentMySelf();
					}
					);
				return true;
			}
			else
			{
				return false;
			}
		}

		//今の表示状態と比較して、クロスフェード可能か
		bool EnableCrossFade(AdvGraphicInfo graphic)
		{
			DicingTextures dicingTexture = graphic.File.UnityObject as DicingTextures;
			string pattern = graphic.SubFileName;
			DicingTextureData data = Dicing.DicingData.GetTextureData(pattern);
			if (data == null) return false;

			return Dicing.DicingData == dicingTexture
				&& Dicing.rectTransform.pivot == graphic.Pivot
				&& Dicing.PatternData.Width == data.Width
				&& Dicing.PatternData.Height == data.Height;
		}
	}
}
