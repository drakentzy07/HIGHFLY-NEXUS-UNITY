using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot2Start : MonoBehaviour
{
    public GameObject 测试组件;
    public GameObject 引导组件;
    public GameObject[] 进入界面;

    public void GunShoot2Start()
    {
        foreach (var item in 进入界面)
        {
            item.SetActive(false);
        }
        引导组件.SetActive(true);
        Invoke("activateTest",10f);
    }

    private void activateTest()
    {
        引导组件.SetActive(false);
        测试组件.SetActive(true);
    }
}
