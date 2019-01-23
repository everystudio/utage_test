// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if false
namespace Utage
{
	/// <summary>
	/// StringGridから作成するKeyValueデータ
	/// </summary>
	public abstract class StringGridContainerKeyValue
	{
		/// <summary>
		/// キー
		/// </summary>
		public string Key { get { return key; } }
		string key;

		/// <summary>
		/// キーの初期化
		/// </summary>
		/// <param name="key"></param>
		internal void InitKey(string key) { this.key = key; }

		/// <summary>
		/// 文字列グリッドの行データから、データを初期化
		/// </summary>
		/// <param name="row">初期化するための文字列グリッドの行データ</param>
		/// <returns>成否。空のデータの場合などはfalseが帰る</returns>
		internal bool InitFromStringGridRowMain(StringGridRow row)
		{
			this.row = row;
			return InitFromStringGridRow(row);
		}

		/// <summary>
		/// 文字列グリッドの行データから、データを初期化
		/// </summary>
		/// <param name="row">初期化するための文字列グリッドの行データ</param>
		/// <returns>成否。空のデータの場合などはfalseが帰る</returns>
		public abstract bool InitFromStringGridRow(StringGridRow row);

		//元となる行データ
		public StringGridRow Row { get { return row; } }
		StringGridRow row;
	}

	/// <summary>
	/// StringGridを複数もって、それを連結してキー・バリューなデータを作るための基底クラス
	/// </summary>
	[System.Serializable]
	public class StringGridContainer<T>
		where T : StringGridContainerKeyValue, new()
	{
		protected List<StringGrid> StringGridList{ get { return stringGridList ?? ( stringGridList = new List<StringGrid>()); }}
		[SerializeField]
		List<StringGrid> stringGridList;

		public Dictionary<string, T> Dictionary { get { return dictionary; } }
		Dictionary<string, T> dictionary = new Dictionary<string, T>();

		public List<T> List { get { return list; } }
		List<T> list = new List<T>();

		/// <summary>
		/// ロードが終了したか
		/// </summary>
		public bool IsLoadEnd { get { return this.isLoadEnd; } }
		bool isLoadEnd;

		public virtual void Clear()
		{
			StringGridList.Clear();
			Dictionary.Clear();
			List.Clear();
		}

		protected virtual void Add(T val)
		{
			if (dictionary.ContainsKey(val.Key))
			{
				Debug.LogError("<color=red>" + val.Key + "</color>" + "  is already contains");
			}
			dictionary.Add(val.Key, val);
			List.Add(val);
		}

		public virtual bool TryGetValue(string key, out T value)
		{
			return Dictionary.TryGetValue(key,out value);
		}

		public virtual bool ContainsKey(string key)
		{
			return Dictionary.ContainsKey(key);
		}
		

		/// <summary>
		/// 起動時に文字列グリッドから、データ初期化
		/// </summary>
		public void BootInit()
		{
			Dictionary.Clear();
			List.Clear();
			ParseBegin();
			foreach( var grid in stringGridList  )
			{
				grid.InitLink();
				ParseFromStringGrid(grid);
			}
			ParseEnd();
		}

		/// <summary>
		/// 文字列グリッドを追加
		/// </summary>
		public virtual void AddGrid(StringGrid grid)
		{
			StringGridList.Add(grid);
		}

		/// <summary>
		/// 文字列グリッドから、データ解析
		/// </summary>
		/// <param name="grid"></param>
		protected virtual void ParseFromStringGrid(StringGrid grid)
		{
			T last = null;
			foreach (StringGridRow row in grid.Rows)
			{
				if (row.RowIndex < grid.DataTopRow) continue;			//データの行じゃない
				if (row.IsEmptyOrCommantOut) continue;								//データがない

				if (!TryParseContinues(last, row))
				{
					T data = ParseFromStringGridRow(last, row);
					if (data != null) last = data;
				}
			}
		}

		//連続するデータとして追加できる場合はする。基本はしない
		protected virtual bool TryParseContinues(T last, StringGridRow row)
		{
			if (last == null) return false;
			return false;
		}

		//連続するデータとして追加できる場合はする。基本はしない
		protected virtual T ParseFromStringGridRow(T last, StringGridRow row)
		{
			T data = new T();
			if (data.InitFromStringGridRowMain(row))
			{
				if (!Dictionary.ContainsKey(data.Key))
				{
					Add(data);
					return data;
				}
				else
				{
					string errorMsg = "";
					errorMsg += row.ToErrorString(ColorUtil.AddColorTag(data.Key, Color.red) + "  is already contains");
					Debug.LogError(errorMsg);
				}
			}
			return null;
		}

		/// <summary>
		/// 解析の前処理
		/// </summary>
		protected virtual void ParseBegin() { }


		/// <summary>
		/// 解析の後処理
		/// </summary>
		protected virtual void ParseEnd() { }


		/// <summary>
		/// CSV設定ファイルをロードして、データ作成
		/// </summary>
		/// <param name="filePathInfoList">ロードするパスのリスト</param>
		/// <returns></returns>
		public virtual IEnumerator LoadCsvAsync(List<AssetFilePathInfo> filePathInfoList)
		{
			isLoadEnd = false;
			Clear();
			ParseBegin();

			List<AssetFile> fileList = new List<AssetFile>();

			foreach (AssetFilePathInfo filePathInfo in filePathInfoList)
			{
				fileList.Add(AssetFileManager.Load(filePathInfo.Path, filePathInfo.Version, this));
			}
			foreach (AssetFile file in fileList)
			{
				while (!file.IsLoadEnd) yield return null;
				if (!file.IsLoadError)
				{
					StringGridList.Add(file.Csv);
				}
				file.Unuse(this);
			}

			ParseEnd();
			isLoadEnd = true;
		}
	}
}
#endif
