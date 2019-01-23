// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Utage
{

	/// <summary>
	/// グラフィックオブジェクトのデータ
	/// </summary>
	internal interface IAdvClickEvent
	{
		GameObject gameObject { get; }

		/// <summary>
		/// クリックイベントを設定
		/// </summary>
		void AddClickEvent(bool isPolygon, StringGridRow row, UnityAction<BaseEventData> action);

		/// <summary>
		/// クリックイベントを削除
		/// </summary>
		void RemoveClickEvent();
	}
}
