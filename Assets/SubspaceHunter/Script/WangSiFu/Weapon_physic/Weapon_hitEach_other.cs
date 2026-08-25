/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 物理武器系统 / Physical weapon system
 * 功能 / Purpose: 管理抓取、释放、刚体添加、盾牌阻挡、武器碰撞和轨迹检测。
 * English: Manages grabbing, releasing, Rigidbody setup, shield blocking, weapon collision, and trace detection.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_hitEach_other : MonoBehaviour
{
    // Start is called before the first frame update
     public ParticleSystem blood;
     public GameObject hit_effect_prefab;
     public GameObject heavy_hit_prefab;
    public AudioSource Get_Hit;
    public AudioSource Get_HitBack;
    public float Collider_speed=1f;
    bool is_interVal;
    public bool is_rigbodyDetect;
    float Cur_weapon_speed;
    public woosh_controller woosh_Controller;
 


    

    IEnumerator interval_hit(Collision other,bool is_hitBack)
    {
        is_interVal=true;
        print("进入间隔2");
         ContactPoint[] test_pointArrary= other.contacts;
                 Vector3 closestPoint =test_pointArrary[0].point;
                /*  blood.transform.position =closestPoint;
                            blood.transform.rotation = Quaternion.LookRotation(-test_pointArrary[0].normal);
                            blood.Emit(5);*/
                    
                   if(!is_hitBack)
                   {
                        if(woosh_Controller!=null) woosh_Controller.Play_oneShot();
                         else  Get_Hit.Play();
                   }  
                    else   Get_HitBack.Play();
        
         GameObject hit_effect=Instantiate(hit_effect_prefab,closestPoint,Quaternion.identity);
        hit_effect.GetComponent<ParticleSystem>().Emit(5);
        Destroy(hit_effect,1.5f);
        yield return new WaitForSeconds(1f);
        is_interVal=false;
    }

    IEnumerator interval_hitTrigger(Collider other,bool is_hitBack)
    {
         is_interVal=true;
         Vector3 postion_hit=other.ClosestPoint(transform.position);                                                 
                     //    blood.transform.position =postion_hit;
                      //   blood.transform.rotation = Quaternion.LookRotation(-test_pointArrary[0].normal);
                      //   blood.Emit(5);            
                      if(!is_hitBack)
                   {
                        if(woosh_Controller!=null) woosh_Controller.Play_oneShot();
                         else  Get_Hit.Play();
                   }  
                    else   Get_HitBack.Play();
        GameObject hit_effect=Instantiate(hit_effect_prefab,postion_hit,Quaternion.identity);
        hit_effect.GetComponent<ParticleSystem>().Emit(5);
        Destroy(hit_effect,1.5f);
        yield return new WaitForSeconds(1f);
        is_interVal=false;
    }


    public void active_souonds_effect(Collider other,bool is_hitBack)
    {
        StartCoroutine(interval_hitTrigger(other,is_hitBack));
    }

     public void active_souonds_effect_rigbody(Collision other,bool is_hitBack)
    {
        StartCoroutine(interval_hit(other,is_hitBack));
    }
    

  /*  private void OnCollisionEnter(Collision other) {
       if(other.transform.gameObject.tag=="Weapon_inHand")
        {
            print("硬碰撞体"+other.gameObject.name+" 自己叫 :"+transform.gameObject.name);
            if(is_rigbodyDetect)
            {
                Cur_weapon_speed=GetComponent<Rigidbody>().velocity.magnitude;
              
            }else
            {
                      Physic_weaponManager weaponManager=transform.GetComponent<Physic_weaponManager>();
                Cur_weapon_speed=weaponManager.Cur_weaponSpeed;
            }   
         //  Physic_weaponManager weaponManager=transform.GetComponent<Physic_weaponManager>();
           
            if(Cur_weapon_speed>Collider_speed)
            {
              // weaponManager.Close_collider();
                if(is_interVal==false)
                {       
                    print("进入间隔1");
                    StartCoroutine(interval_hit(other));
                }
                
                
            }
            
            //print(closestPoint);
        }
    }*/

   /* private void OnTriggerEnter(Collider other) {
       if(other.transform.gameObject.tag=="Weapon_inHand")
        {
            print("击打"+other.gameObject.name+" 自己叫 :"+transform.gameObject.name);
          
                if(is_interVal==false)
                {       
                    print("进入间隔1");
                    StartCoroutine(interval_hitTrigger(other,false));
                }
                
            
            
            //print(closestPoint);
        }
    }*/
}
