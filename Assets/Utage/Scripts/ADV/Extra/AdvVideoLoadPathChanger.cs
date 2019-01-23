//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// ビデオのロードパスを変更
	/// </summary>
	[AddComponentMenu("Utage/ADV/Extra/VideoLoadPathChanger")]
	public class AdvVideoLoadPathChanger : MonoBehaviour
	{
		public string RootPath { get { return rootPath; } }

		[SerializeField]
		string rootPath = "";

		//ファイルのロードを上書きするコールバックを登録
		void Awake()
		{
			AssetFileManager.GetCustomLoadManager().OnFindAsset += FindAsset;
		}

		//ファイルのロードを上書き
		void FindAsset(AssetFileManager mangager, AssetFileInfo fileInfo, IAssetFileSettingData settingData, ref AssetFileBase asset)
		{
			if (IsVideoType(fileInfo, settingData))
			{
				//宴形式の通常ファイルロード
				asset = new AdvLocalVideoFile(this, mangager, fileInfo, settingData);
			}
		}

		bool IsVideoType(AssetFileInfo fileInfo, IAssetFileSettingData settingData)
		{
			if (fileInfo.FileType != AssetFileType.UnityObject) return false;
			if (settingData is AdvCommandSetting)
			{
				AdvCommandSetting setting = settingData as AdvCommandSetting;
				return setting.Command is AdvCommandVideo;
			}
			else
			{
				AdvGraphicInfo info = settingData as AdvGraphicInfo;
				return (info != null && info.FileType == AdvGraphicInfo.FileTypeVideo);
			}
		}
	}

	//ビデオのみ強制的にローカルからロードする処理
	internal class AdvLocalVideoFile : AssetFileUtage
	{
		public AdvLocalVideoFile(AdvVideoLoadPathChanger pathChanger, AssetFileManager assetFileManager, AssetFileInfo fileInfo, IAssetFileSettingData settingData)
			: base(assetFileManager, fileInfo, settingData)
		{
			fileInfo.StrageType = AssetFileStrageType.Resources;
			if (settingData is AdvCommandSetting)
			{
				AdvCommandSetting setting = settingData as AdvCommandSetting;
				string fileName = setting.Command.ParseCell<string>(AdvColumnName.Arg1);
				this.LoadPath = FilePathUtil.Combine(pathChanger.RootPath, fileName);
			}
			else
			{
				AdvGraphicInfo info = settingData as AdvGraphicInfo;
				string fileName = info.FileName;
				this.LoadPath = FilePathUtil.Combine(pathChanger.RootPath, fileName);
			}
		}
	}
}
