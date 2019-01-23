// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utage
{
    /// <summary>
    /// [OverridePropertyDraw]アトリビュート
    /// プロパティの描画をコンポーネントやScriptaleObjectの定義クラス内でオーバーライドする
    /// </summary>
    public class OverridePropertyDrawAttribute : PropertyAttribute
    {
        public string Function { get; set; }
        public OverridePropertyDrawAttribute(string function)
        {
            Function = function;
        }
    }

#if UNITY_EDITOR
	/// <summary>
	/// [OverridePropertyDraw]を表示するためのプロパティ拡張
	/// </summary>
	[CustomPropertyDrawer(typeof(OverridePropertyDrawAttribute))]
    class OverridePropertyDrawer : PropertyDrawerEx<OverridePropertyDrawAttribute>
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            object[] args = new object[] { position, property, label };
            //メソッド呼び出し
            CallFunction<List<string>>(property, Attribute.Function, args);
        }
    }
#endif
}
