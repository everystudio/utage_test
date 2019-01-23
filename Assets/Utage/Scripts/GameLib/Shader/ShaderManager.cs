// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using System.Collections.Generic;
using System;
using UtageExtensions;

namespace Utage
{

	/// <summary>
	/// シェーダーの管理
	/// </summary>
	public static class ShaderManager
	{
		//ルール画像付きのフェード処理をする場合のシェーダー
		static public Shader RuleFade { get { return Shader.Find("Utage/UI/RuleFade"); } }

		//背景を透過しないクロスフェード処理をする場合のシェーダー
		static public Shader CrossFade { get { return Shader.Find("Utage/CrossFadeImage"); } }

		//透過画像を描きこむシェーダー
		static public Shader RenderTexture { get { return Shader.Find("Utage/RenderTexture"); } }

		//透過画像を描き込んだRenderTextureを描画するシェーダー
		static public Shader DrawByRenderTexture { get { return Shader.Find("Utage/DrawByRenderTexture"); } }

		//カラーフェード
		static public string ColorFade = "Utage/ImageEffect/ColorFade";

		//ブルームシェーダー名
		static public string BloomName = "Utage/ImageEffect/Bloom";

		//ブラー
		static public string BlurName = "Utage/ImageEffect/Blur";

		//モザイク
		static public string MosaicName = "Utage/ImageEffect/Mosaic";

		//カラーコレクション（ランプ画像）
		static public string ColorCorrectionRampName = "Utage/ImageEffect/ColorCorrectionRamp";

		//グレースケール
		static public string GrayScaleName = "Utage/ImageEffect/Grayscale";

		//モーションブラー
		static public string MotionBlurName = "Utage/ImageEffect/MotionBlur";

		//ノイズ
		static public string NoiseAndGrainName = "Utage/ImageEffect/NoiseAndGrain";

		//オーバーレイ
		static public string BlendModesOverlayName = "Utage/ImageEffect/BlendModesOverlay";

		//セピア
		static public string SepiatoneName = "Utage/ImageEffect/Sepiatone";

		//ネガポジ反転
		static public string NegaPosiName = "Utage/ImageEffect/NegaPosi";

		//魚眼
		static public string FisheyeName = "Utage/ImageEffect/Fisheye";

		//一点を中心に画像を歪ませる
		static public string TwirlName = "Utage/ImageEffect/Twirl";

		//円で画像を歪ませる
		static public string VortexName = "Utage/ImageEffect/Vortex";
	}
}
