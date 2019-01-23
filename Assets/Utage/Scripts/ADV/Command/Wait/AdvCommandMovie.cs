// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// コマンド：ムービー再生
	/// </summary>
	internal class AdvCommandMovie : AdvCommand
	{
		public AdvCommandMovie(StringGridRow row, AdvSettingDataManager dataManager)
			: base(row)
		{
			this.label = ParseCell<string>(AdvColumnName.Arg1);
			this.loop = ParseCellOptional<bool>(AdvColumnName.Arg2,false);
			this.cancel = ParseCellOptional<bool>(AdvColumnName.Arg3, true);
			this.waitTime = ParseCellOptional<float>(AdvColumnName.Arg6, -1);
		}

		public override void DoCommand(AdvEngine engine)
		{
			if (WrapperMoviePlayer.GetInstance().OverrideRootDirectory)
			{
				WrapperMoviePlayer.Play(FilePathUtil.Combine(WrapperMoviePlayer.GetInstance().RootDirectory, label), loop, cancel);
			}
			else
			{
				string root = FilePathUtil.Combine(engine.DataManager.SettingDataManager.BootSetting.ResourceDir, "Movie");
				WrapperMoviePlayer.Play(FilePathUtil.Combine(root, label), loop, cancel);
			}
			time = 0;
		}

		public override bool Wait(AdvEngine engine)
		{
			if (engine.UiManager.IsInputTrig)
			{
				WrapperMoviePlayer.Cancel();
			}
			bool isWait = WrapperMoviePlayer.IsPlaying();
			if (waitTime >= 0)
			{
				if (time >= waitTime)
				{
					isWait = false;
				}
				time += Time.deltaTime;
			}
			return isWait;
		}
		string label;
		bool loop;
		bool cancel;
		float waitTime;
		float time;
	}
}