using UnityEngine;
using UnityEngine.EventSystems;

namespace Highfly.SkillsLab
{
    /// <summary>
    /// Touch/drag/release; tap is intentionally handled by Button.onClick.
    /// Drag up-right into the cancellation area to cancel without spending.
    /// No dependency on legacy Input.GetAxis / Input.mousePosition.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HighflyMobaAimButton : MonoBehaviour,
        IPointerDownHandler, IBeginDragHandler, IDragHandler,
        IPointerUpHandler, IEndDragHandler
    {
        [SerializeField] private HighflyMobaSkillsLab lab;
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private int slot = 1;
        [SerializeField] private float previewRange = 8f;
        private Vector2 _start;
        private Vector3 _direction;
        private bool _dragging, _cancelled;
        private LineRenderer _preview;
        private Material _previewMaterial;
        private static HighflyMobaAimButton active;

        public void Configure(HighflyMobaSkillsLab owner, Camera camera,
            int skillSlot, float maxRange)
        {
            lab = owner; gameplayCamera = camera; slot = skillSlot;
            previewRange = maxRange;
        }

        public static void CancelActive()
        {
            if (active == null) return;
            active._cancelled = true;
            active.DestroyPreview();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _start = eventData.position;
            _direction = Vector3.zero;
            _dragging = false;
            _cancelled = false;
            active = this;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_cancelled) return;
            _dragging = true;
            eventData.eligibleForClick = false;
            Refresh(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_cancelled) return;
            if ((eventData.position - _start).sqrMagnitude > 576f)
                _dragging = true;
            if (!_dragging) return;
            eventData.eligibleForClick = false;
            Refresh(eventData.position);
        }

        private void Refresh(Vector2 point)
        {
            if (lab == null) return;
            Camera c = gameplayCamera != null ? gameplayCamera : Camera.main;
            Vector2 d = point - _start;
            if (d.sqrMagnitude < 9f) return;
            Vector3 right = c != null ? c.transform.right : Vector3.right;
            Vector3 forward = c != null ? c.transform.forward : Vector3.forward;
            right.y = 0f; forward.y = 0f;
            right.Normalize(); forward.Normalize();
            _direction = (right * d.x + forward * d.y).normalized;
            if (_direction.sqrMagnitude < .001f) return;

            if (_preview == null)
            {
                GameObject go = new GameObject("MOBA_LAB_AIM_ONLY");
                _preview = go.AddComponent<LineRenderer>();
                Shader shader = Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    _previewMaterial = new Material(shader);
                    _preview.sharedMaterial = _previewMaterial;
                }
                _preview.positionCount = 2;
                _preview.useWorldSpace = true;
                _preview.startWidth = .17f;
                _preview.endWidth = .03f;
                _preview.startColor = new Color(.55f, .89f, 1f, .9f);
                _preview.endColor = new Color(.63f, .45f, 1f, .7f);
            }
            Vector3 origin = lab.transform.position + Vector3.up * .14f;
            _preview.SetPosition(0, origin);
            _preview.SetPosition(1, origin + _direction * previewRange);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            bool dragged = _dragging;
            bool cancelled = _cancelled;
            _dragging = false; _cancelled = false;
            DestroyPreview();
            if (active == this) active = null;
            if (cancelled || !dragged || lab == null)
            {
                if (cancelled) eventData.eligibleForClick = false;
                return;
            }
            eventData.eligibleForClick = false;
            // Explicit safe-zone cancel, independent of event registration.
            if (eventData.position.x > Screen.width * .77f &&
                eventData.position.y > Screen.height * .72f) return;
            lab.Cast(slot, _direction);
        }

        public void OnEndDrag(PointerEventData eventData) { DestroyPreview(); }
        private void OnDisable()
        {
            if (active == this) active = null;
            _dragging = false; _cancelled = false;
            DestroyPreview();
        }

        private void DestroyPreview()
        {
            if (_preview != null) Destroy(_preview.gameObject);
            if (_previewMaterial != null) Destroy(_previewMaterial);
            _preview = null; _previewMaterial = null;
        }
    }

    public sealed class HighflyMobaCancelAim : MonoBehaviour
    {
        public void CancelAim() { HighflyMobaAimButton.CancelActive(); }
    }
}
