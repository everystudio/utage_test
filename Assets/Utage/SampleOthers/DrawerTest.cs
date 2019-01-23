// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
#pragma warning disable 0414

using UnityEngine;
using UnityEngine.UI;
using Utage;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Utage
{
	/// <summary>
	/// テキストSEのサンプル
	/// </summary>
	[AddComponentMenu("Utage/Sample/DrawerTest")]
	public class DrawerTest : MonoBehaviour
	{
		[Header("ヘッダー表示")]
		[HelpBox("HelpBoxは、ヘルプボックスを表示するデコレーターです。\n何行にもわたるような、長いテキストにも自動改行で対応します", HelpBoxAttribute.Type.Info)]
		public string helpBox;

		[System.Serializable]
		public class DecoratorTest
		{
			[HelpBox("HelpBoxは、ヘルプボックスを表示するデコレーターです。\n何行にもわたるような、長いテキストにも自動改行で対応します", HelpBoxAttribute.Type.Warning)]
			public string helpBox;

			[HelpBox("Hide。非表示にします", HelpBoxAttribute.Type.Info)]
			[Hide]
			public int hide;

			[HelpBox("NotEditable。表示のみで編集を不可能にします", HelpBoxAttribute.Type.Info)]
			[NotEditable]
			public int notEditable;

			[System.Flags]
			public enum Flags
			{
				Flag0 = 0x1 << 0,
				Flag1 = 0x1 << 1,
				Flag2 = 0x1 << 2,
			};
			[HelpBox("EnumFlags。フラグタイプのenum表示です。マスク（チェックボックス）表示になります", HelpBoxAttribute.Type.Info)]
			[EnumFlags]
			public Flags flags;

			public enum LimitEnum
			{
				Type0,
				Type1,
				Type2,
			};
			[HelpBox("LimitEnum。enumのうち限られたものだけ表示します", HelpBoxAttribute.Type.Info)]
			[LimitEnum("Type0", "Type2")]
			public LimitEnum lmitEnum;

			[HelpBox("StringPopup。指定の文字列のポップアップリストを表示します", HelpBoxAttribute.Type.Info)]
			[StringPopup("hoge", "hoge2")]
			public string stringPopup;

			[HelpBox("StringPopupFunction。指定した名前の関数から取得できる、ポップアップリストを表示します", HelpBoxAttribute.Type.Info)]
			[StringPopupFunction("GetStrings")]
			public string stringPopupFunction;

#if UNITY_EDITOR
			[HelpBox("Folderアセットの登録。Editorでのみ使うことを想定してます", HelpBoxAttribute.Type.Info)]
			[Folder]
			public Object Folder;
#endif


			[HelpBox("ボタンを表示します。", HelpBoxAttribute.Type.Info)]
			[Button("OnPushButton", "Push!")]
			public string pushButton = "HogeHoge!";

			[HelpBox("プロパティの横にボタンを追加します。", HelpBoxAttribute.Type.Info)]
			[AddButton("OnPushAddButton", " Add Button!")]
			public string addButton;

			[HelpBox("パスの文字列を設定するために、ファイルダイアログを開きます", HelpBoxAttribute.Type.Info)]
			[PathDialog(PathDialogAttribute.DialogType.File)]
			public string path;

			[HelpBox("指定範囲のMinMax値を設定", HelpBoxAttribute.Type.Info)]
			[SerializeField, MinMax(0, 10)]
			MinMaxFloat intervalTime = new MinMaxFloat() { Min = 3, Max = 5 };

			[SerializeField, MinMax(0, 10)]
			MinMaxInt intervalTimeInt = new MinMaxInt() { Min = 3, Max = 5 };


			[HelpBox("OverridePropertyDraw。プロパティドロワーを独自のメソッドで上書きします", HelpBoxAttribute.Type.Info)]
			[SerializeField, OverridePropertyDraw("OnGuiOverridePropertyDraw")]
			int overridePropertyDraw = 0;
		}
		public DecoratorTest decoratorTest;


		[SerializeField]
		bool isOverridePropertyDrawEditable = false;
#if UNITY_EDITOR
		public void OnGuiOverridePropertyDraw(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginDisabledGroup(!isOverridePropertyDrawEditable);
			EditorGUI.PropertyField(position, property, label);
			EditorGUI.EndDisabledGroup();
		}
#endif

		public List<string> GetStrings()
		{
			return new List<string> { "str0", "str1" };
		}

		public void OnPushButton()
		{
			Debug.Log("OnPushButton");
		}

		public void OnPushAddButton()
		{
			Debug.Log("OnPushAddButton");
		}
	}
}

