// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utage
{
	/// <summary>
	/// [NotEditable]アトリビュート
	/// 表示のみで編集を不可能にする
	/// </summary>
	public class NotEditableAttribute : PropertyAttribute
	{
		public string EnablePropertyPath { get; private set; }
		public bool IsEnableProperty { get; private set; }

		public NotEditableAttribute() { }
		public NotEditableAttribute(string enablePropertyPath, bool isEnableProperty = true)
		{
			this.EnablePropertyPath = enablePropertyPath;
			this.IsEnableProperty = isEnableProperty;
		}
	}

#if UNITY_EDITOR

	/// <summary>
	/// [NotEditable]を表示するためのプロパティ拡張
	/// </summary>
	[CustomPropertyDrawer(typeof(NotEditableAttribute))]
	public class NotEditableDrawer : PropertyDrawerEx<NotEditableAttribute>
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			bool isNotEditable = true;
			if (!string.IsNullOrEmpty(Attribute.EnablePropertyPath))
			{
				string path = "";
				//子のプロパティの場合ルートのパスが必要になる
				int lastIndex = property.propertyPath.LastIndexOf('.');
				if (lastIndex > 0)
				{
					path += property.propertyPath.Substring(0, lastIndex) + ".";
				}
				path += Attribute.EnablePropertyPath;

				SerializedProperty enalePropery = property.serializedObject.FindProperty(path);
				if (enalePropery != null)
				{
					isNotEditable = enalePropery.boolValue ^ Attribute.IsEnableProperty;
				}
				else
				{
					Debug.LogError("Not found " + path);
				}
			}
			EditorGUI.BeginDisabledGroup(isNotEditable);
			EditorGUI.PropertyField(position, property, label);
			EditorGUI.EndDisabledGroup();
		}
	}

#endif
}
