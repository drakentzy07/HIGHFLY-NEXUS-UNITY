using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Time_record : MonoBehaviour
{
    public TextMeshProUGUI timerText; // UI Text组件的引用
    private float elapsedTime = 0f; // 记录总的经过时间
    private bool isPaused = true; // 标记计时器是否暂停
    public string timerString;

    void Update()
    {
        if (!isPaused)
        {
            elapsedTime += Time.deltaTime;

            // 计算分钟、秒和毫秒
            int minutes = (int)(elapsedTime / 60) % 60;
            int seconds = (int)(elapsedTime % 60);
            int milliseconds = (int)((elapsedTime * 10000) % 10000);

            // 格式化计时器字符串
             timerString = string.Format("{0:00}:{1:00}:{2:0000}", minutes, seconds, milliseconds);

            // 显示计时器字符串
            timerText.text = timerString;
        }
    }

    // 暂停计时器
    public void PauseTimer()
    {
        isPaused = true;
    }

    // 重新开始计时器
    public void ResumeTimer()
    {
        isPaused = false;
    }

    // 重置计时器
    public void ResetTimer()
    {
        elapsedTime = 0f;
        timerText.text = "00:00:0000";
    }
}
