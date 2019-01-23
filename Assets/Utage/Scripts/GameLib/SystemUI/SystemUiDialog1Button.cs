// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

namespace Utage
{

	/// <summary>
	/// ボタン一つのダイアログ
	/// </summary>
	[AddComponentMenu("Utage/Lib/System UI/Dialog1Button")]
	public class SystemUiDialog1Button : MonoBehaviour
	{

		/// <summary>
		/// 本文表示用のテキスト
		/// </summary>
		[SerializeField]
		protected Text titleText;

		/// <summary>
		/// ボタン1用のテキスト
		/// </summary>
		[SerializeField]
		protected Text button1Text;

		/// <summary>
		/// ボタン1を押したときのイベント
		/// </summary>
		[SerializeField]
		protected UnityEvent OnClickButton1;

		/// <summary>
		/// ダイアログを開く
		/// </summary>
		/// <param name="text">表示テキスト</param>
		/// <param name="buttonText1">ボタン1のテキスト</param>
		/// <param name="target">ボタンを押したときの呼ばれるコールバック</param>
		public void Open(string text, string buttonText1, UnityAction callbackOnClickButton1)
		{
			titleText.text = text;
			button1Text.text = buttonText1;
			this.OnClickButton1.RemoveAllListeners();
			this.OnClickButton1.AddListener(callbackOnClickButton1);
			Open();
		}

		/// <summary>
		/// ボタン1が押された時の処理
		/// </summary>
		public void OnClickButton1Sub()
		{
			OnClickButton1.Invoke();
			Close();
		}

		/// <summary>
		/// オープン
		/// </summary>
		public void Open()
		{
			this.gameObject.SetActive(true);
		}

		/// <summary>
		/// クローズ
		/// </summary>
		public void Close()
		{
			this.gameObject.SetActive(false);
		}
	}
}