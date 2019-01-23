// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace Utage
{

	/// <summary>
	/// データ管理
	/// </summary>
	[AddComponentMenu("Utage/ADV/Internal/DataManager ")]
	public partial class AdvDataManager : MonoBehaviour
	{
		//バックグランドでリソースのDLをするか
		[SerializeField]
		bool isBackGroundDownload = true;
		public bool IsBackGroundDownload
		{
			get { return isBackGroundDownload; }
			set { isBackGroundDownload = value; }
		}

		/// <summary>
		/// 設定データ
		/// </summary>
		public AdvSettingDataManager SettingDataManager { get { return this.settingDataManager; } }
		AdvSettingDataManager settingDataManager = new AdvSettingDataManager();

		//シナリオデータ
		public Dictionary<string, AdvScenarioData> ScenarioDataTbl { get { return this.scenarioDataTbl; } }
		Dictionary<string, AdvScenarioData> scenarioDataTbl = new Dictionary<string, AdvScenarioData>();

		/// <summary>
		/// 設定データが準備済みか
		/// </summary>
		public bool IsReadySettingData { get { return (settingDataManager != null); } }

		/// <summary>
		/// マクロ
		/// </summary>
		public AdvMacroManager MacroManager { get { return this.macroManager; } }
		AdvMacroManager macroManager = new AdvMacroManager();


		/// <summary>
		/// 起動時の初期化
		/// </summary>
		/// <param name="rootDirResource">ルートディレクトリのリソース</param>
		public void BootInit( string rootDirResource )
		{
			settingDataManager.BootInit(rootDirResource);
		}

		/// <summary>
		/// シナリオ起動時の初期化
		/// </summary>
		public void BootInitScenario(bool async)
		{
			if (async)
			{
				//非同期初期化
				StartCoroutine(CoBootInitScenariodData());
			}
			else
			{
				//シナリオデータの初期化
				BootInitScenariodData();
				//リソースファイル(画像やサウンド)のダウンロードをバックグラウンドで進めておく
				StartBackGroundDownloadResource();
			}
		}


		public void BootInitChapter(AdvChapterData chapter)
		{
			chapter.BootInit(this.SettingDataManager);

			//シナリオデータ
			Dictionary<string, AdvScenarioData> scenarios = new Dictionary<string, AdvScenarioData>();
			chapter.AddScenario(scenarios);
			//シナリオデータの初期化
			foreach (var keyValue in scenarios)
			{
				this.scenarioDataTbl.Add(keyValue.Key, keyValue.Value);
			}
			//シナリオデータの初期化
			foreach (var keyValue in scenarios)
			{
				keyValue.Value.Init(this.settingDataManager);
			}
		}


		/// <summary>
		/// シナリオデータのロードと初期化を開始
		/// </summary>
		public void BootInitScenariodData()
		{
			Profiler.BeginSample("シナリオのインポート済みのデータをまず初期化");
			//シナリオのインポート済みのデータをまず初期化
			if (this.settingDataManager.ImportedScenarios != null)
			{
				this.settingDataManager.ImportedScenarios.Chapters.ForEach(x => x.AddScenario(this.scenarioDataTbl));
			}
			Profiler.EndSample();

			Profiler.BeginSample("シナリオデータの初期化");
			//シナリオデータの初期化
			foreach (var data in scenarioDataTbl.Values)
			{
				data.Init(this.settingDataManager);
			}
			Profiler.EndSample();
		}


		/// <summary>
		/// シナリオデータのロードと初期化を開始(非同期版)
		/// </summary>
		public IEnumerator CoBootInitScenariodData()
		{
			//シナリオのインポート済みのデータをまず初期化
			if (this.settingDataManager.ImportedScenarios != null)
			{
				this.settingDataManager.ImportedScenarios.Chapters.ForEach(x => x.AddScenario(this.scenarioDataTbl));
			}

			//シナリオデータの初期化
			foreach (var data in scenarioDataTbl.Values)
			{
				data.Init(this.settingDataManager);
				yield return null;
			}

			//リソースファイル(画像やサウンド)のダウンロードをバックグラウンドで進めておく
			this.StartBackGroundDownloadResource();
		}

		
		/// <summary>
		/// リソースファイル(画像やサウンド)のダウンロードをバックグラウンドで進めておく
		/// </summary>
		/// <param name="startScenario">開始シナリオ名</param>
		public void StartBackGroundDownloadResource()
		{
			if (isBackGroundDownload)
			{
				DownloadAll();
			}
		}

		//明示的に全てのリソースをDLをする
		public void DownloadAll()
		{
			foreach (AdvScenarioData data in scenarioDataTbl.Values)
			{
				data.Download(this);
			}
			SettingDataManager.DownloadAll();
		}

		/// <summary>
		/// 指定のシナリオラベルが既にロード終了しているか
		/// </summary>
		public bool IsLoadEndScenarioLabel(AdvScenarioJumpData jumpData)
		{
			return IsLoadEndScenarioLabel(jumpData.ToLabel);
		}

		/// <summary>
		/// 指定のシナリオラベルが既にロード終了しているか
		/// </summary>
		public bool IsLoadEndScenarioLabel(string label)
		{
			AdvScenarioData scenarioData = FindScenarioData(label);
			if (null != scenarioData) return true;

			string msg = LanguageAdvErrorMsg.LocalizeTextFormat(AdvErrorMsg.NotFoundScnarioLabel, label);
			Debug.LogError(msg);
			return false;
		}

		/// <summary>
		///  シナリオデータを検索して取得
		/// </summary>
		/// <param name="ScebarioLabel">シナリオラベル</param>
		/// <returns>シナリオデータ。見つからなかったらnullを返す</returns>
		public AdvScenarioData FindScenarioData(string label)
		{
			foreach (AdvScenarioData data in scenarioDataTbl.Values )
			{
				if (data.IsContainsScenarioLabel(label))
				{
					return data;
				}
			}
			return null;
		}

		/// <summary>
		///  シナリオデータを検索して取得
		/// </summary>
		/// <param name="ScebarioLabel">シナリオラベル</param>
		/// <returns>シナリオデータ。見つからなかったらnullを返す</returns>
		public AdvScenarioLabelData FindScenarioLabelData(string scenarioLabel)
		{
			foreach (AdvScenarioData data in scenarioDataTbl.Values)
			{
				AdvScenarioLabelData labelData = data.FindScenarioLabelData(scenarioLabel);
				if (labelData!=null)
				{
					return labelData;
				}
			}
			return null;
		}


		public AdvScenarioLabelData NextScenarioLabelData(string scenarioLabel)
		{
			foreach (AdvScenarioData data in scenarioDataTbl.Values)
			{
				AdvScenarioLabelData labelData = data.FindNextScenarioLabelData(scenarioLabel);
				if (labelData != null)
				{
					return labelData;
				}
			}
			return null;
		}

		//サブルーチンの帰り先を見つけて情報を設定
		internal void SetSubroutineRetunInfo( string scenarioLabel, int subroutineCommandIndex, SubRoutineInfo info)
		{
			foreach (AdvScenarioData data in scenarioDataTbl.Values)
			{
				AdvScenarioLabelData labelData = data.FindScenarioLabelData(scenarioLabel);
				if (labelData == null) continue;

				if (!labelData.TrySetSubroutineRetunInfo(subroutineCommandIndex, info))
				{
					AdvScenarioLabelData nextData = NextScenarioLabelData(scenarioLabel);

					info.ReturnLabel = nextData.ScenarioLabel;
					info.ReturnPageNo = 0;
					info.ReturnCommand = null;
				}
				break;
			}
		}

		//指定のシナリオラベルの指定ページから最大ファイル数先読み
		public HashSet<AssetFile> MakePreloadFileList(string scenarioLabel, int page, int maxFilePreload, int preloadDeep)
		{
			foreach (AdvScenarioData data in scenarioDataTbl.Values)
			{
				if (data.IsContainsScenarioLabel(scenarioLabel))
				{
					AdvScenarioLabelData label = data.FindScenarioLabelData(scenarioLabel);
					if (label == null) return null;

					return label.MakePreloadFileListSub(this, page, maxFilePreload, preloadDeep);
				}
			}
			return null;
		}
	}
}
