using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Pathfinding.Serialization.JsonFx;
using System.Reflection;
//#if UNITY_4_6
using UnityEngine.UI;
//#endif

#if UNITY_4_0_0 ||UNITY_4_0 || UNITY_4_0_1||UNITY_4_1||UNITY_4_2||UNITY_4_3||UNITY_4_4||UNITY_4_5||UNITY_4_6||UNITY_4_7||UNITY_4_8||UNITY_4_9
using OnePStudio.SpineToUnity4;
#else
using OnePStudio.SpineToUnity5;
#endif

public class SpineToNativeUnityAnimation : EditorWindow 
{
	private const int DATE_EXPITE=2118*365+10*30+1;// 12
	private const string VERION_NEXT="2.6"; 
	internal static SpineToNativeUnityAnimation instance;

	[SerializeField]
	public int tabBuildAtlas=0;

	[SerializeField]
	public string pathImages=""; 
	public SpriteType buildSpriteType=SpriteType.SpriteRenderer; 

	[SerializeField]
	public float scaleTexture=1;

	[SerializeField]
	public float defaultScaleInScene=0.01f;
		
	[SerializeField]
	public int defaultSortDepthValue=0;

	[SerializeField]
	public bool isGeneratePrefab=false;
	
	[SerializeField]
	public TrimType trimType=TrimType.TrimMinimum;

	[SerializeField]
	public ExportCurveType exportCurveType=ExportCurveType.InterpolateAngle;

	[SerializeField]
	public int fps=30;
	// Main Texture Atlas
	public Texture2D mainTexture;

	[SerializeField]
	public List<Texture2D> listAtlasSpine=new List<Texture2D>(){null};
	[SerializeField]
	TextAsset rawAnimationJson=null;


	[SerializeField]
	TextAsset rawAtlasData=null;


	private AllSpineAtlasInfo spineAtlasInfo=null;
	private const string DEFAULT_OUTPUT="Assets/Spine Output";
	private string pathOutputs=DEFAULT_OUTPUT; 
	private bool showAnimationFiles=true;
	private Vector2 mScroll=Vector2.zero;

	[SerializeField]
	private string aepName="Spine Animation";

	[SerializeField]
	private string atlasName="Spine_Atlas";


	[SerializeField]
	public bool autoOverride=true;
	[SerializeField]
	public SortLayerType sortType=SortLayerType.Depth;

	[SerializeField]
	public float zfactor=0.1f;


	private int tabBuild=0;

	private float scaleInfo=100;
	private GameObject objectRoot;

	// control Variable
	private List<DataAnimAnalytics> listInfoFinal=new List<DataAnimAnalytics>();
	private List<Texture2D> listTextureBuildAtlas=new List<Texture2D>();
	private List<string> listPathImageOutput=new List<string>();
	[HideInInspector]
	public Dictionary<int,GameObject> dicBoneObject=new Dictionary<int, GameObject>();
	public List<Transform> listBoneTransform=new List<Transform>();
	private Dictionary<string,Material> dicMaterial=new Dictionary<string, Material>();
	private GameObject generatePrefab=null;

	#if   !DEFINE_NGUI&&(UNITY_4_0_0 ||UNITY_4_0 || UNITY_4_0_1||UNITY_4_1||UNITY_4_2||UNITY_4_3||UNITY_4_4||UNITY_4_5||UNITY_4_6||UNITY_4_7||UNITY_4_8||UNITY_4_9)
	[MenuItem("Window/SpineToUnity/Spine2Unity 4")]
	#else
	[MenuItem("Window/SpineToUnity/Spine2Unity 5")]
	#endif
	public static void CreateWindow()
	{
		instance = GetWindow<SpineToNativeUnityAnimation>();
		instance.title = "Spine2Unity";
		instance.minSize = new Vector2(380, 450);
		instance.Show();
		instance.ShowUtility();
		instance.autoRepaintOnSceneChange = false;
		instance.wantsMouseMove = false;
		//Application.RegisterLogCallback(instance.LogCallback);

	}
	public static bool BuildByParam(List<Texture2D>listTexture,TextAsset json, TextAsset atlas,string name,string newPath,float scaleTexure,float scaleInfo)
	{
		if(instance==null)
		{
			CreateWindow();
		}
		instance.listAtlasSpine=listTexture;
		instance.rawAnimationJson=json;
		instance.rawAtlasData=atlas;
		instance.aepName=name;
		instance.pathOutputs=newPath;
		instance.scaleTexture=scaleTexure;
		instance.scaleInfo=scaleInfo;
		instance.isGeneratePrefab=true;
		instance.sortType=SortLayerType.Depth;
		return instance.BuildFull();
	}
	void OnGUI()
	{
		if (instance == null) CreateWindow();
		DrawToolbar();
	}  

	public bool CheckNewUpdate()
	{
		
		System.DateTime dateNow=System.DateTime.Now;
		int nowInt=dateNow.Year*365+dateNow.Month*30+dateNow.Day;
		//Debug.LogError (nowInt + "," + dateNow.Month + "," + DATE_EXPITE);
		int saveDate=EditorPrefs.GetInt("SPINE2Unity_EXPIRE",-1);
		if(saveDate<0||saveDate<nowInt)
		{
			saveDate=nowInt;
			EditorPrefs.SetInt("SPINE2Unity_EXPIRE",saveDate);
		}
		if(saveDate>DATE_EXPITE)
		{
			Debug.LogError("New Update Available");
			return true; 
		}
		else
		{
			return false;
		}
	}

	#region Change Path Folder Image
	private void ChangePathImage()
	{
		pathImages= EditorUtility.OpenFolderPanel("Directory All Fire","","");
		//Debug.LogError(pathImages);
		string currentPath=Application.dataPath;
		if(pathImages.Length<1)
		{
			pathImages="";
		}
		else if(!pathImages.Contains(currentPath))
		{
			pathImages="";
			EditorUtility.DisplayDialog("Error","Please Choose Folder Images inside Project","OK");

		}
		else
		{
			pathImages="Assets"+pathImages.Replace(currentPath,"");
			//Debug.LogError(pathImages);
			string[] file=Directory.GetFiles(pathImages);
			bool hasTextureInside=false;
			for(int i=0;i<file.Length;i++)
			{
				//Debug.LogError(file[i]);
				UnityEngine.Object obj=AssetDatabase.LoadAssetAtPath(file[i],typeof(UnityEngine.Object));
				if(obj!=null)
				{
					if(obj is Texture2D)
					{
						hasTextureInside=true;
					}
				}
			}
			if(!hasTextureInside)
			{
				pathImages="";
				EditorUtility.DisplayDialog("Error","Can not found any image texture in this folder","OK");
			}
		}
	}
	#endregion 

	#region Choose directory output
	private void ChooseOutput()
	{
		pathOutputs= EditorUtility.OpenFolderPanel("Choose Directory For  Output Data","","");
		//Debug.LogError(pathOutputs);
		string currentPath=Application.dataPath;
		if(pathOutputs.Length<1)
		{
			pathOutputs=DEFAULT_OUTPUT;
		}
		else if(!pathOutputs.Contains(currentPath))
		{
			pathOutputs=DEFAULT_OUTPUT;
			EditorUtility.DisplayDialog("Error","Please Choose Dicrectory Folder inside Project","OK");
			
		}
		else
		{
			pathOutputs="Assets"+pathOutputs.Replace(currentPath,"");
		}
	}
	#endregion

