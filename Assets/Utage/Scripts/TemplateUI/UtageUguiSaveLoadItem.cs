// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Utage;
using System;

/// <summary>
/// セーブロード用のUIのサンプル
/// </summary>
[AddComponentMenu("Utage/TemplateUI/SaveLoadItem")]
[RequireComponent(typeof(UnityEngine.UI.Button))]	
public class UtageUguiSaveLoadItem : MonoBehaviour
{
	/// <summary>本文</summary>
	public Text text;

	/// <summary>セーブ番号</summary>
	public Text no;

	/// <summary>日付</summary>
	public Text date;

	/// <summary>スクショ</summary>
	public RawImage captureImage;

	/// <summary>オートセーブ用のテクスチャ</summary>
	public Texture2D autoSaveIcon;

	/// <summary>未セーブだった場合に表示するテキスト</summary>
	public string textEmpty = "Empty";

	protected UnityEngine.UI.Button button;

	public AdvSaveData Data { get { return data; } }
	protected AdvSaveData data;

	public int Index { get { return index; } }
	protected int index;

	protected Color defaultColor;

	/// <summary>
	/// 初期化
	/// </summary>
	/// <param name="data">セーブデータ</param>
	/// <param name="index">インデックス</param>
	/// <param name="isSave">セーブ画面用ならtrue、ロード画面用ならfalse</param>
	public virtual void Init(AdvSaveData data, Action<UtageUguiSaveLoadItem> ButtonClickedEvent, int index, bool isSave)
	{
		this.data = data;
		this.index = index;
		this.button = this.GetComponent<UnityEngine.UI.Button>();
		this.button.onClick.AddListener( ()=>ButtonClickedEvent(this) );
		Refresh(isSave);
	}

	public virtual void Refresh(bool isSave)
	{
		no.text = string.Format("No.{0,3}", index);
		if (data.IsSaved)
		{
			if (data.Type == AdvSaveData.SaveDataType.Auto || data.Texture == null )
			{
				if (data.Type == AdvSaveData.SaveDataType.Auto && autoSaveIcon != null)
				{	//オートセーブ用のテクスチャ
					captureImage.texture = autoSaveIcon;
					captureImage.color = Color.white;
				}
				else
				{
					//テクスチャがない
					captureImage.texture = null;
					captureImage.color = Color.black;
				}
			}
			else
			{
				captureImage.texture = data.Texture;
				captureImage.color = Color.white;
			}
			text.text = data.Title;
			date.text = UtageToolKit.DateToStringJp(data.Date);
			button.interactable = true;
		}
		else
		{
			text.text = textEmpty;
			date.text = "";
			button.interactable = isSave;
		}


		//オートセーブデータ
		if (data.Type == AdvSaveData.SaveDataType.Auto)
		{
			no.text = "Auto";
			//セーブはできない
			if (isSave)
			{
				button.interactable = false;
			}
		}		
	}

	protected virtual void OnDestroy()
	{
		if (captureImage != null && captureImage.texture != null)
		{
			captureImage.texture = null;
		}
	}
}
