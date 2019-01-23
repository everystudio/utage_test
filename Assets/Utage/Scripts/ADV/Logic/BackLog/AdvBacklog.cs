// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Utage
{
	/// <summary>
	/// バックログのデータ
	/// </summary>
	public class AdvBacklog
	{
		//ページ内でテキストが複数に分かれている場合の各データ
		class AdvBacklogDataInPage
		{
			//バックログテキスト（パラメーターなどで変更される可能性もあるので、記録時のテキストを残す）
			public string LogText { get; private set; }
			//キャラクターラベル
			public string CharacterLabel { get; private set; }
			//キャラクター名（パラメーターなどで変更される可能性もあるので、記録時のテキストを残す）
			public string CharacterNameText { get; private set; }
			//ボイスファイル名
			public string VoiceFileName { get; private set; }

			public AdvBacklogDataInPage()
			{
				LogText="";
				CharacterLabel="";
				CharacterNameText = "";
				VoiceFileName="";
			}
			public AdvBacklogDataInPage(AdvCommandText dataInPage, AdvCharacterInfo characterInfo)
			{
				LogText = "";
				VoiceFileName = "";
				if (characterInfo != null)
				{
					CharacterLabel = characterInfo.Label;
					CharacterNameText = characterInfo.LocalizeNameText;
				}
				else
				{
					CharacterLabel = "";
					CharacterNameText = "";
				}
				LogText = TextParser.MakeLogText(dataInPage.ParseCellLocalizedText());
				if (dataInPage.VoiceFile != null)
				{
					VoiceFileName = dataInPage.VoiceFile.FileName;
					LogText = TextParser.AddTag(LogText, TextParser.TagSound, dataInPage.VoiceFile.FileName);
				}
				else
				{
					VoiceFileName = "";
				}
				if (dataInPage.IsNextBr) LogText += "\n";
			}

			//書き込み
			internal void Write(System.IO.BinaryWriter writer)
			{
				writer.Write(LogText);
				writer.Write(CharacterLabel);
				writer.Write(CharacterNameText);
				writer.Write(VoiceFileName);
			}

			//読み込み
			internal void Read(System.IO.BinaryReader reader, int version)
			{
				LogText = reader.ReadString();
				CharacterLabel = reader.ReadString();
				CharacterNameText = reader.ReadString();
				VoiceFileName = reader.ReadString();
			}

		};
		List<AdvBacklogDataInPage> dataList = new List<AdvBacklogDataInPage>();
		
		//データの追加
		internal void AddData(AdvCommandText log, AdvCharacterInfo characterInfo)
		{
			dataList.Add(new AdvBacklogDataInPage(log, characterInfo));
		}
		
		//データがあるか
		public bool IsEmpty
		{
			get
			{
				return dataList.Count <= 0;
			}
		}

		//テキスト全文
		public string Text
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				foreach (AdvBacklogDataInPage item in dataList)
				{
					builder.Append(item.LogText);
				}
				return builder.ToString().TrimEnd('\n');
			}
		}
		//メインとなるキャラクター名
		public string MainCharacterNameText
		{ 
			get
			{
				foreach (AdvBacklogDataInPage item in dataList)
				{
					if (!string.IsNullOrEmpty(item.CharacterNameText))
					{
						return item.CharacterNameText;
					}
				}
				return "";
			}
		}
		//メインとなるボイスファイル名
		public string MainVoiceFileName
		{
			get
			{
				foreach (AdvBacklogDataInPage item in dataList)
				{
					if (!string.IsNullOrEmpty(item.VoiceFileName))
					{
						return item.VoiceFileName;
					}
				}
				return "";
			}
		}

		//ボイスの数
		internal int CountVoice
		{
			get
			{
				int count = 0;
				foreach (AdvBacklogDataInPage item in dataList)
				{
					if (!string.IsNullOrEmpty(item.VoiceFileName))
					{
						++count;
					}
				}
				return count;
			}
		}

		public string FindCharacerLabel(string voiceFileName)
		{
			foreach (AdvBacklogDataInPage item in dataList)
			{
				if (item.VoiceFileName == voiceFileName)
				{
					return item.CharacterLabel;
				}
			}
			return "";
		}

		const int Version = 0;
		//書き込み
		internal void Write(System.IO.BinaryWriter writer)
		{
			writer.Write(Version);
			writer.Write(dataList.Count);
			foreach (AdvBacklogDataInPage item in dataList)
			{
				writer.Write(item.LogText);
				writer.Write(item.CharacterLabel);
				writer.Write(item.CharacterNameText);
				writer.Write(item.VoiceFileName);
			}
		}

		//読み込み
		internal void Read(System.IO.BinaryReader reader)
		{
			dataList.Clear();
			//バージョンチェック
			int version = reader.ReadInt32();
			if (version == Version)
			{
				int count = reader.ReadInt32();
				for( int i = 0; i < count; ++i )
				{
					AdvBacklogDataInPage data = new AdvBacklogDataInPage();
					data.Read(reader,version);
					dataList.Add(data);
				}
			}
			else
			{
				Debug.LogError(LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.UnknownVersion, version));
			}
		}
	}
}
