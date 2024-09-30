using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class SpineTest : MonoBehaviour
{
    // Start is called before the first frame update
    // [SerializeField] private SkeletonGraphic spinePlane;
    // [SerializeField] private GameObject iconUp;
    // [SerializeField] private GameObject iconDown;
    // [SerializeField] private GameObject startNode;
    [SerializeField] private GameObject model;
    [SerializeField] private Image target;
    void Start()
    {

        Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Init()
    {
        bool isUp = false;
        // spinePlane.AnimationState.SetAnimation(0, "hongzha_shang", false).Complete+= OnSpineComplete;
        // BindBone( skeletonAnimation,iconUp,"bone");
        // BindBone( skeletonAnimation,iconDown,"zi");
        // BindBone( skeletonAnimation,startNode.gameObject,"bone3");
        // skeletonAnimation.AnimationState.SetAnimation(0, "animation", false).Complete += (entry) =>
        // {
        //     Debug.LogError("finish");
        // };
    }
    
    private void OnSpineComplete(TrackEntry entry)
    {
        Debug.Log("OnSpineComplete");
    }

    private void BindBone(SkeletonGraphic spineAni, GameObject go, string boneName)
    {
        
        var skeletonUtility = spineAni.GetComponent<SkeletonUtility>();
        if (skeletonUtility == null)
        {
            skeletonUtility = spineAni.gameObject.AddComponent<SkeletonUtility>();
        }

        if (skeletonUtility.boneRoot == null)
        {
            skeletonUtility.boneRoot = spineAni.transform;
        }
        
        
        var bone = spineAni.Skeleton.FindBone(boneName);
        if (bone == null)
        {
            return;
        }
        
        var newBoneGameObject = new GameObject(boneName);
        newBoneGameObject.transform.SetParent(spineAni.transform, false);
        var skeletonUtilityBone = newBoneGameObject.AddComponent<SkeletonUtilityBone>();
        skeletonUtilityBone.boneName = boneName;
        skeletonUtilityBone.position = true;
        skeletonUtilityBone.rotation = true;
        skeletonUtilityBone.scale = true;
        go.transform.SetParent(newBoneGameObject.transform, false);
    }
}
