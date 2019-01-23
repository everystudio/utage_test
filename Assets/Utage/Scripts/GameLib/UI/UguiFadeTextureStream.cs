// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


namespace Utage
{

	/// <summary>
	/// テクスチャをフェード切り替えしながら次々に表示する
	/// </summary>
	[RequireComponent(typeof(RawImage))]
	[AddComponentMenu("Utage/Lib/UI/FadeTextureStream")]
	public class UguiFadeTextureStream : MonoBehaviour, IPointerClickHandler
	{
		public bool allowSkip = true;
		public bool allowAllSkip = false;

		[System.Serializable]
		public class FadeTextureInfo
		{
			public Texture texture;
			public string moviePath;
			public float fadeInTime = 0.5f;
			public float duration = 3.0f;
			public float fadeOutTime = 0.5f;
			public bool allowSkip = false;
		}
		public FadeTextureInfo[] fadeTextures = new FadeTextureInfo[1];

		bool isInput;
		public void OnPointerClick(PointerEventData eventData)
		{
			isInput = true;
		}
		bool IsInputSkip( FadeTextureInfo info )
		{
			return (isInput && (allowSkip || info.allowSkip));
		}

		bool IsInputAllSkip { get { return isInput && allowAllSkip; } }

		void LateUpdate()
		{
			isInput = false;
		}

		public void Play()
		{
			StartCoroutine(CoPlay());
		}
		public bool IsPlaying { get{ return isPlaying; } }
		bool isPlaying;

		IEnumerator CoPlay()
		{
			isPlaying = true;
			RawImage rawImage = GetComponent<RawImage>();
			rawImage.CrossFadeAlpha(0, 0, true);

			foreach( FadeTextureInfo info in fadeTextures )
			{
				rawImage.texture = info.texture;
				bool allSkip = false;

				if (info.texture)
				{
					rawImage.CrossFadeAlpha(1, info.fadeInTime, true);
					float time = 0;
					while (!IsInputSkip(info))
					{
						yield return null;
						time += Time.deltaTime;
						if (time > info.fadeInTime) break;
					}
					time = 0;
					while (!IsInputSkip(info))
					{
						yield return null;
						time += Time.deltaTime;
						if (time > info.duration) break;
					}
					allSkip = IsInputAllSkip;
					rawImage.CrossFadeAlpha(0, info.fadeOutTime, true);
					yield return new WaitForSeconds(info.fadeOutTime);
				}
				else if (!string.IsNullOrEmpty(info.moviePath))
				{
					WrapperMoviePlayer.Play(info.moviePath);
					while (WrapperMoviePlayer.IsPlaying() )
					{
						yield return null;
						if (IsInputSkip(info))
						{
							WrapperMoviePlayer.Cancel();
						}
						allSkip = IsInputAllSkip;
					}
				}
				if (allSkip) break;
				yield return null;
			}
			isPlaying = false;
		}
	}
}