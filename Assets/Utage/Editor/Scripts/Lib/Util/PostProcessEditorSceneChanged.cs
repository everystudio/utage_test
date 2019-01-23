// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Utage
{
	//シーン変更したときに呼ばれる
	[InitializeOnLoad]
	public static class PostProcessEditorSceneChanged
	{
		//Unity起動時
		static PostProcessEditorSceneChanged()
		{
#if UNITY_2018_1_OR_NEWER
			EditorApplication.hierarchyChanged += OnHierarchyWindowChanged;
#else
			EditorApplication.hierarchyWindowChanged += OnHierarchyWindowChanged;
#endif
		}

		//ヒエラルキー変更時
		static void OnHierarchyWindowChanged()
		{
			if (Application.isPlaying) return;
			if (string.IsNullOrEmpty(WrapperUnityVersion.currentScene)) return;

//			Debug.Log("OnHierarchyWindowChanged " + lastScene);
			if (lastScene != WrapperUnityVersion.currentScene)
			{
				OnChangeScene();
				lastScene = WrapperUnityVersion.currentScene;
			}
		}

		//シーンが変更された
		static string lastScene = "";
		static public System.Action CallbackChangeScene;
		static void OnChangeScene()
		{
			if (CallbackChangeScene != null)
			{
				CallbackChangeScene();
			}
//			Debug.Log( string.Format( "<color=red>OnChangeScene</color> {0} -> {1}",lastScene,EditorApplication.currentScene));
		}
	}
}
