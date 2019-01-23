// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Text;

namespace Utage
{
	[System.Serializable]
	public class AdvPageEvent : UnityEvent<AdvPage> { }

	/// <summary>
	/// テキストメッセージ制御
	/// </summary>
	[AddComponentMenu("Utage/ADV/Internal/MessageWindow")]
	public class AdvPage : MonoBehaviour
	{
		//ページの開始
		public AdvPageEvent OnBeginPage { get { return onBeginPage; } }
		[SerializeField]
		AdvPageEvent onBeginPage = new AdvPageEvent();

		//現在ページのテキストの表示開始
		public AdvPageEvent OnBeginText { get { return onBeginText; } }
		[SerializeField]
		AdvPageEvent onBeginText = new AdvPageEvent();

		//現在ページのテキストが変わった
		public AdvPageEvent OnChangeText { get { return onChangeText; } }
		[SerializeField]
		AdvPageEvent onChangeText = new AdvPageEvent();

		//文字送り中
		public AdvPageEvent OnUpdateSendChar { get { return onUpdateSendChar; } }
		[SerializeField]
		AdvPageEvent onUpdateSendChar = new AdvPageEvent();		

		//現在ページのテキストの表示終了
		public AdvPageEvent OnEndText { get { return onEndText; } }
		[SerializeField]
		AdvPageEvent onEndText = new AdvPageEvent();

		//ページの終了
		public AdvPageEvent OnEndPage { get { return onEndPage; } }
		[SerializeField]
		AdvPageEvent onEndPage = new AdvPageEvent();

		//ステータス変更があったとき
		public AdvPageEvent OnChangeStatus { get { return onChangeStatus; } }
		[SerializeField]
		AdvPageEvent onChangeStatus = new AdvPageEvent();

		//ページ内の入力待ち開始
		public AdvPageEvent OnTrigWaitInputInPage { get { return onTrigWaitInputInPage; } }
		[SerializeField]
		AdvPageEvent onTrigWaitInputInPage = new AdvPageEvent();

		//改ページ入力待ち開始
		public AdvPageEvent OnTrigWaitInputBrPage { get { return onTrigWaitInputBrPage; } }
		[SerializeField]
		AdvPageEvent onTrigWaitInputBrPage = new AdvPageEvent();


		//入力待ちの間に、入力イベントがあった
		public AdvPageEvent OnTrigInput { get { return onTrigInput; } }
		[SerializeField]
		AdvPageEvent onTrigInput = new AdvPageEvent();

		//現在のページのデータ
		public AdvScenarioPageData CurrentData { get; private set; }

		/// <summary>
		/// シナリオラベル
		/// </summary>
		public string ScenarioLabel{ get { return ( CurrentData == null ) ?  "" : CurrentData.ScenarioLabelData.ScenarioLabel; }	}

		/// <summary>
		/// ページ番号
		/// </summary>
		public int PageNo{ get { return (CurrentData == null) ? 0 : CurrentData.PageNo; } }

		/// <summary>
		/// セーブポイントか
		/// </summary>
		public bool IsSavePoint { get { return (CurrentData == null) ? false : (CurrentData.PageNo == 0) && CurrentData.ScenarioLabelData.IsSavePoint; } }

		/// <summary>
		/// 既読チェック
		/// </summary>
		public bool CheckReadPage()
		{
			return Engine.SystemSaveData.ReadData.CheckReadPage(this.ScenarioLabel, this.PageNo);
		}

		//テキストデータ
		public TextData TextData { get; private set; }


		/// <summary>
		/// 現在のコマンドのテキストデータ（）
		/// </summary>
		public AdvCommandText CurrentTextDataInPage { get; private set; }

		/// <summary>
		/// このページ内のテキストデータのリスト
		/// </summary>
		List<AdvCommandText> TextDataList { get { return textDataList; } }
		List<AdvCommandText> textDataList = new List<AdvCommandText>();

		/// <summary>
		/// 現在のキャラクター
		/// </summary>
		public AdvCharacterInfo CharacterInfo { get; set; }

		/// <summary>
		/// 表示する名前テキスト
		/// </summary>
		public string NameText
		{
			get
			{
				return (CharacterInfo == null) ? "" : CharacterInfo.LocalizeNameText;
			}
		}

