// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utage;
using System;



/// <summary>
/// サウンドルーム用のUIのサンプル
/// </summary>
[AddComponentMenu("Utage/TemplateUI/SoundRoomItem")]
public class UtageUguiSoundRoomItem : MonoBehaviour
{
	/// <summary>本文</summary>
	public Text title;

	public AdvSoundSettingData Data { get { return data; } }
	protected AdvSoundSettingData data;

	/// <summary>
	/// 初期化
	/// </summary>
	/// <param name="data">セーブデータ</param>
	/// <param name="index">インデックス</param>
	public virtual void Init(AdvSoundSettingData data, Action<UtageUguiSoundRoomItem> ButtonClickedEvent, int index)
	{
		this.data = data;
		title.text = data.Title;

		UnityEngine.UI.Button button = this.GetComponent<UnityEngine.UI.Button>();
		button.onClick.AddListener( ()=>ButtonClickedEvent(this) );
	}
}
