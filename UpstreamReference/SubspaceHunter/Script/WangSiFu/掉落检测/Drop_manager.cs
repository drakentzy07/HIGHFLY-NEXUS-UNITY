/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 掉落检测 / Drop detection
 * 功能 / Purpose: 检测物体掉落、执行回收或触发掉落管理逻辑。
 * English: Detects dropped objects, performs recovery, or triggers drop-management logic.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 定义掉落物品的记录类
[System.Serializable]
public class DropItem
{
    public GameObject item; // 掉落的物品
    [Range(0, 1)]
    public float probability; // 生成概率
}

public class Drop_manager : MonoBehaviour
{
     
     GameObject[] Ger_objs; // 存储生成的物品
    public GameObject Drop_exc;
    public List<DropItem> dropItems; // 存储多个掉落记录

    public void GenerateDrops()
    {
        print("掉落检测1");
        List<GameObject> generatedItems = new List<GameObject>(); // 临时存储生成的物品
         print("掉落检测2");
        foreach (DropItem dropItem in dropItems)
        {
             print("掉落循环");
            // 生成一个随机数
            float randomValue = Random.Range(0f, 1f);
            
            // 根据概率判断是否生成物品
            if (randomValue <= dropItem.probability)
            {
                print("概率生成");
                generatedItems.Add(dropItem.item); // 添加到生成物品列表
            }
        }

        // 将生成物品列表转换为数组
        Ger_objs = generatedItems.ToArray();
        print("掉落检测4");
        GameObject res = Instantiate(Drop_exc,transform.position,transform.rotation);
        res.transform.position = GetComponent<Enemy_statusController>().defualt_sliceGo.transform.position;
        print("掉落检测5");
        res.GetComponent<Drop_excute>().Set_objs(Ger_objs);
        res.GetComponent<Drop_excute>().ExplodeAndDropItems();
    }
}
