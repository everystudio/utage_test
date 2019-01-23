// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UtageExtensions;

namespace Utage
{


	/// <summary>
	/// リストビュー管理コンポーネント
	/// </summary>
	[RequireComponent(typeof(ScrollRect))]
	[AddComponentMenu("Utage/Lib/UI/ListView")]
	public class UguiListView : MonoBehaviour
	{
		public enum Type
		{
			Horizontal,
			Vertical,
		};
		public Type ScrollType { get { return this.scrollType; } }
		[SerializeField]
		Type scrollType = Type.Horizontal;

		//リストビューの表示するアイテムのプレハブ
		public GameObject ItemPrefab
		{
			get { return itemPrefab; }
			set { itemPrefab = value; }
		}
		[SerializeField]
		GameObject itemPrefab;

		//アイテムのルート
		public RectTransform Content{ get { return this.content ?? (content = ScrollRect.content); } }
		RectTransform content;

		//全てのスクロール内だったら自動でスクロール機能をオフに
		public bool IsStopScroolWithAllInnner { get { return isStopScroolWithAllInnner; } }
		[SerializeField]
		bool isStopScroolWithAllInnner = true;

		//初期状態を中央ぞろえに
		public bool IsAutoCenteringOnRepostion { get { return isAutoCenteringOnRepostion; } }
		[SerializeField]
		bool isAutoCenteringOnRepostion = false;

		//
		public UguiAlignGroup PositionGroup
		{ 
			get
			{
				if (this.positionGroup == null)
				{
					positionGroup = Content.GetComponent<UguiAlignGroup>();
					if (positionGroup == null)
					{
						Debug.LogError("AlignGroup Component is not attached on ScrollRect Content");
					}
				}
				return this.positionGroup;
			}
		}
		UguiAlignGroup positionGroup;

		public ScrollRect ScrollRect { get { return this.scrollRect ?? (scrollRect = GetComponent<ScrollRect>()); } }
		ScrollRect scrollRect;

		public RectTransform ScrollRectTransform { get { return this.scrollRectTransform ?? (scrollRectTransform = ScrollRect.GetComponent<RectTransform>()); } }
		RectTransform scrollRectTransform;

		//表示範囲外にインデックスの小さいアイテム(右や上側のアイテム)があるのを知らせる表示オブジェクト
		public GameObject MinArrow
		{
			get { return minArrow; }
			set { minArrow = value; }
		}
		[SerializeField]
		GameObject minArrow;

		//表示範囲外にインデックスの大きいアイテム(左や下側のアイテム)があるのを知らせる表示オブジェクト
		public GameObject MaxArrow
		{
			get { return maxArrow; }
			set { maxArrow = value; }
		}
		[SerializeField]
		GameObject maxArrow;

		public List<GameObject> Items { get { return items; } }
		List<GameObject> items = new List<GameObject>();
		
		/// <summary>
		/// アイテムを作成
		/// </summary>
		/// <param name="itemNum">アイテムの数</param>
		/// <param name="callbackCreateItem">アイテムが作成されるときに呼ばれるコールバック</param>
		public void CreateItems(int itemNum, System.Action<GameObject, int> callbackCreateItem)
		{
			ClearItems();
			for (int i = 0; i < itemNum; ++i)
			{
				GameObject go = Content.AddChildPrefab(ItemPrefab.gameObject);
				items.Add(go);
				if (null != callbackCreateItem) callbackCreateItem(go, i);
			}
			Reposition();
		}

		/// <summary>
		/// アイテムを追加
		/// </summary>
		/// <param name="itemNum">アイテムの数</param>
		/// <param name="callbackCreateItem">アイテムが作成されるときに呼ばれるコールバック</param>
		public void AddItems( List<GameObject> items )
		{
			foreach( var item in items )
			{
				Content.AddChild(item);
			}
		}

		public void Reposition()
		{
			Content.anchoredPosition = Vector2.zero;
			ScrollRect.velocity = Vector2.zero;
			PositionGroup.Reposition();
			bool isStopScrool = IsContentInnerScrollRect() && IsStopScroolWithAllInnner;
			switch (ScrollType)
			{
				case Type.Horizontal:
					ScrollRect.horizontal = !isStopScrool;
					ScrollRect.vertical = false;
					if (isAutoCenteringOnRepostion)
					{
						if (isStopScrool)
						{
							float offset = (this.ScrollRectTransform.sizeDelta.x - Content.sizeDelta.x) / 2;
							Content.anchoredPosition = new Vector3(offset, 0, 0);
						}
						else
						{
							ScrollRect.horizontalNormalizedPosition = 0.5f;
						}
					}
					break;
				case Type.Vertical:
					ScrollRect.horizontal = false;
					ScrollRect.vertical = !isStopScrool;
					if (isAutoCenteringOnRepostion)
					{
						if (isStopScrool)
						{
							float offset = -(this.ScrollRectTransform.sizeDelta.y - Content.sizeDelta.y) / 2;
							Content.anchoredPosition = new Vector3(0, offset, 0);
						}
						else
						{
							ScrollRect.verticalNormalizedPosition = 0.5f;
						}
					}
					break;
			}
			ScrollRect.enabled = !isStopScrool;
		}

		//アイテムを全消去
		public void ClearItems()
		{
			items.Clear();
			Content.DestroyChildren();
			ScrollRect.velocity = Vector2.zero;
		}

		void Update()
		{
			RefreshArrow ();
		}

		void RefreshArrow()
		{
			if (IsContentInnerScrollRect()) 
			{
				if (null != MinArrow) MinArrow.SetActive (false);
				if (null != MaxArrow) MaxArrow.SetActive (false);
			}
			else
			{
				float normal;
				switch(ScrollType)
				{
				case Type.Horizontal:
					normal = ScrollRect.horizontalNormalizedPosition;
					if (null != MinArrow) MinArrow.SetActive(normal > 0);
					if (null != MaxArrow) MaxArrow.SetActive(normal < 1);
					break;
				case Type.Vertical:
					normal = ScrollRect.verticalNormalizedPosition;
					if (null != MinArrow) MinArrow.SetActive(normal < 1);
					if (null != MaxArrow) MaxArrow.SetActive(normal > 0);
					break;
				}
			}
		}

		bool IsContentInnerScrollRect()
		{
			switch(ScrollType)
			{
			case Type.Horizontal:
				return Content.rect.width <= ScrollRectTransform.rect.width;
			case Type.Vertical:
				return Content.rect.height <= ScrollRectTransform.rect.height;
			}
			return false;
		}
	}
}