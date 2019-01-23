// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UtageExtensions;

namespace Utage
{

	/// <summary>
	/// コマンド：イメージエフェクト開始
	/// </summary>
	internal class AdvCommandImageEffect : AdvCommandImageEffectBase
	{
		public AdvCommandImageEffect(StringGridRow row, AdvSettingDataManager dataManager)
			: base(row, dataManager, false)
		{
		}
	}
}