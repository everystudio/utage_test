// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UtageExtensions;

namespace Utage
{

	/// <summary>
	/// コマンド：イメージエフェクト開始
	/// </summary>
	internal class AdvCommandImageEffectBase : AdvCommandEffectBase
	{
		string animationName;
		float time;
		string imageEffectType { get; set; }
		bool inverse;

		public AdvCommandImageEffectBase(StringGridRow row, AdvSettingDataManager dataManager, bool inverse)
			: base(row)
		{
			this.inverse = inverse;
			this.targetType = AdvEffectManager.TargetType.Camera;
			this.imageEffectType = RowData.ParseCell<string>(AdvColumnName.Arg2.ToString());
			this.animationName = ParseCellOptional<string>(AdvColumnName.Arg3,"");
			this.time = ParseCellOptional<float>(AdvColumnName.Arg6, 0);
		}

		//エフェクト開始時のコールバック
		protected override void OnStartEffect(GameObject target, AdvEngine engine, AdvScenarioThread thread)
		{
			Camera camera = target.GetComponentInChildren<Camera>(true);
			ImageEffectBase imageEffect;
			bool alreadyEnabled;
			if (!ImageEffectUtil.TryGetComonentCreateIfMissing( imageEffectType, out imageEffect, out alreadyEnabled, camera.gameObject))
			{
				Complete(imageEffect, thread);
				return;
			}

			if (!inverse) imageEffect.enabled = true;

			bool enableAnimation = !string.IsNullOrEmpty(animationName);
			bool enableFadeStregth = imageEffect is IImageEffectStrength;

			if (!enableFadeStregth && !enableAnimation)
			{
				Complete(imageEffect, thread);
				return;
			}

			if (enableFadeStregth)
			{
				IImageEffectStrength fade = imageEffect as IImageEffectStrength;
				float start = inverse ? fade.Strength : 0;
				float end = inverse ? 0 : 1;
				Timer timer = camera.gameObject.AddComponent<Timer>();
				timer.AutoDestroy = true;
				timer.StartTimer(
					engine.Page.ToSkippedTime(this.time),
					(x) =>
					{
						fade.Strength = x.GetCurve(start, end);
					},
					(x) =>
					{
						if (!enableAnimation)
						{
							Complete(imageEffect, thread);
						}
					});
			}

			if(enableAnimation)
			{
				//アニメーションの適用
				AdvAnimationData animationData = engine.DataManager.SettingDataManager.AnimationSetting.Find(animationName);
				if (animationData == null)
				{
					Debug.LogError(RowData.ToErrorString("Animation " + animationName + " is not found"));
					Complete(imageEffect, thread);
					return;
				}

				AdvAnimationPlayer player = camera.gameObject.AddComponent<AdvAnimationPlayer>();
				player.AutoDestory = true;
				player.EnableSave = true;
				player.Play(animationData.Clip, engine.Page.SkippedSpeed,
					() =>
					{
						Complete(imageEffect,thread);
					});
			}
		}

		void Complete(ImageEffectBase imageEffect, AdvScenarioThread thread)
		{
			if (inverse) 
            {
                //                imageEffect.enabled = false;                
                UnityEngine.Object.DestroyImmediate(imageEffect);
            }
			OnComplete(thread);
		}
	}
}