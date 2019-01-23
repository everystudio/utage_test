// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Utage
{

	public class AboutUtage: EditorWindow
	{
		const string url = "http://madnesslabo.net/utage/";
		const string documentUrl = "http://madnesslabo.net/utage/?page_id=225";
		const string sampleUrl = "http://madnesslabo.net/utage/?page_id=788";
		const string version3FeturesUrl = "http://madnesslabo.net/utage/?page_id=8546";

		void OnGUI()
		{
			GUILayout.Label("Utage version " + VersionUtil.Version);

			GUILayout.Space(10);
			GUIStyle style = GUI.skin.label;
			style.richText = true;
			if (GUILayout.Button( ColorUtil.AddColorTag("WebSite",Color.blue), style ))
			{
				Application.OpenURL(url);
			}
			GUILayout.Space(8);
			if (GUILayout.Button(ColorUtil.AddColorTag("Document", Color.blue), style))
			{
				Application.OpenURL(documentUrl);
			}
			GUILayout.Space(8);
			if (GUILayout.Button(ColorUtil.AddColorTag("Sample Links", Color.blue), style))
			{
				Application.OpenURL(sampleUrl);
			}

			GUILayout.Space(8);
			if (GUILayout.Button(ColorUtil.AddColorTag("Version3 Fetures", Color.blue), style))
			{
				Application.OpenURL(version3FeturesUrl);
			}
		}
	}
}