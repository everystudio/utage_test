// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System; 
using System.Text.RegularExpressions; 
using UnityEngine;

#if false
namespace Utage
{
	[System.Serializable]
	public class StringGridList
	{
		public List<StringGrid> List { get { return list; } }
		[SerializeField]
		List<StringGrid> list = new List<StringGrid>();
		public bool IsLoadEnd { get; set; }
		public bool IsInit { get; protected set; }

		/// <summary>
		/// CSV設定ファイルをロードして、データ作成
		/// </summary>
		/// <param name="filePathInfoList">ロードするパスのリスト</param>
		/// <returns></returns>
		public virtual IEnumerator LoadCsvAsync(List<AssetFilePathInfo> filePathInfoList)
		{
			IsLoadEnd = false;
			ClearGridList();
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
					List.Add(file.Csv);
				}
				file.Unuse(this);
			}
			IsLoadEnd = true;
		}

		public void ClearGridList()
		{
			List.Clear();
		}

		public void AddGrid(StringGrid grid)
		{
			List.Add(grid);
		}
	}
}
#endif
