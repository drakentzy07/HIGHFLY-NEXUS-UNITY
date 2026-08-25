/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 玩家系统 / Player system
 * 功能 / Purpose: 管理玩家生命、资源、VR 控制、状态重置和战斗交互入口。
 * English: Manages player HP, resources, VR control, state reset, and combat interaction entry points.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;



public enum Locate_type{
KEEP_LOOK,
HOLD_BUTTON_SELECT,

}

public class Player_Manager : MonoBehaviour
{
    //角色数据
    public float Max_Hp=100;
     float Cur_Hp;
    public Image Hp_bar;
    private bool is_interval=false;

    //角色信息UI
    public TextMeshProUGUI Hp_number_text;
    public TextMeshProUGUI Level_number_text;
    public TextMeshProUGUI Player_name_text;

    //残血效果
    public GameObject Low_hp_Canvas;
    public PostProcessVolume volume;
    public AudioSource Middle_hp_bgm;

    public AudioSource Low_hp_bgm;
    
//血条渐动效果模块
    private bool hpBar_moving;
    private float Goal_hp_percent;
    public float hpMove_amout_per=0.01f;
    //死亡音效
    public AudioSource pre_death;
    public AudioSource after_death;
    public AudioSource Death_pop;
    public GameObject Death_Mask;

    //锁定角色音效
    public AudioSource location_CharacterBGM;
    [Header("是否启用注视角色锁定")]
    public bool Need_LocateFUtion;
    public Transform Ray_shoot_Ori;
    private GameObject temp_hit;
    private float Duration_time=0;
    private bool is_located;
    public Locate_type locate_Type;
    

    //挥舞手臂跑步
    public bool  is_swingHand_2Run;
    public float Onece_runAdd_speed=4f;
   // public HVRPlayerController HVRPlayer_controller;
    //public HVRGlobalInputs HVR_inputDetect;
    private float Move_speed_normal;
    public GameObject Right_handDetect_point;
    public GameObject Left_handDetect_point;
    Vector3 Left_hand_pre_position;
    Vector3 Right_hand_pre_position;
   // public float Max_Move_distance;
    bool is_runKeep_status;
    public float Run_Inertial_interval=0.5f;
    public float Run_hand_swingDistance=0.03f;
    int  Activate_runCount;
    float Hand_swing_direction;

    
    
    // Start is called before the first frame update
    void Start()
    {
        Cur_Hp=Max_Hp;
        Hp_bar.fillAmount=1;
        Hp_number_text.text=Cur_Hp.ToString()+"/"+Max_Hp.ToString();
      //  Move_speed_normal=HVRPlayer_controller.MoveSpeed;

    }

    
    IEnumerator interval_minusHP(float minus_amount)
    {
        is_interval=true;
        Cur_Hp-=minus_amount;
        yield return new WaitForSeconds(0.1f);
        is_interval=false;
    }

    IEnumerator Set_HpZero()
    {
        is_interval=true;
        Cur_Hp=0;
        check_isLast_attack();
      //  Deal_hp_zero();
        yield return new WaitForSeconds(0.1f);
        is_interval=false;
    }

   public void Minus_Hp(float minus_amount)
   {
       if(0<Cur_Hp-minus_amount)
       {
         
           if(false==is_interval)
             StartCoroutine(interval_minusHP(minus_amount));
           
       }else
       {
           
           if(false==is_interval)
             StartCoroutine(Set_HpZero());
       }
       float flat=Cur_Hp/Max_Hp;
      //  Hp_bar.fillAmount=flat;
      
        Deal_hpColor(flat);
        Update_palyerStatus();
        
        Deal_hp_moving(flat);
        
   }

    private void Deal_hp_moving(float flat)
    {
        Goal_hp_percent=flat;
        hpBar_moving=true;
    }

    private void check_isLast_attack()
    {
        
            hpMove_amout_per=0.001f;
            pre_death.Play();
            Low_hp_bgm.Stop();
        
    }

