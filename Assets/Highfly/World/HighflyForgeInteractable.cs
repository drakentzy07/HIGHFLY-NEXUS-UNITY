using UnityEngine;

namespace Highfly.World
{
    public enum HighflyForgeActionKind
    {
        Blacksmith,
        Anvil,
        Furnace,
        Workbench,
        WeaponDisplay,
        MaterialStorage
    }

    [RequireComponent(typeof(Collider))]
    public sealed class HighflyForgeInteractable : HighflyInteractable
    {
        [SerializeField] private HighflyForgeActionKind actionKind;

        public void Configure(
            HighflyForgeActionKind kind,
            string label,
            string actionPrompt)
        {
            actionKind = kind;
            displayName = label;
            prompt = actionPrompt;
        }

        public override void Interact(HighflyInteractionController controller)
        {
            if (controller == null)
                return;

            switch (actionKind)
            {
                case HighflyForgeActionKind.Blacksmith:
                    controller.ShowDialogue(
                        "HERRERO — FORJA HIGHFLY",
                        "¿Qué necesitás?\n\n" +
                        "• HABLAR — información, rumores y recetas.\n" +
                        "• COMPRAR / VENDER — materiales y equipo del herrero.\n" +
                        "• REPARAR — servicio preparado para Equipment Core.\n\n" +
                        "El NPC ya es un endpoint separado del yunque, horno y banco de trabajo.");
                    break;

                case HighflyForgeActionKind.Anvil:
                    controller.ShowDialogue(
                        "YUNQUE — FORJAR / MEJORAR",
                        "Punto de trabajo de Blacksmithing.\n\n" +
                        "FORJAR: convierte materiales + receta en equipo.\n" +
                        "MEJORAR: aplica el pipeline 0 / 25 / 50 / 75 / 100%.\n\n" +
                        "La ejecución de recetas se conecta al Loot & Crafting Core; " +
                        "este objeto ya posee su interacción propia.");
                    break;

                case HighflyForgeActionKind.Furnace:
                    controller.ShowDialogue(
                        "HORNO — FUNDIR",
                        "Punto de procesamiento de minerales.\n\n" +
                        "Mineral → lingote / componente refinado.\n" +
                        "Aquí se conectarán combustible, tiempo de proceso y recipes.");
                    break;

                case HighflyForgeActionKind.Workbench:
                    controller.ShowDialogue(
                        "BANCO DE TRABAJO — CRAFTEAR",
                        "Mesa de ensamblaje para componentes, empuñaduras, " +
                        "reparaciones y recetas auxiliares.");
                    break;

                case HighflyForgeActionKind.WeaponDisplay:
                    controller.ShowDialogue(
                        "EXHIBIDOR DE ARMAS",
                        "Inspección de armas disponibles.\n\n" +
                        "Este punto queda separado del vendedor para permitir " +
                        "previsualización, comparación y lore del objeto.");
                    break;

                case HighflyForgeActionKind.MaterialStorage:
                    controller.ShowDialogue(
                        "ALMACÉN DE MATERIALES",
                        "Zona de minerales, lingotes y componentes.\n\n" +
                        "Preparada para Storage / Inventory Core sin convertir " +
                        "la decoración en un menú global.");
                    break;
            }
        }
    }
}
