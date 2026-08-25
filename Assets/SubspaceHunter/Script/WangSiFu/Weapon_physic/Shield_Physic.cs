/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 物理武器系统 / Physical weapon system
 * 功能 / Purpose: 管理抓取、释放、刚体添加、盾牌阻挡、武器碰撞和轨迹检测。
 * English: Manages grabbing, releasing, Rigidbody setup, shield blocking, weapon collision, and trace detection.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield_Physic : MonoBehaviour
{
    bool is_interVal;
    public GameObject hit_effect_prefab;
    public float hit_speed=4f;
    public float Heavyhit_speed=7f;
    public AudioSource Get_Hit;
     [Range(1,10)]
    public float 对武器的伤害=5f;
    public woosh_controller woosh_Controller;
   /* private void OnCollisionEnter(Collision other) {
       if(other.transform.gameObject.tag=="Weapon_inHand")
        {
            print("硬碰撞体"+other.gameObject.name+" 自己叫 :"+transform.gameObject.name);
            
                 Physic_weaponManager weaponManager=transform.GetComponent<Physic_weaponManager>();
                 weaponManager.Close_collider();
                if(is_interVal==false)
                {       
                    print("进入间隔1");
                    StartCoroutine(interval_hit(other));
                }
                
            //print(closestPoint);
        }
    }*/

    private void OnTriggerEnter(Collider other) {
        if(other.transform.gameObject.tag=="Weapon_inHand"||other.transform.gameObject.tag=="Player_bullet"||other.transform.gameObject.tag=="Weapon_inRightHand")
        {
            print("硬碰撞体"+other.gameObject.name+" 自己叫 :"+transform.gameObject.name);
                
                if(other.transform.gameObject.tag=="Player_bullet")
                {
                     if(is_interVal==false)
                    {       
                        print("进入BBBBBB");
                        Destroy(other.gameObject);
                        StartCoroutine(interval_hitTrigger(other,false));
                    }   
                }else
                {
                        Physic_weaponManager weaponManager=other.transform.GetComponent<Physic_weaponManager>();
                        if(weaponManager.Cur_weaponSpeed<=hit_speed)
                        {
                            weaponManager.Deal_short_impusle();
                           // weaponManager.Close_collider();
                        }else if(weaponManager.Cur_weaponSpeed<Heavyhit_speed)
                        {
                            weaponManager.Deal_middle_impusle();
                           // weaponManager.Close_collider();
                             weaponManager.weapon_Durability.Mins_dur(对武器的伤害);
                            if(is_interVal==false)
                            {       
                                print("进入BBBBBB");
                                StartCoroutine(interval_hitTrigger(other,false));
                            }
                        }
                        else
                        {
                             weaponManager.Deal_long_impusle();
                           // weaponManager.Close_collider();
                            weaponManager.weapon_Durability.Mins_dur(对武器的伤害);
                            if(is_interVal==false)
                            {       
                                print("进入BBBBBB");
                                StartCoroutine(interval_hitTrigger(other,false));
                            }
                        }
                       
                }
                
                
            //print(closestPoint);
        }
    }


    private void OnCollisionEnter(Collision other) {
            if(other.transform.gameObject.tag=="Weapon_inHand"||other.transform.gameObject.tag=="Player_bullet"||other.transform.gameObject.tag=="Weapon_inRightHand")
        {
            print("硬碰撞体"+other.gameObject.name+" 自己叫 :"+transform.gameObject.name);
                
                if(other.transform.gameObject.tag=="Player_bullet")
                {
                     if(is_interVal==false)
                    {       
                        print("进入BBBBBB");
                        Destroy(other.gameObject);
                        StartCoroutine(interval_hitcollision(other,false));
                    }   
                }else
                {
                      Physic_weaponManager weaponManager=other.transform.GetComponent<Physic_weaponManager>();
                    if(weaponManager.Cur_weaponSpeed<=hit_speed)
                        {
                            weaponManager.Deal_short_impusle();
                        }else if(weaponManager.Cur_weaponSpeed<Heavyhit_speed)
                        {
                            weaponManager.Deal_middle_impusle();
                            weaponManager.Close_collider();
                             weaponManager.weapon_Durability.Mins_dur(对武器的伤害);
                            if(is_interVal==false)
                            {       
                                print("进入BBBBBB");
                                StartCoroutine(interval_hitcollision(other,false));
                            }
                        }
                        else
                        {
                             weaponManager.Deal_long_impusle();
                            weaponManager.Close_collider();
                            weaponManager.weapon_Durability.Mins_dur(对武器的伤害);
                            if(is_interVal==false)
                            {       
                                print("进入BBBBBB");
                                StartCoroutine(interval_hitcollision(other,false));
                            }
                        }

                
                   
                }
                
                
            //print(closestPoint);
        }
    }

     IEnumerator interval_hitcollision(Collision other,bool is_hitBack)
    {
         is_interVal=true;   
         ContactPoint[] test_pointArrary= other.contacts;
                 Vector3 postion_hit =test_pointArrary[0].point;  
                   if(woosh_Controller!=null) woosh_Controller.Play_oneShot();
                   else  Get_Hit.Play();
        GameObject hit_effect=Instantiate(hit_effect_prefab,postion_hit,Quaternion.identity);
        print("进入CCCCCCC");
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
                     if(woosh_Controller!=null) woosh_Controller.Play_oneShot();
                   else  Get_Hit.Play();
        GameObject hit_effect=Instantiate(hit_effect_prefab,postion_hit,Quaternion.identity);
        print("进入CCCCCCC");
        hit_effect.GetComponent<ParticleSystem>().Emit(5);
        Destroy(hit_effect,1.5f);
        yield return new WaitForSeconds(1f);
        is_interVal=false;
    }





}
