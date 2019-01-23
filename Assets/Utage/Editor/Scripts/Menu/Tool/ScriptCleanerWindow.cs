// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Utage
{

	public class ScriptCleanerWindow : CustomEditorWindow
	{

		[SerializeField]
		List<Object> directories = new List<Object>();

		protected override bool DrawProperties()
		{
			bool ret = base.DrawProperties();
			bool isEnable = false;
			if (ret)
			{
				for( int i = 0; i <directories.Count; ++i )
				{
					var item = directories[i];
					if(item!=null && !new MainAssetInfo(item).IsDirectory)
					{
						directories[i] = null;
					}
				}
			}
			else
			{
				isEnable = directories.Count > 0;
			}

			EditorGUI.BeginDisabledGroup(!isEnable);
			bool isEdit = GUILayout.Button("Go!", GUILayout.Width(80f));
			EditorGUI.EndDisabledGroup();
			if (isEdit) EditAllScript();

			return ret;
		}

		void EditAllScript()
		{
			foreach (Object item in directories)
			{
				if (item == null) continue;
				EditDirectory(new MainAssetInfo(item));
			}
			AssetDatabase.Refresh();
		}

		void EditDirectory(MainAssetInfo directory)
		{
			if (!directory.IsDirectory) return;

			foreach (MainAssetInfo asset in directory.GetAllChildren())
			{
				if (asset.Asset is MonoScript || asset.Asset is Shader || asset.Asset is TextAsset)
				{
					EditScript(asset);
				}
			}
		}

		void EditScript(MainAssetInfo asset)
		{
			try
			{
				string text = File.ReadAllText(asset.FullPath);
#if false
				if (text.Contains("\r\n"))
				{
					File.WriteAllText(asset.FullPath, text.Replace("\r\n", "\n"), System.Text.Encoding.UTF8);
					Debug.Log(asset.FullPath);
				}
#else
				if (text.Contains("\n"))
				{
					File.WriteAllText(asset.FullPath, text.Replace("\n", "\r\n").Replace("\r\r", "\r"), System.Text.Encoding.UTF8);
					Debug.Log(asset.FullPath);
				}
#endif

			}
			catch (System.Exception e)
			{
				Debug.LogError(e.Message);
			}
		}
	}
}

