using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateScenarioBook : MonoBehaviour {

	public Utage.AdvImportBook book;

	public string spread_sheet;
	public string tab_name;

	public bool finished{get;set;}

	public IEnumerator ScenarioUpdate(){

		string sheet_id = "";

		yield return EveryStudioLibrary.CommonNetwork.Instance.RecieveSpreadSheeWorksheetsAsync(spread_sheet,(EveryStudioLibrary.TNetworkData _networkData)=>{
			Debug.LogWarning(_networkData.m_strData);
			sheet_id = EveryStudioLibrary.CommonNetwork.ParseSpreadSheetSerial(spread_sheet, tab_name, _networkData.m_strData);
			Debug.Log(sheet_id);
		});
		
		Debug.Log(sheet_id);

		IDictionary recieve_data = null;
		yield return EveryStudioLibrary.CommonNetwork.Instance.RecieveSpreadSheetAsync(spread_sheet,sheet_id,(EveryStudioLibrary.TNetworkData _networkData)=>{
			Debug.LogWarning(_networkData.m_strData);
			recieve_data = _networkData.m_dictRecievedData;
		});

		List<SpreadSheetData> spread_sheet_data_list =  SpreadSheetData.ConvertSpreadSheetData(recieve_data);


		// 横のデータ数は1行目から推測させる
		List<SpreadSheetData> header_list = spread_sheet_data_list.FindAll(p=>p.row==1);
		Debug.Log(header_list.Count);

		int col_size = header_list.Count;

		int row_size = spread_sheet_data_list[spread_sheet_data_list.Count-1].row;
		Debug.Log(row_size );



		foreach( var data in spread_sheet_data_list){
			Debug.Log( string.Format( "row={0} col={1} param:{2}" , data.row,data.col,  data.param));
		}

		book.ImportGridList[0].Rows.Clear();

		for( int row = 1 ; row <= row_size ; row++){
			string temp = "";
			List<string> add_list = new List<string> ();
			for( int col = 1 ; col <= col_size ; col++){
				SpreadSheetData temp_data = spread_sheet_data_list.Find(p=>p.row==row &&p.col == col);
				if( temp_data != null ){
					add_list.Add(temp_data.param);
					temp += temp_data.param;
				}
				else {
					add_list.Add("");
					temp += "a";
				}
			}
			Debug.Log(temp);
			book.ImportGridList[0].AddRow(add_list);
		}

		yield return 0;
	}
}
