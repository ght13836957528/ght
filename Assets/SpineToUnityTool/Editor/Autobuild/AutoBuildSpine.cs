using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Pathfinding.Serialization.JsonFx;
using System.Reflection;

#if UNITY_4_0_0 ||UNITY_4_0 || UNITY_4_0_1||UNITY_4_1||UNITY_4_2||UNITY_4_3||UNITY_4_4||UNITY_4_5||UNITY_4_6||UNITY_4_7||UNITY_4_8||UNITY_4_9
using OnePStudio.SpineToUnity4;
#else
using OnePStudio.SpineToUnity5;
#endif
public class InputFileOption
{
	public string fileName;
	public string path;
	public bool isSelect;
	public InputFileOption(string _fileName,string _path,bool _isSelect)
	{
		isSelect=_isSelect;
		fileName=_fileName;
		path=_path;
	}
}
public class AutoBuildSpine : EditorWindow 
{
	public string pathInput="";
	public string pathOutput="";
	internal static AutoBuildSpine instance;
	[SerializeField]
	public List<InputFileOption> listShowUI=new List<InputFileOption>();
	private Vector2 mScroll=Vector2.zero;
	[SerializeField]
	private bool showInputFile;
	[MenuItem("Window/SpineToUnity/ExportSpine")]
	public static void CreateWindow()
	{
		instance = GetWindow<AutoBuildSpine>();
		instance.title = "Auto Build Spine";
		instance.minSize = new Vector2(380, 250);
		instance.Show();
		instance.ShowUtility();
		instance.autoRepaintOnSceneChange = false;
		instance.wantsMouseMove = false;
		instance.listShowUI.Clear();
		if(instance.pathInput.Length>0)
		{
			DirectoryInfo dInfo = new DirectoryInfo(instance.pathInput);
			DirectoryInfo[] subdirs = dInfo.GetDirectories();
			//string currentPath=Application.dataPath;
			for(int x=0;x<subdirs.Length;x++)
			{
				instance.listShowUI.Add(new InputFileOption(subdirs[x].Name,subdirs[x].FullName,true));
			}
		}
		//Application.RegisterLogCallback(instance.LogCallback);
		
	}

