/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 交互 UI 系统 / Interactive UI system
 * 功能 / Purpose: 处理菜单点击、提示框、血条、物品菜单、UI 定位和玩家 HUD。
 * English: Handles menu clicks, hint boxes, HP bars, item menus, UI positioning, and player HUD.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player_UI_Controller : MonoBehaviour
{
   
    // Start is called before the first frame update
     // Start is called before the first frame update
    public GameObject Detect_point;
   // public GameObject Detect_left_right_point;
    private Vector3 pre_position;
   // private Vector3 Right_left_localposition;
    private float speed;
    public float MAX_speed;
    public GameObject UI_prefab;
    public GameObject UI_generation_point;
    public GameObject UI_message_point;

    public GameObject SAO_ui;
    private bool UI_fade_Flag=false;
    public bool ui_can_close;
    public AudioSource close_bgm;
    public GameObject UI_look_at;
    public float MAX_abs_value;
    public GameObject Web_generate_point;
    bool UI_is_closed;
    public Transform playerCamera;
    private bool is_UI_Ger_interval;
    public Hand_and_controller hand_And_Controller;

   // public HVRGlobalInputs input;
    void Start()
    {
       // pre_position=Detect_point.transform.position;
        pre_position = playerCamera.InverseTransformPoint(Detect_point.transform.position);
      //  Right_left_localposition=Detect_left_right_point.transform.localPosition;
    }

   

    IEnumerator close_ready()
    {
        yield return new WaitForSeconds(0.5f);
        is_UI_Ger_interval=false;
        ui_can_close=true;
    }

    IEnumerator wait_2CloseUI()
    {
        yield return new  WaitForSeconds(0.5f);
        UI_fade_Flag=false;
        ui_can_close=false;
        Destroy(SAO_ui);
        UI_is_closed=false;

    }

    public void 生成UI()
    {
        if(is_UI_Ger_interval) return;
         is_UI_Ger_interval=true;
                if(null==SAO_ui)
                {
                        SystemUI_Ger();
                }
                else
                {
                        关闭UI();
                        Invoke("SystemUI_Ger",0.6f);
                     //   SystemUI_Ger();
                }
    }

    private void SystemUI_Ger()
    {
                    SAO_ui=Instantiate(UI_prefab,UI_generation_point.transform.position,Quaternion.identity) as GameObject;
                    SAO_ui.transform.LookAt(UI_look_at.transform);
                    StartCoroutine(close_ready());
    }

    public void 关闭UI()
    {
            if(null!=SAO_ui)
            {
                //SAO_ui.GetComponent<Fllow_player>().close_follow();
                if(UI_is_closed) return;
                    UI_is_closed=true;
                    SAO_ui.GetComponentInChildren<Animator>().enabled=false;
                    SAO_ui.GetComponentInChildren<UI_manager>().Call_ALLStream2_close();
                    close_bgm.Play();
                    StartCoroutine(wait_2CloseUI());
                //  Destroy(SAO_ui);
                // SAO_ui=Instantiate(UI_prefab,UI_generation_point.transform.position,Quaternion.Euler(new Vector3(22.06f,0,0)));
                    UI_fade_Flag=true;
            }
           
    }
   
    public GameObject Standard_Generate_UI(GameObject UI,GameObject generate_point)
    {
                    GameObject  generation_ui;
                    if(null!=generate_point)
                     generation_ui=Instantiate(UI,generate_point.transform.position,generate_point.transform.rotation) as GameObject;
                    else
                   generation_ui=Instantiate(UI,UI_message_point.transform.position,Quaternion.identity) as GameObject;
                   generation_ui.transform.LookAt(UI_look_at.transform);
                   return generation_ui;
                   
    }

    public GameObject Web_Generate_UI(GameObject UI,GameObject generate_point)
    {
                    GameObject  generation_ui;
                    if(null!=generate_point)
                     generation_ui=Instantiate(UI,Web_generate_point.transform.position,Quaternion.identity) as GameObject;
                    else
                   generation_ui=Instantiate(UI,Web_generate_point.transform.position,Quaternion.identity) as GameObject;
                   generation_ui.transform.LookAt(UI_look_at.transform);
                   return generation_ui;
                   
    }

    public bool Get_HandMotion_result_V()
    {
             // 获取点在玩家视角下的当前位置
             Vector3 CurrentRelativePosition = playerCamera.InverseTransformPoint(Detect_point.transform.position);

            var  speed=(CurrentRelativePosition-pre_position).magnitude/0.02f;
             // 计算点在玩家视角中的运动方向
            Vector3 movementDirection = CurrentRelativePosition - pre_position;
            float Vector_Y = CurrentRelativePosition.y - pre_position.y;
            float Vector_X = CurrentRelativePosition.x - pre_position.x;
            if (movementDirection.y<0&&Mathf.Abs(Vector_Y)>0.005f&&Mathf.Abs(Vector_X)<0.001f) return true;
            else return false;
    }

    public bool Get_HandMotion_result_H()
    {
             // 获取点在玩家视角下的当前位置
             Vector3 CurrentRelativePosition = playerCamera.InverseTransformPoint(Detect_point.transform.position);

            var  speed=(CurrentRelativePosition-pre_position).magnitude/0.02f;
             // 计算点在玩家视角中的运动方向
            Vector3 movementDirection = CurrentRelativePosition - pre_position;
            float Vector_Y = CurrentRelativePosition.y - pre_position.y;
            float Vector_X = CurrentRelativePosition.x - pre_position.x;
            if (null!=SAO_ui&&ui_can_close&&movementDirection.x<0&&Mathf.Abs(Vector_X)>0.005f&&Mathf.Abs(Vector_Y)<0.001f)
            return true;
            else return false;
    }

    private void UI_call_check()
    {
        if(hand_And_Controller!=null)
        if(hand_And_Controller.is_handtracing) return;
            // 获取点在玩家视角下的当前位置
             Vector3 CurrentRelativePosition = playerCamera.InverseTransformPoint(Detect_point.transform.position);

            var  speed=(CurrentRelativePosition-pre_position).magnitude/0.02f;
             // 计算点在玩家视角中的运动方向
            Vector3 movementDirection = CurrentRelativePosition - pre_position;
            float Vector_Y = CurrentRelativePosition.y - pre_position.y;
            float Vector_X = CurrentRelativePosition.x - pre_position.x;
           // print("X轴数值为："+movementDirection.x);
           
            if (null!=SAO_ui&&ui_can_close&&movementDirection.x<0&&Mathf.Abs(Vector_X)>0.005f&&Mathf.Abs(Vector_Y)<0.001f)
            {
                Debug.Log("点从右往左运动");
                关闭UI();
            }
            if (movementDirection.y<0&&Mathf.Abs(Vector_Y)>0.005f&&Mathf.Abs(Vector_X)<0.001f)
            {
                Debug.Log("点从上往下运动");
               //  SAO_ui=Instantiate(UI_prefab,UI_generation_point.transform.position,Quaternion.identity) as GameObject;
                 //  SAO_ui.transform.LookAt(UI_look_at.transform);
                   生成UI();
                    //StartCoroutine(close_ready());
            }


    }

    private void FixedUpdate() {
        
        if(OVRInput.Get(OVRInput.Button.One,OVRInput.Controller.RTouch))
        {
            UI_call_check();
        }
        if(OVRInput.GetDown(OVRInput.Button.Start,OVRInput.Controller.LTouch))
        {
            if(SAO_ui==null)
            生成UI();
            else  关闭UI();
        }
       /* if(input&&input.RightPrimaryButtonState.Active)
        {
            UI_call_check();
        }*/
        pre_position= playerCamera.InverseTransformPoint(Detect_point.transform.position);
     //  Right_left_localposition=Detect_left_right_point.transform.localPosition;
    }

    private void Deal_UI_FadeAnimation()
    {
        print("执行UI褪去");
        SAO_ui.transform.Translate(SAO_ui.transform.right*Time.deltaTime*1.2f,Space.World);
    }

    // Update is called once per frame
    void Update()
    {
        if(UI_fade_Flag) Deal_UI_FadeAnimation();
    }
}
