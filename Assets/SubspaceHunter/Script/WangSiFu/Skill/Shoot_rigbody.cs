/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 技能与魔法系统 / Skill and magic system
 * 功能 / Purpose: 管理玩家技能、魔法弹体、范围攻击、命中爆炸、护盾和提示表现。
 * English: Manages player skills, magic projectiles, area attacks, hit explosions, shields, and hint presentation.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot_rigbody : MonoBehaviour
{
   public GameObject Fire_prefab;

  
    // Start is called before the first frame update
   
    public void Shoot_fire()
    {
      
        GameObject fire_ball=Instantiate(Fire_prefab,transform.position,transform.rotation) ;
        fire_ball.transform.SetParent(null);
        fire_ball.GetComponent<Rigidbody>().AddForce(fire_ball.transform.forward*25,ForceMode.Impulse);
    }
}
