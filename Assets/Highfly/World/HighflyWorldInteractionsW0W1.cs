using UnityEngine;

namespace Highfly.World
{
    [RequireComponent(typeof(Collider))]
    public sealed class HighflyWorldSceneDoor : HighflyInteractable
    {
        [SerializeField] private string interiorId;
        [SerializeField] private string exteriorAnchorId;

        public void Configure(
            string targetInteriorId,
            string anchorId,
            string buildingName,
            string actionPrompt)
        {
            interiorId = targetInteriorId;
            exteriorAnchorId = anchorId;
            displayName = buildingName;
            prompt = actionPrompt;
        }

        public override void Interact(HighflyInteractionController controller)
        {
            HighflyWorldRuntimeHost host = HighflyWorldRuntimeHost.Current;
            if (host == null || host.SceneService == null)
            {
                if (controller != null)
                    controller.ShowDialogue("WORLD W0", "World Scene Service no disponible.");
                return;
            }

            host.SceneService.EnterInterior(interiorId, exteriorAnchorId, controller);
        }
    }

    [RequireComponent(typeof(Collider))]
    public sealed class HighflyWorldExitDoor : HighflyInteractable
    {
        public void Configure(string placeName)
        {
            displayName = placeName;
            prompt = "SALIR";
        }

        public override void Interact(HighflyInteractionController controller)
        {
            HighflyWorldRuntimeHost host = HighflyWorldRuntimeHost.Current;
            if (host != null && host.SceneService != null)
                host.SceneService.ExitInterior(controller);
        }
    }

    [RequireComponent(typeof(Collider))]
    public sealed class HighflyW1ServiceStation : HighflyInteractable
    {
        [SerializeField] private string serviceType;

        public void Configure(
            string serviceId,
            string stationName,
            string actionPrompt)
        {
            serviceType = serviceId;
            displayName = stationName;
            prompt = actionPrompt;
        }

        public override void Interact(HighflyInteractionController controller)
        {
            if (controller == null)
                return;

            string key = (serviceType ?? "").ToLowerInvariant();

            if (key.Contains("inn") || key.Contains("rest") || key.Contains("tavern"))
            {
                controller.RestoreAll();
                controller.ShowDialogue(
                    "POSADA — W1",
                    "Descanso confirmado. HP, MP y stamina restaurados. " +
                    "El edificio ya funciona como interior independiente y conserva el retorno a la ciudad.");
                return;
            }

            if (key.Contains("guild"))
            {
                controller.ShowDialogue(
                    "GREMIO — W1",
                    "Terminal del Gremio operativa. W1 valida acceso, interior y servicio. " +
                    "Quest Board, contratos y Portal Forecast se conectan en W2 sin cambiar esta escena.");
                return;
            }

            if (key.Contains("forge") || key.Contains("smith"))
            {
                controller.ShowDialogue(
                    "FORJA — W1",
                    "Estación de Forja operativa como endpoint de mundo. " +
                    "El bridge de recetas/inventario HIGHFLY se conecta después sin sustituir el Combat Core.");
                return;
            }

            if (key.Contains("market"))
            {
                controller.ShowDialogue(
                    "MERCADO — W1",
                    "Puesto de Mercado operativo como endpoint de servicio. " +
                    "Vendor/economía y NPCs se conectan en W2 sobre este mismo contrato.");
                return;
            }

            controller.ShowDialogue("WORLD W1", "Servicio registrado: " + serviceType);
        }
    }
}
