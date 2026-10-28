#if UNITY_EDITOR
using System;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Highfly.UI;
using Highfly.World;

namespace Highfly.Editor
{
    /// <summary>
    /// W2-C landscape-first immersive Unity RPG UI. Strictly native uGUI:
    /// proportional anchors, legitimate slots, live legal Quaternius portrait,
    /// zero HTML overlays, no proprietary ClaudeCraft UI art.
    /// </summary>
    public static class HighflyW2CPremiumInstaller
    {
        private static readonly Color Night = new Color(.020f,.030f,.061f,.985f);
        private static readonly Color Rail = new Color(.035f,.051f,.091f,1f);
        private static readonly Color Card = new Color(.064f,.085f,.145f,1f);
        private static readonly Color Deep = new Color(.042f,.059f,.103f,1f);
        private static readonly Color Accent = new Color(.33f,.79f,.99f,1f);
        private static readonly Color Gold = new Color(.95f,.72f,.38f,1f);
        private static readonly Color Label = new Color(.79f,.88f,.97f,1f);
        private static readonly Color Violet = new Color(.45f,.30f,.69f,1f);

        public static void Install(GameObject player, GameObject canvas)
        {
            if (player == null || canvas == null)
                throw new InvalidOperationException("W2-C needs Hunter and HUD.");
            HighflyHunterAppearance look = player.GetComponent<HighflyHunterAppearance>();
            if (look == null) throw new InvalidOperationException(
                "W2-C needs valid legal Quaternius variants.");
            HighflyW2WeaponVisual weapons = player.GetComponent<HighflyW2WeaponVisual>();
            if (weapons == null) weapons = player.AddComponent<HighflyW2WeaponVisual>();
            HighflyW2Progress progress = player.GetComponent<HighflyW2Progress>();
            if (progress == null) progress = player.AddComponent<HighflyW2Progress>();
            if (canvas.GetComponent<HighflyW2Menu>() != null) return;

            HighflyW2Menu menu = canvas.AddComponent<HighflyW2Menu>();
            // Compact, readable always-on entry. No second input system.
            Button open = ButtonAt(canvas.transform, "W2C_MORE", "MÁS  /  MENU",
                .45f,1f,.55f,1f, -10f,-99f,10f,-24f, Deep, Accent, 29);
            UnityEventTools.AddPersistentListener(open.onClick, menu.Toggle);

            GameObject fullscreen = Box(canvas.transform,"W2C_PREMIUM_FULLSCREEN",
                0,0,1,1,0,0,0,0,Night,true);
            // Header and a glowing thin accent strip.
            Box(fullscreen.transform,"W2C_Header",0,1,1,1,
                0,-150f,0,0,Rail,true);
            Box(fullscreen.transform,"W2C_TopLine",0,1,1,1,
                0,-153f,0,-147f,Accent,false);
            Text logo = TextAt(fullscreen.transform,"W2C_Highfly",
                "HIGHFLY  /  NEXUS",0,1,0,1,58,-112,550,-25,62,Gold);
            logo.fontStyle = FontStyle.Bold;
            Text sub = TextAt(fullscreen.transform,"W2C_Breadcrumb","HUNTER UNIVERSAL",
                .20f,1f,.83f,1f,0,-112,0,-31,35,Label);
            Button close = ButtonAt(fullscreen.transform,"W2C_Close","×  CERRAR",
                .87f,1f,.985f,1f,0,-125,0,-31,
                new Color(.30f,.11f,.19f,1f),Gold,32);
            UnityEventTools.AddPersistentListener(close.onClick,menu.Close);

            GameObject nav = Box(fullscreen.transform,"W2C_NavRail",
                0,0,0,1,0,35,555,-175,Rail,true);
            Text navTitle = TextAt(nav.transform,"W2C_RailTitle",
                "NAVEGACIÓN",0,1,1,1,50,-82,-45,-12,25,Gold);
            navTitle.alignment = TextAnchor.MiddleLeft;
            string[] tabs = {"01   HUNTER","02   MOCHILA","03   EQUIPO",
                "04   MISIONES","05   TRAINING","06   SISTEMA"};
            UnityAction[] callbacks = {
                menu.Personaje,menu.Inventario,menu.Equipo,
                menu.Misiones,menu.Entrenamiento,menu.Sistema };
            for (int i=0; i<tabs.Length; i++)
            {
                float y = -210f - i*146f;
                Button navButton = ButtonAt(nav.transform,
                    "W2C_TAB_" + i,tabs[i],
                    0f,1f,1f,1f,36,y-100f,-34f,y,
                    i==0 ? new Color(.11f,.25f,.39f,1f) : Deep,
                    i==0 ? Accent : new Color(.25f,.37f,.52f,1f),30);
                UnityEventTools.AddPersistentListener(navButton.onClick,callbacks[i]);
            }
            Text navFoot=TextAt(nav.transform,"W2C_RailFooter",
                "LV 01 — 20   /   SIN CLASES",0f,0f,1f,0f,
                22f,15f,-20f,90f,23,Label);
            navFoot.alignment=TextAnchor.MiddleCenter;

            GameObject contentRoot = Box(fullscreen.transform,"W2C_Content",
                0f,0f,1f,1f,575f,54f,-40f,-181f,Color.clear,false);
            Text title=TextAt(contentRoot.transform,"W2C_PageTitle","HUNTER",
                0f,1f,.55f,1f,16,-105f,0,-16f,57,Gold);
            title.fontStyle=FontStyle.Bold;
            title.alignment=TextAnchor.MiddleLeft;
            Text hint=TextAt(contentRoot.transform,"W2C_SectionHint",
                "HUNTER UNIVERSAL  /  PERFIL",.52f,1f,1f,1f,
                0,-92f,-15f,-20f,29,Label);
            GameObject pageArea=Box(contentRoot.transform,"W2C_MainArea",
                0,0,1,1,0,94f,0,-126f,Deep,false);

            // CHARACTER: two-column immersive live character model + stats.
            GameObject character=Box(pageArea.transform,"W2C_CharacterCard",
                0,0,1,1,0,0,0,0,Color.clear,false);
            GameObject portraitPanel=Box(character.transform,"W2C_PortraitFrame",
                0f,.04f,.44f,.97f,18f,18f,-17f,-17f,Card,true);
            Box(portraitPanel.transform,"W2C_PortraitTopGlow",
                0,1,1,1,3,-11,-3,-5,Accent,false);
            RawImage portrait = new GameObject("W2C_Live3D",
                typeof(RectTransform),typeof(CanvasRenderer),typeof(RawImage))
                .GetComponent<RawImage>();
            portrait.transform.SetParent(portraitPanel.transform,false);
            RectTransform rt=portrait.rectTransform;
            rt.anchorMin=new Vector2(.035f,.10f);
            rt.anchorMax=new Vector2(.965f,.89f);
            rt.offsetMin=rt.offsetMax=Vector2.zero;
            portrait.raycastTarget=false;
            Text previewLabel=TextAt(portraitPanel.transform,"W2C_PortraitLabel",
                "TU HUNTER · SKIN QUATERNIUS",0,0,1,0,15,10,-15,80,26,Gold);
            GameObject stats=Box(character.transform,"W2C_Stats",
                .47f,.04f,1f,.97f,0,18f,-18f,-17f,Card,true);
            Box(stats.transform,"W2C_StatsTrim",
                0,1,1,1,5,-11,-5,-5,Gold,false);
            Text statsHeading=TextAt(stats.transform,"W2C_StatsHeading",
                "PERFIL DEL CAZADOR",0,1,1,1,40,-104,-35,-22,37,Gold);
            statsHeading.alignment=TextAnchor.MiddleLeft;
            Text hunterStats=TextAt(stats.transform,"W2C_StatsInfo","",
                0,0,1,1,50,180,-35,-143,39,Label);
            hunterStats.alignment=TextAnchor.UpperLeft;
            Button nextLook=ButtonAt(stats.transform,"W2C_NextSkin",
                "CAMBIAR APARIENCIA",0,0,.55f,0,25,38,-12,152,
                Violet,Accent,28);
            UnityEventTools.AddPersistentListener(nextLook.onClick,menu.NextLook);
            Button save=ButtonAt(stats.transform,"W2C_SaveProfile",
                "GUARDAR PROGRESO",.58f,0,1f,0,0,38,-25,152,
                new Color(.10f,.30f,.28f,1f),Gold,27);
            UnityEventTools.AddPersistentListener(save.onClick,menu.SaveNow);

            // INVENTORY: 24 native slots + item detail/equip sidebar.
            GameObject inventory=Box(pageArea.transform,"W2C_SlotPage",
                0,0,1,1,0,0,0,0,Color.clear,false);
            GameObject backpack=Box(inventory.transform,"W2C_BackpackFrame",
                0f,.035f,.70f,.985f,18f,14f,-10f,-13f,Card,true);
            Text bagLabel=TextAt(backpack.transform,"W2C_BagLabel",
                "MOCHILA    24 / 24 SLOTS",0f,1f,1f,1f,32,-101,-30,-16,35,Gold);
            bagLabel.alignment=TextAnchor.MiddleLeft;
            GameObject layoutRoot=Box(backpack.transform,"W2C_IconGrid",
                0,0,1,1,44,70,-32,-132,Color.clear,false);
            GridLayoutGroup layout=layoutRoot.AddComponent<GridLayoutGroup>();
            layout.cellSize=new Vector2(197f,161f);
            layout.spacing=new Vector2(17f,22f);
            layout.startCorner=GridLayoutGroup.Corner.UpperLeft;
            layout.startAxis=GridLayoutGroup.Axis.Horizontal;
            layout.childAlignment=TextAnchor.UpperLeft;
            layout.constraint=GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount=6;
            layout.padding=new RectOffset(22,14,15,16);
            Sprite swordIcon=HighflyW2CIconArt.Get("sword");
            Sprite armorIcon=HighflyW2CIconArt.Get("armor");
            Sprite boneIcon=HighflyW2CIconArt.Get("bone");
            Sprite hideIcon=HighflyW2CIconArt.Get("hide");
            HighflyW2ItemSlot[] slots=new HighflyW2ItemSlot[24];
            for (int i=0;i<slots.Length;i++)
            {
                Button button=ButtonAt(layoutRoot.transform,"W2C_InventorySlot_"+i,"",
                    .5f,.5f,.5f,.5f,0,0,197,161,
                    new Color(.09f,.13f,.19f,1f),new Color(.24f,.42f,.60f,1f),22);
                Text n=TextAt(button.transform,"SlotNumber",
                    (i+1).ToString("00"),0,1,.5f,1,10,-33,0,-4,19,
                    new Color(.49f,.64f,.77f,1f));
                n.alignment=TextAnchor.MiddleLeft;
                Image icon=new GameObject("ItemIcon",typeof(RectTransform),
                    typeof(CanvasRenderer),typeof(Image)).GetComponent<Image>();
                icon.transform.SetParent(button.transform,false);
                RectTransform iconRect=icon.rectTransform;
                iconRect.anchorMin=new Vector2(.15f,.15f);
                iconRect.anchorMax=new Vector2(.85f,.85f);
                iconRect.offsetMin=iconRect.offsetMax=Vector2.zero;
                icon.preserveAspect=true;
                icon.raycastTarget=false;
                Text qty=TextAt(button.transform,"SlotAmount","",
                    .45f,0,1, .30f,0,0,-10,0,23,Gold);
                qty.alignment=TextAnchor.MiddleRight;
                HighflyW2ItemSlot slot=button.gameObject.AddComponent<HighflyW2ItemSlot>();
                slot.Configure(i,menu,button.GetComponent<Image>(),icon,qty,
                    swordIcon,armorIcon,boneIcon,hideIcon);
                UnityEventTools.AddPersistentListener(button.onClick,slot.Click);
                slots[i]=slot;
            }
            GameObject inspector=Box(inventory.transform,"W2C_Inspector",
                .71f,.035f,1f,.985f,6,14,-18,-13f,Card,true);
            Box(inspector.transform,"W2C_InspectorAccent",
                0,1,1,1,6,-11,-6,-5,Accent,false);
            Text inspectorTitle=TextAt(inspector.transform,"W2C_InspectorTitle",
                "DETALLES DEL OBJETO",0,1,1,1,22,-94,-22,-16,29,Gold);
            inspectorTitle.alignment=TextAnchor.MiddleLeft;
            Text inspectorBody=TextAt(inspector.transform,"W2C_InspectorText","",
                0,0,1,1,35,170,-35,-122,31,Label);
            inspectorBody.alignment=TextAnchor.UpperLeft;
            Button equipSelected=ButtonAt(inspector.transform,"W2C_EquipSelected",
                "EQUIPAR SELECCIÓN",0,0,1,0,32,38,-32,150,
                new Color(.15f,.27f,.41f,1f),Accent,29);
            UnityEventTools.AddPersistentListener(equipSelected.onClick,menu.EquipSelected);
            Button removeWeapon=ButtonAt(inspector.transform,"W2C_RemoveWeapon",
                "GUARDAR ESPADA",0,0,1,0,32,150,-32,248,Deep,Gold,25);
            UnityEventTools.AddPersistentListener(removeWeapon.onClick,menu.UnequipSword);

            GameObject prosePanel=Box(pageArea.transform,"W2C_StoryCard",
                0,0,1,1,18,14,-18,-12,Card,true);
            Box(prosePanel.transform,"W2C_StoryAccent",0,1,1,1,
                8,-11,-8,-5,Gold,false);
            Text prose=TextAt(prosePanel.transform,"W2C_StoryText","",
                0,0,1,1,105,110,-85,-102,44,Label);
            prose.alignment=TextAnchor.UpperLeft;

            GameObject statusBar=Box(fullscreen.transform,"W2C_StatusBar",
                0,0,1,0,0,0,0,50,new Color(.030f,.049f,.081f,1f),true);
            Text footer=TextAt(statusBar.transform,"W2C_Status",
                "FURY 360°  /  HUNTER UNIVERSAL  /  GUARDADO LOCAL",
                0,0,1,1,70,3,-70,-3,22,Gold);
            footer.alignment=TextAnchor.MiddleCenter;

            menu.Configure(progress,look,fullscreen,title,prose,footer);
            menu.ConfigureSlots(slots,inventory,inspectorBody);
            menu.ConfigurePremium(character,hint);
            menu.ConfigureHunterReadout(hunterStats,prosePanel);

            // Only the character page renders the isolated 3D portrait.
            HighflyW2CharacterPortrait live=canvas.AddComponent<HighflyW2CharacterPortrait>();
            live.Configure(look,progress,character,portrait);
            Transform oldLook=canvas.transform.Find("FURY_Appearance");
            if(oldLook!=null)oldLook.gameObject.SetActive(false);

            Text beaconText=TextAt(canvas.transform,"W2C_SerinTracker",
                "!  CONTRATO DEL GREMIO   /   SERIN",.21f,1f,.81f,1f,
                0,-194f,0,-128f,25,Gold);
            beaconText.alignment=TextAnchor.MiddleCenter;
            HighflyW2QuestBeacon beacon=canvas.AddComponent<HighflyW2QuestBeacon>();
            beacon.Configure(player.transform,progress,beaconText);
            HighflyW2CombatHudPolish.Apply(canvas);

            Debug.Log("HIGHFLY W2-C PREMIUM: 3D portrait, 24 icon slots, " +
                      "contextual inspector, full-screen mobile layout. " +
                      "FURY/skills preserved.");
        }

