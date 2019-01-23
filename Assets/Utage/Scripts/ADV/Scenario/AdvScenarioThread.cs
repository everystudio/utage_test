﻿// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace Utage
{
	[AddComponentMenu("Utage/ADV/Internal/ScenarioThread")]
	public class AdvScenarioThread : MonoBehaviour
	{
		//スレッド名
		public string ThreadName { get { return threadName; } }
		[SerializeField,NotEditable]
		string threadName;

		// メインスレッドかどうか
		public bool IsMainThread { get; private set; }

		// ロード中か
		public bool IsLoading { get; private set; }

		//サブスレッド含めてロード中か
		public bool IsLoadingDeep
		{
			get
			{
				if (IsLoading) return true;
				foreach (var item in SubThreadList)
				{
					if (item.IsLoading) return true;
				}
				return false;
			}
		}


		// シナリオ実行中か
		public bool IsPlaying { get; set; }

		//If文制御のマネージャー
		internal AdvIfManager IfManager { get { return this.ifManager; } }
		AdvIfManager ifManager = new AdvIfManager();

		//ジャンプのマネージャー
		internal AdvJumpManager JumpManager { get { return this.jumpManager; } }
		AdvJumpManager jumpManager = new AdvJumpManager();

		//待機処理のマネージャー
		internal AdvWaitManager WaitManager { get { return this.waitManager; } }
		AdvWaitManager waitManager = new AdvWaitManager();

		//親スレッド
		AdvScenarioThread ParenetThread{ get; set; }

		//サブスレッドリスト
		List<AdvScenarioThread> SubThreadList { get { return this.subThreadList; } }
		List<AdvScenarioThread> subThreadList = new List<AdvScenarioThread>();

		//プリロードするファイル
		HashSet<AssetFile> preloadFileSet = new HashSet<AssetFile>();

		//現在のコマンド
		public AdvCommand CurrentCommand { get { return currentCommand; } }
		AdvCommand currentCommand;

		//セーブ時にページのヘッダ部分をスキップする
		internal bool SkipPageHeaerOnSave { get; private set; }

		//現在のコマンドか判別
		public bool IsCurrentCommand(AdvCommand command)
		{
			return (command != null) && (currentCommand == command);
		}

		//シナリオプレイヤー
		internal AdvScenarioPlayer ScenarioPlayer { get; private set; }


		//ADVエンジン
		internal AdvEngine Engine { get { return this.ScenarioPlayer.Engine; } }

		//初期化
		internal void Init(AdvScenarioPlayer scenarioPlayer, string name, AdvScenarioThread parent)
		{
			this.ScenarioPlayer = scenarioPlayer; 
			this.threadName = name;
			this.ParenetThread = parent;
			IsMainThread = (parent == null);
		}

		//破棄するときの処理
		void OnDestroy()
		{
			//プリ―ロードファイルだけはちゃんとクリアしておく
			ClearPreload();
			CleaSubTreadList();

			if (this.ParenetThread)
			{
				this.ParenetThread.SubThreadList.Remove(this);
			}
		}

		//クリア処理(開始時やセーブデータロードからの開始時、終了時など、完全にデータをクリアするときに呼ばれる)
		internal void Clear()
		{
			IsPlaying = false;
			CleaSubTreadList();
			ResetOnJump();
			WaitManager.Clear();
			jumpManager.Clear();
			StopAllCoroutines();
		}

		//キャンセル処理
		internal void Cancel()
		{
			Clear();
			Destroy(this);
		}

		//　ジャンプ時のクリア処理
		void ResetOnJump()
		{
			IsLoading = false;
			ifManager.Clear();
			jumpManager.ClearOnJump();
			ClearPreload();
		}

		// 指定のシナリオラベル、ページ数からシナリオの実行開始
		internal void StartScenario(string label, int page, bool skipPageHeaer)
		{
			StartCoroutine(CoStartScenario(label, page, null, skipPageHeaer));
		}

		//指定のシナリオを再生
		IEnumerator CoStartScenario(string label, int page, AdvCommand returnToCommand, bool skipPageHeaer)
		{
			IsPlaying = true;
			SkipPageHeaerOnSave = false;
			//ジャンプ先のシナリオラベルのログを出力
			if (ScenarioPlayer.DebugOutputLog) Debug.Log("Jump : " + label + " :" + page);

			//起動時のロード待ち
			while (Engine.IsLoading)
			{
				yield return null;
			}


			//シナリオロード待ち
			IsLoading = true;
			while (!Engine.DataManager.IsLoadEndScenarioLabel(label))
			{
				yield return null;
			}
			IsLoading = false;

			//各データをリセット
			ResetOnJump();

			if (page < 0) page = 0;
			//ページ指定がある場合はif分岐の設定をしておく
			if (page != 0) ifManager.IsLoadInit = true;

			//ジャンプ先のシナリオデータを取得
			AdvScenarioLabelData currentLabelData = Engine.DataManager.FindScenarioLabelData(label);
			while (currentLabelData != null)
			{
				ScenarioPlayer.UpdateSceneGallery(currentLabelData.ScenarioLabel, Engine);
				AdvScenarioPageData currentPageData = currentLabelData.GetPageData(page);
				//ページデータを取得
				while (currentPageData != null)
				{
					//プリロードを更新
					UpdatePreLoadFiles(currentLabelData.ScenarioLabel, page);

					///ページ開始処理
					if (IsMainThread)
					{
						Engine.Page.BeginPage(currentPageData);
					}

					//0フレーム即コルーチンが終わる場合を考えてこう書く
					var pageCoroutine = StartCoroutine(CoStartPage(currentLabelData, currentPageData, returnToCommand, skipPageHeaer));
					if (pageCoroutine != null)
					{
						yield return pageCoroutine;
					}
					currentCommand = null;
					returnToCommand = null;
					skipPageHeaer = false;
					//ページ終了処理
					if (IsMainThread)
					{
						Engine.Page.EndPage();
					}
					if (IsBreakCommand)
					{
						if (IsMainThread && ScenarioPlayer.IsReservedEndScenario)
						{
							ScenarioPlayer.EndScenario();
							yield break;
						}
						else
						{
							if (JumpManager.IsReserved)
							{
								JumpToReserved();
								yield break;
							}
							else
							{
								OnEndThread();
								yield break;
							}
						}
					}
					currentPageData = currentLabelData.GetPageData(++page);
				}
				//ロード直後処理終了
				IfManager.IsLoadInit = false;
				currentLabelData = Engine.DataManager.NextScenarioLabelData(currentLabelData.ScenarioLabel);
				page = 0;
			}
			OnEndThread();
		}

		//コマンドスレッド終了
		void OnEndThread()
		{
			IsPlaying = false;
			if (IsMainThread)
			{
				ScenarioPlayer.EndScenario();
			}
			else
			{
				Destroy(this);
			}
		}


		//一ページ内のコマンド処理
		IEnumerator CoStartPage(AdvScenarioLabelData labelData, AdvScenarioPageData pageData, AdvCommand returnToCommand, bool skipPageHeaer)
		{
			int index = skipPageHeaer ? pageData.IndexTextTopCommand : 0;
			AdvCommand command = pageData.GetCommand(index);

			if (returnToCommand != null)
			{
				while (command != returnToCommand)
				{
					command = pageData.GetCommand(++index);
				}
			}

			//復帰直後はIf内分岐は無効
			if (IfManager.IsLoadInit)
			{
				index = pageData.GetIfSkipCommandIndex(index);
				command = pageData.GetCommand(index);
			}

			//ページ冒頭の状態をセーブデータとして記憶
			if (EnableSaveOnPageTop() && pageData.EnableSave )
			{
				SkipPageHeaerOnSave = false;
				Engine.SaveManager.UpdateAutoSaveData(Engine);
			}
			//システムパラメーターの変更があった場合にシステムセーブデータとして記憶
			CheckSystemDataWriteIfChanged();

			while (command != null)
			{
				if (command.IsEntityType)
				{
					//エンティティコマンドの場合は、コマンドを作り直して差し替え
					command = AdvEntityData.CreateEntityCommand(command,Engine,pageData);
				}

				//ifスキップチェック
				if (IfManager.CheckSkip(command))
				{
					if (ScenarioPlayer.DebugOutputLog) Debug.Log("Command If Skip: " + command.GetType() + " " + labelData.ScenarioLabel + ":" + pageData.PageNo);
					command = pageData.GetCommand(++index);
					continue;
				}

				currentCommand = command;
				//ロード
				command.Load();

				//テキスト表示開始時におけるオートセーブ
				if (EnableSaveTextTop() && pageData.EnableSaveTextTop(command) )
				{
					SkipPageHeaerOnSave = true;
					//オートセーブデータ作成
					Engine.SaveManager.UpdateAutoSaveData(Engine);
					//システムパラメーターの変更があった場合にシステムセーブデータとして記憶
					CheckSystemDataWriteIfChanged();
				}

				//ロード待ち
				while (!command.IsLoadEnd())
				{
					IsLoading = true;
					yield return null;
				}
				IsLoading = false;

				//コマンド実行
				command.CurrentTread = this;
				if (ScenarioPlayer.DebugOutputLog) Debug.Log("Command : " + command.GetType() + " " + labelData.ScenarioLabel + ":" + pageData.PageNo);
				ScenarioPlayer.OnBeginCommand.Invoke(command);
				command.DoCommand(Engine);

				//コマンド実行後にファイルをアンロード
				command.Unload();
				command.CurrentTread = null;

				while (ScenarioPlayer.IsPausing)
				{
					yield return null;
				}
				//コマンドの処理待ち
				while (true)
				{
					command.CurrentTread = this;
					ScenarioPlayer.OnUpdatePreWaitingCommand.Invoke(command);
					if (!command.Wait(Engine))
					{
						break;
					}
					if (ScenarioPlayer.DebugOutputWaiting) Debug.Log("Wait..." + command.GetType());
					ScenarioPlayer.OnUpdateWaitingCommand.Invoke(command);
					command.CurrentTread = null;
					yield return null;
				}
				command.CurrentTread = this;
				if (ScenarioPlayer.DebugOutputCommandEnd) Debug.Log("End :" + command.GetType() + " " + labelData.ScenarioLabel + ":" + pageData.PageNo);
				ScenarioPlayer.OnEndCommand.Invoke(command);
				command.CurrentTread = null;

				Engine.UiManager.IsInputTrig = false;
				Engine.UiManager.IsInputTrigCustom = false;

				if (IsBreakCommand)
				{
					yield break;
				}
				command = pageData.GetCommand(++index);
			}
		}

		//システムパラメーターの変更があった場合にシステムセーブデータとして記憶
		void CheckSystemDataWriteIfChanged()
		{
			if (Engine.Param.HasChangedSystemParam)
			{
				Engine.Param.HasChangedSystemParam = false;
				Engine.SystemSaveData.Write();
			}

		}



		/// <summary>
		/// ページ冒頭のセーブが有効か
		/// </summary>
		internal bool EnableSaveOnPageTop()
		{
			if (!IsMainThread) return false;
			if (Engine.IsSceneGallery) return false;
			switch (Engine.SaveManager.Type)
			{
				case AdvSaveManager.SaveType.Default:
					return true;
				case AdvSaveManager.SaveType.SavePoint:
					return (Engine.Page.PageNo == 0 && Engine.Page.CurrentData.ScenarioLabelData.IsSavePoint);
				default:
					return false;
			}
		}

		/// <summary>
		/// テキスト開始部分のセーブが有効か
		/// （工事中）
		/// </summary>
		internal bool EnableSaveTextTop()
		{
			if (!IsMainThread) return false;
			if (Engine.IsSceneGallery) return false;

			//
//			if (Engine.SaveManager.Type != AdvSaveManager.SaveType.TextTop) return false;

			if (this.WaitManager.IsWaiting) return false;
			if (this.SubThreadList.Count > 0) return false;

			return false;
		}

		//コマンドが中断されたか
		bool IsBreakCommand
		{
			get { return !IsPlaying || JumpManager.IsReserved || (IsMainThread && ScenarioPlayer.IsReservedEndScenario); }
		}

		//登録先にジャンプ
		void JumpToReserved()
		{
			//前回の実行がまだ回ってるかもしれないので止める
			StopAllCoroutines();
			if (JumpManager.SubRoutineReturnInfo != null)
			{
				SubRoutineInfo info = JumpManager.SubRoutineReturnInfo;
				StartCoroutine(CoStartScenario(info.ReturnLabel, info.ReturnPageNo, info.ReturnCommand, false));
			}
			else
			{
				StartCoroutine(CoStartScenario(JumpManager.Label, 0, null, false));
			}
		}

		//サブスレッドを開始
		internal void StartSubThread(string label, string name)
		{
			AdvScenarioThread subTread = this.gameObject.AddComponent<AdvScenarioThread>();
			subTread.Init(ScenarioPlayer, name, this);
			SubThreadList.Add(subTread);
			subTread.StartScenario(label, 0, false);
		}

		//指定の名前のスレッドが動いているか
		internal bool IsPlayingSubThread(string name)
		{
			foreach ( var subThread in SubThreadList)
			{
				if (subThread && subThread.ThreadName == name )
				{
					return subThread.IsPlaying;
				}
			}
			return false;
		}

		//サブスレッドをすべてクリア
		internal void CleaSubTreadList()
		{
			foreach (var subThread in SubThreadList)
			{
				Destroy(subThread);
			}
			SubThreadList.Clear();
		}

		//指定のスレッドを停止
		internal void CancelSubThread(string name)
		{
			foreach (var subThread in SubThreadList)
			{
				if (subThread && subThread.ThreadName == name)
				{
					subThread.Cancel();
				}
			}
		}


		//先読みファイルをクリア
		void ClearPreload()
		{
			//直前の先読みファイルは参照を減算しておく
			foreach (AssetFile file in preloadFileSet)
			{
				file.Unuse(this);
			}
			preloadFileSet.Clear();
		}

		//先読みかけておく
		void UpdatePreLoadFiles(string scenarioLabel, int page)
		{
			//直前までの先読みファイルリスト
			HashSet<AssetFile> lastPreloadFileSet = preloadFileSet;
			//今回の先読みファイルリスト
			preloadFileSet = Engine.DataManager.MakePreloadFileList(scenarioLabel, page, ScenarioPlayer.MaxFilePreload, ScenarioPlayer.PreloadDeep);

			if (preloadFileSet == null) preloadFileSet = new HashSet<AssetFile>();

			//リストに従って先読み
			foreach (AssetFile file in preloadFileSet)
			{
				//先読み
				AssetFileManager.Preload(file, this);
			}

			//直前の先読みファイルのうち、今回の先読みファイルからなくなったものは使用状態を解除する
			foreach (AssetFile file in lastPreloadFileSet)
			{
				//もうプリロードされなくなったリストを作るために
				if (!(preloadFileSet.Contains(file)))
				{
					file.Unuse(this);
				}
			}
		}

		void OnEnable()
		{
//			Debug.Log("OnEnable");
		}

		void OnDisable()
		{
//			Debug.Log("OnDisable");
		}
	}
}
