// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// テキストの解析クラス
	/// </summary>
	public class TextParser
	{
		public const string TagSound = "sound";
		public const string TagSpeed = "speed";
		public const string TagUnderLine = "u";
		
		public static string AddTag(string text, string tag, string arg)
		{
			return string.Format("<{1}={2}>{0}</{1}>", text, tag, arg);
		}

		/// <summary>
		/// 文字データリスト
		/// </summary>
		public List<CharData> CharList { get { return this.charList; } }
		List<CharData> charList = new List<CharData>();

		/// <summary>
		/// 文字列から数式を計算するコールバック
		/// </summary> 
		public static Func<string, object> CallbackCalcExpression;

		/// <summary>
		/// エラーメッセージ
		/// </summary>
		public string ErrorMsg { get { return this.errorMsg; } }
		string errorMsg = null;
		void AddErrorMsg(string msg)
		{
			if (string.IsNullOrEmpty(errorMsg)) errorMsg = "";
			else errorMsg += "\n";

			errorMsg += msg;
		}

		/// <summary>
		/// 表示文字数（メタデータを覗く）
		/// </summary>
		public int Length { get { return CharList.Count; } }

		//もとのテキスト
		string originalText;

		public string OriginalText
		{
			get { return originalText; }
		}

				/// <summary>
		/// メタ情報なしの文字列を取得
		/// </summary>
		/// <returns>メタ情報なしの文字列</returns>
		public string NoneMetaString
		{
			get
			{
				//未作成なら作成する
				InitNoneMetaText();
				return noneMetaString;
			}
		}
		string noneMetaString;

		//メタ情報なしのテキストを初期化する
		void InitNoneMetaText()
		{
			//作成ずみならなにもしない
			if (!string.IsNullOrEmpty(noneMetaString)) return;

			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < CharList.Count; ++i)
			{
				builder.Append( CharList[i].Char );
			}
			noneMetaString = builder.ToString();
		}

		//現在のテキストインデックス
		int currentTextIndex;
		//文字のカスタムデータ解析用
		CharData.CustomCharaInfo customInfo = new CharData.CustomCharaInfo();

		//パラメーターのみパースする
		bool isParseParamOnly;


		public static string MakeLogText(string text)
		{
			return new TextParser(text, true).NoneMetaString;
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="text">メタデータを含むテキスト</param>
		public TextParser(string text, bool isParseParamOnly = false )
		{
			originalText = text;
			this.isParseParamOnly = isParseParamOnly;
			Parse();
		}

		/// <summary>
		/// メタデータを含むテキストデータを解析
		/// </summary>
		/// <param name="text">解析するテキスト</param>
		void Parse()
		{
			try
			{
				//テキストを先頭から1文字づつ解析
				int max = OriginalText.Length;
				currentTextIndex = 0;
				while (currentTextIndex < max)
				{
					if (ParseEscapeSequence())
					{
						//エスケープシーケンスの処理
					}
					else
					{
						Func<string, string, bool> callbackParseTag;
						if(isParseParamOnly)  callbackParseTag = ParseTagParamOnly;
						else callbackParseTag = ParseTag;

						int endIndex = ParserUtil.ParseTag(OriginalText, currentTextIndex, callbackParseTag);
						if (currentTextIndex == endIndex)
						{
							//通常パターンのテキストを1文字追加
							AddChar(OriginalText[currentTextIndex]);
							++currentTextIndex;
						}
						else
						{
							currentTextIndex = endIndex+1;
						}
					}
				}
			}
			catch( System.Exception e )
			{
				AddErrorMsg(e.Message + e.StackTrace );
			}
		}

		bool ParseEscapeSequence()
		{
			//二文字目がない場合は何もしない
			if (currentTextIndex +1 >= OriginalText.Length)
			{
				return false;
			}

			char c0 = OriginalText[currentTextIndex];
			char c1 = OriginalText[currentTextIndex + 1];

			//改行コードの処理だけはする
			if (c0 == '\\' && c1 == 'n')
			{	//文字列としての改行コード　\n
				//通常パターンのテキストを1文字追加
				AddDoubleLineBreak();
				currentTextIndex += 2;
				return true;
			}
			else if( c0 == '\r' && c1 == '\n')
			{	//改行コード \r\nを1文字で扱う
				AddDoubleLineBreak();
				currentTextIndex += 2;
				return true;
			}
			return false;
		}

		//文字列を追加
		void AddStrng(string text)
		{
			foreach (char c in text)
			{
				AddChar(c);
			}
		}

		//文字を追加
		void AddChar(char c)
		{
			CharData data = new CharData(c, customInfo);
			charList.Add(data);
			customInfo.ClearOnNextChar();
		}

		//本来二文字ぶんの改行文字を追加
		void AddDoubleLineBreak()
		{
			CharData data = new CharData('\n', customInfo);
			data.CustomInfo.IsDoubleWord = true;
			charList.Add(data);
		}

		//棒線（ダッシュ、ダーシ）を追加
		void AddDash(string arg)
		{
			int size;
			if (!int.TryParse(arg, out size))
			{
				size = 1;
			}
			CharData data = new CharData(CharData.Dash, customInfo);
			data.CustomInfo.IsDash = true;
			data.CustomInfo.DashSize = size;
			charList.Add(data);
		}

		//絵文字を追加
		bool TryAddEmoji(string arg)
		{
			if(string.IsNullOrEmpty(arg))
			{
				return false;
			}

			CharData data = new CharData('□', customInfo);
			data.CustomInfo.IsEmoji = true;
			data.CustomInfo.EmojiKey = arg;
			charList.Add(data);
			return true;
		}

		//サイズ指定のスペースの追加
		bool TryAddSpace(string arg)
		{
			int size;
			if (!int.TryParse(arg, out size))
			{
				return false;
			}

			CharData data = new CharData(' ', customInfo);
			data.CustomInfo.IsSpace = true;
			data.CustomInfo.SpaceSize = size;
			charList.Add(data);
			return true;
		}

		//インターバルの追加
		bool TryAddInterval(string arg)
		{
			if (charList.Count <= 0) return false;
			return charList[charList.Count - 1].TryParseInterval(arg);
		}


		bool ParseTag(string name, string arg)
		{
			switch (name)
			{
				//太字
				case "b":
					return customInfo.TryParseBold(arg);
				case "/b":
					customInfo.ResetBold();
					return true;
				//イタリック
				case "i":
					return customInfo.TryParseItalic(arg);
				case "/i":
					customInfo.ResetItalic();
					return true;
				//カラー
				case "color":
					return customInfo.TryParseColor(arg);
				case "/color":
					customInfo.ResetColor();
					return true;
				//サイズ
				case "size":
					return customInfo.TryParseSize(arg);
				case "/size":
					customInfo.ResetSize();
					return true;
				//ルビ
				case "ruby":
					return customInfo.TryParseRuby(arg);
				case "/ruby":
					customInfo.ResetRuby();
					return true;
				//傍点
				case "em":
					return customInfo.TryParseEmphasisMark(arg);
				case "/em":
					customInfo.ResetEmphasisMark();
					return true;
				//上付き文字
				case "sup":
					return customInfo.TryParseSuperScript(arg);
				case "/sup":
					customInfo.ResetSuperScript();
					return true;
				//下付き文字
				case "sub":
					return customInfo.TryParseSubScript(arg);
				case "/sub":
					customInfo.ResetSubScript();
					return true;
				//下線
				case TagUnderLine:
					return customInfo.TryParseUnderLine(arg);
				case "/" + TagUnderLine:
					customInfo.ResetUnderLine();
					return true;
				//取り消し線
				case "strike":
					return customInfo.TryParseStrike(arg);
				case "/strike":
					customInfo.ResetStrike();
					return true;
				//グループ文字
				case "group":
					return customInfo.TryParseGroup(arg);
				case "/group":
					customInfo.ResetGroup();
					return true;
				//絵文字
				case "emoji":
					return TryAddEmoji(arg);
				//ダッシュ（ハイフン・横線）
				case "dash":
					AddDash(arg);
					return true;
				//スペース
				case "space":
					return TryAddSpace(arg);
				//変数の文字表示
				case "param":
					{
						string str = ExpressionToString(arg);
						AddStrng(str);
						return true;
					}
				//リンク
				case "link":
					return customInfo.TryParseLink(arg);
				case "/link":
					customInfo.ResetLink();
					return true;
				//TIPS
				case "tips":
					return customInfo.TryParseTips(arg);
				case "/tips":
					customInfo.ResetTips();
					return true;
				//サウンド
				case TagSound:
					return customInfo.TryParseSound(arg);
				case "/"+TagSound:
					customInfo.ResetSound();
					return true;
				//スピード
				case TagSpeed:
					return customInfo.TryParseSpeed(arg);
				case "/"+TagSpeed:
					customInfo.ResetSpeed();
					return true;
				//インターバル
				case "interval":
					return TryAddInterval(arg);
				//フォーマットつき変数表示
				case "format":
					{
						char[] separator = { ':' };
						string[] args = arg.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
						int num = args.Length - 1;
						string[] paramKeys = new string[num];
						Array.Copy(args, 1, paramKeys, 0, num);
						string str = FormatExpressionToString(args[0], paramKeys);
						AddStrng(str);
						return true;
					}
				default:
					return false;
			};
		}

		bool ParseTagParamOnly(string name, string arg)
		{
			switch (name)
			{
				//変数の文字表示
				case "param":
					{
						string str = ExpressionToString(arg);
						AddStrng(str);
						return true;
					}
				//フォーマットつき変数表示
				case "format":
					{
						char[] separator = { ':' };
						string[] args = arg.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
						int num = args.Length - 1;
						string[] paramKeys = new string[num];
						Array.Copy(args, 1, paramKeys, 0, num);
						string str = FormatExpressionToString(args[0], paramKeys);
						AddStrng(str);
						return true;
					}
				default:
					return false;
			};
		}

		/// <summary>
		/// 数式の結果を文字列にする
		/// </summary>
		/// <param name="exp">数式の文字列</param>
		/// <returns>結果の値の文字列</returns>
		string ExpressionToString(string exp)
		{
			if (null == CallbackCalcExpression)
			{
				AddErrorMsg(LanguageErrorMsg.LocalizeTextFormat(Utage.ErrorMsg.TextCallbackCalcExpression));
				return "";
			}
			else
			{
				object obj = CallbackCalcExpression(exp);
				if (obj == null)
				{
					AddErrorMsg(LanguageErrorMsg.LocalizeTextFormat(Utage.ErrorMsg.TextFailedCalcExpression));
					return "";
				}
				else
				{
					return obj.ToString();
				}
			}
		}



		/// <summary>
		/// フォーマットつき数式の結果を文字列にする
		/// </summary>
		/// <param name="format">出力フォーマット</param>
		/// <param name="exps">数式の文字列のテーブル</param>
		/// <returns>結果の値の文字列</returns>
		string FormatExpressionToString(string format, string[] exps)
		{
			if (null == CallbackCalcExpression)
			{
				AddErrorMsg(LanguageErrorMsg.LocalizeTextFormat(Utage.ErrorMsg.TextCallbackCalcExpression));
				return "";
			}
			else
			{
				List<object> args = new List<object>();
				foreach (string exp in exps)
				{
					args.Add(CallbackCalcExpression(exp));
				}
				return string.Format(format, args.ToArray());
			}
		}
	}
}