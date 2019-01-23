// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;

#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
#endif

namespace Utage
{

#if UNITY_EDITOR
	public class DrawerUtil
	{
		//コンポーネントやScriptableObject（つまり、UnityEngine.Object）を取得
		public static Object TargetObject(SerializedProperty property)
		{
			return property.serializedObject.targetObject;
		}
		public static T TargetObject<T>(SerializedProperty property) where T : Object
		{
			return TargetObject(property) as T;
		}
	
		//コンポーネントやScriptableObjectにあるメソッドを名前で呼び出す
		public static object CallFunction(SerializedProperty property, string functionName, object[] args = null)
		{
			var obj = property.serializedObject.targetObject;       //コンポーネントやScriptableObject
			var type = obj.GetType();                               //その型を取得

			//メソッドを名前で検索
			var method = type.GetMethod(functionName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);              //メソッドを名前で検索
			if (method != null) return method.Invoke(obj, args);                        //メソッド呼び出し

			//プロパティを名前で検索
			var prop = type.GetProperty(functionName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetProperty);
			if (prop != null) return prop.GetValue(obj,args);                        //メソッド呼び出し

			Assert.IsTrue(true,functionName + " is not found in " + type.ToString() );
			return null;
		}

		//フィールドを取得
		internal static T GetField<T>(SerializedProperty property)
		{
			return (T)GetField(property);
		}

		internal static object GetField(SerializedProperty property)
		{
			var obj = property.serializedObject.targetObject;       //コンポーネントやScriptableObject
			var type = obj.GetType();                               //その型を取得

			//フィールドを名前で検索
			var field = type.GetField(property.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);
			if (field != null) return field.GetValue(obj);                        //メソッド呼び出し

			Assert.IsTrue(true, property.name + " is not found in " + type.ToString());
			return null;
		}

		public static T CallFunction<T>(SerializedProperty property, string functionName, object[] args = null)
		{
			return (T)CallFunction(property, functionName, args);
		}


		//子要素をすべて水平に描画（[System.Serializable]なものの描画に使う）		
		public static void DrawHolizontalChildren(Rect position, SerializedProperty property, GUIContent label, float spcae = 8)
		{
			using (var scope = new EditorGUI.PropertyScope(position, label, property))
			{
				//インデント済みの全体矩形を取得
				Rect indentedRect = EditorGUI.IndentedRect(position);
				//インデント記録して、いったん0にする
				int indentLevel = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;

				//子要素の数を取得
				int numChildren = CountChildren(property);
				//矩形を水平に均等分割
				List<Rect> rects = CalcHolizontalRects(indentedRect, numChildren, spcae);

				int i = 0;
				//各子要素を描画
				ForeachChildren(property, (child) =>
				{
					//子要素のラベル
					GUIContent childLabel = new GUIContent(child.displayName);
					//子要素のラベル部分の表示幅を調整
					EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(childLabel).x;
					//子要素を表示
					EditorGUI.PropertyField(rects[i], child, childLabel);
					++i;
				});

				//インデント戻す
				EditorGUI.indentLevel = indentLevel;
			}
		}

		//プロパティを全て描写
		public static void DebugDrawAllPropertiePath(SerializedObject serializedObject)
		{
			SerializedProperty it = serializedObject.GetIterator();
			do
			{
				Debug.Log(it.propertyPath);
			} while (it.NextVisible(true));
		}


		//ヘッダ部分を除く、表示可能なプロパティのカウントを取得
		public static int CountChildren(SerializedProperty property)
		{
			int count = 0;
			ForeachChildren(property, (x) => ++count);
			return count;
		}

		//表示可能な子要素に対してのForeach
		public static void ForeachChildren(SerializedProperty property, System.Action<SerializedProperty> childAction)
		{
			if (!property.hasVisibleChildren) return;

			var child = property.Copy();
			var end = property.Copy().GetEndProperty();
			if (child.Next(true))
			{
				while (!SerializedProperty.EqualContents(child, end))
				{
					childAction(child);
					if (!child.Next(false))
						break;
				}
			}
			return;
		}


