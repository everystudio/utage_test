// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utage;
using System.Collections;
using System.Collections.Generic;
using System;



/// <summary>
/// サウンドルーム用のUIのサンプル
/// </summary>
[AddComponentMenu("Utage/TemplateUI/CgGalleryItem")]
public class UtageUguiCgGalleryItem : MonoBehaviour
{
	public AdvUguiLoadGraphicFile texture;
	public Text count;

	public AdvCgGalleryData Data { get { return data; } }
	AdvCgGalleryData data;

	/// <summary>
	/// 初期化
	/// </summary>
	/// <param name="data">セーブデータ</param>
	/// <param name="index">インデックス</param>
	public virtual void Init(AdvCgGalleryData data, Action<UtageUguiCgGalleryItem> ButtonClickedEvent)
	{
		this.data = data;
		UnityEngine.UI.Button button = this.GetComponent<UnityEngine.UI.Button>();
		button.onClick.AddListener( ()=>ButtonClickedEvent(this) );

		bool isOpen = (data.NumOpen > 0);
		button.interactable = isOpen;
		if (isOpen)
		{
			texture.gameObject.SetActive(true);
			texture.LoadTextureFile(data.ThumbnailPath);
			count.text = string.Format("{0,2}/{1,2}", data.NumOpen, data.NumTotal);
		}
		else
		{
			texture.gameObject.SetActive(false);
			count.text = "";
		}
	}
}
