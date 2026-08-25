/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 弹痕对象池 / Bullet decal pool
 * 功能 / Purpose: 复用命中特效或弹痕对象，降低运行时实例化成本。
 * English: Reuses hit effects or bullet decals to reduce runtime instantiation cost.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Knife;
using Knife.Effects.SimpleController;
using Knife.Effects;


public class Hit_surface_pools :  HittableSurface
{
    
     public new void TakeDamage(DamageData[] damage)
        {
            foreach (var d in damage)
            {
                if (d.damageType == DamageTypes.Bullet)
                {
                    var point = d.point + d.normal * Random.Range(offsetMin, offsetMax);
                    if (decalPrefabs != null && decalPrefabs.Length > 0)
                    {
                        var decalInstance = Instantiate(decalPrefabs[Random.Range(0, decalPrefabs.Length)]);
                        var decal = decalInstance.GetComponent<IDecal>();

                        bool canRotate = decal == null || decal.CanRotate;

                        decalInstance.transform.position = point;
                        decalInstance.transform.rotation = Quaternion.LookRotation(d.normal);

                        if (randomRotation && canRotate)
                            decalInstance.transform.rotation = Quaternion.AngleAxis(Random.Range(0, 360), d.normal) * decalInstance.transform.rotation;

                        decalInstance.transform.localScale = Vector3.one * size * d.size;

                        if (autoParent)
                        {
                            decalInstance.transform.SetParent(transform);
                        }
                    }

                    if (impactPrefabs != null && impactPrefabs.Length > 0)
                    {
                        var impactInstance = Instantiate(impactPrefabs[Random.Range(0, impactPrefabs.Length)]);
                        impactInstance.transform.position = point;
                        impactInstance.transform.up = d.normal;

                        if (randomRotation)
                            impactInstance.transform.rotation = Quaternion.AngleAxis(Random.Range(0, 360), d.normal) * impactInstance.transform.rotation;

                        impactInstance.transform.localScale = Vector3.one * impactSize * d.size;
                        if (autoParent)
                        {
                            impactInstance.transform.SetParent(transform);
                        }
                    }
                }
            }
        }
}
