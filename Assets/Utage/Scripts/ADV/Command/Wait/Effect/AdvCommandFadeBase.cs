// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UtageExtensions;

namespace Utage
{

	/// <summary>
	/// コマンド：フェードイン処理
	/// </summary>
	internal abstract class AdvCommandFadeBase: AdvCommandEffectBase
	{
		float time;
		bool inverse;
		Color color;

		public AdvCommandFadeBase(StringGridRow row, bool inverse)
			: base(row)
		{
			this.inverse = inverse;
		}

		protected override void OnParse()
		{
			this.color = ParseCellOptional<Color>(AdvColumnName.Arg1, Color.white);
			if (IsEmptyCell(AdvColumnName.Arg2))
			{
				this.targetName = "SpriteCamera";
			}
			else
			{
				//第2引数はターゲットの設定
				this.targetName = ParseCell<string>(AdvColumnName.Arg2);
			}

			this.time = ParseCellOptional<float>(AdvColumnName.Arg6,0.2f);

			this.targetType = AdvEffectManager.TargetType.Camera;

			ParseWait(AdvColumnName.WaitType);
		}

		protected override void OnStartEffect(GameObject target, AdvEngine engine, AdvScenarioThread thread)
		{
			Camera camera = target.GetComponentInChildren<Camera>(true);

			ImageEffectBase imageEffect;
			bool alreadyEnabled;
			ImageEffectUtil.TryGetComonentCreateIfMissing(ImageEffectType.ColorFade.ToString(), out imageEffect, out alreadyEnabled, camera.gameObject);
			ColorFade colorFade = imageEffect as ColorFade;
			float start,end;
			if (inverse)
			{
				//画面全体のフェードイン（つまりカメラのカラーフェードアウト）
				start = colorFade.color.a;
				end = 0;
			}
			else
			{
				//画面全体のフェードアウト（つまりカメラのカラーフェードイン）
				//colorFade.Strengthで、すでにフェードされているのでそちらの値をつかう
				start = alreadyEnabled ? colorFade.Strength : 0;
				end =  this.color.a;
			}
			imageEffect.enabled = true;

			colorFade.color = color;

			Timer timer = camera.gameObject.AddComponent<Timer>();
			timer.AutoDestroy = true;
			timer.StartTimer(
				engine.Page.ToSkippedTime(this.time),
				(x) =>
				{
					colorFade.Strength = x.GetCurve(start, end);
				},
				(x) =>
				{
					OnComplete(thread);
					if (inverse)
					{
						imageEffect.enabled = false;
						imageEffect.RemoveComponentMySelf();
					}
				});
		}
	}
}
