// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using Utage;

namespace UtageExtensions
{
	//拡張メソッド
	public static class UtageExtensions
	{
		//カンマやスラッシュなど、区切り文字の前後で文字列を分割する（区切り文字が複数ある場合は最初か最後で分割）
		public static void Separate(this string str, char separator, bool isFirst, out string str1, out string str2)
		{
			int index = isFirst ? str.IndexOf(separator) : str.LastIndexOf(separator);
			str1 = str.Substring(0, index);
			str2 = str.Substring(index + 1);
		}

		public static bool IsNullOrEmpty(this string str)
		{
			return string.IsNullOrEmpty(str);
		}


		//********GameObjectの拡張メソッド********//

		/// <summary>
		/// SendMessageする
		/// ただし、funcがnullだった場合何もしない
		/// </summary>
		/// <param name="functionName">送信するメッセージ</param>
		/// <param name="isForceActive">送り先のGameObjectを強制的にactiveにしてからSendMessageするか</param>
		public static void SafeSendMessage(this GameObject go, string functionName, System.Object obj = null, bool isForceActive = false)
		{
			if (!string.IsNullOrEmpty(functionName))
			{
				if (isForceActive) go.SetActive(true);
				go.SendMessage(functionName, obj, SendMessageOptions.DontRequireReceiver);
			}
		}

		//コンポーネントのキャッシュを取得
		public static T GetComponentCache<T>(this GameObject go, ref T component) where T : class
		{
			return component ?? (component = go.GetComponent<T>());
		}

		//コンポーネントのキャッシュを取得。なかったら作成
		public static T GetComponentCacheCreateIfMissing<T>(this GameObject go, ref T component) where T : Component
		{
			return component ?? (component = GetComponentCreateIfMissing<T>(go));
		}


		//コンポーネンを取得。なかったら作成
		public static T GetComponentCreateIfMissing<T>(this GameObject go) where T : Component
		{
			T component = go.GetComponent<T>();
			if (component == null)
			{
				component = go.AddComponent<T>();
			}
			return component;
		}

		//コンポーネントのキャッシュを子オブジェクトから取得
		public static T GetComponentCacheInChildren<T>(this GameObject go, ref T component) where T : class
		{
			return component ?? (component = go.GetComponentInChildren<T>(true));
		}


		//コンポーネント配列のキャッシュを子オブジェクトから取得
		public static T[] GetComponentsCacheInChildren<T>(this GameObject go, ref T[] components) where T : class
		{
			return components ?? (components = go.GetComponentsInChildren<T>(true));
		}

		//コンポーネント配列のキャッシュを子オブジェクトから取得
		public static List<T> GetComponentListCacheInChildren<T>(this GameObject go, ref List<T> components) where T : class
		{
			return components ?? (components = new List<T>(go.GetComponentsInChildren<T>(true)));
		}

		//コンポーネントのキャッシュを取得(なかったらシーン内からFind)
		public static T GetComponentCacheFindIfMissing<T>(this GameObject go, ref T component) where T : Component
		{
			if (component == null)
			{
				component = GameObject.FindObjectOfType<T>();
			}
			return component;
		}

		//********Componentの拡張メソッド********//

		//コンポーネントのキャッシュを取得
		public static T GetComponentCache<T>(this Component target, ref T component) where T : class
		{
			try
			{
				return target.gameObject.GetComponentCache<T>(ref component);
			}
			catch (System.Exception e)
			{
				Debug.LogError(e.Message);
				return null;
			}
		}

		//コンポーネントのキャッシュを取得
		public static T[] GetComponentsCacheInChildren<T>(this Component target, ref T[] components) where T : class
		{
			try
			{
				return target.gameObject.GetComponentsCacheInChildren<T>(ref components);
			}
			catch (System.Exception e)
			{
				Debug.LogError(e.Message);
				return null;
			}
		}

