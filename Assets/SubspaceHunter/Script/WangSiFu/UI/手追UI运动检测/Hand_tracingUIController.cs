/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 手势追踪 UI 检测 / Hand-tracking UI detection
 * 功能 / Purpose: 检测手部在 UI 区域中的运动并触发交互反馈。
 * English: Detects hand movement in UI regions and triggers interaction feedback.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand_tracingUIController : MonoBehaviour
{
    public GameObject Detect_point;
   // public GameObject Detect_left_right_point;
    private Vector3 pre_position;
   // private Vector3 Right_left_localposition;
   public Transform playerCamera;
   public Player_UI_Controller player_UI_Controller;

   /// <summary>
   /// Start is called on the frame when a script is enabled just before
   /// any of the Update methods is called the first time.
   /// </summary>
   void Start()
   {
         pre_position = playerCamera.InverseTransformPoint(Detect_point.transform.position);
   }
    public void Check_HandMotion_result_V()
    {
             // 获取点在玩家视角下的当前位置
             Vector3 CurrentRelativePosition = playerCamera.InverseTransformPoint(Detect_point.transform.position);

            var  speed=(CurrentRelativePosition-pre_position).magnitude/0.02f;
             // 计算点在玩家视角中的运动方向
            Vector3 movementDirection = CurrentRelativePosition - pre_position;
            float Vector_Y = CurrentRelativePosition.y - pre_position.y;
            float Vector_X = CurrentRelativePosition.x - pre_position.x;
            
            if (movementDirection.y<0&&Mathf.Abs(Vector_Y)>0.00005f&&Mathf.Abs(Vector_X)<0.002f)
             {print("垂直检测Y方向为"+movementDirection.y+" Y值为："+Vector_Y+" X值为："+Vector_X);
             player_UI_Controller.生成UI();}
          
    }

    public void Get_HandMotion_result_H()
    {
             // 获取点在玩家视角下的当前位置
             Vector3 CurrentRelativePosition = playerCamera.InverseTransformPoint(Detect_point.transform.position);

            var  speed=(CurrentRelativePosition-pre_position).magnitude/0.02f;
             // 计算点在玩家视角中的运动方向
            Vector3 movementDirection = CurrentRelativePosition - pre_position;
            float Vector_Y = CurrentRelativePosition.y - pre_position.y;
            float Vector_X = CurrentRelativePosition.x - pre_position.x;
            
            if (null!=player_UI_Controller.SAO_ui&&player_UI_Controller.ui_can_close&&movementDirection.x<0&&Mathf.Abs(Vector_X)>0.00005f&&Mathf.Abs(Vector_Y)<0.002f)
            {print("水平检测X方向为"+movementDirection.x+" Y值为："+Vector_Y+" X值为："+Vector_X);
            player_UI_Controller.关闭UI();}
    }

    private void FixedUpdate() {
        
      
        pre_position= playerCamera.InverseTransformPoint(Detect_point.transform.position);
     
    }
}