    private void Hp_move()
    {

        if(Hp_bar.fillAmount-Goal_hp_percent>0)
        {
            if(Hp_bar.fillAmount>Goal_hp_percent)
            {
                Hp_bar.fillAmount-=hpMove_amout_per;
            }else
            {
                 Hp_bar.fillAmount=Goal_hp_percent;
            }
            
        }else if(Hp_bar.fillAmount-Goal_hp_percent<0)
        {
            if(Hp_bar.fillAmount<Goal_hp_percent)
            {
                Hp_bar.fillAmount+=hpMove_amout_per;
            }else
            {
                 Hp_bar.fillAmount=Goal_hp_percent;
            }
            
        }
        else
        {
            hpBar_moving=false;
            if(Cur_Hp==0)
            {
                Low_hp_Canvas.SetActive(false); 
                Death_Mask.SetActive(true);
                Death_pop.Play();
                Invoke("Play_afterDeath",0.4f);
            }
        }
    }

    private void Play_afterDeath()
    {
        after_death.Play();
    }


    private void Update_palyerStatus()
    {
        Hp_number_text.text=Cur_Hp.ToString()+"/"+Max_Hp.ToString();
    }

   private void Deal_hpColor(float flat)
   {
       Color nowColor;
       if(flat<0.21)
        {
            ColorUtility.TryParseHtmlString("#FF0000", out nowColor);
            Hp_bar.color=nowColor;
            volume.profile.GetSetting<Vignette>().active=false;
                Low_hp_Canvas.SetActive(true); 
                if(Low_hp_bgm.isPlaying==false&&Cur_Hp>0)
                Low_hp_bgm.Play();
                Middle_hp_bgm.Stop();
        }else if(flat>=0.21f&&flat<0.51f)
        {
               ColorUtility.TryParseHtmlString("#FFA600", out nowColor);
                Hp_bar.color=nowColor; 
                volume.profile.GetSetting<Vignette>().active=true;
                Low_hp_Canvas.SetActive(false); 
                Low_hp_bgm.Stop();
                if(Middle_hp_bgm.isPlaying==false)
                Middle_hp_bgm.Play();
        }
        else
        {
              ColorUtility.TryParseHtmlString("#FFFFFF", out nowColor);
            Hp_bar.color=nowColor;  
            volume.profile.GetSetting<Vignette>().active=false;
                Low_hp_Canvas.SetActive(false); 
                Low_hp_bgm.Stop();
                Middle_hp_bgm.Stop();
        }
   }


   public void Add_Hp(float add_amount)
   {
       if(Cur_Hp+add_amount<Max_Hp)
       {
           Cur_Hp+=add_amount;
       }else
       {
           Cur_Hp=Max_Hp;
       }
        float flat=Cur_Hp/Max_Hp;
      // Hp_bar.fillAmount=flat;
        Deal_hpColor(flat);
        Update_palyerStatus();
        Deal_hp_moving(flat);

   }

   private void Activate_located(GameObject hit)
   {
                        is_located=true;
                        Duration_time=0f;
                        print("进入A");
                        location_CharacterBGM.Play();
                        hit.GetComponent<Enemy_head>().enemy_StatusController.Activate_flow();
                        temp_hit=hit;
                      //  body_outline.enabled=true;
   }

   private void Deactivate_located()
   {
                    location_CharacterBGM.Stop();
                    temp_hit.GetComponent<Enemy_head>().enemy_StatusController.Stop_flow();
                    is_located=false;
                  //  body_outline.enabled=false;
   }


   private void locate_character()
   {
      
      switch(locate_Type)
      {
          case Locate_type.KEEP_LOOK:
           Keep_look();
            break;
          case Locate_type.HOLD_BUTTON_SELECT:
           break;
      }
               
   }

