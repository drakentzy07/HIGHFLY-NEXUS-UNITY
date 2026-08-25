/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: UI 打开关闭与渐隐 / UI open/close and fade
 * 功能 / Purpose: 统一控制 UI 面板开关、显隐动画和渐隐状态。
 * English: Centralizes UI panel open/close, visibility animation, and fade state.
 */

using System.Collections;
using System.Collections.Generic;
//using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class UI_hiden : MonoBehaviour
{

    public Material material;
    public float duration_out = 1.0f; // 持续时间为 1 秒
    public float duration_int = 0.3f;
     // 调用该函数使透明度从100降至0
     public bool is_activate=true;
    
    private void Start() {
         // 确保完全不透明
          Color color = material.GetColor("_Color");
        color.a = 1;
        material.SetColor("_Color", color);
        is_activate = true;
    }

    public void FadeOut()
    {
        if (material != null)
        {
             is_activate=false;
            StartCoroutine(FadeOutCoroutine());
        }
    }
     // 协程函数，逐步减少透明度
    private IEnumerator FadeOutCoroutine()
    {
        Color color = material.GetColor("_Color");
        float startAlpha = 1.0f; // Alpha值为1表示100%不透明
        float time = 0;
       

        while (time < duration_out)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0, time / duration_out); // 线性插值Alpha值
            color.a = alpha;
            material.SetColor("_Color", color); // 设置颜色
            yield return null;
        }

        // 确保完全透明
        color.a = 0;
        material.SetColor("_Color", color);
        
    }

      // 调用该函数使透明度从0升至100
    public void FadeIn()
    {
        if (material != null)
        {
            is_activate=true;
            StartCoroutine(FadeInCoroutine());
        }
    }

    // 协程函数，逐步增加透明度
    private IEnumerator FadeInCoroutine()
    {
        Color color = material.GetColor("_Color");
        float startAlpha = 0.0f; // Alpha值为0表示完全透明
        float time = 0;

        while (time < duration_int)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 1, time / duration_int); // 线性插值Alpha值
            color.a = alpha;
            material.SetColor("_Color", color); // 设置颜色
            yield return null;
        }

        // 确保完全不透明
        color.a = 1;
        material.SetColor("_Color", color);
        
    }
}
