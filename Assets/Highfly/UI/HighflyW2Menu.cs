using UnityEngine;
using UnityEngine.UI;
using Highfly.World;

namespace Highfly.UI
{
    /// <summary>
    /// Native full-screen action-RPG menu controller. The old W2 inventory
    /// and save schema stay intact. No class restrictions or external UI.
    /// </summary>
    public sealed class HighflyW2Menu : MonoBehaviour
    {
        [SerializeField] private HighflyW2Progress progress;
        [SerializeField] private HighflyHunterAppearance appearance;
        [SerializeField] private GameObject panel;
        [SerializeField] private Text title;
        [SerializeField] private Text body;
        [SerializeField] private Text footer;
        private string _tab = "PERSONAJE";
        private HighflyW2ItemSlot[] _slots;
        private GameObject _inventoryGrid;
        private GameObject _portraitCard;
        private Text _gridReadout;
        private Text _subtitle;
        private Text _hunterReadout;
        private GameObject _textPage;

        public void ConfigureHunterReadout(Text readout, GameObject otherPages)
        {
            _hunterReadout = readout;
            _textPage = otherPages;
            Refresh();
        }
        private string _selectedItemId = "";

        public void Configure(HighflyW2Progress state, HighflyHunterAppearance look,
            GameObject fullscreen, Text heading, Text detail, Text status)
        {
            if (progress != null) progress.Changed -= Refresh;
            progress = state;
            appearance = look;
            panel = fullscreen;
            title = heading;
            body = detail;
            footer = status;
            if (progress != null) progress.Changed += Refresh;
            if (panel != null) panel.SetActive(false);
            Refresh();
        }

        public void ConfigureSlots(HighflyW2ItemSlot[] slots, GameObject grid, Text readout)
        {
            _slots = slots;
            _inventoryGrid = grid;
            _gridReadout = readout;
            Refresh();
        }

        public void ConfigurePremium(GameObject characterCard, Text subtitle)
        {
            _portraitCard = characterCard;
            _subtitle = subtitle;
            Refresh();
        }

        private void OnDestroy()
        {
            if (progress != null) progress.Changed -= Refresh;
        }

        public void Toggle()
        {
            if (panel == null) return;
            panel.SetActive(!panel.activeSelf);
            Refresh();
        }

        public void Close() { if (panel != null) panel.SetActive(false); }
        public void Personaje() { _tab = "PERSONAJE"; Refresh(); }
        public void Inventario() { _tab = "INVENTARIO"; Refresh(); }
        public void Equipo() { _tab = "EQUIPAMIENTO"; Refresh(); }
        public void Misiones() { _tab = "MISIONES"; Refresh(); }
        public void Entrenamiento() { _tab = "ENTRENAMIENTO"; Refresh(); }
        public void Sistema() { _tab = "SISTEMA"; Refresh(); }

        public void SelectItem(int index)
        {
            if (progress == null) return;
            _selectedItemId = progress.GetItemIdAtSlot(index);
            SetFooter(string.IsNullOrEmpty(_selectedItemId)
                ? "Ranura vacía. Elegí otro objeto."
                : "OBJETO SELECCIONADO: " +
                  HighflyW2Progress.FindItem(_selectedItemId).Name);
            Refresh();
        }

        public void EquipSelected()
        {
            if (progress == null || string.IsNullOrEmpty(_selectedItemId)) return;
            HighflyW2Progress.Item item =
                HighflyW2Progress.FindItem(_selectedItemId);
            if (item.Category != "arma" && item.Category != "armadura")
            {
                SetFooter("Material recolectado. El crafting llegará en W3.");
                return;
            }
            if (progress.Equip(_selectedItemId))
                SetFooter("EQUIPADO · " + item.Name +
                    " · Apariencia Quaternius independiente.");
            Refresh();
        }

        public void EquipSword()
        {
            if (progress == null) return;
            SetFooter(progress.Equip("sword_rookie") ?
                "ESPADA EQUIPADA · +STR" : "No tenés esa espada.");
            Refresh();
        }

        public void UnequipSword()
        {
            if (progress == null) return;
            progress.UnequipWeapon();
            SetFooter("Arma retirada del Hunter.");
            Refresh();
        }

        public void EquipArmor()
        {
            if (progress == null) return;
            SetFooter(progress.Equip("armor_rookie") ?
                "ARMADURA EQUIPADA · +3 VIT · skin sin cambios." :
                "Conseguí la armadura completando la misión de Serin.");
            Refresh();
        }

        public void NextLook()
        {
            if (appearance == null) return;
            appearance.NextLook();
            Personaje();
            SetFooter("APARIENCIA · " + appearance.CurrentAppearance);
        }

        // Old W2-B public callback remains compatible but is not present in
        // the W2-C production UI: the 90° workaround must not recur.
        public void RotateFacing() { SetFooter("Alineación automática W2-C activa."); }

        public void SaveNow()
        {
            if (progress == null) return;
            progress.Save();
            SetFooter("PROGRESO GUARDADO EN ESTE NAVEGADOR.");
        }