		//コンポーネントのキャッシュを取得
		public static List<T> GetComponentListCacheInChildren<T>(this Component target, ref List<T> components) where T : class
		{
			try
			{
				return target.gameObject.GetComponentListCacheInChildren<T>(ref components);
			}
			catch (System.Exception e)
			{
				Debug.LogError(e.Message);
				return null;
			}
		}

		//コンポーネントのキャッシュを取得。なかったら作成
		public static T GetComponentCacheCreateIfMissing<T>(this Component target, ref T component) where T : Component
		{
			try
			{
				return target.gameObject.GetComponentCacheCreateIfMissing<T>(ref component);
			}
			catch (System.Exception e)
			{
				Debug.LogError(e.Message);
				return null;
			}
		}

		//コンポーネンを取得。なかったら作成
		public static T GetComponentCreateIfMissing<T>(this Component target) where T : Component
		{
			return target.gameObject.GetComponentCreateIfMissing<T>();
		}

		//コンポーネンを削除
		public static void RemoveComponent<T>(this GameObject target, bool immediate = true) where T : Component
		{
			T component = target.GetComponent<T>();
			component.RemoveComponentMySelf(immediate);
		}

		//コンポーネンを削除
		public static void RemoveComponents<T>(this GameObject target, bool immediate = true) where T : Component
		{
			T[] components = target.GetComponents<T>();
			foreach (var component in components)
			{
				component.RemoveComponentMySelf(immediate);
			}
		}

		//コンポーネンを削除
		public static void RemoveComponentMySelf(this Component target, bool immediate = true)
		{
			if (target != null)
			{
				if (immediate)
				{
					UnityEngine.Object.DestroyImmediate(target);
				}
				else
				{
					UnityEngine.Object.Destroy(target);
				}
			}
		}

		//コンポーネントのキャッシュを子オブジェクトから取得
		public static T GetComponentCacheInChildren<T>(this Component target, ref T component) where T : Component
		{
			return component ?? (component = target.GetComponentInChildren<T>(true));
		}


		/// <summary>
		/// 子を含む全てのレイヤーを変更する
		/// </summary>
		/// <param name="trans">レイヤーを変更する対象</param>
		/// <param name="layer">設定するレイヤー</param>
		public static void ChangeLayerDeep(this GameObject go, int layer)
		{
			go.layer = layer;
			foreach (Transform child in go.transform)
			{
				child.gameObject.ChangeLayerDeep(layer);
			}
		}

		//コンポーネントのキャッシュを取得(なかったらシーン内からFind)
		public static T GetComponentCacheFindIfMissing<T>(this Component target, ref T component) where T : Component
		{
			return target.gameObject.GetComponentCacheFindIfMissing<T>(ref component);
		}


		//コンポーネントのシングルトンの処理
		public static T GetSingletonFindIfMissing<T>(this T target, ref T instance) where T : Component
		{
			if (instance == null)
			{
				instance = GameObject.FindObjectOfType<T>();
			}
			return instance;
		}

		//コンポーネントのシングルトンの処理
		public static void InitSingletonComponent<T>(this T target, ref T instance) where T : Component
		{
			if (instance != null && instance != target)
			{
				Debug.LogErrorFormat("{0} is multiple created", typeof(T).ToString());
				GameObject.Destroy(target.gameObject);
			}
			else
			{
				instance = target;
			}
		}


		//********Transformの拡張メソッド********//

		//指定の名前のオブジェクトを子供以下の全ての階層から取得して、そのコンポーネントをGetする
		public static T Find<T>(this Transform t, string name) where T : Component
		{
			var child = t.Find(name);
			if (child == null) return null;
			return child.GetComponent<T>();
		}

		//指定の名前のオブジェクトを子供以下の全ての階層から取得
		public static Transform FindDeep(this Transform t, string name, bool includeInactive = false)
		{
			var children = t.GetComponentsInChildren<Transform>(includeInactive);
			foreach (var child in children)
			{
				if (child.gameObject.name == name)
				{
					return child;
				}
			}
			return null;
		}

