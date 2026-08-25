/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 敌人射击定位 / Enemy shooting target location
 * 功能 / Purpose: 计算敌人远程攻击或枪械射击的瞄准位置。
 * English: Computes aim positions for enemy ranged attacks or firearm shots.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShoot_location : MonoBehaviour
{
    private Transform player_trans;
    public Transform father;
    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        player_trans = GameObject.FindWithTag("Player_body").transform;
    }
    // Start is called before the first frame update

   public void Enemy_lookAtAdjust1()
    {
        print("进入定位1");
        Vector3 targetpoint = player_trans.position;
        father.transform.LookAt(new Vector3(targetpoint.x, father.position.y, targetpoint.z));
        //GetComponent<CommandExecutor>().Enemy_lookAtAdjust();
    }

}
