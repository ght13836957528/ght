using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEditor.U2D;
using UnityEngine.U2D;
using System;
using System.Security.Cryptography;
using Object = UnityEngine.Object;

namespace Tools
{
	public class ArtistTools
	{
		static string mainPath = "Assets/Main";
		static string artsPath = "Assets/Art";

		// ----------------------------------------------- UI ----------------------------------------------------------
		// 走热更图片
		static string hotSpritesPath = "Assets/Main/ArtsDyre/Sprites";

		// 包体内的图片
        static string spritesPath = "Assets/Main/Arts/Sprites";
        static string originPath = "Assets/Main/Arts/Sprites/Origin";
		static string texturePath = "Assets/Main/Arts/Sprites/Texture";
		static string hotUpdatePath = "Assets/Main/Arts/Sprites/HotUpdate";
		static string mainArtsPath = "Assets/Main/Arts";

		static string[] atlasPaths = new string[] { originPath/*, hotUpdatePath*/ };

		// ----------------------------------------------- 角色 ---------------------------------------------------------
		static string rolePath = "Assets/Main/Arts/Role";
		static string highModelPath = "Assets/Main/Arts/HighModel";

		// ----------------------------------------------- 场景 ---------------------------------------------------------
		static string scenePath = "Assets/Main/Arts/Scene";
		static string battleScenePath = "Assets/Main/Arts/Scene/BattleScene";

		// ----------------------------------------------- 特效 ---------------------------------------------------------
		static string effectPath = "Assets/Main/Arts/Effects";

		// ----------------------------------------------- 大世界 -------------------------------------------------------
		static string greateWorldPath = "Assets/Main/Arts/World";

		// ----------------------------------------------- Shadow ------------------------------------------------------
		static string shadowPath = "Assets/Main/Arts/Role/Common/Shadow";

		//------------------------------------------------音效-----------------------------------------------------------
		static string audioPath = "Assets/Main/Sound";

		//------------------------------------------------其他-----------------------------------------------------------
		public const string BundleVariantName = "bundle";
		public const string SpriteAtlasSuffixFormat = ".spriteatlas";

		static int textureSize = 512;

		#region 下拉菜单指令

		#region 打印资源安卓ios 不一致

		[MenuItem("Assets/打印资源安卓ios 不一致")]
		static void CheckAssetData()
		{
			int index = 0;
			var audios = AssetDatabase.FindAssets("t:Texture", new string[] { artsPath });
			string assetPath;
			foreach (var guid in audios)
			{
				EditorUtility.DisplayProgressBar("正在检查",
						string.Format("已完成：{0}/{1}", index + 1, audios.Length), 1.0f * (index + 1) / audios.Length);

				index++;

				assetPath = AssetDatabase.GUIDToAssetPath(guid);
				if (string.IsNullOrEmpty(assetPath))
					continue;
				AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
				if (assetImporter == null)
					continue;
				TextureImporter src = assetImporter as TextureImporter;
				if (src == null)
				{
					Debug.Log("资源TextureImporter==null ：" + assetPath);
					continue;
				}
					
				TextureImporterPlatformSettings androidPlatformSetting = src.GetPlatformTextureSettings("Android");
				TextureImporterPlatformSettings iosPlatformSetting = src.GetPlatformTextureSettings("iPhone");
				bool isDifferent = false;
				if (androidPlatformSetting.overridden != iosPlatformSetting.overridden)
				{
					isDifferent = true;
					//Debug.LogError("资源设置不一致 ："+ assetPath);
				}
				if (androidPlatformSetting.maxTextureSize != iosPlatformSetting.maxTextureSize)
				{
					isDifferent = true;
				}

				if (androidPlatformSetting.format != iosPlatformSetting.format)
				{
					isDifferent = true;
				}

				if (isDifferent)
				{
					iosPlatformSetting.maxTextureSize = androidPlatformSetting.maxTextureSize;
					iosPlatformSetting.format = androidPlatformSetting.format;
					iosPlatformSetting.compressionQuality = androidPlatformSetting.compressionQuality;
					Debug.LogError("资源设置不一致 ：" + assetPath);
					src.SetPlatformTextureSettings(iosPlatformSetting);
					AssetDatabase.WriteImportSettingsIfDirty(src.assetPath);

					if (src.importSettingsMissing)
					{
						src.SaveAndReimport();
					}
				}
					


			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.ClearProgressBar();
		}

        #endregion

        #region 打印不符合规范的大纹理

        static Dictionary<string, TextureInfo> textureList = new Dictionary<string, TextureInfo>();

        [MenuItem("Tools/美术工具/打印不符合规范的大纹理(宽高非4的倍数)", false, 1)]
        static void PrintInvalidTextureSize()
        {
            textureList.Clear();

            string[] searchPaths = new string[] { spritesPath };
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", searchPaths);
            for (int i = 0; i < guids.Length; i++)
            {
                if (EditorUtility.DisplayCancelableProgressBar("正在检查超大纹理",
                        string.Format("已完成：{0}/{1}", i + 1, guids.Length), 1.0f * (i + 1) / guids.Length))
                    break;

                var assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                var obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                Texture2D tex = obj as Texture2D;
                if (null != tex && (tex.height % 4 > 0 || tex.width % 4 > 0))
                    textureList.Add(assetPath, new TextureInfo(assetPath, tex.width, tex.height));
            }

            SaveTextureInfo();

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("提示", "处理完毕,请在D:/output/Texture目录下查看!", "OK");
        }

        private static void SaveTextureInfo()
        {
            string outputFile = string.Format("D:/output/Texture/TextureInfos_{0}.txt", GenerateTimeString());

            if (!Directory.Exists("D:/output"))
                Directory.CreateDirectory("D:/output");

            string dir = Path.GetDirectoryName(outputFile);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(outputFile))
                File.Delete(outputFile);

            if (textureList.Count <= 0)
                return;

            using (FileStream fileStream = new FileStream(outputFile, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    string line = string.Empty;
                    foreach (var item in textureList)
                    {
                        line = item.Value.GetString();

                        writer.WriteLine(line);
                    }

                    writer.Flush();
                    writer.Close();
                    writer.Dispose();
                }

                fileStream.Close();
                fileStream.Dispose();
            }
        }

