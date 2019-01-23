// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEditor;
using UnityEngine;

namespace Utage
{
	//プロジェクトビュー内で、スクリプタブルオブジェクトの作成などをサポート
	public class CustomCreateAsset : EditorWindow
	{
		const string root = "Assets/Create/Utage/";
		[MenuItem(root + "CustomProjectSetting")]
		static public void CreateCustomProjectSetting()
		{
			UtageEditorToolKit.CreateNewUniqueAsset<CustomProjectSetting>();
		}

		[MenuItem(root + "LanguageManager")]
		static public void CreateLanguageManager()
		{
			UtageEditorToolKit.CreateNewUniqueAsset<LanguageManager>();
		}

		[MenuItem(root + "TextSettings")]
		static public void CreateTextSettings()
		{
			UtageEditorToolKit.CreateNewUniqueAsset<UguiNovelTextSettings>();
		}
		
		[MenuItem(root + "EmojiData")]
		static public void CreateEmojiData()
		{
			UtageEditorToolKit.CreateNewUniqueAsset<UguiNovelTextEmojiData>();
		}
	}
}