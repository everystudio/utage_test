﻿// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.Profiling;
using System.Collections;
using System.Collections.Generic;

namespace Utage
{
	/// <summary>
	/// 章データ
	/// </summary>
	public class AdvChapterData : ScriptableObject
	{
		//章の名前
		public string ChapterName { get { return chapterName; } }
		[SerializeField]
		string chapterName = "";

		//インポートされたデータのリスト
		public List<AdvImportBook> DataList { get { return dataList; } }
		[SerializeField]
		List<AdvImportBook> dataList = new List<AdvImportBook>();

		public List<StringGrid> SettingList { get { return settingList; } }
		[SerializeField]
		List<StringGrid> settingList = new List<StringGrid>();

		public bool IsInited { get; set; }

		public void Init(string name)
		{
			this.chapterName = name;
		}


		//起動時初期化
		public void BootInit(AdvSettingDataManager settingDataManager)
		{
			IsInited = true;
			//設定データの初期化
			foreach (var grid in settingList)
			{
				IAdvSetting data = AdvSheetParser.FindSettingData(settingDataManager, grid.SheetName);
				if (data != null)
				{
					data.ParseGrid(grid);
				}
			}
			foreach (var grid in settingList)
			{
				IAdvSetting data = AdvSheetParser.FindSettingData(settingDataManager, grid.SheetName);
				if (data != null)
				{
					data.BootInit(settingDataManager);
				}
			}
		}


		public void AddScenario(Dictionary<string, AdvScenarioData> scenarioDataTbl)
		{
			Profiler.BeginSample("AddScenario");
			foreach (var book in DataList)
			{
				foreach (var sheet in book.ImportGridList)
				{
					if (scenarioDataTbl.ContainsKey(sheet.SheetName))
					{
						Debug.LogErrorFormat("{0} is already contains", sheet.SheetName);
						continue;
					}
					Profiler.BeginSample("new Scenario");
					sheet.InitLink();
					AdvScenarioData scenario = new AdvScenarioData(sheet);
					scenarioDataTbl.Add(sheet.SheetName, scenario);
					Profiler.EndSample();
				}
			}
			Profiler.EndSample();
		}



#if UNITY_EDITOR

		public void ImportBooks(List<AdvImportBook> importDataList, AdvMacroManager macroManager)
		{
			this.DataList.Clear();
			this.SettingList.Clear();
			foreach (var book in importDataList)
			{
				this.DataList.Add(book);
				foreach (var sheet in book.GridList)
				{
					ImportSheet(sheet, macroManager);
				}
			}
		}

		void ImportBook(AdvImportBook book, AdvMacroManager macroManager)
		{
			foreach (var sheet in book.GridList)
			{
				ImportSheet(sheet, macroManager);
			}
		}

		void ImportSheet(StringGrid sheet, AdvMacroManager macroManager)
		{
			sheet.InitLink();
			string sheetName = sheet.SheetName;
			if (AdvSheetParser.IsDisableSheetName(sheetName))
			{
				Debug.LogError(sheetName + " is invalid name");
				return;
			}

			if (AdvSheetParser.IsSettingsSheet(sheetName))
			{
				SettingList.Add(sheet);
			}
			else
			{
				macroManager.TryAddMacroData(sheet.SheetName, sheet);
			}
		}

		public void MakeScenarioImportData(AdvSettingDataManager dataManager, AdvMacroManager macroManager)
		{
			foreach (var book in DataList)
			{
				if (book.Reimport)
				{
					book.MakeImportData(dataManager, macroManager);
				}
			}
		}
#endif
	}
}