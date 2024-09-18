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
	public enum ExportCurveType
	{
		InterpolateAngle,
		Default
	}
	public enum TrimType
	{
		Trim2nTexture	=1,
		TrimMinimum		=2,
		NotTrimming		=3
	}
	public class IntRect
	{
		public int x;
		public int y;
		public int width;
		public int height;
		public IntRect()
		{
			
		}
		public IntRect(int _x,int _y,int _w,int _h)
		{
			this.x=_x;
			this.y=_y;
			this.width=_w;
			this.height=_h;
		}
		public string ToText()
		{
			return "{"+x+","+y+","+width+","+height+"}";
		}
	}
	public class SpriteElement
	{
		public Texture2D texture;
		public IntRect originalRect;
		public IntRect optimizeRect;
		public int startX;
		public int startY;
		public string name;
		public int alignment;
		public Vector2 pivot;
		private bool isOptimize;
		private bool needSetAuthority;

		public bool IsOptimize()
		{
			return isOptimize;
		}
		public void SetPivot(Vector2 _vecPivot)
		{
			this.pivot=_vecPivot;
			Vector2 vec=this.pivot;
			float epsilon=0.0001f;
			if(Mathf.Abs(vec.x-0.5f)<epsilon&&Mathf.Abs(vec.y-0.5f)<epsilon)//center
			{
				alignment=0;
			}
			else if(Mathf.Abs(vec.x)<epsilon&&Mathf.Abs(vec.y-1)<epsilon)//top left
			{
				alignment=1;
			}
			else if(Mathf.Abs(vec.x-0.5f)<epsilon&&Mathf.Abs(vec.y-1)<epsilon)//top
			{
				alignment=2;
			}
			else if(Mathf.Abs(vec.x-1)<epsilon&&Mathf.Abs(vec.y-1)<epsilon)//top Right
			{
				alignment=3;
			}
			else if(Mathf.Abs(vec.x-0)<epsilon && Mathf.Abs(vec.y-0.5f)<epsilon)//left
			{
				alignment=4;
			}
			else if(Mathf.Abs(vec.x-1)<epsilon&&Mathf.Abs(vec.y-0.5f)<epsilon)//left
			{
				alignment=5;
			}
			else if(Mathf.Abs(vec.x-0)<epsilon&&Mathf.Abs(vec.y-0)<epsilon)// bottom left
			{
				alignment=6;
			}
			else if(Mathf.Abs(vec.x-0.5f)<epsilon&&Mathf.Abs(vec.y-0)<epsilon)//bottom
			{
				alignment=7;
			}
			else if(Mathf.Abs(vec.x-1)<epsilon&&Mathf.Abs(vec.y-0)<epsilon)//bottom right
			{
				alignment=8;
			}
			else//custom
			{
				alignment=9;
			}
		}
		public SpriteElement(Texture2D _texture,bool _needSetAuthority=true)
		{
			this.needSetAuthority=_needSetAuthority;
			if(this.needSetAuthority)
			{
				ImportTextureUtil.MaxImportSettings(_texture);
			}
			this.texture=_texture;
			this.name=texture.name;
			this.originalRect=new IntRect(0,0,texture.width,texture.height);
			this.optimizeRect=new IntRect(0,0,texture.width,texture.height);
			this.isOptimize=false;
			this.startX=0;
			this.startY=0;
			alignment=0;// o giua
			this.pivot=new Vector2(0.5f,0.5f);//default
			if(this.needSetAuthority)
			{
				string path=AssetDatabase.GetAssetPath(texture);
				TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
				if(ti.textureType==TextureImporterType.Sprite)
				{
					//Debug.LogError("Here:"+Pathfinding.Serialization.JsonFx.JsonWriter.Serialize(ti));

					if(ti.spriteImportMode==SpriteImportMode.Single)
					{
						SetPivot(ti.spritePivot);
					}
					else
					{
						if(ti.spritesheet!=null&&ti.spritesheet.Length>0)
						{
							//Debug.LogError("Here:"+ti.spritesheet[0].pivot+","+ti.spritesheet[0].alignment);
							alignment=ti.spritesheet[0].alignment;
							this.pivot=ti.spritesheet[0].pivot;
						}
					}
				}
			}
		}
		public void CloneFromOriginTexture()
		{
			if(this.needSetAuthority)
			{
				ImportTextureUtil.MakeReadable(texture);
				Texture2D text2=new Texture2D(texture.width,texture.height);
				text2.SetPixels(texture.GetPixels(0,0,texture.width,texture.height));
				text2.Apply();
				this.texture=text2;
			}

		}
		public void TrimTexture()
		{

			if(this.needSetAuthority)
			{
				ImportTextureUtil.MakeReadable(texture);
			}
			Color32[] pixels = texture.GetPixels32();
			int xmin = texture.width;
			int xmax = 0;
			int ymin = texture.height;
			int ymax = 0;
			int oldWidth = texture.width;
			int oldHeight = texture.height;
			
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
			if(xmin>0||ymin>0||xmax<originalRect.width||ymax<originalRect.height)
			{
				// co the optimize dc
				isOptimize=true;
				Texture2D text2=new Texture2D(xmax-xmin,ymax-ymin);
				text2.SetPixels(texture.GetPixels(xmin,ymin,xmax-xmin,ymax-ymin));
				text2.Apply();
				this.texture=text2;
				this.optimizeRect=new IntRect(0,0,texture.width,texture.height);
				this.startX=xmin;
				this.startY=ymin;
			}
		}
		public void FreeMemory()
		{
			texture=null;
			//GameObject.Destroy(texture);

			
		}
		public Rect GetSpriteRect()
		{
			return new Rect(optimizeRect.x,optimizeRect.y,optimizeRect.width,optimizeRect.height);
		}
	}
}