		//指定個数ぶん横に分割した矩形のリストを取得
		//spcaeは分割した矩形同士のスペース
		public static List<Rect> CalcHolizontalRects(Rect rect, int num, float spcae = 0)
		{
			if (num <= 0) return new List<Rect> { rect };

			List<Rect> rects = new List<Rect>();

			float w = (rect.width - spcae * (num - 1)) / num;
			float x = rect.x;
			for (int i = 0; i < num; ++i)
			{
				Rect r = rect;
				r.x = x;
				r.width = w;
				rects.Add(r);
				x += w + spcae;
			}
			return rects;
		}

		//表示ラベルを文字数に合わせて広げて表示するPropertyField
		public static bool DrawPropertyFieldExpandLabel(Rect position, SerializedProperty property)
		{
			return DrawPropertyFieldExpandLabel(position, property, property.displayName);
		}

		//表示ラベルを文字数に合わせて広げて表示するPropertyField
		public static bool DrawPropertyFieldExpandLabel(Rect position, SerializedProperty property, string displayName)
		{
			//子要素のラベル
			GUIContent childLabel = new GUIContent(displayName);
			//子要素のラベル部分の表示幅を調整
			float labelWidth = GUI.skin.label.CalcSize(childLabel).x;
			EditorGUIUtility.labelWidth = labelWidth;
			//子要素を表示
			return EditorGUI.PropertyField(position, property, childLabel);
		}

		//表示ラベルを文字数に合わせて広げて表示するPropertyField
		//変更があったときに呼ばれるコールバックつき
		public static bool DrawPropertyFieldExpandLabel(Rect position, SerializedProperty property, System.Action onChanged )
		{
			return DrawPropertyFieldExpandLabel(position, property, property.displayName, onChanged);
		}

		//表示ラベルを文字数に合わせて広げて表示するPropertyField
		//変更があったときに呼ばれるコールバックつき
		public static bool DrawPropertyFieldExpandLabel(Rect position, SerializedProperty property, string displayName, System.Action onChanged)
		{
			EditorGUI.BeginChangeCheck();
			bool ret = DrawPropertyFieldExpandLabel(position, property, displayName);
			if (EditorGUI.EndChangeCheck())
			{
				onChanged();
			}
			return ret;
		}

		//文字列の配列のプロパティをマスクフィールド（つまりチェックボックスグループのかわり）で表示する
		public static void DrawStringArrayPropertyMaskFiled(Rect position, SerializedProperty property, List<string> options)
		{
			int currentMask = 0;
			for ( int i = 0; i < property.arraySize; ++i )
			{
				SerializedProperty child = property.GetArrayElementAtIndex(i);
				int index = options.FindIndex(x => x == child.stringValue);
				if (index >= 0)
				{
					currentMask |= (0x1 << index);
				}
			}
			int mask = EditorGUI.MaskField(position, property.displayName, currentMask, options.ToArray());
			if (mask != currentMask)
			{
				List<string> list = new List<string>();
				for (int i = 0; i < options.Count; ++i)
				{
					int bin = (0x1 << i);
					if ((mask & bin) != bin) continue;
					list.Add(options[i]);
				}
				SetStringArray(property,list);
			}
		}

		//文字列の配列を上書き設定
		internal static void SetStringArray(SerializedProperty property, List<string> list)
		{
			property.arraySize = list.Count;
			for (int i = 0; i < property.arraySize; ++i)
			{
				SerializedProperty child = property.GetArrayElementAtIndex(i);
				child.stringValue = list[i];
			}
		}

		//文字列の配列を取得
		internal static List<string> GetStringList(SerializedProperty property)
		{
			List<string> list = new List<string>();
			for (int i = 0; i < property.arraySize; ++i)
			{
				SerializedProperty child = property.GetArrayElementAtIndex(i);
				list.Add( child.stringValue );
			}
			return list;
		}

	}
#endif
}
