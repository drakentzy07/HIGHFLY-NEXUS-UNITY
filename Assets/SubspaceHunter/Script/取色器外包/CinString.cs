using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CinString : MonoBehaviour
{
    [TextArea(3, 10)] // 允许在Inspector中输入多行文本
    public string sourceText;  // 源文本字符串
    public TextMeshProUGUI targetText;  // 目标 TextMeshProUGUI 组件
    public float delay = 0.1f;  // 每个字符之间的延迟时间
    public int testValue = 12; //测试变量

   /* void Start()
     {
         if(targetText==null) targetText = GetComponent<TextMeshProUGUI>();
         StartCoroutine(AccumulateText());
     }*/

    private void OnEnable()
    {
        if (targetText == null) targetText = GetComponent<TextMeshProUGUI>();
        StartCoroutine(AccumulateText());
    }

    IEnumerator AccumulateText()
    {
        targetText.text = " ";  // 确保目标文本开始时为空

        foreach (char c in sourceText)
        {
            targetText.text += c;  // 将字符逐个累加到目标文本中
            yield return new WaitForSeconds(delay);  // 等待一段时间
        }
    }
}
