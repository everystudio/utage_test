// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
#if !UNITY_2018_2_OR_NEWER && !UTGAE_DISABLE_MOVIE
#if !(UNITY_IPHONE || UNITY_ANDROID || UNITY_WEBGL) || UNITY_EDITOR
#define USE_MOVIE_TEXTURE
#endif
#endif

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


namespace Utage
{



	/// <summary>
	/// ムービー再生処理のラッパー
	/// </summary>
	[AddComponentMenu("Utage/Lib/Wrapper/MoviePlayer")]
	public class WrapperMoviePlayer: MonoBehaviour
	{
		public static WrapperMoviePlayer GetInstance() { return instance; }
		static WrapperMoviePlayer instance = null;

		public static void SetRenderTarget(GameObject target){ GetInstance().Target =target; }
		public static void Play(string path, bool isLoop = false){ GetInstance().PlayMovie(path, isLoop); }
		public static void Play(string path, bool isLoop, bool cancel) { GetInstance().PlayMovie(path, isLoop, cancel); }
		public static void Cancel() { GetInstance().CancelMovie(); }
		public static bool IsPlaying() { return GetInstance().isPlaying; }

		bool isPlaying;
		bool cancel;

		public Color bgColor = Color.black;
		public bool ignoreCancel = false;
		public float cancelFadeTime = 0.5f;

		public GameObject Target
		{
			set
			{
				if( renderTarget != value )
				{
#if USE_MOVIE_TEXTURE
					ClearRenderTargetTexture();
#endif
					renderTarget = value;
				}
			}
			get
			{
				if(renderTarget==null)
				{
					return this.gameObject;
				}
				else
				{
					return renderTarget;
				}
			}
		}

		[SerializeField]
		GameObject renderTarget;
#if USE_MOVIE_TEXTURE
#if false
		public List<MovieTexture> movieList;
#endif
		MovieTexture movieTexture;
#endif

		public bool OverrideRootDirectory { get { return overrideRootDirectory; } set { overrideRootDirectory = value; } }
		bool NotOverrideRootDirectory { get { return !OverrideRootDirectory; } }
		[SerializeField]
		bool overrideRootDirectory;


		public string RootDirectory { get { return rootDirectory; } set { rootDirectory = value; } }
		[SerializeField, Hide("NotOverrideRootDirectory")]
		string rootDirectory;

		void Awake()
		{
			if (null != instance)
			{
				Destroy(this.gameObject);
				return;
			}
			else
			{
				instance = this;
			}
		}

		public void PlayMovie(string path, bool isLoop, bool cancel)
		{
			this.cancel = cancel && !ignoreCancel;
#if USE_MOVIE_TEXTURE
			PlayMovieTextue(path, isLoop);
#else
			StartCoroutine(CoPlayMobileMovie(path));
#endif
		}

		public void PlayMovie(string path, bool isLoop)
		{
			PlayMovie(path, isLoop, true);
		}

		public void CancelMovie()
		{
			if (!cancel) return;

#if USE_MOVIE_TEXTURE
			CancelMovieTexture();
#else
#endif
		}

#if USE_MOVIE_TEXTURE
		void PlayMovieTextue(string path, bool isLoop)
		{
			isPlaying = true;
			StartCoroutine(CoPlayMovieFromResources(path, isLoop));

#if false
			string name = FilePathUtil.GetFileNameWithoutExtension(path);
			movieTexture = movieList.Find(item => (item.name == name));
			if (movieTexture)
			{
				StartCoroutine(CoPlayMovieTexture(movieTexture, isLoop));
			}
			else
			{
				if (loadLocalResouces)
				{
					StartCoroutine(CoPlayMovieFromResources(name, isLoop));
				}
				else
				{
					StartCoroutine(CoPlayMovieOGV(name, isLoop));
				}
			}
#endif
		}

		IEnumerator CoPlayMovieTexture(MovieTexture movieTexture, bool isLoop)
		{
			this.movieTexture = movieTexture;
			PlayMovie(isLoop);
			while (movieTexture.isPlaying)
			{
				yield return null;
			}
			yield return StartCoroutine(CoStopMovieTexture());
		}

		IEnumerator CoPlayMovieFromResources(string path, bool isLoop)
		{
			path = FilePathUtil.GetPathWithoutExtension(path);
			MovieTexture movieTexture = Resources.Load<MovieTexture>(path);
			if (movieTexture == null)
			{
				Debug.LogError("Movie canot load from " + path);
				yield break;
			}
			yield return StartCoroutine(CoPlayMovieTexture(movieTexture, isLoop));
		}

		void CancelMovieTexture()
		{
			StartCoroutine(CoCancelMovieTexture());
		}
		IEnumerator CoCancelMovieTexture()
		{
			FadeOutMovie (cancelFadeTime);
			yield return new WaitForSeconds(cancelFadeTime);
			yield return StartCoroutine(CoStopMovieTexture() );
		}

		IEnumerator CoStopMovieTexture()
		{
			if(movieTexture)
			{
				movieTexture.Stop ();
				if (movieTexture.audioClip)
				{
					SoundManager.GetInstance().StopBgm();
				}
			}
			ClearRenderTargetTexture ();
/*			if (loadedLocalResouces)
			{
				Resources.UnloadAsset(movieTexture);				
			}*/
			Resources.UnloadAsset(movieTexture);
			movieTexture = null;
			yield return Resources.UnloadUnusedAssets();
			isPlaying = false;
			StopAllCoroutines();
		}

		void PlayMovie(bool isLoop)
		{
			GameObject target = Target;
			RawImage rawImage = target.GetComponent<RawImage>();
			if(rawImage)
			{
				rawImage.enabled = true;
				rawImage.texture = movieTexture;
			}
			else
			{
				target.GetComponent<Renderer>().material.mainTexture = movieTexture;
			}
			movieTexture.loop = isLoop;
			movieTexture.Play();
			if (movieTexture.audioClip)
			{
				SoundManager.GetInstance().PlayBgm(movieTexture.audioClip, isLoop);
			}
		}

		void FadeOutMovie( float fadeTime )
		{
			GameObject target = Target;
			RawImage rawImage = target.GetComponent<RawImage>();
			if(rawImage)
			{
				rawImage.CrossFadeAlpha(0, fadeTime, true);
			}
			if (movieTexture)
			{
				if (movieTexture.audioClip)
				{
					SoundManager.GetInstance().StopBgm(cancelFadeTime);
				}
			}
		}
		void ClearRenderTargetTexture()
		{
			GameObject target = Target;
			RawImage rawImage = target.GetComponent<RawImage>();
			if(rawImage)
			{
				rawImage.texture = null;
				rawImage.CrossFadeAlpha(1, 0,true);
				rawImage.enabled = false;
			}
			else
			{
				target.GetComponent<Renderer>().material.mainTexture = null;
			}
		}
#elif UNITY_2018_2_OR_NEWER || UNITY_WEBGL || UTGAE_DISABLE_MOVIE
		IEnumerator CoPlayMobileMovie(string path)
		{
			isPlaying = false;
			yield break;
		}

#else
		IEnumerator CoPlayMobileMovie(string path)
		{
			isPlaying = true;
			if (!cancel)
			{
				Handheld.PlayFullScreenMovie(path, bgColor);
			}
			else
			{
				Handheld.PlayFullScreenMovie(path,bgColor,FullScreenMovieControlMode.CancelOnInput);
			}
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			isPlaying = false;
		}
#endif
		string ToStreamingPath(string path)
		{
			return FilePathUtil.Combine( (Application.platform == RuntimePlatform.Android) ? "" : "file://", Application.streamingAssetsPath, path);
		}
	}
}