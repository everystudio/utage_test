// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace Utage
{
    /// <summary>
    /// [PathDialog]アトリビュート
    /// パス用の文字列を、選択ダイアログ用のボタンつきで表示する
    /// </summary>
    public class PathDialogAttribute : PropertyAttribute
    {
        public enum DialogType
        {
            Directory,
            File,
        };

        public DialogType Type { get; private set; }
        public string Extention { get; private set; }

        public PathDialogAttribute(DialogType type)
        {
            this.Type = type;
            this.Extention = "";
        }
        public PathDialogAttribute(DialogType type, string extention)
        {
            this.Type = type;
            this.Extention = extention;
        }
    }

#if UNITY_EDITOR

	/// <summary>
	/// [PathDialog]表示のためのプロパティ描画
	/// </summary>
	[CustomPropertyDrawer(typeof(PathDialogAttribute))]
	public class PathDialogDrawer : PropertyDrawerEx<PathDialogAttribute>
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			float buttonWide = 64.0f;
			float spcae = 8.0f;
			position.width -= buttonWide + spcae;
			EditorGUI.PropertyField(position, property, new GUIContent(property.displayName));
			position.x = position.xMax + spcae;
			position.width = buttonWide;
			if (GUI.Button(position, "Select"))
			{
				string path = property.stringValue;
				string dir = string.IsNullOrEmpty(path) ? "" : Path.GetDirectoryName(path);
				string name = string.IsNullOrEmpty(path) ? "" : Path.GetFileName(path);
				switch (Attribute.Type)
				{
					case PathDialogAttribute.DialogType.Directory:
						path = EditorUtility.OpenFolderPanel("Select Directory", dir, name);
						break;
					case PathDialogAttribute.DialogType.File:
						path = EditorUtility.OpenFilePanel("Select File", dir, Attribute.Extention);
						break;
					default:
						Debug.LogError("Unkonwn Type");
						break;
				}
				if (!string.IsNullOrEmpty(path))
				{
					property.stringValue = path;
				}
			}
		}
	}

#endif
}
