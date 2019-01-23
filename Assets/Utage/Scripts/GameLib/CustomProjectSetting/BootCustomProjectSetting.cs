// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using Utage;


/// <summary>
/// ゲーム起動処理のサンプル
/// </summary>
[ExecuteInEditMode]
[AddComponentMenu("Utage/Lib/Other/BootCustomProjectSetting")]
public class BootCustomProjectSetting : MonoBehaviour
{
	//独自カスタムのプロジェクト設定
	public CustomProjectSetting CustomProjectSetting
	{
		get { return customProjectSetting; }
		set { customProjectSetting = value; }
	}
	[SerializeField]
	CustomProjectSetting customProjectSetting;
}
