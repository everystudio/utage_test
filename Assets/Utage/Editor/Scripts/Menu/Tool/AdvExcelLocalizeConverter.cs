// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Utage
{

	//「Utage」のシナリオデータ用のエクセルファイル解析して、ローカライズ用のエクセルファイルを作成する
	public class AdvExcelLocalizeConverter : CustomEditorWindow
	{
		/// <summary>
		/// サーバー用のリソースの出力先のパス
		/// </summary>
		[SerializeField, PathDialog(PathDialogAttribute.DialogType.Directory)]
		string outputDirectiory;
		public string OutputDirectiory
		{
			get { return outputDirectiory; }
			set { outputDirectiory = value; }
		}

		const string TextKey = "Text";

		[SerializeField]
		SystemLanguage defaultLanguage = SystemLanguage.Japanese;

		[SerializeField]
		SystemLanguage[] languages = new SystemLanguage[] { SystemLanguage.English };

		protected override bool DrawProperties()
		{
			bool ret = base.DrawProperties();

			EditorGUI.BeginDisabledGroup(AdvScenarioDataBuilderWindow.ProjectData == null);
			{
				if( GUILayout.Button("Convert", GUILayout.Width(180)) )
				{
					Convert(OutputDirectiory, AdvScenarioDataBuilderWindow.ProjectData.AllExcelPathList);
				}
			}
			EditorGUI.EndDisabledGroup();
			GUILayout.Space(8f);
			return ret; 
		}


		void Convert(string outputDirectiory, List<string> assetPathList)
		{
			foreach (string path in assetPathList)
			{
				StringGridDictionary gridTbl = ExcelParser.Read(path, '#', AdvScenarioDataBuilderWindow.ProjectData.ParseFormula, AdvScenarioDataBuilderWindow.ProjectData.ParseNumreic);
				gridTbl.RemoveSheets(@"^#");
				string outputPath = FilePathUtil.Combine(outputDirectiory, FilePathUtil.GetFileName(path));
				ExcelParser.Write (outputPath,ConvertToLocalized (gridTbl));
			}
		}

		StringGridDictionary ConvertToLocalized( StringGridDictionary gridTbl )
		{
			List<string> languageNameList = LanguageNameList();

			StringGridDictionary localizedGridTbl = new StringGridDictionary ();
			foreach( var keyValue in gridTbl.List )
			{
				int index;
				StringGrid grid = keyValue.Grid;
				if (grid.TryGetColumnIndex(TextKey, out index))
				{
					StringGrid localizedGrid = new StringGrid(grid.Name, grid.SheetName, CsvType.Tsv );
					localizedGrid.AddRow(languageNameList);
					for( int i = 0; i < grid.Rows.Count; ++i  )
					{
						if(i==0) continue;
						string text = grid.Rows[i].ParseCellOptional<string>(TextKey,"");
						localizedGrid.AddRow( new List<string>(new string[]{text}) );
					}
					localizedGridTbl.Add(new StringGridDictionaryKeyValue(grid.SheetName, localizedGrid));
				}
			}
			return localizedGridTbl;
		}

		List<string> LanguageNameList()
		{
			string format = "[[[{0}]]]";
			List<string> list = new List<string>();
			list.Add( string.Format( format, defaultLanguage) );
			foreach (var item in languages)
			{
				list.Add(string.Format(format, item.ToString()));
			}
			return list;
		}

	}
}