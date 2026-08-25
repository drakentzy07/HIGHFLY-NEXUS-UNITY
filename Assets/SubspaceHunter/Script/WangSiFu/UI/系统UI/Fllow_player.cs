/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 系统 UI 跟随 / System UI follow
 * 功能 / Purpose: 让系统 UI 面板跟随玩家或摄像机位置。
 * English: Makes system UI panels follow the player or camera position.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Fllow_player : MonoBehaviour
{
    // 目标物体的 Transform
    private Transform targetTransform;
    
    // 物体与目标物体的初始相对位置
    private Vector3 initialOffset;
    private bool can_follow;

    // 平滑跟随的速度
    //public float followSpeed = 5f;

    // 距离阈值，只有在超过这个距离之后才开始跟随
    public float followThreshold = 1f;

    // 当前物体的位置
    private Vector3 currentVelocity = Vector3.zero;
    private bool is_closing;

    void Start()
    {
        // 计算初始相对位置，包括 Y 轴上的高度差
         targetTransform =  GameObject.FindWithTag("MainCamera").transform;
        initialOffset = transform.position - targetTransform.position;
        can_follow=true;
       
    }

    public void close_follow()
    {
         can_follow=false;
    }

   
    void FixedUpdate()
    {
             float distance = Vector3.Distance(transform.position, targetTransform.position);
             if(distance >followThreshold) 
             {
                Deal_distanc_close();
             }
    }

    private void Deal_distanc_close()
    {
        if(is_closing) return;
            is_closing=true;
            Player_UI_Controller player_UI_Controller = GameObject.FindWithTag("Hand_and_Controller").GetComponent<Player_UI_Controller>();
            player_UI_Controller.关闭UI();
    }

   
   /* void FixedUpdate()
    {
            if(can_follow)
            {
                     // 计算目标物体与物体之间的距离
                    float distance = Vector3.Distance(transform.position, targetTransform.position);
                    if (distance > followThreshold)
                    {
                        // 计算目标位置
                        Vector3 targetPosition = targetTransform.position + initialOffset;

                        // 使用 SmoothDamp 平滑地移动物体
                        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, followSpeed * Time.deltaTime);

                        // 使物体朝向目标物体
                        transform.LookAt(targetTransform);
                    }

                   // 计算新的目标位置
                  //  Vector3 targetPosition = targetTransform.position + initialOffset;

                    // 更新物体的位置
                  //  transform.position = targetPosition;
                    
                    // 使物体朝向目标物体
                   // transform.LookAt(targetTransform);
            }
           
    }*/

    
}
