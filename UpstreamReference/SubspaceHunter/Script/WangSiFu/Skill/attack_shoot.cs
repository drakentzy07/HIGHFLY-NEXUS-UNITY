/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 技能与魔法系统 / Skill and magic system
 * 功能 / Purpose: 管理玩家技能、魔法弹体、范围攻击、命中爆炸、护盾和提示表现。
 * English: Manages player skills, magic projectiles, area attacks, hit explosions, shields, and hint presentation.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Knife.Effects;
using BehaviorDesigner.Runtime.Tasks;

public class attack_shoot : MonoBehaviour
{
    public Transform Fly_trasform;
    public Transform high_transform; //控制剑气生成的方向以及转向
    public Transform middle_transform; //控制剑气生成的方向以及转向
    public Transform low_transform; //控制剑气生成的方向以及转向
    public Transform Foot_trasnform;//控制剑气生成方向，替代原先90°的生成方向

    public Transform Angle45_transform;
    public Transform Angle90_transform;
    public Transform Angle135_transform;

    public GameObject Flying_little;  //剑气本体小
    public GameObject Flying_big;  //剑气本体大
    public GameObject Flying_90; //与地面会产生交互的纵向剑气
    public GameObject dagger_Hor;
    public GameObject dagger_ver;
    public GameObject dagger_45;
    public GameObject dagger_fly;
    public AudioSource Skill_release_woosh;
    private int dager_round;
    public ParticleGroupPlayer[] particleGroupPlayers;
    public ParticleGroupEmitter[] ParticleGroupEmitters;
    public ParticleSystem[] particleSystems;

    public GameObject left_pistol;
    public GameObject right_pistol;
    
    // Start is called before the first frame update
    
    public void Shoot_FlyDagger()
    {
        
        GetComponent<CommandExecutor>().DisLocate_player();
        switch(dager_round)
        {
            case 0:
            Gernerate_OnHigh_withObj(dagger_Hor);
            break;
            case 1:
            Gernerate_OnHigh_withObj(dagger_45);
            break;
            case 2:
            Gernerate_OnHigh_withObj(dagger_ver);
            break;
        }
        if(dager_round==2)
         dager_round=0;
         else
         dager_round++;
           
    }

    public void Gernerate_Random()
    {
        GetComponent<CommandExecutor>().DisLocate_player();
        if(null!=Skill_release_woosh)
        Skill_release_woosh.Play();
        else 
        {
            woosh_fater woosh_Fater=GetComponent<woosh_fater>();
            if(null!=woosh_Fater)
            {
                woosh_Fater.play_WeaponWoosh();
            }
        }
        switch (Random.Range(1,4))
        {
            case 1:
            Gernerate_OnMiddle("little");
            break;
            case 2:
            Gernerate_On45();
            break;
            case 3:
          //  Gernerate_On90();
            Gernerate_On135();
            break;
          /*  case 4:
            Gernerate_On135();
            break;*/
        }
    }

     public void Gernerate_Random_zeroAngle()
    {
        GetComponent<CommandExecutor>().DisLocate_player();
        if(null!=Skill_release_woosh)
         Skill_release_woosh.Play();
        switch (Random.Range(1,3))
        {
            case 1:
            Gernerate_OnMiddle("big");
            break;
            case 2:
            Gernerate_OnHigh("big");
            break;
            case 3:
            Gernerate_OnLow("big");
            break;
          
        }
    }


    private void Activate_shoot_effect()
    {
       /* foreach(var emit in ParticleGroupEmitters)
        {
            if(null!=emit)
            {
                emit.Emit(1);
            }
        }*/
        foreach(var parti in particleSystems)
        {
            if(null!=parti)
            parti.Play();
        }
        foreach(var play in particleGroupPlayers)
        {
            if(null!=play)
            play.Play();
        }
    }

    
    public void 子弹点射()
    {
        
     GetComponent<CommandExecutor>().DisLocate_player();
        GameObject 子弹 = Instantiate(Flying_little,high_transform.position,high_transform.rotation);
        if(null!=子弹.GetComponent<Fly_damageUnit>())
        {子弹.GetComponent<Rigidbody>().AddForce(high_transform.forward*子弹.GetComponent<Fly_damageUnit>().子弹飞行速度,ForceMode.Impulse);
        Destroy(子弹,子弹.GetComponent<Fly_damageUnit>().子弹存在时间);}
        else if(null!=子弹.GetComponent<Bullet_status2>())
        {
            
            Destroy(子弹,子弹.GetComponent<Bullet_status2>().子弹存在时间);
        }
        Activate_shoot_effect();
    }

    public void 双枪左射()
    {
        GetComponent<CommandExecutor>().DisLocate_player();
        GameObject 子弹 = Instantiate(left_pistol.GetComponent<enemy_gunController>().bullet,high_transform.position,high_transform.rotation);
        if(null!=子弹.GetComponent<Fly_damageUnit>())
        {子弹.GetComponent<Rigidbody>().AddForce(high_transform.forward*子弹.GetComponent<Fly_damageUnit>().子弹飞行速度,ForceMode.Impulse);
        Destroy(子弹,子弹.GetComponent<Fly_damageUnit>().子弹存在时间);}
        else if(null!=子弹.GetComponent<Bullet_status2>())
        {
            
            Destroy(子弹,子弹.GetComponent<Bullet_status2>().子弹存在时间);
        }
        left_pistol.GetComponent<enemy_gunController>().fire_effect.Play();
    }


