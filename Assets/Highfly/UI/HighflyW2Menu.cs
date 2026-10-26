using UnityEngine;
using UnityEngine.UI;
using Highfly.World;

namespace Highfly.UI
{
    // One in-world menu, without class/spec selection or a second HUD engine.
    public sealed class HighflyW2Menu : MonoBehaviour
    {
        [SerializeField] private HighflyW2Progress progress;
        [SerializeField] private HighflyHunterAppearance appearance;
        [SerializeField] private GameObject panel;
        [SerializeField] private Text title;
        [SerializeField] private Text body;
        [SerializeField] private Text footer;
        private string _tab = "PERSONAJE";

        public void Configure(HighflyW2Progress state, HighflyHunterAppearance look,
            GameObject mainPanel, Text heading, Text detail, Text status)
        {
            if (progress != null) progress.Changed -= Refresh;
            progress = state;
            appearance = look;
            if (isActiveAndEnabled && progress != null)
                progress.Changed += Refresh;
            panel = mainPanel;
            title = heading;
            body = detail;
            footer = status;
            if (panel != null) panel.SetActive(false);
            Refresh();
        }

        private void OnEnable()
        {
            if (progress != null)
            {
                progress.Changed -= Refresh;
                progress.Changed += Refresh;
            }
        }

        private void OnDisable()
        {
            if (progress != null) progress.Changed -= Refresh;
        }

        public void Toggle()
        {
            if (panel == null) return;
            panel.SetActive(!panel.activeSelf);
            Refresh();
        }

        public void Close()
        {
            if (panel != null) panel.SetActive(false);
        }

        public void Personaje() { _tab = "PERSONAJE"; Refresh(); }
        public void Inventario() { _tab = "INVENTARIO"; Refresh(); }
        public void Equipo() { _tab = "EQUIPAMIENTO"; Refresh(); }
        public void Misiones() { _tab = "MISIONES"; Refresh(); }
        public void Entrenamiento() { _tab = "ENTRENAMIENTO"; Refresh(); }

        public void EquipSword()
        {
            if (progress == null) return;
            if (progress.Equip("sword_rookie"))
                SetFooter("Espada equipada · visible en la mano del Hunter");
            else SetFooter("No tenés esa espada en la mochila");
            Equipo();
        }

        public void UnequipSword()
        {
            if (progress == null) return;
            progress.UnequipWeapon();
            SetFooter("Espada desequipada");
            Equipo();
        }

        public void EquipArmor()
        {
            if (progress == null) return;
            if (progress.Equip("armor_rookie"))
                SetFooter("Armadura equipada: +3 VIT (sin cambiar la skin)");
            else SetFooter("Conseguí la armadura completando la misión de Serin");
            Equipo();
        }

        public void NextLook()
        {
            if (appearance == null) return;
            appearance.NextLook();
            SetFooter("Skin Quaternius: " + appearance.CurrentAppearance);
            Personaje();
        }

        public void SaveNow()
        {
            if (progress != null)
            {
                progress.Save();
                SetFooter("Progreso guardado en este navegador");
            }
        }

        public void Refresh()
        {
            if (progress == null || title == null || body == null) return;
            title.text = "HIGHFLY · " + _tab;
            if (_tab == "INVENTARIO")
                body.text = "MOCHILA\n\n" + progress.InventoryText() +
                    "\nOro: " + progress.Gold + "\n\nEl loot se recoge con USAR.";
            else if (_tab == "EQUIPAMIENTO")
                body.text = "ARMA: " + (string.IsNullOrEmpty(progress.Weapon)
                    ? "Ninguna" : HighflyW2Progress.FindItem(progress.Weapon).Name) +
                    "\nBonificación STR: +" + progress.StrengthBonus +
                    "\n\nARMADURA: " + (string.IsNullOrEmpty(progress.Armor)
                    ? "Ninguna" : HighflyW2Progress.FindItem(progress.Armor).Name) +
                    "\nBonificación VIT: +" + progress.VitalityBonus +
                    "\n\nLas armaduras modifican atributos, NO la apariencia. " +
                    "Las armas equipadas se muestran en Quaternius.";
            else if (_tab == "MISIONES")
                body.text = "GREMIO · CONTRATOS\n\n" + progress.QuestText +
                    "\n\nRecompensa: 75 oro, 80 XP, Armadura de Recluta.";
            else if (_tab == "ENTRENAMIENTO")
                body.text = "TRAINING CORE\n\nSTR / AGI / VIT / PER / INT" +
                    "\n\nEl entrenamiento real se integrará en W2 siguiente etapa. " +
                    "No inventamos puntos ni ejercicios completados.";
            else
                body.text = "HUNTER UNIVERSAL · SIN CLASES\n\nNivel: " +
                    progress.Level + "/20\nXP: " + progress.Experience +
                    "\nOro: " + progress.Gold +
                    "\nBonus STR: +" + progress.StrengthBonus +
                    " · VIT: +" + progress.VitalityBonus +
                    "\n\nSkin: " + (appearance != null ? appearance.CurrentAppearance : "Quaternius") +
                    "\n\nElegí tu apariencia libremente: no cambia el equipo ni tus atributos.";
        }

        private void SetFooter(string text)
        {
            if (footer != null) footer.text = text;
        }
    }
}
