// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UtageExtensions;

namespace Utage
{

	/// <summary>
	/// グラフィックオブジェクトを、キャラクターやBGなどのグループ単位で管理のためのスーパークラス
	/// </summary>
	public class AdvGraphicGroup
	{
		protected AdvLayerSettingData.LayerType type;
		internal AdvGraphicLayer DefaultLayer { get; set; }
		protected AdvGraphicManager manager;

		List<AdvGraphicLayer> layers = new List<AdvGraphicLayer>();

		//起動時の初期化
		internal AdvGraphicGroup(AdvLayerSettingData.LayerType type, AdvLayerSetting setting, AdvGraphicManager manager)
		{
			this.type = type;
			this.manager = manager;
			foreach (var item in setting.List)
			{
				if (item.Type == type)
				{
					//5.6対策でRectTransformを指定したnewが必要
					var go = new GameObject(item.Name, typeof(RectTransform), typeof(Canvas));
					manager.transform.AddChild(go);
					AdvGraphicLayer layer = go.AddComponent<AdvGraphicLayer>();
					layer.Init(manager, item);
					layers.Add(layer);
					if (item.IsDefault) DefaultLayer = layer;
				}
			}
		}

		//クリア
		internal virtual void Clear()
		{
			foreach (AdvGraphicLayer layer in layers)
			{
				layer.Clear();
			}
		}

		internal void DestroyAll()
		{
			foreach (AdvGraphicLayer layer in layers)
			{
				layer.Clear();
				GameObject.Destroy(layer.gameObject);
			}
			layers.Clear();
			DefaultLayer = null;
		}

		//表示する
		internal AdvGraphicObject Draw(string layerName, string name, AdvGraphicOperaitonArg arg)
		{
			return FindLayerOrDefault(layerName).Draw(name, arg);
		}

		//デフォルトレイヤーのデフォルトオブジェクトとして表示する
		internal AdvGraphicObject DrawToDefault(string name, AdvGraphicOperaitonArg arg)
		{
			return DefaultLayer.DrawToDefault(name, arg);
		}

		//キャラクターオブジェクトとして、特殊な表示をする
		internal AdvGraphicObject DrawCharacter(string layerName, string name, AdvGraphicOperaitonArg arg)
		{
			//既に同名のグラフィックがあるなら、そのレイヤーを取得
			AdvGraphicLayer oldLayer = layers.Find((item) => (item.IsEqualDefaultGraphicName(name)));

			//レイヤー名の指定がある場合、そのレイヤーを探す
			AdvGraphicLayer layer = layers.Find((item) => (item.SettingData.Name == layerName));
			if (layer == null)
			{
				//レイヤーがない場合は、旧レイヤーかデフォルトレイヤーを使う
				layer = (oldLayer == null) ? DefaultLayer : oldLayer;
			}

			//レイヤー変更があるか
			bool changeLayer = (oldLayer != layer && oldLayer != null);

			//レイヤー変更ないなら、描画しておわり
			if (!changeLayer)
			{
				//レイヤー上にデフォルトオブジェクトとして表示
				return layer.DrawToDefault(name, arg);
			}

			Vector3 oldScale = Vector3.one;
			Vector3 oldPosition = Vector3.zero;
			Quaternion oldRotation = Quaternion.identity;
			//レイヤーが変わる場合は、昔のほうを消す
			AdvGraphicObject oldObj;
			if (oldLayer.CurrentGraphics.TryGetValue(name, out oldObj))
			{
				oldScale = oldObj.rectTransform.localScale;
				oldPosition = oldObj.rectTransform.localPosition;
				oldRotation = oldObj.rectTransform.localRotation;
				oldLayer.FadeOut(name, arg.GetSkippedFadeTime(manager.Engine));
			}

			//レイヤー上にデフォルトオブジェクトとして表示
			AdvGraphicObject obj = layer.DrawToDefault(name, arg);
			//ローカルTransform値を引き継ぐ処理
			if (!manager.ResetCharacterTransformOnChangeLayer)
			{
				obj.rectTransform.localScale = oldScale;
				obj.rectTransform.localPosition = oldPosition;
				obj.rectTransform.localRotation = oldRotation;
			}
			return obj;
		}

