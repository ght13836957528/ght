using System;
using System.IO;
using UnityEditor;

public static class AssetBundleUtility
{
    public enum AssetBundleNameType
    {
        None,
        File,
        FileWithoutExtension,
        Folder,
    }

    public static void SetAssetBundleNull(string path)
    {
        if (string.IsNullOrEmpty(path))
            return ;

        var importer = AssetImporter.GetAtPath(path);
        if (importer == null)
            return;
        if (importer.assetBundleName == string.Empty)
            return;
        
        
        importer.assetBundleName = string.Empty;

        importer.assetBundleVariant = string.Empty;

    }
    
    public static bool ClearAssetBundleName(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;
        
        var importer = AssetImporter.GetAtPath(path);
        if (importer == null)
            return false;

        bool change = false;
        
        if(!string.IsNullOrEmpty(importer.assetBundleName) || !string.IsNullOrEmpty(importer.assetBundleVariant) )
        {
            change = true;
            importer.SetAssetBundleNameAndVariant("null", "bundle");
            importer.SetAssetBundleNameAndVariant(null, null);
        }

        return change;
    }

    
    public static bool SetAssetBundleNameWithGUID(string guid, AssetBundleNameType nameType, string assetBundleVariant = "bundle")
    {
        if (string.IsNullOrEmpty(guid))
        {
            return false;
        }

        return SetAssetBundleName(AssetDatabase.GUIDToAssetPath(guid), nameType, assetBundleVariant);
    }

    public static bool SetAssetBundleName(string path, AssetBundleNameType nameType, string assetBundleVariant = "bundle")
    {
        if (string.IsNullOrEmpty(path))
            return false;
        
        var importer = AssetImporter.GetAtPath(path);
        if (importer == null)
            return false;

        bool change = false;

        string assetBundleName = GetAssetBundleName(path,nameType);
        // Debug.LogError($"{path} : {assetBundleName}");
        if(importer.assetBundleName != assetBundleName)
        {
            change = true;
            importer.assetBundleName = assetBundleName;
        }

        if(importer.assetBundleVariant != assetBundleVariant)
        {
            change = true;
            importer.assetBundleVariant = assetBundleVariant;
        }

        return change;
    }
    
    public static bool SetAssetBundleName(string path, string assetBundleName , string assetBundleVariant = "bundle")
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }
        
        var importer = AssetImporter.GetAtPath(path);
        if (importer == null)
        {
            return false;
        }

        // Debug.LogError($"{path} : {assetBundleName}");
        importer.assetBundleName = assetBundleName;
        importer.assetBundleVariant = assetBundleVariant;

        return true;
    }
    

    public static string GetAssetBundleName(string path, AssetBundleNameType nameType)
    {
        string assetBundleName = "";
        switch (nameType)
        {
            case AssetBundleNameType.File:
                assetBundleName = Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, Path.GetFileName(path));
                break;
            case AssetBundleNameType.FileWithoutExtension:
                assetBundleName = Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, Path.GetFileNameWithoutExtension(path));
                break;
            case AssetBundleNameType.Folder:
                assetBundleName = Path.GetDirectoryName(path);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(nameType), nameType, null);
        }

        return assetBundleName;
    }
}