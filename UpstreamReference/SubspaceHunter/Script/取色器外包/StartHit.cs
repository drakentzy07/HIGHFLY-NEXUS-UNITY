using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartHit : MonoBehaviour
{
   
    public Shoot2Start shoot2Start;
    private void OnCollisionEnter(Collision other) {
    
        if(other.transform.gameObject.tag=="Player_bullet")
        {
            // print("硬碰撞体"+other.gameObject.name);
              if(other.transform.gameObject.tag=="Player_bullet")
            {
              
                 
                shoot2Start.GunShoot2Start();
                transform.gameObject.SetActive(false);
                 
                
            }
        }
    }
}
