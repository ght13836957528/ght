using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.U2D;

public static class AtlasUtils
{
    public static Dictionary<string, Dictionary<string, Sprite>> m_cacheSprite = new Dictionary<string, Dictionary<string, Sprite>>();
       
    public static Sprite GetOrAddCacheSprite(SpriteAtlas atlas, string spriteFile)
    {
        if (atlas == null || string.IsNullOrEmpty(spriteFile))
            return null;

        string spriteName = spriteFile;

        if (spriteFile.Contains("/"))
            spriteName = Path.GetFileNameWithoutExtension(spriteFile);

        Sprite sprite = null;
        if (m_cacheSprite.TryGetValue(atlas.name, out var sprites))
        {
            if (sprites.TryGetValue(spriteName, out sprite))
                return sprite;
        }
        else
        {
            sprites = new Dictionary<string, Sprite>();
            m_cacheSprite.Add(atlas.name, sprites);
        }

        sprite = atlas.GetSprite(spriteName);
        sprites.Add(spriteName, sprite);
        return sprite;
    }


    public static void ClearCacheSprite(string atlasName)
    {
        if (!string.IsNullOrEmpty(atlasName))
        {
            m_cacheSprite.Remove(atlasName);
        }
    }

    public static void ClearAllCacheSprites()
    {
        m_cacheSprite.Clear();
    }
}
