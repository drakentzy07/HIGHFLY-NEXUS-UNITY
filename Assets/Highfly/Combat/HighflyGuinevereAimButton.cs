using UnityEngine;
using UnityEngine.EventSystems;

namespace Highfly.Combat
{
    /// <summary>
    /// Mobile MOBA drag-to-aim for Prisma (1) and Ruptura (2).
    /// Ordinary tap still invokes the existing Button.onClick binding.
    /// Drag release casts in the camera-relative direction; top-right cancels.
    /// </summary>
    public sealed class HighflyGuinevereAimButton : MonoBehaviour,
        IPointerDownHandler, IBeginDragHandler, IDragHandler, IPointerUpHandler, IEndDragHandler
    {
        [SerializeField] private HighflyGuinevereCombat guinevere;
        [SerializeField] private Camera aimCamera;
        [SerializeField] private int skillSlot = 1;
        [SerializeField] private float aimRange = 8f;
        [SerializeField] private float dragThresholdPixels = 32f;
        private Vector2 _start;
        private bool _dragging;
        private Vector3 _direction;
        private LineRenderer _preview;
        private Material _previewMaterial;

        public void Configure(HighflyGuinevereCombat controller, Camera camera, int slot, float range)
        {
            guinevere = controller;
            aimCamera = camera;
            skillSlot = slot;
            aimRange = range;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _start = eventData.position;
            _dragging = false;
            _direction = Vector3.zero;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _dragging = true;
            eventData.eligibleForClick = false;
            RefreshAim(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_dragging && (eventData.position - _start).sqrMagnitude > dragThresholdPixels * dragThresholdPixels)
                _dragging = true;
            if (_dragging)
            {
                eventData.eligibleForClick = false;
                RefreshAim(eventData.position);
            }
        }

        private void RefreshAim(Vector2 screenPoint)
        {
            if (guinevere == null) return;
            Vector2 delta = screenPoint - _start;
            if (delta.sqrMagnitude < 16f) return;
            Camera cam = aimCamera != null ? aimCamera : Camera.main;
            Vector3 right = cam != null ? cam.transform.right : Vector3.right;
            Vector3 forward = cam != null ? cam.transform.forward : Vector3.forward;
            right.y = forward.y = 0f;
            right.Normalize(); forward.Normalize();
            _direction = (right * delta.x + forward * delta.y).normalized;
            _direction.y = 0f;
            if (_direction.sqrMagnitude < .001f) return;

            if (_preview == null)
            {
                GameObject line = new GameObject("Guinevere_Aim_Preview");
                _preview = line.AddComponent<LineRenderer>();
                _preview.positionCount = 2;
                _preview.startWidth = .13f;
                _preview.endWidth = .025f;
                _preview.startColor = new Color(.96f, .57f, 1f, .85f);
                _preview.endColor = new Color(.62f, .40f, 1f, .45f);
                Shader shader = Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    _previewMaterial = new Material(shader);
                    _preview.sharedMaterial = _previewMaterial;
                }
            }
            _preview.SetPosition(0, guinevere.transform.position + Vector3.up * .12f);
            _preview.SetPosition(1, guinevere.transform.position + Vector3.up * .12f + _direction * aimRange);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            bool dragged = _dragging;
            _dragging = false;
            Cleanup();
            if (!dragged || guinevere == null) return; // Taps use the Button listener.
            eventData.eligibleForClick = false;
            bool cancel = eventData.position.x >= Screen.width * .76f &&
                          eventData.position.y >= Screen.height * .70f;
            if (cancel) return;
            if (skillSlot == 1) guinevere.CastPrisma(_direction);
            if (skillSlot == 2) guinevere.CastRuptura(_direction);
        }

        public void OnEndDrag(PointerEventData eventData) { Cleanup(); }
        private void OnDisable() { Cleanup(); }

        private void Cleanup()
        {
            if (_preview == null) return;
            if (_previewMaterial != null) Destroy(_previewMaterial);
            _previewMaterial = null;
            Destroy(_preview.gameObject);
            _preview = null;
        }
    }
}
