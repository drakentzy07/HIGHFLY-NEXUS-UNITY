/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 敌人系统 / Enemy system
 * 功能 / Purpose: 维护敌人生成、生命值、受击反馈、死亡结算、技能特效和战斗状态。
 * English: Maintains enemy spawning, HP, hit feedback, death settlement, skill effects, and combat state.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Animator_Event : MonoBehaviour
{
   public Box_detect box_Detect;
   public Box_detect box_Detect2;
   public Physic_weaponDetect physic_weaponDetect1;
   public Physic_weaponDetect physic_weaponDetect2;
   public GameObject 武器拖尾1;
   public GameObject 武器拖尾2;
   public GameObject 武器拖尾3;
   public GameObject 武器拖尾4;

   public GameObject Zombie_attack_right;
    public GameObject Zombie_attack_left;

    public GameObject effect_left;
    public GameObject effect_right;

   
   public void Zombie_right()
   {
    GetComponent<CommandExecutor>().Enemy_lookAtAdjust();
      Zombie_attack_right.SetActive(true);
   }

  

   public void Zombie_left()
   {
    GetComponent<CommandExecutor>().Enemy_lookAtAdjust();
      Zombie_attack_left.SetActive(true);
   }

   public void Close_zombie_right()
   {
      Zombie_attack_right.SetActive(false);
   }


   public void Close_zombie_left()
   {
      Zombie_attack_left.SetActive(false);
   }

   
   public void open_box()
   {
      GetComponent<CommandExecutor>().DisLocate_player();
        if(null!=box_Detect)
        {
            box_Detect.enable_boxDetect=true;
            box_Detect.enable_Minus_hp=true;
        }
       

        if(null!=box_Detect2)
        {
             box_Detect2.enable_boxDetect=true;
             box_Detect2.enable_Minus_hp=true;
        }

        if(null!=physic_weaponDetect1)
        {
            physic_weaponDetect1.enable_boxDetect=true;
            physic_weaponDetect1.enable_Minus_hp=true;
        }
       

        if(null!=physic_weaponDetect2)
        {
             physic_weaponDetect2.enable_boxDetect=true;
             physic_weaponDetect2.enable_Minus_hp=true;
        }
      

       if(null!=武器拖尾1)
       武器拖尾1.SetActive(true);
       if(null!=武器拖尾2)
       武器拖尾2.SetActive(true);

       if(null!=武器拖尾3)
       武器拖尾3.SetActive(true);
       if(null!=武器拖尾4)
       武器拖尾4.SetActive(true);

      if(effect_left!=null)
      effect_left.SetActive(true);
      if(effect_right!=null)
      effect_right.SetActive(true);
      

   }

   public void close_box()
   {
        if(null!=box_Detect)
        {
            box_Detect.enable_boxDetect=false;
           box_Detect.enable_Minus_hp=false;
        }
       

       if(null!=box_Detect2)
       {
          box_Detect2.enable_boxDetect=false;
          box_Detect2.enable_Minus_hp=false;
       }

        if(null!=physic_weaponDetect1)
        {
            physic_weaponDetect1.enable_boxDetect=false;
            physic_weaponDetect1.enable_Minus_hp=false;
        }
       

        if(null!=physic_weaponDetect2)
        {
             physic_weaponDetect2.enable_boxDetect=false;
             physic_weaponDetect2.enable_Minus_hp=false;
        }
       
       if(effect_right!=null)
      effect_right.SetActive(false);
      if(effect_left!=null)
      effect_left.SetActive(false);

       if(null!=武器拖尾1)
       武器拖尾1.SetActive(false);
       if(null!=武器拖尾2)
       武器拖尾2.SetActive(false);

       if(null!=武器拖尾3)
       武器拖尾3.SetActive(false);
       if(null!=武器拖尾4)
       武器拖尾4.SetActive(false);
   }

  public void open_box_noHitBack()
  {
    GetComponent<CommandExecutor>().DisLocate_player();
    
    if(null!=box_Detect)
    {
        box_Detect.enable_boxDetect=true;
        box_Detect.enable_boxDetect_NohitBack=true;
        box_Detect.enable_Minus_hp=true;
    }
   

    if(null!=box_Detect2)
    {
        box_Detect2.enable_boxDetect=true;
        box_Detect2.enable_boxDetect_NohitBack=true;
        box_Detect2.enable_Minus_hp=true;
    }

     if(null!=physic_weaponDetect1)
    {
        physic_weaponDetect1.enable_boxDetect=true;
        physic_weaponDetect1.enable_boxDetect_NohitBack=true;
        physic_weaponDetect1.enable_Minus_hp=true;
    }
   

    if(null!=physic_weaponDetect2)
    {
        physic_weaponDetect2.enable_boxDetect=true;
        physic_weaponDetect2.enable_boxDetect_NohitBack=true;
        physic_weaponDetect2.enable_Minus_hp=true;
    }
    

     if(null!=武器拖尾1)
       武器拖尾1.SetActive(true);
       if(null!=武器拖尾2)
       武器拖尾2.SetActive(true);

       if(null!=武器拖尾3)
       武器拖尾3.SetActive(true);
       if(null!=武器拖尾4)
       武器拖尾4.SetActive(true);

        if(effect_left!=null)
      effect_left.SetActive(true);
      if(effect_right!=null)
      effect_right.SetActive(true);
  }

  public void close_box_noHitBack()
  {
    if(null!=box_Detect)
    {
        box_Detect.enable_boxDetect=false;
        box_Detect.enable_boxDetect_NohitBack=false;
        box_Detect.enable_Minus_hp=false;
    }
    
    

    if(null!=box_Detect2)
    {
      box_Detect2.enable_boxDetect=false;
      box_Detect2.enable_boxDetect_NohitBack=false;
      box_Detect2.enable_Minus_hp=false;
    }

    if(null!=physic_weaponDetect1)
    {
        physic_weaponDetect1.enable_boxDetect=false;
        physic_weaponDetect1.enable_boxDetect_NohitBack=false;
        physic_weaponDetect1.enable_Minus_hp=false;
    }
    
    

    if(null!=physic_weaponDetect2)
    {
      physic_weaponDetect2.enable_boxDetect=false;
      physic_weaponDetect2.enable_boxDetect_NohitBack=false;
      physic_weaponDetect2.enable_Minus_hp=false;
    }
    

     if(null!=武器拖尾1)
       武器拖尾1.SetActive(false);
       if(null!=武器拖尾2)
       武器拖尾2.SetActive(false);

        if(null!=武器拖尾3)
       武器拖尾3.SetActive(false);
       if(null!=武器拖尾4)
       武器拖尾4.SetActive(false);

       if(effect_right!=null)
      effect_right.SetActive(false);
      if(effect_left!=null)
      effect_left.SetActive(false);
  }


    public void open_box_left() //左手是1
   {
    GetComponent<CommandExecutor>().DisLocate_player();
        if(null!=box_Detect)
        {
            box_Detect.enable_boxDetect=true;
            box_Detect.enable_Minus_hp=true;
        }

        if(null!=physic_weaponDetect1)
        {
            physic_weaponDetect1.enable_boxDetect=true;
            physic_weaponDetect1.enable_Minus_hp=true;
        }
       

        if(effect_left!=null)
      effect_left.SetActive(true);
      

       if(null!=武器拖尾1)
       武器拖尾1.SetActive(true);
       if(null!=武器拖尾2)
       武器拖尾2.SetActive(true);

       

   }


   public void open_box_right() //右手是2
   {
      GetComponent<CommandExecutor>().DisLocate_player();

        if(null!=box_Detect2)
        {
             box_Detect2.enable_boxDetect=true;
             box_Detect2.enable_Minus_hp=true;
        }

         if(null!=physic_weaponDetect2)
        {
             physic_weaponDetect2.enable_boxDetect=true;
             physic_weaponDetect2.enable_Minus_hp=true;
        }
      
      if(effect_right!=null)
      effect_right.SetActive(true);

       if(null!=武器拖尾3)
       武器拖尾3.SetActive(true);
       if(null!=武器拖尾4)
       武器拖尾4.SetActive(true);

   }

   public void open_box_noHitBack_left()//左手是1
  {
    GetComponent<CommandExecutor>().DisLocate_player();
    if(null!=box_Detect)
    {
         box_Detect.enable_boxDetect=true;
        box_Detect.enable_boxDetect_NohitBack=true;
        box_Detect.enable_Minus_hp=true;
    }

    if(null!=physic_weaponDetect1)
    {
         physic_weaponDetect1.enable_boxDetect=true;
        physic_weaponDetect1.enable_boxDetect_NohitBack=true;
        physic_weaponDetect1.enable_Minus_hp=true;
    }
   
    if(effect_left!=null)
      effect_left.SetActive(true);
    
    

     if(null!=武器拖尾1)
       武器拖尾1.SetActive(true);
       if(null!=武器拖尾2)
       武器拖尾2.SetActive(true);

      
  }

   public void open_box_noHitBack_right()//右手是2
  {
    
   GetComponent<CommandExecutor>().DisLocate_player();

    if(null!=box_Detect2)
    {
        box_Detect2.enable_boxDetect=true;
        box_Detect2.enable_boxDetect_NohitBack=true;
        box_Detect2.enable_Minus_hp=true;
    }

    if(null!=physic_weaponDetect2)
    {
        physic_weaponDetect2.enable_boxDetect=true;
        physic_weaponDetect2.enable_boxDetect_NohitBack=true;
        physic_weaponDetect2.enable_Minus_hp=true;
    }

       if(effect_right!=null)
      effect_right.SetActive(true);


    
       if(null!=武器拖尾3)
       武器拖尾3.SetActive(true);
       if(null!=武器拖尾4)
       武器拖尾4.SetActive(true);
  }




}
