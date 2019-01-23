// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// コマンド：テキスト表示（地の文）
	/// </summary>
	internal class AdvCommandWait : AdvCommand
	{

		public AdvCommandWait(StringGridRow row)
			: base(row)
		{
			this.time = ParseCell<float>(AdvColumnName.Arg6);
		}

		public override void DoCommand(AdvEngine engine)
		{
			waitEndTime = Time.time + (engine.Page.CheckSkip() ? time / engine.Config.SkipSpped : time);
		}

		public override bool Wait(AdvEngine engine)
		{
			return (Time.time < waitEndTime);
		}

		float time;
		float waitEndTime;
	}
}