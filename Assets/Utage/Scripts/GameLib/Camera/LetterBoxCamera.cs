using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UtageExtensions;

namespace Utage
{
	/// <summary>
	/// カメラのイベント処理
	/// </summary>
	[System.Serializable]
	public class LetterBoxCameraEvent : UnityEvent<LetterBoxCamera> { }

	/// <summary>
	/// カメラ制御。
	/// あらかじめ想定するゲームの画面サイズを設定し、
	/// 実行環境のデバイスの解像度あわせて画面全体を拡大・縮小するように設定する
	/// 設定した範囲内で表示するアスペクト比を変更し、余白部分はレターボックスで埋める。
	/// </summary>
	[ExecuteInEditMode]
	[AddComponentMenu("Utage/Lib/Camera/LetterBoxCamera")]
	public class LetterBoxCamera : MonoBehaviour
	{
		/// <summary>
		/// 2Dカメラの1単位あたりのピクセル数
		/// </summary>
		public int PixelsToUnits { get { return pixelsToUnits; } set{ hasChanged = true; pixelsToUnits = value; }}
		[SerializeField]
		int pixelsToUnits = 100;

		/// <summary>画面サイズ：幅(px)</summary>
		public int Width { get { return width; } set{ hasChanged = true; width = value; }}
		[SerializeField]
		int width = 800;

		/// <summary>画面サイズ：高さ(px)</summary>
		public int Height { get { return height; } set{ hasChanged = true; height = value; }}
		[SerializeField]
		int height = 600;

		/// <summary></summary>
		public bool IsFlexible { get { return isFlexible; } set{ hasChanged = true; isFlexible = value; }}
		[SerializeField]
		bool isFlexible = false;
	
		/// <summary>画面サイズ：幅(px)</summary>
		public int MaxWidth { get { return maxWidth; } set{ hasChanged = true; maxWidth = value; }}
		[SerializeField]
		int maxWidth = 800;
		
		/// <summary>画面サイズ：高さ(px)</summary>
		public int MaxHeight { get { return maxHeight; } set{ hasChanged = true; maxHeight = value; }}
		[SerializeField]
		int maxHeight = 600;

		public int FlexibleMinWidth { get { return IsFlexible ? Mathf.Min(Width, Width, MaxWidth) : Width; } }
		public int FlexibleMinHeight { get { return IsFlexible ? Mathf.Min(Height, Height, MaxHeight) : Height; } }

		public int FlexibleMaxWidth { get { return IsFlexible ? Mathf.Max(Width, Width, MaxWidth) : Width; } }
		public int FlexibleMaxHeight { get { return IsFlexible ? Mathf.Max(Height, Height, MaxHeight) : Height; } }

		public enum AnchorType
		{
			UpperLeft,
			UpperCenter,
			UpperRight,
			MiddleLeft,
			MiddleCenter,
			MiddleRight,
			LowerLeft,
			LowerCenter,
			LowerRight
		};

		//レターボックスを使う際に、ゲーム画面を画面中央ではなく、下にくっつける形にする。広告表示などのレイアウトに合わせるために
		[SerializeField]
		AnchorType anchor = AnchorType.MiddleCenter;

		public LetterBoxCameraEvent OnGameScreenSizeChange = new LetterBoxCameraEvent ();

		float screenAspectRatio;		//デバイススクリーンのアスペクト比
		Vector2 padding;                //レターボックスのために使う、カメラのビューポート矩形の余白部分

		/// <summary>
		/// 現在の画面サイズ(px)
		/// </summary>
		public Vector2 CurrentSize
		{
			get
			{
				if (hasChanged)
				{
					RefreshCurrentSize();
				}
				return currentSize;
			}
		}
		Vector2 currentSize;

		public Camera CachedCamera
		{
			get
			{
				if (cachedCamera == null)
				{
					cachedCamera = this.GetComponent<Camera>();
				}
				return cachedCamera;
			}
		}

		//2D（OrthoGrahpic）の場合のズーム倍率
		public float Zoom2D
		{
			get
			{
				return zoom2D;
			}
			set
			{
				zoom2D = value;
				hasChanged = true;
			}
		}
		[SerializeField]
		public float zoom2D = 1.0f;

		//2D（OrthoGrahpic）の場合のズーム中心点
		public Vector2 Zoom2DCenter
		{
			get
			{
				return zoom2DCenter;
			}
			set
			{
				zoom2DCenter = value;
				hasChanged = true;
			}
		}
		[SerializeField]
		public Vector2 zoom2DCenter;

		internal void SetZoom2D(float zoom, Vector2 center)
		{
			this.Zoom2D = zoom;
			this.Zoom2DCenter = center;
		}

		Camera cachedCamera;
		bool hasChanged = true;