		//現在描画オブジェクトのある全てのレイヤー
		internal List<AdvGraphicLayer> AllGraphicsLayers()
		{
			List<AdvGraphicLayer> list = new List<AdvGraphicLayer>();
			foreach (AdvGraphicLayer layer in layers)
			{
				if (layer.CurrentGraphics.Count>0)
				{
					list.Add(layer);
				}
			}
			return list;
		}


		//指定名のオブジェクトを非表示（フェードアウト）する
		internal virtual void FadeOut(string name, float fadeTime)
		{
			AdvGraphicLayer layer = FindLayerFromObjectName(name);
			if (layer != null) layer.FadeOut(name, fadeTime);
		}

		//全オブジェクトを非表示（フェードアウト）する
		internal virtual void FadeOutAll(float fadeTime)
		{
			foreach (AdvGraphicLayer layer in layers)
			{
				layer.FadeOutAll(fadeTime);
			}
		}

		//指定名のパーティクルを非表示にする
		internal void FadeOutParticle(string name)
		{
			foreach (AdvGraphicLayer layer in layers)
			{
				layer.FadeOutParticle(name);
			}
		}

		//パーティクルを全て非表示にする
		internal void FadeOutAllParticle()
		{
			foreach (AdvGraphicLayer layer in layers)
			{
				layer.FadeOutAllParticle();
			}
		}

		//指定名グラフィックオブジェクトを持つか
		internal bool IsContians(string layerName, string name)
		{
			if (string.IsNullOrEmpty(layerName))
			{
				return FindObject(name) !=null;
			}
			else
			{
				AdvGraphicLayer layer = FindLayer(layerName);
				return (layer != null && layer.Find(name) != null);
			}
		}

		//指定の名前のグラフィックオブジェクトを持つレイヤーを探す
		internal AdvGraphicLayer FindLayerFromObjectName(string name)
		{
			foreach (AdvGraphicLayer layer in layers)
			{
				if (layer.Contains(name)) return layer;
			}
			return null;
		}

		//指定の名前のレイヤーを探す
		internal AdvGraphicLayer FindLayer(string name)
		{
			return layers.Find(item => item.name == name);
		}

		//指定の名前のレイヤーを探す（見つからなかったらデフォルト）
		internal AdvGraphicLayer FindLayerOrDefault(string name)
		{
			return layers.Find((item) => (item.SettingData.Name == name)) ?? DefaultLayer;
		}

		//指定の名前のグラフィックオブジェクトをを探す
		internal AdvGraphicObject FindObject(string name)
		{
			foreach (AdvGraphicLayer layer in layers)
			{
				AdvGraphicObject obj = layer.Find(name);
				if (obj != null) return obj;
			}
			return null;
		}

		//全てのグラフィックオブジェクトを取得
		internal List<AdvGraphicObject> AllGraphics()
		{
			List<AdvGraphicObject> allGraphics = new List<AdvGraphicObject>();
			foreach (AdvGraphicLayer layer in layers)
			{
				layer.AddAllGraphics(allGraphics);
			}
			return allGraphics;
		}

		internal void AddAllGraphics(List<AdvGraphicObject> graphics)
		{
			foreach (AdvGraphicLayer layer in layers)
			{
				layer.AddAllGraphics(graphics);
			}
		}

		//ロード中かチェック
		internal bool IsLoading
		{
			get
			{
				foreach (AdvGraphicLayer layer in layers)
				{
					if (layer.IsLoading) return true;
				}
				return false;
			}
		}

		const int Version = 0;
		//セーブデータ用のバイナリ書き込み
		public void Write(BinaryWriter writer)
		{
			writer.Write(Version);
			writer.Write(layers.Count);
			foreach (var layer in layers)
			{
				writer.Write(layer.name);
				writer.WriteBuffer(layer.Write);
			}
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

			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string layerName = reader.ReadString();
				AdvGraphicLayer layer = FindLayer(layerName);
				if (layer != null)
				{
					reader.ReadBuffer(layer.Read);
				}
				else
				{
					reader.SkipBuffer();
				}
			}
		}
	}
}