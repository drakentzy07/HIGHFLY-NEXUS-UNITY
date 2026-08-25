/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 物理武器系统 / Physical weapon system
 * 功能 / Purpose: 管理抓取、释放、刚体添加、盾牌阻挡、武器碰撞和轨迹检测。
 * English: Manages grabbing, releasing, Rigidbody setup, shield blocking, weapon collision, and trace detection.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun_handTrigger : MonoBehaviour
{
    public Gun_Controller gun_Controller;

    private bool interval_shoot;

    public bool is_keepShoot;

    private void OnTriggerEnter(Collider other) {
       if(is_keepShoot) return;
       
        if(other.tag=="Gun_handTrigger"&&interval_shoot==false)
        {
            interval_shoot=true;
            gun_Controller.点射();
        }
    }

    private void OnTriggerStay(Collider other) {
        if(other.tag=="Gun_handTrigger"&&is_keepShoot)
        {
           
            gun_Controller.点射();
        }
    }

    private void OnTriggerExit(Collider other) {
       if(is_keepShoot) return;
         if(other.tag=="Gun_handTrigger")
        {
            interval_shoot=false;
            
        }
    }
}
