/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 敌人系统 / Enemy system
 * 功能 / Purpose: 维护敌人生成、生命值、受击反馈、死亡结算、技能特效和战斗状态。
 * English: Maintains enemy spawning, HP, hit feedback, death settlement, skill effects, and combat state.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Start2_randomZom : MonoBehaviour
{
    [SerializeField]
    Gernerate_StaticMesh gernerate_StaticMesh;

    public GameObject[]  Player_Meshs;
    // Start is called before the first frame update
    void Start()
    {
        gernerate_StaticMesh=GetComponent<Gernerate_StaticMesh>();
        Random_character();
    }

    private void Random_character()
    {
        if(Player_Meshs.Length==0) return;
          int index=Random.Range(0,Player_Meshs.Length);
          gernerate_StaticMesh.skinnedMesh.transform.gameObject.SetActive(false);
          Player_Meshs[index].SetActive(true);
          gernerate_StaticMesh.skinnedMesh=Player_Meshs[index].GetComponent<SkinnedMeshRenderer>();
    }

}
