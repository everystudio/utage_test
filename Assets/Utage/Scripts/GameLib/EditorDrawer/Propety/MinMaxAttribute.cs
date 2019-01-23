// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utage
{
	/// <summary>
	/// 最小値と最大値を設定する
	/// </summary>
	public class MinMax<T>
	{
		public T Min { get { return min; } set { min = value; } }
		[SerializeField]
		T min;

		public T Max { get { return max; } set { max = value; } }
		[SerializeField]
		T max;
	}

	[System.Serializable]
	public class MinMaxFloat : MinMax<float>
	{
		public float RandomRange()
		{
			return Random.Range(Min, Max);
		}
		public float Clamp(float value)
		{
			return Mathf.Clamp(value,Min, Max);
		}

		public bool IsInnner(float v)
		{
			return (v >= Min) && (v <= Max);
		}

		public bool IsOver(float v)
		{
			return (v < Min) || (v > Max);
		}
	}

	[System.Serializable]
	public class MinMaxInt : MinMax<int>
	{
		public int RandomRange()
		{
			return Random.Range(Min, Max+1);
		}
		public int Clamp(int value)
		{
			return Mathf.Clamp(value, Min, Max);
		}

		public bool IsInnner(int v)
		{
			return (v >= Min) && (v <= Max);
		}

		public bool IsOver(int v)
		{
			return (v < Min) || (v > Max);
		}
	}


	/// <summary>
	/// [MinMax]アトリビュート
	/// 設定可能な最小値と最大値を設定する
	/// </summary>
	public class MinMaxAttribute : PropertyAttribute
	{
		public float Min { get; private set; }
		public float Max { get; private set; }
		public string MinPropertyName { get; private set; }
		public string MaxPropertyName { get; private set; }

		public MinMaxAttribute(float min, float max, string minPropertyName = "min", string maxPropertyName = "max")
		{
			Min = min;
			Max = max;
			MinPropertyName = minPropertyName;
			MaxPropertyName = maxPropertyName;
		}
	}

#if UNITY_EDITOR
	/// <summary>
	/// [MinMax]を表示するためのプロパティ拡張
	/// </summary>
	[CustomPropertyDrawer(typeof(MinMaxAttribute))]
    class MinMaxDrawer : PropertyDrawerEx<MinMaxAttribute>
	{
		// Draw the property inside the given rect
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			//インデント済みの全体矩形を取得
//			Rect indentedRect = EditorGUI.IndentedRect(position);

			//各プロパティー取得
			var minProperty = property.FindPropertyRelative(Attribute.MinPropertyName);
			var maxProperty = property.FindPropertyRelative(Attribute.MaxPropertyName);
			//表示位置を調整
			var sliderRect = new Rect(position)
			{
				height = position.height * 0.5f
			};
			var labelRect = new Rect(sliderRect)
			{
				x = sliderRect.x + EditorGUIUtility.labelWidth,
				y = sliderRect.y + sliderRect.height,
				width = sliderRect.width - EditorGUIUtility.labelWidth
			};

			bool isFloatMin = (minProperty.propertyType == SerializedPropertyType.Float);
			bool isFloatMax = (maxProperty.propertyType == SerializedPropertyType.Float);

			float min = isFloatMin ? minProperty.floatValue : minProperty.intValue;
			float max = isFloatMax ? maxProperty.floatValue : maxProperty.intValue;
			EditorGUI.BeginChangeCheck();
#if UNITY_5_5_OR_NEWER
			EditorGUI.MinMaxSlider(sliderRect, label, ref min, ref max, Attribute.Min, Attribute.Max);
#else
			EditorGUI.MinMaxSlider(label, sliderRect, ref min, ref max, Attribute.Min, Attribute.Max);
#endif

			if (EditorGUI.EndChangeCheck())
			{
				if (isFloatMin)
				{
					minProperty.floatValue = min;
				}
				else
				{
					minProperty.intValue = Mathf.FloorToInt(min);
				}

				if (isFloatMax)
				{
					maxProperty.floatValue = max;
				}
				else
				{
					maxProperty.intValue = Mathf.FloorToInt(max);
				}
			}

			//インデント記録して、いったん0にする
			int indentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			//MinMaxの数値を描画
			//矩形を水平に均等分割
			List<Rect> rects = DrawerUtil.CalcHolizontalRects(labelRect, 2, 10);

			DrawerUtil.DrawPropertyFieldExpandLabel(rects[0], minProperty, () =>
			{
				if (isFloatMin)
				{
					minProperty.floatValue = Mathf.Clamp(minProperty.floatValue, Attribute.Min, maxProperty.floatValue);
				}
				else
				{
					minProperty.intValue = Mathf.Clamp(minProperty.intValue, Mathf.FloorToInt(Attribute.Min), maxProperty.intValue);
				}
			});

			DrawerUtil.DrawPropertyFieldExpandLabel(rects[1], maxProperty, () =>
			{
				if (isFloatMax)
				{
					maxProperty.floatValue = Mathf.Clamp(maxProperty.floatValue, minProperty.floatValue, Attribute.Max);
				}
				else
				{
					maxProperty.intValue = Mathf.Clamp(maxProperty.intValue, minProperty.intValue, Mathf.FloorToInt(Attribute.Max));
				}
			});
			EditorGUI.indentLevel = indentLevel;
		}

		//GUI 要素の高さ
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return base.GetPropertyHeight(property, label) * 2;
		}
	}
#endif
}
