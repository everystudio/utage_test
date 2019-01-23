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
	/// デコレーター表示を使いやすくるするための基底クラス
	/// </summary>
	public class DecoratorDrawerEx : DecoratorDrawer
	{
		//コンポーネントやScriptableObject（つまり、UnityEngine.Object）にあるメソッドを名前で呼び出す
		public T1 CallFunction<T1>(SerializedProperty property, string functionName, object[] args = null)
		{
			return DrawerUtil.CallFunction<T1>(property, functionName, args);
		}

		//コンポーネントやScriptableObject（つまり、UnityEngine.Object）を取得
		public T1 TargetObject<T1>(SerializedProperty property) where T1 : Object
		{
			return DrawerUtil.TargetObject<T1>(property);
		}

		//子要素をすべて水平に描画（[System.Serializable]なものの描画に使う）		
		public void DrawHolizontalChildren(Rect position, SerializedProperty property, GUIContent label, float spcae = 8)
		{
			DrawerUtil.DrawHolizontalChildren(position, property, label, spcae);
		}

		public Rect ToIndentedRect(Rect position)
		{
			return EditorGUI.IndentedRect(position);
		}

		//インデント済みの表示幅を取得
		public float GetIndentedViewWidth( int indentLevel)
		{
			//謎の数字だよ！
			int offset = 19 + EditorGUI.indentLevel * 15;
			return EditorGUIUtility.currentViewWidth - offset;
		}
	}

	/// <summary>
	/// デコレーター表示を使いやすくるするための基底クラス
	/// Genericでアトリビュートの型を指定
	/// </summary>
	public class DecoratorDrawerEx<T> : DecoratorDrawerEx where T : PropertyAttribute
	{
		public T Attribute { get { return (this.attribute as T); } }
	}
#endif
}