		/// <summary>
		/// キャラクターラベル
		/// </summary>
		public string CharacterLabel
		{
			get
			{
				return (CharacterInfo == null) ? "" : CharacterInfo.Label;
			}
		}


		/// <summary>
		/// セーブデータのタイトル
		/// </summary>
		public string SaveDataTitle { get; private set; }

		/// <summary>
		/// 現在の表示するテキストの長さ
		/// </summary>
		public int CurrentTextLength { get; protected set; }

		/// <summary>
		/// 現在の表示するテキストの長さの最大値
		/// </summary>
		public int CurrentTextLengthMax { get; private set; }

		/// <summary>
		/// 現在のリップシンク用の文字
		/// </summary>
		public char CurrenLipiSyncWord
		{
			get
			{
				return CurrentCharData.Char;
			}
		}

		/// <summary>
		/// 現在の文字
		/// </summary>
		public CharData CurrentCharData
		{
			get
			{
				int index = Mathf.Clamp(CurrentTextLength, 0, TextData.ParsedText.CharList.Count-1);
				return TextData.ParsedText.CharList[index];
			}
		}

		//ページのステータス
		public enum PageStatus
		{
			None,				//初期状態
			SendChar,           //文字送り中
			WaitEffectOnInputInPage,//ページ末端でのエフェクト終了待ち
			WaitInputInPage,	//ページ内入力待ち
			OtherCommandInPage,	//ページ内のテキスト系以外のコマンド実行中
			WaitEffectOnEndPage,//ページ末端でのエフェクト終了待ち
			WaitInputBrPage,	//改ページ入力待ち
		};
		public PageStatus Status
		{
			get { return status; }
			set
			{
				if (status == value)
				{
					return;
				}
				status = value;

				this.OnChangeStatus.Invoke(this);
				switch(Status)
				{
					case PageStatus.WaitInputInPage:
						this.OnTrigWaitInputInPage.Invoke(this);
						break;
					case PageStatus.WaitInputBrPage:
						this.OnTrigWaitInputBrPage.Invoke(this);
						break;
					default:
						break;
				}
			}
		}
		PageStatus status = PageStatus.None;

		//文字送り中
		public bool IsSendChar
		{
			get { return Status == PageStatus.SendChar; }
		}

		//テキスト系コマンドの実行待ち中か
		public bool IsWaitTextCommand
		{
			get
			{
				if (Engine.SelectionManager.IsWaitInput) return true;
				switch (Status)
				{
					case PageStatus.SendChar:
					case PageStatus.WaitEffectOnInputInPage:
					case PageStatus.WaitInputInPage:
					case PageStatus.WaitEffectOnEndPage:
					case PageStatus.WaitInputBrPage:
						return true;
					default:
						return false;
				}
			}
		}

		//ページ内入力待ち中か
		public bool IsWaitInputInPage
		{
			get
			{
				return (Status == PageStatus.WaitInputInPage) || IsWaitingInputCommand;
			}
		}
		[System.Obsolete("Use IsWaitInputInPage instead")]
		public bool IsWaitIntputInPage { get { return IsWaitInputInPage; } }

		//入力待ちコマンドによる待機
		public bool IsWaitingInputCommand { get; set; }
		[System.Obsolete("Use IsWaitingInputCommand instead")]
		public bool IsWaitingIntputCommand { get { return IsWaitingInputCommand; } }


		//改ページ待ち中か
		public bool IsWaitBrPage
		{
			get
			{
				return (Status == PageStatus.WaitInputBrPage );
			}
		}


		[System.Obsolete]
		//テキスト表示中か(廃止予定)
		public bool IsShowingText
		{
			get { return Engine.UiManager.IsShowingMessageWindow; }
		}

		[System.Obsolete]
		//入力待ち中か(廃止予定)
		public bool IsWaitPage
		{
			get { return Engine.UiManager.IsShowingMessageWindow || Engine.SelectionManager.IsWaitInput; }
		}

		//ページ状態制御
		AdvPageController contoller = new AdvPageController();
		public AdvPageController Contoller
		{
			get { return contoller; }
		}

		public AdvEngine Engine { get { return this.engine ?? (this.engine = GetComponent<AdvEngine>()); } }
		AdvEngine engine;

		AdvIfManager MainThreadIfManager { get { return Engine.ScenarioPlayer.MainThread.IfManager; } }		

