// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura

namespace Utage
{

	/// <summary>
	/// コマンド：メニューボタンを非表示
	/// </summary>
	internal class AdvCommandHideMenuButton : AdvCommand
	{
		public AdvCommandHideMenuButton(StringGridRow row)
			: base(row)
		{
		}

		public override void DoCommand(AdvEngine engine)
		{
			engine.UiManager.HideMenuButton();
		}
	}
}
