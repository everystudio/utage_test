﻿// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Utage
{
	[System.Serializable]
	public class TimerEvent: UnityEvent<Timer>{	}

	//指定の時間経過を処理するタイマー
	//主に一定時間をかけて変化するちょっとしたアニメーションなどににつかう
	[AddComponentMenu("Utage/Lib/Sound/Timer")]
	public class Timer : MonoBehaviour
	{
		//期間（秒）
		public float Duration { get { return duration; } protected set { duration = value; } }
		[SerializeField]
		float duration;

		//開始遅延時間
		public float Delay { get { return delay; } protected set { delay = value; } }
		[SerializeField]
		float delay;

		//経過時間
		public float Time { get { return time; } protected set { time = value; } }
		[SerializeField, NotEditable]
		float time;

		//経過時間の係数(0～１)
		public float Time01 { get { return time01; } protected set { time01 = value; } }
		[SerializeField, NotEditable]
		float time01;

		//Time01の逆（0と1が逆）を返す
		public float Time01Inverse { get { return 1.0f - Time01; } }

		//カーブ値を取得
		public float GetCurve01(EaseType easeType= EaseType.Linear)
		{
			return Easing.GetCurve01(Time01, easeType);
		}

		//カーブの逆値を取得
		public float GetCurve01Inverse(EaseType easeType = EaseType.Linear)
		{
			return Easing.GetCurve01(Time01Inverse, easeType);
		}

		//最初と最後の値を指定して、その間の補間するカーブ値を取得
		public float GetCurve(float start, float end)
		{
			return GetCurve(start, end, EaseType.Linear);
		}
		public float GetCurve(float start, float end, EaseType easeType)
		{
			return Easing.GetCurve(start, end, Time01, easeType);
		}
		public Vector2 GetCurve(Vector2 start, Vector2 end)
		{
			return GetCurve(start, end, EaseType.Linear);
		}
		public Vector2 GetCurve(Vector2 start, Vector2 end, EaseType easeType)
		{
			return Easing.GetCurve(start, end, Time01, easeType);
		}
		public Vector3 GetCurve(Vector3 start, Vector3 end)
		{
			return GetCurve(start, end, EaseType.Linear);
		}
		public Vector3 GetCurve(Vector3 start, Vector3 end, EaseType easeType)
		{
			return Easing.GetCurve(start, end, Time01, easeType);
		}
		public Vector4 GetCurve(Vector4 start, Vector4 end)
		{
			return GetCurve(start, end, EaseType.Linear);
		}
		public Vector4 GetCurve(Vector4 start, Vector4 end, EaseType easeType)
		{
			return Easing.GetCurve(start, end, Time01, easeType);
		}

		//最初と最後の値を指定して、その間の逆補間するカーブ値を取得
		public float GetCurveInverse(float start, float end)
		{
			return GetCurveInverse(start, end, EaseType.Linear);
		}
		public float GetCurveInverse(float start, float end, EaseType easeType)
		{
			return Easing.GetCurve(start, end, Time01Inverse, easeType);
		}
		public Vector2 GetCurveInverse(Vector2 start, Vector2 end)
		{
			return GetCurveInverse(start, end, EaseType.Linear);
		}
		public Vector2 GetCurveInverse(Vector2 start, Vector2 end, EaseType easeType)
		{
			return Easing.GetCurve(start, end, Time01Inverse, easeType);
		}
		public Vector3 GetCurveInverse(Vector3 start, Vector3 end)
		{
			return GetCurveInverse(start, end, EaseType.Linear);
		}
		public Vector3 GetCurveInverse(Vector3 start, Vector3 end, EaseType easeType)
		{
			return Easing.GetCurve(start, end, Time01Inverse, easeType);
		}
		public Vector4 GetCurveInverse(Vector4 start, Vector4 end)
		{
			return GetCurveInverse(start, end, EaseType.Linear);
		}
		public Vector4 GetCurveInverse(Vector4 start, Vector4 end, EaseType easeType)
		{
			return Easing.GetCurve(start, end, Time01Inverse, easeType);
		}

		public TimerEvent onStart = new TimerEvent();
		public TimerEvent onUpdate = new TimerEvent();
		public TimerEvent onComplete = new TimerEvent();
		public bool AutoDestroy
		{
			get { return autoDestroy; }
			set { autoDestroy = value; }
		}
		[SerializeField]
		bool autoDestroy = false;

		[SerializeField]
		bool autoStart = false;

		Action<Timer> callbackUpdate;
		Action<Timer> callbackComplete;


		//シーン内にすでにAddComponentされていた場合こちらに
		void Start()
		{
			if (autoStart)
			{
				StartCoroutine(CoTimer(duration, delay));
			}
		}

		//前回のタイマーを明示的にキャンセルCompleteのコールバックを呼ぶ
		public void Cancel()
		{
			OnCompleteCallback();
			Stop();
		}

		//タイマー起動（プログラムでAddComponentした直後に呼ぶ）
		public void StartTimer(float duration, Action<Timer> onUpdate = null, Action<Timer> onComplete = null, float delay = 0)
		{
			callbackUpdate = onUpdate;
			callbackComplete = onComplete;
			StartTimer(duration,delay);
		}

		//タイマー起動
		public void StartTimer(float duration, float delay = 0)
		{
			autoStart = false;
			Stop();
			StartCoroutine(CoTimer(duration, delay));
		}

		//タイマーのコルーチン
		IEnumerator CoTimer(float duration, float delay)
		{
			this.duration = duration;
			this.delay = delay;
			WaitTimer timer = new WaitTimer(duration, delay, OnStart, OnUpdate, OnComplete);
			yield return timer;
		}

		void Stop()
		{
/*			if (currentTimer != null)
			{
				StopCoroutine(currentTimer);
				currentTimer = null;
			}
			if (currentCoroutine != null)
			{
				StopCoroutine(currentCoroutine);
				currentCoroutine = null;
			}*/
			StopAllCoroutines();
		}

		//遅延を考慮した開始時に呼び出される
		void OnStart(WaitTimer timer)
		{
			onStart.Invoke(this);
		}

		//更新時呼び出される
		void OnUpdate(WaitTimer timer)
		{
			this.Time = timer.Time;
			this.Time01 = timer.Time01;
			onUpdate.Invoke(this);
			if (callbackUpdate != null) callbackUpdate(this);
		}
		//終了時に呼び出される
		void OnComplete(WaitTimer timer)
		{
			OnCompleteCallback();

			if (AutoDestroy)
			{
				UnityEngine.Object.Destroy(this);
			}
		}

		//終了時に呼び出される
		void OnCompleteCallback()
		{
			onComplete.Invoke(this);
			if (callbackComplete != null) callbackComplete(this);
			callbackComplete = null;
		}
	}
}