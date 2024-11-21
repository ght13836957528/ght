using UnityEngine;
using System.Collections;
using System.Reflection;
using System;
#if UNITY_4_0_0 ||UNITY_4_0 || UNITY_4_0_1||UNITY_4_1||UNITY_4_2||UNITY_4_3||UNITY_4_4||UNITY_4_5||UNITY_4_6||UNITY_4_7||UNITY_4_8||UNITY_4_9
namespace OnePStudio.SpineToUnity4
#else
namespace OnePStudio.SpineToUnity5
#endif
{
	public enum TangentMode
	{
		Editable = 0,
		Smooth = 1,
		Linear = 2,
		Stepped = Linear | Smooth,
		Quadratic=11,
		Cubic=12,
	}
	
	public enum TangentDirection
	{
		Left,
		Right
	}
	
	public class SpineCurveUtil
	{
		public static void AddKey(ref AnimationCurve curve, Keyframe keyframe, TangentMode tangleMode)
		{
			var keys = curve.keys;
			
			//Early out - if this is the first key on this curve just add it
			if (keys.Length == 0)
			{
				curve.AddKey(keyframe);
				return;
			}
			
			//Get the last keyframe
			Keyframe lastKeyframe = keys[keys.Length - 1];
			

			switch (tangleMode)
			{
				case TangentMode.Stepped:
				{
					lastKeyframe.outTangent = 0;
					curve.MoveKey(keys.Length - 1, lastKeyframe);
					
					keyframe.inTangent = float.PositiveInfinity;
					curve.AddKey(keyframe);
				}
				break;
				case TangentMode.Quadratic:
				{
					//Increase to cubic
					var c1 = (2 * lastKeyframe.value) / 3;
					var c2 = 1 - (2 * lastKeyframe.value + 1) / 3;
					
					//Convert [0,1] into unity-acceptable tangents
					c1 *= 3 * (keyframe.value - lastKeyframe.value) / (keyframe.time - lastKeyframe.time);
					c2 *= 3 * (keyframe.value - lastKeyframe.value) / (keyframe.time - lastKeyframe.time);
					
					//Set the out tangent for the previous frame and update
					lastKeyframe.outTangent = c1;
					curve.MoveKey(keys.Length - 1, lastKeyframe);
					
					//Set the in tangent for the current frame and add
					keyframe.inTangent = c2;
					curve.AddKey(keyframe);
					break;
				}
					
				case TangentMode.Cubic:
				{
					//Get curve parameters
					var c1 = lastKeyframe.value;
					var c2 = 1 - lastKeyframe.value;
					
					//Convert [0,1] into unity-acceptable tangents
					c1 *= 3 * (keyframe.value - lastKeyframe.value) / (keyframe.time - lastKeyframe.time);
					c2 *= 3 * (keyframe.value - lastKeyframe.value) / (keyframe.time - lastKeyframe.time);
					
					//Set the out tangent for the previous frame and update
					lastKeyframe.outTangent = c1;
					curve.MoveKey(keys.Length - 1, lastKeyframe);
					
					//Set the in tangent for the current frame and add
					keyframe.inTangent = c2;
					curve.AddKey(keyframe);
					break;
				}
				default:
				{
					var val = (keyframe.value - lastKeyframe.value) / (keyframe.time - lastKeyframe.time);
					lastKeyframe.outTangent = val;
					curve.MoveKey(keys.Length - 1, lastKeyframe);
					
					keyframe.inTangent = val;
					curve.AddKey(keyframe);
					break;
				}
			}
		}
		
		/// <summary>
		/// Add the specified key and set the in/out tangents for a linear curve
		/// </summary>
		public static void AddLinearKey(ref AnimationCurve curve, Keyframe keyframe)
		{
			var keys = curve.keys;
			//Second or later keyframe - make the slopes linear
			if (keys.Length > 0)
			{
				var lastFrame = keys[keys.Length - 1];
				float slope = (keyframe.value - lastFrame.value) / (keyframe.time - lastFrame.time);
				lastFrame.outTangent = keyframe.inTangent = slope;
				
				//Update the last keyframe
				curve.MoveKey(keys.Length - 1, lastFrame);
			}
			
			//Add the new frame
			curve.AddKey(keyframe);
		}
	}
	public class KeyframeUtil {

		public static TangentMode GetTangleMode(string str)
		{
			if(str.ToLower().Contains("stepped"))
			{
				return TangentMode.Stepped;
			}
			else if(str.ToLower().Contains("editable"))
			{
				return TangentMode.Editable;
			}
			else if(str.ToLower().Contains("Smooth"))
			{
				return TangentMode.Smooth;
			}
			else
			{
				return TangentMode.Linear;
			}
		}

		public static Keyframe GetNew( float time, float value, TangentMode leftAndRight){
			return GetNew(time,value, leftAndRight,leftAndRight);
		}
		
		public static Keyframe GetNew(float time, float value, TangentMode left, TangentMode right){
			object boxed = new Keyframe(time,value); // cant use struct in reflection			
			
			SetKeyBroken(boxed, true);
			SetKeyTangentMode(boxed, 0, left);
			SetKeyTangentMode(boxed, 1, right);
			
			Keyframe keyframe = (Keyframe)boxed;
			if (left == TangentMode.Stepped )
				keyframe.inTangent = float.PositiveInfinity;
			if (right == TangentMode.Stepped )
				keyframe.outTangent = float.PositiveInfinity;
			
			return keyframe;
		}

		public static Keyframe GetNew2(float time, float value, TangentMode left, TangentMode right){
			object boxed = new Keyframe(time,value); // cant use struct in reflection			
			
			SetKeyBroken(boxed, true);
			SetKeyTangentMode(boxed, 0, left);
			SetKeyTangentMode(boxed, 1, right);
			
			Keyframe keyframe = (Keyframe)boxed;
			if (left == TangentMode.Stepped )
				keyframe.inTangent = float.PositiveInfinity;
			if (right == TangentMode.Stepped )
				keyframe.outTangent = float.PositiveInfinity;
			
			return keyframe;
		}
		
		
		// UnityEditor.CurveUtility.cs (c) Unity Technologies
		public static void SetKeyTangentMode(object keyframe, int leftRight, TangentMode mode)
		{
			
			Type t = typeof( UnityEngine.Keyframe );
			FieldInfo field = t.GetField( "m_TangentMode", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance );
			int tangentMode =  (int)field.GetValue(keyframe);
			
			if (leftRight == 0)
			{
				tangentMode &= -7;
				tangentMode |= (int) mode << 1;
			}
			else
			{
				tangentMode &= -25;
				tangentMode |= (int) mode << 3;
			}
			
			field.SetValue(keyframe, tangentMode);
			if (GetKeyTangentMode(tangentMode, leftRight) == mode)
				return;
			Debug.Log("bug"); 
		}
		
		// UnityEditor.CurveUtility.cs (c) Unity Technologies
		public static TangentMode GetKeyTangentMode(int tangentMode, int leftRight)
		{
			if (leftRight == 0)
				return (TangentMode) ((tangentMode & 6) >> 1);
			else
				return (TangentMode) ((tangentMode & 24) >> 3);
		}
		
		// UnityEditor.CurveUtility.cs (c) Unity Technologies
		public static TangentMode GetKeyTangentMode(Keyframe keyframe, int leftRight)
		{
			Type t = typeof( UnityEngine.Keyframe );
			FieldInfo field = t.GetField( "m_TangentMode", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance );
			int tangentMode =  (int)field.GetValue(keyframe);
			if (leftRight == 0)
				return (TangentMode) ((tangentMode & 6) >> 1);
			else
				return (TangentMode) ((tangentMode & 24) >> 3);
		}
		
		
		// UnityEditor.CurveUtility.cs (c) Unity Technologies
		public static void SetKeyBroken(object keyframe, bool broken)
		{
			Type t = typeof( UnityEngine.Keyframe );
			FieldInfo field = t.GetField( "m_TangentMode", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance );
			int tangentMode =  (int)field.GetValue(keyframe);
			
			if (broken)
				tangentMode |= 1;
			else
				tangentMode &= -2;
			field.SetValue(keyframe, tangentMode);
		}
		// UnityEditor.CurveUtility.cs (c) Unity Technologies
		public static bool isKeyBroken(object keyframe){
			Type t = typeof( UnityEngine.Keyframe );
			FieldInfo field = t.GetField( "m_TangentMode", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance );
			int tangentMode =  (int)field.GetValue(keyframe);
			return (tangentMode & 1) != 0;
		}
		
	}
	public static class CurveExtension {
		
		public static void UpdateAllLinearTangents(this AnimationCurve curve){
			for (int i = 0; i < curve.keys.Length; i++) {
				UpdateTangentsFromMode(curve, i);
			}
		}
		public static void setSteppedInterval(AnimationCurve curve, int i, int nextI){
			
			if (curve.keys[i].value == curve.keys[nextI].value){
				return;
			}
			
			object thisKeyframeBoxed = curve[i];
			object nextKeyframeBoxed = curve[nextI];
			
			if (!KeyframeUtil.isKeyBroken(thisKeyframeBoxed))
				KeyframeUtil.SetKeyBroken(thisKeyframeBoxed, true);
			if (!KeyframeUtil.isKeyBroken(nextKeyframeBoxed))
				KeyframeUtil.SetKeyBroken(nextKeyframeBoxed, true);
			
			KeyframeUtil.SetKeyTangentMode(thisKeyframeBoxed, 1, TangentMode.Stepped);
			KeyframeUtil.SetKeyTangentMode(nextKeyframeBoxed, 0, TangentMode.Stepped);
			
			Keyframe thisKeyframe = (Keyframe)thisKeyframeBoxed;
			Keyframe nextKeyframe = (Keyframe)nextKeyframeBoxed;
			thisKeyframe.outTangent = float.PositiveInfinity;
			nextKeyframe.inTangent  = float.PositiveInfinity;
			curve.MoveKey(i, 	 thisKeyframe);
			curve.MoveKey(nextI, nextKeyframe);
		}
		
		
		public static void setLinearInterval(AnimationCurve curve, int i, int nextI){
			Keyframe thisKeyframe = curve[i];
			Keyframe nextKeyframe = curve[nextI];
			thisKeyframe.outTangent = CurveExtension.CalculateLinearTangent(curve, i, nextI);
			nextKeyframe.inTangent = CurveExtension.CalculateLinearTangent(curve, nextI, i);
			
			KeyframeUtil.SetKeyBroken((object)thisKeyframe, true);
			KeyframeUtil.SetKeyBroken((object)nextKeyframe, true);
			
			KeyframeUtil.SetKeyTangentMode((object)thisKeyframe, 1, TangentMode.Linear);
			KeyframeUtil.SetKeyTangentMode((object)nextKeyframe, 0, TangentMode.Linear);
			
			
			curve.MoveKey(i, 	 thisKeyframe);
			curve.MoveKey(nextI, nextKeyframe);
		}
		public static void UpdateCurveLinear(AnimationCurve curve)
		{
			var listKey=curve.keys;
			for(int i=0;i<listKey.Length-1;i++)
			{
				setLinearInterval(curve,i,i+1);
			}
		}
		
		// UnityEditor.CurveUtility.cs (c) Unity Technologies
		public static void UpdateTangentsFromMode(AnimationCurve curve, int index)
		{
			if (index < 0 || index >= curve.length)
				return;
			Keyframe key = curve[index];
			if (KeyframeUtil.GetKeyTangentMode(key, 0) == TangentMode.Linear && index >= 1)
			{
				key.inTangent = CalculateLinearTangent(curve, index, index - 1);
				curve.MoveKey(index, key);
			}
			if (KeyframeUtil.GetKeyTangentMode(key, 1) == TangentMode.Linear && index + 1 < curve.length)
			{
				key.outTangent = CalculateLinearTangent(curve, index, index + 1);
				curve.MoveKey(index, key);
			}
			if (KeyframeUtil.GetKeyTangentMode(key, 0) != TangentMode.Smooth && KeyframeUtil.GetKeyTangentMode(key, 1) != TangentMode.Smooth)
				return;
			curve.SmoothTangents(index, 0.0f);
		}
		
		// UnityEditor.CurveUtility.cs (c) Unity Technologies
		private static float CalculateLinearTangent(AnimationCurve curve, int index, int toIndex)
		{
			return (float) (((double) curve[index].value - (double) curve[toIndex].value) / ((double) curve[index].time - (double) curve[toIndex].time));
		}
		
	}
	public static class AnimationCurveUtility 
	{
		public enum TangentMode
		{
			Editable,
			Smooth,
			Linear,
			Stepped
		}
		
		public enum TangentDirection
		{
			Left,
			Right
		}
		
		public static void SetLinear( ref AnimationCurve curve )
		{
			Type t = typeof( UnityEngine.Keyframe );
			FieldInfo field = t.GetField( "m_TangentMode", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance );
			
			for ( int i = 0; i < curve.length; i++ )
			{
				object boxed = curve.keys[ i ]; // getting around the fact that Keyframe is a struct by pre-boxing
				field.SetValue( boxed, GetNewTangentKeyMode( ( int ) field.GetValue( boxed ), TangentDirection.Left, TangentMode.Linear ) );
				field.SetValue( boxed, GetNewTangentKeyMode( ( int ) field.GetValue( boxed ), TangentDirection.Right, TangentMode.Linear ) );
				curve.MoveKey( i, ( Keyframe ) boxed );
				curve.SmoothTangents( i, 0f );
			}
		}
		
		public static int GetNewTangentKeyMode( int currentTangentMode, TangentDirection leftRight, TangentMode mode )
		{
			int output = currentTangentMode;
			
			if ( leftRight == TangentDirection.Left )
			{
				output &= -7;
				output |= ( ( int ) mode ) << 1;
			}
			else
			{
				output &= -25;
				output |= ( ( int ) mode ) << 3;
			}
			return output;
		}
		
	}
}