// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Utage
{

	/// <summary>
	/// カテゴリタブつきのグリッドページ
	/// </summary>
	[AddComponentMenu("Utage/Lib/UI/CategoryGridPage")]
	public class UguiCategoryGridPage : MonoBehaviour
	{
		/// <summary>
		/// グリッドビュー
		/// </summary>
		public UguiGridPage gridPage;

		/// <summary>
		/// タブグループ
		/// </summary>
		public UguiToggleGroupIndexed categoryToggleGroup;
		public UguiAlignGroup categoryAlignGroup;
		public GameObject categoryPrefab;

		/// <summary>
		/// ボタンのSpriteリスト
		/// </summary>
		public List<Sprite> buttonSpriteList;

		/// <summary>カテゴリのリスト</summary>
		string[] categoryList;

		//現在のカテゴリ
		public string CurrentCategory
		{
			get
			{
				if (categoryList == null) return "";
				else if (categoryToggleGroup.CurrentIndex >= categoryList.Length) return "";
				else return categoryList[categoryToggleGroup.CurrentIndex];
			}
		}

		public void Clear()
		{
			categoryToggleGroup.ClearToggles();
			categoryAlignGroup.DestroyAllChildren();
			gridPage.ClearItems();
		}

		public void Init(string[] categoryList, System.Action<UguiCategoryGridPage> OpenCurrentCategory)
		{
			this.categoryList = categoryList;
			categoryToggleGroup.ClearToggles();
			categoryAlignGroup.DestroyAllChildren();
			if (categoryList.Length > 1)
			{
				List<GameObject> children = categoryAlignGroup.AddChildrenFromPrefab( categoryList.Length, categoryPrefab, CreateTabButton );
				foreach( GameObject go in children )
				{
					categoryToggleGroup.Add(go.GetComponent<Toggle>());
				}

				categoryToggleGroup.CurrentIndex = 0;
			}

			categoryToggleGroup.OnValueChanged.AddListener((int index) => OpenCurrentCategory(this) );
			OpenCurrentCategory(this);
		}

		/// <summary>
		/// リストビューのアイテムが作成されるときに呼ばれるコールバック
		/// </summary>
		/// <param name="go">作成されたアイテムのGameObject</param>
		/// <param name="index">作成されたアイテムのインデックス</param>
		void CreateTabButton(GameObject go, int index)
		{
			Text text = go.GetComponentInChildren<Text>();
			if (text && index < categoryList.Length) text.text = categoryList[index];

			Image image = go.GetComponentInChildren<Image>();
			if (image && index < buttonSpriteList.Count) image.sprite = buttonSpriteList[index];
		}

		public void OpenCurrentCategory(int itemCount, System.Action<GameObject, int> CreateItem)
		{
			gridPage.Init(itemCount, CreateItem);
			gridPage.CreateItems(0);
		}
	}
}
