// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using System;

namespace Utage
{
	/// <summary>
	/// フェード処理用のインターフェース
	/// </summary>
	public interface IAdvFade
	{
		//フェードイン
		void FadeIn(float time, Action onComplete);

		//フェードアウト
		void FadeOut(float time, Action onComplete);

		//ルール画像つきのフェードイン
		void RuleFadeIn(AdvEngine engine, AdvTransitionArgs data, Action onComplete);

		//ルール画像つきのフェードアウト
		void RuleFadeOut(AdvEngine engine, AdvTransitionArgs data, Action onComplete);
	}

}