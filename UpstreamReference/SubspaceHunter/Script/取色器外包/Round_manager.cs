using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Round_manager : MonoBehaviour
{
    public GameObject[] Rounds;
    public Plate_manager plate_manager;
    public GameObject Display_UI;
    //public TextMeshProUGUI textMeshProUGUI;
    int Cur_index=0;
    public Time_record time_Record;

    private void Start() {
        time_Record.ResetTimer();
        time_Record.ResumeTimer();
    }

    public void Display_result(string res)
    {
        if(Cur_index+1<Rounds.Length)
        {
            Display_UI.SetActive(true);
           // textMeshProUGUI.text = res;
            Rounds[Cur_index].SetActive(false);
            Cur_index++;
            time_Record.PauseTimer();
            plate_manager.setResultAndTime(res,time_Record.timerString);
            Invoke("open_next",3f);
        }else
        {
            Display_UI.SetActive(true);
         //   textMeshProUGUI.text = res;
            Rounds[Cur_index].SetActive(false);
            time_Record.PauseTimer();
            plate_manager.setResultAndTime(res,time_Record.timerString);
            Invoke("Next_p",3f);
        }

        
    }

    private void Next_p()
    {
        Display_UI.SetActive(false);
         plate_manager.Next_plate();
    }


    private void open_next()
    {
        time_Record.ResetTimer();
        time_Record.ResumeTimer();
         Display_UI.SetActive(false);
        Rounds[Cur_index].SetActive(true);
    }

    
    


}