        private static GameObject Box(Transform parent,string name,
            float ax,float ay,float bx,float by,
            float lx,float ly,float rx,float ry,Color color,bool raycast)
        {
            GameObject go=new GameObject(name,typeof(RectTransform),
                typeof(CanvasRenderer),typeof(Image));
            go.transform.SetParent(parent,false);
            RectTransform r=go.GetComponent<RectTransform>();
            r.anchorMin=new Vector2(ax,ay);
            r.anchorMax=new Vector2(bx,by);
            r.offsetMin=new Vector2(lx,ly);
            r.offsetMax=new Vector2(rx,ry);
            Image img=go.GetComponent<Image>();
            img.color=color;
            img.raycastTarget=raycast;
            return go;
        }

        private static Text TextAt(Transform parent,string name,string text,
            float ax,float ay,float bx,float by,
            float lx,float ly,float rx,float ry,int size,Color color)
        {
            GameObject go=new GameObject(name,typeof(RectTransform),
                typeof(CanvasRenderer),typeof(Text));
            go.transform.SetParent(parent,false);
            RectTransform r=go.GetComponent<RectTransform>();
            r.anchorMin=new Vector2(ax,ay);
            r.anchorMax=new Vector2(bx,by);
            r.offsetMin=new Vector2(lx,ly);
            r.offsetMax=new Vector2(rx,ry);
            Text t=go.GetComponent<Text>();
            t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize=size;
            t.text=text;
            t.color=color;
            t.alignment=TextAnchor.MiddleCenter;
            t.horizontalOverflow=HorizontalWrapMode.Wrap;
            t.verticalOverflow=VerticalWrapMode.Truncate;
            t.raycastTarget=false;
            return t;
        }

        private static Button ButtonAt(Transform parent,string name,string label,
            float ax,float ay,float bx,float by,
            float lx,float ly,float rx,float ry,
            Color bg,Color border,int size)
        {
            GameObject go=Box(parent,name,ax,ay,bx,by,lx,ly,rx,ry,bg,true);
            Button button=go.AddComponent<Button>();
            ColorBlock state=button.colors;
            state.normalColor=Color.white;
            state.highlightedColor=new Color(.77f,.88f,1f,1f);
            state.pressedColor=new Color(.58f,.75f,.98f,1f);
            state.selectedColor=Color.white;
            button.colors=state;
            Outline outline=go.AddComponent<Outline>();
            outline.effectColor=border;
            outline.effectDistance=new Vector2(2f,-2f);
            if (!string.IsNullOrEmpty(label))
            {
                Text text=TextAt(go.transform,name+"_Title",label,0,0,1,1,
                    10,4,-10,-4,size,Label);
                text.fontStyle=FontStyle.Bold;
            }
            return button;
        }
    }
}
#endif
