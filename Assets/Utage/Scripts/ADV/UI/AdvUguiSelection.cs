// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

namespace Utage
{
	/// <summary>
	/// 選択肢用UIのサンプル
	/// </summary>
	[AddComponentMenu("Utage/ADV/UiSelection")]
	public class AdvUguiSelection : MonoBehaviour
	{
		/// <summary>本文テキスト</summary>
		public Text text;


		/// <summary>選択肢データ</summary>
		public AdvSelection Data { get { return data; } }
		protected AdvSelection data;

		/// <summary>
		/// 初期化
		/// </summary>
		/// <param name="data">選択肢データ</param>
		public virtual void Init(AdvSelection data, Action<AdvUguiSelection> ButtonClickedEvent)
		{
			this.data = data;
			this.text.text = data.Text;

			UnityEngine.UI.Button button = this.GetComponent<UnityEngine.UI.Button> ();
			button.onClick.AddListener( ()=>ButtonClickedEvent(this) );
		}

		/// <summary>
		/// 選択済みの場合色を変える
		/// </summary>
		/// <param name="data">選択肢データ</param>
		public virtual void OnInitSelected( Color color )
		{
			this.text.color = color;
		}
	}
}
