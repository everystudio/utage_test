// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura

namespace Utage
{

	/// <summary>
	/// コマンド：パーティクル表示
	/// </summary>
	internal class AdvCommandParticleOff : AdvCommand
	{
		public AdvCommandParticleOff(StringGridRow row)
			: base(row)
		{
			this.name = ParseCellOptional<string>(AdvColumnName.Arg1, "");
		}

		public override void DoCommand(AdvEngine engine)
		{
			if (string.IsNullOrEmpty(name))
			{
				engine.GraphicManager.FadeOutAllParticle();
			}
			else
			{
				engine.GraphicManager.FadeOutParticle(name);
			}
		}

		string name;
	}
}