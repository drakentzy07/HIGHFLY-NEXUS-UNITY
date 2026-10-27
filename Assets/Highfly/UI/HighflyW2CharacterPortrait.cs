using UnityEngine;
using UnityEngine.UI;
using Highfly.World;

namespace Highfly.UI
{
    /// <summary>
    /// A live Quaternius 3D portrait rendered to a small offscreen texture.
    /// Isolated render layer: no extra cameras affecting the world camera.
    /// The portrait is ONLY rendered while the full-screen RPG menu is open.
    /// </summary>
    public sealed class HighflyW2CharacterPortrait : MonoBehaviour
    {
        [SerializeField] private HighflyHunterAppearance appearance;
        [SerializeField] private HighflyW2Progress progress;
        [SerializeField] private GameObject menuRoot;
        [SerializeField] private RawImage display;
        private Camera _portraitCamera;
        private RenderTexture _texture;
        private GameObject _standIn;
        private const int PortraitLayer = 29;
        private readonly Vector3 _stage = new Vector3(0f, -1000f, 0f);

        public void Configure(HighflyHunterAppearance look, HighflyW2Progress data,
            GameObject parentPanel, RawImage output)
        {
            appearance = look;
            progress = data;
            menuRoot = parentPanel;
            display = output;
        }

        private void Start()
        {
            if (appearance == null || display == null) return;
            _texture = new RenderTexture(480, 640, 16, RenderTextureFormat.ARGB32);
            _texture.name = "W2C_Hunter_3D_Preview";
            _texture.antiAliasing = 1;
            _texture.Create();
            display.texture = _texture;

            GameObject cameraGo = new GameObject("W2C_Only_Portrait_Camera");
            _portraitCamera = cameraGo.AddComponent<Camera>();
            _portraitCamera.enabled = false;
            _portraitCamera.clearFlags = CameraClearFlags.SolidColor;
            _portraitCamera.backgroundColor = new Color(.07f, .10f, .17f, 1f);
            _portraitCamera.cullingMask = 1 << PortraitLayer;
            _portraitCamera.targetTexture = _texture;
            _portraitCamera.fieldOfView = 30f;
            _portraitCamera.nearClipPlane = .1f;
            _portraitCamera.farClipPlane = 18f;
            _portraitCamera.transform.position = _stage + new Vector3(0f, 1.3f, 4.7f);
            _portraitCamera.transform.LookAt(_stage + Vector3.up * 1f);

            GameObject lightGo = new GameObject("W2C_Only_Portrait_Light");
            lightGo.transform.SetParent(cameraGo.transform, false);
            Light studio = lightGo.AddComponent<Light>();
            studio.type = LightType.Directional;
            studio.intensity = 1.9f;
            studio.cullingMask = 1 << PortraitLayer;
            studio.transform.rotation = Quaternion.Euler(34f, -35f, 0f);

            appearance.AppearanceChanged += OnLook;
            if (progress != null) progress.Changed += OnProgress;
            BuildPreview();
        }

        private void OnLook(int _) { BuildPreview(); }
        private void OnProgress() { BuildPreview(); }

        private void BuildPreview()
        {
            if (appearance == null || !appearance.IsDressed(appearance.CurrentIndex))
                return;
            Transform source = appearance.CurrentVisualRoot;
            if (source == null) return;
            if (_standIn != null) Destroy(_standIn);
            _standIn = Instantiate(source.gameObject);
            _standIn.name = "W2C_3D_Portrait_Clothed_Hunter";
            _standIn.transform.position = _stage;
            _standIn.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            _standIn.SetActive(true);
            foreach (Transform node in _standIn.GetComponentsInChildren<Transform>(true))
                node.gameObject.layer = PortraitLayer;
            foreach (Collider hitbox in _standIn.GetComponentsInChildren<Collider>(true))
                Destroy(hitbox);
            foreach (HighflyAnimationBalance calibrator in
                _standIn.GetComponentsInChildren<HighflyAnimationBalance>(true))
                Destroy(calibrator);
            Animator animator = _standIn.GetComponentInChildren<Animator>(true);
            if (animator != null)
            {
                animator.applyRootMotion = false;
                animator.Rebind();
            }
        }

        private void LateUpdate()
        {
            bool visible = menuRoot != null && menuRoot.activeInHierarchy;
            if (_portraitCamera != null)
                _portraitCamera.enabled = visible && isActiveAndEnabled;
        }

        private void OnDestroy()
        {
            if (appearance != null) appearance.AppearanceChanged -= OnLook;
            if (progress != null) progress.Changed -= OnProgress;
            if (_portraitCamera != null) Destroy(_portraitCamera.gameObject);
            if (_standIn != null) Destroy(_standIn);
            if (_texture != null)
            {
                _texture.Release();
                Destroy(_texture);
            }
        }
    }
}
