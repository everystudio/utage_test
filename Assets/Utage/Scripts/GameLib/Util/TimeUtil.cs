// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// 時間処理
	/// </summary>
	public static class TimeUtil
	{
		static public float GetTime(bool unscaled)
		{
			return unscaled ? Time.unscaledTime : Time.time;
		}

		static public float GetDeltaTime(bool unscaled)
		{
			return unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
		}
	}
}