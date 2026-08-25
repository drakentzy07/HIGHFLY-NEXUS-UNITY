/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 武器与道具收纳 / Weapon and item holster
 * 功能 / Purpose: 管理武器跟随、插槽检测和道具收纳交互。
 * English: Manages weapon following, slot detection, and item holster interactions.
 */

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using OVR.OpenVR;
using UnityEngine;


namespace RootMotion.Dynamics
{

public class Weapon_follow : MonoBehaviour
{
        public AudioSource 物品收纳出;
        public AudioSource 物品收纳入;
        public GameObject 换手检测;
        public GameObject 收纳检测;

         Transform target;
        [Range(0f, 1f)] public float forceWeight = 1f;
        [Range(0f, 1f)] public float torqueWeight = 1f;
        public bool useTargetVelocity = true;

        private Rigidbody r;
        private Vector3 lastTargetPos;
        private Quaternion lastTargetRot = Quaternion.identity;
         public bool is_following;
        //这里是保险起见，如果没停止自己就主动停止.
        public Destory_self destory_Self;

        private Item_holster Cur_holster;

        //private bool is_containItem;

       /* public bool get_isContainItem()
        {
            return is_containItem;
        }*/

   
        public Item_holster get_cur_holster()
        {
             return  Cur_holster;
        }
        

        /// <summary>
        /// Call this after target has been teleported
        /// </summary>
         
        public bool get_holsterStatus()
        {
          
            return is_following;
        }

        public void  set_holster(Transform followTarget,Transform holster)
        {
            print("检测武器进入5");
            if(is_following) return;
            is_following=true;
           // is_containItem=true;
            target = followTarget;
            print("检测武器进入6");
            Cur_holster = holster.GetComponent<Item_holster>();
            lastTargetPos = target.position;
            lastTargetRot = target.rotation;
            print("检测武器进入7");
            收纳检测.SetActive(true);
            if(物品收纳入!=null)
            物品收纳入.Play();
            print("检测武器进入8");
            destory_Self.stop_2waitDestroy();
            is_following=true;
        }

        public void stop_following()
        {
            if(!is_following) return;
            is_following=false;
          //  is_containItem=false;
            Cur_holster.Reset_holster();
             if(物品收纳出!=null)
            物品收纳出.Play();
        }

        private void Start()
        {
            r = GetComponent<Rigidbody>();
           // OnTargetTeleported();
        }

        private void FixedUpdate()
        {
            if(!is_following) return;
            Vector3 targetVelocity = Vector3.zero;
            Vector3 targetAngularVelocity = Vector3.zero;

            // Calculate target velocity and angular velocity
            if (useTargetVelocity)
            {
                targetVelocity = (target.position - lastTargetPos) / Time.deltaTime;

                targetAngularVelocity = PhysXTools.GetAngularVelocity(lastTargetRot, target.rotation, Time.deltaTime);
            }

            lastTargetPos = target.position;
            lastTargetRot = target.rotation;

            // Force
            Vector3 force = PhysXTools.GetLinearAcceleration(r.position, target.position);
            force += targetVelocity;
            force -= r.velocity;
            if (r.useGravity) force -= Physics.gravity * Time.deltaTime;
            force *= forceWeight;
            r.AddForce(force, ForceMode.VelocityChange);

            // Torque
            Vector3 torque = PhysXTools.GetAngularAcceleration(r.rotation, target.rotation);
            torque += targetAngularVelocity;
            torque -= r.angularVelocity;
            torque *= torqueWeight;
            r.AddTorque(torque, ForceMode.VelocityChange);
        }
}

}
