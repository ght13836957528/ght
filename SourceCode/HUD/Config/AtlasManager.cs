using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UW.HUD
{
    [System.Serializable]
    public struct SpriteConfig
    {
        public string[] HeadSpriteNames;
        public string SpriteName;
        [HideInInspector]
        public int[] HeadIDs;
        public int SpriteId;
        public int Width;
        public int Height;
        public JumpWorldType JPType;
        public JumpWorldSpriteType JPSpriteType;
        public string AnimationName;
        [HideInInspector]
        public int[] NumberSpriteId;
    }
    public class AtlasManager : MonoBehaviour
    {
        public TextAsset AtlasConfig;
        public Texture AtlasPng;
        public Shader HudSprite;

        public SpriteConfig BloodBk;
        public SpriteConfig BloodRed;
        public SpriteConfig BloodBlue;
        public SpriteConfig[] JumpWord;
        public Dictionary<JumpWorldType, SpriteConfig> DicJumpWord = new Dictionary<JumpWorldType, SpriteConfig>();

        public void InitJumpWordSpriteConfig(AtlasContext atlasContext)
        {
            for(int i=0; i<JumpWord.Length; ++i)
            {
                var jumpWord = JumpWord[i];
                if(jumpWord.JPSpriteType == JumpWorldSpriteType.Number)
                {
                    int[] treatmentNum = new int[10];
                    for (int k = 0; k < 10; ++k)
                    {
                        treatmentNum[k] = atlasContext.SpriteNameToID($"{jumpWord.SpriteName}{k}");
                    }
                    jumpWord.NumberSpriteId = treatmentNum;
                    if (jumpWord.HeadSpriteNames.Length > 0)
                    {
                        jumpWord.HeadIDs = new int[jumpWord.HeadSpriteNames.Length];
                        for (int index = 0; index < jumpWord.HeadSpriteNames.Length; index++)
                        { 
                            jumpWord.HeadIDs[index] = atlasContext.SpriteNameToID(jumpWord.HeadSpriteNames[index]);
                        }
                    }
                  
                    //if (jumpWord.HeadSpriteName != null)
                    //{
                    //    jumpWord.HeadID = atlasContext.SpriteNameToID(jumpWord.HeadSpriteName);
                    //}
                }
                else if(jumpWord.JPSpriteType == JumpWorldSpriteType.Word)
                {
                    jumpWord.SpriteId = atlasContext.SpriteNameToID(jumpWord.SpriteName);
                    if (jumpWord.HeadSpriteNames.Length > 0)
                    {
                        jumpWord.HeadIDs = new int[jumpWord.HeadSpriteNames.Length];
                        for (int index = 0; index < jumpWord.HeadSpriteNames.Length; index++)
                        {
                            jumpWord.HeadIDs[index] = atlasContext.SpriteNameToID(jumpWord.HeadSpriteNames[index]);
                        }
                    }
                }
                else
                {
                    Debug.LogError($"{jumpWord.SpriteName}  JumpWorldSpriteType is None");
                }
                DicJumpWord.Add(jumpWord.JPType, jumpWord);
            }
        }
        public void Release()
        {
            DicJumpWord.Clear();
        }
    }

}
