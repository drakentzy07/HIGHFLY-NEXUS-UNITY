/*
 * SubspaceHunter-SAO bilingual code note / 双语代码说明
 * 模块 / Module: 战斗流程管理 / Battle flow management
 * 功能 / Purpose: 负责战斗开始、敌人选择与生成、计时、胜负判断和战斗音乐切换。
 * English: Handles battle start, enemy selection and spawning, timing, win/loss decisions, and combat music switching.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using BehaviorDesigner.Runtime.Tasks.Unity.Math;



public class Battle_manager : MonoBehaviour
{
 
    public GameObject[] RandomEnemy_array;
    public Transform Enemy_spawn_trans_right;
     public Transform Enemy_spawn_trans_left;
    public Transform Enemy_spawn_trans;
    public Transform Enemy_spawn_trans4;
    public Transform Enemy_spawn_trans5;
    public GameObject 狗头人小兵;
    public GameObject 暗精灵居合;
    public GameObject 巨斧巨人;
    public GameObject 哥布林枪猎手;
    public GameObject 邪恶的鹿角;
    public GameObject 猪人屠夫;
    public GameObject 黑衣双剑;
    public GameObject 皇都近卫军剑盾;
    public GameObject 弑君者匕首;
    public GameObject 武器大师;
    public GameObject 西部双枪;
    public GameObject 北境之拳;
    public GameObject 北境之腿;  
    public GameObject 时空突击手;
    public GameObject 青眼恶魔;
    public GameObject 狗头人领主;
    public GameObject 蜥蜴人士兵;
    public GameObject 希斯克里夫;
    public GameObject[] 丧尸群;
    
  //  public GameObject 外星人指挥官;
    public GameObject[] 外星人士兵;


    public GameObject[] 殖民士兵;
    //public GameObject[] 辅助机器人;

    //public GameObject 时空特种部队指挥官;
    public GameObject[] 暗精灵危机;

    public GameObject SAO石头人;
    public GameObject 叛教徒尼克拉斯;


    public AudioSource Normal_bgm;
    public AudioSource Boss_bgm;
    public  AudioSource seed_bgm;
    public AudioSource DNF;
    public AudioSource GleamEyes_bgm;
    public AudioSource DogLord_bgm;
    public AudioSource santan_bgm;
    public AudioSource Sword_land;
    

   
    public TextMeshProUGUI textMeshProUGUI;
    private float startTime;
    public bool start_record;
    private float Tmp_time_point;
    public Image imageComponent;

    // Set the fill amount to 0 initially
    float fillAmount = 0f;

// Set the time it takes to fill the image to 1 minute
    public float fillTime = 60f;

    public GameObject winner_flag;
    public GameObject Duel_flag;
    public GameObject Time_count_ui;
    public GameObject Result_time_ui;
    public GameObject Enemy_winUI;
    public GameObject Player_winUI;
    public AudioSource Battle_endBgm;
    private GameObject Cur_enemy;
   
    public GameObject Enemy_set;

    public string ThisTime_enemyName;
    Vector3 ger_point;
        Vector3 ger_point_right;
        Vector3 ger_point_left;

        Vector3 ger_point4;
        Vector3 ger_point5;
       
    
    //动态持续刷怪敌人
    public int Max_group_enemyCount=15;
    public int Max_keep_amouont=3;
    public float Enemy_ger_intetval=1f;
    private int group_enemyCount;
    private int group_death;

    //静态多个数量的一次性敌人
    public int static_enemyNumber=5;
    private int Death_statciEnemy;

    //当前挂载的敌人名字
    GameObject Cur_enemy_Prefab;
    GameObject[] Cur_ememy_PrefabArray;
    AudioSource Cur_music;

    //用于生成单个敌人的小裂缝prefab
    public GameObject unit_portal;
    //随机生成敌人范围
    public int ger_range=6;

    
    private void Start() {
          //  Gernerate_enemy("狗头人小兵");
          group_enemyCount=Max_group_enemyCount;
          battle_start();
    }

    float ger_y=0;
    public void Gernerate_enemy(string enemy_name)
    {   
      
        GameObject enemy_spa = GameObject.FindWithTag("Enemy_spawn");
        if(enemy_spa!=null)
        {
          if(enemy_spa.GetComponent<Battle_call>().is_MR)
          {
            ger_y = 0.1f;
          }else
          {
            ger_y = enemy_spa.transform.position.y;
          }
        } 
         ger_point=new Vector3(Enemy_spawn_trans.position.x,ger_y,Enemy_spawn_trans.position.z);
         ger_point_right=new Vector3(Enemy_spawn_trans_right.position.x,ger_y,Enemy_spawn_trans_right.position.z);
         ger_point_left=new Vector3(Enemy_spawn_trans_left.position.x,ger_y,Enemy_spawn_trans_left.position.z);
         ger_point4=new Vector3(Enemy_spawn_trans4.position.x,ger_y,Enemy_spawn_trans4.position.z);
         ger_point5=new Vector3(Enemy_spawn_trans5.position.x,ger_y,Enemy_spawn_trans5.position.z);
        switch(enemy_name)
        {
            case "狗头人小兵" :
            Cur_enemy_Prefab=狗头人小兵;
            Cur_music= seed_bgm;
            portal_get_solo();
            
           // Cur_enemy=Instantiate(狗头人小兵,ger_point,transform.rotation);
          //  Deal_seedBGM_start();
            // Invoke("Potrol_close",1.5f);
            break;
            case "暗精灵居合":
            Cur_enemy_Prefab=暗精灵居合;
            Cur_music= Boss_bgm;
            portal_get_solo();
          //  Cur_enemy=Instantiate(暗精灵居合,ger_point,transform.rotation);
            
            // Invoke("Potrol_close",1.5f);
            break;
            case "巨斧巨人":
             Cur_enemy_Prefab=巨斧巨人;
             Cur_music= Boss_bgm;
            portal_get_solo();
           // Cur_enemy=Instantiate(巨斧巨人,ger_point,transform.rotation);
            
           // Deal_boss_start();
            // Invoke("Potrol_close",1.5f);
            break;
            case "哥布林枪猎手":
            Cur_enemy_Prefab=哥布林枪猎手;
            Cur_music= seed_bgm;
            portal_get_solo();
            //Cur_enemy=Instantiate(哥布林枪猎手,ger_point,transform.rotation);
            
          //  Deal_seedBGM_start();
            // Invoke("Potrol_close",1.5f);
            break;
            case "邪恶的鹿角":
            Cur_enemy_Prefab=邪恶的鹿角;
            Cur_music= seed_bgm;
            portal_get_solo();
           // Cur_enemy=Instantiate(邪恶的鹿角,ger_point,transform.rotation);
            
          //  Deal_seedBGM_start();
            // Invoke("Potrol_close",1.5f);
            break;
            case "猪人屠夫":
            Cur_enemy_Prefab=猪人屠夫;
              Cur_music= Boss_bgm ;
            portal_get_solo();
           // Cur_enemy=Instantiate(猪人屠夫,ger_point,transform.rotation);
          
           // Deal_boss_start();
            // Invoke("Potrol_close",1.5f);
            break;
            case "黑衣双剑":
            Cur_enemy_Prefab=黑衣双剑;
            Cur_music= seed_bgm;
            portal_get_solo();
           // Cur_enemy=Instantiate(黑衣双剑,ger_point,transform.rotation);
             
            //Deal_seedBGM_start();
             //Invoke("Potrol_close",1.5f);
            break;
            case "皇都近卫军剑盾":
            Cur_enemy_Prefab=皇都近卫军剑盾;
            Cur_music= seed_bgm;
            portal_get_solo();
            //Cur_enemy=Instantiate(皇都近卫军剑盾,ger_point,transform.rotation);
             
           // Deal_seedBGM_start();
           //  Invoke("Potrol_close",1.5f);
            break;
            case "希斯克里夫":
            Cur_enemy_Prefab=希斯克里夫;
            Cur_music= Sword_land;
            portal_get_solo();
            //Cur_enemy=Instantiate(皇都近卫军剑盾,ger_point,transform.rotation);
             
           // Deal_seedBGM_start();
           //  Invoke("Potrol_close",1.5f);
            break;
            case "弑君者匕首":
            Cur_enemy_Prefab=弑君者匕首;
             Cur_music= seed_bgm;
            portal_get_solo();
           // Cur_enemy=Instantiate(弑君者匕首,ger_point,transform.rotation);
            
           // Deal_seedBGM_start();
           //  Invoke("Potrol_close",1.5f);
            break;
            case "武器大师":
             Cur_enemy_Prefab=武器大师;
             Cur_music= seed_bgm;
            portal_get_solo();
           // Cur_enemy=Instantiate(武器大师,ger_point,transform.rotation);
            
           //Deal_seedBGM_start();
           // Invoke("Potrol_close",1.5f);
            break;
            case "西部双枪":
            Cur_enemy_Prefab=西部双枪;
             Cur_music= seed_bgm;
            portal_get_solo();
            //Cur_enemy=Instantiate(西部双枪,ger_point,transform.rotation);
            
            //Deal_seedBGM_start();
           //  Invoke("Potrol_close",1.5f);
            break;
            case "北境之腿":
            Cur_enemy_Prefab=北境之腿;
             Cur_music= seed_bgm;
            portal_get_solo();
            //Cur_enemy=Instantiate(北境之腿,ger_point,transform.rotation);
            //Deal_DNFBGM_start();
            
            //Deal_seedBGM_start();
           //  Invoke("Potrol_close",1.5f);
            break;
            case "时空突击手":
            Cur_enemy_Prefab=时空突击手;
             Cur_music= seed_bgm;
            portal_get_solo();
           // Cur_enemy=Instantiate(时空突击手,ger_point,transform.rotation);
           
           // Deal_seedBGM_start();
            // Invoke("Potrol_close",1.5f);
            break;
            case "北境之拳":
            Cur_enemy_Prefab=北境之拳;
            Cur_music= seed_bgm;
            portal_get_solo();
           // Cur_enemy=Instantiate(北境之拳,ger_point,transform.rotation);
            //Deal_DNFBGM_start();
             
            //Deal_seedBGM_start();
          //   Invoke("Potrol_close",1.5f);
            break;
            case "青眼恶魔":
            Cur_enemy_Prefab=青眼恶魔;
            Cur_music= GleamEyes_bgm;
            portal_get_solo();
           // Cur_enemy=Instantiate(青眼恶魔,ger_point,transform.rotation);
             
            //Deal_GleamEyeBGM_start();
           //  Invoke("Potrol_close",1.5f);
            break;
            case "叛教徒尼克拉斯":
            Cur_enemy_Prefab=叛教徒尼克拉斯;
            Cur_music= santan_bgm;
            portal_get_solo();
            break;
            case "狗头人领主":
            Cur_enemy_Prefab=狗头人领主;
            Cur_music= DogLord_bgm;
            portal_get_solo();
            break;
            case "蜥蜴人士兵":
            print("进入蜥蜴人士兵1");
             Cur_enemy_Prefab=蜥蜴人士兵;
             Cur_music= seed_bgm;
            portal_get_solo();
            break;
            case "丧尸群":
             Cur_ememy_PrefabArray=丧尸群;    
             Cur_music= seed_bgm; 
             init_Group_Enemy_variate();
              
           // Deal_seedBGM_start();
           // Invoke("Potrol_close",1.5f);
            break;
            case "暗精灵危机":
          //  Ger_Space_crisis(); 
            Cur_ememy_PrefabArray=暗精灵危机;  
            Cur_music= seed_bgm;   
             init_Group_Enemy_variate();   
                  
           // Deal_seedBGM_start();
           //  Invoke("Potrol_close",1.5f);
            break;
            case "异星危机":
           //  Ger_Alien_crisis();  
            Cur_ememy_PrefabArray=外星人士兵;  
            Cur_music= seed_bgm;   
             init_Group_Enemy_variate();     
              
           // Deal_seedBGM_start();
           //  Invoke("Potrol_close",1.5f);
            break;
            case "殖民危机":
          //  Ger_AI_crisis(); 
            Cur_ememy_PrefabArray=殖民士兵; 
            Cur_music= seed_bgm;     
             init_Group_Enemy_variate();   
                  
           // Deal_seedBGM_start();
           //  Invoke("Potrol_close",1.5f);
            break;
            case "SAO石头人":
            Cur_enemy_Prefab=SAO石头人;
            Cur_music= seed_bgm;
            init_Group_Enemy();
             
            //Deal_seedBGM_start();
           // Invoke("Potrol_close",0.5f);
          //  Deal_Group_enmey_ger();         
            
            break;
                     
        }
        if(null!=Cur_enemy)
        Cur_enemy.transform.SetParent(Enemy_set.transform);
    }

    private void portal_get_solo()
    {
        print("进入蜥蜴人士兵2");
                 GameObject temp = Instantiate(unit_portal,ger_point,transform.rotation);
                 temp.transform.SetParent(Enemy_set.transform);
                 temp.GetComponent<Portal_unit_Ger>().init_paramater(Cur_enemy_Prefab,Enemy_set,Cur_music);
                 temp.GetComponent<Portal_unit_Ger>().set_protal_type(ThisTime_enemyName);
    }

    //暂时用的
    private void Ger_Alien_crisis()
    {
       /* GameObject temp=Instantiate(外星人指挥官,ger_point,transform.rotation);
        temp.transform.SetParent(Enemy_set.transform);
         temp=Instantiate(外星人士兵[Random.Range(0,外星人士兵.Length)],ger_point_left,transform.rotation);
        temp.transform.SetParent(Enemy_set.transform);
        temp=Instantiate(外星人士兵[Random.Range(0,外星人士兵.Length)],ger_point_right,transform.rotation);
        temp.transform.SetParent(Enemy_set.transform);
        temp=Instantiate(外星人士兵[Random.Range(0,外星人士兵.Length)],ger_point4,transform.rotation);
        temp.transform.SetParent(Enemy_set.transform);
        temp=Instantiate(外星人士兵[Random.Range(0,外星人士兵.Length)],ger_point5,transform.rotation);
        temp.transform.SetParent(Enemy_set.transform);*/
        
    }

    private void Ger_Space_crisis()
    {
       /* GameObject temp=Instantiate(时空特种部队指挥官,ger_point,transform.rotation);
        temp.transform.SetParent(Enemy_set.transform);
         temp=Instantiate(时空特种部队士兵[Random.Range(0,时空特种部队士兵.Length)],ger_point_left,transform.rotation);
        temp.transform.SetParent(Enemy_set.transform);
        temp=Instantiate(时空特种部队士兵[Random.Range(0,时空特种部队士兵.Length)],ger_point_right,transform.rotation);
        temp.transform.SetParent(Enemy_set.transform);
        temp=Instantiate(时空特种部队士兵[Random.Range(0,时空特种部队士兵.Length)],ger_point4,transform.rotation);
        temp.transform.SetParent(Enemy_set.transform);
        temp=Instantiate(时空特种部队士兵[Random.Range(0,时空特种部队士兵.Length)],ger_point5,transform.rotation);
        temp.transform.SetParent(Enemy_set.transform);*/
    }

    private void Ger_AI_crisis()
    {
        GameObject temp=Instantiate(殖民士兵[0],ger_point,transform.rotation);
        temp.transform.SetParent(Enemy_set.transform);
         temp=Instantiate(殖民士兵[0],ger_point_left,transform.rotation);
        temp.transform.SetParent(Enemy_set.transform);
       /* temp=Instantiate(辅助机器人[0],ger_point_right,transform.rotation);
        temp.transform.SetParent(Enemy_set.transform);
        temp=Instantiate(辅助机器人[0],ger_point4,transform.rotation);
        temp.transform.SetParent(Enemy_set.transform);
        temp=Instantiate(辅助机器人[0],ger_point5,transform.rotation);
        temp.transform.SetParent(Enemy_set.transform);*/
    }

    //杀死固定复数敌人后胜利
     public void Deal_Static_Death()
    {
        Death_statciEnemy++;
        if(Death_statciEnemy==static_enemyNumber) { Battle_end("win"); }
       
    }

   //杀死动态变化数量敌人后胜利
    public bool Deal_group_enemy_Death()
    {
         group_death++;
        if(group_death==Max_group_enemyCount) { Battle_end("win");  return true;}
      /*  else
        {
            //GameObject temp=Instantiate(丧尸群[Random.Range(0,丧尸群.Length)],ger_point,transform.rotation);
              //   temp.transform.SetParent(Enemy_set.transform);
            switch(Random.Range(0,3))
            {
                case 0:
                GameObject temp=Instantiate(丧尸群[Random.Range(0,丧尸群.Length)],ger_point,transform.rotation);
                // temp.transform.SetParent(Enemy_set.transform);
                break;
                case 1:
                  GameObject temp1=Instantiate(丧尸群[Random.Range(0,丧尸群.Length)],ger_point_right,transform.rotation);
                 //  temp1.transform.SetParent(Enemy_set.transform);
                break;
                case 2:
                  GameObject temp2=Instantiate(丧尸群[Random.Range(0,丧尸群.Length)],ger_point_left,transform.rotation);
                   // temp2.transform.SetParent(Enemy_set.transform);
                break;
            }
            group_enemyCount--;
        }*/
        return false;
    }

    
    private void Wait_2_gerEnemy()
    {
                Vector2 randomXY = ger_randomXY();
                GameObject temp = Instantiate(unit_portal,new Vector3(ger_point.x+randomXY.x,ger_point.y,ger_point.z+randomXY.y),transform.rotation);
                 temp.transform.SetParent(Enemy_set.transform);
                 temp.GetComponent<Portal_unit_Ger>().init_paramater(Cur_enemy_Prefab,Enemy_set,Cur_music);
                 temp.GetComponent<Portal_unit_Ger>().set_protal_type(ThisTime_enemyName);

             
    }

    private Vector2 ger_randomXY()
    {
         int x=0;
                  int y=0;
                  int direction_seed = Random.Range(0,2);
                  if(0==direction_seed)  x=Random.Range(0,ger_range);
                  else x=-Random.Range(0,ger_range);

                  direction_seed = Random.Range(0,2);
                  if(0==direction_seed)  y=Random.Range(0,ger_range);
                  else y=-Random.Range(0,ger_range);
                  return new Vector2(x,y);
    }

    private void Wait_2_gerEnemy_variate()
    {              
                 
                    Vector2 randomXY = ger_randomXY();
                 GameObject temp = Instantiate(unit_portal,new Vector3(ger_point.x+randomXY.x,ger_point.y,ger_point.z+randomXY.y),transform.rotation);
                temp.transform.SetParent(Enemy_set.transform);
               // temp.transform.localScale= new Vector3(33,33,33);
                temp.GetComponent<Portal_unit_Ger>().init_paramater( Cur_ememy_PrefabArray[Random.Range(0,Cur_ememy_PrefabArray.Length)],Enemy_set,Cur_music);
                temp.GetComponent<Portal_unit_Ger>().set_protal_type(ThisTime_enemyName);
             
    }


    //用于单品种的敌人群体生成.
    private void init_Group_Enemy()
    {
       // int enemy_alive = GameObject.FindGameObjectsWithTag("enemy_body").Length;  
        if(Max_keep_amouont<=Max_group_enemyCount-group_enemyCount) return;
        print("进入生成111");
        Deal_Group_Enemy_ger();
        Invoke("init_Group_Enemy",1f);
       
    }

    //用于非单一品种的敌人生成
     private void init_Group_Enemy_variate()
    {
        if(Max_keep_amouont<=Max_group_enemyCount-group_enemyCount) return;
        print("进入生成111");
        Deal_Group_Enemy_ger_variate();
        Invoke("init_Group_Enemy_variate",1f);
     
    }

    //需要满足敌人死亡后来激活，这样就可以保证场上有固定数量的敌人
    public void Deal_Group_Enemy_ger()
    {
            if(group_enemyCount==0){return;}
            if(null==Enemy_set) return;
            GameObject[] enemy_alive= GameObject.FindGameObjectsWithTag("enemy_body");  
            // if(Max_keep_amouont<=Max_group_enemyCount-group_enemyCount) return;
             print("进入生成222");
          //  if(Max_group_enemyCount-group_enemyCount>=Max_keep_amouont) return;
             Invoke("Wait_2_gerEnemy",Enemy_ger_intetval);

             group_enemyCount--;
           // Invoke("Deal_Group_enmey_ger",11f);
    }

    

    //需要满足敌人死亡后来激活，这样就可以保证场上有固定数量的敌人
    public void Deal_Group_Enemy_ger_variate()
    {
            if(group_enemyCount==0){return;}
            if(null==Enemy_set) return;
             GameObject[] enemy_alive= GameObject.FindGameObjectsWithTag("enemy_body");  
             if(Max_keep_amouont<=enemy_alive.Length) return;
          //  if(Max_group_enemyCount-group_enemyCount>=Max_keep_amouont) return;
          
            Invoke("Wait_2_gerEnemy_variate",Enemy_ger_intetval);

             group_enemyCount--;
           // Invoke("Deal_Group_enmey_ger",11f);
    }


    private void Deal_normal_start()
    {
        if(null!=Normal_bgm)
        Normal_bgm.Play();
    }

    private void Deal_boss_start()
    {
         if(null!=Boss_bgm)
        Boss_bgm.Play();
    }

    private void Deal_seedBGM_start()
    {
         if(null!=seed_bgm)
        seed_bgm.Play();
    }

    private void Deal_DNFBGM_start()
    {
         if(null!=DNF)
        DNF.Play();
    }

    private void Deal_GleamEyeBGM_start()
    {
         if(null!=GleamEyes_bgm)
        GleamEyes_bgm.Play();
    }

     private void Deal_DogLordBGM_start()
    {
         if(null!=DogLord_bgm)
        DogLord_bgm.Play();
    }

    private void close_battle_bgm()
    {
        if(null!=Normal_bgm)
        Normal_bgm.Stop();
        if(null!=Boss_bgm)
        Boss_bgm.Stop();
    }

   

    private void battle_start()
    {
        Gernerate_enemy(ThisTime_enemyName);
        start_record=true;
       
    }

    private void Potrol_close()
    {
        GetComponent<Animator>().SetTrigger("Game_End");
    }

     
 AR_scen_controller colle;
    public void Battle_end(string End_type)
    {
       GameObject temp111 =   GameObject.FindWithTag("enemy_AR_secen_manger");
        
         if(null!= temp111)
         colle = temp111.GetComponent<AR_scen_controller>();

        if(null != colle) colle.Recovery_AR_defultLight();

       
        GetComponent<Animator>().enabled=false;
        if(null!=Duel_flag)
        Duel_flag.SetActive(false);
        start_record=false;
        if(null!=Time_count_ui)
        Time_count_ui.SetActive(false);
        if(null!=Result_time_ui)
        Result_time_ui.SetActive(true);
        if(null!=Result_time_ui)
        Result_time_ui.GetComponent<TextMeshProUGUI>().text ="TIME:"+startTime.ToString("00:00");
        if(null!=winner_flag)
        winner_flag.SetActive(true);
       // Battle_endBgm.Play();
        close_battle_bgm();

        if("win"==End_type)
        {
            Deal_Playerwin();
        }else if("lose"==End_type)
        {
             Deal_Enemywin();
        }
    }

    private void Deal_Playerwin()
    {
        if(null!=Player_winUI)
        Player_winUI.SetActive(true);
        GetComponent<Animator>().SetTrigger("Game_End");
        Destroy(transform.gameObject,8f);
    }

     private void Deal_Enemywin()
    {
        if(null!=Enemy_winUI)
        Enemy_winUI.SetActive(true);
        GetComponent<Animator>().SetTrigger("Game_End");
        Destroy(Cur_enemy);
        Destroy(Enemy_set);
        Destroy(transform.gameObject,8f);
         
    }


    public void RecordStartTimeAndUpdateText()
{
    if(0==Tmp_time_point)
    {   
         Tmp_time_point=Time.time;
        StartCoroutine(FillImage());
    }
   
    // Record the start time
    startTime = Time.time-Tmp_time_point;

    // Update the TextMeshProUGUI component with the start time formatted as "00:00"
    textMeshProUGUI.text = startTime.ToString("00:00");
}

private void Update() {
    if(start_record) RecordStartTimeAndUpdateText();
    if(Input.GetKeyDown(KeyCode.T)) start_record=true;
}


// Create a coroutine to fill the image over time
IEnumerator FillImage()
{
    // Loop indefinitely
    while (true)
    {
        // Increment the fill amount over time
        float timer = 0f;
        while (timer < fillTime)
        {
            timer += Time.deltaTime;
            fillAmount = timer / fillTime;
            imageComponent.fillAmount = fillAmount;
            yield return null;
        }

        // Reset the fill amount to 0 when it reaches 1
        fillAmount = 0f;
        imageComponent.fillAmount = fillAmount;
    }
}


}
