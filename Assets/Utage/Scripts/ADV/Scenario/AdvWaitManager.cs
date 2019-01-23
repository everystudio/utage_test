// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

namespace Utage
{
	//コマンド待機のタイプ
	public enum AdvCommandWaitType
	{
		ThisAndAdd,         //自分自身と、Addで設定したコマンドの終了待ちをする
		PageWait,           //改ページ後に、エフェクト終了待ちをする
		InputWait,          //クリック入力後に、エフェクト終了待ちをする
		Add,                //このコマンドではエフェクト終了待ちをしないが、Waitが設定されたエフェクトコマンドか、改ページのタイミングで終了待ちをする
		NoWait,             //
	};

	//シナリオスレッド内のコマンド待機処理のマネージャー
	internal class AdvWaitManager
	{
		//管理しているコマンドリスト
		List<AdvCommandWaitBase> commandList = new List<AdvCommandWaitBase>();

		internal void Clear()
		{
			this.commandList.Clear();
		}

		internal void StartCommand(AdvCommandWaitBase command)
		{
			//タイプによって管理リストから除外
			switch (command.WaitType)
			{
				case AdvCommandWaitType.NoWait:
					break;
				default:
					commandList.Add(command);
					break;
			}
		}

		internal void CompleteCommand(AdvCommandWaitBase command)
		{
			//タイプによって管理リストから除外
			switch (command.WaitType)
			{
				case AdvCommandWaitType.NoWait:
					break;
				default:
					commandList.Remove(command);
					break;
			}
		}

		//何らかの待機あり
		internal bool IsWaiting
		{
			get { return commandList.Count > 0; }
		}


		//待機コマンドの場合のチェック
		internal bool IsWaitingAdd
		{
			get
			{
				foreach (AdvCommandWaitBase command in commandList)
				{
					//タイプによって終了を待つ
					switch (command.WaitType)
					{
						case AdvCommandWaitType.ThisAndAdd:
						case AdvCommandWaitType.Add:
							return true;
					}
				}
				return false;
			}
		}

		//改ページ入力前にするエフェクトの終了待ち
		internal bool IsWaitingPageEndEffect
		{
			get
			{
				foreach (AdvCommandWaitBase command in commandList)
				{
					//タイプによって終了を待つ
					switch (command.WaitType)
					{
						case AdvCommandWaitType.Add:
						case AdvCommandWaitType.InputWait:
						case AdvCommandWaitType.PageWait:
							return true;
					}
				}
				return false;
			}
		}

		//改行入力などを入力前にするエフェクトの終了待ち
		internal bool IsWaitingInputEffect
		{
			get
			{
				foreach (AdvCommandWaitBase command in commandList)
				{
					//タイプによって終了を待つ
					switch (command.WaitType)
					{
						case AdvCommandWaitType.Add:
						case AdvCommandWaitType.InputWait:
							return true;
					}
				}
				return false;
			}
		}
	}
}