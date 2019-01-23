// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// コマンド：パラメーターに数値代入
	/// </summary>
	internal class AdvCommandParam : AdvCommand
	{

		public AdvCommandParam(StringGridRow row, AdvSettingDataManager dataManager)
			: base(row)
		{
			this.exp = dataManager.DefaultParam.CreateExpression(ParseCell<string>(AdvColumnName.Arg1));
			if (this.exp.ErrorMsg != null)
			{
				Debug.LogError(ToErrorString(this.exp.ErrorMsg));
			}
		}

		public override void DoCommand(AdvEngine engine)
		{
			engine.Param.CalcExpression(exp);
		}
		ExpressionParser exp;
	}
}