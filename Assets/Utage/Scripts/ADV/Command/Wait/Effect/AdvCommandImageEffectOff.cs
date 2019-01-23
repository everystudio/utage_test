// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UtageExtensions;

namespace Utage
{

	/// <summary>
	/// コマンド：フェードアウト処理
	/// </summary>
	internal class AdvCommandImageEffectOff : AdvCommandImageEffectBase
	{
		public AdvCommandImageEffectOff(StringGridRow row, AdvSettingDataManager dataManager)
			: base(row, dataManager,true)
		{
		}
	}
}