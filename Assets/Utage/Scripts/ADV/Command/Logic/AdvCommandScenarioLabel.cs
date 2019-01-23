// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimurausing UnityEngine;

namespace Utage
{

	/// <summary>
	/// コマンド：シナリオラベル
	/// </summary>
	public class AdvCommandScenarioLabel : AdvCommand
	{
		public AdvCommandScenarioLabel(StringGridRow row)
			: base(row)
		{
			this.ScenarioLabel = ParseScenarioLabel(AdvColumnName.Command);
			this.Type = ParseCellOptional<ScenarioLabelType>(AdvColumnName.Arg1, ScenarioLabelType.None);
		}


		public override void DoCommand(AdvEngine engine)
		{
		}

		public enum ScenarioLabelType
		{
			None,
			SavePoint,
		};
		public string ScenarioLabel { get; protected set; }

		public ScenarioLabelType Type { get; protected set; }
		public string Title
		{
			get
			{
				string title = ParseCellOptional<string>(AdvColumnName.Arg2, "");
				if (string.IsNullOrEmpty(title)) return "";

				return ParseCellLocalized(AdvColumnName.Arg2.QuickToString());
			}
		}
	}
}