/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 掉落检测 / Drop detection
 * 功能 / Purpose: 检测物体掉落、执行回收或触发掉落管理逻辑。
 * English: Detects dropped objects, performs recovery, or triggers drop-management logic.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drop_item : MonoBehaviour
{
     public float attractionSpeed = 5f; // 吸向玩家的速度
    public float pickupDistance = 1f; // 吸附拾取的距离
    private Transform playerTransform; // 玩家位置
    private bool isAttracting = false; // 是否正在吸附
    public AudioClip pickupSound; // 拾取音效
    public GameObject[] Disable_set;
    private Rigidbody rb; // Rigidbody 组件
    public float Dispear_time=1f;

    private void Start() {
        // 获取 Rigidbody 组件
        rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other) {
         if (other.CompareTag("Player_body"))
        {
            // 找到玩家
            playerTransform = Camera.main.transform;
            isAttracting = true; // 开始吸附
            BoxCollider[] boxColliders = GetComponents<BoxCollider>();
            foreach (var item in boxColliders)
            {
                item.enabled=false;
            }
             GetComponent<Rigidbody>().useGravity = false;
        }
    }

    private void Close_objs()
    {
        foreach (var item in Disable_set)
        {
            item.SetActive(false);
        }
    }

    private void FixedUpdate() {
        
    }

    void Update()
    {
       if (isAttracting && playerTransform != null)
        {
            // 计算物体与玩家的距离（包括高低差）
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            AudioSource audioSource = GetComponent<AudioSource>();
            // 当物体距离玩家小于指定距离时
            if (distance < pickupDistance)
            {
                // 播放拾取音效
                if (pickupSound != null && !audioSource.isPlaying)
                {
                    audioSource.PlayOneShot(pickupSound);
                    Destroy(gameObject, pickupSound.length);
                }
                else
                {
                    Destroy(gameObject, Dispear_time);
                }
                
                Close_objs();
                // 销毁物体，延迟以播放完音效
                
            }
            else
            {
                // 物体逐渐向玩家三维靠近，考虑高低落差，通过 Rigidbody 移动
                Vector3 newPosition = Vector3.MoveTowards(rb.position, playerTransform.position, attractionSpeed * Time.fixedDeltaTime);
                rb.MovePosition(newPosition);
            }
        }
    }
}
