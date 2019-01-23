// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using Utage;
using System.Collections;


/// <summary>
/// 任意のシナリオにジャンプするサンプル
/// </summary>
[AddComponentMenu("Utage/ADV/Examples/SampleJumpButton")]
public class SampleJumpButton : MonoBehaviour
{
	/// <summary>ADVエンジン</summary>
	public AdvEngine Engine { get { return this.engine ?? (this.engine = FindObjectOfType<AdvEngine>()); } }
	[SerializeField]
	protected AdvEngine engine;

	public string scenarioLabel;


	public void OnClickJump()
	{
		Engine.JumpScenario(scenarioLabel);
	}
}
