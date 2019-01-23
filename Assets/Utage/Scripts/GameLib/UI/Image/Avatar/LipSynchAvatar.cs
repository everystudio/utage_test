// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using UtageExtensions;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Utage
{

	/// <summary>
	/// まばたき処理
	/// </summary>
	[AddComponentMenu("Utage/Lib/UI/LipSynchAvatar")]
	public class LipSynchAvatar : LipSynch2d
	{
		AvatarImage Avator { get { return this.gameObject.GetComponentCache<AvatarImage>(ref avator); } }
		AvatarImage avator;

		protected override IEnumerator CoUpdateLipSync()
		{
			while (IsPlaying)
			{
				string pattern = AvatarData.ToPatternName(Avator.AvatarPattern.GetPatternName(LipTag));
				if (string.IsNullOrEmpty(pattern)) break;

				foreach (var data in AnimationData.DataList)
				{
					Avator.ChangePattern(LipTag, data.ComvertName(pattern));
					yield return new WaitForSeconds(data.Duration);
				}
				Avator.ChangePattern(LipTag, pattern);
				if (!IsPlaying) break;
				yield return new WaitForSeconds(Interval);
			}
			coLypSync = null;
			yield break;
		}

		protected override void OnStopLipSync()
		{
			base.OnStopLipSync();
			string pattern = AvatarData.ToPatternName(Avator.AvatarPattern.GetPatternName(LipTag));
			if (string.IsNullOrEmpty(pattern)) return;
			Avator.ChangePattern(LipTag, pattern);
		}
	}
}
