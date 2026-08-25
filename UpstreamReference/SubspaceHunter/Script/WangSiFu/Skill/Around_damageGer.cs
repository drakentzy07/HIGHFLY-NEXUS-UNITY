/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 技能与魔法系统 / Skill and magic system
 * 功能 / Purpose: 管理玩家技能、魔法弹体、范围攻击、命中爆炸、护盾和提示表现。
 * English: Manages player skills, magic projectiles, area attacks, hit explosions, shields, and hint presentation.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Around_damageGer : MonoBehaviour
{
    public GameObject Damage_prefab;
    public float Destroy_time;
    public Transform Ger_Trans;
    public Transform Weapon_RayPoint;
    public LayerMask Ray_layer;  
    public AudioClip 跳劈格挡音效;
    public Physic_weaponDetect physic_WeaponDetect;

    private void Deal_Parring()
    {
        if(physic_WeaponDetect!=null&&physic_WeaponDetect.physic_WeaponManager!=null&&physic_WeaponDetect.physic_WeaponManager.Get_Player()!=null)
        {
            physic_WeaponDetect.physic_WeaponManager.Get_Player().Minus_HeavyParrying();
        }
    }

    public void HitGround_gernerate()
    {
        
      //  print("地面碰撞进入");
        Ray ray1 = new Ray(Weapon_RayPoint.transform.position, Vector3.down);
       // Ray ray = new Ray(Camera.main.transform.position, Input.mousePosition);
        RaycastHit hit1;

        Ray ray2 = new Ray(Weapon_RayPoint.transform.position, Vector3.up);
       // Ray ray = new Ray(Camera.main.transform.position, Input.mousePosition);
        RaycastHit hit2;

        if (Physics.Raycast(ray1, out hit1,100f,Ray_layer) )
        {
            //print("地面碰撞的距离下方："+hit1.distance);
            if(hit1.distance<1.2f)
            {
                Gernerate_effect();
                Skill_effectGenerate skill_EffectGenerate =  GetComponent<Skill_effectGenerate>();
                if(null!=skill_EffectGenerate)
                skill_EffectGenerate.Gernerate_effect();
            }else
            {
                Deal_Parring();
                GetComponent<AudioSource>().clip=跳劈格挡音效;
                GetComponent<AudioSource>().Play();
            }
            
        }else if(Physics.Raycast(ray2, out hit2,100f,Ray_layer))
        {
           // print("地面碰撞的距离上方："+hit2.distance);
                Gernerate_effect();
                Skill_effectGenerate skill_EffectGenerate =  GetComponent<Skill_effectGenerate>();
                if(null!=skill_EffectGenerate)
                skill_EffectGenerate.Gernerate_effect();
        }else
        {
            print("地面碰撞进入音效播放");
             GetComponent<AudioSource>().clip=跳劈格挡音效;
             GetComponent<AudioSource>().Play();
        }
    }  


//伤害和特效是分开产生的，这个是伤害
    public void Gernerate_effect()
    {
        Enemy_Animator_Event enemy_Animator_Event =GetComponent<Enemy_Animator_Event>();
                         //  print("地面碰撞特效生成111");             
         /*if(null!=enemy_Animator_Event&&false!=enemy_Animator_Event.box_Detect.enable_boxDetect_NohitBack)
         {
            GameObject temp=Instantiate(Damage_prefab,Ger_Trans.position,Quaternion.identity);
            Destroy(temp,Destroy_time);
         }*/
         if(null!=enemy_Animator_Event&&false!=enemy_Animator_Event.physic_weaponDetect1.enable_boxDetect_NohitBack)
         {
            //print("地面碰撞特效生成222"); 
            GameObject temp=Instantiate(Damage_prefab,Ger_Trans.position,Quaternion.identity);
            Destroy(temp,Destroy_time);
         }
    }

    private void OnDrawGizmos()
    {
        if(Weapon_RayPoint)
        {
             Ray ray = new Ray(Weapon_RayPoint.transform.position, Vector3.down*10f);
        Gizmos.color = Color.red;
 
        Gizmos.DrawRay(ray);
        }
       
 
    }   


}
