/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 玩家模型控制 / Player model control
 * 功能 / Purpose: 切换玩家模型、性别表现或角色外观状态。
 * English: Switches player model, gender presentation, or character appearance state.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player_modelController : MonoBehaviour
{
    public GameObject[] male;
    public GameObject[] female;
    
    public void Set_male()
    {
        close_female();
        foreach (var item in male)
        {
            item.SetActive(true);
        }
    }

    public void Set_female()
    {
         close_male();
        foreach (var item in female)
        {
            item.SetActive(true);
        }
    }

    public void close_male()
    {
        foreach (var item in male)
        {
            item.SetActive(false);
        }
    }

    public void close_female()
    {
        foreach (var item in female)
        {
            item.SetActive(false);
        }
    }


    public void Switch_model()
    {
        if(male[0].activeSelf)
        {
            Set_female();
        }else
        {
            Set_male();
        }
    }



}
