// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using System.Collections;
using UtageExtensions;

namespace Utage
{

	/// <summary>
	/// コマンド：アニメーションクリップの再生をする
	/// </summary>
	public class AdvCommandPlayAnimatin : AdvCommandEffectBase
	{
		string animationName;

		public AdvCommandPlayAnimatin(StringGridRow row, AdvSettingDataManager dataManager)
			: base(row)
		{
			this.animationName = ParseCell<string>(AdvColumnName.Arg2);
		}

		//エフェクト開始時のコールバック
		protected override void OnStartEffect(GameObject target, AdvEngine engine, AdvScenarioThread thread)
		{
			AdvAnimationData animationData = engine.DataManager.SettingDataManager.AnimationSetting.Find(animationName);
			if (animationData == null)
			{
				Debug.LogError(RowData.ToErrorString("Animation " + animationName + " is not found"));
				OnComplete(thread);
				return;
			}

			AdvAnimationPlayer player = target.AddComponent<AdvAnimationPlayer>();
			player.AutoDestory = true;
			player.EnableSave = true;
			player.Play(animationData.Clip, engine.Page.SkippedSpeed,
				() =>
				{
					OnComplete(thread);
				});
		}
	}
}
