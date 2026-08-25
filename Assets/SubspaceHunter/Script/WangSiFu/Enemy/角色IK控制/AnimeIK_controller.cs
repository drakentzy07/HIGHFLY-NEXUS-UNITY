/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 敌人角色 IK 控制 / Enemy character IK control
 * 功能 / Purpose: 驱动敌人动画 IK，使武器、手部和目标方向保持一致。
 * English: Drives enemy animation IK so weapons, hands, and target directions stay aligned.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimeIK_controller : MonoBehaviour
{
    [Range(0,1)]
    public float 右手IK=1f;
    [Range(0,1)]
    public float 左手IK=1f;
    [Range(0,1)]
    public float 右脚IK=1f;
    [Range(0,1)]
    public float 左脚IK=1f;



    private void OnAnimatorIK(int layerIndex) {
        // GetComponent<Animator>().SetIKPosition(AvatarIKGoal.RightFoot, new Vector3(0, 0, 0));
        GetComponent<Animator>().SetIKPositionWeight(AvatarIKGoal.RightHand, 右手IK);
        GetComponent<Animator>().SetIKPositionWeight(AvatarIKGoal.LeftHand, 左手IK);
        GetComponent<Animator>().SetIKPositionWeight(AvatarIKGoal.RightFoot, 右脚IK);
        GetComponent<Animator>().SetIKPositionWeight(AvatarIKGoal.LeftFoot, 左脚IK);
    }

}
