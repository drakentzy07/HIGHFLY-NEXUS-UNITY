using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Highfly.World
{
    /// <summary>
    /// Persistent W0 host. It is deliberately tiny: registry + scene transition service.
    /// HIGHFLY/LUCID continues to own player movement, camera, input and combat.
    /// </summary>
    public sealed class HighflyWorldRuntimeHost : MonoBehaviour
    {
        private const string EnableMarker = "WorldFinal/world_final_enabled";

        public static HighflyWorldRuntimeHost Current { get; private set; }

        public HighflyWorldRegistry Registry { get; private set; }
        public HighflyWorldSceneService SceneService { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBoot()
        {
            if (Resources.Load<TextAsset>(EnableMarker) == null)
                return;

            if (Current != null)
                return;

            HighflyWorldRuntimeHost existing = FindObjectOfType<HighflyWorldRuntimeHost>();
            if (existing != null)
            {
                Current = existing;
                return;
            }

            GameObject go = new GameObject("HIGHFLY_WORLD_CORE_W0");
            DontDestroyOnLoad(go);
            go.AddComponent<HighflyWorldRuntimeHost>();
        }

        private void Awake()
        {
            if (Current != null && Current != this)
            {
                Destroy(gameObject);
                return;
            }

            Current = this;
            DontDestroyOnLoad(gameObject);

            Registry = GetComponent<HighflyWorldRegistry>();
            if (Registry == null)
                Registry = gameObject.AddComponent<HighflyWorldRegistry>();

            SceneService = GetComponent<HighflyWorldSceneService>();
            if (SceneService == null)
                SceneService = gameObject.AddComponent<HighflyWorldSceneService>();

        }

        private void OnDestroy()
        {
            if (Current == this)
                Current = null;
        }
    }

    /// <summary>
    /// W0 scene authority. Interiors are real additive Unity scenes included in
    /// the player build. The Hunter/camera remain in the exterior host scene.
    /// </summary>
    public sealed class HighflyWorldSceneService : MonoBehaviour
    {
        private HighflyWorldRegistry _registry;
        private readonly HighflyWorldReturnState _returnState = new HighflyWorldReturnState();

        private bool _transitioning;
        private string _activeInteriorId;
        private string _activeInteriorScene;

        public bool IsTransitioning { get { return _transitioning; } }
        public bool IsInsideInterior { get { return !string.IsNullOrEmpty(_activeInteriorId); } }
        public string ActiveInteriorId { get { return _activeInteriorId ?? ""; } }

        private void Awake()
        {
            _registry = GetComponent<HighflyWorldRegistry>();
        }

        public void EnterInterior(
            string interiorId,
            string exteriorAnchorId,
            HighflyInteractionController controller)
        {
            if (_transitioning || IsInsideInterior || controller == null)
                return;

            HighflyInteriorDefinition definition;
            if (_registry == null || !_registry.TryGetInterior(interiorId, out definition) || definition == null)
            {
                controller.ShowDialogue("WORLD W0", "Interior no registrado: " + interiorId);
                return;
            }

            StartCoroutine(EnterInteriorRoutine(definition, exteriorAnchorId, controller));
        }

        private IEnumerator EnterInteriorRoutine(
            HighflyInteriorDefinition definition,
            string exteriorAnchorId,
            HighflyInteractionController controller)
        {
            _transitioning = true;

            _returnState.valid = true;
            _returnState.exteriorSceneName = controller.gameObject.scene.name;
            _returnState.exteriorAnchorId = exteriorAnchorId ?? definition.returnAnchorId ?? "";
            _returnState.position = controller.transform.position;
            _returnState.rotation = controller.transform.rotation;
            _returnState.worldPhase = "W0";

            HighflyWorldTransitionEvent evt = new HighflyWorldTransitionEvent(
                HighflyWorldTransitionKind.EnterInterior,
                _returnState.exteriorSceneName,
                definition.id,
                _returnState.exteriorAnchorId);
            _registry.PublishStarted(evt);

            if (!Application.CanStreamedLevelBeLoaded(definition.sceneName))
            {
                controller.ShowDialogue(
                    "WORLD W0",
                    "La escena interior no está incluida en el build: " + definition.sceneName);
                _transitioning = false;
                yield break;
            }

            AsyncOperation load = SceneManager.LoadSceneAsync(definition.sceneName, LoadSceneMode.Additive);
            if (load == null)
            {
                controller.ShowDialogue("WORLD W0", "No se pudo iniciar la carga de " + definition.sceneName);
                _transitioning = false;
                yield break;
            }

            while (!load.isDone)
                yield return null;

            // Allow OnEnable anchors from the additive scene to register.
            yield return null;

            HighflyWorldAnchor entry;
            if (!_registry.TryGetAnchor(definition.entryAnchorId, out entry) || entry == null)
                entry = FindAnchorFallback(definition.entryAnchorId);

            if (entry == null)
            {
                controller.ShowDialogue(
                    "WORLD W0",
                    "Interior cargado pero falta entry anchor: " + definition.entryAnchorId);
                AsyncOperation badUnload = SceneManager.UnloadSceneAsync(definition.sceneName);
                if (badUnload != null)
                    while (!badUnload.isDone)
                        yield return null;

                _transitioning = false;
                yield break;
            }

            _activeInteriorId = definition.id;
            _activeInteriorScene = definition.sceneName;

            controller.Teleport(entry.transform.position, entry.transform.forward, false);

            _registry.PublishCompleted(evt);
            _transitioning = false;

            Debug.Log(
                "HIGHFLY W0 ENTER | " + definition.id +
                " scene=" + definition.sceneName +
                " returnAnchor=" + _returnState.exteriorAnchorId);
        }

        public void ExitInterior(HighflyInteractionController controller)
        {
            if (_transitioning || !IsInsideInterior || controller == null)
                return;

            StartCoroutine(ExitInteriorRoutine(controller));
        }

        private IEnumerator ExitInteriorRoutine(HighflyInteractionController controller)
        {
            _transitioning = true;

            string fromInterior = _activeInteriorId;
            string fromScene = _activeInteriorScene;

            HighflyWorldTransitionEvent evt = new HighflyWorldTransitionEvent(
                HighflyWorldTransitionKind.ExitInterior,
                fromInterior,
                _returnState.exteriorSceneName,
                _returnState.exteriorAnchorId);
            _registry.PublishStarted(evt);

            AsyncOperation unload = SceneManager.UnloadSceneAsync(fromScene);
            if (unload != null)
            {
                while (!unload.isDone)
                    yield return null;
            }

            if (_returnState.valid)
            {
                Vector3 facing = _returnState.rotation * Vector3.forward;
                controller.Teleport(_returnState.position, facing, false);
                controller.transform.rotation = _returnState.rotation;
            }

            _activeInteriorId = "";
            _activeInteriorScene = "";

            _registry.PublishCompleted(evt);
            _returnState.Clear();
            _transitioning = false;

            Debug.Log("HIGHFLY W0 EXIT | " + fromInterior + " -> exterior restored");
        }

        private static HighflyWorldAnchor FindAnchorFallback(string anchorId)
        {
            HighflyWorldAnchor[] anchors = FindObjectsOfType<HighflyWorldAnchor>();
            for (int i = 0; i < anchors.Length; i++)
            {
                if (anchors[i] != null &&
                    string.Equals(anchors[i].AnchorId, anchorId, System.StringComparison.OrdinalIgnoreCase))
                    return anchors[i];
            }

            return null;
        }
    }
}
