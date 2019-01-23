// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using System.Collections.Generic;
using System;
using UtageExtensions;

namespace Utage
{
	public class AdvAnimationData : IAdvSettingData
	{
		public AnimationClip Clip { get; set; }

		public AdvAnimationData(StringGrid grid, ref int index, bool legacy)
		{
			Clip = new AnimationClip();
			Clip.legacy = legacy;
			ParseHeader(grid.Rows[index++]);
			List<float> timeTbl = ParseTimeTbl(grid.Rows[index++]);
			if (!Clip.legacy)
			{
				AddDummyCurve(timeTbl);
			}

			while (index < grid.Rows.Count)
			{
				StringGridRow row = grid.Rows[index];
				try
				{
					if (row.IsEmptyOrCommantOut)
					{
						++index;
						continue;
					}

					if (IsHeader(row))
					{
						break;
					}
					PropertyType propertyType;
					if (!row.TryParseCellTypeOptional<PropertyType>(0, PropertyType.Custom, out propertyType))
					{
						string str = row.ParseCell<string>(0);
						//					Debug.LogError( row.ToErrorString("PropertyType Parse Error") );

						string typeName, propertyName;
						str.Separate('.', false, out typeName, out propertyName);
						Type type = System.Type.GetType(typeName);
						if (type == null)
						{
							Debug.LogError(typeName + "is not class name");
						}
						Clip.SetCurve("", type, propertyName, ParseCurve(timeTbl, row));
					}
					else
					{
						if (IsEvent(propertyType))
						{
							AddEvent(propertyType, timeTbl, row);
						}
						else
						{
							AddCurve(propertyType, ParseCurve(timeTbl, row));
						}
					}
					++index;
				}
				catch (System.Exception e)
				{
					Debug.LogError( row.ToErrorString( e.Message) );
				}
			}
		}

		bool IsHeader(StringGridRow row)
		{
			return (row.ParseCell<string>(0)[0] == '*');
		}

		void ParseHeader(StringGridRow row)
		{
			Clip.name = row.ParseCell<string>(0).Substring(1);
			Clip.wrapMode = row.ParseCellOptional<WrapMode>(1, WrapMode.Default);
		}

		List<float> ParseTimeTbl(StringGridRow row)
		{
			List<float> timeTbl = new List<float>();
			for (int i = 1; i < row.Strings.Length; ++i)
			{
				float time;
				if (!row.TryParseCell<float>(i, out time))
				{
					Debug.LogError(row.ToErrorString("TimeTbl pase error"));
				}
				timeTbl.Add(time);
			}
			return timeTbl;
		}

		//アニメーションするのプロパティ
		enum PropertyType
		{
			Custom,
			X,
			Y,
			Z,
			Scale,      //スケールXYZすべて
			ScaleX,
			ScaleY,
			ScaleZ,
			Angle,      //AngleZに同じ（2Dメインなので）
			AngleX,
			AngleY,
			AngleZ,
			Alpha,
			Texture,
		};

		bool IsEvent(PropertyType type)
		{
			switch (type)
			{
				case PropertyType.Texture:
					return true;
				default:
					return false;
			}
		}

		bool IsCustomProperty(PropertyType type)
		{
			switch (type)
			{
				case PropertyType.Custom:
					return true;
				default:
					return false;
			}
		}

		void AddEvent(PropertyType propertyType, List<float> timeTbl, StringGridRow row)
		{
			for (int i = 0; i < row.Strings.Length; ++i)
			{
				if (i == 0) continue;
				if (row.IsEmptyCell(i)) continue;

				//キーの追加
				AnimationEvent e = new AnimationEvent();
				// AnimationCurveの生成.
				switch (propertyType)
				{
					case PropertyType.Texture:
						string value;
						if (!row.TryParseCell<string>(i, out value)) continue;
						e.functionName = "ChangePattern";
						e.stringParameter = value;
						e.time = timeTbl[i - 1];
						break;
				}
				if (Application.isPlaying)
				{
					Clip.AddEvent(e);
				}
				else
				{
#if UNITY_EDITOR
//					UnityEditor.AnimationUtility. Clip.AddEvent(e);
#endif
				}
			}
		}

