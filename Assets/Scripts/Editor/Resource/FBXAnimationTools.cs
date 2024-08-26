using System;
using System.Collections.Generic;
using System.IO;
using GameManager;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PrefabTools
{
    public class FBXAnimationTools : EditorWindow
    {
        class AnimationInfo
        {
            public string m_fullPath;
            public string m_assetOriPath;
            public string m_assetPath;
            public string m_finalName;
            public string m_fbxName;
            public string m_animName;
            public string m_animDir;
            public string m_animPath;
        }

        private static string DATA_PATH_PREFIX = "Assets";
        private string _fbxFolder = "";
        
        [MenuItem("Tools/通用工具/提取clip", false, 4)]
        static void AnimationTools()
        {
            GetWindow<FBXAnimationTools>("提取clip");
        }
        
        void OnGUI()
        {
            _fbxFolder = EditorGUILayout.TextField("CocosProjectRoot", _fbxFolder);

            if (GUILayout.Button("开始提取"))
            {
                Process(_fbxFolder);
            }
        }
        
        private static void Process(string folderPath)
        {
            if (!IsDirectoryValid(folderPath))
            {
                return;
            }

            var infoMap = TraverseDirectoryFBX(folderPath);
            foreach (var infos in infoMap)
            {
                GenerateAnimFiles(infos.Value);
                GenerateAnimatorController(infos.Key, infos.Value);
                GeneratePrefab(infos.Key);
            }

            AssetDatabase.Refresh();
            // EditorUtility.DisplayDialog("提示", "处理完成", "OK");
        }

        private static bool IsDirectoryValid(string folderPath)
        {
            folderPath = folderPath.Replace("\\", "/");

            if (string.IsNullOrEmpty(folderPath))
            {
                EditorUtility.DisplayDialog("提示", "没有填写路径", "OK");
                return false;
            }

            if (!folderPath.StartsWith(DATA_PATH_PREFIX))
            {
                EditorUtility.DisplayDialog("提示", "路径格式错误", "OK");
                return false;
            }

            string fullPath = Application.dataPath;
            fullPath = fullPath.Substring(0, fullPath.Length - DATA_PATH_PREFIX.Length);
            fullPath = Path.Combine(fullPath, folderPath);
            if (!Directory.Exists(fullPath))
            {
                EditorUtility.DisplayDialog("提示", "路径不存在", "OK");
                return false;
            }

            return true;
        }

        private static Dictionary<string, List<AnimationInfo>> TraverseDirectoryFBX(string folderPath)
        {
            Dictionary<string, List<AnimationInfo>> infoMap = new();

            string[] fbxPaths = Directory.GetFiles(folderPath, "*.fbx", SearchOption.AllDirectories);
            foreach (string path in fbxPaths)
            {
                if (!path.Contains("@")) // 不是动画文件
                {
                    continue;
                }

                string fullPath = path.Replace("\\", "/");
                string assetPath = fullPath.Substring(fullPath.IndexOf(DATA_PATH_PREFIX, StringComparison.Ordinal));
                string finalName = assetPath.Split("/")[^1];
                string fbxName = finalName.Split("@")[0];
                string animName = finalName.Split("@")[1].Replace(".fbx", "", StringComparison.OrdinalIgnoreCase);
                string animDir = assetPath.Replace(finalName, $"anim");
                string assetOriPath = assetPath.Replace($"@{animName}", "");

                AnimationInfo info = new();
                info.m_fullPath = fullPath;
                info.m_assetPath = assetPath;
                info.m_finalName = finalName;
                info.m_fbxName = fbxName;
                info.m_animName = animName;
                info.m_animDir = animDir;
                info.m_animPath = Path.Combine(animDir, animName + ".anim");
                info.m_assetOriPath = assetOriPath;

                if (!infoMap.ContainsKey(info.m_assetOriPath))
                {
                    infoMap.Add(info.m_assetOriPath, new());
                }

                infoMap[info.m_assetOriPath].Add(info);
            }

            return infoMap;
        }

        private static void GenerateAnimFiles(List<AnimationInfo> infos)
        {
            foreach (var item in infos)
            {
                if (!string.IsNullOrEmpty(item.m_animDir)  && !Directory.Exists(item.m_animDir))
                {
                    Directory.CreateDirectory(item.m_animDir);
                }

                AnimationClip clip = new AnimationClip();
                AnimationClip fbxClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(item.m_assetPath);
                if (fbxClip != null)
                {
                    AssetDatabase.DeleteAsset(item.m_animPath);
                    EditorUtility.CopySerialized(fbxClip, clip);
                    AssetDatabase.CreateAsset(clip, item.m_animPath);
                    AssetDatabase.DeleteAsset(item.m_assetPath);
                }
            }
        }

        private static void GenerateAnimatorController(string assetPath, List<AnimationInfo> infos)
        {
            string ctrlAssetPath = assetPath.Replace(".fbx", ".controller", StringComparison.OrdinalIgnoreCase);
            string ctrlRealPath = Path.Combine(Application.dataPath, ctrlAssetPath.Substring(DATA_PATH_PREFIX.Length + 1));
            AnimatorController ctrl;
            if (File.Exists(ctrlRealPath))
            {
                ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlAssetPath);
            }
            else
            {
                ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlAssetPath);
            }

            
            if(ctrl.layers.Length == 0)
            {
                ctrl.AddLayer("Base Layer");
            }
            AnimatorControllerLayer layer = ctrl.layers[0];
            foreach (var item in infos)
            {
                AnimatorState state = null;
                for (int i = 0; i < layer.stateMachine.states.Length; i++)
                {
                    if(item.m_animName.Equals(layer.stateMachine.states[i].state.name))
                    {
                        state = layer.stateMachine.states[i].state;
                    }
                }

                if (state == null)
                { 
                    state = layer.stateMachine.AddState(item.m_animName);
                }

                state.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(item.m_animPath);
                if (state.name.Equals("idle", StringComparison.OrdinalIgnoreCase))
                {
                    if (layer.stateMachine.defaultState == null)
                    {
                        layer.stateMachine.defaultState = state;
                    }
                }
            }

            AssetDatabase.SaveAssets();
        }

        private static void GeneratePrefab(string assetPath)
        {
            string ctrlAssetPath = assetPath.Replace(".fbx", ".controller", StringComparison.OrdinalIgnoreCase);
            AnimatorController ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlAssetPath);

            string finalName = assetPath.Split("/")[^1];
            string fbxName = finalName.Replace(".fbx", "", StringComparison.OrdinalIgnoreCase);

            string[] patterns = assetPath.Split("/");
            if (patterns.Length > 2)
            {
                string prefabDir = string.Join("/", patterns, 0, patterns.Length - 2);
                string prefabPath = Path.Combine(prefabDir, fbxName + ".prefab");
                GameObject prefab;
                if (!File.Exists(prefabPath))
                {
                    prefabPath = prefabPath.Substring(prefabPath.IndexOf(DATA_PATH_PREFIX, StringComparison.Ordinal));
                    PrefabUtility.CreateEmptyPrefab(prefabPath);
                }
                
                prefabPath = prefabPath.Substring(prefabPath.IndexOf(DATA_PATH_PREFIX, StringComparison.Ordinal));
                prefab = PrefabUtility.LoadPrefabContents(prefabPath);
                
                GameObject instant = Object.Instantiate(prefab);
                instant.name = instant.name.Replace("(Clone)", "");
                GameObject instantModel = null;
                
                for (int i =  instant.transform.childCount - 1; i >= 0; i--)
                {
                    var child =  instant.transform.GetChild(i);
                    GameObject.DestroyImmediate(child.gameObject);
                   
                }
                GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                instantModel = Object.Instantiate(model);
                instantModel.name = instantModel.name.Replace("(Clone)", "");
                instant.transform.SetParent(instantModel.transform);

                // var sprite3D = instant.GetOrAddComponent<IFSprite3D>();
                // sprite3D.pModel = instantModel;
                // var renders = instant.GetComponentsInChildren<SkinnedMeshRenderer>();
                // if (renders != null && renders.Length > 0) {
                //     foreach (var render in renders) {
                //         sprite3D.AddMesh(render);
                //     }
                // }

                Animator animator = instantModel.GetOrAddComponent<Animator>();
                animator.runtimeAnimatorController = ctrl;
                // sprite3D.animator = animator;
                
                PrefabUtility.SaveAsPrefabAsset(instant, prefabPath);
                Object.DestroyImmediate(instant);
                if (instantModel != null)
                {
                    Object.DestroyImmediate(instantModel);
                }
            }
        }
        
        
    }
}
