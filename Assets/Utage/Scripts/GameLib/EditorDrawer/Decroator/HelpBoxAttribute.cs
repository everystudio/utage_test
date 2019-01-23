// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utage
{

	/// <summary>
	/// [HelpBox]アトリビュート（デコレーター）
	/// ヘルプボックスを表示する
	/// </summary>
	public class HelpBoxAttribute : PropertyAttribute
	{
		/// <summary>
		/// ヘルプボックスの種類
		/// UnityEditor.MessageTypeをそのまま使うと、非エディタ環境でコンパイルエラーになるので
		/// 重複したものを書く
		/// </summary>
		public enum Type
		{
			None,           /// 通常
			Info,           /// 情報
			Warning,		/// 警告
			Error,          /// エラー
		}

		/// <summary>
		/// 表示するメッセージ
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// メッセージのタイプ
		/// </summary>
		public Type MessageType { get; set; }

		public HelpBoxAttribute(string message, Type type = Type.None, int order = 0)
		{
			Message = message;
			MessageType = type;
			this.order = order;
		}
	}

#if UNITY_EDITOR
	/// <summary>
	/// [HeloBox]を表示するためのプロパティ拡張
	/// </summary>
	[CustomPropertyDrawer(typeof(HelpBoxAttribute))]
	public class HelpBoxDrawer : DecoratorDrawerEx<HelpBoxAttribute>
	{
		//デコレーターと、本来のプロパティとの間のスペース
		const int spcae = 4;

		//heightの値を記録する
		//GetHeight()のときはインデントによる幅の違いがとれないので、OnGUIのときの計算を記録するしかない
		float height = -1;
		public override void OnGUI(Rect position)
		{
			position = EditorGUI.IndentedRect(position);
			position.height = GetHelpBoxHeight(position.width);
			this.height = position.height + spcae;

			EditorGUI.HelpBox(position, Attribute.Message, (MessageType)Attribute.MessageType);
		}

		public override float GetHeight()
		{
			if (this.height <= 0)
			{
				return GetHelpBoxHeight(GetIndentedViewWidth(0)) + spcae;
			}
			else
			{
				return this.height;
			}
		}


		public float GetHelpBoxHeight(float width)
		{
			var content = new GUIContent(Attribute.Message);
			//アイコンのあるなしでテキストエリアの幅が違うせい？
			//Unityのバグな気がする…
			if (Attribute.MessageType != HelpBoxAttribute.Type.None)
			{
				width -= 32;
			}
			return EditorStyles.helpBox.CalcHeight(content, width);
		}
	}
#endif
}