		//指定の名前のオブジェクトを子供以下の全ての階層から取得して、そのコンポーネントをGetする
		public static T FindDeepAsComponent<T>(this Transform t, string name, bool includeInactive = false) where T : Component
		{
			var children = t.GetComponentsInChildren<T>(includeInactive);
			foreach (var child in children)
			{
				if (child.gameObject.name == name)
				{
					return child;
				}
			}
			return null;
		}


		/// <summary>
		/// 子の追加
		/// </summary>
		/// <param name="go">子</param>
		/// <returns>追加済みの子</returns>
		public static GameObject AddChild(this Transform t, GameObject child)
		{
			return t.AddChild(child, Vector3.zero, Vector3.one);
		}

		/// <summary>
		/// 子の追加
		/// </summary>
		/// <param name="child">子</param>
		/// <param name="localPosition">子に設定するローカル座標</param>
		/// <returns>追加済みの子</returns>
		public static GameObject AddChild(this Transform t, GameObject child, Vector3 localPosition)
		{
			return t.AddChild(child, localPosition, Vector3.one);
		}
		/// <summary>
		/// 子の追加
		/// </summary>
		/// <param name="parent">親</param>
		/// <param name="go">子</param>
		/// <param name="localPosition">子に設定するローカル座標</param>
		/// <param name="localScale">子に設定するローカルスケール</param>
		/// <returns>追加済みの子</returns>
		public static GameObject AddChild(this Transform t, GameObject child, Vector3 localPosition, Vector3 localScale)
		{
			child.transform.SetParent(t);
			child.transform.localScale = localScale;
			child.transform.localPosition = localPosition;
			if (child.transform is RectTransform)
			{
				(child.transform as RectTransform).anchoredPosition = localPosition;
			}
			child.transform.localRotation = Quaternion.identity;
			child.ChangeLayerDeep(t.gameObject.layer);
			return child;
		}

		/// <summary>
		/// プレハブからGameObjectを作成して子として追加する
		/// </summary>
		/// <param name="prefab">子を作成するためのプレハブ</param>
		/// <returns>追加済みの子</returns>
		public static GameObject AddChildPrefab(this Transform t, GameObject prefab)
		{
			return t.AddChildPrefab(prefab, Vector3.zero, Vector3.one);
		}

		/// <summary>
		/// プレハブからGameObjectを作成して子として追加する
		/// </summary>
		/// <param name="prefab">子を作成するためのプレハブ</param>
		/// <param name="localPosition">子に設定するローカル座標</param>
		/// <returns>追加済みの子</returns>
		public static GameObject AddChildPrefab(this Transform t, GameObject prefab, Vector3 localPosition)
		{
			return t.AddChildPrefab(prefab, localPosition, Vector3.one);
		}
		/// <summary>
		/// プレハブからGameObjectを作成して子として追加する
		/// </summary>
		/// <param name="parent">親</param>
		/// <param name="prefab">子を作成するためのプレハブ</param>
		/// <param name="localPosition">子に設定するローカル座標</param>
		/// <param name="localScale">子に設定するローカルスケール</param>
		public static GameObject AddChildPrefab(this Transform t, GameObject prefab, Vector3 localPosition, Vector3 localScale)
		{
			GameObject go = GameObject.Instantiate(prefab, t) as GameObject;
			go.transform.localScale = localScale;
			go.transform.localPosition = localPosition;
			go.ChangeLayerDeep(t.gameObject.layer);
			return go;
		}


		/// <summary>
		/// UnityオブジェクトからGameObjectを作成して子として追加する
		/// </summary>
		/// <param name="parent">親</param>
		/// <param name="obj">子を作成するためのUnityオブジェクト</param>
		/// <returns>追加済みの子</returns>
		public static GameObject AddChildUnityObject(this Transform t, UnityEngine.Object obj)
		{
			GameObject go = GameObject.Instantiate(obj, t) as GameObject;
			return go;
		}


