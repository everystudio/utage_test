// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utage;

/// <summary>
/// CGギャラリー画面のサンプル
/// </summary>
[AddComponentMenu("Utage/TemplateUI/SceneGallery")]
public class UtageUguiSceneGallery : UguiView
{
	/// <summary>
	/// カテゴリつきのグリッドビュー
	/// </summary>
	[UnityEngine.Serialization.FormerlySerializedAs("categoryGirdPage")]
	public UguiCategoryGridPage categoryGridPage;

	/// <summary>
	/// ギャラリー選択画面
	/// </summary>
	public UtageUguiGallery Gallery { get { return this.gallery ?? (this.gallery = FindObjectOfType<UtageUguiGallery>()); } }
	public UtageUguiGallery gallery;

	/// <summary>
	/// メインゲーム画面
	/// </summary>
	public UtageUguiMainGame mainGame;

	/// <summary>ADVエンジン</summary>
	public AdvEngine Engine { get { return this.engine ?? (this.engine = FindObjectOfType<AdvEngine>() as AdvEngine); } }
	[SerializeField]
	AdvEngine engine;

	protected bool isInit = false;

	/// <summary>アイテムのリスト</summary>
	protected List<AdvSceneGallerySettingData> itemDataList = new List<AdvSceneGallerySettingData>();

	protected virtual void OnEnable()
	{
		OnClose();
		OnOpen();
	}

	/// <summary>
	/// オープンしたときに呼ばれる
	/// </summary>
	protected virtual void OnOpen()
	{
		this.ChangeBgm();
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

		categoryGridPage.Init(Engine.DataManager.SettingDataManager.SceneGallerySetting.CreateCategoryList().ToArray(), OpenCurrentCategory);
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
		itemDataList = Engine.DataManager.SettingDataManager.SceneGallerySetting.CreateGalleryDataList(categoryGridPage.CurrentCategory);
		categoryGridPage.OpenCurrentCategory(itemDataList.Count, CreateItem);
	}

	/// <summary>
	/// リストビューのアイテムが作成されるときに呼ばれるコールバック
	/// </summary>
	/// <param name="go">作成されたアイテムのGameObject</param>
	/// <param name="index">作成されたアイテムのインデックス</param>
	protected virtual void CreateItem(GameObject go, int index)
	{
		AdvSceneGallerySettingData data = itemDataList[index];
		UtageUguiSceneGalleryItem item = go.GetComponent<UtageUguiSceneGalleryItem>();
		item.Init(data, OnTap, Engine.SystemSaveData);
	}

	/// <summary>
	/// 各アイテムが押された
	/// </summary>
	/// <param name="button">押されたアイテム</param>
	protected virtual void OnTap(UtageUguiSceneGalleryItem item)
	{
		gallery.Close();
		mainGame.OpenSceneGallery(item.Data.ScenarioLabel);
	}
}
