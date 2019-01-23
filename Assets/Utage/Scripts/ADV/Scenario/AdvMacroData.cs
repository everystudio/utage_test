// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtageExtensions;


namespace Utage
{

	/// <summary>
	/// マクロのデータ
	/// </summary>
	public class AdvMacroData
	{
		//マクロ名
		public string Name { get; private set; }
		//マクロのヘッダ部分（マクロ名の行と同じで、引数が入る）
		public StringGridRow Header { get; private set; }
		//マクロ部分のデータ
		public List<StringGridRow> DataList { get; private set; }
		public AdvMacroData(string name, StringGridRow header, List<StringGridRow> dataList)
		{
			this.Name = name;
			this.Header = header;
			this.DataList = dataList;
		}

		//指定の行をマクロ展開
		public List<StringGridRow> MacroExpansion(StringGridRow args, string debugMsg)
		{
			//マクロ展開後の行リスト
			List<StringGridRow> list = new List<StringGridRow>();
			if(DataList.Count<=0) return list;
/*
			//マクロシート
			StringGrid macroSheet = DataList[0].Grid;
			string sheetName = args.Grid.Name + ":" + (args.RowIndex+1).ToString() + "-> Macro : " + macroSheet.Name;
			StringGrid grid = new StringGrid(sheetName, args.Grid.SheetName, macroSheet.Type);
			grid.Macro = new StringGrid.MacroInfo(args);
			grid.ColumnIndexTbl = macroSheet.ColumnIndexTbl;
*/

			/*
						//マクロ用の情報
					internal class MacroInfo
					{
						StringGridRow args;
						internal MacroInfo(StringGridRow args)
						{
							this.args = args;
						}

						internal string ToDebugString()
						{
							if (args.Grid.Macro != null)
							{
								return args.Grid.Macro.ToDebugString();
							}
							else
							{
								string sheetName = args.Grid.SheetName;
								return sheetName + ":" + (args.RowIndex + 1) + " ";
							}
						}

					};
					internal MacroInfo Macro { get; set; }
			*/
			for (int i = 0; i < DataList.Count; ++i)
			{
				StringGridRow data = DataList[i];
				//展開先の列数と同じ数のセル（文字列の配列）をもつ
				string[] strings = new string[args.Grid.ColumnIndexTbl.Count];
				foreach (var keyValue in args.Grid.ColumnIndexTbl)
				{
					string argKey = keyValue.Key;
					int argIndex = keyValue.Value;
					strings[argIndex] = ParaseMacroArg(data.ParseCellOptional<string>(argKey, ""), args);
				}
				//展開先のシートの構造に合わせる
				//展開先シートを親Girdに持ち
				StringGridRow macroData = new StringGridRow(args.Grid, args.RowIndex);
				macroData.InitFromStringArray(strings);
				list.Add(macroData);

				//デバッグ情報の記録
				macroData.DebugInfo = debugMsg + " : " + (data.RowIndex + 1) + " ";
			}
			return list;
		}

		//マクロ引数展開
		string ParaseMacroArg(string str, StringGridRow args)
		{
			int index = 0;
			string macroText = "";
			while (index < str.Length)
			{
				bool isFind = false;
				if (str[index] == '%')
				{
					foreach (string key in Header.Grid.ColumnIndexTbl.Keys)
					{
						if (key.Length <= 0) continue;
						for (int i = 0; i < key.Length; ++i)
						{
							if (key[i] != str[index + 1 + i])
							{
								break;
							}
							else if (i == key.Length - 1)
							{
								isFind = true;
							}
						}
						if (isFind)
						{
							string def = Header.ParseCellOptional<string>(key, "");
							macroText += args.ParseCellOptional<string>(key, def);
							index += key.Length;
							break;
						}
					}
				}
				if (!isFind)
				{
					macroText += str[index];
				}
				++index;
			}
			return macroText;
		}

		/*
		string ParaseMacroArg(string str, string argKey, string argValue)
		{
			if (str.Length <= 1) return str;

			int index = 0;
			string macroText = "";
			while (index < str.Length)
			{
				bool isFind = false;
				//%で区切られたデータがあったら、それはマクロ引数を展開する
				if (str[index] == '%')
				{
					foreach (var keyValue in Header.Grid.ColumnIndexTbl)
					{
						if (keyValue.Key.Length <= 0) continue;
						if (str.Length - (index +1) < keyValue.Key.Length) continue;

						//マクロシートのほうの項目名と、%以下のテキストが一致するかチェック
						for (int i = 0; i < keyValue.Key.Length; ++i)
						{
							if (keyValue.Key[i] != str[index + 1 + i])
							{
								continue;
							}
							if (i== keyValue.Key.Length-1)
							{
								isFind = true;
							}
						}

						//項目名と一致したので、引数を展開
						if (isFind)
						{
							//引数ナシならデフォルト値を取得
							macroText += string.IsNullOrEmpty(argValue) ? Header.ParseCellOptional<string>(keyValue.Key, "") : argValue;
							index += keyValue.Key.Length;
							break;
						}
					}
				}
				if (!isFind)
				{
					macroText += str[index];
				}
				++index;
			}
			return macroText;
		}
*/


	}
}