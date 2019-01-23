// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// コマンド：パーティクル表示
	/// </summary>
	internal class AdvCommandParticle : AdvCommand
	{
		public AdvCommandParticle(StringGridRow row, AdvSettingDataManager dataManager)
			: base(row)
		{
			this.label = ParseCell<string>(AdvColumnName.Arg1);
			if (!dataManager.ParticleSetting.Dictionary.ContainsKey(label))
			{
				Debug.LogError(ToErrorString(label + " is not contained in file setting"));
			}

			this.graphic = dataManager.ParticleSetting.LabelToGraphic(label);
			AddLoadGraphic(graphic);
			this.layerName = ParseCellOptional<string>(AdvColumnName.Arg3, "");
			if (!string.IsNullOrEmpty(layerName) && !dataManager.LayerSetting.Contains(layerName))
			{
				Debug.LogError(ToErrorString( layerName + " is not contained in layer setting"));
			}

			//グラフィック表示処理を作成
			this.graphicOperaitonArg =  new AdvGraphicOperaitonArg(this, graphic, 0 );

//			this.sortingOrder = ParseCellOptional<int>(AdvColumnName.Arg4,0);
		}

		public override void DoCommand(AdvEngine engine)
		{
			string layer = layerName;
			if (string.IsNullOrEmpty(layer))
			{
				//レイヤー名指定なしならスプライトのデフォルトレイヤー
				layer = engine.GraphicManager.SpriteManager.DefaultLayer.name;
			}
			//表示する
			engine.GraphicManager.DrawObject(layer, label, graphicOperaitonArg);
			//			AdvGraphicObjectParticle particle = obj.TargetObject as AdvGraphicObjectParticle;
			//			particle.AddSortingOrder(sortingOrder,"");

			//基本以外のコマンド引数の適用
			AdvGraphicObject obj = engine.GraphicManager.FindObject(label);
			if (obj != null)
			{
				//位置の適用（Arg4とArg5）
				obj.SetCommandPostion(this);
				//その他の適用（モーション名など）
				obj.TargetObject.SetCommandArg(this);
			}
		}

		protected string label;
		protected string layerName;
//		protected int sortingOrder;
		protected AdvGraphicInfo graphic;
		protected AdvGraphicOperaitonArg graphicOperaitonArg;
	}
}