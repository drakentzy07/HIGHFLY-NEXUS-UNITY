/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 动画驱动射击 / Animation-driven shooting
 * 功能 / Purpose: 通过动画事件触发子弹发射或枪械反馈。
 * English: Triggers bullet firing or firearm feedback through animation events.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anime_controllerShoot : MonoBehaviour
{
    public Gun_Controller gun_Controller;


    public void shoot()
    {
        gun_Controller.动画子弹射出();
    }

    public void Reset_shoot()
    {
        gun_Controller.动画射出结束();
    }

    
  
  
}
