// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Utage
{

	/// <summary>
	/// 表示言語切り替え用のクラス
	/// </summary>
	[AddComponentMenu("Utage/Lib/UI/Localize/TextSetting")]
	public class UguiLocalizeTextSetting : UguiLocalizeBase
	{
		[Serializable]
		public class Setting
		{
			public string language;
			public Font font;
			public UguiNovelTextSettings setting;
			public int fontSize = 20;
			public float lineSpacing = 1;
		};

		[SerializeField]
		List<Setting> settingList = new List<Setting>();

		[NonSerialized]
		Setting defaultSetting = null;

		/// <summary>
		/// スプライトコンポーネント(アタッチされてない場合はnull)
		/// </summary>
		Text CachedText { get { if (null == cachedText) cachedText = this.GetComponent<Text>(); return cachedText; } }
		Text cachedText;

		/// <summary>
		/// ノベルテキスト
		/// </summary>
		UguiNovelText NovelText { get { if (null == novelText) novelText = this.GetComponent<UguiNovelText>(); return novelText; } }
		UguiNovelText novelText;


		protected override void RefreshSub()
		{
			Text text = CachedText;
			if (text != null)
			{
				Setting setting = settingList.Find(x => x.language == currentLanguage);
				if (setting == null)
				{
					setting = defaultSetting;
				}
				if (setting == null) return;

				CachedText.font = (setting.font != null) ? setting.font : defaultSetting.font;
				CachedText.fontSize = setting.fontSize;
				CachedText.lineSpacing = setting.lineSpacing;

				if (NovelText != null && setting.setting!=null)
				{
					NovelText.TextGenerator.TextSettings = setting.setting;
				}
			}
		}

		protected override void InitDefault()
		{
			defaultSetting = new Setting();
			defaultSetting.font = CachedText.font;
			defaultSetting.fontSize = CachedText.fontSize;
			defaultSetting.lineSpacing = CachedText.lineSpacing;
			if (NovelText != null)
			{
				defaultSetting.setting = NovelText.TextGenerator.TextSettings;
			}
		}
		public override void ResetDefault()
		{
			CachedText.font = defaultSetting.font;
			CachedText.fontSize = defaultSetting.fontSize;
			CachedText.lineSpacing = defaultSetting.lineSpacing;
			if (NovelText != null)
			{
				NovelText.TextGenerator.TextSettings = defaultSetting.setting;
			}
		}
	}
}

