// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// if分岐のデータクラス
	/// </summary>
	internal class AdvIfData
	{
		/// <summary>
		/// 入れ子の親
		/// </summary>
		public AdvIfData Parent { get { return parent; } set { parent = value; } }
		AdvIfData parent;

		/// <summary>
		/// スキップ中（条件判定がfalse）か
		/// </summary>
		public bool IsSkpping { get { return isSkpping; } set { isSkpping = value; } }
		bool isSkpping = false;			//スキップ中か

		bool isIf = false;				//if文がtrueになったか

		/// <summary>
		/// if文の開始
		/// </summary>
		/// <param name="param">判定に使う数値パラメーター</param>
		/// <param name="exp">判定式</param>
		public void BeginIf(AdvParamManager param, ExpressionParser exp)
		{
			isIf = param.CalcExpressionBoolean(exp);
			isSkpping = !isIf;
		}

		/// <summary>
		/// else if文の開始
		/// </summary>
		/// <param name="param">判定に使う数値パラメーター</param>
		/// <param name="exp">判定式</param>
		public void ElseIf(AdvParamManager param, ExpressionParser exp)
		{
			if (!isIf)
			{
				isIf = param.CalcExpressionBoolean(exp);
				isSkpping = !isIf;
			}
			else
			{
				isSkpping = true;
			}
		}

		/// <summary>
		/// else文の開始
		/// </summary>
		public void Else()
		{
			if (!isIf)
			{
				isIf = true;
				isSkpping = false;
			}
			else
			{
				isSkpping = true;
			}
		}

		/// <summary>
		/// if系処理の終了
		/// </summary>
		public void EndIf()
		{
			isSkpping = false;
		}
	};
}