// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections.Generic;

namespace Utage
{
	//シーン変更したときに呼ばれる
	[InitializeOnLoad]
	public static class AdvSceneChecker
	{
		static AdvSceneChecker()
		{
			PostProcessEditorSceneChanged.CallbackChangeScene += OnChangeScene;
		}

		static void OnChangeScene()
		{
			AdvEditorSettingWindow editorSetting = AdvEditorSettingWindow.GetInstance();
			if ( UnityEngine.Object.ReferenceEquals(editorSetting,null)) return;
			AdvEngine engine = null;
			AdvEngineStarter starter = null;

			//宴のシーンが切り替わったら、自動でプロジェクトを変更するか
			if (editorSetting.AutoChangeProject)
			{
				if (engine == null) engine = UtageEditorToolKit.FindComponentAllInTheScene<AdvEngine>();
				starter = UtageEditorToolKit.FindComponentAllInTheScene<AdvEngineStarter>();
				if (engine == null || starter == null) return;

				CheckCurrentProject(engine, starter);
			}

			//宴のシーンが切り替わったら、自動でシーンのチェックをするか
			if (editorSetting.AutoCheckScene)
			{
				engine = UtageEditorToolKit.FindComponentAllInTheScene<AdvEngine>();
				if (engine == null) return;

				//Unityのバージョンアップによる致命的な不具合をチェックする
				if (editorSetting.AutoCheckUnityVersionUp && CheckVersionUpScene(engine))
				{
					if (EditorUtility.DisplayDialog(
						"UTAGE Version Up Scene"
						, LanguageSystemText.LocalizeText(SystemText.VersionUpScene)
						, LanguageSystemText.LocalizeText(SystemText.Yes)
						, "Cancel")
						)
					{
						VersionUpScene(engine);
					}
				}

				//starterに登録されているバージョンチェック
				if (starter != null && !starter.EditorCheckVersion() )
				{
					AdvScenarioDataProject project = starter.ScenarioDataProject as AdvScenarioDataProject;
					if (project != null)
					{
						if (EditorUtility.DisplayDialog(
							"UTAGE Version Up Sccenario"
							, LanguageSystemText.LocalizeText(SystemText.VersionUpScenario)
							, LanguageSystemText.LocalizeText(SystemText.Yes)
							, "Cancel")
							)
						{
							//バージョンアップ
							AdvScenarioDataBuilderWindow.Import();
							starter.Scenarios = project.Scenarios;
							starter.EditorVersionUp();
						}
					}
				}
			}
		}


		//バージョンアップでシーンを修正する必要があるかチェック
		static bool CheckVersionUpScene(AdvEngine engine)
		{
			if( CheckVersionUpSceneToUnity52(engine) ) return true;
			if (CheckVersionUpSceneToUtage25(engine)) return true;

			return false;
		}

		//バージョンアップでシーンを修正する必要があるかチェック
		static void VersionUpScene(AdvEngine engine)
		{
			VersionUpSceneToUnity52(engine);
			VersionUpSceneToUtage25(engine);
		}

		//Uity52で発生したエラー対応
		static bool CheckVersionUpSceneToUnity52(AdvEngine engine)
		{
			//Graphicのないマスクコンポーネントを削除
			foreach( Mask mask in engine.GetComponentsInChildren<Mask>(true) )
			{
				if (mask.GetComponents<Graphic>().Length <= 0)
				{
					return true;
				}
			}

			//ScrollBarとScrollRectの重複を削除
			foreach (Scrollbar scrollbar in engine.GetComponentsInChildren<Scrollbar>(true))
			{
				if (scrollbar.GetComponent<ScrollRect>()!=null)
				{
					return true;
				}
			}

			return false;
		}

		//Uity52で発生したエラー対応
		static void VersionUpSceneToUnity52(AdvEngine engine)
		{
			//Graphicのないマスクコンポーネントを削除
			List<Mask> maskList = new List<Mask>();
			foreach( Mask mask in engine.GetComponentsInChildren<Mask>(true) )
			{
				if (mask.GetComponents<Graphic>().Length <= 0)
				{
					maskList.Add(mask);
				}
			}

			//ScrollBarとScrollRectの重複を削除
			List<Scrollbar> scrollbarList = new List<Scrollbar>();
			foreach (Scrollbar scrollbar in engine.GetComponentsInChildren<Scrollbar>(true))
			{
				if (scrollbar.GetComponent<ScrollRect>()!=null)
				{
					scrollbarList.Add(scrollbar);
				}
			}

			if (scrollbarList.Count > 0 || maskList.Count > 0)
			{
				if (EditorUtility.DisplayDialog(
					"UTAGE Version Up Scene"
					, LanguageSystemText.LocalizeText(SystemText.VersionUpScene)
					, LanguageSystemText.LocalizeText(SystemText.Yes)
					, "Cancel")
					)
				{
					foreach(var item in scrollbarList)
					{
						Object.DestroyImmediate(item);
					}
					foreach (var item in maskList)
					{
						Object.DestroyImmediate(item);
					}
                    WrapperUnityVersion.SaveScene();
				}
			}
		}

		//宴2.5対応
		static bool CheckVersionUpSceneToUtage25(AdvEngine engine)
		{
			AdvMessageWindowManager manager = UtageEditorToolKit.FindComponentAllInTheScene<AdvMessageWindowManager>();
			if (manager == null) return true;

			AdvUiManager uiManager = UtageEditorToolKit.FindComponentAllInTheScene<AdvUiManager>();
			if (uiManager == null) return false;

			AdvUguiMessageWindowManager mangaer = UtageEditorToolKit.FindComponentAllInTheScene<AdvUguiMessageWindowManager>();
			return (mangaer == null);
		}
		//宴2.5対応
		static void VersionUpSceneToUtage25(AdvEngine engine)
		{
			if (engine.MessageWindowManager == null)
			{
			}
			AdvUguiManager uguiManager = UtageEditorToolKit.FindComponentAllInTheScene<AdvUguiManager>();
			if (uguiManager == null) return;
			uguiManager.GetMessageWindowManagerCreateIfMissing();
		}

		//現在の宴プロジェクトをチェック
		static void CheckCurrentProject(AdvEngine engine, AdvEngineStarter starter)
		{
			AdvScenarioDataProject project = starter.ScenarioDataProject as AdvScenarioDataProject;
			if (project == null)
			{
				Selection.activeObject = starter;
				Debug.LogWarning("Set using project asset to 'Editor > Scenario Data Project' ", starter);
				return;
			}

			if (AdvScenarioDataBuilderWindow.ProjectData != project)
			{
				AdvScenarioDataBuilderWindow.ProjectData = project;
			}
		}
	}
}
