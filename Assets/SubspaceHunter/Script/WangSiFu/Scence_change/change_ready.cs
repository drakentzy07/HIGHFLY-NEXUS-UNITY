/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 场景切换 / Scene transition
 * 功能 / Purpose: 封装加载界面、按钮跳转和进入游戏流程。
 * English: Wraps loading screens, button navigation, and game entry flow.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
//using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;




using UnityEngine.Rendering;

public class change_ready : MonoBehaviour
{
  // public GameObject scene1, scene2;
   

   // public PostProcessingBehaviour post;
    [Header("仅做测试用")]
    public string Next_scenceName;
    public GameObject Player_visionUI;
    public Bgm_manager bgm_Manager;

    public GameObject init_tutor;
    private List<BoundBox> boundList = new List<BoundBox>();

    private Material MaskMat;

    private MeshRenderer render;
    private float dis;

    private SphereCollider col;

    private float transparent = 1;

    public float _scaleRaduis = 30f;
    [Range(0f, 1f)]
    public float Lerp = 0f;
    public float transition_time=4f;


    private bool playEnd=true;

    

  

    private void OnEnable()
    {
        render = transform.GetComponent<MeshRenderer>();

        MaskMat = render.sharedMaterial;

        col = transform.GetComponent<SphereCollider>();
        ShowNextScene();
    }

    public void HideCurrentScene()
    {
        // Reset
        bgm_Manager.Play_End();
 
      
        playEnd = false;
        col.radius = 0.5f;
        Lerp = 0.15f;
        transparent = 0;
        if (boundList != null && boundList.Count > 0)
        {

            foreach (var bb in boundList)
            {
                bb.lineColor = new Color(1, 1, 1, 0);
            }

        }

        DOTween.To(() => transparent, x => transparent = x, 1f, 1f);

        //缩放大小 时间 自行调
        transform.DOScale(_scaleRaduis, transition_time).

      OnComplete(() =>
       {
           DOTween.To(() => Lerp, x => Lerp = x, 1f, 1.5f);
           DOTween.To(() => col.radius, x => col.radius = x, 0, 2f).OnComplete(()=> {
               playEnd = true;
          //     if(post) post.enabled=false;
               change_scene();
           });
       }
         )
         .SetEase(Ease.Linear);


    }


     private void ShowNextScene()
    {
        playEnd = false;
        

        col.radius = 0f;
        Lerp = 1f;

        //缩放大小 时间 自行调整
        DOTween.To(() => col.radius, x => col.radius = x, 0.5f, 5f);
    
        DOTween.To(() => Lerp, x => Lerp = x, .15f, 2f).OnComplete(() =>
        {

            transform.DOScale(0, transition_time);
            DOTween.To(() => transparent, x => transparent = x, 0f, 2).OnComplete(() =>
            {
                playEnd = true;
              //  if(post) post.enabled=true;
                if(Player_visionUI) Player_visionUI.SetActive(true);
                      if(init_tutor) init_tutor.SetActive(true);               
            });

        }).SetDelay(1f);


    }

    private void change_scene()
    {
        SceneManager.LoadScene(Next_scenceName);
    }

   


    private void Update()
    {
        SetRenderState();
        /***************测试代码************************/
        if (Input.GetKeyDown(KeyCode.E)&&playEnd)
        {
          // audio_s.clip=start;
           // audio_s.Play();
            HideCurrentScene();

            Debug.Log("按右键切换下个场景 ");

        } else if(Input.GetKeyDown(KeyCode.R)&& playEnd)
        {
                ShowNextScene();
        }   

       


        ///////////////////// 测试代码End////////////////////////////////


       
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag!="ignore_box_shader")
          return;
          
         if(!other.transform.gameObject.GetComponent<BoxCollider>())
          return;

        //Debug.Log(other.name);

        //根据情况剔除

        if (other.tag == "Player")
            return;

        if(other.gameObject.layer==11)
        return;

        

        if (other.gameObject.layer == LayerMask.NameToLayer("PostProcessing"))
            return;
        if (other.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast"))
            return;
     

        BoundBox bb = other.transform.GetComponent<BoundBox>();
        if (bb == null)
        {
           
               
            
            bb = other.transform.gameObject.AddComponent<BoundBox>();
            if (!boundList.Contains(bb))
                boundList.Add(bb);

        }

        bb.line_renderer = true;
        bb.lineColor = new Color(1, 1, 1, 0);

        DOTween.ToAlpha(() => bb.lineColor, x => bb.lineColor = x, 1f, 1.5f).OnUpdate(() => bb.SetLineRenderers());
  

    }

    private void OnTriggerExit(Collider other)
    {

        if(other.tag=="ignore_box_shader")
          return;

          if(!other.transform.gameObject.GetComponent<BoxCollider>())
          return;
        //根据情况剔除
        if (other.tag == "Player")
            return;
        if(other.gameObject.layer==11)
        return;

        if (other.gameObject.layer == LayerMask.NameToLayer("PostProcessing"))
            return;
        if (other.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast"))
            return;
       
            

        BoundBox bb = other.transform.GetComponent<BoundBox>();
        if (bb != null)
        {

            DOTween.ToAlpha(() => bb.lineColor, x => bb.lineColor = x, 0f, .8f).OnUpdate(() => bb.SetLineRenderers());

        }


    }

    private void SetRenderState()
    {
         MaskMat.SetFloat("_Lerp", Lerp);
        MaskMat.SetFloat("_Transparent", transparent);






       dis = Vector3.Distance(Camera.main.transform.position, transform.position);


      if (dis < transform.lossyScale.x / 2 +1f)
        {
            MaskMat.SetInt("_zt", (int)UnityEngine.Rendering.CompareFunction.Always);
            MaskMat.SetInt("_zw", 1);
            MaskMat.SetFloat("_IsInSphere", 1f);
            MaskMat.SetInt("_cull", 1);
        //;
        }

        else
        {
             MaskMat.SetInt("_cull",2);
            MaskMat.SetInt("_zt", (int)UnityEngine.Rendering.CompareFunction.Less);
            MaskMat.SetInt("_zw", 1);
            MaskMat.SetFloat("_IsInSphere", 1f);
        }



    }
}
