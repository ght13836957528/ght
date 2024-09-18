using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
#if UNITY_4_0_0 ||UNITY_4_0 || UNITY_4_0_1||UNITY_4_1||UNITY_4_2||UNITY_4_3||UNITY_4_4||UNITY_4_5||UNITY_4_6||UNITY_4_7||UNITY_4_8||UNITY_4_9
	namespace OnePStudio.SpineToUnity4
#else
	namespace OnePStudio.SpineToUnity5
#endif
{
	public class SpineSkinnedMeshData
	{
		public string name;
		public string renderTextureName;
		public int[] bones;
		public float[] weights, uvs, regionUVs;
		public int[] triangles;
		public List<Transform> skinBone;
		public BoneWeight[] boneWeight;// bang so vertex
		//Matrix4x4[] bindPoses;// bang so bone 
		//public float regionOffsetX, regionOffsetY, regionWidth, regionHeight, regionOriginalWidth, regionOriginalHeight;
		public float r = 1, g = 1, b = 1, a = 1;
		public int hull;

		public float u1=0,v1=0,u2=0,v2=0;
		public bool isRotate;
		public Vector2 scale;
		//public float m00,m01,m11,m10;

		private bool isInit=false;

		public bool CheckIsInit()
		{
			return isInit;
		}
		// Nonessential.
		public int[] Edges { get; set; }
		public float Width { get; set; }
		public float Height { get; set; }
		
		public SpineSkinnedMeshData (string name,SpineSpriteElemnt element,SpineAttachmentElelement attachment,GameObject obj)
		{
			try
			{
				this.skinBone=new List<Transform>();
				this.name=name;
				this.renderTextureName=element.textureName;
				this.isRotate=element.isRotate;
				this.u1=element.u;
				this.u2=element.u2;
				this.v1=element.v;
				this.v2=element.v2;
				this.hull=attachment.hull*2;
				
				triangles=attachment.triangles;
				regionUVs = attachment.uvs;

				scale=new Vector2(attachment.scaleX,attachment.scaleY);
				float[] vertices = attachment.vertices;
				List<float> _weights = new List<float>();//(vertices.Length*3);
				List<int> _bones = new List<int>();//(vertices.Length);
				for (int i = 0, n = vertices.Length; i < n; ) {
					int boneCount = (int)vertices[i];
					i++;
					_bones.Add(boneCount);
					for (int nn = i + boneCount * 4; i < nn; ) {
						_bones.Add((int)vertices[i]);
						_weights.Add(vertices[i + 1] );//* scale);
						_weights.Add(vertices[i + 2] );//* scale);
						_weights.Add(vertices[i + 3]);
						i += 4;
					}
				}
				bones = _bones.ToArray();
				weights = _weights.ToArray();
				UpdateUVs();
				/*
				float radians = 0 * (float)Math.PI / 180;
				float cos = (float)Math.Cos(radians);
				float sin = (float)Math.Sin(radians);
				float worldScaleX=1;
				float worldScaleY=1;
				m00 = cos * worldScaleX;
				m01 = -sin * worldScaleY;

				m10 = -sin * worldScaleX;
				m11 = -cos * worldScaleY;*/
				/*if (worldFlipX) {
					m00 = -cos * worldScaleX;
					m01 = sin * worldScaleY;
				} else {
					m00 = cos * worldScaleX;
					m01 = -sin * worldScaleY;
				}
				if (worldFlipY != yDown) {
					m10 = -sin * worldScaleX;
					m11 = -cos * worldScaleY;
				} else {
					m10 = sin * worldScaleX;
					m11 = cos * worldScaleY;
				}*/
				isInit=true;

			}
			catch(System.Exception ex)
			{
				isInit=false;
				Debug.LogError("#145 Skin Mesh Data Error:"+name+","+ex.Message);
			}
		}
		
		public void UpdateUVs () 
		{
			float u = u1;
			float v = v1;
			float width = u2 - u1;
			float height = v2 - v1;
			float[] regionUVs = this.regionUVs;
			if (this.uvs == null || this.uvs.Length != regionUVs.Length) 
			{
				this.uvs = new float[regionUVs.Length];
			}
			float[] uvs = this.uvs;

			if (isRotate) {
				for (int i = 0, n = uvs.Length; i < n; i += 2) {
					uvs[i] = u + regionUVs[i + 1] * width;
					uvs[i + 1] = v + height - regionUVs[i] * height;
				}
			} else {
				for (int i = 0, n = uvs.Length; i < n; i += 2) {
					uvs[i] = u + regionUVs[i] * width;
					uvs[i + 1] = v + regionUVs[i + 1] * height;
				}
			}
		}


		public void ComputeWorldVertices (ref float[] worldVertices,GameObject obj,SpineToNativeUnityAnimation aep) {
			float[] weights = this.weights;
			int[] bones = this.bones;
			int x=0;
			int y=0;
			int w=0;
		
			boneWeight=new BoneWeight[worldVertices.Length/2];
			//UnityEngine.Debug.Log("tong so bone:"+this.bones.Length+","+this.weights.Length+","+obj.name);
			for (int /*w = 0,*/ v = 0, b = 0, n = bones.Length; v < n; w += 2) {
				float wx = 0, wy = 0;
				int nn = bones[v++] + v;
				int ii=0;
				for (; v < nn; v++, b += 3) {
					float vx = weights[b];
					float vy = weights[b + 1];
					float weight = weights[b + 2];
					GameObject bone = null;
					aep.dicBoneObject.TryGetValue(bones[v],out bone);
					if(bone!=null)
					{
						int index=0;
						if(!skinBone.Contains(bone.transform))
						{
							skinBone.Add(bone.transform);
							index=skinBone.Count-1;
						}
						else
						{
							for(int cx=0;cx<skinBone.Count;cx++)
							{
								if(skinBone[cx]==bone)
								{
									index=cx;
									break;
								}
							}
						}
						if(ii==0)
						{
							boneWeight[w/2].weight0=weight;
							boneWeight[w/2].boneIndex0=index;
						}
						else if(ii==1)
						{
							boneWeight[w/2].weight1=weight;
							boneWeight[w/2].boneIndex1=index;
						}
						else if(ii==2)
						{
							boneWeight[w/2].weight2=weight;
							boneWeight[w/2].boneIndex2=index;
						}
						else if(ii==3)
						{
							boneWeight[w/2].weight3=weight;
							boneWeight[w/2].boneIndex3=index;
						}
						Vector3 objLocation=obj.transform.position;
						//float objRotateZ=obj.transform.rotation.eulerAngles.z;
						Vector3 location=bone.transform.position;

						//objLocation=Vector3.zero;
						//objRotateZ=0;
						//Debug.LogError("Cho nay ne:"+bone.name+":"+bone.transform.rotation+","+objRotateZ);

						float radians = (bone.transform.rotation.eulerAngles.z) * (float)Math.PI / 180;
						float cos = (float)Math.Cos(radians);
						float sin = (float)Math.Sin(radians);
						float worldScaleX=1;
						float worldScaleY=1;
						float m00 = cos * worldScaleX;
						float m01 = -sin * worldScaleY;
						float m10 = sin * worldScaleX;
						float m11 = cos * worldScaleY;
						wx += (vx * m00 + vy * m01 + location.x-objLocation.x) * weight;
						wy += (vx * m10 + vy * m11 + location.y-objLocation.y) * weight;

					}
					else
					{
						wx += vx*weight;
						wy += vy*weight;
					}
					ii++;
				}
				worldVertices[w] = wx + x;
				worldVertices[w + 1] = wy + y;
			}
			//Debug.LogError(Pathfinding.Serialization.JsonFx.JsonWriter.Serialize(worldVertices));
		}
	}
}