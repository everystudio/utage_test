// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// コマンド：背景表示・切り替え
	/// </summary>
	internal class AdvCommandBg : AdvCommand
	{
		public AdvCommandBg(StringGridRow row, AdvSettingDataManager dataManager)
			: base(row)
		{
			string label = ParseCell<string>(AdvColumnName.Arg1);
			if (!dataManager.TextureSetting.ContainsLabel(label))
			{
				Debug.LogError(ToErrorString(label + " is not contained in file setting"));
			}

			this.graphic = dataManager.TextureSetting.LabelToGraphic(label);
			AddLoadGraphic(graphic);

			this.layerName = ParseCellOptional<string>(AdvColumnName.Arg3, "");
			if (!string.IsNullOrEmpty(layerName) && !dataManager.LayerSetting.Contains(layerName, AdvLayerSettingData.LayerType.Bg))
			{
				Debug.LogError(ToErrorString(layerName + " is not contained in layer setting"));
			}
			this.fadeTime = ParseCellOptional<float>(AdvColumnName.Arg6, 0.2f);
		}

		public override void DoCommand(AdvEngine engine)
		{
			AdvGraphicOperaitonArg graphicOperaitonArg = new AdvGraphicOperaitonArg(this, graphic.Main, fadeTime);
			engine.GraphicManager.IsEventMode = false;
			//表示する
			if (string.IsNullOrEmpty(layerName))
			{
				engine.GraphicManager.BgManager.DrawToDefault(engine.GraphicManager.BgSpriteName, graphicOperaitonArg);
			}
			else
			{
				engine.GraphicManager.BgManager.Draw(layerName, engine.GraphicManager.BgSpriteName, graphicOperaitonArg);
			}

			//基本以外のコマンド引数の適用
			AdvGraphicObject obj = engine.GraphicManager.BgManager.FindObject(engine.GraphicManager.BgSpriteName);
			if (obj != null)
			{
				//位置の適用（Arg4とArg5）
				obj.SetCommandPostion(this);
				//その他の適用（モーション名など）
				obj.TargetObject.SetCommandArg(this);
			}
		}

		protected AdvGraphicInfoList graphic;
		protected string layerName;
		protected float fadeTime;
	}
}