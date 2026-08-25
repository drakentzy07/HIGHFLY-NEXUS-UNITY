/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 玩家系统 / Player system
 * 功能 / Purpose: 管理玩家生命、资源、VR 控制、状态重置和战斗交互入口。
 * English: Manages player HP, resources, VR control, state reset, and combat interaction entry points.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using OVR;
using UnityEngine.AI;
using UnityEngine.Animations;
public class Player_managerV2 : MonoBehaviour
{
    [Tooltip("控制AR状态下不会被击退")]
    public VRplayer_controller vRplayer_Controller;//控制玩家运动的控制器
    public bool is_AR;
    [Header("是否启用注视角色锁定")]
    public float look_locate_time=2f;
    public bool Need_LocateFUtion;
    public LayerMask layer;  
    public Transform Ray_shoot_Ori;
    public GameObject temp_hit;
    private float Duration_time=0;
    private bool is_located;
    private GameObject Located_character;
    private GameObject Skill_tempCharacter;
    public string Hint_message;
    public GameObject Hint_prefab;
    GameObject  generation_ui;
    public float DeSelect_GameObject_time=7f;
    public Image select_checkDisplay;
    [Range(0, 1)] [SerializeField] private float percentage;
    public AudioSource 被砍;
    public AudioSource 拳击;
    public AudioSource 子弹;

    public AudioClip 体力耗尽音效;
    


    //角色数据
    public float Max_Hp=100;
     float Cur_Hp;
    public Image Hp_bar;
    private bool is_interval=false;

    //角色信息UI
    public TextMeshProUGUI Hp_number_text;
    public TextMeshProUGUI Level_number_text;
    public TextMeshProUGUI Player_name_text;
    public Skill_Display skill_LeftDisplay;
    public Skill_Display skill_RightDisplay;

    //残血效果
    public GameObject Low_hp_Canvas;
    public PostProcessVolume volume;
    public GameObject red_sideEdge;
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

    //手
    public  UnityEngine.XR.InputDevice leftHand_device;
    public UnityEngine.XR.InputDevice RightHand_device;
    bool left_hand_get;
    bool right_hand_get;
    public float 振幅=0.5f;
    public uint 频道=0;
    public float 持续时长=1f;

    public Transform centerEye;
    public GameObject Be_attack;

    //体力系统
    [Tooltip("体力数值")]
    public float Max_stamina=100f;
    private float Cur_stamina;
    public float stamina_recover=5f;
    public float stamina_recoverInterval=2f;
    bool is_recoverStamina;
    bool is_sprintStamina;
    bool is_wait2Recover;
    public float Sprint_cosumePer_second=20f;
    public Image stamina_bar;
    public UI_hiden uI_HidenStamina;
    public UI_hiden uI_HidenMP;

    public float UI_waitTime=7f;
    private float lastStaminaValue;
    private float timeSinceLastChange=0;

    //运动消耗
    public float jump_stamina=20f;   //跳跃消耗体力
    public float SwingSword_stamina=10f; //重挥舞消耗体力
    public float HeavyParry_stamina=10f; //重攻击格挡消耗体力
    public float Parry_stamina=1f; //普通格挡消耗体力

    //魔法系统
    [Tooltip("魔力数值")]
    public float Max_mana=100f;
    private float Cur_mana;
    public float mana_recover=1f;
    private float last_Mana;
    private float timeSinceLastChangeMana=0f;
    //public float mana_recoverInterval=2f;//魔法美分每秒都在恢复
    //private bool is_recoverMana; //魔法美分每秒都在恢复
     public Image mana_bar;

   
   void Start()
    {
        Cur_Hp=Max_Hp;
        Cur_stamina = Max_stamina;
        Cur_mana = Max_mana;
        lastStaminaValue = Cur_stamina;
        last_Mana = Cur_mana;
        Hp_bar.fillAmount=1;
        Hp_number_text.text=Cur_Hp.ToString()+"/"+Max_Hp.ToString();
     
       // Move_speed_normal=HVRPlayer_controller.MoveSpeed;
        // 获取手部();
       // Invoke("振动左手",10f);
       // Invoke("振动右手",10f);
      //  OVRInput.SetControllerVibration(1f,1f,OVRInput.Controller.Active);
        
    }

    public void Pull_player_back(Vector3 dir,float flyTime)
    {
        if(is_AR) return;
        vRplayer_Controller.Pull_playerBackWard(dir,flyTime);
    }

    public void Active_controller_impusle()
    {
        OVRInput.SetControllerVibration(1f,1f,OVRInput.Controller.Active);
    }

