using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Get_baidu : MonoBehaviour
{
   
    // Start is called before the first frame update
    void Start()
    {
       
    }

    public void Start_talk()
    {
         BaiduRecognition baiduRecognition=GameObject.FindWithTag("baidu_input").GetComponent<BaiduRecognition>();
        if(null!=baiduRecognition)
        baiduRecognition.Start_talk();
    }

    public void End_talk()
    {
         BaiduRecognition baiduRecognition=GameObject.FindWithTag("baidu_input").GetComponent<BaiduRecognition>();
         if(null!=baiduRecognition)
        baiduRecognition.End_talk();
    }

   
}
