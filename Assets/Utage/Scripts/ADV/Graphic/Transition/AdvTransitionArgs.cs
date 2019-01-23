// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// トランジションの実行データ
	/// </summary>
	public class AdvTransitionArgs
	{
		internal AdvTransitionArgs( string textureName, float vague, float time )
		{
			this.TextureName = textureName;
			this.Vague = vague;
			this.Time = time;
		}

		internal string TextureName { get; private set; }
		internal float Vague { get; private set; }
		float Time { get; set; }

		internal float GetSkippedTime(AdvEngine engine)
		{
			return engine.Page.ToSkippedTime(Time);
		}
	}
}