    public void Stop_controller_impusle()
    {
        OVRInput.SetControllerVibration(0f,0f,OVRInput.Controller.Active);
    }

     private void MonitorStaminaChange()
    {
        // 如果体力值发生变化，重置计时器
        if (Cur_stamina != lastStaminaValue)
        {
            timeSinceLastChange = 0f;

            // 如果UI是隐藏状态，则激活它
            if(!uI_HidenStamina.is_activate)
            uI_HidenStamina.FadeIn();
           
            
        }
        else
        {
            timeSinceLastChange += Time.fixedDeltaTime;

            // 如果UI处于激活状态且超过7秒无变化，则隐藏UI
            if (uI_HidenStamina.is_activate && timeSinceLastChange >= UI_waitTime)
            {
                uI_HidenStamina.FadeOut();
            }
        }

        // 在每次FixedUpdate中更新lastStaminaValue，以便于下一次对比
        lastStaminaValue = Cur_stamina;
    }

    private void FixedUpdate() {
        if(hpBar_moving)  Hp_move();
      //  if(!left_hand_get) 振动左手();
       // if(!right_hand_get) 振动右手();
       if(RightHand_device.name==null) 获取手部();
       if(is_recoverStamina) recover_stamina(); //执行体力恢复
       recover_mana();  //执行魔力恢复，魔力的恢复比较慢的话，建议不会被打断，只要不满就在恢复，和体力不同。
       if(is_sprintStamina) SprintStamina();//执行奔跑的体力消耗，奔跑的过程中体力不会恢复，机制和魔法不同，体力要停下来休息才会恢复。
       if(!is_recoverStamina&&Cur_stamina!=Max_stamina&&!is_wait2Recover&&!is_sprintStamina) start_recoverStamina();//除了正在恢复的过程中以及满体力除外，应该时刻在等待恢复的过程
       MonitorStaminaChange();
       MonitorManaChange();
      // if(OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger)) Active_controller_impusle();
       //OVRInput.SetControllerVibration(1f,1f,OVRInput.Controller.Active);
       // if(is_motoinSkill_release) moveforword_2target();
       // if(HVR_inputDetect.LeftPrimaryButtonState.Active) voice_input_controller.Activate_voice();

       /* if(is_swingHand_2Run) {
            if(HVR_inputDetect.LeftJoystickAxis.y>0) Check_Run();
            else if(Left_hand_pre_position!=Vector3.zero) Reset_pre_position();
        }*/

    }

    

    public void stop_recoverStamina()
    {
        is_recoverStamina = false;
    }

    private void start_recoverStamina()
    {
        is_recoverStamina = true;
    }

    private void recover_stamina()
    {
        // 每隔指定的时间间隔恢复一定量的体力
        if (Cur_stamina < Max_stamina)
        {
            Cur_stamina += stamina_recover * Time.fixedDeltaTime;

            // 更新体力条显示
            fresh_staminaBar(Cur_stamina / Max_stamina);
        }else
        {
            Cur_stamina = Max_stamina;
            fresh_staminaBar(1);
        }
    }

    private void fresh_staminaBar(float Bar_amount)
    {
        stamina_bar.fillAmount = Bar_amount;
    }
    private void stopRecover_andRestartWait()
    {
        stop_recoverStamina();
        start_wait2Recover_Stamina();
    }

    public bool Minus_Jump()
    {
        bool res = Minus_StaminaOnce(jump_stamina);
        if(res)
        stopRecover_andRestartWait();
        return res;
    }
    public bool Minus_HeavySwing()
    {
        bool res = Minus_StaminaOnce(SwingSword_stamina,false);
        if(res)
        stopRecover_andRestartWait();
        return res;
    }

    public bool Minus_Skill(float amout)
    {
        bool res = Minus_StaminaOnce(amout,false);
        if(res)
        stopRecover_andRestartWait();
        return res;
    }
    public bool Minus_Parrying()
    {
        //stopRecover_andRestartWait();
        Minus_NotEnoughOnce(Parry_stamina);
        
        return true;
    }
    public bool Minus_HeavyParrying()
    {
        stopRecover_andRestartWait();
        Minus_NotEnoughOnce(HeavyParry_stamina);
        return true;
    }
    private bool Minus_NotEnoughOnce(float amount)
    {
         if(Cur_stamina<amount)  
         {
            Cur_stamina=0;
            fresh_staminaBar(0);
            return false;
         }else
         {
            Cur_stamina-=amount;
            fresh_staminaBar(Cur_stamina / Max_stamina);
            return true;
         }
    }