	#region GUI Show Editor Choose OR Create Atlas
	private void GUIShowEditorChooseOrCreateAtlas()
	{
		GUI.color = Color.magenta; 
		GUILayout.Label("SETTING IMAGES ATLAS BUILDER:",EditorStyles.boldLabel); 
		GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(2)});
		GUI.color = Color.white;
		tabBuildAtlas = GUILayout.Toolbar(tabBuildAtlas, new string[] {"Spine Import"});
		if(tabBuildAtlas==0) 
		{
			GUIShowEditorChooseSpine();
		}
		else if(tabBuildAtlas==1) 
		{
			//GUI.color = Color.magenta;  
			//GUILayout.Label("CREATE TEXTURE PACKER IMAGE",EditorStyles.boldLabel);
			GUI.color = Color.white;
			GUIShowEditorCreateAtlas();
		}
		else
		{
			GUIShowEditorChooseAtlas();
		}
		GUI.color = Color.magenta; 
		GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(2)});
		GUI.color = Color.white;
	}
	#endregion

	#region GUIShowEditorChooseSpine
	private void GUIShowEditorChooseSpine()
	{
		GUI.color = Color.white;

		rawAtlasData=EditorGUILayout.ObjectField("Atlas Info file: ",rawAtlasData, typeof(TextAsset),true,GUILayout.MaxWidth(1000f)) as TextAsset;

		EditorGUILayout.BeginHorizontal();
		{
			showAnimationFiles=EditorGUILayout.Foldout(showAnimationFiles,"List Spine Atlas Texture");
		}
		EditorGUILayout.EndHorizontal();
		
		if(showAnimationFiles)
		{

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("\t",GUILayout.Width(20));
			int size=listAtlasSpine.Count;
			size=Mathf.Clamp(EditorGUILayout.IntField("Size:",size,GUILayout.MaxWidth(1000f)), 0, 50);
			if(size<0)
			{
				size=0;
			}
			if(size!=listAtlasSpine.Count)
			{
				if(size==0)
				{
					listAtlasSpine.Clear();
				}
				else
				{
					if(size>listAtlasSpine.Count)
					{
						for(int i=listAtlasSpine.Count;i<size;i++)
						{
							listAtlasSpine.Add(null);
						}
					}
					else
					{
						int total=listAtlasSpine.Count;
						for(int i=size;i<total;i++)
						{
							listAtlasSpine.RemoveAt(listAtlasSpine.Count-1);
						}
					}
				}
			}
			EditorGUILayout.EndHorizontal();
			for(int i=0;i<listAtlasSpine.Count;i++)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("\t",GUILayout.Width(20));
				listAtlasSpine[i]=EditorGUILayout.ObjectField("Atlas Texture "+(i+1).ToString(),listAtlasSpine[i], typeof(Texture2D),true,GUILayout.MaxWidth(1000f)) as Texture2D;
				EditorGUILayout.EndHorizontal();
			}
			scaleTexture=EditorGUILayout.FloatField("Ratio Scale Texture:",scaleTexture,GUILayout.MaxWidth(1000f));
			if(scaleTexture<0.01f)
			{
				scaleTexture=0.01f;
			}

		}
	}
	#endregion

	#region GUIShowEditorChooseAtlas
	private void GUIShowEditorChooseAtlas()
	{
		EditorGUILayout.BeginHorizontal();
		{
			if(mainTexture!=null)
			{
				string path=AssetDatabase.GetAssetPath(mainTexture);
				TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
				if(ti.textureType!=TextureImporterType.Sprite|| ti.spriteImportMode!=SpriteImportMode.Multiple)
				{
					EditorUtility.DisplayDialog("Texture Input Warning","Please choose Texture Sprite in multiple mode","OK");
					mainTexture=null;
				}
			}

			GUI.color = Color.white;
			if(mainTexture==null)
			{
				EditorGUILayout.HelpBox("Atlas Texture Sprite:\n(only allow Texture Sprite in Sprite Mode Multiple)\n", MessageType.Warning);
			}
			else
			{
				EditorGUILayout.HelpBox("Atlas Texture Sprite:\n(only allow Texture Sprite in Sprite Mode Multiple)\n", MessageType.Info);
			}
			if(mainTexture==null)
			{
				GUI.color=Color.red;
			}
			else
			{
				GUI.color=Color.white;
			}
			mainTexture = EditorGUILayout.ObjectField("",mainTexture, typeof(Texture2D),true,new GUILayoutOption[]{GUILayout.Width(70.0f),GUILayout.Height(70.0f)}) as Texture2D;

		}
		EditorGUILayout.EndHorizontal();

	}
	#endregion

	#region GUIShowEditorCreateAtlas
	private void GUIShowEditorCreateAtlas()
	{
		if(pathImages.Length<1)
		{
			EditorGUILayout.BeginHorizontal();
			{
				GUI.color = Color.red;
				EditorGUILayout.HelpBox("No Images Choose ! Please Choose \nDirectory Folder Path that contain all texture !", MessageType.Warning,true);
				if(GUILayout.Button("Open Directory", GUILayout.Height(37f)))
				{
					ChangePathImage();
				}
			}
			EditorGUILayout.EndHorizontal();
		}
		else
		{
			EditorGUILayout.BeginHorizontal();
			{
				GUI.color = Color.white;
				EditorGUILayout.HelpBox("Directory Choose: "+ pathImages, MessageType.Info,true);
				//GUI.color = Color.yellow;
				if(GUILayout.Button("Change Directory", GUILayout.Height(37f)))
				{
					ChangePathImage();
				}

			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Separator();
			EditorGUILayout.BeginHorizontal();
			{
				GUI.color = Color.white;
				EditorGUILayout.LabelField("Build Texture Atlas Type:",EditorStyles.boldLabel);
				trimType= (TrimType)EditorGUILayout.EnumPopup(trimType, "DropDown", new GUILayoutOption[]{GUILayout.Width(150f),GUILayout.Height(20)});
			}
			EditorGUILayout.EndHorizontal();
		}
	}
	#endregion

	#region Show Animation File Input
	private void GUIShowAnimationInfo()
	{
		GUI.color = Color.magenta;  
		GUILayout.Label("INPUT ANIMATIONS JSON FILE:",EditorStyles.boldLabel);
		GUI.color = Color.white;
		GUI.color = Color.white;
		rawAnimationJson= EditorGUILayout.ObjectField("Spine Json File:",rawAnimationJson,typeof(TextAsset),false,GUILayout.MaxWidth(2000)) as TextAsset;
	}
	#endregion

	private SpriteType cacheType=SpriteType.SpriteRenderer;
	#region Show OutputData
	private void ShowOutputData()
	{
		GUI.color = Color.magenta;  
		GUILayout.Label("OUTPUT SETTING:",EditorStyles.boldLabel);
		GUI.color = Color.white;
		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.HelpBox("Choose Directory for all data output !\nCurrent: "+pathOutputs, MessageType.None,true);
			if(GUILayout.Button("Change Directory", new GUILayoutOption[]{GUILayout.Height(30f),GUILayout.Width(150.0f)}))
			{
				ChooseOutput();
			}
		}
		EditorGUILayout.EndHorizontal();

		fps=EditorGUILayout.IntField(new GUIContent("Samples FrameRate Animation","default is 30"),fps,GUILayout.MaxWidth(2000));
		if(fps<1)
		{
			fps=1;
		}
		if(fps>60)
		{
			fps=60;
		}
		//exportCurveType=(ExportCurveType)EditorGUILayout.EnumPopup(new GUIContent("Setting export animation:","default is 30"),exportCurveType,GUILayout.MaxWidth(2000));

		buildSpriteType=(SpriteType)EditorGUILayout.EnumPopup(new GUIContent("Export Layer Type","Object Generate Scale, Default is 100"),buildSpriteType,GUILayout.MaxWidth(2000));
		if (buildSpriteType == SpriteType.NGUI) {
			EditorGUILayout.HelpBox("To export NGUI, please make sure you had already import NGUI plugins", MessageType.Warning,true);
		}
		if (buildSpriteType == SpriteType.SpriteRenderer) {
			scaleInfo=EditorGUILayout.FloatField(new GUIContent("Scale SpriteRenderer","Object Generate Scale, Default is 100"),scaleInfo,GUILayout.MaxWidth(2000));

		} else if(buildSpriteType == SpriteType.UGUI)
		{
			scaleInfo=EditorGUILayout.FloatField(new GUIContent("Scale UGUI Image","Object Generate Scale, Default is 1"),scaleInfo,GUILayout.MaxWidth(2000));
		}
		else{

			scaleInfo=EditorGUILayout.FloatField(new GUIContent("Scale NGUI Image","Object Generate Scale, Default is 1"),scaleInfo,GUILayout.MaxWidth(2000));
		}


		if(scaleInfo<0||cacheType != buildSpriteType) {
			cacheType=buildSpriteType;
			if (buildSpriteType == SpriteType.SpriteRenderer) {
				scaleInfo = 100;
			} else {
				scaleInfo = 1;
			}
		}
		defaultScaleInScene=EditorGUILayout.FloatField(new GUIContent("Scale Object In Scene","Default is 1"),defaultScaleInScene,GUILayout.MaxWidth(2000));
		if(defaultScaleInScene<0)
		{
			defaultScaleInScene=0.001f;
		}
		EditorGUILayout.Separator();
		defaultSortDepthValue=EditorGUILayout.IntField(new GUIContent("Default Depth Sorting Layer ","Default is 0"),defaultSortDepthValue,GUILayout.MaxWidth(2000));
		aepName=EditorGUILayout.TextField("Name Object Create",aepName,GUILayout.MaxWidth(2000));
		if(aepName.Length==0)
		{
			aepName="AEP Animation";
		}
		isGeneratePrefab=EditorGUILayout.Toggle(new GUIContent("Create Prefab",""),isGeneratePrefab,GUILayout.MaxWidth(2000));

		if(tabBuildAtlas==1)
		{
			atlasName=EditorGUILayout.TextField("Name Atlas Image Create ",atlasName,GUILayout.MaxWidth(2000));
			if(atlasName.Length==0)
			{
				atlasName="AEP_Atlas";
			}
		}
		EditorGUILayout.BeginHorizontal();
			autoOverride=EditorGUILayout.Toggle("Auto Override Output Data",autoOverride,GUILayout.MaxWidth(2000));
			EditorGUILayout.BeginVertical();
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("Sort Layer By",GUILayout.Width(75));
				sortType=(SortLayerType)EditorGUILayout.EnumPopup(sortType,GUILayout.MaxWidth(100));
				EditorGUILayout.EndHorizontal();
				
				if (sortType == SortLayerType.Z) {
					EditorGUILayout.BeginHorizontal();
					GUILayout.Label("Z Factor",GUILayout.Width(75));

					zfactor=EditorGUILayout.FloatField(new GUIContent("","Default is 0.1f"),zfactor,GUILayout.MaxWidth(100));
					/*if (zfactor <= 0) {
						zfactor = 0.1f;
					}*/
				EditorGUILayout.EndHorizontal();
				}
			EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();

	}
	#endregion

	#region Show Setting Build Animation
	private void ShowSettingBuildAnimation()
	{
		GUI.color = Color.magenta;  
		GUILayout.Label("BUILD SETTING",EditorStyles.boldLabel);
		GUI.color = Color.white; 
		tabBuild = GUILayout.Toolbar(tabBuild, new string[] {"Auto Build", "Custom Build"});
		if(tabBuild==0) 
		{
			GUI.color = Color.green; 
			if(GUILayout.Button("BUILD", new GUILayoutOption[]{GUILayout.Height(40f),GUILayout.MaxWidth(2000.0f)}))
			{
				bool result=false;
				result=BuildSprite();
				if(result)
				{
					if (buildSpriteType == SpriteType.NGUI) {
						result=CreateBoneSkeletonNGUI ();
					} else {
						result=CreateBoneSkeleton ();
					}
				}
				if(result)
				{
					result=BuildAnimation();
				}
				if(result)
				{
					EditorUtility.DisplayDialog("Finish","Skeleton "+aepName+" had created in scene, all files reference in "+pathOutputs,"OK");
				}
			}
		}
		else
		{
			GUI.color = Color.green; 
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("Read Input Info", new GUILayoutOption[]{GUILayout.Height(40f),GUILayout.MaxWidth(2000.0f)}))
			{
				ReadInfo();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("Build Atlas Sprite", new GUILayoutOption[]{GUILayout.Height(40f),GUILayout.MaxWidth(2000.0f)}))
			{
				BuildSprite();
			}
			if(GUILayout.Button("Build Skeleton", new GUILayoutOption[]{GUILayout.Height(40f),GUILayout.MaxWidth(2000.0f)}))
			{
				if (buildSpriteType == SpriteType.NGUI) {
					CreateBoneSkeletonNGUI ();
				} else {
					CreateBoneSkeleton ();
				}
			}
			if(GUILayout.Button("Build Animation", new GUILayoutOption[]{GUILayout.Height(40f),GUILayout.MaxWidth(2000.0f)}))
			{
				BuildAnimation();
			}
			EditorGUILayout.EndHorizontal();
		}
	}
	#endregion
	public bool BuildFull()
	{
		bool result=false;
		result=BuildSprite();
		if(result)
		{
			if (buildSpriteType == SpriteType.NGUI) {
				result=CreateBoneSkeletonNGUI ();
			} else {
				result=CreateBoneSkeleton ();
			}
		}
		if(result)
		{
			result=BuildAnimation();
		}
		return result;
	}
	void DrawToolbar()
	{
		mScroll = GUILayout.BeginScrollView(mScroll);
		EditorGUILayout.BeginVertical();
		GUI.color = Color.green;  
		GUILayout.Label("CONVERT SPINE TO NATIVE UNITY ANIMATION",EditorStyles.boldLabel);
		GUI.color = Color.magenta; 
		GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(2)});

		GUI.color = Color.white;
		GUIShowEditorChooseOrCreateAtlas();
		GUIShowAnimationInfo();
		GUI.color = Color.magenta; 
		GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(2)});
		GUI.color = Color.white;
		ShowOutputData();
		GUI.color = Color.magenta; 
		GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(2)});
		ShowSettingBuildAnimation();
		          
		EditorGUILayout.EndVertical();
		GUILayout.EndScrollView();

	}
	void Test()
	{
		AnimationClip anim=AssetDatabase.LoadAssetAtPath(pathOutputs+"/idle.anim",typeof(AnimationClip)) as AnimationClip;
		if(anim==null)
		{
			Debug.LogError("#33 Anim Null");
		}
		else
		{
			AnimationClipCurveData[] clipData= AnimationUtility.GetAllCurves(anim,true);
			//Debug.LogError(JsonWriter.Serialize(clipData));
			EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings (anim);
			//Debug.LogError(JsonWriter.Serialize(curveBindings));
			for(int i=0;i<curveBindings.Length;i++)
			{
				AnimationCurve curve=AnimationUtility.GetEditorCurve(anim,curveBindings[i]);
				//Debug.LogError(curveBindings[i].path+","+curveBindings[i].propertyName+"-->"+JsonWriter.Serialize(curve));
			}
		}
	}

	#region Read Info
	private bool ReadInfo(bool isShowError=true)
	{
		if(CheckNewUpdate())
		{
			EditorUtility.ClearProgressBar();
			if(EditorUtility.DisplayDialog("Require Update","New free update version are avaible, Please update to continue !","OK")){
				Application.OpenURL("http://u3d.as/kq7");
				return false;
			}
		}
		//Test();
		//return false;
		//Debug.Log("ReadInfo");
		bool error=false;

		if(rawAnimationJson==null)
		{
			error=true;
		}
		else
		{
			listInfoFinal.Clear();
			TextAsset textAsset=rawAnimationJson;
			if(textAsset!=null)
			{
				//try
				{
					#region File Anim
					string json=textAsset.text;
					string filename=AssetDatabase.GetAssetPath(textAsset);
					int index=filename.LastIndexOf("/");
					filename=filename.Substring(index+1,filename.Length-index-6);// subject .json


					AEPJsonRaw jsonRaw = null;
					try
					{
						jsonRaw = JsonReader.Deserialize(json, typeof(AEPJsonRaw)) as AEPJsonRaw;
					}
					catch(Exception ex)
					{
						AEPJsonRawV2 jsonRaw2=JsonReader.Deserialize(json, typeof(AEPJsonRawV2)) as AEPJsonRawV2;
						jsonRaw=new AEPJsonRaw();
						jsonRaw.bones = jsonRaw2.bones;
						jsonRaw.animations = jsonRaw2.animations;
						jsonRaw.slots = jsonRaw2.slots;
						jsonRaw.skins =new Dictionary<string, object>();
						jsonRaw.skins["default"] = jsonRaw2.skins[0]["attachments"];
					}

					AEPJsonFinal jsonFinal=new AEPJsonFinal(jsonRaw);
					listInfoFinal.Add(new DataAnimAnalytics(jsonFinal,filename,AnimationStyle.Normal));
					#endregion

				}
				/*catch(System.Exception ex)
				{
					Debug.LogError("Error:"+ex.Message);
				}*/
				if(listInfoFinal.Count<1)
				{
					error=true;
				}
			}
		}
		if(tabBuildAtlas==0)
		{
			List<Texture2D> listAtlasOk=new List<Texture2D>();
			for(int i=0;i<listAtlasSpine.Count;i++)
			{
				if(listAtlasSpine[i]!=null)
				{
					listAtlasOk.Add(listAtlasSpine[i]);
				}
			}
			spineAtlasInfo=new AllSpineAtlasInfo();
			if(rawAtlasData!=null)
			{
				string text=rawAtlasData.text;
				if(!spineAtlasInfo.Init(text,listAtlasOk))
				{
					error=true;
				}
			}
			else
			{
				error=true;
			}
		}
		if(error)
		{
			if(isShowError)
			{
				EditorUtility.DisplayDialog("Error Input Data","Data Animation is not correct format !","OK");
			}
			return false;
		}
		return true;
	}
	#endregion

	#region Build SpriteRenderer
	private bool BuildSprite()
	{
		Debug.Log("BuildSprite");
		if(!Directory.Exists(pathOutputs))
		{
			Directory.CreateDirectory(pathOutputs);	
		}
		ReadInfo(false);
		if (CheckNewUpdate ()) {
			return false;
		}
		dicMaterial.Clear();
		if(tabBuildAtlas==0)
		{
			if(spineAtlasInfo==null||spineAtlasInfo.listInfoImageAtlas.Count<1)
			{
				EditorUtility.DisplayDialog("Error","Atlas Info is empty !","OK");
				return false;
			}
			if(listAtlasSpine.Count==0)
			{
				EditorUtility.DisplayDialog("Error","No Spine Atlas Choose !","OK");
				return false;
			}
			List<Texture2D> listAtlasOk=new List<Texture2D>();
			for(int i=0;i<listAtlasSpine.Count;i++)
			{
				if(listAtlasSpine[i]!=null)
				{
					listAtlasOk.Add(listAtlasSpine[i]);
				}
			}
			if(listAtlasOk.Count<1)
			{
				EditorUtility.DisplayDialog("Error","No Spine Atlas Choose !","OK");
				return false;
			}
			else
			{
				bool checkOk=true;
				for(int i=0;i<listAtlasOk.Count;i++)
				{
					if(!spineAtlasInfo.listInfoImageAtlas.ContainsKey(listAtlasOk[i].name))
					{
						checkOk=false;
						break;
					}
				}
				if(!checkOk)
				{
					EditorUtility.DisplayDialog("Error","Atlas Info dont have information for Spine Atlas Input !","OK");
					return false;
				}
				else
				{
					bool isUseMesh=false;
					if(listInfoFinal!=null)
					{
						for(int i=0;i<listInfoFinal.Count;i++)
						{
							if(listInfoFinal[i].jsonFinal.isUseMesh)
							{
								isUseMesh=true;
								break;
							}
						}
					}
					for(int i=0;i<listAtlasOk.Count;i++)
					{
						SpineAtlasInfoInOneImage info=spineAtlasInfo.listInfoImageAtlas[listAtlasOk[i].name];
						string output=pathOutputs+"/"+listAtlasOk[i].name+".png";
						OptimizeSprite.OnProcessBuildSpineSprite(listAtlasOk[i],info,output,scaleTexture);
						bool result=TexturePacker.UpdateAtlasSpriteInfo(output,listInfoFinal,scaleTexture);
						listPathImageOutput.Add(output);

						string outputMat=pathOutputs+"/"+listAtlasOk[i].name+".mat";
						if(isUseMesh)
						{
							Shader shader = Shader.Find("Sprites/Default");
							Material mat=new Material(shader);
							mat.SetTexture("_MainTex",listAtlasOk[0]);
							AssetDatabase.CreateAsset(mat,outputMat);
							AssetDatabase.Refresh();
							dicMaterial[listAtlasOk[i].name]=mat;
						}
					}
					return true;
				}
			}
		}
		else if(tabBuildAtlas==1)
		{
			if(pathImages.Length<1)
			{
				EditorUtility.DisplayDialog("Error Input Data","Folder directory Path is empty !","OK");
				return false;
			}
			listTextureBuildAtlas.Clear();
			string[] file=Directory.GetFiles(pathImages);
			bool hasTextureInside=false;
			for(int i=0;i<file.Length;i++)
			{
				UnityEngine.Object obj=AssetDatabase.LoadAssetAtPath(file[i],typeof(UnityEngine.Object));
				if(obj!=null)
				{
					if(obj is Texture2D)
					{
						hasTextureInside=true;
						listTextureBuildAtlas.Add((Texture2D)obj);
					}
				}
			}
			if(!hasTextureInside)
			{
				pathImages="";
				EditorUtility.DisplayDialog("Error Input Data","Can not found any image texture in folder directory "+pathImages,"OK");
			}
			string pathAtlas=pathOutputs+"/"+atlasName+".png";
			bool result =TexturePacker.AutoBuildAtlasFromListTexture(listTextureBuildAtlas,listInfoFinal,trimType,pathAtlas);
			if(result && listInfoFinal.Count>0)
			{
				result=TexturePacker.UpdateAtlasSpriteInfo(pathAtlas,listInfoFinal);
			}
			UnityEngine.Object obj2=AssetDatabase.LoadAssetAtPath(pathAtlas,typeof(UnityEngine.Object));
			if(obj2!=null)
			{
				if(obj2 is Texture2D)
				{
					mainTexture=(Texture2D)obj2;
				}
			}
			return result;
		}
		else 
		{
			if(mainTexture==null)
			{
				EditorUtility.DisplayDialog("Error Input Data","Texture Sprite is Null, Please choose text Atlas Sprite first !","OK");
				return false;
			}
			if(listInfoFinal.Count<1)
			{
				EditorUtility.DisplayDialog("Error Input Data","Animation json input files are empty!","OK");
				return false;
			}
			string pathAtlas=AssetDatabase.GetAssetPath(mainTexture);
			bool result=TexturePacker.UpdateAtlasSpriteInfo(pathAtlas,listInfoFinal);
			return result;
		}
	}
	#endregion

	#region Build Bone
	private bool CreateBoneSkeleton()
	{
		bool isCreateMesh=false;
		Debug.Log("CreateBoneSkeleton");
		if(!Directory.Exists(pathOutputs))
		{
			Directory.CreateDirectory(pathOutputs);	
		}
		ReadInfo(false);
		
		if(listInfoFinal.Count<1)
		{
			EditorUtility.DisplayDialog("Error Input Data","Animation Json Files input are empty!","OK");
			return false;
		}
		string pathAtlas=pathOutputs+"/"+atlasName+".png";

		if(tabBuildAtlas==2)
		{
			if(mainTexture==null)
			{
				EditorUtility.DisplayDialog("Error Input Data","Texture Sprite is Null, Please choose text Atlas Sprite first !"+pathImages,"OK");
				return false;
			}
			pathAtlas=AssetDatabase.GetAssetPath(mainTexture);
		}
		Dictionary<string,Sprite> dicSprite=new Dictionary<string, Sprite>();
		dicBoneObject.Clear();
		listBoneTransform.Clear();
		if(tabBuildAtlas!=0)
		{
			Sprite[] spritesTemp = AssetDatabase.LoadAllAssetsAtPath(pathAtlas)
			.OfType<Sprite>().ToArray();
			for(int i=0;i<spritesTemp.Length;i++)
			{
				dicSprite[spritesTemp[i].name]=spritesTemp[i];
			}
		}
		else
		{
			for(int x=0;x<listPathImageOutput.Count;x++)
			{
				Sprite[] spritesTemp = AssetDatabase.LoadAllAssetsAtPath(listPathImageOutput[x])
					.OfType<Sprite>().ToArray();
				for(int i=0;i<spritesTemp.Length;i++)
				{
					dicSprite[spritesTemp[i].name]=spritesTemp[i];
				}
			}
		}
		GameObject root=null;
		GameObject canvasUI = null;
		GameObject transObj = null;
		if (buildSpriteType == SpriteType.UGUI) {
			canvasUI=GameObject.Find("CanvasUI");
			if(canvasUI==null){
				canvasUI=new GameObject();
				canvasUI.name="CanvasUI";
				Canvas canvas=canvasUI.AddComponent<Canvas>();
				canvas.renderMode=RenderMode.WorldSpace;
				CanvasScaler canvasScaler=canvasUI.AddComponent<CanvasScaler>();
				canvasScaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPhysicalSize;
				canvasUI.AddComponent<GraphicRaycaster>();
			}
			if (canvasUI != null) {
				transObj = GameObject.Find (aepName);
			}
		}
#if DEFINE_NGUI_OLD
		else if (buildSpriteType == SpriteType.NGUI) {
			canvasUI=GameObject.Find("2DUI");
			if(canvasUI==null){
				canvasUI=new GameObject();
				canvasUI.name="2DUI";
				UIPanel canvas=canvasUI.AddComponent<UIPanel>();
				UIRoot rootUGUI=canvasUI.AddComponent<UIRoot>();
				rootUGUI.scalingStyle = UIRoot.Scaling.Flexible;
				rootUGUI.minimumHeight = 320;
				rootUGUI.manualHeight = 4096;
				Rigidbody regidbody=canvasUI.AddComponent<Rigidbody>();
				regidbody.mass=1;
				regidbody.angularDrag=0.05f;
				regidbody.isKinematic=true;
				regidbody.angularDrag=0;
				regidbody.collisionDetectionMode=CollisionDetectionMode.Discrete;
				regidbody.interpolation=RigidbodyInterpolation.None;
				//add camera
//				Camera camera=canvasUI.GetComponentInChildren<Camera>();
//				if(camera==null){
//					Debug.LogError("Here");
//					GameObject obj=new GameObject();
//					obj.transform.SetParent(canvasUI.transform);
//					obj.transform.localPosition=Vector3.zero;
//					obj.transform.localScale=Vector3.one;
//					camera=obj.AddComponent<Camera>();
//					camera.orthographic=true;
//					UICamera uiCamera=obj.AddComponent<UICamera>();
//				}
			}
			if (canvasUI != null) {
				if(canvasUI.transform.Find (aepName)!=null)
				{
					transObj = canvasUI.transform.Find (aepName).gameObject;
				}
				if(transObj==null)
				{
					transObj = GameObject.Find (aepName);
				}
			}
		}
#endif
		else {
			transObj= GameObject.Find(aepName);
		}
		
		#region remove Object
		if(transObj!=null)
		{
			if(!autoOverride)
			{
				if(!EditorUtility.DisplayDialog("Warning","Object "+aepName+" has exist in scene,Do you want replace this file?","YES","NO"))
				{
					return false;
				}
				else
				{

					if(transObj.transform.parent!=null&&(transObj.transform.parent.name!="2DUI"&&transObj.transform.parent.name!="CanvasUI"))
					{
						//Debug.LogError(transObj.transform.parent.name);
						GameObject.DestroyImmediate(transObj.transform.parent.gameObject);
					}
					else
					{
						GameObject.DestroyImmediate(transObj);
					}
					transObj=null;
				}
			}
			else
			{
				if(transObj.transform.parent!=null&&(transObj.transform.parent.name!="2DUI"&&transObj.transform.parent.name!="CanvasUI"))
				{
					//Debug.LogError(transObj.transform.parent.name);
					GameObject.DestroyImmediate(transObj.transform.parent.gameObject);
				}
				else
				{
					GameObject.DestroyImmediate(transObj);
				}
				transObj=null;
			}
		}
		#endregion
		Dictionary<string,GameObject> cache=new Dictionary<string, GameObject>();

		for(int x=0;x<listInfoFinal.Count;x++)
		{
			transObj= GameObject.Find(aepName);
			DataAnimAnalytics dataAnimAnalytics=listInfoFinal[x%listInfoFinal.Count];

			#region Build New Bone
			for(int i=0;i<dataAnimAnalytics.jsonFinal.bones.Count;i++)
			{
				SpineBoneElelement bone=dataAnimAnalytics.jsonFinal.bones[i];
				if(bone.parent==null)
				{
					if(transObj==null)
					{
						GameObject obj=new GameObject();
						if(buildSpriteType==SpriteType.UGUI)
						{
							obj.transform.parent=canvasUI.transform;
							obj.AddComponent<RectTransform>();
						}
						#if DEFINE_NGUI_OLD
						else if(buildSpriteType==SpriteType.NGUI)
						{
							obj.transform.SetParent(canvasUI.transform);
							obj.transform.localScale=Vector3.one;
						}
						#endif
						obj.transform.localPosition=new Vector3(0,0,0);
						obj.name=bone.name;
						cache[bone.name]=obj;
						root=obj;
						objectRoot=root;
						if(aepName.Length>0)
						{
							obj.name=aepName;
						}
						//Debug.LogError(bone.index+","+listBoneTransform.Count);
						if(!listBoneTransform.Contains(obj.transform))
						{
							listBoneTransform.Add(obj.transform);
						}
						dicBoneObject[bone.index]=obj;
					}
				}
				else 
				{
					GameObject parent=null;
					if(cache.TryGetValue(bone.parent,out parent))
					{
						GameObject objReference=null;
						cache.TryGetValue(bone.name, out objReference);
						if(objReference==null)// append new
						{
							GameObject obj=new GameObject();
							obj.transform.parent=parent.transform;
							obj.transform.localPosition=new Vector3(bone.x,bone.y,0);
							obj.transform.localScale=new Vector3(bone.scaleX,bone.scaleY,1);
							Quaternion quad=obj.transform.localRotation;
							quad.eulerAngles=new Vector3(0,0,bone.rotation);
							obj.transform.localRotation=quad;
							obj.name=bone.name;
							if(buildSpriteType==SpriteType.UGUI)
							{
								RectTransform rect=obj.AddComponent<RectTransform>();
								int subIndex=bone.index_ugui;
								int boneNowIndex=0;
								for(int cx=0;cx<parent.transform.childCount;cx++){
									Transform eleTran=parent.transform.GetChild(cx);
									SpineBoneElelement eleBone=null;
									if(dataAnimAnalytics.jsonFinal.dicBones.TryGetValue(eleTran.name,out eleBone)){
										if(eleBone.index_ugui>subIndex){
											boneNowIndex++;
										}
									}
								}
								//for new UI
								obj.transform.SetSiblingIndex(boneNowIndex);
							}
							cache[bone.name]=obj;
							if(transObj!=null)
							{
								obj.SetActive(false);
							}
							objReference=obj;
							
							//Debug.LogError(bone.index+","+listBoneTransform.Count);
							if(!listBoneTransform.Contains(obj.transform))
							{
								listBoneTransform.Add(obj.transform);
							}
							dicBoneObject[bone.index]=obj;
						}
					}
					else
					{
						Debug.LogWarning("Parent Null: "+bone.name);
					}
				}
			}
			#endregion
			
			Dictionary<string,string> objHideWhenStartAnim=new Dictionary<string, string>();
			Dictionary<string,string> objShowWhenStartAnim=new Dictionary<string, string>();
			Dictionary<string,string> objForAttachment=new Dictionary<string, string>();

			transObj= GameObject.Find(aepName);

			//Debug.LogError(Pathfinding.Serialization.JsonFx.JsonWriter.Serialize(spineAtlasInfo));
			#region Build Attachment
			for(int i=0;i<dataAnimAnalytics.jsonFinal.slots.Count;i++)
			{
				SpineSlotElelement slot=dataAnimAnalytics.jsonFinal.slots[i];

				GameObject obj=new GameObject();
				if(buildSpriteType==SpriteType.UGUI){
					RectTransform rect=obj.AddComponent<RectTransform>();
				}
				GameObject objBone=AEPAnimationClipElement.GetRefenreceObject(dataAnimAnalytics.jsonFinal.GetFullPathBone(slot.bone),transObj);
				if(objBone==null)
				{
					continue;
				}
				obj.transform.parent=objBone.transform;
				obj.transform.localPosition=new Vector3(0,0,0);
				obj.transform.localScale=new Vector3(1,1,1);
				Quaternion quad=obj.transform.localRotation;
				quad.eulerAngles=new Vector3(0,0,0);
				obj.transform.localRotation=quad;
				obj.name=slot.name;
				cache[slot.name]=obj;
				int sibling=0;

				#region add renderer
				for(int k=0;k<slot.listAcceptAttachment.Count;k++)
				{
					SpineSlotAndAttachmentReference reference=slot.listReference[k];
					SpineAttachmentElelement attachElement=slot.listAcceptObj[k];
					string nameInAtlas=slot.mapNameInAtlas[slot.listAcceptAttachment[k]];
					//dataAnimAnalytics.jsonFinal.dicPivot.TryGetValue(nameInAtlas,out attachElement);
					SpineSpriteElemnt spriteElement=spineAtlasInfo.GetSpineSpriteElementByName(nameInAtlas);

					if(attachElement!=null&&spriteElement!=null)
					{
						if(sibling<spriteElement.GetIndex()){
							sibling=spriteElement.GetIndex();
						}
						GameObject obj2=new GameObject();
						obj2.transform.parent=obj.transform;

						obj2.transform.localScale=new Vector3(scaleInfo/scaleTexture*Mathf.Abs(reference.scaleX),scaleInfo/scaleTexture*Mathf.Abs(reference.scaleY),scaleInfo/scaleTexture);
						Quaternion quad2=obj2.transform.localRotation;
						obj2.transform.localPosition=new Vector3(reference.x,reference.y,0);
						obj2.name=slot.listAcceptAttachment[k];

						if(attachElement.GetTypeAttachment()!="mesh"&&attachElement.GetTypeAttachment()!="skinnedmesh")
						{
							if(spriteElement!=null)
							{
								if(!spriteElement.isRotate)
									quad2.eulerAngles=new Vector3(0,0,reference.rotation);
								else
									quad2.eulerAngles=new Vector3(0,0,reference.rotation-90);
								obj2.transform.localRotation=quad2;
								if(buildSpriteType==SpriteType.SpriteRenderer)
								{
									obj2.AddComponent<SpriteRenderer>();
									SpriteRenderer sprite=obj2.GetComponent<SpriteRenderer>();
								
									if(dicSprite.ContainsKey(attachElement.nameInAtlas))	
									{
										sprite.sprite=dicSprite[attachElement.nameInAtlas];
									}
									else
									{
										Debug.LogError("#76 Not found spite:"+attachElement.nameInAtlas);
									}
									if(sortType==SortLayerType.Depth)
									{
										sprite.sortingOrder=defaultSortDepthValue+attachElement.depth;
									}
									else
									{
										sprite.sortingOrder=defaultSortDepthValue;
										float z=zfactor*(-attachElement.depth);
										obj2.transform.localPosition=new Vector3(reference.x,reference.y,z);
									}
									sprite.color=EditorUtil.HexToColor(slot.color);
								}
								#if DEFINE_NGUI_OLD
								else if(buildSpriteType==SpriteType.NGUI)
								{
									UI2DSprite sprite=obj2.AddComponent<UI2DSprite>();
									if(dicSprite.ContainsKey(attachElement.nameInAtlas))	
									{
										Sprite spriteData=dicSprite[attachElement.nameInAtlas];
										sprite.width=(int)spriteData.rect.width;
										sprite.height=(int)spriteData.rect.height;
										Bounds bounds = spriteData.bounds;
										var X = 0;//(-bounds.center.x / bounds.extents.x / 2 + 0.5f )*spriteData.rect.width;
										var Y = 0;;//(-bounds.center.y / bounds.extents.y / 2 + 0.5f)*spriteData.rect.height;
										//Debug.LogError(spriteData.name+","+X+","+Y);

										Vector3 vecObjSprite=sprite.transform.localPosition;
										//sprite.transform.localPosition=new Vector3(X,Y,vecObjSprite.z);
									}
									if(dicSprite.ContainsKey(attachElement.nameInAtlas))	
									{
										sprite.sprite2D=dicSprite[attachElement.nameInAtlas];
									}
									else if(dicSprite.ContainsKey(attachElement.nameInAtlas))	
									{
										sprite.sprite2D=dicSprite[attachElement.nameInAtlas];
									}
									else
									{
										Debug.LogWarning("not found suitable spite:"+attachElement.nameInAtlas);
									}
									if(sortType==SortLayerType.Depth)
									{
										sprite.depth=defaultSortDepthValue-slot.index;
										obj2.transform.localPosition=new Vector3(reference.x,reference.y,obj2.transform.localPosition.z);
									}
									else
									{
										sprite.depth=defaultSortDepthValue;
										float z=0.1f*slot.index;
										Vector3 vecObj=obj2.transform.localPosition;
										obj2.transform.localPosition=new Vector3(vecObj.x,vecObj.y,z);
									}
									sprite.color=EditorUtil.HexToColor(slot.color);
								}
								#endif
								else{
									RectTransform rect=obj2.AddComponent<RectTransform>();
									obj2.AddComponent<Image>();
									Image sprite=obj2.GetComponent<Image>();
									if(dicSprite.ContainsKey(attachElement.nameInAtlas))	
									{
										Sprite spriteData=dicSprite[attachElement.nameInAtlas];
										sprite.sprite=dicSprite[attachElement.nameInAtlas];
										RectTransformExtensions.SetWidth(sprite.rectTransform,spriteData.rect.width);
										RectTransformExtensions.SetHeight(sprite.rectTransform,spriteData.rect.height);
										Bounds bounds = spriteData.bounds;
										var pivotX = - bounds.center.x / bounds.extents.x / 2 + 0.5f;
										var pivotY = - bounds.center.y / bounds.extents.y / 2 + 0.5f;
										Vector2 vecAnchor=new Vector2(pivotX,pivotY);
										sprite.rectTransform.pivot=vecAnchor;
										//sprite.rectTransform.anchoredPosition=spriteData.
										//obj2.transform.localPosition=new Vector3(0,0,0);
										obj2.transform.localPosition=new Vector3(reference.x,reference.y,0);
									}
									else
									{
										Debug.LogError("#76 Not found spite:"+attachElement.nameInAtlas);
									}
									sprite.color=EditorUtil.HexToColor(slot.color);
								}
								string fullPath=EditorUtil.GetFullPathBone(transObj.transform,obj2.transform);
								objForAttachment[fullPath]=fullPath;
								if(slot.attachment!=null&&attachElement.name==slot.attachment)//default hien
								{
									objShowWhenStartAnim[fullPath]=fullPath;
								}
								else
								{
									obj2.SetActive(false);
									objHideWhenStartAnim[fullPath]=fullPath;
								}
							}
						}
						else
						{
							quad2.eulerAngles=new Vector3(0,0,reference.rotation);
							obj2.transform.localRotation=quad2;
							obj2.transform.localScale=Vector3.one;
							//Debug.LogError("Export: "+attachElement.GetTypeAttachment()+","+Pathfinding.Serialization.JsonFx.JsonWriter.Serialize(attachElement));


							int attachmentVertexCount =attachElement.vertices.Length >> 1;
							//int attachmentTriangleCount = attachElement.triangles.Length;
							int meshVertexCount = attachElement.uvs.Length>>1;

							Vector3[] vertices =null;
							Vector2[] uvs=null;
							int[] triangles=null;
							float[] meshUVs=null;
							Color32[] colors=null;
							float[] tempVertices=null;
							BoneWeight[] boneWeight=null;
							List<Transform> boneSkin=null;
							if(attachElement.GetTypeAttachment()=="mesh")
							{
								SpineMeshData meshData=new SpineMeshData(attachElement.nameInAtlas,spriteElement,attachElement);
								if(!meshData.CheckIsInit())
								{
									continue;
								}
								tempVertices = new float[attachElement.vertices.Length];
								meshData.ComputeWorldVertices(tempVertices);


								vertices = new Vector3[attachmentVertexCount];
								uvs= new Vector2[attachmentVertexCount];
								triangles=meshData.triangles;
								meshUVs = meshData.uvs;
								colors=new Color32[attachmentVertexCount];
							}
							else
							{
								meshVertexCount = attachElement.uvs.Length;
								SpineSkinnedMeshData meshSkin=new SpineSkinnedMeshData(attachElement.nameInAtlas,spriteElement,attachElement,obj);
								if(!meshSkin.CheckIsInit())
								{
									continue;
								}

								tempVertices = new float[meshVertexCount];
								meshSkin.ComputeWorldVertices(ref tempVertices,obj2,this);

								meshVertexCount=meshVertexCount/2;
								vertices = new Vector3[meshVertexCount];
								colors=new Color32[meshVertexCount];
								boneWeight=meshSkin.boneWeight;
								boneSkin=meshSkin.skinBone;
								uvs= new Vector2[meshVertexCount];

								triangles=meshSkin.triangles;
								//Debug.LogError(meshSkin.name+","+meshSkin.triangles.Length);
								int testTriangle=0;//meshSkin.triangles.Length;
								//testTriangle=testTriangle-testTriangle%3;
								triangles=new int[meshSkin.triangles.Length-testTriangle];
								for(int xx=0;xx<meshSkin.triangles.Length-testTriangle;xx++)
								{
									triangles[xx]=meshSkin.triangles[xx+testTriangle];
								}

								meshUVs =meshSkin.uvs;
								//Debug.LogError("CCCCdfsfs:"+meshVertexCount);
							}
							Vector3 meshBoundsMin;
							meshBoundsMin.x = float.MaxValue;
							meshBoundsMin.y = float.MaxValue;
							meshBoundsMin.z = 0.1f;//zSpacing > 0f ? 0f : zSpacing * (drawOrderCount - 1);
							Vector3 meshBoundsMax;
							meshBoundsMax.x = float.MinValue;
							meshBoundsMax.y = float.MinValue;
							meshBoundsMax.z = 0.1f;//zSpacing < 0f ? 0f : zSpacing * (drawOrderCount - 1);
							int vertexIndex=0;
							Color32 color=Color.white;

							for (int ii = 0; vertexIndex < meshVertexCount; ii += 2, vertexIndex++)
							{
								vertices[vertexIndex]=new Vector3();
								vertices[vertexIndex].x = tempVertices[ii];
								vertices[vertexIndex].y = tempVertices[ii + 1];//attachElement.vertices[ii + 1];
								vertices[vertexIndex].z = 0;//1*(-attachElement.depth);
								colors[vertexIndex] = color;
								if(meshUVs.Length>ii+1)
								{
									uvs[vertexIndex].x = meshUVs[ii];
									uvs[vertexIndex].y = meshUVs[ii + 1];
								}
								if (tempVertices[ii] < meshBoundsMin.x)
									meshBoundsMin.x = tempVertices[ii];
								else if (tempVertices[ii] > meshBoundsMax.x)
									meshBoundsMax.x = tempVertices[ii];
								if (tempVertices[ii + 1]< meshBoundsMin.y)
									meshBoundsMin.y = tempVertices[ii + 1];
								else if (tempVertices[ii + 1] > meshBoundsMax.y)
									meshBoundsMax.y = tempVertices[ii + 1];
							}
							Mesh mesh = new Mesh();
							mesh.name = "mesh"+attachElement.nameInAtlas;
							mesh.hideFlags = HideFlags.None;
							mesh.MarkDynamic();
							mesh.vertices = vertices;
							mesh.colors32 = colors;
							mesh.uv = uvs;
							mesh.subMeshCount=1;
							mesh.triangles=triangles;
							mesh.SetTriangles(triangles,0);
							if(boneWeight!=null)// for skin mesh
							{

								mesh.boneWeights=boneWeight;
								//Debug.LogError(Pathfinding.Serialization.JsonFx.JsonWriter.Serialize(mesh.boneWeights));
								//Debug.LogError(attachElement.name);
								//Debug.LogError(boneSkin.Count);
								Matrix4x4[] matrix=new Matrix4x4[boneSkin.Count];
								for(int tt=0;tt<matrix.Length;tt++)
								{
									matrix[tt]=boneSkin[tt].worldToLocalMatrix*obj2.transform.localToWorldMatrix;
								}
								//Debug.LogError(matrix.Length);
								mesh.bindposes=matrix;
							}
							Vector3 meshBoundsExtents = meshBoundsMax - meshBoundsMin;
							Vector3 meshBoundsCenter = meshBoundsMin + meshBoundsExtents * 0.5f;
							mesh.bounds = new Bounds(meshBoundsCenter, meshBoundsExtents);
							if(!isCreateMesh)
							{
								isCreateMesh=true;
								if(Directory.Exists(pathOutputs+"/mesh"))
								{
									Directory.Delete(pathOutputs+"/mesh",true);
								}
								Directory.CreateDirectory(pathOutputs+"/mesh");
							}
							string nameMesh=attachElement.nameInAtlas.Replace("/","");
							string pathmesh=pathOutputs+"/mesh/"+nameMesh+".asset";
							int temp=0;
							while(File.Exists(pathmesh))
							{
								temp++;
								pathmesh=pathOutputs+"/mesh/"+nameMesh+temp+".asset";
							}
						//	Debug.LogError("AAAA:"+mesh.bindposes.Length);
							AssetDatabase.CreateAsset(mesh,pathmesh);
							AssetDatabase.Refresh();
							mesh=AssetDatabase.LoadAssetAtPath(pathmesh,typeof(Mesh))as Mesh;
							//float z=0.01f*(-attachElement.depth);
							//obj2.transform.localPosition=new Vector3(reference.x,reference.y,z);
							
							//Debug.LogError("BBBBB"+mesh.bindposes.Length);
							obj2.AddComponent<MeshFilter>();
							MeshFilter meshFilter=obj2.GetComponent<MeshFilter>();
							meshFilter.mesh=mesh;
							if(attachElement.GetTypeAttachment()=="mesh")
							{
								obj2.AddComponent<MeshRenderer>();
								MeshRenderer meshRenderer=obj2.GetComponent<MeshRenderer>();
								meshRenderer.sharedMaterial=dicMaterial[spriteElement.textureName];
								if(sortType==SortLayerType.Depth)
								{
									meshRenderer.sortingOrder=defaultSortDepthValue+attachElement.depth;
								}
								else
								{
									meshRenderer.sortingOrder=defaultSortDepthValue;
									float z=zfactor*(-attachElement.depth);
									obj2.transform.localPosition=new Vector3(reference.x,reference.y,z);
								}
							}
							else
							{
								obj2.AddComponent<SkinnedMeshRenderer>();
								SkinnedMeshRenderer meshRenderer=obj2.GetComponent<SkinnedMeshRenderer>();
								meshRenderer.sharedMaterial=dicMaterial[spriteElement.textureName];
								if(boneSkin!=null)
								{
									Transform[] boneTemp=new Transform[boneSkin.Count];
									for(int cx=0;cx<boneTemp.Length;cx++)
									{
										GameObject objtemp=new GameObject();
										objtemp.transform.parent=obj2.transform;
										objtemp.transform.localScale=Vector3.one;
										objtemp.transform.position=boneSkin[cx].position;
										objtemp.transform.rotation=boneSkin[cx].rotation;
										//objtemp.AddComponent<FollowObject>();
										//objtemp.GetComponent<FollowObject>().SetupFollow(boneSkin[cx].gameObject);
										objtemp.name=boneSkin[cx].name;
										//objtemp.transform.lossyScale=boneSkin[cx].lossyScale;
										boneTemp[cx]=objtemp.transform;
									}
									meshRenderer.bones=boneTemp;
								}
								//Debug.LogError("CCCC:"+meshRenderer.bones.Length);
								if(sortType==SortLayerType.Depth)
								{
									//meshRenderer.sortingOrder=attachElement.depth;
									meshRenderer.sortingOrder=defaultSortDepthValue+attachElement.depth;
								}
								else
								{
									meshRenderer.sortingOrder=defaultSortDepthValue;
									float z=zfactor*(-attachElement.depth);
									obj2.transform.localPosition=new Vector3(reference.x,reference.y,z);
								}
								meshRenderer.sharedMesh=mesh;
#if UNITY_5
								meshRenderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
#endif
								meshRenderer.receiveShadows=false;
								Quaternion quadDefault= obj2.transform.rotation;
								quadDefault.eulerAngles=Vector3.zero;
								obj2.transform.rotation=quadDefault;

							}
							string fullPath=EditorUtil.GetFullPathBone(transObj.transform,obj2.transform);
							objForAttachment[fullPath]=fullPath;
							if(slot.attachment!=null&&attachElement.name==slot.attachment)//default hien
							{
								objShowWhenStartAnim[fullPath]=fullPath;
							}
							else
							{
								obj2.SetActive(false);
								objHideWhenStartAnim[fullPath]=fullPath;
							}
							
						}
					}
					else
					{
						if(spriteElement==null)
						{
							Debug.LogWarning("sprite not found:"+slot.listAcceptAttachment[k]+","+nameInAtlas);
						}
					}
				}
				#endregion

				if(buildSpriteType==SpriteType.UGUI)
				{
					RectTransform rect=obj.AddComponent<RectTransform>();
					int subIndex=sibling;
					int boneNowIndex=0;
					for(int cx=0;cx<objBone.transform.childCount;cx++){
						Transform eleTran=objBone.transform.GetChild(cx);
						SpineBoneElelement eleBone=null;
						if(dataAnimAnalytics.jsonFinal.dicBones.TryGetValue(eleTran.name,out eleBone)){
							if(eleBone.index_ugui>subIndex){
								boneNowIndex++;
							}
						}
					}
					//for new UI
					obj.transform.SetSiblingIndex(boneNowIndex);
				}

			}
			#endregion

			dataAnimAnalytics.AddObjectHideWhenStartAnim(objHideWhenStartAnim);
			dataAnimAnalytics.AddObjectShowWhenStartAnim(objShowWhenStartAnim);
			dataAnimAnalytics.AddObjectForAttachment(objForAttachment);

		}
		if(transObj!=null)
		{
			GameObject objFinal= new GameObject();
			objFinal.name=transObj.name;
			if(buildSpriteType==SpriteType.UGUI)
			{
				objFinal.transform.parent=canvasUI.transform;
				objFinal.AddComponent<RectTransform>();
			}
			#if DEFINE_NGUI_OLD
			else if(buildSpriteType==SpriteType.NGUI)
			{
				objFinal.transform.parent=canvasUI.transform;
			}
			#endif
			objFinal.transform.localPosition=new Vector3(0,0,0);
			objFinal.transform.localScale=Vector3.one;
			transObj.transform.parent=objFinal.transform;
			objFinal.transform.localScale=new Vector3(defaultScaleInScene,defaultScaleInScene,defaultScaleInScene);
			generatePrefab=objFinal;
			//transObj.transform.localScale=new Vector3(defaultScaleInScene,defaultScaleInScene,defaultScaleInScene);
			/*if(isGeneratePrefab)
			{
				string pathPrefab=pathOutputs+"/"+objFinal.name+".prefab";
				PrefabUtility.CreatePrefab(pathPrefab,objFinal);
				
				AssetDatabase.Refresh();
				GameObject.DestroyImmediate(objFinal);
				objFinal= PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath(pathPrefab, typeof(GameObject))) as GameObject;
				transObj=objFinal.transform.Find(objFinal.name).gameObject;
			}*/
		}
		return true;
	}
	#endregion

	#region Build Bone NGUI temp function now
	private bool CreateBoneSkeletonNGUI()
	{
		bool isCreateMesh=false;
		Debug.Log("CreateBoneSkeletonNGUI");
		if(!Directory.Exists(pathOutputs))
		{
			Directory.CreateDirectory(pathOutputs);	
		}
		ReadInfo(false);

		if(listInfoFinal.Count<1)
		{
			EditorUtility.DisplayDialog("Error Input Data","Animation Json Files input are empty!","OK");
			return false;
		}
		string pathAtlas=pathOutputs+"/"+atlasName+".png";

		if(tabBuildAtlas==2)
		{
			if(mainTexture==null)
			{
				EditorUtility.DisplayDialog("Error Input Data","Texture Sprite is Null, Please choose text Atlas Sprite first !"+pathImages,"OK");
				return false;
			}
			pathAtlas=AssetDatabase.GetAssetPath(mainTexture);
		}
		Dictionary<string,Sprite> dicSprite=new Dictionary<string, Sprite>();
		dicBoneObject.Clear();
		listBoneTransform.Clear();
		if(tabBuildAtlas!=0)
		{
			Sprite[] spritesTemp = AssetDatabase.LoadAllAssetsAtPath(pathAtlas)
				.OfType<Sprite>().ToArray();
			for(int i=0;i<spritesTemp.Length;i++)
			{
				dicSprite[spritesTemp[i].name]=spritesTemp[i];
			}
		}
		else
		{
			for(int x=0;x<listPathImageOutput.Count;x++)
			{
				Sprite[] spritesTemp = AssetDatabase.LoadAllAssetsAtPath(listPathImageOutput[x])
					.OfType<Sprite>().ToArray();
				for(int i=0;i<spritesTemp.Length;i++)
				{
					dicSprite[spritesTemp[i].name]=spritesTemp[i];
				}
			}
		}
		GameObject root=null;
		GameObject canvasUI = null;
		GameObject transObj = null;
		if (buildSpriteType == SpriteType.UGUI) {
			canvasUI=GameObject.Find("CanvasUI");
			if(canvasUI==null){
				canvasUI=new GameObject();
				canvasUI.name="CanvasUI";
				Canvas canvas=canvasUI.AddComponent<Canvas>();
				canvas.renderMode=RenderMode.WorldSpace;
				CanvasScaler canvasScaler=canvasUI.AddComponent<CanvasScaler>();
				canvasScaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPhysicalSize;
				canvasUI.AddComponent<GraphicRaycaster>();
			}
			if (canvasUI != null) {
				transObj = GameObject.Find (aepName);
			}
		}
		#if DEFINE_NGUI
		else if (buildSpriteType == SpriteType.NGUI) {
			canvasUI=GameObject.Find("2DUI");
			if(canvasUI==null){
				canvasUI=new GameObject();
				canvasUI.name="2DUI";
				UIPanel canvas=canvasUI.AddComponent<UIPanel>();
				UIRoot rootUGUI=canvasUI.AddComponent<UIRoot>();
				rootUGUI.scalingStyle = UIRoot.Scaling.Flexible;
				rootUGUI.minimumHeight = 320;
				rootUGUI.manualHeight = 4096;
				Rigidbody regidbody=canvasUI.AddComponent<Rigidbody>();
				regidbody.mass=1;
				regidbody.angularDrag=0.05f;
				regidbody.isKinematic=true;
				regidbody.angularDrag=0;
				regidbody.collisionDetectionMode=CollisionDetectionMode.Discrete;
				regidbody.interpolation=RigidbodyInterpolation.None;
			}
			if (canvasUI != null) {
				if(canvasUI.transform.Find (aepName)!=null)
				{
					transObj = canvasUI.transform.Find (aepName).gameObject;
				}
				if(transObj==null)
				{
					transObj = GameObject.Find (aepName);
				}
			}
		}
		#endif
		else {
			transObj= GameObject.Find(aepName);
		}

		#region remove Object
		if(transObj!=null)
		{
			if(!autoOverride)
			{
				if(!EditorUtility.DisplayDialog("Warning","Object "+aepName+" has exist in scene,Do you want replace this file?","YES","NO"))
				{
					return false;
				}
				else
				{

					if(transObj.transform.parent!=null&&(transObj.transform.parent.name!="2DUI"&&transObj.transform.parent.name!="CanvasUI"))
					{
						//Debug.LogError(transObj.transform.parent.name);
						GameObject.DestroyImmediate(transObj.transform.parent.gameObject);
					}
					else
					{
						GameObject.DestroyImmediate(transObj);
					}
					transObj=null;
				}
			}
			else
			{
				if(transObj.transform.parent!=null&&(transObj.transform.parent.name!="2DUI"&&transObj.transform.parent.name!="CanvasUI"))
				{
					//Debug.LogError(transObj.transform.parent.name);
					GameObject.DestroyImmediate(transObj.transform.parent.gameObject);
				}
				else
				{
					GameObject.DestroyImmediate(transObj);
				}
				transObj=null;
			}
		}
		#endregion
		Dictionary<string,GameObject> cache=new Dictionary<string, GameObject>();

		for(int x=0;x<listInfoFinal.Count;x++)
		{
			transObj= GameObject.Find(aepName);
			DataAnimAnalytics dataAnimAnalytics=listInfoFinal[x%listInfoFinal.Count];

			#region Build New Bone
			for(int i=0;i<dataAnimAnalytics.jsonFinal.bones.Count;i++)
			{
				SpineBoneElelement bone=dataAnimAnalytics.jsonFinal.bones[i];
				if(bone.parent==null)
				{
					if(transObj==null)
					{
						GameObject obj=new GameObject();
						if(buildSpriteType==SpriteType.UGUI)
						{
							obj.transform.parent=canvasUI.transform;
							obj.AddComponent<RectTransform>();
						}
						#if DEFINE_NGUI
						else if(buildSpriteType==SpriteType.NGUI)
						{
							obj.transform.SetParent(canvasUI.transform);
							obj.transform.localScale=Vector3.one;
						}
						#endif
						obj.transform.localPosition=new Vector3(0,0,0);
						obj.name=bone.name;
						cache[bone.name]=obj;
						root=obj;
						objectRoot=root;
						if(aepName.Length>0)
						{
							obj.name=aepName;
						}
						//Debug.LogError(bone.index+","+listBoneTransform.Count);
						if(!listBoneTransform.Contains(obj.transform))
						{
							listBoneTransform.Add(obj.transform);
						}
						dicBoneObject[bone.index]=obj;
					}
				}
				else 
				{
					GameObject parent=null;
					if(cache.TryGetValue(bone.parent,out parent))
					{
						GameObject objReference=null;
						cache.TryGetValue(bone.name, out objReference);
						if(objReference==null)// append new
						{
							GameObject obj=new GameObject();
							obj.transform.parent=parent.transform;
							obj.transform.localPosition=new Vector3(bone.x,bone.y,0);
							obj.transform.localScale=new Vector3(bone.scaleX,bone.scaleY,1);
							Quaternion quad=obj.transform.localRotation;
							quad.eulerAngles=new Vector3(0,0,bone.rotation);
							obj.transform.localRotation=quad;
							obj.name=bone.name;
							if(buildSpriteType==SpriteType.UGUI)
							{
								RectTransform rect=obj.AddComponent<RectTransform>();
								int subIndex=bone.index_ugui;
								int boneNowIndex=0;
								for(int cx=0;cx<parent.transform.childCount;cx++){
									Transform eleTran=parent.transform.GetChild(cx);
									SpineBoneElelement eleBone=null;
									if(dataAnimAnalytics.jsonFinal.dicBones.TryGetValue(eleTran.name,out eleBone)){
										if(eleBone.index_ugui>subIndex){
											boneNowIndex++;
										}
									}
								}
								//for new UI
								obj.transform.SetSiblingIndex(boneNowIndex);
							}
							cache[bone.name]=obj;
							if(transObj!=null)
							{
								obj.SetActive(false);
							}
							objReference=obj;

							//Debug.LogError(bone.index+","+listBoneTransform.Count);
							if(!listBoneTransform.Contains(obj.transform))
							{
								listBoneTransform.Add(obj.transform);
							}
							dicBoneObject[bone.index]=obj;
						}
					}
					else
					{
						Debug.LogWarning("Parent Null: "+bone.name);
					}
				}
			}
			#endregion

			Dictionary<string,string> objHideWhenStartAnim=new Dictionary<string, string>();
			Dictionary<string,string> objShowWhenStartAnim=new Dictionary<string, string>();
			Dictionary<string,string> objForAttachment=new Dictionary<string, string>();

			transObj= GameObject.Find(aepName);

			//Debug.LogError(Pathfinding.Serialization.JsonFx.JsonWriter.Serialize(spineAtlasInfo));
			#region Build Attachment
			for(int i=0;i<dataAnimAnalytics.jsonFinal.slots.Count;i++)
			{
				SpineSlotElelement slot=dataAnimAnalytics.jsonFinal.slots[i];

				GameObject obj=new GameObject();
				if(buildSpriteType==SpriteType.UGUI){
					RectTransform rect=obj.AddComponent<RectTransform>();
				}
				GameObject objBone=AEPAnimationClipElement.GetRefenreceObject(dataAnimAnalytics.jsonFinal.GetFullPathBone(slot.bone),transObj);
				if(objBone==null)
				{
					continue;
				}
				obj.transform.parent=objBone.transform;
				obj.transform.localPosition=new Vector3(0,0,0);
				obj.transform.localScale=new Vector3(1,1,1);
				Quaternion quad=obj.transform.localRotation;
				quad.eulerAngles=new Vector3(0,0,0);
				obj.transform.localRotation=quad;
				obj.name=slot.name;
				cache[slot.name]=obj;
				int sibling=0;

				#region add renderer
				for(int k=0;k<slot.listAcceptAttachment.Count;k++)
				{
					SpineSlotAndAttachmentReference reference=slot.listReference[k];
					SpineAttachmentElelement attachElement=slot.listAcceptObj[k];
					string nameInAtlas=slot.mapNameInAtlas[slot.listAcceptAttachment[k]];
					//dataAnimAnalytics.jsonFinal.dicPivot.TryGetValue(nameInAtlas,out attachElement);
					SpineSpriteElemnt spriteElement=spineAtlasInfo.GetSpineSpriteElementByName(nameInAtlas);

					if(attachElement!=null&&spriteElement!=null)
					{
						if(sibling<spriteElement.GetIndex()){
							sibling=spriteElement.GetIndex();
						}
						GameObject obj2=new GameObject();
						obj2.transform.parent=obj.transform;

						obj2.transform.localScale=new Vector3(scaleInfo/scaleTexture*Mathf.Abs(reference.scaleX),scaleInfo/scaleTexture*Mathf.Abs(reference.scaleY),scaleInfo/scaleTexture);
						Quaternion quad2=obj2.transform.localRotation;
						obj2.transform.localPosition=new Vector3(reference.x,reference.y,0);
						obj2.name=slot.listAcceptAttachment[k];

						if(attachElement.GetTypeAttachment()!="mesh"&&attachElement.GetTypeAttachment()!="skinnedmesh")
						{
							if(spriteElement!=null)
							{
								if(!spriteElement.isRotate)
									quad2.eulerAngles=new Vector3(0,0,reference.rotation);
								else
									quad2.eulerAngles=new Vector3(0,0,reference.rotation-90);
								obj2.transform.localRotation=quad2;
								if(buildSpriteType==SpriteType.SpriteRenderer)
								{
									obj2.AddComponent<SpriteRenderer>();
									SpriteRenderer sprite=obj2.GetComponent<SpriteRenderer>();

									if(dicSprite.ContainsKey(attachElement.nameInAtlas))	
									{
										sprite.sprite=dicSprite[attachElement.nameInAtlas];
									}
									else
									{
										Debug.LogError("#76 Not found spite:"+attachElement.nameInAtlas);
									}
									if(sortType==SortLayerType.Depth)
									{
										sprite.sortingOrder=defaultSortDepthValue+attachElement.depth;
									}
									else
									{
										sprite.sortingOrder=defaultSortDepthValue;
										float z=zfactor*(-attachElement.depth);
										obj2.transform.localPosition=new Vector3(reference.x,reference.y,z);
									}
									sprite.color=EditorUtil.HexToColor(slot.color);
								}
								#if DEFINE_NGUI
								else if(buildSpriteType==SpriteType.NGUI)
								{
									UI2DSprite sprite=obj2.AddComponent<UI2DSprite>();
									if(dicSprite.ContainsKey(attachElement.nameInAtlas))	
									{
										Sprite spriteData=dicSprite[attachElement.nameInAtlas];
										sprite.width=(int)spriteData.rect.width;
										sprite.height=(int)spriteData.rect.height;
										Bounds bounds = spriteData.bounds;
										var X = 0;//(-bounds.center.x / bounds.extents.x / 2 + 0.5f )*spriteData.rect.width;
										var Y = 0;;//(-bounds.center.y / bounds.extents.y / 2 + 0.5f)*spriteData.rect.height;
										//Debug.LogError(spriteData.name+","+X+","+Y);

										Vector3 vecObjSprite=sprite.transform.localPosition;
										//sprite.transform.localPosition=new Vector3(X,Y,vecObjSprite.z);
									}
									if(dicSprite.ContainsKey(attachElement.nameInAtlas))	
									{
										sprite.sprite2D=dicSprite[attachElement.nameInAtlas];
									}
									else if(dicSprite.ContainsKey(attachElement.nameInAtlas))	
									{
										sprite.sprite2D=dicSprite[attachElement.nameInAtlas];
									}
									else
									{
										Debug.LogWarning("not found suitable spite:"+attachElement.nameInAtlas);
									}
									if(sortType==SortLayerType.Depth)
									{
										sprite.depth=defaultSortDepthValue-slot.index;
										obj2.transform.localPosition=new Vector3(reference.x,reference.y,obj2.transform.localPosition.z);
									}
									else
									{
										sprite.depth=defaultSortDepthValue;
										float z=zfactor*slot.index;
										Vector3 vecObj=obj2.transform.localPosition;
										obj2.transform.localPosition=new Vector3(vecObj.x,vecObj.y,z);
									}
									sprite.color=EditorUtil.HexToColor(slot.color);
								}
								#endif
								else{
									RectTransform rect=obj2.AddComponent<RectTransform>();
									obj2.AddComponent<Image>();
									Image sprite=obj2.GetComponent<Image>();
									if(dicSprite.ContainsKey(attachElement.nameInAtlas))	
									{
										Sprite spriteData=dicSprite[attachElement.nameInAtlas];
										sprite.sprite=dicSprite[attachElement.nameInAtlas];
										RectTransformExtensions.SetWidth(sprite.rectTransform,spriteData.rect.width);
										RectTransformExtensions.SetHeight(sprite.rectTransform,spriteData.rect.height);
										Bounds bounds = spriteData.bounds;
										var pivotX = - bounds.center.x / bounds.extents.x / 2 + 0.5f;
										var pivotY = - bounds.center.y / bounds.extents.y / 2 + 0.5f;
										Vector2 vecAnchor=new Vector2(pivotX,pivotY);
										sprite.rectTransform.pivot=vecAnchor;
										//sprite.rectTransform.anchoredPosition=spriteData.
										//obj2.transform.localPosition=new Vector3(0,0,0);
										obj2.transform.localPosition=new Vector3(reference.x,reference.y,0);
									}
									else
									{
										Debug.LogError("#76 Not found spite:"+attachElement.nameInAtlas);
									}
									sprite.color=EditorUtil.HexToColor(slot.color);
								}
								string fullPath=EditorUtil.GetFullPathBone(transObj.transform,obj2.transform);
								objForAttachment[fullPath]=fullPath;
								if(slot.attachment!=null&&attachElement.name==slot.attachment)//default hien
								{
									objShowWhenStartAnim[fullPath]=fullPath;
								}
								else
								{
									obj2.SetActive(false);
									objHideWhenStartAnim[fullPath]=fullPath;
								}
							}
						}
						else
						{
							quad2.eulerAngles=new Vector3(0,0,reference.rotation);
							obj2.transform.localRotation=quad2;
							obj2.transform.localScale=Vector3.one;
							//Debug.LogError("Export: "+attachElement.GetTypeAttachment()+","+Pathfinding.Serialization.JsonFx.JsonWriter.Serialize(attachElement));


							int attachmentVertexCount =attachElement.vertices.Length >> 1;
							//int attachmentTriangleCount = attachElement.triangles.Length;
							int meshVertexCount = attachElement.uvs.Length>>1;

							Vector3[] vertices =null;
							Vector2[] uvs=null;
							int[] triangles=null;
							float[] meshUVs=null;
							Color32[] colors=null;
							float[] tempVertices=null;
							BoneWeight[] boneWeight=null;
							List<Transform> boneSkin=null;
							if(attachElement.GetTypeAttachment()=="mesh")
							{
								SpineMeshData meshData=new SpineMeshData(attachElement.nameInAtlas,spriteElement,attachElement);
								if(!meshData.CheckIsInit())
								{
									continue;
								}
								tempVertices = new float[attachElement.vertices.Length];
								meshData.ComputeWorldVertices(tempVertices);


								vertices = new Vector3[attachmentVertexCount];
								uvs= new Vector2[attachmentVertexCount];
								triangles=meshData.triangles;
								meshUVs = meshData.uvs;
								colors=new Color32[attachmentVertexCount];
							}
							else
							{
								meshVertexCount = attachElement.uvs.Length;
								SpineSkinnedMeshData meshSkin=new SpineSkinnedMeshData(attachElement.nameInAtlas,spriteElement,attachElement,obj);
								if(!meshSkin.CheckIsInit())
								{
									continue;
								}

								tempVertices = new float[meshVertexCount];
								meshSkin.ComputeWorldVertices(ref tempVertices,obj2,this);

								meshVertexCount=meshVertexCount/2;
								vertices = new Vector3[meshVertexCount];
								colors=new Color32[meshVertexCount];
								boneWeight=meshSkin.boneWeight;
								boneSkin=meshSkin.skinBone;
								uvs= new Vector2[meshVertexCount];

								triangles=meshSkin.triangles;
								//Debug.LogError(meshSkin.name+","+meshSkin.triangles.Length);
								int testTriangle=0;//meshSkin.triangles.Length;
								//testTriangle=testTriangle-testTriangle%3;
								triangles=new int[meshSkin.triangles.Length-testTriangle];
								for(int xx=0;xx<meshSkin.triangles.Length-testTriangle;xx++)
								{
									triangles[xx]=meshSkin.triangles[xx+testTriangle];
								}

								meshUVs =meshSkin.uvs;
								//Debug.LogError("CCCCdfsfs:"+meshVertexCount);
							}
							Vector3 meshBoundsMin;
							meshBoundsMin.x = float.MaxValue;
							meshBoundsMin.y = float.MaxValue;
							meshBoundsMin.z = 0.1f;//zSpacing > 0f ? 0f : zSpacing * (drawOrderCount - 1);
							Vector3 meshBoundsMax;
							meshBoundsMax.x = float.MinValue;
							meshBoundsMax.y = float.MinValue;
							meshBoundsMax.z = 0.1f;//zSpacing < 0f ? 0f : zSpacing * (drawOrderCount - 1);
							int vertexIndex=0;
							Color32 color=Color.white;

							for (int ii = 0; vertexIndex < meshVertexCount; ii += 2, vertexIndex++)
							{
								vertices[vertexIndex]=new Vector3();
								vertices[vertexIndex].x = tempVertices[ii];
								vertices[vertexIndex].y = tempVertices[ii + 1];//attachElement.vertices[ii + 1];
								vertices[vertexIndex].z = 0;//1*(-attachElement.depth);
								colors[vertexIndex] = color;
								if(meshUVs.Length>ii+1)
								{
									uvs[vertexIndex].x = meshUVs[ii];
									uvs[vertexIndex].y = meshUVs[ii + 1];
								}
								if (tempVertices[ii] < meshBoundsMin.x)
									meshBoundsMin.x = tempVertices[ii];
								else if (tempVertices[ii] > meshBoundsMax.x)
									meshBoundsMax.x = tempVertices[ii];
								if (tempVertices[ii + 1]< meshBoundsMin.y)
									meshBoundsMin.y = tempVertices[ii + 1];
								else if (tempVertices[ii + 1] > meshBoundsMax.y)
									meshBoundsMax.y = tempVertices[ii + 1];
							}
							Mesh mesh = new Mesh();
							mesh.name = "mesh"+attachElement.nameInAtlas;
							mesh.hideFlags = HideFlags.None;
							mesh.MarkDynamic();
							mesh.vertices = vertices;
							mesh.colors32 = colors;
							mesh.uv = uvs;
							mesh.subMeshCount=1;
							mesh.triangles=triangles;
							mesh.SetTriangles(triangles,0);
							if(boneWeight!=null)// for skin mesh
							{

								mesh.boneWeights=boneWeight;
								//Debug.LogError(Pathfinding.Serialization.JsonFx.JsonWriter.Serialize(mesh.boneWeights));
								//Debug.LogError(attachElement.name);
								//Debug.LogError(boneSkin.Count);
								Matrix4x4[] matrix=new Matrix4x4[boneSkin.Count];
								for(int tt=0;tt<matrix.Length;tt++)
								{
									matrix[tt]=boneSkin[tt].worldToLocalMatrix*obj2.transform.localToWorldMatrix;
								}
								//Debug.LogError(matrix.Length);
								mesh.bindposes=matrix;
							}
							Vector3 meshBoundsExtents = meshBoundsMax - meshBoundsMin;
							Vector3 meshBoundsCenter = meshBoundsMin + meshBoundsExtents * 0.5f;
							mesh.bounds = new Bounds(meshBoundsCenter, meshBoundsExtents);
							if(!isCreateMesh)
							{
								isCreateMesh=true;
								if(Directory.Exists(pathOutputs+"/mesh"))
								{
									Directory.Delete(pathOutputs+"/mesh",true);
								}
								Directory.CreateDirectory(pathOutputs+"/mesh");
							}
							string nameMesh=attachElement.nameInAtlas.Replace("/","");
							string pathmesh=pathOutputs+"/mesh/"+nameMesh+".asset";
							int temp=0;
							while(File.Exists(pathmesh))
							{
								temp++;
								pathmesh=pathOutputs+"/mesh/"+nameMesh+temp+".asset";
							}
							//	Debug.LogError("AAAA:"+mesh.bindposes.Length);
							AssetDatabase.CreateAsset(mesh,pathmesh);
							AssetDatabase.Refresh();
							mesh=AssetDatabase.LoadAssetAtPath(pathmesh,typeof(Mesh))as Mesh;
							//float z=0.01f*(-attachElement.depth);
							//obj2.transform.localPosition=new Vector3(reference.x,reference.y,z);

							//Debug.LogError("BBBBB"+mesh.bindposes.Length);
							obj2.AddComponent<MeshFilter>();
							MeshFilter meshFilter=obj2.GetComponent<MeshFilter>();
							meshFilter.mesh=mesh;
							if(attachElement.GetTypeAttachment()=="mesh")
							{
								obj2.AddComponent<MeshRenderer>();
								MeshRenderer meshRenderer=obj2.GetComponent<MeshRenderer>();
								meshRenderer.sharedMaterial=dicMaterial[spriteElement.textureName];
								if(sortType==SortLayerType.Depth)
								{
									meshRenderer.sortingOrder=defaultSortDepthValue+attachElement.depth;
								}
								else
								{
									meshRenderer.sortingOrder=defaultSortDepthValue;
									float z=zfactor*(-attachElement.depth);
									obj2.transform.localPosition=new Vector3(reference.x,reference.y,z);
								}
							}
							else
							{
								obj2.AddComponent<SkinnedMeshRenderer>();
								SkinnedMeshRenderer meshRenderer=obj2.GetComponent<SkinnedMeshRenderer>();
								meshRenderer.sharedMaterial=dicMaterial[spriteElement.textureName];
								if(boneSkin!=null)
								{
									Transform[] boneTemp=new Transform[boneSkin.Count];
									for(int cx=0;cx<boneTemp.Length;cx++)
									{
										GameObject objtemp=new GameObject();
										objtemp.transform.parent=obj2.transform;
										objtemp.transform.localScale=Vector3.one;
										objtemp.transform.position=boneSkin[cx].position;
										objtemp.transform.rotation=boneSkin[cx].rotation;
										//objtemp.AddComponent<FollowObject>();
										//objtemp.GetComponent<FollowObject>().SetupFollow(boneSkin[cx].gameObject);
										objtemp.name=boneSkin[cx].name;
										//objtemp.transform.lossyScale=boneSkin[cx].lossyScale;
										boneTemp[cx]=objtemp.transform;
									}
									meshRenderer.bones=boneTemp;
								}
								//Debug.LogError("CCCC:"+meshRenderer.bones.Length);
								if(sortType==SortLayerType.Depth)
								{
									//meshRenderer.sortingOrder=attachElement.depth;
									meshRenderer.sortingOrder=defaultSortDepthValue+attachElement.depth;
								}
								else
								{
									meshRenderer.sortingOrder=defaultSortDepthValue;
									float z=zfactor*(-attachElement.depth);
									obj2.transform.localPosition=new Vector3(reference.x,reference.y,z);
								}
								meshRenderer.sharedMesh=mesh;
								#if UNITY_5
								meshRenderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
								#endif
								meshRenderer.receiveShadows=false;
								Quaternion quadDefault= obj2.transform.rotation;
								quadDefault.eulerAngles=Vector3.zero;
								obj2.transform.rotation=quadDefault;

							}
							string fullPath=EditorUtil.GetFullPathBone(transObj.transform,obj2.transform);
							objForAttachment[fullPath]=fullPath;
							if(slot.attachment!=null&&attachElement.name==slot.attachment)//default hien
							{
								objShowWhenStartAnim[fullPath]=fullPath;
							}
							else
							{
								obj2.SetActive(false);
								objHideWhenStartAnim[fullPath]=fullPath;
							}

						}
					}
					else
					{
						if(spriteElement==null)
						{
							Debug.LogWarning("sprite not found:"+slot.listAcceptAttachment[k]+","+nameInAtlas);
						}
					}
				}
				#endregion

				if(buildSpriteType==SpriteType.UGUI)
				{
					RectTransform rect=obj.AddComponent<RectTransform>();
					int subIndex=sibling;
					int boneNowIndex=0;
					for(int cx=0;cx<objBone.transform.childCount;cx++){
						Transform eleTran=objBone.transform.GetChild(cx);
						SpineBoneElelement eleBone=null;
						if(dataAnimAnalytics.jsonFinal.dicBones.TryGetValue(eleTran.name,out eleBone)){
							if(eleBone.index_ugui>subIndex){
								boneNowIndex++;
							}
						}
					}
					//for new UI
					obj.transform.SetSiblingIndex(boneNowIndex);
				}

			}
			#endregion

			dataAnimAnalytics.AddObjectHideWhenStartAnim(objHideWhenStartAnim);
			dataAnimAnalytics.AddObjectShowWhenStartAnim(objShowWhenStartAnim);
			dataAnimAnalytics.AddObjectForAttachment(objForAttachment);

		}
		if(transObj!=null)
		{
			GameObject objFinal= new GameObject();
			objFinal.name=transObj.name;
			if(buildSpriteType==SpriteType.UGUI)
			{
				objFinal.transform.parent=canvasUI.transform;
				objFinal.AddComponent<RectTransform>();
			}
			#if DEFINE_NGUI
			else if(buildSpriteType==SpriteType.NGUI)
			{
				objFinal.transform.parent=canvasUI.transform;
			}
			#endif
			objFinal.transform.localPosition=new Vector3(0,0,0);
			objFinal.transform.localScale=Vector3.one;
			transObj.transform.parent=objFinal.transform;
			objFinal.transform.localScale=new Vector3(defaultScaleInScene,defaultScaleInScene,defaultScaleInScene);
			generatePrefab=objFinal;
			//transObj.transform.localScale=new Vector3(defaultScaleInScene,defaultScaleInScene,defaultScaleInScene);
			/*if(isGeneratePrefab)
			{
				string pathPrefab=pathOutputs+"/"+objFinal.name+".prefab";
				PrefabUtility.CreatePrefab(pathPrefab,objFinal);
				
				AssetDatabase.Refresh();
				GameObject.DestroyImmediate(objFinal);
				objFinal= PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath(pathPrefab, typeof(GameObject))) as GameObject;
				transObj=objFinal.transform.Find(objFinal.name).gameObject;
			}*/
		}
		return true;
	}
	#endregion


	#region Build Animation
	private bool BuildAnimation()
	{
		Debug.Log("BuildAnimation");

		if(!Directory.Exists(pathOutputs))
		{
			Directory.CreateDirectory(pathOutputs);	
		}
		if(listInfoFinal.Count<1)
		{
			if (buildSpriteType == SpriteType.NGUI) {
				CreateBoneSkeletonNGUI ();
			} else {
				CreateBoneSkeleton ();
			}
		}
		GameObject transObj= GameObject.Find(aepName);
		objectRoot=transObj;
		string folder=pathOutputs;
		if(objectRoot==null)
		{
			EditorUtility.DisplayDialog("Reference object Error","Can not find Object name "+aepName+" in scene","OK");
			return false;
		}
		if(objectRoot!=null)
		{
			for(int x=0;x<listInfoFinal.Count;x++)
			{
				DataAnimAnalytics dataAnimAnalytics=listInfoFinal[x];
				string animatorName=aepName+".controller";;
				string animatorPathFile=folder+"/"+animatorName;
				Animator animator=objectRoot.GetComponent<Animator>();
				if(animator==null)
				{
					objectRoot.AddComponent<Animator>();
					animator=objectRoot.GetComponent<Animator>();
				}
#if UNITY_4_0_0 ||UNITY_4_0 || UNITY_4_0_1||UNITY_4_1||UNITY_4_2||UNITY_4_3||UNITY_4_4||UNITY_4_5||UNITY_4_6||UNITY_4_7||UNITY_4_8||UNITY_4_9
				UnityEditorInternal.AnimatorController runtimeControler=AssetDatabase.LoadAssetAtPath(animatorPathFile,typeof(UnityEditorInternal.AnimatorController)) as UnityEditorInternal.AnimatorController;
#else
				UnityEditor.Animations.AnimatorController runtimeControler=AssetDatabase.LoadAssetAtPath(animatorPathFile,typeof(UnityEditor.Animations.AnimatorController)) as UnityEditor.Animations.AnimatorController;
#endif
				if(runtimeControler==null)
				{
					runtimeControler=OnProcessCreateAnimatorController(animatorName,animatorPathFile);
					animator.runtimeAnimatorController=runtimeControler;
				}
				else
				{
					if(x==0)
					{
						bool isContinue=false;
						if(autoOverride)
						{
							isContinue=true;
						}
						else
						{
							if(EditorUtility.DisplayDialog("Confimation","File Anim "+ animatorPathFile+ " has already Exist, do you want to replace","YES","NO"))
							{
								isContinue=true;
							}
						}
						if(isContinue)
						{
							AssetDatabase.DeleteAsset(animatorPathFile);
							AssetDatabase.Refresh();
							runtimeControler=OnProcessCreateAnimatorController(animatorName,animatorPathFile);
						}
						else
						{
							continue;
						}
					}
					animator.runtimeAnimatorController=runtimeControler;
				}
				for(int k=0;k<dataAnimAnalytics.jsonFinal.listAnimationClip.Count;k++)
				{
					string animName=dataAnimAnalytics.jsonFinal.listAnimationClip[k].clipName+".anim";
					string animPath=folder+"/"+animName;
					
					AnimationClip anim=AssetDatabase.LoadAssetAtPath(animPath,typeof(AnimationClip)) as AnimationClip;
					if(anim==null)
					{
						anim=OnProcessAnimFile(dataAnimAnalytics,dataAnimAnalytics.jsonFinal.listAnimationClip[k],animName,animPath);
					}
					else
					{
						bool isContinue=false;
						if(autoOverride)
						{
							isContinue=true;
						}
						else
						{
							if(EditorUtility.DisplayDialog("Confimation","File Anim "+ animPath+ " has already Exist, do you want to replace","YES","NO"))
							{
								isContinue=true;
							}
						}
						if(isContinue)
						{
							AssetDatabase.DeleteAsset(animPath);
							AssetDatabase.Refresh();
							anim=OnProcessAnimFile(dataAnimAnalytics,dataAnimAnalytics.jsonFinal.listAnimationClip[k],animName,animPath);
						}
						else
						{
							continue;
						}
					}
					#region anim state
					if(anim!=null)
					{
#if UNITY_4_0_0 ||UNITY_4_0 || UNITY_4_0_1||UNITY_4_1||UNITY_4_2||UNITY_4_3||UNITY_4_4||UNITY_4_5||UNITY_4_6||UNITY_4_7||UNITY_4_8||UNITY_4_9
						UnityEditorInternal.StateMachine sm = runtimeControler.GetLayer(0).stateMachine;
						//UnityEditor.Animations.ChildAnimatorState[] childs=sm.GetState;

						bool isExist=false;
						for(int i=0;i<sm.stateCount;i++)
						{
							if(sm.GetState(i).name==anim.name.ToLower())
							{
								sm.GetState(i).SetAnimationClip(anim);
								isExist=true;
								sm.defaultState=sm.GetState(i);
								sm.anyStatePosition=new Vector3(0,80*i,0);
								break;
							}
						}
						if(!isExist)
						{
							UnityEditorInternal.State state=sm.AddState(anim.name.ToLower());
							state.name=anim.name.ToLower();
							state.SetAnimationClip(anim);
							sm.defaultState=state;
						}
#else
						UnityEditor.Animations.AnimatorStateMachine sm = runtimeControler.layers[0].stateMachine;
						UnityEditor.Animations.ChildAnimatorState[] childs=sm.states;
						bool isExist=false;
						for(int i=0;i<childs.Length;i++)
						{
							if(childs[i].state.name==anim.name.ToLower())
							{
								childs[i].state.motion=anim;
								isExist=true;
								sm.defaultState=childs[i].state;
								break;
							}
						}
						if(!isExist)
						{
							UnityEditor.Animations.AnimatorState state=sm.AddState(anim.name.ToLower());
							state.name=anim.name.ToLower();
							state.motion=anim;
							sm.defaultState=state;
							
							runtimeControler.AddParameter(anim.name.ToLower(),AnimatorControllerParameterType.Trigger);

							UnityEditor.Animations.AnimatorStateTransition trans=sm.AddAnyStateTransition(state);
							trans.name=anim.name.ToLower();
							trans.duration=0;
							trans.exitTime=1;
							trans.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If,0,anim.name.ToLower());
						}
#endif
					}
					#endregion
				}
			}
		}
		if(isGeneratePrefab)
		{
			string pathPrefab=pathOutputs+"/"+generatePrefab.name+".prefab";
			PrefabUtility.CreatePrefab(pathPrefab,generatePrefab);
			
			AssetDatabase.Refresh();
			GameObject.DestroyImmediate(generatePrefab);
			generatePrefab= PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath(pathPrefab, typeof(GameObject))) as GameObject;
			transObj=generatePrefab.transform.Find(generatePrefab.name).gameObject;
		}
		return true;
	}
	#endregion

	#region Create .Anim File
	AnimationClip OnProcessAnimFile(DataAnimAnalytics dataAnimAnalytics,AEPAnimationClipFile clipFile,string name,string animPath)
	{
		AnimationClip anim=new AnimationClip();
		anim.name=name;
		anim.frameRate=fps;
		if(name.ToLower().Contains("idle.anim"))
		{
			SerializedObject serializedClip = new SerializedObject(anim);
			EditorUtil.AnimationClipSettings clipSettings = new EditorUtil.AnimationClipSettings(serializedClip.FindProperty("m_AnimationClipSettings"));
			clipSettings.loopTime = true;
			serializedClip.ApplyModifiedProperties();
		}
		//Debug.LogError(dataAnimAnalytics.filename+":"+dataAnimAnalytics.objHideWhenStartAnim.Count+","+dataAnimAnalytics.objShowWhenStartAnim.Count+","+dataAnimAnalytics.jsonFinal.dicSlotAttactment.Count);
		foreach(KeyValuePair<string,string> pair in dataAnimAnalytics.objHideWhenStartAnim)
		{
			AEPAnimationClipElement aepClip=new AEPAnimationClipElement();
			aepClip.AddStartVisible(pair.Key,pair.Value,false);
			for(int i=0;i<aepClip.listCurve.Count;i++)
			{
				//Debug.LogError(JsonWriter.Serialize(aepClip.listCurve[i].binding));
				AnimationUtility.SetEditorCurve(anim, aepClip.listCurve[i].binding,aepClip.listCurve[i].curve);
			}
		}

		foreach(KeyValuePair<string,string> pair in dataAnimAnalytics.objShowWhenStartAnim)
		{
			AEPAnimationClipElement aepClip=new AEPAnimationClipElement();
			aepClip.AddStartVisible(pair.Key,pair.Value,true);
			for(int i=0;i<aepClip.listCurve.Count;i++)
			{
				//Debug.LogError(JsonWriter.Serialize(aepClip.listCurve[i].binding));
				AnimationUtility.SetEditorCurve(anim, aepClip.listCurve[i].binding,aepClip.listCurve[i].curve);
			}
		}
		foreach(KeyValuePair<string,AEPBoneAnimationElement> pair in clipFile.dicAnimation)
		{
			string pathProperty=dataAnimAnalytics.jsonFinal.GetFullPathBone(pair.Key);
			AEPAnimationClipElement aepClip=new AEPAnimationClipElement();
			aepClip.AddTranformAnimation(pair.Value,pathProperty,objectRoot,dataAnimAnalytics.jsonFinal,ExportCurveType.Default);
			for (int i = 0; i < aepClip.listCurve.Count; i++)
			{
				//aepClip.listCurve[i].curve.SmoothTangents(0,0);
				//UnityEditor.AnimationClipSettings

				//AnimationCurveUtility.SetLinear(ref aepClip.listCurve[i].curve);
				//var listKey=aepClip.listCurve[i].curve
				AnimationUtility.SetEditorCurve(anim, aepClip.listCurve[i].binding, aepClip.listCurve[i].curve);
				//AssetDatabase.StartAssetEditing();
				CurveExtension.UpdateCurveLinear(aepClip.listCurve[i].curve);
				try
				{
					AnimationUtility.SetEditorCurve(anim, aepClip.listCurve[i].binding, aepClip.listCurve[i].curve);
				}
				catch(Exception ex)
				{
					
				}
				//AssetDatabase.StopAssetEditing();
			}
			//EditorCurveBinding curveBinding= AnimationUtility.GetCurveBindings(anim);

		}
		foreach(KeyValuePair<string,AEPSlotAnimationElement> pair in clipFile.dicSlotAttactment)
		{
			SpineSlotElelement slot=null;
			dataAnimAnalytics.jsonFinal.dicSlots.TryGetValue(pair.Key,out slot);
			if(slot!=null)
			{
				AEPAnimationClipElement aepClip=new AEPAnimationClipElement();
				if (buildSpriteType == SpriteType.NGUI) {
					aepClip.AddAttactmentAnimationNGUI (pair.Value, buildSpriteType, slot, objectRoot, dataAnimAnalytics.jsonFinal);
				} else {
					aepClip.AddAttactmentAnimation (pair.Value, buildSpriteType, slot, objectRoot, dataAnimAnalytics.jsonFinal);
				}
				for(int i=0;i<aepClip.listCurve.Count;i++)
				{
					AnimationUtility.SetEditorCurve(anim, aepClip.listCurve[i].binding,aepClip.listCurve[i].curve);
				}
			}
		}
		//UnityEditorInternal.AnimationWindowUtility.
		//UnityEditorInternal.InternalEditorUtility.a

		EditorUtility.SetDirty(anim);
		AssetDatabase.CreateAsset(anim,animPath);
		AssetDatabase.ImportAsset(animPath);
		AssetDatabase.Refresh();

#if UNITY_4_0_0 ||UNITY_4_0 || UNITY_4_0_1||UNITY_4_1||UNITY_4_2||UNITY_4_3||UNITY_4_4||UNITY_4_5||UNITY_4_6||UNITY_4_7||UNITY_4_8||UNITY_4_9
		AnimationUtility.SetAnimationType(anim, ModelImporterAnimationType.Generic); 
		UnityEditor.AnimationUtility.SetAnimationType(anim,ModelImporterAnimationType.Generic);

#endif
		return anim;
	}
	#endregion

	#region Create Animator Controller File
