// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UtageExtensions;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// エージングテスト。選択肢などを自動入力する
/// </summary>
namespace Utage
{
	[AddComponentMenu("Utage/ADV/Extra/SelectionAutomatic")]
	public class AdvAgingTest : MonoBehaviour
	{
		//選択肢の選び方
		public enum Type
		{
			Random,		//ランダム
			DepthFirst,	//深さ優先
		}
		[SerializeField]
		Type type = Type.Random;


		//無効化フラグ
		[SerializeField]
		bool disable = false;
		public bool Disable
		{
			get { return disable; }
			set { disable = value; }
		}

		[System.Flags]
		enum SkipFlags
		{
			Voice = 0x1<<0,
			Movie = 0x1 << 1,
		}
		[SerializeField,EnumFlags]
		SkipFlags skipFilter = 0;

		/// <summary>ADVエンジン</summary>
		public AdvEngine Engine { get { return this.engine ?? (this.engine = FindObjectOfType<AdvEngine>()); } }
		[SerializeField]
		protected AdvEngine engine;

		public float waitTime = 1.0f;
		float time;

		public bool clearOnEnd = true;

		void Awake()
		{
			Engine.SelectionManager.OnBeginWaitInput.AddListener(OnBeginWaitInput);
			Engine.SelectionManager.OnUpdateWaitInput.AddListener(OnUpdateWaitInput);

			Engine.ScenarioPlayer.OnBeginCommand.AddListener(OnBeginCommand);
			Engine.ScenarioPlayer.OnUpdatePreWaitingCommand.AddListener(OnUpdatePreWaitingCommand);
			Engine.ScenarioPlayer.OnEndScenario.AddListener(OnEndScenario);
		}

		//選択肢待ち開始
		void OnBeginWaitInput(AdvSelectionManager selection)
		{
			time = -Time.deltaTime;
		}

		//選択肢待機中
		void OnUpdateWaitInput(AdvSelectionManager selection)
		{
			if (Disable) return;

			time += Time.deltaTime;
			if (time >= waitTime)
			{
				selection.SelectWithTotalIndex(GetIndex(selection));
			}
		}

		//選択肢待ち開始
		void OnBeginCommand(AdvCommand command)
		{
			time = -Time.deltaTime;
		}

		//コマンド待機中
		void OnUpdatePreWaitingCommand(AdvCommand command)
		{
			if (Disable) return;
			if (!IsWaitInputCommand(command)) return;

			time += Time.deltaTime;
			if (time >= waitTime)
			{
				if (command is AdvCommandWaitInput)
				{
					Engine.UiManager.IsInputTrig = true;
				}
				if (command is AdvCommandSendMessage)
				{
					engine.ScenarioPlayer.SendMessageTarget.SafeSendMessage("OnAgingInput", command);
				}
				if (command is AdvCommandMovie)
				{
					Engine.UiManager.IsInputTrig = true;
				}
				if (command is AdvCommandText)
				{
					if (Engine.SoundManager.IsPlayingVoice())
					{
						Engine.Page.InputSendMessage();
					}
				}
			}
		}

		void OnEndScenario(AdvScenarioPlayer player)
		{
			if (clearOnEnd)
			{
				this.selectedDictionary.Clear();
			}
		}


		bool IsWaitInputCommand(AdvCommand command)
		{
			if (command is AdvCommandWaitInput)
			{
				return true;
			}
			if (command is AdvCommandSendMessage)
			{
				return true;
			}
			if (command is AdvCommandMovie)
			{
				return (skipFilter & SkipFlags.Movie) == SkipFlags.Movie;
			}
			if (command is AdvCommandText)
			{
				return (skipFilter & SkipFlags.Voice) == SkipFlags.Voice;
			}
			return false;
		}


		//選択するインデックス取得
		int GetIndex(AdvSelectionManager selection)
		{
			switch (type)
			{
				case Type.DepthFirst:
					//深さ優先（チュートリアルなど、網羅的に選択する場合に）
					return GetIndexDepthFirst(selection);
				default:
					//ランダム
					return UnityEngine.Random.Range(0, selection.TotalCount);
			}
		}

		//深さ優先の場合にインデックスを取得（チュートリアルなど、網羅的に選択する場合に）
		int GetIndexDepthFirst(AdvSelectionManager selection)
		{
			int index;
			if (!selectedDictionary.TryGetValue(Engine.Page.CurrentData, out index))
			{
				index = 0;
				selectedDictionary.Add(Engine.Page.CurrentData, index);
			}
			else
			{
				if (index + 1 < selection.TotalCount)
				{
					++index;
				}
				selectedDictionary[Engine.Page.CurrentData] = index;
			}
			return index;
		}
		//選択した選択肢情報を記憶
		Dictionary<AdvScenarioPageData, int> selectedDictionary = new Dictionary<AdvScenarioPageData, int>();
	}
}
