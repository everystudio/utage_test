// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura

namespace Utage
{

	/// <summary>
	/// コマンド：ELSE処理
	/// </summary>
	internal class AdvCommandElse : AdvCommand
	{

		public AdvCommandElse(StringGridRow row)
			: base(row)
		{
		}

		public override void DoCommand(AdvEngine engine)
		{
			CurrentTread.IfManager.Else();
		}

		//IF文タイプのコマンドか
		public override bool IsIfCommand { get { return true; } }
	}
}