// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// コマンド：ELSE IF処理
	/// </summary>
	internal class AdvCommandElseIf : AdvCommand
	{

		public AdvCommandElseIf(StringGridRow row, AdvSettingDataManager dataManager)
			: base(row)
		{
			this.exp = dataManager.DefaultParam.CreateExpressionBoolean(ParseCell<string>(AdvColumnName.Arg1));
			if (this.exp.ErrorMsg != null)
			{
				Debug.LogError(ToErrorString(this.exp.ErrorMsg));
			}
		}

		public override void DoCommand(AdvEngine engine)
		{
			CurrentTread.IfManager.ElseIf(engine.Param, exp);
		}

		//IF文タイプのコマンドか
		public override bool IsIfCommand { get { return true; } }

		ExpressionParser exp;
	}
}