		void Start()
		{
			hasChanged = true;
		}

		void OnValidate()
		{
			hasChanged = true;
		}

		void Update()
		{
			if (hasChanged ||
			    (!Mathf.Approximately(screenAspectRatio, 1.0f * Screen.width / Screen.height))
			    )
			{
				Refresh();
			}
		}

		public void Refresh()
		{
			hasChanged = false;
			RefreshCurrentSize();
			RefreshCamera();
		}

		void RefreshCurrentSize()
		{			
			if (TryRefreshCurrentSize())
			{
				OnGameScreenSizeChange.Invoke(this);
			}
		}

		bool TryRefreshCurrentSize()
		{
			screenAspectRatio = 1.0f * Screen.width / Screen.height;

			float defaultAspectRatio = (float)Width/Height;
			float wideAspectRatio = (float)FlexibleMaxWidth / FlexibleMinHeight;
			float nallowAspectRatio = (float)FlexibleMinWidth / FlexibleMaxHeight;

			int w, h;
			//スクリーンのアスペクト比から、ゲームの画面サイズを決める
			if (screenAspectRatio > wideAspectRatio)
			{
				//スクリーンのほうが限界よりも横長なので、左右にレターボックス

				padding.x = (1.0f - wideAspectRatio / screenAspectRatio) / 2;
				padding.y = 0;

				w = FlexibleMaxWidth;	//横は最大を
				h = FlexibleMinHeight;	//縦は最小を
			}
			else if (screenAspectRatio < nallowAspectRatio)
			{
				//スクリーンのほうが限界よりも縦長なので、上下にレターボックス
				padding.x = 0;
				padding.y = (1.0f - screenAspectRatio / nallowAspectRatio) / 2;

				w = FlexibleMinWidth;			//横は最小を
				h = FlexibleMaxHeight;	//縦は最大を
			}
			else
			{
				//アスペクト比が設定の範囲内ならレターボックスなし
				padding.x = 0;
				padding.y = 0;

				if (Mathf.Approximately(screenAspectRatio, defaultAspectRatio))
				{
					//基本的なアスペクト比と同じ
					w = Width;
					h = Height;
				}
				else
				{
					h = FlexibleMinHeight;
					w = Mathf.FloorToInt(screenAspectRatio * h);
					if (w < FlexibleMinWidth)
					{
						w = FlexibleMinWidth;
						h = Mathf.FloorToInt(w / screenAspectRatio);
					}
				}
			}

			bool changed = (currentSize.x != w ) || (currentSize.y != h);
			currentSize = new Vector2(w,h);
			return changed;
		}

		void RefreshCamera()
		{
			float x = padding.x;
			float width = 1 - padding.x * 2;
			float y = padding.y;
			float height = 1 - padding.y * 2;

			switch (anchor)
			{
				case AnchorType.UpperLeft:
					x = 0;
					y = padding.y * 2;
					break;
				case AnchorType.UpperCenter:
					y = padding.y * 2;
					break;
				case AnchorType.UpperRight:
					x = padding.x * 2;
					y = padding.y * 2;
					break;
				case AnchorType.MiddleLeft:
					x = 0;
					break;
				case AnchorType.MiddleCenter:
					break;
				case AnchorType.MiddleRight:
					x = padding.x * 2;
					break;
				case AnchorType.LowerLeft:
					x = 0;
					y = 0;
					break;
				case AnchorType.LowerCenter:
					y = 0;
					break;
				case AnchorType.LowerRight:
					x = padding.x * 2;
					y = 0;
					break;
			}
			Rect rect = new Rect(x, y, width, height);
			CachedCamera.orthographicSize = CurrentSize.y / (2 * pixelsToUnits)/ Zoom2D;
			CachedCamera.rect = rect;

			Vector2 zoom2DCenterOffset = (-1.0f / Zoom2D + 1) * Zoom2DCenter / (pixelsToUnits);
			CachedCamera.transform.localPosition = zoom2DCenterOffset;
		}

		const int Version = 0;
		//セーブデータ用のバイナリ書き込み
		public void Write(BinaryWriter writer)
		{
			writer.Write(Version);
			writer.Write (Zoom2D);
			writer.Write (Zoom2DCenter);
		}

		//セーブデータ用のバイナリ読み込み
		public void Read(BinaryReader reader)
		{
			int version = reader.ReadInt32();
			if (version < 0 || version > Version)
			{
				Debug.LogError(LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.UnknownVersion, version));
				return;
			}
			this.Zoom2D = reader.ReadSingle();
			this.Zoom2DCenter = reader.ReadVector2();
		}

        internal void OnClear()
        {
			this.Zoom2D = 1;
            this.Zoom2DCenter = Vector2.zero;
		}
	}
}