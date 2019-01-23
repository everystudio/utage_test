// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Utage
{

	/// <summary>
	/// キャラクタの表示情報
	/// </summary>
	public class AdvCharacterInfo
	{
		public static AdvCharacterInfo Create( AdvCommand command, AdvSettingDataManager dataManager )
		{
			if (command.IsEmptyCell(AdvColumnName.Arg1))
			{
				return null;
			}

			//名前
			string nameText = command.ParseCell<string>(AdvColumnName.Arg1);
			string characterLabel = nameText;
			//第二引数を解析
			//基本的にはパターン名だが
			//キャラクターラベルの指定タグがあったり、非表示タグする
			bool isHide = false;
			string erroMsg = "";
			string pattern = ParserUtil.ParseTagTextToString(
				command.ParseCellOptional<string>(AdvColumnName.Arg2, ""),
				(tagName, arg) =>
				{
					bool failed = false;
					switch (tagName)
					{
						case "Off":
							//非表示タグ
							isHide = true;
							break;
						case "Character":
							//キャラクターラベルの指定タグ
							characterLabel = arg;
							break;
						default:
							erroMsg = "Unkownn Tag <" + tagName + ">";
							failed = true;
							break;
					}
					return !failed;
				});

			if(!string.IsNullOrEmpty(erroMsg))
			{
				Debug.LogError(erroMsg);
				return null;
			}

			if (!dataManager.CharacterSetting.Contains(characterLabel))
			{
				//そもそもキャラ表示がない場合、名前表示のみになる
				return new AdvCharacterInfo(characterLabel, nameText, pattern, isHide, null);
			}

			AdvCharacterSettingData data = dataManager.CharacterSetting.GetCharacterData(characterLabel, pattern);
			//キャラの表示情報の記述エラー
			if (data == null)
			{
				Debug.LogError(command.ToErrorString(characterLabel + ", " + pattern + " is not contained in Chactecter Sheet"));
				return null;
			}
			//名前テキストをキャラクターシートの定義に変更
			if (!string.IsNullOrEmpty(data.NameText) && nameText == characterLabel)
			{
				nameText = data.NameText;
			}
			return new AdvCharacterInfo(characterLabel, nameText, pattern, isHide, data.Graphic);
		}

		AdvCharacterInfo(string label, string nameText, string pattern, bool isHide, AdvGraphicInfoList graphic)
		{
			this.Label = label;
			this.NameText = nameText;
			this.Pattern = pattern;
			this.IsHide = isHide;
			this.Graphic = graphic;
		}

		public string Label { get; private set; }

		public string NameText { get; private set; }

		public string Pattern { get; private set; }

		public bool IsHide { get; private set; }

		public AdvGraphicInfoList Graphic { get; private set; }
		public string LocalizeNameText
		{
			get
			{
				return LanguageManager.Instance.LocalizeText(TextParser.MakeLogText(this.NameText));
			}
		}
	}
}
