// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UtageExtensions;
using System;

namespace Utage
{

	/// <summary>
	/// 便利クラス 
	/// </summary>
	public class UtageToolKit
	{
		public static bool IsHankaku(char c)
		{
			if ((c <= '\u007e') || // 英数字
				(c == '\u00a5') || // \記号
				(c == '\u203e') || // ~記号
				(c >= '\uff61' && c <= '\uff9f') // 半角カナ
			)
				return true;
			else
				return false;
		}

		public static bool IsPlatformStandAloneOrEditor()
		{
			return Application.isEditor || IsPlatformStandAlone();
		}

		public static bool IsPlatformStandAlone()
		{
			switch (Application.platform)
			{
				case RuntimePlatform.WindowsPlayer:
				case RuntimePlatform.OSXPlayer:
				case RuntimePlatform.LinuxPlayer:
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// キャプチャ用のテクスチャを作る(yield return new WaitForEndOfFrame()の後に呼ぶこと)
		/// </summary>
		/// <returns>キャプチャ画像</returns>
		public static Texture2D CaptureScreen()
		{
			return CaptureScreen(new Rect(0, 0, Screen.width, Screen.height));
		}

		/// <summary>
		/// キャプチャ用のテクスチャを作る(yield return new WaitForEndOfFrame()の後に呼ぶこと)
		/// </summary>
		/// <returns>キャプチャ画像</returns>
		public static Texture2D CaptureScreen(Rect rect)
		{
			return CaptureScreen(TextureFormat.RGB24, rect);
		}

		/// <summary>
		/// キャプチャ用のテクスチャを作る(yield return new WaitForEndOfFrame()の後に呼ぶこと)
		/// </summary>
		/// <param name="format">テクスチャフォーマット</param>
		/// <returns>キャプチャ画像</returns>
		public static Texture2D CaptureScreen(TextureFormat format, Rect rect)
		{
			Texture2D tex = new Texture2D((int)rect.width, (int)rect.height, format, false);
			try
			{
				tex.ReadPixels(rect, 0, 0);
				tex.Apply();
			}
			catch
			{
			}
			return tex;
		}

		/// <summary>
		/// 日付を日本式表記のテキストで取得
		/// </summary>
		/// <param name="date">日付</param>
		/// <returns>日付の日本式表記テキスト</returns>
		static public string DateToStringJp(System.DateTime date)
		{
			return date.ToString(cultureInfJp);
		}
		static System.Globalization.CultureInfo cultureInfJp = new System.Globalization.CultureInfo("ja-JP");


		/// <summary>
		/// サイズ変更したテクスチャを作成する
		/// </summary>
		/// <param name="tex">リサイズするテクスチャ</param>
		/// <param name="captureW">リサイズ後のテクスチャの横幅(pix)</param>
		/// <param name="captureH">リサイズ後のテクスチャの縦幅(pix)</param>
		/// <returns>キャプチャ画像のテクスチャバイナリ</returns>
		public static Texture2D CreateResizeTexture(Texture2D tex, int width, int height)
		{
			if (tex == null) return null;
			return CreateResizeTexture(tex, width, height, tex.format, tex.mipmapCount > 1);
		}

		/// <summary>
		/// サイズ変更したテクスチャを作成する
		/// </summary>
		/// <param name="tex">リサイズするテクスチャ</param>
		/// <param name="width">リサイズ後のテクスチャの横幅(pix)</param>
		/// <param name="height">リサイズ後のテクスチャの縦幅(pix)</param>
		/// <param name="format">リサイズ後のテクスチャフォーマット</param>
		/// <param name="isMipmap">ミップマップを有効にするか</param>
		/// <returns>リサイズして作成したテクスチャ</returns>
		public static Texture2D CreateResizeTexture(Texture2D tex, int width, int height, TextureFormat format, bool isMipmap)
		{
			if (tex == null) return null;

			TextureWrapMode wrap = tex.wrapMode;
			tex.wrapMode = TextureWrapMode.Clamp;
			Color[] colors = new Color[width * height];
			int index = 0;
			for (int y = 0; y < height; y++)
			{
				float v = 1.0f * y / (height - 1);
				for (int x = 0; x < width; x++)
				{
					float u = 1.0f * x / (width - 1);
					colors[index] = tex.GetPixelBilinear(u, v);
					++index;
				}
			}
			tex.wrapMode = wrap;

			Texture2D resizedTex = new Texture2D(width, height, format, isMipmap);
			resizedTex.SetPixels(colors);
			resizedTex.Apply();
			return resizedTex;
		}
		public static Texture2D CreateResizeTexture(Texture2D tex, int width, int height, TextureFormat format)
		{
			return CreateResizeTexture(tex, width, height, format, false);
		}

		/// <summary>
		/// テクスチャから基本的なスプライト作成
		/// </summary>
		/// <param name="tex">テクスチャ</param>
		/// <param name="pixelsToUnits">スプライトを作成する際の、座標1.0単位辺りのピクセル数</param>
		/// <returns></returns>
		public static Sprite CreateSprite(Texture2D tex, float pixelsToUnits)
		{
			return CreateSprite(tex, pixelsToUnits, new Vector2(0.5f, 0.5f));
		}
		/// <summary>
		/// テクスチャから基本的なスプライト作成
		/// </summary>
		/// <param name="tex">テクスチャ</param>
		/// <param name="pixelsToUnits">スプライトを作成する際の、座標1.0単位辺りのピクセル数</param>
		/// <returns></returns>
		public static Sprite CreateSprite(Texture2D tex, float pixelsToUnits, Vector2 pivot)
		{
			if (tex == null)
			{
				Debug.LogError("texture is null");
				tex = Texture2D.whiteTexture;
			}
			if (tex.mipmapCount > 1) Debug.LogWarning(LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.SpriteMimMap, tex.name));
			Rect rect = new Rect(0, 0, tex.width, tex.height);
			return Sprite.Create(tex, rect, pivot, pixelsToUnits);
		}

		/// <summary>
		/// Enum型を文字列から解析
		/// </summary>
		/// <typeparam name="T">enum型</typeparam>
		/// <param name="str">enum値の文字列</param>
		/// <param name="val">結果のenum値</param>
		/// <returns>成否</returns>
		[System.Obsolete]
		public static bool TryParaseEnum<T>(string str, out T val)
		{
			try
			{
				val = (T)System.Enum.Parse(typeof(T), str);
				return true;
			}
			catch (System.Exception)
			{
				val = default(T);
				return false;
			}
		}

		/// <summary>
		/// ただし、targetやfuncがnullだった場合何もしない
		/// </summary>
		/// <param name="functionName">送信するメッセージ</param>
		/// <param name="isForceActive">送り先のGameObjectを強制的にactiveにしてからSendMessageするか</param>
		[System.Obsolete]
		public static void SafeSendMessage(GameObject target, string functionName, System.Object obj = null, bool isForceActive = false)
		{
			if (target == null) return;
			target.SafeSendMessage(functionName, obj, isForceActive);
		}
		/// <summary>
		/// ただし、targetやfuncがnullだった場合何もしない
		/// </summary>
		/// <param name="functionName">送信するメッセージ</param>
		/// <param name="isForceActive">送り先のGameObjectを強制的にactiveにしてからSendMessageするか</param>
		[System.Obsolete]
		public static void SafeSendMessage(System.Object obj, GameObject target, string functionName, bool isForceActive = false)
		{
			SafeSendMessage(target, functionName, obj, isForceActive);
		}


		/// <summary>
		/// 子要素の全削除
		/// </summary>
		/// <param name="parent">親要素</param>
		[System.Obsolete]
		public static void DestroyChildren(Transform parent)
		{
			parent.DestroyChildren();
		}

		/// <summary>
		/// 子要素の全削除(エディタ上ではDestroyImmediateを使う)
		/// </summary>
		/// <param name="parent">親要素</param>
		[System.Obsolete]
		public static void DestroyChildrenInEditorOrPlayer(Transform parent)
		{
			parent.DestroyChildrenInEditorOrPlayer();
		}

		/// <summary>
		/// 子の追加
		/// </summary>
		/// <param name="parent">親</param>
		/// <param name="go">子</param>
		/// <returns>追加済みの子</returns>
		[System.Obsolete]
		public static GameObject AddChild(Transform parent, GameObject go)
		{
			return parent.AddChild(go, Vector3.zero, Vector3.one);
		}
		/// <summary>
		/// 子の追加
		/// </summary>
		/// <param name="parent">親</param>
		/// <param name="go">子</param>
		/// <param name="localPosition">子に設定するローカル座標</param>
		/// <returns>追加済みの子</returns>
		[System.Obsolete]
		public static GameObject AddChild(Transform parent, GameObject go, Vector3 localPosition)
		{
			return parent.AddChild(go, localPosition, Vector3.one);
		}
		/// <summary>
		/// 子の追加
		/// </summary>
		/// <param name="parent">親</param>
		/// <param name="go">子</param>
		/// <param name="localPosition">子に設定するローカル座標</param>
		/// <param name="localScale">子に設定するローカルスケール</param>
		/// <returns>追加済みの子</returns>
		[System.Obsolete]
		public static GameObject AddChild(Transform parent, GameObject go, Vector3 localPosition, Vector3 localScale)
		{
			return parent.AddChild(go,localPosition, localScale);
		}

		/// <summary>
		/// プレハブからGameObjectを作成して子として追加する
		/// </summary>
		/// <param name="parent">親</param>
		/// <param name="prefab">子を作成するためのプレハブ</param>
		/// <returns>追加済みの子</returns>
		[System.Obsolete]
		public static GameObject AddChildPrefab(Transform parent, GameObject prefab )
		{
			return parent.AddChildPrefab(prefab, Vector3.zero, Vector3.one);
		}
		/// <summary>
		/// プレハブからGameObjectを作成して子として追加する
		/// </summary>
		/// <param name="parent">親</param>
		/// <param name="prefab">子を作成するためのプレハブ</param>
		/// <param name="localPosition">子に設定するローカル座標</param>
		/// <returns>追加済みの子</returns>
		[System.Obsolete]
		public static GameObject AddChildPrefab(Transform parent, GameObject prefab, Vector3 localPosition)
		{
			return parent.AddChildPrefab(prefab, localPosition, Vector3.one);
		}

		/// <summary>
		/// プレハブからGameObjectを作成して子として追加する
		/// </summary>
		/// <param name="parent">親</param>
		/// <param name="prefab">子を作成するためのプレハブ</param>
		/// <param name="localPosition">子に設定するローカル座標</param>
		/// <param name="localScale">子に設定するローカルスケール</param>
		/// <returns>追加済みの子</returns>
		[System.Obsolete]
		public static GameObject AddChildPrefab(Transform parent, GameObject prefab, Vector3 localPosition, Vector3 localScale)
		{
			return parent.AddChildPrefab(prefab, localPosition, localScale);
		}

		/// <summary>
		/// UnityオブジェクトからGameObjectを作成して子として追加する
		/// </summary>
		/// <param name="parent">親</param>
		/// <param name="prefab">子を作成するためのUnityオブジェクト</param>
		/// <returns>追加済みの子</returns>
		[System.Obsolete]
		public static GameObject AddChildUnityObject(Transform parent, UnityEngine.Object obj )
		{
			return parent.AddChildUnityObject(obj);
		}

		/// <summary>
		/// 子を含む全てのレイヤーを変更する
		/// </summary>
		/// <param name="trans">レイヤーを変更する対象</param>
		/// <param name="layer">設定するレイヤー</param>
		[System.Obsolete]
		public static void ChangeLayerAllChildren(Transform trans, int layer)
		{
			trans.gameObject.ChangeLayerDeep(layer);
		}

		/// <summary>
		/// 指定のコンポーネントつきのGameObjectを作成して子として追加
		/// </summary>
		/// <typeparam name="T">コンポーネントの型</typeparam>
		/// <param name="parent">親</param>
		/// <param name="name">作成する子の名前</param>
		/// <returns>追加済みの子</returns>
		[System.Obsolete]
		public static T AddChildGameObjectComponent<T>(Transform parent, string name) where T : Component
		{
			return parent.AddChildGameObjectComponent<T>(name, Vector3.zero, Vector3.one);
		}

		/// <summary>
		/// 指定のコンポーネントつきのGameObjectを作成して子として追加
		/// </summary>
		/// <typeparam name="T">コンポーネントの型</typeparam>
		/// <param name="parent">親</param>
		/// <param name="name">作成する子の名前</param>
		/// <param name="localPosition">子に設定するローカル座標</param>
		/// <returns>追加済みの子</returns>
		[System.Obsolete]
		public static T AddChildGameObjectComponent<T>(Transform parent, string name, Vector3 localPosition) where T : Component
		{
			return parent.AddChildGameObjectComponent<T>(name, localPosition, Vector3.one);
		}
		/// <summary>
		/// 指定のコンポーネントつきのGameObjectを作成して子として追加
		/// </summary>
		/// <typeparam name="T">コンポーネントの型</typeparam>
		/// <param name="parent">親</param>
		/// <param name="name">作成する子の名前</param>
		/// <param name="localPosition">子に設定するローカル座標</param>
		/// <param name="localScale">子に設定するローカルスケール</param>
		/// <returns>追加済みの子</returns>
		[System.Obsolete]
		public static T AddChildGameObjectComponent<T>(Transform parent, string name, Vector3 localPosition, Vector3 localScale) where T : Component
		{
			return parent.AddChildGameObjectComponent<T>(name, localPosition, localScale);
		}

		/// <summary>
		/// GameObjectを作成し、子として追加
		/// </summary>
		/// <param name="parent">親</param>
		/// <param name="name">作成する子の名前</param>
		/// <returns>追加済みの子</returns>
		[System.Obsolete]
		public static GameObject AddChildGameObject(Transform parent, string name)
		{
			return parent.AddChildGameObject(name, Vector3.zero, Vector3.one);
		}

		/// <summary>
		/// GameObjectを作成し、子として追加
		/// </summary>
		/// <param name="parent">親</param>
		/// <param name="name">作成する子の名前</param>
		/// <param name="localPosition">子に設定するローカル座標</param>
		/// <returns>追加済みの子</returns>
		[System.Obsolete]
		public static GameObject AddChildGameObject(Transform parent, string name, Vector3 localPosition)
		{
			return parent.AddChildGameObject(name, localPosition, Vector3.one);
		}

		/// <summary>
		/// GameObjectを作成し、子として追加
		/// </summary>
		/// <param name="parent">親</param>
		/// <param name="name">作成する子の名前</param>
		/// <param name="localPosition">子に設定するローカル座標</param>
		/// <returns>追加済みの子</returns>
		/// <param name="localScale">子に設定するローカルスケール</param>
		[System.Obsolete]
		public static GameObject AddChildGameObject(Transform parent, string name, Vector3 localPosition, Vector3 localScale)
		{
			return parent.AddChildGameObject(name, localPosition, localScale);
		}


		/// <summary>
		/// 親オブジェクトやさらにその上位の親から、指定のコンポーネントを持つオブジェクトを検索
		/// </summary>
		/// <typeparam name="T">検索するコンポーネントの型</typeparam>
		/// <param name="transform">自分自身(親ではないので注意)</param>
		/// <returns>最初に見つかった指定のコンポーネントを持つオブジェクト。見つからなかったらnull</returns>
		[System.Obsolete]
		public static T FindParentComponent<T>(Transform transform) where T : Component
		{
			return transform.GetComponentInParent<T>();
		}

		/// <summary>
		/// 子オブジェクトやさらにその子から、指定の名前のGameObjecctのTrasnfromを検索
		/// </summary>
		/// <param name="trasnform">自分自身</param>
		/// <param name="name">検索する名前</param>
		/// <returns>最初にみつかった指定の名前をもつTrasform。見つからなかったらnull</returns>
		[System.Obsolete]
		public static Transform FindInChirdlen( Transform trasnform, string name)
		{
			return trasnform.FindDeep(name,true);
		}

		/// <summary>
		/// 指定のコンポーネントを階層以下から探し、なかったら子オブジェクトとしてそのコンポーネントを持つ子オブジェクトを作成する
		/// </summary>
		[System.Obsolete]
		public static T GetCompoentInChildrenCreateIfMissing<T>(Transform trasnform) where T : Component
		{
			return trasnform.GetComponentCreateIfMissing<T>();
		}

		/// <summary>
		/// コンポーネントを取得。なかったら作成
		/// </summary>
		[System.Obsolete]
		internal static T GetComponentCreateIfMissing<T>(GameObject go) where T : Component
		{
			return go.GetComponentCreateIfMissing<T>();
		}

		//指定インターフェースコンポーネントを全て取得
		[System.Obsolete]
		public static T[] GetInterfaceCompoents<T>(GameObject go) where T : class
		{
			//5.3?　からインターフェースも取得可能になった　
			return go.GetComponents<T>();
		}

		//指定インターフェースコンポーネントを取得
		[System.Obsolete]
		public static T GetInterfaceCompoent<T>(GameObject go) where T : class
		{
			//5.3?　からインターフェースも取得可能になった　
			return go.GetComponent<T>();
		}


		/// <summary>
		/// Transformのローカル情報をバイナリ書き込み
		/// </summary>
		/// <param name="transform">書き込むTransform</param>
		/// <param name="writer">バイナリライター</param>
		[System.Obsolete]
		public static void WriteLocalTransform( Transform transform, System.IO.BinaryWriter writer)
		{
			writer.Write(transform.localPosition.x);
			writer.Write(transform.localPosition.y);
			writer.Write(transform.localPosition.z);

			writer.Write(transform.localEulerAngles.x);
			writer.Write(transform.localEulerAngles.y);
			writer.Write(transform.localEulerAngles.z);

			writer.Write(transform.localScale.x);
			writer.Write(transform.localScale.y);
			writer.Write(transform.localScale.z);
		}

		/// <summary>
		/// Colorをバイナリ書き込み
		/// </summary>
		/// <param name="color">書き込むカラー</param>
		/// <param name="writer">バイナリライター</param>
		[System.Obsolete]
		public static void WriteColor( Color color, System.IO.BinaryWriter writer)
		{
			writer.Write(color.r);
			writer.Write(color.g);
			writer.Write(color.b);
			writer.Write(color.a);
		}

		/// <summary>
		/// Transformのローカル情報をバイナリ読み込み
		/// </summary>
		/// <param name="transform">読み込むTransform</param>
		/// <param name="reader">バイナリリーダー/param>
		[System.Obsolete]
		public static void ReadLocalTransform(Transform transform, System.IO.BinaryReader reader)
		{
			Vector3 pos = new Vector3();
			Vector3 euler = new Vector3();
			Vector3 scale = new Vector3();
			ReadLocalTransform(reader, out pos, out euler, out scale);
			transform.localPosition = pos;
			transform.localEulerAngles = euler;
			transform.localScale = scale;
		}

		/// <summary>
		/// Transformのローカル情報をバイナリ読み込み
		/// </summary>
		/// <param name="transform">読み込むTransform</param>
		/// <param name="reader">バイナリリーダー/param>
		[System.Obsolete]
		public static void ReadLocalTransform(System.IO.BinaryReader reader, out Vector3 pos, out Vector3 euler, out Vector3 scale)
		{
			pos.x = reader.ReadSingle();
			pos.y = reader.ReadSingle();
			pos.z = reader.ReadSingle();

			euler.x = reader.ReadSingle();
			euler.y = reader.ReadSingle();
			euler.z = reader.ReadSingle();

			scale.x = reader.ReadSingle();
			scale.y = reader.ReadSingle();
			scale.z = reader.ReadSingle();
		}


		/// <summary>
		/// Colorをバイナリ書き込み読み込み
		/// </summary>
		/// <param name="transform">読み込むカラー</param>
		/// <param name="reader">バイナリリーダー</param>
		/// <returns>読み込んだカラー値</returns>
		[System.Obsolete]
		public static Color ReadColor(System.IO.BinaryReader reader)
		{
			Color color;
			color.r = reader.ReadSingle();
			color.g = reader.ReadSingle();
			color.b = reader.ReadSingle();
			color.a = reader.ReadSingle();
			return color;
		}


		[System.Obsolete]
		public static void AddEventTriggerEntry(EventTrigger eventTrigger, UnityAction<UnityEngine.EventSystems.BaseEventData> action, EventTriggerType eventTriggerType)
		{
			EventTrigger.Entry entry = new EventTrigger.Entry();
			EventTrigger.TriggerEvent trigger = new EventTrigger.TriggerEvent();
			trigger.AddListener((eventData) => action(eventData));
			entry.callback = trigger;
			entry.eventID = eventTriggerType;
			WrapperUnityVersion.AddEntryToEventTrigger(eventTrigger,entry);
		}

		//配列に、新しい要素を重複させないものだけ追加する
		[System.Obsolete]
		internal static T[] AddArrayUnique<T>(T[] array, T[] addArray)
		{
			List<T> list = new List<T>(array);
			foreach( T item in addArray )
			{
				if (!list.Contains(item)) list.Add(item);
			}
			return list.ToArray();
		}
	}
}