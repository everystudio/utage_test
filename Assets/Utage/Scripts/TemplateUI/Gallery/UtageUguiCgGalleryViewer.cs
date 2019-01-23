// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Utage;

/// <summary>
/// CGギャラリー画面のサンプル
/// </summary>
[AddComponentMenu("Utage/TemplateUI/CgGalleryViewer")]
public class UtageUguiCgGalleryViewer : UguiView, IPointerClickHandler, IDragHandler, IPointerDownHandler
{
	/// <summary>
	/// ギャラリー選択画面
	/// </summary>
	public UtageUguiGallery gallery;

	/// <summary>
	/// CG表示画面
	/// </summary>
	public AdvUguiLoadGraphicFile texture;
	/// <summary>ADVエンジン</summary>
	public AdvEngine Engine { get { return this.engine ?? (this.engine = FindObjectOfType<AdvEngine>() as AdvEngine); } }
	[SerializeField]
	AdvEngine engine;

	/// <summary>スクロール対応</summary>
	public virtual ScrollRect ScrollRect
	{
		get
		{
			if (scrollRect == null)
			{
				scrollRect = GetComponent<ScrollRect>();
				if (scrollRect == null)
				{
					scrollRect = this.gameObject.AddComponent<ScrollRect>();
					scrollRect.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
				}
				if(scrollRect.content == null)
				{
					scrollRect.content = texture.transform as RectTransform;
				}
			}
			return scrollRect;
		}
	}
	[SerializeField]
	ScrollRect scrollRect;

	[SerializeField]
	bool applyPosition = false;

	protected Vector3 startContentPosition;
	protected bool isEnableClick;
	protected bool isLoadEnd;

	protected AdvCgGalleryData data;
	protected int currentIndex = 0;

	protected virtual void Awake()
	{
		texture.OnLoadEnd.AddListener(OnLoadEnd);
	}
	/// <summary>
	/// オープンしたときに呼ばれる
	/// </summary>
	public void Open(AdvCgGalleryData data)
	{
		gallery.Sleep();
		this.Open();
		this.data = data;
		this.currentIndex = 0;
		this.startContentPosition = ScrollRect.content.localPosition;
		LoadCurrentTexture();
	}

	/// <summary>
	/// クローズしたときに呼ばれる
	/// </summary>
	protected virtual void OnClose()
	{
		ScrollRect.content.localPosition = this.startContentPosition;
		texture.ClearFile();
		gallery.WakeUp();
	}

	protected virtual void Update()
	{
		//右クリックで戻る
		if (InputUtil.IsMouseRightButtonDown())
		{
			Back();
		}
	}

	public virtual void OnPointerDown(PointerEventData eventData)
	{
		if(isLoadEnd) isEnableClick = true;
	}

	public virtual void OnPointerClick(PointerEventData eventData)
	{
		if (!isEnableClick) return;

		++currentIndex;
		if (currentIndex >= data.NumOpen)
		{
			Back();
			return;
		}
		else
		{
			LoadCurrentTexture();
		}
	}


	public virtual void OnDrag(PointerEventData eventData)
	{
		isEnableClick = false;
	}

	protected virtual void LoadCurrentTexture()
	{
		isLoadEnd = false;
		isEnableClick = false;
		ScrollRect.enabled = false;
		ScrollRect.content.localPosition = this.startContentPosition;
		AdvTextureSettingData textureData = data.GetDataOpened(currentIndex);
		texture.LoadFile(Engine.DataManager.SettingDataManager.TextureSetting.LabelToGraphic(textureData.Key).Main);
	}

	protected virtual void OnLoadEnd()
	{
		isLoadEnd = true;
		isEnableClick = false;
		ScrollRect.enabled = true;
		if (applyPosition)
		{
			var graphic = data.GetDataOpened(currentIndex).Graphic.Main;
			texture.transform.localPosition = graphic.Position;
		}
	}
}
