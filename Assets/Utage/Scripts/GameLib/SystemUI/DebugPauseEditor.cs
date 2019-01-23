using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Utage
{

	/// <summary>
	/// デバッグ用に、マウスダウン、アウップでUnityエディタをポーズさせる
	/// </summary>
	[AddComponentMenu("Utage/Lib/System UI/DebugPauseEditor")]
	public class DebugPauseEditor : MonoBehaviour
	{
		public bool isPauseOnMouseDown = false;
		public bool isPauseOnMouseUp = false;
		 [Range(0.00001f, 10)] 
		public float timeScale = 1.0f;

#if UNITY_EDITOR
		void Start()
		{
			timeScale = Time.timeScale;
		}

		void Update()
		{
			if ( IsMouseDown() || IsMouseUp() )
			{
				PauseEditor();
			}
		}

		bool IsMouseDown()
		{
			if (!isPauseOnMouseDown || !Input.GetMouseButtonDown(0) )
				return false;
//			return IsInputAlt() && IsInputShift();
			return true;
		}

		bool IsMouseUp()
		{
			if (!isPauseOnMouseUp || !Input.GetMouseButtonUp (0) )
				return false;
//			return IsInputAlt();
			return true;
		}

		bool IsInputAlt()
		{
			return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.LeftAlt);
		}

		bool IsInputShift()
		{
			return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		}
		

		public void PauseEditor()
		{
			UnityEditor.EditorApplication.isPaused = true;
		}

		void OnValidate()
		{
			if ( !Mathf.Approximately( Time.timeScale,timeScale) ) Time.timeScale = timeScale;
		}
#endif
	}
}