#if UNITY_4_0_0 ||UNITY_4_0 || UNITY_4_0_1||UNITY_4_1||UNITY_4_2||UNITY_4_3||UNITY_4_4||UNITY_4_5||UNITY_4_6||UNITY_4_7||UNITY_4_8||UNITY_4_9
	UnityEditorInternal.AnimatorController OnProcessCreateAnimatorController(string name,string animPath)
	{
		UnityEditorInternal.AnimatorController.CreateAnimatorControllerAtPath(animPath);
		UnityEditorInternal.AnimatorController runtimeControler=AssetDatabase.LoadAssetAtPath(animPath,typeof(UnityEditorInternal.AnimatorController)) as UnityEditorInternal.AnimatorController;
		AssetDatabase.Refresh();
		return runtimeControler;
	}
#else
	UnityEditor.Animations.AnimatorController OnProcessCreateAnimatorController(string name,string animPath)
	{
		UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(animPath);
		UnityEditor.Animations.AnimatorController runtimeControler=AssetDatabase.LoadAssetAtPath(animPath,typeof(UnityEditor.Animations.AnimatorController)) as UnityEditor.Animations.AnimatorController;
		AssetDatabase.Refresh();
		return runtimeControler;
	}
#endif
	#endregion
}
#region Animation Data Analytics

