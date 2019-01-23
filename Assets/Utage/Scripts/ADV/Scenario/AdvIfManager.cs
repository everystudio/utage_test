// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// IF分岐のマネージャー
	/// </summary>
	internal class AdvIfManager
	{
		//セーブデータのロード直後
		bool isLoadInit;

		public bool IsLoadInit
		{
			get { return isLoadInit; }
			set { isLoadInit = value; }
		}
		
		//処理中のif文
		AdvIfData current;

		/// <summary>
		/// クリア
		/// </summary>
		public void Clear()
		{
			current = null;
		}

		/// <summary>
		/// if文の開始
		/// </summary>
		/// <param name="param">判定に使う数値パラメーター</param>
		/// <param name="exp">判定式</param>
		public void BeginIf(AdvParamManager param, ExpressionParser exp)
		{
			IsLoadInit = false;
			AdvIfData new_if = new AdvIfData();
			if (null != current)
			{
				new_if.Parent = current;
			}
			current = new_if;
			current.BeginIf(param, exp);
		}

		/// <summary>
		/// else if文の開始
		/// </summary>
		/// <param name="param">判定に使う数値パラメーター</param>
		/// <param name="exp">判定式</param>
		public void ElseIf(AdvParamManager param, ExpressionParser exp)
		{
			if (current == null)
			{
				if(!IsLoadInit)
				{
					Debug.LogError(LanguageAdvErrorMsg.LocalizeTextFormat(AdvErrorMsg.ElseIf, exp));
				}
				current = new AdvIfData();
			}
			current.ElseIf(param, exp);
		}

		/// <summary>
		/// else文の開始
		/// </summary>
		public void Else()
		{
			if (current == null)
			{
				if(!IsLoadInit)
				{
					Debug.LogError(LanguageAdvErrorMsg.LocalizeTextFormat(AdvErrorMsg.Else));
				}
				current = new AdvIfData();
			}
			current.Else();
		}

		/// <summary>
		/// if文の終了
		/// </summary>
		public void EndIf()
		{
			if (current == null)
			{
				if (!IsLoadInit)
				{
					Debug.LogError(LanguageAdvErrorMsg.LocalizeTextFormat(AdvErrorMsg.EndIf));
				}
				current = new AdvIfData();
			}
			current.EndIf();
			current = current.Parent;
		}

		/// <summary>
		/// 分岐によるスキップをする（条件判定がfalseなため処理をしない）か
		/// </summary>
		/// <param name="command">コマンドデータ</param>
		/// <returns>スキップする場合はtrue。しない場合はfalse</returns>
		public bool CheckSkip(AdvCommand command)
		{
			if (command == null) return false;

			if (null == current)
			{
				return false;
			}
			else
			{
				if (current.IsSkpping)
				{
					return !command.IsIfCommand;
				}
				else
				{
					return false;
				}
			}
		}
	}
}