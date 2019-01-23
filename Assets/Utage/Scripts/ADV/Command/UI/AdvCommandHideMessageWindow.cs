// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura

namespace Utage
{

	/// <summary>
	/// コマンド：メッセージウィンドウを非表示
	/// </summary>
	internal class AdvCommandHideMessageWindow : AdvCommand
	{
		public AdvCommandHideMessageWindow(StringGridRow row)
			: base(row)
		{
		}

		public override void DoCommand(AdvEngine engine)
		{
			engine.UiManager.HideMessageWindow();
		}
	}
}
