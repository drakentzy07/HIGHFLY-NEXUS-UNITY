/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 武器与道具收纳 / Weapon and item holster
 * 功能 / Purpose: 管理武器跟随、插槽检测和道具收纳交互。
 * English: Manages weapon following, slot detection, and item holster interactions.
 */

using System.Collections;
using System.Collections.Generic;
using RootMotion.Dynamics;
using UnityEngine;

public class cycle_hlosterDetect : MonoBehaviour
{
    
    public GameObject Weapon;
    public Destory_self destory_Self;
    [Range(0,1)]
    public float switchTime=0.15f;
    [Range(0,1)]
    public float Detect_raduis=0.049f;
    public LayerMask triggerLayerMask;
    private string origin_tag;
    [Range(0,5)]
    public float cycle_detectTime=0.5f;
    //private  bool get_object_tag;
   // private string object_name;
   public bool is_cycling;
   
     void OnEnable()
    {
       // init_weaponStatus();
       // Invoke("init_weaponStatus",switchTime);
      // StartCoroutine("Set_wait2Destroy");
       origin_tag=Weapon.tag;
       is_cycling=true;
       Invoke("cycle_detect",cycle_detectTime);
    }

    private void cycle_detect()
    {
        print("循环检测1");
        if(!is_cycling) {transform.gameObject.SetActive(false); return;}
        print("循环检测2");
        //这里是换手已经被激活
        if(Weapon.GetComponent<Weapon_follow>().换手检测.activeSelf==true) 
        {
            print("循环检测3");
            is_cycling=false; 
            transform.gameObject.SetActive(false);
            return;
        }
        
        //仅作检测工作
       print("循环检测4");
        //这里是换手检测已经结束
        if((Weapon.tag=="Weapon_inRightHand"&&OVRInput.Get(OVRInput.Button.PrimaryHandTrigger,OVRInput.Controller.RTouch))
        ||(Weapon.tag=="Weapon_inHand"&&OVRInput.Get(OVRInput.Button.PrimaryHandTrigger,OVRInput.Controller.LTouch)))
        { is_cycling=false; transform.gameObject.SetActive(false); return; } 

        print("循环检测5");
         //替代换手执行收尾工作
         Set_wait2Destroy();
         
        print("循环检测6");
         Invoke("cycle_detect",cycle_detectTime);
    }

    

    void Set_wait2Destroy()
    {
       
        if(DetectTriggers()!="Weapon")
        {
             if(Weapon.GetComponent<Physic_weaponManager>()!=null&&Weapon.GetComponent<Physic_weaponManager>().have_skill)
            {
               
               if(origin_tag=="Weapon_inHand")
                 Weapon.GetComponent<Physic_weaponManager>().Get_Player().skill_LeftDisplay.Close_Display();
                else if(origin_tag=="Weapon_inRightHand")
                Weapon.GetComponent<Physic_weaponManager>().Get_Player().skill_RightDisplay.Close_Display();

                
                if(Weapon.gameObject.tag=="Weapon_inHand")
                Weapon.GetComponent<Physic_weaponManager>().Get_Player().skill_LeftDisplay.Open_Display();
                if(Weapon.gameObject.tag=="Weapon_inRightHand")
                Weapon.GetComponent<Physic_weaponManager>().Get_Player().skill_RightDisplay.Open_Display();
            }
            //这里换成了换手结束.
            is_cycling=false;
            transform.gameObject.SetActive(false);
        }else
        {
            //没换成换手一直等待
        }
           

    }


    //与换手不同，这里仅获取tag结果
      string DetectTriggers()
    {
        // Use Physics.OverlapSphere to detect colliders within the specified radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, Detect_raduis, triggerLayerMask);

        // Iterate through all detected colliders
        foreach (Collider collider in hitColliders)
        {
            if (collider.isTrigger)
            {
               if(collider.transform.gameObject.tag=="Player_leftHand"&&OVRInput.Get(OVRInput.Button.PrimaryHandTrigger,OVRInput.Controller.LTouch))
               {
                    //if(Weapon.GetComponent<Weapon_follow>().get_holsterStatus())
                    Weapon.GetComponent<Weapon_follow>().stop_following();
                    Weapon.tag = "Weapon_inHand";
                   // Deal_kinematic();
                    return "Weapon_inHand";
               }
               else if(collider.transform.gameObject.tag=="Player_rightHand"&&OVRInput.Get(OVRInput.Button.PrimaryHandTrigger,OVRInput.Controller.RTouch))
               {
                  //  if(Weapon.GetComponent<Weapon_follow>().get_holsterStatus())
                    Weapon.GetComponent<Weapon_follow>().stop_following();
                    Weapon.tag = "Weapon_inRightHand";
                 //   Deal_kinematic();
                    return "Weapon_inRightHand";
               }
                //Debug.Log("Detected trigger: " + collider.name);
                // You can perform additional logic here, such as interacting with the detected triggers
            }
        }
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
