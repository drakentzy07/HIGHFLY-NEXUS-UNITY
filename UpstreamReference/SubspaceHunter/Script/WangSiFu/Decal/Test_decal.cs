using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkinnedDecals;

public class Test_decal : MonoBehaviour
{
    public SkinnedDecal skinnedDecal;
    public SkinnedDecalSystem decalSystem;
    public Transform origin_trans;
    public Transform to_trans;
    private Vector3 direaction;
    
    public Transform camera_trans;

    public Transform it_self_trans;
    // Start is called before the first frame update
    void Start()
    {
        direaction=(to_trans.position-origin_trans.position).normalized;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            decalSystem.CreateDecal(skinnedDecal,origin_trans.position,direaction);
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
             decalSystem.CreateDecal(skinnedDecal,it_self_trans.position,it_self_trans.forward,it_self_trans.up);
        }
    }
}
