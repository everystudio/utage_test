﻿// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.Collections.Generic;
using System.IO;
using UnityEngine.Events;
using UnityEngine;
using System;
using UtageExtensions;

namespace Utage
{

	/// <summary>
	/// サウンドのグループ管理
	/// </summary>
	[AddComponentMenu("Utage/Lib/Sound/Group")]
	public class SoundGroup : MonoBehaviour
	{
		internal SoundManager SoundManager { get { return SoundManagerSystem.SoundManager;  } }
		internal SoundManagerSystem SoundManagerSystem { get; private set; }

		internal Dictionary<string,SoundAudioPlayer> PlayerList { get { return playerList; } }
		Dictionary<string, SoundAudioPlayer> playerList = new Dictionary<string, SoundAudioPlayer>();

		public string GroupName { get { return gameObject.name; } }

		//グループ内で複数のオーディオを鳴らすか
		public bool MultiPlay
		{
			get { return multiPlay; }
			set { multiPlay = value; }
		}
		[SerializeField]
		bool multiPlay;

		//プレイヤーが終了したら自動削除するか
		public bool AutoDestoryPlayer
		{
			get { return autoDestoryPlayer; }
			set { autoDestoryPlayer = value; }
		}
		[SerializeField]
		bool autoDestoryPlayer;

		//マスターボリューム
		public float MasterVolume { get { return masterVolume; } set { masterVolume = value; } }
		[Range(0, 1), SerializeField]
		float masterVolume = 1;

		//グループボリューム
		public float GroupVolume { get { return groupVolume; } set { groupVolume = value; } }
		[Range(0, 1), SerializeField]
		float groupVolume = 1;

		//グループボリュームのフェード時間
		public float GroupVolumeFadeTime { get { return groupVolumeFadeTime; } set { groupVolumeFadeTime = value; } }
		[SerializeField]
		float groupVolumeFadeTime = 1;

		//現在のグループボリューム
		public float CurrentGroupVolume { get; set; }
		float groupVolumeVelocity = 1;

		//ダッキングの影響を与えるグループ
		public List<SoundGroup> DuckGroups { get { return duckGroups; } }
		[SerializeField]
		List<SoundGroup> duckGroups = new List<SoundGroup>();

		float DuckVolume { get; set; }
		float duckVelocity = 1;

		internal void Init(SoundManagerSystem soundManagerSystem)
		{
			SoundManagerSystem = soundManagerSystem;

			this.CurrentGroupVolume = this.GroupVolume;
			this.groupVolumeVelocity = 1;
			DuckVolume = 1;
			duckVelocity = 1;
		}

		internal float GetVolume(string tag)
		{
			float masterVolume = this.CurrentGroupVolume * this.MasterVolume * SoundManager.MasterVolume;
			foreach(var taggedVolume in SoundManager.TaggedMasterVolumes)
			{
				if (taggedVolume.Tag == tag)
				{
					masterVolume *= taggedVolume.Volume;
				}
			}
			return masterVolume * DuckVolume;
		}

		void Update()
		{
			UpdateDucking();
			CurrentGroupVolume = UpdateFade(CurrentGroupVolume, GroupVolume, GroupVolumeFadeTime, ref groupVolumeVelocity);
		}

		void UpdateDucking()
		{
			//以下、ダッキング処理
			if (Mathf.Approximately(1.0f, SoundManager.DuckVolume))
			{
				//ダッキングのボリュームが1なので常に影響受けない
				DuckVolume = 1;
				return;
			}

			//ダッキングの影響をうけるグループがない
			if (DuckGroups.Count <= 0)
			{
				DuckVolume = 1;
				return;
			}
			bool isPlaying = DuckGroups.Exists(x => x.IsPlaying());
			float dukkingTo = (isPlaying) ? SoundManager.DuckVolume : 1;

			DuckVolume = UpdateFade(DuckVolume, dukkingTo, SoundManager.DuckFadeTime, ref duckVelocity);
		}

		float UpdateFade(float from, float to, float fadeTime, ref float velocity)
		{
			if (fadeTime <= 0)
			{
				velocity = 0;
				return to;
			}

			if (Mathf.Abs(to - from) < 0.001f)
			{
				//目標値に近づいた
				velocity = 0;
				return to;
			}
			else
			{

				return Mathf.SmoothDamp(from, to, ref velocity, fadeTime);
			}
		}

