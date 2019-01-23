// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Utage
{

	public class SelectableColorChanger : EditorWindow
	{

		class GameObjectInfo
		{
			public GameObject go;
			public bool isEnable = true;

			public GameObjectInfo(GameObject go)
			{
				this.go = go;
			}

			public bool IsEditable()
			{
				return isEnable;
			}
		};

		List<GameObjectInfo> infoList = new List<GameObjectInfo>();
		Vector2 scrollPosition;
		void OnGUI()
		{

			bool isEnable = FindGameObjects();
			if (isEnable)
			{
				scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
				foreach (GameObjectInfo info in infoList)
				{
					EditorGUILayout.ObjectField(info.go,typeof(GameObject),true);
				}
				EditorGUILayout.EndScrollView();
			}
			EditorGUI.BeginDisabledGroup(!isEnable);
			bool isEdit = GUILayout.Button("Go!", GUILayout.Width(80f));
			EditorGUI.EndDisabledGroup();
			if (isEdit) EditAll();
		}

		void EditAll()
		{
			foreach (GameObjectInfo info in infoList)
			{
				EditGameObject(info);
			}
		}
		bool FindGameObjects()
		{
			Object[] objets = Selection.GetFiltered (typeof(GameObject), SelectionMode.Deep);

			infoList.Clear ();
			foreach( Object obj in objets )
			{
				GameObject go = obj as GameObject;
				if(go!=null)
				{
					Selectable selecatable = go.GetComponent<Selectable>();
					if (selecatable)
					{
						infoList.Add( new GameObjectInfo(go) );
					}
				}
			}

			return infoList.Count > 0;
		}

		bool EditGameObject(GameObjectInfo info)
		{
			if (!info.IsEditable()) return false;

			try
			{
				Selectable selectable = info.go.GetComponent<Selectable>();
				if (selectable == null)
				{
					Debug.LogError("Not Found " + info.go.name);
				}
				else
				{
					selectable.colors = ColorBlock.defaultColorBlock;
				}
				return true;
			}
			catch(System.Exception e )
			{
				Debug.LogError(e.Message);
				return false;
			}
		}
	}
}