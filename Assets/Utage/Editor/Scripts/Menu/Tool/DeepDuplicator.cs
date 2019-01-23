// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Utage
{
	//指定したフォルダをすべて複製して、コピー対象のアセットのGUID参照を付け替える
	public class DeepDuplicate : EditorWindow
	{
		const string MenuPath = "Assets/Utage/Tool/DeepDuplicate";
		// 右クリックMenuに追加.
		[MenuItem(MenuPath)]
		private static void GetFilePath()
		{
			DeepDuplicator deepDuplicator = new DeepDuplicator();
			deepDuplicator.Do(AssetDatabase.GetAssetPath(Selection.activeObject));
		}

		// Validate the menu item defined by the function above.
		// The menu item will be disabled if this function returns false.
		[MenuItem(MenuPath, true)]
		static bool IsValidate()
		{
			return AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(Selection.activeObject));
		}
	}

	//指定したフォルダをすべて複製して、コピー対象のアセットのGUID参照を付け替える
	public class DeepDuplicator
	{
		string srcPath;
		string newPath;
		public void Do(string srcPath)
		{
			this.srcPath = srcPath;
			if (!AssetDatabase.IsValidFolder(srcPath))
			{
				Debug.LogError(srcPath + " is not folder path");
				return;
			}

			this.newPath = AssetDatabase.GenerateUniqueAssetPath(srcPath);

			AssetDatabase.CopyAsset(srcPath, newPath);
			//いったんアセットをリフレッシュ
			AssetDatabase.Refresh();
			//アセットのセーブ
			AssetDatabase.SaveAssets();

			//アセットの編集開始
			AssetDatabase.StartAssetEditing();

			EditorUtility.DisplayProgressBar("DeepDuplicate", "Start", 0);
			//アセットの再設定
			RebuildAssetsSub();
			EditorUtility.ClearProgressBar();

			//アセットの編集終了
			AssetDatabase.StopAssetEditing();
			//アセットのセーブ
			AssetDatabase.SaveAssets();
			//いったんアセットをリフレッシュ
			AssetDatabase.Refresh();
		}

		//テンプレートからコピーしたアセットの
		Dictionary<Object, Object> cloneAssetPair = new Dictionary<Object, Object>();
		//アセットの再設定
		void RebuildAssetsSub()
		{
			cloneAssetPair = new Dictionary<Object, Object>();
			//クローンしたアセットにパッキングタグなどの必要な処置をして
			//テンプレートのアセットとのペアでリスト化する
			List<string> pathList = UtageEditorToolKit.GetAllAssetPathList(newPath);
			foreach (string assetpath in pathList)
			{
				if (Path.GetExtension(assetpath) == ".unity") continue;
				//クローン元のアセット
				string oldPath = FilePathUtil.Format( assetpath).Replace(newPath + "/", srcPath + "/");
				Object mainAsset = AssetDatabase.LoadMainAssetAtPath(assetpath);
				if (WrapperUnityVersion.CheckPrefabAsset(mainAsset))
				{
					Object oldAsset = AssetDatabase.LoadMainAssetAtPath(oldPath);
					cloneAssetPair.Add(oldAsset, mainAsset);
				}
				else
				{
					foreach (Object clone in AssetDatabase.LoadAllAssetsAtPath(assetpath))
					{
						Object oldAsset = AssetDatabase.LoadAssetAtPath(oldPath, clone.GetType());
						if (oldAsset != null)
						{
							if (cloneAssetPair.ContainsKey(oldAsset))
							{
								Debug.LogWarning(oldPath + " is already contains");
							}
							else
							{
								cloneAssetPair.Add(oldAsset, clone);
							}
						}
					}
				}
			}

			float progress = 0.1f;
			EditorUtility.DisplayProgressBar("DeepDuplicate", "", progress);

			float delataProgress = (1-progress)/ cloneAssetPair.Values.Count;
			//クローンしたプレハブやScriptableObject内にテンプレートアセットへのリンクがあったら、クローンアセットのものに変える
			foreach (Object obj in cloneAssetPair.Values)
			{
				EditorUtility.DisplayProgressBar("DeepDuplicate ", obj.name, progress);
				bool isReplaced = false;
				if (WrapperUnityVersion.CheckPrefabAsset(obj))
				{
					UtageEditorToolKit.ReplaceSerializedPropertiesAllComponents(obj as GameObject, cloneAssetPair);
				}
				else
				{
					UtageEditorToolKit.ReplaceSerializedProperties(new SerializedObject(obj), cloneAssetPair);
				}
				if (isReplaced)
				{
					EditorUtility.SetDirty(obj);
				}
				progress += delataProgress;
			}
		}
	}
}