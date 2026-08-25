/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 技能与魔法系统 / Skill and magic system
 * 功能 / Purpose: 管理玩家技能、魔法弹体、范围攻击、命中爆炸、护盾和提示表现。
 * English: Manages player skills, magic projectiles, area attacks, hit explosions, shields, and hint presentation.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fly_damageUnit : MonoBehaviour
{
    public enum hit_type{
        碰撞无影响,
        碰撞火花不消失,
        碰撞火花并消失
    }

    private bool is_minusHp;
    public float minus_hp_amount=10f;
    public hit_type 碰撞类型;
    public GameObject 碰撞效果;
    public GameObject 碰撞声音;
    public float 子弹存在时间 = 2f;
    public float 子弹飞行速度= 100f;
     private void OnTriggerEnter(Collider other) {
        print("碰到的其它物体是"+other.gameObject.name+" 并且tag为: "+other.tag);
        if(other.tag=="Player_body"&&!is_minusHp)
        {
            is_minusHp=true;
            print("碰到玩家");//打到玩家扣血
            other.transform.GetComponent<Player_managerV2>().Minus_Hp(minus_hp_amount,"切割");
            
        }else if(other.tag=="Weapon_inHand"||other.transform.tag=="Weapon_inRightHand")
        {
            Physic_weaponManager physic_WeaponManager= other.transform.GetComponent<Physic_weaponManager>();
            if(null!=physic_WeaponManager) physic_WeaponManager.Deal_short_impusle();
            
            switch(碰撞类型)
            {
                case hit_type.碰撞无影响:
                break;
                case hit_type.碰撞火花并消失:
                   Gen_AND_des();
                break;
                case hit_type.碰撞火花不消失:
                  Ger_AND_NotDes();
                break;
            }
        }
    }

    private void OnCollisionEnter(Collision other) {
        print("碰到的其它物体是"+other.gameObject.name+" 并且tag为: "+other.transform.tag);
        if(other.transform.tag=="Player_body"&&!is_minusHp)
        {
            is_minusHp=true;
            print("碰到玩家");//打到玩家扣血并将子弹消除
            other.transform.GetComponent<Player_managerV2>().Minus_Hp(minus_hp_amount,"切割");
            Destroy(transform.gameObject);
            
        }else if(other.transform.tag=="Weapon_inHand"||other.transform.tag=="Weapon_inRightHand")
        {
            Physic_weaponManager physic_WeaponManager= other.transform.GetComponent<Physic_weaponManager>();
            if(null!=physic_WeaponManager) physic_WeaponManager.Deal_short_impusle();
            
            switch(碰撞类型)
            {
                case hit_type.碰撞无影响:
                break;
                case hit_type.碰撞火花并消失:
                   Gen_AND_des();
                break;
                case hit_type.碰撞火花不消失:
                  Ger_AND_NotDes();
                break;
            }
        }
    }


    private void Gen_AND_des()
    {
        Instantiate(碰撞效果,transform.position,Quaternion.identity);
        Instantiate(碰撞声音,transform.position,Quaternion.identity);   
                Destroy(transform.gameObject);
    }

    private void Ger_AND_NotDes()
    {
        Instantiate(碰撞效果,transform.position,Quaternion.identity);
        Instantiate(碰撞声音,transform.position,Quaternion.identity); 
                 
    }


}
