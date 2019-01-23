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
	/// グラフィックオブジェクトのデータ
	/// </summary>
	public abstract class AdvGraphicBase : MonoBehaviour
	{
		internal AdvGraphicObject ParentObject { get; set; }

		internal AdvGraphicLayer Layer { get { return ParentObject.Layer; } }
		internal AdvEngine Engine { get { return Layer.Manager.Engine; } }
		protected float PixelsToUnits { get { return Layer.Manager.PixelsToUnits; } }

		protected AdvGraphicInfo LastResource { get { return ParentObject.LastResource; } }

		//********初期化********//
		public virtual void Init(AdvGraphicObject parentObject)
		{
			ParentObject = parentObject;
		}

		//********描画時にクロスフェードが失敗するであろうかのチェック********//
		internal abstract bool CheckFailedCrossFade(AdvGraphicInfo graphic);

		//********描画時のリソース変更********//
		internal abstract void ChangeResourceOnDraw(AdvGraphicInfo graphic, float fadeTime);

		//********コマンド特有の引数を適用********//
		internal virtual void SetCommandArg(AdvCommand command)
		{
		}

		//拡大縮小の設定
		internal abstract void Scale(AdvGraphicInfo graphic);

		//配置
		internal abstract void Alignment(Utage.Alignment alignment, AdvGraphicInfo graphic);

		//上下左右の反転
		internal virtual void Flip(bool flipX, bool flipY)
		{
			if (!flipX && !flipY)
				return;
			UguiFlip flip = this.GetComponent<UguiFlip>();
			//フリップ設定を消してもう一度（順番が狂うので）
			if (flip != null)
			{
				flip.RemoveComponentMySelf();
			}
			flip = this.gameObject.AddComponent<UguiFlip>();
			flip.FlipX = flipX;
			flip.FlipY = flipY;
		}

		//エフェクト用の色が変化したとき
		internal virtual void OnEffectColorsChange(AdvEffectColor color)
		{
			UnityEngine.UI.Graphic graphic = GetComponent<UnityEngine.UI.Graphic>();
			if (graphic != null)
			{
				graphic.color = color.MulColor;
			}
		}

		//文字列指定でのパターンチェンジ（キーフレームアニメーションに使う）
		public virtual void ChangePattern(string pattern)
		{
		}

		//ルール画像つきのフェードイン（オブジェクト単位にかけるのでテクスチャ描き込み効果なし）
		public virtual void RuleFadeIn(AdvEngine engine, AdvTransitionArgs data, Action onComplete)
		{
			UguiTransition transition = this.gameObject.AddComponent<UguiTransition>();
			transition.RuleFadeIn(
				engine.EffectManager.FindRuleTexture(data.TextureName),
				data.Vague,
				false,
				data.GetSkippedTime(engine),
				() =>
				{
					transition.RemoveComponentMySelf(false);
					if (onComplete != null) onComplete();
				});
		}
		//ルール画像つきのフェードアウト（オブジェクト単位にかけるのでテクスチャ描き込み効果なし）
		public virtual void RuleFadeOut(AdvEngine engine, AdvTransitionArgs data, Action onComplete)
		{
			UguiTransition transition = this.gameObject.AddComponent<UguiTransition>();
			transition.RuleFadeOut(
				engine.EffectManager.FindRuleTexture(data.TextureName),
				data.Vague,
				false,
				data.GetSkippedTime(engine),
				() =>
				{
					transition.RemoveComponentMySelf(false);
					if (onComplete != null) onComplete();
				});
		}

		public virtual void Read(BinaryReader reader)
		{
		}

		public virtual void Write(BinaryWriter writer)
		{
		}
	}
}