    public void 双枪右射()
    {
         GetComponent<CommandExecutor>().DisLocate_player();
            GameObject 子弹 = Instantiate(right_pistol.GetComponent<enemy_gunController>().bullet,high_transform.position,high_transform.rotation);
            if(null!=子弹.GetComponent<Fly_damageUnit>())
            {子弹.GetComponent<Rigidbody>().AddForce(high_transform.forward*子弹.GetComponent<Fly_damageUnit>().子弹飞行速度,ForceMode.Impulse);
            Destroy(子弹,子弹.GetComponent<Fly_damageUnit>().子弹存在时间);}
            else if(null!=子弹.GetComponent<Bullet_status2>())
            {
                
                Destroy(子弹,子弹.GetComponent<Bullet_status2>().子弹存在时间);
            }
        right_pistol.GetComponent<enemy_gunController>().fire_effect.Play();
    }


    public void 飞射出()
    {

            GetComponent<CommandExecutor>().DisLocate_player();
             Instantiate(dagger_fly,Fly_trasform.position,Fly_trasform.rotation);
    }

    public void 双枪右飞射()
    {
        GetComponent<CommandExecutor>().DisLocate_player();
            GameObject 子弹 = Instantiate(right_pistol.GetComponent<enemy_gunController>().bullet,Fly_trasform.position,Fly_trasform.rotation);
            if(null!=子弹.GetComponent<Fly_damageUnit>())
            {子弹.GetComponent<Rigidbody>().AddForce(Fly_trasform.forward*子弹.GetComponent<Fly_damageUnit>().子弹飞行速度,ForceMode.Impulse);
            Destroy(子弹,子弹.GetComponent<Fly_damageUnit>().子弹存在时间);}
            else if(null!=子弹.GetComponent<Bullet_status2>())
            {
                
                Destroy(子弹,子弹.GetComponent<Bullet_status2>().子弹存在时间);
            }
        right_pistol.GetComponent<enemy_gunController>().fire_effect.Play();
    }

     public void 双枪左飞射()
    {
       GetComponent<CommandExecutor>().DisLocate_player();
        GameObject 子弹 = Instantiate(left_pistol.GetComponent<enemy_gunController>().bullet,Fly_trasform.position,high_transform.rotation);
        if(null!=子弹.GetComponent<Fly_damageUnit>())
        {子弹.GetComponent<Rigidbody>().AddForce(Fly_trasform.forward*子弹.GetComponent<Fly_damageUnit>().子弹飞行速度,ForceMode.Impulse);
        Destroy(子弹,子弹.GetComponent<Fly_damageUnit>().子弹存在时间);}
        else if(null!=子弹.GetComponent<Bullet_status2>())
        {
            
            Destroy(子弹,子弹.GetComponent<Bullet_status2>().子弹存在时间);
        }
        left_pistol.GetComponent<enemy_gunController>().fire_effect.Play();
    }


    public void 子弹下蹲点射()
    {
         GetComponent<CommandExecutor>().DisLocate_player();
            GameObject 子弹 = Instantiate(Flying_little,middle_transform.position,middle_transform.rotation);
          if(null!=子弹.GetComponent<Fly_damageUnit>()){
           子弹.GetComponent<Rigidbody>().AddForce(middle_transform.forward*子弹.GetComponent<Fly_damageUnit>().子弹飞行速度,ForceMode.Impulse);
           Destroy(子弹,子弹.GetComponent<Fly_damageUnit>().子弹存在时间);}
           else if(null!=子弹.GetComponent<Bullet_status2>())
        {
            
            Destroy(子弹,子弹.GetComponent<Bullet_status2>().子弹存在时间);
        }
            Activate_shoot_effect();
        
    }


    public void Gernerate_On45()
    {
        GetComponent<CommandExecutor>().DisLocate_player();
        Instantiate(Flying_little,Angle45_transform.position,Angle45_transform.rotation);
    }

    public void Gernerate_On90()
    {
        GetComponent<CommandExecutor>().DisLocate_player();
        if(null!=Flying_90)
        Instantiate(Flying_90,Foot_trasnform.position,Foot_trasnform.rotation);
    
    }

    public void Gernerate_On135()
    {
        GetComponent<CommandExecutor>().DisLocate_player();
        Instantiate(Flying_little,Angle135_transform.position,Angle135_transform.rotation);
    }

    public void Gernerate_OnHigh_withObj(GameObject shoot)
    {
            GetComponent<CommandExecutor>().DisLocate_player();
             Instantiate(shoot,high_transform.position,high_transform.rotation);
       
    }

    public void Gernerate_OnHigh(string scale)
    {
        GetComponent<CommandExecutor>().DisLocate_player();
        switch (scale)
        {
            case "little":
             Instantiate(Flying_little,high_transform.position,high_transform.rotation);
            break;
            case "big":
           Instantiate(Flying_big,high_transform.position,high_transform.rotation);
            break;
        }
       
    }

    public void Gernerate_OnMiddle(string scale)
    {
        GetComponent<CommandExecutor>().DisLocate_player();
        switch (scale)
        {
            case "little":
            Instantiate(Flying_little,middle_transform.position,middle_transform.rotation);
            break;
            case "big":
             Instantiate(Flying_big,middle_transform.position,high_transform.rotation);
            break;
        }
       
    }

    public void Gernerate_OnLow(string scale)
    {
        GetComponent<CommandExecutor>().DisLocate_player();
        switch (scale)
        {
            case "little":
            Instantiate(Flying_little,low_transform.position,low_transform.rotation);
            break;
            case "big":
             Instantiate(Flying_big,low_transform.position,high_transform.rotation);
            break;
        }
        
    }





}