		internal void Remove(string label)
		{
			PlayerList.Remove(label);
		}

		public bool IsLoading
		{
			get
			{
				foreach (var keyValue in PlayerList)
				{
					if (keyValue.Value.IsLoading) return true;
				}
				return false;
			}
		}

		SoundAudioPlayer GetPlayer(string label)
		{
			SoundAudioPlayer player;
			if( PlayerList.TryGetValue(label, out player))
			{
				return player;
			}
			return null;
		}

		SoundAudioPlayer GetPlayerOrCreateIfMissing(string label)
		{
			SoundAudioPlayer player = GetPlayer(label);
			if (player == null)
			{
				player = this.transform.AddChildGameObjectComponent<SoundAudioPlayer>(label);
				player.Init(label,this);
				PlayerList.Add(label, player);
			}
			return player;
		}

		SoundAudioPlayer GetOnlyOnePlayer(string label, float fadeOutTime)
		{
			SoundAudioPlayer player = GetPlayerOrCreateIfMissing(label);
			if (PlayerList.Count > 1)
			{
				foreach (var keyValue in PlayerList)
				{
					if (keyValue.Value != player)
					{
						keyValue.Value.Stop(fadeOutTime);
					}
				}
			}
			return player;
		}

		internal bool IsPlaying()
		{
			foreach (var keyValue in PlayerList)
			{
				if (keyValue.Value.IsPlaying()) return true;
			}
			return false;
		}

		internal bool IsPlaying(string label)
		{
			SoundAudioPlayer player = GetPlayer(label);
			if (player == null) return false;
			return player.IsPlaying();
		}

		internal bool IsPlayingLoop(string label)
		{
			SoundAudioPlayer player = GetPlayer(label);
			if (player == null) return false;
			return player.IsPlayingLoop();
		}

		internal void Play( string label, SoundData data, float fadeInTime, float fadeOutTime)
		{
			SoundAudioPlayer player = ( MultiPlay ) ? GetPlayerOrCreateIfMissing(label) : GetOnlyOnePlayer(label, fadeOutTime);
			player.Play(data, fadeInTime, fadeOutTime);
		}

		internal void Stop(string label, float fadeTime )
		{
			SoundAudioPlayer player = GetPlayer(label);
			if (player == null) return;
			player.Stop(fadeTime);
		}
		internal void StopAll(float fadeTime)
		{
			foreach (var keyValue in PlayerList)
			{
				keyValue.Value.Stop(fadeTime);
			}
		}

		internal void StopAllLoop(float fadeTime)
		{
			foreach (var keyValue in PlayerList)
			{
				if (!keyValue.Value.IsPlayingLoop()) continue;
				keyValue.Value.Stop(fadeTime);
			}
		}

		internal void StopAllIgnoreLoop(float fadeTime)
		{
			foreach (var keyValue in PlayerList)
			{
				if (keyValue.Value.IsPlayingLoop()) continue;
				keyValue.Value.Stop(fadeTime);
			}
		}

		
		internal AudioSource GetAudioSource(string label)
		{
			SoundAudioPlayer player = GetPlayer(label);
			if (player == null) return null;
			return player.Audio.AudioSource;
		}

		internal float GetSamplesVolume(string label)
		{
			SoundAudioPlayer player = GetPlayer(label);
			if (player == null) return 0;
			return player.GetSamplesVolume();
		}

		const int Version = 1;
		const int Version0 = 0;
		//セーブデータ用のバイナリ書き込み
		internal void Write(BinaryWriter writer)
		{
			writer.Write(Version);
			writer.Write(GroupVolume);
			writer.Write(PlayerList.Count);
			foreach (var keyValue in PlayerList)
			{
				writer.Write(keyValue.Key);
				writer.WriteBuffer(keyValue.Value.Write);
			}
		}

		//セーブデータ用のバイナリ読み込み
		internal void Read(BinaryReader reader)
		{
			int version = reader.ReadInt32();
			if (version <= Version)
			{
				if (version > Version0)
				{
					GroupVolume = reader.ReadSingle();
				}
				int playerCount = reader.ReadInt32();
				for (int i = 0; i < playerCount; ++i)
				{
					string label = reader.ReadString();
					SoundAudioPlayer player = GetPlayerOrCreateIfMissing(label);
					reader.ReadBuffer(player.Read);
				}
			}
			else
			{
				Debug.LogError(LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.UnknownVersion, version));
			}
		}
	}
}