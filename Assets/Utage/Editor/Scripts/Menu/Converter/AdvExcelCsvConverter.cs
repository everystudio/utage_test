// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Utage
{
	//「Utage」のシナリオデータ用のエクセルファイル解析して、CSVファイルを生成する
	public class AdvExcelCsvConverter
	{
		internal class AdvExcelSheets
		{
			public string Path { get; private set; }
			public string Name { get; private set; }
			StringGridDictionary sheets;
			public List<StringGrid> SettingsSheets { get; private set; }
			List<StringGrid> ScenarioSheets { get; set; }
			public List<CsvInfo> CsvList { get; private set; }

			internal AdvExcelSheets(string path)
			{
				this.SettingsSheets = new List<StringGrid>();
				this.ScenarioSheets = new List<StringGrid>();
				this.CsvList = new List<CsvInfo>();
				this.Path = path;
				this.Name = System.IO.Path.GetFileNameWithoutExtension(Path);
				this.sheets = ExcelParser.Read(path, '#',false, false);
				this.sheets.RemoveSheets(@"^#");
			}

			internal bool TryConvertToCsv(int version)
			{
				foreach (var sheet in sheets.List)
				{
					if (AdvSheetParser.IsSettingsSheet(sheet.Grid.SheetName))
					{
						SettingsSheets.Add(sheet.Grid);
						CsvList.Add(new CsvInfo(sheet.Grid, this.Name + "/Settings/"+ sheet.Key));
					}
					else
					{
						ScenarioSheets.Add(sheet.Grid);
						CsvList.Add(new CsvInfo(sheet.Grid, this.Name + "/Scenario/"+ sheet.Key));
					}
				}

				//シナリオ設定シートは個別にコンバート
				CsvList.Add(new CsvInfo(MakeScenarioSettingGrid(version), this.Name + "/Settings/"+ AdvSheetParser.SheetNameScenario) );
				return true;
				///起動用CSVをコンバート
//				csvInfoList.Add(ConvertBootSetting(version));
			}

			StringGrid MakeScenarioSettingGrid(int version)
			{
				StringGrid grid = new StringGrid(AdvSheetParser.SheetNameScenario, AdvSheetParser.SheetNameScenario,CsvType.Tsv);
				grid.AddRow(new List<string> { AdvParser.Localize(AdvColumnName.FileName), AdvParser.Localize(AdvColumnName.Version) });
				grid.ParseHeader();

				foreach (var sheet in ScenarioSheets)
				{
					grid.AddRow(new List<string> { this.Name + "/Scenario/" + sheet.SheetName, "" + version });
				}
				return grid;
			}
		}
		
		public class CsvInfo
		{
			public StringGrid Grid { get; protected set; }
			public string Path { get; protected set; }
			internal CsvInfo(StringGrid grid, string path)
			{
				this.Grid = grid;
				this.Path = path;
			}
			//ファイルの書き込み
			internal void Write(string folderPath)
			{
				string path = FilePathUtil.Combine(folderPath, this.Path + ExtensionUtil.TSV);
				string dir = FilePathUtil.GetDirectoryPath(path);
				if (!Directory.Exists(dir))
				{
					Directory.CreateDirectory(dir);
				}
				// ファイルにテキストを書き出し。
				File.WriteAllText(path, this.Grid.ToText());
			}
		}

		/// <summary>
		/// CSVにコンバートして書き込みをする
		/// </summary>
		/// <param name="folderPath">出力先パス</param>
		/// <param name="assetPathList">読み込むエクセルファイルのリスト</param>
		/// <returns>コンバートしたらtrue。失敗したらfalse</returns>
		public bool Convert(string folderPath, List<string> assetPathList, string chapterName, int version)
		{
			List<CsvInfo> csvInfoList = new List<CsvInfo>();
			if (!TryConvertToCsvList(assetPathList, chapterName, version, ref csvInfoList)) return false;

			//書き込み
			foreach (var item in csvInfoList)
			{
				item.Write(folderPath);
			}
			return true;
		}

		public bool TryConvertToCsvList(List<string> assetPathList, string chapterName, int version, ref List<CsvInfo> csvInfoList)
		{
			List<AdvExcelSheets> excelSheets = new List<AdvExcelSheets>();

			if (assetPathList.Count <= 0) return false;
			//対象のエクセルファイルを全て読み込み
			foreach (string assetPath in assetPathList)
			{
				if (!string.IsNullOrEmpty(assetPath))
				{
					excelSheets.Add(new AdvExcelSheets(assetPath));
				}
			}
			if (excelSheets.Count <= 0) return false;

			//CSVにコンバート
			foreach (var item in excelSheets)
			{
				if (!item.TryConvertToCsv(version)) return false;
			}

			foreach (var item in excelSheets)
			{
				csvInfoList.AddRange( item.CsvList );
			}

			csvInfoList.Add(ConvertBootSetting(excelSheets, chapterName, version));
			return true;
			
		}

		//起動用CSVをコンバート
		CsvInfo ConvertBootSetting(List<AdvExcelSheets> excelSheets, string chapterName, int version)
		{
			if(string.IsNullOrEmpty(chapterName) ) chapterName = AdvSheetParser.SheetNameBoot;
			StringGrid grid = new StringGrid(chapterName, chapterName, CsvType.Tsv);
			grid.AddRow(new List<string> { AdvParser.Localize(AdvColumnName.Tag), AdvParser.Localize(AdvColumnName.Param1), AdvParser.Localize(AdvColumnName.Version) });
			///起動用データをコンバート
			foreach(var excel in excelSheets )
			{
				string excelName = 	System.IO.Path.GetFileNameWithoutExtension(excel.Path);
				//シナリオ設定シートは個別にコンバート
				AddFileDataToTsv(grid, version, excelName, AdvSheetParser.SheetNameScenario);
				foreach (var sheet in excel.SettingsSheets)
				{
					AddFileDataToTsv(grid, version, excelName, sheet.SheetName);
				}
			}

			string path = chapterName;
			return new CsvInfo(grid, path);
		}

		void AddFileDataToTsv(StringGrid grid, int version, string excelFileName, string sheetName)
		{
			const string format = "{0}/Settings/{1}.tsv";
			string tag = AdvSheetParser.ToBootTsvTagName(sheetName);
			grid.AddRow(new List<string> { tag, string.Format(format, excelFileName, sheetName), "" + version });
		}
	}
}