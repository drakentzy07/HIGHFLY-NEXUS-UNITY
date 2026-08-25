/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 技能与魔法系统 / Skill and magic system
 * 功能 / Purpose: 管理玩家技能、魔法弹体、范围攻击、命中爆炸、护盾和提示表现。
 * English: Manages player skills, magic projectiles, area attacks, hit explosions, shields, and hint presentation.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hit_explosion : MonoBehaviour
{
    public GameObject Collusion_effect;
    public GameObject keep_effect;

   private void Collusion_counter(Collision other)
   {
        print("火球碰撞物体名字为："+other.gameObject.tag);
        Body_hit_Reaction body_Hit_Reaction = other.transform.GetComponent<Body_hit_Reaction>();
        if(null!=body_Hit_Reaction)
        {
            other.transform.GetComponent<Rigidbody>().AddForceAtPosition(transform.forward * 18000f, other.contacts[0].point);

        }
       GameObject tmp=Instantiate(Collusion_effect,new Vector3(transform.position.x,0,transform.position.z),Quaternion.identity);
       if(null!=keep_effect)
       Instantiate(keep_effect,new Vector3(transform.position.x,0,transform.position.z),Quaternion.identity);
       //Destroy(tmp,5f);
       Destroy(transform.gameObject);;
   }

   private void OnCollisionEnter(Collision other) {
       Collusion_counter(other);
   }
}
