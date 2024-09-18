using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
#if UNITY_4_0_0 ||UNITY_4_0 || UNITY_4_0_1||UNITY_4_1||UNITY_4_2||UNITY_4_3||UNITY_4_4||UNITY_4_5||UNITY_4_6||UNITY_4_7||UNITY_4_8||UNITY_4_9
namespace OnePStudio.SpineToUnity4
#else
namespace OnePStudio.SpineToUnity5
#endif
{
	public class TexturePacker 
	{
		[MenuItem("Assets/Tools/Spine2Unity/Anim_FixShowHide")]
		static public void FixShowHide()
		{
			Object[] selection = Selection.GetFiltered (typeof(Object), SelectionMode.DeepAssets);
			if(selection.Length<1)
			{
				EditorUtility.DisplayDialog("Error","Please select At least one animation clip","OK");
				return;  
			}
			else
			{
				for(int j=0;j<selection.Length;j++)
				{
					Object obj=selection[j];
					if(obj is AnimationClip)
					{
						AnimationClip anim=(AnimationClip) obj;
						EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings (anim);
						//Debug.LogError(JsonWriter.Serialize(curveBindings));
						for(int x=0;x<curveBindings.Length;x++)
						{
							if(curveBindings[x].propertyName=="m_IsActive")
							{
								AnimationCurve curve=AnimationUtility.GetEditorCurve(anim,curveBindings[x]);
								int count =curve.keys.Length;
								List<UnityEngine.Keyframe> list=new List<Keyframe>();
								for(int i=0;i<count;i++)
								{
									Keyframe keyframe=curve.keys[0];
									//keyframe.tangentMode=
									list.Add(curve.keys[0]);
									curve.RemoveKey(0);
									
									//Debug.LogError(i);
									//KeyframeUtil.SetKeyBroken(curve.keys[i],true);
									//curve.keys[i].tangentMode=0;
								}
								for(int i=0;i<list.Count;i++)
								{
									curve.AddKey(KeyframeUtil.GetNew(list[i].time,list[i].value,KeyframeUtil.GetTangleMode("stepped")));
								}
								AnimationUtility.SetEditorCurve(anim,curveBindings[x],curve);
							}	
						}
					}
				}
			}
		}

		[MenuItem("Assets/Tools/Spine2Unity/Anim_FixRotate")]
		static public void Anim_FixRotate()
		{
			Object[] selection = Selection.GetFiltered (typeof(Object), SelectionMode.DeepAssets);
			if(selection.Length<1)
			{
				EditorUtility.DisplayDialog("Error","Please select At least one animation clip","OK");
				return;  
			}
			else
			{
				for(int j=0;j<selection.Length;j++)
				{
					Object obj=selection[j];
					if(obj is AnimationClip)
					{
						AnimationClip anim=(AnimationClip) obj;
						EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings (anim);
						//Debug.LogError(JsonWriter.Serialize(curveBindings));
						for(int x=0;x<curveBindings.Length;x++)
						{
							if(curveBindings[x].propertyName=="m_LocalRotation.z")
							{
								AnimationCurve curve=AnimationUtility.GetEditorCurve(anim,curveBindings[x]);
								int count =curve.keys.Length;
								List<UnityEngine.Keyframe> list=new List<Keyframe>();
								for(int i=0;i<count-1;i++)
								{
									Keyframe keyframe=curve.keys[0];
									//keyframe.tangentMode=
									list.Add(curve.keys[0]);
									float temp1=keyframe.outTangent;
									keyframe.outTangent=0;
									keyframe.inTangent=0;
									float temp2=keyframe.outTangent;
									curve.MoveKey(i,keyframe);
									
									//Debug.LogError(i+":"+temp1+","+temp2);
									//KeyframeUtil.SetKeyBroken(curve.keys[i],true);
									//curve.keys[i].tangentMode=0;
								}
								/*for(int i=0;i<list.Count;i++)
								{
									curve.AddKey(KeyframeUtil.GetNew(list[i].time,list[i].value,KeyframeUtil.GetTangleMode("stepped")));
								}*/
								AnimationUtility.SetEditorCurve(anim,curveBindings[x],curve);
							
							}	
						}
					}
				}
			}
		}
		[MenuItem("Assets/Tools/Spine2Unity/TexturePacker/Trim To 2(n) Texture size")]
		static public void AutoBuildAtlasTrimTo2n()
		{
			AutoBuildAtlas(TrimType.Trim2nTexture);
		}

		[MenuItem("Assets/Tools/Spine2Unity/TexturePacker/Trim to minimum Texture")]
		static public void AutoBuildAtlasTrimAll()
		{
			AutoBuildAtlas(TrimType.TrimMinimum);
		}

		[MenuItem("Assets/Tools/Spine2Unity/TexturePacker/Not Trimming")]
		static public void AutoBuildAtlasNotTrim()
		{
			AutoBuildAtlas(TrimType.NotTrimming);
		}

		static public void AutoBuildAtlas (TrimType trimType)
		{
			Object[] selection = Selection.GetFiltered (typeof(Object), SelectionMode.DeepAssets);
			if(selection.Length<1)
			{
				
				Debug.LogWarning("Please choose some images File");
				EditorUtility.DisplayDialog("Error","Please choose some images File","OK");
				return;  
			}

			List<SpriteElement> listSprite=new List<SpriteElement>();
			for(int i=0;i<selection.Length;i++)
			{
				Object obj=selection[i];
				if(obj is Texture2D)
				{
					Texture2D tex=(Texture2D)obj;
			
					SpriteElement element=new SpriteElement(tex);
					if(trimType==TrimType.Trim2nTexture||trimType==TrimType.TrimMinimum)
					{
						element.TrimTexture();
						//Debug.LogError(element.texture.name);
					}
					else
					{
						element.CloneFromOriginTexture();
					}
					listSprite.Add(element);
				}

			}
			if(listSprite.Count>0)
			{
				string texturePath = EditorUtility.SaveFilePanelInProject("Save As", "New Atlas", "png", "Save atlas as...");
				if(texturePath.Length==0)
					return;
				BuildAtlas(trimType,listSprite,texturePath,null);
			}
		}
		static public bool UpdateAtlasSpriteInfo(string pathOutput,List<DataAnimAnalytics> listAnim,float scale=1)
		{
			if(listAnim.Count<1)
				return false;

			Dictionary<string,SpineAttachmentElelement> dicPivotCache=new Dictionary<string, SpineAttachmentElelement>();
			for(int i=0;i<listAnim.Count;i++)
			{
				DataAnimAnalytics dataAnalytic=listAnim[i];
				foreach(KeyValuePair<string,SpineAttachmentElelement> pair in dataAnalytic.jsonFinal.dicPivot)
				{
					dicPivotCache[pair.Value.name]=pair.Value;
				}
			}
			Texture2D texSprite=AssetDatabase.LoadAssetAtPath(pathOutput,typeof(Texture2D)) as Texture2D;

			TextureImporter ti = AssetImporter.GetAtPath(pathOutput) as TextureImporter;
			TextureImporterSettings settings = new TextureImporterSettings();
			ti.ReadTextureSettings(settings);
			SpriteMetaData[] lstMetaSprite=ti.spritesheet;
			for(int i=0;i<lstMetaSprite.Length;i++)
			{
				SpriteMetaData meta=lstMetaSprite[i];
				//Debug.LogError(meta.name+","+dicPivotCache.Count);

				if(dicPivotCache.ContainsKey(meta.name))//update new
				{
					SpineAttachmentElelement pivotCache=dicPivotCache[lstMetaSprite[i].name];
					if(!pivotCache.isOptimze)
					{
						meta.pivot=new Vector2(pivotCache.pivotX,pivotCache.pivotY);//currentMeta.pivot;
						meta.alignment=EditorUtil.GetAlignment(meta.pivot);
					}
					else
					{
						float pivotX=pivotCache.pivotX*scale*pivotCache.originalRect.width*scale;
						float pivotY=pivotCache.pivotY*scale*pivotCache.originalRect.height*scale;
						float oWidth=pivotCache.optimizeRect.width*scale;
						float oHeight=pivotCache.optimizeRect.height*scale;
						if(oWidth<1)
							oWidth=1;
						if(oHeight<1)
							oHeight=1;
						pivotX=pivotX-pivotCache.startX;
						pivotY=pivotY-pivotCache.startY;
						pivotX=pivotX/oWidth;
						pivotY=pivotY/oHeight;
						meta.pivot=new Vector2(pivotX,pivotY);
						meta.alignment=EditorUtil.GetAlignment(meta.pivot);
					}
				}
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
			AssetDatabase.ImportAsset(pathOutput);
			EditorUtility.SetDirty(texSprite);
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
			AssetDatabase.ImportAsset(pathOutput);
			return true;
		}
		static public bool AutoBuildAtlasFromListTexture(List<Texture2D> listTexture,List<DataAnimAnalytics> listJsonAnim, TrimType trimType, string pathOutput)
		{
			float prog =0.0f;
			EditorUtility.DisplayCancelableProgressBar("Collecting Textures", "Process...", prog);
			try
			{
				Dictionary<string,SpineAttachmentElelement> dicPivot=new Dictionary<string, SpineAttachmentElelement>();
				for(int i=0;i<listJsonAnim.Count;i++)
				{
					DataAnimAnalytics dataAnalytic=listJsonAnim[i];
					//Debug.LogError(Pathfinding.Serialization.JsonFx.JsonWriter.Serialize(dataAnalytic.jsonFinal));
					foreach(KeyValuePair<string,SpineAttachmentElelement> pair in dataAnalytic.jsonFinal.dicPivot)
					{
						dicPivot[pair.Value.name]=pair.Value;
					}
				}

				List<SpriteElement> listSprite=new List<SpriteElement>();
				for(int i=0;i<listTexture.Count;i++)
				{
					Texture2D tex=listTexture[i];
					SpriteElement element=new SpriteElement(tex);
					if(trimType==TrimType.Trim2nTexture||trimType==TrimType.TrimMinimum)
					{
						element.TrimTexture();
					}
					else
					{
						element.CloneFromOriginTexture();
					}
					foreach(KeyValuePair<string,SpineAttachmentElelement> pair in dicPivot)
					{
						if(pair.Value.name==tex.name)
						{
							element.SetPivot(new Vector2(pair.Value.pivotX,pair.Value.pivotY));
							break;
						}
					}

					listSprite.Add(element);
					prog =(float)(i+1)/listTexture.Count;
					EditorUtility.DisplayCancelableProgressBar("Collecting Textures", "Process...", prog);
				}
				if(listSprite.Count>0)
				{
					return BuildAtlas(trimType,listSprite,pathOutput,dicPivot);
				}
				else
				{
					return false;
				}
			}
			catch(UnityException ex)
			{
				Debug.LogError("#281 Error:"+ex.Message);
				EditorUtility.ClearProgressBar();
				return false;
			}
			catch(System.Exception ex)
			{
				Debug.LogError("#282 Error:"+ex.Message);
				EditorUtility.ClearProgressBar();
				return false;
			}
			catch
			{
				EditorUtility.ClearProgressBar();
				return false;
			}
		}
		static public bool BuildAtlas(TrimType trimType,List<SpriteElement> listSprite,string texturePath,Dictionary<string,SpineAttachmentElelement> dicPivot,bool append=false)
		{
			try
			{
				bool checkAppend=append;
				float prog =0.2f;
				EditorUtility.DisplayCancelableProgressBar("Creating Spritesheet", "Auto Build Atlas Sprites", prog);
				Texture2D[] textArray=new Texture2D[listSprite.Count];
				for(int i=0;i<textArray.Length;i++)
				{
					textArray[i]=listSprite[i].texture;
				}
				Texture2D mainTexture=new Texture2D(8192,8192);
				Rect[] rects = mainTexture.PackTextures(textArray,1, 8192,false);
				mainTexture.Apply();
				
				int xmin =0;
				int ymin =0;
				int cacheWidth=mainTexture.width;
				int cacheHeight=mainTexture.height;
				Texture2D mainTexture2=null;

				prog =0.4f;
				EditorUtility.DisplayCancelableProgressBar("Creating Spritesheet", "Auto Build Atlas Sprites", prog);
				#region Trim to minimum Texture
				if(trimType==TrimType.TrimMinimum)
				{
					Color32[] pixels = mainTexture.GetPixels32();
					xmin = mainTexture.width;
					int xmax = 0;
					ymin = mainTexture.height;
					int ymax = 0;
					int oldWidth = mainTexture.width;
					int oldHeight = mainTexture.height;
					
					// Trim solid pixels
					for (int y = 0, yw = oldHeight; y < yw; ++y)
					{
						for (int x = 0, xw = oldWidth; x < xw; ++x)
						{
							Color32 c = pixels[y * xw + x];
							
							if (c.a != 0)
							{
								if (y < ymin) ymin = y;
								if (y > ymax) ymax = y;
								if (x < xmin) xmin = x;
								if (x > xmax) xmax = x;
							}
						}
					}
					mainTexture2=new Texture2D(xmax-xmin,ymax-ymin);
					mainTexture2.SetPixels(mainTexture.GetPixels(xmin,ymin,xmax-xmin,ymax-ymin));
					mainTexture2.Apply();
					mainTexture=mainTexture2;
				}
				#endregion

				prog =0.5f;
				EditorUtility.DisplayCancelableProgressBar("Creating Spritesheet", "Auto Build Atlas Sprites", prog);
				#region Write New File
				byte[] byt = mainTexture.EncodeToPNG();
				if (texturePath != "") 
				{
					System.IO.File.WriteAllBytes(texturePath, byt);
					if(!checkAppend)
						AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);         
				}
				if(!checkAppend)
					AssetDatabase.ImportAsset(texturePath);
				#endregion
				prog =0.6f;
				EditorUtility.DisplayCancelableProgressBar("Creating Spritesheet", "Auto Build Atlas Sprites", prog);
				//return;
				mainTexture=AssetDatabase.LoadAssetAtPath(texturePath,typeof(Texture2D)) as Texture2D;

				TextureImporter ti = AssetImporter.GetAtPath(texturePath) as TextureImporter;
				TextureImporterSettings settings = new TextureImporterSettings();
				ti.ReadTextureSettings(settings);
				SpriteMetaData[] lstMetaSprite=new SpriteMetaData[listSprite.Count];
				if(append)
				{
					if(ti.spritesheet!=null&&ti.spritesheet.Length>0)
					{
						append=true;
						lstMetaSprite=ti.spritesheet;
					}
					else
					{
						append=false;
					}
				}

				for(int i=0;i<lstMetaSprite.Length;i++)
				{
					if(i<rects.Length)
					{
						SpriteMetaData metaSprite=new SpriteMetaData();
						if(append)
						{
							metaSprite=lstMetaSprite[i];
						}
						metaSprite.name=listSprite[i].name;
						Rect rectInfo=listSprite[i].GetSpriteRect();
					
						Rect rect=new Rect(rects[i].x*cacheWidth-xmin,rects[i].y*cacheHeight-ymin,rectInfo.width,rectInfo.height);
						metaSprite.rect=rect;
						int oWidth=listSprite[i].optimizeRect.width;
						int oHeight=listSprite[i].optimizeRect.height;
						if(oWidth<1)
							oWidth=1;
						if(oHeight<1)
							oHeight=1;
						int xLeft=listSprite[i].startX;
						int yTop=listSprite[i].startY;

						if(listSprite[i].IsOptimize())
						{
							float pivotX=listSprite[i].pivot.x*listSprite[i].originalRect.width;
							float pivotY=listSprite[i].pivot.y*listSprite[i].originalRect.height;
							pivotX=pivotX-xLeft;
							pivotY=pivotY-yTop;
							pivotX=pivotX/oWidth;
							pivotY=pivotY/oHeight;
							metaSprite.pivot=new Vector2(pivotX,pivotY);
							listSprite[i].SetPivot(metaSprite.pivot);
							if(dicPivot!=null)
							{
								foreach(KeyValuePair<string,SpineAttachmentElelement> pair in dicPivot)
								{
									if(pair.Value.name==metaSprite.name)
									{
										pair.Value.SetCache(xLeft,yTop,listSprite[i].originalRect,listSprite[i].optimizeRect);
										//Debug.LogError(pair.Value.spriteName+","+pair.Value.name+","+pair.Value.isOptimze);
									}
								}
							}
						}
						else
						{
							metaSprite.pivot=listSprite[i].pivot;
						}
						metaSprite.alignment=listSprite[i].alignment;
						lstMetaSprite[i]=metaSprite;
					}
				}
				prog =0.7f;
				EditorUtility.DisplayCancelableProgressBar("Creating Spritesheet", "Auto Build Atlas Sprites", prog);

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
				AssetDatabase.ImportAsset(texturePath);
				EditorUtility.SetDirty(mainTexture);
				AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
				AssetDatabase.ImportAsset(texturePath);

				prog =0.9f;
				EditorUtility.DisplayCancelableProgressBar("Creating Spritesheet", "Auto Build Atlas Sprites", prog);

				for(int i=0;i<listSprite.Count;i++)
				{
					listSprite[i].FreeMemory();
				}
				System.GC.Collect();

				prog =1.0f;
				EditorUtility.DisplayCancelableProgressBar("Creating Spritesheet", "Auto Build Atlas Sprites", prog);
				EditorUtility.ClearProgressBar();

				return true;
			}
			catch(UnityException ex)
			{
				Debug.LogError("#283 Error:"+ex.Message);
				EditorUtility.ClearProgressBar();
				return false;
			}
			catch(System.Exception ex)
			{
				Debug.LogError("#284 Error:"+ex.Message);
				EditorUtility.ClearProgressBar();
				return false;
			}
			catch
			{
				EditorUtility.ClearProgressBar();
				return false;
			}
		}
	}
}