	static public void DrawHeader (string text)
	{
		GUILayout.Space(3f);
		GUILayout.BeginHorizontal();
		GUILayout.Space(3f);
		
		GUI.changed = false;
		
		text = "<b><size=11>" + text + "</size></b>";
		
		text = "\u25BC " + text;
		GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f));
		GUILayout.Space(2f);
		GUILayout.EndHorizontal();
	}
	static public void BeginContents ()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Space(4f);
		EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
		GUILayout.BeginVertical();
		GUILayout.Space(2f);
	}
	static public void BeginContentsMaxHeight ()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Space(4f);
		EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MaxHeight(20000f));
		GUILayout.BeginVertical();
		GUILayout.Space(2f);
	}
	static public void EndContents ()
	{
		GUILayout.Space(3f);
		GUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(3f);
		GUILayout.EndHorizontal();
		GUILayout.Space(3f);
	}

	public void OnGUI()
	{
		if(pathInput.Length>0&&listShowUI.Count<1)
		{
			UpdateListUI(); 
		} 
		EditorGUILayout.BeginVertical();

		DrawHeader("Choose Input Folder");
		BeginContents ();
		EditorGUILayout.BeginHorizontal();
		{
			GUI.color = Color.white;
			if(pathInput.Length<1)
			{
				EditorGUILayout.HelpBox("No Spine Input! Please Choose Directory Folder Path Spine!", MessageType.Warning,true);
			}
			else
			{
				EditorGUILayout.HelpBox("Directory Choose: "+ pathInput, MessageType.Info,true);
			}
			if(GUILayout.Button("Choose Input\n Directory", new GUILayoutOption[]{GUILayout.Width(150),GUILayout.Height(37f)}))
			{
				ChooseInputPathFolder();
			}
		}
		EditorGUILayout.EndHorizontal();

		EndContents();

		DrawHeader("Choose OutPut Folder");
		BeginContents ();
		EditorGUILayout.BeginHorizontal();
		{
			GUI.color = Color.white;
			if(pathOutput.Length<1)
			{
				EditorGUILayout.HelpBox("No Output Export Folder, Please Choose Directory Folder to Export!", MessageType.Warning,true);
			}
			else
			{
				EditorGUILayout.HelpBox("Directory Choose: "+ pathOutput, MessageType.Info,true);;
			}
			if(GUILayout.Button("Choose Output\n Directory", new GUILayoutOption[]{GUILayout.Width(150),GUILayout.Height(37f)}))
			{
				ChooseOutputPath();
			}
		}
		EditorGUILayout.EndHorizontal();
		EndContents();

		EditorGUILayout.BeginHorizontal();
		{
			GUI.color = Color.green; 
			if(GUILayout.Button("EXPORT", new GUILayoutOption[]{GUILayout.Height(40f),GUILayout.MaxWidth(2000.0f)}))
			{
				Export();
			}
		}
		EditorGUILayout.EndHorizontal();
		GUIShowFolderInput();
		EditorGUILayout.EndVertical();
	}

	void GUIShowFolderInput()
	{
		GUI.color = Color.cyan;  
		DrawHeader("Sprites");
		GUI.color = Color.white; 
		
		BeginContentsMaxHeight();
		mScroll = GUILayout.BeginScrollView(mScroll);
		
		EditorGUILayout.BeginHorizontal();
		{
			showInputFile=EditorGUILayout.Foldout(showInputFile,"Total Folder Input ("+listShowUI.Count+")");
		}
		if(GUILayout.Button("SelectAll", new GUILayoutOption[]{GUILayout.MaxWidth(80.0f)}))
		{
			for(int i=0;i<listShowUI.Count;i++)
			{
				listShowUI[i].isSelect=true;
			}
		}
		if(GUILayout.Button("UnSelect", new GUILayoutOption[]{GUILayout.MaxWidth(80.0f)}))
		{
			for(int i=0;i<listShowUI.Count;i++)
			{
				listShowUI[i].isSelect=false;
			}
		}
		EditorGUILayout.EndHorizontal();
		
		if(showInputFile)
		{
			for(int i=0;i<listShowUI.Count;i++)
			{
				EditorGUILayout.BeginHorizontal();
				string name="null ";
				if(listShowUI[i]!=null)
				{
					name=listShowUI[i].fileName;
				}
				EditorGUILayout.LabelField("\t"+(i+1).ToString()+" : "+name,EditorStyles.boldLabel);
				if(!listShowUI[i].isSelect)
				{
					GUI.color=Color.red;
					EditorGUILayout.LabelField("Not Build",GUILayout.Width(60));
					GUI.color=Color.green;
					if(GUILayout.Button("+", new GUILayoutOption[]{GUILayout.Width(30.0f)}))
					{
						listShowUI[i].isSelect=true;
					}
					GUI.color=Color.white;
				}
				else
				{
					EditorGUILayout.LabelField("Build it!",GUILayout.Width(60));
					GUI.color=Color.white;
					if(GUILayout.Button("-", new GUILayoutOption[]{GUILayout.Width(20.0f)}))
					{
						listShowUI[i].isSelect=false;
					}
					GUI.color=Color.white;
				}
				EditorGUILayout.EndHorizontal();
			}
		}
		GUILayout.EndScrollView();
		EndContents();
	}

	public void UpdateListUI()
	{
		listShowUI.Clear();
		if(pathInput.Length>0)
		{
			DirectoryInfo dInfo = new DirectoryInfo(pathInput);
			DirectoryInfo[] subdirs = dInfo.GetDirectories();
			//string currentPath=Application.dataPath;
			for(int x=0;x<subdirs.Length;x++)
			{
				listShowUI.Add(new InputFileOption(subdirs[x].Name,subdirs[x].FullName,true));
			}
		}
	}

	#region Choose Input Folder Image
	private void ChooseInputPathFolder()
	{
		pathInput= EditorUtility.OpenFolderPanel("Directory Input Spine","","");
		//Debug.LogError(pathImages);
		string currentPath=Application.dataPath;
		if(pathInput.Length<1)
		{
			pathInput="";
		}
		else if(pathInput.Length>0&&!pathInput.Contains(currentPath))
		{
			pathInput="";
			EditorUtility.DisplayDialog("Error","Please Choose Folder Images inside Project","OK");
		}
		UpdateListUI();
	}
	#endregion 


	#region Choose Output Path Folder
	private void ChooseOutputPath()
	{
		pathOutput= EditorUtility.OpenFolderPanel("Directory Export File","","");
		//Debug.LogError(pathImages);
		string currentPath=Application.dataPath;
		if(pathOutput.Length<1)
		{
			pathOutput="";
		}
		else if(pathOutput.Length >0&&!pathOutput.Contains(currentPath))
		{
			pathOutput="";
			EditorUtility.DisplayDialog("Error","Please Choose Folder output inside Project","OK");
			
		}
	}
	#endregion 


	#region Export

	public void Export()
	{
		//Debug.LogError("Export");
		if(pathInput.Length<1||pathOutput.Length<1)
		{
			EditorUtility.DisplayDialog("Error","Please Choose Input & Output Folder","OK");
			return;
		}
		List<InputFileOption> listFinal=new List<InputFileOption>();
		for(int i=0;i<listShowUI.Count;i++)
		{
			if(listShowUI[i].isSelect)
			{
				listFinal.Add(listShowUI[i]);
			}
		}
		if(listFinal.Count<1)
		{
			EditorUtility.DisplayDialog("Error","No folder spine in input folder","OK");
			return;
		}

		//DirectoryInfo dInfo = new DirectoryInfo(pathInput);
		//DirectoryInfo[] subdirs = dInfo.GetDirectories();
		string currentPath=Application.dataPath;
		for(int x=0;x<listFinal.Count;x++)
		{
			string pathNow="Assets"+listFinal[x].path.Replace(currentPath,"");
			string name=listFinal[x].fileName;
			string newPath=pathOutput+"/"+name;
			string pathAsset="Assets"+newPath.Replace(Application.dataPath,"");
			newPath=pathAsset;
			//Debug.LogError(pathNow);
			//Debug.LogError(subdirs[x]);
			string[] file=Directory.GetFiles(pathNow);
			List<Texture2D> listTexture=new List<Texture2D>();
			TextAsset json=null;
			TextAsset atlas=null;
			for(int i=0;i<file.Length;i++)
			{
				UnityEngine.Object obj=AssetDatabase.LoadAssetAtPath(file[i],typeof(UnityEngine.Object));
				if(obj!=null)
				{
					if(obj is Texture2D)
					{
						listTexture.Add((Texture2D)obj);
					}
					if(obj is TextAsset)
					{
						string str=AssetDatabase.GetAssetPath(obj);
						if(str.Contains(".json"))
						{
							json=(TextAsset) obj;
						}
						else if(str.Contains(".atlas.txt"))
						{
							atlas=(TextAsset) obj;
						}
					}
				}
			}
			if(listTexture.Count>0
			   &&json!=null&&atlas!=null)
			{
				if(Directory.Exists(newPath))
				{
					Directory.Delete(newPath,true);
				}
				Directory.CreateDirectory(newPath);
				AssetDatabase.ImportAsset(newPath);
				AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);  
				bool result=SpineToNativeUnityAnimation.BuildByParam(listTexture,json,atlas, name,newPath,0.25f,100);
				if(result==false)
				{
					Debug.LogError("#91 Can not export Folder:"+pathNow);
				}
			}
		}
		
		EditorUtility.DisplayDialog("Info","Finish","OK");
	}
	#endregion
}