		/// <summary>
		/// 文字送りの入力
		/// 外部から呼ぶこと
		/// </summary>
		public void InputSendMessage() { isInputSendMessage = true; }
		bool IsInputSendMessage() { return isInputSendMessage || CheckSkip(); }
		bool isInputSendMessage;

		bool LastInputSendMessage { get; set; }

		float deltaTimeSendMessage;			//テキスト送りに使う時間経過
		float waitingTimeInput;				//入力待ち経過時間

		/// <summary>
		/// クリア
		/// </summary>
		public void Clear()
		{
			this.Status = PageStatus.None;
			this.CurrentData = null;
			this.CurrentTextLength = 0;
			this.CurrentTextLengthMax = 0;
			this.deltaTimeSendMessage = 0;
			this.Contoller.Clear();
		}

		/// <summary>
		/// ページ冒頭の初期化
		/// </summary>
		/// <param name="scenarioName">シナリオラベル</param>
		/// <param name="pageNo">ページ名</param>
		public void BeginPage(AdvScenarioPageData currentPageData)
		{
			this.LastInputSendMessage = false;
			this.CurrentData = currentPageData;
			this.CurrentTextLength = 0;
			this.CurrentTextLengthMax = 0;
			this.deltaTimeSendMessage = 0;
			this.Contoller.Clear();
			this.TextData = new TextData("");
			this.TextDataList.Clear();
			UpdateText();
			RemakeTextData();
			this.SaveDataTitle = CurrentData.ScenarioLabelData.SaveTitle;
			if (string.IsNullOrEmpty(this.SaveDataTitle))
			{
				this.SaveDataTitle = TextData.OriginalText;
			}

			this.OnBeginPage.Invoke(this);
			Engine.UiManager.OnBeginPage();
			Engine.MessageWindowManager.ChangeCurrentWindow(currentPageData.MessageWindowName);
			if (!currentPageData.IsEmptyText)
			{
				//バックログを追加
				Engine.BacklogManager.AddPage();
			}
		}

		/// <summary>
		/// ページ終了
		/// </summary>
		/// <param name="scenarioName">シナリオラベル</param>
		/// <param name="pageNo">ページ名</param>
		public void EndPage()
		{
			this.Status = PageStatus.None;
			
			//ボイスを止める
			if (Engine.Config.VoiceStopType == VoiceStopType.OnClick)
			{
				if (!CurrentData.IsEmptyText)
				{
					//ループじゃないボイスを止める
					Engine.SoundManager.StopVoiceIgnoreLoop();
				}
			}
			//既読ページ更新
			Engine.SystemSaveData.ReadData.AddReadPage(ScenarioLabel, PageNo);

			Engine.UiManager.OnEndPage();
			this.OnEndPage.Invoke(this);
			this.CurrentData = null;
			this.CurrentTextLength = 0;
			this.CurrentTextLengthMax = 0;
			this.deltaTimeSendMessage = 0;
			this.Contoller.Clear();
		}

		/// <summary>
		/// 現在のページのテキストデータを更新
		/// </summary>
		public void UpdatePageTextData(AdvPageControllerType pageCtrlType)
		{
			bool isLastBr = this.Contoller.IsBr;
			this.Contoller.Update(pageCtrlType);
			if (isLastBr) ++CurrentTextLengthMax;
			if (Engine.SelectionManager.TryStartWaitInputIfShowing())
			{
				return;
			}
			Engine.UiManager.ShowMessageWindow();
		}

		/// <summary>
		/// 現在のページのテキストデータを更新
		/// </summary>
		public void UpdatePageTextData( AdvCommandText text )
		{
			bool isLastBr = this.Contoller.IsBr;
			CurrentTextDataInPage = text;
			this.TextDataList.Add(text);
			this.Contoller.Update(CurrentTextDataInPage.PageCtrlType);
			if (isLastBr) ++CurrentTextLengthMax;

			RemakeText();
			Engine.UiManager.ShowMessageWindow();
			Engine.BacklogManager.AddCurrentPageLog(CurrentTextDataInPage, CharacterInfo);
		}

