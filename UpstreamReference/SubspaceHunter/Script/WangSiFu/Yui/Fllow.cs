/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 跟随与语音助手原型 / Follow and voice-assistant prototype
 * 功能 / Purpose: 控制跟随对象或早期语音助手交互原型。
 * English: Controls follower objects or early voice-assistant interaction prototypes.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fllow : MonoBehaviour
{
    public Transform playerTransform; 
    public float followHeight = 2.0f; 
    public float distance = 3.0f; 

    // 飞行速度
    public float flySpeed = 5f;
    public bool is_arrive_height=false;
    
    private void Start() {
        playerTransform = GameObject.FindWithTag("Yui_point").GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        // 计算宠物与玩家之间的向量，并将其标准化。
        Vector3 direction = playerTransform.position - transform.position;
        direction.Normalize();

        // 移动宠物到指定高度
       float targetY = playerTransform.position.y + followHeight;
        if (transform.position.y != targetY&&false==is_arrive_height) {
            Vector3 targetPosition = new Vector3(transform.position.x, 
                                                 Mathf.MoveTowards(transform.position.y, targetY, flySpeed * Time.deltaTime), 
                                                 transform.position.z);
            transform.position = targetPosition;
        }else
          is_arrive_height=true;

        // 计算与玩家之间的距离
        float distanceToPlayer = Vector3.Distance(transform.position,new Vector3( playerTransform.position.x,transform.position.y, playerTransform.position.z)  );

        // 当物体在distance范围外时才进行移动
        if (distanceToPlayer > distance) {
            // 将宠物位置放置到合理距离内
            Vector3 targetPos = playerTransform.position - direction * distance;
            transform.position = Vector3.Lerp(transform.position, targetPos, flySpeed * Time.deltaTime);

            // 计算目标点与物体之间的方向向量
             Vector3 direction2 = new Vector3( playerTransform.position.x,transform.position.y, playerTransform.position.z) - transform.position;

              // 使用Quaternion.LookRotation方法计算面向目标点所需的旋转
              Quaternion targetRotation = Quaternion.LookRotation(direction2);

              // 为物体应用所需的旋转
                transform.rotation = targetRotation;
            // 仅在移动的过程中使用lookat
           // transform.LookAt(playerTransform);
        }

    }
}