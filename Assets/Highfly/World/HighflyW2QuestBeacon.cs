using UnityEngine;
using UnityEngine.UI;

namespace Highfly.World
{
    /// <summary>Honest in-world navigational clue for first quest giver.
    /// Dynamic marker and distance; no quest auto-completion or teleporting.</summary>
    public sealed class HighflyW2QuestBeacon : MonoBehaviour
    {
        [SerializeField] private Transform hunter;
        [SerializeField] private HighflyW2Progress progress;
        [SerializeField] private Text hud;
        [SerializeField] private string npcObjectName="Serin_W2";
        private Transform _serin;
        private TextMesh _label;
        private float _nextLookup;

        public void Configure(Transform player,HighflyW2Progress state,Text readout)
        {
            hunter=player;progress=state;hud=readout;
        }

        private void Update()
        {
            if(hunter==null||hud==null||progress==null)return;
            if(_serin==null && Time.time>=_nextLookup)
            {
                _nextLookup=Time.time+1f;
                GameObject npc=GameObject.Find(npcObjectName);
                if(npc!=null)
                {
                    _serin=npc.transform;
                    GameObject marker=new GameObject("W2_SERIN_QUEST_MARKER");
                    marker.transform.SetParent(_serin,false);
                    marker.transform.localPosition=new Vector3(0f,2.6f,0f);
                    _label=marker.AddComponent<TextMesh>();
                    _label.text="!  SERIN  !";
                    _label.characterSize=.18f;
                    _label.fontSize=90;
                    _label.fontStyle=FontStyle.Bold;
                    _label.anchor=TextAnchor.MiddleCenter;
                    _label.alignment=TextAlignment.Center;
                    _label.color=new Color(1f,.85f,.32f,1f);
                }
            }
            if(progress.QuestStage>=2)
            {
                hud.text="CONTRATO F COMPLETADO  ·  NUEVAS AVENTURAS PRONTO";
                if(_label!=null)_label.text="SERIN";
                return;
            }
            if(progress.QuestStage==1&&progress.SkeletonsKilled<2)
            {
                hud.text="MISIÓN F · ESQUELETOS "+progress.SkeletonsKilled+
                    "/2  ·  BOSQUE EXTERIOR";
                if(_label!=null)_label.text="SERIN / GREMIO";
                return;
            }
            if(_serin==null)
            {
                hud.text="GREMIO · BUSCÁ A SERIN CERCA DE LA ENTRADA";
                return;
            }
            Vector3 delta=_serin.position-hunter.position;
            float distance=delta.magnitude;
            Camera camera=Camera.main;
            Vector3 right=camera!=null?camera.transform.right:hunter.right;
            Vector3 flatRight=new Vector3(right.x,0f,right.z).normalized;
            float side=Vector3.Dot(delta,flatRight);
            string arrow=side>1.5f?"  →":side< -1.5f?"  ←":"  ↑";
            hud.text=(progress.QuestStage==0?"!  NUEVO CONTRATO · SERIN":
                "!  COBRAR RECOMPENSA · SERIN")+
                "  ·  "+Mathf.RoundToInt(distance)+" m"+arrow;
            if(_label!=null)
            {
                _label.text=progress.QuestStage==0?"!  SERIN · MISIÓN  !":
                    "!  SERIN · RECOMPENSA  !";
                if(camera!=null)_label.transform.rotation=camera.transform.rotation;
            }
        }
    }
}