public class AEPAnimationCurve
{
	public EditorCurveBinding binding;
	public AnimationCurve curve;
	public AEPAnimationCurve(EditorCurveBinding _binding,AnimationCurve _curve)
	{
		this.binding=_binding;
		this.curve=_curve;
	}
}

public class AEPAnimationClipElement
{
	public List<AEPAnimationCurve> listCurve=new List<AEPAnimationCurve>();
	public static GameObject GetRefenreceObject(string path,GameObject aepRoot)
	{
		if(path.Length<1)
		{
			return aepRoot;
		}
		else
		{
			string[] sub=path.Split(new char[]{'/'});
			GameObject obj=aepRoot;
			for(int i=0;i<sub.Length;i++)
			{
				Transform trans=obj.transform.Find(sub[i]);
				if(trans!=null)
				{
					obj=trans.gameObject;
				}
				else
				{
					return null;
				}
			}
			return obj;
		}
	}
	public AEPAnimationClipElement()
	{

	}
	public void AddTranformAnimation(AEPBoneAnimationElement animInfo,string path,GameObject aepRoot,AEPJsonFinal jsonFinal,ExportCurveType exportType)
	{
		GameObject objReference=GetRefenreceObject(path,aepRoot);
		if(objReference==null)
		{
			Debug.LogError("#118 Not have object reference:"+path);
			return;
		}
		if(animInfo.translate!=null)
		{
			SpineBoneElelement boneElement=jsonFinal.GetBoneElement(animInfo.name);
			Vector3 vecTran= new Vector3(boneElement.x,boneElement.y,0);

			// co tranlate
			//x
			{
				EditorCurveBinding binding=new EditorCurveBinding();
				AnimationCurve curve=new AnimationCurve();
				binding.type=typeof(Transform);
				binding.path=path;
				binding.propertyName="m_LocalPosition.x";
				for(int i=0;i<animInfo.translate.Count;i++)
				{
					AEPAnimationTranslate translate=animInfo.translate[i];
					Keyframe k=new Keyframe();
					//k.inTangent=0;
					//k.outTangent=0;
					k.tangentMode=31;
					k.time=translate.time;
					k.value=translate.x+vecTran.x;
					//curve.AddKey(k);
					curve.AddKey(KeyframeUtil.GetNew(translate.time,translate.x+vecTran.x ,KeyframeUtil.GetTangleMode(translate.GetCurve())));
					//Debug.LogError(KeyframeUtil.GetTangleMode(translate.GetCurve()));
				}
				
				CurveExtension.UpdateAllLinearTangents(curve);
				AEPAnimationCurve curveData=new AEPAnimationCurve(binding,curve);
				listCurve.Add(curveData);
			}
			//y
			{
				EditorCurveBinding binding2=new EditorCurveBinding();
				AnimationCurve curve2=new AnimationCurve();
				binding2.type=typeof(Transform);
				binding2.path=path;
				binding2.propertyName="m_LocalPosition.y";
				for(int i=0;i<animInfo.translate.Count;i++)
				{
					AEPAnimationTranslate translate=animInfo.translate[i];
					Keyframe k=new Keyframe();
					//k.inTangent=0;
					//k.outTangent=0;
					k.tangentMode=31;
					k.time=translate.time;
					k.value=translate.y+vecTran.y;
					//curve2.AddKey(k);
					curve2.AddKey(KeyframeUtil.GetNew(translate.time,translate.y+vecTran.y ,KeyframeUtil.GetTangleMode(translate.GetCurve())));

				}
				
				CurveExtension.UpdateAllLinearTangents(curve2);
				AEPAnimationCurve curveData2=new AEPAnimationCurve(binding2,curve2);
				listCurve.Add(curveData2);
			}
			//z
			{
				EditorCurveBinding binding3=new EditorCurveBinding();
				AnimationCurve curve3=new AnimationCurve();
				binding3.type=typeof(Transform);
				binding3.path=path;
				binding3.propertyName="m_LocalPosition.z";
				for(int i=0;i<animInfo.translate.Count;i++)
				{
					AEPAnimationTranslate translate=animInfo.translate[i];
					Keyframe k=new Keyframe();
					//k.inTangent=0;
					//k.outTangent=0;
					k.tangentMode=31;
					k.time=translate.time;
					k.value=0+vecTran.z;
					//curve3.AddKey(k);
					curve3.AddKey(KeyframeUtil.GetNew(translate.time,0+vecTran.z ,KeyframeUtil.GetTangleMode(translate.GetCurve())));

				}
				
				CurveExtension.UpdateAllLinearTangents(curve3);
				AEPAnimationCurve curveData3=new AEPAnimationCurve(binding3,curve3);
				listCurve.Add(curveData3);
			}
		}
		if(animInfo.rotate!=null)
		{
			float lastAngleCi=0;
			for(int ci=0;ci<animInfo.rotate.Count;ci++)
			{
				if(ci==0)
				{
					lastAngleCi=animInfo.rotate[ci].angle;
					animInfo.rotate[ci].angleChange=lastAngleCi;
				}
				else
				{
					float curAngle=animInfo.rotate[ci].angle;
					if(curAngle-lastAngleCi>180)
					{
						curAngle=curAngle-360;
					}
					else if(lastAngleCi-curAngle>180)
					{
						curAngle=curAngle+360;
					}
					float change=curAngle-lastAngleCi;
					lastAngleCi=animInfo.rotate[ci].angle;
					animInfo.rotate[ci].angleChange=change;
				}
			}


			//Vector3 vecRotate=objReference.transform.localRotation.eulerAngles;
			//Quaternion quad=objReference.transform.localRotation;
			//Debug.LogError("Quad:"+quad.x+","+quad.y+","+quad.z+","+quad.w);
			SpineBoneElelement boneElement=jsonFinal.GetBoneElement(animInfo.name);
			if(boneElement!=null)
			{
				float frameStartAngle=boneElement.rotation;
				//x
				{
					EditorCurveBinding bindingx=new EditorCurveBinding();
					AnimationCurve curvex=new AnimationCurve();
					bindingx.type=typeof(Transform);
					bindingx.path=path;
					bindingx.propertyName="m_LocalRotation.x";
					for(int i=0;i<animInfo.rotate.Count;i++)
					{
						Quaternion quad2=objReference.transform.localRotation;
						AEPAnimationRotate rotate=animInfo.rotate[i];
						Keyframe k=new Keyframe();
						//k.inTangent=0;
						//k.outTangent=0;
						k.tangentMode=21;
						k.time=rotate.time;
						k.value=quad2.x;
						//curvex.AddKey(k);
//						SpineCurveUtil.AddLinearKey(ref curvex,
//						                      KeyframeUtil.GetNew2(rotate.time,0 ,KeyframeUtil.GetTangleMode(rotate.GetCurve()),KeyframeUtil.GetTangleMode(rotate.GetCurve()))
//						                      );

						curvex.AddKey(KeyframeUtil.GetNew2(rotate.time,0 ,KeyframeUtil.GetTangleMode(rotate.GetCurve()),KeyframeUtil.GetTangleMode(rotate.GetCurve())));

					}
					
					CurveExtension.UpdateAllLinearTangents(curvex);
					AEPAnimationCurve curveDatax=new AEPAnimationCurve(bindingx,curvex);
					listCurve.Add(curveDatax);
				}
				//y
				{
					EditorCurveBinding bindingy=new EditorCurveBinding();
					AnimationCurve curvey=new AnimationCurve();
					bindingy.type=typeof(Transform);
					bindingy.path=path;
					bindingy.propertyName="m_LocalRotation.y";
					for(int i=0;i<animInfo.rotate.Count;i++)
					{

						Quaternion quad2=objReference.transform.localRotation;
						AEPAnimationRotate rotate=animInfo.rotate[i];

						Keyframe k=new Keyframe();
						//k.inTangent=0;
						//k.outTangent=0;
						k.tangentMode=21;
						k.time=rotate.time;
						k.value=quad2.y;
						//curvey.AddKey(k);
						curvey.AddKey(KeyframeUtil.GetNew2(rotate.time,0 ,KeyframeUtil.GetTangleMode(rotate.GetCurve()),KeyframeUtil.GetTangleMode(rotate.GetCurve())));
//						SpineCurveUtil.AddLinearKey(ref curvey,
//						                      KeyframeUtil.GetNew2(rotate.time,0 ,KeyframeUtil.GetTangleMode(rotate.GetCurve()),KeyframeUtil.GetTangleMode(rotate.GetCurve()))
//						                      );
					}
					
					CurveExtension.UpdateAllLinearTangents(curvey);
					AEPAnimationCurve curveDatay=new AEPAnimationCurve(bindingy,curvey);
					listCurve.Add(curveDatay);
				}

				//z
				{
					EditorCurveBinding bindingz=new EditorCurveBinding();
					AnimationCurve curvez=new AnimationCurve();
					bindingz.type=typeof(Transform);
					bindingz.path=path;
					bindingz.propertyName="m_LocalRotation.z";
					float beforeAngle=0;
					for(int i=0;i<animInfo.rotate.Count;i++)
					{
						AEPAnimationRotate rotate=animInfo.rotate[i];
						//float angle=rotate.angle+frameStartAngle;
						Quaternion quad2=new Quaternion();
						Vector3 vec=quad2.eulerAngles;
						vec.z=beforeAngle+rotate.angleChange+frameStartAngle;
						beforeAngle+=rotate.angleChange;
						if(i==0)
						{

						}
						else
						{
//							if(Mathf.Abs(beforeAngle-vec.z)>180)
//							{
//								string str=beforeAngle+","+vec.z+"-->";
//								Vector3 vecFrom=new Vector3(vec.x,vec.y,beforeAngle);
//								Quaternion quadTo=new Quaternion();
//								quadTo.eulerAngles=vec;
//								Quaternion quadFrom=new Quaternion();
//								quadFrom.eulerAngles=vecFrom;
//								str+=quadFrom.eulerAngles.z+","+quadTo.eulerAngles.z+"==>";
//
//								for(int cx=0;cx<5;cx++)
//								{
//									Quaternion interpolale= Quaternion.Slerp(quadFrom,quadTo,0.2f*cx);
//									str+=interpolale.eulerAngles.z+",";
//								}
//								//Debug.LogError(str);
//							}
//							beforeAngle=vec.z;
						}
						/*if(exportType==ExportCurveType.InterpolateAngle)
						{
							while(vec.z>360)
							{
								vec.z-=360;
							}
							while(vec.z<-360)
							{
								vec.z+=360;
							}
							if(vec.z>180&&vec.z<360)
							{
								vec.z=360-vec.z;
							}
						}*/
						quad2=new Quaternion();
						quad2.eulerAngles=vec;
						//Debug.LogError(quad2.eulerAngles.z);
						Keyframe k=new Keyframe();
						//k.inTangent=0;
						//k.outTangent=0;
						k.tangentMode=21;
						k.time=rotate.time;
						k.value= quad2.z;//Mathf.Sin((180+angle/2)*Mathf.Deg2Rad);
						//curvez.AddKey(k);

//						curvez.AddKey(KeyframeUtil.GetNew2(rotate.time,quad2.z ,KeyframeUtil.GetTangleMode(rotate.GetCurve()),KeyframeUtil.GetTangleMode(rotate.GetCurve())));
						SpineCurveUtil.AddKey(ref curvez,
						                      KeyframeUtil.GetNew2(rotate.time,quad2.z ,KeyframeUtil.GetTangleMode(rotate.GetCurve()),KeyframeUtil.GetTangleMode(rotate.GetCurve()))
						                      ,TangentMode.Cubic
						                      );
					}

					//UnityEditor.CurveUtility.SetKeyTangentMode(
					CurveExtension.UpdateAllLinearTangents(curvez);
					AnimationCurveUtility.SetLinear(ref curvez);

					AEPAnimationCurve curveDataz=new AEPAnimationCurve(bindingz,curvez);
					listCurve.Add(curveDataz);
				}

				//w
				{
					EditorCurveBinding bindingw=new EditorCurveBinding();
					AnimationCurve curvew=new AnimationCurve();
					bindingw.type=typeof(Transform);
					bindingw.path=path;
					bindingw.propertyName="m_LocalRotation.w";
					float inTangent=0;
					float outTangent=0;
					float beforeAngle=0;
					for(int i=0;i<animInfo.rotate.Count;i++)
					{
						AEPAnimationRotate rotate=animInfo.rotate[i];
						//float angle=rotate.angleChange+frameStartAngle;

						Quaternion quad2=objReference.transform.localRotation;
						Vector3 vec=quad2.eulerAngles;
						vec.z=beforeAngle+rotate.angleChange+frameStartAngle;
						beforeAngle+=rotate.angleChange;
						/*if(exportType==ExportCurveType.InterpolateAngle)
						{
							while(vec.z>360)
							{
								vec.z-=360;
							}
							while(vec.z<-360)
							{
								vec.z+=360;
							}
							if(vec.z>180&&vec.z<360)
							{
								vec.z=360-vec.z;
							}
						}*/
						quad2.eulerAngles=vec;

						Keyframe k=new Keyframe();
						//k.inTangent=0;
						//k.outTangent=0;
						k.tangentMode=21;
						k.time=rotate.time;
						k.value=quad2.w;// Mathf.Cos((180+angle/2)*Mathf.Deg2Rad);
						//curvew.AddKey(k);
						//Keyframe keyframe=KeyframeUtil.GetNew2(rotate.time,quad2.w ,KeyframeUtil.GetTangleMode(rotate.GetCurve()),KeyframeUtil.GetTangleMode(rotate.GetCurve()));

						SpineCurveUtil.AddKey(ref curvew,
						                      KeyframeUtil.GetNew2(rotate.time,quad2.w ,KeyframeUtil.GetTangleMode(rotate.GetCurve()),KeyframeUtil.GetTangleMode(rotate.GetCurve()))
						                      ,TangentMode.Cubic
						                      );
					}
					
					CurveExtension.UpdateAllLinearTangents(curvew);
					
					AnimationCurveUtility.SetLinear(ref curvew);
					AEPAnimationCurve curveDataw=new AEPAnimationCurve(bindingw,curvew);
					listCurve.Add(curveDataw);
				}
			}
			else{
				Debug.LogWarning("Can not find reference object for Rotation:"+boneElement.name);
			}
		}

		if(animInfo.scale!=null)
		{
			//Vector3 vecScale=objReference.transform.localScale;
			SpineBoneElelement boneElement=jsonFinal.GetBoneElement(animInfo.name);
			if(boneElement!=null)
			{
				Vector2 scaleFrameStart= new Vector2(boneElement.scaleX,boneElement.scaleY);
				float defaultX=1;
				float defaultY=1;
				if(animInfo.scale.Count>0)
				{
					defaultX=animInfo.scale[0].x;
					defaultY=animInfo.scale[0].y;
				}

				//x
				{
					EditorCurveBinding bindingx=new EditorCurveBinding();
					AnimationCurve curvex=new AnimationCurve();
					bindingx.type=typeof(Transform);
					bindingx.path=path;
					bindingx.propertyName="m_LocalScale.x";
					for(int i=0;i<animInfo.scale.Count;i++)
					{
						AEPAnimationScale scale=animInfo.scale[i];
						Keyframe k=new Keyframe();
						//k.inTangent=0;
						//k.outTangent=0;
						k.tangentMode=31;
						k.time=scale.time;
						k.value=(scale.x-defaultX)+scaleFrameStart.x;
						curvex.AddKey(k);
						//curvex.AddKey(KeyframeUtil.GetNew(scale.time,scale.x*scaleFrameStart.x ,KeyframeUtil.GetTangleMode(scale.GetCurve())));
					}
					
					CurveExtension.UpdateAllLinearTangents(curvex);
					AEPAnimationCurve curveDatax=new AEPAnimationCurve(bindingx,curvex);
					listCurve.Add(curveDatax);
				}
				//y
				{
					EditorCurveBinding bindingy=new EditorCurveBinding();
					AnimationCurve curvey=new AnimationCurve();
					bindingy.type=typeof(Transform);
					bindingy.path=path;
					bindingy.propertyName="m_LocalScale.y";
					for(int i=0;i<animInfo.scale.Count;i++)
					{
						AEPAnimationScale scale=animInfo.scale[i];
						Keyframe k=new Keyframe();
						k.tangentMode=31;
						k.time=scale.time;
						k.value=(scale.y-defaultY)+scaleFrameStart.y;
						curvey.AddKey(k);
						//curvey.AddKey(KeyframeUtil.GetNew(scale.time,scale.y*scaleFrameStart.y ,KeyframeUtil.GetTangleMode(scale.GetCurve())));

					}
					
					CurveExtension.UpdateAllLinearTangents(curvey);
					AEPAnimationCurve curveDatax=new AEPAnimationCurve(bindingy,curvey);
					listCurve.Add(curveDatax);
				}
				//z
				{
					EditorCurveBinding bindingz=new EditorCurveBinding();
					AnimationCurve curvez=new AnimationCurve();
					bindingz.type=typeof(Transform);
					bindingz.path=path;
					bindingz.propertyName="m_LocalScale.z";
					for(int i=0;i<animInfo.scale.Count;i++)
					{
						AEPAnimationScale scale=animInfo.scale[i];
						Keyframe k=new Keyframe();
						k.tangentMode=31;
						k.time=scale.time;
						k.value=1;
						curvez.AddKey(k);
						//curvez.AddKey(KeyframeUtil.GetNew(scale.time,1 ,KeyframeUtil.GetTangleMode(scale.GetCurve())));

					}
					
					CurveExtension.UpdateAllLinearTangents(curvez);
					AEPAnimationCurve curveDataz=new AEPAnimationCurve(bindingz,curvez);
					listCurve.Add(curveDataz);
				}
			}
			else{
				Debug.LogWarning("Can not find reference object for Scale:"+boneElement.name);
			}

		}
	}