    private bool Minus_StaminaOnce(float amount,bool playsound=true)
    {
            if(Cur_stamina<amount) 
            { 
                    if(playsound)
                    Play_staminaZero_sound(); 
                    return false;
            }
            Cur_stamina-=amount;
            fresh_staminaBar(Cur_stamina / Max_stamina);
        return true;
    }

    private void Play_staminaZero_sound()
    {
        GetComponent<AudioSource>().clip=体力耗尽音效;
        GetComponent<AudioSource>().Play();
    }

     public void Add_StaminaOnce()
    {

    }

    public void start_SprintStamina()
    {
        is_sprintStamina=true;
    }

    public void stop_SprintStamina()
    {
        is_sprintStamina=false;
    }

    public bool is_staminaZero()
    {
       

        return Cur_stamina>0?false : true;
    }

    public void SprintStamina()
    {
        if(Cur_stamina>0)
        {
             // 每次FixedUpdate减少的体力值
            float staminaReduction = Sprint_cosumePer_second * Time.fixedDeltaTime;

            // 如果当前体力大于消耗量，则进行消耗
            if (Cur_stamina >= staminaReduction)
            {
                Cur_stamina -= staminaReduction;
            }
            else
            {
                // 如果体力不足以消耗全部，则将体力降为0，并停止奔跑
                Cur_stamina = 0;
                //stop_SprintStamina();
            }
            fresh_staminaBar(Cur_stamina / Max_stamina); 
        }else
        {
                 Cur_stamina = 0;
                 //stop_SprintStamina();
                 fresh_staminaBar(0); 
        }
    }

    public void start_wait2Recover_Stamina()
    {   
        StopCoroutine("Wait2Recover_Stamina");
        StartCoroutine("Wait2Recover_Stamina");
    }



    IEnumerator Wait2Recover_Stamina()
    {
        is_wait2Recover=true;
        yield return new WaitForSeconds(stamina_recoverInterval);
        is_wait2Recover=false;
       // start_recoverStamina();
    }


//魔法相关++++++++++++++++++++++++++++++++++++++++++

    private void recover_mana()
    {
         // 每隔指定的时间间隔恢复一定量的体力
        if (Cur_mana < Max_mana)
        {
            Cur_mana += mana_recover * Time.fixedDeltaTime;

            // 更新体力条显示
            fresh_ManaBar(Cur_mana / Max_mana   );
        }else
        {
            Cur_mana = Max_mana;
            fresh_ManaBar(1);
        }
    }

     private void fresh_ManaBar(float Bar_amount)
    {
        if(null!=mana_bar)
        mana_bar.fillAmount = Bar_amount;
    }

    public bool Minus_ManaOnce(float amount)
    {
        if(Cur_mana<amount) return false;
            Cur_mana-=amount;
            fresh_ManaBar(Cur_mana / Max_mana);
        return true;
    }

    private void MonitorManaChange()
    {
        // 如果体力值发生变化，重置计时器
        if (Cur_mana != last_Mana)
        {
            timeSinceLastChangeMana = 0f;

            // 如果UI是隐藏状态，则激活它
            if(!uI_HidenMP.is_activate)
            uI_HidenMP.FadeIn();
           
            
        }
        else
        {
            timeSinceLastChangeMana += Time.fixedDeltaTime;

            // 如果UI处于激活状态且超过7秒无变化，则隐藏UI
            if (uI_HidenMP.is_activate && timeSinceLastChangeMana >= UI_waitTime)
            {
                uI_HidenMP.FadeOut();
            }
        }

        // 在每次FixedUpdate中更新lastStaminaValue，以便于下一次对比
        last_Mana = Cur_mana;
    }


   //魔力部分结束============================================================
   

     private void 获取手部()
    {
        print("进入获取手部");
            var leftHandedControllers = new List<UnityEngine.XR.InputDevice>();
             var desiredCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Left | UnityEngine.XR.InputDeviceCharacteristics.Controller;
              UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, leftHandedControllers);
 
             foreach (var device in leftHandedControllers)
            {
			        print("振动进入获取设备");
                    Debug.Log(string.Format("左手设备名 '{0}' 该设备具有的特征 '{1}'", device.name, device.characteristics.ToString()));
                     leftHand_device=device;
             }

