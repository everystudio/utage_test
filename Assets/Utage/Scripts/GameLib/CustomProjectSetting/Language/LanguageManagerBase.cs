// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utage
{
	public enum LanguageName
	{
		English,
		Japanese,
	}

	/// <summary>
	/// 表示言語切り替え用のクラス
	/// </summary>
	public abstract class LanguageManagerBase : ScriptableObject
	{
		static LanguageManagerBase instance;
		/// <summary>
		/// シングルトンなインスタンスの取得
		/// </summary>
		/// <returns></returns>
		public static LanguageManagerBase Instance
		{
			get
			{
				if (instance == null)
				{
					if (CustomProjectSetting.Instance)
					{
						instance = CustomProjectSetting.Instance.Language;
					}
					if (instance != null)
					{
						instance.Init();
					}
				}
				return instance;
			}
		}

		//言語がオート設定のときは、システム環境に依存する

		const string Auto = "Auto";
		/// <summary>
		/// 設定言語
		/// </summary>
		public string Language{
			get { return language; }
		}
		[SerializeField]
		protected string language = Auto;

		//デフォルト言語
		public string DefaultLanguage { get { return defaultLanguage; } }
		[SerializeField]
		protected string defaultLanguage = "Japanese";

		//データの言語指定
		public string DataLanguage { get { return dataLanguage; } }
		[SerializeField]
		protected string dataLanguage = "";

		//翻訳テキストのデータ
		[SerializeField]
		List<TextAsset> languageData = new List<TextAsset>();


		//UIのテキストローカライズを無視する
		public bool IgnoreLocalizeUiText { get { return ignoreLocalizeUiText; } }
		[SerializeField]
		bool ignoreLocalizeUiText = false;

		//ボイスのローカライズを無視する
		public bool IgnoreLocalizeVoice { get { return ignoreLocalizeVoice; } }
		[SerializeField]
		bool ignoreLocalizeVoice = true;
		
		//ボイスの対応言語
		public List<string> VoiceLanguages { get { return voiceLanguages; } }
		[SerializeField]
		List<string> voiceLanguages = new List<string>();

		//言語切り替えで呼ばれるコールバック
		public Action OnChangeLanugage {
			get;
			set;
		}
		

		/// <summary>
		/// 現在の設定言語
		/// </summary>
		public string CurrentLanguage
		{
			get
			{
				return currentLanguage;
			}
			set
			{
				if (currentLanguage != value)
				{
					currentLanguage = value;
					RefreshCurrentLanguage();
				}
			}
		}
		string currentLanguage;


		LanguageData Data { get; set; }

		//現在設定されている言語名のリスト
		public List<string> Languages { get { return Data.Languages; } }


		void OnEnable()
		{
			Init();
		}

		//初期化処理
		void Init()
		{
			Data = new LanguageData();
			foreach (var item in languageData)
			{
				if (item == null) continue;

				Data.OverwriteData(item);
			}

			//設定された言語か、システムの言語に変更
			currentLanguage = (string.IsNullOrEmpty(language) || language == Auto) ? Application.systemLanguage.ToString() : language;
			RefreshCurrentLanguage();
		}

		//現在の言語が変わったときの処理
		protected void RefreshCurrentLanguage()
		{
			if (Instance != this) return;

			if (OnChangeLanugage != null)
				OnChangeLanugage();
			OnRefreshCurrentLanguage();
		}
		//現在の言語が変わったときの処理
		protected abstract void OnRefreshCurrentLanguage();

		/// <summary>
		/// 指定のキーのテキストを、指定のデータの、設定された言語に翻訳して取得
		/// </summary>
		/// <param name="dataName">データ名</param>
		/// <param name="key">テキストのキー</param>
		/// <returns>翻訳したテキスト</returns>
		public string LocalizeText(string dataName, string key)
		{
			if (Data.ContainsKey(key))
			{
				string text;
				if (Data.TryLocalizeText(out text, CurrentLanguage, DefaultLanguage, key, dataName))
				{
					return text;
				}
			}

			Debug.LogError(key + " is not found in " + dataName);
			return key;
		}

		/// <summary>
		/// 指定のキーのテキストを、全データ内から検索して、設定された言語に翻訳して取得
		/// </summary>
		/// <param name="key">テキストのキー</param>
		/// <returns>翻訳したテキスト</returns>
		public string LocalizeText(string key)
		{
			string text = key;
			TryLocalizeText(key, out text);
			return text;
		}

		/// <summary>
		/// 指定のキーのテキストを、全データ内から検索して、設定された言語に翻訳して取得
		/// </summary>
		/// <param name="key">テキストのキー</param>
		/// <returns>翻訳したテキスト</returns>
		public bool TryLocalizeText(string key, out string text )
		{
			text = key;
			if (Data.ContainsKey(key))
			{
				if (Data.TryLocalizeText(out text, CurrentLanguage, DefaultLanguage, key))
				{
					return true;
				}
			}
			return false;
		}

		internal void OverwriteData(StringGrid grid)
		{
			Data.OverwriteData(grid);
			RefreshCurrentLanguage();
		}
	}
}
