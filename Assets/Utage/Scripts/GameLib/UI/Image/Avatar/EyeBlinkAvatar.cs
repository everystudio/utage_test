// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using UtageExtensions;


namespace Utage
{

	/// <summary>
	/// アバタータイプのまばたき処理の基本クラス
	/// </summary>
	[AddComponentMenu("Utage/Lib/UI/EyeBlinkAvatar")]
	[RequireComponent(typeof(AvatarImage))]
	public class EyeBlinkAvatar : EyeBlinkBase
	{
		AvatarImage Avator { get { return this.gameObject.GetComponentCache<AvatarImage>(ref avator); } }
		AvatarImage avator;

		protected override IEnumerator CoEyeBlink(Action onComplete)
		{
			string pattern = AvatarData.ToPatternName(Avator.AvatarPattern.GetPatternName(EyeTag));
			if (string.IsNullOrEmpty(pattern))
			{
				if (onComplete != null) onComplete();
				yield break;
			}
			foreach (var data in AnimationData.DataList)
			{
				Avator.ChangePattern(EyeTag, data.ComvertName(pattern));
				yield return new WaitForSeconds(data.Duration);
			}
			Avator.ChangePattern(EyeTag, pattern);
			if (onComplete != null) onComplete();
			yield break;
		}
	}
}

