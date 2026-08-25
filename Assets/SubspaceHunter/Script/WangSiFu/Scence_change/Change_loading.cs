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
using UnityEngine.Rendering;

public class Change_loading : MonoBehaviour
{
    public GameObject scene1, scene2;
    public AudioClip end;
    public AudioClip start;


    private List<BoundBox> boundList = new List<BoundBox>();

    private Material MaskMat;

    private MeshRenderer render;

    private SphereCollider col;

    private float transparent = 1;

    private float _scaleRaduis = 30f;
    [Range(0f, 1f)]
    public float Lerp = 0f;


    private bool playEnd=true;

   /* private void OnEnable()
    {
        render = transform.GetComponent<MeshRenderer>();

        MaskMat = render.sharedMaterial;

        col = transform.GetComponent<SphereCollider>();

    }*/

    private void Start() {
        render = transform.GetComponent<MeshRenderer>();

        MaskMat = render.sharedMaterial;

        col = transform.GetComponent<SphereCollider>();
        ShowNextScene();
    }

     private void ShowNextScene()
    {
        playEnd = false;
        GetComponent<AudioSource>().clip=end;
        GetComponent<AudioSource>().Play();

        col.radius = 0f;
        Lerp = 1f;

        //缩放大小 时间 自行调整
        DOTween.To(() => col.radius, x => col.radius = x, 0.5f, 5f);
    
        DOTween.To(() => Lerp, x => Lerp = x, .15f, 2f).OnComplete(() =>
        {

            transform.DOScale(0, 4f);
            DOTween.To(() => transparent, x => transparent = x, 0f, 2).OnComplete(() =>
            {
                playEnd = true;
            });

        }).SetDelay(1f);


    }

    private void OnTriggerEnter(Collider other)
    {

        //Debug.Log(other.name);

        //根据情况剔除

        if (other.tag == "Player")
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

        //根据情况剔除
        if (other.tag == "Player")
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


    private void Update() {
            SetRenderState();
    }


    private void SetRenderState()
    {
        MaskMat.SetFloat("_Lerp", Lerp);
        MaskMat.SetFloat("_Transparent", transparent);

        if (Vector3.Distance(Camera.main.transform.position, transform.position) < transform.lossyScale.x / 2 + 0.5f)
        {
            MaskMat.SetInt("_zt", (int)UnityEngine.Rendering.CompareFunction.Always);
            MaskMat.SetInt("_zw", 1);
        }

        else
        {
            MaskMat.SetInt("_zt", (int)UnityEngine.Rendering.CompareFunction.Less);
            MaskMat.SetInt("_zw", 1);
        }


    }


}
