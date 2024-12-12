using GameManager;
using UnityEngine;

public class _AutoUnloader : MonoBehaviour
{
    public object asset;

    private void OnDestroy()
    {
        Release();
    }

    public void Release()
    {
        if (asset != null)
        {
            ResourceHelper.UnloadAssetWithObject(asset);
            asset = null;
        }
    }

    public static void BindUnloader(GameObject go, object asset)
    {
        go.GetOrAddComponent<_AutoUnloader>().asset = asset;
    }
}