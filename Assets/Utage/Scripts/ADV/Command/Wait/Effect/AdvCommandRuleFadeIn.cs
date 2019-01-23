// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UtageExtensions;

namespace Utage
{

	/// <summary>
	/// コマンド：ルール画像付きのフェードイン
	/// </summary>
	internal class AdvCommandRuleFadeIn : AdvCommandEffectBase
	{
		public AdvCommandRuleFadeIn(StringGridRow row)
			: base(row)
		{
			string textureName = ParseCell<string>(AdvColumnName.Arg2);
			float vague = ParseCellOptional<float>(AdvColumnName.Arg3, 0.2f);
			float time = ParseCellOptional<float>(AdvColumnName.Arg6, 0.2f);
			this.data = new AdvTransitionArgs(textureName, vague, time);
		}

		//エフェクト開始時のコールバック
		protected override void OnStartEffect(GameObject target, AdvEngine engine, AdvScenarioThread thread)
		{
			IAdvFade fade = target.GetComponentInChildren<IAdvFade>(true);
			if (fade == null)
			{
				Debug.LogError("Can't find [ " + this.TargetName + " ]");
				OnComplete(thread);
			}
			else
			{
				fade.RuleFadeIn(engine, data, ()=>OnComplete(thread) );
			}
		}

		AdvTransitionArgs data;
	}
}