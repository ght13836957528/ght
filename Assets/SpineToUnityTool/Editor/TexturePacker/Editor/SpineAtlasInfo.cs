using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
#if UNITY_4_0_0 ||UNITY_4_0 || UNITY_4_0_1||UNITY_4_1||UNITY_4_2||UNITY_4_3||UNITY_4_4||UNITY_4_5||UNITY_4_6||UNITY_4_7||UNITY_4_8||UNITY_4_9
namespace OnePStudio.SpineToUnity4
#else
namespace OnePStudio.SpineToUnity5
#endif
{
	public class SpineSpriteElemnt
	{
		public string name;
		public string textureName;
		public bool isRotate;
		public IntRect rect;
		public Vector2 original;
		public Vector2 offset;
		public int index=0;
		public int GetIndex(){
			if (index == null) {
				return 0;
			}
			return index;
		}
		public float u,v,u2,v2;
		public SpineSpriteElemnt()
		{

		}
		public SpineSpriteElemnt(bool _isRotate, IntRect _rect, Vector2 _origin,Vector2 _offset,int _index) 
		{
			isRotate=_isRotate;
			rect=_rect;
			original=_origin;
			offset=_offset;
			index=_index;
		}
	}
	public class SpineAtlasInfoInOneImage
	{
		public string imageName;
		public int width=1;
		public int height=1;
		public Dictionary<string,SpineSpriteElemnt> imageSprites=new Dictionary<string, SpineSpriteElemnt>();
		public SpineAtlasInfoInOneImage(string _imageName)
		{
			this.imageName=_imageName;
		}
	}
	public class AllSpineAtlasInfo
	{
		public Dictionary<string,SpineAtlasInfoInOneImage> listInfoImageAtlas=new Dictionary<string,SpineAtlasInfoInOneImage>();
		public AllSpineAtlasInfo()
		{

		}
		public SpineSpriteElemnt GetSpineSpriteElementByName(string name)
		{
			SpineSpriteElemnt element=null;
			foreach(KeyValuePair<string,SpineAtlasInfoInOneImage> pair1 in listInfoImageAtlas)
			{
				Dictionary<string,SpineSpriteElemnt> imageSprites =pair1.Value.imageSprites;
				imageSprites.TryGetValue(name,out element);
				if(element!=null)
				{
					return element;
				}
			}
			return element;
		}

		public bool Init(string text,List<Texture2D> lstTexure)
		{
			listInfoImageAtlas.Clear();
			if(text!=null&&text.Length>0)
			{
				List<string> list=EditorUtil.LoadStringByLine(text);
				List<string> listInfoOneImage=new List<string>();
				for(int i=1;i<list.Count;i++)
				{
					if(list[i].Length==0)
					{
						AddNewInfoDataOneImage(listInfoOneImage,lstTexure);
						listInfoOneImage.Clear();
					}
					else
					{
						listInfoOneImage.Add(list[i]);
					}
				}
			}
			if(listInfoImageAtlas.Count>0)
				return true;
			else
				return false;
		}
		public void AddNewInfoDataOneImage(List<string> list,List<Texture2D> lstTexure)
		{
			int index=0;
			if(list.Count>0)
			{
				string spineNameAtlas=Path.GetFileNameWithoutExtension(list[0].Trim());
				SpineAtlasInfoInOneImage spineOne=new SpineAtlasInfoInOneImage(Path.GetFileNameWithoutExtension(list[0].Trim()));
				try// spine 2
				{
					string[] sizeImage=list[index+1].Replace("size:","").Split(new string[]{","},System.StringSplitOptions.RemoveEmptyEntries);
					spineOne.width=int.Parse(sizeImage[0].Trim());
					spineOne.height=int.Parse(sizeImage[1].Trim());
				}
				catch(System.Exception ex)// spine 1
				{
					Texture2D _textureSpine=null;
					for(int i=0;i<lstTexure.Count;i++)
					{
						if(lstTexure[i].name==spineNameAtlas)
						{
							_textureSpine=lstTexure[i];
							break;
						}
					}
					if(_textureSpine!=null)
					{
						_textureSpine=ImportTextureUtil.MaxImportSettings(_textureSpine);
						spineOne.width=_textureSpine.width;
						spineOne.height=_textureSpine.height;
					}
					else
					{
						Debug.LogError("#331 Input Error, can't not find texture:"+_textureSpine);
					}

				}
				//Debug.LogError("kich thuoc hinh anh:"+spineOne.width+","+spineOne.height);
				int count =0;
				while(true)
				{
					count++;
					if(count>9999999)
					{
						Debug.LogError("Error #532: Process Limit Layer!");
						break;
					}
					string data=list[index];
					//List<string> listData=new List<string>();
					if(data.Length>0)
					{
						//string name="";
						if(data.Length>2&&data.Substring(0,2).Equals("  "))
						{
							SpineSpriteElemnt info=new SpineSpriteElemnt();
							info.name=list[index-1].Trim();
							if (!string.IsNullOrEmpty (info.name)) {
								info.name = info.name.Replace ("/", "_");
							}//Debug.LogError ("AAA:" + info.name);
							/*if (info.name.Length > 0) {
								int index3 = info.name.LastIndexOf ("/");
								info.name = info.name.Substring (index3 + 1, info.name.Length - index3 - 1);
							}*/
							info.textureName=spineOne.imageName;
							if(data.ToLower().Contains("false"))
							{
								info.isRotate=false;
							}
							else
							{
								info.isRotate=true;
							}
							string[] subxy=list[index+1].Replace("  xy:","").Split(new string[]{","},System.StringSplitOptions.RemoveEmptyEntries);
							int x=int.Parse(subxy[0].Trim());
							int y=int.Parse(subxy[1].Trim());
							string[] subwh=list[index+2].Replace("  size:","").Split(new string[]{","},System.StringSplitOptions.RemoveEmptyEntries);
							int w=int.Parse(subwh[0].Trim());
							int h=int.Parse(subwh[1].Trim());
							if(!info.isRotate)
								info.rect=new IntRect(x,y,w,h);
							else
								info.rect=new IntRect(x,y,h,w);
							info.u=(float)x/spineOne.width;
							info.v=1-(float)y/spineOne.height;
							if (info.isRotate) {
								info.u2 = (float)(x + h) / spineOne.width;
								info.v2 = 1-(float)(y + w) / spineOne.height;
							} else {
								info.u2 = (float)(x + w) / spineOne.width;
								info.v2 = 1-(float)(y + h) / spineOne.height;
							}

							string[] suborg=list[index+3].Replace("  orig:","").Split(new string[]{","},System.StringSplitOptions.RemoveEmptyEntries);
							float orgx=float.Parse(suborg[0].Trim());
							float orgy=float.Parse(suborg[1].Trim());
							
							info.original=new Vector2(orgx,orgy);
							string[] suboffset=list[index+4].Replace("  offset:","").Split(new string[]{","},System.StringSplitOptions.RemoveEmptyEntries);
							float offsetx=float.Parse(suboffset[0].Trim());
							float offsety=float.Parse(suboffset[1].Trim());
							
							info.offset=new Vector2(offsetx,offsety);
							string strIndex=list[index+5].Replace("  index:","").Trim();
							info.index=int.Parse(strIndex);
							index+=6;
							spineOne.imageSprites[info.name]=info;
							if(index>=list.Count)
							{
								break;
							}
						}
						else
						{
							index++;
							if(index>=list.Count)
							{
								break;
							}
						}
					}
					else
					{
						Debug.LogError("Error #213:"+data);
						break;
					}
				}
				listInfoImageAtlas[spineOne.imageName]=spineOne;
			}

		}
	}
}