/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 场景剔除 / Scene culling
 * 功能 / Purpose: 根据场景需求控制地面或环境对象的剔除显示。
 * English: Controls culling visibility for ground or environment objects based on scene needs.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground_Culling : MonoBehaviour
{
    public Material groundMaterial; // 地面的材质
    public GameObject areaCube; // 指向你设置的 Cube 物体

    void Update()
    {
        // 获取 Cube 的位置和大小
        Vector3 boundsCenter = areaCube.transform.position;
        Vector3 boundsSize = areaCube.transform.localScale; // 使用 Cube 的缩放作为大小

        // 更新材质的参数
        groundMaterial.SetVector("_BoundsCenter", boundsCenter);
        groundMaterial.SetVector("_BoundsSize", boundsSize);
    }
}
