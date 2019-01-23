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
	/// 文字列リストから一つを選択するポップアップを表示する。文字列リストは、指定した関数名で取得できる
	/// </summary>
	public class StringPopupIndexedAttribute : PropertyAttribute
	{
		public string Function { get; set; }
		public StringPopupIndexedAttribute(string function)
		{
			Function = function;
		}
	}

#if UNITY_EDITOR
	/// <summary>
	/// [StringPopup]を表示するためのプロパティ拡張
	/// </summary>
	[CustomPropertyDrawer(typeof(StringPopupIndexedAttribute))]
    class StringPopupIndexedDrawer : PropertyDrawerEx<StringPopupIndexedAttribute>
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //メソッド呼び出し
            List<string> stringList = CallFunction<List<string>>(property, Attribute.Function, null);
            if (stringList == null || stringList.Count <= 0)
            {
                EditorGUI.PropertyField(position, property, label);
            }
            else
            {
                int index = property.intValue;
                if (index < 0) index = 0;
                index = EditorGUI.Popup(position, label.text, index, stringList.ToArray());
                property.intValue = index;
            }
        }
    }
#endif
}
