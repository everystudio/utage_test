// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utage
{

#if UNITY_EDITOR
	/// <summary>
	/// プロパティ拡張を使いやすくるするための基底クラス
	/// </summary>
	public class PropertyDrawerEx : PropertyDrawer
	{
		//コンポーネントやScriptableObject（つまり、UnityEngine.Object）にあるメソッドを名前で呼び出す
		public void CallFunction(SerializedProperty property, string functionName, object[] args = null)
		{
			DrawerUtil.CallFunction(property, functionName, args);
		}

		//コンポーネントやScriptableObject（つまり、UnityEngine.Object）にあるメソッドを名前で呼び出す
		public T CallFunction<T>(SerializedProperty property, string functionName, object[] args = null)
		{
			return DrawerUtil.CallFunction<T>(property, functionName, args);
		}

		//コンポーネントやScriptableObject（つまり、UnityEngine.Object）を取得
		public T TargetObject<T>(SerializedProperty property) where T : Object
		{
			return DrawerUtil.TargetObject<T>(property);
		}

		//子要素をすべて水平に描画（[System.Serializable]なものの描画に使う）		
		public void DrawHolizontalChildren(Rect position, SerializedProperty property, GUIContent label, float spcae = 8)
		{
			DrawerUtil.DrawHolizontalChildren(position, property, label, spcae);
		}

		//フィールドを取得
		public T GetField<T>(SerializedProperty property)
		{
			return DrawerUtil.GetField<T>(property);
		}

		public void SetDirty(SerializedProperty property)
		{
			EditorUtility.SetDirty(property.serializedObject.targetObject);
		}

	}

	/// <summary>
	/// プロパティ拡張を使いやすくるするための基底クラス
	/// Genericでアトリビュートの型を指定
	/// </summary>
	public class PropertyDrawerEx<T> : PropertyDrawerEx where T : PropertyAttribute
	{
		public T Attribute { get { return (this.attribute as T); } }
	}
#endif
}
