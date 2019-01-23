// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Utage
{
	[System.Serializable]
	public class BacklogEvent : UnityEvent<AdvBacklogManager> { }


	/// <summary>
	/// バックログ管理
	/// </summary>
	[AddComponentMenu("Utage/ADV/Internal/BacklogManager")]
	public class AdvBacklogManager : MonoBehaviour, IAdvSaveData
	{
		/// <summary>
		/// ログの最大数
		/// </summary>
		public int MaxLog { get { return maxLog; } }
		[SerializeField]
		int maxLog = 10;

		//ログを無視するか
		public bool IgnoreLog { get { return ignoreLog; } set { ignoreLog = value; } }
		[SerializeField]
		bool ignoreLog = false;

		//ログのあるページ追加時に呼ばれる
		public BacklogEvent OnAddPage { get { return onAddPage; } }
		[SerializeField]
		BacklogEvent onAddPage = new BacklogEvent();

		//ログのあるページデータで呼ばれる
		public BacklogEvent OnAddData { get { return onAddData; } }
		[SerializeField]
		BacklogEvent onAddData = new BacklogEvent();

		/// <summary>
		/// バックログデータのリスト
		/// </summary>
		/// <returns></returns>
		public List<AdvBacklog> Backlogs{ get { return backlogs; }}
		List<AdvBacklog> backlogs = new List<AdvBacklog>();

		/// <summary>
		/// 最後のバックログ
		/// </summary>
		/// <returns></returns>
		public AdvBacklog LastLog
		{
			get
			{
				if (Backlogs.Count <= 0)
				{
					return null;
				}
				return Backlogs[Backlogs.Count - 1];
			}
		}

		/// <summary>
		/// クリア処理
		/// </summary>
		public void Clear()
		{
			backlogs.Clear();
		}


		//バックログとしてページデータを追加
		internal void AddPage()
		{
			onAddPage.Invoke(this);
			if (IgnoreLog) return;

			AddLog(new AdvBacklog());
		}

		void AddLog(AdvBacklog log)
		{
			if (IgnoreLog) return;
			backlogs.Add(log);
			if (backlogs.Count > MaxLog)
			{
				backlogs.RemoveAt(0);
			}
		}
		

		//現在のページを更新
		internal void AddCurrentPageLog(AdvCommandText dataInPage, AdvCharacterInfo characterInfo)
		{
			onAddData.Invoke(this);
			if (IgnoreLog) return;

			AdvBacklog log = LastLog;
			if (log != null)
			{
				log.AddData(dataInPage, characterInfo);
			}
		}


		//データのキー
		public string SaveKey { get { return "BacklogManager"; } }

		//クリアする(初期状態に戻す)
		public void OnClear()
		{
			Clear();
		}

		const int Version = 0;
		//書き込み
		public void OnWrite(System.IO.BinaryWriter writer)
		{
			writer.Write(Version);
			writer.Write(Backlogs.Count);
			foreach( var item in Backlogs)
			{
				item.Write(writer);
			}
		}

		//読み込み
		public void OnRead(System.IO.BinaryReader reader)
		{
			//バージョンチェック
			int version = reader.ReadInt32();
			if (version == Version)
			{
				int count = reader.ReadInt32();
				for(int i = 0; i < count; ++i )
				{
					AdvBacklog item = new AdvBacklog();
					item.Read(reader);
					if (!item.IsEmpty)
					{
						AddLog(item);
					}
				}
			}
			else
			{
				Debug.LogError(LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.UnknownVersion, version));
			}
		}

	}
}
