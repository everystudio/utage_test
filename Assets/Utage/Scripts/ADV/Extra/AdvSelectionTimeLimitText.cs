// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.UI;
using Utage;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 時間制限のタイマー
/// </summary>
namespace Utage
{
	[AddComponentMenu("Utage/ADV/Extra/SelectionTimeLimitText")]
	public class AdvSelectionTimeLimitText : MonoBehaviour
	{

		/// <summary>表示のルート</summary>
		public GameObject TargetRoot { get { return targetRoot ?? (this.targetRoot = this.gameObject); } }
		[SerializeField]
		protected GameObject targetRoot;
		
		/// <summary>数字を表示するテキスト</summary>
		public Text Target { get { return this.text ?? (this.text =TargetRoot.GetComponentInChildren<Text>()); } }
		[SerializeField]
		protected Text text;

		protected AdvSelectionTimeLimit timeLimit;

		/// <summary>ADVエンジン</summary>
		public AdvEngine Engine { get { return this.engine ?? (this.engine = FindObjectOfType<AdvEngine>()); } }
		[SerializeField]
		protected AdvEngine engine;

		void Awake()
		{
			Engine.SelectionManager.OnBeginWaitInput.AddListener(OnBeginWaitInput);
			Engine.SelectionManager.OnUpdateWaitInput.AddListener(OnUpdateWaitInput);
			Engine.SelectionManager.OnSelected.AddListener(OnSelected);
			Engine.SelectionManager.OnClear.AddListener(OnClear);
			TargetRoot.SetActive(false);
		}

		void OnBeginWaitInput(AdvSelectionManager selection)
		{
			timeLimit = FindObjectOfType<AdvSelectionTimeLimit>();
			if (timeLimit != null)
			{
				TargetRoot.SetActive(true);
			}
		}

		void OnUpdateWaitInput(AdvSelectionManager selection)
		{
			if (TargetRoot.activeSelf && timeLimit != null)
			{
				Target.text = "" + Mathf.CeilToInt(timeLimit.limitTime - timeLimit.TimeCount);
			}
		}

		void OnSelected(AdvSelectionManager selection)
		{
			TargetRoot.SetActive(false);
		}
		void OnClear(AdvSelectionManager selection)
		{
			TargetRoot.SetActive(false);
		}
	}
}
