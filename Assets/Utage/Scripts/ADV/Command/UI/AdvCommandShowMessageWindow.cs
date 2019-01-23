﻿// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura

namespace Utage
{

	/// <summary>
	/// コマンド：メッセージウィンドウを表示
	/// </summary>
	internal class AdvCommandShowMessageWindow : AdvCommand
	{
		public AdvCommandShowMessageWindow(StringGridRow row)
			: base(row)
		{
		}

		public override void DoCommand(AdvEngine engine)
		{
			engine.UiManager.ShowMessageWindow();
		}
	}
}