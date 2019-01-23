// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura

namespace Utage
{

	/// <summary>
	/// コマンド：イベントCG表示OFF
	/// </summary>
	internal class AdvCommandBgEventOff : AdvCommand
	{
		public AdvCommandBgEventOff(StringGridRow row)
			: base(row)
		{
			this.fadeTime = ParseCellOptional<float>(AdvColumnName.Arg6, 0.2f);
		}

		public override void DoCommand(AdvEngine engine)
		{
			engine.GraphicManager.BgManager.FadeOutAll(engine.Page.ToSkippedTime(this.fadeTime));
		}

		float fadeTime;
	}
}