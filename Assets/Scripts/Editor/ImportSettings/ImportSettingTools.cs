#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class ImportSettingTools : AssetPostprocessor
{
    static string shelterPath = "Assets/Shelter";
    static string scriptPath = "Assets/Shelter/Scripts/";
    static string effectFbxPath = "Assets/Shelter/Arts/Effects/Common/mesh";
    static string billZombieFbxPath = "Assets/Shelter/Arts/Role/LowModel/Pve/Zombies/BillZombie";

    static string importMatPath = "Arts/Effects/Common/mesh/ImportMat";

    //处理导入的模型
    void OnPostprocessModel(GameObject gameObject)
    {
        if (!this.assetPath.Contains(shelterPath))
            return;
        if (this.assetPath.Contains(scriptPath))
            return;

        var src = this.assetImporter as ModelImporter;
        bool changed = false;

        if (this.assetPath.StartsWith(effectFbxPath) || this.assetPath.StartsWith(billZombieFbxPath))
        {
            if (src.isReadable != true)
            {
                changed = true;
                src.isReadable = true;
            }
        }
        else
        {
            if (src.isReadable != false)
            {
                changed = true;
                src.isReadable = false;
            }
        }

        if (!this.assetPath.Contains(importMatPath))
        {
#if UNITY_2019_3_OR_NEWER

            if (src.materialImportMode != ModelImporterMaterialImportMode.None)
            {
                changed = true;

                src.materialImportMode = ModelImporterMaterialImportMode.None;
            }
#else
                if (src.importMaterials)
                {
                    changed = true;

                    src.importMaterials = false;

                }
#endif
        }

        if (changed)
            src.SaveAndReimport();
    }
}


#endif