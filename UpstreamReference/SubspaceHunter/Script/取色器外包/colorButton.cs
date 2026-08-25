using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class colorButton : MonoBehaviour
{
   GameObject  generation_ui;
    public float DeSelect_GameObject_time=7f;
    public string Hint_message="confirm choice?";
    public GameObject Hint_prefab;
     public Transform Ray_shoot_Ori;
     [Range(0, 1)] [SerializeField] private float percentage=0.5f;
      //激活物品信息的hub，并且要把对物体的操作权限委托过去
    public void Activate_located()
   {
                        Activate_Std_messageBox(transform);
                         MessageBox_controller mess =generation_ui.transform.FindChildRecursive("Ui_body").GetComponent<MessageBox_controller>();
                        if(mess==null) print("messcontroller为空");
                        else mess.Deal_Confirm_mission+=Action;
                      //  StartCoroutine("Delay_2Deactivate");
                      //  body_outline.enabled=true;
   }

   public void Action()
   {
     print("点击成功");
   }

   

   private  void Activate_Std_messageBox(Transform UI_gerneratePoint)
    {
        //生成UI，并且把信息填入
         Vector3 gerneratePoint=Vector3.Lerp(Ray_shoot_Ori.position,UI_gerneratePoint.position , percentage);
         generation_ui=Instantiate(Hint_prefab,gerneratePoint,Quaternion.identity) as GameObject;
         generation_ui.transform.LookAt(Ray_shoot_Ori);
         generation_ui.transform.FindChildRecursive("Information").GetComponent<TextMeshProUGUI>().text=Hint_message;
    }
}
