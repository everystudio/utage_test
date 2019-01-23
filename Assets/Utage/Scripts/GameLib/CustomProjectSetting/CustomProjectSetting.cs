// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utage
{

	/// <summary>
	/// 表示言語切り替え用のクラス
	/// </summary>
	public class CustomProjectSetting : ScriptableObject
	{
		static CustomProjectSetting instance;
		/// <summary>
		/// シングルトンなインスタンスの取得
		/// </summary>
		/// <returns></returns>
		public static CustomProjectSetting Instance
		{
			get
			{
				if (instance == null)
				{
					BootCustomProjectSetting boot = FindObjectOfType<BootCustomProjectSetting>();
					if (boot != null)
					{
						instance = boot.CustomProjectSetting;
						if (instance == null)
						{
							Debug.LogError("CustomProjectSetting is NONE", boot);
						}
					}
				}
				return instance;
			}
		}
		
		/// <summary>
		/// 設定言語
		/// </summary>
		public LanguageManager Language
		{
			get { return language; }
			set { language = value; }
		}
		[SerializeField]
		LanguageManager language;
	}
}
