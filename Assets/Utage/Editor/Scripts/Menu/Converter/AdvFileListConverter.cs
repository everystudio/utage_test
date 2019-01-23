// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Utage
{
#if false
	//「Utage」のファイルリストコンバーター
	public class AdvFileListConverter
	{
		public string OutputResourcesPath { get; protected set; }

		//ファイルの入出力に使う
		public FileIOManager FileIOManager { get; protected set; }
		
		public ConvertFileList ConvertFileList { get; protected set; }

		public AdvFileListConverter(string outPutDirectoryPath, FileIOManager fileIOManager)
		{
			OutputResourcesPath = outPutDirectoryPath;
			FileIOManager = fileIOManager;
		}

		//リソースをコンバートしてバージョンアップする
		public void VersionUp(System.Action<AdvFileListConverter> CallbackConvertFiles)
		{
			this.Read();
			CallbackConvertFiles(this);
			this.Write();
		}

		//コンバートファイルリストのファイルを読み込む
		void Read()
		{
			//出力先のアセットバンドル情報を読み込む
			string convertFileListPath = FilePathUtil.Combine( OutputResourcesPath,Path.GetFileNameWithoutExtension(OutputResourcesPath) + ExtensionUtil.ConvertFileList);
			convertFileListPath += ExtensionUtil.UtageFile;
			ConvertFileList = new ConvertFileList(convertFileListPath);
			//ファイルから読み込む
			if (File.Exists(convertFileListPath))
			{
				byte[] bytes = File.ReadAllBytes(convertFileListPath);
				bytes = FileIOManager.Decode(bytes);
				BinaryUtil.BinaryRead(bytes, ConvertFileList.Read);
			}
		}

		//コンバートファイルリストのファイルを書き込む
		void Write()
		{
			byte[] bytes = BinaryUtil.BinaryWrite(ConvertFileList.Write);
			bytes = FileIOManager.Encode(bytes);
			File.WriteAllBytes(ConvertFileList.FilePath, bytes);
		}
		
		//ログファイルを書き込む
		public void WriteLog(bool isAssetBundle)
		{
			string logFileListPath = FilePathUtil.Combine( OutputResourcesPath, Path.GetFileNameWithoutExtension(OutputResourcesPath) + ExtensionUtil.ConvertFileListLog);
			logFileListPath += ExtensionUtil.Txt;
			File.WriteAllText(logFileListPath, ConvertFileList.ToLogString(isAssetBundle));
		}
	}
#endif
}