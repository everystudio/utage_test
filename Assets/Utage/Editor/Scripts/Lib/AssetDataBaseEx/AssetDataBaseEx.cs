// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace Utage
{

	/// <summary>
	/// 自分用に使いやすくしたアセットデータベースを
	/// </summary>
	public class AssetDataBaseEx
	{
		//指定のパスのメインアセット情報を取得
		static MainAssetInfo GetAsset(string assetPath)
		{
			return new MainAssetInfo(assetPath);
		}
	}
}