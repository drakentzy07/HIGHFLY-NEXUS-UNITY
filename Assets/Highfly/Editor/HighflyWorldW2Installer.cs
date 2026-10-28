#if UNITY_EDITOR
using System;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Highfly.World;
using Highfly.UI;

namespace Highfly.Editor
{
    /// <summary>
    /// In-Unity full viewport RPG panel. No HTML overlay, no new movement system.
    /// Large 24-cell native backpack, left navigation and right item details.
    /// </summary>
    public static class HighflyWorldW2Installer
    {
        private static readonly Color Navy = new Color(.024f, .038f, .070f, .985f);
        private static readonly Color Panel = new Color(.065f, .090f, .155f, .99f);
        private static readonly Color Gold = new Color(1f, .79f, .40f, 1f);
        private static readonly Color Ice = new Color(.60f, .82f, 1f, 1f);

        public static void Install(GameObject player, GameObject canvas)
        {
            if (player == null || canvas == null)
                throw new ArgumentNullException("W2 requires Hunter and HUD");
            HighflyHunterAppearance appearance =
                player.GetComponent<HighflyHunterAppearance>();
            if (appearance == null)
                throw new InvalidOperationException("W2 needs legal Quaternius skins");
            HighflyW2WeaponVisual weapon = player.GetComponent<HighflyW2WeaponVisual>();
            if (weapon == null) weapon = player.AddComponent<HighflyW2WeaponVisual>();
            HighflyW2Progress state = player.GetComponent<HighflyW2Progress>();
            if (state == null) state = player.AddComponent<HighflyW2Progress>();

            if (canvas.GetComponent<HighflyW2Menu>() != null) return;
            HighflyW2Menu menu = canvas.AddComponent<HighflyW2Menu>();
            Button more = MakeButton(canvas.transform, "W2_MORE", "MÁS",
                new Vector2(.5f, 1f), new Vector2(0f, -70f),
                new Vector2(226f, 74f), new Color(.12f, .17f, .28f, .98f));
            UnityEventTools.AddPersistentListener(more.onClick, menu.Toggle);

            GameObject fullscreen = MakeBox(canvas.transform, "W2_PREMIUM_FULLSCREEN",
                Vector2.zero, Vector2.zero, Vector2.zero, Navy);
            RectTransform all = fullscreen.GetComponent<RectTransform>();
            all.anchorMin = Vector2.zero;
            all.anchorMax = Vector2.one;
            all.offsetMin = Vector2.zero;
            all.offsetMax = Vector2.zero;
            // Background catches all pointer input when open, preventing
            // taps from also operating the camera, attack or jump.
            Image overlay = fullscreen.GetComponent<Image>();
            overlay.raycastTarget = true;

            MakeBox(fullscreen.transform, "W2_TopChrome",
                new Vector2(.5f, 1f), new Vector2(0f, -74f),
                new Vector2(3088f, 148f), new Color(.08f,.12f,.20f,1f));
            Text logo = MakeText(fullscreen.transform, "W2_Logo",
                new Vector2(-1020f, 590f), new Vector2(1020f, 105f), 57,
                "HIGHFLY  /  HUNTER");
            logo.color = Gold;
            logo.fontStyle = FontStyle.Bold;
            logo.alignment = TextAnchor.MiddleLeft;
            Text title = MakeText(fullscreen.transform, "W2_WindowTitle",
                new Vector2(235f, 585f), new Vector2(1120f, 100f), 43, "");
            title.color = Ice;
            title.alignment = TextAnchor.MiddleLeft;
            AddButton(fullscreen.transform,"W2_Close","CERRAR  ✕",
                new Vector2(1265f,595f),new Vector2(330f,86f),
                new Color(.40f,.13f,.20f,1f),menu.Close);

            // Left native RPG navigation rail.
            MakeBox(fullscreen.transform,"W2_LeftRail",Vector2.zero,
                new Vector2(-1175f,-45f),new Vector2(640f,1060f),Panel);
            string[] tabs={"PERSONAJE","INVENTARIO","EQUIPAMIENTO","MISIONES","ENTRENAMIENTO"};
            UnityAction[] actions={menu.Personaje,menu.Inventario,menu.Equipo,
                menu.Misiones,menu.Entrenamiento};
            for(int i=0;i<tabs.Length;i++)
                AddButton(fullscreen.transform,"W2_Tab_"+i,tabs[i],
                    new Vector2(-1175f,353f-i*166f),
                    new Vector2(535f,129f),
                    i==1 ? new Color(.14f,.26f,.43f,.98f) :
                    new Color(.11f,.17f,.28f,.98f),actions[i]);

            // Text pages have their OWN immersive panel (not a web paragraph).
            GameObject detailCard = MakeBox(fullscreen.transform,"W2_Page",
                new Vector2(.5f,.5f),new Vector2(245f,-35f),
                new Vector2(2055f,1010f),Panel);
            Text detail = MakeText(detailCard.transform,"W2_Detail",
                new Vector2(0f,40f),new Vector2(1790f,720f),43,"");
            detail.alignment = TextAnchor.UpperLeft;

            // Real 6 x 4 native Unity inventory slots and right-side inspector.
            GameObject grid = MakeBox(fullscreen.transform,"W2_SlotsAndEquipment",
                new Vector2(.5f,.5f),new Vector2(215f,-28f),
                new Vector2(2120f,1060f),Panel);
            Text gridHeading = MakeText(grid.transform,"W2_GridHeading",
                new Vector2(-240f,455f),new Vector2(1350f,65f),38,
                "MOCHILA     /     OBJETOS Y EQUIPO");
            gridHeading.color = Gold;
            HighflyW2ItemSlot[] slots = new HighflyW2ItemSlot[24];
            for(int row=0; row<4; row++)
                for(int col=0; col<6; col++)
                {
                    int index=row*6+col;
                    Vector2 at = new Vector2(-845f+col*230f,310f-row*206f);
                    Button b=MakeButton(grid.transform,"W2_SLOT_"+index,"",
                        new Vector2(.5f,.5f),at,new Vector2(210f,180f),
                        new Color(.10f,.16f,.25f,.94f));
                    // Legacy W2-B fallback still has to compile even when
                    // the new W2-C premium installer owns the active scene.
                    GameObject iconGo=new GameObject("W2C_Compatible_Icon",
                        typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
                    iconGo.transform.SetParent(b.transform,false);
                    RectTransform iconRect=iconGo.GetComponent<RectTransform>();
                    iconRect.anchorMin=new Vector2(.16f,.16f);
                    iconRect.anchorMax=new Vector2(.84f,.84f);
                    iconRect.offsetMin=iconRect.offsetMax=Vector2.zero;
                    Image icon=iconGo.GetComponent<Image>();
                    icon.raycastTarget=false;
                    icon.preserveAspect=true;
                    Text count=MakeText(b.transform,"Quantity",
                        new Vector2(66f,-65f),new Vector2(90f,45f),23,"");
                    count.color=Gold;
                    HighflyW2ItemSlot slot=b.gameObject.AddComponent<HighflyW2ItemSlot>();
                    slot.Configure(index,menu,b.GetComponent<Image>(),icon,count,
                        HighflyW2CIconArt.Get("sword"),
                        HighflyW2CIconArt.Get("armor"),
                        HighflyW2CIconArt.Get("bone"),
                        HighflyW2CIconArt.Get("hide"));
                    UnityEventTools.AddPersistentListener(b.onClick,slot.Click);
                    slots[index]=slot;
                }
            GameObject infoPanel=MakeBox(grid.transform,"W2_Inspector",
                new Vector2(.5f,.5f),new Vector2(720f,-30f),
                new Vector2(655f,845f),new Color(.10f,.15f,.25f,.98f));
            Text info=MakeText(infoPanel.transform,"W2_ItemInfo",
                new Vector2(0f,-5f),new Vector2(574f,770f),31,"");
            info.alignment=TextAnchor.UpperLeft;
            info.color=new Color(.92f,.95f,1f,1f);

            // One bottom action bar shared by pages, not a wall of controls.
            MakeBox(fullscreen.transform,"W2_BottomChrome",
                new Vector2(.5f,0f),new Vector2(0f,77f),
                new Vector2(3088f,154f),new Color(.05f,.09f,.15f,1f));
            AddButton(fullscreen.transform,"W2_EquipSword","EQUIPAR ARMA",
                new Vector2(-1120f,-620f),new Vector2(380f,90f),
                new Color(.14f,.27f,.42f,.99f),menu.EquipSword);
            AddButton(fullscreen.transform,"W2_UnequipSword","QUITAR ARMA",
                new Vector2(-698f,-620f),new Vector2(380f,90f),
                new Color(.12f,.18f,.30f,.99f),menu.UnequipSword);
            AddButton(fullscreen.transform,"W2_EquipArmor","EQUIPAR ARMADURA",
                new Vector2(-251f,-620f),new Vector2(430f,90f),
                new Color(.17f,.20f,.39f,.99f),menu.EquipArmor);
            AddButton(fullscreen.transform,"W2_NextSkin","CAMBIAR SKIN",
                new Vector2(207f,-620f),new Vector2(405f,90f),
                new Color(.22f,.15f,.40f,.99f),menu.NextLook);
            AddButton(fullscreen.transform,"W2_AlignRig","ALINEAR +90°",
                new Vector2(643f,-620f),new Vector2(390f,90f),
                new Color(.15f,.21f,.35f,.99f),menu.RotateFacing);
            AddButton(fullscreen.transform,"W2_Save","GUARDAR",
                new Vector2(1150f,-620f),new Vector2(290f,90f),
                new Color(.15f,.34f,.28f,.99f),menu.SaveNow);
            Text footer=MakeText(fullscreen.transform,"W2_Footer",
                new Vector2(0f,-514f),new Vector2(2600f,50f),26,
                "PROGRESO LOCAL · CLASES LIBRES · SKINS INDEPENDIENTES");
            footer.color=Gold;
            menu.Configure(state,appearance,fullscreen,title,detail,footer);
            menu.ConfigureSlots(slots,grid,info);
            Transform oldSkin=canvas.transform.Find("FURY_Appearance");
            if(oldSkin!=null)oldSkin.gameObject.SetActive(false);
            // The quest compass remains visible during normal play so the
            // first NPC cannot silently disappear in a large RG POLY city.
            Text tracker=MakeText(canvas.transform,"W2_QuestCompass",
                new Vector2(0f,485f),new Vector2(1680f,70f),28,
                "!  NUEVO CONTRATO · SERIN");
            tracker.fontStyle=FontStyle.Bold;
            tracker.color=Gold;
            tracker.alignment=TextAnchor.MiddleCenter;
            HighflyW2QuestBeacon beacon=canvas.AddComponent<HighflyW2QuestBeacon>();
            beacon.Configure(player.transform,state,tracker);
            HighflyW2CombatHudPolish.Apply(canvas);
            Debug.Log("HIGHFLY W2-B premium full-screen 24-slot UI installed");
        }

        private static GameObject MakeBox(Transform parent,string name,
            Vector2 anchor,Vector2 at,Vector2 size,Color color)
        {
            GameObject go=new GameObject(name,typeof(RectTransform),
                typeof(CanvasRenderer),typeof(Image));
            go.transform.SetParent(parent,false);
            RectTransform r=go.GetComponent<RectTransform>();
            r.anchorMin=r.anchorMax=anchor;
            r.pivot=new Vector2(.5f,.5f);
            r.anchoredPosition=at;
            r.sizeDelta=size;
            Image img=go.GetComponent<Image>();
            img.color=color;
            return go;
        }

        private static Button MakeButton(Transform parent,string name,string label,
            Vector2 anchor,Vector2 pos,Vector2 size,Color color)
        {
            GameObject go=MakeBox(parent,name,anchor,pos,size,color);
            Button b=go.AddComponent<Button>();
            if (!string.IsNullOrEmpty(label))
            {
                Text text=MakeText(go.transform,name+"_Text",Vector2.zero,
                    size-new Vector2(12f,12f),28,label);
                text.fontStyle=FontStyle.Bold;
            }
            Outline border=go.AddComponent<Outline>();
            border.effectColor=new Color(.42f,.62f,.86f,.44f);
            border.effectDistance=new Vector2(2f,-2f);
            return b;
        }

        private static void AddButton(Transform parent,string name,string label,
            Vector2 at,Vector2 size,Color color,UnityAction action)
        {
            Button button=MakeButton(parent,name,label,new Vector2(.5f,.5f),at,size,color);
            UnityEventTools.AddPersistentListener(button.onClick,action);
        }

        private static Text MakeText(Transform parent,string name,
            Vector2 pos,Vector2 size,int fontsize,string content)
        {
            GameObject go=new GameObject(name,typeof(RectTransform),
                typeof(CanvasRenderer),typeof(Text));
            go.transform.SetParent(parent,false);
            RectTransform r=go.GetComponent<RectTransform>();
            r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);
            r.pivot=new Vector2(.5f,.5f);
            r.anchoredPosition=pos;
            r.sizeDelta=size;
            Text t=go.GetComponent<Text>();
            t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize=fontsize;
            t.text=content;
            t.color=Color.white;
            t.alignment=TextAnchor.MiddleCenter;
            t.horizontalOverflow=HorizontalWrapMode.Wrap;
            t.verticalOverflow=VerticalWrapMode.Overflow;
            t.raycastTarget=false;
            Shadow shadow=go.AddComponent<Shadow>();
            shadow.effectColor=new Color(0f,0f,0f,.77f);
            shadow.effectDistance=new Vector2(2f,-2f);
            return t;
        }
    }
}
#endif