		/// <summary>
		/// GameObjectを作成し、子として追加
		/// </summary>
		/// <param name="parent">親</param>
		/// <param name="name">作成する子の名前</param>
		/// <returns>追加済みの子</returns>
		public static GameObject AddChildGameObject(this Transform t, string name)
		{
			return t.AddChildGameObject(name, Vector3.zero, Vector3.one);
		}

		/// <summary>
		/// GameObjectを作成し、子として追加
		/// </summary>
		/// <param name="parent">親</param>
		/// <param name="name">作成する子の名前</param>
		/// <param name="localPosition">子に設定するローカル座標</param>
		/// <returns>追加済みの子</returns>
		public static GameObject AddChildGameObject(this Transform t, string name, Vector3 localPosition)
		{
			return t.AddChildGameObject(name, localPosition, Vector3.one);
		}

		/// <summary>
		/// GameObjectを作成し、子として追加
		/// </summary>
		/// <param name="parent">親</param>
		/// <param name="name">作成する子の名前</param>
		/// <param name="localPosition">子に設定するローカル座標</param>
		/// <returns>追加済みの子</returns>
		/// <param name="localScale">子に設定するローカルスケール</param>
		public static GameObject AddChildGameObject(this Transform t, string name, Vector3 localPosition, Vector3 localScale)
		{
			GameObject go = (t is RectTransform) ? new GameObject(name, typeof(RectTransform)) : new GameObject(name);
			t.AddChild(go, localPosition, localScale);
			return go;
		}

		/// <summary>
		/// 指定のコンポーネントつきのGameObjectを作成して子として追加
		/// </summary>
		/// <typeparam name="T">コンポーネントの型</typeparam>
		/// <param name="name">作成する子の名前</param>
		/// <returns>追加済みの子</returns>
		public static T AddChildGameObjectComponent<T>(this Transform t, string name) where T : Component
		{
			return t.AddChildGameObjectComponent<T>(name, Vector3.zero, Vector3.one);
		}

		/// <summary>
		/// 指定のコンポーネントつきのGameObjectを作成して子として追加
		/// </summary>
		/// <typeparam name="T">コンポーネントの型</typeparam>
		/// <param name="name">作成する子の名前</param>
		/// <param name="localPosition">子に設定するローカル座標</param>
		/// <returns>追加済みの子</returns>
		public static T AddChildGameObjectComponent<T>(this Transform t, string name, Vector3 localPosition) where T : Component
		{
			return t.AddChildGameObjectComponent<T>(name, localPosition, Vector3.one);
		}
		/// <summary>
		/// 指定のコンポーネントつきのGameObjectを作成して子として追加
		/// </summary>
		/// <typeparam name="T">コンポーネントの型</typeparam>
		/// <param name="name">作成する子の名前</param>
		/// <param name="localPosition">子に設定するローカル座標</param>
		/// <param name="localScale">子に設定するローカルスケール</param>
		/// <returns>追加済みの子</returns>
		public static T AddChildGameObjectComponent<T>(this Transform t, string name, Vector3 localPosition, Vector3 localScale) where T : Component
		{
			GameObject go = t.AddChildGameObject(name, localPosition, localScale);
			T component = go.GetComponent<T>();
			if (component == null)
			{
				return go.AddComponent<T>();
			}
			else
			{
				return component;
			}
		}

		/// <summary>
		/// 指定のコンポーネントつきのGameObjectを子オブジェトの先頭からコピーして指定の数になるように追加
		/// </summary>
		/// <typeparam name="T">コンポーネントの型</typeparam>
		/// <param name="count">子の数</param>
		public static void InitCloneChildren<T>(this Transform t, int count) where T : Component
		{
			//今ある子
			T[] chidlren = t.GetComponentsInChildren<T>(true);
			if (chidlren.Length <= 0)
			{
				Debug.LogError(typeof(T).Name + " is not under " + t.gameObject.name);
				return;
			}
			int addCount = Mathf.Max(0, count - chidlren.Length);
			for (int i = 0; i < addCount; ++i)
			{
				t.AddChildPrefab(chidlren[0].gameObject, chidlren[0].gameObject.transform.localPosition, chidlren[0].gameObject.transform.localScale);
			}
		}

