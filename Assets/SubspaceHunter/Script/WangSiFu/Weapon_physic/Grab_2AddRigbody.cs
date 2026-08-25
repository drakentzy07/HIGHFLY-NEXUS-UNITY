/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 物理武器系统 / Physical weapon system
 * 功能 / Purpose: 管理抓取、释放、刚体添加、盾牌阻挡、武器碰撞和轨迹检测。
 * English: Manages grabbing, releasing, Rigidbody setup, shield blocking, weapon collision, and trace detection.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grab_2AddRigbody : MonoBehaviour
{
   public void add_rigbody()
   {
       Rigidbody rb=transform.gameObject.AddComponent<Rigidbody>();
       // 设置使用Gravity
        rb.useGravity = true;

        // 设置Collision Detection类型为Continuous Dynamic
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

   }
}
