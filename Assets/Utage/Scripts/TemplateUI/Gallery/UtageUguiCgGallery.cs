// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utage;

/// <summary>
/// CGギャラリー画面のサンプル
/// </summary>
[AddComponentMenu("Utage/TemplateUI/CgGallery")]
public class UtageUguiCgGallery : UguiView
{
	public UtageUguiGallery Gallery { get { return this.gallery ?? (this.gallery = FindObjectOfType<UtageUguiGallery>()); } }
	[SerializeField]
	UtageUguiGallery gallery;

	/// <summary>
	/// CG表示画面
	/// </summary>
	public UtageUguiCgGalleryViewer CgView;

	/// <summary>
	/// カテゴリつきのグリッドビュー
	/// </summary>
	[UnityEngine.Serialization.FormerlySerializedAs("categoryGirdPage")]
	public UguiCategoryGridPage categoryGridPage;

	/// <summary>アイテムのリスト</summary>
	List<AdvCgGalleryData> itemDataList = new List<AdvCgGalleryData>();

	/// <summary>ADVエンジン</summary>
	public AdvEngine Engine { get { return this.engine ?? (this.engine = FindObjectOfType<AdvEngine>() as AdvEngine); } }
	[SerializeField]
	AdvEngine engine;

	protected bool isInit = false;
	/*
		void OnEnable()
		{
			OnClose();
			OnOpen();
		}
	*/
	/// <summary>
	/// オープンしたときに呼ばれる
	/// </summary>
	protected virtual void OnOpen()
	{
		StartCoroutine( CoWaitOpen() );
	}

	/// <summary>
	/// クローズしたときに呼ばれる
	/// </summary>
	protected virtual void OnClose()
	{
		categoryGridPage.Clear();
	}

	//ロード待ちしてから開く
	protected virtual IEnumerator CoWaitOpen()
	{
		isInit = false;
		while (Engine.IsWaitBootLoading)
		{
			yield return null;
		}

		categoryGridPage.Init(Engine.DataManager.SettingDataManager.TextureSetting.CreateCgGalleryCategoryList().ToArray(), OpenCurrentCategory);
		isInit = true;
	}

	protected virtual void Update()
	{
		//右クリックで戻る
		if (isInit && InputUtil.IsMouseRightButtonDown())
		{
			Gallery.Back();
		}
	}


	/// <summary>
	/// 現在のカテゴリのページを開く
	/// </summary>
	protected virtual void OpenCurrentCategory(UguiCategoryGridPage categoryGridPage)
	{
		itemDataList = Engine.DataManager.SettingDataManager.TextureSetting.CreateCgGalleryList(Engine.SystemSaveData.GalleryData, categoryGridPage.CurrentCategory);
		categoryGridPage.OpenCurrentCategory(itemDataList.Count, CreateItem);
	}

	/// <summary>
	/// リストビューのアイテムが作成されるときに呼ばれるコールバック
	/// </summary>
	/// <param name="go">作成されたアイテムのGameObject</param>
	/// <param name="index">作成されたアイテムのインデックス</param>
	protected virtual void CreateItem(GameObject go, int index)
	{
		AdvCgGalleryData data = itemDataList[index];
		UtageUguiCgGalleryItem item = go.GetComponent<UtageUguiCgGalleryItem>();
		item.Init(data, OnTap);
	}

	/// <summary>
	/// 各アイテムが押された
	/// </summary>
	/// <param name="button">押されたアイテム</param>
	protected virtual void OnTap(UtageUguiCgGalleryItem item)
	{
		CgView.Open(item.Data);
	}
}
