﻿// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using System.Collections.Generic;

namespace Utage
{
	/// <summary>
	/// 補間処理
	/// </summary>
	public static class Easing
	{
		//補間計算
		public static float GetCurve01(float value, EaseType type = EaseType.Linear)
		{
			return GetCurve(0, 1, value, type );
		}

		//補間計算
		public static float GetCurve(float start, float end, float value, EaseType type = EaseType.Linear)
		{
			return Linear(start, end, GetCurve(value,type));
		}
		public static Vector2 GetCurve(Vector2 start, Vector2 end, float value, EaseType type = EaseType.Linear)
		{
			float t = GetCurve(value, type);
			return new Vector2 ( Linear(start.x, end.x, t), Linear(start.y, end.y, t));
		}
		public static Vector3 GetCurve(Vector3 start, Vector3 end, float value, EaseType type = EaseType.Linear)
		{
			float t = GetCurve(value, type);
			return new Vector3(Linear(start.x, end.x, t), Linear(start.y, end.y, t), Linear(start.z, end.z, t));
		}
		public static Vector4 GetCurve(Vector4 start, Vector4 end, float value, EaseType type = EaseType.Linear)
		{
			float t = GetCurve(value, type);
			return new Vector4(Linear(start.x, end.x, t), Linear(start.y, end.y, t), Linear(start.z, end.z, t), Linear(start.w, end.w, t));
		}

		//補間計算
		public static float GetCurve(float value, EaseType type )
		{
			switch (type)
			{
				case EaseType.Linear:
					return value;
				case EaseType.Spring:
					return Spring(value);
				case EaseType.EaseInQuad:
					return EaseInQuad(value);
				case EaseType.EaseOutQuad:
					return EaseOutQuad(value);
				case EaseType.EaseInOutQuad:
					return EaseInOutQuad(value);
				case EaseType.EaseInCubic:
					return EaseInCubic(value);
				case EaseType.EaseOutCubic:
					return EaseOutCubic(value);
				case EaseType.EaseInOutCubic:
					return EaseInOutCubic(value);
				case EaseType.EaseInQuart:
					return EaseInQuart(value);
				case EaseType.EaseOutQuart:
					return EaseOutQuart(value);
				case EaseType.EaseInOutQuart:
					return EaseInOutQuart(value);
				case EaseType.EaseInQuint:
					return EaseInQuint(value);
				case EaseType.EaseOutQuint:
					return EaseOutQuint(value);
				case EaseType.EaseInOutQuint:
					return EaseInOutQuint(value);
				case EaseType.EaseInSine:
					return EaseInSine(value);
				case EaseType.EaseOutSine:
					return EaseOutSine(value);
				case EaseType.EaseInOutSine:
					return EaseInOutSine(value);
				case EaseType.EaseInExpo:
					return EaseInExpo(value);
				case EaseType.EaseOutExpo:
					return EaseOutExpo(value);
				case EaseType.EaseInOutExpo:
					return EaseInOutExpo(value);
				case EaseType.EaseInCirc:
					return EaseInCirc(value);
				case EaseType.EaseOutCirc:
					return EaseOutCirc(value);
				case EaseType.EaseInOutCirc:
					return EaseInOutCirc(value);
				case EaseType.EaseInBack:
					return EaseInBack(value);
				case EaseType.EaseOutBack:
					return EaseOutBack(value);
				case EaseType.EaseInOutBack:
					return EaseInOutBack(value);
				default:
					Debug.LogError("UnkonownType");
					return value;
			}
		}

		public static float Linear(float start, float end, float value)
		{
			return Mathf.Lerp(start, end, value);
		}

		public static float Spring(float value)
		{
			value = Mathf.Clamp01(value);
			return (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
		}

		public static float EaseInQuad(float value)
		{
			return value * value;
		}

		public static float EaseOutQuad(float value)
		{
			return -value * (value - 2);
		}

		public static float EaseInOutQuad(float value)
		{
			value /= .5f;
			if (value < 1) return 0.5f * value * value;
			value--;
			return -0.5f * (value * (value - 2) - 1);
		}

		public static float EaseInCubic(float value)
		{
			return value * value * value;
		}

		public static float EaseOutCubic(float value)
		{
			value--;
			return (value * value * value + 1);
		}

		public static float EaseInOutCubic(float value)
		{
			value /= .5f;
			if (value < 1) return 0.5f * value * value * value;
			value -= 2;
			return 0.5f * (value * value * value + 2);
		}

		public static float EaseInQuart(float value)
		{
			return value * value * value * value;
		}

		public static float EaseOutQuart(float value)
		{
			value--;
			return -(value * value * value * value - 1);
		}

		public static float EaseInOutQuart(float value)
		{
			value /= .5f;
			if (value < 1) return 0.5f * value * value * value * value;
			value -= 2;
			return -0.5f * (value * value * value * value - 2);
		}

		public static float EaseInQuint(float value)
		{
			return value * value * value * value * value;
		}

		public static float EaseOutQuint(float value)
		{
			value--;
			return (value * value * value * value * value + 1);
		}

		public static float EaseInOutQuint(float value)
		{
			value /= .5f;
			if (value < 1) return 0.5f * value * value * value * value * value;
			value -= 2;
			return 0.5f * (value * value * value * value * value + 2);
		}

		public static float EaseInSine(float value)
		{
			return (1-Mathf.Cos(value * (Mathf.PI * 0.5f)));
		}

		public static float EaseOutSine(float value)
		{
			return Mathf.Sin(value * (Mathf.PI * 0.5f));
		}

		public static float EaseInOutSine(float value)
		{
			return -0.5f * (Mathf.Cos(Mathf.PI * value) - 1);
		}

		public static float EaseInExpo(float value)
		{
			return Mathf.Pow(2, 10 * (value - 1));
		}

		public static float EaseOutExpo(float value)
		{
			return (-Mathf.Pow(2, -10 * value) + 1);
		}

		public static float EaseInOutExpo(float value)
		{
			value /= .5f;
			if (value < 1) return 0.5f * Mathf.Pow(2, 10 * (value - 1));
			value--;
			return 0.5f * (-Mathf.Pow(2, -10 * value) + 2);
		}

		public static float EaseInCirc(float value)
		{
			return -(Mathf.Sqrt(1 - value * value) - 1);
		}

		public static float EaseOutCirc(float value)
		{
			value--;
			return Mathf.Sqrt(1 - value * value);
		}

		public static float EaseInOutCirc(float value)
		{
			value /= .5f;
			if (value < 1) return -0.5f * (Mathf.Sqrt(1 - value * value) - 1);
			value -= 2;
			return 0.5f * (Mathf.Sqrt(1 - value * value) + 1);
		}

		public static float EaseInBack(float value)
		{
			value /= 1;
			float s = 1.70158f;
			return (value) * value * ((s + 1) * value - s);
		}

		public static float EaseOutBack(float value)
		{
			float s = 1.70158f;
			value = (value) - 1;
			return ((value) * value * ((s + 1) * value + s) + 1);
		}

		public static float EaseInOutBack(float value)
		{
			float s = 1.70158f;
			value /= .5f;
			if ((value) < 1)
			{
				s *= (1.525f);
				return 0.5f * (value * value * (((s) + 1) * value - s));
			}
			value -= 2;
			s *= (1.525f);
			return 0.5f * ((value) * value * (((s) + 1) * value + s) + 2);
		}
	}
}