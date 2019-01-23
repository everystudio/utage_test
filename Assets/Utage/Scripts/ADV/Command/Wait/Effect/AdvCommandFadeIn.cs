// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// コマンド：フェードイン処理
	/// </summary>
	internal class AdvCommandFadeIn : AdvCommandFadeBase
	{

		public AdvCommandFadeIn(StringGridRow row)
			: base(row,true)
		{
		}
	}
}
