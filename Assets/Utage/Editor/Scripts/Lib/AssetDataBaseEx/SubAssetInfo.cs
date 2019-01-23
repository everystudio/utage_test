// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace Utage
{

	/// <summary>
	/// アセットの情報
	/// </summary>
	public class SubAssetInfo : AssetInfo
	{
		public SubAssetInfo(Object asset, MainAssetInfo mainAsset)
			: base(asset)
		{
			this.MainAsset = mainAsset;
		}
		public MainAssetInfo MainAsset { get; protected set; }

		public override bool IsMainAsset{ get{return false;}}
	}
}
