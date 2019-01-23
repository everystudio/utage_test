// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Utage
{


	/// <summary>
	///  LayoutControllerの拡張クラスを作る場合の基底クラス
	/// </summary>
	[ExecuteInEditMode]
	public abstract class UguiLayoutControllerBase : MonoBehaviour
	{
		RectTransform cachedRectTransform;
		public RectTransform CachedRectTransform { get { if( this.cachedRectTransform == null ) cachedRectTransform = GetComponent<RectTransform>(); return cachedRectTransform; } }

		protected DrivenRectTransformTracker tracker;

		protected virtual void OnEnable()
		{
			SetDirty();
		}

		protected virtual void OnDisable()
		{
			tracker.Clear();
		}


#if UNITY_EDITOR
		protected virtual void OnValidate()
		{
			SetDirty();
		}
#endif

		protected void SetDirty()
		{
			if (!this.gameObject.activeInHierarchy)
				return;

			LayoutRebuilder.MarkLayoutForRebuild(CachedRectTransform);
		}

		protected virtual void Update()
		{
			bool hasChanged = CachedRectTransform.hasChanged;
			if(!hasChanged)
			{
				foreach( RectTransform child in CachedRectTransform )
				{
					if( child.hasChanged )
					{
						hasChanged = true;
						break;
					}
				}
			}
			if (hasChanged)
			{
				SetDirty();
			}
		}
	}
}
