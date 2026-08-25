/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 子弹状态 / Bullet state
 * 功能 / Purpose: 定义子弹飞行、命中、伤害或生命周期状态。
 * English: Defines bullet flight, hit, damage, or lifecycle state.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet_status2 : MonoBehaviour
{

     public enum hit_type{
        碰撞无影响,
        碰撞火花不消失,
        碰撞火花并消失,
        碰撞被切割,
        碰撞切割或跳弹
    }

    private bool is_minusHp;
    public float minus_hp_amount=10f;
    public hit_type 碰撞类型;
    public GameObject 碰撞效果;
    public GameObject 碰撞声音;
    //public GameObject 拦截子弹声音;
    public float 子弹存在时间 = 2f;

     public bool rotate = false;
    public float rotateAmount = 45;
    public bool bounce = false;
    public float bounceForce = 10;
    public float speed;
	[Tooltip("From 0% to 100%")]
	public float accuracy;
	public float fireRate;
	public GameObject muzzlePrefab;
	public GameObject hitPrefab;
	public List<GameObject> trails;

    private Vector3 startPos;
	private float speedRandomness;
	private Vector3 offset;
	private bool collided;
	private Rigidbody rb;
    private RotateToMouseScript rotateToMouse;
    private GameObject target;
    [Range(1,10)]
    public float 对武器的伤害=2f;
    [Range(0.1f,1)]
    public float slice_or_bounceRate=0.5f;

	void Start () {
        startPos = transform.position;
        rb = GetComponent <Rigidbody> ();

		//used to create a radius for the accuracy and have a very unique randomness
		if (accuracy != 100) {
			accuracy = 1 - (accuracy / 100);

			for (int i = 0; i < 2; i++) {
				var val = 1 * Random.Range (-accuracy, accuracy);
				var index = Random.Range (0, 2);
				if (i == 0) {
					if (index == 0)
						offset = new Vector3 (0, -val, 0);
					else
						offset = new Vector3 (0, val, 0);
				} else {
					if (index == 0)
						offset = new Vector3 (0, offset.y, -val);
					else
						offset = new Vector3 (0, offset.y, val);
				}
			}
		}
			
		if (muzzlePrefab != null) {
			var muzzleVFX = Instantiate (muzzlePrefab, transform.position, Quaternion.identity);
			muzzleVFX.transform.forward = gameObject.transform.forward + offset;
			var ps = muzzleVFX.GetComponent<ParticleSystem>();
			if (ps != null)
				Destroy (muzzleVFX, ps.main.duration);
			else {
				var psChild = muzzleVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
				Destroy (muzzleVFX, psChild.main.duration);
			}
		}
	}

	void FixedUpdate () {
       /* if (target != null)
            rotateToMouse.RotateToMouse (gameObject, target.transform.position);*/
        if (rotate)
            transform.Rotate(0, 0, rotateAmount, Space.Self);
        if (speed != 0 && rb != null)
			rb.position += (transform.forward + offset) * (speed * Time.deltaTime);   
    }

	void OnCollisionEnter (Collision co) {
          if(co.transform.tag=="Player_body"&&!is_minusHp)
        {
            is_minusHp=true;
            print("碰到玩家");//打到玩家扣血并将子弹消除
            if(null!=co.transform.GetComponent<Player_managerV2>())
            {
                co.transform.GetComponent<Player_managerV2>().Minus_Hp(minus_hp_amount,"切割");
                Destroy(transform.gameObject);
            }
            
            
        }else if(co.transform.tag=="Weapon_inHand"||co.transform.tag=="Weapon_inRightHand")
        {
            Physic_weaponManager physic_WeaponManager= co.transform.GetComponent<Physic_weaponManager>();
            if(null!=physic_WeaponManager) {
                physic_WeaponManager.Deal_short_impusle();
                physic_WeaponManager.weapon_Durability.Mins_dur(对武器的伤害);
            }
            
            switch(碰撞类型)
            {
                case hit_type.碰撞无影响:
                break;
                case hit_type.碰撞火花并消失:
                   Gen_AND_des();
                break;
                case hit_type.碰撞被切割:
                   Slice_bullet(co.gameObject);
                break;
                case hit_type.碰撞切割或跳弹:
                   Slice_OR_bounce(co.gameObject);
                break;
                case hit_type.碰撞火花不消失:
                  Ger_AND_NotDes();
                break;
            }
        }

        if (!bounce)
        {
            if (co.gameObject.tag != "Bullet" && !collided)
            {
                collided = true;

                if (trails.Count > 0)
                {
                    for (int i = 0; i < trails.Count; i++)
                    {
                        trails[i].transform.parent = null;
                        var ps = trails[i].GetComponent<ParticleSystem>();
                        if (ps != null)
                        {
                            ps.Stop();
                            Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
                        }
                    }
                }

                speed = 0;
                GetComponent<Rigidbody>().isKinematic = true;

                ContactPoint contact = co.contacts[0];
                Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
                Vector3 pos = contact.point;

                if (hitPrefab != null)
                {
                    var hitVFX = Instantiate(hitPrefab, pos, rot) as GameObject;

                    var ps = hitVFX.GetComponent<ParticleSystem>();
                    if (ps == null)
                    {
                        var psChild = hitVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
                        Destroy(hitVFX, psChild.main.duration);
                    }
                    else
                        Destroy(hitVFX, ps.main.duration);
                }

                //StartCoroutine(DestroyParticle(0f));
            }
        }
        else
        {
            rb.useGravity = true;
            rb.drag = 0.5f;
            ContactPoint contact = co.contacts[0];
            rb.AddForce (Vector3.Reflect((contact.point - startPos).normalized, contact.normal) * bounceForce, ForceMode.Impulse);
            Destroy ( this );
        }
       
	}

	public IEnumerator DestroyParticle (float waitTime) {

		if (transform.childCount > 0 && waitTime != 0) {
			List<Transform> tList = new List<Transform> ();

			foreach (Transform t in transform.GetChild(0).transform) {
				tList.Add (t);
			}		

			while (transform.GetChild(0).localScale.x > 0) {
				yield return new WaitForSeconds (0.01f);
				transform.GetChild(0).localScale -= new Vector3 (0.1f, 0.1f, 0.1f);
				for (int i = 0; i < tList.Count; i++) {
					tList[i].localScale -= new Vector3 (0.1f, 0.1f, 0.1f);
				}
			}
		}
		
		yield return new WaitForSeconds (waitTime);
		Destroy (gameObject);
	}

    public void SetTarget (GameObject trg, RotateToMouseScript rotateTo)
    {
        target = trg;
        rotateToMouse = rotateTo;
    }

     private void Gen_AND_des()
    {
        Instantiate(碰撞效果,transform.position,Quaternion.identity);
        Instantiate(碰撞声音,transform.position,Quaternion.identity);   
                Destroy(transform.gameObject);
    }

    private void Ger_AND_NotDes()
    {
        Instantiate(碰撞效果,transform.position,Quaternion.identity);
        Instantiate(碰撞声音,transform.position,Quaternion.identity); 
                 
    }

    private void Slice_bullet(GameObject other)
    {
        Instantiate(碰撞效果,transform.position,Quaternion.identity);
        Instantiate(碰撞声音,transform.position,Quaternion.identity); 
        GetComponent<Slice_Item>().set_point(other);
        GetComponent<Slice_Item>().Deal_slice();         
    }
     private void Slice_OR_bounce(GameObject other)
    {
        if(Random.Range(0,1f)>slice_or_bounceRate)
        {
            GetComponent<Slice_Item>().set_point(other);
            GetComponent<Slice_Item>().Deal_slice();  
        }
        
        
        Instantiate(碰撞效果,transform.position,Quaternion.identity);
        Instantiate(碰撞声音,transform.position,Quaternion.identity); 
               
    }
}
