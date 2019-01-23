// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utage
{

	/// <summary>
	/// ノベル用のアバターデータの表示パターン
	/// </summary>
	[System.Serializable]
	public class AvatarPattern
	{
		//タグごとの表示パターン名のデータ
		[System.Serializable]
		public class PartternData
		{
			public string tag;
			public string patternName;
		}
		//タグごとの表示パターン名のデータのリスト
		public List<PartternData> DataList { get { return avatarPatternDataList; } }
		[SerializeField]
		List<PartternData> avatarPatternDataList = new List<PartternData>();

		public List<string> OptionPatternNameList { get { return optionPatternNameList; } }
		[SerializeField]
		List<string> optionPatternNameList = new List<string>();

		public void SetPatternName(string tag, string patternName)
		{
			PartternData pattern = DataList.Find(x => x.tag == tag);
			if (pattern == null)
			{
				Debug.LogError(string.Format("Unknown Pattern [{0}], tag[{1}] ", patternName, tag));
				return;
			}
			pattern.patternName = patternName;
		}

		public string GetPatternName(string tag)
		{
			PartternData pattern = DataList.Find(x => x.tag == tag);
			return (pattern == null) ? "" : pattern.patternName;
		}

		internal void SetPattern(StringGridRow rowData)
		{
			foreach (var keyValue in rowData.Grid.ColumnIndexTbl)
			{
				PartternData pattern = DataList.Find(x => x.tag == keyValue.Key);
				if (pattern==null) continue;
				pattern.patternName = rowData.Strings[keyValue.Value];
			}
		}

#if UNITY_EDITOR

		/// <summary>
		/// プロパティ描画
		/// </summary>
		[CustomPropertyDrawer(typeof(NovelAvatarPatternAttribute))]
		public class NovelAvatarPatternDrawer : PropertyDrawerEx<NovelAvatarPatternAttribute>
		{
			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				AvatarData data = CallFunction<AvatarData>(property, Attribute.Function);
				if (data == null) return;

				//パターンデータ（タグとパターン名）
				EditorGUI.BeginProperty(position, label, property);
				var dataListProperty = property.FindPropertyRelative("avatarPatternDataList");
				float h = LineHeight();
				for (int i = 0; i < dataListProperty.arraySize; ++i)
				{
					var childProperty = dataListProperty.GetArrayElementAtIndex(i);
					var tagProperty = childProperty.FindPropertyRelative("tag");
					var patternNameProperty = childProperty.FindPropertyRelative("patternName");
					if (tagProperty.stringValue == data.OptionTag)
					{
						continue;
					}
					List<string> patternNameList = new List<string> { "None" };
					patternNameList.AddRange( data.GetPatternNameListInTag(tagProperty.stringValue) );
					int currentPatternIndex = patternNameList.FindIndex(x => x == patternNameProperty.stringValue);
					currentPatternIndex = Mathf.Max(0, currentPatternIndex);
					position.height = h;
					int index = EditorGUI.Popup(position, tagProperty.stringValue, currentPatternIndex, patternNameList.ToArray());
					patternNameProperty.stringValue = patternNameList[index];
					position.y += h;
				}

				//オプションデータ（アクセサリなどの表示）
				var optionPatternNameListProperty = property.FindPropertyRelative("optionPatternNameList");
				List<string> list = DrawerUtil.GetStringList(optionPatternNameListProperty);
				List<string> newList = new List<string>();
				bool hasChanged = false;
				foreach (var optionPattern in data.GetAllOptionPatterns())
				{
					bool check = list.FindIndex(x => x == optionPattern) >= 0;
					bool check1 = EditorGUI.Toggle(position, optionPattern, check);
					if (check!=check1)
					{
						hasChanged = true;
						check = check1;
					}
					if (check)
					{
						newList.Add(optionPattern);
					}
					position.y += h;
				}
				if (hasChanged)
				{
					DrawerUtil.SetStringArray(optionPatternNameListProperty, newList);
				}

				EditorGUI.EndProperty();
			}

			public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
			{
				AvatarData data = CallFunction<AvatarData>(property, Attribute.Function);
				if (data == null) return LineHeight();

				SerializedProperty dataListProperty = property.FindPropertyRelative("avatarPatternDataList");
				return LineHeight()*( dataListProperty.arraySize + data.GetAllOptionPatterns().Count );
			}

			float LineHeight()
			{
				return (EditorStyles.popup.fixedHeight + 2);
			}

			public void OnPreviewGUI(AvatarData data, AvatarPattern pattern, Rect r, GUIStyle background)
			{
				List<Sprite> parts = data.MakeSortedSprites(pattern);
				foreach(var part in  parts)
				{
					if (part == null) continue;
					GUI.DrawTexture(r, AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GetAssetPath(part)), ScaleMode.ScaleToFit, true);
				}
			}
		}

		internal void RebuildOnApply(AvatarData data)
		{
			List<PartternData> dataList = this.DataList;
			this.avatarPatternDataList = new List<PartternData>();
			Rebuild(data);
			foreach (var pattern1 in DataList)
			{
				var pattern0 = dataList.Find(x => x.tag == pattern1.tag);
				if (pattern0 != null)
				{
					pattern1.patternName = pattern0.patternName;
				}
			}
			Debug.LogFormat("this.DataList = {0}", this.DataList.Count);
			dataList.Clear();
		}
#endif
		internal bool Rebuild(AvatarData data)
		{
			if (data == null) return false;
			bool hasChanged = false;
			foreach (var category in data.categories)
			{
				PartternData partternData = DataList.Find(x => x.tag == category.Tag);
				if (partternData == null)
				{
					partternData = new PartternData();
					partternData.tag = category.Tag;
					DataList.Add(partternData);
					hasChanged = true;
				}
			}
			return hasChanged;
		}
	}
	public class NovelAvatarPatternAttribute : PropertyAttribute
	{
		public string Function { get; set; }
		public NovelAvatarPatternAttribute(string function)
		{
			Function = function;
		}
	}
}