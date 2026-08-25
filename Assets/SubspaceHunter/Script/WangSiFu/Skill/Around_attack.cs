/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 技能与魔法系统 / Skill and magic system
 * 功能 / Purpose: 管理玩家技能、魔法弹体、范围攻击、命中爆炸、护盾和提示表现。
 * English: Manages player skills, magic projectiles, area attacks, hit explosions, shields, and hint presentation.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Around_attack : MonoBehaviour
{
    public enum Attack_type
    {
        致盲,
        伤害
    }
    public Attack_type _type;
    public float minus_hp_amount=20f;
    public bool is_keep_attack;
    public float damager_interval=0.5f;
    private bool is_interval;
    public bool if_attack_enmey;
    public AudioClip 耳鸣音效片段;
    private void OnTriggerEnter(Collider other) {
       if(if_attack_enmey)
       {
            if(other.tag=="enemy_body")
            {
                other.transform.GetComponent<Body_hit_Reaction>().Deal_body_damage(minus_hp_amount);
                Destroy(transform.gameObject);
            }
       }
       else
       {
            if(other.tag=="Player_body")
            {
                    Deal_attack(other);
            }
       }
        
    }

    private void Deal_attack(Collider other)
    {
        switch(_type)
        {
            case Attack_type.伤害:
            Deal_damager(other);
            break;
            case Attack_type.致盲:
            break;
             
        }
    }

    private void Deal_damager(Collider other)
    {
         if(is_keep_attack)
          Keep_attack(other);
         else
          Once_attack(other);
    }

    private void Once_attack(Collider other)
    {
        other.transform.GetComponent<Player_managerV2>().Minus_Hp(minus_hp_amount,"切割");
        other.transform.GetComponent<AudioSource>().clip=耳鸣音效片段;
        other.transform.GetComponent<AudioSource>().Play();
         Destroy(transform.gameObject);
    }

    private void Keep_attack(Collider other)
    {
        if(!is_interval)
        {
            other.transform.GetComponent<Player_managerV2>().Minus_Hp(minus_hp_amount,"切割");
            is_interval=true;
            Invoke("reset_damage",damager_interval);
        }
    }

    private void reset_damage()
    {
        is_interval=false;
    }


}
