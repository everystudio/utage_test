// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Utage;
using System;



/// <summary>
/// シーン回想用のUIのサンプル
/// </summary>
[AddComponentMenu("Utage/TemplateUI/SceneGalleryItem")]
public class UtageUguiSceneGalleryItem : MonoBehaviour
{
	public AdvUguiLoadGraphicFile texture;
	public Text title;

	public AdvSceneGallerySettingData Data { get { return data; } }
	protected AdvSceneGallerySettingData data;

	/// <summary>
	/// 初期化
	/// </summary>
	/// <param name="data">セーブデータ</param>
	/// <param name="index">インデックス</param>
	public virtual void Init(AdvSceneGallerySettingData data, Action<UtageUguiSceneGalleryItem> ButtonClickedEvent, AdvSystemSaveData saveData )
	{
		this.data = data;

		UnityEngine.UI.Button button = this.GetComponent<UnityEngine.UI.Button>();
		button.onClick.AddListener( ()=>ButtonClickedEvent(this) );

		bool isOpend = saveData.GalleryData.CheckSceneLabels(data.ScenarioLabel);

		button.interactable = isOpend;
		if (!isOpend)
		{
			texture.gameObject.SetActive(false);
			if (title) title.text = "";
		}
		else{
			texture.gameObject.SetActive(true);
			texture.LoadTextureFile(data.ThumbnailPath);
			if (title) title.text = data.LocalizedTitle;
		}
	}
}
