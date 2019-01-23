// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// 入力処理
	/// </summary>
	public static class InputUtil
	{
		[System.Obsolete("Use IsMouseRightButtonDown instead")]
		public static bool IsMousceRightButtonDown()
		{
			return IsMouseRightButtonDown();
		}

		public static bool EnableWebGLInput()
		{
#if UTAGE_DISABLE_WEBGL_INPUT
			return falase;
#else
			return (Application.platform == RuntimePlatform.WebGLPlayer);
#endif
		}

		public static bool IsMouseRightButtonDown()
		{
			if( UtageToolKit.IsPlatformStandAloneOrEditor() || EnableWebGLInput())
			{ 
				return Input.GetMouseButtonDown(1);
			}
			else
			{
				return false;
			}
		}

		public static bool IsInputControl()
		{
			if( UtageToolKit.IsPlatformStandAloneOrEditor() || EnableWebGLInput())
			{ 
				return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
			}
			else
			{
				return false;
			}
		}

		static float wheelSensitive = 0.1f;
		public static bool IsInputScrollWheelUp()
		{
			float axis = Input.GetAxis("Mouse ScrollWheel");
			if (axis >= wheelSensitive )
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool IsInputScrollWheelDown()
		{
			float axis = Input.GetAxis("Mouse ScrollWheel");
			if (axis <= -wheelSensitive )
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool IsInputKeyboadReturnDown()
		{
			if (UtageToolKit.IsPlatformStandAloneOrEditor() || EnableWebGLInput())
			{
				return Input.GetKeyDown(KeyCode.Return);
			}
			else
			{
				return false;
			}
		}
	}

}