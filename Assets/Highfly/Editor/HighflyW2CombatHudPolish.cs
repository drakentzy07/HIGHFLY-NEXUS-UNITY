#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;

namespace Highfly.Editor
{
    /// <summary>Native Unity action-RPG/MOBA skin for existing FURY HUD.
    /// No new combat buttons, skill listeners or camera interception.
    /// </summary>
    public static class HighflyW2CombatHudPolish
    {
        public static void Apply(GameObject canvas)
        {
            if(canvas==null)return;
            Decorate(canvas.transform,"ATAQUE_Button",
                new Color(.10f,.24f,.42f,.97f),
                new Color(.49f,.91f,1f,1f),"ATK");
            Decorate(canvas.transform,"S1_CORTE_Button",
                new Color(.19f,.17f,.41f,.97f),
                new Color(.83f,.65f,1f,1f),"01");
            Decorate(canvas.transform,"S2_ABANICO_Button",
                new Color(.29f,.15f,.44f,.97f),
                new Color(1f,.67f,.95f,1f),"02");
            Decorate(canvas.transform,"S3_ÁREA_Button",
                new Color(.34f,.12f,.35f,.97f),
                new Color(1f,.77f,.56f,1f),"03");
            Decorate(canvas.transform,"DASH_Button",
                new Color(.08f,.26f,.30f,.97f),
                new Color(.51f,1f,.89f,1f),"EVA");
            Decorate(canvas.transform,"PESADO_Button",
                new Color(.17f,.21f,.33f,.97f),
                new Color(.82f,.88f,1f,1f),"PWR");

            Transform hint=canvas.transform.Find("HIGHFLY_Hint");
            if(hint!=null)hint.gameObject.SetActive(false);
            Transform objective=canvas.transform.Find("CRIPTA F  •  ENEMIGOS -- / --");
            if(objective!=null)objective.gameObject.SetActive(false);
        }

        private static void Decorate(Transform canvas,string name,
            Color dark,Color rim,string rune)
        {
            Transform root=canvas.Find(name);
            if(root==null)return;
            Image img=root.GetComponent<Image>();
            if(img==null)return;
            img.color=dark;
            Outline edge=root.GetComponent<Outline>();
            if(edge==null)edge=root.gameObject.AddComponent<Outline>();
            edge.effectColor=rim;
            edge.effectDistance=new Vector2(3f,-3f);
            Text label=root.GetComponentInChildren<Text>(true);
            if(label!=null)
            {
                label.color=Color.white;
                label.fontStyle=FontStyle.Bold;
                Shadow shadow=label.GetComponent<Shadow>();
                if(shadow==null)shadow=label.gameObject.AddComponent<Shadow>();
                shadow.effectColor=new Color(.03f,.01f,.10f,.9f);
                shadow.effectDistance=new Vector2(2f,-3f);
            }
            GameObject number=new GameObject("W2_Rune_"+rune,
                typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));
            number.transform.SetParent(root,false);
            RectTransform rt=number.GetComponent<RectTransform>();
            rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);
            rt.pivot=new Vector2(.5f,.5f);
            RectTransform rect=root.GetComponent<RectTransform>();
            float size=rect!=null?rect.sizeDelta.y:160f;
            rt.anchoredPosition=new Vector2(0f,size*.34f);
            rt.sizeDelta=new Vector2(size*.72f,40f);
            Text glyph=number.GetComponent<Text>();
            glyph.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            glyph.text=rune;
            glyph.fontSize=20;
            glyph.fontStyle=FontStyle.Bold;
            glyph.alignment=TextAnchor.MiddleCenter;
            glyph.color=rim;
            glyph.raycastTarget=false;
        }
    }
}
#endif
