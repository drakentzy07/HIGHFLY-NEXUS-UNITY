/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 楼层与关卡阶段控制 / Floor and stage control
 * 功能 / Purpose: 管理关卡楼层、阶段推进和场景演示状态。
 * English: Manages floor levels, stage progression, and scene demo state.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage_Controller : MonoBehaviour
{
    public GameObject[] Close_GameObject_set;
    public GameObject[] Open_GameObject_set;
    public Transform Player_location;
    public float no_pass_z;
 

    // Start is called before the first frame update
    public void  Check_player_isPass()
    {
                    print("判断进入");
            if(Player_location.position.z>no_pass_z)
            {
                        print("判断大于");
                        for(int flag=0;flag<Close_GameObject_set.Length;flag++)
                        {
                            Close_GameObject_set[flag].SetActive(false);
                        }
                        for(int flag=0;flag<Open_GameObject_set.Length;flag++)
                        {
                            Open_GameObject_set[flag].SetActive(true);
                        }
            }
    }
}