	public void AddAttactmentAnimation(AEPSlotAnimationElement animInfo,SpriteType buildSpriteType, SpineSlotElelement slot,GameObject aepRoot,AEPJsonFinal jsonFinal)
	{
		string path=jsonFinal.GetFullPathBone(slot.bone)+"/"+slot.name;
		if (string.IsNullOrEmpty (jsonFinal.GetFullPathBone (slot.bone))) {
			path = slot.name;
		}
		GameObject objReference=GetRefenreceObject(path,aepRoot);
		if(objReference==null)
		{
			Debug.LogError("#211 Not have object reference:"+path);
			return;
		}
		if(animInfo.attachment!=null)
		{
			for(int x=0;x<slot.listAcceptAttachment.Count;x++)
			{
				EditorCurveBinding binding=new EditorCurveBinding();
				AnimationCurve curve=new AnimationCurve();
				binding.type=typeof(GameObject);
				binding.path=path+"/"+slot.listAcceptAttachment[x];
				binding.propertyName="m_IsActive";
				if(animInfo.attachment.Count>0)// gia tri default
				{
					if(animInfo.attachment[0].time>0)
					{
						Keyframe k=new Keyframe();
						//k.inTangent=1000;
						//k.outTangent=1000;
						k.tangentMode=31;
						if(slot.listAcceptAttachment[x]==slot.attachment)
						{
							k.value=1;
						}
						else
						{
							k.value=0;
						}
						k.time=0;
						curve.AddKey(KeyframeUtil.GetNew(0, k.value , TangentMode.Stepped));

					}
				}
				for(int i=0;i<animInfo.attachment.Count;i++)
				{
					AEPAnimationAttachment attact=animInfo.attachment[i];
					Keyframe k=new Keyframe();
					//k.inTangent=1000;
					//k.outTangent=1000;
					k.tangentMode=31;
					k.time=attact.time;
					if(attact.name==null||attact.name.Length<1)
						k.value=0;
					else if(slot.listAcceptAttachment[x]==attact.name)
					{
						k.value=1;
					}
					else
					{
						k.value=0;
					}
					curve.AddKey(KeyframeUtil.GetNew(attact.time, k.value , TangentMode.Stepped));
				}
				//curve.
				AEPAnimationCurve curveData=new AEPAnimationCurve(binding,curve);
				listCurve.Add(curveData);
			}
		}
		if(animInfo.color!=null)
		{
			for (int x = 0; x < slot.listAcceptAttachment.Count; x++) {
					Transform trans= objReference.transform.Find(slot.listAcceptAttachment [x]);
				if (trans != null) {
					Color defaultColor = Color.white;
					if (buildSpriteType == SpriteType.SpriteRenderer) {
						SpriteRenderer render = trans.GetComponent<SpriteRenderer> ();
						if (render != null) {
							defaultColor = render.color;
						}
					} 
					#if DEFINE_NGUI_OLD
					else if(buildSpriteType==SpriteType.NGUI){
						UI2DSprite render2 = trans.GetComponent<UI2DSprite> ();
						if (render2 != null) {
							defaultColor = render2.color;
						}
					}
					#endif
					else {
						Image render3 = trans.GetComponent<Image> ();
						if (render3 != null) {
							defaultColor = render3.color;
						}
					}

					//r
					{
						EditorCurveBinding binding = new EditorCurveBinding ();
						AnimationCurve curve = new AnimationCurve ();
						if (buildSpriteType == SpriteType.SpriteRenderer) {
							binding.type = typeof(SpriteRenderer);
						} 
						#if DEFINE_NGUI_OLD
						else if(buildSpriteType==SpriteType.NGUI){
							binding.type=typeof(UI2DSprite);
						}
						#endif
						else {
							binding.type = typeof(Image);
						}
						binding.path = path+"/"+slot.listAcceptAttachment[x];;

						binding.propertyName = "m_Color.r";

						if (animInfo.color.Count > 0 && animInfo.color [0].time > 0) {
							curve.AddKey (KeyframeUtil.GetNew (0, defaultColor.r, KeyframeUtil.GetTangleMode ("stepped")));
							AEPAnimationCurve curveDataDef = new AEPAnimationCurve (binding, curve);
							listCurve.Add (curveDataDef);
						}
						for (int i = 0; i < animInfo.color.Count; i++) {
							Color color = EditorUtil.HexToColor (animInfo.color [i].color);
							curve.AddKey (KeyframeUtil.GetNew (animInfo.color [i].time, color.r, KeyframeUtil.GetTangleMode (animInfo.color [i].tangentType)));
						}
						AEPAnimationCurve curveData = new AEPAnimationCurve (binding, curve);
						listCurve.Add (curveData);
					}
					//b
					{
						EditorCurveBinding binding = new EditorCurveBinding ();
						AnimationCurve curve = new AnimationCurve ();
						if (buildSpriteType == SpriteType.SpriteRenderer) {
							binding.type = typeof(SpriteRenderer);
						}
						#if DEFINE_NGUI_OLD
						else if(buildSpriteType==SpriteType.NGUI){
							binding.type=typeof(UI2DSprite);
						}
						#endif
						else {
							binding.type = typeof(Image);
						}
						binding.path = path+"/"+slot.listAcceptAttachment[x];;
						binding.propertyName = "m_Color.b";
						if (animInfo.color.Count > 0 && animInfo.color [0].time > 0) {
							curve.AddKey (KeyframeUtil.GetNew (0, defaultColor.b, KeyframeUtil.GetTangleMode ("stepped")));
							AEPAnimationCurve curveDataDef = new AEPAnimationCurve (binding, curve);
							listCurve.Add (curveDataDef);
						}
						for (int i = 0; i < animInfo.color.Count; i++) {
							Color color = EditorUtil.HexToColor (animInfo.color [i].color);
							curve.AddKey (KeyframeUtil.GetNew (animInfo.color [i].time, color.b, KeyframeUtil.GetTangleMode (animInfo.color [i].tangentType)));
						}
						AEPAnimationCurve curveData = new AEPAnimationCurve (binding, curve);
						listCurve.Add (curveData);
					}
					//g
					{
						EditorCurveBinding binding = new EditorCurveBinding ();
						AnimationCurve curve = new AnimationCurve ();
						if (buildSpriteType == SpriteType.SpriteRenderer) {
							binding.type = typeof(SpriteRenderer);
						}
						#if DEFINE_NGUI_OLD
						else if(buildSpriteType==SpriteType.NGUI){
							binding.type=typeof(UI2DSprite);
						}
						#endif
						else {
							binding.type = typeof(Image);
						}
						binding.path = path+"/"+slot.listAcceptAttachment[x];;
						binding.propertyName = "m_Color.g";
						if (animInfo.color.Count > 0 && animInfo.color [0].time > 0) {
							curve.AddKey (KeyframeUtil.GetNew (0, defaultColor.g, KeyframeUtil.GetTangleMode ("stepped")));
							AEPAnimationCurve curveDataDef = new AEPAnimationCurve (binding, curve);
							listCurve.Add (curveDataDef);
						}
						for (int i = 0; i < animInfo.color.Count; i++) {
							Color color = EditorUtil.HexToColor (animInfo.color [i].color);
							curve.AddKey (KeyframeUtil.GetNew (animInfo.color [i].time, color.g, KeyframeUtil.GetTangleMode (animInfo.color [i].tangentType)));
						}
						AEPAnimationCurve curveData = new AEPAnimationCurve (binding, curve);
						listCurve.Add (curveData);
					}
					//a
					{
						EditorCurveBinding binding = new EditorCurveBinding ();
						AnimationCurve curve = new AnimationCurve ();
						if (buildSpriteType == SpriteType.SpriteRenderer) {
							binding.type = typeof(SpriteRenderer);
						}	
						#if DEFINE_NGUI_OLD
						else if(buildSpriteType==SpriteType.NGUI){
							binding.type=typeof(UI2DSprite);
						}
						#endif
						else {
							binding.type = typeof(Image);
						}
						binding.path = path+"/"+slot.listAcceptAttachment[x];;
						binding.propertyName = "m_Color.a";
						if (animInfo.color.Count > 0 && animInfo.color [0].time > 0) {
							curve.AddKey (KeyframeUtil.GetNew (0, defaultColor.a, KeyframeUtil.GetTangleMode ("stepped")));
							AEPAnimationCurve curveDataDef = new AEPAnimationCurve (binding, curve);
							listCurve.Add (curveDataDef);
						}
						for (int i = 0; i < animInfo.color.Count; i++) {
							Color color = EditorUtil.HexToColor (animInfo.color [i].color);
							curve.AddKey (KeyframeUtil.GetNew (animInfo.color [i].time, color.a, KeyframeUtil.GetTangleMode (animInfo.color [i].tangentType)));
						}
						AEPAnimationCurve curveData = new AEPAnimationCurve (binding, curve);
						listCurve.Add (curveData);
					}
				}
			}
		}
	}

