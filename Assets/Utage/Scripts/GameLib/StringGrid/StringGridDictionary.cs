// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.Collections.Generic;
using UnityEngine;

namespace Utage
{
	/// <summary>
	/// StringGridのキーバリュー
	/// </summary>
	[System.Serializable]
	public partial class StringGridDictionaryKeyValue : SerializableDictionaryKeyValue
	{
		/// <summary>
		/// 名前（キーと同じ）
		/// </summary>
		public string Name { get { return Key; } }

		/// <summary>
		/// シナリオを記述したStringGrid
		/// </summary>
		public StringGrid Grid { get { return grid; } }
		[SerializeField]
		StringGrid grid;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="key">キー</param>
		/// <param name="value">値</param>
		public StringGridDictionaryKeyValue(string key, StringGrid grid)
		{
			InitKey(key);
			this.grid = grid;
		}
	}

	/// <summary>
	/// StringGridのDictionary
	/// </summary>
	[System.Serializable]
	public partial class StringGridDictionary : SerializableDictionary<StringGridDictionaryKeyValue>
	{
		/// <summary>
		/// 要素（キーバリュー）の追加
		/// </summary>
		/// <param name="key">キー</param>
		/// <param name="value">値</param>
		public void Add(string key, StringGrid value)
		{
			Add(new StringGridDictionaryKeyValue(key, value));
		}
	
		/// <summary>
		/// 特定の名前のシートを消す
		/// </summary>
		public void RemoveSheets(string pattern)
		{
			List<string> removeList = new List<string>();
			foreach (string key in Keys)
			{
				if (System.Text.RegularExpressions.Regex.IsMatch(key, pattern))
				{
					removeList.Add(key);
				}
			}
			foreach (string key in removeList)
			{
				Remove(key);
			}
		}

	}
}
