/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 物体切割 / Object slicing
 * 功能 / Purpose: 调用切割逻辑处理可切割物体的分裂和反馈。
 * English: Invokes slicing logic for splitting cuttable objects and feedback.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;

public class Slice_Item : MonoBehaviour
{
    public GameObject 切割物体;
    public GameObject 默认切割位置;
    private GameObject 切割点;
    public Material sectionMat;//切面材质
    SlicedHull hull;
    public float hull_dispearTime=2f;
 

    private void Update() {
        if(Input.GetKeyDown(KeyCode.P))
        {
                Deal_slice();
        }
    }


    private void OnCollisionEnter(Collision other) {
        if(other.transform.tag=="Weapon_inHand"||other.transform.tag=="Weapon_inRightHand")
        {
            set_point(other.gameObject);
            Deal_slice();
        }
            
    }

    public void set_point(GameObject point)
    {
        切割点 = point;
    }
   
    public void Deal_slice()
    {
        if(切割点==null) 
        {
            print("切割点为空");
            切割点 = 默认切割位置;
        }
         hull = 切割物体.Slice(切割点.transform.position, 切割点.transform.right);
                if(切割物体==null)
              {
                 print("切割物体为空");
              }


             if(hull==null)
              {
                 print("hull为空");
                 hull = 切割物体.Slice(切割点.transform.position, 切割点.transform.right);
              }
              
              GameObject upper = hull.CreateUpperHull(切割物体, sectionMat);
              GameObject lower = hull.CreateLowerHull(切割物体, sectionMat);

            //  if(is_bullet)
           //   {
                    
            //  }
             
              
               upper.AddComponent<MeshCollider>().convex=true;
               upper.AddComponent<Rigidbody>().AddExplosionForce( 100f, upper.transform.position,2f);
                     

                      //  upper.layer=8;
                        
                lower.AddComponent<MeshCollider>().convex=true;
                lower.AddComponent<Rigidbody>().AddExplosionForce( 100f, lower.transform.position,2f);

                transform.gameObject.SetActive(false);
                Destroy(upper,hull_dispearTime);
                Destroy(lower,hull_dispearTime);
    }

}