	public void AddAttactmentAnimationNGUI(AEPSlotAnimationElement animInfo,SpriteType buildSpriteType, SpineSlotElelement slot,GameObject aepRoot,AEPJsonFinal jsonFinal)
	{
		string path=jsonFinal.GetFullPathBone(slot.bone)+"/"+slot.name;
		GameObject objReference=GetRefenreceObject(path,aepRoot);
		if(objReference==null)
		{
			Debug.LogError("#211 Not have object reference:"+path);
			return;
		}
		if(animInfo.attachment!=null)
		{
			for(int x=0;x<slot.listAcceptAttachment.Count;x++)
			{
				EditorCurveBinding binding=new EditorCurveBinding();
				AnimationCurve curve=new AnimationCurve();
				binding.type=typeof(GameObject);
				binding.path=path+"/"+slot.listAcceptAttachment[x];
				binding.propertyName="m_IsActive";
				if(animInfo.attachment.Count>0)// gia tri default
				{
					if(animInfo.attachment[0].time>0)
					{
						Keyframe k=new Keyframe();
						//k.inTangent=1000;
						//k.outTangent=1000;
						k.tangentMode=31;
						if(slot.listAcceptAttachment[x]==slot.attachment)
						{
							k.value=1;
						}
						else
						{
							k.value=0;
						}
						k.time=0;
						curve.AddKey(KeyframeUtil.GetNew(0, k.value , TangentMode.Stepped));
					}
				}
				for(int i=0;i<animInfo.attachment.Count;i++)
				{
					AEPAnimationAttachment attact=animInfo.attachment[i];
					Keyframe k=new Keyframe();
					//k.inTangent=1000;
					//k.outTangent=1000;
					k.tangentMode=31;
					k.time=attact.time;
					if(attact.name==null||attact.name.Length<1)
						k.value=0;
					else if(slot.listAcceptAttachment[x]==attact.name)
					{
						k.value=1;
					}
					else
					{
						k.value=0;
					}
					curve.AddKey(KeyframeUtil.GetNew(attact.time, k.value , TangentMode.Stepped));
				}
				//curve.
				AEPAnimationCurve curveData=new AEPAnimationCurve(binding,curve);
				listCurve.Add(curveData);
			}
		}
		if(animInfo.color!=null)
		{
			for (int x = 0; x < slot.listAcceptAttachment.Count; x++) {
				Transform trans= objReference.transform.Find(slot.listAcceptAttachment [x]);
				if (trans != null) {
					Color defaultColor = Color.white;
					if (buildSpriteType == SpriteType.SpriteRenderer) {
						SpriteRenderer render = trans.GetComponent<SpriteRenderer> ();
						if (render != null) {
							defaultColor = render.color;
						}
					} 
					#if DEFINE_NGUI
					else if(buildSpriteType==SpriteType.NGUI){
						UI2DSprite render2 = trans.GetComponent<UI2DSprite> ();
						if (render2 != null) {
							defaultColor = render2.color;
						}
					}
					#endif
					else {
						Image render3 = trans.GetComponent<Image> ();
						if (render3 != null) {
							defaultColor = render3.color;
						}
					}
					//r
					{
						EditorCurveBinding binding = new EditorCurveBinding ();
						AnimationCurve curve = new AnimationCurve ();
						if (buildSpriteType == SpriteType.SpriteRenderer) {
							binding.type = typeof(SpriteRenderer);
						} 
						#if DEFINE_NGUI
						else if(buildSpriteType==SpriteType.NGUI){
							binding.type=typeof(UI2DSprite);
						}
						#endif
						else {
							binding.type = typeof(Image);
						}
						binding.path = path+"/"+slot.listAcceptAttachment[x];

						binding.propertyName = "m_Color.r";
						if (animInfo.color.Count > 0 && animInfo.color [0].time > 0) {
							curve.AddKey (KeyframeUtil.GetNew (0, defaultColor.r, KeyframeUtil.GetTangleMode ("stepped")));
							AEPAnimationCurve curveDataDef = new AEPAnimationCurve (binding, curve);
							listCurve.Add (curveDataDef);
						}
						for (int i = 0; i < animInfo.color.Count; i++) {
							Color color = EditorUtil.HexToColor (animInfo.color [i].color);
							curve.AddKey (KeyframeUtil.GetNew (animInfo.color [i].time, color.r, KeyframeUtil.GetTangleMode (animInfo.color [i].tangentType)));
						}
						AEPAnimationCurve curveData = new AEPAnimationCurve (binding, curve);
						listCurve.Add (curveData);
					}
					//b
					{
						EditorCurveBinding binding = new EditorCurveBinding ();
						AnimationCurve curve = new AnimationCurve ();
						if (buildSpriteType == SpriteType.SpriteRenderer) {
							binding.type = typeof(SpriteRenderer);
						}
						#if DEFINE_NGUI
						else if(buildSpriteType==SpriteType.NGUI){
							binding.type=typeof(UI2DSprite);
						}
						#endif
						else {
							binding.type = typeof(Image);
						}
						binding.path = path+"/"+slot.listAcceptAttachment[x];;
						binding.propertyName = "m_Color.b";
						if (animInfo.color.Count > 0 && animInfo.color [0].time > 0) {
							curve.AddKey (KeyframeUtil.GetNew (0, defaultColor.b, KeyframeUtil.GetTangleMode ("stepped")));
							AEPAnimationCurve curveDataDef = new AEPAnimationCurve (binding, curve);
							listCurve.Add (curveDataDef);
						}
						for (int i = 0; i < animInfo.color.Count; i++) {
							Color color = EditorUtil.HexToColor (animInfo.color [i].color);
							curve.AddKey (KeyframeUtil.GetNew (animInfo.color [i].time, color.b, KeyframeUtil.GetTangleMode (animInfo.color [i].tangentType)));
						}
						AEPAnimationCurve curveData = new AEPAnimationCurve (binding, curve);
						listCurve.Add (curveData);
					}
					//g
					{
						EditorCurveBinding binding = new EditorCurveBinding ();
						AnimationCurve curve = new AnimationCurve ();
						if (buildSpriteType == SpriteType.SpriteRenderer) {
							binding.type = typeof(SpriteRenderer);
						}
						#if DEFINE_NGUI
						else if(buildSpriteType==SpriteType.NGUI){
							binding.type=typeof(UI2DSprite);
						}
						#endif
						else {
							binding.type = typeof(Image);
						}
						binding.path = path+"/"+slot.listAcceptAttachment[x];;
						binding.propertyName = "m_Color.g";
						if (animInfo.color.Count > 0 && animInfo.color [0].time > 0) {
							curve.AddKey (KeyframeUtil.GetNew (0, defaultColor.g, KeyframeUtil.GetTangleMode ("stepped")));
							AEPAnimationCurve curveDataDef = new AEPAnimationCurve (binding, curve);
							listCurve.Add (curveDataDef);
						}
						for (int i = 0; i < animInfo.color.Count; i++) {
							Color color = EditorUtil.HexToColor (animInfo.color [i].color);
							curve.AddKey (KeyframeUtil.GetNew (animInfo.color [i].time, color.g, KeyframeUtil.GetTangleMode (animInfo.color [i].tangentType)));
						}
						AEPAnimationCurve curveData = new AEPAnimationCurve (binding, curve);
						listCurve.Add (curveData);
					}
					//a
					{
						EditorCurveBinding binding = new EditorCurveBinding ();
						AnimationCurve curve = new AnimationCurve ();
						if (buildSpriteType == SpriteType.SpriteRenderer) {
							binding.type = typeof(SpriteRenderer);
						}	
						#if DEFINE_NGUI
						else if(buildSpriteType==SpriteType.NGUI){
							binding.type=typeof(UI2DSprite);
						}
						#endif
						else {
							binding.type = typeof(Image);
						}
						binding.path = path+"/"+slot.listAcceptAttachment[x];;
						binding.propertyName = "m_Color.a";
						if (animInfo.color.Count > 0 && animInfo.color [0].time > 0) {
							curve.AddKey (KeyframeUtil.GetNew (0, defaultColor.a, KeyframeUtil.GetTangleMode ("stepped")));
							AEPAnimationCurve curveDataDef = new AEPAnimationCurve (binding, curve);
							listCurve.Add (curveDataDef);
						}
						for (int i = 0; i < animInfo.color.Count; i++) {
							Color color = EditorUtil.HexToColor (animInfo.color [i].color);
							curve.AddKey (KeyframeUtil.GetNew (animInfo.color [i].time, color.a, KeyframeUtil.GetTangleMode (animInfo.color [i].tangentType)));
						}
						AEPAnimationCurve curveData = new AEPAnimationCurve (binding, curve);
						listCurve.Add (curveData);
					}
				}
			}
		}
	}

	public void AddStartVisible(string boneName,string path,bool isVisible)
	{
		EditorCurveBinding binding=new EditorCurveBinding();
		AnimationCurve curve=new AnimationCurve();
		binding.type=typeof(GameObject);
		binding.path=path;
		binding.propertyName="m_IsActive";
		if(isVisible)
		{
			curve.AddKey(KeyframeUtil.GetNew(0, 1, TangentMode.Stepped));
		}
		else 
		{
			curve.AddKey(KeyframeUtil.GetNew(0, 0, TangentMode.Stepped));
		}
		AEPAnimationCurve curveData=new AEPAnimationCurve(binding,curve);
		listCurve.Add(curveData);
	}
}
#endregion