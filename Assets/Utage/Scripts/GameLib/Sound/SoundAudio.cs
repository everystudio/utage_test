using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// サウンド再生コンポーネント
	/// オーディオ1オブジェクトに相当する
	/// 基本はシステム内部で使うので外から呼ばないこと
	/// </summary>
	[AddComponentMenu("Utage/Lib/Sound/Audio")]
	internal class SoundAudio : MonoBehaviour
	{
		/// <summary>
		/// 状態
		/// </summary>
		enum SoundStreamStatus
		{
			None,
			FadeIn,     //フェードイン中
			Play,		//プレイ中
			FadeOut,    //フェードアウト中
		};
		SoundStreamStatus status = SoundStreamStatus.None;
		SoundStreamStatus Status { get { return status; } }

		//現在鳴らしているオーディオソース
		public AudioSource AudioSource { get; private set; }
		//イントロループするときのオーディオソース
		AudioSource AudioSourceForIntroLoop { get; set; }

		//イントロループ用の2つオーディオソースを用意して必要に応じて切り替える
		AudioSource Audio0 { get; set; }
		AudioSource Audio1 { get; set; }

		//オーディオの情報
		internal SoundData Data { get; private set; }

		//グループ情報
		SoundAudioPlayer Player { get; set; }

		internal bool IsLoading{ get; private set; }


		//フェードの値
		LinearValue fadeValue = new LinearValue();

		//セーブが有効かどうか
		internal bool EnableSave
		{
			get
			{
				switch (Status)
				{
					case SoundStreamStatus.FadeIn:
					case SoundStreamStatus.Play:
						return Data.EnableSave;
					default:
						return false;
				}
			}
		}

		//初期化
		public void Init( SoundAudioPlayer player, SoundData soundData )
		{
			this.Player = player;
			this.Data = soundData;
			this.Audio0 = this.gameObject.AddComponent<AudioSource>();
			Audio0.playOnAwake = false;
			if (Data.EnableIntroLoop)
			{
				Audio1 = this.gameObject.AddComponent<AudioSource>();
				Audio1.playOnAwake = false;
				Audio1.clip = Data.Clip;
				Audio1.loop = false;
			}
			AudioSource = Audio0;

			AudioSource.clip = Data.Clip;
			AudioSource.loop = Data.IsLoop && !Data.EnableIntroLoop;
			if (Data.File != null)
			{
				Data.File.AddReferenceComponent(this.gameObject);
			}
		}

		void OnDestroy()
		{
			Player.Remove(this);
		}

		//鳴らす
		internal void Play(float fadeInTime, float delay = 0)
		{
			StartCoroutine( CoWaitDelay(fadeInTime, delay) );
		}

		IEnumerator CoWaitDelay(float fadeInTime, float delay)
		{
			IsLoading = (Data.File != null && !Data.File.IsLoadEnd);
			if (IsLoading)
			{
				AssetFileManager.Load(Data.File, this);
			}
			if (delay > 0)
			{
				yield return new WaitForSeconds(delay);
			}
			if (IsLoading)
			{
				while (!Data.File.IsLoadEnd) yield return null;
				Data.File.Unuse(this);
			}
			IsLoading = false;
			PlaySub(fadeInTime);
		}

		void PlaySub(float fadeInTime )
		{
			float volume = GetVolume();
			AudioSource.clip = Data.Clip;
			if (Data.EnableIntroLoop)
			{
				Audio1.clip = Data.Clip;
				Audio1.volume = volume;
			}

			if (fadeInTime > 0)
			{
				status = SoundStreamStatus.FadeIn;
				fadeValue.Init(fadeInTime, fadeValue.GetValue(), 1);
			}
			else
			{
				status = SoundStreamStatus.Play;
				fadeValue.Init(0, 1, 1);
			}

			AudioSource.volume = volume;
			if (Data.EnableIntroLoop)
			{
				//イントロループする場合はPlayだとズレるので、PlayScheduledで正確に
				//ただしちょっと遅らせる必要あり
				const float IntroDelay = 0.1f;
				AudioSource.PlayScheduled(AudioSettings.dspTime + IntroDelay);
			}
			else
			{
				AudioSource.Play();
			}
		}

		//終了
		public void End()
		{
			if (Audio0 != null)
			{
				Audio0.Stop();
			}
			if (Audio1 != null)
			{
				Audio1.Stop();
			}
			GameObject.Destroy(this.gameObject);
		}

		//フェードアウト終了
		void EndFadeOut()
		{
			End();
		}

		//指定のサウンドかどうか
		public bool IsEqualClip(AudioClip clip)
		{
			return (AudioSource != null && AudioSource.clip == clip);
		}

		//指定のサウンドが鳴っているか
		public bool IsPlaying(AudioClip clip)
		{
			if (IsEqualClip(clip) && IsPlaying())
			{
				return (status == SoundStreamStatus.Play);
			}
			return false;
		}
		//サウンドが鳴っているか
		public bool IsPlaying()
		{
			switch (status)
			{
				case SoundStreamStatus.FadeIn:
				case SoundStreamStatus.Play:
					return true;
				default:
					return false;
			}
		}

		//ループ再生中
		public bool IsPlayingLoop()
		{
			return IsPlaying() && Data.IsLoop;
		}

		//指定時間フェードアウトして終了
		public void FadeOut(float fadeTime)
		{
			if (fadeTime > 0 && IsPlaying() )
			{
				status = SoundStreamStatus.FadeOut;
				fadeValue.Init(fadeTime, fadeValue.GetValue(), 0);
			}
			else
			{
				EndFadeOut();
			}
		}

		void Update()
		{
			switch (status)
			{
				case SoundStreamStatus.Play:
					IntroUpdate();
					UpdatePlay();
					break;
				case SoundStreamStatus.FadeIn:
					IntroUpdate();
					UpdateFadeIn();
					break;
				case SoundStreamStatus.FadeOut:
					IntroUpdate();
					UpdateFadeOut();
					break;
				default:
					break;
			}
		}

		void IntroUpdate()
		{
			//イントロループしないなら関係ない
			if (!Data.EnableIntroLoop) return;

			if (AudioSourceForIntroLoop == null)
			{
				if (AudioSource.time > 0)
				{
					SetNextIntroLoop();
				}
			}
			//今のサウンドがもう切り替わっている
			if (IsEndCurrentAudio())
			{
				AudioSource = AudioSourceForIntroLoop;
				//本来は必要ないが、ブレークポイントなどでプロセスを止められたときに
				//オーディオが止まっている可能性があるので復帰させる
				if (AudioSource !=null && !AudioSource.isPlaying) AudioSource.Play();

				SetNextIntroLoop();
			}
		}

		//次のイントロループを設定
		void SetNextIntroLoop()
		{
			//イントロループ用のオーディオを設定
			AudioSourceForIntroLoop = (AudioSource == Audio0) ? Audio1 : Audio0;

			//止めておかないと遅い
			AudioSourceForIntroLoop.Stop();
			//ループポイントから再生開始
			AudioSourceForIntroLoop.time = Data.IntroTime;

			//イントロループ用のオーディオは、今のオーディオが鳴り終るタイミングに合わせた時間を登録して鳴らす
			if (AudioSource != null && AudioSource.clip != null)
			{
				float delay = Mathf.Max(0, AudioSource.clip.length - AudioSource.time);
				AudioSourceForIntroLoop.PlayScheduled(AudioSettings.dspTime + delay);
			}
		}


		//今のサウンドがもう終わっているか？
		bool IsEndCurrentAudio()
		{
			if (AudioSource == null)
			{
				return false;
			}
			if (AudioSource.isPlaying)
			{
				return false;
			}
			else
			{
				//単純に鳴っていないだけだとバックグラウンドに回ったときとかもありうるのでちゃんと長さもチェック
				if (AudioSource.clip.length - AudioSource.time < 0.001f)
				{
					return true;
				}
				else if (Mathf.Approximately(AudioSource.time, 0) || Mathf.Approximately( AudioSource.time, Data.IntroTime) )
				{
					//開始時刻で止まっている場合がある
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		//通常再生
		void UpdatePlay()
		{
			//ループしないなら終了
			if (!Data.IsLoop && IsEndCurrentAudio() )
			{
				EndFadeOut();
			}
		}

		//フェードイン処理
		void UpdateFadeIn()
		{
			fadeValue.IncTime();
			if (fadeValue.IsEnd())
			{
				status = SoundStreamStatus.Play;
			}
		}

		//フェードアウト処理
		void UpdateFadeOut()
		{
			fadeValue.IncTime();
			if (fadeValue.IsEnd())
			{
				EndFadeOut();
			}
		}

#if UNITY_EDITOR
		[NotEditable]
		public float time0 = 0;
		[NotEditable]
		public float time1 = 0;
#endif
		void LateUpdate()
		{
#if UNITY_EDITOR
			if (Audio0 != null) time0 = Audio0.time;
			if (Audio1 != null) time1 = Audio1.time;
#endif

			//ボリュームの更新
			if (AudioSource == null) return;

			float volume = GetVolume();
			if (!Mathf.Approximately(volume, AudioSource.volume))
			{
				if (Audio0 != null)
				{
					Audio0.volume = volume;
				}
				if (Audio1 != null)
				{
					Audio1.volume = volume;
				}
			}
		}

		//ボリューム計算
		float GetVolume()
		{
			float volume = fadeValue.GetValue() * Data.Volume * Player.Group.GetVolume(Data.Tag);
			return volume;
		}

		//現在鳴っているボリュームを取得
		public float GetSamplesVolume()
		{
			if (AudioSource.isPlaying)
			{
				return GetSamplesVolume(AudioSource);
			}
			else
			{
				return 0;
			}
		}

		// オーディオのボリュームを取得
		const int samples = 256;
		const int channel = 0;
		static float[] waveData = new float[samples];
		float GetSamplesVolume(AudioSource audio)
		{
			audio.GetOutputData(waveData, channel);
			float sum = 0;
			foreach (float s in waveData)
			{
				sum += Mathf.Abs(s);
			}
			return (sum / samples);
		}
	};
}
