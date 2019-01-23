// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.UI;
using Utage;

/// <summary>
/// メッセージウィドウ処理のインターフェース
/// </summary>
public interface IAdvMessageWindow
{
	GameObject gameObject { get; }
	//ゲーム起動時の初期化
	void OnInit(AdvMessageWindowManager windowManager);
	//初期状態にもどす
	void OnReset();
	//現在のウィンドウかどうかが変わった
	void OnChangeCurrent(bool isCurrent);
	//アクティブ状態が変わった
	void OnChangeActive(bool isActive);
	//テキストが変わった
	void OnTextChanged(AdvMessageWindow window);
}

