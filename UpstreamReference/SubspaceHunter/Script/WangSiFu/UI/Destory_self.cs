/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 交互 UI 系统 / Interactive UI system
 * 功能 / Purpose: 处理菜单点击、提示框、血条、物品菜单、UI 定位和玩家 HUD。
 * English: Handles menu clicks, hint boxes, HP bars, item menus, UI positioning, and player HUD.
 */

using System.Collections;
using System.Collections.Generic;
using RootMotion.Dynamics;
using UnityEngine;

public class Destory_self : MonoBehaviour
{
    public GameObject parent_joint;
    public GameObject Destroy_effect;
    public float wait_2Destroy=10f;
    public bool is_notAuto_close;
    public bool is_distance_close;
    public float clsoe_Threshold = 2f;
     private Transform targetTransform;
    
    public GameObject close_andActivateGameObject;
    private bool is_closing;

    private bool is_holdOnHand;
    public bool is_dealHolster=true;
    //private Coroutine closeCoroutine = null;


    private void Start() {
        if(!is_notAuto_close)
         start_2waitDestroy();

        if(is_distance_close)
         targetTransform =  GameObject.FindWithTag("MainCamera").transform;
    }

    public void set_inHand()
    {
        is_holdOnHand= true;
    }

    public void set_NonHand()
    {
        is_holdOnHand= false;
    }

    public bool is_holding()
    {
        return is_holdOnHand;
    }

    public void start_2waitDestroy()
    {
         StartCoroutine("count2_Destroy");
    }

    public void stop_2waitDestroy()
    {
        //if(null!=closeCoroutine)
        StopCoroutine("count2_Destroy");
    }

     void FixedUpdate()
    {
        if(is_distance_close){
             float distance = Vector3.Distance(transform.position, targetTransform.position);
             if(distance >clsoe_Threshold) 
             {
                Deal_distanc_close();
             }
        }
    }

     private void Deal_distanc_close()
    {
        if(is_closing) return;
            is_closing=true;

            Destroy_selfGameObject();
           // Player_UI_Controller player_UI_Controller = GameObject.FindWithTag("Player").GetComponent<Player_UI_Controller>();
           // player_UI_Controller.关闭UI();
    }

    IEnumerator count2_Destroy()
    {

        yield return new WaitForSeconds(wait_2Destroy);
        Destroy_selfGameObject();
        //closeCoroutine =null;
    }

Item_holster item_Holster;
    private void Deal_cleanHolster()
    {
        Weapon_follow weapon_Follow = parent_joint.GetComponent<Weapon_follow>();
        if(weapon_Follow!= null)
        item_Holster = weapon_Follow.get_cur_holster();
        
        if(null!= item_Holster)
        item_Holster.Reset_holster();
    }

   public void Destroy_selfGameObject()
   {
       if(is_dealHolster)
        Deal_cleanHolster();
       if(null!=Destroy_effect)
       {
            GameObject temp_effect= Instantiate(Destroy_effect,transform.position,transform.rotation);
            Destroy(temp_effect,4f);
       }
       

       Destroy(parent_joint.gameObject);
   }

   public void Activate_AndDestroyGameObject()
   {
       if(null!=close_andActivateGameObject)
       {
          close_andActivateGameObject.SetActive(true);
       }
   }
}
