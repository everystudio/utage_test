﻿// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utage
{
	/// <summary>
	/// [EnumFlags]アトリビュート
	/// [Flags]用の表示
	/// </summary>
	public class EnumFlagsAttribute : PropertyAttribute	{}

#if UNITY_EDITOR
    /// <summary>
    /// [EnumFlags]を表示するためのプロパティ拡張
    /// </summary>
    [CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
    public class EnumFlagsDrawer : PropertyDrawerEx<EnumFlagsAttribute>
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.intValue = EditorGUI.MaskField(position, label, property.intValue, property.enumNames);
        }
    }
#endif
}
