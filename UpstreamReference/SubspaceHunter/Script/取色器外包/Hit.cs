using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Hit : MonoBehaviour
{
    public ParticleSystem[] particleSystems;
    public Transform effect_point;
    public GameObject paritc;
     private bool is_interval_new;
     public AudioSource hit_sounds;
     public GameObject Target;
     public string Hint_text;
     public Round_manager round_Manager;
     //public GameObject DisplayUI;
     //public TextMeshProUGUI textMeshProUGUI;
    

    private void set_interval_reset()
    {
        is_interval_new=false;
    }    

    private void OnCollisionEnter(Collision other) {
        if(is_interval_new) return;
        if(other.transform.gameObject.tag=="Player_bullet")
        {
             print("硬碰撞体"+other.gameObject.name);
              if(other.transform.gameObject.tag=="Player_bullet")
            {
              
                 
                    is_interval_new=true;
                    Invoke("set_interval_reset",0.2f);
                    hit_sounds.Play();
                    Activate_effect();
                   // DisplayUI.SetActive(true);
                   // textMeshProUGUI.text = Hint_text;
                   round_Manager.Display_result(Hint_text);
                   Target.SetActive(false);
            }
        }
    }


    private void Activate_effect()
    {
        paritc.transform.position = effect_point.position;
        foreach (var item in particleSystems)
        {
            item.Play();
        }
    }

}
