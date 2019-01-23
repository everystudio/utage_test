// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UtageExtensions;

namespace Utage
{

	/// <summary>
	/// プレハブオブジェクト表示のスーパークラス
	/// </summary>
	public abstract class AdvGraphicObjectPrefabBase : AdvGraphicBase
	{
		protected GameObject currentObject;

		Animator animator;

		string AnimationStateName { get; set; }

		//初期化処理
		public override void Init(AdvGraphicObject parentObject)
		{
			AnimationStateName = "";
			base.Init(parentObject);
		}

		//********描画時にクロスフェードが失敗するであろうかのチェック********//
		internal override bool CheckFailedCrossFade(AdvGraphicInfo grapic)
		{
			return LastResource != grapic;
		}

		//********描画時のリソース変更********//
		internal override void ChangeResourceOnDraw(AdvGraphicInfo grapic, float fadeTime)
		{
			//新しくリソースを設定
			if (LastResource != grapic)
			{
				currentObject = GameObject.Instantiate(grapic.File.UnityObject) as GameObject;
				Vector3 localPostion = currentObject.transform.localPosition;
				Vector3 localEulerAngles = currentObject.transform.localEulerAngles;
				Vector3 localScale = currentObject.transform.localScale;
				currentObject.transform.SetParent(this.transform);
				currentObject.transform.localPosition = localPostion;
				currentObject.transform.localScale = localScale;
				currentObject.transform.localEulerAngles = localEulerAngles;
				currentObject.ChangeLayerDeep(this.gameObject.layer);
				currentObject.gameObject.SetActive(true);

				animator = this.GetComponentInChildren<Animator>();
				ChangeResourceOnDrawSub(grapic);
			}

			if (LastResource == null)
			{
				ParentObject.FadeIn(fadeTime, () => { });
			}
		}

		//********描画時のリソース変更********//
		protected abstract void ChangeResourceOnDrawSub(AdvGraphicInfo grapic);
		//		{
		//			this.sprite = currentObject.GetComponent<SpriteRenderer>();
		//		}

		//拡大縮小の設定
		internal override void Scale(AdvGraphicInfo graphic)
		{
			this.transform.localScale = graphic.Scale * Layer.Manager.PixelsToUnits;
		}

		//配置
		internal override void Alignment(Utage.Alignment alignment, AdvGraphicInfo graphic)
		{
			this.transform.localPosition = graphic.Position;
		}

		//上下左右の反転
		internal override void Flip(bool flipX, bool flipY)
		{
		}

		//********描画時の引数適用********//
		internal override void SetCommandArg(AdvCommand command)
		{
			string stateName = command.ParseCellOptional<string>(AdvColumnName.Arg2, "");
			float fadeTime = command.ParseCellOptional<float>(AdvColumnName.Arg6, 0.2f);
			ChangeAnimationState(stateName, fadeTime);
		}

		void ChangeAnimationState(string name, float fadeTime)
		{
			AnimationStateName = name;
			if (!string.IsNullOrEmpty(AnimationStateName))
			{
				if (animator)
				{
					animator.CrossFadeInFixedTime(AnimationStateName, fadeTime);
				}
				else
				{
					//レガシーな手法
					Animation animation = GetComponentInChildren<Animation>();
					if (animation != null)
					{
						animation.CrossFade(AnimationStateName, fadeTime);
					}
				}
			}
		}

		//ルール画像つきのフェードイン（オブジェクト単位にかけるのでテクスチャ描き込み効果なし）
		public override void RuleFadeIn(AdvEngine engine, AdvTransitionArgs data, Action onComplete)
		{
			Debug.LogError(this.gameObject.name + " is not support RuleFadeIn", this.gameObject);
			if (onComplete != null) onComplete();
		}

		//ルール画像つきのフェードアウト（オブジェクト単位にかけるのでテクスチャ描き込み効果なし）
		public override void RuleFadeOut(AdvEngine engine, AdvTransitionArgs data, Action onComplete)
		{
			Debug.LogError(this.gameObject.name + " is not support RuleFadeOut", this.gameObject);
			if (onComplete != null) onComplete();
		}


		enum SaveType
		{
			Animator,
			Other,
		};
		const int Version = 1;
		public override void Write(BinaryWriter writer)
		{
			writer.Write(Version);
			if (animator != null)
			{
				writer.Write(SaveType.Animator.ToString());
				int count = animator.layerCount;
				writer.Write(count);
				for(int i = 0; i < count; ++i)
				{
					AnimatorStateInfo info =  animator.IsInTransition(i) ?
						//状態遷移中なら次の状態を
						animator.GetNextAnimatorStateInfo(i):
						//そうでない今の状態を
						animator.GetCurrentAnimatorStateInfo(i);
					writer.Write(info.fullPathHash);
					writer.Write(info.normalizedTime);
				}
			}
			else
			{
				writer.Write(SaveType.Other.ToString());
				writer.Write(AnimationStateName);
			}
		}

		public override void Read(BinaryReader reader)
		{
			int version = reader.ReadInt32();
			if (version < 0 || version > Version)
			{
				Debug.LogError(LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.UnknownVersion, version));
				return;
			}

			string typeName = reader.ReadString();
			SaveType type = (SaveType)System.Enum.Parse(typeof(SaveType), typeName);
			switch (type)
			{
				case SaveType.Animator:
					{
						int count = reader.ReadInt32();
						for (int i = 0; i < count; ++i)
						{
							int stateNameHash = reader.ReadInt32();
							int layer = i;
							float normalizedTime = reader.ReadSingle();
							animator.Play(stateNameHash, layer, normalizedTime);
						}
					}
					break;
				case SaveType.Other:
				default:
					string stateName = reader.ReadString();
					ChangeAnimationState(stateName, 0);
					break;
			}
		}
	}
}
