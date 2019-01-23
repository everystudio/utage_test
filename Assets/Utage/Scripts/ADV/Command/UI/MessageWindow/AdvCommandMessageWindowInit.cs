// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using System.Collections.Generic;


namespace Utage
{

	/// <summary>
	/// コマンド：MessageWindow操作　初期化
	/// </summary>
	internal class AdvCommandMessageWindowInit : AdvCommand
	{
		public AdvCommandMessageWindowInit(StringGridRow row)
			: base(row)
		{
			if (!IsEmptyCell(AdvColumnName.Arg1)) names.Add(ParseCell<string>(AdvColumnName.Arg1));
			if (!IsEmptyCell(AdvColumnName.Arg2)) names.Add(ParseCell<string>(AdvColumnName.Arg2));
			if (!IsEmptyCell(AdvColumnName.Arg3)) names.Add(ParseCell<string>(AdvColumnName.Arg3));
			if (!IsEmptyCell(AdvColumnName.Arg4)) names.Add(ParseCell<string>(AdvColumnName.Arg4));
			if (!IsEmptyCell(AdvColumnName.Arg5)) names.Add(ParseCell<string>(AdvColumnName.Arg5));
			if (!IsEmptyCell(AdvColumnName.Arg6)) names.Add(ParseCell<string>(AdvColumnName.Arg6));
			if (names.Count <= 0)
			{
				Debug.LogError(ToErrorString("Not set data in this command"));
			}
		}

		//ページ用のデータからコマンドに必要な情報を初期化
		public override void InitFromPageData(AdvScenarioPageData pageData)
		{
			if (names.Count > 0)
			{
				pageData.InitMessageWindowName(this, names[0]);
			}
		}

		public override void DoCommand(AdvEngine engine)
		{
			engine.MessageWindowManager.ChangeActiveWindows(names);
		}

		List<string> names = new List<string>();
	}
}
