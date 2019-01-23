// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// コマンド：背景表示・切り替え
	/// </summary>
	internal class AdvCommandBgEvent : AdvCommand
	{
		public AdvCommandBgEvent(StringGridRow row, AdvSettingDataManager dataManager)
			: base(row)
		{
			this.label = ParseCell<string>(AdvColumnName.Arg1);
			if (!dataManager.TextureSetting.ContainsLabel(label))
			{
				Debug.LogError(ToErrorString(label + " is not contained in file setting"));
			}

			this.graphic = dataManager.TextureSetting.LabelToGraphic(label);
			AddLoadGraphic(graphic);
			this.fadeTime = ParseCellOptional<float>(AdvColumnName.Arg6, 0.2f);
		}

		public override void DoCommand(AdvEngine engine)
		{
			AdvGraphicOperaitonArg graphicOperaitonArg = new AdvGraphicOperaitonArg(this, graphic.Main, fadeTime);

			engine.SystemSaveData.GalleryData.AddCgLabel(label);
			//表示する
			engine.GraphicManager.IsEventMode = true;
			//キャラクターは非表示にする
			engine.GraphicManager.CharacterManager.FadeOutAll(graphicOperaitonArg.GetSkippedFadeTime(engine));
			//表示する
			engine.GraphicManager.BgManager.DrawToDefault(engine.GraphicManager.BgSpriteName, graphicOperaitonArg);

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

		string label;
		AdvGraphicInfoList graphic;
		float fadeTime;
	}
}