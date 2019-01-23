// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using System.Collections.Generic;
using System;
using UtageExtensions;

namespace Utage
{

	/// <summary>
	/// エフェクトの管理（主に、エフェクトの終了待ち(改ページ時や、コマンド終了時のための処理）
	/// </summary>
	[AddComponentMenu("Utage/ADV/EffectManager")]
	public class AdvEffectManager : MonoBehaviour
	{
		public AdvEngine Engine { get { return engine ?? (engine = this.GetComponentInParent<AdvEngine>()); } }
		AdvEngine engine;

		/// <summary>
		/// メッセージウィンドウ
		/// </summary>
		AdvUguiMessageWindowManager MessageWindow { get { return messageWindow ?? (messageWindow = Engine.GetComponentInChildren<AdvUguiMessageWindowManager>(true)); } }
		[SerializeField]
		AdvUguiMessageWindowManager messageWindow;

		//ルール画像
		public List<Texture2D> RuleTextureList
		{
			get { return ruleTextureList; }
			set { ruleTextureList = value; }
		}
		[SerializeField]
		List<Texture2D> ruleTextureList = new List<Texture2D>();

		internal Texture2D FindRuleTexture(string name)
		{
			foreach (var item in ruleTextureList)
			{
				if (item == null) continue;
				if (item.name == name) return item;
			}
			Debug.LogErrorFormat("Not Found Rule Texture [ {0} ]", name);
			return null;
		}

		//エフェクト対象のオブジェクトのタイプ
		public enum TargetType
		{
			Default,            //通常のオブジェクト
			Camera,             //カメラ
			Graphics,           //グラフィック全体
			MessageWindow,      //メッセージウィンドウ
		};

		//エフェクトデータに設定されたオブジェクトを検索する
		internal GameObject FindTarget(AdvCommandEffectBase command)
		{
			return FindTarget(command.Target, command.TargetName);
		}

		//設定されたオブジェクトを検索する
		internal GameObject FindTarget(TargetType targetType, string targetName)
		{
			switch (targetType)
			{
				case TargetType.MessageWindow:
					return MessageWindow.gameObject;
				case TargetType.Graphics:
					return Engine.GraphicManager.gameObject;
				case TargetType.Camera:
					if (string.IsNullOrEmpty(targetName) || targetName == TargetType.Camera.ToString())
					{
						return Engine.CameraManager.gameObject;
					}
					else
					{
						CameraRoot camera = Engine.CameraManager.FindCameraRoot(targetName);
						if (camera == null)
						{
							return null;
						}
						else
						{
							return camera.gameObject;
						}
					}
				case TargetType.Default:
				default:
					return Engine.GraphicManager.FindObjectOrLayer(targetName);
			}
		}
	}
}
