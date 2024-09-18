using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#if UNITY_4_0_0 ||UNITY_4_0 || UNITY_4_0_1||UNITY_4_1||UNITY_4_2||UNITY_4_3||UNITY_4_4||UNITY_4_5||UNITY_4_6||UNITY_4_7||UNITY_4_8||UNITY_4_9
namespace OnePStudio.SpineToUnity4
#else
namespace OnePStudio.SpineToUnity5
#endif
{
	public class OptimizeSprite
	{
		[MenuItem("Assets/Tools/Spine2Unity/Sprite Texure Optimize")]
		static public void AutoOptimizeSprite()
		{
			Object[] selection = Selection.GetFiltered (typeof(Object), SelectionMode.DeepAssets);
			if(selection.Length>1)
			{
				EditorUtility.DisplayDialog("Error","Please just choose only one image texture Sprite","OK");
				return;  
			}
			Object obj=selection[0];
			if(obj is Texture2D)
			{
				Texture2D texture=(Texture2D)obj;
				string path=AssetDatabase.GetAssetPath(texture);

				TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
				if(ti.textureType==TextureImporterType.Sprite&&
				   ti.spriteImportMode==SpriteImportMode.Multiple)
				{
					OnProcessOptimizeSprite(texture);
				}
				else
				{
					EditorUtility.DisplayDialog("Error","Feature just optimize for Texture Sprite with mode Multiple, please choose correct type","OK");
				}

			}
			else
			{
				EditorUtility.DisplayDialog("Error","Please choose Texture type Sprite","OK");
				return;
			}
		}
		public static void OnProcessOptimizeSprite(Texture2D texture)
		{
			//int tryCount=0;
			//try
			//{
				List<SpriteElement> listSprite=new List<SpriteElement>();
				string path=AssetDatabase.GetAssetPath(texture);
				ImportTextureUtil.MaxImportSettings(texture);
				ImportTextureUtil.ReadAndUnScale(texture);
				TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
				//Debug.LogError(Pathfinding.Serialization.JsonFx.JsonWriter.Serialize(ti.spritesheet));
				//EditorUtility.DisplayDialog("Error","Do you want to override this texture (YES) to override, (NO) To create copy","YES","NO");
				if(ti.spritesheet!=null)
				{

					ti.isReadable=true;
					for(int i=0;i<ti.spritesheet.Length;i++)
					{
						SpriteMetaData spriteMeta=ti.spritesheet[i];
						Rect rect=spriteMeta.rect;
						if(rect.x<0)
							rect.x=0;
						if(rect.y<0)
							rect.y=0;
						if(rect.x+rect.width>=texture.width)
						{
							rect.width=texture.width-rect.x;
						}
						
						if(rect.y+rect.height>=texture.height)
						{
							rect.height=texture.height-rect.y;
						}
						Texture2D texSprite=new Texture2D((int)rect.width,(int)rect.height);
						texSprite.SetPixels(texture.GetPixels((int)rect.x,(int)rect.y,(int)rect.width,(int)rect.height));
						texSprite.Apply();
						SpriteElement spriteElement=new SpriteElement(texSprite,false);
						spriteElement.TrimTexture();
						spriteElement.SetPivot(spriteMeta.pivot);
						spriteElement.name=spriteMeta.name;
						listSprite.Add(spriteElement);
						
					}
					bool result=EditorUtility.DisplayDialog("Confirmation","Do you want to override on this image? Press (YES) to overide, (NO) to create working copy.","YES","NO");
					
					if(result==true)
					{
						TexturePacker.BuildAtlas(TrimType.TrimMinimum,listSprite,path,null,true);
					}
					else
					{
						string path2=path.Replace(".png","_copy.png");
						TexturePacker.BuildAtlas(TrimType.TrimMinimum,listSprite,path2,null);
					}
				}
			//}
			/*catch(System.Exception ex)
			{
				tryCount++;
				if(tryCount>3)
				{
					Debug.LogError("Error:"+ex.Message);
				}
				else
				{
					OnProcessOptimizeSprite(texture);
				}
			}*/
		}

		public static bool OnProcessBuildSpineSprite(Texture2D texture,SpineAtlasInfoInOneImage info,string pathoutput,float scale=1)
		{
			float prog =0.0f;
			EditorUtility.DisplayCancelableProgressBar("Generate Atlas Sprite ", "Process... "+pathoutput, prog);

			if(File.Exists(pathoutput))
			{
				//File.Delete(pathoutput);
				AssetDatabase.DeleteAsset(pathoutput);
				AssetDatabase.Refresh();
			}
			prog =0.1f;
			EditorUtility.DisplayCancelableProgressBar("Generate Atlas Sprite ", "Process... "+pathoutput, prog);

			//string path=AssetDatabase.GetAssetPath(texture);
			ImportTextureUtil.MaxImportSettings(texture);
			ImportTextureUtil.ReadAndUnScale(texture);
			Texture2D texSprite=new Texture2D((int)texture.width,(int)texture.height);
			texSprite.SetPixels(texture.GetPixels(0,0,texture.width,texture.height));
			texSprite.Apply();	
			prog =0.2f;
			EditorUtility.DisplayCancelableProgressBar("Generate Atlas Sprite ", "Process... "+pathoutput, prog);

			byte[] byt = texSprite.EncodeToPNG();
			if (pathoutput != "") 
			{
				System.IO.File.WriteAllBytes(pathoutput, byt);
				AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);         
			}
			prog =0.3f;
			EditorUtility.DisplayCancelableProgressBar("Generate Atlas Sprite ", "Process... "+pathoutput, prog);

			AssetDatabase.ImportAsset(pathoutput);
			texSprite=AssetDatabase.LoadAssetAtPath(pathoutput,typeof(Texture2D)) as Texture2D;


			TextureImporter ti = AssetImporter.GetAtPath(pathoutput) as TextureImporter;
			TextureImporterSettings settings = new TextureImporterSettings();
			ti.textureType = TextureImporterType.Sprite;
			ti.ReadTextureSettings(settings);
			List<SpriteMetaData> listTemp=new List<SpriteMetaData>();
			foreach(KeyValuePair<string,SpineSpriteElemnt> pair in info.imageSprites)
			{
				SpriteMetaData metaSprite=new SpriteMetaData();
				metaSprite.name=pair.Key;
				Rect rect=new Rect(pair.Value.rect.x*scale,(texture.height/scale-pair.Value.rect.height-pair.Value.rect.y)*scale,pair.Value.rect.width*scale,pair.Value.rect.height*scale);
				Rect rectInfo=rect;
				metaSprite.rect=rectInfo;

				int oWidth=(int)(pair.Value.rect.width*scale);
				int oHeight=(int)(pair.Value.rect.height*scale);
				if(oWidth<1)
					oWidth=1;
				if(oHeight<1)
					oHeight=1;
				float xLeft=pair.Value.offset.x*scale;
				float yTop=pair.Value.offset.y*scale;

				if(!pair.Value.isRotate)
				{
					float pivotX=0.5f*pair.Value.original.x*scale;
					float pivotY=0.5f*pair.Value.original.y*scale;
					pivotX=pivotX-xLeft;
					pivotY=pivotY-yTop;
					pivotX=pivotX/oWidth;
					pivotY=pivotY/oHeight;
					
					metaSprite.pivot=new Vector2(pivotX,pivotY);
					metaSprite.alignment=EditorUtil.GetAlignment(metaSprite.pivot);
				}
				else
				{

					float pivotX=1-((0.5f*pair.Value.original.y-pair.Value.offset.y))/pair.Value.rect.width;
					float pivotY=((0.5f*pair.Value.original.x-pair.Value.offset.x))/pair.Value.rect.height;


					/*float orgX=0.5f*pair.Value.original.y*scale+rect.x;
					float orgY=0.5f*pair.Value.original.x*scale-xLeft+rect.y;
					float realX=0.5f*oWidth+rect.x;
					float realY=0.5f*oHeight+rect.y;
					float pivotX=oWidth/2.0f+(orgX-realX);
					float pivotY=oHeight/2.0f+(orgY-realY);

					pivotX=pivotX/oWidth;
					pivotY=pivotY/oHeight;*/
					
					metaSprite.pivot=new Vector2(pivotX,pivotY);
					//metaSprite.pivot=new Vector2(0.5f,0.5f);
					metaSprite.alignment=EditorUtil.GetAlignment(metaSprite.pivot);
					if(metaSprite.name.Contains("nham"))
					{
						//Debug.LogError("org:"+pair.Value.original+",rect:"+pair.Value.rect.ToText()+",offset:"+pair.Value.offset);
						//Debug.LogError(metaSprite.name+","+metaSprite.pivot+","+pivotX+","+pivotY);
					}
				}
				/*
				float optimizeX=pair.Value.original.x-pair.Value.rect.width;
				float optimizeY=pair.Value.original.y-pair.Value.rect.height;
				float pivotX=0.5f-(pair.Value.offset.x)/pair.Value.original.x;
				
				float pivotY=0.5f-(pair.Value.offset.y)/pair.Value.original.y;
				*/
				listTemp.Add(metaSprite);
			}
			prog =0.8f;
			EditorUtility.DisplayCancelableProgressBar("Generate Atlas Sprite ", "Process... "+pathoutput, prog);


			SpriteMetaData[] lstMetaSprite=new SpriteMetaData[listTemp.Count];
			for(int i=0;i<listTemp.Count;i++)
			{
				lstMetaSprite[i]=listTemp[i];
			}
			ti.isReadable=true;
			ti.mipmapEnabled=false;
			ti.spritesheet=lstMetaSprite;
			ti.textureType=TextureImporterType.Sprite;
			ti.spriteImportMode=SpriteImportMode.Multiple;
			ti.spritePixelsPerUnit=100;
			settings.textureFormat = TextureImporterFormat.ARGB32;
			settings.npotScale = TextureImporterNPOTScale.None;
			settings.alphaIsTransparency = true;
			ti.SetTextureSettings(settings);
			ti.maxTextureSize=4096;
			ti.mipmapEnabled=false;
			ti.spriteImportMode=SpriteImportMode.Multiple;
			AssetDatabase.ImportAsset(pathoutput);
			EditorUtility.SetDirty(texSprite);
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
			AssetDatabase.ImportAsset(pathoutput);
			prog =1.0f;
			EditorUtility.DisplayCancelableProgressBar("Generate Atlas Sprite ", "Process... "+pathoutput, prog);
			EditorUtility.ClearProgressBar();

			ti.spriteImportMode=SpriteImportMode.Multiple;
			EditorUtility.SetDirty(texSprite);
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
			AssetDatabase.ImportAsset(pathoutput);
			return true;
		}

	}

}