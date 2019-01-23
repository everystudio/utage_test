﻿// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace Utage
{

	/// <summary>
	/// シナリオのページデータ
	/// </summary>
	public class AdvScenarioPageData
	{
		//コマンドのリスト
		public List<AdvCommand> CommandList { get; private set; }

		//ページ内のテキストデータのリスト
		//自動改行処理などのために、ページ内のテキストをあらかじめ全部知る必要がある
		public List<AdvCommandText> TextDataList { get; private set; }

		/// <summary>
		/// シナリオラベル
		/// </summary>
		public AdvScenarioLabelData ScenarioLabelData { get; private set; }

		/// <summary>
		/// このページからジャンプするラベルのリスト
		/// </summary>
		List<AdvScenarioLabelData> JumpLabelList { get; set; }

		/// <summary>
		/// このページから自動ジャンプするラベルのリスト
		/// </summary>
		List<AdvScenarioLabelData> AutoJumpLabelList { get; set; }


		/// <summary>
		/// ページ番号
		/// </summary>
		public int PageNo { get; private set; }

		//メッセージウィンドウ名
		public string MessageWindowName { get; set; }

		//テキスト表示系の一番最初のコマンドのインデックス
		internal int IndexTextTopCommand { get; private set; }

		//セーブが有効か(ジャンプのみのページなどは除外)
		internal bool EnableSave { get; private set; }

		public AdvScenarioPageData(AdvScenarioLabelData scenarioLabelData, int pageNo, List<AdvCommand> commandList)
		{
			this.TextDataList = new List<AdvCommandText>();
			this.ScenarioLabelData = scenarioLabelData;
			this.PageNo = pageNo;
			this.CommandList = commandList;
		}

		internal void Init()
		{ 
			CommandList.ForEach(command =>
			{
				command.InitFromPageData(this);
			});
			EnableSave = true;

			for (int i = 0; i < CommandList.Count; ++i)
			{
				if (CommandList[i].IsTypePage())
				{
					IndexTextTopCommand = i;
					break;
				}
			}

			//ToDo　本当ならウェイトのないコマンドのみで構成されているならセーブ無効
			/*			if (CommandList.Count == 1)
						{
							CommandList[0];
							EnableSave = false;
						}*/
		}

		//ジャンプ先のシナリオデータ
		public List<AdvScenarioLabelData> GetJumpScenarioLabelDataList(AdvDataManager dataManager)
		{
			if (JumpLabelList != null) return JumpLabelList;

			this.JumpLabelList = new List<AdvScenarioLabelData>();
			this.CommandList.ForEach(
					command =>
					{
						///このシナリオからリンクするジャンプ先のシナリオラベルを取得
						string[] jumpLabels = command.GetJumpLabels();
						if (jumpLabels != null)
						{
							foreach (var jumpLabel in jumpLabels)
							{
								JumpLabelList.Add(dataManager.FindScenarioLabelData(jumpLabel));
							}
						}
					});
			return JumpLabelList;
		}


		//自動ジャンプ先のシナリオデータ
		internal List<AdvScenarioLabelData> GetAutoJumpLabels(AdvDataManager dataManager)
		{
			if (AutoJumpLabelList != null) return AutoJumpLabelList;
			this.AutoJumpLabelList = new List<AdvScenarioLabelData>();
			this.CommandList.ForEach(
					command =>
					{
						///このシナリオからリンクするジャンプ先のシナリオラベルを取得
						string[] jumpLabels = command.GetJumpLabels();
						if (jumpLabels != null)
						{
							if (command is AdvCommandJump ||
								command is AdvCommandJumpRandom ||
								command is AdvCommandJumpSubroutine ||
								command is AdvCommandJumpSubroutineRandom)
							{
								foreach (var jumpLabel in jumpLabels)
								{
									AutoJumpLabelList.Add(dataManager.FindScenarioLabelData(jumpLabel));
								}
							}
						}
					});
			return AutoJumpLabelList;
		}


		//データのダウンロード
		public void Download(AdvDataManager dataManager)
		{
			CommandList.ForEach((item) => item.Download(dataManager));
		}

		//指定インデックスのコマンドを取得
		public AdvCommand GetCommand(int index)
		{
			return (index < CommandList.Count) ? CommandList[index] : null;
		}
		
		//ファイルセットを追加
		public void AddToFileSet( HashSet<AssetFile> fileSet)
		{
			foreach( AdvCommand command in CommandList)
			{
				if (command.IsExistLoadFile())
				{
					command.LoadFileList.ForEach((item) => fileSet.Add(item));
				}
			}
		}


		internal void AddTextData(AdvCommandText command)
		{
			TextDataList.Add(command);
		}

		//エンティティ処理のためにテキストデータを修正
		internal void ChangeTextDataOnCreateEntity(int index, AdvCommandText entity)
		{
			if (TextDataList.Count < index)
			{
				Debug.LogError("  Index error On CreateEntity ");
				return;
			}
			TextDataList[index] = entity;
		}

		internal void InitMessageWindowName(AdvCommand command, string messageWindowName)
		{
			if (string.IsNullOrEmpty(messageWindowName)) return;

			if (string.IsNullOrEmpty(MessageWindowName) )
			{
				MessageWindowName = messageWindowName;
			}
			else if (MessageWindowName != messageWindowName)
			{
				Debug.LogError(command.ToErrorString(messageWindowName + ": WindowName already set is this page"));
			}
		}

		public bool IsEmptyText
		{ 
			get
			{
				return TextDataList.Count <= 0;
			}
		}


		//テキスト開始部分のセーブが有効になるか
		internal bool EnableSaveTextTop(AdvCommand command)
		{
			if (command == null) return false;
			//そもそもセーブが無効
			if (!EnableSave) return false;
			//ページ開始時なので無効
			if ( command == GetCommand(0) ) return false;

			return (command == CommandList[IndexTextTopCommand]);
		}

#if UNITY_EDITOR

		// 文字数オーバー　チェック
		internal int EditorCheckCharacterCount(AdvEngine engine, ref string currentWindowName, Dictionary<string, AdvUguiMessageWindow> windows)
		{
			AdvUguiMessageWindow messageWindow;
			if (!string.IsNullOrEmpty(MessageWindowName)) currentWindowName = MessageWindowName;

			if (!windows.TryGetValue(currentWindowName, out messageWindow))
			{
				foreach (var window in windows.Values)
				{
					messageWindow = window;
					break;
				}
			}
			bool isActive = messageWindow.gameObject.activeSelf;
			if (!isActive)
			{
				messageWindow.gameObject.SetActive(true);
			}
			UguiLocalizeBase[] localizeArray = messageWindow.GetComponentsInChildren<UguiLocalizeBase>();
			foreach( var item in localizeArray )
			{
				item.EditorRefresh();
			}			

			UguiNovelText textGUI = messageWindow.Text;
			string oldText = textGUI.text;
			string text = MakeText();
			string errorString;			
			int len;
			if (!textGUI.TextGenerator.EditorCheckRect(text, out len, out errorString) )
			{
				Debug.LogError("TextOver:" + TextDataList[0].RowData.ToStringOfFileSheetLine() + "\n" + errorString);
			}
			textGUI.text = oldText;
			foreach (var item in localizeArray)
			{
				item.ResetDefault();
			}
			messageWindow.gameObject.SetActive(isActive);
			return len;
		}

		internal string MakeText()
		{
			StringBuilder builder = new StringBuilder();
			foreach (var item in TextDataList)
			{
				builder.Append(item.ParseCellLocalizedText());
				if (item.IsNextBr) builder.Append("\n");
			}
			return builder.ToString();
		}
#endif

		//ロード直後のときなどのために、Ifスキップ
		internal int GetIfSkipCommandIndex(int index)
		{
			for (int i = index; i < CommandList.Count; ++i)
			{
				AdvCommand command = CommandList[i];
				//AdvCommandIfで始まっていない場合は、AdvCommandEndIfまでスキップする
				if (command.IsIfCommand)
				{
					if (command.GetType() == typeof(AdvCommandIf))
					{
						return index;
					}
					else
					{
						for (int j = index + 1; j < CommandList.Count; ++j)
						{
							if (CommandList[j].GetType() == typeof(AdvCommandEndIf))
							{
								return j;
							}
						}
					}
				}
			}
			return index;
		}
	}
}