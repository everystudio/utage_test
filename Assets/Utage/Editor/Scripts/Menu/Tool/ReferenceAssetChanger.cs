// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Utage
{

	//アセットの参照を別のアセットに変える
	public class ReferenceAssetChanger : EditorWindow
	{

		enum Mode
		{
			InitSetting,
			ChangeSetting,
		}
		Mode mode;

		void OnGUI()
		{
			switch (mode)
			{
				case Mode.InitSetting:
					OnGuiInitSetting();
					break;
				case Mode.ChangeSetting:
					OnGuiChangeSetting();
					break;
			}
		}

		Object targetDirectry;
		Object srcAsset;
		Object dstAsset;
		class RefereceInfo
		{
			public Object obj;
			public bool isEnable = true;

			public RefereceInfo(Object obj)
			{
				this.obj = obj;
			}

			public bool IsEditFile()
			{
				return isEnable;
			}
		};

		List<RefereceInfo> refereceInfoList = new List<RefereceInfo>();
		Vector2 scrollPosition;

		string dictionaryPath="";
		void OnGuiInitSetting()
		{
			srcAsset = EditorGUILayout.ObjectField("from", srcAsset, typeof(Object), true );

			GUILayout.Label(string.IsNullOrEmpty(this.dictionaryPath) ? "Project Dictionary" : this.dictionaryPath);
			if (GUILayout.Button("Select", GUILayout.Width(100)))
			{
				string convertPath = this.dictionaryPath;
				string dir = string.IsNullOrEmpty(convertPath) ? "" : Path.GetDirectoryName(convertPath);
				string name = string.IsNullOrEmpty(convertPath) ? "" : Path.GetFileName(convertPath);
				string path = EditorUtility.SaveFolderPanel("Select folder", dir, name);
				if (!string.IsNullOrEmpty(path))
				{
					this.dictionaryPath = path;
				}
			}

			EditorGUI.BeginDisabledGroup(srcAsset == null);
			if (GUILayout.Button("Find", GUILayout.Width(100)))
			{
				refereceInfoList.Clear();
				foreach (Component component in UtageEditorToolKit.FindReferencesComponentsInScene(srcAsset))
				{
					refereceInfoList.Add(new RefereceInfo(component));
				}
				foreach (Object obj in UtageEditorToolKit.FindReferencesAssetsInProject(dictionaryPath, srcAsset))
				{
					refereceInfoList.Add(new RefereceInfo(obj));
				}

				mode = Mode.ChangeSetting;
			}
			EditorGUI.EndDisabledGroup();
		}

		void OnGuiChangeSetting()
		{
			OnGuiInitSetting();

			if (srcAsset != null)
			{
				dstAsset = EditorGUILayout.ObjectField("to", dstAsset, srcAsset.GetType(), true);
			}
			
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
			foreach (RefereceInfo info in refereceInfoList)
			{
				EditorGUILayout.BeginHorizontal();
				info.isEnable = GUILayout.Toggle(info.isEnable,"");
				EditorGUILayout.ObjectField(info.obj, typeof(Object),true);
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndScrollView();

			EditorGUI.BeginDisabledGroup(dstAsset==null);
			if (GUILayout.Button("Change!", GUILayout.Width(80f)))
			{
				ChangeAll();
			}
			EditorGUI.EndDisabledGroup();

		}

		void ChangeAll()
		{
			foreach (RefereceInfo info in refereceInfoList)
			{
				if (info.isEnable)
				{
					UtageEditorToolKit.ReplaceSerializedProperties(new SerializedObject(info.obj), srcAsset, dstAsset);
				}
			}
			AssetDatabase.Refresh();
		}

		public static List<Object> FindReferences(Object srcAsset, string dictionaryPath)
		{
			List<Object> list = new List<Object>();
			UtageEditorToolKit.FindReferencesComponentsInScene(srcAsset).ForEach(x => list.Add(x));
			list.AddRange(UtageEditorToolKit.FindReferencesAssetsInProject(dictionaryPath, srcAsset));
			return list;
		}

		public static void ChangeAll(Object srcAsset, Object dstAsset, List<Object> refereceAssets)
		{
			foreach (Object asset in refereceAssets)
			{
				UtageEditorToolKit.ReplaceSerializedProperties(new SerializedObject(asset), srcAsset, dstAsset);
			}
			AssetDatabase.Refresh();
		}

		public static void FindAndChangeAll(Object srcAsset, Object dstAsset, string dictionaryPath)
		{
			ChangeAll(srcAsset, dstAsset, FindReferences(srcAsset, dictionaryPath));
		}
	}
}