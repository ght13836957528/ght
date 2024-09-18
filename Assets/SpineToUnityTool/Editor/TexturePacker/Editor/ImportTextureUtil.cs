using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_4_0_0 ||UNITY_4_0 || UNITY_4_0_1||UNITY_4_1||UNITY_4_2||UNITY_4_3||UNITY_4_4||UNITY_4_5||UNITY_4_6||UNITY_4_7||UNITY_4_8||UNITY_4_9
namespace OnePStudio.SpineToUnity4
#else
namespace OnePStudio.SpineToUnity5
#endif
{
	public class ImportTextureUtil
	{
		public static Texture2D MakeTex(int width, int height, Color col)
		{
			Color[] pix = new Color[width*height];
			
			for(int i = 0; i < pix.Length; i++)
				pix[i] = col;
			
			Texture2D result = new Texture2D(width, height);
			result.SetPixels(pix);
			result.Apply();
			return result;
		}
		public static Texture2D[] MaxImportSettings(Texture2D[] imgs)
		{
			for(int s = 0; s < imgs.Length; s++)
			{
				if(AssetDatabase.GetAssetPath( (Texture2D)imgs[s]) != null)
				{
					TextureImporter tempImporter = TextureImporter.GetAtPath( AssetDatabase.GetAssetPath( (Texture2D)imgs[s]) ) as TextureImporter;
					tempImporter.isReadable = true;
					tempImporter.textureFormat = TextureImporterFormat.ARGB32;
					tempImporter.npotScale = TextureImporterNPOTScale.None;
					tempImporter.textureType = TextureImporterType.GUI;
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath((Texture2D)imgs[s]), ImportAssetOptions.ForceUpdate);
				}
			}
			return(imgs);
		}
		public static Texture2D MaxImportSettings(Texture2D img)
		{
			if(AssetDatabase.GetAssetPath( (Texture2D)img) != null)
			{
				TextureImporter tempImporter = TextureImporter.GetAtPath( AssetDatabase.GetAssetPath( (Texture2D)img) ) as TextureImporter;
				tempImporter.isReadable = true;
				tempImporter.textureFormat = TextureImporterFormat.ARGB32;
				tempImporter.npotScale = TextureImporterNPOTScale.None;
				tempImporter.textureType = TextureImporterType.Sprite;
				tempImporter.maxTextureSize=8192;
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath((Texture2D)img), ImportAssetOptions.ForceUpdate);
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath((Texture2D)img), ImportAssetOptions.ForceUpdate);
			}
			
			return(img);
		}
		public static Texture2D ReadAndUnScale(Texture2D img)
		{
			if(AssetDatabase.GetAssetPath( (Texture2D)img) != null)
			{
				TextureImporter tempImporter = TextureImporter.GetAtPath( AssetDatabase.GetAssetPath( (Texture2D)img) ) as TextureImporter;
				tempImporter.isReadable = true;
				tempImporter.textureFormat = TextureImporterFormat.ARGB32;
				tempImporter.npotScale = TextureImporterNPOTScale.None;
				tempImporter.maxTextureSize=8192;
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath((Texture2D)img), ImportAssetOptions.ForceUpdate);
				AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
			}
			
			return(img);
		}

		public static Texture2D MakeReadable(Texture2D img)
		{
			if(AssetDatabase.GetAssetPath( (Texture2D)img) != null)
			{
				TextureImporter tempImporter = TextureImporter.GetAtPath( AssetDatabase.GetAssetPath( (Texture2D)img) ) as TextureImporter;
				tempImporter.isReadable = true;
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath((Texture2D)img), ImportAssetOptions.ForceUpdate);
			}
			
			return(img);
		}
	}
}