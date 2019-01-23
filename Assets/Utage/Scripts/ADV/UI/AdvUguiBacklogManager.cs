// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.UI;
using Utage;

namespace Utage
{

	/// <summary>
	/// バックログ表示
	/// </summary>
	[AddComponentMenu("Utage/ADV/UiBacklogManager")]
	public class AdvUguiBacklogManager : MonoBehaviour
	{
		public enum BacklogType
		{
			MessageWindow,		//メッセージウィンドウ
			FullScreenText,		//全画面テキスト
		};

		BacklogType Type { get { return type; } }
		[SerializeField]
		BacklogType type = BacklogType.MessageWindow;

		/// <summary>ADVエンジン</summary>
		[SerializeField]
		protected AdvEngine engine;

		/// <summary>選択肢のリストビュー</summary>
		public UguiListView ListView
		{
			get { return listView; }
		}
		[SerializeField]
		UguiListView listView = null;

		/// <summary>全画面テキスト</summary>
		public UguiNovelText FullScreenLogText
		{
			get { return fullScreenLogText; }
		}
		[SerializeField]
		UguiNovelText fullScreenLogText = null;

		//バックログデータへのインターフェース
		protected AdvBacklogManager BacklogManager { get { return engine.BacklogManager; } }
		
		//スクロール最下段でマウスホイール入力で閉じる入力するか
		public bool isCloseScrollWheelDown = false;


		/// <summary>開いているか</summary>
		public virtual bool IsOpen { get { return this.gameObject.activeSelf; } }

		/// <summary>
		/// 閉じる
		/// </summary>
		public virtual void Close()
		{
			if (ListView!=null) ListView.ClearItems();
			if (FullScreenLogText != null) FullScreenLogText.text = "";
			this.gameObject.SetActive(false);
		}

		/// <summary>
		/// 開く
		/// </summary>
		public virtual void Open()
		{
			this.gameObject.SetActive(true);
			switch( Type )
			{
				case BacklogType.FullScreenText:
					InitialzeAsFullScreenText();
					break;
				case BacklogType.MessageWindow:
				default:
					InitialzeAsMessageWindow();
					break;
			}
		}

		protected virtual void InitialzeAsMessageWindow()			
		{
			ListView.CreateItems(BacklogManager.Backlogs.Count, CallbackCreateItem);
		}

		protected virtual void InitialzeAsFullScreenText()
		{
			ListView.CreateItems(BacklogManager.Backlogs.Count, CallbackCreateItem);
		}

		/// <summary>
		/// リストビューのアイテムが作られたときに呼ばれるコールバック
		/// </summary>
		/// <param name="go">作られたアイテムのGameObject</param>
		/// <param name="index">アイテムのインデックス</param>
		protected virtual void CallbackCreateItem(GameObject go, int index)
		{
			AdvBacklog data = BacklogManager.Backlogs[BacklogManager.Backlogs.Count- index -1];
			AdvUguiBacklog backlog = go.GetComponent<AdvUguiBacklog>();
			backlog.Init(data);
		}

		// 戻るボタンが押された
		public void OnTapBack()
		{
			Back();
		}

		// 更新
		protected virtual void Update()
		{
			//閉じる入力された
			if (InputUtil.IsMouseRightButtonDown() || IsInputBottomEndScrollWheelDown() )
			{
				Back();
			}
		}

		// バックログ閉じて、メッセージウィンドウ開く
		protected virtual void Back()
		{
			this.Close();
			engine.UiManager.Status = AdvUiManager.UiStatus.Default;
		}

		//スクロール最下段でマウスホイール入力で閉じる入力するチェック
		protected virtual bool IsInputBottomEndScrollWheelDown()
		{
			if(isCloseScrollWheelDown && InputUtil.IsInputScrollWheelDown())
			{
				Scrollbar scrollBar = ListView.ScrollRect.verticalScrollbar;
				if(scrollBar)
				{
					return scrollBar.value <= 0;
				}
			}
			return false;
		}
	}
}
