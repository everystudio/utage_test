// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// ADVデータ解析
	/// </summary>
	public class AdvParser
	{
		public static string Localize(AdvColumnName name)
		{
			//多言語化をしてみたけど、複雑になってかえって使いづらそうなのでやめた
			return name.QuickToString();
		}

		//指定の名前のセルを、型Tとして解析・取得（データがなかったらエラーメッセージを出す）
		public static T ParseCell<T>(StringGridRow row, AdvColumnName name)
		{
			return row.ParseCell<T>(Localize(name));
		}

		//指定の名前のセルを、型Tとして解析・取得（データがなかったらデフォルト値を返す）
		public static T ParseCellOptional<T>(StringGridRow row, AdvColumnName name, T defaultVal)
		{
			return row.ParseCellOptional<T>(Localize(name), defaultVal);
		}

		//指定の名前のセルを、型Tとして解析・取得（データがなかったらfalse）
		public static bool TryParseCell<T>(StringGridRow row, AdvColumnName name, out T val)
		{
			return row.TryParseCell<T>(Localize(name), out val);
		}

		//セルが空かどうか
		public static bool IsEmptyCell(StringGridRow row, AdvColumnName name)
		{
			return row.IsEmptyCell(Localize(name));
		}

		//現在の設定言語にローカライズされたテキストを取得
		public static string ParseCellLocalizedText(StringGridRow row, AdvColumnName defaultColumnName)
		{
			return ParseCellLocalizedText(row, defaultColumnName.QuickToString());
		}

		//現在の設定言語にローカライズされたテキストを取得
		public static string ParseCellLocalizedText(StringGridRow row, string defaultColumnName)
		{
			string columnName = defaultColumnName;
			if (LanguageManager.Instance != null)
			{
				string currentLanguage = LanguageManager.Instance.CurrentLanguage;
				if (row.Grid.ContainsColumn(currentLanguage))
				{
					//現在の言語があるなら、その列を
					columnName = currentLanguage;
				}
				else
				{
					//デフォルトデータの言語指定がある場合、
					//現在の言語とデフォルトデータの言語が違う場合、
					string dataLanguage = LanguageManager.Instance.DataLanguage;
					if (!string.IsNullOrEmpty(dataLanguage))
					{
						if (currentLanguage == dataLanguage)
						{
							columnName = defaultColumnName;
						}
						else
						{
							columnName = LanguageManager.Instance.DefaultLanguage;
						}
					}
				}
			}
			if (row.IsEmptyCell(columnName))
			{   //指定の言語が空なら、デフォルトのText列を
				return row.ParseCellOptional<string>(defaultColumnName, "");
			}
			else
			{   //指定の言語を
				return row.ParseCellOptional<string>(columnName, "");
			}
		}
	}
}
