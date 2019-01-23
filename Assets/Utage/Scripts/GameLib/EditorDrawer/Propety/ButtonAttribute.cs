// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utage
{

	/// <summary>
	/// [Button]アトリビュート（デコレーター）
	/// ボタンを表示する
	/// </summary>
	public class ButtonAttribute : PropertyAttribute
	{
		/// <summary>
		/// ボタンが押されたとき呼ばれる関数名
		/// </summary>
		public string Function { get; set; }

		/// ボタンの名前
		/// </summary>
		public string Text { get; set; }

		public ButtonAttribute(string function, string text = "", int order = 0)
		{
			Function = function;
			Text = text;
			this.order = order;
		}
	}

#if UNITY_EDITOR
	/// <summary>
	/// [Button]を表示するためのプロパティ拡張
	/// </summary>
	[CustomPropertyDrawer(typeof(ButtonAttribute))]
	public class ButtonDrawer : PropertyDrawerEx<ButtonAttribute>
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (!string.IsNullOrEmpty(Attribute.Text))
			{
				label = new GUIContent(Attribute.Text);
			}
			if (GUI.Button(EditorGUI.IndentedRect(position), label))
			{
				this.CallFunction(property, Attribute.Function);
			}
		}
	}
#endif
}
