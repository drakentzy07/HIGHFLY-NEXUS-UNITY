/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 公开 Demo 场景桥接脚本 / Public demo scene bridge script
 * 功能 / Purpose: 用于在公开演示场景中连接按钮、视频、语音展示和 AR/VR 场景对象激活逻辑。
 * English: Connects buttons, video, voice display, and AR/VR scene object activation logic in public demo scenes.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Oculus.Voice;

public class Voice_Display : MonoBehaviour
{
   
    private float[] samples;

    public RectTransform _uiParentRect;

    public GameObject _prefab;

    [Range(12, 128 * 2)]
    public int BarCount = 16;

    public float BarDistance=4;

    [Range(1, 30)]
    public float UpLerp = 12;


    public float maxHeight = 4f;

    public float sensitive = 250f;


    private List<Image> uiList = new List<Image>();



  
    private int interval=1;




    public AppVoiceExperience appVoice;


    
    private void Start()
    {
        appVoice.events.OnByteDataReady.AddListener(this.OnDataReady) ;
        appVoice.events.OnStoppedListening.AddListener(this.OnStopListening);
        CreatUI();

    }

    private void OnDestroy()
    {
        appVoice.events.OnByteDataReady.RemoveListener(this.OnDataReady);
        appVoice.events.OnStoppedListening.RemoveListener(this.OnStopListening);
    }
    private void CreatUI()
    {
        for (int i = 0; i < BarCount; i++)
        {
            GameObject _prefab_GO = Instantiate(_prefab, _uiParentRect.transform);
         
            _prefab_GO.name = string.Format("Sample[{0}]", i + 1);
            uiList.Add(_prefab_GO.GetComponent<Image>());
            RectTransform _rectTransform = _prefab_GO.GetComponent<RectTransform>();
            Vector3 _v3 = _rectTransform.localScale;
            _v3.y = 0f;
            _rectTransform.localScale = _v3;
            _rectTransform.localPosition = new Vector3(_rectTransform.sizeDelta.x + BarDistance * i, 0, 0);
        }
    }


    void Update()
    {
        if (samples == null || samples.Length <= 0)
            return;
        for (int i = 0; i < uiList.Count; i++)
        {
            int sampleindex = Mathf.Clamp(i * interval,0, samples.Length-1) ;
            Vector3 _v3 = uiList[i].transform.localScale;
            _v3 = new Vector3(1, Mathf.Clamp(samples[sampleindex] * (maxHeight+ i* 0.1f* sensitive), 0, maxHeight), 1);
            uiList[i].transform.localScale = Vector3.Lerp(uiList[i].transform.localScale, _v3, Time.deltaTime * UpLerp);
        }


    }

    private void OnDataReady(byte[] data ,int offset, int length)
    {
        interval = Mathf.Max( (length / BarCount),1);
       samples = UnpackData(data,length); 

    }

    private void OnStopListening()
    {
        samples = null;
        for (int i = 0; i < uiList.Count; i++)
        {
            uiList[i].transform.localScale = Vector3.zero;
        }
       
      
    }


private float[] UnpackData(byte[]data,int length)
    {

        var sampleLength = length / 2;

        float[] samples = new float[sampleLength];

        int rescaleFactor = 32767;


        for (int i = 0; i < sampleLength; i++)
        {
            byte b1 = data[2 * i];
            byte b2 = data[2 * i + 1];
            short result = (short)(b1 + (b2 << 8));
            samples[i] = result / (float)rescaleFactor;

   
        }

        return samples;
    }
}
