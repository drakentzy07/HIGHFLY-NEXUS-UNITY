/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 枪械控制 / Gun control
 * 功能 / Purpose: 管理玩家或敌人的枪械射击、弹道、动画和输入响应。
 * English: Manages player or enemy firearm shooting, ballistics, animation, and input response.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemy_gunController : MonoBehaviour
{
   // public bool is_special_attack;
    public GameObject bullet;
    public ParticleSystem fire_effect;
    public ParticleSystem[] fire_effectSet;
    public GameObject ready_effect;
    public GameObject special_bullet;
    public MeshRenderer meshRenderer;

    public void enable_spec_weapon()
    {
        meshRenderer.enabled=true;
    }

    public void  end_all()
    {   
        meshRenderer.enabled=false;
        special_bullet.SetActive(false);
    }

    public void activate_bullet()
    {
        special_bullet.SetActive(true);
    }

    public void close_ready()
    {
        ready_effect.gameObject.SetActive(false);
    }

    public void activate_fireSet()
    {
            foreach (var item in fire_effectSet)
            {
                item.Play();
            }
    }

    public void start_hill()
    {
        ready_effect.SetActive(true);
    }

    
}
