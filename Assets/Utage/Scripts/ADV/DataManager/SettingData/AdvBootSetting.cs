// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// ゲームの起動設定データ
	/// </summary>
	[System.Serializable]
	public partial class AdvBootSetting
	{

		[System.Serializable]
		public class DefaultDirInfo
		{
			public string defaultDir;		//デフォルトのディレクトリ
			public string defaultExt;		//デフォルトの拡張子

			public string FileNameToPath(string fileName)
			{
				return FileNameToPath(fileName, "");
			}
		
			public string FileNameToPath(string fileName, string LocalizeDir)
			{
				if (string.IsNullOrEmpty(fileName)) return fileName;

				string path;
				//既に絶対URLならそのまま
				if (FilePathUtil.IsAbsoluteUri(fileName))
				{
					path = fileName;
				}
				else
				{
					try
					{
						//拡張子がなければデフォルト拡張子を追加
						if (string.IsNullOrEmpty(FilePathUtil.GetExtension(fileName)))
						{
							fileName += defaultExt;
						}
						path = defaultDir + LocalizeDir + "/" + fileName;
					}
					catch (System.Exception e)
					{
						Debug.LogError(fileName + "  " + e.ToString());
						path = defaultDir + LocalizeDir + "/" + fileName;
					}
				}

				//プラットフォームが対応する拡張子にする(mp3とoggを入れ替え)
				return ExtensionUtil.ChangeSoundExt(path);
			}
		};

		public string ResourceDir { get; set; }		//リソースのルートディレクトリ

		/// <summary>
		/// キャラクターテクスチャのパス情報
		/// </summary>
		public DefaultDirInfo CharacterDirInfo { get { return characterDirInfo; } }
		DefaultDirInfo characterDirInfo;

		/// <summary>
		/// 背景テクスチャのパス情報
		/// </summary>
		public DefaultDirInfo BgDirInfo { get { return bgDirInfo; } }
		DefaultDirInfo bgDirInfo;

		/// <summary>
		/// イベントCGテクスチャのパス情報
		/// </summary>
		public DefaultDirInfo EventDirInfo { get { return eventDirInfo; } }
		DefaultDirInfo eventDirInfo;

		/// <summary>
		/// スプライトテクスチャのパス情報
		/// </summary>
		public DefaultDirInfo SpriteDirInfo { get { return spriteDirInfo; } }
		DefaultDirInfo spriteDirInfo;

		/// <summary>
		/// サムネイルテクスチャのパス情報
		/// </summary>
		public DefaultDirInfo ThumbnailDirInfo { get { return thumbnailDirInfo; } }
		DefaultDirInfo thumbnailDirInfo;

		/// <summary>
		/// BGMのパス情報
		/// </summary>
		public DefaultDirInfo BgmDirInfo { get { return bgmDirInfo; } }
		DefaultDirInfo bgmDirInfo;

		/// <summary>
		/// SEのパス情報
		/// </summary>
		public DefaultDirInfo SeDirInfo { get { return seDirInfo; } }
		DefaultDirInfo seDirInfo;

		/// <summary>
		/// 環境音のパス情報
		/// </summary>
		public DefaultDirInfo AmbienceDirInfo { get { return ambienceDirInfo; } }
		DefaultDirInfo ambienceDirInfo;

		/// <summary>
		/// ボイスのパス情報
		/// </summary>
		public DefaultDirInfo VoiceDirInfo { get { return voiceDirInfo; } }
		DefaultDirInfo voiceDirInfo;

		/// <summary>
		/// パーティクルのパス情報
		/// </summary>
		public DefaultDirInfo ParticleDirInfo { get { return particleDirInfo; } }
		DefaultDirInfo particleDirInfo;

		/// <summary>
		/// パーティクルのパス情報
		/// </summary>
		public DefaultDirInfo VideoDirInfo { get { return videoDirInfo; } }
		DefaultDirInfo videoDirInfo;

		/// <summary>
		/// 起動時の初期化
		/// </summary>
		/// <param name="resourceDir">リソースディレクトリ</param>
		public void BootInit( string resourceDir)
		{
			this.ResourceDir = resourceDir;
			characterDirInfo = new DefaultDirInfo { defaultDir = @"Texture/Character", defaultExt = ".png" };
			bgDirInfo = new DefaultDirInfo { defaultDir = @"Texture/BG", defaultExt = ".jpg" };
			eventDirInfo = new DefaultDirInfo { defaultDir = @"Texture/Event", defaultExt = ".jpg" };
			spriteDirInfo = new DefaultDirInfo { defaultDir = @"Texture/Sprite", defaultExt = ".png" };
			thumbnailDirInfo = new DefaultDirInfo { defaultDir = @"Texture/Thumbnail", defaultExt = ".jpg" };
			bgmDirInfo = new DefaultDirInfo { defaultDir = @"Sound/BGM", defaultExt = ".wav" };
			seDirInfo = new DefaultDirInfo { defaultDir = @"Sound/SE", defaultExt = ".wav" };
			ambienceDirInfo = new DefaultDirInfo { defaultDir = @"Sound/Ambience", defaultExt = ".wav" };
			voiceDirInfo = new DefaultDirInfo { defaultDir = @"Sound/Voice", defaultExt = ".wav" };
			particleDirInfo = new DefaultDirInfo { defaultDir = @"Particle", defaultExt = ".prefab" };
			videoDirInfo = new DefaultDirInfo { defaultDir = @"Video", defaultExt = ".mp4" };

			InitDefaultDirInfo(ResourceDir, characterDirInfo);
			InitDefaultDirInfo(ResourceDir, bgDirInfo);
			InitDefaultDirInfo(ResourceDir, eventDirInfo);
			InitDefaultDirInfo(ResourceDir, spriteDirInfo);
			InitDefaultDirInfo(ResourceDir, thumbnailDirInfo);
			InitDefaultDirInfo(ResourceDir, bgmDirInfo);
			InitDefaultDirInfo(ResourceDir, seDirInfo);
			InitDefaultDirInfo(ResourceDir, ambienceDirInfo);
			InitDefaultDirInfo(ResourceDir, voiceDirInfo);
			InitDefaultDirInfo(ResourceDir, particleDirInfo);
			InitDefaultDirInfo(ResourceDir, videoDirInfo);
		}
		void InitDefaultDirInfo(string root, DefaultDirInfo info)
		{
			info.defaultDir = FilePathUtil.Combine( root,info.defaultDir );
		}

		public string GetLocalizeVoiceFilePath( string file )
		{
			if (LanguageManagerBase.Instance.IgnoreLocalizeVoice)
			{
				return VoiceDirInfo.FileNameToPath(file);
			}
			else
			{
				string language = LanguageManagerBase.Instance.CurrentLanguage;
				if (LanguageManagerBase.Instance.VoiceLanguages.Contains(language))
				{
					return VoiceDirInfo.FileNameToPath(file, language);
				}
				else
				{
					return VoiceDirInfo.FileNameToPath(file);
				}
			}
		}
	}
}