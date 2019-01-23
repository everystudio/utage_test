// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Utage
{

	/// <summary>
	/// 第六引数でウェイト処理をする系のコマンドの基底クラス
	/// </summary>
	public abstract class AdvCommandWaitBase : AdvCommand
	{
		public AdvCommandWaitType WaitType { get; protected set; }

		protected AdvCommandWaitBase(StringGridRow row) : base(row)
		{
		}

		//コマンド実行
		public override void DoCommand(AdvEngine engine)
		{
			CurrentTread.WaitManager.StartCommand(this);
			OnStart(engine,CurrentTread);
		}

		//コマンド終了待ち
		public override bool Wait(AdvEngine engine)
		{
			//タイプによってウェイトチェック
			switch (WaitType)
			{
				case AdvCommandWaitType.ThisAndAdd:
					return CurrentTread.WaitManager.IsWaitingAdd;
				case AdvCommandWaitType.PageWait:
				case AdvCommandWaitType.InputWait:
				case AdvCommandWaitType.Add:
				case AdvCommandWaitType.NoWait:
				default:
					return false;
			}
		}


		//開始時のコールバック
		protected abstract void OnStart(AdvEngine engine, AdvScenarioThread thread);

		//終了時のコールバック
		internal virtual void OnComplete(AdvScenarioThread thread)
		{
			thread.WaitManager.CompleteCommand(this);
		}
	}
}
