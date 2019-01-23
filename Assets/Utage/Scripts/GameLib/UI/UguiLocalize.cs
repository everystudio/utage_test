// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Utage
{

	/// <summary>
	/// テキストの表示言語切り替え用のクラス
	/// </summary>
	[ExecuteInEditMode]
	[AddComponentMenu("Utage/Lib/UI/Localize")]
	public class UguiLocalize : UguiLocalizeBase
	{
		public string Key
		{
			set { key = value; ForceRefresh(); }
			get { return key; }
		}
		[SerializeField]
		string key;

		[NonSerialized]
		string defaultText;

		/// <summary>
		/// スプライトコンポーネント(アタッチされてない場合はnull)
		/// </summary>
		Text CachedText { get { if (null == cachedText) cachedText = this.GetComponent<Text>(); return cachedText; } }
		Text cachedText;

		protected override void RefreshSub()
		{
			Text text = CachedText;
			if (text != null && !LanguageManagerBase.Instance.IgnoreLocalizeUiText )
			{
				string str;
				if (LanguageManagerBase.Instance.TryLocalizeText(key, out str))
				{
					text.text = str;
				}
				else
				{
					Debug.LogError(key + " is not found in localize key" , this);
				}
			}
		}

		protected override void InitDefault()
		{
			Text text = CachedText;
			if (text != null)
			{
				defaultText = text.text;
			}
		}
		public override void ResetDefault()
		{
			Text text = CachedText;
			if (text != null)
			{
				text.text = defaultText;
			}
		}
	}
}

