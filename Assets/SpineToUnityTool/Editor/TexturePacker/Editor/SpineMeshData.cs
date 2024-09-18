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
	public class SpineMeshData 
	{
		public string name;
		public string renderTextureName;
		public int[] bones;
		public float[] vertices, uvs, regionUVs;
		public int[] triangles;
		//public float regionOffsetX, regionOffsetY, regionWidth, regionHeight, regionOriginalWidth, regionOriginalHeight;
		public float r = 1, g = 1, b = 1, a = 1;
		public int hull;
		
		public float u1=0,v1=0,u2=0,v2=0;
		public bool isRotate;
		public Vector2 scale;
		
		private bool isInit=false;
		
		public bool CheckIsInit()
		{
			return isInit;
		}
		// Nonessential.
		public int[] Edges { get; set; }
		public float Width { get; set; }
		public float Height { get; set; }
		
		public SpineMeshData (string name,SpineSpriteElemnt element,SpineAttachmentElelement attachment)
		{
			try
			{
				this.name=name;
				this.renderTextureName=element.textureName;
				this.isRotate=element.isRotate;
				this.u1=element.u;
				this.u2=element.u2;
				this.v1=element.v;
				this.v2=element.v2;
				this.hull=attachment.hull*2;
				
				this.triangles=attachment.triangles;
				this.regionUVs = attachment.uvs;
				this.scale=new Vector2(attachment.scaleX,attachment.scaleY);
				this.vertices = attachment.vertices;
				UpdateUVs();
				this.isInit=true;
			}
			catch(System.Exception ex)
			{
				isInit=false;
				Debug.LogError("#439 Mesh Data Error:"+name+","+ex.Message);
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
		
		public void ComputeWorldVertices (float[] worldVertices) {
			float[] vertices = this.vertices;
			int verticesCount = vertices.Length;
			for (int i = 0; i < verticesCount-1; i += 2) {
				float vx = vertices[i];
				float vy = vertices[i + 1];
				worldVertices[i] = vx;// vx+ vy +x;
				worldVertices[i + 1]= vy;//vx +vy+ y;
			}
		}
	}
}