        public void Refresh()
        {
            if (progress == null || title == null || body == null) return;
            title.text = _tab == "PERSONAJE" ? "HUNTER" :
                _tab == "INVENTARIO" ? "MOCHILA" :
                _tab == "EQUIPAMIENTO" ? "ARSENAL" :
                _tab == "MISIONES" ? "CONTRATOS" :
                _tab == "SISTEMA" ? "SISTEMA" : "TRAINING CORE";
            bool hasSlots = _tab == "INVENTARIO" || _tab == "EQUIPAMIENTO";
            if (_inventoryGrid != null) _inventoryGrid.SetActive(hasSlots);
            if (_portraitCard != null) _portraitCard.SetActive(_tab == "PERSONAJE");
            if (_textPage != null)
                _textPage.SetActive(!hasSlots && _tab != "PERSONAJE");
            body.gameObject.SetActive(!hasSlots && _tab != "PERSONAJE");
            if (_subtitle != null)
                _subtitle.text = _tab == "PERSONAJE" ? "HUNTER UNIVERSAL  /  PERFIL" :
                    _tab == "INVENTARIO" ? "24 RANURAS  /  INSPECCIONÁ Y EQUIPÁ" :
                    _tab == "EQUIPAMIENTO" ? "SIN CLASES  /  EQUIPO Y APARIENCIAS" :
                    _tab == "MISIONES" ? "GREMIO  /  HISTORIAL DE AVENTURAS" :
                    _tab == "SISTEMA" ? "GUARDADO Y CONFIGURACIÓN" :
                    "VIDA REAL  /  PRÓXIMA INTEGRACIÓN";

            if (_hunterReadout != null)
                _hunterReadout.text =
                    "NIVEL  " + progress.Level + " / 20" +
                    "\\nXP ACTUAL  " + progress.Experience +
                    "\\nORO  " + progress.Gold +
                    "\\n\\nFUERZA  +" + progress.StrengthBonus +
                    "\\nVITALIDAD  +" + progress.VitalityBonus +
                    "\\n\\nAPARIENCIA\\n" +
                    (appearance != null ? appearance.CurrentAppearance :
                        "Quaternius CC0") +
                    "\\n\\nSIN CLASES  ·  SKIN LIBRE";
            if (_slots != null)
                for (int i = 0; i < _slots.Length; i++)
                    if (_slots[i] != null)
                        _slots[i].Present(progress.GetItemIdAtSlot(i),
                            progress.GetStackCountAtSlot(i));

            if (_gridReadout != null)
            {
                HighflyW2Progress.Item selected =
                    HighflyW2Progress.FindItem(_selectedItemId);
                _gridReadout.text = !string.IsNullOrEmpty(selected.Id)
                    ? selected.Name.ToUpperInvariant() +
                      "\n\nTIPO   " + selected.Category.ToUpperInvariant() +
                      "\nCANTIDAD   " + progress.Count(selected.Id) +
                      "\n\nFUERZA   +" + selected.Strength +
                      "\nVITALIDAD   +" + selected.Vitality +
                      "\n\n" + (selected.Category == "arma"
                        ? "Se muestra en la mano del Hunter."
                        : selected.Category == "armadura"
                        ? "Solo estadísticas: la skin no cambia."
                        : "Material recolectado para crafting.") +
                      "\n\nEQUIPÁ DESDE ESTA FICHA"
                    : "SELECCIONÁ UN OBJETO\n\nTocá uno de los slots de la mochila." +
                      "\n\nORO  " + progress.Gold +
                      "\n\nARMA   " + (string.IsNullOrEmpty(progress.Weapon)
                          ? "Ninguna" : HighflyW2Progress.FindItem(progress.Weapon).Name) +
                      "\n\nARMADURA   " + (string.IsNullOrEmpty(progress.Armor)
                          ? "Ninguna" : HighflyW2Progress.FindItem(progress.Armor).Name);
            }

            if (_tab == "MISIONES")
                body.text = "01   /   EL PRIMER CONTRATO\n\n" + progress.QuestText +
                    "\n\nOBJETIVO   ·   2 ESQUELETOS\nRECOMPENSA   ·   75 ORO" +
                    "\nEXPERIENCIA   ·   80 XP\nEQUIPO   ·   ARMADURA DE RECLUTA";
            else if (_tab == "ENTRENAMIENTO")
                body.text = "TRAINING CORE\n\nSTR   /   AGI   /   VIT   /   PER   /   INT" +
                    "\n\nPróxima integración: entrenamiento real, métricas y medallas." +
                    "\n\nNo simulamos entrenamientos ni estadísticas inexistentes.";
            else if (_tab == "SISTEMA")
                body.text = "HIGHFLY NEXUS  /  SISTEMA\\n\\nGuardado local activo." +
                    "\\nLos datos se conservan en este navegador." +
                    "\\n\\nW2-C PRESERVA FURY 360°  /  CÁMARA  /  COMBATE.";
            else
                body.text = "";
        }

        private void SetFooter(string message)
        {
            if (footer != null) footer.text = message;
        }
    }
}
