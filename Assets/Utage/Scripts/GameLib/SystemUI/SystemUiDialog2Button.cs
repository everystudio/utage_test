// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

namespace Utage
{
	/// <summary>
	/// ボタン二つのダイアログ
	/// </summary>
	[AddComponentMenu("Utage/Lib/System UI/Dialog2Button")]
	public class SystemUiDialog2Button : SystemUiDialog1Button
	{

		/// <summary>
		/// ボタン2用のテキストエリア
		/// </summary>
		[SerializeField]
		protected Text button2Text;

		/// <summary>
		/// ボタン2を押したときのイベント
		/// </summary>
		[SerializeField]
		protected UnityEvent OnClickButton2;

		/// <summary>
		/// 二ボタンダイアログをダイアログを起動
		/// </summary>
		/// <param name="text">表示テキスト</param>
		/// <param name="buttonText1">ボタン1用のテキスト</param>
		/// <param name="buttonText2">ボタン2用のテキスト</param>
		/// <param name="callbackOnClickButton1">ボタン1を押したときの呼ばれるコールバック</param>
		/// <param name="callbackOnClickButton2">ボタン2を押したときの呼ばれるコールバック</param>
		public void Open(string text, string buttonText1, string buttonText2, UnityAction callbackOnClickButton1, UnityAction callbackOnClickButton2 )
		{
			button2Text.text = buttonText2;
			this.OnClickButton2.RemoveAllListeners();
			this.OnClickButton2.AddListener(callbackOnClickButton2);
			base.Open(text, buttonText1, callbackOnClickButton1 );
		}

		/// <summary>
		/// ボタン2が押された
		/// </summary>
		public void OnClickButton2Sub()
		{
			OnClickButton2.Invoke();
			Close();
		}
	}

}