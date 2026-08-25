/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 技能与魔法系统 / Skill and magic system
 * 功能 / Purpose: 管理玩家技能、魔法弹体、范围攻击、命中爆炸、护盾和提示表现。
 * English: Manages player skills, magic projectiles, area attacks, hit explosions, shields, and hint presentation.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_magicController : MonoBehaviour
{
    public enum magic_type{
        火,
        电,
        盾牌,
        冰,
        陨石,
        治疗,
        未激活
    }
    public magic_type MT=magic_type.未激活;
   

    public GameObject Fire_ball;
    public GameObject electr_ball;
    public GameObject ice_ball;
    public GameObject stone_ball;
    public GameObject Magic_shield_point;//护盾父物体
    public GameObject Magic_shield_prefab;
    private GameObject Magic_shield;
    public Shoot_rigbody 火焰攻击控制;
    public Shoot_rigbody 冰锥攻击控制;

    public ele_round_realease 雷电攻击控制;
    public ele_round_realease 陨石攻击控制;
    public AudioSource 火球释放音效;
    public AudioSource 冰锥释放音效;
    private bool is_magic_ger;
    public ParticleSystem magic_release_standardEffect;

    public GameObject right_handMesh;

    public float[] magic_expent; // 火 雷电  盾牌 冰  陨石
    public float[] magic_prepareTime;
    public Player_managerV2 player_managerV2;

    public AudioClip 魔力为零提示;
    public GameObject Magic_barSet;

    public void Deal_Attack_OR_Pause()
    {
          Magic_barSet.SetActive(false);
                if(Magic_barSet.GetComponent<Magic_SkillBar_controller>().is_skillReady)
                {
                    Deal_magic_attack();
                    Deactivate_magic_ball();
                    Magic_barSet.GetComponent<Magic_SkillBar_controller>().Reset_bar_status();
                }else
                {
                    Deactivate_magic_ball();
                    Magic_barSet.GetComponent<Magic_SkillBar_controller>().Pause_bar();
                }
    }

    
    public void Deal_prepare()
    {
           if(!magic_ready()) return;
                 Magic_barSet.SetActive(true);
                Magic_barSet.GetComponent<Magic_SkillBar_controller>().prepare(Get_Cur_prepareTime());
    }

    public bool is_magic_weapon()
    {
        return Magic_shield==null?false:true;
    }

    public bool magic_ready()
    {
        return MT==magic_type.未激活?false:true;
    }

    public void Activate_Hand()
    {
        if(null!=right_handMesh)
        right_handMesh.SetActive(true);
    }

    public void DeActivate_hand()
    {
        if(null!=right_handMesh)
        right_handMesh.SetActive(false);
    }
   
   public void set_fire()
   {
        is_magic_ger=false;
        MT=magic_type.火;
   }

   public void set_ice()
   {
        is_magic_ger=false;
        MT=magic_type.冰;
   }

   public void set_stone()
   {
        is_magic_ger=false;
        MT=magic_type.陨石;
   }

   public void set_electr()
   {
        is_magic_ger=false;
        MT=magic_type.电;
   }

   public void set_shield()
   {
        is_magic_ger=false;
        MT=magic_type.盾牌;
   }

    GameObject res;
   public GameObject get_CurBall()
   {
     
         switch(MT)
        {
            case magic_type.未激活:
            break;
            case magic_type.火 :
            res=Fire_ball;
            break;
            case magic_type.电 :
             res=electr_ball;
            break;
            case magic_type.盾牌 :
             res=Magic_shield_point;
            break;
            case magic_type.冰 :
             res=ice_ball;
            break;
            case magic_type.陨石 :
             res=stone_ball;
            break;
        }
        return res;
   }

   public bool need_aim()
   {
        return MT==magic_type.盾牌?false:true;
   }

    public void activate_magic_ball()
    {

        is_magic_ger=true;
        switch(MT)
        {
            case magic_type.未激活:
            break;
            case magic_type.火 :
            Fire_ball.SetActive(true);
            break;
            case magic_type.电 :
            electr_ball.SetActive(true);
            break;
            case magic_type.盾牌 :
            
            break;
            case magic_type.冰 :
            ice_ball.SetActive(true);
            break;
            case magic_type.陨石 :
            stone_ball.SetActive(true);
            break;
        }
    }

    public void Deactivate_magic_ball()
    {
        print("进入关闭魔法");
         //is_magic_ger=false;
        switch(MT)
        {
            case magic_type.未激活:
            print("进入关闭魔法未激活");
            break;
            case magic_type.火 :
            print("进入关闭魔法火");
            Fire_ball.SetActive(false);
            break;
            case magic_type.电 :
            print("进入关闭魔法电");
            electr_ball.SetActive(false);
            break;
            case magic_type.盾牌 :
            print("进入关闭魔法盾牌");
            Destroy(Magic_shield);
            break;
            case magic_type.冰 :
            print("进入关闭魔法电");
            ice_ball.SetActive(false);
            break;
            case magic_type.陨石 :
            print("进入关闭魔法电");
            stone_ball.SetActive(false);
            break;
        }
          
    }


    float p_time;
    public float Get_Cur_prepareTime()
    {
        switch(MT)
        {
            case magic_type.未激活:
            break;
            case magic_type.火 :
            p_time=magic_prepareTime[0];
            break;
            case magic_type.电 :
            p_time=magic_prepareTime[1];
            break;
            case magic_type.盾牌 :
            p_time=magic_prepareTime[2];
            break;
            case magic_type.冰 :
            p_time=magic_prepareTime[3];
            break;
            case magic_type.陨石 :
            p_time=magic_prepareTime[4];
            break;
            
        }
        return p_time;
    }

    public void Deal_magic_attack()
    {
        if(is_magic_ger==false) return;
           switch(MT)
        {
            case magic_type.未激活:
            break;
            case magic_type.火 :
            执行火球攻击();
            break;
            case magic_type.电 :
            执行落雷攻击();
            break;
            case magic_type.冰 :
            执行冰锥攻击();
            break;
            case magic_type.陨石 :
            执行陨石攻击();
            break;
           /* case magic_type.盾牌 :
            执行盾牌();
            break;*/
            
        }
    }

    public void Deal_Skill_excute_immediatly()
    {
         switch(MT)
        {
            case magic_type.未激活:
            break;
            case magic_type.盾牌 :
            执行盾牌();
            break;
           /* case magic_type.盾牌 :
            执行盾牌();
            break;*/     
        }
    }

     


  


    private void 执行火球攻击()
    {
        if(!player_managerV2.Minus_ManaOnce(magic_expent[0])) 
        {
            GetComponent<AudioSource>().clip = 魔力为零提示;
            GetComponent<AudioSource>().Play();
            return;}
            magic_release_standardEffect.Play();
            火焰攻击控制.Shoot_fire();
            火球释放音效.Play();
            //player_managerV2.Minus_ManaOnce(magic_expent[0]);
            is_magic_ger=false;
    }

    private void 执行落雷攻击()
    {
            if(!player_managerV2.Minus_ManaOnce(magic_expent[1])) 
            {
                GetComponent<AudioSource>().clip = 魔力为零提示;
                GetComponent<AudioSource>().Play();
                return;}
           // magic_release_standardEffect.Play();
            雷电攻击控制.Deal_range_attack();
           // player_managerV2.Minus_ManaOnce(magic_expent[1]);
            is_magic_ger=false;
    }

    private void 执行盾牌()
    {
        print("进入执行盾牌");
        if(!player_managerV2.Minus_ManaOnce(magic_expent[2])) 
        {
            print("进入执行盾牌1");
            GetComponent<AudioSource>().clip = 魔力为零提示;
            GetComponent<AudioSource>().Play();
            return;}
            print("进入执行盾牌2");
             Magic_shield = Instantiate(Magic_shield_prefab,Magic_shield_point.transform.position,Magic_shield_point.transform.rotation);
            Magic_shield.transform.SetParent(Magic_shield_point.transform);
            //magic_release_standardEffect.Play();
            //火焰攻击控制.Shoot_fire();
            //火球释放音效.Play();
            //player_managerV2.Minus_ManaOnce(magic_expent[0]);
            is_magic_ger=false;
    }

       private void 执行冰锥攻击()
    {
        if(!player_managerV2.Minus_ManaOnce(magic_expent[3])) 
        {
            GetComponent<AudioSource>().clip = 魔力为零提示;
            GetComponent<AudioSource>().Play();
            return;}
            magic_release_standardEffect.Play();
            冰锥攻击控制.Shoot_fire();
            冰锥释放音效.Play();
            //player_managerV2.Minus_ManaOnce(magic_expent[0]);
            is_magic_ger=false;
    }

     private void 执行陨石攻击()
    {
            if(!player_managerV2.Minus_ManaOnce(magic_expent[4])) 
            {
                GetComponent<AudioSource>().clip = 魔力为零提示;
                GetComponent<AudioSource>().Play();
                return;}
            //magic_release_standardEffect.Play();
            陨石攻击控制.Deal_range_attack();
           // player_managerV2.Minus_ManaOnce(magic_expent[1]);
            is_magic_ger=false;
    }




}
