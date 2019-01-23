// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Utage
{
	/// <summary>
	/// パラメーターのデータ
	/// </summary>	
	public class AdvParamData
	{
		/// <summary>
		/// キー
		/// </summary>
		public string Key { get { return key; } }
		string key;

		/// <summary>
		/// 型
		/// </summary>
		public enum ParamType
		{
			/// <summary>bool</summary>
			Bool,
			/// <summary>float</summary>
			Float,
			/// <summary>int</summary>
			Int,
			/// <summary>string</summary>
			String,
		};

		/// <summary>
		/// 型
		/// </summary>
		public ParamType Type { get { return this.type; } }
		ParamType type;

		/// <summary>
		/// 値
		/// </summary>
		public object Parameter
		{
			get
			{
				if (parameter == null) ParseParameterString();
				return this.parameter;
			}
			set
			{
				switch (type)
				{
					case ParamType.Bool:
						this.parameter = (bool)value;
						break;
					case ParamType.Float:
						this.parameter = ExpressionCast.ToFloat(value);
						break;
					case ParamType.Int:
						this.parameter = ExpressionCast.ToInt(value);
						break;
					case ParamType.String:
						this.parameter = (string)value;
						break;
				}
				parameterString = parameter.ToString();
			}
		}
		object parameter;

		public string ParameterString { get { return parameterString; } }
		string parameterString;

		/// <summary>
		/// ファイルタイプ
		/// </summary>
		public enum FileType
		{
			/// <summary>通常</summary>
			Default,
			/// <summary>システムセーブデータ</summary>
			System,
			/// <summary>Const（一定の値。代入やセーブロードの対象にならない）</summary>
			Const,
		};

		/// <summary>
		/// 型
		/// </summary>
		public FileType SaveFileType { get { return this.fileType; } }
		FileType fileType;

		public bool TryParse(string name, string type, string fileType)
		{
			this.key = name;
			if (!ParserUtil.TryParaseEnum<ParamType>(type, out this.type))
			{
				Debug.LogError(type + " is not ParamType");
				return false;
			}
			if (string.IsNullOrEmpty(fileType))
			{
				this.fileType = FileType.Default;
			}
			else
			{
				if (!ParserUtil.TryParaseEnum<FileType>(fileType, out this.fileType))
				{
					Debug.LogError(fileType + " is not FileType");
					return false;
				}
			}
			return true;
		}

		public bool TryParse(AdvParamData src, string value)
		{
			this.key = src.Key;
			this.type = src.Type;
			this.fileType = src.SaveFileType;
			this.parameterString = value;
			try
			{
				ParseParameterString();
				return true;
			}
			catch
			{
				return false;
			}
		}

		public bool TryParse(StringGridRow row)
		{
			string key = AdvParser.ParseCell<string>(row, AdvColumnName.Label);
			if (string.IsNullOrEmpty(key))
			{
				return false;
			}
			else
			{
				this.key = key;
				this.type = AdvParser.ParseCell<ParamType>(row, AdvColumnName.Type);
				this.parameterString = AdvParser.ParseCellOptional<string>(row, AdvColumnName.Value, "");
				this.fileType = AdvParser.ParseCellOptional<FileType>(row, AdvColumnName.FileType, FileType.Default);
				try
				{
					ParseParameterString();
					return true;
				}
				catch
				{
					return false;
				}
			}
		}

		public void Copy(AdvParamData src)
		{
			this.key = src.Key;
			this.type = src.type;
			this.parameterString = src.parameterString;
			ParseParameterString();
			this.fileType = src.fileType;
		}

		public void CopySaveData(AdvParamData src)
		{
			if (this.key != src.Key) Debug.LogError(src.key + "is diffent name of Saved param");
			if (this.type != src.type) Debug.LogError(src.type + "is diffent type of Saved param");
			if (this.fileType != src.fileType) Debug.LogError(src.fileType + "is diffent fileType of Saved param");
			this.parameterString = src.parameterString;
			ParseParameterString();
		}


		//セーブデータ用の読み込み
		public void Read(string paramString)
		{
			this.parameterString = paramString;
			ParseParameterString();
		}

		void ParseParameterString()
		{
			switch (type)
			{
				case ParamType.Bool:
					parameter = bool.Parse(parameterString);
					break;
				case ParamType.Float:
					parameter = WrapperUnityVersion.ParseFloatGlobal(parameterString);
					break;
				case ParamType.Int:
					parameter = int.Parse(parameterString);
					break;
				case ParamType.String:
					parameter = parameterString;
					break;
			}
		}
	}
}
