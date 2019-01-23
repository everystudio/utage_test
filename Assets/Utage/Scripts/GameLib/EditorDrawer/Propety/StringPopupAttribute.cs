// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utage
{
    /// <summary>
    /// [StringPopup]アトリビュート
    /// 指定の文字列リストから一つを選択するポップアップを表示する
    /// </summary>
    public class StringPopupAttribute : PropertyAttribute
    {
        public List<string> StringList { get { return stringList; } }
        List<string> stringList = new List<string>();
        public StringPopupAttribute(params string[] args)
        {
            stringList.AddRange(args);
        }
    }

#if UNITY_EDITOR
	/// <summary>
	/// [StringPopup]を表示するためのプロパティ拡張
	/// </summary>
	[CustomPropertyDrawer(typeof(StringPopupAttribute))]
    class StringListDrawer : PropertyDrawerEx<StringPopupAttribute>
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            List<string> stringList = new List<string>(Attribute.StringList);
            if (stringList == null || stringList.Count <= 0)
            {
                EditorGUI.PropertyField(position, property, label);
            }
            else
            {
                string oldStr = property.stringValue;
                int index = stringList.FindIndex(x => x == oldStr);
                if (index < 0) index = 0;
                index = EditorGUI.Popup(position, label.text, index, stringList.ToArray());
                property.stringValue = stringList[index];
            }
        }
    }
#endif
}
