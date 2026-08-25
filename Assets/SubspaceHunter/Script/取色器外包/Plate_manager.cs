using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Plate_manager : MonoBehaviour
{
     public GameObject[] Plates;
     public TextMeshProUGUI[]  result;
     public TextMeshProUGUI[]  Time;

    int Cur_index=0;
    private int res_index;
    public GameObject ResExcel;
    public GameObject Test_component;

    public void Next_plate()
    {
        if(Cur_index+1<Plates.Length)
        {
            Plates[Cur_index].SetActive(false);
            Cur_index++;
            Plates[Cur_index].SetActive(true);
        }
        else
        {
             Plates[Cur_index].SetActive(false);
             Test_component.SetActive(false);
             ResExcel.SetActive(true);
        }
    }

    public void setResultAndTime(string res,string resTime)
    {
        result[res_index].text = res;
        Time[res_index].text = resTime;
        res_index++;
    }
}
