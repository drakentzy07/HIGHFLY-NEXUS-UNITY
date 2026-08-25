/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 九宫格技能测试 / Grid skill test
 * 功能 / Purpose: 用于测试九宫格斩击、随机剑气和技能演示流程。
 * English: Used to test grid-based slashes, random blade waves, and skill demo flow.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skil_testManager : MonoBehaviour
{
    public once_slashGer 剑气横斩;
    public Transform 中心技能生成点;
    public random_slashfly 三道剑气;
    public GameObject 十字斩;
    public GameObject 爆破;


    public Transform 提示生成点;
    public GameObject 爆破提示;
    int 爆破提示次数;
    public GameObject 三道剑气提示;
    int 三道剑气提示次数;
    public GameObject 十字斩提示;
    int 十字斩提示次数;
    public GameObject 剑气横斩提示;
    int 剑气横斩提示次数;

    public void 生成横斩()
    {
        剑气横斩.Ger_flySlash();
        
    }


    public void 生成爆破()
    {
        Instantiate(爆破,中心技能生成点);
        
    }

    public void 生成十字斩()
    {
        Instantiate(十字斩,中心技能生成点);
       
    }

    public void 生成剑气三连击()
    {
        三道剑气.Gerner_slash();
        
    }

    public void 横斩提示生成()
    {
        if(剑气横斩提示次数<3)
        {
            剑气横斩提示次数++;
            Instantiate(剑气横斩提示,提示生成点);
        }
    }

    public void 十字斩提示生成()
    {
        if(十字斩提示次数<3)
        {
            十字斩提示次数++;
            Instantiate(十字斩提示,提示生成点);
        }
    }

    public void 爆破提示生成()
    {
        if(爆破提示次数<3)
        {
            爆破提示次数++;
            Instantiate(爆破提示,提示生成点);
        }
    }

    public void 三道剑气提示生成()
    {
        if(三道剑气提示次数<3)
        {
            三道剑气提示次数++;
            Instantiate(三道剑气提示,提示生成点);
        }
    }
    
}
