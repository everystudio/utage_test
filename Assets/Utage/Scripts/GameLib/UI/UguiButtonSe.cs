// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


namespace Utage
{

	/// <summary>
	/// イベントを受け取ってSEを鳴らす
	/// </summary>
	[AddComponentMenu("Utage/Lib/UI/ButtonSe")]
	public class UguiButtonSe : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, ISubmitHandler, IMoveHandler
	{
		Selectable Selectable { get { return selectable ?? (selectable = GetComponent<Selectable>()); } }
		Selectable selectable;

		//クリック時のSE
		public AudioClip clicked;
		
		//ハイライト時のSE
		public AudioClip highlited;

		//同じSEが鳴っていたら鳴らさないとか前のを止めるとか
		public SoundPlayMode clickedPlayMode = SoundPlayMode.Add;
		//同じSEが鳴っていたら鳴らさないとか前のを止めるとか
		public SoundPlayMode highlitedPlayMode = SoundPlayMode.Add;

		// クリックイベントでSEを鳴らす
		public void OnPointerClick(PointerEventData data)
		{
			PlayeSe(clickedPlayMode, clicked);
		}

		// ハイライトでSEを鳴らす
		public void OnPointerEnter(PointerEventData data)
		{
			PlayeSe(highlitedPlayMode, highlited);
		}

		// 決定でSEを鳴らす
		public void OnSubmit(BaseEventData eventData)
		{
			PlayeSe(clickedPlayMode, clicked);
		}

		// キー移動でSE鳴らす
		public void OnMove(AxisEventData eventData)
		{
			if (eventData.selectedObject == this.gameObject) return;
			PlayeSe(highlitedPlayMode, highlited);
		}

		void PlayeSe(SoundPlayMode playMode, AudioClip clip)
		{
			if (Selectable == null) return;
			if (!Selectable.interactable) return;

			if (clip != null)
			{
				SoundManager soundManager = SoundManager.GetInstance();

				if (soundManager)
				{
					soundManager.PlaySe(clip, clip.name, playMode);
				}
				else
				{
					AudioSource.PlayClipAtPoint(clip, Vector3.zero);
				}
			}
		}
	}
}