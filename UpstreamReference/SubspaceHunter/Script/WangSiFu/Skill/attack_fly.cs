/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 技能与魔法系统 / Skill and magic system
 * 功能 / Purpose: 管理玩家技能、魔法弹体、范围攻击、命中爆炸、护盾和提示表现。
 * English: Manages player skills, magic projectiles, area attacks, hit explosions, shields, and hint presentation.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class attack_fly : MonoBehaviour
{
    public enum hit_type{
        碰撞无影响,
        碰撞火花不消失,
        碰撞火花并消失
    }
    public float speed = 25f; // 子弹飞行速度

    public float lifeSpan = 2f; // 生命期间为5秒

    private bool is_minusHp;
    public float minus_hp_amount=10f;
    public hit_type 碰撞类型;
    public GameObject 碰撞效果;
    public GameObject 碰撞声音;
     [Range(1,10)]
    public float 对武器的伤害=3f;
    
    // Start is called before the first frame update
    void Start()
    {
        Destroy(transform.gameObject,lifeSpan);
    }
    private void OnTriggerEnter(Collider other) {
        print("碰到的其它物体是"+other.gameObject.name);
        if(other.tag=="Player_body"&&!is_minusHp)
        {
            is_minusHp=true;
            print("碰到玩家");//打到玩家扣血
            other.transform.GetComponent<Player_managerV2>().Minus_Hp(minus_hp_amount,"切割");
            
        }else if(other.tag=="Weapon_inHand"||other.tag=="Weapon_inRightHand")
        {
            other.transform.GetComponent<Physic_weaponManager>().Deal_short_impusle();
            other.transform.GetComponent<Physic_weaponManager>().weapon_Durability.Mins_dur(对武器的伤害);
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

    private void close_controller_impulse()
    {
        OVRInput.SetControllerVibration(0f,0f,OVRInput.Controller.Active);
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

    // Update is called once per frame
    void Update()
    {
        // 子弹沿着前进方向移动
        transform.position += transform.forward * speed * Time.deltaTime;

    }
}
