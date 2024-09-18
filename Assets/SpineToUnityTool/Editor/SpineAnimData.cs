using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_4_0_0 ||UNITY_4_0 || UNITY_4_0_1||UNITY_4_1||UNITY_4_2||UNITY_4_3||UNITY_4_4||UNITY_4_5||UNITY_4_6||UNITY_4_7||UNITY_4_8||UNITY_4_9
namespace OnePStudio.SpineToUnity4
#else
namespace OnePStudio.SpineToUnity5
#endif
{
	public enum SpriteType{
		SpriteRenderer=1,
		UGUI,
		NGUI
	}
	public enum SortLayerType
	{
		Depth=0,
		Z=1,
	}
	public enum AnimationStyle
	{
		Loop,
		Normal
	}


	public class SpineBoneElelement
	{
		public string name;//unique
		public string parent;
		public float x;
		public float y;
		public float scaleX=1;
		public float scaleY=1;
		public float length=0;
		public float rotation;
		public int index=0;
		public int index_ugui=0;
		public SpineBoneElelement()
		{

		}
	}
	public class SpineSlotElelement
	{
		public string name;//unique
		public string bone;
		public string color="ffffffff";
		public string attachment;//attachmentDefault
		public List<string> listAcceptAttachment=new List<string>();
		
		public List<SpineAttachmentElelement> listAcceptObj=new List<SpineAttachmentElelement>();
		public List<SpineSlotAndAttachmentReference> listReference=new List<SpineSlotAndAttachmentReference>();
		public Dictionary<string,string> mapNameInAtlas=new Dictionary<string, string>();
		public int index=0;
		public SpineSlotElelement()
		{

		}
	}

	public class SpineSlotAndAttachmentReference
	{
		public string name;
		public string nameInAtlas;
		public float x;
		public float y;
		public float width;
		public float height;
		public float scaleX=1;
		public float scaleY=1;
		public float rotation;
		public SpineSlotAndAttachmentReference()
		{

		}
	}
	public class SpineAttachmentElelement
	{
		public string name;
		public string nameInAtlas;
		public int depth=-100;
		public float pivotX;
		public float pivotY;
		public float x;
		public float y;
		public float width;
		public float height;
		public float scaleX=1;
		public float scaleY=1;
		public float rotation;

		public string type="region";
		public float[] uvs;
		public int[] triangles;
		public float[] vertices;
		public int hull=0;

		//just use for optimize
		public int startX;
		public int startY;
		public IntRect originalRect;
		public IntRect optimizeRect;
		public bool isOptimze=false;

		public string GetTypeAttachment()
		{
			if(type==null)
			{
				type="region";
			}
			return type.ToLower();
		}

		public void SetCache(int _startX,int _startY,IntRect _originalRect, IntRect _optimizeRect)
		{
			isOptimze=true;
			startX=_startX;
			startY=_startY;
			originalRect=_originalRect;
			optimizeRect=_optimizeRect;
		}
		public SpineAttachmentElelement()
		{

		}
		public SpineAttachmentElelement(SpineRawAttachmentElement raw,string _name,int _depth)
		{
			this.name=_name;
			this.depth=_depth;
			this.x=raw.x;
			this.y=raw.y;
			this.width=raw.width;
			this.height=raw.height;
			this.rotation=raw.rotation;
			this.scaleX=raw.scaleX;
			this.scaleY=raw.scaleY;
			this.type=raw.type;
			this.hull=raw.hull;
			this.uvs=raw.uvs;
			this.vertices=raw.vertices;
			this.triangles=raw.triangles;
			
			this.nameInAtlas=raw.name;
			if(!string.IsNullOrEmpty(raw.path))
			{
				this.nameInAtlas=raw.path;
			}

		}
	}
	
	public class SpineRawAttachmentElement
	{
		public float x;//The X position of the image relative to the slot's bone. Assume 0 if omitted.
		public float y;//
		public float width;//The width of the image.
		public float height;
		public float rotation;//The rotation in degrees of the image relative to the slot's bone. Assume 0 if omitted.
		public float scaleX=1;
		public float scaleY=1;
		public string name;// name mapping with atlas

		public string type="region";
		public float[] uvs;
		public int[] triangles;
		public float[] vertices;
		public int hull=0;
		public string path;
		public SpineRawAttachmentElement()
		{

		}
	}

	#region Animation Data Tranform
	public class AEPAnimationTranslate
	{
		public float time;
		public float x;
		public float y;
		public object curve=null;
		public string GetCurve()
		{
			string curveString="";
			if(curve!=null)
			{
				if(curve is string)
				{
					curveString=(string)curve;
				}
			}
			return curveString;
		}
		public AEPAnimationTranslate()
		{
			
		}
	}

	public class AEPAnimationScale
	{
		public float time;
		public float x;
		public float y;
		public object curve=null;
		public string GetCurve()
		{
			string curveString="";
			if(curve!=null)
			{
				if(curve is string)
				{
					curveString=(string)curve;
				}
			}
			return curveString;
		}
		public AEPAnimationScale()
		{
			
		}
	}
	public class AEPAnimationRotate
	{
		public float time;
		public float angle;
		public float angleChange=0;
		public object curve1=null;
		public string GetCurve()
		{
			string curveString="";
			if(curve1!=null)
			{
				if(curve1 is string)
				{
					curveString=(string)curve1;
				}
			}
			return curveString;
		}
		public AEPAnimationRotate()
		{
			
		}
	}
	#endregion
	public class AEPBoneAnimationElement
	{
		public string name;
		public List<AEPAnimationTranslate> translate;
		public List<AEPAnimationScale> scale;
		public List<AEPAnimationRotate> rotate;
		public AEPBoneAnimationElement()
		{

		}
	}

	public class AEPAnimationAttachment
	{
		public float time;
		public string name;
		public AEPAnimationAttachment()
		{
			
		}
	}
	public class AEPAnimationColor
	{
		public float time;
		public string color;
		public string tangentType="linear";
		public AEPAnimationColor()
		{
			
		}
	}
	public class AEPSlotAnimationElement
	{
		public string name;
		public List<AEPAnimationAttachment> attachment;
		public List<AEPAnimationColor> color;
		public AEPSlotAnimationElement()
		{
			
		}
	}
	public class AEPJsonRaw
	{
		public List<SpineBoneElelement> bones;
		
		public List<SpineSlotElelement> slots;
		public Dictionary<string,object> skins;
		public Dictionary<string,object> animations;
		public AEPJsonRaw()
		{
			
		}
	}

	public class AEPJsonRawV2
	{
		public List<SpineBoneElelement> bones;
		
		public List<SpineSlotElelement> slots;
		public List<Dictionary<string,object>> skins;
		public Dictionary<string,object> animations;
		public AEPJsonRawV2()
		{
			
		}
	}
	public class AEPAnimationClipFile
	{
		public string clipName;
		public Dictionary<string,AEPBoneAnimationElement> dicAnimation=new Dictionary<string,AEPBoneAnimationElement>();
		public Dictionary<string,AEPSlotAnimationElement> dicSlotAttactment=new Dictionary<string,AEPSlotAnimationElement>();
		public AEPAnimationClipFile(string _clipName,Dictionary<string,AEPBoneAnimationElement>_dicAnimation,
		                            Dictionary<string,AEPSlotAnimationElement> _dicSlotAttactment)
		{
			clipName=_clipName.ToLower();
			dicAnimation=_dicAnimation;
			dicSlotAttactment=_dicSlotAttactment;
		}
	}


	public class AEPJsonFinal
	{
		public bool isUseMesh=false;
		public List<SpineBoneElelement> bones;
		public List<SpineSlotElelement> slots;

		public Dictionary<string,SpineBoneElelement> dicBones;
		public Dictionary<string,SpineSlotElelement> dicSlots;

		public Dictionary<string,SpineAttachmentElelement> dicPivot=new Dictionary<string,SpineAttachmentElelement>();
		public List<AEPAnimationClipFile> listAnimationClip=new List<AEPAnimationClipFile>();

		public AEPJsonFinal()
		{
			
		}
		public AEPJsonFinal(AEPJsonRaw raw)
		{
			#region Generate Bones Info
			this.bones=raw.bones;
			dicBones=new Dictionary<string, SpineBoneElelement>();
			for(int i=0;i<bones.Count;i++)
			{
				bones[i].index=i;
				bones[i].index_ugui=i;
				dicBones[bones[i].name]=bones[i];
			}
			#endregion

			#region gernera Slots Info
			this.slots=raw.slots;
			if(this.slots==null)
			{
				this.slots=new List<SpineSlotElelement>();
			}
			dicSlots=new Dictionary<string,SpineSlotElelement>();
			for(int i=0;i<slots.Count;i++)
			{
				slots[i].index=slots.Count-i;

				this.dicSlots[slots[i].name]=slots[i];
				string slotAttachmentName=slots[i].attachment;

				if(slotAttachmentName==null)
				{
					if(dicSlots.ContainsKey(slots[i].name))
					{
						slotAttachmentName="";
					}
				}
				if(slotAttachmentName!=null)
				{
					//Debug.LogError(slotAttachmentName);
					//int index=slotAttachmentName.LastIndexOf("/");
					//slotAttachmentName=slotAttachmentName.Substring(index+1,slotAttachmentName.Length-index-1);
					if (!string.IsNullOrEmpty (slotAttachmentName)) {
						slotAttachmentName=slotAttachmentName.Replace ("/", "_");
					}

					slots[i].attachment=slotAttachmentName;
			    		slots[i].index=slots.Count-i;
			    		this.dicSlots[slots[i].name]=slots[i];
					SpineBoneElelement boneTemp=null;
					dicBones.TryGetValue(slots[i].bone,out boneTemp);
					if(boneTemp!=null){
						//Debug.LogError(slots[i].bone+","+i);
						boneTemp.index_ugui=slots[i].index;
						if(!string.IsNullOrEmpty(boneTemp.parent)){
							SpineBoneElelement boneParent=null;
							if(dicBones.TryGetValue(boneTemp.parent,out boneParent)){
								//boneParent.index=boneTemp.index-1;
								//Debug.LogError(boneParent.name+","+boneParent.index);
							}
						}
					}
				}
			}
			#endregion

			Dictionary<string,object> skins=raw.skins;// json->skins
			
			foreach(KeyValuePair<string,object> pair1 in skins)
			{
				//Debug.LogError(pair1.Key+":"+Pathfinding.Serialization.JsonFx.JsonWriter.Serialize(pair1.Value));
				Dictionary<string,object> temp1=(Dictionary<string,object>)pair1.Value;// json->skins->default
				int total=1;

				#region calculate total attachment
				foreach(KeyValuePair<string,object> pair2 in temp1)
				{
					Dictionary<string,object> temp2=(Dictionary<string,object>)pair2.Value;
					foreach(KeyValuePair<string,object> pair3 in temp2)
					{
						total++;
					}
				}
				#endregion

				int count=0;
				foreach(KeyValuePair<string,object> pair2 in temp1)
				{
					string slotsName=pair2.Key;
					SpineSlotElelement slot=null;
					dicSlots.TryGetValue(slotsName,out slot);


					Dictionary<string,object> temp2=(Dictionary<string,object>)pair2.Value;
					foreach(KeyValuePair<string,object> pair3 in temp2)
					{
						int depth=-count;
						//Debug.LogError(pair3.Key);
						string attachmentName=pair3.Key;
						//Debug.LogError ("BBB:" + attachmentName);

						/*if (attachmentName.Length > 0) {
							int index = attachmentName.LastIndexOf ("/");
							attachmentName = attachmentName.Substring (index + 1, attachmentName.Length - index - 1);
						}*/
						if (!string.IsNullOrEmpty (attachmentName)) {
							attachmentName=attachmentName.Replace ("/", "_");
						}
						SpineRawAttachmentElement rawPivot=Pathfinding.Serialization.JsonFx.JsonReader.Deserialize(
							Pathfinding.Serialization.JsonFx.JsonWriter.Serialize(pair3.Value), typeof(SpineRawAttachmentElement)) as SpineRawAttachmentElement;
						if(slot!=null)
						{
							depth=-slot.index;
						}
						/*SpineSlotElelement slotDepth=null;
						dicSlots.TryGetValue(attachmentName,out slotDepth);
						if(slotDepth!=null)
						{
							depth=-slotDepth.index;
						}
						else
						{
							bool isAdd=false;
							for(int i=0;i<slots.Count;i++)
							{
								if(slots[i].attachment==attachmentName)
								{
									isAdd=true;
									depth=-slots[i].index;
									break;
								}
							}
						}*/
						if(string.IsNullOrEmpty(rawPivot.name))
						{
							rawPivot.name=attachmentName;
						}
						//Debug.LogError(Pathfinding.Serialization.JsonFx.JsonWriter.Serialize(rawPivot));
						SpineAttachmentElelement pivot=new SpineAttachmentElelement(rawPivot,attachmentName,depth);
						if(!isUseMesh)
						{
							if(rawPivot.type!=null&&
							   (rawPivot.type=="mesh"||rawPivot.type=="skinnedmesh"))
							{
								isUseMesh=true;
							}
						}
						pivot.pivotX= 0.5f;//+rawPivot.x/(rawPivot.width);
						pivot.pivotY= 0.5f;//+rawPivot.y/(rawPivot.height);
						count++;
						//Debug.LogError(pivot.name);
						pivot.name=pivot.name.Trim();
						dicPivot[pivot.nameInAtlas]=pivot;
						if(slot!=null&&!slot.listAcceptAttachment.Contains(pivot.name))
						{
							slot.listAcceptAttachment.Add(pivot.name);
							slot.listAcceptObj.Add(pivot);
							slot.mapNameInAtlas[pivot.name]=pivot.nameInAtlas;
							SpineSlotAndAttachmentReference temp=new SpineSlotAndAttachmentReference();
							temp.name=pivot.name;
							temp.x=rawPivot.x;
							temp.y=rawPivot.y;
							temp.scaleX=rawPivot.scaleX;
							temp.scaleY=rawPivot.scaleY;
							temp.height=rawPivot.height;
							temp.width=rawPivot.width;
							temp.rotation=rawPivot.rotation;
							slot.listReference.Add(temp);
						}
					}
				}
			}

			if(raw.animations!=null)
			{
				foreach(KeyValuePair<string,object> pair1 in raw.animations)
				{
					string clipName=pair1.Key;
					Dictionary<string,AEPBoneAnimationElement> dicAnimation=new Dictionary<string,AEPBoneAnimationElement>();
					Dictionary<string,AEPSlotAnimationElement> dicSlotAttactment=new Dictionary<string,AEPSlotAnimationElement>();

					Dictionary<string,object> clipJson=(Dictionary<string,object>)pair1.Value;
					foreach(KeyValuePair<string,object> pairClip in clipJson)
					{
						if(pairClip.Key=="bones")// tam thoi chi su ly cho bone
						{
							Dictionary<string,object> temp1=(Dictionary<string,object>)pairClip.Value;
							
							foreach(KeyValuePair<string,object> pair2 in temp1)
							{
								//Debug.LogError(Pathfinding.Serialization.JsonFx.JsonWriter.Serialize(pair2.Value));
								AEPBoneAnimationElement full=Pathfinding.Serialization.JsonFx.JsonReader.Deserialize(
									Pathfinding.Serialization.JsonFx.JsonWriter.Serialize(pair2.Value), typeof(AEPBoneAnimationElement)) as AEPBoneAnimationElement;
								full.name=pair2.Key;
								dicAnimation[full.name]=full;
							}
							//Debug.LogError(Pathfinding.Serialization.JsonFx.JsonWriter.Serialize(dicAnimation));
						}
						else if(pairClip.Key=="slots")// attachment object
						{
							Dictionary<string,object> temp1=(Dictionary<string,object>)pairClip.Value;
							
							foreach(KeyValuePair<string,object> pair2 in temp1)
							{
								AEPSlotAnimationElement slotAnim=Pathfinding.Serialization.JsonFx.JsonReader.Deserialize(
									Pathfinding.Serialization.JsonFx.JsonWriter.Serialize(pair2.Value), typeof(AEPSlotAnimationElement)) as AEPSlotAnimationElement;
								slotAnim.name=pair2.Key;
								dicSlotAttactment[slotAnim.name]=slotAnim;
							}
							//Debug.LogError("Helu:"+Pathfinding.Serialization.JsonFx.JsonWriter.Serialize(dicSlotAttactment));
						}
					}
					AEPAnimationClipFile clipInfo=new AEPAnimationClipFile(clipName,dicAnimation,dicSlotAttactment);
					listAnimationClip.Add(clipInfo);
				}
			}
		}

		public SpineBoneElelement GetBoneElement(string boneName)
		{
			SpineBoneElelement element=null;
			dicBones.TryGetValue(boneName, out element);
			return element;
		}
		public string GetFullPathBone(string boneName)
		{
			Dictionary<string,SpineBoneElelement> dic=dicBones;
			string path="";
			SpineBoneElelement bone=null;
			if(dic.TryGetValue(boneName,out bone))
			{
				while(true)
				{
					if(bone.parent==null)
					{
						path+="";
						break;
					}
					else
					{
						if(path.Length<1)
							path=bone.name;
						else
							path=bone.name+"/"+path;
						if(!dic.TryGetValue(bone.parent,out bone))
						{
							break;
						}
					}
				}
			}
			return path;
		}

		public void JsonFinal()
		{

		}
	} 
}
