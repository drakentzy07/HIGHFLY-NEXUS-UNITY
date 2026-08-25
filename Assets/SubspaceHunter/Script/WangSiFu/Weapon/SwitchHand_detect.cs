/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 武器交互 / Weapon interaction
 * 功能 / Purpose: 处理左右手检测、武器切换、耐久度和命中特效入口。
 * English: Handles left/right hand detection, weapon switching, durability, and hit-effect entry points.
 */

using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityVector2;
using RootMotion.Dynamics;
using UnityEngine;

public class SwitchHand_detect : MonoBehaviour
{
    public GameObject Weapon;
   
    public Destory_self destory_Self;
    [Range(0,1)]
    public float switchTime=0.15f;
    [Range(0,1)]
    public float Detect_raduis=0.049f;
    public LayerMask triggerLayerMask;
    //[Range(0,1)]
   // /public float hand_distance=0.1f;
    private Collider Cur_hand;
    public Gun_Controller gun_Controller;
    private string origin_tag;
    //private  bool get_object_tag;
   // private string object_name;

     void OnEnable()
    {
       // init_weaponStatus();
       // Invoke("init_weaponStatus",switchTime);
       StartCoroutine("Set_wait2Destroy");
       origin_tag=Weapon.tag;
    }

    

    IEnumerator Set_wait2Destroy()
    {
       
        yield return new  WaitForSeconds(switchTime);
       // print("换手1");
       // DetectTriggers();
       // if( hand_distance<Vector3.Distance(transform.position,Cur_hand.transform.position))
        if(DetectTriggers()=="Weapon")
        {
            destory_Self.set_NonHand();

            //这是放在了收纳点，所以不用回收的意思
           if(!Weapon.GetComponent<Weapon_follow>().get_holsterStatus())  
            destory_Self.start_2waitDestroy();
          //   print("换手6");

           // print("换手7");
            if(null!=gun_Controller)
            {gun_Controller.停止装弹();}
          //  print("换手8");



            if(Weapon.GetComponent<Physic_weaponManager>()!=null&&Weapon.GetComponent<Physic_weaponManager>().have_skill)
            {
              
                if(origin_tag=="Weapon_inHand")
                 Weapon.GetComponent<Physic_weaponManager>().Get_Player().skill_LeftDisplay.Close_Display();
                else if(origin_tag=="Weapon_inRightHand")
                Weapon.GetComponent<Physic_weaponManager>().Get_Player().skill_RightDisplay.Close_Display();
            }
            
            
        }else
        {
             destory_Self.set_inHand();
             //   print("换手10");
             if(Weapon.GetComponent<Physic_weaponManager>()!=null&&Weapon.GetComponent<Physic_weaponManager>().have_skill)
            {
            
               if(origin_tag=="Weapon_inHand")
                 Weapon.GetComponent<Physic_weaponManager>().Get_Player().skill_LeftDisplay.Close_Display();
                else if(origin_tag=="Weapon_inRightHand")
                Weapon.GetComponent<Physic_weaponManager>().Get_Player().skill_RightDisplay.Close_Display();
              //  print("换手12");
                
                if(Weapon.gameObject.tag=="Weapon_inHand")
                Weapon.GetComponent<Physic_weaponManager>().Get_Player().skill_LeftDisplay.Open_Display();
                if(Weapon.gameObject.tag=="Weapon_inRightHand")
                Weapon.GetComponent<Physic_weaponManager>().Get_Player().skill_RightDisplay.Open_Display();
            }
        }
      //  print("换手13");
        transform.gameObject.SetActive(false);

    }


      string DetectTriggers()
    {
        // Use Physics.OverlapSphere to detect colliders within the specified radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, Detect_raduis, triggerLayerMask);
      //  print("换手2");
        // Iterate through all detected colliders
        foreach (Collider collider in hitColliders)
        {
            if (collider.isTrigger)
            {
               if(collider.transform.gameObject.tag=="Player_leftHand")
               {
                    if(Weapon.GetComponent<Weapon_follow>().get_holsterStatus())
                    Weapon.GetComponent<Weapon_follow>().stop_following();
                    Weapon.tag = "Weapon_inHand";
                   // Deal_kinematic();
                //   print("换手3");
                    return "Weapon_inHand";
               }
               else if(collider.transform.gameObject.tag=="Player_rightHand")
               {
                    if(Weapon.GetComponent<Weapon_follow>().get_holsterStatus())
                    Weapon.GetComponent<Weapon_follow>().stop_following();
                    Weapon.tag = "Weapon_inRightHand";
                 //   print("换手4");
                 //   Deal_kinematic();
                    return "Weapon_inRightHand";
               }
                //Debug.Log("Detected trigger: " + collider.name);
                // You can perform additional logic here, such as interacting with the detected triggers
            }
        }
       // print("换手5");
        Weapon.gameObject.tag="Weapon"; 
        return "Weapon";
    }

    private void Deal_kinematic()
    {
        Weapon.transform.SetParent(null);
        if(Weapon.GetComponent<Rigidbody>().isKinematic==true) 
        Weapon.GetComponent<Rigidbody>().isKinematic=false;
        //Weapon.GetComponent<Rigidbody>().useGravity=true;
        Invoke("set_kinematic",0.2f);
    }

    private void set_kinematic()
    {
        if(Weapon.GetComponent<Rigidbody>().isKinematic!=true) 
        Weapon.GetComponent<Rigidbody>().isKinematic=true;
    }

    // Optional: Draw the detection sphere in the editor for visualization
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Detect_raduis);
    }
  
   

   
}