             var RightHandedControllers = new List<UnityEngine.XR.InputDevice>();
             var desiredCharacteristics_Right = UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Right | UnityEngine.XR.InputDeviceCharacteristics.Controller;
             UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics_Right, RightHandedControllers);
 
            foreach (var device in RightHandedControllers)
            {
			    print("振动进入获取设备");
                Debug.Log(string.Format("右手设备名 '{0}' 该设备具有的特征 '{1}'", device.name, device.characteristics.ToString()));
                 RightHand_device=device;
            }
    }


 public void 振动左手()
	{
		

	   UnityEngine.XR.HapticCapabilities capabilities;
	   if (leftHand_device.TryGetHapticCapabilities(out capabilities))
            {
                     print("进入振动逻辑1");
           
                    if (capabilities.supportsImpulse)
                    {
                         print("进入振动逻辑2");
                      //  uint channel = 0;
                       // float amplitude = 5f;
                     //   float duration = 5.0f;
                        leftHand_device.SendHapticImpulse(频道, 振幅, 持续时长);
						left_hand_get=true;
                    }
            }
	}

    public void 振动右手()
	{
		
	   UnityEngine.XR.HapticCapabilities capabilities;
	   if (RightHand_device.TryGetHapticCapabilities(out capabilities))
            {
                     print("进入振动逻辑1");
           
                    if (capabilities.supportsImpulse)
                    {
                         print("进入振动逻辑2");
                      //  uint channel = 0;
                       // float amplitude = 5f;
                     //   float duration = 5.0f;
                        RightHand_device.SendHapticImpulse(频道, 振幅, 持续时长);
                        right_hand_get=true;
                    }
            }
	}

   
   private void LateUpdate() {
         if(Need_LocateFUtion) locate_packageItem();
   }

    private void locate_packageItem()
    {
         Keep_look();
    }

    private void  Keep_look()
    {
        Ray ray = new Ray(Ray_shoot_Ori.transform.position, Ray_shoot_Ori.transform.forward);
       // Ray ray = new Ray(Camera.main.transform.position, Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit,1000f,layer))
        {
            Debug.Log(hit.collider.gameObject.name);
            Debug.Log("当前标签是"+hit.collider.gameObject.tag);
            if(is_located==false&&("Weapon"==hit.collider.gameObject.tag||"Item"==hit.collider.gameObject.tag||"AI_system"==hit.collider.gameObject.tag||"Weapon_inHand"==hit.collider.gameObject.tag||"Gun_inhand"==hit.collider.gameObject.tag||"Gun"==hit.collider.gameObject.tag))
            {
                    Duration_time+=0.02f;
                    if(Duration_time>look_locate_time)
                    {
                        Activate_located(hit.collider.gameObject);
                    }
                           
            }else
            Duration_time=0;
             
        }else
        Duration_time=0;

        select_checkDisplay.fillAmount=Duration_time/look_locate_time;

    }



    //激活物品信息的hub，并且要把对物体的操作权限委托过去
    private void Activate_located(GameObject hit)
   {
                        is_located=true;
                        temp_hit=hit;
                        Activate_Std_messageBox(hit.transform);
                         MessageBox_controller mess =generation_ui.transform.FindChildRecursive("Ui_body").GetComponent<MessageBox_controller>();
                        if(mess==null) print("messcontroller为空");
                        else mess.Deal_Confirm_mission+=Close_Cur_locateGameObject;
                        StartCoroutine("Delay_2Deactivate");
                      //  body_outline.enabled=true;
   }
   

    IEnumerator Delay_2Deactivate()
   {
       yield return new WaitForSeconds(DeSelect_GameObject_time);
       Deactivate_located();
   }

   private void Deactivate_located()
   {
                    //location_CharacterBGM.Stop();
                  //  temp_hit=null;
                    is_located=false;
                    Duration_time=0f;
                  //  body_outline.enabled=false;
   }






    //生成UI
    private  void Activate_Std_messageBox(Transform UI_gerneratePoint)
    {
        //生成UI，并且把信息填入
         Vector3 gerneratePoint=Vector3.Lerp(Ray_shoot_Ori.position,UI_gerneratePoint.position , percentage);
         generation_ui=Instantiate(Hint_prefab,gerneratePoint,Quaternion.identity) as GameObject;
         if(centerEye!=null) generation_ui.transform.LookAt(centerEye);
         else generation_ui.transform.LookAt(transform);
         generation_ui.transform.FindChildRecursive("Information").GetComponent<TextMeshProUGUI>().text=Hint_message;
    }

   

    public void Close_Cur_locateGameObject()
    {
        if(null!=temp_hit)
        {
            //Skill_close
           // if(null!=skill_Display)
            Destory_self child_script=temp_hit.GetComponent<Destory_self>();

            if(child_script.parent_joint.tag=="Weapon_inHand")
            skill_LeftDisplay.Close_Display();
            if(child_script.parent_joint.tag=="Weapon_inRightHand")
            skill_RightDisplay.Close_Display();
            child_script.Destroy_selfGameObject();
            temp_hit=null;
            is_located=false;
            Duration_time=0f;
        }
    }



    public void Minus_Hp(float minus_amount,string weapon_type_sound)
   {
        Player_UI_Controller uI_Controller = GameObject.FindWithTag("Player").GetComponent<Player_UI_Controller>();
        if(null!=uI_Controller) uI_Controller.关闭UI();
        Be_attack.SetActive(true);
            switch(weapon_type_sound)
            {
                case "切割":
                if(null!=被砍)
                被砍.Play();
                break;
                case "拳击":
                if(null!=拳击)
                拳击.Play();
                break;
                case "子弹":
                if(null!=子弹)
                子弹.Play();
                break;
            }
            
        
       
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

     private void Update_palyerStatus()
    {
        Hp_number_text.text=Cur_Hp.ToString()+"/"+Max_Hp.ToString();
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
       // GetComponent<CapsuleCollider>().enabled=false;
        check_isLast_attack();
        //Deal_hp_zero();
        yield return new WaitForSeconds(0.1f);
        is_interval=false;
    }

    private void check_isLast_attack()
    {
        
            hpMove_amout_per=0.001f;
            pre_death.Play();
            Low_hp_bgm.Stop();
        
    }

    private void Deal_hpColor(float flat)
   {
       Color nowColor;
       if(flat<0.21)
        {
            ColorUtility.TryParseHtmlString("#FF0000", out nowColor);
            Hp_bar.color=nowColor;
            //volume.profile.GetSetting<Vignette>().active=false;
            red_sideEdge.SetActive(false);
                Low_hp_Canvas.SetActive(true); 
                if(Low_hp_bgm.isPlaying==false&&Cur_Hp>0)
                Low_hp_bgm.Play();
                Middle_hp_bgm.Stop();
        }else if(flat>=0.21f&&flat<0.51f)
        {
               ColorUtility.TryParseHtmlString("#FFA600", out nowColor);
                Hp_bar.color=nowColor; 
                red_sideEdge.SetActive(true);
                //volume.profile.GetSetting<Vignette>().active=true;
                Low_hp_Canvas.SetActive(false); 
                Low_hp_bgm.Stop();
                if(Middle_hp_bgm.isPlaying==false)
                Middle_hp_bgm.Play();
        }
        else
        {
              ColorUtility.TryParseHtmlString("#FFFFFF", out nowColor);
            Hp_bar.color=nowColor;  
            red_sideEdge.SetActive(false);
            //volume.profile.GetSetting<Vignette>().active=false;
                Low_hp_Canvas.SetActive(false); 
                Low_hp_bgm.Stop();
                Middle_hp_bgm.Stop();
        }
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
             //  Scene_manager manager =GameObject.FindWithTag("Scene_Manager").GetComponent<Scene_manager>();
             //   manager.Disable_All_enemy();
                Low_hp_Canvas.SetActive(false); 
                if(null!=Death_Mask)
                Death_Mask.SetActive(true); //这里弹出你输了
                Death_pop.Play();
                GameObject.FindWithTag("Battle_manager").GetComponent<Battle_manager>().Battle_end("lose");
               //  GetComponent<CapsuleCollider>().enabled=true;
                Reset_playerStatus();
                Invoke("Play_afterDeath",0.4f);
            }
        }
    }

    public void Reset_playerStatus()
    {
        hpBar_moving=false;
        Cur_Hp=Max_Hp;
        Update_palyerStatus();
        Hp_bar.fillAmount=1f;
        Color nowColor;
        hpMove_amout_per=0.005f;
        ColorUtility.TryParseHtmlString("#FFFFFF", out nowColor);
        Hp_bar.color=nowColor; 
        red_sideEdge.SetActive(false);
       // volume.profile.GetSetting<Vignette>().active=false;
        Low_hp_Canvas.SetActive(false); 
        Goal_hp_percent=1f;
        Low_hp_bgm.Stop();
        Middle_hp_bgm.Stop();
        Hp_bar.color=nowColor;  
    }

    private void Play_afterDeath()
    {
              //  Scene_manager manager =GameObject.FindWithTag("Scene_Manager").GetComponent<Scene_manager>();
             //   manager.Close_ALL_BGM();  //关闭所有战斗相关的音乐
             if(null!=after_death)
              after_death.Play();//血条归零后的持续背景音乐
           //    float time=after_death.clip.length;
              //  Invoke("Check_player_Mind",time);
    }



}
