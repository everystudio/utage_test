// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utage
{

	/// <summary>
	/// [UpdateFunction]アトリビュート（デコレーター）
	/// 描画タイミングで指定の関数を呼ぶ（Applyなど最後にUpdateをかけるときに）
	/// </summary>
	public class UpdateFunctionAttribute : PropertyAttribute
	{
		/// <summary>
		/// 呼ばれる関数名
		/// </summary>
		public string Function { get; set; }

		public UpdateFunctionAttribute(string function, int order = 0)
		{
			Function = function;
			this.order = order;
		}
	}

#if UNITY_EDITOR
	/// <summary>
	/// [Button]を表示するためのプロパティ拡張
	/// </summary>
	[CustomPropertyDrawer(typeof(UpdateFunctionAttribute))]
	public class UpdateFunctionDrawer : PropertyDrawerEx<UpdateFunctionAttribute>
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			this.CallFunction(property, Attribute.Function);
		}
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return 0;
		}
	}
#endif
}