		AnimationCurve ParseCurve(List<float> timeTbl, StringGridRow row)
		{
			// AnimationCurveの生成.
			AnimationCurve curve = new AnimationCurve();
			for (int i = 0; i < row.Strings.Length; ++i)
			{
				if (i == 0) continue;
				if (row.IsEmptyCell(i)) continue;

				float value;
				if (!row.TryParseCell<float>(i, out value)) continue;
				//キーの追加
//				Debug.Log("AddKey " + timeTbl[i - 1] + " " + value);
				curve.AddKey(new Keyframe(timeTbl[i-1], value));
			}
			if (curve.keys.Length <= 1)
			{
//				Debug.LogError(row.ToErrorString("Need more than 2 key data"));
			}
			return curve;
		}

		//Animatorの場合、最後のフレームまでカーブデータがないと途中で終わってしまうのでダミーで乗せる
		void AddDummyCurve(List<float> timeTbl)
		{
			AnimationCurve dummyCurve = AnimationCurve.Linear(timeTbl[0], 0, timeTbl[timeTbl.Count - 1], 1);
			Clip.SetCurve("", typeof(UnityEngine.Object), "", dummyCurve);
		}

		void AddCurve(PropertyType type, AnimationCurve curve)
		{
			if (curve.keys.Length <= 0) return;
			switch (type)
			{
				case PropertyType.X:
					Clip.SetCurve("", typeof(Transform), "localPosition.x", curve);
					break;
				case PropertyType.Y:
					Clip.SetCurve("", typeof(Transform), "localPosition.y", curve);
					break;
				case PropertyType.Z:
					Clip.SetCurve("", typeof(Transform), "localPosition.z", curve);
					break;
				case PropertyType.ScaleX:
					Clip.SetCurve("", typeof(Transform), "localScale.x", curve);
					break;
				case PropertyType.ScaleY:
					Clip.SetCurve("", typeof(Transform), "localScale.y", curve);
					break;
				case PropertyType.ScaleZ:
					Clip.SetCurve("", typeof(Transform), "localScale.z", curve);
					break;
				case PropertyType.Scale:
					Clip.SetCurve("", typeof(Transform), "localScale.x", curve);
					Clip.SetCurve("", typeof(Transform), "localScale.y", curve);
					Clip.SetCurve("", typeof(Transform), "localScale.z", curve);
					break;
				case PropertyType.AngleX:
					Clip.SetCurve("", typeof(Transform), "localEulerAngles.x", curve);
					break;
				case PropertyType.AngleY:
					Clip.SetCurve("", typeof(Transform), "localEulerAngles.y", curve);
					break;
				case PropertyType.Angle:
				case PropertyType.AngleZ:
					Clip.SetCurve("", typeof(Transform), "localEulerAngles.z", curve);
					break;
				case PropertyType.Alpha:
					Clip.SetCurve("", typeof(AdvEffectColor), "animationColor.a", curve);
					break;
				default:
					Debug.LogError("UnknownType");
					break;					
			}
		}
	};

	/// <summary>
	/// キーフレームアニメーションの設定
	/// </summary>
	public class AdvAnimationSetting : AdvSettingBase
	{
		List<AdvAnimationData> DataList = new List<AdvAnimationData>();
		protected override void OnParseGrid(StringGrid grid)
		{
			int index = 0;
			while (index < grid.Rows.Count)
			{
				if (grid.Rows[index].IsEmpty)
				{
					index++;
					continue;
				}

				AdvAnimationData data = new AdvAnimationData(grid, ref index, true);
				DataList.Add(data);
			}
		}

		public AdvAnimationData Find(string name)
		{
			return DataList.Find(x => x.Clip.name == name);
		}
	}
}
