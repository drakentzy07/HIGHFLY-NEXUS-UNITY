/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 武器与道具收纳 / Weapon and item holster
 * 功能 / Purpose: 管理武器跟随、插槽检测和道具收纳交互。
 * English: Manages weapon following, slot detection, and item holster interactions.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using RootMotion.Dynamics;
//using Unity.Mathematics;
using UnityEngine;

public class Item_holster : MonoBehaviour
{
    
    public Transform 剑收纳点;
    public Transform 手枪收纳点;
    public Transform 盾牌收纳点;
    public Transform 步枪收纳点;
    public GameObject 提示球;

    Weapon_follow weapon_Follow;
    public bool have_weapon;

    public GameObject[] weapon_model;

    

   
    
    
     public enum 物品类型
    {
        剑,
        手枪,
        步枪,
        盾牌
    }

    public void Reset_holster()
    {
        have_weapon=false;
        Reset_model();
        提示球.SetActive(false);
        weapon_Follow = null;
    }

//暂时使用的收纳逻辑，之后要去除
Weapon_meshManager weapon_MeshManager;
    public void Reset_model()
    {
        if(weapon_Follow!=null)
        weapon_MeshManager = weapon_Follow.transform.GetComponent<Weapon_meshManager>();

        if(null!=weapon_MeshManager)
        weapon_MeshManager.Show_weapon();
        
        foreach (var item in weapon_model)
        {
            item.SetActive(false);
        }
        
    }

//暂时使用的收纳逻辑，之后要去除
    public void set_model()
    {
        print("进入模型1");
        Weapon_meshManager weapon_MeshManager = weapon_Follow.transform.GetComponent<Weapon_meshManager>();
        weapon_MeshManager.hide_weapon();
        print("进入模型2");
         switch(weapon_Follow.transform.gameObject.name)
        {
            case "基础铁剑(Clone)":
             weapon_model[0].SetActive(true);
            break;
            case "吞噬者长剑(Clone)":
            weapon_model[1].SetActive(true);
            break;
            case "哥布林骑士剑(Clone)":
            weapon_model[2].SetActive(true);
            break;
            case "夜雨(Clone)":
            weapon_model[3].SetActive(true);
            break;
            case "战术手枪V2(Clone)":
            weapon_model[4].SetActive(true);
            break;
            case "狼牙匕首(Clone)":
            weapon_model[5].SetActive(true);
            break;
            case "王座长剑(Clone)":
            weapon_model[6].SetActive(true);
            break;
            case "基础铁盾(Clone)":
            weapon_model[7].SetActive(true);
            break;
            case "魔法盾(Clone)":
            weapon_model[8].SetActive(true);
            break;
            case "突击步枪(Clone)":
            weapon_model[9].SetActive(true);
            break;
            case "马格南(Clone)":
            weapon_model[10].SetActive(true);
            break;
        }
    }


    /// <summary>
    /// OnTriggerEnter is called when the Collider other enters the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    void OnTriggerEnter(Collider other)
    {
            
            if(!have_weapon)
            {
                 if(other.tag=="Weapon"){
                         Destory_self des = other.GetComponent<Destory_self>();
                         if(des!=null&&des.is_holding()) 
                        {
                              提示球.SetActive(true);
                        }

                 }
            }else
            {
               
                
                if(other.tag=="Player_rightHand"||other.tag=="Player_leftHand")
                {
                        if(提示球.activeSelf==false) 提示球.SetActive(true);
                }   
            }
            
    }

    /// <summary>
    /// OnTriggerExit is called when the Collider other has stopped touching the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    void OnTriggerExit(Collider other)
    {

         if(!have_weapon)
        {
             
                       if(other.tag=="Weapon"){
                         Destory_self des = other.GetComponent<Destory_self>();
                         if(des!=null&&des.is_holding()) 
                        {
                              提示球.SetActive(false);
                        }

                 }
                        
                
        }else
        {
            if(other.tag=="Player_rightHand"||other.tag=="Player_leftHand")
            {
                    if(提示球.activeSelf==true) 提示球.SetActive(false);
            }   
            
        }
    }

    

    
    void OnTriggerStay(Collider other)
    {
        if(have_weapon) return;
        if(other.tag=="Weapon")
        {
          //  print("检测武器进入1");
            //这里用来判断这个weapon是不是父物体，因为有多个weapon
            Destory_self des = other.GetComponent<Destory_self>();
            if(des==null) return;
            if(des.is_holding()) return;
            have_weapon=true;
            print("检测武器进入2");
           // rigidbody.isKinematic=true;
           GameObject parent_joint =des.parent_joint;
            string item_type  = parent_joint.transform.GetComponent<Physic_weaponManager>().get_itemType();
             weapon_Follow = parent_joint.transform.GetComponent<Weapon_follow>();
            set_model();
              print("检测武器进入3");
            switch (item_type)
            {
                case "剑":
               // other.transform.SetParent(剑收纳点);
                weapon_Follow.set_holster(剑收纳点,transform);
               
                break;
                case "手枪":
                 weapon_Follow.set_holster(手枪收纳点,transform);
                   print("检测武器进入4");
              //  other.transform.SetParent(手枪收纳点);
                break;
                case "步枪":
                 weapon_Follow.set_holster(步枪收纳点,transform);
               // other.transform.SetParent(步枪收纳点);
                break;
                case "盾牌":
                 weapon_Follow.set_holster(盾牌收纳点,transform);
              //  other.transform.SetParent(盾牌收纳点);
                break;
            }
         //   other.transform.localPosition= new Vector3(0,0,0);
          //  other.transform.localRotation = quaternion.Euler(0,0,0);
            

        }
    }


}
