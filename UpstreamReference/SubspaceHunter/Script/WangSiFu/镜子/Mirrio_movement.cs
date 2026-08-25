/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 镜像/镜子表现 / Mirror presentation
 * 功能 / Purpose: 处理镜子或镜像对象的运动同步表现。
 * English: Handles movement synchronization for mirrors or mirrored objects.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mirrio_movement : MonoBehaviour
{
  //  public Transform PlayerTarget;
    public Transform Mirror;
 

    // Update is called once per frame
    void Update()
    {
        Vector3 localPlayer = Mirror.InverseTransformPoint(Camera.main.transform.position);
        transform.position = Mirror.TransformPoint(new Vector3(localPlayer.x,localPlayer.y,-localPlayer.z));
        Vector3 look_at_Mirror = Mirror.TransformPoint(new Vector3(-localPlayer.x,localPlayer.y,localPlayer.z));
        transform.LookAt(look_at_Mirror);
    }
}
