// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utage
{

	/// <summary>
	/// [AddButtonAttribute]アトリビュート
	/// ボタンを追加表示する
	/// </summary>
	public class AddButtonAttribute : PropertyAttribute
	{
		/// <summary>
		/// ボタンが押されたとき呼ばれる関数名
		/// </summary>
		public string Function { get; set; }

		/// ボタンの名前
		/// </summary>
		public string Text { get; set; }

		public AddButtonAttribute(string function, string text = "Button", int order = 0)
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
	[CustomPropertyDrawer(typeof(AddButtonAttribute))]
	public class AddButtonDrawer : PropertyDrawerEx<AddButtonAttribute>
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (!string.IsNullOrEmpty(Attribute.Text))
			{
				label = new GUIContent(Attribute.Text);
			}

			//ボタンのラベル
			GUIContent buttonLabel = new GUIContent(Attribute.Text);
			//子要素のラベル部分の表示幅を調整
			float buttonWidth = GUI.skin.button.CalcSize(buttonLabel).x;

			float spcae = 8.0f;
			position.width -= buttonWidth + spcae;
			EditorGUI.PropertyField(position, property, new GUIContent(property.displayName));

			position.x = position.xMax + spcae;
			position.width = buttonWidth;
			if (GUI.Button(position, buttonLabel))
			{
				this.CallFunction(property, Attribute.Function);
			}
		}
	}
#endif
}