		public void RemakeText()
		{
			if (CurrentData == null) return;

			//エンティティ処理やIf分岐の場合は内容が変わっている可能性があるので再作成が必要
			RemakeTextData();

			this.Status = PageStatus.SendChar;
			if(CurrentTextLength==0)
			{
				this.OnBeginText.Invoke(this);
			}
			if (IsNoWaitAllText || CheckSkip() || LastInputSendMessage)
			{
				EndSendChar();
			}

			this.OnChangeText.Invoke(this);

			Engine.MessageWindowManager.OnPageTextChange(this);
			Engine.OnPageTextChange.Invoke(Engine);
		}

		void RemakeTextData()
		{
			//PageCtrlなどを考慮した、現在のテキストの最大長を取得
			{
				StringBuilder builder = new StringBuilder();
				foreach (var item in this.TextDataList)
				{
					builder.Append(item.ParseCellLocalizedText());
					if (item.IsNextBr) builder.Append("\n");
				}
				CurrentTextLengthMax = new TextData(builder.ToString()).Length;
			}

			//PageCtrlなどを考慮して、ページ内すべての表示するテキストを
			{
				StringBuilder builder2 = new StringBuilder();
				for (int i = 0; i < CurrentData.TextDataList.Count; ++i)
				{
					var item = CurrentData.TextDataList[i];
					builder2.Append(item.ParseCellLocalizedText());
					if (item.IsNextBr) builder2.Append("\n");
				}
				this.TextData = new TextData(builder2.ToString());
			}
		}


		public void OnChangeLanguage()
		{
			if (Application.isPlaying)
			{
				RemakeText();
			}
		}


		/// <summary>
		/// スキップのチェック
		/// </summary>
		/// <returns></returns>
		public bool CheckSkip()
		{
			return Engine.Config.CheckSkip(Engine.SystemSaveData.ReadData.CheckReadPage(ScenarioLabel, PageNo));
		}

		/// <summary>
		/// スキップを考慮した時間に
		/// </summary>
		/// <returns></returns>
		public float ToSkippedTime(float time)
		{
			return CheckSkip() ? time / Engine.Config.SkipSpped : time;
		}

		/// <summary>
		/// スキップを考慮した時間に
		/// </summary>
		/// <returns></returns>
		public float SkippedSpeed
		{
			get { return CheckSkip() ? Engine.Config.SkipSpped : 1; }
		}

		/// <summary>
		/// スキップ可能なページか
		/// </summary>
		/// <returns></returns>
		public bool EnableSkip()
		{
			if(Engine.Config.IsSkipUnread) return true;
			return  CheckReadPage();
		}

		/// <summary>
		/// テキストの更新。外部から呼ぶこと
		/// スキップやページ送りの入力の結果処理・文字送りなどの処理をする
		/// 更新の順番がシビアなので、内部でUpdateをしない。
		/// </summary>
		public void UpdateText()
		{
			LastInputSendMessage = false;
			//状態更新
			switch (Status)
			{
				case PageStatus.SendChar:
					UpdateSendChar();
					LastInputSendMessage = isInputSendMessage;
					break;
				case PageStatus.WaitInputInPage:
				case PageStatus.WaitInputBrPage:
					UpdateWaitInput();
					break;
				case PageStatus.WaitEffectOnInputInPage:
					UpdateWaitEffectOnInput();
					break;
				case PageStatus.WaitEffectOnEndPage:
					UpdateWaitEffectOnEndPage();
					break;
				default:
					break;
			}
			isInputSendMessage = false;
		}

		//文字送り
		void UpdateSendChar()
		{
			this.OnUpdateSendChar.Invoke(this);
			if (IsInputSendMessage() && !CurrentCharData.CustomInfo.IsSpeed)
			{
				//入力による文字飛ばし
				EndSendChar();
			}
			else
			{
				//文字送り
				float timeCharSend = Engine.Config.GetTimeSendChar(CheckReadPage());

				if (CurrentCharData.CustomInfo.IsSpeed && CurrentCharData.CustomInfo.speed >= 0 )
				{
					timeCharSend = CurrentCharData.CustomInfo.speed;
				}
				if (CurrentCharData.CustomInfo.IsInterval)
				{
					timeCharSend = CurrentCharData.CustomInfo.Interval;
				}

				SendChar(timeCharSend);
				if ((CurrentTextLength >= CurrentTextLengthMax))
				{
					EndSendChar();
				}
			}
		}


