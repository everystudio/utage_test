// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


namespace Utage
{

	/// <summary>
	/// 背景（画面全体）に対するレイを受け取る
	/// </summary>
	[AddComponentMenu("Utage/Lib/UI/BackgroundRaycastReciever")]
	public class UguiBackgroundRaycastReciever : MonoBehaviour
	{
		public UguiBackgroundRaycaster Raycaster
		{ 
			get { return raycaster ?? ( raycaster = FindObjectOfType<UguiBackgroundRaycaster>() as UguiBackgroundRaycaster ); }
			set { raycaster = value; }
		}
		[SerializeField]
		UguiBackgroundRaycaster raycaster;

		void OnEnable()
		{
			Raycaster.AddTarget(this.gameObject);
		}

		void OnDisable()
		{
			Raycaster.RemoveTarget(this.gameObject);
		}
	}
}

