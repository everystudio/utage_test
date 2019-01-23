// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Utage
{
	//リップシンクのタイプ
	[System.Flags]
	public enum LipSynchType
	{
		Text,               //テキストのみ
		Voice,              //ボイスが鳴っている場合は、そのボイスに合わせてリップシンク
		TextAndVoice,       //テキストとボイス
	};

	//現在のリップシンクのモード
	public enum LipSynchMode
	{
		Text,               //テキスト
		Voice,              //ボイス
	};

	[System.Serializable]
	public class LipSynchEvent : UnityEvent<LipSynchBase> { }
	/// <summary>
	/// まばたき処理の基底クラス
	/// </summary>
	public abstract class LipSynchBase : MonoBehaviour
	{
		public LipSynchType Type { get { return type; } set { type = value; } }
		[SerializeField]
		LipSynchType type = LipSynchType.TextAndVoice;

		//テキストのリップシンクが現在有効になっているか
		//外部から変更する
		public bool EnableTextLipSync { get; set; }

		public LipSynchMode LipSynchMode { get; set; }

		//テキストのリップシンクチェック
		public LipSynchEvent OnCheckTextLipSync = new LipSynchEvent();

		//ターゲットのキャラクターラベルを取得
		public string CharacterLabel
		{
			get
			{
				if (string.IsNullOrEmpty(characterLabel))
				{
					return this.gameObject.name;
				}
				else
				{
					return characterLabel;
				}
			}
			set
			{
				characterLabel = value;
			}
		}
		string characterLabel;

		//有効か
		public bool IsEnable { get; set; }

		//再生中か
		public bool IsPlaying { get; set; }

		//再生
		public void Play()
		{
			IsEnable = true;
		}

		//強制終了
		public void Cancel()
		{
			IsEnable = false;
			IsPlaying = false;
			OnStopLipSync();
		}

		//更新
		protected virtual void Update()
		{
			bool isVoice = CheckVoiceLipSync();
			bool isText = CheckTextLipSync();
			this.LipSynchMode = isVoice ? LipSynchMode.Voice : LipSynchMode.Text;
			bool enableLipSync = IsEnable && (isVoice || isText);
			if (enableLipSync)
			{
				if (!IsPlaying)
				{
					IsPlaying = true;
					OnStartLipSync();
				}
				OnUpdateLipSync();
			}
			else
			{
				if (IsPlaying)
				{
					IsPlaying = false;
					OnStopLipSync();
				}
			}
		}

		//ボイスのリップシンクのチェック
		protected bool CheckVoiceLipSync()
		{
			switch (Type)
			{
				case LipSynchType.Voice:
				case LipSynchType.TextAndVoice:
					SoundManager soundManager = SoundManager.GetInstance();
					if (soundManager != null)
					{
						if (soundManager.IsPlayingVoice(CharacterLabel))
						{
							return true;
						}
					}
					break;
				default:
					break;
			}
			return false;
		}

		//テキストのリップシンクのチェック
		protected bool CheckTextLipSync()
		{
			switch (Type)
			{
				case LipSynchType.Text:
				case LipSynchType.TextAndVoice:
					{
						OnCheckTextLipSync.Invoke(this);
						return EnableTextLipSync;
					}
				default:
					break;
			}
			return false;
		}

		protected abstract void OnStartLipSync();

		protected abstract void OnUpdateLipSync();

		protected abstract void OnStopLipSync();
	}
}