		//入力待ち
		void UpdateWaitInput()
		{
			if (Engine.Config.IsAutoBrPage)
			{
				//オートモードの場合ボイス終了後、一定時間を経過後していたらページ終了
				if(!Engine.SoundManager.IsPlayingVoice())
				{
					if (waitingTimeInput >= Engine.Config.AutoPageWaitTime)
					{
						ToNextCommand();
						return;
					}
				}
			}
			if (IsInputSendMessage())
			{
				//スキップではなく入力でのみトリガー発生
				if (isInputSendMessage)
				{
					OnTrigInput.Invoke(this);
				}
				if (Engine.Config.VoiceStopType == VoiceStopType.OnClick)
				{
					//ループじゃないボイスを止める
					Engine.SoundManager.StopVoiceIgnoreLoop();
				}	
				ToNextCommand();
				return;
			}

			if(!(Engine.Config.IsAutoBrPage && Engine.SoundManager.IsPlayingVoice()))
			{
				waitingTimeInput += Time.deltaTime;
			}
		}

		//ページ内入力のエフェクト終了待ち
		void UpdateWaitEffectOnInput()
		{
			if (!Engine.ScenarioPlayer.MainThread.WaitManager.IsWaitingInputEffect)
			{
				Status = PageStatus.WaitInputInPage;
			}
		}

		//改ページ後のページ末端のエフェクト終了待ち
		void UpdateWaitEffectOnEndPage()
		{
			if (!Engine.ScenarioPlayer.MainThread.WaitManager.IsWaitingPageEndEffect)
			{
				Status = PageStatus.WaitInputBrPage;
			}
		}

		//文字送り終了
		void EndSendChar()
		{
			this.OnEndText.Invoke(this);
			CurrentTextLength = CurrentTextLengthMax;
			//ページ末端で選択肢の入力待ちをする場合はすぐに次のコマンドへ
			if (CurrentTextDataInPage.IsPageEnd && Engine.SelectionManager.TryStartWaitInputIfShowing())
			{
				ToNextCommand();
				return;
			}

			if (Contoller.IsWaitInput)
			{
				if (CurrentTextDataInPage.IsPageEnd)
				{
					if (Engine.ScenarioPlayer.MainThread.WaitManager.IsWaitingPageEndEffect)
					{
						Status = PageStatus.WaitEffectOnEndPage;
					}
					else
					{
						Status = PageStatus.WaitInputBrPage;
					}
				}
				else
				{
					if (Engine.ScenarioPlayer.MainThread.WaitManager.IsWaitingInputEffect)
					{
						Status = PageStatus.WaitEffectOnInputInPage;
					}
					else
					{
						Status = PageStatus.WaitInputInPage;
					}
				}
				waitingTimeInput = 0;
			}
			else
			{
				ToNextCommand();
			}
		}

		//次のコマンドへ
		void ToNextCommand()
		{			
			//文字送りをしておく
			CurrentTextLength = CurrentTextLengthMax;
			if (CurrentTextDataInPage.IsPageEnd)
			{
				Status = PageStatus.None;
			}
			else
			{
				Status = PageStatus.OtherCommandInPage;
			}
		}

		/// <summary>
		/// 文字送り
		/// </summary>
		/// <param name="timeCharSend">文字送りにかかる時間</param>
		void SendChar(float timeCharSend)
		{
			if (timeCharSend <= 0)
			{
				if (IsNoWaitAllText)
				{
					CurrentTextLength = CurrentTextLengthMax;
					return;
				}
				else
				{
					timeCharSend = 0;
				}
			}

			deltaTimeSendMessage += Time.deltaTime;
			while (deltaTimeSendMessage >= 0)
			{
				++CurrentTextLength;
				deltaTimeSendMessage -= timeCharSend;
				if (CurrentTextLength > CurrentTextLengthMax)
				{
					CurrentTextLength = CurrentTextLengthMax;
					break;
				}
				if (CurrentCharData.CustomInfo.IsInterval || CurrentCharData.CustomInfo.IsSpeed)
				{
					break;
				}
			}
		}

		//このページがNoWait（文字送りスピードが0か）
		bool IsNoWaitAllText
		{
			get {
				if (TextData.IsNoWaitAll)
					return true;
				if (TextData.ContainsSpeedTag)
					return false;

				return (Engine.Config.GetTimeSendChar(CheckReadPage()) <= 0);
			}
		}
	}
}