		#endregion

		#region 查看最近打开过的UI

		/*
		[MenuItem("Tools/查看最近打开过的UI", false, 1)]
		private static void PrintRecentlyOpenedUI()
        {
			if(!Application.isPlaying)
            {
				EditorUtility.DisplayDialog("提示", "请启动游戏后查看!", "OK");
				return;
            }

			var ui = GameEntry.UI.CacheUINames;
			List<UnityEngine.Object> selectedObjects = new List<UnityEngine.Object>();

			StringBuilder uiNames = new StringBuilder();
			for (int i = 0; i < ui.Count; i++)
            {
				uiNames.Append(ui[i]);

				var objectUI = AssetDatabase.LoadMainAssetAtPath(ui[i]);
				if(null != objectUI)
					selectedObjects.Add(objectUI);

				if (i < ui.Count - 1)
                    uiNames.Append(Environment.NewLine);
            }

			Selection.objects = selectedObjects.ToArray();

			EditorUtility.DisplayDialog("提示", $"最近打开过的UI如下:\r\n{uiNames.ToString()}", "OK");
		}
		*/

		#endregion

		#region 批量设置资源的bundle

		static string[] prefabsPath = new string[] { "Assets/Main/PackedAssets", "Assets/Main/Prefabs", "Assets/Art" };

        [MenuItem("Tools/美术工具/批量设置所有Prefab的AssetBundle", false, 2)]
        static void BatchSettingResourceAssetBundles()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", prefabsPath);
            for (int i = 0; i < guids.Length; i++)
            {
                if (EditorUtility.DisplayCancelableProgressBar("正在设置Prefab文件的AssetBundle...", string.Format("已完成：{0}/{1}", i + 1, guids.Length), 1.0f * (i + 1) / guids.Length))
                    return;

				string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
				if (string.IsNullOrEmpty(assetPath))
					continue;

				AssetBundleUtility.SetAssetBundleName(assetPath, AssetBundleUtility.AssetBundleNameType.FileWithoutExtension);
			}

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("提示", "处理完毕!", "OK");
        }

		#endregion

		#region 批量设置Main下所有贴图格式

		[MenuItem("Tools/美术工具/批量设置所有贴图格式及Bundle", false, 3)]
        private static void BatchSetTexturesSettings()
        {
            if (EditorUtility.DisplayDialog("提示", "是否设置Main下所有贴图格式", "确认", "取消"))
            {
				string[] paths = new string[] { spritesPath, artsPath };

				// 设置UI图片Bundle
				SetTextureSettingsByFolder(paths);

                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("提示", "处理完毕", "OK");
            }
        }

        #endregion

        #region 批量设置UI图片的图集、Bundle及压缩格式等信息

        [MenuItem("Tools/美术工具/批量设置UI图片的图集、Bundle及压缩格式等信息", false, 4)]
        private static void BatchSetSpritesSettings()
        {
            if (EditorUtility.DisplayDialog("提示", "是否设置UI图片格式以及Bundle", "确认", "取消"))
            {
                string[] paths = new string[] { spritesPath, hotUpdatePath, hotSpritesPath };
                // 设置UI图片Bundle
                SetTextureSettingsByFolder(paths);

                // 创建图集
                CreateSpriteAtlas();

                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("提示", "处理完毕", "OK");
            }
        }

