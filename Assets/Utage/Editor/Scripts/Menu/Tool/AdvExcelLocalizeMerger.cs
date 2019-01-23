// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace Utage
{

	//「Utage」のシナリオデータ用のエクセルファイル解析して、ローカライズ用のエクセルファイルを作成する
	public class AdvExcelLocalizeMerger : CustomEditorWindow
	{
		/// <summary>
		/// マージ先のパス
		/// </summary>
		[SerializeField, PathDialog(PathDialogAttribute.DialogType.File)]
		string pathBase = "";

		/// <summary>
		/// 言語ファイルのパス
		/// </summary>
		[SerializeField, PathDialog(PathDialogAttribute.DialogType.File)]
		string pathLocalized = "";

		const string TextKey = "Text";

		protected override bool DrawProperties()
		{
			bool ret = base.DrawProperties();

			EditorGUI.BeginDisabledGroup(AdvScenarioDataBuilderWindow.ProjectData == null);
			{
				if( GUILayout.Button("Merge", GUILayout.Width(180)) )
				{
					MergeLanguage(TextKey, pathBase, pathLocalized);
				}

			}
			EditorGUI.EndDisabledGroup();
			GUILayout.Space(8f);
			return ret; 
		}

		//言語用ファイルをマージする
		void MergeLanguage(string textKey, string pathBase, string pathLocalized )
		{
			IWorkbook book = ExcelParser.ReadBook(pathBase);
			IWorkbook bookLocalized = ExcelParser.ReadBook(pathLocalized);

			for (int i = 0; i < bookLocalized.NumberOfSheets; ++i)
			{
				ISheet sheetLocalized = bookLocalized.GetSheetAt(i);
				ISheet sheet = book.GetSheet(sheetLocalized.SheetName);
				if(sheet==null)
				{
					Debug.LogError( sheet.SheetName + " is not found in " + pathBase );
					continue;
				}

				List<int> textColumnIndexList = new List<int>();
				IRow rowLocalized = sheetLocalized.GetRow(sheetLocalized.FirstRowNum);
				for (int cellIndex = 0; cellIndex < rowLocalized.Cells.Count; ++cellIndex)
				{
					ICell cell = rowLocalized.Cells[cellIndex];
					string key = ( cellIndex==0 ) ? textKey : cell.StringCellValue.Replace("[[[","").Replace("]]]","");
					int index = 0;
					if (!ExcelParser.TryGetColumneIndex(sheet, key, out index))
					{
						IRow row = sheet.GetRow(sheet.FirstRowNum);
						index = row.Cells.Count;
						row.CreateCell(index).SetCellValue(key);
					}
					textColumnIndexList.Add(index);
				}
				MergeLanguage(sheet, sheetLocalized, textColumnIndexList);
			}
			ExcelParser.WriteBook(book, pathBase);
		}

		//言語用ファイルをマージする
		void MergeLanguage(ISheet sheet, ISheet sheetLocalized, List<int> indexList)
		{
			//シートの読み込み
			for (int rowIndex = sheetLocalized.FirstRowNum+1; rowIndex <= sheetLocalized.LastRowNum; ++rowIndex)
			{
				IRow rowLocalized = sheetLocalized.GetRow(rowIndex);
				if (rowLocalized==null) continue;
				string text = rowLocalized.Cells[0].StringCellValue;
				if (string.IsNullOrEmpty(text)) continue;

				IRow row = sheet.GetRow(rowIndex);
				if (row == null)
				{
					Debug.LogError("line:" + rowIndex + " is not found in" + sheet.SheetName);
					continue;
				}

				int textIndex = indexList[0];
				if (textIndex > row.LastCellNum || row.GetCell(textIndex).StringCellValue != text)
				{
					Debug.LogError(string.Format("Text [ {0} ] is not equal in {1} :line {2}", text, sheet.SheetName, rowIndex));
					continue;
				}

				for (int i = 1; i < rowLocalized.Cells.Count;++i )
				{
					int index = indexList[i];
					ICell cell = row.GetCell(index, MissingCellPolicy.CREATE_NULL_AS_BLANK);
					cell.SetCellValue(rowLocalized.Cells[i].StringCellValue);
//					string str = rowLocalized.Cells[i].StringCellValue;
//					text = System.Text.Encoding.Unicode.GetString(text.ToCharArray());
//					cell.SetCellValue(str);
				}
			}
		}
	}
}