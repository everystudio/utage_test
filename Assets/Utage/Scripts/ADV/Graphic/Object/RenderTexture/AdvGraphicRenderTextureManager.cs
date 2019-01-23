// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UtageExtensions;

namespace Utage
{

	/// <summary>
	/// テクスチャ書き込みクラス
	/// </summary>
	[AddComponentMenu("Utage/ADV/GraphicRenderTextureManager")]
	public class AdvGraphicRenderTextureManager : MonoBehaviour
	{
		public float offset = 10000;

		List<AdvRenderTextureSpace> spaceList = new List<AdvRenderTextureSpace>();

		//テクスチャ書き込み用の空間（カメラ・キャンバス・オブジェクト）を追加
		internal AdvRenderTextureSpace CreateSpace()
		{
			AdvRenderTextureSpace space = this.transform.AddChildGameObjectComponent<AdvRenderTextureSpace>("RenderTextureSpace");
			int index = 0;
			for ( ; index < spaceList.Count; ++index)
			{
				if (spaceList[index] == null)
				{
					spaceList[index] = space;
					break;
				}
			}
			if (index>= spaceList.Count)
			{
				spaceList.Add(space);
			}
			//描画領域が重複しないように、有り得ないほど遠くに置く
			space.transform.localPosition = new Vector3(0, (index + 1) * offset, 0);
			return space;
		}
	}
}