        static void SetTextureSettingsByFolder(string[] path)
        {
            int index = 0;
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", path);
            foreach (var guid in guids)
            {
                if (EditorUtility.DisplayCancelableProgressBar("正在设置纹理的Bundle和压缩格式",
                        string.Format("已完成：{0}/{1}", index + 1, guids.Length), 1.0f * (index + 1) / guids.Length))
                    break;

                index++;

                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(assetPath))
                    continue;

                // 设置纹理的配置信息
                SetTextureSettings(assetPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static void CreateSpriteAtlas()
        {
            for (int i = 0; i < atlasPaths.Length; i++)
            {
                if (EditorUtility.DisplayCancelableProgressBar("正在为纹理创建图集",
                        string.Format("已完成：{0}/{1}", i + 1, atlasPaths.Length), 1.0f * (i + 1) / atlasPaths.Length))
                    break;

                var rootPath = atlasPaths[i];
				if (!Directory.Exists(rootPath))
					continue;

				TraverseSpriteFolder(rootPath);
            }
        }

		private static void TraverseSpriteFolder(string rootPath)
        {
			if (string.IsNullOrEmpty(rootPath))
				return;

            var direction = new DirectoryInfo(rootPath);
            var fileInfo = direction.GetFiles("*.png");
            if (null != fileInfo && fileInfo.Length > 0)
            {
                CreateSpriteAtlas(GetAssetPath(direction.FullName));
            }

            var directs = direction.GetDirectories();
            if (null != directs && directs.Length > 0)
            {
                for (int i = 0; i < directs.Length; i++)
                {
                    TraverseSpriteFolder(directs[i].FullName);
                }
            }
        }


		private static void CreateSpriteAtlas(string path)
        {
            bool isNewAtlas = true;
            SpriteAtlas spriteAtlas = null;

            //创建图集
            string atlas = path + SpriteAtlasSuffixFormat;
            isNewAtlas = !File.Exists(atlas);
            if (!isNewAtlas)
                spriteAtlas = AssetDatabase.LoadMainAssetAtPath(atlas) as SpriteAtlas;
            else
                spriteAtlas = new SpriteAtlas();

            if (null == spriteAtlas)
                return;

            SpriteAtlasPackingSettings packSet = spriteAtlas.GetPackingSettings();
            packSet.blockOffset = 1;
            packSet.enableRotation = false;
            packSet.enableTightPacking = false;
            packSet.padding = 4;
            spriteAtlas.SetPackingSettings(packSet);

            SpriteAtlasTextureSettings textureSet = spriteAtlas.GetTextureSettings();
            textureSet.readable = false;
            textureSet.generateMipMaps = false;
            textureSet.sRGB = true;
            textureSet.filterMode = FilterMode.Bilinear;
            spriteAtlas.SetTextureSettings(textureSet);

            TextureImporterPlatformSettings androidPlatformSetting = spriteAtlas.GetPlatformSettings("Android");
            androidPlatformSetting.overridden = true;
            androidPlatformSetting.maxTextureSize = 2048;
            androidPlatformSetting.format = TextureImporterFormat.ASTC_6x6;
            androidPlatformSetting.crunchedCompression = true;
            androidPlatformSetting.textureCompression = TextureImporterCompression.Compressed;
            androidPlatformSetting.compressionQuality = (int)UnityEditor.TextureCompressionQuality.Best;
            spriteAtlas.SetPlatformSettings(androidPlatformSetting);

            TextureImporterPlatformSettings iosPlatformSetting = spriteAtlas.GetPlatformSettings("iPhone");
            iosPlatformSetting.overridden = true;
            iosPlatformSetting.maxTextureSize = 2048;
            iosPlatformSetting.format = TextureImporterFormat.ASTC_6x6;
            iosPlatformSetting.crunchedCompression = true;
            iosPlatformSetting.textureCompression = TextureImporterCompression.Compressed;
            iosPlatformSetting.compressionQuality = (int)UnityEditor.TextureCompressionQuality.Best;
            spriteAtlas.SetPlatformSettings(iosPlatformSetting);

            if (isNewAtlas)
            {
                AssetDatabase.CreateAsset(spriteAtlas, atlas);

                // 1、添加文件
                // 这里我使用的是png图片，已经生成Sprite精灵了
                //DirectoryInfo dir = new DirectoryInfo(path);
                //FileInfo[] files = dir.GetFiles("*.png");
                //foreach (FileInfo file in files)
                //{
                //    spriteAtlas.Add(new[] { AssetDatabase.LoadAssetAtPath<Sprite>($"{path}/{file.Name}") });
                //}

                // 2、添加文件夹
                Object obj = AssetDatabase.LoadMainAssetAtPath(path);
                spriteAtlas.Add(new[] { obj });
            }

            // 3、设置Asset
            AssetImporter atlasAssetImporter = AssetImporter.GetAtPath(atlas);
            atlasAssetImporter.assetBundleName = path.Replace("\\", "/");
            atlasAssetImporter.assetBundleVariant = BundleVariantName;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

		#endregion

		#region 处理包体内重复的老图片(保留引用关系,删除多余的图片)

		private static string originSpritesPath = "Assets/Main/Arts/Sprites/Origin/Sprites";

		// 包体内图片
        private static string originCommonPath = "Assets/Main/Arts/Sprites/Origin/Common";
        private static string originExpackPath = "Assets/Main/Arts/Sprites/Origin/Expack";
        private static string originImperialPath = "Assets/Main/Arts/Sprites/Origin/Imperial";
        private static string originLoadingPath = "Assets/Main/Arts/Sprites/Origin/Loading";
        private static string originWorldPath = "Assets/Main/Arts/Sprites/Origin/World";
        //private static string originWorld3DPath = "Assets/Main/Arts/Sprites/Origin/World3D";

		private static string[] targetSpritePath = new string[] 
		{
			originCommonPath, originExpackPath, originImperialPath, 
			originLoadingPath, originWorldPath,
			hotSpritesPath
		};

		private static Dictionary<string, SpriteInfo> repeatSprites = new Dictionary<string, SpriteInfo>();

		[MenuItem("Tools/美术工具/批量处理重复图片(保留引用关系,移动目录并删除多余的图片)", false, 5)]
        private static void BatchRepeatSprites()
        {
			// 1.扫描源文件夹的图片
			GetOriginSprites();

			// 2.扫描目标文件夹的图片
			GetRepeatSprites();

			// 3.重复图片直接覆盖cocos目录下的图片然后删除原图片
			HandleRepeatTexture();

			// 4.手动分析并处理都被引用的图片

			// 5.保存修改
			AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("提示", "处理完毕", "OK");
        }

        private static void HandleRepeatTexture()
        {
			int total = repeatSprites.Count;
			int index = 0;

			SetLogFile("RepeatSprite");

			foreach (var iter in repeatSprites)
            {
				index++;

				if (EditorUtility.DisplayCancelableProgressBar("正在处理重复图片...",
                        string.Format("已完成：{0}/{1}", index, total), 1.0f * (index + 1) / total))
                    break;

                var spriteInfo = iter.Value;
				if (null == spriteInfo)
					continue;

				if (spriteInfo.targetSpriteItemList.Count <= 0)
					continue;

				var originSpriteList = spriteInfo.originSpriteItemList;
				var targetSpriteList = spriteInfo.targetSpriteItemList;
				for (int i = 0; i < targetSpriteList.Count; i++)
                {
					var targetSpriteMD5 = targetSpriteList[i].GetSpriteMD5();
					for(int j = 0; j < originSpriteList.Count; j++)
                    {
						var originSpriteMD5 = originSpriteList[j].GetSpriteMD5();
						if(targetSpriteMD5 == originSpriteMD5)
                        {
							if(!originSpriteList[j].HasDependency())
                            {
								AssetDatabase.DeleteAsset(originSpriteList[j].GetAssetPath());
                            }
                            else if (!targetSpriteList[i].HasDependency())
                            {
                                if (targetSpriteList[i].ReplaceGuid(originSpriteList[j].GetSpriteGuid()))
                                    AssetDatabase.DeleteAsset(originSpriteList[j].GetAssetPath());
                            }
                            else
                            {
								Print($"{originSpriteList[j].GetAssetPath()}\t{targetSpriteList[i].GetAssetPath()}");
							}
                        }
					}
				}
            }

            LogFlush();
        }

        private static void GetOriginSprites()
        {
            //TraverseOriginSprites("Assets/Main/Arts/Sprites/Origin/Sprites/UI/UIFirstDayActivity");
            //TraverseOriginSprites("Assets/Main/Arts/Sprites/Origin/Sprites/WorldBuild");
            TraverseOriginSprites(originSpritesPath);
            //TraverseOriginSprites(hotUpdatePath);
        }

        private static void TraverseOriginSprites(string spritePath)
        {
            if (!Directory.Exists(spritePath))
                return;

            var directoryInfo = new DirectoryInfo(spritePath);
            var fileInfos = directoryInfo.GetFiles("*.png");
			if(null != fileInfos && fileInfos.Length > 0)
            {
                foreach (var item in fileInfos)
                {
                    string spriteName = Path.GetFileNameWithoutExtension(item.Name);
                    if (!repeatSprites.ContainsKey(spriteName))
						repeatSprites.Add(spriteName, new SpriteInfo(item.FullName));
                    else
						repeatSprites[spriteName].AddOriginSprite(item.FullName.Trim());
                }
            }

            DirectoryInfo[] subDirectoryInfos = directoryInfo.GetDirectories();
			if(null != subDirectoryInfos && subDirectoryInfos.Length > 0)
            {
                for (int i = 0; i < subDirectoryInfos.Length; i++)
                {
					TraverseOriginSprites(subDirectoryInfos[i].FullName);
                }
            }
        }

        private static void GetRepeatSprites()
        {
			for(int i = 0; i < targetSpritePath.Length; i++)
            {
				TraverseTargetSprites(targetSpritePath[i]);
			}
        }

        private static void TraverseTargetSprites(string spritePath)
        {
			if (!Directory.Exists(spritePath))
				return;

			var directoryInfo = new DirectoryInfo(spritePath);
			var fileInfos = directoryInfo.GetFiles("*.png");
			if (null != fileInfos && fileInfos.Length > 0)
			{
				foreach (var item in fileInfos)
				{
					string spriteName = Path.GetFileNameWithoutExtension(item.Name);
					if (!repeatSprites.ContainsKey(spriteName))
						continue;
					
					repeatSprites[spriteName].AddTargetSprite(item.FullName);
				}
			}

			DirectoryInfo[] subDirectoryInfos = directoryInfo.GetDirectories();
			if (null != subDirectoryInfos && subDirectoryInfos.Length > 0)
			{
				for (int i = 0; i < subDirectoryInfos.Length; i++)
				{
					TraverseTargetSprites(subDirectoryInfos[i].FullName);
				}
			}
		}

		#region 辅助类

		public class SpriteInfo
        {
            // 图片名称
            private string spriteName;

			public List<SpriteItem> originSpriteItemList = new List<SpriteItem>();
			public List<SpriteItem> targetSpriteItemList = new List<SpriteItem>();

			public SpriteInfo(string fileFullName)
            {
                spriteName = Path.GetFileNameWithoutExtension(fileFullName);
				
				this.AddOriginSprite(fileFullName);
			}

            public void AddOriginSprite(string fileFullName)
            {
                if (originSpriteItemList.Count > 0)
                {
                    var find = originSpriteItemList.Find(o => o.GetSpriteFile() == fileFullName);
                    if (null == find)
                        originSpriteItemList.Add(new SpriteItem(fileFullName));
                }
                else
                {
                    originSpriteItemList.Add(new SpriteItem(fileFullName));
                }
            }

            public void AddTargetSprite(string fileFullName)
            {
                if (targetSpriteItemList.Count > 0)
                {
                    var find = targetSpriteItemList.Find(o => o.GetSpriteFile() == fileFullName);
                    if (null == find)
						targetSpriteItemList.Add(new SpriteItem(fileFullName));
                }
                else
                {
					targetSpriteItemList.Add(new SpriteItem(fileFullName));
                }
            }

            public string GetSpriteName()
            {
                return this.spriteName;
            }
        }

        public class SpriteItem
        {
            private string spriteFile;
            private string spriteGuid;
			private string fileMd5 = string.Empty;

            private string guidFlag = "guid:";

            public SpriteItem(string fileName)
            {
                this.spriteFile = fileName;
            }

			public bool HasDependency()
            {
				string guid = this.GetSpriteGuid();
				var assetDescription = ReferenceFinderWindow.GetReferenceData(guid);
				var references = assetDescription.references;
				if (null == references || references.Count <= 0)
					return false;

				for(int i = 0; i < references.Count; i++)
                {
					string path = AssetDatabase.GUIDToAssetPath(references[i]);
                    if (this.GetAssetPath() != path && !path.EndsWith(".spriteatlas"))
                        return true;
                }

                return false;
            }

			public bool ReplaceGuid(string targetGuid)
            {
                try
                {
                    string metaFile = this.spriteFile + ".meta";
                    if (!File.Exists(metaFile))
                        return false;

                    byte[] buffers = File.ReadAllBytes(metaFile.Trim());
                    string content = Encoding.UTF8.GetString(buffers);

					var guid = GetSpriteGuid();
					if (guid == targetGuid)
						return true;

					// 替换guid
					content = content.Replace(guid, targetGuid);

                    // 获得要写入文件的字节buffer;
                    byte[] replaceContent = Encoding.UTF8.GetBytes(content);

                    // 创建并写入文件;
                    FileStream fileWrite = new FileStream(metaFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    fileWrite.Write(replaceContent, 0, replaceContent.Length);
                    fileWrite.Flush();
                    fileWrite.Close();
                    fileWrite.Dispose();

                }
                catch (Exception ex)
                {
                    Debug.LogErrorFormat("get sprite's MD5 fail, file:{0}, error:{1}", this.spriteFile, ex.Message);

					return false;
                }

				return true;
            }

            public string GetSpriteFile()
            {
                return this.spriteFile;
            }

			public string GetSpriteName()
            {
				return Path.GetFileNameWithoutExtension(this.spriteFile);
            }

			public string GetAssetPath()
            {
				if (this.spriteFile.StartsWith("Assets"))
					return this.spriteFile;

				int index = this.spriteFile.IndexOf("Assets");
				if (index >= 0)
					return this.spriteFile.Substring(index).Replace("\\", "/");

				return this.spriteFile;
			}

            public string GetSpriteGuid()
            {
                if (string.IsNullOrEmpty(this.spriteGuid))
                {
                    string metaFile = this.spriteFile + ".meta";
                    if (!File.Exists(metaFile))
                        return null;

                    try
                    {
                        using (FileStream fileStream = new FileStream(metaFile, FileMode.Open))
                        {
                            using (StreamReader reader = new StreamReader(fileStream))
                            {
                                string line;
                                while (!reader.EndOfStream)
                                {
                                    line = reader.ReadLine();

                                    if (!line.Contains(this.guidFlag))
                                        continue;

                                    this.spriteGuid = line.Substring(line.IndexOf(this.guidFlag) + this.guidFlag.Length).Trim();

                                    break;
                                }

                                reader.Close();
                                reader.Dispose();
                            }

                            fileStream.Close();
                            fileStream.Dispose();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogErrorFormat("Read file throw exception,error:{0},file:{1}", e.Message, metaFile);

                        return null;
                    }
                }

                return this.spriteGuid;
            }

            /// <summary>
            /// 获取文件的MD5码
            /// </summary>
            /// <returns></returns>
            public string GetSpriteMD5()
            {
                try
                {
					if(string.IsNullOrEmpty(fileMd5))
                    {
                        FileStream file = new FileStream(this.spriteFile, FileMode.Open);
                        MD5 md5 = new MD5CryptoServiceProvider();
                        byte[] retVal = md5.ComputeHash(file);
                        file.Close();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < retVal.Length; i++)
                            sb.Append(retVal[i].ToString("x2"));

                        fileMd5 = sb.ToString();
                    }

					return fileMd5;

				}
                catch (Exception ex)
                {
                    Debug.LogErrorFormat("get sprite's MD5 fail, file:{0}, error:{1}", this.spriteFile, ex.Message);
                }

                return null;
            }

			public void Delete()
            {
				var assetPath = this.GetAssetPath();
				if (!string.IsNullOrEmpty(assetPath))
					AssetDatabase.DeleteAsset(assetPath);
            }

			public bool IsImportPath()
            {
                var assetPath = this.GetAssetPath();
				if (string.IsNullOrEmpty(assetPath))
					return false;

				if (assetPath.Contains("Assets/Main/Arts/Sprites/Origin/Sprites") 
					|| assetPath.Contains("Assets/Main/Arts/Sprites/HotUpdate"))
					return false;

				return true;
            }
        }

        #endregion

        #region 处理日志

        private static StreamWriter sw = null;

        private static void SetLogFile(string file = null)
        {
            string logPath = Path.Combine(Application.dataPath, "Output");

            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);

            logPath = Path.Combine(logPath, "Resource");

            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);

            string logFileName = string.Format("{0}_{1}.log", string.IsNullOrEmpty(file) ? "handle_image" : file, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-ms"));
            string logFile = Path.Combine(logPath, logFileName);

            if (File.Exists(logFile))
                File.Delete(logFile);

            sw = new StreamWriter(logFile, false, Encoding.UTF8);
        }

        private static void LogFlush()
        {
            if (null != sw)
            {
                sw.Flush();
                sw.Close();
                sw = null;
            }
        }

        private static void Print(string output)
        {
            if (null == sw)
                SetLogFile("RepeatSprite");

            sw.WriteLine(output);
        }

		#endregion

		#endregion

		#region 删除重复的无引用图片

		private static string[] handleSpritePath = new string[] 
		{ 
			spritesPath, hotSpritesPath
			/*"Assets/Main/Arts/Sprites/Origin/Common"*/
		};

        private static Dictionary<string, SpriteItem> sprites = new Dictionary<string, SpriteItem>();

		[MenuItem("Tools/美术工具/批量删除无引用关系的重复图片", false, 6)]
        private static void DeleteRepeatSprites()
        {
			SetLogFile("DeleteSprite");

			sprites.Clear();

			for (int i = 0; i < handleSpritePath.Length; i++)
            {
				TraverseSpritePath(handleSpritePath[i]);
			}

			LogFlush();

			EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("提示", "处理完毕", "OK");
        }

        private static void TraverseSpritePath(string spritePath)
        {
            if (!Directory.Exists(spritePath))
                return;

            var directoryInfo = new DirectoryInfo(spritePath);
            var fileInfos = directoryInfo.GetFiles("*.png");
            if (null != fileInfos && fileInfos.Length > 0)
            {
                foreach (var item in fileInfos)
                {
					var spriteItem = new SpriteItem(item.FullName);
					var md5 = spriteItem.GetSpriteMD5();
					if (sprites.ContainsKey(md5))
                    {
						if (sprites[md5].GetSpriteName() != spriteItem.GetSpriteName())
							continue;

                        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(spriteItem.GetAssetPath());
						if (null != tex && tex.height <= 100 && tex.width <= 100)
							continue;

                        var hasDependencyA = spriteItem.HasDependency();
						var hasDependencyB = sprites[md5].HasDependency();
						if(!hasDependencyA && !hasDependencyB)
                        {
							if (!spriteItem.IsImportPath())
                            {
								spriteItem.Delete();

								Print($"Repeat reference image => {sprites[md5].GetAssetPath()}, {spriteItem.GetAssetPath()}, have delete.");
							}
							else
                            {
								sprites[md5].Delete();

								Print($"Repeat reference image => {sprites[md5].GetAssetPath()}, {spriteItem.GetAssetPath()}, not delete.");

								sprites[md5] = spriteItem;
							}
						}
						else
                        {
                            if (!hasDependencyA)
                            {
                                spriteItem.Delete();

                                Print($"Repeat reference image => {sprites[md5].GetAssetPath()}, {spriteItem.GetAssetPath()}, have delete.");
                            }
                            else
                            {
                                if (!hasDependencyB)
                                {
                                    sprites[md5].Delete();
                                    Print($"Repeat reference image => {spriteItem.GetAssetPath()}, {sprites[md5].GetAssetPath()}, have delete.");
                                }
                                else
                                {
                                    Print($"Repeat reference image => {sprites[md5].GetAssetPath()}, {spriteItem.GetAssetPath()}, not delete.");
                                }

                                sprites[md5] = spriteItem;
                            }
                        }
					}
					else
                    {
						sprites.Add(md5, spriteItem);
					}
                }
            }

            DirectoryInfo[] subDirectoryInfos = directoryInfo.GetDirectories();
            if (null != subDirectoryInfos && subDirectoryInfos.Length > 0)
            {
                for (int i = 0; i < subDirectoryInfos.Length; i++)
                {
					TraverseSpritePath(subDirectoryInfos[i].FullName);
                }
            }
        }

        #endregion

        #endregion

        #region 右键菜单指令

        #region 设置音效
        [MenuItem("Assets/美术工具/设置音效")]
		static void SetAudio()
		{

			int index = 0;
			var audios = AssetDatabase.FindAssets("t:AudioClip", new string[] { audioPath });
			string assetPath;
			foreach (var guid in audios)
			{
				EditorUtility.DisplayProgressBar("正在设置音效",
						string.Format("已完成：{0}/{1}", index + 1, audios.Length), 1.0f * (index + 1) / audios.Length);

				index++;

				assetPath = AssetDatabase.GUIDToAssetPath(guid);
				if (string.IsNullOrEmpty(assetPath))
					continue;
				AudioImporter src = (AudioImporter)AssetImporter.GetAtPath(assetPath);

				AudioSet(src, assetPath);
				


			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.ClearProgressBar();


		}

		static void AudioSet(AudioImporter audioImporter,string audioPath)
		{
			if (audioImporter == null)
				return;

			//关闭双声道
			audioImporter.forceToMono = true;
			//关闭预加载
			audioImporter.preloadAudioData = false;
			//异步加载
			audioImporter.loadInBackground = false;

			var audio = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath);
			float audioTime = audio.length;
			//Debug.Log("资源路径 " + audioPath + "音频时长" + audioTime);

			AudioImporterSampleSettings defaultImporterSampleSettings = audioImporter.defaultSampleSettings;

			//设置压缩
			if (audioTime < 2)
			{
				defaultImporterSampleSettings.compressionFormat = AudioCompressionFormat.ADPCM;
			}
			else if (audioTime < 10)
			{
				defaultImporterSampleSettings.compressionFormat = AudioCompressionFormat.Vorbis;
				defaultImporterSampleSettings.quality = 1;
			}
			else
			{
				defaultImporterSampleSettings.compressionFormat = AudioCompressionFormat.Vorbis;
				defaultImporterSampleSettings.quality = 0.7f;
			}

			//采样率设置
			defaultImporterSampleSettings.sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;

			//加载景来的方式
			defaultImporterSampleSettings.loadType = AudioClipLoadType.CompressedInMemory;

			audioImporter.defaultSampleSettings = defaultImporterSampleSettings;
			AudioImporterSampleSettings androidImporterSampleSettings = audioImporter.GetOverrideSampleSettings("Android");
			AudioImporterSampleSettings IOSImporterSampleSettings = audioImporter.GetOverrideSampleSettings("IOS");

			audioImporter.SaveAndReimport();
		}



		#endregion

        #region 设置动画压缩格式

        [MenuItem("Assets/美术工具/设置动画压缩格式")]
		private static void SetAnimCompress()
		{

			int index = 0;
			string[] guids = AssetDatabase.FindAssets("t:Model", new string[] { rolePath, highModelPath});
			foreach (var guid in guids)
			{
				EditorUtility.DisplayProgressBar("正在设置动画压缩格式",
						string.Format("已完成：{0}/{1}", index + 1, guids.Length), 1.0f * (index + 1) / guids.Length);

				index++;

				var assetPath = AssetDatabase.GUIDToAssetPath(guid);
				if (string.IsNullOrEmpty(assetPath))
					continue;
				ModelImporter src = (ModelImporter)AssetImporter.GetAtPath(assetPath);

				if (src == null)
					continue;

				if (src.animationCompression == ModelImporterAnimationCompression.Optimal)
					continue;



				src.animationCompression = ModelImporterAnimationCompression.Optimal;
				src.SaveAndReimport();


			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.ClearProgressBar();

		}
        #endregion

		#region 设置图片的Bundle及压缩格式

		[MenuItem("Assets/美术工具/设置图片的Bundle及压缩格式")]
		private static void SetSpritesSettings()
		{
			var guids = Selection.assetGUIDs;
			if (guids == null || guids.Length < 1)
			{
				EditorUtility.DisplayDialog("提示", "请选中包含纹理的文件或者文件夹!", "OK");
				return;
			}

			foreach (var guid in guids)
			{
				var assetPath = AssetDatabase.GUIDToAssetPath(guid);
				if (string.IsNullOrEmpty(assetPath))
					continue;

				if (Directory.Exists(assetPath))
				{
					// 如果是文件夹
					int index = 0;
					string[] subGuids = AssetDatabase.FindAssets("t:Texture2D", new string[] {assetPath});
					foreach (var subGuid in subGuids)
					{
						if (subGuids.Length > 10 && EditorUtility.DisplayCancelableProgressBar("正在设置纹理的Bundle和压缩格式",
							    string.Format("已完成：{0}/{1}", index + 1, subGuids.Length),
							    1.0f * (index + 1) / subGuids.Length))
							break;

						index++;

						var subAssetPath = AssetDatabase.GUIDToAssetPath(subGuid);
						if (string.IsNullOrEmpty(subAssetPath))
							continue;

						SetTextureSettings(subAssetPath);
					}
				}
				else
				{
					// 如果是文件
					SetTextureSettings(assetPath);
				}
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			EditorUtility.ClearProgressBar();
			EditorUtility.DisplayDialog("提示", "处理完毕", "OK");
		}

		#endregion

		#region 创建图集或者刷新图集的Bundle及ImportSetting信息

		[MenuItem("Assets/美术工具/创建图集或者刷新图集的Bundle及ImportSetting信息")]
		private static void CreateAtlasSettings()
		{
			var guids = Selection.assetGUIDs;
			if (guids == null || guids.Length < 1)
			{
				EditorUtility.DisplayDialog("提示", "请选中包含纹理的文件或者文件夹!", "OK");
				return;
			}

			int index = 0;
			int count = 0;
			foreach (var guid in guids)
			{
				if (guids.Length > 10 && EditorUtility.DisplayCancelableProgressBar("正在创建或者刷新图集",
					    string.Format("已完成：{0}/{1}", index + 1, guids.Length), 1.0f * (index + 1) / guids.Length))
					break;

				var assetPath = AssetDatabase.GUIDToAssetPath(guid);
				if (string.IsNullOrEmpty(assetPath))
					continue;

				if (!Directory.Exists(assetPath))
					continue;

				if (!assetPath.Contains(originPath))
					continue;

				CreateSpriteAtlas(assetPath);

				count++;
			}

			if (count <= 0)
			{
				EditorUtility.DisplayDialog("提示", "原因:\n 1.需选中包含纹理的文件或者文件夹,可多选! \n 2.需选中可打图集的文件夹!", "OK");
				return;
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			EditorUtility.ClearProgressBar();
			EditorUtility.DisplayDialog("提示", "处理完毕", "OK");
		}

        #endregion

        #endregion

        #region 公共接口

        public static void SetTextureSettings(string assetPath)
		{
			var texture = AssetDatabase.LoadMainAssetAtPath(assetPath) as Texture2D;
			if (null == texture)
				return;

			TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
			if (null == importer)
				return;

			bool changed = false;

            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
				changed = true;
			}

            // 设置Bundle信息
            if (assetPath.Contains(originPath))
			{
				changed = AssetBundleUtility.SetAssetBundleName(assetPath, AssetBundleUtility.AssetBundleNameType.Folder);
			}
			else
			{
				if (texture.width >= textureSize || texture.height >= textureSize)
				{
					// 按照文件名设置Bundle
					changed = AssetBundleUtility.SetAssetBundleName(assetPath, AssetBundleUtility.AssetBundleNameType.FileWithoutExtension);
				}
				else
				{
					// 按照文件夹设置Bundle
					changed = AssetBundleUtility.SetAssetBundleName(assetPath, AssetBundleUtility.AssetBundleNameType.Folder);
				}
			}

            if (importer.textureType != TextureImporterType.Sprite 
				&& !TextureIsPowerOfTwo(texture) && importer.npotScale == TextureImporterNPOTScale.None)
            {
                importer.npotScale = TextureImporterNPOTScale.ToNearest;
                changed = true;
            }

            if (SetCommonTextureSettings(importer))
                changed = true;

            if (changed)
				importer.SaveAndReimport();
		}

		public static bool SetCommonTextureSettings(TextureImporter importer)
		{
			bool changed = false;
			if (importer.mipmapEnabled)
			{
				importer.mipmapEnabled = false;
				changed = true;
			}

			if (SetCommonPlatformTextureSettings(importer, "Android", 2048))
				changed = true;

			if (SetCommonPlatformTextureSettings(importer, "iPhone", 2048))
				changed = true;

			return changed;
		}

		private static bool SetCommonPlatformTextureSettings(TextureImporter importer, string platform, int maxSize,
			TextureImporterFormat rgbFormat = TextureImporterFormat.ASTC_6x6,
			TextureImporterFormat rgbaFormat = TextureImporterFormat.ASTC_6x6)
		{
			bool changed = false;
			bool hasAlpha = importer.DoesSourceTextureHaveAlpha();

			TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings(platform);

			if (!platformSettings.overridden)
			{
				platformSettings.overridden = true;
				changed = true;
			}

			if (platformSettings.maxTextureSize > maxSize)
			{
				platformSettings.maxTextureSize = maxSize;
				changed = true;
			}

            //if (platformSettings.textureCompression != TextureImporterCompression.Compressed)
            //{
            //    platformSettings.textureCompression = TextureImporterCompression.Compressed;
            //    changed = true;
            //}

            if (platformSettings.compressionQuality != (int)UnityEditor.TextureCompressionQuality.Best)
            {
                platformSettings.compressionQuality = (int)UnityEditor.TextureCompressionQuality.Best;
                changed = true;
            }

            if (hasAlpha)
			{
				if (platformSettings.format != rgbaFormat)
				{
					platformSettings.format = rgbaFormat;
					changed = true;
				}
			}
			else
			{
				if (platformSettings.format != rgbFormat)
				{
					platformSettings.format = rgbFormat;
					changed = true;
				}
			}

			if (changed)
				importer.SetPlatformTextureSettings(platformSettings);

			return changed;
		}

		private static bool SetSpritesPlatformTextureSettings(TextureImporter textureImporter)
		{
			bool changed = false;
			// 设置压缩格式
			var androidSetting = textureImporter.GetPlatformTextureSettings("Android");
			// 是否有Alpha
			bool hasAlpha = textureImporter.DoesSourceTextureHaveAlpha();

			if(hasAlpha)
            {
                changed = (androidSetting.overridden != true
                                       || androidSetting.maxTextureSize != 2048
                                       || androidSetting.crunchedCompression != true
                                       || androidSetting.format != TextureImporterFormat.ASTC_6x6
									   || androidSetting.textureCompression != TextureImporterCompression.Compressed
                                       || androidSetting.compressionQuality != (int)UnityEditor.TextureCompressionQuality.Best);
            }
			else
            {
                changed = (androidSetting.overridden != true
                       || androidSetting.maxTextureSize != 2048
                       || androidSetting.crunchedCompression != true
                       || androidSetting.format != TextureImporterFormat.ASTC_6x6
					   || androidSetting.textureCompression != TextureImporterCompression.Compressed
                       || androidSetting.compressionQuality != (int)UnityEditor.TextureCompressionQuality.Best);
            }

			androidSetting.overridden = true;
			androidSetting.maxTextureSize = 2048;
			androidSetting.format = TextureImporterFormat.ASTC_6x6;
			androidSetting.crunchedCompression = true;
			androidSetting.textureCompression = TextureImporterCompression.Compressed;
			androidSetting.compressionQuality = (int) UnityEditor.TextureCompressionQuality.Best;
			textureImporter.SetPlatformTextureSettings(androidSetting);

			var iossettings = textureImporter.GetPlatformTextureSettings("iPhone");

			if (hasAlpha)
            {
                changed = (changed || iossettings.overridden != true
                               || iossettings.maxTextureSize != 2048
                               || iossettings.crunchedCompression != true
                               || iossettings.format != TextureImporterFormat.ASTC_6x6
							   || iossettings.textureCompression != TextureImporterCompression.Compressed
                               || iossettings.compressionQuality != (int)UnityEditor.TextureCompressionQuality.Best);
            }
			else
            {
                changed = (changed || iossettings.overridden != true
                               || iossettings.maxTextureSize != 2048
                               || iossettings.crunchedCompression != true
                               || iossettings.format != TextureImporterFormat.ASTC_6x6
							   || iossettings.textureCompression != TextureImporterCompression.Compressed
                               || iossettings.compressionQuality != (int)UnityEditor.TextureCompressionQuality.Best);
            }

			iossettings.overridden = true;
			iossettings.maxTextureSize = 2048;
			iossettings.format = TextureImporterFormat.ASTC_6x6;
			iossettings.crunchedCompression = true;
			iossettings.textureCompression = TextureImporterCompression.Compressed;
			iossettings.compressionQuality = (int) UnityEditor.TextureCompressionQuality.Best;
			textureImporter.SetPlatformTextureSettings(iossettings);

			return changed;
		}

		private static string GetAssetPath(string assetPath)
		{
			string assets = "Assets";
			if (!assetPath.StartsWith(assets))
				return assetPath.Substring(assetPath.IndexOf(assets));

			return assetPath;
		}

		private static string GenerateTimeString()
		{
			var dateString = System.DateTime.Now.Year.ToString() + "-" + System.DateTime.Now.Month.ToString() + "-" +
			                 System.DateTime.Now.Day.ToString();
			var timeString = System.DateTime.Now.ToLongTimeString().Replace(":", ".").Replace(" ", "");
			return dateString + "-" + timeString;
		}

		public static bool TextureIsPowerOfTwo(Texture tex)
		{
			if (tex == null)
				return true;
			if (isPowerOfTwo(tex.width) && isPowerOfTwo(tex.height))
				return true;
			return false;
		}

		private static bool isPowerOfTwo(int num)
		{
			return ((num & (num - 1)) == 0);
		}

		#endregion
	}

	#region 打印不符合规范的大纹理辅助类

	public class TextureInfo
	{
		public string texturePath;
		public int width;
		public int height;

		public TextureInfo(string _texturePath, int _width, int _height)
		{
			this.texturePath = _texturePath;
			this.width = _width;
			this.height = _height;
		}

		public string GetString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(this.texturePath);
			stringBuilder.Append("\t");
			stringBuilder.Append(this.width);
			stringBuilder.Append("\t");
			stringBuilder.Append(this.height);

			return stringBuilder.ToString();
		}
	}

	#endregion
}