		/// <summary>
		/// 指定のコンポーネントつきのGameObjectを子オブジェトの先頭からコピーして指定の数になるように追加
		/// </summary>
		/// <typeparam name="T">コンポーネントの型</typeparam>
		/// <param name="count">子の数</param>
		public static void InitCloneChildren<T>(this Transform t, int count, Action<T,int> callback) where T : Component
		{
			t.InitCloneChildren<T>(count);

			T[] chidlren = t.GetComponentsInChildren<T>(true);
			for (int i = 0; i < chidlren.Length; ++i)
			{
				callback(chidlren[i],i);
			}
		}

		/// <summary>
		/// 指定のコンポーネントつきのGameObjectを子オブジェトの先頭からコピーして、リストの指定の数になるようにし、初期化コールバックを返す
		/// </summary>
		/// <typeparam name="T">コンポーネントの型</typeparam>
		/// <param name="count">子の数</param>
		public static void InitCloneChildren<TComponent,TList>(this Transform t, List<TList> list, Action<TComponent, TList> callback) where TComponent : Component
		{
			t.InitCloneChildren<TComponent>(
				Mathf.Max( list.Count,1),
				(item, index) =>
				{
					if (index < list.Count)
					{
						item.gameObject.SetActive(true);
						callback(item, list[index]);
					}
					else
					{
						item.gameObject.SetActive(false);
					}
				});
		}


		/// <summary>
		/// 指定のコンポーネントを階層以下から探し、なかったら子オブジェクトとしてそのコンポーネントを持つ子オブジェクトを作成する
		/// </summary>
		public static T GetCompoentInChildrenCreateIfMissing<T>(this Transform t) where T : Component
		{
			return t.GetCompoentInChildrenCreateIfMissing<T>(typeof(T).Name);
		}

		/// <summary>
		/// 指定のコンポーネントを階層以下から探し、なかったら子オブジェクトとしてそのコンポーネントを持つ子オブジェクトを作成する
		/// </summary>
		public static T GetCompoentInChildrenCreateIfMissing<T>(this Transform t, string name) where T : Component
		{
			T component = t.GetComponentInChildren<T>();
			if (component == null)
			{
				component = t.AddChildGameObjectComponent<T>(name);
			}
			return component;
		}


		/// <summary>
		/// 子要素の全削除
		/// </summary>
		public static void DestroyChildren(this Transform t)
		{
			List<Transform> list = new List<Transform>();
			foreach (Transform child in t)
			{
				child.gameObject.SetActive(false);
				list.Add(child);
			}
			t.DetachChildren();
			foreach (Transform child in list)
			{
				GameObject.Destroy(child.gameObject);
			}
		}

		/// <summary>
		/// 子要素の全削除(エディタ上ではDestroyImmediateを使う)
		/// </summary>
		/// <param name="parent">親要素</param>
		public static void DestroyChildrenInEditorOrPlayer(this Transform t)
		{
			List<Transform> list = new List<Transform>();
			foreach (Transform child in t)
			{
				child.gameObject.SetActive(false);
				list.Add(child);
			}
			t.DetachChildren();
			foreach (Transform child in list)
			{
				if (Application.isPlaying)
				{
					GameObject.Destroy(child.gameObject);
				}
				else
				{
					GameObject.DestroyImmediate(child.gameObject);
				}
			}
		}

		//********RectTransformの拡張メソッド********//

		//サイズの取得
		public static Vector2 GetSize(this RectTransform t)
		{
			Rect rect = t.rect;
			return new Vector2(rect.width, rect.height);
		}
		//ローカルスケールを反映したサイズの取得
		public static Vector2 GetSizeScaled(this RectTransform t)
		{
			Rect rect = t.rect;
			return new Vector2(rect.width * t.localScale.x, rect.height * t.localScale.y);
		}

