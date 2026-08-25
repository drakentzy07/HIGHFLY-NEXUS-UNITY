using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkinnedDecals;

public class Camera_rayDecal : MonoBehaviour
{
    public Transform eye_point;
    public SkinnedDecal decal;
    public LayerMask layer;  

    public SkinnedDecalSystem decal_system;

   public void Activate_Ray2_Decal()
   {
        Ray ray = new Ray(eye_point.position, eye_point.forward);
        // Ray ray = new Ray(Camera.main.transform.position, Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit,1000f,layer))
            {
                print("射中物体");
                Debug.Log(hit.collider.gameObject.name);
                Debug.Log("当前标签是"+hit.collider.gameObject.tag);
                
                    SkinnedDecalSystem skinnedDecalSystem=hit.collider.transform.GetComponent<SkinnedDecalSystem>();
                    if(null!=skinnedDecalSystem)
                    {
                        skinnedDecalSystem.CreateDecal(decal,eye_point.position,ray.direction);
                    }
                            
                
            }
   }

   public void Make_decal_NoRay()
   {
    print("执行 make no ray");
        Ray ray = new Ray(eye_point.position, eye_point.forward);
        decal_system.CreateDecal(decal, eye_point.position,ray.direction);
   }

   private void Update() {
       if(Input.GetKeyDown(KeyCode.T))
       {
            if(null==decal_system)
            Activate_Ray2_Decal();
            else
            Make_decal_NoRay();
       }
   }
}
