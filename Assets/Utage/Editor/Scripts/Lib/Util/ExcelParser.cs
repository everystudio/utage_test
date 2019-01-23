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

	/// <summary>
	/// エクセルの解析用クラス
	/// </summary>
	public static class ExcelParser
	{
		public const string ExtXls = ".xls";
		public const string ExtXlsx = ".xlsx";

		//エクセルファイルか判定
		public static bool IsExcelFile(string path)
		{
			string ext = Path.GetExtension(path);
			return ((ext == ExtXls || ext == ExtXlsx) && File.Exists(path));
		}

		//ファイルの読み込み
		public static StringGridDictionary Read( string path, char ignoreSheetMark, bool parseFormula, bool parseNumreic)
		{
			UnityEngine.Profiling.Profiler.BeginSample("ReadExcel");
			StringGridDictionary gridDictionary = new StringGridDictionary();
			if ( IsExcelFile(path) )
			{
				string ext = Path.GetExtension(path);
				using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					if (ext == ExtXls)
					{
						ReadBook(new HSSFWorkbook(fs), path, ignoreSheetMark, parseFormula, parseNumreic, gridDictionary);
					}
					else if (ext == ExtXlsx)
					{
						ReadBook(new XSSFWorkbook(fs), path, ignoreSheetMark, parseFormula, parseNumreic, gridDictionary);
					}
				}
			}
			UnityEngine.Profiling.Profiler.EndSample();
			return gridDictionary;
		}

		//ブックの読み込み
		static void ReadBook(IWorkbook book, string path, char ignoreSheetMark, bool parseFormula, bool parseNumreic, StringGridDictionary gridDictionary)
		{
			for (int i = 0; i < book.NumberOfSheets; ++i)
			{
				UnityEngine.Profiling.Profiler.BeginSample("ReadBook");
				ISheet sheet = book.GetSheetAt(i);
				StringGrid grid = ReadSheet(sheet, path, ignoreSheetMark, parseFormula, parseNumreic);
				gridDictionary.Add(new StringGridDictionaryKeyValue(sheet.SheetName, grid));
				UnityEngine.Profiling.Profiler.EndSample();
			}
		}

		//シートの読み込み
		static StringGrid ReadSheet(ISheet sheet, string path, char ignoreSheetMark, bool parseFormula, bool parseNumreic)
		{
			int lastRowNum = sheet.LastRowNum;

			StringGrid grid = new StringGrid(path + ":" + sheet.SheetName, sheet.SheetName, CsvType.Tsv);
			if (sheet.SheetName.Length > 0 && sheet.SheetName[0] == ignoreSheetMark)
			{
				return grid;
			}

			for (int rowIndex = sheet.FirstRowNum; rowIndex <= lastRowNum; ++rowIndex)
			{
				IRow row = sheet.GetRow(rowIndex);

				List<string> stringList = new List<string>();
				if (row != null)
				{
					foreach (var cell in row.Cells)
					{
						for (int i = stringList.Count; i < cell.ColumnIndex; ++i)
						{
							stringList.Add("");
						}
						if (parseFormula)
						{
							try
							{
								switch (cell.CellType)
								{
									case CellType.Formula:
									case CellType.String:
										stringList.Add(cell.StringCellValue);
										break;
									case CellType.Numeric:
										if (parseNumreic)
										{
											stringList.Add(cell.NumericCellValue.ToString());
										}
										else
										{
											stringList.Add(cell.ToString());
										}
										break;
									default:
										stringList.Add(cell.ToString());
										break;
								}
							}
							catch (System.Exception e)
							{
								Debug.LogError(e.Message);
								stringList.Add(cell.ToString());
							}
						}
						else
						{
							if (parseNumreic && cell.CellType == CellType.Numeric)
							{
								try
								{
									stringList.Add(cell.NumericCellValue.ToString());
								}
								catch (System.Exception e)
								{
									Debug.LogError(e.Message);
									stringList.Add(cell.ToString());
								}
							}
							else
							{
								stringList.Add(cell.ToString());
							}
						}
					}
				}
				grid.AddRow(stringList);
			}
			grid.ParseHeader();
			return grid;
		}

		public static void Write (string path, StringGridDictionary gridDictionary)
		{
/*			string ext = Path.GetExtension (path);
			switch (ext)
			{
				case ExtXls:
					book = new HSSFWorkbook();
					break;
				case ExtXlsx:
					book = new XSSFWorkbook();
					break;
				default:
					break;
			}
*/
			path = FilePathUtil.ChangeExtension(path, ExtXls);
			IWorkbook book = MakeBook(gridDictionary);
			using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
			{
				book.Write(fs);
			}
		}

		public static IWorkbook MakeBook(StringGridDictionary gridDictionary)
		{
			IWorkbook book = new HSSFWorkbook();
			foreach( var item in gridDictionary.List )
			{
				StringGrid grid = item.Grid;
				ISheet sheet = book.CreateSheet(grid.SheetName);
				for( int i = 0; i <  grid.Rows.Count; ++i )
				{
					StringGridRow gridRow = grid.Rows[i];
					IRow row = sheet.CreateRow(i);
					for( int j = 0; j < gridRow.Strings.Length; ++j )
					{
						row.CreateCell(j).SetCellValue(gridRow.Strings[j]);
					}
				}
			}
			return book;
		}

		//言語用ファイルをマージする
		public static void MergeLanguage(string textKey, string pathBase, string pathLocalized )
		{
			Stream outStream;
			IWorkbook book = ReadBook(pathBase, out outStream);
			IWorkbook bookLocalized = ReadBook(pathLocalized);

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
					string localizeKey = cell.StringCellValue.Replace("##","");
					int index = 0;
					if (!TryGetColumneIndex(sheet, localizeKey, out index))
					{
						IRow row = sheet.GetRow(sheet.FirstRowNum);
						index = row.Cells.Count;
						row.CreateCell(index).SetCellValue(localizeKey);
					}
					textColumnIndexList.Add(index);
				}
				MergeLanguage(sheet, sheetLocalized, textColumnIndexList);
			}
			book.Write(outStream);
		}

		//言語用ファイルをマージする
		public static void MergeLanguage(ISheet sheet, ISheet sheetLocalized, List<int> textColumnIndexList)
		{
			//シートの読み込み
			for (int rowIndex = sheetLocalized.FirstRowNum+1; rowIndex <= sheetLocalized.LastRowNum; ++rowIndex)
			{
				IRow rowLocalized = sheetLocalized.GetRow(rowIndex);
				IRow row = sheet.GetRow(rowIndex);
				if (row == null)
				{
					Debug.LogError("line:" + rowIndex + " is not found in" + sheet.SheetName);
					continue;
				}

				string text = rowLocalized.Cells[0].StringCellValue;
				if(string.IsNullOrEmpty(text)) continue;

				if (rowLocalized.Cells[textColumnIndexList[0]].StringCellValue != text)
				{
					Debug.LogError(string.Format("Text [ {0} ] is not equal in {1} :line {2}", text, sheet.SheetName, rowIndex));
					continue;
				}

				for (int i = 1; i < rowLocalized.Cells.Count;++i )
				{
					ICell cell = rowLocalized.GetCell(i, MissingCellPolicy.CREATE_NULL_AS_BLANK);
					cell.SetCellValue( rowLocalized.Cells[i].StringCellValue );
				}
			}
		}

		//指定のキーの列のインデックスを取得
		public static bool TryGetColumneIndex(ISheet sheet, string key, out int index)
		{
			index = 0;
			IRow row = sheet.GetRow( sheet.FirstRowNum );
			for (int i = 0; i < row.Cells.Count; ++i )
			{
				if (row.Cells[i].StringCellValue == key)
				{
					index = i;
					return true;
				}
			}
			return false;
		}


		//ブックを読み込む
		public static IWorkbook ReadBook(string path)
		{
			string ext = Path.GetExtension(path);
			using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
			{
				if (ext == ExtXls)
				{
					return new HSSFWorkbook(fs);
				}
				else if (ext == ExtXlsx)
				{
					return new XSSFWorkbook(fs);
				}
				else
				{
					Debug.LogError(path + " is not excel file");
					return null;
				}
			}
		}

		//ブックを読み込む
		public static IWorkbook ReadBook(string path, out Stream stream )
		{
			string ext = Path.GetExtension(path);
			using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
			{
				stream = fs;
				if (ext == ExtXls)
				{
					return new HSSFWorkbook(fs);
				}
				else if (ext == ExtXlsx)
				{
					return new XSSFWorkbook(fs);
				}
				else
				{
					Debug.LogError(path + " is not excel file");
					return null;
				}
			}
		}
	
		//ブックを書き込み
		public static void WriteBook(IWorkbook book, string path)
		{
			using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
			{
				book.Write(fs);
			}
		}
	}
}