		//サイズの設定
		public static void SetSize(this RectTransform t, Vector2 size)
		{
			t.SetWidth(size.x);
			t.SetHeight(size.y);
		}
		//サイズの設定
		public static void SetSize(this RectTransform t, float width, float height)
		{
			t.SetWidth(width);
			t.SetHeight(height);
		}

		//Widthの取得
		public static float GetWith(this RectTransform t)
		{
			return t.rect.width;
		}
		//Widthの設定
		public static void SetWidth(this RectTransform t, float width)
		{
			t.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
		}

		//Widthの設定(親の幅との割合で。ゲージなどに使う)
		public static void SetWidthWidthParentRatio(this RectTransform t, float ratio)
		{
			RectTransform p = t.parent as RectTransform;
			float w = p.GetWith() * ratio;
			t.SetWidth(w);
		}

		//Heightの取得
		public static float GetHeight(this RectTransform t)
		{
			return t.rect.height;
		}
		//Heightの設定
		public static void SetHeight(this RectTransform t, float height)
		{
			t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
		}

		//ストレッチ（親オブジェクトの大きさに合わせた矩形）に設定する
		public static void SetStretch(this RectTransform t)
		{
			t.anchorMin = Vector2.zero;
			t.anchorMax = Vector2.one;
			t.sizeDelta = Vector2.zero;
		}

		//キャンバス内での相対座標としての、矩形を取得（回転はないものとする）
		public static Rect RectInCanvas(this RectTransform t, Canvas canvas)
		{
			Rect rect = t.rect;
			Vector3 position = t.TransformPoint(rect.center);
			position = canvas.transform.InverseTransformPoint(position);
			Vector3 size = t.GetSizeScaled();
			t.TransformVector(size);
			canvas.transform.InverseTransformVector(size);
			Rect ret = new Rect();
			ret.size = size;
			ret.center = position;
			return ret;
		}

		//********Graphicの拡張メソッド********//

		//α値の設定
		public static void SetAlpha(this Graphic graphic, float alpha)
		{
			Color c = graphic.color;
			c.a = alpha;
			graphic.color = c;
		}

		//α値の取得
		public static float GetAlpha(this Graphic graphic)
		{
			return graphic.color.a;
		}

		//********Rectの拡張メソッド********//

		//テクスチャの幅と高さと、切り抜き矩形からUV座標を取得
		internal static Rect ToUvRect(this Rect rect, float w, float h)
		{
			return new Rect(rect.x / w, 1.0f - (rect.yMax) / h, rect.width / w, rect.height / h);
		}

		//********RenderTextureの拡張メソッド********//

		//元のテクスチャをコピーした一時的なRenderTextureを作成する
		public static RenderTexture CreateCopyTemporary(this RenderTexture renderTexture)
		{
			return renderTexture.CreateCopyTemporary(renderTexture.depth);
		}
		public static RenderTexture CreateCopyTemporary(this RenderTexture renderTexture, int depth)
		{
			RenderTexture copy = RenderTexture.GetTemporary(renderTexture.width, renderTexture.height, depth, renderTexture.format);
			Graphics.Blit(renderTexture, copy);
			return copy;
		}


		//******** Dictionaryの拡張メソッド********//

		// 値を取得、keyがなければデフォルト値を設定し、デフォルト値を取得
		public static TValue GetValueOrSetDefaultIfMissing<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
		{
			TValue value;
			if (!dictionary.TryGetValue(key, out value))
			{
				dictionary.Add(key, defaultValue);
				return defaultValue;
			}
			else
			{
				return value;
			}
		}

		/// 値を取得、keyがなければデフォルト値を設定し、デフォルト値を取得
		public static TValue GetValueOrGetNullIfMissing<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : class
		{
			TValue value;
			if (!dictionary.TryGetValue(key, out value))
			{
				return null;
			}
			else
			{
				return value;
			}
		}


		//Vector2の拡張
		public static bool Approximately(this Vector2 a, Vector2 b)
		{
			return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y);
		}
	}
}