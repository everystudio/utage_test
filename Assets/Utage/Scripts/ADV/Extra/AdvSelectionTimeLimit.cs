// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using Utage;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 時間制限のタイマー
/// </summary>
namespace Utage
{
	[AddComponentMenu("Utage/ADV/Extra/SelectionTimeLimit")]
	public class AdvSelectionTimeLimit : MonoBehaviour
	{
		//無効化フラグ
		[SerializeField]
		bool disable = false;
		public bool Disable
		{
			get { return disable; }
			set { disable = value; }
		}

		/// <summary>ADVエンジン</summary>
		public AdvEngine Engine { get { return this.engine ?? (this.engine = FindObjectOfType<AdvEngine>()); } }
		[SerializeField]
		protected AdvEngine engine;

		public AdvUguiSelection Selection { get { return this.selection ?? (this.selection = GetComponent<AdvUguiSelection>()); } }
		[SerializeField]
		protected AdvUguiSelection selection;

		public float limitTime = 10.0f;
		public int timeLimitIndex = -1;

		public float TimeCount { get { return time; } }
		float time;

		void Awake()
		{
			Engine.SelectionManager.OnBeginWaitInput.AddListener(OnBeginWaitInput);
			Engine.SelectionManager.OnUpdateWaitInput.AddListener(OnUpdateWaitInput);
		}

		void OnBeginWaitInput(AdvSelectionManager selection)
		{
			time = -Time.deltaTime;
		}

		void OnUpdateWaitInput(AdvSelectionManager selection)
		{
			time += Time.deltaTime;
			if (time >= limitTime)
			{
				if (Engine.SelectionManager.IsWaitInput)
				{
					if (timeLimitIndex < 0)
					{
						if (Selection != null)
						{
							selection.Select(Selection.Data);
						}
					}
					else
					{
						selection.Select(timeLimitIndex);
					}
				}
			}
		}
	}
}