   IEnumerator Delay_2Deactivate()
   {
       yield return new WaitForSeconds(2f);
       Deactivate_located();
   }

private void Hold_button_select()
{
    
}



private void Keep_look()
{
         Ray ray = new Ray(Ray_shoot_Ori.transform.position, Ray_shoot_Ori.transform.forward);
       // Ray ray = new Ray(Camera.main.transform.position, Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit,1000f,LayerMask.GetMask("Enemy_body")))
        {
          //  Debug.Log(hit.collider.gameObject.name);
            Debug.Log("当前标签是"+hit.collider.gameObject.tag);
            if(is_located==false&&("Body"==hit.collider.gameObject.tag||"Head"==hit.collider.gameObject.tag))
            {
                    Duration_time+=0.02f;
                    if(Duration_time>4f)
                    {
                        Activate_located(hit.collider.gameObject);
                    }
                    
            }else if(is_located&&"Body"!=hit.collider.gameObject.tag&&"Head"!=hit.collider.gameObject.tag)
            {
                   // print("进入B");
                   // Deactivate_located();
                   StartCoroutine("Delay_2Deactivate");
            }else if(is_located&&("Body"==hit.collider.gameObject.tag||"Head"==hit.collider.gameObject.tag))
            {
                    StopCoroutine("Delay_2Deactivate");
            }
             
            //这里使用了RaycastHit结构体中的collider属性
            //因为hitInfo是一个结构体类型，其collider属性用于存储射线检测到的碰撞器。
            //通过collider.gameObject.name，来获取该碰撞器的游戏对象的名字。
        }else if(is_located)
        {
           //    print("进入C");
                 StartCoroutine("Delay_2Deactivate");
        }
}

    
    private void LateUpdate() {
        if(Need_LocateFUtion) locate_character();
    }


  

   IEnumerator Run_status_keep()
   {
       yield return new WaitForSeconds(Run_Inertial_interval);
      // HVRPlayer_controller.MoveSpeed=Move_speed_normal;
       is_runKeep_status=false;
   }

    private void Check_Run()
    {
        //处理第一次
        if(Left_hand_pre_position==Vector3.zero) 
        {
            Left_hand_pre_position=Left_handDetect_point.transform.position;
            Right_hand_pre_position=Right_handDetect_point.transform.position;
            return;
         }

            float leftHand_move_distance=Mathf.Abs(Left_hand_pre_position.y-Left_handDetect_point.transform.position.y);
            float RighHand_move_distance=Mathf.Abs(Right_hand_pre_position.y-Right_handDetect_point.transform.position.y);

          
                    //跑步激活
           /*  if(HVRPlayer_controller.MoveSpeed==Move_speed_normal&&!is_runKeep_status&&leftHand_move_distance>=Run_hand_swingDistance&&RighHand_move_distance>=Run_hand_swingDistance)
            {   
               
                     is_runKeep_status=true;
                 //    HVRPlayer_controller.MoveSpeed+=Onece_runAdd_speed;  
                     Activate_runCount=0;
                 
                 
               
            }
            //跑步维持
            else if(is_runKeep_status&&leftHand_move_distance>=Run_hand_swingDistance&&RighHand_move_distance>=Run_hand_swingDistance)
            {
                    StopCoroutine("Run_status_keep");
            }
            //跑步停止
            else if(is_runKeep_status&&leftHand_move_distance<Run_hand_swingDistance&&RighHand_move_distance<Run_hand_swingDistance)
            {
                   StartCoroutine("Run_status_keep");
            }*/

           /* if(Max_Move_distance<leftHand_move_distance)
               Max_Move_distance=leftHand_move_distance;*/

          
            Left_hand_pre_position=Left_handDetect_point.transform.position;
            Right_hand_pre_position=Right_handDetect_point.transform.position;

    }

    private void Reset_pre_position()
    {
         Left_hand_pre_position=Vector3.zero;
            Right_hand_pre_position=Vector3.zero;
          //  HVRPlayer_controller.MoveSpeed=Move_speed_normal;
    }

    


    private void FixedUpdate() {
        if(hpBar_moving)  Hp_move();
      

        if(is_swingHand_2Run) {
           // if(HVR_inputDetect.LeftJoystickAxis.y>0) Check_Run();
         //   else if(Left_hand_pre_position!=Vector3.zero) Reset_pre_position();
        }

    }


   


}
