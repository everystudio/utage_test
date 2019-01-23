// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utage
{
    /// <summary>
    /// [Hide]アトリビュート
    /// 条件によって非表示にする
    /// </summary>
    public class HideAttribute : PropertyAttribute
    {
		public string Function { get; set; }
		public HideAttribute(string function = "")
        {
			this.Function = function;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// [Hide]を表示するためのプロパティ拡張
    /// </summary>
    [CustomPropertyDrawer(typeof(HideAttribute))]
    public class HideDrawer : PropertyDrawerEx<HideAttribute>
	{
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!IsHide(property))
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }

        bool IsHide(SerializedProperty property)
        {
			if (string.IsNullOrEmpty(Attribute.Function))
			{
				return true;
			}
			else
			{
				return CallFunction<bool>(property, Attribute.Function);
			}
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (IsHide(property))
            {
                return 0;
            }
            else
            {
                return base.GetPropertyHeight(property, label);
            }
        }
    }
#endif

}
