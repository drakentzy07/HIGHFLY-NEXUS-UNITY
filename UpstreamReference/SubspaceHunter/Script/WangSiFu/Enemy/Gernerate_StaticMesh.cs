/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 敌人系统 / Enemy system
 * 功能 / Purpose: 维护敌人生成、生命值、受击反馈、死亡结算、技能特效和战斗状态。
 * English: Maintains enemy spawning, HP, hit feedback, death settlement, skill effects, and combat state.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gernerate_StaticMesh : MonoBehaviour
{
   public SkinnedMeshRenderer skinnedMesh;

   
    Mesh tempMesh;
    void Start()
    {
      //  Create_staticMesh();
    }

    public GameObject Create_staticMesh()
    {
       /* Mesh staticMesh=new Mesh();
        skinnedMesh.BakeMesh(staticMesh);

        GameObject New_gameObject=new GameObject();
        New_gameObject.transform.position=skinnedMesh.transform.position;
        New_gameObject.transform.rotation=skinnedMesh.transform.rotation;
        New_gameObject.AddComponent<MeshFilter>().sharedMesh=staticMesh;
        New_gameObject.AddComponent<MeshRenderer>().sharedMaterials=skinnedMesh.sharedMaterials;
        return New_gameObject;*/

      //   skinnedMeshRenders = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
        // meshList = new Mesh[skinnedMeshRenders.Length];
     //   Debug.Log(skinnedMeshRenders.Length);
        GameObject go= BakeMeshRenderer(skinnedMesh, transform);

        if(go==null)
         print("Create_staticMesh为空");
      
        return go;
    }

     
    



    //同步位置
    public void CopyTransform(Transform self,Transform target,bool AsChild)
    {
      
    //  New_gameObject.transform.position=skinnedMesh.transform.position;
     //   New_gameObject.transform.rotation=skinnedMesh.transform.rotation;

        self.transform.position=skinnedMesh.transform.position;
        self.transform.rotation=skinnedMesh.transform.rotation;
     //   Transform orignParent = self.parent;
     //   self.transform.SetParent(target.transform);
     //   self.transform.localPosition = Vector3.zero;
   //    self.transform.localEulerAngles = Vector3.zero;
        self.transform.localScale = Vector3.one;

     //   if(!AsChild)
          //  self.SetParent(orignParent);

    }

  
    private GameObject BakeMeshRenderer(SkinnedMeshRenderer render,Transform parent)
    {
        GameObject go = null;

        render.transform.localEulerAngles = Vector3.zero;
        Mesh msh = new Mesh();
        render.BakeMesh(msh);

        go = new GameObject(render.name + "_Instance");
        go.transform.position=skinnedMesh.transform.position;
        go.transform.rotation=skinnedMesh.transform.rotation;
         go.transform.localScale = Vector3.one;

       // CopyTransform(go.transform,parent, false);

       // go.transform.SetParent(parent);



        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterials = render.sharedMaterials;

        go.AddComponent<MeshFilter>().sharedMesh = msh;


        render.gameObject.SetActive(false);
        
        if(go==null)
         print("BakeMeshRenderer为空");
        return go;

    }


    //合并了所有skinnedmesh以及submesh  多材质id会有问题 仅用于整体模型材质相同情况
    private GameObject CombineSkinnedMeshWithOneMat(SkinnedMeshRenderer[] skinRenders,Transform parent)
    {

        List<Mesh> allMesh = new List<Mesh>();
        Mesh msh_submesh;
        List<Vector2[]> uvList = new List<Vector2[]>();
        CombineInstance[] submeshCombines;
        for (int i = 0; i < skinRenders.Length; i++)
        {

            skinRenders[i].transform.localEulerAngles = Vector3.zero;

            msh_submesh = new Mesh();
            skinRenders[i].BakeMesh(msh_submesh);
            int _vertexCount = 0;
            //合并submesh
            submeshCombines = new CombineInstance[msh_submesh.subMeshCount];
            for (int j = 0; j < skinRenders[i].sharedMesh.subMeshCount; j++)
            {

                submeshCombines[j].mesh = msh_submesh;
                submeshCombines[j].transform = skinRenders[i].localToWorldMatrix;
                submeshCombines[j].subMeshIndex = j;
              
            }

            Mesh submesh = new Mesh();
            submesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            submesh.CombineMeshes(submeshCombines, true, false);
            allMesh.Add(submesh);
            


            //    skinRenders[i].enabled = false;
        }

        CombineInstance[] combines = new CombineInstance[allMesh.Count];
        Mesh combineMesh = new Mesh();
        int vertexCount = 0;
        for (int i = 0; i < allMesh.Count; i++)
        {
            vertexCount += allMesh[i].vertexCount;
            combines[i].mesh = allMesh[i];
        }
        if (vertexCount > int.MaxValue)
            combineMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        combineMesh.CombineMeshes(combines, true, false);

        GameObject go = new GameObject();

         CopyTransform(go.transform, transform, true);
         //go.transform.SetParent(transform);
         //go.transform.localPosition = Vector3.zero;
         //go.transform.localEulerAngles = Vector3.zero;
         //go.transform.localScale = Vector3.one;

         go.AddComponent<MeshFilter>().mesh = combineMesh;

         MeshRenderer mr = go.AddComponent<MeshRenderer>();

         return go;

    }
}
